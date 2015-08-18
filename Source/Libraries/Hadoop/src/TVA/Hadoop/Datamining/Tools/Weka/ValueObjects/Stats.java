package TVA.Hadoop.Datamining.Tools.Weka.ValueObjects;

import java.text.DecimalFormat;

/**
 * Statistics for a single pass
 * 
 * @author Andy Hill
 * @author Galen Riley
 * @version 0.1.0
 */
public class Stats
{
	public double Accuracy;
	public double TimeInSeconds;
	
	/**
	 * Constructor
	 * 
	 * @param accuracy		Accuracy of the pass
	 * @param time_in_secs	Time taken for the pass
	 */
	public Stats(double accuracy, double time_in_secs)
	{
		Accuracy = accuracy;
		TimeInSeconds = time_in_secs;
	}
	
	/**
	 * Returns the string representation of the pass
	 * 
	 * @return String representation of the pass
	 */
	public String toString()
	{
		DecimalFormat format_accuracy = new DecimalFormat("00.00");
		DecimalFormat format_time = new DecimalFormat("0.000000");
		
		return format_accuracy.format(Accuracy) + "% - " + format_time.format(TimeInSeconds) + " seconds";
	}
}