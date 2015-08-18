package TVA.Hadoop.Datamining.Tools;


import org.apache.commons.logging.LogFactory;
import org.apache.log4j.PropertyConfigurator;
import TVA.Hadoop.Datamining.Tools.Weka.Classifiers;
import TVA.Hadoop.Datamining.Tools.Weka.Classify;
import TVA.Hadoop.Datamining.Tools.Weka.CrossValidate;
import TVA.Hadoop.Datamining.Tools.Weka.Weka;
import TVA.Hadoop.Datamining.Tools.Weka.DistanceFunctions.Euclidean;


/**
 * Main driver for the datamining tools
 * 
 * @author Andy Hill
 * @author Josh Patterson
 * @version 0.1.0
 */
public class DataminingTools
{
	public static final org.apache.commons.logging.Log LOG = LogFactory.getLog(DataminingTools.class);
	
	/**
	 * Test Classifier locally with WEKA
	 * 		
	 * @param training_data		Path to training data
	 * @param test_data			Path to test data
	 * @param test_instance		CSV test instance
	 * @param dimensions		Number of symbols in the representation
	 * @param cardinality		Number of symbols in the alphabet
	 * @param folds				Number of folds for cross validation
	 * @throws Exception
	 */
	public static void TestClassifierLocally(String training_data, String test_data, String test_instance, int dimensions, int cardinality, int folds) throws Exception
	{
		if (!training_data.equals("") && test_data.equals("") && !test_instance.equals("") && (dimensions != -1) && (cardinality != -1) && (folds == -1))
			System.out.println(Classify.NeighborSearch(new Weka().generateInstance(dimensions, cardinality, test_instance, false), new Weka().generateInstances(dimensions, cardinality, training_data, true), new weka.core.neighboursearch.CoverTree(), new Euclidean()));
		
		else if (!training_data.equals("") && test_data.equals("") && test_instance.equals("") && (dimensions != -1) && (cardinality != -1) && (folds != -1))
			System.out.println(CrossValidate.NeighborSearch(new Weka().generateInstances(dimensions, cardinality, training_data, true), new weka.core.neighboursearch.CoverTree(), new Euclidean(), folds));
		
		else if (!training_data.equals("") && !test_data.equals("") && test_instance.equals("") && (dimensions != -1) && (cardinality != -1) && (folds == -1))
			System.out.println(Classifiers.NeighborSearch(new Weka().generateInstances(dimensions, cardinality, training_data, true), new Weka().generateInstances(dimensions, cardinality, test_data, true), new weka.core.neighboursearch.CoverTree(), new Euclidean()));
		
		else
			PrintUsage();
	}
	
	public static void PrintUsage()
	{
		System.out.println("");
		System.out.println("Usage: DataminingTools");

		System.out.println("\tTest Classifier Locally:\t[-testClassifierLocally -train <local_training_instances_path> -test <local_test_instances_path> -inst <test_instance> -dim <sax_dimensions> -card <sax_cardinality> -folds <folds>]");
		System.out.println("\tMove File To HDFS:\t\t[-copyFileToHdfs <local_src_path> <hdfs_dst_path>]");
		System.out.println("");
	}
	
	public static void main(String[] args) throws Exception
	{
    	PropertyConfigurator.configure( "conf/log4j.props");

		if (args.length < 2)
			PrintUsage();
			
		else
		{
			int i = 0;
		    String cmd = args[i++];

		    if ("-testClassifierLocally".equals(cmd))
		    {
		    	String training_data = "", test_data = "", test_instance = "";
		    	int dimensions = -1, cardinality = -1, folds = -1;
		    	
		    	while (i < args.length)
		    	{
		    		if (args[i].equals("-test"))
		    			test_data = args[i + 1];
		    		
		    		else if (args[i].equals("-train"))
		    			training_data = args[i + 1];
		    			
		    		else if (args[i].equals("-dim"))
		    			dimensions = Integer.parseInt(args[i + 1]);
		    		
		    		else if (args[i].equals("-card"))
		    			cardinality = Integer.parseInt(args[i + 1]);
		    		
		    		else if (args[i].equals("-folds"))
		    			folds = Integer.parseInt(args[i + 1]);
		    		
		    		else if (args[i].equals("-inst"))
		    			test_instance = args[i + 1];
		    		
		    		i++;
		    	}
		    	
		    	TestClassifierLocally(training_data, test_data, test_instance, dimensions, cardinality, folds);
		    }
		    
		    else if ("-copyFileToHdfs".equals(cmd))
		    {
		    	// Patterson's stuff
		    }
		    else
		    	PrintUsage();
		}
	}
}
