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

            Dictionary<string, string> updateMap = new Dictionary<string, string>();

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

                if (kvps.TryGetValue("internalDataPublisherEnabled", out string value))
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
            const string SystemStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('System', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.PerformanceStatistics', 'GetSystemStatistic_{3}', '', 1, '{4}', '{5}', 0, {0})";
            const string DeviceStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Device', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.DeviceStatistics', 'GetDeviceStatistic_{3}', '', 1, '{4}', '{5}', 0, {0})";
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
            string[] SystemStatNames =
            {
                /* 01 */ "CPU Usage", 
                /* 02 */ "Average CPU Usage", 
                /* 03 */ "Memory Usage", 
                /* 04 */ "Average Memory Usage", 
                /* 05 */ "Thread Count", 
                /* 06 */ "Average Thread Count", 
                /* 07 */ "Threading Contention Rate", 
                /* 08 */ "Average Threading Contention Rate", 
                /* 09 */ "IO Usage", 
                /* 10 */ "Average IO Usage", 
                /* 11 */ "IP Data Send Rate", 
                /* 12 */ "Average IP Data Send Rate", 
                /* 13 */ "IP Data Receive Rate", 
                /* 14 */ "Average IP Data Receive Rate", 
                /* 15 */ "Up Time",
                /* 16 */ "System CPU Usage",
                /* 17 */ "Average System CPU Usage",
                /* 18 */ "Available System Memory",
                /* 19 */ "Average Available System Memory",
                /* 20 */ "System Memory Usage",
                /* 21 */ "Average Device Time",
                /* 22 */ "Minimum Device Time",
                /* 23 */ "Maximum Device Time",
                /* 24 */ "System Time Deviation From Average",
                /* 25 */ "Primary Disk Usage"
            };

            string[] SystemStatDescriptions = 
            {
                /* 01 */ "Percentage of CPU currently used by this process.",
                /* 02 */ "Average percentage of CPU used by this process.",
                /* 03 */ "Amount of memory currently used by this process in megabytes.",
                /* 04 */ "Average amount of memory used by this process in megabytes.",
                /* 05 */ "Number of threads currently used by this process.",
                /* 06 */ "Average number of threads used by this process.",
                /* 07 */ "Current thread lock contention rate in attempts per second.",
                /* 08 */ "Average thread lock contention rate in attempts per second.",
                /* 09 */ "Amount of IO currently used by this process in kilobytes per second.",
                /* 10 */ "Average amount of IO used by this process in kilobytes per second.",
                /* 11 */ "Number of IP datagrams (or bytes on Mono) currently sent by this process per second.",
                /* 12 */ "Average number of IP datagrams (or bytes on Mono) sent by this process per second.",
                /* 13 */ "Number of IP datagrams (or bytes on Mono) currently received by this process per second.",
                /* 14 */ "Average number of IP datagrams (or bytes on Mono) received by this process per second.",
                /* 15 */ "Total number of seconds system has been running.",
                /* 16 */ "Percentage of total CPU currently used by the host system.",
                /* 17 */ "Average percentage of total CPU used by the host system.",
                /* 18 */ "Amount of memory available on the host system in gigabytes",
                /* 19 */ "Average amount of memory available on the host system in gigabytes",
                /* 20 */ "Percentage of memory currently used on the host system",
                /* 21 */ "Average time for all input devices",
                /* 22 */ "Minimum time for all input devices",
                /* 23 */ "Maximum time for all input devices",
                /* 24 */ "System time deviation from average for all input devices in seconds",
                /* 25 */ "Percentage of primary disk (C: or /) usage on the host system"
            };

            string[] SystemStatTypes =
            {
                /* 01 */ "System.Double",
                /* 02 */ "System.Double",
                /* 03 */ "System.Double",
                /* 04 */ "System.Double",
                /* 05 */ "System.Int32",
                /* 06 */ "System.Double",
                /* 07 */ "System.Double",
                /* 08 */ "System.Double",
                /* 09 */ "System.Double",
                /* 10 */ "System.Double",
                /* 11 */ "System.Double",
                /* 12 */ "System.Double",
                /* 13 */ "System.Double",
                /* 14 */ "System.Double",
                /* 15 */ "System.Double",
                /* 16 */ "System.Double",
                /* 17 */ "System.Double",
                /* 18 */ "System.Double",
                /* 19 */ "System.Double",
                /* 20 */ "System.Double",
                /* 21 */ "GSF.UnixTimeTag",
                /* 22 */ "GSF.UnixTimeTag",
                /* 23 */ "GSF.UnixTimeTag",
                /* 24 */ "System.Double",
                /* 25 */ "System.Double"
            };

            string[] SystemStatFormats =
            {
                /* 01 */ "{0:N3} %",
                /* 02 */ "{0:N3} %",
                /* 03 */ "{0:N3} MB",
                /* 04 */ "{0:N3} MB",
                /* 05 */ "{0:N0}",
                /* 06 */ "{0:N3}",
                /* 07 */ "{0:N3}",
                /* 08 */ "{0:N3}",
                /* 09 */ "{0:N3} KBps",
                /* 10 */ "{0:N3} KBps",
                /* 11 */ "{0:N3}",
                /* 12 */ "{0:N3}",
                /* 13 */ "{0:N3}",
                /* 14 */ "{0:N3}",
                /* 15 */ "{0:N3} s",
                /* 16 */ "{0:N3} %",
                /* 17 */ "{0:N3} %",
                /* 18 */ "{0:N3} GB",
                /* 19 */ "{0:N3} GB",
                /* 20 */ "{0:N3} %",
                /* 21 */ "{0:yyyy''-''MM''-''dd'' ''HH'':''mm'':''ss''.''fff}",
                /* 22 */ "{0:yyyy''-''MM''-''dd'' ''HH'':''mm'':''ss''.''fff}",
                /* 23 */ "{0:yyyy''-''MM''-''dd'' ''HH'':''mm'':''ss''.''fff}",
                /* 24 */ "{0:N3} s",
                /* 25 */ "{0:N3} %"
            };

            // NOTE: !! The statistic names defined in the following array are used to define associated function names (minus spaces) - as a result, do *not* leisurely change these statistic names without understanding the consequences
            string[] DeviceStatNames =
            { 
                /* 01 */ "Data Quality Errors", 
                /* 02 */ "Time Quality Errors", 
                /* 03 */ "Device Errors", 
                /* 04 */ "Measurements Received", 
                /* 05 */ "Measurements Expected", 
                /* 06 */ "Measurements With Error", 
                /* 07 */ "Measurements Defined",
                /* 08 */ "Device Time Deviation From Average"
            };

            string[] DeviceStatDescriptions =
            {
                /* 01 */ "Number of data quality errors reported by device during last reporting interval.",
                /* 02 */ "Number of time quality errors reported by device during last reporting interval.",
                /* 03 */ "Number of device errors reported by device during last reporting interval.",
                /* 04 */ "Number of measurements received from device during last reporting interval.",
                /* 05 */ "Expected number of measurements received from device during last reporting interval.",
                /* 06 */ "Number of measurements received while device was reporting errors during last reporting interval.",
                /* 07 */ "Number of defined measurements from device during last reporting interval.",
                /* 08 */ "Device time deviation from average for all input devices in seconds"
            };

            string[] DeviceStatTypes =
            {
                /* 01 */ "System.Int32",
                /* 02 */ "System.Int32",
                /* 03 */ "System.Int32",
                /* 04 */ "System.Int32",
                /* 05 */ "System.Int32",
                /* 06 */ "System.Int32",
                /* 07 */ "System.Int32",
                /* 08 */ "System.Double"
            };

            string[] DeviceStatFormats =
            {
                /* 01 */ "{0:N0}",
                /* 02 */ "{0:N0}",
                /* 03 */ "{0:N0}",
                /* 04 */ "{0:N0}",
                /* 05 */ "{0:N0}",
                /* 06 */ "{0:N0}",
                /* 07 */ "{0:N0}",
                /* 08 */ "{0:N3} s"
            };

            string[] SubscriberStatNames =
            {
                /* 01 */ "Subscriber Connected", 
                /* 02 */ "Subscriber Authenticated", 
                /* 03 */ "Processed Measurements", 
                /* 04 */ "Total Bytes Received", 
                /* 05 */ "Authorized Signal Count", 
                /* 06 */ "Unauthorized Signal Count", 
                /* 07 */ "Lifetime Measurements", 
                /* 08 */ "Lifetime Bytes Received", 
                /* 09 */ "Minimum Measurements Per Second", 
                /* 10 */ "Maximum Measurements Per Second", 
                /* 11 */ "Average Measurements Per Second", 
                /* 12 */ "Lifetime Minimum Latency", 
                /* 13 */ "Lifetime Maximum Latency", 
                /* 14 */ "Lifetime Average Latency", 
                /* 15 */ "Up Time", 
                /* 16 */ "TLS Secured Channel"
            };

            string[] SubscriberStatDescriptions =
            {
                /* 01 */ "Boolean value representing if the subscriber was continually connected during last reporting interval.",
                /* 02 */ "Boolean value representing if the subscriber was authenticated to the publisher during last reporting interval.",
                /* 03 */ "Number of processed measurements reported by the subscriber during last reporting interval.",
                /* 04 */ "Number of bytes received from subscriber during last reporting interval.",
                /* 05 */ "Number of signals authorized to the subscriber by the publisher.",
                /* 06 */ "Number of signals denied to the subscriber by the publisher.",
                /* 07 */ "Number of processed measurements reported by the subscriber during the lifetime of the subscriber.",
                /* 08 */ "Number of bytes received from subscriber during the lifetime of the subscriber.",
                /* 09 */ "The minimum number of measurements received per second during the last reporting interval.",
                /* 10 */ "The maximum number of measurements received per second during the last reporting interval.",
                /* 11 */ "The average number of measurements received per second during the last reporting interval.",
                /* 12 */ "Minimum latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                /* 13 */ "Maximum latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                /* 14 */ "Average latency from output stream, in milliseconds, during the lifetime of the subscriber.",
                /* 15 */ "Total number of seconds subscriber has been running.",
                /* 16 */ "Boolean value representing if subscriber command channel has transport layer security enabled"
            };

            string[] SubscriberStatMethodSuffix =
            { 
                /* 01 */ "Connected", 
                /* 02 */ "Authenticated", 
                /* 03 */ "ProcessedMeasurements", 
                /* 04 */ "TotalBytesReceived", 
                /* 05 */ "AuthorizedCount", 
                /* 06 */ "UnauthorizedCount", 
                /* 07 */ "LifetimeMeasurements", 
                /* 08 */ "LifetimeBytesReceived", 
                /* 09 */ "MinimumMeasurementsPerSecond", 
                /* 10 */ "MaximumMeasurementsPerSecond", 
                /* 11 */ "AverageMeasurementsPerSecond", 
                /* 12 */ "LifetimeMinimumLatency", 
                /* 13 */ "LifetimeMaximumLatency", 
                /* 14 */ "LifetimeAverageLatency", 
                /* 15 */ "UpTime", 
                /* 16 */ "TLSSecuredChannel"
            };

            string[] SubscriberStatTypes =
            {
                /* 01 */ "System.Boolean", 
                /* 02 */ "System.Boolean", 
                /* 03 */ "System.Int32", 
                /* 04 */ "System.Int32", 
                /* 05 */ "System.Int32", 
                /* 06 */ "System.Int32", 
                /* 07 */ "System.Int64", 
                /* 08 */ "System.Int64", 
                /* 09 */ "System.Int32", 
                /* 10 */ "System.Int32", 
                /* 11 */ "System.Int32", 
                /* 12 */ "System.Int32", 
                /* 13 */ "System.Int32", 
                /* 14 */ "System.Int32", 
                /* 15 */ "System.Double", 
                /* 16 */ "System.Boolean"
            };
            
            string[] SubscriberStatFormats =
            {
                /* 01 */ "{0}",
                /* 02 */ "{0}", 
                /* 03 */ "{0:N0}", 
                /* 04 */ "{0:N0}", 
                /* 05 */ "{0:N0}", 
                /* 06 */ "{0:N0}", 
                /* 07 */ "{0:N0}", 
                /* 08 */ "{0:N0}", 
                /* 09 */ "{0:N0}", 
                /* 10 */ "{0:N0}", 
                /* 11 */ "{0:N0}", 
                /* 12 */ "{0:N0} ms", 
                /* 13 */ "{0:N0} ms", 
                /* 14 */ "{0:N0} ms", 
                /* 15 */ "{0:N3} s", 
                /* 16 */ "{0}"
            };

            string[] PublisherStatNames =
            { 
                /* 01 */ "Publisher Connected", 
                /* 02 */ "Connected Clients", 
                /* 03 */ "Processed Measurements", 
                /* 04 */ "Total Bytes Sent", 
                /* 05 */ "Lifetime Measurements", 
                /* 06 */ "Lifetime Bytes Sent", 
                /* 07 */ "Minimum Measurements Per Second", 
                /* 08 */ "Maximum Measurements Per Second", 
                /* 09 */ "Average Measurements Per Second", 
                /* 10 */ "Lifetime Minimum Latency", 
                /* 11 */ "Lifetime Maximum Latency", 
                /* 12 */ "Lifetime Average Latency", 
                /* 13 */ "Up Time", 
                /* 14 */ "TLS Secured Channel"
            };

            string[] PublisherStatDescriptions =
            {
                /* 01 */ "Boolean value representing if the publisher was continually connected during last reporting interval.",
                /* 02 */ "Number of clients connected to the command channel of the publisher during last reporting interval.",
                /* 03 */ "Number of processed measurements reported by the publisher during last reporting interval.",
                /* 04 */ "Number of bytes sent by the publisher during the last reporting interval.",
                /* 05 */ "Number of processed measurements reported by the publisher during the lifetime of the publisher.",
                /* 06 */ "Number of bytes sent by the publisher during the lifetime of the publisher.",
                /* 07 */ "The minimum number of measurements sent per second during the last reporting interval.",
                /* 08 */ "The maximum number of measurements sent per second during the last reporting interval.",
                /* 09 */ "The average number of measurements sent per second during the last reporting interval.",
                /* 10 */ "Minimum latency from output stream, in milliseconds, during the lifetime of the publisher.",
                /* 11 */ "Maximum latency from output stream, in milliseconds, during the lifetime of the publisher.",
                /* 12 */ "Average latency from output stream, in milliseconds, during the lifetime of the publisher.",
                /* 13 */ "Total number of seconds publisher has been running.",
                /* 14 */ "Boolean value representing if publisher command channel has transport layer security enabled"
            };

            string[] PublisherStatMethodSuffix =
            { 
                /* 01 */ "Connected", 
                /* 02 */ "ConnectedClientCount", 
                /* 03 */ "ProcessedMeasurements", 
                /* 04 */ "TotalBytesSent", 
                /* 05 */ "LifetimeMeasurements", 
                /* 06 */ "LifetimeBytesSent", 
                /* 07 */ "MinimumMeasurementsPerSecond", 
                /* 08 */ "MaximumMeasurementsPerSecond", 
                /* 09 */ "AverageMeasurementsPerSecond", 
                /* 10 */ "LifetimeMinimumLatency", 
                /* 11 */ "LifetimeMaximumLatency", 
                /* 12 */ "LifetimeAverageLatency", 
                /* 13 */ "UpTime", 
                /* 14 */ "TLSSecuredChannel"
            };

            string[] PublisherStatTypes =
            { 
                /* 01 */ "System.Boolean", 
                /* 02 */ "System.Int32", 
                /* 03 */ "System.Int32", 
                /* 04 */ "System.Int32", 
                /* 05 */ "System.Int64", 
                /* 06 */ "System.Int64", 
                /* 07 */ "System.Int32", 
                /* 08 */ "System.Int32", 
                /* 09 */ "System.Int32", 
                /* 10 */ "System.Int32", 
                /* 11 */ "System.Int32", 
                /* 12 */ "System.Int32", 
                /* 13 */ "System.Double", 
                /* 14 */ "System.Boolean"
            };

            string[] PublisherStatFormats =
            { 
                /* 01 */ "{0}", 
                /* 02 */ "{0:N0}", 
                /* 03 */ "{0:N0}", 
                /* 04 */ "{0:N0}", 
                /* 05 */ "{0:N0}", 
                /* 06 */ "{0:N0}", 
                /* 07 */ "{0:N0}", 
                /* 08 */ "{0:N0}", 
                /* 09 */ "{0:N0}", 
                /* 10 */ "{0:N0} ms", 
                /* 11 */ "{0:N0} ms", 
                /* 12 */ "{0:N0} ms", 
                /* 13 */ "{0:N3} s", 
                /* 14 */ "{0}"
            };

            // NOTE: !! The statistic names defined in the following array are used to define associated function names (minus spaces) - as a result, do *not* leisurely change these statistic names without understanding the consequences
            string[] ProcessStatNames =
            { 
                /* 01 */ "CPU Usage", 
                /* 02 */ "Memory Usage", 
                /* 03 */ "Up Time"
            };

            string[] ProcessStatDescriptions =
            { 
                /* 01 */ "Percentage of CPU currently used by the launched process.",
                /* 02 */ "Amount of memory currently used by the launched process in megabytes.",
                /* 03 */ "Total number of seconds the launched process has been running."
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
                    statType = SystemStatTypes[i];
                    statFormat = SystemStatFormats[i];
                    database.Connection.ExecuteNonQuery(string.Format(SystemStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat));
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
                    statType = DeviceStatTypes[i];
                    statFormat = DeviceStatFormats[i];
                    database.Connection.ExecuteNonQuery(string.Format(DeviceStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat));
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
                Guid nodeID = Guid.Parse(nodeIDQueryString.Trim('\''));

                // Ensure that the alarm adapter is defined.
                int alarmAdapterCount = connection.ExecuteScalar<int>(AlarmAdapterCountFormat, nodeID);

                if (alarmAdapterCount == 0)
                    connection.ExecuteNonQuery(AlarmAdapterInsertFormat, nodeID);

                // Ensure that the alarm record is defined in the ConfigurationEntity table.
                int alarmConfigEntityCount = connection.ExecuteScalar<int>(AlarmConfigEntityCountFormat);

                if (alarmConfigEntityCount == 0)
                    connection.ExecuteNonQuery(AlarmConfigEntityInsertFormat);

                // Ensure that the alarm record is defined in the SignalType table.
                int alarmSignalTypeCount = connection.ExecuteScalar<int>(AlarmSignalTypeCountFormat);

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

            // Add statistics for the alarms defined in the Alarm table.
            string methodName = $"Get{source}Statistic_MeasurementCountForSeverity";
            DataTable missingStatistics = connection.RetrieveData(MissingStatisticsFormat, source, methodName);

            if (missingStatistics.Rows.Count > 0)
            {
                int signalIndex = connection.ExecuteScalar<int>(MaxSignalIndexFormat, source);

                foreach (DataRow missingStatistic in missingStatistics.Rows)
                {
                    signalIndex++;
                    int severity = missingStatistic.ConvertField<int>("Severity");
                    string name = $"Alarm Severity {severity}";
                    string description = $"Number of measurements received while alarm with severity {severity} was raised during the last reporting interval.";

                    connection.ExecuteNonQuery(InsertAlarmStatisticFormat, source, signalIndex, name, description, "DataQualityMonitoring.dll", "DataQualityMonitoring.AlarmStatistics", methodName, severity, 1, "System.Int32", "{0:N0}", 0, 1001 - severity);
                }
            }
        }
    }
}
