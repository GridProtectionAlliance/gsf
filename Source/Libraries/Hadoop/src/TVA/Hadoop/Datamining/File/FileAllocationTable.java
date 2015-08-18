package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class FileAllocationTable
{
	public static int length = 32;
	
	private double _fileStartTime;
	private double _fileEndTime;
	private int _pointsReceived;
	private int _pointsArchived;
	private int _dataBlockSize;
	private int _dataBlockCount;
	
	public FileAllocationTable()
	{
		_fileStartTime = _fileEndTime = 0;
		_pointsReceived = _pointsArchived = 0;
		
		// typical
		_dataBlockSize = 8;
		
		_dataBlockCount = 0;
	}
	
	public FileAllocationTable(double startTime, int dataBlockSize)
	{
		_fileStartTime = startTime; 
		_fileEndTime = 0;
		_pointsReceived = _pointsArchived = 0;
		
		// typical
		_dataBlockSize = dataBlockSize;
		
		_dataBlockCount = 0;
	}
	
	public double getStartTime()
	{
		return _fileStartTime;
	}
	
	public double getEndTime()
	{
		return _fileEndTime;
	}
	
	public int getPointsReceived()
	{
		return _pointsReceived;
	}
	
	public int getPointsArchived()
	{
		return _pointsArchived;
	}
	
	public int getDataBlockSize()
	{
		return _dataBlockSize;
	}
	public int getDataBlockSizeInBytes()
	{
		return _dataBlockSize * 1024;
	}
	
	public int getDataBlockCount()
	{
		return _dataBlockCount;
	}
	
	public int getBlockMapLength()
	{
		return 10 + _dataBlockCount * DataBlockDescription.length;
	}
	
	public void incrementPointsReceived()
	{
		_pointsReceived++;
	}
	
	public void incrementPointsArchived()
	{
		_pointsArchived++;
	}
	
	public void incrementDataBlockCount()
	{
		_dataBlockCount++;
	}
	
	public byte[] Serialize() throws Exception
	{	
		ByteBuffer buffer = ByteBuffer.allocate(length);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		buffer.putDouble(_fileStartTime);
		buffer.putDouble(_fileEndTime);
		buffer.putInt(_pointsReceived);
		buffer.putInt(_pointsArchived);
		buffer.putInt(_dataBlockSize);
		buffer.putInt(_dataBlockCount);
		
		return buffer.array();
	}
	
	public static FileAllocationTable Deserialize(byte[] data) throws Exception
	{
		if(data.length != length)
			throw new Exception("Serialized data must be 32 bytes long.");
		
		FileAllocationTable fat = new FileAllocationTable();
				
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		fat._fileStartTime = buffer.getDouble();
		fat._fileEndTime = buffer.getDouble();
		fat._pointsReceived = buffer.getInt();
		fat._pointsArchived = buffer.getInt();
		fat._dataBlockSize = buffer.getInt();
		fat._dataBlockCount = buffer.getInt();
		
		return fat;
	}
}
