//******************************************************************************************************
//  IsolatedStorageManager.cs - Gbtc
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
//  09/02/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Static class to read/write data from/to IsolatedStorage.
    /// </summary>
    public static class IsolatedStorageManager
    {
        private static IsolatedStorageFile s_userStoreForAssembly = IsolatedStorageFile.GetUserStoreForAssembly();

        /// <summary>
        /// Writes collection values by converting collection to semi-colon seperated string to IsolatedStorage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="valueList"><see cref="IEnumerable{T}"/> collection to be stored in isolated storage.</param>
        public static void WriteCollectionToIsolatedStorage(string key, IEnumerable<object> valueList)
        {
            using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(key, FileMode.Create, s_userStoreForAssembly)))
            {
                StringBuilder sb = new StringBuilder();
                foreach (object value in valueList)
                    sb.Append(value.ToString() + ";");

                writer.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Writes to isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="value">Value to be written to isolated storage.</param>
        public static void WriteToIsolatedStorage(string key, object value)
        {
            using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(key, FileMode.Create, s_userStoreForAssembly)))
                writer.Write(value.ToString());
        }

        /// <summary>
        /// Reads from isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage to read from.</param>
        /// <returns>Object from the isolated storage.</returns>
        public static object ReadFromIsolatedStorage(string key)
        {
            using (StreamReader reader = new StreamReader(new IsolatedStorageFileStream(key, FileMode.OpenOrCreate, s_userStoreForAssembly)))
            {
                if (reader != null)
                    return reader.ReadToEnd();
                else
                    return null;
            }
        }

        /// <summary>
        /// Initializes or resets existing settings for input status and monitoring screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForInputStatusMonitor(bool overWriteExisting)
        {
            if (!s_userStoreForAssembly.FileExists("ForceIPv4") || overWriteExisting || ReadFromIsolatedStorage("ForceIPv4") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("ForceIPv4").ToString()))
                WriteToIsolatedStorage("ForceIPv4", "true");

            if (!s_userStoreForAssembly.FileExists("InputMonitoringPoints") || overWriteExisting)
                WriteToIsolatedStorage("InputMonitoringPoints", string.Empty);

            if (!s_userStoreForAssembly.FileExists("NumberOfDataPointsToPlot") || overWriteExisting || ReadFromIsolatedStorage("NumberOfDataPointsToPlot") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("NumberOfDataPointsToPlot").ToString()))
                WriteToIsolatedStorage("NumberOfDataPointsToPlot", 150);

            if (!s_userStoreForAssembly.FileExists("DataResolution") || overWriteExisting || ReadFromIsolatedStorage("DataResolution") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DataResolution").ToString()))
                WriteToIsolatedStorage("DataResolution", 30);

            if (!s_userStoreForAssembly.FileExists("LagTime") || overWriteExisting || ReadFromIsolatedStorage("LagTime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("LagTime").ToString()))
                WriteToIsolatedStorage("LagTime", 3);

            if (!s_userStoreForAssembly.FileExists("LeadTime") || overWriteExisting || ReadFromIsolatedStorage("LeadTime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("LeadTime").ToString()))
                WriteToIsolatedStorage("LeadTime", 1);

            if (!s_userStoreForAssembly.FileExists("UseLocalClockAsRealtime") || overWriteExisting || ReadFromIsolatedStorage("UseLocalClockAsRealtime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("UseLocalClockAsRealtime").ToString()))
                WriteToIsolatedStorage("UseLocalClockAsRealtime", "false");

            if (!s_userStoreForAssembly.FileExists("IgnoreBadTimestamps") || overWriteExisting || ReadFromIsolatedStorage("IgnoreBadTimestamps") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("IgnoreBadTimestamps").ToString()))
                WriteToIsolatedStorage("IgnoreBadTimestamps", "false");

            if (!s_userStoreForAssembly.FileExists("ChartRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("ChartRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("ChartRefreshInterval").ToString()))
                WriteToIsolatedStorage("ChartRefreshInterval", 250);

            if (!s_userStoreForAssembly.FileExists("StatisticsDataRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("StatisticsDataRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("StatisticsDataRefreshInterval").ToString()))
                WriteToIsolatedStorage("StatisticsDataRefreshInterval", 2);

            if (!s_userStoreForAssembly.FileExists("MeasurementsDataRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("MeasurementsDataRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("MeasurementsDataRefreshInterval").ToString()))
                WriteToIsolatedStorage("MeasurementsDataRefreshInterval", 2);

            if (!s_userStoreForAssembly.FileExists("DisplayXAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayXAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayXAxis").ToString()))
                WriteToIsolatedStorage("DisplayXAxis", "false");

            if (!s_userStoreForAssembly.FileExists("DisplayFrequencyYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayFrequencyYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayFrequencyYAxis").ToString()))
                WriteToIsolatedStorage("DisplayFrequencyYAxis", "true");

            if (!s_userStoreForAssembly.FileExists("DisplayPhaseAngleYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayPhaseAngleYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayPhaseAngleYAxis").ToString()))
                WriteToIsolatedStorage("DisplayPhaseAngleYAxis", "false");

            if (!s_userStoreForAssembly.FileExists("DisplayVoltageYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayVoltageYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayVoltageYAxis").ToString()))
                WriteToIsolatedStorage("DisplayVoltageYAxis", "false");

            if (!s_userStoreForAssembly.FileExists("DisplayCurrentYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayCurrentYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayCurrentYAxis").ToString()))
                WriteToIsolatedStorage("DisplayCurrentYAxis", "false");

            if (!s_userStoreForAssembly.FileExists("FrequencyRangeMin") || overWriteExisting || ReadFromIsolatedStorage("FrequencyRangeMin") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("FrequencyRangeMin").ToString()))
                WriteToIsolatedStorage("FrequencyRangeMin", 59.95);

            if (!s_userStoreForAssembly.FileExists("FrequencyRangeMax") || overWriteExisting || ReadFromIsolatedStorage("FrequencyRangeMax") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("FrequencyRangeMax").ToString()))
                WriteToIsolatedStorage("FrequencyRangeMax", 60.05);

            if (!s_userStoreForAssembly.FileExists("DisplayLegend") || overWriteExisting || ReadFromIsolatedStorage("DisplayLegend") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayLegend").ToString()))
                WriteToIsolatedStorage("DisplayLegend", "true");
        }

        /// <summary>
        /// Initializes or resets existing settings for remote console screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForRemoteConsole(bool overWriteExisting)
        {
            if (!s_userStoreForAssembly.FileExists("NumberOfMessages") || overWriteExisting)
                WriteToIsolatedStorage("NumberOfMessages", 75);
        }

        /// <summary>
        /// Initializes or resets existing settings for stream statistics screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForStreamStatistics(bool overWriteExisting)
        {
            if (!s_userStoreForAssembly.FileExists("StreamStatisticsDataRefreshInterval") || overWriteExisting)
                WriteToIsolatedStorage("StreamStatisticsDataRefreshInterval", 10);
        }

        /// <summary>
        /// Initializes or resets existing settings for real-time measurements screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForRealTimeMeasurements(bool overWriteExisting)
        {
            if (!s_userStoreForAssembly.FileExists("RealtimeMeasurementsDataRefreshInterval") || overWriteExisting)
                WriteToIsolatedStorage("RealtimeMeasurementsDataRefreshInterval", 10);
        }

        /// <summary>
        /// Initializes or resets existing values in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeIsolatedStorage(bool overWriteExisting)
        {
            InitializeStorageForInputStatusMonitor(overWriteExisting);
            InitializeStorageForRealTimeMeasurements(overWriteExisting);
            InitializeStorageForRemoteConsole(overWriteExisting);
            InitializeStorageForStreamStatistics(overWriteExisting);
        }
    }
}
