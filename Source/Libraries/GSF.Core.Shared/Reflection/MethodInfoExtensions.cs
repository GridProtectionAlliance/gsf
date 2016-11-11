//******************************************************************************************************
//  MethodInfoExtensionsCompile.cs - Gbtc
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
//  11/03/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable AssignNullToNotNullAttribute
namespace GSF.Reflection
{
    /// <summary>
    /// Defines extensions methods related to <see cref="MethodInfo"/>.
    /// </summary>
    /// <remarks>
    /// Many of these functions help generate compiled IL code that can execute a method of a class.
    /// </remarks>
    public static class MethodInfoExtensions
    {
        private static readonly Dictionary<MethodInfo, object> s_compiledCallbacks = new Dictionary<MethodInfo, object>();

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into an <see cref="Action"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object> CreateAction(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // void Fun(object obj)
                    // {
                    //    ((NativeType)obj.Method();
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method);
                    callback = Expression.Lambda<Action<object>>(methodCall, paramTargetObj).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Action<object>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into an <see cref="Action"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1> CreateAction<T1>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // void Fun(object obj ,T1 param1)
                    // {
                    //    ((NativeType)obj.Method(param1);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1);
                    callback = Expression.Lambda<Action<object, T1>>(methodCall, paramTargetObj, paramT1).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Action<object, T1>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into an <see cref="Action"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1, T2> CreateAction<T1, T2>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // void Fun(object obj,T1 param1, T2 param2)
                    // {
                    //    ((NativeType)obj.Method(param1, param2);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2);
                    callback = Expression.Lambda<Action<object, T1, T2>>(methodCall, paramTargetObj, paramT1, paramT2).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Action<object, T1, T2>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into an <see cref="Action"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1, T2, T3> CreateAction<T1, T2, T3>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // void Fun(object obj,T1 param1, T2 param2, T3 param3)
                    // {
                    //    ((NativeType)obj.Method(param1, param2, T3 param3);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                    ParameterExpression paramT3 = Expression.Parameter(typeof(T3));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2, paramT3);
                    callback = Expression.Lambda<Action<object, T1, T2, T3>>(methodCall, paramTargetObj, paramT1, paramT2, paramT3).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Action<object, T1, T2, T3>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into a <see cref="Func{T}"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Func<object, TResult> CreateFunc<TResult>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // TResult Fun(object obj, T1 param1)
                    // {
                    //   return ((NativeType)obj.Method(param1);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method);
                    callback = Expression.Lambda<Func<object, TResult>>(methodCall, paramTargetObj).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Func<object, TResult>)callback;

        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into a <see cref="Func{T}"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Func<object, T1, TResult> CreateFunc<T1, TResult>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // TResult Fun(object obj)
                    // {
                    //   return ((NativeType)obj.Method();
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1);
                    callback = Expression.Lambda<Func<object, T1, TResult>>(methodCall, paramTargetObj, paramT1).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Func<object, T1, TResult>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into a <see cref="Func{T}"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Func<object, T1, T2, TResult> CreateFunc<T1, T2, TResult>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // TResult Fun(object obj, T1 param1, T2 param2)
                    // {
                    //   return ((NativeType)obj.Method(param1, param2);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                    
                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2);
                    callback = Expression.Lambda<Func<object, T1, T2, TResult>>(methodCall, paramTargetObj, paramT1, paramT2).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Func<object, T1, T2, TResult>)callback;
        }

        /// <summary>
        /// Turns a <see cref="MethodInfo"/> into a <see cref="Func{T}"/> that can be called with objects of the specified type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Func<object, T1, T2, T3, TResult> CreateFunc<T1, T2, T3, TResult>(this MethodInfo method)
        {
            object callback;

            lock (s_compiledCallbacks)
            {
                if (!s_compiledCallbacks.TryGetValue(method, out callback))
                {
                    // Creates method
                    // TResult Fun(object obj, T1 param1, T2 param2, T3 param3)
                    // {
                    //   return ((NativeType)obj.Method(param1, param2, param3);
                    // }

                    // Parameters
                    ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                    ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                    ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                    ParameterExpression paramT3 = Expression.Parameter(typeof(T3));
                    Expression targetType = null;

                    if (!method.IsStatic)
                        targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);

                    // Call items
                    Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2, paramT3);
                    callback = Expression.Lambda<Func<object, T1, T2, T3, TResult>>(methodCall, paramTargetObj, paramT1, paramT2, paramT3).Compile();

                    s_compiledCallbacks.Add(method, callback);
                }
            }

            return (Func<object, T1, T2, T3, TResult>)callback;
        }
    }
}