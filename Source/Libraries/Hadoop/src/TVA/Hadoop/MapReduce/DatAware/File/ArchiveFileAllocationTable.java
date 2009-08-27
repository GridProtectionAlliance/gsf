package TVA.Hadoop.MapReduce.DatAware.File;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Calendar;
import java.util.TimeZone;

import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;


public class ArchiveFileAllocationTable {

	public TimeTag _StartTime;
	public TimeTag _EndTime;
	public int _PointsRecieved;
	public int _PointsArchived;
	public int _EventBlockSize;
	public int _EventBlockCount;
	public BlockMap _BlockMap;
	public FSDataInputStream _hdfs_input_stream;
	public FileSystem _fs;
	public int _stream_buffer_size;
	public Path _file_path;
	private long lFileTotalLength;
	private Calendar cBaseDateTime;
	
	public ArchiveFileAllocationTable( FSDataInputStream oFile, long lFileTotalLen ) {
		
		this._BlockMap = new BlockMap();
		this.lFileTotalLength = lFileTotalLen;
		this._hdfs_input_stream = oFile;
		this._PointsRecieved = 0;
		this._PointsArchived = 0;
		this._EventBlockCount = 0;
		this._EventBlockSize = 0;

		this.cBaseDateTime = Calendar.getInstance();
		this.cBaseDateTime.setTimeZone( TimeZone.getTimeZone("UTC") );
		this.cBaseDateTime.set( 1995, 0, 1, 0, 0, 0 );
		this.cBaseDateTime.set( Calendar.MILLISECOND, 0 );
		
	}
	
	
	/*
	 * we have to open and close the file handle in order to read the header at the tail
	 * - later on we may want to come back and find a way to 
	 * 
	 * 
	 */
	public boolean ReadHeaderFromHDFS() {
		
		long header_offset = this.lFileTotalLength - 16; 
		System.out.println( "FAT fixed offset at: " + header_offset );
			
		try {
			
			byte[] buf = new byte[ 16 ];
			ByteBuffer foo = ByteBuffer.wrap(buf);
			foo.order(ByteOrder.LITTLE_ENDIAN);
			
			this._hdfs_input_stream.seek(header_offset);
			this._hdfs_input_stream.read(buf);
			
			this._PointsRecieved = foo.getInt(0);
			this._PointsArchived = foo.getInt(4);
			this._EventBlockSize = foo.getInt(8);
			this._EventBlockCount = foo.getInt(12);
			
			// now read the variable area
			this.ReadVariableFATArea();

		} catch (IOException e) {
			System.out.println( e.toString() );
		}
			
			
		return true;	
		
	}

	private void ReadVariableFATArea() throws IOException {
		
		long lVarBlockMapSize = 10 + (this._EventBlockCount * 12); // block map size in bytes

		byte[] bufBlockMapPtr = new byte[ 12 ];
		ByteBuffer MapPtrBuffer = ByteBuffer.wrap( bufBlockMapPtr );
		MapPtrBuffer.order(ByteOrder.LITTLE_ENDIAN);		
			
		long lBlockMapAreaOffset = this.lFileTotalLength - 32 - lVarBlockMapSize;
		
		// skip 10 more bytes for header info		
		this._hdfs_input_stream.seek(lBlockMapAreaOffset + 10);
		
		for ( int x = 0; x < this._EventBlockCount; x++ ) {
			
			this._hdfs_input_stream.read(bufBlockMapPtr); // read 12 bytes into the buffer
			int iID = MapPtrBuffer.getInt( 0 );
			double firstTimeStamp = MapPtrBuffer.getDouble( 4 );
			this._BlockMap.AddBlockPointer( iID, firstTimeStamp );
			
		} // for
		
	}	
	
	public int GetBlockSize() {
		
		return 1024 * this._EventBlockSize;
		
	}
	
	public void DebugFAT() {

		System.out.println( "Debug FAT:" );
		System.out.println( "Points Archived: " + this._PointsArchived );
		System.out.println( "Points Recieved: " + this._PointsRecieved );
		System.out.println( "Event Block Size: " + this._EventBlockSize );
		System.out.println( "Event Block Count: " + this._EventBlockCount );
		
	}
	
	public void DebugBlockMap( int iLimit ) {
	
		System.out.println( "Debug BlockMap:" );
		
		for ( int x = 0; x < this._BlockMap._arBlockPointers.size(); x++ ) {
			
			Calendar cal = this._BlockMap.GetBlockPointerByIndex( x ).Time.GetCalendar(this.cBaseDateTime);
			System.out.println( "BlockMap - ID: " + this._BlockMap.GetBlockPointerByIndex( x ).iPointID + ", Time: " + cal.getTime() + ", ms: " + cal.get( Calendar.MILLISECOND) );
			
			if (iLimit <= x) {
				break;
			}
			
		}		
		
	}
	
	

}

