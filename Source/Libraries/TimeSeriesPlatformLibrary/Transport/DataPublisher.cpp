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

// ReSharper disable once CppUnusedIncludeDirective
#include "../FilterExpressions/FilterExpressions.h"
#include <boost/bind.hpp>
#include <boost/locale.hpp>
#include <utility>

#include "DataPublisher.h"
#include "Constants.h"
#include "../Common/Convert.h"
#include "../Common/EndianConverter.h"
#include "../FilterExpressions/FilterExpressionParser.h"

using namespace std;
using namespace pugi;
using namespace boost;
using namespace boost::locale::conv;
using namespace boost::asio;
using namespace boost::asio::ip;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::FilterExpressions;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

struct ClientConnectedInfo
{
    const GSF::Guid ClientID;
    const string ConnectionInfo;
    const string SubscriberInfo;

    ClientConnectedInfo(const GSF::Guid& clientID, string connectionInfo, string subscriberInfo) :
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

GSF::Guid ClientConnection::ClientID() const
{
    return m_clientID;
}

const std::string ClientConnection::ConnectionID()
{
    if (m_connectionID.empty())
    {
        // TODO: Develop good connection ID for client...
    }

    return m_connectionID;
}

uint32_t ClientConnection::GetOperationalModes() const
{
    return m_operationalModes;
}

void ClientConnection::SetOperationalModes(uint32_t value)
{
    m_operationalModes = value;
    m_encoding = m_operationalModes & OperationalModes::EncodingMask;
}

uint32_t ClientConnection::GetEncoding() const
{
    return m_encoding;
}

bool ClientConnection::CipherKeysDefined() const
{
    return !m_keys[0].empty();
}

std::vector<uint8_t> ClientConnection::Keys(int cipherIndex)
{
    if (cipherIndex < 0 || cipherIndex > 1)
        throw out_of_range("Cipher index must be 0 or 1");

    return m_keys[cipherIndex];
}

std::vector<uint8_t> ClientConnection::IVs(int cipherIndex)
{
    if (cipherIndex < 0 || cipherIndex > 1)
        throw out_of_range("Cipher index must be 0 or 1");

    return m_ivs[cipherIndex];
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
        m_clientConnections.insert(pair<GSF::Guid, ClientConnectionPtr>(clientConnection->ClientID(), clientConnection));
        clientConnection->Start();
    }

    StartAccept();
}

void DataPublisher::HandleSubscribe(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleUnsubscribe(const ClientConnectionPtr& connection)
{
}

void DataPublisher::HandleMetadataRefresh(const ClientConnectionPtr& connection, uint8_t* buffer, uint32_t startIndex, uint32_t length)
{
    // Ensure that the subscriber is allowed to request meta-data
    if (!m_allowMetadataRefresh)
        throw PublisherException("Meta-data refresh has been disallowed by the DataPublisher.");

    DispatchStatusMessage("Received meta-data refresh request from " + connection->ConnectionID() + ", preparing response...");

    //const GSF::Guid clientID = connection.ClientID();
    map<string, ExpressionTreePtr, StringComparer> filterExpressions;
    string message, tableName, filterExpression, sortField;
    DateTime startTime = UtcNow();

    try
    {
        // Note that these client provided meta-data filter expressions are applied only to the
        // in-memory DataSet and therefore are not subject to SQL injection attacks
        if (length > 4)
        {
            const uint32_t responseLength = EndianConverter::ToBigEndian<uint32_t>(buffer, startIndex);
            startIndex += 4;

            if (length >= responseLength + 4)
            {
                const string metadataFilters = DecodeClientString(connection, buffer, startIndex, responseLength);
                const vector<ExpressionTreePtr> expressions = FilterExpressionParser::GenerateExpressionTrees(m_clientMetadata, "MeasurementDetail", metadataFilters);

                // Go through each subscriber specified filter expressions and add it to dictionary
                for (const auto& expression : expressions)
                    filterExpressions[expression->Table()->Name()] = expression;
            }
        }
    }
    catch (const std::exception& ex)
    {
        DispatchErrorMessage("Failed to parse subscriber provided meta-data filter expressions: " + string(ex.what()));
    }

    try
    {
        const DataSetPtr metadata = FilterClientMetadata(connection, filterExpressions);
        vector<uint8_t> serializedMetadata = SerializeMetadata(connection, metadata);
        vector<DataTablePtr> tables = metadata->Tables();
        uint64_t rowCount = 0;

        for (size_t i = 0; i < tables.size(); i++)
            rowCount += tables[i]->RowCount();

        if (rowCount > 0)
        {
            //Time elapsedTime = (DateTime.UtcNow.Ticks - startTime).ToSeconds();
            //OnStatusMessage(MessageLevel.Info, $"{rowCount:N0} records spanning {metadata.Tables.Count:N0} tables of meta-data prepared in {elapsedTime.ToString(2)}, sending response to {connection.ConnectionID}...");
        }
        else
        {
            //OnStatusMessage(MessageLevel.Info, $"No meta-data is available, sending an empty response to {connection.ConnectionID}...");
        }

        //SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.MetaDataRefresh, serializedMetadata);
    }
    catch (const std::exception& ex)
    {
        message = "Failed to transfer meta-data due to exception: " + string(ex.what());
        //SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.MetaDataRefresh, message);
        DispatchErrorMessage(message);
    }
}

void DataPublisher::HandleUpdateProcessingInterval(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleDefineOperationalModes(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleConfirmNotification(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleConfirmBufferBlock(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandlePublishCommandMeasurements(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
{
}

void DataPublisher::HandleUserCommand(const ClientConnectionPtr& connection, uint8_t* data, uint32_t offset, uint32_t length)
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

void DataPublisher::DispatchClientConnected(const GSF::Guid& clientID, const string& connectionInfo, const string& subscriberInfo)
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

void DataPublisher::SerializeSignalIndexCache(const GSF::Guid& clientID, const SignalIndexCache& signalIndexCache, vector<uint8_t>& buffer)
{
}

//void DataPublisher::SerializeMetadata(const GSF:Guid& clientID, const vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags, vector<uint8_t>& buffer)
//{
//}

//void DataPublisher::SerializeMetadata(const GSF:Guid& clientID, const xml_document& metadata, vector<uint8_t>& buffer)
//{
//}

ClientConnectionPtr DataPublisher::GetClient(const GSF::Guid& clientID) const
{
    ClientConnectionPtr clientConnection;
    TryGetValue<GSF::Guid, ClientConnectionPtr>(m_clientConnections, clientID, clientConnection, nullptr);
    return clientConnection;
}

string DataPublisher::DecodeClientString(const GSF::Guid& clientID, const uint8_t* data, uint32_t offset, uint32_t length) const
{
    return DecodeClientString(GetClient(clientID), data, offset, length);
}

std::string DataPublisher::DecodeClientString(const ClientConnectionPtr& connection, const uint8_t* data, uint32_t offset, uint32_t length) const
{
    uint32_t encoding = OperationalEncoding::UTF8;
    bool swapBytes = EndianConverter::IsLittleEndian();

    if (connection != nullptr)
        encoding = connection->GetEncoding();

    switch (encoding)
    {
        case OperationalEncoding::UTF8:
            return string(reinterpret_cast<const char_t*>(data + offset), length / sizeof(char_t));
        case OperationalEncoding::Unicode:
        case OperationalEncoding::ANSI:
            swapBytes = !swapBytes;
        case OperationalEncoding::BigEndianUnicode:
        {
            wstring value{};
            value.reserve(length / sizeof(wchar_t));

            for (size_t i = 0; i < length; i += sizeof(wchar_t))
            {
                if (swapBytes)
                    value.append(1, EndianConverter::ToLittleEndian<wchar_t>(data, offset + i));
                else
                    value.append(1, *reinterpret_cast<const wchar_t*>(data + offset + i));
            }

            return ToUTF8(value);
        }
        default:
            throw PublisherException("Encountered unexpected operational encoding " + ToHex(encoding));
    }
}

vector<uint8_t> DataPublisher::EncodeClientString(const GSF::Guid& clientID, const std::string& value) const
{
    return EncodeClientString(GetClient(clientID), value);
}

std::vector<uint8_t> DataPublisher::EncodeClientString(const ClientConnectionPtr& connection, const std::string& value) const
{
    uint32_t encoding = OperationalEncoding::UTF8;
    bool swapBytes = EndianConverter::IsLittleEndian();

    if (connection != nullptr)
        encoding = connection->GetEncoding();

    vector<uint8_t> result{};

    switch (encoding)
    {
        case OperationalEncoding::UTF8:
            result.reserve(value.size() * sizeof(char_t));
            result.assign(value.begin(), value.end());
            break;
        case OperationalEncoding::Unicode:
        case OperationalEncoding::ANSI:
            swapBytes = !swapBytes;
        case OperationalEncoding::BigEndianUnicode:
        {
            wstring utf16 = ToUTF16(value);            
            const int32_t size = utf16.size() * sizeof(wchar_t);
            const uint8_t* data = reinterpret_cast<const uint8_t*>(&utf16[0]);

            result.reserve(size);

            for (int32_t i = 0; i < size; i += sizeof(wchar_t))
            {
                if (swapBytes)
                {
                    result.push_back(data[i + 1]);
                    result.push_back(data[i]);
                }
                else
                {
                    result.push_back(data[i]);
                    result.push_back(data[i + 1]);
                }
            }

            break;
        }
        default:
            throw PublisherException("Encountered unexpected operational encoding " + ToHex(encoding));
    }

    return result;
}

DataSetPtr DataPublisher::FilterClientMetadata(const ClientConnectionPtr& connection, const map<string, ExpressionTreePtr, StringComparer>& filterExpressions) const
{
    if (filterExpressions.empty())
        return m_clientMetadata;

    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    vector<DataTablePtr> tables = m_clientMetadata->Tables();

    for (size_t i = 0; i < tables.size(); i++)
    {
        const DataTablePtr table = tables[i];
        DataTablePtr filteredTable = dataSet->CreateTable(table->Name());
        ExpressionTreePtr expression;

        for (int32_t j = 0; j < table->ColumnCount(); j++)
            filteredTable->AddColumn(filteredTable->CloneColumn(table->Column(j)));

        if (TryGetValue<ExpressionTreePtr>(filterExpressions, table->Name(), expression, nullptr))
        {
            vector<DataRowPtr> matchedRows = FilterExpressionParser::Select(expression);

            for (size_t j = 0; j < matchedRows.size(); j++)
                filteredTable->AddRow(filteredTable->CloneRow(matchedRows[j]));
        }
        else
        {
            for (int32_t j = 0; j < table->RowCount(); j++)
                filteredTable->AddRow(filteredTable->CloneRow(table->Row(j)));
        }

        dataSet->AddOrUpdateTable(filteredTable);
    }

    return dataSet;
}

vector<uint8_t> DataPublisher::SerializeMetadata(const ClientConnectionPtr& connection, const DataSetPtr& metadata)
{
    vector<uint8_t> serializedMetadata;

    if (connection != nullptr)
    {
        const uint32_t operationalModes = connection->GetOperationalModes();
        const uint32_t compressionModes = operationalModes & OperationalModes::CompressionModeMask;
        const bool useCommonSerializationFormat = (operationalModes & OperationalModes::UseCommonSerializationFormat) > 0;
        const bool compressMetadata = (operationalModes & OperationalModes::CompressMetadata) > 0;

        if (!useCommonSerializationFormat)
            throw PublisherException("DataPublisher only supports common serialization format");

        metadata->WriteXml(serializedMetadata);

        if (compressMetadata && (compressionModes & CompressionModes::GZip) > 0)
        {
            const MemoryStream metadataStream(serializedMetadata);
            StreamBuffer streamBuffer;

            streamBuffer.push(GZipCompressor());
            streamBuffer.push(metadataStream);

            vector<uint8_t> compressed;
            CopyStream(&streamBuffer, compressed);

            return compressed;
        }
    }

    return serializedMetadata;
}

bool DataPublisher::SendClientResponse(const ClientConnectionPtr& connection, uint8_t responseCode, uint8_t commandCode, const std::vector<uint8_t>& data)
{
    bool success = false;

    try
    {
        const bool dataPacketResponse = responseCode == ServerResponse::DataPacket;
        const bool useDataChannel = dataPacketResponse || responseCode == ServerResponse::BufferBlock;
        vector<uint8_t> buffer {};

        buffer.reserve(data.size() + 6);

        // Add response code
        buffer.push_back(responseCode);

        // Add original in response to command code
        buffer.push_back(commandCode);

        if (data.empty())
        {
            // Add zero sized data buffer to response packet
            buffer.push_back(0);
            buffer.push_back(0);
            buffer.push_back(0);
            buffer.push_back(0);
        }
        else
        {
            if (dataPacketResponse && connection->CipherKeysDefined())
            {
                // TODO: Implement UDP AES data packet encryption
                //// Get a local copy of volatile keyIVs and cipher index since these can change at any time
                //byte[][][] keyIVs = connection.KeyIVs;
                //int cipherIndex = connection.CipherIndex;

                //// Reserve space for size of data buffer to go into response packet
                //workingBuffer.Write(ZeroLengthBytes, 0, 4);

                //// Get data packet flags
                //DataPacketFlags flags = (DataPacketFlags)data[0];

                //// Encode current cipher index into data packet flags
                //if (cipherIndex > 0)
                //    flags |= DataPacketFlags.CipherIndex;

                //// Write data packet flags into response packet
                //workingBuffer.WriteByte((byte)flags);

                //// Copy source data payload into a memory stream
                //MemoryStream sourceData = new MemoryStream(data, 1, data.Length - 1);

                //// Encrypt payload portion of data packet and copy into the response packet
                //Common.SymmetricAlgorithm.Encrypt(sourceData, workingBuffer, keyIVs[cipherIndex][0], keyIVs[cipherIndex][1]);

                //// Calculate length of encrypted data payload
                //int payloadLength = (int)workingBuffer.Length - 6;

                //// Move the response packet position back to the packet size reservation
                //workingBuffer.Seek(2, SeekOrigin.Begin);

                //// Add the actual size of payload length to response packet
                //workingBuffer.Write(BigEndian.GetBytes(payloadLength), 0, 4);
            }
            else
            {
                // Add size of data buffer to response packet
                EndianConverter::WriteBigEndianBytes(buffer, static_cast<int32_t>(data.size()));

                // Write data buffer
                buffer.assign(data.begin(), data.end());
            }

            // TODO: Publish packet
            //IServer publishChannel;

            //// Data packets and buffer blocks can be published on a UDP data channel, so check for this...
            //if (useDataChannel)
            //    publishChannel = m_clientPublicationChannels.GetOrAdd(clientID, id => (object)connection != null ? connection.PublishChannel : m_commandChannel);
            //else
            //    publishChannel = m_commandChannel;

            //// Send response packet
            //if ((object)publishChannel != null && publishChannel.CurrentState == ServerState.Running)
            //{
            //    if (publishChannel is UdpServer)
            //        publishChannel.MulticastAsync(buffer, 0, buffer.size());
            //    else
            //        publishChannel.SendToAsync(connection, buffer, 0, buffer.size());

            //    m_totalBytesSent += buffer.size();
            //    success = true;
            //}
        }
    }
    catch (const std::exception& ex)
    {
        DispatchErrorMessage(ex.what());
    }

    return success;
}

void DataPublisher::DefineMetadata(const vector<DeviceMetadataPtr>& deviceMetadata, const vector<MeasurementMetadataPtr>& measurementMetadata, const vector<PhasorMetadataPtr>& phasorMetadata)
{
}

void DataPublisher::DefineMetadata(const vector<ConfigurationFramePtr>& devices, const MeasurementMetadataPtr& qualityFlags)
{
}

void DataPublisher::DefineMetadata(const xml_document& metadata)
{
}

void DataPublisher::PublishMeasurements(const vector<Measurement>& measurements)
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
