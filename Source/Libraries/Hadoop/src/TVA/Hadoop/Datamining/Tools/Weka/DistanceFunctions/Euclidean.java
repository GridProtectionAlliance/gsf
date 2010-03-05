package TVA.Hadoop.Datamining.Tools.Weka.DistanceFunctions;

import weka.core.EuclideanDistance;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.TechnicalInformationHandler;
import weka.core.neighboursearch.PerformanceStats;

/**
 * Euclidean distance function for classifiers
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
@SuppressWarnings("serial")
public class Euclidean extends EuclideanDistance implements Cloneable, TechnicalInformationHandler
{
	public Instances instances;
	
	/**
	 * Default constructor
	 */
	public Euclidean()
	{
		super();
	}
	
	/**
	 * Sets the Instances group to be compared to
	 * 
	 * @param i	Instances group to be set for the class
	 */
	public void setInstances(Instances i)
	{
		super.setInstances(i);
		instances = i;
	}

	/**
	 * Interface for distance calculation
	 * 
	 * @param first				First instance to compare
	 * @param second			Second instance to compare
	 * @param cutOffValue		Maximum allowed distance
	 * @param PerformanceStats	PerformanceStats
	 */
	public double distance(Instance first, Instance second, double cutOffValue, PerformanceStats stats)
	{
		double distance = 0;
		double diff = 0;
		
		first.setDataset(instances);
		
		try
		{
			diff = calculate(first.stringValue(0).toCharArray(), second.stringValue(0).toCharArray());
		}
		catch (Exception e)
		{
			System.err.println(e);
		}
		
		distance = updateDistance(distance, diff);
		
		if (distance > cutOffValue)
			return Double.POSITIVE_INFINITY;

		return distance;
	}
	
	/**
	 * Actually calculate the distance
	 * 
	 * @param a		First instance in comparison (SAX representation char array)
	 * @param b		Second instance in comparison (SAX representation char array)
	 * @return		Calculated distance
	 * @throws Exception
	 */
	private static double calculate(char[] a, char[] b) throws Exception
	{
		if (a.length != b.length)
			throw new Exception ("unequal lengths");
		
		int distance = 0;
		
		for (int i = 0; i < a.length; i++)
			distance += Math.abs(Character.getNumericValue(a[i]) - Character.getNumericValue(b[i]));
		
		return distance;
	}	
}
