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
                    OnConnectionEstablished();
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
                        OnConnectionEstablished();
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
                OnConnectionTerminated();
        }

        #endregion
    }
}