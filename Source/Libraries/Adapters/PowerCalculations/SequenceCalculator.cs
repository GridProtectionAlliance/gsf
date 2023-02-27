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

namespace PowerCalculations;

/// <summary>
/// Calculates positive, negative and zero sequences using A, B and C phase voltage or current magnitude and angle signals input to the adapter.
/// </summary>
[Description("Sequence Calculator: Computes positive, negative and zero sequences for synchrophasor measurements")]
public class SequenceCalculator : CalculatedMeasurementBase
{
    #region [ Members ]

    // Constants
    internal const bool DefaultTrackRecentValues = false;
    internal const int DefaultSampleSize = 5;
    internal const bool DefaultIncludePositiveSequence = true;
    internal const bool DefaultIncludeNegativeSequence = true;
    internal const bool DefaultIncludeZeroSequence = true;
    internal const BadDataStrategy DefaultBadDataStrategy = BadDataStrategy.FlagAsBad;
    
    private const double Rad120 = 2.0D * Math.PI / 3.0D;

    // Fields
    private MeasurementKey[] m_angles;
    private MeasurementKey[] m_magnitudes;
    private string m_magnitudeUnits;
    private ConcurrentQueue<double> m_positiveSequenceMagnitudeSample;
    private ConcurrentQueue<Angle> m_positiveSequenceAngleSample;
    private ConcurrentQueue<double> m_negativeSequenceMagnitudeSample;
    private ConcurrentQueue<Angle> m_negativeSequenceAngleSample;
    private ConcurrentQueue<double> m_zeroSequenceMagnitudeSample;
    private ConcurrentQueue<Angle> m_zeroSequenceAngleSample;

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
        PositiveSequenceMagnitude,
        /// <summary>
        /// Positive sequence angle measurement.
        /// </summary>
        PositiveSequenceAngle,
        /// <summary>
        /// Negative sequence magnitude measurement.
        /// </summary>
        NegativeSequenceMagnitude,
        /// <summary>
        /// Negative sequence angle measurement.
        /// </summary>
        NegativeSequenceAngle,
        /// <summary>
        /// Zero sequence magnitude measurement.
        /// </summary>
        ZeroSequenceMagnitude,
        /// <summary>
        /// Zero sequence angle measurement.
        /// </summary>
        ZeroSequenceAngle
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag that determines if the last few values should be monitored for diagnostics.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Flag that determines if the last few values should be monitored for diagnostics.")]
    [DefaultValue(DefaultTrackRecentValues)]
    public bool TrackRecentValues { get; set; } = DefaultTrackRecentValues;

    /// <summary>
    /// Gets or sets the sample size of the data to be monitored.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the sample size of the data to be monitored when TrackRecentValues is true.")]
    [DefaultValue(DefaultSampleSize)]
    public int SampleSize { get; set; } = DefaultSampleSize;

    /// <summary>
    /// Gets or sets flag that determines if positive sequence calculations should be included.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Flag that determines if positive sequence calculations should be included.")]
    [DefaultValue(DefaultIncludePositiveSequence)]
    public bool IncludePositiveSequence { get; set; } = DefaultIncludePositiveSequence;

    /// <summary>
    /// Gets or sets flag that determines if negative sequence calculations should be included.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Flag that determines if negative sequence calculations should be included.")]
    [DefaultValue(DefaultIncludeNegativeSequence)]
    public bool IncludeNegativeSequence { get; set; } = DefaultIncludeNegativeSequence;

    /// <summary>
    /// Gets or sets flag that determines if zero sequence calculations should be included.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Flag that determines if zero sequence calculations should be included.")]
    [DefaultValue(DefaultIncludeZeroSequence)]
    public bool IncludeZeroSequence { get; set; } = DefaultIncludeZeroSequence;

    /// <summary>
    /// Gets or sets the bad data strategy used to when inputs are marked with bad quality.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines bad data strategy used to when inputs are marked with bad quality.")]
    [DefaultValue(DefaultBadDataStrategy)]
    public BadDataStrategy BadDataStrategy { get; set; } = DefaultBadDataStrategy;

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
            StringBuilder status = new();

            status.Append(base.Status);
            
            status.AppendLine($"         Phase A magnitude: {m_magnitudes[0]}");
            status.AppendLine($"         Phase B magnitude: {m_magnitudes[1]}");
            status.AppendLine($"         Phase C magnitude: {m_magnitudes[2]}");
            status.AppendLine($"             Phase A angle: {m_angles[0]}");
            status.AppendLine($"             Phase B angle: {m_angles[1]}");
            status.AppendLine($"             Phase C angle: {m_angles[2]}");
            status.AppendLine($"         Bad Data Strategy: {BadDataStrategy}");

            if (!TrackRecentValues)
            {
                status.AppendLine();
                return status.ToString();
            }

            if (IncludePositiveSequence)
            {
                status.AppendLine();
                status.Append("   Last positive sequences: ");

                status.Append(m_positiveSequenceMagnitudeSample.Any() ?
                    m_positiveSequenceMagnitudeSample.Zip(m_positiveSequenceAngleSample, (mag, ang) => 
                        $"{mag:0.00} {m_magnitudeUnits}, {ang.ToDegrees():0.00}°").ToDelimitedString("\r\n                            ") :
                    "No values calculated yet...");
            }

            if (IncludeNegativeSequence)
            {
                status.AppendLine();
                status.Append("   Last negative sequences: ");

                status.Append(m_positiveSequenceMagnitudeSample.Any() ? 
                    m_negativeSequenceMagnitudeSample.Zip(m_negativeSequenceAngleSample, (mag, ang) => 
                        $"{mag:0.00} {m_magnitudeUnits}, {ang.ToDegrees():0.00}°").ToDelimitedString("\r\n                            ") : 
                    "No values calculated yet...");
            }

            if (IncludeZeroSequence)
            {
                status.AppendLine();
                status.Append("       Last zero sequences: ");

                status.Append(m_zeroSequenceMagnitudeSample.Any() ? 
                    m_zeroSequenceMagnitudeSample.Zip(m_zeroSequenceAngleSample, (mag, ang) => 
                        $"{mag:0.00} {m_magnitudeUnits}, {ang.ToDegrees():0.00}°").ToDelimitedString("\r\n                            ") : 
                    "No values calculated yet...");
            }

            status.AppendLine();
            status.AppendLine();

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
        if (settings.TryGetValue(nameof(TrackRecentValues), out string setting))
            TrackRecentValues = setting.ParseBoolean();

        // Data sample size to monitor, in seconds
        if (settings.TryGetValue(nameof(SampleSize), out setting) && int.TryParse(setting, out int sampleSize))
            SampleSize = sampleSize;

        if (settings.TryGetValue(nameof(IncludePositiveSequence), out setting))
            IncludePositiveSequence = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(IncludeNegativeSequence), out setting))
            IncludeNegativeSequence = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(IncludeZeroSequence), out setting))
            IncludeZeroSequence = setting.ParseBoolean();

        // Check old setting name for backwards compatibility with existing configurations
        if (settings.TryGetValue("UseBadQualityData", out setting))
            BadDataStrategy = setting.ParseBoolean() ? BadDataStrategy.FlagAsBad : BadDataStrategy.DropData;

        if (settings.TryGetValue(nameof(BadDataStrategy), out setting) && Enum.TryParse(setting, true, out BadDataStrategy badDataStrategy))
            BadDataStrategy = badDataStrategy;

        // Load needed phase angle measurement keys from defined InputMeasurementKeys
        m_angles = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA).ToArray();

        if (m_angles.Length == 0)
        {
            // No voltage angles existed, check for current angles
            m_angles = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).ToArray();
        }
        else
        {
            // Make sure only one kind of angles are defined - not a mixture of voltage and currents
            if (InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).Any())
                throw new InvalidOperationException("Angle input measurements for a single sequence calculator instance should only be for voltages or currents - not both.");
        }

        // Load needed phase magnitude measurement keys from defined InputMeasurementKeys
        m_magnitudes = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM).ToArray();

        if (m_magnitudes.Length == 0)
        {
            // No voltage magnitudes existed, check for current magnitudes
            m_magnitudes = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).ToArray();
            m_magnitudeUnits = "amps";
        }
        else
        {
            // Make only one kind of magnitudes are defined - not a mixture of voltage and currents
            if (InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).Any())
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
        if (IncludePositiveSequence && IncludeNegativeSequence && IncludeZeroSequence)
        {
            if (OutputMeasurements.Length < 6)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Positive Sequence Magnitude\", \"Positive Sequence Angle\", \"Negative Sequence Magnitude\", \"Negative Sequence Angle\", \"Zero Sequence Magnitude\" and \"Zero Sequence Angle\" - in this order.");
        }
        else if (IncludePositiveSequence && IncludeNegativeSequence)
        {
            if (OutputMeasurements.Length < 4)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Positive Sequence Magnitude\", \"Positive Sequence Angle\", \"Negative Sequence Magnitude\" and \"Negative Sequence Angle\" - in this order.");
        }
        else if (IncludePositiveSequence && IncludeZeroSequence)
        {
            if (OutputMeasurements.Length < 4)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Positive Sequence Magnitude\", \"Positive Sequence Angle\", \"Zero Sequence Magnitude\" and \"Zero Sequence Angle\" - in this order.");
        }
        else if (IncludeNegativeSequence && IncludeZeroSequence)
        {
            if (OutputMeasurements.Length < 4)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Negative Sequence Magnitude\", \"Negative Sequence Angle\", \"Zero Sequence Magnitude\" and \"Zero Sequence Angle\" - in this order.");
        }
        else if (IncludePositiveSequence)
        {
            if (OutputMeasurements.Length < 2)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Positive Sequence Magnitude\" and \"Positive Sequence Angle\" - in this order.");
        }
        else if (IncludeNegativeSequence)
        {
            if (OutputMeasurements.Length < 2)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Negative Sequence Magnitude\" and \"Negative Sequence Angle\" - in this order.");
        }
        else if (IncludeZeroSequence)
        {
            if (OutputMeasurements.Length < 2)
                throw new InvalidOperationException("Not enough output measurements were specified for the sequence calculator, expecting measurements for the \"Zero Sequence Magnitude\" and \"Zero Sequence Angle\" - in this order.");
        }
        else
        {
            throw new InvalidOperationException("At least one of positive, negative or sequence calculations must be included to calculate.");
        }

        if (TrackRecentValues)
        {
            m_positiveSequenceMagnitudeSample = new ConcurrentQueue<double>();
            m_positiveSequenceAngleSample = new ConcurrentQueue<Angle>();
            m_negativeSequenceMagnitudeSample = new ConcurrentQueue<double>();
            m_negativeSequenceAngleSample = new ConcurrentQueue<Angle>();
            m_zeroSequenceMagnitudeSample = new ConcurrentQueue<double>();
            m_zeroSequenceAngleSample = new ConcurrentQueue<Angle>();
        }

        // Assign a default adapter name to be used if sequence calculator is loaded as part of automated collection
        if (string.IsNullOrWhiteSpace(Name))
            Name = $"SC!{OutputMeasurements[(int)Output.PositiveSequenceMagnitude].Key}";
    }

    /// <summary>
    /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
    /// </summary>
    /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
    /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        ComplexNumber positiveSequence = nanSeq;
        ComplexNumber negativeSequence = nanSeq;
        ComplexNumber zeroSequence = nanSeq;
        bool badInputDetected = false;

        bool includeInput(IMeasurement measurement)
        {
            bool qualityIsGood = measurement.ValueQualityIsGood();

            if (BadDataStrategy == BadDataStrategy.DropData || qualityIsGood)
                return qualityIsGood;

            badInputDetected = true;
            return true;
        }

        try
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            double mA = 0.0D, aA = 0.0D, mB = 0.0D, aB = 0.0D, mC = 0.0D, aC = 0.0D;
            bool allInputsReceived = false;

            // Get all needed measurement values from this frame
            if (measurements.TryGetValue(m_magnitudes[0], out IMeasurement measurement) && includeInput(measurement))
            {
                // Get A-phase magnitude value
                mA = measurement.AdjustedValue;

                if (measurements.TryGetValue(m_angles[0], out measurement) && includeInput(measurement))
                {
                    // Get A-phase angle value
                    aA = measurement.AdjustedValue;

                    if (measurements.TryGetValue(m_magnitudes[1], out measurement) && includeInput(measurement))
                    {
                        // Get B-phase magnitude value
                        mB = measurement.AdjustedValue;

                        if (measurements.TryGetValue(m_angles[1], out measurement) && includeInput(measurement))
                        {
                            // Get B-phase angle value
                            aB = measurement.AdjustedValue;

                            if (measurements.TryGetValue(m_magnitudes[2], out measurement) && includeInput(measurement))
                            {
                                // Get C-phase magnitude value
                                mC = measurement.AdjustedValue;

                                if (measurements.TryGetValue(m_angles[2], out measurement) && includeInput(measurement))
                                {
                                    // Get C-phase angle value
                                    aC = measurement.AdjustedValue;

                                    allInputsReceived = true;
                                }
                            }
                        }
                    }
                }
            }

            if (!allInputsReceived)
                return;

            ComplexNumber aPhase = new(Angle.FromDegrees(aA), mA);
            ComplexNumber bPhase = new(Angle.FromDegrees(aB), mB);
            ComplexNumber cPhase = new(Angle.FromDegrees(aC), mC);

            if (IncludePositiveSequence)
                positiveSequence = (aPhase + a * bPhase + aSq * cPhase) / 3.0D;

            if (IncludeNegativeSequence)
                negativeSequence = (aPhase + aSq * bPhase + a * cPhase) / 3.0D;

            if (IncludeZeroSequence)
                zeroSequence = (aPhase + bPhase + cPhase) / 3.0D;

            if (!TrackRecentValues)
                return;

            // Add latest positive sequence to data sample
            if (IncludePositiveSequence)
            {
                m_positiveSequenceMagnitudeSample.Enqueue(positiveSequence.Magnitude);

                while (m_positiveSequenceMagnitudeSample.Count > SampleSize)
                    m_positiveSequenceMagnitudeSample.TryDequeue(out _);
                
                m_positiveSequenceAngleSample.Enqueue(positiveSequence.Angle);

                while (m_positiveSequenceAngleSample.Count > SampleSize)
                    m_positiveSequenceAngleSample.TryDequeue(out _);
            }

            // Add latest negative sequence to data sample
            if (IncludeNegativeSequence)
            {
                m_negativeSequenceMagnitudeSample.Enqueue(negativeSequence.Magnitude);

                while (m_negativeSequenceMagnitudeSample.Count > SampleSize)
                    m_negativeSequenceMagnitudeSample.TryDequeue(out _);

                m_negativeSequenceAngleSample.Enqueue(negativeSequence.Angle);

                while (m_negativeSequenceAngleSample.Count > SampleSize)
                    m_negativeSequenceAngleSample.TryDequeue(out _);
            }

            // Add latest zero sequence to data sample
            if (IncludeZeroSequence)
            {
                m_zeroSequenceMagnitudeSample.Enqueue(zeroSequence.Magnitude);

                while (m_zeroSequenceMagnitudeSample.Count > SampleSize)
                    m_zeroSequenceMagnitudeSample.TryDequeue(out _);

                m_zeroSequenceAngleSample.Enqueue(zeroSequence.Angle);

                while (m_zeroSequenceAngleSample.Count > SampleSize)
                    m_zeroSequenceAngleSample.TryDequeue(out _);
            }
        }
        finally
        {
            IMeasurement[] outputMeasurements = OutputMeasurements;
            MeasurementStateFlags flags = badInputDetected ? MeasurementStateFlags.BadData : MeasurementStateFlags.Normal;

            if (IncludePositiveSequence && IncludeNegativeSequence && IncludeZeroSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], positiveSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], positiveSequence.Angle.ToDegrees(), frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[2], negativeSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[3], negativeSequence.Angle.ToDegrees(), frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[4], zeroSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[5], zeroSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else if (IncludePositiveSequence && IncludeNegativeSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], positiveSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], positiveSequence.Angle.ToDegrees(), frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[2], negativeSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[3], negativeSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else if (IncludePositiveSequence && IncludeZeroSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], positiveSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], positiveSequence.Angle.ToDegrees(), frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[2], zeroSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[3], zeroSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else if (IncludeNegativeSequence && IncludeZeroSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], negativeSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], negativeSequence.Angle.ToDegrees(), frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[2], zeroSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[3], zeroSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else if (IncludePositiveSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], positiveSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], positiveSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else if (IncludeNegativeSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], negativeSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], negativeSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
            else // if (IncludeZeroSequence)
            {
                // Provide calculated measurements for external consumption
                OnNewMeasurements(new IMeasurement[]
                {
                    Measurement.Clone(outputMeasurements[0], zeroSequence.Magnitude, frame.Timestamp, flags),
                    Measurement.Clone(outputMeasurements[1], zeroSequence.Angle.ToDegrees(), frame.Timestamp, flags)
                });
            }
        }
    }

    #endregion

    #region [ Static ]

    // Static Fields

    // a = e^((2/3) * pi * i)
    private static readonly ComplexNumber a = new(Math.Cos(Rad120), Math.Sin(Rad120));
    private static readonly ComplexNumber aSq = a * a;
    private static readonly ComplexNumber nanSeq = new(double.NaN, double.NaN);

    #endregion
}