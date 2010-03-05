package TVA.Hadoop.Datamining.Tools.Weka;
import TVA.Hadoop.Datamining.Tools.Weka.ValueObjects.Stats;
import weka.core.Instance;
import weka.core.Instances;

/**
 * Class containing classifiers for classifying time series data
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class Classifiers
{
	/**
	 * NN Classifier
	 * 
	 * @param dimensions	Number of symbols in the representation
	 * @param cardinality	Number of symbols in the alphabet
	 * @param training_data	Instances object of training data
	 * @param test_data		Instances object of test data
	 * @param folds			Number of folds for cross validation
	 * @param nns			Number of neighbors
	 * @param distance		Distance function to compute
	 * @return				StatsList object containing accuracy and timing data
	 * @throws Exception
	 */
	public static Stats NeighborSearch(Instances training_data, Instances test_data, weka.core.neighboursearch.NearestNeighbourSearch nns, weka.core.DistanceFunction distance) throws Exception
	{
		Instance test_instance;
		int num_correct;
		long before, after;
		
		// Create an instance of the classifier, and prepare the distance function
		distance.setInstances(training_data);
		nns.setDistanceFunction(distance);
		nns.setInstances(training_data);
			
		// Initialize statistic variables
		num_correct = 0;
		before = System.currentTimeMillis();
			
		// For each test instance
		for (int j = 0; j < test_data.numInstances(); j++)
		{
			// Classify
			test_instance = nns.nearestNeighbour(test_data.instance(j));

			// Check for accuracy
			if (test_data.instance(j).classValue() == test_instance.classValue())
				num_correct++;
		}
			
		// Record the runtime
		after = System.currentTimeMillis();			
		
		return new Stats((((double)num_correct / (double)test_data.numInstances()) * 100.0), ((double)(after - before) / 1000.0));
	}
}