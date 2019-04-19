//******************************************************************************************************
//  SubscriberInstance.cpp - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "SubscriberInstance.h"
#include "Constants.h"
#include "../Common/Convert.h"
#include "../Common/EndianConverter.h"
#include "../Common/pugixml.hpp"
#include <iostream>

using namespace std;
using namespace pugi;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

SubscriberInstance::SubscriberInstance() :
    m_hostname("localhost"),
    m_port(6165),
    m_udpPort(0U),
    m_autoReconnect(true),
    m_autoParseMetadata(true),
    m_maxRetries(-1),
    m_retryInterval(2000),
    m_filterExpression(SubscribeAllNoStatsExpression),
    m_startTime(""),
    m_stopTime(""),
    m_userData(nullptr)
{
    // Reference this SubscriberInstance in DataSubsciber user data
    m_subscriber = NewSharedPtr<DataSubscriber>();
    m_subscriber->SetUserData(this);
}

// public functions

void SubscriberInstance::Initialize(const string& hostname, uint16_t port, uint16_t udpPort)
{
    m_hostname = hostname;
    m_port = port;
    m_udpPort = udpPort;
}

const Guid& SubscriberInstance::GetSubscriberID() const
{
    return m_subscriber->GetSubscriberID();
}

bool SubscriberInstance::GetAutoReconnect() const
{
    return m_autoReconnect;
}

void SubscriberInstance::SetAutoReconnect(bool autoReconnect)
{
    m_autoReconnect = autoReconnect;
}

bool SubscriberInstance::GetAutoParseMetadata() const
{
    return m_autoParseMetadata;
}

void SubscriberInstance::SetAutoParseMetadata(bool autoParseMetadata)
{
    m_autoParseMetadata = autoParseMetadata;
}

int16_t SubscriberInstance::GetMaxRetries() const
{
    return m_maxRetries;
}

void SubscriberInstance::SetMaxRetries(int16_t maxRetries)
{
    m_maxRetries = maxRetries;
}

int16_t SubscriberInstance::GetRetryInterval() const
{
    return m_retryInterval;
}

void SubscriberInstance::SetRetyInterval(int16_t retryInterval)
{
    m_retryInterval = retryInterval;
}

void SubscriberInstance::EstablishHistoricalRead(const string& startTime, const string& stopTime)
{
    m_startTime = startTime;
    m_stopTime = stopTime;
}

const std::string& SubscriberInstance::GetFilterExpression() const
{
    return m_filterExpression;
}

void SubscriberInstance::SetFilterExpression(const string& filterExpression)
{
    m_filterExpression = filterExpression;

    // Resubscribe with new filter expression if already connected
    if (m_subscriber->IsSubscribed())
    {
        m_subscriptionInfo.FilterExpression = m_filterExpression;
        m_subscriber->Subscribe(m_subscriptionInfo);
    }
}

const std::string& SubscriberInstance::GetMetadataFilters() const
{
    return m_metadataFilters;
}

void SubscriberInstance::SetMetadataFilters(const std::string& metadataFilters)
{
    m_metadataFilters = metadataFilters;
}

void SubscriberInstance::ConnectAsync()
{
    Thread(bind(&SubscriberInstance::Connect, this));
}

void SubscriberInstance::Connect()
{
    SubscriberConnector& connector = m_subscriber->GetSubscriberConnector();

    // Set up helper objects (derived classes can override behavior and settings)
    SetupSubscriberConnector(connector);
    m_subscriptionInfo = CreateSubscriptionInfo();

    // Register callbacks
    m_subscriber->RegisterStatusMessageCallback(&HandleStatusMessage);
    m_subscriber->RegisterErrorMessageCallback(&HandleErrorMessage);
    m_subscriber->RegisterDataStartTimeCallback(&HandleDataStartTime);
    m_subscriber->RegisterMetadataCallback(&HandleMetadata);
    m_subscriber->RegisterNewMeasurementsCallback(&HandleNewMeasurements);
    m_subscriber->RegisterConfigurationChangedCallback(&HandleConfigurationChanged);
    m_subscriber->RegisterConnectionTerminatedCallback(&HandleConnectionTerminated);

    if (!m_startTime.empty() && !m_stopTime.empty())
    {
        m_subscriber->RegisterProcessingCompleteCallback(&HandleProcessingComplete);
        m_subscriptionInfo.StartTime = m_startTime;
        m_subscriptionInfo.StopTime = m_stopTime;
    }

    if (m_udpPort > 0)
    {
        m_subscriptionInfo.UdpDataChannel = true;
        m_subscriptionInfo.DataChannelLocalPort = m_udpPort;
    }

    // Connect and subscribe to publisher
    if (connector.Connect(*m_subscriber, m_subscriptionInfo))
    {
        ConnectionEstablished();

        // If automatically parsing metadata, request metadata upon successful connection,
        // after metadata is handled the SubscriberInstance will then initiate subscribe;
        // otherwise, initiate subscribe immediately
        if (m_autoParseMetadata)
            SendMetadataRefreshCommand();
        else
            m_subscriber->Subscribe();
    }
    else
    {
        ErrorMessage("All connection attempts failed");
    }
}

void SubscriberInstance::Disconnect() const
{
    m_subscriber->Disconnect();
}

void SubscriberInstance::SetHistoricalReplayInterval(int32_t replayInterval) const
{
    if (m_subscriber->IsSubscribed())
    {
        replayInterval = EndianConverter::Default.ConvertBigEndian(replayInterval);
        m_subscriber->SendServerCommand(ServerCommand::UpdateProcessingInterval, reinterpret_cast<uint8_t*>(&replayInterval), 0, 4);
    }
}

void* SubscriberInstance::GetUserData() const
{
    return m_userData;
}

void SubscriberInstance::SetUserData(void* userData)
{
    m_userData = userData;
}

bool SubscriberInstance::IsPayloadDataCompressed() const
{
    return m_subscriber->IsPayloadDataCompressed();
}

void SubscriberInstance::SetPayloadDataCompressed(bool compressed) const
{
    m_subscriber->SetPayloadDataCompressed(compressed);
}

bool SubscriberInstance::IsMetadataCompressed() const
{
    return m_subscriber->IsMetadataCompressed();
}

void SubscriberInstance::SetMetadataCompressed(bool compressed) const
{
    m_subscriber->SetMetadataCompressed(compressed);
}

bool SubscriberInstance::IsSignalIndexCacheCompressed() const
{
    return m_subscriber->IsSignalIndexCacheCompressed();
}

void SubscriberInstance::SetSignalIndexCacheCompressed(bool compressed) const
{
    m_subscriber->SetSignalIndexCacheCompressed(compressed);
}

uint64_t SubscriberInstance::GetTotalCommandChannelBytesReceived() const
{
    return m_subscriber->GetTotalCommandChannelBytesReceived();
}

uint64_t SubscriberInstance::GetTotalDataChannelBytesReceived() const
{
    return m_subscriber->GetTotalDataChannelBytesReceived();
}

uint64_t SubscriberInstance::GetTotalMeasurementsReceived() const
{
    return m_subscriber->GetTotalMeasurementsReceived();
}

bool SubscriberInstance::IsConnected() const
{
    return m_subscriber->IsConnected();
}

bool SubscriberInstance::IsSubscribed() const
{
    return m_subscriber->IsSubscribed();
}

void SubscriberInstance::IterateDeviceMetadata(const DeviceMetadataIteratorHandlerFunction& iteratorHandler, void* userData)
{
    m_configurationUpdateLock.lock();

    for (auto const& item : m_devices)
        iteratorHandler(item.second, userData);

    m_configurationUpdateLock.unlock();
}

void SubscriberInstance::IterateMeasurementMetadata(const MeasurementMetadataIteratorHandlerFunction& iteratorHandler, void* userData)
{
    m_configurationUpdateLock.lock();

    for (auto const& item : m_measurements)
        iteratorHandler(item.second, userData);

    m_configurationUpdateLock.unlock();
}

void SubscriberInstance::IterateConfigurationFrames(const ConfigurationFrameIteratorHandlerFunction& iteratorHandler, void* userData)
{
    m_configurationUpdateLock.lock();

    for (auto const& item : m_configurationFrames)
        iteratorHandler(item.second, userData);

    m_configurationUpdateLock.unlock();
}

void IterateDeviceMetadataForAcronyms(const DeviceMetadataPtr& device, void* userData)
{
    vector<string>* deviceAcronyms = static_cast<vector<string>*>(userData);
    deviceAcronyms->push_back(device->Acronym);
}

bool SubscriberInstance::TryGetDeviceAcronyms(vector<string>& deviceAcronyms)
{
    deviceAcronyms.clear();

    IterateDeviceMetadata(&IterateDeviceMetadataForAcronyms, &deviceAcronyms);

    return !deviceAcronyms.empty();
}

void IterateDeviceMetadataForCopy(const DeviceMetadataPtr& device, void* userData)
{
    map<string, DeviceMetadataPtr>* devices = static_cast<map<string, DeviceMetadataPtr>*>(userData);
    devices->insert_or_assign(device->Acronym, device);
}

void SubscriberInstance::GetParsedDeviceMetadata(map<string, DeviceMetadataPtr>& devices)
{
    devices.clear();
    IterateDeviceMetadata(&IterateDeviceMetadataForCopy, &devices);
}

void IterateMeasurementMetadataForCopy(const MeasurementMetadataPtr& measurement, void* userData)
{
    map<Guid, MeasurementMetadataPtr>* measurements = static_cast<map<Guid, MeasurementMetadataPtr>*>(userData);
    measurements->insert_or_assign(measurement->SignalID, measurement);
}

void SubscriberInstance::GetParsedMeasurementMetadata(map<Guid, MeasurementMetadataPtr>& measurements)
{
    measurements.clear();
    IterateMeasurementMetadata(&IterateMeasurementMetadataForCopy, &measurements);
}

bool SubscriberInstance::TryGetDeviceMetadata(const string& deviceAcronym, DeviceMetadataPtr& deviceMetadata)
{
    bool found = false;

    m_configurationUpdateLock.lock();

    const auto iterator = m_devices.find(deviceAcronym);

    if (iterator != m_devices.end())
    {
        deviceMetadata = iterator->second;
        found = true;
    }

    m_configurationUpdateLock.unlock();

    return found;
}

bool SubscriberInstance::TryGetMeasurementMetdata(const Guid& signalID, MeasurementMetadataPtr& measurementMetadata)
{
    bool found = false;

    m_configurationUpdateLock.lock();

    const auto iterator = m_measurements.find(signalID);

    if (iterator != m_measurements.end())
    {
        measurementMetadata = iterator->second;
        found = true;
    }

    m_configurationUpdateLock.unlock();

    return found;
}

bool SubscriberInstance::TryGetConfigurationFrame(const string& deviceAcronym, ConfigurationFramePtr& configurationFrame)
{
    bool found = false;

    m_configurationUpdateLock.lock();

    const auto iterator = m_configurationFrames.find(deviceAcronym);

    if (iterator != m_configurationFrames.end())
    {
        configurationFrame = iterator->second;
        found = true;
    }

    m_configurationUpdateLock.unlock();

    return found;
}

bool SubscriberInstance::TryFindTargetConfigurationFrame(const Guid& signalID, ConfigurationFramePtr& targetFrame)
{
    bool found = false;

    m_configurationUpdateLock.lock();

    for (auto const& frameRecord : m_configurationFrames)
    {
        const ConfigurationFramePtr currentFrame = frameRecord.second;
        const auto iterator = currentFrame->Measurements.find(signalID);

        if (iterator != currentFrame->Measurements.end())
        {
            targetFrame = currentFrame;
            found = true;
            break;
        }
    }

    m_configurationUpdateLock.unlock();

    return found;
}

bool SubscriberInstance::TryGetMeasurementMetdataFromConfigurationFrame(const Guid& signalID, const ConfigurationFramePtr& sourceFrame, MeasurementMetadataPtr& measurementMetadata)
{
    if (sourceFrame == nullptr)
        return false;

    bool found = false;

    if (sourceFrame->StatusFlags && sourceFrame->StatusFlags->SignalID == signalID)
    {
        measurementMetadata = sourceFrame->StatusFlags;
        found = true;
    }
    else if (sourceFrame->Frequency && sourceFrame->Frequency->SignalID == signalID)
    {
        measurementMetadata = sourceFrame->Frequency;
        found = true;
    }
    else if (sourceFrame->DfDt && sourceFrame->DfDt->SignalID == signalID)
    {
        measurementMetadata = sourceFrame->DfDt;
        found = true;
    }
    else
    {
        // Search phasors
        for (auto const& phasor : sourceFrame->Phasors)
        {
            if (phasor->Angle && phasor->Angle->SignalID == signalID)
            {
                measurementMetadata = phasor->Angle;
                found = true;
                break;
            }

            if (phasor->Magnitude && phasor->Magnitude->SignalID == signalID)
            {
                measurementMetadata = phasor->Magnitude;
                found = true;
                break;
            }
        }

        // Search analogs
        if (!found)
        {
            for (auto const& analog : sourceFrame->Analogs)
            {
                if (analog && analog->SignalID == signalID)
                {
                    measurementMetadata = analog;
                    found = true;
                    break;
                }
            }
        }

        // Search digitals
        if (!found)
        {
            for (auto const& digital : sourceFrame->Digitals)
            {
                if (digital && digital->SignalID == signalID)
                {
                    measurementMetadata = digital;
                    found = true;
                    break;
                }
            }
        }

        // Check quality flags (rare)
        if (!found)
        {
            if (sourceFrame->QualityFlags && sourceFrame->QualityFlags->SignalID == signalID)
            {
                measurementMetadata = sourceFrame->QualityFlags;
                found = true;
            }
        }
    }

    return found;
}

// protected functions

// All the following protected functions are virtual so that derived
// classes can customize behavior of the SubscriberInstance

void SubscriberInstance::SetupSubscriberConnector(SubscriberConnector& connector)
{
    // SubscriberConnector is another helper object which allows the
    // user to modify settings for auto-reconnects and retry cycles.

    // Register callbacks
    connector.RegisterErrorMessageCallback(&HandleErrorMessage);
    connector.RegisterReconnectCallback(&HandleResubscribe);

    connector.SetHostname(m_hostname);
    connector.SetPort(m_port);
    connector.SetMaxRetries(m_maxRetries);
    connector.SetRetryInterval(m_retryInterval);
    connector.SetAutoReconnect(m_autoReconnect);
}

SubscriptionInfo SubscriberInstance::CreateSubscriptionInfo()
{
    // SubscriptionInfo is a helper object which allows the user
    // to set up their subscription and reuse subscription settings.
    SubscriptionInfo info;

    // Define desired filter expression
    info.FilterExpression = m_filterExpression;
    info.Throttled = false;
    info.UdpDataChannel = false;
    info.IncludeTime = true;
    info.LagTime = 3.0F;
    info.LeadTime = 1.0F;
    info.UseLocalClockAsRealTime = false;
    info.UseMillisecondResolution = true;

    return info;
}

SubscriberInstance::~SubscriberInstance() = default;

void SubscriberInstance::StatusMessage(const string& message)
{
    cout << message << endl << endl;
}

void SubscriberInstance::ErrorMessage(const string& message)
{
    cerr << message << endl << endl;
}

void SubscriberInstance::DataStartTime(time_t unixSOC, uint16_t milliseconds)
{
}

void SubscriberInstance::DataStartTime(datetime_t startTime)
{
}

void SubscriberInstance::ReceivedMetadata(const vector<uint8_t>& payload)
{
    if (!m_autoParseMetadata)
        return;

    if (payload.empty())
    {
        ErrorMessage("Received empty payload for meta data refresh.");
        return;
    }

    vector<uint8_t>* uncompressesBuffer;

    // Step 1: Decompress meta-data if needed
    if (IsMetadataCompressed())
    {
        // Perform zlib decompression on buffer
        const MemoryStream memoryStream(payload);
        StreamBuffer streamBuffer;

        streamBuffer.push(GZipDecompressor());
        streamBuffer.push(memoryStream);

        uncompressesBuffer = new vector<uint8_t>();
        CopyStream(&streamBuffer, *uncompressesBuffer);
    }
    else
    {
        uncompressesBuffer = const_cast<vector<uint8_t>*>(&payload);
    }

    // Step 2: Load string into an XML parser
    xml_document document;

    const xml_parse_result result = document.load_buffer_inplace(static_cast<void*>(uncompressesBuffer->data()), uncompressesBuffer->size());

    if (result.status != xml_parse_status::status_ok)
    {
        if (IsMetadataCompressed())
            delete uncompressesBuffer;

        stringstream errorMessageStream;
        errorMessageStream << "Failed to parse meta data XML, status code = " << ToHex(result.status);
        ErrorMessage(errorMessageStream.str());
        return;
    }

    // Find root node
    xml_node rootNode = document.document_element();

    // Query DeviceDetail records from metadata
    StringMap<DeviceMetadataPtr> devices;

    for (xml_node device = rootNode.child("DeviceDetail"); device; device = device.next_sibling("DeviceDetail"))
    {
        DeviceMetadataPtr deviceMetadata = NewSharedPtr<DeviceMetadata>();

        deviceMetadata->Acronym = device.child_value("Acronym");
        deviceMetadata->Name = device.child_value("Name");
        deviceMetadata->UniqueID = ParseGuid(device.child_value("UniqueID"));
        deviceMetadata->AccessID = stoi(Coalesce(device.child_value("AccessID"), "0"));
        deviceMetadata->ParentAcronym = device.child_value("ParentAcronym");
        deviceMetadata->ProtocolName = device.child_value("ProtocolName");
        deviceMetadata->FramesPerSecond = stoi(Coalesce(device.child_value("FramesPerSecond"), "30"));
        deviceMetadata->CompanyAcronym = device.child_value("CompanyAcronym");
        deviceMetadata->VendorAcronym = device.child_value("vendorAcronym");
        deviceMetadata->VendorDeviceName = device.child_value("VendorDeviceName");
        deviceMetadata->Longitude = stod(Coalesce(device.child_value("Longitude"), "0.0"));
        deviceMetadata->Latitude = stod(Coalesce(device.child_value("Latitude"), "0.0"));
        deviceMetadata->UpdatedOn = ParseTimestamp(device.child_value("UpdatedOn"));

        devices.insert_or_assign(deviceMetadata->Acronym, deviceMetadata);
    }

    // Query MeasurementDetail records from metadata
    unordered_map<Guid, MeasurementMetadataPtr> measurements;

    for (xml_node device = rootNode.child("MeasurementDetail"); device; device = device.next_sibling("MeasurementDetail"))
    {
        MeasurementMetadataPtr measurementMetadata = NewSharedPtr<MeasurementMetadata>();

        measurementMetadata->DeviceAcronym = device.child_value("DeviceAcronym");
        measurementMetadata->ID = device.child_value("ID");
        measurementMetadata->SignalID = ParseGuid(device.child_value("SignalID"));
        measurementMetadata->PointTag = device.child_value("PointTag");
        measurementMetadata->Reference = SignalReference(string(device.child_value("SignalReference")));
        measurementMetadata->PhasorSourceIndex = stoi(Coalesce(device.child_value("PhasorSourceIndex"), "0"));
        measurementMetadata->Description = device.child_value("Description");
        measurementMetadata->UpdatedOn = ParseTimestamp(device.child_value("UpdatedOn"));

        measurements.insert_or_assign(measurementMetadata->SignalID, measurementMetadata);

        // Lookup associated device
        auto iterator = devices.find(measurementMetadata->DeviceAcronym);

        if (iterator != devices.end())
        {
            // Add measurement to device's measurement list
            DeviceMetadataPtr& deviceMetaData = iterator->second;
            deviceMetaData->Measurements.push_back(measurementMetadata);
        }
    }

    // Query PhasorDetail records from metadata
    uint16_t phasorCount = 0;

    for (xml_node device = rootNode.child("PhasorDetail"); device; device = device.next_sibling("PhasorDetail"))
    {
        PhasorMetadataPtr phasorMetadata = NewSharedPtr<PhasorMetadata>();

        phasorMetadata->DeviceAcronym = device.child_value("DeviceAcronym");
        phasorMetadata->Label = device.child_value("Label");
        phasorMetadata->Type = device.child_value("Type");
        phasorMetadata->Phase = device.child_value("Phase");
        phasorMetadata->SourceIndex = stoi(Coalesce(device.child_value("SourceIndex"), "0"));
        phasorMetadata->UpdatedOn = ParseTimestamp(device.child_value("UpdatedOn"));

        // Create a new phasor reference
        PhasorReferencePtr phasorReference = NewSharedPtr<PhasorReference>();
        phasorReference->Phasor = phasorMetadata;

        // Lookup associated device
        auto iterator = devices.find(phasorMetadata->DeviceAcronym);

        if (iterator == devices.end())
        {
            // If associated device was not found, continue on
            stringstream errorMessageStream;
            errorMessageStream << "Could not find device " << phasorMetadata->DeviceAcronym << " referenced by phasor " << phasorMetadata->Label;
            ErrorMessage(errorMessageStream.str());
            continue;
        }

        DeviceMetadataPtr& deviceMetadata = iterator->second;

        // Lookup associated phasor measurements
        uint16_t matchCount = 0;

        for (auto const& measurementMetadata : deviceMetadata->Measurements)
        {
            // Check if source indexes also match - if so, we found an associated measurement
            if (measurementMetadata->PhasorSourceIndex == phasorMetadata->SourceIndex)
            {
                // There should be two measurements that match DeviceAcronym and SourceIndex,
                // specifically one for the angle and one for the magnitude for each phasor
                if (measurementMetadata->Reference.Kind == SignalKind::Angle)
                {
                    phasorReference->Angle = measurementMetadata;
                    matchCount++;
                }
                else if (measurementMetadata->Reference.Kind == SignalKind::Magnitude)
                {
                    phasorReference->Magnitude = measurementMetadata;
                    matchCount++;
                }
                else
                {
                    // Unexpected condition:
                    stringstream errorMessageStream;
                    errorMessageStream << "Encountered a " << SignalKindDescription[measurementMetadata->Reference.Kind] << " measurement \"" << measurementMetadata->Reference << "\" that had a matching SourceIndex for phasor: " << phasorMetadata->Label;
                    ErrorMessage(errorMessageStream.str());
                }

                // Stop looking if we have found both matches
                if (matchCount >= 2)
                    break;
            }
        }

        // Add phasor to associated device meta data record
        deviceMetadata->Phasors.push_back(phasorReference);
        phasorCount++;
    }

    // Construct a "configuration frame" for each of the devices
    StringMap<ConfigurationFramePtr> configurationFrames;
    ConstructConfigurationFrames(devices, measurements, configurationFrames);

    m_configurationUpdateLock.lock();

    m_configurationFrames = configurationFrames;    // Replace the configuration frames list
    m_devices = devices;                            // Replace the device metadata list
    m_measurements = measurements;                  // Replace the measurement metadata list

    m_configurationUpdateLock.unlock();

    stringstream message;
    message << "Loaded " << devices.size() << " devices, " << measurements.size() << " measurements and " << phasorCount << " phasors from GEP meta data...";
    StatusMessage(message.str());

    // Release uncompressed buffer
    if (IsMetadataCompressed())
        delete uncompressesBuffer;

    // Notify derived class that meta-data has been parsed and is now available
    ParsedMetadata();
}

void SubscriberInstance::SendMetadataRefreshCommand()
{
    if (m_metadataFilters.empty())
    {
        m_subscriber->SendServerCommand(ServerCommand::MetadataRefresh);
        return;
    }

    // Send meta-data filters when some are specified
    vector<uint8_t> buffer;
    
    const uint8_t* metadataFiltersPtr = reinterpret_cast<uint8_t*>(&m_metadataFilters[0]);
    const uint32_t metadataFiltersSize = ConvertUInt32(m_metadataFilters.size() * sizeof(char));
    const uint32_t bufferSize = 4 + metadataFiltersSize;

    buffer.reserve(bufferSize);
    EndianConverter::WriteBigEndianBytes(buffer, metadataFiltersSize);
    WriteBytes(buffer, metadataFiltersPtr, 0, metadataFiltersSize);

    m_subscriber->SendServerCommand(ServerCommand::MetadataRefresh, &buffer[0], 0, bufferSize);
}

void SubscriberInstance::ConstructConfigurationFrames(const StringMap<DeviceMetadataPtr>& devices, const unordered_map<Guid, MeasurementMetadataPtr>& measurements, StringMap<ConfigurationFramePtr>& configurationFrames)
{
    for (auto const& deviceMapRecord : devices)
    {
        const DeviceMetadataPtr deviceMetadata = deviceMapRecord.second;
        const vector<PhasorReferencePtr>& phasors = deviceMetadata->Phasors;
        ConfigurationFramePtr configurationFrame = NewSharedPtr<ConfigurationFrame>();
        MeasurementMetadataPtr measurement;

        // Add single measurement definitions
        configurationFrame->DeviceAcronym = deviceMetadata->Acronym;

        if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::Status, measurement))
        {
            configurationFrame->StatusFlags = measurement;
            configurationFrame->Measurements.insert(measurement->SignalID);
        }
        else
        {
            configurationFrame->StatusFlags = nullptr;
        }

        if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::Frequency, measurement))
        {
            configurationFrame->Frequency = measurement;
            configurationFrame->Measurements.insert(measurement->SignalID);
        }
        else
        {
            configurationFrame->Frequency = nullptr;
        }

        if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::DfDt, measurement))
        {
            configurationFrame->DfDt = measurement;
            configurationFrame->Measurements.insert(measurement->SignalID);
        }
        else
        {
            configurationFrame->DfDt = nullptr;
        }


        if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::Quality, measurement))
        {
            configurationFrame->QualityFlags = measurement;
            configurationFrame->Measurements.insert(measurement->SignalID);
        }
        else
        {
            configurationFrame->QualityFlags = nullptr;
        }

        // Add phasor definitions
        const uint16_t phasorCount = GetSignalKindCount(deviceMetadata->Measurements, SignalKind::Angle);

        for (uint16_t i = 1; i <= phasorCount; i++)
        {
            bool found = false;

            for (auto const& phasorReference : phasors)
            {
                if (phasorReference->Phasor->SourceIndex == i)
                {
                    found = true;
                    configurationFrame->Phasors.push_back(phasorReference);

                    if (phasorReference->Angle)
                        configurationFrame->Measurements.insert(phasorReference->Angle->SignalID);

                    if (phasorReference->Magnitude)
                        configurationFrame->Measurements.insert(phasorReference->Magnitude->SignalID);
                    
                    break;
                }
            }

            if (!found)
            {
                // If no associated phasor reference was found,
                // we add an empty one to make sure each phasor
                // "position" has an entry in the config frame
                PhasorReferencePtr phasorReference = NewSharedPtr<PhasorReference>();

                phasorReference->Phasor = NewSharedPtr<PhasorMetadata>();                
                phasorReference->Phasor->DeviceAcronym = configurationFrame->DeviceAcronym;
                phasorReference->Phasor->Label = "UNDEFINED";
                phasorReference->Phasor->Type = "?";
                phasorReference->Phasor->Phase = "+";
                phasorReference->Phasor->SourceIndex = i;
                phasorReference->Phasor->UpdatedOn = datetime_t();
                
                phasorReference->Angle = nullptr;
                phasorReference->Magnitude = nullptr;

                configurationFrame->Phasors.push_back(phasorReference);
            }
        }

        // Add analog definitions
        const uint16_t analogCount = GetSignalKindCount(deviceMetadata->Measurements, SignalKind::Analog);

        for (uint16_t i = 1; i <= analogCount; i++)
        {
            if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::Analog, i, measurement))
            {
                configurationFrame->Analogs.push_back(measurement);
                configurationFrame->Measurements.insert(measurement->SignalID);
            }
            else
            {
                // If no associated analog measurement was found,
                // we add an empty one to make sure each analog
                // "position" has an entry in the config frame
                measurement = NewSharedPtr<MeasurementMetadata>();
                
                measurement->DeviceAcronym = configurationFrame->DeviceAcronym;
                measurement->ID = "__:-1";
                measurement->SignalID = Empty::Guid;
                measurement->PointTag = "UNDEFINED";
                measurement->Reference.SignalID = Empty::Guid;
                measurement->Reference.Acronym = measurement->DeviceAcronym;
                measurement->Reference.Index = i;
                measurement->Reference.Kind = SignalKind::Analog;
                measurement->PhasorSourceIndex = 0U;
                measurement->Description = "";
                measurement->UpdatedOn = datetime_t();

                configurationFrame->Analogs.push_back(measurement);
            }
        }


        // Add digital definitions
        const uint16_t digitalCount = GetSignalKindCount(deviceMetadata->Measurements, SignalKind::Digital);

        for (uint16_t i = 1; i <= digitalCount; i++)
        {
            if (TryFindMeasurement(deviceMetadata->Measurements, SignalKind::Digital, i, measurement))
            {
                configurationFrame->Digitals.push_back(measurement);
                configurationFrame->Measurements.insert(measurement->SignalID);
            }
            else
            {
                // If no associated digital measurement was found,
                // we add an empty one to make sure each digital
                // "position" has an entry in the config frame
                measurement = NewSharedPtr<MeasurementMetadata>();

                measurement->DeviceAcronym = configurationFrame->DeviceAcronym;
                measurement->ID = "__:-1";
                measurement->SignalID = Empty::Guid;
                measurement->PointTag = "UNDEFINED";
                measurement->Reference.SignalID = Empty::Guid;
                measurement->Reference.Acronym = measurement->DeviceAcronym;
                measurement->Reference.Index = i;
                measurement->Reference.Kind = SignalKind::Digital;
                measurement->PhasorSourceIndex = 0U;
                measurement->Description = "";
                measurement->UpdatedOn = datetime_t();

                configurationFrame->Digitals.push_back(measurement);
            }
        }

        configurationFrames.insert_or_assign(configurationFrame->DeviceAcronym, configurationFrame);
    }
}

bool SubscriberInstance::TryFindMeasurement(const vector<MeasurementMetadataPtr>& measurements, SignalKind kind, MeasurementMetadataPtr& measurementMetadata)
{
    return TryFindMeasurement(measurements, kind, 0U, measurementMetadata);
}

bool SubscriberInstance::TryFindMeasurement(const vector<MeasurementMetadataPtr>& measurements, SignalKind kind, uint16_t index, MeasurementMetadataPtr& measurementMetadata)
{
    for (auto const& measurement : measurements)
    {
        const SignalReference& reference = measurement->Reference;

        if (reference.Kind == kind && (index == 0 || reference.Index == index))
        {
            measurementMetadata = measurement;
            return true;
        }
    }

    return false;
}

uint16_t SubscriberInstance::GetSignalKindCount(const vector<MeasurementMetadataPtr>& measurements, SignalKind kind)
{
    uint16_t count = 0U;

    // Find largest signal reference index - this will be count
    for (auto const& measurement : measurements)
    {
        const SignalReference& reference = measurement->Reference;

        if (reference.Kind == kind && reference.Index > count)
            count = reference.Index;
    }

    return count;
}

void SubscriberInstance::ParsedMetadata()
{
}

void SubscriberInstance::ReceivedNewMeasurements(const vector<MeasurementPtr>& measurements)
{
}

void SubscriberInstance::ConfigurationChanged()
{
}

void SubscriberInstance::HistoricalReadComplete()
{
}

void SubscriberInstance::ConnectionEstablished()
{
}

void SubscriberInstance::ConnectionTerminated()
{
}

// private functions

// The following member methods are defined as static so they
// can be used as callback registrations for DataSubscriber

void SubscriberInstance::HandleResubscribe(DataSubscriber* source)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());

    if (source->IsConnected())
    {
        instance->StatusMessage("Reconnected. Subscribing to data...");
        instance->ConnectionEstablished();

        // If automatically parsing metadata, request metadata upon successful connection,
        // after metadata is handled the SubscriberInstance will then initiate subscribe;
        // otherwise, initiate subscribe immediately
        if (instance->m_autoParseMetadata)
            instance->SendMetadataRefreshCommand();
        else
            source->Subscribe();
    }
    else
    {
        source->Disconnect();
        instance->StatusMessage("Connection retry attempts exceeded.");
    }
}

void SubscriberInstance::HandleStatusMessage(DataSubscriber* source, const string& message)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    instance->StatusMessage(message);
}

void SubscriberInstance::HandleErrorMessage(DataSubscriber* source, const string& message)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    instance->ErrorMessage(message);
}

void SubscriberInstance::HandleDataStartTime(DataSubscriber* source, int64_t startTime)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    time_t unixSOC;
    uint16_t milliseconds;

    ToUnixTime(startTime, unixSOC, milliseconds);

    instance->DataStartTime(unixSOC, milliseconds);
    instance->DataStartTime(FromUnixTime(unixSOC, milliseconds));
}

void SubscriberInstance::HandleMetadata(DataSubscriber* source, const vector<uint8_t>& payload)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());

    // Call virtual method to handle metadata payload
    instance->ReceivedMetadata(payload);

    // When auto-parsing metadata, start subscription after successful user meta-data handling
    if (instance->m_autoParseMetadata)
        source->Subscribe();
}

void SubscriberInstance::HandleNewMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& measurements)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    instance->ReceivedNewMeasurements(measurements);
}

void SubscriberInstance::HandleConfigurationChanged(DataSubscriber* source)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());

    // Call virtual method to notify consumer that configuration has changed
    instance->ConfigurationChanged();

    // When publisher configuration has changed, request updated metadata
    instance->SendMetadataRefreshCommand();
}

void SubscriberInstance::HandleProcessingComplete(DataSubscriber* source, const string& message)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    instance->StatusMessage(message);
    instance->HistoricalReadComplete();
}

void SubscriberInstance::HandleConnectionTerminated(DataSubscriber* source)
{
    SubscriberInstance* instance = static_cast<SubscriberInstance*>(source->GetUserData());
    instance->ConnectionTerminated();
}