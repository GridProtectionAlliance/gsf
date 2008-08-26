using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Reflection;

// James Ritchie Carroll - 2005


namespace TVA
{
	namespace Threading
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
			
			public static void ExecuteMethod(object Instance, string MethodName, params object[] Params)
			{
				
				Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static void ExecuteNonPublicMethod(object Instance, string MethodName, params object[] Params)
			{
				
				Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod || BindingFlags.Instance || BindingFlags.NonPublic, @Params);
				
			}
			
			public static void ExecuteSharedMethod(Type ObjectType, string MethodName, params object[] Params)
			{
				
				Execute(ObjectType, null, MethodName, BindingFlags.InvokeMethod || BindingFlags.Static || BindingFlags.Public, @Params);
				
			}
			
			public static void ExecuteNonPublicSharedMethod(Type ObjectType, string MethodName, params object[] Params)
			{
				
				Execute(ObjectType, null, MethodName, BindingFlags.InvokeMethod || BindingFlags.Static || BindingFlags.NonPublic, @Params);
				
			}
			
			public static void ExecutePropertyGet(object Instance, string MethodName, params object[] Params)
			{
				
				Execute(Instance.GetType(), Instance, MethodName, BindingFlags.GetProperty || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static void ExecutePropertySet(object Instance, string MethodName, params object[] Params)
			{
				
				Execute(Instance.GetType(), Instance, MethodName, BindingFlags.SetProperty || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static void Execute(Type ObjectType, object Instance, string MethodName, BindingFlags InvokeAttributes, params object[] Params)
			{
				
				QueueThread qt = new QueueThread();
				
				qt.ObjectType = ObjectType;
				qt.Instance = Instance;
				qt.MethodName = MethodName;
				qt.InvokeAttributes = InvokeAttributes || BindingFlags.IgnoreCase;
				qt.Parameters = new object[@Params.Length];
				@Params.CopyTo(qt.Parameters, 0);
				
				#if ThreadTracking
				ManagedThread with_2 = ManagedThreadPool.QueueUserWorkItem(new TVA.Threading.ManagedThreadPool.ParameterizedThreadStart(ThreadProc), qt);
				with_2.Name = "TVA.Threading.QueueThread.ThreadProc()";
				#else
				ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ThreadProc), qt);
				#endif
				
			}
			
			protected static void ThreadProc(object stateInfo)
			{
				
				// Invoke user method
				QueueThread with_1 = ((QueueThread) stateInfo);
				with_1.ObjectType.InvokeMember(with_1.MethodName, with_1.InvokeAttributes, null, with_1.Instance, with_1.Parameters);
				
			}
			
		}
		
	}
}
