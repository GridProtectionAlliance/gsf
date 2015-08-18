//*******************************************************************************************************
//  ArchiveFileFilteredReader.java - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/16/2009 - Josh L. Patterson
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

/*
 TVA Open Source Agreement

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/

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
