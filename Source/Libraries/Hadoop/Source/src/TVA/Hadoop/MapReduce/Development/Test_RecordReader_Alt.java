package TVA.Hadoop.MapReduce.Development;



import java.io.IOException;
import java.util.StringTokenizer;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import gov.tva.DatAware.hadoop.fs.StandardPointFile;
import gov.tva.mapreduce.DatAware_InputFormat;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.conf.Configured;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.IntWritable;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.DoubleWritable;
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
import org.apache.hadoop.util.Tool;
import org.apache.hadoop.util.ToolRunner;




public class Test_RecordReader_Alt extends Configured implements Tool {

	
	 public static class MapClass extends MapReduceBase implements Mapper<LongWritable, StandardPointFile, IntWritable, StandardPointFile> {

//		 private final static IntWritable one = new IntWritable(1);
	    private int iCount = 0;
	    
	    public void map(LongWritable key, StandardPointFile value, OutputCollector<IntWritable, StandardPointFile> output, Reporter reporter) throws IOException {

	    //	if ( this.iCount < 10 ) {
	    //		this.iCount++;
	    		output.collect( new IntWritable( value.iPointID ), value);
	    //	}
	    	
	    	
	    } // map method
	  
	 } // inner class	
  
  /**
   * A reducer class that just emits the sum of the input values.
   */
  public static class Reduce extends MapReduceBase implements Reducer<IntWritable, StandardPointFile, IntWritable, IntWritable> {
    
    public void reduce(IntWritable key, Iterator<StandardPointFile> values,
                       OutputCollector<IntWritable, IntWritable> output, 
                       Reporter reporter) throws IOException {
    	
    	double dSum = 0;
    	float val = 0;
    	int count = 0;
    	//int count = 0;
    	
    	if (values.hasNext()) {
    		
    	} else {
    		output.collect( key, new IntWritable( 0 ));
    		return;
    	}
    	
    	
    	while (values.hasNext()) {
    		count++;
            //sum += values.next().get();
    		val = values.next().Value;
    		//dSum += val;
    		//Text t = new Text();
    		//t.set( "Value[ " + key.Index + " ] = " + key.Value );
    		//output.collect(t, new IntWritable(values.next().get()));
    		
    		
    		
          }    	
    	

    	output.collect( key, new IntWritable( count ) );
    	
    	
    } // reduce
    
  } // static class Reduce
  
  static int printUsage() {
    System.out.println("Test_RecordReader_Alt [-m <maps>] [-r <reduces>] <input> <output>");
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
    JobConf conf = new JobConf(getConf(), Test_RecordReader_Alt.class);
    conf.setJobName("Test_RecordReader_Alt");
 
    // the keys are words (strings)
    //conf.setOutputKeyClass(IntWritable.class);
    //conf.setOutputValueClass(DoubleWritable.class);
    
    conf.setMapOutputKeyClass(IntWritable.class);
    conf.setMapOutputValueClass(StandardPointFile.class);

    
    conf.set("gov.tva.mapreduce.AverageFrequency.connectionstring", "jdbc:sqlserver://rgocdsql:1433; databaseName=PhasorMeasurementData;user=NaspiApp;password=pw4site;");
    conf.set("gov.tva.mapreduce.AverageFrequency.HistorianID", "2" );
    
    conf.setMapperClass(MapClass.class);        
    //conf.setCombinerClass(Reduce.class);
    conf.setReducerClass(Reduce.class);
    
    conf.setInputFormat( DatAware_InputFormat.class );
    
    
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
    
    /*
     * at this point, we need to check for a parameter that represents the id
     * of any other info we may need to view
     * --- then set the parameter in the job configuration
     * 		ex: conf.set( "gov.tva.AvgFreq.Company.ID", other_args.get( n ) );
     */
    
    
    FileInputFormat.setInputPaths( conf, other_args.get( 0 ) );
    FileOutputFormat.setOutputPath( conf, new Path( other_args.get( 1 ) ) );
        
    JobClient.runJob(conf);
    return 0;
  }
  
  
  public static void main(String[] args) throws Exception {
    int res = ToolRunner.run(new Configuration(), new Test_RecordReader_Alt(), args);
    System.exit(res);
  }




	
	
}
