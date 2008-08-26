using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
//using TVA.DateTime.Common;
using TVA.Threading;

// 09-13-06


namespace TVA.Services
{
	public class ServiceProcess
	{
		
		
		#if ThreadTracking
		private ManagedThread m_processThread;
		#else
		private Thread m_processThread;
		#endif
		private string m_name;
		private object[] m_parameters;
		private ExecutionMethodSignature m_executionMethod;
		private ServiceHelper m_serviceHelper;
		private ProcessState m_currentState;
		private DateTime m_executionStartTime;
		private DateTime m_executionStopTime;
		
		public delegate void ExecutionMethodSignature(string name, object[] parameters);
		
		public ServiceProcess(ExecutionMethodSignature executionMethod, string name, ServiceHelper serviceHelper) : this(executionMethod, name, null, serviceHelper)
		{
		}
		
		public ServiceProcess(ExecutionMethodSignature executionMethod, string name, object[] parameters, ServiceHelper serviceHelper)
		{
			m_name = name;
			m_parameters = parameters;
			m_executionMethod = executionMethod;
			m_serviceHelper = serviceHelper;
			m_currentState = processState.Unprocessed;
		}
		
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				if (! string.IsNullOrEmpty(value))
				{
					m_name = value;
				}
				else
				{
					throw (new ArgumentException("Name cannot be null."));
				}
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
		
		public ExecutionMethodSignature ExecutionMethod
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
				return TVA.DateTime.Common.TicksToSeconds(m_executionStopTime.Ticks - m_executionStartTime.Ticks);
			}
		}
		
		public string Status
		{
			get
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("              Process Name: ");
				with_1.Append(m_name);
				with_1.AppendLine();
				with_1.Append("             Current State: ");
				with_1.Append(m_currentState.ToString());
				with_1.AppendLine();
				with_1.Append("      Execution Start Time: ");
				if (m_executionStartTime != DateTime.MinValue)
				{
					with_1.Append(m_executionStartTime.ToString());
				}
				else
				{
					with_1.Append("N/A");
				}
				with_1.AppendLine();
				with_1.Append("       Execution Stop Time: ");
				if (m_executionStopTime != DateTime.MinValue)
				{
					with_1.Append(m_executionStopTime.ToString());
				}
				else
				{
					with_1.Append("N/A");
				}
				with_1.AppendLine();
				with_1.Append("       Last Execution Time: ");
				with_1.Append(TVA.DateTime.Common.SecondsToText(this.LastExecutionTime));
				with_1.AppendLine();
				
				return with_1.ToString();
			}
		}
		
		public void Start()
		{
			
			// Start the execution on a seperate thread.
			#if ThreadTracking
			m_processThread = new ManagedThread(InvokeExecutionMethod);
			m_processThread.Name = "TVA.Services.ServiceProcess.InvokeExecutionMethod() [" + m_name + "]";
			#else
			m_processThread = new Thread(new System.Threading.ThreadStart(InvokeExecutionMethod));
			#endif
			m_processThread.Start();
			
		}
		
		public void Abort()
		{
			
			if (m_processThread != null)
			{
				// We'll abort the process only if it is currently executing.
				m_processThread.Abort();
			}
			
		}
		
		private void InvokeExecutionMethod()
		{
			
			if (m_executionMethod != null)
			{
				CurrentState = processState.Processing;
				m_executionStartTime = DateTime.Now;
				m_executionStopTime = DateTime.MinValue;
				try
				{
					// We'll keep the invokation of the delegate in Try...Catch to absorb any exceptions that
					// were not handled by the consumer.
					m_executionMethod(m_name, m_parameters);
					this.CurrentState = processState.Processed;
				}
				catch (ThreadAbortException)
				{
					this.CurrentState = processState.Aborted;
				}
				catch (Exception)
				{
					// We'll absorb any exceptions if unhandled by the client.
					this.CurrentState = processState.Exception;
				}
				finally
				{
					m_executionStopTime = DateTime.Now;
				}
			}
			m_processThread = null;
			
		}
		
	}
	
}
