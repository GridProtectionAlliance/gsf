//******************************************************************************************************
//  SubscriberConnectorListener.java - Gbtc
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

/**
 * Interface which defines callbacks for the {@link SubscriberConnector}.
 */
public interface SubscriberConnectorListener
{
	/**
	 * Called when an exception is encountered
	 * by the {@code SubscriberConnector}.
	 * 
	 * @param evt the event which contains the exception
	 */
	void exceptionEncountered(MessageEvent evt);
	
	/**
	 * Occurs when the subscriber is automatically
	 * reconnected by the {@code SubscriberConnector}.
	 * 
	 * @param evt the event which contains the subscriber
	 *        that was reconnected
	 */
	void reconnected(SubscriberEvent evt);
}
