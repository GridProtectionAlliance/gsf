//******************************************************************************************************
//  RoutingTables.cpp - Gbtc
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
//  03/28/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "RoutingTables.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

RoutingTables::RoutingTables() :
    m_activeRoutes(NewSharedPtr<RoutingTable>()),
    m_enabled(true)
{
    Thread([this]()
    {
        while (m_enabled)
        {
            m_routingTableOperations.WaitForData();
            const auto operation = m_routingTableOperations.Dequeue();
            operation.first(*this, operation.second);
        }
    });
}

RoutingTables::~RoutingTables()
{
    m_enabled = false;
    m_routingTableOperations.Release();
}

RoutingTables::RoutingTablePtr RoutingTables::CloneActiveRoutes()
{
    ReaderLock readLock(m_activeRoutesLock);
    RoutingTablePtr clonedRoutes = NewSharedPtr<RoutingTable>(*m_activeRoutes);
    return clonedRoutes;
}

void RoutingTables::SetActiveRoutes(RoutingTablePtr activeRoutes)
{
    WriterLock writeLock(m_activeRoutesLock);
    m_activeRoutes = std::move(activeRoutes);
}

void RoutingTables::UpdateRoutesOperation(RoutingTables& routingTables, const DestinationRoutes& destinationRoutes)
{
    RoutingTablePtr activeRoutes = routingTables.CloneActiveRoutes();
    const SubscriberConnectionPtr& destination = destinationRoutes.first;
    const unordered_set<GSF::Guid>& routes = destinationRoutes.second;

    // Remove subscriber connection from undesired measurement route destinations
    for (auto& pair : *activeRoutes)
    {
        if (routes.find(pair.first) != routes.end())
            pair.second->erase(destination);
    }

    // Add subscriber connection to desired measurement route destinations
    for (auto& signalID : routes)
    {
        DestinationsPtr destinations;

        if (!TryGetValue(*activeRoutes, signalID, destinations, destinations))
        {
            destinations = NewSharedPtr<Destinations>();
            activeRoutes->emplace(signalID, destinations);
        }

        destinations->insert(destination);
    }

    routingTables.SetActiveRoutes(activeRoutes);
}

void RoutingTables::RemoveRoutesOperation(RoutingTables& routingTables, const DestinationRoutes& destinationRoutes)
{
    const RoutingTablePtr activeRoutes = routingTables.CloneActiveRoutes();
    const SubscriberConnectionPtr& destination = destinationRoutes.first;

    // Remove subscriber connection from existing measurement route destinations
    for (auto& pair : *activeRoutes)
        pair.second->erase(destination);

    routingTables.SetActiveRoutes(activeRoutes);
}

void RoutingTables::UpdateRoutes(const SubscriberConnectionPtr& destination, const unordered_set<Guid>& routes)
{
    // Queue update routes operation
    m_routingTableOperations.Enqueue(RoutingTableOperation(&UpdateRoutesOperation, DestinationRoutes(destination, routes)));
}

void RoutingTables::RemoveRoutes(const SubscriberConnectionPtr& destination)
{
    // Queue remove routes operation
    m_routingTableOperations.Enqueue(RoutingTableOperation(&RemoveRoutesOperation, DestinationRoutes(destination, unordered_set<Guid>())));
}

void RoutingTables::PublishMeasurements(const vector<MeasurementPtr>& measurements)
{
    typedef vector<MeasurementPtr> Measurements;
    typedef SharedPtr<Measurements> MeasurementsPtr;
    unordered_map<SubscriberConnectionPtr, MeasurementsPtr> routedMeasurementMap;
    const size_t size = measurements.size();

    // Constrain read lock to this block
    {
        ReaderLock readLock(m_activeRoutesLock);
        const RoutingTable activeRoutes = *m_activeRoutes;

        for (auto& measurement : measurements)
        {
            DestinationsPtr destinationsPtr;

            if (TryGetValue(activeRoutes, measurement->SignalID, destinationsPtr, destinationsPtr))
            {
                const Destinations destinations = *destinationsPtr;

                for (auto& destination : destinations)
                {
                    MeasurementsPtr routedMeasurements;

                    if (!TryGetValue(routedMeasurementMap, destination, routedMeasurements, routedMeasurements))
                    {
                        routedMeasurements = NewSharedPtr<Measurements>();
                        routedMeasurements->reserve(size);
                        routedMeasurementMap.emplace(destination, routedMeasurements);
                    }

                    routedMeasurements->push_back(measurement);
                }
            }
        }
    }

    // Publish routed measurements
    for (auto& pair : routedMeasurementMap)
    {
        auto& destination = pair.first;

        if (destination->GetIsSubscribed() && !destination->GetIsTemporalSubscription())
            destination->PublishMeasurements(*pair.second);
    }
}
