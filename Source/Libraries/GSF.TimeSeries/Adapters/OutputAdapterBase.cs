//******************************************************************************************************
//  OutputAdapterBase.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the base class for any outgoing data stream.
    /// </summary>
    /// <remarks>
    /// This base class acts as a measurement queue so that output adapters can temporarily go
    /// offline without losing any measurements to be processed. Derived classes are expected to
    /// override <see cref="ProcessMeasurements"/> to handle queued measurements.
    /// </remarks>
    public abstract class OutputAdapterBase : AdapterBase, IOutputAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Event is raised every five seconds allowing host to track total number of unprocessed measurements.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Base class reports current queue size of unprocessed measurements so that if queue size reaches
        /// an unhealthy threshold, host can take evasive action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed measurements.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnprocessedMeasurements;

        // Fields
        private List<string> m_inputSourceIDs;
        private readonly LongSynchronizedOperation m_connectionOperation;
        private SharedTimer m_connectionTimer;
        private SharedTimer m_monitorTimer;
        private string m_connectionInfo;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="OutputAdapterBase"/>.
        /// </summary>
        protected OutputAdapterBase()
        {
            MetadataRefreshOperation = new LongSynchronizedOperation(ExecuteMetadataRefresh, ex => OnProcessException(MessageLevel.Error, ex, nameof(ExecuteMetadataRefresh)))
            {
                IsBackground = true
            };

            InternalProcessQueue = ProcessQueue<IMeasurement>.CreateRealTimeQueue(ProcessMeasurements);
            InternalProcessQueue.ProcessException += m_measurementQueue_ProcessException;

            m_connectionOperation = new LongSynchronizedOperation(AttemptConnectionOperation, ex => OnProcessException(MessageLevel.Error, ex, nameof(AttemptConnectionOperation)))
            {
                IsBackground = true
            };

            m_connectionTimer = Common.TimerScheduler.CreateTimer(2000);
            m_connectionTimer.Elapsed += m_connectionTimer_Elapsed;

            m_connectionTimer.AutoReset = false;
            m_connectionTimer.Enabled = false;

            // We monitor total number of unarchived measurements every 5 seconds - this is a useful statistic to monitor, if
            // total number of unarchived measurements gets very large, measurement archival could be falling behind
            m_monitorTimer = Common.TimerScheduler.CreateTimer(5000);
            m_monitorTimer.Elapsed += m_monitorTimer_Elapsed;

            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="OutputAdapterBase"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get => base.DataSource;
            set
            {
                base.DataSource = value;
                RefreshMetadata();
            }
        }

        /// <summary>
        /// Gets or sets whether or not to automatically place measurements back into the processing
        /// queue if an exception occurs while processing.  Defaults to false.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines whether or not to automatically place measurements back into the processing queue if an exception occurs while processing.  Defaults to false.")]
        [DefaultValue(false)]
        public virtual bool RequeueOnException
        {
            get => InternalProcessQueue.RequeueOnException;
            set => InternalProcessQueue.RequeueOnException = value;
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter input measurements.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] InputSourceIDs
        {
            get => m_inputSourceIDs?.ToArray();
            set
            {
                if (value is null)
                {
                    m_inputSourceIDs = null;
                }
                else
                {
                    m_inputSourceIDs = new List<string>(value);
                    m_inputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                LoadInputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets input measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedInputMeasurementKeys { get; set; }

        /// <summary>
        /// Gets the flag that determines if measurements sent to this <see cref="OutputAdapterBase"/> are destined for archival.
        /// </summary>
        /// <remarks>
        /// This property allows the <see cref="OutputAdapterCollection"/> to calculate statistics on how many measurements have
        /// been archived per minute. Historians would normally set this property to <c>true</c>; other custom exports would set
        /// this property to <c>false</c>.
        /// </remarks>
        public abstract bool OutputIsForArchive { get; }

        /// <summary>
        /// Gets the flag indicating if this <see cref="OutputAdapterBase"/> implementation supports temporal processing.
        /// </summary>
        /// <remarks>
        /// For output adapters that archive data it is assumed that the desired behavior will be to not support temporal processing
        /// since the data being processed has already been archived (i.e., no need to attempt to rearchive old data). As a result
        /// the default behavior for an output adapter is to not support temporal processing when <see cref="OutputIsForArchive"/>
        /// is <c>true</c>. If you have an output adapter that you want to support temporal data processing independent of the
        /// <see cref="OutputIsForArchive"/> value, then override this property and force the base value to the desired state.
        /// </remarks>
        public override bool SupportsTemporalProcessing => !OutputIsForArchive;

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the output adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public override int ProcessingInterval
        {
            get => base.ProcessingInterval;
            set
            {
                if (base.ProcessingInterval == value)
                    return;

                base.ProcessingInterval = value;
                bool enabled = false;
                bool requeueOnException = ProcessQueue<IMeasurement>.DefaultRequeueOnException;
                IMeasurement[] unprocessedMeasurements = null;

                if (InternalProcessQueue is not null)
                {
                    enabled = InternalProcessQueue.Enabled;
                    requeueOnException = InternalProcessQueue.RequeueOnException;

                    if (InternalProcessQueue.Count > 0)
                    {
                        InternalProcessQueue.Stop();
                        unprocessedMeasurements = InternalProcessQueue.ToArray();
                    }

                    InternalProcessQueue.ProcessException -= m_measurementQueue_ProcessException;
                    InternalProcessQueue.Dispose();
                }

                if (value <= 0)
                {
                    // The default processing interval is "as fast as possible"
                    InternalProcessQueue = ProcessQueue<IMeasurement>.CreateRealTimeQueue(ProcessMeasurements);
                }
                else
                {
                    // Set the desired processing interval
                    InternalProcessQueue = ProcessQueue<IMeasurement>.CreateSynchronousQueue(ProcessMeasurements);
                    InternalProcessQueue.ProcessInterval = value;
                }

                InternalProcessQueue.ProcessException += m_measurementQueue_ProcessException;
                InternalProcessQueue.RequeueOnException = requeueOnException;

                // Requeue any existing measurements
                if (unprocessedMeasurements is not null && unprocessedMeasurements.Length > 0)
                    InternalProcessQueue.AddRange(unprocessedMeasurements);

                InternalProcessQueue.Enabled = enabled;
            }
        }

        /// <summary>
        /// Gets flag that determines if the data output stream connects asynchronously.
        /// </summary>
        /// <remarks>
        /// Derived classes should return true when data output stream connects asynchronously, otherwise return false.
        /// </remarks>
        protected abstract bool UseAsyncConnect { get; }

        /// <summary>
        /// Gets or sets the connection attempt interval, in milliseconds, for the data output adapter.
        /// </summary>
        protected double ConnectionAttemptInterval
        {
            get
            {
                if (m_connectionTimer is not null)
                    return m_connectionTimer.Interval;

                return 2000.0D;
            }
            set
            {
                if (m_connectionTimer is not null)
                    m_connectionTimer.Interval = (int)value;
            }
        }

        /// <summary>
        /// Gets the operation that calls <see cref="ExecuteMetadataRefresh"/>.
        /// </summary>
        protected LongSynchronizedOperation MetadataRefreshOperation { get; }

        /// <summary>
        /// Allows derived class access to internal processing queue.
        /// </summary>
        protected ProcessQueue<IMeasurement> InternalProcessQueue { get; private set; }

        /// <summary>
        /// Returns the detailed status of the data input source.  Derived classes should extend status with implementation specific information.
        /// </summary>
        public override string Status
        {
            get
            {
                const int MaxMeasurementsToShow = 10;

                StringBuilder status = new();

                status.Append(base.Status);

                if (RequestedInputMeasurementKeys is not null && RequestedInputMeasurementKeys.Length > 0)
                {
                    status.AppendLine($"      Requested input keys: {RequestedInputMeasurementKeys.Length:N0} defined measurements");
                    status.AppendLine();

                    for (int i = 0; i < Math.Min(RequestedInputMeasurementKeys.Length, MaxMeasurementsToShow); i++)
                        status.AppendLine(RequestedInputMeasurementKeys[i].ToString().TruncateRight(25).CenterText(50));

                    if (RequestedInputMeasurementKeys.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".CenterText(50));

                    status.AppendLine();
                }

                status.AppendLine($"     Source ID filter list: {(m_inputSourceIDs is null ? "[No filter applied]" : m_inputSourceIDs.ToDelimitedString(','))}");
                status.AppendLine($"   Asynchronous connection: {UseAsyncConnect}");
                status.AppendLine($"     Output is for archive: {OutputIsForArchive}");
                status.Append(InternalProcessQueue.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the <see cref="AdapterBase"/> will produce, if any.
        /// </summary>
        /// <remarks>
        /// Redefined to hide attributes defined in the base class.
        /// </remarks>
        public new virtual IMeasurement[] OutputMeasurements
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="OutputAdapterBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_connectionTimer is not null)
                {
                    m_connectionTimer.Elapsed -= m_connectionTimer_Elapsed;
                    m_connectionTimer.Dispose();
                }
                m_connectionTimer = null;

                if (m_monitorTimer is not null)
                {
                    m_monitorTimer.Elapsed -= m_monitorTimer_Elapsed;
                    m_monitorTimer.Dispose();
                }
                m_monitorTimer = null;

                if (InternalProcessQueue is not null)
                {
                    InternalProcessQueue.ProcessException -= m_measurementQueue_ProcessException;
                    InternalProcessQueue.Dispose();
                }
                InternalProcessQueue = null;
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="OutputAdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            // Load optional parameters
            if (settings.TryGetValue(nameof(InputSourceIDs), out string setting) || settings.TryGetValue("sourceids", out setting))
                InputSourceIDs = setting.Split(',');
            else
                InputSourceIDs = null;

            if (settings.TryGetValue(nameof(RequeueOnException), out setting))
                RequeueOnException = setting.ParseBoolean();

            // Start data monitor...
            m_monitorTimer?.Start();
        }

        /// <summary>
        /// Initiates request for metadata refresh for <see cref="OutputAdapterBase"/>, if implemented.
        /// </summary>
        [AdapterCommand("Requests metadata refresh of output adapter.", "Administrator", "Editor")]
        [SuppressMessage("SonarQube.Miscellaneous", "S1656", Justification = "Self assignment of \"InputSourceIDs\" initiates parse and load of source IDs.")]
        public void RefreshMetadata()
        {
            // Force a recalculation of input measurement keys so that system can appropriately update routing tables
            InputMeasurementKeys = Settings.TryGetValue(nameof(InputMeasurementKeys), out string setting) ? 
                ParseInputMeasurementKeys(DataSource, true, setting) : 
                Array.Empty<MeasurementKey>();

            InputSourceIDs = InputSourceIDs;
            MetadataRefreshOperation.RunOnceAsync();
        }

        /// <summary>
        /// Executes the metadata refresh in a synchronous fashion.
        /// </summary>
        protected virtual void ExecuteMetadataRefresh()
        {
        }

        /// <summary>
        /// Starts this <see cref="OutputAdapterBase"/> and initiates connection cycle to data output stream.
        /// </summary>		
        public override void Start()
        {
            base.Start();

            // Start the connection cycle
            if (m_connectionTimer is not null)
                m_connectionTimer.Enabled = true;

            // Make sure data monitor is started...
            if (m_monitorTimer is not null && !m_monitorTimer.Enabled)
                m_monitorTimer.Start();
        }

        /// <summary>
        /// Attempts to connect to data output stream.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt connection to data output stream here.  Any exceptions thrown
        /// by this implementation will result in restart of the connection cycle.
        /// </remarks>
        protected abstract void AttemptConnection();

        /// <summary>
        /// Called when data output source connection is established.
        /// </summary>
        /// <remarks>
        /// Derived classes should call this method manually if <see cref="UseAsyncConnect"/> is <c>true</c>.
        /// </remarks>
        protected virtual void OnConnected()
        {
            // Start data processing thread
            InternalProcessQueue?.Start();
            OnStatusMessage(MessageLevel.Info, "Connection established.", "Connecting");
        }

        /// <summary>
        /// Stops this <see cref="OutputAdapterBase"/> and disconnects from data output stream.
        /// </summary>
        public override void Stop()
        {
            try
            {
                bool performedDisconnect = Enabled;

                // Stop the connection cycle
                if (m_connectionTimer is not null)
                    m_connectionTimer.Enabled = false;

                base.Stop();

                // Stop data processing thread
                InternalProcessQueue.Stop();

                // Attempt disconnection from historian (e.g., consumer to call historian API disconnect function)
                AttemptDisconnection();

                if (performedDisconnect && !UseAsyncConnect)
                    OnDisconnected();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception occurred during disconnect: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Attempts to disconnect from data output stream.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt disconnect from data output stream here.  Any exceptions thrown
        /// by this implementation will be reported to host via <see cref="AdapterBase.ProcessException"/> event.
        /// </remarks>
        protected abstract void AttemptDisconnection();

        /// <summary>
        /// Called when data input source is disconnected.
        /// </summary>
        /// <remarks>
        /// Derived classes should call this method manually if <see cref="UseAsyncConnect"/> is <c>true</c>.
        /// </remarks>
        protected virtual void OnDisconnected() => 
            OnStatusMessage(MessageLevel.Info, $"Disconnected from {m_connectionInfo ?? ConnectionInfo}.");

        /// <summary>
        /// Queues a single measurement for processing. Measurement is automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement) => 
            QueueMeasurementsForProcessing(new[] { measurement });

        /// <summary>
        /// Queues a collection of measurements for processing. Measurements are automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (m_disposed)
                return;

            IMeasurement[] collection = measurements as IMeasurement[] ?? measurements.ToArray();
            InternalProcessQueue.AddRange(collection);
            IncrementProcessedMeasurements(collection.Length);
        }

        /// <summary>
        /// Serializes measurements to data output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Derived classes must implement this function to process queued measurements.
        /// For example, this function would "archive" measurements if output adapter is for a historian.
        /// </para>
        /// <para>
        /// It is important that consumers "resume" connection cycle if processing fails (e.g., connection
        /// to archive is lost). Here is an example:
        /// <example>
        /// <code>
        /// protected virtual void ProcessMeasurements(IMeasurement[] measurements)
        /// {
        ///     try
        ///     {
        ///         // Process measurements...
        ///         foreach (IMeasurement measurement in measurement)
        ///         {
        ///             ArchiveMeasurement(measurement);
        ///         }
        ///     }
        ///     catch (Exception)
        ///     {                
        ///         // So long as user hasn't requested to stop, restart connection cycle
        ///         if (Enabled)
        ///             Start();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </para>
        /// </remarks>
        protected abstract void ProcessMeasurements(IMeasurement[] measurements);

        /// <summary>
        /// This removes a range of measurements from the internal measurement queue.
        /// </summary>
        /// <remarks>
        /// This method is typically only used to curtail size of measurement queue if it's getting too large.  If more points are
        /// requested than there are points available - all points in the queue will be removed.
        /// </remarks>
        public virtual void RemoveMeasurements(int total)
        {
            if (m_disposed)
                return;

            lock (InternalProcessQueue.SyncRoot)
                InternalProcessQueue.RemoveRange(0, Math.Min(total, InternalProcessQueue.Count));
        }

        /// <summary>
        /// Blocks the current thread, if the <see cref="OutputAdapterBase"/> is connected, until all items
        /// in <see cref="OutputAdapterBase"/> queue are processed, and then stops processing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is possible for items to be added to the queue while the flush is executing. The flush will continue to
        /// process items as quickly as possible until the queue is empty. Unless the user stops queuing items to be
        /// processed, the flush call may never return (not a happy situation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="OutputAdapterBase"/> does not clear queue prior to destruction. If the user fails to call
        /// this method before the class is destructed, there may be items that remain unprocessed in the queue.
        /// </para>
        /// </remarks>
        public virtual void Flush() => 
            InternalProcessQueue?.Flush();

        /// <summary>
        /// Raises the <see cref="UnprocessedMeasurements"/> event.
        /// </summary>
        /// <param name="unprocessedMeasurements">Total measurements in the queue that have not been processed.</param>
        protected virtual void OnUnprocessedMeasurements(int unprocessedMeasurements)
        {
            try
            {
                UnprocessedMeasurements?.Invoke(this, new EventArgs<int>(unprocessedMeasurements));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(UnprocessedMeasurements)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        private void AttemptConnectionOperation()
        {
            m_connectionInfo = null;

            try
            {
                // Cache connection info before possible failure in case connection switches to another target
                m_connectionInfo = ConnectionInfo;

                // So long as user hasn't requested to stop, attempt connection
                if (!Enabled)
                    return;

                OnStatusMessage(MessageLevel.Info, $"Attempting connection{(m_connectionInfo is null ? "" : $" to {m_connectionInfo}")}...");

                // Attempt connection to data output adapter (e.g., call historian API connect function).
                AttemptConnection();

                // Try to update connection info after successful connection attempt in case info is now available
                m_connectionInfo ??= ConnectionInfo;

                if (!UseAsyncConnect)
                    OnConnected();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Info, new ConnectionException($"Connection attempt failed{(m_connectionInfo is null ? "" : $" for {m_connectionInfo}")}: {ex.Message}", ex));

                // So long as user hasn't requested to stop, keep trying connection
                if (Enabled)
                    Start();
            }
        }

        private void m_connectionTimer_Elapsed(object sender, EventArgs<DateTime> e) => 
            m_connectionOperation.TryRunOnceAsync();

        // All we do here is expose the total number of unarchived measurements in the queue
        private void m_monitorTimer_Elapsed(object sender, EventArgs<DateTime> e) => 
            OnUnprocessedMeasurements(InternalProcessQueue.Count);

        // Bubble any exceptions occurring in the process queue to the base class event
        private void m_measurementQueue_ProcessException(object sender, EventArgs<Exception> e) => 
            OnProcessException(MessageLevel.Info, e.Argument);

        #endregion
    }
}