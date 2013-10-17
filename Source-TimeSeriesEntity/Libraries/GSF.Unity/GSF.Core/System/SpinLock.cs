#if MONO

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using GSF;

namespace System.Threading
{
    /// <summary>Provides a mutual exclusion lock primitive where a thread trying to acquire the lock waits in a loop repeatedly checking until the lock becomes available.</summary>
    [DebuggerDisplay("IsHeld = {IsHeld}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public struct SpinLock
    {
        #region [ Members ]

        // Fields
        private int m_lockState;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets whether the lock is currently held by any thread.
        /// </summary>
        /// <returns>true if the lock is currently held by any thread; otherwise false.</returns>
        public bool IsHeld
        {
            get
            {
                return this.m_lockState != 0;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Acquires the lock in a reliable manner, such that even if an exception occurs within the method call, <paramref name="lockTaken" /> can be examined reliably to determine whether the lock was acquired.
        /// </summary>
        /// <param name="lockTaken">True if the lock is acquired; otherwise, false.</param>
        public void Enter(ref bool taken)
        {
            if (Interlocked.CompareExchange(ref m_lockState, 1, 0) != 0)
            {
                int count = 0;

                while (Interlocked.CompareExchange(ref m_lockState, 1, 0) != 0)
                {
                    count++;

                    if (Environment.ProcessorCount > 1 && count <= 5)
                        Thread.SpinWait(25);
                    else
                        Thread.Sleep(0);
                }
            }

            taken = true;
        }

        /// <summary>
        /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call, <paramref name="lockTaken" /> can be examined reliably to determine whether the lock was acquired.
        /// </summary>
        /// <param name="lockTaken">True if the lock is acquired; otherwise, false.</param>
        public void TryEnter(ref bool lockTaken)
        {
            lockTaken = (Interlocked.CompareExchange(ref m_lockState, 1, 0) == 0);
        }
        
        /// <summary>
        /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call, <paramref name="lockTaken" /> can be examined reliably to determine whether the lock was acquired.
        /// </summary>
        /// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="lockTaken">True if the lock is acquired; otherwise, false. <paramref name="lockTaken" /> must be initialized to false prior to calling this method.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds.</exception>
        public void TryEnter(TimeSpan timeout, ref bool lockTaken)
        {
            long num = (long)timeout.TotalMilliseconds;

            if (num < -1L || num > 2147483647L)
                throw new ArgumentOutOfRangeException("timeout", timeout, "timeout value is out of range");

            TryEnter((int)timeout.TotalMilliseconds, ref lockTaken);
        }

        /// <summary>
        /// Attempts to acquire the lock in a reliable manner, such that even if an exception occurs within the method call, <paramref name="lockTaken" /> can be examined reliably to determine whether the lock was acquired.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
        /// <param name="lockTaken">True if the lock is acquired; otherwise, false. <paramref name="lockTaken" /> must be initialized to false prior to calling this method.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
        public void TryEnter(int millisecondsTimeout, ref bool lockTaken)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", "specified tiemout is a negative number other than -1.");

            if (millisecondsTimeout == -1)
            {
                Enter(ref lockTaken);
                return;
            }

            Ticks startTime = PrecisionTimer.UtcNow.Ticks;

            TryEnter(ref lockTaken);

            while (!lockTaken && (PrecisionTimer.UtcNow.Ticks - startTime).ToMilliseconds() < millisecondsTimeout)
            {
                Thread.Sleep(0);
                TryEnter(ref lockTaken);
            }
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public void Exit()
        {
            this.m_lockState = 0;
        }

        #endregion
    }
}

#endif