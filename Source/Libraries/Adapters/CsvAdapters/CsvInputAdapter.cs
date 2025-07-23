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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    // Fields
    private StreamReader? m_inStream;
    private string? m_header;
    private readonly Dictionary<string, int> m_columns;
    private readonly Dictionary<int, IMeasurement> m_columnMappings;

    private Timer? m_looseTimer;
    private LongSynchronizedOperation? m_readRow;

    private PrecisionInputTimer? m_precisionTimer;
    private long[]? m_subsecondDistribution;
    private long m_previousSecond;
    private int m_previousFrameIndex;

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
        InputInterval = 33.333333;
        MeasurementsPerInterval = 5;

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
    /// Gets or sets the interval of time between sending frames into the concentrator.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the interval of time, in milliseconds, between sending frames into the concentrator.")]
    [DefaultValue(33.333333)]
    public double InputInterval { get; set; }

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
    /// Gets or sets flag that determines if a high-resolution precision timer should be used for CSV file based input.
    /// </summary>
    /// <remarks>
    /// Useful when input frames need be accurately time-aligned to the local clock to better simulate
    /// an input device and calculate downstream latencies.<br/>
    /// This is only applicable when connection is made to a file for replay purposes.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Determines if a high-resolution precision timer should be used for CSV file based input.")]
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
                    m_precisionTimer = PrecisionInputTimer.Attach((int)(1000.0D / InputInterval), ex => OnProcessException(MessageLevel.Warning, ex));
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
    [DefaultValue(5)]
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
    /// Defines the column mappings, must be defined: e.g., 0=Timestamp; 1=PPA:12; 2=PPA13.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the column mappings must defined: e.g., \"0=Timestamp; 1=PPA:12; 2=PPA13\".")]
    [DefaultValue("")]
    public int ColumnMappings { get; set; }

    /// <summary>
    /// Gets a flag that determines if this <see cref="CsvInputAdapter"/>
    /// uses an asynchronous connection.
    /// </summary>
    protected override bool UseAsyncConnect => false;

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
            status.AppendLine($"               File header: {m_header}");
            status.AppendLine($"            Input interval: {InputInterval:N3}");
            status.AppendLine($" Measurements per interval: {MeasurementsPerInterval:N0}");
            status.AppendLine($"     Using transverse mode: {TransverseMode}");
            status.AppendLine($"               Auto-repeat: {AutoRepeat}");
            status.AppendLine($"     Precision input timer: {(UseHighResolutionInputTimer ? "Enabled" : "Offline")}");
            status.AppendLine($"             Lines to skip: {SkipRows:N0}");

            if (m_precisionTimer is not null)
                status.AppendLine($"  Timer resynchronizations: {m_precisionTimer.Resynchronizations}");

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
            else
                m_looseTimer?.Dispose();

            m_looseTimer = null;
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

        if (settings.TryGetValue(nameof(MeasurementsPerInterval), out setting))
            MeasurementsPerInterval = int.Parse(setting);

        if (settings.TryGetValue(nameof(SimulateTimestamp), out setting))
            SimulateTimestamp = setting.ParseBoolean();

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
            m_looseTimer = new Timer();
            m_readRow = new LongSynchronizedOperation(ReadRow, ex => OnProcessException(MessageLevel.Warning, ex));
        }

        if (TransverseMode)
        {
            // Load column mappings:
            if (settings.TryGetValue(nameof(ColumnMappings), out setting))
            {
                Dictionary<int, string> columnMappings = new();

                foreach (KeyValuePair<string, string> mapping in setting.ParseKeyValuePairs())
                {
                    if (int.TryParse(mapping.Key, out int index))
                        columnMappings[index] = mapping.Value;
                }

                if (!SimulateTimestamp && !columnMappings.Values.Contains("Timestamp", StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException("One of the column mappings must be defined as a \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                // In transverse mode, maximum measurements per interval is set to maximum columns in input file
                MeasurementsPerInterval = columnMappings.Keys.Max() + 1;

                // Auto-assign output measurements based on column mappings
                OutputMeasurements = columnMappings.Where(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) != 0).Select(IMeasurement (kvp) =>
                {
                    string measurementID = kvp.Value;
                    Measurement measurement = new();
                    MeasurementKey key;

                    if (Guid.TryParse(measurementID, out Guid id))
                        key = MeasurementKey.LookUpBySignalID(id);
                    else
                        MeasurementKey.TryParse(measurementID, out key);

                    measurement.Metadata = key.Metadata;

                    // Associate measurement with column index
                    m_columnMappings[kvp.Key] = measurement;

                    return measurement;
                }).ToArray();

                if (!SimulateTimestamp)
                {
                    int timestampColumn = columnMappings.First(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0).Key;

                    // Reserve a column mapping for timestamp value
                    IMeasurement timestampMeasurement = new Measurement
                    {
                        Metadata = new MeasurementMetadata(null!, "Timestamp", 0, 1, null)
                    };

                    m_columnMappings[timestampColumn] = timestampMeasurement;
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

        if (m_looseTimer is null)
            return;

        m_looseTimer.Interval = InputInterval;
        m_looseTimer.AutoReset = true;
        m_looseTimer.Elapsed += m_looseTimer_Elapsed;
    }

    /// <summary>
    /// Attempts to connect to this <see cref="CsvInputAdapter"/>.
    /// </summary>
    protected override void AttemptConnection()
    {
        m_inStream = new StreamReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

        // Skip specified number of header lines that exist before column heading definitions
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
            // Start common timer
            m_looseTimer!.Start();
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

        if (!UseHighResolutionInputTimer)
            m_looseTimer!.Stop();
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

    // Handler for loose timer measurements processing
    private void m_looseTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        m_readRow!.Run();
    }

    private void ReadRow()
    {
        if (!Enabled)
            return;

        if (ReadNextRecord(DateTime.UtcNow.Ticks))
            return;

        ReadComplete();
    }

    // Handler for precision timer measurements processing
    private void ProcessMeasurements()
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

    private void ReadComplete()
    {
        AttemptDisconnection();

        if (!AutoRepeat)
            return;

        OnStatusMessage(MessageLevel.Info, "Restarting CSV read for auto-repeat.");
        AttemptConnection();
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
                m_subsecondDistribution = Ticks.SubsecondDistribution(m_precisionTimer.FramesPerSecond)
                    .Select(subsecond => subsecond.Value)
                    .ToArray();

                return jumpToFrameTime();
            }

            if (targetFrameTime - m_previousSecond > TimeSpan.FromSeconds(5.0D).Ticks)
                return jumpToFrameTime();

            long currentFrameTime = calculateFrameTime();

            while (currentFrameTime < targetFrameTime)
            {
                m_previousFrameIndex = (m_previousFrameIndex + 1) % m_precisionTimer.FramesPerSecond;

                if (m_previousFrameIndex == 0)
                    m_previousSecond += Ticks.PerSecond;

                currentFrameTime = calculateFrameTime();

                if (!ReadNextRecord(currentFrameTime))
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
            m_previousFrameIndex = (int)Math.Round(targetFrameSubsecond.TotalSeconds * m_precisionTimer.FramesPerSecond);

            long frameTime = calculateFrameTime();
            return ReadNextRecord(frameTime);
        }
    }

    // Attempt to read the next record
    private bool ReadNextRecord(long currentTime)
    {
        if (m_inStream is null)
            return false;

        try
        {
            List<IMeasurement> newMeasurements = [];
            long fileTime = 0;
            int timestampColumn = 0;

            string? line = m_inStream.ReadLine();

            // Null line indicates end of file, return false
            if (line is null)
                return false;

            // Parse line of CSV file accounting for quoted fields
            string[] fields = ParseCsvLine(line.Trim());

            if (m_inStream.EndOfStream || fields.Length < m_columns.Count)
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
                    if (!SimulateTimestamp && i == timestampColumn)
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
                        measurement.Timestamp = currentTime;
                    else if (m_columns.ContainsKey("Timestamp"))
                        measurement.Timestamp = fileTime;
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
                        measurement.Timestamp = currentTime;
                    else if (m_columns.TryGetValue("Timestamp", out int timeColumn))
                        measurement.Timestamp = long.Parse(fields[timeColumn]);

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
    private static string[] ParseCsvLine(string line)
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