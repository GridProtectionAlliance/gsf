//*******************************************************************************************************
//  TcpServer.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using TVA.Threading;
using TVA.Configuration;
using TVA.Security.Cryptography;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a TCP-based communication server.
    /// </summary>
    /// <remarks>
    /// PayloadAware enabled transmission can transmit up to 100MB of payload in a single transmission.
    /// </remarks>
    public partial class TcpServer : CommunicationServerBase
    {
        #region [ Members ]

        // Fields
        private bool m_payloadAware;
        private Socket m_tcpServer;
        private Dictionary<Guid, StateInfo<Socket>> m_tcpClients;
        private List<StateInfo<Socket>> m_pendingTcpClients;
        private Dictionary<string, string> m_configurationData;
#if ThreadTracking
        private ManagedThread m_listenerThread;
#else
		private Thread m_listenerThread;
#endif
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public TcpServer()
        {
            // Setup the instance defaults.
            m_payloadAware = false;
            m_tcpClients = new Dictionary<Guid, StateInfo<System.Net.Sockets.Socket>>();
            m_pendingTcpClients = new List<StateInfo<System.Net.Sockets.Socket>>();

            base.ConfigurationString = "Port=8888";
            base.Protocol = TransportProtocol.Tcp;
        }

        /// <summary>
        /// Initializes a instance of TVA.Communication.TcpServer with the specified data.
        /// </summary>
        /// <param name="configurationString">The connection string containing the data required for the TCP server to run.</param>
        public TcpServer(string configurationString)
            : this()
        {
            base.ConfigurationString = configurationString;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value indicating whether the message boundaries are to be preserved during transmission.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// True if the message boundaries are to be preserved during transmission; otherwise False.
        /// </returns>
        /// <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
        [Description("Indicates whether the message boundaries are to be preserved during transmission. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(typeof(bool), "False")]
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
        /// Gets the System.Net.Sockets.Socket of the server.
        /// </summary>
        /// <returns>The System.Net.Sockets.Socket of the server.</returns>
        [Browsable(false)]
        public Socket Server
        {
            get
            {
                return m_tcpServer;
            }
        }

        /// <summary>
        /// Gets the current states of all connected clients which includes the System.Net.Sockets.Socket of clients.
        /// </summary>
        /// <remarks>
        /// The current states of all connected clients which includes the System.Net.Sockets.Socket of clients.
        /// </remarks>
        [Browsable(false)]
        public List<StateInfo<Socket>> Clients
        {
            get
            {
                List<StateInfo<Socket>> clientList = new List<StateInfo<Socket>>();
                lock (m_tcpClients)
                {
                    clientList.AddRange(m_tcpClients.Values);
                }

                return clientList;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="FileClient" /> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_listenerThread != null)
                        {
                            if (m_listenerThread.IsAlive)
                                m_listenerThread.Abort();
                        }
                        m_listenerThread = null;

                        if (m_tcpServer != null)
                        {
                            m_tcpServer.Shutdown(SocketShutdown.Both);
                        }
                        m_tcpServer = null;

                        if (m_tcpClients != null)
                        {
                            foreach (StateInfo<Socket> client in m_tcpClients.Values)
                            {
                                if ((client != null) && (client.Client != null))
                                    client.Client.Shutdown(SocketShutdown.Both);
                            }

                            m_tcpClients.Clear();
                        }
                        m_tcpClients = null;

                        if (m_pendingTcpClients != null)
                        {
                            foreach (StateInfo<Socket> client in m_pendingTcpClients)
                            {
                                if ((client != null) && (client.Client != null))
                                    client.Client.Shutdown(SocketShutdown.Both);
                            }

                            m_pendingTcpClients.Clear();
                        }
                        m_pendingTcpClients = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets the current state of the specified client which includes its System.Net.Sockets.Socket.
        /// </summary>
        /// <param name="clientID"></param>
        /// <value></value>
        /// <returns>
        /// The current state of the specified client which includes its System.Net.Sockets.Socket if the
        /// specified client ID is valid (client is connected); otherwise Nothing.
        /// </returns>
        public StateInfo Clients(Guid clientID)
        {
            StateInfo<Socket> client;

            lock (m_tcpClients)
            {
                m_tcpClients.TryGetValue(clientID, out client);
            }

            return client;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public override void Start()
        {
            if (Enabled && !IsRunning && ValidConfigurationString(ConfigurationString))
            {
#if ThreadTracking
                m_listenerThread = new ManagedThread(ListenForConnections);
                m_listenerThread.Name = "TVA.Communication.TcpServer.ListenForConnections()";
#else
                m_listenerThread = new Thread(ListenForConnections);
#endif

                // Start the thread on which the server will listen for incoming connections.
                m_listenerThread.Start();
            }
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public override void Stop()
        {
            if (Enabled && IsRunning)
            {
                // NOTE: Closing the server and all of the connected client sockets will cause a
                //       System.Net.Socket.SocketException in the thread using the socket. This in turn will result in
                //       the thread to exit gracefully because of the exception handling in place in the threads.

                // Stop accepting incoming connections.
                if (m_tcpServer != null)
                    m_tcpServer.Close();

                // Diconnect all of the connected clients.
                DisconnectAll();

                // Diconnect all of the pending clients connections.
                lock (m_pendingTcpClients)
                {
                    foreach (StateInfo<Socket> pendingTcpClient in m_pendingTcpClients)
                    {
                        if ((pendingTcpClient != null) && (pendingTcpClient.Client != null))
                            pendingTcpClient.Client.Close();
                    }
                }
            }
        }

        public override void DisconnectOne(Guid clientID)
        {
            StateInfo<Socket> tcpClient;

            lock (m_tcpClients)
            {
                m_tcpClients.TryGetValue(clientID, out tcpClient);
            }

            if (tcpClient != null)
            {
                tcpClient.Client.Close();
            }
            else
            {
                throw new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid.");
            }
        }

        public override void LoadSettings()
        {
            base.LoadSettings();

            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategoryName];

                if (settings.Count > 0)
                {
                    PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            if (PersistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategoryName];
                    CategorizedSettingsElement setting;

                    setting = settings["PayloadAware", true];
                    setting.Value = m_payloadAware.ToString();
                    setting.Description = "True if the message boundaries are to be preserved during transmission; otherwise False.";

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        /// <summary>
        /// Sends prepared data to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The prepared data that is to be sent to the client.</param>
        protected override void SendPreparedDataTo(Guid clientID, byte[] data)
        {
            if (Enabled && IsRunning)
            {
                StateInfo<Socket> tcpClient;

                lock (m_tcpClients)
                {
                    m_tcpClients.TryGetValue(clientID, out tcpClient);
                }

                if (tcpClient != null)
                {
                    // Encrypt the data with private key if SecureSession is enabled.
                    if (SecureSession)
                        data = CommunicationHelper.EncryptData(data, 0, data.Length, tcpClient.Passphrase, Encryption);

                    // Add payload header if client-server communication is PayloadAware.
                    if (m_payloadAware)
                        data = PayloadAwareHelper.AddPayloadHeader(data);

                    // PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
                    tcpClient.Client.Send(data);
                    tcpClient.LastSendTimestamp = DateTime.Now;

                    //' We'll send data over the wire asynchronously for improved performance.
                    //tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)
                }
                else
                {
                    throw new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid.");
                }
            }
        }

        /// <summary>
        /// Determines whether specified configuration string required for the server to initialize is valid.
        /// </summary>
        /// <param name="configurationString">The configuration string to be validated.</param>
        /// <returns>True if the configuration string is valid.</returns>
        protected override bool ValidConfigurationString(string configurationString)
        {
            if (!string.IsNullOrEmpty(configurationString))
            {
                m_configurationData = configurationString.ParseKeyValuePairs();

                if (m_configurationData.ContainsKey("port") && CommunicationHelper.ValidPortNumber(m_configurationData["port"]))
                {
                    // The configuration string must always contain the following:
                    // >> port - Port number on which the server will be listening for incoming connections.
                    return true;
                }
                else
                {
                    // Configuration string is not in the expected format.
                    StringBuilder exceptionMessage = new StringBuilder();

                    exceptionMessage.Append("Configuration string must be in the following format:");
                    exceptionMessage.AppendLine();
                    exceptionMessage.Append("   Port=Local port number");

                    throw new ArgumentException(exceptionMessage.ToString());
                }
            }
            else
            {
                throw new ArgumentNullException("ConfigurationString");
            }
        }

        /// <summary>
        /// Listens for incoming client connections.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
        private void ListenForConnections()
        {
            try
            {
                // Create a TCP socket and bind it a local endpoint at the specified port. Binding the socket will
                // establish a physical presence of the socket necessary for listening to incoming connections.
                m_tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_tcpServer.Bind(new IPEndPoint(IPAddress.Any, int.Parse(m_configurationData["port"])));

                // Start listening for connections and keep a maximum of 0 pending connection in the queue.
                m_tcpServer.Listen(0);

                OnServerStarted(EventArgs.Empty);

                while (true)
                {
                    if (MaximumClients == -1 || ClientIDs.Count < MaximumClients)
                    {
                        // We can accept incoming client connection requests.
                        StateInfo<Socket> tcpClient = new StateInfo<Socket>();
                        tcpClient.Client = m_tcpServer.Accept(); // Accept client connection.						
                        tcpClient.Client.LingerState = new LingerOption(true, 10);

                        // Start the client on a seperate thread so all the connected clients run independently.
                        //ThreadPool.QueueUserWorkItem(AddressOf ReceiveClientData, tcpClient)

#if ThreadTracking
                        ManagedThread receiveThread = new ManagedThread(ReceiveClientData);
                        receiveThread.Name = "TVA.Communication.TcpServer.ReceiveClientData() [" + ServerID.ToString() + "]";
#else
						Thread receiveThread = new Thread(ReceiveClientData);
#endif
                        receiveThread.Start(tcpClient);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // This will be a normal exception...
            }
            catch (ObjectDisposedException)
            {
                // This will be a normal exception...
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.Interrupted)
                {
                    // If we encounter a socket exception other than SocketError.Interrupt, we'll report it as an exception.
                    OnServerStartupException(ex);
                }
            }
            catch (Exception ex)
            {
                // We will gracefully exit when an exception occurs.
                OnServerStartupException(ex);
            }
            finally
            {
                if (m_tcpServer != null)
                {
                    m_tcpServer.Close();
                }
                OnServerStopped(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Receives any data sent by a client that is connected to the server.
        /// </summary>
        /// <param name="state">TVA.Communication.StateKeeper(Of Socket) of the the connected client.</param>
        /// <remarks>This method is meant to be executed on seperate threads.</remarks>
        private void ReceiveClientData(object state)
        {
            StateInfo<Socket> tcpClient = (StateInfo<Socket>)state;

            try
            {
                if (Handshake)
                {
                    // Handshaking is to be performed to authenticate the client, so we'll add the client to the
                    // list of client who have not been authenticated and give it 30 seconds to initiate handshaking.
                    tcpClient.Client.ReceiveTimeout = 30000;

                    lock (m_pendingTcpClients)
                    {
                        m_pendingTcpClients.Add(tcpClient);
                    }
                }
                else
                {
                    // No handshaking is to be performed for authenicating the client, so we'll add the client
                    // to the list of connected clients.
                    tcpClient.ID = Guid.NewGuid();

                    lock (m_tcpClients)
                    {
                        m_tcpClients.Add(tcpClient.ID, tcpClient);
                    }

                    OnClientConnected(tcpClient.ID);
                }

                // Used to count the number of bytes received in a single receive.
                int bytesReceived = 0;

                // Receiving of data from the client has been seperated into 2 different section resulting in some
                // redundant coding. This is necessary to achive a high performance TCP server component since it
                // may be used in real-time applications where performance is the key and evey millisecond  saved
                // makes a big difference.
                if ((m_receiveRawDataFunction != null) || (m_receiveRawDataFunction == null && !m_payloadAware))
                {
                    // In this section the consumer either wants to receive data and pass it on to a delegate or
                    // receive data that doesn't contain metadata used for preserving message boundaries. In either
                    // case we can use a static buffer that can be used over and over again for receiving data.
                    while (true)
                    {
                        // Receive data into the static buffer.
                        bytesReceived = tcpClient.Client.Receive(m_buffer, 0, m_buffer.Length, SocketFlags.None);
                        tcpClient.LastReceiveTimestamp = DateTime.Now;

                        // We start receiving zero-length data when a TCP connection is disconnected by the
                        // opposite party. In such case we must consider ourself disconnected from the client.
                        if (bytesReceived == 0)
                            throw new SocketException(10101);

                        // Post raw data to the delegate that is most likely used for real-time applications.
                        if (m_receiveRawDataFunction != null)
                            m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
                        else
                            ProcessReceivedClientData(m_buffer.CopyBuffer(0, bytesReceived), tcpClient);
                    }
                }
                else
                {
                    // In this section we will be receiving data that has metadata used for preserving message
                    // boundaries. Here a message (the payload) is sent by the other party along with some metadata
                    // (payload header) prepended to the message. The metadata (payload header) consists of a 4-byte
                    // marker used to mark the beginning of a message, followed by the message size (also 4-bytes),
                    // followed by the actual message.
                    int payloadSize = -1;
                    int totalBytesReceived = 0;

                    while (true)
                    {
                        // If we don't have the payload size, we'll begin by reading the payload header which
                        // contains the payload size. Once we have the payload size we can receive payload.
                        if (payloadSize == -1)
                            tcpClient.DataBuffer = new byte[PayloadAwareHelper.PayloadHeaderSize];

                        // Since TCP is a streaming protocol we can receive a part of the available data and
                        // the remaing data can be received in subsequent receives.
                        bytesReceived = tcpClient.Client.Receive(tcpClient.DataBuffer, totalBytesReceived, (tcpClient.DataBuffer.Length - totalBytesReceived), SocketFlags.None);
                        tcpClient.LastReceiveTimestamp = DateTime.Now;

                        if (bytesReceived == 0)
                            throw new SocketException(10101);

                        if (payloadSize == -1)
                        {
                            // We don't what the payload size is, so we'll check if the data we have contains
                            // the size of the payload we need to receive.
                            payloadSize = PayloadAwareHelper.GetPayloadSize(tcpClient.DataBuffer);

                            if (payloadSize != -1 && payloadSize <= CommunicationClientBase.MaximumDataSize)
                            {
                                // We have a valid payload size, so we'll create a buffer that's big enough
                                // to hold the entire payload. Remember, the payload at the most can be as big
                                // as whatever the MaximumDataSize is.
                                tcpClient.DataBuffer = new byte[payloadSize];
                            }
                        }
                        else
                        {
                            totalBytesReceived += bytesReceived;

                            if (totalBytesReceived == payloadSize)
                            {
                                // We've received the entire payload.
                                ProcessReceivedClientData(tcpClient.DataBuffer, tcpClient);

                                // Initialize for receiving the next payload.
                                payloadSize = -1;
                                totalBytesReceived = 0;
                            }
                        }
                    }
                }
            }
            catch
            {
                // We don't need to take any action when an exception is encountered.
            }
            finally
            {
                // We are now done with the client.
                if ((tcpClient != null) && (tcpClient.Client != null))
                {
                    tcpClient.Client.Close();
                }

                lock (m_pendingTcpClients)
                {
                    m_pendingTcpClients.Remove(tcpClient);
                }

                bool clientDisconnected = false;

                lock (m_tcpClients)
                {
                    clientDisconnected = m_tcpClients.ContainsKey(tcpClient.ID);
                    m_tcpClients.Remove(tcpClient.ID);
                }

                if (clientDisconnected)
                    OnClientDisconnected(tcpClient.ID);
            }
        }

        /// <summary>
        /// This method processes the data received from the client.
        /// </summary>
        /// <param name="data">The data received from the client.</param>
        /// <param name="tcpClient">The TCP client who sent the data.</param>
        private void ProcessReceivedClientData(byte[] data, StateInfo<Socket> tcpClient)
        {
            if (tcpClient.ID == Guid.Empty && Handshake)
            {
                // Authentication is required, but not performed yet. When authentication is required
                // the first message from the client must be information about itself.
                HandshakeMessage clientInfo = TVA.Serialization.GetObject<HandshakeMessage>(GetActualData(data));

                if ((clientInfo != null) && clientInfo.ID != Guid.Empty && clientInfo.Passphrase == HandshakePassphrase)
                {
                    // We'll generate a private key for the client if SecureSession is enabled.
                    tcpClient.ID = clientInfo.ID;
                    if (SecureSession) tcpClient.Passphrase = Cipher.GenerateKey();

                    // We'll send our information to the client which may contain a private crypto key .
                    byte[] myInfo = GetPreparedData(TVA.Serialization.GetBytes(new HandshakeMessage(ServerID, tcpClient.Passphrase)));

                    if (m_payloadAware)
                        myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo);

                    tcpClient.Client.Send(myInfo);
                    tcpClient.ID = clientInfo.ID;
                    tcpClient.Client.ReceiveTimeout = 0; // We don't want to timeout while waiting for data from client.

                    // The client's authentication is now complete.
                    lock (m_pendingTcpClients)
                    {
                        m_pendingTcpClients.Remove(tcpClient);
                    }

                    lock (m_tcpClients)
                    {
                        m_tcpClients.Add(tcpClient.ID, tcpClient);
                    }

                    OnClientConnected(tcpClient.ID);
                }
                else
                {
                    // The first response from the client is either not information about itself, or
                    // the information provided by the client is invalid.
                    throw new ApplicationException("Failed to authenticate the client.");
                }
            }
            else
            {
                // Decrypt the data usign private key if SecureSession is enabled.
                if (SecureSession)
                    data = CommunicationHelper.DecryptData(data, tcpClient.Passphrase, Encryption);

                // We'll pass the received data along to the consumer via event.
                OnReceivedClientData(new IdentifiableItem<Guid, byte[]>(tcpClient.ID, data));
            }
        }

        #endregion
    }
}
