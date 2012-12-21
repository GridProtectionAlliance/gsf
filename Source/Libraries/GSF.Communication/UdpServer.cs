//******************************************************************************************************
//  UdpServer.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/09/2009 - Pinal C. Patel
//       Modified to attempt resuming reception on SocketException for non-Handshake enabled connection.
//  07/17/2009 - Pinal C. Patel
//       Added support to specify a specific interface address on a multiple interface machine.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/14/2009 - Pinal C. Patel
//       Fixed bug in the processing of Handshake messages.
//       Added null reference checks to Stop() and DisconnectOne() for safety.
//  10/30/2009 - Pinal C. Patel
//       Added support for one-way communication by specifying Port=-1 in ConfigurationString.
//  04/29/2010 - Pinal C. Patel
//       Modified Start() to parse client endpoint strings correctly to address IPv6 IP parsing issue.
//  02/13/2011 - Pinal C. Patel
//       Modified Start() to use "interface" in the creation of client endpoint.
//  03/10/2011 - Pinal C. Patel
//       Fixed a bug reported by Jeffrey Martin at Areva-TD (jeffrey.martin-econ@areva-td.com) that
//       prevented the ServerStopped event from being raised under certain configuration.
//  12/04/2011 - J. Ritchie Carroll
//       Modified to use concurrent dictionary.
//  07/23/2012 - Stephen C. Wills
//       Performed a full refactor to use the SocketAsyncEventArgs API calls.
//  10/31/2012 - Stephen C. Wills
//       Replaced single-threaded BlockingCollection pattern with asynchronous loop pattern.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="UdpServer"/> when the primary purpose is to transmit data.
    /// </para>
    /// <para>
    /// The <see cref="UdpServer.Server"/> socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the interface in the <see cref="ServerBase.ConfigurationString"/> (Example: "Port=8888; Clients=localhost:8989; Interface=127.0.0.1")
    /// </para>
    /// <para>
    /// The <see cref="UdpServer.Server"/> socket can be used just for transmitting data without being bound to a local interface 
    /// by specifying -1 for the port number in the <see cref="ServerBase.ConfigurationString"/> (Example: "Port=-1; Clients=localhost:8989")
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="UdpServer"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// using GSF.Security.Cryptography;
    /// using GSF.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static UdpServer m_server;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the server.
    ///         m_server = new UdpServer("Port=8888; Clients=localhost:8989");
    ///         m_server.Handshake = false;
    ///         m_server.ReceiveTimeout = -1;
    ///         m_server.Encryption = CipherStrength.None;
    ///         m_server.Compression = CompressionStrength.NoCompression;
    ///         m_server.SecureSession = false;
    ///         m_server.Initialize();
    ///         // Register event handlers.
    ///         m_server.ServerStarted += m_server_ServerStarted;
    ///         m_server.ServerStopped += m_server_ServerStopped;
    ///         m_server.ClientConnected += m_server_ClientConnected;
    ///         m_server.ClientDisconnected += m_server_ClientDisconnected;
    ///         m_server.ReceiveClientDataComplete += m_server_ReceiveClientDataComplete;
    ///         // Start the server.
    ///         m_server.Start();
    /// 
    ///         // Multicast user input to all connected clients.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_server.Multicast(input);
    ///         }
    /// 
    ///         // Stop the server on shutdown.
    ///         m_server.Stop();
    ///     }
    /// 
    ///     static void m_server_ServerStarted(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Server has been started!");
    ///     }
    /// 
    ///     static void m_server_ServerStopped(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Server has been stopped!");
    ///     }
    /// 
    ///     static void m_server_ClientConnected(object sender, EventArgs&lt;Guid&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Client connected - {0}.", e.Argument));
    ///     }
    /// 
    ///     static void m_server_ClientDisconnected(object sender, EventArgs&lt;Guid&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Client disconnected - {0}.", e.Argument));
    ///     }
    /// 
    ///     static void m_server_ReceiveClientDataComplete(object sender, EventArgs&lt;Guid, byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data from {0} - {1}.", e.Argument1, m_server.TextEncoding.GetString(e.Argument2, 0, e.Argument3)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UdpServer : ServerBase
    {
        #region [ Members ]

        // Nested Types
        private class UdpClientInfo
        {
            public TransportProvider<EndPoint> Client;
            public SocketAsyncEventArgs SendArgs;
            public SpinLock SendLock;
            public ConcurrentQueue<UdpServerPayload> SendQueue;
            public int Sending;
        }

        private class UdpServerPayload
        {
            // Per payload state
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;

            // Per client state
            public UdpClientInfo ClientInfo;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888; Clients=localhost:8989";

        /// <summary>
        /// Specifies the constant to be used for disabling <see cref="SocketError.ConnectionReset"/> when endpoint is not listening.
        /// </summary>
        private const int SIO_UDP_CONNRESET = -1744830452;

        // Fields
        private TransportProvider<Socket> m_udpServer;
        private SocketAsyncEventArgs m_receiveArgs;
        private ConcurrentDictionary<Guid, UdpClientInfo> m_clientInfoLookup;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Dictionary<string, string> m_configData;

        private EventHandler<SocketAsyncEventArgs> m_sendHandler;
        private EventHandler<SocketAsyncEventArgs> m_receiveHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        public UdpServer()
            : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="UdpServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public UdpServer(string configString)
            : base(TransportProtocol.Udp, configString)
        {
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_clientInfoLookup = new ConcurrentDictionary<Guid, UdpClientInfo>();

            m_sendHandler = (sender, args) => ProcessSend(args);
            m_receiveHandler = (sender, args) => ProcessReceive(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="UdpServer"/>.</param>
        public UdpServer(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that determines if dual-mode socket is allowed when endpoint address is IPv6.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultAllowDualStackSocket),
        Description("Determines if dual-mode socket is allowed when endpoint address is IPv6.")]
        public bool AllowDualStackSocket
        {
            get
            {
                return m_allowDualStackSocket;
            }
            set
            {
                m_allowDualStackSocket = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMaxSendQueueSize),
        Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize
        {
            get
            {
                return m_maxSendQueueSize;
            }
            set
            {
                m_maxSendQueueSize = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="UdpServer"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<Socket> Server
        {
            get
            {
                return m_udpServer;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                int count = 0;

                foreach (ConcurrentQueue<UdpServerPayload> sendQueue in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.SendQueue))
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0} for client {1}", sendQueue.Count, ++count);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("     Wait handle pool size: {0}", ReusableObjectPool<ManualResetEventSlim>.Default.GetPoolSize());
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("         Payload pool size: {0}", ReusableObjectPool<UdpServerPayload>.Default.GetPoolSize());
                statusBuilder.AppendLine();

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="clientID">ID of the client from which data buffer should be read.</param>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ServerBase.ReceiveClientData"/> event handler. Calling this method
        /// outside this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// No received data buffer has been defined to read -or-
        /// Specified <paramref name="clientID"/> does not exist, cannot read buffer.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(Guid clientID, byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            UdpClientInfo clientInfo;
            TransportProvider<EndPoint> udpClient;

            if (m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
            {
                udpClient = clientInfo.Client;

                if ((object)udpClient.ReceiveBuffer != null)
                {
                    int readIndex = ReadIndicies[clientID];
                    int sourceLength = udpClient.BytesReceived - readIndex;
                    int readBytes = length > sourceLength ? sourceLength : length;
                    Buffer.BlockCopy(udpClient.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

                    // Update read index for next call
                    readIndex += readBytes;

                    if (readIndex >= udpClient.BytesReceived)
                        readIndex = 0;

                    ReadIndicies[clientID] = readIndex;

                    return readBytes;
                }

                throw new InvalidOperationException("No received data buffer has been defined to read.");
            }

            throw new InvalidOperationException("Specified client ID does not exist, cannot read buffer.");
        }

        /// <summary>
        /// Saves <see cref="TcpServer"/> settings to the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["AllowDualStackSocket", true].Update(m_allowDualStackSocket);
                settings["MaxSendQueueSize", true].Update(m_maxSendQueueSize);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="TcpServer"/> settings from the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
            }
        }

        /// <summary>
        /// Stops the <see cref="UdpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            if (CurrentState == ServerState.Running)
            {
                try
                {
                    // Disconnect all clients.
                    DisconnectAll();
                    m_udpServer.Reset();
                    OnServerStopped();
                }
                finally
                {
                    if ((object)m_receiveArgs != null)
                    {
                        m_receiveArgs.Dispose();
                        m_receiveArgs = null;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="UdpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="UdpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState == ServerState.NotRunning)
            {
                int maxSendQueueSize;

                // Initialize if unitialized
                if (!Initialized)
                    Initialize();

                // Overwrite config file if max send queue size exists in connection string.
                if (m_configData.ContainsKey("maxSendQueueSize") && int.TryParse(m_configData["maxSendQueueSize"], out maxSendQueueSize))
                    m_maxSendQueueSize = maxSendQueueSize;

                // Bind server socket to local end-point
                m_udpServer = new TransportProvider<Socket>();
                m_udpServer.SetReceiveBuffer(ReceiveBufferSize);
                m_udpServer.Provider = Transport.CreateSocket(m_configData["interface"], int.Parse(m_configData["port"]), ProtocolType.Udp, m_ipStack, m_allowDualStackSocket);

                // Disable SocketError.ConnectionReset exception from being thrown when the endpoint is not listening
                m_udpServer.Provider.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                // Notify that the server has been started successfully
                OnServerStarted();

                if ((object)m_udpServer.Provider.LocalEndPoint != null)
                {
                    m_receiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    m_receiveArgs.SocketFlags = SocketFlags.None;
                    m_receiveArgs.Completed += m_receiveHandler;
                    ReceivePayloadAsync(m_receiveArgs);
                }

                // We process the static list to clients.
                foreach (string clientString in m_configData["clients"].Replace(" ", "").Split(','))
                {
                    try
                    {
                        Match endpoint = Regex.Match(clientString, Transport.EndpointFormatRegex);

                        if (endpoint != Match.Empty)
                        {
                            UdpClientInfo clientInfo;
                            TransportProvider<EndPoint> udpClient = new TransportProvider<EndPoint>();
                            IPEndPoint clientEndpoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);

                            udpClient.SetReceiveBuffer(ReceiveBufferSize);
                            udpClient.SetSendBuffer(SendBufferSize);
                            udpClient.Provider = clientEndpoint;

                            // If the IP specified for the client is a multicast IP, subscribe to the specified multicast group.
                            if (Transport.IsMulticastIP(clientEndpoint.Address))
                            {
                                string multicastSource;

                                if (m_configData.TryGetValue("multicastSource", out multicastSource))
                                {
                                    IPAddress sourceAddress = IPAddress.Parse(multicastSource);
                                    IPAddress localAddress = (clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any);

                                    if (sourceAddress.AddressFamily != clientEndpoint.AddressFamily)
                                        throw new InvalidOperationException(string.Format("Source address \"{0}\" is not in the same IP format as server address \"{1}\"", sourceAddress, clientEndpoint.Address));

                                    if (localAddress.AddressFamily != clientEndpoint.AddressFamily)
                                        throw new InvalidOperationException(string.Format("Local address \"{0}\" is not in the same IP format as server address \"{1}\"", localAddress, clientEndpoint.Address));

                                    MemoryStream membershipAddresses = new MemoryStream();

                                    byte[] serverAddressBytes = clientEndpoint.Address.GetAddressBytes();
                                    byte[] sourceAddressBytes = sourceAddress.GetAddressBytes();
                                    byte[] localAddressBytes = localAddress.GetAddressBytes();

                                    membershipAddresses.Write(serverAddressBytes, 0, serverAddressBytes.Length);
                                    membershipAddresses.Write(sourceAddressBytes, 0, sourceAddressBytes.Length);
                                    membershipAddresses.Write(localAddressBytes, 0, localAddressBytes.Length);

                                    udpClient.MulticastMembershipAddresses = membershipAddresses.ToArray();

                                    // Execute multicast subscribe for specific source
                                    SocketOptionLevel level = clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP;
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.AddSourceMembership, udpClient.MulticastMembershipAddresses);
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.MulticastTimeToLive, int.Parse(m_configData["multicastTimeToLive"]));
                                }
                                else
                                {
                                    // Execute multicast subscribe for any source
                                    SocketOptionLevel level = clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP;
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.AddMembership, new MulticastOption(clientEndpoint.Address));
                                    m_udpServer.Provider.SetSocketOption(level, SocketOptionName.MulticastTimeToLive, int.Parse(m_configData["multicastTimeToLive"]));
                                }
                            }

                            clientInfo = new UdpClientInfo()
                            {
                                Client = udpClient,
                                SendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction(),
                                SendLock = new SpinLock(),
                                SendQueue = new ConcurrentQueue<UdpServerPayload>()
                            };

                            clientInfo.SendArgs.RemoteEndPoint = udpClient.Provider;
                            clientInfo.SendArgs.SetBuffer(udpClient.SendBuffer, 0, udpClient.SendBufferSize);
                            clientInfo.SendArgs.Completed += m_sendHandler;

                            m_clientInfoLookup.TryAdd(udpClient.ID, clientInfo);
                            OnClientConnected(udpClient.ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = string.Format("Unable to connect to client {0}: {1}", clientString, ex.Message);
                        OnClientConnectingException(new Exception(errorMessage, ex));
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Server is currently running");
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            UdpClientInfo clientInfo;
            TransportProvider<EndPoint> client = null;

            try
            {
                if (!m_clientInfoLookup.TryRemove(clientID, out clientInfo))
                    return;

                client = clientInfo.Client;

                if ((object)client.Provider != null)
                {
                    // If the IP specified for the client is a multicast IP, unsubscribe from the specified multicast group.
                    IPEndPoint clientEndpoint = (IPEndPoint)client.Provider;

                    if (Transport.IsMulticastIP(clientEndpoint.Address))
                    {
                        if ((object)client.MulticastMembershipAddresses != null)
                        {
                            // Execute multicast unsubscribe for specific source
                            m_udpServer.Provider.SetSocketOption(clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropSourceMembership, client.MulticastMembershipAddresses);
                        }
                        else
                        {
                            // Execute multicast unsubscribe for any source
                            m_udpServer.Provider.SetSocketOption(clientEndpoint.AddressFamily == AddressFamily.InterNetworkV6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(clientEndpoint.Address));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, new InvalidOperationException(string.Format("Failed to drop multicast membership: {0}", ex.Message), ex));
            }

            if ((object)client != null)
                client.Reset();

            OnClientDisconnected(clientID);
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{EndPoint}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="udpClient">The UDP client.</param>
        /// <returns>An <see cref="TransportProvider{EndPoint}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public bool TryGetClient(Guid clientID, out TransportProvider<EndPoint> udpClient)
        {
            UdpClientInfo clientInfo;
            bool clientExists;

            clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                udpClient = clientInfo.Client;
            else
                udpClient = null;

            return clientExists;
        }

        /// <summary>
        /// Validates the specified <paramref name="configurationString"/>.
        /// </summary>
        /// <param name="configurationString">Configuration string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConfigurationString(string configurationString)
        {
            string setting;
            int value;

            m_configData = configurationString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_configData);

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port is missing (Example: {0})", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]) && int.Parse(m_configData["port"]) != -1)
                throw new ArgumentOutOfRangeException("configurationString", string.Format("Port number must be {0} or between {1} and {2}", -1, Transport.PortRangeLow, Transport.PortRangeHigh));

            if (!m_configData.ContainsKey("multicastTimeToLive"))
                m_configData.Add("multicastTimeToLive", "10");

            // Make sure a valid multi-cast time-to-live value is defined in the configuration data
            if (!(m_configData.TryGetValue("multicastTimeToLive", out setting) && int.TryParse(setting, out value)))
                m_configData["multicastTimeToLive"] = "10";
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataToAsync(Guid clientID, byte[] data, int offset, int length)
        {
            UdpClientInfo clientInfo;
            ConcurrentQueue<UdpServerPayload> sendQueue;
            UdpServerPayload payload;
            ManualResetEventSlim handle;

            UdpServerPayload dequeuedPayload;
            bool lockTaken = false;

            if (!m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
                throw new InvalidOperationException(string.Format("No client found for ID {0}.", clientID));

            sendQueue = clientInfo.SendQueue;

            // Check to see if the client has reached the maximum send queue size.
            if (m_maxSendQueueSize > 0 && sendQueue.Count >= m_maxSendQueueSize)
            {
                for (int i = 0; i < m_maxSendQueueSize; i++)
                {
                    if (sendQueue.TryDequeue(out payload))
                    {
                        payload.WaitHandle.Set();
                        payload.WaitHandle.Dispose();
                        payload.WaitHandle = null;
                    }
                }

                throw new InvalidOperationException(string.Format("Client {0} connected to UDP server reached maximum send queue size. {1} payloads dumped from the queue.", clientID, m_maxSendQueueSize));
            }

            // Create payload and wait handle.
            payload = ReusableObjectPool<UdpServerPayload>.Default.TakeObject();
            handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            payload.ClientInfo = clientInfo;
            handle.Reset();

            // Queue payload for sending.
            sendQueue.Enqueue(payload);

            try
            {
                clientInfo.SendLock.Enter(ref lockTaken);

                // Send next queued payload.
                if (Interlocked.CompareExchange(ref clientInfo.Sending, 1, 0) == 0)
                {
                    if (sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
            finally
            {
                if (lockTaken)
                    clientInfo.SendLock.Exit();
            }

            // Notify that the send operation has started.
            OnSendClientDataStart(clientID);

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle.WaitHandle;
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.SendClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        protected override void OnSendClientDataException(Guid clientID, Exception ex)
        {
            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
                base.OnSendClientDataException(clientID, ex);
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.ReceiveClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        protected override void OnReceiveClientDataException(Guid clientID, Exception ex)
        {
            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
                base.OnReceiveClientDataException(clientID, ex);
        }

        /// <summary>
        /// Loops waiting for payloads and sends them on the socket.
        /// </summary>
        private void SendPayload(UdpServerPayload payload)
        {
            UdpClientInfo clientInfo = null;
            TransportProvider<EndPoint> client = null;
            SocketAsyncEventArgs args;
            ManualResetEventSlim handle;
            int copyLength;

            try
            {
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                args = clientInfo.SendArgs;
                handle = payload.WaitHandle;
                args.UserToken = payload;

                // Copy payload into send buffer.
                copyLength = Math.Min(payload.Length, client.SendBufferSize);
                Buffer.BlockCopy(payload.Data, payload.Offset, client.SendBuffer, 0, copyLength);

                // Set buffer and user token of send args.
                args.SetBuffer(0, copyLength);

                // Update offset and length.
                payload.Offset += copyLength;
                payload.Length -= copyLength;

                // Send data over socket.
                if (!m_udpServer.Provider.SendToAsync(args))
                    ProcessSend(args);
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);

                if ((object)clientInfo != null)
                {
                    // Assume process send was not able
                    // to continue the asynchronous loop.
                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs args)
        {
            UdpServerPayload payload = null;
            UdpClientInfo clientInfo = null;
            TransportProvider<EndPoint> client = null;
            ConcurrentQueue<UdpServerPayload> sendQueue = null;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

            try
            {
                payload = (UdpServerPayload)args.UserToken;
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                sendQueue = clientInfo.SendQueue;
                handle = payload.WaitHandle;

                // Determine whether we are finished with this
                // payload and, if so, set the wait handle.
                if (payload.Length <= 0)
                    handle.Set();

                // Check for errors during send operation.
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                // Update statistics on the client.
                client.Statistics.UpdateBytesSent(args.BytesTransferred);

                // Send operation is complete.
                if (payload.Length <= 0)
                    OnSendClientDataComplete(client.ID);
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);
            }
            finally
            {
                try
                {
                    if (payload.Length > 0)
                    {
                        // Still more to send for this payload.
                        ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                    }
                    else
                    {
                        payload.WaitHandle = null;
                        payload.ClientInfo = null;

                        // Return payload and wait handle to their respective object pools.
                        ReusableObjectPool<UdpServerPayload>.Default.ReturnObject(payload);
                        ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                        // Begin sending next client payload.
                        if (sendQueue.TryDequeue(out payload))
                        {
                            ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                        }
                        else
                        {
                            try
                            {
                                clientInfo.SendLock.Enter(ref lockTaken);

                                if (sendQueue.TryDequeue(out payload))
                                    ThreadPool.QueueUserWorkItem(state => SendPayload((UdpServerPayload)state), payload);
                                else
                                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                            }
                            finally
                            {
                                if (lockTaken)
                                    clientInfo.SendLock.Exit();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);

                    if ((object)client != null)
                        OnSendClientDataException(client.ID, new Exception(errorMessage, ex));

                    if ((object)clientInfo != null)
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ReceivePayloadAsync(SocketAsyncEventArgs args)
        {
            // Attempt to receive data on the socket.
            byte[] buffer = m_udpServer.ReceiveBuffer;
            int length = m_udpServer.ReceiveBufferSize;

            args.SetBuffer(buffer, 0, length);
            args.RemoteEndPoint = m_udpServer.Provider.LocalEndPoint;

            if (!m_udpServer.Provider.ReceiveFromAsync(args))
                ThreadPool.QueueUserWorkItem(state => ProcessReceive((SocketAsyncEventArgs)state), args);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            Guid clientID = default(Guid);

            try
            {
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                // Update statistics and pointers.
                m_udpServer.Statistics.UpdateBytesReceived(args.BytesTransferred);
                m_udpServer.BytesReceived = args.BytesTransferred;

                // Search connected clients for a client connected to the end-point from where this data is received.
                foreach (TransportProvider<EndPoint> client in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.Client))
                {
                    if (client.Provider.Equals(args.RemoteEndPoint))
                    {
                        // Found a match, notify of data.
                        clientID = client.ID;
                        client.Statistics.UpdateBytesReceived(args.BytesTransferred);
                        OnReceiveClientDataComplete(client.ID, m_udpServer.ReceiveBuffer, m_udpServer.BytesReceived);
                        break;
                    }
                }

                // Resume receive operation on the server socket.
                ReceivePayloadAsync(args);
            }
            catch (Exception ex)
            {
                OnReceiveClientDataException(clientID, ex);
            }
        }

        #endregion
    }
}