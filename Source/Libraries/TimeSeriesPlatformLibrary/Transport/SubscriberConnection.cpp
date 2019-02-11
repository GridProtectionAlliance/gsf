//******************************************************************************************************
//  SubscriberConnection.cpp - Gbtc
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

#include "SubscriberConnection.h"
#include "DataPublisher.h"
#include "CompactMeasurement.h"
#include "../Common/Convert.h"
#include "../Common/EndianConverter.h"

using namespace std;
using namespace boost::asio;
using namespace boost::asio::ip;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

SubscriberConnection::SubscriberConnection(DataPublisherPtr parent, IOContext& commandChannelService, IOContext& dataChannelService) :
    m_parent(std::move(parent)),
    m_commandChannelService(commandChannelService),
    m_subscriberID(NewGuid()),
    m_operationalModes(OperationalModes::NoFlags),
    m_encoding(OperationalEncoding::UTF8),
    m_usePayloadCompression(false),
    m_useCompactMeasurementFormat(true),
    m_includeTime(true),
    m_useMillisecondResolution(false), // Defaults to microsecond resolution
    m_isNaNFiltered(false),
    m_isSubscribed(false),
    m_startTimeSent(false),
    m_stopped(true),
    m_commandChannelSocket(m_commandChannelService),
    m_readBuffer(Common::MaxPacketSize),
    m_udpPort(0),
    m_dataChannelSocket(dataChannelService),
    m_timeIndex(0),
    m_baseTimeOffsets{0L, 0L}
{
    // Setup ping timer
    m_pingTimer.SetInterval(5000);
    m_pingTimer.SetAutoReset(true);
    m_pingTimer.SetCallback(&SubscriberConnection::PingTimerElapsed);
    m_pingTimer.SetUserData(this);
}

SubscriberConnection::~SubscriberConnection() = default;

const DataPublisherPtr& SubscriberConnection::GetParent() const
{
    return m_parent;
}

TcpSocket& SubscriberConnection::CommandChannelSocket()
{
    return m_commandChannelSocket;
}

const GSF::Guid& SubscriberConnection::GetSubscriberID() const
{
    return m_subscriberID;
}

void SubscriberConnection::SetSubscriberID(const GSF::Guid& id)
{
    m_subscriberID = id;
}

const std::string& SubscriberConnection::GetConnectionID() const
{
    return m_connectionID;
}

const GSF::IPAddress& SubscriberConnection::GetIPAddress() const
{
    return m_ipAddress;
}

const std::string& SubscriberConnection::GetHostName() const
{
    return m_hostName;
}

uint32_t SubscriberConnection::GetOperationalModes() const
{
    return m_operationalModes;
}

void SubscriberConnection::SetOperationalModes(uint32_t value)
{
    m_operationalModes = value;
    m_encoding = m_operationalModes & OperationalModes::EncodingMask;
}

uint32_t SubscriberConnection::GetEncoding() const
{
    return m_encoding;
}

bool SubscriberConnection::GetUsePayloadCompression() const
{
    return m_usePayloadCompression;
}

void SubscriberConnection::SetUsePayloadCompression(bool value)
{
    m_usePayloadCompression = value;
}

bool SubscriberConnection::GetUseCompactMeasurementFormat() const
{
    return m_useCompactMeasurementFormat;
}

void SubscriberConnection::SetUseCompactMeasurementFormat(bool value)
{
    m_useCompactMeasurementFormat = value;
}

bool SubscriberConnection::GetIncludeTime() const
{
    return m_includeTime;
}

void SubscriberConnection::SetIncludeTime(bool value)
{
    m_includeTime = value;
}

bool SubscriberConnection::GetUseMillisecondResolution() const
{
    return m_useMillisecondResolution;
}

void SubscriberConnection::SetUseMillisecondResolution(bool value)
{
    m_useMillisecondResolution = value;
}

bool SubscriberConnection::GetIsNaNFiltered() const
{
    return m_isNaNFiltered;
}

void SubscriberConnection::SetIsNaNFiltered(bool value)
{
    m_isNaNFiltered = value;
}

bool SubscriberConnection::GetIsSubscribed() const
{
    return m_isSubscribed;
}

void SubscriberConnection::SetIsSubscribed(bool value)
{
    m_isSubscribed = value;
}

const string& SubscriberConnection::GetSubscriptionInfo() const
{
    return m_subscriptionInfo;
}

void SubscriberConnection::SetSubscriptionInfo(const string& value)
{
    if (value.empty())
    {
        m_subscriptionInfo.clear();
        return;
    }

    const StringMap<string> settings = ParseKeyValuePairs(value);
    string source, version, buildDate;

    TryGetValue(settings, "source", source);
    TryGetValue(settings, "version", version);
    TryGetValue(settings, "buildDate", buildDate);

    if (source.empty())
        source = "unknown source";

    if (version.empty())
        version = "?.?.?.?";

    if (buildDate.empty())
        buildDate = "undefined date";

    m_subscriptionInfo = source + " version " + version + " built on " + buildDate;
}

const SignalIndexCachePtr& SubscriberConnection::GetSignalIndexCache() const
{
    return m_signalIndexCache;
}

void SubscriberConnection::SetSignalIndexCache(SignalIndexCachePtr signalIndexCache)
{
    m_signalIndexCache = std::move(signalIndexCache);
}

bool SubscriberConnection::CipherKeysDefined() const
{
    return !m_keys[0].empty();
}

vector<uint8_t> SubscriberConnection::Keys(int32_t cipherIndex)
{
    if (cipherIndex < 0 || cipherIndex > 1)
        throw out_of_range("Cipher index must be 0 or 1");

    return m_keys[cipherIndex];
}

vector<uint8_t> SubscriberConnection::IVs(int32_t cipherIndex)
{
    if (cipherIndex < 0 || cipherIndex > 1)
        throw out_of_range("Cipher index must be 0 or 1");

    return m_ivs[cipherIndex];
}

void SubscriberConnection::Start()
{
    // Attempt to lookup remote connection identification for logging purposes
    auto remoteEndPoint = m_commandChannelSocket.remote_endpoint();
    m_ipAddress = remoteEndPoint.address();

    if (remoteEndPoint.protocol() == tcp::v6())
        m_connectionID = "[" + m_ipAddress.to_string() + "]:" + ToString(remoteEndPoint.port());
    else
        m_connectionID = m_ipAddress.to_string() + ":" + ToString(remoteEndPoint.port());

    try
    {
        DnsResolver resolver(m_commandChannelService);
        const DnsResolver::query query(m_ipAddress.to_string(), ToString(remoteEndPoint.port()));
        DnsResolver::iterator iterator = resolver.resolve(query);
        const DnsResolver::iterator end;

        while (iterator != end)
        {
            auto endPoint = *iterator++;

            if (!endPoint.host_name().empty())
            {
                m_hostName = endPoint.host_name();
                m_connectionID = m_hostName + " (" + m_connectionID + ")";
                break;
            }
        }
    }
    catch (...)
    {   //-V565
        // DNS lookup failure is not catastrophic
    }

    if (m_hostName.empty())
        m_hostName = m_ipAddress.to_string();

    m_pingTimer.Start();
    m_stopped = false;
    ReadCommandChannel();
}

void SubscriberConnection::Stop()
{
    m_stopped = true;
    m_pingTimer.Stop();
    m_commandChannelSocket.shutdown(socket_base::shutdown_both);
    m_commandChannelSocket.cancel();
    m_parent->RemoveConnection(shared_from_this());
}

void SubscriberConnection::PublishMeasurements(const vector<Measurement>& measurements)
{
    static const uint32_t MaxPacketSize = 32768U;

    if (measurements.empty() || !m_isSubscribed)
        return;

    if (!m_startTimeSent)
        m_startTimeSent = SendDataStartTime(measurements[0].Timestamp);

    // TODO: Consider queuing measurements for processing

    CompactMeasurement serializer(m_signalIndexCache, m_baseTimeOffsets, m_includeTime, m_useCompactMeasurementFormat);
    vector<uint8_t> packet, buffer;
    int32_t count = 0;

    packet.reserve(MaxPacketSize);
    buffer.reserve(16);

    for (size_t i = 0; i < measurements.size(); i++)
    {
        const Measurement& measurement = measurements[i];
        const uint16_t runtimeID = m_signalIndexCache->GetSignalIndex(measurement.SignalID);

        if (runtimeID == UInt16::MaxValue)
            continue;

        const uint32_t length = serializer.SerializeMeasurement(measurement, buffer, runtimeID);

        if (packet.size() + length > MaxPacketSize)
        {
            PublishDataPacket(packet, count);
            packet.clear();
            count = 0;
        }

        WriteBytes(packet, buffer);
        buffer.clear();
        count++;
    }

    if (count > 0)
        PublishDataPacket(packet, count);
}

void SubscriberConnection::PublishMeasurements(const vector<MeasurementPtr>& measurements)
{
    if (measurements.empty() || !m_isSubscribed)
        return;

    if (!m_startTimeSent)
        m_startTimeSent = SendDataStartTime(measurements[0]->Timestamp);
}

void SubscriberConnection::PublishDataPacket(const std::vector<uint8_t>& packet, const int32_t count)
{
    vector<uint8_t> buffer;
    buffer.reserve(packet.size() + 5);

    // Serialize data packet flags into response
    buffer.push_back(DataPacketFlags::Compact);

    // Serialize total number of measurement values to follow
    EndianConverter::WriteBigEndianBytes(buffer, count);

    // Serialize measurements to data buffer
    WriteBytes(buffer, packet);

    // Publish data packet to client
    m_parent->SendClientResponse(shared_from_this(), ServerResponse::DataPacket, ServerCommand::Subscribe, buffer);

    // Track last publication time
    m_lastPublishTime = UtcNow();
}

bool SubscriberConnection::SendDataStartTime(uint64_t timestamp)
{
    vector<uint8_t> buffer;
    EndianConverter::WriteBigEndianBytes(buffer, timestamp);
    const bool result = m_parent->SendClientResponse(shared_from_this(), ServerResponse::DataStartTime, ServerCommand::Subscribe, buffer);

    if (result)
        m_parent->DispatchStatusMessage("Start time sent to " + m_connectionID + ".");

    return result;
}
// All commands received from the client are handled by this thread.
void SubscriberConnection::ReadCommandChannel()
{
    if (!m_stopped)
        async_read(m_commandChannelSocket, buffer(m_readBuffer, Common::PayloadHeaderSize), bind(&SubscriberConnection::ReadPayloadHeader, this, _1, _2));
}

void SubscriberConnection::ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred)
{
    const uint32_t PacketSizeOffset = 4;

    if (m_stopped)
        return;

    // Stop cleanly, i.e., don't report, on these errors
    if (error == error::connection_aborted || error == error::connection_reset || error == error::eof)
    {
        Stop();
        return;
    }

    if (error)
    {
        stringstream messageStream;

        messageStream << "Error reading data from client \"";
        messageStream << m_connectionID;
        messageStream << "\" command channel: ";
        messageStream << SystemError(error).what();

        m_parent->DispatchErrorMessage(messageStream.str());

        Stop();
        return;
    }

    const uint32_t packetSize = EndianConverter::ToLittleEndian<uint32_t>(&m_readBuffer[0], PacketSizeOffset);

    if (packetSize > static_cast<uint32_t>(m_readBuffer.size()))
        m_readBuffer.resize(packetSize);

    // Read packet (payload body)
    // This read method is guaranteed not to return until the
    // requested size has been read or an error has occurred.
    async_read(m_commandChannelSocket, buffer(m_readBuffer, packetSize), bind(&SubscriberConnection::ParseCommand, this, _1, _2));
}

void SubscriberConnection::ParseCommand(const ErrorCode& error, uint32_t bytesTransferred)
{
    if (m_stopped)
        return;

    // Stop cleanly, i.e., don't report, on these errors
    if (error == error::connection_aborted || error == error::connection_reset || error == error::eof)
    {
        Stop();
        return;
    }

    if (error)
    {
        stringstream messageStream;

        messageStream << "Error reading data from client \"";
        messageStream << m_connectionID;
        messageStream << "\" command channel: ";
        messageStream << SystemError(error).what();

        m_parent->DispatchErrorMessage(messageStream.str());

        Stop();
        return;
    }

    try
    {
        const SubscriberConnectionPtr connection = shared_from_this();
        uint8_t* data = &m_readBuffer[0];
        const uint32_t command = data[0];
        data++;

        switch (command)
        {
            case ServerCommand::Subscribe:
                m_parent->HandleSubscribe(connection, data, bytesTransferred);
                break;
            case ServerCommand::Unsubscribe:
                m_parent->HandleUnsubscribe(connection);
                break;
            case ServerCommand::MetadataRefresh:
                m_parent->HandleMetadataRefresh(connection, data, bytesTransferred);
                break;
            case ServerCommand::RotateCipherKeys:
                m_parent->HandleRotateCipherKeys(connection);
                break;
            case ServerCommand::UpdateProcessingInterval:
                m_parent->HandleUpdateProcessingInterval(connection, data, bytesTransferred);
                break;
            case ServerCommand::DefineOperationalModes:
                m_parent->HandleDefineOperationalModes(connection, data, bytesTransferred);
                break;
            case ServerCommand::ConfirmNotification:
                m_parent->HandleConfirmNotification(connection, data, bytesTransferred);
                break;
            case ServerCommand::ConfirmBufferBlock:
                m_parent->HandleConfirmBufferBlock(connection, data, bytesTransferred);
                break;
            case ServerCommand::PublishCommandMeasurements:
                m_parent->HandlePublishCommandMeasurements(connection, data, bytesTransferred);
                break;
            case ServerCommand::UserCommand00:
            case ServerCommand::UserCommand01:
            case ServerCommand::UserCommand02:
            case ServerCommand::UserCommand03:
            case ServerCommand::UserCommand04:
            case ServerCommand::UserCommand05:
            case ServerCommand::UserCommand06:
            case ServerCommand::UserCommand07:
            case ServerCommand::UserCommand08:
            case ServerCommand::UserCommand09:
            case ServerCommand::UserCommand10:
            case ServerCommand::UserCommand11:
            case ServerCommand::UserCommand12:
            case ServerCommand::UserCommand13:
            case ServerCommand::UserCommand14:
            case ServerCommand::UserCommand15:
                m_parent->HandleUserCommand(connection, command, data, bytesTransferred);
                break;
            default:
            {
                stringstream messageStream;

                messageStream << "\"" << m_connectionID << "\"";
                messageStream << " sent an unrecognized server command: ";
                messageStream << ToHex(command);

                const string message = messageStream.str();
                m_parent->SendClientResponse(connection, ServerResponse::Failed, command, message);
                m_parent->DispatchErrorMessage(message);
                break;
            }
        }
    }
    catch (const std::exception& ex)
    {
        m_parent->DispatchErrorMessage("Encountered an exception while processing received client data: " + string(ex.what()));
    }

    ReadCommandChannel();
}

void SubscriberConnection::CommandChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length)
{
    if (!m_stopped)
        async_write(m_commandChannelSocket, buffer(&data[offset], length), bind(&SubscriberConnection::WriteHandler, this, _1, _2));
}

void SubscriberConnection::DataChannelSendAsync(uint8_t* data, uint32_t offset, uint32_t length)
{
    // TODO: Implement UDP send
    CommandChannelSendAsync(data, offset, length);
}

void SubscriberConnection::WriteHandler(const ErrorCode& error, uint32_t bytesTransferred)
{
    if (m_stopped)
        return;

    // Stop cleanly, i.e., don't report, on these errors
    if (error == error::connection_aborted || error == error::connection_reset || error == error::eof)
    {
        Stop();
        return;
    }

    if (error)
    {
        stringstream messageStream;

        messageStream << "Error writing data to client \"";
        messageStream << m_connectionID;
        messageStream << "\" command channel: ";
        messageStream << SystemError(error).what();

        m_parent->DispatchErrorMessage(messageStream.str());

        Stop();
    }
}

void SubscriberConnection::PingTimerElapsed(Timer* timer, void* userData)
{
    SubscriberConnection* connection = static_cast<SubscriberConnection*>(userData);

    if (connection == nullptr)
        return;

    if (!connection->m_stopped)
        connection->m_parent->SendClientResponse(connection->shared_from_this(), ServerResponse::NoOP, ServerCommand::Subscribe);
}