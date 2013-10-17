//******************************************************************************************************
//  SampleSubscriberConnectorListener.java - Gbtc
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

package org.gpa.gsf.timeseries.sample;

import java.io.PrintStream;

import org.gpa.gsf.timeseries.transport.SubscriptionInfo;
import org.gpa.gsf.timeseries.transport.event.MessageEvent;
import org.gpa.gsf.timeseries.transport.event.SubscriberConnectorAdapter;
import org.gpa.gsf.timeseries.transport.event.SubscriberEvent;

/**
 * Sample listener for a subscriber connector.
 * 
 * @see SubscriberConnector
 */
public class SampleSubscriberConnectorListener extends SubscriberConnectorAdapter
{
	private SubscriptionInfo m_info;
	private PrintStream m_errorStream;
	
	/**
	 * Constructs a new instance using the standard {@code System.err}
	 * print stream for displaying errors.
	 * 
	 * @param info subscription info used to automatically
	 *        resubscribe to the publisher
	 */
	public SampleSubscriberConnectorListener(SubscriptionInfo info)
	{
		this(info, System.err);
	}
	
	/**
	 * Constructs a new instance.
	 * 
	 * @param info subscription info used to automatically
	 *        resubscribe to the publisher
	 * @param errorStream print stream used for displaying errors
	 */
	public SampleSubscriberConnectorListener(SubscriptionInfo info, PrintStream errorStream)
	{
		if (info == null)
			throw new IllegalArgumentException("info must not be null");
		
		m_info = info;
		m_errorStream = errorStream;
	}
	
	@Override
	public void exceptionEncountered(MessageEvent evt)
	{
		if (m_errorStream != null)
			evt.getException().printStackTrace(m_errorStream);
	}
	
	@Override
	public void reconnected(SubscriberEvent evt)
	{
		try
		{
			evt.getSubscriber().subscribe(m_info);
		}
		catch (Exception ex)
		{
			ex.printStackTrace(m_errorStream);
		}
	}
}
