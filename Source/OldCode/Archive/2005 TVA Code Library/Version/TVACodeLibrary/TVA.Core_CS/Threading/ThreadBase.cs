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
		
		// This is a convienent base class for new threads - deriving your own thread class from from this
		// class ensures your thread will properly terminate when your object is ready to be garbage
		// collected and allows you to define properties for the needed parameters of your thread proc
		public abstract class ThreadBase : IDisposable
		{
			
			
			
			#if ThreadTracking
			protected ManagedThread WorkerThread;
			#else
			protected Thread WorkerThread;
			#endif
			
			~ThreadBase()
			{
				
				Abort();
				
			}
			
			public virtual void Start()
			{
				
				#if ThreadTracking
				WorkerThread = new ManagedThread(ThreadExec);
				WorkerThread.Name = "TVA.Threading.ThreadBase.ThreadExec() [" + this.GetType().Name + "]";
				#else
				WorkerThread = new Thread(new System.Threading.ThreadStart(ThreadExec));
				#endif
				WorkerThread.Start();
				
			}
			
			public void Dispose()
			{
				this.Abort();
			}
			
			public void Abort()
			{
				
				GC.SuppressFinalize(this);
				
				if (WorkerThread != null)
				{
					try
					{
						if (WorkerThread.IsAlive)
						{
							WorkerThread.Abort();
							ThreadStopped();
						}
					}
					catch
					{
					}
					WorkerThread = null;
				}
				
			}
			
			#if ThreadTracking
			public ManagedThread Thread
			{
				#else
				public Thread Thread
				{
					#endif
					
}
					return WorkerThread;
				}
			}
			
			private void ThreadExec()
			{
				
				ThreadStarted();
				ThreadProc();
				ThreadStopped();
				
			}
			
			protected virtual void ThreadStarted()
			{
			}
			
			protected abstract void ThreadProc();
			
			protected virtual void ThreadStopped()
			{
			}
			
		}
		
	}
}
