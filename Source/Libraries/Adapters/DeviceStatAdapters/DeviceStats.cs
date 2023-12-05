//******************************************************************************************************
//  DeviceStats.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/19/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using DeviceStatAdapters.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GSF;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.IO;
using GSF.Parsing;
using GSF.Scheduling;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;

// ReSharper disable UnusedMember.Global
// ReSharper disable InheritdocConsiderUsage
namespace DeviceStatAdapters
{
    /// <summary>
    /// Represents an adapter that will monitor and report device alarm states.
    /// </summary>
    [Description("Device Stats Writer: Maintains detailed device statistics to a database table.")]
    public class DeviceStats : FacileActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for <see cref="DatabaseConnnectionString"/>.
        /// </summary>
        public const string DefaultDatabaseConnnectionString = "";

        /// <summary>
        /// Defines the default value for <see cref="DatabaseProviderString"/>.
        /// </summary>
        public const string DefaultDatabaseProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter";

        /// <summary>
        /// Defines the default value for <see cref="DatabaseCommand"/>.
        /// </summary>
        public const string DefaultDatabaseCommand = "";


        /// <summary>
        /// Defines the default value for <see cref="DatabaseCommandTimeout"/>.
        /// </summary>
        public const int DefaultDatabaseCommandTimeout = 240;

        /// <summary>
        /// Defines the default value for <see cref="DatabaseCommandParameters"/>.
        /// </summary>
        public const string DefaultDatabaseCommandParameters = "";

        /// <summary>
        /// Defines the default value for <see cref="DatabaseCommandSchedule"/>.
        /// </summary>
        public const string DefaultDatabaseCommandSchedule = "0 0 * * *";

        /// <summary>
        /// Defines the default value for <see cref="MinuteStatsInsertTimeout"/>.
        /// </summary>
        public const int DefaultMinuteStatsInsertTimeout = DataExtensions.DefaultTimeoutDuration;

        /// <summary>
        /// Defines the default value for <see cref="EnableTimeReasonabilityCheck"/>.
        /// </summary>
        public const bool DefaultEnableTimeReasonabilityCheck = true;

        /// <summary>
        /// Defines the default value for <see cref="PastTimeReasonabilityLimit"/>.
        /// </summary>
        public const double DefaultPastTimeReasonabilityLimit = 43200.0D;

        /// <summary>
        /// Defines the default value for <see cref="FutureTimeReasonabilityLimit"/>.
        /// </summary>
        public const double DefaultFutureTimeReasonabilityLimit = 43200.0D;

        // Nested Types
        private class MinuteCounts
        {
            public long ReceivedCount { get; set; }
            public long DataErrorCount { get; set; }
            public long TimeErrorCount { get; set; }
            public double MinLatency { get; set; } = double.NaN;
            public double MaxLatency { get; set; } = double.NaN;
            public double AvgLatencyTotal { get; set; }
            public int LatencyCount { get; set; }
        }

        // Fields
        private ShortSynchronizedOperation m_deviceSyncOperation;
        private ShortSynchronizedOperation m_databaseOperation;
        private ScheduleManager m_scheduleManager;
        private readonly ConcurrentDictionary<Guid, int> m_measurementDevice;
        private readonly ConcurrentDictionary<int, Ticks> m_deviceMinuteTime;
        private readonly ConcurrentDictionary<int, MinuteCounts> m_deviceMinuteCounts;
        private readonly HashSet<int> m_deviceMinuteInitialized;
        private long m_pastTimeReasonabilityLimit;
        private long m_futureTimeReasonabilityLimit;
        private long m_measurementTests;
        private long m_databaseWrites;
        private string m_lastDatabaseResult;
        private long m_totalDatabaseOperations;
        private object m_lastDatabaseOperationResult;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DeviceStats"/>.
        /// </summary>
        public DeviceStats()
        {
            m_measurementDevice = new ConcurrentDictionary<Guid, int>();
            m_deviceMinuteTime = new ConcurrentDictionary<int, Ticks>();
            m_deviceMinuteCounts = new ConcurrentDictionary<int, MinuteCounts>();
            m_deviceMinuteInitialized = new HashSet<int>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the external database connection string used for saving device stats.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the database connection string used for saving device stats. Leave empty to use current configured host database connection.")]
        [DefaultValue(DefaultDatabaseConnnectionString)]
        public string DatabaseConnnectionString { get; set; }

        /// <summary>
        /// Gets or sets the external database provider string used for saving device stats.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the external database provider string used for saving device stats.")]
        [DefaultValue(DefaultDatabaseProviderString)]
        public string DatabaseProviderString { get; set; }

        /// <summary>
        /// Gets or sets the command used for database operation, e.g., a stored procedure name or SQL expression like "INSERT".
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the command used for database operation, e.g., a stored procedure name or SQL expression like \"INSERT\".")]
        [DefaultValue(DefaultDatabaseCommand)]
        public string DatabaseCommand { get; set; }

        /// <summary>
        /// Gets or sets the time, in seconds, to wait for the defined database operation to complete.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the time, in seconds, to wait for the defined database operation to complete.")]
        [DefaultValue(DefaultDatabaseCommandTimeout)]
        public int DatabaseCommandTimeout { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the command that includes any desired value substitutions used for database operation. Available substitutions: {Acronym} and {Timestamp}.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the parameters for the command that includes any desired value substitutions used for database operation. Available substitutions: {Acronym} and {Timestamp}.")]
        [DefaultValue(DefaultDatabaseCommandParameters)]
        public string DatabaseCommandParameters { get; set; }

        /// <summary>
        /// Gets or sets the CRON schedule on which to execute the defined database command.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the CRON schedule on which to execute the defined database command. Defaults to once per day.")]
        [DefaultValue(DefaultDatabaseCommandSchedule)]
        public string DatabaseCommandSchedule { get; set; }

        /// <summary>
        /// Gets or sets the time, in seconds, to wait for a minute stats database insert operation to complete.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the time, in seconds, to wait for a minute stats database insert operation to complete.")]
        [DefaultValue(DefaultMinuteStatsInsertTimeout)]
        public int MinuteStatsInsertTimeout {  get; set; }

        /// <summary>
        /// Gets or sets flag that indicates if incoming timestamps to the historian should be validated for reasonability.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the flag that indicates if incoming timestamps to the historian should be validated for reasonability.")]
        [DefaultValue(DefaultEnableTimeReasonabilityCheck)]
        public bool EnableTimeReasonabilityCheck { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of seconds that a past timestamp, as compared to local clock, will be considered valid.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the maximum number of seconds that a past timestamp, as compared to local clock, will be considered valid.")]
        [DefaultValue(DefaultPastTimeReasonabilityLimit)]
        public double PastTimeReasonabilityLimit
        {
            get => new Ticks(m_pastTimeReasonabilityLimit).ToSeconds();
            set => m_pastTimeReasonabilityLimit = Ticks.FromSeconds(Math.Abs(value));
        }

        /// <summary>
        /// Gets or sets the maximum number of seconds that a future timestamp, as compared to local clock, will be considered valid.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the maximum number of seconds that a future timestamp, as compared to local clock, will be considered valid.")]
        [DefaultValue(DefaultFutureTimeReasonabilityLimit)]
        public double FutureTimeReasonabilityLimit
        {
            get => new Ticks(m_futureTimeReasonabilityLimit).ToSeconds();
            set => m_futureTimeReasonabilityLimit = Ticks.FromSeconds(Math.Abs(value));
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the adapter expects, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Automatically controlled
        public override MeasurementKey[] InputMeasurementKeys
        {
            get => base.InputMeasurementKeys;
            set => base.InputMeasurementKeys = value;
        }

        /// <summary>
        /// Gets or sets output measurements that the adapter will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Not used
        public override IMeasurement[] OutputMeasurements
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        /// <summary>
        /// Gets or sets the frames per second to be used by the <see cref="FacileActionAdapterBase"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Not used
        public override int FramesPerSecond
        {
            get => base.FramesPerSecond;
            set => base.FramesPerSecond = value;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Not used
        public new double LagTime { get; set; }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Not used
        public new double LeadTime { get; set; }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="AdapterBase"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;

                // Update device synchronization at any data source update to check for newly added devices
                QueueDeviceSync();
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine($"    Monitored Device Count: {InputMeasurementKeys?.Length ?? 0:N0}");
                status.AppendLine($"         Measurement Tests: {m_measurementTests:N0}");
                status.AppendLine($"           Database Writes: {m_databaseWrites:N0}");
                status.AppendLine($"      Last Database Result: {m_lastDatabaseResult ?? "N/A"}");

                if (!string.IsNullOrWhiteSpace(DatabaseCommand))
                {
                    status.AppendLine($"Scheduled Database Command: {DatabaseCommand}");
                    status.AppendLine($"Command Parameters, if any: {DatabaseCommandParameters}");
                    status.AppendLine($"  Database Command Timeout: {DatabaseCommandTimeout:N0} seconds");
                    status.AppendLine($"  Total Command Operations: {m_totalDatabaseOperations:N0}");
                    status.AppendLine($"       Last Command Result: {m_lastDatabaseOperationResult}");
                    
                    if (m_scheduleManager?.Schedules.Count > 0)
                        status.AppendLine(m_scheduleManager.Schedules[0].Status);
                }

                status.AppendLine($"  Time reasonability check: {(EnableTimeReasonabilityCheck ? "Enabled" : "Not Enabled")}");

                if (EnableTimeReasonabilityCheck)
                {
                    status.AppendLine($"   Maximum past time limit: {PastTimeReasonabilityLimit:N4}s, i.e., {new Ticks(m_pastTimeReasonabilityLimit).ToElapsedTimeString(4)}");
                    status.AppendLine($" Maximum future time limit: {FutureTimeReasonabilityLimit:N4}s, i.e., {new Ticks(m_futureTimeReasonabilityLimit).ToElapsedTimeString(4)}");
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DeviceStats"/> adapter and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (!(m_scheduleManager is null))
                {
                    m_scheduleManager.ScheduleDue -= ScheduleManager_ScheduleDue;
                    m_scheduleManager.Dispose();
                    m_scheduleManager = null;
                }
            }
            finally
            {
                m_disposed = true;       // Prevent duplicate dispose.
                base.Dispose(disposing); // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="DeviceStats" />.
        /// </summary>
        public override void Initialize()
        {
            const int DeviceGroupAccessID = -99999;
            
            base.Initialize();

            ConnectionStringParser parser = new ConnectionStringParser();
            parser.ParseConnectionString(ConnectionString, this);

            // Define synchronized monitoring operation
            m_deviceSyncOperation = new ShortSynchronizedOperation(() =>
            {
                List<MeasurementKey> inputMeasurementKeys = new List<MeasurementKey>();

                using (AdoDataConnection statConnection = GetDatabaseConnection())
                using (AdoDataConnection gsfConnection = new AdoDataConnection("systemSettings"))
                {
                    // Load any newly defined devices into the statistics device table
                    TableOperations<Device> deviceTable = new TableOperations<Device>(statConnection);
                    DataRow[] devices = gsfConnection.RetrieveData($"SELECT * FROM Device WHERE IsConcentrator = 0 AND AccessID <> {DeviceGroupAccessID}").Select();

                    foreach (DataRow device in devices)
                    {
                        string acronym = device["Acronym"].ToString();

                        if (string.IsNullOrWhiteSpace(acronym))
                            continue;

                        Device statDevice = deviceTable.QueryRecordWhere("Acronym = {0}", acronym) ?? deviceTable.NewRecord();

                        // If using local database, skip Device table synchronization
                        if (!string.IsNullOrWhiteSpace(DatabaseConnnectionString))
                        {
                            statDevice.UniqueID = device.ConvertField<Guid>("UniqueID");
                            statDevice.Acronym = device.ConvertField<string>("Acronym");
                            statDevice.Name = device.ConvertField<string>("Name");
                            statDevice.ParentAcronym = gsfConnection.ExecuteScalar<string>("SELECT Acronym FROM Device WHERE ID = {0}", device.ConvertField<int>("ParentID"));
                            statDevice.Protocol = gsfConnection.ExecuteScalar<string>("SELECT Acronym FROM Protocol WHERE ID = {0}", device.ConvertField<int>("ProtocolID"));
                            statDevice.Longitude = device.ConvertField<decimal>("Longitude");
                            statDevice.Latitude = device.ConvertField<decimal>("Latitude");
                            statDevice.FramesPerSecond = device.ConvertField<int>("FramesPerSecond");

                            deviceTable.AddNewOrUpdateRecord(statDevice);

                            if (statDevice.ID == 0)
                                statDevice = deviceTable.QueryRecordWhere("Acronym = {0}", acronym);
                        }

                        DataRow row = gsfConnection.RetrieveRow("SELECT SignalID, ID FROM MeasurementDetail WHERE DeviceAcronym = {0} AND SignalAcronym = 'FREQ'", acronym);

                        if (!string.IsNullOrEmpty(row.ConvertField<string>("ID")))
                        {
                            MeasurementKey key = MeasurementKey.LookUpOrCreate(row.ConvertField<Guid>("SignalID"), row.ConvertField<string>("ID"));

                            // Add measurement key
                            inputMeasurementKeys.Add(key);

                            // Update device minute maps
                            m_measurementDevice.GetOrAdd(key.SignalID, statDevice.ID);
                            m_deviceMinuteTime.GetOrAdd(statDevice.ID, 0L);
                            m_deviceMinuteCounts.GetOrAdd(statDevice.ID, new MinuteCounts());
                        }
                    }
                }

                // Load desired input measurements
                InputMeasurementKeys = inputMeasurementKeys.ToArray();
            },
            ex => OnProcessException(MessageLevel.Warning, ex));

            m_deviceSyncOperation.Run();

            // Setup optional scheduled database command operation
            if (string.IsNullOrWhiteSpace(DatabaseCommand))
                return;

            m_databaseOperation = new ShortSynchronizedOperation(() =>
            {
                OnStatusMessage(MessageLevel.Info, $"Executing scheduled database operation \"{DatabaseCommand}\"...");

                using (AdoDataConnection connection = GetDatabaseConnection())
                {
                    List<object> parameters = new List<object>();

                    if (!string.IsNullOrWhiteSpace(DatabaseCommandParameters))
                    {
                        TemplatedExpressionParser parameterTemplate = new TemplatedExpressionParser
                        {
                            TemplatedExpression = DatabaseCommandParameters
                        };

                        Dictionary<string, string> substitutions = new Dictionary<string, string>
                        {
                            ["{Acronym}"] = Name,
                            ["{Timestamp}"] = RealTime.ToString(TimeTagBase.DefaultFormat)
                        };

                        string[] commandParameters = parameterTemplate.Execute(substitutions).Split(',');

                        // Do some basic typing on command parameters
                        foreach (string commandParameter in commandParameters)
                        {
                            string parameter = commandParameter.Trim();

                            if (parameter.StartsWith("'") && parameter.EndsWith("'"))
                                parameters.Add(parameter.Length > 2 ? parameter.Substring(1, parameter.Length - 2) : "");
                            else if (int.TryParse(parameter, out int ival))
                                parameters.Add(ival);
                            else if (double.TryParse(parameter, out double dval))
                                parameters.Add(dval);
                            else if (bool.TryParse(parameter, out bool bval))
                                parameters.Add(bval);
                            else if (DateTime.TryParseExact(parameter, TimeTagBase.DefaultFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dtval))
                                parameters.Add(dtval);
                            else if (DateTime.TryParse(parameter, out dtval))
                                parameters.Add(dtval);
                            else
                                parameters.Add(parameter);
                        }
                    }

                    try
                    {
                        m_lastDatabaseOperationResult = connection.ExecuteScalar(DatabaseCommandTimeout, DatabaseCommand, parameters.ToArray());
                        m_totalDatabaseOperations++;
                    }
                    catch (Exception ex)
                    {
                        m_lastDatabaseOperationResult = $"ERROR: {ex.Message}";
                    }
                }
            },
            ex => OnProcessException(MessageLevel.Warning, ex));

            m_scheduleManager = new ScheduleManager();
            m_scheduleManager.ScheduleDue += ScheduleManager_ScheduleDue;
            m_scheduleManager.AddSchedule($"{Name}:{nameof(DeviceStats)}DatabaseOperation", DatabaseCommandSchedule);
            m_scheduleManager.Start();
        }

        private void ScheduleManager_ScheduleDue(object sender, EventArgs<Schedule> e) => 
            m_databaseOperation?.RunOnceAsync();

        /// <summary>
        /// Queues database sync operation for device meta-data.
        /// </summary>
        [AdapterCommand("Queues database sync operation for device meta-data.", "Administrator", "Editor")]
        public void QueueDeviceSync() =>
            m_deviceSyncOperation?.RunOnceAsync();

        /// <summary>
        /// Queues database operation for execution. Operation will execute immediately if not already running.
        /// </summary>
        [AdapterCommand("Queues database operation for execution. Operation will execute immediately if not already running.", "Administrator", "Editor")]
        public void QueueDatabaseOperation() =>
            m_databaseOperation?.RunOnceAsync();

        /// <summary>
        /// Gets a short one-line status of this adapter.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this adapter.</returns>
        public override string GetShortStatus(int maxLength) => 
            (Enabled ? $"Actively counting statistics for {InputMeasurementKeys.Length:N0} devices, {m_databaseWrites:N0} database writes so far..." : "Adapter is not running").CenterText(maxLength);

        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            Ticks currentTime = DateTime.UtcNow.Ticks;

            foreach (IMeasurement measurement in measurements)
            {
                // Validate timestamp reasonability as compared to local clock, when enabled
                if (EnableTimeReasonabilityCheck)
                {
                    long deviation = DateTime.UtcNow.Ticks - measurement.Timestamp.Value;

                    if (deviation < -m_futureTimeReasonabilityLimit || deviation > m_pastTimeReasonabilityLimit)
                        continue;
                }

                if (!m_measurementDevice.TryGetValue(measurement.ID, out int deviceID))
                    continue;

                if (!m_deviceMinuteTime.TryGetValue(deviceID, out Ticks lastMinuteTime))
                    continue;

                Ticks minuteTime = measurement.Timestamp.BaselinedTimestamp(BaselineTimeInterval.Minute);

                if (minuteTime > lastMinuteTime)
                {
                    if (lastMinuteTime > 0L)
                    {
                        // Skip first minute of data, it will likely not contain a full set of data
                        if (m_deviceMinuteInitialized.Contains(deviceID))
                            WriteDeviceMinuteCounts(deviceID, lastMinuteTime, m_deviceMinuteCounts[deviceID]);
                        else
                            m_deviceMinuteInitialized.Add(deviceID);
                    }

                    // Reset counts
                    m_deviceMinuteTime[deviceID] = minuteTime;
                    m_deviceMinuteCounts[deviceID] = new MinuteCounts();
                }

                if (m_deviceMinuteCounts.TryGetValue(deviceID, out MinuteCounts counts))
                {
                    // Increment counts
                    counts.ReceivedCount++;
                    counts.DataErrorCount += measurement.ValueQualityIsGood() ? 0L : 1L;
                    counts.TimeErrorCount += measurement.TimestampQualityIsGood() ? 0L : 1L;

                    // Track latency statistics
                    double latency = (currentTime - measurement.Timestamp).ToMilliseconds();

                    if (double.IsNaN(counts.MinLatency) || latency < counts.MinLatency)
                        counts.MinLatency = latency;

                    if (double.IsNaN(counts.MaxLatency) || latency > counts.MaxLatency)
                        counts.MaxLatency = latency;

                    counts.AvgLatencyTotal += latency;
                    counts.LatencyCount++;

                    m_measurementTests++;
                }
            }
        }

        private void WriteDeviceMinuteCounts(int deviceID, Ticks minuteTime, MinuteCounts counts)
        {
            try
            {
                using (AdoDataConnection connection = GetDatabaseConnection(MinuteStatsInsertTimeout))
                {
                    TableOperations<MinuteStats> minuteStatsTable = new TableOperations<MinuteStats>(connection);

                    MinuteStats record = minuteStatsTable.NewRecord();

                    record.DeviceID = deviceID;
                    record.Timestamp = minuteTime;
                    record.ReceivedCount = counts.ReceivedCount;
                    record.DataErrorCount = counts.DataErrorCount;
                    record.TimeErrorCount = counts.TimeErrorCount;
                    record.MinLatency = (int)Math.Round(counts.MinLatency);
                    record.MaxLatency = (int)Math.Round(counts.MaxLatency);
                    record.AvgLatency = (int)Math.Round(counts.AvgLatencyTotal / counts.LatencyCount);

                    minuteStatsTable.AddNewRecord(record);

                    m_databaseWrites++;
                    m_lastDatabaseResult = "Success";
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Error, ex);
                m_lastDatabaseResult = ex.Message;
            }
        }

        private AdoDataConnection GetDatabaseConnection(int? timeout = null) => string.IsNullOrWhiteSpace(DatabaseConnnectionString) ?
            new AdoDataConnection("systemSettings") { DefaultTimeout = timeout ?? DataExtensions.DefaultTimeoutDuration } :
            new AdoDataConnection(DatabaseConnnectionString, DatabaseProviderString) { DefaultTimeout = timeout ?? DataExtensions.DefaultTimeoutDuration };

        #endregion

        #region [ Static ]

        // Static Constructor
        static DeviceStats()
        {
            try
            {
                RestoreSQLScripts();
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        // Static Methods
        private static void RestoreSQLScripts()
        {
            Assembly executingAssembly = typeof(DeviceStats).Assembly;
            string targetPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""));

            // This simple file restoration assumes embedded resources to restore are in root namespace
            foreach (string name in executingAssembly.GetManifestResourceNames().Where(name => name.EndsWith(".sql")))
            {
                using (Stream resourceStream = executingAssembly.GetManifestResourceStream(name))
                {
                    if (resourceStream is null)
                        continue;

                    string sourceNamespace = $"{nameof(DeviceStatAdapters)}.";
                    string filePath = name;

                    // Remove namespace prefix from resource file name
                    if (filePath.StartsWith(sourceNamespace))
                        filePath = filePath.Substring(sourceNamespace.Length);

                    string targetFileName = Path.Combine(targetPath, filePath);
                    bool restoreFile = true;

                    if (File.Exists(targetFileName))
                    {
                        string resourceMD5 = GetMD5HashFromStream(resourceStream);
                        resourceStream.Seek(0, SeekOrigin.Begin);
                        restoreFile = !resourceMD5.Equals(GetMD5HashFromFile(targetFileName));
                    }

                    if (!restoreFile)
                        continue;

                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, (int)resourceStream.Length);

                    using (StreamWriter writer = File.CreateText(targetFileName))
                        writer.Write(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                }
            }
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
                return GetMD5HashFromStream(stream);
        }

        private static string GetMD5HashFromStream(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }
        
        #endregion
    }
}