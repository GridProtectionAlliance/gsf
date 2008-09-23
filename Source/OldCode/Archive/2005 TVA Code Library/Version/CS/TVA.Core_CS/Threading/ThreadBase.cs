//*******************************************************************************************************
//  ThreadBase.cs
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
//  04/13/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/11/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Threading;

namespace TVA.Threading
{
    /// <summary>
    /// This is a convienent base class for new threads - deriving your own thread class from from this
    /// class allows you to define properties for the needed parameters of your thread proc.
    /// </summary>
    public abstract class ThreadBase : IDisposable
    {
#if ThreadTracking
        protected ManagedThread baseThread;
#else
		protected Thread baseThread;
#endif

        ~ThreadBase()
        {
            Abort();
        }

        public virtual void Start()
        {
#if ThreadTracking
            baseThread = new ManagedThread(ThreadExec);
            baseThread.Name = "TVA.Threading.ThreadBase.ThreadExec() [" + this.GetType().Name + "]";
#else
			baseThread = new Thread(ThreadExec);
#endif
            baseThread.Start();
        }

        public void Dispose()
        {
            this.Abort();
        }

        public void Abort()
        {
            if (baseThread != null)
            {
                try
                {
                    if (baseThread.IsAlive)
                    {
                        baseThread.Abort();
                        ThreadStopped();
                    }
                }
                catch
                {
                }
                baseThread = null;
            }

            GC.SuppressFinalize(this);
        }

#if ThreadTracking
        public ManagedThread Thread
#else
		public Thread Thread
#endif
        {
            get
            {
                return baseThread;
            }
        }

        private void ThreadExec()
        {
            ThreadStarted();
            ThreadProc();
            ThreadStopped();
        }

        protected virtual void ThreadStarted() { }

        protected abstract void ThreadProc();

        protected virtual void ThreadStopped() { }
    }
}