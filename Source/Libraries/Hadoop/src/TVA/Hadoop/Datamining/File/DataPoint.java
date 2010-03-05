package TVA.Hadoop.Datamining.File;

public class DataPoint extends RawDataPoint
{
	protected DataBlockDescription _blockDescrip;
	
	public long getRealTimeInMillis(DataBlockDescription ref)
	{
		// seconds into long datatype
		long time = (long) this._timestampOffset;
		// convert seconds to millis
		time *= 1000;		
				
		// millis stored in flags
		int millis = this._flags;
		millis = millis >> 5;
		
		return ArchiveEpoch.base.getTimeInMillis() + time + (long) millis;
	}
}
