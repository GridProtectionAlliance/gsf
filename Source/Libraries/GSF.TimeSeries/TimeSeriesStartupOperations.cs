//******************************************************************************************************
//  TimeSeriesStartupOperations.cs - Gbtc
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
using GSF.Configuration;
using GSF.Data;
using GSF.Identity;

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
            ValidateActiveMeasurements(database, nodeIDQueryString);
            ValidateAccountsAndGroups(database, nodeIDQueryString);
            ValidateDataPublishers(database, nodeIDQueryString);
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
                        sid = UserInfo.AccountNameToSID(accountName);

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
                        sid = UserInfo.AccountNameToSID(accountName);

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
        private static void ValidateDataPublishers(AdoDataConnection database, string nodeIDQueryString)
        {
            const string DataPublisherCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName='{0}!DATAPUBLISHER' AND NodeID = {1}";
            const string DataPublisherInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, Enabled) VALUES({0}, '{1}!DATAPUBLISHER', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Transport.DataPublisher', 'securityMode={2}; allowSynchronizedSubscription=false; useBaseTimeOffsets=true; {3}', 1)";
            //const string DataPublisherUpdateformat = "UPDATE CustomActionAdapter SET ConnectionString = '{0}' WHERE AdapterName = '{1}!DATAPUBLISHER'";

            int internalDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "INTERNAL", nodeIDQueryString)));
            int externalDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "EXTERNAL", nodeIDQueryString)));
            int tlsDataPublisherCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(DataPublisherCountFormat, "TLS", nodeIDQueryString)));

            if (internalDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(DataPublisherInsertFormat, nodeIDQueryString, "INTERNAL", "None", "cacheMeasurementKeys={FILTER ActiveMeasurements WHERE SignalType = ''STAT''}"));

            if (externalDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(DataPublisherInsertFormat, nodeIDQueryString, "EXTERNAL", "Gateway", ""));

            if (tlsDataPublisherCount == 0)
                database.Connection.ExecuteNonQuery(string.Format(DataPublisherInsertFormat, nodeIDQueryString, "TLS", "TLS", ""));
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
            const string RuntimeDeviceCountFormat = "SELECT COUNT(*) FROM Runtime WHERE ID = {0} AND SourceTable = 'Device'";

            const string StatHistorianIDFormat = "SELECT ID FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
            const string StatSignalTypeIDFormat = "SELECT ID FROM SignalType WHERE Acronym = 'STAT'";
            const string StatMeasurementCountFormat = "SELECT COUNT(*) FROM Measurement WHERE SignalReference = '{0}' AND HistorianID = {1}";

            const string SubscriberRowsFormat = "SELECT * FROM IaonInputAdapter WHERE TypeName = 'GSF.TimeSeries.Transport.DataSubscriber' AND NodeID = {0}";
            const string DeviceRowsFormat = "SELECT * FROM Device WHERE ParentID = {0}";
            const string PublisherRowsFormat = "SELECT * FROM IaonActionadapter WHERE TypeName = 'GSF.TimeSeries.Transport.DataPublisher' AND NodeID = {0}";
            const string RuntimeSourceIDFormat = "SELECT SourceID FROM Runtime WHERE ID = {0}";

            // INSERT queries
            const string StatConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('RuntimeStatistic', 'Statistics', 'Defines statistics that are monitored for the system, devices, and output streams', 11, 1)";
            const string StatSignalTypeInsertFormat = "INSERT INTO SignalType(Name, Acronym, Suffix, Abbreviation, Source, EngineeringUnits) VALUES('Statistic', 'STAT', 'ST', 'P', 'Any', '')";

            const string StatHistorianInsertFormat = "INSERT INTO Historian(NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, IsLocal, Description, LoadOrder, Enabled) VALUES({0}, 'STAT', 'Statistics Archive', 'TestingAdapters.dll', 'TestingAdapters.VirtualOutputAdapter', '', 1, 'Local historian used to archive system statistics', 9999, 1)";
            const string StatEngineInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, LoadOrder, Enabled) VALUES({0}, 'STATISTIC!SERVICES', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.StatisticsEngine', 0, 1)";
            const string SystemStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('System', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.PerformanceStatistics', 'GetSystemStatistic_{3}', '', 1, 'System.Double', '{{0:N3}}', 0, {0})";
            const string DeviceStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Device', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.DeviceStatistics', 'GetDeviceStatistic_{3}', '', 1, 'System.Int32', '{{0:N0}}', 0, {0})";
            const string SubscriberStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Subscriber', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.GatewayStatistics', 'GetSubscriberStatistic_{3}', '', 1, '{4}', '{5}', 0, {0})";
            const string PublisherStatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('Publisher', {0}, '{1}', '{2}', 'GSF.TimeSeries.dll', 'GSF.TimeSeries.Statistics.GatewayStatistics', 'GetPublisherStatistic_{3}', '', 1, '{4}', '{5}', 0, {0})";

            const string StatMeasurementInsertFormat = "INSERT INTO Measurement(HistorianID, PointTag, SignalTypeID, SignalReference, Description, Enabled) VALUES({0}, {1}, {2}, {3}, {4}, 1)";
            const string DeviceStatMeausrementInsertFormat = "INSERT INTO Measurement(HistorianID, DeviceID, PointTag, SignalTypeID, SignalReference, Description, Enabled) VALUES({0}, {1}, {2}, {3}, {4}, {5}, 1)";

            // DELETE queries
            const string SystemStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'System' AND SignalIndex <= {0}";
            const string DeviceStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Device' AND SignalIndex <= {0}";
            const string SubscriberStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Subscriber' AND SignalIndex <= {0}";
            const string PublisherStatisticDeleteFormat = "DELETE FROM Statistic WHERE Source = 'Publisher' AND SignalIndex <= {0}";

            // Names and descriptions for each of the statistics
            string[] SystemStatNames = { "CPU Usage", "Average CPU Usage", "Memory Usage", "Average Memory Usage", "Thread Count", "Average Thread Count", "Threading Contention Rate", "Average Threading Contention Rate", "IO Usage", "Average IO Usage", "Datagram Send Rate", "Average Datagram Send Rate", "Datagram Receive Rate", "Average Datagram Receive Rate" };

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
                                                "Number of IPv4 datagrams currently sent by this process per second.",
                                                "Average number of IPv4 datagrams sent by this process per second.",
                                                "Number of IPv4 datagrams currently received by this process per second.",
                                                "Average number of IPv4 datagrams received by this process per second."
                                              };

            string[] DeviceStatNames = { "Data Quality Errors", "Time Quality Errors", "Device Errors", "Measurements Received", "Measurements Expected" };

            string[] DeviceStatDescriptions = { "Number of data quaility errors reported by device during last reporting interval.",
                                                "Number of time quality errors reported by device during last reporting interval.",
                                                "Number of device errors reported by device during last reporting interval.",
                                                "Number of measurements received from device during last reporting interval.",
                                                "Expected number of measurements received from device during last reporting interval."
                                              };

            string[] SubscriberStatNames = { "Subscriber Connected", "Subscriber Authenticated", "Processed Measurements", "Total Bytes Received", "Authorized Signal Count", "Unauthorized Signal Count", "Lifetime Measurements", "Lifetime Bytes Received", "Minimum Measurements Per Second", "Maximum Measurements Per Second", "Average Measurements Per Second", "Lifetime Minimum Latency", "Lifetime Maximum Latency", "Lifetime Average Latency" };

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
                                                    "Average latency from output stream, in milliseconds, during the lifetime of the subscriber."
                                                  };

            string[] SubscriberStatMethodSuffix = { "Connected", "Authenticated", "ProcessedMeasurements", "TotalBytesReceived", "AuthorizedCount", "UnauthorizedCount", "LifetimeMeasurements", "LifetimeBytesReceived", "MinimumMeasurementsPerSecond", "MaximumMeasurementsPerSecond", "AverageMeasurementsPerSecond", "LifetimeMinimumLatency", "LifetimeMaximumLatency", "LifetimeAverageLatency", "LifetimeMinimumLatency", "LifetimeMaximumLatency", "LifetimeAverageLatency" };
            string[] SubscriberStatTypes = { "System.Boolean", "System.Boolean", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int64", "System.Int64", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32" };
            string[] SubscriberStatFormats = { "{0}", "{0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0} ms", "{0:N0} ms", "{0:N0} ms" };

            string[] PublisherStatNames = { "Publisher Connected", "Connected Clients", "Processed Measurements", "Total Bytes Sent", "Lifetime Measurements", "Lifetime Bytes Sent", "Minimum Measurements Per Second", "Maximum Measurements Per Second", "Average Measurements Per Second", "Lifetime Minimum Latency", "Lifetime Maximum Latency", "Lifetime Average Latency" };

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
                                                    "Average latency from output stream, in milliseconds, during the lifetime of the publisher."
                                                 };

            string[] PublisherStatMethodSuffix = { "Connected", "ConnectedClientCount", "ProcessedMeasurements", "TotalBytesSent", "LifetimeMeasurements", "LifetimeBytesSent", "MinimumMeasurementsPerSecond", "MaximumMeasurementsPerSecond", "AverageMeasurementsPerSecond", "LifetimeMinimumLatency", "LifetimeMaximumLatency", "LifetimeAverageLatency" };
            string[] PublisherStatTypes = { "System.Boolean", "System.Int32", "System.Int32", "System.Int32", "System.Int64", "System.Int64", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32" };
            string[] PublisherStatFormats = { "{0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0}", "{0:N0} ms", "{0:N0} ms", "{0:N0} ms" };

            // Parameterized query string for inserting statistic measurements
            string statMeasurementInsertQuery = database.ParameterizedQueryString(StatMeasurementInsertFormat, "historianID", "pointTag", "signalTypeID", "signalReference", "description");
            string deviceStatMeasurementInsertQuery = database.ParameterizedQueryString(DeviceStatMeausrementInsertFormat, "historianID", "deviceID", "pointTag", "signalTypeID", "signalReference", "description");

            // Query for count values to ensure existence of these records
            int statConfigEntityCount = Convert.ToInt32(database.Connection.ExecuteScalar(StatConfigEntityCountFormat));
            int statSignalTypeCount = Convert.ToInt32(database.Connection.ExecuteScalar(StatSignalTypeCountFormat));

            int statHistorianCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatHistorianCountFormat, nodeIDQueryString)));
            int statEngineCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatEngineCountFormat, nodeIDQueryString)));
            int systemStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(SystemStatCountFormat));
            int deviceStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(DeviceStatCountFormat));
            int subscriberStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(SubscriberStatCountFormat));
            int publisherStatCount = Convert.ToInt32(database.Connection.ExecuteScalar(PublisherStatCountFormat));

            // Statistic info for inserting statistics
            int signalIndex;
            string statName;
            string statDescription;
            string statMethodSuffix;
            string statType;
            string statFormat;

            // Values from queries to ensure existence of statistic measurements
            int statHistorianID;
            int statSignalTypeID;
            int statMeasurementCount;
            int runtimeDeviceCount;

            // Statistic measurement info for inserting statistic measurements
            string nodeName;
            string pointTag;
            string signalReference;
            string measurementDescription;

            // Adapter info for inserting gateway measurements
            int adapterID;
            int adapterSourceID;
            string adapterName;
            string companyAcronym;

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
            if (systemStatCount != SystemStatNames.Length)
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
            if (deviceStatCount != DeviceStatNames.Length)
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
            if (subscriberStatCount != SubscriberStatNames.Length)
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
                    database.Connection.ExecuteNonQuery(string.Format(SubscriberStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat));
                }
            }

            // Ensure that subscriber statistics exist
            if (publisherStatCount != PublisherStatNames.Length)
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
                    database.Connection.ExecuteNonQuery(string.Format(PublisherStatInsertFormat, signalIndex, statName, statDescription, statMethodSuffix, statType, statFormat));
                }
            }

            // Ensure that system statistic measurements exist
            statHistorianID = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatHistorianIDFormat, nodeIDQueryString)));
            statSignalTypeID = Convert.ToInt32(database.Connection.ExecuteScalar(StatSignalTypeIDFormat));
            nodeName = GetNodeName(database, nodeIDQueryString);

            // Modify node name so that it can be applied in a measurement point tag
            nodeName = nodeName.RemoveCharacters(c => !char.IsLetterOrDigit(c));
            nodeName = nodeName.Replace(' ', '_').ToUpper();

            for (int i = 0; i < SystemStatNames.Length; i++)
            {
                signalIndex = i + 1;
                signalReference = string.Format("{0}!SYSTEM-ST{1}", nodeName, signalIndex);
                statMeasurementCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatMeasurementCountFormat, signalReference, statHistorianID)));

                if (statMeasurementCount == 0)
                {
                    pointTag = string.Format("{0}!SYSTEM:ST{1}", nodeName, signalIndex);
                    measurementDescription = string.Format("System Statistic for {0}", SystemStatDescriptions[i]);
                    database.Connection.ExecuteNonQuery(statMeasurementInsertQuery, (object)statHistorianID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                }
            }

            // Ensure that subscriber statistic measurements exist
            foreach (DataRow subscriber in database.Connection.RetrieveData(database.AdapterType, string.Format(SubscriberRowsFormat, nodeIDQueryString)).Rows)
            {
                adapterID = subscriber.ConvertField<int>("ID");
                adapterSourceID = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(RuntimeSourceIDFormat, adapterID)));
                runtimeDeviceCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(RuntimeDeviceCountFormat, adapterID)));
                adapterName = subscriber.Field<string>("AdapterName");

                if (!TryGetCompanyAcronymFromDevice(database, adapterSourceID, out companyAcronym))
                    companyAcronym = GetCompanyAcronym(database, nodeIDQueryString);

                for (int i = 0; i < SubscriberStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    signalReference = string.Format("{0}!SUB-ST{1}", adapterName, signalIndex);
                    statMeasurementCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatMeasurementCountFormat, signalReference, statHistorianID)));

                    if (statMeasurementCount == 0)
                    {
                        pointTag = string.Format("{0}_{1}!SUB:ST{2}", companyAcronym, adapterName, signalIndex);
                        measurementDescription = string.Format("Subscriber Statistic for {0}", SubscriberStatDescriptions[i]);

                        if (runtimeDeviceCount > 0)
                        {
                            // Subscriber is defined in the Device table; include the device ID in the insert query
                            database.Connection.ExecuteNonQuery(deviceStatMeasurementInsertQuery, (object)statHistorianID, adapterSourceID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                        }
                        else
                        {
                            // Subscriber is not defined in the Device table; do not include a device ID
                            database.Connection.ExecuteNonQuery(statMeasurementInsertQuery, (object)statHistorianID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                        }
                    }
                }

                if (runtimeDeviceCount > 0)
                {
                    // Ensure that device statistic measurements exist
                    foreach (DataRow device in database.Connection.RetrieveData(database.AdapterType, string.Format(DeviceRowsFormat, adapterSourceID)).Rows)
                    {
                        adapterID = device.ConvertField<int>("ID");
                        adapterName = "LOCAL$" + device.Field<string>("Acronym");

                        for (int i = 0; i < DeviceStatNames.Length; i++)
                        {
                            signalIndex = i + 1;
                            signalReference = string.Format("{0}!PMU-ST{1}", adapterName, signalIndex);
                            statMeasurementCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatMeasurementCountFormat, signalReference, statHistorianID)));

                            if (statMeasurementCount == 0)
                            {
                                pointTag = string.Format("{0}_{1}!PMU:ST{2}", companyAcronym, adapterName, signalIndex);
                                measurementDescription = string.Format("Device Statistic local to node '{0}' for {1}", nodeName, DeviceStatDescriptions[i]);
                                database.Connection.ExecuteNonQuery(deviceStatMeasurementInsertQuery, (object)statHistorianID, adapterID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                            }
                        }
                    }
                }
            }

            // Ensure that publisher statistic measurements exist
            companyAcronym = GetCompanyAcronym(database, nodeIDQueryString);

            foreach (DataRow publisher in database.Connection.RetrieveData(database.AdapterType, string.Format(PublisherRowsFormat, nodeIDQueryString)).Rows)
            {
                adapterName = publisher.Field<string>("AdapterName");

                for (int i = 0; i < PublisherStatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    signalReference = string.Format("{0}!PUB-ST{1}", adapterName, signalIndex);
                    statMeasurementCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(StatMeasurementCountFormat, signalReference, statHistorianID)));

                    if (statMeasurementCount == 0)
                    {
                        pointTag = string.Format("{0}_{1}!PUB:ST{2}", companyAcronym, adapterName, signalIndex);
                        measurementDescription = string.Format("Publisher Statistic for {0}", PublisherStatDescriptions[i]);
                        database.Connection.ExecuteNonQuery(statMeasurementInsertQuery, (object)statHistorianID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                    }
                }
            }
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records
        /// that are required for alarming exist in the database.
        /// </summary>
        private static void ValidateAlarming(AdoDataConnection database, string nodeIDQueryString)
        {
            // SELECT queries
            const string AlarmCountFormat = "SELECT COUNT(*) FROM Alarm";
            const string AlarmAdapterCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName = 'ALARM!SERVICES' AND NodeID = {0}";
            const string AlarmConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'Alarms'";
            const string AlarmSignalTypeCountFormat = "SELECT COUNT(*) FROM SignalType WHERE Name = 'Alarm'";

            // INSERT queries
            const string AlarmAdapterInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, LoadOrder, Enabled) VALUES({0}, 'ALARM!SERVICES', 'DataQualityMonitoring.dll', 'DataQualityMonitoring.AlarmAdapter', 0, 1)";
            const string AlarmConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('Alarm', 'Alarms', 'Defines alarms that monitor the values of measurements', 17, 1)";
            const string AlarmSignalTypeInsertFormat = "INSERT INTO SignalType(Name, Acronym, Suffix, Abbreviation, Source, EngineeringUnits) VALUES('Alarm', 'ALRM', 'AL', 'AL', 'Any', '')";

            bool alarmTableExists;
            int alarmAdapterCount;
            int alarmConfigEntityCount;
            int alarmSignalTypeCount;

            try
            {
                // Determine whether the alarm table exists
                // before inserting records related to alarming
                database.Connection.ExecuteScalar(AlarmCountFormat);
                alarmTableExists = true;
            }
            catch
            {
                alarmTableExists = false;
            }

            if (alarmTableExists)
            {
                // Ensure that the alarm adapter is defined.
                alarmAdapterCount = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(AlarmAdapterCountFormat, nodeIDQueryString)));

                if (alarmAdapterCount == 0)
                    database.Connection.ExecuteNonQuery(string.Format(AlarmAdapterInsertFormat, nodeIDQueryString));

                // Ensure that the alarm record is defined in the ConfigurationEntity table.
                alarmConfigEntityCount = Convert.ToInt32(database.Connection.ExecuteScalar(AlarmConfigEntityCountFormat));

                if (alarmConfigEntityCount == 0)
                    database.Connection.ExecuteNonQuery(AlarmConfigEntityInsertFormat);

                // Ensure that the alarm record is defined in the SignalType table.
                alarmSignalTypeCount = Convert.ToInt32(database.Connection.ExecuteScalar(AlarmSignalTypeCountFormat));

                if (alarmSignalTypeCount == 0)
                    database.Connection.ExecuteNonQuery(AlarmSignalTypeInsertFormat);
            }
        }

        // Gets the name of the node identified by the given node ID query string.
        private static string GetNodeName(AdoDataConnection database, string nodeIDQueryString)
        {
            const string NodeNameFormat = "SELECT Name FROM Node WHERE ID = {0}";
            return database.Connection.ExecuteScalar(string.Format(NodeNameFormat, nodeIDQueryString)).ToString();
        }

        // Attempts to get company acronym from device table in database
        private static bool TryGetCompanyAcronymFromDevice(AdoDataConnection database, int deviceID, out string companyAcronym)
        {
            const string CompanyIDFormat = "SELECT CompanyID FROM Device WHERE ID = {0}";
            const string CompanyAcronymFormat = "SELECT MapAcronym FROM Company WHERE ID = {0}";
            int companyID;

            try
            {
                companyID = Convert.ToInt32(database.Connection.ExecuteScalar(string.Format(CompanyIDFormat, deviceID)));
                companyAcronym = database.Connection.ExecuteScalar(string.Format(CompanyAcronymFormat, companyID)).ToNonNullString();
                return true;
            }
            catch
            {
                companyAcronym = string.Empty;
                return false;
            }
        }

        // Attempts to get company acronym from database and, failing
        // that, attempts to get it from the configuration file.
        private static string GetCompanyAcronym(AdoDataConnection database, string nodeIDQueryString)
        {
            const string NodeCompanyIDFormat = "SELECT CompanyID FROM Node WHERE ID = {0}";
            const string CompanyAcronymFormat = "SELECT MapAcronym FROM Company WHERE ID = {0}";

            int nodeCompanyID;
            string companyAcronym;

            nodeCompanyID = int.Parse(database.Connection.ExecuteScalar(string.Format(NodeCompanyIDFormat, nodeIDQueryString)).ToNonNullString("0"));

            if (nodeCompanyID > 0)
                companyAcronym = database.Connection.ExecuteScalar(string.Format(CompanyAcronymFormat, nodeCompanyID)).ToNonNullString();
            else
                companyAcronym = ConfigurationFile.Current.Settings["systemSettings"]["CompanyAcronym"].Value.TruncateRight(3);

            return companyAcronym;
        }
    }
}
