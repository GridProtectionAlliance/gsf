//******************************************************************************************************
//  AverageFrequency.cs - Gbtc
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
using System.Linq;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations;

/// <summary>
/// Calculates a real-time average frequency reporting the average, maximum and minimum values.
/// </summary>
[Description("Average Frequency: Calculates a real-time average frequency reporting the average, maximum, and minimum values")]
public class AverageFrequency : CalculatedMeasurementBase
{
    #region [ Members ]

    // Constants
    private const double DefaultLowFrequencyThreshold = 57.0D;
    private const double DefaultHighFrequencyThreshold = 62.0D;
    private const bool DefaultReportUnreasonableResultsAsNaN = false;

    // Fields
    private double m_averageFrequency;
    private double m_maximumFrequency;
    private double m_minimumFrequency;
    private readonly ConcurrentDictionary<Guid, Tuple<int, long>> m_lastValues = new();

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
    /// Gets or sets low frequency reasonability threshold, inclusive.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines low frequency reasonability threshold. Value is inclusive, i.e., frequency will be unreasonable at and beyond specified threshold.")]
    [DefaultValue(DefaultLowFrequencyThreshold)]
    public double LowFrequencyThreshold { get; set; }

    /// <summary>
    /// Gets or sets high frequency reasonability threshold, inclusive.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines high frequency reasonability threshold. Value is inclusive, i.e., frequency will be unreasonable at and beyond specified threshold.")]
    [DefaultValue(DefaultHighFrequencyThreshold)]
    public double HighFrequencyThreshold { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if unreasonable results should be reported as NaN.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines flag that determines if unreasonable results should be reported as NaN.")]
    [DefaultValue(DefaultReportUnreasonableResultsAsNaN)]
    public bool ReportUnreasonableResultsAsNaN { get; set; }

    /// <summary>
    /// Returns the detailed status of the <see cref="AverageFrequency"/> calculator.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendFormat("   Low frequency threshold: {0:N3} Hz", LowFrequencyThreshold);
            status.AppendLine();
            status.AppendFormat("  High frequency threshold: {0:N3} Hz", HighFrequencyThreshold);
            status.AppendLine();
            status.AppendFormat("Report unreasonable as NaN: {0}", ReportUnreasonableResultsAsNaN);
            status.AppendLine();
            status.AppendFormat("    Last average frequency: {0:N3} Hz", m_averageFrequency);
            status.AppendLine();
            status.AppendFormat("    Last maximum frequency: {0:N3} Hz", m_maximumFrequency);
            status.AppendLine();
            status.AppendFormat("    Last minimum frequency: {0:N3} Hz", m_minimumFrequency);
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

        Dictionary<string, string> settings = Settings;

        // Get settings
        if (settings.TryGetValue("LowFrequencyThreshold", out string setting) && double.TryParse(setting, out double threshold))
            LowFrequencyThreshold = threshold;
        else
            LowFrequencyThreshold = DefaultLowFrequencyThreshold;

        if (settings.TryGetValue("HighFrequencyThreshold", out setting) && double.TryParse(setting, out threshold))
            HighFrequencyThreshold = threshold;
        else
            HighFrequencyThreshold = DefaultHighFrequencyThreshold;

        ReportUnreasonableResultsAsNaN = settings.TryGetValue("ReportUnreasonableResultsAsNaN", out setting) && setting.ParseBoolean();

        // Validate input measurements
        MeasurementKey[] validInputMeasurementKeys = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.FREQ).ToArray();

        if (validInputMeasurementKeys.Length == 0)
            throw new InvalidOperationException("No valid frequency measurements were specified as inputs to the average frequency calculator.");

        // Make sure only frequencies are used as input
        InputMeasurementKeys = validInputMeasurementKeys;

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
            double total = 0.0D;
            double maximumFrequency = LowFrequencyThreshold;
            double minimumFrequency = HighFrequencyThreshold;
            int count = 0;

            foreach (IMeasurement measurement in frame.Measurements.Values)
            {
                double frequency = measurement.AdjustedValue;

                // Do some simple flat line avoidance, validating at three decimal places...
                int truncatedFrequency = (int)(frequency * 1000.0D);

                Tuple<int, long> lastValues = m_lastValues.GetOrAdd(measurement.ID, _ => new Tuple<int, long>(truncatedFrequency, 0L));
                long flatLineCount = lastValues.Item2;

                if (lastValues.Item1 == truncatedFrequency)
                {
                    flatLineCount++;

                    // If value remains the same more than 4 times, assume flat-line value
                    if (flatLineCount > 4L)
                        frequency = double.NaN;
                }
                else
                {
                    flatLineCount = 0L;
                }

                m_lastValues[measurement.ID] = new Tuple<int, long>(truncatedFrequency, flatLineCount);

                // Validate frequency
                if (double.IsNaN(frequency) || !(frequency > LowFrequencyThreshold) || !(frequency < HighFrequencyThreshold))
                    continue;

                total += frequency;

                if (frequency > maximumFrequency)
                    maximumFrequency = frequency;

                if (frequency < minimumFrequency)
                    minimumFrequency = frequency;

                count++;
            }

            if (count > 0)
            {
                m_averageFrequency = total / count;
                m_maximumFrequency = maximumFrequency;
                m_minimumFrequency = minimumFrequency;
            }

            if (ReportUnreasonableResultsAsNaN)
            {
                if (m_averageFrequency <= LowFrequencyThreshold || m_averageFrequency >= HighFrequencyThreshold)
                    m_averageFrequency = double.NaN;

                if (m_maximumFrequency <= LowFrequencyThreshold || m_maximumFrequency >= HighFrequencyThreshold)
                    m_maximumFrequency = double.NaN;

                if (m_minimumFrequency <= LowFrequencyThreshold || m_minimumFrequency >= HighFrequencyThreshold)
                    m_minimumFrequency = double.NaN;
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