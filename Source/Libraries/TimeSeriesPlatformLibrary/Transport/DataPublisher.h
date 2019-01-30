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
        GSF::Guid m_subscriberID;
        std::string m_connectionID;
        uint32_t m_operationalModes;
        uint32_t m_encoding;

        std::unordered_set<GSF::Guid> m_subscribedSignalIDs;
        GSF::Timer m_pingTimer;

        // Command channel
        GSF::TcpSocket m_commandChannelSocket;
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

        //void ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred);
        //void ReadResponse(const ErrorCode& error, uint32_t bytesTransferred);
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

        bool CipherKeysDefined() const;
        std::vector<uint8_t> Keys(int cipherIndex);
        std::vector<uint8_t> IVs(int cipherIndex);

        void Start();

        void CommandChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
        void DataChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
    };

    typedef SharedPtr<ClientConnection> ClientConnectionPtr;

    class DataPublisher : public EnableSharedThisPtr<DataPublisher> // NOLINT
    {
    private:
        // Function pointer types
        typedef void(*DispatcherFunction)(DataPublisher*, const std::vector<uint8_t>&);
        typedef void(*MessageCallback)(DataPublisher*, const std::string&);
        typedef void(*ClientConnectedCallback)(DataPublisher*, const GSF::Guid&, const std::string&, const std::string&);

        // Structure used to dispatch
        // callbacks on the callback thread.
        struct CallbackDispatcher
        {
            DataPublisher* Source;
            SharedPtr<std::vector<uint8_t>> Data;
            DispatcherFunction Function;

            CallbackDispatcher();
        };

        GSF::Data::DataSetPtr m_clientMetadata;
        std::unordered_set<ClientConnectionPtr> m_clientConnections;
        SecurityMode m_securityMode;
        bool m_allowMetadataRefresh;
        bool m_allowNaNValueFilter;
        bool m_forceNaNValueFilter;
        uint32_t m_cipherKeyRotationPeriod;
        bool m_disconnecting;
        void* m_userData;

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

        // Command channel accept handlers
        void StartAccept();
        void AcceptConnection(const ClientConnectionPtr& clientConnection, const ErrorCode& error);

        // Callbacks
        MessageCallback m_statusMessageCallback;
        MessageCallback m_errorMessageCallback;
        ClientConnectedCallback m_clientConnectedCallback;

        // Server request handlers
        void HandleSubscribe(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUnsubscribe(const ClientConnectionPtr& connection);
        void HandleMetadataRefresh(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUpdateProcessingInterval(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleDefineOperationalModes(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleConfirmNotification(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleConfirmBufferBlock(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandlePublishCommandMeasurements(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUserCommand(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length);

        // Dispatchers
        void Dispatch(DispatcherFunction function);
        void Dispatch(DispatcherFunction function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);
        void DispatchClientConnected(const GSF::Guid& clientID, const std::string& connectionInfo, const std::string& subscriberInfo);

        static void StatusMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientConnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);        
        static void SerializeSignalIndexCache(const GSF::Guid& clientID, const SignalIndexCache& signalIndexCache, std::vector<uint8_t>& buffer);

        std::string DecodeClientString(const ClientConnectionPtr& connection, const uint8_t* data, uint32_t offset, uint32_t length) const;
        std::vector<uint8_t> EncodeClientString(const ClientConnectionPtr& connection, const std::string& value) const;
        GSF::Data::DataSetPtr FilterClientMetadata(const ClientConnectionPtr& connection, const std::map<std::string, GSF::FilterExpressions::ExpressionTreePtr, StringComparer>& filterExpressions) const;
        std::vector<uint8_t> SerializeMetadata(const ClientConnectionPtr& connection, const GSF::Data::DataSetPtr& metadata);
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
        void DefineMetadata(const std::vector<DeviceMetadataPtr>& deviceMetadata, const std::vector<MeasurementMetadataPtr>& measurementMetadata, const std::vector<PhasorMetadataPtr>& phasorMetadata);

        // Define metadata from existing configuration frames
        void DefineMetadata(const std::vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags = nullptr);
        
        // Define metadata from existing XML document
        void DefineMetadata(const pugi::xml_document& metadata);

        void PublishMeasurements(const std::vector<Measurement>& measurements);
        void PublishMeasurements(const std::vector<MeasurementPtr>& measurements);

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
        //   void ProcessClientConnected(DataPublisher*, const GSF::Guid& clientID, const string& connectionInfo, const string& subscriberInfo);
        void RegisterStatusMessageCallback(MessageCallback statusMessageCallback);
        void RegisterErrorMessageCallback(MessageCallback errorMessageCallback);
        void RegisterClientConnectedCallback(ClientConnectedCallback clientConnectedCallback);

        friend class ClientConnection;
    };

    typedef SharedPtr<DataPublisher> DataPublisherPtr;
}}}

#endif