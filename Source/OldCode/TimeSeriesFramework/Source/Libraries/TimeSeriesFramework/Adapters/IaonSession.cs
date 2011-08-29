//******************************************************************************************************
//  IaonSession.cs - Gbtc
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
//  08/23/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using TVA;

namespace TimeSeriesFramework.Adapters
{
    /// <summary>
    /// Represents a new Input, Action, Output interface session.
    /// </summary>
    public class IaonSession
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="UseMeasurementRouting"/> property.
        /// </summary>
        public const bool DefaultUseMeasurementRouting = true;

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

        /// <summary>
        /// Event is raised when <see cref="IAdapter.InputMeasurementKeys"/> are updated.
        /// </summary>
        public event EventHandler InputMeasurementKeysUpdated;

        /// <summary>
        /// Event is raised when <see cref="IAdapter.OutputMeasurements"/> are updated.
        /// </summary>
        public event EventHandler OutputMeasurementsUpdated;

        /// <summary>
        /// This event is raised every second allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// Event is raised every second allowing host to track total number of unprocessed measurements.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="IOutputAdapter"/> implementation reports its current queue size of unprocessed
        /// measurements so that if queue size reaches an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed measurements.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnprocessedMeasurements;

        /// <summary>
        /// Event is raised when this <see cref="AdapterCollectionBase{T}"/> is disposed or an <see cref="IAdapter"/> in the collection is disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private AllAdaptersCollection m_allAdapters;
        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;
        private OutputAdapterCollection m_outputAdapters;
        private bool m_useMeasurementRouting;
        private RoutingTables m_routingTables;
        private Guid m_nodeID;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IaonSession"/>.
        /// </summary>
        public IaonSession()
        {
            m_useMeasurementRouting = DefaultUseMeasurementRouting;

            // Create a new set of routing tables
            m_routingTables = new RoutingTables();

            // Create a collection to manage all input, action and output adapter collections as a unit
            m_allAdapters = new AllAdaptersCollection();
            m_allAdapters.StatusMessage += StatusMessage;
            m_allAdapters.ProcessException += ProcessException;
            m_allAdapters.InputMeasurementKeysUpdated += InputMeasurementKeysUpdated;
            m_allAdapters.InputMeasurementKeysUpdated += AdapterMeasurementsUpdated;
            m_allAdapters.OutputMeasurementsUpdated += OutputMeasurementsUpdated;
            m_allAdapters.OutputMeasurementsUpdated += AdapterMeasurementsUpdated;
            m_allAdapters.Disposed += Disposed;

            // Create input adapters collection
            m_inputAdapters = new InputAdapterCollection();
            if (m_useMeasurementRouting)
                m_inputAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
            else
                m_inputAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;
            m_inputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
            m_routingTables.InputAdapters = m_inputAdapters;

            // Create action adapters collection
            m_actionAdapters = new ActionAdapterCollection();
            if (m_useMeasurementRouting)
                m_actionAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
            else
                m_actionAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;
            m_actionAdapters.UnpublishedSamples += UnpublishedSamples;
            m_actionAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
            m_routingTables.ActionAdapters = m_actionAdapters;

            // Create output adapters collection
            m_outputAdapters = new OutputAdapterCollection();
            m_outputAdapters.UnprocessedMeasurements += UnprocessedMeasurements;
            m_outputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
            m_routingTables.OutputAdapters = m_outputAdapters;

            // We group these adapters such that they are initialized in the following order: output, input, action. This
            // is done so that the archival capabilities will be setup before we start receiving input and the input data
            // will be flowing before any actions get established for the input - at least generally.
            m_allAdapters.Add(m_outputAdapters);
            m_allAdapters.Add(m_inputAdapters);
            m_allAdapters.Add(m_actionAdapters);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="IaonSession"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~IaonSession()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the all adapters collection for this <see cref="IaonSession"/>.
        /// </summary>
        public AllAdaptersCollection AllAdapters
        {
            get
            {
                return m_allAdapters;
            }
        }

        /// <summary>
        /// Gets the input adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public InputAdapterCollection InputAdapters
        {
            get
            {
                return m_inputAdapters;
            }
        }

        /// <summary>
        /// Gets the action adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public ActionAdapterCollection ActionAdapters
        {
            get
            {
                return m_actionAdapters;
            }
        }

        /// <summary>
        /// Gets the output adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public OutputAdapterCollection OutputAdapters
        {
            get
            {
                return m_outputAdapters;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if measurement routing should be used.
        /// </summary>
        public bool UseMeasurementRouting
        {
            get
            {
                return m_useMeasurementRouting;
            }
            set
            {
                if (m_useMeasurementRouting != value)
                {
                    if (m_useMeasurementRouting)
                        m_inputAdapters.NewMeasurements -= m_routingTables.RoutedMeasurementsHandler;
                    else
                        m_inputAdapters.NewMeasurements -= m_routingTables.BroadcastMeasurementsHandler;

                    if (m_useMeasurementRouting)
                        m_actionAdapters.NewMeasurements -= m_routingTables.RoutedMeasurementsHandler;
                    else
                        m_actionAdapters.NewMeasurements -= m_routingTables.BroadcastMeasurementsHandler;

                    m_useMeasurementRouting = value;

                    if (m_useMeasurementRouting)
                        m_inputAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
                    else
                        m_inputAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;

                    if (m_useMeasurementRouting)
                        m_actionAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
                    else
                        m_actionAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;

                    m_inputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
                    m_actionAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
                    m_outputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
                }
            }
        }

        /// <summary>
        /// Gets the routing tables for this <see cref="IaonSession"/>.
        /// </summary>
        public RoutingTables RoutingTables
        {
            get
            {
                return m_routingTables;
            }
        }

        /// <summary>
        /// Gets or sets the configuration <see cref="DataSet"/> for this <see cref="IaonSession"/>.
        /// </summary>
        public DataSet DataSource
        {
            get
            {
                return m_allAdapters.DataSource;
            }
            set
            {
                m_allAdapters.DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> node ID  for this <see cref="IaonSession"/>.
        /// </summary>
        public Guid NodeID
        {
            get
            {
                return m_nodeID;
            }
            set
            {
                m_nodeID = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="IaonSession"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="IaonSession"/> object and optionally releases the managed resources.
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
                        // Dispose input adapters collection
                        if (m_inputAdapters != null)
                        {
                            m_inputAdapters.Stop();

                            if (m_useMeasurementRouting)
                                m_inputAdapters.NewMeasurements -= m_routingTables.RoutedMeasurementsHandler;
                            else
                                m_inputAdapters.NewMeasurements -= m_routingTables.BroadcastMeasurementsHandler;

                            m_inputAdapters.Dispose();
                        }
                        m_inputAdapters = null;

                        // Dispose action adapters collection
                        if (m_actionAdapters != null)
                        {
                            m_actionAdapters.Stop();

                            if (m_useMeasurementRouting)
                                m_actionAdapters.NewMeasurements -= m_routingTables.RoutedMeasurementsHandler;
                            else
                                m_actionAdapters.NewMeasurements -= m_routingTables.BroadcastMeasurementsHandler;

                            m_actionAdapters.UnpublishedSamples -= UnpublishedSamples;
                            m_actionAdapters.Dispose();
                        }
                        m_actionAdapters = null;

                        // Dispose output adapters collection
                        if (m_outputAdapters != null)
                        {
                            m_outputAdapters.Stop();
                            m_outputAdapters.UnprocessedMeasurements -= UnprocessedMeasurements;
                            m_outputAdapters.Dispose();
                        }
                        m_outputAdapters = null;

                        // Dispose all adapters collection
                        if (m_allAdapters != null)
                        {
                            m_allAdapters.StatusMessage -= StatusMessage;
                            m_allAdapters.ProcessException -= ProcessException;
                            m_allAdapters.InputMeasurementKeysUpdated -= InputMeasurementKeysUpdated;
                            m_allAdapters.InputMeasurementKeysUpdated -= AdapterMeasurementsUpdated;
                            m_allAdapters.OutputMeasurementsUpdated -= OutputMeasurementsUpdated;
                            m_allAdapters.OutputMeasurementsUpdated -= AdapterMeasurementsUpdated;
                            m_allAdapters.Disposed -= Disposed;
                            m_allAdapters.Dispose();
                        }
                        m_allAdapters = null;

                        // Dispose of routing tables
                        if (m_routingTables != null)
                            m_routingTables.Dispose();

                        m_routingTables = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initialize and start adapters.
        /// </summary>
        public void Initialize()
        {
            // Initialize all adapters
            m_allAdapters.Initialize();

            // Start all adapters
            m_allAdapters.Start();

            // Spawn routing table calculation
            RecalculateRoutingTables();
        }

        /// <summary>
        /// Recalculates routing tables as long as all adapters have been initialized.
        /// </summary>
        public void RecalculateRoutingTables()
        {
            if (m_useMeasurementRouting && m_routingTables != null && m_allAdapters != null && m_allAdapters.Initialized)
                m_routingTables.CalculateRoutingTables();
        }

        /// <summary>
        /// Handler for updates to adapter input or output measurement definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        private void AdapterMeasurementsUpdated(object sender, EventArgs e)
        {
            // When adapter measurement keys are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();
        }

        #endregion
    }
}