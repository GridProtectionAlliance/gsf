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
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using TVA.Threading;

namespace TVA.Services
{
    public delegate void ProcessExecutionMethod(string name, object[] parameters);

    public class ServiceProcess : IDisposable
	{
        #region [ Members ]

        // Fields
#if ThreadTracking
        private ManagedThread m_processThread;
#else
		private Thread m_processThread;
#endif
        private string m_name;
        private object[] m_parameters;
        private ProcessExecutionMethod m_executionMethod;
        private ServiceHelper m_serviceHelper;
        private ProcessState m_currentState;
        private DateTime m_executionStartTime;
        private DateTime m_executionStopTime;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public ServiceProcess(ProcessExecutionMethod executionMethod, string name, ServiceHelper serviceHelper)
            : this(executionMethod, name, null, serviceHelper)
        {
        }

        public ServiceProcess(ProcessExecutionMethod executionMethod, string name, object[] parameters, ServiceHelper serviceHelper)
        {
            m_name = name;
            m_parameters = parameters;
            m_executionMethod = executionMethod;
            m_serviceHelper = serviceHelper;
            m_currentState = ProcessState.Unprocessed;
        }

        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="ServiceProcess" /> class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual <see cref="Dispose(bool)" /> method, passing in <strong>false</strong>.
        /// </remarks>
        ~ServiceProcess()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_name = value;
                else
                    throw new ArgumentException("Name cannot be null.");
            }
        }

        public object[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = value;
            }
        }

        public ProcessExecutionMethod ExecutionMethod
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

        public ServiceHelper ServiceHelper
        {
            get
            {
                return m_serviceHelper;
            }
            set
            {
                m_serviceHelper = value;
            }
        }

        public ProcessState CurrentState
        {
            get
            {
                return m_currentState;
            }
            private set
            {
                m_currentState = value;
                m_serviceHelper.ProcessStateChanged(m_name, m_currentState);
            }
        }

        public DateTime ExecutionStartTime
        {
            get
            {
                return m_executionStartTime;
            }
        }

        public DateTime ExecutionStopTime
        {
            get
            {
                return m_executionStopTime;
            }
        }

        public double LastExecutionTime
        {
            get
            {
                return Ticks.ToSeconds(m_executionStopTime.Ticks - m_executionStartTime.Ticks);
            }
        }

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
                status.Append(Seconds.ToText(LastExecutionTime));
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all resources used by an instance of the <see cref="ServiceProcess" /> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)" /> method, passing in <strong>true</strong>, and then suppresses 
        /// finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="ServiceProcess" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        Abort();
                        m_executionMethod = null;
                        m_serviceHelper = null;
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        public void Start()
        {
            // Start the execution on a seperate thread.
#if ThreadTracking
            m_processThread = new ManagedThread(InvokeExecutionMethod);
            m_processThread.Name = "TVA.Services.ServiceProcess.InvokeExecutionMethod() [" + m_name + "]";
#else
			m_processThread = new Thread(InvokeExecutionMethod);
#endif
            m_processThread.Start();
        }

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

        private void InvokeExecutionMethod()
        {
            if (m_executionMethod != null)
            {
                CurrentState = ProcessState.Processing;
                m_executionStartTime = DateTime.Now;
                m_executionStopTime = DateTime.MinValue;

                try
                {
                    // We'll keep the invocation of the delegate in Try...Catch to absorb any exceptions that
                    // were not handled by the consumer.
                    m_executionMethod(m_name, m_parameters);

                    CurrentState = ProcessState.Processed;
                }
                catch (ThreadAbortException)
                {
                    CurrentState = ProcessState.Aborted;
                }
                catch (Exception)
                {
                    // We'll absorb any exceptions if unhandled by the client.
                    CurrentState = ProcessState.Exception;
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
