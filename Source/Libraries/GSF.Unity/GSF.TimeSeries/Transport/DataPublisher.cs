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
//  05/26/2011 - J. Ritchie Carroll
//       Implemented subscriber authentication model.
//  02/07/2012 - Mehulbhai Thakkar
//       Modified m_metadataTables to include filter expression and made the list ";" seperated.
//
//******************************************************************************************************

using GSF.Communication;
//using GSF.Data;
//using GSF.Security.Cryptography;
using GSF.TimeSeries.Adapters;
//using GSF.TimeSeries.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace GSF.TimeSeries.Transport
{
    #region [ Enumerations ]

    /// <summary>
    /// Server commands received by <see cref="DataPublisher"/> and sent by <see cref="DataSubscriber"/>.
    /// </summary>
    /// <remarks>
    /// Solicited server commands will receive a <see cref="ServerResponse.Succeeded"/> or <see cref="ServerResponse.Failed"/>
    /// response code along with an associated success or failure message. Message type for successful responses will be based
    /// on server command - for example, server response for a successful MetaDataRefresh command will return a serialized
    /// <see cref="DataSet"/> of the available server metadata. Message type for failed responses will always be a string of
    /// text respresenting the error message.
    /// </remarks>
    public enum ServerCommand : byte
    {
        /// <summary>
        /// Authenticate command.
        /// </summary>
        /// <remarks>
        /// Requests that server authenticate client with the following encrypted authentication packet.
        /// Successful return message type will be string indicating client ID was validated.
        /// Client should not attempt further steps if authentication fails, user will need to take action.
        /// Client should send MetaDataRefresh and Subscribe commands upon successful authentication.
        /// If server is setup to not require authentication, authentication step can be skipped - however
        /// it will be up to the implementation to know if authentication is required - the server cannot be
        /// queried to determine if authentication is required.
        /// It is expected that use of this protocol between gateways over a shared network (i.e., the lower
        /// security zone) will require authentication.
        /// It is expected that use of this protocol between gateway and other systems (e.g., a PDC) in a
        /// private network (i.e., the higher security zone) can be setup to not require authentication.
        /// </remarks>
        Authenticate = 0x00,
        /// <summary>
        /// Meta data refresh command.
        /// </summary>
        /// <remarks>
        /// Requests that server send an updated set of metadata so client can refresh its point list.
        /// Successful return message type will be a <see cref="DataSet"/> containing server device and measurement metadata.
        /// Received device list should be defined as children of the "parent" server device connection similar to the way
        /// PMUs are defined as children of a parent PDC device connection.
        /// Devices and measurements contain unique Guids that should be used to key metadata updates in local repository.
        /// </remarks>
        MetaDataRefresh = 0x01,
        /// <summary>
        /// Subscribe command.
        /// </summary>
        /// <remarks>
        /// Requests a subscription of streaming data from server based on connection string that follows.
        /// It will not be necessary to stop an existing subscription before requesting a new one.
        /// Successful return message type will be string indicating total number of allowed points.
        /// Client should wait for UpdateSignalIndexCache and UpdateBaseTime response codes before attempting
        /// to parse data when using the compact measurement format.
        /// </remarks>
        Subscribe = 0x02,
        /// <summary>
        /// Unsubscribe command.
        /// </summary>
        /// <remarks>
        /// Requests that server stop sending streaming data to the client and cancel the current subscription.
        /// </remarks>
        Unsubscribe = 0x03,
        /// <summary>
        /// Rotate cipher keys.
        /// </summary>
        /// <remarks>
        /// Manually requests that server send a new set of cipher keys for data packet encryption.
        /// </remarks>
        RotateCipherKeys = 0x04,
        /// <summary>
        /// Update processing interval.
        /// </summary>
        /// <remarks>
        /// Manually requests server to update the processing interval with the following specified value.
        /// </remarks>
        UpdateProcessingInterval = 0x05,
        /// <summary>
        /// Define operational modes for subscriber connection.
        /// </summary>
        /// <remarks>
        /// As soon as connection is established, requests that server set operational
        /// modes that affect how the subscriber and publisher will communicate.
        /// </remarks>
        DefineOperationalModes = 0x06
    }

    /// <summary>
    /// Server responses sent by <see cref="DataPublisher"/> and received by <see cref="DataSubscriber"/>.
    /// </summary>
    public enum ServerResponse : byte
    {
        // Although the server commands and responses will be on two different paths, the response enumeration values
        // are defined as distinct from the command values to make it easier to identify codes from a wire analysis.

        /// <summary>
        /// Command succeeded response.
        /// </summary>
        /// <remarks>
        /// Informs client that its solicited server command succeeded, original command and success message follow.
        /// </remarks>
        Succeeded = 0x80,
        /// <summary>
        /// Command failed response.
        /// </summary>
        /// <remarks>
        /// Informs client that its solicited server command failed, original command and failure message follow.
        /// </remarks>
        Failed = 0x81,
        /// <summary>
        /// Data packet response.
        /// </summary>
        /// <remarks>
        /// Unsolicited response informs client that a data packet follows.
        /// </remarks>
        DataPacket = 0x82,
        /// <summary>
        /// Update signal index cache response.
        /// </summary>
        /// <remarks>
        /// Unsolicited response requests that client update its runtime signal index cache with the one that follows.
        /// </remarks>
        UpdateSignalIndexCache = 0x83,
        /// <summary>
        /// Update runtime base-timestamp offsets response.
        /// </summary>
        /// <remarks>
        /// Unsolicited response requests that client update its runtime base-timestamp offsets with those that follow.
        /// </remarks>
        UpdateBaseTimes = 0x84,
        /// <summary>
        /// Update runtime cipher keys response.
        /// </summary>
        /// <remarks>
        /// Response, solicited or unsolicited, requests that client update its runtime data cipher keys with those that follow.
        /// </remarks>
        UpdateCipherKeys = 0x85,
        /// <summary>
        /// Data start time response packet.
        /// </summary>
        /// <remarks>
        /// Unsolicited response provides the start time of data being processed from the first measurement.
        /// </remarks>
        DataStartTime = 0x86,
        /// <summary>
        /// Processing complete notification.
        /// </summary>
        /// <remarks>
        /// Unsolicited response provides notification that input processing has completed, typically via temporal constraint.
        /// </remarks>
        ProcessingComplete = 0x87,
        /// <summary>
        /// No operation keep-alive ping.
        /// </summary>
        /// <remarks>
        /// The command channel can remain quiet for some time, this command allows a period test of client connectivity.
        /// </remarks>
        NoOP = 0xFF
    }

    /// <summary>
    /// <see cref="DataPublisher"/> data packet flags.
    /// </summary>
    [Flags()]
    public enum DataPacketFlags : byte
    {
        /// <summary>
        /// Determines if data packet is synchronized.
        /// </summary>
        /// <remarks>
        /// Bit set = synchronized, bit clear = unsynchronized.
        /// </remarks>
        Synchronized = (byte)Bits.Bit00,
        /// <summary>
        /// Determines if serialized measurement is compact.
        /// </summary>
        /// <remarks>
        /// Bit set = compact, bit clear = full fidelity.
        /// </remarks>
        Compact = (byte)Bits.Bit01,
        /// <summary>
        /// Detemines which cipher index to use when encrypting data packet.
        /// </summary>
        /// <remarks>
        /// Bit set = use odd cipher index (i.e., 1), bit clear = use even cipher index (i.e., 0).
        /// </remarks>
        CipherIndex = (byte)Bits.Bit02,
        /// <summary>
        /// No flags set.
        /// </summary>
        /// <remarks>
        /// This would represent unsynchronized, full fidelity measurement data packets.
        /// </remarks>
        NoFlags = (Byte)Bits.Nil
    }

    /// <summary>
    /// Operational modes that affect how <see cref="DataPublisher"/> and <see cref="DataSubscriber"/> communicate.
    /// </summary>
    /// <remarks>
    /// Operational modes only apply to fundamental protocol control.
    /// </remarks>
    [Flags()]
    public enum OperationalModes : uint
    {
        /// <summary>
        /// Mask to get version number of protocol.
        /// </summary>
        /// <remarks>
        /// Version number is currently set to 0.
        /// </remarks>
        VersionMask = (uint)(Bits.Bit04 | Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// Mask to get mode of compression.
        /// </summary>
        /// <remarks>
        /// Compression mode is currently not supported.
        /// These bits are simply reserved for different compression modes.
        /// </remarks>
        CompressionModeMask = (uint)(Bits.Bit07 | Bits.Bit06 | Bits.Bit05),
        /// <summary>
        /// Mask to get character encoding used when exchanging messages between publisher and subscriber.
        /// </summary>
        /// <remarks>
        /// 00 = UTF-16 little endian<br/>
        /// 01 = UTF-16 big endian<br/>
        /// 10 = UTF-8<br/>
        /// 11 = ANSI
        /// </remarks>
        EncodingMask = (uint)(Bits.Bit09 | Bits.Bit08),
        /// <summary>
        /// Determines type of serialization to use when exchanging signal index cache and metadata.
        /// </summary>
        /// <remarks>
        /// Bit set = common serialization format, bit clear = .NET serialization format
        /// </remarks>
        UseCommonSerializationFormat = (uint)Bits.Bit24,
        /// <summary>
        /// Determines whether the signal index cache is compressed when exchanging between publisher and subscriber.
        /// </summary>
        /// <remarks>
        /// Bit set = compress, bit clear = no compression
        /// </remarks>
        CompressSignalIndexCache = (uint)Bits.Bit30,
        /// <summary>
        /// Determines whether is compressed when exchanging between publisher and subscriber.
        /// </summary>
        /// <remarks>
        /// Bit set = compress, bit clear = no compression
        /// </remarks>
        CompressMetadata = (uint)Bits.Bit31,
        /// <summary>
        /// No flags set.
        /// </summary>
        /// <remarks>
        /// This would represent protocol version 0,
        /// UTF-16 little endian character encoding,
        /// .NET serialization and no compression.
        /// </remarks>
        NoFlags = (uint)Bits.Nil
    }

    /// <summary>
    /// Enumeration for character encodings supported by the Gateway Exchange Protocol.
    /// </summary>
    public enum OperationalEncoding : uint
    {
        /// <summary>
        /// UTF-16 little endian
        /// </summary>
        Unicode = (uint)Bits.Nil,
        /// <summary>
        /// UTF-16 bit endian
        /// </summary>
        BigEndianUnicode = (uint)Bits.Bit08,
        /// <summary>
        /// UTF-8
        /// </summary>
        UTF8 = (uint)Bits.Bit09,
        /// <summary>
        /// ANSI
        /// </summary>
        ANSI = (uint)(Bits.Bit09 | Bits.Bit08)
    }

    /// <summary>
    /// Enumeration for compression modes supported by the Gateway Exchange Protocol.
    /// </summary>
    public enum GatewayCompressionMode : uint
    {
        /// <summary>
        /// GZip compression
        /// </summary>
        GZip = (uint)Bits.Bit05,
        /// <summary>
        /// No compression
        /// </summary>
        None = (uint)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Represents a data publishing server that allows multiple connections for data subscriptions.
    /// </summary>
    [Description("DataPublisher: server component that allows gateway-style subscription connections.")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public class DataPublisher : ActionAdapterCollection
    {
        #region [ Members ]

        // Nested Types

        private class LatestMeasurementCache : FacileActionAdapterBase
        {
            public LatestMeasurementCache(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public override string Name
            {
                get
                {
                    return "LatestMeasurementCache";
                }
                set
                {
                    base.Name = value;
                }
            }

            public override bool SupportsTemporalProcessing
            {
                get
                {
                    return false;
                }
            }

            public override string GetShortStatus(int maxLength)
            {
                return "LatestMeasurementCache happily exists. :)";
            }
        }

        // Events

        /// <summary>
        /// Indicates that a new client has connected to the publisher.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1, T2, T3}.Argument1"/> is the <see cref="Guid"/> based subscriber ID.<br/>
        /// <see cref="EventArgs{T1, T2, T3}.Argument2"/> is the connection identification (e.g., IP and DNS name, if available).<br/>
        /// <see cref="EventArgs{T1, T2, T3}.Argument3"/> is the subscriber information as reported by the client.
        /// </remarks>
        public event EventHandler<EventArgs<Guid, string, string>> ClientConnected;

        /// <summary>
        /// Indicates to the host that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal procesing.
        /// </remarks>
        public event EventHandler ProcessingComplete;

        // Constants

        /// <summary>
        /// Default value for <see cref="RequireAuthentication"/>.
        /// </summary>
        public const bool DefaultRequireAuthentication = false;

        /// <summary>
        /// Default value for <see cref="EncryptPayload"/>.
        /// </summary>
        public const bool DefaultEncryptPayload = false;

        /// <summary>
        /// Default value for <see cref="SharedDatabase"/>.
        /// </summary>
        public const bool DefaultSharedDatabase = false;

        /// <summary>
        /// Default value for <see cref="AllowSynchronizedSubscription"/>.
        /// </summary>
        public const bool DefaultAllowSynchronizedSubscription = true;

        /// <summary>
        /// Default value for <see cref="UseBaseTimeOffsets"/>.
        /// </summary>
        public const bool DefaultUseBaseTimeOffsets = false;

        /// <summary>
        /// Default value for <see cref="CipherKeyRotationPeriod"/>.
        /// </summary>
        public const double DefaultCipherKeyRotationPeriod = 60000.0D;

        // Maximum packet size before software fragmentation of UDP payload
        internal const int MaxPacketSize = ushort.MaxValue / 2;

        // Length of random salt prefix
        internal const int CipherSaltLength = 8;

        // Fields
        private ManualResetEvent m_initializeWaitHandle;
        private TcpServer m_commandChannel;
        private ConcurrentDictionary<Guid, ClientConnection> m_clientConnections;
        private ConcurrentDictionary<Guid, IServer> m_clientPublicationChannels;
        private ConcurrentDictionary<MeasurementKey, Guid> m_signalIDCache;
        private System.Timers.Timer m_commandChannelRestartTimer;
        private System.Timers.Timer m_cipherKeyRotationTimer;
        private RoutingTables m_routingTables;
        private IAdapterCollection m_parent;
        private string m_metadataTables;
        private bool m_requireAuthentication;
        private bool m_encryptPayload;
        private bool m_sharedDatabase;
        private bool m_allowSynchronizedSubscription;
        private bool m_useBaseTimeOffsets;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataPublisher"/>.
        /// </summary>
        public DataPublisher()
            : this(null)
        {
            // When collection is spawned as an adapter, it needs a parameterless constructor
        }

        /// <summary>
        /// Creates a new <see cref="DataPublisher"/>.
        /// </summary>
        /// <param name="waitHandles">Wait handle dictionary.</param>
        public DataPublisher(ConcurrentDictionary<string, AutoResetEvent> waitHandles)
            : base(waitHandles)
        {
            base.Name = "Data Publisher Collection";
            base.DataMember = "[internal]";

            m_initializeWaitHandle = new ManualResetEvent(false);
            m_clientConnections = new ConcurrentDictionary<Guid, ClientConnection>();
            m_clientPublicationChannels = new ConcurrentDictionary<Guid, IServer>();
            m_signalIDCache = new ConcurrentDictionary<MeasurementKey, Guid>();
            m_requireAuthentication = DefaultRequireAuthentication;
            m_encryptPayload = DefaultEncryptPayload;
            m_sharedDatabase = DefaultSharedDatabase;
            m_allowSynchronizedSubscription = DefaultAllowSynchronizedSubscription;
            m_useBaseTimeOffsets = DefaultUseBaseTimeOffsets;

            m_metadataTables =
                "SELECT NodeID, UniqueID, OriginalSource, IsConcentrator, Acronym, Name, ParentAcronym, ProtocolName, FramesPerSecond, Enabled FROM DeviceDetail WHERE OriginalSource IS NULL AND IsConcentrator = 0;" +
                "SELECT Internal, DeviceAcronym, DeviceName, SignalAcronym, ID, SignalID, PointTag, SignalReference, Description, Enabled FROM MeasurementDetail WHERE Internal <> 0;" +
                "SELECT DeviceAcronym, Label, Type, Phase, SourceIndex FROM PhasorDetail";

            m_routingTables = new RoutingTables()
            {
                ActionAdapters = this
            };
            m_routingTables.ProcessException += m_routingTables_ProcessException;

            // Setup a timer for restarting the command channel if it fails
            m_commandChannelRestartTimer = new System.Timers.Timer(2000.0D);
            m_commandChannelRestartTimer.AutoReset = false;
            m_commandChannelRestartTimer.Enabled = false;
            m_commandChannelRestartTimer.Elapsed += m_commandChannelRestartTimer_Elapsed;

            // Setup a timer for rotating cipher keys
            m_cipherKeyRotationTimer = new System.Timers.Timer(DefaultCipherKeyRotationPeriod);
            m_cipherKeyRotationTimer.AutoReset = true;
            m_cipherKeyRotationTimer.Enabled = false;
            m_cipherKeyRotationTimer.Elapsed += m_cipherKeyRotationTimer_Elapsed;
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
        [ConnectionStringParameter,
        Description("Define the flag that determines if the publisher should require subscribers to authenticate before making data requests."),
        DefaultValue(DefaultRequireAuthentication)]
        public bool RequireAuthentication
        {
            get
            {
                return m_requireAuthentication;
            }
            set
            {
                m_requireAuthentication = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether data sent over the data channel should be encrypted.
        /// </summary>
        /// <remarks>
        /// This value is only relevant if <see cref="RequireAuthentication"/> is true.
        /// </remarks>
        [ConnectionStringParameter,
        Description("Define the flag that determines whether data sent over the data channel should be encrypted. This value is only relevant when requireAuthentication is true."),
        DefaultValue(DefaultEncryptPayload)]
        public bool EncryptPayload
        {
            get
            {
                return m_encryptPayload;
            }
            set
            {
                m_encryptPayload = value;

                // Start cipher key rotation timer when encrypting payload
                if (m_cipherKeyRotationTimer != null)
                    m_cipherKeyRotationTimer.Enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that indicates whether this publisher is publishing
        /// data that this node subscribed to from another node in a shared database.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag that indicates whether this publisher is publishing data that this node subscribed to from another node in a shared database."),
        DefaultValue(DefaultSharedDatabase)]
        public bool SharedDatabase
        {
            get
            {
                return m_sharedDatabase;
            }
            set
            {
                m_sharedDatabase = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that indicates if this publisher will allow synchronized subscriptions.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag that indicates if this publisher will allow synchronized subscriptions."),
        DefaultValue(DefaultAllowSynchronizedSubscription)]
        public bool AllowSynchronizedSubscription
        {
            get
            {
                return m_allowSynchronizedSubscription;
            }
            set
            {
                m_allowSynchronizedSubscription = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether to use base time offsets to decrease the size of compact measurements.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag that determines whether to use base time offsets to decrease the size of compact measurements."),
        DefaultValue(DefaultUseBaseTimeOffsets)]
        public bool UseBaseTimeOffsets
        {
            get
            {
                return m_useBaseTimeOffsets;
            }
            set
            {
                m_useBaseTimeOffsets = value;
            }
        }

        /// <summary>
        /// Gets or sets the cipher key rotation period.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the period, in milliseconds, over which new cipher keys will be provided to subscribers when EncryptPayload is true."),
        DefaultValue(DefaultCipherKeyRotationPeriod)]
        public double CipherKeyRotationPeriod
        {
            get
            {
                if (m_cipherKeyRotationTimer != null)
                    return m_cipherKeyRotationTimer.Interval;

                return double.NaN;
            }
            set
            {
                if (value < 1000.0D)
                    throw new ArgumentOutOfRangeException("value", "Cipher key rotation period should not be set to less than 1000 milliseconds.");

                if (m_cipherKeyRotationTimer != null)
                    m_cipherKeyRotationTimer.Interval = value;

                throw new ArgumentException("Cannot assign new cipher rotation period, timer is not defined.");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>.
        /// Updates to this property will cascade to all items in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
                UpdateRights();
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
                    m_commandChannel.SettingsCategory = value.Replace("!", "").ToLower();
            }
        }

        /// <summary>
        /// Gets or sets comma seperated list of tables to include in meta-data exchange.
        /// </summary>
        public string MetadataTables
        {
            get
            {
                return m_metadataTables;
            }
            set
            {
                m_metadataTables = value;
            }
        }

        /// <summary>
        /// Gets dictionary of connected clients.
        /// </summary>
        internal protected ConcurrentDictionary<Guid, ClientConnection> ClientConnections
        {
            get
            {
                return m_clientConnections;
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
                    m_commandChannel.ReceiveClientDataComplete -= m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.ReceiveClientDataException -= m_commandChannel_ReceiveClientDataException;
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
                    m_commandChannel.ReceiveClientDataComplete += m_commandChannel_ReceiveClientDataComplete;
                    m_commandChannel.ReceiveClientDataException += m_commandChannel_ReceiveClientDataException;
                    m_commandChannel.SendClientDataException += m_commandChannel_SendClientDataException;
                    m_commandChannel.ServerStarted += m_commandChannel_ServerStarted;
                    m_commandChannel.ServerStopped += m_commandChannel_ServerStopped;
                }
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the adapter has been initialized successfully.
        /// </summary>
        public override bool Initialized
        {
            get
            {
                return base.Initialized;
            }
            set
            {
                base.Initialized = value;

                if ((object)m_initializeWaitHandle != null)
                {
                    if (value)
                        m_initializeWaitHandle.Set();
                    else
                        m_initializeWaitHandle.Reset();
                }
            }
        }

        /// <summary>
        /// Gets flag indicating if publisher is connected and listening.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return m_commandChannel.Enabled;
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

                        if (m_clientConnections != null)
                        {
                            foreach (ClientConnection connection in m_clientConnections.Values)
                            {
                                connection.Dispose();
                            }
                        }

                        m_clientConnections = null;

                        if (m_routingTables != null)
                        {
                            m_routingTables.ProcessException -= m_routingTables_ProcessException;
                            m_routingTables.Dispose();
                        }
                        m_routingTables = null;

                        if (m_initializeWaitHandle != null)
                            m_initializeWaitHandle.Close();

                        m_initializeWaitHandle = null;

                        // Dispose command channel restart timer
                        if (m_commandChannelRestartTimer != null)
                        {
                            m_commandChannelRestartTimer.Elapsed -= m_commandChannelRestartTimer_Elapsed;
                            m_commandChannelRestartTimer.Dispose();
                        }
                        m_commandChannelRestartTimer = null;

                        // Dispose the cipher key rotation timer
                        if (m_cipherKeyRotationTimer != null)
                        {
                            m_cipherKeyRotationTimer.Elapsed -= m_cipherKeyRotationTimer_Elapsed;
                            m_cipherKeyRotationTimer.Dispose();
                        }
                        m_cipherKeyRotationTimer = null;
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
            double period;

            // Setup data publishing server with or without required authentication 
            if (settings.TryGetValue("requireAuthentication", out setting))
                m_requireAuthentication = setting.ParseBoolean();

            // Check flag that will determine if subsciber payloads should be encrypted by default
            if (settings.TryGetValue("encryptPayload", out setting))
                m_encryptPayload = setting.ParseBoolean();

            // Check flag that indicates whether publisher is publishing data
            // that its node subscribed to from another node in a shared database
            if (settings.TryGetValue("sharedDatabase", out setting))
                m_sharedDatabase = setting.ParseBoolean();

            // Check flag to see if synchronized subscriptions are allowed
            if (settings.TryGetValue("allowSynchronizedSubscription", out setting))
                m_allowSynchronizedSubscription = setting.ParseBoolean();

            if (settings.TryGetValue("useBaseTimeOffsets", out setting))
                m_useBaseTimeOffsets = setting.ParseBoolean();

            // Get user specified period for cipher key rotation
            if (settings.TryGetValue("cipherKeyRotationPeriod", out setting) && double.TryParse(setting, out period))
                CipherKeyRotationPeriod = period;

            if (settings.TryGetValue("cacheMeasurementKeys", out setting))
            {
                LatestMeasurementCache cache = new LatestMeasurementCache(string.Format("trackLatestMeasurements=true;lagTime=60;leadTime=60;inputMeasurementKeys={{{0}}}", setting));
                cache.DataSource = DataSource;
                Add(cache);
                m_routingTables.CalculateRoutingTables(null);
                cache.Start();
            }

            // Create a new TCP server
            TcpServer commandChannel = new TcpServer();

            // Initialize default settings
            commandChannel.SettingsCategory = Name.Replace("!", "").ToLower();
            commandChannel.ConfigurationString = "port=6165";
            commandChannel.PayloadAware = true;
            commandChannel.PersistSettings = true;

            // Assign command channel client reference and attach to needed events
            this.CommandChannel = commandChannel;

            // Initialize TCP server (loads config file settings)
            commandChannel.Initialize();

            // Start cipher key rotation timer when encrypting payload
            if (m_encryptPayload && m_cipherKeyRotationTimer != null)
                m_cipherKeyRotationTimer.Start();

            // Register publisher with the statistics engine
            //StatisticsEngine.Register(this, "Publisher", "PUB");

            Initialized = true;
        }

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IActionAdapter"/> connected to this <see cref="DataPublisher"/>.
        /// </summary>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (this.ProcessMeasurementFilter)
                base.QueueMeasurementsForProcessing(measurements);
            else
                m_routingTables.RoutedMeasurementsHandler(measurements);
        }

        /// <summary>
        /// Establish <see cref="DataPublisher"/> and start listening for client connections.
        /// </summary>
        public override void Start()
        {
            if (!WaitForInitialize(InitializationTimeout))
            {
                OnProcessException(new TimeoutException("Failed to start adapter due to timeout waiting for initialization."));
                return;
            }

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
        /// Assigns the reference to the parent <see cref="IAdapterCollection"/> that will contain this <see cref="DataPublisher"/>, if any.
        /// </summary>
        /// <param name="parent">Parent adapter collection.</param>
        protected override void AssignParentCollection(IAdapterCollection parent)
        {
            // Get a local reference to the parent collection
            m_parent = parent;

            // Pass reference along to base class
            base.AssignParentCollection(parent);
        }

        /// <summary>
        /// Gets a common wait handle for inter-adapter synchronization.
        /// </summary>
        /// <param name="name">Case-insensitive wait handle name.</param>
        /// <returns>A <see cref="AutoResetEvent"/> based wait handle associated with the given <paramref name="name"/>.</returns>
        public override AutoResetEvent GetExternalEventHandle(string name)
        {
            // Since this collection can act as an adapter, proxy event handle request to its parent collection when defined
            if (m_parent != null)
                return m_parent.GetExternalEventHandle(name);

            // Otherwise just handle the request normally
            return base.GetExternalEventHandle(name);
        }

        /// <summary>
        /// Blocks the <see cref="Thread.CurrentThread"/> until the adapter is <see cref="Initialized"/>.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait.</param>
        /// <returns><c>true</c> if the initialization succeeds; otherwise, <c>false</c>.</returns>
        public override bool WaitForInitialize(int timeout)
        {
            if ((object)m_initializeWaitHandle != null)
                return m_initializeWaitHandle.WaitOne(timeout);

            return false;
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

        /// <summary>
        /// Enumerates connected clients.
        /// </summary>
        [AdapterCommand("Enumerates connected clients.")]
        public virtual void EnumerateClients()
        {
            StringBuilder clientEnumeration = new StringBuilder();
            Guid[] clientIDs = (Guid[])m_commandChannel.ClientIDs.Clone();
            ClientConnection connection;
            string timestampFormat;

            clientEnumeration.AppendFormat("\r\nIndices for {0} connected clients:\r\n\r\n", clientIDs.Length);

            for (int i = 0; i < clientIDs.Length; i++)
            {
                if (m_clientConnections.TryGetValue(clientIDs[i], out connection) && (object)connection != null && (object)connection.Subscription != null)
                {
                    if (connection.Subscription is UnsynchronizedClientSubscription)
                        timestampFormat = string.Format("{0} Format, {1}-byte timestamps", connection.Subscription.UseCompactMeasurementFormat ? "Compact" : "Full", connection.Subscription.TimestampSize);
                    else
                        timestampFormat = string.Format("{0} Format, 8-byte frame-level timestamp", connection.Subscription.UseCompactMeasurementFormat ? "Compact" : "Full");

                    clientEnumeration.AppendFormat("  {0} - {1}\r\n          {2}\r\n          {3}\r\n          {4}\r\n\r\n", i.ToString().PadLeft(3), connection.ConnectionID, connection.SubscriberInfo, timestampFormat, connection.OperationalModes);
                }
            }

            // Display enumeration
            OnStatusMessage(clientEnumeration.ToString());
        }

        /// <summary>
        /// Rotates cipher keys for specified client connection.
        /// </summary>
        /// <param name="clientIndex">Enumerated index for client connection.</param>
        [AdapterCommand("Rotates cipher keys for client connection using its enumerated index.")]
        public virtual void RotateCipherKeys(int clientIndex)
        {
            Guid clientID = Guid.Empty;
            bool success = true;

            try
            {
                clientID = m_commandChannel.ClientIDs[clientIndex];
            }
            catch
            {
                success = false;
                OnStatusMessage("ERROR: Failed to find connected client with enumerated index " + clientIndex);
            }

            if (success)
            {
                ClientConnection connection;

                if (m_clientConnections.TryGetValue(clientID, out connection))
                    connection.RotateCipherKeys();
                else
                    OnStatusMessage("ERROR: Failed to find connected client " + clientID);
            }
        }

        /// <summary>
        /// Gets subscriber information for specified client connection.
        /// </summary>
        /// <param name="clientIndex">Enumerated index for client connection.</param>
        [AdapterCommand("Gets subscriber information for client connection using its enumerated index.")]
        public virtual string GetSubscriberInfo(int clientIndex)
        {
            Guid clientID = Guid.Empty;
            bool success = true;

            try
            {
                clientID = m_commandChannel.ClientIDs[clientIndex];
            }
            catch
            {
                success = false;
                OnStatusMessage("ERROR: Failed to find connected client with enumerated index " + clientIndex);
            }

            if (success)
            {
                ClientConnection connection;

                if (m_clientConnections.TryGetValue(clientID, out connection))
                    return connection.SubscriberInfo;

                OnStatusMessage("ERROR: Failed to find connected client " + clientID);
            }

            return "";
        }

        /// <summary>
        /// Gets subscriber status for specified subscriber ID.
        /// </summary>
        /// <param name="subscriberID">Guid based subscriber ID for client connection.</param>
        [AdapterCommand("Gets subscriber status for client connection using its subscriber ID.")]
        public virtual Tuple<Guid, bool, string> GetSubscriberStatus(Guid subscriberID)
        {
            return new Tuple<Guid, bool, string>(subscriberID, GetConnectionProperty(subscriberID, cc => cc.IsConnected), GetConnectionProperty(subscriberID, cc => cc.SubscriberInfo));
        }

        /// <summary>
        /// Updates signal index cache based on input measurement keys.
        /// </summary>
        /// <param name="clientID">Client ID of connection over which to update signal index cache.</param>
        /// <param name="signalIndexCache">New signal index cache.</param>
        /// <param name="inputMeasurementKeys">Subscribed measurement keys.</param>
        public void UpdateSignalIndexCache(Guid clientID, SignalIndexCache signalIndexCache, MeasurementKey[] inputMeasurementKeys)
        {
            ConcurrentDictionary<ushort, Tuple<Guid, string, uint>> reference = new ConcurrentDictionary<ushort, Tuple<Guid, string, uint>>();
            List<Guid> unauthorizedKeys = new List<Guid>();
            ushort index = 0;
            Guid signalID;

            byte[] serializedSignalIndexCache;
            ClientConnection connection;

            if (inputMeasurementKeys != null)
            {
                // We will now go through the client's requested keys and see which ones are authorized for subscription,
                // this information will be available through the returned signal index cache which will also define
                // a runtime index optimization for the allowed measurements.
                foreach (MeasurementKey key in inputMeasurementKeys)
                {
                    if (m_requireAuthentication)
                    {
                        // Validate that subscriber has rights to this signal
                        if (SubscriberHasRights(signalIndexCache.SubscriberID, key, out signalID))
                            reference.TryAdd(index++, new Tuple<Guid, string, uint>(signalID, key.Source, key.ID));
                        else
                            unauthorizedKeys.Add(key.SignalID);
                    }
                    else
                    {
                        // When client authorization is not required, all points are assumed to be allowed
                        reference.TryAdd(index++, new Tuple<Guid, string, uint>(LookupSignalID(key), key.Source, key.ID));
                    }
                }
            }

            signalIndexCache.Reference = reference;
            signalIndexCache.UnauthorizedSignalIDs = unauthorizedKeys.ToArray();
            serializedSignalIndexCache = SerializeSignalIndexCache(clientID, signalIndexCache);

            // Send client updated signal index cache
            if (m_clientConnections.TryGetValue(clientID, out connection) && connection.IsSubscribed)
                SendClientResponse(clientID, ServerResponse.UpdateSignalIndexCache, ServerCommand.Subscribe, serializedSignalIndexCache);
        }

        /// <summary>
        /// Updates each subscription's inputs based on
        /// possible updates to that subscriber's rights.
        /// </summary>
        protected void UpdateRights()
        {
            IClientSubscription subscription;

            // If authentication is not required,
            // rights are not applicable
            if (!m_requireAuthentication)
                return;

            lock (this)
            {
                foreach (IAdapter adapter in this)
                {
                    subscription = adapter as IClientSubscription;

                    if ((object)subscription != null)
                        UpdateRights(subscription);
                }
            }

            m_routingTables.CalculateRoutingTables(null);
        }

        /// <summary>
        /// Determines if subscriber has rights to specified <paramref name="signalID"/>.
        /// </summary>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
        /// <returns><c>true</c> if subscriber has rights to specified <see cref="MeasurementKey"/>; otherwise <c>false</c>.</returns>
        protected bool SubscriberHasRights(Guid subscriberID, Guid signalID)
        {
            try
            {
                // Lookup explicitly defined individual measurements
                DataRow[] explicitMeasurement = DataSource.Tables["SubscriberMeasurements"].Select(string.Format("SubscriberID='{0}' AND SignalID='{1}'", subscriberID, signalID));

                if (explicitMeasurement.Length > 0)
                    return explicitMeasurement[0]["Allowed"].ToNonNullString("0").ParseBoolean();

                // Lookup implicitly defined group based measurements
                DataRow[] implicitMeasurements;

                foreach (DataRow subscriberMeasurementGroup in DataSource.Tables["SubscriberMeasurementGroups"].Select(string.Format("SubscriberID='{0}'", subscriberID)))
                {
                    implicitMeasurements = DataSource.Tables["MeasurementGroupMeasurements"].Select(string.Format("SignalID='{0}' AND MeasurementGroupID={1}", signalID, int.Parse(subscriberMeasurementGroup["MeasurementGroupID"].ToNonNullString("0"))));

                    if (implicitMeasurements.Length > 0)
                        return subscriberMeasurementGroup["Allowed"].ToNonNullString("0").ParseBoolean();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to determine subscriber rights for {0} due to exception: {1}", GetConnectionProperty(subscriberID, cc => cc.SubscriberAcronym), ex.Message)));
            }

            return false;
        }

        /// <summary>
        /// Determines if subscriber has rights to specified <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="subscriberID"><see cref="Guid"/> based subscriber ID.</param>
        /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
        /// <param name="signalID"><see cref="Guid"/> signal ID if found; otherwise an empty Guid.</param>
        /// <returns><c>true</c> if subscriber has rights to specified <see cref="MeasurementKey"/>; otherwise <c>false</c>.</returns>
        protected bool SubscriberHasRights(Guid subscriberID, MeasurementKey key, out Guid signalID)
        {
            signalID = LookupSignalID(key);

            if (signalID != Guid.Empty)
                return SubscriberHasRights(subscriberID, signalID);

            return false;
        }

        /// <summary>
        /// Looks up <see cref="Guid"/> signal ID for given <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
        /// <returns><see cref="Guid"/> signal ID if found; otherwise an empty Guid.</returns>
        protected Guid LookupSignalID(MeasurementKey key)
        {
            // Attempt to lookup measurement key's signal ID that may have already been cached
            return m_signalIDCache.GetOrAdd(key, keyParam =>
            {
                Guid signalID = Guid.Empty;

                if (DataSource != null && DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    // Attempt to lookup input measurement keys for given source IDs from default measurement table, if defined
                    try
                    {
                        DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("ID='" + keyParam.ToString() + "'");

                        if (filteredRows.Length > 0)
                            signalID = new Guid(filteredRows[0]["SignalID"].ToString());
                    }
                    catch
                    {
                        // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys based on specified source ID's
                    }
                }

                if (signalID == Guid.Empty)
                    signalID = key.SignalID;

                return signalID;
            });
        }

        /// <summary>
        /// Gets the text encoding associated with a particular client.
        /// </summary>
        /// <param name="clientID">ID of client.</param>
        /// <returns>Text encoding associated with a particular client.</returns>
        protected internal Encoding GetClientEncoding(Guid clientID)
        {
            ClientConnection connection;

            if (m_clientConnections.TryGetValue(clientID, out connection))
            {
                Encoding clientEncoding = connection.Encoding;

                if ((object)clientEncoding != null)
                    return clientEncoding;
            }

            // Default to unicode
            return Encoding.Unicode;
        }

        /// <summary>
        /// Sends the start time of the first measurement in a connection transmission.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="startTime">Start time, in <see cref="Ticks"/>, of first measurement transmitted.</param>
        internal protected virtual bool SendDataStartTime(Guid clientID, Ticks startTime)
        {
            bool result = SendClientResponse(clientID, ServerResponse.DataStartTime, ServerCommand.Subscribe, EndianOrder.BigEndian.GetBytes((long)startTime));

            ClientConnection connection;

            if (m_clientConnections.TryGetValue(clientID, out connection))
                OnStatusMessage("Start time sent to {0}.", connection.ConnectionID);

            return result;
        }

        // Handle input processing complete notifications

        /// <summary>
        /// Sends response back to specified client.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="response">Server response.</param>
        /// <param name="command">In response to command.</param>
        /// <returns><c>true</c> if send was successful; otherwise <c>false</c>.</returns>
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command)
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
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, string status)
        {
            if (status != null)
                return SendClientResponse(clientID, response, command, GetClientEncoding(clientID).GetBytes(status));

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
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, string formattedStatus, params object[] args)
        {
            if (!formattedStatus.IsNullOrWhiteSpace())
                return SendClientResponse(clientID, response, command, GetClientEncoding(clientID).GetBytes(string.Format(formattedStatus, args)));

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
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, byte[] data)
        {
            return SendClientResponse(clientID, (byte)response, (byte)command, data);
        }

        // Update rights for the given subscription.
        private void UpdateRights(IClientSubscription subscription)
        {
            MeasurementKey[] requestedInputs = AdapterBase.ParseInputMeasurementKeys(DataSource, subscription.RequestedInputFilter);
            HashSet<MeasurementKey> authorizedSignals = new HashSet<MeasurementKey>();
            Guid subscriberID = subscription.SubscriberID;
            string message;

            foreach (MeasurementKey input in requestedInputs)
            {
                if (SubscriberHasRights(subscriberID, input.SignalID))
                    authorizedSignals.Add(input);
            }

            if (!authorizedSignals.SetEquals(subscription.InputMeasurementKeys))
            {
                message = string.Format("Update to authorized signals caused subscription to change. Now subscribed to {0} signals.", authorizedSignals.Count);
                subscription.InputMeasurementKeys = authorizedSignals.ToArray();
                SendClientResponse(subscription.ClientID, ServerResponse.Succeeded, ServerCommand.Subscribe, message);
            }
        }

        // Send binary response packet to client
        private bool SendClientResponse(Guid clientID, byte responseCode, byte commandCode, byte[] data)
        {
            ClientConnection connection = null;
            bool success = false;

            // Attempt to lookup associated client connection
            if (m_clientConnections.TryGetValue(clientID, out connection) && (object)connection != null)
            {
                try
                {
                    MemoryStream responsePacket = new MemoryStream();
                    bool dataPacketResponse = responseCode == (byte)ServerResponse.DataPacket;

                    // Add response code
                    responsePacket.WriteByte(responseCode);

                    // Add original in response to command code
                    responsePacket.WriteByte(commandCode);

                    if ((object)data == null || data.Length == 0)
                    {
                        // Add zero sized data buffer to response packet
                        responsePacket.Write(s_zeroLengthBytes, 0, 4);
                    }
                    else
                    {
                        // If response is for a data packet and a connection key is defined, encrypt the data packet payload
                        if (dataPacketResponse && (object)connection.KeyIVs != null)
                        {
//                            // Get a local copy of volatile keyIVs and cipher index since these can change at any time
//                            byte[][][] keyIVs = connection.KeyIVs;
//                            int cipherIndex = connection.CipherIndex;
//
//                            // Reserve space for size of data buffer to go into response packet
//                            responsePacket.Write(s_zeroLengthBytes, 0, 4);
//
//                            // Get data packet flags
//                            DataPacketFlags flags = (DataPacketFlags)data[0];
//
//                            // Encode current cipher index into data packet flags
//                            if (cipherIndex > 0)
//                                flags |= DataPacketFlags.CipherIndex;
//
//                            // Write data packet flags into response packet
//                            responsePacket.WriteByte((byte)flags);
//
//                            // Copy source data payload into a memory stream
//                            MemoryStream sourceData = new MemoryStream(data, 1, data.Length - 1);
//
//                            // Encrypt payload portion of data packet and copy into the response packet
//                            Common.SymmetricAlgorithm.Encrypt(sourceData, responsePacket, keyIVs[cipherIndex][0], keyIVs[cipherIndex][1]);
//
//                            // Calculate length of encrypted data payload
//                            int payloadLength = (int)responsePacket.Length - 6;
//
//                            // Move the response packet position back to the packet size reservation
//                            responsePacket.Seek(2, SeekOrigin.Begin);
//
//                            // Add the actual size of payload length to response packet
//                            responsePacket.Write(EndianOrder.BigEndian.GetBytes(payloadLength), 0, 4);
                        }
                        else
                        {
                            // Add size of data buffer to response packet
                            responsePacket.Write(EndianOrder.BigEndian.GetBytes(data.Length), 0, 4);

                            // Add data buffer
                            responsePacket.Write(data, 0, data.Length);
                        }
                    }

                    IServer publishChannel;

                    // Data packets can be published on a UDP data channel, so check for this...
                    if (dataPacketResponse)
                        publishChannel = m_clientPublicationChannels.GetOrAdd(clientID, id => connection == null ? m_commandChannel : connection.PublishChannel);
                    else
                        publishChannel = m_commandChannel;

                    // Send response packet
                    if ((object)publishChannel != null && publishChannel.CurrentState == ServerState.Running)
                    {
                        byte[] responseData = responsePacket.ToArray();

                        if (publishChannel is UdpServer)
                            publishChannel.MulticastAsync(responseData, 0, responseData.Length);
                        else
                            publishChannel.SendToAsync(clientID, responseData, 0, responseData.Length);

                        success = true;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // This happens when there is still data to be sent to a disconnected client - we can safely ignore this exception
                }
                catch (NullReferenceException)
                {
                    // This happens when there is still data to be sent to a disconnected client - we can safely ignore this exception
                }
                catch (SocketException ex)
                {
                    if (!HandleSocketException(clientID, ex))
                        OnProcessException(new InvalidOperationException("Failed to send response packet to client due to exception: " + ex.Message, ex));
                }
                catch (InvalidOperationException ex)
                {
                    // Could still be processing threads with client data after client has been disconnected, this can be safely ignored
                    if (!ex.Message.StartsWith("No client found"))
                        OnProcessException(new InvalidOperationException("Failed to send response packet to client due to exception: " + ex.Message, ex));
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to send response packet to client due to exception: " + ex.Message, ex));
                }
            }

            return success;
        }

        // Socket exception handler
        private bool HandleSocketException(Guid clientID, SocketException ex)
        {
            if ((object)ex != null)
            {
                // WSAECONNABORTED and WSAECONNRESET are common errors after a client disconnect,
                // if they happen for other reasons, make sure disconnect procedure is handled
                if (ex.ErrorCode == 10053 || ex.ErrorCode == 10054)
                {
                    try
                    {
                        ThreadPool.QueueUserWorkItem(DisconnectClient, clientID);
                    }
                    catch (Exception queueException)
                    {
                        // Process exception for logging
                        OnProcessException(new InvalidOperationException("Failed to queue client disconnect due to exception: " + queueException.Message, queueException));
                    }

                    return true;
                }
            }

            return false;
        }

        // Disconnect client - this should be called from non-blocking thread (e.g., thread pool)
        private void DisconnectClient(object state)
        {
            try
            {
                Guid clientID = (Guid)state;
                ClientConnection connection;
                IServer publicationChannel;

                RemoveClientSubscription(clientID);

                if (m_clientConnections.TryRemove(clientID, out connection))
                {
                    connection.Dispose();
                    OnStatusMessage("Client disconnected from command channel.");
                }

                m_clientPublicationChannels.TryRemove(clientID, out publicationChannel);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Encountered an exception while processing client disconnect: {0}", ex.Message), ex));
            }
        }

        // Remove client subscription
        private void RemoveClientSubscription(Guid clientID)
        {
            lock (this)
            {
                IClientSubscription clientSubscription;

                if (TryGetClientSubscription(clientID, out clientSubscription))
                {
                    Remove(clientSubscription);
                    clientSubscription.Stop();

                    try
                    {
                        // Notify system that subscriber disconnected therefore demanded measurements may have changed
                        ThreadPool.QueueUserWorkItem(NotifyHostOfSubscriptionRemoval);
                    }
                    catch (Exception ex)
                    {
                        // Process exception for logging
                        OnProcessException(new InvalidOperationException("Failed to queue notification of subscription removal due to exception: " + ex.Message, ex));
                    }
                }
            }
        }

        // Handle notfication on input measurement key change
        private void NotifyHostOfSubscriptionRemoval(object state)
        {
            OnInputMeasurementKeysUpdated();
        }

        // Attempt to find client subscription
        private bool TryGetClientSubscription(Guid clientID, out IClientSubscription subscription)
        {
            IActionAdapter adapter;

            // Lookup adapter by its client ID
            if (TryGetAdapter<Guid>(clientID, GetClientSubscription, out adapter))
            {
                subscription = (IClientSubscription)adapter;
                return true;
            }

            subscription = null;
            return false;
        }

        private bool GetClientSubscription(IActionAdapter item, Guid value)
        {
            IClientSubscription subscription = item as IClientSubscription;

            if ((object)subscription != null)
                return subscription.ClientID == value;

            return false;
        }

        // Gets specfied property from client connection based on subscriber ID
        private TResult GetConnectionProperty<TResult>(Guid subscriberID, Func<ClientConnection, TResult> predicate)
        {
            TResult result = default(TResult);

            // Lookup client connection by subscriber ID
            ClientConnection connection = m_clientConnections.Values.FirstOrDefault(cc => cc.SubscriberID == subscriberID);

            // Extract desired property from client connection using given predicate function
            if ((object)connection != null)
                result = predicate(connection);

            return result;
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        protected virtual void OnProcessingComplete()
        {
            try
            {
                if ((object)ProcessingComplete != null)
                    ProcessingComplete(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ProcessingComplete event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="subscriberID">Subscriber <see cref="Guid"/> (normally <see cref="ClientConnection.SubscriberID"/>).</param>
        /// <param name="connectionID">Connection identification (normally <see cref="ClientConnection.ConnectionID"/>).</param>
        /// <param name="subscriberInfo">Subscriber information (normally <see cref="ClientConnection.SubscriberInfo"/>).</param>
        protected virtual void OnClientConnected(Guid subscriberID, string connectionID, string subscriberInfo)
        {
            try
            {
                if ((object)ClientConnected != null)
                    ClientConnected(this, new EventArgs<Guid, string, string>(subscriberID, connectionID, subscriberInfo));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ClientConnected event: {0}", ex.Message), ex));
            }
        }

        // Make sure to expose any routing table exceptions
        private void m_routingTables_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        // Cipher key rotation timer handler
        private void m_cipherKeyRotationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((object)m_clientConnections != null)
            {
                foreach (ClientConnection connection in m_clientConnections.Values)
                {
                    if (connection != null && connection.Authenticated)
                        connection.RotateCipherKeys();
                }
            }
        }

        // Command channel restart timer handler
        private void m_commandChannelRestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((object)m_commandChannel != null)
            {
                try
                {
                    // After a short delay, we try to restart the command channel
                    m_commandChannel.Start();
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to restart data publisher command channel: " + ex.Message, ex));
                }
            }
        }

        #region [ Server Command Request Handlers ]

        // Handles authentication request
        private void HandleAuthenticationRequest(ClientConnection connection, byte[] buffer, int startIndex, int length)
        {
//            Guid clientID = connection.ClientID;
//            string message;
//
//            // Handle authentication request
//            try
//            {
//                DataRow subscriber = null;
//
//                // Reset existing authentication state
//                connection.Authenticated = false;
//
//                // Subscriber connection is first referenced by its IP
//                foreach (DataRow row in DataSource.Tables["Subscribers"].Select("Enabled <> 0"))
//                {
//                    IEnumerable<IPAddress> ipAddresses = row["ValidIPAddresses"].ToNonNullString().Split(';', ',').Where(ip => !ip.IsNullOrWhiteSpace()).Select(ip => IPAddress.Parse(ip.Trim()));
//                    foreach (IPAddress ipAddress in ipAddresses)
//                    {
//                        if (connection.IPAddress.ToString().Contains(ipAddress.ToString()))
//                        {
//                            subscriber = row;
//                            break;
//                        }
//                    }
//
//                    if (subscriber != null)
//                        break;
//                }
//
//                if (subscriber == null)
//                {
//                    message = string.Format("No subscriber is registered for {0}, cannnot authenticate connection - {1} request denied.", connection.ConnectionID, ServerCommand.Authenticate);
//                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                    OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber is disabled or not registered.", connection.ConnectionID, ServerCommand.Authenticate);
//                    return;
//                }
//                else
//                {
//                    string sharedSecret = subscriber["SharedSecret"].ToNonNullString().Trim();
//                    string authenticationID = subscriber["AuthKey"].ToNonNullString().Trim();
//
//                    // Update subscriber data in associated connection object
//                    connection.SubscriberID = new Guid(subscriber["ID"].ToNonNullString(Guid.Empty.ToString()).Trim());
//                    connection.SubscriberAcronym = subscriber["Acronym"].ToNonNullString().Trim();
//                    connection.SubscriberName = subscriber["Name"].ToNonNullString().Trim();
//                    connection.SharedSecret = sharedSecret;
//
//                    if (length >= 5)
//                    {
//                        // First 4 bytes beyond command byte represent an integer representing the length of the authentication string that follows
//                        int byteLength = EndianOrder.BigEndian.ToInt32(buffer, startIndex);
//                        startIndex += 4;
//
//                        // Byte length should be reasonable
//                        if (byteLength >= 16 && byteLength <= 256)
//                        {
//                            if (length >= 5 + byteLength)
//                            {
//                                // Decrypt encoded portion of buffer
//                                byte[] bytes = buffer.Decrypt(startIndex, byteLength, sharedSecret, CipherStrength.Aes256);
//                                startIndex += byteLength;
//
//                                // Validate the authentication ID - if it matches, connection is authenticated
//                                connection.Authenticated = (string.Compare(authenticationID, GetClientEncoding(clientID).GetString(bytes, CipherSaltLength, bytes.Length - CipherSaltLength)) == 0);
//
//                                if (connection.Authenticated)
//                                {
//                                    // Send success response
//                                    message = string.Format("Registered subscriber \"{0}\" {1} was successfully authenticated.", connection.SubscriberName, connection.ConnectionID);
//                                    SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Authenticate, message);
//                                    OnStatusMessage(message);
//                                    return;
//                                }
//                                else
//                                {
//                                    message = string.Format("Subscriber authentication failed - {0} request denied.", ServerCommand.Authenticate);
//                                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                                    OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber authentication failed.", connection.ConnectionID, ServerCommand.Authenticate);
//                                    return;
//                                }
//                            }
//                            else
//                            {
//                                message = "Not enough buffer was provided to parse client request.";
//                                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                                OnProcessException(new InvalidOperationException(message));
//                                return;
//                            }
//                        }
//                        else
//                        {
//                            message = string.Format("Received request packet with an unexpected size from {0} - {1} request denied.", connection.ConnectionID, ServerCommand.Authenticate);
//                            SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                            OnStatusMessage("WARNING: Registered subscriber \"{0}\" {1} {2} command request was denied due to oddly sized {3} byte authentication packet.", connection.SubscriberName, connection.ConnectionID, ServerCommand.Authenticate, byteLength);
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        message = "Not enough buffer was provided to parse client request.";
//                        SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                        OnProcessException(new InvalidOperationException(message));
//                        return;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                message = "Failed to process authentication request due to exception: " + ex.Message;
//                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
//                OnProcessException(new InvalidOperationException(message, ex));
//                return;
//            }
        }

        // Handles subscribe request
        private void HandleSubscribeRequest(ClientConnection connection, byte[] buffer, int startIndex, int length)
        {
            Guid clientID = connection.ClientID;
            IClientSubscription subscription;
            string message, setting;

            // Handle subscribe
            try
            {
                // Make sure there is enough buffer for flags and connection string length
                if (length >= 6)
                {
                    // Next byte is the data packet flags
                    DataPacketFlags flags = (DataPacketFlags)buffer[startIndex];
                    startIndex++;

                    bool useSynchronizedSubscription = ((byte)(flags & DataPacketFlags.Synchronized) > 0);

                    if (useSynchronizedSubscription && !m_allowSynchronizedSubscription)
                    {
                        // Remotely synchronized subscriptions are currently disallowed by data publisher
                        message = "Client request for remotely synchronized data subscription was denied. Data publisher is currently configured to disallow synchronized subscriptions.";
                        SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
                        OnProcessException(new InvalidOperationException(message));
                    }
                    else
                    {
                        bool useCompactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                        bool addSubscription = false;

                        // Next 4 bytes are an integer representing the length of the connection string that follows
                        int byteLength = EndianOrder.BigEndian.ToInt32(buffer, startIndex);
                        startIndex += 4;

                        if (byteLength > 0 && length >= 6 + byteLength)
                        {
                            string connectionString = GetClientEncoding(clientID).GetString(buffer, startIndex, byteLength);
                            startIndex += byteLength;

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

                            // Pass subscriber assembly information to connection, if defined
                            if (subscription.Settings.TryGetValue("assemblyInfo", out setting))
                                connection.SubscriberInfo = setting;

                            // Set up UDP data channel if client has requested this
                            connection.DataChannel = null;

                            if (subscription.Settings.TryGetValue("dataChannel", out setting))
                            {
                                TransportProvider<Socket> client;
                                Dictionary<string, string> settings = setting.ParseKeyValuePairs();
                                IPEndPoint localEndPoint = null;
                                string networkInterface = "::0";

                                // Make sure return interface matches incoming client connection
                                if (m_commandChannel.TryGetClient(connection.ClientID, out client))
                                    localEndPoint = client.Provider.LocalEndPoint as IPEndPoint;
                                else
                                    localEndPoint = m_commandChannel.Server.LocalEndPoint as IPEndPoint;

                                if ((object)localEndPoint != null)
                                {
                                    networkInterface = localEndPoint.Address.ToString();

                                    // Remove dual-stack prefix
                                    if (networkInterface.StartsWith("::ffff:", true, CultureInfo.InvariantCulture))
                                        networkInterface = networkInterface.Substring(7);
                                }

                                if (settings.TryGetValue("port", out setting) || settings.TryGetValue("localport", out setting))
                                {
                                    connection.DataChannel = new UdpServer(string.Format("Port=-1; Clients={0}:{1}; interface={2}", connection.IPAddress, int.Parse(setting), networkInterface));
                                    connection.DataChannel.Start();
                                }
                            }

                            // Remove any existing cached publication channel since connection is changing
                            IServer publicationChannel;
                            m_clientPublicationChannels.TryRemove(clientID, out publicationChannel);

                            // Update measurement serialization format type
                            subscription.UseCompactMeasurementFormat = useCompactMeasurementFormat;

                            // Track subscription in connection information
                            connection.Subscription = subscription;

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

                                // Attach to processing completed notification
                                subscription.ProcessingComplete += subscription_ProcessingComplete;
                            }
                            else
                            {
                                // Manually re-initialize existing client subscription
                                subscription.Initialize();
                                subscription.Initialized = true;
                            }

                            // Ensure that subscription is initialized and signal index cache has been updated.
                            subscription.WaitForInitialize(subscription.InitializationTimeout);

                            // Send updated signal index cache to client with validated rights of the selected input measurement keys
                            byte[] serializedSignalIndexCache = SerializeSignalIndexCache(clientID, subscription.SignalIndexCache);
                            SendClientResponse(clientID, ServerResponse.UpdateSignalIndexCache, ServerCommand.Subscribe, serializedSignalIndexCache);

                            // Spawn routing table recalculation
                            m_routingTables.CalculateRoutingTables(null);

                            // Make sure adapter is started
                            subscription.Start();

                            // TODO: Add a flag to the database to allow payload encryption to be subsciber specific instead of global to publisher...
                            // Send new or updated cipher keys
                            if (connection.Authenticated && m_encryptPayload)
                                connection.RotateCipherKeys();

                            // If client has subscribed to any cached measurements, queue them up for the client
                            IActionAdapter adapter;
                            LatestMeasurementCache cache;

                            if (TryGetAdapterByName("LatestMeasurementCache", out adapter))
                            {
                                cache = adapter as LatestMeasurementCache;

                                if ((object)cache != null)
                                {
                                    IEnumerable<IMeasurement> cachedMeasurements = cache.LatestMeasurements.Where(measurement => subscription.InputMeasurementKeys.Any(key => key.SignalID == measurement.ID)).Cast<IMeasurement>();
                                    subscription.QueueMeasurementsForProcessing(cachedMeasurements);
                                }
                            }

                            // Notify any direct publisher consumers about the new client connection
                            try
                            {
                                OnClientConnected(connection.SubscriberID, connection.ConnectionID, connection.SubscriberInfo);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(new InvalidOperationException(string.Format("ClientConnected event handler exception: {0}", ex.Message), ex));
                            }

                            // Send success response
                            if (subscription.TemporalConstraintIsDefined())
                            {
                                message = string.Format("Client subscribed as {0}compact {1}synchronized with a temporal constraint.", useCompactMeasurementFormat ? "" : "non-", useSynchronizedSubscription ? "" : "un");
                            }
                            else
                            {
                                if (subscription.InputMeasurementKeys != null)
                                    message = string.Format("Client subscribed as {0}compact {1}synchronized with {2} signals.", useCompactMeasurementFormat ? "" : "non-", useSynchronizedSubscription ? "" : "un", subscription.InputMeasurementKeys.Length);
                                else
                                    message = string.Format("Client subscribed as {0}compact {1}synchronized, but no signals were specified. Make sure \"inputMeasurementKeys\" setting is properly defined.", useCompactMeasurementFormat ? "" : "non-", useSynchronizedSubscription ? "" : "un");
                            }

                            connection.IsSubscribed = true;
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
        }

        // Handles unsubscribe request
        private void HandleUnsubscribeRequest(ClientConnection connection)
        {
            Guid clientID = connection.ClientID;

            RemoveClientSubscription(clientID); // This does not disconnect client command channel - nor should it...

            // Detach from processing completed notification
            if (connection.Subscription != null)
                connection.Subscription.ProcessingComplete -= subscription_ProcessingComplete;

            connection.Subscription = null;
            connection.IsSubscribed = false;

            SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Unsubscribe, "Client unsubscribed.");
            OnStatusMessage(connection.ConnectionID + " unsubscribed.");
        }

        // Handles meta-data refresh request
        private void HandleMetadataRefresh(ClientConnection connection)
        {
//            Guid clientID = connection.ClientID;
//            string message;
//
//            try
//            {
//                using (AdoDataConnection adoDatabase = new AdoDataConnection("systemSettings"))
//                {
//                    IDbConnection dbConnection = adoDatabase.Connection;
//                    DataSet metadata = new DataSet();
//                    DataTable table;
//
//                    byte[] serializedMetadata;
//
//                    // Initialize active node ID
//                    Guid nodeID = new Guid(dbConnection.ExecuteScalar(string.Format("SELECT NodeID FROM IaonActionAdapter WHERE ID = {0}", ID)).ToString());
//
//                    // Copy key meta-data tables
//                    foreach (string tableExpression in m_metadataTables.Split(';'))
//                    {
//                        if (!tableExpression.IsNullOrWhiteSpace())
//                        {
//                            // Query the table or view information from the database
//                            table = dbConnection.RetrieveData(adoDatabase.AdapterType, tableExpression);
//
//                            // Remove any expression from table name
//                            Match regexMatch = Regex.Match(tableExpression, @"FROM \w+");
//                            table.TableName = regexMatch.Value.Split(' ')[1];
//
//                            // If table has a NodeID column, filter table data for just this node.
//                            // Also, determine whether we need to check subscriber for rights to the data
//                            bool applyNodeIDFilter = table.Columns.Contains("NodeID");
//                            bool checkSubscriberRights = m_requireAuthentication && table.Columns.Contains("SignalID");
//
//                            if (m_sharedDatabase || (!applyNodeIDFilter && !checkSubscriberRights))
//                            {
//                                // Add a copy of the results to the dataset for meta-data exchange
//                                metadata.Tables.Add(table.Copy());
//                            }
//                            else
//                            {
//                                IEnumerable<DataRow> filteredRows;
//                                List<DataRow> filteredRowList;
//
//                                // Make a copy of the table structure
//                                metadata.Tables.Add(table.Clone());
//
//                                // Reduce data to only this node
//                                if (applyNodeIDFilter)
//                                    filteredRows = table.Select(string.Format("NodeID = '{0}'", nodeID));
//                                else
//                                    filteredRows = table.Rows.Cast<DataRow>();
//
//                                // Reduce data to only what the subscriber has rights to
//                                if (checkSubscriberRights)
//                                    filteredRows = filteredRows.Where(row => SubscriberHasRights(connection.SubscriberID, adoDatabase.Guid(row, "SignalID")));
//
//                                filteredRowList = filteredRows.ToList();
//
//                                if (filteredRowList.Count > 0)
//                                {
//                                    DataTable metadataTable = metadata.Tables[table.TableName];
//
//                                    // Manually copy-in each row into table
//                                    foreach (DataRow row in filteredRowList)
//                                    {
//                                        DataRow newRow = metadataTable.NewRow();
//
//                                        // Copy each column of data in the current row
//                                        for (int x = 0; x < table.Columns.Count; x++)
//                                        {
//                                            newRow[x] = row[x];
//                                        }
//
//                                        metadataTable.Rows.Add(newRow);
//                                    }
//                                }
//                            }
//                        }
//                    }
//
//                    serializedMetadata = SerializeMetadata(clientID, metadata);
//                    SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.MetaDataRefresh, serializedMetadata);
//                }
//            }
//            catch (Exception ex)
//            {
//                message = "Failed to transfer meta-data due to exception: " + ex.Message;
//                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
//                OnProcessException(new InvalidOperationException(message, ex));
//            }
        }

        // Handles request to update processing interval on client session
        private void HandleUpdateProcessingInterval(ClientConnection connection, byte[] buffer, int startIndex, int length)
        {
            Guid clientID = connection.ClientID;
            string message;

            // Make sure there is enough buffer for new processing interval value
            if (length >= 4)
            {
                // Next 4 bytes are an integer representing the new processing interval
                int processingInterval = EndianOrder.BigEndian.ToInt32(buffer, startIndex);

                IClientSubscription subscription = connection.Subscription;

                if (subscription != null)
                {
                    subscription.ProcessingInterval = processingInterval;
                    SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.UpdateProcessingInterval, "New processing interval of {0} assigned.", processingInterval);
                    OnStatusMessage("{0} was assigned a new processing interval of {1}.", connection.ConnectionID, processingInterval);
                }
                else
                {
                    message = "Client subcription was not available, could not update processing interval.";
                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.UpdateProcessingInterval, message);
                    OnProcessException(new InvalidOperationException(message));
                }
            }
            else
            {
                message = "Not enough buffer was provided to update client processing interval.";
                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.UpdateProcessingInterval, message);
                OnProcessException(new InvalidOperationException(message));
            }
        }

        // Handle request to define operational modes for client connection
        private void HandleDefineOperationalModes(ClientConnection connection, byte[] buffer, int startIndex, int length)
        {
            uint operationalModes;

            if (length >= 4)
            {
                operationalModes = EndianOrder.BigEndian.ToUInt32(buffer, startIndex);

                if ((operationalModes & (uint)OperationalModes.VersionMask) != 0u)
                    OnStatusMessage("WARNING: Protocol version not supported. Operational modes may not be set correctly for client {0}.", connection.ClientID);

                connection.OperationalModes = (OperationalModes)operationalModes;
            }
        }

        private byte[] SerializeSignalIndexCache(Guid clientID, SignalIndexCache signalIndexCache)
        {
            byte[] serializedSignalIndexCache = null;
            ClientConnection connection;

            if (m_clientConnections.TryGetValue(clientID, out connection))
            {
                OperationalModes operationalModes = connection.OperationalModes;
                GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(operationalModes & OperationalModes.CompressionModeMask);
                bool useCommonSerializationFormat = (operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
                bool compressSignalIndexCache = (operationalModes & OperationalModes.CompressSignalIndexCache) > 0;

                MemoryStream compressedData = null;
                GZipStream deflater = null;

                if (!useCommonSerializationFormat)
                {
                    // Use standard .NET BinaryFormatter
                    serializedSignalIndexCache = Serialization.Serialize(signalIndexCache, GSF.SerializationFormat.Binary);
                }
                else
                {
                    // Use ISupportBinaryImage implementation
                    signalIndexCache.Encoding = GetClientEncoding(clientID);
                    serializedSignalIndexCache = new byte[signalIndexCache.BinaryLength];
                    signalIndexCache.GenerateBinaryImage(serializedSignalIndexCache, 0);
                }

                if (compressSignalIndexCache && gatewayCompressionMode == GatewayCompressionMode.GZip)
                {
                    try
                    {
                        // Compress serialized signal index cache into compressed data buffer
                        compressedData = new MemoryStream();
                        deflater = new GZipStream(compressedData, CompressionMode.Compress);
                        deflater.Write(serializedSignalIndexCache, 0, serializedSignalIndexCache.Length);
                        deflater.Close();
                        deflater = null;

                        serializedSignalIndexCache = compressedData.ToArray();
                    }
                    finally
                    {
                        if ((object)deflater != null)
                            deflater.Close();

                        if ((object)compressedData != null)
                            compressedData.Close();
                    }
                }
            }

            return serializedSignalIndexCache;
        }

        private byte[] SerializeMetadata(Guid clientID, DataSet metadata)
        {
            byte[] serializedMetadata = null;
            ClientConnection connection;

            if (m_clientConnections.TryGetValue(clientID, out connection))
            {
                OperationalModes operationalModes = connection.OperationalModes;
                GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(operationalModes & OperationalModes.CompressionModeMask);
                bool useCommonSerializationFormat = (operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
                bool compressMetadata = (operationalModes & OperationalModes.CompressMetadata) > 0;

                MemoryStream encodedData = null;
                XmlTextWriter unicodeWriter = null;

                MemoryStream compressedData = null;
                GZipStream deflater = null;

                if (!useCommonSerializationFormat)
                {
                    serializedMetadata = Serialization.Serialize(metadata, GSF.SerializationFormat.Binary);
                }
                else
                {
                    try
                    {
                        // Encode XML into encoded data buffer
                        encodedData = new MemoryStream();
                        unicodeWriter = new XmlTextWriter(encodedData, GetClientEncoding(clientID));
                        metadata.WriteXml(unicodeWriter, XmlWriteMode.WriteSchema);
                        unicodeWriter.Close();
                        unicodeWriter = null;

                        // Return result of compression
                        serializedMetadata = encodedData.ToArray();
                    }
                    finally
                    {
                        if ((object)unicodeWriter != null)
                            unicodeWriter.Close();

                        if ((object)encodedData != null)
                            encodedData.Close();
                    }
                }

                if (compressMetadata && gatewayCompressionMode == GatewayCompressionMode.GZip)
                {
                    try
                    {
                        // Compress serialized metadata into compressed data buffer
                        compressedData = new MemoryStream();
                        deflater = new GZipStream(compressedData, CompressionMode.Compress);
                        deflater.Write(serializedMetadata, 0, serializedMetadata.Length);
                        deflater.Close();
                        deflater = null;

                        serializedMetadata = compressedData.ToArray();
                    }
                    finally
                    {
                        if ((object)deflater != null)
                            deflater.Close();

                        if ((object)compressedData != null)
                            compressedData.Close();
                    }
                }

            }

            return serializedMetadata;
        }

        // Bubble up processing complete notifications from subscriptions
        private void subscription_ProcessingComplete(object sender, EventArgs<IClientSubscription, EventArgs> e)
        {
            // Expose notification via data publisher event subscribers
            if (ProcessingComplete != null)
                ProcessingComplete(sender, e.Argument2);

            IClientSubscription subscription = e.Argument1;
            string senderType = sender == null ? "N/A" : sender.GetType().Name;

            // Send direct notification to associated client
            if (subscription != null)
                SendClientResponse(subscription.ClientID, ServerResponse.ProcessingComplete, ServerCommand.Subscribe, senderType);
        }

        #endregion

        #region [ Command Channel Handlers ]

        private void m_commandChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            try
            {
                Guid clientID = e.Argument1;
                byte[] buffer = e.Argument2;
                int length = e.Argument3;
                int index = 0;

                if (length > 0 && buffer != null)
                {
                    ClientConnection connection;
                    ServerCommand command = ServerCommand.DefineOperationalModes;
                    string message;
                    byte commandByte = buffer[index];
                    index++;

                    // Attempt to parse solicited server command
                    bool validServerCommand = command.TryParse(commandByte.ToString(), out command);

                    // Look up this client connection
                    if (!m_clientConnections.TryGetValue(clientID, out connection))
                    {
                        // Received a request from an unknown client, this request is denied
                        OnStatusMessage("WARNING: Ignored {0} byte {1} command request received from an unrecognized client: {2}", length, validServerCommand ? command.ToString() : "unidentified", clientID);
                    }
                    else if (validServerCommand)
                    {
                        if (command != ServerCommand.DefineOperationalModes)
                        {
                            if (command == ServerCommand.Authenticate)
                            {
                                // Handle authenticate
                                HandleAuthenticationRequest(connection, buffer, index, length);
                                return;
                            }

                            if (m_requireAuthentication && !connection.Authenticated)
                            {
                                message = string.Format("Subscriber not authenticated - {0} request denied.", command);
                                SendClientResponse(clientID, ServerResponse.Failed, command, message);
                                OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber not authenticated.", connection.ConnectionID, command);
                                return;
                            }
                        }

                        switch (command)
                        {
                            case ServerCommand.Subscribe:
                                // Handle subscribe
                                HandleSubscribeRequest(connection, buffer, index, length);
                                break;
                            case ServerCommand.Unsubscribe:
                                // Handle unsubscribe
                                HandleUnsubscribeRequest(connection);
                                break;
                            case ServerCommand.MetaDataRefresh:
                                // Handle meta data refresh (per subscriber request)
                                HandleMetadataRefresh(connection);
                                break;
                            case ServerCommand.RotateCipherKeys:
                                // Handle rotation of cipher keys (per subscriber request)
                                connection.RotateCipherKeys();
                                break;
                            case ServerCommand.UpdateProcessingInterval:
                                // Handle request to update processing interval
                                HandleUpdateProcessingInterval(connection, buffer, index, length);
                                break;
                            case ServerCommand.DefineOperationalModes:
                                // Handle request to define oeprational modes
                                HandleDefineOperationalModes(connection, buffer, index, length);
                                break;
                        }
                    }
                    else
                    {
                        // Handle unrecognized commands
                        message = " sent an unrecognized server command: 0x" + commandByte.ToString("X").PadLeft(2, '0');
                        SendClientResponse(clientID, (byte)ServerResponse.Failed, commandByte, GetClientEncoding(clientID).GetBytes("Client" + message));
                        OnProcessException(new InvalidOperationException("WARNING: " + connection.ConnectionID + message));
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Encountered an exception while processing received client data: {0}", ex.Message), ex));
            }
        }

        private void m_commandChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;

            m_clientConnections[clientID] = new ClientConnection(this, clientID, m_commandChannel);

            OnStatusMessage("Client connected to command channel.");
        }

        private void m_commandChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(DisconnectClient, e.Argument);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue client disconnect due to exception: " + ex.Message, ex));
            }
        }

        private void m_commandChannel_ServerStarted(object sender, EventArgs e)
        {
            OnStatusMessage("Data publisher command channel started.");
        }

        private void m_commandChannel_ServerStopped(object sender, EventArgs e)
        {
            if (Enabled)
            {
                OnStatusMessage("Data publisher command channel was unexpectedly terminated, restarting...");

                // We must wait for command channel to completely shutdown before trying to restart...
                if (m_commandChannelRestartTimer != null)
                    m_commandChannelRestartTimer.Start();
            }
            else
            {
                OnStatusMessage("Data publisher command channel stopped.");
            }
        }

        private void m_commandChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!HandleSocketException(e.Argument1, ex as SocketException) && !(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data publisher encountered an exception while sending command channel data to client connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!HandleSocketException(e.Argument1, ex as SocketException) && !(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data publisher encountered an exception while receiving command channel data from client connection: " + ex.Message, ex));
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        // Constant zero length integer byte array
        private readonly static byte[] s_zeroLengthBytes = new byte[] { 0, 0, 0, 0 };

        #endregion
    }
}
