#if MONO

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
    /// <summary>Signals to a <see cref="T:System.Threading.CancellationToken" /> that it should be canceled.</summary>
    [__DynamicallyInvokable, ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class CancellationTokenSource : IDisposable
    {
        private static readonly CancellationTokenSource _staticSource_Set = new CancellationTokenSource(true);
        private static readonly CancellationTokenSource _staticSource_NotCancelable = new CancellationTokenSource(false);
        private static readonly int s_nLists = (PlatformHelper.ProcessorCount > 24) ? 24 : PlatformHelper.ProcessorCount;
        private volatile ManualResetEvent m_kernelEvent;
        private volatile SparselyPopulatedArray<CancellationCallbackInfo>[] m_registeredCallbacksLists;
        private volatile int m_state;
        private volatile int m_threadIDExecutingCallbacks = -1;
        private bool m_disposed;
        private CancellationTokenRegistration[] m_linkingRegistrations;
        private static readonly Action<object> s_LinkedTokenCancelDelegate = new Action<object>(CancellationTokenSource.LinkedTokenCancelDelegate);
        private volatile CancellationCallbackInfo m_executingCallback;
        private volatile Timer m_timer;
        private static readonly TimerCallback s_timerCallback = new TimerCallback(CancellationTokenSource.TimerCallbackLogic);
        private const int CANNOT_BE_CANCELED = 0;
        private const int NOT_CANCELED = 1;
        private const int NOTIFYING = 2;
        private const int NOTIFYINGCOMPLETE = 3;
        /// <summary>Gets whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
        /// <returns>Whether cancellation has been requested for this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
        [__DynamicallyInvokable]
        public bool IsCancellationRequested
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_state >= 2;
            }
        }
        internal bool IsCancellationCompleted
        {
            get
            {
                return this.m_state == 3;
            }
        }
        internal bool IsDisposed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_disposed;
            }
        }
        internal int ThreadIDExecutingCallbacks
        {
            get
            {
                return this.m_threadIDExecutingCallbacks;
            }
            set
            {
                this.m_threadIDExecutingCallbacks = value;
            }
        }
        /// <summary>Gets the <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
        /// <returns>The <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.CancellationTokenSource" />.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The token source has been disposed.</exception>
        [__DynamicallyInvokable]
        public CancellationToken Token
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                this.ThrowIfDisposed();
                return new CancellationToken(this);
            }
        }
        internal bool CanBeCanceled
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_state != 0;
            }
        }
        internal WaitHandle WaitHandle
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.m_kernelEvent != null)
                {
                    return this.m_kernelEvent;
                }
                ManualResetEvent manualResetEvent = new ManualResetEvent(false);
                if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_kernelEvent, manualResetEvent, null) != null)
                {
                    ((IDisposable)manualResetEvent).Dispose();
                }
                if (this.IsCancellationRequested)
                {
                    this.m_kernelEvent.Set();
                }
                return this.m_kernelEvent;
            }
        }
        internal CancellationCallbackInfo ExecutingCallback
        {
            get
            {
                return this.m_executingCallback;
            }
        }
        /// <summary>Initializes the <see cref="T:System.Threading.CancellationTokenSource" />.</summary>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public CancellationTokenSource()
        {
            this.m_state = 1;
        }
        [__DynamicallyInvokable]
        public CancellationTokenSource(TimeSpan delay)
        {
            long num = (long)delay.TotalMilliseconds;
            if (num < -1L || num > 2147483647L)
            {
                throw new ArgumentOutOfRangeException("delay");
            }
            this.InitializeWithTimer((int)num);
        }
        [__DynamicallyInvokable]
        public CancellationTokenSource(int millisecondsDelay)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            this.InitializeWithTimer(millisecondsDelay);
        }
        /// <summary>Communicates a request for cancellation.</summary>
        /// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Cancel()
        {
            this.Cancel(false);
        }
        /// <summary>Communicates a request for cancellation.</summary>
        /// <param name="throwOnFirstException">Specifies whether exceptions should immediately propagate.</param>
        /// <exception cref="T:System.ObjectDisposedException">This <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
        [__DynamicallyInvokable]
        public void Cancel(bool throwOnFirstException)
        {
            this.ThrowIfDisposed();
            this.NotifyCancellation(throwOnFirstException);
        }
        [__DynamicallyInvokable]
        public void CancelAfter(TimeSpan delay)
        {
            long num = (long)delay.TotalMilliseconds;
            if (num < -1L || num > 2147483647L)
            {
                throw new ArgumentOutOfRangeException("delay");
            }
            this.CancelAfter((int)num);
        }
        [__DynamicallyInvokable]
        public void CancelAfter(int millisecondsDelay)
        {
            this.ThrowIfDisposed();
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            }
            if (this.IsCancellationRequested)
            {
                return;
            }
            if (this.m_timer == null)
            {
                Timer timer = new Timer(CancellationTokenSource.s_timerCallback, this, -1, -1);
                if (Interlocked.CompareExchange<Timer>(ref this.m_timer, timer, null) != null)
                {
                    timer.Dispose();
                }
            }
            try
            {
                this.m_timer.Change(millisecondsDelay, -1);
            }
            catch (ObjectDisposedException)
            {
            }
        }
        /// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenSource" /> class.</summary>
        [__DynamicallyInvokable]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        [__DynamicallyInvokable]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.m_disposed)
                {
                    return;
                }
                if (this.m_timer != null)
                {
                    this.m_timer.Dispose();
                }
                CancellationTokenRegistration[] linkingRegistrations = this.m_linkingRegistrations;
                if (linkingRegistrations != null)
                {
                    this.m_linkingRegistrations = null;
                    for (int i = 0; i < linkingRegistrations.Length; i++)
                    {
                        linkingRegistrations[i].Dispose();
                    }
                }
                this.m_registeredCallbacksLists = null;
                if (this.m_kernelEvent != null)
                {
                    this.m_kernelEvent.Close();
                    this.m_kernelEvent = null;
                }
                this.m_disposed = true;
            }
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal void ThrowIfDisposed()
        {
            if (this.m_disposed)
            {
                CancellationTokenSource.ThrowObjectDisposedException();
            }
        }
        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(null, "CancellationTokenSource_Disposed");
        }
        internal static CancellationTokenSource InternalGetStaticSource(bool set)
        {
            if (!set)
            {
                return CancellationTokenSource._staticSource_NotCancelable;
            }
            return CancellationTokenSource._staticSource_Set;
        }
        internal CancellationTokenRegistration InternalRegister(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext executionContext)
        {
            this.ThrowIfDisposed();
            if (!this.IsCancellationRequested)
            {
                int num = Thread.CurrentThread.ManagedThreadId % CancellationTokenSource.s_nLists;
                CancellationCallbackInfo cancellationCallbackInfo = new CancellationCallbackInfo(callback, stateForCallback, targetSyncContext, executionContext, this);
                SparselyPopulatedArray<CancellationCallbackInfo>[] array = this.m_registeredCallbacksLists;
                if (array == null)
                {
                    SparselyPopulatedArray<CancellationCallbackInfo>[] array2 = new SparselyPopulatedArray<CancellationCallbackInfo>[CancellationTokenSource.s_nLists];
                    array = Interlocked.CompareExchange<SparselyPopulatedArray<CancellationCallbackInfo>[]>(ref this.m_registeredCallbacksLists, array2, null);
                    if (array == null)
                    {
                        array = array2;
                    }
                }
                SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read<SparselyPopulatedArray<CancellationCallbackInfo>>(ref array[num]);
                if (sparselyPopulatedArray == null)
                {
                    SparselyPopulatedArray<CancellationCallbackInfo> value = new SparselyPopulatedArray<CancellationCallbackInfo>(4);
                    Interlocked.CompareExchange<SparselyPopulatedArray<CancellationCallbackInfo>>(ref array[num], value, null);
                    sparselyPopulatedArray = array[num];
                }
                SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo = sparselyPopulatedArray.Add(cancellationCallbackInfo);
                CancellationTokenRegistration result = new CancellationTokenRegistration(cancellationCallbackInfo, registrationInfo);
                if (!this.IsCancellationRequested)
                {
                    return result;
                }
                if (!result.TryDeregister())
                {
                    return result;
                }
            }
            callback(stateForCallback);
            return default(CancellationTokenRegistration);
        }
        /// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
        /// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
        /// <param name="token1">The first <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
        /// <param name="token2">The second <see cref="T:System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
        /// <exception cref="T:System.ArgumentException">If any of the tokens cannot be canceled, they will not be linked. The returned source will be cancelable.-or-If any of the tokens are already canceled then the linked token will be returned in canceled state.</exception>
        [__DynamicallyInvokable]
        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            bool canBeCanceled = token2.CanBeCanceled;
            if (token1.CanBeCanceled)
            {
                cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[canBeCanceled ? 2 : 1];
                cancellationTokenSource.m_linkingRegistrations[0] = token1.InternalRegisterWithoutEC(CancellationTokenSource.s_LinkedTokenCancelDelegate, cancellationTokenSource);
            }
            if (canBeCanceled)
            {
                int num = 1;
                if (cancellationTokenSource.m_linkingRegistrations == null)
                {
                    cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[1];
                    num = 0;
                }
                cancellationTokenSource.m_linkingRegistrations[num] = token2.InternalRegisterWithoutEC(CancellationTokenSource.s_LinkedTokenCancelDelegate, cancellationTokenSource);
            }
            return cancellationTokenSource;
        }
        /// <summary>Creates a <see cref="T:System.Threading.CancellationTokenSource" /> that will be in the canceled state when any of the source tokens are in the canceled state.</summary>
        /// <returns>A <see cref="T:System.Threading.CancellationTokenSource" /> that is linked to the source tokens.</returns>
        /// <param name="tokens">The <see cref="T:System.Threading.CancellationToken" /> instances to observe.</param>
        /// <exception cref="T:System.ObjectDisposedException">A <see cref="T:System.Threading.CancellationTokenSource" /> associated with one of the source tokens has been disposed.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="tokens" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">If any of the tokens cannot be canceled, they will not be linked. The returned source will be cancelable.-or-If any of the tokens are already canceled then the linked token will be returned in canceled state.</exception>
        [__DynamicallyInvokable]
        public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }
            if (tokens.Length == 0)
            {
                throw new ArgumentException("CancellationToken_CreateLinkedToken_TokensIsEmpty");
            }
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.m_linkingRegistrations = new CancellationTokenRegistration[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].CanBeCanceled)
                {
                    cancellationTokenSource.m_linkingRegistrations[i] = tokens[i].InternalRegisterWithoutEC(CancellationTokenSource.s_LinkedTokenCancelDelegate, cancellationTokenSource);
                }
            }
            return cancellationTokenSource;
        }
        internal void WaitForCallbackToComplete(CancellationCallbackInfo callbackInfo)
        {
            SpinWait spinWait = default(SpinWait);
            while (this.ExecutingCallback == callbackInfo)
            {
                spinWait.SpinOnce();
            }
        }
        private static void LinkedTokenCancelDelegate(object source)
        {
            CancellationTokenSource cancellationTokenSource = source as CancellationTokenSource;
            cancellationTokenSource.Cancel();
        }
        private CancellationTokenSource(bool set)
        {
            this.m_state = (set ? 3 : 0);
        }
        private void InitializeWithTimer(int millisecondsDelay)
        {
            this.m_state = 1;
            this.m_timer = new Timer(CancellationTokenSource.s_timerCallback, this, millisecondsDelay, -1);
        }
        private static void TimerCallbackLogic(object obj)
        {
            CancellationTokenSource cancellationTokenSource = (CancellationTokenSource)obj;
            if (!cancellationTokenSource.IsDisposed)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    if (!cancellationTokenSource.IsDisposed)
                    {
                        throw;
                    }
                }
            }
        }
        private void NotifyCancellation(bool throwOnFirstException)
        {
            if (this.IsCancellationRequested)
            {
                return;
            }
            if (Interlocked.CompareExchange(ref this.m_state, 2, 1) == 1)
            {
                Timer timer = this.m_timer;
                if (timer != null)
                {
                    timer.Dispose();
                }
                this.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                if (this.m_kernelEvent != null)
                {
                    this.m_kernelEvent.Set();
                }
                this.ExecuteCallbackHandlers(throwOnFirstException);
            }
        }
        private void ExecuteCallbackHandlers(bool throwOnFirstException)
        {
            List<Exception> list = null;
            SparselyPopulatedArray<CancellationCallbackInfo>[] registeredCallbacksLists = this.m_registeredCallbacksLists;
            if (registeredCallbacksLists == null)
            {
                Interlocked.Exchange(ref this.m_state, 3);
                return;
            }
            try
            {
                for (int i = 0; i < registeredCallbacksLists.Length; i++)
                {
                    SparselyPopulatedArray<CancellationCallbackInfo> sparselyPopulatedArray = Volatile.Read<SparselyPopulatedArray<CancellationCallbackInfo>>(ref registeredCallbacksLists[i]);
                    if (sparselyPopulatedArray != null)
                    {
                        for (SparselyPopulatedArrayFragment<CancellationCallbackInfo> sparselyPopulatedArrayFragment = sparselyPopulatedArray.Tail; sparselyPopulatedArrayFragment != null; sparselyPopulatedArrayFragment = sparselyPopulatedArrayFragment.Prev)
                        {
                            for (int j = sparselyPopulatedArrayFragment.Length - 1; j >= 0; j--)
                            {
                                this.m_executingCallback = sparselyPopulatedArrayFragment[j];
                                if (this.m_executingCallback != null)
                                {
                                    CancellationCallbackCoreWorkArguments cancellationCallbackCoreWorkArguments = new CancellationCallbackCoreWorkArguments(sparselyPopulatedArrayFragment, j);
                                    try
                                    {
                                        if (this.m_executingCallback.TargetSyncContext != null)
                                        {
                                            this.m_executingCallback.TargetSyncContext.Send(new SendOrPostCallback(this.CancellationCallbackCoreWork_OnSyncContext), cancellationCallbackCoreWorkArguments);
                                            this.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                                        }
                                        else
                                        {
                                            this.CancellationCallbackCoreWork(cancellationCallbackCoreWorkArguments);
                                        }
                                    }
                                    catch (Exception item)
                                    {
                                        if (throwOnFirstException)
                                        {
                                            throw;
                                        }
                                        if (list == null)
                                        {
                                            list = new List<Exception>();
                                        }
                                        list.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.m_state = 3;
                this.m_executingCallback = null;
                Thread.MemoryBarrier();
            }
            if (list != null)
            {
                throw new AggregateException(list);
            }
        }
        private void CancellationCallbackCoreWork_OnSyncContext(object obj)
        {
            this.CancellationCallbackCoreWork((CancellationCallbackCoreWorkArguments)obj);
        }
        private void CancellationCallbackCoreWork(CancellationCallbackCoreWorkArguments args)
        {
            CancellationCallbackInfo cancellationCallbackInfo = args.m_currArrayFragment.SafeAtomicRemove(args.m_currArrayIndex, this.m_executingCallback);
            if (cancellationCallbackInfo == this.m_executingCallback)
            {
                if (cancellationCallbackInfo.TargetExecutionContext != null)
                {
                    cancellationCallbackInfo.CancellationTokenSource.ThreadIDExecutingCallbacks = Thread.CurrentThread.ManagedThreadId;
                }
                cancellationCallbackInfo.ExecuteCallback();
            }
        }
    }
}

#endif