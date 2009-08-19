//*******************************************************************************************************
//  OutputAdapterBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TVA.Collections;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents that base class for any outgoing data stream.
    /// </summary>
    /// <remarks>
    /// This base class acts as a measurement queue so that output adapters can temporarily go
    /// offline without losing any measurements to be processed. Derived classes are expected to
    /// override <see cref="ProcessMeasurements"/> to handle queued measurements.
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class OutputAdapterBase : AdapterBase, IOutputAdapter
	{
        #region [ Members ]

        // Constants
        private const int ProcessedMeasurementInterval = 100000;

        // Events

        /// <summary>
        /// Event is raised every second allowing host to track total number of unprocessed measurements.
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
        private ProcessQueue<IMeasurement> m_measurementQueue;
        private long m_processedMeasurements;
        private List<string> m_sourceIDs;
        private System.Timers.Timer m_connectionTimer;
        private System.Timers.Timer m_monitorTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="OutputAdapterBase"/>.
        /// </summary>
        protected OutputAdapterBase()
        {
            m_measurementQueue = ProcessQueue<IMeasurement>.CreateRealTimeQueue(ProcessMeasurements);
            m_measurementQueue.ProcessException += m_measurementQueue_ProcessException;

            m_connectionTimer = new System.Timers.Timer();
            m_connectionTimer.Elapsed += m_connectionTimer_Elapsed;

            m_connectionTimer.AutoReset = false;
            m_connectionTimer.Interval = 2000;
            m_connectionTimer.Enabled = false;

            m_monitorTimer = new System.Timers.Timer();
            m_monitorTimer.Elapsed += m_monitorTimer_Elapsed;

            // We monitor total number of unarchived measurements every second - this is a useful statistic to monitor, if
            // total number of unarchived measurements gets very large, measurement archival could be falling behind
            m_monitorTimer.Interval = 1000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets whether or not to automatically place measurements back into the processing
        /// queue if an exception occurs while processing.  Defaults to false.
        /// </summary>
        public virtual bool RequeueOnException
        {
            get
            {
                return m_measurementQueue.RequeueOnException;
            }
            set
            {
                m_measurementQueue.RequeueOnException = value;
            }
        }

        /// <summary>
        /// Gets the total number of measurements processed thus far by the <see cref="OutputAdapterBase"/>.
        /// </summary>
        public virtual long ProcessedMeasurements
        {
            get
            {
                return m_processedMeasurements;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="IMeasurement"/> source ID's used to filter output measurements.
        /// </summary>
        /// <remarks>
        /// Set to <c>null</c> apply no filter (i.e., adapter expects all measurements).
        /// </remarks>
        public virtual string[] SourceIDs
        {
            get
            {
                if (m_sourceIDs == null)
                    return null;

                return m_sourceIDs.ToArray();
            }
            set
            {
                if (value == null)
                    m_sourceIDs = null;
                else
                {
                    m_sourceIDs = new List<string>(value);
                    m_sourceIDs.Sort();
                }
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="OutputAdapterBase"/> are
        /// destined for archival.
        /// </summary>
        /// <remarks>
        /// This property allows the <see cref="OutputAdapterCollection"/> to calculate statistics on how
        /// many measurements have been archived per minute. Historians would normally set this property
        /// to <c>true</c>; other custom exports would set this property to <c>false</c>.
        /// </remarks>
        public abstract bool OutputIsForArchive { get; }
        
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
                return m_connectionTimer.Interval;
            }
            set
            {
                m_connectionTimer.Interval = value;
            }
        }

        /// <summary>
        /// Allows derived class access to internal processing queue.
        /// </summary>
        protected ProcessQueue<IMeasurement> InternalProcessQueue
        {
            get
            {
                return m_measurementQueue;
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.  Derived classes should extend status with implementation specific information.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("     Source ID filter list: {0}", (m_sourceIDs == null ? "[No filter applied]" : m_sourceIDs.ToDelimitedString(',')));
                status.AppendLine();
                status.AppendFormat("    Processed measurements: {0}", m_processedMeasurements);
                status.AppendLine();
                status.AppendFormat("   Asynchronous connection: {0}", UseAsyncConnect);
                status.AppendLine();
                status.AppendFormat("     Output is for archive: {0}", OutputIsForArchive);
                status.AppendLine();
                status.Append(m_measurementQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
        
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="OutputAdapterBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_connectionTimer != null)
                        {
                            m_connectionTimer.Elapsed -= m_connectionTimer_Elapsed;
                            m_connectionTimer.Dispose();
                        }
                        m_connectionTimer = null;

                        if (m_monitorTimer != null)
                        {
                            m_monitorTimer.Elapsed -= m_monitorTimer_Elapsed;
                            m_monitorTimer.Dispose();
                        }
                        m_monitorTimer = null;

                        if (m_measurementQueue != null)
                        {
                            m_measurementQueue.ProcessException -= m_measurementQueue_ProcessException;
                            m_measurementQueue.Dispose();
                        }
                        m_measurementQueue = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="OutputAdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters
            if (settings.TryGetValue("sourceids", out setting))
                SourceIDs = setting.Split(',');
            else
                SourceIDs = null;
        }

        /// <summary>
        /// Starts this <see cref="OutputAdapterBase"/> and initiates connection cycle to data output stream.
        /// </summary>		
        public override void Start()
        {
            // Make sure we are disconnected before attempting a connection
            Stop();

            base.Start();

            // Start the connection cycle
            m_connectionTimer.Enabled = true;
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
        /// Called when data input source connection is established.
        /// </summary>
        /// <remarks>
        /// Derived classes should call this method manually if <see cref="UseAsyncConnect"/> is <c>true</c>.
        /// </remarks>
        protected virtual void OnConnected()
        {
            OnStatusMessage("Connection established.");
        }

        /// <summary>
        /// Stops this <see cref="OutputAdapterBase"/> and disconnects from data output stream.
        /// </summary>
        public override void Stop()
        {
            try
            {
                bool performedDisconnect = Enabled;

                base.Stop();

                // Stop data processing thread
                m_measurementQueue.Stop();

                // Stop data monitor...
                m_monitorTimer.Stop();

                // Attempt disconnection from historian (e.g., consumer to call historian API disconnect function)
                AttemptDisconnection();

                if (performedDisconnect && !UseAsyncConnect)
                    OnDisconnected();
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Exception occured during disconnect: {0}", ex.Message), ex));
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
        protected virtual void OnDisconnected()
        {
            OnStatusMessage("Disconnected.");
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        public virtual void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            QueueMeasurementsForProcessing(new IMeasurement[] { measurement });
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (m_sourceIDs == null)
            {
                // Apply no filter to measurements
                m_measurementQueue.AddRange(measurements);
                IncrementProcessedMeasurements(measurements.Count());
            }
            else
            {
                // Filter measurements to list of specified source IDs
                IEnumerable<IMeasurement> filteredMeasurements = measurements.Where(measurement => m_sourceIDs.BinarySearch(measurement.Source) > -1);
                m_measurementQueue.AddRange(filteredMeasurements);
                IncrementProcessedMeasurements(filteredMeasurements.Count());
            }
        }

        /// <summary>
        /// Serializes measurements to data output stream.
        /// </summary>
        /// <remarks>
        /// Derived classes must implement this function to process queued measurements.
        /// For example, this function would "archive" measurements if output adapter is for a historian.
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
            lock (m_measurementQueue.SyncRoot)
            {
                if (total > m_measurementQueue.Count)
                    total = m_measurementQueue.Count;

                m_measurementQueue.RemoveRange(0, total);
            }
        }

        /// <summary>
        /// Blocks the current thread, if the <see cref="OutputAdapterBase"/> is connected, until all items
        /// in <see cref="OutputAdapterBase"/> queue are processed, and then stops processing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is possible for items to be added to the queue while the flush is executing. The flush will continue to
        /// process items as quickly as possible until the queue is empty. Unless the user stops queueing items to be
        /// processed, the flush call may never return (not a happy situtation on shutdown).
        /// </para>
        /// <para>
        /// The <see cref="OutputAdapterBase"/> does not clear queue prior to destruction. If the user fails to call
        /// this method before the class is destructed, there may be items that remain unprocessed in the queue.
        /// </para>
        /// </remarks>
        public virtual void Flush()
        {
            if (m_measurementQueue != null)
                m_measurementQueue.Flush();
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
        /// Raises <see cref="AdapterBase.ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected override void OnProcessException(Exception ex)
        {
            base.OnProcessException(ex);

            // Exceptions thrown during measurement processing will cause a "reconnect" - this has to be
            // this way - for example, if historian threw an exception because it was no longer connected
            // this is the only way you could make sure historian connection cycle was restarted...
            Start();
        }

        private void m_connectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                OnStatusMessage("Attempting connection...");

                // Attempt connection to data output adapter (e.g., call historian API connect function).
                AttemptConnection();

                // Start data processing thread
                m_measurementQueue.Start();

                // Start data monitor...
                m_monitorTimer.Start();

                if (!UseAsyncConnect)
                    OnConnected();
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Connection attempt failed: {0}", ex.Message), ex));
                
                // So long as user hasn't requested to stop, keep trying connection
                if (Enabled)
                    Start();
            }
        }

        // All we do here is expose the total number of unarchived measurements in the queue
        private void m_monitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnUnprocessedMeasurements(m_measurementQueue.Count);
        }

        // Bubble any exceptions occuring in the process queue to the base class event
        private void m_measurementQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void IncrementProcessedMeasurements(long totalAdded)
        {
            // Check to see if total number of added points will exceed process interval used to show periodic
            // messages of how many points have been archived so far...
            bool showMessage = m_processedMeasurements + totalAdded >= (m_processedMeasurements / ProcessedMeasurementInterval + 1) * ProcessedMeasurementInterval;

            m_processedMeasurements += totalAdded;

            if (showMessage)
                OnStatusMessage(string.Format("{0:N0} measurements have been queued for processing so far...", m_processedMeasurements));
        }

        #endregion
    }
}