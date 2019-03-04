//******************************************************************************************************
//  PublisherInstance.cpp - Gbtc
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
//  12/05/2018 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "PublisherInstance.h"
#include "../Common/Convert.h"
#include <iostream>

using namespace std;
using namespace pugi;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

PublisherInstance::PublisherInstance(uint16_t port, bool ipV6) :
    m_port(port),
    m_isIPV6(ipV6),
    m_initialized(false),
    m_userData(nullptr)
{
    // Reference this PublisherInstance in DataPublisher user data
    m_publisher = NewSharedPtr<DataPublisher>(port, ipV6);
    m_publisher->SetUserData(this);
}

PublisherInstance::~PublisherInstance() = default;

void PublisherInstance::HandleStatusMessage(DataPublisher* source, const string& message)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->StatusMessage(message);
}

void PublisherInstance::HandleErrorMessage(DataPublisher* source, const string& message)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->ErrorMessage(message);
}

void PublisherInstance::HandleClientConnected(DataPublisher* source, const SubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->ClientConnected(connection);
}

void PublisherInstance::HandleClientDisconnected(DataPublisher* source, const SubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->ClientDisconnected(connection);
}

void PublisherInstance::HandleProcessingIntervalChangeRequested(DataPublisher* source, const SubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->ProcessingIntervalChangeRequested(connection);
}

void PublisherInstance::HandleTemporalSubscriptionRequested(DataPublisher* source, const TemporalSubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->TemporalSubscriptionRequested(connection);
}

void PublisherInstance::HandleTemporalProcessingIntervalChangeRequested(DataPublisher* source, const TemporalSubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->TemporalProcessingIntervalChangeRequested(connection);
}

void PublisherInstance::HandleTemporalSubscriptionCanceled(DataPublisher* source, const TemporalSubscriberConnectionPtr& connection)
{
    PublisherInstance* instance = static_cast<PublisherInstance*>(source->GetUserData());
    instance->TemporalSubscriptionCanceled(connection);
}

void PublisherInstance::StatusMessage(const string& message)
{
    cout << message << endl << endl;
}

void PublisherInstance::ErrorMessage(const string& message)
{
    cerr << message << endl << endl;
}

void PublisherInstance::ClientConnected(const SubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " connected..." << endl << endl;
}

void PublisherInstance::ClientDisconnected(const SubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " disconnected..." << endl << endl;
}

void PublisherInstance::ProcessingIntervalChangeRequested(const SubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " has requested to change its temporal processing interval to " << ToString(connection->GetProcessingInterval()) << "ms" << endl << endl;
}

void PublisherInstance::TemporalSubscriptionRequested(const TemporalSubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " has requested a temporal subscription starting at " << ToString(connection->GetStartTimeConstraint()) << endl << endl;
}

void PublisherInstance::TemporalProcessingIntervalChangeRequested(const TemporalSubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " has requested to change its temporal processing interval to " << ToString(connection->GetProcessingInterval()) << "ms" << endl << endl;
}

void PublisherInstance::TemporalSubscriptionCanceled(const TemporalSubscriberConnectionPtr& connection)
{
    cout << "Client \"" << connection->GetConnectionID() << "\" with subscriber ID " << ToString(connection->GetSubscriberID()) << " has canceled the temporal subscription starting at " << ToString(connection->GetStartTimeConstraint()) << endl << endl;
}

void PublisherInstance::Initialize()
{
    // Register callbacks
    m_publisher->RegisterStatusMessageCallback(&HandleStatusMessage);
    m_publisher->RegisterErrorMessageCallback(&HandleErrorMessage);
    m_publisher->RegisterClientConnectedCallback(&HandleClientConnected);
    m_publisher->RegisterClientDisconnectedCallback(&HandleClientDisconnected);
    m_publisher->RegisterProcessingIntervalChangeRequestedCallback(&HandleProcessingIntervalChangeRequested);
    m_publisher->RegisterTemporalSubscriptionRequestedCallback(&HandleTemporalSubscriptionRequested);
    m_publisher->RegisterTemporalProcessingIntervalChangeRequestedCallback(&HandleTemporalProcessingIntervalChangeRequested);
    m_publisher->RegisterTemporalSubscriptionCanceledCallback(&HandleTemporalSubscriptionCanceled);

    m_initialized = true;
}

void PublisherInstance::DefineMetadata(const vector<DeviceMetadataPtr>& deviceMetadata, const vector<MeasurementMetadataPtr>& measurementMetadata, const vector<PhasorMetadataPtr>& phasorMetadata, int32_t versionNumber) const
{
    m_publisher->DefineMetadata(deviceMetadata, measurementMetadata, phasorMetadata, versionNumber);
}

void PublisherInstance::DefineMetadata(const DataSetPtr& metadata) const
{
    m_publisher->DefineMetadata(metadata);
}

const DataSetPtr& PublisherInstance::GetMetadata() const
{
    return m_publisher->GetMetadata();
}

const DataSetPtr& PublisherInstance::GetFilteringMetadata() const
{
    return m_publisher->GetFilteringMetadata();
}

vector<MeasurementMetadataPtr> PublisherInstance::FilterMetadata(const string& filterExpression) const
{
    return m_publisher->FilterMetadata(filterExpression);
}

void PublisherInstance::PublishMeasurements(const vector<Measurement>& measurements) const
{
    if (!m_initialized)
        throw PublisherException("Operation failed, publisher is not initialized.");

    m_publisher->PublishMeasurements(measurements);
}

void PublisherInstance::PublishMeasurements(const vector<MeasurementPtr>& measurements) const
{
    if (!m_initialized)
        throw PublisherException("Operation failed, publisher is not initialized.");

    m_publisher->PublishMeasurements(measurements);
}

const GSF::Guid& PublisherInstance::GetNodeID() const
{
    return m_publisher->GetNodeID();
}

void PublisherInstance::SetNodeID(const GSF::Guid& nodeID) const
{
    m_publisher->SetNodeID(nodeID);
}

SecurityMode PublisherInstance::GetSecurityMode() const
{
    return m_publisher->GetSecurityMode();
}

void PublisherInstance::SetSecurityMode(SecurityMode securityMode) const
{
    m_publisher->SetSecurityMode(securityMode);
}

bool PublisherInstance::IsMetadataRefreshAllowed() const
{
    return m_publisher->GetIsMetadataRefreshAllowed();
}

void PublisherInstance::SetMetadataRefreshAllowed(bool allowed) const
{
    m_publisher->SetIsMetadataRefreshAllowed(allowed);
}

bool PublisherInstance::IsNaNValueFilterAllowed() const
{
    return m_publisher->GetIsNaNValueFilterAllowed();
}

void PublisherInstance::SetNaNValueFilterAllowed(bool allowed) const
{
    m_publisher->SetNaNValueFilterAllowed(allowed);
}

bool PublisherInstance::IsNaNValueFilterForced() const
{
    return m_publisher->GetIsNaNValueFilterForced();
}

void PublisherInstance::SetNaNValueFilterForced(bool forced) const
{
    m_publisher->SetIsNaNValueFilterForced(forced);
}

uint32_t PublisherInstance::GetCipherKeyRotationPeriod() const
{
    return m_publisher->GetCipherKeyRotationPeriod();
}

void PublisherInstance::SetCipherKeyRotationPeriod(uint32_t period) const
{
    m_publisher->SetCipherKeyRotationPeriod(period);
}

uint16_t PublisherInstance::GetPort() const
{
    return m_port;
}

bool PublisherInstance::IsIPv6() const
{
    return m_isIPV6;
}

void* PublisherInstance::GetUserData() const
{
    return m_userData;
}

void PublisherInstance::SetUserData(void* userData)
{
    m_userData = userData;
}

uint64_t PublisherInstance::GetTotalCommandChannelBytesSent() const
{
    return m_publisher->GetTotalCommandChannelBytesSent();
}

uint64_t PublisherInstance::GetTotalDataChannelBytesSent() const
{
    return m_publisher->GetTotalDataChannelBytesSent();
}

uint64_t PublisherInstance::GetTotalMeasurementsSent() const
{
    return m_publisher->GetTotalMeasurementsSent();
}

bool PublisherInstance::IsInitialized() const
{
    return m_initialized;
}
