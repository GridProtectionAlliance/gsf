//******************************************************************************************************
//  CsvOutputAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace CsvAdapters
{
    /// <summary>
    /// Represents an output adapter that writes measurements to a CSV file.
    /// </summary>
    [Description("CSV: Archives measurements to a CSV file")]
    public class CsvOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_fileName;
        private StreamWriter m_outStream;
        private int m_measurementCount;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvOutputAdapter"/> class.
        /// </summary>
        public CsvOutputAdapter()
        {
            m_fileName = "measurements.csv";
            m_measurementCount = 0;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the CSV file.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the CSV file to which measurements will be archived."),
        DefaultValue("measurements.csv"),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FileDialogEditor", "type=save; defaultExt=.csv; filter=CSV files|*.csv|All files|*.*")]
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
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="CsvOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="CsvOutputAdapter"/>
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
                status.AppendFormat("                 File name: {0}", m_fileName);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("fileName", out setting))
                m_fileName = setting;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_outStream = new StreamWriter(m_fileName);
            m_outStream.WriteLine(s_fileHeader);
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_outStream.Close();
        }

        /// <summary>
        /// Archives <paramref name="measurements"/> locally.
        /// </summary>
        /// <param name="measurements">Measurements to be archived.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements != null)
            {
                StringBuilder builder = new StringBuilder();

                foreach (IMeasurement measurement in measurements)
                {
                    builder.Append(measurement.ID);
                    builder.Append(',');
                    builder.Append(measurement.Key);
                    builder.Append(',');
                    builder.Append((long)measurement.Timestamp);
                    builder.Append(',');
                    builder.Append(measurement.AdjustedValue);
                    builder.Append(Environment.NewLine);
                }

                m_outStream.Write(builder.ToString());
                m_measurementCount += measurements.Length;
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="CsvOutputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to CSV file.", m_measurementCount).CenterText(maxLength);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string s_fileHeader = "Signal ID,Measurement Key,Timestamp,Value";

        #endregion
    }
}
