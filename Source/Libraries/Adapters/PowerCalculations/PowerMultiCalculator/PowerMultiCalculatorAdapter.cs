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
		private const double SqrtOf3 = 1.7320508075688772935274463415059D;
		private const int ValuesToTrack = 5;

		private List<PowerCalculation> _configuredCalculations;
		private RunningAverage _averageCalculationsPerFrame = new RunningAverage();
		private RunningAverage _averageCalculationTime = new RunningAverage();
		private RunningAverage _averageTotalCalculationTime = new RunningAverage();
		private double _lastTotalCalculationTime = 0;
		private int _lastTotalCalculations = 0;

		private Queue<IMeasurement> _lastRealPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
		private Queue<IMeasurement> _lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
		private Queue<IMeasurement> _lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);

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

		public override string Status
		{
			get
			{
				var status = new StringBuilder();

				status.AppendLine(string.Format("        Last Total Calculations: {0}", _lastTotalCalculations));
				status.AppendLine(string.Format("     Average Total Calculations: {0}", Math.Round(_averageCalculationsPerFrame.Average)));
				status.AppendLine(string.Format("       Average Calculation Time: {0} ms", _averageCalculationTime.Average.ToString("N4")));
				status.AppendLine(string.Format("    Last Total Calculation Time: {0} ms", _lastTotalCalculationTime.ToString("N4")));
				status.AppendLine(string.Format("Average  Total Calculation Time: {0} ms", _averageTotalCalculationTime.Average.ToString("N4")));

				status.AppendLine("   Last Real Power Measurements:");
				if (!_lastRealPowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var realPowerValues = new IMeasurement[_lastRealPowerCalculations.Count];
					_lastRealPowerCalculations.CopyTo(realPowerValues, 0);
					foreach (var measurement in realPowerValues)
					{
						status.AppendLine(string.Format("\t{0} = {1}", measurement.Key, measurement.AdjustedValue.ToString("N3")));
					}
				}

				status.AppendLine("   Last Reactive Power Measurements:");
				if (!_lastReactivePowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var reactivePowerValues = new IMeasurement[_lastReactivePowerCalculations.Count];
					_lastReactivePowerCalculations.CopyTo(reactivePowerValues, 0);
					foreach (var measurement in reactivePowerValues)
					{
						status.AppendLine(string.Format("\t{0} = {1}", measurement.Key, measurement.AdjustedValue.ToString("N3")));
					}
				}

				status.AppendLine("   Last Active Power Measurements:");
				if (!_lastActivePowerCalculations.Any())
				{
					status.AppendLine("\tNot enough values...");
				}
				else
				{
					var activePowerValues = new IMeasurement[_lastActivePowerCalculations.Count];
					_lastActivePowerCalculations.CopyTo(activePowerValues, 0);
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

		public override void Initialize()
		{
			base.Initialize();

			_averageCalculationsPerFrame = new RunningAverage();
			_averageCalculationTime = new RunningAverage();
			_averageTotalCalculationTime = new RunningAverage();
			_lastRealPowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
			_lastReactivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);
			_lastActivePowerCalculations = new Queue<IMeasurement>(ValuesToTrack);

			_configuredCalculations = new List<PowerCalculation>();
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
					_configuredCalculations.Add(pc);
				}
			}

			if (_configuredCalculations.Any())
			{
				InputMeasurementKeys = _configuredCalculations.SelectMany(pc => new[] { pc.CurrentAngleSignalId, pc.CurrentMagnitudeSignalId, pc.VoltageAngleSignalId, pc.VoltageMagnitudeSignalId }).ToArray();
			}

			var settings = Settings;
			string setting;

			if (settings.TryGetValue("AlwaysProduceResult", out setting))
				AlwaysProduceResult = setting.ParseBoolean();
			if (settings.TryGetValue("ApplySqrt3Adjustment", out setting))
				ApplySqrt3Adjustment = setting.ParseBoolean();
		}

		public override bool SupportsTemporalProcessing
		{
			get { return true; }
		}

		protected override void PublishFrame(IFrame frame, int index)
		{
			var totalCalculationTimeStopwatch = new Stopwatch();
			totalCalculationTimeStopwatch.Start();
			var lastCalculationTimeStopwatch = new Stopwatch();
			var calculations = 0;
			var outputMeasurements = new List<IMeasurement>();
			var measurements = frame.Measurements;
			foreach (var powerCalculation in _configuredCalculations)
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
						var realPowerMeasurement = Measurement.Clone(powerCalculation.RealPowerOutputMeasurement, power, frame.Timestamp);
						var reactivePowerMeasurement = Measurement.Clone(powerCalculation.ReactivePowerOutputMeasurement, reactivePower, frame.Timestamp);
						var activePowerMeasurement = Measurement.Clone(powerCalculation.ActivePowerOutputMeasurement, apparentPower, frame.Timestamp);

						if (AlwaysProduceResult || !double.IsNaN(realPowerMeasurement.Value))
						{
							outputMeasurements.Add(realPowerMeasurement);
							calculations++;
							_lastRealPowerCalculations.Enqueue(realPowerMeasurement);
							while (_lastRealPowerCalculations.Count > ValuesToTrack)
							{
								_lastRealPowerCalculations.Dequeue();
							}
						}
						if (AlwaysProduceResult || !double.IsNaN(reactivePowerMeasurement.Value))
						{
							outputMeasurements.Add(reactivePowerMeasurement);
							calculations++;
							_lastReactivePowerCalculations.Enqueue(reactivePowerMeasurement);
							while (_lastReactivePowerCalculations.Count > ValuesToTrack)
							{
								_lastReactivePowerCalculations.Dequeue();
							}
						}
						if (AlwaysProduceResult || !double.IsNaN(activePowerMeasurement.Value))
						{
							outputMeasurements.Add(activePowerMeasurement);
							calculations++;
							_lastActivePowerCalculations.Enqueue(activePowerMeasurement);
							while (_lastActivePowerCalculations.Count > ValuesToTrack)
							{
								_lastActivePowerCalculations.Dequeue();
							}
						}
					}

					lastCalculationTimeStopwatch.Stop();
					_averageCalculationTime.AddValue(lastCalculationTimeStopwatch.Elapsed.TotalMilliseconds);
				}
			}

			totalCalculationTimeStopwatch.Stop();
			_lastTotalCalculationTime = totalCalculationTimeStopwatch.Elapsed.TotalMilliseconds;
			_averageTotalCalculationTime.AddValue(totalCalculationTimeStopwatch.Elapsed.TotalMilliseconds);
			_lastTotalCalculations = calculations;
			_averageCalculationsPerFrame.AddValue(calculations);

			OnNewMeasurements(outputMeasurements);
		}

		private Measurement AddOutputMeasurement(Guid signalId)
		{
			var measurement = GetMeasurement(signalId);
			if (measurement != null)
				OutputMeasurements = (OutputMeasurements ?? Enumerable.Empty<IMeasurement>()).Concat(Enumerable.Repeat(measurement, 1)).ToArray();

			return measurement;
		}

		private Measurement GetMeasurement(Guid signalId)
		{
			var rows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SignalID = '{0}'", signalId));
			if (!rows.Any()) return null;

			var meas = new Measurement();
			meas.Key = MeasurementKey.LookUpBySignalID(signalId);
			meas.TagName = rows[0]["PointTag"].ToString();
			meas.Adder = Convert.ToDouble(rows[0]["Adder"]);
			meas.Multiplier = Convert.ToDouble(rows[0]["Multiplier"]);
			return meas;
		}
	}
}
