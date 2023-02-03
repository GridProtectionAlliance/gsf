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

namespace PowerCalculations.EventDetection;

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

    private const double DefaultEstimateTriggerThreshold = 0.0256D;
    private const int DefaultConsecutiveDetections = 2;
    private const int DefaultMinimumValidChannels = 3;
    private const double DefaultPowerEstimateRatio = 19530.0D;
    private const int DefaultMinimumAlarmInterval = 20;

    private const int AnalysisWindowSizeFactor = 4;

    // Fields
    private int m_alarmProhibitCounter;         // Counter to prevent duplicated alarms
    private List<double> m_frequencies;         // Frequency measurement values
    private List<DateTime> m_timeStamps;        // Timestamps of frequencies
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
    [ConnectionStringParameter]
    [Description("Define the threshold for detecting an abnormal excursion in frequency.")]
    [DefaultValue(DefaultEstimateTriggerThreshold)]
    public double EstimateTriggerThreshold { get; set; } = DefaultEstimateTriggerThreshold;

    /// <summary>
    /// Gets or sets the number of frames to be analyzed at any given time.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the number of frames to be analyzed at any given time. The default value is 4 times the frame-rate defined in the connection string for this Frequency Excursion.")]
    public int AnalysisWindowSize { get; set; }

    /// <summary>
    /// Gets or sets the interval between adjacent calculations.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the interval between adjacent calculations. The default value is the frame-rate defined in the connection string for this Frequency Excursion.")]
    public int AnalysisInterval { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of consecutive excursions needed in order to trip the alarm.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the minimum number of consecutive excursions needed in order to trip the alarm.")]
    [DefaultValue(DefaultConsecutiveDetections)]
    public int ConsecutiveDetections { get; set; } = DefaultConsecutiveDetections;

    /// <summary>
    /// Gets or sets the minimum frequency values needed to perform a valid calculation.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the minimum frequency values needed to perform a valid calculation.")]
    [DefaultValue(DefaultMinimumValidChannels)]
    public int MinimumValidChannels { get; set; } = DefaultMinimumValidChannels;

    /// <summary>
    /// Gets or sets the ratio used to calculate the total estimated MW change from frequency.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the ratio used to calculate the total estimated MW change from frequency.")]
    [DefaultValue(DefaultPowerEstimateRatio)]
    public double PowerEstimateRatio { get; set; } = DefaultPowerEstimateRatio;

    /// <summary>
    /// Gets or sets the period, in seconds, used to prevent duplicate alarms.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the period, in seconds, used to prevent duplicate alarms.")]
    [DefaultValue(DefaultMinimumAlarmInterval)]
    public int MinimumAlarmInterval { get; set; } = DefaultMinimumAlarmInterval;

    private int AlarmProhibitPeriod => MinimumAlarmInterval * FramesPerSecond;

    /// <summary>
    /// Returns the detailed status of the <see cref="FrequencyExcursion"/> detector.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendLine($"Estimate Trigger Threshold: {EstimateTriggerThreshold:N3}");
            status.AppendLine($"      Analysis Window Size: {AnalysisWindowSize:N0}");
            status.AppendLine($"         Analysis Interval: {AnalysisInterval:N0}");
            status.AppendLine($"   Detections before Alarm: {ConsecutiveDetections:N0}");
            status.AppendLine($" Minimum Valid Frequencies: {MinimumValidChannels:N0}");
            status.AppendLine($"      Power Estimate Ratio: {PowerEstimateRatio:N3}MW");
            status.AppendLine($"    Minimum Alarm Interval: {MinimumAlarmInterval:N0} seconds");

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

        //  <Parameters paraName="EstimateTriggerThreshold" value="0.0256" description="The threshold of estimation trigger"></Parameters>
        //  <Parameters paraName="AnalysisWindowSize" value="120" description="The sample size of the analysis window"></Parameters>
        //  <Parameters paraName="AnalysisInterval" value="30" description="The frame interval between two adjacent frequency testing"></Parameters>
        //  <Parameters paraName="ConsecutiveDetections" value="2" description="Number of needed consecutive detections before positive alarm"></Parameters>
        //  <Parameters paraName="MinimumValidChannel" value="3" description="Minimum valid channel for conduction the frequency testing"></Parameters>
        //  <Parameters paraName="PowerEstimateRatio" value="19530.00" description="The ratio of total amount of generator (load) trip over the frequency excursion"></Parameters>
        //  <Parameters paraName="MinimumAlarmInterval" value="20" description="Minimum duration between alarms, in whole seconds"></Parameters>

        // Load required parameters
        if (settings.TryGetValue(nameof(EstimateTriggerThreshold), out string setting) && double.TryParse(setting, out double result))
            EstimateTriggerThreshold = result;

        if (settings.TryGetValue(nameof(AnalysisWindowSize), out setting) && int.TryParse(setting, out int value))
            AnalysisWindowSize = value;
        else
            AnalysisWindowSize = AnalysisWindowSizeFactor * FramesPerSecond;

        if (settings.TryGetValue(nameof(AnalysisInterval), out setting) && int.TryParse(setting, out value))
            AnalysisInterval = value;
        else
            AnalysisInterval = FramesPerSecond;

        if (settings.TryGetValue(nameof(ConsecutiveDetections), out setting) && int.TryParse(setting, out value))
            ConsecutiveDetections = value;

        if (settings.TryGetValue(nameof(MinimumValidChannels), out setting) && int.TryParse(setting, out value))
            MinimumValidChannels = value;

        if (settings.TryGetValue(nameof(PowerEstimateRatio), out setting) && double.TryParse(setting, out result))
            PowerEstimateRatio = result;

        if (settings.TryGetValue(nameof(MinimumAlarmInterval), out setting) && int.TryParse(setting, out value))
            MinimumAlarmInterval = value;

        m_frequencies = new List<double>();
        m_timeStamps = new List<DateTime>();

        // Validate input measurements
        MeasurementKey[] validInputMeasurementKeys = InputMeasurementKeys.Where((_, index) => InputMeasurementKeyTypes[index] == SignalType.FREQ).ToArray();

        if (validInputMeasurementKeys.Length == 0)
            throw new InvalidOperationException("No valid frequency measurements were specified as inputs to the frequency excursion detector.");

        if (validInputMeasurementKeys.Length < MinimumValidChannels)
            throw new InvalidOperationException($"Minimum valid frequency measurements (i.e., \"minimumValidChannels\") for the frequency excursion detector is currently set to {MinimumValidChannels}, only {validInputMeasurementKeys.Length} {(validInputMeasurementKeys.Length == 1 ? "was" : "were")} defined.");

        // Make sure only frequencies are used as input
        InputMeasurementKeys = validInputMeasurementKeys;

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
        while (m_frequencies.Count > AnalysisWindowSize)
        {
            m_frequencies.RemoveAt(0);
            m_timeStamps.RemoveAt(0);
        }

        if (m_count % AnalysisInterval != 0 || m_frequencies.Count != AnalysisWindowSize)
            return;

        double frequency1 = m_frequencies[0];
        double frequency2 = m_frequencies[AnalysisWindowSize - 1];
        double frequencyDelta = 0.0D, estimatedSize = 0.0D;
        ExcursionType typeofExcursion = ExcursionType.GenerationTrip;
        bool warningSignaled = false;

        if (!double.IsNaN(frequency1) && !double.IsNaN(frequency2))
        {
            frequencyDelta = frequency2 - frequency1;

            if (Math.Abs(frequencyDelta) > EstimateTriggerThreshold)
                m_detectedExcursions++;
            else
                m_detectedExcursions = 0;

            if (m_detectedExcursions >= ConsecutiveDetections)
            {
                typeofExcursion = (frequency1 > frequency2 ? ExcursionType.GenerationTrip : ExcursionType.LoadTrip);
                estimatedSize = Math.Abs(frequencyDelta) * PowerEstimateRatio;

                // Display frequency excursion detection warning
                if (m_alarmProhibitCounter == 0)
                {
                    OutputFrequencyWarning(m_timeStamps[0], frequencyDelta, typeofExcursion, estimatedSize);
                    m_alarmProhibitCounter = AlarmProhibitPeriod;
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

    private void OutputFrequencyWarning(DateTime timestamp, double delta, ExcursionType typeOfExcursion, double totalAmount) =>
        OnStatusMessage(MessageLevel.Info,
            $"Frequency excursion detected!{Environment.NewLine}" + 
            $"              Time = {timestamp:dd-MMM-yyyy HH:mm:ss.fff}{Environment.NewLine}" + 
            $"             Delta = {delta}{Environment.NewLine}" + 
            $"              Type = {typeOfExcursion}{Environment.NewLine}" + 
            $"    Estimated Size = {totalAmount:0.00}MW{Environment.NewLine}");

    #endregion
}