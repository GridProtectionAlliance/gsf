//******************************************************************************************************
//  SequenceCalculator.cs - Gbtc
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
//  05/16/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations
{
    /// <summary>
    /// Calculates positive, negative and zero sequences using A, B and C phase voltage or current magnitude and angle signals input to the adapter.
    /// </summary>
    [Description("Sequence Calculator: Computes positive, negative and zero sequences for synchrophasor measurements")]
    public class SequenceCalculator : CalculatedMeasurementBase
    {
        #region [ Members ]

        // Constants
        private const double Rad120 = 2.0D * Math.PI / 3.0D;

        // Fields
        private MeasurementKey[] m_angles;
        private MeasurementKey[] m_magnitudes;
        private string m_magnitudeUnits;
        private List<double> m_positiveMagnitudeSample;
        private List<double> m_positiveAngleSample;
        private List<double> m_negativeMagnitudeSample;
        private List<double> m_negativeAngleSample;
        private List<double> m_zeroMagnitudeSample;
        private List<double> m_zeroAngleSample;

        /// <summary>
        /// Defines the output measurements for the <see cref="SequenceCalculator"/>.
        /// </summary>
        /// <remarks>
        /// One output measurement should be defined for each enumeration value, in order:
        /// </remarks>
        public enum Output
        {
            /// <summary>
            /// Positive sequence magnitude measurement.
            /// </summary>
            PositiveMagnitude,
            /// <summary>
            /// Positive sequence angle measurement.
            /// </summary>
            PositiveAngle,
            /// <summary>
            /// Negative sequence magnitude measurement.
            /// </summary>
            NegativeMagnitude,
            /// <summary>
            /// Negative sequence angle measurement.
            /// </summary>
            NegativeAngle,
            /// <summary>
            /// Zero sequence magnitude measurement.
            /// </summary>
            ZeroMagnitude,
            /// <summary>
            /// Zero sequence angle measurement.
            /// </summary>
            ZeroAngle
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if the last few values should be monitored.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Flag that determines if the last few values should be monitored.")]
        [DefaultValue(true)]
        public bool TrackRecentValues { get; set; }

        /// <summary>
        /// Gets or sets the sample size of the data to be monitored.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the sample size of the data to be monitored.")]
        [DefaultValue(5)]
        public int SampleSize { get; set; }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => true;

        /// <summary>
        /// Returns the detailed status of the <see cref="PowerCalculator"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                const int ValuesToShow = 3;

                StringBuilder status = new StringBuilder();

                status.AppendFormat("         Phase A magnitude: {0}", m_magnitudes[0]);
                status.AppendLine();
                status.AppendFormat("         Phase B magnitude: {0}", m_magnitudes[1]);
                status.AppendLine();
                status.AppendFormat("         Phase C magnitude: {0}", m_magnitudes[2]);
                status.AppendLine();
                status.AppendFormat("             Phase A angle: {0}", m_angles[0]);
                status.AppendLine();
                status.AppendFormat("             Phase B angle: {0}", m_angles[1]);
                status.AppendLine();
                status.AppendFormat("             Phase C angle: {0}", m_angles[2]);
                status.AppendLine();

                if (TrackRecentValues)
                {
                    status.AppendLine();

                    status.Append("   Last positive sequences: ");

                    lock (m_positiveMagnitudeSample)
                    {
                        lock (m_positiveAngleSample)
                        {
                            // Display last several values
                            if (m_positiveMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> positiveMagnitudeSample = m_positiveMagnitudeSample.GetRange(m_positiveMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> positiveAngleSample = m_positiveAngleSample.GetRange(m_positiveMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(positiveMagnitudeSample.Zip(positiveAngleSample, (mV, mA) => $"{mV:0.00} {m_magnitudeUnits}, {mA:0.00}°").ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();

                    status.Append("   Last negative sequences: ");

                    lock (m_negativeMagnitudeSample)
                    {
                        lock (m_negativeAngleSample)
                        {
                            // Display last several values
                            if (m_positiveMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> negativeMagnitudeSample = m_negativeMagnitudeSample.GetRange(m_negativeMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> negativeAngleSample = m_negativeAngleSample.GetRange(m_negativeMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(negativeMagnitudeSample.Zip(negativeAngleSample, (mV, mA) => $"{mV:0.00} {m_magnitudeUnits}, {mA:0.00}°").ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();

                    status.Append("       Last zero sequences: ");

                    lock (m_zeroMagnitudeSample)
                    {
                        lock (m_zeroAngleSample)
                        {
                            // Display last several values
                            if (m_zeroMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> zeroMagnitudeSample = m_zeroMagnitudeSample.GetRange(m_zeroMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> zeroAngleSample = m_zeroAngleSample.GetRange(m_zeroMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(zeroMagnitudeSample.Zip(zeroAngleSample, (mV, mA) => $"{mV:0.00} {m_magnitudeUnits}, {mA:0.00}°").ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();
                    status.AppendLine();
                }

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="PowerCalculator"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            // Load parameters
            TrackRecentValues = !settings.TryGetValue("trackRecentValues", out string setting) || setting.ParseBoolean();

            // Data sample size to monitor, in seconds
            if (settings.TryGetValue("sampleSize", out setting) && int.TryParse(setting, out int sampleSize))
                SampleSize = sampleSize;
            else
                SampleSize = 5;

            // Load needed phase angle measurement keys from defined InputMeasurementKeys
            m_angles = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA).ToArray();

            if (m_angles.Length == 0)
            {
                // No voltage angles existed, check for current angles
                m_angles = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).ToArray();
            }
            else
            {
                // Make sure only one kind of angles are defined - not a mixture of voltage and currents
                if (InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).Any())
                    throw new InvalidOperationException("Angle input measurements for a single sequence calculator instance should only be for voltages or currents - not both.");
            }

            // Load needed phase magnitude measurement keys from defined InputMeasurementKeys
            m_magnitudes = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM).ToArray();

            if (m_magnitudes.Length == 0)
            {
                // No voltage magnitudes existed, check for current magnitudes
                m_magnitudes = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).ToArray();
                m_magnitudeUnits = "amps";
            }
            else
            {
                // Make only one kind of magnitudes are defined - not a mixture of voltage and currents
                if (InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).Any())
                    throw new InvalidOperationException("Magnitude input measurements for a single sequence calculator instance should only be for voltages or currents - not both.");

                m_magnitudeUnits = "volts";
            }

            if (m_angles.Length < 3)
                throw new InvalidOperationException("Three angle input measurements, i.e., A, B and C - in that order, are required for the sequence calculator.");

            if (m_magnitudes.Length < 3)
                throw new InvalidOperationException("Three magnitude input measurements, i.e., A, B and C - in that order, are required for the sequence calculator.");

            if (m_angles.Length != m_magnitudes.Length)
                throw new InvalidOperationException("A different number of magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs in A, B, C sequence, i.e., one magnitude input measurement must be supplied for each angle input measurement in a consecutive sequence (e.g., A1;M1;  A2;M2; ... An;Mn)");

            // Make sure only these phasor measurements are used as input
            InputMeasurementKeys = m_angles.Concat(m_magnitudes).ToArray();

            // Validate output measurements
            if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Positive Sequence Magnitude\", \"Positive Sequence Angle\", \"Negative Sequence Magnitude\", \"Negative Sequence Angle\", \"Zero Sequence Magnitude\" and \"Zero Sequence Angle\" - in this order.");

            if (TrackRecentValues)
            {
                m_positiveMagnitudeSample = new List<double>();
                m_positiveAngleSample = new List<double>();
                m_negativeMagnitudeSample = new List<double>();
                m_negativeAngleSample = new List<double>();
                m_zeroMagnitudeSample = new List<double>();
                m_zeroAngleSample = new List<double>();
            }

            // Assign a default adapter name to be used if sequence calculator is loaded as part of automated collection
            if (string.IsNullOrWhiteSpace(Name))
                Name = $"SC!{OutputMeasurements[(int)Output.PositiveMagnitude].Key}";
        }

        /// <summary>
        /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
        /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ComplexNumber positiveSequence = new ComplexNumber(double.NaN, double.NaN);
            ComplexNumber negativeSequence = new ComplexNumber(double.NaN, double.NaN);
            ComplexNumber zeroSequence = new ComplexNumber(double.NaN, double.NaN);

            try
            {
                ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
                double mA = 0.0D, aA = 0.0D, mB = 0.0D, aB = 0.0D, mC = 0.0D, aC = 0.0D;
                bool allValuesReceived = false;

                // Get all needed measurement values from this frame
                if (measurements.TryGetValue(m_magnitudes[0], out IMeasurement measurement) && measurement.ValueQualityIsGood())
                {
                    // Get A-phase magnitude value
                    mA = measurement.AdjustedValue;

                    if (measurements.TryGetValue(m_angles[0], out measurement) && measurement.ValueQualityIsGood())
                    {
                        // Get A-phase angle value
                        aA = measurement.AdjustedValue;

                        if (measurements.TryGetValue(m_magnitudes[1], out measurement) && measurement.ValueQualityIsGood())
                        {
                            // Get B-phase magnitude value
                            mB = measurement.AdjustedValue;

                            if (measurements.TryGetValue(m_angles[1], out measurement) && measurement.ValueQualityIsGood())
                            {
                                // Get B-phase angle value
                                aB = measurement.AdjustedValue;

                                if (measurements.TryGetValue(m_magnitudes[2], out measurement) && measurement.ValueQualityIsGood())
                                {
                                    // Get C-phase magnitude value
                                    mC = measurement.AdjustedValue;

                                    if (measurements.TryGetValue(m_angles[2], out measurement) && measurement.ValueQualityIsGood())
                                    {
                                        // Get C-phase angle value
                                        aC = measurement.AdjustedValue;

                                        allValuesReceived = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!allValuesReceived)
                    return;

                ComplexNumber aPhase = new ComplexNumber(Angle.FromDegrees(aA), mA);
                ComplexNumber bPhase = new ComplexNumber(Angle.FromDegrees(aB), mB);
                ComplexNumber cPhase = new ComplexNumber(Angle.FromDegrees(aC), mC);

                zeroSequence = (aPhase + bPhase + cPhase) / 3.0D;
                positiveSequence = (aPhase + a * bPhase + aSq * cPhase) / 3.0D;
                negativeSequence = (aPhase + aSq * bPhase + a * cPhase) / 3.0D;

                if (!TrackRecentValues)
                    return;

                // Add latest positive sequence to data sample
                lock (m_positiveMagnitudeSample)
                {
                    m_positiveMagnitudeSample.Add(positiveSequence.Magnitude);

                    // Maintain sample size
                    while (m_positiveMagnitudeSample.Count > SampleSize)
                        m_positiveMagnitudeSample.RemoveAt(0);
                }

                lock (m_positiveAngleSample)
                {
                    m_positiveAngleSample.Add(positiveSequence.Angle);

                    // Maintain sample size
                    while (m_positiveAngleSample.Count > SampleSize)
                        m_positiveAngleSample.RemoveAt(0);
                }

                // Add latest negative sequence to data sample
                lock (m_negativeMagnitudeSample)
                {
                    m_negativeMagnitudeSample.Add(negativeSequence.Magnitude);

                    // Maintain sample size
                    while (m_negativeMagnitudeSample.Count > SampleSize)
                        m_negativeMagnitudeSample.RemoveAt(0);
                }

                lock (m_negativeAngleSample)
                {
                    m_negativeAngleSample.Add(negativeSequence.Angle);

                    // Maintain sample size
                    while (m_negativeAngleSample.Count > SampleSize)
                        m_negativeAngleSample.RemoveAt(0);
                }

                // Add latest zero sequence to data sample
                lock (m_zeroMagnitudeSample)
                {
                    m_zeroMagnitudeSample.Add(zeroSequence.Magnitude);

                    // Maintain sample size
                    while (m_zeroMagnitudeSample.Count > SampleSize)
                        m_zeroMagnitudeSample.RemoveAt(0);
                }

                lock (m_zeroAngleSample)
                {
                    m_zeroAngleSample.Add(zeroSequence.Angle);

                    // Maintain sample size
                    while (m_zeroAngleSample.Count > SampleSize)
                        m_zeroAngleSample.RemoveAt(0);
                }
            }
            finally
            {
                IMeasurement[] outputMeasurements = OutputMeasurements;

                Measurement positiveMagnitudeMeasurement = Measurement.Clone(outputMeasurements[(int)Output.PositiveMagnitude], positiveSequence.Magnitude, frame.Timestamp);
                Measurement positiveAngleMeasurement = Measurement.Clone(outputMeasurements[(int)Output.PositiveAngle], positiveSequence.Angle.ToDegrees(), frame.Timestamp);
                Measurement negativeMagnitudeMeasurement = Measurement.Clone(outputMeasurements[(int)Output.NegativeMagnitude], negativeSequence.Magnitude, frame.Timestamp);
                Measurement negativeAngleMeasurement = Measurement.Clone(outputMeasurements[(int)Output.NegativeAngle], negativeSequence.Angle.ToDegrees(), frame.Timestamp);
                Measurement zeroMagnitudeMeasurement = Measurement.Clone(outputMeasurements[(int)Output.ZeroMagnitude], zeroSequence.Magnitude, frame.Timestamp);
                Measurement zeroAngleMeasurement = Measurement.Clone(outputMeasurements[(int)Output.ZeroAngle], zeroSequence.Angle.ToDegrees(), frame.Timestamp);

                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[] { positiveMagnitudeMeasurement, positiveAngleMeasurement, negativeMagnitudeMeasurement, negativeAngleMeasurement, zeroMagnitudeMeasurement, zeroAngleMeasurement });
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        // a = e^((2/3) * pi * i)
        private static readonly ComplexNumber a = new ComplexNumber(Math.Cos(Rad120), Math.Sin(Rad120));
        private static readonly ComplexNumber aSq = a * a;

        /// <summary>
        /// Array of <see cref="Output"/> enumeration values.
        /// </summary>
        public static readonly Output[] Outputs = Enum.GetValues(typeof(Output)) as Output[];

        #endregion
    }
}