//******************************************************************************************************
//  TemporalSubscriberConnection.h - Gbtc
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

#ifndef _TEMPORAL_SUBSCRIBER_INSTANCE
#define _TEMPORAL_SUBSCRIBER_INSTANCE

#include "../Common/CommonTypes.h"
#include "CompactMeasurement.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class SubscriberConnection;
    typedef GSF::SharedPtr<SubscriberConnection> SubscriberConnectionPtr;

    class TemporalSubscriberConnection;
    typedef GSF::SharedPtr<TemporalSubscriberConnection> TemporalSubscriberConnectionPtr;

    class TemporalSubscriberConnection : public GSF::EnableSharedThisPtr<TemporalSubscriberConnection> // NOLINT
    {
    private:
        const GSF::TimeSeries::Transport::SubscriberConnectionPtr m_connection;
        bool m_stopped;

    public:
        TemporalSubscriberConnection(GSF::TimeSeries::Transport::SubscriberConnectionPtr connection);
        virtual ~TemporalSubscriberConnection();

        const GSF::Guid& GetSubscriberID() const;
        const GSF::Guid& GetInstanceID() const;
        const std::string& GetConnectionID() const;
        const GSF::IPAddress& GetIPAddress() const;
        const std::string& GetHostName() const;

        TemporalSubscriberConnectionPtr GetReference();

        int64_t GetStartTicks() const;
        GSF::datetime_t GetStartTimeConstraint() const;

        int64_t GetStopTicks() const;
        GSF::datetime_t GetStopTimeConstraint() const;

        int32_t GetProcessingInterval() const;

        void PublishMeasurements(const std::vector<GSF::TimeSeries::Measurement>& measurements) const;
        void PublishMeasurements(const std::vector<GSF::TimeSeries::MeasurementPtr>& measurements) const;

        void CompleteTemporalSubscription();

        friend class GSF::TimeSeries::Transport::SubscriberConnection;
    };

    typedef GSF::SharedPtr<TemporalSubscriberConnection> TemporalSubscriberConnectionPtr;
}}}

#endif