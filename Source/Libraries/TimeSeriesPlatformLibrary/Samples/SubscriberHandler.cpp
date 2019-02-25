//******************************************************************************************************
//  SubscriberHandler.cpp - Gbtc
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

#include "SubscriberHandler.h"
#include "../Common/Convert.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

Mutex SubscriberHandler::s_coutLock {};

SubscriberHandler::SubscriberHandler(string name) :
    m_name(std::move(name)),
    m_processCount(0L)
{
}

SubscriptionInfo SubscriberHandler::CreateSubscriptionInfo()
{
    SubscriptionInfo info = SubscriberInstance::CreateSubscriptionInfo();

    // TODO: Modify subscription info properties as desired...

    // To set up a remotely synchronized subscription, set this flag
    // to true and add the framesPerSecond parameter to the
    // ExtraConnectionStringParameters. Additionally, the following
    // example demonstrates the use of some other useful parameters
    // when setting up remotely synchronized subscriptions.

    //info.RemotelySynchronized = true;
    //info.ExtraConnectionStringParameters = "framesPerSecond=30;timeResolution=10000;downsamplingMethod=Closest";
    //info.LagTime = 3.0;
    //info.LeadTime = 1.0;
    //info.UseLocalClockAsRealTime = false;

    // Other example properties (see SubscriptionInfo class in DataSubscriber.h for all properties)
    //info.Throttled = false;
    //info.IncludeTime = true;
    //info.UseMillisecondResolution = true;

    return info;
}

void SubscriberHandler::SetupSubscriberConnector(SubscriberConnector& connector)
{
    SubscriberInstance::SetupSubscriberConnector(connector);

    // TODO: Modify connector properties as desired...
    //connector.SetMaxRetries(-1);
}

void SubscriberHandler::StatusMessage(const string& message)
{
    // TODO: Make sure these messages get logged to an appropriate location
    // For now, the base class just displays to console:
    stringstream status;

    status << "[" << m_name << "] " << message;

    // Calls can come from multiple threads, so we impose a simple lock before write to console
    s_coutLock.lock();
    SubscriberInstance::StatusMessage(status.str());
    s_coutLock.unlock();
}

void SubscriberHandler::ErrorMessage(const string& message)
{
    // TODO: Make sure these messages get logged to an appropriate location
    // For now, the base class just displays to console:
    stringstream status;

    status << "[" << m_name << "] " << message;

    // Calls can come from multiple threads, so we impose a simple lock before write to console
    s_coutLock.lock();
    SubscriberInstance::ErrorMessage(status.str());
    s_coutLock.unlock();
}

void SubscriberHandler::DataStartTime(time_t unixSOC, uint16_t milliseconds)
{
    // TODO: This reports timestamp, time_t format, of very first received measurement (if useful)
}

void SubscriberHandler::DataStartTime(datetime_t startTime)
{
    // TODO: This reports timestamp, ptime format, of very first received measurement (if useful)
}

void SubscriberHandler::ReceivedMetadata(const vector<uint8_t>& payload)
{
    stringstream message;
    message << "Received " << payload.size() << " bytes of metadata, parsing...";
    StatusMessage(message.str());

    SubscriberInstance::ReceivedMetadata(payload);
}

void SubscriberHandler::ParsedMetadata()
{
    StatusMessage("Metadata successfully parsed.");
}

// ReSharper disable CppDeclaratorNeverUsed
void SubscriberHandler::ReceivedNewMeasurements(const vector<MeasurementPtr>& measurements)
{
    // TODO: The following code could be used to generate frame based output, e.g., for IEEE C37.118
    // Start processing measurements
    //for (auto &measurement : measurements)
    //{
    //    // Get adjusted value
    //    const float64_t value = measurement->AdjustedValue();

    //    // Get timestamp
    //    DateTime timestamp = measurement->GetDateTime();

    //    // Handle per measurement quality flags
    //    int32_t qualityFlags = measurement->Flags;

    //    ConfigurationFramePtr configurationFrame;
    //    MeasurementMetadataPtr measurementMetadata;

    //    // Find associated configuration for measurement
    //    if (TryFindTargetConfigurationFrame(measurement->SignalID, configurationFrame))
    //    {
    //        // Lookup measurement metadata - it's faster to find metadata from within configuration frame
    //        if (TryGetMeasurementMetdataFromConfigurationFrame(measurement->SignalID, configurationFrame, measurementMetadata))
    //        {
    //            const SignalReference& reference = measurementMetadata->Reference;

    //            // reference.Acronym	<< target device acronym 
    //            // reference.Kind		<< kind of signal (see SignalKind in "Types.h"), like Frequency, Angle, etc
    //            // reference.Index    << for Phasors, Analogs and Digitals - this is the ordered "index"

    //            // TODO: Handle measurement processing here...
    //        }
    //    }
    //    else if (TryGetMeasurementMetdata(measurement->SignalID, measurementMetadata))
    //    {
    //        // Received measurement is not part of a defined configuration frame, e.g., a statistic
    //        const SignalReference& reference = measurementMetadata->Reference;
    //    }
    //}

    // TODO: *** Temporary Testing Code Below *** -- REMOVE BEFORE USE
    static const uint64_t interval = 10 * 60;
    const uint64_t measurementCount = measurements.size();
    const bool showMessage = (m_processCount + measurementCount >= (m_processCount / interval + 1) * interval);

    m_processCount += measurementCount;

    // Only display messages every few seconds
    if (showMessage)
    {
        stringstream message;

        message << GetTotalMeasurementsReceived() << " measurements received so far..." << endl;
        message << ToString(measurements[0]->GetDateTime())  << endl;        
        message << "Signal ID: " << ToString(measurements[0]->SignalID) << endl;
        message << "\tPoint\tValue" << endl;

        for (const auto& measurement : measurements)
            message << '\t' << measurement->ID << '\t' << measurement->Value << endl;

        StatusMessage(message.str());
    }
}

void SubscriberHandler::ConfigurationChanged()
{
    StatusMessage("Configuration change detected. Metadata refresh requested.");
}

void SubscriberHandler::HistoricalReadComplete()
{
    StatusMessage("Historical data read complete.");
}

void SubscriberHandler::ConnectionEstablished()
{
    StatusMessage("Connection established.");    
}

void SubscriberHandler::ConnectionTerminated()
{
    StatusMessage("Connection terminated.");
}
