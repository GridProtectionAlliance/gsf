//******************************************************************************************************
//  SubscriberListener.java - Gbtc
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

import org.gpa.gsf.timeseries.transport.DataSubscriber;

/**
 * Interface which defines the callbacks
 * used by the subscriber to handle events.
 * 
 * @see DataSubscriber
 * @see DataSubscriber#addSubscriberListener(SubscriberListener)
 */
public interface SubscriberListener
{
	/**
	 * Occurs when the subscriber wishes to
	 * expose a status message to the user.
	 * 
	 * @param evt the message event which contains the status message
	 */
	void statusMessageReceived(MessageEvent evt);
	
	/**
	 * Occurs when the subscriber encounters an exception.
	 * 
	 * @param evt the message event which contains the exception encountered
	 */
	void exceptionEncountered(MessageEvent evt);
	
	/**
	 * Occurs when the data start time is received from the publisher.
	 * 
	 * @param evt the event which contains the data start time received
	 */
	void dataStartTimeReceived(StartTimeEvent evt);
	
	/**
	 * Occurs when metadata is received from the publisher.
	 * 
	 * @param evt the metadata event which contains the metadata received
	 */
	void metadataReceived(MetadataEvent evt);
	
	/**
	 * Occurs when a new collection of measurements
	 * are received from the publisher.
	 * 
	 * @param evt the collection of measurements received
	 */
	void newMeasurementsReceived(MeasurementEvent evt);
	
	/**
	 * Occurs when the publisher notifies the subscriber
	 * that a temporal session has been completed.
	 * 
	 * @param evt the event which contains a message from the publisher
	 */
	void processingCompleteCallback(MessageEvent evt);
	
	/**
	 * Occurs when the TCP connection between the publisher
	 * and subscriber is terminated unexpectedly.
	 * 
	 * @param evt the event which contains an exception that occurred as
	 *        a result of the connection termination
	 */
	void connectionTerminated(MessageEvent evt);
}
