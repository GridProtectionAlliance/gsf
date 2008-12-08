//*******************************************************************************************************
//  TcpServer.cs
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
//  06/02/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access
//  12/01/2006 - Pinal C. Patel
//       Modified code for handling "PayloadAware" transmissions
//  01/28/3008 - J. Ritchie Carroll
//       Placed accepted TCP socket connections on their own threads instead of thread pool
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;
using PCS.Security.Cryptography;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a TCP-based communication server.
    /// </summary>
    /// <remarks>
    /// "Payload-Aware" enabled transmission can transmit up to 100MB of payload in a single transmission.
    /// </remarks>
    /// <example>
    /// 
    /// <code>
    /// using System;
    /// using PCS.Communication;
    /// using PCS.Security.Cryptography;
    /// using PCS.IO.Compression;
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
    ///         m_server.PayloadAware = true;
    ///         m_server.ReceiveTimeout = -1;
    ///         //m_server.Encryption = CipherStrength.Level1;
    ///         //m_server.Compression = CompressionStrength.BestSpeed;
    ///         //m_server.SecureSession = true;
    ///         // Register event handlers.
    ///         m_server.ServerStarted += m_server_ServerStarted;
    ///         m_server.ServerStopped += m_server_ServerStopped;
    ///         m_server.ClientConnected += m_server_ClientConnected;
    ///         m_server.ClientDisconnected += m_server_ClientDisconnected;
    ///         m_server.ReceiveClientDataComplete += m_server_ReceiveClientDataComplete;
    ///         // Start the server.
    ///         m_server.Start();
    /// 
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_server.Multicast(System.IO.File.ReadAllText(@"C:\My Projects\CLR 3.5\Win\TVACodeLibrary\Source\TVA.Core\Collections\KeyedProcessQueue.vb"));
    ///         }
    /// 
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

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888";

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        // Fields
        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private Socket m_tcpServer;
        private Dictionary<string, string> m_configData;
        private Dictionary<Guid, TransportProvider<Socket>> m_tcpClients;

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
        /// <param name="configString">Config string of the server. See <see cref="DefaultConfigurationString"/> for format.</param>
        public TcpServer(string configString)
            : base(TransportProtocol.Tcp, configString)
        {
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_tcpClients = new Dictionary<Guid, TransportProvider<Socket>>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
        /// <remarks>This property must be set to True if either <see cref="ServerBase.Encryption"/> or <see cref="ServerBase.Compression"/> is enabled.</remarks>
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
        /// <exception cref="ArgumentNullException">The value specified is null or empty buffer.</exception>
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
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException();

                m_payloadMarker = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpServer"/>.
        /// </summary>
        [Browsable(false)]
        public Socket ServerSocket
        {
            get
            {
                return m_tcpServer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Stops the <see cref="TcpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            if (IsRunning)
            {
                m_tcpServer.Close();    // Stop accepting new connections.
                DisconnectAll();        // Disconnection all connection clients.
            }
        }

        /// <summary>
        /// Starts the <see cref="TcpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="TcpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (!IsRunning)
            {
                // Initialize if unitialized.
                Initialize();
                // Bind server socket to local end-point and listen.
                m_tcpServer = Transport.CreateSocket(int.Parse(m_configData["port"]), ProtocolType.Tcp);
                m_tcpServer.Listen(1);
                // Begin accepting incoming connection asynchronously.
                m_tcpServer.BeginAccept(AcceptAsyncCallback, null);
                // Notify that the server has been started successfully.
                OnServerStarted(EventArgs.Empty);
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
            ClientSocket(clientID).Provider.Close();
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{Socket}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <returns>An <see cref="TransportProvider{Socket}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public TransportProvider<Socket> ClientSocket(Guid clientID)
        {
            TransportProvider<Socket> tcpClient;
            lock (m_tcpClients)
            {
                if (m_tcpClients.TryGetValue(clientID, out tcpClient))
                    return tcpClient;
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
            return ClientSocket(clientID).Passphrase;
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
            TransportProvider<Socket> tcpClient = ClientSocket(clientID);

            // Prepare for payload-aware transmission.
            if (m_payloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

            // Send payload to the client asynchronously.
            tcpClient.SendBuffer = data;
            tcpClient.SendBufferOffset = offset;
            tcpClient.SendBufferLength = length;
            handle = tcpClient.Provider.BeginSend(tcpClient.SendBuffer, tcpClient.SendBufferOffset, tcpClient.SendBufferLength, SocketFlags.None, SendPayloadAsyncCallback, tcpClient).AsyncWaitHandle;
            OnSendClientDataStarted(new EventArgs<Guid>(tcpClient.ID));

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous accept operation.
        /// </summary>
        private void AcceptAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = new TransportProvider<Socket>();
            try
            {
                // Return to accepting new connections.
                m_tcpServer.BeginAccept(AcceptAsyncCallback, null);
                // Process the newly connected client.
                tcpClient.Passphrase = HandshakePassphrase;
                tcpClient.Provider = m_tcpServer.EndAccept(asyncResult);
                if (MaxClientConnections != -1 && ClientIDs.Length >= MaxClientConnections)
                {
                    // Reject client connection since limit has been reached.
                    TerminateConnection(tcpClient, false);
                }
                else
                {
                    // We can proceed further with receiving data from the client.
                    if (Handshake)
                    {
                        // Handshaking must be performed. 
                        ReceiveHandshakeAsync(tcpClient);
                    }
                    else
                    {
                        // No handshaking to be performed.
                        lock (m_tcpClients)
                        {
                            m_tcpClients.Add(tcpClient.ID, tcpClient);
                        }
                        OnClientConnected(new EventArgs<Guid>(tcpClient.ID));
                        ReceivePayloadAsync(tcpClient);
                    }
                }
            }
            catch
            {
                // Server socket has been terminated.
                m_tcpServer.Close();
                OnServerStopped(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendPayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = asyncResult.AsyncState as TransportProvider<Socket>;
            try
            {
                // Send operation is complete.
                tcpClient.Statistics.UpdateBytesSent(tcpClient.Provider.EndSend(asyncResult));
                OnSendClientDataComplete(new EventArgs<Guid>(tcpClient.ID));
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendClientDataException(new EventArgs<Guid, Exception>(tcpClient.ID, ex));
            }
        }

        /// <summary>
        /// Initiate method for receving handshake data.
        /// </summary>
        private void ReceiveHandshakeAsync(TransportProvider<Socket> worker)
        {
            // Prepare buffer used for receiving data.
            worker.ReceiveBufferOffset = 0;
            worker.ReceiveBufferLength = -1;
            worker.ReceiveBuffer = new byte[ReceiveBufferSize];
            // Receive data asynchronously with a timeout.
            worker.WaitAsync(HandshakeTimeout,
                             ReceiveHandshakeAsyncCallback,
                             worker.Provider.BeginReceive(worker.ReceiveBuffer,
                                                          worker.ReceiveBufferOffset,
                                                          worker.ReceiveBuffer.Length,
                                                          SocketFlags.None,
                                                          ReceiveHandshakeAsyncCallback,
                                                          worker));
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of handshake data.
        /// </summary>
        private void ReceiveHandshakeAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = asyncResult.AsyncState as TransportProvider<Socket>;
            if (!asyncResult.IsCompleted)
            {
                // Handshake didn't complete in a timely fashion.
                TerminateConnection(tcpClient, false);
                OnHandshakeTimedout(EventArgs.Empty);
            }
            else
            {
                // Received handshake data from client so we'll process it.
                try
                {
                    // Update statistics and pointers.
                    tcpClient.Statistics.UpdateBytesReceived(tcpClient.Provider.EndReceive(asyncResult));
                    tcpClient.ReceiveBufferLength = tcpClient.Statistics.LastBytesReceived;

                    if (tcpClient.Statistics.LastBytesReceived == 0)
                        // Client disconnected gracefully.
                        throw new SocketException((int)SocketError.Disconnecting);

                    // Process the received handshake message.
                    Payload.ProcessReceived(ref tcpClient.ReceiveBuffer, ref tcpClient.ReceiveBufferOffset, ref tcpClient.ReceiveBufferLength, Encryption, HandshakePassphrase, Compression);

                    HandshakeMessage handshake = new HandshakeMessage();
                    if (handshake.Initialize(tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferOffset, tcpClient.ReceiveBufferLength) != -1)
                    {
                        // Received handshake message could be parsed successfully.
                        if (handshake.ID != Guid.Empty && handshake.Passphrase == HandshakePassphrase)
                        {
                            // Authentication is successful; respond to the handshake.
                            tcpClient.ID = handshake.ID;
                            handshake.ID = this.ServerID;
                            if (SecureSession)
                            {
                                // Create a secret key for ciphering client data.
                                tcpClient.Passphrase = Cipher.GenerateKey();
                                handshake.Passphrase = tcpClient.Passphrase;
                            }

                            // Prepare binary image of handshake response to be transmitted.
                            tcpClient.SendBuffer = handshake.BinaryImage;
                            tcpClient.SendBufferOffset = 0;
                            tcpClient.SendBufferLength = tcpClient.SendBuffer.Length;
                            Payload.ProcessTransmit(ref tcpClient.SendBuffer, ref tcpClient.SendBufferOffset, ref tcpClient.SendBufferLength, Encryption, HandshakePassphrase, Compression);

                            // Transmit the prepared and processed handshake response message.
                            tcpClient.Provider.Send(tcpClient.SendBuffer, tcpClient.SendBufferOffset, tcpClient.SendBufferLength, SocketFlags.None);

                            // Handshake process is complete and client is considered connected.
                            lock (m_tcpClients)
                            {
                                m_tcpClients.Add(tcpClient.ID, tcpClient);
                            }
                            OnClientConnected(new EventArgs<Guid>(tcpClient.ID));
                            ReceivePayloadAsync(tcpClient);
                        }
                        else
                        {
                            // Authentication during handshake failed, so we terminate the client connection.
                            TerminateConnection(tcpClient, false);
                            OnHandshakeUnsuccessful(EventArgs.Empty);
                        }
                    }
                    else
                    {
                        // Handshake message could not be parsed, so we terminate the client connection.
                        TerminateConnection(tcpClient, false);
                        OnHandshakeUnsuccessful(EventArgs.Empty);
                    }
                }
                catch
                {
                    // Handshake process could not be completed most likely due to client disconnect.
                    TerminateConnection(tcpClient, false);
                    OnHandshakeUnsuccessful(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Initiate method for receiving payload data.
        /// </summary>
        private void ReceivePayloadAsync(TransportProvider<Socket> worker)
        {
            // Initialize pointers.
            worker.ReceiveBufferOffset = 0;
            worker.ReceiveBufferLength = -1;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Payload boundaries are to be preserved.
                worker.ReceiveBuffer = new byte[m_payloadMarker.Length + Payload.LengthSegment];
                ReceivePayloadAwareAsync(worker);
            }
            else
            {
                // Payload boundaries are not to be preserved.
                worker.ReceiveBuffer = new byte[ReceiveBufferSize];
                ReceivePayloadUnawareAsync(worker);
            }
        }

        /// <summary>
        /// Initiate method for receiving payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(TransportProvider<Socket> worker)
        {
            if (ReceiveTimeout == -1)
            {
                // Wait for data indefinitely.
                worker.Provider.BeginReceive(worker.ReceiveBuffer,
                                             worker.ReceiveBufferOffset,
                                             worker.ReceiveBuffer.Length - worker.ReceiveBufferOffset,
                                             SocketFlags.None,
                                             ReceivePayloadAwareAsyncCallback,
                                             worker);
            }
            else
            {
                // Wait for data with a timeout.
                worker.WaitAsync(ReceiveTimeout,
                                 ReceivePayloadAwareAsyncCallback,
                                 worker.Provider.BeginReceive(worker.ReceiveBuffer,
                                                              worker.ReceiveBufferOffset,
                                                              worker.ReceiveBuffer.Length - worker.ReceiveBufferOffset,
                                                              SocketFlags.None,
                                                              ReceivePayloadAwareAsyncCallback,
                                                              worker));
            }

        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = asyncResult.AsyncState as TransportProvider<Socket>;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveClientDataTimedout(new EventArgs<Guid>(tcpClient.ID));
                tcpClient.WaitAsync(ReceiveTimeout, ReceivePayloadAwareAsyncCallback, asyncResult);
            }
            else
            {
                try
                {
                    // Update statistics and pointers.
                    tcpClient.Statistics.UpdateBytesReceived(tcpClient.Provider.EndReceive(asyncResult));
                    tcpClient.ReceiveBufferOffset += tcpClient.Statistics.LastBytesReceived;

                    if (tcpClient.Statistics.LastBytesReceived == 0)
                        // Client disconnected gracefully.
                        throw new SocketException((int)SocketError.Disconnecting);

                    if (tcpClient.ReceiveBufferLength == -1)
                    {
                        // We're waiting on the payload length, so we'll check if the received data has this information.
                        tcpClient.ReceiveBufferOffset = 0;
                        tcpClient.ReceiveBufferLength = Payload.ExtractLength(tcpClient.ReceiveBuffer, m_payloadMarker);

                        if (tcpClient.ReceiveBufferLength != -1)
                        {
                            // We have the payload length, so we'll create a buffer that's big enough to hold the entire payload.
                            tcpClient.ReceiveBuffer = new byte[tcpClient.ReceiveBufferLength];
                        }

                        ReceivePayloadAwareAsync(tcpClient);
                    }
                    else
                    {
                        // We're accumulating the payload in the receive buffer until the entire payload is received.
                        if (tcpClient.ReceiveBufferOffset == tcpClient.ReceiveBufferLength)
                        {
                            // We've received the entire payload.
                            OnReceiveClientDataComplete(tcpClient.ID, tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferLength);
                            ReceivePayloadAsync(tcpClient);
                        }
                        else
                        {
                            // We've not yet received the entire payload.
                            ReceivePayloadAwareAsync(tcpClient);
                        }
                    }
                }
                catch
                {
                    // Client disconnected so we'll process it's disconnect.
                    TerminateConnection(tcpClient, true);
                }
            }
        }

        /// <summary>
        /// Initiate method for receiving payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync(TransportProvider<Socket> worker)
        {
            if (ReceiveTimeout == -1)
            {
                // Wait for data indefinitely.
                worker.Provider.BeginReceive(worker.ReceiveBuffer,
                                             worker.ReceiveBufferOffset,
                                             worker.ReceiveBuffer.Length - worker.ReceiveBufferOffset,
                                             SocketFlags.None,
                                             ReceivePayloadUnawareAsyncCallback,
                                             worker);
            }
            else
            {
                // Wait for data with a timeout.
                worker.WaitAsync(ReceiveTimeout,
                                 ReceivePayloadUnawareAsyncCallback,
                                 worker.Provider.BeginReceive(worker.ReceiveBuffer,
                                                              worker.ReceiveBufferOffset,
                                                              worker.ReceiveBuffer.Length - worker.ReceiveBufferOffset,
                                                              SocketFlags.None,
                                                              ReceivePayloadUnawareAsyncCallback,
                                                              worker));

            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = asyncResult.AsyncState as TransportProvider<Socket>;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveClientDataTimedout(new EventArgs<Guid>(tcpClient.ID));
                tcpClient.WaitAsync(ReceiveTimeout, ReceivePayloadUnawareAsyncCallback, asyncResult);
            }
            else
            {
                try
                {
                    // Update statistics and pointers.
                    tcpClient.Statistics.UpdateBytesReceived(tcpClient.Provider.EndReceive(asyncResult));
                    tcpClient.ReceiveBufferLength = tcpClient.Statistics.LastBytesReceived;

                    if (tcpClient.Statistics.LastBytesReceived == 0)
                        // Client disconnected gracefully.
                        throw new SocketException((int)SocketError.Disconnecting);

                    // Notify of received data and resume receive operation.
                    OnReceiveClientDataComplete(tcpClient.ID, tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferLength);
                    ReceivePayloadUnawareAsync(tcpClient);
                }
                catch
                {
                    // Client disconnected so we'll process it's disconnect.
                    TerminateConnection(tcpClient, true);
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<Socket> client, bool raiseEvent)
        {
            client.Reset();
            if (raiseEvent)
                OnClientDisconnected(new EventArgs<Guid>(client.ID));

            lock (m_tcpClients)
            {
                if (m_tcpClients.ContainsKey(client.ID))
                    m_tcpClients.Remove(client.ID);
            }
        }

        #endregion
    }
}



//        #region [ Members ]

//        // Fields
//        private bool m_payloadAware;
//        private Socket m_tcpServer;
//        private Dictionary<Guid, StateInfo<Socket>> m_tcpClients;
//        private List<StateInfo<Socket>> m_pendingTcpClients;
//        private Dictionary<string, string> m_configurationData;
//#if ThreadTracking
//        private ManagedThread m_listenerThread;
//#else
//        private Thread m_listenerThread;
//#endif
//        private bool m_disposed;

//        #endregion

//        #region [ Constructors ]

//        public TcpServer()
//        {
//            // Setup the instance defaults.
//            m_payloadAware = false;
//            m_tcpClients = new Dictionary<Guid, StateInfo<System.Net.Sockets.Socket>>();
//            m_pendingTcpClients = new List<StateInfo<System.Net.Sockets.Socket>>();

//            base.ConfigurationString = "Port=8888";
//            base.Protocol = TransportProtocol.Tcp;
//        }

//        /// <summary>
//        /// Initializes a instance of PCS.Communication.TcpServer with the specified data.
//        /// </summary>
//        /// <param name="configurationString">The connection string containing the data required for the TCP server to run.</param>
//        public TcpServer(string configurationString)
//            : this()
//        {
//            base.ConfigurationString = configurationString;
//        }

//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether the message boundaries are to be preserved during transmission.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// True if the message boundaries are to be preserved during transmission; otherwise False.
//        /// </returns>
//        /// <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
//        [Description("Indicates whether the message boundaries are to be preserved during transmission. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(typeof(bool), "False")]
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
//        /// Gets the System.Net.Sockets.Socket of the server.
//        /// </summary>
//        /// <returns>The System.Net.Sockets.Socket of the server.</returns>
//        [Browsable(false)]
//        public Socket Server
//        {
//            get
//            {
//                return m_tcpServer;
//            }
//        }

//        /// <summary>
//        /// Gets the current states of all connected clients which includes the System.Net.Sockets.Socket of clients.
//        /// </summary>
//        /// <remarks>
//        /// The current states of all connected clients which includes the System.Net.Sockets.Socket of clients.
//        /// </remarks>
//        [Browsable(false)]
//        public List<StateInfo<Socket>> Clients
//        {
//            get
//            {
//                List<StateInfo<Socket>> clientList = new List<StateInfo<Socket>>();
//                lock (m_tcpClients)
//                {
//                    clientList.AddRange(m_tcpClients.Values);
//                }

//                return clientList;
//            }
//        }

//        #endregion

//        #region [ Methods ]

//        /// <summary>
//        /// Releases the unmanaged resources used by an instance of the <see cref="FileClient" /> class and optionally releases the managed resources.
//        /// </summary>
//        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (!m_disposed)
//            {
//                try
//                {
//                    if (disposing)
//                    {
//                        if (m_listenerThread != null)
//                        {
//                            if (m_listenerThread.IsAlive)
//                                m_listenerThread.Abort();
//                        }
//                        m_listenerThread = null;

//                        if (m_tcpServer != null)
//                        {
//                            if (m_tcpServer.Connected)
//                            {
//                                m_tcpServer.Shutdown(SocketShutdown.Both);
//                                m_tcpServer.Close();
//                            }
//                        }
//                        m_tcpServer = null;

//                        if (m_tcpClients != null)
//                        {
//                            foreach (StateInfo<Socket> client in m_tcpClients.Values)
//                            {
//                                if (client != null && client.Client != null && client.Client.Connected)
//                                {
//                                    client.Client.Shutdown(SocketShutdown.Both);
//                                    client.Client.Close();
//                                }
//                            }

//                            m_tcpClients.Clear();
//                        }
//                        m_tcpClients = null;

//                        if (m_pendingTcpClients != null)
//                        {
//                            foreach (StateInfo<Socket> client in m_pendingTcpClients)
//                            {
//                                if (client != null && client.Client != null && client.Client.Connected)
//                                {
//                                    client.Client.Shutdown(SocketShutdown.Both);
//                                    client.Client.Close();
//                                }
//                            }

//                            m_pendingTcpClients.Clear();
//                        }
//                        m_pendingTcpClients = null;
//                    }
//                }
//                finally
//                {
//                    base.Dispose(disposing);    // Call base class Dispose().
//                    m_disposed = true;          // Prevent duplicate dispose.
//                }
//            }
//        }

//        /// <summary>
//        /// Gets the current state of the specified client which includes its System.Net.Sockets.Socket.
//        /// </summary>
//        /// <param name="clientID"></param>
//        /// <value></value>
//        /// <returns>
//        /// The current state of the specified client which includes its System.Net.Sockets.Socket if the
//        /// specified client ID is valid (client is connected); otherwise Nothing.
//        /// </returns>
//        public StateInfo<Socket> ClientState(Guid clientID)
//        {
//            StateInfo<Socket> client;

//            lock (m_tcpClients)
//            {
//                m_tcpClients.TryGetValue(clientID, out client);
//            }

//            return client;
//        }

//        /// <summary>
//        /// Starts the server.
//        /// </summary>
//        public override void Start()
//        {
//            if (Enabled && !IsRunning && ValidConfigurationString(ConfigurationString))
//            {
//#if ThreadTracking
//                m_listenerThread = new ManagedThread(ListenForConnections);
//                m_listenerThread.Name = "PCS.Communication.TcpServer.ListenForConnections()";
//#else
//                m_listenerThread = new Thread(ListenForConnections);
//#endif

//                // Start the thread on which the server will listen for incoming connections.
//                m_listenerThread.Start();
//            }
//        }

//        /// <summary>
//        /// Stops the server.
//        /// </summary>
//        public override void Stop()
//        {
//            if (Enabled && IsRunning)
//            {
//                // NOTE: Closing the server and all of the connected client sockets will cause a
//                //       System.Net.Socket.SocketException in the thread using the socket. This in turn will result in
//                //       the thread to exit gracefully because of the exception handling in place in the threads.

//                // Stop accepting incoming connections.
//                if (m_tcpServer != null)
//                    m_tcpServer.Close();

//                // Diconnect all of the connected clients.
//                DisconnectAll();

//                // Diconnect all of the pending clients connections.
//                lock (m_pendingTcpClients)
//                {
//                    foreach (StateInfo<Socket> pendingTcpClient in m_pendingTcpClients)
//                    {
//                        if ((pendingTcpClient != null) && (pendingTcpClient.Client != null))
//                            pendingTcpClient.Client.Close();
//                    }
//                }
//            }
//        }

//        public override void DisconnectOne(Guid clientID)
//        {
//            StateInfo<Socket> tcpClient;

//            lock (m_tcpClients)
//            {
//                m_tcpClients.TryGetValue(clientID, out tcpClient);
//            }

//            if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
//            {
//                tcpClient.Client.Shutdown(SocketShutdown.Both);
//                tcpClient.Client.Close();
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
//                    setting.Description = "True if the message boundaries are to be preserved during transmission; otherwise False.";

//                    ConfigurationFile.Current.Save();
//                }
//                catch
//                {
//                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
//                }
//            }
//        }

//        /// <summary>
//        /// Sends prepared data to the specified client.
//        /// </summary>
//        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
//        /// <param name="data">The prepared data that is to be sent to the client.</param>
//        protected override void SendPreparedDataTo(Guid clientID, byte[] data)
//        {
//            if (Enabled && IsRunning)
//            {
//                StateInfo<Socket> tcpClient;

//                lock (m_tcpClients)
//                {
//                    m_tcpClients.TryGetValue(clientID, out tcpClient);
//                }

//                if (tcpClient != null)
//                {
//                    // Encrypt the data with private key if SecureSession is enabled.
//                    if (SecureSession)
//                        data = Transport.EncryptData(data, 0, data.Length, tcpClient.Passphrase, Encryption);

//                    // Add payload header if client-server communication is PayloadAware.
//                    if (m_payloadAware)
//                        data = Payload.AddHeader(data);

//                    // PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
//                    tcpClient.Client.Send(data);
//                    tcpClient.LastSendTimestamp = DateTime.Now;

//                    //' We'll send data over the wire asynchronously for improved performance.
//                    //tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)
//                }
//                else
//                {
//                    throw new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid.");
//                }
//            }
//        }

//        /// <summary>
//        /// Determines whether specified configuration string required for the server to initialize is valid.
//        /// </summary>
//        /// <param name="configurationString">The configuration string to be validated.</param>
//        /// <returns>True if the configuration string is valid.</returns>
//        protected override bool ValidConfigurationString(string configurationString)
//        {
//            if (!string.IsNullOrEmpty(configurationString))
//            {
//                m_configurationData = configurationString.ParseKeyValuePairs();

//                if (m_configurationData.ContainsKey("port") && Transport.IsValidPortNumber(m_configurationData["port"]))
//                {
//                    // The configuration string must always contain the following:
//                    // >> port - Port number on which the server will be listening for incoming connections.
//                    return true;
//                }
//                else
//                {
//                    // Configuration string is not in the expected format.
//                    StringBuilder exceptionMessage = new StringBuilder();

//                    exceptionMessage.Append("Configuration string must be in the following format:");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("   Port=Local port number");

//                    throw new ArgumentException(exceptionMessage.ToString());
//                }
//            }
//            else
//            {
//                throw new ArgumentNullException("ConfigurationString");
//            }
//        }

//        /// <summary>
//        /// Listens for incoming client connections.
//        /// </summary>
//        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
//        private void ListenForConnections()
//        {
//            try
//            {
//                // Create a TCP socket and bind it a local endpoint at the specified port. Binding the socket will
//                // establish a physical presence of the socket necessary for listening to incoming connections.
//                m_tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                m_tcpServer.Bind(new IPEndPoint(IPAddress.Any, int.Parse(m_configurationData["port"])));

//                // Start listening for connections and keep a maximum of 0 pending connection in the queue.
//                m_tcpServer.Listen(0);

//                OnServerStarted(EventArgs.Empty);

//                while (true)
//                {
//                    if (MaximumClients == -1 || ClientIDs.Count < MaximumClients)
//                    {
//                        // We can accept incoming client connection requests.
//                        StateInfo<Socket> tcpClient = new StateInfo<Socket>();
//                        tcpClient.Client = m_tcpServer.Accept(); // Accept client connection.						
//                        tcpClient.Client.LingerState = new LingerOption(true, 10);

//                        // Start the client on a seperate thread so all the connected clients run independently.
//                        //ThreadPool.QueueUserWorkItem(AddressOf ReceiveClientData, tcpClient)

//#if ThreadTracking
//                        ManagedThread receiveThread = new ManagedThread(ReceiveClientData);
//                        receiveThread.Name = "PCS.Communication.TcpServer.ReceiveClientData() [" + ServerID.ToString() + "]";
//#else
//                        Thread receiveThread = new Thread(ReceiveClientData);
//#endif
//                        receiveThread.Start(tcpClient);
//                    }
//                }
//            }
//            catch (ThreadAbortException)
//            {
//                // This will be a normal exception...
//            }
//            catch (ObjectDisposedException)
//            {
//                // This will be a normal exception...
//            }
//            catch (SocketException ex)
//            {
//                if (ex.SocketErrorCode != SocketError.Interrupted)
//                {
//                    // If we encounter a socket exception other than SocketError.Interrupt, we'll report it as an exception.
//                    OnServerStartupException(ex);
//                }
//            }
//            catch (Exception ex)
//            {
//                // We will gracefully exit when an exception occurs.
//                OnServerStartupException(ex);
//            }
//            finally
//            {
//                if (m_tcpServer != null)
//                {
//                    m_tcpServer.Close();
//                }
//                OnServerStopped(EventArgs.Empty);
//            }
//        }

//        /// <summary>
//        /// Receives any data sent by a client that is connected to the server.
//        /// </summary>
//        /// <param name="state">PCS.Communication.StateKeeper(Of Socket) of the the connected client.</param>
//        /// <remarks>This method is meant to be executed on seperate threads.</remarks>
//        private void ReceiveClientData(object state)
//        {
//            StateInfo<Socket> tcpClient = (StateInfo<Socket>)state;

//            try
//            {
//                if (Handshake)
//                {
//                    // Handshaking is to be performed to authenticate the client, so we'll add the client to the
//                    // list of client who have not been authenticated and give it 30 seconds to initiate handshaking.
//                    tcpClient.Client.ReceiveTimeout = 30000;

//                    lock (m_pendingTcpClients)
//                    {
//                        m_pendingTcpClients.Add(tcpClient);
//                    }
//                }
//                else
//                {
//                    // No handshaking is to be performed for authenicating the client, so we'll add the client
//                    // to the list of connected clients.
//                    tcpClient.ID = Guid.NewGuid();

//                    lock (m_tcpClients)
//                    {
//                        m_tcpClients.Add(tcpClient.ID, tcpClient);
//                    }

//                    OnClientConnected(tcpClient.ID);
//                }

//                // Used to count the number of bytes received in a single receive.
//                int bytesReceived = 0;

//                // Receiving of data from the client has been seperated into 2 different section resulting in some
//                // redundant coding. This is necessary to achive a high performance TCP server component since it
//                // may be used in real-time applications where performance is the key and evey millisecond  saved
//                // makes a big difference.
//                if ((m_receiveRawDataFunction != null) || (m_receiveRawDataFunction == null && !m_payloadAware))
//                {
//                    // In this section the consumer either wants to receive data and pass it on to a delegate or
//                    // receive data that doesn't contain metadata used for preserving message boundaries. In either
//                    // case we can use a static buffer that can be used over and over again for receiving data.
//                    while (true)
//                    {
//                        // Receive data into the static buffer.
//                        bytesReceived = tcpClient.Client.Receive(m_buffer, 0, m_buffer.Length, SocketFlags.None);
//                        tcpClient.LastReceiveTimestamp = DateTime.Now;

//                        // We start receiving zero-length data when a TCP connection is disconnected by the
//                        // opposite party. In such case we must consider ourself disconnected from the client.
//                        if (bytesReceived == 0)
//                            throw new SocketException(10101);

//                        // Post raw data to the delegate that is most likely used for real-time applications.
//                        if (m_receiveRawDataFunction != null)
//                            m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
//                        else
//                            ProcessReceivedClientData(m_buffer.CopyBuffer(0, bytesReceived), tcpClient);
//                    }
//                }
//                else
//                {
//                    // In this section we will be receiving data that has metadata used for preserving message
//                    // boundaries. Here a message (the payload) is sent by the other party along with some metadata
//                    // (payload header) prepended to the message. The metadata (payload header) consists of a 4-byte
//                    // marker used to mark the beginning of a message, followed by the message size (also 4-bytes),
//                    // followed by the actual message.
//                    int payloadSize = -1;
//                    int totalBytesReceived = 0;

//                    while (true)
//                    {
//                        // If we don't have the payload size, we'll begin by reading the payload header which
//                        // contains the payload size. Once we have the payload size we can receive payload.
//                        if (payloadSize == -1)
//                            tcpClient.DataBuffer = new byte[Payload.HeaderSize];

//                        // Since TCP is a streaming protocol we can receive a part of the available data and
//                        // the remaing data can be received in subsequent receives.
//                        bytesReceived = tcpClient.Client.Receive(tcpClient.DataBuffer, totalBytesReceived, (tcpClient.DataBuffer.Length - totalBytesReceived), SocketFlags.None);
//                        tcpClient.LastReceiveTimestamp = DateTime.Now;

//                        if (bytesReceived == 0)
//                            throw new SocketException(10101);

//                        if (payloadSize == -1)
//                        {
//                            // We don't what the payload size is, so we'll check if the data we have contains
//                            // the size of the payload we need to receive.
//                            payloadSize = Payload.GetSize(tcpClient.DataBuffer);

//                            if (payloadSize != -1 && payloadSize <= ClientBase.MaximumDataSize)
//                            {
//                                // We have a valid payload size, so we'll create a buffer that's big enough
//                                // to hold the entire payload. Remember, the payload at the most can be as big
//                                // as whatever the MaximumDataSize is.
//                                tcpClient.DataBuffer = new byte[payloadSize];
//                            }
//                        }
//                        else
//                        {
//                            totalBytesReceived += bytesReceived;

//                            if (totalBytesReceived == payloadSize)
//                            {
//                                // We've received the entire payload.
//                                ProcessReceivedClientData(tcpClient.DataBuffer, tcpClient);

//                                // Initialize for receiving the next payload.
//                                payloadSize = -1;
//                                totalBytesReceived = 0;
//                            }
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
//                // We are now done with the client.
//                if ((tcpClient != null) && (tcpClient.Client != null))
//                {
//                    tcpClient.Client.Close();
//                }

//                lock (m_pendingTcpClients)
//                {
//                    m_pendingTcpClients.Remove(tcpClient);
//                }

//                bool clientDisconnected = false;

//                lock (m_tcpClients)
//                {
//                    clientDisconnected = m_tcpClients.ContainsKey(tcpClient.ID);
//                    m_tcpClients.Remove(tcpClient.ID);
//                }

//                if (clientDisconnected)
//                    OnClientDisconnected(tcpClient.ID);
//            }
//        }

//        /// <summary>
//        /// This method processes the data received from the client.
//        /// </summary>
//        /// <param name="data">The data received from the client.</param>
//        /// <param name="tcpClient">The TCP client who sent the data.</param>
//        private void ProcessReceivedClientData(byte[] data, StateInfo<Socket> tcpClient)
//        {
//            if (tcpClient.ID == Guid.Empty && Handshake)
//            {
//                // Authentication is required, but not performed yet. When authentication is required
//                // the first message from the client must be information about itself.
//                HandshakeMessage clientInfo = Serialization.GetObject<HandshakeMessage>(GetActualData(data));

//                if ((clientInfo != null) && clientInfo.ID != Guid.Empty && clientInfo.Passphrase == HandshakePassphrase)
//                {
//                    // We'll generate a private key for the client if SecureSession is enabled.
//                    tcpClient.ID = clientInfo.ID;
//                    if (SecureSession) tcpClient.Passphrase = Cipher.GenerateKey();

//                    // We'll send our information to the client which may contain a private crypto key .
//                    byte[] myInfo = GetPreparedData(Serialization.GetBytes(new HandshakeMessage(ServerID, tcpClient.Passphrase)));

//                    if (m_payloadAware)
//                        myInfo = Payload.AddHeader(myInfo);

//                    tcpClient.Client.Send(myInfo);
//                    tcpClient.ID = clientInfo.ID;
//                    tcpClient.Client.ReceiveTimeout = 0; // We don't want to timeout while waiting for data from client.

//                    // The client's authentication is now complete.
//                    lock (m_pendingTcpClients)
//                    {
//                        m_pendingTcpClients.Remove(tcpClient);
//                    }

//                    lock (m_tcpClients)
//                    {
//                        m_tcpClients.Add(tcpClient.ID, tcpClient);
//                    }

//                    OnClientConnected(tcpClient.ID);
//                }
//                else
//                {
//                    // The first response from the client is either not information about itself, or
//                    // the information provided by the client is invalid.
//                    throw new ApplicationException("Failed to authenticate the client.");
//                }
//            }
//            else
//            {
//                // Decrypt the data usign private key if SecureSession is enabled.
//                if (SecureSession)
//                    data = Transport.DecryptData(data, tcpClient.Passphrase, Encryption);

//                // We'll pass the received data along to the consumer via event.
//                OnReceivedClientData(new IdentifiableItem<Guid, byte[]>(tcpClient.ID, data));
//            }
//        }

//        #endregion