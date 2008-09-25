//*******************************************************************************************************
//  RunThread.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/11/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Reflection;

namespace TVA.Threading
{
    /// <summary>
    /// Executes any existing sub or function (regardless of parameters or signature) on a new thread.
    /// </summary>
    /// <remarks>
    /// This class uses reflection to invoke an existing sub or function on a new thread. An instance of
    /// the RunThread class is returned so user can retrieve any return value and/or abort thread later.
    /// Method name invocation is case-insensitive.
    /// </remarks>
    /// <example>
    /// <code>RunThread.ExecuteMethod(Me, "MyMethod", "param1", "param2", True)</code>
    /// </example>
    public class RunThread : ThreadBase
    {
        public Type ObjectType;
        public object Instance;
        public string MethodName;
        public object[] Parameters;
        public BindingFlags InvokeAttributes;
        public object ReturnValue;

        private RunThread() {}

        public static RunThread ExecuteMethod(object instance, string methodName, params object[] parameters)
        {
            return Execute(instance.GetType(), instance, methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static RunThread ExecuteNonPublicMethod(object instance, string methodName, params object[] parameters)
        {
            return Execute(instance.GetType(), instance, methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, parameters);
        }

        public static RunThread ExecuteSharedMethod(Type objectType, string methodName, params object[] parameters)
        {
            return Execute(objectType, null, methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, parameters);
        }

        public static RunThread ExecuteNonPublicSharedMethod(Type objectType, string methodName, params object[] parameters)
        {
            return Execute(objectType, null, methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, parameters);
        }

        public static RunThread ExecutePropertyGet(object instance, string methodName, params object[] parameters)
        {
            return Execute(instance.GetType(), instance, methodName, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static RunThread ExecutePropertySet(object instance, string methodName, params object[] parameters)
        {
            return Execute(instance.GetType(), instance, methodName, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static RunThread Execute(Type objectType, object instance, string methodName, BindingFlags invokeAttributes, params object[] parameters)
        {
            RunThread runThread = new RunThread();

            runThread.ObjectType = objectType;
            runThread.Instance = instance;
            runThread.MethodName = methodName;
            runThread.InvokeAttributes = invokeAttributes | BindingFlags.IgnoreCase;
            runThread.Parameters = new object[parameters.Length];
            parameters.CopyTo(runThread.Parameters, 0);
            runThread.Start();

            return runThread;
        }

        protected override void ThreadProc()
        {
            // Invoke user method
            ReturnValue = ObjectType.InvokeMember(MethodName, InvokeAttributes, null, Instance, Parameters);
        }
    }
}