//******************************************************************************************************
//  AverageFrequencyCalculator.cpp - Gbtc
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
//  04/25/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include <string>
#include <vector>
#include <map>

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

// Sample application to demonstrate average frequency calculation using the subscriber API.
int main(int argc, char* argv[])
{
	std::string hostname;
	gsfts::uint16_t port;

	// Ensure that the necessary
	// command line arguments are given.
	if (argc < 3)
	{
		std::cout << "Usage:" << std::endl;
		std::cout << "    AverageFrequencyCalculator HOSTNAME PORT" << std::endl;
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

	info.FilterExpression = "FILTER ActiveMeasurements WHERE SignalType = 'FREQ'";
	info.NewMeasurementsCallback = &ProcessMeasurements;

	// Uncomment to enable optional UDP data channel
	//info.UdpDataChannel = true;
	//info.DataChannelLocalPort = 9600;

	info.IncludeTime = true;
	info.UseLocalClockAsRealTime = false;
	info.UseMillisecondResolution = true;
	
	// This controls the downsampling time, in seconds
	info.Throttled = true;
	info.LagTime = 1.0;

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
	connector.SetMaxRetries(-1);
	connector.SetRetryInterval(2000);
	connector.SetAutoReconnect(true);

	return connector;
}

// Callback which is called when the subscriber has
// received a new packet of measurements from the publisher.
void ProcessMeasurements(tst::DataSubscriber* source, std::vector<gsfts::Measurement> newMeasurements)
{
	const double LoFrequency = 57.0;
	const double HiFrequency = 62.0;
	const double HzResolution = 1000.0; // three decimal places

	const std::string TimestampFormat = "%Y-%m-%d %H:%M:%S.%f";
	const std::size_t MaxTimestampSize = 80;

	static std::map<gsfts::Guid, int> m_lastValues;

	std::map<gsfts::Guid, int>::iterator lastValueIter;
	gsfts::Measurement currentMeasurement;
	gsfts::Guid signalID;

	double frequency;
	double frequencyTotal;
	double maximumFrequency = LoFrequency;
	double minimumFrequency = HiFrequency;
	int adjustedFrequency;
	int lastValue;
	int total;

	char timestamp[MaxTimestampSize];
	std::size_t i;

	frequencyTotal = 0.0;
	total = 0;

	std::cout << Subscriber.GetTotalMeasurementsReceived() << " measurements received so far..." << std::endl;

	if (newMeasurements.size() > 0)
	{
		if (gsfts::TicksToString(timestamp, MaxTimestampSize, TimestampFormat, newMeasurements[0].Timestamp))
			std::cout << "Timestamp: " << std::string(timestamp) << std::endl;

		std::cout << "Point\tValue" << std::endl;

		for (i = 0; i < newMeasurements.size(); ++i)
			std::cout << newMeasurements[i].ID << '\t' << newMeasurements[i].Value << std::endl;

		std::cout << std::endl;

		for (i = 0; i < newMeasurements.size(); ++i)
		{
			currentMeasurement = newMeasurements[i];
			frequency = currentMeasurement.Value;
			signalID = currentMeasurement.SignalID;
			adjustedFrequency = (int)(frequency * HzResolution);

			// Do some simple flat line avoidance...
			lastValueIter = m_lastValues.find(signalID);

			if (lastValueIter != m_lastValues.end())
			{
				lastValue = lastValueIter->second;

				if (lastValue == adjustedFrequency)
					frequency = 0.0;
				else
					m_lastValues[signalID] = adjustedFrequency;
			}
			else
			{
				m_lastValues[signalID] = adjustedFrequency;
			}

			// Validate frequency
			if (frequency > LoFrequency && frequency < HiFrequency)
			{
				frequencyTotal += frequency;

				if (frequency > maximumFrequency)
					maximumFrequency = frequency;

				if (frequency < minimumFrequency)
					minimumFrequency = frequency;

				total++;
			}
		}

		if (total > 0)
		{
			std::cout << "Avg frequency: " << (frequencyTotal / total) << std::endl;
			std::cout << "Max frequency: " << maximumFrequency << std::endl;
			std::cout << "Min frequency: " << minimumFrequency << std::endl << std::endl;
		}
	}
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