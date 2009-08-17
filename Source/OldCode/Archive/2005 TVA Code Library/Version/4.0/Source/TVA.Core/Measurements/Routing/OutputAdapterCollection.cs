//*******************************************************************************************************
//  OutputAdapterCollection.cs
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
//  05/07/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using TVA;
using TVA.Measurements;
using TVA.Units;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="IOutputAdapter"/> implementations.
    /// </summary>
    [CLSCompliant(false)]
    public class OutputAdapterCollection : AdapterCollectionBase<IOutputAdapter>, IOutputAdapter
    {
        #region [ Members ]

        // Events

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

        // Fields
        private Ticks m_lastProcessTime;
        private Time m_totalProcessTime;
        private long m_processedMeasurements;
        private System.Timers.Timer m_monitorTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InputAdapterCollection"/>.
        /// </summary>
        public OutputAdapterCollection()
        {
            base.Name = "Output Adapter Collection";
            base.DataMember = "OutputAdapters";

            m_monitorTimer = new System.Timers.Timer();
            m_monitorTimer.Elapsed += m_monitorTimer_Elapsed;

            // We monitor total number of measurements destined for archival every minute
            m_monitorTimer.Interval = 60000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of measurements processed thus far by each <see cref="IOutputAdapter"/> implementation
        /// in the <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public virtual long ProcessedMeasurements
        {
            get
            {
                return m_processedMeasurements;
            }
        }

        /// <summary>
        /// Returns a flag that determines if all measurements sent to this <see cref="OutputAdapterCollection"/> are
        /// destined for archival.
        /// </summary>
        public virtual bool OutputIsForArchive
        {
            get
            {
                foreach (IOutputAdapter item in this)
                {
                    if (!item.OutputIsForArchive)
                        return false;
                }

                return true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="OutputAdapterCollection"/> object and optionally releases the managed resources.
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
                        if (m_monitorTimer != null)
                        {
                            m_monitorTimer.Elapsed -= m_monitorTimer_Elapsed;
                            m_monitorTimer.Dispose();
                        }
                        m_monitorTimer = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts each <see cref="IOutputAdapter"/> implementation in this <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public override void Start()
        {
            // Reset statistics
            m_processedMeasurements = 0;
            m_totalProcessTime = 0.0D;
            m_lastProcessTime = DateTime.UtcNow.Ticks;
            
            // Start data monitor...
            m_monitorTimer.Start();

            base.Start();
        }

        /// <summary>
        /// Stops each <see cref="IOutputAdapter"/> implementation in this <see cref="OutputAdapterCollection"/>.
        /// </summary>
        public override void Stop()
        {
            // Stop data monitor...
            m_monitorTimer.Stop();

            base.Stop();
        }

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IOutputAdapter"/> implementation in
        /// this <see cref="OutputAdapterCollection"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public virtual void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            foreach (IOutputAdapter item in this)
            {
                item.QueueMeasurementsForProcessing(measurements);
            }
        }

        /// <summary>
        /// This function removes a range of measurements from the internal measurement queues. Note that the requested
        /// <paramref name="total"/> will be removed from each <see cref="IOutputAdapter"/> implementation in this
        /// <see cref="OutputAdapterCollection"/>.
        /// </summary>
        /// <param name="total">Total measurements to remove from the each <see cref="IOutputAdapter"/> queue.</param>
        /// <remarks>
        /// This method is typically only used to curtail size of measurement queue if it's getting too large.  If more points are
        /// requested than there are points available - all points in the queue will be removed.
        /// </remarks>
        public virtual void RemoveMeasurements(int total)
        {
            foreach (IOutputAdapter item in this)
            {
                item.RemoveMeasurements(total);
            }
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
        /// Wires events and initializes new <see cref="IOutputAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IOutputAdapter"/> implementation.</param>
        protected override void InitializeItem(IOutputAdapter item)
        {
            if (item != null)
            {
                // Wire up unprocessed measurements event
                item.UnprocessedMeasurements += UnprocessedMeasurements;                
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IOutputAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IOutputAdapter"/> to dispose.</param>
        protected override void DisposeItem(IOutputAdapter item)
        {
            if (item != null)
            {
                // Un-wire unprocessed measurements event
                item.UnprocessedMeasurements -= UnprocessedMeasurements;
                base.DisposeItem(item);
            }
        }

        // We monitor the total number of measurements destined for archival here...
        private void m_monitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StringBuilder status = new StringBuilder();
            Ticks currentTime, totalProcessTime;
            long totalNew, processedMeasurements = 0;

            // Calculate time since last call
            currentTime = DateTime.UtcNow.Ticks;
            totalProcessTime = currentTime - m_lastProcessTime;
            m_totalProcessTime += totalProcessTime.ToSeconds();
            m_lastProcessTime = currentTime;

            // Calculate new total for all archive destined output adapters
            foreach (IOutputAdapter item in this)
            {
                if (item.OutputIsForArchive)
                    processedMeasurements += item.ProcessedMeasurements;
            }

            // Calculate how many new measurements have been received in the last minute...
            totalNew = processedMeasurements - m_processedMeasurements;
            m_processedMeasurements = processedMeasurements;

            // Archive Statistics:
            //
            //          1              1                 1
            // 12345678901234 12345678901234567 1234567890
            // Time span        Measurements    Per second
            // -------------- ----------------- ----------
            // Entire runtime 9,999,999,999,999 99,999,999
            // Last minute         4,985            83

            status.AppendFormat("\r\nArchive Statistics for {0} total runtime:\r\n\r\n", m_totalProcessTime.ToString().ToLower());
            status.Append("Time span".PadRight(14));
            status.Append(' ');
            status.Append("Measurements".CenterText(17));
            status.Append(' ');
            status.Append("Per second".CenterText(10));
            status.AppendLine();
            status.Append(new string('-', 14));
            status.Append(' ');
            status.Append(new string('-', 17));
            status.Append(' ');
            status.Append(new string('-', 10));
            status.AppendLine();

            status.Append("Entire runtime".PadRight(14));
            status.Append(' ');
            status.Append(m_processedMeasurements.ToString("N0").CenterText(17));
            status.Append(' ');
            status.Append(((int)(m_processedMeasurements / m_totalProcessTime)).ToString("N0").CenterText(10));
            status.AppendLine();
            status.Append("Last minute".PadRight(14));
            status.Append(' ');
            status.Append(totalNew.ToString("N0").CenterText(17));
            status.Append(' ');
            status.Append(((int)(totalNew / totalProcessTime.ToSeconds())).ToString("N0").CenterText(10));

            // Report updated statistics every minute...
            OnStatusMessage(status.ToString());
        }

        #endregion
    }
}