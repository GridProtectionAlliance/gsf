//******************************************************************************************************
//  IaonSession.cs - Gbtc
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
//  08/23/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/04/2013 - Stephen C. Wills
//       Updated to process time-series entities.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.Configuration;
using GSF.TimeSeries.Routing;

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
        /// Event is raised when <see cref="IAdapter.InputSignals"/> are updated.
        /// </summary>
        public event EventHandler InputSignalsUpdated;

        /// <summary>
        /// Event is raised when <see cref="IAdapter.OutputSignals"/> are updated.
        /// </summary>
        public event EventHandler OutputSignalsUpdated;

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
        /// Event is raised every five seconds allowing host to track total number of unprocessed time-series entities.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each <see cref="IOutputAdapter"/> implementation reports its current queue size of unprocessed
        /// entities so that if queue size reaches an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed entities.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnprocessedEntities;

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
        private DataSet m_dataSource;
        private readonly object m_dataSourceLock;
        private InputAdapterCollection m_inputAdapters;
        private ActionAdapterCollection m_actionAdapters;
        private OutputAdapterCollection m_outputAdapters;
        private bool m_temporalSession;
        private bool m_temporalSupportEvaluated;
        private int m_processingInterval;
        private readonly ConcurrentDictionary<object, string> m_derivedNameCache;
        private ISet<Guid> m_inputSignalsRestriction;
        private readonly int m_entityWarningThreshold;
        private readonly int m_entityDumpingThreshold;
        private readonly int m_defaultSampleSizeWarningThreshold;
        private string m_name;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="IaonSession"/>.
        /// </summary>
        public IaonSession()
            : this(false)
        {
        }

        /// <summary>
        /// Creates a new <see cref="IaonSession"/> using the existing data source.
        /// </summary>
        /// <param name="dataSource">Preexisting data source.</param>
        /// <param name="temporalSession">Determines if Iaon Session should be for temporal use.</param>
        public IaonSession(DataSet dataSource, bool temporalSession)
            : this(temporalSession)
        {
            if (temporalSession && (object)dataSource == null)
                throw new ArgumentNullException("dataSource", "Cannot establish a temporal Iaon Session without a preexisting data source.");

            m_dataSource = dataSource;
            m_inputAdapters.DataSource = m_dataSource;
            m_actionAdapters.DataSource = m_dataSource;
            m_outputAdapters.DataSource = m_dataSource;
            m_temporalSupportEvaluated = true;
        }

        /// <summary>
        /// Creates a new <see cref="IaonSession"/>.
        /// </summary>
        /// <param name="temporalSession">Determines if Iaon Session should be for temporal use.</param>
        protected IaonSession(bool temporalSession)
        {
            ConfigurationFile configFile = ConfigurationFile.Current;

            // Initialize system settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];

            // TODO: In future versions we may consider removing the notion of a "node" from the schema since this doesn't add significant value - it's just as easy to add another schema in most cases
            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");

            m_nodeID = systemSettings["NodeID"].ValueAs<Guid>();

            // TODO: Move these threshold settings to base class implementation of what will become "QueuedOutputAdapterBase.cs"
            // Initialize threshold settings
            CategorizedSettingsElementCollection thresholdSettings = configFile.Settings["thresholdSettings"];

            thresholdSettings.Add("EntityWarningThreshold", "100000", "Number of unarchived time-series entities allowed in any output adapter queue before displaying a warning message");
            thresholdSettings.Add("EntityDumpingThreshold", "500000", "Number of unarchived time-series entities allowed in any output adapter queue before taking evasive action and dumping data");
            thresholdSettings.Add("DefaultSampleSizeWarningThreshold", "10", "Default number of unpublished samples (in seconds) allowed in any action adapter queue before displaying a warning message");

            m_entityWarningThreshold = thresholdSettings["EntityWarningThreshold"].ValueAsInt32();
            m_entityDumpingThreshold = thresholdSettings["EntityDumpingThreshold"].ValueAsInt32();
            m_defaultSampleSizeWarningThreshold = thresholdSettings["DefaultSampleSizeWarningThreshold"].ValueAsInt32();

            // Create a cache for derived adapter names
            m_derivedNameCache = new ConcurrentDictionary<object, string>();

            // Create a new set of routing tables
            m_routingTables = new RoutingTables();
            m_routingTables.StatusMessage += m_routingTables_StatusMessage;
            m_routingTables.ProcessException += m_routingTables_ProcessException;

            // Create input adapters collection
            m_inputAdapters = new InputAdapterCollection(temporalSession);
            m_inputAdapters.NewEntities += m_routingTables.RoutingEventHandler;
            m_inputAdapters.ProcessingComplete += ProcessingCompleteHandler;
            m_inputAdapters.StatusMessage += StatusMessageHandler;
            m_inputAdapters.ProcessException += ProcessExceptionHandler;
            m_inputAdapters.InputSignalsUpdated += InputSignalsUpdatedHandler;
            m_inputAdapters.OutputSignalsUpdated += OutputSignalsUpdatedHandler;
            m_inputAdapters.ConfigurationChanged += ConfigurationChangedHandler;
            m_inputAdapters.Disposed += DisposedHandler;

            // Create action adapters collection
            m_actionAdapters = new ActionAdapterCollection(temporalSession);
            m_actionAdapters.NewEntities += m_routingTables.RoutingEventHandler;
            m_actionAdapters.UnpublishedSamples += UnpublishedSamplesHandler;
            m_actionAdapters.StatusMessage += StatusMessageHandler;
            m_actionAdapters.ProcessException += ProcessExceptionHandler;
            m_actionAdapters.InputSignalsUpdated += InputSignalsUpdatedHandler;
            m_actionAdapters.OutputSignalsUpdated += OutputSignalsUpdatedHandler;
            m_actionAdapters.ConfigurationChanged += ConfigurationChangedHandler;
            m_actionAdapters.Disposed += DisposedHandler;

            // Create output adapters collection
            m_outputAdapters = new OutputAdapterCollection(temporalSession);
            m_outputAdapters.UnprocessedEntities += UnprocessedEntitiesHandler;
            m_outputAdapters.StatusMessage += StatusMessageHandler;
            m_outputAdapters.ProcessException += ProcessExceptionHandler;
            m_outputAdapters.InputSignalsUpdated += InputSignalsUpdatedHandler;
            m_outputAdapters.OutputSignalsUpdated += OutputSignalsUpdatedHandler;
            m_outputAdapters.ConfigurationChanged += ConfigurationChangedHandler;
            m_outputAdapters.Disposed += DisposedHandler;

            // Associate adapter collections with routing tables
            m_routingTables.InputAdapters = m_inputAdapters;
            m_routingTables.ActionAdapters = m_actionAdapters;
            m_routingTables.OutputAdapters = m_outputAdapters;

            m_dataSourceLock = new object();
            m_processingInterval = -1;
            m_temporalSession = temporalSession;
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
        /// Gets an enumeration of the input, action and output adapter collections.
        /// </summary>
        protected IEnumerable<IAdapterCollection> AdapterCollections
        {
            get
            {
                return new IAdapterCollection[] { m_inputAdapters, m_actionAdapters, m_outputAdapters };
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
        /// Gets or sets a routing table restriction for a collection of input signals.
        /// </summary>
        /// <remarks>
        /// This is useful for creating a new IaonSession over which only a few key signals are
        /// actually desired, e.g., a temporal session from a remote subscription. When specified,
        /// all signals that are required to produce the desired input signals will be included
        /// in the routing paths, but no more.
        /// </remarks>
        public virtual ISet<Guid> InputSignalsRestriction
        {
            get
            {
                return m_inputSignalsRestriction;
            }
            set
            {
                m_inputSignalsRestriction = value;
            }
        }

        /// <summary>
        /// Gets or sets the configuration <see cref="DataSet"/> for this <see cref="IaonSession"/>.
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                lock (m_dataSourceLock)
                {
                    DataSet originalDataSource = m_dataSource;

                    m_dataSource = value;
                    m_inputAdapters.DataSource = m_dataSource;
                    m_actionAdapters.DataSource = m_dataSource;
                    m_outputAdapters.DataSource = m_dataSource;
                    m_temporalSupportEvaluated = false;

                    if ((object)originalDataSource != null)
                        originalDataSource.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets or sets the processing interval for all adapter collections in this <see cref="IaonSession"/>.
        /// </summary>
        public virtual int ProcessingInterval
        {
            get
            {
                return m_processingInterval;
            }
            set
            {
                m_processingInterval = value;
                m_inputAdapters.ProcessingInterval = m_processingInterval;
                m_actionAdapters.ProcessingInterval = m_processingInterval;
                m_outputAdapters.ProcessingInterval = m_processingInterval;
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
                return m_name.ToNonNullString("[IaonSession]");
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
                status.AppendLine(m_inputAdapters.Status);

                status.AppendLine();
                status.AppendLine(">> Action Adapters:");
                status.AppendLine();
                status.AppendLine(m_actionAdapters.Status);

                status.AppendLine();
                status.AppendLine(">> Output Adapters:");
                status.AppendLine();
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
                        if ((object)m_inputAdapters != null)
                        {
                            m_inputAdapters.Stop();
                            m_inputAdapters.NewEntities -= m_routingTables.RoutingEventHandler;
                            m_inputAdapters.ProcessingComplete -= ProcessingCompleteHandler;
                            m_inputAdapters.StatusMessage -= StatusMessageHandler;
                            m_inputAdapters.ProcessException -= ProcessExceptionHandler;
                            m_inputAdapters.InputSignalsUpdated -= InputSignalsUpdatedHandler;
                            m_inputAdapters.OutputSignalsUpdated -= OutputSignalsUpdatedHandler;
                            m_inputAdapters.ConfigurationChanged -= ConfigurationChangedHandler;
                            m_inputAdapters.Disposed -= DisposedHandler;
                            m_inputAdapters.Dispose();
                        }
                        m_inputAdapters = null;

                        // Dispose action adapters collection
                        if ((object)m_actionAdapters != null)
                        {
                            m_actionAdapters.Stop();
                            m_actionAdapters.NewEntities -= m_routingTables.RoutingEventHandler;
                            m_actionAdapters.UnpublishedSamples -= UnpublishedSamplesHandler;
                            m_actionAdapters.StatusMessage -= StatusMessageHandler;
                            m_actionAdapters.ProcessException -= ProcessExceptionHandler;
                            m_actionAdapters.InputSignalsUpdated -= InputSignalsUpdatedHandler;
                            m_actionAdapters.OutputSignalsUpdated -= OutputSignalsUpdatedHandler;
                            m_actionAdapters.ConfigurationChanged -= ConfigurationChangedHandler;
                            m_actionAdapters.Disposed -= DisposedHandler;
                            m_actionAdapters.Dispose();
                        }
                        m_actionAdapters = null;

                        // Dispose output adapters collection
                        if ((object)m_outputAdapters != null)
                        {
                            m_outputAdapters.Stop();
                            m_outputAdapters.UnprocessedEntities -= UnprocessedEntitiesHandler;
                            m_outputAdapters.StatusMessage -= StatusMessageHandler;
                            m_outputAdapters.ProcessException -= ProcessExceptionHandler;
                            m_outputAdapters.InputSignalsUpdated -= InputSignalsUpdatedHandler;
                            m_outputAdapters.OutputSignalsUpdated -= OutputSignalsUpdatedHandler;
                            m_outputAdapters.ConfigurationChanged -= ConfigurationChangedHandler;
                            m_outputAdapters.Disposed -= DisposedHandler;
                            m_outputAdapters.Dispose();
                        }
                        m_outputAdapters = null;

                        // Dispose of routing tables
                        if ((object)m_routingTables != null)
                        {
                            m_routingTables.StatusMessage -= m_routingTables_StatusMessage;
                            m_routingTables.ProcessException -= m_routingTables_ProcessException;
                            m_routingTables.Dispose();
                        }
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
        /// Initialize adapters.
        /// </summary>
        /// <param name="delayAutoStart">Determines if adapters should delay auto-start until after initialization.</param>
        public virtual void Initialize(bool delayAutoStart = false)
        {
            if ((object)m_dataSource == null)
                throw new NullReferenceException("No data source for IaonSession has been defined - cannot initialize Iaon Session");

            // Set desired delay auto-start state for collections
            m_inputAdapters.DelayAutoStart = delayAutoStart;
            m_actionAdapters.DelayAutoStart = delayAutoStart;
            m_outputAdapters.DelayAutoStart = delayAutoStart;

            // Initialize adapter collections
            m_inputAdapters.Initialize();
            m_actionAdapters.Initialize();
            m_outputAdapters.Initialize();

            // Start adapter collections - due to adapter initialization sequence, adapters may already
            // be automatically starting but this step also enables adapter monitoring by the collections
            if (!delayAutoStart)
                Start();

            if (!m_temporalSession)
            {
                // After adapters have been initialized we begin the process to determine if adapters support temporal processing
                Thread itemThread = new Thread(WaitAndEvaluateTemporalAdapterSupport);
                itemThread.IsBackground = true;
                itemThread.Start();
            }
        }

        /// <summary>
        /// Create newly defined adapters and remove adapters that are no longer present in the adapter collection configurations. 
        /// </summary>
        public virtual void UpdateCollectionConfigurations()
        {
            foreach (IAdapterCollection adapterCollection in AdapterCollections)
            {
                string dataMember = adapterCollection.DataMember;

                if (DataSource.Tables.Contains(dataMember))
                {
                    // Remove adapters that are no longer present in the configuration
                    for (int i = adapterCollection.Count - 1; i >= 0; i--)
                    {
                        IAdapter adapter = adapterCollection[i];
                        DataRow[] adapterRows = DataSource.Tables[dataMember].Select(string.Format("ID = {0}", adapter.ID));

                        if (adapterRows.Length == 0 && adapter.ID != 0)
                        {
                            try
                            {
                                adapter.Stop();
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(this, new InvalidOperationException(string.Format("Exception while stopping adapter {0}: {1}", adapter.Name, ex.Message), ex));
                            }

                            adapterCollection.Remove(adapter);
                        }
                    }

                    // Create newly defined adapters
                    DataRow[] rows;

                    if (m_temporalSession)
                        rows = DataSource.Tables[dataMember].Select("TemporalSession <> 0");
                    else
                        rows = DataSource.Tables[dataMember].Select();

                    foreach (DataRow row in rows)
                    {
                        IAdapter adapter;

                        if (!adapterCollection.TryGetAdapterByID(uint.Parse(row["ID"].ToNonNullString("0")), out adapter) && adapterCollection.TryCreateAdapter(row, out adapter))
                            adapterCollection.Add(adapter);
                    }
                }

                // Reassess temporal support when new adapters may have come online
                m_temporalSupportEvaluated = false;

                if (!m_temporalSession)
                {
                    // After adapters have been initialized we begin the process to determine if adapters support temporal processing
                    Thread itemThread = new Thread(WaitAndEvaluateTemporalAdapterSupport);
                    itemThread.IsBackground = true;
                    itemThread.Start();
                }
            }
        }

        /// <summary>
        /// Attempts to get any adapter in all collections with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <param name="adapterCollection">Adapter collection reference if <paramref name="adapter"/> is found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="id"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAnyAdapterByID(uint id, out IAdapter adapter, out IAdapterCollection adapterCollection)
        {
            foreach (IAdapterCollection collection in AdapterCollections)
            {
                if (collection.TryGetAdapterByID(id, out adapter))
                {
                    adapterCollection = collection;
                    return true;
                }
            }

            adapter = null;
            adapterCollection = null;
            return false;
        }

        /// <summary>
        /// Attempts to get any adapter in all collections with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <param name="adapterCollection">Adapter collection reference if <paramref name="adapter"/> is found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAnyAdapterByName(string name, out IAdapter adapter, out IAdapterCollection adapterCollection)
        {
            foreach (IAdapterCollection collection in AdapterCollections)
            {
                if (collection.TryGetAdapterByName(name, out adapter))
                {
                    adapterCollection = collection;
                    return true;
                }
            }

            adapter = null;
            adapterCollection = null;
            return false;
        }

        /// <summary>
        /// Attempts to initialize (or reinitialize) an individual <see cref="IAdapter"/> based on its ID from any collection.
        /// </summary>
        /// <param name="id">The numeric ID associated with the <see cref="IAdapter"/> to be initialized.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// This method traverses all collections looking for an adapter with the specified ID.
        /// </remarks>
        public virtual bool TryInitializeAdapterByID(uint id)
        {
            foreach (IAdapterCollection collection in AdapterCollections)
            {
                if (collection.TryInitializeAdapterByID(id))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets derived name of specified object.
        /// </summary>
        /// <param name="sender">Sending object from which to derive name.</param>
        /// <returns>Derived name of specified object.</returns>
        public virtual string GetDerivedName(object sender)
        {
            return m_derivedNameCache.GetOrAdd(sender, key =>
            {
                string name = null;
                IProvideStatus statusProvider = key as IProvideStatus;

                if ((object)statusProvider != null)
                    name = statusProvider.Name;
                else if (key is string)
                    name = (string)key;

                if (string.IsNullOrWhiteSpace(name))
                    name = key.GetType().Name;

                if (!string.IsNullOrWhiteSpace(m_name))
                    name += "#" + m_name;

                return name;
            });
        }

        /// <summary>
        /// Recalculates routing tables as long as all adapters have been initialized.
        /// </summary>
        public virtual void RecalculateRoutingTables()
        {
            if ((object)m_routingTables != null && m_inputAdapters.Initialized && m_actionAdapters.Initialized && m_outputAdapters.Initialized)
                m_routingTables.CalculateRoutingTables(InputSignalsRestriction);
        }

        /// <summary>
        /// Starts each <see cref="IAdapter"/> implementation in all adapter collections.
        /// </summary>
        public virtual void Start()
        {
            m_inputAdapters.Start();
            m_actionAdapters.Start();
            m_outputAdapters.Start();
        }

        /// <summary>
        /// Stops each <see cref="IAdapter"/> implementation in all adapter collections.
        /// </summary>
        public virtual void Stop()
        {
            m_inputAdapters.Stop();
            m_actionAdapters.Stop();
            m_outputAdapters.Stop();
        }

        /// <summary>
        /// Defines a temporal processing constraint for each of the adapter collections and applies this constraint to all adapters.
        /// </summary>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint.</param>
        /// <remarks>
        /// <para>
        /// This method defines a temporal processing constraint for an adapter, i.e., the start and stop time over which an
        /// adapter will process data. Actual implementation of the constraint will be adapter specific. Implementations
        /// should be able to dynamically handle multiple calls to this function with new constraints. Passing in <c>null</c>
        /// for the <paramref name="startTime"/> and <paramref name="stopTime"/> should cancel the temporal constraint and
        /// return the adapter to standard / real-time operation.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public virtual void SetTemporalConstraint(string startTime, string stopTime, string constraintParameters)
        {
            // Apply temporal constraint to all adapters in each collection
            m_inputAdapters.SetTemporalConstraint(startTime, stopTime, constraintParameters);
            m_actionAdapters.SetTemporalConstraint(startTime, stopTime, constraintParameters);
            m_outputAdapters.SetTemporalConstraint(startTime, stopTime, constraintParameters);
        }

        /// <summary>
        /// Gets flag that determines if temporal processing is supported in this <see cref="IaonSession"/>.
        /// </summary>
        /// <param name="collection">Name of collection over which to check support (e.g., "InputAdapters"); or <c>null</c> for all collections.</param>
        /// <returns><c>true</c> if temporal processing is supported in this <see cref="IaonSession"/>; otherwise <c>false</c>.</returns>
        public virtual bool TemporalProcessingSupportExists(string collection = null)
        {
            lock (m_dataSourceLock)
            {
                if (!m_temporalSupportEvaluated)
                    throw new InvalidOperationException("Temporal processing support has yet to be evaluated.");

                if (string.IsNullOrWhiteSpace(collection))
                    return ((object)m_dataSource != null && (
                        (m_dataSource.Tables["InputAdapters"].Select("TemporalSupport <> 0").Length > 0) ||
                        (m_dataSource.Tables["ActionAdapters"].Select("TemporalSupport <> 0").Length > 0) ||
                        (m_dataSource.Tables["OutputAdapters"].Select("TemporalSupport <> 0").Length > 0)));

                switch (collection.ToLower())
                {
                    case "inputadapters":
                        return ((object)m_dataSource != null && m_dataSource.Tables["InputAdapters"].Select("TemporalSupport <> 0").Length > 0);
                    case "actionadapters":
                        return ((object)m_dataSource != null && m_dataSource.Tables["ActionAdapters"].Select("TemporalSupport <> 0").Length > 0);
                    case "outputadapters":
                        return ((object)m_dataSource != null && m_dataSource.Tables["OutputAdapters"].Select("TemporalSupport <> 0").Length > 0);
                }
            }

            return false;
        }

        // Attempt to wait for adapter initialization before initial temporal support evaluation
        private void WaitAndEvaluateTemporalAdapterSupport(object state)
        {
            Ticks startTime = DateTime.UtcNow.Ticks;

            // We'll hold the lock while we wait or timeout to give as many adapters as possible a chance to get started before
            // the official temporal support evaluation. This will limit reload config operations to once every 15 seconds.
            lock (m_dataSourceLock)
            {
                // TODO: See if there is a possible improvement to this behavior - could get a more immediate notification from collection that all adapters are "initializing" (at least after parse of any temporal support flags)

                while ((DateTime.UtcNow.Ticks - startTime).ToMilliseconds() < AdapterBase.DefaultInitializationTimeout && (
                       !m_inputAdapters.All<IInputAdapter>(adapter => adapter.Initialized) ||
                       !m_actionAdapters.All<IActionAdapter>(adapter => adapter.Initialized) ||
                       !m_outputAdapters.All<IOutputAdapter>(adapter => adapter.Initialized)))
                {
                    Thread.Sleep(100);
                }

                // Establish temporal configuration if it hasn't been
                if (!m_temporalSupportEvaluated)
                {
                    IAdapterCollection collection;

                    foreach (DataTable table in m_dataSource.Tables)
                    {
                        collection = null;

                        switch (table.TableName.ToLower())
                        {
                            case "inputadapters":
                                collection = m_inputAdapters;
                                break;
                            case "actionadapters":
                                collection = m_actionAdapters;
                                break;
                            case "outputadapters":
                                collection = m_outputAdapters;
                                break;
                        }

                        if ((object)collection == null)
                            continue;

                        // Add a temporal support column to the adapter definition table
                        if (!table.Columns.Contains("TemporalSupport"))
                            table.Columns.Add("TemporalSupport", typeof(bool));

                        // Determine which adapters support temporal processing
                        foreach (IAdapter adapter in collection)
                        {
                            DataRow[] rows = table.Select(string.Format("ID = {0}", adapter.ID));

                            if (rows.Length > 0)
                                rows[0]["TemporalSupport"] = adapter.SupportsTemporalProcessing;
                        }
                    }

                    m_temporalSupportEvaluated = true;
                }
            }
        }

        // TODO: Perhaps all status message events should be modified with an optional "UpdateType" parameter 

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
        /// Raises <see cref="InputSignalsUpdated"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnInputSignalsUpdated(object sender)
        {
            if ((object)InputSignalsUpdated != null)
                InputSignalsUpdated(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="OutputSignalsUpdated"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        protected virtual void OnOutputSignalsUpdated(object sender)
        {
            if ((object)OutputSignalsUpdated != null)
                OutputSignalsUpdated(sender, EventArgs.Empty);
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
        /// Raises the <see cref="UnprocessedEntities"/> event.
        /// </summary>
        /// <param name="sender">Object source raising the event.</param>
        /// <param name="unprocessedEntities">Total entities in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedEntities(object sender, int unprocessedEntities)
        {
            if ((object)UnprocessedEntities != null)
                UnprocessedEntities(sender, new EventArgs<int>(unprocessedEntities));
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
        /// Event handler for updates to adapter input signal definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void InputSignalsUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter input signals are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnInputSignalsUpdated(sender);
        }

        /// <summary>
        /// Event handler for updates to adapter output signal definitions.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments, if any.</param>
        public virtual void OutputSignalsUpdatedHandler(object sender, EventArgs e)
        {
            // When adapter output signals are dynamically updated, routing tables need to be updated
            RecalculateRoutingTables();

            // Bubble message up to any event subscribers
            OnOutputSignalsUpdated(sender);
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

        // TODO: See if it is possible for this method to be replaced with existing UnprocessedEntities event since its only purpose is to notify user of queue size getting too large...

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

        // TODO: If a simple "OutputAdapterBase" is created then the following code will be specific to a "QueuedOutputAdapterBase" style class - perhaps handlers should be managed with "types" of base classes.

        /// <summary>
        /// Event handler for monitoring unprocessed time-series entities.
        /// </summary>
        /// <param name="sender">Event source reference to adapter, typically an output adapter, that is reporting the number of unprocessed entities.</param>
        /// <param name="e">Event arguments containing number of queued (i.e., unprocessed) entities in the source adapter.</param>
        /// <remarks>
        /// Time-series framework uses this handler to monitor the number of unprocessed entities in output adapters.<br/>
        /// This method is typically called once every five seconds.
        /// </remarks>
        public virtual void UnprocessedEntitiesHandler(object sender, EventArgs<int> e)
        {
            int unprocessedEntities = e.Argument;

            if (unprocessedEntities > m_entityDumpingThreshold)
            {
                IOutputAdapter outputAdapter = sender as IOutputAdapter;

                if (outputAdapter != null)
                {
                    // If an output adapter queue size exceeds the defined entity dumping threshold,
                    // then the queue will be truncated before system runs out of memory
                    outputAdapter.RemoveEntities(m_entityDumpingThreshold);
                    OnStatusMessage(sender, "[{0}] System exercised evasive action to conserve memory and dumped {1} unprocessed entities from the output queue :(", UpdateType.Alarm, outputAdapter.Name, m_entityDumpingThreshold);
                    OnStatusMessage(sender, "[{0}] NOTICE: Please adjust entity threshold settings and/or increase amount of available system memory.", UpdateType.Warning, outputAdapter.Name);
                }
                else
                {
                    // It is only expected that output adapters will be mapped to this handler, but in case
                    // another adapter type uses this handler we will still display a message
                    OnStatusMessage(sender, "[{0}] CRITICAL: There are {1} unprocessed entities in the adapter queue - but sender \"{2}\" is not an IOutputAdapter, so no evasive action can be exercised.", UpdateType.Warning, GetDerivedName(sender), unprocessedEntities, sender.GetType().Name);
                }
            }
            else if (unprocessedEntities > m_entityWarningThreshold)
            {
                if (unprocessedEntities >= m_entityDumpingThreshold - m_entityWarningThreshold)
                    OnStatusMessage(sender, "[{0}] CRITICAL: There are {1} unprocessed entities in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedEntities);
                else
                    OnStatusMessage(sender, "[{0}] There are {1} unprocessed entities in the output queue.", UpdateType.Warning, GetDerivedName(sender), unprocessedEntities);
            }

            // Bubble message up to any event subscribers
            OnUnprocessedEntities(sender, e.Argument);
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
    }
}