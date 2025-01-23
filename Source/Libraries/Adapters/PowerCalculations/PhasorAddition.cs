//******************************************************************************************************
//  PhasorAddition.cs - Gbtc
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
//  05/14/2025 - C. Lackner
//       Generated original version of source code.
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
/// Calculates sum or difference between two voltages or currents.
/// </summary>
[Description("Phasor Addition: Computes the sum or difference between two phasors")]
public class PhasorAddition : CalculatedMeasurementBase
{
    #region [ Members ]

    // Constants
    private const double Rad120 = 2.0D * Math.PI / 3.0D;
    private const bool DefaultDifference = false;
    // Fields
    private MeasurementKey[] m_angles;
    private MeasurementKey[] m_magnitudes;
    private double m_lastMagnitudeResult;
    private Angle m_lastAngleResult;

    /// <summary>
    /// Defines the output measurements for the <see cref="PhasorAddition"/>.
    /// </summary>
    /// <remarks>
    /// One output measurement should be defined for each enumeration value, in order:
    /// </remarks>
    public enum Output
    {
        /// <summary>
        /// Magnitude measurement.
        /// </summary>
        Magnitude,
        /// <summary>
        /// Angle measurement.
        /// </summary>
        Phase
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag that determines if the second phasor should be subtracted.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Flag that determines if the second phasor should be subtracted.")]
    [DefaultValue(DefaultDifference)]
    public bool Difference { get; set; } = DefaultDifference;

    /// <summary>
    /// Gets the flag indicating if this adapter supports temporal processing.
    /// </summary>
    public override bool SupportsTemporalProcessing => true;

    /// <summary>
    /// Returns the detailed status of the <see cref="PhasorAddition"/>.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.Append(base.Status);
            
            status.AppendLine($"         Phasor 1 magnitude: {m_magnitudes[0]}");
            status.AppendLine($"         Phasor 2 magnitude: {m_magnitudes[1]}");
            status.AppendLine($"             Phase 1 angle: {m_angles[0]}");
            status.AppendLine($"             Phase 2 angle: {m_angles[1]}");
            status.AppendLine($"                Difference: {Difference}");

            status.AppendLine();
            status.Append("   Last calculated angle: ");

            status.Append(!double.IsNaN(m_lastAngleResult) ? $"{m_lastAngleResult.ToDegrees():0.00}°" :                "No values calculated yet...");
           
            status.AppendLine();
            status.Append("       Last calculated magnitude: ");

            status.Append(!double.IsNaN(m_lastMagnitudeResult) ? $"{m_lastMagnitudeResult:0.00}" : 
                "No values calculated yet...");
            
            status.AppendLine();
            status.AppendLine();

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes the <see cref="PhasorAddition"/>.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        // Load parameters
        if (settings.TryGetValue(nameof(Difference), out string setting))
            Difference = setting.ParseBoolean();

        // Load needed phase angle measurement keys from defined InputMeasurementKeys
        m_angles = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA || InputMeasurementKeyTypes[index] == SignalType.IPHA).ToArray();

        if (m_angles.Length != 2)
        {
            throw new InvalidOperationException("Exactly 2 angle input measurements are required.");
        }
       
        // Load needed phase magnitude measurement keys from defined InputMeasurementKeys
        m_magnitudes = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM || InputMeasurementKeyTypes[index] == SignalType.IPHM).ToArray();

        if (m_magnitudes.Length != 2)
        {
            throw new InvalidOperationException("Exactly 2 magnitude input measurements are required.");
        }
       
        
        // Make sure only these phasor measurements are used as input
        InputMeasurementKeys = m_angles.Concat(m_magnitudes).ToArray();

        // Validate output measurements     
        if (OutputMeasurements.Length < 2)
            throw new InvalidOperationException("Not enough output measurements were specified for the phasor addition, expecting measurements for the \"Magnitude\", and  \"Angle\" - in this order.");
       
        m_lastMagnitudeResult = double.NaN;
        m_lastAngleResult = double.NaN;

    }

    /// <summary>
    /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
    /// </summary>
    /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
    /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        ComplexNumber result = nanSeq;

        try
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            double m1 = 0.0D, a1 = 0.0D, m2 = 0.0D, a2 = 0.0D;
            // Get first magnitude value
            if (!measurements.TryGetValue(m_magnitudes[0], out IMeasurement measurement))
                return;

            m1 = measurement.AdjustedValue;

            // Get first angle value
            if (!measurements.TryGetValue(m_angles[0], out measurement))
                return;

            a1 = measurement.AdjustedValue;

            // Get second magnitude value
            if (!measurements.TryGetValue(m_magnitudes[1], out measurement))
                return;

            m2 = measurement.AdjustedValue;

            // Get second angle value
            if (!measurements.TryGetValue(m_angles[1], out measurement))
                return;

            a2 = measurement.AdjustedValue;

            ComplexNumber phasor1 = new(Angle.FromDegrees(a1), m1);
            ComplexNumber phasor2 = new(Angle.FromDegrees(a2), m2);

            if (Difference)
                result = phasor1 - phasor2;
            else
                result = phasor1 + phasor2;
        }
        finally
        {
            IMeasurement[] outputMeasurements = OutputMeasurements;
           
            m_lastAngleResult = result.Angle.ToDegrees();
            m_lastMagnitudeResult = result.Magnitude;
            // Provide calculated measurements for external consumption
            OnNewMeasurements(new IMeasurement[]
            {
                Measurement.Clone(outputMeasurements[0], result.Magnitude, frame.Timestamp),
                Measurement.Clone(outputMeasurements[1], result.Angle.ToDegrees(), frame.Timestamp)
            });
        }
    }

    #endregion

    #region [ Static ]

    private static readonly ComplexNumber nanSeq = new(double.NaN, double.NaN);

    #endregion
}