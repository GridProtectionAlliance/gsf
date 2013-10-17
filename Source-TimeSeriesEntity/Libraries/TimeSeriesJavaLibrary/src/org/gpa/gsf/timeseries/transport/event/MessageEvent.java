//******************************************************************************************************
//  MessageEvent.java - Gbtc
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
 * Event that occurs when the subscriber
 * needs to expose a message to the user.
 */
public class MessageEvent extends EventObject
{
	private String m_message;
	private Exception m_exception;
	
	/**
	 * Constructs a new instance.
	 * 
	 * @param source the source of the message
	 * @param message the message to be exposed
	 */
	public MessageEvent(Object source, String message)
	{
		super(source);
		m_message = message;
	}
	
	/**
	 * Constructs a new instance associated with an exception.
	 * 
	 * @param source the source of the exception
	 * @param exception the exception to be exposed
	 */
	public MessageEvent(Object source, Exception exception)
	{
		this(source, exception.getMessage());
		m_exception = exception;
	}

	/**
	 * Gets the message to be exposed to the user. If this
	 * {@code MessageEvent} is associated with an exception,
	 * this is equivalent to calling {@code getException().getMessage()}.
	 * 
	 * @return the message to be exposed to the user
	 */
	public String getMessage()
	{
		return m_message;
	}

	/**
	 * Gets the exception associated with this
	 * message event, or null if there is none.
	 * 
	 * @return the exception associated with the message event,
	 *         if there is one
	 */
	public Exception getException()
	{
		return m_exception;
	}
	
	private static final long serialVersionUID = -6228918892670504322L;
}
