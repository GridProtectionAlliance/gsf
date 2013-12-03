#if MONO

using System;
namespace System.Runtime.ExceptionServices
{
    [__DynamicallyInvokable]
    public sealed class ExceptionDispatchInfo
    {
        private Exception m_Exception;
        private string m_remoteStackTrace;
        private object m_stackTrace;
        private object m_dynamicMethods;
        private UIntPtr m_IPForWatsonBuckets;
        private object m_WatsonBuckets;
        internal UIntPtr IPForWatsonBuckets
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_IPForWatsonBuckets;
            }
        }
        internal object WatsonBuckets
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_WatsonBuckets;
            }
        }
        internal object BinaryStackTraceArray
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_stackTrace;
            }
        }
        internal object DynamicMethodArray
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_dynamicMethods;
            }
        }
        internal string RemoteStackTrace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_remoteStackTrace;
            }
        }
        [__DynamicallyInvokable]
        public Exception SourceException
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_Exception;
            }
        }
        [__DynamicallyInvokable]
        public static ExceptionDispatchInfo Capture(Exception source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "ArgumentNull_Obj");
            }
            return new ExceptionDispatchInfo(source);
        }
        [__DynamicallyInvokable]
        public void Throw()
        {
            //this.m_Exception.RestoreExceptionDispatchInfo(this);
            throw this.m_Exception;
        }
        private ExceptionDispatchInfo(Exception exception)
        {
            this.m_Exception = exception;
            //this.m_remoteStackTrace = exception.RemoteStackTrace;
            //object stackTrace;
            //object dynamicMethods;
            //this.m_Exception.GetStackTracesDeepCopy(out stackTrace, out dynamicMethods);
            //this.m_stackTrace = stackTrace;
            //this.m_dynamicMethods = dynamicMethods;
            //this.m_IPForWatsonBuckets = exception.IPForWatsonBuckets;
            //this.m_WatsonBuckets = exception.WatsonBuckets;
        }
    }
}

#endif