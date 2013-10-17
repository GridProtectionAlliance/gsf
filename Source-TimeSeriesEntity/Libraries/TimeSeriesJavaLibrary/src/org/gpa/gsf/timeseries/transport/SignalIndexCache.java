//******************************************************************************************************
//  SignalIndexCache.java - Gbtc
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

import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

import org.gpa.gsf.timeseries.MeasurementKey;

/**
 * Represents a cache used to map 16-bit indexes to {@link MeasurementKey}s
 * in order to compact measurement IDs for the compact measurement format.
 */
public class SignalIndexCache
{
	private Map<Short, MeasurementKey> m_reference;
	private Map<UUID, Short> m_signalIdCache;
	
	/**
	 * Constructs a new signal index cache.
	 */
	public SignalIndexCache()
	{
		m_reference = new HashMap<Short, MeasurementKey>();
		m_signalIdCache = new HashMap<UUID, Short>();
	}

	/**
	 * Adds a measurement key to the signal index
	 * cache, identified by the given signal index.
	 * 
	 * @param signalIndex the 16-bit index by which the
	 *        measurement key can be retrieved from the cache
	 * @param signalId the measurement's globally unique identifier
	 * @param source the measurement's source
	 * @param id the measurement's simple numeric identifier
	 * @throws IllegalArgumentException if the given {@code signalID} is
	 *         {@code null} or the given {@code source} is {@code null} or
	 *         empty
	 */
	public void addMeasurementKey(short signalIndex, UUID signalId, String source, int id)
	{
		addMeasurementKey(signalIndex, new MeasurementKey(signalId, source, id));
	}
	
	/**
	 * Adds a measurement key to the signal index
	 * cache, identified by the given signal index.
	 * 
	 * @param signalIndex the 16-bit index by which the
	 *        measurement key can be retrieved from the cache
	 * @param key the measurement key to be placed in the cache
	 */
	public void addMeasurementKey(short signalIndex, MeasurementKey key)
	{
		m_reference.put(signalIndex, key);
		m_signalIdCache.put(key.getSignalId(), signalIndex);
	}
	
	/**
	 * Empties the signal index cache.
	 */
	public void clear()
	{
		m_reference.clear();
		m_signalIdCache.clear();
	}
	
	/**
	 * Determines whether a measurement key exists
	 * in the cache for the given signal index.
	 * 
	 * @param signalIndex the signal index to be searched for
	 * @return true if the signal index refers to
	 *         a measurement key; false otherwise
	 */
	public boolean contains(short signalIndex)
	{
		return m_reference.containsKey(signalIndex);
	}

	/**
	 * Gets the measurement key identified by the given signal index.
	 * 
	 * @param signalIndex the signal index which identifies the measurement key
	 * @return the measurement key identified by the given signal index,
	 *         or {@code null} if no mapping exists for the signal index
	 */
	public MeasurementKey getMeasurementKey(short signalIndex)
	{
		return m_reference.get(signalIndex);
	}
	
	/**
	 * Provides a backwards mapping from the globally unique identifiers
	 * of the measurements whose keys are stored in the cache to the
	 * signal indexes that refer to them.
	 * 
	 * @param signalId the measurement's globally unique identifier
	 * @return the signal index which identifies the measurement key 
	 */
	short getSignalIndex(UUID signalId)
	{
		return m_signalIdCache.get(signalId);
	}
}
