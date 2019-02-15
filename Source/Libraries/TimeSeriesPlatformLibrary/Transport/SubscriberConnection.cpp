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

// ReSharper disable once CppUnusedIncludeDirective
#include "../FilterExpressions/FilterExpressions.h"
#include "SubscriberConnection.h"
#include "DataPublisher.h"
#include "CompactMeasurement.h"
#include "ActiveMeasurementsSchema.h"
#include "../Common/EndianConverter.h"
#include "../Data/DataSet.h"
#include "../FilterExpressions/FilterExpressionParser.h"

using namespace std;
using namespace boost::asio;
using namespace boost::asio::ip;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::FilterExpressions;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

static const uint32_t MaxPacketSize = 32768U;

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
    m_totalCommandChannelBytesSent(0L),
    m_totalDataChannelBytesSent(0L),
    m_totalMeasurementsSent(0L),
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

const string& SubscriberConnection::GetConnectionID() const
{
    return m_connectionID;
}

const GSF::IPAddress& SubscriberConnection::GetIPAddress() const
{
    return m_ipAddress;
}

const string& SubscriberConnection::GetHostName() const
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

uint64_t SubscriberConnection::GetTotalCommandChannelBytesSent() const
{
    return m_totalCommandChannelBytesSent;
}

uint64_t SubscriberConnection::GetTotalDataChannelBytesSent() const
{
    return m_totalDataChannelBytesSent;
}

uint64_t SubscriberConnection::GetTotalMeasurementsSent() const
{
    return m_totalMeasurementsSent;
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

    // TODO: Consider queuing measurements for processing

    CompactMeasurement serializer(m_signalIndexCache, m_baseTimeOffsets, m_includeTime, m_useCompactMeasurementFormat);
    vector<uint8_t> packet, buffer;
    int32_t count = 0;

    packet.reserve(MaxPacketSize);
    buffer.reserve(16);

    for (size_t i = 0; i < measurements.size(); i++)
    {
        const Measurement& measurement = *measurements[i];
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

void SubscriberConnection::HandleSubscribe(uint8_t* data, uint32_t length)
{
    try
    {
        if (length >= 6)
        {
            const uint8_t flags = data[0];
            int32_t index = 1;

            if ((flags & DataPacketFlags::Synchronized) > 0)
            {
                // Remotely synchronized subscriptions are currently disallowed by data publisher
                const string message = "Client request for remotely synchronized data subscription was denied. Data publisher currently does not allow for synchronized subscriptions.";
                SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, message);
                m_parent->DispatchErrorMessage(message);
            }
            else
            {
                // Next 4 bytes are an integer representing the length of the connection string that follows
                const uint32_t byteLength = EndianConverter::ToBigEndian<uint32_t>(data, index);
                index += 4;

                if (byteLength > 0 && length >= byteLength + 6U)
                {
                    const bool usePayloadCompression = (GetOperationalModes() & OperationalModes::CompressPayloadData) > 0;
                    const bool useCompactMeasurementFormat = (flags & DataPacketFlags::Compact) > 0;
                    const string connectionString = DecodeString(data, index, byteLength);
                    const StringMap<string> settings = ParseKeyValuePairs(connectionString);
                    string setting;

                    if (TryGetValue(settings, "includeTime", setting))
                        SetIncludeTime(ParseBoolean(setting));

                    if (TryGetValue(settings, "useMillisecondResolution", setting))
                        SetUseMillisecondResolution(ParseBoolean(setting));

                    if (TryGetValue(settings, "requestNaNValueFilter", setting))
                        SetIsNaNFiltered(ParseBoolean(setting));

                    SetUsePayloadCompression(usePayloadCompression);
                    SetUseCompactMeasurementFormat(useCompactMeasurementFormat);

                    SignalIndexCachePtr signalIndexCache = nullptr;

                    // Apply subscriber filter expression and build signal index cache
                    if (TryGetValue(settings, "inputMeasurementKeys", setting))
                    {
                        if (!ParseSubscriptionRequest(setting, signalIndexCache))
                            return;
                    }

                    // Pass subscriber assembly information to connection, if defined
                    if (TryGetValue(settings, "assemblyInfo", setting))
                    {
                        SetSubscriptionInfo(setting);
                        m_parent->DispatchStatusMessage("Reported client subscription info: " + GetSubscriptionInfo());
                    }

                    // TODO: Set up UDP data channel if client has requested this
                    if (TryGetValue(settings, "dataChannel", setting))
                    {
                        /*
                        Socket clientSocket = connection.GetCommandChannelSocket();
                        Dictionary<string, string> settings = setting.ParseKeyValuePairs();
                        IPEndPoint localEndPoint = null;
                        string networkInterface = "::0";

                        // Make sure return interface matches incoming client connection
                        if ((object)clientSocket != null)
                        localEndPoint = clientSocket.LocalEndPoint as IPEndPoint;

                        if ((object)localEndPoint != null)
                        {
                        networkInterface = localEndPoint.Address.ToString();

                        // Remove dual-stack prefix
                        if (networkInterface.StartsWith("::ffff:", true, CultureInfo.InvariantCulture))
                        networkInterface = networkInterface.Substring(7);
                        }

                        if (settings.TryGetValue("port", out setting) || settings.TryGetValue("localport", out setting))
                        {
                        if ((compressionModes & CompressionModes.TSSC) > 0)
                        {
                        // TSSC is a stateful compression algorithm which will not reliably support UDP
                        OnStatusMessage(MessageLevel.Warning, "Cannot use TSSC compression mode with UDP - special compression mode disabled");

                        // Disable TSSC compression processing
                        compressionModes &= ~CompressionModes.TSSC;
                        connection.OperationalModes &= ~OperationalModes.CompressionModeMask;
                        connection.OperationalModes |= (OperationalModes)compressionModes;
                        }

                        connection.DataChannel = new UdpServer($"Port=-1; Clients={connection.IPAddress}:{int.Parse(setting)}; interface={networkInterface}");
                        connection.DataChannel.Start();
                        }
                        */
                    }

                    int32_t signalCount = 0;

                    if (signalIndexCache != nullptr)
                    {
                        signalCount = signalIndexCache->Count();

                        // Send updated signal index cache to client with validated rights of the selected input measurement keys                        
                        SendResponse(ServerResponse::UpdateSignalIndexCache, ServerCommand::Subscribe, SerializeSignalIndexCache(signalIndexCache));
                    }

                    SetSignalIndexCache(signalIndexCache);

                    const string message = "Client subscribed as " + string(useCompactMeasurementFormat ? "" : "non-") + "compact unsynchronized with " + ToString(signalCount) + " signals.";

                    SetIsSubscribed(true);
                    SendResponse(ServerResponse::Succeeded, ServerCommand::Subscribe, message);
                    m_parent->DispatchStatusMessage(message);
                }
                else
                {
                    const string message = byteLength > 0 ?
                        "Not enough buffer was provided to parse client data subscription." :
                        "Cannot initialize client data subscription without a connection string.";

                    SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, message);
                    m_parent->DispatchErrorMessage(message);
                }            
            }            
        }
        else
        {
            const string message = "Not enough buffer was provided to parse client data subscription.";
            SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, message);
            m_parent->DispatchErrorMessage(message);
        }
    }
    catch (const std::exception& ex)
    {
        const string message = "Failed to process client data subscription due to exception: " + string(ex.what());
        SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, message);
        m_parent->DispatchErrorMessage(message);
    }
}

void SubscriberConnection::HandleUnsubscribe()
{
    SetIsSubscribed(false);
}

void SubscriberConnection::HandleMetadataRefresh(uint8_t* data, uint32_t length)
{
    // Ensure that the subscriber is allowed to request meta-data
    if (!m_parent->m_allowMetadataRefresh)
        throw PublisherException("Meta-data refresh has been disallowed by the DataPublisher.");

    m_parent->DispatchStatusMessage("Received meta-data refresh request from " + GetConnectionID() + ", preparing response...");

    StringMap<ExpressionTreePtr> filterExpressions;
    const DateTime startTime = UtcNow(); //-V821

    try
    {
        uint32_t index = 0;

        // Note that these client provided meta-data filter expressions are applied only to the
        // in-memory DataSet and therefore are not subject to SQL injection attacks
        if (length > 4)
        {
            const uint32_t responseLength = EndianConverter::ToBigEndian<uint32_t>(data, index);
            index += 4;

            if (length >= responseLength + 4)
            {
                const string metadataFilters = DecodeString(data, index, responseLength);
                const vector<ExpressionTreePtr> expressions = FilterExpressions::FilterExpressionParser::GenerateExpressionTrees(m_parent->m_metadata, "MeasurementDetail", metadataFilters);

                // Go through each subscriber specified filter expressions and add it to dictionary
                for (const auto& expression : expressions)
                    filterExpressions[expression->Table()->Name()] = expression;
            }
        }
    }
    catch (const std::exception& ex)
    {
        m_parent->DispatchErrorMessage("Failed to parse subscriber provided meta-data filter expressions: " + string(ex.what()));
    }

    try
    {
        const DataSetPtr metadata = FilterClientMetadata(filterExpressions);
        const vector<uint8_t> serializedMetadata = SerializeMetadata(metadata);
        vector<DataTablePtr> tables = metadata->Tables();
        uint64_t rowCount = 0;

        for (size_t i = 0; i < tables.size(); i++)
            rowCount += tables[i]->RowCount();

        if (rowCount > 0)
        {
            const TimeSpan elapsedTime = UtcNow() - startTime;
            m_parent->DispatchStatusMessage(ToString(rowCount) + " records spanning " + ToString(tables.size()) + " tables of meta-data prepared in " + ToString(elapsedTime) + ", sending response to " + GetConnectionID() + "...");
        }
        else
        {
            m_parent->DispatchStatusMessage("No meta-data is available" + string(filterExpressions.empty() ? "" : " due to user applied meta-data filters") + ", sending an empty response to " + GetConnectionID() + "...");
        }

        SendResponse(ServerResponse::Succeeded, ServerCommand::MetadataRefresh, serializedMetadata);
    }
    catch (const std::exception& ex)
    {
        const string message = "Failed to transfer meta-data due to exception: " + string(ex.what());
        SendResponse(ServerResponse::Failed, ServerCommand::MetadataRefresh, message);
        m_parent->DispatchErrorMessage(message);
    }
}

void SubscriberConnection::HandleRotateCipherKeys()
{
}

void SubscriberConnection::HandleUpdateProcessingInterval(uint8_t* data, uint32_t length)
{
}

void SubscriberConnection::HandleDefineOperationalModes(uint8_t* data, uint32_t length)
{
    if (length < 4)
        return;

    const uint32_t operationalModes = EndianConverter::ToBigEndian<uint32_t>(data, 0);

    if ((operationalModes & OperationalModes::VersionMask) != 0U)
        m_parent->DispatchStatusMessage("Protocol version not supported. Operational modes may not be set correctly for client \"" + GetConnectionID() + "\".");

    SetOperationalModes(operationalModes);
}

void SubscriberConnection::HandleConfirmNotification(uint8_t* data, uint32_t length)
{
}

void SubscriberConnection::HandleConfirmBufferBlock(uint8_t* data, uint32_t length)
{
}

void SubscriberConnection::HandlePublishCommandMeasurements(uint8_t* data, uint32_t length)
{
}

void SubscriberConnection::HandleUserCommand(uint8_t command, uint8_t* data, uint32_t length)
{
}

bool SubscriberConnection::ParseSubscriptionRequest(const std::string& filterExpression, SignalIndexCachePtr& signalIndexCache)
{
    string exceptionMessage, parsingException;
    FilterExpressionParserPtr parser = NewSharedPtr<FilterExpressionParser>(filterExpression);

    // Define an empty schema if none has been defined
    if (m_parent->m_filteringMetadata == nullptr)
        m_parent->m_filteringMetadata = DataSet::FromXml(ActiveMeasurementsSchema, ActiveMeasurementsSchemaLength);

    // Set filtering dataset, this schema contains a more flattened, denormalized view of available metadata for easier filtering
    parser->SetDataSet(m_parent->m_filteringMetadata);

    // Manually specified signal ID and measurement key fields are expected to be searched against ActiveMeasurements table
    parser->SetTableIDFields("ActiveMeasurements", FilterExpressionParser::DefaultTableIDFields);
    parser->SetPrimaryTableName("ActiveMeasurements");

    // Register call-back for ANLTR parsing exceptions -- these will be appended to any primary exception message
    parser->RegisterParsingExceptionCallback([&parsingException](FilterExpressionParserPtr, const string& exception) { parsingException = exception; });

    try
    {
        parser->Evaluate();
    }
    catch (const FilterExpressionParserException& ex)
    {
        exceptionMessage = "FilterExpressionParser exception: " + string(ex.what());
    }
    catch (const ExpressionTreeException& ex)
    {
        exceptionMessage = "ExpressionTree exception: " + string(ex.what());
    }
    catch (...)
    {
        exceptionMessage = boost::current_exception_diagnostic_information(true);
    }

    if (!exceptionMessage.empty())
    {
        if (!parsingException.empty())
            exceptionMessage += "\n" + parsingException;

        SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, exceptionMessage);
        m_parent->DispatchErrorMessage(exceptionMessage);
        return false;
    }

    uint32_t charSizeEstimate;

    switch (GetEncoding())
    {
        case OperationalEncoding::ANSI:
        case OperationalEncoding::Unicode:
        case OperationalEncoding::BigEndianUnicode:
            charSizeEstimate = 2U;
            break;
        default:
            charSizeEstimate = 1U;
            break;
    }

    const DataTablePtr& activeMeasurements = m_parent->m_filteringMetadata->Table("ActiveMeasurements");
    const vector<DataRowPtr>& rows = parser->FilteredRows();
    const int32_t idColumn = DataPublisher::GetColumnIndex(activeMeasurements, "ID");
    const int32_t signalIDColumn = DataPublisher::GetColumnIndex(activeMeasurements, "SignalID");

    // Create a new signal index cache for filtered measurements
    signalIndexCache = NewSharedPtr<SignalIndexCache>();

    for (size_t i = 0; i < rows.size(); i++)
    {
        const DataRowPtr& row = rows[i];
        const Guid& signalID = row->ValueAsGuid(signalIDColumn).GetValueOrDefault();        
        string source;
        uint32_t id;

        ParseMeasurementKey(row->ValueAsString(idColumn).GetValueOrDefault(), source, id);
        signalIndexCache->AddMeasurementKey(uint16_t(i), signalID, source, id, charSizeEstimate);
    }

    return true;
}

void SubscriberConnection::PublishDataPacket(const vector<uint8_t>& packet, const int32_t count)
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
    SendResponse(ServerResponse::DataPacket, ServerCommand::Subscribe, buffer);

    // Track last publication time
    m_lastPublishTime = UtcNow();
}

bool SubscriberConnection::SendDataStartTime(uint64_t timestamp)
{
    vector<uint8_t> buffer;
    EndianConverter::WriteBigEndianBytes(buffer, timestamp);
    const bool result = SendResponse(ServerResponse::DataStartTime, ServerCommand::Subscribe, buffer);

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
        uint8_t* data = &m_readBuffer[0];
        const uint32_t command = data[0];
        data++;

        switch (command)
        {
            case ServerCommand::Subscribe:
                HandleSubscribe(data, bytesTransferred);
                break;
            case ServerCommand::Unsubscribe:
                HandleUnsubscribe();
                break;
            case ServerCommand::MetadataRefresh:
                HandleMetadataRefresh(data, bytesTransferred);
                break;
            case ServerCommand::RotateCipherKeys:
                HandleRotateCipherKeys();
                break;
            case ServerCommand::UpdateProcessingInterval:
                HandleUpdateProcessingInterval(data, bytesTransferred);
                break;
            case ServerCommand::DefineOperationalModes:
                HandleDefineOperationalModes(data, bytesTransferred);
                break;
            case ServerCommand::ConfirmNotification:
                HandleConfirmNotification(data, bytesTransferred);
                break;
            case ServerCommand::ConfirmBufferBlock:
                HandleConfirmBufferBlock(data, bytesTransferred);
                break;
            case ServerCommand::PublishCommandMeasurements:
                HandlePublishCommandMeasurements(data, bytesTransferred);
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
                HandleUserCommand(command, data, bytesTransferred);
                break;
            default:
            {
                stringstream messageStream;

                messageStream << "\"" << m_connectionID << "\"";
                messageStream << " sent an unrecognized server command: ";
                messageStream << ToHex(command);

                const string message = messageStream.str();
                SendResponse(ServerResponse::Failed, command, message);
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

std::vector<uint8_t> SubscriberConnection::SerializeSignalIndexCache(const SignalIndexCachePtr& signalIndexCache)
{
    vector<uint8_t> serializationBuffer;

    const uint32_t operationalModes = GetOperationalModes();
    const bool useCommonSerializationFormat = (operationalModes & OperationalModes::UseCommonSerializationFormat) > 0;
    const bool compressSignalIndexCache = (operationalModes & OperationalModes::CompressSignalIndexCache) > 0;
    const bool useGZipCompression = (operationalModes & CompressionModes::GZip) > 0;

    if (!useCommonSerializationFormat)
        throw PublisherException("DataPublisher only supports common serialization format");

    serializationBuffer.reserve(uint32_t(signalIndexCache->GetBinaryLength() * 0.02));
    signalIndexCache->Serialize(shared_from_this(), serializationBuffer);

    if (compressSignalIndexCache && useGZipCompression)
    {
        const MemoryStream memoryStream(serializationBuffer);
        StreamBuffer streamBuffer;

        streamBuffer.push(GZipCompressor());
        streamBuffer.push(memoryStream);

        vector<uint8_t> compressedBuffer;
        CopyStream(&streamBuffer, compressedBuffer);
        return compressedBuffer;
    }

    return serializationBuffer;
}

std::vector<uint8_t> SubscriberConnection::SerializeMetadata(const GSF::Data::DataSetPtr& metadata) const
{
    vector<uint8_t> serializationBuffer;

    const uint32_t operationalModes = GetOperationalModes();
    const bool useCommonSerializationFormat = (operationalModes & OperationalModes::UseCommonSerializationFormat) > 0;
    const bool compressMetadata = (operationalModes & OperationalModes::CompressMetadata) > 0;
    const bool useGZipCompression = (operationalModes & CompressionModes::GZip) > 0;

    if (!useCommonSerializationFormat)
        throw PublisherException("DataPublisher only supports common serialization format");

    metadata->WriteXml(serializationBuffer);

    if (compressMetadata && useGZipCompression)
    {
        const MemoryStream memoryStream(serializationBuffer);
        StreamBuffer streamBuffer;

        streamBuffer.push(GZipCompressor());
        streamBuffer.push(memoryStream);

        vector<uint8_t> compressionBuffer;
        CopyStream(&streamBuffer, compressionBuffer);

        return compressionBuffer;
    }

    return serializationBuffer;
}

DataSetPtr SubscriberConnection::FilterClientMetadata(const StringMap<ExpressionTreePtr>& filterExpressions) const
{
    if (filterExpressions.empty())
        return m_parent->m_metadata;

    DataSetPtr dataSet = NewSharedPtr<DataSet>();
    vector<DataTablePtr> tables = m_parent->m_metadata->Tables();

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

bool SubscriberConnection::SendResponse(uint8_t responseCode, uint8_t commandCode, const string& message)
{
    return SendResponse(responseCode, commandCode, EncodeString(message));
}

bool SubscriberConnection::SendResponse(uint8_t responseCode, uint8_t commandCode, const vector<uint8_t>& data)
{
    bool success = false;

    try
    {
        const bool dataPacketResponse = responseCode == ServerResponse::DataPacket;
        const bool useDataChannel = dataPacketResponse || responseCode == ServerResponse::BufferBlock;
        const uint32_t packetSize = data.size() + 6;
        vector<uint8_t> buffer {};

        buffer.reserve(Common::PayloadHeaderSize + packetSize);

        // Add command payload alignment header (deprecated)
        buffer.push_back(0xAA);
        buffer.push_back(0xBB);
        buffer.push_back(0xCC);
        buffer.push_back(0xDD);

        EndianConverter::WriteLittleEndianBytes(buffer, packetSize);

        // Add response code
        buffer.push_back(responseCode);

        // Add original in response to command code
        buffer.push_back(commandCode);

        if (data.empty())
        {
            // Add zero sized data buffer to response packet
            WriteBytes(buffer, uint32_t(0));
        }
        else
        {
            if (dataPacketResponse && CipherKeysDefined())
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
                WriteBytes(buffer, data);
            }

            // TODO: Publish packet on UDP
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

            //}

            CommandChannelSendAsync(buffer.data(), 0, buffer.size());
            m_totalCommandChannelBytesSent += buffer.size();
            success = true;
        }
    }
    catch (const std::exception& ex)
    {
        m_parent->DispatchErrorMessage(ex.what());
    }

    return success;
}

string SubscriberConnection::DecodeString(const uint8_t* data, uint32_t offset, uint32_t length) const
{
    static bool swapBytes = EndianConverter::IsLittleEndian();

    switch (m_encoding)
    {
        case OperationalEncoding::UTF8:
            return string(reinterpret_cast<const char*>(data + offset), length / sizeof(char));
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
            throw PublisherException("Encountered unexpected operational encoding " + ToHex(m_encoding));
    }
}

vector<uint8_t> SubscriberConnection::EncodeString(const string& value) const
{
    static bool swapBytes = EndianConverter::IsLittleEndian();
    vector<uint8_t> result{};

    switch (m_encoding)
    {
        case OperationalEncoding::UTF8:
            result.reserve(value.size() * sizeof(char));
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
            throw PublisherException("Encountered unexpected operational encoding " + ToHex(m_encoding));
    }

    return result;
}

void SubscriberConnection::PingTimerElapsed(Timer* timer, void* userData)
{
    SubscriberConnection* connection = static_cast<SubscriberConnection*>(userData);

    if (connection == nullptr)
        return;

    if (!connection->m_stopped)
        connection->SendResponse(ServerResponse::NoOP, ServerCommand::Subscribe);
}