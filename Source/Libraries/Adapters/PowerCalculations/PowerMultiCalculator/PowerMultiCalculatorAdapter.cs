//******************************************************************************************************
//  PowerMultiCalculatorAdapter.cs - Gbtc
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
//  11/02/2015 - Ryan McCoy
//       Generated original version of source code.
//  12/02/2015 - J. Ritchie Carroll
//       Updated power calculations to use common Power functions.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;

namespace PowerCalculations.PowerMultiCalculator;

/// <summary>
/// Performs MW, MVA, and MVAR calculations based on current and voltage phasors input to the adapter
/// </summary>
[Description("PowerMultiCalculatorAdapter: Performs MW, MVAR and MVA calculations based on current and voltage phasors input to the adapter")]
public class PowerMultiCalculatorAdapter : ActionAdapterBase
{
    #region [ Members ]

    // Constants
    internal const bool DefaultTrackRecentValues = false;
    private const string DefaultTableName = "PowerCalculation";
    private const bool DefaultAlwaysProduceResult = false;
    private const VoltageAdjustmentStrategy DefaultAdjustmentStrategy = VoltageAdjustmentStrategy.LineToNeutral;
    private const bool DefaultEnableTemporalProcessing = false;
    private const BadDataStrategy DefaultBadDataStrategy = BadDataStrategy.FlagAsBad;

    private const double SqrtOf3 = 1.7320508075688772935274463415059D;
    private const int ValuesToTrack = 5;

    // Fields
    private PowerCalculation[] m_configuredCalculations;
    private Dictionary<MeasurementKey, VoltageAdjustmentStrategy> m_adjustmentStrategies;
    private RunningAverage m_averageCalculationsPerFrame;
    private RunningAverage m_averageCalculationTime;
    private RunningAverage m_averageTotalCalculationTime;
    private ConcurrentQueue<IMeasurement> m_lastActivePowerCalculations;
    private ConcurrentQueue<IMeasurement> m_lastReactivePowerCalculations;
    private ConcurrentQueue<IMeasurement> m_lastApparentPowerCalculations;
    private double m_lastTotalCalculationTime;
    private int m_lastTotalCalculations;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates the adapter
    /// </summary>
    public PowerMultiCalculatorAdapter()
    {
        try
        {
            // Validate that data operation and adapter instance exist within database
            PowerCalculationConfigurationValidation.ValidateDatabaseDefinitions();
        }
        catch
        {
            // This should never cause unhanded exception
        }
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
    /// Gets or sets the name of the table this adapter will use to obtain its metadata.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the name of the table this adapter will use to obtain its metadata.")]
    [DefaultValue(DefaultTableName)]
    public string TableName { get; set; } = DefaultTableName;

    /// <summary>
    /// Gets or sets flag indicating whether or not this adapter will produce a result for all calculations. If this value is true and a calculation fails,
    /// the adapter will produce NaN for that calculation. If this value is false and a calculation fails, the adapter will not produce any result.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines flag that determines if adapter should always produce a result. When true, adapter will produce NaN for calculations that fail.")]
    [DefaultValue(DefaultAlwaysProduceResult)]
    public bool AlwaysProduceResult { get; set; } = DefaultAlwaysProduceResult;

    /// <summary>
    /// Gets or sets the default strategy used to adjust voltage values for based on the nature of the voltage measurements.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines default strategy used to adjust voltage values for based on the nature of the voltage measurements.")]
    [DefaultValue(DefaultAdjustmentStrategy)]
    public VoltageAdjustmentStrategy AdjustmentStrategy { get; set; } = DefaultAdjustmentStrategy;

    /// <summary>
    /// Gets or sets flag that determines if adapter should enable temporal processing support.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines flag that determines if adapter should enable temporal processing support.")]
    [DefaultValue(DefaultEnableTemporalProcessing)]
    public bool EnableTemporalProcessing { get; set; } = DefaultEnableTemporalProcessing;

    /// <summary>
    /// Gets or sets SI units factor to use for power calculations, defaults to Mega (10^6).
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines SI units factor to use for power calculations, defaults to Mega (10^6).")]
    [DefaultValue(SI.Mega)]
    public double SIUnitsFactor { get; set; } = SI.Mega;

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
    public override bool SupportsTemporalProcessing => EnableTemporalProcessing;

    /// <summary>
    /// Returns the adapter status, including real-time statistics about adapter operation
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();
            VoltageAdjustmentStrategy[] strategies = m_adjustmentStrategies?.Values.ToArray() ?? Array.Empty<VoltageAdjustmentStrategy>();

            status.Append(base.Status);

            status.AppendLine($"     SI Factor for Outputs: {SI.ToScaledString(SIUnitsFactor, 0, "W")} ({SIUnitsFactor})");
            status.AppendLine($"Default Voltage Adjustment: {AdjustmentStrategy}");
            status.AppendLine($"         Bad Data Strategy: {BadDataStrategy}");
            status.AppendLine($"       Temporal Processing: {(EnableTemporalProcessing ? "Enabled" : "Disabled")}");
            status.AppendLine($"   Per-Circuit Adjustments: {strategies.Length:N0}");

            if (strategies.Length > 0)
            {
                int strategyCount(params VoltageAdjustmentStrategy[] targetStrategies) =>
                    strategies.Count(targetStrategies.Contains);

                status.AppendLine("      -- Totals per Adjustment Strategy --");
                status.AppendLine($"             Est. 3-Phase Line-to-Line    : {strategyCount(VoltageAdjustmentStrategy.LineToLine):N0}");
                status.AppendLine($"             Est. 3-Phase Line-to-Neutral : {strategyCount(VoltageAdjustmentStrategy.LineToNeutral):N0}");
                status.AppendLine($"             1-Phase Line-to-Line         : {strategyCount(VoltageAdjustmentStrategy.LineToLineSinglePhase):N0}");
                status.AppendLine($"             1-Phase Line-to-Neutral/None : {strategyCount(VoltageAdjustmentStrategy.LineToNeutralSinglePhase, VoltageAdjustmentStrategy.None):N0}");
            }

            status.AppendLine($"               Total Calcs: {m_lastTotalCalculations:N0} for last frame");
            status.AppendLine($"       Average Total Calcs: {Math.Round(m_averageCalculationsPerFrame.Average):N3} per frame");

            lock (m_averageCalculationTime)
                status.AppendLine($"         Average Calc Time: {m_averageCalculationTime.Average:N3} ms per V/I phasor pair");

            status.AppendLine($"           Total Calc Time: {m_lastTotalCalculationTime:N3} ms for last frame");
            status.AppendLine($"   Average Total Calc Time: {m_averageTotalCalculationTime.Average:N3} ms per frame");

            if (!TrackRecentValues)
            {
                status.AppendLine();
                return status.ToString();
            }

            status.AppendLine("   Last Active Power Measurements:");

            if (m_lastActivePowerCalculations.Any())
            {
                foreach (IMeasurement measurement in m_lastActivePowerCalculations)
                    status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
            }
            else
            {
                status.AppendLine("\tNot enough values calculated yet...");
            }

            status.AppendLine("   Last Reactive Power Measurements:");

            if (m_lastReactivePowerCalculations.Any())
            {
                foreach (IMeasurement measurement in m_lastReactivePowerCalculations)
                    status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
            }
            else
            {
                status.AppendLine("\tNot enough values calculated yet...");
            }

            status.AppendLine("   Last Apparent Power Measurements:");

            if (m_lastApparentPowerCalculations.Any())
            {
                foreach (IMeasurement measurement in m_lastApparentPowerCalculations)
                    status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
            }
            else
            {
                status.AppendLine("\tNot enough values calculated yet...");
            }

            status.AppendLine();
            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Loads configuration from the database and configures adapter to run with that configuration
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        HashSet<IMeasurement> outputMeasurements = new();
        List<PowerCalculation> configuredCalculations = new();

        m_adjustmentStrategies = new Dictionary<MeasurementKey, VoltageAdjustmentStrategy>();
        m_averageCalculationsPerFrame = new RunningAverage();
        m_averageCalculationTime = new RunningAverage();
        m_averageTotalCalculationTime = new RunningAverage();

        if (Settings.TryGetValue(nameof(TableName), out string tableName))
            TableName = tableName;

        //                     0           1                    2                     3                   4                     5
        string query = "SELECT ID, CircuitDescription, VoltageAngleSignalID, VoltageMagSignalID, CurrentAngleSignalID, CurrentMagSignalID, " +
                       //            6                          7                           8
                       "ActivePowerOutputSignalID, ReactivePowerOutputSignalID, ApparentPowerOutputSignalID " +
                       $"FROM {TableName} WHERE NodeId = {{0}} AND Enabled <> 0";

        using (AdoDataConnection database = new("systemSettings"))
        using (IDataReader reader = database.ExecuteReader(query, ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>()))
        {
            while (reader.Read())
            {
                configuredCalculations.Add(new PowerCalculation
                {
                    PowerCalculationID = reader.GetInt32(0),
                    CircuitDescription = reader.GetString(1),
                    VoltageAngleMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(reader[2].ToString())),
                    VoltageMagnitudeMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(reader[3].ToString())),
                    CurrentAngleMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(reader[4].ToString())),
                    CurrentMagnitudeMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(reader[5].ToString())),
                    ActivePowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(reader[6].ToString()), outputMeasurements),
                    ReactivePowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(reader[7].ToString()), outputMeasurements),
                    ApparentPowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(reader[8].ToString()), outputMeasurements)
                });
            }
        }

        m_configuredCalculations = configuredCalculations.ToArray();

        if (m_configuredCalculations.Length > 0)
        {
            InputMeasurementKeys = m_configuredCalculations.SelectMany(calculation => new[]
            {
                calculation.CurrentAngleMeasurementKey,
                calculation.CurrentMagnitudeMeasurementKey,
                calculation.VoltageAngleMeasurementKey,
                calculation.VoltageMagnitudeMeasurementKey
            })
            .ToArray();
        }
        else
        {
            throw new InvalidOperationException("Skipped initialization of power calculator: no defined power calculations...");
        }

        if (outputMeasurements.Any())
            OutputMeasurements = outputMeasurements.ToArray();

        Dictionary<string, string> settings = Settings;

        // Load parameters
        if (settings.TryGetValue(nameof(TrackRecentValues), out string setting))
            TrackRecentValues = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AlwaysProduceResult), out setting))
            AlwaysProduceResult = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AdjustmentStrategy), out setting) && Enum.TryParse(setting, true, out VoltageAdjustmentStrategy adjustmentStrategy))
            AdjustmentStrategy = adjustmentStrategy;

        if (settings.TryGetValue(nameof(EnableTemporalProcessing), out setting))
            EnableTemporalProcessing = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(SIUnitsFactor), out setting) && double.TryParse(setting, out double factor))
            SIUnitsFactor = factor;

        if (settings.TryGetValue(nameof(BadDataStrategy), out setting) && Enum.TryParse(setting, true, out BadDataStrategy badDataStrategy))
            BadDataStrategy = badDataStrategy;

        if (TrackRecentValues)
        {
            m_lastActivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastReactivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastApparentPowerCalculations = new ConcurrentQueue<IMeasurement>();
        }

        // Define per power calculation line adjustment strategies
        foreach (PowerCalculation powerCalculation in m_configuredCalculations)
        {
            if (powerCalculation.VoltageMagnitudeMeasurementKey is null || string.IsNullOrWhiteSpace(powerCalculation.CircuitDescription))
                continue;

            try
            {
                Dictionary<string, string> circuitSettings = powerCalculation.CircuitDescription.ParseKeyValuePairs();

                if (circuitSettings.TryGetValue(nameof(AdjustmentStrategy), out setting) && Enum.TryParse(setting, true, out adjustmentStrategy))
                    m_adjustmentStrategies[powerCalculation.VoltageMagnitudeMeasurementKey] = adjustmentStrategy;
            }
            catch (Exception ex)
            {
                OnStatusMessage(MessageLevel.Warning, $"Failed to parse settings from circuit description \"{powerCalculation.CircuitDescription}\": {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Calculates MW, MVAR and MVA then publishes those measurements.
    /// </summary>
    /// <param name="frame">Input values for calculation</param>
    /// <param name="index">Index of frame within second.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        long frameCalculationStartTime = DateTime.UtcNow.Ticks;
        ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
        int calculations = 0;

        Parallel.ForEach(m_configuredCalculations, powerCalculation =>
        {
            try
            {
                long powerCalculationStartTime = DateTime.UtcNow.Ticks;
                bool calculateActivePower = powerCalculation.ActivePowerOutputMeasurement is not null;
                bool calculateReactivePower = powerCalculation.ReactivePowerOutputMeasurement is not null;
                bool calculateApparentPower = powerCalculation.ApparentPowerOutputMeasurement is not null;
                double activePower = double.NaN, reactivePower = double.NaN, apparentPower = double.NaN;
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
                    double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
                    bool allInputsReceived = false;

                    if (measurements.TryGetValue(powerCalculation.VoltageMagnitudeMeasurementKey, out IMeasurement measurement) && includeInput(measurement))
                    {
                        voltageMagnitude = measurement.AdjustedValue;

                        if (!m_adjustmentStrategies.TryGetValue(powerCalculation.VoltageMagnitudeMeasurementKey, out VoltageAdjustmentStrategy adjustmentStrategy))
                            adjustmentStrategy = AdjustmentStrategy;

                        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                        switch (adjustmentStrategy)
                        {
                            case VoltageAdjustmentStrategy.LineToNeutral:
                                voltageMagnitude *= 3.0D;
                                break;

                            case VoltageAdjustmentStrategy.LineToLine:
                                voltageMagnitude *= SqrtOf3;
                                break;

                            case VoltageAdjustmentStrategy.LineToLineSinglePhase:
                                voltageMagnitude /= SqrtOf3;
                                break;
                        }

                        if (measurements.TryGetValue(powerCalculation.VoltageAngleMeasurementKey, out measurement) && includeInput(measurement))
                        {
                            voltageAngle = measurement.AdjustedValue;

                            if (measurements.TryGetValue(powerCalculation.CurrentMagnitudeMeasurementKey, out measurement) && includeInput(measurement))
                            {
                                currentMagnitude = measurement.AdjustedValue;

                                if (measurements.TryGetValue(powerCalculation.CurrentAngleMeasurementKey, out measurement) && includeInput(measurement))
                                {
                                    currentAngle = measurement.AdjustedValue;
                                    allInputsReceived = true;
                                }
                            }
                        }
                    }

                    if (allInputsReceived)
                    {
                        // Calculate power (P), reactive power (Q) and apparent power (|S|)
                        Phasor voltage = new(PhasorType.Voltage, Angle.FromDegrees(voltageAngle), voltageMagnitude);
                        Phasor current = new(PhasorType.Current, Angle.FromDegrees(currentAngle), currentMagnitude);

                        if (calculateActivePower)
                            activePower = (double)Phasor.CalculateActivePower(voltage, current) / SIUnitsFactor;

                        if (calculateReactivePower)
                            reactivePower = (double)Phasor.CalculateReactivePower(voltage, current) / SIUnitsFactor;

                        if (calculateApparentPower)
                            apparentPower = (double)Phasor.CalculateApparentPower(voltage, current) / SIUnitsFactor;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, ex);
                }
                finally
                {
                    List<IMeasurement> outputMeasurements = new(3);
                    MeasurementStateFlags flags = badInputDetected ? MeasurementStateFlags.BadData : MeasurementStateFlags.Normal;

                    if (calculateActivePower)
                    {
                        Measurement activePowerMeasurement = Measurement.Clone(powerCalculation.ActivePowerOutputMeasurement, activePower, frame.Timestamp, flags);

                        if (AlwaysProduceResult || !double.IsNaN(activePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(activePowerMeasurement);
                            calculations++;

                            if (TrackRecentValues)
                            {
                                m_lastActivePowerCalculations.Enqueue(activePowerMeasurement);

                                while (m_lastActivePowerCalculations.Count > ValuesToTrack)
                                    m_lastActivePowerCalculations.TryDequeue(out _);
                            }
                        }
                    }

                    if (calculateReactivePower)
                    {
                        Measurement reactivePowerMeasurement = Measurement.Clone(powerCalculation.ReactivePowerOutputMeasurement, reactivePower, frame.Timestamp, flags);

                        if (AlwaysProduceResult || !double.IsNaN(reactivePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(reactivePowerMeasurement);
                            calculations++;

                            if (TrackRecentValues)
                            {
                                m_lastReactivePowerCalculations.Enqueue(reactivePowerMeasurement);

                                while (m_lastReactivePowerCalculations.Count > ValuesToTrack)
                                    m_lastReactivePowerCalculations.TryDequeue(out _);
                            }
                        }
                    }

                    if (calculateApparentPower)
                    {
                        Measurement apparentPowerMeasurement = Measurement.Clone(powerCalculation.ApparentPowerOutputMeasurement, apparentPower, frame.Timestamp, flags);

                        if (AlwaysProduceResult || !double.IsNaN(apparentPowerMeasurement.Value))
                        {
                            outputMeasurements.Add(apparentPowerMeasurement);
                            calculations++;

                            if (TrackRecentValues)
                            {
                                m_lastApparentPowerCalculations.Enqueue(apparentPowerMeasurement);

                                while (m_lastApparentPowerCalculations.Count > ValuesToTrack)
                                    m_lastApparentPowerCalculations.TryDequeue(out _);
                            }
                        }
                    }

                    OnNewMeasurements(outputMeasurements);

                    lock (m_averageCalculationTime)
                        m_averageCalculationTime.AddValue(new Ticks(DateTime.UtcNow.Ticks - powerCalculationStartTime).ToMilliseconds());
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Failed to calculate power for {powerCalculation.CircuitDescription}: {ex.Message}", ex));
            }
        });

        m_lastTotalCalculations = calculations;
        m_averageCalculationsPerFrame.AddValue(calculations);

        m_lastTotalCalculationTime = new Ticks(DateTime.UtcNow.Ticks - frameCalculationStartTime).ToMilliseconds();
        m_averageTotalCalculationTime.AddValue(m_lastTotalCalculationTime);
    }

    #endregion

    #region [ Static ]

    // Static Methods
    private static Measurement AddOutputMeasurement(Guid signalID, HashSet<IMeasurement> outputMeasurements)
    {
        MeasurementKey key = MeasurementKey.LookUpBySignalID(signalID);

        if (key.SignalID == Guid.Empty)
            return null;

        Measurement measurement = new() { Metadata = key.Metadata };
        outputMeasurements.Add(measurement);

        return measurement;
    }

    #endregion
}