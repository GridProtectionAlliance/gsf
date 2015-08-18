package TVA.Hadoop.Datamining.Tools.Weka.ValueObjects;

/**
 * Group of Stats objects
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class StatsList
{
	public Stats[] Stats;
	
	/**
	 * Constructor
	 * @param length	Number of Stats objects in this list
	 */
	public StatsList(int length)
	{
		Stats = new Stats[length];
	}
	
	/**
	 * Calculates the average accuracy
	 * 
	 * @return	The average accuracy of all of the Stats objects in the list
	 */
	public double AverageAccuracy()
	{
		double running_total = 0;
		
		for (int i = 0; i < Stats.length; i++)
			running_total += Stats[i].Accuracy;
		
		return (running_total / (double)Stats.length);
	}
	
	/**
	 * Calculates the average Time taken
	 * 
	 * @return	The average time taken of all of the Stats objects in the list
	 */
	public double AverageTime()
	{
		double running_total = 0;
		
		for (int i = 0; i < Stats.length; i++)
			running_total += Stats[i].TimeInSeconds;
		
		return (running_total / (double)Stats.length);
	}
	
	/**
	 * Returns the string representation of the pass with all data presented
	 * 
	 * @return String representation of the pass
	 */
	public String toFullString()
	{
		String return_value = "";
		
		for (int i = 0; i < Stats.length; i++)
			return_value += Stats[i].toString() + "\r\n";
		
		return_value += "Average:" + "\r\n" + new Stats(AverageAccuracy(), AverageTime()).toString();
		
		return return_value;
	}
	
	/**
	 * Returns the string representation of the pass
	 * 
	 * @return String representation of the pass
	 */
	public String toString()
	{
		return new Stats(AverageAccuracy(), AverageTime()).toString();
	}
}
