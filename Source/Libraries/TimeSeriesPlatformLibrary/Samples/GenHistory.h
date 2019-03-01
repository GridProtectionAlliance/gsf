//******************************************************************************************************
//  GenHistory.h - Gbtc
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
#include "../Data/DataSet.h"
#include "../Transport/DataSubscriber.h"

class GenHistory // NOLINT
{
private:
    GSF::TimeSeries::Transport::DataSubscriberPtr m_subscriber;
    GSF::Data::DataSetPtr m_history;
    const uint16_t m_port;

public:
    GenHistory(uint16_t port);

    void StartArchive();
    void StopArchive() const;

    static void ProcessMeasurements(GSF::TimeSeries::Transport::DataSubscriber* source, const std::vector<GSF::TimeSeries::MeasurementPtr>& measurements);
};

typedef GSF::SharedPtr<GenHistory> GenHistoryPtr;
