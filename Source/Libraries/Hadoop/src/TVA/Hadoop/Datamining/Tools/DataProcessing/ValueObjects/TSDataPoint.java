package TVA.Hadoop.Datamining.Tools.DataProcessing.ValueObjects;

public class TSDataPoint implements Comparable<TSDataPoint>, Cloneable
{
	public long m_timestamp = 0;
	public float m_value = 0;
	
	public TSDataPoint( String time, String value )
	{
		m_timestamp = Long.parseLong(time);
		m_value = Float.parseFloat(value);
	}
	public TSDataPoint( long time, float value )
	{
		m_timestamp = time;
		m_value = value;
	}
	
	public long GetTimestamp()
	{
		return m_timestamp;
	}
	public void GetTimestamp(long value)
	{
		m_timestamp = value;
	}
	
	public float GetValue()
	{
		return m_value;
	}
	public void GetValue(float value)
	{
		m_value = value; 
	}
	
	public int compareTo(TSDataPoint arg0)
	{
		if( this.GetTimestamp() == arg0.GetTimestamp() )
			return 0;
		else if( this.GetTimestamp() < arg0.GetTimestamp() )
			return -1;
		else if( this.GetTimestamp() > arg0.GetTimestamp() )
			return 1;
		
		// shouldn't ever get here, java won't let me throw an exception
		return 0;
	}
	
	@Override
	public String toString()
	{
		return Long.toString(m_timestamp) + "," + Float.toString(m_value) + "\r\n";
	}
	
	public TSDataPoint clone()
	{
		return new TSDataPoint(m_timestamp, m_value);
	}
}
