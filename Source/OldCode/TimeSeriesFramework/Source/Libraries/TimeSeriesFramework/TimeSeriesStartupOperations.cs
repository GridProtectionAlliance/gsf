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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TVA;
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
            ValidateAlarming(connection, nodeIDQueryString);
        }

        /// <summary>
        /// Data operation to validate and ensure that certain records
        /// that are required for alarming exist in the database.
        /// </summary>
        private static void ValidateAlarming(IDbConnection connection, string nodeIDQueryString)
        {
            // SELECT queries
            const string AlarmCountFormat = "SELECT COUNT(*) FROM Alarm";
            const string AlarmAdapterCountFormat = "SELECT COUNT(*) FROM IaonActionAdapter WHERE AdapterName = 'ALARM!SERVICES' AND NodeID = {0}";
            const string AlarmConfigEntityCountFormat = "SELECT COUNT(*) FROM ConfigurationEntity WHERE SourceName = 'Alarm'";
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
    }
}
