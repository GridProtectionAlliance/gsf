//******************************************************************************************************
//  FileExporter.cs - Gbtc
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
//  12/04/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PhasorProtocolAdapters;

namespace COMTRADEAdapters
{
    /// <summary>
    /// Represents an action adapter that exports measurements on an interval to a COMTRADE formatted file that can be imported into other systems for analysis.
    /// </summary>
    [Description("COMTRADE: Exports measurements to a COMTRADE formatted file that can be imported into other systems for analysis")]
    public class FileExporter : CalculatedMeasurementBase
    {
        #region [ Members ]

        /// Fields
        private string m_stationName;
        private string m_deviceID;
        private bool m_statusDisplayed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets station name to use in COMTRADE configuration file.
        /// </summary>
        [ConnectionStringParameter, Description("Defines station name to use in COMTRADE configuration file.")]
        public string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                m_stationName = value;
            }
        }

        /// <summary>
        /// Gets or sets device ID to use in COMTRADE configuration file.
        /// </summary>
        [ConnectionStringParameter, Description("Defines device ID to use in COMTRADE configuration file.")]
        public string DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Returns the detailed status of the <see cref="FileExporter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("              Station name: {0}\r\n", m_stationName);
                status.AppendFormat("                 Device ID: {0}\r\n", m_deviceID);
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        private bool m_disposed;

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileExporter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
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
        /// Initializes <see cref="FileExporter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load required parameters
            if (InputMeasurementKeys == null || InputMeasurementKeys.Length == 0)
                throw new InvalidOperationException("There are no input measurements defined. You must define \"inputMeasurementKeys\" to define which measurements to export.");

            // Get station name
            if (settings.TryGetValue("stationName", out setting))
                m_stationName = setting;
            else
                m_stationName = Name;

            // Get device ID
            if (settings.TryGetValue("deviceID", out setting))
                m_deviceID = setting;

            // We enable tracking of latest measurements so we can use these values if points are missing
            TrackLatestMeasurements = true;
        }

        /// <summary>
        /// Process frame of time-aligned measurements that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements that arrived within lag time and are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within one second of data ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            if (measurements.Count > 0)
            {

                // We display export status every other minute
                if (new DateTime(frame.Timestamp).Minute % 2 == 0)
                {
                    //Make sure message is only displayed once during the minute
                    if (!m_statusDisplayed)
                    {
                        //OnStatusMessage(string.Format("{0} successful file based measurement exports...", m_dataExporter.TotalExports));
                        m_statusDisplayed = true;
                    }
                }
                else
                    m_statusDisplayed = false;
            }
            else
            {
                // No data was available in the frame, lag time set too tight?
                OnProcessException(MessageLevel.Warning, new InvalidOperationException("No measurements were available for COMTRADE file based data export, possible reasons: system is initializing , receiving no data or lag time is too small. COMTRADE File creation was skipped."));
            }
        }

        #endregion
    }
}
