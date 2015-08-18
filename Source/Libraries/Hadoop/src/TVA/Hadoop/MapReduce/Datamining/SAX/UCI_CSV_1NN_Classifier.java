package TVA.Hadoop.MapReduce.Datamining.SAX;


import edu.hawaii.jmotif.lib.ts.TSException;
import edu.hawaii.jmotif.lib.ts.Timeseries;
import edu.hawaii.jmotif.logic.sax.SAXFactory;
import edu.hawaii.jmotif.logic.sax.alphabet.Alphabet;
import edu.hawaii.jmotif.logic.sax.alphabet.NormalAlphabet;


import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Iterator;
import java.util.List;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.conf.Configured;
import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.IntWritable;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.Text;
import org.apache.hadoop.mapred.FileInputFormat;
import org.apache.hadoop.mapred.FileOutputFormat;
import org.apache.hadoop.mapred.JobClient;
import org.apache.hadoop.mapred.JobConf;
import org.apache.hadoop.mapred.MapReduceBase;
import org.apache.hadoop.mapred.Mapper;
import org.apache.hadoop.mapred.OutputCollector;
import org.apache.hadoop.mapred.Reducer;
import org.apache.hadoop.mapred.Reporter;
import org.apache.hadoop.mapred.TextInputFormat;
import org.apache.hadoop.mapred.TextOutputFormat;
import org.apache.hadoop.util.Tool;
import org.apache.hadoop.util.ToolRunner;

import TVA.Hadoop.MapReduce.Datamining.Weka.WekaUtils;

import weka.core.Attribute;
import weka.core.FastVector;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.neighboursearch.BallTree;

public class UCI_CSV_1NN_Classifier extends Configured implements Tool {
	
    private final static IntWritable one = new IntWritable(1);
    private Text locText = new Text();
 


	 public static class MapClass extends MapReduceBase implements Mapper<LongWritable, Text, IntWritable, Text> {
	    
		static enum ExCounter { DISCARDED, MAPPED };
	    
		
		// we need to seperate out points into time buckets here
public void map(LongWritable key, Text value, OutputCollector<IntWritable, Text> output, Reporter reporter) throws IOException {

	String line = value.toString();
	String classID = line.split(",")[21];
	int iClassID = Integer.decode(classID);
	
	output.collect( new IntWritable( 0 ), value );
	reporter.incrCounter( ExCounter.MAPPED, 1 );
	
} // map method
  
 } // inner class	    
 
  
	  
	  
	  
	  
	  
	  
	  
	  
	  
	  
	  
	  
	  
  
  
  
  
  
  
  
  
	 
  public static class Reduce extends MapReduceBase implements Reducer<IntWritable, Text, IntWritable, IntWritable> {

	  
	    public void reduce(IntWritable key, Iterator<Text> values, OutputCollector<IntWritable, IntWritable> output, Reporter reporter) throws IOException {

	    	int iTotalInstances = 0;
	    	int iNumberRight = 0;
	    	int iTotalCompared = 0;
	    	
	    	int iDimensionality = 20;
	    	int iCardinality = 11;

	    	BallTree oBT_Classifier = null;
	    	FastVector fvWekaAttributes = null;
	    	
	    	System.out.println( "BallTree > Reduce_DelayScan" );

try {

	fvWekaAttributes = WekaUtils.Generate_MapReduce_Timeseries_WekaSetup();
	
	if ( null == fvWekaAttributes ) {
		System.out.println( "BallTree > fvWekaAttributes is NULL!" );
} else {
	System.out.println( "BallTree > fvWekaAttributes attribute count: " + fvWekaAttributes.size() );
}

//oBT_Classifier = WekaUtils.ConstructBallTreeClassifier_ForHDFS( "/user/output/jpatterson/training_instances/waveform.csv" );
//oBT_Classifier = WekaUtils.ConstructBallTreeClassifier_ForHDFS( "/user/output/jpatterson/training_instances/hdfs_waveform_train_c500.csv" );
//oBT_Classifier = WekaUtils.ConstructBallTreeClassifier_ForHDFS( "/user/output/jpatterson/training_instances/hdfs_waveform_test_c10.csv" );
//oBT_Classifier = WekaUtils.ConstructBallTreeClassifier_ForHDFS( "/user/output/jpatterson/test_instances/hdfs_waveform_test_c200.csv" );
//oBT_Classifier = WekaUtils.ConstructBallTreeClassifier_ByHandForDebug( "/user/output/jpatterson/test_instances/hdfs_waveform_test_c200.csv" );














//BallTree bt = null;
ArrayList<SAXInstance> arSAXTrainingInstances = null;

try {
	
	arSAXTrainingInstances = WekaUtils.LoadTrainingInstancesCSV_FromHDFS( "/user/output/jpatterson/test_instances/hdfs_waveform_test_c200.csv", iDimensionality, iCardinality, 21 );
	
} catch (NumberFormatException e1) {
	// TODO Auto-generated catch block
	e1.printStackTrace();
} catch (TSException e1) {
	// TODO Auto-generated catch block
	e1.printStackTrace();
} catch (IOException e1) {
	// TODO Auto-generated catch block
	e1.printStackTrace();
}


Instances oWekaTrainingInstances = new Instances( "Training", fvWekaAttributes, 100 );
	oWekaTrainingInstances.setClassIndex( 1 );

	WekaUtils.BuildWekaInstances( fvWekaAttributes, arSAXTrainingInstances, oWekaTrainingInstances );


try {
	oBT_Classifier = WekaUtils.BuildBallTree( oWekaTrainingInstances );
} catch (Exception e) {
	// TODO Auto-generated catch block
		e.printStackTrace();
	}		    		
	
	
	
	
} catch (Exception e) {
	
	System.out.println( "BallTree > [Reduce] > " + e );
	
}

System.out.println( "BallTree > [Reduce] > Ball Tree Constructed!" );

System.out.println( "Number Instances Loaded Into BallTree: " + oBT_Classifier.getInstances().numInstances() );

// this is the point we construct a sliding window;
//		- at each time step, the data in the window is converted into SAX Testing Instance
//		- the Instance is fed to the Ball Tree, and a NN is returned. 
//		- the class of the instance is then assigned the class of the NN returned (unless a distance threshold is exceeded?)










//Instances oWekaTestInstances = new Instances( "Test", fvWekaAttributes, 10 );
Instances oWekaTestInstances = new Instances( "Test", fvWekaAttributes, 200 );
	oWekaTestInstances.setClassIndex( 1 );

	
	
	
while (values.hasNext()) {

	

	Text strCsvLine = values.next();
	
	String strLine = String.copyValueOf( strCsvLine.toString().toCharArray() );
	
	
	//Instance oInstanceToTest = WekaUtils.GenerateInstanceFromPointWindow( strLine, fvWekaAttributes, oWekaTestInstances );
SAXInstance oSaxInstance = WekaUtils.GenerateInstanceFromPointWindow( strLine, iDimensionality, iCardinality );
//SAXInstance oSaxInstance = WekaUtils.GenerateInstanceFromPointWindow( strLine, 21, 11 );



Instance oWekaInstance = new Instance( 2 );

	oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(0), oSaxInstance.SAX_Representation );      
	oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(1), oSaxInstance.ClassID );

oWekaInstance.setDataset( oWekaTestInstances );

	oWekaTestInstances.add( oWekaInstance );	
	
	
//	System.out.println( "SAX > " + iTotalInstances + " > sax: " + oWekaInstance.stringValue(0) + ", class: " + oWekaInstance.stringValue(1) );
	
	Instance oNN;
	
	try {
		
		iTotalCompared++;
		
		oNN = oBT_Classifier.nearestNeighbour( oWekaInstance );

		
		int iNNClassValue = (int) oNN.classValue();
		int iRealClassValue = Integer.parseInt( oSaxInstance.ClassID );
		
		if ( iNNClassValue == iRealClassValue ) {
			
			//output.collect( key, new IntWritable( 1 ) );
			iNumberRight++;
			
		} else {
			
			//output.collect( key, new IntWritable( 1 ) );
			
		}		    								
		
	} catch (Exception e) {
		// TODO Auto-generated catch block
		e.printStackTrace();
	}			    		


			
	
//	System.out.println( "Ball Tree > [Comparison Cycle: " + iTotalInstances + "],  [Node Count: " + oBT_Classifier.getInstances().numInstances() + "] " );
	
	iTotalInstances++;

// now clear it
	
}    // while





System.out.println( "iDimensionality = " + iDimensionality );

System.out.println( "iCardinality = " + iCardinality );


System.out.println( "Number Test Instances: " + oWekaTestInstances.numInstances() );

double dPercentRight = 100.0 * (double)( (double)iNumberRight / (double)iTotalCompared );

System.out.println( "Correct: " + iNumberRight + " out of " + iTotalCompared + ", " + dPercentRight + "%" );


System.out.println( "BallTree > [Reduce] > count: " + iTotalInstances );
	
} // reduce
    
  } // static class Reduce   		  
  	  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
static int printUsage() {
  System.out.println("CSVTest [-m <maps>] [-r <reduces>] <input> <output>");
  ToolRunner.printGenericCommandUsage(System.out);
  return -1;
}



/**
 * The main driver for word count map/reduce program.
 * Invoke this method to submit the map/reduce job.
 * @throws IOException When there is communication problems with the 
 *                     job tracker.
 */
public int run(String[] args) throws Exception {
  
  JobConf conf = new JobConf( getConf(), UCI_CSV_1NN_Classifier.class );
  conf.setJobName("UCI_CSV_1NN_Classifier");
  
  conf.setMapOutputKeyClass(IntWritable.class);
  conf.setMapOutputValueClass(Text.class);

  conf.setMapperClass( MapClass.class );        
  conf.setReducerClass( Reduce.class ); //Reduce_Evaluate.class ); //Reduce.class);
      
  conf.setInputFormat(TextInputFormat.class);
  conf.setOutputFormat(TextOutputFormat.class);
  
  
  List<String> other_args = new ArrayList<String>();
  for(int i=0; i < args.length; ++i) {
    try {
      if ("-m".equals(args[i])) {
    conf.setNumMapTasks(Integer.parseInt(args[++i]));
  } else if ("-r".equals(args[i])) {
    conf.setNumReduceTasks(Integer.parseInt(args[++i]));
  } else {
    other_args.add(args[i]);
  }
} catch (NumberFormatException except) {
  System.out.println("ERROR: Integer expected instead of " + args[i]);
  return printUsage();
} catch (ArrayIndexOutOfBoundsException except) {
  System.out.println("ERROR: Required parameter missing from " +
                         args[i-1]);
      return printUsage();
    }
  }
  // Make sure there are exactly 2 parameters left.
  if (other_args.size() != 2) {
    System.out.println("ERROR: Wrong number of parameters: " +
       other_args.size() + " instead of 2.");
        return printUsage();
      }
      
      FileInputFormat.setInputPaths( conf, other_args.get(0) );
      FileOutputFormat.setOutputPath( conf, new Path(other_args.get(1)) );
          
      JobClient.runJob(conf);
      
      return 0;
    }
    
    
    public static void main(String[] args) throws Exception {
      
    	int res = ToolRunner.run( new Configuration(), new UCI_CSV_1NN_Classifier(), args );
    	System.exit(res);
      
    }    
    
 }
	
		