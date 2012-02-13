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
using TVA.Data;
using TVA.IO.Compression;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.Transport
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
        UpdateProcessingInterval = 0x05
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
        /// No flags set.
        /// </summary>
        /// <remarks>
        /// This would represent unsynchronized, full fidelity measurement data packets.
        /// </remarks>
        NoFlags = (Byte)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Represents a data publishing server that allows multiple connections for data subscriptions.
    /// </summary>
    public class DataPublisher : ActionAdapterCollection
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
        public event EventHandler ProcessingComplete;

        // Constants

        // Maximum packet size before split
        internal const int MaxPacketSize = ushort.MaxValue / 2;

        // Length of random salt prefix
        internal const int CipherSaltLength = 8;

        // Fields
        private TcpServer m_commandChannel;
        private ConcurrentDictionary<Guid, ClientConnection> m_clientConnections;
        private ConcurrentDictionary<Guid, IServer> m_clientPublicationChannels;
        private ConcurrentDictionary<MeasurementKey, Guid> m_signalIDCache;
        private RoutingTables m_routingTables;
        private IAdapterCollection m_parent;
        private string m_metadataTables;
        private bool m_requireAuthentication;
        private Guid m_nodeID;
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
            m_clientConnections = new ConcurrentDictionary<Guid, ClientConnection>();
            m_clientPublicationChannels = new ConcurrentDictionary<Guid, IServer>();
            m_signalIDCache = new ConcurrentDictionary<MeasurementKey, Guid>();
            m_metadataTables = "DeviceDetail WHERE OriginalSource IS NULL AND IsConcentrator <> 1 AND NodeID = {1};MeasurementDetail WHERE Internal <> 0 AND NodeID = {1}";
            m_routingTables = new RoutingTables()
            {
                ActionAdapters = this
            };
            m_routingTables.ProcessException += m_routingTables_ProcessException;
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

                        if (m_clientConnections != null)
                            m_clientConnections.Values.AsParallel().ForAll(cc => cc.Dispose());

                        m_clientConnections = null;

                        if (m_routingTables != null)
                        {
                            m_routingTables.ProcessException -= m_routingTables_ProcessException;
                            m_routingTables.Dispose();
                        }
                        m_routingTables = null;
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

            // Setup data publishing server with or without required authentication 
            if (settings.TryGetValue("requireAuthentication", out setting))
                m_requireAuthentication = setting.ParseBoolean();

            // Create a new TCP server
            TcpServer commandChannel = new TcpServer();

            // Initialize default settings
            commandChannel.SettingsCategory = Name.ToLower();
            commandChannel.ConfigurationString = "port=6165";
            commandChannel.PayloadAware = true;
            commandChannel.Compression = CompressionStrength.NoCompression;
            commandChannel.PersistSettings = true;

            // Assign command channel client reference and attach to needed events
            this.CommandChannel = commandChannel;

            // Initialize TCP server (loads config file settings)
            commandChannel.Initialize();

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
            signalIndexCache.UnauthorizedKeys = unauthorizedKeys.ToArray();

            // Send client updated signal index cache
            SendClientResponse(clientID, ServerResponse.UpdateSignalIndexCache, ServerCommand.Subscribe, Serialization.Serialize(signalIndexCache, TVA.SerializationFormat.Binary));
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
                Guid signalID;

                // Attempt to lookup input measurement keys for given source IDs from default measurement table, if defined
                try
                {
                    DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("ID='" + keyParam.ToString() + "'");

                    if (filteredRows.Length > 0 && Guid.TryParse(filteredRows[0]["SignalID"].ToString(), out signalID))
                        return signalID;
                }
                catch
                {
                    // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys based on specified source ID's
                }

                return Guid.Empty;
            });
        }

        /// <summary>
        /// Sends the start time of the first measurement in a connection transmission.
        /// </summary>
        /// <param name="clientID">ID of client to send response.</param>
        /// <param name="startTime">Start time, in <see cref="Ticks"/>, of first measurement transmitted.</param>
        internal protected virtual bool SendDataStartTime(Guid clientID, Ticks startTime)
        {
            bool result = SendClientResponse(clientID, ServerResponse.DataStartTime, ServerCommand.Subscribe, EndianOrder.BigEndian.GetBytes((long)startTime));
            OnStatusMessage("Start time sent to {0}.", m_clientConnections[clientID].ConnectionID);
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
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, string formattedStatus, params object[] args)
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
        internal protected virtual bool SendClientResponse(Guid clientID, ServerResponse response, ServerCommand command, byte[] data)
        {
            return SendClientResponse(clientID, (byte)response, (byte)command, data);
        }

        // Send binary response packet to client
        private bool SendClientResponse(Guid clientID, byte responseCode, byte commandCode, byte[] data)
        {
            bool success = false;
            IServer publishChannel;

            // Data packets can be published on a UDP data channel, so check for this...
            if (responseCode == (byte)ServerResponse.DataPacket)
            {
                ClientConnection connection;

                // Attempt to lookup associated client connection
                m_clientConnections.TryGetValue(clientID, out connection);

                // Lookup proper publication channel
                publishChannel = m_clientPublicationChannels.GetOrAdd(clientID, id => connection == null ? m_commandChannel : connection.PublishChannel);

                // Encrypt data packet payload if connection key is defined
                if (connection != null && connection.Key != null)
                    data = data.Encrypt(connection.Key, connection.IV, CipherStrength.Aes256);
            }
            else
            {
                publishChannel = m_commandChannel;
            }

            // Send response packet
            if (publishChannel != null && publishChannel.CurrentState == ServerState.Running)
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

                    byte[] responseData = responsePacket.ToArray();
                    int responseLength = unchecked((int)responsePacket.Length);

                    if (publishChannel is UdpServer)
                        publishChannel.MulticastAsync(responseData, 0, responseLength);
                    else
                        publishChannel.SendToAsync(clientID, responseData, 0, responseLength);

                    success = true;
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
                    if (ex.ErrorCode != 10053 && ex.ErrorCode != 10054)
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

            // Notify system that subscriber disconnected therefore demanded measurements may have changed
            ThreadPool.QueueUserWorkItem(NotifyHostOfSubscriptionRemoval);
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
            if (TryGetAdapter<Guid>(clientID, (item, value) => ((IClientSubscription)item).ClientID == value, out adapter))
            {
                subscription = (IClientSubscription)adapter;
                return true;
            }

            subscription = null;
            return false;
        }

        // Gets specfied property from client connection based on subscriber ID
        private TResult GetConnectionProperty<TResult>(Guid subscriberID, Func<ClientConnection, TResult> predicate)
        {
            TResult result = default(TResult);

            // Lookup client connection by subscriber ID
            ClientConnection connection = m_clientConnections.Values.FirstOrDefault(cc => cc.SubscriberID == subscriberID);

            // Extract desired property from client connection using given predicate function
            if (connection != null)
                result = predicate(connection);

            return result;
        }

        // Make sure to expose any routing table exceptions
        private void m_routingTables_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #region [ Server Command Request Handlers ]

        // Handles authentication request
        private void HandleAuthenticationRequest(ClientConnection connection, byte[] buffer, int startIndex, int length)
        {
            Guid clientID = connection.ClientID;
            string message;

            // Handle authentication request
            try
            {
                DataRow subscriber = null;

                // Reset existing authentication state
                connection.Authenticated = false;

                // Subscriber connection is first referenced by its IP
                foreach (DataRow row in DataSource.Tables["Subscribers"].Select("Enabled <> 0"))
                {
                    IEnumerable<IPAddress> ipAddresses = row["ValidIPAddresses"].ToNonNullString().Split(';', ',').Where(ip => !string.IsNullOrWhiteSpace(ip)).Select(ip => IPAddress.Parse(ip.Trim()));
                    foreach (IPAddress ipAddress in ipAddresses)
                    {
                        if (connection.IPAddress.ToString().Contains(ipAddress.ToString()))
                        {
                            subscriber = row;
                            break;
                        }

                    }

                    if (subscriber != null)
                        break;

                    //if (row["ValidIPAddresses"].ToNonNullString().Split(';', ',').Where(ip => !string.IsNullOrWhiteSpace(ip)).Select(ip => IPAddress.Parse(ip.Trim())).Contains(connection.IPAddress))
                    //{
                    //    // Found registered subscriber record for the connected IP
                    //    subscriber = row;
                    //    break;
                    //}
                }

                if (subscriber == null)
                {
                    message = string.Format("No subscriber is registered for {0}, cannnot authenticate connection - {1} request denied.", connection.ConnectionID, ServerCommand.Authenticate);
                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                    OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber is disabled or not registered.", connection.ConnectionID, ServerCommand.Authenticate);
                    return;
                }
                else
                {
                    string sharedSecret = subscriber["SharedSecret"].ToNonNullString().Trim();
                    string authenticationID = subscriber["AuthKey"].ToNonNullString().Trim();

                    // Update subscriber data in associated connection object
                    connection.SubscriberID = Guid.Parse(subscriber["ID"].ToNonNullString(Guid.Empty.ToString()).Trim());
                    connection.SubscriberAcronym = subscriber["Acronym"].ToNonNullString().Trim();
                    connection.SubscriberName = subscriber["Name"].ToNonNullString().Trim();
                    connection.SharedSecret = sharedSecret;

                    if (length >= 5)
                    {
                        // First 4 bytes beyond command byte represent an integer representing the length of the authentication string that follows
                        int byteLength = EndianOrder.BigEndian.ToInt32(buffer, startIndex);
                        startIndex += 4;

                        // Byte length should be reasonable
                        if (byteLength >= 16 && byteLength <= 256)
                        {
                            if (length >= 5 + byteLength)
                            {
                                // Decrypt encoded portion of buffer
                                byte[] bytes = buffer.Decrypt(startIndex, byteLength, sharedSecret, CipherStrength.Aes256);
                                startIndex += byteLength;

                                // Validate the authentication ID - if it matches, connection is authenticated
                                connection.Authenticated = (string.Compare(authenticationID, Encoding.Unicode.GetString(bytes, CipherSaltLength, bytes.Length - CipherSaltLength)) == 0);

                                if (connection.Authenticated)
                                {
                                    // Send success response
                                    message = string.Format("Registered subscriber \"{0}\" {1} was successfully authenticated.", connection.SubscriberName, connection.ConnectionID);
                                    SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Authenticate, message);
                                    OnStatusMessage(message);
                                    return;
                                }
                                else
                                {
                                    message = string.Format("Subscriber authentication failed - {0} request denied.", ServerCommand.Authenticate);
                                    SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                                    OnStatusMessage("WARNING: Client {0} {1} command request denied - subscriber authentication failed.", connection.ConnectionID, ServerCommand.Authenticate);
                                    return;
                                }
                            }
                            else
                            {
                                message = "Not enough buffer was provided to parse client request.";
                                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                                OnProcessException(new InvalidOperationException(message));
                                return;
                            }
                        }
                        else
                        {
                            message = string.Format("Received request packet with an unexpected size from {0} - {1} request denied.", connection.ConnectionID, ServerCommand.Authenticate);
                            SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                            OnStatusMessage("WARNING: Registered subscriber \"{0}\" {1} {2} command request was denied due to oddly sized {3} byte authentication packet.", connection.SubscriberName, connection.ConnectionID, ServerCommand.Authenticate, byteLength);
                            return;
                        }
                    }
                    else
                    {
                        message = "Not enough buffer was provided to parse client request.";
                        SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                        OnProcessException(new InvalidOperationException(message));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Failed to process authentication request due to exception: " + ex.Message;
                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Authenticate, message);
                OnProcessException(new InvalidOperationException(message, ex));
                return;
            }
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
                    bool useCompactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                    bool addSubscription = false;

                    // Next 4 bytes are an integer representing the length of the connection string that follows
                    int byteLength = EndianOrder.BigEndian.ToInt32(buffer, startIndex);
                    startIndex += 4;

                    if (byteLength > 0 && length >= 6 + byteLength)
                    {
                        string connectionString = Encoding.Unicode.GetString(buffer, startIndex, byteLength);
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

                        // Set up UDP data channel if client has requested this
                        connection.DataChannel = null;

                        if (subscription.Settings.TryGetValue("dataChannel", out setting))
                        {
                            Dictionary<string, string> settings = setting.ParseKeyValuePairs();
                            string networkInterface;
                            bool compressionEnabled = false;

                            settings.TryGetValue("interface", out networkInterface);

                            if (string.IsNullOrWhiteSpace(networkInterface))
                                networkInterface = "::0";

                            if (settings.TryGetValue("compression", out setting))
                                compressionEnabled = setting.ParseBoolean();

                            if (settings.TryGetValue("port", out setting))
                            {
                                connection.DataChannel = new UdpServer(string.Format("Port=-1; Clients={0}:{1}; interface={2}", connection.IPAddress, int.Parse(setting), networkInterface));

                                if (compressionEnabled)
                                    connection.DataChannel.Compression = CompressionStrength.Standard;

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

                        // Spawn routing table recalculation
                        m_routingTables.CalculateRoutingTables(null);

                        // Make sure adapter is started
                        subscription.Start();

                        // Send updated signal index cache to client with validated rights of the selected input measurement keys
                        SendClientResponse(clientID, ServerResponse.UpdateSignalIndexCache, ServerCommand.Subscribe, Serialization.Serialize(subscription.SignalIndexCache, TVA.SerializationFormat.Binary));

                        // Send new or updated cipher keys
                        //if (connection.Authenticated)
                        //    connection.RotateCipherKeys();

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
        }

        // Handles unsubscribe request
        private void HandleUnsubscribeRequest(ClientConnection connection)
        {
            Guid clientID = connection.ClientID;

            RemoveClientSubscription(clientID); // This does not disconnect client command channel...

            // Detach from processing completed notification
            if (connection.Subscription != null)
                connection.Subscription.ProcessingComplete -= subscription_ProcessingComplete;

            connection.Subscription = null;

            SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.Unsubscribe, "Client unsubscribed.");
            OnStatusMessage(connection.ConnectionID + " unsubscribed.");
        }

        // Handles meta-data refresh request
        private void HandleMetadataRefresh(ClientConnection connection)
        {
            Guid clientID = connection.ClientID;
            string message;

            try
            {
                AdoDataConnection adoDatabase = new AdoDataConnection("systemSettings");
                IDbConnection dbConnection = adoDatabase.Connection;
                DataSet metadata = new DataSet();
                DataTable table;

                // Initialize active node ID
                if (m_nodeID == Guid.Empty)
                    m_nodeID = Guid.Parse(dbConnection.ExecuteScalar(string.Format("SELECT NodeID FROM IaonActionAdapter WHERE ID = {0};", ID)).ToString());

                // Copy key meta-data tables
                foreach (string tableName in m_metadataTables.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        table = dbConnection.RetrieveData(adoDatabase.AdapterType, string.Format("SELECT * FROM {0}", tableName, m_nodeID));
                        table.TableName = tableName.Split(' ')[0];
                        metadata.Tables.Add(table.Copy());
                    }
                }

                SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.MetaDataRefresh, Serialization.Serialize(metadata, TVA.SerializationFormat.Binary));
            }
            catch (Exception ex)
            {
                message = "Failed to transfer meta-data due to exception: " + ex.Message;
                SendClientResponse(clientID, ServerResponse.Failed, ServerCommand.Subscribe, message);
                OnProcessException(new InvalidOperationException(message, ex));
            }
        }

        // Handles request to rotate cipher keys on client session
        private void HandleRotateCipherKeys(ClientConnection connection)
        {
            Guid clientID = connection.ClientID;

            connection.RotateCipherKeys();

            SendClientResponse(clientID, ServerResponse.Succeeded, ServerCommand.RotateCipherKeys, "New cipher keys established.");
            OnStatusMessage(connection.ConnectionID + " cipher keys rotated.");
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

        private void m_commandChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;

            m_clientConnections[clientID] = new ClientConnection(this, clientID, m_commandChannel);

            OnStatusMessage("Client connected.");
        }

        private void m_commandChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;
            ClientConnection connection;
            IServer publicationChannel;

            RemoveClientSubscription(clientID);

            if (m_clientConnections.TryRemove(clientID, out connection))
                connection.Dispose();

            m_clientPublicationChannels.TryRemove(clientID, out publicationChannel);

            OnStatusMessage("Client disconnected.");
        }

        private void m_commandChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            Guid clientID = e.Argument1;
            byte[] buffer = e.Argument2;
            int length = e.Argument3;
            int index = 0;

            if (length > 0 && buffer != null)
            {
                ClientConnection connection;
                ServerCommand command;
                string message;
                byte commandByte = buffer[index];
                index++;

                // Attempt to parse solicited server command
                bool validServerCommand = Enum.TryParse<ServerCommand>(commandByte.ToString(), out command);

                // Look up this client connection
                if (!m_clientConnections.TryGetValue(clientID, out connection))
                {
                    // Received a request from an unknown client, this request is denied
                    OnStatusMessage("WARNING: Ignored {0} byte {1} command request received from an unrecognized client: {2}", length, validServerCommand ? command.ToString() : "unidentified", clientID);
                }
                else if (validServerCommand)
                {
                    if (command == ServerCommand.Authenticate)
                    {
                        // Handle authenticate
                        HandleAuthenticationRequest(connection, buffer, index, length);
                        return;
                    }
                    else if (m_requireAuthentication && !connection.Authenticated)
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
                            HandleSubscribeRequest(connection, buffer, index, length);
                            break;
                        case ServerCommand.Unsubscribe:
                            // Handle unsubscribe
                            HandleUnsubscribeRequest(connection);
                            break;
                        case ServerCommand.MetaDataRefresh:
                            // Handle meta data refresh
                            HandleMetadataRefresh(connection);
                            break;
                        case ServerCommand.RotateCipherKeys:
                            // Handle rotation of cipher keys
                            HandleRotateCipherKeys(connection);
                            break;
                        case ServerCommand.UpdateProcessingInterval:
                            // Handle request to update processing interval
                            HandleUpdateProcessingInterval(connection, buffer, index, length);
                            break;
                    }
                }
                else
                {
                    // Handle unrecognized commands
                    message = " sent an unrecognized server command: 0x" + commandByte.ToString("X").PadLeft(2, '0');
                    SendClientResponse(clientID, (byte)ServerResponse.Failed, commandByte, Encoding.Unicode.GetBytes("Client" + message));
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

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && (((System.Net.Sockets.SocketException)ex).ErrorCode == 10053 || ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054)))
                OnProcessException(new InvalidOperationException("Data publisher encountered an exception while sending data to client connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && (((System.Net.Sockets.SocketException)ex).ErrorCode == 10053 || ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054)))
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

        #endregion
    }
}
