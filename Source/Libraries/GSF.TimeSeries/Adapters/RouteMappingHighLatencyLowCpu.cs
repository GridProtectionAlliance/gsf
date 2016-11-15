//******************************************************************************************************
//  RoutingMappingHighLatencyLowCpu.cs - Gbtc
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
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Collection;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents an alternative routing table that has intentional delays to lower overall CPU utilization.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class RouteMappingHighLatencyLowCpu : IRouteMappingTables
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
        private class Consumer
        {
            private readonly ScheduledTask m_task;
            private readonly Action<IEnumerable<IMeasurement>> m_callback;
            private readonly ConcurrentQueue<List<IMeasurement>> m_pendingMeasurements;
            public List<IMeasurement> MeasurementsToRoute;

            public bool UsesOptimizedRouting;
            public OptimizedRoutingConsumerMethods OptimizedMethods;

            public Consumer(IAdapter adapter)
            {
                OptimizedMethods = (adapter as IOptimizedRoutingConsumer)?.GetOptimizedRoutingConsumerMethods();
                UsesOptimizedRouting = OptimizedMethods != null;
                if (!UsesOptimizedRouting)
                {
                    if (adapter is IActionAdapter)
                    {
                        m_callback = ((IActionAdapter)adapter).QueueMeasurementsForProcessing;
                    }
                    else
                    {
                        m_callback = ((IOutputAdapter)adapter).QueueMeasurementsForProcessing;
                    }

                    m_task = new ScheduledTask();
                    m_task.Running += m_task_Running;
                    m_pendingMeasurements = new ConcurrentQueue<List<IMeasurement>>();
                    MeasurementsToRoute = new List<IMeasurement>();
                }
            }

            public void RoutingComplete()
            {
                if (MeasurementsToRoute.Count > 0)
                {
                    m_pendingMeasurements.Enqueue(MeasurementsToRoute);
                    MeasurementsToRoute = new List<IMeasurement>();
                    m_task.Start();
                }
            }

            void m_task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
            {
                if (e.Argument == ScheduledTaskRunningReason.Disposing)
                    return;

                List<IMeasurement> lst;
                while (m_pendingMeasurements.TryDequeue(out lst))
                {
                    m_callback(lst);
                }
            }

        }

        private class GlobalCache
        {
            public readonly IndexedArray<List<Consumer>> GlobalSignalLookup;
            public readonly Dictionary<IAdapter, Consumer> GlobalDestinationLookup;
            public readonly Consumer[] GlobalDestinationList;
            public readonly List<Consumer> BroadcastConsumers;
            public readonly int Version;

            public GlobalCache(Dictionary<IAdapter, Consumer> consumers, int version)
            {
                GlobalSignalLookup = new IndexedArray<List<Consumer>>();
                GlobalDestinationLookup = consumers;
                BroadcastConsumers = new List<Consumer>();
                Version = version;

                // Generate routes for all signals received by each consumer adapter
                foreach (var kvp in consumers)
                {
                    var consumerAdapter = kvp.Key;
                    var consumer = kvp.Value;

                    if ((object)consumerAdapter.InputMeasurementKeys != null)
                    {
                        // Create routes for each of the consumer's input signals
                        foreach (MeasurementKey key in consumerAdapter.InputMeasurementKeys)
                        {
                            var list = GlobalSignalLookup[key.RuntimeID];
                            if (list == null)
                            {
                                list = new List<Consumer>();
                                GlobalSignalLookup[key.RuntimeID] = list;
                            }

                            list.Add(consumer);
                        }
                    }
                    else
                    {
                        // Add this consumer to the broadcast routes to begin receiving all measurements
                        BroadcastConsumers.Add(consumer);
                    }
                }

                // Broadcast consumers receive all measurements, so add them to every signal route
                foreach (List<Consumer> consumerList in GlobalSignalLookup)
                {
                    consumerList?.AddRange(BroadcastConsumers);
                }

                GlobalDestinationList = GlobalDestinationLookup.Values.ToArray();
            }
        }

        private class LocalCache
        {
            private static int s_nextRuntimeId = 0;

            private int m_runtimeId;
            public bool Enabled;
            private RouteMappingHighLatencyLowCpu m_route;
            public LocalCache(RouteMappingHighLatencyLowCpu route, IAdapter adapter)
            {
                Enabled = true;
                m_route = route;
                m_runtimeId = Interlocked.Increment(ref s_nextRuntimeId);

                IInputAdapter inputAdapter = adapter as IInputAdapter;
                IActionAdapter actionAdapter = adapter as IActionAdapter;

                if ((object)inputAdapter != null)
                    inputAdapter.NewMeasurements += Route;
                else if ((object)actionAdapter != null)
                    actionAdapter.NewMeasurements += Route;
            }

            public void Route(object sender, EventArgs<ICollection<IMeasurement>> measurements)
            {
                if (!Enabled || measurements?.Argument == null)
                    return;
                var lst = ToArrayOptimized(measurements.Argument);
                m_route.Route(lst, m_runtimeId);
            }

            private static class ArrayHelper<T>
            {
                public static T[] Empty = new T[0];
            }

            /// <summary>Creates an array from the <see cref="IEnumerable{T}"/>. 
            /// Twice as fast as <see cref="Enumerable.ToArray{T}"/> if <param name="source"/>
            /// implements <see cref="ICollection{T}"/></summary>
            public static T[] ToArrayOptimized<T>(IEnumerable<T> source)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));

                ICollection<T> collection = source as ICollection<T>;
                if (collection != null)
                {
                    int count = collection.Count;
                    if (count == 0)
                        return ArrayHelper<T>.Empty;
                    var array = new T[count];
                    collection.CopyTo(array, 0);
                    return array;
                }
                return new List<T>(source).ToArray();
            }
        }

        // Fields

        private Dictionary<IAdapter, LocalCache> m_producerLookup;

        private readonly ScheduledTask m_task;
        private readonly IsolatedQueue<IMeasurement[]>[] m_list;
        private long m_measurementsRoutedInputFrames;
        private long m_measurementsRoutedInputMeasurements;
        private long m_measurementsRoutedOutput;
        private long m_routeOperations;
        private int m_routeLatency;

        private GlobalCache m_globalCache;
        private Action<string> m_onStatusMessage;
        private Action<Exception> m_onProcessException;
        private LocalCache m_injectMeasurementsLocalCache;

        private int m_pendingMeasurements;
        /// <summary>
        /// Once this many measurements have been queued, a route operation will not wait the mandatory wait time
        /// and will immediately start the routing process.
        /// </summary>
        private int m_maxPendingMeasurements;

        /// <summary>
        /// Creates a <see cref="RouteMappingHighLatencyLowCpu"/>
        /// </summary>
        /// <param name="routeLatency">The desired wait latency. Must be between 1 and 500ms inclusive</param>
        public RouteMappingHighLatencyLowCpu(int routeLatency)
        {
            if (routeLatency < 1 || routeLatency > 500)
                throw new ArgumentOutOfRangeException(nameof(routeLatency), "Must be between 1 and 500ms");

            m_maxPendingMeasurements = 1000;
            m_routeLatency = routeLatency;
            m_list = new IsolatedQueue<IMeasurement[]>[8];
            for (int x = 0; x < m_list.Length; x++)
            {
                m_list[x] = new IsolatedQueue<IMeasurement[]>();
            }

            m_task = new ScheduledTask(ThreadingMode.DedicatedBackground, ThreadPriority.AboveNormal);
            m_task.Running += m_task_Running;
            m_task.UnhandledException += m_task_UnhandledException;
            m_task.Disposing += m_task_Disposing;
            m_task.Start(m_routeLatency);

            m_onStatusMessage = x => { };
            m_onProcessException = x => { };
            m_producerLookup = new Dictionary<IAdapter, LocalCache>();
            m_globalCache = new GlobalCache(new Dictionary<IAdapter, Consumer>(), 0);
            RouteCount = m_globalCache.GlobalSignalLookup.Count(x => x != null);
            m_injectMeasurementsLocalCache = new LocalCache(this, null);
        }

        /// <summary>
        /// Gets the number of routes in this routing table.
        /// </summary>
        public int RouteCount { get; private set; }

        /// <summary>
        /// Assigns the status messaging callbacks.
        /// </summary>
        /// <param name="onStatusMessage">Raise status messages on this callback</param>
        /// <param name="onProcessException">Raise exceptions on this callback</param>
        public void Initialize(Action<string> onStatusMessage, Action<Exception> onProcessException)
        {
            if (onStatusMessage == null)
                throw new ArgumentNullException(nameof(onStatusMessage));
            if (onProcessException == null)
                throw new ArgumentNullException(nameof(onProcessException));

            m_onStatusMessage = onStatusMessage;
            m_onProcessException = onProcessException;
        }

        /// <summary>
        /// Patches the existing routing table with the supplied adapters.
        /// </summary>
        /// <param name="producerAdapters">all of the producers</param>
        /// <param name="consumerAdapters">all of the consumers</param>
        public void PatchRoutingTable(RoutingTablesAdaptersList producerAdapters, RoutingTablesAdaptersList consumerAdapters)
        {
            if (producerAdapters == null)
                throw new ArgumentNullException(nameof(producerAdapters));
            if (consumerAdapters == null)
                throw new ArgumentNullException(nameof(consumerAdapters));

            foreach (var producerAdapter in producerAdapters.NewAdapter)
            {
                m_producerLookup.Add(producerAdapter, new LocalCache(this, producerAdapter));
            }

            foreach (var producerAdapter in producerAdapters.OldAdapter)
            {
                m_producerLookup[producerAdapter].Enabled = false;
                m_producerLookup.Remove(producerAdapter);
            }

            Dictionary<IAdapter, Consumer> consumerLookup = new Dictionary<IAdapter, Consumer>(m_globalCache.GlobalDestinationLookup);

            foreach (var consumerAdapter in consumerAdapters.NewAdapter)
            {
                consumerLookup.Add(consumerAdapter, new Consumer(consumerAdapter));
            }

            foreach (var consumerAdapter in consumerAdapters.OldAdapter)
            {
                consumerLookup.Remove(consumerAdapter);
            }

            m_globalCache = new GlobalCache(consumerLookup, m_globalCache.Version + 1);
            RouteCount = m_globalCache.GlobalSignalLookup.Count(x => x != null);

        }

        void m_task_Disposing(object sender, EventArgs e)
        {
            m_onProcessException(new Exception("Routing table disposing."));
        }

        void m_task_UnhandledException(object sender, EventArgs<Exception> e)
        {
            m_onProcessException(e.Argument);
        }

        void m_task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            if (e.Argument == ScheduledTaskRunningReason.Disposing)
                return;

            m_task.Start(m_routeLatency);

            m_routeOperations++;

            if (m_routeOperations % 1000 == 0)
            {
                Log.Publish(MessageLevel.Info, MessageFlags.None, "Routing Update",
                string.Format("Route Operations: {0}, Input Frames: {1}, Input Measurements: {2}, Output Measurements: {3}",
                            m_routeOperations, m_measurementsRoutedInputFrames,
                            m_measurementsRoutedInputMeasurements,
                            m_measurementsRoutedOutput));
            }

            var map = m_globalCache;

            List<object> locks = new List<object>();

            try
            {
                foreach (var item in map.GlobalDestinationList)
                {
                    if (item.UsesOptimizedRouting)
                    {
                        Monitor.Enter(item.OptimizedMethods.LockObject);
                        locks.Add(item.OptimizedMethods.LockObject);
                        item.OptimizedMethods.BeginRouting();
                    }
                }

                int measurementsRouted = 0;

                foreach (var queue in m_list)
                {
                    IMeasurement[] measurements;
                    while (queue.TryDequeue(out measurements))
                    {
                        measurementsRouted += measurements.Length;

                        Interlocked.Add(ref m_pendingMeasurements, -measurements.Length);

                        m_measurementsRoutedInputFrames++;
                        m_measurementsRoutedInputMeasurements += measurements.Length;
                        foreach (var measurement in measurements)
                        {
                            List<Consumer> consumers = map.GlobalSignalLookup[measurement.Key.RuntimeID];
                            if (consumers == null)
                            {
                                consumers = map.BroadcastConsumers;
                            }

                            // Add this measurement to the producers' list
                            for (int index = 0; index < consumers.Count; index++)
                            {
                                var consumer = consumers[index];
                                m_measurementsRoutedOutput++;

                                if (consumer.UsesOptimizedRouting)
                                {
                                    consumer.OptimizedMethods.ProcessMeasurement(measurement);
                                }
                                else
                                {
                                    consumer.MeasurementsToRoute.Add(measurement);
                                }
                            }
                        }

                        //Limit routing to between 100 and 200 measurements per sub-route.
                        if (measurementsRouted > 100)
                        {
                            measurementsRouted = 0;
                            foreach (var consumer in map.GlobalDestinationList)
                            {
                                if (!consumer.UsesOptimizedRouting && consumer.MeasurementsToRoute.Count > 100)
                                {
                                    foreach (var c2 in map.GlobalDestinationLookup.Values)
                                    {
                                        if (!c2.UsesOptimizedRouting)
                                        {
                                            c2.RoutingComplete();
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                foreach (var consumer in map.GlobalDestinationList)
                {
                    if (consumer.UsesOptimizedRouting)
                    {
                        consumer.OptimizedMethods.EndRouting();
                    }
                    else
                    {
                        consumer.RoutingComplete();
                    }
                }

                foreach (var item in locks)
                {
                    Monitor.Exit(item);
                }
            }
        }


        private void Route(IMeasurement[] measurements, int runtimeId)
        {
            if (measurements.Length > 0)
            {
                var list = m_list[runtimeId & 7];
                lock (list)
                {
                    list.Enqueue(measurements);
                }
                if (Interlocked.Add(ref m_pendingMeasurements, measurements.Length) > m_maxPendingMeasurements)
                {
                    m_task.Start();
                }
            }
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

        private readonly LogPublisher Log = Logger.CreatePublisher(typeof(RouteMappingHighLatencyLowCpu), MessageClass.Framework);
    }
}
