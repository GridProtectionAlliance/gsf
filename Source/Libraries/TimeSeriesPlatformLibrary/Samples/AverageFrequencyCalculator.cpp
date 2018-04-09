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
#include "../Transport/DataSubscriber.h"

using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

DataSubscriber Subscriber;
SubscriptionInfo Info;

// Create helper objects for subscription.
SubscriberConnector CreateSubscriberConnector(string hostname, uint16_t port);
SubscriptionInfo CreateSubscriptionInfo();

// Handlers for subscriber callbacks.
void Resubscribe(DataSubscriber* source);
void ProcessMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& newMeasurements);
void DisplayStatusMessage(DataSubscriber* source, const string& message);
void DisplayErrorMessage(DataSubscriber* source, const string& message);

// Runs the subscriber.
void RunSubscriber(string hostname, uint16_t port);

// Sample application to demonstrate average frequency calculation using the subscriber API.
int main(int argc, char* argv[])
{
    string hostname;
    uint16_t port;

    // Ensure that the necessary
    // command line arguments are given.
    if (argc < 3)
    {
        cout << "Usage:" << endl;
        cout << "    AverageFrequencyCalculator HOSTNAME PORT" << endl;
        return 0;
    }

    // Get hostname and port.
    hostname = argv[1];
    stringstream(argv[2]) >> port;

    // Run the subscriber.
    RunSubscriber(hostname, port);

    // Wait until the user presses enter before quitting.
    string line;
    getline(cin, line);

    // Disconnect the subscriber to stop background threads.
    Subscriber.Disconnect();
    cout << "Disconnected." << endl;

    return 0;
}

// The proper procedure when creating and running a subscriber is:
//   - Create subscriber
//   - Register callbacks
//   - Connect to publisher
//   - Subscribe
void RunSubscriber(string hostname, uint16_t port)
{
    // The connector is declared here because it
    // is only needed for the initial connection
    SubscriberConnector connector;

    // Set up helper objects
    connector = CreateSubscriberConnector(hostname, port);
    Info = CreateSubscriptionInfo();

    // Register callbacks
    Subscriber.RegisterStatusMessageCallback(&DisplayStatusMessage);
    Subscriber.RegisterErrorMessageCallback(&DisplayErrorMessage);

    // Connect and subscribe to publisher
    cout << endl << "Connecting to " << hostname << ":" << port << "..." << endl << endl;

    // Connect and subscribe to publisher
    if (connector.Connect(Subscriber))
    {
        cout << "Connected! Subscribing to data..." << endl << endl;
        Subscriber.Subscribe(Info);
    }
    else
    {
        cout << "Connection attempts exceeded. Press enter to exit." << endl;
    }
}

SubscriptionInfo CreateSubscriptionInfo()
{
    // SubscriptionInfo is a helper object which allows the user
    // to set up their subscription and reuse subscription settings.
    SubscriptionInfo info;

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

SubscriberConnector CreateSubscriberConnector(string hostname, uint16_t port)
{
    // SubscriberConnector is another helper object which allows the
    // user to modify settings for auto-reconnects and retry cycles.
    SubscriberConnector connector;

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
void ProcessMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& newMeasurements)
{
    const double LoFrequency = 57.0;
    const double HiFrequency = 62.0;
    const double HzResolution = 1000.0; // three decimal places

    const string TimestampFormat = "%Y-%m-%d %H:%M:%S.%f";
    const size_t MaxTimestampSize = 80;

    static map<Guid, int> m_lastValues;

    map<Guid, int>::iterator lastValueIter;
    MeasurementPtr currentMeasurement;
    Guid signalID;

    double frequency;
    double frequencyTotal;
    double maximumFrequency = LoFrequency;
    double minimumFrequency = HiFrequency;
    int adjustedFrequency;
    int lastValue;
    int total;

    char timestamp[MaxTimestampSize];
    size_t i;

    frequencyTotal = 0.0;
    total = 0;

    cout << source->GetTotalMeasurementsReceived() << " measurements received so far..." << endl;

    if (!newMeasurements.empty())
    {
        if (TicksToString(timestamp, MaxTimestampSize, TimestampFormat, newMeasurements[0]->Timestamp))
            cout << "Timestamp: " << string(timestamp) << endl;

        cout << "Point\tValue" << endl;

        for (i = 0; i < newMeasurements.size(); ++i)
            cout << newMeasurements[i]->ID << '\t' << newMeasurements[i]->Value << endl;

        cout << endl;

        for (i = 0; i < newMeasurements.size(); ++i)
        {
            currentMeasurement = newMeasurements[i];
            frequency = currentMeasurement->Value;
            signalID = currentMeasurement->SignalID;
            adjustedFrequency = static_cast<int>(frequency * HzResolution);

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
            cout << "Avg frequency: " << (frequencyTotal / total) << endl;
            cout << "Max frequency: " << maximumFrequency << endl;
            cout << "Min frequency: " << minimumFrequency << endl << endl;
        }
    }
}

// Callback that is called when the subscriber auto-reconnects.
void Resubscribe(DataSubscriber* source)
{
    if (source->IsConnected())
    {
        cout << "Reconnected! Subscribing to data..." << endl << endl;
        source->Subscribe(Info);
    }
    else
    {
        source->Disconnect();
        cout << "Connection retry attempts exceeded. Press enter to exit." << endl;
    }
}

// Callback which is called to display status messages from the subscriber.
void DisplayStatusMessage(DataSubscriber* source, const string& message)
{
    cout << message << endl << endl;
}

// Callback which is called to display error messages from the connector and subscriber.
void DisplayErrorMessage(DataSubscriber* source, const string& message)
{
    cerr << message << endl << endl;
}