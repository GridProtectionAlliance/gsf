package TVA.Hadoop.Datamining.File;

public class DataPoint extends RawDataPoint
{
	protected DataBlockDescription _blockDescrip;
	
	public static DataPoint convert(DataBlockDescription blockDescription, RawDataPoint point)
	{
		DataPoint convertedPoint = new DataPoint();
		convertedPoint._blockDescrip = blockDescription;
		convertedPoint._flags = point._flags;
		convertedPoint._isValid = point._isValid;
		convertedPoint._timestampOffset = point._timestampOffset;
		convertedPoint._value = point._value;
		
		return convertedPoint;
	}
	
	public DataBlockDescription getBlockDescription()
	{
		return _blockDescrip;
	}
}
