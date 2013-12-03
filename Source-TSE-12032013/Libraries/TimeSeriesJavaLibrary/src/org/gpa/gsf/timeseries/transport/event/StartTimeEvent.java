//******************************************************************************************************
//  SubscriberEvent.java - Gbtc
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

import java.util.EventObject;

/**
 * Event that occurs when the subscriber receives
 * the data start time from the publisher.
 */
public class StartTimeEvent extends EventObject
{
	private long m_dataStartTime;

	/**
	 * Constructs a new instance with the given data start time.
	 * 
	 * @param source the source of the event
	 * @param dataStartTime the data start time received from the publisher
	 */
	public StartTimeEvent(Object source, long dataStartTime)
	{
		super(source);
		m_dataStartTime = dataStartTime;
	}

	/**
	 * Gets the data start time received from the publisher.
	 * 
	 * @return the data start time
	 */
	public long getDataStartTime()
	{
		return m_dataStartTime;
	}
	
	private static final long serialVersionUID = -3551015702900244809L;
}
