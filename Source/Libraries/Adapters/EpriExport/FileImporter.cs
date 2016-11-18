//******************************************************************************************************
//  FileImporter.cs - Gbtc
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
//  09/26/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Collections;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Timer = System.Timers.Timer;

namespace EpriExport
{
    /// <summary>
    /// Represents an input adapter that reads measurements from an EPRI export file.
    /// </summary>
    [Description("EPRI File Based Data Importer: Reads measurements from an EPRI export file")]
    public class FileImporter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_importPath;
        private string m_fileName;
        private StreamReader m_fileStream;
        private string m_header;
        private readonly Dictionary<string, int> m_columns;
        private readonly Dictionary<int, IMeasurement> m_columnMappings;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private int m_skipRows;
        private bool m_simulateTimestamp;
        private Timer m_looseTimer;
        private PrecisionInputTimer m_precisionTimer;
        private SafeFileWatcher m_fileSystemWatcher;
        private AsyncQueue<string> m_fileProcessQueue;
        private long m_processedFiles;
        private string m_timestampFormat;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImporter"/> class.
        /// </summary>
        public FileImporter()
        {
            m_fileProcessQueue = new AsyncQueue<string>();
            m_fileProcessQueue.ProcessItemFunction = ProcessFile;
            m_fileProcessQueue.ProcessException += m_fileProcessQueue_ProcessException;

            m_columns = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            m_columnMappings = new Dictionary<int, IMeasurement>();

            m_inputInterval = 5.0D;
            m_measurementsPerInterval = 13;
            m_skipRows = 2;
            m_timestampFormat = "dd-MMM-yyyy HH:mm:ss.fff";

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the import path for EPRI CSV files from which measurements will be read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the import path for EPRI CSV files from which measurements will be read."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FolderBrowserEditor")]
        public string ImportPath
        {
            get
            {
                return m_importPath;
            }
            set
            {
                m_importPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval of time between sending frames into the concentrator.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval of time, in milliseconds, between sending frames into the concentrator."),
        DefaultValue(5.0D)]
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
        /// Gets or sets number of lines to skip in the source file before the header line is encountered.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of lines to skip in the source file before the header line is encountered."),
        DefaultValue(2)]
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
        /// Gets or sets timestamp format for the file.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the timestamp format for the file."),
        DefaultValue("dd-MMM-yyyy HH:mm:ss.fff")]
        public string TimestampFormat
        {
            get
            {
                return m_timestampFormat;
            }
            set
            {
                m_timestampFormat = value;
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
                return ((object)m_precisionTimer != null);
            }
            set
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                if (value && (object)m_precisionTimer == null && !Debugger.IsAttached)
                    m_precisionTimer = PrecisionInputTimer.Attach((int)(1000.0D / m_inputInterval), OnProcessException);
                else if (!value && m_precisionTimer != null)
                    PrecisionInputTimer.Detach(ref m_precisionTimer);
            }
        }

        /// <summary>
        /// Gets or sets the number of measurements that are read from the CSV file in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements that are read from the CSV file in each frame."),
        DefaultValue(13)]
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
        /// Gets a flag that determines if this <see cref="FileImporter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the detailed status of this <see cref="FileImporter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("   Queued files to process: {0}", m_fileProcessQueue.Count);
                status.AppendLine();
                status.AppendFormat("      Currently processing: {0}", FilePath.GetFileName(m_fileName));
                status.AppendLine();
                status.AppendFormat("     Total processed files: {0}", m_processedFiles);
                status.AppendLine();
                status.AppendFormat("               Import path: {0}", m_importPath);
                status.AppendLine();
                status.AppendFormat("               File header: {0}", m_header);
                status.AppendLine();
                status.AppendFormat("            Input interval: {0}", m_inputInterval);
                status.AppendLine();
                status.AppendFormat(" Measurements per interval: {0}", m_measurementsPerInterval);
                status.AppendLine();
                status.AppendFormat("     Precision input timer: {0}", UseHighResolutionInputTimer ? "Enabled" : "Offline");
                status.AppendLine();
                status.AppendFormat("             Lines to skip: {0}", m_skipRows);
                status.AppendLine();
                status.AppendFormat("          Timestamp format: {0}", m_timestampFormat);
                status.AppendLine();

                if (m_precisionTimer != null)
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
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileImporter"/> object and optionally releases the managed resources.
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
                        if ((object)m_fileSystemWatcher != null)
                        {
                            m_fileSystemWatcher.Created -= m_fileSystemWatcher_Created;
                            m_fileSystemWatcher.Renamed -= m_fileSystemWatcher_Renamed;
                            m_fileSystemWatcher.Dispose();
                        }
                        m_fileSystemWatcher = null;

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

                        if ((object)m_fileStream != null)
                        {
                            m_fileStream.Close();
                            m_fileStream.Dispose();
                        }
                        m_fileStream = null;

                        if ((object)m_fileProcessQueue != null)
                            m_fileProcessQueue.ProcessException -= m_fileProcessQueue_ProcessException;

                        m_fileProcessQueue = null;

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
        /// Initializes this <see cref="FileImporter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("importPath", out setting))
                m_importPath = setting;
            else
                throw new InvalidOperationException("No import path was specified for EPRI file importer - this is a required setting.");

            if (settings.TryGetValue("inputInterval", out setting))
                m_inputInterval = double.Parse(setting);

            if (settings.TryGetValue("measurementsPerInterval", out setting))
                m_measurementsPerInterval = int.Parse(setting);

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("timestampFormat", out setting))
                m_timestampFormat = setting;

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
                m_measurementsPerInterval = columnMappings.Keys.Max();

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

                    if (key.SignalID != Guid.Empty)
                    {
                        measurement.Metadata = key.Metadata;

                        // Associate measurement with column index
                        m_columnMappings[kvp.Key] = measurement;
                    }

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

            // Override input interval based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
                m_inputInterval = ProcessingInterval == 0 ? 1 : ProcessingInterval;

            if ((object)m_looseTimer != null)
            {
                m_looseTimer.Interval = m_inputInterval;
                m_looseTimer.AutoReset = true;
                m_looseTimer.Elapsed += m_looseTimer_Elapsed;
            }

            m_fileSystemWatcher = new SafeFileWatcher(m_importPath, "EPRI-VS-Output-*.csv");
            m_fileSystemWatcher.Created += m_fileSystemWatcher_Created;
            m_fileSystemWatcher.Renamed += m_fileSystemWatcher_Renamed;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="FileImporter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            // Add any existing files that need processing
            List<string> existingFiles = new List<string>(FilePath.GetFileList(Path.Combine(m_importPath, m_fileSystemWatcher.Filter)));

            existingFiles.Sort();

            foreach (string fileName in existingFiles)
            {
                m_fileProcessQueue.Enqueue(fileName);
            }

            if ((object)m_fileSystemWatcher != null && !m_fileSystemWatcher.EnableRaisingEvents)
                m_fileSystemWatcher.EnableRaisingEvents = true;

            m_fileProcessQueue.Enabled = true;
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="FileImporter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_fileSystemWatcher != null)
                m_fileSystemWatcher.EnableRaisingEvents = false;

            m_fileProcessQueue.Enabled = false;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FileImporter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"{ProcessedMeasurements} measurements read from CSV file.".CenterText(maxLength);
        }

        private void m_fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            m_fileProcessQueue.Enqueue(e.FullPath);
        }

        private void m_fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            m_fileProcessQueue.Enqueue(e.FullPath);
        }

        private void ProcessFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            m_fileName = fileName;
            OnStatusMessage("Processing EPRI file \"{0}\"...", m_fileName);

            FilePath.WaitForReadLock(m_fileName);
            m_fileStream = new StreamReader(m_fileName);

            // Skip specified number of header lines that exist before column heading definitions
            for (int i = 0; i < m_skipRows; i++)
            {
                m_fileStream.ReadLine();
            }

            m_header = m_fileStream.ReadLine();
            m_columns.Clear();

            string[] headings = m_header.ToNonNullString().Split(',');

            for (int i = 0; i < headings.Length; i++)
            {
                m_columns.Add(headings[i], i);
            }

            if (UseHighResolutionInputTimer)
            {
                // Start a new thread to process measurements using precision timer
                (new Thread(ProcessMeasurements)).Start();
            }
            else
            {
                // Start common timer
                m_looseTimer.Start();
            }

            // Wait until file stream is closed
            while ((object)m_fileStream != null)
            {
                Thread.Sleep(100);
            }
        }

        private void CloseFile()
        {
            if (!UseHighResolutionInputTimer)
                m_looseTimer.Stop();

            if ((object)m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
            }

            OnStatusMessage("Completed processing of EPRI file \"{0}\".", m_fileName);

            ThreadPool.QueueUserWorkItem(DeleteFile, m_fileName);

            m_fileStream = null;
            m_processedFiles++;
        }

        private void DeleteFile(object state)
        {
            string fileName = state as string;

            try
            {
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Failed to delete file \"{fileName}\": {ex.Message}", ex));
            }
        }

        private void ProcessMeasurements()
        {
            while (Enabled && ReadNextRecord(m_precisionTimer.LastFrameTime))
            {
                // When high resolution input timing is requested, we only need to wait for the next signal...
                m_precisionTimer.FrameWaitHandle.Wait();
            }

            CloseFile();
        }

        private void m_looseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Enabled || !ReadNextRecord(DateTime.UtcNow.Ticks))
                CloseFile();
        }

        // Attempt to read the next record
        private bool ReadNextRecord(long currentTime)
        {
            try
            {
                List<IMeasurement> newMeasurements = new List<IMeasurement>();
                long fileTime;
                int timestampColumn = 0;
                string line = m_fileStream.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    return false;

                string[] fields = line.Split(',');

                if (fields.Length <= 0)
                    return false;

                // Read time from Timestamp column
                if (m_simulateTimestamp)
                {
                    fileTime = currentTime;
                }
                else
                {
                    timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", StringComparison.OrdinalIgnoreCase) == 0).Key;
                    fileTime = DateTime.ParseExact(fields[timestampColumn].Trim(), m_timestampFormat, CultureInfo.InvariantCulture).Ticks;
                }

                for (int i = 0; i <= m_measurementsPerInterval; i++)
                {
                    IMeasurement measurement;

                    // No measurement will be defined for timestamp column
                    if (i == timestampColumn)
                        continue;

                    if (m_columnMappings.TryGetValue(i, out measurement))
                    {
                        double value;

                        measurement = Measurement.Clone(measurement);

                        if (!double.TryParse(fields[i], out value))
                            value = fields[i].ParseBoolean() ? 1.0D : 0.0D;

                        measurement.Value = value;
                    }
                    else
                    {
                        continue;
                    }

                    if (m_simulateTimestamp)
                        measurement.Timestamp = currentTime;
                    else if (m_columns.ContainsKey("Timestamp"))
                        measurement.Timestamp = fileTime;

                    newMeasurements.Add(measurement);
                }

                OnNewMeasurements(newMeasurements);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
                return false;
            }

            return true;
        }

        private void m_fileProcessQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion
    }
}
