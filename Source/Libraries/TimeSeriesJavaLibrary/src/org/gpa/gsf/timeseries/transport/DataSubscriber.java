//******************************************************************************************************
//  DataSubscriber.java - Gbtc
//
//  Copyright � 2010, Grid Protection Alliance.  All Rights Reserved.
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

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collection;
import java.util.UUID;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;
import java.util.zip.GZIPInputStream;

import org.gpa.gsf.timeseries.Measurement;
import org.gpa.gsf.timeseries.transport.constant.CompressionMode;
import org.gpa.gsf.timeseries.transport.constant.DataPacketFlags;
import org.gpa.gsf.timeseries.transport.constant.OperationalEncoding;
import org.gpa.gsf.timeseries.transport.constant.OperationalModes;
import org.gpa.gsf.timeseries.transport.constant.ServerCommand;
import org.gpa.gsf.timeseries.transport.constant.ServerResponse;
import org.gpa.gsf.timeseries.transport.event.MeasurementEvent;
import org.gpa.gsf.timeseries.transport.event.MessageEvent;
import org.gpa.gsf.timeseries.transport.event.MetadataEvent;
import org.gpa.gsf.timeseries.transport.event.StartTimeEvent;
import org.gpa.gsf.timeseries.transport.event.SubscriberListener;
import org.gpa.gsf.timeseries.util.ArrayExtensions;
import org.gpa.gsf.timeseries.util.InputStreamExtensions;
import org.gpa.gsf.timeseries.util.Masks;
import org.gpa.gsf.timeseries.util.StringExtensions;

/**
 * Implementation of the Subscriber API that will connect
 * to a data publisher for a data subscription.
 */
public class DataSubscriber
{
	private SubscriptionInfo m_currentSubscription;
	private InetAddress[] m_allHostAddresses;
	private InetAddress m_hostAddress;
	private int m_operationalModes;
	private Charset m_characterEncoding;
	private boolean m_unsubscribing;
	private boolean m_disconnecting;
	
	// Statistics counters
	private long m_totalCommandChannelBytesReceived;
	private long m_totalDataChannelBytesReceived;
	private long m_totalMeasurementsReceived;
	private boolean m_connected;
	private boolean m_subscribed;
	
	// Measurement parsing
	private SignalIndexCache m_signalIndexCache;
	private long[] m_baseTimeOffsets;
	
	// Command thread members
	private Thread m_commandThread;
	private BlockingQueue<byte[]> m_commandQueue;
	
	// Callback thread members
	private Thread m_callbackThread;
	private BlockingQueue<Runnable> m_callbackQueue;
	
	// Command channel
	private Thread m_commandChannelResponseThread;
	private Socket m_commandChannelSocket;
	
	// Data channel
	private Thread m_dataChannelResponseThread;
	private DatagramSocket m_dataChannelSocket;
	
	// Callbacks
	private Collection<SubscriberListener> m_subscriberListeners;
	
	/**
	 * Constructs a new data subscriber.
	 */
	public DataSubscriber()
	{
		m_commandQueue = new LinkedBlockingQueue<byte[]>();
		m_callbackQueue = new LinkedBlockingQueue<Runnable>();
		m_subscriberListeners = new ArrayList<SubscriberListener>();
		
		setOperationalModes(OperationalModes.UseCommonSerializationFormat | OperationalEncoding.Unicode | OperationalModes.CompressMetadata);
	}
	
	/**
	 * Adds the given listener to receive events from this subscriber.
	 * If the {@code listener} is {@code null}, no exception is thrown
	 * and no action is performed.
	 * 
	 * @param subscriberListener the listener to be added
	 */
	public void addSubscriberListener(SubscriberListener subscriberListener)
	{
		if (subscriberListener != null)
			m_subscriberListeners.add(subscriberListener);
	}
	
	/**
	 * Removes the given listener so that it no longer receives events
	 * from this subscriber. This method performs no function, nor does
	 * it throw an exception, if {@code subscriberListener} was not
	 * previously added to this component. If {@code subscriberListener}
	 * is {@code null}, no exception is thrown and no action is performed.
	 * 
	 * @param subscriberListener the listener to be removed
	 */
	public void removeSubscriberListener(SubscriberListener subscriberListener)
	{
		if (subscriberListener != null)
			m_subscriberListeners.remove(subscriberListener);
	}

	/**
	 * Gets the operational modes currently in use by this subscriber.
	 * 
	 * @return the subscriber's operational modes
	 */
	public int getOperationalModes()
	{
		return m_operationalModes;
	}

	/**
	 * Sets the operational modes of the subscriber.
	 * <p>
	 * The operational modes define a set of flags that can be used to
	 * indicate to the publisher how the subscriber wishes to communicate
	 * with it. This method will throw an {@code UnsupportedOperationException}
	 * if any of the following are true.
	 * <p>
	 * <ul>
	 *   <li>{@link OperationalModes.UseCommonSerializationFormat} is unset</li>
	 *   <li>{@link OperationalModes.CompressSignalIndexCache} is set</li>
	 * </ul>
	 * 
	 * @param operationalModes the new operational modes
	 * @throws UnsupportedOperationException if an unsupported operational mode is detected
	 * @see OperationalModes
	 */
	public void setOperationalModes(int operationalModes)
	{
		int operationalEncoding;
		
		// .NET serialization format not supported
		if ((operationalModes & OperationalModes.UseCommonSerializationFormat) == 0)
			throw new UnsupportedOperationException("Cannot use .NET serialization format. Use common serialization format instead.");

		// Update operational modes and character encoding
		m_operationalModes = operationalModes;
		operationalEncoding = operationalModes & OperationalModes.EncodingMask;
		
		switch (operationalEncoding)
		{
		case OperationalEncoding.Unicode:
			m_characterEncoding = Charset.forName("UTF-16LE");
			break;
			
		case OperationalEncoding.BigEndianUnicode:
			m_characterEncoding = Charset.forName("UTF-16BE");
			break;
			
		case OperationalEncoding.UTF8:
			m_characterEncoding = Charset.forName("UTF-8");
			break;
			
		case OperationalEncoding.OperatingSystemDefault:
			m_characterEncoding = Charset.defaultCharset();
			break;
		}
		
		// If subscriber is connected, send automatically
		if (m_connected)
			sendOperationalModes();
	}
	
	/**
	 * Connects to the publisher listening at the given hostname and port.
	 * 
	 * @param hostname name or address of the machine hosting the publisher
	 * @param port the port that the publisher is listening on
	 * @throws IllegalStateException if the subscriber is already connected
	 * @throws IOException if an I/O error occurs when creating the socket
	 * @see #disconnect()
	 */
	public void connect(String hostname, int port) throws IOException
	{
		m_totalCommandChannelBytesReceived = 0L;
		m_totalDataChannelBytesReceived = 0L;
		m_totalMeasurementsReceived = 0L;
		
		if (m_connected)
			throw new IllegalStateException("Subscriber is already connected; disconnect first");
		
		m_commandChannelSocket = new Socket(hostname, port);
		m_hostAddress = m_commandChannelSocket.getInetAddress();

		if (m_hostAddress.isAnyLocalAddress())
			m_hostAddress = InetAddress.getLocalHost();
		
		try
		{
			m_allHostAddresses = InetAddress.getAllByName(m_hostAddress.getHostName());
		}
		catch (UnknownHostException ex)
		{
			m_allHostAddresses = new InetAddress[0];
		}
		
		startCommandThread();
		startCallbackThread();
		startCommandChannelResponseThread();

		sendOperationalModes();
		m_connected = true;
	}
	
	/**
	 * Disconnects from the data publisher. This method does not return
	 * until all connections have been closed and all threads spawned by
	 * the subscriber have shut down gracefully (with the exception of
	 * the thread that executes the connection terminated callback).
	 * 
	 * @see #connect(String, int)
	 */
	public void disconnect()
	{
		// Notify running threads that
		// the subscriber is disconnecting
		m_unsubscribing = true;
		m_disconnecting = true;

		try
		{
			// Close sockets so that threads using
			// them can shut down gracefully
			if (m_commandChannelSocket != null)
				m_commandChannelSocket.close();
		}
		catch (IOException ex)
		{
			// Ignore exceptions and
			// continue shutdown procedure
		}
		
		if (m_dataChannelSocket != null)
			m_dataChannelSocket.close();

		// Join with all threads to guarantee their completion
		// before returning control to the caller
		joinThread(m_commandThread);
		joinThread(m_callbackThread);
		joinThread(m_commandChannelResponseThread);
		joinThread(m_dataChannelResponseThread);
		
		// Empty queues so they can be used again later
		m_commandQueue.clear();
		m_callbackQueue.clear();
		
		// Release references to threads and
		// sockets that we are no longer using
		m_commandThread = null;
		m_callbackThread = null;
		m_commandChannelResponseThread = null;
		m_dataChannelResponseThread = null;
		m_commandChannelSocket = null;
		m_dataChannelSocket = null;
		
		// Disconnect completed
		m_unsubscribing = false;
		m_subscribed = false;
		m_disconnecting = false;
		m_connected = false;
	}
	
	/**
	 * Subscribes to the signals specified by the
	 * filter expression in the {@code info} object.
	 * 
	 * @param info subscription info used to configure the subscription
	 * @throws SocketException if, when defining a UDP data channel, the
	 *         socket could not be opened, or the socket could not bind
	 *         to the specified local port
	 * @see #unsubscribe()
	 */
	public void subscribe(SubscriptionInfo info) throws SocketException
	{
		StringBuilder connectionStringBuilder = new StringBuilder();
		byte[] connectionStringBytes;
		ByteBuffer packetData;
		int packetDataSize;
		
		// Make sure to unsubscribe before attempting another
		// subscription so we don't leave connections open
		if (m_subscribed)
			unsubscribe();
		
		m_currentSubscription = new SubscriptionInfo(info);
		m_totalMeasurementsReceived = 0L;
		
		connectionStringBuilder.append("trackLatestMeasurements").append(info.isThrottled()).append(';');
		connectionStringBuilder.append("includeTime=").append(info.isTimeIncluded()).append(';');
		connectionStringBuilder.append("lagTime=").append(info.getLagTime()).append(';');
		connectionStringBuilder.append("leadTime=").append(info.getLeadTime()).append(';');
		connectionStringBuilder.append("useLocalClockAsRealTime=").append(info.isLocalClockAsRealTime()).append(';');
		connectionStringBuilder.append("processingInterval=").append(info.getProcessingInterval()).append(';');
		connectionStringBuilder.append("useMillisecondResolution=").append(info.isMillisecondResolution()).append(';');
		connectionStringBuilder.append("assemblyInfo={source=TSF Java Library;version=0.9.0;buildDate=April 2012};");
		
		if (!StringExtensions.isNullOrWhitespace(info.getFilterExpression()))
			connectionStringBuilder.append("inputMeasurementKeys={").append(info.getFilterExpression()).append("};");
		
		if (info.isUdpDataChannel())
		{
			m_dataChannelSocket = new DatagramSocket(info.getDataChannelLocalPort(), m_commandChannelSocket.getLocalAddress());
			startDataChannelResponseThread();
			connectionStringBuilder.append("dataChannel={localport=").append(info.getDataChannelLocalPort()).append("};");
		}
		
		if (!StringExtensions.isNullOrWhitespace(info.getStartTime()))
			connectionStringBuilder.append("startTimeConstraint=").append(info.getStartTime()).append(';');
		
		if (!StringExtensions.isNullOrWhitespace(info.getStopTime()))
			connectionStringBuilder.append("stopTimeConstraint=").append(info.getStopTime()).append(';');
		
		if (!StringExtensions.isNullOrWhitespace(info.getConstraintParameters()))
			connectionStringBuilder.append("timeConstraintParameters=").append(info.getConstraintParameters()).append(';');
		
		if (!StringExtensions.isNullOrWhitespace(info.getWaitHandleNames()))
		{
			connectionStringBuilder.append("waitHandleNames=").append(info.getWaitHandleNames()).append(';');
			connectionStringBuilder.append("waitHandleTimeout=").append(info.getWaitHandleTimeout()).append(';');
		}
		
		if (!StringExtensions.isNullOrWhitespace(info.getExtraConnectionStringParameters()))
			connectionStringBuilder.append(info.getExtraConnectionStringParameters()).append(';');

		connectionStringBytes = connectionStringBuilder.toString().getBytes(m_characterEncoding);
		packetDataSize = 5 + connectionStringBytes.length;
		packetData = ByteBuffer.allocate(packetDataSize);
		
		packetData.put((byte)(DataPacketFlags.Compact | (info.isRemotelySynchronized() ? DataPacketFlags.Synchronized : DataPacketFlags.NoFlags)));
		packetData.putInt(connectionStringBytes.length);
		packetData.put(connectionStringBytes);
		
		sendServerCommand(ServerCommand.Subscribe, packetData.array(), 0, packetDataSize);
	}
	
	/**
	 * Gets a copy of the subscription info currently in use by the subscriber.
	 * 
	 * @return copy of the subscription info currently in use by the subscriber
	 */
	public SubscriptionInfo getCurrentSubscription()
	{
		return new SubscriptionInfo(m_currentSubscription);
	}
	
	/**
	 * Unsubscribes to stop receiving data from the publisher.
	 */
	public void unsubscribe()
	{
		// Notify data channel response
		// thread that we are unsubscribing
		m_unsubscribing = true;
		
		// Close socket so that data channel
		// response thread can finish
		if (m_dataChannelSocket != null)
			m_dataChannelSocket.close();
		
		// Join with data channel response thread to ensure
		// that it finishes before returning from this method
		joinThread(m_dataChannelResponseThread);
		
		// Unsubscribe complete
		m_unsubscribing = false;

		// Notify server to stop sending data
		sendServerCommand(ServerCommand.Unsubscribe);
	}
	
	/**
	 * Sends a command to the publisher.
	 * 
	 * @param commandCode the {@link ServerCommand} code to be sent
	 */
	public void sendServerCommand(byte commandCode)
	{
		sendServerCommand(commandCode, null, 0, 0);
	}
	
	/**
	 * Sends a command to the publisher over the command channel.
	 * 
	 * @param commandCode the {@link ServerCommand} code to be sent
	 * @param data packet data to be sent to the server with the command
	 * @param offset offset in the buffer at which data sent to the server starts
	 * @param length amount of data in buffer that gets sent to the server
	 */
	public void sendServerCommand(byte commandCode, byte[] data, int offset, int length)
	{
		final int PayloadHeaderSize = 8;
		
		int packetSize = 1 + length;
		int payloadSize = PayloadHeaderSize + packetSize;
		byte[] command = new byte[payloadSize];
		
		// Insert payload marker
		command[0] = (byte)0xAA;
		command[1] = (byte)0xBB;
		command[2] = (byte)0xCC;
		command[3] = (byte)0xDD;
		
		// Insert packet size
		command[4] = (byte)packetSize;
		command[5] = (byte)(packetSize >> 8);
		command[6] = (byte)(packetSize >> 16);
		command[7] = (byte)(packetSize >> 24);
		
		// Insert command code
		command[8] = commandCode;
		
		if (data != null)
			ArrayExtensions.blockCopy(data, offset, command, PayloadHeaderSize + 1, length);

		m_commandQueue.add(command);
	}
	
	/**
	 * Sends operational modes to the server.
	 * 
	 * @see #setOperationalModes(int)
	 * @see #getOperationalModes()
	 */
	public void sendOperationalModes()
	{
		byte[] commandData = ByteBuffer.allocate(4).putInt(m_operationalModes).array();
		sendServerCommand(ServerCommand.DefineOperationalModes, commandData, 0, 4);
	}
	
	/**
	 * Gets the total number of bytes received on
	 * the command channel since last connection.
	 * 
	 * @return the total number of bytes received on the command channel
	 */
	public long getTotalCommandChannelBytesReceived()
	{
		return m_totalCommandChannelBytesReceived;
	}
	
	/**
	 * Gets the total number of bytes received on
	 * the data channel since last connection.
	 * 
	 * @return the total number of bytes received on the data channel
	 */
	public long getTotalDataChannelBytesReceived()
	{
		return m_totalDataChannelBytesReceived;
	}
	
	/**
	 * Gets the total number of measurements
	 * received since last subscription.
	 * 
	 * @return the total number of measurements received
	 */
	public long getTotalMeasurementsReceived()
	{
		return m_totalMeasurementsReceived;
	}
	
	/**
	 * Indicates whether the subscriber is
	 * currently connected to the publisher.
	 * 
	 * @return flag indicating whether subscriber is connected
	 */
	public boolean isConnected()
	{
		return m_connected;
	}
	
	/**
	 * Indicates whether the subscriber is
	 * currently subscribed to the publisher.
	 * 
	 * @return flag indicating whether subscriber is subscribed
	 */
	public boolean isSubscribed()
	{
		return m_subscribed;
	}
	
	/**
	 * Disconnects in case sockets are
	 * open or threads are still running.
	 */
	@Override
	protected void finalize() throws Throwable
	{
		if (m_connected)
			disconnect();
	}
	
	// Joins with the given thread, checking for null
	// and ignoring possible InterruptedExceptions. 
	private void joinThread(Thread th)
	{
		try
		{
			if (th != null)
				th.join();
		}
		catch(InterruptedException ex)
		{
			// Ignore exception and
			// continue shutdown procedure
		}
	}
	
	// Creates and starts the command thread.
	private void startCommandThread()
	{
		m_commandThread = new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				runCommandThread();
			}
		}, "CommandThread");
		
		m_commandThread.start();
	}
	
	// Creates and starts the callback thread.
	private void startCallbackThread()
	{
		m_callbackThread = new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				runCallbackThread();
			}
		},"CallbackThread");

		m_callbackThread.start();
	}
	
	// Creates and starts the command channel response thread.
	private void startCommandChannelResponseThread()
	{
		m_commandChannelResponseThread = new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				runCommandChannelResponseThread();
			}
		},"CommandChannelResponseThread");

		m_commandChannelResponseThread.start();
	}
	
	// Creates and starts the data channel response thread.
	private void startDataChannelResponseThread()
	{
		m_dataChannelResponseThread = new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				runDataChannelResponseThread();
			}
		});

		m_dataChannelResponseThread.start();
	}
	
	// Sends commands queued by other threads to the publisher.
	private void runCommandThread()
	{
		try
		{
			OutputStream out = m_commandChannelSocket.getOutputStream();
			byte[] command;
			
			while (!m_disconnecting)
			{
				command = m_commandQueue.poll(PollTimeout, PollTimeoutUnit);
				
				if (command != null)
					out.write(command);
			}
		}
		catch (InterruptedException ex)
		{
			// This thread is internal and
			// should never be interrupted
			throw new IllegalStateException(ex);
		}
		catch (IOException ex)
		{
			if (!m_disconnecting)
				dispatchConnectionTerminated(ex);
		}
	}
	
	// Executes callbacks queued by other threads.
	private void runCallbackThread()
	{
		try
		{
			Runnable callback;
			
			while (!m_disconnecting)
			{
				callback = m_callbackQueue.poll(PollTimeout, PollTimeoutUnit);
				
				if (callback != null)
				{
					try
					{
						callback.run();
					}
					catch (Exception ex)
					{
						dispatchException(ex);
					}
				}
			}
		}
		catch (InterruptedException ex)
		{
			// This thread is internal and
			// should never be interrupted
			throw new IllegalStateException(ex);
		}
	}
	
	// Processes data received from the publisher on the command channel.
	private void runCommandChannelResponseThread()
	{
		try
		{
			final int PayloadHeaderSize = 8;
			
			InputStream in = m_commandChannelSocket.getInputStream();

			ByteBuffer littleEndianBuffer = ByteBuffer.allocate(PayloadHeaderSize);
			ByteBuffer bigEndianBuffer = ByteBuffer.allocate(MaxPacketSize);
			int bytesRead;
			
			int payloadBodySize;
			
			littleEndianBuffer.order(ByteOrder.LITTLE_ENDIAN);
			bigEndianBuffer.order(ByteOrder.BIG_ENDIAN);
			
			while (true)
			{
				// Read the payload header from the input stream
				bytesRead = InputStreamExtensions.read(in, littleEndianBuffer.array(), 0, PayloadHeaderSize);
				m_totalCommandChannelBytesReceived += bytesRead;
				
				if (m_disconnecting || bytesRead < PayloadHeaderSize)
					break;

				// Skip the 4-byte sync pattern
				payloadBodySize = littleEndianBuffer.getInt(4);
				
				// Increase the buffer size, if necessary
				if (payloadBodySize > bigEndianBuffer.capacity())
					bigEndianBuffer = ByteBuffer.allocate(payloadBodySize);
				
				// Read the packet from the input stream
				bytesRead = InputStreamExtensions.read(in, bigEndianBuffer.array(), 0, payloadBodySize);
				m_totalCommandChannelBytesReceived += bytesRead;
				
				if (m_disconnecting || bytesRead < payloadBodySize)
					break;
				
				// Set the limit on the buffer, process the
				// buffer, and then rewind the buffer
				bigEndianBuffer.limit(payloadBodySize);
				processServerResponse(bigEndianBuffer);
				bigEndianBuffer.rewind();
			}
		}
		catch (IOException ex)
		{
			if (!m_disconnecting)
				dispatchConnectionTerminated(ex);
		}
	}
	
	// Processes data received by the publisher on the data channel.
	private void runDataChannelResponseThread()
	{
		DatagramPacket packet = new DatagramPacket(new byte[MaxPacketSize], MaxPacketSize);
		
		try
		{
			while (true)
			{
				m_dataChannelSocket.receive(packet);
				
				if (m_unsubscribing)
					break;
				
				if (isValidHostAddress(packet.getAddress()))
				{
					m_totalDataChannelBytesReceived += packet.getLength();
					processServerResponse(ByteBuffer.wrap(packet.getData(), packet.getOffset(), packet.getLength()));
				}
			}
		}
		catch (IOException ex)
		{
			if (!m_unsubscribing)
				dispatchException(ex);
		}
	}
	
	// Delegates data received from the publisher to the proper handler.
	private void processServerResponse(ByteBuffer buffer)
	{
		byte responseCode = buffer.get();
		byte commandCode = buffer.get();
		
		// Skip length field in packet header,
		// as length has already been determined
		// by checking the payload header
		buffer.getInt();
		
		switch (responseCode)
		{
		case ServerResponse.Succeeded:
			handleSucceeded(commandCode, buffer);
			break;
			
		case ServerResponse.Failed:
			handleFailed(commandCode, buffer);
			break;
			
		case ServerResponse.DataPacket:
			handleDataPacket(buffer);
			break;
			
		case ServerResponse.DataStartTime:
			handleDataStartTime(buffer);
			break;
			
		case ServerResponse.ProcessingComplete:
			handleProcessingComplete(buffer);
			break;
			
		case ServerResponse.UpdateSignalIndexCache:
			handleUpdateSignalIndexCache(buffer);
			break;
			
		case ServerResponse.UpdateBaseTimes:
			handleUpdateBaseTimes(buffer);
			break;
		}
	}
	
	// Handle success messages from the publisher.
	private void handleSucceeded(byte commandCode, ByteBuffer buffer)
	{
		StringBuilder messageBuilder = new StringBuilder();
		String hexCommandCode = Integer.toHexString(commandCode & Masks.getIntMask(Byte.SIZE));
		
		switch (commandCode)
		{
		case ServerCommand.MetadataRefresh:
			// Metadata refresh message is not sent with a
			// message, but rather the metadata itself
			handleMetadataRefresh(buffer);
			break;
			
		case ServerCommand.Subscribe:
		case ServerCommand.Unsubscribe:
			// Do not break on these messages because there is
			// still an associated message to be processed
			m_subscribed = (commandCode == ServerCommand.Subscribe);
			
		case ServerCommand.Authenticate:
		case ServerCommand.RotateCipherKeys:
			// Each of these responses comes with a message that will
			// be delivered to the user via the status message callback
			messageBuilder.append("Received success code in response to server command 0x").append(hexCommandCode).append(": ");
			messageBuilder.append(m_characterEncoding.decode(buffer));
			
			dispatchStatusMessage(messageBuilder.toString());
			break;
			
		default:
			// If we don't know what the message is, we can't interpret
			// the data sent with the packet. Deliver an error message
			// to the user via the exception encountered callback
			messageBuilder.append("Received success code in response to unknown server command 0x").append(hexCommandCode);
			dispatchException(new UnsupportedOperationException(messageBuilder.toString()));
			break;
		}
	}
	
	// Handle failure messages from the publisher.
	private void handleFailed(byte commandCode, ByteBuffer buffer)
	{
		StringBuilder messageBuilder = new StringBuilder();
		String hexCommandCode = Integer.toHexString(commandCode & Masks.getIntMask(Byte.SIZE));
		
		messageBuilder.append("Received failure code from server command 0x").append(hexCommandCode).append(": ");
		messageBuilder.append(m_characterEncoding.decode(buffer));
		dispatchException(new RuntimeException(messageBuilder.toString()));
	}
	
	// Handle metadata received from the publisher.
	private void handleMetadataRefresh(ByteBuffer buffer)
	{
		ByteBuffer buf = buffer;
		
		if ((m_operationalModes & OperationalModes.CompressMetadata) != 0)
		{
			if ((m_operationalModes & OperationalModes.CompressionModeMask) == CompressionMode.GZIP)
			{
				buf = decompress(buffer);
			}
		}
		
		dispatchMetadata(m_characterEncoding.decode(buf).toString());
	}
	
	// Handle data packets received from the publisher.
	private void handleDataPacket(ByteBuffer buffer)
	{
		GatewayMeasurementParser parser;
		Collection<Measurement> newMeasurements;
		
		byte dataPacketFlags;
		boolean compactFlag;
		boolean syncFlag;
		long frameLevelTimestamp = 0L;
		
		SubscriptionInfo info;
		boolean includeTime;
		boolean useMillisecondResolution;
		
		// Get relevante information about the current subscription
		info = m_currentSubscription;
		includeTime = info.isTimeIncluded();
		useMillisecondResolution = info.isMillisecondResolution();
		
		// Parse the data packet flags
		dataPacketFlags = buffer.get();
		compactFlag = (dataPacketFlags & DataPacketFlags.Compact) != 0;
		syncFlag = (dataPacketFlags & DataPacketFlags.Synchronized) != 0;
		
		// Gather statistics
		m_totalMeasurementsReceived += buffer.getInt();
		
		if (syncFlag)
		{
			frameLevelTimestamp = buffer.getLong();
			includeTime = false;
		}
		
		if (compactFlag)
		{
			// If signal index cache is null, we
			// cannot parse compact measurements
			if (m_signalIndexCache == null)
				return;
			
			parser = new CompactMeasurementParser(m_signalIndexCache, m_baseTimeOffsets, includeTime, useMillisecondResolution);
		}
		else
		{
			// Not supported; dispatch exception and give up
			dispatchException(new UnsupportedOperationException("Non-compact measurement format not supported"));
			return;
		}
		
		try
		{
			// Parse measurements and expose them via listener
			newMeasurements = parser.parseMeasurements(buffer);
			
			if (syncFlag)
			{
				// Apply frame-level timestamp
				for (Measurement newMeasurement : newMeasurements)
					newMeasurement.setTimestamp(frameLevelTimestamp);
			}
			
			if (newMeasurements.size() > 0)
				dispatchNewMeasurements(newMeasurements);
		}
		catch (Exception ex)
		{
			// Notify of exceptions during parse
			dispatchException(ex);
		}
	}
	
	// Handles data start time message received from publisher.
	private void handleDataStartTime(ByteBuffer buffer)
	{
		dispatchDataStartTime(buffer.getLong());
	}
	
	// Handles processing complete message received from publisher.
	private void handleProcessingComplete(ByteBuffer buffer)
	{
		dispatchProcessingComplete(m_characterEncoding.decode(buffer).toString());		
	}
	
	// Updates the signal index cache with new values received from the publisher.
	private void handleUpdateSignalIndexCache(ByteBuffer buffer)
	{
		ByteBuffer buf = buffer;
		SignalIndexCache newCache = new SignalIndexCache();
		int referenceCount;
		
		short signalIndex;
		UUID signalId;
		int sourceSize;
		String source;
		int id;
		
		if ((m_operationalModes & OperationalModes.CompressSignalIndexCache) != 0)
		{
			if ((m_operationalModes & OperationalModes.CompressionModeMask) == CompressionMode.GZIP)
			{
				buf = decompress(buffer);
			}
		}
		
		// Skip 4-byte length and 16-byte subscriber ID
		// We may need to parse these in the future...
		buf.position(buf.position() + 20);
		
		referenceCount = buf.getInt();
		
		for (int i = 0; i < referenceCount; i++)
		{
			signalIndex = buf.getShort();
			signalId = new UUID(buf.getLong(), buf.getLong());
			sourceSize = buf.getInt();
			source = m_characterEncoding.decode((ByteBuffer)buf.slice().limit(sourceSize)).toString();
			id = ((ByteBuffer)buf.position(buf.position() + sourceSize)).getInt();
			
			newCache.addMeasurementKey(signalIndex, signalId, source, id);
		}
		
		m_signalIndexCache = newCache;
	}
	
	// Updates the base times with new values received from the publisher.
	private void handleUpdateBaseTimes(ByteBuffer buffer)
	{
		// Skip time index
		buffer.getInt();
		
		m_baseTimeOffsets = new long[] { buffer.getLong(), buffer.getLong() };
	}
	
	// Queues a status message callback.
	private void dispatchStatusMessage(final String message)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.statusMessageReceived(new MessageEvent(DataSubscriber.this, message));
			}
		});
	}
	
	// Queues an exception encountered callback.
	private void dispatchException(final Exception exception)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.exceptionEncountered(new MessageEvent(DataSubscriber.this, exception));
			}
		});
	}
	
	// Queues a data start time received callback.
	private void dispatchDataStartTime(final long dataStartTime)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.dataStartTimeReceived(new StartTimeEvent(DataSubscriber.this, dataStartTime));
			}
		});
	}
	
	// Queues a metadata received callback.
	private void dispatchMetadata(final byte[] compressedMetadata)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.metadataReceived(new MetadataEvent(DataSubscriber.this, compressedMetadata));
			}
		});
	}
	
	// Queues a metadata received callback.
	private void dispatchMetadata(final String xmlMetadata)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.metadataReceived(new MetadataEvent(DataSubscriber.this, xmlMetadata));
			}
		});
	}
	
	// Queues a new measurements callback.
	private void dispatchNewMeasurements(final Collection<Measurement> newMeasurements)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.newMeasurementsReceived(new MeasurementEvent(DataSubscriber.this, newMeasurements));
			}
		});
	}
	
	// Queues a processing complete callback.
	private void dispatchProcessingComplete(final String message)
	{
		m_callbackQueue.add(new Runnable()
		{
			@Override
			public void run()
			{
				for (SubscriberListener listener : m_subscriberListeners)
					listener.processingCompleteCallback(new MessageEvent(DataSubscriber.this, message));
			}
		});
	}
	
	// Executes a connection terminated callback on a separate thread.
	private void dispatchConnectionTerminated(final Exception ex)
	{
		Thread connectionTerminatedThread = new Thread()
		{
			@Override
			public void run()
			{
				disconnect();
				
				for (SubscriberListener listener : m_subscriberListeners)
					listener.connectionTerminated(new MessageEvent(DataSubscriber.this, ex));
			}
		};
		
		connectionTerminatedThread.start();
	}
	
	// Decompresses the data in the given buffer.
	private ByteBuffer decompress(ByteBuffer buffer)
	{
		ByteArrayInputStream inputStream = null;
		GZIPInputStream inflater = null;
		ArrayList<Byte> byteList;
		byte[] swap;
		int bytesRead;
		
		try
		{
			inputStream = new ByteArrayInputStream(buffer.array(), buffer.position() + buffer.arrayOffset(), buffer.remaining());
			inflater = new GZIPInputStream(inputStream);
			byteList = new ArrayList<Byte>((int)(buffer.remaining() * 1.5));
			swap = new byte[1024];
			
			do
			{
				// Decompress data and place it into swap array.
				bytesRead = inflater.read(swap, 0, swap.length);

				// Swap the decompressed data into the byte list.
				for (int i = 0; i < bytesRead; i++)
					byteList.add(swap[i]);
			} while (bytesRead > 0);
			
			// Increase swap size to contain the fully decompressed data.
			swap = new byte[byteList.size()];
			
			// Copy the data into the swap array.
			for (int i = 0; i < swap.length; i++)
				swap[i] = byteList.get(i);
			
			return ByteBuffer.wrap(swap);
		}
		catch (IOException ex)
		{
			
			dispatchException(ex);
		}
		finally
		{
			try
			{
				// Attempt to close the input
				// streams to release resources.
				if (inflater != null)
					inflater.close();
				
				if (inputStream != null)
					inputStream.close();
			} catch (IOException ex)
			{
				dispatchException(ex);
			}
		}
		
		return buffer;
	}
	
	// Determines whether the given IP address is
	// valid for the currently connected host.
	private boolean isValidHostAddress(InetAddress address)
	{
		// If it equals the cached host address, simply return
		if (m_hostAddress.equals(address))
			return true;
		
		// Check all valid host addresses
		for (InetAddress hostAddress : m_allHostAddresses)
		{
			if (hostAddress.equals(address))
			{
				// Cache this address so we hopefully
				// don't have to loop every time
				m_hostAddress = hostAddress;
				return true;
			}
		}
		
		// Address is not valid
		return false;
	}
	
	// Maximum possible packet size to be received on the data channel.
	private static final int MaxPacketSize = 32767;
	
	private static final long PollTimeout = 1L;
	private static final TimeUnit PollTimeoutUnit = TimeUnit.SECONDS;
}
