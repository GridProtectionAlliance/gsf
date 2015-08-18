package TVA.Hadoop.Datamining.Tools.Weka;

import weka.core.Instance;
import weka.core.Instances;

/**
 * Class to attempt to classify a single instance based on a set of training instances for a
 * given classifier
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class Classify
{
	/**
	 * Classify a single test instance based on a set of training instances using a Weka
	 * nearest neighbor search algorithm
	 * 
	 * @param test_instance		Weka Instance to classify
	 * @param training_data		Weka Instances group of training data
	 * @param nns				Weka nearest neighbor search algorithm
	 * @param distance			Distance function to be used by the search algorithm
	 * @return					The perceived classification of the test instance
	 * @throws Exception
	 */
	public static String NeighborSearch(Instance test_instance, Instances training_data, weka.core.neighboursearch.NearestNeighbourSearch nns, weka.core.DistanceFunction distance) throws Exception
	{
		// Create an instance of the classifier, and prepare the distance function
		distance.setInstances(training_data);
		nns.setDistanceFunction(distance);
		nns.setInstances(training_data);
		
		return nns.nearestNeighbour(test_instance).stringValue(test_instance.classIndex());
	}
}