//******************************************************************************************************
//  DataPublisher.h - Gbtc
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
//  10/25/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __DATA_PUBLISHER_H
#define __DATA_PUBLISHER_H

#include "../Common/CommonTypes.h"
#include "../Common/ThreadSafeQueue.h"
#include "../Common/Timer.h"
#include "../Common/pugixml.hpp"
#include "../Data/DataSet.h"
#include "TransportTypes.h"
#include "SignalIndexCache.h"
#include "Constants.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class ClientConnection;
    typedef GSF::SharedPtr<ClientConnection> ClientConnectionPtr;
}}}

// Setup standard hash code for ClientConnectionPtr
namespace std  // NOLINT
{
    template<>
    struct hash<GSF::TimeSeries::Transport::ClientConnectionPtr>
    {
        size_t operator () (const GSF::TimeSeries::Transport::ClientConnectionPtr& connection) const
        {
            return boost::hash<GSF::TimeSeries::Transport::ClientConnectionPtr>()(connection);
        }
    };
}

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher;
    typedef SharedPtr<DataPublisher> DataPublisherPtr;

    class ClientConnection : public EnableSharedThisPtr<ClientConnection>  // NOLINT
    {
    private:
        const DataPublisherPtr m_parent;
        GSF::IOContext& m_commandChannelService;
        GSF::Timer m_pingTimer;
        GSF::Guid m_subscriberID;
        std::string m_connectionID;
        std::string m_subscriptionInfo;
        uint32_t m_operationalModes;
        uint32_t m_encoding;
        bool m_usePayloadCompression;
        bool m_useCompactMeasurementFormat;
        bool m_isSubscribed;
        bool m_stopped;

        // Command channel
        GSF::TcpSocket m_commandChannelSocket;
        std::vector<uint8_t> m_readBuffer;
        GSF::IPAddress m_ipAddress;
        std::string m_hostName;

        // Data channel
        int16_t m_udpPort;
        GSF::UdpSocket m_dataChannelSocket;        
        std::vector<uint8_t> m_keys[2];
        std::vector<uint8_t> m_ivs[2];

        // Measurement parsing
        SignalIndexCache m_signalIndexCache;
        int32_t m_timeIndex;
        int64_t m_baseTimeOffsets[2];
        //TSSCMeasurementParser m_tsscMeasurementParser;
        //bool m_tsscResetRequested;
        //uint16_t m_tsscSequenceNumber;

        void ReadCommandChannel();
        void ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred);
        void ParseCommand(const ErrorCode& error, uint32_t bytesTransferred);
        static void PingTimerElapsed(Timer* timer, void* userData);
    public:
        ClientConnection(DataPublisherPtr parent, GSF::IOContext& commandChannelService, GSF::IOContext& dataChannelService);
        ~ClientConnection();

        GSF::TcpSocket& CommandChannelSocket();

        // Gets or sets subscriber identification used when subscriber is known and pre-established
        const GSF::Guid& GetSubscriberID() const;
        void SetSubscriberID(const GSF::Guid& id);

        const std::string& GetConnectionID() const;
        const GSF::IPAddress& GetIPAddress() const;
        const std::string& GetHostName() const;

        uint32_t GetOperationalModes() const;
        void SetOperationalModes(uint32_t value);

        uint32_t GetEncoding() const;

        bool GetUsePayloadCompression() const;
        void SetUsePayloadCompression(bool value);

        bool GetUseCompactMeasurementFormat() const;
        void SetUseCompactMeasurementFormat(bool value);

        bool GetIsSubscribed() const;
        void SetIsSubscribed(bool value);

        const std::string& GetSubscriptionInfo() const;
        void SetSubscriptionInfo(const std::string& value);

        SignalIndexCache& GetSignalIndexCache();

        bool CipherKeysDefined() const;
        std::vector<uint8_t> Keys(int32_t cipherIndex);
        std::vector<uint8_t> IVs(int32_t cipherIndex);

        void Start();
        void Stop();

        void CommandChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
        void DataChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
        void WriteHandler(const ErrorCode& error, uint32_t bytesTransferred);
    };

    typedef SharedPtr<ClientConnection> ClientConnectionPtr;

    class DataPublisher : public EnableSharedThisPtr<DataPublisher> // NOLINT
    {
    private:
        // Function pointer types
        typedef void(*DispatcherFunction)(DataPublisher*, const std::vector<uint8_t>&);
        typedef void(*MessageCallback)(DataPublisher*, const std::string&);
        typedef void(*ClientConnectionCallback)(DataPublisher*, const GSF::Guid&, const std::string&);

        // Structure used to dispatch
        // callbacks on the callback thread.
        struct CallbackDispatcher
        {
            DataPublisher* Source;
            SharedPtr<std::vector<uint8_t>> Data;
            DispatcherFunction Function;

            CallbackDispatcher();
        };

        GSF::Guid m_nodeID;
        GSF::Data::DataSetPtr m_allMetadata;
        GSF::Data::DataSetPtr m_activeMetadata;
        std::unordered_set<ClientConnectionPtr> m_clientConnections;
        GSF::Mutex m_clientConnectionsLock;
        SecurityMode m_securityMode;
        bool m_allowMetadataRefresh;
        bool m_allowNaNValueFilter;
        bool m_forceNaNValueFilter;
        uint32_t m_cipherKeyRotationPeriod;
        void* m_userData;
        bool m_disposing;

        // Statistics counters
        uint64_t m_totalCommandChannelBytesSent;
        uint64_t m_totalDataChannelBytesSent;
        uint64_t m_totalMeasurementsSent;
        bool m_connected;

        // Callback thread members
        Thread m_callbackThread;
        ThreadSafeQueue<CallbackDispatcher> m_callbackQueue;

        // Command channel
        Thread m_commandChannelAcceptThread;
        GSF::IOContext m_commandChannelService;
        GSF::TcpAcceptor m_clientAcceptor;

        // Data channel
        GSF::IOContext m_dataChannelService;

        // Threads
        void RunCallbackThread();
        void RunCommandChannelAcceptThread();

        // Command channel handlers
        void StartAccept();
        void AcceptConnection(const ClientConnectionPtr& connection, const ErrorCode& error);
        void RemoveConnection(const ClientConnectionPtr& connection);

        // Callbacks
        MessageCallback m_statusMessageCallback;
        MessageCallback m_errorMessageCallback;
        ClientConnectionCallback m_clientConnectedCallback;
        ClientConnectionCallback m_clientDisconnectedCallback;

        // Server request handlers
        void HandleSubscribe(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleUnsubscribe(const ClientConnectionPtr& connection);
        void HandleMetadataRefresh(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleRotateCipherKeys(const ClientConnectionPtr& connection);
        void HandleUpdateProcessingInterval(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleDefineOperationalModes(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleConfirmNotification(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleConfirmBufferBlock(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandlePublishCommandMeasurements(const ClientConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleUserCommand(const ClientConnectionPtr& connection, uint8_t command, uint8_t* data, uint32_t length);

        // Dispatchers
        void Dispatch(DispatcherFunction function);
        void Dispatch(DispatcherFunction function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);
        void DispatchClientConnected(const GSF::Guid& subscriberID, const std::string& connectionID);
        void DispatchClientDisconnected(const GSF::Guid& subscriberID, const std::string& connectionID);

        static void StatusMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientConnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientDisconnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void SerializeSignalIndexCache(const GSF::Guid& clientID, const SignalIndexCache& signalIndexCache, std::vector<uint8_t>& buffer);

        std::string DecodeClientString(const ClientConnectionPtr& connection, const uint8_t* data, uint32_t offset, uint32_t length) const;
        std::vector<uint8_t> EncodeClientString(const ClientConnectionPtr& connection, const std::string& value) const;
        GSF::Data::DataSetPtr FilterClientMetadata(const ClientConnectionPtr& connection, const GSF::StringMap<GSF::FilterExpressions::ExpressionTreePtr>& filterExpressions) const;
        std::vector<uint8_t> SerializeMetadata(const ClientConnectionPtr& connection, const GSF::Data::DataSetPtr& metadata) const;
        bool SendClientResponse(const ClientConnectionPtr& connection, uint8_t responseCode, uint8_t commandCode, const std::string& message);
        bool SendClientResponse(const ClientConnectionPtr& connection, uint8_t responseCode, uint8_t commandCode, const std::vector<uint8_t>& data = {});
    public:
        // Creates a new instance of the data publisher.
        DataPublisher(const GSF::TcpEndPoint& endpoint);
        DataPublisher(uint16_t port, bool ipV6 = false);

        // Releases all threads and sockets
        // tied up by the publisher.
        ~DataPublisher();

        // Define metadata from existing metadata tables
        void DefineMetadata(const std::vector<DeviceMetadataPtr>& deviceMetadata, const std::vector<MeasurementMetadataPtr>& measurementMetadata, const std::vector<PhasorMetadataPtr>& phasorMetadata, int32_t versionNumber = 0);

        // Define metadata from an existing dataset
        void DefineMetadata(const GSF::Data::DataSetPtr& metadata);

        void PublishMeasurements(const std::vector<Measurement>& measurements);
        void PublishMeasurements(const std::vector<MeasurementPtr>& measurements);

        // Node ID defines a unique identification for the DataPublisher
        // instance that gets included in published metadata so that clients
        // can easily distinguish the source of the measurements
        const GSF::Guid& GetNodeID() const;
        void SetNodeID(const GSF::Guid& nodeID);

        SecurityMode GetSecurityMode() const;
        void SetSecurityMode(SecurityMode securityMode);

        bool IsMetadataRefreshAllowed() const;
        void SetMetadataRefreshAllowed(bool allowed);

        bool IsNaNValueFilterAllowed() const;
        void SetNaNValueFilterAllowed(bool allowed);

        bool IsNaNValueFilterForced() const;
        void SetNaNValueFilterForced(bool forced);

        uint32_t GetCipherKeyRotationPeriod() const;
        void SetCipherKeyRotationPeriod(uint32_t period);

        // Gets or sets user defined data reference
        void* GetUserData() const;
        void SetUserData(void* userData);

        // Statistical functions
        uint64_t GetTotalCommandChannelBytesSent() const;
        uint64_t GetTotalDataChannelBytesSent() const;
        uint64_t GetTotalMeasurementsSent() const;
        bool IsConnected() const;

        // Callback registration
        //
        // Callback functions are defined with the following signatures:
        //   void ProcessStatusMessage(DataPublisher*, const string& message)
        //   void ProcessErrorMessage(DataPublisher*, const string& message)
        //   void ProcessClientConnected(DataPublisher*, const GSF::Guid& subscriberID, const string& connectionID);
        //   void ProcessClientDisconnected(DataPublisher*, const GSF::Guid& subscriberID, const string& connectionID);
        void RegisterStatusMessageCallback(MessageCallback statusMessageCallback);
        void RegisterErrorMessageCallback(MessageCallback errorMessageCallback);
        void RegisterClientConnectedCallback(ClientConnectionCallback clientConnectedCallback);
        void RegisterClientDisconnectedCallback(ClientConnectionCallback clientDisconnectedCallback);

        friend class ClientConnection;
    };

    typedef SharedPtr<DataPublisher> DataPublisherPtr;
}}}

#endif