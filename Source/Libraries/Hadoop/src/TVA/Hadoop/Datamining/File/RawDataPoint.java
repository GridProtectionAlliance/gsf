package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class RawDataPoint
{
	
	public static int length = 10;
	
	protected int _timestampOffset;
	protected short _flags;
	protected float _value;
	
	protected boolean _isValid = false;
	
	public int getTimestampOffset() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		return _timestampOffset;
	}
	
	public short getFlags() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		return _flags;
	}
	
	public float getValue() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		return _value;
	}
	
	public byte[] Serialize() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		ByteBuffer buffer = ByteBuffer.allocate(length);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		buffer.putInt(_timestampOffset);
		buffer.putShort(_flags);
		buffer.putFloat(_value);
		
		return buffer.array();
	}
	
	public static RawDataPoint Deserialize(byte[] data) throws Exception
	{
		if(data.length != length)
			throw new Exception("Serialized data must be 10 bytes long.");
		
		RawDataPoint point = new RawDataPoint();
		
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		point._timestampOffset = buffer.getInt();
		point._flags = buffer.getShort();
		point._value = buffer.getFloat();

		point._isValid = true;
		
		return point;
	}
}
