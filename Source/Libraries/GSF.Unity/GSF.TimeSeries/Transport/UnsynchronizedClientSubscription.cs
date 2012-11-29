//******************************************************************************************************
//  UnsynchronizedClientSubscription.cs - Gbtc
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Collections;
using GSF.Parsing;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents an unsynchronized client subscription to the <see cref="DataPublisher" />.
    /// </summary>
    internal class UnsynchronizedClientSubscription : FacileActionAdapterBase, IClientSubscription
    {
        #region [ Members ]

        // Constants
        private const int ProcessWaitTimeout = 1000;

        // Events

        /// <summary>
        /// Indicates to the host that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal procesing.
        /// </remarks>
        public event EventHandler<EventArgs<IClientSubscription, EventArgs>> ProcessingComplete;

        // Fields
        private readonly AsyncQueue<IEnumerable<IMeasurement>> m_processQueue;
        private readonly SignalIndexCache m_signalIndexCache;
        private readonly Guid m_clientID;
        private readonly Guid m_subscriberID;
        private DataPublisher m_parent;
        private string m_hostName;
        private bool m_useCompactMeasurementFormat;
        private long m_lastPublishTime;
        private string m_requestedInputFilter;
        private double m_publishInterval;
        private bool m_includeTime;
        private bool m_useMillisecondResolution;
        private volatile long[] m_baseTimeOffsets;
        private volatile int m_timeIndex;
        private System.Timers.Timer m_baseTimeRotationTimer;
        private volatile bool m_initializedBaseTimeOffsets;
        private volatile bool m_startTimeSent;
        private IaonSession m_iaonSession;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="parent">Reference to parent.</param>
        /// <param name="clientID"><see cref="Guid"/> based client connection ID.</param>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        public UnsynchronizedClientSubscription(DataPublisher parent, Guid clientID, Guid subscriberID)
        {
            // Pass parent reference into base class
            AssignParentCollection(parent);

            m_parent = parent;
            m_clientID = clientID;
            m_subscriberID = subscriberID;

            m_signalIndexCache = new SignalIndexCache()
            {
                SubscriberID = subscriberID
            };

            m_processQueue = new AsyncQueue<IEnumerable<IMeasurement>>()
            {
                ProcessItemFunction = ProcessMeasurements
            };

            m_processQueue.ProcessException += m_processQueue_ProcessException;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid ClientID
        {
            get
            {
                return m_clientID;
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid SubscriberID
        {
            get
            {
                return m_subscriberID;
            }
        }

        /// <summary>
        /// Gets the current signal index cache of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public SignalIndexCache SignalIndexCache
        {
            get
            {
                return m_signalIndexCache;
            }
        }

        /// <summary>
        /// Gets the input filter requested by the subscriber when establishing this <see cref="IClientSubscription"/>.
        /// </summary>
        public string RequestedInputFilter
        {
            get
            {
                return m_requestedInputFilter;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public bool UseCompactMeasurementFormat
        {
            get
            {
                return m_useCompactMeasurementFormat;
            }
            set
            {
                m_useCompactMeasurementFormat = value;
            }
        }

        /// <summary>
        /// Gets size of timestamp in bytes.
        /// </summary>
        public int TimestampSize
        {
            get
            {
                if (!m_useCompactMeasurementFormat)
                    return 8;
                else if (!m_includeTime)
                    return 0;
                else if (!m_parent.UseBaseTimeOffsets)
                    return 8;
                else if (!m_useMillisecondResolution)
                    return 4;
                else
                    return 2;
            }
        }

        /// <summary>
        /// Gets or sets host name used to identify connection source of client subscription.
        /// </summary>
        public string HostName
        {
            get
            {
                return m_hostName;
            }
            set
            {
                m_hostName = value;
            }
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, overwhich to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public override int ProcessingInterval
        {
            get
            {
                return base.ProcessingInterval;
            }
            set
            {
                base.ProcessingInterval = value;

                // Update processing interval in private temporal session, if defined
                if (m_iaonSession != null && m_iaonSession.AllAdapters != null)
                    m_iaonSession.AllAdapters.ProcessingInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the <see cref="UnsynchronizedClientSubscription"/> expects, if any.
        /// </summary>
        /// <remarks>
        /// We override method so assignment can be synchronized such that dynamic updates won't interfere
        /// with filtering in <see cref="QueueMeasurementsForProcessing"/>.
        /// </remarks>
        public override MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                lock (this)
                {
                    // Update signal index cache unless "detaching" from real-time
                    if (value != null && !(value.Length == 1 && value[0] == MeasurementKey.Undefined))
                    {
                        m_parent.UpdateSignalIndexCache(m_clientID, m_signalIndexCache, value);

                        if ((object)DataSource != null && (object)m_signalIndexCache != null)
                            value = AdapterBase.ParseInputMeasurementKeys(DataSource, string.Join("; ", m_signalIndexCache.AuthorizedSignalIDs.Select(signalID => signalID.ToString()).ToArray()));
                    }

                    base.InputMeasurementKeys = value;
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        /// <remarks>
        /// Although this adapter provisions support for temporal processing by proxying historical data to a remote sink, the adapter
        /// does not need to be automatically engaged within an actual temporal <see cref="IaonSession"/>, therefore this method returns
        /// <c>false</c> to make sure the adapter doesn't get automatically instantiated within a temporal session.
        /// </remarks>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a formatted message describing the status of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                ClientConnection connection;

                if (m_parent.ClientConnections.TryGetValue(m_clientID, out connection))
                {
                    status.Append(connection.Status);
                    status.AppendLine();
                }

                status.Append(base.Status);

                if (m_iaonSession != null)
                    status.Append(m_iaonSession.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UnsynchronizedClientSubscription"/> object and optionally releases the managed resources.
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
                        // Remove reference to parent
                        m_parent = null;

                        // Dispose base time rotation timer
                        if ((object)m_baseTimeRotationTimer != null)
                        {
                            m_baseTimeRotationTimer.Dispose();
                            m_baseTimeRotationTimer = null;
                        }

                        // Dispose Iaon session
                        this.DisposeTemporalSession(ref m_iaonSession);
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            string setting;

            if (Settings.TryGetValue("inputMeasurementKeys", out setting))
                m_requestedInputFilter = setting;
            else
                m_requestedInputFilter = null;

            if (Settings.TryGetValue("publishInterval", out setting))
                m_publishInterval = int.Parse(setting);
            else
                m_publishInterval = -1;

            if (Settings.TryGetValue("includeTime", out setting))
                m_includeTime = setting.ParseBoolean();
            else
                m_includeTime = true;

            if (Settings.TryGetValue("useMillisecondResolution", out setting))
                m_useMillisecondResolution = setting.ParseBoolean();
            else
                m_useMillisecondResolution = false;

            if (m_parent.UseBaseTimeOffsets && m_includeTime)
            {
                m_baseTimeRotationTimer = new System.Timers.Timer();
                m_baseTimeRotationTimer.Interval = m_useMillisecondResolution ? 60000 : 420000;
                m_baseTimeRotationTimer.AutoReset = true;
                m_baseTimeRotationTimer.Elapsed += BaseTimeRotationTimer_Elapsed;
            }

            // Handle temporal session intialization
            if (this.TemporalConstraintIsDefined())
                m_iaonSession = this.CreateTemporalSession();
        }

        /// <summary>
        /// Starts the <see cref="UnsynchronizedClientSubscription"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            if (!Enabled)
                m_startTimeSent = false;

            base.Start();

            if ((object)m_baseTimeRotationTimer != null && m_includeTime)
                m_baseTimeRotationTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>	
        public override void Stop()
        {
            base.Stop();

            if ((object)m_baseTimeRotationTimer != null)
            {
                m_baseTimeRotationTimer.Stop();
                m_baseTimeOffsets = null;
            }

            m_initializedBaseTimeOffsets = false;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            int inputCount = 0, outputCount = 0;

            if (InputMeasurementKeys != null)
                inputCount = InputMeasurementKeys.Length;

            if (OutputMeasurements != null)
                outputCount = OutputMeasurements.Length;

            return string.Format("Total input measurements: {0}, total output measurements: {1}", inputCount, outputCount).PadLeft(maxLength);
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="InputMeasurementKeys"/> so we override method
        /// so that dyanmic updates to keys will be synchronized with filtering to prevent interference.
        /// </remarks>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (!m_startTimeSent && (object)measurements != null && measurements.Any())
            {
                m_startTimeSent = true;

                IMeasurement measurement = measurements.FirstOrDefault(m => m != null);
                Ticks timestamp = 0;

                if (measurement != null)
                    timestamp = measurement.Timestamp;

                m_parent.SendDataStartTime(m_clientID, timestamp);
            }

            if (ProcessMeasurementFilter)
            {
                List<IMeasurement> filteredMeasurements = new List<IMeasurement>();

                lock (this)
                {
                    foreach (IMeasurement measurement in measurements)
                    {
                        if (IsInputMeasurement(measurement.Key))
                            filteredMeasurements.Add(measurement);
                    }
                }

                measurements = filteredMeasurements;
            }

            if (measurements.Any() && Enabled)
            {
                if (TrackLatestMeasurements)
                {
                    double publishInterval;

                    // Keep track of latest measurements
                    base.QueueMeasurementsForProcessing(measurements);
                    publishInterval = (m_publishInterval > 0) ? m_publishInterval : LagTime;

                    if (DateTime.UtcNow.Ticks > m_lastPublishTime + Ticks.FromSeconds(publishInterval))
                    {
                        List<IMeasurement> currentMeasurements = new List<IMeasurement>();
                        Measurement newMeasurement;

                        // Create a new set of measurements that represent the latest known values setting value to NaN if it is old
                        foreach (TemporalMeasurement measurement in LatestMeasurements)
                        {
                            newMeasurement = new Measurement()
                            {
                                ID = measurement.ID,
                                Key = measurement.Key,
                                Value = measurement.GetValue(RealTime),
                                Adder = measurement.Adder,
                                Multiplier = measurement.Multiplier,
                                Timestamp = measurement.Timestamp,
                                StateFlags = measurement.StateFlags
                            };

                            currentMeasurements.Add(newMeasurement);
                        }

                        // Publish latest data values...
                        if ((object)m_processQueue != null)
                            m_processQueue.Enqueue(currentMeasurements);
                    }
                }
                else
                {
                    // Publish unsynchronized on data receipt otherwise...
                    m_processQueue.Enqueue(measurements);
                }
            }
        }

        private void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            List<ISupportBinaryImage> packet = new List<ISupportBinaryImage>();
            bool useCompactMeasurementFormat = m_useCompactMeasurementFormat;
            int packetSize = 5;

            // Wait for any external events, if needed
            WaitForExternalEvents();

            // If a set of base times has not yet been initialized, initialize a set by rotating
            if (!m_initializedBaseTimeOffsets)
            {
                if (m_parent.UseBaseTimeOffsets)
                    RotateBaseTimes();

                m_initializedBaseTimeOffsets = true;
            }

            foreach (IMeasurement measurement in measurements)
            {
                ISupportBinaryImage binaryMeasurement;
                int binaryLength;

                // Serialize the current measurement.
                if (useCompactMeasurementFormat)
                    binaryMeasurement = new CompactMeasurement(measurement, m_signalIndexCache, m_includeTime, m_baseTimeOffsets, m_timeIndex, m_useMillisecondResolution);
                else
                    binaryMeasurement = new SerializableMeasurement(measurement, m_parent.GetClientEncoding(m_clientID));

                // Determine the size of the measurement in bytes.
                binaryLength = binaryMeasurement.BinaryLength;

                // If the current measurement will not fit in the packet based on the max
                // packet size, process the current packet and start a new packet.
                if (packetSize + binaryLength > DataPublisher.MaxPacketSize)
                {
                    ProcessBinaryMeasurements(packet, useCompactMeasurementFormat);
                    packet.Clear();
                    packetSize = 5;
                }

                // Add the current measurement to the packet.
                packet.Add(binaryMeasurement);
                packetSize += binaryLength;
            }

            // Process the remaining measurements.
            ProcessBinaryMeasurements(packet, useCompactMeasurementFormat);
        }

        private void ProcessBinaryMeasurements(IEnumerable<ISupportBinaryImage> measurements, bool useCompactMeasurementFormat)
        {
            MemoryStream data = new MemoryStream();

            // Serialize data packet flags into response
            DataPacketFlags flags = DataPacketFlags.NoFlags; // No flags means bit is cleared, i.e., unsynchronized

            if (useCompactMeasurementFormat)
                flags |= DataPacketFlags.Compact;

            data.WriteByte((byte)flags);

            // No frame level timestamp is serialized into the data packet since all data is unsynchronized and essentially
            // published upon receipt, however timestamps are optionally included in the serialized measurements.

            // Serialize total number of measurement values to follow
            data.Write(EndianOrder.BigEndian.GetBytes(measurements.Count()), 0, 4);

            // Serialize measurements to data buffer
            foreach (ISupportBinaryImage measurement in measurements)
            {
                measurement.CopyBinaryImageToStream(data);
            }

            // Publish data packet to client
            if (m_parent != null)
                m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, data.ToArray());

            // Track last publication time
            m_lastPublishTime = DateTime.UtcNow.Ticks;
        }

        // Rotates base time offsets
        private void RotateBaseTimes()
        {
            if ((object)m_parent != null && (object)m_baseTimeRotationTimer != null)
            {
                MemoryStream responsePacket = new MemoryStream();

                if ((object)m_baseTimeOffsets == null)
                {
                    m_baseTimeOffsets = new long[2];
                    m_baseTimeOffsets[0] = RealTime;
                    m_baseTimeOffsets[1] = RealTime + (long)m_baseTimeRotationTimer.Interval * Ticks.PerMillisecond;
                    m_timeIndex = 0;
                }
                else
                {
                    int oldIndex = m_timeIndex;

                    // Switch to newer timestamp
                    m_timeIndex ^= 1;

                    // Now make older timestamp the newer timestamp
                    m_baseTimeOffsets[oldIndex] = RealTime + (long)m_baseTimeRotationTimer.Interval * Ticks.PerMillisecond;
                }

                responsePacket.Write(EndianOrder.BigEndian.GetBytes(m_timeIndex), 0, 4);
                responsePacket.Write(EndianOrder.BigEndian.GetBytes(m_baseTimeOffsets[0]), 0, 8);
                responsePacket.Write(EndianOrder.BigEndian.GetBytes(m_baseTimeOffsets[1]), 0, 8);

                m_parent.SendClientResponse(m_clientID, ServerResponse.UpdateBaseTimes, ServerCommand.Subscribe, responsePacket.ToArray());
            }
        }

        // Explicitly implement status message event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnStatusMessage(string status)
        {
            OnStatusMessage(status);
        }

        // Explicitly implement process exception event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnProcessException(Exception ex)
        {
            OnProcessException(ex);
        }

        // Explicitly implement processing completed event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnProcessingCompleted(object sender, EventArgs e)
        {
            if (ProcessingComplete != null)
                ProcessingComplete(sender, new EventArgs<IClientSubscription, EventArgs>(this, e));
        }

        private void BaseTimeRotationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RotateBaseTimes();
        }

        private void m_processQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion
    }
}
