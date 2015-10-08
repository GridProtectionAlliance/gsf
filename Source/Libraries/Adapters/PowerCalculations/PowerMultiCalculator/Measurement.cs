using System;

namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Simple model class for measurements
	/// </summary>
	public class Measurement
	{
		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Signal Id
		/// </summary>
		public Guid SignalId { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Point Tag
		/// </summary>
		public string PointTag { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Adder
		/// </summary>
		public int Adder { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Multiplier
		/// </summary>
		public string Multiplier { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Device Id
		/// </summary>
		public int DeviceId { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Historian Id
		/// </summary>
		public int HistorianId { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Signal Type Id
		/// </summary>
		public int SignalTypeId { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s Enabled flag
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the current <see cref="Measurement"/>'s signal reference
		/// </summary>
		public string SignalReference { get; set; }
	}
}
