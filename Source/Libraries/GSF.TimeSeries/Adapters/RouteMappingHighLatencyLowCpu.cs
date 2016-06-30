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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Collections;
using GSF.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents an alternative routing table that has intentional delays to lower overall CPU utilization.
    /// </summary>
    public class RouteMappingHighLatencyLowCpu : IRouteMappingTables
    {
        private class ArrayEnumerator<T> : IEnumerator<T>
        {
            private readonly T[] m_items;
            private readonly int m_startingIndex;
            private readonly int m_endingIndex;
            private int m_index;
            public ArrayEnumerator(T[] items, int startingIndex, int length)
            {
                items.ValidateParameters(startingIndex, length);
                m_items = items;
                m_startingIndex = startingIndex;
                m_endingIndex = startingIndex + length - 1;
                m_index = startingIndex - 1;
            }

            public bool MoveNext()
            {
                m_index++;
                return m_index <= m_endingIndex;
            }

            public void Reset()
            {
                m_index = m_startingIndex - 1;
            }

            public T Current
            {
                get
                {
                    if (m_index >= m_startingIndex && m_index <= m_endingIndex)
                        return m_items[m_index];
                    return default(T);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            void IDisposable.Dispose()
            {

            }
        }

        private class ArrayEnumerable<T> : ICollection<T>
        {
            public T[] Items;
            public int StartIndex;
            public int Length;
            public ArrayEnumerable(int count)
            {
                Items = new T[count];
                StartIndex = 0;
                Length = 0;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new ArrayEnumerator<T>(Items, StartIndex, Length);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotImplementedException();
            }

            void ICollection<T>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<T>.Contains(T item)
            {
                throw new NotImplementedException();
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                Array.Copy(Items, StartIndex, array, arrayIndex, Length);
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotImplementedException();
            }

            int ICollection<T>.Count
            {
                get
                {
                    return Length;
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }
        }

        private class DoubleBufferQueue2
        {
            private const int MaxItems = 1000;
            private ScheduledTask m_task;
            public IsolatedQueue<IMeasurement> Queue;
            Action<IEnumerable<IMeasurement>> m_callback;
            ArrayEnumerable<IMeasurement> m_callbackList;

            public DoubleBufferQueue2(Action<IEnumerable<IMeasurement>> callback)
            {
                m_callbackList = new ArrayEnumerable<IMeasurement>(MaxItems);
                Queue = new IsolatedQueue<IMeasurement>();
                m_callback = callback;
                m_task = new ScheduledTask();
                m_task.Running += m_task_Running;
            }

            void m_task_Running(object sender, EventArgs<ScheduledTaskRunningReason> e)
            {
                if (e.Argument == ScheduledTaskRunningReason.Disposing)
                    return;

                int maxCopied = 0;

                while (true)
                {
                    m_callbackList.StartIndex = 0;
                    m_callbackList.Length = Queue.Dequeue(m_callbackList.Items, 0, MaxItems);
                    maxCopied = Math.Max(maxCopied, m_callbackList.Length);

                    if (m_callbackList.Length > 0)
                    {
                        m_callback(m_callbackList);
                    }

                    //If the list was not filled, that means m_callbackList was completely drained.
                    //I can safely quit. Note: ScheduledTask will rerun immediately in the event of a race condition.
                    if (m_callbackList.Length != MaxItems)
                    {
                        Array.Clear(m_callbackList.Items, 0, maxCopied);
                        return;
                    }
                }
            }

            public void RoutingComplete()
            {
                if (Queue.Count > 0)
                    m_task.Start();
            }
        }

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
                foreach (var kvp in consumers)
                {
                    var consumerAdapter = kvp.Key;
                    var consumer = kvp.Value;

                    if ((object)consumerAdapter.InputMeasurementKeys != null)
                    {
                        // Create routes for each of the consumer's input signals
                        foreach (Guid signalID in consumerAdapter.InputMeasurementKeys.Select(key => key.SignalID))
                        {
                            GlobalSignalLookup.GetOrAdd(signalID, id => new List<Consumer>()).Add(consumer);
                        }
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
            public bool Enabled;
            private RouteMappingHighLatencyLowCpu m_route;
            public LocalCache(RouteMappingHighLatencyLowCpu route, IAdapter adapter)
            {
                Enabled = true;
                m_route = route;

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
                m_route.Route(lst);
            }

            private static class ArrayHelper<T>
            {
                public static T[] Empty = new T[0];
            }

            /// <summary>Creates an array from the <see cref="IEnumerable{T}"/>. 
            /// Twice as fast as <see cref="Enumerable.ToArray{T}"/> if <see cref="source"/>
            /// implements <see cref="ICollection{T}"/></summary>
            public static T[] ToArrayOptimized<T>(IEnumerable<T> source)
            {
                if (source == null)
                    throw new ArgumentNullException("source");

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

        private class Consumer
        {
            public readonly DoubleBufferQueue2 Manager;

            public Consumer(IAdapter adapter)
            {
                IActionAdapter actionAdapter = adapter as IActionAdapter;

                if ((object)actionAdapter != null)
                {
                    Manager = new DoubleBufferQueue2(actionAdapter.QueueMeasurementsForProcessing);
                }
                else
                {
                    var outputAdapter = adapter as IOutputAdapter;

                    if ((object)outputAdapter != null)
                        Manager = new DoubleBufferQueue2(outputAdapter.QueueMeasurementsForProcessing);
                    else
                        Manager = null;
                }
            }
        }
        
        // Fields

        private Dictionary<IAdapter, LocalCache> m_producerLookup;

        private readonly ScheduledTask m_task;
        private readonly ConcurrentQueue<IMeasurement[]> m_list;
        private long m_measurementsRoutedInputFrames;
        private long m_measurementsRoutedInputMeasurements;
        private long m_measurementsRoutedOutput;
        private long m_measurementsRoutedDiscarded;
        private long m_routeOperations;
        private int m_routeLatency;

        private GlobalCache m_globalCache;
        private Action<string> m_onStatusMessage;
        private Action<Exception> m_onProcessException;
        private LocalCache m_injectMeasurementsLocalCache;

        /// <summary>
        /// Creates a <see cref="RouteMappingHighLatencyLowCpu"/>
        /// </summary>
        /// <param name="routeLatency">The desired wait latency. Must be between 1 and 500ms inclusive</param>
        public RouteMappingHighLatencyLowCpu(int routeLatency)
        {
            if (routeLatency < 1 || routeLatency > 500)
                throw new ArgumentOutOfRangeException("routeLatency", "Must be between 1 and 500ms");

            m_routeLatency = routeLatency;
            m_list = new ConcurrentQueue<IMeasurement[]>();
            m_task = new ScheduledTask(ThreadingMode.DedicatedBackground, ThreadPriority.AboveNormal);
            m_task.Running += m_task_Running;
            m_task.UnhandledException += m_task_UnhandledException;
            m_task.Disposing += m_task_Disposing;
            m_task.Start(m_routeLatency);

            m_onStatusMessage = x => { };
            m_onProcessException = x => { };
            m_producerLookup = new Dictionary<IAdapter, LocalCache>();
            m_globalCache = new GlobalCache(new Dictionary<IAdapter, Consumer>(), 0);
            m_injectMeasurementsLocalCache = new LocalCache(this, null);
        }

        /// <summary>
        /// Gets the number of routes in this routing table.
        /// </summary>
        public int RouteCount => m_globalCache.GlobalSignalLookup.Count;

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
                m_onStatusMessage(string.Format(
                            "Route Operations: {0}, Input Frames: {1}, Input Measurements: {2}, Output Measurements: {3}, DiscardedOutputs: {4}",
                            m_routeOperations, m_measurementsRoutedInputFrames,
                            m_measurementsRoutedInputMeasurements,
                            m_measurementsRoutedOutput, m_measurementsRoutedDiscarded));
            }

            var map = m_globalCache;

            try
            {
                IMeasurement[] measurements;
                while (m_list.TryDequeue(out measurements))
                {
                    m_measurementsRoutedInputFrames++;
                    m_measurementsRoutedInputMeasurements += measurements.Length;
                    foreach (var measurement in measurements)
                    {
                        List<Consumer> consumers;
                        if (!map.GlobalSignalLookup.TryGetValue(measurement.ID, out consumers))
                        {
                            consumers = map.BroadcastConsumers;
                        }

                        // Add this measurement to the producers' list
                        for (int index = 0; index < consumers.Count; index++)
                        {
                            var consumer = consumers[index];
                            if (consumer.Manager != null)
                            {
                                m_measurementsRoutedOutput++;
                                consumer.Manager.Queue.Enqueue(measurement);
                            }
                            else
                            {
                                m_measurementsRoutedDiscarded++;
                            }
                        }
                    }
                }
            }
            finally
            {
                foreach (var consumer in map.GlobalDestinationLookup.Values)
                {
                    consumer.Manager?.RoutingComplete();
                }
            }
        }

        private void Route(IMeasurement[] measurements)
        {
            if (measurements.Length > 0)
            {
                m_list.Enqueue(measurements);
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


    }
}
