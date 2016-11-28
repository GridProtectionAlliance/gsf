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
//  05/03/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace EpriExport
{
    /// <summary>
    /// Represents an action adapter that exports measurements on an interval to a file that can be picked up by EPRI applications.
    /// </summary>
    [Description("EPRI File Exporter: Exports measurements to a file that can be used by EPRI systems")]
    public class FileExporter : CalculatedMeasurementBase
    {
        #region [ Members ]

        // Constants
        private const string RowCountMarker = "<RCM>";
        //private readonly double[] BaseKVs = { -1.0D, -1.0D, 69.0D * SI.Kilo, 115.0D * SI.Kilo, 138.0D * SI.Kilo, 161.0D * SI.Kilo, 230.0D * SI.Kilo, 345.0D * SI.Kilo, 500.0D * SI.Kilo, 765.0D * SI.Kilo };
        //private const double SqrtOf3 = 1.7320508075688772935274463415059D;

        // Fields
        private string m_fileExportPath;
        private string m_header;
        private MeasurementKey m_referenceAngleKey;
        private int m_exportInterval;
        private string m_comments;
        private string m_modelIdentifier;
        private bool m_statusDisplayed;
        private long m_skippedExports;
        private readonly LongSynchronizedOperation m_fileExport;
        private readonly object m_fileDataLock;
        private StringBuilder m_fileData;
        private Ticks m_startTime;
        private long m_rowCount;
        private long m_totalExports;
        //private Dictionary<MeasurementKey, double> m_baseVoltages;

        #endregion

        /// <summary>
        /// Creates a new instance of the EPRI <see cref="FileExporter"/>.
        /// </summary>
        public FileExporter()
        {
            m_fileExport = new LongSynchronizedOperation(WriteFileData, ex => OnProcessException(MessageLevel.Error, "EpriFileExporter", ex))
            {
                IsBackground = true
            };
            m_fileDataLock = new object();
        }

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the file export path for the EPRI export.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the file export path for the EPRI export."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.FolderBrowserEditor")]
        public string FileExportPath
        {
            get
            {
                return m_fileExportPath;
            }
            set
            {
                m_fileExportPath = value;
            }
        }

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
        /// Gets or sets the comments to be exported to the output file.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the comments to exported in the output file.")]
        public string Comments
        {
            get
            {
                return m_comments;
            }
            set
            {
                m_comments = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
            }
        }

        /// <summary>
        /// Gets or sets the model identifier to be exported to the output file.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the model identifier to exported in the output file.")]
        public string ModelIdentifier
        {
            get
            {
                return m_modelIdentifier;
            }
            set
            {
                m_modelIdentifier = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
            }
        }

        /// <summary>
        /// Gets or sets the key of the measurement used to adjust the value of phase angles.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the key of the measurement used to adjust the value of phase angles."),
        DefaultValue(""),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "filterExpression={SignalAcronym = 'IPHA' OR SignalAcronym = 'VPHA'}; selectable=false")]
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
        /// Returns the detailed status of the <see cref="FileExporter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("          File export path: {0}", m_fileExportPath);
                status.AppendLine();
                status.AppendFormat("           Export comments: {0}", m_comments);
                status.AppendLine();
                status.AppendFormat("          Model identifier: {0}", m_modelIdentifier);
                status.AppendLine();
                status.AppendFormat("     Reference angle point: {0}", m_referenceAngleKey);
                status.AppendLine();
                status.AppendFormat("             Total exports: {0}", m_totalExports);
                status.AppendLine();
                status.AppendFormat("           Skipped exports: {0}", m_skippedExports);
                status.AppendLine();

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="FileExporter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            const string errorMessage = "{0} is missing from Settings - Example: exportInterval=5; modelIdentifier=Goslin; referenceAngleMeasurement=DEVARCHIVE:6; inputMeasurementKeys={{FILTER ActiveMeasurements WHERE Device='SHELBY' AND SignalType='FREQ'}}";
            string setting;
            double seconds;

            // Load required parameters
            if (!settings.TryGetValue("exportInterval", out setting) || !double.TryParse(setting, out seconds))
                throw new ArgumentException(string.Format(errorMessage, "exportInterval"));

            if (!settings.TryGetValue("fileExportPath", out m_fileExportPath))
                m_fileExportPath = FilePath.GetAbsolutePath("");

            m_exportInterval = (int)(seconds * 1000.0D);

            if (m_exportInterval <= 0)
                throw new ArgumentException("exportInterval should not be 0 - Example: exportInterval=5.5");

            if ((object)InputMeasurementKeys == null || InputMeasurementKeys.Length == 0)
                throw new InvalidOperationException("There are no input measurements defined. You must define \"inputMeasurementKeys\" to define which measurements to export.");

            // Reference angle measurement has to be defined if using reference angle
            if (!settings.TryGetValue("referenceAngleMeasurement", out setting))
                throw new ArgumentException(string.Format(errorMessage, "referenceAngleMeasurement"));

            m_referenceAngleKey = MeasurementKey.Parse(setting);

            // Make sure reference angle is first angle of input measurement keys collection
            InputMeasurementKeys = (new[] { m_referenceAngleKey }).Concat(InputMeasurementKeys).ToArray();

            // Make sure sure reference angle key is actually an angle measurement
            SignalType signalType = InputMeasurementKeyTypes[InputMeasurementKeys.IndexOf(key => key == m_referenceAngleKey)];

            if (signalType != SignalType.IPHA && signalType != SignalType.VPHA)
                throw new InvalidOperationException($"Specified reference angle measurement key is a {signalType.GetFormattedName()} signal, not a phase angle.");

            Comments = settings.TryGetValue("comments", out setting) ? setting : "Comment section---";

            if (!settings.TryGetValue("modelIdentifier", out setting))
                throw new ArgumentException(string.Format(errorMessage, "modelIdentifier"));

            ModelIdentifier = setting;

            // We enable tracking of latest measurements so we can use these values if points are missing - since we are using
            // latest measurement tracking, we sort all incoming points even though most of them will be thrown out...
            TrackLatestMeasurements = true;

            //// Create a new dictionary of base voltages
            //m_baseVoltages = new Dictionary<MeasurementKey, double>();

            StringBuilder header = new StringBuilder();
            //MeasurementKey voltageMagnitudeKey;
            //double baseKV;

            // Write header row
            header.Append("TimeStamp");

            DataTable measurements = DataSource.Tables["ActiveMeasurements"];
            int tieLines = 0;
            bool referenceAdded = false;

            for (int i = 0; i < InputMeasurementKeys.Length; i++)
            {
                // Lookup measurement key in active measurements table
                DataRow row = measurements.Select($"ID='{InputMeasurementKeys[i]}'")[0];
                string deviceName = row["Device"].ToNonNullString("UNDEFINED").ToUpper().Trim();

                if (!referenceAdded && InputMeasurementKeys[i] == m_referenceAngleKey)
                {
                    header.AppendFormat(",Ref. Angle of {0}", deviceName);
                    referenceAdded = true;
                }
                else
                    switch (InputMeasurementKeyTypes[i])
                    {
                        case SignalType.VPHM:
                            header.AppendFormat(",{0} |V|", deviceName);
                            tieLines++;

                            //voltageMagnitudeKey = InputMeasurementKeys[i];

                            //if (settings.TryGetValue(voltageMagnitudeKey + "BaseKV", out setting) && double.TryParse(setting, out baseKV))
                            //{
                            //    m_baseVoltages.Add(voltageMagnitudeKey, baseKV * SI.Kilo);
                            //}
                            //else
                            //{
                            //    int baseKVCode;

                            //    // Second check if base KV can be inferred from device name suffixed KV index
                            //    if (int.TryParse(deviceName[deviceName.Length - 1].ToString(), out baseKVCode) && baseKVCode > 1 && baseKVCode < BaseKVs.Length)
                            //    {
                            //        m_baseVoltages.Add(voltageMagnitudeKey, BaseKVs[baseKVCode]);
                            //    }
                            //    else
                            //    {
                            //        OnStatusMessage("WARNING: Did not find a valid base KV setting for voltage magnitude {0}, assumed 500KV", voltageMagnitudeKey.ToString());
                            //        m_baseVoltages.Add(voltageMagnitudeKey, 500.0D * SI.Kilo);
                            //    }
                            //}
                            break;
                        case SignalType.VPHA:
                            header.AppendFormat(",{0} Voltage Angle", deviceName);
                            break;
                        case SignalType.IPHM:
                            header.AppendFormat(",{0} |I|", deviceName);
                            break;
                        case SignalType.IPHA:
                            header.AppendFormat(",{0} Current Angle", deviceName);
                            break;
                        default:
                            header.AppendFormat(",{0} ??", deviceName);
                            break;
                    }
            }

            string row5 = header.ToString();
            header = new StringBuilder();

            // Add row 1
            header.AppendFormat("Comments: {0}\r\n", Comments);

            // Add row 2
            header.AppendFormat("Model Identifier: {0}\r\n", ModelIdentifier);

            // Add row 3
            header.Append("Datapoints,Tielines,TimeStep");

            if (InputMeasurementKeys.Length - 3 > 0)
                header.Append(new string(',', InputMeasurementKeys.Length - 3));

            header.AppendLine();

            // Add row 4
            header.AppendFormat("{0},{1},{2}", RowCountMarker, tieLines, 1.0D / FramesPerSecond);
            header.AppendLine();

            // Add row 5
            header.AppendLine(row5);

            // Cache header for each file export
            m_header = header.ToString();
        }

        /// <summary>
        /// Process frame of time-aligned measurements that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements that arrived within lag time and are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within one second of data ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            Ticks timestamp = frame.Timestamp;
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            if (measurements.Count > 0)
            {
                lock (m_fileDataLock)
                {
                    IMeasurement measurement;
                    MeasurementKey inputMeasurementKey;
                    SignalType signalType;
                    double measurementValue;

                    if ((object)m_fileData == null)
                    {
                        m_fileData = new StringBuilder();
                        m_startTime = timestamp;
                        m_rowCount = 0;
                    }

                    m_fileData.AppendFormat("{0}", timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                    // Export all defined input measurements
                    for (int i = 0; i < InputMeasurementKeys.Length; i++)
                    {
                        m_fileData.Append(',');
                        inputMeasurementKey = InputMeasurementKeys[i];
                        signalType = InputMeasurementKeyTypes[i];

                        // Get measurement for this frame, falling back on latest value
                        measurementValue = measurements.TryGetValue(inputMeasurementKey, out measurement) ? measurement.AdjustedValue : LatestMeasurements[inputMeasurementKey];

                        // Export measurement value making any needed adjustments based on signal type
                        if (signalType == SignalType.VPHM)
                        {
                            // Convert voltages to base units
                            m_fileData.Append(measurementValue / SI.Kilo);
                        }
                        else
                        {
                            // Export all other types of measurements as their raw value
                            m_fileData.Append(measurementValue);
                        }
                    }

                    // Terminate line
                    m_fileData.AppendLine();
                    m_rowCount++;
                }
            }

            // Only publish when the export interval time has passed
            if ((timestamp - m_startTime).ToMilliseconds() > m_exportInterval)
                m_fileExport.TryRunOnceAsync();
        }

        private void WriteFileData()
        {
            string fileData = null;
            long rowCount = 0;
            Ticks startTime = 0;
            bool displayedWarning = false;

            // Get current file data and reset buffer
            lock (m_fileDataLock)
            {
                if ((object)m_fileData != null)
                {
                    fileData = m_fileData.ToString();
                    rowCount = m_rowCount;
                    startTime = m_startTime;
                    m_fileData = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(fileData))
            {
                try
                {
                    string fileName = Path.Combine(FilePath.GetAbsolutePath(m_fileExportPath), "EPRI-VS-Input-" + startTime.ToString("yyyyMMddHHmmss") + ".csv");

                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        // Update actual row count
                        writer.Write(m_header.Replace(RowCountMarker, rowCount.ToString()));
                        writer.Write(fileData);
                    }

                    m_totalExports++;
                }
                catch (Exception ex)
                {
                    m_skippedExports++;
                    OnStatusMessage(MessageLevel.Warning, "EpriFileExporter", "WARNING: Skipped export due to exception: " + ex.Message);
                    displayedWarning = true;
                }

                // We display export status every other minute
                if (DateTime.UtcNow.Minute % 2 == 0 && !displayedWarning)
                {
                    // Make sure message is only displayed once during the minute
                    if (!m_statusDisplayed)
                    {
                        OnStatusMessage(MessageLevel.Info, "EpriFileExporter", $"{m_totalExports:N0} successful file based measurement exports...");
                        m_statusDisplayed = true;
                    }
                }
                else
                {
                    m_statusDisplayed = false;
                }
            }
        }

        #endregion
    }
}
