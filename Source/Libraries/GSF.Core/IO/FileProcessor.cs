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

        // Nested Types

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

        [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "LogicalThreadLocal only needs disposal if being accessed from a non-logical thread")]
        private class FileEnumerator
        {
            #region [ Members ]

            // Fields
            private FileProcessor m_fileProcessor;
            private Dictionary<string, CancellationToken> m_cancellationTokens;
            private int m_enumerationThreads;

            private LogicalThread m_sequentialEnumerationThread;
            private LogicalThreadLocal<bool> m_isActive;
            private LogicalThreadLocal<Stack<Action>> m_wrapperStack;
            private LogicalThreadLocal<Queue<Action>> m_directoryQueue;

            private LogicalThread m_cleanProcessedFilesThread;
            private LogicalThreadOperation m_cleanProcessedFilesOperation;

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets the number of active enumeration threads.
            /// </summary>
            public int EnumerationThreads
            {
                get
                {
                    return Interlocked.CompareExchange(ref m_enumerationThreads, 0, 0);
                }
            }

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new instance of the <see cref="FileEnumerator"/> class.
            /// </summary>
            /// <param name="fileProcessor">The file processor that created the file enumerator.</param>
            public FileEnumerator(FileProcessor fileProcessor)
            {
                m_fileProcessor = fileProcessor;
                m_cancellationTokens = new Dictionary<string, CancellationToken>();

                m_sequentialEnumerationThread = m_fileProcessor.m_threadScheduler.CreateThread();
                m_isActive = new LogicalThreadLocal<bool>();
                m_wrapperStack = new LogicalThreadLocal<Stack<Action>>(() => new Stack<Action>());
                m_directoryQueue = new LogicalThreadLocal<Queue<Action>>(() => new Queue<Action>());

                m_cleanProcessedFilesThread = m_fileProcessor.m_threadScheduler.CreateThread();
                m_cleanProcessedFilesOperation = new LogicalThreadOperation(m_cleanProcessedFilesThread, GetProcessedFiles, false);
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Initiates enumeration of the watch directories.
            /// </summary>
            /// <param name="directory">The directory to be enumerated.</param>
            public void Enumerate(string directory)
            {
                CancellationToken cancellationToken;

                lock (m_cancellationTokens)
                {
                    if (m_cancellationTokens.TryGetValue(directory, out cancellationToken))
                        cancellationToken.Cancel();

                    cancellationToken = new CancellationToken();
                    m_cancellationTokens[directory] = cancellationToken;
                }

                switch (m_fileProcessor.EnumerationStrategy)
                {
                    // Sequential enumeration strategy kicks
                    // off all its processing on a single thread
                    case FileEnumerationStrategy.Sequential:
                        m_sequentialEnumerationThread.Push(() =>
                        {
                            // Create the enumerable wrappers for file and directory enumeration
                            EnumerableWrapper fileWrapper = new EnumerableWrapper(Directory.EnumerateFiles(directory), cancellationToken);
                            EnumerableWrapper directoryWrapper = new EnumerableWrapper(Directory.EnumerateDirectories(directory), cancellationToken);

                            if (m_isActive.Value)
                            {
                                // If the thread is already active,
                                // push processing into the directory queue
                                m_directoryQueue.Value.Enqueue(() =>
                                {
                                    m_wrapperStack.Value.Push(() => EnumerateNextFile(fileWrapper));
                                    EnumerateNextDirectory(directoryWrapper);
                                });
                            }
                            else
                            {
                                // If the thread is inactive, mark it as active
                                // and then begin processing the directory wrapper
                                ActivateThread();
                                m_wrapperStack.Value.Push(() => EnumerateNextFile(fileWrapper));
                                EnumerateNextDirectory(directoryWrapper);
                            }
                        });
                        break;

                    // Parallel processing kicks off new
                    // enumerations on their own thread
                    case FileEnumerationStrategy.ParallelWatchDirectories:
                    case FileEnumerationStrategy.ParallelSubdirectories:
                        m_fileProcessor.m_threadScheduler.CreateThread().Push(() =>
                        {
                            EnumerableWrapper fileWrapper = new EnumerableWrapper(Directory.EnumerateFiles(directory), cancellationToken);
                            EnumerableWrapper directoryWrapper = new EnumerableWrapper(Directory.EnumerateDirectories(directory), cancellationToken);
                            m_wrapperStack.Value.Push(() => EnumerateNextFile(fileWrapper));
                            LogicalThread.CurrentThread.Push(() => EnumerateNextDirectory(directoryWrapper));
                            ActivateThread();
                        });
                        break;
                }

                // Initiate the process to clean up the files
                // in the collection of processed files
                m_cleanProcessedFilesOperation.RunOnceAsync();
            }

            /// <summary>
            /// Cancels enumeration for a specific directory.
            /// </summary>
            /// <param name="directory">The directory for which enumeration should be cancelled.</param>
            public void Cancel(string directory)
            {
                CancellationToken cancellationToken;

                lock (m_cancellationTokens)
                {
                    if (!m_cancellationTokens.TryGetValue(directory, out cancellationToken))
                        return;

                    m_cancellationTokens.Remove(directory);
                }

                cancellationToken.Cancel();
            }

            /// <summary>
            /// Cancels all running enumeration operations.
            /// </summary>
            public void Cancel()
            {
                CancellationToken[] cancellationTokens;

                lock (m_cancellationTokens)
                {
                    cancellationTokens = m_cancellationTokens.Values.ToArray();
                    m_cancellationTokens.Clear();
                }

                foreach (CancellationToken cancellationToken in cancellationTokens)
                    cancellationToken.Cancel();
            }

            // Handle the next directory in the enumeration.
            private void EnumerateNextDirectory(EnumerableWrapper wrapper)
            {
                EnumerableWrapper fileWrapper;
                EnumerableWrapper directoryWrapper;
                string directory;

                try
                {
                    // Advance directory enumeration
                    if (!wrapper.MoveNext())
                    {
                        // No more directories, so dispose
                        // and then move to the next wrapper
                        wrapper.Dispose();
                        EnumerateNextWrapper();
                        return;
                    }

                    // Initialize the fileWrapper
                    // and directoryWrapper
                    directory = wrapper.Current;
                    fileWrapper = null;
                    directoryWrapper = null;
                }
                catch
                {
                    // If an error occurs, dispose of the
                    // wrapper and then move to the next wrapper
                    wrapper.Dispose();
                    EnumerateNextWrapper();
                    throw;
                }

                try
                {
                    switch (m_fileProcessor.EnumerationStrategy)
                    {
                        // Sequential and ParallelWatchDirectories strategies
                        // place subdirectories on the current thread
                        case FileEnumerationStrategy.Sequential:
                        case FileEnumerationStrategy.ParallelWatchDirectories:
                            // Create the fileWrapper and directoryWrapper objects
                            fileWrapper = new EnumerableWrapper(Directory.EnumerateFiles(directory), wrapper.CancellationToken);
                            directoryWrapper = new EnumerableWrapper(Directory.EnumerateDirectories(directory), wrapper.CancellationToken);

                            // Push the current directory wrapper onto the stack
                            m_wrapperStack.Value.Push(() => EnumerateNextDirectory(wrapper));

                            // Push the subdirectory's file wrapper onto the stack
                            m_wrapperStack.Value.Push(() => EnumerateNextFile(fileWrapper));

                            // Continue enumeration with the directory wrapper for the subdirectory
                            LogicalThread.CurrentThread.Push(() => EnumerateNextDirectory(directoryWrapper));
                            break;

                        // ParallelSubdirectories strategy spawns new threads for subdirectories
                        case FileEnumerationStrategy.ParallelSubdirectories:
                            // Create the fileWrapper and directoryWrapper objects
                            fileWrapper = new EnumerableWrapper(Directory.EnumerateFiles(directory), wrapper.CancellationToken);
                            directoryWrapper = new EnumerableWrapper(Directory.EnumerateDirectories(directory), wrapper.CancellationToken);

                            // Create a new thread, push the file wrapper onto the new thread's
                            // stack, then enumerate the directory wrapper on the new thread
                            m_fileProcessor.m_threadScheduler.CreateThread().Push(() =>
                            {
                                ActivateThread();
                                m_wrapperStack.Value.Push(() => EnumerateNextFile(fileWrapper));
                                EnumerateNextDirectory(directoryWrapper);
                            });

                            // Continue enumeration on this thread with the current directory wrapper
                            LogicalThread.CurrentThread.Push(() => EnumerateNextDirectory(wrapper));
                            break;

                        // The only other file enumeration
                        // strategy is no strategy at all
                        default:
                            // Clear out the stack and queue,
                            // then dispose of the wrapper
                            m_wrapperStack.Value.Clear();
                            m_directoryQueue.Value.Clear();
                            wrapper.Dispose();
                            DeactivateThread();
                            break;
                    }
                }
                catch
                {
                    // If an exception occurs, dispose of the file
                    // wrapper and directory wrapper, then continue
                    // enumeration with the current directory wrapper
                    if ((object)fileWrapper != null)
                        fileWrapper.Dispose();

                    if ((object)directoryWrapper != null)
                        directoryWrapper.Dispose();

                    LogicalThread.CurrentThread.Push(() => EnumerateNextDirectory(wrapper));

                    throw;
                }
            }

            // Handle the next file in the enumeration.
            private void EnumerateNextFile(EnumerableWrapper wrapper)
            {
                LogicalThread enumerationThread;
                Action enumerateNextFile;
                string file;

                try
                {
                    while (true)
                    {
                        // Advance enumeration until the
                        // next file that matches the filter
                        if (!wrapper.MoveNext())
                            return;

                        if (m_fileProcessor.MatchesFilter(wrapper.Current))
                            break;

                        Interlocked.Increment(ref m_fileProcessor.m_skippedFileCount);
                    }

                    // Prepare the callback to return execution to this thread
                    enumerationThread = LogicalThread.CurrentThread;
                    enumerateNextFile = () => EnumerateNextFile(wrapper);
                    file = wrapper.Current;

                    // Kick off the operation to process the current file,
                    // but don't enumerate to the next file until processing
                    // for the current file has begun
                    m_fileProcessor.m_processingThread.Push(1, () =>
                    {
                        // Check the state of cancellation for the
                        // enumeration thread on the processing
                        // thread as well to speed up cancellation
                        if (wrapper.CancellationToken.IsCancelled)
                        {
                            enumerationThread.Push(DeactivateThread);
                            return;
                        }

                        enumerationThread.Push(enumerateNextFile);
                        m_fileProcessor.TouchLockAndProcess(file);
                    });
                }
                catch
                {
                    // If an exception occurs,
                    // dispose of the enumerable wrapper
                    wrapper.Dispose();
                    throw;
                }
                finally
                {
                    // If there are no more files to enumerate,
                    // dispose of the wrapper and move to the next wrapper
                    if (!wrapper.LastMove)
                    {
                        wrapper.Dispose();
                        EnumerateNextWrapper();
                    }
                }
            }

            // Advances to processing the next wrapper in the stack.
            // If the stack is empty, processes the next wrapper in the queue.
            // If there are no wrappers left, deactivate the thread.
            private void EnumerateNextWrapper()
            {
                LogicalThread currentThread = LogicalThread.CurrentThread;

                if (m_wrapperStack.Value.Count > 0)
                    currentThread.Push(m_wrapperStack.Value.Pop());
                else if (m_directoryQueue.Value.Count > 0)
                    currentThread.Push(m_directoryQueue.Value.Dequeue());
                else
                    DeactivateThread();
            }

            // Activates the thread and increments
            // the number of active threads.
            private void ActivateThread()
            {
                m_isActive.Value = true;
                Interlocked.Increment(ref m_enumerationThreads);
            }

            // Deactivates the thread and decrements
            // the number of active threads.
            private void DeactivateThread()
            {
                Interlocked.Decrement(ref m_enumerationThreads);
                m_isActive.Value = false;
            }

            // Gets the list of processed files from the file processor
            // and passes it to the cleaning thread for filtering.
            private void GetProcessedFiles()
            {
                m_fileProcessor.m_processingThread.Push(1, () => m_cleanProcessedFilesOperation.ExecuteAction(() =>
                {
                    string[] processedFiles = m_fileProcessor.m_processedFiles.ToArray();
                    FilterProcessedFiles(processedFiles);
                }));
            }

            // Filters the list of processed files to only the files that no longer
            // exist, then passes them back to the processing thread for removal.
            private void FilterProcessedFiles(string[] processedFiles)
            {
                m_cleanProcessedFilesThread.Push(() => m_cleanProcessedFilesOperation.ExecuteAction(() =>
                {
                    IList<string> trackedDirectories = m_fileProcessor.TrackedDirectories;

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
                m_fileProcessor.m_processingThread.Push(1, () => m_cleanProcessedFilesOperation.ExecuteAction(() =>
                {
                    foreach (string file in files)
                        m_fileProcessor.m_processedFiles.Remove(file);

                    m_cleanProcessedFilesOperation.RunIfPending();
                }));
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Default value for the <see cref="Filter"/> property.
        /// </summary>
        public const string DefaultFilter = @"**\*";

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
        private Func<string, bool> m_filterMethod;
        private bool m_trackChanges;
        private string m_cachePath;
        private int m_internalBufferSize;
        private int m_maxFragmentation;
        private FileEnumerationStrategy m_enumerationStrategy;

        private readonly object m_fileWatchersLock;
        private readonly List<SafeFileWatcher> m_fileWatchers;
        private readonly FileEnumerator m_enumerator;

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
            m_filterMethod = filePath => true;
            m_trackChanges = DefaultTrackChanges;
            m_cachePath = DefaultCachePath;
            m_internalBufferSize = DefaultInternalBufferSize;
            m_maxFragmentation = DefaultMaxFragmentation;
            m_enumerationStrategy = DefaultEnumerationStrategy;

            m_fileWatchersLock = new object();
            m_fileWatchers = new List<SafeFileWatcher>();
            m_threadScheduler = new LogicalThreadScheduler();
            m_threadScheduler.UnhandledException += (sender, args) => OnError(args.Argument);
            m_processingThread = m_threadScheduler.CreateThread(2);
            m_watcherThread = m_threadScheduler.CreateThread();
            m_fileWatchTimer = new Timer(15000);
            m_fileWatchTimer.Elapsed += FileWatchTimer_Elapsed;
            m_waitObject = new ManualResetEvent(false);

            m_touchedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            m_processedFiles = new FileBackedHashSet<string>(Path.Combine(m_cachePath, m_processorID.ToString()), StringComparer.OrdinalIgnoreCase);

            // Create the enumerator last since we are passing
            // a reference to 'this' into its constructor
            m_enumerator = new FileEnumerator(this);
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

        /// <summary>
        /// Gets the number of enumeration threads currently running.
        /// </summary>
        public int EnumerationThreads
        {
            get
            {
                return m_enumerator.EnumerationThreads;
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
            string fullPath = FilePath.GetAbsolutePath(path);
            SafeFileWatcher watcher;

            if (!TrackedDirectories.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                watcher = new SafeFileWatcher(fullPath);
                watcher.IncludeSubdirectories = true;
                watcher.InternalBufferSize = m_internalBufferSize;

                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.Deleted += Watcher_Deleted;
                watcher.Error += Watcher_Error;

                lock (m_fileWatchersLock)
                {
                    if (m_fileWatchers.Count == 0)
                    {
                        m_processingThread.Push(2, LoadProcessedFiles);
                        m_fileWatchTimer.Start();
                    }

                    m_fileWatchers.Add(watcher);
                }

                watcher.EnableRaisingEvents = true;
                m_enumerator.Enumerate(fullPath);
            }
        }

        /// <summary>
        /// Removes a directory from the list of directories tracked by this <see cref="FileProcessor"/>.
        /// </summary>
        /// <param name="path">The path to the directory to stop tracking.</param>
        public void RemoveTrackedDirectory(string path)
        {
            string fullPath = FilePath.GetAbsolutePath(path);

            List<SafeFileWatcher> fileWatchersToRemove;

            lock (m_fileWatchers)
            {
                fileWatchersToRemove = m_fileWatchers
                    .Where(w => fullPath.Equals(w.Path, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (SafeFileWatcher watcher in fileWatchersToRemove)
                    RemoveFileWatcher(watcher);

                if (m_fileWatchers.Count == 0)
                {
                    m_fileWatchTimer.Stop();
                    m_processedFiles.Close();
                }
            }

            m_enumerator.Cancel(path);
        }

        /// <summary>
        /// Forces enumeration of directories currently being watched.
        /// </summary>
        public void EnumerateWatchDirectories()
        {
            lock (m_fileWatchers)
            {
                foreach (SafeFileWatcher fileWatcher in m_fileWatchers)
                    m_enumerator.Enumerate(fileWatcher.Path);
            }
        }

        /// <summary>
        /// Stops all enumeration operations that are currently running.
        /// </summary>
        public void StopEnumeration()
        {
            m_enumerator.Cancel();
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
                    m_enumerator.Cancel();
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
            string[] filters = m_filter.Split(Path.PathSeparator);

            if (!FilePath.IsFilePatternMatch(filters, filePath, true))
                return false;

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

        // Queues the file for processing.
        private void QueueFileForProcessing(string filePath)
        {
            if (!MatchesFilter(filePath))
            {
                Interlocked.Increment(ref m_skippedFileCount);
                return;
            }

            m_processingThread.Push(2, () => TouchLockAndProcess(filePath));
        }

        // Checks and updates the touchedFiles lookup table, then calls LockAndProcess.
        private void TouchLockAndProcess(string filePath)
        {
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            DateTime lastKnownWriteTime;

            if (!m_touchedFiles.TryGetValue(filePath, out lastKnownWriteTime) || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;
                StartProcessLoop(filePath);
            }
        }

        // Starts a loop to process the file.
        // Continues looping so long as the user continues requesting to requeue.
        private void StartProcessLoop(string filePath)
        {
            int retryCount = 0;
            Action delayAndProcess = null;

            delayAndProcess = () =>
            {
                int priority = (++retryCount < 32) ? 2 : 1;

                m_processingThread.Push(priority, () =>
                {
                    int delay;

                    if (ProcessFile(filePath))
                    {
                        if (retryCount < 8)
                            delay = 250;
                        else if (retryCount < 23)
                            delay = 1000;
                        else if (retryCount < 32)
                            delay = 5000;
                        else
                            delay = 60000;

                        DelayAndExecute(delayAndProcess, delay);

                        return;
                    }

                    Interlocked.Decrement(ref m_requeuedFileCount);
                });
            };

            if (ProcessFile(filePath))
            {
                Interlocked.Increment(ref m_requeuedFileCount);
                DelayAndExecute(delayAndProcess, 250);
            }
        }

        // Attempts to processes the given file.
        // Returns true if the user requested to requeue the file.
        private bool ProcessFile(string filePath)
        {
            bool alreadyProcessed;

            // If the file processor is disposed or the
            // file no longer exists, return immediately
            if (m_disposed || !File.Exists(filePath))
                return false;

            // Process the file at the given file path
            alreadyProcessed = m_processedFiles.Contains(filePath);

            // If the user requests to requeue the file, return true
            if (OnProcessing(filePath, alreadyProcessed))
                return true;

            Interlocked.Increment(ref m_processedFileCount);

            // Update the list of processed files
            // and save it back to the cache
            if (!alreadyProcessed)
                m_processedFiles.Add(filePath);

            return false;
        }

        // Executes the given action after waiting the given number of milliseconds.
        private void DelayAndExecute(Action action, int delay)
        {
            object waitHandleLock = new object();
            RegisteredWaitHandle waitHandle = null;

            WaitOrTimerCallback callback = (state, timeout) =>
            {
                if (Interlocked.Exchange(ref waitHandleLock, null) == null)
                    waitHandle.Unregister(null);

                if (!timeout)
                    return;

                action();
            };

            waitHandle = ThreadPool.RegisterWaitForSingleObject(m_waitObject, callback, null, delay, true);

            if (Interlocked.Exchange(ref waitHandleLock, null) == null)
                waitHandle.Unregister(null);
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
        private void RemoveFileWatcher(SafeFileWatcher watcher)
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
            m_watcherThread.Push(() =>
            {
                if (!MatchesFilter(args.FullPath))
                {
                    Interlocked.Increment(ref m_skippedFileCount);
                    return;
                }

                m_processingThread.Push(2, () => TouchLockAndProcess(args.FullPath));
            });
        }

        // If the watcher tracks changes, queues the changed file for processing.
        private void Watcher_Changed(object sender, FileSystemEventArgs args)
        {
            if (!m_trackChanges)
                return;

            m_watcherThread.Push(() =>
            {
                if (MatchesFilter(args.FullPath))
                {
                    m_processingThread.Push(2, () =>
                    {
                        m_touchedFiles.Remove(args.FullPath);
                        TouchLockAndProcess(args.FullPath);
                    });
                }
                else
                {
                    Interlocked.Increment(ref m_skippedFileCount);
                }
            });
        }

        // Track renames so that files whose names are changed can be updated in the processed files list.
        private void Watcher_Renamed(object sender, RenamedEventArgs args)
        {
            m_watcherThread.Push(() =>
            {
                bool oldMatch = MatchesFilter(args.OldFullPath);
                bool newMatch = MatchesFilter(args.FullPath);

                if (!oldMatch && !newMatch)
                    return;

                m_processingThread.Push(2, () =>
                {
                    if (oldMatch && m_touchedFiles.Remove(args.OldFullPath) && newMatch)
                        m_touchedFiles.Add(args.FullPath, File.GetLastWriteTimeUtc(args.FullPath));

                    if (oldMatch && m_processedFiles.Remove(args.OldFullPath))
                        m_processedFiles.Add(args.FullPath);

                    if (!oldMatch && newMatch)
                        TouchLockAndProcess(args.FullPath);
                });
            });
        }

        // Track deletes so that files can be removed from the processed files list.
        private void Watcher_Deleted(object sender, FileSystemEventArgs args)
        {
            m_watcherThread.Push(() =>
            {
                if (!MatchesFilter(args.FullPath))
                    return;

                m_processingThread.Push(2, () =>
                {
                    m_touchedFiles.Remove(args.FullPath);
                    m_processedFiles.Remove(args.FullPath);
                });
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
                            SafeFileWatcher newWatcher = new SafeFileWatcher(m_fileWatchers[i].Path);

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
                            m_enumerator.Enumerate(newWatcher.Path);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                }
            }

            // Determine if we need to defragment the lookup table for processed files
            m_processingThread.Push(1, () =>
            {
                if (m_processedFiles.FragmentationCount > m_maxFragmentation)
                {
                    DateTime lastCompactTime = DateTime.UtcNow;
                    m_processedFiles.Compact();
                    m_lastCompactTime = lastCompactTime;
                    m_lastCompactDuration = m_lastCompactTime - DateTime.UtcNow;
                }
            });
        }

        #endregion
    }
}
