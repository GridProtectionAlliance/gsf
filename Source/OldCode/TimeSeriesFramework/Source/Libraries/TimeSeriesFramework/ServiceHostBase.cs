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
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using TVA;
using TVA.Configuration;
using TVA.Data;
using TVA.IO;

using TimeSeriesFramework.Adapters;
using TVA.Reflection;
using TVA.Services;
using TVA.Units;

namespace TimeSeriesFramework
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
    /// <param name="statusMessage">Reference to host status message function.</param>
    /// <param name="processException">Reference to host process exception function.</param>
    public delegate void DataOperationFunction(IDbConnection connection, Type adapterType, string nodeIDQueryString, Action<object, EventArgs<string>> statusMessage, Action<object, EventArgs<Exception>> processException);

    /// <summary>
    /// Represents the time-series framework service host.
    /// </summary>
    public partial class ServiceHostBase : ServiceBase
    {
        #region [ Members ]

        // Fields

        // Input, action and output adapters
        private AllAdaptersCollection m_allAdapters;
        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;
        private OutputAdapterCollection m_outputAdapters;

        // System settings
        private Guid m_nodeID;
        private string m_nodeIDQueryString;
        private DataSet m_configuration;
        private ConfigurationType m_configurationType;
        private string m_connectionString;
        private string m_dataProviderString;
        private string m_cachedConfigurationFile;
        private bool m_uniqueAdapterIDs;
        private Dictionary<object, string> m_derivedNameCache;

        // Threshold settings
        private int m_measurementWarningThreshold;
        private int m_measurementDumpingThreshold;
        private int m_defaultSampleSizeWarningThreshold;

        // Health and status exporters
        private MultipleDestinationExporter m_healthExporter;
        private MultipleDestinationExporter m_statusExporter;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ServiceHostBase"/>.
        /// </summary>
        public ServiceHostBase()
            : base()
        {
            InitializeComponent();

            // Register event handlers.
            m_serviceHelper.ServiceStarting += ServiceStartingHandler;
            m_serviceHelper.ServiceStarted += ServiceStartedHandler;
            m_serviceHelper.ServiceStopping += ServiceStoppingHandler;
            m_serviceHelper.StatusLog.LogException += ProcessExceptionHandler;
            m_serviceHelper.ErrorLogger.ErrorLog.LogException += ProcessExceptionHandler;
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
        /// Gets reference to the <see cref="AllAdaptersCollection"/>.
        /// </summary>
        protected AllAdaptersCollection AllAdapters
        {
            get
            {
                return m_allAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="InputAdaptersCollection"/>.
        /// </summary>
        protected InputAdapterCollection InputAdapters
        {
            get
            {
                return m_inputAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        protected ActionAdapterCollection ActionAdapters
        {
            get
            {
                return m_actionAdapters;
            }
        }

        /// <summary>
        /// Gets reference to the <see cref="OutputAdapterCollection"/>.
        /// </summary>
        protected OutputAdapterCollection OutputAdapters
        {
            get
            {
                return m_outputAdapters;
            }
        }

        /// <summary>
        /// Gets the current node ID.
        /// </summary>
        protected Guid NodeID
        {
            get
            {
                return m_nodeID;
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
        protected DataSet Configuration
        {
            get
            {
                return m_configuration;
            }
        }

        /// <summary>
        /// Gets the defined system <see cref="TimeSeriesFramework.ConfigurationType"/>.
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
            // Make sure default service settings exist
            ConfigurationFile configFile = ConfigurationFile.Current;
            string cachePath = string.Format("{0}\\ConfigurationCache\\", FilePath.GetAbsolutePath(""));

            // System settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");
            systemSettings.Add("ConfigurationType", "Database", "Specifies type of configuration: Database, WebService or XmlFile");
            systemSettings.Add("ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=IaonHost.mdb", "Configuration database connection string");
            systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Configuration database ADO.NET data provider assembly type creation string");
            systemSettings.Add("ConfigurationCachePath", cachePath, "Defines the path used to cache serialized configurations");
            systemSettings.Add("CachedConfigurationFile", "SystemConfiguration.xml", "File name for last known good system configuration (only cached for a Database or WebService connection)");
            systemSettings.Add("UniqueAdaptersIDs", "True", "Set to true if all runtime adapter ID's will be unique to allow for easier adapter specification");
            systemSettings.Add("ProcessPriority", "RealTime", "Sets desired process priority: Normal, AboveNormal, High, RealTime");

            // Example connection settings
            CategorizedSettingsElementCollection exampleSettings = configFile.Settings["exampleConnectionSettings"];
            exampleSettings.Add("SqlServer.ConnectionString", "Data Source=serverName;Initial Catalog=openPDC;User Id=userName;Password=password", "Example SQL Server database connection string");
            exampleSettings.Add("SqlServer.DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.SqlClient.SqlConnection;AdapterType=System.Data.SqlClient.SqlDataAdapter", "Example SQL Server database .NET provider string");
            exampleSettings.Add("MySQL.ConnectionString", "Server=serverName;Database=openPDC;Uid=root;Pwd=password", "Example MySQL database connection string");
            exampleSettings.Add("MySQL.DataProviderString", "AssemblyName={MySql.Data, Version=6.2.3.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d};ConnectionType=MySql.Data.MySqlClient.MySqlConnection;AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter", "Example MySQL database .NET provider string");
            exampleSettings.Add("Oracle.ConnectionString", "Data Source=openPDC;User Id=username;Password=password;Integrated Security=no", "Example Oracle database connection string");
            exampleSettings.Add("Oracle.DataProviderString", "AssemblyName={System.Data.OracleClient, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OracleClient.OracleConnection;AdapterType=System.Data.OracleClient.OracleDataAdapter", "Example Oracle database .NET provider string");
            exampleSettings.Add("OleDB.ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=openPDC.mdb", "Example Microsoft Access (via OleDb) database connection string");
            exampleSettings.Add("OleDB.DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Example OleDb database .NET provider string");
            exampleSettings.Add("Odbc.ConnectionString", "Driver={SQL Server Native Client 10.0};Server=serverName;Database=openPDC;Uid=userName;Pwd=password;", "Example ODBC database connection string");
            exampleSettings.Add("Odbc.DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.Odbc.OdbcConnection;AdapterType=System.Data.Odbc.OdbcDataAdapter", "Example ODBC database .NET provider string");
            exampleSettings.Add("WebService.ConnectionString", "https://naspi.tva.com/openPDC/LoadConfigurationData.aspx", "Example web service connection string");
            exampleSettings.Add("XmlFile.ConnectionString", "SystemConfiguration.xml", "Example XML configuration file connection string");

            // Threshold settings
            CategorizedSettingsElementCollection thresholdSettings = configFile.Settings["thresholdSettings"];
            thresholdSettings.Add("MeasurementWarningThreshold", "100000", "Number of unarchived measurements allowed in any output adapter queue before displaying a warning message");
            thresholdSettings.Add("MeasurementDumpingThreshold", "500000", "Number of unarchived measurements allowed in any output adapter queue before taking evasive action and dumping data");
            thresholdSettings.Add("DefaultSampleSizeWarningThreshold", "10", "Default number of unpublished samples (in seconds) allowed in any action adapter queue before displaying a warning message");

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
            m_nodeID = systemSettings["NodeID"].ValueAs<Guid>();
            m_configurationType = systemSettings["ConfigurationType"].ValueAs<ConfigurationType>();
            m_connectionString = systemSettings["ConnectionString"].Value;
            m_dataProviderString = systemSettings["DataProviderString"].Value;
            m_cachedConfigurationFile = FilePath.AddPathSuffix(cachePath) + systemSettings["CachedConfigurationFile"].Value;
            m_uniqueAdapterIDs = systemSettings["UniqueAdaptersIDs"].ValueAsBoolean(true);
            m_derivedNameCache = new Dictionary<object, string>();

            // Define guid with query string delimeters according to database needs
            Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
            string setting;

            if (settings.TryGetValue("Provider", out setting))
            {
                // Check if provider is for Access since it uses braces as Guid delimeters
                if (setting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                {
                    m_nodeIDQueryString = "{" + m_nodeID + "}";

                    // Make sure path to Access database is fully qualified
                    if (settings.TryGetValue("Data Source", out setting))
                    {
                        settings["Data Source"] = FilePath.GetAbsolutePath(setting);
                        m_connectionString = settings.JoinKeyValuePairs();
                    }
                }
            }

            if (string.IsNullOrEmpty(m_nodeIDQueryString))
                m_nodeIDQueryString = "'" + m_nodeID + "'";

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

            // Initialize threshold settings
            m_measurementWarningThreshold = thresholdSettings["MeasurementWarningThreshold"].ValueAsInt32();
            m_measurementDumpingThreshold = thresholdSettings["MeasurementDumpingThreshold"].ValueAsInt32();
            m_defaultSampleSizeWarningThreshold = thresholdSettings["DefaultSampleSizeWarningThreshold"].ValueAsInt32();
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
                " Process Account: {10}\\{11}\r\n\r\n" +
                "{12}\r\n",
                stars, 
                m_nodeID, 
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                FilePath.TrimFileName(FilePath.RemovePathSuffix(FilePath.GetAbsolutePath("")), 61),
                Environment.MachineName,
                Environment.OSVersion.VersionString,
                Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", null).ToNonNullString("<Unavailable>"),
                SI2.ToScaledIECString(Environment.WorkingSet, 3, "B"),
                IntPtr.Size * 8,
                Environment.ProcessorCount,
                Environment.UserDomainName,
                Environment.UserName,
                stars);            

            // Create health exporter
            m_healthExporter = new MultipleDestinationExporter("HealthExporter", Timeout.Infinite);
            m_healthExporter.Initialize(new ExportDestination[] { new ExportDestination(FilePath.GetAbsolutePath("Health.txt"), false, "", "", "") });
            m_healthExporter.StatusMessage += StatusMessageHandler;
            m_serviceHelper.ServiceComponents.Add(m_healthExporter);

            // Create status exporter
            m_statusExporter = new MultipleDestinationExporter("StatusExporter", Timeout.Infinite);
            m_statusExporter.Initialize(new ExportDestination[] { new ExportDestination(FilePath.GetAbsolutePath("Status.txt"), false, "", "", "") });
            m_statusExporter.StatusMessage += StatusMessageHandler;
            m_serviceHelper.ServiceComponents.Add(m_statusExporter);

            // Define scheduled service processes
            m_serviceHelper.AddScheduledProcess(HealthMonitorProcessHandler, "HealthMonitor", "* * * * *");    // Every minute
            m_serviceHelper.AddScheduledProcess(StatusExportProcessHandler, "StatusExport", "*/30 * * * *");   // Every 30 minutes

            // Create a collection to manage all input, action and output adapter collections as a unit
            m_allAdapters = new AllAdaptersCollection();
            m_allAdapters.StatusMessage += StatusMessageHandler;
            m_allAdapters.ProcessException += ProcessExceptionHandler;
            m_allAdapters.Disposed += DisposedHandler;

            // Create input adapters collection
            m_inputAdapters = new InputAdapterCollection();
            m_inputAdapters.NewMeasurements += NewMeasurementsHandler;
            m_serviceHelper.ServiceComponents.Add(m_inputAdapters);

            // Create action adapters collection
            m_actionAdapters = new ActionAdapterCollection();
            m_actionAdapters.NewMeasurements += NewMeasurementsHandler;
            m_actionAdapters.UnpublishedSamples += UnpublishedSamplesHandler;
            m_serviceHelper.ServiceComponents.Add(m_actionAdapters);

            // Create output adapters collection
            m_outputAdapters = new OutputAdapterCollection();
            m_outputAdapters.UnprocessedMeasurements += UnprocessedMeasurementsHandler;
            m_serviceHelper.ServiceComponents.Add(m_outputAdapters);

            // We group these adapters such that they are initialized in the following order: output, input, action. This
            // is done so that the archival capabilities will be setup before we start receiving input and the input data
            // will be flowing before any actions get established for the input.
            m_allAdapters.Add(m_outputAdapters);
            m_allAdapters.Add(m_inputAdapters);
            m_allAdapters.Add(m_actionAdapters);

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

            // Start system initialization on an independent thread so that service responds in a timely fashion...
            ThreadPool.QueueUserWorkItem(InitializeSystem);
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
            if (m_healthExporter != null)
            {
                m_healthExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_healthExporter);
                m_healthExporter.StatusMessage -= StatusMessageHandler;
                m_healthExporter.Dispose();
            }
            m_healthExporter = null;

            // Dispose system status exporter
            if (m_statusExporter != null)
            {
                m_statusExporter.Enabled = false;
                m_serviceHelper.ServiceComponents.Remove(m_statusExporter);
                m_statusExporter.StatusMessage -= StatusMessageHandler;
                m_statusExporter.Dispose();
            }
            m_statusExporter = null;

            // Dispose input adapters collection
            if (m_inputAdapters != null)
            {
                m_inputAdapters.Stop();
                m_serviceHelper.ServiceComponents.Remove(m_inputAdapters);
                m_inputAdapters.NewMeasurements -= NewMeasurementsHandler;
                m_inputAdapters.Dispose();
            }
            m_inputAdapters = null;

            // Dispose action adapters collection
            if (m_actionAdapters != null)
            {
                m_actionAdapters.Stop();
                m_serviceHelper.ServiceComponents.Remove(m_actionAdapters);
                m_actionAdapters.NewMeasurements -= NewMeasurementsHandler;
                m_actionAdapters.UnpublishedSamples -= UnpublishedSamplesHandler;
                m_actionAdapters.Dispose();
            }
            m_actionAdapters = null;

            // Dispose output adapters collection
            if (m_outputAdapters != null)
            {
                m_outputAdapters.Stop();
                m_serviceHelper.ServiceComponents.Remove(m_outputAdapters);
                m_outputAdapters.UnprocessedMeasurements -= UnprocessedMeasurementsHandler;
                m_outputAdapters.Dispose();
            }
            m_outputAdapters = null;

            // Dispose all adapters collection
            if (m_allAdapters != null)
            {
                m_allAdapters.StatusMessage -= StatusMessageHandler;
                m_allAdapters.ProcessException -= ProcessExceptionHandler;
                m_allAdapters.Disposed -= DisposedHandler;
                m_allAdapters.Dispose();
            }
            m_allAdapters = null;
        }

        #endregion

        #region [ System Initialization ]

        // Perform system initialization
        private void InitializeSystem(object state)
        {
            // Attempt to load system configuration
            if (LoadSystemConfiguration())
            {
                // Initialize all adapters
                m_allAdapters.Initialize();

                // Start all adapters
                m_allAdapters.Start();

                DisplayStatusMessage("System initialization complete.", UpdateType.Information);

                // If any settings have been added to configuration file, we go ahead and save them now
                m_serviceHelper.SaveSettings(true);
                ConfigurationFile.Current.Save();
            }
            else
                DisplayStatusMessage("System initialization failed due to unavailable configuration.", UpdateType.Alarm);
        }

        // Load the the system configuration data set
        private bool LoadSystemConfiguration()
        {
            DisplayStatusMessage("Loading system configuration...", UpdateType.Information);

            // Attempt to load (or reload) system configuration
            m_configuration = GetConfigurationDataSet(m_configurationType, m_connectionString, m_dataProviderString);

            if (m_configuration != null)
            {
                // Update data source on all adapters in all collections
                m_allAdapters.DataSource = m_configuration;
                return true;
            }

            return false;
        }

        // Load system configuration data set
        private DataSet GetConfigurationDataSet(ConfigurationType configType, string connectionString, string dataProviderString)
        {
            DataSet configuration = null;
            DataTable entities, entity;

            switch (configType)
            {
                case ConfigurationType.Database:
                    // Attempt to load configuration from a database connection
                    IDbConnection connection = null;
                    Dictionary<string, string> settings;
                    string assemblyName, connectionTypeName, adapterTypeName;
                    Assembly assembly;
                    Type connectionType, adapterType;

                    try
                    {
                        settings = dataProviderString.ParseKeyValuePairs();
                        assemblyName = settings["AssemblyName"].ToNonNullString();
                        connectionTypeName = settings["ConnectionType"].ToNonNullString();
                        adapterTypeName = settings["AdapterType"].ToNonNullString();

                        if (string.IsNullOrEmpty(connectionTypeName))
                            throw new InvalidOperationException("Database connection type was not defined.");

                        if (string.IsNullOrEmpty(adapterTypeName))
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

                        // Add each configuration entity to the system configuration
                        foreach (DataRow row in entities.Rows)
                        {
                            // Load configuration entity data filtered by node ID
                            entity = connection.RetrieveData(adapterType, string.Format("SELECT * FROM {0} WHERE NodeID={1}", row["SourceName"].ToString(), m_nodeIDQueryString));
                            entity.TableName = row["RuntimeName"].ToString();

                            DisplayStatusMessage("Loaded configuration entity {0} with {1} rows of data...", UpdateType.Information, entity.TableName, entity.Rows.Count);

                            // Remove redundant node ID column
                            entity.Columns.Remove("NodeID");

                            // Add entity configuration data to system configuration
                            configuration.Tables.Add(entity.Copy());
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

                        if (string.IsNullOrEmpty(assemblyName))
                            throw new InvalidOperationException("Data operation assembly name was not defined.");

                        if (string.IsNullOrEmpty(typeName))
                            throw new InvalidOperationException("Data operation type name was not defined.");

                        if (string.IsNullOrEmpty(methodName))
                            throw new InvalidOperationException("Data operation method name was not defined.");

                        // Load data operation from containing assembly and type
                        assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));
                        type = assembly.GetType(typeName);
                        method = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

                        // Execute data operation via loaded assembly method
                        ((DataOperationFunction)Delegate.CreateDelegate(typeof(DataOperationFunction), method))(connection, adapterType, m_nodeIDQueryString, new Action<object, EventArgs<string>>(StatusMessageHandler), new Action<object, EventArgs<Exception>>(ProcessExceptionHandler));
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

        // Create newly defined adapters and remove adapters that are no longer present in the adapter collection configurations
        private void UpdateAdapterCollectionConfigurations()
        {
            foreach (IAdapterCollection adapterCollection in m_allAdapters)
            {
                string dataMember = adapterCollection.DataMember;

                if (m_configuration.Tables.Contains(dataMember))
                {
                    // Remove adapters that are no longer present in the configuration
                    for (int i = adapterCollection.Count - 1; i >= 0; i--)
                    {
                        IAdapter adapter = adapterCollection[i];
                        DataRow[] adapterRows = m_configuration.Tables[dataMember].Select(string.Format("ID = {0}", adapter.ID));

                        if (adapterRows.Length == 0 && adapter.ID != 0)
                            adapterCollection.Remove(adapter);
                    }

                    // Create newly defined adapters
                    foreach (DataRow adapterRow in m_configuration.Tables[dataMember].Rows)
                    {
                        IAdapter adapter;

                        if (!adapterCollection.TryGetAdapterByID(uint.Parse(adapterRow["ID"].ToNonNullString("0")), out adapter) && adapterCollection.TryCreateAdapter(adapterRow, out adapter))
                            adapterCollection.Add(adapter);
                    }
                }
            }
        }

        // Cache the current system configuration so it can be used if primary configuration source is unavailable
        protected virtual void CacheCurrentConfiguration(DataSet configuration)
        {
            try
            {
                // Back up existing configuration file, if any
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

        #endregion

        #region [ Primary Adapter Event Handlers ]

        /// <summary>
        /// Event handler for distributing new measurements.
        /// </summary>
        /// <param name="sender">Event source reference to adapter that generated new measurements.</param>
        /// <param name="e">Event arguments containing a collection of new measurements.</param>
        /// <remarks>
        /// Time-series framework uses this handler to route new measurements to the action and output adapters.
        /// </remarks>
        protected virtual void NewMeasurementsHandler(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            ICollection<IMeasurement> measurements = e.Argument;

            // All new measurements get passed to action and output adapters for processing
            m_actionAdapters.QueueMeasurementsForProcessing(measurements);
            m_outputAdapters.QueueMeasurementsForProcessing(measurements);
        }

        /// <summary>
        /// Event handler for monitoring unpublished samples.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an action adapter, that is reporting the number of unpublished data samples.</param>
        /// <param name="e">Event arguments containing number of samples, in seconds of data, of unpublished data in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unpublished samples, in seconds of data, in action adapters.<br/>
        /// This method is typically called once per second.
        /// </remarks>
        protected virtual void UnpublishedSamplesHandler(object sender, EventArgs<int> e)
        {
            int secondsOfData = e.Argument;
            int threshold = m_defaultSampleSizeWarningThreshold;
            ConcentratorBase concentrator = sender as ConcentratorBase;

            // Most action adapters will be based on a concentrator, if so we monitor the unpublished sample queue size compared to the defined
            // lag time - if the queue size is over twice the lag size, the action adapter could be falling behind
            if (concentrator != null)
                threshold = (int)(2 * Math.Ceiling(concentrator.LagTime));

            if (secondsOfData > threshold)
                DisplayStatusMessage("[{0}] There are {1} seconds of unpublished data in the action adapter concentration queue.", UpdateType.Warning, GetDerivedName(sender), secondsOfData);
        }

        /// <summary>
        /// Event handler for monitoring unprocessed measurements.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an output adapter, that is reporting the number of unprocessed measurements.</param>
        /// <param name="e">Event arguments containing number of queued (i.e., unprocessed) measurements in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unprocessed measurements in output adapters.<br/>
        /// This method is typically called once per second.
        /// </remarks>
        protected virtual void UnprocessedMeasurementsHandler(object sender, EventArgs<int> e)
        {
            int unprocessedMeasurements = e.Argument;

            if (unprocessedMeasurements > m_measurementDumpingThreshold)
            {
                IOutputAdapter outputAdpater = sender as IOutputAdapter;

                if (outputAdpater != null)
                {
                    // If an output adapter queue size exceeds the defined measurement dumping threshold,
                    // then the queue will be truncated before system runs out of memory
                    outputAdpater.RemoveMeasurements(m_measurementDumpingThreshold);
                    DisplayStatusMessage("[{0}] System exercised evasive action to convserve memory and dumped {1} unprocessed measurements from the output queue :(", UpdateType.Alarm, outputAdpater.Name, m_measurementDumpingThreshold);
                    DisplayStatusMessage("[{0}] NOTICE: Please adjust measurement threshold settings and/or increase amount of available system memory.", UpdateType.Warning, outputAdpater.Name);
                }
                else
                    // It is only expected that output adapters will be mapped to this handler, but in case
                    // another adapter type uses this handler we will still display a message
                    DisplayStatusMessage("[{0}] CRITICAL: There are {1} unprocessed measurements in the adapter queue - but sender \"{2}\" is not an IOutputAdapter, so no evasive action can be exercised.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements, sender.GetType().Name);
            }
            else if (unprocessedMeasurements > m_measurementWarningThreshold)
            {
                if (unprocessedMeasurements >= m_measurementDumpingThreshold - m_measurementWarningThreshold)
                    DisplayStatusMessage("[{0}] CRITICAL: There are {1} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
                else
                    DisplayStatusMessage("[{0}] There are {1} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
            }
        }

        /// <summary>
        /// Event handler for reporting status messages.
        /// </summary>
        /// <param name="sender">Event source of the status message.</param>
        /// <param name="e">Event arguments containing the status message to report.</param>
        /// <remarks>
        /// Time-series framework uses this handler to report adapter status messages (e.g., to a log file or console window).
        /// </remarks>
        protected virtual void StatusMessageHandler(object sender, EventArgs<string> e)
        {
            DisplayStatusMessage("[{0}] {1}", UpdateType.Information, GetDerivedName(sender), e.Argument);
        }

        // Handle process exceptions from all adapters
        /// <summary>
        /// Event handler for processing reported exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            m_serviceHelper.ErrorLogger.Log(ex, false);
            DisplayStatusMessage("[{0}] {1}", UpdateType.Alarm, GetDerivedName(sender), ex.Message);
        }

        // Handle disposed events from all adapters
        protected virtual void DisposedHandler(object sender, EventArgs e)
        {
            DisplayStatusMessage("[{0}] Disposed.", UpdateType.Information, GetDerivedName(sender));
        }

        // Handle health monitoring processing
        protected virtual void HealthMonitorProcessHandler(string name, object[] parameters)
        {
            string requestCommand = "Health";
            ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(requestCommand);

            if (requestHandler != null)
            {
                // We pretend to be a client and send a "Health" command to ourselves...
                requestHandler.HandlerMethod(ClientHelper.PretendRequest(requestCommand));

                // We also export human readable health information to a text file for external display
                m_healthExporter.ExportData(m_serviceHelper.PerformanceMonitor.Status);
            }
        }

        // Handle status export processing
        protected virtual void StatusExportProcessHandler(string name, object[] parameters)
        {
            // Every thirty minutes we export a human readable service status to a text file for external display
            m_statusExporter.ExportData(m_serviceHelper.Status);
        }

        // Attempt to get name of component raising an event
        protected virtual string GetDerivedName(object sender)
        {
            string name;

            if (!m_derivedNameCache.TryGetValue(sender, out name))
            {
                IProvideStatus statusProvider = sender as IProvideStatus;

                if (statusProvider != null)
                    name = statusProvider.Name.NotEmpty(sender.GetType().Name);

                else if (sender != null && sender is string)
                    name = (string)sender;
                else
                    name = sender.GetType().Name;

                m_derivedNameCache.Add(sender, name);
            }

            return name;
        }

        #endregion

        #region [ Remote Client Request Handlers ]

        // Get requested adapters collection
        protected virtual IAdapterCollection GetRequestedCollection(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.Exists("A"))
                return m_actionAdapters;
            else if (requestInfo.Request.Arguments.Exists("O"))
                return m_outputAdapters;
            else
                return m_inputAdapters;
        }

        // Get requested adapter
        protected virtual IAdapter GetRequestedAdapter(ClientRequestInfo requestInfo)
        {
            IAdapterCollection collection;
            return GetRequestedAdapter(requestInfo, out collection);
        }

        // Get requested adapter and its parent collection
        protected virtual IAdapter GetRequestedAdapter(ClientRequestInfo requestInfo, out IAdapterCollection collection)
        {
            IAdapter adapter;
            string adapterID = requestInfo.Request.Arguments["OrderedArg1"];
            collection = GetRequestedCollection(requestInfo);

            if (!string.IsNullOrEmpty(adapterID))
            {
                if (adapterID.IsAllNumbers())
                {
                    // Adapter ID is numeric, lookup by adapter ID
                    uint id = uint.Parse(adapterID);

                    // Try requested collection
                    if (collection.TryGetAdapterByID(id, out adapter))
                        return adapter;
                    // Try looking for ID in any collection if all runtime ID's are unique
                    else if (m_uniqueAdapterIDs && m_allAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
                        return adapter;
                    else
                    {
                        collection = GetRequestedCollection(requestInfo);
                        SendResponse(requestInfo, false, "Failed to find adapter with ID \"{0}\" in {1}.", id, collection.Name);
                    }
                }
                else
                {
                    // Adapter ID is alpha-numeric, lookup by adapter name
                    if (collection.TryGetAdapterByName(adapterID, out adapter))
                        return adapter;
                    else
                        SendResponse(requestInfo, false, "Failed to find adapter \"{0}\" in {1}.", adapterID, collection.Name);
                }
            }

            return null;
        }

        // List specified adapters
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

        // Start specified adapter
        protected virtual void StartRequestHandler(ClientRequestInfo requestInfo)
        {
            ActionRequestHandler(requestInfo, adapter => adapter.Start());
        }

        // Stop specified adapter
        protected virtual void StopRequestHandler(ClientRequestInfo requestInfo)
        {
            ActionRequestHandler(requestInfo, adapter => adapter.Stop());
        }

        // Abstract handler for adapter actions
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

        // Reflected invoke command request handler
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
                                            parameters[i] = parameterValue.ConvertToType(parameterInfo[i].ParameterType);
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
                                        SendResponseWithAttachment(requestInfo, returnValue, "Command \"{0}\" successfully invoked, return value = {1}", command, returnValue.ToNonNullString("null"));
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

        // Reflected list commands request handler
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
                        if (methods != null)
                        {
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

                                    if (!string.IsNullOrEmpty(commandAttribute.Description))
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
                        else
                            SendResponse(requestInfo, false, "Specified adapter \"{0}\" [Type = {1}] has no commands.", adapter.Name, adapter.GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(requestInfo, false, "Failed to list commands: {0}", ex.Message);
                        m_serviceHelper.ErrorLogger.Log(ex);
                    }
                }
            }
        }

        // Initialize specified adapter or collection of adapters
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
                    if (LoadSystemConfiguration())
                    {
                        // See if specific ID for an adapter was requested
                        if (requestInfo.Request.Arguments.Exists("OrderedArg1"))
                        {
                            string adapterID = requestInfo.Request.Arguments["OrderedArg1"];
                            uint id;

                            // Try initializing new adapter by ID searching in any collection if all runtime ID's are unique
                            if (m_uniqueAdapterIDs && uint.TryParse(adapterID, out id) && m_allAdapters.TryInitializeAdapterByID(id))
                            {
                                IAdapter adapter;

                                if (m_allAdapters.TryGetAnyAdapterByID(id, out adapter, out collection))
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
                                    SendResponse(requestInfo, false, "Requested adapter was not found.");
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
                                SendResponse(requestInfo, false, "Requested collection was unavailable.");
                        }
                    }
                    else
                        SendResponse(requestInfo, false, "Failed to load system configuration.");
                }
            }
        }

        // Reload system configuration 
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
                    SendResponse(requestInfo, false, "System configuration failed to reload.");
                else
                {
                    UpdateAdapterCollectionConfigurations();
                    SendResponse(requestInfo, true, "System configuration was successfully reloaded.");
                }
            }
        }

        // Update configuration file options
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

        // Attempts to authenticate (or reauthenticate) to network shares
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

        #endregion

        #region [ Broadcast Message Handling ]

        // Send actionable response to client
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success)
        {
            string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");

            // Send response to service
            m_serviceHelper.SendResponse(requestInfo.Sender.ClientID, new ServiceResponse(responseType));

            if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
            {
                string arguments = requestInfo.Request.Arguments.ToString();
                string message = responseType + (string.IsNullOrEmpty(arguments) ? "" : "(" + arguments + ")");
                m_serviceHelper.StatusLog.WriteTimestampedLine(message);
            }
        }

        // Send actionable response to client with message
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success, string status, params object[] args)
        {
            string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
            string message;

            if (args.Length == 0)
                message = status + "\r\n\r\n";
            else
                message = string.Format(status, args) + "\r\n\r\n";

            // Send response to service
            m_serviceHelper.SendResponse(requestInfo.Sender.ClientID, new ServiceResponse(responseType, message));

            if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
            {
                string arguments = requestInfo.Request.Arguments.ToString();
                message = responseType + (string.IsNullOrEmpty(arguments) ? "" : "(" + arguments + ")") + " - " + message;
                m_serviceHelper.StatusLog.WriteTimestampedLine(message);
            }
        }

        // Send actionable response to client with attachments (assumes successful response)
        protected virtual void SendResponseWithAttachment(ClientRequestInfo requestInfo, object attachment, string status, params object[] args)
        {
            string responseType = requestInfo.Request.Command + ":Success";
            string message;
            
            if (args.Length == 0)
                message = status + "\r\n\r\n";
            else
                message = string.Format(status, args) + "\r\n\r\n";

            ServiceResponse response = new ServiceResponse(responseType, message);

            // Add attachments to service response
            response.Attachments.Add(attachment);

            // Send response to service
            m_serviceHelper.SendResponse(requestInfo.Sender.ClientID, response);

            if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
            {
                string arguments = requestInfo.Request.Arguments.ToString();
                message = responseType + (string.IsNullOrEmpty(arguments) ? "" : "(" + arguments + ")") + " - " + message;
                m_serviceHelper.StatusLog.WriteTimestampedLine(message);
            }
        }

        // Display response message (send to request sender)
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, string.Format("{0}\r\n\r\n", status), args);
        }

        // Display status messages (broadcast to all clients)
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
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status due to an exception.\r\n\r\n");
            }
        }

        protected virtual void DisplayStatusMessage(string status, UpdateType type, params object[] args)
        {
            DisplayStatusMessage(string.Format(status, args), type);
        }

        #endregion

        #endregion
    }
}
