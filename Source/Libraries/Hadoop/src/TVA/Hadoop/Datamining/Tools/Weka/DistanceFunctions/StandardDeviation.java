package TVA.Hadoop.Datamining.Tools.Weka.DistanceFunctions;

import weka.core.EuclideanDistance;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.TechnicalInformationHandler;
import weka.core.neighboursearch.PerformanceStats;

/**
 * Standard Deviation distance function for classifiers
 * 
 * @author Galen Riley
 * @version 0.1.0
 */
@SuppressWarnings("serial")
public class StandardDeviation extends EuclideanDistance implements Cloneable, TechnicalInformationHandler
{
	public Instances instances;
	
	/**
	 * Default constructor
	 */
	public StandardDeviation()
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
			return diff;
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
		
		// don't use the class id at the end of the line
		int count = a.length - 1;

		// calculating standard deviation of each input array
		double sum_a = 0.0, sum_b = 0.0;
		double mean_a = 0.0, mean_b = 0.0;
		double[] dev_a = new double[count], dev_b = new double[count];
		double maxdev_a = 0.0, maxdev_b = 0.0;
		double mindev_a = 0.0, mindev_b = 0.0;
		double sumdevs_a = 0.0, sumdevs_b = 0.0;
		double stdev_a = 0.0, stdev_b = 0.0;
		
		for(int i = 0; i < count; i++)
			sum_a += Character.getNumericValue(a[i]);
		for(int i = 0; i < count; i++)
			sum_b += Character.getNumericValue(b[i]);
		
		mean_a = sum_a / count;
		mean_b = sum_b / count;
		
		for(int i = 0; i < count; i++)
		{
			dev_a[i] = (Character.getNumericValue(a[i]) - mean_a) * (Character.getNumericValue(a[i]) - mean_a);
			sumdevs_a += dev_a[i];
			
			if( i == 0 )
				maxdev_a = mindev_a = dev_a[i];
			
			maxdev_a = Math.max(maxdev_a, dev_a[i]);
			mindev_a = Math.min(mindev_a, dev_a[i]);
		}
		stdev_a = Math.sqrt(sumdevs_a / count);
		
		for(int i = 0; i < count; i++)
		{
			dev_b[i] = (Character.getNumericValue(b[i]) - mean_b) * (Character.getNumericValue(b[i]) - mean_b);
			sumdevs_b += dev_b[i];
			
			if( i == 0 )
				maxdev_b = mindev_b = dev_b[i];
			
			maxdev_b = Math.max(maxdev_b, dev_b[i]);
			mindev_b = Math.min(mindev_b, dev_b[i]);
		}
		stdev_b = Math.sqrt(sumdevs_b / count);
		
		// difference approach
		return Math.abs(stdev_b - stdev_a);
	}
}
