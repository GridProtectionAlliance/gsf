//******************************************************************************************************
//  SampleSubscriberListener.java - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/19/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.sample;

import java.io.PrintStream;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Collection;
import java.util.Iterator;

import org.gpa.gsf.timeseries.Measurement;
import org.gpa.gsf.timeseries.transport.DataSubscriber;
import org.gpa.gsf.timeseries.transport.event.MeasurementEvent;
import org.gpa.gsf.timeseries.transport.event.MessageEvent;
import org.gpa.gsf.timeseries.transport.event.SubscriberAdapter;
import org.gpa.gsf.timeseries.util.TimeExtensions;

/**
 * Sample listener class for the {@link DataSubscriber}.
 * 
 * @see SimpleSubscribe
 */
public class SampleSubscriberListener extends SubscriberAdapter
{
	private PrintStream m_outputStream;
	private PrintStream m_errorStream;
	private int m_processCount;
	
	/**
	 * Constructs a new sample subscriber listener using the standard
	 * {@link System.out} and {@link System.err} print streams.
	 */
	public SampleSubscriberListener()
	{
		this(System.out, System.err);
	}
	
	/**
	 * Constructs a new sample subscriber listener using
	 * the given output stream and no error stream.
	 * 
	 * @param outputStream the output stream to which messages will be written
	 * @throws IllegalArgumentException if {@code outputStream} is {@code null}
	 */
	public SampleSubscriberListener(PrintStream outputStream)
	{
		this(outputStream, null);
	}
	
	/**
	 * Constructs a new sample subscriber litener using
	 * the given output stream and error stream.
	 * 
	 * @param outputStream the output stream to which status messages will be written
	 * @param errorStream the error stream to which error messages will be written
	 * @throws IllegalArgumentException if {@code outputStream} is {@code null}
	 */
	public SampleSubscriberListener(PrintStream outputStream, PrintStream errorStream)
	{
		if (outputStream == null)
			throw new IllegalArgumentException("outputStream cannot be null");
		
		m_outputStream = outputStream;
		m_errorStream = errorStream;
	}
	
	/**
	 * Writes status messages from the {@link DataSubscriber} to the output stream.
	 */
	@Override
	public void statusMessageReceived(MessageEvent evt)
	{
		m_outputStream.println(evt.getMessage());
		m_outputStream.println();
	}
	
	/**
	 * Writes the stack trace of exceptions encountered
	 * by the {@link DataSubscriber} to the error stream.
	 */
	@Override
	public void exceptionEncountered(MessageEvent evt)
	{
		if (m_errorStream != null)
		{
			evt.getException().printStackTrace(m_errorStream);
			m_errorStream.println();
		}
	}
	
	/**
	 * Writes information about measurements received from the
	 * {@link DataSubscriber} to the output stream every so often.
	 */
	@Override
	public void newMeasurementsReceived(MeasurementEvent evt)
	{
		Collection<Measurement> newMeasurements = evt.getMeasurements();
		
		DataSubscriber subscriber = null;
		Measurement firstMeasurement = null;
		Measurement nextMeasurement = null;
		Iterator<Measurement> iter = null;
		
		DateFormat dateFormat;
		String dateString;
		
		// Only display messages every five
		// seconds (assuming 30 calls per second)
		if (m_processCount % 150 == 0)
		{
			if (evt.getSource() instanceof DataSubscriber)
				subscriber = (DataSubscriber)evt.getSource();
			
			if (newMeasurements.size() > 0)
			{
				firstMeasurement = newMeasurements.iterator().next();
				iter = newMeasurements.iterator();
			}
			
			if (subscriber != null)
				m_outputStream.println(subscriber.getTotalMeasurementsReceived() + " measurements received so far...");
			
			if (firstMeasurement != null)
			{
				dateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS");
				dateString = dateFormat.format(TimeExtensions.fromTicks(firstMeasurement.getTimestamp()).getTime());
				m_outputStream.println("Timestamp: " + dateString);
			}

			if (iter != null)
			{
				m_outputStream.println("Point\tValue");
				
				while (iter.hasNext())
				{
					nextMeasurement = iter.next();
					m_outputStream.println(nextMeasurement.getId() + "\t" + nextMeasurement.getValue());
				}
				
				m_outputStream.println();
			}
		}
		
		m_processCount++;
	}
	
	/**
	 * Writes the message received when the {@link DataSubscriber}'s
	 * connection is terminated to the error stream.
	 */
	@Override
	public void connectionTerminated(MessageEvent evt)
	{
		if (m_errorStream != null)
		{
			m_errorStream.println(evt.getMessage());
			m_errorStream.println();
		}
	}
}
