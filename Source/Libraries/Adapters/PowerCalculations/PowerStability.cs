//******************************************************************************************************
//  PowerStability.cs - Gbtc
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
//  11/08/2006 - J. Ritchie Carroll
//      Initial version of source generated
//  12/22/2009 - Jian R. Zuo
//       Converted code to C#;
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for stability calculator.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.Collections;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations;

/// <summary>
/// Represents an algorithm that calculates power and stability from a synchrophasor device.
/// </summary>
/// <remarks>
/// <para>
/// This algorithm calculates power and its standard deviation in real-time that can be used to
/// determine if there is an oscillatory signature in the power output.
/// </para>
/// <para>
/// If multiple voltage phasors are provided as inputs to this algorithm, then they are assumed to be
/// redundant values on the same bus, the first energized value will be the voltage phasor that is
/// used in the calculation.<br/>
/// If multiple current phasors are provided as inputs to this algorithm, then they are assumed to be
/// cumulative inputs representing the desired power output summation of the generation source.
/// </para>
/// <para>
/// Individual phase angle and magnitude phasor elements are expected to be defined consecutively.
/// That is the definition order of angles and magnitudes must match so that the angle / magnitude
/// pair can be matched up appropriately. For example: angle1;mag1;  angle2;mag2;  angle3;mag3.
/// </para>
/// </remarks>
[Description("Power Stability: Calculates power and stability for a synchrophasor device")]
public class PowerStability : CalculatedMeasurementBase
{
    #region [ Members ]

    // Fields
    private int m_minimumSamples;
    private List<double> m_powerDataSample;
    private double m_lastStdev;
    private MeasurementKey[] m_voltageAngles;
    private MeasurementKey[] m_voltageMagnitudes;
    private MeasurementKey[] m_currentAngles;
    private MeasurementKey[] m_currentMagnitudes;

    // Important: Make sure output definition defines points in the following order
    private enum Output
    {
        Power,
        StDev
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the sample size, in seconds, of the data to be monitored.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the sample size, in seconds, of the data to be monitored.")]
    [DefaultValue(15)]
    public int SampleSize
    {
        get => m_minimumSamples / FramesPerSecond;
        set => m_minimumSamples = value * FramesPerSecond;
    }

    /// <summary>
    /// Gets or sets the energized bus threshold, in volts. The recommended value is 20% of nominal line-to-neutral voltage.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the energized bus threshold, in volts. The recommended value is 20% of nominal line-to-neutral voltage.")]
    [DefaultValue(58000.0D)]
    public double EnergizedThreshold { get; set; }

    /// <summary>
    /// Returns the detailed status of the <see cref="PowerStability"/> monitor.
    /// </summary>
    public override string Status
    {
        get
        {
            const int ValuesToShow = 3;

            StringBuilder status = new();

            status.AppendFormat("          Data sample size: {0} seconds", m_minimumSamples / FramesPerSecond);
            status.AppendLine();
            status.AppendFormat("   Energized bus threshold: {0:0.00} volts", EnergizedThreshold);
            status.AppendLine();
            status.AppendFormat("     Total voltage phasors: {0}", m_voltageMagnitudes.Length);
            status.AppendLine();
            status.AppendFormat("     Total current phasors: {0}", m_currentMagnitudes.Length);
            status.AppendLine();
            status.Append("         Last power values: ");

            lock (m_powerDataSample)
            {
                // Display last several values
                if (m_powerDataSample.Count > ValuesToShow)
                    status.Append(m_powerDataSample.GetRange(m_powerDataSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MW")).ToDelimitedString(", "));
                else
                    status.Append("Not enough values calculated yet...");
            }

            status.AppendLine();
            status.AppendFormat("     Latest stdev of power: {0}", m_lastStdev);
            status.AppendLine();
            status.Append(base.Status);

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes the <see cref="PowerStability"/> monitor.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        // Load parameters
        if (settings.TryGetValue("sampleSize", out string setting))            // Data sample size to monitor, in seconds
            m_minimumSamples = int.Parse(setting) * FramesPerSecond;
        else
            m_minimumSamples = 15 * FramesPerSecond;

        if (settings.TryGetValue("energizedThreshold", out setting))    // Energized bus threshold, in volts, recommended value
            EnergizedThreshold = double.Parse(setting);               // is 20% of nominal line-to-neutral voltage
        else
            EnergizedThreshold = 58000.0D;

        // Load needed phase angle and magnitude measurement keys from defined InputMeasurementKeys
        m_voltageAngles = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA).ToArray();
        m_voltageMagnitudes = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM).ToArray();
        m_currentAngles = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).ToArray();
        m_currentMagnitudes = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).ToArray();

        if (m_voltageAngles.Length < 1)
            throw new InvalidOperationException("No voltage angle input measurement keys were not found - at least one voltage angle input measurement is required for the power stability monitor.");

        if (m_voltageMagnitudes.Length < 1)
            throw new InvalidOperationException("No voltage magnitude input measurement keys were not found - at least one voltage magnitude input measurement is required for the power stability monitor.");

        if (m_currentAngles.Length < 1)
            throw new InvalidOperationException("No current angle input measurement keys were not found - at least one current angle input measurement is required for the power stability monitor.");

        if (m_currentMagnitudes.Length < 1)
            throw new InvalidOperationException("No current magnitude input measurement keys were not found - at least one current magnitude input measurement is required for the power stability monitor.");

        if (m_voltageAngles.Length != m_voltageMagnitudes.Length)
            throw new InvalidOperationException("A different number of voltage magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one voltage magnitude input measurement must be supplied for each voltage angle input measurement in a consecutive sequence (e.g., VA1;VM1;  VA2;VM2; ... VAn;VMn)");

        if (m_currentAngles.Length != m_currentMagnitudes.Length)
            throw new InvalidOperationException("A different number of current magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one current magnitude input measurement must be supplied for each current angle input measurement in a consecutive sequence (e.g., IA1;IM1;  IA2;IM2; ... IAn;IMn)");

        // Make sure only these phasor measurements are used as input
        InputMeasurementKeys = m_voltageAngles.Concat(m_voltageMagnitudes).Concat(m_currentAngles).Concat(m_currentMagnitudes).ToArray();

        // Validate output measurements
        if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
            throw new InvalidOperationException("Not enough output measurements were specified for the power stability monitor, expecting measurements for the \"Calculated Power\", and the \"Standard Deviation of Power\" - in this order.");

        m_powerDataSample = new List<double>();
    }

    /// <summary>
    /// Publishes the <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
    /// adapter's defined <see cref="ConcentratorBase.LagTime"/>.
    /// </summary>
    /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
    /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
        IMeasurement magnitude, angle;
        double voltageMagnitude = double.NaN, voltageAngle = double.NaN, power = 0.0D;
        int i;

        // Get first voltage magnitude and angle value pair that is above the energized threshold
        for (i = 0; i < m_voltageMagnitudes.Length; i++)
        {
            if (!measurements.TryGetValue(m_voltageMagnitudes[i], out magnitude) || !measurements.TryGetValue(m_voltageAngles[i], out angle))
                continue;

            if (!(magnitude.AdjustedValue > EnergizedThreshold))
                continue;

            voltageMagnitude = magnitude.AdjustedValue;
            voltageAngle = angle.AdjustedValue;
            break;
        }

        // Exit if bus voltage measurements were not available for calculation
        if (double.IsNaN(voltageMagnitude))
            return;

        // Calculate the sum of the current phasors
        for (i = 0; i < m_currentMagnitudes.Length; i++)
        {
            // Retrieve current magnitude and angle measurements as consecutive pairs
            if (measurements.TryGetValue(m_currentMagnitudes[i], out magnitude) && measurements.TryGetValue(m_currentAngles[i], out angle))
                power += magnitude.AdjustedValue * Math.Cos(Angle.FromDegrees(angle.AdjustedValue - voltageAngle));
            else
                return; // Exit if current measurements were not available for calculation
        }

        // Apply bus voltage and convert to 3-phase megawatts
        power = power * voltageMagnitude / (SI.Mega / 3.0D);

        // Add latest calculated power to data sample
        lock (m_powerDataSample)
        {
            m_powerDataSample.Add(power);

            // Maintain sample size
            while (m_powerDataSample.Count > m_minimumSamples)
                m_powerDataSample.RemoveAt(0);
        }

        IMeasurement[] outputMeasurements = OutputMeasurements;
        Measurement powerMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Power], power, frame.Timestamp);

        // Check to see if the needed number of samples are available to begin producing the standard deviation output measurement
        // ReSharper disable once InconsistentlySynchronizedField
        if (m_powerDataSample.Count >= m_minimumSamples)
        {
            Measurement stdevMeasurement = Measurement.Clone(outputMeasurements[(int)Output.StDev], frame.Timestamp);

            lock (m_powerDataSample)
                stdevMeasurement.Value = m_powerDataSample.StandardDeviation();

            // Provide calculated measurements for external consumption
            OnNewMeasurements(new IMeasurement[] { powerMeasurement, stdevMeasurement });

            // Track last standard deviation...
            m_lastStdev = stdevMeasurement.AdjustedValue;
        }
        else if (power > 0.0D)
        {
            // If not, we can still start publishing power calculation as soon as we have one...
            OnNewMeasurements(new IMeasurement[] { powerMeasurement });
        }
    }

    #endregion
}