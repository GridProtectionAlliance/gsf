//******************************************************************************************************
//  MeasurementKey.java - Gbtc
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

package org.gpa.gsf.timeseries;

import java.util.UUID;

import org.gpa.gsf.timeseries.util.StringExtensions;

/**
 * Represents a grouping of a measurement's globally unique identifier
 * and the alternate human-readable identifier {@code SOURCE:ID}.
 * @see Measurement
 */
public class MeasurementKey
{
	private UUID m_signalId;
	private String m_source;
	private int m_id;
	
	/**
	 * Constructs a measurement key by grouping the given parameters.
	 * 
	 * @param signalId the measurement's globally unique identifier
	 * @param source the measurement's source
	 * @param id the measurement's ID
	 * @throws IllegalArgumentException if the given {@code signalID} is {@code null}
	 *         or the given {@code source} is {@code null} or empty
	 */
	public MeasurementKey(UUID signalId, String source, int id)
	{
		if (signalId == null)
			throw new IllegalArgumentException("signalId must not be null");
		
		if (StringExtensions.isNullOrWhitespace(source))
			throw new IllegalArgumentException("source must not be null or empty");
		
		m_signalId = signalId;
		m_source = source;
		m_id = id;
	}
	
	/**
	 * Gets the globally unique identifier of the measurement.
	 * 
	 * @return the measurement's globally unique identifier
	 * @see Measurement#getSignalId()
	 */
	public UUID getSignalId()
	{
		return m_signalId;
	}
	
	/**
	 * Gets the source of the measurement.
	 * 
	 * @return the measurement's source
	 * @see Measurement#getSource()
	 */
	public String getSource()
	{
		return m_source;
	}
	
	/**
	 * Gets the simple numeric identifier of the measurement.
	 * 
	 * @return the measurement's simple numeric identifier
	 * @see Measurement#getId()
	 */
	public int getId()
	{
		return m_id;
	}
	
	/**
	 * Compares this object to the specified object. The result is
	 * true if and only if the argument is not {@code null} and is
	 * a {@code MeasurementKey} that contains the same signal ID as
	 * this object.
	 */
	@Override
	public boolean equals(Object obj)
	{
		MeasurementKey key;
		
		if(obj == null || !(obj instanceof MeasurementKey))
			return false;
		
		key = (MeasurementKey)obj;
		return m_signalId.equals(key.m_signalId);
	}
	
	/**
	 * Returns a hash code for this {@code MeasurementKey}.
	 */
	@Override
	public int hashCode()
	{
		return m_signalId.hashCode();
	}
	
	/**
	 * Creates a measurement key for the given measurement, if it has one.
	 * This method returns null if either the measurement's signal ID
	 * is null or the source is null or empty.
	 * 
	 * @param measurement the measurement whose key is to be created
	 * @return the key for the given measurement
	 */
	public static MeasurementKey getKey(Measurement measurement)
	{
		UUID signalId = measurement.getSignalId();
		String source = measurement.getSource();
		int id = measurement.getId();
		MeasurementKey key = null;
		
		if (signalId != null && StringExtensions.isNullOrWhitespace(source))
			key = new MeasurementKey(signalId, source, id);
		
		return key;
	}
}
