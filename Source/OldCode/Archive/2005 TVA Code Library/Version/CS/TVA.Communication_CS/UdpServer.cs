using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.ComponentModel;
//using TVA.Serialization;
using TVA.Communication.CommunicationHelper;
//using TVA.Security.Cryptography.Common;
using TVA.Threading;

//*******************************************************************************************************
//  TVA.Communication.UdpServer.vb - UDP-based communication server
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access
//
//*******************************************************************************************************


/// <summary>
/// Represents a UDP-based communication server.
/// </summary>
/// <remarks>
/// UDP by nature is a connectionless protocol, but with this implementation of UDP server we can have a
/// connectionfull session with the server by enabling Handshake. This in-turn enables us to take advantage
/// of SecureSession which otherwise is not possible.
/// </remarks>
namespace TVA.Communication
{
	public partial class UdpServer
	{
		
		
		#region " Member Declaration "
		
		private bool m_payloadAware;
		private bool m_destinationReachableCheck;
		private StateInfo<Socket> m_udpServer;
		private Dictionary<Guid, StateInfo<IPEndPoint>> m_udpClients;
		private Dictionary<string, string> m_configurationData;
		
		#endregion
		
		#region " Code Scope: Public "
		
		/// <summary>
		/// The minimum size of the receive buffer for UDP.
		/// </summary>
		public const int MinimumUdpBufferSize = 512;
		
		/// <summary>
		/// The maximum number of bytes that can be sent in a single UDP datagram.
		/// </summary>
		public const int MaximumUdpDatagramSize = 32768;
		
		/// <summary>
		/// Initializes a instance of TVA.Communication.UdpServer with the specified data.
		/// </summary>
		/// <param name="configurationString">The configuration string containing the data required for initializing the UDP server.</param>
		public UdpServer(string configurationString) : this()
		{
			
			base.ConfigurationString = configurationString;
			
		}
		
		/// <summary>
		/// Gets or sets a boolean value indicating whether the messages that are broken down into multiple datagram
		/// for the purpose of transmission while being sent are to be assembled back when received.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// True if the messages that are broken down into multiple datagram for the purpose of transmission while being
		/// sent are to be assembled back when received; otherwise False.
		/// </returns>
		/// <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
		[Description("Indicates whether the messages that are broken down into multiple datagram for the purpose of transmission are to be assembled back when received. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(typeof(bool), "False")]public bool PayloadAware
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
		/// Gets or sets a boolean value indicating whether a test is to be performed to check if the destination
		/// endpoint that is to receive data is listening for data.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// True if a test is to be performed to check if the destination endpoint that is to receive data is listening
		/// for data; otherwise False.
		/// </returns>
		[Description("Indicates whether a test is to be performed to check if the destination endpoint that is to receive data is listening for data."), Category("Behavior"), DefaultValue(typeof(bool), "False")]public bool DestinationReachableCheck
		{
			get
			{
				return m_destinationReachableCheck;
			}
			set
			{
				m_destinationReachableCheck = value;
			}
		}
		
		/// <summary>
		/// Gets the System.Net.IPEndPoint of the server.
		/// </summary>
		/// <returns>The System.Net.IPEndPoint of the server.</returns>
		[Browsable(false)]public IPEndPoint Server
		{
			get
			{
				return ((IPEndPoint) m_udpServer.Client.LocalEndPoint);
			}
		}
		
		/// <summary>
		/// Gets the current states of all connected clients which includes the System.Net.IPEndPoint of clients.
		/// </summary>
		/// <remarks>
		/// The current states of all connected clients which includes the System.Net.IPEndPoint of clients.
		/// </remarks>
		[Browsable(false)]public List<StateInfo<IPEndPoint>> Clients
		{
			get
			{
				List<StateInfo<IPEndPoint>> clientList = new List<StateInfo<IPEndPoint>>();
				lock(m_udpClients)
				{
					clientList.AddRange(m_udpClients.Values);
				}
				
				return clientList;
			}
		}
		
		/// <summary>
		/// Gets the current state of the specified client which includes its System.Net.IPEndPoint.
		/// </summary>
		/// <param name="clientID"></param>
		/// <value></value>
		/// <returns>
		/// The current state of the specified client which includes its System.Net.IPEndPoint if the
		/// specified client ID is valid (client is connected); otherwise Nothing.
		/// </returns>
		public StateInfo Clients(Guid clientID)
		{
			StateInfo<IPEndPoint> client = null;
			lock(m_udpClients)
			{
				m_udpClients.TryGetValue(clientID, client);
			}
			
			return client;
		}
		
		#region " Overrides "
		
		/// <summary>
		/// Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
		/// </summary>
		/// <value>Receive buffer size</value>
		/// <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
		/// <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
		
		public override int ReceiveBufferSize
		{
			get
			{
				return base.ReceiveBufferSize;
			}
			set
			{
				if (value >= UdpClient.MinimumUdpBufferSize && value <= UdpClient.MaximumUdpDatagramSize)
				{
					base.ReceiveBufferSize = value;
				}
				else
				{
					throw (new ArgumentOutOfRangeException("ReceiveBufferSize", "ReceiveBufferSize for UDP must be between " + UdpClient.MinimumUdpBufferSize + " and " + UdpClient.MaximumUdpDatagramSize + "."));
				}
			}
		}
		
		public override void Start()
		{
			
			if (Enabled && ! IsRunning && ValidConfigurationString(ConfigurationString))
			{
				try
				{
					int serverPort = 0;
					if (m_configurationData.ContainsKey("port"))
					{
						serverPort = Convert.ToInt32(m_configurationData("port"));
					}
					
					m_udpServer = new StateInfo<Socket>();
					m_udpServer.ID = ServerID;
					m_udpServer.Passphrase = HandshakePassphrase;
					m_udpServer.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					if (Handshake)
					{
						// We will listen for data only when Handshake is enabled and a valid port has been specified in
						// the configuration string. We do this in order to keep the server stable and besides that, the
						// main purpose of a UDP server is to serve data in most cases.
						if (serverPort > 0)
						{
							m_udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, serverPort));
							
							#if ThreadTracking
							TVA.Threading.ManagedThread with_1 = new ManagedThread(ReceiveClientData);
							with_1.Name = "TVA.Communication.UdpServer.ReceiveClientData() [" + ServerID.ToString() + "]";
							#else
							System.Threading.Thread with_2 = new Thread(new System.Threading.ThreadStart(ReceiveClientData));
							#endif
							.Start();
						}
						else
						{
							throw (new ArgumentException("Server port must be specified in the configuration string."));
						}
					}
					
					OnServerStarted(EventArgs.Empty);
					
					if (! Handshake && m_configurationData.ContainsKey("clients"))
					{
						// We will ignore the client list in configuration string when Handshake is enabled.
						foreach (string clientString in m_configurationData("clients").Replace(" ", "").Split(','))
						{
							try
							{
								int clientPort = 0;
								string[] clientStringSegments = clientString.Split(':');
								if (clientStringSegments.Length == 2)
								{
									clientPort = Convert.ToInt32(clientStringSegments[1]);
								}
								
								StateInfo<IPEndPoint> udpClient = new StateInfo<IPEndPoint>();
								udpClient.ID = Guid.NewGuid();
								udpClient.Client = GetIpEndPoint(clientStringSegments[0], clientPort);
								lock(m_udpClients)
								{
									m_udpClients.Add(udpClient.ID, udpClient);
								}
								
								OnClientConnected(udpClient.ID);
							}
							catch (Exception)
							{
								// Ignore invalid client entries.
							}
						}
					}
				}
				catch
				{
					OnServerStartupException(ex);
				}
			}
			
		}
		
		public override void @Stop()
		{
			
			if (Enabled && IsRunning)
			{
				// Disconnect all of the clients.
				DisconnectAll();
				
				// Stop the server after we've disconnected the clients.
				if ((m_udpServer != null)&& (m_udpServer.Client != null))
				{
					m_udpServer.Client.Close();
				}
				
				OnServerStopped(EventArgs.Empty);
			}
			
		}
		
		public override void DisconnectOne(System.Guid clientID)
		{
			
			StateInfo<IPEndPoint> udpClient = null;
			lock(m_udpClients)
			{
				m_udpClients.TryGetValue(clientID, udpClient);
			}
			
			if (udpClient != null)
			{
				if (Handshake)
				{
					// Handshake is enabled so we'll notify the client.
					byte[] goodbye = GetPreparedData(TVA.Serialization.GetBytes(new GoodbyeMessage(udpClient.ID)));
					if (m_payloadAware)
					{
						goodbye = PayloadAwareHelper.AddPayloadHeader(goodbye);
					}
					
					m_udpServer.Client.SendTo(goodbye, udpClient.Client);
				}
				
				lock(m_udpClients)
				{
					m_udpClients.Remove(udpClient.ID);
				}
				OnClientDisconnected(udpClient.ID);
			}
			else
			{
				throw (new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid."));
			}
			
		}
		
		public override void LoadSettings()
		{
			
			base.LoadSettings();
			
			try
			{
				TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName);
				if (with_1.Count > 0)
				{
					PayloadAware = with_1.Item("PayloadAware").GetTypedValue(m_payloadAware);
					DestinationReachableCheck = with_1.Item("DestinationReachableCheck").GetTypedValue(m_destinationReachableCheck);
				}
			}
			catch (Exception)
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
					TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName);
					object with_2 = with_1.Item("PayloadAware", true);
					with_2.Value = m_payloadAware.ToString();
					with_2.Description = "True if the messages that are broken down into multiple datagram for the purpose of transmission while being sent are to be assembled back when received; otherwise False.";
					object with_3 = with_1.Item("DestinationReachableCheck", true);
					with_3.Value = m_destinationReachableCheck.ToString();
					with_3.Description = "True if a test is to be performed to check if the destination endpoint that is to receive data is listening for data; otherwise False.";
					TVA.Configuration.Common.SaveSettings();
				}
				catch (Exception)
				{
					// We might encounter an exception if for some reason the settings cannot be saved to the config file.
				}
			}
			
		}
		
		#endregion
		
		#endregion
		
		#region " Code Scope: Protected "
		
		#region " Overrides "
		
		protected override void SendPreparedDataTo(System.Guid clientID, byte[] data)
		{
			
			if (Enabled && IsRunning)
			{
				StateInfo<IPEndPoint> udpClient = null;
				lock(m_udpClients)
				{
					m_udpClients.TryGetValue(clientID, udpClient);
				}
				
				if (udpClient != null)
				{
					if (m_destinationReachableCheck && ! IsDestinationReachable(udpClient.Client))
					{
						return;
					}
					
					if (SecureSession)
					{
						data = EncryptData(data, udpClient.Passphrase, Encryption);
					}
					
					if (m_payloadAware)
					{
						data = PayloadAwareHelper.AddPayloadHeader(data);
					}
					
					int toIndex = 0;
					int datagramSize = ReceiveBufferSize;
					if (data.Length > datagramSize)
					{
						toIndex = data.Length - 1;
					}
					for (int i = 0; i <= toIndex; i += datagramSize)
					{
						// Last or the only datagram in the series.
						if (data.Length - i < datagramSize)
						{
							datagramSize = data.Length - i;
						}
						
						// PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
						m_udpServer.Client.SendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client);
						udpClient.LastSendTimestamp = DateTime.Now;
						//' We'll send the data asynchronously for better performance.
						//m_udpServer.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client, Nothing, Nothing)
					}
				}
				else
				{
					throw (new ArgumentException("Client ID \'" + clientID.ToString() + "\' is invalid."));
				}
			}
			
		}
		
		protected override bool ValidConfigurationString(string configurationString)
		{
			
			if (! string.IsNullOrEmpty(configurationString))
			{
				m_configurationData = TVA.Text.Common.ParseKeyValuePairs(configurationString);
				if ((m_configurationData.ContainsKey("port") && ValidPortNumber(m_configurationData("port"))) || (m_configurationData.ContainsKey("clients") && ! string.IsNullOrEmpty(m_configurationData("clients"))))
				{
					// The configuration string must contain either of the following:
					// >> port - Port number on which the server will be listening for incoming data.
					// OR
					// >> clients - A list of clients the server will be sending data to.
					return true;
				}
				else
				{
					// Configuration string is not in the expected format.
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("Configuration string must be in the following format:");
					with_1.AppendLine();
					with_1.Append("   [Port=Local port number;] [Clients=Client name or IP[:Port number], ..., Client name or IP[:Port number]]");
					with_1.AppendLine();
					with_1.Append("Text between square brackets, [...], is optional.");
					throw (new ArgumentException(with_1.ToString()));
				}
			}
			else
			{
				throw (new ArgumentNullException("ConfigurationString"));
			}
			
		}
		
		#endregion
		
		#endregion
		
		#region " Code Scope: Private "
		
		private void ReceiveClientData()
		{
			
			try
			{
				int bytesReceived = 0;
				EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
				if ((m_receiveRawDataFunction != null)|| (m_receiveRawDataFunction == null && ! m_payloadAware))
				{
					// In this section the consumer either wants to receive the datagrams and pass it on to a
					// delegate or receive datagrams that don't contain metadata used for re-assembling the
					// datagrams into the original message and be notified via events. In either case we can use
					// a static buffer that can be used over and over again for receiving datagrams as long as
					// the datagrams received are not bigger than the receive buffer.
					while (true)
					{
						bytesReceived = m_udpServer.Client.ReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, clientEndPoint);
						m_udpServer.LastReceiveTimestamp = DateTime.Now;
						
						if (m_receiveRawDataFunction != null)
						{
							m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
						}
						else
						{
							ProcessReceivedClientData(TVA.IO.Common.CopyBuffer(m_buffer, 0, bytesReceived), clientEndPoint);
						}
					}
				}
				else
				{
					int payloadSize = - 1;
					int totalBytesReceived = 0;
					while (true)
					{
						if (payloadSize == - 1)
						{
							m_udpServer.DataBuffer = TVA.Common.CreateArray<byte>(ReceiveBufferSize);
						}
						
						bytesReceived = m_udpServer.Client.ReceiveFrom(m_udpServer.DataBuffer, totalBytesReceived, (m_udpServer.DataBuffer.Length - totalBytesReceived), SocketFlags.None, clientEndPoint);
						m_udpServer.LastReceiveTimestamp = DateTime.Now;
						
						if (payloadSize == - 1)
						{
							payloadSize = PayloadAwareHelper.GetPayloadSize(m_udpServer.DataBuffer);
							if (payloadSize != - 1 && payloadSize <= CommunicationClientBase.MaximumDataSize)
							{
								byte[] payload = PayloadAwareHelper.GetPayload(m_udpServer.DataBuffer);
								
								m_udpServer.DataBuffer = TVA.Common.CreateArray<byte>(payloadSize);
								Buffer.BlockCopy(payload, 0, m_udpServer.DataBuffer, 0, payload.Length);
								bytesReceived = payload.Length;
							}
						}
						
						totalBytesReceived += bytesReceived;
						if (totalBytesReceived == payloadSize)
						{
							// We've received the entire payload.
							ProcessReceivedClientData(m_udpServer.DataBuffer, clientEndPoint);
							
							// Initialize for receiving the next payload.
							payloadSize = - 1;
							totalBytesReceived = 0;
						}
					}
				}
			}
			catch (Exception)
			{
				// We don't need to take any action when an exception is encountered.
			}
			finally
			{
				if ((m_udpServer != null)&& (m_udpServer.Client != null))
				{
					m_udpServer.Client.Close();
				}
			}
			
		}
		
		private void ProcessReceivedClientData(byte[] data, EndPoint senderEndPoint)
		{
			
			if (data.Length == 0)
			{
				return;
			}
			
			object clientMessage = TVA.Serialization.GetObject(GetActualData(data));
			if (clientMessage != null)
			{
				// We were able to deserialize the data to an object.
				if (clientMessage is HandshakeMessage)
				{
					HandshakeMessage connectedClient = (HandshakeMessage) clientMessage;
					if (connectedClient.ID != Guid.Empty && connectedClient.Passphrase == HandshakePassphrase)
					{
						StateInfo<IPEndPoint> udpClient = new StateInfo<IPEndPoint>();
						udpClient.ID = connectedClient.ID;
						udpClient.Client = (IPEndPoint) senderEndPoint;
						if (SecureSession)
						{
							udpClient.Passphrase = TVA.Security.Cryptography.Common.GenerateKey();
						}
						
						byte[] myInfo = GetPreparedData(TVA.Serialization.GetBytes(new HandshakeMessage(ServerID, udpClient.Passphrase)));
						if (m_payloadAware)
						{
							myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo);
						}
						m_udpServer.Client.SendTo(myInfo, udpClient.Client);
						
						lock(m_udpClients)
						{
							m_udpClients.Add(udpClient.ID, udpClient);
						}
						
						OnClientConnected(udpClient.ID);
					}
				}
				else if (clientMessage is GoodbyeMessage)
				{
					GoodbyeMessage disconnectedClient = (GoodbyeMessage) clientMessage;
					
					lock(m_udpClients)
					{
						m_udpClients.Remove(disconnectedClient.ID);
					}
					
					OnClientDisconnected(disconnectedClient.ID);
				}
			}
			else
			{
				StateInfo<IPEndPoint> sender = null;
				IPEndPoint senderIPEndPoint = (IPEndPoint) senderEndPoint;
				if (! senderIPEndPoint.Equals(GetIpEndPoint(Dns.GetHostName(), Convert.ToInt32(m_configurationData("port")))))
				{
					// The data received is not something that we might have broadcasted.
					lock(m_udpClients)
					{
						// So, now we'll find the ID of the client who sent the data.
						foreach (StateInfo<IPEndPoint> udpClient in m_udpClients.Values)
						{
							if (senderIPEndPoint.Equals(udpClient.Client))
							{
								sender = udpClient;
								break;
							}
						}
					}
					
					if (sender != null)
					{
						if (SecureSession)
						{
							data = DecryptData(data, sender.Passphrase, Encryption);
						}
						OnReceivedClientData(new IdentifiableItem<Guid, byte>(sender.ID, data));
					}
					else
					{
						OnReceivedClientData(new IdentifiableItem<Guid, byte>(Guid.Empty, data));
					}
				}
			}
			
		}
		
		#endregion
		
	}
	
}
