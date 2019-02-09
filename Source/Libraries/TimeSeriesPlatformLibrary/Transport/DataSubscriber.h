//******************************************************************************************************
//  DataSubscriber.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/26/2012 - Stephen C. Wills
//       Generated original version of source code.
//  03/22/2018 - J. Ritchie Carroll
//		 Updated DataSubscriber callback function signatures to always include instance reference.
//
//******************************************************************************************************

#ifndef __DATA_SUBSCRIBER_H
#define __DATA_SUBSCRIBER_H

#include "TransportTypes.h"
#include "SignalIndexCache.h"
#include "TSSCMeasurementParser.h"
#include "../Common/ThreadSafeQueue.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataSubscriber;

    // Info structure used to configure subscriptions.
    struct SubscriptionInfo
    {
        std::string FilterExpression;

        bool RemotelySynchronized;
        bool Throttled;

        bool UdpDataChannel;
        uint16_t DataChannelLocalPort;

        bool IncludeTime;
        float64_t LagTime;
        float64_t LeadTime;
        bool UseLocalClockAsRealTime;
        bool UseMillisecondResolution;

        std::string StartTime;
        std::string StopTime;
        std::string ConstraintParameters;
        int32_t ProcessingInterval;

        std::string ExtraConnectionStringParameters;

        SubscriptionInfo();
    };

    // Helper class to provide retry and auto-reconnect functionality to the subscriber.
    class SubscriberConnector
    {
    private:
        typedef std::function<void(DataSubscriber*, const std::string&)> ErrorMessageCallback;
        typedef std::function<void(DataSubscriber*)> ReconnectCallback;

        ErrorMessageCallback m_errorMessageCallback;
        ReconnectCallback m_reconnectCallback;

        std::string m_hostname;
        uint16_t m_port;

        int32_t m_maxRetries;
        int32_t m_retryInterval;
        bool m_autoReconnect;

        bool m_cancel;

        // Auto-reconnect handler.
        static void AutoReconnect(DataSubscriber* subscriber);

        bool Connect(DataSubscriber& subscriber);

    public:
        // Creates a new instance.
        SubscriberConnector();

        // Registers a callback to provide error messages each time
        // the subscriber fails to connect during a connection sequence.
        void RegisterErrorMessageCallback(const ErrorMessageCallback& errorMessageCallback);

        // Registers a callback to notify after an automatic reconnection attempt has been made.
        // This callback will be called whether the connection was successful or not, so it is
        // recommended to check the connected state of the subscriber using the IsConnected() method.
        void RegisterReconnectCallback(const ReconnectCallback& reconnectCallback);

        // Begin connection sequence
        bool Connect(DataSubscriber& subscriber, const SubscriptionInfo& info);

        // Cancel all current and
        // future connection sequences.
        void Cancel();

        // Set the hostname of the publisher to connect to.
        void SetHostname(const std::string& hostname);

        // Set the port that the publisher is listening on.
        void SetPort(uint16_t port);

        // Set the maximum number of retries during a connection sequence.
        void SetMaxRetries(int32_t maxRetries);

        // Sets the interval of idle time (in milliseconds) between connection attempts.
        void SetRetryInterval(int32_t retryInterval);

        // Sets flag that determines whether the subscriber should
        // automatically attempt to reconnect when the connection is terminated.
        void SetAutoReconnect(bool autoReconnect);

        // Getters for configurable settings.
        std::string GetHostname() const;
        uint16_t GetPort() const;
        int32_t GetMaxRetries() const;
        int32_t GetRetryInterval() const;
        bool GetAutoReconnect() const;
    };

    class DataSubscriber // NOLINT
    {
    private:
        // Function pointer types
        typedef std::function<void(DataSubscriber*, const std::vector<uint8_t>&)> DispatcherFunction;
        typedef std::function<void(DataSubscriber*, const std::string&)> MessageCallback;
        typedef std::function<void(DataSubscriber*, int64_t)> DataStartTimeCallback;
        typedef std::function<void(DataSubscriber*, const std::vector<uint8_t>&)> MetadataCallback;
        typedef std::function<void(DataSubscriber*, const std::vector<MeasurementPtr>&)> NewMeasurementsCallback;
        typedef std::function<void(DataSubscriber*)> ConfigurationChangedCallback;
        typedef std::function<void(DataSubscriber*)> ConnectionTerminatedCallback;

        // Structure used to dispatch
        // callbacks on the callback thread.
        struct CallbackDispatcher
        {
            DataSubscriber* Source;
            SharedPtr<std::vector<uint8_t>> Data;
            DispatcherFunction Function;

            CallbackDispatcher();
        };

        SubscriberConnector m_connector;
        SubscriptionInfo m_subscriptionInfo;
        IPAddress m_hostAddress;
        Guid m_subscriberID;
        bool m_compressPayloadData;
        bool m_compressMetadata;
        bool m_compressSignalIndexCache;
        bool m_disconnecting;
        void* m_userData;

        // Statistics counters
        uint64_t m_totalCommandChannelBytesReceived;
        uint64_t m_totalDataChannelBytesReceived;
        uint64_t m_totalMeasurementsReceived;
        bool m_connected;
        bool m_subscribed;

        // Measurement parsing
        SignalIndexCachePtr m_signalIndexCache;
        int32_t m_timeIndex;
        int64_t m_baseTimeOffsets[2];
        TSSCMeasurementParser m_tsscMeasurementParser;
        bool m_tsscResetRequested;
        uint16_t m_tsscSequenceNumber;

        // Callback thread members
        Thread m_callbackThread;
        ThreadSafeQueue<CallbackDispatcher> m_callbackQueue;

        // Command channel
        Thread m_commandChannelResponseThread;
        boost::asio::io_context m_commandChannelService;
        TcpSocket m_commandChannelSocket;
        std::vector<uint8_t> m_readBuffer;
        std::vector<uint8_t> m_writeBuffer;

        // Data channel
        Thread m_dataChannelResponseThread;
        boost::asio::io_context m_dataChannelService;
        UdpSocket m_dataChannelSocket;

        // Callbacks
        MessageCallback m_statusMessageCallback;
        MessageCallback m_errorMessageCallback;
        DataStartTimeCallback m_dataStartTimeCallback;
        MetadataCallback m_metadataCallback;
        NewMeasurementsCallback m_newMeasurementsCallback;
        MessageCallback m_processingCompleteCallback;
        ConfigurationChangedCallback m_configurationChangedCallback;
        ConnectionTerminatedCallback m_connectionTerminatedCallback;
        ConnectionTerminatedCallback m_autoReconnectCallback;

        // Threads
        void RunCallbackThread();
        void RunCommandChannelResponseThread();
        void RunDataChannelResponseThread();

        // Command channel callbacks
        void ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred);
        void ReadPacket(const ErrorCode& error, uint32_t bytesTransferred);
        void WriteHandler(const ErrorCode& error, uint32_t bytesTransferred);

        // Server response handlers
        void ProcessServerResponse(uint8_t* buffer, uint32_t offset, uint32_t length);
        void HandleSucceeded(uint8_t commandCode, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleFailed(uint8_t commandCode, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleMetadataRefresh(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleDataStartTime(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleProcessingComplete(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUpdateSignalIndexCache(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUpdateBaseTimes(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleConfigurationChanged(uint8_t* data, uint32_t offset, uint32_t length);
        void HandleDataPacket(uint8_t* data, uint32_t offset, uint32_t length);
        void ParseTSSCMeasurements(uint8_t* data, uint32_t offset, uint32_t length, std::vector<MeasurementPtr>& measurements);
        void ParseCompactMeasurements(uint8_t* data, uint32_t offset, uint32_t length, bool includeTime, bool useMillisecondResolution, int64_t frameLevelTimestamp, std::vector<MeasurementPtr>& measurements);

        // Dispatchers
        void Dispatch(const DispatcherFunction& function);
        void Dispatch(const DispatcherFunction& function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);

        static void StatusMessageDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);
        static void DataStartTimeDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);
        static void MetadataDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);
        static void ProcessingCompleteDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);
        static void ConfigurationChangedDispatcher(DataSubscriber* source, const std::vector<uint8_t>& buffer);

        // The connection terminated callback is a special case that
        // must be called on its own separate thread so that it can
        // safely close all sockets and stop all subscriber threads
        // (including the callback thread) before executing the callback.
        void ConnectionTerminatedDispatcher();
        
        void Disconnect(bool autoReconnect);

    public:
        // Creates a new instance of the data subscriber.
        DataSubscriber();

        // Releases all threads and sockets
        // tied up by the subscriber.
        ~DataSubscriber();

        // Callback registration
        //
        // Callback functions are defined with the following signatures:
        //   void ProcessStatusMessage(DataSubscriber*, const string& message)
        //   void ProcessErrorMessage(DataSubscriber*, const string& message)
        //   void ProcessDataStartTime(DataSubscriber*, int64_t startTime)
        //   void ProcessMetadata(DataSubscriber*, const vector<uint8_t>& metadata)
        //   void ProcessNewMeasurements(DataSubscriber*, const vector<MeasurementPtr>& newMeasurements)
        //   void ProcessProcessingComplete(DataSubscriber*, const string& message)
        //   void ProcessConfigurationChanged(DataSubscriber*)
        //   void ProcessConnectionTerminated(DataSubscriber*)
        //
        // Metadata is provided to the user as zlib-compressed XML,
        // and must be decompressed and interpreted before it can be used.
        void RegisterStatusMessageCallback(const MessageCallback& statusMessageCallback);
        void RegisterErrorMessageCallback(const MessageCallback& errorMessageCallback);
        void RegisterDataStartTimeCallback(const DataStartTimeCallback& dataStartTimeCallback);
        void RegisterMetadataCallback(const MetadataCallback& metadataCallback);
        void RegisterNewMeasurementsCallback(const NewMeasurementsCallback& newMeasurementsCallback);
        void RegisterProcessingCompleteCallback(const MessageCallback& processingCompleteCallback);
        void RegisterConfigurationChangedCallback(const ConfigurationChangedCallback& configurationChangedCallback);
        void RegisterConnectionTerminatedCallback(const ConnectionTerminatedCallback& connectionTerminatedCallback);
        void RegisterAutoReconnectCallback(const ConnectionTerminatedCallback& autoReconnectCallback);

        const Guid& GetSubscriberID() const;

        // Gets or sets value that determines whether
        // payload data is compressed using TSSC.
        bool IsPayloadDataCompressed() const;
        void SetPayloadDataCompressed(bool compressed);

        // Gets or sets value that determines whether the
        // metadata transfer is compressed using GZip.
        bool IsMetadataCompressed() const;
        void SetMetadataCompressed(bool compressed);

        // Gets or sets value that determines whether the
        // signal index cache is compressed using GZip.
        bool IsSignalIndexCacheCompressed() const;
        void SetSignalIndexCacheCompressed(bool compressed);

        // Gets or sets user defined data reference
        void* GetUserData() const;
        void SetUserData(void* userData);

        SubscriberConnector& GetSubscriberConnector();

        void SetSubscriptionInfo(const SubscriptionInfo& info);
        const SubscriptionInfo& GetSubscriptionInfo() const;

        // Synchronously connects to publisher.
        void Connect(const std::string& hostname, uint16_t port);

        // Disconnects from the publisher.
        //
        // The method does not return until all connections have been
        // closed and all threads spawned by the subscriber have shut
        // down gracefully (with the exception of the thread that
        // executes the connection terminated callback).
        void Disconnect();

        // Subscribe to measurements to start receiving data.
        void Subscribe();
        void Subscribe(const SubscriptionInfo& info);

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
        //   ServerCommand::ConfirmNotification
        //   ServerCommand::ConfirmBufferBlock
        //   ServerCommand::PublishCommandMeasurements
        void SendServerCommand(uint8_t commandCode);
        void SendServerCommand(uint8_t commandCode, std::string message);
        void SendServerCommand(uint8_t commandCode, const uint8_t* data, uint32_t offset, uint32_t length);

        // Convenience method to send the currently defined and/or supported
        // operational modes to the server. Supported operational modes are
        // UTF-8 encoding, common serialization format, and optional metadata compression.
        void SendOperationalModes();

        // Functions for statistics gathering
        uint64_t GetTotalCommandChannelBytesReceived() const;
        uint64_t GetTotalDataChannelBytesReceived() const;
        uint64_t GetTotalMeasurementsReceived() const;
        bool IsConnected() const;
        bool IsSubscribed() const;
    };
}}}

#endif