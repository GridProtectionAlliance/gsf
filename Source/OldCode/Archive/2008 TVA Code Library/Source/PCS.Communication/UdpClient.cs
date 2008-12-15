//*******************************************************************************************************
//  UdpClient.cs
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
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload
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

namespace PCS.Communication
{
    /// <summary>
    /// Represents a UDP-based communication server.
    /// </summary>
    /// <remarks>
    /// Use <see cref="UdpClient"/> when the primary purpose is to receive data.
    /// </remarks>
    public class UdpClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ReceiveBufferSize"/> property.
        /// </summary>
        public new const int DefaultReceiveBufferSize = 32768;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888; Port=8989";

        // Fields
        //private bool m_destinationReachableCheck;
        private EndPoint m_udpServer;
        private TransportProvider<Socket> m_udpClient;
        private Dictionary<string, string> m_connectData;
        private Func<TransportProvider<Socket>, bool> m_receivedGoodbye;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        public UdpClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the client. See <see cref="DefaultConnectionString"/> for format.</param>
        public UdpClient(string connectString)
            : base(TransportProtocol.Udp, connectString)
        {
            base.ReceiveBufferSize = DefaultReceiveBufferSize;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="TransportProvider{Socket}"/> object for the <see cref="UdpClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<Socket> Client
        {
            get
            {
                return m_udpClient;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Disconnects the <see cref="UdpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                if (Handshake)
                {
                    // Handshake is enabled so we'll notify the client.
                    m_udpClient.SendBuffer = new GoodbyeMessage(m_udpClient.ID).BinaryImage;
                    m_udpClient.SendBufferOffset = 0;
                    m_udpClient.SendBufferLength = m_udpClient.SendBuffer.Length;
                    Payload.ProcessTransmit(ref m_udpClient.SendBuffer, ref m_udpClient.SendBufferOffset, ref m_udpClient.SendBufferLength, Encryption, m_udpClient.Passphrase, Compression);

                    m_udpClient.Provider.SendTo(m_udpClient.SendBuffer, m_udpServer);
                }

                m_udpClient.Provider.Close();
            }
        }

        /// <summary>
        /// Connects the <see cref="UdpClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="UdpClient"/> when it is connected.</exception>
        public override void ConnectAsync()
        {
            if (CurrentState == ClientState.Disconnected)
            {
                // Initialize if unitialized.
                Initialize();

                OnConnectionAttempt();
                m_udpClient = new TransportProvider<Socket>();
                m_udpClient.ID = this.ClientID;
                m_udpClient.Passphrase = HandshakePassphrase;
                m_udpClient.ReceiveBuffer = new byte[ReceiveBufferSize];
                // Create client socket to establish presence.
                if (m_connectData.ContainsKey("server"))
                {
                    string[] parts = m_connectData["server"].Split(':');
                    if (parts.Length == 2)
                    {
                        m_udpServer = Transport.CreateEndPoint(parts[0], int.Parse(parts[1]));
                    }
                    else
                    {
                        throw new FormatException("Format of Server property in ConnectionString is incorrect. Example: Server=udpserver:8888.");
                    }
                }
                else
                {
                    if (Handshake)
                        throw new InvalidOperationException("Handshake requires Server property in the ConnectionString.");

                    m_udpServer = Transport.CreateEndPoint(string.Empty, 0);
                }
                m_udpClient.Provider = Transport.CreateSocket(int.Parse(m_connectData["port"]), ProtocolType.Udp);

                if (Handshake)
                {
                    // Handshaking must be performed. 
                    m_receivedGoodbye = DoGoodbyeCheck;
                    HandshakeMessage handshake = new HandshakeMessage();
                    handshake.ID = this.ClientID;
                    handshake.Passphrase = this.HandshakePassphrase;

                    // Prepare binary image of handshake to be transmitted.
                    m_udpClient.SendBuffer = handshake.BinaryImage;
                    m_udpClient.SendBufferOffset = 0;
                    m_udpClient.SendBufferLength = m_udpClient.SendBuffer.Length;
                    Payload.ProcessTransmit(ref m_udpClient.SendBuffer, ref m_udpClient.SendBufferOffset, ref m_udpClient.SendBufferLength, Encryption, HandshakePassphrase, Compression);

                    // Transmit the prepared and processed handshake message.
                    m_udpClient.Provider.SendTo(m_udpClient.SendBuffer, m_udpServer);

                    // Wait for the server's reponse to the handshake message.
                    ReceiveHandshakeAsync(m_udpClient);
                }
                else
                {
                    m_receivedGoodbye = NoGoodbyeCheck;
                    OnConnectionEstablish();
                    ReceivePayloadAsync(m_udpClient);
                }
            }
            else
            {
                throw new InvalidOperationException("Client is currently not disconnected.");
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888; Port=8989
            // Old Format: Server=localhost; RemotePort=8888; LocalPort=8888
            if (m_connectData.ContainsKey("localport") && !m_connectData.ContainsKey("port"))
                m_connectData.Add("port", m_connectData["localport"]);

            if (m_connectData.ContainsKey("server") && m_connectData.ContainsKey("remoteport"))
                m_connectData["server"] = m_connectData["server"] + ":" + m_connectData["remoteport"];

            if (!m_connectData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing. Example: {0}.", DefaultConnectionString));

            if (!Transport.IsPortNumberValid(m_connectData["port"]))
                throw new ArgumentOutOfRangeException("connectionString", string.Format("Port number must between {0} and {1}.", Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Gets the passphrase to be used for ciphering client data.
        /// </summary>
        /// <returns>Cipher passphrase.</returns>
        protected override string GetSessionPassphrase()
        {
            return m_udpClient.Passphrase;
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send payload to the client asynchronously.
            handle = m_udpClient.Provider.BeginSendTo(data, offset, length, SocketFlags.None, m_udpServer, SendPayloadAsyncCallback, m_udpClient).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_udpClient.SendBuffer = data;
            m_udpClient.SendBufferOffset = offset;
            m_udpClient.SendBufferLength = length;
            OnSendDataStart();

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
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        private void ReceiveHandshakeAsync(TransportProvider<Socket> worker)
        {
            // Receive data asynchronously with a timeout.
            worker.WaitAsync(HandshakeTimeout,
                             ReceiveHandshakeAsyncCallback,
                             worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                              worker.ReceiveBufferOffset,
                                                              worker.ReceiveBuffer.Length,
                                                              SocketFlags.None,
                                                              ref m_udpServer,
                                                              ReceiveHandshakeAsyncCallback,
                                                              worker));
        }

        private void ReceiveHandshakeAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Handshake response is not recevied in a timely fashion.
                TerminateConnection(udpClient, false);
                OnHandshakeProcessTimeout();
            }
            else
            {
                // Received handshake response from server so we'll process it.
                try
                {
                    // Update statistics and pointers.
                    udpClient.Statistics.UpdateBytesReceived(udpClient.Provider.EndReceiveFrom(asyncResult, ref m_udpServer));
                    udpClient.ReceiveBufferLength = udpClient.Statistics.LastBytesReceived;

                    // Process the received handshake response message.
                    Payload.ProcessReceived(ref udpClient.ReceiveBuffer, ref udpClient.ReceiveBufferOffset, ref udpClient.ReceiveBufferLength, Encryption, HandshakePassphrase, Compression);

                    HandshakeMessage handshake = new HandshakeMessage();
                    if (handshake.Initialize(udpClient.ReceiveBuffer, udpClient.ReceiveBufferOffset, udpClient.ReceiveBufferLength) != -1)
                    {
                        // Received handshake response message could be parsed.
                        this.ServerID = handshake.ID;
                        udpClient.Passphrase = handshake.Passphrase;

                        // Client is now considered to be connected to the server.
                        OnConnectionEstablish();
                        ReceivePayloadAsync(udpClient);
                    }
                    else
                    {
                        // Received handshake response message could not be parsed.
                        TerminateConnection(udpClient, false);
                        OnHandshakeProcessUnsuccessful();
                    }
                }
                catch
                {
                    // This is most likely because the server forcibly disconnected the client.
                    TerminateConnection(udpClient, false);
                    OnHandshakeProcessUnsuccessful();
                }
            }
        }

        private void ReceivePayloadAsync(TransportProvider<Socket> worker)
        {
            if (ReceiveTimeout == -1)
            {
                // Wait for data indefinitely.
                worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                 worker.ReceiveBufferOffset,
                                                 worker.ReceiveBuffer.Length,
                                                 SocketFlags.None,
                                                 ref m_udpServer,
                                                 ReceivePayloadAsyncCallback,
                                                 worker);
            }
            else
            {
                // Wait for data with a timeout.
                worker.WaitAsync(ReceiveTimeout,
                                 ReceivePayloadAsyncCallback,
                                 worker.Provider.BeginReceiveFrom(worker.ReceiveBuffer,
                                                                  worker.ReceiveBufferOffset,
                                                                  worker.ReceiveBuffer.Length,
                                                                  SocketFlags.None,
                                                                  ref m_udpServer,
                                                                  ReceivePayloadAsyncCallback,
                                                                  worker));
            }
        }

        private void ReceivePayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> udpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveDataTimeout();
                udpClient.WaitAsync(ReceiveTimeout, ReceivePayloadAsyncCallback, asyncResult);
            }
            else
            {
                try
                {
                    // Update statistics and pointers.
                    udpClient.Statistics.UpdateBytesReceived(udpClient.Provider.EndReceiveFrom(asyncResult, ref m_udpServer));
                    udpClient.ReceiveBufferLength = udpClient.Statistics.LastBytesReceived;
                    
                    // Received a goodbye message from the server.
                    if (m_receivedGoodbye(udpClient))
                        throw new SocketException((int)SocketError.Disconnecting);

                    // Notify of received data and resume receive operation.
                    OnReceiveDataComplete(udpClient.ReceiveBuffer, udpClient.ReceiveBufferLength);
                    ReceivePayloadAsync(udpClient);
                }
                catch (ObjectDisposedException)
                {
                    // Terminate connection when client is disposed.
                    TerminateConnection(udpClient, true);
                }
                catch (SocketException ex)
                {
                    if (!Handshake && ex.SocketErrorCode == SocketError.ConnectionReset)
                        // This occurs if server is not listening for data.
                        ReceivePayloadAsync(udpClient);
                    else
                        // Terminate connection on other type of exception.
                        TerminateConnection(udpClient, true);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        ReceivePayloadAsync(udpClient);
                        OnReceiveDataException(ex);
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
                OnConnectionTerminate();
        }

        #endregion
    }
}








////*******************************************************************************************************
////  UdpClient.cs
////  Copyright © 2008 - TVA, all rights reserved - Gbtc
////
////  Build Environment: C#, Visual Studio 2008
////  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
////      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
////       Phone: 423/751-3024
////       Email: pcpatel@tva.gov
////
////  Code Modification History:
////  -----------------------------------------------------------------------------------------------------
////  07/06/2006 - Pinal C. Patel
////       Original version of source code generated
////  09/06/2006 - J. Ritchie Carroll
////       Added bypass optimizations for high-speed socket access
////  09/27/2007 - J. Ritchie Carroll
////       Added disconnect timeout overload
////  09/29/2008 - James R Carroll
////       Converted to C#.
////
////*******************************************************************************************************

//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.ComponentModel;
//using System.Collections.Generic;
//using PCS.Configuration;
//using PCS.Threading;

//namespace PCS.Communication
//{
//    /// <summary>
//    /// Represents a UDP-based communication server.
//    /// </summary>
//    /// <remarks>
//    /// UDP by nature is a connectionless protocol, but with this implementation of UDP server we can have a
//    /// connectionfull session with the server by enabling Handshake. This in-turn enables us to take advantage
//    /// of SecureSession which otherwise is not possible.
//    /// </remarks>
//    public class UdpClient : ClientBase
//    {
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
//        private IPEndPoint m_udpServer;
//        private StateInfo<Socket> m_udpClient;
//#if ThreadTracking
//        private ManagedThread m_receivingThread;
//        private ManagedThread m_connectionThread;
//#else
//        private Thread m_receivingThread;
//        private Thread m_connectionThread;
//#endif
//        private Dictionary<string, string> m_connectionData;
//        private bool m_disposed;

//        #endregion

//        #region [ Constructors ]

//        public UdpClient()
//        {
//            m_payloadAware = false;
//            m_destinationReachableCheck = false;
//            base.ConnectionString = "Server=localhost; RemotePort=8888; LocalPort=8888";
//            base.Protocol = TransportProtocol.Udp;
//            base.ReceiveBufferSize = MaximumUdpDatagramSize;
//        }

//        /// <summary>
//        /// Initializes a instance of PCS.Communication.UdpClient with the specified data.
//        /// </summary>
//        /// <param name="connectionString">The connection string containing the data required for initializing the UDP client.</param>
//        public UdpClient(string connectionString)
//            : this()
//        {
//            base.ConnectionString = connectionString;
//        }

//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
//        /// </summary>
//        /// <value>Receive buffer size</value>
//        /// <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while client is connected</exception>
//        /// <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
//        /// <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
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
//                        if (m_connectionThread != null)
//                        {
//                            if (m_connectionThread.IsAlive)
//                                m_connectionThread.Abort();
//                        }
//                        m_connectionThread = null;

//                        if (m_receivingThread != null)
//                        {
//                            if (m_receivingThread.IsAlive)
//                                m_receivingThread.Abort();
//                        }
//                        m_receivingThread = null;

//                        if (m_udpClient != null)
//                        {
//                            if (m_udpClient.Client != null && m_udpClient.Client.Connected)
//                            {
//                                m_udpClient.Client.Shutdown(SocketShutdown.Both);
//                                m_udpClient.Client.Close();
//                            }
//                            m_udpClient.Client = null;
//                        }
//                        m_udpClient = null;
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
//        /// Cancels any active attempts of connecting to the server.
//        /// </summary>
//        public override void CancelConnect()
//        {
//            if (base.Enabled)
//            {
//                // We'll abort the thread on which the client is initialized if it's alive.
//                if (m_connectionThread.IsAlive)
//                    m_connectionThread.Abort();

//                // *** The above and below conditions are mutually exclusive ***

//                // If the client has Handshake enabled, it is not considered connected until a handshake message is
//                // received from the server. So, if the thread on which we receive data from the server is alive, but
//                // the client is not yet flagged as connected, we'll abort that thread.
//                if (!base.IsConnected && m_receivingThread.IsAlive)
//                {
//                    m_receivingThread.Abort();
//                    OnConnectingCancelled(EventArgs.Empty);
//                }
//            }
//        }

//        /// <summary>
//        /// Connects to the server asynchronously.
//        /// </summary>
//        public override void Connect()
//        {
//            if (base.Enabled && !base.IsConnected && ValidConnectionString(base.ConnectionString))
//            {
//                // Start the thread on which the client will be initialized.
//#if ThreadTracking
//                m_connectionThread = new ManagedThread(ConnectToServer);
//                m_connectionThread.Name = "PCS.Communication.UdpClient.ConnectToServer()";
//#else
//                m_connectionThread = new Thread(new System.Threading.ThreadStart(ConnectToServer));
//#endif
//                m_connectionThread.Start();
//            }

//        }

//        /// <summary>
//        /// Disconnects client from the connected server.
//        /// </summary>
//        public override void Disconnect(int timeout)
//        {
//            CancelConnect(); // Cancel any active connection attempts.

//            if (base.Enabled && base.IsConnected && m_udpClient != null && m_udpClient.Client != null)
//            {
//                if (base.Handshake)
//                {
//                    // We have a connectionfull session with the server, so we'll send a goodbye message to the server
//                    // indicating the that the session has ended.
//                    byte[] goodbye = GetPreparedData(Serialization.GetBytes(new GoodbyeMessage(m_udpClient.ID)));

//                    // Add payload header if client-server communication is PayloadAware.
//                    if (m_payloadAware)
//                    {
//                        goodbye = Payload.AddHeader(goodbye);
//                    }

//                    try
//                    {
//                        m_udpClient.Client.SendTo(goodbye, m_udpServer);
//                    }
//                    catch (ObjectDisposedException)
//                    {
//                        // Its OK to igonore ObjectDisposedException which we might encounter if Disconnect() method is
//                        // called consecutively within a very short duration (before the client is flagged as disconnected).
//                    }
//                }

//                // Close the UDP socket.
//                m_udpClient.Client.Shutdown(SocketShutdown.Both);

//                // JRC: Allowing call with disconnect timeout...
//                if (timeout <= 0)
//                    m_udpClient.Client.Close();
//                else
//                    m_udpClient.Client.Close(timeout);
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

//        /// <summary>
//        /// Sends prepared data to the server.
//        /// </summary>
//        /// <param name="data">The prepared data that is to be sent to the server.</param>
//        protected override void SendPreparedData(byte[] data)
//        {
//            if (base.Enabled && base.IsConnected)
//            {
//                // We'll check if the server is reachable before send data to it.
//                if (m_destinationReachableCheck && !Transport.IsDestinationReachable(m_udpServer))
//                    return;

//                // Encrypt the data with private key if SecureSession is enabled.
//                if (base.SecureSession)
//                    data = Transport.EncryptData(data, 0, data.Length, m_udpClient.Passphrase, base.Encryption);

//                // Add payload header if client-server communication is PayloadAware.
//                if (m_payloadAware)
//                    data = Payload.AddHeader(data);

//                OnSendDataBegin(new IdentifiableItem<Guid, byte[]>(ClientID, data));

//                // Since UDP is a Datagram protocol, we must make sure that the datagram we transmit are no bigger
//                // than what the server can receive. For this reason we'll break up the data into multiple datagrams
//                // if data being transmitted is bigger than what the server can receive. Since we don't know what the
//                // the server's ReceiveBufferSize is, we assume it to be the same as the client's. And it is for this
//                // reason it is important that the ReceiveBufferSize of both the client and server are the same.
//                int toIndex = 0;
//                int datagramSize = base.ReceiveBufferSize;

//                if (data.Length > datagramSize)
//                    toIndex = data.Length - 1;

//                for (int i = 0; i <= toIndex; i += datagramSize)
//                {
//                    // Last or the only datagram in the series.
//                    if (data.Length - i < datagramSize)
//                        datagramSize = data.Length - i;

//                    // PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
//                    m_udpClient.Client.SendTo(data, i, datagramSize, SocketFlags.None, m_udpServer);

//                    //' We'll send the data asynchronously for better performance.
//                    //m_udpClient.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, m_udpServer, Nothing, Nothing)
//                }

//                OnSendDataComplete(new IdentifiableItem<Guid, byte[]>(ClientID, data));
//            }
//        }

//        protected override bool ValidConnectionString(string connectionString)
//        {
//            if (!string.IsNullOrEmpty(connectionString))
//            {
//                m_connectionData = connectionString.ParseKeyValuePairs();

//                // At the very least the connection string must have a local port specified and can optionally have a
//                // server and a remote port. Server and remote port is required when Handshake is enable, but if they
//                // are not specified then an arbitrary server enpoint will be created and any attempt of sending data
//                // to the server will fail. So, it becomes the consumer's responsibility to provide a valid server name
//                // and remote port if Handshake is enabled. At the same time when Handshake is enabled, the local port
//                // value will be ignored even if it is specified.
//                if ((m_connectionData.ContainsKey("localport") && Transport.IsValidPortNumber(m_connectionData["localport"])) ||
//                    (m_connectionData.ContainsKey("server") && !string.IsNullOrEmpty(m_connectionData["server"])) &&
//                    (m_connectionData.ContainsKey("port") && Transport.IsValidPortNumber(m_connectionData["port"])) ||
//                    m_connectionData.ContainsKey("remoteport") && Transport.IsValidPortNumber(m_connectionData["remoteport"]))
//                {
//                    // The connection string must always contain the following:
//                    // >> localport - Port number on which the client is listening for data.
//                    // OR
//                    // >> server - Name or IP of the machine machine on which the server is running.
//                    // >> port or remoteport - Port number on which the server is listening for connections.
//                    return true;
//                }
//                else
//                {
//                    // Connection string is not in the expected format.
//                    StringBuilder exceptionMessage = new StringBuilder();

//                    exceptionMessage.Append("Connection string must be in the following format:");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("   [Server=Server name or IP;] [[Remote]Port=Server port number;] LocalPort=Local port number");
//                    exceptionMessage.AppendLine();
//                    exceptionMessage.Append("Text between square brackets, [...], is optional.");

//                    throw new ArgumentException(exceptionMessage.ToString());
//                }
//            }
//            else
//            {
//                throw new ArgumentNullException("ConnectionString");
//            }
//        }

//        /// <summary>
//        /// Connects to the server.
//        /// </summary>
//        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
//        private void ConnectToServer()
//        {
//            int connectionAttempts = 0;

//            while (base.MaximumConnectionAttempts == -1 || connectionAttempts < base.MaximumConnectionAttempts)
//            {
//                try
//                {
//                    OnConnecting(EventArgs.Empty);

//                    // When the client is not intended for communicating with the server, the "LocalPort" value will be
//                    // present and "Server" and "Port" or "RemotePort" values may not be present in the connection string.
//                    // In this case we'll use the default values for server (localhost) and remoteport (0) to create an
//                    // imaginary server endpoint.
//                    // When the client is intended for communicating with the server, the "Server" and "Port" or
//                    // "RemotePort" will be present along with the "LocalPort" value. The "LocalPort" value however
//                    // becomes optional when client is configured to do Handshake with the server. When Handshake is
//                    // enabled, we let the system assign a port to us and the server will then send data to us at the
//                    // assigned port.
//                    string server = "localhost";
//                    int localPort = 0;
//                    int remotePort = 0;

//                    if (m_connectionData.ContainsKey("server"))
//                        server = m_connectionData["server"];

//                    if (m_connectionData.ContainsKey("port"))
//                    {
//                        remotePort = int.Parse(m_connectionData["port"]);
//                    }
//                    else if (m_connectionData.ContainsKey("remoteport"))
//                    {
//                        remotePort = int.Parse(m_connectionData["remoteport"]);
//                    }

//                    if (!base.Handshake)
//                        localPort = int.Parse(m_connectionData["localport"]);

//                    // Create the server endpoint that will be used for sending data.
//                    m_udpServer = Transport.GetIpEndPoint(server, remotePort);

//                    // Create a UDP socket and bind it to a local endpoint for receiving data.
//                    m_udpClient = new StateInfo<Socket>();
//                    m_udpClient.ID = base.ClientID;
//                    m_udpClient.Passphrase = base.HandshakePassphrase;
//                    m_udpClient.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//                    m_udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));

//                    // Imposed a timeout on receiving data if specified.
//                    if (base.ReceiveTimeout != -1)
//                        m_udpClient.Client.ReceiveTimeout = base.ReceiveTimeout;

//                    // Start listening for data from the server on a seperate thread.
//#if ThreadTracking
//                    m_receivingThread = new ManagedThread(ReceiveServerData);
//                    m_receivingThread.Name = "PCS.Communication.UdpClient.ReceiveServerData() [" + ClientID.ToString() + "]";
//#else
//                    m_receivingThread = new Thread(new System.Threading.ThreadStart(ReceiveServerData));
//#endif

//                    m_receivingThread.Start();

//                    break; // The process of initiating the connection is complete.
//                }
//                catch (ThreadAbortException)
//                {
//                    // We'll stop trying to connect if a System.Threading.ThreadAbortException exception is encountered.
//                    // This will be the case when the thread is deliberately aborted in CancelConnect() method in which
//                    // case we want to stop attempting to connect to the server.
//                    OnConnectingCancelled(EventArgs.Empty);
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    connectionAttempts++;
//                    OnConnectingException(ex);
//                }
//            }
//        }

//        private void ReceiveServerData()
//        {
//            try
//            {
//                // In order to make UDP connectionfull, which can be done by enabling Handshake, we must send our
//                // information to the server so that it is knowledge of the client and in return the server sends us
//                // its about itself, so we have knowledge of the server. This allows UDP to function more like TCP in
//                // the sense that multiple UDP client can be connected to a server when both server and clien are on
//                // the same machine.
//                int connectionAttempts = 0;

//                while (base.MaximumConnectionAttempts == -1 || connectionAttempts < base.MaximumConnectionAttempts)
//                {
//                    OnConnecting(EventArgs.Empty);

//                    if (base.Handshake)
//                    {
//                        // Handshaking is enabled so we'll send our information to the server.
//                        byte[] myInfo = GetPreparedData(Serialization.GetBytes(new HandshakeMessage(m_udpClient.ID, m_udpClient.Passphrase)));

//                        // Add payload header if client-server communication is PayloadAware.
//                        if (m_payloadAware)
//                            myInfo = Payload.AddHeader(myInfo);

//                        m_udpClient.Client.SendTo(myInfo, m_udpServer);
//                    }
//                    else
//                    {
//                        // If handshaking is disabled, the client is considered to be connected to the server.
//                        OnConnected(EventArgs.Empty);
//                    }

//                    // Used to count the number of bytes received in a single receive.
//                    int bytesReceived = 0;

//                    // Receiving of data from the server has been seperated into 2 different section resulting in
//                    // some redundant coding. This is necessary to achive a high performance UDP client component
//                    // since it may be used in real-time applications where performance is the key and evey
//                    // millisecond saved makes a big difference.
//                    if (m_receiveRawDataFunction != null || (m_receiveRawDataFunction == null && !m_payloadAware))
//                    {
//                        // In this section the consumer either wants to receive the datagrams and pass it on to a
//                        // delegate or receive datagrams that don't contain metadata used for re-assembling the
//                        // datagrams into the original message and be notified via events. In either case we can use
//                        // a static buffer that can be used over and over again for receiving datagrams as long as
//                        // the datagrams received are not bigger than the receive buffer.
//                        while (true)
//                        {
//                            try
//                            {
//                                EndPoint udpServer = m_udpServer;

//                                // Receive a datagram into the static buffer.
//                                bytesReceived = m_udpClient.Client.ReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, ref udpServer);

//                                if (m_receiveRawDataFunction != null)
//                                {
//                                    // Post the received datagram to the delegate.
//                                    m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
//                                    m_totalBytesReceived += bytesReceived;
//                                    continue;
//                                }
//                                else
//                                {
//                                    ProcessReceivedServerData(m_buffer.CopyBuffer(0, bytesReceived));
//                                }

//                                // If Handshake is enabled and we haven't received server information than we're not
//                                // considered as connected and so we'll keep trying to connect.
//                                if (!base.IsConnected)
//                                {
//                                    break;
//                                }
//                            }
//                            catch (SocketException ex)
//                            {
//                                switch (ex.SocketErrorCode)
//                                {
//                                    case SocketError.TimedOut:
//                                        HandleReceiveTimeout();
//                                        break;
//                                    case SocketError.ConnectionReset:
//                                        // We'll encounter this exception when we try sending our information to the
//                                        // server and the server is unreachable (or not running). So, keep trying!
//                                        OnConnectingException(ex);
//                                        break;
//                                    default:
//                                        throw;
//                                }
//                            }
//                        }
//                    }
//                    else
//                    {
//                        // In this section we will be receiving datagrams in which a single datagrams may contain
//                        // the entire message or a part of the message (i.e. A message too big to fit in a datagram
//                        // when sending is split up into multiple datagrams). In either case the first datagram will
//                        // contain the metadata (payload header) used for re-assembling the datagrams into the
//                        // original message (payload). The metadata consists of a 4-byte marker used to identify the
//                        // first datagram in the series, followed by the message size (also 4-bytes), followed by the
//                        // actual message.
//                        int payloadSize = -1;
//                        int totalBytesReceived = 0;

//                        while (true)
//                        {
//                            if (payloadSize == -1)
//                                m_udpClient.DataBuffer = new byte[base.ReceiveBufferSize];

//                            try
//                            {
//                                EndPoint udpServer = m_udpServer;

//                                // Since UDP is a datagram protocol, we must receive the entire datagram and not just
//                                // a portion if it. This also means that our receive buffer must be as big as the
//                                // datagram that is to be received.
//                                bytesReceived = m_udpClient.Client.ReceiveFrom(m_udpClient.DataBuffer, totalBytesReceived, (m_udpClient.DataBuffer.Length - totalBytesReceived), SocketFlags.None, ref udpServer);

//                                if (payloadSize == -1)
//                                {
//                                    // We don't have the payload size, so we'll check if the datagram we received
//                                    // contains the payload size. Remember, only the first datagram (even in a
//                                    // series, if the message needs to be broken down into multiple datagrams)
//                                    // contains the payload size.
//                                    payloadSize = Payload.GetSize(m_udpClient.DataBuffer);
//                                    if (payloadSize != -1 && payloadSize <= ClientBase.MaximumDataSize)
//                                    {
//                                        // We have a valid payload size.
//                                        byte[] payload = Payload.Retrieve(m_udpClient.DataBuffer);

//                                        // We'll extract the payload we've received in the datagram. It may be
//                                        // that this is the only datagram in the series and that this datagram
//                                        // contains the entire payload; this is tested in the code below.
//                                        m_udpClient.DataBuffer = new byte[payloadSize];
//                                        Buffer.BlockCopy(payload, 0, m_udpClient.DataBuffer, 0, payload.Length);
//                                        bytesReceived = payload.Length;
//                                    }
//                                }

//                                totalBytesReceived += bytesReceived;
//                                if (totalBytesReceived == payloadSize)
//                                {
//                                    // We've received the entire payload.
//                                    ProcessReceivedServerData(m_udpClient.DataBuffer);

//                                    // Initialize for receiving the next payload.
//                                    payloadSize = -1;
//                                    totalBytesReceived = 0;
//                                }

//                                if (!base.IsConnected)
//                                {
//                                    break;
//                                }
//                            }
//                            catch (SocketException ex)
//                            {
//                                switch (ex.SocketErrorCode)
//                                {
//                                    case SocketError.TimedOut:
//                                        HandleReceiveTimeout();
//                                        break;
//                                    case SocketError.ConnectionReset:
//                                        OnConnectingException(ex);
//                                        break;
//                                    case SocketError.MessageSize:
//                                        // When in "PayloadAware" mode, we may by receving a payload broken down into
//                                        // a series of datagrams and if one of the datagrams from that series is
//                                        // dropped, we'll encounter this exception because we'll probably be expecting
//                                        // a datagram of one size whereas we receive a datagram of a different size
//                                        // (most likely bigger than the size we're expecting). In this case we'll
//                                        // drop the partial content of the payload we've received so far and go back
//                                        // to receiving the next payload.
//                                        payloadSize = -1;
//                                        totalBytesReceived = 0;
//                                        break;
//                                    default:
//                                        throw;
//                                }
//                            }
//                        }
//                    }

//                    // If we're here, it means that Handshake is enabled and we were unable to get a response back
//                    // from the server, so we must keep trying to connect to the server.
//                    connectionAttempts++;
//                }
//            }
//            catch
//            {
//                // We don't need to take any action when an exception is encountered.
//            }
//            finally
//            {
//                if (m_udpClient != null && m_udpClient.Client != null)
//                    m_udpClient.Client.Close();

//                if (base.IsConnected)
//                    OnDisconnected(EventArgs.Empty);
//            }
//        }

//        /// <summary>
//        /// This method will not be required once the bug in .Net Framwork is fixed.
//        /// </summary>
//        private void HandleReceiveTimeout()
//        {
//            OnReceiveTimedOut(EventArgs.Empty); // Notify that a timeout has been encountered.

//            // TODO: See if this bug has been fixed in .NET 3.5...
//            // NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
//            // Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
//            m_udpClient.Client.Blocking = true; // <= Temporary bug fix!
//        }

//        /// <summary>
//        /// This method processes the data received from the server.
//        /// </summary>
//        /// <param name="data">The data received from the server.</param>
//        private void ProcessReceivedServerData(byte[] data)
//        {
//            // Don't proceed further if there is not data to process.
//            if (data.Length == 0)
//                return;

//            if (base.ServerID == Guid.Empty && base.Handshake)
//            {
//                // Handshaking is to be performed, but it's not complete yet.

//                HandshakeMessage serverInfo = Serialization.GetObject<HandshakeMessage>(GetActualData(data));
//                if (serverInfo != null && serverInfo.ID != Guid.Empty)
//                {
//                    // Authentication was successful and the server responded with its information.
//                    base.ServerID = serverInfo.ID;
//                    m_udpClient.Passphrase = serverInfo.Passphrase;
//                    OnConnected(EventArgs.Empty);
//                }
//            }
//            else
//            {
//                // Handshaking is enabled and the server has sent up nootification to disconnect.
//                if (base.Handshake && (Serialization.GetObject<GoodbyeMessage>(GetActualData(data)) != null))
//                    throw new SocketException(10101);

//                // Decrypt the data usign private key if SecureSession is enabled.
//                if (base.SecureSession)
//                {
//                    data = Transport.DecryptData(data, m_udpClient.Passphrase, base.Encryption);
//                }

//                // We'll pass the received data along to the consumer via event.
//                OnReceivedData(new IdentifiableItem<Guid, byte[]>(ServerID, data));
//            }
//        }

//        #endregion
//    }
//}