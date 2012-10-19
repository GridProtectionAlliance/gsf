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
//  04/23/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.transport.event;

import java.util.EventObject;

import org.gpa.gsf.timeseries.transport.DataSubscriber;

/**
 * Generic event associated with a subscriber.
 */
public class SubscriberEvent extends EventObject
{
	private DataSubscriber m_subscriber;

	/**
	 * Constructs a new instance.
	 * 
	 * @param source the source of the event
	 * @param subscriber the subscriber associated with the event
	 */
	public SubscriberEvent(Object source, DataSubscriber subscriber)
	{
		super(source);
		m_subscriber = subscriber;
	}
	
	/**
	 * Gets the subscriber associated with the event.
	 * 
	 * @return the subscriber associated with the event
	 */
	public DataSubscriber getSubscriber()
	{
		return m_subscriber;
	}

	private static final long serialVersionUID = -1854428244027543102L;
}
