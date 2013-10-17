//******************************************************************************************************
//  Measurement.java - Gbtc
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

/**
 * Fundamental data type used by the Time Series Framework.
 */
public class Measurement
{
	private int m_id;
	private String m_source;
	private UUID m_signalId;
	private String m_tag;
	private double m_value;
	private double m_adder;
	private double m_multiplier;
	private long m_timestamp;
	private int m_flags;
	
	/**
	 * Creates a new instance.
	 */
	public Measurement()
	{
		m_multiplier = 1;
	}
	
	/**
	 * Creates a new instance that is a copy of another instance.
	 * 
	 * @param copy the measurement to be copied
	 */
	public Measurement(Measurement copy)
	{
		m_id = copy.m_id;
		m_source = copy.m_source;
		m_signalId = copy.m_signalId;
		m_tag = copy.m_tag;
		m_value = copy.m_value;
		m_adder = copy.m_adder;
		m_multiplier = copy.m_multiplier;
		m_timestamp = copy.m_timestamp;
		m_flags = copy.m_flags;
	}

	/**
	 * Gets the point ID of the measurement, which is
	 * the suffix of the human-readable measurement key.
	 * 
	 * @return the point ID of the measurement
	 * @see #setId(int)
	 */
	public int getId()
	{
		return m_id;
	}

	/**
	 * Sets the point ID of the measurement.
	 * 
	 * @param id the new value of the point ID
	 * @see #getId()
	 */
	public void setId(int id)
	{
		m_id = id;
	}

	/**
	 * Gets the source of the measurement, which is
	 * the prefix of the human-readable measurement key.
	 * 
	 * @return the source of the measurement
	 * @see #setSource(String)
	 */
	public String getSource()
	{
		return m_source;
	}

	/**
	 * Sets the source of the measurement.
	 * 
	 * @param source the new value of the source
	 * @see #getSource()
	 */
	public void setSource(String source)
	{
		m_source = source;
	}

	/**
	 * Gets the measurement's globally unique identifier.
	 * 
	 * @return the measurement's globally unique identifier
	 * @see #setSignalId(UUID)
	 */
	public UUID getSignalId()
	{
		return m_signalId;
	}

	/**
	 * Sets the measurement's globally unique identifier.
	 * 
	 * @param signalID the new value of the measurement's globally unique identifier
	 * @see #getSignalId()
	 */
	public void setSignalId(UUID signalID)
	{
		m_signalId = signalID;
	}

	/**
	 * Gets the human-readable tag name that describes the measurement.
	 * 
	 * @return the human readable tag name
	 * @see #setTag(String)
	 */
	public String getTag()
	{
		return m_tag;
	}

	/**
	 * Sets the human-readable tag name.
	 * 
	 * @param tag the new value of the human-readable tag name
	 * @see #getTag()
	 */
	public void setTag(String tag)
	{
		m_tag = tag;
	}

	/**
	 * Gets the value of the measurement.
	 * 
	 * @return the value of the measurement
	 * @see #setValue(double)
	 * @see #getAdjustedValue()
	 */
	public double getValue()
	{
		return m_value;
	}

	/**
	 * Sets the value of the measurement.
	 * 
	 * @param value the new value
	 * @see #getValue()
	 * @see #getAdjustedValue()
	 */
	public void setValue(double value)
	{
		m_value = value;
	}

	/**
	 * Gets the additive value modifier.
	 * 
	 * @return the additive value modifier
	 * @see #setAdder(double)
	 * @see #getAdjustedValue()
	 */
	public double getAdder()
	{
		return m_adder;
	}

	/**
	 * Sets the additive value modifier.
	 * 
	 * @param adder the new additive value modifier
	 * @see #getAdder()
	 * @see #getAdjustedValue()
	 */
	public void setAdder(double adder)
	{
		m_adder = adder;
	}

	/**
	 * Gets the multiplicative value modifier.
	 * 
	 * @return the multiplicative value modifier
	 * @see #setMultiplier(double)
	 * @see #getAdjustedValue()
	 */
	public double getMultiplier()
	{
		return m_multiplier;
	}

	/**
	 * Sets the multiplicative value modifier.
	 * 
	 * @param multiplier the new multiplicative value modifier
	 * @see #getMultiplier()
	 * @see #getAdjustedValue()
	 */
	public void setMultiplier(double multiplier)
	{
		m_multiplier = multiplier;
	}

	/**
	 * Gets the timestamp of the measurement in ticks.
	 * 
	 * @return the timestamp of the measurement
	 */
	public long getTimestamp()
	{
		return m_timestamp;
	}

	/**
	 * Sets the timestamp of the measurement.
	 * 
	 * @param timestamp the new timestamp
	 */
	public void setTimestamp(long timestamp)
	{
		m_timestamp = timestamp;
	}

	/**
	 * Gets the flags which define the measurement's state.
	 * 
	 * @return the measurement's state flags
	 */
	public int getFlags()
	{
		return m_flags;
	}

	/**
	 * Sets the flags which define the measurement's state.
	 * 
	 * @param flags the new state flags
	 */
	public void setFlags(int flags)
	{
		m_flags = flags;
	}
	
	/**
	 * Gets the adjusted value of the measurement, which is the value
	 * of the measurement after applying the multiplicative and
	 * additive value modifiers. Mathematically defined as:
	 * <p>
	 * {@code AdjustedValue = (Multiplier * Value) + Adder}
	 * 
	 * @return the value of the measurement after applying modifiers
	 * @see #getValue()
	 * @see #getAdder()
	 * @see #getMultiplier()
	 */
	public double getAdjustedValue()
	{
		return m_value * m_multiplier + m_adder;
	}
}
