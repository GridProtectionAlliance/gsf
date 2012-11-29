#if MONO

using System;
using System.Runtime;
namespace System.Diagnostics.Tracing
{
    [__DynamicallyInvokable, AttributeUsage(AttributeTargets.Class)]
    public sealed class EventSourceAttribute : Attribute
    {
        [__DynamicallyInvokable]
        public string Name
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get;
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set;
        }
        [__DynamicallyInvokable]
        public string Guid
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get;
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set;
        }
        [__DynamicallyInvokable]
        public string LocalizationResources
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get;
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set;
        }
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventSourceAttribute()
        {
        }
    }
}

#endif