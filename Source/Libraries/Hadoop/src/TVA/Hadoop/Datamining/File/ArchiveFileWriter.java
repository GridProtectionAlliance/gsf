package TVA.Hadoop.Datamining.File;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileOutputStream;

/**
 * 
 * @author gkriley
 * This class currently only supports one input source at a time.
 *
 */
public class ArchiveFileWriter
{
	private File _file;
	private DataOutputStream _writeStream;
	private FileAllocationTable _fat;
	private BlockMap _blockMap;
	private DataBlock _currentBlock;	

	public ArchiveFileWriter(File file, double startTime) throws Exception
	{
		_file = file;
		
		if( !_file.exists() )
			_file.createNewFile();
		else
			throw new Exception("This file already exists, please specify a different name.");
		
		_writeStream = new DataOutputStream(new FileOutputStream(_file));
		
		// typical FAT properties
		_fat = new FileAllocationTable(startTime, 8);
		
		_blockMap = new BlockMap();
		_currentBlock = null;
	}
	
	public ArchiveFileWriter(File file, double startTime, int dataBlockSize) throws Exception
	{
		_file = file;
		
		if( !_file.exists() )
			_file.createNewFile();
		else
			throw new Exception("This file already exists, please specify a different name.");
		
		_writeStream = new DataOutputStream(new FileOutputStream(_file));

		_fat = new FileAllocationTable(startTime, dataBlockSize);
		
		_blockMap = new BlockMap();
		_currentBlock = null;
	}
	
	private void writeCurrentBlock() throws Exception
	{
		if( _currentBlock != null)
		{
			_writeStream.write(_currentBlock.Serialize());
			_fat.incrementDataBlockCount();
			_currentBlock = null;
		}
	}
	
	public void initializeBlockMap(byte[] header)
	{
		_blockMap = new BlockMap(header);
	}
	
	private void initializeNewBlock(DataBlockDescription blockDescription) throws Exception
	{
		_currentBlock = new DataBlock(_fat.getDataBlockSizeInBytes());
		_currentBlock.setBlockDescription(blockDescription);
		_blockMap.addBlockDescription(blockDescription);
	}
	
	public void addPoint(DataPoint point) throws Exception
	{
		// currently ignoring DataPoint.pointId, but it'll be useful later
		if(_currentBlock == null)
			initializeNewBlock(point.getBlockDescription());
		
		if(_currentBlock.isFull())
		{
			writeCurrentBlock();
			initializeNewBlock(point.getBlockDescription());
		}
		
		_currentBlock.addPoint(point);
		_fat.incrementPointsReceived();
		_fat.incrementPointsArchived();
	}
	
	public void close() throws Exception
	{
		writeCurrentBlock();
		// serialize and write block map
		_writeStream.write(_blockMap.Serialize());
		// serialize and write fat
		_writeStream.write(_fat.Serialize());
		// close file
		_writeStream.close();
	}
}
