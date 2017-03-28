//******************************************************************************************************
//  ServiceHostBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using GSF.Collections;
using GSF.Communication;
using GSF.Configuration;
using GSF.Console;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Net.Security;
using GSF.Reflection;
using GSF.Security;
using GSF.ServiceProcess;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Configuration;
using GSF.TimeSeries.Reports;
using GSF.TimeSeries.Statistics;
using GSF.Units;

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
        /// Configuration source is a web-service.
        /// </summary>
        WebService,
        /// <summary>
        /// Configuration source is a binary file.
        /// </summary>
        BinaryFile,
        /// <summary>
        /// Configuration source is a XML file.
        /// </summary>
        XmlFile
    }

    #endregion

    /// <summary>
    /// Represents the time-series framework service host.
    /// </summary>
    public partial class ServiceHostBase : ServiceBase
    {
        #region [ Members ]

        // Constants
        private const int DefaultMinThreadPoolSize = 25;
        private const int DefaultMaxThreadPoolSize = 2048;
        private const int DefaultConfigurationBackups = 5;
        private const int DefaultMaxLogFiles = 300;

        internal event EventHandler<EventArgs<Guid, string, UpdateType>> UpdatedStatus;
        internal event EventHandler<EventArgs<Exception>> LoggedException;

        // Fields
        private IaonSession m_iaonSession;
        private string m_nodeIDQueryString;
        private ConfigurationType m_configurationType;
        private IConfigurationLoader m_configurationLoader;
        private BinaryFileConfigurationLoader m_binaryCacheConfigurationLoader;
        private XMLConfigurationLoader m_xmlCacheConfigurationLoader;
        private string m_cachedXmlConfigurationFile;
        private string m_cachedBinaryConfigurationFile;
        private int m_configurationBackups;
        private bool m_uniqueAdapterIDs;
        private bool m_allowRemoteRestart;
        private bool m_preferCachedConfiguration;
        private MultipleDestinationExporter m_healthExporter;
        private MultipleDestinationExporter m_statusExporter;
        private ReportingProcessCollection m_reportingProcesses;
        private ProcessQueue<Tuple<string, Action<bool>>> m_reloadConfigQueue;
        private LongSynchronizedOperation m_configurationCacheOperation;
        private volatile DataSet m_latestConfiguration;
        private RunTimeLog m_runTimeLog;

        private ServiceHelper m_serviceHelper;
        private ServerBase m_remotingServer;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ServiceHostBase"/>.
        /// </summary>
        public ServiceHostBase()
        {
            InitializeComponent();
            ServiceName = "IaonHost";
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
        protected ServerBase RemotingServer
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
            ShutdownHandler.Initialize();

            // Define a run-time log
            m_runTimeLog = new RunTimeLog();
            m_runTimeLog.FileName = "RunTimeLog.txt";
            m_runTimeLog.ProcessException += ProcessExceptionHandler;
            m_runTimeLog.Initialize();

            // Use this run-time log for system up-time calculations
            PerformanceStatistics.SystemRunTimeLog = m_runTimeLog;

            // Initialize Iaon session
            m_iaonSession = new IaonSession();
            m_iaonSession.StatusMessage += StatusMessageHandler;
            m_iaonSession.ProcessException += ProcessExceptionHandler;
            m_iaonSession.ConfigurationChanged += ConfigurationChangedHandler;

            // Create a handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Make sure default service settings exist
            ConfigurationFile configFile = ConfigurationFile.Current;

            string servicePath = FilePath.GetAbsolutePath("");
            string cachePath = string.Format("{0}{1}ConfigurationCache{1}", servicePath, Path.DirectorySeparatorChar);
            string defaultLogPath = string.Format("{0}{1}Logs{1}", servicePath, Path.DirectorySeparatorChar);

            // System settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("ConfigurationType", "Database", "Specifies type of configuration: Database, WebService, BinaryFile or XmlFile");
            systemSettings.Add("ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=IaonHost.mdb", "Configuration database connection string");
            systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Configuration database ADO.NET data provider assembly type creation string");
            systemSettings.Add("ConfigurationCachePath", cachePath, "Defines the path used to cache serialized configurations");
            systemSettings.Add("LogPath", defaultLogPath, "Defines the path used to archive log files");
            systemSettings.Add("MaxLogFiles", DefaultMaxLogFiles, "Defines the maximum number of log files to keep");
            systemSettings.Add("CachedConfigurationFile", "SystemConfiguration.xml", "File name for last known good system configuration (only cached for a Database or WebService connection)");
            systemSettings.Add("UniqueAdaptersIDs", "True", "Set to true if all runtime adapter ID's will be unique to allow for easier adapter specification");
            systemSettings.Add("ProcessPriority", "High", "Sets desired process priority: Normal, AboveNormal, High, RealTime");
            systemSettings.Add("AllowRemoteRestart", "True", "Controls ability to remotely restart the host service.");
            systemSettings.Add("MinThreadPoolWorkerThreads", DefaultMinThreadPoolSize, "Defines the minimum number of allowed thread pool worker threads.");
            systemSettings.Add("MaxThreadPoolWorkerThreads", DefaultMaxThreadPoolSize, "Defines the maximum number of allowed thread pool worker threads.");
            systemSettings.Add("MinThreadPoolIOPortThreads", DefaultMinThreadPoolSize, "Defines the minimum number of allowed thread pool I/O completion port threads (used by socket layer).");
            systemSettings.Add("MaxThreadPoolIOPortThreads", DefaultMaxThreadPoolSize, "Defines the maximum number of allowed thread pool I/O completion port threads (used by socket layer).");
            systemSettings.Add("ConfigurationBackups", DefaultConfigurationBackups, "Defines the total number of older backup configurations to maintain.");
            systemSettings.Add("PreferCachedConfiguration", "False", "Set to true to try the cached configuration first, before loading database configuration - typically used when cache is updated by external process.");
            systemSettings.Add("LocalCertificate", $"{ServiceName}.cer", "Path to the local certificate used by this server for authentication.");
            systemSettings.Add("RemoteCertificatesPath", @"Certs\Remotes", "Path to the directory where remote certificates are stored.");
            systemSettings.Add("DefaultCulture", "en-US", "Default culture to use for language, country/region and calendar formats.");

            // Example connection settings
            CategorizedSettingsElementCollection exampleSettings = configFile.Settings["exampleConnectionSettings"];
            exampleSettings.Add("SqlServer.ConnectionString", "Data Source=serverName; Initial Catalog=databaseName; User ID=userName; Password=password", "Example SQL Server database connection string");
            exampleSettings.Add("SqlServer.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter", "Example SQL Server database .NET provider string");
            exampleSettings.Add("MySQL.ConnectionString", "Server=serverName;Database=databaseName; Uid=root; Pwd=password; allow user variables = true;", "Example MySQL database connection string");
            exampleSettings.Add("MySQL.DataProviderString", "AssemblyName={MySql.Data, Version=6.3.6.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter", "Example MySQL database .NET provider string");
            exampleSettings.Add("Oracle.ConnectionString", "Data Source=tnsName; User ID=schemaUserName; Password=schemaPassword", "Example Oracle database connection string");
            exampleSettings.Add("Oracle.DataProviderString", "AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter", "Example Oracle database .NET provider string");
            exampleSettings.Add("SQLite.ConnectionString", "Data Source=databaseName.db; Version=3; Foreign Keys=True; FailIfMissing=True", "Example SQLite database connection string");
            exampleSettings.Add("SQLite.DataProviderString", "AssemblyName={System.Data.SQLite, Version=1.0.99.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter", "Example SQLite database .NET provider string");
            exampleSettings.Add("OleDB.ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=databaseName.mdb", "Example Microsoft Access (via OleDb) database connection string");
            exampleSettings.Add("OleDB.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.OleDb.OleDbConnection; AdapterType=System.Data.OleDb.OleDbDataAdapter", "Example OleDb database .NET provider string");
            exampleSettings.Add("Odbc.ConnectionString", "Driver={SQL Server Native Client 10.0}; Server=serverName; Database=databaseName; Uid=userName; Pwd=password;", "Example ODBC database connection string");
            exampleSettings.Add("Odbc.DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter", "Example ODBC database .NET provider string");
            exampleSettings.Add("WebService.ConnectionString", "http://localhost/ConfigSource/SystemConfiguration.xml", "Example web service connection string");
            exampleSettings.Add("XmlFile.ConnectionString", "SystemConfiguration.xml", "Example XML configuration file connection string");

            // Attempt to set default culture
            try
            {
                string defaultCulture = systemSettings["DefaultCulture"].ValueAs("en-US");
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture(defaultCulture);     // Defaults for date formatting, etc.
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture(defaultCulture);   // Culture for resource strings, etc.
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to set default culture due to exception, defaulting to \"{1}\": {0}", UpdateType.Alarm, ex.Message, CultureInfo.CurrentCulture.Name.ToNonNullNorEmptyString("Undetermined"));
                LogException(ex);
            }

            // Retrieve application log path as defined in the config file
            string logPath = FilePath.GetAbsolutePath(systemSettings["LogPath"].Value);

            // Make sure log directory exists
            try
            {
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
            }
            catch (Exception ex)
            {
                // Attempt to default back to common log file path
                if (!Directory.Exists(defaultLogPath))
                {
                    try
                    {
                        Directory.CreateDirectory(defaultLogPath);
                    }
                    catch
                    {
                        defaultLogPath = servicePath;
                    }
                }

                DisplayStatusMessage("Failed to create logging directory \"{0}\" due to exception, defaulting to \"{1}\": {2}", UpdateType.Alarm, logPath, defaultLogPath, ex.Message);
                LogException(ex);
                logPath = defaultLogPath;
            }

            int maxLogFiles = systemSettings["MaxLogFiles"].ValueAs(DefaultMaxLogFiles);

            try
            {
                Logger.FileWriter.SetPath(logPath);
                Logger.FileWriter.SetLoggingFileCount(maxLogFiles);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to set logging path \"{0}\" or max file count \"{1}\" due to exception: {2}", UpdateType.Alarm, logPath, maxLogFiles, ex.Message);
                LogException(ex);
            }

            // Retrieve configuration cache directory as defined in the config file
            cachePath = FilePath.GetAbsolutePath(systemSettings["ConfigurationCachePath"].Value);

            // Make sure configuration cache directory exists
            try
            {
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to create configuration cache directory \"{0}\" due to exception: {1}", UpdateType.Alarm, cachePath, ex.Message);
                LogException(ex);
            }

            try
            {
                Directory.SetCurrentDirectory(servicePath);
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to set current directory to execution path \"{0}\" due to exception: {1}", UpdateType.Alarm, servicePath, ex.Message);
                LogException(ex);
            }

            // Initialize system settings
            m_configurationType = systemSettings["ConfigurationType"].ValueAs<ConfigurationType>();
            m_cachedXmlConfigurationFile = FilePath.AddPathSuffix(cachePath) + systemSettings["CachedConfigurationFile"].Value;
            m_cachedBinaryConfigurationFile = FilePath.AddPathSuffix(cachePath) + FilePath.GetFileNameWithoutExtension(m_cachedXmlConfigurationFile) + ".bin";
            m_configurationBackups = systemSettings["ConfigurationBackups"].ValueAs(DefaultConfigurationBackups);
            m_uniqueAdapterIDs = systemSettings["UniqueAdaptersIDs"].ValueAsBoolean(true);
            m_allowRemoteRestart = systemSettings["AllowRemoteRestart"].ValueAsBoolean(true);
            m_preferCachedConfiguration = systemSettings["PreferCachedConfiguration"].ValueAsBoolean(false);

            m_reloadConfigQueue = ProcessQueue<Tuple<string, Action<bool>>>.CreateSynchronousQueue(ExecuteReloadConfig, 500.0D, Timeout.Infinite, false, false);
            m_reloadConfigQueue.ProcessException += m_iaonSession.ProcessExceptionHandler;

            m_configurationCacheOperation = new LongSynchronizedOperation(ExecuteConfigurationCache)
            {
                IsBackground = true
            };

            // Setup default thread pool size
            try
            {
                ThreadPool.SetMinThreads(systemSettings["MinThreadPoolWorkerThreads"].ValueAs(DefaultMinThreadPoolSize), systemSettings["MinThreadPoolIOPortThreads"].ValueAs(DefaultMinThreadPoolSize));
                ThreadPool.SetMaxThreads(systemSettings["MaxThreadPoolWorkerThreads"].ValueAs(DefaultMaxThreadPoolSize), systemSettings["MaxThreadPoolIOPortThreads"].ValueAs(DefaultMaxThreadPoolSize));
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to set desired thread pool size due to exception: {0}", UpdateType.Alarm, ex.Message);
                LogException(ex);
            }

            // Define guid with query string delimiters according to database needs
            if (string.IsNullOrWhiteSpace(m_nodeIDQueryString))
                m_nodeIDQueryString = "'" + m_iaonSession.NodeID + "'";

            // Set up the configuration loader
            switch (m_configurationType)
            {
                case ConfigurationType.Database:
                    m_configurationLoader = new DatabaseConfigurationLoader
                    {
                        ConnectionString = systemSettings["ConnectionString"].Value,
                        DataProviderString = systemSettings["DataProviderString"].Value,
                        NodeIDQueryString = m_nodeIDQueryString
                    };

                    break;

                case ConfigurationType.WebService:
                    m_configurationLoader = new WebServiceConfigurationLoader
                    {
                        URI = systemSettings["ConnectionString"].Value
                    };

                    break;

                case ConfigurationType.BinaryFile:
                    m_configurationLoader = new BinaryFileConfigurationLoader
                    {
                        FilePath = systemSettings["ConnectionString"].Value
                    };

                    break;

                case ConfigurationType.XmlFile:
                    m_configurationLoader = new XMLConfigurationLoader
                    {
                        FilePath = systemSettings["ConnectionString"].Value
                    };

                    break;
            }

            m_binaryCacheConfigurationLoader = new BinaryFileConfigurationLoader
            {
                FilePath = m_cachedBinaryConfigurationFile
            };

            m_xmlCacheConfigurationLoader = new XMLConfigurationLoader
            {
                FilePath = m_cachedXmlConfigurationFile
            };

            m_configurationLoader.StatusMessage += (o, args) => DisplayStatusMessage(args.Argument, UpdateType.Information);
            m_binaryCacheConfigurationLoader.StatusMessage += (o, args) => DisplayStatusMessage(args.Argument, UpdateType.Information);
            m_xmlCacheConfigurationLoader.StatusMessage += (o, args) => DisplayStatusMessage(args.Argument, UpdateType.Information);

            m_configurationLoader.ProcessException += ConfigurationLoader_ProcessException;
            m_binaryCacheConfigurationLoader.ProcessException += ConfigurationLoader_ProcessException;
            m_xmlCacheConfigurationLoader.ProcessException += ConfigurationLoader_ProcessException;

            m_reloadConfigQueue.Start();

#if !MONO
            try
            {
                // Attempt to assign desired process priority. Note that process will require SeIncreaseBasePriorityPrivilege or 
                // Administrative privileges to make this change
                Process.GetCurrentProcess().PriorityClass = systemSettings["ProcessPriority"].ValueAs<ProcessPriorityClass>();
            }
            catch (Exception ex)
            {
                LogException(ex);
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

            // Get current process memory usage
            long processMemory = GSF.Common.GetProcessMemory();

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
                GSF.Common.GetOSProductName(),
                processMemory > 0 ? SI2.ToScaledString(processMemory, 4, "B", SI2.IECSymbols) : "Undetermined",
                IntPtr.Size * 8,
                Environment.ProcessorCount,
                GCSettings.IsServerGC,
                GCSettings.LatencyMode,
                Environment.UserDomainName,
                Environment.UserName,
                stars);

            // Add run-time log as a service component
            m_serviceHelper.ServiceComponents.Add(m_runTimeLog);

            // Create health exporter
            m_healthExporter = new MultipleDestinationExporter("HealthExporter", Timeout.Infinite);
            m_healthExporter.Initialize(new[] { new ExportDestination(FilePath.GetAbsolutePath("Health.txt"), false) });
            m_healthExporter.StatusMessage += m_iaonSession.StatusMessageHandler;
            m_healthExporter.ProcessException += m_iaonSession.ProcessExceptionHandler;
            m_serviceHelper.ServiceComponents.Add(m_healthExporter);

            // Create status exporter
            m_statusExporter = new MultipleDestinationExporter("StatusExporter", Timeout.Infinite);
            m_statusExporter.Initialize(new[] { new ExportDestination(FilePath.GetAbsolutePath("Status.txt"), false) });
            m_statusExporter.StatusMessage += m_iaonSession.StatusMessageHandler;
            m_statusExporter.ProcessException += m_iaonSession.ProcessExceptionHandler;
            m_serviceHelper.ServiceComponents.Add(m_statusExporter);

            // Create reporting processes
            m_reportingProcesses = ReportingProcessCollection.LoadImplementations(ex =>
            {
                DisplayStatusMessage("Failed to load reporting process: {0}", UpdateType.Warning, ex.Message);
                LogException(ex);
            });
            m_serviceHelper.ServiceComponents.Add(m_reportingProcesses);

            // Define scheduled service processes
            m_serviceHelper.AddScheduledProcess(HealthMonitorProcessHandler, "HealthMonitor", "* * * * *");    // Every minute
            m_serviceHelper.AddScheduledProcess(StatusExportProcessHandler, "StatusExport", "*/30 * * * *");   // Every 30 minutes

            // Define processes for each report (initially unscheduled)
            foreach (IReportingProcess reportingProcess in m_reportingProcesses)
                m_serviceHelper.AddProcess(ReportingProcessHandler, $"{reportingProcess.ReportType}Reporting");

            // Add key Iaon collections as service components
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.InputAdapters);
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.ActionAdapters);
            m_serviceHelper.ServiceComponents.Add(m_iaonSession.OutputAdapters);

            // Define remote client requests (i.e., console commands)
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("List", "Displays status for specified adapter or collection", ListRequestHandler, new[] { "ls", "dir" }));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Connect", "Connects (or starts) specified adapter", StartRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Disconnect", "Disconnects (or stops) specified adapter", StopRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Invoke", "Invokes a command for specified adapter", InvokeRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ListCommands", "Displays possible commands for specified adapter", ListCommandsRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Initialize", "Initializes specified adapter or collection", InitializeRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ReloadConfig", "Manually reloads the system configuration", ReloadConfigRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("UpdateConfigFile", "Updates an option in the configuration file", UpdateConfigFileRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Authenticate", "Authenticates network shares for health and status exports", AuthenticateRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("Restart", "Attempts to restart the host service", RestartServiceHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("RefreshRoutes", "Spawns request to recalculate routing tables", RefreshRoutesRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("TemporalSupport", "Determines if any adapters support temporal processing", TemporalSupportRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ListReports", "Lists the available reports that have been generated", ListReportsRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("GetReport", "Gets a previously generated report for the specified date", GetReportRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("GetReportStatus", "Gets a reporting process status", GetReportStatusRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("GenerateReport", "Generates a report for the specified date", GenerateReportRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ReportingConfig", "Displays or modifies the configuration of the reporting process", ReportingConfigRequestHandler, false));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("LogEvent", "Logs remote event log entries.", LogEventRequestHandler, false));

            // Start system initialization on an independent thread so that service responds in a timely fashion...
            InitializeSystem();
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
            // Dispose system health exporter
            if ((object)m_healthExporter != null)
            {
                m_healthExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_healthExporter);
                m_healthExporter.Dispose();
                m_healthExporter.StatusMessage -= m_iaonSession.StatusMessageHandler;
                m_healthExporter.ProcessException -= m_iaonSession.ProcessExceptionHandler;
                m_healthExporter = null;
            }

            // Dispose system status exporter
            if ((object)m_statusExporter != null)
            {
                m_statusExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_statusExporter);
                m_statusExporter.Dispose();
                m_statusExporter.StatusMessage -= m_iaonSession.StatusMessageHandler;
                m_statusExporter.ProcessException -= m_iaonSession.ProcessExceptionHandler;
                m_statusExporter = null;
            }

            // Dispose reload config queue
            if ((object)m_reloadConfigQueue != null)
            {
                m_reloadConfigQueue.ProcessException -= m_iaonSession.ProcessExceptionHandler;
                m_reloadConfigQueue.Dispose();
                m_reloadConfigQueue = null;
            }

            // Dispose reporting processes
            if ((object)m_reportingProcesses != null)
            {
                m_serviceHelper.ServiceComponents.Remove(m_reportingProcesses);
                m_reportingProcesses = null;
            }

            // Dispose Iaon session
            if ((object)m_iaonSession != null)
            {
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.InputAdapters);
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.ActionAdapters);
                m_serviceHelper.ServiceComponents.Remove(m_iaonSession.OutputAdapters);

                m_iaonSession.Dispose();
                m_iaonSession.StatusMessage -= StatusMessageHandler;
                m_iaonSession.ProcessException -= ProcessExceptionHandler;
                m_iaonSession.ConfigurationChanged -= ConfigurationChangedHandler;
                m_iaonSession = null;
            }

            // Dispose of run-time log
            if ((object)m_runTimeLog != null)
            {
                m_serviceHelper.ServiceComponents.Remove(m_runTimeLog);
                m_runTimeLog.ProcessException -= ProcessExceptionHandler;
                m_runTimeLog.Dispose();
                m_runTimeLog = null;
            }

            m_serviceHelper.ServiceStarting -= ServiceStartingHandler;
            m_serviceHelper.ServiceStarted -= ServiceStartedHandler;
            m_serviceHelper.ServiceStopping -= ServiceStoppingHandler;
            m_serviceHelper.UpdatedStatus -= UpdatedStatusHandler;
            m_serviceHelper.LoggedException -= LoggedExceptionHandler;

            if ((object)m_serviceHelper.StatusLog != null)
            {
                m_serviceHelper.StatusLog.Flush();
                m_serviceHelper.StatusLog.LogException -= LogExceptionHandler;
            }

            if ((object)m_serviceHelper.ErrorLogger != null && (object)m_serviceHelper.ErrorLogger.ErrorLog != null)
            {
                m_serviceHelper.ErrorLogger.ErrorLog.Flush();
                m_serviceHelper.ErrorLogger.ErrorLog.LogException -= LogExceptionHandler;
            }

            // Detach from handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

            ShutdownHandler.InitiateSafeShutdown();
        }

        private void UpdatedStatusHandler(object sender, EventArgs<Guid, string, UpdateType> e)
        {
            if ((object)UpdatedStatus != null)
                UpdatedStatus(sender, new EventArgs<Guid, string, UpdateType>(e.Argument1, e.Argument2, e.Argument3));
        }

        private void LoggedExceptionHandler(object sender, EventArgs<Exception> e)
        {
            if ((object)LoggedException != null)
                LoggedException(sender, new EventArgs<Exception>(e.Argument));
        }

        #endregion

        #region [ System Initialization ]

        private void GenerateLocalCertificate()
        {
            ConfigurationFile configurationFile;
            CategorizedSettingsElementCollection remotingServer;

            string certificatePath;
            CertificateGenerator certificateGenerator = null;
            X509Certificate2 certificate = null;

            if (string.IsNullOrWhiteSpace(ServiceName))
                throw new InvalidOperationException("EstablishServiceProperties must be overridden and ServiceName must be set");

            try
            {
                configurationFile = ConfigurationFile.Current;
                remotingServer = configurationFile.Settings["remotingServer"];

                remotingServer.Add("CertificateFile", $"{ServiceName}.cer", "Path to the local certificate used by this server for authentication.");
                certificatePath = FilePath.GetAbsolutePath(remotingServer["CertificateFile"].Value);

                certificateGenerator = new CertificateGenerator()
                {
                    Issuer = ServiceName,
                    CertificatePath = certificatePath
                };

                if (File.Exists(certificatePath))
                    certificate = new X509Certificate2(certificatePath);

                if (!Equals(certificate, certificateGenerator.GenerateCertificate()))
                    EventLog.WriteEntry(ServiceName, $"Created self-signed certificate for service: \"{certificatePath}\"", EventLogEntryType.Information, 0);
            }
            catch (Exception ex)
            {
                string message = string.Join(Environment.NewLine, new[]
                {
                    ex.ToString(),
                    string.Empty,
                    "Debug info:",
                    string.Join(Environment.NewLine, certificateGenerator?.DebugLog)
                });

                EventLog.WriteEntry(ServiceName, message, EventLogEntryType.Error, 0);
            }
        }

        /// <summary>
        /// Initializes the service helper.
        /// </summary>
        public void InitializeServiceHelper()
        {
            CategorizedSettingsElementCollection remotingServerSettings = ConfigurationFile.Current.Settings["remotingServer"];

            m_serviceHelper = new ServiceHelper();

            if (remotingServerSettings.Cast<CategorizedSettingsElement>().Any(element => element.Name.Equals("EnabledSslProtocols", StringComparison.OrdinalIgnoreCase) && !element.Value.Equals("None", StringComparison.OrdinalIgnoreCase)))
                m_remotingServer = InitializeTlsServer();
            else
                m_remotingServer = InitializeTcpServer();

            m_serviceHelper.ErrorLogger.ErrorLog.FileName = "ErrorLog.txt";
            m_serviceHelper.ErrorLogger.ErrorLog.PersistSettings = true;
            m_serviceHelper.ErrorLogger.ErrorLog.SettingsCategory = "ErrorLog";
            m_serviceHelper.ErrorLogger.ErrorLog.Initialize();

            m_serviceHelper.ErrorLogger.LogToEventLog = false;
            m_serviceHelper.ErrorLogger.PersistSettings = true;
            m_serviceHelper.ErrorLogger.SmtpServer = "";
            m_serviceHelper.ErrorLogger.Initialize();

            m_serviceHelper.ProcessScheduler.PersistSettings = true;
            m_serviceHelper.ProcessScheduler.SettingsCategory = "ProcessScheduler";
            m_serviceHelper.ProcessScheduler.Initialize();

            m_serviceHelper.StatusLog.FileName = "StatusLog.txt";
            m_serviceHelper.StatusLog.PersistSettings = true;
            m_serviceHelper.StatusLog.SettingsCategory = "StatusLog";
            m_serviceHelper.StatusLog.Initialize();

            m_serviceHelper.ParentService = this;
            m_serviceHelper.PersistSettings = true;
            m_serviceHelper.RemotingServer = m_remotingServer;
            m_serviceHelper.Initialize();
        }

        private TcpServer InitializeTcpServer()
        {
            TcpServer remotingServer;

            remotingServer = new TcpServer();
            remotingServer.ConfigurationString = "Port=8500";
            remotingServer.IgnoreInvalidCredentials = true;
            remotingServer.PayloadAware = true;
            remotingServer.PersistSettings = true;
            remotingServer.SettingsCategory = "RemotingServer";
            remotingServer.Initialize();

            return remotingServer;
        }

        private TlsServer InitializeTlsServer()
        {
            TlsServer remotingServer;

            remotingServer = new TlsServer();
            remotingServer.CertificateFile = "Internal.cer";
            remotingServer.ConfigurationString = "Port=8500";
            remotingServer.IgnoreInvalidCredentials = true;
            remotingServer.PayloadAware = true;
            remotingServer.PersistSettings = true;
            remotingServer.SettingsCategory = "RemotingServer";
            remotingServer.TrustedCertificatesPath = $"Certs{Path.DirectorySeparatorChar}Remotes";
            remotingServer.Initialize();

            remotingServer.RemoteCertificateValidationCallback = (o, certificate, chain, errors) => true;

            return remotingServer;
        }

        // Perform system initialization
        private void InitializeSystem()
        {
            m_reloadConfigQueue.Add(Tuple.Create("Startup", new Action<bool>(success =>
            {
                // Attempt to load system configuration
                if (success)
                {
                    // Initialize and start all session adapters
                    m_iaonSession.Initialize();

                    DisplayStatusMessage("System initialization complete.", UpdateType.Information);

                    // If any settings have been added to configuration file, we go ahead and save them now
                    m_serviceHelper.SaveSettings(true);
                    ConfigurationFile.Current.Save();
                }
                else
                {
                    DisplayStatusMessage("System initialization failed due to unavailable configuration.", UpdateType.Alarm);
                }

                // Log current thread pool size
                int minWorkerThreads, minIOThreads, maxWorkerThreads, maxIOThreads;

                ThreadPool.GetMinThreads(out minWorkerThreads, out minIOThreads);
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxIOThreads);

                DisplayStatusMessage("Thread pool size: minimum {0} worker {1} I/O, maximum {2} worker {3} I/O", UpdateType.Information, minWorkerThreads, minIOThreads, maxWorkerThreads, maxIOThreads);
            })));
        }

        private void ExecuteReloadConfig(Tuple<string, Action<bool>>[] items)
        {
            List<string> requestTypes;
            List<string> distinctRequestsTypes;

            DataSet configuration = null;
            bool systemConfigurationLoaded = false;

            requestTypes = items
                .Select(tuple => tuple.Item1)
                .ToList();

            distinctRequestsTypes = requestTypes
                .Distinct()
                .OrderBy(type => requestTypes.LastIndexOf(type))
                .ToList();

            foreach (string type in distinctRequestsTypes)
            {
                string typeInner = type;

                if (type == m_configurationType.ToString())
                {
                    DisplayStatusMessage("Loading system configuration...", UpdateType.Information);
                    configuration = GetConfigurationDataSet();

                    // Update data source on all adapters in all collections
                    if ((object)configuration != null)
                        PropagateDataSource(configuration);
                }
                else if (type == "Augmented")
                {
                    DisplayStatusMessage("Augmenting current system configuration...", UpdateType.Information);
                    configuration = AugmentConfigurationDataSet(DataSource);

                    // Update data source on all adapters in all collections
                    if ((object)configuration != null)
                        PropagateDataSource(configuration);
                }
                else if (type == "BinaryCache")
                {
                    DisplayStatusMessage("Loading binary cached configuration...", UpdateType.Information);
                    configuration = GetBinaryCachedConfigurationDataSet();

                    // Update data source on all adapters in all collections
                    if ((object)configuration != null)
                        PropagateDataSource(configuration);
                }
                else if (type == "XmlCache")
                {
                    DisplayStatusMessage("Loading XML cached configuration...", UpdateType.Information);
                    configuration = GetXMLCachedConfigurationDataSet();

                    // Update data source on all adapters in all collections
                    if ((object)configuration != null)
                        PropagateDataSource(configuration);
                }
                else if (type == "Startup")
                {
                    systemConfigurationLoaded = LoadStartupConfiguration();
                }
                else
                {
                    // No specific reload command was issued;
                    // load system configuration as normal
                    systemConfigurationLoaded = LoadSystemConfiguration();
                }

                foreach (Action<bool> callback in items.Where(tuple => tuple.Item1 == typeInner).Select(tuple => tuple.Item2))
                {
                    try
                    {
                        callback(systemConfigurationLoaded || (object)configuration != null);
                    }
                    catch (Exception ex)
                    {
                        DisplayStatusMessage("Failed to execute callback for ReloadConfig request of type {0}: {1}", UpdateType.Alarm, type, ex.Message);
                        LogException(ex);
                    }
                }

                // Spawn routing table calculation updates
                m_iaonSession.RecalculateRoutingTables();
            }
        }

        // Load the system configuration data set
        private bool LoadStartupConfiguration()
        {
            DataSet configuration;
            bool loadedFromCache;

            // If cached configuration is not preferred,
            // load system configuration normally
            if (!m_preferCachedConfiguration)
                return LoadSystemConfiguration();

            // Cached configuration is preferred, so start by loading cached configuration
            loadedFromCache = true;
            configuration = GetBinaryCachedConfigurationDataSet();

            if ((object)configuration == null)
                configuration = GetXMLCachedConfigurationDataSet();

            // If cached configuration failed to load,
            // load from regular configuration data source after all
            if ((object)configuration == null)
            {
                loadedFromCache = false;
                configuration = GetConfigurationDataSet();
            }

            if ((object)configuration != null)
            {
                // Update data source on all adapters in all collections
                if (PropagateDataSource(configuration))
                {
                    // Cache the configuration if it wasn't already loaded from a cache
                    if (!loadedFromCache)
                        CacheCurrentConfiguration(configuration);

                    return true;
                }
            }

            return false;
        }

        // Load the system configuration data set
        private bool LoadSystemConfiguration()
        {
            DatabaseConfigurationLoader dbConfigurationLoader = m_configurationLoader as DatabaseConfigurationLoader;

            DataSet configuration = null;
            DataSet binaryCache = null;
            DataSet xmlCache = null;

            bool binaryCacheLoadAttempted = false;
            bool xmlCacheLoadAttempted = false;
            bool loadedFromCache = false;

            try
            {
                // If the configuration is a database configuration, make sure
                // to open the database before we start so we're not constantly
                // opening and closing connections to the database
                if ((object)dbConfigurationLoader != null)
                    dbConfigurationLoader.Open();

                if (m_configurationLoader.CanAugment)
                {
                    // Get the current running configuration
                    configuration = DataSource;

                    if ((object)configuration != null)
                    {
                        // Existing configuration is available so we attempt to augment that
                        DisplayStatusMessage("Attempting to augment existing configuration data set...", UpdateType.Information);
                        configuration = AugmentConfigurationDataSet(configuration.Copy());
                    }
                    else
                    {
                        // Attempt to load the binary cached configuration
                        binaryCache = GetBinaryCachedConfigurationDataSet();
                        binaryCacheLoadAttempted = true;

                        if ((object)binaryCache != null)
                        {
                            // Binary cached configuration is available so we attempt to augment that
                            DisplayStatusMessage("Attempting to augment binary cached configuration data set...", UpdateType.Information);
                            configuration = AugmentConfigurationDataSet(binaryCache.Copy());
                        }
                        else
                        {
                            // Attempt to load the XML cached configuration
                            xmlCache = GetXMLCachedConfigurationDataSet();
                            xmlCacheLoadAttempted = true;

                            if ((object)xmlCache != null)
                            {
                                // XML cached configuration is available so we attempt to augment that
                                DisplayStatusMessage("Attempting to augment XML cached configuration data set...", UpdateType.Information);
                                configuration = AugmentConfigurationDataSet(xmlCache.Copy());
                            }
                        }
                    }
                }

                if ((object)configuration == null)
                {
                    // Either we cannot augment configuration or augmentation failed,
                    // so we attempt to load the whole configuration from the source
                    configuration = GetConfigurationDataSet();
                }

                if ((object)configuration == null)
                {
                    // If all else fails, load configuration from the binary cache
                    loadedFromCache = true;

                    if (binaryCacheLoadAttempted)
                        configuration = binaryCache;
                    else
                        configuration = GetBinaryCachedConfigurationDataSet();
                }

                if ((object)configuration == null)
                {
                    // If we even failed to load from the
                    // binary cache, load from the XML cache
                    if (xmlCacheLoadAttempted)
                        configuration = xmlCache;
                    else
                        configuration = GetXMLCachedConfigurationDataSet();
                }

                if ((object)configuration != null)
                {
                    // Update data source on all adapters in all collections
                    if (PropagateDataSource(configuration))
                    {
                        // Cache the configuration if it wasn't already loaded from a cache
                        if (!loadedFromCache)
                            CacheCurrentConfiguration(configuration);

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                DisplayStatusMessage(ex.Message, UpdateType.Alarm);
                LogException(ex);

                // Most of the methods used above have a try-catch implemented to display helpful error messages
                // and prevent this code from executing. This should only execute when using a database configuration
                // as the primary configuration source and only if the system is unable to open a connection to the
                // database. In that case, we simply attempt to load the cached configuration and use that instead
                DisplayStatusMessage("Failed to open a connection to the database. Falling back on cached configuration...", UpdateType.Warning);

                if ((object)configuration == null)
                    configuration = GetBinaryCachedConfigurationDataSet();

                if ((object)configuration == null)
                    configuration = GetXMLCachedConfigurationDataSet();

                if ((object)configuration != null)
                    return PropagateDataSource(configuration);

                return false;
            }
            finally
            {
                if ((object)dbConfigurationLoader != null)
                    dbConfigurationLoader.Close();
            }
        }

        private DataSet GetConfigurationDataSet()
        {
            try
            {
                return m_configurationLoader.Load();
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to load {0} configuration due to exception: {1}", UpdateType.Warning, m_configurationType, ex.Message);
                LogException(ex);
                return null;
            }
        }

        private DataSet GetBinaryCachedConfigurationDataSet()
        {
            try
            {
                if (File.Exists(m_binaryCacheConfigurationLoader.FilePath))
                    return m_binaryCacheConfigurationLoader.Load();

                return null;
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to load cached binary configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                LogException(ex);
                return null;
            }
        }

        private DataSet GetXMLCachedConfigurationDataSet()
        {
            try
            {
                if (File.Exists(m_xmlCacheConfigurationLoader.FilePath))
                    return m_xmlCacheConfigurationLoader.Load();

                return null;
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to load cached XML configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                LogException(ex);
                return null;
            }
        }

        private DataSet AugmentConfigurationDataSet(DataSet configuration)
        {
            try
            {
                m_configurationLoader.Augment(configuration);
                return configuration;
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to augment configuration due to exception: {0}", UpdateType.Warning, ex.Message);
                LogException(ex);
                return null;
            }
        }

        private bool PropagateDataSource(DataSet dataSource)
        {
            try
            {
                m_iaonSession.DataSource = dataSource;
                return true;
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Unable to propagate configuration data set to adapters due to exception: {0}", UpdateType.Alarm, ex.Message);
                LogException(ex);
                return false;
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
            m_latestConfiguration = configuration;
            m_configurationCacheOperation.RunOnceAsync();
        }

        // Executes actual serialization of current configuration
        private void ExecuteConfigurationCache()
        {
            DataSet configuration = m_latestConfiguration;

            if ((object)configuration != null)
            {
                // Create backups of binary configurations
                BackupConfiguration(ConfigurationType.BinaryFile, m_cachedBinaryConfigurationFile);

                // Create backups of XML configurations
                BackupConfiguration(ConfigurationType.XmlFile, m_cachedXmlConfigurationFile);

                try
                {
                    // Wait a moment for write lock in case binary file is open by another process
                    if (File.Exists(m_cachedBinaryConfigurationFile))
                        FilePath.WaitForWriteLock(m_cachedBinaryConfigurationFile);

                    // Cache binary serialized version of data set
                    using (FileStream configurationFileStream = File.OpenWrite(m_cachedBinaryConfigurationFile))
                    {
                        configuration.SerializeToStream(configurationFileStream);
                    }

                    DisplayStatusMessage("Successfully cached current configuration to binary.", UpdateType.Information);
                }
                catch (Exception ex)
                {
                    DisplayStatusMessage("Failed to cache last known configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                    LogException(ex);
                }

                // Serialize current data set to configuration files
                try
                {

                    // Wait a moment for write lock in case XML file is open by another process
                    if (File.Exists(m_cachedXmlConfigurationFile))
                        FilePath.WaitForWriteLock(m_cachedXmlConfigurationFile);

                    // Cache XML serialized version of data set
                    configuration.WriteXml(m_cachedXmlConfigurationFile, XmlWriteMode.WriteSchema);

                    DisplayStatusMessage("Successfully cached current configuration to XML.", UpdateType.Information);
                }
                catch (Exception ex)
                {
                    DisplayStatusMessage("Failed to cache last known configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                    LogException(ex);
                }
            }
        }

        private void BackupConfiguration(ConfigurationType configType, string configurationFile)
        {
            try
            {
                // Create multiple backup configurations, if requested
                for (int i = m_configurationBackups; i > 0; i--)
                {
                    string origConfigFile = configurationFile + ".backup" + (i == 1 ? "" : (i - 1).ToString());

                    if (File.Exists(origConfigFile))
                    {
                        string nextConfigFile = configurationFile + ".backup" + i;

                        if (File.Exists(nextConfigFile))
                            File.Delete(nextConfigFile);

                        File.Move(origConfigFile, nextConfigFile);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to create extra backup {0} configurations due to exception: {1}", UpdateType.Warning, configType, ex.Message);
                LogException(ex);
            }

            try
            {
                if (m_configurationBackups > 0)
                {
                    // Back up current configuration file, if any
                    if (File.Exists(configurationFile))
                    {
                        string backupConfigFile = configurationFile + ".backup";

                        if (File.Exists(backupConfigFile))
                            File.Delete(backupConfigFile);

                        File.Move(configurationFile, backupConfigFile);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to backup last known cached {0} configuration due to exception: {1}", UpdateType.Warning, configType, ex.Message);
                LogException(ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ServiceHostBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)components != null)
                        {
                            components.Dispose();
                            components = null;
                        }

                        if ((object)m_serviceHelper != null)
                        {
                            m_serviceHelper.Dispose();
                            m_serviceHelper = null;
                        }

                        if ((object)m_remotingServer != null)
                        {
                            m_remotingServer.Dispose();
                            m_remotingServer = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        #endregion

        #region [ Service Binding ]

        internal void StartHostedService()
        {
            OnStart(Environment.CommandLine.Split(' '));
        }

        internal void StopHostedService()
        {
            OnStop();
        }

        internal void SendRequest(Guid clientID, string userInput)
        {
            ClientRequest request = ClientRequest.Parse(userInput);

            if ((object)request != null)
            {
                ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(request.Command);

                if ((object)requestHandler != null)
                    requestHandler.HandlerMethod(new ClientRequestInfo(new ClientInfo() { ClientID = clientID }, request));
                else
                    DisplayStatusMessage($"Command \"{request.Command}\" is not supported\r\n\r\n", UpdateType.Alarm);
            }
        }

        /// <summary>
        /// Handles service start event.
        /// </summary>
        /// <param name="args">Service startup arguments, if any.</param>
        protected override void OnStart(string[] args)
        {
            GenerateLocalCertificate();
            InitializeServiceHelper();

            // Register service level event handlers
            m_serviceHelper.ServiceStarting += ServiceStartingHandler;
            m_serviceHelper.ServiceStarted += ServiceStartedHandler;
            m_serviceHelper.ServiceStopping += ServiceStoppingHandler;
            m_serviceHelper.UpdatedStatus += UpdatedStatusHandler;
            m_serviceHelper.LoggedException += LoggedExceptionHandler;

            if (m_serviceHelper.StatusLog != null)
                m_serviceHelper.StatusLog.LogException += LogExceptionHandler;

            if (m_serviceHelper.ErrorLogger?.ErrorLog != null)
                m_serviceHelper.ErrorLogger.ErrorLog.LogException += LogExceptionHandler;

            m_serviceHelper.OnStart(args);
        }

        /// <summary>
        /// Handles service stop event.
        /// </summary>
        protected override void OnStop()
        {
            m_serviceHelper.OnStop();
        }

        /// <summary>
        /// Handles service pause event.
        /// </summary>
        protected override void OnPause()
        {
            m_serviceHelper.OnPause();
        }

        /// <summary>
        /// Handles service continue event.
        /// </summary>
        protected override void OnContinue()
        {
            m_serviceHelper.OnResume();
        }

        /// <summary>
        /// Handles service shut down event.
        /// </summary>
        protected override void OnShutdown()
        {
            m_serviceHelper.OnShutdown();
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
        private void StatusMessageHandler(object sender, EventArgs<string, UpdateType> e)
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
        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            LogException(e.Argument);
        }

        /// <summary>
        /// Event handler for processing notifications from adapters that configuration has changed.
        /// </summary>
        /// <param name="sender">Event source of the notification.</param>
        /// <param name="e">Event arguments, if any.</param>
        /// <remarks>
        /// The time-series framework <see cref="IaonSession"/> uses this event to report configuration changes.
        /// </remarks>
        private void ConfigurationChangedHandler(object sender, EventArgs e)
        {
            Action<bool> empty = success =>
            {
            };

            m_reloadConfigQueue.Add(Tuple.Create("System", empty));
        }

        // Handle task scheduler exceptions
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (Exception ex in e.Exception.Flatten().InnerExceptions)
            {
                LogException(ex);
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
            const string RequestCommand = "Health";
            ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(RequestCommand);

            if (requestHandler != null)
            {
                // We pretend to be a client and send a "Health" command to ourselves...
                requestHandler.HandlerMethod(ClientHelper.PretendRequest(RequestCommand));

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

        /// <summary>
        /// Event handler for scheduled reporting services.
        /// </summary>
        /// <param name="name">Scheduled event name.</param>
        /// <param name="parameters">Scheduled event parameters.</param>
        protected virtual void ReportingProcessHandler(string name, object[] parameters)
        {
            // Report processing schedule names end with "Reporting", remove suffix
            //                 123456789
            if (name.EndsWith("Reporting", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 9);

            // Lookup reporting process
            IReportingProcess reportingProcess = m_reportingProcesses.FindReportType(name);

            // Reporting process schedule is due, generate a new report
            if ((object)reportingProcess != null)
            {
                reportingProcess.CleanReportLocation();
                reportingProcess.GenerateReport(DateTime.UtcNow - TimeSpan.FromDays(1), true);
            }
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
                        if (adapter.Initialized)
                        {
                            adapterAction(adapter);
                            SendResponse(requestInfo, true);
                        }
                        else
                        {
                            SendResponse(requestInfo, false, "Unable to {0} -- adapter is not initialized.", actionName);
                        }
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
                Thread invocationThread = new Thread(() =>
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
                                if (method.TryGetAttribute(out commandAttribute) && (!m_serviceHelper.SecureRemoteInteractions || commandAttribute.AllowedRoles.Any(role => Thread.CurrentPrincipal.IsInRole(role))))
                                {
                                    ParameterInfo[] parameterInfo = method.GetParameters();
                                    object returnValue = null;
                                    bool success = true;

                                    if (parameterInfo.Length == 0)
                                    {
                                        // Invoke parameterless adapter command
                                        returnValue = method.Invoke(adapter, null);
                                    }
                                    else
                                    {
                                        int argCount = requestInfo.Request.Arguments.Count - 2;
                                        int attachmentCount = requestInfo.Request.Attachments.Count;

                                        // Create typed parameters for method and invoke
                                        if (argCount + attachmentCount >= parameterInfo.Length)
                                        {
                                            // Attempt to convert command parameters to the method parameter types
                                            object[] parameters = requestInfo.Request.Arguments.Skip(2)
                                                .Select((arg, i) => arg.Value.ConvertToType<object>(parameterInfo[i].ParameterType))
                                                .Concat(requestInfo.Request.Attachments)
                                                .Take(parameterInfo.Length)
                                                .ToArray();

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
                                        if ((object)returnValue == null)
                                            SendResponse(requestInfo, true, "Command \"{0}\" successfully invoked.", command);
                                        else
                                            SendResponseWithAttachment(requestInfo, true, returnValue, "Command \"{0}\" successfully invoked, return value = {1}", command, returnValue.ToNonNullString("null"));
                                    }
                                }
                                else
                                {
                                    SendResponse(requestInfo, false, "Specified command \"{0}\" is not marked as invokable for adapter \"{1}\" [Type = {2}].", command, adapter.Name, adapter.GetType().Name);
                                }
                            }
                            else
                            {
                                SendResponse(requestInfo, false, "Specified command \"{0}\" does not exist for adapter \"{1}\" [Type = {2}].", command, adapter.Name, adapter.GetType().Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            SendResponse(requestInfo, false, "Failed to invoke command: {0}", ex.Message);
                            LogException(ex);
                        }
                    }
                });

                invocationThread.IsBackground = true;
                invocationThread.Start();
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
                        // Get public command methods of specified adapter using reflection
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
                                // Don't bother displaying commands to users who cannot invoke them
                                if (m_serviceHelper.SecureRemoteInteractions && !commandAttribute.AllowedRoles.Any(role => Thread.CurrentPrincipal.IsInRole(role)))
                                    continue;

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
                                    if (typeName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) && typeName.CharCount('.') == 1)
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
                        LogException(ex);
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
                helpMessage.Append("       -SkipReloadConfig".PadRight(20));
                helpMessage.Append("Skips configuration reload before initialize");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                bool requestedAdapterExists = RequestedAdapterExists(requestInfo);

                if (requestInfo.Request.Arguments.Exists("SkipReloadConfig"))
                {
                    if (!requestedAdapterExists)
                    {
                        SendResponse(requestInfo, false, "Unable to initialize disabled or nonexistent adapter with ID.");
                        return;
                    }

                    HandleInitializeRequest(requestInfo);

                    // Spawn routing table calculation updates
                    m_iaonSession.RecalculateRoutingTables();
                }
                else
                {
                    m_reloadConfigQueue.Add(Tuple.Create("System", new Action<bool>(success =>
                    {
                        if (!success)
                        {
                            SendResponse(requestInfo, false, "Failed to load system configuration.");
                            return;
                        }

                        if (requestedAdapterExists)
                            HandleInitializeRequest(requestInfo);
                    })));
                }
            }
        }

        private void HandleInitializeRequest(ClientRequestInfo requestInfo)
        {
            IAdapterCollection collection;
            IAdapter adapter;
            string adapterID;
            uint id;

            // See if specific ID for an adapter was requested
            if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
            {
                adapterID = requestInfo.Request.Arguments["OrderedArg1"];

                // Try initializing new adapter by ID searching in any collection if all runtime ID's are unique
                if (m_uniqueAdapterIDs && uint.TryParse(adapterID, out id) && m_iaonSession.AllAdapters.TryInitializeAdapterByID(id))
                {
                    if (m_iaonSession.AllAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
                        SendResponse(requestInfo, true, "Adapter \"{0}\" ({1}) was successfully initialized...", adapter.Name, adapter.ID);
                    else
                        SendResponse(requestInfo, true, "Adapter ({1}) was successfully initialized...", id);
                }
                else
                {
                    adapter = GetRequestedAdapter(requestInfo, out collection);

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
        }

        private void ListReportsRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Returns a list of reports that are available from the report location.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ListReports ReportType [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ReportType:".PadRight(20));
                helpMessage.Append("Type of the report to process (e.g., Completeness)");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IReportingProcess reportingProcess;

                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    reportingProcess = m_reportingProcesses.FindReportType(requestInfo.Request.Arguments["OrderedArg1"]);
                else
                    throw new ArgumentException("ReportType not specified.");

                if ((object)reportingProcess == null)
                    throw new ArgumentException($"ReportType \"{requestInfo.Request.Arguments["OrderedArg1"]}\" undefined.");

                StringBuilder listBuilder;
                List<string> reportsList;
                List<string> pendingReportsList;

                int fileColumnWidth;
                string reportPath;
                FileInfo info;
                TimeSpan expiration;

                reportingProcess.CleanReportLocation();

                reportsList = reportingProcess.GetReportsList();
                pendingReportsList = reportingProcess.GetPendingReportsList();
                fileColumnWidth = Math.Max(6, reportsList.Concat(pendingReportsList).Select(report => report.Length).DefaultIfEmpty(0).Max()) / 4 * 4 + 4;

                listBuilder = new StringBuilder();
                listBuilder.Append("Report".PadRight(fileColumnWidth));
                listBuilder.AppendLine("Status");
                listBuilder.Append("------".PadRight(fileColumnWidth));
                listBuilder.AppendLine("------");

                foreach (string report in reportsList)
                {
                    reportPath = FilePath.GetAbsolutePath(Path.Combine(reportingProcess.ReportLocation, report));
                    listBuilder.Append(report.PadRight(fileColumnWidth));

                    info = new FileInfo(reportPath);
                    expiration = TimeSpan.FromDays(reportingProcess.IdleReportLifetime) - (DateTime.UtcNow - info.LastAccessTimeUtc);

                    if (expiration.TotalSeconds <= 0.0D)
                        listBuilder.Append("Expired");
                    if (expiration.TotalSeconds < 60.0D)
                        listBuilder.AppendFormat("Expires in {0:0} seconds", expiration.TotalSeconds);
                    else if (expiration.TotalMinutes < 60.0D)
                        listBuilder.AppendFormat("Expires in {0:0} minutes", expiration.TotalMinutes);
                    else if (expiration.TotalHours < 24.0D)
                        listBuilder.AppendFormat("Expires in {0:0} hours", expiration.TotalHours);
                    else
                        listBuilder.AppendFormat("Expires in {0:0} days", expiration.TotalDays);

                    listBuilder.AppendLine();
                }

                foreach (string report in pendingReportsList)
                {
                    listBuilder.Append(report.PadRight(fileColumnWidth));
                    listBuilder.AppendLine("Pending");
                }

                SendResponse(requestInfo, true, listBuilder.ToString());
            }
        }

        private void GetReportRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount == 0)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Gets a report for the specified date.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       GetReport ReportType [ReportDate] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ReportType:".PadRight(20));
                helpMessage.Append("Type of the report to process (e.g., Completeness)");
                helpMessage.AppendLine();
                helpMessage.Append("   ReportDate:".PadRight(20));
                helpMessage.Append("Date of the report to be generated (yyyy-MM-dd), or yesterday if not specified");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IReportingProcess reportingProcess;

                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    reportingProcess = m_reportingProcesses.FindReportType(requestInfo.Request.Arguments["OrderedArg1"]);
                else
                    throw new ArgumentException("ReportType not specified.");

                if ((object)reportingProcess == null)
                    throw new ArgumentException($"ReportType \"{requestInfo.Request.Arguments["OrderedArg1"]}\" undefined.");

                DateTime now = DateTime.UtcNow;
                DateTime today = DateTime.Parse(now.ToString("yyyy-MM-dd"));
                DateTime reportDate;
                string reportPath;

                try
                {
                    if (!requestInfo.Request.Arguments.Exists("OrderedArg2") || !DateTime.TryParse(requestInfo.Request.Arguments["OrderedArg2"], out reportDate))
                        reportDate = today - TimeSpan.FromDays(1);

                    if (reportDate < today)
                    {
                        reportPath = FilePath.GetAbsolutePath(Path.Combine(reportingProcess.ReportLocation, $"{reportingProcess.Title} {reportDate:yyyy-MM-dd}.pdf"));

                        if (File.Exists(reportPath))
                        {
                            File.SetLastAccessTimeUtc(reportPath, DateTime.UtcNow);
                            SendResponseWithAttachment(requestInfo, true, File.ReadAllBytes(reportPath), $"Found and returned report for {reportDate:yyyy-MM-dd}.");
                        }
                        else
                        {
                            SendResponse(requestInfo, false, $"Unable to find report for {reportDate:yyyy-MM-dd}.");
                        }
                    }
                    else
                    {
                        SendResponse(requestInfo, false, $"[{now:yyyy-MM-dd HH:mm:ss}] Unable to generate report for {reportDate:yyyy-MM-dd} until {reportDate + TimeSpan.FromDays(1):yyyy-MM-dd}. The statistics archive is not fully populated for that date.");
                    }
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Unable to generate report due to exception: {0}", ex.Message);
                }
            }
        }

        private void GetReportStatusRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount == 0)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Gets a reporting process status.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       GetReportStatus ReportType [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ReportType:".PadRight(20));
                helpMessage.Append("Type of the report to process (e.g., Completeness)");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IReportingProcess reportingProcess;

                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    reportingProcess = m_reportingProcesses.FindReportType(requestInfo.Request.Arguments["OrderedArg1"]);
                else
                    throw new ArgumentException("ReportType not specified.");

                if ((object)reportingProcess == null)
                    throw new ArgumentException($"ReportType \"{requestInfo.Request.Arguments["OrderedArg1"]}\" undefined.");

                try
                {
                    SendResponse(requestInfo, true, reportingProcess.Status);
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Unable to get report status due to exception: {0}", ex.Message);
                }
            }
        }

        private void GenerateReportRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest || requestInfo.Request.Arguments.OrderedArgCount == 0)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Generates a report for the specified date.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       GenerateReport ReportType [ReportDate] [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ReportType:".PadRight(20));
                helpMessage.Append("Type of the report to process (e.g., Completeness)");
                helpMessage.AppendLine();
                helpMessage.Append("   ReportDate:".PadRight(20));
                helpMessage.Append("Date of the report to be generated (yyyy-MM-dd), or yesterday if not specified");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -email".PadRight(20));
                helpMessage.Append("E-mail generated report, if enabled");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IReportingProcess reportingProcess;

                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    reportingProcess = m_reportingProcesses.FindReportType(requestInfo.Request.Arguments["OrderedArg1"]);
                else
                    throw new ArgumentException("ReportType not specified.");

                if ((object)reportingProcess == null)
                    throw new ArgumentException($"ReportType \"{requestInfo.Request.Arguments["OrderedArg1"]}\" undefined.");

                DateTime today = DateTime.UtcNow;
                DateTime reportDate;
                bool emailReport = requestInfo.Request.Arguments.Exists("email");

                try
                {
                    if (!requestInfo.Request.Arguments.Exists("OrderedArg2") || !DateTime.TryParse(requestInfo.Request.Arguments["OrderedArg2"], out reportDate))
                        reportDate = today - TimeSpan.FromDays(1);

                    if (reportDate < today)
                    {
                        reportingProcess.GenerateReport(reportDate, emailReport);
                        SendResponse(requestInfo, true, "Report was successfully queued for generation.");
                    }
                    else
                    {
                        SendResponse(requestInfo, false, $"[{today:yyyy-MM-dd HH:mm:ss}] Unable to generate report for {reportDate:yyyy-MM-dd} until {reportDate + TimeSpan.FromDays(1):yyyy-MM-dd}. The statistics archive is not fully populated for that date.");
                    }
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Unable to generate report due to exception: {0}", ex.Message);
                }
            }
        }

        private void ReportingConfigRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Displays or modifies the configuration of the reporting process.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       ReportingConfig ReportType [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   ReportType:".PadRight(20));
                helpMessage.Append("Type of the report to process (e.g., Completeness)");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -get".PadRight(20));
                helpMessage.Append("Gets the configuration of the reporting process as command-line arguments");
                helpMessage.AppendLine();
                helpMessage.Append("       -set Args".PadRight(20));
                helpMessage.Append("Sets the configuration of the reporting process using the given command-line arguments");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                IReportingProcess reportingProcess;

                if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                    reportingProcess = m_reportingProcesses.FindReportType(requestInfo.Request.Arguments["OrderedArg1"]);
                else
                    throw new ArgumentException("ReportType not specified.");

                if ((object)reportingProcess == null)
                    throw new ArgumentException($"ReportType \"{requestInfo.Request.Arguments["OrderedArg1"]}\" undefined.");

                if (requestInfo.Request.Arguments.Exists("get") || !requestInfo.Request.Arguments.Exists("set"))
                {
                    try
                    {
                        string configuration = $"{reportingProcess.GetArguments()} --idleReportLifetime=\" {reportingProcess.IdleReportLifetime} \"";

                        SendResponse(requestInfo, true, configuration);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Unable to send reporting configuration due to exception: {0}", ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        lock (reportingProcess)
                        {
                            reportingProcess.SetArguments(requestInfo.Request.Arguments);
                            reportingProcess.SaveSettings();
                            SendResponse(requestInfo, true, "Reporting configuration saved successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Unable to save settings reporting configuration due to exception: {0}", ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Sends remote entry for logging.
        /// </summary>
        /// <param name="requestInfo"></param>
        protected virtual void LogEventRequestHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Logs remote entry to event log.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       LogEvent [Options]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();
                helpMessage.Append("       -Message=\"Event Message\"".PadRight(20));
                helpMessage.Append("Specifies message for event log entry (required)");
                helpMessage.AppendLine();
                helpMessage.Append("       -Type=[Error|Warning|Information|...]".PadRight(20));
                helpMessage.Append("Specifies EventLogEntryType setting (optional)");
                helpMessage.AppendLine();
                helpMessage.Append("       -ID=0".PadRight(20));
                helpMessage.Append("Specifies application event log ID (optional)");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (requestInfo.Request.Arguments.Exists("Message"))
                {
                    try
                    {
                        string message = requestInfo.Request.Arguments["Message"];
                        string type, id;
                        EventLogEntryType entryType;
                        ushort eventID;

                        if (!(requestInfo.Request.Arguments.TryGetValue("Type", out type) && Enum.TryParse(type, out entryType)))
                            entryType = EventLogEntryType.Information;

                        if (!(requestInfo.Request.Arguments.TryGetValue("ID", out id) && ushort.TryParse(id, out eventID)))
                            eventID = 0;

                        EventLog.WriteEntry(ServiceName, message, entryType, eventID);
                        SendResponse(requestInfo, true, "Successfully wrote event log entry.");
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Failed to write event log entry: {0}", ex.Message);
                    }
                }
                else
                {
                    SendResponse(requestInfo, false, "Failed to write event log entry: required \"message\" parameter was not specified.");
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
        protected virtual void ReloadConfigRequestHandler(ClientRequestInfo requestInfo)
        {
            Arguments arguments = requestInfo.Request.Arguments;

            ClientRequestInfo invokeInfo = null;
            ClientRequestHandler invokeHandler = null;
            string resource;
            bool invoking;

            if (arguments.ContainsHelpRequest)
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
                helpMessage.AppendLine();
                helpMessage.Append("       -Invoke=\"Command [Args]\"".PadRight(20));
                helpMessage.AppendFormat("Invokes another command once the reload config is complete");
                helpMessage.AppendLine();
                helpMessage.Append("       -Augmented".PadRight(20));
                helpMessage.AppendFormat("Augments the current running configuration from the {0}, if supported", m_configurationType);
                helpMessage.AppendLine();
                helpMessage.Append($"       -{m_configurationType}".PadRight(20));
                helpMessage.AppendFormat("Loads configuration from the {0}", m_configurationType);
                helpMessage.AppendLine();
                helpMessage.Append("       -BinaryCache".PadRight(20));
                helpMessage.Append("Loads configuration from the latest cached binary file");
                helpMessage.AppendLine();
                helpMessage.Append("       -XmlCache".PadRight(20));
                helpMessage.Append("Loads configuration from the latest cached XML file");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                invoking = arguments.Exists("Invoke");

                if (invoking)
                {
                    invokeInfo = new ClientRequestInfo(requestInfo.Sender, ClientRequest.Parse(arguments["Invoke"]));
                    resource = invokeInfo.Request.Command;

                    // Check if remote client has permission to invoke the requested command.
                    if (m_serviceHelper.SecureRemoteInteractions)
                    {
                        // Validate current client principal
                        if (SecurityProviderUtility.IsResourceSecurable(resource) && !SecurityProviderUtility.IsResourceAccessible(resource))
                            throw new SecurityException($"Access to '{resource}' is denied");
                    }

                    invokeHandler = m_serviceHelper.FindClientRequestHandler(resource);
                }

                if (arguments.Count == (invoking ? 2 : 1))
                {
                    if (!arguments.Exists(m_configurationType.ToString()) && !arguments.Exists("Augmented") && !arguments.Exists("BinaryCache") && !arguments.Exists("XmlCache"))
                    {
                        SendResponse(requestInfo, false, "Invalid argument supplied to ReloadConfig command.");
                        return;
                    }
                }
                else if (arguments.Count > (invoking ? 2 : 1))
                {
                    SendResponse(requestInfo, false, "Invalid arguments supplied to ReloadConfig command.");
                    return;
                }

                m_reloadConfigQueue.Add(Tuple.Create(GetReloadConfigType(requestInfo), new Action<bool>(success =>
                {
                    if (success)
                    {
                        if (invoking && (object)invokeHandler != null)
                            invokeHandler.HandlerMethod(invokeInfo);
                        else
                            SendResponse(requestInfo, true, "System configuration was successfully reloaded.");
                    }
                    else
                    {
                        SendResponse(requestInfo, false, "System configuration failed to reload.");
                    }
                })));
            }
        }

        private string GetReloadConfigType(ClientRequestInfo requestInfo)
        {
            Arguments arguments = requestInfo.Request.Arguments;

            if (arguments.Exists("Augmented"))
                return "Augmented";

            if (arguments.Exists(m_configurationType.ToString()))
                return m_configurationType.ToString();

            if (arguments.Exists("BinaryCache"))
                return "BinaryCache";

            if (arguments.Exists("XmlCache"))
                return "XmlCache";

            return "System";
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
                        if ((object)xmlDoc.DocumentElement != null)
                        {
                            foreach (XmlNode node in xmlDoc.DocumentElement)
                            {
                                categoryList.Append("   ");
                                categoryList.Append(node.Name);
                                categoryList.AppendLine();
                            }
                        }

                        categoryList.AppendLine();
                        DisplayResponseMessage(requestInfo, categoryList.ToString());
                    }
                    else
                    {
                        CategorizedSettingsElementCollection settings = config.Settings[categoryName];
                        StringBuilder settingsList = new StringBuilder();
                        settingsList.Append($"List of settings under the category {categoryName}:");
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
                        if ((object)setting == null)
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
        /// Attempts to authenticate or re-authenticate to network shares.
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
                DisplayStatusMessage("Attempting to re-authenticate network shares for health and status exports...", UpdateType.Information);

                try
                {
                    m_healthExporter.Initialize();
                    m_statusExporter.Initialize();
                    SendResponse(requestInfo, true);
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Failed to re-authenticate network shares: {0}", ex.Message);
                    LogException(ex);
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
                        LogException(ex);
                    }
                }
                else
                {
                    DisplayStatusMessage("Remote restart request denied, this is currently disallowed in the system configuration.", UpdateType.Warning);
                }
            }
        }

        /// <summary>
        /// Determines whether the requested adapter exists.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <returns>True if the requested adapter exists; false otherwise.</returns>
        private bool RequestedAdapterExists(ClientRequestInfo requestInfo)
        {
            IAdapter adapter;
            string adapterID = requestInfo.Request.Arguments["OrderedArg1"];
            IAdapterCollection collection = GetRequestedCollection(requestInfo);

            if (!string.IsNullOrWhiteSpace(adapterID))
            {
                uint id;

                adapterID = adapterID.Trim();

                if (adapterID.IsAllNumbers() && uint.TryParse(adapterID, out id))
                {
                    // Adapter ID is numeric, try numeric lookup by adapter ID in requested collection
                    if (collection.TryGetAdapterByID(id, out adapter))
                        return true;

                    // Try looking for ID in any collection if all runtime ID's are unique
                    if (m_uniqueAdapterIDs && m_iaonSession.AllAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
                        return true;
                }
                else
                {
                    // Adapter ID is alpha-numeric, try text-based lookup by adapter name in requested collection
                    if (collection.TryGetAdapterByName(adapterID, out adapter))
                        return true;

                    // Try looking for adapter name in any collection
                    if (m_iaonSession.AllAdapters.TryGetAnyAdapterByName(adapterID, out adapter, out collection))
                        return true;
                }
            }

            return false;
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
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, $"Failed to send client response due to an exception: {ex.Message}\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a response message to client requester.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            try
            {
                m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, $"{status}\r\n\r\n", args);
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, $"Failed to update client status \"{status.ToNonNullString()}\" due to an exception: {ex.Message}\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type)
        {
            DisplayStatusMessage(status, type, true);
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        /// <param name="publishToLog">Determines if messages should be sent logging engine.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type, bool publishToLog)
        {
            try
            {
                status = status.Replace("{", "{{").Replace("}", "}}");
                m_serviceHelper.UpdateStatus(type, publishToLog, $"{status}\r\n\r\n");
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, $"Failed to update client status \"{status.ToNonNullString()}\" due to an exception: {ex.Message}\r\n\r\n");
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
            DisplayStatusMessage(status, type, true, args);
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Formatted status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        /// <param name="publishToLog">Determines if messages should be sent logging engine.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type, bool publishToLog, params object[] args)
        {
            try
            {
                DisplayStatusMessage(string.Format(status, args), type, publishToLog);
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, $"Failed to update client status \"{status.ToNonNullString()}\" due to an exception: {ex.Message}\r\n\r\n");
            }
        }

        /// <summary>
        /// Logs an exception to the service helper <see cref="GSF.ErrorManagement.ErrorLogger"/>.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to log.</param>
        protected virtual void LogException(Exception ex)
        {
            m_serviceHelper.LogException(ex);
        }

        // Processes exceptions coming from the configuration loaders.
        private void ConfigurationLoader_ProcessException(object sender, EventArgs<Exception> args)
        {
            DisplayStatusMessage(args.Argument.Message, UpdateType.Warning);
            LogException(args.Argument);
        }

        #endregion

        #endregion
    }
}
