//******************************************************************************************************
//  TemporalSubscriber.h - Gbtc
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
//  03/01/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#pragma once

#include "../Common/CommonTypes.h"
#include "../Transport/SubscriberConnection.h"

using namespace GSF::TimeSeries::Transport;

class TemporalSubscriber // NOLINT
{
private:
    const TemporalSubscriberConnectionPtr m_connection;
    int64_t m_currentTimestamp;
    int32_t m_currentRow;
    int32_t m_lastRow;
    GSF::TimerPtr m_processTimer;
    bool m_stopped;

    void SendTemporalData();
    void CompleteTemporalSubscription();

    static GSF::Data::DataSetPtr s_historyDataSet;
    static GSF::Data::DataTablePtr s_history;

public:
    TemporalSubscriber(TemporalSubscriberConnectionPtr connection);
    ~TemporalSubscriber();

    void SetProcessingInterval(int32_t processingInterval) const;

    static constexpr const int64_t HistoryInterval = GSF::Ticks::PerMillisecond * 33L;
};

typedef GSF::SharedPtr<TemporalSubscriber> TemporalSubscriberPtr;
