package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

public class DataBlock
{
	private ArrayList<RawDataPoint> _points;
	private int _blockSize;
	private int _blockCapacity;
	private DataBlockDescription _description;
	
	public DataBlock()
	{
		_points = new ArrayList<RawDataPoint>();
		_blockSize = 0;
		// typical value
		_blockCapacity = 8912;
		
		_description = null;
	}
	
	public DataBlock(int capacity)
	{
		_points = new ArrayList<RawDataPoint>();
		_blockSize = 0;
		_blockCapacity = capacity;
		
		_description = null;
	}
	
	public DataBlockDescription getBlockDescription()
	{
		return _description;
	}
	public void setBlockDescription(DataBlockDescription description)
	{
		_description = description;
	}
	
	public boolean isFull()
	{
		if(_blockCapacity >= (_blockSize + RawDataPoint.length))
			return false;
		else
			return true;
	}
	
	public void addPoint(RawDataPoint point) throws Exception
	{
		if(!isFull())
		{
			_points.add(point);
			_blockSize += RawDataPoint.length;
		}
		else
			throw new Exception("This block is full.");
	}
	
	public ArrayList<RawDataPoint> getPoints()
	{
		return _points;
	}
	
	public int getBlockSize()
	{
		return _blockSize;
	}
	
	public int getBlockCapacity()
	{
		return _blockCapacity;
	}
	
	public int getPointCount()
	{
		return _blockSize / RawDataPoint.length;
	}
	
	public byte[] Serialize() throws Exception
	{
		ByteBuffer buffer = ByteBuffer.allocate(_blockCapacity);
		buffer.order(ByteOrder.LITTLE_ENDIAN);

		for (RawDataPoint point : _points)
			buffer.put(point.Serialize());
		
		// fill the extra space in this buffer
		while(buffer.remaining() > 0)
			buffer.put((byte) 0x00);
		
		return buffer.array();
	}
	
	public static DataBlock Deserialize(byte[] data) throws Exception
	{		
		DataBlock block = new DataBlock();
		
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		
		while(buffer.remaining() > RawDataPoint.length)
		{
			byte[] pointBuffer = new byte[RawDataPoint.length];
			buffer.get(pointBuffer);
			block.addPoint(RawDataPoint.Deserialize(pointBuffer));
		}
		
		block._blockSize = block.getPoints().size() * RawDataPoint.length;
		block._blockCapacity = buffer.remaining() + block._points.size() * RawDataPoint.length;
		
		return block;
	}
}
