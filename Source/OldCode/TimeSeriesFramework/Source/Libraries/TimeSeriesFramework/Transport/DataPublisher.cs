//******************************************************************************************************
//  DataPublisher.cs - Gbtc
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
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/15/2010 - Mehulbhai P Thakkar
//       Fixed bug when DataSubscriber tries to resubscribe by setting subscriber. Initialized manually 
//       in ReceiveClientDataComplete event handler.
//  12/02/2010 - J. Ritchie Carroll
//       Fixed an issue for when DataSubcriber dynamically resubscribes with a different
//       synchronization method (e.g., going from unsynchronized to synchronized)
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Communication;
using TVA.IO.Compression;

namespace TimeSeriesFramework.Transport
{
    #region [ Enumerations ]

    /// <summary>
    /// <see cref="DataPublisher"/> server commands.
    /// </summary>
    public enum ServerCommand : byte
    {
        /// <summary>
        /// Authenticate command.
        /// </summary>
        Authenticate = 0x00,
        /// <summary>
        /// Meta data refresh command.
        /// </summary>
        MetaDataRefresh = 0x01,
        /// <summary>
        /// Subscribe command.
        /// </summary>
        Subscribe = 0x02,
        /// <summary>
        /// Unsubscribe command.
        /// </summary>
        Unsubscribe = 0x03
    }

    /// <summary>
    /// <see cref="DataPublisher"/> server responses.
    /// </summary>
    public enum ServerResponse : byte
    {
        /// <summary>
        /// Command succeeded response.
        /// </summary>
        Succeeded = 0xD0,
        /// <summary>
        /// Command failed response.
        /// </summary>
        Failed = 0xD1,
        /// <summary>
        /// Data packet response.
        /// </summary>
        DataPacket = 0xD2,
        /// <summary>
        /// Signal index cache update.
        /// </summary>
        SignalIndexCacheUpdate = 0xD3,
        /// <summary>
        /// Base time update.
        /// </summary>
        BaseTimeUpdate = 0xD4,
        /// <summary>
        /// Cipher key update.
        /// </summary>
        CipherKeyUpdate = 0xD5
    }

    /// <summary>
    /// <see cref="DataPublisher"/> data packet flags.
    /// </summary>
    [Flags()]
    public enum DataPacketFlags : byte
    {
        /// <summary>
        /// Determines if data packet is synchronized. Bit set = synchronized, bit clear = unsynchronized.
        /// </summary>
        Synchronized = (byte)Bits.Bit00,
        /// <summary>
        /// Determines if serialized measurement is compact. Bit set = compact, bit clear = full fidelity.
        /// </summary>
        Compact = (byte)Bits.Bit01,
        /// <summary>
        /// No flags set. This would represent unsynchronized, full fidelity measurement data packets.
        /// </summary>
        NoFlags = (Byte)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Represents a data publishing server that allows multiple connections for data subscriptions.
    /// </summary>
    public class DataPublisher : ActionAdapterCollection
    {
        #region [ Members ]

        // Nested Types

        // Client subscription action adapter interface
        private interface IClientSubscription : IActionAdapter
        {
            /// <summary>
            /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="IClientSubscription"/>.
            /// </summary>
            Guid ClientID
            {
                get;
            }

            /// <summary>
            /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="IClientSubscription"/>.
            /// </summary>
            Guid SubscriberID
            {
                get;
            }

            /// <summary>
            /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="IClientSubscription"/>.
            /// </summary>
            bool UseCompactMeasurementFormat
            {
                get;
                set;
            }
        }

        // Synchronized action adapter interface
        private class SynchronizedClientSubscription : ActionAdapterBase, IClientSubscription
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
                        m_parent.UpdateSignalIndexCache(m_signalIndexCache, value);
                    }
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
                    data.Write(EndianOrder.BigEndian.GetBytes((long)frame.Timestamp), 0, 8);

                    // Serialize total number of measurement values to follow
                    data.Write(EndianOrder.BigEndian.GetBytes(frame.Measurements.Values.Count), 0, 4);

                    // Serialize measurements to data buffer
                    foreach (IMeasurement measurement in frame.Measurements.Values)
                    {
                        if (useCompactMeasurementFormat)
                            buffer = (new CompactMeasurement(measurement, m_signalIndexCache, false)).BinaryImage;
                        else
                            buffer = (new SerializableMeasurement(measurement)).BinaryImage;

                        data.Write(buffer, 0, buffer.Length);
                    }

                    // Publish data packet to client
                    if (m_parent != null)
                        m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, data.ToArray());
                }
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

        // Unsynchronized action adapter interface
        private class UnsynchronizedClientSubscription : FacileActionAdapterBase, IClientSubscription
        {
            #region [ Members ]

            // Fields
            private SignalIndexCache m_signalIndexCache;
            private DataPublisher m_parent;
            private Guid m_clientID;
            private Guid m_subscriberID;
            private bool m_useCompactMeasurementFormat;
            private long m_lastPublishTime;
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
                        m_parent.UpdateSignalIndexCache(m_signalIndexCache, value);
                    }
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
            /// Queues a single measurement for processing.
            /// </summary>
            /// <param name="measurement">Measurement to queue for processing.</param>
            /// <remarks>
            /// Measurement is filtered against the defined <see cref="InputMeasurementKeys"/> so we override method
            /// so that dyanmic updates to keys will be synchronized with filtering to prevent interference.
            /// </remarks>
            public override void QueueMeasurementForProcessing(IMeasurement measurement)
            {
                QueueMeasurementsForProcessing(new IMeasurement[] { measurement });
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
                                newMeasurement = new Measurement(measurement.ID, measurement.Source, measurement.SignalID, measurement.GetAdjustedValue(RealTime), measurement.Adder, measurement.Multiplier, measurement.Timestamp);
                                newMeasurement.TimestampQualityIsGood = measurement.TimestampQualityIsGood;
                                newMeasurement.ValueQualityIsGood = measurement.ValueQualityIsGood;
                                currentMeasurements.Add(newMeasurement);
                            }

                            // Publish latest data values...
                            ThreadPool.QueueUserWorkItem(ProcessMeasurements, currentMeasurements);
                        }
                    }
                    else
                    {
                        // Publish unsynchronized on data receipt otherwise...
                        ThreadPool.QueueUserWorkItem(ProcessMeasurements, measurements);
                    }
                }
            }

            private void ProcessMeasurements(object state)
            {
                if (state != null && !m_disposed)
                {
                    IEnumerable<IMeasurement> measurements = state as IEnumerable<IMeasurement>;
                    MemoryStream data = new MemoryStream();
                    bool useCompactMeasurementFormat = m_useCompactMeasurementFormat;
                    byte[] buffer;

                    // Serialize data packet flags into response
                    DataPacketFlags flags = DataPacketFlags.NoFlags; // No flags means bit is cleared, i.e., unsynchronized

                    if (useCompactMeasurementFormat)
                        flags |= DataPacketFlags.Compact;

                    data.WriteByte((byte)flags);

                    // No frame level timestamp is serialized into the data packet since all data is unsynchronized and
                    // essentially published upon receipt, however timestamps are included in the serialized measurements.

                    // Serialize total number of measurement values to follow
                    data.Write(EndianOrder.BigEndian.GetBytes(measurements.Count()), 0, 4);

                    // Serialize measurements to data buffer
                    foreach (IMeasurement measurement in measurements)
                    {
                        if (useCompactMeasurementFormat)
                            buffer = (new CompactMeasurement(measurement, m_signalIndexCache, true)).BinaryImage;
                        else
                            buffer = (new SerializableMeasurement(measurement)).BinaryImage;

                        data.Write(buffer, 0, buffer.Length);
                    }

                    // Publish data packet to client
                    if (m_parent != null)
                        m_parent.SendClientResponse(m_clientID, ServerResponse.DataPacket, ServerCommand.Subscribe, data.ToArray());

                    // Track last publication time
                    m_lastPublishTime = DateTime.UtcNow.Ticks;
                }
            }

            #endregion
        }

        // Client connection class
        private class ClientConnection
        {
            #region [ Members ]

            // Fields
            private Guid m_clientID;
            private Guid m_subscriberID;
            private string m_connectionID;
            private bool m_authenticated;
            private IClientSubscription m_subscription;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new <see cref="ClientConnection"/> instance.
            /// </summary>
            /// <param name="clientID">Client ID of associated connection.</param>
            /// <param name="commandChannel"><see cref="TcpServer"/> command channel used to lookup connection information.</param>
            public ClientConnection(Guid clientID, TcpServer commandChannel)
            {
                m_clientID = clientID;
                m_subscriberID = clientID;

                // Attempt to lookup remote connection identification for logging purposes
                try
                {
                    IPEndPoint remoteEndPoint = null;

                    if (commandChannel != null)
                        remoteEndPoint = commandChannel.Client(clientID).Provider.RemoteEndPoint as IPEndPoint;

                    if (remoteEndPoint != null)
                    {
                        if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                            m_connectionID = "[" + remoteEndPoint.Address + "]:" + remoteEndPoint.Port;
                        else
                            m_connectionID = remoteEndPoint.Address + ":" + remoteEndPoint.Port;

                        try
                        {
                            IPHostEntry ipHost = Dns.GetHostEntry(remoteEndPoint.Address);

                            if (!string.IsNullOrWhiteSpace(ipHost.HostName))
                                m_connectionID = ipHost.HostName + " (" + m_connectionID + ")";
                        }
                        catch
                        {
                            // Just ignoring possible DNS lookup failures...
                        }
                    }
                }
                catch
                {
                    // At worst we'll just use the client GUID for identification
                    m_connectionID = (m_subscriberID == Guid.Empty ? clientID.ToString() : m_subscriberID.ToString());
                }

                if (string.IsNullOrEmpty(m_connectionID))
                    m_connectionID = "unavailable";
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets client ID of this <see cref="ClientConnection"/>.
            /// </summary>
            public Guid ClientID
            {
                get
                {
                    return m_clientID;
                }
            }

            /// <summary>
            /// Gets or sets the <see cref="Guid"/> based subscriber ID of this <see cref="ClientConnection"/>.
            /// </summary>
            public Guid SubscriberID
            {
                get
                {
                    return m_subscriberID;
                }
                set
                {
                    m_subscriberID = value;
                }
            }

            /// <summary>
            /// Gets the connection identification of this <see cref="ClientConnection"/>.
            /// </summary>
            public string ConnectionID
            {
                get
                {
                    return m_connectionID;
                }
            }

            /// <summary>
            /// Gets or sets authenticated state of this <see cref="ClientConnection"/>.
            /// </summary>
            public bool Authenticated
            {
                get
                {
                    return m_authenticated;
                }
                set
                {
                    m_authenticated = value;
                }
            }

            /// <summary>
            /// Gets or sets subscription associated with this <see cref="ClientConnection"/>.
            /// </summary>
            public IClientSubscription Subscription
            {
                get
                {
                    return m_subscription;
                }
                set
                {
                    m_subscription = value;
                }
            }

            #endregion

            #region [ Methods ]

            #endregion
        }

        // Fields
        private TcpServer m_commandChannel;
        private ConcurrentDictionary<Guid, ClientConnection> m_clientConnections;
        private bool m_requireClientAuthentication;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataPublisher"/>.
        /// </summary>
        public DataPublisher()
        {
            base.Name = "Data Publisher Collection";
            base.DataMember = "[internal]";
            m_clientConnections = new ConcurrentDictionary<Guid, ClientConnection>();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="DataPublisher"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~DataPublisher()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataPublisher"/> should require subscribers to authenticate before making data requests.
        /// </summary>
        public bool RequireClientAuthentication
        {
            get
            {
                return m_requireClientAuthentication;
            }
            set
            {
                m_requireClientAuthentication = value;
            }
        }

        /// <summary>
        /// Gets the status of this <see cref="DataPublisher"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide current status information about the adapter for display purposes.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                if (m_commandChannel != null)
                    status.Append(m_commandChannel.Status);

                status.Append(base.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the name of this <see cref="DataPublisher"/>.
        /// </summary>
        /// <remarks>
        /// The assigned name is used as the settings category when persisting the TCP server settings.
        /// </remarks>
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value.ToUpper();

                if (m_commandChannel != null)
                    m_commandChannel.SettingsCategory = value;
            }
        }


        /// <summary>
        /// Gets or sets reference to <see cref="TcpServer"/> command channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected TcpServer CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                if (m_commandChannel != null)
                {
                    // Detach from events on existing command channel reference
                    m_commandChannel.ClientConnected -= m_commandChannel_ClientConnected;
                    m_commandChannel.ClientDisconnected -= m_commandChannel_ClientDisconnected;
                    m_commandChannel.HandshakeProcessTimeout -= m_commandChannel_HandshakeProcessTimeout;
                    m_commandChannel.HandshakeProcessUnsuccessful -= m_commandChannel_HandshakeProcessUnsuccessful;
                    m_commandChannel.ReceiveClientDataComplete -= m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.ReceiveClientDataException -= m_commandChannel_ReceiveClientDataException;
                    m_commandChannel.ReceiveClientDataTimeout -= m_commandChannel_ReceiveClientDataTimeout;
                    m_commandChannel.SendClientDataException -= m_commandChannel_SendClientDataException;
                    m_commandChannel.ServerStarted -= m_commandChannel_ServerStarted;
                    m_commandChannel.ServerStopped -= m_commandChannel_ServerStopped;

                    if (m_commandChannel != value)
                        m_commandChannel.Dispose();
                }

                // Assign new command channel reference
                m_commandChannel = value;

                if (m_commandChannel != null)
                {
                    // Attach to desired events on new command channel reference
                    m_commandChannel.ClientConnected += m_commandChannel_ClientConnected;
                    m_commandChannel.ClientDisconnected += m_commandChannel_ClientDisconnected;
                    m_commandChannel.HandshakeProcessTimeout += m_commandChannel_HandshakeProcessTimeout;
                    m_commandChannel.HandshakeProcessUnsuccessful += m_commandChannel_HandshakeProcessUnsuccessful;
                    m_commandChannel.ReceiveClientDataComplete += m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.ReceiveClientDataException += m_commandChannel_ReceiveClientDataException;
                    m_commandChannel.ReceiveClientDataTimeout += m_commandChannel_ReceiveClientDataTimeout;
                    m_commandChannel.SendClientDataException += m_commandChannel_SendClientDataException;
                    m_commandChannel.ServerStarted += m_commandChannel_ServerStarted;
                    m_commandChannel.ServerStopped += m_commandChannel_ServerStopped;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataPublisher"/> object and optionally releases the managed resources.
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
                        CommandChannel = null;
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
        /// Intializes <see cref="DataPublisher"/>.
        /// </summary>
        public override void Initialize()
        {
            // We don't call base class initialize since it tries to auto-load adapters from the defined
            // data member - instead, the data publisher dynamically creates adapters upon request
            Initialized = false;

            Clear();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue("requireClientAuthentication", out setting))
                m_requireClientAuthentication = setting.ParseBoolean();

            // Create a new TCP server
            TcpServer commandChannel = new TcpServer();

            // Initialize default settings
            commandChannel.SettingsCategory = Name;
            commandChannel.ConfigurationString = "port=6165";
            commandChannel.PayloadAware = true;
            commandChannel.Compression = CompressionStrength.Standard;
            commandChannel.PersistSettings = true;

            // Assign command channel client reference and attach to needed events
            this.CommandChannel = commandChannel;

            // Initialize TCP server (loads config file settings)
            commandChannel.Initialize();

            Initialized = true;
        }

        /// <summary>
        /// Establish <see cref="DataPublisher"/> and start listening for client connections.
        /// </summary>
        public override void Start()
        {
            if (!Enabled)
            {
                base.Start();

                if (m_commandChannel != null)
                    m_commandChannel.Start();
            }
        }

        /// <summary>
        /// Terminate <see cref="DataPublisher"/> and stop listening for client connections.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (m_commandChannel != null)
                m_commandChannel.Stop();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="DataPublisher"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of the <see cref="DataPublisher"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (m_commandChannel != null)
                return string.Format("Publishing data to {0} clients.", m_commandChannel.ClientIDs.Length).CenterText(maxLength);

            return "Currently not connected".CenterText(maxLength);
        }

        // Update signal index cache based on input measurement keys
        private void UpdateSignalIndexCache(SignalIndexCache signalIndexCache, MeasurementKey[] inputMeasurementKeys)
        {
            List<MeasurementKey> unauthorizedKeys = new List<MeasurementKey>();
            ushort index = 0;
            Guid signalID;

            signalIndexCache.Reference.Clear();

            foreach (MeasurementKey key in inputMeasurementKeys)
            {
                if (m_requireClientAuthentication)
                {
                    // Validate that subscriber has rights to this signal
                    if (SubcriberHasRights(signalIndexCache.SubscriberID, key, out signalID))
                        signalIndexCache.Reference.TryAdd(index++, new Tuple<Guid, MeasurementKey>(signalID, key));
                    else
                        unauthorizedKeys.Add(key);
                }
                else
                {
                    TryLookupSignalID(key, out signalID);
                    signalIndexCache.Reference.TryAdd(index++, new Tuple<Guid, MeasurementKey>(signalID, key));
                }
            }

            signalIndexCache.UnauthorizedKeys = unauthorizedKeys.ToArray();
        }

        /// <summary>
        /// Determines if subscriber has rights to specified <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
        /// <param name="signalID"><see cref="Guid"/> signal ID if found; otherwise an empty Guid.</param>
        /// <returns><c>true</c> if subscriber has rights to specified <see cref="MeasurementKey"/>; otherwise <c>false</c>.</returns>
        protected bool SubcriberHasRights(Guid subscriberID, MeasurementKey key, out Guid signalID)
        {
            if (TryLookupSignalID(key, out signalID))
            {
                try
                {
                    // Lookup explicit measurements
                    DataRow[] explicitMeasurement = DataSource.Tables["SubscriberMeasurement"].Select("SignalID='" + signalID.ToString() + "'");

                    if (explicitMeasurement.Length > 0)
                        return explicitMeasurement[0]["Allowed"].ToNonNullString("0").ParseBoolean();

                    // Lookup group based measurements
                    //DataRow[] implicitMeasurements;
                }
                catch
                {
                    // Errors here are not catastrophic, this simply limits the rights of a subscriber to a measurement
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to lookup <see cref="Guid"/> signal ID for given <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
        /// <param name="signalID"><see cref="Guid"/> signal ID if found; otherwise an empty Guid.</param>
        /// <returns><c>true</c> if <see cref="Guid"/> signal ID was found for given <see cref="MeasurementKey"/>; otherwise <c>false</c>.</returns>
        protected bool TryLookupSignalID(MeasurementKey key, out Guid signalID)
        {
            // TODO: cache this in a local concurrent dictionary for quick subsequent runtime lookups

            // Attempt to lookup input measurement keys for given source IDs from default measurement table, if defined
            try
            {
                DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("ID='" + key.ToString() + "'");

                if (filteredRows.Length > 0 && Guid.TryParse(filteredRows[0]["SignalID"].ToString(), out signalID))
                    return true;
            }
            catch
            {
                // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys based on specified source ID's
            }

            signalID = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Sends response back to specified client.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="response">Server response.</param>
        /// <param name="command">In response to command.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command)
        {
            return SendClientResponse(clientID, response, command, (byte[])null);
        }

        /// <summary>
        /// Sends response back to specified client with a message.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="response">Server response.</param>
        /// <param name="command">In response to command.</param>
        /// <param name="status">Status message to return.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, string status)
        {
            if (status != null)
                return SendClientResponse(clientID, response, command, Encoding.Unicode.GetBytes(status));

            return SendClientResponse(clientID, response, command);
        }

        /// <summary>
        /// Sends response back to specified client with a formatted message.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="response">Server response.</param>
        /// <param name="command">In response to command.</param>
        /// <param name="formattedStatus">Formatted status message to return.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, string formattedStatus, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(formattedStatus))
                return SendClientResponse(clientID, response, command, Encoding.Unicode.GetBytes(string.Format(formattedStatus, args)));

            return SendClientResponse(clientID, response, command);
        }

        /// <summary>
        /// Sends response back to specified client with attached data.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="response">Server response.</param>
        /// <param name="command">In response to command.</param>
        /// <param name="data">Data to return to client; null if none.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, byte[] data)
        {
            return SendClientResponse(clientID, (byte)response, (byte)command, data);
        }

        // Send binary response packet to client
        private bool SendClientResponse(Guid clientID, byte responseCode, byte commandCode, byte[] data)
        {
            bool success = false;

            // Send response packet
            if (m_commandChannel != null && m_commandChannel.CurrentState == ServerState.Running)
            {
                try
                {
                    MemoryStream responsePacket = new MemoryStream();

                    // Add response code
                    responsePacket.WriteByte(responseCode);

                    // Add original in response to command code
                    responsePacket.WriteByte(commandCode);

                    if (data == null || data.Length == 0)
                    {
                        // Add zero sized data buffer to response packet
                        responsePacket.Write(EndianOrder.BigEndian.GetBytes(0), 0, 4);
                    }
                    else
                    {
                        // Add size of data buffer to response packet
                        responsePacket.Write(EndianOrder.BigEndian.GetBytes(data.Length), 0, 4);

                        // Add data buffer
                        responsePacket.Write(data, 0, data.Length);
                    }

                    m_commandChannel.SendToAsync(clientID, responsePacket.ToArray());

                    success = true;
                }
                catch (ObjectDisposedException)
                {
                    // This happens when there is still data to be sent to a disconnected client - we can safely ignore this exception
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    if (ex.ErrorCode != 10054)
                        OnProcessException(new InvalidOperationException("Failed to send response packet to client due to exception: " + ex.Message, ex));
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to send response packet to client due to exception: " + ex.Message, ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException("Publisher is not running. Cannot send response packet."));

            return success;
        }

        // Remove client subscription
        private void RemoveClientSubscription(Guid clientID)
        {
            IClientSubscription clientSubscription;

            lock (this)
            {
                if (TryGetClientSubscription(clientID, out clientSubscription))
                    Remove(clientSubscription);
            }
        }

        // Attempt to find client subscription
        private bool TryGetClientSubscription(Guid clientID, out IClientSubscription subscription)
        {
            IActionAdapter adapter;

            // Lookup adapter by its client ID
            if (TryGetAdapter<Guid>(clientID, (item, value) => ((IClientSubscription)item).ClientID == value, out adapter))
            {
                subscription = (IClientSubscription)adapter;
                return true;
            }

            subscription = null;
            return false;
        }

        private void m_commandChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;

            m_clientConnections[clientID] = new ClientConnection(clientID, m_commandChannel);

            OnStatusMessage("Client connected.");
        }

        private void m_commandChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;
            ClientConnection connection;

            RemoveClientSubscription(clientID);
            m_clientConnections.TryRemove(clientID, out connection);

            OnStatusMessage("Client disconnected.");
        }

        private void m_commandChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            ClientConnection connection;
            Guid clientID = e.Argument1;
            byte[] buffer = e.Argument2;
            int length = e.Argument3;

            // Look up this client connection
            if (!m_clientConnections.TryGetValue(clientID, out connection))
            {
                // Received a request from an unknown client, this request is denied
                OnStatusMessage("WARNING: Ignored {0} byte request received from an unrecognized client: {1}", length, clientID);
                return;
            }

            if (length > 0 && buffer != null)
            {
                // Query command byte
                byte commandByte = buffer[0];
                ServerCommand command;
                IClientSubscription subscription;
                string message, setting;

                // See if command byte represents a valid server command
                if (Enum.TryParse<ServerCommand>(commandByte.ToString(), out command))
                {
                    if (command == ServerCommand.Authenticate)
                    {
                        // Handle authentication request

                        // If validated authenication request...
                        if (true)
                        {
                            connection.Authenticated = true;
                            connection.SubscriberID = Guid.Empty; // Should have looked up subscriber ID based on auth request and/or IP validation...
                        }
                    }
                    else if (m_requireClientAuthentication && !connection.Authenticated)
                    {
                        message = string.Format("Subscriber not authenticated - {0} request denied.", command);
                        SendClientResponse(clientID, ServerResponse.Failed, command, message);
                        OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber not authenticated.", connection.ConnectionID, command);
                        return;
                    }

                    switch (command)
                    {
                        case ServerCommand.Subscribe:
                            // Handle subscribe
                            try
                            {
                                // Make sure there is enough buffer for flags and connection string length
                                if (length >= 6)
                                {
                                    // Next byte is the data packet flags
                                    DataPacketFlags flags = (DataPacketFlags)buffer[1];
                                    bool useSynchronizedSubscription = ((byte)(flags & DataPacketFlags.Synchronized) > 0);
                                    bool useCompactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                                    bool addSubscription = false;

                                    // Next 4 bytes are an integer representing the length of the connection string that follows
                                    int byteLength = EndianOrder.BigEndian.ToInt32(buffer, 2);

                                    if (byteLength > 0 && length >= 6 + byteLength)
                                    {
                                        string connectionString = Encoding.Unicode.GetString(buffer, 6, byteLength);

                                        // Get client subscription
                                        if (connection.Subscription == null)
                                            TryGetClientSubscription(clientID, out subscription);
                                        else
                                            subscription = connection.Subscription;

                                        if (subscription == null)
                                        {
                                            // Client subscription not established yet, so we create a new one
                                            if (useSynchronizedSubscription)
                                                subscription = new SynchronizedClientSubscription(this, clientID, connection.SubscriberID);
                                            else
                                                subscription = new UnsynchronizedClientSubscription(this, clientID, connection.SubscriberID);

                                            addSubscription = true;
                                        }
                                        else
                                        {
                                            // Check to see if consumer is requesting to change synchronization method
                                            if (useSynchronizedSubscription)
                                            {
                                                if (subscription is UnsynchronizedClientSubscription)
                                                {
                                                    // Subscription is for unsynchronized measurements and consumer is requesting synchronized
                                                    subscription.Stop();

                                                    lock (this)
                                                    {
                                                        Remove(subscription);
                                                    }

                                                    // Create a new synchronized subscription
                                                    subscription = new SynchronizedClientSubscription(this, clientID, connection.SubscriberID);
                                                    addSubscription = true;
                                                }
                                            }
                                            else
                                            {
                                                if (subscription is SynchronizedClientSubscription)
                                                {
                                                    // Subscription is for synchronized measurements and consumer is requesting unsynchronized
                                                    subscription.Stop();

                                                    lock (this)
                                                    {
                                                        Remove(subscription);
                                                    }

                                                    // Create a new unsynchronized subscription
                                                    subscription = new UnsynchronizedClientSubscription(this, clientID, connection.SubscriberID);
                                                    addSubscription = true;
                                                }
                                            }
                                        }

                                        // Update client subscription properties
                                        subscription.ConnectionString = connectionString;
                                        subscription.DataSource = DataSource;

                                        if (subscription.Settings.TryGetValue("initializationTimeout", out setting))
                                            subscription.InitializationTimeout = int.Parse(setting);
                                        else
                                            subscription.InitializationTimeout = InitializationTimeout;

                                        // Update measurement serialization format type
                                        subscription.UseCompactMeasurementFormat = useCompactMeasurementFormat;

                                        // Subscribed signals (i.e., input measurement keys) will be parsed from connection string during
                                        // initialization of adapter. This should also gracefully handle "resubscribing" which can add and
                                        // remove subscribed points since assignment and use of input measurement keys is synchronized
                                        // within the client subscription class
                                        if (addSubscription)
                                        {
                                            // Adding client subscription to collection will automatically initialize it
                                            lock (this)
                                            {
                                                Add(subscription);
                                            }
                                        }
                                        else
                                        {
                                            // Manually re-initialize existing client subscription
                                            subscription.Initialize();
                                            subscription.Initialized = true;
                                        }

                                        // Make sure adapter is started
                                        subscription.Start();

                                        // Track subscription in connection information
                                        connection.Subscription = subscription;

                                        // Validate the rights of the selected input measurement keys


                                        // Send success response
                                        if (subscription.InputMeasurementKeys != null)
                                            message = string.Format("Client subscribed as {0}compact {1}synchronized with {2} signals.", useCompactMeasurementFormat ? "" : "non-", useSynchronizedSubscription ? "" : "un", subscription.InputMeasurementKeys.Length);
                                        else
                                            message = string.Format("Client subscribed as {0}compact {1}synchronized, but no signals were specified. Make sure \"inputMeasurementKeys\" setting is properly defined.", useCompactMeasurementFormat ? "" : "non-", useSynchronizedSubscription ? "" : "un");

                                        SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Subscribe, message);
                                        OnStatusMessage(message);
                                    }
                                    else
                                    {
                                        if (byteLength > 0)
                                            message = "Not enough buffer was provided to parse client data subscription.";
                                        else
                                            message = "Cannot initialize client data subscription without a connection string.";

                                        SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
                                        OnProcessException(new InvalidOperationException(message));
                                    }
                                }
                                else
                                {
                                    message = "Not enough buffer was provided to parse client data subscription.";
                                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
                                    OnProcessException(new InvalidOperationException(message));
                                }
                            }
                            catch (Exception ex)
                            {
                                message = "Failed to process client data subscription due to exception: " + ex.Message;
                                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
                                OnProcessException(new InvalidOperationException(message, ex));
                            }
                            break;
                        case ServerCommand.Unsubscribe:
                            // Handle unsubscribe
                            RemoveClientSubscription(clientID); // This does not disconnect client command channel...
                            connection.Subscription = null;
                            message = "Client unsubscribed.";
                            SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Unsubscribe, message);
                            OnStatusMessage(connection.ConnectionID + " unsubscribed.");
                            break;
                        case ServerCommand.MetaDataRefresh:
                            // Handle meta data refresh
                            message = "Client request for query points command is not implemented yet.";
                            SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.MetaDataRefresh, message);
                            OnProcessException(new NotImplementedException(message));
                            break;
                    }
                }
                else
                {
                    // Handle unrecognized commands
                    message = "sent an unrecognized server command: 0x" + commandByte.ToString("X").PadLeft(2, '0');
                    SendClientResponse(clientID, (byte)ServerResponse.Failed, commandByte, Encoding.Unicode.GetBytes("Client " + message));
                    OnProcessException(new InvalidOperationException("WARNING: " + connection.ConnectionID + message));
                }
            }
        }

        private void m_commandChannel_ServerStarted(object sender, EventArgs e)
        {
            OnStatusMessage("Data publisher started.");
        }

        private void m_commandChannel_ServerStopped(object sender, EventArgs e)
        {
            OnStatusMessage("Data publisher stopped.");
        }

        private void m_commandChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data publisher encountered an exception while sending data to client connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data publisher encountered an exception while receiving data from client connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveClientDataTimeout(object sender, EventArgs<Guid> e)
        {
            OnProcessException(new InvalidOperationException("Data publisher timed out while receiving data from client connection"));
        }

        private void m_commandChannel_HandshakeProcessUnsuccessful(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data publisher failed to validate client connection"));
        }

        private void m_commandChannel_HandshakeProcessTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data publisher timed out while trying validate client connection"));
        }

        #endregion
    }
}
