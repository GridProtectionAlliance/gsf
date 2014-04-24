//******************************************************************************************************
//  RoutingTableInputBuffer.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/23/2014 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Collections;
using GSF.Threading;

namespace GSF.TimeSeries.Adapters
{
    public partial class RoutingTables
    {
        private class RoutingTableInputBuffer : IDisposable
        {
            class Buffer
            {
                public List<List<IMeasurement>> MeasurementList = new List<List<IMeasurement>>();
                public List<IEnumerable<IMeasurement>> Measurement = new List<IEnumerable<IMeasurement>>();

                public void Enqueue(IEnumerable<IMeasurement> measurements)
                {
                    List<IMeasurement> list = measurements as List<IMeasurement>;
                    if (list != null)
                    {
                        MeasurementList.Add(list);
                    }
                    else
                    {
                        Measurement.Add(measurements);
                    }
                }

                public void Clear()
                {
                    MeasurementList.Clear();
                    Measurement.Clear();
                }
            }

            Buffer m_activeBuffer;
            Buffer m_processingBuffer;

            private Dictionary<Guid, List<Producer>> m_localSignalLookup;
            private Dictionary<Consumer, Producer> m_localDestinationLookup;
            private RoutingTables m_routingTables;
            private object m_localCacheLock;
            private int m_version;
            private ScheduledTask m_task;

            public RoutingTableInputBuffer(RoutingTables routingTables)
            {
                m_activeBuffer = new Buffer();
                m_processingBuffer = new Buffer();
                m_localCacheLock = new object();
                m_localSignalLookup = new Dictionary<Guid, List<Producer>>();
                m_localDestinationLookup = new Dictionary<Consumer, Producer>();
                m_routingTables = routingTables;
                m_task = new ScheduledTask(ThreadingMode.DedicatedBackground, ThreadPriority.Normal);
                m_task.Running += m_task_Running;
            }

            void m_task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
            {
                if (e.Argument == ScheduledTaskRunningReason.Disposing)
                    return;

                lock (m_localCacheLock)
                {
                    Buffer temp = m_activeBuffer;
                    m_activeBuffer = m_processingBuffer;
                    m_processingBuffer = temp;
                    m_activeBuffer.Clear();
                }

                GlobalCache globalCache;
                List<Producer> producers;
                List<Consumer> consumers;

                // Get the global cache from the routing tables
                globalCache = Interlocked.CompareExchange(ref m_routingTables.m_globalCache, null, null);

                // Return if routes are still being calculated
                if ((object)globalCache == null)
                    return;

                // Check the version of the local cache against that of the global cache.
                // We need to clear the local cache if the versions don't match because
                // that means routes have changed
                if (m_version != globalCache.Version)
                {
                    // Dump the signal lookup
                    m_localSignalLookup.Clear();

                    // Best if we hang onto producers for adapters that still have routes
                    foreach (Consumer consumer in m_localDestinationLookup.Keys.Where(consumer => !globalCache.GlobalDestinationLookup.ContainsKey(consumer.Adapter)).ToList())
                    {
                        m_localDestinationLookup[consumer].QueueProducer.Dispose();
                        m_localDestinationLookup.Remove(consumer);
                    }

                    // Update the local cache version
                    m_version = globalCache.Version;
                }

                foreach (var group in m_processingBuffer.Measurement)
                {
                    //Exact same code as below
                    foreach (IMeasurement measurement in group)
                    {
                        // Attempt to look up the signal in the local cache
                        if (!m_localSignalLookup.TryGetValue(measurement.ID, out producers))
                        {
                            // Not in the local cache - check the global cache and fall back on broadcast consumers
                            if (!globalCache.GlobalSignalLookup.TryGetValue(measurement.ID, out consumers))
                                consumers = globalCache.BroadcastConsumers;

                            // Get a producer for each of the consumers
                            producers = consumers
                                .Select(consumer => m_localDestinationLookup.GetOrAdd(consumer, c => new Producer(c.Manager)))
                                .ToList();

                            // Add this signal to the local cache
                            m_localSignalLookup.Add(measurement.ID, producers);
                        }

                        // Add this measurement to the producers' list
                        foreach (Producer producer in producers)
                            producer.Measurements.Add(measurement);
                    }
                }

                foreach (var group in m_processingBuffer.MeasurementList)
                {
                    //Exact same code as above
                    foreach (IMeasurement measurement in group)
                    {
                        // Attempt to look up the signal in the local cache
                        if (!m_localSignalLookup.TryGetValue(measurement.ID, out producers))
                        {
                            // Not in the local cache - check the global cache and fall back on broadcast consumers
                            if (!globalCache.GlobalSignalLookup.TryGetValue(measurement.ID, out consumers))
                                consumers = globalCache.BroadcastConsumers;

                            // Get a producer for each of the consumers
                            producers = consumers
                                .Select(consumer => m_localDestinationLookup.GetOrAdd(consumer, c => new Producer(c.Manager)))
                                .ToList();

                            // Add this signal to the local cache
                            m_localSignalLookup.Add(measurement.ID, producers);
                        }

                        // Add this measurement to the producers' list
                        foreach (Producer producer in producers)
                            producer.Measurements.Add(measurement);
                    }
                }

                // Produce measurements to consumers in the local destination
                // cache which have measurements to be received
                foreach (Producer producer in m_localDestinationLookup.Values)
                {
                    if (producer.Measurements.Count > 0)
                    {
                        producer.QueueProducer.Produce(producer.Measurements);
                        producer.Measurements.Clear();
                    }
                }
            }

            public void Route(IEnumerable<IMeasurement> measurements)
            {
                lock (m_localCacheLock)
                {
                    m_activeBuffer.Enqueue(measurements);
                }
                m_task.Start(1);
            }

            public void Dispose()
            {
                m_task.Dispose();
            }
        }
    }
}
