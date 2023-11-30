//******************************************************************************************************
//  IsolatedStorageManager.cs - Gbtc
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
//  09/02/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Static class to access data settings in isolated storage.
    /// </summary>
    public static class IsolatedStorageManager
    {
        private static readonly object s_userStoreLock = new();

        /// <summary>
        /// Writes collection values by converting collection to semi-colon separated string to IsolatedStorage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="valueList"><see cref="IEnumerable{T}"/> collection to be stored in isolated storage.</param>
        public static void WriteCollectionToIsolatedStorage(string key, IEnumerable<object> valueList)
        {
            lock (s_userStoreLock)
            {
                using IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForAssembly();
                using StreamWriter writer = new(new IsolatedStorageFileStream(key, FileMode.Create, userStore));
                StringBuilder sb = new();

                foreach (object value in valueList)
                    sb.Append(value + ";");

                writer.Write(sb.ToString());
                writer.Flush();
            }
        }

        /// <summary>
        /// Writes to isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage.</param>
        /// <param name="value">Value to be written to isolated storage.</param>
        public static void WriteToIsolatedStorage(string key, object value)
        {
            lock (s_userStoreLock)
            {
                using IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForAssembly();
                using StreamWriter writer = new(new IsolatedStorageFileStream(key, FileMode.Create, userStore));
                writer.Write(value.ToString());
                writer.Flush();
            }
        }

        /// <summary>
        /// Reads from isolated storage.
        /// </summary>
        /// <param name="key">Name of the isolated storage to read from.</param>
        /// <returns>Object from the isolated storage.</returns>
        public static object ReadFromIsolatedStorage(string key)
        {
            lock (s_userStoreLock)
            {
                using IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForAssembly();
                using StreamReader reader = new(new IsolatedStorageFileStream(key, FileMode.OpenOrCreate, userStore));
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Determines if isolated storage setting exists.
        /// </summary>
        /// <param name="setting">Setting name.</param>
        /// <returns><c>true</c> if isolated storage <paramref name="setting"/> exists; otherwise, <c>false</c>.</returns>
        public static bool SettingExists(string setting)
        {
            lock (s_userStoreLock)
            {
                using IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForAssembly();
                return userStore.FileExists(setting);
            }
        }

        /// <summary>
        /// Initializes or resets existing settings for input status and monitoring screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForInputStatusMonitor(bool overWriteExisting)
        {
            if (!SettingExists("ForceIPv4") || overWriteExisting || ReadFromIsolatedStorage("ForceIPv4") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("ForceIPv4").ToString()))
                WriteToIsolatedStorage("ForceIPv4", "true");

            if (!SettingExists("InputMonitoringPoints") || overWriteExisting)
                WriteToIsolatedStorage("InputMonitoringPoints", string.Empty);

            if (!SettingExists("NumberOfDataPointsToPlot") || overWriteExisting || ReadFromIsolatedStorage("NumberOfDataPointsToPlot") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("NumberOfDataPointsToPlot").ToString()))
                WriteToIsolatedStorage("NumberOfDataPointsToPlot", 150);

            if (!SettingExists("DataResolution") || overWriteExisting || ReadFromIsolatedStorage("DataResolution") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DataResolution").ToString()))
                WriteToIsolatedStorage("DataResolution", 30);

            if (!SettingExists("LagTime") || overWriteExisting || ReadFromIsolatedStorage("LagTime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("LagTime").ToString()))
                WriteToIsolatedStorage("LagTime", 10.0D);

            if (!SettingExists("LeadTime") || overWriteExisting || ReadFromIsolatedStorage("LeadTime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("LeadTime").ToString()))
                WriteToIsolatedStorage("LeadTime", 10.0D);

            if (!SettingExists("UseLocalClockAsRealtime") || overWriteExisting || ReadFromIsolatedStorage("UseLocalClockAsRealtime") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("UseLocalClockAsRealtime").ToString()))
                WriteToIsolatedStorage("UseLocalClockAsRealtime", "false");

            if (!SettingExists("IgnoreBadTimestamps") || overWriteExisting || ReadFromIsolatedStorage("IgnoreBadTimestamps") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("IgnoreBadTimestamps").ToString()))
                WriteToIsolatedStorage("IgnoreBadTimestamps", "false");

            if (!SettingExists("ChartRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("ChartRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("ChartRefreshInterval").ToString()))
                WriteToIsolatedStorage("ChartRefreshInterval", 250);

            if (!SettingExists("StatisticsDataRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("StatisticsDataRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("StatisticsDataRefreshInterval").ToString()))
                WriteToIsolatedStorage("StatisticsDataRefreshInterval", 2);

            if (!SettingExists("MeasurementsDataRefreshInterval") || overWriteExisting || ReadFromIsolatedStorage("MeasurementsDataRefreshInterval") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("MeasurementsDataRefreshInterval").ToString()))
                WriteToIsolatedStorage("MeasurementsDataRefreshInterval", 2);

            if (!SettingExists("DisplayXAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayXAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayXAxis").ToString()))
                WriteToIsolatedStorage("DisplayXAxis", "false");

            if (!SettingExists("DisplayFrequencyYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayFrequencyYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayFrequencyYAxis").ToString()))
                WriteToIsolatedStorage("DisplayFrequencyYAxis", "true");

            if (!SettingExists("DisplayPhaseAngleYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayPhaseAngleYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayPhaseAngleYAxis").ToString()))
                WriteToIsolatedStorage("DisplayPhaseAngleYAxis", "false");

            if (!SettingExists("DisplayVoltageYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayVoltageYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayVoltageYAxis").ToString()))
                WriteToIsolatedStorage("DisplayVoltageYAxis", "false");

            if (!SettingExists("DisplayCurrentYAxis") || overWriteExisting || ReadFromIsolatedStorage("DisplayCurrentYAxis") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayCurrentYAxis").ToString()))
                WriteToIsolatedStorage("DisplayCurrentYAxis", "false");

            if (!SettingExists("FrequencyRangeMin") || overWriteExisting || ReadFromIsolatedStorage("FrequencyRangeMin") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("FrequencyRangeMin").ToString()))
                WriteToIsolatedStorage("FrequencyRangeMin", 59.95);

            if (!SettingExists("FrequencyRangeMax") || overWriteExisting || ReadFromIsolatedStorage("FrequencyRangeMax") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("FrequencyRangeMax").ToString()))
                WriteToIsolatedStorage("FrequencyRangeMax", 60.05);

            if (!SettingExists("DisplayLegend") || overWriteExisting || ReadFromIsolatedStorage("DisplayLegend") == null || string.IsNullOrEmpty(ReadFromIsolatedStorage("DisplayLegend").ToString()))
                WriteToIsolatedStorage("DisplayLegend", "true");
        }

        /// <summary>
        /// Initializes or resets existing settings for remote console screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForRemoteConsole(bool overWriteExisting)
        {
            if (!SettingExists("NumberOfMessages") || overWriteExisting)
                WriteToIsolatedStorage("NumberOfMessages", 75);
        }

        /// <summary>
        /// Initializes or resets existing settings for stream statistics screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForStreamStatistics(bool overWriteExisting)
        {
            if (!SettingExists("StreamStatisticsDataRefreshInterval") || overWriteExisting)
                WriteToIsolatedStorage("StreamStatisticsDataRefreshInterval", 5);
        }

        /// <summary>
        /// Initializes or resets existing settings for real-time measurements screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForRealTimeMeasurements(bool overWriteExisting)
        {
            if (!SettingExists("RealtimeMeasurementsDataRefreshInterval") || overWriteExisting)
                WriteToIsolatedStorage("RealtimeMeasurementsDataRefreshInterval", 5);
        }

        /// <summary>
        /// Initializes or resets existing settings for alarm status screen in <see cref="IsolatedStorageFile"/> to default values.
        /// </summary>
        /// <param name="overWriteExisting">Boolean flag indicating if existing values should be reset to default value.</param>
        public static void InitializeStorageForAlarmStatus(bool overWriteExisting)
        {
            if (!SettingExists("AlarmStatusRefreshInterval") || overWriteExisting)
                WriteToIsolatedStorage("AlarmStatusRefreshInterval", AlarmMonitor.DefaultRefreshInterval);
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
            InitializeStorageForAlarmStatus(overWriteExisting);
        }
    }
}
