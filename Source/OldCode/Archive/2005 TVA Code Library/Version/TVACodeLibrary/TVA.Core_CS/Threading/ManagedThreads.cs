using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Text;
//using TVA.DateTime.Common;


namespace TVA
{
	namespace Threading
	{
		
		/// <summary>
		/// Maintains a reference to all managed threads
		/// </summary>
		public class ManagedThreads
		{
			
			
			private static LinkedList<ManagedThread> m_queuedThreads;
			private static LinkedList<ManagedThread> m_activeThreads;
			
			static ManagedThreads()
			{
				
				m_queuedThreads = new LinkedList<ManagedThread>;
				m_activeThreads = new LinkedList<ManagedThread>;
				
			}
			
			/// <summary>
			/// Add an item to the active thread list
			/// </summary>
			/// <remarks>
			/// Typically only used by standard threads when user calls "Start"
			/// </remarks>
			internal static void Add(ManagedThread item)
			{
				
				// Standard threads are simply added to the active thread list when started
				lock(m_queuedThreads)
				{
					item.Status = ThreadStatus.Started;
					m_activeThreads.AddLast(item);
				}
				
			}
			
			/// <summary>
			/// Remove completed thread from active thread list
			/// </summary>
			internal static void Remove(ManagedThread item)
			{
				
				lock(m_queuedThreads)
				{
					m_activeThreads.Remove(item);
				}
				
			}
			
			/// <summary>
			/// Queue thread for processing
			/// </summary>
			/// <remarks>
			/// Typically only used by queued threads to add work items to the queue
			/// </remarks>
			internal static void Queue(ManagedThread item)
			{
				
				lock(m_queuedThreads)
				{
					m_queuedThreads.AddLast(item);
				}
				
			}
			
			/// <summary>
			/// Removes first item from the queue and transfers the item the active thread list
			/// </summary>
			/// <returns>Next item to be processed</returns>
			internal static ManagedThread Pop()
			{
				
				ManagedThread item;
				
				// Transfer next queued thread to the active thread list
				lock(m_queuedThreads)
				{
					if (m_queuedThreads.Count > 0)
					{
						item = m_queuedThreads.First.Value;
						m_queuedThreads.RemoveFirst();
					}
					
					if (item != null)
					{
						// Capture current thread (this is owned by .NET ThreadPool)
						item.Thread = Thread.CurrentThread;
						item.Status = ThreadStatus.Started;
						m_activeThreads.AddLast(item);
					}
				}
				
				return item;
				
			}
			
			/// <summary>
			/// Returns a descriptive status of all queued and active mananged threads
			/// </summary>
			public static string ActiveThreadStatus
			{
				get
				{
					System.Text.StringBuilder with_1 = new StringBuilder;
					ManagedThread[] items = QueuedThreads;
					int index;
					
					// Managed Thread Count: 1
					//
					// Thread 1 - Completed in 25 seconds
					//      Type: Standard Thread
					//      Name: TVASPDC.Service.CalculatedMeasurementInitialization.Initialize()
					
					with_1.AppendFormat("Managed Thread Count: {0}{1}", items.Length, Environment.NewLine);
					with_1.AppendLine();
					
					foreach (ManagedThread item in items)
					{
						index++;
						
						with_1.AppendFormat("Thread {0} - {1}{2}", index, ThreadStatusText(item), Environment.NewLine);
						
						with_1.AppendFormat("     Type: {0}{1}", @Enum.GetName(typeof(ThreadType), item.Type), Environment.NewLine);
						
						with_1.AppendFormat("     Name: {0}{1}", item.Name, Environment.NewLine);
						with_1.AppendLine();
					}
					
					return with_1.ToString();
				}
			}
			
			private static string ThreadStatusText(ManagedThread item)
			{
				string runtime = TVA.DateTime.Common.SecondsToText(item.RunTime);
				
				switch (item.Status)
				{
					case ThreadStatus.Unstarted:
						return "Not Started";
					case ThreadStatus.Queued:
						return "Queued";
					case ThreadStatus.Executing:
						return "Executing for " + runtime;
					case ThreadStatus.Completed:
						return "Completed in " + runtime;
					case ThreadStatus.Aborted:
						return "Aborted, ran for " + runtime;
					default:
						return "Status Unknown";
				}
			}
			
			/// <summary>
			/// Returns a copy of the currently queued and active threads
			/// </summary>
			public static ManagedThread[] QueuedThreads
			{
				get
				{
					List with_1 = new List<ManagedThread>;
					lock(m_queuedThreads)
					{
						with_1.AddRange(m_queuedThreads);
						with_1.AddRange(m_activeThreads);
					}
					
					return with_1.ToArray();
				}
			}
			
			/// <summary>
			/// Removes a queued thread from thread pool if still queued, if allowAbort is True
			/// aborts the thread if executing (standard or queued)
			/// </summary>
			/// <param name="item">Thread to cancel</param>
			/// <param name="allowAbort">Set to True to abort thread if executing</param>
			/// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted.</param>
			public static void Cancel(ManagedThread item, bool allowAbort, object stateInfo)
			{
				
				if (item == null)
				{
					throw (new ArgumentNullException("item"));
				}
				
				LinkedListNode<ManagedThread> node;
				
				lock(m_queuedThreads)
				{
					// Change thread status to aborted
					item.Status = ThreadStatus.Aborted;
					
					// See if item is still queued for execution in thread pool
					node = m_queuedThreads.Find(item);
					
					// Handle abort or dequeue
					if (node == null)
					{
						if (allowAbort)
						{
							// Started items may be aborted, even if running in thread pool
							try
							{
								if (stateInfo == null)
								{
									item.Thread.Abort();
								}
								else
								{
									item.Thread.Abort(stateInfo);
								}
							}
							finally
							{
							}
						}
					}
					else
					{
						// Remove item from queue if queued thread has yet to start
						m_queuedThreads.Remove(node);
					}
				}
			}
			
		}
		
	}
	
}
