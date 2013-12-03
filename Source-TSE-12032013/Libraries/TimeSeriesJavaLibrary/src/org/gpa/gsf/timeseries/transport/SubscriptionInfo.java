//******************************************************************************************************
//  SubscriptionInfo.java - Gbtc
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

package org.gpa.gsf.timeseries.transport;

/**
 * Configuration object for data subscriptions.
 * 
 * @see DataSubscriber
 * @see DataSubscriber#subscribe(SubscriptionInfo)
 */
public class SubscriptionInfo
{
	private String m_filterExpression;
	
	private boolean m_remotelySynchronized;
	private boolean m_throttled;
	
	private boolean m_udpDataChannel;
	private int m_dataChannelLocalPort;
	
	private boolean m_timeIncluded;
	private double m_lagTime;
	private double m_leadTime;
	private boolean m_localClockAsRealTime;
	private boolean m_millisecondResolution;
	
	private String m_startTime;
	private String m_stopTime;
	private String m_constraintParameters;
	private int m_processingInterval;
	
	private String m_waitHandleNames;
	private int m_waitHandleTimeout;
	
	private String m_extraConnectionStringParameters;
	
	/**
	 * Constructs a new subscription info.
	 * The following default values have been overwritten.
	 * 
	 * <ul>
	 *   <li>{@code DataChannelLocalPort = 9500}</li>
	 *   <li>{@code IncludeTime = true}</li>
	 *   <li>{@code LagTime = 10.0}</li>
	 *   <li>{@code LeadTime = 5.0}</li>
	 *   <li>{@code ProcessingInterval = -1}</li>
	 * </ul>
	 */
	public SubscriptionInfo()
	{
		m_dataChannelLocalPort = 9500;
		m_timeIncluded = true;
		m_lagTime = 10.0;
		m_leadTime = 5.0;
		m_processingInterval = -1;
	}
	
	/**
	 * Constructs a new subscription info object that
	 * is a copy of an existing subscription info object.
	 * 
	 * @param info the subscription info to be copied
	 */
	public SubscriptionInfo(SubscriptionInfo info)
	{
		m_filterExpression = info.m_filterExpression;
		
		m_remotelySynchronized = info.m_remotelySynchronized;
		m_throttled = info.m_throttled;
		
		m_udpDataChannel = info.m_udpDataChannel;
		m_dataChannelLocalPort = info.m_dataChannelLocalPort;
		
		m_timeIncluded = info.m_timeIncluded;
		m_lagTime = info.m_lagTime;
		m_leadTime = info.m_leadTime;
		m_localClockAsRealTime = info.m_localClockAsRealTime;
		m_millisecondResolution = info.m_millisecondResolution;
		
		m_startTime = info.m_startTime;
		m_stopTime = info.m_stopTime;
		m_constraintParameters = info.m_constraintParameters;
		m_processingInterval = info.m_processingInterval;
		
		m_waitHandleNames = info.m_waitHandleNames;
		m_waitHandleTimeout = info.m_waitHandleTimeout;
		
		m_extraConnectionStringParameters = info.m_extraConnectionStringParameters;
	}

	/**
	 * Gets the filter expression used to define which
	 * measurements are being requested by the subscriber.
	 * 
	 * @return the filter expression
	 * @see #setFilterExpression(String)
	 */
	public String getFilterExpression()
	{
		return m_filterExpression;
	}

	/**
	 * Sets the filter expression used to define which
	 * measurements are being requested by the subscriber.
	 * 
	 * @param filterExpression the new value for the filter expression
	 * @see #getFilterExpression()
	 */
	public void setFilterExpression(String filterExpression)
	{
		m_filterExpression = filterExpression;
	}

	/**
	 * Gets the flag that indicates whether the subscriber is requesting
	 * that its measurements be synchronized by the publisher.
	 * 
	 * @return the flag that indicates whether the subscriber is requesting
	 *         that its measurements be synchronized by the publisher
	 * @see #setRemotelySynchronized(boolean)
	 */
	public boolean isRemotelySynchronized()
	{
		return m_remotelySynchronized;
	}

	/**
	 * Sets the flag that indicates whether the subscriber is requesting
	 * that its measurements be synchronized by the publisher.
	 * 
	 * @param remotelySynchronized the new value of the flag that indicates
	 *        whether the subscriber is requesting that its measurements be
	 *        synchronized by the publisher
	 * @see #isRemotelySynchronized()
	 */
	public void setRemotelySynchronized(boolean remotelySynchronized)
	{
		m_remotelySynchronized = remotelySynchronized;
	}

	/**
	 * Gets a flag indicating whether the
	 * requested position is to be throttled.
	 * 
	 * @return flag indicating whether the subscription is throttled
	 * @see #setThrottled(boolean)
	 */
	public boolean isThrottled()
	{
		return m_throttled;
	}

	/**
	 * Sets the flag which determines whether to
	 * request that the subscription be throttled.
	 * 
	 * @param throttled the new value for the flag
	 * @see #isThrottled()
	 */
	public void setThrottled(boolean throttled)
	{
		m_throttled = throttled;
	}

	/**
	 * Gets the flag which indicates whether the subscriber is
	 * requesting its data over a separate UDP data channel.
	 * 
	 * @return the flag which indicates whether the subscriber is
	 *         requesting its data over a separate UDP data channel
	 * @see #setUdpDataChannel(boolean)
	 */
	public boolean isUdpDataChannel()
	{
		return m_udpDataChannel;
	}

	/**
	 * Sets the flag which determines whether the subscriber is
	 * requesting its data over a separate UDP data channel.
	 * 
	 * @param udpDataChannel the new value for the flag
	 * @see #isUdpDataChannel()
	 */
	public void setUdpDataChannel(boolean udpDataChannel)
	{
		m_udpDataChannel = udpDataChannel;
	}

	/**
	 * Gets the port that the UDP data channel binds to. This value is
	 * only used when the subscriber requests a separate UDP data channel.
	 * 
	 * @return the local port that the UDP data channel binds to
	 * @see #setDataChannelLocalPort(int)
	 * @see #isUdpDataChannel()
	 */
	public int getDataChannelLocalPort()
	{
		return m_dataChannelLocalPort;
	}

	/**
	 * Sets the port that the UDP data channel binds to. This value is
	 * only used when the subscriber requests a separate UDP data channel.
	 * 
	 * @param dataChannelLocalPort
	 * @see #getDataChannelLocalPort()
	 * @see #setUdpDataChannel(boolean)
	 */
	public void setDataChannelLocalPort(int dataChannelLocalPort)
	{
		m_dataChannelLocalPort = dataChannelLocalPort;
	}

	/**
	 * Gets the flag indicating whether timestamps are included in the
	 * data sent from the publisher. This value is ignored if the data
	 * is remotely synchronized.
	 * 
	 * @return the flag indicating whether timestamps are included
	 * @see #setTimeIncluded(boolean)
	 */
	public boolean isTimeIncluded()
	{
		return m_timeIncluded;
	}

	/**
	 * Sets the flag which determines whether timestamps are included in
	 * the data sent from the publisher. This value is ignored if the data
	 * is remotely synchronized.
	 * 
	 * @param timeIncluded the new value for the flag
	 * @see #isTimeIncluded()
	 */
	public void setTimeIncluded(boolean timeIncluded)
	{
		m_timeIncluded = timeIncluded;
	}

	/**
	 * Gets the allowed past time deviation tolerance, in seconds.
	 * 
	 * @return the allowed past time deviation tolerance
	 * @see #setLagTime(double)
	 */
	public double getLagTime()
	{
		return m_lagTime;
	}

	/**
	 * Sets the allowed past time deviation
	 * tolerance, in seconds (can be subscecond).
	 * 
	 * @param lagTime the new value for the allowed
	 *        past time deviation tolerance
	 * @see #getLagTime()
	 */
	public void setLagTime(double lagTime)
	{
		m_lagTime = lagTime;
	}

	/**
	 * Gets the allowed future time deviation tolerance, in seconds.
	 * 
	 * @return the allowed future time deviation tolerance
	 * @see #setLeadTime(double)
	 */
	public double getLeadTime()
	{
		return m_leadTime;
	}

	/**
	 * Sets the allowed future time deviation
	 * tolerance, in seconds (can be subsecond).
	 * 
	 * @param leadTime the new value for the allowed
	 *        future time deviation tolerance
	 * @see #getLeadTime()
	 */
	public void setLeadTime(double leadTime)
	{
		m_leadTime = leadTime;
	}

	/**
	 * Gets the flag that indicates whether the server's local clock is
	 * used as real-time. Otherwise, the timestamps of the measurements
	 * will be used as real-time.
	 * 
	 * @return the flag that indicates whether the server's local clock
	 *         is used as real-time
	 * @see #setLocalClockAsRealTime(boolean)
	 */
	public boolean isLocalClockAsRealTime()
	{
		return m_localClockAsRealTime;
	}

	/**
	 * Sets the flag which determines whether the server's local clock is
	 * used as real-time. Otherwise, the timestamps of the measurements
	 * will be used as real-time.
	 * 
	 * @param localClockAsRealTime the new value for the flag
	 * @see #isLocalClockAsRealTime()
	 */
	public void setLocalClockAsRealTime(boolean localClockAsRealTime)
	{
		m_localClockAsRealTime = localClockAsRealTime;
	}

	/**
	 * Gets the flag that indicates whether measurement timestamps use
	 * millisecond resolution. Otherwise, they will use 100-nanosecond
	 * resolution.
	 * <p>
	 * This flag determines the size of the timestamps transmitted as
	 * part of the compact measurement format when the server is using
	 * base time offsets.
	 * 
	 * @return the flag that indicates whether measurement timestamps use
	 *         millisecond resolution
	 * @see #setMillisecondResolution(boolean)
	 */
	public boolean isMillisecondResolution()
	{
		return m_millisecondResolution;
	}

	/**
	 * Gets the flag that indicates whether measurement timestamps use
	 * millisecond resolution. Otherwise, they will use 100-nanosecond
	 * resolution.
	 * <p>
	 * This flag determines the size of the timestamps transmitted as
	 * part of the compact measurement format when the server is using
	 * base time offsets.
	 * 
	 * @param millisecondResolution the new value for the flag
	 * @see #isMillisecondResolution()
	 */
	public void setMillisecondResolution(boolean millisecondResolution)
	{
		m_millisecondResolution = millisecondResolution;
	}

	/**
	 * Gets the start time of the requested temporal
	 * session for streaming historic data.
	 * 
	 * @return the start time of the requested temporal session
	 * @see #setStartTime(String)
	 * @see #getStopTime()
	 * @see #getConstraintParameters()
	 */
	public String getStartTime()
	{
		return m_startTime;
	}

	/**
	 * Sets the start time of the requested temporal
	 * session for streaming historic data.
	 * 
	 * @param startTime the new value for the start time
	 * @see #getStartTime()
	 * @see #setStopTime(String)
	 * @see #setConstraintParameters(String)
	 */
	public void setStartTime(String startTime)
	{
		m_startTime = startTime;
	}

	/**
	 * Gets the stop time of the requested temporal
	 * session for streaming historic data.
	 * 
	 * @return the stop time of the requested temporal session
	 * @see #setStopTime(String)
	 * @see #getStartTime()
	 * @see #getConstraintParameters()
	 */
	public String getStopTime()
	{
		return m_stopTime;
	}

	/**
	 * Sets the stop time of the requested temporal
	 * session for streaming historic data.
	 * 
	 * @param stopTime the new value for the stop time
	 * @see #getStopTime()
	 * @see #setStartTime(String)
	 * @see #setConstraintParameters(String)
	 */
	public void setStopTime(String stopTime)
	{
		m_stopTime = stopTime;
	}

	/**
	 * Gets the additional constraint parameters supplied
	 * to temporal adapters in a temporal session.
	 * 
	 * @return the additional constraint parameters for temporal adapters
	 * @see #setConstraintParameters(String)
	 * @see #getStartTime()
	 * @see #getStopTime()
	 */
	public String getConstraintParameters()
	{
		return m_constraintParameters;
	}

	/**
	 * Sets the additional constraint parameters supplied
	 * to temporal adapters in a temporal session.
	 * 
	 * @param constraintParameters the new value for the additional parameters
	 * @see #getConstraintParameters()
	 * @see #setStartTime(String)
	 * @see #setStopTime(String)
	 */
	public void setConstraintParameters(String constraintParameters)
	{
		m_constraintParameters = constraintParameters;
	}

	/**
	 * Gets the processing interval requested by the subscriber. A value
	 * of {@code -1} indicates the default processing interval. A value
	 * of {@code 0} indicates data will be processed as fast as possible.
	 * 
	 * @return the processing interval requested by the subscriber
	 * @see #setProcessingInterval(int)
	 */
	public int getProcessingInterval()
	{
		return m_processingInterval;
	}

	/**
	 * Sets the processing interval requested by the subscriber. A value
	 * of {@code -1} indicates the default processing interval. A value
	 * of {@code 0} indicates data will be processed as fast as possible.
	 * 
	 * @param processingInterval the new value for the processing interval
	 * @see #getProcessingInterval()
	 */
	public void setProcessingInterval(int processingInterval)
	{
		m_processingInterval = processingInterval;
	}

	/**
	 * Gets the wait handle names used to deterministically wait for data
	 * to become available on the server side.
	 * 
	 * @return the wait handle names
	 * @see #setWaitHandleNames(String)
	 * @see #getWaitHandleTimeout()
	 */
	public String getWaitHandleNames()
	{
		return m_waitHandleNames;
	}

	/**
	 * Sets the wait handle names used to deterministically wait for data
	 * to become available on the server side.
	 * 
	 * @param waitHandleNames the new value for the wait handle names
	 * @see #getWaitHandleNames()
	 * @see #setWaitHandleTimeout(int)
	 */
	public void setWaitHandleNames(String waitHandleNames)
	{
		m_waitHandleNames = waitHandleNames;
	}

	/**
	 * Gets the amount of time to wait on the wait handles identified by
	 * the wait handle names before the operation times out.
	 * 
	 * @return the amount of time to wait on the wait handles before timing out
	 * @see #setWaitHandleTimeout(int)
	 * @see #getWaitHandleNames()
	 */
	public int getWaitHandleTimeout()
	{
		return m_waitHandleTimeout;
	}

	/**
	 * Sets the amount of time to wait on the wait handle identified by
	 * the wait handle names before the operation times out.
	 * 
	 * @param waitHandleTimeout the new value for the amount of time to
	 *        wait on the wait handles before timing out
	 * @see #getWaitHandleTimeout()
	 * @see #setWaitHandleNames(String)
	 */
	public void setWaitHandleTimeout(int waitHandleTimeout)
	{
		m_waitHandleTimeout = waitHandleTimeout;
	}

	/**
	 * Gets the additional connection string parameters to be applied
	 * to the connection string sent to the publisher during subscription.
	 * 
	 * @return the additional connection string parameters sent to the
	 *         publisher during subscription
	 * @see #setExtraConnectionStringParameters(String)
	 */
	public String getExtraConnectionStringParameters()
	{
		return m_extraConnectionStringParameters;
	}

	/**
	 * Sets the additional connection string parameters to be applied
	 * to the connection string sent to the publisher during subscription.
	 * 
	 * @param extraConnectionStringParameters the new value for the addtional
	 *        connection string parameters sent to the publisher during
	 *        subscription
	 * @see #getExtraConnectionStringParameters()
	 */
	public void setExtraConnectionStringParameters(String extraConnectionStringParameters)
	{
		m_extraConnectionStringParameters = extraConnectionStringParameters;
	}
}
