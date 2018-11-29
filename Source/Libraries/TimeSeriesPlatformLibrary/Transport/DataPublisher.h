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

#include <string>
#include <vector>

#include "TransportTypes.h"
#include "SignalIndexCache.h"
#include "TSSCMeasurementParser.h"
#include "../Common/EndianConverter.h"
#include "../Common/ThreadSafeQueue.h"
#include "../Common/pugixml.hpp"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher;

    typedef SharedPtr<DataPublisher> DataPublisherPtr;

    class ClientConnection
    {
    private:
        const DataPublisherPtr m_parent;
        const Guid m_clientID;

        // Command channel
        TcpSocket m_commandChannelSocket;   // Client

        // Data channel
        int16_t m_udpPort;
        UdpSocket m_dataChannelSocket;

        // Measurement parsing
        SignalIndexCache m_signalIndexCache;
        int32_t m_timeIndex;
        int64_t m_baseTimeOffsets[2];
        TSSCMeasurementParser m_tsscMeasurementParser;
        //bool m_tsscResetRequested;
        //uint16_t m_tsscSequenceNumber;

        //void ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred);
        //void ReadResponse(const ErrorCode& error, uint32_t bytesTransferred);
    public:
        ClientConnection(DataPublisherPtr parent, boost::asio::io_context& commandChannelService, boost::asio::io_context& dataChannelService);
        ~ClientConnection();

        TcpSocket& CommandChannelSocket();

        Guid ClientID() const;

        void Start();
    };

    typedef SharedPtr<ClientConnection> ClientConnectionPtr;

    class DataPublisher : public boost::enable_shared_from_this<DataPublisher>
    {
    private:
        // Function pointer types
        typedef void(*DispatcherFunction)(DataPublisher*, const std::vector<uint8_t>&);
        typedef void(*MessageCallback)(DataPublisher*, const std::string&);
        typedef void(*ClientConnectedCallback)(DataPublisher*, const Guid&, const std::string&, const std::string&);
        //typedef void

        // Structure used to dispatch
        // callbacks on the callback thread.
        struct CallbackDispatcher
        {
            DataPublisher* Source;
            SharedPtr<std::vector<uint8_t>> Data;
            DispatcherFunction Function;

            CallbackDispatcher();
        };

        std::map<Guid, ClientConnectionPtr> m_clientConnections;
        uint32_t m_securityMode;
        bool m_encryptPayload;
        bool m_allowMetadataRefresh;
        bool m_allowNaNValueFilter;
        bool m_forceNaNValueFilter;
        uint32_t m_cipherKeyRotationPeriod;
        bool m_disconnecting;

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
        boost::asio::io_context m_commandChannelService;
        boost::asio::ip::tcp::acceptor m_clientAcceptor;

        // Data channel
        boost::asio::io_context m_dataChannelService;

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
        void HandleSubscribe(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUnsubscribe(ClientConnection& connection);
        void HandleMetadataRefresh(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUpdateProcessingInterval(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleDefineOperationalModes(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleConfirmNotification(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleConfirmBufferBlock(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandlePublishCommandMeasurements(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);
        void HandleUserCommand(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length);

        // Dispatchers
        void Dispatch(DispatcherFunction function);
        void Dispatch(DispatcherFunction function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);
        void DispatchClientConnected(const Guid& clientID, const std::string& connectionInfo, const std::string& subscriberInfo);

        static void StatusMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientConnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        
        static void SerializeSignalIndexCache(const Guid& clientID, const SignalIndexCache& signalIndexCache, std::vector<uint8_t>& buffer);
        //static void SerializeMetadata(const Guid& clientID, const vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags, vector<uint8_t>& buffer);
        //static void SerializeMetadata(const Guid& clientID, const xml_document& metadata, vector<uint8_t>& buffer);

    public:
        // Creates a new instance of the data publisher.
        DataPublisher(const boost::asio::ip::tcp::endpoint& endpoint);
        DataPublisher(uint16_t port, bool ipV6 = false);

        // Releases all threads and sockets
        // tied up by the publisher.
        ~DataPublisher();

        // Define metadata from existing configuration frames
        void DefineMetadata(const std::vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags = nullptr);
        
        // Define metadata from existing XML document
        void DefineMetadata(const pugi::xml_document& metadata);

        void PublishMeasurements(const std::vector<MeasurementPtr>& measurements);

        // Callback registration
        //
        // Callback functions are defined with the following signatures:
        //   void ProcessStatusMessage(DataPublisher*, const string& message)
        //   void ProcessErrorMessage(DataPublisher*, const string& message)
        //   void ProcessClientConnected(DataPublisher*, const Guid& clientID, const string& connectionInfo, const string& subscriberInfo);
        void RegisterStatusMessageCallback(MessageCallback statusMessageCallback);
        void RegisterErrorMessageCallback(MessageCallback errorMessageCallback);
        void RegisterClientConnectedCallback(ClientConnectedCallback clientConnectedCallback);
    };
}}}

#endif