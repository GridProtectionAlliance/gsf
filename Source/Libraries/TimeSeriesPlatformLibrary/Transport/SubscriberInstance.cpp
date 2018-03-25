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

SubscriberInstance::SubscriberInstance() :
    m_hostname("localhost"),
    m_port(6165),
    m_udpPort(0),
    m_filterExpression(SubscribeAllNoStatsExpression),
    m_startTime(""),
    m_stopTime("")
{
    // Reference this SubscriberInstance in DataSubsciber user data
    m_subscriber.SetUserData(this);
    m_subscriber.SetMetadataCompressed(false);
}

SubscriberInstance::~SubscriberInstance()
{
}

// public functions

void SubscriberInstance::Initialize(string hostname, uint16_t port, uint16_t udpPort)
{
    m_hostname = hostname;
    m_port = port;
    m_udpPort = udpPort;
}

void SubscriberInstance::EstablishHistoricalRead(string startTime, string stopTime)
{
    m_startTime = startTime;
    m_stopTime = stopTime;
}

void SubscriberInstance::SetFilterExpression(string filterExpression)
{
    m_filterExpression = filterExpression;

    // Resubscribe with new filter expression if already connected
    if (m_subscriber.IsSubscribed())
    {
        m_info.FilterExpression = m_filterExpression;
        m_subscriber.Subscribe(m_info);
    }
}

void SubscriberInstance::Connect()
{
    // The connector is declared here because it
    // is only needed for the initial connection
    SubscriberConnector connector;

    // Set up helper objects (derived classes can override behavior and settings)
    connector = CreateSubscriberConnector();
    m_info = CreateSubscriptionInfo();

    // Register callbacks
    m_subscriber.RegisterStatusMessageCallback(&HandleStatusMessage);
    m_subscriber.RegisterErrorMessageCallback(&HandleErrorMessage);
    m_subscriber.RegisterDataStartTimeCallback(&HandleDataStartTime);
    m_subscriber.RegisterMetadataCallback(&HandleMetadata);
    m_subscriber.RegisterNewMeasurementsCallback(&HandleNewMeasurements);
    m_subscriber.RegisterConfigurationChangedCallback(&HandleConfigurationChanged);
    m_subscriber.RegisterConnectionTerminatedCallback(&HandleConnectionTerminated);

    if (!m_startTime.empty() && !m_stopTime.empty())
    {
        m_subscriber.RegisterProcessingCompleteCallback(&HandleProcessingComplete);
        m_info.StartTime = m_startTime;
        m_info.StopTime = m_stopTime;
    }

    if (m_udpPort > 0)
    {
        m_info.UdpDataChannel = true;
        m_info.DataChannelLocalPort = m_udpPort;
    }

    // Connect and subscribe to publisher
    if (connector.Connect(m_subscriber))
    {
        ConnectionEstablished();

        // Request metadata upon successful connection, after metadata is handled
        // the SubscriberInstance will then subscribe to the desired data
        m_subscriber.SendServerCommand(ServerCommand::MetadataRefresh);
    }
    else
    {
        ErrorMessage("All connection attempts failed");
    }
}

void SubscriberInstance::Disconnect()
{
    m_subscriber.Disconnect();
}

void SubscriberInstance::SetHistoricalReplayInterval(int32_t replayInterval)
{
    if (m_subscriber.IsSubscribed())
    {
        replayInterval = m_endianConverter.ConvertBigEndian(replayInterval);
        m_subscriber.SendServerCommand(ServerCommand::UpdateProcessingInterval, (uint8_t*)&replayInterval, 0, 4);
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

bool SubscriberInstance::IsMetadataCompressed() const
{
    return m_subscriber.IsMetadataCompressed();
}

void SubscriberInstance::SetMetadataCompressed(bool compressed)
{
    m_subscriber.SetMetadataCompressed(compressed);
}

long SubscriberInstance::GetTotalCommandChannelBytesReceived() const
{
    return m_subscriber.GetTotalCommandChannelBytesReceived();
}

long SubscriberInstance::GetTotalDataChannelBytesReceived() const
{
    return m_subscriber.GetTotalDataChannelBytesReceived();
}

long SubscriberInstance::GetTotalMeasurementsReceived() const
{
    return m_subscriber.GetTotalMeasurementsReceived();
}

bool SubscriberInstance::IsConnected() const
{
    return m_subscriber.IsConnected();
}

bool SubscriberInstance::IsSubscribed() const
{
    return m_subscriber.IsSubscribed();
}

// protected functions

// All the following protected functions are virtual so that derived
// classes can customize behavior of the SubscriberInstance

SubscriberConnector SubscriberInstance::CreateSubscriberConnector()
{
    // SubscriberConnector is another helper object which allows the
    // user to modify settings for auto-reconnects and retry cycles.
    SubscriberConnector connector;

    // Register callbacks
    connector.RegisterErrorMessageCallback(&HandleErrorMessage);
    connector.RegisterReconnectCallback(&HandleResubscribe);

    connector.SetHostname(m_hostname);
    connector.SetPort(m_port);
    connector.SetMaxRetries(-1);
    connector.SetRetryInterval(5000);
    connector.SetAutoReconnect(true);

    return connector;
}

SubscriptionInfo SubscriberInstance::CreateSubscriptionInfo()
{
    // SubscriptionInfo is a helper object which allows the user
    // to set up their subscription and reuse subscription settings.
    SubscriptionInfo info;

    // Define desired filter expression
    info.FilterExpression = m_filterExpression;

    // To set up a remotely synchronized subscription, set this flag
    // to true and add the framesPerSecond parameter to the
    // ExtraConnectionStringParameters. Additionally, the following
    // example demonstrates the use of some other useful parameters
    // when setting up remotely synchronized subscriptions.
    //
    //info.RemotelySynchronized = true;
    //info.ExtraConnectionStringParameters = "framesPerSecond=30;timeResolution=10000;downsamplingMethod=Closest";

    info.RemotelySynchronized = false;
    info.Throttled = false;

    info.UdpDataChannel = false;

    info.IncludeTime = true;
    info.LagTime = 3.0;
    info.LeadTime = 1.0;
    info.UseLocalClockAsRealTime = false;
    info.UseMillisecondResolution = true;

    return info;
}

void SubscriberInstance::StatusMessage(string message)
{
    cout << message << endl << endl;
}

void SubscriberInstance::ErrorMessage(string message)
{
    cerr << message << endl << endl;
}

void SubscriberInstance::DataStartTime(time_t unixSOC, int milliseconds)
{
}

void SubscriberInstance::ReceivedMetadata(vector<uint8_t> payload)
{
}

void SubscriberInstance::ReceivedNewMeasurements(vector<Measurement> measurements)
{
}

void SubscriberInstance::ConfigurationChanged()
{
    StatusMessage("Configuration changed");
}

void SubscriberInstance::HistoricalReadComplete()
{
    StatusMessage("Historical read complete");
}

void SubscriberInstance::ConnectionEstablished()
{
    StatusMessage("Connection established");
}

void SubscriberInstance::ConnectionTerminated()
{
    StatusMessage("Connection terminated");
}

// private functions

// The following member methods are defined as static so they
// can be used as callback registrations for DataSubscriber

void SubscriberInstance::HandleResubscribe(DataSubscriber* source)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();

    if (source->IsConnected())
    {
        instance->ConnectionEstablished();
        source->Subscribe(instance->m_info);
    }
}

void SubscriberInstance::HandleStatusMessage(DataSubscriber* source, string message)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    instance->StatusMessage(message);
}

void SubscriberInstance::HandleErrorMessage(DataSubscriber* source, string message)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    instance->ErrorMessage(message);
}

void SubscriberInstance::HandleDataStartTime(DataSubscriber* source, int64_t startTime)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    time_t unixSOC;
    int16_t milliseconds;
    
    GetUnixTime(startTime, unixSOC, milliseconds);
    
    instance->DataStartTime(unixSOC, milliseconds);
}

void SubscriberInstance::HandleMetadata(DataSubscriber* source, vector<uint8_t> payload)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    
    // Call virtual method to handle metadata payload
    instance->ReceivedMetadata(payload);

    // Start subscription after successful user meta-data handling
    source->Subscribe(instance->m_info);
}

void SubscriberInstance::HandleNewMeasurements(DataSubscriber* source, vector<Measurement> measurements)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    instance->ReceivedNewMeasurements(measurements);
}

void SubscriberInstance::HandleConfigurationChanged(DataSubscriber* source)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();

    // Call virtual method to notify consumer that configuration has changed
    instance->ConfigurationChanged();

    // When publisher configuration has changed, request updated metadata
    source->SendServerCommand(ServerCommand::MetadataRefresh);
}

void SubscriberInstance::HandleProcessingComplete(DataSubscriber* source, string message)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();   
    instance->StatusMessage(message);
    instance->HistoricalReadComplete();
}

void SubscriberInstance::HandleConnectionTerminated(DataSubscriber* source)
{
    SubscriberInstance* instance = (SubscriberInstance*)source->GetUserData();
    instance->ConnectionTerminated();
}