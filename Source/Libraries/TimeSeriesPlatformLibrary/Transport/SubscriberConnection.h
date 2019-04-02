//******************************************************************************************************
//  SubscriberConnection.h - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  02/07/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __SUBSCRIBER_CONNECTION_H
#define __SUBSCRIBER_CONNECTION_H

#include "../Common/CommonTypes.h"
#include "../Common/Timer.h"
#include "../Data/DataSet.h"
#include "SignalIndexCache.h"
#include "TransportTypes.h"
#include "TSSCEncoder.h"
#include <deque>

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher;
    typedef GSF::SharedPtr<DataPublisher> DataPublisherPtr;

    class SubscriberConnection;
    typedef GSF::SharedPtr<SubscriberConnection> SubscriberConnectionPtr;

    // Represents a subscriber connection to a data publisher
    class SubscriberConnection : public GSF::EnableSharedThisPtr<SubscriberConnection> // NOLINT
    {
    private:
        static constexpr const uint32_t TSSCBufferSize = 32768U;

        const DataPublisherPtr m_parent;
        GSF::IOContext& m_commandChannelService;
        GSF::Strand m_tcpWriteStrand;
        GSF::Timer m_pingTimer;
        GSF::Guid m_subscriberID;
        const GSF::Guid m_instanceID;
        std::string m_connectionID;
        std::string m_subscriptionInfo;
        uint32_t m_operationalModes;
        uint32_t m_encoding;
        GSF::datetime_t m_startTimeConstraint;
        GSF::datetime_t m_stopTimeConstraint;
        int32_t m_processingInterval;
        bool m_temporalSubscriptionCanceled;
        bool m_usePayloadCompression;
        bool m_useCompactMeasurementFormat;
        bool m_includeTime;
        bool m_useMillisecondResolution;
        bool m_isNaNFiltered;
        volatile bool m_connectionAccepted;
        volatile bool m_isSubscribed;
        volatile bool m_startTimeSent;
        volatile bool m_dataChannelActive;
        volatile bool m_stopped;

        // Command channel
        GSF::TcpSocket m_commandChannelSocket;
        std::vector<uint8_t> m_readBuffer;
        std::deque<SharedPtr<std::vector<uint8_t>>> m_tcpWriteBuffers;
        GSF::IPAddress m_ipAddress;
        std::string m_hostName;

        // Data channel
        uint16_t m_udpPort;
        GSF::Mutex m_dataChannelMutex;
        GSF::WaitHandle m_dataChannelWaitHandle;
        GSF::IOContext m_dataChannelService;
        GSF::UdpSocket m_dataChannelSocket;
        GSF::Strand m_udpWriteStrand;
        std::deque<SharedPtr<std::vector<uint8_t>>> m_udpWriteBuffers;
        std::vector<uint8_t> m_keys[2];
        std::vector<uint8_t> m_ivs[2];

        // Statistics counters
        uint64_t m_totalCommandChannelBytesSent;
        uint64_t m_totalDataChannelBytesSent;
        uint64_t m_totalMeasurementsSent;

        // Measurement parsing
        SignalIndexCachePtr m_signalIndexCache;
        int32_t m_timeIndex;
        int64_t m_baseTimeOffsets[2];
        datetime_t m_lastPublishTime;
        TSSCEncoder m_tsscEncoder;
        GSF::Mutex m_tsscEncoderLock;
        uint8_t m_tsscWorkingBuffer[TSSCBufferSize];
        bool m_tsscResetRequested;
        uint16_t m_tsscSequenceNumber;

        // Server request handlers
        void HandleSubscribe(uint8_t* data, uint32_t length);
        void HandleSubscribeFailure(const std::string& message);
        void HandleUnsubscribe();
        void HandleMetadataRefresh(uint8_t* data, uint32_t length);
        void HandleRotateCipherKeys();
        void HandleUpdateProcessingInterval(const uint8_t* data, uint32_t length);
        void HandleDefineOperationalModes(uint8_t* data, uint32_t length);
        void HandleConfirmNotification(uint8_t* data, uint32_t length);
        void HandleConfirmBufferBlock(uint8_t* data, uint32_t length);
        void HandlePublishCommandMeasurements(uint8_t* data, uint32_t length);
        void HandleUserCommand(uint8_t command, uint8_t* data, uint32_t length);

        SignalIndexCachePtr ParseSubscriptionRequest(const std::string& filterExpression, bool& success);
        void PublishCompactMeasurements(const std::vector<MeasurementPtr>& measurements);
        void PublishCompactDataPacket(const std::vector<uint8_t>& packet, int32_t count);
        void PublishTSSCMeasurements(const std::vector<MeasurementPtr>& measurements);
        void PublishTSSCDataPacket(int32_t count);
        bool SendDataStartTime(uint64_t timestamp);
        void ReadCommandChannel();
        void ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred);
        void ParseCommand(const ErrorCode& error, uint32_t bytesTransferred);
        std::vector<uint8_t> SerializeSignalIndexCache(SignalIndexCache& signalIndexCache) const;
        std::vector<uint8_t> SerializeMetadata(const GSF::Data::DataSetPtr& metadata) const;
        GSF::Data::DataSetPtr FilterClientMetadata(const StringMap<GSF::FilterExpressions::ExpressionTreePtr>& filterExpressions) const;
        void CommandChannelSendAsync();
        void CommandChannelWriteHandler(const ErrorCode& error, uint32_t bytesTransferred);
        void DataChannelSendAsync();
        void DataChannelWriteHandler(const ErrorCode& error, uint32_t bytesTransferred);

        static void PingTimerElapsed(Timer*, void* userData);
    public:
        SubscriberConnection(DataPublisherPtr parent, GSF::IOContext& commandChannelService);
        ~SubscriberConnection();

        const DataPublisherPtr& GetParent() const;
        SubscriberConnectionPtr GetReference();

        GSF::TcpSocket& CommandChannelSocket();

        // Gets or sets subscriber UUID used when subscriber is known and pre-established
        const GSF::Guid& GetSubscriberID() const;
        void SetSubscriberID(const GSF::Guid& id);

        // Gets a UUID representing a unique run-time identifier for the current subscriber connection,
        // this can be used to disambiguate when the same subscriber makes multiple connections
        const GSF::Guid& GetInstanceID() const;

        // Gets subscriber connection identification, e.g., remote IP/port, for display and logging references
        const std::string& GetConnectionID() const;

        // Gets subscriber remote IP address
        const GSF::IPAddress& GetIPAddress() const;

        // Gets subscriber communications port
        const std::string& GetHostName() const;

        // Gets or sets established subscriber operational modes
        uint32_t GetOperationalModes() const;
        void SetOperationalModes(uint32_t value);

        // Gets established subscriber string encoding
        uint32_t GetEncoding() const;

        // Gets flags that determines if this subscription is temporal based
        bool GetIsTemporalSubscription() const;

        // Gets or sets the start time temporal processing constraint
        const GSF::datetime_t& GetStartTimeConstraint() const;
        void SetStartTimeConstraint(const GSF::datetime_t& value);

        // Gets or sets the stop time temporal processing constraint
        const GSF::datetime_t& GetStopTimeConstraint() const;
        void SetStopTimeConstraint(const GSF::datetime_t& value);

        // Gets or sets the desired processing interval, in milliseconds
        // With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        // basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        // interval while a value of 0 means to process data as fast as possible.
        int32_t GetProcessingInterval() const;
        void SetProcessingInterval(int32_t value);

        // Gets or sets flag that determines if payload compression should be enabled in data packets
        bool GetUsePayloadCompression() const;
        void SetUsePayloadCompression(bool value);

        // Gets or sets flag that determines if the compact measurement format should be used in data packets
        bool GetUseCompactMeasurementFormat() const;
        void SetUseCompactMeasurementFormat(bool value);

        // Gets or sets flag that determines if time should be included in data packets when the compact measurement format used
        bool GetIncludeTime() const;
        void SetIncludeTime(bool value);

        // Gets or sets flag that determines if time should be restricted to millisecond resolution in data packets when the
        // compact measurement format used; otherwise, full resolution time will be used
        bool GetUseMillisecondResolution() const;
        void SetUseMillisecondResolution(bool value);

        // Gets or sets flag that determines if NaN values should be excluded from data packets
        bool GetIsNaNFiltered() const;
        void SetIsNaNFiltered(bool value);

        // Gets or sets flag that determines if subscriber connection is currently subscribed
        bool GetIsSubscribed() const;
        void SetIsSubscribed(bool value);

        // Gets or sets subscription details about subscriber
        const std::string& GetSubscriptionInfo() const;
        void SetSubscriptionInfo(const std::string& value);

        // Gets or sets signal index cache for subscriber representing run-time mappings for subscribed points
        const SignalIndexCachePtr& GetSignalIndexCache() const;
        void SetSignalIndexCache(SignalIndexCachePtr signalIndexCache);

        // Statistical functions
        uint64_t GetTotalCommandChannelBytesSent() const;
        uint64_t GetTotalDataChannelBytesSent() const;
        uint64_t GetTotalMeasurementsSent() const;

        bool CipherKeysDefined() const;
        std::vector<uint8_t> Keys(int32_t cipherIndex);
        std::vector<uint8_t> IVs(int32_t cipherIndex);

        void Start(bool connectionAccepted = true);
        void Stop(bool shutdownSocket = true);

        void PublishMeasurements(const std::vector<MeasurementPtr>& measurements);
        void CancelTemporalSubscription();

        bool SendResponse(uint8_t responseCode, uint8_t commandCode);
        bool SendResponse(uint8_t responseCode, uint8_t commandCode, const std::string& message);
        bool SendResponse(uint8_t responseCode, uint8_t commandCode, const std::vector<uint8_t>& data);

        std::string DecodeString(const uint8_t* data, uint32_t offset, uint32_t length) const;
        std::vector<uint8_t> EncodeString(const std::string& value) const;
    };

    typedef GSF::SharedPtr<SubscriberConnection> SubscriberConnectionPtr;

}}}

// Setup standard hash code for SubscriberConnectionPtr
namespace std  // NOLINT
{
    template<>
    struct hash<GSF::TimeSeries::Transport::SubscriberConnectionPtr>
    {
        size_t operator () (const GSF::TimeSeries::Transport::SubscriberConnectionPtr& connection) const
        {
            return boost::hash<GSF::TimeSeries::Transport::SubscriberConnectionPtr>()(connection);
        }
    };
}

#endif