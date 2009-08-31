package TVA.Hadoop.MapReduce.Historian;

import java.io.IOException;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.mapred.*;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;
import org.apache.commons.logging.*;

import TVA.Hadoop.MapReduce.Historian.File.ArchiveFile;
import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;
import TVA.Hadoop.MapReduce.Historian.File.ArchiveFile.Reader;

/**
 * Basic record reader used to read DatAware .d files; DatAware storage files are the primary storage
 * format for PMU time-series point data from the NERC sponsored smart grid. They flow from around the 
 * U.S. power grid in a large hadoop cluster for storage and processing.

		// Generally, the record reader works like this:
		// make sure Reader is position to get the next K/V pair;
		// shift the ArchiveFile.Reader head to be sitting on the first
		// relevant K/V pair offset in the block
		// 1. splits work on HDFS blocks
		// 2. read head starts at a HDFS block. 
		// 3. reader processes whole HDFS block
		// 4. have to have heuristic for bounds of DatAware format
  
 * @author jpatter0
 *
 */
public class HistorianRecordReader implements RecordReader<LongWritable, StandardPointFile> {

	private static final Log LOG = LogFactory.getLog("TVA.Hadoop.MapReduce.Historian.HistorianRecordReader");
	
	private long start;
	private long end;
	protected Configuration conf;
	private FileSplit _FileSplit;
	private Reader _ArchiveFileReaderInput;

	/**
	 * Constructor
	 * Sets up ArchiveFile.Reader to know how to read the file format inside the split
	 * Gets the start and end of the split
	 */
	public HistorianRecordReader(Configuration conf_job, FileSplit split) throws IOException {
		
		this.conf = conf_job;
		this._FileSplit = split;
		
		Path path = split.getPath();
		FileSystem fs = path.getFileSystem( conf_job );
		
		LOG.info("HistorianRecordReader> Split Path: " + path );
		
		this._ArchiveFileReaderInput = new ArchiveFile.Reader( fs, path, split.getStart(), split.getLength(), conf_job, false );
		this.end = split.getStart() + split.getLength();
		this.start = this._ArchiveFileReaderInput.getPosition();
		
		LOG.info( "Split: " + this._FileSplit.getStart() + " to " + this.end );
		
		// We need to call init at first to heuristically align the read head at the start of the next DatAware block inside our split
		this._ArchiveFileReaderInput.init();
		        
	}
  
	public LongWritable createKey() {
		return new LongWritable();
	}
  
	public StandardPointFile createValue() {
		return new StandardPointFile();
	}

	/**
	 * Try and read the next key from the underlying split
	 * @return Boolean indicating whether the operation was successful or not.
	 */
	public synchronized boolean next(LongWritable key, StandardPointFile value) throws IOException {
	  
		boolean bKeyRead = this._ArchiveFileReaderInput.next( key );
  
		if (bKeyRead) {
			this._ArchiveFileReaderInput.getCurrentValue(value);
			return true;
		} else {
			return false;
		}
	    
	}	  

	/**
	 * Get the progress of the current reader.
	 */
	public float getProgress() throws IOException {
		
		if (end == start) {
			return 0.0f;
		} else {
			return Math.min( 1.0f, (_ArchiveFileReaderInput.getPosition() - start) / (float)(end - start) );
		}
		
	}

	/**
	 * Get the position in the file split of the current reader.
	 */
	public  synchronized long getPos() throws IOException {

		return _ArchiveFileReaderInput.getPosition();
		
	}

	/**
	 * Close the current reader.
	 */
	public synchronized void close() throws IOException {
		
		if (_ArchiveFileReaderInput != null) {
			_ArchiveFileReaderInput.close();
		}
		
	}

}
