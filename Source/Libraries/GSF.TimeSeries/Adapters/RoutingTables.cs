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
using GSF.Collections;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the routing tables for the Iaon adapters.
    /// </summary>
    public class RoutingTables : IDisposable
    {
        #region [ Members ]

        // Nested Types

        private class DependencyMeasurement
        {
            public ISet<IAdapter> Dependencies = new HashSet<IAdapter>();
            public ISet<IAdapter> Notifications = new HashSet<IAdapter>();
            public IMeasurement Measurement;
        }

        // Events

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
        private Dictionary<MeasurementKey, List<IActionAdapter>> m_actionRoutes;
        private Dictionary<MeasurementKey, List<IOutputAdapter>> m_outputRoutes;
        private List<IActionAdapter> m_actionBroadcastRoutes;
        private List<IOutputAdapter> m_outputBroadcastRoutes;
        private ReaderWriterLockSlim m_adapterRoutesCacheLock;
        private AutoResetEvent m_calculationComplete;
        private object m_queuedCalculationPending;

        private AsyncQueue<Tuple<IAdapter, object>> m_dependencyOperationQueue;
        private volatile Dictionary<IAdapter, ISet<IAdapter>> m_dependencies;
        private volatile Dictionary<IAdapter, ISet<IAdapter>> m_backwardDependencies;
        private Dictionary<IAdapter, Dictionary<Guid, Queue<DependencyMeasurement>>> m_dependencyMeasurementsLookup;
        private Dictionary<IAdapter, IList<IMeasurement>> m_notifiedMeasurementLookup;
        private volatile int m_operationsUntilNextPublish;
        
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RoutingTables"/> class.
        /// </summary>
        public RoutingTables()
        {
            m_adapterRoutesCacheLock = new ReaderWriterLockSlim();
            m_calculationComplete = new AutoResetEvent(true);
            m_queuedCalculationPending = new object();

            m_dependencyOperationQueue = new AsyncQueue<Tuple<IAdapter, object>>();
            m_dependencies = new Dictionary<IAdapter, ISet<IAdapter>>();
            m_backwardDependencies = new Dictionary<IAdapter, ISet<IAdapter>>();
            m_dependencyMeasurementsLookup = new Dictionary<IAdapter, Dictionary<Guid, Queue<DependencyMeasurement>>>();
            m_notifiedMeasurementLookup = new Dictionary<IAdapter, IList<IMeasurement>>();

            m_dependencyOperationQueue.ProcessItemFunction = ProcessDependencyOperation;
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
                        m_actionRoutes = null;
                        m_outputRoutes = null;
                        m_actionBroadcastRoutes = null;
                        m_outputBroadcastRoutes = null;
                        m_inputAdapters = null;
                        m_actionAdapters = null;
                        m_outputAdapters = null;

                        if ((object)m_adapterRoutesCacheLock != null)
                            m_adapterRoutesCacheLock.Dispose();

                        m_adapterRoutesCacheLock = null;

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
                    m_disposed = true;  // Prevent duplicate dispose.
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
            IInputAdapter[] inputAdapterCollection = null;
            IActionAdapter[] actionAdapterCollection = null;
            IOutputAdapter[] outputAdapterCollection = null;
            bool retry = true;

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
                Dictionary<MeasurementKey, List<IActionAdapter>> actionRoutes = new Dictionary<MeasurementKey, List<IActionAdapter>>();
                Dictionary<MeasurementKey, List<IOutputAdapter>> outputRoutes = new Dictionary<MeasurementKey, List<IOutputAdapter>>();
                List<IActionAdapter> actionAdapters, actionBroadcastRoutes = new List<IActionAdapter>();
                List<IOutputAdapter> outputAdapters, outputBroadcastRoutes = new List<IOutputAdapter>();
                MeasurementKey[] measurementKeys;

                Dictionary<IAdapter, ISet<IAdapter>> dependencies = new Dictionary<IAdapter, ISet<IAdapter>>();
                Dictionary<IAdapter, ISet<IAdapter>> backwardDependencies = new Dictionary<IAdapter, ISet<IAdapter>>();

                if ((object)actionAdapterCollection != null)
                {
                    foreach (IActionAdapter actionAdapter in actionAdapterCollection)
                    {
                        // Make sure adapter is initialized before calculating route
                        if (actionAdapter.WaitForInitialize(actionAdapter.InitializationTimeout))
                        {
                            measurementKeys = actionAdapter.InputMeasurementKeys;

                            if ((object)measurementKeys != null)
                            {
                                foreach (MeasurementKey key in measurementKeys)
                                {
                                    if (!actionRoutes.TryGetValue(key, out actionAdapters))
                                    {
                                        actionAdapters = new List<IActionAdapter>();
                                        actionRoutes.Add(key, actionAdapters);
                                    }

                                    if (!actionAdapters.Contains(actionAdapter))
                                        actionAdapters.Add(actionAdapter);
                                }
                            }
                            else
                                actionBroadcastRoutes.Add(actionAdapter);

                            AddDependencies(actionAdapter, dependencies, backwardDependencies);
                        }
                        else
                            actionBroadcastRoutes.Add(actionAdapter);
                    }
                }

                if ((object)outputAdapterCollection != null)
                {
                    foreach (IOutputAdapter outputAdapter in outputAdapterCollection)
                    {
                        // Make sure adapter is initialized before calculating route
                        if (outputAdapter.WaitForInitialize(outputAdapter.InitializationTimeout))
                        {
                            measurementKeys = outputAdapter.InputMeasurementKeys;

                            if ((object)measurementKeys != null)
                            {
                                foreach (MeasurementKey key in measurementKeys)
                                {
                                    if (!outputRoutes.TryGetValue(key, out outputAdapters))
                                    {
                                        outputAdapters = new List<IOutputAdapter>();
                                        outputRoutes.Add(key, outputAdapters);
                                    }

                                    if (!outputAdapters.Contains(outputAdapter))
                                        outputAdapters.Add(outputAdapter);
                                }
                            }
                            else
                                outputBroadcastRoutes.Add(outputAdapter);

                            AddDependencies(outputAdapter, dependencies, backwardDependencies);
                        }
                        else
                            outputBroadcastRoutes.Add(outputAdapter);
                    }
                }

                m_dependencies = dependencies;
                m_backwardDependencies = backwardDependencies;

                // Synchronously update adapter routing cache
                m_adapterRoutesCacheLock.EnterWriteLock();

                try
                {
                    m_actionRoutes = actionRoutes;
                    m_outputRoutes = outputRoutes;
                    m_actionBroadcastRoutes = actionBroadcastRoutes;
                    m_outputBroadcastRoutes = outputBroadcastRoutes;
                }
                finally
                {
                    m_adapterRoutesCacheLock.ExitWriteLock();
                }

                // Start or stop any connect on demand adapters
                HandleConnectOnDemandAdapters((MeasurementKey[])state, inputAdapterCollection, actionAdapterCollection, outputAdapterCollection);
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

        private void AddDependencies(IAdapter adapter, Dictionary<IAdapter, ISet<IAdapter>> dependencies, Dictionary<IAdapter, ISet<IAdapter>> backwardDependencies)
        {
            IAdapter dependency = null;
            IActionAdapter actionAdapter;
            IOutputAdapter outputAdapter;
            string dependenciesSetting;

            ISet<IAdapter> adapterSet;

            if (adapter.Settings.TryGetValue("dependencies", out dependenciesSetting))
            {
                foreach (string adapterName in dependenciesSetting.Split(','))
                {
                    if (m_actionAdapters.TryGetAdapterByName(adapterName, out actionAdapter))
                        dependency = actionAdapter;
                    else if (m_outputAdapters.TryGetAdapterByName(adapterName, out outputAdapter))
                        dependency = outputAdapter;

                    if ((object)dependency != null)
                    {
                        // Add dependency adapter to collection of dependencies for adapter
                        if (!dependencies.TryGetValue(adapter, out adapterSet))
                        {
                            adapterSet = new HashSet<IAdapter>();
                            dependencies.Add(adapter, adapterSet);
                        }

                        adapterSet.Add(actionAdapter);

                        // Add adapter to collection of backward dependencies for dependency adapter
                        if (!backwardDependencies.TryGetValue(actionAdapter, out adapterSet))
                        {
                            adapterSet = new HashSet<IAdapter>();
                            backwardDependencies.Add(actionAdapter, adapterSet);
                        }

                        adapterSet.Add(adapter);
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for distributing new measurements in a routed fashion.
        /// </summary>
        /// <param name="sender">Event source reference to adapter that generated new measurements.</param>
        /// <param name="e">Event arguments containing a collection of new measurements.</param>
        /// <remarks>
        /// Time-series framework uses this handler to directly route new measurements to the action and output adapters.
        /// </remarks>
        public virtual void RoutedMeasurementsHandler(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            RoutedMeasurementsHandler(e.Argument);
        }

        /// <summary>
        /// Method for distributing new measurements in a routed fashion.
        /// </summary>
        /// <param name="newMeasurements">Collection of new measurements.</param>
        /// <remarks>
        /// Time-series framework uses this handler to directly route new measurements to the action and output adapters.
        /// </remarks>
        public virtual void RoutedMeasurementsHandler(IEnumerable<IMeasurement> newMeasurements)
        {
            if ((object)m_actionRoutes == null || (object)m_outputRoutes == null)
                return;

            Dictionary<IAdapter, List<IMeasurement>> adapterMeasurementsLookup = new Dictionary<IAdapter, List<IMeasurement>>();

            List<IActionAdapter> actionRoutes;
            List<IOutputAdapter> outputRoutes;
            List<IMeasurement> measurements;
            ISet<IAdapter> adapters;
            ISet<IAdapter> dependencies;
            MeasurementKey key;
            DependencyMeasurement dependencyMeasurement;

            m_adapterRoutesCacheLock.EnterReadLock();

            try
            {
                // Loop through each new measurement and look for destination routes
                foreach (IMeasurement measurement in newMeasurements)
                {
                    key = measurement.Key;

                    // Create the set of adapters to which this measurement is routed
                    adapters = new HashSet<IAdapter>();
                    m_actionBroadcastRoutes.ForEach(adapter => adapters.Add(adapter));
                    m_outputBroadcastRoutes.ForEach(adapter => adapters.Add(adapter));

                    if (m_actionRoutes.TryGetValue(key, out actionRoutes))
                    {
                        // Add action adapters to the adapter set
                        foreach (IActionAdapter actionAdapter in actionRoutes)
                            adapters.Add(actionAdapter);
                    }

                    if (m_outputRoutes.TryGetValue(key, out outputRoutes))
                    {
                        // Add output adapters to the adapter set
                        foreach (IOutputAdapter outputAdapter in outputRoutes)
                            adapters.Add(outputAdapter);
                    }

                    // Search for dependencies in each adapter receiving this measurement
                    foreach (IAdapter adapter in adapters)
                    {
                        // Get dependencies for the adapter
                        if (m_dependencies.TryGetValue(adapter, out dependencies))
                        {
                            // Intersect with the set of adapters to see if this measurement
                            // is also routed to the adapters it is dependent upon
                            dependencies = new HashSet<IAdapter>(dependencies);
                            dependencies.IntersectWith(adapters);

                            if (dependencies.Count > 0)
                            {
                                // Add a queuing operation to the dependency operation queue
                                dependencyMeasurement = new DependencyMeasurement()
                                {
                                    Dependencies = dependencies,
                                    Measurement = measurement
                                };

                                m_dependencyOperationQueue.Enqueue(Tuple.Create(adapter, (object)dependencyMeasurement));

                                continue;
                            }
                        }

                        // Dependencies are not receiving this measurement, so add this measurement to the list
                        if (!adapterMeasurementsLookup.TryGetValue(adapter, out measurements))
                        {
                            measurements = new List<IMeasurement>();
                            adapterMeasurementsLookup.Add(adapter, measurements);
                        }

                        measurements.Add(measurement);
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Error occurred in routed measurements handler: {0}", ex.Message), ex));
            }
            finally
            {
                m_adapterRoutesCacheLock.ExitReadLock();
            }

            // Send independent measurements
            foreach (KeyValuePair<IAdapter, List<IMeasurement>> pair in adapterMeasurementsLookup)
            {
                try
                {
                    QueueMeasurementsForProcessing(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("ERROR: Exception queuing data to adapter [{0}]: {1}", pair.Key.Name, ex.Message), ex));
                }
            }
        }

        private void QueueMeasurementsForProcessing(IAdapter adapter, IEnumerable<IMeasurement> measurements)
        {
            IActionAdapter actionAdapter;

            if (adapter.Enabled)
            {
                actionAdapter = adapter as IActionAdapter;

                if ((object)actionAdapter != null)
                    actionAdapter.QueueMeasurementsForProcessing(measurements);
                else
                    ((IOutputAdapter)adapter).QueueMeasurementsForProcessing(measurements);
            }
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

        /// <summary>
        /// Event handler for notifying dependent adapters of updates to measurements.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The measurement that was updated.</param>
        public void NotifyHandler(object sender, EventArgs<IMeasurement> args)
        {
            IAdapter adapter = sender as IAdapter;

            if ((object)adapter != null)
                m_dependencyOperationQueue.Enqueue(Tuple.Create(adapter, (object)args.Argument));
        }

        private void ProcessDependencyOperation(Tuple<IAdapter, object> operationParameters)
        {
            DependencyMeasurement dependencyMeasurement = operationParameters.Item2 as DependencyMeasurement;

            // Determine the type of operation based on the type of the parameters
            if ((object)dependencyMeasurement != null)
                ProcessQueue(operationParameters.Item1, dependencyMeasurement);
            else
                ProcessNotify(operationParameters.Item1, (IMeasurement)operationParameters.Item2);

            // Determine if we need to run publish and timeout operations.
            // To improve performance, we do not publish measurements every
            // time a notification is received. Instead, the number of operations
            // between each publication is determined by the number of operations
            // in the async queue after each publication. This allows the system
            // to perform more publications when the system is keeping up and
            // perform fewer when the system is falling behind while still
            // guaranteeing that all notified measurements get published.
            if (m_operationsUntilNextPublish == 0)
                m_operationsUntilNextPublish = m_dependencyOperationQueue.Count;
            else
                m_operationsUntilNextPublish--;

            if (m_operationsUntilNextPublish == 0)
            {
                ProcessTimeouts();
                PublishNotifiedMeasurements();
            }
        }

        private void ProcessQueue(IAdapter adapter, DependencyMeasurement dependencyMeasurement)
        {
            Dictionary<Guid, Queue<DependencyMeasurement>> queueLookup;
            Queue<DependencyMeasurement> dependencyMeasurements;
            Guid signalID = dependencyMeasurement.Measurement.ID;

            // Get the lookup table for the adapter's dependency measurements
            if (!m_dependencyMeasurementsLookup.TryGetValue(adapter, out queueLookup))
            {
                queueLookup = new Dictionary<Guid, Queue<DependencyMeasurement>>();
                m_dependencyMeasurementsLookup.Add(adapter, queueLookup);
            }

            // Get the queue of dependency measurements for this signal
            if (!queueLookup.TryGetValue(signalID, out dependencyMeasurements))
            {
                dependencyMeasurements = new Queue<DependencyMeasurement>();
                queueLookup.Add(signalID, dependencyMeasurements);
            }

            // Add the dependency measurement to the queue
            dependencyMeasurements.Enqueue(dependencyMeasurement);
        }

        private void ProcessNotify(IAdapter notifier, IMeasurement processedMeasurement)
        {
            ISet<IAdapter> dependentAdapters;
            Dictionary<Guid, Queue<DependencyMeasurement>> queueLookup;
            Queue<DependencyMeasurement> dependencyMeasurements;
            IList<IMeasurement> notifiedMeasurements;
            DependencyMeasurement dependencyMeasurement;
            DependencyMeasurement dequeuedMeasurement;

            // Look up dependent adapters using reverse lookup table
            if (!m_backwardDependencies.TryGetValue(notifier, out dependentAdapters))
                return;

            // Determine if notification means that all dependencies have been met for any dependent adapter
            foreach (IAdapter dependentAdapter in dependentAdapters)
            {
                dequeuedMeasurement = null;

                // Look up collection of queues for the dependent adapter
                if (!m_dependencyMeasurementsLookup.TryGetValue(dependentAdapter, out queueLookup))
                    continue;

                // Look up the specific queue for this signal
                if (!queueLookup.TryGetValue(processedMeasurement.ID, out dependencyMeasurements))
                    continue;

                // Attempt to find the notified measurement in the queue
                dependencyMeasurement = dependencyMeasurements.FirstOrDefault(depMeasurement => (object)depMeasurement.Measurement == (object)processedMeasurement);

                if ((object)dependencyMeasurement == null)
                    continue;

                // Add the notification to the set of notifications for that measurement
                dependencyMeasurement.Notifications.Add(notifier);

                // Check to see if all dependencies have been met
                if (dependencyMeasurement.Dependencies.Count != dependencyMeasurement.Notifications.Count)
                    continue;

                // Get the collection of measurements that have been notified since the last adapter queueing operation
                if (!m_notifiedMeasurementLookup.TryGetValue(dependentAdapter, out notifiedMeasurements))
                {
                    notifiedMeasurements = new List<IMeasurement>();
                    m_notifiedMeasurementLookup.Add(dependentAdapter, notifiedMeasurements);
                }

                // If the measurement that was notified is not the first measurement in the queue,
                // assume that all the ones that came before it have timed out
                while (dequeuedMeasurement != dependencyMeasurement)
                {
                    dequeuedMeasurement = dependencyMeasurements.Dequeue();
                    notifiedMeasurements.Add(dequeuedMeasurement.Measurement);
                }
            }
        }

        private void ProcessTimeouts()
        {
            long now = PrecisionTimer.UtcNow.Ticks;

            IAdapter adapter;
            DependencyMeasurement dequeuedMeasurement;
            IList<IMeasurement> notifiedMeasurements = null;
            int timedOutMeasurements;

            // Search through all the queues to find all the measurements which have timed out
            foreach (KeyValuePair<IAdapter, Dictionary<Guid, Queue<DependencyMeasurement>>> pair in m_dependencyMeasurementsLookup)
            {
                adapter = pair.Key;

                foreach (Queue<DependencyMeasurement> measurementQueue in pair.Value.Values)
                {
                    // Determine the number of measurements in this queue which have timed out
                    timedOutMeasurements = measurementQueue.TakeWhile(depMeasurement => (now - depMeasurement.Measurement.Timestamp) > adapter.DependencyTimeout).Count();

                    // If there are any measurements that have timed out, get the collection
                    // of notified measurements for that adapter so we can put them in it
                    if (timedOutMeasurements > 0)
                    {
                        if (!m_notifiedMeasurementLookup.TryGetValue(adapter, out notifiedMeasurements))
                        {
                            notifiedMeasurements = new List<IMeasurement>();
                            m_notifiedMeasurementLookup.Add(adapter, notifiedMeasurements);
                        }
                    }

                    // Place the measurements in the queue.
                    // NOTE: If we are able to enter the loop, then notifiedMeasurements cannot be
                    //       null because the for-loop condition is the same as the if statement above.
                    for (int i = 0; i < timedOutMeasurements; i++)
                    {
                        dequeuedMeasurement = measurementQueue.Dequeue();
                        notifiedMeasurements.Add(dequeuedMeasurement.Measurement);
                    }
                }
            }
        }

        private void PublishNotifiedMeasurements()
        {
            // Go through each key-value pair and queue the measurements for the adapter
            foreach (KeyValuePair<IAdapter, IList<IMeasurement>> pair in m_notifiedMeasurementLookup)
                QueueMeasurementsForProcessing(pair.Key, pair.Value);

            // Clear out the collections of notified measurements and start over
            m_notifiedMeasurementLookup.Clear();
        }

        #endregion
    }
}
