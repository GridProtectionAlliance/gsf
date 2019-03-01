//******************************************************************************************************
//  GenHistory.cpp - Gbtc
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
//  03/01/2019 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "GenHistory.h"
#include "../Transport/SubscriberInstance.h"
#include <iostream>

using namespace std;
using namespace GSF;
using namespace GSF::Data;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

GenHistory::GenHistory(uint16_t port) :
    m_history(nullptr),
    m_port(port)
{
    m_subscriber = NewSharedPtr<DataSubscriber>();
    m_subscriber->SetUserData(this);
}

void GenHistory::StartArchive()
{
    m_history = NewSharedPtr<DataSet>();
    DataTablePtr history = m_history->CreateTable("History");

    history->AddColumn(history->CreateColumn("SignalID", DataType::Guid));
    history->AddColumn(history->CreateColumn("Timestamp", DataType::Int64));
    history->AddColumn(history->CreateColumn("Value", DataType::Double));

    m_history->AddOrUpdateTable(history);

    m_subscriber->RegisterNewMeasurementsCallback(&GenHistory::ProcessMeasurements);
    m_subscriber->Connect("localhost", m_port);

    SubscriptionInfo info;
    info.FilterExpression = SubscriberInstance::SubscribeAllNoStatsExpression;

    m_subscriber->Subscribe(info);
}

void GenHistory::StopArchive() const
{
    if (m_subscriber->IsConnected())
    {
        if (m_subscriber->IsSubscribed())
            m_subscriber->Unsubscribe();

        m_subscriber->Disconnect();

        if (m_history)
        {
            cout << endl << "Client disconnected, writing history dataset..." << endl;
            m_history->WriteXml("History.xml");
            cout << endl << "Dataset export complete, see \"History.xml\"." << endl;
        }
    }
}

void GenHistory::ProcessMeasurements(DataSubscriber* source, const vector<MeasurementPtr>& measurements)
{
    static const GenHistory& instance = *static_cast<const GenHistory*>(source->GetUserData());
    static DataTable& history = *instance.m_history->Table("History");
    static const int32_t signalIDColumn = history["SignalID"]->Index();
    static const int32_t timestampColumn = history["Timestamp"]->Index();
    static const int32_t valueColumn = history["Value"]->Index();

    for (const auto& measurement : measurements)
    {
        DataRowPtr row = history.CreateRow();
        
        row->SetGuidValue(signalIDColumn, measurement->SignalID);
        row->SetInt64Value(timestampColumn, measurement->Timestamp);
        row->SetDoubleValue(valueColumn, measurement->Value);

        history.AddRow(row);
    }

    if (history.RowCount() >= 400)
        Thread([]{ instance.StopArchive(); });
}
