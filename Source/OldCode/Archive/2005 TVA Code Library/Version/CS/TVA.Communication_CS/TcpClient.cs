//*******************************************************************************************************
//  TcpClient.cs
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
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload
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
using TVA.Configuration;
using TVA.Threading;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a TCP-based communication client.
    /// </summary>
    /// <remarks>
    /// PayloadAware enabled transmission can transmit up to 100MB of payload in a single transmission.
    /// </remarks>
    public class TcpClient : ClientBase
    {
        #region [ Members ]

        // Fields
        private bool m_payloadAware;
        private StateInfo<Socket> m_tcpClient;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
		private Thread m_connectionThread;
#endif
        private Dictionary<string, string> m_connectionData;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public TcpClient()
        {
            m_payloadAware = false;
            base.ConnectionString = "Server=localhost; Port=8888";
            base.Protocol = TransportProtocol.Tcp;
        }

        /// <summary>
        /// Initializes a instance of TVA.Communication.TcpClient with the specified data.
        /// </summary>
        /// <param name="connectionString">The connection string containing the data required for connecting to a TCP server.</param>
        public TcpClient(string connectionString)
            : this()
        {
            base.ConnectionString = connectionString;
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
                        if (m_connectionThread != null)
                        {
                            if (m_connectionThread.IsAlive)
                                m_connectionThread.Abort();
                        }
                        m_connectionThread = null;

                        if (m_tcpClient != null)
                        {
                            if (m_tcpClient.Client != null && m_tcpClient.Client.Connected)
                            {
                                m_tcpClient.Client.Shutdown(SocketShutdown.Both);
                                m_tcpClient.Client.Close();
                            }
                            m_tcpClient.Client = null;
                        }
                        m_tcpClient = null;
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
        /// Cancels any active attempts of connecting to the server.
        /// </summary>
        public override void CancelConnect()
        {
            // The client attempts to connect to the server on a seperate thread and since that thread is still
            // running, we know that the client has not yet connected to the server. We can now abort the thread
            // to stop the client from attempting to connect to the server.
            if (base.Enabled && m_connectionThread.IsAlive)
                m_connectionThread.Abort();
        }

        /// <summary>
        /// Connects to the server asynchronously.
        /// </summary>
        public override void Connect()
        {
            if (base.Enabled && !base.IsConnected && ValidConnectionString(base.ConnectionString))
            {
#if ThreadTracking
                m_connectionThread = new TVA.Threading.ManagedThread(ConnectToServer);
                m_connectionThread.Name = "TVA.Communication.TcpClient.ConnectToServer()";
#else
                m_connectionThread = new Thread(ConnectToServer);
#endif
                m_connectionThread.Start();
            }
        }

        /// <summary>
        /// Disconnects client from the connected server.
        /// </summary>
        public override void Disconnect(int timeout)
        {
            CancelConnect(); // Cancel any active connection attempts.

            if (base.Enabled && base.IsConnected && (m_tcpClient != null) && (m_tcpClient.Client != null))
            {
                // Close the client socket that is connected to the server.
                m_tcpClient.Client.Shutdown(SocketShutdown.Both);

                // JRC: Allowing call with disconnect timeout...
                try
                {
                    if (timeout <= 0)
                        m_tcpClient.Client.Close();
                    else
                        m_tcpClient.Client.Close(timeout);
                }
                catch
                {
                    // This very rarely throws an exception - so we just ignore it...
                }
            }
        }

        public override void LoadSettings()
        {
            base.LoadSettings();

            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];

                if (settings.Count > 0)
                    PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
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
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[SettingsCategory];
                    CategorizedSettingsElement setting;

                    setting = settings["PayloadAware", true];
                    setting.Value = m_payloadAware.ToString();
                    setting.Description = "True if the message boundaries are to be preserved during transmission; otherwise False.";

                    ConfigurationFile.Current.Save();
                }
                catch (Exception)
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        /// <summary>
        /// Sends prepared data to the server.
        /// </summary>
        /// <param name="data">The prepared data that is to be sent to the server.</param>
        protected override void SendPreparedData(byte[] data)
        {
            if (base.Enabled && base.IsConnected)
            {
                // Encrypt the data with private key if SecureSession is enabled.
                if (base.SecureSession)
                    data = Transport.EncryptData(data, 0, data.Length, m_tcpClient.Passphrase, base.Encryption);

                // Add payload header if client-server communication is PayloadAware.
                if (m_payloadAware)
                    data = Payload.AddHeader(data);

                OnSendDataBegin(new IdentifiableItem<Guid, byte[]>(ClientID, data));

                // PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
                m_tcpClient.Client.Send(data);

                // TODO: make asynchronous send an option, but off by default, for improved performance.
                //m_tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)

                OnSendDataComplete(new IdentifiableItem<Guid, byte[]>(ClientID, data));
            }
        }

        /// <summary>
        /// Determines whether specified connection string required for connecting to the server is valid.
        /// </summary>
        /// <param name="connectionString">The connection string to be validated.</param>
        /// <returns>True is the connection string is valid; otherwise False.</returns>
        protected override bool ValidConnectionString(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                m_connectionData = connectionString.ParseKeyValuePairs();
                if (m_connectionData.ContainsKey("server") &&
                    !string.IsNullOrEmpty(m_connectionData["server"]) &&
                    m_connectionData.ContainsKey("port") &&
                    Transport.IsValidPortNumber(m_connectionData["port"]))
                {
                    // The connection string must always contain the following:
                    // >> server - Name or IP of the machine machine on which the server is running.
                    // >> port - Port number on which the server is listening for connections.
                    return true;
                }
                else
                {
                    // Connection string is not in the expected format.
                    StringBuilder exceptionMessage = new StringBuilder();

                    exceptionMessage.Append("Connection string must be in the following format:");
                    exceptionMessage.AppendLine();
                    exceptionMessage.Append("   Server=Server name or IP; Port=Server port number");

                    throw new ArgumentException(exceptionMessage.ToString());
                }
            }
            else
            {
                throw new ArgumentNullException("ConnectionString");
            }
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
        private void ConnectToServer()
        {
            int connectionAttempts = 0;

            while (base.MaximumConnectionAttempts == -1 || connectionAttempts < base.MaximumConnectionAttempts)
            {
                try
                {
                    OnConnecting(EventArgs.Empty);

                    // Create a TCP socket and bind it to a local endpoint. Binding the socket will establish a
                    // physical presence of the socket necessary for receving data from the server
                    m_tcpClient = new StateInfo<Socket>();
                    m_tcpClient.ID = base.ClientID;
                    m_tcpClient.Passphrase = base.HandshakePassphrase;
                    m_tcpClient.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_tcpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                    m_tcpClient.Client.LingerState = new LingerOption(true, 10);

                    // Imposed a timeout on receiving data if specified.
                    if (base.ReceiveTimeout != -1)
                        m_tcpClient.Client.ReceiveTimeout = base.ReceiveTimeout;

                    // Attempt to connect the client socket to the remote server endpoint.
                    m_tcpClient.Client.Connect(Transport.GetIpEndPoint(m_connectionData["server"], int.Parse(m_connectionData["port"])));

                    if (m_tcpClient.Client.Connected) // Client connected to the server successfully.
                    {
                        // Start a seperate thread for the client to receive data from the server.
#if ThreadTracking
                        ManagedThread receiveThread = new ManagedThread(ReceiveServerData);
                        receiveThread.Name = "TVA.Communication.TcpClient.ReceiveServerData() [" + m_tcpClient.ID.ToString() + "]";
#else
						Thread receiveThread = new Thread(ReceiveServerData);
#endif
                        receiveThread.Start();

                        break; // Client successfully connected to the server.
                    }
                }
                catch (ThreadAbortException)
                {
                    // We'll stop trying to connect if a System.Threading.ThreadAbortException exception is encountered.
                    // This will be the case when the thread is deliberately aborted in CancelConnect() method in which
                    // case we want to stop attempting to connect to the server.
                    OnConnectingCancelled(EventArgs.Empty);
                    break;
                }
                catch (Exception ex)
                {
                    connectionAttempts++;
                    OnConnectingException(ex);
                }
            }
        }

        /// <summary>
        /// Receives data sent by the server.
        /// </summary>
        /// <remarks>This method is meant to be executed on a seperate thread.</remarks>
        private void ReceiveServerData()
        {
            try
            {
                if (base.Handshake)
                {
                    // Handshaking is to be performed so we'll send our information to the server.
                    byte[] myInfo = GetPreparedData(Serialization.GetBytes(new HandshakeMessage(m_tcpClient.ID, m_tcpClient.Passphrase)));

                    // Add payload header if client-server communication is PayloadAware.
                    if (m_payloadAware)
                        myInfo = Payload.AddHeader(myInfo);

                    m_tcpClient.Client.Send(myInfo);
                }
                else
                {
                    OnConnected(EventArgs.Empty);
                }

                // Used to count the number of bytes received in a single receive.
                int bytesReceived = 0;

                // Receiving of data from the server has been seperated into 2 different section resulting in some
                // redundant coding. This is necessary to achive a high performance TCP client component since it
                // may be used in real-time applications where performance is the key and evey millisecond saved
                // makes a big difference.
                if ((m_receiveRawDataFunction != null) || (m_receiveRawDataFunction == null && !m_payloadAware))
                {
                    // In this section the consumer either wants to receive data and pass it on to a delegate or
                    // receive data that doesn't contain metadata used for preserving message boundaries. In either
                    // case we can use a static buffer that can be used over and over again for receiving data.
                    while (true)
                    {
                        try
                        {
                            // Receive data into the static buffer.
                            bytesReceived = m_tcpClient.Client.Receive(m_buffer, 0, m_buffer.Length, SocketFlags.None);

                            // We start receiving zero-length data when a TCP connection is disconnected by the
                            // opposite party. In such case we must consider ourself disconnected from the server.
                            if (bytesReceived == 0)
                                throw new SocketException(10101);

                            if (m_receiveRawDataFunction != null)
                            {
                                // Post raw data to the delegate that is most likely used for real-time applications.
                                m_receiveRawDataFunction(m_buffer, 0, bytesReceived);
                                m_totalBytesReceived += bytesReceived;
                            }
                            else
                            {
                                ProcessReceivedServerData(m_buffer.CopyBuffer(0, bytesReceived));
                            }
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.TimedOut)
                                HandleReceiveTimeout();
                            else
                                throw;
                        }
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
                            m_tcpClient.DataBuffer = new byte[Payload.HeaderSize];

                        try
                        {
                            // Since TCP is a streaming protocol we can receive a part of the available data and
                            // the remaing data can be received in subsequent receives.
                            bytesReceived = m_tcpClient.Client.Receive(m_tcpClient.DataBuffer, totalBytesReceived, (m_tcpClient.DataBuffer.Length - totalBytesReceived), SocketFlags.None);

                            if (bytesReceived == 0)
                                throw new SocketException(10101);

                            if (payloadSize == -1)
                            {
                                // We don't what the payload size is, so we'll check if the data we have contains
                                // the size of the payload we need to receive.
                                payloadSize = Payload.GetSize(m_tcpClient.DataBuffer);

                                if (payloadSize != -1 && payloadSize <= ClientBase.MaximumDataSize)
                                {
                                    // We have a valid payload size, so we'll create a buffer that's big enough
                                    // to hold the entire payload. Remember, the payload at the most can be as big
                                    // as whatever the MaximumDataSize is.
                                    m_tcpClient.DataBuffer = new byte[payloadSize];
                                }
                            }
                            else
                            {
                                totalBytesReceived += bytesReceived;

                                if (totalBytesReceived == payloadSize)
                                {
                                    // We've received the entire payload.
                                    ProcessReceivedServerData(m_tcpClient.DataBuffer);

                                    // Initialize for receiving the next payload.
                                    payloadSize = -1;
                                    totalBytesReceived = 0;
                                }
                            }
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.TimedOut)
                                HandleReceiveTimeout();
                            else
                                throw;
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
                if ((m_tcpClient != null) && (m_tcpClient.Client != null))
                    m_tcpClient.Client.Close();

                OnDisconnected(EventArgs.Empty);
            }

        }

        /// <summary>
        /// This method will not be required once the bug in .Net Framwork is fixed.
        /// </summary>
        private void HandleReceiveTimeout()
        {
            OnReceiveTimedOut(EventArgs.Empty); // Notify that a timeout has been encountered.

            // TODO: Determine if this is still a necessary bug fix in .NET 3.5...
            // NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
            // Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
            m_tcpClient.Client.Blocking = true; // <= Temporary bug fix!
        }

        /// <summary>
        /// This method processes the data received from the server.
        /// </summary>
        /// <param name="data">The data received from the server.</param>
        private void ProcessReceivedServerData(byte[] data)
        {
            if (base.ServerID == Guid.Empty && base.Handshake)
            {
                // Handshaking is to be performed, but it's not complete yet.
                HandshakeMessage serverInfo = Serialization.GetObject<HandshakeMessage>(GetActualData(data));

                if ((serverInfo != null) && serverInfo.ID != Guid.Empty)
                {
                    // Authentication was successful and the server responded with its information.
                    base.ServerID = serverInfo.ID;
                    m_tcpClient.Passphrase = serverInfo.Passphrase;
                }
                else
                {
                    // Authetication was unsuccessful, so we must now disconnect.
                    throw (new ApplicationException("Authentication with the server failed."));
                }

                OnConnected(EventArgs.Empty);
            }
            else
            {
                // Decrypt the data usign private key if SecureSession is enabled.
                if (base.SecureSession)
                {
                    data = Transport.DecryptData(data, m_tcpClient.Passphrase, base.Encryption);
                }

                // We'll pass the received data along to the consumer via event.
                OnReceivedData(new IdentifiableItem<Guid, byte[]>(ServerID, data));
            }
        }


        #endregion
    }
}
