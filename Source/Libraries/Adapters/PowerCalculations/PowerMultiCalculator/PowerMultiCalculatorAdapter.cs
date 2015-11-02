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
//  11/2/2015 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;

namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Performs MW, MVA, and MVAR calculations based on current and voltage phasors input to the adapter
	/// </summary>
	[Description("PowerMultiCalculatorAdapter: Performs MW, MVA, and MVAR calculations based on current and voltage phasors input to the adapter")]
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

		private Queue<IMeasurement> m_lastRealPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
		private Queue<IMeasurement> m_lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
		private Queue<IMeasurement> m_lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);

		#endregion

		#region [ Constructors ]

		/// <summary>
		/// Creates the adapter
		/// </summary>
		public PowerMultiCalculatorAdapter()
		{
			using (var database = new AdoDataConnection("systemSettings"))
			{
				var dataOperationExists = PowerCalculationConfigurationValidation.CheckDataOperationExists(database);
				if (!dataOperationExists)
				{
					PowerCalculationConfigurationValidation.CreateDataOperation(database);
				}
			}
		}

		#endregion

		#region [ Properties ]

		/// <summary>
		/// Gets or sets a boolean indicating whether or not this adapter will produce a result for all calculations. If this value is true and a calculation fails,
		/// the adapter will produce NaN for that calculation. If this value is false and a calculation fails, the adapter will not produce any result.
		/// </summary>
		[ConnectionStringParameter,
		 Description("Defines whether or not the adapter should always produce a result. When true, adapter will produce NaN for calculations that fail."),
		 DefaultValue(false)]
		public bool AlwaysProduceResult { get; set; }

		/// <summary>
		/// Gets or sets a boolean indicating whether or not this adapter should multiply all calculation results by sqrt(3)
		/// </summary>
		[ConnectionStringParameter,
		 Description("Defines whether the adapter should apply a sqrt(3) adjustment to all results."),
		 DefaultValue(false)]
		public bool ApplySqrt3Adjustment { get; set; }

		/// <summary>
		/// Returns the adapter status, including real-time statistics about adapter operation
		/// </summary>
		public override string Status
		{
			get
			{
				var status = new StringBuilder();

				status.AppendLine(string.Format("        Last Total Calculations: {0}", m_lastTotalCalculations));
				status.AppendLine(string.Format("     Average Total Calculations: {0}", Math.Round(m_averageCalculationsPerFrame.Average)));
				status.AppendLine(string.Format("       Average Calculation Time: {0} ms", m_averageCalculationTime.Average.ToString("N4")));
				status.AppendLine(string.Format("    Last Total Calculation Time: {0} ms", m_lastTotalCalculationTime.ToString("N4")));
				status.AppendLine(string.Format("Average  Total Calculation Time: {0} ms", m_averageTotalCalculationTime.Average.ToString("N4")));

				status.AppendLine("   Last Real Power Measurements:");
				if (!m_lastRealPowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var realPowerValues = new IMeasurement[m_lastRealPowerCalculations.Count];
					m_lastRealPowerCalculations.CopyTo(realPowerValues, 0);
					foreach (var measurement in realPowerValues)
					{
						status.AppendLine(string.Format("\t{0} = {1}", measurement.Key, measurement.AdjustedValue.ToString("N3")));
					}
				}

				status.AppendLine("   Last Reactive Power Measurements:");
				if (!m_lastReactivePowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var reactivePowerValues = new IMeasurement[m_lastReactivePowerCalculations.Count];
					m_lastReactivePowerCalculations.CopyTo(reactivePowerValues, 0);
					foreach (var measurement in reactivePowerValues)
					{
						status.AppendLine(string.Format("\t{0} = {1}", measurement.Key, measurement.AdjustedValue.ToString("N3")));
					}
				}

				status.AppendLine("   Last Active Power Measurements:");
				if (!m_lastActivePowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var activePowerValues = new IMeasurement[m_lastActivePowerCalculations.Count];
					m_lastActivePowerCalculations.CopyTo(activePowerValues, 0);
					foreach (var measurement in activePowerValues)
					{
						status.AppendLine(string.Format("\t{0} = {1}", measurement.Key, measurement.AdjustedValue.ToString("N3")));
					}
				}

				status.AppendLine();

				status.Append(base.Status);
				return status.ToString();
			}
		}

		/// <summary>
		/// Returns true or false to indicate whether this adapter will run in a non-realtime IAON session
		/// </summary>
		public override bool SupportsTemporalProcessing
		{
			get { return true; }
		}

		#endregion

		#region [ Methods ]

		/// <summary>
		/// Loads configuration from the database and configures adapter to run with that configuration
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			m_averageCalculationsPerFrame = new RunningAverage();
			m_averageCalculationTime = new RunningAverage();
			m_averageTotalCalculationTime = new RunningAverage();
			m_lastRealPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
			m_lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
			m_lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);

			m_configuredCalculations = new List<PowerCalculation>();
			using (var database = new AdoDataConnection("systemSettings"))
			using (var cmd = database.Connection.CreateCommand())
			{
				cmd.CommandText = string.Format("SELECT PowerCalculationId, CircuitDescription, VoltageAngleSignalId, VoltageMagSignalId, CurrentAngleSignalId, CurrentMagSignalID, " +
								  "RealPowerOutputSignalId, ReactivePowerOutputSignalId, ActivePowerOutputSignalId FROM PowerCalculation " +
								  "WHERE NodeId = '{0}' AND CalculationEnabled=1", ConfigurationFile.Current.Settings["systemSettings"]["NodeID"].ValueAs<Guid>());

				var rdr = cmd.ExecuteReader();
				while (rdr.Read())
				{
					var pc = new PowerCalculation();
					pc.PowerCalculationId = rdr.GetInt32(0);
					pc.CircuitDescription = rdr.GetString(1);
					pc.VoltageAngleSignalId = MeasurementKey.LookUpBySignalID(rdr.GetGuid(2));
					pc.VoltageMagnitudeSignalId = MeasurementKey.LookUpBySignalID(rdr.GetGuid(3));
					pc.CurrentAngleSignalId = MeasurementKey.LookUpBySignalID(rdr.GetGuid(4));
					pc.CurrentMagnitudeSignalId = MeasurementKey.LookUpBySignalID(rdr.GetGuid(5));
					pc.ActivePowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(6));
					pc.ReactivePowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(7));
					pc.RealPowerOutputMeasurement = AddOutputMeasurement(rdr.GetGuid(8));
					m_configuredCalculations.Add(pc);
				}
			}

			if (m_configuredCalculations.Any())
			{
				InputMeasurementKeys = m_configuredCalculations.SelectMany(pc => new[] { pc.CurrentAngleSignalId, pc.CurrentMagnitudeSignalId, pc.VoltageAngleSignalId, pc.VoltageMagnitudeSignalId }).ToArray();
			}

			var settings = Settings;
			string setting;

			if (settings.TryGetValue("AlwaysProduceResult", out setting))
				AlwaysProduceResult = setting.ParseBoolean();
			if (settings.TryGetValue("ApplySqrt3Adjustment", out setting))
				ApplySqrt3Adjustment = setting.ParseBoolean();
		}

		/// <summary>
		/// Calculates MW, MVAR, and MVA, and publishes those measurements
		/// </summary>
		/// <param name="frame">Input values for calculation</param>
		/// <param name="index"></param>
		protected override void PublishFrame(IFrame frame, int index)
		{
			var totalCalculationTimeStopwatch = new Stopwatch();
			totalCalculationTimeStopwatch.Start();
			var lastCalculationTimeStopwatch = new Stopwatch();
			var calculations = 0;
			var outputMeasurements = new List<IMeasurement>();
			var measurements = frame.Measurements;
			foreach (var powerCalculation in m_configuredCalculations)
			{
				double power = double.NaN, reactivePower = double.NaN, apparentPower = double.NaN;
				try
				{
					lastCalculationTimeStopwatch.Restart();
					double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
					IMeasurement measurement;
					var allValuesReceived = false;

					if (measurements.TryGetValue(powerCalculation.VoltageMagnitudeSignalId, out measurement) && measurement.ValueQualityIsGood())
					{
						voltageMagnitude = measurement.AdjustedValue;
						if (ApplySqrt3Adjustment)
						{
							voltageMagnitude *= SqrtOf3;
						}

						if (measurements.TryGetValue(powerCalculation.VoltageAngleSignalId, out measurement) && measurement.ValueQualityIsGood())
						{
							voltageAngle = measurement.AdjustedValue;

							if (measurements.TryGetValue(powerCalculation.CurrentMagnitudeSignalId, out measurement) && measurement.ValueQualityIsGood())
							{
								currentMagnitude = measurement.AdjustedValue;

								if (measurements.TryGetValue(powerCalculation.CurrentAngleSignalId, out measurement) && measurement.ValueQualityIsGood())
								{
									currentAngle = measurement.AdjustedValue;
									allValuesReceived = true;
								}
							}
						}
					}

					if (allValuesReceived)
					{
						var angleDifference = Math.Abs(voltageAngle - currentAngle);

						if (angleDifference > 180)
							angleDifference = 360 - angleDifference;

						// Convert phase angle difference to radians
						var impedancePhaseAngle = Angle.FromDegrees(angleDifference);

						// Calculate apparent power (S) vector magnitude in Mega volt-amps
						apparentPower = (Math.Abs(voltageMagnitude) / SI.Mega) * Math.Abs(currentMagnitude);

						// Calculate power (P) and reactive power (Q)
						power = apparentPower * Math.Cos(impedancePhaseAngle);
						reactivePower = apparentPower * Math.Sin(impedancePhaseAngle);
					}
				}
				catch (Exception e)
				{
					OnProcessException(e);
				}
				finally
				{
					if (OutputMeasurements != null && OutputMeasurements.Any())
					{
						var realPowerMeasurement = GSF.TimeSeries.Measurement.Clone(powerCalculation.RealPowerOutputMeasurement, power, frame.Timestamp);
						var reactivePowerMeasurement = GSF.TimeSeries.Measurement.Clone(powerCalculation.ReactivePowerOutputMeasurement, reactivePower, frame.Timestamp);
						var activePowerMeasurement = GSF.TimeSeries.Measurement.Clone(powerCalculation.ActivePowerOutputMeasurement, apparentPower, frame.Timestamp);

						if (AlwaysProduceResult || !double.IsNaN(realPowerMeasurement.Value))
						{
							outputMeasurements.Add(realPowerMeasurement);
							calculations++;
							m_lastRealPowerCalculations.Enqueue(realPowerMeasurement);
							while (m_lastRealPowerCalculations.Count > ValuesToTrack)
							{
								m_lastRealPowerCalculations.Dequeue();
							}
						}
						if (AlwaysProduceResult || !double.IsNaN(reactivePowerMeasurement.Value))
						{
							outputMeasurements.Add(reactivePowerMeasurement);
							calculations++;
							m_lastReactivePowerCalculations.Enqueue(reactivePowerMeasurement);
							while (m_lastReactivePowerCalculations.Count > ValuesToTrack)
							{
								m_lastReactivePowerCalculations.Dequeue();
							}
						}
						if (AlwaysProduceResult || !double.IsNaN(activePowerMeasurement.Value))
						{
							outputMeasurements.Add(activePowerMeasurement);
							calculations++;
							m_lastActivePowerCalculations.Enqueue(activePowerMeasurement);
							while (m_lastActivePowerCalculations.Count > ValuesToTrack)
							{
								m_lastActivePowerCalculations.Dequeue();
							}
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

		private GSF.TimeSeries.Measurement AddOutputMeasurement(Guid signalId)
		{
			var measurement = GetMeasurement(signalId);
			if (measurement != null)
				OutputMeasurements = (OutputMeasurements ?? Enumerable.Empty<IMeasurement>()).Concat(Enumerable.Repeat(measurement, 1)).ToArray();

			return measurement;
		}

		private GSF.TimeSeries.Measurement GetMeasurement(Guid signalId)
		{
			var rows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}'", signalId));
			if (!rows.Any()) return null;

			var meas = new GSF.TimeSeries.Measurement();
			meas.Key = MeasurementKey.LookUpBySignalID(signalId);
			meas.TagName = rows[0]["PointTag"].ToString();
			meas.Adder = Convert.ToDouble(rows[0]["Adder"]);
			meas.Multiplier = Convert.ToDouble(rows[0]["Multiplier"]);
			return meas;
		}

		#endregion
	}
}
