//******************************************************************************************************
//  TimeSeriesStartupOperations.cs - Gbtc
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
//  02/14/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSF.Data;
using GSF.Identity;

// ReSharper disable UnusedParameter.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
namespace GSF.TimeSeries
{
    /// <summary>
    /// Defines a data operations to be performed at startup.
    /// </summary>
    public static class TimeSeriesStartupOperations
    {
        // Messaging to the service
        private static Action<string> s_statusMessage;
        private static Action<Exception> s_processException;

        /// <summary>
        /// Delegates control to the data operations that are to be performed at startup.
        /// </summary>
        private static void PerformTimeSeriesStartupOperations(AdoDataConnection database, string nodeIDQueryString, ulong trackingVersion, string arguments, Action<string> statusMessage, Action<Exception> processException)
        {
            // Set up messaging to the service
            s_statusMessage = statusMessage;
            s_processException = processException;

            // Run data operations
            ValidateDefaultNode(database, nodeIDQueryString);
            ValidateAdapterCollections(database, nodeIDQueryString);
            ValidateActiveMeasurements(database, nodeIDQueryString);
            ValidateAccountsAndGroups(database, nodeIDQueryString);
            ValidateDataPublishers(database, nodeIDQueryString, arguments);
            ValidateStatistics(database, nodeIDQueryString);
            ValidateAlarming(database, nodeIDQueryString);
        }

        /// <summary>
        /// Data operation to validate and ensure there is a node in the database.
        /// </summary>
        private static void ValidateDefaultNode(AdoDataConnection database, string nodeIDQueryString)
        {
            // Queries
            const string NodeCountFormat = "SELECT COUNT(*) FROM Node";

            const string NodeInsertFormat = "INSERT INTO Node(Name, CompanyID, Description, Settings, MenuType, MenuData, Master, LoadOrder, Enabled) " +
                "VALUES('Default', NULL, 'Default node', 'RemoteStatusServerConnectionString={server=localhost:8500};datapublisherport=6165;RealTimeStatisticServiceUrl=http://localhost:6052/historian', " +
                "'File', 'Menu.xml', 1, 0, 1)";

            const string NodeUpdateFormat = "UPDATE Node SET ID = {0}";

            // Determine whether the node exists in the database and create it if it doesn't.
            int nodeCount = Convert.ToInt32(database.Connection.ExecuteScalar(NodeCountFormat));

            if (nodeCount == 0)
            {
                database.Connection.ExecuteNonQuery(NodeInsertFormat);
                database.Connection.ExecuteNonQuery(string.Format(NodeUpdateFormat, nodeIDQueryString));
            }
        }

        /// <summary>
        /// Data operation to validate and ensure there is a record in the
        /// ConfigurationEntity table for each of the adapter collections.
        /// </summary>
        private static void ValidateAdapterCollections(AdoDataConnection database, string nodeIDQueryString)
        {
            const string ConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = {0}";
            const string ConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES({0}, {1}, {2}, {3}, 1)";

            string[] sourceNames =
            {
                "IaonFilterAdapter",
                "IaonInputAdapter",
                "IaonActionAdapter",
                "IaonOutputAdapter"
            };

            string[] runtimeNames =
            {
                "FilterAdapters",
                "InputAdapters",
                "ActionAdapters",
                "OutputAdapters"
            };

            string[] descriptions =
            {
                "Defines IFilterAdapter definitions for a PDC node",
                "Defines IInputAdapter definitions for a PDC node",
                "Defines IActionAdapter definitions for a PDC node",
                "Defines IOutputAdapter definitions for a PDC node"
            };

            for (int i = 0; i < runtimeNames.Length; i++)
            {
                string sourceName = sourceNames[i];
                string runtimeName = runtimeNames[i];
                string description = descriptions[i];
                int loadOrder = i + 1;

                if (database.ExecuteScalar<int>(ConfigEntityCountFormat, runtimeName) == 0)
                    database.ExecuteNonQuery(ConfigEntityInsertFormat, sourceName, runtimeName, description, loadOrder);
            }
        }

        /// <summary>
        /// Data operation to validate and ensure there is a record
        /// in the ConfigurationEntity table for ActiveMeasurements.
        /// </summary>
        private static void ValidateActiveMeasurements(AdoDataConnection database, string nodeIDQueryString)
        {
            const string MeasurementConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'ActiveMeasurements'";
            const string MeasurementConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('ActiveMeasurement', 'ActiveMeasurements', 'Defines active system measurements for a TSF node', 4, 1)";

            int measurementConfigEntityCount = Convert.ToInt32(database.Connection.ExecuteScalar(MeasurementConfigEntityCountFormat));

            if (measurementConfigEntityCount == 0)
                database.Connection.ExecuteNonQuery(MeasurementConfigEntityInsertFormat);
        }

        /// <summary>
        /// Data operation to validate accounts and groups to ensure
        /// that account names and group names are converted to SIDs.
        /// </summary>
        private static void ValidateAccountsAndGroups(AdoDataConnection database, string nodeIDQueryString)
        {
            const string SelectUserAccountQuery = "SELECT ID, Name, UseADAuthentication FROM UserAccount";
            const string SelectSecurityGroupQuery = "SELECT ID, Name FROM SecurityGroup";
            const string UpdateUserAccountFormat = "UPDATE UserAccount SET Name = '{0}' WHERE ID = '{1}'";
            const string UpdateSecurityGroupFormat = "UPDATE SecurityGroup SET Name = '{0}' WHERE ID = '{1}'";

            string id;
            string sid;
            string accountName;
            Dictionary<string, string> updateMap;

            updateMap = new Dictionary<string, string>();

            // Find user accounts that need to be updated
            using (IDataReader userAccountReader = database.Connection.ExecuteReader(SelectUserAccountQuery))
            {
                while (userAccountReader.Read())
                {
                    id = userAccountReader["ID"].ToNonNullString();
                    accountName = userAccountReader["Name"].ToNonNullString();

                    if (userAccountReader["UseADAuthentication"].ToNonNullString().ParseBoolean())
                    {
                        sid = UserInfo.UserNameToSID(accountName);

                        if (!ReferenceEquals(accountName, sid) && UserInfo.IsUserSID(sid))
                            updateMap.Add(id, sid);
                    }
                }
            }

            // Update user accounts
            foreach (KeyValuePair<string, string> pair in updateMap)
                database.Connection.ExecuteNonQuery(string.Format(UpdateUserAccountFormat, pair.Value, pair.Key));

            updateMap.Clear();

            // Find security groups that need to be updated
            using (IDataReader securityGroupReader = database.Connection.ExecuteReader(SelectSecurityGroupQuery))
            {
                while (securityGroupReader.Read())
                {
                    id = securityGroupReader["ID"].ToNonNullString();
                    accountName = securityGroupReader["Name"].ToNonNullString();

                    if (accountName.Contains('\\'))
                    {
                        sid = UserInfo.GroupNameToSID(accountName);

                        if (!ReferenceEquals(accountName, sid) && UserInfo.IsGroupSID(sid))
                            updateMap.Add(id, sid);
                    }
                }
            }

            // Update security groups
            foreach (KeyValuePair<string, string> pair in updateMap)
                database.Connection.ExecuteNonQuery(string.Format(UpdateSecurityGroupFormat, pair.Value, pair.Key));
        }

        /// <summary>
        /// Data operation to validate and ensure there is a record in the
        /// CustomActionAdapter table for the external and TLS data publishers.
        /// </summary>
        private static void ValidateDataPublishers(AdoDataConnection database, string nodeIDQueryString, string arguments)
        {
            const string DataPublisherCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName='{0}!DATAPUBLISHER' AND NodeID = {1}";
            const string GEPDataPublisherInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, Enabled) VALUES({0}, '{1}!DATAPUBLISHER', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Transport.DataPublisher', 'securityMode={2}; allowSynchronizedSubscription=false; useBaseTimeOffsets=true; {3}', {4})";
            const string STTPDataPublisherInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, Enabled) VALUES({0}, '{1}!DATAPUBLISHER', 'sttp.gsf.dll', 'sttp.DataPublisher', 'securityMode={2}; {3}', {4})";

            bool internalDataPublisherEnabled = true;
            bool externalDataPublisherEnabled = true;
            bool tlsDataPublisherEnabled = true;
            bool sttpDataPublisherEnabled = true;
            bool sttpsDataPublisherEnabled = true;

            if (!string.IsNullOrEmpty(arguments))
            {
                Dictionary<string, string> kvps = arguments.ParseKeyValuePairs();
                string value;

                if (kvps.TryGetValue("internalDataPublisherEnabled", out value))
                    internalDataPublisherEnabled = value.ParseBoolean();

                if (kvps.TryGetValue("externalDataPublisherEnabled", out value))
                    externalDataPublisherEnabled = value.ParseBoolean();

                if (kvps.TryGetValue("tlsDataPublisherEnabled", out value))
                    tlsDataPublisherEnabled = value.ParseBoolean();

                if (kvps.TryGetValue("sttpDataPublisherEnabled", out value))
                    sttpDataPublisherEnabled = value.ParseBoolean();
                
                if (kvps.TryGetValue("sttpsDataPublisherEnabled", out value))
                    sttpsDataPublisherEnabled = value.ParseBoolean();
            }

            int internalDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "INTERNAL", nodeIDQueryString)));
            int externalDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "EXTERNAL", nodeIDQueryString)));
            int tlsDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "TLS", nodeIDQueryString)));
            int sttpDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "STTP", nodeIDQueryString)));
            int sttpsDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "STTPS", nodeIDQueryString)));

            if (internalDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(GEPDataPublisherInsertFormat, nodeIDQueryString, "INTERNAL", "None", "cacheMeasurementKeys={FILTER ActiveMeasurements WHERE SignalType = ''STAT''}", internalDataPublisherEnabled ? 1 : 0));

            if (externalDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(GEPDataPublisherInsertFormat, nodeIDQueryString, "EXTERNAL", "Gateway", "", externalDataPublisherEnabled ? 1 : 0));

            if (tlsDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(GEPDataPublisherInsertFormat, nodeIDQueryString, "TLS", "TLS", "", tlsDataPublisherEnabled ? 1 : 0));

            if (sttpDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(STTPDataPublisherInsertFormat, nodeIDQueryString, "STTP", "None", "cachedMeasurementExpression={FILTER ActiveMeasurements WHERE SignalType = ''STAT''}", sttpDataPublisherEnabled ? 1 : 0));

            if (sttpsDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(STTPDataPublisherInsertFormat, nodeIDQueryString, "STTPS", "TLS", "", sttpsDataPublisherEnabled ? 1 : 0));
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records that
        /// are required for statistics calculations exist in the database.
        /// </summary>
        private static void ValidateStatistics(AdoDataConnection database, string nodeIDQueryString)
        {
            // SELECT queries
            const string StatConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'Statistics'";
            const string StatSignalTypeCountFormat = "SELECT COUNT(*) FROM SignalType WHERE Acronym = 'STAT'";

            const string StatHistorianCountFormat = "SELECT COUNT(*) FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
            const string StatEngineCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName = 'STATISTIC!SERVICES' AND NodeID = {0}";
            const string SystemStatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'System' AND AssemblyName = 'GSF.TimeSeries.dll'";
            const string DeviceStatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'Device' AND AssemblyName = 'GSF.TimeSeries.dll'";
            const string SubscriberStatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'Subscriber' AND AssemblyName = 'GSF.TimeSeries.dll'";
            const string PublisherStatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'Publisher' AND AssemblyName = 'GSF.TimeSeries.dll'";
            const string ProcessStatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'Process' AND AssemblyName = 'FileAdapters.dll'";

            // INSERT queries
            const string StatConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('RuntimeStatistic', 'Statistics', 'Defines statistics that are monitored for the system, devices, and output streams', 11, 1)";
            const string StatSignalTypeInsertFormat = "INSERT INTO SignalType(Name, Acronym, Suffix, Abbreviation, Source, EngineeringUnits) VALUES('Statistic', 'STAT', 'ST', 'P', 'Any', '')";

            const string StatHistorianInsertFormat = "INSERT INTO Historian(NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, IsLocal, Description, LoadOrder, Enabled) VALUES({0}, 'STAT', 'Statistics Archive', 'TestingAdapters.dll', 'TestingAdapters.VirtualOutputAdapter', '', 1, 'Local historian used to archive system statistics', 9999, 1)";
            const string StatEngineInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, LoadOrder, Enabled) VALUES({0}, 'STATISTIC!SERVICES', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.StatisticsEngine', 0, 1)";
            const string SystemStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('System', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.PerformanceStatistics', 'GetSystemStatistic_{3}', '', 1, 'System.Double', '{{0:N3}}', 0, {0})";
            const string DeviceStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Device', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.DeviceStatistics', 'GetDeviceStatistic_{3}', '', 1, 'System.Int32', '{{0:N0}}', 0, {0})";
            const string SubscriberStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Subscriber', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.GatewayStatistics', 'GetSubscriberStatistic_{3}', '', 1, '{4}', '{5}', {6}, {0})";
            const string PublisherStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Publisher', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.GatewayStatistics', 'GetPublisherStatistic_{3}', '', 1, '{4}', '{5}', {6}, {0})";
            const string ProcessStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Process', {0}, '{1}', '{2}', 'FileAdapters.dll', 'FileAdapters.ProcessLauncher', 'GetProcessStatistic_{3}', '', 1, 'System.Double', '{{0:N3}}', 0, {0})";

            // DELETE queries
            const string SystemStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'System' AND SignalIndex <= {0}";
            const string DeviceStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Device' AND SignalIndex <= {0}";
            const string SubscriberStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Subscriber' AND SignalIndex <= {0}";
            const string PublisherStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Publisher' AND SignalIndex <= {0}";
            const string ProcessStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Process' AND SignalIndex <= {0}";

            // Names and descriptions for each of the statistics

            // NOTE: !! The statistic names defined in the following array are used to define associated function names (minus spaces) - as a result, do *not* leisurely change these statistic names without understanding the consequences
            string[] SystemStatNames = { "CPU Usage", "Average CPU Usage", "Memory Usage", "Average Memory Usage", "Thread Count", "Average Thread Count", "Threading Contention Rate", "Average Threading Contention Rate", "IO Usage", "Average IO Usage", "IP Data Send Rate", "Average IP Data Send Rate", "IP Data Receive Rate", "Average IP Data Receive Rate", "Up Time" };

            string[] SystemStatDescriptions = { "Percentage of CPU currently used by this process.",
                                                "Average percentage of CPU used by this process.",
                                                "Amount of memory currently used by this process in megabytes.",
                                                "Average amount of memory used by this process in megabytes.",
                                                "Number of threads currently used by this process.",
                                                "Average number of threads used by this process.",
                                                "Current thread lock contention rate in attempts per second.",
                                                "Average thread lock contention rate in attempts per second.",
                                                "Amount of IO currently used by this process in kilobytes per second.",
                                                "Average amount of IO used by this process in kilobytes per second.",
                                                "Number of IP datagrams (or bytes on Mono) currently sent by this process per second.",
                                                "Average number of IP datagrams (or bytes on Mono) sent by this process per second.",
                                                "Number of IP datagrams (or bytes on Mono) currently received by this process per second.",
                                                "Average number of IP datagrams (or bytes on Mono) received by this process per second.",
                                                "Total number of seconds system has been running."
                                              };

            // NOTE: !! The statistic names defined in the following array are used to define associated function names (minus spaces) - as a result, do *not* leisurely change these statistic names without understanding the consequences
            string[] DeviceStatNames = { "Data Quality Errors", "Time Quality Errors", "Device Errors", "Measurements Received", "Measurements Expected", "Measurements With Error", "Measurements Defined" };

            string[] DeviceStatDescriptions = { "Number of data quality errors reported by device during last reporting interval.",
                                                "Number of time quality errors reported by device during last reporting interval.",
                                                "Number of device errors reported by device during last reporting interval.",
                                                "Number of measurements received from device during last reporting interval.",
                                                "Expected number of measurements received from device during last reporting interval.",
                                                "Number of measurements received while device was reporting errors during last reporting interval.",
                                                "Number of defined measurements from device during last reporting interval."
                                              };

            string[] SubscriberStatNames = { "Subscriber Connected", "Subscriber Authenticated", "Processed Measurements", "Total Bytes Received", "Authorized Signal Count", "Unauthorized Signal Count", "Lifetime Measurements", "Lifetime Bytes Received", "Minimum Measurements Per Second", "Maximum Measurements Per Second", "Average Measurements Per Second", "Lifetime Minimum Latency", "Lifetime Maximum Latency", "Lifetime Average Latency", "Up Time" };

            string[] SubscriberStatDescriptions = { "Boolean value representing if the subscriber was continually connected during last reporting interval.",
                                                    "Boolean value representing if the subscriber was authenticated to the publisher during last reporting interval.",
                                                    "Number of processed measurements reported by the subscriber during last reporting interval.",
                                                    "Number of bytes received from subscriber during last reporting interval.",
                                                    "Number of signals authorized to the subscriber by the publisher.",
                                                    "Number of signals denied to the subscriber by the publisher.",
                                                    "Number of processed measurements reported by the subscriber during the lifetime of the subscriber.",
                                                    "Number of bytes received from subscriber during the lifetime of the subscriber.",
                                                    "The minimum number of measurements received per second during the last reporting interval.",
                                                    "The maximum number of measurements received per second during the last reporting interval.",
                                                    "The average number of measurements received per second during the last reporting interval.",
                                                    "Minimum latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                                                    "Maximum latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                                                    "Average latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                                                    "Total number of seconds subscriber has been running."
                                                  };

            string[] SubscriberStatMethodSuffix = { "Connected", "Authenticated", "ProcessedMeasurements", "TotalBytesReceived", "AuthorizedCount", "UnauthorizedCount", "LifetimeMeasurements", "LifetimeBytesReceived", "MinimumMeasurementsPerSecond", "MaximumMeasurementsPerSecond", "AverageMeasurementsPerSecond", "LifetimeMinimumLatency", "LifetimeMaximumLatency", "LifetimeAverageLatency", "UpTime" };
            string[] SubscriberStatTypes = { "System.Boolean", "System.Boolean", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int64", "System.Int64", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Double" };
            string[] SubscriberStatFormats = { "{0}", "{0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0} ms", "{0:N0} ms", "{0:N0} ms", "{0:N3} s" };

            string[] PublisherStatNames = { "Publisher Connected", "Connected Clients", "Processed Measurements", "Total Bytes Sent", "Lifetime Measurements", "Lifetime Bytes Sent", "Minimum Measurements Per Second", "Maximum Measurements Per Second", "Average Measurements Per Second", "Lifetime Minimum Latency", "Lifetime Maximum Latency", "Lifetime Average Latency", "Up Time" };

            string[] PublisherStatDescriptions = { "Boolean value representing if the publisher was continually connected during last reporting interval.",
                                                   "Number of clients connected to the command channel of the publisher during last reporting interval.",
                                                   "Number of processed measurements reported by the publisher during last reporting interval.",
                                                   "Number of bytes sent by the publisher during the last reporting interval.",
                                                   "Number of processed measurements reported by the publisher during the lifetime of the publisher.",
                                                   "Number of bytes sent by the publisher during the lifetime of the publisher.",
                                                   "The minimum number of measurements sent per second during the last reporting interval.",
                                                   "The maximum number of measurements sent per second during the last reporting interval.",
                                                   "The average number of measurements sent per second during the last reporting interval.",
                                                   "Minimum latency from output stream, in milliseconds, during the lifetime of the publisher.",
                                                   "Maximum latency from output stream, in milliseconds, during the lifetime of the publisher.",
                                                   "Average latency from output stream, in milliseconds, during the lifetime of the publisher.",
                                                   "Total number of seconds publisher has been running."
                                                  };

            string[] PublisherStatMethodSuffix = { "Connected", "ConnectedClientCount", "ProcessedMeasurements", "TotalBytesSent", "LifetimeMeasurements", "LifetimeBytesSent", "MinimumMeasurementsPerSecond", "MaximumMeasurementsPerSecond", "AverageMeasurementsPerSecond", "LifetimeMinimumLatency", "LifetimeMaximumLatency", "LifetimeAverageLatency", "UpTime" };
            string[] PublisherStatTypes = { "System.Boolean", "System.Int32", "System.Int32", "System.Int32", "System.Int64", "System.Int64", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Double" };
            string[] PublisherStatFormats = { "{0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0} ms", "{0:N0} ms", "{0:N0} ms", "{0:N3} s" };

            // NOTE: !! The statistic names defined in the following array are used to define associated function names (minus spaces) - as a result, do *not* leisurely change these statistic names without understanding the consequences
            string[] ProcessStatNames = { "CPU Usage", "Memory Usage", "Up Time" };

            string[] ProcessStatDescriptions = { "Percentage of CPU currently used by the launched process.",
                                                "Amount of memory currently used by the launched process in megabytes.",
                                                "Total number of seconds the launched process has been running."
                                               };

            // Query for count values to ensure existence of these records
            int statConfigEntityCount = Convert.ToInt32(database.Connection.ExecuteScalar(StatConfigEntityCountFormat));
            int statSignalTypeCount = Convert.ToInt32(database.Connection.ExecuteScalar(StatSignalTypeCountFormat));

            int statHistorianCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatHistorianCountFormat, nodeIDQueryString)));
            int statEngineCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatEngineCountFormat, nodeIDQueryString)));
            int systemStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(SystemStatCountFormat));
            int deviceStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(DeviceStatCountFormat));
            int subscriberStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(SubscriberStatCountFormat));
            int publisherStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(PublisherStatCountFormat));
            int processStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(ProcessStatCountFormat));

            // Statistic info for inserting statistics
            int signalIndex;
            string statName;
            string statDescription;
            string statMethodSuffix;
            string statType;
            string statFormat;

            // Ensure that STAT signal type exists
            if (statSignalTypeCount == 0)
                database.Connection.ExecuteNonQuery(StatSignalTypeInsertFormat);

            // Ensure that statistics configuration entity record exists
            if (statConfigEntityCount == 0)
                database.Connection.ExecuteNonQuery(StatConfigEntityInsertFormat);

            // Ensure that statistics historian exists
            if (statHistorianCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(StatHistorianInsertFormat, nodeIDQueryString));

            // Ensure that statistics engine exists
            if (statEngineCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(StatEngineInsertFormat, nodeIDQueryString));

            // Ensure that system statistics exist
            if (systemStatCount < SystemStatNames.Length)
            {
                database.Connection.ExecuteNonQuery(string.Format(SystemStatisticDeleteFormat, SystemStatNames.Length));

                for (int i = 0; i < SystemStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = SystemStatNames[i];
                    statDescription = SystemStatDescriptions[i];
                    statMethodSuffix = statName.Replace(" ", "");
                    database.Connection.ExecuteNonQuery(string.Format(SystemStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix));
                }
            }

            // Ensure that system statistics exist
            if (deviceStatCount < DeviceStatNames.Length)
            {
                database.Connection.ExecuteNonQuery(string.Format(DeviceStatisticDeleteFormat, DeviceStatNames.Length));

                for (int i = 0; i < DeviceStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = DeviceStatNames[i];
                    statDescription = DeviceStatDescriptions[i];
                    statMethodSuffix = statName.Replace(" ", "");
                    database.Connection.ExecuteNonQuery(string.Format(DeviceStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix));
                }
            }

            // Ensure that subscriber statistics exist
            if (subscriberStatCount < SubscriberStatNames.Length)
            {
                database.Connection.ExecuteNonQuery(string.Format(SubscriberStatisticDeleteFormat, SubscriberStatNames.Length));

                for (int i = 0; i < SubscriberStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = SubscriberStatNames[i];
                    statDescription = SubscriberStatDescriptions[i];
                    statMethodSuffix = SubscriberStatMethodSuffix[i];
                    statType = SubscriberStatTypes[i];
                    statFormat = SubscriberStatFormats[i];
                    database.Connection.ExecuteNonQuery(string.Format(SubscriberStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat, signalIndex == 1 ? 1 : 0));
                }
            }

            // Ensure that publisher statistics exist
            if (publisherStatCount < PublisherStatNames.Length)
            {
                database.Connection.ExecuteNonQuery(string.Format(PublisherStatisticDeleteFormat, PublisherStatNames.Length));

                for (int i = 0; i < PublisherStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = PublisherStatNames[i];
                    statDescription = PublisherStatDescriptions[i];
                    statMethodSuffix = PublisherStatMethodSuffix[i];
                    statType = PublisherStatTypes[i];
                    statFormat = PublisherStatFormats[i];
                    database.Connection.ExecuteNonQuery(string.Format(PublisherStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat, signalIndex == 1 ? 1 : 0));
                }
            }

            // Ensure that process statistics exist
            if (processStatCount < ProcessStatNames.Length)
            {
                database.Connection.ExecuteNonQuery(string.Format(ProcessStatisticDeleteFormat, ProcessStatNames.Length));

                for (int i = 0; i < ProcessStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = ProcessStatNames[i];
                    statDescription = ProcessStatDescriptions[i];
                    statMethodSuffix = statName.Replace(" ", "");
                    database.Connection.ExecuteNonQuery(string.Format(ProcessStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix));
                }
            }
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records
        /// that are required for alarming exist in the database.
        /// </summary>
        private static void ValidateAlarming(AdoDataConnection connection, string nodeIDQueryString)
        {
            // SELECT queries
            const string AlarmCountFormat = "SELECT COUNT(*) FROM Alarm";
            const string AlarmAdapterCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName = 'ALARM!SERVICES' AND NodeID = {0}";
            const string AlarmConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'Alarms'";
            const string AlarmSignalTypeCountFormat = "SELECT COUNT(*) FROM SignalType WHERE Name = 'Alarm'";

            // INSERT queries
            const string AlarmAdapterInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, LoadOrder, Enabled) VALUES({0}, 'ALARM!SERVICES', 'DataQualityMonitoring.dll', 'DataQualityMonitoring.AlarmAdapter', 'useAlarmLog=false', 0, 1)";
            const string AlarmConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('Alarm', 'Alarms', 'Defines alarms that monitor the values of measurements', 17, 1)";
            const string AlarmSignalTypeInsertFormat = "INSERT INTO SignalType(Name, Acronym, Suffix, Abbreviation, Source, EngineeringUnits) VALUES('Alarm', 'ALRM', 'AL', 'AL', 'Any', '')";

            bool alarmTableExists;

            Guid nodeID;
            int alarmAdapterCount;
            int alarmConfigEntityCount;
            int alarmSignalTypeCount;

            try
            {
                // Determine whether the alarm table exists
                // before inserting records related to alarming
                connection.Connection.ExecuteScalar(AlarmCountFormat);
                alarmTableExists = true;
            }
            catch
            {
                alarmTableExists = false;
            }

            if (alarmTableExists)
            {
                nodeID = Guid.Parse(nodeIDQueryString.Trim('\''));

                // Ensure that the alarm adapter is defined.
                alarmAdapterCount = connection.ExecuteScalar<int>(AlarmAdapterCountFormat, nodeID);

                if (alarmAdapterCount == 0)
                    connection.ExecuteNonQuery(AlarmAdapterInsertFormat, nodeID);

                // Ensure that the alarm record is defined in the ConfigurationEntity table.
                alarmConfigEntityCount = connection.ExecuteScalar<int>(AlarmConfigEntityCountFormat);

                if (alarmConfigEntityCount == 0)
                    connection.ExecuteNonQuery(AlarmConfigEntityInsertFormat);

                // Ensure that the alarm record is defined in the SignalType table.
                alarmSignalTypeCount = connection.ExecuteScalar<int>(AlarmSignalTypeCountFormat);

                if (alarmSignalTypeCount == 0)
                    connection.ExecuteNonQuery(AlarmSignalTypeInsertFormat);

                ValidateAlarmStatistics(connection, nodeID, "Point");
            }
        }

        private static void ValidateAlarmStatistics(AdoDataConnection connection, Guid nodeID, string source)
        {
            const string MissingStatisticsFormat = "SELECT DISTINCT Severity FROM Alarm WHERE Severity <> 0 AND Severity NOT IN (SELECT Arguments FROM Statistic WHERE Source = {0} AND MethodName = {1})";
            const string MaxSignalIndexFormat = "SELECT COALESCE(MAX(SignalIndex), 0) FROM Statistic WHERE Source = {0}";
            const string InsertAlarmStatisticFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})";

            string methodName;

            DataTable missingStatistics;

            int signalIndex;
            int severity;
            string name;
            string description;

            // Add statistics for the alarms defined in the Alarm table.
            methodName = $"Get{source}Statistic_MeasurementCountForSeverity";
            missingStatistics = connection.RetrieveData(MissingStatisticsFormat, source, methodName);

            if (missingStatistics.Rows.Count > 0)
            {
                signalIndex = connection.ExecuteScalar<int>(MaxSignalIndexFormat, source);

                foreach (DataRow missingStatistic in missingStatistics.Rows)
                {
                    signalIndex++;
                    severity = missingStatistic.ConvertField<int>("Severity");
                    name = $"Alarm Severity {severity}";
                    description = $"Number of measurements received while alarm with severity {severity} was raised during the last reporting interval.";

                    connection.ExecuteNonQuery(InsertAlarmStatisticFormat, source, signalIndex, name, description, "DataQualityMonitoring.dll", "DataQualityMonitoring.AlarmStatistics", methodName, severity, 1, "System.Int32", "{0:N0}", 0, 1001 - severity);
                }
            }
        }
    }
}
