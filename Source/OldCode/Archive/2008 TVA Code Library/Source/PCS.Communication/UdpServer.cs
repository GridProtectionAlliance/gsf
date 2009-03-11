//*******************************************************************************************************
//  UdpServer.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PCS.Security.Cryptography;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// Use <see cref="UdpServer"/> when the primary purpose is to transmit data.
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="UdpServer"/> component:
    /// <code>
    /// using System;
    /// using PCS.Communication;
    /// using PCS.Security.Cryptography;
    /// using PCS.IO.Compression;
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

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ReceiveBufferSize"/> property.
        /// </summary>
        public new const int DefaultReceiveBufferSize = 32768;

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
        private Dictionary<Guid, TransportProvider<Socket>> m_udpClients;
        private EndPoint m_udpClientEndPoint;
        private Dictionary<string, string> m_configData;
        private Func<TransportProvider<Socket>, bool> m_receivedGoodbye;

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
            base.ReceiveBufferSize = DefaultReceiveBufferSize;
            m_udpClients = new Dictionary<Guid, TransportProvider<Socket>>();
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Stops the <see cref="UdpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            if (CurrentState == ServerState.Running)
            {
                DisconnectAll();                // Disconnection all clients.
                m_udpServer.Provider.Close();   // Stop accepting new connections.
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
                // Initialize if unitialized.
                Initialize();
                // Create end-point for receiving data.
                m_udpClientEndPoint = Transport.CreateEndPoint(string.Empty, 0);
                // Bind server socket to local end-point.
                m_udpServer = new TransportProvider<Socket>();
                m_udpServer.ID = this.ServerID;
                m_udpServer.ReceiveBuffer = new byte[ReceiveBufferSize];
                m_udpServer.Provider = Transport.CreateSocket(int.Parse(m_configData["port"]), ProtocolType.Udp);
                // Notify that the server has been started successfully.
                OnServerStarted();

                if (Handshake)
                {
                    m_receivedGoodbye = DoGoodbyeCheck;
                    ReceiveHandshakeAsync(m_udpServer);
                }
                else
                {
                    m_receivedGoodbye = NoGoodbyeCheck;
                    ReceivePayloadAnyAsync(m_udpServer);

                    // When handshake is not to be performed, we process the static list to clients.
                    foreach (string clientString in m_configData["clients"].Replace(" ", "").Split(','))
                    {
                        try
                        {
                            string[] clientStringSegments = clientString.Split(':');

                            if (clientStringSegments.Length == 2)
                            {
                                TransportProvider<Socket> udpClient = new TransportProvider<Socket>();
                                udpClient.Passphrase = HandshakePassphrase;
                                udpClient.ReceiveBuffer = new byte[ReceiveBufferSize];
                                udpClient.Provider = Transport.CreateSocket(0, ProtocolType.Udp);
                                // Disable SocketError.ConnectionReset exception from being thrown when the enpoint is not listening.
                                udpClient.Provider.IOControl(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                                // Connect socket to the client endpoint so communication on the socket is restricted to a single endpoint.
                                udpClient.Provider.Connect(Transport.CreateEndPoint(clientStringSegments[0], int.Parse(clientStringSegments[1])));

                                lock (m_udpClients)
                                {
                                    m_udpClients.Add(udpClient.ID, udpClient);
                                }
                                OnClientConnected(udpClient.ID);

                                ReceivePayloadOneAsync(udpClient);
                            }
                        }
                        catch
                        {
                            // Ignore invalid client entries.
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Server is currently running.");
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            TransportProvider<Socket> udpClient = Client(clientID);
            if (Handshake)
            {
                // Handshake is enabled so we'll notify the client.
                udpClient.SendBuffer = new GoodbyeMessage(udpClient.ID).BinaryImage;
                udpClient.SendBufferOffset = 0;
                udpClient.SendBufferLength = udpClient.SendBuffer.Length;
                Payload.ProcessTransmit(ref udpClient.SendBuffer, ref udpClient.SendBufferOffset, ref udpClient.SendBufferLength, Encryption, udpClient.Passphrase, Compression);

                udpClient.Provider.SendTo(udpClient.SendBuffer, udpClient.Provider.RemoteEndPoint);
            }

            udpClient.Provider.Close();
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{EndPoint}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <returns>An <see cref="TransportProvider{EndPoint}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public TransportProvider<Socket> Client(Guid clientID)
        {
            TransportProvider<Socket> udpClient;
            lock (m_udpClients)
            {
                if (m_udpClients.TryGetValue(clientID, out udpClient))
                    return udpClient;
                else
                    throw new InvalidOperationException(string.Format("No client exists for Client ID \"{0}\".", clientID));
            }
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

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port is missing. Example: {0}.", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]))
                throw new ArgumentOutOfRangeException("configurationString", string.Format("Port number must between {0} and {1}.", Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Gets the passphrase to be used for ciphering client data.
        /// </summary>
        /// <param name="clientID">ID of the client whose passphrase is to be retrieved.</param>
        /// <returns>Cipher passphrase of the client with the specified <paramref name="clientID"/>.</returns>
        protected override string GetSessionPassphrase(Guid clientID)
        {
            return Client(clientID).Passphrase;
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
            WaitHandle handle;
            TransportProvider<Socket> udpClient = Client(clientID);

            // Send payload to the client asynchronously.
            handle = udpClient.Provider.BeginSendTo(data, offset, length, SocketFlags.None, udpClient.Provider.RemoteEndPoint, SendPayloadAsyncCallback, udpClient).AsyncWaitHandle;

            // Notify that the send operation has started.
            udpClient.SendBuffer = data;
            udpClient.SendBufferOffset = offset;
            udpClient.SendBufferLength = length;
            OnSendClientDataStart(udpClient.ID);

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendPayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            try
            {
                // Send operation is complete.
                udpClient.Statistics.UpdateBytesSent(udpClient.Provider.EndSendTo(asyncResult));
                OnSendClientDataComplete(udpClient.ID);
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendClientDataException(udpClient.ID, ex);
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of handshake data.
        /// </summary>
        private void ReceiveHandshakeAsync(TransportProvider<Socket> worker)
        {
            // Receive data asynchronously.
            EndPoint client = Transport.CreateEndPoint(string.Empty, 0);
            worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                             worker.ReceiveBufferOffset,
                                             worker.ReceiveBuffer.Length,
                                             SocketFlags.None,
                                             ref client,
                                             ReceiveHandshakeAsyncCallback,
                                             worker);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of handshake data.
        /// </summary>
        private void ReceiveHandshakeAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpServer = (TransportProvider<Socket>)asyncResult.AsyncState;
            // Received handshake data from client so we'll process it.
            try
            {
                // Update statistics and pointers.
                EndPoint client = Transport.CreateEndPoint(string.Empty, 0);
                udpServer.Statistics.UpdateBytesReceived(udpServer.Provider.EndReceiveFrom(asyncResult, ref client));
                udpServer.ReceiveBufferLength = udpServer.Statistics.LastBytesReceived;

                // Process the received handshake message.
                Payload.ProcessReceived(ref udpServer.ReceiveBuffer, ref udpServer.ReceiveBufferOffset, ref udpServer.ReceiveBufferLength, Encryption, HandshakePassphrase, Compression);

                HandshakeMessage handshake = new HandshakeMessage();
                if (handshake.Initialize(udpServer.ReceiveBuffer, udpServer.ReceiveBufferOffset, udpServer.ReceiveBufferLength) != -1)
                {
                    // Received handshake message could be parsed successfully.
                    if (handshake.ID != Guid.Empty && handshake.Passphrase == HandshakePassphrase)
                    {
                        // Create a random socket and connect it to the client.
                        TransportProvider<Socket> udpClient = new TransportProvider<Socket>();
                        udpClient.ReceiveBuffer = new byte[ReceiveBufferSize];
                        udpClient.Passphrase = HandshakePassphrase;
                        udpClient.Provider = Transport.CreateSocket(0, ProtocolType.Udp);
                        udpClient.Provider.Connect(client);

                        // Authentication is successful; respond to the handshake.
                        udpClient.ID = handshake.ID;
                        handshake.ID = this.ServerID;
                        if (SecureSession)
                        {
                            // Create a secret key for ciphering client data.
                            udpClient.Passphrase = Cipher.GenerateKey();
                            handshake.Passphrase = udpClient.Passphrase;
                        }

                        // Prepare binary image of handshake response to be transmitted.
                        udpClient.SendBuffer = handshake.BinaryImage;
                        udpClient.SendBufferOffset = 0;
                        udpClient.SendBufferLength = udpClient.SendBuffer.Length;
                        Payload.ProcessTransmit(ref udpClient.SendBuffer, ref udpClient.SendBufferOffset, ref udpClient.SendBufferLength, Encryption, HandshakePassphrase, Compression);

                        // Transmit the prepared and processed handshake response message.
                        udpClient.Provider.SendTo(udpClient.SendBuffer, udpClient.Provider.RemoteEndPoint);

                        // Handshake process is complete and client is considered connected.
                        lock (m_udpClients)
                        {
                            m_udpClients.Add(udpClient.ID, udpClient);
                        }
                        OnClientConnected(udpClient.ID);
                        ReceiveHandshakeAsync(udpServer);

                        try
                        {
                            ReceivePayloadOneAsync(udpClient);
                        }
                        catch
                        {
                            // Receive will fail if client disconnected before handshake is complete.
                            TerminateConnection(udpClient, true);
                        }
                    }
                    else
                    {
                        // Authentication during handshake failed, so we terminate the client connection.
                        TerminateConnection(udpServer, false);
                        OnHandshakeProcessUnsuccessful();
                    }
                }
                else
                {
                    // Handshake message could not be parsed, so we terminate the client connection.
                    TerminateConnection(udpServer, false);
                    OnHandshakeProcessUnsuccessful();
                }
            }
            catch
            {
                // Server socket has been terminated.
                udpServer.Reset();
                OnServerStopped();
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ReceivePayloadAnyAsync(TransportProvider<Socket> worker)
        {
            worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                             worker.ReceiveBufferOffset,
                                             worker.ReceiveBuffer.Length,
                                             SocketFlags.None,
                                             ref m_udpClientEndPoint,
                                             ReceivePayloadAnyAsyncCallback,
                                             worker);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data from any endpoint.
        /// </summary>
        private void ReceivePayloadAnyAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpServer = (TransportProvider<Socket>)asyncResult.AsyncState;
            try
            {
                // Update statistics and pointers.
                udpServer.Statistics.UpdateBytesReceived(udpServer.Provider.EndReceiveFrom(asyncResult, ref m_udpClientEndPoint));
                udpServer.ReceiveBufferLength = udpServer.Statistics.LastBytesReceived;

                // Get a local copy of all connected clients.
                TransportProvider<Socket>[] clients = null;
                lock (m_udpClients)
                {
                    clients = new TransportProvider<Socket>[m_udpClients.Count];
                    m_udpClients.Values.CopyTo(clients, 0);
                }

                // Search connected clients for a client connected to the end-point from where this data is received.
                foreach (TransportProvider<Socket> client in clients)
                {
                    if (client.Provider.RemoteEndPoint.Equals(m_udpClientEndPoint))
                    {
                        // Found a match, notify of data.
                        OnReceiveClientDataComplete(client.ID, udpServer.ReceiveBuffer, udpServer.ReceiveBufferLength);
                        break;
                    }
                }

                // Resume receive operation on the server socket.
                ReceivePayloadAnyAsync(udpServer);
            }
            catch
            {
                // Server socket has been terminated.
                udpServer.Reset();
                OnServerStopped();
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data from a single endpoint.
        /// </summary>
        private void ReceivePayloadOneAsync(TransportProvider<Socket> worker)
        {
            EndPoint client = worker.Provider.RemoteEndPoint;
            if (ReceiveTimeout == -1)
            {
                // Wait for data indefinitely.
                worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                 worker.ReceiveBufferOffset,
                                                 worker.ReceiveBuffer.Length,
                                                 SocketFlags.None,
                                                 ref client,
                                                 ReceivePayloadOneAsyncCallback,
                                                 worker);
            }
            else
            {
                // Wait for data with a timeout.
                worker.WaitAsync(ReceiveTimeout,
                                 ReceivePayloadOneAsyncCallback,
                                 worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                                  worker.ReceiveBufferOffset,
                                                                  worker.ReceiveBuffer.Length,
                                                                  SocketFlags.None,
                                                                  ref client,
                                                                  ReceivePayloadOneAsyncCallback,
                                                                  worker));
            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data from a single endpoint.
        /// </summary>
        private void ReceivePayloadOneAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveClientDataTimeout(udpClient.ID);
                udpClient.WaitAsync(ReceiveTimeout, ReceivePayloadOneAsyncCallback, asyncResult);
            }
            else
            {
                try
                {
                    // Update statistics and pointers.
                    EndPoint client = udpClient.Provider.RemoteEndPoint;
                    udpClient.Statistics.UpdateBytesReceived(udpClient.Provider.EndReceiveFrom(asyncResult, ref client));
                    udpClient.ReceiveBufferLength = udpClient.Statistics.LastBytesReceived;

                    // Received a goodbye message from the client.
                    if (m_receivedGoodbye(udpClient))
                        throw new SocketException((int)SocketError.Disconnecting);

                    // Notify of received data and resume receive operation.
                    OnReceiveClientDataComplete(udpClient.ID, udpClient.ReceiveBuffer, udpClient.ReceiveBufferLength);
                    ReceivePayloadOneAsync(udpClient);
                }
                catch (ObjectDisposedException ex)
                {
                    // Terminate connection when client is disposed.
                    OnReceiveClientDataException(udpClient.ID, ex);
                    TerminateConnection(udpClient, true);
                }
                catch (SocketException ex)
                {
                    // Terminate the connection when a socket exception is encountered. The most likely socket exception that 
                    // could be encountered is the SocketError.ConnectionReset when Handshake is turned on and the endpoint 
                    // is not listening for data.
                    OnReceiveClientDataException(udpClient.ID, ex);
                    TerminateConnection(udpClient, true);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        OnReceiveClientDataException(udpClient.ID, ex);
                        ReceivePayloadOneAsync(udpClient);
                    }
                    catch
                    {
                        // Terminate connection if resuming receiving fails.
                        TerminateConnection(udpClient, true);
                    }
                }
            }
        }

        /// <summary>
        /// Delegate that gets called to verify client disconnect when <see cref="ServerBase.Handshake"/> is turned off.
        /// </summary>
        private bool NoGoodbyeCheck(TransportProvider<Socket> client)
        {
            return false;
        }

        /// <summary>
        /// Delegate that gets called to verify client disconnect when <see cref="ServerBase.Handshake"/> is turned on.
        /// </summary>
        private bool DoGoodbyeCheck(TransportProvider<Socket> client)
        {
            // Process data received in the buffer.
            int offset = client.ReceiveBufferOffset;
            int length = client.ReceiveBufferLength;
            byte[] buffer = client.ReceiveBuffer.BlockCopy(0, length);
            Payload.ProcessReceived(ref buffer, ref offset, ref length, Encryption, client.Passphrase, Compression);

            // Check if data is for goodbye message.
            return (new GoodbyeMessage().Initialize(buffer, offset, length) != -1);
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<Socket> client, bool raiseEvent)
        {
            client.Reset();
            if (raiseEvent)
                OnClientDisconnected(client.ID);

            lock (m_udpClients)
            {
                if (m_udpClients.ContainsKey(client.ID))
                    m_udpClients.Remove(client.ID);
            }
        }

        #endregion
    }
}