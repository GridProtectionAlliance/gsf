//*******************************************************************************************************
//  InputAdapterBase.cs
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
//  12/01/2006 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TVA;
using TVA.Measurements;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the base class for any incoming input adapter.
    /// </summary>
    /// <remarks>
    /// Derived classes are expected to call <see cref="OnNewMeasurements"/> when new measurements are received.
    /// </remarks>
    [CLSCompliant(false)]
    public abstract class InputAdapterBase : AdapterBase, IInputAdapter
	{
        #region [ Members ]

        // Constants
        private const int ReceivedMeasurementInterval = 100000;

        // Events

        /// <summary>
        /// Provides new measurements from input adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private long m_receivedMeasurements;
        private System.Timers.Timer m_connectionTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="InputAdapterBase"/>.
        /// </summary>
        protected InputAdapterBase()
        {
            m_connectionTimer = new System.Timers.Timer();
            m_connectionTimer.Elapsed += m_connectionTimer_Elapsed;

            m_connectionTimer.AutoReset = false;
            m_connectionTimer.Interval = 2000;
            m_connectionTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of measurements received thus far by the <see cref="InputAdapterBase"/>.
        /// </summary>
        public virtual long ReceivedMeasurements
        {
            get
            {
                return m_receivedMeasurements;
            }
        }

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        /// <remarks>
        /// Derived classes should return true when data input source is connects asynchronously, otherwise return false.
        /// </remarks>
        protected abstract bool UseAsyncConnect { get; }

        /// <summary>
        /// Gets or sets the connection attempt interval, in milliseconds, for the data input source.
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
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("     Received measurements: {0}", m_receivedMeasurements);
                status.AppendLine();
                status.AppendFormat("   Asynchronous connection: {0}", UseAsyncConnect);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InputAdapterBase"/> object and optionally releases the managed resources.
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
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts this <see cref="InputAdapterBase"/> and initiates connection cycle to data input source.
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
        /// Attempts to connect to data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt connection to data input source here.  Any exceptions thrown
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
        /// Stops this <see cref="InputAdapterBase"/> and disconnects from data input source.
        /// </summary>
        public override void Stop()
        {
            try
            {
                bool performedDisconnect = Enabled;

                base.Stop();

                // Attempt disconnection from data source
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
        /// Attempts to disconnect from data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt disconnect from data input source here.  Any exceptions thrown
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
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            if (NewMeasurements != null)
                NewMeasurements(this, new EventArgs<ICollection<IMeasurement>>(measurements));

            IncrementReceivedMeasurements(measurements.Count);
        }

        /// <summary>
        /// Raises <see cref="AdapterBase.ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected override void OnProcessException(Exception ex)
        {
            base.OnProcessException(ex);

            // Exceptions thrown during measurement processing will cause a "reconnect" - this has to be
            // this way - for example, if data source threw an exception because it was no longer connected
            // this is the only way you could make sure connection cycle was restarted...
            Start();
        }

        private void m_connectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                OnStatusMessage("Attempting connection...");

                // Attempt connection to data source
                AttemptConnection();

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

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void IncrementReceivedMeasurements(long totalAdded)
        {
            // Check to see if total number of added points will exceed process interval used to show periodic
            // messages of how many points have been archived so far...
            bool showMessage = m_receivedMeasurements + totalAdded >= (m_receivedMeasurements / ReceivedMeasurementInterval + 1) * ReceivedMeasurementInterval;

            m_receivedMeasurements += totalAdded;

            if (showMessage)
                OnStatusMessage("{0:N0} measurements have been received so far...", m_receivedMeasurements);
        }

        #endregion
	}	
}