package TVA.Hadoop.MapReduce.Historian.File;

import org.apache.hadoop.fs.*;
import org.apache.hadoop.fs.FileSystem;

import java.io.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.apache.commons.logging.*;
import org.apache.hadoop.io.*;
import org.apache.hadoop.conf.*;


public class ArchiveFile {
	
	public ArchiveFile() {
				
	}
	
	/**
	 * ArchiveFile.Reader
	 * 
	 * 
	 */
	public static class Reader implements java.io.Closeable {
		
		private static final Log LOG = LogFactory.getLog("TVA.Hadoop.MapReduce.Historian.File.ArchiveFile.Reader");
		
		private boolean bDone = false;
	    private Path file;
	    private FSDataInputStream in;
	    private FileSystem fs;
	    private int bufferSize;
	    private long _split_start;
	    private long _split_length;
	    private int iPassCounter; // debug
	    private long end;
	    public long lTotalDatAwareFileLength;
	    public StandardPointFile currentPoint;
	    private Configuration conf;		
	    private int iPossiblePointsInBlock;
	    private int iCurrentBlockPointIndex;
	    private int iCurrentBlockIndex;
		private ArchiveFileAllocationTable _FAT;
		


	    public Reader(FileSystem fs, Path file_path, long split_byte_start, long split_byte_length, Configuration conf, boolean tempReader) throws IOException {
	    	
	    	this.currentPoint = new StandardPointFile();
	    	this.iPossiblePointsInBlock = 0;
	    	this.iCurrentBlockPointIndex = 0;
	    	this.iCurrentBlockIndex = 0;
	    	this.iPassCounter = 0;
	    	this.lTotalDatAwareFileLength = 0;
	    	this.fs = fs;
	    	this.bufferSize = conf.getInt( "io.file.buffer.size", 4096 );
	    	this._split_start = split_byte_start;
	    	this._split_length = split_byte_length;
	    	
	      this.file = file_path;
	      this.in = openFile( fs, this.file, bufferSize, _split_length );
	      this.conf = conf;
	      
	      FileStatus fileStatus = fs.getFileStatus(file);
	      this.lTotalDatAwareFileLength = fileStatus.getLen();
	      
	    	this.ReadFAT();
		      
	      this.seek(this._split_start);
	      this.end = in.getPos() + this._split_length;

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

		/**
		 * This method is the most critical part of the ArchiveFile.Reader; it performs alignment of the read head
		 * on a valid block inside our "split". It could probably use some refinement.
		 * @param lStartByte		a Long value indicating our current position to work with.
		 * @return 					a boolean indicating whether we still have more data to read
		 * @throws IOException
		 */
		public boolean PositionReadHeadAtStartOfBlock( long lStartByte ) throws IOException {
			
			//boolean bFound = false;
			long block_size = (long)this._FAT.GetBlockSize();
			long lstart_block = lStartByte / block_size;
			long lmod = lStartByte % block_size;
			int start_block = (int)lstart_block;

			if (lmod > 0) {
				start_block++;
			}

			this.iCurrentBlockIndex = start_block;
			this.iCurrentBlockPointIndex = 0;
			long lStartBlockOffset = start_block * block_size;
			
			// would our seek point be beyond the current valid range of all blocks in the file?
			if (lStartBlockOffset >= this.CalcValidBlockByteRanges()) {
				// seek to end of our split, lStartBlockOffset would be out of valid block range
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
				// we're outside our split, which means we are also done with this split.
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
	     * value.  True if another entry exists, and false at end of file.
	     * This method could probably be refined. 
	     */
	    public synchronized boolean next(Writable key) throws IOException {
	    	
	    	boolean bDataLeftInBlock = false;
	    	long lCurrentPosition = this.getPosition();
	    	this.iPassCounter++;
	    	
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

			byte[] bufPoint = new byte[ 10 ];
			ByteBuffer PointBuffer = ByteBuffer.wrap( bufPoint );
			PointBuffer.order( ByteOrder.LITTLE_ENDIAN );
			
			this.currentPoint = new StandardPointFile();
			this.iCurrentBlockPointIndex++;
			PointBuffer.clear();
			this.in.read( bufPoint, 0, 10 ); //(bufPoint);

			this.currentPoint.iTimeTag = PointBuffer.getInt( 0 );
			this.currentPoint.Flags = PointBuffer.getShort( 4 );
			this.currentPoint.Value = PointBuffer.getFloat( 6 );		
			
			// now tack on the point ID
			ArchiveDataBlockPointer BlockPointer = this._FAT._BlockMap.GetBlockPointerByIndex( this.iCurrentBlockIndex );
			if ( BlockPointer != null ) {
				this.currentPoint.iPointID = BlockPointer.iPointID;
			} else {
				this.currentPoint.iPointID = 0;
			}
			
	    }
	    
	    

	    /**
	     * Get the 'value' corresponding to the last read 'key'.
	     * @param val : The 'value' to be read.
	     * @throws IOException
	     */
	    public synchronized StandardPointFile getCurrentValue(StandardPointFile val) throws IOException {
	    	
	    	val.Flags 		= this.currentPoint.Flags;
	    	val.iTimeTag 	= this.currentPoint.iTimeTag;
	    	val.Value 		= this.currentPoint.Value;
	    	val.iPointID 	= this.currentPoint.iPointID;
	    	
	      return val;

	    }	    
		
	    /** Close the file. */
	    public synchronized void close() throws IOException {

	      in.close();

	    }
		
	    /** Set the current byte position in the input file.
	     *
	     */
	    public synchronized void seek(long position) throws IOException {
	    
	    	in.seek(position);
	      
	    }

	    /** Return the current byte position in the input file. */
	    public synchronized long getPosition() throws IOException {
	      return in.getPos();
	    }

	    /** Returns the name of the file. */
	    public String toString() {
	      return file.toString();
	    }		
		
	} 
	
	// ---------------------------------------------------------------------------------------------------------------
	
} // class definition
