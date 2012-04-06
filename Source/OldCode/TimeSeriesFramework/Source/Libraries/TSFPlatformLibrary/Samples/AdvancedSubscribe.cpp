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

#include "../Common/Measurement.h"
#include "../Transport/DataSubscriber.h"

namespace tsf = TimeSeriesFramework;
namespace tsft = tsf::Transport;

tsft::DataSubscriber Subscriber;
tsft::SubscriptionInfo Info;

// Create helper objects for subscription.
tsft::SubscriberConnector CreateSubscriberConnector(std::string hostname, uint16_t port);
tsft::SubscriptionInfo CreateSubscriptionInfo();

// Handlers for subscriber callbacks.
void Resubscribe(tsft::DataSubscriber* source);
void ProcessMeasurements(std::vector<tsf::Measurement> newMeasurements);
void DisplayStatusMessage(std::string message);
void DisplayErrorMessage(std::string message);

// Runs the subscriber.
void RunSubscriber(std::string hostname, tsf::uint16_t port);

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
	tsf::uint16_t port;

	// Ensure that the necessary
	// command line arguments are given.
	if (argc < 3)
	{
		std::cout << "Usage:" << std::endl;
		std::cout << "    SimpleSubscribe HOSTNAME PORT" << std::endl;
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
void RunSubscriber(std::string hostname, tsf::uint16_t port)
{
	// The connector is declared here because it
	// is only needed for the initial connection
	tsft::SubscriberConnector connector;

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

tsft::SubscriptionInfo CreateSubscriptionInfo()
{
	// SubscriptionInfo is a helper object which allows the user
	// to set up their subscription and reuse subscription settings.
	tsft::SubscriptionInfo info;

	// The following filter expression formats are also available:
	//
	// - Signal ID list -
	//info.FilterExpression = "74bc3271-35b5-4fe3-85ec-8cbc45ee4fb1;"
	//						"fd7f5c51-5cca-4e84-b387-bd39d812b733;"
	//						"36af1d3d-cd16-4973-afe8-dec94a24f5c4;"
	//						"532b4b4c-d360-4263-bb1b-2fda7a7ac330;"
	//						"81e3d66d-5eff-4bd6-8490-8cac3a715636;"
	//						"1fdc0182-2ef5-4a93-83fa-a6b2ce074f71;"
	//						"44d89db2-05ae-40d0-9a59-dfcbc08c464d;"
	//						"91f9adea-e595-4b3c-8b00-c126f969a31d;"
	//						"cf007929-7a25-408b-b684-9161e137d586;"
	//						"96c1118b-9e7f-4fbb-af7e-0c3623532603;"
	//						"9018180a-2ffa-4e46-94e1-6f6c9241fb31;"
	//						"c97fda6d-c9bf-4cfd-ac85-09f4c04ce1c3;"
	//						"3f172393-eb5d-43bf-8783-8a824793764c;"
	//						"9ebf2209-d6f3-4f02-8766-4c30948b5a18";
	//
	// - Filter pattern -
	//info.FilterExpression = "FILTER ActiveMeasurements WHERE ID LIKE 'PPA:*'";

	info.FilterExpression = "PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14";
	info.NewMeasurementsCallback = &ProcessMeasurements;

	info.RemotelySynchronized = false;
	info.Throttled = false;

	info.UdpDataChannel = true;
	info.DataChannelLocalPort = 9600;

	// Use 0.0.0.0 for IPv4 default interface,
	// use ::0 for IPv6 default interface, or
	// use specific IP to specify network card
	info.DataChannelInterface = "::0:0.0.0.0";

	info.IncludeTime = true;
	info.LagTime = 3.0;
	info.LeadTime = 1.0;
	info.UseLocalClockAsRealTime = false;
	info.UseMillisecondResolution = true;

	return info;
}

tsft::SubscriberConnector CreateSubscriberConnector(std::string hostname, tsf::uint16_t port)
{
	// SubscriberConnector is another helper object which allows the
	// user to modify settings for auto-reconnects and retry cycles.
	tsft::SubscriberConnector connector;

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
void ProcessMeasurements(std::vector<tsf::Measurement> newMeasurements)
{
	static int processCount = 0;
	std::size_t i;

	// Only display messages every five
	// seconds (assuming 30 calls per second).
	if (processCount % 150 == 0)
	{
		std::cout << Subscriber.GetTotalMeasurementsReceived() << " measurements received so far..." << std::endl;

		if (newMeasurements.size() > 0)
		{
			std::cout << "Timestamp: " << newMeasurements[0].Timestamp << std::endl;
			std::cout << "Point\tValue" << std::endl;

			for (i = 0; i < newMeasurements.size(); ++i)
				std::cout << newMeasurements[i].ID << '\t' << newMeasurements[i].Value << std::endl;

			std::cout << std::endl;
		}
	}

	++processCount;
}

// Callback that is called when the subscriber auto-reconnects.
void Resubscribe(tsft::DataSubscriber* source)
{
	if (source->IsConnected())
		source->Subscribe(Info);
}

// Callback which is called to display status messages from the subscriber.
void DisplayStatusMessage(std::string message)
{
	std::cout << message << std::endl << std::endl;
}

// Callback which is called to display error messages from the connector and subscriber.
void DisplayErrorMessage(std::string message)
{
	std::cerr << message << std::endl << std::endl;
}