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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TVA;
using TVA.Collections;
using TVA.Configuration;

namespace TimeSeriesFramework.Adapters
{
    /// <summary>
    /// Represents a new Input, Action, Output interface session.
    /// </summary>
    public class IaonSession : IProvideStatus
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
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the new status message.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the message <see cref="UpdateType"/>.
        /// </remarks>
        public event EventHandler<EventArgs<string, UpdateType>> StatusMessage;

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
        private Guid m_nodeID;
        private RoutingTables m_routingTables;
        private AllAdaptersCollection m_allAdapters;
        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;
        private OutputAdapterCollection m_outputAdapters;
        private bool m_useMeasurementRouting;
        private int m_measurementWarningThreshold;
        private int m_measurementDumpingThreshold;
        private int m_defaultSampleSizeWarningThreshold;
        private Dictionary<object, string> m_derivedNameCache;
        private string m_name;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IaonSession"/>.
        /// </summary>
        public IaonSession()
        {
            ConfigurationFile configFile = ConfigurationFile.Current;

            // Initialize system settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];

            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");
            systemSettings.Add("UseMeasurementRouting", DefaultUseMeasurementRouting, "Set to true to use optimized adapter measurement routing.");

            m_nodeID = systemSettings["NodeID"].ValueAs<Guid>();
            m_useMeasurementRouting = systemSettings["UseMeasurementRouting"].ValueAsBoolean(IaonSession.DefaultUseMeasurementRouting);

            // Initialize threshold settings
            CategorizedSettingsElementCollection thresholdSettings = configFile.Settings["thresholdSettings"];

            thresholdSettings.Add("MeasurementWarningThreshold", "100000", "Number of unarchived measurements allowed in any output adapter queue before displaying a warning message");
            thresholdSettings.Add("MeasurementDumpingThreshold", "500000", "Number of unarchived measurements allowed in any output adapter queue before taking evasive action and dumping data");
            thresholdSettings.Add("DefaultSampleSizeWarningThreshold", "10", "Default number of unpublished samples (in seconds) allowed in any action adapter queue before displaying a warning message");

            m_measurementWarningThreshold = thresholdSettings["MeasurementWarningThreshold"].ValueAsInt32();
            m_measurementDumpingThreshold = thresholdSettings["MeasurementDumpingThreshold"].ValueAsInt32();
            m_defaultSampleSizeWarningThreshold = thresholdSettings["DefaultSampleSizeWarningThreshold"].ValueAsInt32();

            // Create a cache for derived adapter names
            m_derivedNameCache = new Dictionary<object, string>();

            // Create a new set of routing tables
            m_routingTables = new RoutingTables();

            // Create a collection to manage all input, action and output adapter collections as a unit
            m_allAdapters = new AllAdaptersCollection();

            // Attach to common adapter events
            m_allAdapters.StatusMessage += StatusMessageHandler;
            m_allAdapters.ProcessException += ProcessExceptionHandler;
            m_allAdapters.InputMeasurementKeysUpdated += InputMeasurementKeysUpdatedHandler;
            m_allAdapters.OutputMeasurementsUpdated += OutputMeasurementsUpdatedHandler;
            m_allAdapters.Disposed += DisposedHandler;

            // Create input adapters collection
            m_inputAdapters = new InputAdapterCollection();

            if (m_useMeasurementRouting)
                m_inputAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
            else
                m_inputAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;

            m_inputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;

            // Create action adapters collection
            m_actionAdapters = new ActionAdapterCollection();

            if (m_useMeasurementRouting)
                m_actionAdapters.NewMeasurements += m_routingTables.RoutedMeasurementsHandler;
            else
                m_actionAdapters.NewMeasurements += m_routingTables.BroadcastMeasurementsHandler;

            m_actionAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
            m_actionAdapters.UnpublishedSamples += UnpublishedSamplesHandler;

            // Create output adapters collection
            m_outputAdapters = new OutputAdapterCollection();
            m_outputAdapters.ProcessMeasurementFilter = !m_useMeasurementRouting;
            m_outputAdapters.UnprocessedMeasurements += UnprocessedMeasurementsHandler;

            // Associate adapter collections with routing tables
            m_routingTables.InputAdapters = m_inputAdapters;
            m_routingTables.ActionAdapters = m_actionAdapters;
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

                // Create a new temporal support identification table
                DataTable temporalSupport = new DataTable("TemporalSupport");

                temporalSupport.Columns.Add("Source", typeof(string));
                temporalSupport.Columns.Add("ID", typeof(uint));

                // Add rows for each Iaon adapter collection to identify which adapters support temporal processing
                foreach (IAdapterCollection collection in m_allAdapters)
                {
                    foreach (IAdapter adapter in collection.Where(adapter => adapter.SupportsTemporalProcessing))
                    {
                        temporalSupport.Rows.Add(collection.DataMember, adapter.ID);
                    }
                }

                if (m_allAdapters.DataSource.Tables.Contains("TemporalSupport"))
                    m_allAdapters.DataSource.Tables.Remove("TemporalSupport");

                m_allAdapters.DataSource.Tables.Add(temporalSupport.Copy());
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

        /// <summary>
        /// Gets name assigned to this <see cref="IaonSession"/>, if any.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name.ToNonNullString();
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets the combined status of the adapters in this <see cref="IaonSession"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine(">> Input Adapters:");
                status.AppendLine();

                if (m_inputAdapters != null)
                    status.AppendLine(m_inputAdapters.Status);

                status.AppendLine(">> Action Adapters:");
                status.AppendLine();

                if (m_actionAdapters != null)
                    status.AppendLine(m_actionAdapters.Status);

                status.AppendLine(">> Output Adapters:");
                status.AppendLine();

                if (m_outputAdapters != null)
                    status.AppendLine(m_outputAdapters.Status);

                return status.ToString();
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

                            m_actionAdapters.UnpublishedSamples -= UnpublishedSamplesHandler;
                            m_actionAdapters.Dispose();
                        }
                        m_actionAdapters = null;

                        // Dispose output adapters collection
                        if (m_outputAdapters != null)
                        {
                            m_outputAdapters.Stop();
                            m_outputAdapters.UnprocessedMeasurements -= UnprocessedMeasurementsHandler;
                            m_outputAdapters.Dispose();
                        }
                        m_outputAdapters = null;

                        // Dispose all adapters collection
                        if (m_allAdapters != null)
                        {
                            m_allAdapters.StatusMessage -= StatusMessageHandler;
                            m_allAdapters.ProcessException -= ProcessExceptionHandler;
                            m_allAdapters.InputMeasurementKeysUpdated -= InputMeasurementKeysUpdatedHandler;
                            m_allAdapters.OutputMeasurementsUpdated -= OutputMeasurementsUpdatedHandler;
                            m_allAdapters.Disposed -= DisposedHandler;
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
        /// <param name="autoStart">Sets flag that determines if adapters should be automatically started.</param>
        public void Initialize(bool autoStart = true)
        {
            // Initialize all adapters
            m_allAdapters.Initialize();

            // Start all adapters
            if (autoStart)
                m_allAdapters.Start();

            // Spawn routing table calculation
            RecalculateRoutingTables();
        }

        /// <summary>
        /// Gets flag that determines if temporal processing is supported in this <see cref="IaonSession"/>.
        /// </summary>
        /// <param name="collection">Name of collection over which to check support (e.g., "InputAdapters"); or <c>null</c> for all collections.</param>
        /// <returns>Flag that determines if temporal processing is supported in this <see cref="IaonSession"/>.</returns>
        public bool TemporalProcessingSupportExists(string collection = null)
        {
            try
            {
                DataTable temporalSupport = m_allAdapters.DataSource.Tables["TemporalSupport"];

                if (string.IsNullOrWhiteSpace(collection))
                    return temporalSupport.Rows.Count > 0;

                return temporalSupport.Select(string.Format("Source = '{0}'", collection)).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets derived name of specified object.
        /// </summary>
        /// <param name="sender">Sending object from which to derive name.</param>
        /// <returns>Derived name of specified object.</returns>
        public virtual string GetDerivedName(object sender)
        {
            string name;

            if (!m_derivedNameCache.TryGetValue(sender, out name))
            {
                IProvideStatus statusProvider = sender as IProvideStatus;

                if (statusProvider != null)
                    name = statusProvider.Name.NotEmpty(sender.GetType().Name);
                else if (sender != null && sender is string)
                    name = (string)sender;
                else
                    name = sender.GetType().Name;

                if (!string.IsNullOrWhiteSpace(m_name))
                    name += "#" + m_name;

                m_derivedNameCache.Add(sender, name);
            }

            return name;
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
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        /// <param name="type"><see cref="UpdateType"/> of status message.</param>
        protected virtual void OnStatusMessage(string status, UpdateType type = UpdateType.Information)
        {
            if (StatusMessage != null)
            {
                // When using default informational update type, see if an update type code was embedded in the status message - this allows for compatibility for event
                // handlers that are normally unware of the update type
                if (type == UpdateType.Information && (object)status != null && status.Length > 3 && status.StartsWith("0x") && Enum.TryParse(status[2].ToString(), out type))
                    status = status.Substring(3);

                StatusMessage(this, new EventArgs<string, UpdateType>(status, type));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="type"><see cref="UpdateType"/> of status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convienence.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, UpdateType type, params object[] args)
        {
            if (StatusMessage != null)
                OnStatusMessage(string.Format(formattedStatus, args), type);
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises <see cref="InputMeasurementKeysUpdated"/> event.
        /// </summary>
        protected virtual void OnInputMeasurementKeysUpdated()
        {
            if (InputMeasurementKeysUpdated != null)
                InputMeasurementKeysUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="OutputMeasurementsUpdated"/> event.
        /// </summary>
        protected virtual void OnOutputMeasurementsUpdated()
        {
            if (OutputMeasurementsUpdated != null)
                OutputMeasurementsUpdated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="seconds">Total number of unpublished seconds of data.</param>
        protected virtual void OnUnpublishedSamples(int seconds)
        {
            if (UnpublishedSamples != null)
                UnpublishedSamples(this, new EventArgs<int>(seconds));
        }

        /// <summary>
        /// Raises the <see cref="UnprocessedMeasurements"/> event.
        /// </summary>
        /// <param name="unprocessedMeasurements">Total measurements in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedMeasurements(int unprocessedMeasurements)
        {
            if (UnprocessedMeasurements != null)
                UnprocessedMeasurements(this, new EventArgs<int>(unprocessedMeasurements));
        }

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        protected virtual void OnDisposed()
        {
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event handler for reporting status messages.
        /// </summary>
        /// <param name="sender">Event source of the status message.</param>
        /// <param name="e">Event arguments containing the status message to report.</param>
        public void StatusMessageHandler(object sender, EventArgs<string> e)
        {
            OnStatusMessage("[{0}] {1}", UpdateType.Information, GetDerivedName(sender), e.Argument);
        }

        /// <summary>
        /// Event handler for processing reported exceptions.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        public void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnStatusMessage("[{0}] {1}", UpdateType.Alarm, GetDerivedName(sender), e.Argument.Message);
            OnProcessException(e.Argument);
        }

        /// <summary>
        /// Event handler for updates to adapter input measurement key definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public void InputMeasurementKeysUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter measurement keys are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnInputMeasurementKeysUpdated();
        }

        /// <summary>
        /// Event handler for updates to adapter output measurement definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public void OutputMeasurementsUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter measurement keys are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnOutputMeasurementsUpdated();
        }

        /// <summary>
        /// Event handler for disposed events from all adapters.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public void DisposedHandler(object sender, EventArgs e)
        {
            OnStatusMessage("[{0}] Disposed.", UpdateType.Information, GetDerivedName(sender));

            // Bubble message up to any event subscribers
            OnDisposed();
        }

        /// <summary>
        /// Event handler for monitoring unpublished samples.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an action adapter, that is reporting the number of unpublished data samples.</param>
        /// <param name="e">Event arguments containing number of samples, in seconds of data, of unpublished data in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unpublished samples, in seconds of data, in action adapters.<br/>
        /// This method is typically called once per second.
        /// </remarks>
        public void UnpublishedSamplesHandler(object sender, EventArgs<int> e)
        {
            int secondsOfData = e.Argument;
            int threshold = m_defaultSampleSizeWarningThreshold;
            ConcentratorBase concentrator = sender as ConcentratorBase;

            // Most action adapters will be based on a concentrator, if so we monitor the unpublished sample queue size compared to the defined
            // lag time - if the queue size is over twice the lag size, the action adapter could be falling behind
            if (concentrator != null)
                threshold = (int)(2 * Math.Ceiling(concentrator.LagTime));

            if (secondsOfData > threshold)
                OnStatusMessage("[{0}] There are {1} seconds of unpublished data in the action adapter concentration queue.", UpdateType.Warning, GetDerivedName(sender), secondsOfData);
        }

        /// <summary>
        /// Event handler for monitoring unprocessed measurements.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an output adapter, that is reporting the number of unprocessed measurements.</param>
        /// <param name="e">Event arguments containing number of queued (i.e., unprocessed) measurements in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unprocessed measurements in output adapters.<br/>
        /// This method is typically called once per second.
        /// </remarks>
        public void UnprocessedMeasurementsHandler(object sender, EventArgs<int> e)
        {
            int unprocessedMeasurements = e.Argument;

            if (unprocessedMeasurements > m_measurementDumpingThreshold)
            {
                IOutputAdapter outputAdpater = sender as IOutputAdapter;

                if (outputAdpater != null)
                {
                    // If an output adapter queue size exceeds the defined measurement dumping threshold,
                    // then the queue will be truncated before system runs out of memory
                    outputAdpater.RemoveMeasurements(m_measurementDumpingThreshold);
                    OnStatusMessage("[{0}] System exercised evasive action to convserve memory and dumped {1} unprocessed measurements from the output queue :(", UpdateType.Alarm, outputAdpater.Name, m_measurementDumpingThreshold);
                    OnStatusMessage("[{0}] NOTICE: Please adjust measurement threshold settings and/or increase amount of available system memory.", UpdateType.Warning, outputAdpater.Name);
                }
                else
                {
                    // It is only expected that output adapters will be mapped to this handler, but in case
                    // another adapter type uses this handler we will still display a message
                    OnStatusMessage("[{0}] CRITICAL: There are {1} unprocessed measurements in the adapter queue - but sender \"{2}\" is not an IOutputAdapter, so no evasive action can be exercised.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements, sender.GetType().Name);
                }
            }
            else if (unprocessedMeasurements > m_measurementWarningThreshold)
            {
                if (unprocessedMeasurements >= m_measurementDumpingThreshold - m_measurementWarningThreshold)
                    OnStatusMessage("[{0}] CRITICAL: There are {1} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
                else
                    OnStatusMessage("[{0}] There are {1} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Extracts a configuration that supports temporal processing from an existing real-time configuration.
        /// </summary>
        /// <param name="realtimeConfiguration">Real-time <see cref="DataSet"/> configuration.</param>
        /// <returns>A new <see cref="DataSet"/> configuration for adapters that support temporal processing.</returns>
        public static DataSet ExtractTemporalConfiguration(DataSet realtimeConfiguration)
        {
            // Duplicate current run-time session configuration that has temporal support
            DataSet temporalConfiguration = new DataSet("IaonTemporal");
            DataTable temporalSupport = realtimeConfiguration.Tables["TemporalSupport"];
            string tableName;

            foreach (DataTable table in realtimeConfiguration.Tables)
            {
                tableName = table.TableName;

                switch (tableName.ToLower())
                {
                    case "inputadapters":
                    case "actionadapters":
                    case "outputadapters":
                        // For Iaon adapter tables, we only copy in adapters that support temporal processing
                        temporalConfiguration.Tables.Add(table.Clone());

                        // Check for adapters with temporal support in this adapter collection
                        DataRow[] temporalAdapters = temporalSupport.Select(string.Format("Source = '{0}'", tableName));

                        // If any adapters support temporal processing, add them to the temporal configuration
                        if (temporalAdapters.Length > 0)
                        {
                            DataTable realtimeTable = realtimeConfiguration.Tables[tableName];
                            DataTable temporalTable = temporalConfiguration.Tables[tableName];

                            foreach (DataRow row in realtimeTable.Select(string.Format("ID IN ({0})", temporalAdapters.Select(row => row["ID"].ToString()).ToDelimitedString(','))))
                            {
                                DataRow newRow = temporalTable.NewRow();

                                for (int x = 0; x < realtimeTable.Columns.Count; x++)
                                {
                                    newRow[x] = row[x];
                                }

                                temporalConfiguration.Tables[tableName].Rows.Add(newRow);
                            }
                        }

                        break;
                    default:
                        // For all other tables we add configuration information as-is
                        temporalConfiguration.Tables.Add(table.Copy());
                        break;
                }
            }

            return temporalConfiguration;
        }

        #endregion
    }
}