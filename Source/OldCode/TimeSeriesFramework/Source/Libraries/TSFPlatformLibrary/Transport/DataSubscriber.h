//******************************************************************************************************
//  DataSubscriber.h - Gbtc
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
//  03/26/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __DATA_SUBSCRIBER_H
#define __DATA_SUBSCRIBER_H

#include <vector>

#include <boost/asio.hpp>
#include <boost/thread.hpp>

#include "../Common/Types.h"
#include "../Common/Measurement.h"
#include "../Common/EndianConverter.h"
#include "../Common/ThreadSafeQueue.h"

#include "SignalIndexCache.h"

namespace TimeSeriesFramework {
namespace Transport
{
	// Simple exception type thrown by the data subscriber.
	class SubscriberException : public std::exception
	{
	private:
		std::string m_message;

	public:
		SubscriberException(std::string message)
		{
			m_message = message;
		}

		~SubscriberException() throw()
		{
		}

		const char* what() const throw()
		{
			return &m_message[0];
		}
	};

	// Info structure used to configure subscriptions.
	struct SubscriptionInfo
	{
		std::string FilterExpression;
		void (*NewMeasurementsCallback)(std::vector<Measurement>);
			
		//bool RemotelySynchronized;
		//bool CompactFormat;
		bool Throttled;

		bool UdpDataChannel;
		uint16_t DataChannelLocalPort;
		std::string DataChannelInterface;
		//bool DataChannelCompression;

		bool IncludeTime;
		float64_t LagTime;
		float64_t LeadTime;
		bool UseLocalClockAsRealTime;
		bool UseMillisecondResolution;

		std::string StartTime;
		std::string StopTime;
		std::string ConstraintParameters;
		int32_t ProcessingInterval;

		std::string WaitHandleNames;
		uint32_t WaitHandleTimeout;

		SubscriptionInfo()
			: NewMeasurementsCallback(0),
			  Throttled(false),
			  UdpDataChannel(false),
			  DataChannelLocalPort(9500),
			  IncludeTime(true),
			  LagTime(10.0),
			  LeadTime(5.0),
			  UseLocalClockAsRealTime(false),
			  UseMillisecondResolution(false),
			  ProcessingInterval(-1),
			  WaitHandleTimeout(0)
		{
		}
	};

	class DataSubscriber
	{
	private:
		static const int MaxPacketSize = 32767;

		typedef boost::asio::ip::address IPAddress;
		typedef std::vector<uint8_t> CommandPacket;

		// Function pointer types
		typedef void (*DispatcherFunction)(DataSubscriber*, std::vector<uint8_t>);
		typedef void (*MessageCallback)(std::string);
		typedef void (*DataStartTimeCallback)(int64_t);
		typedef void (*MetadataCallback)(std::vector<uint8_t>);
		typedef void (*NewMeasurementsCallback)(std::vector<Measurement>);
		typedef void (*ConnectionTerminatedCallback)();
		
		// Structure used to dispatch
		// callbacks on the callback thread.
		struct CallbackDispatcher
		{
			DataSubscriber* Source;
			std::vector<uint8_t> Data;
			DispatcherFunction Function;
		};

		SubscriptionInfo m_currentSubscription;
		EndianConverter m_endianConverter;
		IPAddress m_hostAddress;
		bool m_compressMetadata;
		bool m_disconnecting;

		// Statistics counters
		long m_totalCommandChannelBytesReceived;
		long m_totalDataChannelBytesReceived;
		long m_totalMeasurementsReceived;
		bool m_connected;
		bool m_subscribed;

		// Measurement parsing
		SignalIndexCache m_signalIndexCache;
		std::size_t m_timeIndex;
		int64_t m_baseTimeOffsets[2];

		// Command thread members
		boost::thread m_commandThread;
		ThreadSafeQueue<CommandPacket> m_commandQueue;

		// Callback thread members
		boost::thread m_callbackThread;
		ThreadSafeQueue<CallbackDispatcher> m_callbackQueue;

		// Command channel
		boost::thread m_commandChannelResponseThread;
		boost::asio::io_service m_commandChannelService;
		TcpSocket m_commandChannelSocket;

		// Data channel
		boost::thread m_dataChannelResponseThread;
		boost::asio::io_service m_dataChannelService;
		UdpSocket m_dataChannelSocket;

		// Callbacks
		MessageCallback m_statusMessageCallback;
		MessageCallback m_errorMessageCallback;
		DataStartTimeCallback m_dataStartTimeCallback;
		MetadataCallback m_metadataCallback;
		NewMeasurementsCallback m_newMeasurementsCallback;
		MessageCallback m_processingCompleteCallback;
		ConnectionTerminatedCallback m_connectionTerminatedCallback;

		// Threads
		void RunCommandThread();
		void RunCallbackThread();
		void RunCommandChannelResponseThread();
		void RunDataChannelResponseThread();

		// Server response handlers
		void HandleSucceeded(uint8_t commandCode, uint8_t* data, std::size_t offset, std::size_t length);
		void HandleFailed(uint8_t commandCode, uint8_t* data, std::size_t offset, std::size_t length);
		void HandleMetadataRefresh(uint8_t* data, std::size_t offset, std::size_t length);
		void HandleDataPacket(uint8_t* data, std::size_t offset, std::size_t length);
		void HandleDataStartTime(uint8_t* data, std::size_t offset, std::size_t length);
		void HandleProcessingComplete(uint8_t* data, std::size_t offset, std::size_t length);
		void HandleUpdateSignalIndexCache(uint8_t* data, std::size_t offset, std::size_t length);
		void HandleUpdateBaseTimes(uint8_t* data, std::size_t offset, std::size_t length);
		void ProcessServerResponse(uint8_t* buffer, std::size_t offset, std::size_t length);

		// Dispatchers
		void Dispatch(DispatcherFunction function);
		void Dispatch(DispatcherFunction function, uint8_t* data, std::size_t offset, std::size_t length);
		void DispatchStatusMessage(std::string message);
		void DispatchErrorMessage(std::string message);

		static void StatusMessageDispatcher(DataSubscriber* source, std::vector<uint8_t> data);
		static void ErrorMessageDispatcher(DataSubscriber* source, std::vector<uint8_t> data);
		static void DataStartTimeDispatcher(DataSubscriber* source, std::vector<uint8_t> data);
		static void MetadataDispatcher(DataSubscriber* source, std::vector<uint8_t> data);
		static void NewMeasurementsDispatcher(DataSubscriber* source, std::vector<uint8_t> data);
		static void ProcessingCompleteDispatcher(DataSubscriber* source, std::vector<uint8_t> data);

		// The connection terminated callback is a special case that
		// must be called on its own separate thread so that it can
		// safely close all sockets and stop all subscriber threads
		// (including the callback thread) before executing the callback.
		void ConnectionTerminatedDispatcher();

	public:
		// Creates a new instance of the data subscriber.
		DataSubscriber(bool compressMetadata = true)
			: m_compressMetadata(compressMetadata),
			  m_totalCommandChannelBytesReceived(0L),
			  m_totalDataChannelBytesReceived(0L),
			  m_totalMeasurementsReceived(0L),
			  m_connected(false),
			  m_subscribed(false),
			  m_disconnecting(false),
			  m_commandChannelSocket(m_commandChannelService),
			  m_dataChannelSocket(m_dataChannelService),
			  m_statusMessageCallback(0),
			  m_errorMessageCallback(0),
			  m_dataStartTimeCallback(0),
			  m_metadataCallback(0),
			  m_newMeasurementsCallback(0),
			  m_processingCompleteCallback(0),
			  m_connectionTerminatedCallback(0)
		{
			m_baseTimeOffsets[0] = 0;
			m_baseTimeOffsets[1] = 0;
		}

		// Releases all threads and sockets
		// tied up by the subscriber.
		~DataSubscriber();

		// Callback registration
		//
		// Callback functions are defined with the following signatures:
		//   void ProcessStatusMessage(std::string message)
		//   void ProcessErrorMessage(std::string message)
		//   void ProcessDataStartTime(TimeSeriesFramework::int64_t startTime)
		//   void ProcessMetadata(std::vector<TimeSeriesFramework::uint8_t> metadata)
		//   void ProcessNewMeasurements(std::vector<TimeSeriesFramework::Measurement> newMeasurements)
		//   void ProcessProcessingComplete(std::string message)
		//   void ProcessConnectionTerminated()
		//
		// Metadata is provided to the user as zlib-compressed XML,
		// and must be decompressed and interpreted before it can be used.
		void RegisterStatusMessageCallback(MessageCallback statusMessageCallback);
		void RegisterErrorMessageCallback(MessageCallback errorMessageCallback);
		void RegisterDataStartTimeCallback(DataStartTimeCallback dataStartTimeCallback);
		void RegisterMetadataCallback(MetadataCallback metadataCallback);
		void RegisterNewMeasurementsCallback(NewMeasurementsCallback newMeasurementsCallback);
		void RegisterProcessingCompleteCallback(MessageCallback processingCompleteCallback);
		void RegisterConnectionTerminatedCallback(ConnectionTerminatedCallback connectionTerminatedCallback);

		// Gets or sets value that determines
		// whether metadata transfer is compressed.
		bool IsMetadataCompressed() const;
		void SetMetadataCompressed(bool compressed);

		// Synchronously connects to publisher.
		void Connect(std::string hostname, uint16_t port);

		// Disconnects from the publisher.
		//
		// The method does not return until all connections have been
		// closed and all threads spawned by the subscriber have shut
		// down gracefully (with the exception of the thread that
		// executes the connection terminated callback).
		void Disconnect();

		// Subscribe to measurements to start receiving data.
		void Subscribe(SubscriptionInfo info);
		SubscriptionInfo GetCurrentSubscription() const;

		// Cancel the current subscription to stop receiving data.
		void Unsubscribe();

		// Send a command to the server.
		//
		// Command codes can be found in the "Constants.h" header file.
		// They are defined as:
		//   ServerCommand::Authenticate
		//   ServerCommand::MetadataRefresh
		//   ServerCommand::Subscribe
		//   ServerCommand::Unsubscribe
		//   ServerCommand::RotateCipherKeys
		//   ServerCommand::UpdateProcessingInterval
		//   ServerCommand::DefineOperationalModes
		void SendServerCommand(uint8_t commandCode);
		void SendServerCommand(uint8_t commandCode, uint8_t* data, std::size_t offset, std::size_t length);

		// Convenience method to send the currently defined and/or supported
		// operational modes to the server. Supported operational modes are
		// UTF-8 encoding, common serialization format, and optional metadata compression.
		void SendOperationalModes();

		// Functions for statistics gathering
		long GetTotalCommandChannelBytesReceived() const;
		long GetTotalDataChannelBytesReceived() const;
		long GetTotalMeasurementsReceived() const;
		bool IsConnected() const;
		bool IsSubscribed() const;
	};
}}

#endif