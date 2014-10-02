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
    public class FileProcessor : IDisposable
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

        /// <summary>
        /// Default value for the <see cref="UseTimer"/> property.
        /// </summary>
        public const bool DefaultUseTimer = false;

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

        private readonly List<FileSystemWatcher> m_fileWatchers;
        private ProcessQueue<Action> m_processingQueue;
        private Timer m_fileWatchTimer;
        private Mutex m_waitObject;

        private readonly Dictionary<string, DateTime> m_touchedFiles;
        private readonly HashSet<string> m_processedFiles;

        private bool m_disposed;
        private bool m_useTimer;

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
            m_useTimer = DefaultUseTimer;

            m_fileWatchers = new List<FileSystemWatcher>();
            m_processingQueue = ProcessQueue<Action>.CreateRealTimeQueue(action => action());
            m_processingQueue.SynchronizedOperationType = SynchronizedOperationType.LongBackground;
            m_processingQueue.ProcessException += (sender, args) => OnError(args.Argument);
            m_fileWatchTimer = new Timer(15000);
            m_fileWatchTimer.Elapsed += FileWatchTimer_Elapsed;
            m_waitObject = new Mutex(true);

            m_touchedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            m_processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FileProcessor"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FileProcessor()
        {
            Dispose(false);
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
                if (m_fileWatchers.Count > 0)
                    throw new InvalidOperationException("File processor is already tracking directories - modification of the cache path would be unsafe.");

                if ((object)m_cachePath != null)
                    m_cachePath = FilePath.GetAbsolutePath(value);
                else
                    m_cachePath = DefaultCachePath;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether the file processor should use a
        /// timer in addition to the file watchers to track changes to the directories.
        /// </summary>
        public bool UseTimer
        {
            get
            {
                return m_useTimer;
            }
            set
            {
                m_useTimer = value;

                if (value && m_fileWatchers.Count > 0)
                    m_fileWatchTimer.Start();
                else
                    m_fileWatchTimer.Stop();
            }
        }

        /// <summary>
        /// Gets the list of directories currently being tracked by the <see cref="FileProcessor"/>.
        /// </summary>
        public IList<string> TrackedDirectories
        {
            get
            {
                return m_fileWatchers
                    .Select(watcher => watcher.Path)
                    .ToList();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FileProcessor"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileProcessor"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
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

                        if ((object)m_waitObject != null)
                        {
                            m_waitObject.ReleaseMutex();
                            m_waitObject.Dispose();
                            m_waitObject = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Adds a directory to the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to be tracked.</param>
        public void AddTrackedDirectory(string path)
        {
            string fullPath = FilePath.GetAbsolutePath(path);
            FileSystemWatcher watcher;
            HashSet<string> listedFiles;

            if (!TrackedDirectories.Contains(fullPath))
            {
                watcher = new FileSystemWatcher(fullPath);
                watcher.IncludeSubdirectories = true;

                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.Deleted += Watcher_Deleted;
                watcher.Error += Watcher_Error;

                if (m_fileWatchers.Count == 0)
                {
                    m_processingQueue.Start();

                    if (m_useTimer)
                        m_fileWatchTimer.Start();
                }

                m_fileWatchers.Add(watcher);
                watcher.EnableRaisingEvents = true;

                listedFiles = new HashSet<string>(Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories));

                m_processingQueue.Add(() =>
                {
                    foreach (string filePath in listedFiles)
                        QueueFileForProcessing(filePath);

                    if (m_processedFiles.Count == 0)
                        LoadProcessedFiles();

                    if (m_processedFiles.RemoveWhere(filePath => !listedFiles.Contains(filePath, StringComparer.OrdinalIgnoreCase) && filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase)) > 0)
                        SaveProcessedFiles();
                });
            }
        }

        /// <summary>
        /// Removes a directory from the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to stop tracking.</param>
        public void RemoveTrackedDirectory(string path)
        {
            string fullPath = FilePath.GetAbsolutePath(path);

            List<FileSystemWatcher> fileWatchersToRemove = m_fileWatchers
                .Where(w => fullPath.Equals(w.Path, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (FileSystemWatcher watcher in fileWatchersToRemove)
                RemoveFileWatcher(watcher);

            if (m_fileWatchers.Count == 0)
            {
                m_processingQueue.Stop();
                m_fileWatchTimer.Stop();
            }
        }

        /// <summary>
        /// Empties the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        public void ClearTrackedDirectories()
        {
            while (m_fileWatchers.Count > 0)
                RemoveFileWatcher(m_fileWatchers[0]);
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

            m_processingQueue.Add(() =>
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                DateTime lastKnownWriteTime;

                if (!m_touchedFiles.TryGetValue(filePath, out lastKnownWriteTime) || lastKnownWriteTime < lastWriteTime)
                {
                    m_touchedFiles[filePath] = lastWriteTime;
                    LockAndQueue(filePath);
                }
            });
        }

        // Attempts to obtain a read lock on the file and,
        // if successful, queues the file.
        // Otherwise, waits 250 milliseconds and tries again.
        private void LockAndQueue(string filePath)
        {
            if (m_disposed || !File.Exists(filePath))
                return;

            if (FilePath.TryGetReadLockExclusive(filePath))
                m_processingQueue.Add(() => ProcessFile(filePath));
            else
                DelayLockAndQueue(filePath);
        }

        // Waits 250 milliseconds, then calls LockAndQueue.
        private void DelayLockAndQueue(string filePath)
        {
            WaitOrTimerCallback callback = (state, timeout) =>
            {
                if (timeout)
                    LockAndQueue(filePath);
            };

            ThreadPool.RegisterWaitForSingleObject(m_waitObject, callback, null, 250, true);
        }

        // Processes the given file.
        private void ProcessFile(string filePath)
        {
            bool alreadyProcessed;

            // If the file processor is disposed or the
            // file no longer exists, return immediately
            if (m_disposed || !File.Exists(filePath))
                return;

            // Load processed files from the cache
            // before the first file is processed
            if (m_processedFiles.Count == 0)
                LoadProcessedFiles();

            // Process the file at the given file path
            alreadyProcessed = m_processedFiles.Contains(filePath);

            if (OnProcessing(filePath, alreadyProcessed))
            {
                // If the handler requeuests a requeue,
                // requeue the file after a 250 millisecond delay.
                DelayLockAndQueue(filePath);
            }
            else if (!alreadyProcessed)
            {
                // Update the list of processed files
                // and save it back to the cache
                m_processedFiles.Add(filePath);
                AppendProcessedFile(filePath);
            }
        }

        // Loads the list of processed files from the cache.
        private void LoadProcessedFiles()
        {
            try
            {
                string cachePath = Path.Combine(m_cachePath, m_processorID.ToString());

                if (File.Exists(cachePath))
                {
                    // Set up the stream reader to read the list of processed files
                    using (FileStream stream = File.OpenRead(cachePath))
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        // Clear the existing list of processed files
                        m_processedFiles.Clear();

                        while (!reader.EndOfStream)
                        {
                            // Each path is on its own line
                            string fullPath = reader.ReadLine();

                            // Add the path to the list of processed files
                            if (!string.IsNullOrEmpty(fullPath))
                                m_processedFiles.Add(fullPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to load processed files cache due to exception: {0}", ex.Message);
                OnError(new InvalidOperationException(message, ex));
            }
        }

        // Saves the list of processed files to the cache.
        private void SaveProcessedFiles()
        {
            try
            {
                // Ensure that the directory to store the cache exists
                if (!Directory.Exists(m_cachePath))
                    Directory.CreateDirectory(m_cachePath);

                // Set up the stream writer to write the list of processed files
                using (FileStream stream = File.Open(Path.Combine(m_cachePath, m_processorID.ToString()), FileMode.Create))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                {
                    // Write each processed file path to the writer
                    foreach (string filePath in m_processedFiles)
                        writer.WriteLine(filePath);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to save processed files cache due to exception: {0}", ex.Message);
                OnError(new InvalidOperationException(message, ex));
            }
        }

        // Appends the given file path to the end of the list of processed files.
        private void AppendProcessedFile(string filePath)
        {
            try
            {
                // Ensure that the directory to store the cache exists
                if (!Directory.Exists(m_cachePath))
                    Directory.CreateDirectory(m_cachePath);

                // Set up the stream writer to write the list of processed files
                using (FileStream stream = File.Open(Path.Combine(m_cachePath, m_processorID.ToString()), FileMode.Append))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                {
                    // Write processed file path to the writer
                    writer.WriteLine(filePath);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Unable to save processed files cache due to exception: {0}", ex.Message);
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
            if (m_trackChanges)
            {
                m_processingQueue.Add(() => m_touchedFiles.Remove(args.FullPath));
                QueueFileForProcessing(args.FullPath);
            }
        }

        // Track renames so that files whose names are changed can be updated in the processed files list.
        private void Watcher_Renamed(object sender, RenamedEventArgs args)
        {
            m_processingQueue.Add(() =>
            {
                if (m_touchedFiles.Remove(args.OldFullPath))
                    m_touchedFiles.Add(args.FullPath, File.GetLastWriteTimeUtc(args.FullPath));

                if (m_processedFiles.Remove(args.OldFullPath))
                {
                    m_processedFiles.Add(args.FullPath);
                    SaveProcessedFiles();
                }
            });
        }

        // Track deletes so that files can be removed from the processed files list.
        private void Watcher_Deleted(object sender, FileSystemEventArgs args)
        {
            m_processingQueue.Add(() =>
            {
                m_touchedFiles.Remove(args.FullPath);

                if (m_processedFiles.Remove(args.FullPath))
                    SaveProcessedFiles();
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
            IList<string> trackedDirectories = null;
            HashSet<string> listedFiles;

            // Loop until we get the list of tracked directories
            while ((object)trackedDirectories == null)
            {
                try
                {
                    trackedDirectories = TrackedDirectories;
                }
                catch (InvalidOperationException)
                {
                    // Collection was modified on another thread,
                    // so we'll loop around and try again
                }
            }

            // Gets the list of all files in all the directories tracked by this file processor
            listedFiles = new HashSet<string>(trackedDirectories.SelectMany(directory => Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)));

            // Now queue an operation to remove files
            // from lists that are no longer tracked
            m_processingQueue.Add(() =>
            {
                // Queue these files for processing
                foreach (string file in listedFiles)
                    QueueFileForProcessing(file);

                foreach (string file in m_touchedFiles.Keys.Where(file => !listedFiles.Contains(file)).ToList())
                    m_touchedFiles.Remove(file);

                if (m_processedFiles.RemoveWhere(file => !listedFiles.Contains(file)) > 0)
                    SaveProcessedFiles();
            });
        }

        #endregion
    }
}
