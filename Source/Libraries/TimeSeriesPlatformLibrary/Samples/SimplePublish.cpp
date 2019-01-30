//******************************************************************************************************
//  SimplePublish.cpp - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  01/30/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include <string>
#include <vector>

#include "../Common/Convert.h"
#include "../Transport/DataPublisher.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

DataPublisher Subscriber;

bool RunPublisher(uint16_t port);
//void ProcessMeasurements(DataPublisher* source, const vector<MeasurementPtr>& measurements);
void DisplayStatusMessage(DataPublisher* source, const string& message);
void DisplayErrorMessage(DataPublisher* source, const string& message);

// Sample application to demonstrate the most simple use of the publisher API.
//
// This application accepts the port of the publisher via command line arguments,
// starts listening for subscriber connections, and displays information about the
// measurements it publishes. It provides fourteen measurements, i.e., PPA:1 through
// PPA:14
//
// Measurements are transmitted via the TCP command channel.
int main(int argc, char* argv[])
{
    uint16_t port;

    // Ensure that the necessary
    // command line arguments are given.
    if (argc < 2)
    {
        cout << "Usage:" << endl;
        cout << "    SimplePublish PORT" << endl;
        return 0;
    }

    // Get hostname and port.
    stringstream(argv[1]) >> port;

    // Run the subscriber.
    if (RunPublisher(port))
    {
        // Wait until the user presses enter before quitting.
        string line;
        getline(cin, line);
    }

    // Disconnect the subscriber to stop background threads.
    //Subscriber.Disconnect();
    cout << "Disconnected." << endl;

    return 0;
}

// The proper procedure when creating and running a subscriber is:
//   - Create publisher
//   - Register callbacks
//   - Start publisher to listen for subscribers
//   - Publish
bool RunPublisher(uint16_t port)
{
    // SubscriptionInfo is a helper object which allows the user
    // to set up their subscription and reuse subscription settings.
    //SubscriptionInfo info;
    //info.FilterExpression = "PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14";

    // Register callbacks
    Subscriber.RegisterStatusMessageCallback(&DisplayStatusMessage);
    Subscriber.RegisterErrorMessageCallback(&DisplayErrorMessage);
    //Subscriber.RegisterNewMeasurementsCallback(&ProcessMeasurements);

    cout << endl << "Listening on port: " << port << "..." << endl << endl;

    string errorMessage;
    bool connected = false;

    try
    {
        //Subscriber.Connect(hostname, port);
        connected = true;
    }
    catch (SubscriberException& ex)
    {
        errorMessage = ex.what();
    }
    catch (SystemError& ex)
    {
        errorMessage = ex.what();
    }
    catch (...)
    {
        errorMessage = current_exception_diagnostic_information(true);
    }

    if (connected)
    {
        cout << "Connected! Subscribing to data..." << endl << endl;
        Subscriber.Subscribe(info);
    }
    else
    {
        cerr << "Failed to connect to \"" << hostname << ":" << port << "\": " << errorMessage;
    }

    return connected;
}

// Callback which is called when the subscriber has
// received a new packet of measurements from the publisher.
void ProcessMeasurements(DataPublisher* source, const vector<MeasurementPtr>& measurements)
{
    const string TimestampFormat = "%Y-%m-%d %H:%M:%S.%f";
    const uint32_t MaxTimestampSize = 80;

    static long processCount = 0;
    static char timestamp[MaxTimestampSize];
    static const long interval = 5 * 60;
    const long measurementCount = measurements.size();
    const bool showMessage = (processCount + measurementCount >= (processCount / interval + 1) * interval);

    processCount += measurementCount;

    // Only display messages every few seconds
    if (showMessage)
    {
        stringstream message;

        message << source->GetTotalMeasurementsReceived() << " measurements received so far..." << endl;

        if (TicksToString(timestamp, MaxTimestampSize, TimestampFormat, measurements[0]->Timestamp))
            message << "Timestamp: " << string(timestamp) << endl;

        message << "Point\tValue" << endl;

        for (const auto& measurement : measurements)
            message << measurement->ID << '\t' << measurement->Value << endl;

        message << endl;

        cout << message.str();
    }
}

// Callback which is called to display status messages from the subscriber.
void DisplayStatusMessage(DataPublisher* source, const string& message)
{
    cout << message << endl << endl;
}

// Callback which is called to display error messages from the connector and subscriber.
void DisplayErrorMessage(DataPublisher* source, const string& message)
{
    cerr << message << endl << endl;
}