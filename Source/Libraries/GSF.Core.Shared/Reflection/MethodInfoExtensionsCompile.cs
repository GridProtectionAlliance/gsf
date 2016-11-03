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

namespace GSF.Reflection
{
    /// <summary>
    /// Generates compiled IL code that can execute a method of a class.
    /// </summary>
    public static class MethodInfoExtensionsCompile
    {
        /// <summary>
        /// Turns a MethodInfo into an Action that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object> CreateAction(this MethodInfo method)
        {
            return SubClass.CreateAction(method);
        }
        /// <summary>
        /// Turns a MethodInfo into an Action that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1> CreateAction<T1>(this MethodInfo method)
        {
            return SubClass.CreateAction<T1>(method);
        }
        /// <summary>
        /// Turns a MethodInfo into an Action that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1, T2> CreateAction<T1, T2>(this MethodInfo method)
        {
            return SubClass.CreateAction<T1, T2>(method);

        }
        /// <summary>
        /// Turns a MethodInfo into an Action that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        public static Action<object, T1, T2, T3> CreateAction<T1, T2, T3>(this MethodInfo method)
        {
            return SubClass.CreateAction<T1, T2, T3>(method);
        }

        #region [ MethodInfo to Func ]

        /// <summary>
        /// Turns a MethodInfo into an Func that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T")]
        public static Func<object, R1> CreateFunc<R1>(this MethodInfo method)
        {
            return SubClass.CreateFunc<R1>(method);
        }

        /// <summary>
        /// Turns a MethodInfo into an Func that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T")]
        public static Func<object, T1, R1> CreateFunc<T1, R1>(this MethodInfo method)
        {
            return SubClass.CreateFunc<T1, R1>(method);
        }

        /// <summary>
        /// Turns a MethodInfo into an Func that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T")]
        public static Func<object, T1, T2, R1> CreateFunc<T1, T2, R1>(this MethodInfo method)
        {
            return SubClass.CreateFunc<T1, T2, R1>(method);
        }

        /// <summary>
        /// Turns a MethodInfo into an Func that can be called with objects of the type. 
        /// </summary>
        /// <param name="method">the method that should be compiled.</param>
        /// <returns>The compiled method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId = "T")]
        public static Func<object, T1, T2, T3, R1> CreateFunc<T1, T2, T3, R1>(this MethodInfo method)
        {
            return SubClass.CreateFunc<T1, T2, T3, R1>(method);
        }

        #endregion

        /// <summary>
        /// Since extension methods cannot have member variables.
        /// </summary>
        private static class SubClass
        {
            private static readonly Dictionary<MethodInfo, object> CompiledCallbacks = new Dictionary<MethodInfo, object>();

            public static Action<object> CreateAction(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //void Fun(object obj)
                        //{
                        //   ((NativeType)obj.Method();
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }
                        //Call items
                        Expression methodCall = Expression.Call(targetType, method);
                        callback = Expression.Lambda<Action<object>>(methodCall, paramTargetObj).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Action<object>)callback;
            }

            public static Action<object, T1> CreateAction<T1>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //void Fun(object obj ,T1 param1)
                        //{
                        //   ((NativeType)obj.Method(param1);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }
                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1);
                        callback = Expression.Lambda<Action<object, T1>>(methodCall, paramTargetObj, paramT1).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Action<object, T1>)callback;
            }

            public static Action<object, T1, T2> CreateAction<T1, T2>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //void Fun(object obj,T1 param1, T2 param2)
                        //{
                        //   ((NativeType)obj.Method(param1, param2);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }
                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2);
                        callback = Expression.Lambda<Action<object, T1, T2>>(methodCall, paramTargetObj, paramT1, paramT2).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Action<object, T1, T2>)callback;
            }

            public static Action<object, T1, T2, T3> CreateAction<T1, T2, T3>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //void Fun(object obj,T1 param1, T2 param2, T3 param3)
                        //{
                        //   ((NativeType)obj.Method(param1, param2, T3 param3);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                        ParameterExpression paramT3 = Expression.Parameter(typeof(T3));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }
                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2, paramT3);
                        callback = Expression.Lambda<Action<object, T1, T2, T3>>(methodCall, paramTargetObj, paramT1, paramT2, paramT3).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Action<object, T1, T2, T3>)callback;
            }

            public static Func<object, R1> CreateFunc<R1>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //R1 Fun(object obj, T1 param1)
                        //{
                        //  return ((NativeType)obj.Method(param1);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }

                        //Call items
                        Expression methodCall = Expression.Call(targetType, method);
                        callback = Expression.Lambda<Func<object, R1>>(methodCall, paramTargetObj).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Func<object, R1>)callback;

            }

            public static Func<object, T1, R1> CreateFunc<T1, R1>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //R1 Fun(object obj)
                        //{
                        //  return ((NativeType)obj.Method();
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }

                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1);
                        callback = Expression.Lambda<Func<object, T1, R1>>(methodCall, paramTargetObj, paramT1).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Func<object, T1, R1>)callback;
            }

            public static Func<object, T1, T2, R1> CreateFunc<T1, T2, R1>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //R1 Fun(object obj, T1 param1, T2 param2)
                        //{
                        //  return ((NativeType)obj.Method(param1, param2);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }

                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2);
                        callback = Expression.Lambda<Func<object, T1, T2, R1>>(methodCall, paramTargetObj, paramT1, paramT2).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Func<object, T1, T2, R1>)callback;
            }

            public static Func<object, T1, T2, T3, R1> CreateFunc<T1, T2, T3, R1>(MethodInfo method)
            {
                object callback;

                lock (CompiledCallbacks)
                {
                    if (!CompiledCallbacks.TryGetValue(method, out callback))
                    {
                        //Creates method
                        //R1 Fun(object obj, T1 param1, T2 param2, T3 param3)
                        //{
                        //  return ((NativeType)obj.Method(param1, param2, param3);
                        //}

                        //Parameters
                        ParameterExpression paramTargetObj = Expression.Parameter(typeof(object));
                        ParameterExpression paramT1 = Expression.Parameter(typeof(T1));
                        ParameterExpression paramT2 = Expression.Parameter(typeof(T2));
                        ParameterExpression paramT3 = Expression.Parameter(typeof(T3));
                        Expression targetType = null;
                        if (!method.IsStatic)
                        {
                            targetType = Expression.TypeAs(paramTargetObj, method.DeclaringType);
                        }

                        //Call items
                        Expression methodCall = Expression.Call(targetType, method, paramT1, paramT2, paramT3);
                        callback = Expression.Lambda<Func<object, T1, T2, T3, R1>>(methodCall, paramTargetObj, paramT1, paramT2, paramT3).Compile();

                        CompiledCallbacks.Add(method, callback);
                    }
                }

                return (Func<object, T1, T2, T3, R1>)callback;
            }


        }
    }
}