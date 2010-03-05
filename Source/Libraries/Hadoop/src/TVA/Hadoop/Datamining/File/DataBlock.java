package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

public class DataBlock
{
	private ArrayList<RawDataPoint> _points = new ArrayList<RawDataPoint>();
	
	public ArrayList<RawDataPoint> getPoints()
	{
		return _points;
	}
	
	public byte[] Serialize() throws Exception
	{
		ByteBuffer buffer = ByteBuffer.allocate(RawDataPoint.length * _points.size());
		buffer.order(ByteOrder.LITTLE_ENDIAN);

		for (RawDataPoint point : _points)
			buffer.put(point.Serialize());
		
		return buffer.array();
	}
	
	public static DataBlock Deserialize(byte[] data) throws Exception
	{		
		DataBlock block = new DataBlock();
		
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		
		while(buffer.remaining() > 0)
		{
			byte[] pointBuffer = new byte[RawDataPoint.length];
			buffer.get(pointBuffer);
			block.getPoints().add(RawDataPoint.Deserialize(pointBuffer));
		}
		
		return block;
	}
}
