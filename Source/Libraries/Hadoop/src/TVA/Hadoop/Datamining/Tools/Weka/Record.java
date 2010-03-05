package TVA.Hadoop.Datamining.Tools.Weka;

import edu.hawaii.jmotif.logic.sax.alphabet.NormalAlphabet;
import edu.hawaii.jmotif.logic.sax.SAXFactory;
import edu.hawaii.jmotif.lib.ts.Timeseries;
import edu.hawaii.jmotif.lib.ts.TSException;

/**
 * Instance of a record in the data set
 * 
 * @author Andy Hill
 * @version 0.1.0
 */
public class Record
{
	public int SampleID;
	public String SAXRepresentation;
	public String ClassID;
	
	/**
	 * Default constructor
	 */
	public Record()
	{
		SampleID = -1;
		SAXRepresentation = "";
		ClassID = "";
	}
	
	/**
	 * Overloaded constructor
	 * 
	 * @param id				Unique sample identifier
	 * @param csv_input			Raw data
	 * @param classification	Class of the record
	 * @param dimensions		Number of symbols in the representation
	 * @param cardinality		Number of symbols in the alphabet
	 * @param labeled			Boolean specifying whether or not the input data is labeled
	 * @throws TSException
	 */
	public Record(int id, String csv_input, int dimensions, int cardinality, boolean labeled) throws TSException
	{
		String[] points = csv_input.split(",");
		int num_points = (labeled) ? (points.length - 1) : points.length;
		
		double[] values = new double[num_points];
		long[] timestamps = new long[num_points]; 
		
		for (int i = 0; i < num_points; i++)
		{			
			values[i] = Double.parseDouble(points[i]);
			timestamps[i] = (long) i;
		}
		
		SampleID = id;
		SAXRepresentation = SAXFactory.ts2string(new Timeseries(values, timestamps), dimensions, new NormalAlphabet(), cardinality);
		ClassID = (labeled) ? points[points.length - 1] : "-1";
		
	}
}
