//******************************************************************************************************
//  FileExporter.cs - Gbtc
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
//  12/04/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PhasorProtocolAdapters;

namespace Comtrade
{
    /// <summary>
    /// Represents an action adapter that exports measurements on an interval to a COMTRADE formatted file that can be imported into other systems for analysis.
    /// </summary>
    [Description("Comtrade: exports measurements to a COMTRADE formatted file that can be imported into other systems for analysis")]
    public class FileExporter : CalculatedMeasurementBase
    {
        #region [ Members ]

        /// Nested Types
        private enum DataQuality
        {
            Good = 0,
            Suspect = 20,
            Bad = 32
        };

        /// Constants
        private const double SqrtOf3 = 1.7320508075688772935274463415059D;
        //COMTRADE version year 1999
        private const UInt16 Rev_year = 1999;

        /// Fields
        private MultipleDestinationExporter m_dataExporter;
        private Dictionary<MeasurementKey, string> m_measurementTags;
        private MeasurementKey m_referenceAngleKey;
        private bool m_useReferenceAngle;
        private bool m_useNumericQuality;
        private int m_exportInterval;
        private string m_companyTagPrefix;
        private bool m_statusDisplayed;
        private string m_stationName;
        private string m_deviceID;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the interval, in seconds, at which data will be queued for concentration and then exported.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval, in seconds, at which data will be queued for concentration and then exported.")]
        public double ExportInterval
        {
            get
            {
                return m_exportInterval / 1000.0D;
            }
            set
            {
                m_exportInterval = (int)(value * 1000.0D);
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether a reference angle is used to adjust the value of phase angles.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicates whether a reference angle is used to adjust the value of phase angles. IMPORTANT: If this is true, ReferenceAngleMeasurement is a required parameter.")]
        public bool UseReferenceAngle
        {
            get
            {
                return m_useReferenceAngle;
            }
            set
            {
                m_useReferenceAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the key of the measurement used to adjust the value of phase angles.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the key of the measurement used to adjust the value of phase angles."),
        DefaultValue("")]
        public string ReferenceAngleMeasurement
        {
            get
            {
                return m_referenceAngleKey.ToString();
            }
            set
            {
                m_referenceAngleKey = MeasurementKey.Parse(value);
            }
        }

        /// <summary>
        /// Gets or sets the prefix attached to the beginning of the measurements' point tags, excluding the underscore.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the prefix attached to the beginning of the measurements' point tags, excluding the underscore."),
        DefaultValue(null)]
        public string CompanyTagPrefix
        {
            get
            {
                return m_companyTagPrefix;
            }
            set
            {
                m_companyTagPrefix = value.ToUpper().Trim();
            }
        }

        /// <summary>
        /// Gets or sets the value that determines whether the measurement quality is represented as a numeric value rather than a symbolic value.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether the measurement quality is represented as a numeric value rather than a symbolic value. QUALITIES: Good = 0; Suspect = 20; Bad = 32"),
        DefaultValue(false)]
        public bool UseNumericQuality
        {
            get
            {
                return m_useNumericQuality;
            }
            set
            {
                m_useNumericQuality = value;
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

        /// <summary>
        /// Returns the detailed status of the <see cref="FileExporter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("     Using numeric quality: {0}", m_useNumericQuality);
                status.AppendLine();
                status.AppendFormat("     Using reference angle: {0}", m_useReferenceAngle);
                status.AppendLine();

                if (m_useReferenceAngle)
                {
                    status.AppendFormat("     Reference angle point: {0}", m_referenceAngleKey.ToString());
                    status.AppendLine();
                }

                if (!string.IsNullOrEmpty(m_companyTagPrefix))
                {
                    status.AppendFormat("        Company tag prefix: {0}", m_companyTagPrefix);
                    status.AppendLine();
                }

                if (m_dataExporter != null)
                    status.Append(m_dataExporter.Status);

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
                        if (m_dataExporter != null)
                        {
                            m_dataExporter.SaveSettings();
                            m_dataExporter.Dispose();
                        }

                        m_dataExporter = null;
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
        /// Intializes <see cref="FileExporter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string errorMessage = "{0} is missing from Settings - Example: exportInterval=5; useReferenceAngle=True; referenceAngleMeasurement=DEVARCHIVE:6; companyTagPrefix=TVA; useNumericQuality=True; inputMeasurementKeys={{FILTER ActiveMeasurements WHERE Device='SHELBY' AND SignalType='FREQ'}}";
            string setting;

            // Load required parameters
            if (!settings.TryGetValue("exportInterval", out setting))
                throw new ArgumentException(string.Format(errorMessage, "exportInterval"));

            m_exportInterval = int.Parse(setting);

            if (m_exportInterval == 0)
                throw new ArgumentException("exportInterval should not be 0 - Example: exportInterval=5");

            if (InputMeasurementKeys == null || InputMeasurementKeys.Length == 0)
                throw new InvalidOperationException("There are no input measurements defined. You must define \"inputMeasurementKeys\" to define which measurements to export.");

            if (!settings.TryGetValue("useReferenceAngle", out setting))
                throw new ArgumentException(string.Format(errorMessage, "useReferenceAngle"));

            m_useReferenceAngle = setting.ParseBoolean();

            if (m_useReferenceAngle)
            {
                // Reference angle measurement has to be defined if using reference angle
                if (!settings.TryGetValue("referenceAngleMeasurement", out setting))
                    throw new ArgumentException(string.Format(errorMessage, "referenceAngleMeasurement"));

                m_referenceAngleKey = MeasurementKey.Parse(setting);
            }

            // Make sure reference angle is part of input measurement keys collection
            if (!InputMeasurementKeys.Contains(m_referenceAngleKey))
                InputMeasurementKeys = InputMeasurementKeys.Concat(new MeasurementKey[] { m_referenceAngleKey }).ToArray();

            // Make sure sure reference angle key is actually an angle measurement
            SignalType signalType = InputMeasurementKeyTypes[InputMeasurementKeys.IndexOf(key => key == m_referenceAngleKey)];

            if (signalType != SignalType.IPHA && signalType != SignalType.VPHA)
                throw new InvalidOperationException(string.Format("Specified reference angle measurement key is a {0} signal, not a phase angle.", signalType.GetFormattedSignalTypeName()));

            // Load optional parameters
            if (settings.TryGetValue("companyTagPrefix", out setting))
                m_companyTagPrefix = setting.ToUpper().Trim();
            else
                m_companyTagPrefix = null;

            if (settings.TryGetValue("useNumericQuality", out setting))
                m_useNumericQuality = setting.ParseBoolean();
            else
                m_useNumericQuality = false;

            // COMTRADE SPECIFIC CODE:
            //Get station name
            if (settings.TryGetValue("stationName", out setting))
                m_stationName = setting;
            else
                m_stationName = Name;
            //Get device id
            if (settings.TryGetValue("deviceID", out setting))
                m_deviceID = setting;


            // Suffix company tag prefix with an underscore if defined
            if (!string.IsNullOrEmpty(m_companyTagPrefix))
                m_companyTagPrefix = m_companyTagPrefix.EnsureEnd('_');

            // Define a default export location - user can override and add multiple locations in config later...
            m_dataExporter = new MultipleDestinationExporter(ConfigurationSection, m_exportInterval * 1000);
            m_dataExporter.Initialize(new ExportDestination[] { new ExportDestination(FilePath.GetAbsolutePath(ConfigurationSection + ".txt"), false, "", "", "") });


            // Create new measurement tag name dictionary
            m_measurementTags = new Dictionary<MeasurementKey, string>();
            string pointID = "undefined";

            // Lookup point tag name for input measurement in the ActiveMeasurements table
            foreach (MeasurementKey key in InputMeasurementKeys)
            {
                try
                {
                    // Get measurement key as a string
                    pointID = key.ToString();

                    // Lookup measurement key in active measurements table
                    DataRow row = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID='{0}'", pointID))[0];

                    // Remove invalid symbols that may be in tag name
                    string pointTag = row["PointTag"].ToNonNullString(pointID).Replace('-', '_').Replace(':', '_').ToUpper();

                    // Prefix point tag with company prefix if defined
                    if (!string.IsNullOrEmpty(m_companyTagPrefix) && !pointTag.StartsWith(m_companyTagPrefix))
                        pointTag = m_companyTagPrefix + pointTag;

                    m_measurementTags.Add(key, pointTag);
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Failed to lookup point tag for measurement [{0}] due to exception: {1}", pointID, ex.Message)));
                }
            }

            // We enable tracking of latest measurements so we can use these values if points are missing
            TrackLatestMeasurements = true;
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        public override void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            QueueMeasurementsForProcessing(new IMeasurement[] { measurement });
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// We override this method to only queue measurements at the desired export interval - no need
        /// do excess sorting work for measurements that will never be used :)
        /// </remarks>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            List<IMeasurement> inputMeasurements = new List<IMeasurement>();
            Ticks timestamp;
            bool sortMeasurement;

            foreach (IMeasurement measurement in measurements)
            {
                timestamp = measurement.Timestamp;

                // Measurement will exported if the following criteria are true:
                //   A) Timestamp's seconds are an interval of the defined export interval
                //   B) Timestamp falls within first frame of data in the second
                //   C) This is a defined input measurement for this adapter
                sortMeasurement =
                        ((DateTime)timestamp).Second % m_exportInterval == 0 && // <-- A
                        timestamp.DistanceBeyondSecond() < TicksPerFrame &&     // <-- B
                        IsInputMeasurement(measurement.Key);                    // <-- C

                if (sortMeasurement)
                    inputMeasurements.Add(measurement);
            }

            if (inputMeasurements.Count > 0)
                SortMeasurements(inputMeasurements);
        }

        // Lookup signal type for given measurement key
        private string LookupSignalTypeValue(MeasurementKey key)
        {
            try
            {
                DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", key.ToString()));

                if (filteredRows.Length > 0)
                    return filteredRows[0]["SignalType"].ToString();
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to lookup signal type value for measurement {0}: {1}", key.ToString(), ex.Message), ex));
            }

            return string.Empty;
        }

        /// <summary>
        /// Process frame of time-aligned measurements that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements that arrived within lag time and are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within one second of data ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            Ticks timestamp = frame.Timestamp;
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            if (measurements.Count > 0)
            {
                StringBuilder fileData = new StringBuilder();
                IMeasurement measurement, referenceAngle;
                MeasurementKey inputMeasurementKey;
                SignalType signalType;
                DataQuality measurementQuality;
                double measurementValue; //, referenceAngleValue;
                string measurementTag;
                int totalChannels, totalAnalogChannels, totalDigitalChannels;
                int analogChannelID; //, digitalChannelID;

                // We need to get calculated reference angle value in order to export relative phase angles
                // If the value is not here, we don't export
                referenceAngle = null;
                analogChannelID = 0;
                //digitalChannelID = 0;

                // Make sure reference made it in this frame...
                if (m_useReferenceAngle && !measurements.TryGetValue(m_referenceAngleKey, out referenceAngle))
                {
                    OnProcessException(new InvalidOperationException("Calculated reference angle was not found in this frame, possible reasons: system is initializing, receiving no data or lag time is too small. File creation was skipped."));
                }
                else
                {
                    //Get station name, device id and version standard for COMTRADE FILE
                    //--Code here
                    fileData.Append(m_stationName);
                    fileData.Append(",");
                    fileData.Append(m_deviceID);
                    fileData.Append(",");
                    fileData.Append(Rev_year);
                    // Terminate line 
                    fileData.AppendLine(" ");

                    //Get analog channels count
                    totalAnalogChannels = InputMeasurementKeyTypes.Count(s => s == SignalType.ALOG);
                    //Get digital channels count
                    totalDigitalChannels = InputMeasurementKeyTypes.Count(s => s == SignalType.DIGI);
                    //Count total analog and digital channels
                    totalChannels = totalAnalogChannels + totalDigitalChannels;
                    //
                    fileData.Append(totalChannels + "," + totalAnalogChannels + "A," + totalDigitalChannels + "D");
                    fileData.AppendLine(" ");

                    // Export all defined input measurements for analog channel
                    for (int i = 0; i < InputMeasurementKeys.Length; i++)
                    {
                        inputMeasurementKey = InputMeasurementKeys[i];
                        signalType = InputMeasurementKeyTypes[i];

                        // Look up measurement's tag name
                        if (m_measurementTags.TryGetValue(inputMeasurementKey, out measurementTag))
                        {
                            // See if measurement exists in this frame
                            if (measurements.TryGetValue(inputMeasurementKey, out measurement))
                            {
                                // Get measurement's adjusted value (takes into account any adder and or multipler)
                                measurementValue = measurement.AdjustedValue;

                                // Interpret data quality flags
                                measurementQuality = (measurement.ValueQualityIsGood() ? (measurement.TimestampQualityIsGood() ? DataQuality.Good : DataQuality.Suspect) : DataQuality.Bad);
                            }
                            else
                            {
                                // Didn't find measurement in this frame, try using most recent value
                                measurementValue = LatestMeasurements[inputMeasurementKey.SignalID];

                                // Interpret data quality flags - since measurement was missing in this frame we mark it as
                                // suspect. Could have just missed the time window for sorting.
                                measurementQuality = (Double.IsNaN(measurementValue) ? DataQuality.Bad : DataQuality.Suspect);

                                // We'll export zero instead of NaN for bad data
                                if (measurementQuality == DataQuality.Bad)
                                    measurementValue = 0.0D;
                            }

                            //// Export tag name field
                            //fileData.Append(measurementTag);
                            //fileData.Append(",");

                            //Get mesurement information by input mesurement key
                            DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", inputMeasurementKey));

                            // Export measurement value making any needed adjustments based on signal type
                            if (signalType == SignalType.ALOG || signalType == SignalType.VPHA ||
                                    signalType == SignalType.IPHM || signalType == SignalType.VPHA || signalType == SignalType.IPHA)
                            {
                                //Increment analog channel counter by 1
                                analogChannelID += 1;
                                //Set analog channel counter in file data
                                fileData.Append(analogChannelID);
                                fileData.Append(",");

                                if (filteredRows.Length > 0)
                                {
                                    //Get COMTRADE Channel id (STATION:CHANNELNAME)
                                    fileData.Append(m_stationName + ":" + filteredRows[0]["DeviceID"].ToString());
                                    fileData.Append(",");
                                    //Get phase value from filter row
                                    fileData.Append(filteredRows[0]["Phase"].ToString());
                                    //Magnitude for polar components
                                    if (signalType == SignalType.VPHA || signalType == SignalType.IPHA)
                                        fileData.Append("a");
                                    else
                                        fileData.Append("m");
                                    //Get ccbm- circuit component - Currently keep as blank until get more information
                                    fileData.Append(",");
                                    //Get uu - Alphanumeric channel units -- Currently keep as blank until get more information
                                    fileData.Append(",");
                                    //Get channel multiplier
                                    fileData.Append(measurement.Multiplier);
                                    fileData.Append(",");
                                    //Get channel offset
                                    fileData.Append(measurement.Adder);
                                    fileData.Append(",");
                                    //Get time skew between channels
                                    fileData.Append(",");
                                    //Get data range minimun value
                                    fileData.Append(",");
                                    //Get data range maximum value
                                    fileData.Append(",");
                                    //Get PT/CT primary ratio factor
                                    fileData.Append(",");
                                    //Get PT/CT secondary ratio factor
                                    fileData.Append(",");
                                    //End of Line
                                    fileData.AppendLine(" ");
                                }

                            }
                            //else if (signalType == SignalType.VPHA || signalType == SignalType.IPHA)
                            //{
                            //    // Export tag name field
                            //    fileData.Append(measurementTag);
                            //    fileData.Append(",");
                            //    // This is a phase angle measurement, export the value relative to the reference angle (if available)
                            //    if (referenceAngle == null)
                            //    {
                            //        // No reference angle defined, export raw angle
                            //        fileData.Append(measurementValue);
                            //    }
                            //    else
                            //    {

                            //        // Get reference angle's adjusted value (takes into account any adder and or multipler)
                            //        referenceAngleValue = referenceAngle.AdjustedValue;

                            //        // Handle relative angle wrapping
                            //        double dis0 = Math.Abs(measurementValue - referenceAngleValue);
                            //        double dis1 = Math.Abs(measurementValue - referenceAngleValue + 360);
                            //        double dis2 = Math.Abs(measurementValue - referenceAngleValue - 360);

                            //        if ((dis0 < dis1) && (dis0 < dis2))
                            //            measurementValue = measurementValue - referenceAngleValue;
                            //        else if (dis1 < dis2)
                            //            measurementValue = measurementValue - referenceAngleValue + 360;
                            //        else
                            //            measurementValue = measurementValue - referenceAngleValue - 360;

                            //        fileData.Append(measurementValue);
                            //    }
                            //}
                            //else if (signalType == SignalType.VPHM)
                            //{
                            //    // Typical voltages from PMU's are line-to-neutral volts so we convert them to line-to-line kilovolts
                            //    fileData.Append(measurementValue * SqrtOf3 / 1000.0D);
                            //}
                            //else
                            //{
                            //    // Export all other types of measurements as their raw value
                            //    fileData.Append(measurementValue);
                            //}

                            //// Export interpreted measurement quality
                            //fileData.Append(",");

                            //if (m_useNumericQuality)
                            //    fileData.Append((int)measurementQuality);
                            //else
                            //    fileData.Append(measurementQuality);

                            // Terminate line (ICCP file link expects these two terminating commas, weird...)
                            //fileData.AppendLine(",,");
                            //fileData.AppendLine(" ");
                        }
                        else
                        {
                            // We were unable to find measurement tag for this key - this is unexpected
                            OnProcessException(new InvalidOperationException(string.Format("Failed to find measurement tag for measurement {0}", inputMeasurementKey)));
                        }
                    }
                }

                // Measurement export to a file may take more than available processing time - so we queue this work up ...
                m_dataExporter.ExportData(fileData.ToString());

                // We display export status every other minute
                if (new DateTime(frame.Timestamp).Minute % 2 == 0)
                {
                    //Make sure message is only displayed once during the minute
                    if (!m_statusDisplayed)
                    {
                        OnStatusMessage(string.Format("{0} successful file based measurement exports...", m_dataExporter.TotalExports));
                        m_statusDisplayed = true;
                    }
                }
                else
                    m_statusDisplayed = false;
            }
            else
            {
                // No data was available in the frame, lag time set too tight?
                OnProcessException(new InvalidOperationException("No measurements were available for COMTRADE file based data export, possible reasons: system is initializing , receiving no data or lag time is too small. COMTRADE File creation was skipped."));
            }
        }

        #endregion
    }
}
