using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;


namespace TVA
{
	namespace Threading
	{
		
		/// <summary>
		/// Defines a managed thread pool
		/// </summary>
		/// <remarks>
		/// This class works like the normal thread pool but provides the benefit of automatic tracking
		/// of queued threads through the ManagedThreads collection, returns a reference to the
		/// queued thread with the ability to dequeue and/or abort, total thread runtime and the
		/// ability to run the queued thread in an alternate execution context
		/// </remarks>
		public class ManagedThreadPool
		{
			
			
			/// <summary>
			/// Queues a work item for processing on the managed thread pool
			/// </summary>
			/// <param name="callback">A WaitCallback representing the method to execute.</param>
			/// <returns>Reference to queued thread</returns>
			/// <remarks>
			/// This differs from the normal thread pool QueueUserWorkItem function in that it does
			/// not return a success value determing if item was queued, but rather a reference to
			/// to the managed thread that was actually placed on the queue.
			/// </remarks>
			public static ManagedThread QueueUserWorkItem(ThreadStart callback)
			{
				
				if (callback == null)
				{
					throw (new ArgumentNullException("callback"));
				}
				
				ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, null, null);
				
				ManagedThreads.Queue(item);
				
				ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(HandleItem));
				
				return item;
				
			}
			
			/// <summary>
			/// Queues a work item for processing on the managed thread pool
			/// </summary>
			/// <param name="callback">A WaitCallback representing the method to execute.</param>
			/// <returns>Reference to queued thread</returns>
			/// <remarks>
			/// This differs from the normal thread pool QueueUserWorkItem function in that it does
			/// not return a success value determing if item was queued, but rather a reference to
			/// to the managed thread that was actually placed on the queue.
			/// </remarks>
			public static ManagedThread QueueUserWorkItem(ParameterizedThreadStart callback)
			{
				
				return QueueUserWorkItem(callback, null);
				
			}
			
			/// <summary>
			/// Queues a work item for processing on the managed thread pool
			/// </summary>
			/// <param name="callback">A WaitCallback representing the method to execute.</param>
			/// <param name="state">An object containing data to be used by the method.</param>
			/// <returns>Reference to queued thread</returns>
			/// <remarks>
			/// This differs from the normal thread pool QueueUserWorkItem function in that it does
			/// not return a success value determing if item was queued, but rather a reference to
			/// to the managed thread that was actually placed on the queue.
			/// </remarks>
			public static ManagedThread QueueUserWorkItem(ParameterizedThreadStart callback, object state)
			{
				
				if (callback == null)
				{
					throw (new ArgumentNullException("callback"));
				}
				
				ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, state, null);
				
				ManagedThreads.Queue(item);
				
				ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(HandleItem));
				
				return item;
				
			}
			
			/// <summary>
			/// Queues a work item for processing on the managed thread pool
			/// </summary>
			/// <param name="callback">A WaitCallback representing the method to execute.</param>
			/// <param name="ctx">Alternate execution context in which to run the thread.</param>
			/// <returns>Reference to queued thread</returns>
			/// <remarks>
			/// This differs from the normal thread pool QueueUserWorkItem function in that it does
			/// not return a success value determing if item was queued, but rather a reference to
			/// to the managed thread that was actually placed on the queue.
			/// </remarks>
			public static ManagedThread QueueUserWorkItem(ContextCallback callback, ExecutionContext ctx)
			{
				
				return QueueUserWorkItem(callback, null, ctx);
				
			}
			
			/// <summary>
			/// Queues a work item for processing on the managed thread pool
			/// </summary>
			/// <param name="callback">A WaitCallback representing the method to execute.</param>
			/// <param name="state">An object containing data to be used by the method.</param>
			/// <param name="ctx">Alternate execution context in which to run the thread.</param>
			/// <returns>Reference to queued thread</returns>
			/// <remarks>
			/// This differs from the normal thread pool QueueUserWorkItem function in that it does
			/// not return a success value determing if item was queued, but rather a reference to
			/// to the managed thread that was actually placed on the queue.
			/// </remarks>
			public static ManagedThread QueueUserWorkItem(ContextCallback callback, object state, ExecutionContext ctx)
			{
				
				if (callback == null)
				{
					throw (new ArgumentNullException("callback"));
				}
				
				ManagedThread item = new ManagedThread(ThreadType.QueuedThread, callback, state, ctx);
				
				ManagedThreads.Queue(item);
				
				ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(HandleItem));
				
				return item;
				
			}
			
			private static void HandleItem(object state)
			{
				
				// Get next queued item
				ManagedThread item = ManagedThreads.Pop();
				
				// Execute callback...
				if (item != null)
				{
					item.HandleItem();
				}
				
			}
			
		}
		
	}
	
}
