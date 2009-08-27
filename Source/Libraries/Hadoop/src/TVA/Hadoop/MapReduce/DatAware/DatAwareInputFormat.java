package TVA.Hadoop.MapReduce.DatAware;

import java.io.IOException;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.mapred.*;

import TVA.Hadoop.MapReduce.DatAware.DatAwareRecordReader;
import TVA.Hadoop.MapReduce.DatAware.File.StandardPointFile;

/**
 * Custom InputFormat class for reading DatAware .d files that store smartgrid PMU data
 * @author jpatter0
 *
 */
public class DatAwareInputFormat extends FileInputFormat<LongWritable, StandardPointFile> implements JobConfigurable {
	
	public void configure(JobConf conf) {
	
	}

	public RecordReader<LongWritable, StandardPointFile> getRecordReader( InputSplit genericSplit, JobConf job, Reporter reporter ) throws IOException {
	    
		reporter.setStatus( genericSplit.toString() );
	
		return new DatAwareRecordReader( job, (FileSplit)genericSplit );
	    
	}

}
