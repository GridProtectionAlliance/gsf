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
            public List<IAdapter> BroadcastAdapters;
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
        /// <param name="inputMeasurementKeysRestriction">Input measurement keys restriction.</param>
        /// <remarks>
        /// Set the <paramref name="inputMeasurementKeysRestriction"/> to null to use full adapter I/O routing demands.
        /// </remarks>
        public virtual void CalculateRoutingTables(MeasurementKey[] inputMeasurementKeysRestriction)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(QueueRoutingTableCalculation, inputMeasurementKeysRestriction);
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
                // Pre-calculate internal routes to improve performance
                IEnumerable<IAdapter> adapterCollection;

                Dictionary<Guid, ICollection<IAdapter>> destinationsLookup = new Dictionary<Guid, ICollection<IAdapter>>();
                Dictionary<IAdapter, ICollection<SignalRoute>> routesLookup = new Dictionary<IAdapter, ICollection<SignalRoute>>();
                Dictionary<Type, ICollection<SignalRoute>> routesLookupTemplates = new Dictionary<Type, ICollection<SignalRoute>>();
                List<IAdapter> broadcastAdapters = new List<IAdapter>();

                Func<Type, ICollection<SignalRoute>> templateFactory;
                ICollection<IAdapter> destinations;
                ICollection<SignalRoute> routes;

                MeasurementKey[] measurementKeys;
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
                if ((object)actionAdapterCollection != null && (object)outputAdapterCollection != null)
                    adapterCollection = actionAdapterCollection.AsEnumerable<IAdapter>().Concat(outputAdapterCollection);
                else if ((object)actionAdapterCollection != null)
                    adapterCollection = actionAdapterCollection;
                else if ((object)outputAdapterCollection != null)
                    adapterCollection = outputAdapterCollection;
                else
                    adapterCollection = Enumerable.Empty<IAdapter>();

                // Calculate possible routes for each action and output adapter
                foreach (IAdapter adapter in adapterCollection)
                {
                    // Make sure adapter is initialized before calculating route
                    if (adapter.Initialized)
                    {
                        measurementKeys = adapter.InputMeasurementKeys;
                        adapterType = adapter.GetType();

                        if ((object)measurementKeys != null)
                        {
                            // Add this adapter as a destination to each
                            // measurement defined in its input measurement keys
                            foreach (MeasurementKey key in measurementKeys)
                            {
                                destinations = destinationsLookup.GetOrAdd(key.SignalID, signalID => new List<IAdapter>());
                                destinations.Add(adapter);
                            }
                        }
                        else
                        {
                            // InputMeasurementKeys is null, therefore this adapter
                            // requests a broadcast of all measurements in the system
                            broadcastAdapters.Add(adapter);
                        }

                        // Determine which methods defined on the adapter type qualify as routes and store those in a lookup table
                        routes = routesLookupTemplates.GetOrAdd(adapterType, templateFactory)
                            .Select(route => new SignalRoute(route, adapter))
                            .ToList();

                        routesLookup.Add(adapter, routes);
                    }
                }

                // Add the broadcast adapters to all existing routes since
                // they will be receiving all measurements in the system
                foreach (ICollection<IAdapter> signalDestinations in destinationsLookup.Values)
                    broadcastAdapters.ForEach(adapter => signalDestinations.Add(adapter));

                // Update global lookup tables for routing
                m_globalCache = new GlobalCache()
                {
                    DestinationsLookup = destinationsLookup,
                    AdapterRoutesLookup = routesLookup,
                    BroadcastAdapters = broadcastAdapters,
                    CacheVersion = m_globalCache.CacheVersion + 1
                };

                // Start or stop any connect on demand adapters
                HandleConnectOnDemandAdapters((MeasurementKey[])state, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);

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
                            // This can occur if the adapter producing this measurement changes the type of the measurement at runtime.
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
                OnProcessException(new InvalidOperationException(string.Format("Error occurred in routed measurements handler: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Event handler for distributing new time-series entities in a broadcast fashion.
        /// </summary>
        /// <param name="sender">Event source reference to adapter that generated new time-series entities.</param>
        /// <param name="e">Event arguments containing a collection of new time-series entities.</param>
        /// <remarks>
        /// Time-series framework uses this handler to route new time-series entities to the action and output adapters; adapter will handle filtering.
        /// </remarks>
        public virtual void BroadcastEventHandler(object sender, RoutingEventArgs e)
        {
            // TODO: Decide if this is still useful
        }

        private ICollection<SignalRoute> FindAndCacheSignalRoutes(ITimeSeriesEntity timeSeriesEntity, GlobalCache globalCache, RoutingEventArgs localCache)
        {
            List<SignalRoute> signalRoutes = new List<SignalRoute>();
            Type timeSeriesEntityType = timeSeriesEntity.GetType();

            ICollection<IAdapter> destinations;
            ICollection<SignalRoute> localAdapterRoutes;
            ICollection<SignalRoute> globalAdapterRoutes;
            List<SignalRoute> matchingRoutes;

            if (!globalCache.DestinationsLookup.TryGetValue(timeSeriesEntity.ID, out destinations))
                destinations = globalCache.BroadcastAdapters;

            foreach (IAdapter destination in destinations)
            {
                // Check the local cache for routes first
                localAdapterRoutes = localCache.AdapterRoutes.GetOrAdd(destination, adapter => new List<SignalRoute>());

                // Get routes where the signal type is compatible with this time-series entity
                matchingRoutes = localAdapterRoutes
                    .Where(route => !route.ProcessingMethod.IsGenericMethod)
                    .Where(route => route.SignalType.IsAssignableFrom(timeSeriesEntityType))
                    .ToList();

                // Fall back on generic routes where the signal type is compatible
                if (matchingRoutes.Count == 0)
                {
                    matchingRoutes = localAdapterRoutes
                        .Where(route => route.ProcessingMethod.IsGenericMethod)
                        .Where(route => route.SignalType.IsAssignableFrom(timeSeriesEntityType))
                        .ToList();
                }

                if (matchingRoutes.Count == 0 && globalCache.AdapterRoutesLookup.TryGetValue(destination, out globalAdapterRoutes))
                {
                    // Get global routes where the signal type is compatible with this time-series entity
                    matchingRoutes = globalAdapterRoutes.Where(route => route.SignalType.IsAssignableFrom(timeSeriesEntityType)).ToList();

                    if (matchingRoutes.Count == 0)
                    {
                        // If all else fails, attempt to create a matching generic method
                        matchingRoutes = globalAdapterRoutes
                            .Select(route => route.MakeGenericSignalRoute(timeSeriesEntityType))
                            .Where(route => (object)route != null)
                            .ToList();
                    }

                    if (matchingRoutes.Count > 0)
                    {
                        // Add the global routes that were found to the local cache
                        foreach (SignalRoute route in matchingRoutes)
                            localAdapterRoutes.Add(new SignalRoute(route));
                    }
                }

                if (matchingRoutes.Count != 0)
                {
                    // In the case of multiple matches, get routes with the most defined signal types, where it is not compatible with any other type in the list
                    matchingRoutes = matchingRoutes.Where(r1 => !matchingRoutes.Any(r2 => r1 != r2 && r1.SignalType.IsAssignableFrom(r2.SignalType))).ToList();

                    // There should only be one route, except in the
                    // case of ambiguous matches or no matches
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
        /// Starts or stops connect on demand adapters based on current state of demanded input or output measurements.
        /// </summary>
        /// <param name="inputMeasurementKeysRestriction">Input measurement keys restriction.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputAdapterCollection">Collection of output adapters at start of routing table calculation.</param>
        /// <remarks>
        /// Set the <paramref name="inputMeasurementKeysRestriction"/> to null to use full adapter routing demands.
        /// </remarks>
        protected virtual void HandleConnectOnDemandAdapters(MeasurementKey[] inputMeasurementKeysRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, IOutputAdapter[] outputAdapterCollection)
        {
            IEnumerable<MeasurementKey> outputMeasurementKeys = null;
            IEnumerable<MeasurementKey> inputMeasurementKeys = null;
            MeasurementKey[] requestedOutputMeasurementKeys, requestedInputMeasurementKeys, emptyKeys = new MeasurementKey[0];

            if ((object)inputMeasurementKeysRestriction != null && inputMeasurementKeysRestriction.Any())
            {
                // When an input measurement keys restriction has been defined, extract the needed input and output measurement keys by
                // walking the dependency chain of the restriction
                TraverseMeasurementKeyDependencyChain(inputMeasurementKeysRestriction, inputAdapterCollection, actionAdapterCollection, out outputMeasurementKeys, out inputMeasurementKeys);
            }
            else
            {
                // Get the full list of output measurements keys that can be provided in this Iaon session
                if ((object)inputAdapterCollection != null)
                    outputMeasurementKeys = inputAdapterCollection.OutputMeasurementKeys();

                if ((object)actionAdapterCollection != null)
                {
                    if ((object)outputMeasurementKeys == null || !outputMeasurementKeys.Any())
                    {
                        outputMeasurementKeys = actionAdapterCollection.OutputMeasurementKeys();
                    }
                    else
                    {
                        IEnumerable<MeasurementKey> actionAdapterOutputMeasurementKeys = actionAdapterCollection.OutputMeasurementKeys();

                        if ((object)actionAdapterOutputMeasurementKeys != null && actionAdapterOutputMeasurementKeys.Any())
                            outputMeasurementKeys = outputMeasurementKeys.Concat(actionAdapterOutputMeasurementKeys).Distinct();
                    }
                }

                // Get the full list of input measurements that can be demanded in this Iaon session
                if ((object)outputAdapterCollection != null)
                    inputMeasurementKeys = outputAdapterCollection.InputMeasurementKeys();

                if ((object)actionAdapterCollection != null)
                {
                    if (inputMeasurementKeys == null || !inputMeasurementKeys.Any())
                    {
                        inputMeasurementKeys = actionAdapterCollection.InputMeasurementKeys();
                    }
                    else
                    {
                        MeasurementKey[] actionAdapterInputMeasurementKeys = actionAdapterCollection.InputMeasurementKeys();

                        if ((object)actionAdapterInputMeasurementKeys != null && actionAdapterInputMeasurementKeys.Length > 0)
                            inputMeasurementKeys = inputMeasurementKeys.Concat(actionAdapterInputMeasurementKeys).Distinct();
                    }
                }
            }

            // Handle connect on demand action adapters and output adapters based on currently provisioned output measurements
            if ((object)outputMeasurementKeys != null && outputMeasurementKeys.Any())
            {
                if ((object)actionAdapterCollection != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                    {
                        if (!actionAdapter.AutoStart && actionAdapter.RespectInputDemands)
                        {
                            // Create an intersection between the measurements the adapter can handle and those that are demanded throughout this Iaon session
                            if ((object)actionAdapter.InputMeasurementKeys != null && actionAdapter.InputMeasurementKeys.Length > 0)
                                requestedInputMeasurementKeys = actionAdapter.InputMeasurementKeys.Intersect(outputMeasurementKeys).ToArray();
                            else
                                requestedInputMeasurementKeys = emptyKeys;

                            // Only update requested input keys if they have changed since adapters may use this as a notification to resubscribe to needed data
                            if (actionAdapter.RequestedInputMeasurementKeys.CompareTo(requestedInputMeasurementKeys) != 0)
                                actionAdapter.RequestedInputMeasurementKeys = requestedInputMeasurementKeys;

                            // Do not start or stop adapter
                            // Adapter will be stopped or started later
                            // after calculating requested output measurements
                        }
                    }
                }

                if ((object)outputAdapterCollection != null)
                {
                    // Start or stop connect on demand output adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                    {
                        if (!outputAdapter.AutoStart)
                        {
                            // Create an intersection between the measurements the adapter can handle and those that are demanded throughout this Iaon session
                            if ((object)outputAdapter.InputMeasurementKeys != null && outputAdapter.InputMeasurementKeys.Length > 0)
                                requestedInputMeasurementKeys = outputAdapter.InputMeasurementKeys.Intersect(outputMeasurementKeys).ToArray();
                            else
                                requestedInputMeasurementKeys = emptyKeys;

                            // Only update requested input keys if they have changed since adapters may use this as a notification to resubscribe to needed data
                            if (outputAdapter.RequestedInputMeasurementKeys.CompareTo(requestedInputMeasurementKeys) != 0)
                                outputAdapter.RequestedInputMeasurementKeys = requestedInputMeasurementKeys;

                            // Start or stop adapter
                            outputAdapter.Enabled = ((object)outputAdapter.RequestedInputMeasurementKeys != null && outputAdapter.RequestedInputMeasurementKeys.Length > 0);
                        }
                    }
                }
            }
            else
            {
                // Handle special case of clearing requested input keys for connect on demand action adapters when no output measurement keys are defined
                if ((object)actionAdapterCollection != null)
                {
                    foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                    {
                        if (!actionAdapter.AutoStart && (object)actionAdapter.RequestedInputMeasurementKeys != null && actionAdapter.RespectInputDemands)
                            actionAdapter.RequestedInputMeasurementKeys = null;

                        // Do not start or stop adapter
                        // Adapter will be stopped or started later
                        // after calculating requested output measurements
                    }
                }

                // Handle special case of clearing requested input keys and stopping connect on demand output adapters when no output measurement keys are defined
                if ((object)outputAdapterCollection != null)
                {
                    foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                    {
                        if (!outputAdapter.AutoStart)
                        {
                            if ((object)outputAdapter.RequestedInputMeasurementKeys != null)
                                outputAdapter.RequestedInputMeasurementKeys = null;

                            outputAdapter.Enabled = false;
                        }
                    }
                }
            }

            // Handle connect on demand action adapters and input adapters based on currently demanded input measurements
            if ((object)inputMeasurementKeys != null && inputMeasurementKeys.Any())
            {
                if ((object)actionAdapterCollection != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                    {
                        if (!actionAdapter.AutoStart)
                        {
                            if (actionAdapter.RespectOutputDemands)
                            {
                                // Create an intersection between the measurements the adapter can provide and those that are demanded throughout this Iaon session
                                if ((object)actionAdapter.OutputMeasurements != null && actionAdapter.OutputMeasurements.Length > 0)
                                    requestedOutputMeasurementKeys = actionAdapter.OutputMeasurementKeys().Intersect(inputMeasurementKeys).ToArray();
                                else
                                    requestedOutputMeasurementKeys = emptyKeys;

                                // Only update requested output keys if they have changed since adapters may use this as a notification to resubscribe to needed data
                                if (actionAdapter.RequestedOutputMeasurementKeys.CompareTo(requestedOutputMeasurementKeys) != 0)
                                    actionAdapter.RequestedOutputMeasurementKeys = requestedOutputMeasurementKeys;
                            }

                            // Start or stop adapter, action adapter should only be stopped if it also has no requested input measurements keys, as determined prior
                            if (actionAdapter.RespectOutputDemands && (object)actionAdapter.RequestedOutputMeasurementKeys != null && actionAdapter.RequestedOutputMeasurementKeys.Length > 0)
                                actionAdapter.Enabled = true;
                            else
                                actionAdapter.Enabled = (actionAdapter.RespectInputDemands && (object)actionAdapter.RequestedInputMeasurementKeys != null && actionAdapter.RequestedInputMeasurementKeys.Length > 0);
                        }
                    }
                }

                if ((object)inputAdapterCollection != null)
                {
                    // Start or stop connect on demand input adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                    {
                        if (!inputAdapter.AutoStart)
                        {
                            // Create an intersection between the measurements the adapter can provide and those that are demanded throughout this Iaon session
                            if ((object)inputAdapter.OutputMeasurements != null && inputAdapter.OutputMeasurements.Length > 0)
                                requestedOutputMeasurementKeys = inputAdapter.OutputMeasurementKeys().Intersect(inputMeasurementKeys).ToArray();
                            else
                                requestedOutputMeasurementKeys = emptyKeys;

                            // Only update requested output keys if they have changed since adapters may use this as a notification to resubscribe to needed data
                            if (inputAdapter.RequestedOutputMeasurementKeys.CompareTo(requestedOutputMeasurementKeys) != 0)
                                inputAdapter.RequestedOutputMeasurementKeys = requestedOutputMeasurementKeys;

                            // Start or stop adapter
                            inputAdapter.Enabled = ((object)inputAdapter.RequestedOutputMeasurementKeys != null && inputAdapter.RequestedOutputMeasurementKeys.Length > 0);
                        }
                    }
                }
            }
            else
            {
                // Handle special case of clearing requested output keys and stopping connect on demand action adapters when no input measurement keys are defined
                if ((object)actionAdapterCollection != null)
                {
                    foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                    {
                        if (!actionAdapter.AutoStart)
                        {
                            if (actionAdapter.RespectOutputDemands)
                            {
                                if ((object)actionAdapter.RequestedOutputMeasurementKeys != null)
                                    actionAdapter.RequestedOutputMeasurementKeys = null;
                            }

                            // Action adapter should be stopped if it has no requested input measurements keys, as determined prior
                            if (!(actionAdapter.RespectInputDemands && (object)actionAdapter.RequestedInputMeasurementKeys != null && actionAdapter.RequestedInputMeasurementKeys.Length > 0))
                                actionAdapter.Enabled = false;
                        }
                    }
                }

                // Handle special case of clearing requested output keys and stopping connect on demand input adapters when no input measurement keys are defined
                if ((object)inputAdapterCollection != null)
                {
                    foreach (IInputAdapter inputAdapter in inputAdapterCollection)
                    {
                        if (!inputAdapter.AutoStart)
                        {
                            if ((object)inputAdapter.RequestedOutputMeasurementKeys != null)
                                inputAdapter.RequestedOutputMeasurementKeys = null;

                            inputAdapter.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the input and output measurement key dependency chain based on a set of input measurement keys restriction.
        /// </summary>
        /// <param name="inputMeasurementKeysRestriction">Input measurement keys restriction.</param>
        /// <param name="inputAdapterCollection">Collection of input adapters at start of routing table calculation.</param>
        /// <param name="actionAdapterCollection">Collection of action adapters at start of routing table calculation.</param>
        /// <param name="outputMeasurementKeys">Dependent output measurement keys to return.</param>
        /// <param name="inputMeasurementKeys">Dependent input measurement keys to return.</param>
        protected virtual void TraverseMeasurementKeyDependencyChain(MeasurementKey[] inputMeasurementKeysRestriction, IInputAdapter[] inputAdapterCollection, IActionAdapter[] actionAdapterCollection, out IEnumerable<MeasurementKey> outputMeasurementKeys, out IEnumerable<MeasurementKey> inputMeasurementKeys)
        {
            List<MeasurementKey> inputMeasurementKeyList = new List<MeasurementKey>(inputMeasurementKeysRestriction);
            outputMeasurementKeys = null;

            List<IActionAdapter> actionAdapterList = new List<IActionAdapter>();
            IEnumerable<IActionAdapter> actionAdapters = null;

            // Keep walking dependency chain until all subordinate action adapters have been found
            do
            {
                if ((object)actionAdapters != null)
                {
                    actionAdapterList.AddRange(actionAdapters);

                    IEnumerable<MeasurementKey> newInputKeys = actionAdapterList.InputMeasurementKeys();

                    if ((object)newInputKeys != null && newInputKeys.Any())
                        inputMeasurementKeyList = inputMeasurementKeyList.Concat(newInputKeys).Distinct().ToList();
                }

                actionAdapters = FindDistinctAdapters(actionAdapterCollection, inputMeasurementKeyList, actionAdapterList);
            }
            while ((object)actionAdapters != null);

            if (actionAdapterList.Count > 0)
                outputMeasurementKeys = actionAdapterList.OutputMeasurementKeys();

            List<IInputAdapter> inputAdapterList = new List<IInputAdapter>();
            IEnumerable<IInputAdapter> inputAdapters = null;

            // Keep walking dependency chain until all subordinate input adapters have been found
            do
            {
                if ((object)inputAdapters != null)
                {
                    inputAdapterList.AddRange(inputAdapters);

                    IEnumerable<MeasurementKey> newInputKeys = inputAdapterList.InputMeasurementKeys();

                    if ((object)newInputKeys != null && newInputKeys.Any())
                        inputMeasurementKeyList = inputMeasurementKeyList.Concat(newInputKeys).Distinct().ToList();
                }

                inputAdapters = FindDistinctAdapters(inputAdapterCollection, inputMeasurementKeyList, inputAdapterList);
            }
            while ((object)inputAdapters != null);

            if (inputAdapterList.Count > 0)
            {
                if ((object)outputMeasurementKeys == null)
                    outputMeasurementKeys = inputAdapterList.OutputMeasurementKeys();
                else
                    outputMeasurementKeys = outputMeasurementKeys.Concat(inputAdapterList.OutputMeasurementKeys()).Distinct();
            }

            inputMeasurementKeys = inputMeasurementKeyList;
        }

        // Find all the distinct adapters (i.e., that haven't been found already) in the source list for the given input measurement keys list
        private IEnumerable<T> FindDistinctAdapters<T>(IEnumerable<T> sourceCollection, IEnumerable<MeasurementKey> inputMeasurementKeysList, IEnumerable<T> existingList) where T : IAdapter
        {
            IEnumerable<T> adapters = null;

            if ((object)sourceCollection != null)
            {
                lock (sourceCollection)
                {
                    adapters = sourceCollection.Where(adapter => !existingList.Contains(adapter) && adapter.OutputMeasurementKeys().Any(inputMeasurementKeysList.Contains));
                }

                if (!adapters.Any())
                    adapters = null;
            }

            return adapters;
        }

        #endregion
    }
}
