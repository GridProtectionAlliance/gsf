//******************************************************************************************************
//  SubscriberInstance.h - Gbtc
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

#include <iostream>
#include <string>
#include <vector>

#include "../Common/Convert.h"
#include "../Common/Measurement.h"
#include "../Transport/DataSubscriber.h"

namespace gsfts = GSF::TimeSeries;
namespace tst = gsfts::Transport;

class SubscriberInstance
{
private:
    // Function pointer types
    typedef void (*MessageCallback)(std::string);
    typedef void (*MetadataCallback)(std::vector<uint8_t>);
    typedef void (*NewMeasurementsCallback)(std::vector<gsfts::Measurement> newMeasurements);
    typedef void (*HistoricalReadCompleteCallback)();

    // Subscription members
    std::string m_hostname;
    gsfts::uint16_t m_port;
    gsfts::uint16_t m_udpPort;
    std::string m_filterExpression;
    tst::DataSubscriber m_subscriber;
    tst::SubscriptionInfo m_info;
    std::string m_startTime;
    std::string m_stopTime;
    gsfts::EndianConverter m_endianConverter;

    // Callbacks
    MessageCallback m_statusMessageCallback;
    MessageCallback m_errorMessageCallback;
    MetadataCallback m_metadataCallback;
    NewMeasurementsCallback m_newMeasurementsCallback;
    HistoricalReadCompleteCallback m_historicalReadCompleteCallback;

    // Internal subscription functions
    tst::SubscriberConnector CreateSubscriberConnector(std::string hostname, gsfts::uint16_t port);
    tst::SubscriptionInfo CreateSubscriptionInfo();
    
    // Internal subscription event handlers
    void HandleResubscribe(tst::DataSubscriber* source);
    void HandleMetadata(std::vector<uint8_t> payload);
    void HandleConfigurationChanged();
    void HandleProcessComplete(std::string message);

public:
    SubscriberInstance();
    ~SubscriberInstance();

    // Callback registration
    void RegisterStatusMessageCallback(MessageCallback statusMessageCallback);
    void RegisterErrorMessageCallback(MessageCallback errorMessageCallback);
    void RegisterMetadataCallback(MetadataCallback metadataCallback);
    void RegisterNewMeasurementsCallback(NewMeasurementsCallback processMeasurementsCallback);
    void RegisterHistoricalReadCompleteCallback(HistoricalReadCompleteCallback processingCompleteCallback);

    // Subscription functions
    void Initialize(std::string hostname, gsfts::uint16_t port, gsfts::uint16_t udpPort = 0); // Always call before connect!

    // The following are example filter expression formats:
    //
    // - Signal ID list -
    // subscriber.SetFilterExpression("7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255;"
    //						"93673c68-d59d-4926-b7e9-e7678f9f66b4;"
    //						"65ac9cf6-ae33-4ece-91b6-bb79343855d5;"
    //						"3647f729-d0ed-4f79-85ad-dae2149cd432;"
    //						"069c5e29-f78a-46f6-9dff-c92cb4f69371;"
    //						"25355a7b-2a9d-4ef2-99ba-4dd791461379");
    //
    // - Measurement key list pattern -
    // subscriber.SetFilterExpression("PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14");
    //
    // - Filter pattern -
    // subscriber.SetFilterExpression("FILTER ActiveMeasurements WHERE ID LIKE 'PPA:*'");
    // subscriber.SetFilterExpression("FILTER ActiveMeasurements WHERE Device = 'SHELBY' AND SignalType = 'FREQ'");
    void SetFilterExpression(std::string filterExpression); // Call before connect, this defaults to all measurements if not defined

    void Connect();
    void Disconnect();

    // Historical subscription functions
    void EstablishHistoricalRead(std::string startTime, std::string stopTime); // Call before connect, if needed
    void SetHistoricalReplayInterval(int32_t replayInterval);
};