package TVA.Hadoop.Datamining.Tools.DataProcessing;

import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

import TVA.Hadoop.MapReduce.Historian.File.ArchiveFileAllocationTable;
import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

public class LocalArchiveFile {

	public LocalArchiveFileAllocationTable FAT;
	private File _refFile;
	public ArrayList<StandardPointFile> arPoints;
	
	
	public LocalArchiveFile( String strFilename ) {

		_refFile = new File( strFilename );
		this.arPoints = new ArrayList<StandardPointFile>();	
	}
	
	public boolean Open() {
		return false;
	}
	
	public LocalArchiveFileAllocationTable ReadFAT() {
	
		this.FAT = new LocalArchiveFileAllocationTable( this, this._refFile );
		//this._FAT.Debug();
			
		return this.FAT;		
	}
	
	public boolean ParseAllPointsInByteRange( long lStartByte, long lEndByte) {
		
		if (lStartByte > lEndByte) {
			return false;
		}
		
		long block_size = (long)this.FAT.GetBlockSize();
		long lstart_block = lStartByte / block_size;
		long lmod = lStartByte % block_size;
		int start_block = (int)lstart_block;
		if (lmod > 0) {
			start_block++;
		}
		
		System.out.println( "\r\nStart Byte: " + lStartByte );
		System.out.println( "End Byte: " + lEndByte );
		System.out.println( "start block: " + start_block );
		
		for ( int x = start_block; x < this.FAT._EventBlockCount; x++ ) {
			
			long block_offset = block_size * x;
			
			if ( block_offset <= lEndByte ) {
				this.ParseBlock( x );
				System.out.println( "!! Processing block " + x );
			} else {
				System.out.println( ">> block " + x + " is out of range, stopping." );
				break;
			}
			
		}
		
		// scan through all blocks in the byte range
		
		return true;
	}
	

	

	public void ParseBlock( int iBlockIndex ) {

		int BlockSize = 1024 * this.FAT._EventBlockSize;
		int iPossiblePointsInBlock = BlockSize / 10;
		long lBlockOffset = (long)(BlockSize * iBlockIndex);
		int x = 0;
		
		DataInputStream is;
		byte[] bufPoint = new byte[10];
		ByteBuffer PointBuffer = ByteBuffer.wrap(bufPoint);
		PointBuffer.order(ByteOrder.LITTLE_ENDIAN);
		
		try {
				
			//System.out.println( "Blocksize: " + BlockSize + ", Possible Points In Block: " + iPossiblePointsInBlock );
			
			is = new DataInputStream( new FileInputStream( this._refFile ) );
			// seek to offset (0)
			
			is.skip(lBlockOffset);
			
			for ( x = 0; x < iPossiblePointsInBlock; x++ ) {
				
				StandardPointFile point = new StandardPointFile();
				
				PointBuffer.clear();
				
				is.read(bufPoint, 0, 10); //(bufPoint);

				point.iTimeTag = PointBuffer.getInt(0);
				point.Flags = PointBuffer.getShort(4);
				point.Value = PointBuffer.getFloat(6);
				/*
				point.arBytesTime = new byte[ 4 ];
				point.arBytesTime[ 0 ] = PointBuffer.get(0);
				point.arBytesTime[ 1 ] = PointBuffer.get(1);
				point.arBytesTime[ 2 ] = PointBuffer.get(2);
				point.arBytesTime[ 3 ] = PointBuffer.get(3);
*/
				
				if ( point.iTimeTag == 0 && point.Value == 0 ) {
					break;
				} else {
					this.arPoints.add( point );
					
				} // if
				
			}
			
			//System.out.println( "Found " + x + " Points" );
			
			is.close();
			
		} catch (IOException e) {
			System.out.println( e.toString() );
		}
		
		
	}	

	public void Debug() {
		
		System.out.println( "Debug Archive File..." );

		for ( int x = 0; x < this.arPoints.size(); x++ ) {
			
			System.out.println( "Point[ " + x + " ]: Value: " + this.arPoints.get(x).Value + ", Time: " + this.arPoints.get(x).iTimeTag );
		
		}		
		
	}
	
}
