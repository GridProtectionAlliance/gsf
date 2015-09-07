using GSF.TimeSeries;

namespace PowerCalculations.PowerMultiCalculator
{
	public class PowerCalculation
	{
		public int PowerCalculationId { get; set; }
		public string CircuitDescription { get; set; }
		public MeasurementKey VoltageAngleSignalId { get; set; }
		public MeasurementKey VoltageMagnitudeSignalId { get; set; }
		public MeasurementKey CurrentAngleSignalId { get; set; }
		public MeasurementKey CurrentMagnitudeSignalId { get; set; }
		public IMeasurement RealPowerOutputMeasurement { get; set; }
		public IMeasurement ReactivePowerOutputMeasurement { get; set; }
		public IMeasurement ActivePowerOutputMeasurement { get; set; }
	}
}
