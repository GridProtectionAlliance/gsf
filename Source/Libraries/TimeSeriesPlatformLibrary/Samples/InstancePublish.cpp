//******************************************************************************************************
//  InstancePublish.cpp - Gbtc
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
//  03/27/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include <iostream>
#include "PublisherHandler.h"
#include "../Common/Convert.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

#define TotalInstances 3
PublisherHandler* Publisher[TotalInstances];

int main(int argc, char* argv[])
{
    string hostname;
    uint16_t port;

    // Ensure that the necessary
    // command line arguments are given.
    if (argc < 2)
    {
        cout << "Usage:" << endl;
        cout << "    InstancePublish PORT" << endl;
        return 0;
    }

    // Get port.
    stringstream(argv[1]) >> port;

    // Initialize the publishers.
    for (uint32_t i = 0; i < TotalInstances; i++)
    {
        // Maintain the life-time of PublisherHandler instances within main
        PublisherHandler* publisher = new PublisherHandler("Publisher " + ToString(i + 1), port + i, false);

		// Set second publisher to only allow one connection
		if (i == 1)
			publisher->SetMaximumAllowedConnections(1);

    	publisher->Start();
		Publisher[i] = publisher;
    }

    // Wait until the user presses enter before quitting.
    string line;
    getline(cin, line);

    // Shutdown publisher instances
    for (uint32_t i = 0; i < TotalInstances; i++)
        Publisher[i]->Stop();

    // Disconnect the publisher to stop background threads.
    cout << "Disconnected." << endl;

    // Delete publisher instances
    for (uint32_t i = 0; i < TotalInstances; i++)
        delete Publisher[i];

    return 0;
}