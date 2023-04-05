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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        private readonly bool m_raisedByFileWatcher;
        private readonly Func<int> m_retryCounter;
        private bool m_requeue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileProcessorEventArgs"/> class.
        /// </summary>
        /// <param name="fullPath">The full path to the file to be processed.</param>
        /// <param name="raisedByFileWatcher">Flag indicating whether this event was raised by the file watcher.</param>
        /// <param name="retryCounter">The function that provides the value for <see cref="RetryCount"/>.</param>
        public FileProcessorEventArgs(string fullPath, bool raisedByFileWatcher, Func<int> retryCounter)
        {
            m_fullPath = fullPath;
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

            // Events
            private event EventHandler<EventArgs<List<string>>> ActivelyVisitedPathsRequested;

            // Fields
            private readonly FileProcessor m_fileProcessor;
            private SafeFileWatcher m_fileWatcher;
            private CancellationToken m_cancellationToken;
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

            public List<string> ActivelyEnumeratedPaths
            {
                get
                {
                    List<string> activelyVisitedPaths = new List<string>();
                    EventArgs<List<string>> args = new EventArgs<List<string>>(activelyVisitedPaths);
                    ActivelyVisitedPathsRequested?.Invoke(this, args);
                    return activelyVisitedPaths;
                }
            }

            #endregion

            #region [ Methods ]

            public async Task EnumerateAsync(FileEnumerationStrategy fileEnumerationStrategy)
            {
                if (m_disposed)
                    return;

                CancellationToken cancellationToken = new CancellationToken();
                Interlocked.Exchange(ref m_cancellationToken, cancellationToken);
                DirectoryInfo directory = new DirectoryInfo(Path);
                await EnumerateDirectoryAsync(directory, fileEnumerationStrategy, cancellationToken);
            }

            public void CancelEnumeration()
            {
                CancellationToken cancellationToken = Interlocked.CompareExchange(ref m_cancellationToken, null, null);
                cancellationToken?.Cancel();
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

            private async Task EnumerateDirectoryAsync(DirectoryInfo directory, FileEnumerationStrategy fileEnumerationStrategy, CancellationToken cancellationToken)
            {
                bool parallelSubdirectories =
                    fileEnumerationStrategy == FileEnumerationStrategy.ParallelSubdirectories;

                LogicalThread thread = LogicalThread.CurrentThread;

                if (parallelSubdirectories)
                {
                    LogicalThreadScheduler scheduler = m_fileProcessor.m_threadScheduler;
                    thread = scheduler.CreateThread();
                    await thread.Join();
                }

                async Task ForEach(IEnumerable<Task> tasks)
                {
                    foreach (Task task in tasks)
                    {
                        if (cancellationToken.IsCancelled)
                            return;

                        await task;
                        await thread.Yield();
                    }
                }

                LogicalThread processingThread = m_fileProcessor.m_processingThread;
                string activePath = null;

                async Task VisitSubdirectoryAsync(DirectoryInfo subdirectory)
                {
                    activePath = subdirectory.FullName;

                    if (cancellationToken.IsCancelled)
                        return;

                    if (m_fileProcessor.MatchesFolderExclusion(subdirectory.FullName))
                        return;

                    await EnumerateDirectoryAsync(subdirectory, fileEnumerationStrategy, cancellationToken);
                }

                async Task<Task> VisitFileAsync(FileInfo file)
                {
                    activePath = file.FullName;

                    if (cancellationToken.IsCancelled)
                        return Task.CompletedTask;

                    async Task InvokeOnProcessingThread(Action action)
                    {
                        await processingThread.Join();
                        action();
                    }

                    DateTime lastWriteTime = file.LastWriteTimeUtc;
                    void Process() => m_fileProcessor.TouchAndProcess(file.FullName, lastWriteTime, false);
                    void Skip() => m_fileProcessor.TouchAndSkip(file.FullName, lastWriteTime);

                    if (!m_fileProcessor.MatchesFilter(file.FullName))
                        return InvokeOnProcessingThread(Skip);

                    Task processTask = InvokeOnProcessingThread(Process);
                    await thread.Yield();
                    return processTask;
                }

                async Task EnumerateSubdirectoriesAsync()
                {
                    Func<IEnumerable<Task>, Task> whenAll =
                        parallelSubdirectories ? Task.WhenAll : ForEach;

                    IEnumerable<Task> subdirectoryTasks = FilePath
                        .EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly, m_fileProcessor.OnError)
                        .Select(VisitSubdirectoryAsync);

                    await whenAll(subdirectoryTasks);
                }

                async Task EnumerateFilesAsync()
                {
                    IEnumerable<Task<Task>> fileTasks = FilePath
                        .EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                        .Select(VisitFileAsync);

                    List<Task> processTasks = new List<Task>();

                    foreach (Task<Task> fileTask in fileTasks)
                        processTasks.Add(await fileTask);

                    await Task.WhenAll(processTasks);
                }

                EventHandler<EventArgs<List<string>>> handler = (sender, args) =>
                {
                    if (!(activePath is null))
                        args.Argument.Add(activePath);
                };

                try
                {
                    ActivelyVisitedPathsRequested += handler;

                    if (parallelSubdirectories)
                    {
                        Task subdirectoriesTask = EnumerateSubdirectoriesAsync();
                        await EnumerateFilesAsync();
                        ActivelyVisitedPathsRequested -= handler;
                        await subdirectoriesTask;
                    }
                    else
                    {
                        await EnumerateFilesAsync();
                        await EnumerateSubdirectoriesAsync();
                    }
                }
                finally
                {
                    ActivelyVisitedPathsRequested -= handler;
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
        /// Default value for the <see cref="InternalBufferSize"/> property.
        /// </summary>
        public const int DefaultInternalBufferSize = 8192;

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
        private string m_filter;
        private string m_folderExclusion;
        private bool m_trackChanges;
        private int m_internalBufferSize;
        private FileEnumerationStrategy m_enumerationStrategy;
        private bool m_orderedEnumeration;

        private readonly object m_trackedDirectoriesLock;
        private readonly List<TrackedDirectory> m_trackedDirectories;
        private readonly TaskSynchronizedOperation m_enumerationOperation;

        private readonly LogicalThreadScheduler m_threadScheduler;
        private readonly LogicalThread m_processingThread;
        private readonly LogicalThread m_watcherThread;
        private readonly LogicalThread m_sequentialEnumerationThread;
        private readonly Timer m_fileWatchTimer;
        private readonly ManagedCancellationTokenSource m_requeueTokenSource;

        private readonly Dictionary<string, DateTime> m_touchedFiles;

        private int m_processedFileCount;
        private int m_skippedFileCount;
        private int m_requeuedFileCount;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FileProcessor"/> class.
        /// </summary>
        public FileProcessor()
        {
            m_filter = DefaultFilter;
            m_folderExclusion = DefaultFolderExclusion;
            m_trackChanges = DefaultTrackChanges;
            m_internalBufferSize = DefaultInternalBufferSize;
            m_enumerationStrategy = DefaultEnumerationStrategy;

            m_trackedDirectoriesLock = new object();
            m_trackedDirectories = new List<TrackedDirectory>();
            m_enumerationOperation = new TaskSynchronizedOperation(EnumerateWatchDirectoriesAsync, OnError);

            m_threadScheduler = new LogicalThreadScheduler(2);
            m_threadScheduler.UnhandledException += (sender, args) => OnError(args.Argument);
            m_processingThread = m_threadScheduler.CreateThread();
            m_watcherThread = m_threadScheduler.CreateThread();
            m_sequentialEnumerationThread = m_threadScheduler.CreateThread();
            m_fileWatchTimer = new Timer(15000);
            m_fileWatchTimer.Elapsed += FileWatchTimer_Elapsed;
            m_requeueTokenSource = new ManagedCancellationTokenSource();

            m_touchedFiles = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
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
        public bool IsEnumerating => m_enumerationOperation.IsRunning;

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
                    m_fileWatchTimer.Stop();
            }
        }

        /// <summary>
        /// Forces enumeration of directories currently being watched.
        /// </summary>
        public void EnumerateWatchDirectories()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            m_enumerationOperation.RunOnceAsync();
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
        /// Resets the internal file index so all files can be reprocessed.
        /// </summary>
        /// <remarks>
        /// As an optimization, the internal file index allows the file processor to enumerate
        /// the folder without raising events for files that were already processed, enabling
        /// the file processor to process files that were missed by the file watcher. However,
        /// because of this optimization, it can instead be difficult to force the file processor
        /// to reprocess files it has already processed once before. This method clears the
        /// internal file index, allowing for reprocessing of files that have already been processed.
        /// </remarks>
        public void ResetIndexAndStatistics()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(FileProcessor));

            m_processingThread.Push(2, () =>
            {
                m_touchedFiles.Clear();
                Interlocked.Exchange(ref m_processedFileCount, 0);
                Interlocked.Exchange(ref m_skippedFileCount, 0);
                Interlocked.Exchange(ref m_requeuedFileCount, 0);
            });
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
                    StopEnumeration();
                    ClearTrackedDirectories();
                    m_watcherThread.Clear();
                    m_fileWatchTimer.Stop();
                    m_fileWatchTimer.Dispose();
                    m_requeueTokenSource.Dispose();
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }

        // Forces enumeration of directories currently being watched.
        private async Task EnumerateWatchDirectoriesAsync()
        {
            List<TrackedDirectory> GetTrackedDirectories()
            {
                lock (m_trackedDirectoriesLock)
                    return new List<TrackedDirectory>(m_trackedDirectories);
            }

            List<TrackedDirectory> trackedDirectories = GetTrackedDirectories();
            FileEnumerationStrategy enumerationStrategy = m_enumerationStrategy;
            bool sequential = enumerationStrategy == FileEnumerationStrategy.Sequential;

            Func<LogicalThread> getEnumerationThread = sequential
                ? () => m_sequentialEnumerationThread
                : () => m_threadScheduler.CreateThread();

            async Task EnumerateTrackedDirectoryAsync(TrackedDirectory trackedDirectory)
            {
                try
                {
                    LogicalThread enumerationThread = getEnumerationThread();
                    await enumerationThread.Join();
                    await trackedDirectory.EnumerateAsync(enumerationStrategy);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }

            IEnumerable<Task> enumerationTasks = trackedDirectories
                .Select(EnumerateTrackedDirectoryAsync);

            async Task ForEach(IEnumerable<Task> tasks)
            {
                foreach (Task task in tasks)
                    await task;
            }

            Func<IEnumerable<Task>, Task> whenAll =
                sequential ? ForEach : Task.WhenAll;

            await whenAll(enumerationTasks);
        }
        
        // Checks and updates the touchedFiles lookup table, then starts the processing loop.
        private void TouchAndProcess(string filePath, DateTime lastWriteTime, bool raisedByFileWatcher)
        {
            bool touched = m_touchedFiles.TryGetValue(filePath, out DateTime lastKnownWriteTime);

            async Task ProcessAsync()
            {
                await RunProcessLoopAsync(filePath, raisedByFileWatcher);

                if (!touched)
                    Interlocked.Increment(ref m_processedFileCount);
            }

            if (!touched || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;
                _ = ProcessAsync();
            }
        }

        // Checks and updates the touchedFiles lookup table, then increments the skipped file count.
        private void TouchAndSkip(string filePath, DateTime lastWriteTime)
        {
            bool touched = m_touchedFiles.TryGetValue(filePath, out DateTime lastKnownWriteTime);

            if (!touched || lastKnownWriteTime < lastWriteTime)
            {
                m_touchedFiles[filePath] = lastWriteTime;

                if (!touched)
                    Interlocked.Increment(ref m_skippedFileCount);
            }
        }

        // Runs an asynchronous loop to process the file.
        // Continues looping so long as the user continues requesting to requeue.
        private async Task RunProcessLoopAsync(string filePath, bool raisedByFileWatcher)
        {
            int retryCount = 0;
            int RetryCounter() => retryCount;
            FileProcessorEventArgs args = new FileProcessorEventArgs(filePath, raisedByFileWatcher, RetryCounter);

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

            int GetDelay()
            {
                if (retryCount < FastRetryLimit)
                    return FastRetryDelay;

                if (retryCount < QuickRetryLimit)
                    return QuickRetryDelay;

                if (retryCount < RelaxedRetryLimit)
                    return RelaxedRetryDelay;

                return SlowRetryDelay;
            }

            using (m_requeueTokenSource.RetrieveToken(out var cancellationToken))
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    int priority = (retryCount < RelaxedRetryLimit) ? 2 : 1;
                    await m_processingThread.Join(priority);

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    ProcessFile(args);

                    if (!args.Requeue)
                        break;

                    if (retryCount == 0)
                        Interlocked.Increment(ref m_requeuedFileCount);

                    retryCount++;
                    int delay = GetDelay();
                    try { await Task.Delay(delay, cancellationToken); }
                    catch (TaskCanceledException) { break; }
                }

                if (retryCount > 0)
                    Interlocked.Decrement(ref m_requeuedFileCount);
            }
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
        }

        // Triggers the processing event for the given file.
        private void OnProcessing(FileProcessorEventArgs args)
        {
            Processing?.Invoke(this, args);
        }

        // Triggers the error event with the given exception.
        private void OnError(Exception ex)
        {
            Error?.Invoke(this, new ErrorEventArgs(ex));
        }

        // Queues the created file for processing.
        private void Watcher_Created(object sender, FileSystemEventArgs args)
        {
            string fullPath = args.FullPath;

            m_watcherThread.Push(() =>
            {
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(fullPath);

                if (!MatchesFilter(fullPath))
                {
                    m_processingThread.Push(1, () => TouchAndSkip(fullPath, lastWriteTime));
                    return;
                }

                m_processingThread.Push(2, () => TouchAndProcess(fullPath, lastWriteTime, true));
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
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(fullPath);

                if (!MatchesFilter(fullPath))
                {
                    m_processingThread.Push(1, () => TouchAndSkip(fullPath, lastWriteTime));
                    return;
                }

                m_processingThread.Push(2, () =>
                {
                    m_touchedFiles.Remove(fullPath);
                    TouchAndProcess(fullPath, lastWriteTime, true);
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
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(fullPath);
                bool matchesFilter = MatchesFilter(fullPath);
                int priority = matchesFilter ? 2 : 1;

                m_processingThread.Push(priority, () =>
                {
                    if (m_touchedFiles.TryGetValue(oldFullPath, out DateTime lastWriteTime))
                    {
                        m_touchedFiles.Remove(oldFullPath);
                        m_touchedFiles.Add(fullPath, lastWriteTime);
                    }

                    if (matchesFilter)
                        TouchAndProcess(fullPath, lastWriteTime, true);
                    else
                        TouchAndSkip(fullPath, lastWriteTime);
                });
            });
        }

        // Track deletes so that files can be removed from the processed files list.
        private void Watcher_Deleted(object sender, FileSystemEventArgs args)
        {
            string fullPath = args.FullPath;
            m_processingThread.Push(2, () => m_touchedFiles.Remove(fullPath));
        }

        // Triggers the error event for the exception encountered by the file watcher.
        private void Watcher_Error(object sender, ErrorEventArgs args)
        {
            Error?.Invoke(sender, args);
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
        }

        #endregion
    }
}
