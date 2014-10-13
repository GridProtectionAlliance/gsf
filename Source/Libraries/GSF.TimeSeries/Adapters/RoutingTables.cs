//******************************************************************************************************
//  RoutingTables.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  06/30/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  07/25/2011 - J. Ritchie Carroll
//       Added code to handle connect on demand adapters (i.e., where AutoStart = false).
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  02/11/2013 - Stephen C. Wills
//       Added code to handle queue and notify for adapter synchronization.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Annotations;
using GSF.Collections;
using GSF.Threading;
using GSF.Units;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the routing tables for the Iaon adapters.
    /// </summary>
    public class RoutingTables : IDisposable
    {
        #region [ Members ]

        // Nested Types

        private class GlobalCache
        {
            public HashSet<IAdapter> ProducerAdapters;
            public Dictionary<Guid, List<Consumer>> GlobalSignalLookup;
            public Dictionary<IAdapter, Consumer> GlobalDestinationLookup;
            public List<Consumer> BroadcastConsumers;
            public int Version;
        }

        private class LocalCache
        {
            private Dictionary<Guid, List<Producer>> m_localSignalLookup;
            private Dictionary<Consumer, Producer> m_localDestinationLookup;
            private RoutingTables m_routingTables;
            private object m_localCacheLock;
            private int m_version;

            public LocalCache(RoutingTables routingTables)
            {
                m_localCacheLock = new object();
                m_localSignalLookup = new Dictionary<Guid, List<Producer>>();
                m_localDestinationLookup = new Dictionary<Consumer, Producer>();
                m_routingTables = routingTables;
            }

            public void Route(IEnumerable<IMeasurement> measurements)
            {
                GlobalCache globalCache;
                List<Producer> producers;
                List<Consumer> consumers;

                // Get the global cache from the routing tables
                globalCache = Interlocked.CompareExchange(ref m_routingTables.m_globalCache, null, null);

                // Return if routes are still being calculated
                if ((object)globalCache == null)
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

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;
        private OutputAdapterCollection m_outputAdapters;

        private GlobalCache m_globalCache;
        private LongSynchronizedOperation m_calculateRoutingTablesOperation;
        private volatile MeasurementKey[] m_inputMeasurementKeysRestriction;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RoutingTables"/> class.
        /// </summary>
        public RoutingTables()
        {
            m_calculateRoutingTablesOperation = new LongSynchronizedOperation(CalculateRoutingTables)
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="RoutingTables"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~RoutingTables()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the active <see cref="InputAdapterCollection"/>.
        /// </summary>
        public InputAdapterCollection InputAdapters
        {
            get
            {
                return m_inputAdapters;
            }
            set
            {
                m_inputAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets the active <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public ActionAdapterCollection ActionAdapters
        {
            get
            {
                return m_actionAdapters;
            }
            set
            {
                m_actionAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets the active <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public OutputAdapterCollection OutputAdapters
        {
            get
            {
                return m_outputAdapters;
            }
            set
            {
                m_outputAdapters = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="RoutingTables"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RoutingTables"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        m_inputAdapters = null;
                        m_actionAdapters = null;
                        m_outputAdapters = null;
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Spawn routing tables recalculation.
        /// </summary>
        /// <param name="inputMeasurementKeysRestriction">Input measurement keys restriction.</param>
        /// <remarks>
        /// Set the <paramref name="inputMeasurementKeysRestriction"/> to null to use full adapter I/O routing demands.
        /// </remarks>
        public virtual void CalculateRoutingTables(MeasurementKey[] inputMeasurementKeysRestriction)
        {
            try
            {
                m_inputMeasurementKeysRestriction = inputMeasurementKeysRestriction;
                m_calculateRoutingTablesOperation.RunOnceAsync();
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue routing table calculation: " + ex.Message, ex));
            }
        }

        private void CalculateRoutingTables()
        {
            long startTime = DateTime.UtcNow.Ticks;
            Time elapsedTime;

            int destinationCount = 0;
            int routeCount;

            IInputAdapter[] inputAdapterCollection = null;
            IActionAdapter[] actionAdapterCollection = null;
            IOutputAdapter[] outputAdapterCollection = null;
            bool retry = true;

            OnStatusMessage("Starting measurement route calculation...");

            // Attempt to cache input, action, and output adapters for routing table calculation.
            // This could fail if another thread modifies the collections while caching is in
            // progress (rare), so retry if the caching fails.
            //
            // We don't attempt to lock here because we don't own the collections.
            while (retry)
            {
                try
                {
                    if ((object)m_inputAdapters != null)
                        inputAdapterCollection = m_inputAdapters.ToArray<IInputAdapter>();

                    if ((object)m_actionAdapters != null)
                        actionAdapterCollection = m_actionAdapters.ToArray<IActionAdapter>();

                    if ((object)m_outputAdapters != null)
                        outputAdapterCollection = m_outputAdapters.ToArray<IOutputAdapter>();

                    retry = false;
                }
                catch (InvalidOperationException)
                {
                    // Attempt to catch "Collection was modified; enumeration operation may not execute."
                }
                catch (NullReferenceException)
                {
                    // Catch rare exceptions where IaonSession is disposed during a context switch
                    inputAdapterCollection = null;
                    actionAdapterCollection = null;
                    outputAdapterCollection = null;
                    retry = false;
                }
            }

            try
            {
                GlobalCache globalCache;

                Dictionary<Guid, List<Consumer>> globalSignalLookup;
                Dictionary<IAdapter, Consumer> globalDestinationLookup;
                List<Consumer> broadcastConsumers;

                HashSet<IAdapter> producerAdapters;
                IInputAdapter inputAdapter;

                IEnumerable<IAdapter> consumerAdapters;
                Consumer consumer;

                // Get the global cache from the last routing tables calculation
                globalCache = Interlocked.CompareExchange(ref m_globalCache, null, null);

                // Create collections to be entered into the new global cache
                globalSignalLookup = new Dictionary<Guid, List<Consumer>>();
                globalDestinationLookup = new Dictionary<IAdapter, Consumer>();
                broadcastConsumers = new List<Consumer>();

                // Get a full list of all producer (input/action) adapters
                producerAdapters = new HashSet<IAdapter>((inputAdapterCollection ?? Enumerable.Empty<IAdapter>())
                    .Concat(actionAdapterCollection ?? Enumerable.Empty<IAdapter>()));

                // Attach to NewMeasurements event of all producer adapters
                foreach (IAdapter producerAdapter in producerAdapters)
                {
                    if ((object)globalCache == null || !globalCache.ProducerAdapters.Contains(producerAdapter))
                    {
                        inputAdapter = producerAdapter as IInputAdapter;

                        if ((object)inputAdapter != null)
                            inputAdapter.NewMeasurements += GetRoutedMeasurementsHandler();
                        else
                            ((IActionAdapter)producerAdapter).NewMeasurements += GetRoutedMeasurementsHandler();
                    }
                }

                // Get a full list of all consumer (action/output) adapters
                consumerAdapters = (actionAdapterCollection ?? Enumerable.Empty<IAdapter>())
                    .Concat(outputAdapterCollection ?? Enumerable.Empty<IAdapter>());

                // Generate routes for all signals received by each consumer adapter
                foreach (IAdapter consumerAdapter in consumerAdapters)
                {
                    // Search the old global cache for an existing consumer for this adapter
                    if ((object)globalCache == null || !globalCache.GlobalDestinationLookup.TryGetValue(consumerAdapter, out consumer))
                        consumer = new Consumer(consumerAdapter, OnProcessException);

                    if ((object)consumerAdapter.InputMeasurementKeys != null)
                    {
                        // Create routes for each of the consumer's input signals
                        foreach (Guid signalID in consumerAdapter.InputMeasurementKeys.Select(key => key.SignalID))
                            globalSignalLookup.GetOrAdd(signalID, id => new List<Consumer>()).Add(consumer);
                    }
                    else
                    {
                        // Add this consumer to the broadcast routes to begin receiving all measurements
                        broadcastConsumers.Add(consumer);
                    }

                    // Add this adapter to the global destinations lookup
                    globalDestinationLookup.Add(consumerAdapter, consumer);
                    destinationCount++;
                }

                // Broadcast consumers receive all measurements, so add them to every signal route
                foreach (List<Consumer> consumerList in globalSignalLookup.Values)
                    consumerList.AddRange(broadcastConsumers);

                // Swap the new global cache with the old one and increment the version number
                Interlocked.Exchange(ref m_globalCache, new GlobalCache()
                {
                    ProducerAdapters = producerAdapters,
                    GlobalSignalLookup = globalSignalLookup,
                    GlobalDestinationLookup = globalDestinationLookup,
                    BroadcastConsumers = broadcastConsumers,
                    Version = (object)globalCache != null ? globalCache.Version + 1 : 0
                });

                // Start or stop any connect on demand adapters
                HandleConnectOnDemandAdapters(new HashSet<MeasurementKey>(m_inputMeasurementKeysRestriction ?? Enumerable.Empty<MeasurementKey>()), inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);

                elapsedTime = Ticks.ToSeconds(DateTime.UtcNow.Ticks - startTime);
                routeCount = globalSignalLookup.Count;

                OnStatusMessage("Calculated {0} route{1} for {2} destination{3} in {4}.", routeCount, (routeCount == 1) ? "" : "s", destinationCount, (destinationCount == 1) ? "" : "s", elapsedTime.ToString(2));
            }
            catch (ObjectDisposedException)
            {
                // Ignore this error. Seems to happen during normal
                // operation and does not affect the result.
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Routing tables calculation error: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Gets a handler for measurements that routes measurements to the appropriate consumers.
        /// </summary>
        /// <returns>The measurement handler used for routing.</returns>
        public virtual EventHandler<EventArgs<ICollection<IMeasurement>>> GetRoutedMeasurementsHandler()
        {
            LocalCache localCache = new LocalCache(this);
            return (sender, args) => localCache.Route(args.Argument);
        }

        /// <summary>
        /// Event handler for distributing new measurements in a broadcast fashion.
        /// </summary>
        /// <param name="sender">Event source reference to adapter that generated new measurements.</param>
        /// <param name="e">Event arguments containing a collection of new measurements.</param>
        /// <remarks>
        /// Time-series framework uses this handler to route new measurements to the action and output adapters; adapter will handle filtering.
        /// </remarks>
        public virtual void BroadcastMeasurementsHandler(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            ICollection<IMeasurement> newMeasurements = e.Argument;

            m_actionAdapters.QueueMeasurementsForProcessing(newMeasurements);
            m_outputAdapters.QueueMeasurementsForProcessing(newMeasurements);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            try
            {
                if (StatusMessage != null)
                    StatusMessage(this, new EventArgs<string>(status));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for StatusMessage event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convenience.
        /// </remarks>
        [StringFormatMethod("formattedStatus")]
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            try
            {
                if (StatusMessage != null)
                    StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for StatusMessage event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Starts or stops connect on demand adapters based on current state of demanded input or output signals.
        /// </summary>
        /// <param name="inputMeasurementKeysRestriction">The set of signals to be produced by the chain of adapters to be handled.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        /// <remarks>
        /// Set the <paramref name="inputMeasurementKeysRestriction"/> to null to use full adapter routing demands.
        /// </remarks>
        protected virtual void HandleConnectOnDemandAdapters(ISet<MeasurementKey> inputMeasurementKeysRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            ISet<IAdapter> dependencyChain;

            ISet<MeasurementKey> inputSignals;
            ISet<MeasurementKey> outputSignals;
            ISet<MeasurementKey> requestedInputSignals;
            ISet<MeasurementKey> requestedOutputSignals;

            if (inputMeasurementKeysRestriction.Any())
            {
                // When an input signals restriction has been defined, determine the set of adapters
                // by walking the dependency chain of the restriction
                dependencyChain = TraverseDependencyChain(inputMeasurementKeysRestriction, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }
            else
            {
                // Determine the set of adapters in the dependency chain for all adapters in the system
                dependencyChain = TraverseDependencyChain(inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            // Get the full set of requested input and output signals in the entire dependency chain
            inputSignals = new HashSet<MeasurementKey>(dependencyChain.SelectMany(adapter => adapter.InputMeasurementKeys()));
            outputSignals = new HashSet<MeasurementKey>(dependencyChain.SelectMany(adapter => adapter.OutputMeasurementKeys()));

            // Turn connect on demand input adapters on or off based on whether they are part of the dependency chain
            if ((object)inputAdapterCollection != null)
            {
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (!inputAdapter.AutoStart)
                    {
                        if (dependencyChain.Contains(inputAdapter))
                        {
                            requestedOutputSignals = new HashSet<MeasurementKey>(inputAdapter.OutputMeasurementKeys());
                            requestedOutputSignals.IntersectWith(inputSignals);
                            inputAdapter.RequestedOutputMeasurementKeys = requestedOutputSignals.ToArray();
                            inputAdapter.Enabled = true;
                        }
                        else
                        {
                            inputAdapter.RequestedOutputMeasurementKeys = null;
                            inputAdapter.Enabled = false;
                        }
                    }
                }
            }

            // Turn connect on demand action adapters on or off based on whether they are part of the dependency chain
            if ((object)actionAdapterCollection != null)
            {
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (!actionAdapter.AutoStart)
                    {
                        if (dependencyChain.Contains(actionAdapter))
                        {
                            if (actionAdapter.RespectInputDemands)
                            {
                                requestedInputSignals = new HashSet<MeasurementKey>(actionAdapter.InputMeasurementKeys());
                                requestedInputSignals.IntersectWith(outputSignals);
                                actionAdapter.RequestedInputMeasurementKeys = requestedInputSignals.ToArray();
                            }

                            if (actionAdapter.RespectOutputDemands)
                            {
                                requestedOutputSignals = new HashSet<MeasurementKey>(actionAdapter.OutputMeasurementKeys());
                                requestedOutputSignals.IntersectWith(inputSignals);
                                actionAdapter.RequestedOutputMeasurementKeys = requestedOutputSignals.ToArray();
                            }

                            actionAdapter.Enabled = true;
                        }
                        else
                        {
                            actionAdapter.RequestedInputMeasurementKeys = null;
                            actionAdapter.RequestedOutputMeasurementKeys = null;
                            actionAdapter.Enabled = false;
                        }
                    }
                }
            }

            // Turn connect on demand output adapters on or off based on whether they are part of the dependency chain
            if ((object)outputAdapterCollection != null)
            {
                foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                {
                    if (!outputAdapter.AutoStart)
                    {
                        if (dependencyChain.Contains(outputAdapter))
                        {
                            requestedInputSignals = new HashSet<MeasurementKey>(outputAdapter.OutputMeasurementKeys());
                            requestedInputSignals.IntersectWith(inputSignals);
                            outputAdapter.RequestedInputMeasurementKeys = requestedInputSignals.ToArray();
                            outputAdapter.Enabled = true;
                        }
                        else
                        {
                            outputAdapter.RequestedInputMeasurementKeys = null;
                            outputAdapter.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the set of adapters in the dependency chain that produces the set of signals in the
        /// <paramref name="inputMeasurementKeysRestriction"/> and returns the set of input signals required by the
        /// adapters in the chain and the set of output signals produced by the adapters in the chain.
        /// </summary>
        /// <param name="inputMeasurementKeysRestriction">The set of signals that must be produced by the dependency chain.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        protected virtual ISet<IAdapter> TraverseDependencyChain(ISet<MeasurementKey> inputMeasurementKeysRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            ISet<IAdapter> dependencyChain = new HashSet<IAdapter>();

            if ((object)inputAdapterCollection != null)
            {
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (!dependencyChain.Contains(inputAdapter) && inputMeasurementKeysRestriction.Overlaps(inputAdapter.OutputMeasurementKeys()))
                        AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)actionAdapterCollection != null)
            {
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (!dependencyChain.Contains(actionAdapter) && inputMeasurementKeysRestriction.Overlaps(actionAdapter.OutputMeasurementKeys()))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            return dependencyChain;
        }

        /// <summary>
        /// Determines the set of adapters in the dependency chain for all adapters in the system which are either not connect or demand or are demanded.
        /// </summary>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        protected virtual ISet<IAdapter> TraverseDependencyChain(IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            ISet<IAdapter> dependencyChain = new HashSet<IAdapter>();

            if ((object)inputAdapterCollection != null)
            {
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (inputAdapter.AutoStart && !dependencyChain.Contains(inputAdapter))
                        AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)actionAdapterCollection != null)
            {
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (actionAdapter.AutoStart && !dependencyChain.Contains(actionAdapter))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)outputAdapterCollection != null)
            {
                foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                {
                    if (outputAdapter.AutoStart && !dependencyChain.Contains(outputAdapter))
                        AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            return dependencyChain;
        }

        // Adds an input adapter to the dependency chain.
        private void AddInputAdapter(IInputAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            HashSet<MeasurementKey> outputMeasurementKeys = new HashSet<MeasurementKey>(adapter.OutputMeasurementKeys());

            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            if ((object)actionAdapterCollection != null)
            {
                // Checks all action adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (actionAdapter.RespectInputDemands && !dependencyChain.Contains(actionAdapter) && outputMeasurementKeys.Overlaps(actionAdapter.InputMeasurementKeys()))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)outputAdapterCollection != null)
            {
                // Checks all output adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                {
                    if (!dependencyChain.Contains(outputAdapter) && outputMeasurementKeys.Overlaps(outputAdapter.InputMeasurementKeys()))
                        AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }
        }

        // Adds an action adapter to the dependency chain.
        private void AddActionAdapter(IActionAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            HashSet<MeasurementKey> inputMeasurementKeys = new HashSet<MeasurementKey>(adapter.InputMeasurementKeys());
            HashSet<MeasurementKey> outputMeasurementKeys = new HashSet<MeasurementKey>(adapter.OutputMeasurementKeys());

            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            if ((object)inputAdapterCollection != null)
            {
                // Checks all input adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (!dependencyChain.Contains(inputAdapter) && inputMeasurementKeys.Overlaps(inputAdapter.OutputMeasurementKeys()))
                        AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)actionAdapterCollection != null)
            {
                // Checks all action adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (!dependencyChain.Contains(actionAdapter))
                    {
                        if (actionAdapter.RespectInputDemands && outputMeasurementKeys.Overlaps(actionAdapter.InputMeasurementKeys()))
                            AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                        else if (actionAdapter.RespectOutputDemands && inputMeasurementKeys.Overlaps(actionAdapter.OutputMeasurementKeys()))
                            AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                    }
                }
            }

            if ((object)outputAdapterCollection != null)
            {
                // Checks all output adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                {
                    if (!dependencyChain.Contains(outputAdapter) && outputMeasurementKeys.Overlaps(outputAdapter.InputMeasurementKeys()))
                        AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }
        }

        // Adds an output adapter to the dependency chain.
        private void AddOutputAdapter(IOutputAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            HashSet<MeasurementKey> inputMeasurementKeys = new HashSet<MeasurementKey>(adapter.InputMeasurementKeys());

            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            if ((object)inputAdapterCollection != null)
            {
                // Checks all input adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (!dependencyChain.Contains(inputAdapter) && inputMeasurementKeys.Overlaps(inputAdapter.OutputMeasurementKeys()))
                        AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            if ((object)actionAdapterCollection != null)
            {
                // Checks all action adapters to determine whether they also need to be
                // added to the chain as a result of this adapter being added to the chain
                foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                {
                    if (actionAdapter.RespectOutputDemands && !dependencyChain.Contains(actionAdapter) && inputMeasurementKeys.Overlaps(actionAdapter.OutputMeasurementKeys()))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }
        }

        #endregion
    }
}
