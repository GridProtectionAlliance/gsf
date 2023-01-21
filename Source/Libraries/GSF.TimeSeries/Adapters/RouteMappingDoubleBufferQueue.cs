//******************************************************************************************************
//  RouteMappingDoubleBufferQueue.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/29/2016 - Steven E. Chisholm
//       Generated original version of source code.
//       Derived from code existing in RoutingTables.cs
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using GSF.Collections;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// The standard and default routing table that uses double buffer queues to route measurements.
    /// </summary>
    public class RouteMappingDoubleBufferQueue
        : IRouteMappingTables
    {
        // Nested Types

        private class GlobalCache
        {
            public readonly Dictionary<Guid, List<Consumer>> GlobalSignalLookup;
            public readonly Dictionary<IAdapter, Consumer> GlobalDestinationLookup;
            public readonly List<Consumer> BroadcastConsumers;
            public readonly int Version;

            public GlobalCache(Dictionary<IAdapter, Consumer> consumers, int version)
            {
                GlobalSignalLookup = new Dictionary<Guid, List<Consumer>>();
                GlobalDestinationLookup = consumers;
                BroadcastConsumers = new List<Consumer>();
                Version = version;

                // Generate routes for all signals received by each consumer adapter
                foreach (KeyValuePair<IAdapter, Consumer> kvp in consumers)
                {
                    IAdapter consumerAdapter = kvp.Key;
                    Consumer consumer = kvp.Value;

                    if (consumerAdapter.InputMeasurementKeys is not null)
                    {
                        // Create routes for each of the consumer's input signals
                        foreach (Guid signalID in consumerAdapter.InputMeasurementKeys.Select(key => key.SignalID))
                            GlobalSignalLookup.GetOrAdd(signalID, _ => new List<Consumer>()).Add(consumer);
                    }
                    else
                    {
                        // Add this consumer to the broadcast routes to begin receiving all measurements
                        BroadcastConsumers.Add(consumer);
                    }
                }

                // Broadcast consumers receive all measurements, so add them to every signal route
                foreach (List<Consumer> consumerList in GlobalSignalLookup.Values)
                {
                    consumerList.AddRange(BroadcastConsumers);
                }
            }
        }

        private class LocalCache
        {
            private readonly Dictionary<Guid, List<Producer>> m_localSignalLookup;
            private readonly Dictionary<Consumer, Producer> m_localDestinationLookup;
            private readonly RouteMappingDoubleBufferQueue m_routingTables;
            private readonly object m_localCacheLock;
            private int m_version;

            public LocalCache(RouteMappingDoubleBufferQueue routingTables, IAdapter producerAdapter)
            {
                m_localCacheLock = new object();
                m_localSignalLookup = new Dictionary<Guid, List<Producer>>();
                m_localDestinationLookup = new Dictionary<Consumer, Producer>();
                m_routingTables = routingTables;

                if (producerAdapter is IInputAdapter inputAdapter)
                    inputAdapter.NewMeasurements += Route;
                else if (producerAdapter is IActionAdapter actionAdapter)
                    actionAdapter.NewMeasurements += Route;
            }

            public void Route(object sender, EventArgs<ICollection<IMeasurement>> e)
            {
                ICollection<IMeasurement> measurements = e?.Argument;

                if (measurements is null)
                    return;

                // Get the global cache from the routing tables
                GlobalCache globalCache = Interlocked.CompareExchange(ref m_routingTables.m_globalCache, null, null);

                // Return if routes are still being calculated
                if (globalCache is null)
                    return;

                lock (m_localCacheLock)
                {
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

                    foreach (IMeasurement measurement in measurements)
                    {
                        // Attempt to look up the signal in the local cache
                        if (!m_localSignalLookup.TryGetValue(measurement.ID, out List<Producer> producers))
                        {
                            // Not in the local cache - check the global cache and fall back on broadcast consumers
                            if (!globalCache.GlobalSignalLookup.TryGetValue(measurement.ID, out List<Consumer> consumers))
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

                    // Produce measurements to consumers in the local destination
                    // cache which have measurements to be received
                    foreach (Producer producer in m_localDestinationLookup.Values.Where(producer => producer.Measurements.Count > 0))
                    {
                        producer.QueueProducer.Produce(producer.Measurements);
                        producer.Measurements.Clear();
                    }
                }
            }
        }

        private class Producer
        {
            [SuppressMessage("SonarQube.Immutability", "S3887", Justification = "Internal use only. Reference marked as read-only for future code update safety.")]
            public readonly List<IMeasurement> Measurements;
            public readonly DoubleBufferedQueueProducer<IMeasurement> QueueProducer;

            public Producer(DoubleBufferedQueueManager<IMeasurement> manager)
            {
                Measurements = new List<IMeasurement>();
                QueueProducer = manager.GetProducer();
            }
        }

        private class Consumer
        {
            public readonly IAdapter Adapter;
            public readonly DoubleBufferedQueueManager<IMeasurement> Manager;

            public Consumer(IAdapter adapter, Action<Exception> exceptionAction)
            {
                IActionAdapter actionAdapter = adapter as IActionAdapter;
                IOutputAdapter outputAdapter;

                Adapter = adapter;

                if (actionAdapter is not null)
                {
                    Manager = new DoubleBufferedQueueManager<IMeasurement>(measurements => actionAdapter.QueueMeasurementsForProcessing(new List<IMeasurement>(measurements)), exceptionAction);
                }
                else
                {
                    outputAdapter = adapter as IOutputAdapter;

                    Manager = outputAdapter is not null ? 
                        new DoubleBufferedQueueManager<IMeasurement>(measurements => outputAdapter.QueueMeasurementsForProcessing(new List<IMeasurement>(measurements)), exceptionAction) : 
                        new DoubleBufferedQueueManager<IMeasurement>(() => { });
                }
            }
        }

        private GlobalCache m_globalCache;
        private Action<string> m_onStatusMessage;
        private Action<Exception> m_onProcessException;
        private readonly LocalCache m_injectMeasurementsLocalCache;

        /// <summary>
        /// Instances a new <see cref="RouteMappingDoubleBufferQueue"/>.
        /// </summary>
        public RouteMappingDoubleBufferQueue()
        {
            m_onStatusMessage = _ => { };
            m_onProcessException = _ => { };
            m_globalCache = new GlobalCache(new Dictionary<IAdapter, Consumer>(), 0);
            m_injectMeasurementsLocalCache = new LocalCache(this, null);
        }

        /// <summary>
        /// Assigns the status messaging callbacks.
        /// </summary>
        /// <param name="onStatusMessage">Raise status messages on this callback</param>
        /// <param name="onProcessException">Raise exceptions on this callback</param>
        public void Initialize(Action<string> onStatusMessage, Action<Exception> onProcessException)
        {
            m_onStatusMessage = onStatusMessage ?? throw new ArgumentNullException(nameof(onStatusMessage));
            m_onProcessException = onProcessException ?? throw new ArgumentNullException(nameof(onProcessException));
        }

        /// <summary>
        /// Gets the number of routes in this routing table.
        /// </summary>
        public int RouteCount => m_globalCache.GlobalSignalLookup.Count;

        /// <summary>
        /// Patches the existing routing table with the supplied adapters.
        /// </summary>
        /// <param name="producerAdapters">all of the producers</param>
        /// <param name="consumerAdapters">all of the consumers</param>
        [SuppressMessage("SonarQube.UnusedObject", "S1848", Justification = "Class instantiation of \"LocalCache\" attaches event handlers for adapters.")]
        public void PatchRoutingTable(RoutingTablesAdaptersList producerAdapters, RoutingTablesAdaptersList consumerAdapters)
        {
            if (producerAdapters is null)
                throw new ArgumentNullException(nameof(producerAdapters));
            
            if (consumerAdapters is null)
                throw new ArgumentNullException(nameof(consumerAdapters));

            // Attach to NewMeasurements event of all producer adapters that are new
            // ReSharper disable once ObjectCreationAsStatement
            foreach (IAdapter producerAdapter in producerAdapters.NewAdapter)
                new LocalCache(this, producerAdapter);

            Dictionary<IAdapter, Consumer> consumerLookup = new(m_globalCache.GlobalDestinationLookup);

            // Create new consumer adapters
            foreach (IAdapter consumerAdapter in consumerAdapters.NewAdapter)
                consumerLookup.Add(consumerAdapter, new Consumer(consumerAdapter, m_onProcessException));

            // Remove old adapters
            foreach (IAdapter consumerAdapter in consumerAdapters.OldAdapter)
                consumerLookup.Remove(consumerAdapter);

            Interlocked.Exchange(ref m_globalCache, new GlobalCache(consumerLookup, m_globalCache.Version + 1));
        }

        /// <summary>
        /// This method will directly inject measurements into the routing table and use a shared local input adapter. For
        /// contention reasons, it is not recommended this be its default use case, but it is necessary at times.
        /// </summary>
        /// <param name="sender">the sender object</param>
        /// <param name="measurements">the event arguments</param>
        public void InjectMeasurements(object sender, EventArgs<ICollection<IMeasurement>> measurements)
        {
            m_injectMeasurementsLocalCache.Route(sender, measurements);
        }
    }
}