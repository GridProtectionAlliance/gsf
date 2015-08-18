package TVA.Hadoop.Datamining.Tools.DataProcessing.ValueObjects;

import java.util.ArrayList;
import java.util.Collections;

public class TSInstance implements Cloneable
{
	private ArrayList<TSDataPoint> m_dataPoints;
	private FrequencyClassification m_classification;
	
	public TSInstance()
	{
		m_dataPoints = new ArrayList<TSDataPoint>();
		m_classification = FrequencyClassification.UNCLASSIFIED;
	}
	
	public TSInstance(ArrayList<TSDataPoint> points)
	{
		m_dataPoints = points;
		m_classification = FrequencyClassification.UNCLASSIFIED;
	}
	
	public TSInstance(ArrayList<TSDataPoint> points, FrequencyClassification classification)
	{
		m_dataPoints = points;
		m_classification = classification;
	}
	
	/**
	 * Values only, no time component, followed by the classification enum.
	 * 
	 * @return
	 */
	public String SerializeToCsv()
	{
		String returnValue = "";
		
		for (TSDataPoint point : m_dataPoints)
			returnValue += Float.toString(point.m_value) + ",";
		
		returnValue += m_classification.toString();
		
		return returnValue;
	}
	
	public void DeserializeFromCsv(String input)
	{
		String[] cells = input.split(",");
		
		for(int i = 0; i < cells.length-1; i++)
			m_dataPoints.add(new TSDataPoint(Integer.toString(i), cells[i]));
		
		m_classification = FrequencyClassification.valueOf(cells[cells.length-1]);
	}
	
	public ArrayList<TSDataPoint> GetPoints()
	{
		return m_dataPoints;
	}
	public void SetPoints(ArrayList<TSDataPoint> points)
	{
		m_dataPoints = points;
	}
	
	public FrequencyClassification GetClassification()
	{
		return m_classification;
	}	
	public void SetClassification(FrequencyClassification classification)
	{
		m_classification = classification;
	}
	
	public void AddDataPoint(TSDataPoint point)
	{
		m_dataPoints.add(point);
		Collections.sort(m_dataPoints);
	}
	
	public int Count()
	{
		return m_dataPoints.size();
	}
	
	public float GetMeanValue()
	{
		float sum = 0;
		for (TSDataPoint point: m_dataPoints)
			sum += point.GetValue();
		
		return sum / (float) m_dataPoints.size();
	}
	
	public double GetSumOfValues()
	{
		float sum = 0;
		for (TSDataPoint point: m_dataPoints)
			sum += point.GetValue();
		
		return sum;
	}
	
	public TSInstance clone()
	{
		ArrayList<TSDataPoint> clonedPoints = new ArrayList<TSDataPoint>();
		for (TSDataPoint point : m_dataPoints)
			clonedPoints.add(point.clone());
		
		return new TSInstance(clonedPoints, m_classification);
	}
}
