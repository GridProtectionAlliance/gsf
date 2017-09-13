package GSF.Hadoop.MapReduce.Datamining.SAX;



//import edu.hawaii.jmotif.lib.ts.TSException;
import edu.hawaii.jmotif.datatype.TSException;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.conf.Configured;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.FloatWritable;
import org.apache.hadoop.io.IntWritable;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.Text;
import org.apache.hadoop.io.WritableComparable;
import org.apache.hadoop.io.WritableComparator;
//import org.apache.hadoop.mapred.FileInputFormat;
//import org.apache.hadoop.mapred.FileOutputFormat;
//import org.apache.hadoop.mapred.JobClient;
//import org.apache.hadoop.mapred.JobConf;
//import org.apache.hadoop.mapred.MapReduceBase;
//import org.apache.hadoop.mapred.Mapper;
//import org.apache.hadoop.mapred.OutputCollector;
//import org.apache.hadoop.mapred.Reducer;
//import org.apache.hadoop.mapred.Reporter;
import org.apache.hadoop.mapreduce.lib.input.FileInputFormat;
import org.apache.hadoop.mapreduce.lib.output.FileOutputFormat;
import org.apache.hadoop.mapreduce.Job;
import org.apache.hadoop.mapreduce.Mapper;
import org.apache.hadoop.mapreduce.Reducer;
import org.apache.hadoop.util.Tool;
import org.apache.hadoop.util.ToolRunner;
//import org.apache.hadoop.mapred.Partitioner;
import org.apache.hadoop.mapreduce.Partitioner;

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
 * @author Josh Patterson
 * @revised 09/12/2017 Song Zhang (ISO New England) - refactor code to use up-to-date Hadoop APIs
 * @version 0.2.0
 */
public class SlidingTSClassifier_kNN extends Configured implements Tool {

		  
		  /**
		   * Partition based on the first part of the pair.
		   */
		  public static class FirstPartitioner extends Partitioner<TimeSeriesKey,StandardPointFile>{
			  
		    @Override
		    public int getPartition(TimeSeriesKey key, StandardPointFile value, int numPartitions) {
		      return Math.abs( key.getPointID() * 127) % numPartitions;
		    }

			//@Override
			//public void configure(JobConf arg0) {
				// TODO Auto-generated method stub
				
			//}
		  }
		  


		  /**
		   * Compare only the first part of the pair, so that reduce is called once
		   * for each value of the first part.
		   */
		  public static class KeyComparator extends WritableComparator {
			  
			    protected KeyComparator() {
			        super(TimeSeriesKey.class, true);
			      }
			      @Override
			      public int compare(WritableComparable w1, WritableComparable w2) {

			    	  TimeSeriesKey ip1 = (TimeSeriesKey) w1;
			    	  TimeSeriesKey ip2 = (TimeSeriesKey) w2;
			    	  int cmp = ip1.getPointID() == ip2.getPointID() ? 0 : (ip1.getPointID() < ip2.getPointID() ? -1 : 1);
			        if (cmp != 0) {
			          return cmp;
			        }
			        
			        return ip1.getTimestamp() == ip2.getTimestamp() ? 0 : (ip1.getTimestamp() < ip2.getTimestamp() ? -1 : 1);
			        
			      }
	  
			  
		  }	  
		  
		  
		  
		  
		  
		  // only should compare point IDs
		  public static class ValueGroupingComparator extends WritableComparator {
		  
			  protected ValueGroupingComparator() {
				  super( TimeSeriesKey.class, true );
			  }
			  
				@Override
				public int compare(WritableComparable o1, WritableComparable o2) {
					
					TimeSeriesKey tsK1 = (TimeSeriesKey)o1;
					TimeSeriesKey tsK2 = (TimeSeriesKey)o2;
				
				     int l = tsK1.getPointID();
				     int r = tsK2.getPointID();
				     return l == r ? 0 : (l < r ? -1 : 1);
				
					
				}	


		  }	  	  
		  
		  
		  public static class TimeSeriesKey implements WritableComparable<TimeSeriesKey> {

			private int PointID = 0;
			private long Timestamp = 0;
			
			public void set( int iPointID, long lTS ) {
				
				this.PointID = iPointID;
				this.Timestamp = lTS;
				
			}
			
			public int getPointID() {
				return this.PointID;
			}
			
			public long getTimestamp() {
				return this.Timestamp;			
			}
			
			public String Debug() {
				
				return "k: " + this.PointID + "_" + this.Timestamp;
				
			}
			
			@Override
			public void readFields(DataInput in) throws IOException {

				this.PointID = in.readInt();
				this.Timestamp = in.readLong();
			}

			@Override
			public void write(DataOutput out) throws IOException {

				out.writeInt( this.PointID );
				out.writeLong( this.Timestamp );
				
			}

			@Override
			public int compareTo(TimeSeriesKey other) {

			      if (this.PointID != other.PointID) {
			        return PointID < other.PointID ? -1 : 1;
			      } else if (this.Timestamp != other.Timestamp) {
			        return Timestamp < other.Timestamp ? -1 : 1;
			      } else {
			        return 0;
			      }			
				
			}

			
			
			
			
	 
		    public static class TimeSeriesKeyComparator extends WritableComparator {
		      public TimeSeriesKeyComparator() {
		        super(TimeSeriesKey.class);
		      }

		      public int compare(byte[] b1, int s1, int l1,
		                         byte[] b2, int s2, int l2) {
		        return compareBytes(b1, s1, l1, b2, s2, l2);
		      }
		    }

		    static {                                        // register this comparator
		      WritableComparator.define( TimeSeriesKey.class, new TimeSeriesKeyComparator() );
		    }

			
			
		} // end of static class
		
		
		
		
		
		
		
		
		 public static class MapClass extends Mapper<LongWritable, StandardPointFile, TimeSeriesKey, StandardPointFile> {
		    
			//static enum ExCounter { DISCARDED, MAPPED };
			//private JobConf configuration;
                        private Configuration configuration;
			private int iPointTypeID;
		    private final TimeSeriesKey key = new TimeSeriesKey();

			
		    @Override
		    //public void configure(JobConf job) {
                    protected void setup(Context context) throws IOException, InterruptedException {     //member method configure has been deprecated
		    	
		    	System.out.println("Map.setup();");
		    	this.configuration = context.getConfiguration();
		    	
		    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID", "-1" ) );
		    
		    	this.iPointTypeID = this.configuration.getInt( "gov.tva.mapreduce.ClassifyAnomoly.pointTypeID", 0 );
		    	
		    }
		    

		    public void map(LongWritable key, StandardPointFile value, Context context) throws IOException, InterruptedException {

		    	if ( this.iPointTypeID == value.iPointID ) {

		    		this.key.set( value.iPointID, value.GetCalendar().getTimeInMillis() );
		    		
		    		context.write( this.key, value );
		    		
		    	} // if
		    	
		    } // map method
		  
		 } // inner class	
	 
	 	
		
		

		 /**
		  * Reducer needs to load up the classifier
		  * then 
		  */
			public static class Reduce_SlidingWindow extends Reducer<TimeSeriesKey, StandardPointFile, Text, LongWritable> {

			    //private JobConf configuration;
                            private Configuration configuration;
			    
			    @Override
			    //public void configure(JobConf job) {
                            protected void setup(Context context) throws IOException, InterruptedException {
			    
			    	System.out.println("Reduce.configure();");
			    	this.configuration = context.getConfiguration();

			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.windowSize: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.windowSize", "-1" ) );
			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.windowStepSize: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.windowStepSize", "-1" ) );

			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.trainingSetCSV", "-1" ) );
		           
			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.samplesPerRawInstance", "-1" ) );

			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.saxDim: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.saxDim", "-1" ) );

			    	System.out.println( "gov.tva.mapreduce.ClassifyAnomoly.saxCard: " + this.configuration.get( "gov.tva.mapreduce.ClassifyAnomoly.saxCard", "-1" ) );
			    	
			    	
			    	
			    } // configure()		
				
				 public void reduce(TimeSeriesKey key, Iterable<StandardPointFile> values, Context context) throws IOException, InterruptedException {


				    	int count = 0;
				    	int iBucketWindowsProcessed = 0;
				       	//StandardPointFile next_point;
	 	
				       	

				    	int iTotalInstances = 0;
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

			    		long lPrev = 0;
			    		long lCurr = 0;

			    	
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
				    	
			    		System.out.println( "Reducer > Key: " + key.Debug() + " ----------------------------- " );

			    		ContinuousSlidingWindow sliding_window = new ContinuousSlidingWindow( iWindowSizeInMS, iWindowStepSizeInMS );
			    		
			    		
			    		
			    		//while () 
				    	//while (values.hasNext()) {
                                        for (StandardPointFile p_val : values) {

			        		/*while ( sliding_window.WindowIsFull() == false && values.hasNext() ) {
				        						    		
				        		next_point = values.next();
//				        		lCurr = next_point.GetCalendar().getTimeInMillis();

				        		StandardPointFile p_copy = new StandardPointFile();
				        			p_copy.Flags = next_point.Flags;
				        			p_copy.iPointID = next_point.iPointID;
				        			p_copy.iTimeTag = next_point.iTimeTag;
				        			p_copy.Value = next_point.Value;
				        		
				
				        		sliding_window.AddPoint( p_copy );
				        	
//				        		lPrev = lCurr;
				        		count++;
				    			
		        			
			        		}*/
                                                
                                                // keep adding points to the sliding window until it's full
                                                if ( !sliding_window.WindowIsFull() ) {
                                                    sliding_window.AddPoint( p_val );
                                                }
			        		
			        		//if ( sliding_window.WindowIsFull() ) {
                                                else {
			        			// process the window
			        			
			        			// 1. generate instance
			        					        			
			        			LinkedList<StandardPointFile> oWindow = sliding_window.GetCurrentWindow();
			        			iTotalInstances++;
			        			
				    			SAXInstance oSaxInstance = WekaUtils.GenerateInstanceFromPointWindow( oWindow, iDimensionality, iCardinality );
				    			
				    			if ( iBucketWindowsProcessed < 5 ) {
				    				System.out.println( "SAX: " + oSaxInstance.SAX_Representation );
				    			}
				    			
				    			if ( null != oWindow ) {

				    			} else {
				    				System.out.println( "Window > null!" );
				    			}

				    			
				    			
					    		
					    		Instance oWekaInstance = new Instance( 2 );
					    		
					    			oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(0), oSaxInstance.SAX_Representation );      
					    			oWekaInstance.setValue( (Attribute)fvWekaAttributes.elementAt(1), "0" ); //oSaxInstance.ClassID );

					    		oWekaInstance.setDataset( oWekaTestInstances );
				    			oWekaTestInstances.add( oWekaInstance );	
					    			
					    			
									Instance oNN;
									
									try {
										
										iTotalCompared++;
										
										oNN = oBT_Classifier.nearestNeighbour( oWekaInstance );

							    		
										int iNNClassValue = (int) oNN.classValue();
										
										if ( iNNClassValue == 1) {
											iOscillationsFound++;
											context.write( new Text("1"), new LongWritable( oWindow.getFirst().GetCalendar().getTimeInMillis() ) );
										}
										
							
									} catch (Exception e) {
										// TODO Auto-generated catch block
										e.printStackTrace();
									}			    		
				
									
					    			
				    			iBucketWindowsProcessed++;		        			
			        			
			        			// 2. remove front half of window
			        			
			        			sliding_window.SlideWindowForward();
			        			
			        		}
			        		
			        		
				    	} // for loop				    		
			    		
				    	
				    	
			    		System.out.println( "Reducer > Count: " + count + ", Windows: " + iTotalInstances + ", Oscillations: " + iOscillationsFound );
				    	
			    		
				    	
				    	
				 } // reduce
		   
		 } // static class Reduce
		 
		
		
		
		
		
	 static int printUsage() {
	   //System.out.println("SlidingTSClassifier_kNN [-m <maps>] [-r <reduces>] <input> <output>");
           System.out.println("SlidingTSClassifier_kNN [-r <reduces>] <input> <output>");  //the number of map tasks is not user-defined any more
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
		  
	   //JobConf conf = new JobConf( getConf(), SlidingTSClassifier_kNN.class );
           Configuration conf = getConf();
	   //conf.setJobName( "SlidingTSClassifier_kNN" );
	   
	   //conf.setMapOutputKeyClass( TimeSeriesKey.class );
	   //conf.setMapOutputValueClass( StandardPointFile.class );

	   //conf.setMapperClass( MapClass.class );        
	   //conf.setReducerClass( Reduce_SlidingWindow.class );
	 
	   // group and partition by the first int in the pair
	   //conf.setPartitionerClass(FirstPartitioner.class);
	   //conf.setOutputKeyComparatorClass(KeyComparator.class);
	   //conf.setOutputValueGroupingComparator(ValueGroupingComparator.class);
	   
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

           Job job = Job.getInstance(conf, "SlidingTSClassifier_kNN");
           job.setJarByClass( SlidingTSClassifier_kNN.class );
           job.setMapOutputKeyClass( TimeSeriesKey.class );
           job.setMapOutputValueClass( StandardPointFile.class );
           job.setMapperClass( MapClass.class );
           job.setReducerClass( Reduce_SlidingWindow.class );
           job.setPartitionerClass(FirstPartitioner.class);
           job.setSortComparatorClass(KeyComparator.class);
           job.setGroupingComparatorClass(ValueGroupingComparator.class);
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
	   FileOutputFormat.setOutputPath( job, new Path(path.get(1)) );
	       
	   //JobClient.runJob(conf);
	   
	   //return 0;

           return job.waitForCompletion(true) ? 0 : 1;
	 }
	 
	 
	 public static void main(String[] args) throws Exception {
	   
		int res = ToolRunner.run( new Configuration(), new SlidingTSClassifier_kNN(), args );
	    System.exit(res);
	 
	 }


		
		
	}
