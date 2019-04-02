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
#include "DataPublisher.h"
#include "MetadataSchema.h"
#include "ActiveMeasurementsSchema.h"
#include "../FilterExpressions/FilterExpressionParser.h"

using namespace std;
using namespace boost::asio::ip;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::FilterExpressions;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

DataPublisher::DataPublisher(const TcpEndPoint& endpoint) :
    m_nodeID(NewGuid()),
    m_securityMode(SecurityMode::None),
    m_maximumAllowedConnections(-1),
    m_isMetadataRefreshAllowed(true),
    m_isNaNValueFilterAllowed(true),
    m_isNaNValueFilterForced(false),
    m_supportsTemporalSubscriptions(false),
    m_cipherKeyRotationPeriod(60000),
    m_userData(nullptr),
    m_disposing(false),
    m_clientAcceptor(m_commandChannelService, endpoint)
{
    m_callbackThread = Thread(bind(&DataPublisher::RunCallbackThread, this));
    m_commandChannelAcceptThread = Thread(bind(&DataPublisher::RunCommandChannelAcceptThread, this));
}

DataPublisher::DataPublisher(uint16_t port, bool ipV6) :
    DataPublisher(TcpEndPoint(ipV6 ? tcp::v6() : tcp::v4(), port))
{
}

DataPublisher::DataPublisher(const string& networkInterface, uint16_t port) :
    DataPublisher(TcpEndPoint(address::from_string(networkInterface), port))
{
}

DataPublisher::~DataPublisher()
{
    m_disposing = true;
}

DataPublisher::CallbackDispatcher::CallbackDispatcher() :
    Source(nullptr),
    Data(nullptr),
    Function(nullptr)
{
}

void DataPublisher::RunCallbackThread()
{
    while (true)
    {
        m_callbackQueue.WaitForData();

        if (m_disposing)
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
    const SubscriberConnectionPtr connection = NewSharedPtr<SubscriberConnection, DataPublisherPtr, IOContext&>(shared_from_this(), m_commandChannelService);
    m_clientAcceptor.async_accept(connection->CommandChannelSocket(), boost::bind(&DataPublisher::AcceptConnection, this, connection, boost::asio::placeholders::error));
}

void DataPublisher::AcceptConnection(const SubscriberConnectionPtr& connection, const ErrorCode& error)
{
    if (!error)
    {
        m_subscriberConnectionsLock.lock();
        const bool connectionAccepted = m_maximumAllowedConnections == -1 || static_cast<int32_t>(m_subscriberConnections.size()) < m_maximumAllowedConnections;
        m_subscriberConnections.insert(connection);
        m_subscriberConnectionsLock.unlock();

        // TODO: For secured connections, validate certificate and IP information here to assign subscriberID
        connection->Start(connectionAccepted);

        if (connectionAccepted)
        {
            DispatchClientConnected(connection.get());
        }
        else
        {
            DispatchErrorMessage("Subscriber connection refused: connection would exceed " + ToString(m_maximumAllowedConnections) + " maximum allowed connections.");
            
            Thread([connection]
            {
                boost::this_thread::sleep(boost::posix_time::milliseconds(1500));
                connection->SendResponse(ServerResponse::Failed, ServerCommand::Subscribe, "Connection refused: too many active connections.");
                boost::this_thread::sleep(boost::posix_time::milliseconds(500));
                connection->Stop();
            });
        }
    }

    StartAccept();
}

void DataPublisher::ConnectionTerminated(const SubscriberConnectionPtr& connection)
{
    DispatchClientDisconnected(connection.get());
}

void DataPublisher::RemoveConnection(const SubscriberConnectionPtr& connection)
{
    m_routingTables.RemoveRoutes(connection);

    m_subscriberConnectionsLock.lock();
    m_subscriberConnections.erase(connection);
    m_subscriberConnectionsLock.unlock();
}

void DataPublisher::Dispatch(const DispatcherFunction& function)
{
    Dispatch(function, nullptr, 0, 0);
}

void DataPublisher::Dispatch(const DispatcherFunction& function, const uint8_t* data, uint32_t offset, uint32_t length)
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
    const uint32_t messageSize = (message.size() + 1) * sizeof(char);
    Dispatch(&StatusMessageDispatcher, reinterpret_cast<const uint8_t*>(message.c_str()), 0, messageSize);
}

void DataPublisher::DispatchErrorMessage(const string& message)
{
    const uint32_t messageSize = (message.size() + 1) * sizeof(char);
    Dispatch(&ErrorMessageDispatcher, reinterpret_cast<const uint8_t*>(message.c_str()), 0, messageSize);
}

void DataPublisher::DispatchClientConnected(SubscriberConnection* connection)
{
    Dispatch(&ClientConnectedDispatcher, reinterpret_cast<uint8_t*>(&connection), 0, sizeof(SubscriberConnection**));
}

void DataPublisher::DispatchClientDisconnected(SubscriberConnection* connection)
{
    Dispatch(&ClientDisconnectedDispatcher, reinterpret_cast<uint8_t*>(&connection), 0, sizeof(SubscriberConnection**));
}

void DataPublisher::DispatchProcessingIntervalChangeRequested(SubscriberConnection* connection)
{
    Dispatch(&ProcessingIntervalChangeRequestedDispatcher, reinterpret_cast<uint8_t*>(&connection), 0, sizeof(SubscriberConnection**));
}

void DataPublisher::DispatchTemporalSubscriptionRequested(SubscriberConnection* connection)
{
    Dispatch(&TemporalSubscriptionRequestedDispatcher, reinterpret_cast<uint8_t*>(&connection), 0, sizeof(SubscriberConnection**));
}

void DataPublisher::DispatchTemporalSubscriptionCanceled(SubscriberConnection* connection)
{
    Dispatch(&TemporalSubscriptionCanceledDispatcher, reinterpret_cast<uint8_t*>(&connection), 0, sizeof(SubscriberConnection**));
}

// Dispatcher function for status messages. Decodes the message and provides it to the user via the status message callback.
void DataPublisher::StatusMessageDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    if (source == nullptr)
        return;

    const MessageCallback statusMessageCallback = source->m_statusMessageCallback;

    if (statusMessageCallback != nullptr)
        statusMessageCallback(source, reinterpret_cast<const char*>(&buffer[0]));
}

// Dispatcher function for error messages. Decodes the message and provides it to the user via the error message callback.
void DataPublisher::ErrorMessageDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    if (source == nullptr)
        return;

    const MessageCallback errorMessageCallback = source->m_errorMessageCallback;

    if (errorMessageCallback != nullptr)
        errorMessageCallback(source, reinterpret_cast<const char*>(&buffer[0]));
}

void DataPublisher::ClientConnectedDispatcher(DataPublisher* source, const vector<uint8_t>& buffer)
{
    SubscriberConnection* connection = *reinterpret_cast<SubscriberConnection**>(const_cast<uint8_t*>(&buffer[0]));

    if (source != nullptr)
    {
        const SubscriberConnectionCallback clientConnectedCallback = source->m_clientConnectedCallback;

        if (clientConnectedCallback != nullptr)
            clientConnectedCallback(source, connection->GetReference());
    }
}

void DataPublisher::ClientDisconnectedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer)
{
    SubscriberConnection* connection = *reinterpret_cast<SubscriberConnection**>(const_cast<uint8_t*>(&buffer[0]));

    if (source != nullptr)
    {
        const SubscriberConnectionCallback clientDisconnectedCallback = source->m_clientDisconnectedCallback;

        if (clientDisconnectedCallback != nullptr)
            clientDisconnectedCallback(source, connection->GetReference());

        source->RemoveConnection(connection->GetReference());
    }
}

void DataPublisher::ProcessingIntervalChangeRequestedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer)
{
    SubscriberConnection* connection = *reinterpret_cast<SubscriberConnection**>(const_cast<uint8_t*>(&buffer[0]));

    if (source != nullptr)
    {
        const SubscriberConnectionCallback temporalProcessingIntervalChangeRequestedCallback = source->m_processingIntervalChangeRequestedCallback;

        if (temporalProcessingIntervalChangeRequestedCallback != nullptr)
            temporalProcessingIntervalChangeRequestedCallback(source, connection->GetReference());
    }
}

void DataPublisher::TemporalSubscriptionRequestedDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer)
{
    SubscriberConnection* connection = *reinterpret_cast<SubscriberConnection**>(const_cast<uint8_t*>(&buffer[0]));

    if (source != nullptr)
    {
        const SubscriberConnectionCallback temporalSubscriptionRequestedCallback = source->m_temporalSubscriptionRequestedCallback;

        if (temporalSubscriptionRequestedCallback != nullptr)
            temporalSubscriptionRequestedCallback(source, connection->GetReference());
    }
}

void DataPublisher::TemporalSubscriptionCanceledDispatcher(DataPublisher* source, const std::vector<uint8_t>& buffer)
{
    SubscriberConnection* connection = *reinterpret_cast<SubscriberConnection**>(const_cast<uint8_t*>(&buffer[0]));

    if (source != nullptr)
    {
        const SubscriberConnectionCallback temporalSubscriptionCanceledCallback = source->m_temporalSubscriptionCanceledCallback;

        if (temporalSubscriptionCanceledCallback != nullptr)
            temporalSubscriptionCanceledCallback(source, connection->GetReference());
    }
}

int32_t DataPublisher::GetColumnIndex(const GSF::Data::DataTablePtr& table, const std::string& columnName)
{
        const DataColumnPtr& column = table->Column(columnName);
    
        if (column == nullptr)
            throw PublisherException("Column name \"" + columnName + "\" was not found in table \"" + table->Name() + "\"");
    
        return column->Index();
}

void DataPublisher::DefineMetadata(const vector<DeviceMetadataPtr>& deviceMetadata, const vector<MeasurementMetadataPtr>& measurementMetadata, const vector<PhasorMetadataPtr>& phasorMetadata, const int32_t versionNumber)
{
    typedef unordered_map<uint16_t, char> PhasorTypeMap;
    typedef SharedPtr<PhasorTypeMap> PhasorTypeMapPtr;
    const PhasorTypeMapPtr nullPhasorTypeMap = nullptr;

    // Load meta-data schema
    const DataSetPtr metadata = DataSet::FromXml(MetadataSchema, MetadataSchemaLength);
    const DataTablePtr& deviceDetail = metadata->Table("DeviceDetail");
    const DataTablePtr& measurementDetail = metadata->Table("MeasurementDetail");
    const DataTablePtr& phasorDetail = metadata->Table("PhasorDetail");
    const DataTablePtr& schemaVersion = metadata->Table("SchemaVersion");

    StringMap<PhasorTypeMapPtr> phasorTypes;
    PhasorTypeMapPtr phasors;

    if (deviceDetail != nullptr)
    {
        const int32_t nodeID = GetColumnIndex(deviceDetail, "NodeID");
        const int32_t uniqueID = GetColumnIndex(deviceDetail, "UniqueID");
        const int32_t isConcentrator = GetColumnIndex(deviceDetail, "IsConcentrator");
        const int32_t acronym = GetColumnIndex(deviceDetail, "Acronym");
        const int32_t name = GetColumnIndex(deviceDetail, "Name");
        const int32_t accessID = GetColumnIndex(deviceDetail, "AccessID");
        const int32_t parentAcronym = GetColumnIndex(deviceDetail, "ParentAcronym");
        const int32_t protocolName = GetColumnIndex(deviceDetail, "ProtocolName");
        const int32_t framesPerSecond = GetColumnIndex(deviceDetail, "FramesPerSecond");
        const int32_t companyAcronym = GetColumnIndex(deviceDetail, "CompanyAcronym");
        const int32_t vendorAcronym = GetColumnIndex(deviceDetail, "VendorAcronym");
        const int32_t vendorDeviceName = GetColumnIndex(deviceDetail, "VendorDeviceName");
        const int32_t longitude = GetColumnIndex(deviceDetail, "Longitude");
        const int32_t latitude = GetColumnIndex(deviceDetail, "Latitude");
        const int32_t enabled = GetColumnIndex(deviceDetail, "Enabled");
        const int32_t updatedOn = GetColumnIndex(deviceDetail, "UpdatedOn");

        for (size_t i = 0; i < deviceMetadata.size(); i++)
        {
            const DeviceMetadataPtr device = deviceMetadata[i];

            if (device == nullptr)
                continue;

            DataRowPtr row = deviceDetail->CreateRow();

            row->SetGuidValue(nodeID, m_nodeID);
            row->SetGuidValue(uniqueID, device->UniqueID);
            row->SetBooleanValue(isConcentrator, device->ParentAcronym.empty());
            row->SetStringValue(acronym, device->Acronym);
            row->SetStringValue(name, device->Name);
            row->SetInt32Value(accessID, device->AccessID);
            row->SetStringValue(parentAcronym, device->ParentAcronym);
            row->SetStringValue(protocolName, device->ProtocolName);
            row->SetInt32Value(framesPerSecond, device->FramesPerSecond);
            row->SetStringValue(companyAcronym, device->CompanyAcronym);
            row->SetStringValue(vendorAcronym, device->VendorAcronym);
            row->SetStringValue(vendorDeviceName, device->VendorDeviceName);
            row->SetDecimalValue(longitude, decimal_t(device->Longitude));
            row->SetDecimalValue(latitude, decimal_t(device->Latitude));
            row->SetBooleanValue(enabled, true);
            row->SetDateTimeValue(updatedOn, device->UpdatedOn);

            deviceDetail->AddRow(row);
        }
    }

    if (phasorDetail != nullptr)
    {
        const int32_t id = GetColumnIndex(phasorDetail, "ID");
        const int32_t deviceAcronym = GetColumnIndex(phasorDetail, "DeviceAcronym");
        const int32_t label = GetColumnIndex(phasorDetail, "Label");
        const int32_t type = GetColumnIndex(phasorDetail, "Type");
        const int32_t phase = GetColumnIndex(phasorDetail, "Phase");
        const int32_t sourceIndex = GetColumnIndex(phasorDetail, "SourceIndex");
        const int32_t updatedOn = GetColumnIndex(phasorDetail, "UpdatedOn");

        for (size_t i = 0; i < phasorMetadata.size(); i++)
        {
            const PhasorMetadataPtr phasor = phasorMetadata[i];

            if (phasor == nullptr)
                continue;

            DataRowPtr row = phasorDetail->CreateRow();

            row->SetInt32Value(id, static_cast<int32_t>(i));
            row->SetStringValue(deviceAcronym, phasor->DeviceAcronym);
            row->SetStringValue(label, phasor->Label);
            row->SetStringValue(type, phasor->Type);
            row->SetStringValue(phase, phasor->Phase);
            row->SetInt32Value(sourceIndex, phasor->SourceIndex);
            row->SetDateTimeValue(updatedOn, phasor->UpdatedOn);

            phasorDetail->AddRow(row);

            // Track phasor information related to device for measurement signal type derivation later
            if (!TryGetValue(phasorTypes, phasor->DeviceAcronym, phasors, nullPhasorTypeMap))
            {
                phasors = NewSharedPtr<PhasorTypeMap>();
                phasorTypes[phasor->DeviceAcronym] = phasors;
            }

            phasors->insert_or_assign(phasor->SourceIndex, phasor->Type.empty() ? 'I' : phasor->Type[0]);
        }
    }

    if (measurementDetail != nullptr)
    {
        const int32_t deviceAcronym = GetColumnIndex(measurementDetail, "DeviceAcronym");
        const int32_t id = GetColumnIndex(measurementDetail, "ID");
        const int32_t signalID = GetColumnIndex(measurementDetail, "SignalID");
        const int32_t pointTag = GetColumnIndex(measurementDetail, "PointTag");
        const int32_t signalReference = GetColumnIndex(measurementDetail, "SignalReference");
        const int32_t signalAcronym = GetColumnIndex(measurementDetail, "SignalAcronym");
        const int32_t phasorSourceIndex = GetColumnIndex(measurementDetail, "PhasorSourceIndex");
        const int32_t description = GetColumnIndex(measurementDetail, "Description");
        const int32_t internal = GetColumnIndex(measurementDetail, "Internal");
        const int32_t enabled = GetColumnIndex(measurementDetail, "Enabled");
        const int32_t updatedOn = GetColumnIndex(measurementDetail, "UpdatedOn");
        char phasorType = 'I';

        for (size_t i = 0; i < measurementMetadata.size(); i++)
        {
            const MeasurementMetadataPtr measurement = measurementMetadata[i];
            DataRowPtr row = measurementDetail->CreateRow();

            row->SetStringValue(deviceAcronym, measurement->DeviceAcronym);
            row->SetStringValue(id, measurement->ID);
            row->SetGuidValue(signalID, measurement->SignalID);
            row->SetStringValue(pointTag, measurement->PointTag);
            row->SetStringValue(signalReference, ToString(measurement->Reference));

            if (TryGetValue(phasorTypes, measurement->DeviceAcronym, phasors, nullPhasorTypeMap))
                TryGetValue(*phasors, measurement->PhasorSourceIndex, phasorType, 'I');

            row->SetStringValue(signalAcronym, GetSignalTypeAcronym(measurement->Reference.Kind, phasorType));
            row->SetInt32Value(phasorSourceIndex, measurement->PhasorSourceIndex);
            row->SetStringValue(description, measurement->Description);
            row->SetBooleanValue(internal, true);
            row->SetBooleanValue(enabled, true);
            row->SetDateTimeValue(updatedOn, measurement->UpdatedOn);

            measurementDetail->AddRow(row);
        }
    }

    if (schemaVersion != nullptr)
    {
        DataRowPtr row = schemaVersion->CreateRow();
        row->SetInt32Value("VersionNumber", versionNumber);
        schemaVersion->AddRow(row);
    }

    DefineMetadata(metadata);
}

void DataPublisher::DefineMetadata(const DataSetPtr& metadata)
{
    m_metadata = metadata;

    // Create device data map used to build a flatter meta-data view used for easier client filtering
    struct DeviceData
    {
        int32_t DeviceID{};
        int32_t FramesPerSecond{};
        string Company;
        string Protocol;
        string ProtocolType;
        decimal_t Longitude;
        decimal_t Latitude;
    };

    typedef SharedPtr<DeviceData> DeviceDataPtr;
    const DeviceDataPtr nullDeviceData = nullptr;

    const DataTablePtr& deviceDetail = metadata->Table("DeviceDetail");
    StringMap<DeviceDataPtr> deviceData;

    if (deviceDetail != nullptr)
    {
        const int32_t acronym = GetColumnIndex(deviceDetail, "Acronym");
        const int32_t protocolName = GetColumnIndex(deviceDetail, "ProtocolName");
        const int32_t framesPerSecond = GetColumnIndex(deviceDetail, "FramesPerSecond");
        const int32_t companyAcronym = GetColumnIndex(deviceDetail, "CompanyAcronym");
        const int32_t longitude = GetColumnIndex(deviceDetail, "Longitude");
        const int32_t latitude = GetColumnIndex(deviceDetail, "Latitude");

        for (int32_t i = 0; i < deviceDetail->RowCount(); i++)
        {
            const DataRowPtr& row = deviceDetail->Row(i);
            const DeviceDataPtr device = NewSharedPtr<DeviceData>();

            device->DeviceID = i;
            device->FramesPerSecond = row->ValueAsInt32(framesPerSecond).GetValueOrDefault();
            device->Company = row->ValueAsString(companyAcronym).GetValueOrDefault();
            device->Protocol = row->ValueAsString(protocolName).GetValueOrDefault();
            device->ProtocolType = GetProtocolType(device->Protocol);
            device->Longitude = row->ValueAsDecimal(longitude).GetValueOrDefault();
            device->Latitude = row->ValueAsDecimal(latitude).GetValueOrDefault();

            string deviceAcronymRef = row->ValueAsString(acronym).GetValueOrDefault();

            if (!deviceAcronymRef.empty())
                deviceData[deviceAcronymRef] = device;
        }
    }

    // Create phasor data map used to build a flatter meta-data view used for easier client filtering
    struct PhasorData
    {
        int32_t PhasorID{};
        string PhasorType;
        string Phase;
    };

    typedef SharedPtr<PhasorData> PhasorDataPtr;
    const PhasorDataPtr nullPhasorData = nullptr;

    typedef unordered_map<int32_t, PhasorDataPtr> PhasorDataMap;
    typedef SharedPtr<PhasorDataMap> PhasorDataMapPtr;
    const PhasorDataMapPtr nullPhasorDataMap = nullptr;

    const DataTablePtr& phasorDetail = metadata->Table("PhasorDetail");
    StringMap<PhasorDataMapPtr> phasorData;

    if (phasorDetail != nullptr)
    {
        const int32_t id = GetColumnIndex(phasorDetail, "ID");
        const int32_t deviceAcronym = GetColumnIndex(phasorDetail, "DeviceAcronym");
        const int32_t type = GetColumnIndex(phasorDetail, "Type");
        const int32_t phase = GetColumnIndex(phasorDetail, "Phase");
        const int32_t sourceIndex = GetColumnIndex(phasorDetail, "SourceIndex");
        
        for (int32_t i = 0; i < phasorDetail->RowCount(); i++)
        {
            const DataRowPtr& row = phasorDetail->Row(i);
            
            string deviceAcronymRef = row->ValueAsString(deviceAcronym).GetValueOrDefault();

            if (deviceAcronymRef.empty())
                continue;

            PhasorDataMapPtr phasorMap;
            const PhasorDataPtr phasor = NewSharedPtr<PhasorData>();

            phasor->PhasorID = row->ValueAsInt32(id).GetValueOrDefault();
            phasor->PhasorType = row->ValueAsString(type).GetValueOrDefault();
            phasor->Phase = row->ValueAsString(phase).GetValueOrDefault();

            if (!TryGetValue(phasorData, deviceAcronymRef, phasorMap, nullPhasorDataMap))
            {
                phasorMap = NewSharedPtr<PhasorDataMap>();
                phasorData[deviceAcronymRef] = phasorMap;
            }

            phasorMap->insert_or_assign(row->ValueAsInt32(sourceIndex).GetValueOrDefault(), phasor);
        }
    }

    // Load active meta-data measurements schema
    DataSetPtr filteringMetadata = DataSet::FromXml(ActiveMeasurementsSchema, ActiveMeasurementsSchemaLength);

    // Build active meta-data measurements from all meta-data
    const DataTablePtr& measurementDetail = metadata->Table("MeasurementDetail");
    const DataTablePtr& activeMeasurements = filteringMetadata->Table("ActiveMeasurements");

    if (measurementDetail != nullptr && activeMeasurements != nullptr)
    {
        // Lookup column indices for measurement detail table
        const int32_t md_deviceAcronym = GetColumnIndex(measurementDetail, "DeviceAcronym");
        const int32_t md_id = GetColumnIndex(measurementDetail, "ID");
        const int32_t md_signalID = GetColumnIndex(measurementDetail, "SignalID");
        const int32_t md_pointTag = GetColumnIndex(measurementDetail, "PointTag");
        const int32_t md_signalReference = GetColumnIndex(measurementDetail, "SignalReference");
        const int32_t md_signalAcronym = GetColumnIndex(measurementDetail, "SignalAcronym");
        const int32_t md_phasorSourceIndex = GetColumnIndex(measurementDetail, "PhasorSourceIndex");
        const int32_t md_description = GetColumnIndex(measurementDetail, "Description");
        const int32_t md_internal = GetColumnIndex(measurementDetail, "Internal");
        const int32_t md_enabled = GetColumnIndex(measurementDetail, "Enabled");
        const int32_t md_updatedOn = GetColumnIndex(measurementDetail, "UpdatedOn");

        // Lookup column indices for active measurements table
        const int32_t am_sourceNodeID = GetColumnIndex(activeMeasurements, "SourceNodeID");
        const int32_t am_id = GetColumnIndex(activeMeasurements, "ID");
        const int32_t am_signalID = GetColumnIndex(activeMeasurements, "SignalID");
        const int32_t am_pointTag = GetColumnIndex(activeMeasurements, "PointTag");
        const int32_t am_signalReference = GetColumnIndex(activeMeasurements, "SignalReference");
        const int32_t am_internal = GetColumnIndex(activeMeasurements, "Internal");
        const int32_t am_subscribed = GetColumnIndex(activeMeasurements, "Subscribed");
        const int32_t am_device = GetColumnIndex(activeMeasurements, "Device");
        const int32_t am_deviceID = GetColumnIndex(activeMeasurements, "DeviceID");
        const int32_t am_framesPerSecond = GetColumnIndex(activeMeasurements, "FramesPerSecond");
        const int32_t am_protocol = GetColumnIndex(activeMeasurements, "Protocol");
        const int32_t am_protocolType = GetColumnIndex(activeMeasurements, "ProtocolType");
        const int32_t am_signalType = GetColumnIndex(activeMeasurements, "SignalType");
        const int32_t am_engineeringUnits = GetColumnIndex(activeMeasurements, "EngineeringUnits");
        const int32_t am_phasorID = GetColumnIndex(activeMeasurements, "PhasorID");
        const int32_t am_phasorType = GetColumnIndex(activeMeasurements, "PhasorType");
        const int32_t am_phase = GetColumnIndex(activeMeasurements, "Phase");
        const int32_t am_adder = GetColumnIndex(activeMeasurements, "Adder");
        const int32_t am_multiplier = GetColumnIndex(activeMeasurements, "Multiplier");
        const int32_t am_company = GetColumnIndex(activeMeasurements, "Company");
        const int32_t am_longitude = GetColumnIndex(activeMeasurements, "Longitude");
        const int32_t am_latitude = GetColumnIndex(activeMeasurements, "Latitude");
        const int32_t am_description = GetColumnIndex(activeMeasurements, "Description");
        const int32_t am_updatedOn = GetColumnIndex(activeMeasurements, "UpdatedOn");

        for (int32_t i = 0; i < measurementDetail->RowCount(); i++)
        {
            const DataRowPtr& md_row = measurementDetail->Row(i);

            if (!md_row->ValueAsBoolean(md_enabled).GetValueOrDefault())
                continue;
            
            DataRowPtr am_row = activeMeasurements->CreateRow();

            am_row->SetGuidValue(am_sourceNodeID, m_nodeID);
            am_row->SetStringValue(am_id, md_row->ValueAsString(md_id));
            am_row->SetGuidValue(am_signalID, md_row->ValueAsGuid(md_signalID));
            am_row->SetStringValue(am_pointTag, md_row->ValueAsString(md_pointTag));
            am_row->SetStringValue(am_signalReference, md_row->ValueAsString(md_signalReference));
            am_row->SetInt32Value(am_internal, md_row->ValueAsBoolean(md_internal).GetValueOrDefault() ? 1 : 0);
            am_row->SetInt32Value(am_subscribed, 0);
            am_row->SetStringValue(am_description, md_row->ValueAsString(md_description));
            am_row->SetDoubleValue(am_adder, 0.0);
            am_row->SetDoubleValue(am_multiplier, 1.0);
            am_row->SetDateTimeValue(am_updatedOn, md_row->ValueAsDateTime(md_updatedOn));

            string signalType = md_row->ValueAsString(md_signalAcronym).GetValueOrDefault();

            if (signalType.empty())
                signalType = "CALC";

            am_row->SetStringValue(am_signalType, signalType);
            am_row->SetStringValue(am_engineeringUnits, GetEngineeringUnits(signalType));

            string deviceAcronymRef = md_row->ValueAsString(md_deviceAcronym).GetValueOrDefault();

            if (deviceAcronymRef.empty())
            {
                // Set any default values when measurement is not associated with a device
                am_row->SetInt32Value(am_framesPerSecond, 30);
            }
            else
            {
                am_row->SetStringValue(am_device, deviceAcronymRef);

                DeviceDataPtr device;

                // Lookup associated device record
                if (TryGetValue(deviceData, deviceAcronymRef, device, nullDeviceData))
                {
                    am_row->SetInt32Value(am_deviceID, device->DeviceID);
                    am_row->SetInt32Value(am_framesPerSecond, device->FramesPerSecond);
                    am_row->SetStringValue(am_company, device->Company);
                    am_row->SetStringValue(am_protocol, device->Protocol);
                    am_row->SetStringValue(am_protocolType, device->ProtocolType);
                    am_row->SetDecimalValue(am_longitude, device->Longitude);
                    am_row->SetDecimalValue(am_latitude, device->Latitude);
                }

                PhasorDataMapPtr phasorMap;

                // Lookup associated phasor records
                if (TryGetValue(phasorData, deviceAcronymRef, phasorMap, nullPhasorDataMap))
                {
                    PhasorDataPtr phasor;
                    int32_t sourceIndex = md_row->ValueAsInt32(md_phasorSourceIndex).GetValueOrDefault();

                    if (TryGetValue(*phasorMap, sourceIndex, phasor, nullPhasorData))
                    {
                        am_row->SetInt32Value(am_phasorID, phasor->PhasorID);
                        am_row->SetStringValue(am_phasorType, phasor->PhasorType);
                        am_row->SetStringValue(am_phase, phasor->Phase);
                    }
                }
            }

            activeMeasurements->AddRow(am_row);
        }
    }

    m_filteringMetadata.swap(filteringMetadata);

    // Notify all subscribers that the configuration metadata has changed
    m_subscriberConnectionsLock.lock();

    for (const auto& connection : m_subscriberConnections)
        connection->SendResponse(ServerResponse::ConfigurationChanged, ServerCommand::Subscribe);

    m_subscriberConnectionsLock.unlock();
}

const DataSetPtr& DataPublisher::GetMetadata() const
{
    return m_metadata;
}

const DataSetPtr& DataPublisher::GetFilteringMetadata() const
{
    return m_filteringMetadata;
}

vector<MeasurementMetadataPtr> DataPublisher::FilterMetadata(const string& filterExpression) const
{
    if (m_metadata == nullptr)
        throw PublisherException("Cannot filter metadata, no metadata has been defined.");

    vector<DataRowPtr> rows = FilterExpressionParser::Select(m_metadata, filterExpression, "MeasurementDetail");
    vector<MeasurementMetadataPtr> measurementMetadata;
    const DataTablePtr& measurementDetail = m_metadata->Table("MeasurementDetail");
    
    const int32_t deviceAcronym = GetColumnIndex(measurementDetail, "DeviceAcronym");
    const int32_t id = GetColumnIndex(measurementDetail, "ID");
    const int32_t signalID = GetColumnIndex(measurementDetail, "SignalID");
    const int32_t pointTag = GetColumnIndex(measurementDetail, "PointTag");
    const int32_t signalReference = GetColumnIndex(measurementDetail, "SignalReference");
    const int32_t phasorSourceIndex = GetColumnIndex(measurementDetail, "PhasorSourceIndex");
    const int32_t description = GetColumnIndex(measurementDetail, "Description");
    const int32_t enabled = GetColumnIndex(measurementDetail, "Enabled");
    const int32_t updatedOn = GetColumnIndex(measurementDetail, "UpdatedOn");

    for (size_t i = 0; i < rows.size(); i++)
    {
        DataRowPtr row = rows[i];
        MeasurementMetadataPtr metadata = NewSharedPtr<MeasurementMetadata>();

        if (!row->ValueAsBoolean(enabled).GetValueOrDefault())
            continue;

        metadata->DeviceAcronym = row->ValueAsString(deviceAcronym).GetValueOrDefault();
        metadata->ID = row->ValueAsString(id).GetValueOrDefault();
        metadata->SignalID = row->ValueAsGuid(signalID).GetValueOrDefault();
        metadata->PointTag = row->ValueAsString(pointTag).GetValueOrDefault();
        metadata->Reference = SignalReference(row->ValueAsString(signalReference).GetValueOrDefault());
        metadata->PhasorSourceIndex = uint16_t(row->ValueAsInt32(phasorSourceIndex).GetValueOrDefault());
        metadata->Description = row->ValueAsString(description).GetValueOrDefault();
        metadata->UpdatedOn = row->ValueAsDateTime(updatedOn).GetValueOrDefault();

        measurementMetadata.push_back(metadata);
    }

    return measurementMetadata;
}

void DataPublisher::PublishMeasurements(const vector<Measurement>& measurements)
{
    vector<MeasurementPtr> measurementPtrs;

    measurementPtrs.reserve(measurements.size());

    for (const auto& measurement : measurements)
        measurementPtrs.push_back(ToPtr(measurement));

    PublishMeasurements(measurementPtrs);
}

void DataPublisher::PublishMeasurements(const vector<MeasurementPtr>& measurements)
{
    m_routingTables.PublishMeasurements(measurements);
}

const GSF::Guid& DataPublisher::GetNodeID() const
{
    return m_nodeID;
}

void DataPublisher::SetNodeID(const GSF::Guid& value)
{
    m_nodeID = value;
}

SecurityMode DataPublisher::GetSecurityMode() const
{
    return m_securityMode;
}

void DataPublisher::SetSecurityMode(SecurityMode value)
{
    m_securityMode = value;
}

int32_t DataPublisher::GetMaximumAllowedConnections() const
{
    return m_maximumAllowedConnections;
}

void DataPublisher::SetMaximumAllowedConnections(int32_t value)
{
    m_maximumAllowedConnections = value;
}

bool DataPublisher::GetIsMetadataRefreshAllowed() const
{
    return m_isMetadataRefreshAllowed;
}

void DataPublisher::SetIsMetadataRefreshAllowed(bool value)
{
    m_isMetadataRefreshAllowed = value;
}

bool DataPublisher::GetIsNaNValueFilterAllowed() const
{
    return m_isNaNValueFilterAllowed;
}

void DataPublisher::SetNaNValueFilterAllowed(bool value)
{
    m_isNaNValueFilterAllowed = value;
}

bool DataPublisher::GetIsNaNValueFilterForced() const
{
    return m_isNaNValueFilterForced;
}

void DataPublisher::SetIsNaNValueFilterForced(bool value)
{
    m_isNaNValueFilterForced = value;
}

bool DataPublisher::GetSupportsTemporalSubscriptions() const
{
    return m_supportsTemporalSubscriptions;
}

void DataPublisher::SetSupportsTemporalSubscriptions(bool value)
{
    m_supportsTemporalSubscriptions = value;
}

uint32_t DataPublisher::GetCipherKeyRotationPeriod() const
{
    return m_cipherKeyRotationPeriod;
}

void DataPublisher::SetCipherKeyRotationPeriod(uint32_t value)
{
    m_cipherKeyRotationPeriod = value;
}

void* DataPublisher::GetUserData() const
{
    return m_userData;
}

void DataPublisher::SetUserData(void* userData)
{
    m_userData = userData;
}

uint64_t DataPublisher::GetTotalCommandChannelBytesSent()
{
    uint64_t totalCommandChannelBytesSent = 0L;

    m_subscriberConnectionsLock.lock();

    for (const auto& connection : m_subscriberConnections)
        totalCommandChannelBytesSent += connection->GetTotalCommandChannelBytesSent();

    m_subscriberConnectionsLock.unlock();

    return totalCommandChannelBytesSent;
}

uint64_t DataPublisher::GetTotalDataChannelBytesSent()
{
    uint64_t totalDataChannelBytesSent = 0L;

    m_subscriberConnectionsLock.lock();

    for (const auto& connection : m_subscriberConnections)
        totalDataChannelBytesSent += connection->GetTotalDataChannelBytesSent();

    m_subscriberConnectionsLock.unlock();

    return totalDataChannelBytesSent;
}

uint64_t DataPublisher::GetTotalMeasurementsSent()
{
    uint64_t totalMeasurementsSent = 0L;

    m_subscriberConnectionsLock.lock();

    for (const auto& connection : m_subscriberConnections)
        totalMeasurementsSent += connection->GetTotalMeasurementsSent();

    m_subscriberConnectionsLock.unlock();

    return totalMeasurementsSent;
}

void DataPublisher::RegisterStatusMessageCallback(const MessageCallback& statusMessageCallback)
{
    m_statusMessageCallback = statusMessageCallback;
}

void DataPublisher::RegisterErrorMessageCallback(const MessageCallback& errorMessageCallback)
{
    m_errorMessageCallback = errorMessageCallback;
}

void DataPublisher::RegisterClientConnectedCallback(const SubscriberConnectionCallback& clientConnectedCallback)
{
    m_clientConnectedCallback = clientConnectedCallback;
}

void DataPublisher::RegisterClientDisconnectedCallback(const SubscriberConnectionCallback& clientDisconnectedCallback)
{
    m_clientDisconnectedCallback = clientDisconnectedCallback;
}

void DataPublisher::RegisterProcessingIntervalChangeRequestedCallback(const SubscriberConnectionCallback& processingIntervalChangeRequestedCallback)
{
    m_processingIntervalChangeRequestedCallback = processingIntervalChangeRequestedCallback;
}

void DataPublisher::RegisterTemporalSubscriptionRequestedCallback(const SubscriberConnectionCallback& temporalSubscriptionRequestedCallback)
{
    m_temporalSubscriptionRequestedCallback = temporalSubscriptionRequestedCallback;
}

void DataPublisher::RegisterTemporalSubscriptionCanceledCallback(const SubscriberConnectionCallback& temporalSubscriptionCanceledCallback)
{
    m_temporalSubscriptionCanceledCallback = temporalSubscriptionCanceledCallback;
}

void DataPublisher::IterateSubscriberConnections(const SubscriberConnectionIteratorHandlerFunction& iteratorHandler, void* userData)
{
    m_subscriberConnectionsLock.lock();

    for (const auto& connection : m_subscriberConnections)
        iteratorHandler(connection, userData);

    m_subscriberConnectionsLock.unlock();
}
