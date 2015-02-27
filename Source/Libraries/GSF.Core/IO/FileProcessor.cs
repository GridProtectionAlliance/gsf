//******************************************************************************************************
//  FileTracker.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/29/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Threading;
using Timer = System.Timers.Timer;

namespace GSF.IO
{
    /// <summary>
    /// Arguments to events triggered by the <see cref="FileProcessor"/>.
    /// </summary>
    public class FileProcessorEventArgs : EventArgs
    {
        #region [ Members ]

        // Fields
        private readonly string m_fullPath;
        private readonly bool m_alreadyProcessed;
        private bool m_requeue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileProcessorEventArgs"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to the file to be processed.</param>
        /// <param name="alreadyProcessed">Flag indicating whether this file has been processed before.</param>
        public FileProcessorEventArgs(string fullPath, bool alreadyProcessed)
        {
            m_fullPath = fullPath;
            m_alreadyProcessed = alreadyProcessed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the full path to the file to be processed.
        /// </summary>
        public string FullPath
        {
            get
            {
                return m_fullPath;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether this file has been processed before.
        /// </summary>
        public bool AlreadyProcessed
        {
            get
            {
                return m_alreadyProcessed;
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether the
        /// file should be requeued by the file processor.
        /// </summary>
        public bool Requeue
        {
            get
            {
                return m_requeue;
            }
            set
            {
                m_requeue = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Tracks files processed in a list of directories, and
    /// notifies when new files become available to be processed.
    /// </summary>
    public sealed class FileProcessor : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="Filter"/> property.
        /// </summary>
        public const string DefaultFilter = "*";

        /// <summary>
        /// Default value for the <see cref="TrackChanges"/> property.
        /// </summary>
        public const bool DefaultTrackChanges = false;

        /// <summary>
        /// Default value for the <see cref="CachePath"/> property.
        /// </summary>
        public static readonly string DefaultCachePath = Path.Combine(FilePath.GetCommonApplicationDataFolder(), "FileProcessors");

        // Events

        /// <summary>
        /// Event triggered when a file is to be processed.
        /// </summary>
        public event EventHandler<FileProcessorEventArgs> Processing;

        /// <summary>
        /// Event triggered when an unexpected error occurs during
        /// normal operation of the <see cref="FileProcessor"/>.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        // Fields
        private readonly Guid m_processorID;

        private string m_filter;
        private bool m_trackChanges;
        private string m_cachePath;

        private readonly object m_fileWatchersLock;
        private readonly List<FileSystemWatcher> m_fileWatchers;
        private ProcessQueue<Action> m_processingQueue;
        private Timer m_fileWatchTimer;
        private ManualResetEvent m_waitObject;

        private readonly Dictionary<string, DateTime> m_touchedFiles;
        private readonly FileBackedHashSet<string> m_processedFiles;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileProcessor"/> class.
        /// </summary>
        /// <param name="processorID">Identifies the file processor so that it can locate its processed file cache.</param>
        public FileProcessor(Guid processorID)
        {
            m_processorID = processorID;

            m_filter = DefaultFilter;
            m_trackChanges = DefaultTrackChanges;
            m_cachePath = DefaultCachePath;

            m_fileWatchersLock = new object();
            m_fileWatchers = new List<FileSystemWatcher>();
            m_processingQueue = ProcessQueue<Action>.CreateRealTimeQueue(action => action());
            m_processingQueue.SynchronizedOperationType = SynchronizedOperationType.LongBackground;
            m_processingQueue.ProcessException += (sender, args) => OnError(args.Argument);
            m_fileWatchTimer = new Timer(15000);
            m_fileWatchTimer.Elapsed += FileWatchTimer_Elapsed;
            m_waitObject = new ManualResetEvent(false);

            m_touchedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            m_processedFiles = new FileBackedHashSet<string>(Path.Combine(m_cachePath, m_processorID.ToString()), StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the filter used to determine whether a file should be processed.
        /// </summary>
        public string Filter
        {
            get
            {
                return m_filter;
            }
            set
            {
                m_filter = value ?? DefaultFilter;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether files should be processed on change.
        /// </summary>
        public bool TrackChanges
        {
            get
            {
                return m_trackChanges;
            }
            set
            {
                m_trackChanges = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the cache where the list of processed files will be persisted.
        /// </summary>
        public string CachePath
        {
            get
            {
                return m_cachePath;
            }
            set
            {
                lock (m_fileWatchers)
                {
                    if (m_fileWatchers.Count > 0)
                        throw new InvalidOperationException("File processor is already tracking directories - modification of the cache path would be unsafe.");

                    if (m_cachePath != value)
                    {
                        if ((object)m_cachePath != null)
                            m_cachePath = FilePath.GetAbsolutePath(value);
                        else
                            m_cachePath = DefaultCachePath;

                        m_processedFiles.FilePath = Path.Combine(m_cachePath, m_processorID.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of directories currently being tracked by the <see cref="FileProcessor"/>.
        /// </summary>
        public IList<string> TrackedDirectories
        {
            get
            {
                lock (m_fileWatchers)
                {
                    return m_fileWatchers
                        .Select(watcher => watcher.Path)
                        .ToList();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a directory to the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to be tracked.</param>
        public void AddTrackedDirectory(string path)
        {
            string fullPath = FilePath.GetAbsolutePath(path);
            FileSystemWatcher watcher;

            DateTime enumerationStart;
            HashSet<string> enumeratedFiles;
            IEnumerator<string> enumerator;

            if (!TrackedDirectories.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                watcher = new FileSystemWatcher(fullPath);
                watcher.IncludeSubdirectories = true;

                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.Deleted += Watcher_Deleted;
                watcher.Error += Watcher_Error;

                lock (m_fileWatchersLock)
                {
                    if (m_fileWatchers.Count == 0)
                    {
                        m_processingQueue.Start();
                        m_processingQueue.Add(LoadProcessedFiles);
                        m_fileWatchTimer.Start();
                    }

                    m_fileWatchers.Add(watcher);
                }

                watcher.EnableRaisingEvents = true;

                enumerationStart = DateTime.UtcNow;
                enumeratedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                enumerator = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories).GetEnumerator();
                m_processingQueue.Add(() => EnumerateNextFile(fullPath, enumerationStart, enumeratedFiles, enumerator));
            }
        }

        /// <summary>
        /// Removes a directory from the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to stop tracking.</param>
        public void RemoveTrackedDirectory(string path)
        {
            string fullPath = FilePath.GetAbsolutePath(path);

            List<FileSystemWatcher> fileWatchersToRemove;

            lock (m_fileWatchers)
            {
                fileWatchersToRemove = m_fileWatchers
                    .Where(w => fullPath.Equals(w.Path, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (FileSystemWatcher watcher in fileWatchersToRemove)
                    RemoveFileWatcher(watcher);

                if (m_fileWatchers.Count == 0)
                {
                    m_processingQueue.Stop();
                    m_fileWatchTimer.Stop();
                    m_processedFiles.Close();
                }
            }
        }

        /// <summary>
        /// Empties the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        public void ClearTrackedDirectories()
        {
            lock (m_fileWatchers)
            {
                while (m_fileWatchers.Count > 0)
                    RemoveFileWatcher(m_fileWatchers[0]);
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FileProcessor"/> object.
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    ClearTrackedDirectories();

                    if ((object)m_processingQueue != null)
                    {
                        m_processingQueue.Dispose();
                        m_processingQueue = null;
                    }

                    if ((object)m_fileWatchTimer != null)
                    {
                        m_fileWatchTimer.Dispose();
                        m_fileWatchTimer = null;
                    }

                    if ((object)m_processingQueue != null)
                    {
                        m_processingQueue.Dispose();
                        m_processingQueue = null;
                    }

                    if ((object)m_waitObject != null)
                    {
                        m_waitObject.Set();
                        m_waitObject.Dispose();
                        m_waitObject = null;
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }

        // Determines if the given file matches the file processor's filter.
        private bool MatchesFilter(string filePath)
        {
            string fileName = FilePath.GetFileName(filePath);
            string[] filters = m_filter.Split(Path.PathSeparator);
            return FilePath.IsFilePatternMatch(filters, fileName, true);
        }

        // Queues the file for processing.
        private void QueueFileForProcessing(string filePath)
        {
            if (!MatchesFilter(filePath))
                return;

            m_processingQueue.Add(() => TouchLockAndProcess(filePath));
        }

        // Attempts to obtain a read lock on the file and,
        // if successful, processes the file.
        // Otherwise, waits 250 milliseconds and tries again.
        private void LockAndProcess(string filePath)
        {
            if (m_disposed || !File.Exists(filePath))
                return;

            if (FilePath.TryGetReadLockExclusive(filePath))
                ProcessFile(filePath);
            else
                DelayLockAndProcess(filePath);
        }

        // Waits 250 milliseconds, then calls LockAndProcess.
        private void DelayLockAndProcess(string filePath)
        {
            WaitOrTimerCallback callback = (state, timeout) =>
            {
                if (timeout)
                    m_processingQueue.Add(() => LockAndProcess(filePath));
            };

            ThreadPool.RegisterWaitForSingleObject(m_waitObject, callback, null, 250, true);
        }

        // Checks and updates the touchedFiles lookup table, then calls LockAndProcess.
        private void TouchLockAndProcess(string filePath)
        {
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            DateTime lastKnownWriteTime;

            if (!m_touchedFiles.TryGetValue(filePath, out lastKnownWriteTime) || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;
                LockAndProcess(filePath);
            }
        }

        // Processes the given file.
        private void ProcessFile(string filePath)
        {
            bool alreadyProcessed;

            // If the file processor is disposed or the
            // file no longer exists, return immediately
            if (m_disposed || !File.Exists(filePath))
                return;

            // Process the file at the given file path
            alreadyProcessed = m_processedFiles.Contains(filePath);

            if (OnProcessing(filePath, alreadyProcessed))
            {
                // If the handler requeuests a requeue,
                // requeue the file after a 250 millisecond delay.
                DelayLockAndProcess(filePath);
            }
            else if (!alreadyProcessed)
            {
                // Update the list of processed files
                // and save it back to the cache
                m_processedFiles.Add(filePath);
            }
        }

        // Advances the enumerator to the next file to be processed and attempts to process it.
        private void EnumerateNextFile(string watchDirectory, DateTime enumerationStart, HashSet<string> enumeratedFiles, IEnumerator<string> enumerator)
        {
            // If the file processor has been
            // disposed, return immediately
            if (m_disposed)
                return;

            if (enumerator.MoveNext())
            {
                // Add the file to the collection of enumerated files
                enumeratedFiles.Add(enumerator.Current);

                // Attempt to process this file
                if (File.Exists(enumerator.Current) && MatchesFilter(enumerator.Current))
                    TouchLockAndProcess(enumerator.Current);

                // Move to the next file
                m_processingQueue.Add(() => EnumerateNextFile(watchDirectory, enumerationStart, enumeratedFiles, enumerator));
            }
            else
            {
                // No more files to process, so we use the enumeratedFiles hash set
                // to clean up files that have been removed from the watch directory
                Predicate<string> predicate = filePath =>
                {
                    DateTime lastWriteTime;
                    bool enumerated = enumeratedFiles.Contains(filePath);
                    bool touched = m_touchedFiles.TryGetValue(filePath, out lastWriteTime) && (lastWriteTime > enumerationStart);
                    bool isInWatchPath = filePath.StartsWith(watchDirectory, StringComparison.OrdinalIgnoreCase);
                    return !enumerated && !touched && isInWatchPath;
                };

                m_processedFiles.RemoveWhere(predicate);
            }
        }

        // Loads the list of processed files from the cache.
        private void LoadProcessedFiles()
        {
            try
            {
                string cachePath = Path.Combine(m_cachePath, m_processorID.ToString());
                List<string> processedFilesList;
                byte[] signature;

                if (File.Exists(cachePath))
                {
                    processedFilesList = new List<string>();
                    signature = m_processedFiles.DefaultSignature;

                    using (FileStream stream = File.OpenRead(cachePath))
                    {
                        // Read the signature from the start of the file
                        if (stream.Read(signature, 0, signature.Length) <= 0)
                            return;

                        // Compare the signature to the default signature of the processedFiles hash set
                        if (signature.SequenceEqual(m_processedFiles.DefaultSignature))
                            return;

                        // Signature of the file does not match that of the m_processedFiles hash set so
                        // assume this is a text file with a list of the files that have been processed
                        stream.Position = 0;

                        // Read each line of the file into a list to
                        // be consumed by the m_processedFiles hash set
                        using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                        {
                            while (!reader.EndOfStream)
                            {
                                string fullPath = reader.ReadLine();

                                if (!string.IsNullOrEmpty(fullPath))
                                    processedFilesList.Add(fullPath);
                            }
                        }
                    }

                    // Delete the existing file since it
                    // does not have the right signature
                    File.Delete(cachePath);

                    // Copy files that we read from the text file
                    // into the m_processedFiles hash set
                    foreach (string fullPath in processedFilesList)
                        m_processedFiles.Add(fullPath);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to load processed files cache due to exception: {0}", ex.Message);
                OnError(new InvalidOperationException(message, ex));
            }
        }

        // Detaches from events, removes the given file watcher from
        // the list of watchers, and disposes of the file watcher.
        private void RemoveFileWatcher(FileSystemWatcher watcher)
        {
            watcher.Created -= Watcher_Created;
            watcher.Changed -= Watcher_Changed;
            watcher.Renamed -= Watcher_Renamed;
            watcher.Deleted -= Watcher_Deleted;
            watcher.Error -= Watcher_Error;

            m_fileWatchers.Remove(watcher);
            watcher.Dispose();
        }

        // Triggers the processing event for the given file.
        private bool OnProcessing(string fullPath, bool alreadyProcessed)
        {
            FileProcessorEventArgs args;

            if ((object)Processing != null)
            {
                args = new FileProcessorEventArgs(fullPath, alreadyProcessed);
                Processing(this, args);
                return args.Requeue;
            }

            return false;
        }

        // Triggers the error event with the given exception.
        private void OnError(Exception ex)
        {
            if ((object)Error != null)
                Error(this, new ErrorEventArgs(ex));
        }

        // Queues the created file for processing.
        private void Watcher_Created(object sender, FileSystemEventArgs args)
        {
            QueueFileForProcessing(args.FullPath);
        }

        // If the watcher tracks changes, queues the changed file for processing.
        private void Watcher_Changed(object sender, FileSystemEventArgs args)
        {
            if (m_trackChanges && MatchesFilter(args.FullPath))
            {
                m_processingQueue.Add(() =>
                {
                    m_touchedFiles.Remove(args.FullPath);
                    TouchLockAndProcess(args.FullPath);
                });
            }
        }

        // Track renames so that files whose names are changed can be updated in the processed files list.
        private void Watcher_Renamed(object sender, RenamedEventArgs args)
        {
            bool oldMatch = MatchesFilter(args.OldFullPath);
            bool newMatch = MatchesFilter(args.FullPath);

            if (!oldMatch && !newMatch)
                return;

            m_processingQueue.Add(() =>
            {
                if (oldMatch && m_touchedFiles.Remove(args.OldFullPath) && newMatch)
                    m_touchedFiles.Add(args.FullPath, File.GetLastWriteTimeUtc(args.FullPath));

                if (oldMatch && m_processedFiles.Remove(args.OldFullPath))
                    m_processedFiles.Add(args.FullPath);

                if (!oldMatch && newMatch)
                    TouchLockAndProcess(args.FullPath);
            });
        }

        // Track deletes so that files can be removed from the processed files list.
        private void Watcher_Deleted(object sender, FileSystemEventArgs args)
        {
            if (!MatchesFilter(args.FullPath))
                return;

            m_processingQueue.Add(() =>
            {
                m_touchedFiles.Remove(args.FullPath);
                m_processedFiles.Remove(args.FullPath);
            });
        }

        // Triggers the error event for the exception encountered by the file watcher.
        private void Watcher_Error(object sender, ErrorEventArgs args)
        {
            if ((object)Error != null)
                Error(sender, args);
        }

        // Picks up files that were missed by the file watchers.
        private void FileWatchTimer_Elapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Check each of the existing file watchers to determine whether an
            // error occurred that forced the file watcher to stop raising events
            lock (m_fileWatchers)
            {
                for (int i = 0; i < m_fileWatchers.Count; i++)
                {
                    if (!m_fileWatchers[i].EnableRaisingEvents)
                    {
                        try
                        {
                            // This file watcher is no longer raising events so
                            // attempt to create a new file watcher for that file path
                            FileSystemWatcher newWatcher = new FileSystemWatcher(m_fileWatchers[i].Path);

                            DateTime enumerationStart;
                            HashSet<string> enumeratedFiles;
                            IEnumerator<string> enumerator;

                            newWatcher.IncludeSubdirectories = true;
                            newWatcher.Created += Watcher_Created;
                            newWatcher.Changed += Watcher_Changed;
                            newWatcher.Renamed += Watcher_Renamed;
                            newWatcher.Deleted += Watcher_Deleted;
                            newWatcher.Error += Watcher_Error;

                            m_fileWatchers[i].Created -= Watcher_Created;
                            m_fileWatchers[i].Changed -= Watcher_Changed;
                            m_fileWatchers[i].Renamed -= Watcher_Renamed;
                            m_fileWatchers[i].Deleted -= Watcher_Deleted;
                            m_fileWatchers[i].Error -= Watcher_Error;
                            m_fileWatchers[i].Dispose();

                            m_fileWatchers[i] = newWatcher;
                            newWatcher.EnableRaisingEvents = true;

                            // Files may have been dropped or removed while the file watcher
                            // was disconnected so we need to enumerate the files again
                            enumerationStart = DateTime.UtcNow;
                            enumeratedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            enumerator = Directory.EnumerateFiles(newWatcher.Path, "*.*", SearchOption.AllDirectories).GetEnumerator();
                            m_processingQueue.Add(() => EnumerateNextFile(newWatcher.Path, enumerationStart, enumeratedFiles, enumerator));

                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
