#if MONO

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
    /// <summary>Provides a slimmed down version of <see cref="T:System.Threading.ManualResetEvent" />.</summary>
    [__DynamicallyInvokable, DebuggerDisplay("Set = {IsSet}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class ManualResetEventSlim : IDisposable
    {
        private volatile object m_lock;
        private volatile ManualResetEvent m_eventObj;
        private volatile int m_combinedState;
        private static Action<object> s_cancellationTokenCallback = new Action<object>(ManualResetEventSlim.CancellationTokenCallback);
        private const int DEFAULT_SPIN_SP = 1;
        private const int DEFAULT_SPIN_MP = 10;
        private const int SignalledState_BitMask = -2147483648;
        private const int SignalledState_ShiftCount = 31;
        private const int Dispose_BitMask = 1073741824;
        private const int SpinCountState_BitMask = 1073217536;
        private const int SpinCountState_ShiftCount = 19;
        private const int SpinCountState_MaxValue = 2047;
        private const int NumWaitersState_BitMask = 524287;
        private const int NumWaitersState_ShiftCount = 0;
        private const int NumWaitersState_MaxValue = 524287;
        /// <summary>Gets the underlying <see cref="T:System.Threading.WaitHandle" /> object for this <see cref="T:System.Threading.ManualResetEventSlim" />.</summary>
        /// <returns>The underlying <see cref="T:System.Threading.WaitHandle" /> event object fore this <see cref="T:System.Threading.ManualResetEventSlim" />.</returns>
        [__DynamicallyInvokable]
        public WaitHandle WaitHandle
        {
            [__DynamicallyInvokable]
            get
            {
                this.ThrowIfDisposed();
                if (this.m_eventObj == null)
                {
                    this.LazyInitializeEvent();
                }
                return this.m_eventObj;
            }
        }
        /// <summary>Gets whether the event is set.</summary>
        /// <returns>true if the event has is set; otherwise, false.</returns>
        [__DynamicallyInvokable]
        public bool IsSet
        {
            [__DynamicallyInvokable]
            get
            {
                return 0 != ManualResetEventSlim.ExtractStatePortion(this.m_combinedState, -2147483648);
            }
            private set
            {
                this.UpdateStateAtomically((value ? 1 : 0) << 31, -2147483648);
            }
        }
        /// <summary>Gets the number of spin waits that will be occur before falling back to a kernel-based wait operation.</summary>
        /// <returns>Returns the number of spin waits that will be occur before falling back to a kernel-based wait operation.</returns>
        [__DynamicallyInvokable]
        public int SpinCount
        {
            [__DynamicallyInvokable]
            get
            {
                return ManualResetEventSlim.ExtractStatePortionAndShiftRight(this.m_combinedState, 1073217536, 19);
            }
            private set
            {
                this.m_combinedState = ((this.m_combinedState & -1073217537) | value << 19);
            }
        }
        private int Waiters
        {
            get
            {
                return ManualResetEventSlim.ExtractStatePortionAndShiftRight(this.m_combinedState, 524287, 0);
            }
            set
            {
                if (value >= 524287)
                {
                    throw new InvalidOperationException("ManualResetEventSlim_ctor_TooManyWaiters");
                }
                this.UpdateStateAtomically(value, 524287);
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with an initial state of nonsignaled.</summary>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManualResetEventSlim()
            : this(false)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with a Boolean value indicating whether to set the intial state to signaled.</summary>
        /// <param name="initialState">true to set the initial state signaled; false to set the initial state to nonsignaled.</param>
        [__DynamicallyInvokable]
        public ManualResetEventSlim(bool initialState)
        {
            this.Initialize(initialState, 10);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class with a Boolean value indicating whether to set the intial state to signaled and a specified spin count.</summary>
        /// <param name="initialState">true to set the initial state to signaled; false to set the initial state to nonsignaled.</param>
        /// <param name="spinCount">The number of spin waits that will occur before falling back to a kernel-based wait operation.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="spinCount" /> is less than 0 or greater than the maximum allowed value.</exception>
        [__DynamicallyInvokable]
        public ManualResetEventSlim(bool initialState, int spinCount)
        {
            if (spinCount < 0)
            {
                throw new ArgumentOutOfRangeException("spinCount");
            }
            if (spinCount > 2047)
            {
                throw new ArgumentOutOfRangeException("spinCount", "ManualResetEventSlim_ctor_SpinCountOutOfRange");
            }
            this.Initialize(initialState, spinCount);
        }
        /// <summary>Sets the state of the event to signaled, which allows one or more threads waiting on the event to proceed.</summary>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Set()
        {
            this.Set(false);
        }
        private void Set(bool duringCancellation)
        {
            this.IsSet = true;
            if (this.Waiters > 0)
            {
                lock (this.m_lock)
                {
                    Monitor.PulseAll(this.m_lock);
                }
            }
            ManualResetEvent eventObj = this.m_eventObj;
            if (eventObj != null && !duringCancellation)
            {
                lock (eventObj)
                {
                    if (this.m_eventObj != null)
                    {
                        this.m_eventObj.Set();
                    }
                }
            }
        }
        /// <summary>Sets the state of the event to nonsignaled, which causes threads to block.</summary>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
        [__DynamicallyInvokable]
        public void Reset()
        {
            this.ThrowIfDisposed();
            if (this.m_eventObj != null)
            {
                this.m_eventObj.Reset();
            }
            this.IsSet = false;
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set.</summary>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
        [__DynamicallyInvokable]
        public void Wait()
        {
            this.Wait(-1, default(CancellationToken));
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> receives a signal, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.OperationCanceledExcepton">
        ///   <paramref name="cancellationToken" /> was canceled.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public void Wait(CancellationToken cancellationToken)
        {
            this.Wait(-1, cancellationToken);
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the time interval.</summary>
        /// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
        [__DynamicallyInvokable]
        public bool Wait(TimeSpan timeout)
        {
            long num = (long)timeout.TotalMilliseconds;
            if (num < -1L || num > 2147483647L)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.Wait((int)num, default(CancellationToken));
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a <see cref="T:System.TimeSpan" /> to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
        /// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="T:System.Threading.OperationCanceledException">
        ///   <paramref name="cancellationToken" /> was canceled.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long num = (long)timeout.TotalMilliseconds;
            if (num < -1L || num > 2147483647L)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.Wait((int)num, cancellationToken);
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval.</summary>
        /// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed.</exception>
        [__DynamicallyInvokable]
        public bool Wait(int millisecondsTimeout)
        {
            return this.Wait(millisecondsTimeout, default(CancellationToken));
        }
        /// <summary>Blocks the current thread until the current <see cref="T:System.Threading.ManualResetEventSlim" /> is set, using a 32-bit signed integer to measure the time interval, while observing a <see cref="T:System.Threading.CancellationToken" />.</summary>
        /// <returns>true if the <see cref="T:System.Threading.ManualResetEventSlim" /> was set; otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="T:System.Threading.OperationCanceledException">
        ///   <paramref name="cancellationToken" /> was canceled.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.</exception>
        /// <exception cref="T:System.InvalidOperationException">The maximum number of waiters has been exceeded.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The object has already been disposed or the <see cref="T:System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (!this.IsSet)
            {
                if (millisecondsTimeout == 0)
                {
                    return false;
                }
                uint startTime = 0u;
                bool flag = false;
                int num = millisecondsTimeout;
                if (millisecondsTimeout != -1)
                {
                    startTime = TimeoutHelper.GetTime();
                    flag = true;
                }
                int num2 = 10;
                int num3 = 5;
                int num4 = 20;
                int spinCount = this.SpinCount;
                for (int i = 0; i < spinCount; i++)
                {
                    if (this.IsSet)
                    {
                        return true;
                    }
                    if (i < num2)
                    {
                        if (i == num2 / 2)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.SpinWait(PlatformHelper.ProcessorCount * (4 << i));
                        }
                    }
                    else
                    {
                        if (i % num4 == 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            Thread.Sleep(0);
                            //if (i % num3 == 0)
                            //{
                            //    Thread.Sleep(0);
                            //}
                            //else
                            //{
                            //    Thread.Sleep(0);
                            //}
                        }
                    }
                    if (i >= 100 && i % 10 == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                this.EnsureLockObjectCreated();
                using (cancellationToken.InternalRegisterWithoutEC(ManualResetEventSlim.s_cancellationTokenCallback, this))
                {
                    lock (this.m_lock)
                    {
                        while (!this.IsSet)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (flag)
                            {
                                num = TimeoutHelper.UpdateTimeOut(startTime, millisecondsTimeout);
                                if (num <= 0)
                                {
                                    bool result = false;
                                    return result;
                                }
                            }
                            this.Waiters++;
                            if (this.IsSet)
                            {
                                this.Waiters--;
                                bool result = true;
                                return result;
                            }
                            try
                            {
                                if (!Monitor.Wait(this.m_lock, num))
                                {
                                    bool result = false;
                                    return result;
                                }
                            }
                            finally
                            {
                                this.Waiters--;
                            }
                        }
                    }
                }
                return true;
            }
            return true;
        }
        /// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.ManualResetEventSlim" /> class.</summary>
        [__DynamicallyInvokable]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>When overridden in a derived class, releases the unmanaged resources used by the <see cref="T:System.Threading.ManualResetEventSlim" />, and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [__DynamicallyInvokable]
        protected virtual void Dispose(bool disposing)
        {
            if ((this.m_combinedState & 1073741824) != 0)
            {
                return;
            }
            this.m_combinedState |= 1073741824;
            if (disposing)
            {
                ManualResetEvent eventObj = this.m_eventObj;
                if (eventObj != null)
                {
                    lock (eventObj)
                    {
                        eventObj.Close();
                        this.m_eventObj = null;
                    }
                }
            }
        }
        private void Initialize(bool initialState, int spinCount)
        {
            this.m_combinedState = (initialState ? -2147483648 : 0);
            this.SpinCount = (PlatformHelper.IsSingleProcessor ? 1 : spinCount);
        }
        private void EnsureLockObjectCreated()
        {
            if (this.m_lock != null)
            {
                return;
            }
            object value = new object();
            Interlocked.CompareExchange(ref this.m_lock, value, null);
        }
        private bool LazyInitializeEvent()
        {
            bool isSet = this.IsSet;
            ManualResetEvent manualResetEvent = new ManualResetEvent(isSet);
            if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_eventObj, manualResetEvent, null) != null)
            {
                manualResetEvent.Close();
                return false;
            }
            bool isSet2 = this.IsSet;
            if (isSet2 != isSet)
            {
                lock (manualResetEvent)
                {
                    if (this.m_eventObj == manualResetEvent)
                    {
                        manualResetEvent.Set();
                    }
                }
            }
            return true;
        }
        private void ThrowIfDisposed()
        {
            if ((this.m_combinedState & 1073741824) != 0)
            {
                throw new ObjectDisposedException("ManualResetEventSlim_Disposed");
            }
        }
        private static void CancellationTokenCallback(object obj)
        {
            ManualResetEventSlim manualResetEventSlim = obj as ManualResetEventSlim;
            lock (manualResetEventSlim.m_lock)
            {
                Monitor.PulseAll(manualResetEventSlim.m_lock);
            }
        }
        private void UpdateStateAtomically(int newBits, int updateBitsMask)
        {
            SpinWait spinWait = default(SpinWait);
            while (true)
            {
                int combinedState = this.m_combinedState;
                int value = (combinedState & ~updateBitsMask) | newBits;
                if (Interlocked.CompareExchange(ref this.m_combinedState, value, combinedState) == combinedState)
                {
                    break;
                }
                spinWait.SpinOnce();
            }
        }
        private static int ExtractStatePortionAndShiftRight(int state, int mask, int rightBitShiftCount)
        {
            return (int)((uint)(state & mask) >> rightBitShiftCount);
        }
        private static int ExtractStatePortion(int state, int mask)
        {
            return state & mask;
        }
    }
}

#endif