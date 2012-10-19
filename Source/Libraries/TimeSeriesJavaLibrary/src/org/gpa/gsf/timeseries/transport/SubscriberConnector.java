//******************************************************************************************************
//  SubscriberConnector.java - Gbtc
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

package org.gpa.gsf.timeseries.transport;

import java.util.ArrayList;
import java.util.Collection;

import org.gpa.gsf.timeseries.transport.event.MessageEvent;
import org.gpa.gsf.timeseries.transport.event.SubscriberAdapter;
import org.gpa.gsf.timeseries.transport.event.SubscriberConnectorListener;
import org.gpa.gsf.timeseries.transport.event.SubscriberEvent;
import org.gpa.gsf.timeseries.transport.event.SubscriberListener;

/**
 * Helper class to provide retry and auto-reconnect
 * functionality to the subscriber.
 * 
 * @see DataSubscriber
 */
public class SubscriberConnector
{
	Collection<SubscriberConnectorListener> m_listeners;
	private DataSubscriber m_subscriber;
	private SubscriberListener m_autoReconnectHandler;
	
	private String m_hostname;
	private int m_port;
	
	private int m_maxRetries;
	private int m_retryInterval;
	private boolean m_autoReconnect;
	
	private boolean m_cancel;
	
	/**
	 * Constructs a new instance. The default configuration is infinite
	 * retries on a 2-second interval, and auto-reconnect is on. 
	 */
	public SubscriberConnector()
	{
		m_maxRetries = -1;
		m_retryInterval = 2000;
		m_autoReconnect = true;

		m_listeners = new ArrayList<SubscriberConnectorListener>();
	}
	
	/**
	 * Adds the given listener to receive events from this subscriber
	 * connector. If the {@code listener} is {@code null}, no exception
	 * is thrown and no action is performed.
	 * 
	 * @param listener the listener to be added
	 */
	public void addSubscriberConnectorListener(SubscriberConnectorListener listener)
	{
		if (listener != null)
			m_listeners.add(listener);
	}
	
	/**
	 * Removes the given listener so that it no longer receives events
	 * from this subscriber connector. This method performs no function,
	 * nor does it throw an exception, if {@code subscriberListener} was
	 * not previously added to this component. If {@code subscriberListener}
	 * is {@code null}, no exception is thrown and no action is performed.
	 * 
	 * @param listener the listener to be removed
	 */
	public void removeSubscriberConnectorListener(SubscriberConnectorListener listener)
	{
		if (listener != null)
			m_listeners.remove(listener);
	}
	
	/**
	 * Begins the connection sequence.
	 * 
	 * @param subscriber the subscriber to be connected
	 * @return connected status of the subscriber
	 * @throws IllegalArgumentException if {@code subscriber} is {@code null}
	 */
	public boolean connect(DataSubscriber subscriber)
	{
		if (subscriber == null)
			throw new IllegalArgumentException("subscriber must not be null");
		
		// If we are no longer set to auto reconnect, we
		// may need to detach from an existing subscriber.
		if (!m_autoReconnect && m_subscriber != null)
		{
			m_subscriber.removeSubscriberListener(m_autoReconnectHandler);
			m_subscriber = null;
		}
		
		// If we are set to auto reconnect and we are not currently
		// attached to the given subscriber, we need to attach to the
		// new subscriber.
		if (m_autoReconnect && m_subscriber != subscriber)
		{
			if (m_subscriber != null)
				m_subscriber.removeSubscriberListener(m_autoReconnectHandler);
			
			if (m_autoReconnectHandler == null)
				m_autoReconnectHandler = getNewAutoReconnectHandler();
			
			m_subscriber = subscriber;
			subscriber.addSubscriberListener(m_autoReconnectHandler);
		}

		// Attempt to connect for the given number of retries.
		for (int i = 0; !m_cancel && (m_maxRetries == -1 || i < m_maxRetries); i++)
		{
			try
			{
				subscriber.connect(m_hostname, m_port);
				break;
			}
			catch (Exception ex)
			{
				triggerExceptionEncounteredAsync(ex);
				sleep(m_retryInterval);
			}
		}
		
		// Return connection status of subscriber.
		return subscriber.isConnected();
	}
	
	/**
	 * Cancels all current and future connection sequences.
	 */
	public void cancel()
	{
		m_cancel = true;
		
		if (m_subscriber != null)
			m_subscriber.removeSubscriberListener(m_autoReconnectHandler);
	}

	/**
	 * Gets the hostname of the server this connector is connecting to.
	 * 
	 * @return the hostname of the server
	 */
	public String getHostname()
	{
		return m_hostname;
	}

	/**
	 * Sets the hostname of the server this connector is connecting to.
	 * 
	 * @param hostname the new value for the hostname of the server
	 */
	public void setHostname(String hostname)
	{
		m_hostname = hostname;
	}

	/**
	 * Gets the port that the publisher is listening on.
	 * 
	 * @return the port that the publisher is listening on
	 */
	public int getPort()
	{
		return m_port;
	}

	/**
	 * Sets the port that the publisher is listening on.
	 * 
	 * @param port the new value for the port that the publisher
	 *        is listening on
	 */
	public void setPort(int port)
	{
		m_port = port;
	}

	/**
	 * Gets the maximum number of connection attempts to be made before
	 * giving up. A value of {@code -1} means an infinite number of retries.
	 * 
	 * @return the maximum number of retries during a connection sequence
	 */
	public int getMaxRetries()
	{
		return m_maxRetries;
	}

	/**
	 * Sets the maximum number of connection attempts to be made before
	 * giving up. A value of {@code -1} means an infinite number of retries.
	 * 
	 * @param maxRetries the new value for the maximum number of retries
	 */
	public void setMaxRetries(int maxRetries)
	{
		m_maxRetries = maxRetries;
	}

	/**
	 * Gets the interval of time, in milliseconds, between
	 * each connection attempt during a connection sequence.
	 * 
	 * @return the retry interval, in milliseconds
	 */
	public int getRetryInterval()
	{
		return m_retryInterval;
	}

	/**
	 * Sets the interval of time, in milliseconds, between
	 * each connection attempt during a connection sequence.
	 * 
	 * @param retryInterval the new value for the retry interval,
	 *        in milliseconds
	 */
	public void setRetryInterval(int retryInterval)
	{
		m_retryInterval = retryInterval;
	}

	/**
	 * Gets the flag that indicates whether this connector should
	 * automatically attempt to reconnect the subscriber when the
	 * connection is terminated.
	 * 
	 * @return the auto-reconnect flag
	 */
	public boolean isAutoReconnect()
	{
		return m_autoReconnect;
	}

	/**
	 * Sets the flag that indicates whether this connector should
	 * automatically attempt to reconnect the subscriber when the
	 * connection is terminated.
	 * 
	 * @param autoReconnect the new value for the auto-reconnect flag
	 */
	public void setAutoReconnect(boolean autoReconnect)
	{
		m_autoReconnect = autoReconnect;
	}
	
	// Creates and returns a new auto-reconnect handler.
	private SubscriberListener getNewAutoReconnectHandler()
	{
		return new SubscriberAdapter()
		{
			@Override
			public void connectionTerminated(MessageEvent evt)
			{
				if (m_autoReconnect)
				{
					if (!m_cancel)
						triggerExceptionEncountered(evt.getException());
					
					connect(m_subscriber);
					
					if (!m_cancel)
						triggerReconnected();
				}
			}
		};
	}
	
	// Triggers the exceptionEncountered event for
	// all listeners subscribed to this connector.
	// The event is triggered on a separate thread.
	private void triggerExceptionEncounteredAsync(final Exception ex)
	{
		new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				triggerExceptionEncountered(ex);
			}
		}).start();
	}

	// Triggers the exceptionEncountered event for
	// all listeners subscribed to this connector.
	private void triggerExceptionEncountered(Exception encounteredException)
	{
		for (SubscriberConnectorListener listener : m_listeners)
		{
			try
			{
				listener.exceptionEncountered(new MessageEvent(this, encounteredException));
			}
			catch (Exception ex)
			{
				// Ignore exceptions encountered
				// while reporting exceptions
			}
		}
	}
	
	// Triggers the reconnected event for all
	// listeners subscribed to this connector.
	private void triggerReconnected()
	{
		for (SubscriberConnectorListener listener : m_listeners)
		{
			try
			{
				listener.reconnected(new SubscriberEvent(this, m_subscriber));
			}
			catch (Exception ex)
			{
				triggerExceptionEncountered(ex);
			}
		}
	}
	
	// Puts the current thread to sleep for the given period of time.
	private void sleep(long millis)
	{
		try
		{
			Thread.sleep(millis);
		}
		catch (InterruptedException ex)
		{
			triggerExceptionEncountered(ex);
		}
	}
}
