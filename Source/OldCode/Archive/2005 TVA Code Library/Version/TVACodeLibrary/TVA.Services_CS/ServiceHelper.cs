using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ServiceProcess;
//using TVA.Common;
using TVA.Communication;
//using TVA.Serialization;
using TVA.Scheduling;
using TVA.Configuration;
//using TVA.Configuration.Common;
using TVA.Diagnostics;

//*******************************************************************************************************
//  TVA.Services.ServiceHelper.vb - Helper class for a windows service
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
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
//       Made monitoring of service health optional via the MonitorServiceHealth property as this
//       feature was causing the highly utilized services to die after running for a prolonged period of
//       time. By default monitoring service health is off.
//
//*******************************************************************************************************


namespace TVA.Services
{
	[ToolboxBitmap(typeof(ServiceHelper))]public partial class ServiceHelper : TVA.Configuration.IPersistSettings, ISupportInitialize
	{
		
		
		#region " Variables "
		
		private string m_pursip;
		private bool m_logStatusUpdates;
		private bool m_monitorServiceHealth;
		private int m_requestHistoryLimit;
		private string m_queryableSettingsCategories;
		private bool m_persistSettings;
		private string m_settingsCategoryName;
		private ServiceBase m_service;
		private List<ServiceProcess> m_processes;
		private List<IServiceComponent> m_serviceComponents;
		
		private bool m_suppressUpdates;
		private Guid m_remoteCommandClientID;
		private PerformanceMonitor m_performanceMonitor;
		private List<ClientInfo> m_connectedClients;
		private List<ClientRequestInfo> m_clientRequestHistory;
		private List<ClientRequestHandlerInfo> m_clientRequestHandlers;
		
		private Process m_remoteCommandProcess;
		private CommunicationServerBase m_remotingServer;
		private TVA.IO.LogFile m_statusLog;
		private TVA.Scheduling.ScheduleManager m_scheduler;
		private TVA.ErrorManagement.GlobalExceptionLogger m_exceptionLogger;
		
		#endregion
		
		#region " Constants "
		
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
		public const bool DefaultMonitorServiceHealth = false;
		
		/// <summary>
		/// Default value for RequestHistoryLimit property.
		/// </summary>
		public const int DefaultRequestHistoryLimit = 50;
		
		/// <summary>
		/// Default value for QueryableSettingsCategories property.
		/// </summary>
		public const string DefaultQueryableSettingsCategories = "ServiceHelper, StatusLog, ExceptionLogger";
		
		/// <summary>
		/// Default value for PersistSettings property.
		/// </summary>
		public const bool DefaultPersistSettings = false;
		
		/// <summary>
		/// Default value for SettingsCategoryName property.
		/// </summary>
		public const string DefaultSettingsCategoryName = "ServiceHelper";
		
		#endregion
		
		#region " Events "
		
		/// <summary>
		/// Occurs when the service is starting.
		/// </summary>
		public delegate void ServiceStartingEventHandler(object Of);
		private ServiceStartingEventHandler ServiceStartingEvent;
		
		public event ServiceStartingEventHandler ServiceStarting
		{
			add
			{
				ServiceStartingEvent = (ServiceStartingEventHandler) System.Delegate.Combine(ServiceStartingEvent, value);
			}
			remove
			{
				ServiceStartingEvent = (ServiceStartingEventHandler) System.Delegate.Remove(ServiceStartingEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service has started.
		/// </summary>
		private EventHandler ServiceStartedEvent;
		public event EventHandler ServiceStarted
		{
			add
			{
				ServiceStartedEvent = (EventHandler) System.Delegate.Combine(ServiceStartedEvent, value);
			}
			remove
			{
				ServiceStartedEvent = (EventHandler) System.Delegate.Remove(ServiceStartedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service is stopping.
		/// </summary>
		private EventHandler ServiceStoppingEvent;
		public event EventHandler ServiceStopping
		{
			add
			{
				ServiceStoppingEvent = (EventHandler) System.Delegate.Combine(ServiceStoppingEvent, value);
			}
			remove
			{
				ServiceStoppingEvent = (EventHandler) System.Delegate.Remove(ServiceStoppingEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service has stopped.
		/// </summary>
		private EventHandler ServiceStoppedEvent;
		public event EventHandler ServiceStopped
		{
			add
			{
				ServiceStoppedEvent = (EventHandler) System.Delegate.Combine(ServiceStoppedEvent, value);
			}
			remove
			{
				ServiceStoppedEvent = (EventHandler) System.Delegate.Remove(ServiceStoppedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service is pausing.
		/// </summary>
		private EventHandler ServicePausingEvent;
		public event EventHandler ServicePausing
		{
			add
			{
				ServicePausingEvent = (EventHandler) System.Delegate.Combine(ServicePausingEvent, value);
			}
			remove
			{
				ServicePausingEvent = (EventHandler) System.Delegate.Remove(ServicePausingEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service has paused.
		/// </summary>
		private EventHandler ServicePausedEvent;
		public event EventHandler ServicePaused
		{
			add
			{
				ServicePausedEvent = (EventHandler) System.Delegate.Combine(ServicePausedEvent, value);
			}
			remove
			{
				ServicePausedEvent = (EventHandler) System.Delegate.Remove(ServicePausedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service is resuming.
		/// </summary>
		private EventHandler ServiceResumingEvent;
		public event EventHandler ServiceResuming
		{
			add
			{
				ServiceResumingEvent = (EventHandler) System.Delegate.Combine(ServiceResumingEvent, value);
			}
			remove
			{
				ServiceResumingEvent = (EventHandler) System.Delegate.Remove(ServiceResumingEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the service has resumed.
		/// </summary>
		private EventHandler ServiceResumedEvent;
		public event EventHandler ServiceResumed
		{
			add
			{
				ServiceResumedEvent = (EventHandler) System.Delegate.Combine(ServiceResumedEvent, value);
			}
			remove
			{
				ServiceResumedEvent = (EventHandler) System.Delegate.Remove(ServiceResumedEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when the system is being shutdowm.
		/// </summary>
		private EventHandler SystemShutdownEvent;
		public event EventHandler SystemShutdown
		{
			add
			{
				SystemShutdownEvent = (EventHandler) System.Delegate.Combine(SystemShutdownEvent, value);
			}
			remove
			{
				SystemShutdownEvent = (EventHandler) System.Delegate.Remove(SystemShutdownEvent, value);
			}
		}
		
		
		/// <summary>
		/// Occurs when a request is received from a client.
		/// </summary>
		public delegate void ReceivedClientRequestEventHandler(object sender, GenericEventArgs<IdentifiableItem<Guid, ClientRequest>> e);
		private ReceivedClientRequestEventHandler ReceivedClientRequestEvent;
		
		public event ReceivedClientRequestEventHandler ReceivedClientRequest
		{
			add
			{
				ReceivedClientRequestEvent = (ReceivedClientRequestEventHandler) System.Delegate.Combine(ReceivedClientRequestEvent, value);
			}
			remove
			{
				ReceivedClientRequestEvent = (ReceivedClientRequestEventHandler) System.Delegate.Remove(ReceivedClientRequestEvent, value);
			}
		}
		
		
		#endregion
		
		#region " Properties "
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether status updates are to be logged to a text file.
		/// </summary>
		/// <value></value>
		/// <returns>True if status updates are to be logged to a text file; otherwise false.</returns>
		[Category("Preferences"), DefaultValue(DefaultLogStatusUpdates)]public bool LogStatusUpdates
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
		[Category("Preferences"), DefaultValue(DefaultMonitorServiceHealth)]public bool MonitorServiceHealth
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
		[Category("Preferences"), DefaultValue(DefaultRequestHistoryLimit)]public int RequestHistoryLimit
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
					throw (new ArgumentOutOfRangeException("RequestHistoryLimit", "Value must be greater that 0."));
				}
			}
		}
		
		/// <summary>
		/// Gets or sets a comma seperated list of config file settings categories updateable by the service.
		/// </summary>
		/// <value></value>
		/// <returns>A comma seperated list of config file settings categories updateable by the service.</returns>
		[Category("Preferences"), DefaultValue(DefaultQueryableSettingsCategories)]public string QueryableSettingsCategories
		{
			get
			{
				return m_queryableSettingsCategories;
			}
			set
			{
				if (! string.IsNullOrEmpty(value))
				{
					m_queryableSettingsCategories = value;
				}
				else
				{
					throw (new ArgumentNullException("QueryableSettingsCategories"));
				}
			}
		}
		
		[Category("Persistance"), DefaultValue(DefaultPersistSettings)]public bool PersistSettings
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
		
		[Category("Persistance"), DefaultValue(DefaultSettingsCategoryName)]public string SettingsCategoryName
		{
			get
			{
				return m_settingsCategoryName;
			}
			set
			{
				if (! string.IsNullOrEmpty(value))
				{
					m_settingsCategoryName = value;
				}
				else
				{
					throw (new ArgumentNullException("SettingsCategoryName"));
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the parent service to which the service helper belongs.
		/// </summary>
		/// <value></value>
		/// <returns>The parent service to which the service helper belongs.</returns>
		[Category("Components")]public ServiceBase Service
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
		[Category("Components")]public CommunicationServerBase RemotingServer
		{
			get
			{
				return m_remotingServer;
			}
			set
			{
				m_remotingServer = value;
				m_remotingServer.ClientDisconnected += new System.EventHandler`1[[TVA.GenericEventArgs`1[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingServer_ClientDisconnected);
				m_remotingServer.ReceivedClientData += new System.EventHandler`1[[TVA.GenericEventArgs`1[[TVA.IdentifiableItem`2[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingServer_ReceivedClientData);
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]public TVA.Scheduling.ScheduleManager Scheduler
		{
			get
			{
				return m_scheduler;
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]public TVA.IO.LogFile StatusLog
		{
			get
			{
				return m_statusLog;
			}
		}
		
		[Category("Components"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]public TVA.ErrorManagement.GlobalExceptionLogger ExceptionLogger
		{
			get
			{
				return m_exceptionLogger;
			}
		}
		
		/// <summary>
		/// Gets a list of all the components that implement the TVA.Services.IServiceComponent interface.
		/// </summary>
		/// <value></value>
		/// <returns>An instance of System.Collections.Generic.List(Of TVA.Services.IServiceComponent).</returns>
		[Browsable(false)]public List<IServiceComponent> ServiceComponents
		{
			get
			{
				return m_serviceComponents;
			}
		}
		
		[Browsable(false)]public List<ServiceProcess> Processes
		{
			get
			{
				return m_processes;
			}
		}
		
		public ServiceProcess Processes(string processName)
		{
			ServiceProcess match = null;
			foreach (ServiceProcess process in m_processes)
			{
				if (string.Compare(process.name, processName, true) == 0)
				{
					match = process;
					break;
				}
			}
			return match;
		}
		
		[Browsable(false)]public List<ClientInfo> ConnectedClients
		{
			get
			{
				return m_connectedClients;
			}
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
		
		[Browsable(false)]public List<ClientRequestInfo> ClientRequestHistory
		{
			get
			{
				return m_clientRequestHistory;
			}
		}
		
		[Browsable(false)]public List<ClientRequestHandlerInfo> ClientRequestHandlers
		{
			get
			{
				return m_clientRequestHandlers;
			}
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
		
		[Browsable(false)]public PerformanceMonitor PerformanceMonitor
		{
			get
			{
				return m_performanceMonitor;
			}
		}
		
		#endregion
		
		#region " Methods "
		
		/// <summary>
		/// To be called when the service is starts (inside the service's OnStart method).
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]public void OnStart(string[] args)
		{
			
			if (m_service != null)
			{
				if (ServiceStartingEvent != null)
					ServiceStartingEvent(this, new GenericEventArgs<object>(args));
				
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Clients", "Displays list of clients connected to the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowClients)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Settings", "Displays queryable service settings from config file", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowSettings)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Processes", "Displays list of service or system processes", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowProcesses)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Schedules", "Displays list of process schedules defined in the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowSchedules)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("History", "Displays list of requests received from the clients", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowRequestHistory)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Help", "Displays list of commands supported by the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowRequestHelp)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Status", "Displays the current service status", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowServiceStatus)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Start", "Start a service or system process", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(StartProcess)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Abort", "Aborts a service or system process", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(AbortProcess)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("UpdateSettings", "Updates service setting in the config file", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(UpdateSettings)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("ReloadSettings", "Reloads services settings from the config file", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ReloadSettings)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Reschedule", "Reschedules a process defined in the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(RescheduleProcess)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Unschedule", "Unschedules a process defined in the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(UnscheduleProcess)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("SaveSchedules", "Saves process schedules to the config file", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(SaveSchedules)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("LoadSchedules", "Loads process schedules from the config file", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(LoadSchedules)));
				m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Command", "Allows for a telnet-like remote command session", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(RemoteCommandSession), false));
				if (m_monitorServiceHealth)
				{
					// Advertise "Health" command only if monitoring service health is enabled.
					m_clientRequestHandlers.Add(new ClientRequestHandlerInfo("Health", "Displays a report of resource utilization for the service", new TVA.Services.ClientRequestHandlerInfo.HandlerMethodSignature(ShowHealthReport)));
				}
				
				m_serviceComponents.Add(m_scheduler);
				m_serviceComponents.Add(m_remotingServer);
				if (m_monitorServiceHealth)
				{
					m_performanceMonitor = new PerformanceMonitor();
				}
				
				if (m_remotingServer != null)
				{
					m_remotingServer.Handshake = true;
					m_remotingServer.HandshakePassphrase = m_service.ServiceName;
				}
				
				foreach (IServiceComponent component in m_serviceComponents)
				{
					if (component != null)
					{
						component.ServiceStateChanged(ServiceState.Started);
					}
				}
				
				if (m_logStatusUpdates)
				{
					m_statusLog.Open();
				}
				
				SendServiceStateChangedResponse(ServiceState.Started);
				
				if (ServiceStartedEvent != null)
					ServiceStartedEvent(this, EventArgs.Empty);
			}
			else
			{
				throw (new InvalidOperationException("Service cannot be started. The Service property of ServiceHelper is not set."));
			}
			
		}
		
		/// <summary>
		/// To be called when the service is stopped (inside the service's OnStop method).
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]public void OnStop()
		{
			
			if (ServiceStoppingEvent != null)
				ServiceStoppingEvent(this, EventArgs.Empty);
			
			SendServiceStateChangedResponse(ServiceState.Stopped);
			
			// Abort any processes that are currently executing.
			foreach (ServiceProcess process in m_processes)
			{
				if (process != null)
				{
					process.Abort();
				}
			}
			
			// Notify all of the components that the service is stopping.
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
				{
					component.ServiceStateChanged(ServiceState.Stopped);
				}
			}
			
			if (m_statusLog.IsOpen)
			{
				m_statusLog.Close();
			}
			if (m_performanceMonitor != null)
			{
				m_performanceMonitor.Dispose();
			}
			
			// We do this to prevent any updates from being posted from other threads as this might cause exceptions.
			m_suppressUpdates = true;
			
			if (ServiceStoppedEvent != null)
				ServiceStoppedEvent(this, EventArgs.Empty);
			
		}
		
		/// <summary>
		/// To be called when the service is paused (inside the service's OnPause method).
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]public void OnPause()
		{
			
			if (ServicePausingEvent != null)
				ServicePausingEvent(this, EventArgs.Empty);
			
			SendServiceStateChangedResponse(ServiceState.Paused);
			
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
				{
					component.ServiceStateChanged(ServiceState.Paused);
				}
			}
			
			if (ServicePausedEvent != null)
				ServicePausedEvent(this, EventArgs.Empty);
			
		}
		
		/// <summary>
		/// To be called when the service is resumed (inside the service's OnContinue method).
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]public void OnResume()
		{
			
			if (ServiceResumingEvent != null)
				ServiceResumingEvent(this, EventArgs.Empty);
			
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
				{
					component.ServiceStateChanged(ServiceState.Resumed);
				}
			}
			
			SendServiceStateChangedResponse(ServiceState.Resumed);
			
			if (ServiceResumedEvent != null)
				ServiceResumedEvent(this, EventArgs.Empty);
			
		}
		
		/// <summary>
		/// To be when the system is shutting down (inside the service's OnShutdown method).
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]public void OnShutdown()
		{
			
			SendServiceStateChangedResponse(ServiceState.Shutdown);
			
			// Abort any processes that are executing.
			foreach (ServiceProcess process in m_processes)
			{
				if (process != null)
				{
					process.Abort();
				}
			}
			
			// Stop all of the components that implement IServiceComponent interface.
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
				{
					component.ServiceStateChanged(ServiceState.Shutdown);
				}
			}
			
			if (SystemShutdownEvent != null)
				SystemShutdownEvent(this, EventArgs.Empty);
			
		}
		
		/// <summary>
		/// To be called when the state of a process changes.
		/// </summary>
		/// <param name="processName">Name of the process whose state changed.</param>
		/// <param name="processState">New state of the process.</param>
		public void ProcessStateChanged(string processName, ProcessState processState)
		{
			
			foreach (IServiceComponent component in m_serviceComponents)
			{
				if (component != null)
				{
					component.ProcessStateChanged(processName, processState);
				}
			}
			
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
				m_remotingServer.Multicast(response);
			}
			else
			{
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
			
			if (! m_suppressUpdates)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append(message);
				
				for (int i = 0; i <= crlfCount - 1; i++)
				{
					with_1.AppendLine();
				}
				
				// Send the status update to all connected clients.
				SendUpdateClientStatusResponse(clientID, with_1.ToString());
				
				// Log the status update to the log file if logging is enabled.
				if (m_logStatusUpdates)
				{
					m_statusLog.WriteTimestampedLine(with_1.ToString());
				}
			}
			
		}
		
		public void AddProcess(ServiceProcess.ExecutionMethodSignature processExecutionMethod, string processName)
		{
			
			AddProcess(processExecutionMethod, processName, null);
			
		}
		
		public void AddProcess(ServiceProcess.ExecutionMethodSignature processExecutionMethod, string processName, object[] processParameters)
		{
			
			processName = processName.Trim();
			
			if (Processes(processName) == null)
			{
				m_processes.Add(new ServiceProcess(processExecutionMethod, processName, processParameters, this));
			}
			else
			{
				throw (new InvalidOperationException(string.Format("Process \"{0}\" is already defined.", processName)));
			}
			
		}
		
		public void AddScheduledProcess(ServiceProcess.ExecutionMethodSignature processExecutionMethod, string processName, string processSchedule)
		{
			
			AddScheduledProcess(processExecutionMethod, processName, null, processSchedule);
			
		}
		
		public void AddScheduledProcess(ServiceProcess.ExecutionMethodSignature processExecutionMethod, string processName, object[] processParameters, string processSchedule)
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
					if (updateExistingSchedule)
					{
						// Update the process schedule if it is already exists.
						existingSchedule.Rule = scheduleRule;
					}
				}
				else
				{
					// Schedule the process if it is not scheduled already.
					m_scheduler.Schedules.Add(new Schedule(processName, scheduleRule));
				}
			}
			else
			{
				throw (new InvalidOperationException(string.Format("Process \"{0}\" is not defined.", processName)));
			}
			
		}
		
		public void LoadSettings()
		{
			
			try
			{
				TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
				if (with_1.Count > 0)
				{
					m_pursip = with_1.Item("Pursip").GetTypedValue(m_pursip);
					LogStatusUpdates = with_1.Item("LogStatusUpdates").GetTypedValue(m_logStatusUpdates);
					MonitorServiceHealth = with_1.Item("MonitorServiceHealth").GetTypedValue(m_monitorServiceHealth);
					RequestHistoryLimit = with_1.Item("RequestHistoryLimit").GetTypedValue(m_requestHistoryLimit);
					QueryableSettingsCategories = with_1.Item("QueryableSettingsCategories").GetTypedValue(m_queryableSettingsCategories);
				}
			}
			catch (Exception)
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
					TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
					with_1.Clear();
					object with_2 = with_1.Item("Pursip", true);
					with_2.Value = m_pursip;
					with_2.Description = "";
					with_2.Encrypted = true;
					object with_3 = with_1.Item("LogStatusUpdates", true);
					with_3.Value = m_logStatusUpdates.ToString();
					with_3.Description = "";
					object with_4 = with_1.Item("MonitorServiceHealth", true);
					with_4.Value = m_monitorServiceHealth.ToString();
					with_4.Description = "";
					object with_5 = with_1.Item("RequestHistoryLimit", true);
					with_5.Value = m_requestHistoryLimit.ToString();
					with_5.Description = "";
					object with_6 = with_1.Item("QueryableSettingsCategories", true);
					with_6.Value = m_queryableSettingsCategories;
					with_6.Description = "";
					TVA.Configuration.Common.SaveSettings();
				}
				catch (Exception)
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
			
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				LoadSettings(); // Load settings from the config file.
			}
			
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
			ServiceResponse.Attachments.Add(new ObjectState<ServiceState>(m_service.ServiceName, currentState));
			
			SendResponse(serviceResponse);
			
		}
		
		private void SendProcessStateChangedResponse(string processName, ProcessState currentState)
		{
			
			ServiceResponse serviceResponse = new ServiceResponse("PROCESSSTATECHANGED");
			ServiceResponse.Attachments.Add(new ObjectState<ProcessState>(processName, currentState));
			
			SendResponse(serviceResponse);
			
		}
		
		#endregion
		
		#region " Handlers "
		
		private void ShowClients(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of clients currently connected to the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Clients -options");
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
				if (m_connectedClients.Count > 0)
				{
					// Display info about all of the clients connected to the service.
					System.Text.StringBuilder with_2 = new StringBuilder();
					with_2.AppendFormat("Clients connected to {0}:", m_service.ServiceName);
					with_2.AppendLine();
					with_2.AppendLine();
					with_2.Append("Client".PadRight(25));
					with_2.Append(' ');
					with_2.Append("Machine".PadRight(15));
					with_2.Append(' ');
					with_2.Append("User".PadRight(15));
					with_2.Append(' ');
					with_2.Append("Connected".PadRight(20));
					with_2.AppendLine();
					with_2.Append(new string('-', 25));
					with_2.Append(' ');
					with_2.Append(new string('-', 15));
					with_2.Append(' ');
					with_2.Append(new string('-', 15));
					with_2.Append(' ');
					with_2.Append(new string('-', 20));
					foreach (ClientInfo clientInfo in m_connectedClients)
					{
						with_2.AppendLine();
						if (! string.IsNullOrEmpty(clientInfo.ClientName))
						{
							with_2.Append(clientInfo.ClientName.PadRight(25));
						}
						else
						{
							with_2.Append("[Not Available]".PadRight(25));
						}
						with_2.Append(' ');
						if (! string.IsNullOrEmpty(clientInfo.MachineName))
						{
							with_2.Append(clientInfo.MachineName.PadRight(15));
						}
						else
						{
							with_2.Append("[Not Available]".PadRight(15));
						}
						with_2.Append(' ');
						if (! string.IsNullOrEmpty(clientInfo.UserName))
						{
							with_2.Append(clientInfo.UserName.PadRight(15));
						}
						else
						{
							with_2.Append("[Not Available]".PadRight(15));
						}
						with_2.Append(' ');
						with_2.Append(clientInfo.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
				}
				else
				{
					// This will never be the case because at the least the client sending the request will be connected.
					UpdateStatus(requestInfo.Sender.ClientID, string.Format("No clients are connected to {0}", m_service.ServiceName), UpdateCrlfCount);
				}
			}
			
		}
		
		private void ShowSettings(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of queryable settings of the service from the config file.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Settings -options");
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
				string[] settingsCategories = m_queryableSettingsCategories.Replace(" ", "").Split(',');
				if (settingsCategories.Length > 0)
				{
					// Display info about all of the queryable settings defined in the service.
					System.Text.StringBuilder with_2 = new StringBuilder();
					with_2.AppendFormat("Queryable settings of {0}:", m_service.ServiceName);
					with_2.AppendLine();
					with_2.AppendLine();
					with_2.Append("Category".PadRight(25));
					with_2.Append(' ');
					with_2.Append("Name".PadRight(20));
					with_2.Append(' ');
					with_2.Append("Value".PadRight(30));
					with_2.AppendLine();
					with_2.Append(new string('-', 25));
					with_2.Append(' ');
					with_2.Append(new string('-', 20));
					with_2.Append(' ');
					with_2.Append(new string('-', 30));
					foreach (string category in settingsCategories)
					{
						foreach (CategorizedSettingsElement setting in TVA.Configuration.Common.CategorizedSettings(category))
						{
							with_2.AppendLine();
							with_2.Append(category.PadRight(25));
							with_2.Append(' ');
							with_2.Append(setting.Name.PadRight(20));
							with_2.Append(' ');
							if (! string.IsNullOrEmpty(setting.Value))
							{
								with_2.Append(setting.Value.PadRight(30));
							}
							else
							{
								with_2.Append("[Not Set]".PadRight(30));
							}
						}
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
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
				
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of defined service processes or running system processes.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Processes -options");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Options:");
				with_1.AppendLine();
				with_1.Append("       -".PadRight(20));
				with_1.Append("Displays this help message");
				if (showAdvancedHelp)
				{
					with_1.AppendLine();
					with_1.Append("       -system".PadRight(20));
					with_1.Append("Displays system processes instead of service processes");
				}
				
				UpdateStatus(requestInfo.Sender.ClientID, with_1.ToString(), UpdateCrlfCount);
			}
			else
			{
				bool listSystemProcesses = requestInfo.Request.Arguments.Exists("system");
				
				if (! listSystemProcesses)
				{
					if (m_processes.Count > 0)
					{
						// Display info about all the processes defined in the service.
						System.Text.StringBuilder with_2 = new StringBuilder();
						with_2.AppendFormat("Processes defined in {0}:", m_service.ServiceName);
						with_2.AppendLine();
						with_2.AppendLine();
						with_2.Append("Name".PadRight(20));
						with_2.Append(' ');
						with_2.Append("State".PadRight(15));
						with_2.Append(' ');
						with_2.Append("Last Exec. Start".PadRight(20));
						with_2.Append(' ');
						with_2.Append("Last Exec. Stop".PadRight(20));
						with_2.AppendLine();
						with_2.Append(new string('-', 20));
						with_2.Append(' ');
						with_2.Append(new string('-', 15));
						with_2.Append(' ');
						with_2.Append(new string('-', 20));
						with_2.Append(' ');
						with_2.Append(new string('-', 20));
						foreach (ServiceProcess process in m_processes)
						{
							with_2.AppendLine();
							with_2.Append(process.name.PadRight(20));
							with_2.Append(' ');
							with_2.Append(process.CurrentState.ToString().PadRight(15));
							with_2.Append(' ');
							if (process.ExecutionStartTime != DateTime.MinValue)
							{
								with_2.Append(process.ExecutionStartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
							}
							else
							{
								with_2.Append("[Not Executed]".PadRight(20));
							}
							with_2.Append(' ');
							if (process.ExecutionStopTime != DateTime.MinValue)
							{
								with_2.Append(process.ExecutionStopTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
							}
							else
							{
								if (process.ExecutionStartTime != DateTime.MinValue)
								{
									with_2.Append("[Executing]".PadRight(20));
								}
								else
								{
									with_2.Append("[Not Executed]".PadRight(20));
								}
							}
						}
						
						UpdateStatus(requestInfo.Sender.ClientID, with_2.ToString(), UpdateCrlfCount);
					}
					else
					{
						// No processes defined in the service to be displayed.
						UpdateStatus(requestInfo.Sender.ClientID, string.Format("No processes are defined in {0}.", m_service.ServiceName), UpdateCrlfCount);
					}
				}
				else
				{
					System.Text.StringBuilder with_3 = new StringBuilder();
					with_3.AppendFormat("Processes running on {0}:", Environment.MachineName);
					with_3.AppendLine();
					with_3.AppendLine();
					with_3.Append("ID".PadRight(5));
					with_3.Append(' ');
					with_3.Append("Name".PadRight(25));
					with_3.Append(' ');
					with_3.Append("Priority".PadRight(15));
					with_3.Append(' ');
					with_3.Append("Responding".PadRight(10));
					with_3.Append(' ');
					with_3.Append("Start Time".PadRight(20));
					with_3.AppendLine();
					with_3.Append(new string('-', 5));
					with_3.Append(' ');
					with_3.Append(new string('-', 25));
					with_3.Append(' ');
					with_3.Append(new string('-', 15));
					with_3.Append(' ');
					with_3.Append(new string('-', 10));
					with_3.Append(' ');
					with_3.Append(new string('-', 20));
					foreach (System.Diagnostics.Process process in System.Diagnostics.Process.GetProcesses())
					{
						try
						{
							with_3.Append(process.StartInfo.UserName);
							with_3.AppendLine();
							with_3.Append(process.Id.ToString().PadRight(5));
							with_3.Append(' ');
							with_3.Append(process.ProcessName.PadRight(25));
							with_3.Append(' ');
							with_3.Append(process.PriorityClass.ToString().PadRight(15));
							with_3.Append(' ');
							with_3.Append(TVA.Common.IIf(process.Responding, "Yes", "No").PadRight(10));
							with_3.Append(' ');
							with_3.Append(process.StartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
						}
						catch (Exception)
						{
							
						}
					}
					
					UpdateStatus(requestInfo.Sender.ClientID, with_3.ToString(), UpdateCrlfCount);
				}
			}
			
		}
		
		private void ShowSchedules(ClientRequestInfo requestInfo)
		{
			
			if (requestInfo.Request.Arguments.ContainsHelpRequest)
			{
				System.Text.StringBuilder with_1 = new StringBuilder();
				with_1.Append("Displays a list of schedules for processes defined in the service.");
				with_1.AppendLine();
				with_1.AppendLine();
				with_1.Append("   Usage:");
				with_1.AppendLine();
				with_1.Append("       Schedules -options");
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
				if (m_scheduler.Schedules.Count > 0)
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
		
		private void m_statusLog_LogException(object sender, GenericEventArgs<System.Exception> e)
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
		
		private void m_remotingServer_ClientDisconnected(object sender, GenericEventArgs<System.Guid> e)
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
		
		private void m_remotingServer_ReceivedClientData(object sender, GenericEventArgs<IdentifiableItem<System.Guid, byte[]>> e)
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
							ReceivedClientRequestEvent(this, new GenericEventArgs<IdentifiableItem<Guid, ClientRequest>>(new IdentifiableItem<Guid, ClientRequest>(requestSender.ClientID, request)));
						
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
					m_exceptionLogger.Log(ex);
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
		
		#region " Obsolete "
		
		[Obsolete("Property is replaced by RemotingServer and will be deleted in version 3.3."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public CommunicationServerBase CommunicationServer
		{
			get
			{
				return m_remotingServer;
			}
			set
			{
				m_remotingServer = value;
				m_remotingServer.ClientDisconnected += new System.EventHandler`1[[TVA.GenericEventArgs`1[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingServer_ClientDisconnected);
				m_remotingServer.ReceivedClientData += new System.EventHandler`1[[TVA.GenericEventArgs`1[[TVA.IdentifiableItem`2[[System.Guid, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte[], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]], TVA.Core, Version=3.0.116.286, Culture=neutral, PublicKeyToken=null]](m_remotingServer_ReceivedClientData);
			}
		}
		
		[Obsolete("Property is replaced by StatusLog and will be deleted in version 3.3."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public TVA.IO.LogFile LogFile
		{
			get
			{
				return m_statusLog;
			}
		}
		
		[Obsolete("Property is replaced by Scheduler and will be deleted in version 3.3."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public TVA.Scheduling.ScheduleManager ScheduleManager
		{
			get
			{
				return m_scheduler;
			}
		}
		
		[Obsolete("Property is replaced by ExceptionLogger and will be deleted in version 3.3."), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]public TVA.ErrorManagement.GlobalExceptionLogger GlobalExceptionLogger
		{
			get
			{
				return m_exceptionLogger;
			}
		}
		
		#endregion
		
	}
}
