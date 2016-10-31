//******************************************************************************************************
//  IaonSession.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GSF.Annotations;
using GSF.Collections;
using GSF.Configuration;
using GSF.Diagnostics;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a new Input, Action, Output interface session.
    /// </summary>
    public class IaonSession : IProvideStatus, IDisposable
    {
        #region [ Members ]

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
        /// Event is raised when adapter is aware of a configuration change.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Event is raised every five seconds allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        /// <summary>
        /// Event is raised every five seconds allowing host to track total number of unprocessed measurements.
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
        /// Indicates to the host that processing for one of the input adapters has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public event EventHandler ProcessingComplete;

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
        private MeasurementKey[] m_inputMeasurementKeysRestriction;
        private readonly int m_measurementWarningThreshold;
        private readonly int m_measurementDumpingThreshold;
        private readonly int m_defaultSampleSizeWarningThreshold;
        private readonly object m_requestTemporalSupportLock;
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
            m_nodeID = systemSettings["NodeID"].ValueAs<Guid>();

            // Initialize threshold settings
            CategorizedSettingsElementCollection thresholdSettings = configFile.Settings["thresholdSettings"];

            thresholdSettings.Add("MeasurementWarningThreshold", "100000", "Number of unarchived measurements allowed in any output adapter queue before displaying a warning message");
            thresholdSettings.Add("MeasurementDumpingThreshold", "500000", "Number of unarchived measurements allowed in any output adapter queue before taking evasive action and dumping data");
            thresholdSettings.Add("DefaultSampleSizeWarningThreshold", "10", "Default number of unpublished samples (in seconds) allowed in any action adapter queue before displaying a warning message");

            m_measurementWarningThreshold = thresholdSettings["MeasurementWarningThreshold"].ValueAsInt32();
            m_measurementDumpingThreshold = thresholdSettings["MeasurementDumpingThreshold"].ValueAsInt32();
            m_defaultSampleSizeWarningThreshold = thresholdSettings["DefaultSampleSizeWarningThreshold"].ValueAsInt32();

            // Create a new set of routing tables
            switch (OptimizationOptions.DefaultRoutingMethod)
            {
                case OptimizationOptions.RoutingMethod.HighLatencyLowCpu:
                    m_routingTables = new RoutingTables(new RouteMappingHighLatencyLowCpu(OptimizationOptions.RoutingLatency));
                    break;
                default:
                    m_routingTables = new RoutingTables();
                    break;
            }

            m_routingTables.StatusMessage += m_routingTables_StatusMessage;
            m_routingTables.ProcessException += m_routingTables_ProcessException;

            // Create a collection to manage all input, action and output adapter collections as a unit
            m_allAdapters = new AllAdaptersCollection();

            // Attach to common adapter events
            m_allAdapters.StatusMessage += StatusMessageHandler;
            m_allAdapters.ProcessException += ProcessExceptionHandler;
            m_allAdapters.InputMeasurementKeysUpdated += InputMeasurementKeysUpdatedHandler;
            m_allAdapters.OutputMeasurementsUpdated += OutputMeasurementsUpdatedHandler;
            m_allAdapters.ConfigurationChanged += ConfigurationChangedHandler;
            m_allAdapters.Disposed += DisposedHandler;

            // Create input adapters collection
            m_inputAdapters = new InputAdapterCollection();
            m_inputAdapters.ProcessingComplete += ProcessingCompleteHandler;

            // Create action adapters collection
            m_actionAdapters = new ActionAdapterCollection();
            m_actionAdapters.UnpublishedSamples += UnpublishedSamplesHandler;
            m_actionAdapters.RequestTemporalSupport += RequestTemporalSupportHandler;

            // Create output adapters collection
            m_outputAdapters = new OutputAdapterCollection();
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

            m_requestTemporalSupportLock = new object();
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
        public virtual AllAdaptersCollection AllAdapters
        {
            get
            {
                return m_allAdapters;
            }
        }

        /// <summary>
        /// Gets the input adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual InputAdapterCollection InputAdapters
        {
            get
            {
                return m_inputAdapters;
            }
        }

        /// <summary>
        /// Gets the action adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual ActionAdapterCollection ActionAdapters
        {
            get
            {
                return m_actionAdapters;
            }
        }

        /// <summary>
        /// Gets the output adapter collection for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual OutputAdapterCollection OutputAdapters
        {
            get
            {
                return m_outputAdapters;
            }
        }

        /// <summary>
        /// Gets the routing tables for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual RoutingTables RoutingTables
        {
            get
            {
                return m_routingTables;
            }
        }

        /// <summary>
        /// Gets or sets a routing table restriction for a collection of input measurement keys.
        /// </summary>
        public virtual MeasurementKey[] InputMeasurementKeysRestriction
        {
            get
            {
                return m_inputMeasurementKeysRestriction;
            }
            set
            {
                m_inputMeasurementKeysRestriction = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration <see cref="DataSet"/> for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual DataSet DataSource
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
        public virtual Guid NodeID
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
        public virtual string Name
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
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine();
                status.AppendLine(">> Input Adapters:");
                status.AppendLine();

                if (m_inputAdapters != null)
                    status.AppendLine(m_inputAdapters.Status);

                status.AppendLine();
                status.AppendLine(">> Action Adapters:");
                status.AppendLine();

                if (m_actionAdapters != null)
                    status.AppendLine(m_actionAdapters.Status);

                status.AppendLine();
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
                        DataSet dataSource = DataSource;

                        // Dispose input adapters collection
                        if (m_inputAdapters != null)
                        {
                            m_inputAdapters.Stop();
                            m_inputAdapters.ProcessingComplete -= ProcessingCompleteHandler;
                            m_inputAdapters.Dispose();
                        }
                        m_inputAdapters = null;

                        // Dispose action adapters collection
                        if (m_actionAdapters != null)
                        {
                            m_actionAdapters.Stop();
                            m_actionAdapters.UnpublishedSamples -= UnpublishedSamplesHandler;
                            m_actionAdapters.RequestTemporalSupport -= RequestTemporalSupportHandler;
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
                            m_allAdapters.ConfigurationChanged -= ConfigurationChangedHandler;
                            m_allAdapters.Disposed -= DisposedHandler;
                            m_allAdapters.Dispose();
                        }
                        m_allAdapters = null;

                        // Dispose of routing tables
                        if (m_routingTables != null)
                        {
                            m_routingTables.StatusMessage -= m_routingTables_StatusMessage;
                            m_routingTables.ProcessException -= m_routingTables_ProcessException;
                            m_routingTables.Dispose();
                        }
                        m_routingTables = null;

                        if ((object)dataSource != null)
                            dataSource.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initialize and start adapters.
        /// </summary>
        /// <param name="autoStart">Sets flag that determines if adapters should be automatically started.</param>
        public virtual void Initialize(bool autoStart = true)
        {
            // Initialize all adapters
            m_allAdapters.Initialize();

            if (autoStart)
            {
                // Start all adapters if they
                // haven't started already
                m_allAdapters.Start();
            }
        }

        /// <summary>
        /// Gets flag that determines if temporal processing is supported in this <see cref="IaonSession"/>.
        /// </summary>
        /// <param name="collection">Name of collection over which to check support (e.g., "InputAdapters"); or <c>null</c> for all collections.</param>
        /// <returns>Flag that determines if temporal processing is supported in this <see cref="IaonSession"/>.</returns>
        public virtual bool TemporalProcessingSupportExists(string collection = null)
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
            string name = null;
            IProvideStatus statusProvider = sender as IProvideStatus;

            if ((object)statusProvider != null)
                name = statusProvider.Name;
            else if (sender is string)
                name = (string)sender;

            if (string.IsNullOrWhiteSpace(name))
                name = sender.GetType().Name;

            if (!string.IsNullOrWhiteSpace(m_name))
                name += "#" + m_name;

            return name;
        }

        /// <summary>
        /// Recalculates routing tables as long as all adapters have been initialized.
        /// </summary>
        public virtual void RecalculateRoutingTables()
        {
            if ((object)m_routingTables != null && (object)m_allAdapters != null && m_allAdapters.Initialized)
                m_routingTables.CalculateRoutingTables(m_inputMeasurementKeysRestriction);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="status">New status message.</param>
        /// <param name="type"><see cref="UpdateType"/> of status message.</param>
        protected virtual void OnStatusMessage(object sender, string status, UpdateType type = UpdateType.Information)
        {
            if ((object)StatusMessage != null)
            {
                // When using default informational update type, see if an update type code was embedded in the status message - this allows for compatibility for event
                // handlers that are normally unaware of the update type
                if (type == UpdateType.Information && (object)status != null && status.Length > 3 && status.StartsWith("0x") && Enum.TryParse(status[2].ToString(), out type))
                    status = status.Substring(3);

                StatusMessage(sender, new EventArgs<string, UpdateType>(status, type));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="type"><see cref="UpdateType"/> of status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convenience.
        /// </remarks>
        [StringFormatMethod("formattedStatus")]
        protected virtual void OnStatusMessage(object sender, string formattedStatus, UpdateType type, params object[] args)
        {
            if ((object)StatusMessage != null)
                OnStatusMessage(sender, string.Format(formattedStatus, args), type);
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(object sender, Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(sender, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises <see cref="InputMeasurementKeysUpdated"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnInputMeasurementKeysUpdated(object sender)
        {
            if ((object)InputMeasurementKeysUpdated != null)
                InputMeasurementKeysUpdated(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="OutputMeasurementsUpdated"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnOutputMeasurementsUpdated(object sender)
        {
            if ((object)OutputMeasurementsUpdated != null)
                OutputMeasurementsUpdated(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="ConfigurationChanged"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnConfigurationChanged(object sender)
        {
            if ((object)ConfigurationChanged != null)
                ConfigurationChanged(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="seconds">Total number of unpublished seconds of data.</param>
        protected virtual void OnUnpublishedSamples(object sender, int seconds)
        {
            if ((object)UnpublishedSamples != null)
                UnpublishedSamples(sender, new EventArgs<int>(seconds));
        }

        /// <summary>
        /// Raises the <see cref="UnprocessedMeasurements"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="unprocessedMeasurements">Total measurements in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedMeasurements(object sender, int unprocessedMeasurements)
        {
            if ((object)UnprocessedMeasurements != null)
                UnprocessedMeasurements(sender, new EventArgs<int>(unprocessedMeasurements));
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="e"><see cref="EventArgs"/>, if any.</param>
        protected virtual void OnProcessingComplete(object sender, EventArgs e = null)
        {
            if ((object)ProcessingComplete != null)
                ProcessingComplete(sender, e ?? EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnDisposed(object sender)
        {
            if ((object)Disposed != null)
                Disposed(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Event handler for reporting status messages.
        /// </summary>
        /// <param name="sender">Event source of the status message.</param>
        /// <param name="e">Event arguments containing the status message to report.</param>
        public virtual void StatusMessageHandler(object sender, EventArgs<string> e)
        {
            // Bubble message up to any event subscribers
            OnStatusMessage(sender, "[{0}] {1}", UpdateType.Information, GetDerivedName(sender), e.Argument);
        }

        /// <summary>
        /// Event handler for processing reported exceptions.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        public virtual void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnStatusMessage(sender, "[{0}] {1}", UpdateType.Alarm, GetDerivedName(sender), e.Argument.Message);

            // Bubble message up to any event subscribers
            OnProcessException(sender, e.Argument);
        }

        /// <summary>
        /// Event handler for updates to adapter input measurement key definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void InputMeasurementKeysUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter measurement keys are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnInputMeasurementKeysUpdated(sender);
        }

        /// <summary>
        /// Event handler for updates to adapter output measurement definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void OutputMeasurementsUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter measurement keys are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnOutputMeasurementsUpdated(sender);
        }

        /// <summary>
        /// Event handler for adapter notifications about configuration changes.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void ConfigurationChangedHandler(object sender, EventArgs e)
        {
            // Bubble message up to any event subscribers
            OnConfigurationChanged(sender);
        }

        /// <summary>
        /// Event handler for monitoring unpublished samples.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an action adapter, that is reporting the number of unpublished data samples.</param>
        /// <param name="e">Event arguments containing number of samples, in seconds of data, of unpublished data in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unpublished samples, in seconds of data, in action adapters.<br/>
        /// This method is typically called once every five seconds.
        /// </remarks>
        public virtual void UnpublishedSamplesHandler(object sender, EventArgs<int> e)
        {
            int secondsOfData = e.Argument;
            int threshold = m_defaultSampleSizeWarningThreshold;
            int processingInterval = -1;
            ConcentratorBase concentrator = sender as ConcentratorBase;
            IAdapter adapter = sender as IAdapter;

            // Most action adapters will be based on a concentrator, if so we monitor the unpublished sample queue size compared to the defined
            // lag time - if the queue size is over twice the lag size, the action adapter could be falling behind
            if ((object)concentrator != null)
                threshold = (int)(2 * Math.Ceiling(concentrator.LagTime));

            // Get processing interval for adapter
            if ((object)adapter != null)
                processingInterval = adapter.ProcessingInterval;

            // Allow much more time before warning for when a fast processing interval has been defined
            if (processingInterval > -1 && processingInterval < 100)
                threshold *= 4;

            if (secondsOfData > threshold)
                OnStatusMessage(sender, "[{0}] There are {1} seconds of unpublished data in the action adapter concentration queue.", UpdateType.Warning, GetDerivedName(sender), secondsOfData);

            // Bubble message up to any event subscribers
            OnUnpublishedSamples(sender, e.Argument);
        }

        /// <summary>
        /// Event handler for requesting temporal support.
        /// </summary>
        /// <param name="sender">Event source reference to adapter collection, typically an action adapter collection, that is requesting temporal support.</param>
        /// <param name="e">Event arguments are not used.</param>
        /// <remarks>
        /// Action adapter collections use this handler to make sure temporal support is initialized before setting up temporal sessions.
        /// </remarks>
        public virtual void RequestTemporalSupportHandler(object sender, EventArgs e)
        {
            lock (m_requestTemporalSupportLock)
            {
                if (!m_allAdapters.DataSource.Tables.Contains("TemporalSupport"))
                {
                    // Create a new temporal support identification table
                    DataTable temporalSupport = new DataTable("TemporalSupport");

                    temporalSupport.Columns.Add("Source", typeof(string));
                    temporalSupport.Columns.Add("ID", typeof(uint));

                    // Add rows for each Iaon adapter collection to identify which adapters support temporal processing
                    lock (m_allAdapters)
                    {
                        foreach (IAdapterCollection collection in m_allAdapters)
                        {
                            foreach (IAdapter adapter in collection.Where(adapter => adapter.SupportsTemporalProcessing))
                            {
                                temporalSupport.Rows.Add(collection.DataMember, adapter.ID);
                            }
                        }

                        m_allAdapters.DataSource.Tables.Add(temporalSupport.Copy());
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for monitoring unprocessed measurements.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an output adapter, that is reporting the number of unprocessed measurements.</param>
        /// <param name="e">Event arguments containing number of queued (i.e., unprocessed) measurements in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unprocessed measurements in output adapters.<br/>
        /// This method is typically called once every five seconds.
        /// </remarks>
        public virtual void UnprocessedMeasurementsHandler(object sender, EventArgs<int> e)
        {
            int unprocessedMeasurements = e.Argument;

            if (unprocessedMeasurements > m_measurementDumpingThreshold)
            {
                IOutputAdapter outputAdapter = sender as IOutputAdapter;

                if (outputAdapter != null)
                {
                    // If an output adapter queue size exceeds the defined measurement dumping threshold,
                    // then the queue will be truncated before system runs out of memory
                    outputAdapter.RemoveMeasurements(m_measurementDumpingThreshold);
                    OnStatusMessage(sender, "[{0}] System exercised evasive action to conserve memory and dumped {1:N0} unprocessed measurements from the output queue :(", UpdateType.Alarm, outputAdapter.Name, m_measurementDumpingThreshold);
                    OnStatusMessage(sender, "[{0}] NOTICE: Adapter may be offline or processing data too slowly to keep up with incoming data volume. It may be necessary to adjust measurement threshold configuration settings and/or increase amount of available system memory.", UpdateType.Warning, outputAdapter.Name);
                }
                else
                {
                    // It is only expected that output adapters will be mapped to this handler, but in case
                    // another adapter type uses this handler we will still display a message
                    OnStatusMessage(sender, "[{0}] CRITICAL: There are {1:N0} unprocessed measurements in the adapter queue - but sender \"{2}\" is not an IOutputAdapter, so no evasive action can be exercised.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements, sender.GetType().Name);
                }
            }
            else if (unprocessedMeasurements > m_measurementWarningThreshold)
            {
                if (unprocessedMeasurements >= m_measurementDumpingThreshold - m_measurementWarningThreshold)
                    OnStatusMessage(sender, "[{0}] CRITICAL: There are {1:N0} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
                else
                    OnStatusMessage(sender, "[{0}] There are {1:N0} unprocessed measurements in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedMeasurements);
            }

            // Bubble message up to any event subscribers
            OnUnprocessedMeasurements(sender, e.Argument);
        }

        /// <summary>
        /// Event handler for processing complete notifications from input adapters.
        /// </summary>
        /// <param name="sender">Event source reference to input adapter that is reporting processing completion.</param>
        /// <param name="e">Event arguments for event, if any; otherwise <see cref="EventArgs.Empty"/>.</param>
        public virtual void ProcessingCompleteHandler(object sender, EventArgs e)
        {
            OnStatusMessage(sender, "[{0}] Processing completed.", UpdateType.Information, GetDerivedName(sender));

            // Bubble message up to any event subscribers
            OnProcessingComplete(sender, e);
        }

        /// <summary>
        /// Event handler for disposed events from all adapters.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void DisposedHandler(object sender, EventArgs e)
        {
            OnStatusMessage(sender, "[{0}] Disposed.", UpdateType.Information, GetDerivedName(sender));

            // Bubble message up to any event subscribers
            OnDisposed(sender);
        }

        // Bubble routing table messages out through Iaon session
        private void m_routingTables_StatusMessage(object sender, EventArgs<string> e)
        {
            OnStatusMessage(this, e.Argument);
        }

        // Bubble routing table exceptions out through Iaon session
        private void m_routingTables_ProcessException(object sender, EventArgs<Exception> e)
        {
            ProcessExceptionHandler(sender, e);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(IaonSession), MessageClass.Framework);
        private static DataSet s_currentRealTimeConfiguration;
        private static DataSet s_currentTemporalConfiguration;

        // Static Methods

        /// <summary>
        /// Extracts a configuration that supports temporal processing from an existing real-time configuration.
        /// </summary>
        /// <param name="realtimeConfiguration">Real-time <see cref="DataSet"/> configuration.</param>
        /// <returns>A new <see cref="DataSet"/> configuration for adapters that support temporal processing.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static DataSet ExtractTemporalConfiguration(DataSet realtimeConfiguration)
        {
            // Since the same real-time configuration is shared among all adapters we can return
            // existing cached temporal configuration if real-time configuration hasn't changed
            if ((object)s_currentRealTimeConfiguration != null && (object)s_currentTemporalConfiguration != null && ReferenceEquals(s_currentRealTimeConfiguration, realtimeConfiguration))
                return s_currentTemporalConfiguration;

            try
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

                // Cache new temporal configuration
                s_currentRealTimeConfiguration = realtimeConfiguration;
                s_currentTemporalConfiguration = temporalConfiguration;

                return temporalConfiguration;
            }
            catch
            {
                s_currentRealTimeConfiguration = null;
                s_currentTemporalConfiguration = null;
                throw;
            }
        }

        #endregion
    }
}