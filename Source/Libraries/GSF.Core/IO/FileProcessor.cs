//******************************************************************************************************
//  FileTracker.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Threading;
using CancellationToken = GSF.Threading.CancellationToken;
using Timer = System.Timers.Timer;

namespace GSF.IO
{
    /// <summary>
    /// Defines strategies for enumerating
    /// files in the watch directories.
    /// </summary>
    public enum FileEnumerationStrategy
    {
        /// <summary>
        /// Enumerate all files sequentially.
        /// </summary>
        Sequential,

        /// <summary>
        /// Enumerates the watch directories in parallel,
        /// but subdirectories are processed sequentially.
        /// </summary>
        ParallelWatchDirectories,

        /// <summary>
        /// Enumerates every directory, including subdirectories, in parallel.
        /// </summary>
        ParallelSubdirectories,

        /// <summary>
        /// Does not enumerate directories, relying only on
        /// the file watcher to handle file processing events.
        /// </summary>
        None
    }

    /// <summary>
    /// Arguments to events triggered by the <see cref="FileProcessor"/>.
    /// </summary>
    public class FileProcessorEventArgs : EventArgs
    {
        #region [ Members ]

        // Fields
        private readonly string m_fullPath;
        private readonly bool m_alreadyProcessed;
        private readonly bool m_raisedByFileWatcher;
        private readonly Func<int> m_retryCounter;
        private bool m_requeue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileProcessorEventArgs"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to the file to be processed.</param>
        /// <param name="alreadyProcessed">Flag indicating whether this file has been processed before.</param>
        /// <param name="raisedByFileWatcher">Flag indicating whether this event was raised by the file watcher.</param>
        /// <param name="retryCounter">The function that provides the value for <see cref="RetryCount"/>.</param>
        public FileProcessorEventArgs(string fullPath, bool alreadyProcessed, bool raisedByFileWatcher, Func<int> retryCounter)
        {
            m_fullPath = fullPath;
            m_alreadyProcessed = alreadyProcessed;
            m_raisedByFileWatcher = raisedByFileWatcher;
            m_retryCounter = retryCounter;
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
        /// Gets the flag that indicates whether this event was raised by the file watcher.
        /// </summary>
        public bool RaisedByFileWatcher
        {
            get
            {
                return m_raisedByFileWatcher;
            }
        }

        /// <summary>
        /// Gets the number of file processing attempts.
        /// </summary>
        public int RetryCount
        {
            get
            {
                return m_retryCounter();
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

        // Nested Types

        private sealed class TrackedDirectory : IDisposable
        {
            #region [ Members ]

            // Fields
            private FileProcessor m_fileProcessor;
            private SafeFileWatcher m_fileWatcher;
            private FileEnumerator m_fileEnumerator;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            public TrackedDirectory(FileProcessor fileProcessor, string path)
            {
                m_fileProcessor = fileProcessor;
                Path = path;
                CreateFileWatcher();
            }

            #endregion

            #region [ Properties ]

            public string Path { get; }

            public bool IsEnumerating
            {
                get
                {
                    return m_fileEnumerator?.Active ?? false;
                }
            }

            public List<string> ActivelyEnumeratedPaths
            {
                get
                {
                    return m_fileEnumerator.ActivelyVisitedPaths;
                }
            }

            #endregion

            #region [ Methods ]

            public void EnumerateFiles(FileEnumerationStrategy fileEnumerationStrategy)
            {
                if (m_disposed)
                    return;

                if (IsEnumerating)
                    throw new InvalidOperationException("Enumeration is already in progress.");

                CancellationToken cancellationToken = new CancellationToken();
                m_fileEnumerator = new FileEnumerator(Path, m_fileProcessor, cancellationToken);
                m_fileEnumerator.Enumerate(fileEnumerationStrategy);
            }

            public void CancelEnumeration()
            {
                m_fileEnumerator?.Cancel();
            }

            public void CheckFileWatcher()
            {
                if (m_disposed)
                    return;

                if (!m_fileWatcher.EnableRaisingEvents)
                {
                    DisposeFileWatcher();
                    CreateFileWatcher();
                }
            }

            /// <summary>
            /// Releases all the resources used by the <see cref="TrackedDirectory"/> object.
            /// </summary>
            public void Dispose()
            {
                if (m_disposed)
                    return;

                try
                {
                    CancelEnumeration();
                    DisposeFileWatcher();
                }
                finally
                {
                    m_disposed = true;
                }
            }

            private void CreateFileWatcher()
            {
                m_fileWatcher = new SafeFileWatcher(Path);
                m_fileWatcher.InternalBufferSize = m_fileProcessor.InternalBufferSize;
                m_fileWatcher.IncludeSubdirectories = true;

                m_fileWatcher.Created += m_fileProcessor.Watcher_Created;
                m_fileWatcher.Changed += m_fileProcessor.Watcher_Changed;
                m_fileWatcher.Renamed += m_fileProcessor.Watcher_Renamed;
                m_fileWatcher.Deleted += m_fileProcessor.Watcher_Deleted;
                m_fileWatcher.Error += m_fileProcessor.Watcher_Error;

                m_fileWatcher.EnableRaisingEvents = true;
            }

            private void DisposeFileWatcher()
            {
                m_fileWatcher.Created -= m_fileProcessor.Watcher_Created;
                m_fileWatcher.Changed -= m_fileProcessor.Watcher_Changed;
                m_fileWatcher.Renamed -= m_fileProcessor.Watcher_Renamed;
                m_fileWatcher.Deleted -= m_fileProcessor.Watcher_Deleted;
                m_fileWatcher.Error -= m_fileProcessor.Watcher_Error;
                m_fileWatcher.Dispose();
            }

            #endregion
        }

        private class FileEnumerator
        {
            #region [ Members ]

            // Fields
            private readonly FileProcessor m_fileProcessor;
            private readonly CancellationToken m_cancellationToken;
            private string m_lastVisitedPath;
            private bool m_active;

            #endregion

            #region [ Constructors ]

            public FileEnumerator(string path, FileProcessor fileProcessor, CancellationToken cancellationToken)
            {
                Path = path;
                m_fileProcessor = fileProcessor;
                m_cancellationToken = cancellationToken;
            }

            #endregion

            #region [ Properties ]

            public string Path { get; }
            public FileEnumerator[] SubdirectoryEnumerators { get; private set; } = new FileEnumerator[0];

            public bool Active
            {
                get
                {
                    if (m_active)
                        return true;

                    FileEnumerator[] subdirectoryEnumerators = SubdirectoryEnumerators;
                    return m_active || subdirectoryEnumerators.Any(enumerator => enumerator.Active);
                }
            }

            public List<string> ActivelyVisitedPaths
            {
                get
                {
                    List<string> activelyVisitedPaths = new List<string>();
                    BuildActivelyVisitedPaths(activelyVisitedPaths);
                    return activelyVisitedPaths;
                }
            }

            #endregion

            #region [ Methods ]

            public void Enumerate(FileEnumerationStrategy enumerationStrategy)
            {
                if (Active)
                    throw new InvalidOperationException("Enumeration is already in progress.");

                if ((object)LogicalThread.CurrentThread == null)
                    throw new InvalidOperationException("Enumeration must be executed on a logical thread.");

                m_active = true;

                if (enumerationStrategy != FileEnumerationStrategy.ParallelSubdirectories)
                {
                    SubdirectoryEnumerators = new FileEnumerator[0];

                    // If subdirectories do not need to be processed in parallel,
                    // we can simply process a recursive call to enumerate all files
                    IEnumerable<string> fileEnumerable = EnumerateFilesRecursively(Path);
                    EnumerateNextFile(new EnumerableWrapper(fileEnumerable, m_cancellationToken));
                }
                else
                {
                    // To process subdirectories in parallel, we need to create a recursive structure
                    // of enumerators to process each subdirectory on its own logical thread
                    InitializeSubdirectoryEnumerators();
                    EnumerateNextDirectory(0);
                }
            }

            public void Cancel()
            {
                m_cancellationToken.Cancel();
            }

            // Recursively enumerates files under the given path, ignoring subfolders based on folder exclusion setting.
            private IEnumerable<string> EnumerateFilesRecursively(string path)
            {
                try
                {
                    IEnumerable<string> subDirectoryEnumerable = FilePath.EnumerateDirectories(path, exceptionHandler: m_fileProcessor.OnError)
                        .Where(directory => !m_fileProcessor.MatchesFolderExclusion(directory))
                        .SelectMany(EnumerateFilesRecursively);

                    return FilePath.EnumerateFiles(path, exceptionHandler: m_fileProcessor.OnError).Concat(subDirectoryEnumerable);
                }
                catch (Exception ex)
                {
                    m_fileProcessor.OnError(ex);
                    return null;
                }
            }

            // Initializes the FileEnumerators for subdirectories.
            private void InitializeSubdirectoryEnumerators()
            {
                // Immediately enumerate all subdirectories so we can build the array of FileEnumerators.
                // Using a fixed-size collection like an array greatly simplifies multithreaded access to the collection.
                using (EnumerableWrapper wrapper = new EnumerableWrapper(EnumerateDirectories(), m_cancellationToken))
                {
                    List<string> subDirectories = new List<string>();

                    while (wrapper.MoveNext())
                    {
                        if (!m_fileProcessor.MatchesFolderExclusion(wrapper.Current))
                            subDirectories.Add(wrapper.Current);
                    }

                    SubdirectoryEnumerators = subDirectories
                        .Select(path => new FileEnumerator(path, m_fileProcessor, m_cancellationToken))
                        .ToArray();
                }
            }

            // Enumerates the next subdirectory in the list of subdirectory enumerators.
            private void EnumerateNextDirectory(int index)
            {
                try
                {
                    m_active = !m_cancellationToken.IsCancelled;

                    if (!m_active)
                        return;

                    FileEnumerator[] subdirectoryEnumerators = SubdirectoryEnumerators;

                    if (index >= subdirectoryEnumerators.Length)
                    {
                        EnumerateNextFile(new EnumerableWrapper(EnumerateFiles(), m_cancellationToken));
                        return;
                    }

                    // Create a new thread for the subdirectory since this code is only invoked if the file
                    // processor is using the enumeration strategy for processing subdirectories in parallel
                    LogicalThread subdirectoryThread = m_fileProcessor.m_threadScheduler.CreateThread();
                    subdirectoryThread.Push(() => subdirectoryEnumerators[index].Enumerate(FileEnumerationStrategy.ParallelSubdirectories));

                    // Invoke the asynchronous call to enumerate the next directory
                    LogicalThread.CurrentThread.Push(() => EnumerateNextDirectory(index + 1));

                    m_lastVisitedPath = subdirectoryEnumerators[index].Path;
                }
                catch
                {
                    m_active = false;
                    throw;
                }
            }

            // Advances enumeration until the next file that matches the filter.
            private bool AdvanceToNextSubdirectory(EnumerableWrapper wrapper)
            {
                List<string> skippedFiles = new List<string>();

                while (true)
                {
                    if (!wrapper.MoveNext())
                        return false;

                    if (!m_fileProcessor.MatchesFolderExclusion(wrapper.Current))
                        return true;

                    m_lastVisitedPath = wrapper.Current;
                }
            }

            // Enumerates the next file in the given enumerable.
            private void EnumerateNextFile(EnumerableWrapper wrapper)
            {
                try
                {
                    m_active = AdvanceToNextMatch(wrapper);

                    if (!m_active)
                        return;

                    ProcessCurrentFileAndEnumerateNextFile(wrapper);
                    m_lastVisitedPath = wrapper.Current;
                }
                catch
                {
                    m_active = false;
                    wrapper.Dispose();
                    throw;
                }
                finally
                {
                    if (!wrapper.LastMove)
                    {
                        m_active = false;
                        wrapper.Dispose();
                    }
                }
            }

            // Advances enumeration until the next file that matches the filter.
            private bool AdvanceToNextMatch(EnumerableWrapper wrapper)
            {
                List<string> skippedFiles = new List<string>();

                try
                {
                    while (true)
                    {
                        if (!wrapper.MoveNext())
                            return false;

                        if (m_fileProcessor.MatchesFilter(wrapper.Current))
                            return true;

                        skippedFiles.Add(wrapper.Current);
                        m_lastVisitedPath = wrapper.Current;
                    }
                }
                finally
                {
                    m_fileProcessor.m_processingThread.Push(1, () =>
                    {
                        foreach (string filePath in skippedFiles)
                            m_fileProcessor.TouchAndSkip(filePath);
                    });
                }
            }

            // Processes the current file in the given enumerable.
            private void ProcessCurrentFileAndEnumerateNextFile(EnumerableWrapper wrapper)
            {
                LogicalThread enumerationThread = LogicalThread.CurrentThread;
                string filePath = wrapper.Current;

                // Kick off the operation to process the current file,
                // but don't enumerate to the next file until processing
                // for the current file has begun
                m_fileProcessor.m_processingThread.Push(1, () =>
                {
                    // Check the state of cancellation for the
                    // enumeration thread on the processing
                    // thread as well to speed up cancellation
                    if (m_cancellationToken.IsCancelled)
                    {
                        m_active = false;
                        wrapper.Dispose();
                        return;
                    }

                    enumerationThread.Push(() => EnumerateNextFile(wrapper));
                    m_fileProcessor.TouchAndProcess(filePath, false);
                });
            }

            private IEnumerable<string> EnumerateDirectories()
            {
                IEnumerable<string> directoryEnumerable = Directory.EnumerateDirectories(Path);

                return m_fileProcessor.OrderedEnumeration
                    ? directoryEnumerable.OrderBy(dir => dir)
                    : directoryEnumerable;
            }

            private IEnumerable<string> EnumerateFiles()
            {
                IEnumerable<string> fileEnumerable = Directory.EnumerateFiles(Path);

                return m_fileProcessor.OrderedEnumeration
                    ? fileEnumerable.OrderBy(dir => dir)
                    : fileEnumerable;
            }

            private void BuildActivelyVisitedPaths(List<string> activelyVisitedPaths)
            {
                string lastVisitedPath = m_lastVisitedPath;

                if (m_active && (object)lastVisitedPath != null)
                    activelyVisitedPaths.Add(lastVisitedPath);

                FileEnumerator[] subdirectoryEnumerators = SubdirectoryEnumerators;

                foreach (FileEnumerator enumerator in subdirectoryEnumerators)
                    enumerator.BuildActivelyVisitedPaths(activelyVisitedPaths);
            }

            #endregion
        }

        private class EnumerableWrapper : IDisposable
        {
            #region [ Members ]

            // Fields
            private IEnumerable<string> m_enumerable;
            private IEnumerator<string> m_enumerator;
            private bool m_lastMove;
            private string m_current;

            private CancellationToken m_cancellationToken;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new instance of the <see cref="EnumerableWrapper"/> class.
            /// </summary>
            /// <param name="enumerable">The enumerable to be wrapped.</param>
            /// <param name="cancellationToken">The token used to cancel the enumeration operation.</param>
            public EnumerableWrapper(IEnumerable<string> enumerable, CancellationToken cancellationToken)
            {
                if ((object)enumerable == null)
                    throw new ArgumentNullException(nameof(enumerable));

                m_enumerable = enumerable;
                m_enumerator = enumerable.GetEnumerator();
                m_cancellationToken = cancellationToken;
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets the value returned by the
            /// last call to <see cref="MoveNext"/>.
            /// </summary>
            public bool LastMove
            {
                get
                {
                    return m_lastMove;
                }
            }

            /// <summary>
            /// Gets the current value of the enumerator.
            /// </summary>
            public string Current
            {
                get
                {
                    return m_current;
                }
            }

            /// <summary>
            /// Gets the cancellation token for this enumerator.
            /// </summary>
            public CancellationToken CancellationToken
            {
                get
                {
                    return m_cancellationToken;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>True if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                m_lastMove = (!m_cancellationToken.IsCancelled) && m_enumerator.MoveNext();
                m_current = m_lastMove ? m_enumerator.Current : null;
                return m_lastMove;
            }

            /// <summary>
            /// Releases all the resources used by the enumerable.
            /// </summary>
            public void Dispose()
            {
                if (!m_disposed)
                {
                    try
                    {
                        (m_enumerator as IDisposable)?.Dispose();
                        (m_enumerable as IDisposable)?.Dispose();
                    }
                    finally
                    {
                        m_lastMove = false;
                        m_current = null;
                        m_disposed = true;
                    }
                }
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Default value for the <see cref="Filter"/> property.
        /// </summary>
        public const string DefaultFilter = @"**\*";

        /// <summary>
        /// Default value for the <see cref="FolderExclusion"/> property.
        /// </summary>
        public const string DefaultFolderExclusion = "";

        /// <summary>
        /// Default value for the <see cref="TrackChanges"/> property.
        /// </summary>
        public const bool DefaultTrackChanges = false;

        /// <summary>
        /// Default value for the <see cref="CachePath"/> property.
        /// </summary>
        public static readonly string DefaultCachePath = Path.Combine(FilePath.GetCommonApplicationDataFolder(), "FileProcessors");

        /// <summary>
        /// Default value for the <see cref="InternalBufferSize"/> property.
        /// </summary>
        public const int DefaultInternalBufferSize = 8192;

        /// <summary>
        /// Default value for the <see cref="MaxFragmentation"/> property.
        /// </summary>
        public const int DefaultMaxFragmentation = 10;

        /// <summary>
        /// Default value for the <see cref="EnumerationStrategy"/> property.
        /// </summary>
        public const FileEnumerationStrategy DefaultEnumerationStrategy = FileEnumerationStrategy.ParallelSubdirectories;

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
        private string m_folderExclusion;
        private Func<string, bool> m_filterMethod;
        private bool m_trackChanges;
        private string m_cachePath;
        private int m_internalBufferSize;
        private int m_maxFragmentation;
        private FileEnumerationStrategy m_enumerationStrategy;
        private bool m_orderedEnumeration;

        private readonly object m_trackedDirectoriesLock;
        private readonly List<TrackedDirectory> m_trackedDirectories;
        private LogicalThread m_sequentialEnumerationThread;
        private LogicalThread m_cleanProcessedFilesThread;
        private LogicalThreadOperation m_cleanProcessedFilesOperation;

        private LogicalThreadScheduler m_threadScheduler;
        private LogicalThread m_processingThread;
        private LogicalThread m_watcherThread;
        private Timer m_fileWatchTimer;
        private ManualResetEvent m_waitObject;

        private readonly Dictionary<string, DateTime> m_touchedFiles;
        private FileBackedHashSet<string> m_processedFiles;

        private int m_processedFileCount;
        private int m_skippedFileCount;
        private int m_requeuedFileCount;
        private DateTime m_lastCompactTime;
        private TimeSpan m_lastCompactDuration;

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
            m_folderExclusion = DefaultFolderExclusion;
            m_filterMethod = filePath => true;
            m_trackChanges = DefaultTrackChanges;
            m_cachePath = DefaultCachePath;
            m_internalBufferSize = DefaultInternalBufferSize;
            m_maxFragmentation = DefaultMaxFragmentation;
            m_enumerationStrategy = DefaultEnumerationStrategy;

            m_trackedDirectoriesLock = new object();
            m_trackedDirectories = new List<TrackedDirectory>();
            m_threadScheduler = new LogicalThreadScheduler(2);
            m_threadScheduler.UnhandledException += (sender, args) => OnError(args.Argument);
            m_processingThread = m_threadScheduler.CreateThread();
            m_watcherThread = m_threadScheduler.CreateThread();
            m_sequentialEnumerationThread = m_threadScheduler.CreateThread();
            m_cleanProcessedFilesThread = m_threadScheduler.CreateThread();
            m_cleanProcessedFilesOperation = new LogicalThreadOperation(m_cleanProcessedFilesThread, GetProcessedFiles, false);
            m_fileWatchTimer = new Timer(15000);
            m_fileWatchTimer.Elapsed += FileWatchTimer_Elapsed;
            m_waitObject = new ManualResetEvent(false);

            m_touchedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

            // As of .NET 4.7.1, the OrdinalIgnoreCase comparer produces a consistent hash code across 32-bit and 64-bit platforms,
            // but Microsoft provides no guarantees in terms of consistency across CLR versions. No testing has been done against Mono.
            // TODO: Consider implementing a platform-independent equality comparer for ignoring case, though it would be a breaking change.
            m_processedFiles = new FileBackedHashSet<string>(Path.Combine(m_cachePath, m_processorID.ToString()), StringComparer.OrdinalIgnoreCase);

            // Fragmentation statistics are only valid for the lifetime of the process
            // so go ahead and compact the processed files collection for good measure
            Compact();
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
        /// Gets or sets the pattern used to determine whether a folder should be excluded from enumeration.
        /// </summary>
        public string FolderExclusion
        {
            get
            {
                return m_folderExclusion;
            }
            set
            {
                m_folderExclusion = value ?? DefaultFolderExclusion;
            }
        }

        /// <summary>
        /// Gets or sets the user-defined method by which to filter
        /// files before raising the <see cref="Processing"/> event.
        /// </summary>
        public Func<string, bool> FilterMethod
        {
            get
            {
                return m_filterMethod;
            }
            set
            {
                m_filterMethod = value ?? (filePath => true);
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
                lock (m_trackedDirectoriesLock)
                {
                    if (m_trackedDirectories.Count > 0)
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
        /// Gets or sets the internal buffer size of each of the
        /// <see cref="SafeFileWatcher"/>s instantiated by this file processor.
        /// </summary>
        public int InternalBufferSize
        {
            get
            {
                return m_internalBufferSize;
            }
            set
            {
                m_internalBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of threads
        /// used for file processing and enumeration.
        /// </summary>
        public int MaxThreadCount
        {
            get
            {
                return m_threadScheduler.MaxThreadCount;
            }
            set
            {
                m_threadScheduler.MaxThreadCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of fragmentation allowed
        /// before compacting the lookup table for processed files.
        /// </summary>
        /// <remarks>
        /// If files are frequently removed from the watch directories,
        /// increasing this value may improve performance. Setting the
        /// value too high may result in the file perpetually growing
        /// or long pauses on the processing thread.
        /// </remarks>
        public int MaxFragmentation
        {
            get
            {
                return m_maxFragmentation;
            }
            set
            {
                m_maxFragmentation = value;
            }
        }

        /// <summary>
        /// Gets or sets the strategy to use to
        /// enumerate files in the watch directories.
        /// </summary>
        public FileEnumerationStrategy EnumerationStrategy
        {
            get
            {
                return m_enumerationStrategy;
            }
            set
            {
                m_enumerationStrategy = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether the file enumeration process
        /// should sort files and directories before raising events for enumerated files.
        /// </summary>
        public bool OrderedEnumeration
        {
            get
            {
                return m_orderedEnumeration;
            }
            set
            {
                m_orderedEnumeration = value;
            }
        }

        /// <summary>
        /// Gets the list of directories currently being tracked by the <see cref="FileProcessor"/>.
        /// </summary>
        public IList<string> TrackedDirectories
        {
            get
            {
                lock (m_trackedDirectoriesLock)
                {
                    return m_trackedDirectories
                        .Select(dir => dir.Path)
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Gets the list of paths that are being actively enumerated.
        /// </summary>
        public IList<string> ActivelyEnumeratedPaths
        {
            get
            {
                lock (m_trackedDirectoriesLock)
                {
                    return m_trackedDirectories
                        .SelectMany(dir => dir.ActivelyEnumeratedPaths)
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating if the file processor is actively enumerating.
        /// </summary>
        public bool IsEnumerating
        {
            get
            {
                lock (m_trackedDirectoriesLock)
                {
                    return m_trackedDirectories.Any(dir => dir.IsEnumerating);
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating if the file processor is actively
        /// purging processed files from the internal lookup table.
        /// </summary>
        public bool IsCleaning
        {
            get
            {
                return m_cleanProcessedFilesOperation.IsRunning;
            }
        }

        /// <summary>
        /// Gets the number of files
        /// processed by the file processor.
        /// </summary>
        public int ProcessedFileCount
        {
            get
            {
                return Interlocked.CompareExchange(ref m_processedFileCount, 0, 0);
            }
        }

        /// <summary>
        /// Gets the number of files skipped by the file processor.
        /// </summary>
        public int SkippedFileCount
        {
            get
            {
                return Interlocked.CompareExchange(ref m_skippedFileCount, 0, 0);
            }
        }

        /// <summary>
        /// Gets the number of times files that are being requeued by the file processor.
        /// </summary>
        public int RequeuedFileCount
        {
            get
            {
                return Interlocked.CompareExchange(ref m_requeuedFileCount, 0, 0);
            }
        }

        /// <summary>
        /// Gets the time at which the last operation to
        /// compact the set of processed files occurred.
        /// </summary>
        public DateTime LastCompactTime
        {
            get
            {
                return m_lastCompactTime;
            }
        }

        /// <summary>
        /// Gets the amount of time spent during the last
        /// operation to compact the set of processed files.
        /// </summary>
        public TimeSpan LastCompactDuration
        {
            get
            {
                return m_lastCompactDuration;
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
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            string fullPath = FilePath.GetAbsolutePath(path);
            TrackedDirectory trackedDirectory;

            lock (m_trackedDirectoriesLock)
            {
                if (TrackedDirectories.Contains(fullPath))
                    return;

                if (m_trackedDirectories.Count == 0)
                    m_fileWatchTimer.Start();

                trackedDirectory = new TrackedDirectory(this, fullPath);
                m_trackedDirectories.Add(trackedDirectory);
            }

            FileEnumerationStrategy enumerationStrategy = m_enumerationStrategy;

            LogicalThread enumerationThread = (enumerationStrategy == FileEnumerationStrategy.Sequential)
                    ? m_sequentialEnumerationThread
                    : m_threadScheduler.CreateThread();

            enumerationThread.Push(() => trackedDirectory.EnumerateFiles(enumerationStrategy));
        }

        /// <summary>
        /// Removes a directory from the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to stop tracking.</param>
        public void RemoveTrackedDirectory(string path)
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            string fullPath = FilePath.GetAbsolutePath(path);

            lock (m_trackedDirectoriesLock)
            {
                List<TrackedDirectory> trackedDirectoriesToRemove = m_trackedDirectories
                    .Where(dir => fullPath.Equals(dir.Path, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (TrackedDirectory trackedDirectory in trackedDirectoriesToRemove)
                {
                    m_trackedDirectories.Remove(trackedDirectory);
                    trackedDirectory.Dispose();
                }

                if (m_trackedDirectories.Count == 0)
                {
                    m_fileWatchTimer.Stop();
                    m_processedFiles.Close();
                }
            }
        }

        /// <summary>
        /// Forces enumeration of directories currently being watched.
        /// </summary>
        public void EnumerateWatchDirectories()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            List<TrackedDirectory> trackedDirectories;

            lock (m_trackedDirectoriesLock)
            {
                trackedDirectories = new List<TrackedDirectory>(m_trackedDirectories);
            }

            FileEnumerationStrategy enumerationStrategy = m_enumerationStrategy;

            LogicalThread GetThread()
            {
                return (enumerationStrategy == FileEnumerationStrategy.Sequential)
                    ? m_sequentialEnumerationThread
                    : m_threadScheduler.CreateThread();
            }

            foreach (TrackedDirectory trackedDirectory in trackedDirectories)
            {
                if (!trackedDirectory.IsEnumerating)
                    GetThread().Push(() => trackedDirectory.EnumerateFiles(enumerationStrategy));
            }

            m_cleanProcessedFilesOperation.RunOnceAsync();
        }

        /// <summary>
        /// Determines if the given file matches the filter string provided through the <see cref="Filter"/> property.
        /// </summary>
        /// <param name="filePath">The path to the file to be tested against the filter string.</param>
        /// <returns>True if the file matches the filter string; false otherwise.</returns>
        public bool MatchesFilter(string filePath)
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            string[] filters = m_filter.Split(Path.PathSeparator);

            return FilePath.IsFilePatternMatch(filters, filePath, true);
        }

        /// <summary>
        /// Determines if the given file matches the filter method provided through the <see cref="FilterMethod"/> property.
        /// </summary>
        /// <param name="filePath">The path to the file to be tested against the filter method.</param>
        /// <returns>True if the file matches the filter method; false otherwise.</returns>
        public bool MatchesFilterMethod(string filePath)
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            try
            {
                return m_filterMethod(filePath);
            }
            catch (Exception ex)
            {
                string message = $"An exception occurred while attempting to execute user-defined filter: {ex.Message}";
                OnError(new Exception(message, ex));
                return false;
            }
        }

        /// <summary>
        /// Determines if the given folder matches the exclusion string provided through the <see cref="FolderExclusion"/> property.
        /// </summary>
        /// <param name="folderPath">The path to the folder to be tested against the exclusion string.</param>
        /// <returns>True if the folder matches the exclusion string; false otherwise.</returns>
        public bool MatchesFolderExclusion(string folderPath)
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            string folderPathWithoutSeparator = folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string folderPathWithSeparator = $"{folderPathWithoutSeparator}\\";
            string[] patterns = m_folderExclusion.Split(Path.PathSeparator);

            return
                FilePath.IsFilePatternMatch(patterns, folderPathWithoutSeparator, true) ||
                FilePath.IsFilePatternMatch(patterns, folderPathWithSeparator, true);
        }

        /// <summary>
        /// Stops all enumeration operations that are currently running.
        /// </summary>
        public void StopEnumeration()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            lock (m_trackedDirectoriesLock)
            {
                foreach (TrackedDirectory trackedDirectory in m_trackedDirectories)
                    trackedDirectory.CancelEnumeration();
            }
        }

        /// <summary>
        /// Empties the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        public void ClearTrackedDirectories()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            lock (m_trackedDirectoriesLock)
            {
                while (m_trackedDirectories.Count > 0)
                    RemoveTrackedDirectory(m_trackedDirectories[0].Path);
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="FileProcessor"/> object.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_waitObject", Justification = "Thread pool threads could still be using this object.")]
        public void Dispose()
        {
            if (!m_disposed)
            {
                try
                {
                    StopEnumeration();
                    ClearTrackedDirectories();

                    if ((object)m_fileWatchTimer != null)
                    {
                        m_fileWatchTimer.Dispose();
                        m_fileWatchTimer = null;
                    }

                    if ((object)m_processedFiles != null)
                    {
                        m_processedFiles.Dispose();
                        m_processedFiles = null;
                    }

                    if ((object)m_waitObject != null)
                    {
                        // DO NOT dispose of the wait object
                        // We have to rely on the garbage collector to reclaim this
                        // one or else we risk unhandled exceptions on the thread pool
                        m_waitObject.Set();
                        m_waitObject = null;
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }
        
        // Checks and updates the touchedFiles lookup table, then starts the processing loop.
        private void TouchAndProcess(string filePath, bool raisedByFileWatcher)
        {
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            DateTime lastKnownWriteTime;

            if (!m_touchedFiles.TryGetValue(filePath, out lastKnownWriteTime) || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;

                if (!MatchesFilterMethod(filePath))
                {
                    Interlocked.Increment(ref m_skippedFileCount);
                    return;
                }

                StartProcessLoop(filePath, raisedByFileWatcher);
            }
        }

        // Checks and updates the touchedFiles lookup table, then increments the skipped file count.
        private void TouchAndSkip(string filePath)
        {
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            DateTime lastKnownWriteTime;

            if (!m_touchedFiles.TryGetValue(filePath, out lastKnownWriteTime) || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;
                Interlocked.Increment(ref m_skippedFileCount);
            }
        }

        // Starts a loop to process the file.
        // Continues looping so long as the user continues requesting to requeue.
        private void StartProcessLoop(string filePath, bool raisedByFileWatcher)
        {
            ManualResetEvent waitObject = m_waitObject;

            if ((object)waitObject == null)
                return;

            bool alreadyProcessed = m_processedFiles.Contains(filePath);
            int retryCount = 0;
            Func<int> retryCounter = () => retryCount;
            FileProcessorEventArgs args = new FileProcessorEventArgs(filePath, alreadyProcessed, raisedByFileWatcher, retryCounter);

            Action delayAndProcess = null;

            // 8 * 250 ms = 2 sec (cumulative: 2 sec)
            const int FastRetryLimit = 8;
            const int FastRetryDelay = 250;

            // 13 * 1000 ms = 13 sec (cumulative: 15 sec)
            const int QuickRetryLimit = 13 + FastRetryLimit;
            const int QuickRetryDelay = 1000;

            // 9 * 5000 ms = 45 sec (cumulative: 60 sec)
            const int RelaxedRetryLimit = 9 + QuickRetryLimit;
            const int RelaxedRetryDelay = 5000;

            // After 60 seconds, continue with the slow retry delay
            const int SlowRetryDelay = 60000;

            Action reprocessAction = () =>
            {
                ProcessFile(args);

                if (args.Requeue)
                {
                    int delay;

                    if (retryCount < FastRetryLimit)
                        delay = FastRetryDelay;
                    else if (retryCount < QuickRetryLimit)
                        delay = QuickRetryDelay;
                    else if (retryCount < RelaxedRetryLimit)
                        delay = RelaxedRetryDelay;
                    else
                        delay = SlowRetryDelay;

                    delayAndProcess.DelayAndExecute(waitObject, delay);

                    return;
                }

                Interlocked.Decrement(ref m_requeuedFileCount);
            };

            delayAndProcess = () =>
            {
                int priority = (++retryCount < RelaxedRetryLimit) ? 2 : 1;
                m_processingThread.Push(priority, reprocessAction);
            };

            ProcessFile(args);

            if (!args.Requeue)
                return;

            Interlocked.Increment(ref m_requeuedFileCount);
            delayAndProcess.DelayAndExecute(waitObject, 250);
        }

        // Attempts to processes the given file.
        // Returns true if the user requested to requeue the file.
        private void ProcessFile(FileProcessorEventArgs args)
        {
            string filePath = args.FullPath;

            // Requeue is requested by the user so
            // we must reset it before raising the event
            args.Requeue = false;

            if (m_disposed || !File.Exists(filePath))
                return;

            OnProcessing(args);

            // If the user requests to requeue the file,
            // we don't consider the file processed yet
            if (args.Requeue)
                return;

            Interlocked.Increment(ref m_processedFileCount);

            if (!args.AlreadyProcessed)
                m_processedFiles.Add(filePath);
        }

        // Gets the list of processed files from the file processor
        // and passes it to the cleaning thread for filtering.
        private void GetProcessedFiles()
        {
            m_processingThread.Push(1, () => m_cleanProcessedFilesOperation.ExecuteAction(() =>
            {
                string[] processedFiles = m_processedFiles.ToArray();
                FilterProcessedFiles(processedFiles);
            }));
        }

        // Filters the list of processed files to only the files that no longer
        // exist, then passes them back to the processing thread for removal.
        private void FilterProcessedFiles(string[] processedFiles)
        {
            m_cleanProcessedFilesThread.Push(() => m_cleanProcessedFilesOperation.ExecuteAction(() =>
            {
                IList<string> trackedDirectories = TrackedDirectories;

                string[] files = processedFiles
                    .Where(file => trackedDirectories.Any(dir => file.StartsWith(dir, StringComparison.OrdinalIgnoreCase)))
                    .Where(file => !File.Exists(file))
                    .ToArray();

                RemoveProcessedFiles(files);
            }));
        }

        // Removes the list of files that no longer exist,
        // then deactivates the cleaning thread.
        private void RemoveProcessedFiles(string[] files)
        {
            m_processingThread.Push(1, () => m_cleanProcessedFilesOperation.ExecuteAction(() =>
            {
                foreach (string file in files)
                {
                    m_touchedFiles.Remove(file);
                    m_processedFiles.Remove(file);
                }

                m_cleanProcessedFilesOperation.RunIfPending();
            }));
        }

        // Defragments the lookup table to reduce disk space usage.
        private void Compact()
        {
            if (LogicalThread.CurrentThread != m_processingThread)
            {
                m_processingThread.Push(Compact);
                return;
            }

            DateTime lastCompactTime = DateTime.UtcNow;
            m_processedFiles.Compact();
            m_lastCompactTime = lastCompactTime;
            m_lastCompactDuration = DateTime.UtcNow - m_lastCompactTime;
        }

        // Triggers the processing event for the given file.
        private void OnProcessing(FileProcessorEventArgs args)
        {
            Processing?.Invoke(this, args);
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
            string fullPath = args.FullPath;

            m_watcherThread.Push(() =>
            {
                if (!MatchesFilter(fullPath))
                {
                    m_processingThread.Push(1, () => TouchAndSkip(fullPath));
                    return;
                }

                m_processingThread.Push(2, () => TouchAndProcess(fullPath, true));
            });
        }

        // If the watcher tracks changes, queues the changed file for processing.
        private void Watcher_Changed(object sender, FileSystemEventArgs args)
        {
            if (!m_trackChanges)
                return;

            string fullPath = args.FullPath;

            m_watcherThread.Push(() =>
            {
                if (!MatchesFilter(fullPath))
                {
                    m_processingThread.Push(1, () => TouchAndSkip(fullPath));
                    return;
                }

                m_processingThread.Push(2, () =>
                {
                    m_touchedFiles.Remove(fullPath);
                    TouchAndProcess(fullPath, true);
                });
            });
        }

        // Track renames so that files whose names are changed can be updated in the processed files list.
        private void Watcher_Renamed(object sender, RenamedEventArgs args)
        {
            string oldFullPath = args.OldFullPath;
            string fullPath = args.FullPath;

            m_watcherThread.Push(() =>
            {
                bool matchesFilter = MatchesFilter(fullPath);
                int priority = matchesFilter ? 2 : 1;

                m_processingThread.Push(priority, () =>
                {
                    if (m_touchedFiles.TryGetValue(oldFullPath, out DateTime lastWriteTime))
                    {
                        m_touchedFiles.Remove(oldFullPath);
                        m_touchedFiles.Add(fullPath, lastWriteTime);
                    }

                    if (m_processedFiles.Remove(oldFullPath))
                        m_processedFiles.Add(fullPath);

                    if (matchesFilter)
                        TouchAndProcess(fullPath, true);
                    else
                        TouchAndSkip(fullPath);
                });
            });
        }

        // Track deletes so that files can be removed from the processed files list.
        private void Watcher_Deleted(object sender, FileSystemEventArgs args)
        {
            string fullPath = args.FullPath;

            m_processingThread.Push(2, () =>
            {
                m_touchedFiles.Remove(fullPath);
                m_processedFiles.Remove(fullPath);
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
            lock (m_trackedDirectoriesLock)
            {
                foreach (TrackedDirectory trackedDirectory in m_trackedDirectories)
                {
                    try { trackedDirectory.CheckFileWatcher(); }
                    catch (Exception ex) { OnError(ex); }
                }
            }

            // Determine if we need to defragment the lookup table for processed files
            m_processingThread.Push(1, () =>
            {
                if (m_processedFiles.FragmentationCount > m_maxFragmentation)
                    Compact();
            });
        }

        #endregion
    }
}
