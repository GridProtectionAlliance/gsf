//******************************************************************************************************
//  TimeSeriesDataOperation.cs - Gbtc
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
//  02/14/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Linq;
using TVA;
using TVA.Configuration;
using TVA.Data;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Defines a data operations to be performed at startup.
    /// </summary>
    public static class TimeSeriesStartupOperations
    {
        // Messaging to the service
        private static Action<object, EventArgs<string>> s_statusMessage;
        private static Action<object, EventArgs<Exception>> s_processException;

        /// <summary>
        /// Delegates control to the data operations that are to be performed at startup.
        /// </summary>
        private static void PerformTimeSeriesStartupOperations(IDbConnection connection, Type adapterType, string nodeIDQueryString, Action<object, EventArgs<string>> statusMessage, Action<object, EventArgs<Exception>> processException)
        {
            // Set up messaging to the service
            s_statusMessage = statusMessage;
            s_processException = processException;

            // Run data operations
            ValidateDefaultNode(connection, nodeIDQueryString);
            ValidateActiveMeasurements(connection, nodeIDQueryString);
            ValidateStatistics(connection, nodeIDQueryString);
            ValidateAlarming(connection, nodeIDQueryString);
        }

        /// <summary>
        /// Data operation to validate and ensure there is a node in the database.
        /// </summary>
        private static void ValidateDefaultNode(IDbConnection connection, string nodeIDQueryString)
        {
            // Queries
            const string NodeCountFormat = "SELECT COUNT(*) FROM Node";

            const string NodeInsertFormat = "INSERT INTO Node(Name, CompanyID, Description, Settings, MenuType, MenuData, Master, LoadOrder, Enabled) " +
                "VALUES('Default', NULL, 'Default node', 'RemoteStatusServerConnectionString={server=localhost:8500};datapublisherport=6165;RealTimeStatisticServiceUrl=http://localhost:6052/historian', " +
                "'File', 'Menu.xml', 1, 0, 1)";

            const string NodeUpdateFormat = "UPDATE Node SET ID = {0}";

            // Determine whether the node exists in the database and create it if it doesn't.
            int nodeCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(NodeCountFormat, nodeIDQueryString)));

            if (nodeCount == 0)
            {
                connection.ExecuteNonQuery(NodeInsertFormat);
                connection.ExecuteNonQuery(string.Format(NodeUpdateFormat, nodeIDQueryString));
            }
        }

        /// <summary>
        /// Data operation to validate and ensure there is a record
        /// in the ConfigurationEntity table for ActiveMeasurements.
        /// </summary>
        private static void ValidateActiveMeasurements(IDbConnection connection, string nodeIDQueryString)
        {
            const string MeasurementConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'ActiveMeasurements'";
            const string MeasurementConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('ActiveMeasurement', 'ActiveMeasurements', 'Defines active system measurements for a TSF node', 4, 1)";

            int measurementConfigEntityCount = Convert.ToInt32(connection.ExecuteScalar(MeasurementConfigEntityCountFormat));

            if (measurementConfigEntityCount == 0)
                connection.ExecuteNonQuery(MeasurementConfigEntityInsertFormat);
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records that
        /// are required for statistics calculations exist in the database.
        /// </summary>
        private static void ValidateStatistics(IDbConnection connection, string nodeIDQueryString)
        {
            // SELECT queries
            const string StatConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE RuntimeName = 'Statistics'";
            const string StatSignalTypeCountFormat = "SELECT COUNT(*) FROM SignalType WHERE Acronym = 'STAT'";

            const string StatHistorianCountFormat = "SELECT COUNT(*) FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
            const string StatEngineCountFormat = "SELECT COUNT(*) FROM CustomActionAdapter WHERE AdapterName = 'STATISTIC!SERVICES' AND NodeID = {0}";
            const string StatCountFormat = "SELECT COUNT(*) FROM Statistic WHERE Source = 'System'";

            const string StatHistorianIDFormat = "SELECT ID FROM Historian WHERE Acronym = 'STAT' AND NodeID = {0}";
            const string StatSignalTypeIDFormat = "SELECT ID FROM SignalType WHERE Acronym = 'STAT'";
            const string StatMeasurementCountFormat = "SELECT COUNT(*) FROM Measurement WHERE SignalReference = '{0}' AND HistorianID = {1}";

            // INSERT queries
            const string StatConfigEntityInsertFormat = "INSERT INTO ConfigurationEntity(SourceName, RuntimeName, Description, LoadOrder, Enabled) VALUES('RuntimeStatistic', 'Statistics', 'Defines statistics that are monitored for the system, devices, and output streams', 11, 1)";
            const string StatSignalTypeInsertFormat = "INSERT INTO SignalType(Name, Acronym, Suffix, Abbreviation, Source, EngineeringUnits) VALUES('Statistic', 'STAT', 'ST', 'P', 'Any', '')";

            const string StatHistorianInsertFormat = "INSERT INTO Historian(NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, IsLocal, Description, LoadOrder, Enabled) VALUES({0}, 'STAT', 'Statistics Archive', 'TestingAdapters.dll', 'TestingAdapters.VirtualOutputAdapter', '', 1, 'Local historian used to archive system statistics', 9999, 1)";
            const string StatEngineInsertFormat = "INSERT INTO CustomActionAdapter(NodeID, AdapterName, AssemblyName, TypeName, LoadOrder, Enabled) VALUES({0}, 'STATISTIC!SERVICES', 'TimeSeriesFramework.dll', 'TimeSeriesFramework.Statistics.StatisticsEngine', 0, 1)";
            const string StatInsertFormat = "INSERT INTO Statistic(Source, SignalIndex, Name, Description, AssemblyName, TypeName, MethodName, Arguments, Enabled, DataType, DisplayFormat, IsConnectedState, LoadOrder) VALUES('System', {0}, '{1}', '{2}', 'TimeSeriesFramework.dll', 'TimeSeriesFramework.Statistics.PerformanceStatistics', 'GetSystemStatistic_{3}', '', 1, 'System.Double', '{{0:N3}}', 0, {0})";

            const string StatMeasurementInsertFormat = "INSERT INTO Measurement(HistorianID, PointTag, SignalTypeID, SignalReference, Description, Enabled) VALUES({0}, {1}, {2}, {3}, {4}, 1)";

            // Names and descriptions for each of the statistics
            string[] StatNames = { "CPU Usage", "Average CPU Usage", "Memory Usage", "Average Memory Usage", "Thread Count", "Average Thread Count", "Threading Contention Rate", "Average Threading Contention Rate", "IO Usage", "Average IO Usage", "Datagram Send Rate", "Average Datagram Send Rate", "Datagram Receive Rate", "Average Datagram Receive Rate" };

            string[] StatDescriptions = { "Percentage of CPU currently used by this process.",
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

            // Parameterized query string for inserting statistic measurements
            string statMeasurementInsertQuery = ParameterizedQueryString(connection.GetType(), StatMeasurementInsertFormat, "historianID", "pointTag", "signalTypeID", "signalReference", "description");

            // Query for count values to ensure existence of these records
            int statConfigEntityCount = Convert.ToInt32(connection.ExecuteScalar(StatConfigEntityCountFormat));
            int statSignalTypeCount = Convert.ToInt32(connection.ExecuteScalar(StatSignalTypeCountFormat));

            int statHistorianCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(StatHistorianCountFormat, nodeIDQueryString)));
            int statEngineCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(StatEngineCountFormat, nodeIDQueryString)));
            int statCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(StatCountFormat, nodeIDQueryString)));

            // Statistic info for inserting statistics
            int signalIndex;
            string statName;
            string statDescription;
            string methodSuffix;

            // Values from queries to ensure existence of statistic measurements
            int statHistorianID;
            int statSignalTypeID;
            int statMeasurementCount;

            // Statistic measurement info for inserting statistic measurements
            string nodeName;
            string pointTag;
            string signalReference;
            string measurementDescription;

            // Ensure that STAT signal type exists
            if (statSignalTypeCount == 0)
                connection.ExecuteNonQuery(StatSignalTypeInsertFormat);

            // Ensure that statistics configuration entity record exists
            if (statConfigEntityCount == 0)
                connection.ExecuteNonQuery(StatConfigEntityInsertFormat);

            // Ensure that statistics historian exists
            if (statHistorianCount == 0)
                connection.ExecuteNonQuery(string.Format(StatHistorianInsertFormat, nodeIDQueryString));

            // Ensure that statistics engine exists
            if (statEngineCount == 0)
                connection.ExecuteNonQuery(string.Format(StatEngineInsertFormat, nodeIDQueryString));

            // Ensure that system statistics exist
            if (statCount == 0)
            {
                for (int i = 0; i < StatNames.Length; i++)
                {
                    signalIndex = i + 1;
                    statName = StatNames[i];
                    statDescription = StatDescriptions[i];
                    methodSuffix = statName.Replace(" ", "");
                    connection.ExecuteNonQuery(string.Format(StatInsertFormat, signalIndex, statName, statDescription, methodSuffix));
                }
            }

            // Ensure that system statistic measurements exist
            statHistorianID = Convert.ToInt32(connection.ExecuteScalar(string.Format(StatHistorianIDFormat, nodeIDQueryString)));
            statSignalTypeID = Convert.ToInt32(connection.ExecuteScalar(StatSignalTypeIDFormat));
            nodeName = GetNodeName(connection, nodeIDQueryString);

            // Modify node name so that it can be applied in a measurement point tag
            nodeName = nodeName.RemoveCharacters(c => !char.IsLetterOrDigit(c));
            nodeName = nodeName.Replace(' ', '_').ToUpper();

            for (int i = 0; i < StatNames.Length; i++)
            {
                signalIndex = i + 1;
                signalReference = string.Format("SYSTEM-ST{0}", signalIndex);
                statMeasurementCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(StatMeasurementCountFormat, signalReference, statHistorianID)));

                if (statMeasurementCount == 0)
                {
                    pointTag = string.Format("{0}:ST{1}", nodeName, signalIndex);
                    measurementDescription = string.Format("System Statistic for {0}", StatDescriptions[i]);
                    connection.ExecuteNonQuery(statMeasurementInsertQuery, (object)statHistorianID, pointTag, statSignalTypeID, signalReference, measurementDescription);
                }
            }
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records
        /// that are required for alarming exist in the database.
        /// </summary>
        private static void ValidateAlarming(IDbConnection connection, string nodeIDQueryString)
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
                connection.ExecuteScalar(AlarmCountFormat);
                alarmTableExists = true;
            }
            catch
            {
                alarmTableExists = false;
            }

            if (alarmTableExists)
            {
                // Ensure that the alarm adapter is defined.
                alarmAdapterCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(AlarmAdapterCountFormat, nodeIDQueryString)));

                if (alarmAdapterCount == 0)
                    connection.ExecuteNonQuery(string.Format(AlarmAdapterInsertFormat, nodeIDQueryString));

                // Ensure that the alarm record is defined in the ConfigurationEntity table.
                alarmConfigEntityCount = Convert.ToInt32(connection.ExecuteScalar(AlarmConfigEntityCountFormat));

                if (alarmConfigEntityCount == 0)
                    connection.ExecuteNonQuery(AlarmConfigEntityInsertFormat);

                // Ensure that the alarm record is defined in the SignalType table.
                alarmSignalTypeCount = Convert.ToInt32(connection.ExecuteScalar(AlarmSignalTypeCountFormat));

                if (alarmSignalTypeCount == 0)
                    connection.ExecuteNonQuery(AlarmSignalTypeInsertFormat);
            }
        }

        // Gets the name of the node identified by the given node ID query string.
        private static string GetNodeName(IDbConnection connection, string nodeIDQueryString)
        {
            const string NodeNameFormat = "SELECT Name FROM Node WHERE ID = {0}";
            return connection.ExecuteScalar(string.Format(NodeNameFormat, nodeIDQueryString)).ToString();
        }

        /// <summary>
        /// Creates a parameterized query string for the underlying database type 
        /// based on the given format string and the parameter names.
        /// </summary>
        /// <param name="connectionType">The adapter type used to determine the underlying database type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="parameterNames">A string array that contains zero or more parameter names to format.</param>
        /// <returns>A parameterized query string based on the given format and parameter names.</returns>
        private static string ParameterizedQueryString(Type connectionType, string format, params string[] parameterNames)
        {
            bool oracle = connectionType.Name == "OracleConnection";
            char paramChar = oracle ? ':' : '@';
            object[] parameters = parameterNames.Select(name => paramChar + name).ToArray();
            return string.Format(format, parameters);
        }
    }
}
