using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Reflection;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Threading
	{
		
		// This class uses reflection to invoke an existing sub or function on a new thread, its usage
		// can be as simple as RunThread.ExecuteMethod(Me, "MyMethod", "param1", "param2", True)
		public class RunThread : ThreadBase, IComparable
		{
			
			
			public delegate void ThreadCompleteEventHandler();
			private ThreadCompleteEventHandler ThreadCompleteEvent;
			
			public event ThreadCompleteEventHandler ThreadComplete
			{
				add
				{
					ThreadCompleteEvent = (ThreadCompleteEventHandler) System.Delegate.Combine(ThreadCompleteEvent, value);
				}
				remove
				{
					ThreadCompleteEvent = (ThreadCompleteEventHandler) System.Delegate.Remove(ThreadCompleteEvent, value);
				}
			}
			
			public delegate void ThreadExecErrorEventHandler(Exception ex);
			private ThreadExecErrorEventHandler ThreadExecErrorEvent;
			
			public event ThreadExecErrorEventHandler ThreadExecError
			{
				add
				{
					ThreadExecErrorEvent = (ThreadExecErrorEventHandler) System.Delegate.Combine(ThreadExecErrorEvent, value);
				}
				remove
				{
					ThreadExecErrorEvent = (ThreadExecErrorEventHandler) System.Delegate.Remove(ThreadExecErrorEvent, value);
				}
			}
			
			
			public Type ObjectType;
			public object Instance;
			public string MethodName;
			public object[] Parameters;
			public BindingFlags InvokeAttributes;
			public object ReturnValue;
			protected Guid ID;
			protected static ArrayList AllocatedThreads = new ArrayList();
			
			public RunThread()
			{
				ID = Guid.NewGuid();
				
				
				// Create a static reference to this thread
				lock(AllocatedThreads.SyncRoot)
				{
					AllocatedThreads.Add(this);
					AllocatedThreads.Sort();
				}
				
			}
			
			public static RunThread ExecuteMethod(object Instance, string MethodName, params object[] Params)
			{
				
				return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static RunThread ExecuteNonPublicMethod(object Instance, string MethodName, params object[] Params)
			{
				
				return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod || BindingFlags.Instance || BindingFlags.NonPublic, @Params);
				
			}
			
			public static RunThread ExecuteSharedMethod(Type ObjectType, string MethodName, params object[] Params)
			{
				
				return Execute(ObjectType, null, MethodName, BindingFlags.InvokeMethod || BindingFlags.Static || BindingFlags.Public, @Params);
				
			}
			
			public static RunThread ExecuteNonPublicSharedMethod(Type ObjectType, string MethodName, params object[] Params)
			{
				
				return Execute(ObjectType, null, MethodName, BindingFlags.InvokeMethod || BindingFlags.Static || BindingFlags.NonPublic, @Params);
				
			}
			
			public static RunThread ExecutePropertyGet(object Instance, string MethodName, params object[] Params)
			{
				
				return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.GetProperty || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static RunThread ExecutePropertySet(object Instance, string MethodName, params object[] Params)
			{
				
				return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.SetProperty || BindingFlags.Instance || BindingFlags.Public, @Params);
				
			}
			
			public static RunThread Execute(Type ObjectType, object Instance, string MethodName, BindingFlags InvokeAttributes, params object[] Params)
			{
				
				RunThread rt = new RunThread();
				
				rt.ObjectType = ObjectType;
				rt.Instance = Instance;
				rt.MethodName = MethodName;
				rt.InvokeAttributes = InvokeAttributes || BindingFlags.IgnoreCase;
				rt.Parameters = new object[@Params.Length];
				@Params.CopyTo(rt.Parameters, 0);
				rt.Start();
				
				return rt;
				
			}
			
			protected override void ThreadProc()
			{
				
				try
				{
					// Invoke user method
					ReturnValue = ObjectType.InvokeMember(MethodName, InvokeAttributes, null, Instance, Parameters);
				}
				catch (Exception ex)
				{
					if (ThreadExecErrorEvent != null)
						ThreadExecErrorEvent(ex);
				}
				
			}
			
			protected override void ThreadStopped()
			{
				
				// Remove the static reference to this thread
				lock(AllocatedThreads.SyncRoot)
				{
					int intIndex = AllocatedThreads.BinarySearch(this);
					if (intIndex >= 0)
					{
						AllocatedThreads.RemoveAt(intIndex);
					}
				}
				
				if (ThreadCompleteEvent != null)
					ThreadCompleteEvent();
				
			}
			
			public int CompareTo(object obj)
			{
				
				// Allocated threads are sorted by ID
				if (obj is RunThread)
				{
					return ID.CompareTo(((RunThread) obj).ID);
				}
				else
				{
					throw (new ArgumentException("RunThread can only be compared to other RunThreads"));
				}
				
			}
			
		}
		
	}
}
