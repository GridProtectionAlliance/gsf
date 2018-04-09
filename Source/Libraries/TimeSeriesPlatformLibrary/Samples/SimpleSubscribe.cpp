//******************************************************************************************************
//  SimpleSubscribe.cpp - Gbtc
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
//  04/05/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include <string>
#include <vector>

#include "../Common/Convert.h"
#include "../Transport/DataSubscriber.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

DataSubscriber Subscriber;

void RunSubscriber(string hostname, uint16_t port);
void ProcessMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& newMeasurements);
void DisplayStatusMessage(DataSubscriber* source, const string& message);
void DisplayErrorMessage(DataSubscriber* source, const string& message);

// Sample application to demonstrate the most simple use of the subscriber API.
//
// This application accepts the hostname and port of the publisher via command
// line arguments, connects to the publisher, subscribes, and displays information
// about the measurements it receives. It assumes that the publisher is providing
// fourteen measurements (PPA:1 through PPA:14) and that the publisher is running
// smoothly (no reconnection attempts are made if the connection fails). 
//
// Measurements are transmitted via the TCP command channel.
int main(int argc, char* argv[])
{
    string hostname;
    uint16_t port;

    // Ensure that the necessary
    // command line arguments are given.
    if (argc < 3)
    {
        cout << "Usage:" << endl;
        cout << "    SimpleSubscribe HOSTNAME PORT" << endl;
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
    // SubscriptionInfo is a helper object which allows the user
    // to set up their subscription and reuse subscription settings.
    SubscriptionInfo info;
    info.FilterExpression = "PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14";

    // Register callbacks
    Subscriber.RegisterStatusMessageCallback(&DisplayStatusMessage);
    Subscriber.RegisterErrorMessageCallback(&DisplayErrorMessage);
    Subscriber.RegisterNewMeasurementsCallback(&ProcessMeasurements);

    cout << endl << "Connecting to " << hostname << ":" << port << "..." << endl << endl;
    Subscriber.Connect(hostname, port);

    cout << "Connected! Subscribing to data..." << endl << endl;
    Subscriber.Subscribe(info);
}

// Callback which is called when the subscriber has
// received a new packet of measurements from the publisher.
void ProcessMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& newMeasurements)
{
    const string TimestampFormat = "%Y-%m-%d %H:%M:%S.%f";
    const size_t MaxTimestampSize = 80;

    static int processCount = 0;
    size_t i;

    char timestamp[MaxTimestampSize];

    // Only display messages every five
    // seconds (assuming 30 calls per second).
    if (processCount % 150 == 0)
    {
        cout << source->GetTotalMeasurementsReceived() << " measurements received so far..." << endl;

        if (!newMeasurements.empty())
        {
            if (TicksToString(timestamp, MaxTimestampSize, TimestampFormat, newMeasurements[0]->Timestamp))
                cout << "Timestamp: " << string(timestamp) << endl;

            cout << "Point\tValue" << endl;

            for (i = 0; i < newMeasurements.size(); ++i)
                cout << newMeasurements[i]->ID << '\t' << newMeasurements[i]->Value << endl;

            cout << endl;
        }
    }

    ++processCount;
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