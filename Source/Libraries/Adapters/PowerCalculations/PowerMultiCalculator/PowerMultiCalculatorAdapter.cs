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
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Units.EE;

namespace PowerCalculations.PowerMultiCalculator
{
    /// <summary>
    /// Performs MW, MVA, and MVAR calculations based on current and voltage phasors input to the adapter
    /// </summary>
    [Description("PowerMultiCalculatorAdapter: Performs MW, MVAR and MVA calculations based on current and voltage phasors input to the adapter")]
    public class PowerMultiCalculatorAdapter : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const string DefaultTableName = "PowerCalculation";
        private const double SqrtOf3 = 1.7320508075688772935274463415059D;
        private const int ValuesToTrack = 5;

        // Fields
        private List<PowerCalculation> m_configuredCalculations;
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
        /// Gets or sets the name of the table this adapter will use to obtain its metadata.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the name of the table this adapter will use to obtain its metadata.")]
        [DefaultValue(DefaultTableName)]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets flag indicating whether or not this adapter will produce a result for all calculations. If this value is true and a calculation fails,
        /// the adapter will produce NaN for that calculation. If this value is false and a calculation fails, the adapter will not produce any result.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if adapter should always produce a result. When true, adapter will produce NaN for calculations that fail.")]
        [DefaultValue(false)]
        public bool AlwaysProduceResult { get; set; }

        /// <summary>
        /// Gets or sets the default strategy used to adjust voltage values for based on the nature of the voltage measurements.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines default strategy used to adjust voltage values for based on the nature of the voltage measurements.")]
        [DefaultValue(VoltageAdjustmentStrategy.LineToNeutral)]
        public VoltageAdjustmentStrategy AdjustmentStrategy { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if adapter should enable temporal processing support.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if adapter should enable temporal processing support.")]
        [DefaultValue(false)]
        public bool EnableTemporalProcessing { get; set; }

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
                StringBuilder status = new StringBuilder();
                VoltageAdjustmentStrategy[] strategies = m_adjustmentStrategies?.Values.ToArray() ?? Array.Empty<VoltageAdjustmentStrategy>();

                status.Append(base.Status);

                status.AppendLine($"    Default Adjustment Strategy: {AdjustmentStrategy}");
                status.AppendLine($"  Total Per-Circuit Adjustments: {strategies.Length:N0}");

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

                status.AppendLine($"        Last Total Calculations: {m_lastTotalCalculations}");
                status.AppendLine($"     Average Total Calculations: {Math.Round(m_averageCalculationsPerFrame.Average):N3} per frame");
                status.AppendLine($"       Average Calculation Time: {m_averageCalculationTime.Average:N3} ms");
                status.AppendLine($"    Last Total Calculation Time: {m_lastTotalCalculationTime:N3} ms");
                status.AppendLine($" Average Total Calculation Time: {m_averageTotalCalculationTime.Average:N3} ms");

                status.AppendLine("   Last Active Power Measurements:");

                if (!m_lastActivePowerCalculations.Any())
                {
                    status.AppendLine("\tNot enough values...");
                }
                else
                {
                    foreach (IMeasurement measurement in m_lastActivePowerCalculations)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
                }

                status.AppendLine("   Last Reactive Power Measurements:");

                if (!m_lastReactivePowerCalculations.Any())
                {
                    status.AppendLine("\tNot enough values...");
                }
                else
                {
                    foreach (IMeasurement measurement in m_lastReactivePowerCalculations)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
                }

                status.AppendLine("   Last Apparent Power Measurements:");

                if (!m_lastApparentPowerCalculations.Any())
                {
                    status.AppendLine("\tNot enough values...");
                }
                else
                {
                    foreach (IMeasurement measurement in m_lastApparentPowerCalculations)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
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

            HashSet<IMeasurement> outputMeasurements = new HashSet<IMeasurement>();

            m_configuredCalculations = new List<PowerCalculation>();
            m_adjustmentStrategies = new Dictionary<MeasurementKey, VoltageAdjustmentStrategy>();
            m_averageCalculationsPerFrame = new RunningAverage();
            m_averageCalculationTime = new RunningAverage();
            m_averageTotalCalculationTime = new RunningAverage();
            m_lastActivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastReactivePowerCalculations = new ConcurrentQueue<IMeasurement>();
            m_lastApparentPowerCalculations = new ConcurrentQueue<IMeasurement>();

            if (!Settings.TryGetValue(nameof(TableName), out string tableName))
                tableName = DefaultTableName;

            string query = "SELECT " +
                         // 0          1                    2                     3                   4                     5
                           "ID, CircuitDescription, VoltageAngleSignalID, VoltageMagSignalID, CurrentAngleSignalID, CurrentMagSignalID, " +
                         //            6                          7                            8
                           "ActivePowerOutputSignalID, ReactivePowerOutputSignalID, ApparentPowerOutputSignalID FROM " + tableName + " " +
                           "WHERE NodeId = {0} AND Enabled <> 0 ";

            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            using (IDataReader rdr = database.ExecuteReader(query, ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>()))
            {
                while (rdr.Read())
                {
                    m_configuredCalculations.Add(new PowerCalculation
                    {
                        PowerCalculationID = rdr.GetInt32(0),
                        CircuitDescription = rdr.GetString(1),
                        VoltageAngleMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(rdr[2].ToString())),
                        VoltageMagnitudeMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(rdr[3].ToString())),
                        CurrentAngleMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(rdr[4].ToString())),
                        CurrentMagnitudeMeasurementKey = MeasurementKey.LookUpBySignalID(Guid.Parse(rdr[5].ToString())),
                        ActivePowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(rdr[6].ToString()), outputMeasurements),
                        ReactivePowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(rdr[7].ToString()), outputMeasurements),
                        ApparentPowerOutputMeasurement = AddOutputMeasurement(Guid.Parse(rdr[8].ToString()), outputMeasurements)
                    });
                }
            }

            if (m_configuredCalculations.Any())
                InputMeasurementKeys = m_configuredCalculations.SelectMany(pc => new[] { pc.CurrentAngleMeasurementKey, pc.CurrentMagnitudeMeasurementKey, pc.VoltageAngleMeasurementKey, pc.VoltageMagnitudeMeasurementKey }).ToArray();
            else
                throw new InvalidOperationException("Skipped initialization of power calculator: no defined power calculations...");

            if (outputMeasurements.Any())
                OutputMeasurements = outputMeasurements.ToArray();

            Dictionary<string, string> settings = Settings;

            if (settings.TryGetValue(nameof(AlwaysProduceResult), out string setting))
                AlwaysProduceResult = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(AdjustmentStrategy), out setting) && Enum.TryParse(setting, out VoltageAdjustmentStrategy adjustmentStrategy))
                AdjustmentStrategy = adjustmentStrategy;

            if (settings.TryGetValue(nameof(EnableTemporalProcessing), out setting))
                EnableTemporalProcessing = setting.ParseBoolean();

            // Define per power calculation line adjustment strategies
            foreach (PowerCalculation powerCalculation in m_configuredCalculations)
            {
                if (powerCalculation.VoltageMagnitudeMeasurementKey is null || string.IsNullOrWhiteSpace(powerCalculation.CircuitDescription))
                    continue;

                try
                {
                    Dictionary<string, string> circuitSettings = powerCalculation.CircuitDescription.ParseKeyValuePairs();

                    if (circuitSettings.TryGetValue(nameof(AdjustmentStrategy), out setting) && Enum.TryParse(setting, out adjustmentStrategy))
                        m_adjustmentStrategies[powerCalculation.VoltageMagnitudeMeasurementKey] = adjustmentStrategy;
                }
                catch (Exception ex)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Failed to parse settings from circuit description \"{powerCalculation.CircuitDescription}\": {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calculates MW, MVAR and MVA then publishes those measurements
        /// </summary>
        /// <param name="frame">Input values for calculation</param>
        /// <param name="index">Index of frame within second.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            Ticks totalCalculationTime = DateTime.UtcNow.Ticks;
            Ticks lastCalculationTime = DateTime.UtcNow.Ticks;
            List<IMeasurement> outputMeasurements = new List<IMeasurement>();
            int calculations = 0;

            foreach (PowerCalculation powerCalculation in m_configuredCalculations)
            {
                double activePower = double.NaN, reactivePower = double.NaN, apparentPower = double.NaN;
                IMeasurement measurement;

                try
                {
                    double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
                    bool allValuesReceivedWithGoodQuality = false;

                    lastCalculationTime = DateTime.UtcNow.Ticks;

                    if (measurements.TryGetValue(powerCalculation.VoltageMagnitudeMeasurementKey, out measurement) && measurement.ValueQualityIsGood())
                    {
                        voltageMagnitude = measurement.AdjustedValue;

                        if (!m_adjustmentStrategies.TryGetValue(powerCalculation.VoltageMagnitudeMeasurementKey, out VoltageAdjustmentStrategy adjustmentStrategy))
                            adjustmentStrategy = AdjustmentStrategy;

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

                        if (measurements.TryGetValue(powerCalculation.VoltageAngleMeasurementKey, out measurement) && measurement.ValueQualityIsGood())
                        {
                            voltageAngle = measurement.AdjustedValue;

                            if (measurements.TryGetValue(powerCalculation.CurrentMagnitudeMeasurementKey, out measurement) && measurement.ValueQualityIsGood())
                            {
                                currentMagnitude = measurement.AdjustedValue;

                                if (measurements.TryGetValue(powerCalculation.CurrentAngleMeasurementKey, out measurement) && measurement.ValueQualityIsGood())
                                {
                                    currentAngle = measurement.AdjustedValue;
                                    allValuesReceivedWithGoodQuality = true;
                                }
                            }
                        }
                    }

                    if (allValuesReceivedWithGoodQuality)
                    {
                        // Calculate power (P), reactive power (Q) and apparent power (|S|)
                        Phasor voltage = new Phasor(PhasorType.Voltage, Angle.FromDegrees(voltageAngle), voltageMagnitude);
                        Phasor current = new Phasor(PhasorType.Current, Angle.FromDegrees(currentAngle), currentMagnitude);

                        activePower = Phasor.CalculateActivePower(voltage, current) / SI.Mega;
                        reactivePower = Phasor.CalculateReactivePower(voltage, current) / SI.Mega;
                        apparentPower = Phasor.CalculateApparentPower(voltage, current) / SI.Mega;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, ex);
                }
                finally
                {
                    if (!(powerCalculation.ActivePowerOutputMeasurement is null))
                    {
                        Measurement activePowerMeasurement = Measurement.Clone(powerCalculation.ActivePowerOutputMeasurement, activePower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(activePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(activePowerMeasurement);
                            calculations++;
                            m_lastActivePowerCalculations.Enqueue(activePowerMeasurement);

                            while (m_lastActivePowerCalculations.Count > ValuesToTrack)
                                m_lastActivePowerCalculations.TryDequeue(out measurement);
                        }
                    }

                    if (!(powerCalculation.ReactivePowerOutputMeasurement is null))
                    {
                        Measurement reactivePowerMeasurement = Measurement.Clone(powerCalculation.ReactivePowerOutputMeasurement, reactivePower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(reactivePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(reactivePowerMeasurement);
                            calculations++;
                            m_lastReactivePowerCalculations.Enqueue(reactivePowerMeasurement);

                            while (m_lastReactivePowerCalculations.Count > ValuesToTrack)
                                m_lastReactivePowerCalculations.TryDequeue(out measurement);
                        }
                    }

                    if (!(powerCalculation.ApparentPowerOutputMeasurement is null))
                    {
                        Measurement apparentPowerMeasurement = Measurement.Clone(powerCalculation.ApparentPowerOutputMeasurement, apparentPower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(apparentPowerMeasurement.Value))
                        {
                            outputMeasurements.Add(apparentPowerMeasurement);
                            calculations++;
                            m_lastApparentPowerCalculations.Enqueue(apparentPowerMeasurement);

                            while (m_lastApparentPowerCalculations.Count > ValuesToTrack)
                                m_lastApparentPowerCalculations.TryDequeue(out measurement);
                        }
                    }

                    m_averageCalculationTime.AddValue((DateTime.UtcNow.Ticks - lastCalculationTime).ToMilliseconds());
                }
            }

            m_lastTotalCalculationTime = (DateTime.UtcNow.Ticks - totalCalculationTime).ToMilliseconds();
            m_averageTotalCalculationTime.AddValue(m_lastTotalCalculationTime);

            m_lastTotalCalculations = calculations;
            m_averageCalculationsPerFrame.AddValue(calculations);

            OnNewMeasurements(outputMeasurements);
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private static Measurement AddOutputMeasurement(Guid signalID, HashSet<IMeasurement> outputMeasurements)
        {
            Measurement measurement = GetMeasurement(signalID);

            if (!(measurement is null))
                outputMeasurements.Add(measurement);

            return measurement;
        }

        private static Measurement GetMeasurement(Guid signalID)
        {
            MeasurementKey key = MeasurementKey.LookUpBySignalID(signalID);

            if (key.SignalID == Guid.Empty)
                return null;

            Measurement measurement = new Measurement
            {
                Metadata = key.Metadata,
            };

            return measurement;
        }

        #endregion
    }
}
