﻿//******************************************************************************************************
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

using GSF.Diagnostics;
using GSF.IO;
using GSF.Parsing;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport.TSSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// ReSharper disable PossibleMultipleEnumeration
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
        private DataPublisher m_parent;
        private volatile byte m_compressionStrength;
        private volatile bool m_usePayloadCompression;
        private volatile bool m_useCompactMeasurementFormat;
        private readonly CompressionModes m_compressionModes;
        private bool m_resetTsscEncoder;
        private TsscEncoder m_tsscEncoder;
        private readonly object m_tsscSyncLock;
        private byte[] m_tsscWorkingBuffer;
        private ushort m_tsscSequenceNumber;
        private long m_lastPublishTime;
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
            ClientID = clientID;
            SubscriberID = subscriberID;
            m_compressionModes = compressionModes;

            SignalIndexCache = new SignalIndexCache { SubscriberID = subscriberID };

            m_bufferBlockCache = new List<byte[]>();
            m_bufferBlockCacheLock = new object();

            m_tsscSyncLock = new object();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets name of the action adapter.
        /// </summary>
        public override string Name
        {
            get => base.Name;

            set
            {
                base.Name = value;
                Log.InitialStackMessages = Log.InitialStackMessages.Union("AdapterName", GetType().Name, "HostName", value);
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid ClientID { get; }

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public Guid SubscriberID { get; }

        /// <summary>
        /// Gets the current signal index cache of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public SignalIndexCache SignalIndexCache { get; }

        /// <summary>
        /// Gets the input filter requested by the subscriber when establishing this <see cref="IClientSubscription"/>.
        /// </summary>
        public string RequestedInputFilter { get; private set; }

        /// <summary>
        /// Gets or sets flag that determines if payload compression should be enabled in data packets of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public bool UsePayloadCompression
        {
            get => m_usePayloadCompression;
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
            get => m_compressionStrength;
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
            get => m_useCompactMeasurementFormat;
            set => m_useCompactMeasurementFormat = value || m_usePayloadCompression;
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
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public override int ProcessingInterval
        {
            get => base.ProcessingInterval;
            set
            {
                base.ProcessingInterval = value;

                // Update processing interval in private temporal session, if defined
                if (m_iaonSession?.AllAdapters != null)
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
            get => base.InputMeasurementKeys;
            set
            {
                lock (this)
                {
                    // Update signal index cache unless "detaching" from real-time
                    if (value is not null && !(value.Length == 1 && value[0] == MeasurementKey.Undefined))
                    {
                        m_parent.UpdateSignalIndexCache(ClientID, SignalIndexCache, value);

                        if (DataSource is not null && SignalIndexCache is not null)
                            value = ParseInputMeasurementKeys(DataSource, false, string.Join("; ", SignalIndexCache.AuthorizedSignalIDs));
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
                StringBuilder status = new();

                if (m_parent.ClientConnections.TryGetValue(ClientID, out ClientConnection connection))
                {
                    status.Append(connection.Status);
                    status.AppendLine();
                }

                status.Append(base.Status);

                if (m_iaonSession is not null)
                    status.Append(m_iaonSession.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the status of the active temporal session, if any.
        /// </summary>
        public string TemporalSessionStatus => m_iaonSession?.Status;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="UnsynchronizedClientSubscription"/> object and optionally releases the managed resources.
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

                m_parent = null;

                // Dispose base time rotation timer
                if (m_baseTimeRotationTimer is not null)
                {
                    m_baseTimeRotationTimer.Dispose();
                    m_baseTimeRotationTimer = null;
                }

                // Dispose Iaon session
                this.DisposeTemporalSession(ref m_iaonSession);
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        public override void Initialize()
        {
            MeasurementKey[] inputMeasurementKeys;

            if (Settings.TryGetValue(nameof(inputMeasurementKeys), out string setting))
            {
                // IMPORTANT: The allowSelect argument of ParseInputMeasurementKeys must be null
                //            in order to prevent SQL injection via the subscription filter expression
                inputMeasurementKeys = ParseInputMeasurementKeys(DataSource, false, setting);
                RequestedInputFilter = setting;

                // IMPORTANT: We need to remove the setting before calling base.Initialize()
                //            or else we will still be subject to SQL injection
                Settings.Remove(nameof(inputMeasurementKeys));
            }
            else
            {
                inputMeasurementKeys = Array.Empty<MeasurementKey>();
                RequestedInputFilter = null;
            }

            base.Initialize();

            // Set the InputMeasurementKeys property after calling base.Initialize()
            // so that the base class does not overwrite our setting
            InputMeasurementKeys = inputMeasurementKeys;

            if (!Settings.TryGetValue("publishInterval", out setting) || !double.TryParse(setting, out m_publishInterval))
                m_publishInterval = -1;

            m_includeTime = !Settings.TryGetValue("includeTime", out setting) || setting.ParseBoolean();

            // Default throttled publication to continue with value publication but with suspect time state when timestamps
            // are outside defined lag/lead bounds, but allow override for original NaN output behavior which can be enabled
            // by adding 'publishThrottledOutliersAsNan=true' to ExtraConnectionStringParameters in subscription info objects
            LatestMeasurements.OutlierOperation = Settings.TryGetValue("publishThrottledOutliersAsNan", out setting) && setting.ParseBoolean() ? 
                TemporalOutlierOperation.PublishValueAsNan : 
                TemporalOutlierOperation.PublishWithBadState;

            m_useMillisecondResolution = Settings.TryGetValue("useMillisecondResolution", out setting) && setting.ParseBoolean();

            if (Settings.TryGetValue("requestNaNValueFilter", out setting))
                m_isNaNFiltered = m_parent.AllowNaNValueFilter && setting.ParseBoolean();
            else
                m_isNaNFiltered = false;

            m_bufferBlockRetransmissionTimeout = Settings.TryGetValue("bufferBlockRetransmissionTimeout", out setting) ? double.Parse(setting) : 5.0D;

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
            m_resetTsscEncoder = true;

            base.Start();

            if (m_baseTimeRotationTimer is not null && m_includeTime)
                m_baseTimeRotationTimer.Start();
        }

        /// <summary>
        /// Stops the <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>	
        public override void Stop()
        {
            base.Stop();

            if (m_baseTimeRotationTimer is null)
                return;

            m_baseTimeRotationTimer.Stop();
            m_baseTimeOffsets = null;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="UnsynchronizedClientSubscription"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            int inputCount = 0, outputCount = 0;

            if (InputMeasurementKeys is not null)
                inputCount = InputMeasurementKeys.Length;

            if (OutputMeasurements is not null)
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
        // IMPORTANT: TSSC is sensitive to order - always make sure this function gets called sequentially, concurrent
        // calls to this function can cause TSSC parsing to get out of sequence and fail
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (measurements is null)
                return;

            if (!m_startTimeSent && measurements.Any())
            {
                m_startTimeSent = true;

                IMeasurement measurement = measurements.FirstOrDefault(m => m is not null);
                Ticks timestamp = 0;

                if (measurement is not null)
                    timestamp = measurement.Timestamp;

                m_parent.SendDataStartTime(ClientID, timestamp);
            }

            if (m_isNaNFiltered)
                measurements = measurements.Where(measurement => !double.IsNaN(measurement.Value));

            if (!measurements.Any() || !Enabled)
                return;

            if (TrackLatestMeasurements)
            {
                // Keep track of latest measurements
                base.QueueMeasurementsForProcessing(measurements);
                double publishInterval = m_publishInterval > 0 ? m_publishInterval : LagTime;

                if (DateTime.UtcNow.Ticks <= m_lastPublishTime + Ticks.FromSeconds(publishInterval))
                    return;

                List<IMeasurement> currentMeasurements = new();

                // Create a new set of measurements that represent the latest known values setting value to NaN if it is old
                foreach (TemporalMeasurement measurement in LatestMeasurements)
                {
                    MeasurementStateFlags timeQuality = measurement.Timestamp.TimeIsValid(RealTime, measurement.LagTime, measurement.LeadTime)
                        ? MeasurementStateFlags.Normal
                        : MeasurementStateFlags.BadTime;

                    Measurement newMeasurement = new()
                    {
                        Metadata = measurement.Metadata,
                        Value = measurement.Value,
                        Timestamp = measurement.Timestamp,
                        StateFlags = measurement.StateFlags | timeQuality
                    };

                    currentMeasurements.Add(newMeasurement);
                }

                ProcessMeasurements(currentMeasurements);
            }
            else
            {
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
            DataPublisher parent = m_parent;

            // We are still receiving confirmations,
            // so stop the retransmission timer
            m_bufferBlockRetransmissionTimer.Stop();

            lock (m_bufferBlockCacheLock)
            {
                // Find the buffer block's location in the cache
                int sequenceIndex = (int)(sequenceNumber - m_expectedBufferBlockConfirmationNumber);

                if (sequenceIndex >= 0 && sequenceIndex < m_bufferBlockCache.Count && m_bufferBlockCache[sequenceIndex] is not null)
                {
                    // Remove the confirmed block from the cache
                    m_bufferBlockCache[sequenceIndex] = null;

                    if (sequenceNumber == m_expectedBufferBlockConfirmationNumber)
                    {
                        // Get the number of elements to trim from the start of the cache
                        int removalCount = m_bufferBlockCache.TakeWhile(m => m is null).Count();

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
                            if (m_bufferBlockCache[i] is null)
                                continue;

                            parent?.SendClientResponse(ClientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, m_bufferBlockCache[i]);
                            OnBufferBlockRetransmission();
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
            if (m_usePayloadCompression && m_compressionModes.HasFlag(CompressionModes.TSSC))
            {
                ProcessTSSCMeasurements(measurements);
                return;
            }

            // Includes data packet flags and measurement count
            const int PacketHeaderSize = DataPublisher.ClientResponseHeaderSize + 5;

            try
            {
                if (!Enabled)
                    return;

                List<IBinaryMeasurement> packet = new();
                int packetSize = PacketHeaderSize;

                bool usePayloadCompression = m_usePayloadCompression;
                bool useCompactMeasurementFormat = m_useCompactMeasurementFormat || usePayloadCompression;

                foreach (IMeasurement measurement in measurements)
                {
                    if (measurement is BufferBlockMeasurement bufferBlockMeasurement)
                    {
                        // Still sending buffer block measurements to client; we are expecting
                        // confirmations which will indicate whether retransmission is necessary,
                        // so we will restart the retransmission timer
                        m_bufferBlockRetransmissionTimer.Stop();

                        // Handle buffer block measurements as a special case - this can be any kind of data,
                        // measurement subscriber will need to know how to interpret buffer
                        byte[] bufferBlock = new byte[6 + bufferBlockMeasurement.Length];

                        // Prepend sequence number
                        BigEndian.CopyBytes(m_bufferBlockSequenceNumber, bufferBlock, 0);
                        m_bufferBlockSequenceNumber++;

                        // Copy signal index into buffer
                        ushort bufferBlockSignalIndex = SignalIndexCache.GetSignalIndex(bufferBlockMeasurement.Key);
                        BigEndian.CopyBytes(bufferBlockSignalIndex, bufferBlock, 4);

                        // Append measurement data and send
                        Buffer.BlockCopy(bufferBlockMeasurement.Buffer, 0, bufferBlock, 6, bufferBlockMeasurement.Length);
                        m_parent.SendClientResponse(ClientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, bufferBlock);

                        // Cache buffer block for retransmission
                        lock (m_bufferBlockCacheLock)
                            m_bufferBlockCache.Add(bufferBlock);

                        // Start the retransmission timer in case we never receive a confirmation
                        m_bufferBlockRetransmissionTimer.Start();
                    }
                    else
                    {
                        // Serialize the current measurement.
                        IBinaryMeasurement binaryMeasurement;
                        if (useCompactMeasurementFormat)
                            binaryMeasurement = new CompactMeasurement(measurement, SignalIndexCache, m_includeTime, m_baseTimeOffsets, m_timeIndex, m_useMillisecondResolution);
                        else
                            binaryMeasurement = new SerializableMeasurement(measurement, m_parent.GetClientEncoding(ClientID));

                        // Determine the size of the measurement in bytes.
                        int binaryLength = binaryMeasurement.BinaryLength;

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
                OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
            }
        }

        private void ProcessBinaryMeasurements(IEnumerable<IBinaryMeasurement> measurements, bool useCompactMeasurementFormat, bool usePayloadCompression)
        {
            // Create working buffer
            using BlockAllocatedMemoryStream workingBuffer = new();

            // Serialize data packet flags into response
            DataPacketFlags flags = DataPacketFlags.NoFlags; // No flags means bit is cleared, i.e., unsynchronized

            if (useCompactMeasurementFormat)
                flags |= DataPacketFlags.Compact;

            workingBuffer.WriteByte((byte)flags);

            // No frame level timestamp is serialized into the data packet since all data is unsynchronized and essentially
            // published upon receipt, however timestamps are optionally included in the serialized measurements.

            // Serialize total number of measurement values to follow
            workingBuffer.Write(BigEndian.GetBytes(measurements.Count()), 0, 4);

            // Attempt compression when requested - encoding of compressed buffer only happens if size would be smaller than normal serialization
            if (!usePayloadCompression || !measurements.Cast<CompactMeasurement>().CompressPayload(workingBuffer, m_compressionStrength, m_includeTime, ref flags))
            {
                // Serialize measurements to data buffer
                foreach (IBinaryMeasurement measurement in measurements)
                    measurement.CopyBinaryImageToStream(workingBuffer);
            }

            // Update data packet flags if it has updated compression flags
            if ((flags & DataPacketFlags.Compressed) > 0)
            {
                workingBuffer.Seek(0, SeekOrigin.Begin);
                workingBuffer.WriteByte((byte)flags);
            }

            // Publish data packet to client
            m_parent?.SendClientResponse(ClientID, ServerResponse.DataPacket, ServerCommand.Subscribe, workingBuffer.ToArray());

            // Track last publication time
            m_lastPublishTime = DateTime.UtcNow.Ticks;
        }

        private void ProcessTSSCMeasurements(IEnumerable<IMeasurement> measurements)
        {
            lock (m_tsscSyncLock)
            {
                try
                {
                    if (!Enabled)
                        return;

                    if (m_tsscEncoder is null || m_resetTsscEncoder)
                    {
                        m_resetTsscEncoder = false;
                        m_tsscEncoder = new TsscEncoder();
                        m_tsscWorkingBuffer = new byte[32 * 1024];
                        OnStatusMessage(MessageLevel.Info, $"TSSC algorithm reset before sequence number: {m_tsscSequenceNumber}", nameof(TSSC));
                        m_tsscSequenceNumber = 0;
                    }

                    m_tsscEncoder.SetBuffer(m_tsscWorkingBuffer, 0, m_tsscWorkingBuffer.Length);

                    int count = 0;

                    foreach (IMeasurement measurement in measurements)
                    {
                        ushort index = SignalIndexCache.GetSignalIndex(measurement.Key);

                        if (!m_tsscEncoder.TryAddMeasurement(index, measurement.Timestamp.Value, (uint)measurement.StateFlags, (float)measurement.AdjustedValue))
                        {
                            SendTSSCPayload(count);
                            count = 0;
                            m_tsscEncoder.SetBuffer(m_tsscWorkingBuffer, 0, m_tsscWorkingBuffer.Length);

                            // This will always succeed
                            m_tsscEncoder.TryAddMeasurement(index, measurement.Timestamp.Value, (uint)measurement.StateFlags, (float)measurement.AdjustedValue);
                        }

                        count++;
                    }

                    if (count > 0)
                        SendTSSCPayload(count);

                    IncrementProcessedMeasurements(measurements.Count());

                    // Update latency statistics
                    m_parent.UpdateLatencyStatistics(measurements.Select(m => (long)(m_lastPublishTime - m.Timestamp)));
                }
                catch (Exception ex)
                {
                    string message = $"Error processing measurements: {ex.Message}";
                    OnProcessException(MessageLevel.Info, new InvalidOperationException(message, ex));
                }
            }
        }

        private void SendTSSCPayload(int count)
        {
            int length = m_tsscEncoder.FinishBlock();
            byte[] packet = new byte[length + 8];

            packet[0] = (byte)DataPacketFlags.Compressed;

            // Serialize total number of measurement values to follow
            BigEndian.CopyBytes(count, packet, 1);

            packet[1 + 4] = 85; // A version number
            BigEndian.CopyBytes(m_tsscSequenceNumber, packet, 5 + 1);
            
            m_tsscSequenceNumber++;

            //Do not increment to 0
            if (m_tsscSequenceNumber == 0)
                m_tsscSequenceNumber = 1;

            Array.Copy(m_tsscWorkingBuffer, 0, packet, 8, length);

            m_parent?.SendClientResponse(ClientID, ServerResponse.DataPacket, ServerCommand.Subscribe, packet);

            // Track last publication time
            m_lastPublishTime = DateTime.UtcNow.Ticks;
        }

        // Retransmits all buffer blocks for which confirmation has not yet been received
        private void BufferBlockRetransmissionTimer_Elapsed(object sender, EventArgs<DateTime> e)
        {
            lock (m_bufferBlockCacheLock)
            {
                foreach (byte[] bufferBlock in m_bufferBlockCache.Where(bufferBlock => bufferBlock is not null))
                {
                    m_parent.SendClientResponse(ClientID, ServerResponse.BufferBlock, ServerCommand.Subscribe, bufferBlock);
                    OnBufferBlockRetransmission();
                }
            }

            // Restart the retransmission timer
            m_bufferBlockRetransmissionTimer.Start();
        }

        // Rotates base time offsets
        private void RotateBaseTimes()
        {
            if (m_parent is null || m_baseTimeRotationTimer is null)
                return;

            if (m_baseTimeOffsets is null)
            {
                m_baseTimeOffsets = new long[2];
                m_baseTimeOffsets[0] = RealTime;
                m_baseTimeOffsets[1] = RealTime + m_baseTimeRotationTimer.Interval * Ticks.PerMillisecond;
                m_timeIndex = 0;
            }
            else
            {
                int oldIndex = m_timeIndex;

                // Switch to newer timestamp
                Interlocked.Exchange(ref m_timeIndex, m_timeIndex ^ 1);

                // Now make older timestamp the newer timestamp
                m_baseTimeOffsets[oldIndex] = RealTime + m_baseTimeRotationTimer.Interval * Ticks.PerMillisecond;
            }

            // Since this function will only be called periodically, there is no real benefit
            // to maintaining this memory stream at a member level
            using BlockAllocatedMemoryStream responsePacket = new();

            responsePacket.Write(BigEndian.GetBytes(m_timeIndex), 0, 4);
            responsePacket.Write(BigEndian.GetBytes(m_baseTimeOffsets[0]), 0, 8);
            responsePacket.Write(BigEndian.GetBytes(m_baseTimeOffsets[1]), 0, 8);

            m_parent.SendClientResponse(ClientID, ServerResponse.UpdateBaseTimes, ServerCommand.Subscribe, responsePacket.ToArray());
        }

        private void OnBufferBlockRetransmission() => 
            BufferBlockRetransmission?.Invoke(this, EventArgs.Empty);

        // Explicitly implement status message event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnStatusMessage(MessageLevel level, string status, string eventName, MessageFlags flags) => 
            OnStatusMessage(level, status, eventName, flags);

        // Explicitly implement process exception event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnProcessException(MessageLevel level, Exception ex, string eventName, MessageFlags flags) => 
            OnProcessException(level, ex, eventName, flags);

        // Explicitly implement processing completed event bubbler to satisfy IClientSubscription interface
        void IClientSubscription.OnProcessingCompleted(object sender, EventArgs e) => 
            ProcessingComplete?.Invoke(sender, new EventArgs<IClientSubscription, EventArgs>(this, e));

        private void BaseTimeRotationTimer_Elapsed(object sender, EventArgs<DateTime> e) => 
            RotateBaseTimes();

        #endregion
    }
}
