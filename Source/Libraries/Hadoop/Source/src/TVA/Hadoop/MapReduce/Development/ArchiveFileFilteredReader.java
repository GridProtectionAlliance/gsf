package TVA.Hadoop.MapReduce.Development;

import java.io.EOFException;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FileStatus;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.io.LongWritable;
import org.apache.hadoop.io.SequenceFile;
import org.apache.hadoop.io.Writable;

import TVA.Hadoop.MapReduce.DatAware.File.ArchiveDataBlockPointer;
import TVA.Hadoop.MapReduce.DatAware.File.ArchiveFileAllocationTable;
import TVA.Hadoop.MapReduce.DatAware.File.DataTypeLookupTable;
import TVA.Hadoop.MapReduce.DatAware.File.StandardPointFile;
import TVA.Hadoop.MapReduce.DatAware.File.ArchiveFile.FilteredReader;
import TVA.Hadoop.MapReduce.DatAware.File.ArchiveFile.Reader;

import gov.tva.pmu.*;


public class ArchiveFileFilteredReader {

	
	// --------------------------------------------------------------------------------------------------
	
	
	/**
	 * The {@link FilteredReader} is similar to the stock Reader but differs in that it allows the underlying system
	 * to take a list of relevant block ids and filter what blocks we actually have HDFS process. The theory being, 
	 * this cuts down on the amount of DFS seeking overhead and thus speeds up performance.
	 * 
	 */
	public static class FilteredReader implements java.io.Closeable {
		
		private static final Log LOG = LogFactory.getLog("gov.tva.mapreduce.ArchiveFile.FilteredReader");
		
		//private boolean syncSeen;
		
		private boolean bDone = false;
		
	    private Path file;
	    private FSDataInputStream in;
	    private FileSystem fs;
	    private int bufferSize;
	    private long _split_start;
	    private long _split_length;
	    private int iPassCounter; // debug

	    //private boolean syncSeen;

	    private long end;
	    public long lTotalDatAwareFileLength;
	    public StandardPointFile currentPoint;
	    
	    private Configuration conf;		
		
	    private int iPossiblePointsInBlock;
	    private int iCurrentBlockPointIndex;
	    
	    private int iCurrentBlockIndex;
		

		private ArchiveFileAllocationTable _FAT;
		//private FSDataInputStream _ref_HDFS_File;
		//public ArrayList<StandardPointFile> arPoints;	
		
		// this is constructed elsewhere. it contains block ids that are valid to this session
		// is null unless explicitly set. if null, all points in file are used.
		// used as essentially as a filter to cut down on hdfs access time
		public DataTypeLookupTable _lookupTable;


	    public FilteredReader( FileSystem fs, Path file_path, long split_byte_start, long split_byte_length, Configuration conf, boolean tempReader ) throws IOException {
	    	
	    	this.currentPoint = new StandardPointFile();
	    	this.iPossiblePointsInBlock = 0;
	    	this.iCurrentBlockPointIndex = 0;
	    	this.iCurrentBlockIndex = 0;
	    	this.iPassCounter = 0;
	    	
	    	//FileSystem fs = path.getFileSystem(conf);
	    	this.lTotalDatAwareFileLength = 0;
	    	this.fs = fs;
	    	this.bufferSize = conf.getInt( "io.file.buffer.size", 4096 );
	    	this._split_start = split_byte_start;
	    	this._split_length = split_byte_length;
	    	
	      this.file = file_path;
	      this.in = openFile(fs, this.file, bufferSize, _split_length);
	      this.conf = conf;
	      
	      FileStatus fileStatus = fs.getFileStatus(file);
	      this.lTotalDatAwareFileLength = fileStatus.getLen();
	      
	    	this.ReadFAT();
	    	
	    	// once the FAT has been read, we need to re-open the stream.
	    	// which is very odd; can we not seek back to our split start?
	    	
	    	// ----- testing to seek if we can see backwards
	    	//this.in = openFile(this.fs, this.file, this.bufferSize, this._split_length);
		      
	      this.seek(this._split_start);
	      this.end = in.getPos() + this._split_length;
	      // init will adjust the read head to the next block offset
	      
	     // LOG.info("> Split start: " + this._split_start + ", split end: " + this.end);
	      
	      
	      //this.init(tempReader);
	    }

	    public void SetBlockLookupFilter( DataTypeLookupTable lookupTable ) {
	    	
	    	this._lookupTable = lookupTable;
	    	
	    }

	    
	    /**
	     * Override this method to specialize the type of
	     * {@link FSDataInputStream} returned.
	     */
	    protected FSDataInputStream openFile(FileSystem fs, Path file, int bufferSize, long length) throws IOException {
	      return fs.open(file, bufferSize);
	    }
	    
	    /**
	     * Initialize the {@link Reader}
	     * @param tmpReader <code>true</code> if we are constructing a temporary
	     *                  reader {@link SequenceFile.Sorter.cloneFileAttributes}, 
	     *                  and hence do not initialize every component; 
	     *                  <code>false</code> otherwise.
	     * @throws IOException
	     */
	    public void init() throws IOException {
	    	
	    	// move the read head to the first block in the split
	    	this.PositionReadHeadAtStartOfBlock( this._split_start );
	    	
	    }		
		

		public ArchiveFileAllocationTable ReadFAT() {
		
			this._FAT = new ArchiveFileAllocationTable( this.in, this.lTotalDatAwareFileLength );
			this._FAT.ReadHeaderFromHDFS();
			this._FAT.DebugBlockMap( 100 );
			
		//	LOG.info("> [FAT] Found " + this._FAT._EventBlockCount + " Blocks.");
		//	LOG.info("> [FAT] Block Size " + this._FAT._EventBlockSize + " KB.");
		//	LOG.info("> [FAT] Read " + this._FAT._BlockMap._arBlockPointers.size() + " block map pointers. ");

			int BlockSize = 1024 * this._FAT._EventBlockSize;
			this.iPossiblePointsInBlock = BlockSize / 10;
			
			
			return this._FAT;
			
		}
		
		private long CalcValidBlockByteRanges() {
			
			long lVarBlockMapSize = 10 + (this._FAT._EventBlockCount * 12); // block map size in bytes
			
			long lBlockArea = this.lTotalDatAwareFileLength - 32 - lVarBlockMapSize;
			
			if (lBlockArea > 0) {
				
				return lBlockArea;
			}
			
			return 0;
			
		}
		
		private boolean BlockIsInLookupTable(int iBlockIndex) {
			
			if ( null != this._lookupTable) {
				
				// the lookup table is not null, so we want to filter the blocks
				ArchiveDataBlockPointer p = this._FAT._BlockMap.GetBlockPointerByIndex(iBlockIndex);
				
				if ( null != p ) {
					
					return this._lookupTable.KeyExists( p.iPointID );
					
				} else {
					
					// if we could not get a block pointer from the block map (bad!)
					// then there was some sort of error
					// default to returning false as the block does not exist (or worse?)
					return false;
					
				}
				
			} // if			

			// if we have no lookuptable, then the default is true
			return true;
			
		}
		

		// need to make sure that we dont read over into the tail-header area
		// TODO: need to make sure the block is applicable, by checking the point id of the block
		// 
		public boolean PositionReadHeadAtStartOfBlock( long lStartByte ) throws IOException {
			
			//LOG.info(">>> PositionReadHeadAtStartOfBlock(" + lStartByte + ")!" );
			
			long block_size = (long)this._FAT.GetBlockSize();
			long lstart_block = lStartByte / block_size;
			long lmod = lStartByte % block_size;
			int start_block = (int)lstart_block;
			if (lmod > 0) {
				start_block++;
			}
			
			// now check to see if the block contains points we are actually interested in

			boolean bFound = false;
			
			//for ( int x = start_block; x < this._FAT._EventBlockCount; x++ ) {
			for ( ; start_block < this._FAT._EventBlockCount; start_block++ ) {
				
				// scan til we hit a relevant block
				if (this.BlockIsInLookupTable( start_block )) {
					//LOG.info("Block[" + start_block + "] is in lookup table -> " + this._FAT._BlockMap.GetBlockPointerByIndex(start_block).iPointID );
					bFound = true;
					break;
				}
				
			}
			
			

			if (!bFound) {
				
				this.iCurrentBlockPointIndex = this.iPossiblePointsInBlock;

				//LOG.info(">>> Did not find a relevant block in valid block range, kicking out!" );
				this.in.seek( this.end );
				return false;
				
			}
			
			this.iCurrentBlockIndex = start_block;
			this.iCurrentBlockPointIndex = 0;
			long lStartBlockOffset = start_block * block_size;
			
		
			
			
			// would our seek point be beyond the current valid range of all blocks in the file?
			if (lStartBlockOffset >= this.CalcValidBlockByteRanges()) {
				// seek to end of our split
				LOG.info(">>> lStartBlockOffset would be out of valid block range, kicking out!" );
				this.in.seek( this.end );
				this.bDone = true;
				return false;
			}

			

			// first make sure that we havent ran out of blocks to play with.
			if (start_block >= this._FAT._EventBlockCount) {
				LOG.info(">>> Out of valid blocks, kicking out!" );
				this.bDone = true;
				this.in.seek( this.end );
				return false;
			}
			
			this.in.seek( lStartBlockOffset );
			

			if (this.getPosition() > this.end) {
				// we're outside our split
				// we're done completely with our split
				this.bDone = true;
				LOG.info(">>> Outside our split, kicking out!" );
				return false;
				
			}			
						
			
			// are the number of bytes left greater than the tail area, the variable FAT, and padding?
			if (this.getPosition() >= this.CalcValidBlockByteRanges()) {
				
				this.bDone = true;
				LOG.info(">>> getPosition() reports out of valid block range, kicking out!" );
				// not enough bytes left to read, we've hit the end of the file
				return false;
				
			}

	
			
			return true;
		}
				
		
		
	    

	    /** Read the next key in the file into <code>key</code>, skipping its
	     * value.  True if another entry exists, and false at end of file. */
	    public synchronized boolean next(Writable key) throws IOException {
	    	
	    	boolean bDataLeftInBlock = false;
	    	long lCurrentPosition = this.getPosition();
	    	this.iPassCounter++;
	    	
	    	
	    	
	    	//LOG.info(">>> next(k)" );
	    	
	    	if (this.bDone) {
	    		
	    		return false;
	    		
	    	}
	    	
	          try {
	        	  
	        	  bDataLeftInBlock = this.ReadNextPointInBlock();
	        	  
	        	  if (bDataLeftInBlock) {
	  	  				
	        		  // we return true, there was data read
	        		  ((LongWritable) key).set( lCurrentPosition );
  	  				
  	  			} else {
  	  				
  	  				// no data left in block
  	  				
  	  				// are we outside the split? if so, terminate

	  				if (this.getPosition() > this.end) {
						// we're outside our split, AND we're done with the currrent block
	  					// we're done completely with our split
						return false;
						
					}
  	  				
	  				// we are still inside the split, find next block
	  				
  					if (this.PositionReadHeadAtStartOfBlock( this.getPosition() ) == false) {
  						
  						// we've moved to the next block, and we've still got no valid data
  						// now we know there is no more data to be had. we're done.
  						return false;
  						
  					} else {
  						
  		  				if (this.getPosition() > this.end) {
  							// we're outside our split
  		  					// we're done completely with our split
  							return false;
  							
  						}  						
  						
  						// now we should either be at the start of a block,
  		  				//LOG.info(">>> next(k) ------> here" );

  		  				lCurrentPosition = this.getPosition();
  						bDataLeftInBlock = this.ReadNextPointInBlock();
  						((LongWritable) key).set( lCurrentPosition );
  						
  					} // if
  	  				
  	  				// if the result was false, then it will kick out and return false up the stack
  	  				
  	  			} // if
  	  			
  	  			

	        	  
	          } catch (EOFException eof) {
	        	  
	        	  return false;
	        	  
		      }
	          
	
	      return bDataLeftInBlock;
	    }	
	    
	    
	    /*
	     * ReadNextPointInBlock
	     * 
	     * -	We terminate a read when 
	     * 
	     * 		-	we are over the split boundary AND we are past the end of the block
	     * 
	     * 		-	we hit points which have no values
	     * 
	     * -	we always read to the end of a block
	     * 
	     * -	we need to deal with a block rollover in a split
	     * 
	     * 
	     */
	    
	    private boolean ReadNextPointInBlock() throws IOException {
	    	
			if ( this.iCurrentBlockPointIndex >= this.iPossiblePointsInBlock) {

				return false;
							
			}
			
			this.ReadPointAtCurrentPosition();
			
			if (this.currentPoint.iTimeTag == 0 && this.currentPoint.Value == 0) {
				return false; // no data left in block
			}
			
	    	return true; // data left in block
	    	
	    }
	    
	    private void ReadPointAtCurrentPosition() throws IOException {

			byte[] bufPoint = new byte[10];
			ByteBuffer PointBuffer = ByteBuffer.wrap(bufPoint);
			PointBuffer.order(ByteOrder.LITTLE_ENDIAN);
	    	
			
			this.currentPoint = new StandardPointFile();
			
			// filling out DEBUG member variable --- this is not real data!
			//this.currentPoint.Index = (this.iCurrentBlockIndex * 1000) + this.iCurrentBlockPointIndex;
			
			this.iCurrentBlockPointIndex++;
			
			PointBuffer.clear();
			
			this.in.read(bufPoint, 0, 10); //(bufPoint);

			this.currentPoint.iTimeTag = PointBuffer.getInt(0);
			this.currentPoint.Flags = PointBuffer.getShort(4);
			this.currentPoint.Value = PointBuffer.getFloat(6);		
			
			// now tack on the point ID
			ArchiveDataBlockPointer BlockPointer = this._FAT._BlockMap.GetBlockPointerByIndex( this.iCurrentBlockIndex );
			if ( BlockPointer != null ) {
				this.currentPoint.iPointID = BlockPointer.iPointID;
			} else {
				this.currentPoint.iPointID = 0;
			}
	    	
			//LOG.info(">>> Read Point Time: " + this.currentPoint.iTimeTag );
			
	    }
	    
	    

	    /**
	     * Get the 'value' corresponding to the last read 'key'.
	     * @param val : The 'value' to be read.
	     * @throws IOException
	     */
	    public synchronized StandardPointFile getCurrentValue(StandardPointFile val) throws IOException {
	    	
	    	val.Flags = this.currentPoint.Flags;
	    	val.iTimeTag = this.currentPoint.iTimeTag;
	    	val.Value = this.currentPoint.Value;
	    	//val.Index = this.currentPoint.Index;
	    	val.iPointID = this.currentPoint.iPointID;
	    	
	      return val;

	    }	    
	    
		
		
		
	    /** Close the file. */
	    public synchronized void close() throws IOException {

	      // Close the input-stream
	      in.close();

	    }
		
	    /** Set the current byte position in the input file.
	     *
	     * <p>The position passed must be a position returned by {@link
	     * SequenceFile.Writer#getLength()} when writing this file.  To seek to an arbitrary
	     * position, use {@link SequenceFile.Reader#sync(long)}.
	     */
	    public synchronized void seek(long position) throws IOException {
	    
	    	in.seek(position);
	      
	    }


	    

	    /** Returns true iff the previous call to next passed a sync mark.*/
	    //public boolean syncSeen() { return syncSeen; }

	    /** Return the current byte position in the input file. */
	    public synchronized long getPosition() throws IOException {
	      return in.getPos();
	    }

	    /** Returns the name of the file. */
	    public String toString() {
	      return file.toString();
	    }		
		
	} // inner reader class definition
		
	
	
	
}
