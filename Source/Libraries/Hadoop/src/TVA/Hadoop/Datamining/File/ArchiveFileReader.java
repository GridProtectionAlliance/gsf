package TVA.Hadoop.Datamining.File;

import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;

public class ArchiveFileReader
{
	// file handling
	private File _file;
	private DataInputStream _readStream;
	
	// members
	private FileAllocationTable _fat;
	private BlockMap _blockMap;
	
	private DataBlock _currentBlock;
	
	public ArchiveFileReader(File file) throws Exception
	{
		_file = file;
		_readStream = new DataInputStream(new FileInputStream(_file));
		
		readFAT();
		readBlockMap();
	}
	
	public FileAllocationTable getFAT()
	{
		return _fat;
	}
	
	public BlockMap getBlockMap()
	{
		return _blockMap;
	}
	
	public DataBlock getCurrentBlock()
	{
		return _currentBlock;
	}
	
	private void readFAT() throws Exception
	{
		// reset
		_readStream.close();
		_readStream = new DataInputStream(new FileInputStream(_file));
		
		_readStream.skip(_file.length() - FileAllocationTable.length);
		byte[] buffer = new byte[FileAllocationTable.length];
		_readStream.read(buffer);
		_fat = FileAllocationTable.Deserialize(buffer);
	}
	
	// TODO: What to do for a huge block map? Maybe this won't fit entirely in main memory. Maybe it's not even necessary!
	private void readBlockMap() throws Exception
	{
		// reset
		_readStream.close();
		_readStream = new DataInputStream(new FileInputStream(_file));
		
		_readStream.skip(_file.length() - FileAllocationTable.length - _fat.getBlockMapLength());
		byte[] buffer = new byte[_fat.getBlockMapLength()];
		_readStream.read(buffer);
		_blockMap = BlockMap.Deserialize(buffer);
	}
	
	public DataBlock parseBlock(int index) throws Exception
	{
		// reset
		_readStream.close();
		_readStream = new DataInputStream(new FileInputStream(_file));
		
		_readStream.skip(index * _fat.getDataBlockSizeInBytes());
		byte[] buffer = new byte[_fat.getDataBlockSizeInBytes()];
		_readStream.read(buffer);
		_currentBlock = DataBlock.Deserialize(buffer);
		
		_currentBlock.setBlockDescription(_blockMap.getBlockDescriptions().get(index));
		
		return _currentBlock;
	}
}
