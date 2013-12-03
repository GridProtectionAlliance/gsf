//******************************************************************************************************
//  SynchronizedClientSubscription.cs - Gbtc
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

using GSF.Parsing;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a synchronized client subscription to the <see cref="DataPublisher" />.
    /// </summary>
    internal class SynchronizedClientSubscription : ActionAdapterBase, IClientSubscription
    {
        #region [ Members ]

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
        private SignalIndexCache m_signalIndexCache;
        private DataPublisher m_parent;
        private Guid m_clientID;
        private Guid m_subscriberID;
        private string m_hostName;
        private string m_requestedInputFilter;
        private volatile bool m_useCompactMeasurementFormat;
        private volatile bool m_startTimeSent;
        private IaonSession m_iaonSession;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="parent">Reference to parent.</param>
        /// <param name="clientID"><see cref="Guid"/> based client connection ID.</param>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        public SynchronizedClientSubscription(DataPublisher parent, Guid clientID, Guid subscriberID)
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
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="SynchronizedClientSubscription"/>.
        /// </summary>
        public Guid ClientID
        {
            get
            {
                return m_clientID;
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="SynchronizedClientSubscription"/>.
        /// </summary>
        public Guid SubscriberID
        {
            get
            {
                return m_subscriberID;
            }
        }

        /// <summary>
        /// Gets the current signal index cache of this <see cref="SynchronizedClientSubscription"/>.
        /// </summary>
        public SignalIndexCache SignalIndexCache
        {
            get
            {
                return m_signalIndexCache;
            }
        }

        public string RequestedInputFilter
        {
            get
            {
                return m_requestedInputFilter;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="SynchronizedClientSubscription"/>.
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
                return 8;
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
        /// Gets or sets primary keys of input measurements the <see cref="SynchronizedClientSubscription"/> expects, if any.
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
        /// Gets a formatted message describing the status of this <see cref="SynchronizedClientSubscription"/>.
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
        /// Releases the unmanaged resources used by the <see cref="SynchronizedClientSubscription"/> object and optionally releases the managed resources.
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
        /// Initializes <see cref="SynchronizedClientSubscription"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            base.UsePrecisionTimer = false;

            if (!Settings.TryGetValue("inputMeasurementKeys", out m_requestedInputFilter))
                m_requestedInputFilter = null;

            // Handle temporal session intialization
            if (this.TemporalConstraintIsDefined())
                m_iaonSession = this.CreateTemporalSession();
        }

        /// <summary>
        /// Starts the <see cref="SynchronizedClientSubscription"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            if (!Enabled)
                m_startTimeSent = false;

            base.Start();
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// Measurements are filtered against the defined <see cref="InputMeasurementKeys"/> so we override method
        /// so that dynamic updates to keys will be synchronized with filtering to prevent interference.
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
                lock (this)
                {
                    base.QueueMeasurementsForProcessing(measurements);
                }
            }
            else
            {
                base.QueueMeasurementsForProcessing(measurements);
            }
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            if (!m_disposed)
            {
                List<ISupportBinaryImage> packet = new List<ISupportBinaryImage>();
                bool useCompactMeasurementFormat = m_useCompactMeasurementFormat;
                long frameLevelTimestamp = frame.Timestamp;
                int packetSize = 13;

                foreach (IMeasurement measurement in frame.Measurements.Values)
                {
                    ISupportBinaryImage binaryMeasurement;
                    int binaryLength;

                    // Serialize the current measurement.
                    if (useCompactMeasurementFormat)
                        binaryMeasurement = new CompactMeasurement(measurement, m_signalIndexCache, false, null, 0, false);
                    else
                        binaryMeasurement = new SerializableMeasurement(measurement, m_parent.GetClientEncoding(ClientID));

                    // Determine the size of the measurement in bytes.
                    binaryLength = binaryMeasurement.BinaryLength;

                    // If the current measurement will not fit in the packet based on
                    // the max packet size, process the packet and start a new one.
                    if (packetSize + binaryLength > DataPublisher.MaxPacketSize)
                    {
                        ProcessBinaryMeasurements(packet, frameLevelTimestamp, useCompactMeasurementFormat);
                        packet.Clear();
                        packetSize = 13;
                    }

                    // Add the measurement to the packet.
                    packet.Add(binaryMeasurement);
                    packetSize += binaryLength;
                }

                // Process the remaining measurements.
                ProcessBinaryMeasurements(packet, frameLevelTimestamp, useCompactMeasurementFormat);
            }
        }

        private void ProcessBinaryMeasurements(IEnumerable<ISupportBinaryImage> measurements, long frameLevelTimestamp, bool useCompactMeasurementFormat)
        {
            MemoryStream data = new MemoryStream();

            // Serialize data packet flags into response
            DataPacketFlags flags = DataPacketFlags.Synchronized;

            if (useCompactMeasurementFormat)
                flags |= DataPacketFlags.Compact;

            data.WriteByte((byte)flags);

            // Serialize frame timestamp into data packet - this only occurs in synchronized data packets,
            // unsynchronized subcriptions always include timestamps in the serialized measurements
            data.Write(EndianOrder.BigEndian.GetBytes(frameLevelTimestamp), 0, 8);

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

        #endregion
    }
}
