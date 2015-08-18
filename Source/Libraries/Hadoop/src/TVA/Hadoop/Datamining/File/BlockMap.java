package TVA.Hadoop.Datamining.File;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;

public class BlockMap
{
	private byte[] _header = new byte[10];
	ArrayList<DataBlockDescription> _descriptions;
	
	public BlockMap()
	{
		_descriptions = new ArrayList<DataBlockDescription>();
	}
	
	public ArrayList<DataBlockDescription> getBlockDescriptions() throws Exception
	{		
		return _descriptions;
	}
	
	public void addBlockDescription(DataBlockDescription blockDescription) throws Exception
	{		
		_descriptions.add(blockDescription);
	}
	
	public void printPointIds() throws Exception
	{
		for(int i = 0; i < _descriptions.size(); i++)
			System.out.print(_descriptions.get(i).getPointId() + ",");
	}
	
	/**
	 * Header provides legacy support to serialize arrays in vb6
	 */
	private void calculateHeader()
	{
		ByteBuffer buffer = ByteBuffer.allocate(_header.length);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		
		// 1 dimension descriptor
		buffer.putShort((short)1);
		
		// length
		buffer.putInt(_descriptions.size());
		
		// VB6 arrays are 1 based
		buffer.putInt(1);
	}
	
	public byte[] Serialize() throws Exception
	{
		ByteBuffer buffer = ByteBuffer.allocate(_header.length + DataBlockDescription.length*_descriptions.size());
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		
		calculateHeader();
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
		
		return blockMap;
	}
}
