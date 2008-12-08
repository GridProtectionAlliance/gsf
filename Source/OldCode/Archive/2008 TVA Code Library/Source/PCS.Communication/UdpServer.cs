////*******************************************************************************************************
////  UdpServer.cs
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
////  09/29/2008 - James R Carroll
////       Converted to C#.
////
////*******************************************************************************************************
//using System;
//using System.Text;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.ComponentModel;
//using System.Collections.Generic;
//using PCS.Threading;
//using PCS.Configuration;
//using PCS.Security.Cryptography;

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
//    public class UdpServer : ServerBase
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
//    }
//}
