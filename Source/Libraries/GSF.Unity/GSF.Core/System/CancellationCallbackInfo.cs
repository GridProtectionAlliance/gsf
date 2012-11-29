#if MONO

using System;
using System.Runtime;
using System.Security;

namespace System.Threading
{
    internal class CancellationCallbackInfo
    {
        internal readonly Action<object> Callback;
        internal readonly object StateForCallback;
        internal readonly SynchronizationContext TargetSyncContext;
        internal readonly ExecutionContext TargetExecutionContext;
        internal readonly CancellationTokenSource CancellationTokenSource;
        [SecurityCritical]
        private static ContextCallback s_executionContextCallback;

        internal CancellationCallbackInfo(Action<object> callback, object stateForCallback, SynchronizationContext targetSyncContext, ExecutionContext targetExecutionContext, CancellationTokenSource cancellationTokenSource)
        {
            this.Callback = callback;
            this.StateForCallback = stateForCallback;
            this.TargetSyncContext = targetSyncContext;
            this.TargetExecutionContext = targetExecutionContext;
            this.CancellationTokenSource = cancellationTokenSource;
        }
        [SecuritySafeCritical]
        internal void ExecuteCallback()
        {
            if (this.TargetExecutionContext != null)
            {
                ContextCallback contextCallback = CancellationCallbackInfo.s_executionContextCallback;
                if (contextCallback == null)
                {
                    contextCallback = (CancellationCallbackInfo.s_executionContextCallback = new ContextCallback(CancellationCallbackInfo.ExecutionContextCallback));
                }
                ExecutionContext.Run(this.TargetExecutionContext, contextCallback, this);
                return;
            }
            CancellationCallbackInfo.ExecutionContextCallback(this);
        }
        [SecurityCritical]
        private static void ExecutionContextCallback(object obj)
        {
            CancellationCallbackInfo cancellationCallbackInfo = obj as CancellationCallbackInfo;
            cancellationCallbackInfo.Callback(cancellationCallbackInfo.StateForCallback);
        }
    }
}

#endif