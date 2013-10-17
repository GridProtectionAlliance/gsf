//******************************************************************************************************
//  RoutingTables.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//
//******************************************************************************************************

using GSF.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the routing tables for the Iaon adapters.
    /// </summary>
    public class RoutingTables : IDisposable
    {
        #region [ Members ]

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
                            m_calculationComplete.Close();
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
            try
            {
                // Pre-calculate internal routes to improve performance
                Dictionary<MeasurementKey, List<IActionAdapter>> actionRoutes = new Dictionary<MeasurementKey, List<IActionAdapter>>();
                Dictionary<MeasurementKey, List<IOutputAdapter>> outputRoutes = new Dictionary<MeasurementKey, List<IOutputAdapter>>();
                List<IActionAdapter> actionAdapters, actionBroadcastRoutes = new List<IActionAdapter>();
                List<IOutputAdapter> outputAdapters, outputBroadcastRoutes = new List<IOutputAdapter>();
                MeasurementKey[] measurementKeys;

                if ((object)m_actionAdapters != null)
                {
                    lock (m_actionAdapters)
                    {
                        foreach (IActionAdapter actionAdapter in m_actionAdapters)
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
                            }
                            else
                                actionBroadcastRoutes.Add(actionAdapter);
                        }
                    }
                }

                if ((object)m_outputAdapters != null)
                {
                    lock (m_outputAdapters)
                    {
                        foreach (IOutputAdapter outputAdapter in m_outputAdapters)
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
                            }
                            else
                                outputBroadcastRoutes.Add(outputAdapter);
                        }
                    }
                }

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
                HandleConnectOnDemandAdapters((MeasurementKey[])state);
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

            List<IActionAdapter> actionRoutes;
            List<IOutputAdapter> outputRoutes;
            Dictionary<IActionAdapter, List<IMeasurement>> actionMeasurements = new Dictionary<IActionAdapter, List<IMeasurement>>();
            Dictionary<IOutputAdapter, List<IMeasurement>> outputMeasurements = new Dictionary<IOutputAdapter, List<IMeasurement>>();
            List<IMeasurement> measurements;
            MeasurementKey key;

            m_adapterRoutesCacheLock.EnterReadLock();

            try
            {
                // Loop through each new measurement and look for destination routes
                foreach (IMeasurement measurement in newMeasurements)
                {
                    key = measurement.Key;

                    if (m_actionRoutes.TryGetValue(key, out actionRoutes))
                    {
                        // Add measurements for each destination action adapter route
                        foreach (IActionAdapter actionAdapter in actionRoutes)
                        {
                            if (!actionMeasurements.TryGetValue(actionAdapter, out measurements))
                            {
                                measurements = new List<IMeasurement>();
                                actionMeasurements.Add(actionAdapter, measurements);
                            }

                            measurements.Add(measurement);
                        }
                    }

                    if (m_outputRoutes.TryGetValue(key, out outputRoutes))
                    {
                        // Add measurements for each destination output adapter route
                        foreach (IOutputAdapter outputAdapter in outputRoutes)
                        {
                            if (!outputMeasurements.TryGetValue(outputAdapter, out measurements))
                            {
                                measurements = new List<IMeasurement>();
                                outputMeasurements.Add(outputAdapter, measurements);
                            }

                            measurements.Add(measurement);
                        }
                    }
                }

                // Send broadcast action measurements
                foreach (IActionAdapter actionAdapter in m_actionBroadcastRoutes)
                {
                    if (actionAdapter.Enabled)
                        actionAdapter.QueueMeasurementsForProcessing(newMeasurements);
                }

                // Send broadcast output measurements
                foreach (IOutputAdapter outputAdapter in m_outputBroadcastRoutes)
                {
                    if (outputAdapter.Enabled)
                        outputAdapter.QueueMeasurementsForProcessing(newMeasurements);
                }
            }
            finally
            {
                m_adapterRoutesCacheLock.ExitReadLock();
            }

            // Send routed action measurements
            foreach (KeyValuePair<IActionAdapter, List<IMeasurement>> actionAdapterMeasurements in actionMeasurements)
            {
                IActionAdapter actionAdapter = actionAdapterMeasurements.Key;

                if (actionAdapter.Enabled)
                    actionAdapter.QueueMeasurementsForProcessing(actionAdapterMeasurements.Value);
            }

            // Send routed output measurements
            foreach (KeyValuePair<IOutputAdapter, List<IMeasurement>> outputAdapterMeasurements in outputMeasurements)
            {
                IOutputAdapter outputAdapter = outputAdapterMeasurements.Key;

                if (outputAdapter.Enabled)
                    outputAdapter.QueueMeasurementsForProcessing(outputAdapterMeasurements.Value);
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
        /// <remarks>
        /// Set the <paramref name="inputMeasurementKeysRestriction"/> to null to use full adapter routing demands.
        /// </remarks>
        protected virtual void HandleConnectOnDemandAdapters(MeasurementKey[] inputMeasurementKeysRestriction)
        {
            IEnumerable<MeasurementKey> outputMeasurementKeys = null;
            IEnumerable<MeasurementKey> inputMeasurementKeys = null;
            MeasurementKey[] requestedOutputMeasurementKeys, requestedInputMeasurementKeys, emptyKeys = new MeasurementKey[0];

            if ((object)inputMeasurementKeysRestriction != null && inputMeasurementKeysRestriction.Any())
            {
                // When an input measurement keys restriction has been defined, extract the needed input and output measurement keys by
                // walking the dependency chain of the restriction
                TraverseMeasurementKeyDependencyChain(inputMeasurementKeysRestriction, out outputMeasurementKeys, out inputMeasurementKeys);
            }
            else
            {
                // Get the full list of output measurements keys that can be provided in this Iaon session
                if ((object)m_inputAdapters != null)
                    outputMeasurementKeys = m_inputAdapters.OutputMeasurementKeys();

                if ((object)m_actionAdapters != null)
                {
                    if ((object)outputMeasurementKeys == null || !outputMeasurementKeys.Any())
                    {
                        outputMeasurementKeys = m_actionAdapters.OutputMeasurementKeys();
                    }
                    else
                    {
                        IEnumerable<MeasurementKey> actionAdapterOutputMeasurementKeys = m_actionAdapters.OutputMeasurementKeys();

                        if ((object)actionAdapterOutputMeasurementKeys != null && actionAdapterOutputMeasurementKeys.Any())
                            outputMeasurementKeys = outputMeasurementKeys.Concat(actionAdapterOutputMeasurementKeys).Distinct();
                    }
                }

                // Get the full list of input measurements that can be demanded in this Iaon session
                if ((object)m_outputAdapters != null)
                    inputMeasurementKeys = m_outputAdapters.InputMeasurementKeys;

                if ((object)m_actionAdapters != null)
                {
                    if (inputMeasurementKeys == null || !inputMeasurementKeys.Any())
                    {
                        inputMeasurementKeys = m_actionAdapters.InputMeasurementKeys;
                    }
                    else
                    {
                        MeasurementKey[] actionAdapterInputMeasurementKeys = m_actionAdapters.InputMeasurementKeys;

                        if ((object)actionAdapterInputMeasurementKeys != null && actionAdapterInputMeasurementKeys.Length > 0)
                            inputMeasurementKeys = inputMeasurementKeys.Concat(actionAdapterInputMeasurementKeys).Distinct();
                    }
                }
            }

            // Handle connect on demand action adapters and output adapters based on currently provisioned output measurements
            if ((object)outputMeasurementKeys != null && outputMeasurementKeys.Any())
            {
                if ((object)m_actionAdapters != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IActionAdapter actionAdapter in m_actionAdapters)
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

                if ((object)m_outputAdapters != null)
                {
                    // Start or stop connect on demand output adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IOutputAdapter outputAdapter in m_outputAdapters)
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
                if ((object)m_actionAdapters != null)
                {
                    foreach (IActionAdapter actionAdapter in m_actionAdapters)
                    {
                        if (!actionAdapter.AutoStart && (object)actionAdapter.RequestedInputMeasurementKeys != null && actionAdapter.RespectInputDemands)
                            actionAdapter.RequestedInputMeasurementKeys = null;

                        // Do not start or stop adapter
                        // Adapter will be stopped or started later
                        // after calculating requested output measurements
                    }
                }

                // Handle special case of clearing requested input keys and stopping connect on demand output adapters when no output measurement keys are defined
                if ((object)m_outputAdapters != null)
                {
                    foreach (IOutputAdapter outputAdapter in m_outputAdapters)
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
                if ((object)m_actionAdapters != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IActionAdapter actionAdapter in m_actionAdapters)
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

                if ((object)m_inputAdapters != null)
                {
                    // Start or stop connect on demand input adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IInputAdapter inputAdapter in m_inputAdapters)
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
                if ((object)m_actionAdapters != null)
                {
                    foreach (IActionAdapter actionAdapter in m_actionAdapters)
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
                if ((object)m_inputAdapters != null)
                {
                    foreach (IInputAdapter inputAdapter in m_inputAdapters)
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
        /// <param name="outputMeasurementKeys">Dependent output measurement keys to return.</param>
        /// <param name="inputMeasurementKeys">Dependent input measurement keys to return.</param>
        protected virtual void TraverseMeasurementKeyDependencyChain(MeasurementKey[] inputMeasurementKeysRestriction, out IEnumerable<MeasurementKey> outputMeasurementKeys, out IEnumerable<MeasurementKey> inputMeasurementKeys)
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

                actionAdapters = FindDistinctAdapters<IActionAdapter>(m_actionAdapters, inputMeasurementKeyList, actionAdapterList);
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

                inputAdapters = FindDistinctAdapters<IInputAdapter>(m_inputAdapters, inputMeasurementKeyList, inputAdapterList);
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
