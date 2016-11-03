//******************************************************************************************************
//  WeakAction.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  01/16/2013 - Steven E. Chisholm
//       Generated original version of source code. 
//  11/03/2016 - Steven E. Chisholm
//       Modified the class to not require a local reference be kept by compiling the method 
//       to prevent that data from being collected.
//
//******************************************************************************************************

using System;
using System.Runtime.CompilerServices;
using GSF.Reflection;

namespace GSF.Threading
{
    /// <summary>
    /// Provides a weak referenced <see cref="Action"/> delegate.
    /// </summary>
    /// <remarks>
    /// This class will store the information necessary so the callback
    /// object will have a weak reference to it. This information is compiled
    /// an can be quickly executed without the overhead of using reflection.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class WeakAction : WeakReference
    {
        private bool m_isStatic;
        private Action<object> m_compiledMethod;
        /// <summary>
        /// Creates a WeakAction.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public WeakAction(Action callback)
            : base(callback.Target)
        {
            m_isStatic = callback.Method.IsStatic;
            m_compiledMethod = callback.Method.CreateAction();
        }

        /// <summary>
        /// Attempts to invoke the delegate to a weak reference object.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryInvoke()
        {
            if (m_isStatic)
            {
                m_compiledMethod(null);
                return true;
            }

            object target = Target;

            if ((object)target == null)
                return false;

            m_compiledMethod(target);
            return true;
        }

        /// <summary>
        /// Clears <see cref="Action"/> callback target.
        /// </summary>
        public void Clear()
        {
            //Note, the race condition that exists here would simply cause
            //A static method exit anyway since Target is always null for static 
            //methods.
            m_isStatic = false;
            Target = null;
        }
    }

    /// <summary>
    /// Provides a weak referenced <see cref="Action"/> delegate.
    /// </summary>
    /// <remarks>
    /// This class will store the information necessary so the callback
    /// object will have a weak reference to it. This information is compiled
    /// an can be quickly executed without the overhead of using reflection.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class WeakAction<T> : WeakReference
    {
        private bool m_isStatic;
        private Action<object,T> m_compiledMethod;
        /// <summary>
        /// Creates a WeakAction.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public WeakAction(Action<T> callback)
            : base(callback.Target)
        {
            m_isStatic = callback.Method.IsStatic;
            m_compiledMethod = callback.Method.CreateAction<T>();
        }

        /// <summary>
        /// Attempts to invoke the delegate to a weak reference object.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryInvoke(T param1)
        {
            if (m_isStatic)
            {
                m_compiledMethod(null, param1);
                return true;
            }

            object target = Target;

            if ((object)target == null)
                return false;

            m_compiledMethod(target, param1);
            return true;
        }

        /// <summary>
        /// Clears <see cref="Action"/> callback target.
        /// </summary>
        public void Clear()
        {
            //Note, the race condition that exists here would simply cause
            //A static method exit anyway since Target is always null for static 
            //methods.
            m_isStatic = false;
            Target = null;
        }
    }

}