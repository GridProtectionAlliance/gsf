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
using GSF;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

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

        // Fields
        private ShortSynchronizedOperation m_deviceSyncOperation;
        private readonly ConcurrentDictionary<Guid, int> m_measurementDevice;
        private readonly ConcurrentDictionary<int, Ticks> m_deviceMinuteTime;
        private readonly ConcurrentDictionary<int, Tuple<long, long, long>> m_deviceMinuteCounts;
        private readonly HashSet<int> m_deviceMinuteInitialized;
        private long m_measurementTests;
        private long m_databaseWrites;
        private string m_lastDatabaseResult;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DeviceStats"/>.
        /// </summary>
        public DeviceStats()
        {
            m_measurementDevice = new ConcurrentDictionary<Guid, int>();
            m_deviceMinuteTime = new ConcurrentDictionary<int, Ticks>();
            m_deviceMinuteCounts = new ConcurrentDictionary<int, Tuple<long, long, long>>();
            m_deviceMinuteInitialized = new HashSet<int>();
        }

        #endregion
        
        #region [ Properties ]

        /// <summary>
        /// Gets or sets the external database connection string used for saving device stats.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the database connection string used for saving device stats.")]
        public string DatabaseConnnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the external database provider string used for saving device stats.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the external database provider string used for saving device stats.")]
        [DefaultValue("AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter")]
        public string DatabaseProviderString
        {
            get;
            set;
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
        public new double LagTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Not used
        public new double LeadTime
        {
            get;
            set;
        }

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
                status.AppendFormat("    Monitored Device Count: {0:N0}", InputMeasurementKeys?.Length ?? 0);
                status.AppendLine();
                status.AppendFormat("         Measurement Tests: {0:N0}", m_measurementTests);
                status.AppendLine();
                status.AppendFormat("           Database Writes: {0:N0}", m_databaseWrites);
                status.AppendLine();
                status.AppendFormat("      Last Database Result: {0}", m_lastDatabaseResult ?? "N/A");
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="DeviceStats" />.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            ConnectionStringParser parser = new ConnectionStringParser();
            parser.ParseConnectionString(ConnectionString, this);

            // Define synchronized monitoring operation
            m_deviceSyncOperation = new ShortSynchronizedOperation(() =>
            {
                List<MeasurementKey> inputMeasurementKeys = new List<MeasurementKey>();

                using (AdoDataConnection statConnection = new AdoDataConnection(DatabaseConnnectionString, DatabaseProviderString))
                using (AdoDataConnection gsfConnection = new AdoDataConnection("systemSettings"))
                {
                    // Load any newly defined devices into the statistics device table
                    TableOperations<Device> deviceTable = new TableOperations<Device>(statConnection);
                    DataRow[] devices = gsfConnection.RetrieveData("SELECT * FROM Device WHERE IsConcentrator = 0").Select();

                    foreach (DataRow device in devices)
                    {
                        Device statDevice = deviceTable.QueryRecordWhere("Acronym = {0}", device["Acronym"]) ?? deviceTable.NewRecord();
                      
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
                            statDevice = deviceTable.QueryRecordWhere("Acronym = {0}", device["Acronym"]);

                        DataRow row = gsfConnection.RetrieveRow("SELECT SignalID, ID FROM MeasurementDetail WHERE DeviceAcronym = {0} AND SignalAcronym = 'FREQ'", statDevice.Acronym);

                        if (!string.IsNullOrEmpty(row.ConvertField<string>("ID")))
                        {
                            MeasurementKey key = MeasurementKey.LookUpOrCreate(row.ConvertField<Guid>("SignalID"), row.ConvertField<string>("ID"));

                            // Add measurement key
                            inputMeasurementKeys.Add(key);

                            // Update device minute maps
                            m_measurementDevice.GetOrAdd(key.SignalID, statDevice.ID);
                            m_deviceMinuteTime.GetOrAdd(statDevice.ID, 0L);
                            m_deviceMinuteCounts.GetOrAdd(statDevice.ID, new Tuple<long, long, long>(0L, 0L, 0L));
                        }
                    }
                }

                // Load desired input measurements
                InputMeasurementKeys = inputMeasurementKeys.ToArray();
            },
            exception => OnProcessException(MessageLevel.Warning, exception));

            m_deviceSyncOperation.Run();
        }

        /// <summary>
        /// Queues database sync operation for device meta-data.
        /// </summary>
        [AdapterCommand("Queues database sync operation for device meta-data.", "Administrator", "Editor")]
        public void QueueDeviceSync() => m_deviceSyncOperation?.RunOnceAsync();

        /// <summary>
        /// Gets a short one-line status of this adapter.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this adapter.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return (Enabled ? $"Actively counting statistics for {InputMeasurementKeys.Length:N0} devices, {m_databaseWrites:N0} database writes so far..." : "Adapter is not running").CenterText(maxLength);
        }

        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
            {
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
                    m_deviceMinuteCounts[deviceID] = new Tuple<long, long, long>(0L, 0L, 0L);
                }

                Tuple<long, long, long> counts = m_deviceMinuteCounts[deviceID];

                // Increment counts
                m_deviceMinuteCounts[deviceID] = new Tuple<long, long, long>(
                    /*  ReceivedCount */ counts.Item1 + 1L,
                    /* DataErrorCount */ counts.Item2 + (measurement.ValueQualityIsGood() ? 0L : 1L),
                    /* TimeErrorCount */ counts.Item3 + (measurement.TimestampQualityIsGood() ? 0L : 1L));

                m_measurementTests++;
            }
        }

        private void WriteDeviceMinuteCounts(int deviceID, Ticks minuteTime, Tuple<long, long, long> counts)
        {
            try
            {
                using (AdoDataConnection connection = new AdoDataConnection(DatabaseConnnectionString, DatabaseProviderString))
                {
                    TableOperations<MinuteStats> minuteStatsTable = new TableOperations<MinuteStats>(connection);

                    MinuteStats record = minuteStatsTable.NewRecord();

                    record.DeviceID = deviceID;
                    record.Timestamp = minuteTime;
                    record.ReceivedCount = counts.Item1;
                    record.DataErrorCount = counts.Item2;
                    record.TimeErrorCount = counts.Item3;

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

        #endregion
    }
}