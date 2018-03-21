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
#include "../Transport/Constants.h"

SubscriberInstance::SubscriberInstance() :
	m_hostname("localhost"),
	m_port(6165),
	m_udpPort(0),
	m_filterExpression("FILTER ActiveMeasurements WHERE Protocol = 'GatewayTransport'"),
	m_startTime(""),
	m_stopTime(""),
	m_statusMessageCallback(0),
	m_errorMessageCallback(0),
	m_metadataCallback(0),
	m_newMeasurementsCallback(0),
	m_historicalReadCompleteCallback(0)
{
}

SubscriberInstance::~SubscriberInstance()
{
}

// public functions

void SubscriberInstance::RegisterStatusMessageCallback(MessageCallback statusMessageCallback)
{
	m_statusMessageCallback = statusMessageCallback;
}

void SubscriberInstance::RegisterErrorMessageCallback(MessageCallback errorMessageCallback)
{
	m_errorMessageCallback = errorMessageCallback;
}

void SubscriberInstance::RegisterMetadataCallback(MetadataCallback metadataCallback)
{
	m_metadataCallback = metadataCallback;
}

void SubscriberInstance::RegisterNewMeasurementsCallback(NewMeasurementsCallback processMeasurementsCallback)
{
	m_newMeasurementsCallback = processMeasurementsCallback;
}

void SubscriberInstance::Initialize(std::string hostname, gsfts::uint16_t port, gsfts::uint16_t udpPort)
{
	m_hostname = hostname;
	m_port = port;
	m_udpPort = udpPort;
}

void SubscriberInstance::EstablishHistoricalRead(std::string startTime, std::string stopTime)
{
	m_startTime = startTime;
	m_stopTime = stopTime;
}

void SubscriberInstance::Connect()
{
	// The connector is declared here because it
	// is only needed for the initial connection
	tst::SubscriberConnector connector;

	// Set up helper objects
	connector = CreateSubscriberConnector(m_hostname, m_port);
	m_info = CreateSubscriptionInfo();

	// Register callbacks
	if (m_statusMessageCallback)
		m_subscriber.RegisterStatusMessageCallback(m_statusMessageCallback);

	if (m_errorMessageCallback)
		m_subscriber.RegisterErrorMessageCallback(m_errorMessageCallback);

	if (!m_startTime.empty() && !m_stopTime.empty())
	{
		m_subscriber.RegisterProcessingCompleteCallback(this->HandleProcessComplete);
		m_info.StartTime = m_startTime;
		m_info.StopTime = m_stopTime;
	}

	if (m_udpPort > 0)
	{
		m_info.UdpDataChannel = true;
		m_info.DataChannelLocalPort = m_udpPort;
	}

	m_subscriber.RegisterMetadataCallback(this->HandleMetadata);
	m_subscriber.RegisterConfigurationChangedCallback(this->HandleConfigurationChanged);

	// Connect and subscribe to publisher
	if (connector.Connect(m_subscriber))
	{
		// Request metadata upon successful connection, after metadata is handled
		// the SubscriberInstance will then subscribe to the desired data
		m_subscriber.SendServerCommand(tst::ServerCommand::MetadataRefresh);
	}
	else
	{
		if (m_errorMessageCallback)
			m_errorMessageCallback("All connection attempts failed");
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
		m_subscriber.SendServerCommand(tst::ServerCommand::UpdateProcessingInterval, (gsfts::uint8_t*)&replayInterval, 0, 4);
	}
}

// private functions

tst::SubscriberConnector SubscriberInstance::CreateSubscriberConnector(std::string hostname, gsfts::uint16_t port)
{
	// SubscriberConnector is another helper object which allows the
	// user to modify settings for auto-reconnects and retry cycles.
	tst::SubscriberConnector connector;

	if (m_errorMessageCallback)
		connector.RegisterErrorMessageCallback(m_errorMessageCallback);

	connector.RegisterReconnectCallback(this->HandleResubscribe);

	connector.SetHostname(hostname);
	connector.SetPort(port);
	connector.SetMaxRetries(5);
	connector.SetRetryInterval(1000);
	connector.SetAutoReconnect(true);

	return connector;
}

tst::SubscriptionInfo SubscriberInstance::CreateSubscriptionInfo()
{
	// SubscriptionInfo is a helper object which allows the user
	// to set up their subscription and reuse subscription settings.
	tst::SubscriptionInfo info;

	// Define desired filter expression
	info.FilterExpression = m_filterExpression;

	// Establish user new measurements call back (if defined)
	if (m_newMeasurementsCallback)
		info.NewMeasurementsCallback = m_newMeasurementsCallback;

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

// Callback that is called when the subscriber auto-reconnects
void SubscriberInstance::HandleResubscribe(tst::DataSubscriber* source)
{
	if (source->IsConnected())
		source->Subscribe(m_info);
}

void SubscriberInstance::HandleMetadata(std::vector<uint8_t> payload)
{
	// Call user defined handle meta-data function
	if (m_metadataCallback)
		m_metadataCallback(payload);

	// Start subscription after successful meta-data parse
	m_subscriber.Subscribe(m_info);
}

void SubscriberInstance::HandleConfigurationChanged()
{
	// When publisher configuration has changed, request updated metadata
	m_subscriber.SendServerCommand(tst::ServerCommand::MetadataRefresh);
}

void SubscriberInstance::HandleProcessComplete(std::string message)
{
	if (m_statusMessageCallback)
		m_statusMessageCallback(message);

	if (m_historicalReadCompleteCallback)
		m_historicalReadCompleteCallback();
}