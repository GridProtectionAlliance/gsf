//******************************************************************************************************
//  PowerCalculator.cs - Gbtc
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
/// Calculates MW and MVAR using Voltage and Current Magnitude and Angle signals input to the adapter.
/// </summary>
[Description("Power Calculator: Calculates power and reactive power for synchrophasor measurements")]
public class PowerCalculator : CalculatedMeasurementBase
{
    #region [ Members ]

    // Constants
    private const double SqrtOf3 = 1.7320508075688772935274463415059D;

    // Fields
    private MeasurementKey m_voltageAngle;
    private MeasurementKey m_voltageMagnitude;
    private MeasurementKey m_currentAngle;
    private MeasurementKey m_currentMagnitude;
    private List<double> m_powerSample;
    private List<double> m_reactivePowerSample;

    // Important: Make sure output definition defines points in the following order
    private enum Output
    {
        Power,
        ReactivePower
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

            StringBuilder status = new();

            if (TrackRecentValues)
            {
                status.Append("         Last power values: ");

                lock (m_powerSample)
                {
                    // Display last several values
                    if (m_powerSample.Count > ValuesToShow)
                        status.Append(m_powerSample.GetRange(m_powerSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MW")).ToDelimitedString(", "));
                    else
                        status.Append("Not enough values calculated yet...");
                }
                status.AppendLine();

                status.Append("Last reactive power values: ");

                lock (m_reactivePowerSample)
                {
                    // Display last several values
                    if (m_reactivePowerSample.Count > ValuesToShow)
                        status.Append(m_reactivePowerSample.GetRange(m_reactivePowerSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MVAR")).ToDelimitedString(", "));
                    else
                        status.Append("Not enough values calculated yet...");
                }
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
        SampleSize = settings.TryGetValue("sampleSize", out setting) ? int.Parse(setting) : 5;

        // Load needed phase angle and magnitude measurement keys from defined InputMeasurementKeys
        m_voltageAngle = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA).FirstOrDefault();
        m_voltageMagnitude = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM).FirstOrDefault();
        m_currentAngle = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).FirstOrDefault();
        m_currentMagnitude = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).FirstOrDefault();

        if (m_voltageAngle == null || m_voltageAngle.ID == 0)
            throw new InvalidOperationException("No voltage angle input was defined - one voltage angle input measurement is required for the power calculator.");

        if (m_voltageMagnitude == null || m_voltageMagnitude.ID == 0)
            throw new InvalidOperationException("No voltage magnitude input was defined - one voltage magnitude input measurement is required for the power calculator.");

        if (m_currentAngle == null || m_currentAngle.ID == 0)
            throw new InvalidOperationException("No current angle input was defined - one current angle input measurement is required for the power calculator.");

        if (m_currentMagnitude == null || m_currentMagnitude.ID == 0)
            throw new InvalidOperationException("No current magnitude input measurement was defined - one current magnitude input measurement is required for the power calculator.");

        // Make sure only these four phasor measurements are used as input (any others will be ignored)
        InputMeasurementKeys = new[] { m_voltageAngle, m_voltageMagnitude, m_currentAngle, m_currentMagnitude };

        // Validate output measurements
        if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
            throw new InvalidOperationException("Not enough output measurements were specified for the power calculator, expecting measurements for the \"Calculated Power\" and the \"Calculated Reactive Power\" - in this order.");

        if (TrackRecentValues)
        {
            m_powerSample = new List<double>();
            m_reactivePowerSample = new List<double>();
        }

        // Assign a default adapter name to be used if power calculator is loaded as part of automated collection
        if (string.IsNullOrWhiteSpace(Name))
            Name = $"PC!{OutputMeasurements[(int)Output.Power].Key}";
    }

    /// <summary>
    /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
    /// </summary>
    /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
    /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        double power = double.NaN, reactivePower = double.NaN;

        try
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
            bool allValuesReceived = false;

            // Get each needed value from this frame
            if (measurements.TryGetValue(m_voltageMagnitude, out IMeasurement measurement) && measurement.ValueQualityIsGood())
            {
                // Get voltage magnitude value
                voltageMagnitude = measurement.AdjustedValue;

                if (measurements.TryGetValue(m_voltageAngle, out measurement) && measurement.ValueQualityIsGood())
                {
                    // Get voltage angle value
                    voltageAngle = measurement.AdjustedValue;

                    if (measurements.TryGetValue(m_currentMagnitude, out measurement) && measurement.ValueQualityIsGood())
                    {
                        // Get current magnitude value
                        currentMagnitude = measurement.AdjustedValue;

                        if (measurements.TryGetValue(m_currentAngle, out measurement) && measurement.ValueQualityIsGood())
                        {
                            // Get current angle value
                            currentAngle = measurement.AdjustedValue;
                            allValuesReceived = true;
                        }
                    }
                }
            }

            if (!allValuesReceived)
                return;

            double angleDifference = Math.Abs(voltageAngle - currentAngle);

            if (angleDifference > 180)
                angleDifference = 360 - angleDifference;

            // Convert phase angle difference to radians
            double impedancePhaseAngle = Angle.FromDegrees(angleDifference);

            // Calculate line-to-neutral apparent power (S) vector magnitude in Mega volt-amps
            double apparentPower = SqrtOf3 * (Math.Abs(voltageMagnitude) / SI.Mega) * Math.Abs(currentMagnitude);

            // Calculate power (P) and reactive power (Q)
            power = apparentPower * Math.Cos(impedancePhaseAngle);
            reactivePower = apparentPower * Math.Sin(impedancePhaseAngle);

            if (!TrackRecentValues)
                return;

            // Add latest calculated power to data sample
            lock (m_powerSample)
            {
                m_powerSample.Add(power);

                // Maintain sample size
                while (m_powerSample.Count > SampleSize)
                    m_powerSample.RemoveAt(0);
            }

            // Add latest calculated reactive power to data sample
            lock (m_reactivePowerSample)
            {
                m_reactivePowerSample.Add(reactivePower);

                // Maintain sample size
                while (m_reactivePowerSample.Count > SampleSize)
                    m_reactivePowerSample.RemoveAt(0);
            }
        }
        finally
        {

            IMeasurement[] outputMeasurements = OutputMeasurements;
            Measurement powerMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Power], power, frame.Timestamp);
            Measurement stdevMeasurement = Measurement.Clone(outputMeasurements[(int)Output.ReactivePower], reactivePower, frame.Timestamp);

            // Provide calculated measurements for external consumption
            OnNewMeasurements(new IMeasurement[] { powerMeasurement, stdevMeasurement });
        }
    }

    #endregion
}