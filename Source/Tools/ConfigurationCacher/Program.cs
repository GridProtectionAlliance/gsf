//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  02/07/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Units;
using SerializationFormat = GSF.SerializationFormat;

namespace ConfigurationCacher
{
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
        /// Configuration source is a binary file.
        /// </summary>
        BinaryFile,
        /// <summary>
        /// Configuration source is a XML file.
        /// </summary>
        XmlFile
    }

    public class Program
    {
        private static Guid s_nodeID;
        private static ConfigurationType s_configurationType;
        private static string s_connectionString;
        private static string s_dataProviderString;
        private static string s_cachedBinaryConfigurationFile;
        private static string s_cachedXmlConfigurationFile;
        private static int s_configurationBackups;

        public static void Main(string[] args)
        {
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Open(GetConfigurationFileName(args)).Settings["systemSettings"];
            string cachePath = string.Format("{0}{1}ConfigurationCache{1}", FilePath.GetAbsolutePath(""), Path.DirectorySeparatorChar);
            DataSet configuration;

            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");
            systemSettings.Add("ConfigurationType", "Database", "Specifies type of configuration: Database, WebService, BinaryFile or XmlFile");
            systemSettings.Add("ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=IaonHost.mdb", "Configuration database connection string");
            systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Configuration database ADO.NET data provider assembly type creation string");
            systemSettings.Add("ConfigurationCachePath", cachePath, "Defines the path used to cache serialized configurations");
            systemSettings.Add("CachedConfigurationFile", "SystemConfiguration.xml", "File name for last known good system configuration (only cached for a Database or WebService connection)");
            systemSettings.Add("ConfigurationBackups", 5, "Defines the total number of older backup configurations to maintain.");

            s_nodeID = systemSettings["NodeID"].ValueAs<Guid>();
            s_configurationType = systemSettings["ConfigurationType"].ValueAs<ConfigurationType>();
            s_connectionString = systemSettings["ConnectionString"].Value;
            s_dataProviderString = systemSettings["DataProviderString"].Value;
            s_cachedXmlConfigurationFile = FilePath.AddPathSuffix(cachePath) + systemSettings["CachedConfigurationFile"].Value;
            s_cachedBinaryConfigurationFile = FilePath.AddPathSuffix(cachePath) + FilePath.GetFileNameWithoutExtension(s_cachedXmlConfigurationFile) + ".bin";
            s_configurationBackups = systemSettings["ConfigurationBackups"].ValueAs(5);

            configuration = GetConfigurationDataSet();
            ExecuteConfigurationCache(configuration);

            Console.WriteLine("Press 'Enter' to exit...");
            Console.ReadLine();
        }

        private static string GetConfigurationFileName(string[] args)
        {
            string[] knownConfigurationFileNames = { "openPDC.exe.config", "openPG.exe.config", "openHistorian.exe.config", "SIEGate.exe.config" };
            string absolutePath = "Configuration file";

            // Search for the file name in the arguments to the program
            foreach (string arg in args)
            {
                absolutePath = FilePath.GetAbsolutePath(arg);

                if (File.Exists(absolutePath))
                    return absolutePath;
            }

            // Search for the file name in the list of known configuration files
            foreach (string fileName in knownConfigurationFileNames)
            {
                absolutePath = FilePath.GetAbsolutePath(fileName);

                if (File.Exists(absolutePath))
                    return absolutePath;
            }

            // Prompt the user for the file name
            Console.WriteLine("Unable to find a configuration file. Please enter a file name:");

            while (true)
            {
                absolutePath = FilePath.GetAbsolutePath(Console.ReadLine());

                if (!File.Exists(absolutePath))
                    Console.WriteLine("Unable to find {0}. Please enter a file name:", FilePath.GetFileName(absolutePath));
                else
                    break;
            }

            return absolutePath;
        }

        // Load system configuration data set
        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        private static DataSet GetConfigurationDataSet()
        {
            DataSet configuration = null;
            bool configException = false;
            Ticks startTime = DateTime.UtcNow.Ticks;
            Time elapsedTime;
            string nodeIDQueryString = null;

            switch (s_configurationType)
            {
                case ConfigurationType.Database:
                    // Attempt to load configuration from a database connection
                    IDbConnection connection = null;
                    Dictionary<string, string> settings;
                    string assemblyName, connectionTypeName, adapterTypeName;
                    string setting;
                    Assembly assembly;
                    Type connectionType, adapterType;
                    DataTable entities, source, destination;

                    try
                    {
                        settings = s_dataProviderString.ParseKeyValuePairs();
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
                        connection.ConnectionString = s_connectionString;
                        connection.Open();

                        configuration = new DataSet("Iaon");

                        // Load configuration entities defined in database
                        entities = connection.RetrieveData(adapterType, "SELECT * FROM ConfigurationEntity WHERE Enabled <> 0 ORDER BY LoadOrder");
                        entities.TableName = "ConfigurationEntity";

                        // Add configuration entities table to system configuration for reference
                        configuration.Tables.Add(entities.Copy());

                        // Get the node ID query string
                        if (settings.TryGetValue("Provider", out setting))
                        {
                            // Check if provider is for Access since it uses braces as Guid delimiters
                            if (setting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                            {
                                nodeIDQueryString = "{" + s_nodeID + "}";

                                // Make sure path to Access database is fully qualified
                                if (settings.TryGetValue("Data Source", out setting))
                                {
                                    settings["Data Source"] = FilePath.GetAbsolutePath(setting);
                                    s_connectionString = settings.JoinKeyValuePairs();
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(nodeIDQueryString))
                            nodeIDQueryString = "'" + s_nodeID + "'";

                        Ticks operationStartTime;
                        Time operationElapsedTime;

                        // Add each configuration entity to the system configuration
                        foreach (DataRow entityRow in entities.Rows)
                        {
                            // Load configuration entity data filtered by node ID
                            operationStartTime = DateTime.UtcNow.Ticks;
                            source = connection.RetrieveData(adapterType, string.Format("SELECT * FROM {0} WHERE NodeID={1}", entityRow["SourceName"], nodeIDQueryString));
                            operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                            // Update table name as defined in configuration entity
                            source.TableName = entityRow["RuntimeName"].ToString();

                            DisplayStatusMessage("Loaded {0} row{1} from \"{2}\" in {3}...", UpdateType.Information, source.Rows.Count, source.Rows.Count == 1 ? "" : "s", source.TableName, operationElapsedTime.ToString(3));

                            operationStartTime = DateTime.UtcNow.Ticks;

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

                            operationElapsedTime = (DateTime.UtcNow.Ticks - operationStartTime).ToSeconds();

                            // Add entity configuration data to system configuration
                            configuration.Tables.Add(destination);

                            DisplayStatusMessage("{0} configuration pre-cache completed in {1}.", UpdateType.Information, source.TableName, operationElapsedTime.ToString(3));
                        }

                        DisplayStatusMessage("Database configuration successfully loaded.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        configException = true;
                        DisplayStatusMessage("Failed to load database configuration due to exception: {0}", UpdateType.Warning, ex.Message);
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
                        request = WebRequest.Create(s_connectionString);
                        response = request.GetResponse().GetResponseStream();
                        configuration.ReadXml(response);

                        DisplayStatusMessage("Webservice configuration successfully loaded.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        configException = true;
                        DisplayStatusMessage("Failed to load webservice configuration due to exception: {0}", UpdateType.Warning, ex.Message);
                    }
                    finally
                    {
                        if (response != null)
                            response.Dispose();

                        DisplayStatusMessage("Webservice configuration connection closed.", UpdateType.Information);
                    }

                    break;
                case ConfigurationType.BinaryFile:
                    // Attempt to load cached binary configuration file
                    try
                    {
                        DisplayStatusMessage("Loading binary based configuration from \"{0}\".", UpdateType.Information, s_connectionString);

                        configuration = Serialization.Deserialize<DataSet>(File.OpenRead(s_connectionString), SerializationFormat.Binary);

                        DisplayStatusMessage("Binary based configuration successfully loaded.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        configException = true;
                        DisplayStatusMessage("Failed to load binary based configuration due to exception: {0}.", UpdateType.Alarm, ex.Message);
                    }

                    break;
                case ConfigurationType.XmlFile:
                    // Attempt to load cached XML configuration file
                    try
                    {
                        DisplayStatusMessage("Loading XML based configuration from \"{0}\".", UpdateType.Information, s_connectionString);

                        configuration = new DataSet();
                        configuration.ReadXml(s_connectionString);

                        DisplayStatusMessage("XML based configuration successfully loaded.", UpdateType.Information);
                    }
                    catch (Exception ex)
                    {
                        configException = true;
                        DisplayStatusMessage("Failed to load XML based configuration due to exception: {0}.", UpdateType.Alarm, ex.Message);
                    }

                    break;
            }

            if (!configException)
            {
                elapsedTime = (DateTime.UtcNow.Ticks - startTime).ToSeconds();
                DisplayStatusMessage("{0} configuration load process completed in {1}...", UpdateType.Information, s_configurationType, elapsedTime.ToString(3));
            }

            return configuration;
        }

        // Executes actual serialization of current configuration
        private static void ExecuteConfigurationCache(DataSet configuration)
        {
            if ((object)configuration != null)
            {
                // Create backups of binary configurations
                BackupConfiguration(ConfigurationType.BinaryFile, s_cachedBinaryConfigurationFile);

                // Create backups of XML configurations
                BackupConfiguration(ConfigurationType.XmlFile, s_cachedXmlConfigurationFile);

                try
                {
                    // Wait a moment for write lock in case binary file is open by another process
                    if (File.Exists(s_cachedBinaryConfigurationFile))
                        FilePath.WaitForWriteLock(s_cachedBinaryConfigurationFile);

                    // Cache binary serialized version of data set
                    using (FileStream configurationFileStream = File.OpenWrite(s_cachedBinaryConfigurationFile))
                    {
                        configuration.SerializeToStream(configurationFileStream);
                    }

                    DisplayStatusMessage("Successfully cached current configuration to binary.", UpdateType.Information);
                }
                catch (Exception ex)
                {
                    DisplayStatusMessage("Failed to cache last known configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                }

                // Serialize current data set to configuration files
                try
                {

                    // Wait a moment for write lock in case XML file is open by another process
                    if (File.Exists(s_cachedXmlConfigurationFile))
                        FilePath.WaitForWriteLock(s_cachedXmlConfigurationFile);

                    // Cache XML serialized version of data set
                    configuration.WriteXml(s_cachedXmlConfigurationFile, XmlWriteMode.WriteSchema);

                    DisplayStatusMessage("Successfully cached current configuration to XML.", UpdateType.Information);
                }
                catch (Exception ex)
                {
                    DisplayStatusMessage("Failed to cache last known configuration due to exception: {0}", UpdateType.Alarm, ex.Message);
                }
            }
        }

        private static void BackupConfiguration(ConfigurationType configType, string configurationFile)
        {
            try
            {
                // Create multiple backup configurations, if requested
                for (int i = s_configurationBackups; i > 0; i--)
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
            }

            try
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
            catch (Exception ex)
            {
                DisplayStatusMessage("Failed to backup last known cached {0} configuration due to exception: {1}", UpdateType.Warning, configType, ex.Message);
            }
        }

        private static void DisplayStatusMessage(string message, UpdateType updateType, params object[] args)
        {
            TextWriter writer = (updateType == UpdateType.Alarm) ? Console.Error : Console.Out;

            switch (updateType)
            {
                case UpdateType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case UpdateType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case UpdateType.Alarm:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            writer.WriteLine(message, args);
        }
    }
}
