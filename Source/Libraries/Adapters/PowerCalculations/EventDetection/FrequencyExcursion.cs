//******************************************************************************************************
//  FrequencyExcursion.cs - Gbtc
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
//  09/29/2009 - Jian (Ryan) Zuo
//       Generated original version of source code.
//  10/19/2009 - J. Ritchie Carroll
//       Migrated code to action adapter type.
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for excursion detection.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations.EventDetection
{
    /// <summary>
    /// Defines the type of frequency excursion detected.
    /// </summary>
    public enum ExcursionType
    {
        /// <summary>
        /// Generation based frequency excursion.
        /// </summary>
        GenerationTrip,
        /// <summary>
        /// Load based frequency excursion.
        /// </summary>
        LoadTrip
    }

    /// <summary>
    /// Represents an algorithm that detects frequency excursions.
    /// </summary>
    [Description("Frequency Excursion: Detects frequency excursions in synchrophasor data")]
    public class FrequencyExcursion : CalculatedMeasurementBase
    {
        #region [ Members ]

        // Fields
        private double m_estimateTriggerThreshold;  // Threshold for detecting abnormal excursion in frequency
        private int m_analysisWindowSize;           // Analysis Window Size
        private int m_analysisInterval;             // Analysis Interval
        private int m_consecutiveDetections;        // Consecutive detections used to determine if the alarm is true or false
        private double m_powerEstimateRatio;        // Ratio used to calculate the total estimated MW change from frequency 
        private int m_alarmProhibitCounter;         // Counter to prevent duplicated alarms
        private int m_alarmProhibitPeriod;          // Period to prevent duplicated alarms
        private List<double> m_frequencies;         // Frequency measurement values
        private List<DateTime> m_timeStamps;        // Timestamps of frequencies
        private int m_minimumValidChannels;         // Minimum frequency values needed to perform a valid calculation
        private int m_detectedExcursions;           // Number of detected excursions
        private long m_count;                       // Published frame count

        // Important: Make sure output definition defines points in the following order
        private enum Output
        {
            WarningSignal,
            FrequencyDelta,
            TypeOfExcursion,
            EstimatedSize
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the threshold for detecting an abnormal excursion in frequency.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the threshold for detecting an abnormal excursion in frequency."),
        DefaultValue(0.0256D)]
        public double EstimateTriggerThreshold
        {
            get
            {
                return m_estimateTriggerThreshold;
            }
            set
            {
                m_estimateTriggerThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of frames to be analyzed at any given time.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the number of frames to be analyzed at any given time. The default value is 4 times the frame-rate defined in the connection string for this Frequency Excursion."),
        DefaultValue("")]
        public int AnalysisWindowSize
        {
            get
            {
                return m_analysisWindowSize;
            }
            set
            {
                m_analysisWindowSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval between adjacent calculations.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval between adjacent calculations. The default value is the frame-rate defined in the connection string for this Frequency Excursion."),
        DefaultValue("")]
        public int AnalysisInterval
        {
            get
            {
                return m_analysisInterval;
            }
            set
            {
                m_analysisInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of consecutive excursions needed in order to trip the alarm.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the minimum number of consecutive excursions needed in order to trip the alarm."),
        DefaultValue(2)]
        public int ConsecutiveDetections
        {
            get
            {
                return m_consecutiveDetections;
            }
            set
            {
                m_consecutiveDetections = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum frequency values needed to perform a valid calculation.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the minimum frequency values needed to perform a valid calculation."),
        DefaultValue(3)]
        public int MinimumValidChannels
        {
            get
            {
                return m_minimumValidChannels;
            }
            set
            {
                m_minimumValidChannels = value;
            }
        }

        /// <summary>
        /// Gets or sets the ratio used to calculate the total estimated MW change from frequency.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the ratio used to calculate the total estimated MW change from frequency."),
        DefaultValue(19530.0D)]
        public double PowerEstimateRatio
        {
            get
            {
                return m_powerEstimateRatio;
            }
            set
            {
                m_powerEstimateRatio = value;
            }
        }

        /// <summary>
        /// Gets or sets the period used to prevent duplicate alarms.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the period used to prevent duplicate alarms."),
        DefaultValue(20)]
        public int AlarmProhibitPeriod
        {
            get
            {
                return m_alarmProhibitPeriod / FramesPerSecond;
            }
            set
            {
                m_alarmProhibitPeriod = value * FramesPerSecond;
            }
        }

        /// <summary>
        /// Returns the detailed status of the <see cref="FrequencyExcursion"/> detector.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("Estimate trigger threshold: {0}", m_estimateTriggerThreshold);
                status.AppendLine();
                status.AppendFormat("      Analysis window size: {0}", m_analysisWindowSize);
                status.AppendLine();
                status.AppendFormat("         Analysis interval: {0}", m_analysisInterval);
                status.AppendLine();
                status.AppendFormat("   Detections before alarm: {0}", m_consecutiveDetections);
                status.AppendLine();
                status.AppendFormat(" Minimum valid frequencies: {0}", m_minimumValidChannels);
                status.AppendLine();
                status.AppendFormat("      Power estimate ratio: {0}MW", m_powerEstimateRatio);
                status.AppendLine();
                status.AppendFormat("    Minimum alarm interval: {0} seconds", (int)(m_alarmProhibitPeriod / FramesPerSecond));
                status.AppendLine();

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="FrequencyExcursion"/> detector.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            //  <Parameters paraName="EstimateTriggerThreshold" value="0.0256" description="The threshold of estimation trigger"></Parameters>
            //  <Parameters paraName="AnalysisWindowSize" value="120" description="The sample size of the analysis window"></Parameters>
            //  <Parameters paraName="AnalysisInterval" value="30" description="The frame interval between two adjacent frequency testing"></Parameters>
            //  <Parameters paraName="ConsecutiveDetections" value="2" description="Number of needed consecutive detections before positive alarm"></Parameters>
            //  <Parameters paraName="MinimumValidChannel" value="3" description="Minimum valid channel for conduction the frequency testing"></Parameters>
            //  <Parameters paraName="PowerEstimateRatio" value="19530.00" description="The ratio of total amount of generator (load) trip over the frequency excursion"></Parameters>
            //  <Parameters paraName="MinimumAlarmInterval" value="20" description="Minimum duration between alarms, in whole seconds"></Parameters>

            // Load required parameters
            if (settings.TryGetValue("estimateTriggerThreshold", out setting))
                m_estimateTriggerThreshold = double.Parse(setting);
            else
                m_estimateTriggerThreshold = 0.0256D;

            if (settings.TryGetValue("analysisWindowSize", out setting))
                m_analysisWindowSize = int.Parse(setting);
            else
                m_analysisWindowSize = 4 * FramesPerSecond;

            if (settings.TryGetValue("analysisInterval", out setting))
                m_analysisInterval = int.Parse(setting);
            else
                m_analysisInterval = FramesPerSecond;

            if (settings.TryGetValue("consecutiveDetections", out setting))
                m_consecutiveDetections = int.Parse(setting);
            else
                m_consecutiveDetections = 2;

            if (settings.TryGetValue("minimumValidChannels", out setting))
                m_minimumValidChannels = int.Parse(setting);
            else
                m_minimumValidChannels = 3;

            if (settings.TryGetValue("powerEstimateRatio", out setting))
                m_powerEstimateRatio = double.Parse(setting);
            else
                m_powerEstimateRatio = 19530.0D;

            if (settings.TryGetValue("minimumAlarmInterval", out setting))
                m_alarmProhibitPeriod = int.Parse(setting) * FramesPerSecond;
            else
                m_alarmProhibitPeriod = 20 * FramesPerSecond;

            m_frequencies = new List<double>();
            m_timeStamps = new List<DateTime>();

            // Validate input measurements
            List<MeasurementKey> validInputMeasurementKeys = new List<MeasurementKey>();

            for (int i = 0; i < InputMeasurementKeys.Length; i++)
            {
                if (InputMeasurementKeyTypes[i] == SignalType.FREQ)
                    validInputMeasurementKeys.Add(InputMeasurementKeys[i]);
            }

            if (validInputMeasurementKeys.Count == 0)
                throw new InvalidOperationException("No valid frequency measurements were specified as inputs to the frequency excursion detector.");

            if (validInputMeasurementKeys.Count < m_minimumValidChannels)
                throw new InvalidOperationException($"Minimum valid frequency measurements (i.e., \"minimumValidChannels\") for the frequency excursion detector is currently set to {m_minimumValidChannels}, only {validInputMeasurementKeys.Count} {(validInputMeasurementKeys.Count == 1 ? "was" : "were")} defined.");

            // Make sure only frequencies are used as input
            InputMeasurementKeys = validInputMeasurementKeys.ToArray();

            // Validate output measurements
            if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
                throw new InvalidOperationException("Not enough output measurements were specified for the frequency excursion detector, expecting measurements for \"Warning Signal Status (0 = Not Signaled, 1 = Signaled)\", \"Frequency Delta\", \"Type of Excursion (0 = Gen Trip, 1 = Load Trip)\" and \"Estimated Size (MW)\" - in this order.");
        }

        /// <summary>
        /// Publishes the <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// adapter's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            double averageFrequency = double.NaN;

            // Increment frame counter
            m_count++;

            if (m_alarmProhibitCounter > 0)
                m_alarmProhibitCounter--;

            // Calculate the average of all the frequencies that arrived in this frame
            if (frame.Measurements.Count > 0)
                averageFrequency = frame.Measurements.Select(m => m.Value.AdjustedValue).Average();

            // Track new frequency and its timestamp
            m_frequencies.Add(averageFrequency);
            m_timeStamps.Add(frame.Timestamp);

            // Maintain analysis window size
            while (m_frequencies.Count > m_analysisWindowSize)
            {
                m_frequencies.RemoveAt(0);
                m_timeStamps.RemoveAt(0);
            }

            if (m_count % m_analysisInterval == 0 && m_frequencies.Count == m_analysisWindowSize)
            {
                double frequency1 = m_frequencies[0];
                double frequency2 = m_frequencies[m_analysisWindowSize - 1];
                double frequencyDelta = 0.0D, estimatedSize = 0.0D;
                ExcursionType typeofExcursion = ExcursionType.GenerationTrip;
                bool warningSignaled = false;

                if (!double.IsNaN(frequency1) && !double.IsNaN(frequency2))
                {
                    frequencyDelta = frequency2 - frequency1;

                    if (Math.Abs(frequencyDelta) > m_estimateTriggerThreshold)
                        m_detectedExcursions++;
                    else
                        m_detectedExcursions = 0;

                    if (m_detectedExcursions >= m_consecutiveDetections)
                    {
                        typeofExcursion = (frequency1 > frequency2 ? ExcursionType.GenerationTrip : ExcursionType.LoadTrip);
                        estimatedSize = Math.Abs(frequencyDelta) * m_powerEstimateRatio;

                        // Display frequency excursion detection warning
                        if (m_alarmProhibitCounter == 0)
                        {
                            OutputFrequencyWarning(m_timeStamps[0], frequencyDelta, typeofExcursion, estimatedSize);
                            m_alarmProhibitCounter = m_alarmProhibitPeriod;
                        }

                        warningSignaled = true;
                    }
                }

                // Expose output measurement values
                IMeasurement[] outputMeasurements = OutputMeasurements;

                OnNewMeasurements(new IMeasurement[]
                { 
                    Measurement.Clone(outputMeasurements[(int)Output.WarningSignal], warningSignaled ? 1.0D : 0.0D, frame.Timestamp),
                    Measurement.Clone(outputMeasurements[(int)Output.FrequencyDelta], frequencyDelta, frame.Timestamp),
                    Measurement.Clone(outputMeasurements[(int)Output.TypeOfExcursion], (int)typeofExcursion, frame.Timestamp),
                    Measurement.Clone(outputMeasurements[(int)Output.EstimatedSize], estimatedSize, frame.Timestamp)
                });
            }
        }

        private void OutputFrequencyWarning(DateTime timestamp, double delta, ExcursionType typeOfExcursion, double totalAmount)
        {
            OnStatusMessage(MessageLevel.Info, $"Frequency excursion detected!\r\n              Time = {timestamp:dd-MMM-yyyy HH:mm:ss.fff}\r\n             Delta = {delta}\r\n              Type = {typeOfExcursion}\r\n    Estimated Size = {totalAmount:0.00}MW\r\n");
        }

        #endregion
    }
}