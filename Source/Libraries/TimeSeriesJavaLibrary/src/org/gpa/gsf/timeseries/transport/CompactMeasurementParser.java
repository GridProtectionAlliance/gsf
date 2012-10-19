//******************************************************************************************************
//  CompactMeasurementParser.java - Gbtc
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
//  04/12/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.transport;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Collection;

import org.gpa.gsf.timeseries.Measurement;
import org.gpa.gsf.timeseries.MeasurementKey;
import org.gpa.gsf.timeseries.util.Masks;

/**
 * Implementation of {@link GatewayMeasurementParser} that parses
 * measurements serialized using the compact measurement format.
 */
public class CompactMeasurementParser implements GatewayMeasurementParser
{
	private SignalIndexCache m_signalIndexCache;
	private long[] m_baseTimeOffsets;
	private boolean m_includeTime;
	private boolean m_useMillisecondResolution;
	
	/**
	 * Constructs a new compact measurement parser with the given signal index cache.
	 *  
	 * @param signalIndexCache the cache used to compact measurement IDs during serialization
	 * @throws IllegalArgumentException if {@code signalIndexCache} is {@code null}
	 */
	public CompactMeasurementParser(SignalIndexCache signalIndexCache)
	{
		this(signalIndexCache, null, true, false);
	}
	
	/**
	 * Constructs a new compact measurement parser with the
	 * given signal index cache and base time offsets.
	 * 
	 * @param signalIndexCache the cache used to compact measurement IDs during serialization
	 * @param baseTimeOffsets the offsets used to compact measurement timestamps during serialization
	 * @param includeTime indicates whether timestamps were included when serializing the measurements
	 * @param useMillisecondResolution determines whether measurement timestamps were further compacted
	 *        during serialization by specifying them to be in millisecond resolution
	 * @throws IllegalArgumentException if {@code signalIndexCache} is {@code null}
	 */
	public CompactMeasurementParser(SignalIndexCache signalIndexCache, long[] baseTimeOffsets, boolean includeTime, boolean useMillisecondResolution)
	{
		if(signalIndexCache == null)
			throw new IllegalArgumentException("signalIndexCache cannot be null");
		
		m_signalIndexCache = signalIndexCache;
		m_baseTimeOffsets = baseTimeOffsets;
		m_includeTime = includeTime;
		m_useMillisecondResolution = useMillisecondResolution;
	}

	/**
	 * Parses a collection of serialized measurements.
	 */
	@Override
	public Collection<Measurement> parseMeasurements(byte[] serializedMeasurements, int offset, int length)
	{
		return parseMeasurements(ByteBuffer.wrap(serializedMeasurements, offset, length));
	}
	
	/**
	 * Parses a collection of serialized measurements.
	 */
	@Override
	public Collection<Measurement> parseMeasurements(ByteBuffer serializedMeasurementBuffer)
	{
		Collection<Measurement> measurements = new ArrayList<Measurement>();
		Measurement parsedMeasurement;
		
		// Parsed from serialized measurement
		byte compactFlags;
		short signalIndex;
		float measurementValue;
		long timestamp = 0L;
		
		// Used for base time offsets
		boolean usingBaseTimeOffset;
		int timeIndex;

		// Obtained from signal index cache
		MeasurementKey key;
		
		while (serializedMeasurementBuffer.hasRemaining())
		{
			// Get and validate compact measurement flags
			compactFlags = serializedMeasurementBuffer.get();
			usingBaseTimeOffset = (compactFlags & CompactBaseTimeOffsetFlag) != 0;
			timeIndex = ((compactFlags & CompactTimeIndexFlag) != 0) ? 1 : 0;
			
			if (usingBaseTimeOffset && (m_baseTimeOffsets == null || m_baseTimeOffsets[timeIndex] == 0))
				break;
			
			if (serializedMeasurementBuffer.remaining() + 1 < getMeasurementByteLength(usingBaseTimeOffset))
				break;
			
			// Get and validate signal index
			signalIndex = serializedMeasurementBuffer.getShort();
			
			if (!m_signalIndexCache.contains(signalIndex))
				break;
			
			// Get measurement value and timestamp
			measurementValue = serializedMeasurementBuffer.getFloat();
			
			if (m_includeTime)
			{
				if (!usingBaseTimeOffset)
				{
					timestamp = serializedMeasurementBuffer.getLong();
				}
				else if (!m_useMillisecondResolution)
				{
					timestamp = serializedMeasurementBuffer.getInt() & Masks.getLongMask(Integer.SIZE);
					timestamp += m_baseTimeOffsets[timeIndex];
				}
				else
				{
					timestamp = serializedMeasurementBuffer.getShort() & Masks.getLongMask(Short.SIZE);
					timestamp *= 10000;
					timestamp += m_baseTimeOffsets[timeIndex];
				}
			}
			
			// Construct the parsed measurement and add it to the collection
			key = m_signalIndexCache.getMeasurementKey(signalIndex);
			parsedMeasurement = new Measurement();
			
			parsedMeasurement.setFlags(mapToFullFlags(compactFlags));
			parsedMeasurement.setSignalId(key.getSignalId());
			parsedMeasurement.setSource(key.getSource());
			parsedMeasurement.setId(key.getId());
			parsedMeasurement.setValue(measurementValue);
			parsedMeasurement.setTimestamp(timestamp);
			
			measurements.add(parsedMeasurement);
		}
		
		return measurements;
	}
	
	/**
	 * Gets the byte length of measurements parsed by this parser.
	 * 
	 * @param usingBaseTimeOffset If false, measurement timestamps are 8 bytes; otherwise, they're smaller.
	 *        This value is determined at serialization time for each individual measurement, so the value
	 *        returned by this method may not be representative of all parsed measurements.
	 * @return the byte length of measurements parsed by this parser
	 */
	public int getMeasurementByteLength(boolean usingBaseTimeOffset)
	{
		int byteLength = 7;
		
		if (m_includeTime)
		{
			if (!usingBaseTimeOffset)
				byteLength += 8;
			else if(!m_useMillisecondResolution)
				byteLength += 4;
			else
				byteLength += 2;
		}
		
		return byteLength;
	}
	
	// Maps the 8-bit compact flag format to the 32-bit full flag format.
	private int mapToFullFlags(byte compactFlags)
	{
		int fullFlags = 0;
		
		if ((compactFlags & CompactDataRangeFlag) != 0)
			fullFlags |= DataRangeMask;

		if ((compactFlags & CompactDataQualityFlag) != 0)
			fullFlags |= DataQualityMask;

		if ((compactFlags & CompactTimeQualityFlag) != 0)
			fullFlags |= TimeQualityMask;

		if ((compactFlags & CompactSystemIssueFlag) != 0)
			fullFlags |= SystemIssueMask;

		if ((compactFlags & CompactCalculatedValueFlag) != 0)
			fullFlags |= CalculatedValueMask;

		if ((compactFlags & CompactDiscardedValueFlag) != 0)
			fullFlags |= DiscardedValueMask;
		
		return fullFlags;
	}

	/**
	 * A data range flag was set.
	 */
	public static final byte CompactDataRangeFlag = 0x01;
	
	/**
	 * A data quality flag was set.
	 */
	public static final byte CompactDataQualityFlag = 0x02;
	
	/**
	 * A time quality flag was set.
	 */
	public static final byte CompactTimeQualityFlag = 0x04;
	
	/**
	 * A system flag was set.
	 */
	public static final byte CompactSystemIssueFlag = 0x08;
	
	/**
	 * Calculated value bit was set.
	 */
	public static final byte CompactCalculatedValueFlag = 0x10;
	
	/**
	 * Discarded value bit was set.
	 */
	public static final byte CompactDiscardedValueFlag = 0x20;
	
	/**
	 * Compact measurement timestamp was serialized using base time offset when set.
	 */
	public static final byte CompactBaseTimeOffsetFlag = 0x40;
	
	/**
	 * Use odd time index (i.e., 1) when bit is set; even time index (i.e., 0) when bit is clear.
	 */
	public static final byte CompactTimeIndexFlag = (byte)0x80;

	/**
	 * Mask over the data range flags.
	 */
	public static final int DataRangeMask       = 0x000000FC;
	
	/**
	 * Mask over the data quality flags.
	 */
	public static final int DataQualityMask     = 0x0000EF03;
	
	/**
	 * Mask over the time quality flags.
	 */
	public static final int TimeQualityMask     = 0x00BF0000;
	
	/**
	 * Mask over the system issue flags.
	 */
	public static final int SystemIssueMask     = 0xE0000000;
	
	/**
	 * Mask over the calculated value bit.
	 */
	public static final int CalculatedValueMask = 0x00001000;
	
	/**
	 * Mask over the dicarded value bit.
	 */
	public static final int DiscardedValueMask  = 0x00400000;
	
}
