//******************************************************************************************************
//  InterprocessCache.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/21/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Timers;
using GSF.Collections;
using GSF.Threading;
using Timer = System.Timers.Timer;

namespace GSF.IO
{
    /// <summary>
    /// Represents a serialized data cache that can be saved or read from multiple applications using inter-process synchronization.
    /// </summary>
    /// <remarks>
    /// Note that all file data in this class gets serialized to and from memory, as such, the design intention for this class is for
    /// use with smaller data sets such as serialized lists or dictionaries that need inter-process synchronized loading and saving.
    /// </remarks>
    public class InterprocessCache : IDisposable
    {
        #region [ Members ]

        // Constants
        private const int WriteEvent = 0;
        private const int ReadEvent = 1;

        /// <summary>
        /// Default maximum retry attempts allowed for loading <see cref="InterprocessCache"/>.
        /// </summary>
        public const int DefaultMaximumRetryAttempts = 5;

        /// <summary>
        /// Default wait interval, in milliseconds, before retrying load of <see cref="InterprocessCache"/>.
        /// </summary>
        public const double DefaultRetryDelayInterval = 1000.0D;

        // Fields
        private string m_fileName;                                  // Path and file name of file needing inter-process synchronization
        private byte[] m_fileData;                                  // Data loaded or to be saved
        private readonly LongSynchronizedOperation m_loadOperation; // Synchronized operation to asynchronously load data from the file
        private readonly LongSynchronizedOperation m_saveOperation; // Synchronized operation to asynchronously save data to the file
        private InterprocessReaderWriterLock m_fileLock;            // Inter-process reader/writer lock used to synchronize file access
        private ManualResetEventSlim m_loadIsReady;                 // Wait handle used so that system will wait for file data load
        private ManualResetEventSlim m_saveIsReady;                 // Wait handle used so that system will wait for file data save
        private SafeFileWatcher m_fileWatcher;                      // Optional file watcher used to reload changes
        private readonly BitArray m_retryQueue;                     // Retry event queue
        private Timer m_retryTimer;                                 // File I/O retry timer
        private long m_lastRetryTime;                               // Time of last retry attempt
        private int m_retryCount;                                   // Total number of retries attempted so far
        private bool m_disposed;                                    // Class disposed flag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessCache"/>.
        /// </summary>
        public InterprocessCache()
            : this(InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        public InterprocessCache(int maximumConcurrentLocks)
        {
            // Initialize field values
            m_loadOperation = new LongSynchronizedOperation(SynchronizedRead) { IsBackground = true };
            m_saveOperation = new LongSynchronizedOperation(SynchronizedWrite);
            m_loadIsReady = new ManualResetEventSlim(false);
            m_saveIsReady = new ManualResetEventSlim(true);
            MaximumConcurrentLocks = maximumConcurrentLocks;
            MaximumRetryAttempts = DefaultMaximumRetryAttempts;
            m_retryQueue = new BitArray(2);
            m_fileData = new byte[0];

            // Setup retry timer
            m_retryTimer = new Timer();
            m_retryTimer.Elapsed += m_retryTimer_Elapsed;
            m_retryTimer.AutoReset = false;
            m_retryTimer.Interval = DefaultRetryDelayInterval;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="InterprocessCache"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~InterprocessCache() =>
            Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path and file name for the cache needing inter-process synchronization.
        /// </summary>
        public string FileName
        {
            get => m_fileName;
            set
            {
                if (value is null)
                    throw new NullReferenceException("FileName cannot be null");

                m_fileName = FilePath.GetAbsolutePath(value);

                // Initialize reader/writer lock for given file name
                m_fileLock?.Dispose();
                m_fileLock = new InterprocessReaderWriterLock(m_fileName, MaximumConcurrentLocks);
            }
        }

        /// <summary>
        /// Gets or sets file data for the cache to be saved or that has been loaded.
        /// </summary>
        /// <remarks>
        /// Setting value to <c>null</c> will create a zero-length file.
        /// </remarks>
        public byte[] FileData
        {
            get
            {
                byte[] fileData;

                // Calls to this property are blocked until data is available
                WaitForLoad();

                fileData = Interlocked.CompareExchange(ref m_fileData, null, null);

                return fileData.Copy(0, fileData.Length);
            }
            set
            {
                if (m_fileName is null)
                    throw new NullReferenceException("FileName property must be defined before setting FileData");

                bool dataChanged = false;

                // If value is null, assume user means zero-length file
                if (value is null)
                    value = new byte[0];

                byte[] fileData = Interlocked.Exchange(ref m_fileData, value);

                if (AutoSave)
                    dataChanged = (fileData.CompareTo(value) != 0);

                // Initiate save if data has changed
                if (dataChanged)
                {
                    m_saveIsReady.Reset();
                    m_saveOperation.RunOnceAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="InterprocessCache"/> should automatically initiate a save when <see cref="FileData"/> has been updated.
        /// </summary>
        public bool AutoSave  { get; set; }

        /// <summary>
        /// Gets or sets flag that enables system to monitor for changes in <see cref="FileName"/> and automatically reload <see cref="FileData"/>.
        /// </summary>
        /// <remarks>
        /// Use extreme caution when enabling this property - it will be critical to dispose of this class when this property is set to true
        /// since it will create a file watcher that attaches to an event referenced by this class using pinned memory, as a result, this class
        /// will not get garbage collected until this class is disposed thus detaching the associated event.
        /// </remarks>
        public bool ReloadOnChange
        {
            get => !(m_fileWatcher is null);
            set
            {
                if (value && m_fileWatcher is null)
                {
                    if (m_fileName is null)
                        throw new NullReferenceException("FileName property must be defined before enabling ReloadOnChange");

                    // Setup file watcher to monitor for external updates
                    m_fileWatcher = new SafeFileWatcher();
                    m_fileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                    m_fileWatcher.Filter = FilePath.GetFileName(m_fileName);
                    m_fileWatcher.EnableRaisingEvents = true;
                    m_fileWatcher.Changed += m_fileWatcher_Changed;
                }
                else if (!value && !(m_fileWatcher is null))
                {
                    // Disable file watcher
                    m_fileWatcher.Changed -= m_fileWatcher_Changed;
                    m_fileWatcher.Dispose();
                    m_fileWatcher = null;
                }
            }
        }

        /// <summary>
        /// Gets the maximum concurrent reader locks allowed.
        /// </summary>
        public int MaximumConcurrentLocks { get; }

        /// <summary>
        /// Maximum retry attempts allowed for loading or saving cache file data.
        /// </summary>
        public int MaximumRetryAttempts { get; set; }

        /// <summary>
        /// Wait interval, in milliseconds, before retrying load or save of cache file data.
        /// </summary>
        public double RetryDelayInterval
        {
            get => m_retryTimer.Interval;
            set => m_retryTimer.Interval = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="InterprocessCache"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InterprocessCache"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (!(m_fileWatcher is null))
                {
                    m_fileWatcher.Changed -= m_fileWatcher_Changed;
                    m_fileWatcher.Dispose();
                    m_fileWatcher = null;
                }

                if (!(m_retryTimer is null))
                {
                    m_retryTimer.Elapsed -= m_retryTimer_Elapsed;
                    m_retryTimer.Dispose();
                    m_retryTimer = null;
                }

                if (!(m_loadIsReady is null))
                {
                    m_loadIsReady.Dispose();
                    m_loadIsReady = null;
                }

                if (!(m_saveIsReady is null))
                {
                    m_saveIsReady.Dispose();
                    m_saveIsReady = null;
                }

                if (!(m_fileLock is null))
                {
                    m_fileLock.Dispose();
                    m_fileLock = null;
                }

                m_fileName = null;
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Initiates inter-process synchronized cache file save.
        /// </summary>
        /// <remarks>
        /// Subclasses should always call <see cref="WaitForLoad()"/> before calling this method.
        /// </remarks>
        public virtual void Save()
        {
            if (m_disposed)
                throw new ObjectDisposedException(null);

            if (m_fileName is null)
                throw new NullReferenceException("FileName is null, cannot initiate save");

            if (m_fileData is null)
                throw new NullReferenceException("FileData is null, cannot initiate save");

            m_saveIsReady.Reset();
            m_saveOperation.RunOnceAsync();
        }

        /// <summary>
        /// Initiates inter-process synchronized cache file load.
        /// </summary>
        /// <remarks>
        /// Subclasses should always call <see cref="WaitForLoad()"/> before calling this method.
        /// </remarks>
        public virtual void Load()
        {
            if (m_disposed)
                throw new ObjectDisposedException(null);

            if (m_fileName is null)
                throw new NullReferenceException("FileName is null, cannot initiate load");

            m_loadIsReady.Reset();
            m_loadOperation.RunOnceAsync();
        }

        /// <summary>
        /// Blocks current thread and waits for any pending load to complete; wait time is <c><see cref="RetryDelayInterval"/> * <see cref="MaximumRetryAttempts"/></c>.
        /// </summary>
        public virtual void WaitForLoad()
        {
            WaitForLoad((int)(RetryDelayInterval * MaximumRetryAttempts));
        }

        /// <summary>
        /// Blocks current thread and waits for specified <paramref name="millisecondsTimeout"/> for any pending load to complete.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        public virtual void WaitForLoad(int millisecondsTimeout)
        {
            if (m_disposed)
                throw new ObjectDisposedException(null);

            // Calls to this method are blocked until data is available
            if (!m_loadIsReady.IsSet && !m_loadIsReady.Wait(millisecondsTimeout))
                throw new TimeoutException("Timeout waiting to read data from " + m_fileName);
        }

        /// <summary>
        /// Blocks current thread and waits for any pending save to complete; wait time is <c><see cref="RetryDelayInterval"/> * <see cref="MaximumRetryAttempts"/></c>.
        /// </summary>
        public virtual void WaitForSave()
        {
            WaitForSave((int)(RetryDelayInterval * MaximumRetryAttempts));
        }

        /// <summary>
        /// Blocks current thread and waits for specified <paramref name="millisecondsTimeout"/> for any pending save to complete.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        public virtual void WaitForSave(int millisecondsTimeout)
        {
            if (m_disposed)
                throw new ObjectDisposedException(null);

            // Calls to this method are blocked until data is saved
            if (!m_saveIsReady.IsSet && !m_saveIsReady.Wait(millisecondsTimeout))
                throw new TimeoutException("Timeout waiting to save data to " + m_fileName);
        }

        /// <summary>
        /// Handles serialization of file to disk; virtual method allows customization (e.g., pre-save encryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to serialize data.</param>
        /// <param name="fileData">File data to be serialized.</param>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected virtual void SaveFileData(FileStream fileStream, byte[] fileData)
        {
            fileStream.Write(fileData, 0, fileData.Length);
        }

        /// <summary>
        /// Handles deserialization of file from disk; virtual method allows customization (e.g., pre-load decryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to deserialize data.</param>
        /// <returns>Deserialized file data.</returns>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected virtual byte[] LoadFileData(FileStream fileStream)
        {
            return fileStream.ReadStream();
        }

        /// <summary>
        /// Synchronously writes file data when no reads are active.
        /// </summary>
        private void SynchronizedWrite()
        {
            try
            {
                if (m_disposed)
                    return;

                if (m_fileLock.TryEnterWriteLock((int)m_retryTimer.Interval))
                {
                    FileStream fileStream = null;
                    byte[] fileData;

                    try
                    {
                        fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.Write, FileShare.None);

                        try
                        {
                            // Disable file watch notification before update
                            if (!(m_fileWatcher is null))
                                m_fileWatcher.EnableRaisingEvents = false;

                            fileData = Interlocked.CompareExchange(ref m_fileData, null, null);
                            SaveFileData(fileStream, fileData);

                            // Release any threads waiting for file save
                            m_saveIsReady.Set();
                        }
                        finally
                        {
                            // Re-enable file watch notification
                            if (!(m_fileWatcher is null))
                                m_fileWatcher.EnableRaisingEvents = true;
                        }
                    }
                    catch (IOException ex)
                    {
                        RetrySynchronizedEvent(ex, WriteEvent);
                    }
                    finally
                    {
                        m_fileLock?.ExitWriteLock();
                        fileStream?.Close();
                    }
                }
                else
                {
                    RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire write lock for " + m_fileName), WriteEvent);
                }
            }
            catch (ThreadAbortException)
            {
                // Release any threads waiting for file save in case of thread abort
                m_saveIsReady?.Set();
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Release any threads waiting for file save in case of I/O or locking failures during write attempt
                m_saveIsReady?.Set();
                throw;
            }
            catch
            {
                // Other exceptions can happen, e.g., NullReferenceException if thread resumes and the class is disposed middle way through this method
                // or other serialization issues in call to SaveFileData, in these cases, release any threads waiting for file save
                m_saveIsReady?.Set();
            }
        }

        /// <summary>
        /// Synchronously reads file data when no writes are active.
        /// </summary>
        private void SynchronizedRead()
        {
            try
            {
                if (m_disposed)
                    return;

                if (File.Exists(m_fileName))
                {
                    if (m_fileLock.TryEnterReadLock((int)m_retryTimer.Interval))
                    {
                        FileStream fileStream = null;

                        try
                        {
                            fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                            Interlocked.Exchange(ref m_fileData, LoadFileData(fileStream));

                            // Release any threads waiting for file data
                            m_loadIsReady.Set();
                        }
                        catch (IOException ex)
                        {
                            RetrySynchronizedEvent(ex, ReadEvent);
                        }
                        finally
                        {
                            m_fileLock?.ExitReadLock();
                            fileStream?.Close();
                        }
                    }
                    else
                    {
                        RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire read lock for " + m_fileName), ReadEvent);
                    }
                }
                else
                {
                    // File doesn't exist, create an empty array representing a zero-length file
                    m_fileData = new byte[0];

                    // Release any threads waiting for file data
                    m_loadIsReady.Set();
                }
            }
            catch (ThreadAbortException)
            {
                // Release any threads waiting for file data in case of thread abort
                m_loadIsReady?.Set();
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                // Release any threads waiting for file load in case of I/O or locking failures during read attempt
                m_loadIsReady?.Set();
                throw;
            }
            catch
            {
                // Other exceptions can happen, e.g., NullReferenceException if thread resumes and the class is disposed middle way through this method
                // or other deserialization issues in call to LoadFileData, in these cases, release any threads waiting for file load
                m_loadIsReady?.Set();
            }
        }

        /// <summary>
        /// Initiates a retry for specified event type.
        /// </summary>
        /// <param name="ex">Exception causing retry.</param>
        /// <param name="eventType">Event type to retry.</param>
        private void RetrySynchronizedEvent(Exception ex, int eventType)
        {
            if (m_disposed)
                return;

            // A retry is only being initiating for basic file I/O or locking errors - monitor these failures occurring
            // in quick succession so that retry activity is not allowed to go on forever...
            if (DateTime.UtcNow.Ticks - m_lastRetryTime > (long)Ticks.FromMilliseconds(m_retryTimer.Interval * MaximumRetryAttempts))
            {
                // Significant time has passed since last retry, so we reset counter
                m_retryCount = 0;
                m_lastRetryTime = DateTime.UtcNow.Ticks;
            }
            else
            {
                m_retryCount++;

                if (m_retryCount >= MaximumRetryAttempts)
                    throw new UnauthorizedAccessException("Failed to " + (eventType == WriteEvent ? "write data to " : "read data from ") + m_fileName + " after " + MaximumRetryAttempts + " attempts: " + ex.Message, ex);
            }

            // Technically the inter-process mutex will handle serialized access to the file, but if the OS or other process
            // not participating with the mutex has the file locked, all we can do is queue up a retry for this event.
            lock (m_retryQueue)
            {
                m_retryQueue[eventType] = true;
            }
            m_retryTimer.Start();
        }

        /// <summary>
        /// Retries specified serialize or deserialize event in case of file I/O failures.
        /// </summary>
        private void m_retryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (m_disposed)
                return;

            LongSynchronizedOperation operation = null;

            lock (m_retryQueue)
            {
                // Reads should always occur first since you may need to load any
                // newly written data before saving new data. Users can override
                // load and save behavior to "merge" data sets if needed.
                if (m_retryQueue[ReadEvent])
                {
                    operation = m_loadOperation;
                    m_retryQueue[ReadEvent] = false;
                }
                else if (m_retryQueue[WriteEvent])
                {
                    operation = m_saveOperation;
                    m_retryQueue[WriteEvent] = false;
                }

                // If any events remain queued for retry, start timer for next event
                if (m_retryQueue.Any(true))
                    m_retryTimer.Start();
            }

            operation?.TryRunOnceAsync();
        }

        /// <summary>
        /// Reload file upon external modification.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">An object which provides data for directory events.</param>
        private void m_fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                Load();
        }

        #endregion
    }
}
