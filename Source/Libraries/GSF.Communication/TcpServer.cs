//******************************************************************************************************
//  TcpServer.cs - Gbtc
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
//  06/02/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access.
//  12/01/2006 - Pinal C. Patel
//       Modified code for handling "PayloadAware" transmissions.
//  01/28/3008 - J. Ritchie Carroll
//       Placed accepted TCP socket connections on their own threads instead of thread pool.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/17/2009 - Pinal C. Patel
//       Added support to specify a specific interface address on a multiple interface machine.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/14/2009 - Pinal C. Patel
//       Added null reference check to DisconnectOne() for safety.
//  02/11/2011 - Pinal C. Patel
//       Added IntegratedSecurity property to enable integrated windows authentication.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
#if !MONO
using System.Net.Security;
using System.Security.Authentication;
#endif
using GSF.Configuration;
using GSF.Threading;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a TCP-based communication server.
    /// </summary>
    /// <remarks>
    /// The <see cref="TcpServer.Server"/> socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the interface in the <see cref="ServerBase.ConfigurationString"/> (Example: "Port=8888; Interface=127.0.0.1")
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="TcpServer"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// using GSF.Security.Cryptography;
    /// using GSF.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static TcpServer m_server;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the server.
    ///         m_server = new TcpServer("Port=8888");
    ///         m_server.Handshake = false;
    ///         m_server.PayloadAware = false;
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
    public class TcpServer : ServerBase
    {
        #region [ Members ]

        // Nested Types
        private class TcpClientInfo
        {
            public TransportProvider<Socket> Client;
            public SocketAsyncEventArgs SendArgs;
            public object SendLock;
            public ConcurrentQueue<TcpServerPayload> SendQueue;
            public ShortSynchronizedOperation DumpPayloadsOperation;
            public int Sending;

            public WindowsPrincipal ClientPrincipal;
        }

        private class TcpServerPayload
        {
            // Per payload state
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;

            // Per client state
            public TcpClientInfo ClientInfo;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        /// <summary>
        /// Specifies the default value for the <see cref="IntegratedSecurity"/> property.
        /// </summary>
        public const bool DefaultIntegratedSecurity = false;

        /// <summary>
        /// Specifies the default value for the <see cref="IgnoreInvalidCredentials"/> property.
        /// </summary>
        public const bool DefaultIgnoreInvalidCredentials = false;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = 500000;

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888";

        // Fields
        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private bool m_integratedSecurity;
        private bool m_ignoreInvalidCredentials;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Socket m_tcpServer;
        private SocketAsyncEventArgs m_acceptArgs;
        private readonly ConcurrentDictionary<Guid, TcpClientInfo> m_clientInfoLookup;
        private Dictionary<string, string> m_configData;

        private readonly EventHandler<SocketAsyncEventArgs> m_acceptHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_sendHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_receivePayloadAwareHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_receivePayloadUnawareHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TcpServer()
            : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="TcpServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public TcpServer(string configString)
            : base(TransportProtocol.Tcp, configString)
        {
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_integratedSecurity = DefaultIntegratedSecurity;
            m_ignoreInvalidCredentials = DefaultIgnoreInvalidCredentials;
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_clientInfoLookup = new ConcurrentDictionary<Guid, TcpClientInfo>();

            m_acceptHandler = (sender, args) => ProcessAccept(args);
            m_sendHandler = (sender, args) => ProcessSend(args);
            m_receivePayloadAwareHandler = (sender, args) => ProcessReceivePayloadAware(args);
            m_receivePayloadUnawareHandler = (sender, args) => ProcessReceivePayloadUnaware(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpServer"/>.</param>
        public TcpServer(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultPayloadAware),
        Description("Indicates whether the payload boundaries are to be preserved during transmission.")]
        public bool PayloadAware
        {
            get
            {
                return m_payloadAware;
            }
            set
            {
                m_payloadAware = value;
            }
        }

        /// <summary>
        /// Gets or sets the byte sequence used to mark the beginning of a payload in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty buffer.</exception>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] PayloadMarker
        {
            get
            {
                return m_payloadMarker;
            }
            set
            {
                if ((object)value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(value));

                m_payloadMarker = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client Windows account credentials are used for authentication.
        /// </summary>
        /// <remarks>   
        /// This option is ignored under Mono deployments.
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultIntegratedSecurity),
        Description("Indicates whether the client Windows account credentials are used for authentication.")]
        public bool IntegratedSecurity
        {
            get
            {
                return m_integratedSecurity;
            }
            set
            {
#if MONO
                if (value)
                    throw new NotImplementedException("Not supported under Mono.");
#else
                m_integratedSecurity = value;
#endif
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server
        /// should ignore errors when the client's credentials are invalid.
        /// </summary>
        /// <remarks>
        /// This property should only be set to true if there is an alternative by which
        /// to authenticate the client when integrated security fails. When this is set
        /// to true, if the client's credentials are invalid, the <see cref="TryGetClientPrincipal"/>
        /// method will return true for that client, but the principal will still be null.
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultIgnoreInvalidCredentials),
        Description("Indicates whether the client Windows account credentials are validated during authentication.")]
        public bool IgnoreInvalidCredentials
        {
            get
            {
                return m_ignoreInvalidCredentials;
            }
            set
            {
                m_ignoreInvalidCredentials = value;
            }
        }

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
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpServer"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Server
        {
            get
            {
                return m_tcpServer;
            }
        }

        /// <summary>
        /// Gets the receive handler used to handle data received on the connected sockets.
        /// </summary>
        private EventHandler<SocketAsyncEventArgs> ReceiveHandler
        {
            get
            {
                return m_payloadAware ? m_receivePayloadAwareHandler : m_receivePayloadUnawareHandler;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the server.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                int count = 0;

                foreach (ConcurrentQueue<TcpServerPayload> sendQueue in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.SendQueue))
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0} for client {1}", sendQueue.Count, ++count);
                    statusBuilder.AppendLine();
                }

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

            TcpClientInfo clientInfo;
            TransportProvider<Socket> tcpClient;

            if (m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
            {
                tcpClient = clientInfo.Client;

                if ((object)tcpClient.ReceiveBuffer != null)
                {
                    int readIndex = ReadIndicies[clientID];
                    int sourceLength = tcpClient.BytesReceived - readIndex;
                    int readBytes = length > sourceLength ? sourceLength : length;
                    Buffer.BlockCopy(tcpClient.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

                    // Update read index for next call
                    readIndex += readBytes;

                    if (readIndex >= tcpClient.BytesReceived)
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
                settings["PayloadAware", true].Update(m_payloadAware);
                settings["IntegratedSecurity", true].Update(m_integratedSecurity);
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
                settings.Add("PayloadAware", m_payloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
                settings.Add("IntegratedSecurity", m_integratedSecurity, "True if the client Windows account credentials are used for authentication, otherwise False.");
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
                PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                IntegratedSecurity = settings["IntegratedSecurity"].ValueAs(m_integratedSecurity);
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
            }
        }

        /// <summary>
        /// Stops the <see cref="TcpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            SocketAsyncEventArgs acceptArgs = m_acceptArgs;

            m_acceptArgs = null;

            if (CurrentState == ServerState.Running)
            {
                DisconnectAll();        // Disconnection all clients.
                m_tcpServer.Close();    // Stop accepting new connections.

                // Clean up accept args.
                acceptArgs?.Dispose();

                OnServerStopped();
            }
        }

        /// <summary>
        /// Starts the <see cref="TcpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="TcpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState == ServerState.NotRunning)
            {
                string integratedSecuritySetting;
                int maxClientConnections, maxSendQueueSize;

                // Initialize if unitialized.
                if (!Initialized)
                    Initialize();

                // Overwrite config file if integrated security exists in connection string.
                if (m_configData.TryGetValue("integratedSecurity", out integratedSecuritySetting))
                    m_integratedSecurity = integratedSecuritySetting.ParseBoolean();

#if MONO
                // Force integrated security to be False under Mono since it's not supported
                m_integratedSecurity = false;
#endif

                // Overwrite config file if max client connections exists in connection string.
                if (m_configData.ContainsKey("maxClientConnections") && int.TryParse(m_configData["maxClientConnections"], out maxClientConnections))
                    MaxClientConnections = maxClientConnections;

                // Overwrite config file if max send queue size exists in connection string.
                if (m_configData.ContainsKey("maxSendQueueSize") && int.TryParse(m_configData["maxSendQueueSize"], out maxSendQueueSize))
                    m_maxSendQueueSize = maxSendQueueSize;

                // Bind server socket to local end-point and listen.
                m_tcpServer = Transport.CreateSocket(m_configData["interface"], int.Parse(m_configData["port"]), ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);
                m_tcpServer.Listen(1);

                // Begin accepting incoming connection asynchronously.
                m_acceptArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();

                m_acceptArgs.AcceptSocket = null;
                m_acceptArgs.SetBuffer(null, 0, 0);
                m_acceptArgs.SocketFlags = SocketFlags.None;
                m_acceptArgs.Completed += m_acceptHandler;

                if (!m_tcpServer.AcceptAsync(m_acceptArgs))
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept((SocketAsyncEventArgs)state), m_acceptArgs);

                // Notify that the server has been started successfully.
                OnServerStarted();
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            TransportProvider<Socket> client;

            if (!TryGetClient(clientID, out client))
                return;

            try
            {
                if ((object)client.Provider != null && client.Provider.Connected)
                    client.Provider.Disconnect(false);

                OnClientDisconnected(clientID);
                client.Reset();
            }
            catch (Exception ex)
            {
                OnSendClientDataException(client.ID, new InvalidOperationException(string.Format("Client disconnection exception: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{Socket}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="tcpClient">The TCP client.</param>
        /// <returns>A <see cref="TransportProvider{Socket}"/> object.</returns>
        public bool TryGetClient(Guid clientID, out TransportProvider<Socket> tcpClient)
        {
            TcpClientInfo clientInfo;
            bool clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                tcpClient = clientInfo.Client;
            else
                tcpClient = null;

            return clientExists;
        }

        /// <summary>
        /// Gets the <see cref="WindowsPrincipal"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="clientPrincipal">The principal of the client.</param>
        /// <returns>A <see cref="WindowsPrincipal"/> object.</returns>
        public bool TryGetClientPrincipal(Guid clientID, out WindowsPrincipal clientPrincipal)
        {
            TcpClientInfo clientInfo;
            bool clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                clientPrincipal = clientInfo.ClientPrincipal;
            else
                clientPrincipal = null;

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
            m_configData = configurationString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_configData);

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing (Example: {0})", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]))
                throw new ArgumentOutOfRangeException(nameof(configurationString), string.Format("Port number must be between {0} and {1}", Transport.PortRangeLow, Transport.PortRangeHigh));
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
            TcpClientInfo clientInfo;
            ConcurrentQueue<TcpServerPayload> sendQueue;
            TcpServerPayload dequeuedPayload;

            TcpServerPayload payload;
            ManualResetEventSlim handle;

            if (!m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
                throw new InvalidOperationException(string.Format("No client found for ID {0}.", clientID));

            sendQueue = clientInfo.SendQueue;

            // Execute operation to see if the client has reached the maximum send queue size.
            clientInfo.DumpPayloadsOperation.TryRun();

            // Prepare for payload-aware transmission.
            if (m_payloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

            // Create payload and wait handle.
            payload = FastObjectFactory<TcpServerPayload>.CreateObjectFunction();
            handle = FastObjectFactory<ManualResetEventSlim>.CreateObjectFunction();

            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            payload.ClientInfo = clientInfo;
            handle.Reset();

            // Queue payload for sending.
            sendQueue.Enqueue(payload);

            lock (clientInfo.SendLock)
            {
                // Send next queued payload.
                if (Interlocked.CompareExchange(ref clientInfo.Sending, 1, 0) == 0)
                {
                    if (sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((TcpServerPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
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
        protected virtual void OnReceiveClientDataException(Guid clientID, SocketException ex)
        {
            if (m_clientInfoLookup.ContainsKey(clientID) && ex.SocketErrorCode != SocketError.Disconnecting)
                OnReceiveClientDataException(clientID, (Exception)ex);
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
        /// Callback method for asynchronous accept operation.
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs acceptArgs)
        {
            TransportProvider<Socket> client = new TransportProvider<Socket>();
            SocketAsyncEventArgs receiveArgs = null;
            WindowsPrincipal clientPrincipal = null;
            TcpClientInfo clientInfo;

            try
            {
                if (CurrentState == ServerState.NotRunning)
                    return;

                // If acceptArgs was disposed, m_acceptArgs will either
                // be null or another instance of SocketAsyncEventArgs.
                // This check will tell us whether it's been disposed.
                if ((object)acceptArgs != m_acceptArgs)
                    return;

                if (acceptArgs.SocketError != SocketError.Success)
                {
                    // Error is unrecoverable.
                    // We need to make sure to restart the
                    // server before we throw the error.
                    SocketError error = acceptArgs.SocketError;
                    ThreadPool.QueueUserWorkItem(state => ReStart());
                    throw new SocketException((int)error);
                }

                // Process the newly connected client.
                client.Provider = acceptArgs.AcceptSocket;
                client.Provider.ReceiveBufferSize = ReceiveBufferSize;

                // Set up SocketAsyncEventArgs for receive operations.
                receiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                receiveArgs.Completed += ReceiveHandler;

                // Return to accepting new connections.
                acceptArgs.AcceptSocket = null;

                if (!m_tcpServer.AcceptAsync(acceptArgs))
                {
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept(acceptArgs));
                }

#if !MONO
                // Authenticate the connected client Windows credentials.
                if (m_integratedSecurity)
                {
                    NetworkStream socketStream = null;
                    NegotiateStream authenticationStream = null;
                    try
                    {
                        socketStream = new NetworkStream(client.Provider);
                        authenticationStream = new NegotiateStream(socketStream);
                        authenticationStream.AuthenticateAsServer();

                        if (authenticationStream.RemoteIdentity is WindowsIdentity)
                            clientPrincipal = new WindowsPrincipal((WindowsIdentity)authenticationStream.RemoteIdentity);
                    }
                    catch (InvalidCredentialException)
                    {
                        if (!m_ignoreInvalidCredentials)
                            throw;
                    }
                    finally
                    {
                        if (socketStream != null)
                            socketStream.Dispose();

                        if (authenticationStream != null)
                            authenticationStream.Dispose();
                    }
                }
#endif

                if (MaxClientConnections != -1 && ClientIDs.Length >= MaxClientConnections)
                {
                    // Reject client connection since limit has been reached.
                    TerminateConnection(client, receiveArgs, false);
                }
                else
                {
                    // We can proceed further with receiving data from the client.
                    clientInfo = new TcpClientInfo
                    {
                        Client = client,
                        SendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction(),
                        SendLock = new object(),
                        SendQueue = new ConcurrentQueue<TcpServerPayload>(),
                        ClientPrincipal = clientPrincipal
                    };

                    // Create operation to dump send queue payloads when the queue grows too large.
                    clientInfo.DumpPayloadsOperation = new ShortSynchronizedOperation(() =>
                    {
                        TcpServerPayload payload;

                        // Check to see if the client has reached the maximum send queue size.
                        if (m_maxSendQueueSize > 0 && clientInfo.SendQueue.Count >= m_maxSendQueueSize)
                        {
                            for (int i = 0; i < m_maxSendQueueSize; i++)
                            {
                                if (clientInfo.SendQueue.TryDequeue(out payload))
                                {
                                    payload.WaitHandle.Set();
                                    payload.WaitHandle.Dispose();
                                    payload.WaitHandle = null;
                                }
                            }

                            throw new InvalidOperationException(string.Format("Client {0} connected to TCP server reached maximum send queue size. {1} payloads dumped from the queue.", clientInfo.Client.ID, m_maxSendQueueSize));
                        }
                    }, ex => OnSendClientDataException(clientInfo.Client.ID, ex));

                    // Set up socket args.
                    client.SetSendBuffer(SendBufferSize);
                    clientInfo.SendArgs.Completed += m_sendHandler;
                    clientInfo.SendArgs.SetBuffer(client.SendBuffer, 0, client.SendBufferSize);

                    m_clientInfoLookup.TryAdd(client.ID, clientInfo);

                    OnClientConnected(client.ID);

                    if (!m_payloadAware)
                    {
                        receiveArgs.UserToken = client;
                    }
                    else
                    {
                        EventArgs<TransportProvider<Socket>, bool> userToken = FastObjectFactory<EventArgs<TransportProvider<Socket>, bool>>.CreateObjectFunction();
                        userToken.Argument1 = client;
                        receiveArgs.UserToken = userToken;
                    }

                    ReceivePayloadAsync(client, receiveArgs);
                }
            }
            catch (ObjectDisposedException)
            {
                // m_acceptArgs may be disposed while in the middle of accepting a connection
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if ((object)client.Provider != null && (object)client.Provider.RemoteEndPoint != null)
                {
                    string clientAddress = ((IPEndPoint)client.Provider.RemoteEndPoint).Address.ToString();
                    string errorMessage = string.Format("Unable to accept connection to client [{0}]: {1}", clientAddress, ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }
                else
                {
                    string errorMessage = string.Format("Unable to accept connection to client [unknown]: {0}", ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }

                if ((object)receiveArgs != null)
                {
                    TerminateConnection(client, receiveArgs, false);
                }
            }
        }

        /// <summary>
        /// Asynchronous loop sends payloads on the socket.
        /// </summary>
        private void SendPayload(TcpServerPayload payload)
        {
            TcpClientInfo clientInfo = null;
            TransportProvider<Socket> client = null;
            SocketAsyncEventArgs args;
            //ManualResetEventSlim handle;
            int copyLength;

            try
            {
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                args = clientInfo.SendArgs;
                args.UserToken = payload;
                //handle = payload.WaitHandle;

                // Copy payload into send buffer.
                copyLength = Math.Min(payload.Length, client.SendBufferSize);
                Buffer.BlockCopy(payload.Data, payload.Offset, client.SendBuffer, 0, copyLength);

                // Set buffer and user token of send args.
                args.SetBuffer(0, copyLength);

                // Update offset and length.
                payload.Offset += copyLength;
                payload.Length -= copyLength;

                // Send data over socket.
                if (!client.Provider.SendAsync(args))
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
            TcpServerPayload payload = null;
            TcpClientInfo clientInfo = null;
            TransportProvider<Socket> client = null;
            ConcurrentQueue<TcpServerPayload> sendQueue = null;
            ManualResetEventSlim handle = null;

            try
            {
                payload = (TcpServerPayload)args.UserToken;
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
                if ((object)payload != null)
                {
                    try
                    {
                        if (payload.Length > 0)
                        {
                            // Still more to send for this payload.
                            ThreadPool.QueueUserWorkItem(state => SendPayload((TcpServerPayload)state), payload);
                        }
                        else if ((object)sendQueue != null)
                        {
                            payload.WaitHandle = null;
                            payload.ClientInfo = null;

                            // Begin sending next client payload.
                            if (sendQueue.TryDequeue(out payload))
                            {
                                ThreadPool.QueueUserWorkItem(state => SendPayload((TcpServerPayload)state), payload);
                            }
                            else if ((object)clientInfo != null)
                            {
                                lock (clientInfo.SendLock)
                                {
                                    if (sendQueue.TryDequeue(out payload))
                                        ThreadPool.QueueUserWorkItem(state => SendPayload((TcpServerPayload)state), payload);
                                    else
                                        Interlocked.Exchange(ref clientInfo.Sending, 0);
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
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync(TransportProvider<Socket> client, SocketAsyncEventArgs args)
        {
            // Initialize bytes received.
            client.BytesReceived = 0;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Set user token to indicate we are waiting for payload header.
                EventArgs<TransportProvider<Socket>, bool> userToken = (EventArgs<TransportProvider<Socket>, bool>)args.UserToken;
                userToken.Argument2 = true;

                // Payload boundaries are to be preserved.
                client.SetReceiveBuffer(m_payloadMarker.Length + Payload.LengthSegment);
                ReceivePayloadAwareAsync(client, args);
            }
            else
            {
                // Payload boundaries are not to be preserved.
                client.SetReceiveBuffer(ReceiveBufferSize);
                ReceivePayloadUnawareAsync(client, args);
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(TransportProvider<Socket> client, SocketAsyncEventArgs args)
        {
            byte[] buffer = client.ReceiveBuffer;
            int offset = client.BytesReceived;
            int length = client.ReceiveBufferSize - offset;

            args.SetBuffer(buffer, offset, length);

            if (!client.Provider.ReceiveAsync(args))
                ThreadPool.QueueUserWorkItem(state => ProcessReceivePayloadAware((SocketAsyncEventArgs)state), args);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(SocketAsyncEventArgs args)
        {
            EventArgs<TransportProvider<Socket>, bool> userToken = (EventArgs<TransportProvider<Socket>, bool>)args.UserToken;
            TransportProvider<Socket> client = userToken.Argument1;

            try
            {
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                if (args.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(args.BytesTransferred);
                client.BytesReceived += args.BytesTransferred;

                if (userToken.Argument2)
                {
                    // We're waiting on the payload length, so we'll check if the received data has this information.
                    int payloadLength = Payload.ExtractLength(client.ReceiveBuffer, client.BytesReceived, m_payloadMarker);

                    // We have the payload length.
                    // If it is set to zero, there is no payload; wait for another header.
                    // Otherwise we'll create a buffer that's big enough to hold the entire payload.
                    if (payloadLength == 0)
                    {
                        client.BytesReceived = 0;
                    }
                    else if (payloadLength != -1)
                    {
                        client.BytesReceived = 0;
                        client.SetReceiveBuffer(payloadLength);
                        userToken.Argument2 = false;
                    }

                    ReceivePayloadAwareAsync(client, args);
                }
                else
                {
                    // We're accumulating the payload in the receive buffer until the entire payload is received.
                    if (client.BytesReceived == client.ReceiveBufferSize)
                    {
                        // We've received the entire payload.
                        OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                        ReceivePayloadAsync(client, args);
                    }
                    else
                    {
                        // We've not yet received the entire payload.
                        ReceivePayloadAwareAsync(client, args);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, args, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, args, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client, args);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, args, true);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync(TransportProvider<Socket> client, SocketAsyncEventArgs args)
        {
            byte[] buffer = client.ReceiveBuffer;
            int length = client.ReceiveBufferSize;

            args.SetBuffer(buffer, 0, length);

            if (!client.Provider.ReceiveAsync(args))
                ThreadPool.QueueUserWorkItem(state => ProcessReceivePayloadUnaware((SocketAsyncEventArgs)state), args);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(SocketAsyncEventArgs args)
        {
            TransportProvider<Socket> client = (TransportProvider<Socket>)args.UserToken;

            try
            {
                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                if (args.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(args.BytesTransferred);
                client.BytesReceived = args.BytesTransferred;

                // Notify of received data and resume receive operation.
                OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                ReceivePayloadUnawareAsync(client, args);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, args, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, args, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client, args);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, args, true);
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<Socket> client, SocketAsyncEventArgs args, bool raiseEvent)
        {
            TcpClientInfo clientInfo;

            try
            {
                if (m_clientInfoLookup.TryRemove(client.ID, out clientInfo))
                {
                    client.Reset();
                    clientInfo.SendArgs.Dispose();
                }

                if (raiseEvent)
                    OnClientDisconnected(client.ID);
            }
            finally
            {
                args.Dispose();
            }
        }

        #endregion
    }
}