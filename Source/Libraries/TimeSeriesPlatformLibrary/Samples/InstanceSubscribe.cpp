//******************************************************************************************************
//  InstanceSubscribe.cpp - Gbtc
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
//  03/27/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>

#include "SubscriberHandler.h"

#define TotalInstances 3

SubscriberHandler* Subscriber[TotalInstances];

int main(int argc, char* argv[])
{
    string hostname;
    uint16_t port;

    // Ensure that the necessary
    // command line arguments are given.
    if (argc < 3)
    {
        cout << "Usage:" << endl;
        cout << "    InstanceSubscribe HOSTNAME PORT" << endl;
        return 0;
    }

    // Get hostname and port.
    hostname = argv[1];
    stringstream(argv[2]) >> port;

    // Initialize the subscribers.
    for (size_t i = 0; i < TotalInstances; i++)
    {
        stringstream name;
        
        name << "Subscriber" << (i + 1);

        SubscriberHandler* subscriber = new SubscriberHandler(name.str());

        subscriber->Initialize(hostname, port);

        switch (i)
        {
            case 1:
                subscriber->SetFilterExpression("FILTER TOP 5 ActiveMeasurements WHERE SignalType = 'FREQ'");
                break;
            case 2:
                subscriber->SetFilterExpression("FILTER TOP 10 ActiveMeasurements WHERE SignalType LIKE '%PHA'");
                break;
            default:
                subscriber->SetFilterExpression("FILTER TOP 10 ActiveMeasurements WHERE SignalType LIKE '%PHM'");
                break;
        }

        subscriber->Connect();

        Subscriber[i] = subscriber;
    }

    // Wait until the user presses enter before quitting.
    string line;
    getline(cin, line);

    // Shutdown subscriber instances
    for (size_t i = 0; i < TotalInstances; i++)
    {
        Subscriber[i]->Disconnect();
        delete Subscriber[i];
    }

    // Disconnect the subscriber to stop background threads.
    cout << "Disconnected." << endl;

    return 0;
}