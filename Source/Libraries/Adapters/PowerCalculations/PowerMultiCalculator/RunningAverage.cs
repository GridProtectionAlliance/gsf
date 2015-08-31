namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Calculates running average of a value
	/// </summary>
	public class RunningAverage
	{
		private double _numberOfValues = 0;
		private double _average = 0;

		/// <summary>
		/// Average calculated on values provided so far
		/// </summary>
		public double Average
		{
			get { return _average; }
		}

		/// <summary>
		/// Calculates running average based on previous values and the new value
		/// </summary>
		/// <param name="value">Value to be added to the running average</param>
		/// <returns>New running average</returns>
		public double AddValue(double value)
		{
			var total = _numberOfValues * _average + value;
			_numberOfValues++;
			_average = total / _numberOfValues;
			return _average;
		}
	}
}
