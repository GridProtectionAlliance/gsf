//******************************************************************************************************
//  UnsynchronizedClientSubscription.cs - Gbtc
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GSF.IO;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents an unsynchronized client subscription to the <see cref="DataPublisher" />.
    /// </summary>
    internal class UnsynchronizedClientSubscription : FacileActionAdapterBase, IClientSubscription
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Indicates that a buffer block needed to be retransmitted because
        /// it was previously sent, but no confirmation was received.
        /// </summary>
        public event EventHandler BufferBlockRetransmission;

        /// <summary>
        /// Indicates to the host that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public event EventHandler<EventArgs<IClientSubscription, EventArgs>> ProcessingComplete;

        // Fields
        private readonly SignalIndexCache m_signalIndexCache;
        private readonly Guid m_clientID;
        private readonly Guid m_subscriberID;
        private DataPublisher m_parent;
        private string m_hostName;
        private volatile byte m_compressionStrength;
        private volatile bool m_usePayloadCompression;
        private volatile bool m_useCompactMeasurementFormat;
        private readonly CompressionModes m_compressionModes;
        private MeasurementCompressionBlock m_compressionBlock;
        private long m_lastPublishTime;
        private string m_requestedInputFilter;
        private double m_publishInterval;
        private bool m_includeTime;
        private bool m_useMillisecondResolution;
        private bool m_isNaNFiltered;
        private volatile long[] m_baseTimeOffsets;
        private volatile int m_timeIndex;
        private SharedTimer m_baseTimeRotationTimer;
        private volatile bool m_startTimeSent;
        private IaonSession m_iaonSession;

        private readonly List<byte[]> m_bufferBlockCache;
        private readonly object m_bufferBlockCacheLock;
        private uint m_bufferBlockSequenceNumber;
        private uint m_expectedBufferBlockConfirmationNumber;
        private SharedTimer m_bufferBlockRetransmissionTimer;
        private double m_bufferBlockRetransmissionTimeout;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="parent">Reference to parent.</param>
        /// <param name="clientID"><see cref="Guid"/> based client connection ID.</param>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        /// <param name="compressionModes"><see cref="CompressionModes"/> requested by client.</param>
        public UnsynchronizedClientSubscription(DataPublisher parent, Guid clientID, Guid subscriberID, CompressionModes compressionModes)
        {
            m_parent = parent;
            m_clientID = clientID;
            m_subscriberID = subscriberID;
            m_compressionModes = compressionModes;

            m_signalIndexCache = new SignalIndexCache();
            m_signalIndexCache.SubscriberID = subscriberID;

            m_bufferBlockCache = new List<byte[]>();
            m_bufferBlockCacheLock = new object();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid ClientID => m_clientID;

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid SubscriberID => m_subscriberID;

        /// <summary>
        /// Gets the current signal index cache of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public SignalIndexCache SignalIndexCache => m_signalIndexCache;

        /// <summary>
        /// Gets the input filter requested by the subscriber when establishing this <see cref="IClientSubscription"/>.
        /// </summary>
        public string RequestedInputFilter => m_requestedInputFilter;

        /// <summary>
        /// Gets or sets flag that determines if payload compression should be enabled in data packets of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public bool UsePayloadCompression
        {
            get
            {
                return m_usePayloadCompression;
            }
            set
            {
                m_usePayloadCompression = value;

                if (m_usePayloadCompression)
                    m_useCompactMeasurementFormat = true;
            }
        }

        /// <summary>
        /// Gets or sets the compression strength value to use when <see cref="UsePayloadCompression"/> is <c>true</c> for this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public int CompressionStrength
        {
            get
            {
                return m_compressionStrength;
            }
            set
            {
                if (value < 0)
                    value = 0;

                if (value > 31)
                    value = 31;

                m_compressionStrength = (byte)value;
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
                m_useCompactMeasurementFormat = value || m_usePayloadCompression;
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

                if (!m_includeTime)
                    return 0;

                if (!m_parent.UseBaseTimeOffsets)
                    return 8;

                return !m_useMillisecondResolution ? 4 : 2;
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
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
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
                if ((object)m_iaonSession != null && m_iaonSession.AllAdapters != null)
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
                    if ((object)value != null && !(value.Length == 1 && value[0] == MeasurementKey.Undefined))
                    {
                        m_parent.UpdateSignalIndexCache(m_clientID, m_signalIndexCache, value);

                        if ((object)DataSource != null && (object)m_signalIndexCache != null)
                            value = ParseInputMeasurementKeys(DataSource, false, string.Join("; ", m_signalIndexCache.AuthorizedSignalIDs));
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
        public override bool SupportsTemporalProcessing => false;

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

                if ((object)m_iaonSession != null)
                    status.Append(m_iaonSession.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the status of the active temporal session, if any.
        /// </summary>
        public string TemporalSessionStatus
        {
            get
            {
                if ((object)m_iaonSession == null)
                    return null;

                return m_iaonSession.Status;
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
            MeasurementKey[] inputMeasurementKeys;
            string setting;

            if (Settings.TryGetValue("inputMeasurementKeys", out setting))
            {
                // IMPORTANT: The allowSelect argument of ParseInputMeasurementKeys must be null
                //            in order to prevent SQL injection via the subscription filter expression
                inputMeasurementKeys = ParseInputMeasurementKeys(DataSource, false, setting);
                m_requestedInputFilter = setting;

                // IMPORTANT: We need to remove the setting before calling base.Initialize()
                //            or else we will still be subject to SQL injection
                Settings.Remove("inputMeasurementKeys");
            }
            else
            {
                inputMeasurementKeys = new MeasurementKey[0];
                m_requestedInputFilter = null;
            }

            base.Initialize();

            // Set the InputMeasurementKeys property after calling base.Initialize()
            // so that the base class does not overwrite our setting
            InputMeasurementKeys = inputMeasurementKeys;

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

            if (Settings.TryGetValue("requestNaNValueFilter", out setting))
                m_isNaNFiltered = m_parent.AllowNaNValueFilter && setting.ParseBoolean();
            else
                m_isNaNFiltered = false;

            if (Settings.TryGetValue("bufferBlockRetransmissionTimeout", out setting))
                m_bufferBlockRetransmissionTimeout = double.Parse(setting);
            else
                m_bufferBlockRetransmissionTimeout = 5.0D;

            if (m_parent.UseBaseTimeOffsets && m_includeTime)
            {
                m_baseTimeRotationTimer = Common.TimerScheduler.CreateTimer(m_useMillisecondResolution ? 60000 : 420000);
                m_baseTimeRotationTimer.AutoReset = true;
                m_baseTimeRotationTimer.Elapsed += BaseTimeRotationTimer_Elapsed;
            }

            m_bufferBlockRetransmissionTimer = Common.TimerScheduler.CreateTimer((int)(m_bufferBlockRetransmissionTimeout * 1000.0D));
            m_bufferBlockRetransmissionTimer.AutoReset = false;
            m_bufferBlockRetransmissionTimer.Elapsed += BufferBlockRetransmissionTimer_Elapsed;

            // Handle temporal session initialization
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

            // Reset compressor on successful resubscription
            if ((object)m_compressionBlock != null)
                m_compressionBlock.Reset();

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
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            int inputCount = 0, outputCount = 0;

            if ((object)InputMeasurementKeys != null)
                inputCount = InputMeasurementKeys.Length;

            if ((object)OutputMeasurements != null)
                outputCount = OutputMeasurements.Length;

            return $"Total input measurements: {inputCount}, total output measurements: {outputCount}".PadLeft(maxLength);
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
            if ((object)measurements == null)
                return;

            if (!m_startTimeSent && measurements.Any())
            {
                m_startTimeSent = true;

                IMeasurement measurement = measurements.FirstOrDefault(m => (object)m != null);
                Ticks timestamp = 0;

                if ((object)measurement != null)
                    timestamp = measurement.Timestamp;

                m_parent.SendDataStartTime(m_clientID, timestamp);
            }

            if (m_isNaNFiltered)
                measurements = measurements.Where(measurement => !double.IsNaN(measurement.Value));

            if (!measurements.Any() || !Enabled)
                return;

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
                        newMeasurement = new Measurement
                        {
                            Metadata = measurement.Metadata,
                            Value = measurement.GetValue(RealTime),
                            Timestamp = measurement.Timestamp,
                            StateFlags = measurement.StateFlags
                        };

                        currentMeasurements.Add(newMeasurement);
                    }

                    // Order measurements by signal type for better compression when enabled
                    if (m_usePayloadCompression)
                        ProcessMeasurements(currentMeasurements.OrderBy(measurement => measurement.GetSignalType(DataSource)));
                    else
                        ProcessMeasurements(currentMeasurements);
                }
            }
            else
            {
                // Order measurements by signal type for better compression for non-TSSC compression modes
                if (m_usePayloadCompression && !m_compressionModes.HasFlag(CompressionModes.TSSC))
                    ProcessMeasurements(measurements.OrderBy(measurement => measurement.GetSignalType(DataSource)));
                else
                    ProcessMeasurements(measurements);
            }
        }

        /// <summary>
        /// Handles the confirmation message received from the
        /// subscriber to indicate that a buffer block was received.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the buffer block.</param>
        /// <returns>A list of buffer block sequence numbers for blocks that need to be retransmitted.</returns>
        public void ConfirmBufferBlock(uint sequenceNumber)
        {
            int sequenceIndex;
            int removalCount;

            // We are still receiving confirmations,
            // so stop the retransmission timer
            m_bufferBlockRetransmissionTimer.Stop();

            lock (m_bufferBlockCacheLock)
            {
                // Find the buffer block's location in the cache
                sequenceIndex = (int)(sequenceNumber - m_expectedBufferBlockConfirmationNumber);

                if (sequenceIndex >= 0 && sequenceIndex < m_bufferBlockCache.Count && (object)m_bufferBlockCache[sequenceIndex] != null)
                {
                    // Remove the confirmed block from the cache
                    m_bufferBlockCache[sequenceIndex] = null;

                    if (sequenceNumber == m_expectedBufferBlockConfirmationNumber)
                    {
                        // Get the number of elements to trim from the start of the cache
                        removalCount = m_bufferBlockCache.TakeWhile(m => (object)m == null).Count();

                        // Trim the cache
                        m_bufferBlockCache.RemoveRange(0, removalCount);

                        // Increase the expected confirmation number
                        m_expectedBufferBlockConfirmationNumber += (uint)removalCount;
                    }
                    else
                    {
                        // Retransmit if confirmations are received out of order
                        for (int i = 0; i < sequenceIndex; i++)
                        {
                            if ((object)m_bufferBlockCache[i] != null)
                            {
                                m_parent.SendClientResponse(m_clientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, m_bufferBlockCache[i]);
                                OnBufferBlockRetransmission();
                            }
                        }
                    }
                }

                // If there are any objects lingering in the
                // cache, start the retransmission timer
                if (m_bufferBlockCache.Count > 0)
                    m_bufferBlockRetransmissionTimer.Start();
            }
        }

        private void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            //TSSC does not require a measurement that implements IBinaryMeasurement,
            //Therefore large performance improvements can be attained by using an alternative
            //routing method.
            if (m_usePayloadCompression && m_compressionModes.HasFlag(CompressionModes.TSSC))
            {
                ProcessMeasurementsTSSC(measurements);
                return;
            }
            // Includes data packet flags and measurement count
            const int PacketHeaderSize = DataPublisher.ClientResponseHeaderSize + 5;

            List<IBinaryMeasurement> packet;
            int packetSize;

            bool usePayloadCompression;
            bool useCompactMeasurementFormat;

            BufferBlockMeasurement bufferBlockMeasurement;
            byte[] bufferBlock;
            ushort bufferBlockSignalIndex;

            IBinaryMeasurement binaryMeasurement;
            int binaryLength;

            try
            {
                if (!Enabled)
                    return;

                packet = new List<IBinaryMeasurement>();
                packetSize = PacketHeaderSize;

                usePayloadCompression = m_usePayloadCompression;
                useCompactMeasurementFormat = m_useCompactMeasurementFormat || usePayloadCompression;

                foreach (IMeasurement measurement in measurements)
                {
                    bufferBlockMeasurement = measurement as BufferBlockMeasurement;

                    if ((object)bufferBlockMeasurement != null)
                    {
                        // Still sending buffer block measurements to client; we are expecting
                        // confirmations which will indicate whether retransmission is necessary,
                        // so we will restart the retransmission timer
                        m_bufferBlockRetransmissionTimer.Stop();

                        // Handle buffer block measurements as a special case - this can be any kind of data,
                        // measurement subscriber will need to know how to interpret buffer
                        bufferBlock = new byte[6 + bufferBlockMeasurement.Length];

                        // Prepend sequence number
                        BigEndian.CopyBytes(m_bufferBlockSequenceNumber, bufferBlock, 0);
                        m_bufferBlockSequenceNumber++;

                        // Copy signal index into buffer
                        bufferBlockSignalIndex = m_signalIndexCache.GetSignalIndex(bufferBlockMeasurement.Key);
                        BigEndian.CopyBytes(bufferBlockSignalIndex, bufferBlock, 4);

                        // Append measurement data and send
                        Buffer.BlockCopy(bufferBlockMeasurement.Buffer, 0, bufferBlock, 6, bufferBlockMeasurement.Length);
                        m_parent.SendClientResponse(m_clientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, bufferBlock);

                        lock (m_bufferBlockCacheLock)
                        {
                            // Cache buffer block for retransmission
                            m_bufferBlockCache.Add(bufferBlock);
                        }

                        // Start the retransmission timer in case we never receive a confirmation
                        m_bufferBlockRetransmissionTimer.Start();
                    }
                    else
                    {
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
                            ProcessBinaryMeasurements(packet, useCompactMeasurementFormat, usePayloadCompression);
                            packet.Clear();
                            packetSize = PacketHeaderSize;
                        }

                        // Add the current measurement to the packet.
                        packet.Add(binaryMeasurement);
                        packetSize += binaryLength;
                    }
                }

                // Process the remaining measurements.
                if (packet.Count > 0)
                    ProcessBinaryMeasurements(packet, useCompactMeasurementFormat, usePayloadCompression);

                IncrementProcessedMeasurements(measurements.Count());

                // Update latency statistics
                m_parent.UpdateLatencyStatistics(measurements.Select(m => (long)(m_lastPublishTime - m.Timestamp)));
            }
            catch (Exception ex)
            {
                string message = $"Error processing measurements: {ex.Message}";
                OnProcessException(new InvalidOperationException(message, ex));
            }
        }

        private void ProcessMeasurementsTSSC(IEnumerable<IMeasurement> measurements)
        {
            try
            {
                if (!Enabled)
                    return;

                if ((object)m_compressionBlock == null)
                    m_compressionBlock = new MeasurementCompressionBlock();
                else
                    m_compressionBlock.Clear();

                int count = 0;
                foreach (IMeasurement measurement in measurements)
                {
                    if (!m_compressionBlock.CanAddMeasurements)
                    {
                        SendTSSCPayload(m_compressionBlock, count);
                        count = 0;
                        m_compressionBlock.Clear();
                    }

                    count++;
                    ushort index = m_signalIndexCache.GetSignalIndex(measurement.Key);
                    m_compressionBlock.AddMeasurement(index, measurement.Timestamp.Value, (uint)measurement.StateFlags, (float)measurement.AdjustedValue);
                }

                if (count > 0)
                {
                    SendTSSCPayload(m_compressionBlock, count);
                    count = 0;
                    m_compressionBlock.Clear();
                }

                IncrementProcessedMeasurements(measurements.Count());

                // Update latency statistics
                m_parent.UpdateLatencyStatistics(measurements.Select(m => (long)(m_lastPublishTime - m.Timestamp)));
            }
            catch (Exception ex)
            {
                string message = $"Error processing measurements: {ex.Message}";
                OnProcessException(new InvalidOperationException(message, ex));
            }
        }

        private void SendTSSCPayload(MeasurementCompressionBlock block, int measurementCount)
        {
            // Create working buffer
            using (BlockAllocatedMemoryStream workingBuffer = new BlockAllocatedMemoryStream())
            {
                // Serialize data packet flags into response
                DataPacketFlags flags = DataPacketFlags.NoFlags; // No flags means bit is cleared, i.e., unsynchronized
                flags |= DataPacketFlags.Compressed;

                workingBuffer.WriteByte((byte)flags);
                workingBuffer.Write(BigEndian.GetBytes(measurementCount), 0, 4);
                block.CopyTo(workingBuffer);

                // Publish data packet to client
                if ((object)m_parent != null)
                    m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, workingBuffer.ToArray());

                // Track last publication time
                m_lastPublishTime = DateTime.UtcNow.Ticks;
            }

        }


        private void ProcessBinaryMeasurements(IEnumerable<IBinaryMeasurement> measurements, bool useCompactMeasurementFormat, bool usePayloadCompression)
        {
            // Create working buffer
            using (BlockAllocatedMemoryStream workingBuffer = new BlockAllocatedMemoryStream())
            {
                // Serialize data packet flags into response
                DataPacketFlags flags = DataPacketFlags.NoFlags; // No flags means bit is cleared, i.e., unsynchronized

                if (m_compressionModes.HasFlag(CompressionModes.TSSC))
                {
                    flags |= DataPacketFlags.Compressed;
                }
                else
                {
                    if (useCompactMeasurementFormat)
                        flags |= DataPacketFlags.Compact;
                }

                workingBuffer.WriteByte((byte)flags);

                // No frame level timestamp is serialized into the data packet since all data is unsynchronized and essentially
                // published upon receipt, however timestamps are optionally included in the serialized measurements.

                // Serialize total number of measurement values to follow
                workingBuffer.Write(BigEndian.GetBytes(measurements.Count()), 0, 4);

                if (usePayloadCompression && m_compressionModes.HasFlag(CompressionModes.TSSC))
                {
                    if ((object)m_compressionBlock == null)
                        m_compressionBlock = new MeasurementCompressionBlock();
                    else
                        m_compressionBlock.Clear();

                    foreach (CompactMeasurement measurement in measurements.Cast<CompactMeasurement>())
                    {
                        if (m_compressionBlock.CanAddMeasurements)
                        {
                            m_compressionBlock.AddMeasurement(measurement.RuntimeID, measurement.Timestamp.Value, (uint)measurement.StateFlags, (float)measurement.AdjustedValue);
                        }
                        else
                        {
                            m_compressionBlock.CopyTo(workingBuffer);
                            m_compressionBlock.Clear();
                            m_compressionBlock.AddMeasurement(measurement.RuntimeID, measurement.Timestamp.Value, (uint)measurement.StateFlags, (float)measurement.AdjustedValue);
                        }
                    }

                    m_compressionBlock.CopyTo(workingBuffer);
                }
                else
                {
                    // Attempt compression when requested - encoding of compressed buffer only happens if size would be smaller than normal serialization
                    if (!usePayloadCompression || !measurements.Cast<CompactMeasurement>().CompressPayload(workingBuffer, m_compressionStrength, m_includeTime, ref flags))
                    {
                        // Serialize measurements to data buffer
                        foreach (IBinaryMeasurement measurement in measurements)
                        {
                            measurement.CopyBinaryImageToStream(workingBuffer);
                        }
                    }

                    // Update data packet flags if it has updated compression flags
                    if ((flags & DataPacketFlags.Compressed) > 0)
                    {
                        workingBuffer.Seek(0, SeekOrigin.Begin);
                        workingBuffer.WriteByte((byte)flags);
                    }
                }

                // Publish data packet to client
                if ((object)m_parent != null)
                    m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, workingBuffer.ToArray());

                // Track last publication time
                m_lastPublishTime = DateTime.UtcNow.Ticks;
            }
        }

        // Retransmits all buffer blocks for which confirmation has not yet been received
        private void BufferBlockRetransmissionTimer_Elapsed(object sender, EventArgs<DateTime> e)
        {
            lock (m_bufferBlockCacheLock)
            {
                foreach (byte[] bufferBlock in m_bufferBlockCache)
                {
                    if ((object)bufferBlock != null)
                    {
                        m_parent.SendClientResponse(m_clientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, bufferBlock);
                        OnBufferBlockRetransmission();
                    }
                }
            }

            // Restart the retransmission timer
            m_bufferBlockRetransmissionTimer.Start();
        }

        // Rotates base time offsets
        private void RotateBaseTimes()
        {
            if ((object)m_parent != null && (object)m_baseTimeRotationTimer != null)
            {
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

                // Since this function will only be called periodically, there is no real benefit
                // to maintaining this memory stream at a member level
                using (BlockAllocatedMemoryStream responsePacket = new BlockAllocatedMemoryStream())
                {
                    responsePacket.Write(BigEndian.GetBytes(m_timeIndex), 0, 4);
                    responsePacket.Write(BigEndian.GetBytes(m_baseTimeOffsets[0]), 0, 8);
                    responsePacket.Write(BigEndian.GetBytes(m_baseTimeOffsets[1]), 0, 8);

                    m_parent.SendClientResponse(m_clientID, ServerResponse.UpdateBaseTimes, ServerCommand.Subscribe, responsePacket.ToArray());
                }
            }
        }

        private void OnBufferBlockRetransmission()
        {
            if ((object)BufferBlockRetransmission != null)
                BufferBlockRetransmission(this, EventArgs.Empty);
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
            if ((object)ProcessingComplete != null)
                ProcessingComplete(sender, new EventArgs<IClientSubscription, EventArgs>(this, e));
        }

        private void BaseTimeRotationTimer_Elapsed(object sender, EventArgs<DateTime> e)
        {
            RotateBaseTimes();
        }

        #endregion
    }
}
