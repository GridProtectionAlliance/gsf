package TVA.Hadoop.Datamining.Tools.DataProcessing;

import java.io.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

import TVA.Hadoop.MapReduce.Historian.File.BlockMap;
import TVA.Hadoop.MapReduce.Historian.File.TimeTag;

public class LocalArchiveFileAllocationTable {

	public TimeTag _StartTime;
	public TimeTag _EndTime;
	public int _PointsRecieved;
	public int _PointsArchived;
	public int _EventBlockSize;
	public int _EventBlockCount;
	
	public BlockMap _BlockMap;
	public byte[] debugBuffer;
	
	public File file;
	
	public LocalArchiveFileAllocationTable(LocalArchiveFile ParentFile, File oFile ) {
		
		this.file = oFile;
		
		//byte[] fixedHeaderBuffer = new byte[ 32 ]; // header
		this.debugBuffer = new byte[ 16 ];
		
		long offset = oFile.length() - 16;
		DataInputStream is;

		double dTmpStartTime = 0;
		double dTmpEndTime = 0;
		
		// read the fixed fields from the filestream
		this._PointsRecieved = 0;
		this._PointsArchived = 0;
		this._EventBlockCount = 0;
		this._EventBlockSize = 0;
		
		this._BlockMap = new BlockMap();
		
		
		
		
		System.out.println( "Reading FAT of archive " + oFile.length() + " bytes in length" );
		System.out.println( "FAT fixed offset at: " + offset );
		
		
		
		try {
	
			byte[] bufPoint = new byte[10];
			ByteBuffer PointBuffer = ByteBuffer.wrap(bufPoint);
			PointBuffer.order(ByteOrder.LITTLE_ENDIAN);
			
			byte[] buf = new byte[16];
			ByteBuffer foo = ByteBuffer.wrap(buf);
			foo.order(ByteOrder.LITTLE_ENDIAN);
			
			
			is = new DataInputStream( new FileInputStream( oFile ) );
			is.skip(offset);
			is.read(buf);
			is.close();
			
			//is = new DataInputStream( new FileInputStream( oFile ) );
			//is.skip(offset);
			//System.out.println( "Avail bytes at read point: " + is.available() );
		//	dTmpStartTime = is.readDouble();
		//	dTmpEndTime = is.readDouble();
			
			/*
			this._PointsRecieved = is.readInt();
			this._PointsArchived = is.readInt();
			this._DataBlockSize = is.readInt();
			this._DataBlockCount = is.readInt();
			*/

			this._PointsRecieved = foo.getInt(0);
			this._PointsArchived = foo.getInt(4);
			this._EventBlockSize = foo.getInt(8);
			this._EventBlockCount = foo.getInt(12);
			
			this.ReadVariableFATArea();

		} catch (IOException e) {
			System.out.println( e.toString() );
		}
		
		
		
		
		
		
		
	}
	
	private void ReadVariableFATArea() throws IOException {
		
		long lVarBlockMapSize = 10 + (this._EventBlockCount * 12); // block map size in bytes
		
		

		byte[] bufBlockMapPtr = new byte[12];
		ByteBuffer MapPtrBuffer = ByteBuffer.wrap(bufBlockMapPtr);
		MapPtrBuffer.order(ByteOrder.LITTLE_ENDIAN);		
		
		//long offset = file.length() - 16;
		DataInputStream is;
		is = new DataInputStream( new FileInputStream( this.file ) );
		
		long lBlockMapAreaOffset = file.length() - 32 - lVarBlockMapSize;

		// skip 10 more bytes for header info		
		is.skip( lBlockMapAreaOffset + 10 );
		
		for ( int x = 0; x < this._EventBlockCount; x++ ) {
			
			is.read(bufBlockMapPtr); // read 12 bytes into the buffer
			//this._PointsRecieved = foo.getInt(0);
			int iID = MapPtrBuffer.getInt( 0 );
			long time = MapPtrBuffer.getLong(4);
			this._BlockMap.AddBlockPointer(iID, time);
			
		} // for

		
		is.close();
		
	}
	
	public int GetBlockSize() {
		
		return 1024 * this._EventBlockSize;
		
	}
	

	byte[] intToBytes(int i){
		ByteBuffer bb = ByteBuffer.allocate(4); 
		bb.putInt(i); 
		return bb.array();
		}	
	
	public void Debug() {
		
		// print out debug stuff here
		System.out.println( "Debug FAT:" );
		
		System.out.println( "Points Archived: " + this._PointsArchived );
		System.out.println( "Points Recieved: " + this._PointsRecieved );
		System.out.println( "Event Block Size: " + this._EventBlockSize );
		System.out.println( "Event Block Count: " + this._EventBlockCount );
		
		
		
		//System.out.println( byteArrayToHexString( this.intToBytes( this._EventBlockCount ) ) );
		
		System.out.println( "BlockMap Pointer Count: " + this._BlockMap._arBlockPointers.size() );
		this._BlockMap.Debug();
		/*
		for ( int x = 0; x < this._BlockMap._arBlockPointers.size(); x++ ) {
			
			System.out.println( "[ " + this._BlockMap._arBlockPointers.get(x).iPointID + " ], time: " + this._BlockMap._arBlockPointers.get(x).Time.m_timeTagOffsetTicks );
			
		}
*/
		
		//System.out.println( byteArrayToHexString( this.debugBuffer ) );
		
		
		
	} // debug
	

	static String byteArrayToHexString(byte in[]) {

	    byte ch = 0x00;

	    int i = 0; 

	    if (in == null || in.length <= 0)

	        return null;

	        

	    String pseudo[] = {"0", "1", "2",
	"3", "4", "5", "6", "7", "8",
	"9", "A", "B", "C", "D", "E",
	"F"};

	    StringBuffer out = new StringBuffer(in.length * 2);

	    

	    while (i < in.length) {

	    	if (i % 4 == 0) {
	    		out.append("\r\n");
	    	}
	    	out.append(i + ": 0x");
	    	
	        ch = (byte) (in[i] & 0xF0); // Strip off


	        ch = (byte) (ch >>> 4);
	     // shift the bits down

	        ch = (byte) (ch & 0x0F);    
	// must do this is high order bit is on!

	        out.append(pseudo[ (int) ch]); // convert the


	        ch = (byte) (in[i] & 0x0F); // Strip off


	        out.append(pseudo[ (int) ch]); // convert the

	        out.append("\r\n");

	        i++;
	    

	    }
	    
	    String rslt = new String(out);

	    return rslt;	    
	}	
	
	
}
