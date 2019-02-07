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
#include "../Data/DataSet.h"
#include "SubscriberConnection.h"
#include "TransportTypes.h"
#include "SignalIndexCache.h"
#include "Constants.h"

namespace GSF {
namespace FilterExpressions
{
    struct TableIDFields;
    typedef GSF::SharedPtr<TableIDFields> TableIDFieldsPtr;

    class ExpressionTree;
    typedef GSF::SharedPtr<ExpressionTree> ExpressionTreePtr;
}}

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher : public EnableSharedThisPtr<DataPublisher> // NOLINT
    {
    private:
        // Function pointer types
        typedef std::function<void(DataPublisher*, const std::vector<uint8_t>&)> DispatcherFunction;
        typedef std::function<void(DataPublisher*, const std::string&)> MessageCallback;
        typedef std::function<void(DataPublisher*, const GSF::Guid&, const std::string&)> SubscriberConnectionCallback;

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
        GSF::FilterExpressions::TableIDFieldsPtr m_tableIDFields;
        std::unordered_set<SubscriberConnectionPtr> m_subscriberConnections;
        GSF::Mutex m_subscriberConnectionsLock;
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
        void AcceptConnection(const SubscriberConnectionPtr& connection, const ErrorCode& error);
        void RemoveConnection(const SubscriberConnectionPtr& connection);
        bool ParseSubscriptionRequest(const SubscriberConnectionPtr& connection, const std::string& filterExpression, SignalIndexCachePtr& signalIndexCache);

        // Callbacks
        MessageCallback m_statusMessageCallback;
        MessageCallback m_errorMessageCallback;
        SubscriberConnectionCallback m_clientConnectedCallback;
        SubscriberConnectionCallback m_clientDisconnectedCallback;

        // Server request handlers
        void HandleSubscribe(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleUnsubscribe(const SubscriberConnectionPtr& connection);
        void HandleMetadataRefresh(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleRotateCipherKeys(const SubscriberConnectionPtr& connection);
        void HandleUpdateProcessingInterval(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleDefineOperationalModes(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleConfirmNotification(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleConfirmBufferBlock(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandlePublishCommandMeasurements(const SubscriberConnectionPtr& connection, uint8_t* data, uint32_t length);
        void HandleUserCommand(const SubscriberConnectionPtr& connection, uint8_t command, uint8_t* data, uint32_t length);

        // Dispatchers
        void Dispatch(const DispatcherFunction& function);
        void Dispatch(const DispatcherFunction& function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);
        void DispatchClientConnected(const GSF::Guid& subscriberID, const std::string& connectionID);
        void DispatchClientDisconnected(const GSF::Guid& subscriberID, const std::string& connectionID);

        static void StatusMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientConnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientDisconnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void SerializeSignalIndexCache(const SubscriberConnectionPtr& connection, const SignalIndexCachePtr& signalIndexCache, std::vector<uint8_t>& buffer);

        GSF::Data::DataSetPtr FilterClientMetadata(const SubscriberConnectionPtr& connection, const GSF::StringMap<GSF::FilterExpressions::ExpressionTreePtr>& filterExpressions) const;
        std::vector<uint8_t> SerializeSignalIndexCache(const SubscriberConnectionPtr& connection, const SignalIndexCachePtr& signalIndexCache) const;
        std::vector<uint8_t> SerializeMetadata(const SubscriberConnectionPtr& connection, const GSF::Data::DataSetPtr& metadata) const;
        bool SendClientResponse(const SubscriberConnectionPtr& connection, uint8_t responseCode, uint8_t commandCode, const std::string& message);
        bool SendClientResponse(const SubscriberConnectionPtr& connection, uint8_t responseCode, uint8_t commandCode, const std::vector<uint8_t>& data = {});
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
        void RegisterStatusMessageCallback(const MessageCallback& statusMessageCallback);
        void RegisterErrorMessageCallback(const MessageCallback& errorMessageCallback);
        void RegisterClientConnectedCallback(const SubscriberConnectionCallback& clientConnectedCallback);
        void RegisterClientDisconnectedCallback(const SubscriberConnectionCallback& clientDisconnectedCallback);

        static std::string DecodeClientString(const SubscriberConnectionPtr& connection, const uint8_t* data, uint32_t offset, uint32_t length);
        static std::vector<uint8_t> EncodeClientString(const SubscriberConnectionPtr& connection, const std::string& value);

        friend class SubscriberConnection;
    };

    typedef SharedPtr<DataPublisher> DataPublisherPtr;
}}}

#endif