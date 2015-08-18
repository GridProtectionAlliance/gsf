package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class DataBlockDescription
{
	public static int length = 12;
	
	private int _pointId;
	private long _baseTimestamp;
	
	private boolean _isValid = false;
	
	public int getPointId() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		return _pointId;
	}
	
	public long getBaseTimestamp() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		return _baseTimestamp;
	}
	
	public byte[] Serialize() throws Exception
	{
		if(!_isValid)
			throw new Exception("This point has not been initialized yet.");
		
		ByteBuffer buffer = ByteBuffer.allocate(length);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		buffer.putInt(_pointId);
		buffer.putLong(_baseTimestamp);
		
		return buffer.array();
	}
	
	public static DataBlockDescription Deserialize(byte[] data) throws Exception
	{
		if(data.length != length)
			throw new Exception("Serialized data must be 12 bytes long.");
		
		DataBlockDescription blockDescription = new DataBlockDescription();
				
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		blockDescription._pointId = buffer.getInt();
		blockDescription._baseTimestamp = buffer.getLong();

		blockDescription._isValid = true;
		
		return blockDescription;
	}
}
