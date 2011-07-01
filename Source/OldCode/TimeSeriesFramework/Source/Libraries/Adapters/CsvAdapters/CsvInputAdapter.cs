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
using System.IO;
using System.Text;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a CSV file.
    /// </summary>
    [Description("CSV: archives measurements to a CSV file.")]
    public class CsvInputAdapter : InputAdapterBase
    {

        #region [ Members ]

        // Fields
        private string m_fileName;
        private StreamReader m_inStream;
        private string m_header;
        private Dictionary<string, int> m_columns;
        private double m_inputInterval;
        private int m_measurementsPerInterval;
        private bool m_simulateTimestamp;
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

                return status.ToString();
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
            string[] headings = m_header.Split(',');

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
            IMeasurement[] newMeasurements = new IMeasurement[m_measurementsPerInterval];
            Ticks currentTime = DateTime.Now;

            for (int i = 0; i < m_measurementsPerInterval; i++)
            {
                IMeasurement measurement = new Measurement();
                string line = m_inStream.ReadLine();
                string[] fields = line.Split(',');

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

                newMeasurements[i] = measurement;
            }

            OnNewMeasurements(newMeasurements);
        }

        #endregion
    }
}
