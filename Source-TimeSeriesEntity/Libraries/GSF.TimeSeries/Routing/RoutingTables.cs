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
//  02/11/2013 - Stephen C. Wills
//       Added code to handle queue and notify for adapter synchronization.
//  10/23/2013 - Stephen C. Wills
//       Removed queue and notify and modified the routing tables to support the new mechanism for
//       defining processing methods for time-series entities in adapters.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GSF.Collections;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Routing
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
            public Dictionary<Guid, ICollection<IAdapter>> DestinationsLookup;
            public Dictionary<IAdapter, ICollection<SignalRoute>> AdapterRoutesLookup;
            public int CacheVersion;
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

        private volatile GlobalCache m_globalCache;
        private AutoResetEvent m_calculationComplete;
        private readonly object m_queuedCalculationPending;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RoutingTables"/> class.
        /// </summary>
        public RoutingTables()
        {
            m_calculationComplete = new AutoResetEvent(true);
            m_queuedCalculationPending = new object();
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
                        m_globalCache = null;
                        m_inputAdapters = null;
                        m_actionAdapters = null;
                        m_outputAdapters = null;

                        if ((object)m_calculationComplete != null)
                        {
                            // Release any waiting threads before disposing wait handle
                            m_calculationComplete.Set();
                            m_calculationComplete.Dispose();
                        }

                        m_calculationComplete = null;
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
        /// <param name="inputSignalsRestriction">The set of signals to be produced by the chain of adapters to be handled.</param>
        /// <remarks>
        /// Set the <paramref name="inputSignalsRestriction"/> to null to use full adapter I/O routing demands.
        /// </remarks>
        public virtual void CalculateRoutingTables(ISet<Guid> inputSignalsRestriction)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(QueueRoutingTableCalculation, inputSignalsRestriction);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue routing table calculation: " + ex.Message, ex));
            }
        }

        private void QueueRoutingTableCalculation(object state)
        {
            try
            {
                // Queue up a routing table calculation unless another thread has already requested one
                if (!m_disposed && Monitor.TryEnter(m_queuedCalculationPending))
                {
                    try
                    {
                        // Queue new routing table calculation after waiting for any prior calculation to complete
                        if (m_calculationComplete.WaitOne())
                            ThreadPool.QueueUserWorkItem(CalculateRoutingTables, state);
                    }
                    finally
                    {
                        Monitor.Exit(m_queuedCalculationPending);
                    }
                }
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue routing table calculation: " + ex.Message, ex));
            }
        }

        private void CalculateRoutingTables(object state)
        {
            long startTime = DateTime.UtcNow.Ticks;
            double elapsedTime;

            int destinationCount = 0;
            int routeCount;

            IInputAdapter[] inputAdapterCollection = null;
            IActionAdapter[] actionAdapterCollection = null;
            IOutputAdapter[] outputAdapterCollection = null;
            bool retry = true;

            OnStatusMessage("Starting routing tables calculation...");

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
                // Pre-calculate internal routes to improve performance
                IEnumerable<IAdapter> adapterCollection;

                Dictionary<Guid, ICollection<IAdapter>> destinationsLookup = new Dictionary<Guid, ICollection<IAdapter>>();
                Dictionary<IAdapter, ICollection<SignalRoute>> routesLookup = new Dictionary<IAdapter, ICollection<SignalRoute>>();
                Dictionary<Type, ICollection<SignalRoute>> routesLookupTemplates = new Dictionary<Type, ICollection<SignalRoute>>();

                Func<Type, ICollection<SignalRoute>> templateFactory;
                ICollection<IAdapter> destinations;
                ICollection<SignalRoute> routes;

                ISet<Guid> signalIDs;
                Type adapterType;

                // Factory method for creating templates for routes that can be looked up by adapter
                // type so that reflection only needs to be performed once per adapter type
                // TODO: Use an attribute or something to allow adapter writers to define what their processing functions should be
                templateFactory = type => type.GetMethods()
                    .Where(method => method.Name == "QueueEntriesForProcessing")
                    .Select(method => new SignalRoute(method))
                    .Where(route => (object)route.ListType != null)
                    .ToList();

                // Create a collection containing the action and output adapters together
                adapterCollection = (actionAdapterCollection ?? Enumerable.Empty<IAdapter>())
                    .Concat(outputAdapterCollection ?? Enumerable.Empty<IAdapter>());

                // Calculate possible routes for each action and output adapter
                foreach (IAdapter adapter in adapterCollection)
                {
                    // Make sure adapter is initialized before calculating route
                    if (adapter.Initialized)
                    {
                        signalIDs = adapter.InputSignals;
                        adapterType = adapter.GetType();

                        // Add this adapter as a destination to each
                        // signal defined in its input measurement keys
                        foreach (Guid signalID in signalIDs)
                        {
                            destinations = destinationsLookup.GetOrAdd(signalID, guid => new List<IAdapter>());
                            destinations.Add(adapter);
                        }

                        // Determine which methods defined on the adapter type qualify as routes and store those in a lookup table
                        routes = routesLookupTemplates.GetOrAdd(adapterType, templateFactory)
                            .Select(route => new SignalRoute(route, adapter))
                            .ToList();

                        routesLookup.Add(adapter, routes);
                    }
                }

                // Update global lookup tables for routing
                m_globalCache = new GlobalCache()
                {
                    DestinationsLookup = destinationsLookup,
                    AdapterRoutesLookup = routesLookup,
                    CacheVersion = ((object)m_globalCache != null) ? m_globalCache.CacheVersion + 1 : 0
                };

                // Start or stop any connect on demand adapters
                HandleConnectOnDemandAdapters((ISet<Guid>)state, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);

                elapsedTime = Ticks.ToSeconds(DateTime.UtcNow.Ticks - startTime);

                if ((object)actionAdapterCollection != null)
                    destinationCount += actionAdapterCollection.Length;

                if ((object)outputAdapterCollection != null)
                    destinationCount += outputAdapterCollection.Length;

                routeCount = routesLookup.Count;

                OnStatusMessage("Calculated {0} route{1} for {2} destination{3} in {4}.", routeCount, (routeCount == 1) ? "" : "s", destinationCount, (destinationCount == 1) ? "" : "s", elapsedTime < 0.01D ? "less than a second" : elapsedTime.ToString("0.00") + " seconds");
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
            finally
            {
                if ((object)m_calculationComplete != null)
                    m_calculationComplete.Set();
            }
        }

        /// <summary>
        /// Event handler for distributing new time-series entities in a routed fashion.
        /// </summary>
        /// <param name="sender">Event source reference to adapter that generated new time-series entities.</param>
        /// <param name="localCache">Event arguments containing a collection of new time-series entities.</param>
        /// <remarks>
        /// Time-series framework uses this handler to directly route new time-series entities to the action and output adapters.
        /// </remarks>
        public virtual void RoutingEventHandler(object sender, RoutingEventArgs localCache)
        {
            HashSet<SignalRoute> activeRoutes = new HashSet<SignalRoute>();
            ICollection<SignalRoute> signalRoutes;

            GlobalCache globalCache = m_globalCache;

            try
            {
                // Always make sure the local cache
                // is initialized and up-to-date
                localCache.Initialize(globalCache.CacheVersion);

                foreach (ITimeSeriesEntity timeSeriesEntity in localCache.TimeSeriesEntities)
                {
                    if (!localCache.SignalRoutesLookup.TryGetValue(timeSeriesEntity.ID, out signalRoutes))
                    {
                        // Use the global cache to find the signal
                        // routes, and store them in the local cache
                        signalRoutes = FindAndCacheSignalRoutes(timeSeriesEntity, globalCache, localCache);
                    }

                    foreach (SignalRoute signalRoute in signalRoutes)
                    {
                        try
                        {
                            // Add the time-series entity to the
                            // list of entities to be processed
                            signalRoute.List.Add(timeSeriesEntity);

                            // Add this route to the set of active routes so
                            // we know to invoke its processing method later
                            activeRoutes.Add(signalRoute);
                        }
                        catch (ArgumentException ex)
                        {
                            // ArgumentException occurs when a type that is not compatible with the list is added to the list.
                            // This can occur if the adapter producing this signal changes the type of the signal at runtime.
                            // Rather than allowing this behavior, doing additional lookups, and incurring slowdowns in the routing tables,
                            // this behavior is discouraged by cutting that signal off and refusing to route it to the adapters.
                            string message = string.Format("Type change detected! Dumping all routes for [{0}].", timeSeriesEntity.ID);
                            OnProcessException(new InvalidOperationException(message, ex));
                            localCache.SignalRoutesLookup[timeSeriesEntity.ID].Clear();
                        }
                    }
                }

                // Invoke the processing method for each active route,
                // and also clear the list of entities to prepare for
                // the next routing event
                foreach (SignalRoute activeRoute in activeRoutes)
                {
                    try
                    {
                        activeRoute.Invoke();
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException(string.Format("ERROR: Exception routing data to adapter [{0}]: {1}", activeRoute.Adapter.Name, ex.Message), ex));
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("ERROR: Exception occurred in time-series routing event handler: {0}", ex.Message), ex));
            }
        }

        private ICollection<SignalRoute> FindAndCacheSignalRoutes(ITimeSeriesEntity timeSeriesEntity, GlobalCache globalCache, RoutingEventArgs localCache)
        {
            List<SignalRoute> signalRoutes = new List<SignalRoute>();
            Type timeSeriesEntityType = timeSeriesEntity.GetType();

            ICollection<IAdapter> destinations;
            ICollection<SignalRoute> localAdapterRoutes;
            List<SignalRoute> matchingRoutes;
            List<SignalRoute> genericRoutes;

            Func<IAdapter, ICollection<SignalRoute>> adapterRoutesFactory = adapter =>
            {
                ICollection<SignalRoute> adapterRoutes;

                return globalCache.AdapterRoutesLookup.TryGetValue(adapter, out adapterRoutes)
                    ? adapterRoutes.Select(route => new SignalRoute(route)).ToList()
                    : new List<SignalRoute>();
            };

            if (globalCache.DestinationsLookup.TryGetValue(timeSeriesEntity.ID, out destinations))
            {
                foreach (IAdapter destination in destinations)
                {
                    // Check the local cache for routes first
                    localAdapterRoutes = localCache.AdapterRoutes.GetOrAdd(destination, adapterRoutesFactory);

                    // If there is a route that matches exactly with this time-series
                    // entity, then we are guaranteed there can be no better match
                    matchingRoutes = localAdapterRoutes
                        .Where(route => route.SignalType == timeSeriesEntityType)
                        .ToList();

                    if (matchingRoutes.Count == 0)
                    {
                        // Attempt to create matching generic routes
                        genericRoutes = localAdapterRoutes
                            .Select(route => route.MakeGenericSignalRoute(timeSeriesEntityType))
                            .Where(genericRoute => (object)genericRoute != null)
                            .Where(genericRoute => localAdapterRoutes.All(route => route.SignalType != genericRoute.SignalType))
                            .ToList();

                        // Add all created routes to the local cache
                        foreach (SignalRoute genericRoute in genericRoutes)
                            localAdapterRoutes.Add(genericRoute);

                        // Get routes where the signal type is compatible with this time-series entity
                        matchingRoutes = localAdapterRoutes
                            .Where(route => route.SignalType.IsAssignableFrom(timeSeriesEntityType))
                            .ToList();
                    }

                    // We should have found at least one route unless there
                    // are no routes defined to handle the signal's type
                    if (matchingRoutes.Count > 0)
                    {
                        // In the case of multiple matches, get routes with
                        // the most defined signal types, where it is not
                        // compatible with any other type in the list
                        if (matchingRoutes.Count > 1)
                        {
                            matchingRoutes = matchingRoutes
                                .Where(r1 => !matchingRoutes.Any(r2 => r1 != r2 && r1.SignalType.IsAssignableFrom(r2.SignalType)))
                                .ToList();
                        }

                        // If we still have multiple matches,
                        // try throwing out the generic routes
                        if (matchingRoutes.Count > 1)
                        {
                            matchingRoutes = matchingRoutes
                                .Where(route => !route.ProcessingMethod.IsGenericMethod)
                                .ToList();
                        }

                        // There should only be one route,
                        // except in the case of ambiguous matches
                        if (matchingRoutes.Count == 1)
                        {
                            signalRoutes.Add(matchingRoutes[0]);
                        }
                        else
                        {
                            const string Message = "WARNING: When locating routes for signal [{0}] going to adapter [{1}]," +
                                " multiple processing methods were found that could handle the signal's type, and the" +
                                " system was unable to resolve the ambiguity. The signal will not be processed by this" +
                                " adapter. The adapter will likely need to be fixed to resolve the ambiguity.";

                            OnStatusMessage(Message, timeSeriesEntity.ID, destination.Name);
                        }
                    }
                    else
                    {
                        const string Message = "WARNING: When locating routes for signal [{0}] going to adapter [{1}]," +
                            " no processing methods were found that could handle the signal's type. The signal will" +
                            " not be processed by this adapter. This is likely a configuration error.";

                        OnStatusMessage(Message, timeSeriesEntity.ID, destination.Name);
                    }
                }
            }

            // Store these routes in the local cache
            localCache.SignalRoutesLookup.Add(timeSeriesEntity.ID, signalRoutes);

            return signalRoutes;
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
        /// <param name="inputSignalsRestriction">The set of signals to be produced by the chain of adapters to be handled.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        /// <remarks>
        /// Set the <paramref name="inputSignalsRestriction"/> to null to use full adapter routing demands.
        /// </remarks>
        protected virtual void HandleConnectOnDemandAdapters(ISet<Guid> inputSignalsRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            ISet<IAdapter> dependencyChain;

            ISet<Guid> inputSignals;
            ISet<Guid> outputSignals;
            ISet<Guid> requestedInputSignals;
            ISet<Guid> requestedOutputSignals;

            if ((object)inputSignalsRestriction != null && inputSignalsRestriction.Any())
            {
                // When an input signals restriction has been defined, determine the set of adapters
                // by walking the dependency chain of the restriction
                dependencyChain = TraverseDependencyChain(inputSignalsRestriction, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }
            else
            {
                // Determine the set of adapters in the dependency chain for all adapters in the system
                dependencyChain = TraverseDependencyChain(inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            // Get the full set of requested input and output signals in the entire dependency chain
            inputSignals = new HashSet<Guid>(dependencyChain.SelectMany(adapter => adapter.InputSignals));
            outputSignals = new HashSet<Guid>(dependencyChain.SelectMany(adapter => adapter.OutputSignals));

            // Turn connect on demand input adapters on or off based on whether they are part of the dependency chain
            if ((object)inputAdapterCollection != null)
            {
                foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                {
                    if (!inputAdapter.AutoStart)
                    {
                        if (dependencyChain.Contains(inputAdapter))
                        {
                            requestedOutputSignals = new HashSet<Guid>(inputAdapter.OutputSignals);
                            requestedOutputSignals.IntersectWith(inputSignals);
                            inputAdapter.RequestedOutputSignals = requestedOutputSignals;
                            inputAdapter.Enabled = true;
                        }
                        else
                        {
                            inputAdapter.RequestedOutputSignals = null;
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
                                requestedInputSignals = new HashSet<Guid>(actionAdapter.InputSignals);
                                requestedInputSignals.IntersectWith(outputSignals);
                                actionAdapter.RequestedInputSignals = requestedInputSignals;
                            }

                            if (actionAdapter.RespectOutputDemands)
                            {
                                requestedOutputSignals = new HashSet<Guid>(actionAdapter.OutputSignals);
                                requestedOutputSignals.IntersectWith(inputSignals);
                                actionAdapter.RequestedOutputSignals = requestedOutputSignals;
                            }

                            actionAdapter.Enabled = true;
                        }
                        else
                        {
                            actionAdapter.RequestedInputSignals = null;
                            actionAdapter.RequestedOutputSignals = null;
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
                            requestedInputSignals = new HashSet<Guid>(outputAdapter.OutputSignals);
                            requestedInputSignals.IntersectWith(inputSignals);
                            outputAdapter.RequestedInputSignals = requestedInputSignals;
                            outputAdapter.Enabled = true;
                        }
                        else
                        {
                            outputAdapter.RequestedInputSignals = null;
                            outputAdapter.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the set of adapters in the dependency chain that produces the set of signals in the
        /// <paramref name="inputSignalsRestriction"/> and returns the set of input signals required by the
        /// adapters in the chain and the set of output signals produced by the adapters in the chain.
        /// </summary>
        /// <param name="inputSignalsRestriction">The set of signals that must be produced by the dependency chain.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        protected virtual ISet<IAdapter> TraverseDependencyChain(ISet<Guid> inputSignalsRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            ISet<IAdapter> dependencyChain = new HashSet<IAdapter>();

            foreach (IInputAdapter inputAdapter in inputAdapterCollection)
            {
                if (!dependencyChain.Contains(inputAdapter) && inputSignalsRestriction.Overlaps(inputAdapter.OutputSignals))
                    AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            foreach (IActionAdapter actionAdapter in actionAdapterCollection)
            {
                if (!dependencyChain.Contains(actionAdapter) && inputSignalsRestriction.Overlaps(actionAdapter.OutputSignals))
                    AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
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

            foreach (IInputAdapter inputAdapter in inputAdapterCollection)
            {
                if (inputAdapter.AutoStart && !dependencyChain.Contains(inputAdapter))
                    AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            foreach (IActionAdapter actionAdapter in actionAdapterCollection)
            {
                if (actionAdapter.AutoStart && !dependencyChain.Contains(actionAdapter))
                    AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
            {
                if (outputAdapter.AutoStart && !dependencyChain.Contains(outputAdapter))
                    AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            return dependencyChain;
        }

        // Adds an input adapter to the dependency chain.
        private void AddInputAdapter(IInputAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            // Checks all action adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IActionAdapter actionAdapter in actionAdapterCollection)
            {
                if (actionAdapter.RespectInputDemands && !dependencyChain.Contains(actionAdapter) && adapter.OutputSignals.Overlaps(actionAdapter.InputSignals))
                    AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            // Checks all output adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
            {
                if (!dependencyChain.Contains(outputAdapter) && adapter.OutputSignals.Overlaps(outputAdapter.InputSignals))
                    AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }
        }

        // Adds an action adapter to the dependency chain.
        private void AddActionAdapter(IActionAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            // Checks all input adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IInputAdapter inputAdapter in inputAdapterCollection)
            {
                if (!dependencyChain.Contains(inputAdapter) && adapter.InputSignals.Overlaps(inputAdapter.OutputSignals))
                    AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            // Checks all action adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IActionAdapter actionAdapter in actionAdapterCollection)
            {
                if (!dependencyChain.Contains(actionAdapter))
                {
                    if (actionAdapter.RespectInputDemands && adapter.OutputSignals.Overlaps(actionAdapter.InputSignals))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                    else if (actionAdapter.RespectOutputDemands && adapter.InputSignals.Overlaps(actionAdapter.OutputSignals))
                        AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
                }
            }

            // Checks all output adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
            {
                if (!dependencyChain.Contains(outputAdapter) && adapter.OutputSignals.Overlaps(outputAdapter.InputSignals))
                    AddOutputAdapter(outputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }
        }

        // Adds an output adapter to the dependency chain.
        private void AddOutputAdapter(IOutputAdapter adapter, ISet<IAdapter> dependencyChain, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            // Adds the adapter to the chain
            dependencyChain.Add(adapter);

            // Checks all input adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IInputAdapter inputAdapter in inputAdapterCollection)
            {
                if (!dependencyChain.Contains(inputAdapter) && adapter.InputSignals.Overlaps(inputAdapter.OutputSignals))
                    AddInputAdapter(inputAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }

            // Checks all action adapters to determine whether they also need to be
            // added to the chain as a result of this adapter being added to the chain
            foreach (IActionAdapter actionAdapter in actionAdapterCollection)
            {
                if (actionAdapter.RespectOutputDemands && !dependencyChain.Contains(actionAdapter) && adapter.InputSignals.Overlaps(actionAdapter.OutputSignals))
                    AddActionAdapter(actionAdapter, dependencyChain, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
            }
        }

        ///// <summary>
        ///// Determines the set of adapters in the dependency chain that produces the set of signals in the
        ///// <paramref name="inputSignalsRestriction"/> and returns the set of input signals required by the
        ///// adapters in the chain and the set of output signals produced by the adapters in the chain.
        ///// </summary>
        ///// <param name="inputSignalsRestriction">The set of signals that must be produced by the dependency chain.</param>
        ///// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        ///// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        ///// <param name="inputSignals">The set of input signals required by the adapters in the dependency chain.</param>
        ///// <param name="outputSignals">The set of output signals produced by the adapters in the dependency chain.</param>
        //protected virtual void TraverseMeasurementKeyDependencyChain(ISet<Guid> inputSignalsRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, out ISet<Guid> inputSignals, out ISet<Guid> outputSignals)
        //{
        //    List<IAdapter> producerAdapters = (inputAdapterCollection ?? Enumerable.Empty<IAdapter>())
        //            .Concat(actionAdapterCollection ?? Enumerable.Empty<IAdapter>())
        //            .ToList();

        //    ISet<IAdapter> flattenedAdapterChain = new HashSet<IAdapter>();

        //    ISet<Guid> currentInputSignals = new HashSet<Guid>(inputSignalsRestriction);
        //    List<IAdapter> currentLink;
        //    int adapterCount;

        //    do
        //    {
        //        // Get the number of adapters in the chain before
        //        // augmenting the amount with the next link in the chain
        //        adapterCount = flattenedAdapterChain.Count;

        //        // Get the adapters which are in the next link in the chain
        //        currentLink = producerAdapters
        //            .Where(adapter => adapter.OutputSignals.Overlaps(currentInputSignals))
        //            .ToList();

        //        // Get the set of input signals for the current link in the chain
        //        currentInputSignals.Clear();
        //        currentInputSignals.UnionWith(currentLink.SelectMany(adapter => adapter.InputSignals));

        //        // Add the adapters in the current link of the chain
        //        // to the flattened set of adapters in the chain
        //        flattenedAdapterChain.UnionWith(currentLink);
        //    }
        //    while (flattenedAdapterChain.Count != adapterCount);

        //    // Build the set of input signals and output signals for all the adapters in the chain
        //    inputSignals = new HashSet<Guid>(flattenedAdapterChain.SelectMany(adapter => adapter.InputSignals));
        //    outputSignals = new HashSet<Guid>(flattenedAdapterChain.SelectMany(adapter => adapter.OutputSignals));
        //}

        #endregion
    }
}
