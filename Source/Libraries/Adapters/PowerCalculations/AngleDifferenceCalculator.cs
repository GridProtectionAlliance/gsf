//******************************************************************************************************
//  AngleDifferenceCalculator.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/26/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations;

/// <summary>
/// Calculates a composed angle difference.
/// </summary>
[Description("Angle Difference Calculator: Safely calculates a difference between two angles, unwrapping source angles as needed")]
public class AngleDifferenceCalculator : CalculatedMeasurementBase
{
    #region [ Members ]

    // Constants
    private const int ValuesToShow = 4;
    private const double PhaseResetAngle = 720.0D;

    // Fields
    private double[] m_lastAngles;
    private double[] m_unwrapOffsets;
    private List<double> m_latestCalculatedAngles;
    private IMeasurement[] m_measurements;

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag indicating whether or not this adapter will produce a result for all calculations. If this value is true and a calculation fails,
    /// the adapter will produce NaN for that calculation. If this value is false and a calculation fails, the adapter will not produce any result.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines flag that determines if adapter should always produce a result. When true, adapter will produce NaN for calculations that fail.")]
    [DefaultValue(false)]
    public bool AlwaysProduceResult { get; set; }
        
    /// <summary>
    /// Returns the detailed status of the <see cref="AngleDifferenceCalculator"/> calculator.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendFormat("  Last " + ValuesToShow + " calculated angles:");

            lock (m_latestCalculatedAngles)
            {
                if (m_latestCalculatedAngles.Count > ValuesToShow)
                    status.Append(m_latestCalculatedAngles.GetRange(m_latestCalculatedAngles.Count - ValuesToShow, ValuesToShow).Select(v => v.ToString("0.00°")).ToDelimitedString(", "));
                else
                    status.Append("Not enough values calculated yet ...");
            }

            status.AppendLine();
            status.Append(base.Status);

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes the <see cref="AngleDifferenceCalculator"/> calculator.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        // Validate input measurements
        List<MeasurementKey> validInputMeasurementKeys = new();

        for (int i = 0; i < InputMeasurementKeys.Length; i++)
        {
            SignalType keyType = InputMeasurementKeyTypes[i];

            // Make sure measurement key type is a phase angle
            if (keyType is SignalType.VPHA or SignalType.IPHA)
                validInputMeasurementKeys.Add(InputMeasurementKeys[i]);
        }

        // Make sure only phase angles are used as input
        if (validInputMeasurementKeys.Count == 0)
            throw new InvalidOperationException("No valid phase angles were specified as inputs to the angle difference calculator.");

        if (InputMeasurementKeyTypes.Count(s => s == SignalType.VPHA) > 0 && InputMeasurementKeyTypes.Count(s => s == SignalType.IPHA) > 0)
            throw new InvalidOperationException("A mixture of voltage and current phase angles were specified as inputs to the angle difference calculator - you must specify one or the other: only voltage phase angles or only current phase angles.");

        if (validInputMeasurementKeys.Count != 2)
            throw new InvalidOperationException($"Expected exactly two phase angles as inputs to the angle difference calculator, got {validInputMeasurementKeys.Count}.");

        InputMeasurementKeys = validInputMeasurementKeys.ToArray();

        // Validate output measurements
        if (OutputMeasurements.Length < 1)
            throw new InvalidOperationException("An output measurement was not specified for the angle difference calculator - one measurement is expected to represent the \"Calculated Angle Difference\" value.");

        Dictionary<string, string> settings = Settings;

        if (settings.TryGetValue("AlwaysProduceResult", out string setting))
            AlwaysProduceResult = setting.ParseBoolean();
            
        // Initialize member fields
        m_lastAngles = new double[2];
        m_unwrapOffsets = new double[2];
        m_latestCalculatedAngles = new List<double>();

        // Set last angles as uninitialized
        m_lastAngles[0] = double.NaN;
        m_lastAngles[1] = double.NaN;
    }

    /// <summary>
    /// Calculates an angle difference.
    /// </summary>
    /// <param name="frame">Single frame of measurement data within one second samples</param>
    /// <param name="index">Index of frame within the one second samples</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        Measurement calculatedMeasurement = Measurement.Clone(OutputMeasurements[0], frame.Timestamp);
        double angleDifference, angle1 = double.NaN, angle2 = double.NaN;

        // Attempt to get minimum needed reporting set of composite angles used to calculate angle difference
        if (TryGetMinimumNeededMeasurements(frame, ref m_measurements))
        {
            // Get both phase angles, unwrapping as necessary
            for (int i = 0; i < m_measurements.Length; i++)
            {
                // Get current angle value
                double angle = m_measurements[i].AdjustedValue;

                // Get the unwrap offset for this angle
                double unwrapOffset = m_unwrapOffsets[i];

                // Get angle value from last run, if there was a last run
                double lastAngle = m_lastAngles[i];

                if (!double.IsNaN(lastAngle))
                {
                    // Calculate the angle difference from last run
                    double deltaAngle = angle - lastAngle;

                    // Adjust angle unwrap offset if needed
                    if (deltaAngle > 300)
                        unwrapOffset -= 360;
                    else if (deltaAngle < -300)
                        unwrapOffset += 360;

                    // Reset angle unwrap offset if needed
                    if (unwrapOffset > PhaseResetAngle)
                        unwrapOffset -= PhaseResetAngle;
                    else if (unwrapOffset < -PhaseResetAngle)
                        unwrapOffset += PhaseResetAngle;

                    // Record last angle unwrap offset
                    m_unwrapOffsets[i] = unwrapOffset;
                }

                // Pick off unwrapped angles
                if (i == 0)
                    angle1 = angle + unwrapOffset;
                else
                    angle2 = angle + unwrapOffset;
            }

            // We use modulus function to make sure angle is in range of 0 to 360
            angleDifference = (angle1 - angle2) % 360.0D;

            // Record last angles for next run
            for (int i = 0; i < m_measurements.Length; i++)
                m_lastAngles[i] = m_measurements[i].AdjustedValue;
        }
        else
        {
            angleDifference = double.NaN;
            calculatedMeasurement.StateFlags |= MeasurementStateFlags.BadData;
        }

        // Convert angle value to the range of -180 to 180
        if (angleDifference > 180)
            angleDifference -= 360;

        if (angleDifference <= -180)
            angleDifference += 360;

        if (double.IsNaN(angleDifference) && !AlwaysProduceResult)
            return;

        calculatedMeasurement.Value = angleDifference;

        // Expose calculated value
        OnNewMeasurements(new IMeasurement[] { calculatedMeasurement });

        // Track last few calculated reference angles
        lock (m_latestCalculatedAngles)
        {
            m_latestCalculatedAngles.Add(angleDifference);

            while (m_latestCalculatedAngles.Count > ValuesToShow)
                m_latestCalculatedAngles.RemoveAt(0);
        }
    }

    #endregion
}