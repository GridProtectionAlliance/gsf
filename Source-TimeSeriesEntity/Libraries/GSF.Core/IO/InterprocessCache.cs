//******************************************************************************************************
//  InterprocessCache.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  03/21/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/06/2011 - J. Ritchie Carroll
//       Added Flush() method to block current thread and wait for any pending save to complete.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
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
    /// Represents a serialized data cache that can be saved or read from multiple applications using interprocess synchronization.
    /// </summary>
    /// <remarks>
    /// Note that all file data in this class gets serialized to and from memory, as such, the design intention for this class is for
    /// use with smaller data sets such as serialized lists or dictionaries that need interprocess synchronized loading and saving.
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
        public const int DefaultMaximumRetryAttempts = 10;

        /// <summary>
        /// Default wait interval, in milliseconds, before retrying load of <see cref="InterprocessCache"/>.
        /// </summary>
        public const double DefaultRetryDelayInterval = 200.0D;

        // Fields
        private string m_fileName;                          // Path and file name of file needing interprocess synchronization
        private byte[] m_fileData;                          // Data loaded or to be saved
        private bool m_autoSave;                            // Flag to auto save when file data has changed
        private InterprocessReaderWriterLock m_fileLock;    // Interprocess reader/writer lock used to synchronize file access
        private ReaderWriterLockSlim m_dataLock;            // Thread level reader/writer lock used to synchronize file data access
        private ManualResetEventSlim m_loadIsReady;         // Wait handle used so that system will wait for file data load
        private ManualResetEventSlim m_saveIsReady;         // Wait handle used so that system will wait for file data save
        private FileSystemWatcher m_fileWatcher;            // Optional file watcher used to reload changes
        private readonly int m_maximumConcurrentLocks;               // Maximum concurrent reader locks allowed
        private int m_maximumRetryAttempts;                 // Maximum retry attempts allowed for loading file
        private readonly BitArray m_retryQueue;                      // Retry event queue
        private Timer m_retryTimer;           // File I/O retry timer
        private long m_lastRetryTime;                       // Time of last retry attempt
        private int m_retryCount;                           // Total number of retries attempted so far
        private bool m_disposed;                            // Class disposed flag

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
            m_dataLock = new ReaderWriterLockSlim();
            m_loadIsReady = new ManualResetEventSlim(false);
            m_saveIsReady = new ManualResetEventSlim(true);
            m_maximumConcurrentLocks = maximumConcurrentLocks;
            m_maximumRetryAttempts = DefaultMaximumRetryAttempts;
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
        ~InterprocessCache()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path and file name for the cache needing interprocess synchronization.
        /// </summary>
        public virtual string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException("FileName", "FileName cannot be null");

                m_fileName = FilePath.GetAbsolutePath(value);

                // Initialize reader/writer lock for given file name
                if ((object)m_fileLock != null)
                    m_fileLock.Dispose();

                m_fileLock = new InterprocessReaderWriterLock(m_fileName, m_maximumConcurrentLocks);
            }
        }

        /// <summary>
        /// Gets or sets file data for the cache to be saved or that has been loaded.
        /// </summary>
        /// <remarks>
        /// Setting value to <c>null</c> will create a zero-length file.
        /// </remarks>
        public virtual byte[] FileData
        {
            get
            {
                // Calls to this property are blocked until data is available
                WaitForLoad();

                m_dataLock.EnterReadLock();

                try
                {
                    // Make a copy of the file data for external use
                    if ((object)m_fileData != null)
                        return m_fileData.Copy(0, m_fileData.Length);
                }
                finally
                {
                    m_dataLock.ExitReadLock();
                }

                return null;
            }
            set
            {
                if ((object)m_fileName == null)
                    throw new ArgumentNullException("FileName", "FileName property must be defined before setting FileData");

                bool dataChanged = false;

                // If value is null, assume user means zero-length file
                if ((object)value == null)
                    value = new byte[0];

                m_dataLock.EnterWriteLock();

                try
                {
                    if (m_autoSave)
                        dataChanged = (m_fileData.CompareTo(value) != 0);

                    m_fileData = value;
                }
                finally
                {
                    m_dataLock.ExitWriteLock();
                }

                // Initiate save if data has changed
                if (dataChanged)
                {
                    m_saveIsReady.Reset();
                    ThreadPool.QueueUserWorkItem(SynchronizedWrite);
                }
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="InterprocessCache"/> should automatically initiate a save when <see cref="FileData"/> has been updated.
        /// </summary>
        public virtual bool AutoSave
        {
            get
            {
                return m_autoSave;
            }
            set
            {
                m_autoSave = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that enables system to monitor for changes in <see cref="FileName"/> and automatically reload <see cref="FileData"/>.
        /// </summary>
        public virtual bool ReloadOnChange
        {
            get
            {
                return (object)m_fileWatcher != null;
            }
            set
            {
                if (value && (object)m_fileWatcher == null)
                {
                    if ((object)m_fileName == null)
                        throw new ArgumentNullException("FileName", "FileName property must be defined before enabling ReloadOnChange");

                    // Setup file watcher to monitor for external updates
                    m_fileWatcher = new FileSystemWatcher();
                    m_fileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                    m_fileWatcher.Filter = FilePath.GetFileName(m_fileName);
                    m_fileWatcher.EnableRaisingEvents = true;
                    m_fileWatcher.Changed += m_fileWatcher_Changed;
                }
                else if (!value && (object)m_fileWatcher != null)
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
        public virtual int MaximumConcurrentLocks
        {
            get
            {
                return m_maximumConcurrentLocks;
            }
        }

        /// <summary>
        /// Maximum retry attempts allowed for loading or saving cache file data.
        /// </summary>
        public virtual int MaximumRetryAttempts
        {
            get
            {
                return m_maximumRetryAttempts;
            }
            set
            {
                m_maximumRetryAttempts = value;
            }
        }

        /// <summary>
        /// Wait interval, in milliseconds, before retrying load or save of cache file data.
        /// </summary>
        public virtual double RetryDelayInterval
        {
            get
            {
                return m_retryTimer.Interval;
            }
            set
            {
                m_retryTimer.Interval = value;
            }
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
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_fileWatcher != null)
                        {
                            m_fileWatcher.Changed -= m_fileWatcher_Changed;
                            m_fileWatcher.Dispose();
                        }
                        m_fileWatcher = null;

                        if ((object)m_retryTimer != null)
                        {
                            m_retryTimer.Elapsed -= m_retryTimer_Elapsed;
                            m_retryTimer.Dispose();
                        }
                        m_retryTimer = null;

                        if ((object)m_loadIsReady != null)
                            m_loadIsReady.Dispose();

                        m_loadIsReady = null;

                        if ((object)m_saveIsReady != null)
                            m_saveIsReady.Dispose();

                        m_saveIsReady = null;

                        if ((object)m_dataLock != null)
                            m_dataLock.Dispose();

                        m_dataLock = null;

                        if ((object)m_fileLock != null)
                            m_fileLock.Dispose();

                        m_fileLock = null;
                        m_fileName = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Initiates interprocess synchronized cache file save.
        /// </summary>
        /// <remarks>
        /// Subclasses should always call <see cref="WaitForLoad()"/> before calling this method.
        /// </remarks>
        public virtual void Save()
        {
            if ((object)m_fileName == null)
                throw new ArgumentNullException("FileName", "FileName is null, cannot initiate save");

            if ((object)m_fileData == null)
                throw new ArgumentNullException("FileData", "FileData is null, cannot initiate save");

            m_saveIsReady.Reset();
            ThreadPool.QueueUserWorkItem(SynchronizedWrite);
        }

        /// <summary>
        /// Initiates interprocess synchronized cache file load.
        /// </summary>
        /// <remarks>
        /// Subclasses should always call <see cref="WaitForLoad()"/> before calling this method.
        /// </remarks>
        public virtual void Load()
        {
            if ((object)m_fileName == null)
                throw new ArgumentNullException("FileName", "FileName is null, cannot initiate load");

            m_loadIsReady.Reset();
            ThreadPool.QueueUserWorkItem(SynchronizedRead);
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
        private void SynchronizedWrite(object state)
        {
            try
            {
                if (!m_disposed)
                {
                    if (m_fileLock.TryEnterWriteLock((int)m_retryTimer.Interval))
                    {
                        FileStream fileStream = null;

                        try
                        {
                            fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.Write, FileShare.None);

                            if (m_dataLock.TryEnterReadLock((int)m_retryTimer.Interval))
                            {
                                try
                                {
                                    // Disable file watch notification before update
                                    if ((object)m_fileWatcher != null)
                                        m_fileWatcher.EnableRaisingEvents = false;

                                    SaveFileData(fileStream, m_fileData);
                                    m_saveIsReady.Set();
                                }
                                finally
                                {
                                    m_dataLock.ExitReadLock();

                                    // Reenable file watch notification
                                    if ((object)m_fileWatcher != null)
                                        m_fileWatcher.EnableRaisingEvents = true;
                                }
                            }
                            else
                            {
                                RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire read lock for local cache"), WriteEvent);
                            }
                        }
                        catch (IOException ex)
                        {
                            RetrySynchronizedEvent(ex, WriteEvent);
                        }
                        finally
                        {
                            m_fileLock.ExitWriteLock();

                            if ((object)fileStream != null)
                                fileStream.Close();
                        }
                    }
                    else
                    {
                        RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire write lock for " + m_fileName), WriteEvent);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // This is an normal exception
                throw;
            }
            catch
            {
                // Other exceptions can happen (e.g., NullReferenceException) if thread resumes and the class is disposed middle way through this method
            }
        }

        /// <summary>
        /// Synchronously reads file data when no writes are active.
        /// </summary>
        private void SynchronizedRead(object state)
        {
            try
            {
                if (!m_disposed)
                {
                    if (File.Exists(m_fileName))
                    {
                        if (m_fileLock.TryEnterReadLock((int)m_retryTimer.Interval))
                        {
                            FileStream fileStream = null;

                            try
                            {
                                fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                                if (m_dataLock.TryEnterWriteLock((int)m_retryTimer.Interval))
                                {
                                    try
                                    {
                                        m_fileData = LoadFileData(fileStream);
                                    }
                                    finally
                                    {
                                        m_dataLock.ExitWriteLock();
                                    }

                                    // Release any threads waiting for file data
                                    m_loadIsReady.Set();
                                }
                                else
                                {
                                    RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire write lock for local cache"), ReadEvent);
                                }
                            }
                            catch (IOException ex)
                            {
                                RetrySynchronizedEvent(ex, ReadEvent);
                            }
                            finally
                            {
                                m_fileLock.ExitReadLock();

                                if ((object)fileStream != null)
                                    fileStream.Close();
                            }
                        }
                        else
                        {
                            RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire read lock for " + m_fileName), ReadEvent);
                        }
                    }
                    else
                    {
                        // File doesn't exist, create ane empty array representing a zero-length file
                        m_fileData = new byte[0];

                        // Release any threads waiting for file data
                        m_loadIsReady.Set();
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // This is an normal exception
                throw;
            }
            catch
            {
                // Other exceptions can happen (e.g., NullReferenceException) if thread resumes and the class is disposed middle way through this method
            }
        }

        /// <summary>
        /// Initiates a retry for specified event type.
        /// </summary>
        /// <param name="ex">Exception causing retry.</param>
        /// <param name="eventType">Event type to retry.</param>
        private void RetrySynchronizedEvent(Exception ex, int eventType)
        {
            // A retry is only being initiating for basic file I/O or locking errors, all other errors will initiate an unhandled
            // exception causing system exit. It would be an error, IMO, for the system to create values then not be able to load
            // them at next run or not be able to use values from last run because file could not be loaded.
            if (!m_disposed)
            {
                // We monitor basic I/O and lock failures occurring in quick succession, we can't allow retry activity to go on forever
                if (DateTime.UtcNow.Ticks - m_lastRetryTime > (long)Ticks.FromMilliseconds(m_retryTimer.Interval * m_maximumRetryAttempts))
                {
                    // Significant time has passed since last retry, so we reset counter
                    m_retryCount = 0;
                    m_lastRetryTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    m_retryCount++;

                    if (m_retryCount >= m_maximumRetryAttempts)
                        throw new UnauthorizedAccessException("Failed to " + (eventType == WriteEvent ? "write data to " : "read data from ") + m_fileName + " after " + m_maximumRetryAttempts + " attempts: " + ex.Message, ex);
                }

                // Technically the interprocess mutex will handle serialized access to the file, but if the OS or other process
                // not participating with the mutex has the file locked, all we can do is queue up a retry for this event.
                lock (m_retryQueue)
                {
                    m_retryQueue[eventType] = true;
                }
                m_retryTimer.Start();
            }
        }

        /// <summary>
        /// Retries specified serialize or deserialize event in case of file I/O failures.
        /// </summary>
        private void m_retryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!m_disposed)
            {
                WaitCallback callBackEvent = null;

                lock (m_retryQueue)
                {
                    // Reads should always occur first since you may need to load any
                    // newly written data before saving new data. Users can override
                    // load and save behavior to "merge" data sets if needed.
                    if (m_retryQueue[ReadEvent])
                    {
                        callBackEvent = SynchronizedRead;
                        m_retryQueue[ReadEvent] = false;
                    }
                    else if (m_retryQueue[WriteEvent])
                    {
                        callBackEvent = SynchronizedWrite;
                        m_retryQueue[WriteEvent] = false;
                    }

                    // If any events remain queued for retry, start timer for next event
                    if (m_retryQueue.Any(true))
                        m_retryTimer.Start();
                }

                if ((object)callBackEvent != null)
                    ThreadPool.QueueUserWorkItem(callBackEvent);
            }
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
