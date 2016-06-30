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
using System.Linq;
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

        private class LocalCache
        {
            private Dictionary<Guid, List<Producer>> m_localSignalLookup;
            private Dictionary<Consumer, Producer> m_localDestinationLookup;
            private RouteMappingDoubleBufferQueue m_routingTables;
            private object m_localCacheLock;
            private int m_version;

            public LocalCache(RouteMappingDoubleBufferQueue routingTables)
            {
                m_localCacheLock = new object();
                m_localSignalLookup = new Dictionary<Guid, List<Producer>>();
                m_localDestinationLookup = new Dictionary<Consumer, Producer>();
                m_routingTables = routingTables;
            }

            public void Route(IEnumerable<IMeasurement> measurements)
            {
                RouteMappingDoubleBufferQueue globalCache;
                List<Producer> producers;
                List<Consumer> consumers;

                // Get the global cache from the routing tables
                globalCache = m_routingTables.m_getCurrentRouteMappingTables() as RouteMappingDoubleBufferQueue;

                // Return if routes are still being calculated
                if ((object)globalCache == null)
                    return;

                lock (m_localCacheLock)
                {
                    // Check the version of the local cache against that of the global cache.
                    // We need to clear the local cache if the versions don't match because
                    // that means routes have changed
                    if (m_version != globalCache.m_version)
                    {
                        // Dump the signal lookup
                        m_localSignalLookup.Clear();

                        // Best if we hang onto producers for adapters that still have routes
                        foreach (Consumer consumer in m_localDestinationLookup.Keys.Where(consumer => !globalCache.m_globalDestinationLookup.ContainsKey(consumer.Adapter)).ToList())
                        {
                            m_localDestinationLookup[consumer].QueueProducer.Dispose();
                            m_localDestinationLookup.Remove(consumer);
                        }

                        // Update the local cache version
                        m_version = globalCache.m_version;
                    }

                    foreach (IMeasurement measurement in measurements)
                    {
                        // Attempt to look up the signal in the local cache
                        if (!m_localSignalLookup.TryGetValue(measurement.ID, out producers))
                        {
                            // Not in the local cache - check the global cache and fall back on broadcast consumers
                            if (!globalCache.m_globalSignalLookup.TryGetValue(measurement.ID, out consumers))
                                consumers = globalCache.m_broadcastConsumers;

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
                    foreach (Producer producer in m_localDestinationLookup.Values)
                    {
                        if (producer.Measurements.Count > 0)
                        {
                            producer.QueueProducer.Produce(producer.Measurements);
                            producer.Measurements.Clear();
                        }
                    }
                }
            }
        }

        private class Producer
        {
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

                if ((object)actionAdapter != null)
                {
                    Manager = new DoubleBufferedQueueManager<IMeasurement>(measurements => actionAdapter.QueueMeasurementsForProcessing(new List<IMeasurement>(measurements)), exceptionAction);
                }
                else
                {
                    outputAdapter = adapter as IOutputAdapter;

                    if ((object)outputAdapter != null)
                        Manager = new DoubleBufferedQueueManager<IMeasurement>(measurements => outputAdapter.QueueMeasurementsForProcessing(new List<IMeasurement>(measurements)), exceptionAction);
                    else
                        Manager = new DoubleBufferedQueueManager<IMeasurement>(() => { });
                }
            }
        }

        // Fields
        private readonly Dictionary<Guid, List<Consumer>> m_globalSignalLookup;
        private readonly Dictionary<IAdapter, Consumer> m_globalDestinationLookup;
        private readonly List<Consumer> m_broadcastConsumers;
        private readonly int m_version;

        private readonly Action<string> m_onStatusMessage;
        private readonly Action<Exception> m_onProcessException;
        private readonly Func<IRouteMappingTables> m_getCurrentRouteMappingTables;

        private RouteMappingDoubleBufferQueue(int version)
        {
            m_globalSignalLookup = new Dictionary<Guid, List<Consumer>>();
            m_globalDestinationLookup = new Dictionary<IAdapter, Consumer>();
            m_broadcastConsumers = new List<Consumer>();
            m_version = version;
        }

        /// <summary>
        /// Instances a new <see cref="RouteMappingDoubleBufferQueue"/>.
        /// </summary>
        /// <param name="getCurrentRouteMappingTables">A callback to get the most latest implementation of the Routing Map.</param>
        /// <param name="onStatusMessage">Raise status messages on this callback</param>
        /// <param name="onProcessException">Raise exceptions on this callback</param>
        public RouteMappingDoubleBufferQueue(Func<IRouteMappingTables> getCurrentRouteMappingTables, Action<string> onStatusMessage, Action<Exception> onProcessException)
            : this(0)
        {
            if (getCurrentRouteMappingTables == null)
                throw new ArgumentNullException(nameof(getCurrentRouteMappingTables));
            if (onStatusMessage == null)
                throw new ArgumentNullException(nameof(onStatusMessage));
            if (onProcessException == null)
                throw new ArgumentNullException(nameof(onProcessException));

            m_getCurrentRouteMappingTables = getCurrentRouteMappingTables;
            m_onStatusMessage = onStatusMessage;
            m_onProcessException = onProcessException;
        }

        private RouteMappingDoubleBufferQueue(RouteMappingDoubleBufferQueue previousMapping, RoutingTablesAdaptersList producerAdapters, RoutingTablesAdaptersList consumerAdapters)
            : this(previousMapping.m_version + 1)
        {
            m_getCurrentRouteMappingTables = previousMapping.m_getCurrentRouteMappingTables;
            m_onStatusMessage = previousMapping.m_onStatusMessage;
            m_onProcessException = previousMapping.m_onProcessException;

            // Attach to NewMeasurements event of all producer adapters that are new
            foreach (IAdapter producerAdapter in producerAdapters.NewAdapter)
            {
                IInputAdapter inputAdapter = producerAdapter as IInputAdapter;

                if ((object)inputAdapter != null)
                    inputAdapter.NewMeasurements += GetRoutedMeasurementsHandler();
                else
                    ((IActionAdapter)producerAdapter).NewMeasurements += GetRoutedMeasurementsHandler();
            }

            // Copy existing consumer adapters.
            foreach (IAdapter consumerAdapter in consumerAdapters.ExistingAdapter)
            {
                m_globalDestinationLookup.Add(consumerAdapter, previousMapping.m_globalDestinationLookup[consumerAdapter]);
            }

            // Create new consumer adapters
            foreach (IAdapter consumerAdapter in consumerAdapters.NewAdapter)
            {
                // Add this adapter to the global destinations lookup
                m_globalDestinationLookup.Add(consumerAdapter, new Consumer(consumerAdapter, m_onProcessException));
            }

            // Generate routes for all signals received by each consumer adapter
            foreach (KeyValuePair<IAdapter, Consumer> kvp in m_globalDestinationLookup)
            {
                IAdapter consumerAdapter = kvp.Key;
                Consumer consumer = kvp.Value;

                if ((object)consumerAdapter.InputMeasurementKeys != null)
                {
                    // Create routes for each of the consumer's input signals
                    foreach (Guid signalID in consumerAdapter.InputMeasurementKeys.Select(key => key.SignalID))
                        m_globalSignalLookup.GetOrAdd(signalID, id => new List<Consumer>()).Add(consumer);
                }
                else
                {
                    // Add this consumer to the broadcast routes to begin receiving all measurements
                    m_broadcastConsumers.Add(consumer);
                }
            }

            // Broadcast consumers receive all measurements, so add them to every signal route
            foreach (List<Consumer> consumerList in m_globalSignalLookup.Values)
                consumerList.AddRange(m_broadcastConsumers);

        }

        /// <summary>
        /// Gets the number of routes in this routing table.
        /// </summary>
        public int RouteCount => m_globalSignalLookup.Count;

        /// <summary>
        /// Calculates new routes for the supplied list of producers and consumers.
        /// This new mapping table will replace the existing one when complete.
        /// </summary>
        /// <param name="previousMapping">the most recent mapping table that will get replaced by this one.</param>
        /// <param name="producerAdapters">all of the producers</param>
        /// <param name="consumerAdapters">all of the consumers</param>
        /// <returns>The new mapping table that will replace the old one.</returns>
        public IRouteMappingTables CalculateNewRoutingTable(IRouteMappingTables previousMapping, RoutingTablesAdaptersList producerAdapters, RoutingTablesAdaptersList consumerAdapters)
        {
            var prevMapping = previousMapping as RouteMappingDoubleBufferQueue;
            if (previousMapping == null)
                throw new ArgumentNullException(nameof(previousMapping));
            if (producerAdapters == null)
                throw new ArgumentNullException(nameof(producerAdapters));
            if (consumerAdapters == null)
                throw new ArgumentNullException(nameof(consumerAdapters));
            if (prevMapping == null)
                throw new ArgumentException(nameof(previousMapping), "The previous map must be of type RouteMappingDoubleBufferQueue");

            return new RouteMappingDoubleBufferQueue(prevMapping, producerAdapters, consumerAdapters);
        }

        /// <summary>
        /// Gets a handler for measurements that routes measurements to the appropriate consumers.
        /// </summary>
        /// <returns>The measurement handler used for routing.</returns>
        private EventHandler<EventArgs<ICollection<IMeasurement>>> GetRoutedMeasurementsHandler()
        {
            LocalCache localCache = new LocalCache(this);
            return (sender, args) => localCache.Route(args.Argument);
        }
    }
}