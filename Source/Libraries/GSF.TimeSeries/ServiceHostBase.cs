//******************************************************************************************************
//  ServiceHost.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/30/2011 - Pinal C. Patel
//       Updated to subscribe to new ProcessException event of MultiDestinationExporter class.
//  04/21/2011 - Timothy M. Yardley / Erich A. Heine (UIUC)
//       Implemented an optional, but enabled by default, non-broadcast method of directly routed
//       measurement distribution as a system performance optimization.
//
//******************************************************************************************************

using GSF.Communication;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Reflection;
using GSF.ServiceProcess;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GSF.TimeSeries
{
    #region [ Enumerations ]

    /// <summary>
    /// Configuration data source type enumeration.
    /// </summary>
    public enum ConfigurationType
    {
        /// <summary>
        /// Configuration source is a database.
        /// </summary>
        Database,
        /// <summary>
        /// Configuration source is a webservice.
        /// </summary>
        WebService,
        /// <summary>
        /// Configuration source is a XML file.
        /// </summary>
        XmlFile
    }

    #endregion

    /// <summary>
    /// Defines a method signature for a bootstrap data source operation.
    /// </summary>
    /// <param name="connection">Connection to database.</param>
    /// <param name="adapterType">Adapter type for database connection.</param>
    /// <param name="nodeIDQueryString">Formatted node ID guid query string.</param>
    /// <param name="arguments">Optional data operation arguments.</param>
    /// <param name="statusMessage">Reference to host status message function.</param>
    /// <param name="processException">Reference to host process exception function.</param>
    public delegate void DataOperationFunction(IDbConnection connection, Type adapterType, string nodeIDQueryString, string arguments, Action<object, EventArgs<string>> statusMessage, Action<object, EventArgs<Exception>> processException);

    /// <summary>
    /// Represents the time-series framework service host.
    /// </summary>
    public partial class ServiceHostBase : ServiceBase
    {
        #region [ Members ]

        // Constants
        private const int DefaultMinThreadPoolSize = 25;
        private const int DefaultMaxThreadPoolSize = 2048;
        private const int DefaultGCGenZeroInterval = 500;
        private const int DefaultConfigurationBackups = 5;

        // Fields
        private IaonSession m_iaonSession;
        private string m_nodeIDQueryString;
        private ConfigurationType m_configurationType;
        private string m_connectionString;
        private string m_dataProviderString;
        private string m_cachedConfigurationFile;
        private int m_configurationBackups;
        private bool m_uniqueAdapterIDs;
        private bool m_allowRemoteRestart;
        private System.Timers.Timer m_gcGenZeroTimer;
        private MultipleDestinationExporter m_healthExporter;
        private MultipleDestinationExporter m_statusExporter;
        private AutoResetEvent m_configurationCacheComplete;
        private object m_queuedConfigurationCachePending;
        private object m_latestConfiguration;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ServiceHostBase"/>.
        /// </summary>
        public ServiceHostBase()
        {
            InitializeComponent();

            // Register service level event handlers
            m_serviceHelper.ServiceStarting += ServiceStartingHandler;
            m_serviceHelper.ServiceStarted += ServiceStartedHandler;
            m_serviceHelper.ServiceStopping += ServiceStoppingHandler;

            if (m_serviceHelper.StatusLog != null)
                m_serviceHelper.StatusLog.LogException += LogExceptionHandler;

            if (m_serviceHelper.ErrorLogger != null && m_serviceHelper.ErrorLogger.ErrorLog != null)
                m_serviceHelper.ErrorLogger.ErrorLog.LogException += LogExceptionHandler;
        }

        /// <summary>
        /// Creates a new <see cref="ServiceHostBase"/> from specified parameters.
        /// </summary>
        /// <param name="container">Service host <see cref="IContainer"/>.</param>
        public ServiceHostBase(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the related remote console application name.
        /// </summary>
        protected virtual string ConsoleApplicationName
        {
            get
            {
                return ServiceName + "Console.exe";
            }
        }

        /// <summary>
        /// Gets access to the <see cref="GSF.ServiceProcess.ServiceHelper"/>.
        /// </summary>
        protected ServiceHelper ServiceHelper
        {
            get
            {
                return m_serviceHelper;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="TcpServer"/> based remoting server.
        /// </summary>
        protected TcpServer RemotingServer
        {
            get
            {
                return m_remotingServer;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="AllAdaptersCollection"/>.
        /// </summary>
        protected AllAdaptersCollection AllAdapters
        {
            get
            {
                return m_iaonSession.AllAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="InputAdapterCollection"/>.
        /// </summary>
        protected InputAdapterCollection InputAdapters
        {
            get
            {
                return m_iaonSession.InputAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        protected ActionAdapterCollection ActionAdapters
        {
            get
            {
                return m_iaonSession.ActionAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="OutputAdapterCollection"/>.
        /// </summary>
        protected OutputAdapterCollection OutputAdapters
        {
            get
            {
                return m_iaonSession.OutputAdapters;
            }
        }

        /// <summary>
        /// Gets the current node ID.
        /// </summary>
        protected Guid NodeID
        {
            get
            {
                return m_iaonSession.NodeID;
            }
        }

        /// <summary>
        /// Gets the current node ID formatted for use in a SQL query string based on <see cref="ServiceHostBase.ConfigurationType"/>.
        /// </summary>
        protected string NodeIDQueryString
        {
            get
            {
                return m_nodeIDQueryString;
            }
        }

        /// <summary>
        /// Gets the currently loaded system configuration <see cref="DataSet"/>.
        /// </summary>
        protected DataSet DataSource
        {
            get
            {
                return m_iaonSession.DataSource;
            }
        }

        /// <summary>
        /// Gets the defined system <see cref="GSF.TimeSeries.ConfigurationType"/>.
        /// </summary>
        protected ConfigurationType ConfigurationType
        {
            get
            {
                return m_configurationType;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Service Event Handlers ]

        /// <summary>
        /// Event handler for service starting operations.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments containing command line arguments passed into service at startup.</param>
        /// <remarks>
        /// Time-series framework uses this handler to load settings from configuration file as service is starting.
        /// </remarks>
        protected virtual void ServiceStartingHandler(object sender, EventArgs<string[]> e)
        {
            // Initialize Iaon session
            m_iaonSession = new IaonSession();
            m_iaonSession.StatusMessage += m_iaonSession_StatusMessage;
            m_iaonSession.ProcessException += m_iaonSession_ProcessException;

            // Create a handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Make sure default service settings exist
            ConfigurationFile configFile = ConfigurationFile.Current;
            string cachePath = string.Format("{0}\\ConfigurationCache\\", FilePath.GetAbsolutePath(""));

            // System settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("ConfigurationType", "Database", "Specifies type of configuration: Database, WebService or XmlFile");
            systemSettings.Add("ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=IaonHost.mdb", "Configuration database connection string");
            systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Configuration database ADO.NET data provider assembly type creation string");
            systemSettings.Add("ConfigurationCachePath", cachePath, "Defines the path used to cache serialized configurations");
            systemSettings.Add("CachedConfigurationFile", "SystemConfiguration.xml", "File name for last known good system configuration (only cached for a Database or WebService connection)");
            systemSettings.Add("UniqueAdaptersIDs", "True", "Set to true if all runtime adapter ID's will be unique to allow for easier adapter specification");
            systemSettings.Add("ProcessPriority", "High", "Sets desired process priority: Normal, AboveNormal, High, RealTime");
            systemSettings.Add("AllowRemoteRestart", "True", "Controls ability to remotely restart the host service.");
            systemSettings.Add("MinThreadPoolWorkerThreads", DefaultMinThreadPoolSize, "Defines the minimum number of allowed thread pool worker threads.");
            systemSettings.Add("MaxThreadPoolWorkerThreads", DefaultMaxThreadPoolSize, "Defines the maximum number of allowed thread pool worker threads.");
            systemSettings.Add("MinThreadPoolIOPortThreads", DefaultMinThreadPoolSize, "Defines the minimum number of allowed thread pool I/O completion port threads (used by socket layer).");
            systemSettings.Add("MaxThreadPoolIOPortThreads", DefaultMaxThreadPoolSize, "Defines the maximum number of allowed thread pool I/O completion port threads (used by socket layer).");
            systemSettings.Add("GCGenZeroInterval", DefaultGCGenZeroInterval, "Defines the interval, in milliseconds, over which to force a generation zero garbage collection. Set to -1 to disable.");
            systemSettings.Add("ConfigurationBackups", DefaultConfigurationBackups, "Defines the total number of older backup configurations to maintain.");

            // Example connection settings
            CategorizedSettingsElementCollection exampleSettings = configFile.Settings["exampleConnectionSettings"];
            exampleSettings.Add("SqlServer.ConnectionString", "Data Source=serverName; Initial Catalog=databaseName; User ID=userName; Password=password", "Example SQL Server database connection string");
            exampleSettings.Add("SqlServer.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter", "Example SQL Server database .NET provider string");
            exampleSettings.Add("MySQL.ConnectionString", "Server=serverName;Database=databaseName; Uid=root; Pwd=password; allow user variables = true;", "Example MySQL database connection string");
            exampleSettings.Add("MySQL.DataProviderString", "AssemblyName={MySql.Data, Version=6.3.6.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter", "Example MySQL database .NET provider string");
            exampleSettings.Add("Oracle.ConnectionString", "Data Source=tnsName; User ID=schemaUserName; Password=schemaPassword", "Example Oracle database connection string");
            exampleSettings.Add("Oracle.DataProviderString", "AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter", "Example Oracle database .NET provider string");
            exampleSettings.Add("SQLite.ConnectionString", "Data Source=databaseName.db; Version=3", "Example SQLite database connection string");
            exampleSettings.Add("SQLite.DataProviderString", "AssemblyName={System.Data.SQLite, Version=1.0.74.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter", "Example SQLite database .NET provider string");
            exampleSettings.Add("OleDB.ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=databaseName.mdb", "Example Microsoft Access (via OleDb) database connection string");
            exampleSettings.Add("OleDB.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.OleDb.OleDbConnection; AdapterType=System.Data.OleDb.OleDbDataAdapter", "Example OleDb database .NET provider string");
            exampleSettings.Add("Odbc.ConnectionString", "Driver={SQL Server Native Client 10.0}; Server=serverName; Database=databaseName; Uid=userName; Pwd=password;", "Example ODBC database connection string");
            exampleSettings.Add("Odbc.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter", "Example ODBC database .NET provider string");
            exampleSettings.Add("WebService.ConnectionString", "http://localhost/ConfigSource/SystemConfiguration.xml", "Example web service connection string");
            exampleSettings.Add("XmlFile.ConnectionString", "SystemConfiguration.xml", "Example XML configuration file connection string");

            // Retrieve configuration cache directory as defined in the config file
            cachePath = systemSettings["ConfigurationCachePath"].Value;

            // Make sure configuration cache directory exists
            try
            {
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to create configuration cache directory due to exception: {0}", UpdateType.Alarm, ex.Message);
                m_serviceHelper.ErrorLogger.Log(ex);
            }

            // Initialize system settings
            m_configurationType = systemSettings["ConfigurationType"].ValueAs<ConfigurationType>();
            m_connectionString = systemSettings["ConnectionString"].Value;
            m_dataProviderString = systemSettings["DataProviderString"].Value;
            m_cachedConfigurationFile = FilePath.AddPathSuffix(cachePath) + systemSettings["CachedConfigurationFile"].Value;
            m_configurationBackups = systemSettings["ConfigurationBackups"].ValueAs<int>(DefaultConfigurationBackups);
            m_uniqueAdapterIDs = systemSettings["UniqueAdaptersIDs"].ValueAsBoolean(true);
            m_allowRemoteRestart = systemSettings["AllowRemoteRestart"].ValueAsBoolean(true);
            m_configurationCacheComplete = new AutoResetEvent(true);
            m_queuedConfigurationCachePending = new object();

            // Setup default thread pool size
            try
            {
                ThreadPool.SetMinThreads(systemSettings["MinThreadPoolWorkerThreads"].ValueAs<int>(DefaultMinThreadPoolSize), systemSettings["MinThreadPoolIOPortThreads"].ValueAs<int>(DefaultMinThreadPoolSize));
                ThreadPool.SetMaxThreads(systemSettings["MaxThreadPoolWorkerThreads"].ValueAs<int>(DefaultMaxThreadPoolSize), systemSettings["MaxThreadPoolIOPortThreads"].ValueAs<int>(DefaultMaxThreadPoolSize));
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to set desired thread pool size due to exception: {0}", UpdateType.Alarm, ex.Message);
                m_serviceHelper.ErrorLogger.Log(ex);
            }

            // Define a generation zero garbage collection timer
            int gcGenZeroInterval = systemSettings["GCGenZeroInterval"].ValueAs<int>(DefaultGCGenZeroInterval);

            if (gcGenZeroInterval > 0)
            {
                m_gcGenZeroTimer = new System.Timers.Timer();
                m_gcGenZeroTimer.Elapsed += m_gcGenZeroTimer_Elapsed;
                m_gcGenZeroTimer.Interval = gcGenZeroInterval;
                m_gcGenZeroTimer.Enabled = true;
            }

            // Define guid with query string delimeters according to database needs
            Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
            string setting;

            if (settings.TryGetValue("Provider", out setting))
            {
                // Check if provider is for Access since it uses braces as Guid delimeters
                if (setting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                {
                    m_nodeIDQueryString = "{" + m_iaonSession.NodeID + "}";

                    // Make sure path to Access database is fully qualified
                    if (settings.TryGetValue("Data Source", out setting))
                    {
                        settings["Data Source"] = FilePath.GetAbsolutePath(setting);
                        m_connectionString = settings.JoinKeyValuePairs();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(m_nodeIDQueryString))
                m_nodeIDQueryString = "'" + m_iaonSession.NodeID + "'";

#if !MONO
            try
            {
                // Attempt to assign desired process priority. Note that process will require SeIncreaseBasePriorityPrivilege or 
                // Administrative privileges to make this change
                Process.GetCurrentProcess().PriorityClass = systemSettings["ProcessPriority"].ValueAs<ProcessPriorityClass>();
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex, false);
            }
#endif
        }

        /// <summary>
        /// Event handler for service started operation.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Time-series framework uses this handler to handle initialization of system objects.
        /// </remarks>
        protected virtual void ServiceStartedHandler(object sender, EventArgs e)
        {
            // Define a line of asterisks for emphasis
            string stars = new string('*', 79);

            // Log startup information
            m_serviceHelper.UpdateStatus(
                UpdateType.Information,
                "\r\n\r\n{0}\r\n\r\n" +
                "Node {{{1}}} Initializing\r\n\r\n" +
                "     System Time: {2} UTC\r\n\r\n" +
                "    Current Path: {3}\r\n\r\n" +
                "    Machine Name: {4}\r\n\r\n" +
                "      OS Version: {5}\r\n\r\n" +
                "    Product Name: {6}\r\n\r\n" +
                "  Working Memory: {7}\r\n\r\n" +
                "  Execution Mode: {8}-bit\r\n\r\n" +
                "      Processors: {9}\r\n\r\n" +
                "  GC Server Mode: {10}\r\n\r\n" +
                " GC Latency Mode: {11}\r\n\r\n" +
                " Process Account: {12}\\{13}\r\n\r\n" +
                "{14}\r\n",
                stars,
                m_iaonSession.NodeID,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                FilePath.TrimFileName(FilePath.RemovePathSuffix(FilePath.GetAbsolutePath("")), 61),
                Environment.MachineName,
                Environment.OSVersion.VersionString,
                Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", null).ToNonNullString("<Unavailable>"),
                SI2.ToScaledIECString(Environment.WorkingSet, 3, "B"),
                IntPtr.Size * 8,
                Environment.ProcessorCount,
                GCSettings.IsServerGC,
                GCSettings.LatencyMode,
                Environment.UserDomainName,
                Environment.UserName,
                stars);

            // Create health exporter
            m_healthExporter = new MultipleDestinationExporter("HealthExporter", Timeout.Infinite);
            m_healthExporter.Initialize(new ExportDestination[] { new ExportDestination(FilePath.GetAbsolutePath("Health.txt"), false) });
            m_healthExporter.StatusMessage += m_iaonSession.StatusMessageHandler;
            m_healthExporter.ProcessException += m_iaonSession.ProcessExceptionHandler;
            m_serviceHelper.ServiceComponents.Add(m_healthExporter);

            // Create status exporter
            m_statusExporter = new MultipleDestinationExporter("StatusExporter", Timeout.Infinite);
            m_statusExporter.Initialize(new ExportDestination[] { new ExportDestination(FilePath.GetAbsolutePath("Status.txt"), false) });
            m_statusExporter.StatusMessage += m_iaonSession.StatusMessageHandler;
            m_statusExporter.ProcessException += m_iaonSession.ProcessExceptionHandler;
            m_serviceHelper.ServiceComponents.Add(m_statusExporter);

            // Define scheduled service processes
            m_serviceHelper.AddScheduledProcess(HealthMonitorProcessHandler, "HealthMonitor", "* * * * *");    // Every minute
            m_serviceHelper.AddScheduledProcess(StatusExportProcessHandler, "StatusExport", "*/30 * * * *");   // Every 30 minutes

            // Add key Iaon collections as service components
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.InputAdapters);
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.ActionAdapters);
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.OutputAdapters);

            // Define remote client requests (i.e., console commands)
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("List", "Displays status for specified adapter or collection", ListRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Connect", "Connects (or starts) specified adapter", StartRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Disconnect", "Disconnects (or stops) specified adapter", StopRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Invoke", "Invokes a command for specified adapter", InvokeRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ListCommands", "Displays possible commands for specified adapter", ListCommandsRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Initialize", "Initializes specified adapter or collection", InitializeRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ReloadConfig", "Manually reloads the system configuration", ReloadConfigRequstHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("UpdateConfigFile", "Updates an option in the configuration file", UpdateConfigFileRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Authenticate", "Authenticates network shares for health and status exports", AuthenticateRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Restart", "Attempts to restart the host service", RestartServiceHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("RefreshRoutes", "Spawns request to recalculate routing tables", RefreshRoutesRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("TemporalSupport", "Detemines if any adapters support temporal processing", TemporalSupportRequestHandler));

            try
            {
                // Start system initialization on an independent thread so that service responds in a timely fashion...
                ThreadPool.QueueUserWorkItem(InitializeSystem);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                DisplayStatusMessage("Failed to queue system initialization due to exception: {0}", UpdateType.Alarm, ex.Message);
                m_serviceHelper.ErrorLogger.Log(ex);
            }
        }

        /// <summary>
        /// Event handler for service stopping operation.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Time-series framework uses this handler to un-wire events and dispose of system objects.
        /// </remarks>
        protected virtual void ServiceStoppingHandler(object sender, EventArgs e)
        {
            // Stop generation zero garbage collection timer
            if (m_gcGenZeroTimer != null)
            {
                m_gcGenZeroTimer.Enabled = false;
                m_gcGenZeroTimer.Elapsed -= m_gcGenZeroTimer_Elapsed;
                m_gcGenZeroTimer.Dispose();
            }
            m_gcGenZeroTimer = null;

            // Dispose system health exporter
            if (m_healthExporter != null)
            {
                m_healthExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_healthExporter);
                m_healthExporter.Dispose();
                m_healthExporter.StatusMessage -= m_iaonSession.StatusMessageHandler;
                m_healthExporter.ProcessException -= m_iaonSession.ProcessExceptionHandler;
            }
            m_healthExporter = null;

            // Dispose system status exporter
            if (m_statusExporter != null)
            {
                m_statusExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_statusExporter);
                m_statusExporter.Dispose();
                m_statusExporter.StatusMessage -= m_iaonSession.StatusMessageHandler;
                m_statusExporter.ProcessException -= m_iaonSession.ProcessExceptionHandler;
            }
            m_statusExporter = null;

            // Dispose Iaon session
            if (m_iaonSession != null)
            {
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.InputAdapters);
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.ActionAdapters);
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.OutputAdapters);
                m_iaonSession.Dispose();
                m_iaonSession.StatusMessage -= m_iaonSession_StatusMessage;
                m_iaonSession.ProcessException -= m_iaonSession_ProcessException;
            }
            m_iaonSession = null;

            m_serviceHelper.ServiceStarting -= ServiceStartingHandler;
            m_serviceHelper.ServiceStarted -= ServiceStartedHandler;
            m_serviceHelper.ServiceStopping -= ServiceStoppingHandler;

            if (m_serviceHelper.StatusLog != null)
            {
                m_serviceHelper.StatusLog.Flush();
                m_serviceHelper.StatusLog.LogException -= LogExceptionHandler;
            }

            if (m_serviceHelper.ErrorLogger != null && m_serviceHelper.ErrorLogger.ErrorLog != null)
            {
                m_serviceHelper.ErrorLogger.ErrorLog.Flush();
                m_serviceHelper.ErrorLogger.ErrorLog.LogException -= LogExceptionHandler;
            }

            if ((object)m_configurationCacheComplete != null)
            {
                // Release any waiting threads before disposing wait handle
                m_configurationCacheComplete.Set();
                m_configurationCacheComplete.Dispose();
            }

            m_configurationCacheComplete = null;

            // Unattach from handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
        }

        // Generation zero garbage collection handler
        [SuppressMessage("Microsoft.Reliability", "CA2002")]
        private void m_gcGenZeroTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // The time-series framework can allocate hundreds of thousands of measurements per second, non-stop, as a result the typical
            // wait for breathing room style algorithms for timing garbage collections are not practical. A simple forced generation zero
            // collection on a timer is often the best way to stabilize garbage collection so that it can stay ahead of the curve and
            // reduce overall garbage collection times, which pauses all threads, for the large volumes of short lifespan data.
            if (Monitor.TryEnter(m_gcGenZeroTimer))
            {
                try
                {
                    GC.Collect(0);
                }
                finally
                {
                    Monitor.Exit(m_gcGenZeroTimer);
                }
            }
        }

        #endregion

        #region [ System Initialization ]

        // Perform system initialization
        private void InitializeSystem(object state)
        {
            // Attempt to load system configuration
            if (LoadSystemConfiguration())
            {
                // Initialize and start all session adapters
                m_iaonSession.Initialize();

                DisplayStatusMessage("System initialization complete.", UpdateType.Information);

                // If any settings have been added to configuration file, we go ahead and save them now
                m_serviceHelper.SaveSettings(true);
                ConfigurationFile.Current.Save();
            }
            else
                DisplayStatusMessage("System initialization failed due to unavailable configuration.", UpdateType.Alarm);

            // Log current thread pool size
            int minWorkerThreads, minIOThreads, maxWorkerThreads, maxIOThreads;

            ThreadPool.GetMinThreads(out minWorkerThreads, out minIOThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxIOThreads);

            DisplayStatusMessage("Thread pool size: minimum {0} worker {1} I/O, maximum {2} worker {3} I/O", UpdateType.Information, minWorkerThreads, minIOThreads, maxWorkerThreads, maxIOThreads);
        }

        // Load the the system configuration data set
        private bool LoadSystemConfiguration()
        {
            DisplayStatusMessage("Loading system configuration...", UpdateType.Information);

            // Attempt to load (or reload) system configuration
            DataSet dataSource = GetConfigurationDataSet(m_configurationType, m_connectionString, m_dataProviderString);

            if (dataSource != null)
            {
                // Update data source on all adapters in all collections
                m_iaonSession.DataSource = dataSource;
                return true;
            }

            return false;
        }

        // Load system configuration data set
        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        private DataSet GetConfigurationDataSet(ConfigurationType configType, string connectionString, string dataProviderString)
        {
            DataSet configuration = null;

            switch (configType)
            {
                case ConfigurationType.Database:
                    // Attempt to load configuration from a database connection
                    IDbConnection connection = null;
                    Dictionary<string, string> settings;
                    string assemblyName, connectionTypeName, adapterTypeName;
                    Assembly assembly;
                    Type connectionType, adapterType;
                    DataTable entities, source, destination;

                    try
                    {
                        settings = dataProviderString.ParseKeyValuePairs();
                        assemblyName = settings["AssemblyName"].ToNonNullString();
                        connectionTypeName = settings["ConnectionType"].ToNonNullString();
                        adapterTypeName = settings["AdapterType"].ToNonNullString();

                        if (string.IsNullOrWhiteSpace(connectionTypeName))
                            throw new InvalidOperationException("Database connection type was not defined.");

                        if (string.IsNullOrWhiteSpace(adapterTypeName))
                            throw new InvalidOperationException("Database adapter type was not defined.");

                        assembly = Assembly.Load(new AssemblyName(assemblyName));
                        connectionType = assembly.GetType(connectionTypeName);
                        adapterType = assembly.GetType(adapterTypeName);

                        connection = (IDbConnection)Activator.CreateInstance(connectionType);
                        connection.ConnectionString = m_connectionString;
                        connection.Open();

                        DisplayStatusMessage("Database configuration connection opened.", UpdateType.Information);

                        // Execute any defined startup data operations
                        ExecuteStartupDataOperations(connection, adapterType);

                        configuration = new DataSet("Iaon");

                        // Load configuration entities defined in database
                        entities = connection.RetrieveData(adapterType, "SELECT * FROM ConfigurationEntity WHERE Enabled <> 0 ORDER BY LoadOrder");
                        entities.TableName = "ConfigurationEntity";

                        // Add configuration entities table to system configuration for reference
                        configuration.Tables.Add(entities.Copy());

                        Ticks startTime;
                        double elapsedTime;

                        // Add each configuration entity to the system configuration
                        foreach (DataRow entityRow in entities.Rows)
                        {
                            // Load configuration entity data filtered by node ID
                            startTime = PrecisionTimer.UtcNow.Ticks;
                            source = connection.RetrieveData(adapterType, string.Format("SELECT * FROM {0} WHERE NodeID={1}", entityRow["SourceName"].ToString(), m_nodeIDQueryString));
                            elapsedTime = (PrecisionTimer.UtcNow.Ticks - startTime).ToSeconds();

                            // Update table name as defined in configuration entity
                            source.TableName = entityRow["RuntimeName"].ToString();

                            DisplayStatusMessage("Loaded {0} row{1} from \"{2}\" in {3}...", UpdateType.Information, source.Rows.Count, source.Rows.Count == 1 ? "" : "s", source.TableName, elapsedTime < 0.01D ? "less than a second" : elapsedTime.ToString("0.00") + " seconds");

                            startTime = PrecisionTimer.UtcNow.Ticks;

                            // Clone data source
                            destination = source.Clone();

                            // Get destination column collection
                            DataColumnCollection columns = destination.Columns;

                            // Remove redundant node ID column
                            columns.Remove("NodeID");

                            // Pre-cache column index translation after removal of NodeID column to speed data copy
                            Dictionary<int, int> columnIndex = new Dictionary<int, int>();

                            foreach (DataColumn column in columns)
                            {
                                columnIndex[column.Ordinal] = source.Columns[column.ColumnName].Ordinal;
                            }

                            // Manually copy-in each row into table
                            foreach (DataRow sourceRow in source.Rows)
                            {
                                DataRow newRow = destination.NewRow();

                                // Copy each column of data in the current row
                                for (int x = 0; x < columns.Count; x++)
                                {
                                    newRow[x] = sourceRow[columnIndex[x]];
                                }

                                // Add new row to destination table
                                destination.Rows.Add(newRow);
                            }

                            elapsedTime = (PrecisionTimer.UtcNow.Ticks - startTime).ToSeconds();

                            // Add entity configuration data to system configuration
                            configuration.Tables.Add(destination);

                            DisplayStatusMessage("Configuration cache completed in {0}.", UpdateType.Information, elapsedTime < 0.01D ? "less than a second" : elapsedTime.ToString("0.00") + " seconds");
                        }

                        DisplayStatusMessage("Database configuration successfully loaded.", UpdateType.Information);

                        CacheCurrentConfiguration(configuration);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to load database configuration due to exception: {0} Attempting to use last known good configuration.", UpdateType.Warning, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                        configuration = GetConfigurationDataSet(ConfigurationType.XmlFile, m_cachedConfigurationFile, null);
                    }
                    finally
                    {
                        if (connection != null)
                            connection.Dispose();

                        DisplayStatusMessage("Database configuration connection closed.", UpdateType.Information);
                    }

                    break;
                case ConfigurationType.WebService:
                    // Attempt to load configuration from webservice based connection
                    WebRequest request = null;
                    Stream response = null;
                    try
                    {
                        DisplayStatusMessage("Webservice configuration connection opened.", UpdateType.Information);

                        configuration = new DataSet();
                        request = WebRequest.Create(connectionString);
                        response = request.GetResponse().GetResponseStream();
                        configuration.ReadXml(response);

                        DisplayStatusMessage("Webservice configuration successfully loaded.", UpdateType.Information);

                        CacheCurrentConfiguration(configuration);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to load webservice configuration due to exception: {0} Attempting to use last known good configuration.", UpdateType.Warning, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                        configuration = GetConfigurationDataSet(ConfigurationType.XmlFile, m_cachedConfigurationFile, null);
                    }
                    finally
                    {
                        if (response != null)
                            response.Dispose();

                        DisplayStatusMessage("Webservice configuration connection closed.", UpdateType.Information);
                    }

                    break;
                case ConfigurationType.XmlFile:
                    // Attempt to load cached configuration file
                    try
                    {
                        DisplayStatusMessage("Loading XML based configuration from \"{0}\".", UpdateType.Information, connectionString);

                        configuration = new DataSet();
                        configuration.ReadXml(connectionString);

                        DisplayStatusMessage("XML based configuration successfully loaded.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to load XML based configuration due to exception: {0}.", UpdateType.Alarm, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                        configuration = null;
                    }

                    break;
            }

            return configuration;
        }

        // Execute any defined startup data operations
        private void ExecuteStartupDataOperations(IDbConnection connection, Type adapterType)
        {
            try
            {
                string assemblyName = "", typeName = "", methodName = "", arguments;
                Assembly assembly;
                Type type;
                MethodInfo method;

                foreach (DataRow row in connection.RetrieveData(adapterType, string.Format("SELECT * FROM DataOperation WHERE (NodeID IS NULL OR NodeID={0}) AND Enabled <> 0 ORDER BY LoadOrder", m_nodeIDQueryString)).Rows)
                {
                    try
                    {
                        DisplayStatusMessage("Executing startup data operation \"{0}\".", UpdateType.Information, row["Description"].ToNonNullString("Unlabled"));

                        // Load data operation parameters
                        assemblyName = row["AssemblyName"].ToNonNullString();
                        typeName = row["TypeName"].ToNonNullString();
                        methodName = row["MethodName"].ToNonNullString();
                        arguments = row["Arguments"].ToNonNullString();

                        if (string.IsNullOrWhiteSpace(assemblyName))
                            throw new InvalidOperationException("Data operation assembly name was not defined.");

                        if (string.IsNullOrWhiteSpace(typeName))
                            throw new InvalidOperationException("Data operation type name was not defined.");

                        if (string.IsNullOrWhiteSpace(methodName))
                            throw new InvalidOperationException("Data operation method name was not defined.");

                        // Load data operation from containing assembly and type
                        assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));
                        type = assembly.GetType(typeName);
                        method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

                        // Execute data operation via loaded assembly method
                        ((DataOperationFunction)Delegate.CreateDelegate(typeof(DataOperationFunction), method))(connection, adapterType, m_nodeIDQueryString, arguments, m_iaonSession.StatusMessageHandler, m_iaonSession.ProcessExceptionHandler);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to execute startup data operation \"{0} [{1}::{2}()]\" due to exception: {3}", UpdateType.Warning, assemblyName, typeName, methodName, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to execute startup data operations due to exception: {0}", UpdateType.Warning, ex.Message);
                m_serviceHelper.ErrorLogger.Log(ex);
            }
        }

        /// <summary>
        /// Caches the current system configuration.
        /// </summary>
        /// <param name="configuration">Configuration <see cref="DataSet"/>.</param>
        /// <remarks>
        /// This method allows caching of the current system configuration so it can be used if primary configuration source is unavailable.
        /// </remarks>
        protected virtual void CacheCurrentConfiguration(DataSet configuration)
        {
            try
            {
                // Queue configuration serialization using latest dataset
                ThreadPool.QueueUserWorkItem(QueueConfigurationCache, configuration);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to queue configuration caching due to exception: {0}", UpdateType.Alarm, ex.Message);
                m_serviceHelper.ErrorLogger.Log(ex);
            }
        }

        // Since configuration serialization may take a while, we queue-up activity for one-at-a-time processing using latest dataset
        private void QueueConfigurationCache(object state)
        {
            // Always attempt cache for most recent configuration
            Interlocked.Exchange(ref m_latestConfiguration, state);

            // Queue up a configuration cache unless another thread has already requested one
            if (Monitor.TryEnter(m_queuedConfigurationCachePending, 500))
            {
                try
                {
                    // Queue new configration cache after waiting for any prior cache operation to complete
                    if (m_configurationCacheComplete.WaitOne())
                    {
                        object latestConfiguration = null;

                        // Get latest configuration
                        Interlocked.Exchange(ref latestConfiguration, m_latestConfiguration);

                        try
                        {
                            // Queue up task to to execute cache of the latest configuration
                            ThreadPool.QueueUserWorkItem(ExecuteConfigurationCache, latestConfiguration);
                        }
                        catch (Exception ex)
                        {
                            DisplayStatusMessage("Failed to queue configuration caching due to exception: {0}", UpdateType.Alarm, ex.Message);
                            m_serviceHelper.ErrorLogger.Log(ex);
                        }

                        // Dereference data set configuration if another one hasn't been queued-up in the mean time
                        Interlocked.CompareExchange(ref m_latestConfiguration, null, latestConfiguration);
                    }
                }
                finally
                {
                    Monitor.Exit(m_queuedConfigurationCachePending);
                }
            }
        }

        // Executes actual serialization of current configuration
        private void ExecuteConfigurationCache(object state)
        {
            try
            {
                DataSet configuration = state as DataSet;

                if ((object)configuration != null)
                {
                    try
                    {
                        // Create multiple backup configurations, if requested
                        for (int i = m_configurationBackups; i > 0; i--)
                        {
                            string origConfigFile = m_cachedConfigurationFile + ".backup" + (i == 1 ? "" : (i - 1).ToString());

                            if (File.Exists(origConfigFile))
                            {
                                string nextConfigFile = m_cachedConfigurationFile + ".backup" + i;

                                if (File.Exists(nextConfigFile))
                                    File.Delete(nextConfigFile);

                                File.Move(origConfigFile, nextConfigFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to create extra backup configurations due to exception: {0}", UpdateType.Warning, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }

                    try
                    {
                        // Back up current configuration file, if any
                        if (File.Exists(m_cachedConfigurationFile))
                        {
                            string backupConfigFile = m_cachedConfigurationFile + ".backup";

                            if (File.Exists(backupConfigFile))
                                File.Delete(backupConfigFile);

                            File.Move(m_cachedConfigurationFile, backupConfigFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to backup last known cached configuration due to exception: {0}", UpdateType.Warning, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }

                    try
                    {
                        // Write current data set to a file
                        configuration.WriteXml(m_cachedConfigurationFile, XmlWriteMode.WriteSchema);
                        DisplayStatusMessage("Successfully cached current configuration.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to cache last known configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
            }
            finally
            {
                // Release any waiting threads
                if ((object)m_configurationCacheComplete != null)
                    m_configurationCacheComplete.Set();
            }
        }

        #endregion

        #region [ Primary Adapter Event Handlers ]

        /// <summary>
        /// Event handler for reporting status messages.
        /// </summary>
        /// <param name="sender">Event source of the status message.</param>
        /// <param name="e">Event arguments containing the status message and its type to report.</param>
        /// <remarks>
        /// The time-series framework <see cref="IaonSession"/> uses this event to report adapter status messages (e.g., to a log file or console window).
        /// </remarks>
        private void m_iaonSession_StatusMessage(object sender, EventArgs<string, UpdateType> e)
        {
            DisplayStatusMessage(e.Argument1, e.Argument2);
        }

        /// <summary>
        /// Event handler for processing reported exceptions.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        /// <remarks>
        /// The time-series framework <see cref="IaonSession"/> uses this event to report exceptions.
        /// </remarks>
        private void m_iaonSession_ProcessException(object sender, EventArgs<Exception> e)
        {
            m_serviceHelper.ErrorLogger.Log(e.Argument, false);
        }

        // Handle task scheduler exceptions
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (Exception ex in e.Exception.Flatten().InnerExceptions)
            {
                m_serviceHelper.ErrorLogger.Log(ex, false);
            }

            e.SetObserved();
        }

        /// <summary>
        /// Event handler for processing exceptions encountered while writing entries to a log file.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        protected virtual void LogExceptionHandler(object sender, EventArgs<Exception> e)
        {
            DisplayStatusMessage("Log file exception: " + e.Argument.Message, UpdateType.Alarm);
        }

        /// <summary>
        /// Event handler for scheduled health monitor display.
        /// </summary>
        /// <param name="name">Scheduled event name.</param>
        /// <param name="parameters">Scheduled event parameters.</param>
        protected virtual void HealthMonitorProcessHandler(string name, object[] parameters)
        {
            const string requestCommand = "Health";
            ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(requestCommand);

            if (requestHandler != null)
            {
                // We pretend to be a client and send a "Health" command to ourselves...
                requestHandler.HandlerMethod(ClientHelper.PretendRequest(requestCommand));

                // We also export human readable health information to a text file for external display
                m_healthExporter.ExportData(m_serviceHelper.PerformanceMonitor.Status);
            }
        }

        /// <summary>
        /// Event handler for scheduled adapter status export.
        /// </summary>
        /// <param name="name">Scheduled event name.</param>
        /// <param name="parameters">Scheduled event parameters.</param>
        protected virtual void StatusExportProcessHandler(string name, object[] parameters)
        {
            // Every thirty minutes we export a human readable service status to a text file for external display
            m_statusExporter.ExportData(m_serviceHelper.Status);
        }

        #endregion

        #region [ Remote Client Request Handlers ]

        /// <summary>
        /// Gets requested <see cref="IAdapterCollection"/>.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <returns>Requested <see cref="IAdapterCollection"/>.</returns>
        protected virtual IAdapterCollection GetRequestedCollection(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.Exists("A"))
                return m_iaonSession.ActionAdapters;

            if (requestInfo.Request.Arguments.Exists("O"))
                return m_iaonSession.OutputAdapters;

            return m_iaonSession.InputAdapters;
        }

        /// <summary>
        /// Gets requested <see cref="IAdapter"/>.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <returns>Requested <see cref="IAdapter"/>.</returns>
        protected virtual IAdapter GetRequestedAdapter(ClientRequestInfo requestInfo)
        {
            IAdapterCollection collection;
            return GetRequestedAdapter(requestInfo, out collection);
        }

        /// <summary>
        /// Gets requested <see cref="IAdapter"/> and its containing <see cref="IAdapterCollection"/>.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="collection">Containing <see cref="IAdapterCollection"/> for <see cref="IAdapter"/>.</param>
        /// <returns>Requested <see cref="IAdapter"/>.</returns>
        protected virtual IAdapter GetRequestedAdapter(ClientRequestInfo requestInfo, out IAdapterCollection collection)
        {
            IAdapter adapter;
            string adapterID = requestInfo.Request.Arguments["OrderedArg1"];
            collection = GetRequestedCollection(requestInfo);

            if (!string.IsNullOrWhiteSpace(adapterID))
            {
                uint id;

                adapterID = adapterID.Trim();

                if (adapterID.IsAllNumbers() && uint.TryParse(adapterID, out id))
                {
                    // Adapter ID is numeric, try numeric lookup by adapter ID in requested collection
                    if (collection.TryGetAdapterByID(id, out adapter))
                        return adapter;

                    // Try looking for ID in any collection if all runtime ID's are unique
                    if (m_uniqueAdapterIDs && m_iaonSession.AllAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
                        return adapter;

                    collection = GetRequestedCollection(requestInfo);
                    SendResponse(requestInfo, false, "Failed to find adapter with ID \"{0}\".", id);
                }
                else
                {
                    // Adapter ID is alpha-numeric, try text-based lookup by adapter name in requested collection
                    if (collection.TryGetAdapterByName(adapterID, out adapter))
                        return adapter;

                    // Try looking for adapter name in any collection
                    if (m_iaonSession.AllAdapters.TryGetAnyAdapterByName(adapterID, out adapter, out collection))
                        return adapter;

                    collection = GetRequestedCollection(requestInfo);
                    SendResponse(requestInfo, false, "Failed to find adapter named \"{0}\".", adapterID);
                }
            }

            return null;
        }

        /// <summary>
        /// Displays status of specified adapter or collection.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void ListRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays status of specified adapter or collection.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       List [ID] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ID:".PadRight(20));
                helpMessage.Append("ID of the adapter to display, or all adapters if not specified");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.Append("Enumerate input adapters (default)");
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.Append("Enumerate action adapters");
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.Append("Enumerate output adapters");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                StringBuilder adapterList = new StringBuilder();
                IAdapterCollection collection = GetRequestedCollection(requestInfo);
                IEnumerable<IAdapter> listItems = collection;
                bool idArgExists = requestInfo.Request.Arguments.Exists("OrderedArg1");
                int enumeratedItems = 0;

                adapterList.AppendFormat("System Uptime: {0}", m_serviceHelper.RemotingServer.RunTime.ToString());
                adapterList.AppendLine();
                adapterList.AppendLine();

                if (idArgExists)
                    adapterList.AppendFormat(">> Selected adapter from {0}", collection.Name);
                else
                    adapterList.AppendFormat(">> All defined adapters in {0} ({1} total)", collection.Name, collection.Count);

                // Make a collection of one item for individual adapters
                if (idArgExists)
                {
                    IAdapter adapter = GetRequestedAdapter(requestInfo);
                    List<IAdapter> singleItemList = new List<IAdapter>();

                    if (adapter != null)
                        singleItemList.Add(adapter);

                    listItems = singleItemList;
                }

                adapterList.AppendLine();
                adapterList.AppendLine();
                adapterList.Append("    ID     Name");
                adapterList.AppendLine();
                //                  12345678901234567890123456789012345678901234567890123456789012345678901234567890
                //                           1         2         3         4         5         6         7         8
                adapterList.Append("---------- --------------------------------------------------------------------");
                //                             123456789012345678901234567890123456789012345678901234567890123456789
                //                                      1         2         3         4         5         6
                adapterList.AppendLine();

                foreach (IAdapter adapter in listItems)
                {
                    adapterList.AppendFormat("{0} {1}", adapter.ID.ToString().CenterText(10), adapter.Name.TruncateRight(66));
                    adapterList.AppendLine();
                    adapterList.Append("           ");
                    adapterList.Append(adapter.GetShortStatus(68).TruncateRight(68));

                    // If a request was made to list a specific item, we request full status
                    if (idArgExists)
                    {
                        adapterList.AppendLine();
                        adapterList.AppendLine();
                        adapterList.Append(adapter.Status);
                    }
                    adapterList.AppendLine();

                    enumeratedItems++;
                }

                if (enumeratedItems > 0)
                    SendResponse(requestInfo, true, adapterList.ToString());
                else
                    SendResponse(requestInfo, false, "No items were available enumerate.");
            }
        }

        /// <summary>
        /// Starts specified adapter.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void StartRequestHandler(ClientRequestInfo requestInfo)
        {
            ActionRequestHandler(requestInfo, adapter => adapter.Start());
        }

        /// <summary>
        /// Stops specified adapter.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void StopRequestHandler(ClientRequestInfo requestInfo)
        {
            ActionRequestHandler(requestInfo, adapter => adapter.Stop());
        }

        /// <summary>
        /// Generic adapter request handler.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="adapterAction">Action to perform on <see cref="IAdapter"/>.</param>
        protected virtual void ActionRequestHandler(ClientRequestInfo requestInfo, Action<IAdapter> adapterAction)
        {
            string actionName = requestInfo.Request.Command.ToTitleCase();

            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.AppendFormat("Handles {0} command for specified adapter.", actionName);
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.AppendFormat("       {0} ID [Options]", actionName);
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ID:".PadRight(20));
                helpMessage.Append("ID of the adapter to execute action on");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.AppendFormat("Perform {0} command on input adapters (default)", actionName);
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.AppendFormat("Perform {0} command on action adapters", actionName);
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.AppendFormat("Perform {0} command on output adapters", actionName);
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                {
                    IAdapter adapter = GetRequestedAdapter(requestInfo);

                    if (adapter != null)
                    {
                        adapterAction(adapter);
                        SendResponse(requestInfo, true);
                    }
                }
                else
                    SendResponse(requestInfo, false, "No ID was specified for \"{0}\" command.", actionName);
            }
        }

        /// <summary>
        /// Invokes specified adapter command.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void InvokeRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Invokes specified adapter command.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Invoke [ID] Command [Params] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ID:".PadRight(20));
                helpMessage.Append("ID of the adapter to execute command on");
                helpMessage.AppendLine();
                helpMessage.AppendLine("   If ID is not specified, action will be on collection itself.");
                helpMessage.AppendLine();
                helpMessage.Append("   Command:".PadRight(20));
                helpMessage.Append("Name of the adapter command (i.e., method) to invoke");
                helpMessage.AppendLine();
                helpMessage.Append("   Params:".PadRight(20));
                helpMessage.Append("Command parameters, if any, separated by spaces");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.Append("Invoke specified command on input adapter (default)");
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.Append("Invoke specified command on action adapter");
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.Append("Invoke specified command on output adapter");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IAdapter adapter = null;
                string command = null;

                // See if specific ID for an adapter was requested
                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                {
                    if (requestInfo.Request.Arguments.Exists("OrderedArg2"))
                    {
                        adapter = GetRequestedAdapter(requestInfo);
                        command = requestInfo.Request.Arguments["OrderedArg2"];
                    }
                    else
                    {
                        adapter = GetRequestedCollection(requestInfo);
                        command = requestInfo.Request.Arguments["OrderedArg1"];
                    }
                }
                else
                    SendResponse(requestInfo, false, "No command was specified to invoke.");

                if (adapter != null)
                {
                    try
                    {
                        // See if method exists with specified name using reflection
                        MethodInfo method = adapter.GetType().GetMethod(command, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        // Invoke method
                        if (method != null)
                        {
                            AdapterCommandAttribute commandAttribute;

                            // Make sure method is marked as invokable (i.e., AdapterCommandAttribute exists on method)
                            if (method.TryGetAttribute(out commandAttribute))
                            {
                                ParameterInfo[] parameterInfo = method.GetParameters();
                                object returnValue = null;
                                bool success = true;

                                if (parameterInfo == null || (parameterInfo != null && parameterInfo.Length == 0))
                                {
                                    // Invoke parameterless adapter command
                                    returnValue = method.Invoke(adapter, null);
                                }
                                else
                                {
                                    // Create typed parameters for method and invoke
                                    if (requestInfo.Request.Arguments.OrderedArgCount - 2 >= parameterInfo.Length)
                                    {
                                        // Attempt to convert command parameters to the method parameter types
                                        object[] parameters = new object[parameterInfo.Length];
                                        string parameterValue;

                                        for (int i = 0; i < parameterInfo.Length; i++)
                                        {
                                            parameterValue = requestInfo.Request.Arguments["OrderedArg" + (3 + i)];
                                            parameters[i] = parameterValue.ConvertToType<object>(parameterInfo[i].ParameterType);
                                        }

                                        // Invoke adapter command with specified parameters
                                        returnValue = method.Invoke(adapter, parameters);
                                    }
                                    else
                                    {
                                        success = false;
                                        SendResponse(requestInfo, false, "Parameter count mismatch, \"{0}\" command expects {1} parameters.", command, parameterInfo.Length);
                                    }
                                }

                                // If invoke was successful, return actionable response
                                if (success)
                                {
                                    // Return value, if any, will be returned to requesting client as a response attachment
                                    if (returnValue == null)
                                        SendResponse(requestInfo, true, "Command \"{0}\" successfully invoked.", command);
                                    else
                                        SendResponseWithAttachment(requestInfo, true, returnValue, "Command \"{0}\" successfully invoked, return value = {1}", command, returnValue.ToNonNullString("null"));
                                }
                            }
                            else
                                SendResponse(requestInfo, false, "Specified command \"{0}\" is not marked as invokable for adapter \"{1}\" [Type = {2}].", command, adapter.Name, adapter.GetType().Name);
                        }
                        else
                            SendResponse(requestInfo, false, "Specified command \"{0}\" does not exist for adapter \"{1}\" [Type = {2}].", command, adapter.Name, adapter.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Failed to invoke command: {0}", ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Lists possible commands of specified adapter.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void ListCommandsRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Lists possible commands of specified adapter.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ListCommands [ID] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ID:".PadRight(20));
                helpMessage.Append("ID of the adapter to execute command on");
                helpMessage.AppendLine();
                helpMessage.AppendLine("   If ID is not specified, action will be on collection itself.");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.Append("Lists commands on input adapter (default)");
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.Append("Lists commands on action adapter");
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.Append("Lists commands on output adapter");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IAdapter adapter;

                // See if specific ID for an adapter was requested
                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    adapter = GetRequestedAdapter(requestInfo);
                else
                    adapter = GetRequestedCollection(requestInfo);

                if (adapter != null)
                {
                    try
                    {
                        // Get public command methods of specified adpater using reflection
                        MethodInfo[] methods = adapter.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        // Invoke method
                        StringBuilder methodList = new StringBuilder();
                        AdapterCommandAttribute commandAttribute;
                        bool firstParameter;
                        string typeName;

                        methodList.AppendFormat("Adapter \"{0}\" [Type = {1}] Command List:", adapter.Name, adapter.GetType().Name);
                        methodList.AppendLine();
                        methodList.AppendLine();

                        // Enumerate each public method
                        foreach (MethodInfo method in methods)
                        {
                            // Only display methods marked as invokable (i.e., AdapterCommandAttribute exists on method)
                            if (method.TryGetAttribute(out commandAttribute))
                            {
                                firstParameter = true;

                                methodList.Append("    ");
                                methodList.Append(method.Name);
                                methodList.Append('(');

                                // Enumerate each method parameter
                                foreach (ParameterInfo parameter in method.GetParameters())
                                {
                                    if (!firstParameter)
                                        methodList.Append(", ");

                                    typeName = parameter.ParameterType.ToString();

                                    // Assume namespace for basic System types...
                                    if (typeName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) && typeName.CharCount('.') == 1)
                                        typeName = typeName.Substring(7);

                                    methodList.Append(typeName);
                                    methodList.Append(' ');
                                    methodList.Append(parameter.Name);

                                    firstParameter = false;
                                }

                                methodList.Append(')');
                                methodList.AppendLine();

                                if (!string.IsNullOrWhiteSpace(commandAttribute.Description))
                                {
                                    methodList.Append("        ");
                                    methodList.Append(commandAttribute.Description);
                                }

                                methodList.AppendLine();
                            }
                        }

                        methodList.AppendLine();

                        SendResponse(requestInfo, true, methodList.ToString());
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Failed to list commands: {0}", ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Performs initialization or reinitialization of specified adapter or collection.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void InitializeRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Performs (re)initialization of specified adapter or collection.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Initialize [ID] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ID:".PadRight(20));
                helpMessage.Append("ID of the adapter to initialize, or all adapters if not specified");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.Append("Initialize input adapters (default)");
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.Append("Initialize action adapters");
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.Append("Initialize output adapters");
                helpMessage.AppendLine();
                helpMessage.Append("       -System".PadRight(20));
                helpMessage.Append("Performs full system initialization");
                helpMessage.AppendLine();
                helpMessage.Append("       -SkipReloadConfig".PadRight(20));
                helpMessage.Append("Skips configuration reload before initialize");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (requestInfo.Request.Arguments.Exists("System"))
                {
                    DisplayStatusMessage("Starting manual full system initialization...", UpdateType.Information);
                    InitializeSystem(null);
                    SendResponse(requestInfo, true);
                }
                else
                {
                    IAdapterCollection collection;

                    // Reload system configuration
                    if (requestInfo.Request.Arguments.Exists("SkipReloadConfig") || LoadSystemConfiguration())
                    {
                        // See if specific ID for an adapter was requested
                        if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                        {
                            string adapterID = requestInfo.Request.Arguments["OrderedArg1"];
                            uint id;

                            // Try initializing new adapter by ID searching in any collection if all runtime ID's are unique
                            if (m_uniqueAdapterIDs && uint.TryParse(adapterID, out id) && m_iaonSession.AllAdapters.TryInitializeAdapterByID(id))
                            {
                                IAdapter adapter;

                                if (m_iaonSession.AllAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
                                    SendResponse(requestInfo, true, "Adapter \"{0}\" ({1}) was successfully initialized...", adapter.Name, adapter.ID);
                                else
                                    SendResponse(requestInfo, true, "Adapter ({1}) was successfully initialized...", id);
                            }
                            else
                            {
                                IAdapter adapter = GetRequestedAdapter(requestInfo, out collection);

                                // Initialize specified adapter
                                if (adapter != null && collection != null)
                                {
                                    if (collection.TryInitializeAdapterByID(adapter.ID))
                                        SendResponse(requestInfo, true, "Adapter \"{0}\" ({1}) was successfully initialized...", adapter.Name, adapter.ID);
                                    else
                                        SendResponse(requestInfo, false, "Adapter \"{0}\" ({1}) failed to initialize.", adapter.Name, adapter.ID);
                                }
                                else
                                {
                                    SendResponse(requestInfo, false, "Requested adapter was not found.");
                                }
                            }
                        }
                        else
                        {
                            // Get specified adapter collection
                            collection = GetRequestedCollection(requestInfo);

                            if (collection != null)
                            {
                                DisplayStatusMessage("Initializing all adapters in {0}...", UpdateType.Information, collection.Name);
                                collection.Initialize();
                                DisplayStatusMessage("{0} initialization complete.", UpdateType.Information, collection.Name);
                                SendResponse(requestInfo, true);
                            }
                            else
                            {
                                SendResponse(requestInfo, false, "Requested collection was unavailable.");
                            }
                        }

                        // Spawn routing table calculation updates
                        m_iaonSession.RecalculateRoutingTables();
                    }
                    else
                    {
                        SendResponse(requestInfo, false, "Failed to load system configuration.");
                    }
                }
            }
        }

        /// <summary>
        /// Determines support for temporal processing from existing adapters.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void TemporalSupportRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Checks support for temporal processing.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       TemporalSupport [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -I".PadRight(20));
                helpMessage.Append("Checks support for input adapters (default)");
                helpMessage.AppendLine();
                helpMessage.Append("       -A".PadRight(20));
                helpMessage.Append("Checks support for action adapters");
                helpMessage.AppendLine();
                helpMessage.Append("       -O".PadRight(20));
                helpMessage.Append("Checks support for output adapters");
                helpMessage.AppendLine();
                helpMessage.Append("       -System".PadRight(20));
                helpMessage.Append("Checks support for any adapter");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (requestInfo.Request.Arguments.Exists("System"))
                {
                    bool supported = m_iaonSession.TemporalProcessingSupportExists();
                    SendResponseWithAttachment(requestInfo, true, supported, "Temporal processing support {0}", supported ? "exists" : "does not exist");
                }
                else
                {
                    string collectionName = GetRequestedCollection(requestInfo).DataMember;
                    bool supported = m_iaonSession.TemporalProcessingSupportExists(collectionName);
                    SendResponseWithAttachment(requestInfo, true, supported, "Temporal processing support {0} for {1}", supported ? "exists" : "does not exist", collectionName);
                }
            }
        }

        /// <summary>
        /// Recalculates routing tables.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void RefreshRoutesRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Recalculates routing tables.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       RefreshRoutes [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                // Spawn routing table calculation updates
                m_iaonSession.RecalculateRoutingTables();
                SendResponse(requestInfo, true, "Spawned request to refresh routing tables.");
            }
        }

        /// <summary>
        /// Manually reloads system configuration.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void ReloadConfigRequstHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Manually reloads system configuration.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ReloadConfig [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (!LoadSystemConfiguration())
                {
                    SendResponse(requestInfo, false, "System configuration failed to reload.");
                }
                else
                {
                    m_iaonSession.AllAdapters.UpdateCollectionConfigurations();
                    SendResponse(requestInfo, true, "System configuration was successfully reloaded.");
                }

                // Spawn routing table calculation updates
                m_iaonSession.RecalculateRoutingTables();
            }
        }

        /// <summary>
        /// Updates an option in the configuration file.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void UpdateConfigFileRequestHandler(ClientRequestInfo requestInfo)
        {
            int orderedArgCount = requestInfo.Request.Arguments.OrderedArgCount;
            bool listSetting = requestInfo.Request.Arguments.Exists("list");
            bool deleteSetting = requestInfo.Request.Arguments.Exists("delete");
            bool addSetting = requestInfo.Request.Arguments.Exists("add");

            if (requestInfo.Request.Arguments.ContainsHelpRequest || (!listSetting && orderedArgCount < 2) || (!listSetting && !deleteSetting && orderedArgCount < 3))
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Updates an option in the configuration file.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       UpdateConfigFile [Category Name] -list");
                helpMessage.AppendLine();
                helpMessage.Append("       UpdateConfigFile \"Category Name\" \"Setting Name\" -delete");
                helpMessage.AppendLine();
                helpMessage.Append("       UpdateConfigFile \"Category Name\" \"Setting Name\" \"Setting Value\" [-add]");
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
                helpMessage.Append("       -list".PadRight(20));
                helpMessage.Append("Lists categories or settings under a specified category");
                helpMessage.AppendLine();
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                string categoryName = requestInfo.Request.Arguments["OrderedArg1"];
                string settingName = requestInfo.Request.Arguments["OrderedArg2"];
                string settingValue = requestInfo.Request.Arguments["OrderedArg3"];

                ConfigurationFile config = ConfigurationFile.Current;

                if (listSetting)
                {
                    if (orderedArgCount == 0)
                    {
                        StringBuilder categoryList = new StringBuilder();
                        categoryList.Append("List of categories in the configuration file:");
                        categoryList.AppendLine();
                        categoryList.AppendLine();

                        string xml = config.Settings.SectionInformation.GetRawXml();
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);

                        // List settings categories.
                        foreach (XmlNode node in xmlDoc.DocumentElement)
                        {
                            categoryList.Append("   ");
                            categoryList.Append(node.Name);
                            categoryList.AppendLine();
                        }

                        categoryList.AppendLine();
                        DisplayResponseMessage(requestInfo, categoryList.ToString());
                    }
                    else
                    {
                        CategorizedSettingsElementCollection settings = config.Settings[categoryName];
                        StringBuilder settingsList = new StringBuilder();
                        settingsList.Append(string.Format("List of settings under the category {0}:", categoryName));
                        settingsList.AppendLine();
                        settingsList.AppendLine();

                        // List settings under specified category.
                        foreach (CategorizedSettingsElement settingsElement in settings)
                        {
                            // Skip encrypted settings for security purpose.
                            if (settingsElement.Encrypted)
                                continue;

                            settingsList.Append("   Name:        ");
                            settingsList.Append(settingsElement.Name);
                            settingsList.AppendLine();
                            settingsList.Append("   Value:       ");
                            settingsList.Append(settingsElement.Value);
                            settingsList.AppendLine();
                            settingsList.Append("   Description: ");
                            settingsList.Append(settingsElement.Description);
                            settingsList.AppendLine();
                            settingsList.AppendLine();
                        }

                        settingsList.Replace("{", "{{{{");
                        settingsList.Replace("}", "}}}}");

                        DisplayResponseMessage(requestInfo, settingsList.ToString());
                    }
                }
                else
                {
                    CategorizedSettingsElementCollection settings = config.Settings[categoryName];
                    CategorizedSettingsElement setting = settings[settingName];

                    if (deleteSetting)
                    {
                        // Delete existing setting.
                        if (setting != null)
                        {
                            settings.Remove(setting);
                            config.Save();
                            SendResponse(requestInfo, true, "Successfully deleted setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                        }
                        else
                        {
                            SendResponse(requestInfo, false, "Failed to delete setting \"{0}\" under category \"{1}\". Setting does not exist.\r\n\r\n", settingName, categoryName);
                        }
                    }
                    else if (addSetting)
                    {
                        // Add new setting.
                        if (setting == null)
                        {
                            settings.Add(settingName, settingValue);
                            config.Save();
                            SendResponse(requestInfo, true, "Successfully added setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                        }
                        else
                        {
                            SendResponse(requestInfo, false, "Failed to add setting \"{0}\" under category \"{1}\". Setting already exists.\r\n\r\n", settingName, categoryName);
                        }
                    }
                    else
                    {
                        // Update existing setting.
                        if (setting != null)
                        {
                            setting.Value = settingValue;
                            config.Save();
                            SendResponse(requestInfo, true, "Successfully updated setting \"{0}\" under category \"{1}\".\r\n\r\n", settingName, categoryName);
                        }
                        else
                        {
                            SendResponse(requestInfo, false, "Failed to update value of setting \"{0}\" under category \"{1}\" . Setting does not exist.\r\n\r\n", settingName, categoryName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to authenticate or reauthenticate to network shares.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void AuthenticateRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Attempts to (re)authenticate to network shares.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Authenticate [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                DisplayStatusMessage("Attempting to reauthenticate network shares for health and status exports...", UpdateType.Information);

                try
                {
                    m_healthExporter.Initialize();
                    m_statusExporter.Initialize();
                    SendResponse(requestInfo, true);
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Failed to reauthenticate network shares: {0}", ex.Message);
                    m_serviceHelper.ErrorLogger.Log(ex);
                }
            }
        }

        /// <summary>
        /// Attempts to restart the hose service.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        protected virtual void RestartServiceHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Attempts to restart the host service.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       Restart [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (m_allowRemoteRestart)
                {
                    DisplayStatusMessage("Attempting to restart host service...", UpdateType.Information);

                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(ConsoleApplicationName)
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            Arguments = ServiceName + " -restart"
                        };

                        using (Process shell = new Process())
                        {
                            shell.StartInfo = psi;
                            shell.Start();

                            if (!shell.WaitForExit(30000))
                                shell.Kill();
                        }

                        SendResponse(requestInfo, true);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Failed to restart host service: {0}", ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
                else
                {
                    DisplayStatusMessage("Remote restart request denied, this is currently disallowed in the system configuration.", UpdateType.Warning);
                }
            }
        }

        #endregion

        #region [ Broadcast Message Handling ]

        /// <summary>
        /// Sends an actionable response to client.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success)
        {
            SendResponseWithAttachment(requestInfo, success, null, null);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success, string status, params object[] args)
        {
            SendResponseWithAttachment(requestInfo, success, null, status, args);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message and attachment.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="attachment">Attachment to send with response.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponseWithAttachment(ClientRequestInfo requestInfo, bool success, object attachment, string status, params object[] args)
        {
            try
            {
                // Send actionable response
                m_serviceHelper.SendActionableResponse(requestInfo, success, attachment, status, args);

                // Log details of client request as well as response
                if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
                {
                    string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
                    string arguments = requestInfo.Request.Arguments.ToString();
                    string message = responseType + (string.IsNullOrWhiteSpace(arguments) ? "" : "(" + arguments + ")");

                    if (status != null)
                    {
                        if (args.Length == 0)
                            message += " - " + status;
                        else
                            message += " - " + string.Format(status, args);
                    }

                    m_serviceHelper.StatusLog.WriteTimestampedLine(message);
                }
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to send client response due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a response message to client requestor.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            try
            {
                m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, string.Format("{0}\r\n\r\n", status), args);
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type)
        {
            try
            {
                status = status.Replace("{", "{{").Replace("}", "}}");
                m_serviceHelper.UpdateStatus(type, string.Format("{0}\r\n\r\n", status));
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Formatted status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type, params object[] args)
        {
            try
            {
                DisplayStatusMessage(string.Format(status, args), type);
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        #endregion

        #endregion
    }
}
