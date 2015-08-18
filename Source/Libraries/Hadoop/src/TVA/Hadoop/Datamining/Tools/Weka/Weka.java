package TVA.Hadoop.Datamining.Tools.Weka;

import edu.hawaii.jmotif.lib.ts.TSException;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import weka.core.Attribute;
import weka.core.FastVector;
import weka.core.Instance;
import weka.core.Instances;

/**
 * Weka interface
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class Weka
{
	private FastVector FeatureVector;
	
	/**
	 * Default constructor
	 */
	public Weka()
	{
	}
	
	/**
	 * Create instances of records from a CSV file such that the first n - 1 columns are features and the nth
	 * column is the class of the record
	 * 
	 * @param dimensions	Number of symbols in the representation
	 * @param cardinality	Number of symbols in the alphabet
	 * @param file_name		Data to load
	 * @param labeled		Boolean specifying whether or not the input data is labeled
	 * @return				Weka Instances group
	 * @throws Exception
	 */
	public Instances generateInstances(int dimensions, int cardinality, String file_name, boolean labeled) throws Exception
	{
		ArrayList<Record> records = loadData(dimensions, cardinality, file_name, labeled);
		createFastVector(getClasses(records));
		
		// Initialize the instances
		Instances instances = new Instances("weka_data", FeatureVector, records.size());
		instances.setClassIndex(FeatureVector.size() - 1);
		Instance instance;
		
		for (int i = 0; i < records.size(); i++)
		{
			// Write the features
			instance = new Instance(FeatureVector.size());
			instance.setValue((Attribute)FeatureVector.elementAt(0), records.get(i).SAXRepresentation);
			instance.setValue((Attribute)FeatureVector.elementAt(1), records.get(i).ClassID);
			instance.setDataset(instances);
			instances.add(instance);
		}
		
		return instances;
	}
	
	/**
	 * Creates a single instance from a CSV string containing a single record.
	 * 
	 * @param dimensions	Number of symbols in the representation
	 * @param cardinality	Number of symbols in the alphabet
	 * @param csv_data		String containing the features in CSV format
	 * @param labeled		Boolean specifying whether or not the input data is labeled
	 * @return				Weka Instance representing the data
	 * @throws TSException
	 */
	public Instance generateInstance(int dimensions, int cardinality, String csv_data, boolean labeled) throws TSException
	{
		Record record = new Record(0, csv_data, dimensions, cardinality, labeled);
		createFastVector(record.ClassID);
		
		// Initialize the instances
		Instances instances = new Instances("weka_data", FeatureVector, 1);
		instances.setClassIndex(FeatureVector.size() - 1);
		
		Instance instance = new Instance(FeatureVector.size());
		instance.setValue((Attribute)FeatureVector.elementAt(0), record.SAXRepresentation);
		instance.setValue((Attribute)FeatureVector.elementAt(1), record.ClassID);
		instance.setDataset(instances);
		instances.add(instance);
		
		return instance;
	}

	/**
	 * Create the FastVector object for the records
	 * 
	 * @param classes	Classes in the data
	 */
	private void createFastVector(ArrayList<String> classes)
	{
		FastVector class_vector = new FastVector(classes.size());
		for (int i = 0; i < class_vector.capacity(); i++)
			class_vector.addElement(classes.get(i));
		
		FeatureVector = new FastVector(2);
		FeatureVector.addElement(new Attribute("sax_representation", (FastVector)null));
		FeatureVector.addElement(new Attribute("classification", class_vector));
	}
	
	/**
	 * Create the FastVector object for the records
	 * 
	 * @param class_id	Class in the data
	 */
	private void createFastVector(String class_id)
	{
		FastVector class_vector = new FastVector(1);
		class_vector.addElement(class_id);
		
		FeatureVector = new FastVector(2);
		FeatureVector.addElement(new Attribute("sax_representation", (FastVector)null));
		FeatureVector.addElement(new Attribute("classification", class_vector));
	}
	
	/**
	 * Load Record instances from the comma separated value file organized such that the final element of any row is the class
	 * 
	 * @param dimensions	Number of symbols in the representation
	 * @param cardinality	Number of symbols in the alphabet
	 * @param file_name		Data to load
	 * @param labeled		Boolean specifying whether or not the input data is labeled
	 * @return				ArrayList of Record instances
	 * @throws IOException
	 * @throws TSException
	 */
	private ArrayList<Record> loadData(int dimensions, int cardinality, String file_name, boolean labeled) throws IOException, TSException
	{
		// Read the file
		BufferedReader reader = new BufferedReader(new FileReader(new File(file_name)));
		String line = "";
		int i = 0;
		ArrayList<Record> records = new ArrayList<Record>();
		
		// Iterate through the file and create SAX records
		while ((line = reader.readLine()) != null )
			records.add(new Record(i++, line, dimensions, cardinality, labeled));
		
		reader.close();
		return records;
	}

	/**
	 * Determine the classes that exist in the data
	 * 
	 * @param records	Array of records
	 * @return
	 */
	private ArrayList<String> getClasses(ArrayList<Record> records)
	{
		ArrayList<String> classes = new ArrayList<String>();
		boolean class_found;
		
		// Iterate through all of the records to determine the number of classes
		for (int i = 0; i < records.size(); i++)
		{
			class_found = false;
			// Search through the current array of classes for the class of the current record
			for (int j = 0; j < classes.size(); j++)
				if (records.get(i).ClassID.equals(classes.get(j)))
					class_found = true;
			
			// If the class of the current record does not exist in the class array, add it
			if (!class_found)
				classes.add(records.get(i).ClassID);
		}
		
		return classes;
	}
}