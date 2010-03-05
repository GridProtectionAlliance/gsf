package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

public class BlockMap
{
	private byte[] _header = new byte[10];
	ArrayList<DataBlockDescription> _descriptions;
	
	private boolean _isValid = false;
	
	public byte[] getHeader() throws Exception
	{
		if(!_isValid)
			throw new Exception("This object has not been initialized yet.");
		
		return _header;
	}
	
	public ArrayList<DataBlockDescription> getBlockDescriptions() throws Exception
	{
		if(!_isValid)
			throw new Exception("This object has not been initialized yet.");
		
		return _descriptions;
	}
	
	public void addBlockDescription(DataBlockDescription blockDescription) throws Exception
	{
		if(!_isValid)
			throw new Exception("This object has not been initialized yet.");
		
		_descriptions.add(blockDescription);
	}
	
	public byte[] Serialize() throws Exception
	{
		if(!_isValid)
			throw new Exception("This object has not been initialized yet.");
		
		ByteBuffer buffer = ByteBuffer.allocate(_header.length + DataBlockDescription.length*_descriptions.size());
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		buffer.put(_header);
		
		for (DataBlockDescription blockDescription : _descriptions)
			buffer.put(blockDescription.Serialize());
		
		return buffer.array();
	}
	
	public static BlockMap Deserialize(byte[] data) throws Exception
	{			
		BlockMap blockMap = new BlockMap();
		blockMap._descriptions = new ArrayList<DataBlockDescription>();
		
		ByteBuffer buffer = ByteBuffer.wrap(data);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		buffer.get(blockMap._header);
		
		while(buffer.remaining() > 0)
		{
			byte[] descripBuffer = new byte[DataBlockDescription.length];
			buffer.get(descripBuffer);
			blockMap._descriptions.add(DataBlockDescription.Deserialize(descripBuffer));
		}

		blockMap._isValid = true;
		
		return blockMap;
	}
}
