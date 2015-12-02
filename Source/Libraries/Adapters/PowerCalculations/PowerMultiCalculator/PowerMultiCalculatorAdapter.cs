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
using System.Diagnostics;
using System.Linq;
using System.Text;
using GSF;
using GSF.Configuration;
using GSF.Data;
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

        private const double SqrtOf3 = 1.7320508075688772935274463415059D;
        private const int ValuesToTrack = 5;

        private List<PowerCalculation> m_configuredCalculations;
        private RunningAverage m_averageCalculationsPerFrame = new RunningAverage();
        private RunningAverage m_averageCalculationTime = new RunningAverage();
        private RunningAverage m_averageTotalCalculationTime = new RunningAverage();
        private double m_lastTotalCalculationTime;
        private int m_lastTotalCalculations;

        private Queue<IMeasurement> m_lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
        private Queue<IMeasurement> m_lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
        private Queue<IMeasurement> m_lastApparentPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates the adapter
        /// </summary>
        public PowerMultiCalculatorAdapter()
        {
            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            {
                bool dataOperationExists = PowerCalculationConfigurationValidation.CheckDataOperationExists(database);
                if (!dataOperationExists)
                {
                    PowerCalculationConfigurationValidation.CreateDataOperation(database);
                }
            }
        }

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
        /// Gets or sets flag indicating whether or not this adapter should multiply all calculation results by sqrt(3)
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if adapter should apply a sqrt(3) adjustment to all results.")]
        [DefaultValue(false)]
        public bool ApplySqrt3Adjustment { get; set; }

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

                status.Append(base.Status);

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
                    IMeasurement[] activePowerValues = new IMeasurement[m_lastActivePowerCalculations.Count];
                    m_lastActivePowerCalculations.CopyTo(activePowerValues, 0);

                    foreach (IMeasurement measurement in activePowerValues)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
                }

                status.AppendLine("   Last Reactive Power Measurements:");

                if (!m_lastReactivePowerCalculations.Any())
                {
                    status.AppendLine("\tNot enough values...");
                }
                else
                {
                    IMeasurement[] reactivePowerValues = new IMeasurement[m_lastReactivePowerCalculations.Count];

                    m_lastReactivePowerCalculations.CopyTo(reactivePowerValues, 0);

                    foreach (IMeasurement measurement in reactivePowerValues)
                        status.AppendLine($"\t{measurement.Key} = {measurement.AdjustedValue:N3}");
                }

                status.AppendLine("   Last Apparent Power Measurements:");

                if (!m_lastApparentPowerCalculations.Any())
                {
                    status.AppendLine("\tNot enough values...");
                }
                else
                {
                    IMeasurement[] apparentPowerValues = new IMeasurement[m_lastApparentPowerCalculations.Count];

                    m_lastApparentPowerCalculations.CopyTo(apparentPowerValues, 0);

                    foreach (IMeasurement measurement in apparentPowerValues)
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

            m_averageCalculationsPerFrame = new RunningAverage();
            m_averageCalculationTime = new RunningAverage();
            m_averageTotalCalculationTime = new RunningAverage();
            m_lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
            m_lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
            m_lastApparentPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
            m_configuredCalculations = new List<PowerCalculation>();

            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            using (IDbCommand cmd = database.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT " +
                    //   0                   1                   2                     3                   4                     5
                    "PowerCalculationId, CircuitDescription, VoltageAngleSignalId, VoltageMagSignalId, CurrentAngleSignalId, CurrentMagSignalID, " +
                    //   6                        7                            8
                    "RealPowerOutputSignalId, ReactivePowerOutputSignalId, ActivePowerOutputSignalId FROM PowerCalculation " + 
                    $"WHERE NodeId = '{ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>()}' AND CalculationEnabled <> 0 ";

                IDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    m_configuredCalculations.Add(new PowerCalculation
                    {
                        PowerCalculationID = rdr.GetInt32(0),
                        CircuitDescription = rdr.GetString(1),
                        VoltageAngleSignalID = MeasurementKey.LookUpBySignalID(rdr.GetGuid(2)),
                        VoltageMagnitudeSignalID = MeasurementKey.LookUpBySignalID(rdr.GetGuid(3)),
                        CurrentAngleSignalID = MeasurementKey.LookUpBySignalID(rdr.GetGuid(4)),
                        CurrentMagnitudeSignalID = MeasurementKey.LookUpBySignalID(rdr.GetGuid(5)),
                        ActivePowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(6), outputMeasurements),
                        ReactivePowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(7), outputMeasurements),
                        ApparentPowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(8), outputMeasurements)
                    });
                }
            }

            if (m_configuredCalculations.Any())
                InputMeasurementKeys = m_configuredCalculations.SelectMany(pc => new[] { pc.CurrentAngleSignalID, pc.CurrentMagnitudeSignalID, pc.VoltageAngleSignalID, pc.VoltageMagnitudeSignalID }).ToArray();

            if (outputMeasurements.Any())
                OutputMeasurements = outputMeasurements.ToArray();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("AlwaysProduceResult", out setting))
                AlwaysProduceResult = setting.ParseBoolean();

            if (settings.TryGetValue("ApplySqrt3Adjustment", out setting))
                ApplySqrt3Adjustment = setting.ParseBoolean();

            if (settings.TryGetValue("EnableTemporalProcessing", out setting))
                EnableTemporalProcessing = setting.ParseBoolean();
        }

        /// <summary>
        /// Calculates MW, MVAR and MVA then publishes those measurements
        /// </summary>
        /// <param name="frame">Input values for calculation</param>
        /// <param name="index">Index of frame within second.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            Stopwatch totalCalculationTimeStopwatch = new Stopwatch();
            totalCalculationTimeStopwatch.Start();

            Stopwatch lastCalculationTimeStopwatch = new Stopwatch();
            int calculations = 0;
            List<IMeasurement> outputMeasurements = new List<IMeasurement>();

            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;

            foreach (PowerCalculation powerCalculation in m_configuredCalculations)
            {
                double activePower = double.NaN, reactivePower = double.NaN, apparentPower = double.NaN;

                try
                {
                    lastCalculationTimeStopwatch.Restart();
                    double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
                    IMeasurement measurement;
                    bool allValuesReceived = false;

                    if (measurements.TryGetValue(powerCalculation.VoltageMagnitudeSignalID, out measurement) && measurement.ValueQualityIsGood())
                    {
                        voltageMagnitude = measurement.AdjustedValue;

                        if (ApplySqrt3Adjustment)
                            voltageMagnitude *= SqrtOf3;

                        if (measurements.TryGetValue(powerCalculation.VoltageAngleSignalID, out measurement) && measurement.ValueQualityIsGood())
                        {
                            voltageAngle = measurement.AdjustedValue;

                            if (measurements.TryGetValue(powerCalculation.CurrentMagnitudeSignalID, out measurement) && measurement.ValueQualityIsGood())
                            {
                                currentMagnitude = measurement.AdjustedValue;

                                if (measurements.TryGetValue(powerCalculation.CurrentAngleSignalID, out measurement) && measurement.ValueQualityIsGood())
                                {
                                    currentAngle = measurement.AdjustedValue;
                                    allValuesReceived = true;
                                }
                            }
                        }
                    }

                    if (allValuesReceived)
                    {
                        // Calculate power (P), reactive power (Q) and apparent power (|S|)
                        Phasor voltage = new Phasor(PhasorType.Voltage, Angle.FromDegrees(voltageAngle), voltageMagnitude);
                        Phasor current = new Phasor(PhasorType.Current, Angle.FromDegrees(currentAngle), currentMagnitude);

                        activePower = Phasor.CalculateActivePower(voltage, current);
                        reactivePower = Phasor.CalculateReactivePower(voltage, current);
                        apparentPower = Phasor.CalculateApparentPower(voltage, current);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(ex);
                }
                finally
                {
                    if ((object)powerCalculation.ActivePowerOutputMeasurement != null)
                    {
                        Measurement activePowerMeasurement = Measurement.Clone(powerCalculation.ActivePowerOutputMeasurement, activePower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(activePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(activePowerMeasurement);
                            calculations++;
                            m_lastActivePowerCalculations.Enqueue(activePowerMeasurement);

                            while (m_lastActivePowerCalculations.Count > ValuesToTrack)
                                m_lastActivePowerCalculations.Dequeue();
                        }
                    }

                    if ((object)powerCalculation.ReactivePowerOutputMeasurement != null)
                    {
                        Measurement reactivePowerMeasurement = Measurement.Clone(powerCalculation.ReactivePowerOutputMeasurement, reactivePower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(reactivePowerMeasurement.Value))
                        {
                            outputMeasurements.Add(reactivePowerMeasurement);
                            calculations++;
                            m_lastReactivePowerCalculations.Enqueue(reactivePowerMeasurement);

                            while (m_lastReactivePowerCalculations.Count > ValuesToTrack)
                                m_lastReactivePowerCalculations.Dequeue();
                        }
                    }

                    if ((object)powerCalculation.ApparentPowerOutputMeasurement != null)
                    {
                        Measurement apparentPowerMeasurement = Measurement.Clone(powerCalculation.ApparentPowerOutputMeasurement, apparentPower, frame.Timestamp);

                        if (AlwaysProduceResult || !double.IsNaN(apparentPowerMeasurement.Value))
                        {
                            outputMeasurements.Add(apparentPowerMeasurement);
                            calculations++;
                            m_lastApparentPowerCalculations.Enqueue(apparentPowerMeasurement);

                            while (m_lastApparentPowerCalculations.Count > ValuesToTrack)
                                m_lastApparentPowerCalculations.Dequeue();
                        }
                    }

                    lastCalculationTimeStopwatch.Stop();
                    m_averageCalculationTime.AddValue(lastCalculationTimeStopwatch.Elapsed.TotalMilliseconds);
                }
            }

            totalCalculationTimeStopwatch.Stop();

            m_lastTotalCalculationTime = totalCalculationTimeStopwatch.Elapsed.TotalMilliseconds;
            m_averageTotalCalculationTime.AddValue(totalCalculationTimeStopwatch.Elapsed.TotalMilliseconds);

            m_lastTotalCalculations = calculations;
            m_averageCalculationsPerFrame.AddValue(calculations);

            OnNewMeasurements(outputMeasurements);
        }

        private Measurement AddOutputMeasurement(Guid signalID, HashSet<IMeasurement> outputMeasurements)
        {
            Measurement measurement = GetMeasurement(signalID);

            if ((object)measurement != null)
                outputMeasurements.Add(measurement);

            return measurement;
        }

        private Measurement GetMeasurement(Guid signalID)
        {
            DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select($"SignalID = '{signalID}'");

            if (!rows.Any())
                return null;

            Measurement measurement = new Measurement
            {
                Key = MeasurementKey.LookUpBySignalID(signalID),
                TagName = rows[0]["PointTag"].ToString(),
                Adder = Convert.ToDouble(rows[0]["Adder"]),
                Multiplier = Convert.ToDouble(rows[0]["Multiplier"])
            };

            return measurement;
        }

        #endregion
    }
}
