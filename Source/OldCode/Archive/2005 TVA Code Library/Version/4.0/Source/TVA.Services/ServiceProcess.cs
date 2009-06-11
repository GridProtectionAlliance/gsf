//*******************************************************************************************************
//  ServiceProcess.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/13/2006 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//  03/03/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Threading;
using TVA.Units;

namespace TVA.Services
{
    /// <summary>
    /// Indicates the current state of <see cref="ServiceProcess"/>.
    /// </summary>
    public enum ServiceProcessState
    {
        /// <summary>
        /// <see cref="ServiceProcess"/> has not been started.
        /// </summary>
        Unprocessed,
        /// <summary>
        /// <see cref="ServiceProcess"/> is currently executing.
        /// </summary>
        Processing,
        /// <summary>
        /// <see cref="ServiceProcess"/> has completed processing.
        /// </summary>
        Processed,
        /// <summary>
        /// <see cref="ServiceProcess"/> was aborted.
        /// </summary>
        Aborted,
        /// <summary>
        /// <see cref="ServiceProcess"/> stopped due to exception.
        /// </summary>
        Exception
    }

    /// <summary>
    /// Represents a process that executes asynchronously inside a <see cref="ServiceHelper"/>.
    /// </summary>
    /// <seealso cref="ServiceHelper"/>
    public class ServiceProcess : IDisposable, IProvideStatus
	{
        #region [ Members ]

        //Events

        /// <summary>
        /// Occurs when the <see cref="CurrentState"/> of the <see cref="ServiceProcess"/> changes.
        /// </summary>
        public event EventHandler StateChanged;

        // Fields
#if ThreadTracking
        private ManagedThread m_processThread;
#else
		private Thread m_processThread;
#endif
        private string m_name;
        private object[] m_arguments;
        private Action<string, object[]> m_executionMethod;
        private ServiceProcessState m_currentState;
        private DateTime m_executionStartTime;
        private DateTime m_executionStopTime;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProcess"/> class.
        /// </summary>
        /// <param name="executionMethod"><see cref="Delegate"/> that gets invoked when <see cref="Start()"/> is called.</param>
        /// <param name="name">Name of the <see cref="ServiceProcess"/>.</param>
        /// <param name="arguments">Arguments to be passed in to the <paramref name="executionMethod"/>.</param>
        public ServiceProcess(Action<string, object[]> executionMethod, string name, params object[] arguments)
        {
            m_name = name;
            m_arguments = arguments;
            m_executionMethod = executionMethod;
            m_currentState = ServiceProcessState.Unprocessed;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ServiceProcess" /> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ServiceProcess()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="ServiceProcess"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is a null or empty string.</exception>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("Name");

                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the arguments to be passed in to the <see cref="ExecutionMethod"/>.
        /// </summary>
        public object[] Arguments
        {
            get
            {
                return m_arguments;
            }
            set
            {
                m_arguments = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Delegate"/> that gets invoked when <see cref="Start()"/> is called.
        /// </summary>
        /// <remarks>
        /// Argument1 gets the <see cref="Name"/> of the <see cref="ServiceProcess"/>.<br/>
        /// Argument2 gets the <see cref="Arguments"/> of the <see cref="ServiceProcess"/>.
        /// </remarks>
        public Action<string, object[]> ExecutionMethod
        {
            get
            {
                return m_executionMethod;
            }
            set
            {
                m_executionMethod = value;
            }
        }

        /// <summary>
        /// Gets the current <see cref="ServiceProcessState"/>.
        /// </summary>
        public ServiceProcessState CurrentState
        {
            get
            {
                return m_currentState;
            }
            private set
            {
                m_currentState = value;
                OnStateChanged();       // Notify of the change in state.
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when execution of <see cref="ServiceProcess"/> last started.
        /// </summary>
        public DateTime ExecutionStartTime
        {
            get
            {
                return m_executionStartTime;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when execution of <see cref="ServiceProcess"/> last completed.
        /// </summary>
        public DateTime ExecutionStopTime
        {
            get
            {
                return m_executionStopTime;
            }
        }

        /// <summary>
        /// Gets the <see cref="Time"/> taken by the <see cref="ServiceProcess"/> during the last execution.
        /// </summary>
        public Time LastExecutionTime
        {
            get
            {
                return Ticks.ToSeconds(m_executionStopTime.Ticks - m_executionStartTime.Ticks);
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="ServiceProcess"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("              Process Name: ");
                status.Append(m_name);
                status.AppendLine();
                status.Append("             Current State: ");
                status.Append(m_currentState.ToString());
                status.AppendLine();
                status.Append("      Execution Start Time: ");

                if (m_executionStartTime != DateTime.MinValue)
                {
                    status.Append(m_executionStartTime.ToString());
                }
                else
                {
                    status.Append("N/A");
                }

                status.AppendLine();
                status.Append("       Execution Stop Time: ");

                if (m_executionStopTime != DateTime.MinValue)
                {
                    status.Append(m_executionStopTime.ToString());
                }
                else
                {
                    status.Append("N/A");
                }

                status.AppendLine();
                status.Append("       Last Execution Time: ");
                status.Append(LastExecutionTime.ToString());
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Starts the execution of <see cref="ServiceProcess"/>.
        /// </summary>
        public void Start()
        {
            Start(m_arguments);
        }

        /// <summary>
        /// Starts the execution of <see cref="ServiceProcess"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed in to the <see cref="ExecutionMethod"/>.</param>
        public void Start(object[] arguments)
        {
            // Start the execution on a seperate thread.
#if ThreadTracking
            m_processThread = new ManagedThread(InvokeExecutionMethod);
            m_processThread.Name = "TVA.Services.ServiceProcess.InvokeExecutionMethod() [" + m_name + "]";
#else
			m_processThread = new Thread(InvokeExecutionMethod);
#endif
            m_processThread.Start(arguments);
        }

        /// <summary>
        /// Stops the execution of <see cref="ServiceProcess"/> if it executing.
        /// </summary>
        public void Abort()
        {
            // We'll abort the process only if it is currently executing.
            if (m_processThread != null)
            {
                if (m_processThread.IsAlive)
                    m_processThread.Abort();
            }
            m_processThread = null;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="ServiceProcess"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceProcess"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    Abort();
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        m_executionMethod = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="StateChanged"/> event.
        /// </summary>
        protected virtual void OnStateChanged()
        {
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
        }

        private void InvokeExecutionMethod(object state)
        {
            if (m_executionMethod != null)
            {
                CurrentState = ServiceProcessState.Processing;
                m_executionStartTime = DateTime.Now;
                m_executionStopTime = DateTime.MinValue;

                try
                {
                    // We'll keep the invocation of the delegate in Try...Catch to absorb any exceptions that
                    // were not handled by the consumer.
                    m_executionMethod(m_name, state as object[]);

                    CurrentState = ServiceProcessState.Processed;
                }
                catch (ThreadAbortException)
                {
                    CurrentState = ServiceProcessState.Aborted;
                }
                catch (Exception)
                {
                    // We'll absorb any exceptions if unhandled by the client.
                    CurrentState = ServiceProcessState.Exception;
                }
                finally
                {
                    m_executionStopTime = DateTime.Now;
                }
            }
            m_processThread = null;
        }

        #endregion
	}
}
