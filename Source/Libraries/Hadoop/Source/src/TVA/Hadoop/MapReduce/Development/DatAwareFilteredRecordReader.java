package TVA.Hadoop.MapReduce.Development;

import java.io.IOException;

import java.io.InputStream;
import java.io.OutputStream;

import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.mapred.*;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.Text;
import org.apache.hadoop.io.compress.CompressionCodec;
import org.apache.hadoop.io.compress.CompressionCodecFactory;
import org.apache.commons.logging.*;
import org.apache.hadoop.util.LineReader;

import org.apache.hadoop.mapred.*;
import org.apache.hadoop.util.*;

import TVA.Hadoop.MapReduce.DatAware.File.ArchiveFile;
import TVA.Hadoop.MapReduce.DatAware.File.StandardPointFile;
import TVA.Hadoop.MapReduce.DatAware.File.ArchiveFile.FilteredReader;

import gov.tva.pmu.sql.FrequencyData;

public class DatAwareFilteredRecordReader implements RecordReader<LongWritable, StandardPointFile> {
	
	 //private static final Log LOG = LogFactory.getLog(DatAware_RecordReader.class);
	private static final Log LOG = LogFactory.getLog("gov.tva.mapreduce.DatAwareRecordReader");

	  //private CompressionCodecFactory compressionCodecs = null;
	  private long start;
	  private long end;
	  
	  //private boolean more = true;
	  protected Configuration conf;
	  private FileSplit split;
	  
	  private int iDebug_KeyCounter;
	  private int iDebug_ValCounter;
	  
	  //private LineReader in;
	  private FilteredReader in;
	  
	  FrequencyData databaseLookupTable;
	  private String _strHistorianID;
	  private String _strPmuID;
	  
	  int maxLineLength;

	  /*
	   * 	modified to read DatAware fileformat into Text value 
	   */
	  public DatAwareFilteredRecordReader(Configuration conf_job, FileSplit split) throws IOException {

		  	this.conf = conf_job;
		  	this.split = split;

	        Path path = split.getPath();
	        FileSystem fs = path.getFileSystem(conf_job);
	        
	        LOG.info( "DatAwareFilteredRecordReader> Split Path: " + path );
	        
	        this.in = new ArchiveFile.FilteredReader( fs, path, split.getStart(), split.getLength(), conf_job, false );
	        this.end = split.getStart() + split.getLength();
	        
	        // ----------------------
	        // make sure Reader is position to get the next K/V pair;
	        // shift the ArchiveFile.Reader head to be sitting on the first
	        // relevant K/V pair offset in the block
	        // 1. splits work on blocks
	        // 2. read head starts at a block. 
	        // 3. reader processes whole block
	        // ----------------------
	        
	        this.start = this.in.getPosition();
	        
	   //     this.iDebug_KeyCounter = 0;
	   //     this.iDebug_ValCounter = 0;
	        
	        LOG.info("This is the constructor for DatAwareFilteredRecordReader");
	        LOG.info("Split: " + split.getStart() + " to " + this.end);
	        
		  	this.databaseLookupTable = new FrequencyData( this.conf.get("gov.tva.mapreduce.AverageFrequency.connectionstring") );

	        this._strHistorianID = this.conf.get( "gov.tva.mapreduce.AverageFrequency.HistorianID" );
	        
	        this._strPmuID = this.conf.get( "gov.tva.mapreduce.AverageFrequency.PmuID" );
	        
	        if ( null != this._strHistorianID) {
	        	
	        	LOG.info("DatAware_RecordReader: Using Historian Filter for ID: " + this._strHistorianID );
	        	
	        	int iHistID = Integer.parseInt(this._strHistorianID);
	        	
	        	this.databaseLookupTable.LoadFreqIDFromDBForHistorian(iHistID);
	        	
	        	LOG.info("DatAware_RecordReader: Loading Historian Lookup Table for: " + this._strHistorianID);
	        	
	        } else if ( null != this._strPmuID) {
	        	
	        	LOG.info("DatAware_RecordReader: Using Pmu Filter for ID: " + this._strPmuID );
	        	
	        	int iPmuID = Integer.parseInt(this._strPmuID);
	        	
//	        	this.databaseLookupTable.LoadFreqIDFromDBForHistorian(iHistID);
	        	this.databaseLookupTable.LoadFreqIDFromDBForSpecificPMU(iPmuID);
	        	
	        	LOG.info("DatAware_RecordReader: Loading Pmu Lookup Table for: " + this._strPmuID);
	        	
	        } else {
	        	// nothing to load
	        	
	        }
		  	
	        
		  	
		  	LOG.info("Loading Frequency Lookup Table...");
		  	LOG.info("ConnectionString: " + this.conf.get("gov.tva.mapreduce.AverageFrequency.connectionstring") );
		  	LOG.info( "Freq records found: " + this.databaseLookupTable._lookupTable.Size() );
		  		        
	        
	        // set the lookup table
	        this.in.SetBlockLookupFilter( this.databaseLookupTable._lookupTable );
	        // now we either have a lookup table or not. init!
	        this.in.init();
	        
	        
	  }
	  
	  public LongWritable createKey() {
	    return new LongWritable();
	  }
	  
	  public StandardPointFile createValue() {
	    return new StandardPointFile();
	  }
	  
	  
	  
	  
	  public synchronized boolean next(LongWritable key, StandardPointFile value) throws IOException {
		  
		  boolean bKeyRead = this.in.next(key);
		  
		  if (bKeyRead) {
			  this.in.getCurrentValue(value);
			  return true;
		  } else {
			  
			  //LOG.info("DatAware_RecordReader: No More Keys For Split");
			  return false;
		  }
		    
	  }	  
	  
	  
	  /**
	   * Get the progress within the split
	   */
	  public float getProgress() throws IOException {
		if (end == start) {
		    return 0.0f;
		  } else {
		    return Math.min(1.0f, (in.getPosition() - start) / (float)(end - start));
		  }
	  }
	  
	  public  synchronized long getPos() throws IOException {
	    return in.getPosition();
	  }

	  public synchronized void close() throws IOException {
	    if (in != null) {
	      in.close();
	    }
	  }
	
}
