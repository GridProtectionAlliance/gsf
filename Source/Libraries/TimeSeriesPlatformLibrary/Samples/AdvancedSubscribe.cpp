//******************************************************************************************************
//  AdvancedSubscribe.cpp - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/06/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include <string>
#include <vector>

#include "../Common/Convert.h"
#include "../Common/Measurement.h"
#include "../Transport/DataSubscriber.h"

namespace gsfts = GSF::TimeSeries;
namespace tst = gsfts::Transport;

tst::DataSubscriber Subscriber;
tst::SubscriptionInfo Info;

// Create helper objects for subscription.
tst::SubscriberConnector CreateSubscriberConnector(std::string hostname, uint16_t port);
tst::SubscriptionInfo CreateSubscriptionInfo();

// Handlers for subscriber callbacks.
void Resubscribe(tst::DataSubscriber* source);
void ProcessMeasurements(tst::DataSubscriber* source, std::vector<gsfts::Measurement> newMeasurements);
void DisplayStatusMessage(tst::DataSubscriber* source, std::string message);
void DisplayErrorMessage(tst::DataSubscriber* source, std::string message);

// Runs the subscriber.
void RunSubscriber(std::string hostname, gsfts::uint16_t port);

// Sample application to demonstrate the more advanced use of the subscriber API.
//
// This application accepts the hostname and port of the publisher via command
// line arguments, connects to the publisher, subscribes, and displays information
// about the measurements it receives. It assumes that the publisher is providing
// fourteen measurements (PPA:1 through PPA:14) and will make a maximum of five
// connection attempts before giving up. It will also auto-reconnect if the connection
// is terminated.
//
// Measurements are transmitted via a separate UDP data channel.
int main(int argc, char* argv[])
{
	std::string hostname;
	gsfts::uint16_t port;

	// Ensure that the necessary
	// command line arguments are given.
	if (argc < 3)
	{
		std::cout << "Usage:" << std::endl;
		std::cout << "    AdvancedSubscribe HOSTNAME PORT" << std::endl;
		return 0;
	}

	// Get hostname and port.
	hostname = argv[1];
	std::stringstream(argv[2]) >> port;

	// Run the subscriber.
	RunSubscriber(hostname, port);

	// Wait until the user presses enter before quitting.
	std::string line;
	std::getline(std::cin, line);
	
	// Disconnect the subscriber to stop background threads.
	Subscriber.Disconnect();

	return 0;
}

// The proper procedure when creating and running a subscriber is:
//   - Create subscriber
//   - Register callbacks
//   - Connect to publisher
//   - Subscribe
void RunSubscriber(std::string hostname, gsfts::uint16_t port)
{
	// The connector is declared here because it
	// is only needed for the initial connection
	tst::SubscriberConnector connector;

	// Set up helper objects
	connector = CreateSubscriberConnector(hostname, port);
	Info = CreateSubscriptionInfo();

	// Register callbacks
	Subscriber.RegisterStatusMessageCallback(&DisplayStatusMessage);
	Subscriber.RegisterErrorMessageCallback(&DisplayErrorMessage);

	// Connect and subscribe to publisher
	if (connector.Connect(Subscriber))
		Subscriber.Subscribe(Info);
	else
		std::cerr << "All connection attempts failed" << std::endl;
}

tst::SubscriptionInfo CreateSubscriptionInfo()
{
	// SubscriptionInfo is a helper object which allows the user
	// to set up their subscription and reuse subscription settings.
	tst::SubscriptionInfo info;

	// The following filter expression formats are also available:
	//
	// - Signal ID list -
	//info.FilterExpression = "7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255;"
	//						"93673c68-d59d-4926-b7e9-e7678f9f66b4;"
	//						"65ac9cf6-ae33-4ece-91b6-bb79343855d5;"
	//						"3647f729-d0ed-4f79-85ad-dae2149cd432;"
	//						"069c5e29-f78a-46f6-9dff-c92cb4f69371;"
	//						"25355a7b-2a9d-4ef2-99ba-4dd791461379";
	//
	// - Filter pattern -
	//info.FilterExpression = "FILTER ActiveMeasurements WHERE ID LIKE 'PPA:*'";
	//info.FilterExpression = "FILTER ActiveMeasurements WHERE Device = 'SHELBY' AND SignalType = 'FREQ'";

	info.FilterExpression = "PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14";
	info.NewMeasurementsCallback = &ProcessMeasurements;

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

	info.UdpDataChannel = true;
	info.DataChannelLocalPort = 9600;

	info.IncludeTime = true;
	info.LagTime = 3.0;
	info.LeadTime = 1.0;
	info.UseLocalClockAsRealTime = false;
	info.UseMillisecondResolution = true;

	return info;
}

tst::SubscriberConnector CreateSubscriberConnector(std::string hostname, gsfts::uint16_t port)
{
	// SubscriberConnector is another helper object which allows the
	// user to modify settings for auto-reconnects and retry cycles.
	tst::SubscriberConnector connector;

	connector.RegisterErrorMessageCallback(&DisplayErrorMessage);
	connector.RegisterReconnectCallback(&Resubscribe);

	connector.SetHostname(hostname);
	connector.SetPort(port);
	connector.SetMaxRetries(5);
	connector.SetRetryInterval(1000);
	connector.SetAutoReconnect(true);

	return connector;
}

// Callback which is called when the subscriber has
// received a new packet of measurements from the publisher.
void ProcessMeasurements(tst::DataSubscriber* source, std::vector<gsfts::Measurement> newMeasurements)
{
	const std::string TimestampFormat = "%Y-%m-%d %H:%M:%S.%f";
	const std::size_t MaxTimestampSize = 80;

	static int processCount = 0;
	std::size_t i;

	char timestamp[MaxTimestampSize];

	// Only display messages every five
	// seconds (assuming 30 calls per second).
	if (processCount % 150 == 0)
	{
		std::cout << Subscriber.GetTotalMeasurementsReceived() << " measurements received so far..." << std::endl;

		if (newMeasurements.size() > 0)
		{
			if (gsfts::TicksToString(timestamp, MaxTimestampSize, TimestampFormat, newMeasurements[0].Timestamp))
				std::cout << "Timestamp: " << std::string(timestamp) << std::endl;

			std::cout << "Point\tValue" << std::endl;

			for (i = 0; i < newMeasurements.size(); ++i)
				std::cout << newMeasurements[i].ID << '\t' << newMeasurements[i].Value << std::endl;

			std::cout << std::endl;
		}
	}

	++processCount;
}

// Callback that is called when the subscriber auto-reconnects.
void Resubscribe(tst::DataSubscriber* source)
{
	if (source->IsConnected())
		source->Subscribe(Info);
}

// Callback which is called to display status messages from the subscriber.
void DisplayStatusMessage(tst::DataSubscriber* source, std::string message)
{
	std::cout << message << std::endl << std::endl;
}

// Callback which is called to display error messages from the connector and subscriber.
void DisplayErrorMessage(tst::DataSubscriber* source, std::string message)
{
	std::cerr << message << std::endl << std::endl;
}