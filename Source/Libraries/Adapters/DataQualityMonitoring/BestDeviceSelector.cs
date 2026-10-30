//******************************************************************************************************
//  BestDeviceSelector.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  10/30/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Produces the signals of an output device by selecting, for each frame, the best of several
    /// redundant source devices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Signals are paired across devices by signal reference suffix, i.e., the part of the signal reference
    /// that follows the device acronym, e.g., "-PA1" or "-FQ". The output device defines the set of signals;
    /// pairing is rebuilt from the active measurements each time the adapter is initialized.
    /// </para>
    /// <para>
    /// For each frame, the first source device, in priority order, that has every signal present and good
    /// supplies all outputs. When no source device qualifies, each output is taken from its own best source
    /// device, unless <see cref="AllowMixedSources"/> is <c>false</c>, in which case all outputs are taken
    /// from the source device with the most good values so a frame never combines source devices.
    /// </para>
    /// </remarks>
    [Description("Best Device: Produces the signals of an output device by selecting the best source device for each frame, falling back to per signal selection when no source device is fully good.")]
    public class BestDeviceSelector : BestValueSelectorBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="AllowMixedSources"/> property.
        /// </summary>
        public const bool DefaultAllowMixedSources = true;

        /// <summary>
        /// Defines the default value for the <see cref="FailbackDelay"/> property.
        /// </summary>
        public const double DefaultFailbackDelay = 0.0D;

        // Source has not been all good since its last evaluation
        private const long NotAllGood = long.MinValue;

        // Signal types never paired: statistics and alarms belong to each device
        private static readonly string[] s_excludedSignalTypes = { "STAT", "ALRM" };

        // Fields
        private string[] m_sourceDevices;
        private string m_outputDevice;
        private bool m_allowMixedSources;
        private double m_failbackDelay;
        private long m_failbackDelayTicks;
        private string[] m_signals;                 // Signal reference suffix for each output signal
        private IMeasurement[] m_outputTemplates;   // Output measurement for each signal
        private MeasurementKey[][] m_sourceKeys;    // Input key per source per signal, null when source lacks signal
        private IMeasurement m_sourceIndexTemplate;
        private long[] m_allGoodSince;              // Timestamp each source became all good, or NotAllGood
        private int m_currentSource;                // Source last selected as a whole device, -1 when none

        // Statistics
        private long[] m_wholeDeviceFrames;
        private long m_degradedFrames;
        private long m_mixedFrames;
        private long m_emptyFrames;
        private long m_failovers;
        private long m_failbacks;
        private int m_lastSourceIndex;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the source device acronyms, in priority order, separated by semicolons.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the acronyms of the source devices, in priority order with the preferred device first, separated by semicolons.")]
        public string SourceDevices
        {
            get => m_sourceDevices is null ? null : string.Join("; ", m_sourceDevices);
            set => m_sourceDevices = ParseDeviceList(value);
        }

        /// <summary>
        /// Gets or sets the acronym of the output device whose measurements receive the selected values.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the acronym of the output device whose measurements receive the selected values.")]
        public string OutputDevice
        {
            get => m_outputDevice;
            set => m_outputDevice = value?.Trim();
        }

        /// <summary>
        /// Gets or sets the flag that determines if a frame can combine values from more than one source
        /// device when no source device has all good values.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultAllowMixedSources),
        Description("Define the flag that determines if, when no source device has all good values, each output can be taken from a different source device; when false, all outputs are taken from the source device with the most good values.")]
        public bool AllowMixedSources
        {
            get => m_allowMixedSources;
            set => m_allowMixedSources = value;
        }

        /// <summary>
        /// Gets or sets the time, in seconds, a higher priority source device must have all good values
        /// before it is selected again after a fail-over; zero disables the delay.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFailbackDelay),
        Description("Defines the time, in seconds, a higher priority source device must have all good values before it is selected again after a fail-over; zero disables the delay.")]
        public double FailbackDelay
        {
            get => m_failbackDelay;
            set
            {
                m_failbackDelay = value < 0.0D ? 0.0D : value;
                m_failbackDelayTicks = Ticks.FromSeconds(m_failbackDelay);
            }
        }

        /// <summary>
        /// Gets or sets the optional measurement that receives the 1-based index of the source device
        /// used for each frame, or zero when the frame combined source devices.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines the optional measurement that receives the 1-based index of the source device used for each frame, or zero when values were taken from more than one source device; can be a measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public string SourceIndexMeasurement { get; set; }

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new(base.Status);
                string[] sourceDevices = m_sourceDevices;
                MeasurementKey[][] sourceKeys = m_sourceKeys;
                long[] wholeDeviceFrames = m_wholeDeviceFrames;

                status.AppendLine($"{"Output Device",28}: {m_outputDevice}");
                status.AppendLine($"{"Allow Mixed Sources",28}: {(m_allowMixedSources ? "Yes" : "No")}");
                status.AppendLine($"{"Failback Delay",28}: {(m_failbackDelay > 0.0D ? $"{m_failbackDelay:N3} seconds" : "Disabled")}");
                status.AppendLine($"{"Source Index Measurement",28}: {(m_sourceIndexTemplate is null ? "Undefined" : m_sourceIndexTemplate.ToString())}");
                status.AppendLine($"{"Paired Signals",28}: {m_signals?.Length ?? 0:N0}");

                // Arrays are only consistent with each other after a successful initialization
                if (sourceDevices is not null && sourceKeys?.Length == sourceDevices.Length && wholeDeviceFrames?.Length == sourceDevices.Length)
                {
                    status.AppendLine($"{"Source Devices",28}:");

                    for (int i = 0; i < sourceDevices.Length; i++)
                    {
                        int pairedSignals = sourceKeys[i].Count(key => key is not null);
                        string current = i == m_currentSource ? " (current)" : "";

                        status.AppendLine($"{i + 1,30}: {sourceDevices[i]}{current} - {pairedSignals:N0} paired signals, {wholeDeviceFrames[i]:N0} whole device frames");
                    }
                }

                status.AppendLine($"{"Frames With No Good Device",28}: {m_degradedFrames:N0}");
                status.AppendLine($"{"Mixed Source Frames",28}: {m_mixedFrames:N0}");
                status.AppendLine($"{"Frames With No Outputs",28}: {m_emptyFrames:N0}");
                status.AppendLine($"{"Fail-overs",28}: {m_failovers:N0}");
                status.AppendLine($"{"Failbacks",28}: {m_failbacks:N0}");
                status.AppendLine($"{"Last Source Device Index",28}: {m_lastSourceIndex}");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="BestDeviceSelector"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue(nameof(SourceDevices), out string setting) || (m_sourceDevices = ParseDeviceList(setting)).Length == 0)
                throw new ArgumentException($"{nameof(SourceDevices)} is missing from Settings - Example: sourceDevices={{PMU.A; PMU.B}}; outputDevice=PMU");

            if (!settings.TryGetValue(nameof(OutputDevice), out setting) || string.IsNullOrWhiteSpace(setting))
                throw new ArgumentException($"{nameof(OutputDevice)} is missing from Settings - Example: sourceDevices={{PMU.A; PMU.B}}; outputDevice=PMU");

            m_outputDevice = setting.Trim();

            if (m_sourceDevices.Contains(m_outputDevice, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Output device \"{m_outputDevice}\" cannot also be a source device.");

            DataTable activeMeasurements = DataSource?.Tables["ActiveMeasurements"] ?? throw new InvalidOperationException("Cannot pair device signals, ActiveMeasurements table is not available in the data source.");
            DataRow[] outputRows = SelectDeviceRows(activeMeasurements, m_outputDevice);

            // Frame rate defaults to the output device's rate, it must be defined before base initialization
            if (!settings.ContainsKey(nameof(FramesPerSecond)) && outputRows.Length > 0 && activeMeasurements.Columns.Contains(nameof(FramesPerSecond)))
                settings[nameof(FramesPerSecond)] = outputRows[0][nameof(FramesPerSecond)].ToString();

            base.Initialize();

            if (settings.ContainsKey(nameof(InputMeasurementKeys)) || settings.ContainsKey(nameof(OutputMeasurements)))
                OnStatusMessage(MessageLevel.Warning, $"{nameof(InputMeasurementKeys)} and {nameof(OutputMeasurements)} settings are ignored, they are derived from {nameof(SourceDevices)} and {nameof(OutputDevice)}.", flags: MessageFlags.UsageIssue);

            m_allowMixedSources = settings.TryGetValue(nameof(AllowMixedSources), out setting) ? setting.ParseBoolean() : DefaultAllowMixedSources;
            FailbackDelay = settings.TryGetValue(nameof(FailbackDelay), out setting) && double.TryParse(setting, out double failbackDelay) ? failbackDelay : DefaultFailbackDelay;

            if (settings.TryGetValue(nameof(SourceIndexMeasurement), out setting) && !string.IsNullOrWhiteSpace(setting))
            {
                IMeasurement[] sourceIndexMeasurements = AdapterBase.ParseOutputMeasurements(DataSource, true, setting);

                if (sourceIndexMeasurements.Length != 1)
                    throw new ArgumentException($"{nameof(SourceIndexMeasurement)} must define exactly one measurement. Amount defined: {sourceIndexMeasurements.Length}");

                SourceIndexMeasurement = setting;
                m_sourceIndexTemplate = sourceIndexMeasurements[0];
            }
            else
            {
                SourceIndexMeasurement = null;
                m_sourceIndexTemplate = null;
            }

            PairSignals(activeMeasurements, outputRows);

            m_allGoodSince = Enumerable.Repeat(NotAllGood, m_sourceDevices.Length).ToArray();
            m_wholeDeviceFrames = new long[m_sourceDevices.Length];
            m_currentSource = -1;
            m_lastSourceIndex = 0;
        }

        // Pairs output device signals with source device signals by signal reference suffix
        private void PairSignals(DataTable activeMeasurements, DataRow[] outputRows)
        {
            Dictionary<string, DataRow> outputSignals = GetDeviceSignals(m_outputDevice, outputRows);

            // Source index measurement may be defined on the output device, it is not a paired signal
            if (m_sourceIndexTemplate is not null)
            {
                foreach (string signal in outputSignals.Where(pair => GetSignalID(pair.Value) == m_sourceIndexTemplate.ID).Select(pair => pair.Key).ToArray())
                    outputSignals.Remove(signal);
            }

            if (outputSignals.Count == 0)
                throw new InvalidOperationException($"Output device \"{m_outputDevice}\" has no active measurements to receive selected values.");

            m_signals = outputSignals.Keys.OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase).ToArray();
            m_outputTemplates = m_signals.Select(signal => CreateOutputMeasurement(outputSignals[signal])).ToArray();
            m_sourceKeys = new MeasurementKey[m_sourceDevices.Length][];

            List<MeasurementKey> inputKeys = new();
            int sourcesFound = 0;

            for (int i = 0; i < m_sourceDevices.Length; i++)
            {
                string sourceDevice = m_sourceDevices[i];
                Dictionary<string, DataRow> sourceSignals = GetDeviceSignals(sourceDevice, SelectDeviceRows(activeMeasurements, sourceDevice));
                MeasurementKey[] keys = new MeasurementKey[m_signals.Length];

                m_sourceKeys[i] = keys;

                if (sourceSignals.Count == 0)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Source device \"{sourceDevice}\" has no active measurements, it is either undefined or disabled and will not be used.", flags: MessageFlags.UsageIssue);
                    continue;
                }

                sourcesFound++;

                List<string> missingSignals = new();

                for (int j = 0; j < m_signals.Length; j++)
                {
                    if (sourceSignals.TryGetValue(m_signals[j], out DataRow row))
                    {
                        keys[j] = GetMeasurementKey(row);
                        inputKeys.Add(keys[j]);
                    }
                    else
                    {
                        missingSignals.Add(m_signals[j]);
                    }
                }

                if (missingSignals.Count > 0)
                    OnStatusMessage(MessageLevel.Warning, $"Source device \"{sourceDevice}\" is missing signals {string.Join(", ", missingSignals)}, it can only supply values when no source device has all good values.", flags: MessageFlags.UsageIssue);

                string[] unpairedSignals = sourceSignals.Keys.Where(signal => !outputSignals.ContainsKey(signal)).ToArray();

                if (unpairedSignals.Length > 0)
                    OnStatusMessage(MessageLevel.Warning, $"Source device \"{sourceDevice}\" signals {string.Join(", ", unpairedSignals)} are not defined on output device \"{m_outputDevice}\" and will be ignored.", flags: MessageFlags.UsageIssue);
            }

            if (sourcesFound == 0)
                throw new InvalidOperationException($"None of the source devices {string.Join(", ", m_sourceDevices)} have active measurements.");

            InputMeasurementKeys = inputKeys.ToArray();

            OutputMeasurements = m_sourceIndexTemplate is null ?
                m_outputTemplates :
                m_outputTemplates.Concat(new[] { m_sourceIndexTemplate }).ToArray();
        }

        // Gets device measurements keyed by signal reference suffix, excluding device statistics and alarms
        private Dictionary<string, DataRow> GetDeviceSignals(string device, DataRow[] rows)
        {
            Dictionary<string, DataRow> signals = new(StringComparer.OrdinalIgnoreCase);
            bool hasSignalType = rows.Length > 0 && rows[0].Table.Columns.Contains("SignalType");

            foreach (DataRow row in rows)
            {
                string signalReference = row["SignalReference"].ToNonNullString();

                if (hasSignalType && s_excludedSignalTypes.Contains(row["SignalType"].ToNonNullString(), StringComparer.OrdinalIgnoreCase))
                    continue;

                if (!signalReference.StartsWith(device, StringComparison.OrdinalIgnoreCase) || signalReference.Length == device.Length)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Measurement \"{row["PointTag"].ToNonNullString()}\" signal reference \"{signalReference}\" does not start with device acronym \"{device}\" followed by a suffix and will be ignored.", flags: MessageFlags.UsageIssue);
                    continue;
                }

                string signal = signalReference.Substring(device.Length);

                // System generated signals, e.g., statistics, use "!" in their signal reference
                if (signal.IndexOf('!') >= 0)
                    continue;

                if (signals.ContainsKey(signal))
                {
                    OnStatusMessage(MessageLevel.Warning, $"Device \"{device}\" has more than one measurement with signal reference \"{signalReference}\", only the first will be used.", flags: MessageFlags.UsageIssue);
                    continue;
                }

                signals.Add(signal, row);
            }

            return signals;
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            MeasurementKey[][] sourceKeys = m_sourceKeys;
            long[] allGoodSince = m_allGoodSince;
            long timestamp = frame.Timestamp;
            int firstAllGood = -1;

            // Track how long each source has been continuously all good
            for (int i = 0; i < sourceKeys.Length; i++)
            {
                if (IsAllGood(measurements, sourceKeys[i]))
                {
                    if (allGoodSince[i] == NotAllGood)
                        allGoodSince[i] = timestamp;

                    if (firstAllGood < 0)
                        firstAllGood = i;
                }
                else
                {
                    allGoodSince[i] = NotAllGood;
                }
            }

            List<IMeasurement> outputs = new(m_outputTemplates.Length + 1);
            int sourceIndex;

            if (firstAllGood >= 0)
            {
                int selected = SelectWholeDevice(timestamp, firstAllGood);
                MeasurementKey[] keys = sourceKeys[selected];

                for (int i = 0; i < keys.Length; i++)
                    AddOutput(outputs, i, measurements[keys[i]], timestamp);

                if (m_currentSource >= 0 && selected != m_currentSource)
                {
                    if (selected > m_currentSource)
                        m_failovers++;
                    else
                        m_failbacks++;
                }

                m_currentSource = selected;
                m_wholeDeviceFrames[selected]++;
                sourceIndex = selected + 1;
            }
            else
            {
                m_degradedFrames++;

                int usedSource = m_allowMixedSources ?
                    AddBestSignalOutputs(outputs, measurements, timestamp) :
                    AddBestDeviceOutputs(outputs, measurements, timestamp);

                if (usedSource == MixedSources)
                    m_mixedFrames++;

                // Index is zero when values came from more than one source device
                sourceIndex = usedSource >= 0 ? usedSource + 1 : 0;
            }

            if (outputs.Count == 0)
            {
                m_emptyFrames++;
                return;
            }

            if (m_sourceIndexTemplate is not null)
                outputs.Add(Measurement.Clone(m_sourceIndexTemplate, sourceIndex, timestamp, MeasurementStateFlags.Normal));

            m_lastSourceIndex = sourceIndex;

            OnNewMeasurements(outputs);
        }

        // Selects the first all good source that is at or below the current source's priority, or that
        // has been all good for the failback delay; when none qualifies, the current source has failed
        // and the first all good source is selected
        private int SelectWholeDevice(long timestamp, int firstAllGood)
        {
            long[] allGoodSince = m_allGoodSince;

            for (int i = firstAllGood; i < allGoodSince.Length; i++)
            {
                if (allGoodSince[i] == NotAllGood)
                    continue;

                if (m_currentSource < 0 || i >= m_currentSource || timestamp - allGoodSince[i] >= m_failbackDelayTicks)
                    return i;
            }

            return firstAllGood;
        }

        // Adds each output from its first good source, or least bad source when bad values are published;
        // returns the index of the single source used, MixedSources or NoSource
        private int AddBestSignalOutputs(List<IMeasurement> outputs, ConcurrentDictionary<MeasurementKey, IMeasurement> measurements, long timestamp)
        {
            MeasurementKey[][] sourceKeys = m_sourceKeys;
            bool publishBadValues = PublishBadValues;
            int usedSource = NoSource;

            for (int signal = 0; signal < m_outputTemplates.Length; signal++)
            {
                IMeasurement best = null;
                int bestSource = NoSource;
                int bestScore = int.MaxValue;

                for (int source = 0; source < sourceKeys.Length; source++)
                {
                    MeasurementKey key = sourceKeys[source][signal];

                    if (key is null || !measurements.TryGetValue(key, out IMeasurement measurement) || measurement is null)
                        continue;

                    if (IsGood(measurement))
                    {
                        best = measurement;
                        bestSource = source;
                        break;
                    }

                    if (!publishBadValues)
                        continue;

                    int score = GetBadnessScore(measurement);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = measurement;
                        bestSource = source;
                    }
                }

                if (best is null)
                    continue;

                AddOutput(outputs, signal, best, timestamp);

                if (usedSource == NoSource)
                    usedSource = bestSource;
                else if (usedSource != bestSource)
                    usedSource = MixedSources;
            }

            return usedSource;
        }

        // Adds outputs from the source with the most good values, ties broken by the most present values
        // then priority; bad values are only added when bad values are published; returns the source
        // index used or NoSource
        private int AddBestDeviceOutputs(List<IMeasurement> outputs, ConcurrentDictionary<MeasurementKey, IMeasurement> measurements, long timestamp)
        {
            MeasurementKey[][] sourceKeys = m_sourceKeys;
            bool publishBadValues = PublishBadValues;
            int bestSource = NoSource;
            int bestGood = 0;
            int bestPresent = 0;

            for (int source = 0; source < sourceKeys.Length; source++)
            {
                MeasurementKey[] keys = sourceKeys[source];
                int good = 0;
                int present = 0;

                for (int signal = 0; signal < keys.Length; signal++)
                {
                    if (keys[signal] is null || !measurements.TryGetValue(keys[signal], out IMeasurement measurement) || measurement is null)
                        continue;

                    present++;

                    if (IsGood(measurement))
                        good++;
                }

                if (good > bestGood || (good == bestGood && present > bestPresent))
                {
                    bestSource = source;
                    bestGood = good;
                    bestPresent = present;
                }
            }

            if (bestSource == NoSource || (bestGood == 0 && !publishBadValues))
                return NoSource;

            MeasurementKey[] bestKeys = sourceKeys[bestSource];

            for (int signal = 0; signal < bestKeys.Length; signal++)
            {
                if (bestKeys[signal] is null || !measurements.TryGetValue(bestKeys[signal], out IMeasurement measurement) || measurement is null)
                    continue;

                if (publishBadValues || IsGood(measurement))
                    AddOutput(outputs, signal, measurement, timestamp);
            }

            return bestSource;
        }

        // Source is all good when it supplies every signal and each value is present and good
        private bool IsAllGood(ConcurrentDictionary<MeasurementKey, IMeasurement> measurements, MeasurementKey[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] is null || !measurements.TryGetValue(keys[i], out IMeasurement measurement) || !IsGood(measurement))
                    return false;
            }

            return true;
        }

        private void AddOutput(List<IMeasurement> outputs, int signal, IMeasurement measurement, long timestamp) =>
            outputs.Add(Measurement.Clone(m_outputTemplates[signal], measurement.Value, timestamp, measurement.StateFlags));

        #endregion

        #region [ Static ]

        // Static Fields
        private const int NoSource = -1;
        private const int MixedSources = -2;

        // Static Methods
        private static string[] ParseDeviceList(string value) =>
            value is null ?
                Array.Empty<string>() :
                value.Split(';')
                    .Select(device => device.Trim())
                    .Where(device => device.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

        private static DataRow[] SelectDeviceRows(DataTable activeMeasurements, string device) =>
            activeMeasurements.Select($"Device = '{device.Replace("'", "''")}'");

        private static Guid GetSignalID(DataRow row) =>
            row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();

        private static MeasurementKey GetMeasurementKey(DataRow row) =>
            MeasurementKey.LookUpOrCreate(GetSignalID(row), row["ID"].ToString());

        private static IMeasurement CreateOutputMeasurement(DataRow row) =>
            new Measurement { Metadata = GetMeasurementKey(row).Metadata };

        #endregion
    }
}
