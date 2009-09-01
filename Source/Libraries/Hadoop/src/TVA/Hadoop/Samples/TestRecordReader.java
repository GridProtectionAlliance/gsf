package TVA.Hadoop.Samples;

	import java.io.IOException;
	import java.util.ArrayList;
	import java.util.Iterator;
	import java.util.List;

	import org.apache.hadoop.conf.Configuration;
	import org.apache.hadoop.conf.Configured;
	import org.apache.hadoop.fs.Path;
	import org.apache.hadoop.io.IntWritable;
	import org.apache.hadoop.io.LongWritable;
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

	import TVA.Hadoop.MapReduce.Historian.HistorianInputFormat;
	import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

	/**
	 * A simple record reader built to show an example data path for the StandardPointFile type as a value with Map Reduce.
	 * An actual count MR program could get away without a reduce phase, this example is for demo purposes only.
	 * 
	 * @author jpatter0
	 *
	 */
	public class TestRecordReader extends Configured implements Tool {

		 public static class MapClass extends MapReduceBase implements Mapper<LongWritable, StandardPointFile, IntWritable, StandardPointFile> {
		    
			static enum ExCounter { DISCARDED, MAPPED };
		    
		    public void map(LongWritable key, StandardPointFile value, OutputCollector<IntWritable, StandardPointFile> output, Reporter reporter) throws IOException {

		    	output.collect( new IntWritable( value.iPointID ), value);
		    	reporter.incrCounter( ExCounter.MAPPED, 1 );
		    	
		    } // map method
		  
		 } // inner class	
	  
	  /**
	   * A reducer class that just emits the sum of the input values.
	   */
	  public static class Reduce extends MapReduceBase implements Reducer<IntWritable, StandardPointFile, IntWritable, IntWritable> {
	    
	    public void reduce(IntWritable key, Iterator<StandardPointFile> values, OutputCollector<IntWritable, IntWritable> output, Reporter reporter) throws IOException {

	    	int count = 0;
	    	
	    	while (values.hasNext()) {

	    		count++;
	    		values.next(); // if we dont call next(), we get stuck in an infinite loop
	    		
	        }    	

	    	output.collect( key, new IntWritable( count ) );
	    	
	    } // reduce
	    
	  } // static class Reduce
	  
	  static int printUsage() {
	    System.out.println("TestRecordReader [-m <maps>] [-r <reduces>] <input> <output>");
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
		  
	    JobConf conf = new JobConf( getConf(), TestRecordReader.class );
	    conf.setJobName("TestRecordReader");
	    
	    conf.setMapOutputKeyClass(IntWritable.class);
	    conf.setMapOutputValueClass(StandardPointFile.class);

	    conf.setMapperClass(MapClass.class);        
	    conf.setReducerClass(Reduce.class);
	    
	    conf.setInputFormat( HistorianInputFormat.class );
	    
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
	    
		int res = ToolRunner.run( new Configuration(), new TestRecordReader(), args );
	    System.exit(res);
	    
	  }




		
		
	}
