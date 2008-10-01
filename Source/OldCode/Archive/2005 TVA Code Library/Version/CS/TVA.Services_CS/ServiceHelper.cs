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
using TVA.IO;
using TVA.ErrorManagement;
using TVA.Communication;
using TVA.Scheduling;
using TVA.Configuration;
using TVA.Diagnostics;

namespace TVA.Services
{
    /// <summary>Helper class for a windows service</summary>
	[ToolboxBitmap(typeof(ServiceHelper))]
    public class ServiceHelper : Component, IPersistSettings, ISupportInitialize
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
		public event EventHandler<EventArgs<object>> ServiceStarting;
		
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

        // Fields
		private bool m_logStatusUpdates;
		private bool m_monitorServiceHealth;
		private int m_requestHistoryLimit;
		private string m_queryableSettingsCategories;
		private bool m_persistSettings;
		private string m_settingsCategoryName;
		private bool m_suppressUpdates;
		private Guid m_remoteCommandClientID;
		private ServiceBase m_service;
		private List<ServiceProcess> m_processes;
		private List<IServiceComponent> m_serviceComponents;		
		private List<ClientInfo> m_connectedClients;
		private List<ClientRequestInfo> m_clientRequestHistory;
		private List<ClientRequestHandlerInfo> m_clientRequestHandlers;
		private PerformanceMonitor m_performanceMonitor;
		private Process m_remoteCommandProcess;
		private CommunicationServerBase m_remotingServer;
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
			m_settingsCategoryName = DefaultSettingsCategoryName;
			m_processes = new List<ServiceProcess>();
			m_connectedClients = new List<ClientInfo>();
			m_clientRequestHistory = new List<ClientRequestInfo>();
			m_serviceComponents = new List<IServiceComponent>();
			m_clientRequestHandlers = new List<ClientRequestHandlerInfo>();
			m_pursip = "s3cur3";

			// Components
			m_statusLog = new LogFile();
			m_statusLog.LogException += m_statusLog_LogException;
			m_statusLog.Name = "StatusLog.txt";
			m_statusLog.PersistSettings = true;
			m_statusLog.SettingsCategoryName = "StatusLog";

            m_scheduler = new ScheduleManager();
			m_scheduler.ScheduleDue += m_scheduler_ScheduleDue;
			m_scheduler.PersistSettings = true;
			m_scheduler.SettingsCategoryName = "Scheduler";

			m_errorLogger = new ErrorLogger();
			m_errorLogger.ExitOnUnhandledException = false;
			m_errorLogger.PersistSettings = true;
			m_errorLogger.SettingsCategoryName = "ErrorLogger";
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
        public string SettingsCategoryName
		{
			get
			{
				return m_settingsCategoryName;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					m_settingsCategoryName = value;
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
        public CommunicationServerBase RemotingServer
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
		/// Gets a list of all the components that implement the TVA.Services.IServiceComponent interface.
		/// </summary>
		/// <value></value>
		/// <returns>An instance of System.Collections.Generic.List(Of TVA.Services.IServiceComponent).</returns>
		[Browsable(false)]
        public List<IServiceComponent> ServiceComponents
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

        #endregion

        #region [ Methods ]
				
		/// <summary>
		/// Releases the unmanaged resources used by an instance of the <see cref="ServiceHelper" /> class and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
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

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]
		
        // Static Fields
		
        // Static Constructor
		
        // Static Properties
		
        // Static Methods

        #endregion
				
		#region " Methods "
		
		public ServiceProcess Processes(string processName)
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

        public ClientInfo ConnectedClients(Guid clientID)
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
		
		public ClientRequestHandlerInfo ClientRequestHandlers(string requestType)
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
					ServiceStarting(this, new EventArgs<object>(args));
				
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
				
                // Add scheduler and remoting server as service components by default
				m_serviceComponents.Add(m_scheduler);
				m_serviceComponents.Add(m_remotingServer);

                // JRC: Disabled override of remoting server handshake operations since ServiceHelper doesn't
                // own remoting server - consumer does - and they may have specially defined transport options
                //if (m_remotingServer != null)
                //{
                //    m_remotingServer.Handshake = true;
                //    m_remotingServer.HandshakePassphrase = m_service.ServiceName;
                //}

				// Open the status log
				if (m_logStatusUpdates)
					m_statusLog.Open();
				
                // Notify all service components of service started
				foreach (IServiceComponent component in m_serviceComponents)
				{
					if (component != null)
						component.ServiceStateChanged(ServiceState.Started);
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
			
			// Notify all service components of service stopping
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
					component.ServiceStateChanged(ServiceState.Stopped);
			}

			// Close the status log, if open
			if (m_statusLog.IsOpen)
				m_statusLog.Close();
            			
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
			
			// Notify all service components of service pausing
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
					component.ServiceStateChanged(ServiceState.Paused);
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
            // Notify service event consumers of pending service resume
			if (ServiceResuming != null)
				ServiceResuming(this, EventArgs.Empty);
			
			// Notify all service components of service resuming
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
					component.ServiceStateChanged(ServiceState.Resumed);
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
			
			// Notify all service components of service shutdown - typically components
            // self-dispose when this message is received
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
					component.ServiceStateChanged(ServiceState.Shutdown);
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
		public void ProcessStateChanged(string processName, ProcessState processState)
		{
			// Notify all service components of change in process state
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
					component.ProcessStateChanged(processName, processState);
			}
			
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
				System.Text.StringBuilder formattedMessage = new StringBuilder();

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
			
			if (Processes(processName) == null)
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
			
			if (Processes(processName) != null)
			{
				// The specified process exists, so we'll schedule it, or update its schedule if it is acheduled already.
				Schedule existingSchedule = m_scheduler.Schedules(processName);
				
				if (existingSchedule != null)
				{
				    // Update the process schedule if it is already exists.
					if (updateExistingSchedule)
						existingSchedule.Rule = scheduleRule;
				}
				else
				{
					// Schedule the process if it is not scheduled already.
					m_scheduler.Schedules().Add(new Schedule(processName, scheduleRule));
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
				CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

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
					CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];
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
			serviceResponse.Attachments.Add(new ObjectState<ServiceState>(m_service.ServiceName, currentState));
			SendResponse(serviceResponse);
		}
		
		private void SendProcessStateChangedResponse(string processName, ProcessState currentState)
		{
			ServiceResponse serviceResponse = new ServiceResponse("PROCESSSTATECHANGED");
			serviceResponse.Attachments.Add(new ObjectState<ProcessState>(processName, currentState));			
			SendResponse(serviceResponse);			
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
					StringBuilder resposneMessage = new StringBuilder();

					resposneMessage.AppendFormat("Clients connected to {0}:", m_service.ServiceName);
					resposneMessage.AppendLine();
					resposneMessage.AppendLine();
					resposneMessage.Append("Client".PadRight(25));
					resposneMessage.Append(' ');
					resposneMessage.Append("Machine".PadRight(15));
					resposneMessage.Append(' ');
					resposneMessage.Append("User".PadRight(15));
					resposneMessage.Append(' ');
					resposneMessage.Append("Connected".PadRight(20));
					resposneMessage.AppendLine();
					resposneMessage.Append(new string('-', 25));
					resposneMessage.Append(' ');
					resposneMessage.Append(new string('-', 15));
					resposneMessage.Append(' ');
					resposneMessage.Append(new string('-', 15));
					resposneMessage.Append(' ');
					resposneMessage.Append(new string('-', 20));

					foreach (ClientInfo clientInfo in m_connectedClients)
					{
						resposneMessage.AppendLine();
						
                        if (!string.IsNullOrEmpty(clientInfo.ClientName))
							resposneMessage.Append(clientInfo.ClientName.PadRight(25));
						else
							resposneMessage.Append("[Not Available]".PadRight(25));

                        resposneMessage.Append(' ');
						if (!string.IsNullOrEmpty(clientInfo.MachineName))
							resposneMessage.Append(clientInfo.MachineName.PadRight(15));
						else
							resposneMessage.Append("[Not Available]".PadRight(15));

                        resposneMessage.Append(' ');
						if (!string.IsNullOrEmpty(clientInfo.UserName))
							resposneMessage.Append(clientInfo.UserName.PadRight(15));
						else
							resposneMessage.Append("[Not Available]".PadRight(15));

                        resposneMessage.Append(' ');
						resposneMessage.Append(clientInfo.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, resposneMessage.ToString(), UpdateCrlfCount);
				}
				else
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("No clients are connected to {0}", m_service.ServiceName), UpdateCrlfCount);
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

					responseMessage.AppendFormat("Queryable settings of {0}:", m_service.ServiceName);
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
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("No queryable settings are defined in {0}.", m_service.ServiceName), UpdateCrlfCount);
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

						responseMessage.AppendFormat("Processes defined in {0}:", m_service.ServiceName);
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
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("No processes are defined in {0}.", m_service.ServiceName), UpdateCrlfCount);
					}
				}
				else
				{
                    // We enumerate "system" processes when -system parameter is specified
					StringBuilder ResponseMessage = new StringBuilder();

					ResponseMessage.AppendFormat("Processes running on {0}:", Environment.MachineName);
					ResponseMessage.AppendLine();
					ResponseMessage.AppendLine();
					ResponseMessage.Append("ID".PadRight(5));
					ResponseMessage.Append(' ');
					ResponseMessage.Append("Name".PadRight(25));
					ResponseMessage.Append(' ');
					ResponseMessage.Append("Priority".PadRight(15));
					ResponseMessage.Append(' ');
					ResponseMessage.Append("Responding".PadRight(10));
					ResponseMessage.Append(' ');
					ResponseMessage.Append("Start Time".PadRight(20));
					ResponseMessage.AppendLine();
					ResponseMessage.Append(new string('-', 5));
					ResponseMessage.Append(' ');
					ResponseMessage.Append(new string('-', 25));
					ResponseMessage.Append(' ');
					ResponseMessage.Append(new string('-', 15));
					ResponseMessage.Append(' ');
					ResponseMessage.Append(new string('-', 10));
					ResponseMessage.Append(' ');
					ResponseMessage.Append(new string('-', 20));

					foreach (Process process in Process.GetProcesses())
					{
						try
						{
							ResponseMessage.Append(process.StartInfo.UserName);
							ResponseMessage.AppendLine();
							ResponseMessage.Append(process.Id.ToString().PadRight(5));
							ResponseMessage.Append(' ');
							ResponseMessage.Append(process.ProcessName.PadRight(25));
							ResponseMessage.Append(' ');
							ResponseMessage.Append(process.PriorityClass.ToString().PadRight(15));
							ResponseMessage.Append(' ');
							ResponseMessage.Append((process.Responding ? "Yes" : "No").PadRight(10));
							ResponseMessage.Append(' ');
							ResponseMessage.Append(process.StartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
						}
						catch
						{
						}
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, ResponseMessage.ToString(), UpdateCrlfCount);
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
				if (m_scheduler.Schedules().Count > 0)
				{
					// Display info about all the process schedules defined in the service.
					System.Text.StringBuilder with_2 = new StringBuilder();
					with_2.AppendFormat("Process schedules defined in {0}:", m_service.ServiceName);
					with_2.AppendLine();
					with_2.AppendLine();
					with_2.Append("Name".PadRight(20));
					with_2.Append(' ');
					with_2.Append("Rule".PadRight(20));
					with_2.Append(' ');
					with_2.Append("Last Due".PadRight(30));
					with_2.AppendLine();
					with_2.Append(new string('-', 20));
					with_2.Append(' ');
					with_2.Append(new string('-', 20));
					with_2.Append(' ');
					with_2.Append(new string('-', 30));
					foreach (Schedule schedule in m_scheduler.Schedules)
					{
						with_2.AppendLine();
						with_2.Append(schedule.Name.PadRight(20));
						with_2.Append(' ');
						with_2.Append(schedule.Rule.PadRight(20));
						with_2.Append(' ');
						if (schedule.LastDueAt != DateTime.MinValue)
						{
							with_2.Append(schedule.LastDueAt.ToString().PadRight(30));
						}
						else
						{
							with_2.Append("[Never]".PadRight(30));
						}
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
				}
				else
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("No process schedules are defined in {0}.", m_service.ServiceName), UpdateCrlfCount);
				}
			}
			
		}
		
		private void ShowRequestHistory(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of recent requests received from the clients.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       History -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				System.Text.StringBuilder with_2 = new StringBuilder();
				with_2.AppendFormat("History of requests received by {0}:", m_service.ServiceName);
				with_2.AppendLine();
				with_2.AppendLine();
				with_2.Append("Command".PadRight(20));
				with_2.Append(' ');
				with_2.Append("Received".PadRight(25));
				with_2.Append(' ');
				with_2.Append("Sender".PadRight(30));
				with_2.AppendLine();
				with_2.Append(new string('-', 20));
				with_2.Append(' ');
				with_2.Append(new string('-', 25));
				with_2.Append(' ');
				with_2.Append(new string('-', 30));
				foreach (ClientRequestInfo historicRequest in m_clientRequestHistory)
				{
					with_2.AppendLine();
					with_2.Append(historicRequest.Request.Command.PadRight(20));
					with_2.Append(' ');
					with_2.Append(historicRequest.ReceivedAt.ToString().PadRight(25));
					with_2.Append(' ');
					// For some reason StringBuilder.AppendFormat() doesn't work as expected with String.PadRight()
					with_2.Append((historicRequest.Sender.UserName + " from " + historicRequest.Sender.MachineName).PadRight(30));
				}
				
				UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
			}
			
		}
		
		private void ShowRequestHelp(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of commands supported by the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Help -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");
				
				System.Text.StringBuilder with_2 = new StringBuilder();
				with_2.AppendFormat("Commands supported by {0}:", m_service.ServiceName);
				with_2.AppendLine();
				with_2.AppendLine();
				with_2.Append("Command".PadRight(20));
				with_2.Append(' ');
				with_2.Append("Description".PadRight(55));
				with_2.AppendLine();
				with_2.Append(new string('-', 20));
				with_2.Append(' ');
				with_2.Append(new string('-', 55));
				foreach (ClientRequestHandlerInfo handler in m_clientRequestHandlers)
				{
					if (handler.IsAdvertised || showAdvancedHelp)
					{
						with_2.AppendLine();
						with_2.Append(handler.Command.PadRight(20));
						with_2.Append(' ');
						with_2.Append(handler.CommandDescription.PadRight(55));
					}
				}
				
				UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
			}
			
		}
		
		private void ShowHealthReport(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a resource utilization report for the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Health -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				UpdateStatus(requestInfo.Sender.ClientID, m_performanceMonitor.Status, UpdateCrlfCount);
			}
			
		}
		
		private void ShowServiceStatus(ClientRequestInfo requestInfo)
		{
			
			System.Text.StringBuilder with_1 = new StringBuilder();
			with_1.Append(string.Format("Status of components used by {0}:", m_service.ServiceName));
			with_1.AppendLine();
			foreach (IServiceComponent serviceComponent in m_serviceComponents)
			{
				if (serviceComponent != null)
				{
					with_1.AppendLine();
					with_1.Append(string.Format("Status of {0}:", serviceComponent.Name));
					with_1.AppendLine();
					with_1.Append(serviceComponent.Status);
				}
			}
			
			UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			
		}
		
		private void ReloadSettings(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Reloads settings of the component whose settings are saved under the specified category in the config file.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       ReloadSettings \"Category Name\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("IMPORTANT: Category name must be defined as one of the queryable settings categories in the QueryableSettingsCategories property of ServiceHelper. ");
				with_1.Append("Also, category name is case sensitive so it must be the same case as it appears in the settings listing.");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				Component targetComponent = null;
				string categoryName = requestInfo.Request.Arguments["orderedarg1"];
				if (m_queryableSettingsCategories.IndexOf(categoryName) >= 0)
				{
					if (m_settingsCategoryName == categoryName)
					{
						LoadSettings();
						targetComponent = this;
					}
					else
					{
						if (targetComponent != null)
						{
							foreach (Component component in components.Components)
							{
								TVA.Configuration.IPersistSettings reloadableComponent = component as TVA.Configuration.IPersistSettings;
								if ((reloadableComponent != null)&& reloadableComponent.SettingsCategoryName == categoryName)
								{
									reloadableComponent.LoadSettings();
									targetComponent = component;
									break;
								}
							}
						}
						
						if (targetComponent != null)
						{
							foreach (Component component in m_serviceComponents)
							{
								TVA.Configuration.IPersistSettings reloadableComponent = component as TVA.Configuration.IPersistSettings;
								if ((reloadableComponent != null)&& reloadableComponent.SettingsCategoryName == categoryName)
								{
									reloadableComponent.LoadSettings();
									targetComponent = component;
									break;
								}
							}
						}
					}
					
					if (targetComponent != null)
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully loaded settings for component \"{0}\" from category \"{1}\".", targetComponent.GetType().Name, categoryName), UpdateCrlfCount);
					}
					else
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to load component settings from category \"{0}\" - No corresponding component found.", categoryName), UpdateCrlfCount);
					}
				}
				else
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to load component settings from category \"{0}\" - Category is not one of the queryable categories.", categoryName), UpdateCrlfCount);
				}
			}
			
		}
		
		private void UpdateSettings(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 3)
			{
				// We'll display help about the request since we either don't have the required arguments or the user
				// has explicitly requested for the help to be displayed for this request type.
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Updates the specified setting under the specified category in the config file.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       UpdateSettings \"Category Name\" \"Setting Name\" \"Setting Value\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -add".PadRight(20));
				with_1.Append("Adds specified setting to the specified category");
				with_1.AppendLine();
				with_1.Append("       -delete".PadRight(20));
				with_1.Append("Deletes specified setting from the specified category");
				with_1.AppendLine();
				with_1.Append("       -reload".PadRight(20));
				with_1.Append("Causes corresponding component to reload settings");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list all of the queryable settings");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("IMPORTANT: Category name must be defined as one of the queryable settings categories in the QueryableSettingsCategories property of ServiceHelper. ");
				with_1.Append("Also, category and setting names are case sensitive so they must be the same case as they appears in the settings listing.");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				string categoryName = requestInfo.Request.Arguments["orderedarg1"];
				string settingName = requestInfo.Request.Arguments["orderedarg2"];
				string settingValue = requestInfo.Request.Arguments["orderedarg3"];
				bool addSetting = requestInfo.Request.Arguments.Exists("add");
				bool deleteSetting = requestInfo.Request.Arguments.Exists("delete");
				bool doReloadSettings = requestInfo.Request.Arguments.Exists("reload");
				bool doListSettings = requestInfo.Request.Arguments.Exists("list");
				
				if (m_queryableSettingsCategories.IndexOf(categoryName) >= 0)
				{
					// The specified category is one of the defined queryable categories.
					if (true == addSetting)
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to add setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
						TVA.Configuration.Common.CategorizedSettings(categoryName).Add(settingName, settingValue);
						TVA.Configuration.Common.SaveSettings();
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully added setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
					}
					else if (true == deleteSetting)
					{
						CategorizedSettingsElement setting = TVA.Configuration.Common.CategorizedSettings(categoryName)[settingName];
						if (setting != null)
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to delete setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
							TVA.Configuration.Common.CategorizedSettings(categoryName).Remove(setting);
							TVA.Configuration.Common.SaveSettings();
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully deleted setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
						}
						else
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to delete setting \"{0}\" under category \"{1}\" - Setting does not exist.", settingName, categoryName), UpdateCrlfCount);
						}
					}
					else
					{
						CategorizedSettingsElement setting_1 = TVA.Configuration.Common.CategorizedSettings(categoryName)[settingName];
						if (setting_1 != null)
						{
							// The requested setting does exist under the specified category.
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to update setting \"{0}\" under category \"{1}\"...", settingName, categoryName), UpdateCrlfCount);
							setting_1.Value = settingValue;
							TVA.Configuration.Common.SaveSettings();
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully updated setting \"{0}\" under category \"{1}\".", settingName, categoryName), UpdateCrlfCount);
						}
						else
						{
							// The requested setting does not exist under the specified category.
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to update value of setting \"{0}\" under category \"{1}\" - Setting does not exist.", settingName, categoryName), UpdateCrlfCount);
						}
					}
					
					if (doReloadSettings)
					{
						// The user has requested to reload settings for all the components.
						requestInfo.Request = ClientRequest.Parse(string.Format("ReloadSettings {0}", categoryName));
						ReloadSettings(requestInfo);
					}
					
					if (doListSettings)
					{
						// The user has requested to list all of the queryable settings.
						requestInfo.Request = ClientRequest.Parse("Settings");
						ShowSettings(requestInfo);
					}
				}
				else
				{
					// The specified category is not one of the defined queryable categories.
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to update value of setting \"{0}\" under category \"{1}\" - Category is not one of the queryable categories.", settingName, categoryName), UpdateCrlfCount);
				}
			}
			
		}
		
		private void StartProcess(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
			{
				bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");
				
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Starts execution of the specified service or system process.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Start \"Process Name\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -restart".PadRight(20));
				with_1.Append("Aborts the process if executing and start it again");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all service or system processes");
				if (showAdvancedHelp)
				{
					with_1.AppendLine();
					with_1.Append("       -system".PadRight(20));
					with_1.Append("Treats the specified process as a system process");
				}
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				string processName = requestInfo.Request.Arguments["orderedarg1"];
				bool isSystemProcess = requestInfo.Request.Arguments.Exists("system");
				bool doRestartProcess = requestInfo.Request.Arguments.Exists("restart");
				bool doListProcesses = requestInfo.Request.Arguments.Exists("list");
				
				if (doRestartProcess)
				{
					requestInfo.Request = ClientRequest.Parse(string.Format("Abort \"{0}\" {1}", processName, TVA.Common.IIf(isSystemProcess, "-system", "")));
					AbortProcess(requestInfo);
				}
				
				if (! isSystemProcess)
				{
					ServiceProcess processToStart = Processes(processName);
					
					if (processToStart != null)
					{
						if (processToStart.CurrentState != processState.Processing)
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to start service process \"{0}\"...", processName), UpdateCrlfCount);
							processToStart.Start();
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully started service process \"{0}\".", processName), UpdateCrlfCount);
						}
						else
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start process \"{0}\" - Process is already executing.", processName), UpdateCrlfCount);
						}
					}
					else
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start service process \"{0}\" - Process is not defined.", processName), UpdateCrlfCount);
					}
				}
				else
				{
					try
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to start system process \"{0}\"...", processName), UpdateCrlfCount);
						Process startedProcess = Process.Start(processName);
						if (startedProcess != null)
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully started system process \"{0}\".", processName), UpdateCrlfCount);
						}
						else
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start system process \"{0}\" - Reason unknown.", processName), UpdateCrlfCount);
						}
					}
					catch (Exception ex)
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to start system process \"{0}\" - {1}.", processName, ex.Message), UpdateCrlfCount);
					}
				}
				
				if (doListProcesses)
				{
					requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", TVA.Common.IIf(isSystemProcess, "-system", "")));
					ShowProcesses(requestInfo);
				}
			}
			
		}
		
		private void AbortProcess(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 1)
			{
				bool showAdvancedHelp = requestInfo.Request.Arguments.Exists("advanced");
				
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Aborts the specified service or system process if executing.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Abort \"Process Name\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all service or system processes");
				if (showAdvancedHelp)
				{
					with_1.AppendLine();
					with_1.Append("       -system".PadRight(20));
					with_1.Append("Treats the specified process as a system process");
				}
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				string processName = requestInfo.Request.Arguments["orderedarg1"];
				bool isSystemProcess = requestInfo.Request.Arguments.Exists("system");
				bool doListProcesses = requestInfo.Request.Arguments.Exists("list");
				
				if (! isSystemProcess)
				{
					ServiceProcess processToAbort = Processes(processName);
					
					if (processToAbort != null)
					{
						if (processToAbort.CurrentState == processState.Processing)
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to abort service process \"{0}\"...", processName), UpdateCrlfCount);
							processToAbort.Abort();
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully aborted service process \"{0}\".", processName), UpdateCrlfCount);
						}
						else
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort service process \"{0}\" - Process is not executing.", processName), UpdateCrlfCount);
						}
					}
					else
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort service process \"{0}\" - Process is not defined.", processName), UpdateCrlfCount);
					}
				}
				else
				{
					Process processToAbort = null;
					
					if (string.Compare(processName, "Me", true) == 0)
					{
						processName = Process.GetCurrentProcess().ProcessName;
					}
					
					foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName(processName))
					{
						// Lookup for the system process by name.
						processToAbort = process;
						break;
					}
					
					if (processToAbort == null)
					{
						int processID;
						
						if (int.TryParse(processName, ref processID) && processID > 0)
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
								UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\" - Process not responding.", processName), UpdateCrlfCount);
							}
						}
						catch (Exception ex)
						{
							UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\" - {1}.", processName, ex.Message), UpdateCrlfCount);
						}
					}
					else
					{
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to abort system process \"{0}\" - Process is not running.", processName), UpdateCrlfCount);
					}
				}
				
				if (doListProcesses)
				{
					requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", TVA.Common.IIf(isSystemProcess, "-system", "")));
					ShowProcesses(requestInfo);
				}
			}
			
		}
		
		private void RescheduleProcess(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount < 2)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Schedules or re-schedules an existing process defined in the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Reschedule \"Process Name\" \"Schedule Rule\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -save".PadRight(20));
				with_1.Append("Saves all process schedules to the config file");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all process schedules");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("NOTE: The schedule rule is a UNIX \"cron\" style rule which consists of 5 parts (For example, \"* * * * *\"). ");
				with_1.Append("Following is a brief description of each of the 5 parts that make up the rule:");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Part 1 - Minute part; value range 0 to 59. ");
				with_1.AppendLine();
				with_1.Append("   Part 2 - Hour part; value range 0 to 23. ");
				with_1.AppendLine();
				with_1.Append("   Part 3 - Day of month part; value range 1 to 31. ");
				with_1.AppendLine();
				with_1.Append("   Part 4 - Month part; value range 1 to 12. ");
				with_1.AppendLine();
				with_1.Append("   Part 5 - Day of week part; value range 0 to 6 (0 = Sunday). ");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("Following is a description of valid syntax for all parts of the rule:");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   *       - Any value in the range for the date-time part.");
				with_1.AppendLine();
				with_1.Append("   */n     - Every nth value for the data-time part.");
				with_1.AppendLine();
				with_1.Append("   n1-n2   - Range of values (inclusive) for the date-time part.");
				with_1.AppendLine();
				with_1.Append("   n1,n2   - 1 or more specific values for the date-time part.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("Examples:");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   \"* * * * *\"       - Process executes every minute.");
				with_1.AppendLine();
				with_1.Append("   \"*/5 * * * *\"     - Process executes every 5 minutes.");
				with_1.AppendLine();
				with_1.Append("   \"5 * * * *\"       - Process executes 5 past every hour.");
				with_1.AppendLine();
				with_1.Append("   \"0 0 * * *\"       - Process executes every day at midnight.");
				with_1.AppendLine();
				with_1.Append("   \"0 0 1 * *\"       - Process executes 1st of every month at midnight.");
				with_1.AppendLine();
				with_1.Append("   \"0 0 * * 0\"       - Process executes every Sunday at midnight.");
				with_1.AppendLine();
				with_1.Append("   \"0 0 31 12 *\"     - Process executes on December 31 at midnight.");
				with_1.AppendLine();
				with_1.Append("   \"5,10 0-2 * * *\"  - Process executes 5 and 10 past hours 12am to 2am.");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				string processName = requestInfo.Request.Arguments["orderedarg1"];
				string scheduleRule = requestInfo.Request.Arguments["orderedarg2"];
				bool doSaveSchedules = requestInfo.Request.Arguments.Exists("save");
				bool doListSchedules = requestInfo.Request.Arguments.Exists("list");
				
				try
				{
					// Schedule the process if not scheduled or update its schedule if scheduled.
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to schedule process \"{0}\" with rule \"{1}\"...", processName, scheduleRule), UpdateCrlfCount);
					ScheduleProcess(processName, scheduleRule, true);
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully scheduled process \"{0}\" with rule \"{1}\".", processName, scheduleRule), UpdateCrlfCount);
					
					if (doSaveSchedules)
					{
						requestInfo.Request = ClientRequest.Parse("SaveSchedules");
						SaveSchedules(requestInfo);
					}
				}
				catch (Exception ex)
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to schedule process \"{0}\" - {1}", processName, ex.Message), UpdateCrlfCount);
				}
				
				if (doListSchedules)
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
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Unschedules a scheduled process defined in the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Unschedule \"Process Name\" -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -save".PadRight(20));
				with_1.Append("Saves all process schedules to the config file");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all process schedules");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				string processName = requestInfo.Request.Arguments["orderedarg1"];
				bool doSaveSchedules = requestInfo.Request.Arguments.Exists("save");
				bool doListSchedules = requestInfo.Request.Arguments.Exists("list");
				Schedule scheduleToRemove = m_scheduler.Schedules(processName);
				
				if (scheduleToRemove != null)
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Attempting to unschedule process \"{0}\"...", processName), UpdateCrlfCount);
					m_scheduler.Schedules.Remove(scheduleToRemove);
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Successfully unscheduled process \"{0}\".", processName), UpdateCrlfCount);
					
					if (doSaveSchedules)
					{
						requestInfo.Request = ClientRequest.Parse("SaveSchedules");
						SaveSchedules(requestInfo);
					}
				}
				else
				{
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("Failed to unschedule process \"{0}\" - Process is not scheduled.", null), UpdateCrlfCount);
				}
				
				if (doListSchedules)
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
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Saves all process schedules to the config file.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       SaveSchedules -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all process schedules");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				bool doListSchedules = requestInfo.Request.Arguments.Exists("list");
				
				UpdateStatus(requestInfo.Sender.ClientID, "Attempting to save process schedules to the config file...", UpdateCrlfCount);
				m_scheduler.SaveSettings();
				UpdateStatus(requestInfo.Sender.ClientID, "Successfully saved process schedules to the config file.", UpdateCrlfCount);
				
				if (doListSchedules)
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
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Loads all process schedules from the config file.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       LoadSchedules -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -list".PadRight(20));
				with_1.Append("Displays list of all process schedules");
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				bool doListSchedules = requestInfo.Request.Arguments.Exists("list");
				
				UpdateStatus(requestInfo.Sender.ClientID, "Attempting to load process schedules from the config file...", UpdateCrlfCount);
				m_scheduler.LoadSettings();
				UpdateStatus(requestInfo.Sender.ClientID, "Successfully loaded process schedules from the config file.", UpdateCrlfCount);
				
				if (doListSchedules)
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
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Allows for a telnet-like remote command session.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Command -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				with_1.AppendLine();
				with_1.Append("       -connect".PadRight(20));
				with_1.Append("Established a remote command session");
				with_1.AppendLine();
				with_1.Append("       -disconnect".PadRight(20));
				with_1.Append("Terminates established remote command session");
				with_1.AppendLine();
				with_1.Append("       -password".PadRight(20));
				with_1.Append("Password required to establish a remote command session");
				
				UpdateStatus(requestinfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
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
						m_remoteCommandProcess = new Process();
						m_remoteCommandProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(m_remoteCommandProcess_ErrorDataReceived);
						m_remoteCommandProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(m_remoteCommandProcess_OutputDataReceived);
						m_remoteCommandProcess.StartInfo.FileName = "cmd.exe";
						m_remoteCommandProcess.StartInfo.UseShellExecute = false;
						m_remoteCommandProcess.StartInfo.RedirectStandardInput = true;
						m_remoteCommandProcess.StartInfo.RedirectStandardOutput = true;
						m_remoteCommandProcess.StartInfo.RedirectStandardError = true;
						m_remoteCommandProcess.Start();
						m_remoteCommandProcess.BeginOutputReadLine();
						m_remoteCommandProcess.BeginErrorReadLine();
						
						m_remoteCommandClientID = requestinfo.Sender.ClientID;
						
						UpdateStatus(requestinfo.Sender.ClientID, "Remote command session established - Future commands will be redirected.", UpdateCrlfCount);
						
						SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("CommandSession", "Established"));
					}
					else
					{
						UpdateStatus(requestinfo.Sender.ClientID, "Failed to establish remote command session - Password is invalid.", UpdateCrlfCount);
					}
				}
				else if (string.Compare(requestinfo.Request.Command, "Command", true) == 0 && (m_remoteCommandProcess != null)&& disconnectSession)
				{
					// User wants to terminate an established remote command session.
					m_remoteCommandProcess.Kill();
					m_remoteCommandProcess = null;
					m_remoteCommandProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(m_remoteCommandProcess_ErrorDataReceived);
					m_remoteCommandProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(m_remoteCommandProcess_OutputDataReceived);
					m_remoteCommandClientID = Guid.Empty;
					
					UpdateStatus(requestinfo.Sender.ClientID, "Remote command session terminated - Future commands will be processed normally.", UpdateCrlfCount);
					
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
		
		private void m_statusLog_LogException(object sender, EventArgs<System.Exception> e)
		{
			
			// We'll let the connected clients know that we encountered an exception while logging the status update.
			m_logStatusUpdates = false;
			UpdateStatus(string.Format("Error occurred while logging status update - {0}", e.Argument.ToString()), UpdateCrlfCount);
			m_logStatusUpdates = true;
			
		}
		
		private void m_scheduler_ScheduleDue(object sender, ScheduleEventArgs e)
		{
			
			ServiceProcess scheduledProcess = Processes(e.Schedule.Name);
			if (scheduledProcess != null)
			{
				scheduledProcess.Start(); // Start the process execution if it exists.
			}
			
		}
		
		private void m_remotingServer_ClientDisconnected(object sender, EventArgs<System.Guid> e)
		{
			
			ClientInfo disconnectedClient = ConnectedClients(e.Argument);
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
				
				m_connectedClients.Remove(ConnectedClients(e.Argument));
				UpdateStatus(string.Format("Remote client disconnected - {0} from {1}.", disconnectedClient.UserName, disconnectedClient.MachineName), UpdateCrlfCount);
			}
			
		}
		
		private void m_remotingServer_ReceivedClientData(object sender, EventArgs<IdentifiableItem<System.Guid, byte[]>> e)
		{
			
			ClientInfo client = TVA.Serialization.GetObject<ClientInfo>(e.Argument.Item);
			ClientRequest request = TVA.Serialization.GetObject<ClientRequest>(e.Argument.Item);
			ClientInfo requestSender = ConnectedClients(e.Argument.Source);
			
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
						if (m_clientRequestHistory.Count > m_requestHistoryLimit)
						{
							// We'll remove old request entries if we've exceeded the limit for request history.
							m_clientRequestHistory.RemoveRange(0, (m_clientRequestHistory.Count - m_requestHistoryLimit));
						}
						
						// Notify the consumer about the incoming request from client.
						if (ReceivedClientRequestEvent != null)
							ReceivedClientRequestEvent(this, new EventArgs<IdentifiableItem<Guid, ClientRequest>>(new IdentifiableItem<Guid, ClientRequest>(requestSender.ClientID, request)));
						
						ClientRequestHandlerInfo requestHandler = ClientRequestHandlers(request.Command);
						if (requestHandler != null)
						{
							requestHandler.HandlerMethod(requestInfo);
						}
						else
						{
							UpdateStatus(requestSender.ClientID, string.Format("Failed to process request \"{0}\" - Request is invalid.", request.Command), UpdateCrlfCount);
						}
					}
					else
					{
						RemoteCommandSession(requestInfo);
					}
				}
				catch (Exception ex)
				{
					m_errorLogger.Log(ex);
					UpdateStatus(requestSender.ClientID, string.Format("Failed to process request \"{0}\" - {1}.", request.Command, ex.Message), UpdateCrlfCount);
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
		
		#endregion		
	}
}
