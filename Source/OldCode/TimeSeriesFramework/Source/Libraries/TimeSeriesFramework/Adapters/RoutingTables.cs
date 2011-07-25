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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TVA;

namespace TimeSeriesFramework.Adapters
{
    /// <summary>
    /// Represents the routing tables for the Iaon adapters.
    /// </summary>
    public class RoutingTables : IDisposable
    {
        #region [ Members ]

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

                        if (m_adapterRoutesCacheLock != null)
                            m_adapterRoutesCacheLock.Dispose();

                        m_adapterRoutesCacheLock = null;

                        if (m_calculationComplete != null)
                            m_calculationComplete.Dispose();

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
        public virtual void CalculateRoutingTables()
        {
            ThreadPool.QueueUserWorkItem(QueueRoutingTableCalculation);
        }

        private void QueueRoutingTableCalculation(object state)
        {
            // Queue up a routing table calculation unless another thread has already requested one
            if (Monitor.TryEnter(m_queuedCalculationPending))
            {
                try
                {
                    // Queue new routing table calculation after waiting for any prior calculation to complete
                    if (m_calculationComplete.WaitOne())
                        ThreadPool.QueueUserWorkItem(CalculateRoutingTables);
                }
                finally
                {
                    Monitor.Exit(m_queuedCalculationPending);
                }
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

                if (m_actionAdapters != null)
                {
                    lock (m_actionAdapters)
                    {
                        foreach (IActionAdapter actionAdapter in m_actionAdapters)
                        {
                            // Make sure adapter is initialized before calculating route
                            if (actionAdapter.WaitForInitialize(actionAdapter.InitializationTimeout))
                            {
                                measurementKeys = actionAdapter.InputMeasurementKeys;

                                if (measurementKeys != null)
                                {
                                    foreach (MeasurementKey key in actionAdapter.InputMeasurementKeys)
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

                if (m_outputAdapters != null)
                {
                    lock (m_outputAdapters)
                    {
                        foreach (IOutputAdapter outputAdapter in m_outputAdapters)
                        {
                            // Make sure adapter is initialized before calculating route
                            if (outputAdapter.WaitForInitialize(outputAdapter.InitializationTimeout))
                            {
                                measurementKeys = outputAdapter.InputMeasurementKeys;

                                if (measurementKeys != null)
                                {
                                    foreach (MeasurementKey key in outputAdapter.InputMeasurementKeys)
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
                HandleConnectOnDemandAdapters();
            }
            finally
            {
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
            if (m_actionRoutes == null || m_outputRoutes == null)
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
        /// Starts or stops connect on demand adapters based on current state of demanded input or output measurements.
        /// </summary>
        protected virtual void HandleConnectOnDemandAdapters()
        {
            InputAdapterCollection inputAdapters;
            ActionAdapterCollection actionAdapters;
            OutputAdapterCollection outputAdapters;

            m_adapterRoutesCacheLock.EnterReadLock();

            try
            {
                inputAdapters = m_inputAdapters;
                actionAdapters = m_actionAdapters;
                outputAdapters = m_outputAdapters;
            }
            finally
            {
                m_adapterRoutesCacheLock.ExitReadLock();
            }

            // Get the full list of output measurements that can be provided in this Iaon session
            IEnumerable<MeasurementKey> outputMeasurementKeys = null;

            if (inputAdapters != null)
                outputMeasurementKeys = inputAdapters.OutputMeasurements.Select(m => m.Key);

            if (actionAdapters != null)
            {
                if (outputMeasurementKeys == null)
                    outputMeasurementKeys = actionAdapters.OutputMeasurements.Select(m => m.Key);
                else
                    outputMeasurementKeys = outputMeasurementKeys.Concat(actionAdapters.OutputMeasurements.Select(m => m.Key)).Distinct();
            }

            // Handle connect on demand action adapters and output adapters based on currently provisioned output measurements
            if (outputMeasurementKeys != null)
            {
                IEnumerable<MeasurementKey> demandedKeys;

                if (actionAdapters != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IActionAdapter actionAdapter in actionAdapters)
                    {
                        if (!actionAdapter.AutoStart)
                        {
                            if (actionAdapter.InputMeasurementKeys != null)
                            {
                                // Create an intersection between the measurements the adapter can handle and those that are demanded throughout this Iaon session
                                demandedKeys = actionAdapter.InputMeasurementKeys.Intersect(outputMeasurementKeys);

                                if (demandedKeys != null)
                                {
                                    StringBuilder requestedInputMeasurementKeys = new StringBuilder();

                                    foreach (MeasurementKey key in demandedKeys)
                                    {
                                        if (requestedInputMeasurementKeys.Length > 0)
                                            requestedInputMeasurementKeys.Append(";");

                                        requestedInputMeasurementKeys.Append(key.ToString());
                                    }

                                    if (requestedInputMeasurementKeys.Length > 0)
                                    {
                                        // Let action adapter know which input measurements are being provided and start adapter
                                        actionAdapter.RequestedInputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(actionAdapter.DataSource, requestedInputMeasurementKeys.ToString());
                                        actionAdapter.Enabled = true;
                                    }
                                    else
                                    {
                                        actionAdapter.RequestedInputMeasurementKeys = null;
                                        // Action adapter should only be stopped if it also has no requested output measurements keys, which will be determined later
                                    }
                                }
                                else
                                {
                                    actionAdapter.RequestedInputMeasurementKeys = null;
                                    // Action adapter should only be stopped if it also has no requested output measurements keys, which will be determined later
                                }
                            }
                            else
                            {
                                actionAdapter.RequestedInputMeasurementKeys = null;
                                // Action adapter should only be stopped if it also has no requested output measurements keys, which will be determined later
                            }
                        }
                    }
                }

                if (outputAdapters != null)
                {
                    // Start or stop connect on demand output adapters based on need, i.e., they handle any of the currently created output measurements
                    foreach (IOutputAdapter outputAdapter in outputAdapters)
                    {
                        if (!outputAdapter.AutoStart)
                        {
                            if (outputAdapter.InputMeasurementKeys != null)
                            {
                                // Create an intersection between the measurements the adapter can handle and those that are demanded throughout this Iaon session
                                demandedKeys = outputAdapter.InputMeasurementKeys.Intersect(outputMeasurementKeys);

                                if (demandedKeys != null)
                                {
                                    StringBuilder requestedInputMeasurementKeys = new StringBuilder();

                                    foreach (MeasurementKey key in demandedKeys)
                                    {
                                        if (requestedInputMeasurementKeys.Length > 0)
                                            requestedInputMeasurementKeys.Append(";");

                                        requestedInputMeasurementKeys.Append(key.ToString());
                                    }

                                    if (requestedInputMeasurementKeys.Length > 0)
                                    {
                                        // Let output adapter know which input measurements are being provided and start adapter
                                        outputAdapter.RequestedInputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(outputAdapter.DataSource, requestedInputMeasurementKeys.ToString());
                                        outputAdapter.Enabled = true;
                                    }
                                    else
                                    {
                                        outputAdapter.RequestedInputMeasurementKeys = null;
                                        outputAdapter.Enabled = false;
                                    }
                                }
                                else
                                {
                                    outputAdapter.RequestedInputMeasurementKeys = null;
                                    outputAdapter.Enabled = false;
                                }
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

            // Get the full list of input measurements that can are demanded in this Iaon session
            IEnumerable<MeasurementKey> inputMeasurementKeys = null;

            if (outputAdapters != null)
                inputMeasurementKeys = outputAdapters.InputMeasurementKeys;

            if (actionAdapters != null)
            {
                if (inputMeasurementKeys == null)
                    inputMeasurementKeys = actionAdapters.InputMeasurementKeys;
                else
                    inputMeasurementKeys = inputMeasurementKeys.Concat(actionAdapters.InputMeasurementKeys).Distinct();
            }

            // Handle connect on demand action adapters and input adapters based on currently demanded input measurements
            if (inputMeasurementKeys != null)
            {
                IEnumerable<MeasurementKey> demandedKeys;

                if (actionAdapters != null)
                {
                    // Start or stop connect on demand action adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IActionAdapter actionAdapter in actionAdapters)
                    {
                        if (!actionAdapter.AutoStart)
                        {
                            if (actionAdapter.OutputMeasurements != null)
                            {
                                // Create an intersection between the measurements the adapter can provide and those that are demanded throughout this Iaon session
                                demandedKeys = actionAdapter.OutputMeasurements.Select(m => m.Key).Intersect(inputMeasurementKeys);

                                if (demandedKeys != null)
                                {
                                    StringBuilder requestedOutputMeasurementKeys = new StringBuilder();

                                    foreach (MeasurementKey key in demandedKeys)
                                    {
                                        if (requestedOutputMeasurementKeys.Length > 0)
                                            requestedOutputMeasurementKeys.Append(";");

                                        requestedOutputMeasurementKeys.Append(key.ToString());
                                    }

                                    if (requestedOutputMeasurementKeys.Length > 0)
                                    {
                                        // Let action adapter know which output measurements are being demanded and start adapter
                                        actionAdapter.RequestedOutputMeasurementKeys = AdapterBase.ParseOutputMeasurementKeys(actionAdapter.DataSource, requestedOutputMeasurementKeys.ToString());
                                        actionAdapter.Enabled = true;
                                    }
                                    else
                                    {
                                        actionAdapter.RequestedOutputMeasurementKeys = null;

                                        // Action adapter should only be stopped if it also has no requested input measurements keys, determined prior
                                        actionAdapter.Enabled = (actionAdapter.RequestedInputMeasurementKeys != null);
                                    }
                                }
                                else
                                {
                                    actionAdapter.RequestedOutputMeasurementKeys = null;

                                    // Action adapter should only be stopped if it also has no requested input measurements keys, determined prior
                                    actionAdapter.Enabled = (actionAdapter.RequestedInputMeasurementKeys != null);
                                }
                            }
                            else
                            {
                                actionAdapter.RequestedOutputMeasurementKeys = null;

                                // Action adapter should only be stopped if it also has no requested input measurements keys, determined prior
                                actionAdapter.Enabled = (actionAdapter.RequestedInputMeasurementKeys != null);
                            }
                        }
                    }
                }

                if (inputAdapters != null)
                {
                    // Start or stop connect on demand input adapters based on need, i.e., they provide any of the currently demanded input measurements
                    foreach (IInputAdapter inputAdapter in inputAdapters)
                    {
                        if (!inputAdapter.AutoStart)
                        {
                            if (inputAdapter.OutputMeasurements != null)
                            {
                                // Create an intersection between the measurements the adapter can provide and those that are demanded throughout this Iaon session
                                demandedKeys = inputAdapter.OutputMeasurements.Select(m => m.Key).Intersect(inputMeasurementKeys);

                                if (demandedKeys != null)
                                {
                                    StringBuilder requestedOutputMeasurementKeys = new StringBuilder();

                                    foreach (MeasurementKey key in demandedKeys)
                                    {
                                        if (requestedOutputMeasurementKeys.Length > 0)
                                            requestedOutputMeasurementKeys.Append(";");

                                        requestedOutputMeasurementKeys.Append(key.ToString());
                                    }

                                    if (requestedOutputMeasurementKeys.Length > 0)
                                    {
                                        // Let input adapter know which output measurements are being demanded and start adapter
                                        inputAdapter.RequestedOutputMeasurementKeys = AdapterBase.ParseOutputMeasurementKeys(inputAdapter.DataSource, requestedOutputMeasurementKeys.ToString());
                                        inputAdapter.Enabled = true;
                                    }
                                    else
                                    {
                                        inputAdapter.RequestedOutputMeasurementKeys = null;
                                        inputAdapter.Enabled = false;
                                    }
                                }
                                else
                                {
                                    inputAdapter.RequestedOutputMeasurementKeys = null;
                                    inputAdapter.Enabled = false;
                                }
                            }
                            else
                            {
                                inputAdapter.RequestedOutputMeasurementKeys = null;
                                inputAdapter.Enabled = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
