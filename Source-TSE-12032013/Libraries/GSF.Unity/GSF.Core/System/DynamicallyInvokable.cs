#if MONO

using System;

namespace System
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class __DynamicallyInvokableAttribute : Attribute
    {
        public __DynamicallyInvokableAttribute()
        {
        }
    }
}

#endif