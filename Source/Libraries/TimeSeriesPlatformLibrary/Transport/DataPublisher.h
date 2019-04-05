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
#include "RoutingTables.h"
#include "TransportTypes.h"
#include "Constants.h"

namespace GSF {
namespace FilterExpressions
{
    class ExpressionTree;
    typedef GSF::SharedPtr<ExpressionTree> ExpressionTreePtr;
}}

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher : public EnableSharedThisPtr<DataPublisher> // NOLINT
    {
    public:
        // Function pointer types
        typedef std::function<void(DataPublisher*, const std::vector<uint8_t>&)> DispatcherFunction;
        typedef std::function<void(DataPublisher*, const std::string&)> MessageCallback;
        typedef std::function<void(DataPublisher*, const SubscriberConnectionPtr&)> SubscriberConnectionCallback;

    private:
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
        GSF::Data::DataSetPtr m_metadata;
        GSF::Data::DataSetPtr m_filteringMetadata;
        RoutingTables m_routingTables;
        std::unordered_set<SubscriberConnectionPtr> m_subscriberConnections;
        GSF::SharedMutex m_subscriberConnectionsLock;
        SecurityMode m_securityMode;
        int32_t m_maximumAllowedConnections;
        bool m_isMetadataRefreshAllowed;
        bool m_isNaNValueFilterAllowed;
        bool m_isNaNValueFilterForced;
        bool m_supportsTemporalSubscriptions;
        uint32_t m_cipherKeyRotationPeriod;
        void* m_userData;
        bool m_disposing;

        // Callback queue
        ThreadSafeQueue<CallbackDispatcher> m_callbackQueue;

        // Command channel
        GSF::IOContext m_commandChannelService;
        GSF::TcpAcceptor m_clientAcceptor;

        // Command channel handlers
        void StartAccept();
        void AcceptConnection(const SubscriberConnectionPtr& connection, const ErrorCode& error);
        void ConnectionTerminated(const SubscriberConnectionPtr& connection);
        void RemoveConnection(const SubscriberConnectionPtr& connection);

        // Callbacks
        MessageCallback m_statusMessageCallback;
        MessageCallback m_errorMessageCallback;
        SubscriberConnectionCallback m_clientConnectedCallback;
        SubscriberConnectionCallback m_clientDisconnectedCallback;
        SubscriberConnectionCallback m_processingIntervalChangeRequestedCallback;
        SubscriberConnectionCallback m_temporalSubscriptionRequestedCallback;
        SubscriberConnectionCallback m_temporalSubscriptionCanceledCallback;

        // Dispatchers
        void Dispatch(const DispatcherFunction& function);
        void Dispatch(const DispatcherFunction& function, const uint8_t* data, uint32_t offset, uint32_t length);
        void DispatchStatusMessage(const std::string& message);
        void DispatchErrorMessage(const std::string& message);
        void DispatchClientConnected(SubscriberConnection* connection);
        void DispatchClientDisconnected(SubscriberConnection* connection);
        void DispatchProcessingIntervalChangeRequested(SubscriberConnection* connection);
        void DispatchTemporalSubscriptionRequested(SubscriberConnection* connection);
        void DispatchTemporalSubscriptionCanceled(SubscriberConnection* connection);

        static void StatusMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ErrorMessageDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientConnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ClientDisconnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void ProcessingIntervalChangeRequestedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void TemporalSubscriptionRequestedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static void TemporalSubscriptionCanceledDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer);
        static int32_t GetColumnIndex(const GSF::Data::DataTablePtr& table, const std::string& columnName);
    public:
        // Creates a new instance of the data publisher.
        DataPublisher(const GSF::TcpEndPoint& endpoint);
        DataPublisher(uint16_t port, bool ipV6 = false);                    // Bind to default NIC
        DataPublisher(const std::string& networkInterface, uint16_t port);  // Bind to specified NIC IP, format determines IP version

        // Releases all threads and sockets
        // tied up by the publisher.
        ~DataPublisher();

        // Iterator handler delegates
        typedef std::function<void(const SubscriberConnectionPtr&, void* userData)> SubscriberConnectionIteratorHandlerFunction;

        // Defines metadata from existing metadata records
        void DefineMetadata(const std::vector<DeviceMetadataPtr>& deviceMetadata, const std::vector<MeasurementMetadataPtr>& measurementMetadata, const std::vector<PhasorMetadataPtr>& phasorMetadata, int32_t versionNumber = 0);

        // Defines metadata from an existing dataset
        void DefineMetadata(const GSF::Data::DataSetPtr& metadata);

        // Gets primary metadata. This dataset contains all the normalized metadata tables that define
        // the available detail about the data points that can be subscribed to by clients.
        const GSF::Data::DataSetPtr& GetMetadata() const;

        // Gets filtering metadata. This dataset, derived from primary metadata, contains a flattened
        // table used to subscribe to a filtered set of points with an expression, e.g.:
        // FILTER ActiveMeasurements WHERE SignalType LIKE '%PHA'
        const GSF::Data::DataSetPtr& GetFilteringMetadata() const;

        // Filters primary MeasurementDetail metadata returning values as measurement metadata records
        std::vector<MeasurementMetadataPtr> FilterMetadata(const std::string& filterExpression) const;

        void PublishMeasurements(const std::vector<Measurement>& measurements);
        void PublishMeasurements(const std::vector<MeasurementPtr>& measurements);

        // Node ID defines a unique identification for the DataPublisher
        // instance that gets included in published metadata so that clients
        // can easily distinguish the source of the measurements
        const GSF::Guid& GetNodeID() const;
        void SetNodeID(const GSF::Guid& value);

        SecurityMode GetSecurityMode() const;
        void SetSecurityMode(SecurityMode value);

        // Gets or sets value that defines the maximum number of allowed connections, -1 = no limit
        int32_t GetMaximumAllowedConnections() const;
        void SetMaximumAllowedConnections(int32_t value);

        // Gets or sets flag that determines if metadata refresh is allowed by subscribers
        bool GetIsMetadataRefreshAllowed() const;
        void SetIsMetadataRefreshAllowed(bool value);

        // Gets or sets flag that determines if NaN value filter is allowed by subscribers
        bool GetIsNaNValueFilterAllowed() const;
        void SetNaNValueFilterAllowed(bool value);

        // Gets or sets flag that determines if NaN value filter is forced by publisher, regardless of subscriber request
        bool GetIsNaNValueFilterForced() const;
        void SetIsNaNValueFilterForced(bool value);

        bool GetSupportsTemporalSubscriptions() const;
        void SetSupportsTemporalSubscriptions(bool value);

        uint32_t GetCipherKeyRotationPeriod() const;
        void SetCipherKeyRotationPeriod(uint32_t period);

        // Gets or sets user defined data reference
        void* GetUserData() const;
        void SetUserData(void* userData);

        // Statistical functions
        uint64_t GetTotalCommandChannelBytesSent();
        uint64_t GetTotalDataChannelBytesSent();
        uint64_t GetTotalMeasurementsSent();

        // Callback registration
        //
        // Callback functions are defined with the following signatures:
        //   void HandleStatusMessage(DataPublisher* source, const string& message)
        //   void HandleErrorMessage(DataPublisher* source, const string& message)
        //   void HandleClientConnected(DataPublisher* source, const SubscriberConnectionPtr& connection);
        //   void HandleClientDisconnected(DataPublisher* source, const SubscriberConnectionPtr& connection);
        //   void HandleProcessingIntervalChangeRequested(DataPublisher* source, const SubscriberConnectionPtr& connection);
        //   void HandleTemporalSubscriptionRequested(DataPublisher* source, const SubscriberConnectionPtr& connection);
        //   void HandleTemporalSubscriptionCanceled(DataPublisher* source, const SubscriberConnectionPtr& connection);
        void RegisterStatusMessageCallback(const MessageCallback& statusMessageCallback);
        void RegisterErrorMessageCallback(const MessageCallback& errorMessageCallback);
        void RegisterClientConnectedCallback(const SubscriberConnectionCallback& clientConnectedCallback);
        void RegisterClientDisconnectedCallback(const SubscriberConnectionCallback& clientDisconnectedCallback);
        void RegisterProcessingIntervalChangeRequestedCallback(const SubscriberConnectionCallback& processingIntervalChangeRequestedCallback);
        void RegisterTemporalSubscriptionRequestedCallback(const SubscriberConnectionCallback& temporalSubscriptionRequestedCallback);
        void RegisterTemporalSubscriptionCanceledCallback(const SubscriberConnectionCallback& temporalSubscriptionCanceledCallback);

        // SubscriberConnection iteration function - note that full lock will be maintained on source collection
        // for the entire call, so keep work time minimized or clone collection before work
        void IterateSubscriberConnections(const SubscriberConnectionIteratorHandlerFunction& iteratorHandler, void* userData);

        friend class SubscriberConnection;
    };

    typedef SharedPtr<DataPublisher> DataPublisherPtr;
}}}

#endif