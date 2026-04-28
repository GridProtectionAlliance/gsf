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
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;

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
    private const bool DefaultEnablePowerFactorCalculation = false;
    private const PowerFactorSignConvention DefaultPowerFactorSignConvention = PowerFactorSignConvention.Unsigned;

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
    private ConcurrentQueue<IMeasurement> m_lastPowerFactorCalculations;
    private ManualResetEventSlim m_configurationReloaded;
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
    /// Gets or sets flag indicating whether this adapter will produce a result for all calculations. If this value is true and a calculation fails,
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
    /// Gets or sets flag that determines if a power factor output measurement should be calculated for each circuit.
    /// </summary>
    /// <remarks>
    /// When enabled, a power factor output measurement will be auto-created (per circuit) using the
    /// <see cref="MetadataHelpers"/> APIs if one is not already present in active configuration. The naming
    /// convention is the existing active power (MW) point tag with the trailing power suffix replaced by <c>PF</c>.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines flag that determines if power factor calculations should be added to the outputs. When enabled, output measurements are auto-created (per circuit) if not already present in active configuration.")]
    [DefaultValue(DefaultEnablePowerFactorCalculation)]
    public bool EnablePowerFactorCalculation { get; set; } = DefaultEnablePowerFactorCalculation;

    /// <summary>
    /// Gets or sets the sign convention used for published power factor values.
    /// </summary>
    /// <remarks>
    /// Only relevant when <see cref="EnablePowerFactorCalculation"/> is <c>true</c>.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines sign convention used for published power factor values. Only used when EnablePowerFactorCalculation is true.")]
    [DefaultValue(DefaultPowerFactorSignConvention)]
    public PowerFactorSignConvention PowerFactorSignConvention { get; set; } = DefaultPowerFactorSignConvention;

    /// <summary>
    /// Gets the flag indicating if this adapter supports temporal processing.
    /// </summary>
    public override bool SupportsTemporalProcessing => EnableTemporalProcessing;

    /// <summary>
    /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="PowerMultiCalculatorAdapter"/>.
    /// </summary>
    public override DataSet DataSource
    {
        get => base.DataSource;
        set
        {
            base.DataSource = value;

            // Notify any pending configuration reload waiters that active configuration has been refreshed
            m_configurationReloaded?.Set();
        }
    }

    /// <summary>
    /// Returns the adapter status, including real-time statistics about adapter operation
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();
            VoltageAdjustmentStrategy[] strategies = m_adjustmentStrategies?.Values.ToArray() ?? [];

            status.Append(base.Status);

            status.AppendLine($"     SI Factor for Outputs: {SI.ToScaledString(SIUnitsFactor, 0, "W")} ({SIUnitsFactor})");
            status.AppendLine($"Default Voltage Adjustment: {AdjustmentStrategy}");
            status.AppendLine($"         Bad Data Strategy: {BadDataStrategy}");
            status.AppendLine($"       Temporal Processing: {(EnableTemporalProcessing ? "Enabled" : "Disabled")}");
            status.AppendLine($"  Power Factor Calculation: {(EnablePowerFactorCalculation ? $"Enabled ({PowerFactorSignConvention})" : "Disabled")}");
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

            if (EnablePowerFactorCalculation)
            {
                status.AppendLine("   Last Power Factor Measurements:");

                if (m_lastPowerFactorCalculations is not null && m_lastPowerFactorCalculations.Any())
                {
                    foreach (IMeasurement measurement in m_lastPowerFactorCalculations)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N4}");
                }
                else
                {
                    status.AppendLine("\tNot enough values calculated yet...");
                }
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
        // Allocate configuration-reload wait handle ahead of base.Initialize() so any DataSource
        // assignment that fires during initialization is observed.
        m_configurationReloaded ??= new ManualResetEventSlim();

        base.Initialize();

        HashSet<IMeasurement> outputMeasurements = [];
        List<PowerCalculation> configuredCalculations = [];

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

        if (settings.TryGetValue(nameof(EnablePowerFactorCalculation), out setting))
            EnablePowerFactorCalculation = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(PowerFactorSignConvention), out setting) && Enum.TryParse(setting, true, out PowerFactorSignConvention pfSignConvention))
            PowerFactorSignConvention = pfSignConvention;

        // Auto-create power factor output measurements when enabled. Soft-fails so a misconfigured
        // circuit does not prevent the rest of the adapter from running.
        CreatePowerFactorOutputs();

        if (TrackRecentValues)
        {
            m_lastActivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastReactivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastApparentPowerCalculations = new ConcurrentQueue<IMeasurement>();

            if (EnablePowerFactorCalculation)
                m_lastPowerFactorCalculations = new ConcurrentQueue<IMeasurement>();
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
        ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
        AsyncDoubleBufferedQueue<IMeasurement> publicationBuffer = new() { ProcessItemsFunction = OnNewMeasurements };
        long frameCalculationStartTime = DateTime.UtcNow.Ticks;
        int calculations = 0;

        Parallel.ForEach(m_configuredCalculations, powerCalculation =>
        {
            try
            {
                long powerCalculationStartTime = DateTime.UtcNow.Ticks;
                bool calculateActivePower = powerCalculation.ActivePowerOutputMeasurement is not null;
                bool calculateReactivePower = powerCalculation.ReactivePowerOutputMeasurement is not null;
                bool calculateApparentPower = powerCalculation.ApparentPowerOutputMeasurement is not null;
                bool calculatePowerFactor = EnablePowerFactorCalculation && powerCalculation.PowerFactorOutputMeasurement is not null;
                double activePower = double.NaN, reactivePower = double.NaN, apparentPower = double.NaN, powerFactor = double.NaN;
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

                        if (!calculatePowerFactor)
                            return;
                        
                        // Compute P, |S| and (when needed) Q regardless of whether the configured outputs were
                        // requested, SI scaling cancels out, so we reuse already-computed values when available:
                        double p = calculateActivePower ? activePower : (double)Phasor.CalculateActivePower(voltage, current) / SIUnitsFactor;
                        double s = calculateApparentPower ? apparentPower : (double)Phasor.CalculateApparentPower(voltage, current) / SIUnitsFactor;

                        if (s <= 0.0D)
                            return;
                        
                        double magnitude = Math.Abs(p) / s;

                        if (PowerFactorSignConvention == PowerFactorSignConvention.Unsigned)
                        {
                            powerFactor = magnitude;
                        }
                        else
                        {
                            double q = calculateReactivePower ? reactivePower : (double)Phasor.CalculateReactivePower(voltage, current) / SIUnitsFactor;
                            int sign = Math.Sign(q);

                            if (PowerFactorSignConvention == PowerFactorSignConvention.Leading)
                                sign = -sign;

                            // sign == 0 (Q exactly zero) -> unity power factor; preserve positive magnitude
                            powerFactor = sign == 0 ? magnitude : sign * magnitude;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, ex);
                }
                finally
                {
                    List<IMeasurement> outputMeasurements = new(4);
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

                    if (calculatePowerFactor)
                    {
                        Measurement powerFactorMeasurement = Measurement.Clone(powerCalculation.PowerFactorOutputMeasurement, powerFactor, frame.Timestamp, flags);

                        if (AlwaysProduceResult || !double.IsNaN(powerFactorMeasurement.Value))
                        {
                            outputMeasurements.Add(powerFactorMeasurement);
                            calculations++;

                            if (TrackRecentValues && m_lastPowerFactorCalculations is not null)
                            {
                                m_lastPowerFactorCalculations.Enqueue(powerFactorMeasurement);

                                while (m_lastPowerFactorCalculations.Count > ValuesToTrack)
                                    m_lastPowerFactorCalculations.TryDequeue(out _);
                            }
                        }
                    }

                    publicationBuffer.Enqueue(outputMeasurements);

                    lock (m_averageCalculationTime)
                        m_averageCalculationTime.AddValue(new Ticks(DateTime.UtcNow.Ticks - powerCalculationStartTime).ToMilliseconds());
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Failed to calculate power for {powerCalculation.CircuitDescription}: {ex.Message}", ex));
            }
        });

        // Wait for all power calculations to publish
        SpinWait.SpinUntil(() => publicationBuffer.Count == 0);

        m_lastTotalCalculations = calculations;
        m_averageCalculationsPerFrame.AddValue(calculations);

        m_lastTotalCalculationTime = new Ticks(DateTime.UtcNow.Ticks - frameCalculationStartTime).ToMilliseconds();
        m_averageTotalCalculationTime.AddValue(m_lastTotalCalculationTime);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="PowerMultiCalculatorAdapter"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (!disposing)
                return;

            ManualResetEventSlim configurationReloaded = m_configurationReloaded;
            m_configurationReloaded = null;
            configurationReloaded?.Set();
            configurationReloaded?.Dispose();
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Validates power factor output measurements when <see cref="EnablePowerFactorCalculation"/> is enabled,
    /// auto-creating any missing per-circuit measurements via <see cref="MetadataHelpers"/>.
    /// </summary>
    /// <remarks>
    /// Soft-fails: any per-circuit lookup or creation issue is logged as a warning, leaves the circuit's
    /// <see cref="PowerCalculation.PowerFactorOutputMeasurement"/> as <c>null</c>, and skips PF publication
    /// for that circuit only.
    /// </remarks>
    private void CreatePowerFactorOutputs()
    {
        if (!EnablePowerFactorCalculation || m_configuredCalculations is null || m_configuredCalculations.Length == 0)
            return;

        List<Guid> newSignalIDs = [];
        bool recordsAdded = false;

        // First pass: look up existing PF measurements; create new ones for circuits missing them,
        // tracks (calc, derivedPointTag) so we can resolve newly-created measurements after the wait
        List<(PowerCalculation calc, string pointTag)> pendingBindings = [];

        foreach (PowerCalculation calc in m_configuredCalculations)
        {
            try
            {
                IMeasurement anchor = calc.ActivePowerOutputMeasurement
                                   ?? calc.ApparentPowerOutputMeasurement
                                   ?? calc.ReactivePowerOutputMeasurement;

                if (anchor is null)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Skipping power factor output for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}): no MW/MVA/MVAR output measurement is configured to derive a point tag from.");
                    continue;
                }

                string anchorPointTag = anchor.TagName;
                string pfPointTag = DerivePowerFactorPointTag(anchorPointTag);

                if (string.IsNullOrWhiteSpace(pfPointTag))
                {
                    OnStatusMessage(MessageLevel.Warning, $"Skipping power factor output for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}): unable to derive a point tag from anchor \"{anchorPointTag}\".");
                    continue;
                }

                pendingBindings.Add((calc, pfPointTag));

                if (this.PointTagExists(pfPointTag, out _))
                    continue;

                // Create new PF measurement on the same device as the anchor so the new tag is co-located
                // in the UI / historian browser with its sibling MW/MVA/MVAR points.
                int deviceID = this.LookupDevice(anchor.Key.SignalID).ID;
                string pfSignalReference = SignalReference.ToString(pfPointTag, SignalKind.Calculation);
                string description = calc.CircuitDescription ?? string.Empty;
                int index = description.LastIndexOf(';');
                
                if (index > 0)
                    description = description.Substring(0, index).Trim();

                OnStatusMessage(MessageLevel.Info, $"Creating power factor output measurement \"{pfSignalReference}\"...");

                MeasurementRecord record = this.GetMeasurementRecord(
                    deviceID > 0 ? deviceID : null,
                    pfPointTag,
                    null,
                    pfSignalReference,
                    $"{description} Power Factor Calculation");

                newSignalIDs.Add(record.SignalID);
                recordsAdded = true;
            }
            catch (Exception ex)
            {
                OnStatusMessage(MessageLevel.Warning, $"Failed to create power factor output measurement for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}): {ex.Message}");
            }
        }

        if (recordsAdded)
        {
            m_configurationReloaded.Reset();

            // Notify host system that configuration has changed
            this.OnConfigurationChanged();

            OnStatusMessage(MessageLevel.Info, "Waiting for the newly created power factor output measurements to be loaded into active configuration...");

            if (!this.WaitForSignalsToLoad(m_configurationReloaded, newSignalIDs))
                OnStatusMessage(MessageLevel.Warning, $"Power factor output measurements not found in active configuration after waiting {MetadataHelpers.ElapsedWaitTimeString()} - power factor outputs may not be available until next reload.");
        }

        // Second pass: bind looked-up / newly-created PF signals back onto each PowerCalculation, then
        // append them to OutputMeasurements so the routing layer accepts them as valid outputs
        List<IMeasurement> pfOutputs = [];

        foreach ((PowerCalculation calc, string pointTag) in pendingBindings)
        {
            try
            {
                if (!this.PointTagExists(pointTag, out Guid signalID))
                {
                    OnStatusMessage(MessageLevel.Warning, $"Power factor output \"{pointTag}\" for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}) was not found in active configuration; PF will not be published for this circuit until the next reload.");
                    continue;
                }

                MeasurementKey key = MeasurementKey.LookUpBySignalID(signalID);

                if (key.SignalID == Guid.Empty)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Power factor measurement key for \"{pointTag}\" could not be resolved; PF will not be published for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}).");
                    continue;
                }

                Measurement pfMeasurement = new() { Metadata = key.Metadata };
                calc.PowerFactorOutputMeasurement = pfMeasurement;
                pfOutputs.Add(pfMeasurement);
            }
            catch (Exception ex)
            {
                OnStatusMessage(MessageLevel.Warning, $"Failed to bind power factor output for power calculation {calc.PowerCalculationID} ({calc.CircuitDescription}): {ex.Message}");
            }
        }

        if (pfOutputs.Count == 0)
            return;

        OutputMeasurements = OutputMeasurements is null ? 
            pfOutputs.ToArray() : 
            OutputMeasurements.Concat(pfOutputs).ToArray();
    }

    /// <summary>
    /// Derives a power factor point tag from an existing MW / MVAR / MVA point tag by replacing the
    /// trailing power suffix with <c>PF</c>. Falls back to appending <c>-PF</c> when no recognized suffix
    /// is present.
    /// </summary>
    private static string DerivePowerFactorPointTag(string anchorPointTag)
    {
        if (string.IsNullOrWhiteSpace(anchorPointTag))
            return null;

        string tag = anchorPointTag.Trim();

        // Walk longest-first so MVAR/MVA aren't shadowed by MW. Each token is matched as a hyphen- or
        // underscore-prefixed segment immediately preceding either end-of-string or a ':' (e.g., ":CALC")
        string[] suffixes = ["MVAR", "MVA", "MW"];

        foreach (string suffix in suffixes)
        {
            foreach (char separator in new[] { '-', '_' })
            {
                string token = $"{separator}{suffix}";
                int index = tag.LastIndexOf(token, StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    continue;

                int end = index + token.Length;

                // Suffix must terminate the tag, or be immediately followed by a recognized signal-type marker
                if (end != tag.Length && tag[end] != ':')
                    continue;

                return $"{tag.Substring(0, index)}{separator}PF{tag.Substring(end)}";
            }
        }

        // Strip any trailing ":CALC" style signal-type segment before appending so the new tag stays clean
        int colonIndex = tag.IndexOf(':');
        return colonIndex < 0 ? $"{tag}-PF" : $"{tag.Substring(0, colonIndex)}-PF{tag.Substring(colonIndex)}";
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