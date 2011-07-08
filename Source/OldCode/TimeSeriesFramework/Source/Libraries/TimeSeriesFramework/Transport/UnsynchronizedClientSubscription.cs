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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Parsing;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents an unsynchronized client subscription to the <see cref="DataPublisher" />.
    /// </summary>
    internal class UnsynchronizedClientSubscription : FacileActionAdapterBase, IClientSubscription
    {
        #region [ Members ]

        // Fields
        private SignalIndexCache m_signalIndexCache;
        private DataPublisher m_parent;
        private Guid m_clientID;
        private Guid m_subscriberID;
        private bool m_useCompactMeasurementFormat;
        private long m_lastPublishTime;
        private bool m_includeTime;
        private volatile bool m_startTimeSent;
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
            m_signalIndexCache = new SignalIndexCache(subscriberID);
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
                    base.InputMeasurementKeys = value;
                    m_parent.UpdateSignalIndexCache(m_clientID, m_signalIndexCache, value);
                }
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

            if (Settings.TryGetValue("includeTime", out setting))
                m_includeTime = setting.ParseBoolean();
            else
                m_includeTime = true;
        }

        /// <summary>
        /// Starts the <see cref="UnsynchronizedClientSubscription"/> or restarts it if it is already running.
        /// </summary>
        public override void Start()
        {
            if (!Enabled)
                m_startTimeSent = false;

            base.Start();
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
            if (!m_startTimeSent && measurements != null && measurements.Count() > 0)
            {
                m_startTimeSent = true;

                IMeasurement measurement = measurements.FirstOrDefault(m => m != null);
                Ticks timestamp = 0;

                if (measurement != null)
                    timestamp = measurement.Timestamp;

                m_parent.SendStartTime(m_clientID, timestamp);
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

            if (measurements.Count() > 0 && Enabled)
            {
                if (TrackLatestMeasurements)
                {
                    // Keep track of latest measurements
                    base.QueueMeasurementsForProcessing(measurements);

                    // See if it is time to publish
                    if (m_lastPublishTime == 0)
                    {
                        // Allow at least one set of measurements to be defined before initial publication
                        m_lastPublishTime = 1;
                    }
                    else if (DateTime.UtcNow.Ticks > m_lastPublishTime + Ticks.FromSeconds(LatestMeasurements.LagTime))
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
                        ProcessMeasurements(currentMeasurements);
                    }
                }
                else
                {
                    // Publish unsynchronized on data receipt otherwise...
                    ProcessMeasurements(measurements);
                }
            }
        }

        private void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            List<ISupportBinaryImage> packet = new List<ISupportBinaryImage>();
            bool useCompactMeasurementFormat = m_useCompactMeasurementFormat;
            int packetSize = 5;

            foreach (IMeasurement measurement in measurements)
            {
                ISupportBinaryImage binaryMeasurement;
                int binaryLength;

                // Serialize the current measurement.
                if (useCompactMeasurementFormat)
                    binaryMeasurement = new CompactMeasurement(measurement, m_signalIndexCache, m_includeTime);
                else
                    binaryMeasurement = new SerializableMeasurement(measurement);

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
            byte[] buffer;

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
                buffer = measurement.BinaryImage;
                data.Write(buffer, 0, buffer.Length);
            }

            // Publish data packet to client
            if (m_parent != null)
                m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, data.ToArray());

            // Track last publication time
            m_lastPublishTime = DateTime.UtcNow.Ticks;
        }

        #endregion
    }
}
