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
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
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
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RoutingTables"/> class.
        /// </summary>
        public RoutingTables()
        {
            m_adapterRoutesCacheLock = new ReaderWriterLockSlim();
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
            ThreadPool.QueueUserWorkItem(CalculateRoutingTables);
        }

        private void CalculateRoutingTables(object state)
        {
            // Pre-calculate internal routes to improve performance
            Dictionary<MeasurementKey, List<IActionAdapter>> actionRoutes = new Dictionary<MeasurementKey, List<IActionAdapter>>();
            Dictionary<MeasurementKey, List<IOutputAdapter>> outputRoutes = new Dictionary<MeasurementKey, List<IOutputAdapter>>();
            List<IActionAdapter> actionAdapters, actionBroadcastRoutes = new List<IActionAdapter>();
            List<IOutputAdapter> outputAdapters, outputBroadcastRoutes = new List<IOutputAdapter>();
            MeasurementKey[] measurementKeys;

            if (m_actionAdapters != null)
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

            if (m_outputAdapters != null)
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
                    actionAdapter.QueueMeasurementsForProcessing(newMeasurements);
                }

                // Send broadcast output measurements
                foreach (IOutputAdapter outputAdapter in m_outputBroadcastRoutes)
                {
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
                actionAdapterMeasurements.Key.QueueMeasurementsForProcessing(actionAdapterMeasurements.Value);
            }

            // Send routed output measurements
            foreach (KeyValuePair<IOutputAdapter, List<IMeasurement>> outputAdapterMeasurements in outputMeasurements)
            {
                outputAdapterMeasurements.Key.QueueMeasurementsForProcessing(outputAdapterMeasurements.Value);
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

        #endregion
    }
}
