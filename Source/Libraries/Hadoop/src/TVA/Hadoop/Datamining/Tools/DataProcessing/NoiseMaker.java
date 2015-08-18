package TVA.Hadoop.Datamining.Tools.DataProcessing;

import TVA.Hadoop.Datamining.Tools.DataProcessing.ValueObjects.TSInstance;

public class NoiseMaker
{
	public TSInstance Noise(TSInstance sample, double minNoiseFactor, double maxNoiseFactor)
	{
		TSInstance returnSample = sample.clone();
		float mean = returnSample.GetMeanValue();
		
		for(int i = 0; i < returnSample.Count(); i++)
		{
			double scaleFactor = (minNoiseFactor + Math.random() * (maxNoiseFactor - minNoiseFactor));
			returnSample.GetPoints().get(i).m_value -= mean;
			returnSample.GetPoints().get(i).m_value *= scaleFactor;
			returnSample.GetPoints().get(i).m_value += mean;
		}
		
		return returnSample;
	}
	
	public TSInstance Scale(TSInstance sample, double scaleFactor)
	{
		TSInstance returnSample = sample.clone();
		float mean = returnSample.GetMeanValue();
		
		for(int i = 0; i < returnSample.Count(); i++)
		{
			returnSample.GetPoints().get(i).m_value -= mean;
			returnSample.GetPoints().get(i).m_value *= scaleFactor;
			returnSample.GetPoints().get(i).m_value += mean;
		}
		
		return returnSample;
	}
	
	public TSInstance RandomScale(TSInstance sample, double minScaleFactor, double maxScaleFactor)
	{
		double scaleFactor = (minScaleFactor + Math.random() * (maxScaleFactor - minScaleFactor));
		return Scale(sample, scaleFactor);
	}
}
