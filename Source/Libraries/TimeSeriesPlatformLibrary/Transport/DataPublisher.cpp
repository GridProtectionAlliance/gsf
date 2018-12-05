//******************************************************************************************************
//  DataPublisher.cpp - Gbtc
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

#include <boost/bind.hpp>
#include <utility>

#include "DataPublisher.h"
#include "Constants.h"
#include "../Common/Convert.h"

using namespace std;
using namespace pugi;
using namespace boost;
using namespace boost::asio;
using namespace boost::asio::ip;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

struct ClientConnectedInfo
{
    const Guid ClientID;
    const string ConnectionInfo;
    const string SubscriberInfo;

    ClientConnectedInfo(const Guid& clientID, string connectionInfo, string subscriberInfo) :
        ClientID(clientID),
        ConnectionInfo(std::move(connectionInfo)),
        SubscriberInfo(std::move(subscriberInfo))
    {
    }
};

ClientConnection::ClientConnection(DataPublisherPtr parent, io_context& commandChannelService, io_context& dataChannelService) :  // NOLINT(modernize-pass-by-value)
    m_parent(parent),
    m_clientID(),
    m_commandChannelSocket(commandChannelService),
    m_udpPort(0),
    m_dataChannelSocket(dataChannelService),
    m_timeIndex(0),
    m_baseTimeOffsets{0L, 0L}
{
}

ClientConnection::~ClientConnection()
{
}

Guid ClientConnection::ClientID() const
{
    return m_clientID;
}

TcpSocket& ClientConnection::CommandChannelSocket()
{
    return m_commandChannelSocket;
}

void ClientConnection::Start()
{
    
}

//void ClientConnection::ReadPayloadHeader(const ErrorCode& error, uint32_t bytesTransferred)
//{
//}

//void ClientConnection::ReadResponse(const ErrorCode& error, uint32_t bytesTransferred)
//{
//}

DataPublisher::DataPublisher(const tcp::endpoint& endpoint) :    
    m_securityMode(SecurityMode::None),
    m_allowMetadataRefresh(true),
    m_allowNaNValueFilter(true),
    m_forceNaNValueFilter(false),
    m_cipherKeyRotationPeriod(60000),
    m_disconnecting(false),
    m_totalCommandChannelBytesSent(0L),
    m_totalDataChannelBytesSent(0L),
    m_totalMeasurementsSent(0L),
    m_connected(false),
    m_clientAcceptor(m_commandChannelService, endpoint),
    m_statusMessageCallback(nullptr),
    m_errorMessageCallback(nullptr),
    m_clientConnectedCallback(nullptr)
{
    m_commandChannelService.restart();
    m_callbackThread = Thread(bind(&DataPublisher::RunCallbackThread, this));
    m_commandChannelAcceptThread = Thread(bind(&DataPublisher::RunCommandChannelAcceptThread, this));
}

DataPublisher::DataPublisher(uint16_t port, bool ipV6) :
    DataPublisher(tcp::endpoint(ipV6 ? tcp::v6() : tcp::v4(), port))
{
}

DataPublisher::~DataPublisher()
{
}

void DataPublisher::RunCallbackThread()
{
    while (true)
    {
        m_callbackQueue.WaitForData();

        if (m_disconnecting)
            break;

        const CallbackDispatcher dispatcher = m_callbackQueue.Dequeue();
        dispatcher.Function(dispatcher.Source, *dispatcher.Data);
    }
}

void DataPublisher::RunCommandChannelAcceptThread()
{
    StartAccept();
    m_commandChannelService.run();
}

void DataPublisher::StartAccept()
{
    ClientConnectionPtr clientConnection = NewSharedPtr<ClientConnection, DataPublisherPtr, io_context&, io_context&>(shared_from_this(), m_commandChannelService, m_dataChannelService);
    m_clientAcceptor.async_accept(clientConnection->CommandChannelSocket(), boost::bind(&DataPublisher::AcceptConnection, this, clientConnection, asio::placeholders::error));
}

void DataPublisher::AcceptConnection(const ClientConnectionPtr& clientConnection, const ErrorCode& error)
{
    if (!error)
    {
        m_clientConnections.insert(pair<Guid, ClientConnectionPtr>(clientConnection->ClientID(), clientConnection));
        clientConnection->Start();
    }

    StartAccept();
}

void DataPublisher::HandleSubscribe(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleUnsubscribe(ClientConnection& connection)
{
}

void DataPublisher::HandleMetadataRefresh(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleUpdateProcessingInterval(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleDefineOperationalModes(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleConfirmNotification(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleConfirmBufferBlock(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandlePublishCommandMeasurements(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleUserCommand(ClientConnection& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::Dispatch(DispatcherFunction function)
{
    Dispatch(function, nullptr, 0, 0);
}

void DataPublisher::Dispatch(DispatcherFunction function, const uint8_t* data, uint32_t offset, uint32_t length)
{
    CallbackDispatcher dispatcher;
    SharedPtr<vector<uint8_t>> dataVector = NewSharedPtr<vector<uint8_t>>();

    dataVector->resize(length);

    if (data != nullptr)
    {
        for (uint32_t i = 0; i < length; ++i)
            dataVector->at(i) = data[offset + i];
    }

    dispatcher.Source = this;
    dispatcher.Data = dataVector;
    dispatcher.Function = function;

    m_callbackQueue.Enqueue(dispatcher);
}

void DataPublisher::DispatchStatusMessage(const string& message)
{
    const uint32_t messageSize = message.size() * sizeof(char);
    const char* data = message.c_str();

    Dispatch(&StatusMessageDispatcher, reinterpret_cast<const uint8_t*>(data), 0, messageSize);
}

void DataPublisher::DispatchErrorMessage(const string& message)
{
    const uint32_t messageSize = message.size() * sizeof(char);
    const char* data = message.c_str();

    Dispatch(&ErrorMessageDispatcher, reinterpret_cast<const uint8_t*>(data), 0, messageSize);
}

void DataPublisher::DispatchClientConnected(const Guid& clientID, const string& connectionInfo, const string& subscriberInfo)
{
    const ClientConnectedInfo* data = new ClientConnectedInfo(clientID, connectionInfo, subscriberInfo);
    Dispatch(&ClientConnectedDispatcher, reinterpret_cast<const uint8_t*>(data), 0, sizeof(ClientConnectedInfo*));
}

// Dispatcher function for status messages. Decodes the message and provides it to the user via the status message callback.
void DataPublisher::StatusMessageDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    if (source == nullptr)
        return;

    const MessageCallback statusMessageCallback = source->m_statusMessageCallback;

    if (statusMessageCallback != nullptr)
    {
        stringstream messageStream;

        for (unsigned char i : buffer)
            messageStream << buffer[i];

        statusMessageCallback(source, messageStream.str());
    }

}

// Dispatcher function for error messages. Decodes the message and provides it to the user via the error message callback.
void DataPublisher::ErrorMessageDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    if (source == nullptr)
        return;

    const MessageCallback errorMessageCallback = source->m_errorMessageCallback;

    if (errorMessageCallback != nullptr)
    {
        stringstream messageStream;

        for (unsigned char i : buffer)
            messageStream << i;

        errorMessageCallback(source, messageStream.str());
    }
}

void DataPublisher::ClientConnectedDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    if (source == nullptr)
        return;

    const ClientConnectedCallback clientConnectedCallback = source->m_clientConnectedCallback;

    if (clientConnectedCallback != nullptr)
    {
        const ClientConnectedInfo* data = reinterpret_cast<const ClientConnectedInfo*>(&buffer[0]);
        clientConnectedCallback(source, data->ClientID, data->ConnectionInfo, data->SubscriberInfo);
        delete data;
    }
}

void DataPublisher::SerializeSignalIndexCache(const Guid& clientID, const SignalIndexCache& signalIndexCache, vector<uint8_t>& buffer)
{
}

//void DataPublisher::SerializeMetadata(const Guid& clientID, const vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags, vector<uint8_t>& buffer)
//{
//}
//
//void DataPublisher::SerializeMetadata(const Guid& clientID, const xml_document& metadata, vector<uint8_t>& buffer)
//{
//}

void DataPublisher::DefineMetadata(const vector<DeviceMetadataPtr>& deviceMetadata, const vector<MeasurementMetadataPtr>& measurementMetadata, const vector<PhasorMetadataPtr>& phasorMetadata)
{
}

void DataPublisher::DefineMetadata(const vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags)
{
}

void DataPublisher::DefineMetadata(const xml_document& metadata)
{
}

void DataPublisher::PublishMeasurements(const vector<MeasurementPtr>& measurements)
{
}

SecurityMode DataPublisher::GetSecurityMode() const
{
    return m_securityMode;
}

void DataPublisher::SetSecurityMode(SecurityMode securityMode)
{
    if (IsConnected())
        throw PublisherException("Cannot change security mode once publisher has been connected");

    m_securityMode = securityMode;
}

bool DataPublisher::IsMetadataRefreshAllowed() const
{
    return m_allowMetadataRefresh;
}

void DataPublisher::SetMetadataRefreshAllowed(bool allowed)
{
    m_allowMetadataRefresh = allowed;
}

bool DataPublisher::IsNaNValueFilterAllowed() const
{
    return m_allowNaNValueFilter;
}

void DataPublisher::SetNaNValueFilterAllowed(bool allowed)
{
    m_allowNaNValueFilter = allowed;
}

bool DataPublisher::IsNaNValueFilterForced() const
{
    return m_forceNaNValueFilter;
}

void DataPublisher::SetNaNValueFilterForced(bool forced)
{
    m_forceNaNValueFilter = forced;
}

uint32_t DataPublisher::GetCipherKeyRotationPeriod() const
{
    return m_cipherKeyRotationPeriod;
}

void DataPublisher::SetCipherKeyRotationPeriod(uint32_t period)
{
    m_cipherKeyRotationPeriod = period;
}

void* DataPublisher::GetUserData() const
{
    return m_userData;
}

void DataPublisher::SetUserData(void* userData)
{
    m_userData = userData;
}

uint64_t DataPublisher::GetTotalCommandChannelBytesSent() const
{
    return m_totalCommandChannelBytesSent;
}

uint64_t DataPublisher::GetTotalDataChannelBytesSent() const
{
    return m_totalDataChannelBytesSent;
}

uint64_t DataPublisher::GetTotalMeasurementsSent() const
{
    return m_totalMeasurementsSent;
}

bool DataPublisher::IsConnected() const
{
    return m_connected;
}

void DataPublisher::RegisterStatusMessageCallback(MessageCallback statusMessageCallback)
{
    m_statusMessageCallback = statusMessageCallback;
}

void DataPublisher::RegisterErrorMessageCallback(MessageCallback errorMessageCallback)
{
    m_errorMessageCallback = errorMessageCallback;
}

void DataPublisher::RegisterClientConnectedCallback(ClientConnectedCallback clientConnectedCallback)
{
    m_clientConnectedCallback = clientConnectedCallback;
}
