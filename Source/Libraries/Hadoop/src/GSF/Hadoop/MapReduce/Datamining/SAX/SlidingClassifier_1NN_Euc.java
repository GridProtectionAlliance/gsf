package GSF.Hadoop.MapReduce.Datamining.SAX;

//import edu.hawaii.jmotif.lib.ts.TSException;
import edu.hawaii.jmotif.datatype.TSException;

import java.io.IOException;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.conf.Configured;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.IntWritable;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.Text;
//import org.apache.hadoop.mapred.FileInputFormat;
//import org.apache.hadoop.mapred.FileOutputFormat;
import org.apache.hadoop.mapreduce.lib.input.FileInputFormat;
import org.apache.hadoop.mapreduce.lib.output.FileOutputFormat;
//import org.apache.hadoop.mapred.JobClient;
//import org.apache.hadoop.mapred.JobConf;
import org.apache.hadoop.mapreduce.Job;
//import org.apache.hadoop.mapred.MapReduceBase;
//import org.apache.hadoop.mapred.Mapper;
import org.apache.hadoop.mapreduce.Mapper;
//import org.apache.hadoop.mapred.OutputCollector;
//import org.apache.hadoop.mapred.Reducer;
import org.apache.hadoop.mapreduce.Reducer;
//import org.apache.hadoop.mapred.Reporter;
import org.apache.hadoop.util.Tool;
import org.apache.hadoop.util.ToolRunner;

import weka.core.Attribute;
import weka.core.FastVector;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.neighboursearch.BallTree;

import GSF.Hadoop.MapReduce.Datamining.Weka.WekaUtils;
import GSF.Hadoop.MapReduce.Historian.HistorianInputFormat;
import GSF.Hadoop.MapReduce.Historian.File.StandardPointFile;

/**
 * Map Reduce application for use with hadoop which slides a window along a time series dataset looking for a particular pattern.
 * 
 * @author Josh Patterson
 ^ @revised 09/12/2017 Song Zhang - refactor code to use the up-to-date Hadoop APIs
 * @version 0.2.0
 */
public class SlidingClassifier_1NN_Euc extends Configured implements Tool {


	 public static class MapClass extends Mapper<LongWritable, StandardPointFile, Context context> {
	    
		//static enum ExCounter { DISCARDED, MAPPED };
		//private JobConf configuration;
                private Configuration configuration;
		private int iPointTypeID;
		
	    @Override         //member method configuration has been deprecated
	    protected void setup(Context context) throws IOException, InterruptedException {
	    	
	    	System.out.println("Map.setup();");
	    	this.configuration = context.getConfiguration();
	    	
	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID", "-1" ) );
	    
	    	this.iPointTypeID = this.configuration.getInt( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID", 0 );
	    	
	    }
	    
		
		public static String CalculateIndexRowKeyString(int iPointID, long lTimeStamp)
		{
			// convert to string, return bytes
			// make sure to pad the numbers for the keyspace:
			return "p" + String.format("%012d", iPointID) + "_t"
					+ String.format("%022d", CalculateHourBlock(lTimeStamp));
		}
		
		public static long CalculateHourBlock(long point_time_stamp_ms)
		{
			// this effectively truncates the value
			long hour_block_ts = (point_time_stamp_ms / 3600000) * 3600000;
			return hour_block_ts;
		}			
				
		// we need to separate out points into time buckets here
	    public void map(LongWritable key, StandardPointFile value, Context context) throws IOException {

	    	if ( this.iPointTypeID == value.iPointID ) {
	    		
	    		context.write( new Text( CalculateIndexRowKeyString( value.iPointID, value.GetCalendar().getTimeInMillis() ) ), value);
	    		
	    	} // if
	    	
	    } // map method
	  
	 } // inner class	
 
 /**
  * Reducer needs to load up the classifier
  * then 
  */
	public static class Reduce extends Reducer<Text, StandardPointFile, Text, IntWritable> {

	    //private JobConf configuration;
            private Configuration configuration;
	    
	    @Override
	    protected void setup(Context context) throws IOException, InterruptedException {
	    
	    	System.out.println("Reduce.setup();");
	    	this.configuration = context.getConfiguration();

	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.windowSize: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.windowSize", "-1" ) );
	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.windowStepSize: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.windowStepSize", "-1" ) );

	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV", "-1" ) );
           
	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance", "-1" ) );

	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.saxDim: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.saxDim", "-1" ) );

	    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.saxCard: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.saxCard", "-1" ) );
	    	
	    	
	    	
	    } // configure()		
		
		 public void reduce(Text key, Iterable<StandardPointFile> values, Context context) throws IOException, InterruptedException {


		    	int count = 0;
		    	int iBucketWindowsProcessed = 0;
		       	StandardPointFile next_point;

		       	System.out.println("\n\n-------------------------- Construct Classifier -----------------------------" );
		       	
		       	

		    	int iTotalInstances = 0;
		    	int iNumberRight = 0;
		    	int iOscillationsFound = 0;
		    	int iTotalCompared = 0;
		    	
		    	
		    	int iDimensionality = this.configuration.getInt( "gov.tva.mapreduce.ClassifyAnomoly.saxDim", 20 );
		    	int iCardinality = this.configuration.getInt( "gov.tva.mapreduce.ClassifyAnomoly.saxCard", 11 );

		    	// should match the width of your training samples sizes
		    	int iWindowSizeInMS = this.configuration.getInt("gov.tva.mapreduce.ClassifyAnomoly.windowSize", 10 * 1000 );
		    	int iWindowStepSizeInMS = this.configuration.getInt("gov.tva.mapreduce.ClassifyAnomoly.windowStepSize", 5 * 1000 );
		    	String strCsvTrainingSetPath = this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV", "/none/" );
		    	
		    	// this is used when dealing with training instances
		    	int iSamplesPerRawInstance = this.configuration.getInt( "gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance", 10 );
		    	
		    	BallTree oBT_Classifier = null;
		    	FastVector fvWekaAttributes = null;
	    		ArrayList<SAXInstance> arSAXTrainingInstances = null;
		    	
		    	try {

		    		fvWekaAttributes = WekaUtils.Generate_MapReduce_Timeseries_WekaSetup();
		    		
		    		if ( null == fvWekaAttributes ) {
		    			System.out.println( "BallTree > fvWekaAttributes is NULL!" );
		    		} else {
		    			System.out.println( "BallTree > fvWekaAttributes attribute count: " + fvWekaAttributes.size() );
		    		}
		    		
		    		
		    		try {
		    			
		    			// strCsvTrainingSetPath
		    			//arSAXTrainingInstances = WekaUtils.LoadTrainingInstancesCSV_FromHDFS( "/user/output/jpatterson/test_instances/hdfs_waveform_test_c200.csv", iDimensionality, iCardinality, 21 );
		    			arSAXTrainingInstances = WekaUtils.LoadTrainingInstancesCSV_FromHDFS( strCsvTrainingSetPath, iDimensionality, iCardinality, iSamplesPerRawInstance );
		    			
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
		    	
		    	
		    	
		    	if ( null == arSAXTrainingInstances ) {
		    		
		    		System.out.println( "Error > Training Instances were null, quitting..." );
		    		
		    		return;
		    	}
		    	
		    	
		    	
		    	System.out.println( "BallTree > [Reduce] > Ball Tree Constructed!" );
		    	
		    	System.out.println( "Number Instances Loaded Into BallTree: " + oBT_Classifier.getInstances().numInstances() );
		    	
		    	Instances oWekaTestInstances = new Instances( "Test", fvWekaAttributes, 200 );
					oWekaTestInstances.setClassIndex( 1 );
		    	
							       	
		       	
		    	System.out.println("-------------------------- REDUCE -----------------------------" );
		    	
		    	BucketSlidingWindow windowBucket = new BucketSlidingWindow( iWindowSizeInMS, iWindowStepSizeInMS );
		    	
		    	//while (values.hasNext()) {
                        for (StandardPointFile p_val : values) { 

		        		
	        		//next_point = values.next();
	        		
	        		//StandardPointFile p_copy = new StandardPointFile();
	        		//	p_copy.Flags = next_point.Flags;
	        		//	p_copy.iPointID = next_point.iPointID;
	        		//	p_copy.iTimeTag = next_point.iTimeTag;
	        		//	p_copy.Value = next_point.Value;
		    	
	        			
	        			windowBucket.AddPoint(p_val);
	        			
		    	} // while			 
			 
		    	System.out.println("Bucket construction > " + key + " > " + windowBucket.GetHeapPointCount() ); 
		    	
		    	System.out.println( "Scanning bucket" );
	    	
		    	while ( windowBucket.hasNext() ) {
   		
		    		
		    		try {
		    			
		    			windowBucket.SlideWindowForward();
		    			LinkedList<StandardPointFile> oWindow = windowBucket.GetCurrentWindow();
		    			
		    			SAXInstance oSaxInstance = WekaUtils.GenerateInstanceFromPointWindow( oWindow, iDimensionality, iCardinality );
		    			
		    			if ( iBucketWindowsProcessed < 5 ) {
		    				System.out.println( "SAX: " + oSaxInstance.SAX_Representation );
		    			}
		    			
		    			if ( null != oWindow ) {
		    			//	System.out.println( "Window > size: " + oWindow.size() + ", Head TS: " + oWindow.getFirst().GetCalendar().getTimeInMillis() + ", Last TS: " + oWindow.getLast().GetCalendar().getTimeInMillis() );
		    			} else {
		    				System.out.println( "Window > null!" );
		    			}

		    			
		    			
			    		
			    		Instance oWekaInstance = new Instance( 2 );
			    		
			    			oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(0), oSaxInstance.SAX_Representation );      
			    			oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(1), "0" ); //oSaxInstance.ClassID );

			    		oWekaInstance.setDataset( oWekaTestInstances );
		    			oWekaTestInstances.add( oWekaInstance );	
			    			
			    			
			    		//	System.out.println( "SAX > " + iTotalInstances + " > sax: " + oWekaInstance.stringValue(0) + ", class: " + oWekaInstance.stringValue(1) );
			    			
							Instance oNN;
							
							try {
								
								iTotalCompared++;
								
								oNN = oBT_Classifier.nearestNeighbour( oWekaInstance );

					    		
								int iNNClassValue = (int) oNN.classValue();
								
								if ( iNNClassValue == 1) {
									iOscillationsFound++;
								}
								
					
							} catch (Exception e) {
								// TODO Auto-generated catch block
								e.printStackTrace();
							}			    		
		
				    		
				    		//iTotalInstances++;		    			
		    			
		    			
		    			iBucketWindowsProcessed++;
		    			
		    		} catch (Exception e) {
		    			e.printStackTrace();
		    			break;
		    		}
		    		
		    	} // while
		    	
		    	System.out.println( "Windows Scanned > " + iBucketWindowsProcessed + ", Oscillations Detected: " + iOscillationsFound + ", iTotalCompared: " + iTotalCompared );
		    	
		    	
		    	
		    	
		 } // reduce
   
 } // static class Reduce
 
	
	
	
	
	
	
	
	
	
	
	
	
 static int printUsage() {
   //System.out.println("ClassifyAnomoly_1NN_EUC [-m <maps>] [-r <reduces>] <input> <output>");
   System.out.println("ClassifyAnomoly_1NN_EUC [-r <reduces>] <input> <output>");  //number of map tasks is not determined by users since Hadoop 2.0
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
	  
   //JobConf conf = new JobConf( getConf(), SlidingClassifier_1NN_Euc.class );
   Configuration conf = getConf();
   //conf.setJobName("SlidingClassifier_1NN_Euc");
   
   //conf.setMapOutputKeyClass(Text.class);
   //conf.setMapOutputValueClass(StandardPointFile.class);

   //conf.setMapperClass(MapClass.class);        
   //conf.setReducerClass(Reduce.class);
   
   //conf.setInputFormat( HistorianInputFormat.class );
   
   List<String> other_args = new ArrayList<String>();
   for(int i=0; i < args.length; ++i) {
     try {
       if ("-windowSize".equals(args[i])) {
       
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.windowSize", args[++i] );
       	
       } else if ("-windowStepSize".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.windowStepSize", args[++i] );

       } else if ("-trainingSetCSV".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV", args[++i] );

       } else if ("-samplesPerRawInstance".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance", args[++i] );

       } else if ("-saxDim".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.saxDim", args[++i] );

       } else if ("-saxCard".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.saxCard", args[++i] );

       } else if ("-pointTypeID".equals(args[i])) {
           
       	conf.set("gov.tva.mapreduce.ClassifyAnomoly.pointTypeID", args[++i] );
       	
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

   Job job = Job.getInstance(conf, "SlidingClassifier_1NN_Euc");
   job.setJarByClass(SlidingClassifier_1NN_Euc.class);
   job.setMapOutputKeyClass(Text.class);
   job.setMapOutputValueClass(StandardPointFile.class);
   job.setMapperClass(MapClass.class);
   job.setReducerClass(Reduce.class);
   job.setInputFormatClass(HistorianInputFormat.class);


   List<String> paths = new ArrayList<String>();
   for (int i = 0; i < other_args.size(); ++i) {
     try {
        if ("-r".equals(other_args.get(i))) {
           job.setNumReduceTasks(Integer.parseInt(other_args.get(++i)));
        }
        else {
           paths.add(other_args.get(i));
        }
     }
   }

   // Make sure there are exactly 2 parameters left.
   if (other_args.size() != 2) {
     System.out.println("ERROR: Wrong number of parameters: " +
                        other_args.size() + " instead of 2.");
     return printUsage();
   }
   
   FileInputFormat.setInputPaths( job, paths.get(0) );
   FileOutputFormat.setOutputPath( job, new Path(paths.get(1)) );
       
   //JobClient.runJob(conf);
   
   //return 0;
   
   return job.waitForCompletion(true) ? 0 : 1;
 }
 
 
 public static void main(String[] args) throws Exception {
   
	int res = ToolRunner.run( new Configuration(), new SlidingClassifier_1NN_Euc(), args );
   System.exit(res);
   
 }




	
	
}
