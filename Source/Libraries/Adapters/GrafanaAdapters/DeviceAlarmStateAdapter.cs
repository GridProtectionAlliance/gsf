//******************************************************************************************************
//  DeviceAlarmStateAdapter.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  07/18/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//  27/11/2019 - C. Lackner
//      Moved Adapter to GSF
//
//******************************************************************************************************

using GrafanaAdapters.Model.Database;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using AlarmStateRecord = GrafanaAdapters.Model.Database.AlarmState;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;
using Timer = System.Timers.Timer;

namespace GrafanaAdapters;

/// <summary>
/// Represents an adapter that will monitor and report device alarm states.
/// </summary>
[Description("Device Alarm State: Monitors and updates alarm states for devices")]
public class DeviceAlarmStateAdapter : FacileActionAdapterBase
{
    #region [ Members ]

    private enum AlarmState
    {
        Good,           // Everything is kosher
        Alarm,          // Not available for longer than configured alarm time
        NotAvailable,   // Data is missing or timestamp is outside lead/lag time range
        BadData,        // Quality flags report bad data
        BadTime,        // Quality flags report bad time
        OutOfService,   // Source device is disabled
        Acknowledged    // Any issues are considered acknowledged, do not report issues
    }

    // Constants

    /// <summary>
    /// Defines the default value for the <see cref="MonitoringRate"/>.
    /// </summary>
    public const int DefaultMonitoringRate = 30000;

    /// <summary>
    /// Defines the default value for the <see cref="AlarmMinutes"/>.
    /// </summary>
    public const double DefaultAlarmMinutes = 10.0D;

    /// <summary>
    /// Defines the default value for the <see cref="AcknowledgedTransitionHysteresisDelay"/>.
    /// </summary>
    public const double DefaultAcknowledgedTransitionHysteresisDelay = 30.0D;

    /// <summary>
    /// Defines the default value for the <see cref="DefaultExternalDatabaseHysteresisDelay"/>.
    /// </summary>
    public const double DefaultExternalDatabaseHysteresisDelay = 5.0D;

    // Fields
    private Timer m_monitoringTimer;
    private ShortSynchronizedOperation m_monitoringOperation;
    private Dictionary<AlarmState, AlarmStateRecord> m_alarmStates;
    private Dictionary<int, AlarmState> m_alarmStateIDs;
    private Dictionary<int, MeasurementKey[]> m_deviceMeasurementKeys;
    private Dictionary<int, DataRow> m_deviceMetadata;
    private Dictionary<MeasurementKey, Ticks> m_lastDeviceDataUpdates;
    private FileBackedDictionary<int, long> m_lastDeviceStateChange;
    private Dictionary<int, Ticks> m_lastAcknowledgedTransition;
    private Ticks m_lastExternalDatabaseStateChange;
    private Dictionary<AlarmState, string> m_mappedAlarmStates;
    private Dictionary<AlarmState, int> m_stateCounts;
    private List<int> m_compositeStates;
    private object m_stateCountLock;
    private Ticks m_alarmTime;
    private long m_alarmStateUpdates;
    private long m_externalDatabaseUpdates;
    private object m_lastExternalDatabaseResult;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="DeviceAlarmStateAdapter"/>.
    /// </summary>
    public DeviceAlarmStateAdapter()
    {
        m_alarmTime = TimeSpan.FromMinutes(DefaultAlarmMinutes).Ticks;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets monitoring rate, in milliseconds, for devices.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines overall monitoring rate, in milliseconds, for devices.")]
    [DefaultValue(DefaultMonitoringRate)]
    public int MonitoringRate { get; set; }

    /// <summary>
    /// Gets or sets the time, in minutes, for which to change the device state to alarm when no data is received.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the time, in minutes, for which to change the device state to alarm when no data is received.")]
    [DefaultValue(DefaultAlarmMinutes)]
    public double AlarmMinutes
    {
        get => m_alarmTime.ToMinutes();
        set => m_alarmTime = TimeSpan.FromMinutes(value).Ticks;
    }

    /// <summary>
    /// Gets or sets the flag that determines if alarm states should only target parent devices, i.e., PDCs and direct connect PMUs, or all devices.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if alarm states should only target parent devices, i.e., PDCs and direct connect PMUs, or all devices.")]
    public bool TargetParentDevices { get; set; }

    /// <summary>
    /// Gets or sets delay time, in minutes, before transitioning the Acknowledged state back to Good.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the delay time, in minutes, before transitioning the Acknowledged state back to Good.")]
    [DefaultValue(DefaultAcknowledgedTransitionHysteresisDelay)]
    public double AcknowledgedTransitionHysteresisDelay { get; set; }

    /// <summary>
    /// Gets or sets the delay time, in minutes, before reporting the external database state.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the delay time, in minutes, before reporting the external database state.")]
    [DefaultValue(DefaultExternalDatabaseHysteresisDelay)]
    public double ExternalDatabaseHysteresisDelay { get; set; }

    /// <summary>
    /// Gets or sets the minimum state for application of <see cref="ExternalDatabaseHysteresisDelay"/>. Defaults to setting Good and Alarm states immediately and applying delay to all other states.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the minimum state for application of \"ExternalDatabaseHysteresisDelay\". Defaults to setting Good and Alarm states immediately and applying delay to all other states.")]
    [DefaultValue(1)]
    public int ExternalDatabaseHysteresisMinimumState { get; set; } = 1;

    /// <summary>
    /// Gets or sets the flag that determines if an external database connection should be enabled for synchronization of alarm states.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if an external database connection should be enabled for synchronization of alarm states.")]
    [DefaultValue(false)]
    public bool EnableExternalDatabaseSynchronization { get; set; }

    /// <summary>
    /// Gets or sets the external database connection string used for synchronization of alarm states. Leave blank to use local configuration database defined in "systemSettings".
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the external database connection string used for synchronization of alarm states. Leave blank to use local configuration database defined in \"systemSettings\".")]
    [DefaultValue("")]
    public string ExternalDatabaseConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the external database provider string used for synchronization of alarm states.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the external database provider string used for synchronization of alarm states.")]
    [DefaultValue("AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter")]
    public string ExternalDatabaseProviderString { get; set; }

    /// <summary>
    /// Gets or sets the external database command used for synchronization of alarm states.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the external database command used for synchronization of alarm states.")]
    [DefaultValue("sp_LogSsamEvent")]
    public string ExternalDatabaseCommand { get; set; }

    /// <summary>
    /// Gets or sets the external database command parameters with value substitutions used for synchronization of alarm states.
    /// </summary>
    /// <remarks>
    /// Examples for composite state reporting:
    /// <code>
    /// 'openPDC Overall Device Status = {AlarmState}[?{AlarmState}!=Good[ -- for \[{Device.Acronym}\]]]'
    /// </code>
    /// <code>
    /// 'Good = {GoodStateCount} / Alarmed = {AlarmStateCount} / Unavailable = {NotAvailableStateCount} / Bad Data = {BadDataStateCount} / Bad Time = {BadTimeStateCount} / Out of Service = {OutOfServiceStateCount}[?{AlarmState}!=Good[ -- \&lt;a href=\"http://localhost:8280/DeviceStatus.cshtml?DeviceID={Device.ID}\"\&gt;\[{Device.Acronym}\] Device Status\&lt;/a\&gt;]]'
    /// </code>
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines the external database command parameters with value substitutions used for synchronization of alarm states.")]
    [DefaultValue("{MappedAlarmState},1,'PDC_DEVICE_{Device.Acronym}','','openPDC Device Status = {AlarmState} for \\[{Device.Acronym}\\]','\\<a href=\"http://localhost:8280/DeviceStatus.cshtml?DeviceID={Device.ID}\"\\>\\[{Device.Acronym}\\] Device Status\\</a\\>'")]
    public string ExternalDatabaseCommandParameters { get; set; }

    /// <summary>
    /// Gets or sets the external database mapped alarm states defining the {MappedAlarmState} command parameter substitution parameter used for synchronization of alarm states.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the external database mapped alarm states defining the {MappedAlarmState} command parameter substitution parameter used for synchronization of alarm states.")]
    [DefaultValue("Good=1,Alarm=3,NotAvailable=2,BadData=3,BadTime=3,OutOfService=5")]
    public string ExternalDatabaseMappedAlarmStates { get; set; }

    /// <summary>
    /// Gets or sets the flag that determines if external database should report a single composite state or a state for each device.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if external database should report a single composite state or a state for each device.")]
    [DefaultValue(false)]
    public bool ExternalDatabaseReportSingleCompositeState { get; set; }

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
    [EditorBrowsable(EditorBrowsableState.Never)] // Automatically controlled
    public override IMeasurement[] OutputMeasurements
    {
        get => base.OutputMeasurements;
        set => base.OutputMeasurements = value;
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
            StringBuilder status = new();

            status.Append(base.Status);
            status.AppendLine($"           Monitoring Rate: {MonitoringRate:N0}ms");
            status.AppendLine($"        Monitoring Enabled: {MonitoringEnabled}");
            status.AppendLine($"  Targeting Parent Devices: {TargetParentDevices}");
            status.AppendLine($"    Monitored Device Count: {InputMeasurementKeys?.Length ?? 0:N0}");
            status.AppendLine($"     No Data Alarm Timeout: {m_alarmTime.ToElapsedTimeString(2)}");
            status.AppendLine($"       Alarm State Updates: {m_alarmStateUpdates:N0}");
            status.AppendLine($" External Database Updates: {m_externalDatabaseUpdates:N0}");
            status.AppendLine($"   Last External DB Result: {m_lastExternalDatabaseResult?.ToString() ?? "null"}");

            lock (m_stateCountLock)
            {
                status.AppendLine($"              Good Devices: {m_stateCounts[AlarmState.Good]:N0}");
                status.AppendLine($"           Alarmed Devices: {m_stateCounts[AlarmState.Alarm]:N0}");
                status.AppendLine($"       Unavailable Devices: {m_stateCounts[AlarmState.NotAvailable]:N0}");
                status.AppendLine($"Devices Reporting Bad Data: {m_stateCounts[AlarmState.BadData]:N0}");
                status.AppendLine($"Devices Reporting Bad Time: {m_stateCounts[AlarmState.BadTime]:N0}");
                status.AppendLine($"    Out of Service Devices: {m_stateCounts[AlarmState.OutOfService]:N0}");
                status.AppendLine($"      Acknowledged Devices: {m_stateCounts[AlarmState.Acknowledged]:N0}");
            }

            return status.ToString();
        }
    }

    private bool MonitoringEnabled => Enabled && (m_monitoringTimer?.Enabled ?? false);

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DeviceAlarmStateAdapter"/> object and optionally releases the managed resources.
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

            if (m_monitoringTimer is not null)
            {
                m_monitoringTimer.Enabled = false;
                m_monitoringTimer.Elapsed -= MonitoringTimer_Elapsed;
                m_monitoringTimer.Dispose();
            }

            m_lastDeviceStateChange?.Dispose();
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Initializes <see cref="DeviceAlarmStateAdapter" />.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        ConnectionStringParser parser = new();
        parser.ParseConnectionString(ConnectionString, this);

        m_alarmStates = new Dictionary<AlarmState, AlarmStateRecord>();
        m_alarmStateIDs = new Dictionary<int, AlarmState>();
        m_deviceMeasurementKeys = new Dictionary<int, MeasurementKey[]>();
        m_deviceMetadata = new Dictionary<int, DataRow>();
        m_lastDeviceDataUpdates = new Dictionary<MeasurementKey, Ticks>();
        m_lastDeviceStateChange = new FileBackedDictionary<int, long>(FilePath.GetAbsolutePath($"{Name}_LastStateChangeCache.bin".RemoveInvalidFileNameCharacters()));
        m_lastAcknowledgedTransition = new Dictionary<int, Ticks>();
        m_lastExternalDatabaseStateChange = 0L;
        m_mappedAlarmStates = new Dictionary<AlarmState, string>();
        m_stateCounts = CreateNewStateCountsMap();
        m_compositeStates = new List<int>();
        m_stateCountLock = new object();

        LoadAlarmStates();

        // Parse external database mapped alarm states, if defined
        if (!string.IsNullOrEmpty(ExternalDatabaseMappedAlarmStates))
        {
            string[] mappings = ExternalDatabaseMappedAlarmStates.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string mapping in mappings)
            {
                string[] parts = mapping.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2 && Enum.TryParse(parts[0].Trim(), out AlarmState state))
                    m_mappedAlarmStates[state] = parts[1].Trim();
            }
        }

        // Define synchronized monitoring operation
        m_monitoringOperation = new ShortSynchronizedOperation(MonitoringOperation, exception => OnProcessException(MessageLevel.Warning, exception));

        // Define monitoring timer
        m_monitoringTimer = new Timer(MonitoringRate)
        {
            AutoReset = true
        };

        m_monitoringTimer.Elapsed += MonitoringTimer_Elapsed;
        m_monitoringTimer.Enabled = true;
    }

    private void LoadAlarmStates(bool reload = false)
    {
        const int DeviceGroupAccessID = -99999;

        using AdoDataConnection connection = new("systemSettings");

        // Load alarm state map - this defines database state ID and custom color for each alarm state
        TableOperations<AlarmStateRecord> alarmStateTable = new(connection);
        AlarmStateRecord[] alarmStateRecords = alarmStateTable.QueryRecords().ToArray();

        foreach (AlarmState alarmState in Enum.GetValues(typeof(AlarmState)))
        {
            AlarmStateRecord alarmStateRecord = alarmStateRecords.FirstOrDefault(record => record.State.RemoveWhiteSpace().Equals(alarmState.ToString(), StringComparison.OrdinalIgnoreCase));

            if (alarmStateRecord is null)
            {
                alarmStateRecord = alarmStateTable.NewRecord();
                alarmStateRecord.State = alarmState.ToString();
                alarmStateRecord.Color = "white";
            }

            m_alarmStates[alarmState] = alarmStateRecord;
            m_alarmStateIDs[alarmStateRecord.ID] = alarmState;
        }

        // Define SQL expression for direct connect and parent devices or all direct connect and child devices
        string deviceSQL = TargetParentDevices ?
            "SELECT * FROM Device WHERE (IsConcentrator != 0 OR ParentID IS NULL) AND ID NOT IN (SELECT DeviceID FROM AlarmDevice)" :
            $"SELECT * FROM Device WHERE IsConcentrator = 0 AND AccessID <> {DeviceGroupAccessID} AND ID NOT IN (SELECT DeviceID FROM AlarmDevice)";

        // Load any newly defined devices into the alarm device table
        TableOperations<AlarmDevice> alarmDeviceTable = new(connection);
        DataRow[] newDevices = connection.RetrieveData(deviceSQL).Select();

        foreach (DataRow newDevice in newDevices)
        {
            AlarmDevice alarmDevice = alarmDeviceTable.NewRecord();

            bool enabled = newDevice["Enabled"].ToString().ParseBoolean();

            alarmDevice.DeviceID = newDevice.ConvertField<int>("ID");
            alarmDevice.StateID = enabled ? m_alarmStates[AlarmState.NotAvailable].ID : m_alarmStates[AlarmState.OutOfService].ID;
            alarmDevice.DisplayData = enabled ? "0" : GetOutOfServiceTime(newDevice);

            alarmDeviceTable.AddNewRecord(alarmDevice);

            // Foreign key relationship with Device table with delete cascade should ensure automatic removals
        }

        List<MeasurementKey> inputMeasurementKeys = new();

        // Load measurement signal ID to alarm device map
        foreach (AlarmDevice alarmDevice in alarmDeviceTable.QueryRecords())
        {
            MeasurementKey[] keys = null;
            DataRow metadata = connection.RetrieveRow("SELECT * FROM Device WHERE ID = {0}", alarmDevice.DeviceID);

            if (metadata is not null)
            {
                // Querying from MeasurementDetail because we also want to include disabled device measurements
                string measurementSQL = TargetParentDevices ?
                    "SELECT MeasurementDetail.SignalID AS SignalID, MeasurementDetail.ID AS ID FROM MeasurementDetail INNER JOIN DeviceDetail ON MeasurementDetail.DeviceID = DeviceDetail.ID WHERE (DeviceDetail.Acronym = {0} OR DeviceDetail.ParentAcronym = {0}) AND MeasurementDetail.SignalAcronym = 'FREQ'" :
                    "SELECT SignalID, ID FROM MeasurementDetail WHERE DeviceAcronym = {0} AND SignalAcronym = 'FREQ'";

                DataTable table = connection.RetrieveData(measurementSQL, metadata.ConvertField<string>("Acronym"));

                // ReSharper disable once AccessToDisposedClosure
                keys = table.AsEnumerable().Select(row => MeasurementKey.LookUpOrCreate(connection.Guid(row, "SignalID"), row["ID"].ToString())).ToArray();
            }

            if (keys?.Length > 0)
            {
                inputMeasurementKeys.AddRange(keys);
                m_deviceMeasurementKeys[alarmDevice.DeviceID] = keys;
                m_deviceMetadata[alarmDevice.DeviceID] = metadata;

                if (!m_lastDeviceStateChange.ContainsKey(alarmDevice.DeviceID))
                    m_lastDeviceStateChange.Add(alarmDevice.DeviceID, alarmDevice.TimeStamp.Ticks);

                foreach (MeasurementKey key in keys)
                {
                    if (reload)
                    {
                        if (!m_lastDeviceDataUpdates.ContainsKey(key))
                            m_lastDeviceDataUpdates.Add(key, DateTime.UtcNow.Ticks);
                    }
                    else
                    {
                        m_lastDeviceDataUpdates[key] = DateTime.UtcNow.Ticks;
                    }
                }
            }
            else
            {
                // Mark alarm record as unavailable if no frequency measurement is available for device
                alarmDevice.StateID = m_alarmStates[AlarmState.NotAvailable].ID;
                alarmDevice.DisplayData = GetOutOfServiceTime(metadata);
                alarmDeviceTable.UpdateRecord(alarmDevice);
            }
        }

        // Load desired input measurements
        InputMeasurementKeys = inputMeasurementKeys.ToArray();
        TrackLatestMeasurements = true;
    }

    /// <summary>
    /// Queues monitoring operation to update alarm state for immediate execution.
    /// </summary>
    [AdapterCommand("Queues monitoring operation to update alarm state for immediate execution.", "Administrator", "Editor")]
    public void QueueStateUpdate()
    {
        m_monitoringOperation?.RunOnceAsync();
    }

    /// <summary>
    /// Updates alarm states from database.
    /// </summary>
    [AdapterCommand("Updates alarm states from database.", "Administrator", "Editor")]
    public void UpdateAlarmStates()
    {
        if (!Initialized)
            return;

        lock (m_alarmStates)
            LoadAlarmStates(true);
    }

    /// <summary>
    /// Gets a short one-line status of this adapter.
    /// </summary>
    /// <param name="maxLength">Maximum number of available characters for display.</param>
    /// <returns>A short one-line summary of the current status of this adapter.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return MonitoringEnabled
            ? $"Monitoring enabled for every {Ticks.FromMilliseconds(MonitoringRate).ToElapsedTimeString()}"
                .CenterText(maxLength)
            : "Monitoring is disabled...".CenterText(maxLength);
    }

    private void MonitoringOperation()
    {
        lock (m_alarmStates)
        {
            ImmediateMeasurements measurements = LatestMeasurements;
            List<AlarmDevice> alarmDeviceUpdates = new();
            Dictionary<AlarmState, int> stateCounts = CreateNewStateCountsMap();

            OnStatusMessage(MessageLevel.Info, "Updating device alarm states");

            using (AdoDataConnection connection = new("systemSettings"))
            {
                TableOperations<AlarmDevice> alarmDeviceTable = new(connection);

                foreach (AlarmDevice alarmDevice in alarmDeviceTable.QueryRecords())
                {
                    if (!m_deviceMeasurementKeys.TryGetValue(alarmDevice.DeviceID, out MeasurementKey[] keys) ||
                        !m_deviceMetadata.TryGetValue(alarmDevice.DeviceID, out DataRow metadata))
                        continue;

                    if (!m_alarmStateIDs.TryGetValue(alarmDevice.StateID, out AlarmState currentState))
                        currentState = AlarmState.NotAvailable;

                    AlarmState newState = AlarmState.Good;
                    Ticks currentTime = DateTime.UtcNow.Ticks;

                    // Determine and update state
                    if (metadata["Enabled"].ToString().ParseBoolean())
                    {
                        AlarmState compositeState = AlarmState.OutOfService;

                        foreach (MeasurementKey key in keys)
                        {
                            Ticks lastUpdateTime = m_lastDeviceDataUpdates.GetOrAdd(key, currentTime);
                            TemporalMeasurement measurement = measurements.Measurement(key);
                            AlarmState deviceState = AlarmState.Good;

                            // Check quality of device frequency measurement
                            if (double.IsNaN(measurement.AdjustedValue) || measurement.AdjustedValue == 0.0D)
                            {
                                // If value is missing for longer than defined adapter lead / lag time tolerances,
                                // state is unavailable or in alarm if unavailable beyond configured alarm time
                                deviceState = currentTime - lastUpdateTime > m_alarmTime ? AlarmState.Alarm : AlarmState.NotAvailable;
                            }
                            else
                            {
                                // Have a value, update last device data time
                                m_lastDeviceDataUpdates[key] = currentTime;

                                if (!measurement.ValueQualityIsGood())
                                    deviceState = AlarmState.BadData;
                                else if (!measurement.TimestampQualityIsGood() || !measurement.Timestamp.UtcTimeIsValid(LagTime, LeadTime))
                                    deviceState = AlarmState.BadTime;
                            }

                            // Reporting device with worst state
                            if (deviceState != AlarmState.Good && deviceState < compositeState)
                                compositeState = deviceState;
                        }

                        if (compositeState < AlarmState.OutOfService)
                            newState = compositeState;
                        else if (TargetParentDevices)
                            newState = AlarmState.Good;
                    }
                    else
                    {
                        newState = AlarmState.OutOfService;
                    }

                    // Maintain any acknowledged state unless state changes to good
                    if (currentState == AlarmState.Acknowledged)
                    {
                        if (newState == AlarmState.Good)
                        {
                            Ticks lastTransitionTime = m_lastAcknowledgedTransition.GetOrAdd(alarmDevice.DeviceID, currentTime);

                            if (lastTransitionTime == Ticks.MinValue)
                            {
                                lastTransitionTime = currentTime;
                                m_lastAcknowledgedTransition[alarmDevice.DeviceID] = lastTransitionTime;
                            }

                            if ((DateTime.UtcNow.Ticks - lastTransitionTime).ToMinutes() <= AcknowledgedTransitionHysteresisDelay)
                                newState = AlarmState.Acknowledged;
                        }
                        else
                        {
                            newState = AlarmState.Acknowledged;
                            m_lastAcknowledgedTransition[alarmDevice.DeviceID] = Ticks.MinValue;
                        }
                    }

                    // Track current state counts
                    stateCounts[newState]++;

                    // Update alarm device state if it has changed
                    int stateID = m_alarmStates[newState].ID;

                    if (stateID != alarmDevice.StateID)
                    {
                        m_lastDeviceStateChange[alarmDevice.DeviceID] = currentTime;
                        alarmDevice.StateID = stateID;
                    }

                    // Update display text to show time since last alarm state change
                    alarmDevice.DisplayData = newState switch
                    {
                        AlarmState.Good => "0",
                        AlarmState.OutOfService => GetOutOfServiceTime(metadata),
                        _ => GetShortElapsedTimeString(currentTime - m_lastDeviceStateChange[alarmDevice.DeviceID])
                    };

                    // Update alarm table record
                    alarmDeviceTable.UpdateRecord(alarmDevice);
                    alarmDeviceUpdates.Add(alarmDevice);
                }

                m_alarmStateUpdates++;

                lock (m_stateCountLock)
                    m_stateCounts = stateCounts;
            }

            if (EnableExternalDatabaseSynchronization)
            {
                TemplatedExpressionParser parameterTemplate = new()
                {
                    TemplatedExpression = ExternalDatabaseCommandParameters
                };

                AlarmState compositeState = AlarmState.OutOfService;
                DataRow alarmedDeviceMetadata = null;
                Dictionary<string, string> substitutions = new();

                // Provide state counts as available substitution parameters
                foreach (KeyValuePair<AlarmState, int> stateCount in stateCounts)
                    substitutions[$"{{{stateCount.Key}StateCount}}"] = stateCount.Value.ToString();

                using AdoDataConnection connection = string.IsNullOrWhiteSpace(ExternalDatabaseConnectionString) ? new AdoDataConnection("systemSettings") : new AdoDataConnection(ExternalDatabaseConnectionString, ExternalDatabaseProviderString);

                foreach (AlarmDevice alarmDevice in alarmDeviceUpdates)
                {
                    if (!m_deviceMetadata.TryGetValue(alarmDevice.DeviceID, out DataRow metadata))
                        continue;

                    if (!m_alarmStateIDs.TryGetValue(alarmDevice.StateID, out AlarmState state))
                        state = AlarmState.NotAvailable;

                    alarmedDeviceMetadata ??= metadata;

                    if (ExternalDatabaseReportSingleCompositeState)
                    {
                        if (state == AlarmState.Good)
                            continue;

                        // First encountered alarmed device with highest alarm state will be reported as composite state
                        // because AlarmState values after Good are order by highest to lowest before OutOfService
                        if (state >= compositeState)
                            continue;

                        compositeState = state;
                        alarmedDeviceMetadata = metadata;
                    }
                    else
                    {
                        // When ExternalDatabaseReportSingleCompositeState is false, reporting state per device
                        if (state != AlarmState.Acknowledged)
                            ExternalDatabaseReportState(parameterTemplate, connection, state, metadata, substitutions);
                    }
                }

                if (ExternalDatabaseReportSingleCompositeState)
                {
                    AlarmState minimumHysteresisState = (AlarmState)ExternalDatabaseHysteresisMinimumState;

                    if (compositeState == AlarmState.OutOfService)
                        compositeState = AlarmState.Good;

                    string lastUpdate = m_lastExternalDatabaseStateChange > 0L ? $", last update: {(DateTime.UtcNow.Ticks - m_lastExternalDatabaseStateChange).ToElapsedTimeString(2)}" : "";
                    OnStatusMessage(MessageLevel.Info, $"Current composite reporting state: {compositeState}{lastUpdate}");

                    if (compositeState <= minimumHysteresisState)
                    {
                        // Report composite states less than specified minimum hysteresis state, typically Good and Alarm, immediately
                        ExternalDatabaseReportState(parameterTemplate, connection, compositeState, alarmedDeviceMetadata, substitutions);
                        m_compositeStates.Clear();
                        m_externalDatabaseUpdates++;
                    }
                    else
                    {
                        m_compositeStates.Add((int)compositeState);

                        // Report other composite states only after specified hysteresis delay has passed
                        if (!((DateTime.UtcNow.Ticks - m_lastExternalDatabaseStateChange).ToMinutes() > ExternalDatabaseHysteresisDelay))
                            return;

                        // Pick average composite state
                        compositeState = (AlarmState)(int)Math.Round(m_compositeStates.Average());

                        // Validate average composite state
                        if (compositeState <= minimumHysteresisState || compositeState >= AlarmState.OutOfService)
                            compositeState = m_compositeStates.Select(state => (AlarmState)state).FirstOrDefault(state => state > minimumHysteresisState && state < AlarmState.OutOfService);

                        ExternalDatabaseReportState(parameterTemplate, connection, compositeState, alarmedDeviceMetadata, substitutions);
                        m_compositeStates.Clear();
                        m_externalDatabaseUpdates++;
                    }
                }
                else
                {
                    m_externalDatabaseUpdates++;
                }
            }
        }
    }

    private void ExternalDatabaseReportState(TemplatedExpressionParser parameterTemplate, AdoDataConnection connection, AlarmState state, DataRow metadata, Dictionary<string, string> initialSubstitutions)
    {
        Dictionary<string, string> substitutions = new(initialSubstitutions)
        {
            ["{AlarmState}"] = state.ToString(),
            ["{AlarmStateValue}"] = ((int)state).ToString()
        };

        if (m_mappedAlarmStates.TryGetValue(state, out string mappedValue))
            substitutions["{MappedAlarmState}"] = mappedValue;
        else
            substitutions["{MappedAlarmState}"] = "0";

        // Use device metadata columns as possible substitution parameters
        if (metadata is not null)
        {
            foreach (DataColumn column in metadata.Table.Columns)
                substitutions[$"{{Device.{column.ColumnName}}}"] = metadata[column.ColumnName].ToString();
        }

        List<object> parameters = new();
        string commandParameters = parameterTemplate.Execute(substitutions);
        string[] splitParameters = commandParameters.Split(',');

        // Do some basic typing on command parameters
        foreach (string splitParameter in splitParameters)
        {
            string parameter = splitParameter.Trim();

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

        m_lastExternalDatabaseResult = connection.ExecuteScalar(ExternalDatabaseCommand, parameters.ToArray());
        m_lastExternalDatabaseStateChange = DateTime.UtcNow.Ticks;
    }

    private void MonitoringTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        m_monitoringOperation?.RunOnce();
    }

    #endregion

    #region [ Static ]

    private static readonly string[] s_shortTimeNames = { " yr", " yr", " d", " d", " hr", " hr", " m", " m", " s", " s", "< " };
    private static readonly double s_daysPerYear = new Time(Time.SecondsPerYear(DateTime.UtcNow.Year)).ToDays();

    private static Dictionary<AlarmState, int> CreateNewStateCountsMap()
    {
        return new Dictionary<AlarmState, int>
        {
            [AlarmState.Good] = 0,
            [AlarmState.Alarm] = 0,
            [AlarmState.NotAvailable] = 0,
            [AlarmState.BadData] = 0,
            [AlarmState.BadTime] = 0,
            [AlarmState.OutOfService] = 0,
            [AlarmState.Acknowledged] = 0
        };
    }

    private static string GetOutOfServiceTime(DataRow deviceRow)
    {
        if (deviceRow is null)
            return "U/A";

        try
        {
            return GetShortElapsedTimeString(DateTime.UtcNow.Ticks - Convert.ToDateTime(deviceRow["UpdatedOn"]).Ticks);
        }
        catch
        {
            return "U/A";
        }
    }

    /// <summary>
    /// Get short elapsed time string for specified <paramref name="span"/>.
    /// </summary>
    /// <param name="span"><see cref="Ticks"/> representing time span.</param>
    /// <returns>Short elapsed time string.</returns>
    public static string GetShortElapsedTimeString(Ticks span)
    {
        double days = span.ToDays();

        if (days > s_daysPerYear)
            return $"{days / s_daysPerYear:N2} yrs";

        if (days > 1.0D)
            span = span.BaselinedTimestamp(BaselineTimeInterval.Day);
        else if (span.ToHours() > 1.0D)
            span = span.BaselinedTimestamp(BaselineTimeInterval.Hour);
        else if (span.ToMinutes() > 1.0D)
            span = span.BaselinedTimestamp(BaselineTimeInterval.Minute);
        else if (span.ToSeconds() > 1.0D)
            span = span.BaselinedTimestamp(BaselineTimeInterval.Second);
        else
            return "0";

        string elapsedTimeString = span.ToElapsedTimeString(0, s_shortTimeNames);

        if (elapsedTimeString.Length > 10)
            elapsedTimeString = elapsedTimeString.Substring(0, 10);

        return elapsedTimeString;
    }

    #endregion
}