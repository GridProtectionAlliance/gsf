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
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a CSV file.
    /// </summary>
    [Description("CSV: Reads measurements from a CSV file")]
    public class CsvInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_fileName;
        private StreamReader m_inStream;
        private string m_header;
        private readonly Dictionary<string, int> m_columns;
        private readonly Dictionary<int, IMeasurement> m_columnMappings;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private int m_skipRows;
        private bool m_simulateTimestamp;
        private bool m_transverse;
        private bool m_autoRepeat;
        private Timer m_looseTimer;
        private PrecisionInputTimer m_precisionTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvInputAdapter"/> class.
        /// </summary>
        public CsvInputAdapter()
        {
            m_fileName = "measurements.csv";
            m_columns = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            m_columnMappings = new Dictionary<int, IMeasurement>();
            m_inputInterval = 33.333333;
            m_measurementsPerInterval = 5;

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the CSV file from which measurements will be read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the CSV file from which measurements will be read."),
        DefaultValue("measurements.csv"),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FileDialogEditor", "type=open; checkFileExists=true; defaultExt=.csv; filter=CSV files|*.csv|AllFiles|*.*")]
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval of time between sending frames into the concentrator.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval of time, in milliseconds, between sending frames into the concentrator."),
        DefaultValue(33.333333)]
        public double InputInterval
        {
            get
            {
                return m_inputInterval;
            }
            set
            {
                m_inputInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines if the CSV input file data should be replayed repeatedly.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define if the CSV input file data should be replayed repeatedly."),
        DefaultValue(false)]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets number of lines to skip in the source file before the header line is encountered.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of lines to skip in the source file before the header line is encountered."),
        DefaultValue(0)]
        public int SkipRows
        {
            get
            {
                return m_skipRows;
            }
            set
            {
                m_skipRows = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if a high-resolution precision timer should be used for CSV file based input.
        /// </summary>
        /// <remarks>
        /// Useful when input frames need be accurately time-aligned to the local clock to better simulate
        /// an input device and calculate downstream latencies.<br/>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Determines if a high-resolution precision timer should be used for CSV file based input."),
        DefaultValue(false)]
        public bool UseHighResolutionInputTimer
        {
            get
            {
                return (object)m_precisionTimer != null;
            }
            set
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                if (value && (object)m_precisionTimer == null && !Debugger.IsAttached)
                    m_precisionTimer = PrecisionInputTimer.Attach((int)(1000.0D / m_inputInterval), ex => OnProcessException(MessageLevel.Warning, "CsvInputAdapter", ex));
                else if (!value && m_precisionTimer != null)
                    PrecisionInputTimer.Detach(ref m_precisionTimer);
            }
        }

        /// <summary>
        /// Gets or sets the number of measurements that are read from the CSV file in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements that are read from the CSV file in each frame."),
        DefaultValue(5)]
        public int MeasurementsPerInterval
        {
            get
            {
                return m_measurementsPerInterval;
            }
            set
            {
                m_measurementsPerInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether CSV file is in transverse mode for real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether CSV file is in transverse mode for real-time concentration."),
        DefaultValue(false)]
        public bool TransverseMode
        {
            get
            {
                return m_transverse;
            }
            set
            {
                m_transverse = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether timestamps are
        /// simulated for the purposes of real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps are simulated for real-time concentration."),
        DefaultValue(false)]
        public bool SimulateTimestamp
        {
            get
            {
                return m_simulateTimestamp;
            }
            set
            {
                m_simulateTimestamp = value;
            }
        }

        /// <summary>
        /// Defines the column mappings must defined: e.g., 0=Timestamp; 1=PPA:12; 2=PPA13.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the column mappings must defined: e.g., \"0=Timestamp; 1=PPA:12; 2=PPA13\"."),
        DefaultValue("")]
        public int ColumnMappings
        {
            get;
            set;
        }

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
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("                 File name: {0}", m_fileName);
                status.AppendLine();
                status.AppendFormat("               File header: {0}", m_header);
                status.AppendLine();
                status.AppendFormat("            Input interval: {0}", m_inputInterval);
                status.AppendLine();
                status.AppendFormat(" Measurements per interval: {0}", m_measurementsPerInterval);
                status.AppendLine();
                status.AppendFormat("     Using traverse format: {0}", m_transverse);
                status.AppendLine();
                status.AppendFormat("               Auto-repeat: {0}", m_autoRepeat);
                status.AppendLine();
                status.AppendFormat("     Precision input timer: {0}", UseHighResolutionInputTimer ? "Enabled" : "Offline");
                status.AppendLine();
                status.AppendFormat("             Lines to skip: {0}", m_skipRows);
                status.AppendLine();

                if ((object)m_precisionTimer != null)
                {
                    status.AppendFormat("  Timer resynchronizations: {0}", m_precisionTimer.Resynchronizations);
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
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (UseHighResolutionInputTimer)
                        {
                            PrecisionInputTimer.Detach(ref m_precisionTimer);
                        }
                        else if ((object)m_looseTimer != null)
                        {
                            m_looseTimer.Stop();
                            m_looseTimer.Dispose();
                        }
                        m_looseTimer = null;

                        if ((object)m_inStream != null)
                        {
                            m_inStream.Close();
                            m_inStream.Dispose();
                        }

                        m_inStream = null;

                        // Clear minimum timer resolution.
                        PrecisionTimer.ClearMinimumTimerResolution(1);
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes this <see cref="CsvInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("fileName", out setting))
                m_fileName = setting;

            if (settings.TryGetValue("inputInterval", out setting))
                m_inputInterval = double.Parse(setting);

            if (settings.TryGetValue("measurementsPerInterval", out setting))
                m_measurementsPerInterval = int.Parse(setting);

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("transverse", out setting) || settings.TryGetValue("transverseMode", out setting))
                m_transverse = setting.ParseBoolean();

            if (settings.TryGetValue("autoRepeat", out setting))
                m_autoRepeat = setting.ParseBoolean();

            if (settings.TryGetValue("skipRows", out setting))
                int.TryParse(setting, out m_skipRows);

            if (m_skipRows < 0)
                m_skipRows = 0;

            settings.TryGetValue("useHighResolutionInputTimer", out setting);

            if (string.IsNullOrEmpty(setting))
                setting = "false";

            UseHighResolutionInputTimer = setting.ParseBoolean();

            if (!UseHighResolutionInputTimer)
                m_looseTimer = new Timer();

            if (m_transverse)
            {
                // Load column mappings:
                if (settings.TryGetValue("columnMappings", out setting))
                {
                    Dictionary<int, string> columnMappings = new Dictionary<int, string>();
                    int index;

                    foreach (KeyValuePair<string, string> mapping in setting.ParseKeyValuePairs())
                    {
                        if (int.TryParse(mapping.Key, out index))
                            columnMappings[index] = mapping.Value;
                    }

                    if (!m_simulateTimestamp && !columnMappings.Values.Contains("Timestamp", StringComparer.OrdinalIgnoreCase))
                        throw new InvalidOperationException("One of the column mappings must be defined as a \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                    // In transverse mode, maximum measurements per interval is set to maximum columns in input file
                    m_measurementsPerInterval = columnMappings.Keys.Max() + 1;

                    // Auto-assign output measurements based on column mappings
                    OutputMeasurements = columnMappings.Where(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) != 0).Select(kvp =>
                    {
                        string measurementID = kvp.Value;
                        IMeasurement measurement = new Measurement();
                        MeasurementKey key;
                        Guid id;

                        if (Guid.TryParse(measurementID, out id))
                        {
                            key = MeasurementKey.LookUpBySignalID(id);
                        }
                        else
                        {
                            MeasurementKey.TryParse(measurementID, out key);
                        }

                        measurement.Metadata = key.Metadata;

                        // Associate measurement with column index
                        m_columnMappings[kvp.Key] = measurement;

                        return measurement;
                    }).ToArray();

                    int timestampColumn = columnMappings.First(kvp => string.Compare(kvp.Value, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0).Key;

                    // Reserve a column mapping for timestamp value
                    IMeasurement timestampMeasurement = new Measurement
                    {
                        Metadata = new MeasurementMetadata(null, "Timestamp", 0, 1, null)
                    };

                    m_columnMappings[timestampColumn] = timestampMeasurement;
                }
                else
                {
                    throw new InvalidOperationException("Column mappings must be defined when using transverse format: e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");
                }
            }

            // Override input interval based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
                m_inputInterval = ProcessingInterval == 0 ? 1 : ProcessingInterval;

            if ((object)m_looseTimer != null)
            {
                m_looseTimer.Interval = m_inputInterval;
                m_looseTimer.AutoReset = true;
                m_looseTimer.Elapsed += m_looseTimer_Elapsed;
            }
        }

        /// <summary>
        /// Attempts to connect to this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            string[] headings;

            m_inStream = new StreamReader(File.Open(m_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // Skip specified number of header lines that exist before column heading definitions
            for (int i = 0; i < m_skipRows; i++)
                m_inStream.ReadLine();

            m_columns.Clear();
            m_header = m_inStream.ReadLine();
            headings = m_header.ToNonNullString().Split(',');

            for (int i = 0; i < headings.Length; i++)
                m_columns.Add(headings[i], i);

            if (UseHighResolutionInputTimer)
            {
                // Start a new thread to process measurements using precision timer
                new Thread(ProcessMeasurements).Start();
            }
            else
            {
                // Start common timer
                m_looseTimer.Start();
            }
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_inStream != null)
            {
                m_inStream.Close();
                m_inStream.Dispose();
            }

            m_inStream = null;

            if (!UseHighResolutionInputTimer)
                m_looseTimer.Stop();
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

        private void ProcessMeasurements()
        {
            while (Enabled && ReadNextRecord(m_precisionTimer.LastFrameTime))
            {
                // When high resolution input timing is requested, we only need to wait for the next signal...
                m_precisionTimer.FrameWaitHandle.Wait();
            }

            if (Enabled)
            {
                Stop();

                if (m_autoRepeat)
                    Start();
            }
        }

        private void m_looseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!ReadNextRecord(DateTime.UtcNow.Ticks) && Enabled)
            {
                Stop();

                if (m_autoRepeat)
                    Start();
            }
        }

        // Attempt to read the next record
        private bool ReadNextRecord(long currentTime)
        {
            try
            {
                List<IMeasurement> newMeasurements = new List<IMeasurement>();
                long fileTime = 0;
                int timestampColumn = 0;
                string[] fields = m_inStream.ReadLine().ToNonNullString().Split(',');

                if (m_inStream.EndOfStream || fields.Length < m_columns.Count)
                    return false;

                // Read time from Timestamp column in transverse mode
                if (m_transverse)
                {
                    if (m_simulateTimestamp)
                    {
                        fileTime = currentTime;
                    }
                    else
                    {
                        timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0).Key;
                        fileTime = long.Parse(fields[timestampColumn]);
                    }
                }

                for (int i = 0; i < m_measurementsPerInterval; i++)
                {
                    IMeasurement measurement;

                    if (m_transverse)
                    {
                        // No measurement will be defined for timestamp column
                        if (i == timestampColumn)
                            continue;

                        if (m_columnMappings.TryGetValue(i, out measurement))
                        {
                            measurement = Measurement.Clone(measurement);
                            measurement.Value = double.Parse(fields[i]);
                        }
                        else
                        {
                            measurement = new Measurement();
                            measurement.Metadata = MeasurementKey.Undefined.Metadata;
                            measurement.Value = double.NaN;
                        }

                        if (m_simulateTimestamp)
                            measurement.Timestamp = currentTime;
                        else if (m_columns.ContainsKey("Timestamp"))
                            measurement.Timestamp = fileTime;
                    }
                    else
                    {
                        measurement = new Measurement();

                        if (m_columns.ContainsKey("Signal ID"))
                        {
                            Guid measurementID = new Guid(fields[m_columns["Signal ID"]]);

                            if (m_columns.ContainsKey("Measurement Key"))
                                measurement.Metadata = MeasurementKey.LookUpOrCreate(measurementID, fields[m_columns["Measurement Key"]]).Metadata;
                            else
                                measurement.Metadata = MeasurementKey.LookUpBySignalID(measurementID).Metadata;
                        }
                        else if (m_columns.ContainsKey("Measurement Key"))
                        {
                            measurement.Metadata = MeasurementKey.Parse(fields[m_columns["Measurement Key"]]).Metadata;
                        }

                        if (m_simulateTimestamp)
                            measurement.Timestamp = currentTime;
                        else if (m_columns.ContainsKey("Timestamp"))
                            measurement.Timestamp = long.Parse(fields[m_columns["Timestamp"]]);

                        if (m_columns.ContainsKey("Value"))
                            measurement.Value = double.Parse(fields[m_columns["Value"]]);
                    }

                    newMeasurements.Add(measurement);
                }

                OnNewMeasurements(newMeasurements);
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, "CsvInputAdapter", ex);
            }

            return true;
        }

        #endregion
    }
}
