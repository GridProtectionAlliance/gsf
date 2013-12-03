#if MONO

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading
{
    [Serializable]
    internal enum StackCrawlMark
    {
        LookForMe,
        LookForMyCaller,
        LookForMyCallersCaller,
        LookForThread
    }

    /// <summary>Propagates notification that operations should be canceled.</summary>
    [__DynamicallyInvokable, DebuggerDisplay("IsCancellationRequested = {IsCancellationRequested}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public struct CancellationToken
    {
        private CancellationTokenSource m_source;
        private static readonly Action<object> s_ActionToActionObjShunt = new Action<object>(CancellationToken.ActionToActionObjShunt);
        /// <summary>Returns an empty CancellationToken value.</summary>
        /// <returns>Returns an empty CancellationToken value.</returns>
        [__DynamicallyInvokable]
        public static CancellationToken None
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return default(CancellationToken);
            }
        }
        /// <summary>Gets whether cancellation has been requested for this token.</summary>
        /// <returns>true if cancellation has been requested for this token; otherwise false.</returns>
        [__DynamicallyInvokable]
        public bool IsCancellationRequested
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_source != null && this.m_source.IsCancellationRequested;
            }
        }
        /// <summary>Gets whether this token is capable of being in the canceled state.</summary>
        /// <returns>true if this token is capable of being in the canceled state; otherwise false.</returns>
        [__DynamicallyInvokable]
        public bool CanBeCanceled
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_source != null && this.m_source.CanBeCanceled;
            }
        }
        /// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that is signaled when the token is canceled.</summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is signaled when the token is canceled.</returns>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public WaitHandle WaitHandle
        {
            [__DynamicallyInvokable]
            get
            {
                if (this.m_source == null)
                {
                    this.InitializeDefaultSource();
                }
                return this.m_source.WaitHandle;
            }
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal CancellationToken(CancellationTokenSource source)
        {
            this.m_source = source;
        }
        /// <summary>Initializes the <see cref="T:System.Threading.CancellationToken" />.</summary>
        /// <param name="canceled">The canceled state for the token.</param>
        [__DynamicallyInvokable]
        public CancellationToken(bool canceled)
        {
            this = default(CancellationToken);
            if (canceled)
            {
                this.m_source = CancellationTokenSource.InternalGetStaticSource(canceled);
            }
        }
        /// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
        /// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
        /// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="callback" /> is null.</exception>
        [__DynamicallyInvokable]
        public CancellationTokenRegistration Register(Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(CancellationToken.s_ActionToActionObjShunt, callback, false, true);
        }
        /// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
        /// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
        /// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
        /// <param name="useSynchronizationContext">A Boolean value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="callback" /> is null.</exception>
        [__DynamicallyInvokable]
        public CancellationTokenRegistration Register(Action callback, bool useSynchronizationContext)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(CancellationToken.s_ActionToActionObjShunt, callback, useSynchronizationContext, true);
        }
        /// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
        /// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
        /// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
        /// <param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="callback" /> is null.</exception>
        [__DynamicallyInvokable]
        public CancellationTokenRegistration Register(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return this.Register(callback, state, false, true);
        }
        /// <summary>Registers a delegate that will be called when this <see cref="T:System.Threading.CancellationToken" /> is canceled.</summary>
        /// <returns>The <see cref="T:System.Threading.CancellationTokenRegistration" /> instance that can be used to deregister the callback.</returns>
        /// <param name="callback">The delegate to be executed when the <see cref="T:System.Threading.CancellationToken" /> is canceled.</param>
        /// <param name="state">The state to pass to the <paramref name="callback" /> when the delegate is invoked. This may be null.</param>
        /// <param name="useSynchronizationContext">A Boolean value that indicates whether to capture the current <see cref="T:System.Threading.SynchronizationContext" /> and use it when invoking the <paramref name="callback" />.</param>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="callback" /> is null.</exception>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext)
        {
            return this.Register(callback, state, useSynchronizationContext, true);
        }
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CancellationTokenRegistration InternalRegisterWithoutEC(Action<object> callback, object state)
        {
            return this.Register(callback, state, false, false);
        }
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private CancellationTokenRegistration Register(Action<object> callback, object state, bool useSynchronizationContext, bool useExecutionContext)
        {
            StackCrawlMark stackCrawlMark = StackCrawlMark.LookForMyCaller;
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (!this.CanBeCanceled)
            {
                return default(CancellationTokenRegistration);
            }
            SynchronizationContext targetSyncContext = null;
            ExecutionContext executionContext = null;
            if (!this.IsCancellationRequested)
            {
                if (useSynchronizationContext)
                {
                    targetSyncContext = SynchronizationContext.Current;
                }
                if (useExecutionContext)
                {
                    //executionContext = ExecutionContext.Capture(ref stackCrawlMark, ExecutionContext.CaptureOptions.OptimizeDefaultCase);
                    executionContext = ExecutionContext.Capture();
                }
            }
            return this.m_source.InternalRegister(callback, state, targetSyncContext, executionContext);
        }
        /// <summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified token.</summary>
        /// <returns>True if the instances are equal; otherwise, false. Two tokens are equal if they are associated with the same <see cref="T:System.Threading.CancellationTokenSource" /> or if they were both constructed from public CancellationToken constructors and their <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> values are equal.</returns>
        /// <param name="other">The other <see cref="T:System.Threading.CancellationToken" /> to which to compare this instance.</param>
        [__DynamicallyInvokable]
        public bool Equals(CancellationToken other)
        {
            if (this.m_source == null && other.m_source == null)
            {
                return true;
            }
            if (this.m_source == null)
            {
                return other.m_source == CancellationTokenSource.InternalGetStaticSource(false);
            }
            if (other.m_source == null)
            {
                return this.m_source == CancellationTokenSource.InternalGetStaticSource(false);
            }
            return this.m_source == other.m_source;
        }
        /// <summary>Determines whether the current <see cref="T:System.Threading.CancellationToken" /> instance is equal to the specified <see cref="T:System.Object" />.</summary>
        /// <returns>True if <paramref name="other" /> is a <see cref="T:System.Threading.CancellationToken" /> and if the two instances are equal; otherwise, false. Two tokens are equal if they are associated with the same <see cref="T:System.Threading.CancellationTokenSource" /> or if they were both constructed from public CancellationToken constructors and their <see cref="P:System.Threading.CancellationToken.IsCancellationRequested" /> values are equal.</returns>
        /// <param name="other">The other object to which to compare this instance.</param>
        /// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public override bool Equals(object other)
        {
            return other is CancellationToken && this.Equals((CancellationToken)other);
        }
        /// <summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationToken" />.</summary>
        /// <returns>A hash code for the current <see cref="T:System.Threading.CancellationToken" /> instance.</returns>
        [__DynamicallyInvokable]
        public override int GetHashCode()
        {
            if (this.m_source == null)
            {
                return CancellationTokenSource.InternalGetStaticSource(false).GetHashCode();
            }
            return this.m_source.GetHashCode();
        }
        /// <summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are equal.</summary>
        /// <returns>True if the instances are equal; otherwise, false.</returns>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public static bool operator ==(CancellationToken left, CancellationToken right)
        {
            return left.Equals(right);
        }
        /// <summary>Determines whether two <see cref="T:System.Threading.CancellationToken" /> instances are not equal.</summary>
        /// <returns>True if the instances are not equal; otherwise, false.</returns>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <exception cref="T:System.ObjectDisposedException">An associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        [__DynamicallyInvokable]
        public static bool operator !=(CancellationToken left, CancellationToken right)
        {
            return !left.Equals(right);
        }
        /// <summary>Throws a <see cref="T:System.OperationCanceledException" /> if this token has had cancellation requested.</summary>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The associated <see cref="T:System.Threading.CancellationTokenSource" /> has been disposed.</exception>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void ThrowIfCancellationRequested()
        {
            if (this.IsCancellationRequested)
            {
                this.ThrowOperationCanceledException();
            }
        }
        internal void ThrowIfSourceDisposed()
        {
            if (this.m_source != null && this.m_source.IsDisposed)
            {
                CancellationToken.ThrowObjectDisposedException();
            }
        }
        private void ThrowOperationCanceledException()
        {
            throw new OperationCanceledException("OperationCanceled");
        }
        private static void ActionToActionObjShunt(object obj)
        {
            Action action = obj as Action;
            action();
        }
        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(null, "CancellationToken_SourceDisposed");
        }
        private void InitializeDefaultSource()
        {
            this.m_source = CancellationTokenSource.InternalGetStaticSource(false);
        }
    }
}

#endif