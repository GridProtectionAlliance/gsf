//******************************************************************************************************
//  AverageFrequency.cs - Gbtc
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
//  11/22/2006 - J. Ritchie Carroll
//       Initial version of source generated
//  12/24/2009 - Jian R. Zuo
//       Converted code to C#
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for average calculation.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GSF.TimeSeries;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations
{
    /// <summary>
    /// Calculates a real-time average frequency reporting the average, maximum and minimum values.
    /// </summary>
    [Description("Average Frequency: Calculates a real-time average frequency reporting the average, maximum, and minimum values")]
    public class AverageFrequency : CalculatedMeasurementBase
    {
        #region [ Members ]

        // Constants
        private const double LoFrequency = 57.0D;
        private const double HiFrequency = 62.0D;

        // Fields
        private double m_averageFrequency;
        private double m_maximumFrequency;
        private double m_minimumFrequency;
        private readonly ConcurrentDictionary<Guid, int> m_lastValues = new ConcurrentDictionary<Guid, int>();

        // Important: Make sure output definition defines points in the following order
        private enum Output
        {
            Average,
            Maximum,
            Minimum
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the detailed status of the <see cref="AverageFrequency"/> calculator.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("    Last average frequency: {0}", m_averageFrequency);
                status.AppendLine();
                status.AppendFormat("    Last maximum frequency: {0}", m_maximumFrequency);
                status.AppendLine();
                status.AppendFormat("    Last minimum frequency: {0}", m_minimumFrequency);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }
        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="AverageFrequency"/> calculator.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Validate input measurements
            List<MeasurementKey> validInputMeasurementKeys = new List<MeasurementKey>();

            for (int i = 0; i < InputMeasurementKeys.Length; i++)
            {
                if (InputMeasurementKeyTypes[i] == SignalType.FREQ)
                    validInputMeasurementKeys.Add(InputMeasurementKeys[i]);
            }

            if (validInputMeasurementKeys.Count == 0)
                throw new InvalidOperationException("No valid frequency measurements were specified as inputs to the average frequency calculator.");

            // Make sure only frequencies are used as input
            InputMeasurementKeys = validInputMeasurementKeys.ToArray();

            // Validate output measurements
            if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
                throw new InvalidOperationException("Not enough output measurements were specified for the average frequency calculator, expecting measurements for \"Average\", \"Maximum\", and \"Minimum\" frequencies - in this order.");
        }

        /// <summary>
        /// Calculates the average frequency for all frequencies that have reported in the specified lag time.
        /// </summary>
        /// <param name="frame">Single frame of measurement data within a one second sample.</param>
        /// <param name="index">Index of frame within the one second sample.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            if (frame.Measurements.Count > 0)
            {
                const double hzResolution = 1000.0; // three decimal places

                double frequency;
                double frequencyTotal;
                double maximumFrequency = LoFrequency;
                double minimumFrequency = HiFrequency;
                int adjustedFrequency;
                int lastValue;
                int total;

                frequencyTotal = 0.0D;
                total = 0;

                foreach (IMeasurement measurement in frame.Measurements.Values)
                {
                    frequency = measurement.AdjustedValue;
                    adjustedFrequency = (int)(frequency * hzResolution);

                    // Do some simple flat line avoidance...
                    if (m_lastValues.TryGetValue(measurement.ID, out lastValue))
                    {
                        if (lastValue == adjustedFrequency)
                            frequency = 0.0D;
                        else
                            m_lastValues[measurement.ID] = adjustedFrequency;
                    }
                    else
                    {
                        m_lastValues[measurement.ID] = adjustedFrequency;
                    }

                    // Validate frequency
                    if (frequency > LoFrequency && frequency < HiFrequency)
                    {
                        frequencyTotal += frequency;

                        if (frequency > maximumFrequency)
                            maximumFrequency = frequency;

                        if (frequency < minimumFrequency)
                            minimumFrequency = frequency;

                        total++;
                    }
                }

                if (total > 0)
                {
                    m_averageFrequency = (frequencyTotal / total);
                    m_maximumFrequency = maximumFrequency;
                    m_minimumFrequency = minimumFrequency;
                }

                // Provide calculated measurements for external consumption
                IMeasurement[] outputMeasurements = OutputMeasurements;

                OnNewMeasurements(new IMeasurement[]{
                    Measurement.Clone(outputMeasurements[(int)Output.Average], m_averageFrequency, frame.Timestamp),
                    Measurement.Clone(outputMeasurements[(int)Output.Maximum], m_maximumFrequency, frame.Timestamp),
                    Measurement.Clone(outputMeasurements[(int)Output.Minimum], m_minimumFrequency, frame.Timestamp)});
            }
            else
            {
                m_averageFrequency = 0.0D;
                m_maximumFrequency = 0.0D;
                m_minimumFrequency = 0.0D;
            }
        }

        #endregion
    }
}