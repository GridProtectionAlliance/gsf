//*******************************************************************************************************
//  TVA.Threading.ThreadBase.vb - Convienent base class for new threads
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
    // This is a convienent base class for new threads - deriving your own thread class from from this
    // class allows you to define properties for the needed parameters of your thread proc
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