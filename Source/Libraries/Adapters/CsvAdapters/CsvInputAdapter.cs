//******************************************************************************************************
//  CsvInputAdapter.cs - Gbtc
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
//  04/06/2010 - Stephen C. Wills
//       Generated original version of source code.
//  07/03/2012 - J. Ritchie Carroll
//       Added high-resolution input timer, auto-repeat and transverse operational mode.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

namespace CSVAdapters;

/// <summary>
/// Represents an input adapter that reads measurements from a CSV file.
/// </summary>
[Description("CSV: Reads measurements from a CSV file")]
public class CsvInputAdapter : InputAdapterBase
{
    #region [ Members ]

    // Nested Types

    // Timer class used to precisely pace CSV play-back speed. Uses absolute deadline
    // advancement to maintain consistent inter-frame intervals. For longer intervals,
    // e.g., 30 FPS / ~33ms, a single bulk Thread.Sleep is used for most of the wait,
    // followed by yield/spin for the final approach — minimizing OS scheduler jitter
    // compared to many individual Thread.Sleep(1) calls.
    private sealed class PacingTimer
    {
        // Guard-band in milliseconds: we sleep until this much
        // time remains, then yield/spin to the exact deadline
        private const double SleepGuardBand = 2.0D;

        private readonly long m_periodTicks;    // Query Performance Counter (QPC) ticks per frame
        private readonly long m_guardBandTicks; // QPC ticks for the guard-band
        private long m_nextTick;                // Next scheduled QPC tick (not equal to DateTime ticks)

        /// <summary>
        /// Creates a new <see cref="PacingTimer"/> instance.
        /// </summary>
        /// <param name="interval">The defined interval in milliseconds.</param>
        public PacingTimer(double interval)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval));

            m_periodTicks = (long)Math.Round(Stopwatch.Frequency * interval / 1000.0D);
            m_guardBandTicks = (long)Math.Round(Stopwatch.Frequency * SleepGuardBand / 1000.0D);

            if (m_periodTicks <= 0L)
                throw new ArgumentOutOfRangeException(nameof(interval), $"{nameof(InputInterval)} is too small for the timer resolution.");
        }

        /// <summary>
        /// Set the next waiting period based on current time.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNextPeriod()
        {
            m_nextTick = Stopwatch.GetTimestamp() + m_periodTicks;
        }

        /// <summary>
        /// Blocks until the next scheduled waiting period.
        /// </summary>
        /// <param name="setNextPeriod">
        /// Flag that determines if next waiting period should be set; when <c>true</c>, the
        /// deadline advances by exactly one period from the current deadline (absolute
        /// cadence) to keep inter-frame intervals consistent. If the deadline has already
        /// fallen behind by more than one full period, it resets to "now + period" to
        /// prevent a burst of catch-up frames. When <c>false</c>, caller must manually
        /// call <see cref="SetNextPeriod"/> to schedule the next waiting period.
        /// </param>
        public void WaitNext(bool setNextPeriod = true)
        {
            long nextTick = m_nextTick;

            if (nextTick == 0L)
                throw new InvalidOperationException("Next period has not been set. Call SetNextPeriod() before WaitNext().");

            if (setNextPeriod)
            {
                // Advance deadline absolutely from previous deadline to maintain
                // a steady cadence regardless of per-frame processing jitter
                long advancedTick = nextTick + m_periodTicks;

                // If the advanced deadline is already in the past, we've fallen
                // behind by more than a full period — reset to relative mode to
                // avoid a burst of catch-up frames
                m_nextTick = Stopwatch.GetTimestamp() >= advancedTick
                    ? Stopwatch.GetTimestamp() + m_periodTicks
                    : advancedTick;
            }
            else
            {
                m_nextTick = 0L;
            }

            // Fast exit if already past deadline (e.g., processing took longer than interval)
            if (Stopwatch.GetTimestamp() >= nextTick)
                return;

            long ticksRemaining = nextTick - Stopwatch.GetTimestamp();

            // For longer waits, sleep in one bulk call leaving only a small
            // guard-band for spin/yield — this dramatically reduces the number
            // of OS scheduler interactions compared to repeated Sleep(1) calls
            if (ticksRemaining > m_guardBandTicks)
            {
                long sleepTicks = ticksRemaining - m_guardBandTicks;
                int sleepMs = (int)(sleepTicks * 1000L / Stopwatch.Frequency);

                if (sleepMs > 0)
                    Thread.Sleep(sleepMs);
            }

            // Yield / spin for the remaining guard-band to hit precise deadline
            while (true)
            {
                long now = Stopwatch.GetTimestamp();

                if (now >= nextTick)
                    return;

                double remaining = (nextTick - now) * 1000.0 / Stopwatch.Frequency;

                if (remaining >= 0.3D)
                {
                    // Yield to reduce CPU but avoid oversleeping
                    Thread.Yield();
                }
                else
                {
                    // Spin to hit sub-millisecond cadence
                    SpinWait sw = new();

                    while (Stopwatch.GetTimestamp() < nextTick)
                        sw.SpinOnce();

                    return;
                }
            }
        }
    }

    // Constants
    private const double DefaultInputInterval = 1000.0D / 30.0D;
    private const int DefaultMeasurementsPerInterval = 5;

    // Fields
    private StreamReader? m_inStream;
    private string? m_header;
    private readonly Dictionary<string, int> m_columns;
    private readonly Dictionary<int, IMeasurement> m_columnMappings;
    private ReadOnlyDictionary<Guid, int>? m_signalIDPublishOrder;

    private PacingTimer? m_pacingTimer;
    private LongSynchronizedOperation? m_readRow;

    private PrecisionInputTimer? m_precisionTimer;
    private long[]? m_subsecondDistribution;
    private long m_previousSecond;
    private int m_previousFrameIndex;
    private bool m_subSecondCorrection = true;

    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvInputAdapter"/> class.
    /// </summary>
    public CsvInputAdapter()
    {
        FileName = "measurements.csv";
        m_columns = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        m_columnMappings = new Dictionary<int, IMeasurement>();

        // ReSharper disable VirtualMemberCallInConstructor
        InputInterval = DefaultInputInterval;
        MeasurementsPerInterval = DefaultMeasurementsPerInterval;
        AutoAssignMappingsToOutputs = true;

        // Set minimum timer resolution to one millisecond to improve timer accuracy
        PrecisionTimer.SetMinimumTimerResolution(1);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the name of the CSV file from which measurements will be read.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the name of the CSV file from which measurements will be read.")]
    [CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FileDialogEditor", "type=open; checkFileExists=true; defaultExt=.csv; filter=CSV files|*.csv|AllFiles|*.*")]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the interval of time between reading rows from the CSV file.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the interval of time, in milliseconds, between reading rows from the CSV file.")]
    [DefaultValue(DefaultInputInterval)]
    public virtual double InputInterval { get; set; }

    /// <summary>
    /// Gets or sets value that determines if the CSV input file data should be replayed repeatedly.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define if the CSV input file data should be replayed repeatedly.")]
    [DefaultValue(false)]
    public bool AutoRepeat { get; set; }

    /// <summary>
    /// Gets or sets number of lines to skip in the source file before the header line is encountered.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the number of lines to skip in the source file before the header line is encountered.")]
    [DefaultValue(0)]
    public int SkipRows { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if a high-resolution timer should be used for CSV file based input.
    /// </summary>
    /// <remarks>
    /// Useful when input needs be accurately time-aligned to a specified frame rate. Maximum interval is 1ms (1000 FPS).
    /// Set to <c>false</c> if faster inputs are needed, e.g., 0.3333333ms (3000 FPS) or greater.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Determines if a high-resolution timer should be used for CSV file based input.")]
    [DefaultValue(false)]
    public bool UseHighResolutionInputTimer
    {
        get => m_precisionTimer is not null;
        set
        {
            switch (value)
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                case true when m_precisionTimer is null && !Debugger.IsAttached:
                    m_precisionTimer = PrecisionInputTimer.Attach(FrameRate, ex => OnProcessException(MessageLevel.Warning, ex));
                    break;
                case false when m_precisionTimer is not null:
                    PrecisionInputTimer.Detach(ref m_precisionTimer);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of measurements that are read from the CSV file in each frame.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the number of measurements that are read from the CSV file in each frame.")]
    [DefaultValue(DefaultMeasurementsPerInterval)]
    public int MeasurementsPerInterval { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether CSV file is in transverse mode for real-time concentration.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Indicate whether CSV file is in transverse mode for real-time concentration.")]
    [DefaultValue(false)]
    public bool TransverseMode { get; set; }

    /// <summary>
    /// Gets or sets a value that determines whether timestamps are
    /// simulated for the purposes of real-time concentration.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Indicate whether timestamps are simulated for real-time concentration.")]
    [DefaultValue(false)]
    public bool SimulateTimestamp { get; set; }

    /// <summary>
    /// Manually defines the column mappings, e.g.: "0=Timestamp; 1=PPA:12; 2=PPA13". Use <see cref="AutoMapToOutputMeasurements"/>
    /// instead to automatically assign column mappings to output measurements when transverse mode is enabled.
    /// </summary>
    [ConnectionStringParameter]
    [Description($"""
        Manually defines the column mappings, e.g.: "0=Timestamp; 1=PPA:12; 2=PPA13". Use '{nameof(AutoMapToOutputMeasurements)}'
        instead to automatically assign column mappings to output measurements when transverse mode is enabled.
        """)]
    [DefaultValue("")]
    public string ColumnMappings { get; set; } = null!;

    /// <summary>
    /// Gets or sets flags that determines whether to automatically assign column mappings to output measurements when transverse mode is enabled. Output measurement definition order preserved.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines whether to automatically assign column mappings to output measurements when transverse mode is enabled. Output measurement definition order preserved.")]
    [DefaultValue(false)]
    public bool AutoMapToOutputMeasurements { get; set; }

    /// <summary>
    /// Gets or sets the column index for timestamp values when <see cref="AutoMapToOutputMeasurements"/> is enabled.
    /// </summary>
    /// <remarks>
    /// Value is required when <see cref="AutoMapToOutputMeasurements"/> is enabled to identify which column contains timestamp values.
    /// </remarks>
    [ConnectionStringParameter]
    [Description($"Defines the column index for timestamp values when '{nameof(AutoMapToOutputMeasurements)}' is enabled.")]
    [DefaultValue(0)]
    public int TimestampColumnIndex { get; set; }

    /// <summary>
    /// Gets or sets a flag that determines whether to automatically assign column mappings to output measurements when transverse mode is enabled.
    /// </summary>
    /// <remarks>
    /// Value will be set to <c>false</c> if <see cref="AutoMapToOutputMeasurements"/> is enabled. No need to reassign column mappings to output
    /// measurements if they are already being used as source for column mappings.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines flag that determines whether to automatically assign column mappings to output measurements when transverse mode is enabled.")]
    [DefaultValue(true)]
    public bool AutoAssignMappingsToOutputs { get; set; }

    /// <summary>
    /// Gets or sets flag that determines whether to round timestamps to the inferred <see cref="FrameRate"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines flag that determines whether to round timestamps to the inferred 'FrameRate'.")]
    [DefaultValue(true)]
    public bool RoundTimestampsToFrameRate { get; set; }

    /// <summary>
    /// Gets inferred frame rate based on the defined <see cref="InputInterval"/>.
    /// </summary>
    public int FrameRate { get; private set; }

    /// <summary>
    /// Gets a flag that determines if this <see cref="CsvInputAdapter"/>
    /// uses an asynchronous connection.
    /// </summary>
    protected override bool UseAsyncConnect => false;

    /// <summary>
    /// Gets a read-only mapping of measurement signal ID to zero-based publish order index.
    /// </summary>
    /// <remarks>
    /// This excludes the timestamp column mapping and all indexes are normalized to
    /// zero-based index sequential to match publication order.
    /// </remarks>
    protected ReadOnlyDictionary<Guid, int> SignalIDPublishOrder
    {
        get
        {
            if (m_signalIDPublishOrder is not null)
                return m_signalIDPublishOrder;

            Dictionary<Guid, int> signalIDPublishOrder = new();
            int publishOrder = 0;

            foreach (KeyValuePair<int, IMeasurement> kvp in m_columnMappings.OrderBy(kvp => kvp.Key))
            {
                int index = kvp.Key;
                IMeasurement measurement = kvp.Value;

                if (index == TimestampColumnIndex)
                    continue;

                signalIDPublishOrder[measurement.ID] = publishOrder++;
            }

            m_signalIDPublishOrder = new ReadOnlyDictionary<Guid, int>(signalIDPublishOrder);

            return m_signalIDPublishOrder;
        }
    }

    /// <summary>
    /// Returns the detailed status of this <see cref="CsvInputAdapter"/>.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.Append(base.Status);
            status.AppendLine();
            status.AppendLine($"                 File name: {FilePath.TrimFileName(FileName, 51)}");
            status.AppendLine($"               File header: {m_header.TruncateRight(51)}");
            status.AppendLine($"            Input interval: {InputInterval:N3} ({FrameRate} fps)");
            status.AppendLine($" Measurements per interval: {MeasurementsPerInterval:N0}");
            status.AppendLine($"        Simulate timestamp: {SimulateTimestamp}");
            status.AppendLine($"  Round time to frame rate: {RoundTimestampsToFrameRate}");
            status.AppendLine($"     Using transverse mode: {TransverseMode}");

            if (TransverseMode)
            {
                status.AppendLine($"Auto map to config outputs: {AutoMapToOutputMeasurements}");

                if (AutoMapToOutputMeasurements)
                    status.AppendLine($"    Timestamp column index: {TimestampColumnIndex:N0}");

                status.AppendLine($"Auto assign map to outputs: {AutoAssignMappingsToOutputs}");
            }

            status.AppendLine($"               Auto-repeat: {AutoRepeat}");
            status.AppendLine($"     Precision input timer: {(UseHighResolutionInputTimer ? "Enabled" : "Offline")}");
            status.AppendLine($"             Lines to skip: {SkipRows:N0}");

            if (m_precisionTimer is not null)
            {
                status.Append($"  Timer resynchronizations: {m_precisionTimer.Resynchronizations}");
                status.AppendLine();
            }

            return status.ToString();
        }
    }

    /// <summary>
    /// Gets the flag indicating if this adapter supports temporal processing.
    /// </summary>
    public override bool SupportsTemporalProcessing => true;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="CsvInputAdapter"/> object and optionally releases the managed resources.
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

            if (UseHighResolutionInputTimer)
                PrecisionInputTimer.Detach(ref m_precisionTimer);

            m_readRow = null;

            m_inStream?.Dispose();
            m_inStream = null;

            // Clear minimum timer resolution.
            PrecisionTimer.ClearMinimumTimerResolution(1);
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Initializes this <see cref="CsvInputAdapter"/>.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        // Load optional parameters
        if (settings.TryGetValue(nameof(FileName), out string? setting))
            FileName = setting;

        if (settings.TryGetValue(nameof(InputInterval), out setting))
            InputInterval = double.Parse(setting);

        FrameRate = (int)Math.Round(1000.0D / InputInterval);

        if (settings.TryGetValue(nameof(MeasurementsPerInterval), out setting))
            MeasurementsPerInterval = int.Parse(setting);

        if (settings.TryGetValue(nameof(SimulateTimestamp), out setting))
            SimulateTimestamp = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(RoundTimestampsToFrameRate), out setting))
            RoundTimestampsToFrameRate = setting.ParseBoolean();
        else
            RoundTimestampsToFrameRate = true;

        if (settings.TryGetValue(nameof(TransverseMode), out setting) || settings.TryGetValue("transverse", out setting))
            TransverseMode = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AutoRepeat), out setting))
            AutoRepeat = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(SkipRows), out setting) && int.TryParse(setting, out int skipRows) && skipRows > 0)
            SkipRows = skipRows;
        else
            SkipRows = 0;

        settings.TryGetValue(nameof(UseHighResolutionInputTimer), out setting);

        if (string.IsNullOrEmpty(setting))
            setting = "false";

        UseHighResolutionInputTimer = setting.ParseBoolean();

        if (!UseHighResolutionInputTimer)
        {
            m_pacingTimer = new PacingTimer(InputInterval);
            m_readRow = new LongSynchronizedOperation(ReadRow, ex => OnProcessException(MessageLevel.Warning, ex));
        }

        if (TransverseMode)
        {
            if (settings.TryGetValue(nameof(AutoMapToOutputMeasurements), out setting))
                AutoMapToOutputMeasurements = setting.ParseBoolean();

            if (AutoMapToOutputMeasurements)
            {
                if (settings.TryGetValue(nameof(ColumnMappings), out setting) && !string.IsNullOrWhiteSpace(setting))
                    OnStatusMessage(MessageLevel.Warning, $"Manually assigned '{nameof(ColumnMappings)}' will be ignored since '{nameof(AutoMapToOutputMeasurements)}' is enabled in transverse mode.");

                if (OutputMeasurements?.Length == 0)
                    throw new InvalidOperationException($"Output measurements must be defined when using '{nameof(AutoMapToOutputMeasurements)}' in transverse mode.");

                if (settings.TryGetValue(nameof(TimestampColumnIndex), out setting) && int.TryParse(setting, out int index))
                    TimestampColumnIndex = index;

                // No need to assign mappings to outputs if they are already being used as source
                AutoAssignMappingsToOutputs = false;

                StringBuilder columnMappings = new();

                // Auto-assign column mappings based on output measurements
                bool timestampAssigned = false;

                for (int i = 0; i < OutputMeasurements!.Length; i++)
                {
                    if (i > 0)
                        columnMappings.Append(';');

                    if (i == TimestampColumnIndex)
                    {
                        columnMappings.Append($"{i}=Timestamp;");
                        timestampAssigned = true;
                    }

                    int columnIndex = timestampAssigned ? i + 1 : i;
                    columnMappings.Append($"{columnIndex}={OutputMeasurements[i].ID}");
                }

                if (TimestampColumnIndex >= OutputMeasurements.Length)
                    columnMappings.Append($";{OutputMeasurements.Length}=Timestamp");

                settings[nameof(ColumnMappings)] = columnMappings.ToString();
            }

            // Load column mappings:
            if (settings.TryGetValue(nameof(ColumnMappings), out setting))
            {
                Dictionary<int, string> columnMappings = new();

                foreach (KeyValuePair<string, string> mapping in setting.ParseKeyValuePairs())
                {
                    if (int.TryParse(mapping.Key, out int index))
                        columnMappings[index] = mapping.Value;
                }

                KeyValuePair<int, string> timestampColumn = columnMappings.FirstOrDefault(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0);
                bool timestampColumnDefined = timestampColumn.Value is not null;

                // Lookup timestamp index
                if (!SimulateTimestamp)
                {
                    if (!timestampColumnDefined)
                        throw new InvalidOperationException("No \"Timestamp\" column mapping found: when not simulating timestamp in transverse mode, one of the column mappings must be defined as \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                    // Assign configured timestamp column index when auto-mapping is disabled (value is used by SignalIDPublishOrder map)
                    if (!AutoMapToOutputMeasurements)
                        TimestampColumnIndex = timestampColumn.Key;
                }

                // Auto-assign output measurements based on column mappings
                IMeasurement[] outputMeasurements = columnMappings.Where(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) != 0).Select(IMeasurement (kvp) =>
                {
                    string measurementID = kvp.Value;
                    MeasurementKey key = ParseInputMeasurementKeys(DataSource, false, measurementID).FirstOrDefault() ?? MeasurementKey.Undefined;

                    if (key == MeasurementKey.Undefined)
                        throw new InvalidOperationException($"Unable to parse measurement identifier of expression \"{kvp.Key}={measurementID}\" defined in column mappings as a point tag, measurement key, Guid-based signal ID or filter expression.");

                    Measurement measurement = new() { Metadata = key.Metadata };
                    Debug.Assert(measurement.Metadata is not null);

                    // Associate measurement with column index
                    m_columnMappings[kvp.Key] = measurement;

                    return measurement;
                }).ToArray();

                // In transverse mode, maximum measurements per interval is set to maximum column index in input file
                MeasurementsPerInterval = m_columnMappings.Keys.Max();

                // If timestamp column is included within column mappings and its index is not last in the set,
                // increment measurements per interval so that publish count will include all mapped columns
                if (SimulateTimestamp || (timestampColumnDefined && TimestampColumnIndex < MeasurementsPerInterval))
                    MeasurementsPerInterval++;

                if (!AutoMapToOutputMeasurements && settings.TryGetValue(nameof(AutoAssignMappingsToOutputs), out setting))
                    AutoAssignMappingsToOutputs = setting.ParseBoolean();

                if (AutoAssignMappingsToOutputs)
                    OutputMeasurements = outputMeasurements;

                if (!SimulateTimestamp)
                {
                    // Reserve a column mapping for timestamp value
                    IMeasurement timestampMeasurement = new Measurement
                    {
                        Metadata = new MeasurementMetadata(null!, "Timestamp", 0, 1, null)
                    };

                    m_columnMappings[TimestampColumnIndex] = timestampMeasurement;
                }
            }
            else
            {
                throw new InvalidOperationException("Column mappings must be defined when using transverse format: e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA:13}.");
            }
        }

        // Override input interval based on temporal processing interval if it's not set to default
        if (ProcessingInterval > -1)
            InputInterval = ProcessingInterval == 0 ? 1 : ProcessingInterval;
    }

    /// <summary>
    /// Attempts to connect to this <see cref="CsvInputAdapter"/>.
    /// </summary>
    protected override void AttemptConnection()
    {
        m_inStream = new StreamReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

        // Skip configured number of header lines that exist before column heading definitions
        for (int i = 0; i < SkipRows; i++)
            m_inStream.ReadLine();

        m_columns.Clear();
        m_header = m_inStream.ReadLine();
        string[] headings = m_header.ToNonNullString().Split(',');

        for (int i = 0; i < headings.Length; i++)
            m_columns.Add(headings[i], i);

        if (UseHighResolutionInputTimer)
        {
            // Start a new thread to process measurements using precision timer
            new Thread(ProcessMeasurements)
            {
                IsBackground = true
            }
            .Start();
        }
        else
        {
            // Start replay timer
            m_pacingTimer!.SetNextPeriod();
            m_readRow!.RunOnceAsync();
        }
    }

    /// <summary>
    /// Attempts to disconnect from this <see cref="CsvInputAdapter"/>.
    /// </summary>
    protected override void AttemptDisconnection()
    {
        if (m_inStream is not null)
        {
            m_inStream.Close();
            m_inStream.Dispose();
        }

        m_inStream = null;
    }

    /// <summary>
    /// Gets a short one-line status of this <see cref="CsvInputAdapter"/>.
    /// </summary>
    /// <param name="maxLength">Maximum length of the status message.</param>
    /// <returns>Text of the status message.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return $"{ProcessedMeasurements} measurements read from CSV file.".CenterText(maxLength);
    }

    private void ReadRow()
    {
        if (!Enabled)
            return;

        if (ReadNextRecord(DateTime.UtcNow.Ticks))
        {
            m_pacingTimer!.WaitNext();
            m_readRow!.RunOnceAsync();
            return;
        }

        ReadComplete();

        if (!AutoRepeat)
            return;

        m_pacingTimer!.WaitNext();
        m_readRow!.RunOnceAsync();
    }

    // Handler for precision timer measurements processing
    private void ProcessMeasurements()
    {
        do
        {
            if (m_precisionTimer is null)
                return;

            // When high resolution input timing is requested, we only need to wait for the next signal...
            while (Enabled && ReadToFrame(m_precisionTimer.LastFrameTime))
                m_precisionTimer.FrameWaitHandle?.Wait();

            if (!Enabled)
                return;

            ReadComplete();
        }
        while (AutoRepeat);
    }

    private void ReadComplete()
    {
        if (!AutoRepeat)
        {
            AttemptDisconnection();
            return;
        }

        OnStatusMessage(MessageLevel.Info, "Restarting CSV read for auto-repeat.");
        Debug.Assert(m_inStream is not null);

        m_inStream!.BaseStream.Position = 0L;
        m_inStream.DiscardBufferedData();

        // Skip configured number of header lines that exist before column heading definitions
        for (int i = 0; i < SkipRows; i++)
            m_inStream.ReadLine();

        // Skip header line
        m_inStream.ReadLine();
    }

    // Attempt to read as many records as necessary to reach the target frame index
    private bool ReadToFrame(long targetFrameTime)
    {
        if (m_precisionTimer is null)
            return false;

        try
        {
            if (m_subsecondDistribution is null)
            {
                m_subsecondDistribution = Ticks.SubsecondDistribution(FrameRate)
                    .Select(subsecond => subsecond.Value)
                    .ToArray();

                return jumpToFrameTime();
            }

            if (m_subSecondCorrection)
                return jumpToFrameTime();

            if (targetFrameTime - m_previousSecond > TimeSpan.FromSeconds(5.0D).Ticks)
            {
                m_subSecondCorrection = true;
                return jumpToFrameTime();
            }

            long currentFrameTime = calculateFrameTime();

            while (currentFrameTime < targetFrameTime)
            {
                m_previousFrameIndex = (m_previousFrameIndex + 1) % FrameRate;

                if (m_previousFrameIndex == 0)
                    m_previousSecond += Ticks.PerSecond;

                currentFrameTime = calculateFrameTime();

                if (ReadNextRecord(currentFrameTime))
                    continue;

                if (m_previousFrameIndex == 0)
                {
                    m_previousSecond -= Ticks.PerSecond;
                    m_previousFrameIndex = FrameRate - 1;
                }
                else
                {
                    m_previousFrameIndex--;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, ex);
        }

        return true;

        long calculateFrameTime()
        {
            long subseconds = m_subsecondDistribution[m_previousFrameIndex];
            return m_previousSecond + subseconds;
        }

        bool jumpToFrameTime()
        {
            TimeSpan targetFrameSpan = TimeSpan.FromTicks(targetFrameTime);
            TimeSpan targetFrameSecond = TimeSpan.FromSeconds(Math.Truncate(targetFrameSpan.TotalSeconds));
            TimeSpan targetFrameSubsecond = targetFrameSpan - targetFrameSecond;

            m_previousSecond = targetFrameSecond.Ticks;
            m_previousFrameIndex = (int)Math.Round(targetFrameSubsecond.TotalSeconds * FrameRate);
            if (m_subSecondCorrection && m_previousFrameIndex != 0)
                return true;

            long frameTime = calculateFrameTime();
            return ReadNextRecord(frameTime, m_previousFrameIndex);
        }
    }

    // Attempt to read the next record
    private bool ReadNextRecord(long currentTime, int skip = 0)
    {
        if (m_inStream is null)
            return false;

        try
        {
            List<IMeasurement> newMeasurements = [];
            long fileTime = 0;
            int timestampColumn = 0;

            string? line;
            int index = 0;

            do
            {
                line = m_inStream.ReadLine();
                index++;
            } while (index <= skip);

            // Null line indicates end of file, return false
            if (line is null)
            {
                m_subSecondCorrection = true;
                return false;
            }

            // Parse line of CSV file accounting for quoted fields
            string[] fields = ParseCSVLine(line.Trim());
            m_subSecondCorrection = false;

            if (fields.Length < m_columns.Count)
                return false;

            // Read time from Timestamp column in transverse mode
            if (TransverseMode)
            {
                if (SimulateTimestamp)
                {
                    fileTime = currentTime;
                }
                else
                {
                    timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0).Key;
                    fileTime = long.Parse(fields[timestampColumn]);
                }
            }

            for (int i = 0; i < MeasurementsPerInterval; i++)
            {
                IMeasurement? measurement;

                if (TransverseMode)
                {
                    // No measurement will be defined for timestamp column
                    if (i == timestampColumn)
                        continue;

                    if (m_columnMappings.TryGetValue(i, out measurement))
                    {
                        measurement = Measurement.Clone(measurement);

                        try
                        {
                            measurement.Value = double.Parse(fields[i]);
                        }
                        catch
                        {
                            measurement.Value = double.NaN;
                        }
                    }
                    else
                    {
                        measurement = new Measurement();
                        measurement.Metadata = MeasurementKey.Undefined.Metadata;
                        measurement.Value = double.NaN;
                    }

                    if (SimulateTimestamp)
                        measurement.Timestamp = RoundTimestampsToFrameRate ? Ticks.RoundToSubsecondDistribution(currentTime, FrameRate) : currentTime;
                    else if (m_columns.ContainsKey("Timestamp"))
                        measurement.Timestamp = RoundTimestampsToFrameRate ? Ticks.RoundToSubsecondDistribution(fileTime, FrameRate) : fileTime;
                }
                else
                {
                    measurement = new Measurement();

                    if (m_columns.TryGetValue("Signal ID", out int idColumn))
                    {
                        Guid measurementID = new(fields[idColumn]);

                        measurement.Metadata = m_columns.TryGetValue("Measurement Key", out int keyColumn) ?
                            MeasurementKey.LookUpOrCreate(measurementID, fields[keyColumn]).Metadata :
                            MeasurementKey.LookUpBySignalID(measurementID).Metadata;
                    }
                    else if (m_columns.TryGetValue("Measurement Key", out int keyColumn))
                    {
                        measurement.Metadata = MeasurementKey.Parse(fields[keyColumn]).Metadata;
                    }

                    if (SimulateTimestamp)
                        measurement.Timestamp = RoundTimestampsToFrameRate ? Ticks.RoundToSubsecondDistribution(currentTime, FrameRate) : currentTime;
                    else if (m_columns.TryGetValue("Timestamp", out int timeColumn) && long.TryParse(fields[timeColumn], out fileTime))
                        measurement.Timestamp = RoundTimestampsToFrameRate ? Ticks.RoundToSubsecondDistribution(fileTime, FrameRate) : fileTime;
                    else
                        throw new FormatException($"Failed to parse timestamp value '{fields[timeColumn]}' as a long integer.");

                    if (m_columns.TryGetValue("Value", out int valueColumn))
                        measurement.Value = double.Parse(fields[valueColumn]);
                }

                newMeasurements.Add(measurement);
            }

            OnNewMeasurements(newMeasurements);
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, ex);
        }

        return true;
    }

    // Parse a CSV line into fields, accounting for quoted fields
    private static string[] ParseCSVLine(string line)
    {
        List<string> fields = [];
        bool inQuotes = false;
        bool fieldHasQuotes = false;
        StringBuilder fieldBuilder = new();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Peek ahead to see if it's an escaped quote (two quotes in a row)
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        fieldBuilder.Append('"');
                        i++; // Skip second quote
                    }
                    else
                    {
                        inQuotes = false; // End of quoted field
                    }
                }
                else
                {
                    fieldBuilder.Append(c);
                }
            }
            else
            {
                switch (c)
                {
                    case ',':
                        fields.Add(fieldHasQuotes ? fieldBuilder.ToString() : fieldBuilder.ToString().Trim());
                        fieldBuilder.Clear();
                        fieldHasQuotes = false;
                        break;
                    case '"':
                        inQuotes = true;
                        fieldHasQuotes = true;
                        break;
                    default:
                        fieldBuilder.Append(c);
                        break;
                }
            }
        }

        // Add last field
        fields.Add(fieldHasQuotes ? fieldBuilder.ToString() : fieldBuilder.ToString().Trim());

        return fields.ToArray();
    }

    #endregion
}