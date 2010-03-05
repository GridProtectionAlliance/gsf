package TVA.Hadoop.Datamining.File;

import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;

public class ArchiveFileReader
{
	// file handling
	private File _file;
	private DataInputStream _fileStream;
	
	// members
	private FileAllocationTable _fat;
	private BlockMap _blockMap;
	
	private DataBlock _currentBlock;
	
	public ArchiveFileReader(File file) throws Exception
	{
		_file = file;
		_fileStream = new DataInputStream(new FileInputStream(_file));
		
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
	
	private void readFAT() throws Exception
	{
		_fileStream.reset();
		_fileStream.skip(_file.length() - FileAllocationTable.length);
		byte[] buffer = new byte[FileAllocationTable.length];
		_fileStream.read(buffer);
		_fat = FileAllocationTable.Deserialize(buffer);
	}
	
	// TODO: What to do for a huge block map? Maybe this won't fit entirely in main memory. Maybe it's not even necessary!
	private void readBlockMap() throws Exception
	{
		_fileStream.reset();
		_fileStream.skip(_file.length() - FileAllocationTable.length - _fat.getBlockMapLength());
		byte[] buffer = new byte[_fat.getBlockMapLength()];
		_fileStream.read(buffer);
		_blockMap = BlockMap.Deserialize(buffer);
	}
	
	public DataBlock parseBlock(int index) throws Exception
	{
		_fileStream.reset();
		_fileStream.skip(index * _fat.getDataBlockSize());
		byte[] buffer = new byte[_fat.getDataBlockSize()];
		_fileStream.read(buffer);
		return _currentBlock = DataBlock.Deserialize(buffer);
	}
}
