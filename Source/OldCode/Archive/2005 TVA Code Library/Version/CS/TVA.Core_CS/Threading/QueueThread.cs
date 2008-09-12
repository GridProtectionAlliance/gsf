//*******************************************************************************************************
//  TVA.Threading.RunThread.vb - Uses reflection to queue an existing sub or function on thread pool
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/04/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/11/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Threading;
using System.Reflection;

namespace TVA.Threading
{
    // This class uses reflection to invoke an existing sub or function on a new thread, its usage
    // can be as simple as QueueThread.ExecuteMethod(Me, "MyMethod", "param1", "param2", True)
    public class QueueThread
    {
        public Type ObjectType;
        public object Instance;
        public string MethodName;
        public object[] Parameters;
        public BindingFlags InvokeAttributes;

        private QueueThread() { }

        public static void ExecuteMethod(object instance, string methodName, params object[] parameters)
        {
            Execute(instance.GetType(), instance, methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static void ExecuteNonPublicMethod(object instance, string methodName, params object[] parameters)
        {
            Execute(instance.GetType(), instance, methodName, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, parameters);
        }

        public static void ExecuteSharedMethod(Type objectType, string methodName, params object[] parameters)
        {
            Execute(objectType, null, methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, parameters);
        }

        public static void ExecuteNonPublicSharedMethod(Type objectType, string methodName, params object[] parameters)
        {
            Execute(objectType, null, methodName, BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic, parameters);
        }

        public static void ExecutePropertyGet(object instance, string methodName, params object[] parameters)
        {
            Execute(instance.GetType(), instance, methodName, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static void ExecutePropertySet(object instance, string methodName, params object[] parameters)
        {
            Execute(instance.GetType(), instance, methodName, BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, parameters);
        }

        public static void Execute(Type objectType, object instance, string methodName, BindingFlags invokeAttributes, params object[] parameters)
        {
            QueueThread queuedThread = new QueueThread();

            queuedThread.ObjectType = objectType;
            queuedThread.Instance = instance;
            queuedThread.MethodName = methodName;
            queuedThread.InvokeAttributes = invokeAttributes | BindingFlags.IgnoreCase;
            queuedThread.Parameters = new object[parameters.Length];
            parameters.CopyTo(queuedThread.Parameters, 0);

#if ThreadTracking
            ManagedThread managedThread = ManagedThreadPool.QueueUserWorkItem(ThreadProc, queuedThread);
            managedThread.Name = "TVA.Threading.QueueThread.ThreadProc()";
#else
			ThreadPool.QueueUserWorkItem(ThreadProc, runThread);
#endif
        }

        protected static void ThreadProc(object stateInfo)
        {
            // Invoke user method
            QueueThread queuedThread = stateInfo as QueueThread;

            if (queuedThread != null)
                queuedThread.ObjectType.InvokeMember(queuedThread.MethodName, queuedThread.InvokeAttributes, null, queuedThread.Instance, queuedThread.Parameters);
        }
    }
}