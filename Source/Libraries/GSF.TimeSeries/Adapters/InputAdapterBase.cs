//******************************************************************************************************
//  InputAdapterBase.cs - Gbtc
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
using System.Text;
using GSF.Diagnostics;
using GSF.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the base class for any incoming input adapter.
    /// </summary>
    /// <remarks>
    /// Derived classes are expected to call <see cref="OnNewMeasurements"/> when new measurements are received.
    /// </remarks>
    public abstract class InputAdapterBase : AdapterBase, IInputAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides new measurements from input adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a collection of new measurements for host to process.
        /// </remarks>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        /// <summary>
        /// Indicates to the host that processing for the input adapter has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public event EventHandler ProcessingComplete;

        // Fields
        private List<string> m_outputSourceIDs;
        private readonly LongSynchronizedOperation m_connectionOperation;
        private SharedTimer m_connectionTimer;
        private string m_connectionInfo;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="InputAdapterBase"/>.
        /// </summary>
        protected InputAdapterBase()
        {
            m_connectionOperation = new LongSynchronizedOperation(AttemptConnectionOperation)
            {
                IsBackground = true
            };

            m_connectionTimer = Common.TimerScheduler.CreateTimer(2000);
            m_connectionTimer.Elapsed += m_connectionTimer_Elapsed;

            m_connectionTimer.AutoReset = false;
            m_connectionTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output measurements.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of measurements based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        public virtual string[] OutputSourceIDs
        {
            get => m_outputSourceIDs?.ToArray();
            set
            {
                if (value is null)
                {
                    m_outputSourceIDs = null;
                }
                else
                {
                    m_outputSourceIDs = new List<string>(value);
                    m_outputSourceIDs.Sort();
                }

                // Filter measurements to list of specified source IDs
                LoadOutputSourceIDs(this);
            }
        }

        /// <summary>
        /// Gets or sets output measurement keys that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        public virtual MeasurementKey[] RequestedOutputMeasurementKeys { get; set; }

        /// <summary>
        /// Gets flag that determines if <see cref="InputAdapterBase"/> is connected.
        /// </summary>
        public virtual bool IsConnected { get; protected set; }

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
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                const int MaxMeasurementsToShow = 10;

                StringBuilder status = new();

                status.Append(base.Status);

                if (RequestedOutputMeasurementKeys is not null && RequestedOutputMeasurementKeys.Length > 0)
                {
                    status.AppendLine($"     Requested output keys: {RequestedOutputMeasurementKeys.Length:N0} defined measurements");
                    status.AppendLine();

                    for (int i = 0; i < Math.Min(RequestedOutputMeasurementKeys.Length, MaxMeasurementsToShow); i++)
                        status.AppendLine(RequestedOutputMeasurementKeys[i].ToString().TruncateRight(25).CenterText(50));

                    if (RequestedOutputMeasurementKeys.Length > MaxMeasurementsToShow)
                        status.AppendLine("...".CenterText(50));

                    status.AppendLine();
                }

                status.AppendLine($"    Connection established: {IsConnected}");
                status.AppendLine($"   Asynchronous connection: {UseAsyncConnect}");

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="AdapterBase"/> expects, if any.
        /// </summary>
        /// <remarks>
        /// Redefined to hide attributes defined in the base class.
        /// </remarks>
        public new virtual MeasurementKey[] InputMeasurementKeys
        {
            get => base.InputMeasurementKeys;
            set => base.InputMeasurementKeys = value;
        }

        /// <summary>
        /// When false, connection errors do not get logged through OnProcessException. When true, connection errors will be logged normally.
        /// </summary>
        protected bool EnableConnectionErrors { get; set; } = true;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InputAdapterBase"/> object and optionally releases the managed resources.
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
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="InputAdapterBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            // Load optional parameters
            if (settings.TryGetValue("outputSourceIDs", out string setting) || settings.TryGetValue("sourceids", out setting))
                OutputSourceIDs = setting.Split(',');
            else
                OutputSourceIDs = null;

            if (settings.TryGetValue("enableConnectionErrors", out setting))
                EnableConnectionErrors = setting.ParseBoolean();
        }

        /// <summary>
        /// Starts this <see cref="InputAdapterBase"/> and initiates connection cycle to data input source.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Start the connection cycle
            if (m_connectionTimer is not null)
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
            IsConnected = true;
            OnStatusMessage(MessageLevel.Info, "Connection established.", "Connecting");
        }

        /// <summary>
        /// Stops this <see cref="InputAdapterBase"/> and disconnects from data input source.
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

                // Attempt disconnection from data source
                AttemptDisconnection();

                if (performedDisconnect && !UseAsyncConnect)
                    OnDisconnected();
            }
            catch (Exception ex)
            {
                if (EnableConnectionErrors)
                    OnProcessException(MessageLevel.Info, new ConnectionException($"Exception occurred during disconnect: {ex.Message}", ex));
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
            IsConnected = false;
            OnStatusMessage(MessageLevel.Info, $"Disconnected from {m_connectionInfo ?? ConnectionInfo}.");
        }

        /// <summary>
        /// Raises the <see cref="NewMeasurements"/> event.
        /// </summary>
        protected virtual void OnNewMeasurements(ICollection<IMeasurement> measurements)
        {
            try
            {
                NewMeasurements?.Invoke(this, new EventArgs<ICollection<IMeasurement>>(measurements));

                IncrementProcessedMeasurements(measurements.Count);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(NewMeasurements)} event: {ex.Message}", ex), "ConsumerEventException");
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        protected virtual void OnProcessingComplete()
        {
            try
            {
                ProcessingComplete?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception in consumer handler for {nameof(ProcessingComplete)} event: {ex.Message}", ex), "ConsumerEventException");
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

                OnStatusMessage(MessageLevel.Info, $"Attempting connection{(m_connectionInfo is null ? "" : $" to {m_connectionInfo}")}...") ;

                // Attempt connection to data source
                AttemptConnection();

                // Try to update connection info after successful connection attempt in case info is now available
                m_connectionInfo ??= ConnectionInfo;

                if (!UseAsyncConnect)
                    OnConnected();
            }
            catch (Exception ex)
            {
                if (EnableConnectionErrors)
                    OnProcessException(MessageLevel.Info, new ConnectionException($"Connection attempt failed{(m_connectionInfo is null ? "" : $" for {m_connectionInfo}")}: {ex.Message}", ex));

                // So long as user hasn't requested to stop, keep trying connection
                if (Enabled)
                    Start();
            }
        }

        private void m_connectionTimer_Elapsed(object sender, EventArgs<DateTime> e) => 
            m_connectionOperation.TryRunOnceAsync();

        #endregion
    }
}