//******************************************************************************************************
//  SubscriberHandler.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  03/27/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __SUBSCRIBER_HANDLER_H
#define __SUBSCRIBER_HANDLER_H

#include "../Transport/SubscriberInstance.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

class SubscriberHandler : public SubscriberInstance
{
private:
    string m_name;

protected:
    SubscriptionInfo CreateSubscriptionInfo() override;
    void StatusMessage(const string& message) override;
    void ErrorMessage(const string& message) override;
    void DataStartTime(time_t unixSOC, uint16_t milliseconds) override;
    void ReceivedMetadata(const vector<uint8_t>& payload) override;
    void ReceivedNewMeasurements(const vector<MeasurementPtr>& measurements) override;
    void ParsedMetadata() override;
    void ConfigurationChanged() override;
    void HistoricalReadComplete() override;
    void ConnectionEstablished() override;
    void ConnectionTerminated() override;

public:
    SubscriberHandler(const string& name);
};

#endif