//******************************************************************************************************
//  MeasurementEvent.java - Gbtc
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
//  04/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.transport.event;

import java.util.Collection;
import java.util.Collections;
import java.util.EventObject;

import org.gpa.gsf.timeseries.Measurement;

/**
 * Event that occurs when new measurements are received from the publisher.
 */
public class MeasurementEvent extends EventObject
{
	private Collection<Measurement> m_measurements;

	/**
	 * Constructs a new instance.
	 * 
	 * @param source the source of the event
	 * @param measurements the new measurements received from the publisher
	 */
	public MeasurementEvent(Object source, Collection<Measurement> measurements)
	{
		super(source);

		if (measurements == null)
			throw new IllegalArgumentException("measurements cannot be null");
		
		m_measurements = Collections.unmodifiableCollection(measurements);
	}

	/**
	 * Gets the measurements that were received from the publisher.
	 * 
	 * @return the measurements received from the publisher
	 */
	public Collection<Measurement> getMeasurements()
	{
		return m_measurements;
	}

	private static final long serialVersionUID = 5788181880673812873L;
}
