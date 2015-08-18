package TVA.Hadoop.Datamining.Tools.Weka;
import java.util.Random;

import TVA.Hadoop.Datamining.Tools.Weka.ValueObjects.StatsList;
import weka.core.Instances;

/**
 * Class to run cross validation testing on learning algorithms
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class CrossValidate
{
	/**
	 * Run cross validation testing for Weka nearest neighbor algorithms
	 * 
	 * @param data		Labeled data to test
	 * @param nns		Weka nearest neighbor search algorithm
	 * @param distance	Distance function to be used by the search algorithm
	 * @param folds		Number of folds for cross validation
	 * @return			StatsList object containing accuracy and timing data
	 * @throws Exception
	 */
	public static StatsList NeighborSearch(Instances data, weka.core.neighboursearch.NearestNeighbourSearch nns, weka.core.DistanceFunction distance, int folds) throws Exception
	{
		data.stratify(folds);
		
		StatsList stats = new StatsList(folds);
		Instances training_data, test_data, rand_data = new Instances(data);
		
		for (int i = 0; i < folds; i++)
		{
			rand_data.randomize(new Random((int)System.currentTimeMillis()));
			// Split the data into training and test instances for cross-validation
			training_data = rand_data.trainCV(folds, i);
			test_data = rand_data.testCV(folds, i);
			
			stats.Stats[i] = Classifiers.NeighborSearch(training_data, test_data, nns, distance); 
		}
		
		return stats;
	}
}