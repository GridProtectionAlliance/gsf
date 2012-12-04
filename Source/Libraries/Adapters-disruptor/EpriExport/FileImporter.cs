//******************************************************************************************************
//  FileImporter.cs - Gbtc
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
//  09/26/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF;
using GSF.IO;

namespace EpriExport
{
    /// <summary>
    /// Represents an input adapter that reads measurements from an EPRI export file.
    /// </summary>
    [Description("EPRI: reads measurements from an EPRI export file.")]
    public class FileImporter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_importPath;
        private string m_fileName;
        private StreamReader m_inStream;
        private string m_header;
        private readonly Dictionary<string, int> m_columns;
        private readonly Dictionary<int, IMeasurement> m_columnMappings;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private int m_skipRows;
        private bool m_simulateTimestamp;
        private System.Timers.Timer m_looseTimer;
        private PrecisionInputTimer m_precisionTimer;
        private System.Timers.Timer m_fileScanner;
        private BlockingCollection<string> m_fileNames;
        private List<string> m_processedFileList;
        private long m_processedFiles;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImporter"/> class.
        /// </summary>
        public FileImporter()
        {
            m_fileNames = new BlockingCollection<string>();
            m_processedFileList = new List<string>();
            m_columns = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            m_columnMappings = new Dictionary<int, IMeasurement>();
            m_inputInterval = 33.333333;
            m_measurementsPerInterval = 13;
            m_skipRows = 2;

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the import path for EPRI CSV files from which measurements will be read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the import path for EPRI CSV files from which measurements will be read.")]
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
                if (value && (object)m_precisionTimer == null && !System.Diagnostics.Debugger.IsAttached)
                    m_precisionTimer = PrecisionInputTimer.Attach((int)(1000.0D / m_inputInterval), OnProcessException);
                else if (!value && m_precisionTimer != null)
                    PrecisionInputTimer.Detach(ref m_precisionTimer);
            }
        }

        /// <summary>
        /// Gets or sets the number of measurements that are read from the CSV file in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements measurements that are read from the CSV file in each frame."),
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
                status.AppendFormat("   Queued files to process: {0}", m_fileNames.Count);
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
                        if ((object)m_fileScanner != null)
                        {
                            m_fileScanner.Stop();
                            m_fileScanner.Dispose();
                        }
                        m_fileScanner = null;

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

                        if ((object)m_fileNames != null)
                            m_fileNames.Dispose();

                        m_fileNames = null;

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

            if (settings.TryGetValue("skipRows", out setting))
                int.TryParse(setting, out m_skipRows);

            if (m_skipRows < 0)
                m_skipRows = 0;

            settings.TryGetValue("useHighResolutionInputTimer", out setting);

            if (string.IsNullOrEmpty(setting))
                setting = "false";

            UseHighResolutionInputTimer = setting.ParseBoolean();

            if (!UseHighResolutionInputTimer)
                m_looseTimer = new System.Timers.Timer();

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

                if (!m_simulateTimestamp && !columnMappings.Values.Contains("Timestamp", StringComparer.InvariantCultureIgnoreCase))
                    throw new InvalidOperationException("One of the column mappings must be defined as a \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                // In transverse mode, maximum measurements per interval is set to maximum columns in input file
                m_measurementsPerInterval = columnMappings.Keys.Max() + 1;

                // Auto-assign output measurements based on column mappings
                OutputMeasurements = columnMappings.Where(kvp => string.Compare(kvp.Value, "Timestamp", true) != 0).Select(kvp =>
                {
                    string measurementID = kvp.Value;
                    IMeasurement measurement = new Measurement();
                    MeasurementKey key;
                    Guid id;

                    if (Guid.TryParse(measurementID, out id))
                    {
                        measurement.ID = id;
                        measurement.Key = MeasurementKey.LookupBySignalID(id);
                    }
                    else if (MeasurementKey.TryParse(measurementID, Guid.Empty, out key))
                    {
                        measurement.Key = key;
                        measurement.ID = key.SignalID;
                    }

                    try
                    {
                        DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}'", measurement.ID));

                        if (filteredRows.Length > 0)
                        {
                            DataRow row = filteredRows[0];

                            // Assign other attributes
                            measurement.TagName = row["PointTag"].ToNonNullString();
                            measurement.Multiplier = double.Parse(row["Multiplier"].ToString());
                            measurement.Adder = double.Parse(row["Adder"].ToString());
                        }
                    }
                    catch
                    {
                        // Failure to lookup extra metadata is not catastrophic
                    }

                    // Associate measurement with column index
                    m_columnMappings[kvp.Key] = measurement;

                    return measurement;
                }).ToArray();

                int timestampColumn = columnMappings.First(kvp => string.Compare(kvp.Value, "Timestamp", true) == 0).Key;

                // Reserve a column mapping for timestamp value
                IMeasurement timestampMeasurement = new Measurement()
                {
                    TagName = "Timestamp"
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

            m_fileScanner = new System.Timers.Timer();
            m_fileScanner.Interval = 1000.0D;
            m_fileScanner.AutoReset = true;
            m_fileScanner.Elapsed += m_fileScanner_Elapsed;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="FileImporter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            if ((object)m_fileScanner != null)
                m_fileScanner.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="FileImporter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_fileScanner != null)
                m_fileScanner.Stop();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FileImporter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from CSV file.", ProcessedMeasurements).CenterText(maxLength);
        }

        private void m_fileScanner_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<string> fileNames = new List<string>();

            fileNames.AddRange(FilePath.GetFileList(Path.Combine(m_importPath, "EPRI-VS-Output-*.csv")));
            fileNames.Sort();

            foreach (string fileName in fileNames)
            {
                lock (m_processedFileList)
                {
                    if (!m_fileNames.Contains(fileName, StringComparer.InvariantCultureIgnoreCase) &&
                        !m_processedFileList.Contains(fileName, StringComparer.InvariantCultureIgnoreCase))
                        m_fileNames.Add(fileName);
                }
            }

            if (m_fileNames.Count > 0 && (object)m_inStream == null)
                ThreadPool.QueueUserWorkItem(ReadNextFileName);
        }

        private void ReadNextFileName(object state)
        {
            string fileName;

            if ((object)m_inStream == null && m_fileNames.TryTake(out fileName, 1000))
            {
                lock (m_processedFileList)
                {
                    m_processedFileList.Add(fileName);
                }
                m_fileName = fileName;
                OpenFile();
            }
        }

        private void OpenFile()
        {
            OnStatusMessage("Processing EPRI file \"{0}\"...", m_fileName);

            m_inStream = new StreamReader(m_fileName);

            // Skip specified number of header lines that exist before column heading definitions
            for (int i = 0; i < m_skipRows; i++)
            {
                m_inStream.ReadLine();
            }

            m_header = m_inStream.ReadLine();
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
        }

        private void CloseFile()
        {
            if (!UseHighResolutionInputTimer)
                m_looseTimer.Stop();

            if ((object)m_inStream != null)
            {
                m_inStream.Close();
                m_inStream.Dispose();
            }

            OnStatusMessage("Completed processing of EPRI file \"{0}\".", m_fileName);

            ThreadPool.QueueUserWorkItem(DeleteFile, m_fileName);

            m_inStream = null;
            m_processedFiles++;
        }

        private void DeleteFile(object state)
        {
            string fileName = state as string;

            try
            {
                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    File.Delete(fileName);

                lock (m_processedFileList)
                {
                    m_processedFileList.Remove(fileName);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to delete file \"{0}\": {1}", fileName, ex.Message), ex));
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

        private void m_looseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!ReadNextRecord(DateTime.UtcNow.Ticks) && Enabled)
            {
                CloseFile();
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

                if (fields.Length <= 0)
                    return false;

                // Read time from Timestamp column
                if (m_simulateTimestamp)
                {
                    fileTime = currentTime;
                }
                else
                {
                    timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", true) == 0).Key;
                    fileTime = DateTime.ParseExact(fields[timestampColumn], "dd-MMM-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture).Ticks;
                }

                for (int i = 0; i < m_measurementsPerInterval; i++)
                {
                    IMeasurement measurement;

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
                        measurement.ID = Guid.Empty;
                        measurement.Key = MeasurementKey.Undefined;
                        measurement.Value = double.NaN;
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

        #endregion
    }
}
