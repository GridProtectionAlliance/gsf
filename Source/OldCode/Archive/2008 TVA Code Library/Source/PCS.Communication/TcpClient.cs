//*******************************************************************************************************
//  TcpClient.cs
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
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a TCP-based communication client.
    /// </summary>
    /// <remarks>
    /// PayloadAware enabled transmission can transmit up to 100MB of payload in a single transmission.
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
    ///     static TcpClient m_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the server.
    ///         m_client = new TcpClient("Server=h51g1d1; Port=8888");
    ///         m_client.Handshake = false;
    ///         m_client.PayloadAware = true;
    ///         m_client.ReceiveTimeout = -1;
    ///         m_client.MaxConnectionAttempts = 5;
    ///         //m_client.Encryption = CipherStrength.Level1;
    ///         //m_client.Compression = CompressionStrength.BestSpeed;
    ///         //m_client.SecureSession = true;
    ///         // Register event handlers.
    ///         m_client.Connecting += m_client_Connecting;
    ///         m_client.Connected += m_client_Connected;
    ///         m_client.Disconnected += m_client_Disconnected;
    ///         m_client.ReceiveDataComplete += m_client_ReceiveDataComplete;
    ///         // Start the server.
    ///         m_client.ConnectAsync();
    /// 
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             m_client.Send(input);
    ///         }
    /// 
    ///         m_client.Disconnect();
    ///     }
    /// 
    ///     static void m_client_Connecting(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void m_client_Connected(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void m_client_Disconnected(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void m_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", m_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class TcpClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost; Port=8888";

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        // Fields
        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private TransportProvider<Socket> m_tcpClient;
        private int m_connectionAttempts;
        private Dictionary<string, string> m_connectData;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        public TcpClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the client. See <see cref="DefaultConnectionString"/> for format.</param>
        public TcpClient(string connectString)
            : base(TransportProtocol.Tcp, connectString)
        {
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_tcpClient = new TransportProvider<Socket>();
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
        /// Gets the <see cref="TransportProvider{Socket}"/> object for the <see cref="TcpClient"/>.
        /// </summary>
        [Browsable(false)]
        public TransportProvider<Socket> Client
        {
            get
            {
                return m_tcpClient;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Disconnects the <see cref="TcpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                m_tcpClient.Provider.Close();
            }
        }

        /// <summary>
        /// Connects the <see cref="TcpClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="TcpClient"/> when it is connected.</exception>
        public override void ConnectAsync()
        {
            if (CurrentState == ClientState.Disconnected)
            {
                // Initialize if unitialized.
                Initialize();

                OnConnecting();
                if (m_tcpClient.Provider == null)
                {
                    // Create client socket to establish presence.
                    m_tcpClient.Provider = Transport.CreateSocket(0, ProtocolType.Tcp);
                }
                // Begin asynchronous connect operation and return wait handle for the asynchronous operation.
                m_tcpClient.Provider.BeginConnect(Transport.CreateEndPoint(m_connectData["server"], int.Parse(m_connectData["port"])), ConnectAsyncCallback, m_tcpClient);
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
        /// <exception cref="ArgumentException">Server property is missing.</exception>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("server"))
                throw new ArgumentException(string.Format("Server property is missing. Example: {0}.", DefaultConnectionString));

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
            return m_tcpClient.Passphrase;
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

            // Prepare for payload-aware transmission.
            if (m_payloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

            // Send payload to the client asynchronously.
            handle = m_tcpClient.Provider.BeginSend(data, offset, length, SocketFlags.None, SendPayloadAsyncCallback, m_tcpClient).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_tcpClient.SendBuffer = data;
            m_tcpClient.SendBufferOffset = offset;
            m_tcpClient.SendBufferLength = length;
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous connect operation.
        /// </summary>
        private void ConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            try
            {
                // Perform post-connect operations.
                m_connectionAttempts++;
                tcpClient.Provider.EndConnect(asyncResult);
                tcpClient.ID = this.ClientID;
                tcpClient.Passphrase = HandshakePassphrase;

                // We can proceed further with receiving data from the client.
                if (Handshake)
                {
                    // Handshaking must be performed. 
                    HandshakeMessage handshake = new HandshakeMessage();
                    handshake.ID = this.ClientID;
                    handshake.Passphrase = this.HandshakePassphrase;

                    // Prepare binary image of handshake to be transmitted.
                    tcpClient.SendBuffer = handshake.BinaryImage;
                    tcpClient.SendBufferOffset = 0;
                    tcpClient.SendBufferLength = tcpClient.SendBuffer.Length;
                    Payload.ProcessTransmit(ref tcpClient.SendBuffer, ref tcpClient.SendBufferOffset, ref tcpClient.SendBufferLength, Encryption, HandshakePassphrase, Compression);

                    // Transmit the prepared and processed handshake message.
                    tcpClient.Provider.Send(tcpClient.SendBuffer);

                    // Wait for the server's reponse to the handshake message.
                    ReceiveHandshakeAsync(tcpClient);
                }
                else
                {
                    // No handshaking to be performed.
                    OnConnected();
                    ReceivePayloadAsync(tcpClient);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused &&
                    (MaxConnectionAttempts == -1 || m_connectionAttempts < MaxConnectionAttempts))
                {
                    // Server is unavailable, so keep retrying connection to the server.
                    ConnectAsync();
                }
                else
                {
                    // For any other reason, clean-up as if the client was disconnected.
                    TerminateConnection(tcpClient, false);
                }
            }
            catch
            {
                // This is highly unlikely, but we must handle this situation just in-case.
                TerminateConnection(tcpClient, false);
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendPayloadAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            try
            {
                // Send operation is complete.
                tcpClient.Statistics.UpdateBytesSent(tcpClient.Provider.EndSend(asyncResult));
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        /// <summary>
        /// Initiate method for receving handshake response data.
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
        /// Callback method for asynchronous receive operation of handshake response data.
        /// </summary>
        private void ReceiveHandshakeAsyncCallback(IAsyncResult asyncResult)
        {
            TransportProvider<Socket> tcpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Handshake response is not recevied in a timely fashion.
                TerminateConnection(tcpClient, false);
                OnHandshakeProcessTimeout();
            }
            else
            {
                // Received handshake response from server so we'll process it.
                try
                {
                    // Update statistics and pointers.
                    tcpClient.Statistics.UpdateBytesReceived(tcpClient.Provider.EndReceive(asyncResult));
                    tcpClient.ReceiveBufferLength = tcpClient.Statistics.LastBytesReceived;

                    if (tcpClient.Statistics.LastBytesReceived == 0)
                        // Client disconnected gracefully.
                        throw new SocketException((int)SocketError.Disconnecting);

                    // Process the received handshake response message.
                    Payload.ProcessReceived(ref tcpClient.ReceiveBuffer, ref tcpClient.ReceiveBufferOffset, ref tcpClient.ReceiveBufferLength, Encryption, HandshakePassphrase, Compression);

                    HandshakeMessage handshake = new HandshakeMessage();
                    if (handshake.Initialize(tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferOffset, tcpClient.ReceiveBufferLength) != -1)
                    {
                        // Received handshake response message could be parsed.
                        this.ServerID = handshake.ID;
                        tcpClient.Passphrase = handshake.Passphrase;

                        // Client is now considered to be connected to the server.
                        OnConnected();
                        ReceivePayloadAsync(tcpClient);
                    }
                    else 
                    {
                        // Received handshake response message could not be parsed.
                        TerminateConnection(tcpClient, false);
                        OnHandshakeProcessUnsuccessful();
                    }
                }
                catch
                {
                    // This is most likely because the server forcibly disconnected the client.
                    TerminateConnection(tcpClient, false);
                    OnHandshakeProcessUnsuccessful();
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
            TransportProvider<Socket> tcpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveDataTimeout();
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
                            OnReceiveDataComplete(tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferLength);
                            ReceivePayloadAsync(tcpClient);
                        }
                        else
                        {
                            // We've not yet received the entire payload.
                            ReceivePayloadAwareAsync(tcpClient);
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Terminate connection when client is disposed.
                    TerminateConnection(tcpClient, true);
                }
                catch (SocketException)
                {
                    // Terminate connection when socket exception is encountered.
                    TerminateConnection(tcpClient, true);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        ReceivePayloadAsync(tcpClient);
                        OnReceiveDataException(ex);
                    }
                    catch
                    {
                        // Terminate connection if resuming receiving fails.
                        TerminateConnection(tcpClient, true);
                    }
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
            TransportProvider<Socket> tcpClient = (TransportProvider<Socket>)asyncResult.AsyncState;
            if (!asyncResult.IsCompleted)
            {
                // Timedout on reception of data so notify via event and continue waiting for data.
                OnReceiveDataTimeout();
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
                    OnReceiveDataComplete(tcpClient.ReceiveBuffer, tcpClient.ReceiveBufferLength);
                    ReceivePayloadUnawareAsync(tcpClient);
                }
                catch (ObjectDisposedException)
                {
                    // Terminate connection when client is disposed.
                    TerminateConnection(tcpClient, true);
                }
                catch (SocketException)
                {
                    // Terminate connection when socket exception is encountered.
                    TerminateConnection(tcpClient, true);
                }
                catch (Exception ex)
                {
                    try
                    {
                        // For any other exception, notify and resume receive.
                        ReceivePayloadAsync(tcpClient);
                        OnReceiveDataException(ex);
                    }
                    catch
                    {
                        // Terminate connection if resuming receiving fails.
                        TerminateConnection(tcpClient, true);
                    }
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
                OnDisconnected();
        }

        #endregion
    }
}