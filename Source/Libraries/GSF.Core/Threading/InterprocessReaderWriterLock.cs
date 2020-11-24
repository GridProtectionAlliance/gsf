//******************************************************************************************************
//  InterprocessReaderWriterLock.cs - Gbtc
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
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Threading
{
    /// <summary>
    /// Represents an inter-process reader/writer lock using <see cref="Semaphore"/> and <see cref="Mutex"/> native locking mechanisms.
    /// </summary>
    public class InterprocessReaderWriterLock : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default maximum concurrent locks allowed for <see cref="InterprocessReaderWriterLock"/>.
        /// </summary>
        public const int DefaultMaximumConcurrentLocks = 10;

        // Fields
        private Mutex m_semaphoreLock;                  // Mutex used to synchronize access to Semaphore
        private Semaphore m_concurrencyLock;            // Semaphore used for reader/writer lock on consumer object
        private readonly int m_maximumConcurrentLocks;  // Maximum number of concurrent locks before waiting
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessReaderWriterLock"/> associated with the specified
        /// <paramref name="name"/> that identifies a source object needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifying name of source object needing concurrency locking (e.g., a path and file name).</param>
        public InterprocessReaderWriterLock(string name)
            : this(name, DefaultMaximumConcurrentLocks)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessReaderWriterLock"/> associated with the specified
        /// <paramref name="name"/> that identifies a source object needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifying name of source object needing concurrency locking (e.g., a path and file name).</param>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        /// <remarks>
        /// If more reader locks are requested than the <paramref name="maximumConcurrentLocks"/>, excess reader locks will simply
        /// wait until a lock is available (i.e., one of the existing reads completes).
        /// </remarks>
        public InterprocessReaderWriterLock(string name, int maximumConcurrentLocks)
        {
            m_maximumConcurrentLocks = maximumConcurrentLocks;
            m_semaphoreLock = InterprocessLock.GetNamedMutex(name);
            m_concurrencyLock = InterprocessLock.GetNamedSemaphore(name, m_maximumConcurrentLocks);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="InterprocessReaderWriterLock"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~InterprocessReaderWriterLock() =>
            Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the maximum concurrent reader locks allowed.
        /// </summary>
        public int MaximumConcurrentLocks => m_maximumConcurrentLocks;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="InterprocessReaderWriterLock"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InterprocessReaderWriterLock"/> object and optionally releases the managed resources.
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

                if (!(m_concurrencyLock is null))
                {
                    m_concurrencyLock.Close();
                    m_concurrencyLock = null;
                }

                if (!(m_semaphoreLock is null))
                {
                    m_semaphoreLock.Close();
                    m_semaphoreLock = null;
                }
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Tries to enter the lock in read mode.
        /// </summary>
        /// <remarks>
        /// Upon successful acquisition of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </remarks>
        public void EnterReadLock() =>
            TryEnterReadLock(Timeout.Infinite);

        /// <summary>
        /// Tries to enter the lock in write mode.
        /// </summary>
        /// <remarks>
        /// Upon successful acquisition of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </remarks>
        public void EnterWriteLock() =>
            TryEnterWriteLock(Timeout.Infinite);

        /// <summary>
        /// Exits read mode and returns the prior read lock count.
        /// </summary>
        /// <remarks>
        /// Upon successful acquisition of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </remarks>
        public int ExitReadLock() =>
            m_concurrencyLock.Release();    // Release the semaphore lock and restore the slot

        /// <summary>
        /// Exits write mode.
        /// </summary>
        /// <remarks>
        /// Upon successful acquisition of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </remarks>
        public void ExitWriteLock() =>
            m_semaphoreLock.ReleaseMutex(); // Release semaphore synchronization mutex lock

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional time-out.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="Timeout.Infinite"/>) to wait indefinitely.</param>
        /// <returns><c>true</c> if the calling thread entered read mode, otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// Upon successful acquisition of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </para>
        /// <para>
        /// Note that this function may wait as long as 2 * <paramref name="millisecondsTimeout"/> since the function first waits for synchronous access
        /// to the semaphore, then waits again on an available semaphore slot.
        /// </para>
        /// </remarks>
        public bool TryEnterReadLock(int millisecondsTimeout)
        {
            bool success;

            try
            {
                // Wait for system level mutex lock to synchronize access to semaphore
                if (!m_semaphoreLock.WaitOne(millisecondsTimeout))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Abnormal application terminations can leave a mutex abandoned, in
                // this case we now own the mutex so we just ignore the exception
            }

            try
            {
                // Wait for a semaphore slot to become available
                success = m_concurrencyLock.WaitOne(millisecondsTimeout);
            }
            finally
            {
                // Release mutex so others can access the semaphore
                m_semaphoreLock.ReleaseMutex();
            }

            return success;
        }

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="Timeout.Infinite"/>) to wait indefinitely.</param>
        /// <returns><c>true</c> if the calling thread entered write mode, otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// Upon successful acquisition of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </para>
        /// <para>
        /// Note that this function may wait as long as 2 * <paramref name="millisecondsTimeout"/> since the function first waits for synchronous access
        /// to the semaphore, then waits again on an available semaphore slot.
        /// </para>
        /// </remarks>
        public bool TryEnterWriteLock(int millisecondsTimeout)
        {
            bool success = false;

            try
            {
                // Wait for system level mutex lock to synchronize access to semaphore
                if (!m_semaphoreLock.WaitOne(millisecondsTimeout))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Abnormal application terminations can leave a mutex abandoned, in
                // this case we now own the mutex so we just ignore the exception
            }

            try
            {
                // At this point no other threads can acquire read or write access since we own the mutex.
                // Other threads may be busy reading, so we wait until all semaphore slots become available.
                // The only way to get a semaphore slot count is to execute a successful wait and release.
                long startTime = DateTime.UtcNow.Ticks;

                success = m_concurrencyLock.WaitOne(millisecondsTimeout);

                if (success)
                {
                    int count = m_concurrencyLock.Release();
                    int adjustedTimeout = millisecondsTimeout;

                    // After a successful wait and release the returned semaphore slot count will be -1 of the
                    // actual count since we owned one slot after a successful wait.
                    while (success && count != m_maximumConcurrentLocks - 1)
                    {
                        // Sleep to allow any remaining reads to complete
                        Thread.Sleep(1);

                        // Continue to adjust remaining time to accommodate user specified millisecond timeout
                        if (millisecondsTimeout > 0)
                            adjustedTimeout = millisecondsTimeout - (int)Ticks.ToMilliseconds(DateTime.UtcNow.Ticks - startTime);

                        if (adjustedTimeout < 0)
                            adjustedTimeout = 0;

                        success = m_concurrencyLock.WaitOne(adjustedTimeout);

                        if (success)
                            count = m_concurrencyLock.Release();
                    }
                }
            }
            finally
            {
                // If lock failed, release mutex so others can access the semaphore
                if (!success)
                    m_semaphoreLock.ReleaseMutex();
            }

            // Successfully entering write lock leaves state of semaphore locking mutex "owned", it is critical that
            // consumer call ExitWriteLock upon completion of write action to release mutex regardless of whether
            // their code succeeds or fails, that is, consumer should use the finally clause of a "try/finally"
            // expression to ExitWriteLock.
            return success;
        }

        #endregion
    }
}
