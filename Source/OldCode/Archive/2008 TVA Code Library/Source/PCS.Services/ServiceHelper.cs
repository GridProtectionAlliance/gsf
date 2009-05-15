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
//  03/06/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.ServiceProcess;
using System.Text;
using PCS.Communication;
using PCS.Configuration;
using PCS.Diagnostics;
using PCS.ErrorManagement;
using PCS.IO;
using PCS.Scheduling;

namespace PCS.Services
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the state of a Windows Service.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Service has started.
        /// </summary>
        Started,
        /// <summary>
        /// Service has stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// Service has paused.
        /// </summary>
        Paused,
        /// <summary>
        /// Service has resumed.
        /// </summary>
        Resumed,
        /// <summary>
        /// Service has shutdown.
        /// </summary>
        Shutdown
    }

    #endregion

    /// <summary>
    /// Component that provides added functionality to a Windows Service.
    /// </summary>
    /// <seealso cref="ServiceProcess"/>
    /// <seealso cref="ServiceResponse"/>
    /// <seealso cref="ClientInfo"/>
    /// <seealso cref="ClientRequest"/>
    /// <seealso cref="ClientRequestHandler"/>
    [ToolboxBitmap(typeof(ServiceHelper))]
    public class ServiceHelper : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
	{
		#region [ Members ]
		
        // Constants
		
		/// <summary>
        /// Specifies the default value for the <see cref="LogStatusUpdates"/> property.
		/// </summary>
		public const bool DefaultLogStatusUpdates = true;
		
		/// <summary>
        /// Specifies the default value for the <see cref="MonitorServiceHealth"/> property.
		/// </summary>
		public const bool DefaultMonitorServiceHealth = true;
		
		/// <summary>
        /// Specifies the default value for the <see cref="RequestHistoryLimit"/> property.
		/// </summary>
		public const int DefaultRequestHistoryLimit = 50;
		
		/// <summary>
        /// Specifies the default value for the <see cref="QueryableSettingsCategories"/> property.
		/// </summary>
		public const string DefaultQueryableSettingsCategories = "ServiceHelper, StatusLog, ErrorLogger";
		
		/// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
		/// </summary>
		public const bool DefaultPersistSettings = false;
		
		/// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
		/// </summary>
		public const string DefaultSettingsCategory = "ServiceHelper";

        // Events
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> is starting.
		/// </summary>
        [Category("Service"), 
        Description("Occurs when the ParentService is starting.")]
        public event EventHandler<EventArgs<string[]>> ServiceStarting;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> has started.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has started.")]		
        public event EventHandler ServiceStarted;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> is stopping.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is stopping.")]
        public event EventHandler ServiceStopping;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> has stopped.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has stopped.")]
        public event EventHandler ServiceStopped;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> is pausing.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is pausing.")]		
        public event EventHandler ServicePausing;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> has paused.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has paused.")]
        public event EventHandler ServicePaused;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> is resuming.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService is resuming.")]
        public event EventHandler ServiceResuming;
		
		/// <summary>
		/// Occurs when the <see cref="ParentService"/> has resumed.
		/// </summary>
        [Category("Service"),
        Description("Occurs when the ParentService has resumed.")]
        public event EventHandler ServiceResumed;
		
		/// <summary>
		/// Occurs when the system is being shutdown.
		/// </summary>
        [Category("System"),
        Description("Occurs when the system is being shutdown.")]
        public event EventHandler SystemShutdown;	
		
		/// <summary>
		/// Occurs when a request is received from one of the <see cref="RemoteClients"/>.
		/// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the remote client that sent the request.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="ClientRequest"/> sent by the remote client.
        /// </remarks>
        [Category("Client"),
        Description("Occurs when a request is received from one of the RemoteClients.")]
        public event EventHandler<EventArgs<Guid, ClientRequest>> ReceivedClientRequest;

        /// <summary>
        /// Occurs when the state of a defiend <see cref="ServiceProcess"/> changes.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="ServiceProcess.Name"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="ServiceProcess.CurrentState"/>
        /// </remarks>
        [Category("Process"),
        Description("Occurs when the state of a defiend ServiceProcess changes.")]
        public event EventHandler<EventArgs<string, ServiceProcessState>> ProcessStateChanged;

        // Fields
        private string m_telnetPassword;
		private bool m_logStatusUpdates;
		private bool m_monitorServiceHealth;
		private int m_requestHistoryLimit;
		private string m_queryableSettingsCategories;
		private bool m_persistSettings;
		private string m_settingsCategory;
		private ServiceBase m_parentService;
        private ServerBase m_remotingServer;
        private LogFile m_statusLog;
        private ScheduleManager m_processScheduler;
        private ErrorLogger m_errorLogger;
        private PerformanceMonitor m_performanceMonitor;
        private List<ServiceProcess> m_processes;
        private List<ISupportLifecycle> m_serviceComponents;
		private List<ClientInfo> m_remoteClients;
		private List<ClientRequest.Info> m_clientRequestHistory;
		private List<ClientRequestHandler> m_clientRequestHandlers;
        private Dictionary<ISupportLifecycle, bool> m_componentEnabledStates;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;       
        private bool m_suppressUpdates;
        private Guid m_remoteCommandClientID;
        private Process m_remoteCommandProcess;	
		
        #endregion

        #region [ Constructors ]
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        public ServiceHelper()
            : base()
		{
			m_logStatusUpdates = DefaultLogStatusUpdates;
			m_monitorServiceHealth = DefaultMonitorServiceHealth;
			m_requestHistoryLimit = DefaultRequestHistoryLimit;
			m_queryableSettingsCategories = DefaultQueryableSettingsCategories;
			m_persistSettings = DefaultPersistSettings;
			m_settingsCategory = DefaultSettingsCategory;
			m_processes = new List<ServiceProcess>();
			m_remoteClients = new List<ClientInfo>();
			m_clientRequestHistory = new List<ClientRequest.Info>();
			m_serviceComponents = new List<ISupportLifecycle>();
			m_clientRequestHandlers = new List<ClientRequestHandler>();
            m_componentEnabledStates = new Dictionary<ISupportLifecycle, bool>();
			m_telnetPassword = "s3cur3";

			// Components
			m_statusLog = new LogFile();
			m_statusLog.LogException += m_statusLog_LogException;
			m_statusLog.FileName = "StatusLog.txt";
			m_statusLog.SettingsCategory = "StatusLog";

            m_processScheduler = new ScheduleManager();
			m_processScheduler.ScheduleDue += m_scheduler_ScheduleDue;
			m_processScheduler.SettingsCategory = "ProcessScheduler";

			m_errorLogger = new ErrorLogger();
			m_errorLogger.ExitOnUnhandledException = false;
			m_errorLogger.SettingsCategory = "ErrorLogger";
            m_errorLogger.ErrorLog.SettingsCategory = "ErrorLog";
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ServiceHelper"/>.</param>
        public ServiceHelper(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]
		
        /// <summary>
        /// Gets or sets a boolean value that indicates whether messages sent using <see cref="UpdateStatus(string,object[])"/> 
        /// or <see cref="UpdateStatus(Guid,string,object[])"/> are to be logged to the <see cref="StatusLog"/>.
        /// </summary>
		[Category("Settings"), 
        DefaultValue(DefaultLogStatusUpdates), 
        Description("Indicates whether messages sent using UpdateStatus() method overloads are to be logged to the StatusLog.")]
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
        /// Gets or sets a boolean value that indicates whether the health of the <see cref="ParentService"/> is to be monitored.
        /// </summary>
        [Category("Settings"), 
        DefaultValue(DefaultMonitorServiceHealth),
        Description("Indicates whether the health of the ParentService is to be monitored.")]
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
        /// Gets or sets the maximum number of <see cref="ClientRequest"/> entries to be maintained in the <see cref="ClientRequestHistory"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being set is zero or negative.</exception>
        [Category("Settings"), 
        DefaultValue(DefaultRequestHistoryLimit), 
        Description("Maximum number of ClientRequest entries to be maintained in the ClientRequestHistory.")]
        public int RequestHistoryLimit
		{
			get
			{
				return m_requestHistoryLimit;
			}
			set
			{
                if (value < 1)
                    throw new ArgumentOutOfRangeException("RequestHistoryLimit", "Value must be greater that 0.");

                m_requestHistoryLimit = value;
			}
		}
		
        /// <summary>
        /// Gets or sets a comma-seperated list of <see cref="ConfigurationFile.Settings"/> section names whose settings can 
        /// be manupulated by the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is either a null empty string.</exception>
        [Category("Settings"), 
        DefaultValue(DefaultQueryableSettingsCategories), 
        Description("Comma-seperated list of ConfigurationFile.Settings section names whose settings can be manupulated by the ServiceHelper.")]
        public string QueryableSettingsCategories
		{
			get
			{
				return m_queryableSettingsCategories;
			}
			set
			{
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("QueryableSettingsCategories");

                m_queryableSettingsCategories = value;
			}
		}

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ServiceHelper"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of ServiceHelper are to be saved to the config file.")]
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

        /// <summary>
        /// Gets or sets the category under which the settings of <see cref="ServiceHelper"/> are to be saved to the config file 
        /// if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value specified is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of ServiceHelper are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }
	
		/// <summary>
		/// Gets or sets the <see cref="ServiceBase"/> to which the <see cref="ServiceHelper"/> will provided added functionality.
		/// </summary>
		[Category("Components"), 
        Description("ServiceBase to which the ServiceHelper will provided added functionality.")]
        public ServiceBase ParentService
		{
			get
			{
				return m_parentService;
			}
			set
			{
				m_parentService = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="ServerBase"/> component used for communicating with <see cref="RemoteClients"/>.
		/// </summary>
		[Category("Components"), 
        Description("ServerBase component used for communicating with RemoteClients.")]
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
				    m_remotingServer.ReceiveClientDataComplete -= m_remotingServer_ReceiveClientDataComplete;
                }

				m_remotingServer = value;

                if (m_remotingServer != null)
                {
                    // Attach events to new instance
				    m_remotingServer.ClientDisconnected += m_remotingServer_ClientDisconnected;
				    m_remotingServer.ReceiveClientDataComplete += m_remotingServer_ReceiveClientDataComplete;
                }
			}
		}
		
        /// <summary>
        /// Gets the <see cref="ScheduleManager"/> component used for scheduling defined <see cref="ServiceProcess"/>.
        /// </summary>
		[Category("Components"), 
        Description("ScheduleManager component used for scheduling defined ServiceProcess."), 
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ScheduleManager ProcessScheduler
		{
			get
			{
				return m_processScheduler;
			}
		}
		
        /// <summary>
        /// Gets the <see cref="LogFile"/> component used for logging status messages to a text file.
        /// </summary>
		[Category("Components"), 
        Description("LogFile component used for logging status messages to a text file."), 
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LogFile StatusLog
		{
			get
			{
				return m_statusLog;
			}
		}
		
        /// <summary>
        /// Gets the <see cref="ErrorLogger"/> component used for logging errors encountered in the <see cref="ParentService"/>.
        /// </summary>
		[Category("Components"), 
        Description("ErrorLogger component used for logging errors encountered in the ParentService."), 
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ErrorLogger ErrorLogger
		{
			get
			{
				return m_errorLogger;
			}
		}

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHelper"/> is currently enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value)
                {
                    // Re-enable all service components.
                    bool state;
                    foreach (ISupportLifecycle component in m_serviceComponents)
                    {
                        if (component != null)
                        {
                            // Restore previous state.
                            if (m_componentEnabledStates.TryGetValue(component, out state))
                                component.Enabled = state;
                        }
                    }
                }
                else
                {
                    // Disable all service components.
                    m_componentEnabledStates.Clear();
                    foreach (ISupportLifecycle component in m_serviceComponents)
                    {
                        if (component != null)
                        {
                            m_componentEnabledStates.Add(component, component.Enabled);
                            component.Enabled = false;
                        }
                    }
                }

                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a list of components that implement the <see cref="ISupportLifecycle"/> interface used by the <see cref="ParentService"/>.
        /// </summary>
		[Browsable(false), 
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ISupportLifecycle> ServiceComponents
		{
			get
			{
				return m_serviceComponents;
			}
		}

		/// <summary>
		/// Gets a list of <see cref="ServiceProcess"/> defined in the <see cref="ServiceHelper"/>.
		/// </summary>
		[Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ServiceProcess> Processes
		{
			get
			{
				return m_processes;
			}
		}
		
        /// <summary>
        /// Gets a list of <see cref="ClientInfo"/> for remote clients connected to the <see cref="RemotingServer"/>.
        /// </summary>
		[Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientInfo> RemoteClients
		{
			get
			{
				return m_remoteClients;
			}
		}

        /// <summary>
        /// Gets a list of <see cref="ClientRequest.Info"/> for requests made by <see cref="RemoteClients"/>.
        /// </summary>
		[Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientRequest.Info> ClientRequestHistory
		{
			get
			{
				return m_clientRequestHistory;
			}
		}
		
        /// <summary>
        /// Gets a list of <see cref="ClientRequestHandler"/> registered for handling requests from <see cref="RemoteClients"/>.
        /// </summary>
		[Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public List<ClientRequestHandler> ClientRequestHandlers
		{
			get
			{
				return m_clientRequestHandlers;
			}
		}
		
        /// <summary>
        /// Gets the <see cref="PerformanceMonitor"/> object used for monitoring the health of the <see cref="ParentService"/>.
        /// </summary>
		[Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public PerformanceMonitor PerformanceMonitor
		{
			get
			{
				return m_performanceMonitor;
			}
		}

        /// <summary>
        /// Gets the unique identifier of the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                if (m_parentService == null)
                    return m_settingsCategory;
                else
                    return m_parentService.ServiceName;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                // Show the uptime for the windows service.
                if (m_remotingServer != null)
                {
                    status.AppendFormat("System Uptime: {0}", m_remotingServer.RunTime.ToString());
                    status.AppendLine();
                    status.AppendLine();
                }

                // Show the status of registered components.
                status.AppendFormat("Status of components used by {0}:", Name);
                status.AppendLine();
                status.AppendLine();
                foreach (ISupportLifecycle serviceComponent in m_serviceComponents)
                {
                    IProvideStatus statusProvider = serviceComponent as IProvideStatus;

                    if (statusProvider != null)
                    {
                        // This component provides status information.                       
                        status.AppendFormat("Status of {0}:", statusProvider.Name);
                        status.AppendLine();
                        status.Append(statusProvider.Status);
                        status.AppendLine();
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
				
        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnStart(string[])"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStart(string[] args)
        {
            // Ensure required components are present.
            if (m_parentService == null)
                throw new InvalidOperationException("ParentService property of ServiceHelper component is not set.");

            if (m_remotingServer == null)
                throw new InvalidOperationException("RemotingServer property of ServiceHelper component is not set.");

            OnServiceStarting(args);

            m_clientRequestHandlers.Add(new ClientRequestHandler("Clients", "Displays list of clients connected to the service", ShowClients));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Settings", "Displays queryable service settings from config file", ShowSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Processes", "Displays list of service or system processes", ShowProcesses));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Schedules", "Displays list of process schedules defined in the service", ShowSchedules));
            m_clientRequestHandlers.Add(new ClientRequestHandler("History", "Displays list of requests received from the clients", ShowRequestHistory));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Help", "Displays list of commands supported by the service", ShowRequestHelp));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Status", "Displays the current service status", ShowServiceStatus));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Start", "Start a service or system process", StartProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Abort", "Aborts a service or system process", AbortProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("UpdateSettings", "Updates service setting in the config file", UpdateSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("ReloadSettings", "Reloads services settings from the config file", ReloadSettings));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Reschedule", "Reschedules a process defined in the service", RescheduleProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Unschedule", "Unschedules a process defined in the service", UnscheduleProcess));
            m_clientRequestHandlers.Add(new ClientRequestHandler("SaveSchedules", "Saves process schedules to the config file", SaveSchedules));
            m_clientRequestHandlers.Add(new ClientRequestHandler("LoadSchedules", "Loads process schedules from the config file", LoadSchedules));
            m_clientRequestHandlers.Add(new ClientRequestHandler("Telnet", "Allows for a telnet session to the service server", RemoteTelnetSession, false));

            // Define "Health" command only if monitoring service health is enabled.
            if (m_monitorServiceHealth)
            {
                m_clientRequestHandlers.Add(new ClientRequestHandler("Health", "Displays a report of resource utilization for the service", ShowHealthReport));
                m_performanceMonitor = new PerformanceMonitor();
            }

            // Add internal components as service components by default.
            m_serviceComponents.Add(m_processScheduler);
            m_serviceComponents.Add(m_statusLog);
            m_serviceComponents.Add(m_errorLogger);
            m_serviceComponents.Add(m_remotingServer);

            // Initialize all service components when service has started.
            Initialize();

            // Open log file if file logging is enabled.
            if (m_logStatusUpdates)
                m_statusLog.Open();

            // Start the scheduler if it is not running.
            if (!m_processScheduler.IsRunning)
                m_processScheduler.Start();

            // Start the remoting server if it is not running.
            if (m_remotingServer.CurrentState == ServerState.NotRunning)
                m_remotingServer.Start();

            OnServiceStarted();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnStop()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStop()
        {
            OnServiceStopping();

            // Abort any processes that may be currently executing.
            foreach (ServiceProcess process in m_processes)
            {
                if (process != null)
                    process.Abort();
            }

            // Set flag to prevent status updates from being posted from other threads, at this point this might cause exceptions.
            m_suppressUpdates = true;

            OnServiceStopped();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnPause()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnPause()
        {
            OnServicePausing();

            // Disable all service components when service is pausing
            Enabled = false;

            OnServicePaused();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnContinue()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnResume()
        {
            OnServiceResuming();

            // Re-enable all service components when service is resuming
            Enabled = true;

            OnServiceResumed();
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnShutdown()"/> method of <see cref="ParentService"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnShutdown()
        {
            OnSystemShutdown();

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
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        public void AddProcess(Action<string, object[]> processExecutionMethod, string processName)
        {
            AddProcess(processExecutionMethod, processName, null);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        public void AddProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments)
        {
            processName = processName.Trim();

            if (GetProcess(processName) == null)
            {
                ServiceProcess process = new ServiceProcess(processExecutionMethod, processName, processArguments);
                process.StateChanged += process_StateChanged;

                m_processes.Add(process);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is already defined.", processName));
            }
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        public void AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, string processSchedule)
        {
            AddScheduledProcess(processExecutionMethod, processName, null, processSchedule);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        public void AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments, string processSchedule)
        {
            AddProcess(processExecutionMethod, processName, processArguments);
            ScheduleProcess(processName, processSchedule);
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        public void ScheduleProcess(string processName, string scheduleRule)
        {
            ScheduleProcess(processName, scheduleRule, false);
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        /// <param name="updateExistingSchedule">true if the <see cref="ServiceProcess"/> is to be re-scheduled; otherwise false.</param>
        public void ScheduleProcess(string processName, string scheduleRule, bool updateExistingSchedule)
        {
            processName = processName.Trim();

            if (GetProcess(processName) != null)
            {
                // The specified process exists, so we'll schedule it, or update its schedule if it is acheduled already.
                Schedule existingSchedule = m_processScheduler.FindSchedule(processName);

                if (existingSchedule != null)
                {
                    // Update the process schedule if it is already exists.
                    if (updateExistingSchedule)
                        existingSchedule.Rule = scheduleRule;
                }
                else
                {
                    // Schedule the process if it is not scheduled already.
                    m_processScheduler.Schedules.Add(new Schedule(processName, scheduleRule));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Process \"{0}\" is not defined.", processName));
            }
        }

        /// <summary>
        /// Sends the specified <paramref name="response"/> to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="response">The <see cref="ServiceResponse"/> to be sent to all <see cref="RemoteClients"/>.</param>
        public void SendResponse(ServiceResponse response)
        {
            SendResponse(Guid.Empty, response);
        }

        /// <summary>
        /// Sends the specified <paramref name="response"/> to the specified <paramref name="client"/> only.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="response"/> is to be sent.</param>
        /// <param name="response">The <see cref="ServiceResponse"/> to be sent to the <paramref name="client"/>.</param>
        public void SendResponse(Guid client, ServiceResponse response)
        {
            try
            {
                if (client == Guid.Empty)
                {
                    // Multicast message to all clients.
                    if (m_remoteCommandClientID == Guid.Empty)
                        // Process if remote command session is not in progress.
                        m_remotingServer.MulticastAsync(response);
                }
                else
                {
                    // Send message directly to specified client.
                    m_remotingServer.SendToAsync(client, response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception.
                m_errorLogger.Log(ex);
            }
        }

        /// <summary>
        /// Provides a status update to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="message">Text message to be transmitted to all <see cref="RemoteClients"/>.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        public void UpdateStatus(string message, params object[] args)
        {
            UpdateStatus(Guid.Empty, message, args);
        }

        /// <summary>
        /// Provides a status update to the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="message"/> is to be sent.</param>
        /// <param name="message">Text message to be transmitted to the <paramref name="client"/>.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        public void UpdateStatus(Guid client, string message, params object[] args)
        {
            if (!m_suppressUpdates)
            {
                // Prepare the message.
                message = string.Format(message, args);

                // Send the status update to specified client(s)
                SendUpdateClientStatusResponse(client, message);

                // Log the status update to the log file if logging is enabled.
                if (m_logStatusUpdates)
                    m_statusLog.WriteTimestampedLine(message);
            }
        }

        /// <summary>
        /// Initializes the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ServiceHelper"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                // Initialize all service components.
                foreach (ISupportLifecycle component in m_serviceComponents)
                {
                    if (component != null)
                        component.Initialize();
                }

                m_initialized = true; // Initialize only once.
            }
        }

        /// <summary>
        /// Saves settings of the <see cref="ServiceHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                // Add settings if they don't exist in config file.
                settings.Add("LogStatusUpdates", m_logStatusUpdates, "True if status update messages are to be logged to a text file; otherwise False.");
                settings.Add("MonitorServiceHealth", m_monitorServiceHealth, "True if the service health is to be monitored; otherwise False.");
                settings.Add("RequestHistoryLimit", m_requestHistoryLimit, "Number of client request entries to be kept in the history.");
                settings.Add("QueryableSettingsCategories", m_queryableSettingsCategories, "Category names under categorizedSettings section of the config file that can be managed by the service.");
                // Update settings with the latest property values.
                element = settings["LogStatusUpdates"];
                element.Update(m_logStatusUpdates, element.Description, element.Encrypted);
                element = settings["MonitorServiceHealth"];
                element.Update(m_monitorServiceHealth, element.Description, element.Encrypted);
                element = settings["RequestHistoryLimit"];
                element.Update(m_requestHistoryLimit, element.Description, element.Encrypted);
                element = settings["QueryableSettingsCategories"];
                element.Update(m_queryableSettingsCategories, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings of the <see cref="ServiceHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                if (settings["TelnetPassword"] != null)
                    m_telnetPassword = settings["TelnetPassword"].ValueAs(m_telnetPassword);
                LogStatusUpdates = settings["LogStatusUpdates", true].ValueAs(m_logStatusUpdates);
                MonitorServiceHealth = settings["MonitorServiceHealth", true].ValueAs(m_monitorServiceHealth);
                RequestHistoryLimit = settings["RequestHistoryLimit", true].ValueAs(m_requestHistoryLimit);
                QueryableSettingsCategories = settings["QueryableSettingsCategories", true].ValueAs(m_queryableSettingsCategories);
            }
        }

        /// <summary>
        /// Performs necessary operations before the <see cref="ServiceHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ServiceHelper"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="ServiceHelper"/> properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="ServiceHelper"/> is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ServiceProcess"/> for the specified <paramref name="processName"/>.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be retrieved.</param>
        /// <returns><see cref="ServiceProcess"/> object if found; otherwise null.</returns>
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

        /// <summary>
        /// Gets the <see cref="ClientInfo"/> object for the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">ID of the client whose <see cref="ClientInfo"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientInfo"/> object if found; otherwise null.</returns>
        public ClientInfo GetConnectedClient(Guid client)
        {
            ClientInfo match = null;

            foreach (ClientInfo clientInfo in m_remoteClients)
            {
                if (client == clientInfo.ClientID)
                {
                    match = clientInfo;
                    break;
                }
            }

            return match;
        }

        /// <summary>
        /// Gets the <see cref="ClientRequestHandler"/> object for the specified <paramref name="requestType"/>.
        /// </summary>
        /// <param name="requestType">Request type whose <see cref="ClientRequestHandler"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientRequestHandler"/> object if found; otherwise null.</returns>
        public ClientRequestHandler GetClientRequestHandler(string requestType)
        {
            ClientRequestHandler match = null;

            foreach (ClientRequestHandler handler in m_clientRequestHandlers)
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
        /// Releases the unmanaged resources used by the <see cref="ServiceHelper"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if (m_statusLog != null)
                        {
                            m_statusLog.LogException -= m_statusLog_LogException;
                            m_statusLog.Dispose();
                        }
                        m_statusLog = null;

                        if (m_processScheduler != null)
                        {
                            m_processScheduler.ScheduleDue -= m_scheduler_ScheduleDue;
                            m_processScheduler.Dispose();
                        }
                        m_processScheduler = null;

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
                                process.StateChanged -= process_StateChanged;
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

        /// <summary>
        /// Raises the <see cref="ServiceStarting"/> event.
        /// </summary>
        /// <param name="args">Arguments to be sent to <see cref="ServiceStarting"/> event.</param>
        protected virtual void OnServiceStarting(string[] args)
        {
            // Notify service event consumers of pending service start
            if (ServiceStarting != null)
                ServiceStarting(this, new EventArgs<string[]>(args));
        }

        /// <summary>
        /// Raises the <see cref="ServiceStarted"/> event.
        /// </summary>
        protected virtual void OnServiceStarted()
        {
            // Notify all remote clients that might possibly be connected at of service start (not likely)
            SendServiceStateChangedResponse(ServiceState.Started);

            // Notify service event consumers that service has started
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStopping"/> event.
        /// </summary>
        protected virtual void OnServiceStopping()
        {
            // Notify service event consumers of pending service stop
            if (ServiceStopping != null)
                ServiceStopping(this, EventArgs.Empty);

            // Notify all remote clients of service stop
            SendServiceStateChangedResponse(ServiceState.Stopped);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStopped"/> event.
        /// </summary>
        protected virtual void OnServiceStopped()
        {
            // Notify service event consumers that service has stopped
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServicePausing"/> event.
        /// </summary>
        protected virtual void OnServicePausing()
        {
            // Notify service event consumers of pending service stop
            if (ServicePausing != null)
                ServicePausing(this, EventArgs.Empty);

            // Notify all remote clients of service pause
            SendServiceStateChangedResponse(ServiceState.Paused);
        }

        /// <summary>
        /// Raises the <see cref="ServicePaused"/> event.
        /// </summary>
        protected virtual void OnServicePaused()
        {
            // Notify service event consumers that service has been paused
            if (ServicePaused != null)
                ServicePaused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceResuming"/> event.
        /// </summary>
        protected virtual void OnServiceResuming()
        {
            // Notify service event consumers of pending service resume
            if (ServiceResuming != null)
                ServiceResuming(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceResumed"/> event.
        /// </summary>
        protected virtual void OnServiceResumed()
        {
            // Notify all remote clients of service resume
            SendServiceStateChangedResponse(ServiceState.Resumed);

            // Notify service event consumers that service has been resumed
            if (ServiceResumed != null)
                ServiceResumed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SystemShutdown"/> event.
        /// </summary>
        protected virtual void OnSystemShutdown()
        {
            // Notify service event consumers of pending service shutdown
            SendServiceStateChangedResponse(ServiceState.Shutdown);

            // Notify service event consumers that service has shutdown
            if (SystemShutdown != null)
                SystemShutdown(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ReceivedClientRequest"/> event.
        /// </summary>
        /// <param name="request">The <see cref="ClientRequest"/> that was received.</param>
        /// <param name="requestSender">The <see cref="ClientInfo"/> object of the <paramref name="request"/> sender.</param>
        protected virtual void OnReceivedClientRequest(ClientRequest request, ClientInfo requestSender)
        {
            if (ReceivedClientRequest != null)
                ReceivedClientRequest(this, new EventArgs<Guid, ClientRequest>(requestSender.ClientID, request));
        }

        /// <summary>
        /// Raises the <see cref="ProcessStateChanged"/> event.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> whose state changed.</param>
        /// <param name="processState">New <see cref="ServiceProcessState"/> of the <see cref="ServiceProcess"/>.</param>
        protected virtual void OnProcessStateChanged(string processName, ServiceProcessState processState)
        {
            // Notify all service event consumer of change in process state
            if (ProcessStateChanged != null)
                ProcessStateChanged(this, new EventArgs<string, ServiceProcessState>(processName, processState));

            // Notify all remote clients of change in process state
            SendProcessStateChangedResponse(processName, processState);
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

        private void SendProcessStateChangedResponse(string processName, ServiceProcessState currentState)
        {
            ServiceResponse serviceResponse = new ServiceResponse("PROCESSSTATECHANGED");
            serviceResponse.Attachments.Add(new ObjectState<ServiceProcessState>(processName, currentState));
            SendResponse(serviceResponse);
        }

        private void process_StateChanged(object sender, EventArgs e)
        {
            ServiceProcess process = sender as ServiceProcess;
            OnProcessStateChanged(process.Name, process.CurrentState);
        }

        private void m_statusLog_LogException(object sender, EventArgs<System.Exception> e)
        {
            // We'll let the connected clients know that we encountered an exception while logging the status update.
            m_logStatusUpdates = false;
            UpdateStatus("Error occurred while logging status update - {0}\r\n\r\n", e.Argument);
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
                        RemoteTelnetSession(new ClientRequest.Info(disconnectedClient, ClientRequest.Parse("Telnet -disconnect")));
                    }
                    catch (Exception ex)
                    {
                        // We'll encounter an exception because we'll try to update the status of the client that had the
                        // remote command session open and this will fail since the client is disconnected.
                        m_errorLogger.Log(ex);
                    }
                }

                m_remoteClients.Remove(disconnectedClient);
                UpdateStatus("Remote client disconnected - {0} from {1}.\r\n\r\n", disconnectedClient.UserName, disconnectedClient.MachineName);
            }
        }

        private void m_remotingServer_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            ClientInfo requestSender = GetConnectedClient(e.Argument1);

            if (requestSender == null)
            {
                // First message from a remote client should be its info.
                ClientInfo client = null;
                Serialization.TryGetObject<ClientInfo>(e.Argument2.BlockCopy(0, e.Argument3), out client);
                if (client != null)
                {
                    client.ClientID = e.Argument1;
                    client.ConnectedAt = DateTime.Now;
                    m_remoteClients.Add(client);
                    UpdateStatus("Remote client connected - {0} from {1}.\r\n\r\n", client.UserName, client.MachineName);
                }
                else
                {
                    try
                    {
                        m_remotingServer.DisconnectOne(e.Argument1);
                    }
                    catch (Exception ex)
                    {
                        m_errorLogger.Log(ex);
                    }
                }               
            }
            else
            { 
                // All subsequest messages from a remote client would be requests.
                ClientRequest request = null;
                Serialization.TryGetObject<ClientRequest>(e.Argument2.BlockCopy(0, e.Argument3), out request);
                if (request != null)
                {
                    try
                    {
                        ClientRequest.Info requestInfo = new ClientRequest.Info(requestSender, request);
                        if (m_remoteCommandClientID == Guid.Empty)
                        {
                            // Process all incoming requests when remote command session is not in progress.
                            m_clientRequestHistory.Add(requestInfo);

                            // We'll remove old request entries if we've exceeded the limit for request history.
                            if (m_clientRequestHistory.Count > m_requestHistoryLimit)
                                m_clientRequestHistory.RemoveRange(0, (m_clientRequestHistory.Count - m_requestHistoryLimit));

                            // Notify the consumer about the incoming request from client.
                            OnReceivedClientRequest(request, requestSender);

                            ClientRequestHandler requestHandler = GetClientRequestHandler(request.Command);
                            if (requestHandler != null)
                                requestHandler.HandlerMethod(requestInfo);
                            else
                                UpdateStatus(requestSender.ClientID, "Failed to process request \"{0}\" - Request is not supported.\r\n\r\n", request.Command);
                        }
                        else if (requestSender.ClientID == m_remoteCommandClientID)
                        {
                            // Redirect requests to remote command session if requests are from its originator.
                            RemoteTelnetSession(requestInfo);
                        }
                        else
                        {
                            // Reject all request from other clients since remote command session is in progress.
                            UpdateStatus(requestSender.ClientID, "Failed to process request \"{0}\" - Remote command session is in progress.\r\n\r\n", request.Command);
                        }
                    }
                    catch (Exception ex)
                    {
                        m_errorLogger.Log(ex);
                        UpdateStatus(requestSender.ClientID, "Failed to process request \"{0}\" - {1}.\r\n\r\n", request.Command, ex.Message);
                    }
                }
                else
                {
                    UpdateStatus(requestSender.ClientID, "Failed to process request - Request could not be deserialized.\r\n\r\n");
                }
            }
        }

        private void m_remoteCommandProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, e.Data + "\r\n");
        }

        private void m_remoteCommandProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, e.Data + "\r\n");
        }

        #region [ Client Request Handlers ]

        private void ShowClients(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                if (m_remoteClients.Count > 0)
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

                    foreach (ClientInfo clientInfo in m_remoteClients)
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
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, "No clients are connected to {0}\r\n\r\n", Name);
                }
            }
        }

        private void ShowSettings(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                            // Skip encrypted settings for security purpose.
                            if (setting.Encrypted)
                                continue;   

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
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
                }
                else
                {
                    // No queryable settings are defined in the service.
                    UpdateStatus(requestInfo.Sender.ClientID, "No queryable settings are defined in {0}.\r\n\r\n", Name);
                }
            }
        }

        private void ShowProcesses(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                if (showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Displays system processes instead of service processes");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                        responseMessage.AppendLine();
                        responseMessage.AppendLine();

                        UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
                    }
                    else
                    {
                        // No processes defined in the service to be displayed.
                        UpdateStatus(requestInfo.Sender.ClientID, "No processes are defined in {0}.\r\n\r\n", Name);
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
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
                }
            }
        }

        private void ShowSchedules(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                if (m_processScheduler.Schedules.Count > 0)
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

                    foreach (Schedule schedule in m_processScheduler.Schedules)
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
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, "No process schedules are defined in {0}.\r\n\r\n", Name);
                }
            }
        }

        private void ShowRequestHistory(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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

                foreach (ClientRequest.Info historicRequest in m_clientRequestHistory)
                {
                    responseMessage.AppendLine();
                    responseMessage.Append(historicRequest.Request.Command.PadRight(20));
                    responseMessage.Append(' ');
                    responseMessage.Append(historicRequest.ReceivedAt.ToString().PadRight(25));
                    responseMessage.Append(' ');
                    responseMessage.Append(string.Format("{0} from {1}", historicRequest.Sender.UserName, historicRequest.Sender.MachineName).PadRight(30));
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
            }
        }

        private void ShowRequestHelp(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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

                foreach (ClientRequestHandler handler in m_clientRequestHandlers)
                {
                    if (handler.IsAdvertised || showAdvancedHelp)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(handler.Command.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(handler.CommandDescription.PadRight(55));
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, responseMessage.ToString());
            }
        }

        private void ShowHealthReport(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                if (m_performanceMonitor != null)
                    UpdateStatus(requestInfo.Sender.ClientID, m_performanceMonitor.Status + "\r\n");
                else
                    UpdateStatus(requestInfo.Sender.ClientID, "Performance monitor is not available.");
            }
        }

        private void ShowServiceStatus(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                UpdateStatus(requestInfo.Sender.ClientID, Status);
            }
        }

        private void ReloadSettings(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Category name must be defined as one of the queryable settings categories in the QueryableSettingsCategories property of ServiceHelper. ");
                helpMessage.Append("Also, category name is case sensitive so it must be the same case as it appears in the settings listing.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully loaded settings for category \"{0}\".\r\n\r\n", categoryName);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully loaded settings for component \"{0}\" from category \"{1}\".\r\n\r\n", settingsTarget, categoryName);
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to load component settings from category \"{0}\". No corresponding settings category name found.\r\n\r\n", categoryName);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, "Failed to load component settings from category \"{0}\". Category is not one of the queryable settings categories.\r\n\r\n", categoryName);
                }
            }
        }

        private void UpdateSettings(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
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
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                            UpdateStatus(requestInfo.Sender.ClientID, "Attempting to add setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                            settings.Add(settingName, settingValue);
                            ConfigurationFile.Current.Save();
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully added setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                        }
                        else if (true == deleteSetting)
                        {
                            setting = settings[settingName];
                            if (setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to delete setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                settings.Remove(setting);
                                ConfigurationFile.Current.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, "Successfully deleted setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, "Failed to delete setting \"{0}\" under category \"{1}\". Setting does not exist.\r\n\r\n", settingName, categoryName);
                            }
                        }
                        else
                        {
                            setting = settings[settingName];
                            if (setting != null)
                            {
                                // The requested setting does exist under the specified category.
                                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to update setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                setting.Value = settingValue;
                                ConfigurationFile.Current.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, "Successfully updated setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                // The requested setting does not exist under the specified category.
                                UpdateStatus(requestInfo.Sender.ClientID, "Failed to update value of setting \"{0}\" under category \"{1}\" . Setting does not exist.\r\n\r\n", settingName, categoryName);
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
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to update value of setting \"{0}\" under category \"{1}\". Category does not exist.\r\n\r\n", settingName, categoryName);
                    }
                }
                else
                {
                    // The specified category is not one of the defined queryable categories.
                    UpdateStatus(requestInfo.Sender.ClientID, "Failed to update value of setting \"{0}\" under category \"{1}\". Category is not one of the queryable categories.\r\n\r\n", settingName, categoryName);
                }
            }
        }

        private void StartProcess(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -args".PadRight(20));
                helpMessage.Append("Arguments to be passed in to the process");
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
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                string processArgs = requestInfo.Request.Arguments["args"];
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
                        if (processToStart.CurrentState != ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, "Attempting to start service process \"{0}\"...\r\n\r\n", processName);
                            if (string.IsNullOrEmpty(processArgs))
                            {
                                processToStart.Start();
                            }
                            else
                            {
                                // Prepare the arguments.
                                string[] splitArgs = processArgs.Split(',');
                                Array.ForEach<string>(splitArgs, (string arg) => arg.Trim());

                                // Start the service process.
                                processToStart.Start(splitArgs);
                            }
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully started service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, "Failed to start process \"{0}\". Process is already executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to start service process \"{0}\". Process is not defined.\r\n\r\n", processName);
                    }
                }
                else
                {
                    try
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, "Attempting to start system process \"{0}\"...\r\n\r\n", processName);
                        Process startedProcess = Process.Start(processName, processArgs);

                        if (startedProcess != null)
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully started system process \"{0}\".\r\n\r\n", processName);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, "Failed to start system process \"{0}\".\r\n\r\n", processName);
                    }
                    catch (Exception ex)
                    {
                        m_errorLogger.Log(ex);
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to start system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void AbortProcess(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
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
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                        if (processToAbort.CurrentState == ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, "Attempting to abort service process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Abort();
                            UpdateStatus(requestInfo.Sender.ClientID, "Successfully aborted service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, "Failed to abort service process \"{0}\". Process is not executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to abort service process \"{0}\". Process is not defined.\r\n\r\n", processName);
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
                            UpdateStatus(requestInfo.Sender.ClientID, "Attempting to abort system process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Kill();
                            if (processToAbort.WaitForExit(10000))
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, "Successfully aborted system process \"{0}\".\r\n\r\n", processName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, "Failed to abort system process \"{0}\". Process not responding.\r\n\r\n", processName);
                            }
                        }
                        catch (Exception ex)
                        {
                            m_errorLogger.Log(ex);
                            UpdateStatus(requestInfo.Sender.ClientID, "Failed to abort system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, "Failed to abort system process \"{0}\". Process is not running.\r\n\r\n", processName);
                    }
                }

                if (listProcesses)
                {
                    requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                    ShowProcesses(requestInfo);
                }
            }
        }

        private void RescheduleProcess(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
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
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
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
                    UpdateStatus(requestInfo.Sender.ClientID, "Attempting to schedule process \"{0}\" with rule \"{1}\"...\r\n\r\n", processName, scheduleRule);
                    ScheduleProcess(processName, scheduleRule, true);
                    UpdateStatus(requestInfo.Sender.ClientID, "Successfully scheduled process \"{0}\" with rule \"{1}\".\r\n\r\n", processName, scheduleRule);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                catch (Exception ex)
                {
                    m_errorLogger.Log(ex);
                    UpdateStatus(requestInfo.Sender.ClientID, "Failed to schedule process \"{0}\". {1}\r\n\r\n", processName, ex.Message);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void UnscheduleProcess(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -save".PadRight(20));
                helpMessage.Append("Saves all process schedules to the config file");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                Schedule scheduleToRemove = m_processScheduler.FindSchedule(processName);

                if (scheduleToRemove != null)
                {
                    UpdateStatus(requestInfo.Sender.ClientID, "Attempting to unschedule process \"{0}\"...\r\n\r\n", processName);
                    m_processScheduler.Schedules.Remove(scheduleToRemove);
                    UpdateStatus(requestInfo.Sender.ClientID, "Successfully unscheduled process \"{0}\".\r\n\r\n", processName);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, "Failed to unschedule process \"{0}\". Process is not scheduled.\r\n\r\n", processName);
                }

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void SaveSchedules(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to save process schedules to the config file...\r\n\r\n");
                m_processScheduler.SaveSettings();
                UpdateStatus(requestInfo.Sender.ClientID, "Successfully saved process schedules to the config file.\r\n\r\n");

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void LoadSchedules(ClientRequest.Info requestInfo)
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, "Attempting to load process schedules from the config file...\r\n\r\n");
                m_processScheduler.LoadSettings();
                UpdateStatus(requestInfo.Sender.ClientID, "Successfully loaded process schedules from the config file.\r\n\r\n");

                if (listSchedules)
                {
                    requestInfo.Request = ClientRequest.Parse("Schedules");
                    ShowSchedules(requestInfo);
                }
            }
        }

        private void RemoteTelnetSession(ClientRequest.Info requestinfo)
        {
            if (m_remoteCommandProcess == null && requestinfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Allows for a telnet session to the service server.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Telnet -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -connect".PadRight(20));
                helpMessage.Append("Establishes a telnet session (requires password)");
                helpMessage.AppendLine();
                helpMessage.Append("       -disconnect".PadRight(20));
                helpMessage.Append("Terminates established telnet session");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestinfo.Sender.ClientID, helpMessage.ToString());
            }
            else
            {
                bool connectSession = requestinfo.Request.Arguments.Exists("connect");
                bool disconnectSession = requestinfo.Request.Arguments.Exists("disconnect");

                if (m_remoteCommandProcess == null && connectSession && !string.IsNullOrEmpty(requestinfo.Request.Arguments["connect"]))
                {
                    // User wants to establish a remote command session.
                    string password = requestinfo.Request.Arguments["connect"];

                    if (password == m_telnetPassword)
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

                        UpdateStatus("Remote command session established - status updates are suspended.\r\n\r\n");

                        m_remoteCommandClientID = requestinfo.Sender.ClientID;
                        SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("CommandSession", "Established"));
                    }
                    else
                    {
                        UpdateStatus(requestinfo.Sender.ClientID, "Failed to establish remote command session - Password is invalid.\r\n\r\n");
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
                    SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("CommandSession", "Terminated"));

                    UpdateStatus("Remote command session terminated - status updates are resumed.\r\n\r\n");
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
                    requestinfo.Request = ClientRequest.Parse("Telnet /?");
                    RemoteTelnetSession(requestinfo);
                }
            }
        }

        #endregion

        #endregion
	}
}
