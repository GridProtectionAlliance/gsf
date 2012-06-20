//******************************************************************************************************
//  CsvInputAdapter.cs - Gbtc
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
//  04/06/2010 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a CSV file.
    /// </summary>
    [Description("CSV: reads measurements from a CSV file.")]
    public class CsvInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_fileName;
        private StreamReader m_inStream;
        private string m_header;
        private Dictionary<string, int> m_columns;
        private Dictionary<int, IMeasurement> m_columnMappings;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private bool m_simulateTimestamp;
        private bool m_transverse;
        private System.Timers.Timer m_timer;

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
            m_timer = new System.Timers.Timer();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the CSV file from which measurements will be read.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the CSV file from which measurements will be read."),
        DefaultValue("measurements.csv")]
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
        /// Gets or sets the number of measurements that are read from the CSV file in each frame.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of measurements measurements that are read from the CSV file in each frame."),
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
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

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

                    if (!m_simulateTimestamp && !columnMappings.Values.Contains("Timestamp", StringComparer.InvariantCultureIgnoreCase))
                        throw new InvalidOperationException("One of the column mappings must be defined as a \"Timestamp\": e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");

                    // In transverse mode, maximum measurements per interval is set to maximum columns in input file
                    m_measurementsPerInterval = columnMappings.Keys.Max();

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
                else
                {
                    throw new InvalidOperationException("Column mappings must be defined when using transverse format: e.g., columnMappings={0=Timestamp; 1=PPA:12; 2=PPA13}.");
                }
            }

            // Override input interval based on temporal processing interval if it's not set to default
            if (ProcessingInterval > -1)
            {
                if (ProcessingInterval == 0)
                    m_inputInterval = 1;
                else
                    m_inputInterval = ProcessingInterval;
            }

            m_timer.Interval = m_inputInterval;
            m_timer.AutoReset = true;
            m_timer.Elapsed += m_timer_Elapsed;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_inStream = new StreamReader(m_fileName);

            m_header = m_inStream.ReadLine();

            string[] headings = m_header.ToNonNullString().Split(',');

            for (int i = 0; i < headings.Length; i++)
            {
                m_columns.Add(headings[i], i);
            }

            m_timer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="CsvInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_timer.Stop();
            m_inStream.Close();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from CSV file.", ProcessedMeasurements).CenterText(maxLength);
        }

        private void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<IMeasurement> newMeasurements = new List<IMeasurement>();
            long fileTime = 0;
            long currentTime = DateTime.Now.Ticks;
            int timestampColumn = 0;
            string measurementID;
            string[] fields = m_inStream.ReadLine().ToNonNullString().Split(',');

            if (fields.Length <= 0)
                return;

            // Read time from Timestamp column in transverse mode
            if (m_transverse)
            {
                if (m_simulateTimestamp)
                {
                    fileTime = currentTime;
                }
                else
                {
                    timestampColumn = m_columnMappings.First(kvp => string.Compare(kvp.Value.TagName, "Timestamp", true) == 0).Key;
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
                        measurement.ID = Guid.Empty;
                        measurement.Key = MeasurementKey.Undefined;
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
                        measurement.ID = new Guid(fields[m_columns["Signal ID"]]);

                    if (m_columns.ContainsKey("Measurement Key"))
                        measurement.Key = MeasurementKey.Parse(fields[m_columns["Measurement Key"]], measurement.ID);

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

        #endregion
    }
}
