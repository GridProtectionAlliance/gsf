//*******************************************************************************************************
//  ServiceHelper.cs
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
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager. UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  12/14/2007 - Pinal C. Patel
//       Made monitoring of service health optional via the MonitorServiceHealth property.
//  09/30/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Diagnostics;
using PCS.IO;
using PCS.ErrorManagement;
using PCS.Communication;
using PCS.Scheduling;
using PCS.Configuration;
using PCS.Diagnostics;

namespace PCS.Services
{
    /// <summary>Helper class for a windows service</summary>
	[ToolboxBitmap(typeof(ServiceHelper))]
    public class ServiceHelper : Component, IPersistSettings, ISupportInitialize, IProvideStatus
	{
		#region [ Members ]
		
        // Constants
		
		/// <summary>
		/// The number of CrLfs to be appended to all the updates.
		/// </summary>
		public const int UpdateCrlfCount = 2;
		
		/// <summary>
		/// Default value for LogStatusUpdates property.
		/// </summary>
		public const bool DefaultLogStatusUpdates = true;
		
		/// <summary>
		/// Default value for MonitorServiceHealth property.
		/// </summary>
		public const bool DefaultMonitorServiceHealth = true;
		
		/// <summary>
		/// Default value for RequestHistoryLimit property.
		/// </summary>
		public const int DefaultRequestHistoryLimit = 50;
		
		/// <summary>
		/// Default value for QueryableSettingsCategories property.
		/// </summary>
		public const string DefaultQueryableSettingsCategories = "ServiceHelper, StatusLog, ErrorLogger";
		
		/// <summary>
		/// Default value for PersistSettings property.
		/// </summary>
		public const bool DefaultPersistSettings = false;
		
		/// <summary>
		/// Default value for SettingsCategoryName property.
		/// </summary>
		public const string DefaultSettingsCategoryName = "ServiceHelper";

        // Events
		
		/// <summary>
		/// Occurs when the service is starting.
		/// </summary>
        public event EventHandler<EventArgs<string[]>> ServiceStarting;
		
		/// <summary>
		/// Occurs when the service has started.
		/// </summary>
		public event EventHandler ServiceStarted;
		
		/// <summary>
		/// Occurs when the service is stopping.
		/// </summary>
		public event EventHandler ServiceStopping;
		
		/// <summary>
		/// Occurs when the service has stopped.
		/// </summary>
		public event EventHandler ServiceStopped;
		
		/// <summary>
		/// Occurs when the service is pausing.
		/// </summary>
		public event EventHandler ServicePausing;
		
		/// <summary>
		/// Occurs when the service has paused.
		/// </summary>
		public event EventHandler ServicePaused;
		
		/// <summary>
		/// Occurs when the service is resuming.
		/// </summary>
		public event EventHandler ServiceResuming;
		
		/// <summary>
		/// Occurs when the service has resumed.
		/// </summary>
		public event EventHandler ServiceResumed;
		
		/// <summary>
		/// Occurs when the system is being shutdowm.
		/// </summary>
		public event EventHandler SystemShutdown;	
		
		/// <summary>
		/// Occurs when a request is received from a client.
		/// </summary>
		public event EventHandler<EventArgs<IdentifiableItem<Guid, ClientRequest>>> ReceivedClientRequest;

        /// <summary>
        /// Occurs when the state of a defiend service process changes.
        /// </summary>
        public event EventHandler<EventArgs<string, ProcessState>> ProcessStateChanged;

        // Fields
		private bool m_logStatusUpdates;
		private bool m_monitorServiceHealth;
		private int m_requestHistoryLimit;
		private string m_queryableSettingsCategories;
		private bool m_persistSettings;
		private string m_settingsCategory;
		private bool m_suppressUpdates;
		private Guid m_remoteCommandClientID;
		private ServiceBase m_service;
		private List<ServiceProcess> m_processes;
        private List<ISupportLifecycle> m_serviceComponents;
		private List<ClientInfo> m_connectedClients;
		private List<ClientRequestInfo> m_clientRequestHistory;
		private List<ClientRequestHandlerInfo> m_clientRequestHandlers;
        private Dictionary<ISupportLifecycle, bool> m_componentEnabledStates;
        private PerformanceMonitor m_performanceMonitor;
		private Process m_remoteCommandProcess;
		private ServerBase m_remotingServer;
		private LogFile m_statusLog;
		private ScheduleManager m_scheduler;
		private ErrorLogger m_errorLogger;
		private string m_pursip;
        private bool m_disposed;
		
        #endregion

        #region [ Constructors ]
        
        public ServiceHelper()
		{
			m_logStatusUpdates = DefaultLogStatusUpdates;
			m_monitorServiceHealth = DefaultMonitorServiceHealth;
			m_requestHistoryLimit = DefaultRequestHistoryLimit;
			m_queryableSettingsCategories = DefaultQueryableSettingsCategories;
			m_persistSettings = DefaultPersistSettings;
			m_settingsCategory = DefaultSettingsCategoryName;
			m_processes = new List<ServiceProcess>();
			m_connectedClients = new List<ClientInfo>();
			m_clientRequestHistory = new List<ClientRequestInfo>();
			m_serviceComponents = new List<ISupportLifecycle>();
			m_clientRequestHandlers = new List<ClientRequestHandlerInfo>();
            m_componentEnabledStates = new Dictionary<ISupportLifecycle, bool>();
			m_pursip = "s3cur3";

			// Components
			m_statusLog = new LogFile();
			m_statusLog.LogException += m_statusLog_LogException;
			m_statusLog.FileName = "StatusLog.txt";
			m_statusLog.PersistSettings = true;
			m_statusLog.SettingsCategory = "StatusLog";

            m_scheduler = new ScheduleManager();
			m_scheduler.ScheduleDue += m_scheduler_ScheduleDue;
			m_scheduler.PersistSettings = true;
			m_scheduler.SettingsCategory = "Scheduler";

			m_errorLogger = new ErrorLogger();
			m_errorLogger.ExitOnUnhandledException = false;
			m_errorLogger.PersistSettings = true;
			m_errorLogger.SettingsCategory = "ErrorLogger";
		}

        #endregion

        #region [ Properties ]
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether status updates are to be logged to a text file.
		/// </summary>
		/// <value></value>
		/// <returns>True if status updates are to be logged to a text file; otherwise false.</returns>
		[Category("Preferences"), DefaultValue(DefaultLogStatusUpdates)]
        public bool LogStatusUpdates
		{
			get
			{
				return m_logStatusUpdates;
			}
			set
			{
				m_logStatusUpdates = value;
			}
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether the service health is to be monitored.
		/// </summary>
		/// <value></value>
		/// <returns>True if the service health is to be monitored; otherwise False.</returns>
		[Category("Preferences"), DefaultValue(DefaultMonitorServiceHealth)]
        public bool MonitorServiceHealth
		{
			get
			{
				return m_monitorServiceHealth;
			}
			set
			{
				m_monitorServiceHealth = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the number of request entries to be kept in the history.
		/// </summary>
		/// <value></value>
		/// <returns>The number of request entries to be kept in the history.</returns>
		[Category("Preferences"), DefaultValue(DefaultRequestHistoryLimit)]
        public int RequestHistoryLimit
		{
			get
			{
				return m_requestHistoryLimit;
			}
			set
			{
				if (value > 0)
				{
					m_requestHistoryLimit = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException("RequestHistoryLimit", "Value must be greater that 0.");
				}
			}
		}
		
		/// <summary>
		/// Gets or sets a comma seperated list of config file settings categories updateable by the service.
		/// </summary>
		/// <value></value>
		/// <returns>A comma seperated list of config file settings categories updateable by the service.</returns>
		[Category("Preferences"), DefaultValue(DefaultQueryableSettingsCategories)]
        public string QueryableSettingsCategories
		{
			get
			{
				return m_queryableSettingsCategories;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					m_queryableSettingsCategories = value;
				}
				else
				{
					throw new ArgumentNullException("QueryableSettingsCategories");
				}
			}
		}
		
		[Category("Persistance"), DefaultValue(DefaultPersistSettings)]
        public bool PersistSettings
		{
			get
			{
				return m_persistSettings;
			}
			set
			{
				m_persistSettings = value;
			}
		}
		
		[Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)]
        public string SettingsCategory
		{
			get
			{
				return m_settingsCategory;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					m_settingsCategory = value;
				}
				else
				{
					throw new ArgumentNullException("SettingsCategoryName");
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the parent service to which the service helper belongs.
		/// </summary>
		/// <value></value>
		/// <returns>The parent service to which the service helper belongs.</returns>
		[Category("Components")]
        public ServiceBase Service
		{
			get
			{
				return m_service;
			}
			set
			{
				m_service = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the instance of TCP server used for communicating with the clients.
		/// </summary>
		/// <value></value>
		/// <returns>An instance of TCP server.</returns>
		[Category("Components")]
        public ServerBase RemotingServer
		{
			get
			{
				return m_remotingServer;
			}
			set
			{
                if (m_remotingServer != null)
                {
                    // Detach events from any existing instance
				    m_remotingServer.ClientDisconnected -= m_remotingServer_ClientDisconnected;
				    m_remotingServer.ReceivedClientData -= m_remotingServer_ReceivedClientData;
                }

				m_remotingServer = value;

                if (m_remotingServer != null)
                {
                    // Attach events to new instance
				    m_remotingServer.ClientDisconnected += m_remotingServer_ClientDisconnected;
				    m_remotingServer.ReceivedClientData += m_remotingServer_ReceivedClientData;
                }
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ScheduleManager Scheduler
		{
			get
			{
				return m_scheduler;
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogFile StatusLog
		{
			get
			{
				return m_statusLog;
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ErrorLogger ErrorLogger
		{
			get
			{
				return m_errorLogger;
			}
		}
		
		/// <summary>
        /// Gets a list of all the components that implement the PCS.ISupportLifecycle interface assigned to the service.
		/// </summary>
		/// <value></value>
		/// <returns>A reference to the list of all the components (i.e., those that support ISupportLifecycle) associated with this service.</returns>
		[Browsable(false)]
        public List<ISupportLifecycle> ServiceComponents
		{
			get
			{
				return m_serviceComponents;
			}
		}
		
		[Browsable(false)]
        public List<ServiceProcess> Processes
		{
			get
			{
				return m_processes;
			}
		}
		
		[Browsable(false)]
        public List<ClientInfo> ConnectedClients
		{
			get
			{
				return m_connectedClients;
			}
		}
				
		[Browsable(false)]
        public List<ClientRequestInfo> ClientRequestHistory
		{
			get
			{
				return m_clientRequestHistory;
			}
		}
		
		[Browsable(false)]
        public List<ClientRequestHandlerInfo> ClientRequestHandlers
		{
			get
			{
				return m_clientRequestHandlers;
			}
		}
		
		[Browsable(false)]
        public PerformanceMonitor PerformanceMonitor
		{
			get
			{
				return m_performanceMonitor;
			}
		}

        public string Name
        {
            get
            {
                if (m_service != null)
                    return m_service.ServiceName;
                else
                    return this.GetType().Name;
            }
        }

        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                if (m_remotingServer != null)
                {
                    status.AppendFormat("System Uptime: {0}", Seconds.ToText(m_remotingServer.RunTime));
                    status.AppendLine();
                    status.AppendLine();
                }

                status.AppendFormat("Status of components used by {0}:", Name);
                status.AppendLine();

                foreach (ISupportLifecycle serviceComponent in m_serviceComponents)
                {
                    IProvideStatus statusProvider = serviceComponent as IProvideStatus;

                    if (statusProvider != null)
                    {
                        status.AppendLine();
                        status.AppendFormat("Status of {0}:", statusProvider.Name);
                        status.AppendLine();
                        status.Append(statusProvider.Status);
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
				
		/// <summary>
		/// Releases the unmanaged resources used by an instance of the <see cref="ServiceHelper" /> class and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!m_disposed)
			{
			    try
			    {			
				    if (disposing)
				    {
				        SaveSettings();

                        if (m_statusLog != null)
                        {
			                m_statusLog.LogException -= m_statusLog_LogException;
				            m_statusLog.Dispose();
                        }
                        m_statusLog = null;

                        if (m_scheduler != null)
                        {
			                m_scheduler.ScheduleDue -= m_scheduler_ScheduleDue;
				            m_scheduler.Dispose();
                        }
                        m_scheduler = null;

                        if (m_errorLogger != null)
                        {
				            m_errorLogger.Dispose();
                        }
                        m_errorLogger = null;

                        if (m_performanceMonitor != null)
			            {
				            m_performanceMonitor.Dispose();
			            }
                        m_performanceMonitor = null;

                        if (m_remoteCommandProcess != null)
                        {
                            m_remoteCommandProcess.ErrorDataReceived -= m_remoteCommandProcess_ErrorDataReceived;
                            m_remoteCommandProcess.OutputDataReceived -= m_remoteCommandProcess_OutputDataReceived;

                            if (!m_remoteCommandProcess.HasExited)
                                m_remoteCommandProcess.Kill();

                            m_remoteCommandProcess.Dispose();
                        }
                        m_remoteCommandProcess = null;

                        // Service processes are created and owned by remoting server, so we dispose them
                        if (m_processes != null)
                        {
                            foreach (ServiceProcess process in m_processes)
	                        {
                        		process.Dispose(); 
	                        }

                            m_processes.Clear();
                        }
                        m_processes = null;

                        // Detach any remoting server events, we don't own this component so we don't dispose it
                        RemotingServer = null;
				    }
                }
			    finally
			    {
			    	base.Dispose(disposing);    // Call base class Dispose().
				    m_disposed = true;          // Prevent duplicate dispose.
			    }
			}
		}
        public ServiceProcess GetProcess(string processName)
        {
            ServiceProcess match = null;

            foreach (ServiceProcess process in m_processes)
            {
                if (string.Compare(process.Name, processName, true) == 0)
                {
                    match = process;
                    break;
                }
            }

            return match;
        }

        public ClientInfo GetConnectedClient(Guid clientID)
        {
            ClientInfo match = null;

            foreach (ClientInfo clientInfo in m_connectedClients)
            {
                if (clientID == clientInfo.ClientID)
                {
                    match = clientInfo;
                    break;
                }
            }

            return match;
        }

        public ClientRequestHandlerInfo GetClientRequestHandler(string requestType)
        {
            ClientRequestHandlerInfo match = null;

            foreach (ClientRequestHandlerInfo handler in m_clientRequestHandlers)
            {
                if (string.Compare(handler.Command, requestType, true) == 0)
                {
                    match = handler;
                    break;
                }
            }

            return match;
        }

        /// <summary>
        /// To be called when the service is starts (inside the service's OnStart method).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStart(string[] args)
        {
            if (m_service != null)
            {
                // Notify service event consumers of pending service start
                if (ServiceStarting != null)
                    ServiceStarting(this, new EventArgs<string[]>(args));

                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Clients", "Displays list of clients connected to the service", ShowClients));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Settings", "Displays queryable service settings from config file", ShowSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Processes", "Displays list of service or system processes", ShowProcesses));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Schedules", "Displays list of process schedules defined in the service", ShowSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("History", "Displays list of requests received from the clients", ShowRequestHistory));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Help", "Displays list of commands supported by the service", ShowRequestHelp));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Status", "Displays the current service status", ShowServiceStatus));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Start", "Start a service or system process", StartProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Abort", "Aborts a service or system process", AbortProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("UpdateSettings", "Updates service setting in the config file", UpdateSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("ReloadSettings", "Reloads services settings from the config file", ReloadSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Reschedule", "Reschedules a process defined in the service", RescheduleProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Unschedule", "Unschedules a process defined in the service", UnscheduleProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("SaveSchedules", "Saves process schedules to the config file", SaveSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("LoadSchedules", "Loads process schedules from the config file", LoadSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Command", "Allows for a telnet-like remote command session", RemoteCommandSession, false));

                // Define "Health" command only if monitoring service health is enabled
                if (m_monitorServiceHealth)
                {
                    m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Health", "Displays a report of resource utilization for the service", ShowHealthReport));
                    m_performanceMonitor = new PerformanceMonitor();
                }

                // Add scheduler, status log and remoting server as service components by default
                m_serviceComponents.Add(m_scheduler);
                m_serviceComponents.Add(m_statusLog);
                m_serviceComponents.Add(m_remotingServer);

                // JRC: Disabled override of remoting server handshake operations since ServiceHelper doesn't
                // own remoting server - consumer does - and they may have specially defined transport options
                //if (m_remotingServer != null)
                //{
                //    m_remotingServer.Handshake = true;
                //    m_remotingServer.HandshakePassphrase = Name;
                //}

                // Open the status log
                if (m_logStatusUpdates)
                    m_statusLog.Open();

                // Initialize all service components when service has started
                foreach (ISupportLifecycle component in m_serviceComponents)
                {
                    if (component != null)
                        component.Initialize();
                }

                // Notify all remote clients that might possibly be connected at of service start (not likely)
                SendServiceStateChangedResponse(ServiceState.Started);

                // Notify service event consumers that service has started
                if (ServiceStarted != null)
                    ServiceStarted(this, EventArgs.Empty);
            }
            else
            {
                throw new InvalidOperationException("Service cannot be started. The Service property of ServiceHelper is not set.");
            }
        }

        /// <summary>
        /// To be called when the service is stopped (inside the service's OnStop method).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStop()
        {
            // Notify service event consumers of pending service stop
            if (ServiceStopping != null)
                ServiceStopping(this, EventArgs.Empty);

            // Notify all remote clients of service stop
            SendServiceStateChangedResponse(ServiceState.Stopped);

            // Abort any processes that may be currently executing.
            foreach (ServiceProcess process in m_processes)
            {
                if (process != null)
                    process.Abort();
            }

            // Set flag to prevent status updates from being posted from other threads, at this point this might cause exceptions.
            m_suppressUpdates = true;

            // Notify service event consumers that service has stopped
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// To be called when the service is paused (inside the service's OnPause method).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnPause()
        {
            // Notify service event consumers of pending service stop
            if (ServicePausing != null)
                ServicePausing(this, EventArgs.Empty);

            // Notify all remote clients of service pause
            SendServiceStateChangedResponse(ServiceState.Paused);

            // Track all enabled states at time of pause
            m_componentEnabledStates.Clear();

            // Disable all service components when service is pausing
            foreach (ISupportLifecycle component in m_serviceComponents)
            {
                if (component != null)
                {
                    m_componentEnabledStates.Add(component, component.Enabled);
                    component.Enabled = false;
                }
            }

            // Notify service event consumers that service has been paused
            if (ServicePaused != null)
                ServicePaused(this, EventArgs.Empty);
        }

        /// <summary>
        /// To be called when the service is resumed (inside the service's OnContinue method).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnResume()
        {
            bool state;

            // Notify service event consumers of pending service resume
            if (ServiceResuming != null)
                ServiceResuming(this, EventArgs.Empty);

            // Re-enable all service components when service is resuming
            foreach (ISupportLifecycle component in m_serviceComponents)
            {
                if (component != null)
                {
                    // Restore previous "enabled" state
                    if (m_componentEnabledStates.TryGetValue(component, out state))
                        component.Enabled = state;
                }
            }

            // Notify all remote clients of service resume
            SendServiceStateChangedResponse(ServiceState.Resumed);

            // Notify service event consumers that service has been resumed
            if (ServiceResumed != null)
                ServiceResumed(this, EventArgs.Empty);
        }

        /// <summary>
        /// To be when the system is shutting down (inside the service's OnShutdown method).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnShutdown()
        {
            // Notify service event consumers of pending service shutdown
            SendServiceStateChangedResponse(ServiceState.Shutdown);

            // Abort any processes that may be currently executing.
            foreach (ServiceProcess process in m_processes)
            {
                if (process != null)
                    process.Abort();
            }

            // Dispose of all service components when service is shutting down
            foreach (ISupportLifecycle component in m_serviceComponents)
            {
                if (component != null)
                    component.Dispose();
            }

            // Notify service event consumers that service has shutdown
            if (SystemShutdown != null)
                SystemShutdown(this, EventArgs.Empty);
        }

        /// <summary>
        /// To be called when the state of a process changes.
        /// </summary>
        /// <param name="processName">Name of the process whose state changed.</param>
        /// <param name="processState">New state of the process.</param>
        public void OnProcessStateChanged(string processName, ProcessState processState)
        {
            // Notify all service event consumer of change in process state
            if (ProcessStateChanged != null)
                ProcessStateChanged(this, new EventArgs<string, ProcessState>(processName, processState));

            // Notify all remote clients of change in process state
            SendProcessStateChangedResponse(processName, processState);
        }

        /// <summary>
        /// Sends the specified response to all of the connected clients.
        /// </summary>
        /// <param name="response">The response to be sent to the clients.</param>
        public void SendResponse(ServiceResponse response)
        {
            SendResponse(Guid.Empty, response);
        }

        /// <summary>
        /// Sends the specified resonse to the specified client only.
        /// </summary>
        /// <param name="clientID">ID of the client to whom the response is to be sent.</param>
        /// <param name="response">The response to be sent to the client.</param>
        public void SendResponse(Guid clientID, ServiceResponse response)
        {
            if (clientID == Guid.Empty)
            {
                // Multi-cast message to all connected clients if no client ID is specified
                m_remotingServer.Multicast(response);
            }
            else
            {
                // Send message directly to specified client
                m_remotingServer.SendTo(clientID, response);
            }
        }

        public void UpdateStatus(string message)
        {
            UpdateStatus(Guid.Empty, message);
        }

        public void UpdateStatus(Guid clientID, string message)
        {
            UpdateStatus(clientID, message, 0);
        }

        public void UpdateStatus(string message, int crlfCount)
        {
            UpdateStatus(Guid.Empty, message, crlfCount);
        }

        public void UpdateStatus(Guid clientID, string message, int crlfCount)
        {
            if (!m_suppressUpdates)
            {
                // Append desired number of line feeds to message
                StringBuilder formattedMessage = new StringBuilder();

                formattedMessage.Append(message);

                for (int i = 0; i < crlfCount; i++)
                {
                    formattedMessage.AppendLine();
                }

                // Send the status update to specified client(s)
                SendUpdateClientStatusResponse(clientID, formattedMessage.ToString());

                // Log the status update to the log file if logging is enabled.
                if (m_logStatusUpdates)
                    m_statusLog.WriteTimestampedLine(formattedMessage.ToString());
            }
        }

        public void AddProcess(ProcessExecutionMethod processExecutionMethod, string processName)
        {
            AddProcess(processExecutionMethod, processName, null);
        }

        public void AddProcess(ProcessExecutionMethod processExecutionMethod, string processName, object[] processParameters)
        {
            processName = processName.Trim();

            if (GetProcess(processName) == null)
            {
                m_processes.Add(new ServiceProcess(processExecutionMethod, processName, processParameters, this));
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is already defined.", processName));
            }
        }

        public void AddScheduledProcess(ProcessExecutionMethod processExecutionMethod, string processName, string processSchedule)
        {
            AddScheduledProcess(processExecutionMethod, processName, null, processSchedule);
        }

        public void AddScheduledProcess(ProcessExecutionMethod processExecutionMethod, string processName, object[] processParameters, string processSchedule)
        {
            AddProcess(processExecutionMethod, processName, processParameters);
            ScheduleProcess(processName, processSchedule);
        }

        public void ScheduleProcess(string processName, string scheduleRule)
        {
            ScheduleProcess(processName, scheduleRule, false);
        }

        public void ScheduleProcess(string processName, string scheduleRule, bool updateExistingSchedule)
        {
            processName = processName.Trim();

            if (GetProcess(processName) != null)
            {
                // The specified process exists, so we'll schedule it, or update its schedule if it is acheduled already.
                Schedule existingSchedule = m_scheduler.FindSchedule(processName);

                if (existingSchedule != null)
                {
                    // Update the process schedule if it is already exists.
                    if (updateExistingSchedule)
                        existingSchedule.Rule = scheduleRule;
                }
                else
                {
                    // Schedule the process if it is not scheduled already.
                    m_scheduler.Schedules.Add(new Schedule(processName, scheduleRule));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is not defined.", processName));
            }
        }

        public void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];

                if (settings.Count > 0)
                {
                    m_pursip = settings["Pursip"].ValueAs(m_pursip);
                    LogStatusUpdates = settings["LogStatusUpdates"].ValueAs(m_logStatusUpdates);
                    MonitorServiceHealth = settings["MonitorServiceHealth"].ValueAs(m_monitorServiceHealth);
                    RequestHistoryLimit = settings["RequestHistoryLimit"].ValueAs(m_requestHistoryLimit);
                    QueryableSettingsCategories = settings["QueryableSettingsCategories"].ValueAs(m_queryableSettingsCategories);
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];
                    CategorizedSettingsElement setting;

                    settings.Clear();

                    setting = settings["Pursip", true];
                    setting.Value = m_pursip;
                    setting.Description = "";
                    setting.Encrypted = true;

                    setting = settings["LogStatusUpdates", true];
                    setting.Value = m_logStatusUpdates.ToString();
                    setting.Description = "";

                    setting = settings["MonitorServiceHealth", true];
                    setting.Value = m_monitorServiceHealth.ToString();
                    setting.Description = "";

                    setting = settings["RequestHistoryLimit", true];
                    setting.Value = m_requestHistoryLimit.ToString();
                    setting.Description = "";

                    setting = settings["QueryableSettingsCategories", true];
                    setting.Value = m_queryableSettingsCategories;
                    setting.Description = "";

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        public void BeginInit()
        {
            // We don't need to do anything before the component is initialized.
        }

        public void EndInit()
        {
            // Load settings from the config file at run-time
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                LoadSettings();
        }

        private void SendUpdateClientStatusResponse(string response)
        {
            SendUpdateClientStatusResponse(Guid.Empty, response);
        }

        private void SendUpdateClientStatusResponse(Guid clientID, string response)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            serviceResponse.Type = "UPDATECLIENTSTATUS";
            serviceResponse.Message = response;
            SendResponse(clientID, serviceResponse);
        }

        private void SendServiceStateChangedResponse(ServiceState currentState)
        {
            ServiceResponse serviceResponse = new ServiceResponse("SERVICESTATECHANGED");
            serviceResponse.Attachments.Add(new ObjectState<ServiceState>(Name, currentState));
            SendResponse(serviceResponse);
        }

        private void SendProcessStateChangedResponse(string processName, ProcessState currentState)
        {
            ServiceResponse serviceResponse = new ServiceResponse("PROCESSSTATECHANGED");
            serviceResponse.Attachments.Add(new ObjectState<ProcessState>(processName, currentState));
            SendResponse(serviceResponse);
        }

        private void m_statusLog_LogException(object sender, EventArgs<System.Exception> e)
        {
            // We'll let the connected clients know that we encountered an exception while logging the status update.
            m_logStatusUpdates = false;
            UpdateStatus(string.Format("Error occurred while logging status update - {0}", e.Argument.ToString()), UpdateCrlfCount);
            m_logStatusUpdates = true;
        }

        private void m_scheduler_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            ServiceProcess scheduledProcess = GetProcess(e.Argument.Name);

            // Start the process execution if it exists.
            if (scheduledProcess != null)
                scheduledProcess.Start();
        }

        private void m_remotingServer_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            ClientInfo disconnectedClient = GetConnectedClient(e.Argument);

            if (disconnectedClient != null)
            {
                if (e.Argument == m_remoteCommandClientID)
                {
                    try
                    {
                        RemoteCommandSession(new ClientRequestInfo(disconnectedClient, ClientRequest.Parse("Command -disconnect")));
                    }
                    catch (Exception)
                    {
                        // We'll encounter an exception because we'll try to update the status of the client that had the
                        // remote command session open and this will fail since the client is disconnected.
                    }
                }

                m_connectedClients.Remove(disconnectedClient);
                UpdateStatus(string.Format("Remote client disconnected - {0} from {1}.", disconnectedClient.UserName, disconnectedClient.MachineName), UpdateCrlfCount);
            }
        }

        private void m_remotingServer_ReceivedClientData(object sender, EventArgs<IdentifiableItem<Guid, byte[]>> e)
        {
            ClientInfo client = Serialization.GetObject<ClientInfo>(e.Argument.Item);
            ClientRequest request = Serialization.GetObject<ClientRequest>(e.Argument.Item);
            ClientInfo requestSender = GetConnectedClient(e.Argument.ID);

            if (client != null)
            {
                // We've received client information from a recently connected client.
                client.ConnectedAt = DateTime.Now;
                m_connectedClients.Add(client);
                UpdateStatus(string.Format("Remote client connected - {0} from {1}.", client.UserName, client.MachineName), UpdateCrlfCount);
            }
            else if (request != null)
            {
                try
                {
                    ClientRequestInfo requestInfo = new ClientRequestInfo(requestSender, request);

                    if (m_remoteCommandClientID == Guid.Empty || requestSender.ClientID != m_remoteCommandClientID)
                    {
                        // Log the received request.
                        m_clientRequestHistory.Add(requestInfo);

                        // We'll remove old request entries if we've exceeded the limit for request history.
                        if (m_clientRequestHistory.Count > m_requestHistoryLimit)
                            m_clientRequestHistory.RemoveRange(0, (m_clientRequestHistory.Count - m_requestHistoryLimit));

                        // Notify the consumer about the incoming request from client.
                        if (ReceivedClientRequest != null)
                            ReceivedClientRequest(this, new EventArgs<IdentifiableItem<Guid, ClientRequest>>(new IdentifiableItem<Guid, ClientRequest>(requestSender.ClientID, request)));

                        ClientRequestHandlerInfo requestHandler = GetClientRequestHandler(request.Command);

                        if (requestHandler != null)
                            requestHandler.HandlerMethod(requestInfo);
                        else
                            UpdateStatus(requestSender.ClientID, string.Format("Failed to process request \"{0}\". Request is invalid.", request.Command), UpdateCrlfCount);
                    }
                    else
                    {
                        RemoteCommandSession(requestInfo);
                    }
                }
                catch (Exception ex)
                {
                    m_errorLogger.Log(ex);
                    UpdateStatus(requestSender.ClientID, string.Format("Failed to process request \"{0}\". {1}.", request.Command, ex.Message), UpdateCrlfCount);
                }
            }
            else
            {
                UpdateStatus(requestSender.ClientID, "Failed to process request - Request could not be deserialized.", UpdateCrlfCount);
            }
        }

        private void m_remoteCommandProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, e.Data, 1);
        }

        private void m_remoteCommandProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, e.Data, 1);
        }

        #region [ Client Request Handlers ]

        private void ShowClients(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of clients currently connected to the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Clients -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                if (m_connectedClients.Count > 0)
                {
                    // Display info about all of the clients connected to the service.
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Clients connected to {0}:", Name);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("Client".PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append("Machine".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("User".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("Connected".PadRight(20));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 25));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));

                    foreach (ClientInfo clientInfo in m_connectedClients)
                    {
                        responseMessage.AppendLine();

                        if (!string.IsNullOrEmpty(clientInfo.ClientName))
                            responseMessage.Append(clientInfo.ClientName.PadRight(25));
                        else
                            responseMessage.Append("[Not Available]".PadRight(25));

                        responseMessage.Append(' ');
                        if (!string.IsNullOrEmpty(clientInfo.MachineName))
                            responseMessage.Append(clientInfo.MachineName.PadRight(15));
                        else
                            responseMessage.Append("[Not Available]".PadRight(15));

                        responseMessage.Append(' ');
                        if (!string.IsNullOrEmpty(clientInfo.UserName))
                            responseMessage.Append(clientInfo.UserName.PadRight(15));
                        else
                            responseMessage.Append("[Not Available]".PadRight(15));

                        responseMessage.Append(' ');
                        responseMessage.Append(clientInfo.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                    }

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("No clients are connected to {0}", Name), UpdateCrlfCount);
                }
            }
        }

        private void ShowSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of queryable settings of the service from the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Settings -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string[] settingsCategories = m_queryableSettingsCategories.Replace(" ", "").Split(',');

                if (settingsCategories.Length > 0)
                {
                    // Display info about all of the queryable settings defined in the service.
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Queryable settings of {0}:", Name);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("Category".PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append("Name".PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append("Value".PadRight(30));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 25));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 30));

                    foreach (string category in settingsCategories)
                    {
                        foreach (CategorizedSettingsElement setting in ConfigurationFile.Current.Settings[category])
                        {
                            responseMessage.AppendLine();
                            responseMessage.Append(category.PadRight(25));
                            responseMessage.Append(' ');
                            responseMessage.Append(setting.Name.PadRight(20));
                            responseMessage.Append(' ');

                            if (!string.IsNullOrEmpty(setting.Value))
                                responseMessage.Append(setting.Value.PadRight(30));
                            else
                                responseMessage.Append("[Not Set]".PadRight(30));
                        }
                    }

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
                }
                else
                {
                    // No queryable settings are defined in the service.
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("No queryable settings are defined in {0}.", Name), UpdateCrlfCount);
                }
            }
        }

        private void ShowProcesses(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of defined service processes or running system processes.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Processes -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Displays system processes instead of service processes");
                }

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                bool listSystemProcesses = requestInfo.Request.Arguments.Exists("system");

                if (!listSystemProcesses)
                {
                    if (m_processes.Count > 0)
                    {
                        // Display info about all the processes defined in the service.
                        StringBuilder responseMessage = new StringBuilder();

                        responseMessage.AppendFormat("Processes defined in {0}:", Name);
                        responseMessage.AppendLine();
                        responseMessage.AppendLine();
                        responseMessage.Append("Name".PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append("State".PadRight(15));
                        responseMessage.Append(' ');
                        responseMessage.Append("Last Exec. Start".PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append("Last Exec. Stop".PadRight(20));
                        responseMessage.AppendLine();
                        responseMessage.Append(new string('-', 20));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 15));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 20));
                        responseMessage.Append(' ');
                        responseMessage.Append(new string('-', 20));

                        foreach (ServiceProcess process in m_processes)
                        {
                            responseMessage.AppendLine();
                            responseMessage.Append(process.Name.PadRight(20));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.CurrentState.ToString().PadRight(15));
                            responseMessage.Append(' ');

                            if (process.ExecutionStartTime != DateTime.MinValue)
                                responseMessage.Append(process.ExecutionStartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                            else
                                responseMessage.Append("[Not Executed]".PadRight(20));

                            responseMessage.Append(' ');

                            if (process.ExecutionStopTime != DateTime.MinValue)
                            {
                                responseMessage.Append(process.ExecutionStopTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                            }
                            else
                            {
                                if (process.ExecutionStartTime != DateTime.MinValue)
                                    responseMessage.Append("[Executing]".PadRight(20));
                                else
                                    responseMessage.Append("[Not Executed]".PadRight(20));
                            }
                        }

                        UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
                    }
                    else
                    {
                        // No processes defined in the service to be displayed.
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("No processes are defined in {0}.", Name), UpdateCrlfCount);
                    }
                }
                else
                {
                    // We enumerate "system" processes when -system parameter is specified
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Processes running on {0}:", Environment.MachineName);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("ID".PadRight(5));
                    responseMessage.Append(' ');
                    responseMessage.Append("Name".PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append("Priority".PadRight(15));
                    responseMessage.Append(' ');
                    responseMessage.Append("Responding".PadRight(10));
                    responseMessage.Append(' ');
                    responseMessage.Append("Start Time".PadRight(20));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 5));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 25));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 15));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 10));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));

                    foreach (Process process in Process.GetProcesses())
                    {
                        try
                        {
                            responseMessage.Append(process.StartInfo.UserName);
                            responseMessage.AppendLine();
                            responseMessage.Append(process.Id.ToString().PadRight(5));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.ProcessName.PadRight(25));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.PriorityClass.ToString().PadRight(15));
                            responseMessage.Append(' ');
                            responseMessage.Append((process.Responding ? "Yes" : "No").PadRight(10));
                            responseMessage.Append(' ');
                            responseMessage.Append(process.StartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                        }
                        catch
                        {
                        }
                    }

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
                }
            }
        }

        private void ShowSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of schedules for processes defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Schedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                if (m_scheduler.Schedules.Count > 0)
                {
                    // Display info about all the process schedules defined in the service.
                    StringBuilder responseMessage = new StringBuilder();

                    responseMessage.AppendFormat("Process schedules defined in {0}:", Name);
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();
                    responseMessage.Append("Name".PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append("Rule".PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append("Last Due".PadRight(30));
                    responseMessage.AppendLine();
                    responseMessage.Append(new string('-', 20));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 20));
                    responseMessage.Append(' ');
                    responseMessage.Append(new string('-', 30));

                    foreach (Schedule schedule in m_scheduler.Schedules)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(schedule.Name.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(schedule.Rule.PadRight(20));
                        responseMessage.Append(' ');

                        if (schedule.LastDueAt != DateTime.MinValue)
                            responseMessage.Append(schedule.LastDueAt.ToString().PadRight(30));
                        else
                            responseMessage.Append("[Never]".PadRight(30));
                    }

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("No process schedules are defined in {0}.", Name), UpdateCrlfCount);
                }
            }
        }

        private void ShowRequestHistory(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of recent requests received from the clients.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       History -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                StringBuilder responseMessage = new StringBuilder();

                responseMessage.AppendFormat("History of requests received by {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Command".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Received".PadRight(25));
                responseMessage.Append(' ');
                responseMessage.Append("Sender".PadRight(30));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 25));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 30));

                foreach (ClientRequestInfo historicRequest in m_clientRequestHistory)
                {
                    responseMessage.AppendLine();
                    responseMessage.Append(historicRequest.Request.Command.PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append(historicRequest.ReceivedAt.ToString().PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append(string.Format("{0} from {1}", historicRequest.Sender.UserName, historicRequest.Sender.MachineName).PadRight(30));
                }

                UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
            }
        }

        private void ShowRequestHelp(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of commands supported by the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Help -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder responseMessage = new StringBuilder();

                responseMessage.AppendFormat("Commands supported by {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Command".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Description".PadRight(55));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 55));

                foreach (ClientRequestHandlerInfo handler in m_clientRequestHandlers)
                {
                    if (handler.IsAdvertised || showAdvancedHelp)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(handler.Command.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(handler.CommandDescription.PadRight(55));
                    }
                }

                UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString(), UpdateCrlfCount);
            }
        }

        private void ShowHealthReport(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a resource utilization report for the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Health -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                if (m_performanceMonitor != null)
                    UpdateStatus(requestInfo.Sender.ClientID, m_performanceMonitor.Status, UpdateCrlfCount);
                else
                    UpdateStatus(requestInfo.Sender.ClientID, "Performance monitor is not available.", UpdateCrlfCount);
            }
        }

        private void ShowServiceStatus(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays status of this service and its components.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Status -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                UpdateStatus(requestInfo.Sender.ClientID, Status, UpdateCrlfCount);
            }
        }

        private void ReloadSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Reloads settings of the component whose settings are saved under the specified category in the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ReloadSettings \"Category Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Category name must be defined as one of the queryable settings categories in the QueryableSettingsCategories property of ServiceHelper. ");
                helpMessage.Append("Also, category name is case sensitive so it must be the same case as it appears in the settings listing.");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string settingsTarget = null;
                string categoryName = requestInfo.Request.Arguments["orderedarg1"];

                if (m_queryableSettingsCategories.IndexOf(categoryName) >= 0)
                {
                    if (m_settingsCategory == categoryName)
                    {
                        LoadSettings();
                        settingsTarget = categoryName;
                    }
                    else
                    {
                        if (settingsTarget != null)
                        {
                            // Check service components
                            foreach (ISupportLifecycle component in m_serviceComponents)
                            {
                                IPersistSettings reloadableComponent = component as IPersistSettings;
                                if (reloadableComponent != null && reloadableComponent.SettingsCategory == categoryName)
                                {
                                    reloadableComponent.LoadSettings();

                                    IProvideStatus statusProvider = component as IProvideStatus;
                                    if (statusProvider != null)
                                        settingsTarget = statusProvider.Name;

                                    break;
                                }
                            }
                        }

                        if (settingsTarget != null)
                        {
                            // Check containter components
                            foreach (Component component in Container.Components)
                            {
                                IPersistSettings reloadableComponent = component as IPersistSettings;
                                if (reloadableComponent != null && reloadableComponent.SettingsCategory == categoryName)
                                {
                                    reloadableComponent.LoadSettings();
                                    settingsTarget = component.GetType().Name;
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(settingsTarget))
                    {
                        if (settingsTarget == categoryName)
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully loaded settings for category \"{1}\".", categoryName), UpdateCrlfCount);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully loaded settings for component \"{0}\" from category \"{1}\".", settingsTarget, categoryName), UpdateCrlfCount);
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to load component settings from category \"{0}\". No corresponding settings category name found.", categoryName), UpdateCrlfCount);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to load component settings from category \"{0}\". Category is not one of the queryable settings categories.", categoryName), UpdateCrlfCount);
                }
            }
        }

        private void UpdateSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 3)
            {
                // We'll display help about the request since we either don't have the required arguments or the user
                // has explicitly requested for the help to be displayed for this request type.
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Updates the specified setting under the specified category in the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       UpdateSettings \"Category Name\" \"Setting Name\" \"Setting Value\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -add".PadRight(20));
                helpMessage.Append("Adds specified setting to the specified category");
                helpMessage.AppendLine();
                helpMessage.Append("       -delete".PadRight(20));
                helpMessage.Append("Deletes specified setting from the specified category");
                helpMessage.AppendLine();
                helpMessage.Append("       -reload".PadRight(20));
                helpMessage.Append("Causes corresponding component to reload settings");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list all of the queryable settings");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Category name must be defined as one of the queryable settings categories in the QueryableSettingsCategories property of ServiceHelper. ");
                helpMessage.Append("Also, category and setting names are case sensitive so they must be the same case as they appears in the settings listing.");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string categoryName = requestInfo.Request.Arguments["orderedarg1"];
                string settingName = requestInfo.Request.Arguments["orderedarg2"];
                string settingValue = requestInfo.Request.Arguments["orderedarg3"];
                bool addSetting = requestInfo.Request.Arguments.Exists("add");
                bool deleteSetting = requestInfo.Request.Arguments.Exists("delete");
                bool reloadSettings = requestInfo.Request.Arguments.Exists("reload");
                bool listSettings = requestInfo.Request.Arguments.Exists("list");

                if (m_queryableSettingsCategories.IndexOf(categoryName) >= 0)
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[categoryName];
                    CategorizedSettingsElement setting;

                    if (settings != null)
                    {
                        // The specified category is one of the defined queryable categories.
                        if (true == addSetting)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to add setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
                            settings.Add(settingName, settingValue);
                            ConfigurationFile.Current.Save();
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully added setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
                        }
                        else if (true == deleteSetting)
                        {
                            setting = settings[settingName];
                            if (setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to delete setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
                                settings.Remove(setting);
                                ConfigurationFile.Current.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully deleted setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to delete setting \"{0}\" under category \"{1}\". Setting does not exist.", settingName, categoryName), UpdateCrlfCount);
                            }
                        }
                        else
                        {
                            setting = settings[settingName];
                            if (setting != null)
                            {
                                // The requested setting does exist under the specified category.
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to update setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
                                setting.Value = settingValue;
                                ConfigurationFile.Current.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully updated setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
                            }
                            else
                            {
                                // The requested setting does not exist under the specified category.
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to update value of setting \"{0}\" under category \"{1}\" . Setting does not exist.", settingName, categoryName), UpdateCrlfCount);
                            }
                        }

                        if (reloadSettings)
                        {
                            // The user has requested to reload settings for all the components.
                            requestInfo.Request = ClientRequest.Parse(string.Format("ReloadSettings {0}", categoryName));
                            ReloadSettings(requestInfo);
                        }

                        if (listSettings)
                        {
                            // The user has requested to list all of the queryable settings.
                            requestInfo.Request = ClientRequest.Parse("Settings");
                            ShowSettings(requestInfo);
                        }
                    }
                    else
                    {
                        // The specified category does not exist.
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to update value of setting \"{0}\" under category \"{1}\". Category does not exist.", settingName, categoryName), UpdateCrlfCount);
                    }
                }
                else
                {
                    // The specified category is not one of the defined queryable categories.
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to update value of setting \"{0}\" under category \"{1}\". Category is not one of the queryable categories.", settingName, categoryName), UpdateCrlfCount);
                }
            }
        }

        private void StartProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Starts execution of the specified service or system process.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Start \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -restart".PadRight(20));
                helpMessage.Append("Aborts the process if executing and start it again");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all service or system processes");
                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Treats the specified process as a system process");
                }

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool systemProcess = requestInfo.Request.Arguments.Exists("system");
                bool restartProcess = requestInfo.Request.Arguments.Exists("restart");
                bool listProcesses = requestInfo.Request.Arguments.Exists("list");

                if (restartProcess)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Abort \"{0}\" {1}", processName, systemProcess ? "-system" : ""));
                    AbortProcess(requestInfo);
                }

                if (!systemProcess)
                {
                    ServiceProcess processToStart = GetProcess(processName);

                    if (processToStart != null)
                    {
                        if (processToStart.CurrentState != ProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to start service process \"{0}\"...", processName), UpdateCrlfCount);
                            processToStart.Start();
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully started service process \"{0}\".", processName), UpdateCrlfCount);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start process \"{0}\". Process is already executing.", processName), UpdateCrlfCount);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start service process \"{0}\". Process is not defined.", processName), UpdateCrlfCount);
                    }
                }
                else
                {
                    try
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to start system process \"{0}\"...", processName), UpdateCrlfCount);
                        Process startedProcess = Process.Start(processName);

                        if (startedProcess != null)
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully started system process \"{0}\".", processName), UpdateCrlfCount);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start system process \"{0}\".", processName), UpdateCrlfCount);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start system process \"{0}\". {1}.", processName, ex.Message), UpdateCrlfCount);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void AbortProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");

                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Aborts the specified service or system process if executing.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Abort \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all service or system processes");

                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Treats the specified process as a system process");
                    helpMessage.AppendLine();
                    helpMessage.AppendLine();
                    helpMessage.Append("NOTE: Specify process name of \"Me\" to kill current service process. ");
                }

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool systemProcess = requestInfo.Request.Arguments.Exists("system");
                bool listProcesses = requestInfo.Request.Arguments.Exists("list");

                if (!systemProcess)
                {
                    ServiceProcess processToAbort = GetProcess(processName);

                    if (processToAbort != null)
                    {
                        if (processToAbort.CurrentState == ProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to abort service process \"{0}\"...", processName), UpdateCrlfCount);
                            processToAbort.Abort();
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully aborted service process \"{0}\".", processName), UpdateCrlfCount);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort service process \"{0}\". Process is not executing.", processName), UpdateCrlfCount);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort service process \"{0}\". Process is not defined.", processName), UpdateCrlfCount);
                    }
                }
                else
                {
                    Process processToAbort = null;

                    if (string.Compare(processName, "Me", true) == 0)
                    {
                        processName = Process.GetCurrentProcess().ProcessName;
                    }

                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        // Lookup for the system process by name.
                        processToAbort = process;
                        break;
                    }

                    if (processToAbort == null)
                    {
                        int processID;

                        if (int.TryParse(processName, out processID) && processID > 0)
                        {
                            processToAbort = Process.GetProcessById(processID);
                            processName = processToAbort.ProcessName;
                        }
                    }

                    if (processToAbort != null)
                    {
                        try
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to abort system process \"{0}\"...", processName), UpdateCrlfCount);
                            processToAbort.Kill();
                            if (processToAbort.WaitForExit(10000))
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully aborted system process \"{0}\".", processName), UpdateCrlfCount);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\". Process not responding.", processName), UpdateCrlfCount);
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\". {1}.", processName, ex.Message), UpdateCrlfCount);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\". Process is not running.", processName), UpdateCrlfCount);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void RescheduleProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 2)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Schedules or re-schedules an existing process defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Reschedule \"Process Name\" \"Schedule Rule\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -save".PadRight(20));
                helpMessage.Append("Saves all process schedules to the config file");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("NOTE: The schedule rule uses UNIX crontab syntax which consists of 5 parts (For example, \"* * * * *\"). ");
                helpMessage.Append("Following is a brief description of each of the 5 parts that make up the rule:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Part 1 - Minute part; value range 0 to 59. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 2 - Hour part; value range 0 to 23. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 3 - Day of month part; value range 1 to 31. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 4 - Month part; value range 1 to 12. ");
                helpMessage.AppendLine();
                helpMessage.Append("   Part 5 - Day of week part; value range 0 to 6 (0 = Sunday). ");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("Following is a description of valid syntax for all parts of the rule:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   *       - Any value in the range for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   */n     - Every nth value for the data-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   n1-n2   - Range of values (inclusive) for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.Append("   n1,n2   - 1 or more specific values for the date-time part.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("Examples:");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   \"* * * * *\"       - Process executes every minute.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"*/5 * * * *\"     - Process executes every 5 minutes.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"5 * * * *\"       - Process executes 5 past every hour.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 * * *\"       - Process executes every day at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 1 * *\"       - Process executes 1st of every month at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 * * 0\"       - Process executes every Sunday at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"0 0 31 12 *\"     - Process executes on December 31 at midnight.");
                helpMessage.AppendLine();
                helpMessage.Append("   \"5,10 0-2 * * *\"  - Process executes 5 and 10 past hours 12am to 2am.");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                string scheduleRule = requestInfo.Request.Arguments["orderedarg2"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                try
                {
                    // Schedule the process if not scheduled or update its schedule if scheduled.
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to schedule process \"{0}\" with rule \"{1}\"...", processName, scheduleRule), UpdateCrlfCount);
                    ScheduleProcess(processName, scheduleRule, true);
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully scheduled process \"{0}\" with rule \"{1}\".", processName, scheduleRule), UpdateCrlfCount);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to schedule process \"{0}\". {1}", processName, ex.Message), UpdateCrlfCount);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void UnscheduleProcess(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Unschedules a scheduled process defined in the service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Unschedule \"Process Name\" -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -save".PadRight(20));
                helpMessage.Append("Saves all process schedules to the config file");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                Schedule scheduleToRemove = m_scheduler.FindSchedule(processName);

                if (scheduleToRemove != null)
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to unschedule process \"{0}\"...", processName), UpdateCrlfCount);
                    m_scheduler.Schedules.Remove(scheduleToRemove);
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully unscheduled process \"{0}\".", processName), UpdateCrlfCount);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to unschedule process \"{0}\". Process is not scheduled.", null), UpdateCrlfCount);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void SaveSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Saves all process schedules to the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       SaveSchedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to save process schedules to the config file...", UpdateCrlfCount);
                m_scheduler.SaveSettings();
                UpdateStatus(requestInfo.Sender.ClientID, "Successfully saved process schedules to the config file.", UpdateCrlfCount);

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void LoadSchedules(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Loads all process schedules from the config file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       LoadSchedules -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to load process schedules from the config file...", UpdateCrlfCount);
                m_scheduler.LoadSettings();
                UpdateStatus(requestInfo.Sender.ClientID, "Successfully loaded process schedules from the config file.", UpdateCrlfCount);

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void RemoteCommandSession(ClientRequestInfo requestinfo)
        {
            if (m_remoteCommandProcess == null && requestinfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Allows for a telnet-like remote command session.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Command -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -connect".PadRight(20));
                helpMessage.Append("Established a remote command session");
                helpMessage.AppendLine();
                helpMessage.Append("       -disconnect".PadRight(20));
                helpMessage.Append("Terminates established remote command session");
                helpMessage.AppendLine();
                helpMessage.Append("       -password=\"Command Session Password\"".PadRight(20));
                helpMessage.Append("Password required to establish a remote command session");

                UpdateStatus(requestinfo.Sender.ClientID, helpMessage.ToString(), UpdateCrlfCount);
            }
            else
            {
                bool connectSession = requestinfo.Request.Arguments.Exists("connect");
                bool disconnectSession = requestinfo.Request.Arguments.Exists("disconnect");
                bool passwordProvided = requestinfo.Request.Arguments.Exists("password");

                if (string.Compare(requestinfo.Request.Command, "Command", true) == 0 && m_remoteCommandProcess == null && connectSession && passwordProvided)
                {
                    // User wants to establish a remote command session.
                    string password = requestinfo.Request.Arguments["password"];

                    if (password == m_pursip)
                    {
                        // Establish remote command session
                        m_remoteCommandProcess = new Process();
                        m_remoteCommandProcess.ErrorDataReceived += m_remoteCommandProcess_ErrorDataReceived;
                        m_remoteCommandProcess.OutputDataReceived += m_remoteCommandProcess_OutputDataReceived;
                        m_remoteCommandProcess.StartInfo.FileName = "cmd.exe";
                        m_remoteCommandProcess.StartInfo.UseShellExecute = false;
                        m_remoteCommandProcess.StartInfo.RedirectStandardInput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardOutput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardError = true;
                        m_remoteCommandProcess.Start();
                        m_remoteCommandProcess.BeginOutputReadLine();
                        m_remoteCommandProcess.BeginErrorReadLine();

                        m_remoteCommandClientID = requestinfo.Sender.ClientID;

                        UpdateStatus(requestinfo.Sender.ClientID, "Remote command session established - all entry will now be redirected to command session.", UpdateCrlfCount);

                        SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("CommandSession", "Established"));
                    }
                    else
                    {
                        UpdateStatus(requestinfo.Sender.ClientID, "Failed to establish remote command session - Password is invalid.", UpdateCrlfCount);
                    }
                }
                else if (string.Compare(requestinfo.Request.Command, "Command", true) == 0 && m_remoteCommandProcess != null && disconnectSession)
                {
                    // User wants to terminate an established remote command session.                   
                    m_remoteCommandProcess.ErrorDataReceived -= m_remoteCommandProcess_ErrorDataReceived;
                    m_remoteCommandProcess.OutputDataReceived -= m_remoteCommandProcess_OutputDataReceived;
                    
                    if (!m_remoteCommandProcess.HasExited)
                        m_remoteCommandProcess.Kill();

                    m_remoteCommandProcess.Dispose();
                    m_remoteCommandProcess = null;
                    
                    m_remoteCommandClientID = Guid.Empty;

                    UpdateStatus(requestinfo.Sender.ClientID, "Remote command session terminated - all entry will now be redirected back to service session.", UpdateCrlfCount);

                    SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("CommandSession", "Terminated"));
                }
                else if (m_remoteCommandProcess != null)
                {
                    // User has entered commands that must be redirected to the established command session.
                    string input = requestinfo.Request.Command + " " + requestinfo.Request.Arguments.ToString();
                    m_remoteCommandProcess.StandardInput.WriteLine(input);
                }
                else
                {
                    // User has provided insufficient information.
                    requestinfo.Request = ClientRequest.Parse("Command /");
                    RemoteCommandSession(requestinfo);
                }
            }
        }

        #endregion

        #endregion
	}
}
