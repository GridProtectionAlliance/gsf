//*******************************************************************************************************
//  InputAdapterCollection.cs
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
using TVA.Units;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="IInputAdapter"/> implementations.
    /// </summary>
    [CLSCompliant(false)]
    public class InputAdapterCollection : AdapterCollectionBase<IInputAdapter>, IInputAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// This event will be raised when there are new measurements available from the input data source.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private Ticks m_lastProcessTime;
        private Time m_totalProcessTime;
        private long m_receivedMeasurements;
        private System.Timers.Timer m_monitorTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InputAdapterCollection"/>.
        /// </summary>
        public InputAdapterCollection()
        {
            base.Name = "Input Adapter Collection";
            base.DataMember = "InputAdapters";

            m_monitorTimer = new System.Timers.Timer();
            m_monitorTimer.Elapsed += m_monitorTimer_Elapsed;

            // We monitor total number of measurements received every minute
            m_monitorTimer.Interval = 60000;
            m_monitorTimer.AutoReset = true;
            m_monitorTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of measurements received thus far by each <see cref="IInputAdapter"/> implementation
        /// in the <see cref="InputAdapterCollection"/>.
        /// </summary>
        public virtual long ReceivedMeasurements
        {
            get
            {
                return m_receivedMeasurements;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InputAdapterCollection"/> object and optionally releases the managed resources.
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
        /// Starts each <see cref="IInputAdapter"/> implementation in this <see cref="InputAdapterCollection"/>.
        /// </summary>
        public override void Start()
        {
            // Reset statistics
            m_receivedMeasurements = 0;
            m_totalProcessTime = 0.0D;
            m_lastProcessTime = DateTime.UtcNow.Ticks;

            // Start data monitor...
            m_monitorTimer.Start();

            base.Start();
        }

        /// <summary>
        /// Stops each <see cref="IInputAdapter"/> implementation in this <see cref="InputAdapterCollection"/>.
        /// </summary>
        public override void Stop()
        {
            // Stop data monitor...
            m_monitorTimer.Stop();

            base.Stop();
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        /// <param name="measurements">New measurements.</param>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IInputAdapter"/> implementation.</param>
        protected override void InitializeItem(IInputAdapter item)
        {
            if (item != null)
            {
                // Wire up new measurement event
                item.NewMeasurements += NewMeasurements;                
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IInputAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IInputAdapter"/> to dispose.</param>
        protected override void DisposeItem(IInputAdapter item)
        {
            if (item != null)
            {
                // Un-wire new meaurements event
                item.NewMeasurements -= NewMeasurements;
                base.DisposeItem(item);
            }
        }

        // We monitor the total number of measurements received from the input adapters here...
        private void m_monitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StringBuilder status = new StringBuilder();
            Ticks currentTime, totalProcessTime;
            long totalNew, receivedMeasurements = 0;

            // Calculate time since last call
            currentTime = DateTime.UtcNow.Ticks;
            totalProcessTime = currentTime - m_lastProcessTime;
            m_totalProcessTime += totalProcessTime.ToSeconds();
            m_lastProcessTime = currentTime;

            // Calculate new total for all incoming input adapters
            foreach (IInputAdapter item in this)
            {
                receivedMeasurements += item.ReceivedMeasurements;
            }

            // Calculate how many new measurements have been received in the last minute...
            totalNew = receivedMeasurements - m_receivedMeasurements;
            m_receivedMeasurements = receivedMeasurements;

            // Receive Statistics:
            //
            //          1              1                 1
            // 12345678901234 12345678901234567 1234567890
            // Time span        Measurements    Per second
            // -------------- ----------------- ----------
            // Entire runtime 9,999,999,999,999 99,999,999
            // Last minute         4,985            83

            status.AppendFormat("\r\nReceive Statistics for {0} total runtime:\r\n\r\n", m_totalProcessTime.ToString().ToLower());
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
            status.Append(m_receivedMeasurements.ToString("N0").CenterText(17));
            status.Append(' ');
            status.Append(((int)(m_receivedMeasurements / m_totalProcessTime)).ToString("N0").CenterText(10));
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