//******************************************************************************************************
//  TemporalSubscriberConnection.cpp - Gbtc
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
//  03/04/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "TemporalSubscriberConnection.h"
#include "SubscriberConnection.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

TemporalSubscriberConnection::TemporalSubscriberConnection(SubscriberConnectionPtr connection) :
    m_connection(std::move(connection)),
    m_stopped(false)
{
}

TemporalSubscriberConnection::~TemporalSubscriberConnection()
{
    CompleteTemporalSubscription();
}

const GSF::Guid& TemporalSubscriberConnection::GetSubscriberID() const
{
    return m_connection->GetSubscriberID();
}

const GSF::Guid& TemporalSubscriberConnection::GetInstanceID() const
{
    return m_connection->GetInstanceID();
}

const string& TemporalSubscriberConnection::GetConnectionID() const
{
    return m_connection->GetConnectionID();
}

const GSF::IPAddress& TemporalSubscriberConnection::GetIPAddress() const
{
    return m_connection->GetIPAddress();
}

const string& TemporalSubscriberConnection::GetHostName() const
{
    return m_connection->GetHostName();
}

TemporalSubscriberConnectionPtr TemporalSubscriberConnection::GetReference()
{
    return shared_from_this();
}

int64_t TemporalSubscriberConnection::GetStartTicks() const
{
    return ToTicks(m_connection->GetStartTimeConstraint());
}

GSF::datetime_t TemporalSubscriberConnection::GetStartTimeConstraint() const
{
    return m_connection->GetStartTimeConstraint();
}

int64_t TemporalSubscriberConnection::GetStopTicks() const
{
    return ToTicks(m_connection->GetStopTimeConstraint());
}

GSF::datetime_t TemporalSubscriberConnection::GetStopTimeConstraint() const
{
    return m_connection->GetStopTimeConstraint();
}

int32_t TemporalSubscriberConnection::GetProcessingInterval() const
{
    return m_connection->GetProcessingInterval();
}

void TemporalSubscriberConnection::PublishMeasurements(const vector<Measurement>& measurements) const
{
    vector<MeasurementPtr> measurementPtrs;

    measurementPtrs.reserve(measurements.size());

    for (const auto& measurement : measurements)
        measurementPtrs.push_back(ToPtr(measurement));

    PublishMeasurements(measurementPtrs);
}

void TemporalSubscriberConnection::PublishMeasurements(const vector<MeasurementPtr>& measurements) const
{
    m_connection->PublishMeasurements(measurements);
}

void TemporalSubscriberConnection::CompleteTemporalSubscription()
{
    if (m_stopped)
        return;

    m_stopped = true;
    m_connection->CompleteTemporalSubscription();
}
