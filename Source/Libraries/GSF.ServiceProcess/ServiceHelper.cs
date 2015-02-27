//******************************************************************************************************
//  ServiceHelper.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/29/2006 - Pinal C. Patel
//       Original version of source code generated.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager. UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  12/14/2007 - Pinal C. Patel
//       Made monitoring of service health optional via the MonitorServiceHealth property.
//  09/30/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/06/2009 - Pinal C. Patel
//       Edited code comments.
//  06/19/2009 - Pinal C. Patel
//       Modified Initialize() method to just load settings from the config file.
//  06/30/2009 - Pinal C. Patel
//       Changed ServiceComponents to be a collection of object instead of ISupportLifecycle so it can
//       host a wide range of object implementing various interfaces used by the ServiceHelper commands.
//  07/15/2009 - Pinal C. Patel
//       Added AllowedRemoteUsers and ImpersonateRemoteUser properties as part of security.
//  07/16/2009 - Pinal C. Patel
//       Added SupportTelnetSessions property so that telnet support can be optionally turned-on.
//  07/17/2009 - Pinal C. Patel
//       Modified MonitorServiceHealth to default to false as it is not always needed.
//  07/30/2009 - Pinal C. Patel
//       Modified to set the principal of the thread where client request are handled to that of the the 
//       process or remote client (if transmitted by the remote client upon connection).
//  08/07/2009 - Pinal C. Patel
//       Subscribed to ErrorLogger.LoggingException event.
//  08/10/2009 - Josh L. Patterson
//       Edited Comments
//  09/08/2009 - Pinal C. Patel
//       Modified GetProcess(), GetConnectedClient() and GetClientRequestHandler() to use lambda
//       functions instead of manual iteration of collection items.
//       Renamed GetProcess(), GetConnectedClient() and GetClientRequestHandler() to FindProcess(), 
//       FindConnectedClient() and FindClientRequestHandler() respectively.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/23/2009 - Pinal C. Patel
//       Modified UpdateStatus() method to allow the type of update to be specified.
//  12/18/2009 - Pinal C. Patel
//       Added message flooding control to UpdateStatus().
//  06/16/2010 - Pinal C. Patel
//       Made changes necessary to implement role-based security.
//       Added thread synchronization to list-based member variable.
//  10/14/2010 - Pinal C. Patel
//       Updated security implementation to include the entire client request text including command 
//       and arguments instead of just the command.
//  12/16/2010 - Pinal C. Patel
//       Added SupportSystemCommands property that can be used to enable or disable system-level access 
//       via the build-in commands.
//  01/04/2011 - J. Ritchie Carroll
//       Changed default performance sampling interval on services to every 5 seconds instead of every
//       second to reduce overhead and increase performance. Added a "-lifetime" option to the Health
//       command to show lifetime performance statistics. Made sampling interval a user config option.
//  01/17/2011 - J. Ritchie Carroll
//       Added default "version" and "time" service commands.
//  02/11/2011 - Pinal C. Patel
//       Updated VerifySecurity() to work correctly with TCP integrated windows authentication. 
//  03/09/2011 - Pinal C. Patel
//       Moved UpdateType enumeration to GSF namespace in GSF.Core.dll for broader usage.
//  04/14/2011 - Pinal C. Patel
//       Updated to use new serialization methods in GSF.Serialization class.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  02/17/2012 - Stephen C. Wills
//       Added ReloadCryptoCache service command.
//  04/12/2012 - Pinal C. Patel
//       Added new UnscheduleProcess() and RemoveScheduledProcess methods.
//       Changed AddProcess(), AddScheduledProcess() and ScheduleProcess() method to return a boolean.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using GSF.Annotations;
using GSF.Collections;
using GSF.Communication;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.ErrorManagement;
using GSF.IO;
using GSF.Reflection;
using GSF.Scheduling;
using GSF.Security;
using GSF.Security.Cryptography;
using GSF.Units;

namespace GSF.ServiceProcess
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

        // Nested Types
        private class StatusUpdate
        {
            public StatusUpdate(Guid client, UpdateType type, string message)
            {
                this.Client = client;
                this.Type = type;
                this.Message = message;
            }

            public readonly Guid Client;
            public readonly UpdateType Type;
            public readonly string Message;
        }

        // Delegates
        private delegate bool TryGetClientPrincipalFunctionSignature(Guid clientID, out WindowsPrincipal clientPrincipal);

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="LogStatusUpdates"/> property.
        /// </summary>
        public const bool DefaultLogStatusUpdates = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxStatusUpdatesLength"/> property.
        /// </summary>
        public const int DefaultMaxStatusUpdatesLength = 8192;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxStatusUpdatesFrequency"/> property.
        /// </summary>
        public const int DefaultMaxStatusUpdatesFrequency = 30;

        /// <summary>
        /// Specifies the default value for the <see cref="MonitorServiceHealth"/> property.
        /// </summary>
        public const bool DefaultMonitorServiceHealth = false;

        /// <summary>
        /// Specifies the default value for the <see cref="HealthMonitorInterval"/> property.
        /// </summary>
        public const double DefaultHealthMonitorInterval = 5.0D;

        /// <summary>
        /// Specifies the default value for the <see cref="RequestHistoryLimit"/> property.
        /// </summary>
        public const int DefaultRequestHistoryLimit = 50;

        /// <summary>
        /// Specifies the default value for the <see cref="SupportTelnetSessions"/> property.
        /// </summary>
        public const bool DefaultSupportTelnetSessions = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SupportSystemCommands"/> property.
        /// </summary>
        public const bool DefaultSupportSystemCommands = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SecureRemoteInteractions"/> property.
        /// </summary>
        public const bool DefaultSecureRemoteInteractions = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ServiceHelper.SerializationFormat"/> property.
        /// </summary>
        public const SerializationFormat DefaultSerializationFormat = SerializationFormat.Binary;

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
        /// Occurs when the state of a defined <see cref="ServiceProcess"/> changes.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="ServiceProcess.Name"/>.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="ServiceProcess.CurrentState"/>
        /// </remarks>
        [Category("Process"),
        Description("Occurs when the state of a defined ServiceProcess changes.")]
        public event EventHandler<EventArgs<string, ServiceProcessState>> ProcessStateChanged;

        /// <summary>
        /// Provides notification of when status messages were sent to consumer(s).
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the status message sent to consumer(s).<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the message <see cref="UpdateType"/>.
        /// </remarks>
        [Category("Notification"),
        Description("Occurs when there are status messages sent to consumer(s).")]
        public event EventHandler<EventArgs<string, UpdateType>> UpdatedStatus;

        /// <summary>
        /// Provides notification of when there is an exception logged.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was logged.
        /// </remarks>
        [Category("Notification"),
        Description("Occurs when there is an exception logged.")]
        public event EventHandler<EventArgs<Exception>> LoggedException;

        // Fields
        private bool m_logStatusUpdates;
        private int m_maxStatusUpdatesLength;
        private int m_maxStatusUpdatesFrequency;
        private bool m_monitorServiceHealth;
        private double m_healthMonitorInterval;
        private int m_requestHistoryLimit;
        private bool m_supportTelnetSessions;
        private bool m_supportSystemCommands;
        private bool m_secureRemoteInteractions;
        private SerializationFormat m_serializationFormat;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private string m_telnetSessionPassword;
        private ServiceBase m_parentService;
        private ServerBase m_remotingServer;
        private readonly LogFile m_statusLog;
        private readonly ScheduleManager m_processScheduler;
        private readonly ErrorLogger m_errorLogger;
        private PerformanceMonitor m_performanceMonitor;
        private readonly List<ServiceProcess> m_processes;
        private readonly List<object> m_serviceComponents;
        private readonly List<ClientInfo> m_remoteClients;
        private readonly List<ClientRequestInfo> m_clientRequestHistory;
        private readonly List<ClientRequestHandler> m_clientRequestHandlers;
        private readonly Dictionary<ISupportLifecycle, bool> m_componentEnabledStates;
        private TryGetClientPrincipalFunctionSignature m_tryGetClientPrincipalFunction;
        private readonly ProcessQueue<StatusUpdate> m_statusUpdateQueue;
        private bool m_suppressUpdates;
        private Guid m_remoteCommandClientID;
        private Process m_remoteCommandProcess;
        private Ticks m_lastStatusUpdateTime;
        private long m_statusUpdateCount;
        private bool m_supressStatusUpdates;
        private bool m_enabled;
        private bool m_initialized;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        public ServiceHelper()
        {
            m_telnetSessionPassword = "s3cur3";
            m_logStatusUpdates = DefaultLogStatusUpdates;
            m_maxStatusUpdatesLength = DefaultMaxStatusUpdatesLength;
            m_maxStatusUpdatesFrequency = DefaultMaxStatusUpdatesFrequency;
            m_monitorServiceHealth = DefaultMonitorServiceHealth;
            m_healthMonitorInterval = DefaultHealthMonitorInterval;
            m_requestHistoryLimit = DefaultRequestHistoryLimit;
            m_supportTelnetSessions = DefaultSupportTelnetSessions;
            m_supportSystemCommands = DefaultSupportSystemCommands;
            m_secureRemoteInteractions = DefaultSecureRemoteInteractions;
            m_serializationFormat = DefaultSerializationFormat;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            m_processes = new List<ServiceProcess>();
            m_remoteClients = new List<ClientInfo>();
            m_clientRequestHistory = new List<ClientRequestInfo>();
            m_serviceComponents = new List<object>();
            m_clientRequestHandlers = new List<ClientRequestHandler>();
            m_componentEnabledStates = new Dictionary<ISupportLifecycle, bool>();

            // Components
            m_statusUpdateQueue = ProcessQueue<StatusUpdate>.CreateRealTimeQueue(ProcessStatusUpdates);
            m_statusUpdateQueue.Name = "StatusUpdateQueue";
            m_statusUpdateQueue.ProcessException += StatusUpdateQueue_ProcessException;

            m_statusLog = new LogFile();
            m_statusLog.FileName = "StatusLog.txt";
            m_statusLog.SettingsCategory = "StatusLog";
            m_statusLog.LogException += StatusLog_LogException;

            m_processScheduler = new ScheduleManager();
            m_processScheduler.SettingsCategory = "ProcessScheduler";
            m_processScheduler.ScheduleDue += Scheduler_ScheduleDue;

            m_errorLogger = new ErrorLogger();
            m_errorLogger.ExitOnUnhandledException = false;
            m_errorLogger.SettingsCategory = "ErrorLogger";
            m_errorLogger.ErrorLog.SettingsCategory = "ErrorLog";
            m_errorLogger.LoggingException += ErrorLogger_LoggingException;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHelper"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ServiceHelper"/>.</param>
        public ServiceHelper(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether messages sent using <see cref="UpdateStatus(UpdateType,string,object[])"/> 
        /// or <see cref="UpdateStatus(Guid,UpdateType,string,object[])"/> are to be logged to the <see cref="StatusLog"/>.
        /// </summary>
        [Category("Updates"),
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
        /// Gets or sets the maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        [Category("Updates"),
        DefaultValue(DefaultMaxStatusUpdatesLength),
        Description("Maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.")]
        public int MaxStatusUpdatesLength
        {
            get
            {
                return m_maxStatusUpdatesLength;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                m_maxStatusUpdatesLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        [Category("Updates"),
        DefaultValue(DefaultMaxStatusUpdatesFrequency),
        Description("Maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.")]
        public int MaxStatusUpdatesFrequency
        {
            get
            {
                return m_maxStatusUpdatesFrequency;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                m_maxStatusUpdatesFrequency = value;
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
        /// Gets or sets the interval, in seconds, over which to sample the performance monitor for health statistics.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultHealthMonitorInterval),
        Description("Indicates the interval, in seconds, over which to sample the performance monitor for health statistics.")]
        public double HealthMonitorInterval
        {
            get
            {
                return m_healthMonitorInterval;
            }
            set
            {
                m_healthMonitorInterval = value <= 0.0D ? DefaultHealthMonitorInterval : value;

                if ((object)m_performanceMonitor != null)
                    m_performanceMonitor.SamplingInterval = m_healthMonitorInterval * 1000.0D;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of <see cref="ClientRequest"/> entries to be maintained in the <see cref="ClientRequestHistory"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is zero or negative.</exception>
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
                    throw new ArgumentOutOfRangeException("value", "Value must be greater that 0");

                m_requestHistoryLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="ServiceHelper"/> will have support for remote telnet-like sessions.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultSupportTelnetSessions),
        Description("Indicates whether the ServiceHelper will have support for remote telnet-like sessions.")]
        public bool SupportTelnetSessions
        {
            get
            {
                return m_supportTelnetSessions;
            }
            set
            {
                m_supportTelnetSessions = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="ServiceHelper"/> commands will have support for system-level access (-system switch).
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultSupportSystemCommands),
        Description("Indicates whether the ServiceHelper commands will have support for system-level access (-system switch).")]
        public bool SupportSystemCommands
        {
            get
            {
                return m_supportSystemCommands;
            }
            set
            {
                m_supportSystemCommands = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether <see cref="ServiceHelper"/> will secure remote interactions from <see cref="ClientHelper"/>.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultSecureRemoteInteractions),
        Description("Indicates whether ServiceHelper will secure remote interactions from ClientHelper.")]
        public bool SecureRemoteInteractions
        {
            get
            {
                return m_secureRemoteInteractions;
            }
            set
            {
                m_secureRemoteInteractions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the desired message <see cref="GSF.SerializationFormat"/> for interaction with <see cref="ClientHelper"/>.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultSerializationFormat),
        Description("Indicates messaging serialization format for interactions with ClientHelper.")]
        public SerializationFormat SerializationFormat
        {
            get
            {
                return m_serializationFormat;
            }
            set
            {
                m_serializationFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="ServiceHelper"/> are to be saved to the config file.
        /// </summary>
        [Category("Persistence"),
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
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistence"),
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
                    throw new ArgumentNullException("value");

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
                if ((object)m_remotingServer != null)
                {
                    // Detach events from any existing instance
                    m_remotingServer.ClientConnectingException -= RemotingServer_ClientConnectingException;
                    m_remotingServer.ClientDisconnected -= RemotingServer_ClientDisconnected;
                    m_remotingServer.ReceiveClientDataComplete -= RemotingServer_ReceiveClientDataComplete;
                }

                m_remotingServer = value;
                m_tryGetClientPrincipalFunction = null;

                if ((object)m_remotingServer == null)
                    return;

                // Attach events to new instance
                m_remotingServer.ClientConnectingException += RemotingServer_ClientConnectingException;
                m_remotingServer.ClientDisconnected += RemotingServer_ClientDisconnected;
                m_remotingServer.ReceiveClientDataComplete += RemotingServer_ReceiveClientDataComplete;
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
                    ISupportLifecycle typedComponent;

                    lock (m_serviceComponents)
                    {
                        foreach (object component in m_serviceComponents)
                        {
                            typedComponent = component as ISupportLifecycle;

                            if ((object)typedComponent == null)
                                continue;

                            // Restore previous state.
                            if (m_componentEnabledStates.TryGetValue(typedComponent, out state))
                                typedComponent.Enabled = state;
                        }
                    }
                }
                else
                {
                    // Disable all service components.
                    m_componentEnabledStates.Clear();
                    ISupportLifecycle typedComponent;
                    lock (m_serviceComponents)
                    {
                        foreach (object component in m_serviceComponents)
                        {
                            typedComponent = component as ISupportLifecycle;

                            if ((object)typedComponent == null)
                                continue;

                            // Save current state.
                            m_componentEnabledStates.Add(typedComponent, typedComponent.Enabled);
                            typedComponent.Enabled = false;
                        }
                    }
                }

                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ServiceProcess"/> defined in the <see cref="ServiceHelper"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public IList<ServiceProcess> Processes
        {
            get
            {
                return m_processes.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientInfo"/> for remote clients connected to the <see cref="RemotingServer"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public IList<ClientInfo> RemoteClients
        {
            get
            {
                return m_remoteClients.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientRequestInfo"/> for requests made by <see cref="RemoteClients"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public IList<ClientRequestInfo> ClientRequestHistory
        {
            get
            {
                return m_clientRequestHistory.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets a list of <see cref="ClientRequestHandler"/> registered for handling requests from <see cref="RemoteClients"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public IList<ClientRequestHandler> ClientRequestHandlers
        {
            get
            {
                return m_clientRequestHandlers;
            }
        }

        /// <summary>
        /// Gets a list of components that implement the <see cref="ISupportLifecycle"/> interface used by the <see cref="ParentService"/>.
        /// </summary>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)]
        public IList<object> ServiceComponents
        {
            get
            {
                return m_serviceComponents;
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
                if ((object)m_parentService == null)
                    return m_settingsCategory;

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
                if ((object)m_remotingServer != null)
                {
                    status.AppendFormat("System Uptime: {0}", m_remotingServer.RunTime.ToString());
                    status.AppendLine();
                    status.AppendLine();
                }

                // Show the status of registered components.
                status.AppendFormat("Status of components used by {0}:", Name);
                status.AppendLine();
                status.AppendLine();

                IProvideStatus typedComponent;

                lock (m_serviceComponents)
                {
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as IProvideStatus;

                        if ((object)typedComponent == null)
                            continue;

                        // This component provides status information.                       
                        status.AppendFormat("Status of {0}:", typedComponent.Name);
                        status.AppendLine();
                        status.Append(typedComponent.Status);
                        status.AppendLine();
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ServiceHelper"/> 
        /// object is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (m_initialized)
                return;

            LoadSettings();         // Load settings from the config file.
            m_initialized = true;   // Initialize only once.
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
            //if (!DesignMode)
            //{
            //    try
            //    {
            //        // Nothing needs to be done before component is initialized.
            //    }
            //    catch
            //    {
            //        // Prevent the IDE from crashing when component is in design mode.
            //    }
            //}
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
            if (DesignMode)
                return;

            try
            {
                Initialize();
            }
            catch
            {
                // Prevent the IDE from crashing when component is in design mode.
            }
        }

        /// <summary>
        /// Saves settings of the <see cref="ServiceHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (!m_persistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
            settings["LogStatusUpdates", true].Update(m_logStatusUpdates);
            settings["MaxStatusUpdatesLength", true].Update(m_maxStatusUpdatesLength);
            settings["MaxStatusUpdatesFrequency", true].Update(m_maxStatusUpdatesFrequency);
            settings["MonitorServiceHealth", true].Update(m_monitorServiceHealth);
            settings["HealthMonitorInterval", true].Update(m_healthMonitorInterval);
            settings["RequestHistoryLimit", true].Update(m_requestHistoryLimit);
            settings["SupportTelnetSessions", true].Update(m_supportTelnetSessions);
            settings["SupportSystemCommands", true].Update(m_supportSystemCommands);
            settings["SecureRemoteInteractions", true].Update(m_secureRemoteInteractions);
            settings["SerializationFormat", true].Update(m_serializationFormat);
            config.Save();
        }

        /// <summary>
        /// Saves settings of the <see cref="ServiceHelper"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <param name="includeServiceComponents">A boolean value that indicates whether the settings of <see cref="ServiceComponents"/> are to be saved.</param>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings(bool includeServiceComponents)
        {
            SaveSettings();

            if (!includeServiceComponents)
                return;

            IPersistSettings typedComponent;
            lock (m_serviceComponents)
            {
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;

                    if ((object)typedComponent != null)
                        typedComponent.SaveSettings();
                }
            }
        }

        /// <summary>
        /// Loads saved settings of the <see cref="ServiceHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (!m_persistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

            settings.Add("LogStatusUpdates", m_logStatusUpdates, "True if status update messages are to be logged to a text file; otherwise False.");
            settings.Add("MaxStatusUpdatesLength", m_maxStatusUpdatesLength, "Maximum numbers of characters allowed in update status messages without getting suppressed from being displayed.");
            settings.Add("MaxStatusUpdatesFrequency", m_maxStatusUpdatesFrequency, "Maximum number of status update messages that can be issued in a second without getting suppressed from being displayed.");
            settings.Add("MonitorServiceHealth", m_monitorServiceHealth, "True if the service health is to be monitored; otherwise False.");
            settings.Add("HealthMonitorInterval", m_healthMonitorInterval, "The interval, in seconds, over which to sample the performance monitor for health statistics.");
            settings.Add("RequestHistoryLimit", m_requestHistoryLimit, "Number of client request entries to be kept in the history.");
            settings.Add("SupportTelnetSessions", m_supportTelnetSessions, "True to enable the support for remote telnet-like sessions; otherwise False.");
            settings.Add("SupportSystemCommands", m_supportSystemCommands, "True to enable system-level access (-system switch) via the build-in commands; otherwise False.");
            settings.Add("SecureRemoteInteractions", m_secureRemoteInteractions, "True to enable security of remote client interactions; otherwise False.");
            settings.Add("SerializationFormat", m_serializationFormat, "Message serialization format for interactions with clients, one of: Xml, Json or Binary. Default is Binary.");

            if ((object)settings["TelnetSessionPassword"] != null)
                m_telnetSessionPassword = settings["TelnetSessionPassword"].ValueAs(m_telnetSessionPassword);

            LogStatusUpdates = settings["LogStatusUpdates"].ValueAs(m_logStatusUpdates);
            MaxStatusUpdatesLength = settings["MaxStatusUpdatesLength"].ValueAs(m_maxStatusUpdatesLength);
            MaxStatusUpdatesFrequency = settings["MaxStatusUpdatesFrequency"].ValueAs(m_maxStatusUpdatesFrequency);
            MonitorServiceHealth = settings["MonitorServiceHealth"].ValueAs(m_monitorServiceHealth);
            HealthMonitorInterval = settings["HealthMonitorInterval"].ValueAs(m_healthMonitorInterval);
            RequestHistoryLimit = settings["RequestHistoryLimit"].ValueAs(m_requestHistoryLimit);
            SupportTelnetSessions = settings["SupportTelnetSessions"].ValueAs(m_supportTelnetSessions);
            SupportSystemCommands = settings["SupportSystemCommands"].ValueAs(m_supportSystemCommands);
            SecureRemoteInteractions = settings["SecureRemoteInteractions"].ValueAs(m_secureRemoteInteractions);
            SerializationFormat = settings["SerializationFormat"].ValueAs(m_serializationFormat);
        }

        /// <summary>
        /// Loads saved settings of the <see cref="ServiceHelper"/> from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <param name="includeServiceComponents">A boolean value that indicates whether the settings of <see cref="ServiceComponents"/> are to be loaded.</param>
        public void LoadSettings(bool includeServiceComponents)
        {
            LoadSettings();

            if (!includeServiceComponents)
                return;

            IPersistSettings typedComponent;

            lock (m_serviceComponents)
            {
                foreach (object component in m_serviceComponents)
                {
                    typedComponent = component as IPersistSettings;

                    if ((object)typedComponent != null)
                        typedComponent.LoadSettings();
                }
            }
        }

        /// <summary>
        /// To be called from the <see cref="ServiceBase.OnStart(string[])"/> method of <see cref="ParentService"/>.
        /// </summary>
        /// <param name="args">Array of type <see cref="String"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void OnStart(string[] args)
        {
            // Ensure required components are present.
            if ((object)m_parentService == null)
                throw new InvalidOperationException("ParentService property of ServiceHelper component is not set");

            if ((object)m_remotingServer == null)
                throw new InvalidOperationException("RemotingServer property of ServiceHelper component is not set");

            OnServiceStarting(args);

            lock (m_clientRequestHandlers)
            {
                m_clientRequestHandlers.Add(new ClientRequestHandler("Clients", "Displays list of clients connected to the service", ShowClients));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Settings", "Displays queryable service settings from config file", ShowSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Processes", "Displays list of service or system processes", ShowProcesses));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Schedules", "Displays list of process schedules defined in the service", ShowSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandler("History", "Displays list of requests received from the clients", ShowRequestHistory));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Help", "Displays list of commands supported by the service", ShowRequestHelp, new[] { "?" }));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Status", "Displays the current service status", ShowServiceStatus, new[] { "stat" }));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Start", "Start a service or system process", StartProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Abort", "Aborts a service or system process", AbortProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandler("ReloadCryptoCache", "Reloads local cryptography cache", ReloadCryptoCache));
                m_clientRequestHandlers.Add(new ClientRequestHandler("UpdateSettings", "Updates service setting in the config file", UpdateSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandler("ReloadSettings", "Reloads services settings from the config file", ReloadSettings));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Reschedule", "Reschedules a process defined in the service", RescheduleProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Unschedule", "Unschedules a process defined in the service", UnscheduleProcess));
                m_clientRequestHandlers.Add(new ClientRequestHandler("SaveSchedules", "Saves process schedules to the config file", SaveSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandler("LoadSchedules", "Loads process schedules from the config file", LoadSchedules));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Version", "Displays current service version", ShowVersion, new[] { "ver" }));
                m_clientRequestHandlers.Add(new ClientRequestHandler("Time", "Displays current system time", ShowTime));
                m_clientRequestHandlers.Add(new ClientRequestHandler("User", "Displays current user information", ShowUser, new[] { "whoami" }));

                // Enable telnet support if requested.
                if (m_supportTelnetSessions)
                    m_clientRequestHandlers.Add(new ClientRequestHandler("Telnet", "Allows for a telnet session to the service server", RemoteTelnetSession, false));

                // Enable health monitoring if requested.
                if (m_monitorServiceHealth)
                {
                    try
                    {
                        m_performanceMonitor = new PerformanceMonitor(m_healthMonitorInterval * 1000.0D);
                        m_clientRequestHandlers.Add(new ClientRequestHandler("Health", "Displays a report of resource utilization for the service", ShowHealthReport));
                        m_clientRequestHandlers.Add(new ClientRequestHandler("ResetHealthMonitor", "Resets the system resource utilization monitor", ResetHealthMonitor));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        string message = string.Format("Unable to start health monitor due to exception: {0}", ex.Message);
                        LogException(new InvalidOperationException(message, ex));
                        UpdateStatus(UpdateType.Warning, "{0} Is the service account a member of the \"Performance Log Users\" group?", message);
                    }
                }
            }

            // Add internal components as service components by default.
            lock (m_serviceComponents)
            {
                m_serviceComponents.Add(m_processScheduler);
                m_serviceComponents.Add(m_statusLog);
                m_serviceComponents.Add(m_errorLogger);
                m_serviceComponents.Add(m_errorLogger.ErrorLog);
                m_serviceComponents.Add(m_remotingServer);
                m_serviceComponents.Add(m_statusUpdateQueue);
            }

            // Open log file if file logging is enabled.
            if (m_logStatusUpdates)
                m_statusLog.Open();

            // Start all of the core components.
            m_statusUpdateQueue.Start();
            m_processScheduler.Start();
            m_remotingServer.Start();

            m_enabled = true;
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
                if ((object)process != null)
                    process.Abort();
            }

            m_enabled = false;          // Mark as disabled.
            m_suppressUpdates = true;   // Suppress status updates.

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
            OnStop();
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
            SendResponse(client, response, true);
        }

        /// <summary>
        /// Sends the specified <paramref name="response"/> to the specified <paramref name="client"/> only.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="response"/> is to be sent.</param>
        /// <param name="response">The <see cref="ServiceResponse"/> to be sent to the <paramref name="client"/>.</param>
        /// <param name="async">Flag to determine whether to wait for the send operations to complete.</param>
        public void SendResponse(Guid client, ServiceResponse response, bool async)
        {
            try
            {
                WaitHandle[] handles = new WaitHandle[0];

                if (client != Guid.Empty)
                {
                    // Send message directly to specified client.
                    handles = new[] { m_remotingServer.SendToAsync(client, response) };
                }
                else
                {
                    // Send message to all of the connected clients.
                    if (m_remoteCommandClientID == Guid.Empty)
                    {
                        lock (m_remoteClients)
                        {
                            handles = m_remoteClients.Select(clientInfo => m_remotingServer.SendToAsync(clientInfo.ClientID, response)).ToArray();
                        }
                    }
                }

                if (!async)
                    WaitHandle.WaitAll(handles);
            }
            catch (Exception ex)
            {
                // Log the exception.
                LogException(ex);
            }
        }

        /// <summary>
        /// Sends an actionable response to client along with an optional formatted message and attachment.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="attachment">Attachment to send with response.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        /// <remarks>
        /// This method is used to send an actionable client response that can be used for responding to an event after a command has been issued.
        /// </remarks>
        public void SendActionableResponse(ClientRequestInfo requestInfo, bool success, object attachment = null, string status = null, params object[] args)
        {
            try
            {
                string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
                string message = "";

                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (args.Length == 0)
                        message = status + "\r\n\r\n";
                    else
                        message = string.Format(status, args) + "\r\n\r\n";
                }

                ServiceResponse response = new ServiceResponse(responseType, CurtailMessageLength(message));

                // Add any specified attachment to the service response
                if ((object)attachment != null)
                    response.Attachments.Add(attachment);

                // Add original command arguments as an attachment
                response.Attachments.Add(requestInfo.Request.Arguments);

                // Send response to service
                SendResponse(requestInfo.Sender.ClientID, response);

                OnUpdatedStatus(response.Message, success ? UpdateType.Information : UpdateType.Alarm);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);
                UpdateStatus(UpdateType.Alarm, "Failed to send actionable client response with attachment due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Provides a status update to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="message">Text message to be transmitted to all <see cref="RemoteClients"/>.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        [StringFormatMethod("message")]
        public void UpdateStatus(UpdateType type, string message, params object[] args)
        {
            UpdateStatus(Guid.Empty, type, message, args);
        }

        /// <summary>
        /// Provides a status update to the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="message"/> is to be sent.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="message">Text message to be transmitted to the <paramref name="client"/>.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        [StringFormatMethod("message")]
        public void UpdateStatus(Guid client, UpdateType type, string message, params object[] args)
        {
            if (!m_suppressUpdates)
            {
                // Queue the status update for processing.
                m_statusUpdateQueue.Add(new StatusUpdate(client, type, string.Format(message, args)));
            }
        }

        /// <summary>
        /// Provides a line terminated status update to all <see cref="RemoteClients"/>.
        /// </summary>
        /// <param name="message">Text message to be transmitted to all <see cref="RemoteClients"/>.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        [StringFormatMethod("message")]
        public void UpdateStatusAppendLine(UpdateType type, string message, params object[] args)
        {
            UpdateStatusAppendLine(Guid.Empty, type, message, args);
        }

        /// <summary>
        /// Provides a line terminated status update to the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">ID of the client to whom the <paramref name="message"/> is to be sent.</param>
        /// <param name="type">One of the <see cref="UpdateType"/> values.</param>
        /// <param name="message">Text message to be transmitted to the <paramref name="client"/>.</param>
        /// <param name="args">Arguments to be used for formatting the <paramref name="message"/>.</param>
        [StringFormatMethod("message")]
        public void UpdateStatusAppendLine(Guid client, UpdateType type, string message, params object[] args)
        {
            UpdateStatus(type, message + "\r\n", args);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> does not already exist and is added, otherwise false.</returns>
        public bool AddProcess(Action<string, object[]> processExecutionMethod, string processName)
        {
            return AddProcess(processExecutionMethod, processName, null);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> does not already exist and is added, otherwise false.</returns>
        public bool AddProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments)
        {
            processName = processName.Trim();

            if ((object)FindProcess(processName) != null)
                return false;

            ServiceProcess process = new ServiceProcess(processExecutionMethod, processName, processArguments);

            process.StateChanged += Process_StateChanged;

            lock (m_processes)
            {
                m_processes.Add(process);
            }

            return true;
        }

        /// <summary>
        /// Removes an existing <see cref="ServiceProcess"/> from the <see cref="ServiceHelper"/>.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be removed.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> exists and is removed, otherwise false.</returns>
        public bool RemoveProcess(string processName)
        {
            ServiceProcess process = FindProcess(processName.Trim());

            if ((object)process == null)
                return false;

            process.StateChanged -= Process_StateChanged;

            lock (m_processes)
            {
                m_processes.Remove(process);
            }

            return true;
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> does not exist already and is added and scheduled, otherwise false.</returns>
        public bool AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, string processSchedule)
        {
            return AddScheduledProcess(processExecutionMethod, processName, null, processSchedule);
        }

        /// <summary>
        /// Adds a new <see cref="ServiceProcess"/> to the <see cref="ServiceHelper"/> that executes on a schedule.
        /// </summary>
        /// <param name="processExecutionMethod">The <see cref="Delegate"/> to be invoked the <see cref="ServiceProcess"/> is started.</param>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> being added.</param>
        /// <param name="processArguments">Arguments to be passed in to the <paramref name="processExecutionMethod"/> during execution.</param>
        /// <param name="processSchedule"><see cref="Schedule"/> for the execution of the <see cref="ServiceProcess"/>.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> does not exist already and is added and scheduled, otherwise false.</returns>
        public bool AddScheduledProcess(Action<string, object[]> processExecutionMethod, string processName, object[] processArguments, string processSchedule)
        {
            if (AddProcess(processExecutionMethod, processName, processArguments))
                return ScheduleProcess(processName, processSchedule);

            return false;
        }

        /// <summary>
        /// Removes an existing <see cref="ServiceProcess"/> from the <see cref="ServiceHelper"/> that is scheduled for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the scheduled <see cref="ServiceProcess"/> to be removed.</param>
        /// <returns>true is the scheduled <see cref="ServiceProcess"/> is removed, otherwise false.</returns>
        public bool RemoveScheduledProcess(string processName)
        {
            if (UnscheduleProcess(processName))
                return RemoveProcess(processName);

            return false;
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> exists and is scheduled, otherwise false.</returns>
        public bool ScheduleProcess(string processName, string scheduleRule)
        {
            return ScheduleProcess(processName, scheduleRule, false);
        }

        /// <summary>
        /// Schedules an existing <see cref="ServiceProcess"/> for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be scheduled.</param>
        /// <param name="scheduleRule">Rule that defines the execution pattern of the <see cref="ServiceProcess"/>.</param>
        /// <param name="updateExistingSchedule">true if the <see cref="ServiceProcess"/> is to be re-scheduled; otherwise false.</param>
        /// <returns>true if the <see cref="ServiceProcess"/> exists and is scheduled or re-scheduled, otherwise false.</returns>
        public bool ScheduleProcess(string processName, string scheduleRule, bool updateExistingSchedule)
        {
            processName = processName.Trim();

            if ((object)FindProcess(processName) == null)
                return false;

            // The specified process exists, so we'll schedule it, or update its schedule if it is scheduled already.
            Schedule existingSchedule = m_processScheduler.FindSchedule(processName);

            if ((object)existingSchedule != null)
            {
                // Update the process schedule if it is already exists.
                if (!updateExistingSchedule)
                    return false;

                existingSchedule.Rule = scheduleRule;
                return true;
            }

            // Schedule the process if it is not scheduled already.
            m_processScheduler.Schedules.Add(new Schedule(processName, scheduleRule));
            return true;
        }

        /// <summary>
        /// Unschedules an existing <see cref="ServiceProcess"/> scheduled for automatic execution.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be unscheduled.</param>
        /// <returns>true if the scheduled <see cref="ServiceProcess"/> is unscheduled, otherwise false.</returns>
        public bool UnscheduleProcess(string processName)
        {
            return m_processScheduler.RemoveSchedule(processName.Trim());
        }

        /// <summary>
        /// Returns the <see cref="ServiceProcess"/> for the specified <paramref name="processName"/>.
        /// </summary>
        /// <param name="processName">Name of the <see cref="ServiceProcess"/> to be retrieved.</param>
        /// <returns><see cref="ServiceProcess"/> object if found; otherwise null.</returns>
        public ServiceProcess FindProcess(string processName)
        {
            lock (m_processes)
            {
                return m_processes.Find(process => string.Compare(process.Name, processName, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }

        /// <summary>
        /// Returns the <see cref="ClientInfo"/> object for the specified <paramref name="clientID"/>.
        /// </summary>
        /// <param name="clientID">ID of the client whose <see cref="ClientInfo"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientInfo"/> object if found; otherwise null.</returns>
        public ClientInfo FindConnectedClient(Guid clientID)
        {
            lock (m_remoteClients)
            {
                return m_remoteClients.Find(clientInfo => clientInfo.ClientID == clientID);
            }
        }

        /// <summary>
        /// Returns the <see cref="ClientRequestHandler"/> object for the specified <paramref name="handlerCommand"/>.
        /// </summary>
        /// <param name="handlerCommand">Request type whose <see cref="ClientRequestHandler"/> object is to be retrieved.</param>
        /// <returns><see cref="ClientRequestHandler"/> object if found; otherwise null.</returns>
        public ClientRequestHandler FindClientRequestHandler(string handlerCommand)
        {
            lock (m_clientRequestHandlers)
            {
                return m_clientRequestHandlers.Find(
                    handler => handler.Command.Equals(handlerCommand, StringComparison.OrdinalIgnoreCase) ||
                    ((object)handler.Aliases != null && handler.Aliases.Any(alias => alias.Equals(handlerCommand, StringComparison.OrdinalIgnoreCase))));
            }
        }

        /// <summary>
        /// Log exception to <see cref="ErrorLogger"/>.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public void LogException(Exception ex)
        {
            if ((object)m_errorLogger != null)
                m_errorLogger.Log(ex);

            OnLoggedException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStarting"/> event.
        /// </summary>
        /// <param name="args">Arguments to be sent to <see cref="ServiceStarting"/> event.</param>
        protected virtual void OnServiceStarting(string[] args)
        {
            // Notify service event consumers of pending service start
            if ((object)ServiceStarting != null)
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
            if ((object)ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceStopping"/> event.
        /// </summary>
        protected virtual void OnServiceStopping()
        {
            // Notify service event consumers of pending service stop
            if ((object)ServiceStopping != null)
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
            if ((object)ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServicePausing"/> event.
        /// </summary>
        protected virtual void OnServicePausing()
        {
            // Notify service event consumers of pending service stop
            if ((object)ServicePausing != null)
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
            if ((object)ServicePaused != null)
                ServicePaused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ServiceResuming"/> event.
        /// </summary>
        protected virtual void OnServiceResuming()
        {
            // Notify service event consumers of pending service resume
            if ((object)ServiceResuming != null)
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
            if ((object)ServiceResumed != null)
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
            if ((object)SystemShutdown != null)
                SystemShutdown(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ReceivedClientRequest"/> event.
        /// </summary>
        /// <param name="request">The <see cref="ClientRequest"/> that was received.</param>
        /// <param name="requestSender">The <see cref="ClientInfo"/> object of the <paramref name="request"/> sender.</param>
        protected virtual void OnReceivedClientRequest(ClientRequest request, ClientInfo requestSender)
        {
            if ((object)ReceivedClientRequest != null)
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
            if ((object)ProcessStateChanged != null)
                ProcessStateChanged(this, new EventArgs<string, ServiceProcessState>(processName, processState));

            // Notify all remote clients of change in process state
            SendProcessStateChangedResponse(processName, processState);
        }

        /// <summary>
        /// Raises the <see cref="UpdatedStatus"/> event with the updated status message.
        /// </summary>
        /// <param name="status">Updated status message.</param>
        /// <param name="type"><see cref="UpdateType"/> of status message.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convenience.
        /// </remarks>
        protected virtual void OnUpdatedStatus(string status, UpdateType type)
        {
            if ((object)UpdatedStatus != null)
                UpdatedStatus(this, new EventArgs<string, UpdateType>(status, type));
        }

        /// <summary>
        /// Raises <see cref="LoggedException"/> event with logged exception.
        /// </summary>
        /// <param name="ex">Logged <see cref="Exception"/>.</param>
        protected virtual void OnLoggedException(Exception ex)
        {
            if ((object)LoggedException != null)
                LoggedException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceHelper"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                // This will be done regardless of whether the object is finalized or disposed.
                if (!disposing)
                    return;

                // This will be done only when the object is disposed by calling Dispose().
                SaveSettings();

                if ((object)m_statusLog != null)
                {
                    m_statusLog.LogException -= StatusLog_LogException;
                    m_statusLog.Dispose();
                }

                if ((object)m_processScheduler != null)
                {
                    m_processScheduler.ScheduleDue -= Scheduler_ScheduleDue;
                    m_processScheduler.Dispose();
                }

                if ((object)m_errorLogger != null)
                {
                    m_errorLogger.LoggingException -= ErrorLogger_LoggingException;
                    m_errorLogger.Dispose();
                }

                if ((object)m_performanceMonitor != null)
                {
                    m_performanceMonitor.Dispose();
                }

                if ((object)m_remoteCommandProcess != null)
                {
                    m_remoteCommandProcess.ErrorDataReceived -= RemoteCommandProcess_ErrorDataReceived;
                    m_remoteCommandProcess.OutputDataReceived -= RemoteCommandProcess_OutputDataReceived;

                    if (!m_remoteCommandProcess.HasExited)
                        m_remoteCommandProcess.Kill();

                    m_remoteCommandProcess.Dispose();
                }

                if ((object)m_statusUpdateQueue != null)
                {
                    m_statusUpdateQueue.ProcessException -= StatusUpdateQueue_ProcessException;
                    m_statusUpdateQueue.Flush();
                    m_statusUpdateQueue.Dispose();
                }

                // Service processes are created and owned by remoting server, so we dispose them
                if ((object)m_processes != null)
                {
                    lock (m_processes)
                    {
                        foreach (ServiceProcess process in m_processes)
                        {
                            process.StateChanged -= Process_StateChanged;
                            process.Dispose();
                        }

                        m_processes.Clear();
                    }
                }

                // Detach any remoting server events, we don't own this component so we don't dispose it
                RemotingServer = null;
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        private bool TrySetCurrentThreadPrincipal(ClientInfo client)
        {
            if ((object)client == null)
                throw new ArgumentNullException("client");

            WindowsPrincipal clientPrincipal;

            // Attempt to find the TryGetClientPrincipal method using reflection - remoting server could be a TCP or TLS server
            if ((object)m_remotingServer != null && (object)m_tryGetClientPrincipalFunction == null)
            {
                MethodInfo tryGetClientPrincipalInfo = m_remotingServer.GetType().GetMethod("TryGetClientPrincipal", new[] { typeof(Guid), typeof(WindowsPrincipal).MakeByRefType() });

                if ((object)tryGetClientPrincipalInfo != null && tryGetClientPrincipalInfo.ReturnType == typeof(bool))
                    m_tryGetClientPrincipalFunction = (TryGetClientPrincipalFunctionSignature)Delegate.CreateDelegate(typeof(TryGetClientPrincipalFunctionSignature), m_remotingServer, tryGetClientPrincipalInfo);
            }

            // Attempt to get the client principal from the remoting server
            if ((object)m_tryGetClientPrincipalFunction != null && m_tryGetClientPrincipalFunction(client.ClientID, out clientPrincipal))
            {
                if ((object)clientPrincipal != null)
                    Thread.CurrentPrincipal = clientPrincipal;
                else if ((object)client.ClientUser != null)
                    Thread.CurrentPrincipal = client.ClientUser;

                return true;
            }

            return false;
        }

        private bool VerifySecurity(ClientInfo client)
        {
            if ((object)client == null)
                throw new ArgumentNullException("client");

            // Set current thread principal to remote client's user principal.
            if (!(Thread.CurrentPrincipal is WindowsPrincipal) && (object)client.ClientUser != null)
                Thread.CurrentPrincipal = client.ClientUser;

            // Retrieve previously initialized security provider of the remote client's user.
            if ((object)SecurityProviderCache.CurrentProvider == null)
                SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(string.Empty);

            // Initialize security provider for the remote client's user from specified credentials.
            if ((!Thread.CurrentPrincipal.Identity.IsAuthenticated || (object)client.ClientUser == null) && !string.IsNullOrEmpty(client.ClientUserCredentials))
            {
                string[] credentialParts = client.ClientUserCredentials.Split(':');

                if (credentialParts.Length == 2)
                {
                    ISecurityProvider provider = SecurityProviderUtility.CreateProvider(credentialParts[0]);

                    if (provider.Authenticate(credentialParts[1]))
                        SecurityProviderCache.CurrentProvider = provider;
                }
            }

            // Save the initialized security provider of remote client's user for subsequent uses.
            if (client.ClientUser != Thread.CurrentPrincipal)
                client.SetClientUser(Thread.CurrentPrincipal);

            return (object)client.ClientUser != null && client.ClientUser.Identity.IsAuthenticated;
        }

        // Curtail response message size if needed
        private string CurtailMessageLength(string message)
        {
            const int OneHalfK = (int)SI.Kilo / 2;

            // Don't curtail unless we are at least 512 characters over message length threshold,
            // seems superfluous in warning message to curtail just a handful of characters
            if (message.Length > m_maxStatusUpdatesLength + OneHalfK)
            {
                //                                                             1         2         3         4         5         6         7         8
                //                                                    12345678901234567890123456789012345678901234567890123456789012345678901234567890
                string warningMessage = string.Format("\r\n...\r\n\r\nMaximum status message size exceeded - suppressed over {0:N2}K characters.\r\n\r\n...\r\n", (message.Length - m_maxStatusUpdatesLength) / SI.Kilo);
                int availableLength = (m_maxStatusUpdatesLength - warningMessage.Length) / 2;

                if (availableLength > 0 && message.Length > availableLength)
                {
                    string top = message.Substring(0, availableLength);
                    string bottom = message.Substring(message.Length - availableLength);
                    int newLineIndex;

                    newLineIndex = top.LastIndexOf("\r\n", StringComparison.Ordinal);

                    if (newLineIndex > 0)
                        top = top.Substring(0, newLineIndex).TrimEnd('\r', '\n');

                    newLineIndex = bottom.IndexOf("\r\n", StringComparison.Ordinal);

                    if (newLineIndex > -1 && newLineIndex + 1 < bottom.Length)
                        bottom = bottom.Substring(newLineIndex + 1).TrimStart('\r', '\n');

                    message = string.Format("{0}{1}{2}", top, warningMessage, bottom);
                }
            }

            return message;
        }

        private void ProcessStatusUpdates(StatusUpdate[] items)
        {
            // Reset status update frequency counters after one second
            if ((DateTime.UtcNow.Ticks - m_lastStatusUpdateTime).ToSeconds() >= 1.0D)
            {
                m_lastStatusUpdateTime = DateTime.UtcNow.Ticks;

                if (m_supressStatusUpdates)
                    SendUpdateClientStatusResponse(Guid.Empty, UpdateType.Warning, string.Format("Suppressed {0:N0} status updates due to high frequency to avoid flooding.\r\n\r\n", m_statusUpdateCount));

                m_supressStatusUpdates = (m_statusUpdateCount > m_maxStatusUpdatesFrequency);
                m_statusUpdateCount = 0;
            }

            // Handle priority messages
            IEnumerable<StatusUpdate> priorityStatuses = items.Where(status => status.Client != Guid.Empty || status.Type != UpdateType.Information);

            foreach (StatusUpdate clientStatus in priorityStatuses)
            {
                m_statusUpdateCount++;

                // All messages sent directly to client should be displayed
                if (clientStatus.Client != Guid.Empty || !m_supressStatusUpdates)
                    SendUpdateClientStatusResponse(clientStatus.Client, clientStatus.Type, clientStatus.Message);

                if (m_logStatusUpdates)
                    m_statusLog.WriteTimestampedLine(clientStatus.Message);
            }

            // Handle broadcast messages
            IEnumerable<StatusUpdate> broadcastStatuses = items.Where(status => status.Client == Guid.Empty && status.Type == UpdateType.Information);
            StringBuilder broadcastMessages = new StringBuilder();

            foreach (StatusUpdate item in broadcastStatuses)
            {
                broadcastMessages.Append(item.Message);

                if (m_logStatusUpdates)
                    m_statusLog.WriteTimestampedLine(item.Message);
            }

            // Counting bulk broadcast status set as one update
            m_statusUpdateCount++;

            if (!m_supressStatusUpdates)
                SendUpdateClientStatusResponse(Guid.Empty, UpdateType.Information, broadcastMessages.ToString());
        }

        private void SendAuthenticationSuccessResponse(Guid clientID)
        {
            SendResponse(clientID, new ServiceResponse("AuthenticationSuccess"));
        }

        // ReSharper disable once UnusedMember.Local
        private void SendAuthenticationFailureResponse(Guid clientID)
        {
            SendResponse(clientID, new ServiceResponse("AuthenticationFailure"));
        }

        private void SendUpdateClientStatusResponse(Guid clientID, UpdateType type, string responseMessage)
        {
            if (string.IsNullOrEmpty(responseMessage))
                return;

            ServiceResponse response = new ServiceResponse();
            response.Type = "UPDATECLIENTSTATUS-" + type.ToString().ToUpper();
            response.Message = CurtailMessageLength(responseMessage);
            SendResponse(clientID, response);

            OnUpdatedStatus(response.Message, type);
        }

        private void SendServiceStateChangedResponse(ServiceState currentState)
        {
            ServiceResponse response = new ServiceResponse("SERVICESTATECHANGED");
            response.Attachments.Add(new ObjectState<ServiceState>(Name, currentState));
            SendResponse(response);
        }

        private void SendProcessStateChangedResponse(string processName, ServiceProcessState currentState)
        {
            ServiceResponse response = new ServiceResponse("PROCESSSTATECHANGED");
            response.Attachments.Add(new ObjectState<ServiceProcessState>(processName, currentState));
            SendResponse(response);
        }

        private void Process_StateChanged(object sender, EventArgs e)
        {
            ServiceProcess process = sender as ServiceProcess;

            if ((object)process == null)
                return; // Avoiding Null reference exception

            OnProcessStateChanged(process.Name, process.CurrentState);
        }

        private void Scheduler_ScheduleDue(object sender, EventArgs<Schedule> e)
        {
            ServiceProcess scheduledProcess = FindProcess(e.Argument.Name);

            // Start the process execution if it exists.
            if ((object)scheduledProcess != null)
                scheduledProcess.Start();
        }

        private void StatusUpdateQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            LogException(e.Argument);
        }

        private void StatusLog_LogException(object sender, EventArgs<Exception> e)
        {
            // We'll let the connected clients know that we encountered an exception while logging the status update.
            m_logStatusUpdates = false;
            UpdateStatus(UpdateType.Alarm, "Error occurred while logging status update - {0}\r\n\r\n", e.Argument.Message);
            m_logStatusUpdates = true;
        }

        private void ErrorLogger_LoggingException(object sender, EventArgs<Exception> e)
        {
            UpdateStatus(UpdateType.Alarm, "Error occurred while logging an error - {0}\r\n\r\n", e.Argument.Message);
        }

        private void RemotingServer_ClientConnectingException(object sender, EventArgs<Exception> e)
        {
            UpdateStatus(UpdateType.Alarm, "Error occurred while connecting client to remoting server - {0}\r\n\r\n", e.Argument.Message);
            LogException(e.Argument);
        }

        private void RemotingServer_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            ClientInfo disconnectedClient = FindConnectedClient(e.Argument);

            if ((object)disconnectedClient == null)
                return;

            if (e.Argument == m_remoteCommandClientID)
            {
                try
                {
                    RemoteTelnetSession(new ClientRequestInfo(disconnectedClient, ClientRequest.Parse("Telnet -disconnect")));
                }
                catch (Exception ex)
                {
                    // We'll encounter an exception because we'll try to update the status of the client that had the
                    // remote command session open and this will fail since the client is disconnected.
                    LogException(ex);
                }
            }

            lock (m_remoteClients)
            {
                m_remoteClients.Remove(disconnectedClient);
            }
            UpdateStatus(UpdateType.Information, "Remote client disconnected - {0} from {1}.\r\n\r\n", disconnectedClient.ClientUser.Identity.Name, disconnectedClient.MachineName);
        }

        private void RemotingServer_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            ClientInfo client = FindConnectedClient(e.Argument1);

            if ((object)client == null)
            {
                // First message from a remote client should be its info.
                Serialization.TryDeserialize(e.Argument2.BlockCopy(0, e.Argument3), m_serializationFormat, out client);

                try
                {
                    if ((object)client != null)
                    {
                        client.ClientID = e.Argument1;
                        client.ConnectedAt = DateTime.UtcNow;

                        if (TrySetCurrentThreadPrincipal(client))
                        {
                            // Engage security for the remote client connection if configured.
                            if (!m_secureRemoteInteractions || VerifySecurity(client))
                            {
                                lock (m_remoteClients)
                                {
                                    m_remoteClients.Add(client);
                                }

                                SendAuthenticationSuccessResponse(client.ClientID);
                                UpdateStatus(UpdateType.Information, "Remote client connected - {0} from {1}.\r\n\r\n", client.ClientUser.Identity.Name, client.MachineName);
                            }
                            else
                            {
                                throw new SecurityException(string.Format("Failed to authenticate '{0}' - thread principal identity was '{1}'", client.ClientName, Thread.CurrentPrincipal.Identity.Name));
                            }
                        }
                        else
                        {
                            throw new SecurityException(string.Format("Failed to retrieve client principal from the socket connection: remote client '{0}' not found", client.ClientID));
                        }
                    }
                    else
                    {
                        // Required client information is missing.
                        throw new SecurityException("Remote client failed to transmit the required information");
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);

                    try
                    {
                        SendResponse(e.Argument1, new ServiceResponse("AuthenticationFailure"), false);

                        if ((object)m_remotingServer != null)
                            m_remotingServer.DisconnectOne(e.Argument1);

                        if ((object)client != null)
                            UpdateStatus(UpdateType.Warning, "Remote client connection rejected - {0} [{1}] from {2}\r\n\r\n", client.ClientName, client.ClientUser.Identity.Name, client.MachineName);
                    }
                    catch (Exception ex2)
                    {
                        LogException(ex2);
                    }
                }
            }
            else
            {
                // All subsequent messages from a remote client would be requests.
                ClientRequest request;
                ClientRequestInfo requestInfo = null;

                Serialization.TryDeserialize(e.Argument2.BlockCopy(0, e.Argument3), m_serializationFormat, out request);

                if ((object)request != null)
                {
                    try
                    {
                        requestInfo = new ClientRequestInfo(client, request);
                        string resource = requestInfo.Request.Command;

                        if (m_remoteCommandClientID == Guid.Empty)
                        {
                            // Process incoming requests when remote command session is not in progress.
                            lock (m_clientRequestHistory)
                            {
                                m_clientRequestHistory.Add(requestInfo);

                                if (m_clientRequestHistory.Count > m_requestHistoryLimit)
                                    m_clientRequestHistory.RemoveRange(0, (m_clientRequestHistory.Count - m_requestHistoryLimit));
                            }

                            // Check if remote client has permission to invoke the requested command.
                            if (m_secureRemoteInteractions)
                            {
                                // Validate current client principal
                                if (TrySetCurrentThreadPrincipal(client))
                                {
                                    if (VerifySecurity(requestInfo.Sender))
                                    {
                                        if (SecurityProviderUtility.IsResourceSecurable(resource) && !SecurityProviderUtility.IsResourceAccessible(resource))
                                            throw new SecurityException(string.Format("Access to '{0}' is denied", requestInfo.Request.Command));
                                    }
                                    else
                                    {
                                        throw new SecurityException(string.Format("Failed to authenticate '{0}'", Thread.CurrentPrincipal.Identity.Name));
                                    }
                                }
                                else
                                {
                                    throw new SecurityException(string.Format("Failed to retrieve client principal from the socket connection: remote client '{0}' not found", client.ClientID));
                                }
                            }

                            // Notify the consumer about the incoming request from client.
                            OnReceivedClientRequest(request, client);

                            ClientRequestHandler requestHandler = FindClientRequestHandler(request.Command);

                            if ((object)requestHandler != null)
                            {
                                // Request handler exists.
                                requestHandler.HandlerMethod(requestInfo);
                            }
                            else
                            {
                                // No request handler exists.
                                throw new InvalidOperationException("Request is not supported");
                            }
                        }
                        else if (client.ClientID == m_remoteCommandClientID)
                        {
                            // Redirect requests to remote command session if requests are from its originator.
                            RemoteTelnetSession(requestInfo);
                        }
                        else
                        {
                            // Reject all request from other clients since remote command session is in progress.
                            throw new InvalidOperationException("Remote telnet session is in progress");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);

                        if ((object)requestInfo != null)
                            SendActionableResponse(requestInfo, false, null, "Failed to process request \"{0}\" - {1}\r\n\r\n", request.Command, ex.Message);
                        else
                            UpdateStatus(client.ClientID, UpdateType.Alarm, "Failed to process request \"{0}\" - {1}\r\n\r\n", request.Command, ex.Message);
                    }
                }
                else
                {
                    UpdateStatus(client.ClientID, UpdateType.Alarm, "Failed to process request - Request could not be deserialized.\r\n\r\n");
                }
            }
        }

        private void RemoteCommandProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, UpdateType.Alarm, e.Data + "\r\n");
        }

        private void RemoteCommandProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UpdateStatus(m_remoteCommandClientID, UpdateType.Information, e.Data + "\r\n");
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                    lock (m_remoteClients)
                    {
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
                            if (!string.IsNullOrEmpty(clientInfo.ClientUser.Identity.Name))
                                responseMessage.Append(clientInfo.ClientUser.Identity.Name.PadRight(15));
                            else
                                responseMessage.Append("[Not Available]".PadRight(15));

                            responseMessage.Append(' ');
                            responseMessage.Append(clientInfo.ConnectedAt.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                        }
                    }
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No clients are connected to {0}\r\n\r\n", Name);
                }
            }
        }

        private void ShowSettings(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays a list of service settings from the config file.");
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

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                StringBuilder responseMessage = new StringBuilder();
                responseMessage.AppendFormat("Settings for {0}:", Name);
                responseMessage.AppendLine();
                responseMessage.AppendLine();
                responseMessage.Append("Category".PadRight(20));
                responseMessage.Append(' ');
                responseMessage.Append("Name".PadRight(25));
                responseMessage.Append(' ');
                responseMessage.Append("Value".PadRight(30));
                responseMessage.AppendLine();
                responseMessage.Append(new string('-', 20));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 25));
                responseMessage.Append(' ');
                responseMessage.Append(new string('-', 30));

                IPersistSettings typedComponent;
                lock (m_serviceComponents)
                {
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as IPersistSettings;

                        if ((object)typedComponent == null)
                            continue;

                        foreach (CategorizedSettingsElement setting in ConfigurationFile.Current.Settings[typedComponent.SettingsCategory].Cast<CategorizedSettingsElement>().Where(setting => !setting.Encrypted))
                        {
                            responseMessage.AppendLine();
                            responseMessage.Append(typedComponent.SettingsCategory.PadRight(20));
                            responseMessage.Append(' ');
                            responseMessage.Append(setting.Name.PadRight(25));
                            responseMessage.Append(' ');

                            if (!string.IsNullOrEmpty(setting.Value))
                                responseMessage.Append(setting.Value.PadRight(30));
                            else
                                responseMessage.Append("[Not Set]".PadRight(30));
                        }
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                if (m_supportSystemCommands && showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Displays system processes instead of service processes");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSystemProcesses = requestInfo.Request.Arguments.Exists("system");
                if (listSystemProcesses && m_supportSystemCommands)
                {
                    // Enumerate "system" processes when -system parameter is specified
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
#if MONO
                            responseMessage.Append("Undetermined".PadRight(15));
                            responseMessage.Append(' ');
                            responseMessage.Append("N/A".PadRight(10));
                            responseMessage.Append(' ');
#else
                            responseMessage.Append(process.PriorityClass.ToString().PadRight(15));
                            responseMessage.Append(' ');
                            responseMessage.Append((process.Responding ? "Yes" : "No").PadRight(10));
                            responseMessage.Append(' ');
#endif
                            responseMessage.Append(process.StartTime.ToString("MM/dd/yy hh:mm:ss tt").PadRight(20));
                        }
                        catch
                        {
                        }
                    }
                    responseMessage.AppendLine();
                    responseMessage.AppendLine();

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
                else
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

                        lock (m_processes)
                        {
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
                        }
                        responseMessage.AppendLine();
                        responseMessage.AppendLine();

                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                    }
                    else
                    {
                        // No processes defined in the service to be displayed.
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No processes are defined in {0}.\r\n\r\n", Name);
                    }
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "No process schedules are defined in {0}.\r\n\r\n", Name);
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                lock (m_clientRequestHistory)
                {
                    foreach (ClientRequestInfo historicRequest in m_clientRequestHistory)
                    {
                        responseMessage.AppendLine();
                        responseMessage.Append(historicRequest.Request.Command.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(historicRequest.ReceivedAt.ToString().PadRight(25));
                        responseMessage.Append(' ');
                        responseMessage.Append(string.Format("{0} from {1}", historicRequest.Sender.ClientUser.Identity.Name, historicRequest.Sender.MachineName).PadRight(30));
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                lock (m_clientRequestHandlers)
                {
                    foreach (ClientRequestHandler handler in m_clientRequestHandlers)
                    {
                        if (m_secureRemoteInteractions &&
                            SecurityProviderUtility.IsResourceSecurable(handler.Command) &&
                            !SecurityProviderUtility.IsResourceAccessible(handler.Command))
                            continue;

                        if (!handler.IsAdvertised && !showAdvancedHelp)
                            continue;

                        responseMessage.AppendLine();
                        responseMessage.Append(handler.Command.PadRight(20));
                        responseMessage.Append(' ');
                        responseMessage.Append(handler.CommandDescription.PadRight(55));
                    }
                }
                responseMessage.AppendLine();
                responseMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, responseMessage.ToString());
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -lifetime".PadRight(20));
                helpMessage.Append("Shows utilization over entire service lifetime");
                helpMessage.AppendLine();
                helpMessage.Append("       -actionable".PadRight(20));
                helpMessage.Append("Returns results via an actionable event");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string message;
                bool success;

                if ((object)m_performanceMonitor != null)
                {
                    try
                    {
                        if (requestInfo.Request.Arguments.Exists("lifetime"))
                            message = m_performanceMonitor.LifetimeStatus;
                        else
                            message = m_performanceMonitor.Status;

                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "\r\n" + message + "\r\n");
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        message = "Failed to query system health monitor status: " + ex.Message;
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, message + "\r\n\r\n");
                        success = false;
                    }
                }
                else
                {
                    message = "System health monitor is unavailable.";
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Warning, message + "\r\n\r\n");
                    success = false;
                }

                // Also allow consumers to directly consume message via event in response to a health request
                if (requestInfo.Request.Arguments.Exists("actionable"))
                    SendActionableResponse(requestInfo, success, null, message);
            }
        }

        private void ResetHealthMonitor(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Resets the system resource utilization monitor.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ResetHealthMonitor -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                try
                {
                    // Dispose existing performance monitor
                    if ((object)m_performanceMonitor != null)
                        m_performanceMonitor.Dispose();

                    // Recreate the performance monitor
                    m_performanceMonitor = new PerformanceMonitor(m_healthMonitorInterval * 1000.0D);

                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "System health monitor successfully reset.\r\n\r\n");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to reset system health monitor: {0}\r\n\r\n", ex.Message);
                }
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -actionable".PadRight(20));
                helpMessage.Append("Returns results via an actionable event");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string message = Status;
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, message);

                // Also allow consumers to directly consume message via event in response to a status request
                if (requestInfo.Request.Arguments.Exists("actionable"))
                    SendActionableResponse(requestInfo, true, null, message);
            }
        }

        private void ReloadCryptoCache(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Reloads the local system cryptography cache with data from the common cryptography cache.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ReloadCryptoCache -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                Cipher.ReloadCache();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Crypto cache successfully reloaded.\r\n\r\n");
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("IMPORTANT: Only settings under the categories listed by the \"Settings\" command can be reloaded.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string categoryName = requestInfo.Request.Arguments["orderedarg1"];

                IPersistSettings typedComponent;
                lock (m_serviceComponents)
                {
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as IPersistSettings;

                        if ((object)typedComponent == null || string.Compare(categoryName, typedComponent.SettingsCategory, StringComparison.OrdinalIgnoreCase) != 0)
                            continue;

                        typedComponent.LoadSettings();
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully loaded settings from category \"{0}\".\r\n\r\n", categoryName);
                        return;
                    }
                }

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to load settings from category \"{0}\". No corresponding component exists.\r\n\r\n", categoryName);
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
                helpMessage.Append("IMPORTANT: Only settings under the categories listed by the \"Settings\" command can be updated.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                IPersistSettings typedComponent;
                lock (m_serviceComponents)
                {
                    foreach (object component in m_serviceComponents)
                    {
                        typedComponent = component as IPersistSettings;

                        if ((object)typedComponent == null || string.Compare(categoryName, typedComponent.SettingsCategory, StringComparison.OrdinalIgnoreCase) != 0)
                            continue;

                        ConfigurationFile config = ConfigurationFile.Current;
                        CategorizedSettingsElementCollection settings = config.Settings[categoryName];
                        CategorizedSettingsElement setting = settings[settingName];

                        if (addSetting)
                        {
                            // Add new setting.
                            if ((object)setting == null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to add setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                settings.Add(settingName, settingValue);
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully added setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to add setting \"{0}\" under category \"{1}\". Setting already exists.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }
                        else if (deleteSetting)
                        {
                            // Delete existing setting.
                            if ((object)setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to delete setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                settings.Remove(setting);
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully deleted setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to delete setting \"{0}\" under category \"{1}\". Setting does not exist.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }
                        else
                        {
                            // Update existing setting.
                            if ((object)setting != null)
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to update setting \"{0}\" under category \"{1}\"...\r\n\r\n", settingName, categoryName);
                                setting.Value = settingValue;
                                config.Save();
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully updated setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to update value of setting \"{0}\" under category \"{1}\" . Setting does not exist.\r\n\r\n", settingName, categoryName);
                                return;
                            }
                        }

                        if (reloadSettings)
                        {
                            // The user has requested to reload settings for all the components.
                            requestInfo.Request = ClientRequest.Parse(string.Format("ReloadSettings {0}", categoryName));
                            ReloadSettings(requestInfo);
                        }

                        if (!listSettings)
                            return;

                        // The user has requested to list all of the queryable settings.
                        requestInfo.Request = ClientRequest.Parse("Settings");
                        ShowSettings(requestInfo);

                        return;
                    }
                }

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to update settings under category \"{0}\". No corresponding component exists.\r\n\r\n", categoryName);
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
                if (m_supportSystemCommands && showAdvancedHelp)
                {
                    helpMessage.AppendLine();
                    helpMessage.Append("       -system".PadRight(20));
                    helpMessage.Append("Treats the specified process as a system process");
                }
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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

                if (systemProcess && m_supportSystemCommands)
                {
                    // Start system process.
                    try
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to start system process \"{0}\"...\r\n\r\n", processName);
                        Process startedProcess = Process.Start(processName, processArgs);

                        if ((object)startedProcess != null)
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully started system process \"{0}\".\r\n\r\n", processName);
                        else
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start system process \"{0}\".\r\n\r\n", processName);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                    }
                }
                else
                {
                    // Start service process.
                    ServiceProcess processToStart = FindProcess(processName);

                    if ((object)processToStart != null)
                    {
                        if (processToStart.CurrentState != ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to start service process \"{0}\"...\r\n\r\n", processName);

                            if (string.IsNullOrEmpty(processArgs))
                            {
                                processToStart.Start();
                            }
                            else
                            {
                                // Prepare the arguments.
                                string[] splitArgs = processArgs.Split(',');

                                for (int i = 0; i < splitArgs.Length; i++)
                                {
                                    splitArgs[i] = splitArgs[i].Trim();
                                }

                                // Start the service process.
                                processToStart.Start(splitArgs.Cast<object>().ToArray());
                            }
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully started service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start process \"{0}\". Process is already executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to start service process \"{0}\". Process is not defined.\r\n\r\n", processName);
                    }
                }

                if (!listProcesses)
                    return;

                requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                ShowProcesses(requestInfo);
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all service or system processes");
                if (m_supportSystemCommands && showAdvancedHelp)
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

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool systemProcess = requestInfo.Request.Arguments.Exists("system");
                bool listProcesses = requestInfo.Request.Arguments.Exists("list");

                if (systemProcess && m_supportSystemCommands)
                {
                    // Abort system process.
                    Process processToAbort = null;

                    if (string.Compare(processName, "Me", StringComparison.OrdinalIgnoreCase) == 0)
                        processName = Process.GetCurrentProcess().ProcessName;

                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        // Lookup for the system process by name.
                        processToAbort = process;
                        break;
                    }

                    if ((object)processToAbort == null)
                    {
                        int processID;

                        if (int.TryParse(processName, out processID) && processID > 0)
                        {
                            processToAbort = Process.GetProcessById(processID);
                            processName = processToAbort.ProcessName;
                        }
                    }

                    if ((object)processToAbort != null)
                    {
                        try
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to abort system process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Kill();
                            if (processToAbort.WaitForExit(10000))
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully aborted system process \"{0}\".\r\n\r\n", processName);
                            }
                            else
                            {
                                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". Process not responding.\r\n\r\n", processName);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". {1}.\r\n\r\n", processName, ex.Message);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort system process \"{0}\". Process is not running.\r\n\r\n", processName);
                    }
                }
                else
                {
                    // Abort service process.
                    ServiceProcess processToAbort = FindProcess(processName);

                    if ((object)processToAbort != null)
                    {
                        if (processToAbort.CurrentState == ServiceProcessState.Processing)
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to abort service process \"{0}\"...\r\n\r\n", processName);
                            processToAbort.Abort();
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully aborted service process \"{0}\".\r\n\r\n", processName);
                        }
                        else
                        {
                            UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort service process \"{0}\". Process is not executing.\r\n\r\n", processName);
                        }
                    }
                    else
                    {
                        UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to abort service process \"{0}\". Process is not defined.\r\n\r\n", processName);
                    }
                }

                if (!listProcesses)
                    return;

                requestInfo.Request = ClientRequest.Parse(string.Format("Processes {0}", systemProcess ? "-system" : ""));
                ShowProcesses(requestInfo);
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

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
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
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to schedule process \"{0}\" with rule \"{1}\"...\r\n\r\n", processName, scheduleRule);
                    ScheduleProcess(processName, scheduleRule, true);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully scheduled process \"{0}\" with rule \"{1}\".\r\n\r\n", processName, scheduleRule);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to schedule process \"{0}\". {1}\r\n\r\n", processName, ex.Message);
                }

                if (!listSchedules)
                    return;

                requestInfo.Request = ClientRequest.Parse("Schedules");
                ShowSchedules(requestInfo);
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

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string processName = requestInfo.Request.Arguments["orderedarg1"];
                bool saveSchedules = requestInfo.Request.Arguments.Exists("save");
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                Schedule scheduleToRemove = m_processScheduler.FindSchedule(processName);

                if ((object)scheduleToRemove != null)
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to unschedule process \"{0}\"...\r\n\r\n", processName);
                    m_processScheduler.Schedules.Remove(scheduleToRemove);
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully unscheduled process \"{0}\".\r\n\r\n", processName);

                    if (saveSchedules)
                    {
                        requestInfo.Request = ClientRequest.Parse("SaveSchedules");
                        SaveSchedules(requestInfo);
                    }
                }
                else
                {
                    UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Alarm, "Failed to unschedule process \"{0}\". Process is not scheduled.\r\n\r\n", processName);
                }

                if (!listSchedules)
                    return;

                requestInfo.Request = ClientRequest.Parse("Schedules");
                ShowSchedules(requestInfo);
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to save process schedules to the config file...\r\n\r\n");
                m_processScheduler.SaveSettings();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully saved process schedules to the config file.\r\n\r\n");

                if (!listSchedules)
                    return;

                requestInfo.Request = ClientRequest.Parse("Schedules");
                ShowSchedules(requestInfo);
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
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Displays list of all process schedules");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool listSchedules = requestInfo.Request.Arguments.Exists("list");

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Attempting to load process schedules from the config file...\r\n\r\n");
                m_processScheduler.LoadSettings();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "Successfully loaded process schedules from the config file.\r\n\r\n");

                if (!listSchedules)
                    return;

                requestInfo.Request = ClientRequest.Parse("Schedules");
                ShowSchedules(requestInfo);
            }
        }

        private void ShowVersion(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Shows the current service version.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Version -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -actionable".PadRight(20));
                helpMessage.Append("Returns results via an actionable event");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                StringBuilder versionInfo = new StringBuilder();
                AssemblyInfo serviceAssembly = AssemblyInfo.EntryAssembly;
                string serviceName;

                if ((object)m_parentService != null && !string.IsNullOrWhiteSpace(m_parentService.ServiceName))
                    serviceName = m_parentService.ServiceName;
                else
                    serviceName = AppDomain.CurrentDomain.FriendlyName;

                // Get current process memory usage
                long processMemory = Common.GetProcessMemory();

                versionInfo.AppendFormat("{0} Service Version:{1}{1}", serviceName, Environment.NewLine);
                versionInfo.AppendFormat("      App Domain: {0}, running on .NET {1}{2}", AppDomain.CurrentDomain.FriendlyName, Environment.Version, Environment.NewLine);
                versionInfo.AppendFormat("    Machine Name: {0}{1}", Environment.MachineName, Environment.NewLine);
                versionInfo.AppendFormat("      OS Version: {0}{1}", Environment.OSVersion.VersionString, Environment.NewLine);
                versionInfo.AppendFormat("    Product Name: {0}{1}", Common.GetOSProductName(), Environment.NewLine);
                versionInfo.AppendFormat("  Working Memory: {0}{1}", processMemory > 0 ? SI2.ToScaledString(processMemory, 4, "B", SI2.IECSymbols) : "Undetermined", Environment.NewLine);
                versionInfo.AppendFormat("  Execution Mode: {0}-bit{1}", IntPtr.Size * 8, Environment.NewLine);
                versionInfo.AppendFormat("      Processors: {0}{1}", Environment.ProcessorCount, Environment.NewLine);
                versionInfo.AppendFormat("       Code Base: {0}{1}", serviceAssembly.CodeBase, Environment.NewLine);
                versionInfo.AppendFormat("      Build Date: {0}{1}", serviceAssembly.BuildDate, Environment.NewLine);
                versionInfo.AppendFormat("         Version: {0}", serviceAssembly.Version);

                string message = versionInfo.ToString();
                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, message + "{0}{0}", Environment.NewLine);

                // Also allow consumers to directly consume message via event in response to a version request
                if (requestInfo.Request.Arguments.Exists("actionable"))
                    SendActionableResponse(requestInfo, true, null, message);
            }
        }

        private void ShowTime(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Shows the current system time.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Time -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -actionable".PadRight(20));
                helpMessage.Append("Returns results via an actionable event");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                string message;
                //          1         2         3         4         5         6         7         8
                // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                //  Current system time: yyyy-MM-dd HH:mm:ss.fff, yyyy-MM-dd HH:mm:ss.fff UTC
                // Total system runtime: xx days yy hours zz minutes ii seconds
                if ((object)m_remotingServer != null)
                    message = string.Format(" Current system time: {0}, {1} UTC\r\nTotal system runtime: {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), m_remotingServer.RunTime);
                else
                    message = string.Format("Current system time: {0}, {1} UTC", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, message + "\r\n\r\n");

                // Also allow consumers to directly consume message via event in response to a time request
                if (requestInfo.Request.Arguments.Exists("actionable"))
                    SendActionableResponse(requestInfo, true, null, message);
            }
        }

        private void ShowUser(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Shows the current user information.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       User -options");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -actionable".PadRight(20));
                helpMessage.Append("Returns results via an actionable event");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                //          1         2         3         4         5         6         7         8
                // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                //   Current user: ABC\john
                //    Client name: openPDCConsole
                //   From machine: JohnPC
                // Connected time: xx days yy hours zz minutes ii seconds
                //  Authenticated: true

                ClientInfo info = requestInfo.Sender;

                string message = string.Format(
                    "  Current user: {0}\r\n" +
                    "   Client name: {1}\r\n" +
                    "  From machine: {2}\r\n" +
                    "Connected time: {3}\r\n" +
                    " Authenticated: {4}",
                    info.ClientUser.Identity.Name.ToNonNullNorEmptyString("Undetermined"),
                    info.ClientName.ToNonNullNorEmptyString("Undetermined"),
                    info.MachineName.ToNonNullNorEmptyString("Undetermined"),
                    info.ConnectedAt > DateTime.MinValue ? (DateTime.UtcNow - info.ConnectedAt).ToElapsedTimeString() : m_remotingServer.RunTime.ToString(),
                    info.ClientUser.Identity.IsAuthenticated);

                UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, message + "\r\n\r\n");

                // Also allow consumers to directly consume message via event in response to a user info request
                if (requestInfo.Request.Arguments.Exists("actionable"))
                    SendActionableResponse(requestInfo, true, null, message);
            }
        }

        private void RemoteTelnetSession(ClientRequestInfo requestinfo)
        {
            if ((object)m_remoteCommandProcess == null && requestinfo.Request.Arguments.ContainsHelpRequest)
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

                UpdateStatus(requestinfo.Sender.ClientID, UpdateType.Information, helpMessage.ToString());
            }
            else
            {
                bool connectSession = requestinfo.Request.Arguments.Exists("connect");
                bool disconnectSession = requestinfo.Request.Arguments.Exists("disconnect");

                if ((object)m_remoteCommandProcess == null && connectSession && !string.IsNullOrEmpty(requestinfo.Request.Arguments["connect"]))
                {
                    // User wants to establish a remote command session.
                    string password = requestinfo.Request.Arguments["connect"];

                    if (password == m_telnetSessionPassword)
                    {
                        // Establish remote command session
                        m_remoteCommandProcess = new Process();
                        m_remoteCommandProcess.ErrorDataReceived += RemoteCommandProcess_ErrorDataReceived;
                        m_remoteCommandProcess.OutputDataReceived += RemoteCommandProcess_OutputDataReceived;
                        m_remoteCommandProcess.StartInfo.FileName = "cmd.exe";
                        m_remoteCommandProcess.StartInfo.UseShellExecute = false;
                        m_remoteCommandProcess.StartInfo.RedirectStandardInput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardOutput = true;
                        m_remoteCommandProcess.StartInfo.RedirectStandardError = true;
                        m_remoteCommandProcess.Start();
                        m_remoteCommandProcess.BeginOutputReadLine();
                        m_remoteCommandProcess.BeginErrorReadLine();

                        UpdateStatus(UpdateType.Information, "Remote command session established - status updates are suspended.\r\n\r\n");

                        m_remoteCommandClientID = requestinfo.Sender.ClientID;
                        SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("TelnetSession", "Established"));
                    }
                    else
                    {
                        UpdateStatus(requestinfo.Sender.ClientID, UpdateType.Alarm, "Failed to establish remote command session - Password is invalid.\r\n\r\n");
                    }
                }
                else if (string.Compare(requestinfo.Request.Command, "Telnet", StringComparison.OrdinalIgnoreCase) == 0 && (object)m_remoteCommandProcess != null && disconnectSession)
                {
                    // User wants to terminate an established remote command session.                   
                    m_remoteCommandProcess.ErrorDataReceived -= RemoteCommandProcess_ErrorDataReceived;
                    m_remoteCommandProcess.OutputDataReceived -= RemoteCommandProcess_OutputDataReceived;

                    if (!m_remoteCommandProcess.HasExited)
                        m_remoteCommandProcess.Kill();

                    m_remoteCommandProcess.Dispose();
                    m_remoteCommandProcess = null;

                    m_remoteCommandClientID = Guid.Empty;
                    SendResponse(requestinfo.Sender.ClientID, new ServiceResponse("TelnetSession", "Terminated"));

                    UpdateStatus(UpdateType.Information, "Remote command session terminated - status updates are resumed.\r\n\r\n");
                }
                else if ((object)m_remoteCommandProcess != null)
                {
                    // User has entered commands that must be redirected to the established command session.
                    string input = requestinfo.Request.Command + " " + requestinfo.Request.Arguments;
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
