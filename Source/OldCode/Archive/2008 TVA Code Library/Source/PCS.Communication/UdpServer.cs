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
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using PCS.Threading;
using PCS.Configuration;
using PCS.Security.Cryptography;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// Use <see cref="UdpServer"/> when the primary purpose is to transmit data.
    /// </remarks>
    public class UdpServer : ServerBase
    {
        #region [ Members ]

        // Constants

        public const int DefaultReceiveBufferSize = 32768;

        public const string DefaultConfigurationString = "Port=8888; Clients=localhost:8989";

        //public const bool DefaultDestinationReachableCheck = false;

        // Fields
        //private bool m_destinationReachableCheck;
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
        /// <param name="configString">Config string of the server. See <see cref="DefaultConfigurationString"/> for format.</param>
        public UdpServer(string configString)
            : base(TransportProtocol.Udp, configString)
        {
            base.ReceiveBufferSize = DefaultReceiveBufferSize;
            //m_destinationReachableCheck = DefaultDestinationReachableCheck;
            m_udpClients = new Dictionary<Guid, TransportProvider<Socket>>();
        }

        #endregion

        #region [ Properties ]

        ///// <summary>
        ///// Gets or sets a boolean value indicating whether a test is to be performed to check if the destination
        ///// endpoint that is to receive data is listening for data.
        ///// </summary>
        ///// <value></value>
        ///// <returns>
        ///// True if a test is to be performed to check if the destination endpoint that is to receive data is listening
        ///// for data; otherwise False.
        ///// </returns>
        //[Category("Behavior"),
        //DefaultValue(DefaultDestinationReachableCheck),
        //Description("Indicates whether a test is to be performed to check if the destination endpoint that is to receive data is listening for data.")]
        //public bool DestinationReachableCheck
        //{
        //    get
        //    {
        //        return m_destinationReachableCheck;
        //    }
        //    set
        //    {
        //        m_destinationReachableCheck = value;
        //    }
        //}

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
            if (IsRunning)
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
            if (!IsRunning)
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
                catch (ObjectDisposedException)
                {
                    // Terminate connection when client is disposed.
                    TerminateConnection(udpClient, true);
                }
                catch (SocketException ex)
                {
                    if (!Handshake && ex.SocketErrorCode == SocketError.ConnectionReset)
                        // This occurs if client is not listening for data.
                        ReceivePayloadOneAsync(udpClient);
                    else
                        // Terminate connection on other type of exception.
                        TerminateConnection(udpClient, true);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        ReceivePayloadOneAsync(udpClient);
                        OnReceiveClientDataException(udpClient.ID, ex);
                    }
                    catch
                    {
                        // Terminate connection if resuming receiving fails.
                        TerminateConnection(udpClient, true);
                    }
                }
            }
        }

        private bool NoGoodbyeCheck(TransportProvider<Socket> client)
        {
            return false;
        }

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








//        #region [ Members ]

//        // Constants

//        /// <summary>
//        /// The minimum size of the receive buffer for UDP.
//        /// </summary>
//        public const int MinimumUdpBufferSize = 512;

//        /// <summary>
//        /// The maximum number of bytes that can be sent in a single UDP datagram.
//        /// </summary>
//        public const int MaximumUdpDatagramSize = 32768;

//        // Fields
//        private bool m_payloadAware;
//        private bool m_destinationReachableCheck;
//        private StateInfo<Socket> m_udpServer;
//        private Dictionary<Guid, StateInfo<IPEndPoint>> m_udpClients;
//        private Dictionary<string, string> m_configurationData;

//        #endregion

//        #region [ Constructors ]

//        public UdpServer()
//        {
//            m_payloadAware = false;
//            m_destinationReachableCheck = false;
//            m_udpClients = new Dictionary<Guid, StateInfo<System.Net.IPEndPoint>>();

//            base.ConfigurationString = "Port=8888; Clients=255.255.255.255:8888";
//            base.Protocol = TransportProtocol.Udp;
//            base.ReceiveBufferSize = MaximumUdpDatagramSize;

//        }

//        /// <summary>
//        /// Initializes a instance of PCS.Communication.UdpServer with the specified data.
//        /// </summary>
//        /// <param name="configurationString">The configuration string containing the data required for initializing the UDP server.</param>
//        public UdpServer(string configurationString)
//            : this()
//        {
//            base.ConfigurationString = configurationString;
//        }

//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether the messages that are broken down into multiple datagram
//        /// for the purpose of transmission while being sent are to be assembled back when received.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// True if the messages that are broken down into multiple datagram for the purpose of transmission while being
//        /// sent are to be assembled back when received; otherwise False.
//        /// </returns>
//        /// <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
//        [Description("Indicates whether the messages that are broken down into multiple datagram for the purpose of transmission are to be assembled back when received. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(typeof(bool), "False")]
//        public bool PayloadAware
//        {
//            get
//            {
//                return m_payloadAware;
//            }
//            set
//            {
//                m_payloadAware = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether a test is to be performed to check if the destination
//        /// endpoint that is to receive data is listening for data.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// True if a test is to be performed to check if the destination endpoint that is to receive data is listening
//        /// for data; otherwise False.
//        /// </returns>
//        [Description("Indicates whether a test is to be performed to check if the destination endpoint that is to receive data is listening for data."), Category("Behavior"), DefaultValue(typeof(bool), "False")]
//        public bool DestinationReachableCheck
//        {
//            get
//            {
//                return m_destinationReachableCheck;
//            }
//            set
//            {
//                m_destinationReachableCheck = value;
//            }
//        }

//        /// <summary>
//        /// Gets the System.Net.IPEndPoint of the server.
//        /// </summary>
//        /// <returns>The System.Net.IPEndPoint of the server.</returns>
//        [Browsable(false)]
//        public IPEndPoint Server
//        {
//            get
//            {
//                return ((IPEndPoint)m_udpServer.Client.LocalEndPoint);
//            }
//        }

//        /// <summary>
//        /// Gets the current states of all connected clients which includes the System.Net.IPEndPoint of clients.
//        /// </summary>
//        /// <remarks>
//        /// The current states of all connected clients which includes the System.Net.IPEndPoint of clients.
//        /// </remarks>
//        [Browsable(false)]
//        public List<StateInfo<IPEndPoint>> Clients
//        {
//            get
//            {
//                List<StateInfo<IPEndPoint>> clientList = new List<StateInfo<IPEndPoint>>();
//                lock (m_udpClients)
//                {
//                    clientList.AddRange(m_udpClients.Values);
//                }

//                return clientList;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
//        /// </summary>
//        /// <value>Receive buffer size</value>
//        /// <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
//        /// <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
//        public override int ReceiveBufferSize
//        {
//            get
//            {
//                return base.ReceiveBufferSize;
//            }
//            set
//            {
//                if (value >= UdpClient.MinimumUdpBufferSize && value <= UdpClient.MaximumUdpDatagramSize)
//                {
//                    base.ReceiveBufferSize = value;
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("ReceiveBufferSize", "ReceiveBufferSize for UDP must be between " + UdpClient.MinimumUdpBufferSize + " and " + UdpClient.MaximumUdpDatagramSize + ".");
//                }
//            }
//        }

//        #endregion

//        #region [ Methods ]

//        /// <summary>
//        /// Gets the current state of the specified client which includes its System.Net.IPEndPoint.
//        /// </summary>
//        /// <param name="clientID"></param>
//        /// <value></value>
//        /// <returns>
//        /// The current state of the specified client which includes its System.Net.IPEndPoint if the
//        /// specified client ID is valid (client is connected); otherwise Nothing.
//        /// </returns>
//        public StateInfo<IPEndPoint> ClientState(Guid clientID)
//        {
//            StateInfo<IPEndPoint> client;

//            lock (m_udpClients)
//            {
//                m_udpClients.TryGetValue(clientID, out client);
//            }

//            return client;
//        }

//        public override void Start()
//        {
//            if (Enabled && !IsRunning && ValidConfigurationString(ConfigurationString))
//            {
//                try
//                {
//                    int serverPort = 0;

//                    if (m_configurationData.ContainsKey("port"))
//                        serverPort = int.Parse(m_configurationData["port"]);

//                    m_udpServer = new StateInfo<Socket>();
//                    m_udpServer.ID = ServerID;
//                    m_udpServer.Passphrase = HandshakePassphrase;
//                    m_udpServer.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

//                    if (Handshake)
//                    {
//                        // We will listen for data only when Handshake is enabled and a valid port has been specified in
//                        // the configuration string. We do this in order to keep the server stable and besides that, the
//                        // main purpose of a UDP server is to serve data in most cases.
//                        if (serverPort > 0)
//                        {
//                            m_udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, serverPort));

//#if ThreadTracking
//                            ManagedThread receiveThread = new ManagedThread(ReceiveClientData);
//                            receiveThread.Name = "PCS.Communication.UdpServer.ReceiveClientData() [" + ServerID.ToString() + "]";
//#else
//                            Thread receiveThread = new Thread(ReceiveClientData);
//#endif
//                            receiveThread.Start();
//                        }
//                        else
//                        {
//                            throw new ArgumentException("Server port must be specified in the configuration string.");
//                        }
//                    }

//                    OnServerStarted(EventArgs.Empty);

//                    if (!Handshake && m_configurationData.ContainsKey("clients"))
//                    {
//                        // We will ignore the client list in configuration string when Handshake is enabled.
//                        foreach (string clientString in m_configurationData["clients"].Replace(" ", "").Split(','))
//                        {
//                            try
//                            {
//                                int clientPort = 0;

//                                string[] clientStringSegments = clientString.Split(':');

//                                if (clientStringSegments.Length == 2)
//                                    clientPort = int.Parse(clientStringSegments[1]);

//                                StateInfo<IPEndPoint> udpClient = new StateInfo<IPEndPoint>();
//                                udpClient.ID = Guid.NewGuid();
//                                udpClient.Client = Transport.GetIpEndPoint(clientStringSegments[0], clientPort);

//                                lock (m_udpClients)
//                                {
//                                    m_udpClients.Add(udpClient.ID, udpClient);
//                                }

//                                OnClientConnected(udpClient.ID);
//                            }
//                            catch
//                            {
//                                // Ignore invalid client entries.
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    OnServerStartupException(ex);
//                }
//            }
//        }

//        public override void Stop()
//        {
//            if (Enabled && IsRunning)
//            {
//                // Disconnect all of the clients.
//                DisconnectAll();

//                // Stop the server after we've disconnected the clients.
//                if (m_udpServer != null && m_udpServer.Client != null && m_udpServer.Client.Connected)
//                    m_udpServer.Client.Close();

//                OnServerStopped(EventArgs.Empty);
//            }

//        }

//        public override void DisconnectOne(Guid clientID)
//        {
//            StateInfo<IPEndPoint> udpClient;

//            lock (m_udpClients)
//            {
//                m_udpClients.TryGetValue(clientID, out udpClient);
//            }

//            if (udpClient != null)
//            {
//                if (Handshake)
//                {
//                    // Handshake is enabled so we'll notify the client.
//                    byte[] goodbye = GetPreparedData(Serialization.GetBytes(new GoodbyeMessage(udpClient.ID)));

//                    if (m_payloadAware)
//                        goodbye = Payload.AddHeader(goodbye);

//                    m_udpServer.Client.SendTo(goodbye, udpClient.Client);
//                }

//                lock (m_udpClients)
//                {
//                    m_udpClients.Remove(udpClient.ID);
//                }

//                OnClientDisconnected(udpClient.ID);
//            }
//            else
//            {
//                throw new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid.");
//            }
//        }

//        public override void LoadSettings()
//        {
//            base.LoadSettings();

//            try
//            {
//                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];

//                if (settings.Count > 0)
//                {
//                    PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
//                    DestinationReachableCheck = settings["DestinationReachableCheck"].ValueAs(m_destinationReachableCheck);
//                }
//            }
//            catch
//            {
//                // We'll encounter exceptions if the settings are not present in the config file.
//            }
//        }

//        public override void SaveSettings()
//        {
//            base.SaveSettings();

//            if (PersistSettings)
//            {
//                try
//                {
//                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];
//                    CategorizedSettingsElement setting;

//                    setting = settings["PayloadAware", true];
//                    setting.Value = m_payloadAware.ToString();
//                    setting.Description = "True if the messages that are broken down into multiple datagram for the purpose of transmission while being sent are to be assembled back when received; otherwise False.";

//                    setting = settings["DestinationReachableCheck", true];
//                    setting.Value = m_destinationReachableCheck.ToString();
//                    setting.Description = "True if a test is to be performed to check if the destination endpoint that is to receive data is listening for data; otherwise False.";

//                    ConfigurationFile.Current.Save();
//                }
//                catch
//                {
//                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
//                }
//            }
//        }

//        protected override void SendPreparedDataTo(Guid clientID, byte[] data)
//        {
//            if (Enabled && IsRunning)
//            {
//                StateInfo<IPEndPoint> udpClient;

//                lock (m_udpClients)
//                {
//                    m_udpClients.TryGetValue(clientID, out udpClient);
//                }

//                if (udpClient != null)
//                {
//                    if (m_destinationReachableCheck && !Transport.IsDestinationReachable(udpClient.Client))
//                        return;

//                    if (SecureSession)
//                        data = Transport.EncryptData(data, 0, data.Length, udpClient.Passphrase, Encryption);

//                    if (m_payloadAware)
//                        data = Payload.AddHeader(data);

//                    int toIndex = 0;
//                    int datagramSize = ReceiveBufferSize;

//                    if (data.Length > datagramSize)
//                        toIndex = data.Length - 1;

//                    for (int i = 0; i <= toIndex; i += datagramSize)
//                    {
//                        // Last or the only datagram in the series.
//                        if (data.Length - i < datagramSize)
//                            datagramSize = data.Length - i;

//                        // PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
//                        m_udpServer.Client.SendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client);
//                        udpClient.LastSendTimestamp = DateTime.Now;

//                        //' We'll send the data asynchronously for better performance.
//                        //m_udpServer.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client, Nothing, Nothing)
//                    }
//                }
//                else
//                {
//                    throw new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid.");
//                }
//            }
//        }

//        protected override bool ValidConfigurationString(string configurationString)
//        {
//            if (!string.IsNullOrEmpty(configurationString))
//            {
//                m_configurationData = configurationString.ParseKeyValuePairs();

//                if ((m_configurationData.ContainsKey("port") && Transport.IsValidPortNumber(m_configurationData["port"])) ||
//                    (m_configurationData.ContainsKey("clients") && !string.IsNullOrEmpty(m_configurationData["clients"])))
//                {
//                    // The configuration string must contain either of the following:
//                    // >> port - Port number on which the server will be listening for incoming data.
//                    // OR
//                    // >> clients - A list of clients the server will be sending data to.
//                    return true;
//                }
//                else
//                {
//                    // Configuration string is not in the expected format.
//                    StringBuilder exceptionMessage = new StringBuilder();

//                    exceptionMessage.Append("Configuration string must be in the following format:");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("   [Port=Local port number;] [Clients=Client name or IP[:Port number], ..., Client name or IP[:Port number]]");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("Text between square brackets, [...], is optional.");

//                    throw new ArgumentException(exceptionMessage.ToString());
//                }
//            }
//            else
//            {
//                throw new ArgumentNullException("ConfigurationString");
//            }
//        }

//        private void ReceiveClientData()
//        {
//            try
//            {
//                int bytesReceived = 0;
//                EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

//                if (m_receiveRawDataFunction != null || (m_receiveRawDataFunction == null && !m_payloadAware))
//                {
//                    // In this section the consumer either wants to receive the datagrams and pass it on to a
//                    // delegate or receive datagrams that don't contain metadata used for re-assembling the
//                    // datagrams into the original message and be notified via events. In either case we can use
//                    // a static buffer that can be used over and over again for receiving datagrams as long as
//                    // the datagrams received are not bigger than the receive buffer.
//                    while (true)
//                    {
//                        bytesReceived = m_udpServer.Client.ReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, ref clientEndPoint);
//                        m_udpServer.LastReceiveTimestamp = DateTime.Now;

//                        if (m_receiveRawDataFunction != null)
//                            m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
//                        else
//                            ProcessReceivedClientData(m_buffer.CopyBuffer(0, bytesReceived), clientEndPoint);
//                    }
//                }
//                else
//                {
//                    int payloadSize = -1;
//                    int totalBytesReceived = 0;

//                    while (true)
//                    {
//                        if (payloadSize == -1)
//                            m_udpServer.DataBuffer = new byte[ReceiveBufferSize];

//                        bytesReceived = m_udpServer.Client.ReceiveFrom(m_udpServer.DataBuffer, totalBytesReceived, (m_udpServer.DataBuffer.Length - totalBytesReceived), SocketFlags.None, ref clientEndPoint);
//                        m_udpServer.LastReceiveTimestamp = DateTime.Now;

//                        if (payloadSize == -1)
//                        {
//                            payloadSize = Payload.GetSize(m_udpServer.DataBuffer);
//                            if (payloadSize != -1 && payloadSize <= ClientBase.MaximumDataSize)
//                            {
//                                byte[] payload = Payload.Retrieve(m_udpServer.DataBuffer);

//                                m_udpServer.DataBuffer = new byte[payloadSize];
//                                Buffer.BlockCopy(payload, 0, m_udpServer.DataBuffer, 0, payload.Length);
//                                bytesReceived = payload.Length;
//                            }
//                        }

//                        totalBytesReceived += bytesReceived;
//                        if (totalBytesReceived == payloadSize)
//                        {
//                            // We've received the entire payload.
//                            ProcessReceivedClientData(m_udpServer.DataBuffer, clientEndPoint);

//                            // Initialize for receiving the next payload.
//                            payloadSize = -1;
//                            totalBytesReceived = 0;
//                        }
//                    }
//                }
//            }
//            catch
//            {
//                // We don't need to take any action when an exception is encountered.
//            }
//            finally
//            {
//                if (m_udpServer != null && m_udpServer.Client != null)
//                    m_udpServer.Client.Close();
//            }
//        }

//        private void ProcessReceivedClientData(byte[] data, EndPoint senderEndPoint)
//        {
//            if (data.Length == 0)
//                return;

//            object clientMessage = Serialization.GetObject(GetActualData(data));

//            if (clientMessage != null)
//            {
//                // We were able to deserialize the data to an object.
//                if (clientMessage is HandshakeMessage)
//                {
//                    HandshakeMessage connectedClient = (HandshakeMessage)clientMessage;

//                    if (connectedClient.ID != Guid.Empty && connectedClient.Passphrase == HandshakePassphrase)
//                    {
//                        StateInfo<IPEndPoint> udpClient = new StateInfo<IPEndPoint>();
//                        udpClient.ID = connectedClient.ID;
//                        udpClient.Client = (IPEndPoint)senderEndPoint;

//                        if (SecureSession)
//                            udpClient.Passphrase = Cipher.GenerateKey();

//                        byte[] myInfo = GetPreparedData(Serialization.GetBytes(new HandshakeMessage(ServerID, udpClient.Passphrase)));

//                        if (m_payloadAware)
//                            myInfo = Payload.AddHeader(myInfo);

//                        m_udpServer.Client.SendTo(myInfo, udpClient.Client);

//                        lock (m_udpClients)
//                        {
//                            m_udpClients.Add(udpClient.ID, udpClient);
//                        }

//                        OnClientConnected(udpClient.ID);
//                    }
//                }
//                else if (clientMessage is GoodbyeMessage)
//                {
//                    GoodbyeMessage disconnectedClient = (GoodbyeMessage)clientMessage;

//                    lock (m_udpClients)
//                    {
//                        m_udpClients.Remove(disconnectedClient.ID);
//                    }

//                    OnClientDisconnected(disconnectedClient.ID);
//                }
//            }
//            else
//            {
//                StateInfo<IPEndPoint> sender = null;
//                IPEndPoint senderIPEndPoint = (IPEndPoint)senderEndPoint;

//                if (!senderIPEndPoint.Equals(Transport.GetIpEndPoint(Dns.GetHostName(), int.Parse(m_configurationData["port"]))))
//                {
//                    // The data received is not something that we might have broadcasted.
//                    lock (m_udpClients)
//                    {
//                        // So, now we'll find the ID of the client who sent the data.
//                        foreach (StateInfo<IPEndPoint> udpClient in m_udpClients.Values)
//                        {
//                            if (senderIPEndPoint.Equals(udpClient.Client))
//                            {
//                                sender = udpClient;
//                                break;
//                            }
//                        }
//                    }

//                    if (sender != null)
//                    {
//                        if (SecureSession)
//                        {
//                            data = Transport.DecryptData(data, sender.Passphrase, Encryption);
//                        }
//                        OnReceivedClientData(new IdentifiableItem<Guid, byte[]>(sender.ID, data));
//                    }
//                    else
//                    {
//                        OnReceivedClientData(new IdentifiableItem<Guid, byte[]>(Guid.Empty, data));
//                    }
//                }
//            }
//        }

//        #endregion