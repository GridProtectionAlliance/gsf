#region [ Modification History ]
/*
 * 07/22/2012 Denis Kholine
 *  Generated Original version of source code.
 *
 * 08/29/2012 Denis Kholine
 *  Comment out async connect case
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Using ]
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using GSF.TestsSuite.TimeSeries.Wrappers;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a CSV file.
    /// </summary>
    [Description("CSV: archives measurements to a CSV file.")]
    public class IInputAdapterCase : InputAdapterBaseWrapper
    {
        #region [ Members ]
        private Dictionary<string, int> m_columns;
        private string m_fileName;
        private string m_header;
        private double m_inputInterval;
        private StreamReader m_inStream;
        private int m_measurementsPerInterval;
        private bool m_simulateTimestamp;
        private System.Timers.Timer m_timer;

        #endregion

        #region [ Constructors ]
        /// <summary>
        /// Initializes a new instance of the <see cref="CsvInputAdapter"/> class.
        /// </summary>
        public IInputAdapterCase()
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

        /*
        /// <summary>
        /// Gets a flag that determines if this <see cref="CsvInputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        public virtual bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }
         */

        #endregion

        #region [ Methods ]
        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("{0} measurements read from CSV file.", ProcessedMeasurements).CenterText(maxLength);
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