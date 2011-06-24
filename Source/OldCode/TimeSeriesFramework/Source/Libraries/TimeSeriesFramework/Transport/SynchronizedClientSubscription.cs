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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Parsing;
using System.Text;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a synchronized client subscription to the <see cref="DataPublisher" />.
    /// </summary>
    internal class SynchronizedClientSubscription : ActionAdapterBase, IClientSubscription
    {
        #region [ Members ]

        // Fields
        private SignalIndexCache m_signalIndexCache;
        private DataPublisher m_parent;
        private Guid m_clientID;
        private Guid m_subscriberID;
        private bool m_useCompactMeasurementFormat;
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
            m_signalIndexCache = new SignalIndexCache(subscriberID);
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
                    base.InputMeasurementKeys = value;
                    m_parent.UpdateSignalIndexCache(m_clientID, m_signalIndexCache, value);
                }
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
                        binaryMeasurement = new CompactMeasurement(measurement, m_signalIndexCache, false);
                    else
                        binaryMeasurement = new SerializableMeasurement(measurement);

                    // Determine the size of the measurement in bytes.
                    binaryLength = binaryMeasurement.BinaryLength;

                    // If the current measurement will not fit in the packet based on
                    // the max packet size, process the packet and start a new one.
                    if (packetSize + binaryLength > DataPublisher.MaxPacketSize)
                    {
                        ProcessBinaryMeasurements(packet, frameLevelTimestamp);
                        packet.Clear();
                        packetSize = 13;
                    }

                    // Add the measurement to the packet.
                    packet.Add(binaryMeasurement);
                    packetSize += binaryLength;
                }

                // Process the remaining measurements.
                ProcessBinaryMeasurements(packet, frameLevelTimestamp);
            }
        }

        private void ProcessBinaryMeasurements(IEnumerable<ISupportBinaryImage> measurements, long frameLevelTimestamp)
        {
            MemoryStream data = new MemoryStream();
            bool useCompactMeasurementFormat = m_useCompactMeasurementFormat;
            byte[] buffer;

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
                buffer = measurement.BinaryImage;
                data.Write(buffer, 0, buffer.Length);
            }

            // Publish data packet to client
            if (m_parent != null)
                m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, data.ToArray());
        }

        /// <summary>
        /// Queues a single measurement for processing.
        /// </summary>
        /// <param name="measurement">Measurement to queue for processing.</param>
        /// <remarks>
        /// Measurement is filtered against the defined <see cref="InputMeasurementKeys"/> so we override method
        /// so that dynamic updates to keys will be synchronized with filtering to prevent interference.
        /// </remarks>
        public override void QueueMeasurementForProcessing(IMeasurement measurement)
        {
            lock (this)
            {
                if (!m_disposed)
                    base.QueueMeasurementForProcessing(measurement);
            }
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
            lock (this)
            {
                if (!m_disposed)
                    base.QueueMeasurementsForProcessing(measurements);
            }
        }

        #endregion
    }
}
