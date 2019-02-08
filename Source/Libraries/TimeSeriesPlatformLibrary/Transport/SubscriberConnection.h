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
#include "SignalIndexCache.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class DataPublisher;
    typedef GSF::SharedPtr<DataPublisher> DataPublisherPtr;

    // Represents a subscriber connection to a data publisher
    class SubscriberConnection : public GSF::EnableSharedThisPtr<SubscriberConnection> // NOLINT
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
        SignalIndexCachePtr m_signalIndexCache;
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
        SubscriberConnection(DataPublisherPtr parent, GSF::IOContext& commandChannelService, GSF::IOContext& dataChannelService);
        ~SubscriberConnection();

        const DataPublisherPtr& GetParent() const;

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

        const SignalIndexCachePtr& GetSignalIndexCache() const;
        void SetSignalIndexCache(SignalIndexCachePtr signalIndexCache);

        bool CipherKeysDefined() const;
        std::vector<uint8_t> Keys(int32_t cipherIndex);
        std::vector<uint8_t> IVs(int32_t cipherIndex);

        void Start();
        void Stop();

        void CommandChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
        void DataChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length);
        void WriteHandler(const ErrorCode& error, uint32_t bytesTransferred);
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