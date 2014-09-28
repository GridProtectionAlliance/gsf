//******************************************************************************************************
//  TcpClient.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/02/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed socket access.
//  12/01/2006 - Pinal C. Patel
//       Modified code for handling "PayloadAware" transmissions.
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  07/15/2009 - Pinal C. Patel
//       Modified Disconnect() to add error checking.
//  07/17/2009 - Pinal C. Patel
//       Added support to specify a specific interface address on a multiple interface machine.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  03/24/2010 - Pinal C. Patel
//       Updated the interpretation of server property in ConnectionString to correctly interpret 
//       IPv6 IP addresses according to IETF - A Recommendation for IPv6 Address Text Representation.
//  11/29/2010 - Pinal C. Patel
//       Corrected the implementation of ConnectAsync() method.
//  02/11/2011 - Pinal C. Patel
//       Added IntegratedSecurity property to enable integrated windows authentication.
//  02/13/2011 - Pinal C. Patel
//       Modified ConnectAsync() to handle loopback address resolution failure on IPv6 enabled OSes.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  07/23/2012 - Stephen C. Wills
//       Performed a full refactor to use the SocketAsyncEventArgs API calls.
//  10/31/2012 - Stephen C. Wills
//       Replaced single-threaded BlockingCollection pattern with asynchronous loop pattern.
//  12/13/2012 - Starlynn Danyelle Gilliam
//        Modified Header.
//
//******************************************************************************************************

using GSF.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#if !MONO
using System.Net.Security;
using System.Security.Authentication;
#endif

namespace GSF.Communication
{
    /// <summary>
    /// Represents a TCP-based communication client.
    /// </summary>
    /// <remarks>
    /// The socket can be bound to a specified interface on a machine with multiple interfaces by 
    /// specifying the IP of the interface in the <see cref="ClientBase.ConnectionString"/>
    /// (Example: "Server=localhost:8888; Interface=192.168.1.15")
    /// </remarks>
    /// <example>
    /// This example shows how to use the <see cref="TcpClient"/> component:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// using GSF.Security.Cryptography;
    /// using GSF.IO.Compression;
    /// 
    /// class Program
    /// {
    ///     static TcpClient s_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         s_client = new TcpClient("Server=localhost:8888");
    ///         s_client.Handshake = false;
    ///         s_client.PayloadAware = false;
    ///         s_client.ReceiveTimeout = -1;
    ///         s_client.MaxConnectionAttempts = 5;
    ///         s_client.Encryption = CipherStrength.None;
    ///         s_client.Compression = CompressionStrength.NoCompression;
    ///         s_client.SecureSession = false;
    ///         s_client.Initialize();
    ///         // Register event handlers.
    ///         s_client.ConnectionAttempt += s_client_ConnectionAttempt;
    ///         s_client.ConnectionEstablished += s_client_ConnectionEstablished;
    ///         s_client.ConnectionTerminated += s_client_ConnectionTerminated;
    ///         s_client.ReceiveDataComplete += s_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         s_client.Connect();
    /// 
    ///         // Transmit user input to the server.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             s_client.Send(input);
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         s_client.Dispose();
    ///     }
    /// 
    ///     static void s_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to server.");
    ///     }
    /// 
    ///     static void s_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to server.");
    ///     }
    /// 
    ///     static void s_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from server.");
    ///     }
    /// 
    ///     static void s_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}.", s_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    // ReSharper disable UnusedVariable
    public class TcpClient : ClientBase
    {
        #region [ Members ]

        // Nested Types
        private class TcpClientPayload
        {
            public byte[] Data;
            public int Offset;
            public int Length;

            public ManualResetEventSlim WaitHandle;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        /// <summary>
        /// Specifies the default value for the <see cref="IntegratedSecurity"/> property.
        /// </summary>
        public const bool DefaultIntegratedSecurity = false;

        /// <summary>
        /// Specifies the default value for the <see cref="IgnoreInvalidCredentials"/> property.
        /// </summary>
        public const bool DefaultIgnoreInvalidCredentials = false;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowDualStackSocket"/> property.
        /// </summary>
        public const bool DefaultAllowDualStackSocket = true;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        public const int DefaultMaxSendQueueSize = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888";

        // Fields
        private readonly object m_sendLock;
        private readonly ConcurrentQueue<TcpClientPayload> m_sendQueue;
        private int m_sending;
        private int m_receiving;
        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private bool m_integratedSecurity;
        private bool m_ignoreInvalidCredentials;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private int m_connectionAttempts;
        private readonly TransportProvider<Socket> m_tcpClient;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectWaitHandle;
        private NetworkCredential m_networkCredential;

        private SocketAsyncEventArgs m_connectArgs;
        private SocketAsyncEventArgs m_receiveArgs;
        private SocketAsyncEventArgs m_sendArgs;
        private readonly EventHandler<SocketAsyncEventArgs> m_connectHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_sendHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_receivePayloadAwareHandler;
        private readonly EventHandler<SocketAsyncEventArgs> m_receivePayloadUnawareHandler;

        private bool m_disposed;

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
        /// <param name="connectString">Connect string of the <see cref="TcpClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public TcpClient(string connectString)
            : base(TransportProtocol.Tcp, connectString)
        {
            m_sendLock = new object();
            m_sendQueue = new ConcurrentQueue<TcpClientPayload>();
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_integratedSecurity = DefaultIntegratedSecurity;
            m_ignoreInvalidCredentials = DefaultIgnoreInvalidCredentials;
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_tcpClient = new TransportProvider<Socket>();

            m_connectHandler = (o, args) => ProcessConnect();
            m_sendHandler = (o, args) => ProcessSend();
            m_receivePayloadAwareHandler = (o, args) => ProcessReceivePayloadAware();
            m_receivePayloadUnawareHandler = (o, args) => ProcessReceivePayloadUnaware();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpClient"/>.</param>
        public TcpClient(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
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
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty buffer.</exception>
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
                if ((object)value == null || value.Length == 0)
                    throw new ArgumentNullException("value");

                m_payloadMarker = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the current Windows account credentials are used for authentication.
        /// </summary>
        /// <remarks>   
        /// This option is ignored under Mono deployments.
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultIntegratedSecurity),
        Description("Indicates whether the current Windows account credentials are used for authentication.")]
        public bool IntegratedSecurity
        {
            get
            {
                return m_integratedSecurity;
            }
            set
            {
#if MONO
                if (value)
                    throw new NotImplementedException("Not supported under Mono.");
#else
                m_integratedSecurity = value;
#endif
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server
        /// should ignore errors when the client's credentials are invalid.
        /// </summary>
        /// <remarks>
        /// This property should only be set to true if there is an alternative by which
        /// to authenticate the client when integrated security fails.
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultIgnoreInvalidCredentials),
        Description("Indicates whether the client Windows account credentials are validated during authentication.")]
        public bool IgnoreInvalidCredentials
        {
            get
            {
                return m_ignoreInvalidCredentials;
            }
            set
            {
                m_ignoreInvalidCredentials = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if dual-mode socket is allowed when endpoint address is IPv6.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultAllowDualStackSocket),
        Description("Determines if dual-mode socket is allowed when endpoint address is IPv6.")]
        public bool AllowDualStackSocket
        {
            get
            {
                return m_allowDualStackSocket;
            }
            set
            {
                m_allowDualStackSocket = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultMaxSendQueueSize),
        Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize
        {
            get
            {
                return m_maxSendQueueSize;
            }
            set
            {
                m_maxSendQueueSize = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpClient"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Client
        {
            get
            {
                return m_tcpClient.Provider;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="TcpClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["server"]).ToLower();
            }
        }

        /// <summary>
        /// Gets the receive handler for receiving data on the socket.
        /// </summary>
        private EventHandler<SocketAsyncEventArgs> ReceiveHandler
        {
            get
            {
                return m_payloadAware ? m_receivePayloadAwareHandler : m_receivePayloadUnawareHandler;
            }
        }

        /// <summary>
        /// Gets or sets network credential that is used when
        /// <see cref="IntegratedSecurity"/> is set to <c>true</c>.
        /// </summary>
        public NetworkCredential NetworkCredential
        {
            get
            {
                return m_networkCredential;
            }
            set
            {
                m_networkCredential = value;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                if ((object)m_sendQueue != null)
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0}", m_sendQueue.Count);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("     Wait handle pool size: {0}", ReusableObjectPool<ManualResetEventSlim>.Default.GetPoolSize());
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("         Payload pool size: {0}", ReusableObjectPool<TcpClientPayload>.Default.GetPoolSize());
                statusBuilder.AppendLine();

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside
        /// this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No received data buffer has been defined to read.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if ((object)m_tcpClient.ReceiveBuffer != null)
            {
                int sourceLength = m_tcpClient.BytesReceived - ReadIndex;
                int readBytes = length > sourceLength ? sourceLength : length;
                Buffer.BlockCopy(m_tcpClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

                // Update read index for next call
                ReadIndex += readBytes;

                if (ReadIndex >= m_tcpClient.BytesReceived)
                    ReadIndex = 0;

                return readBytes;
            }

            throw new InvalidOperationException("No received data buffer has been defined to read.");
        }

        /// <summary>
        /// Saves <see cref="TcpClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["PayloadAware", true].Update(m_payloadAware);
                settings["IntegratedSecurity", true].Update(m_integratedSecurity);
                settings["AllowDualStackSocket", true].Update(m_allowDualStackSocket);
                settings["MaxSendQueueSize", true].Update(m_maxSendQueueSize);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="TcpClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            int maxSendQueueSize;

            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("PayloadAware", m_payloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
                settings.Add("IntegratedSecurity", m_integratedSecurity, "True if the current Windows account credentials are used for authentication, otherwise False.");
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
                PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                IntegratedSecurity = settings["IntegratedSecurity"].ValueAs(m_integratedSecurity);
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);

                // Overwrite config file if max send queue size exists in connection string.
                if (m_connectData.ContainsKey("maxSendQueueSize") && int.TryParse(m_connectData["maxSendQueueSize"], out maxSendQueueSize))
                    m_maxSendQueueSize = maxSendQueueSize;
            }
        }

        /// <summary>
        /// Disconnects the <see cref="TcpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (CurrentState != ClientState.Disconnected)
                {
                    base.Disconnect();

                    if ((object)m_tcpClient.Provider != null && m_tcpClient.Provider.Connected)
                        m_tcpClient.Provider.Disconnect(false);

                    if ((object)m_connectWaitHandle != null)
                        m_connectWaitHandle.Set();

                    m_tcpClient.Reset();
                }
            }
            catch (Exception ex)
            {
                OnSendDataException(new InvalidOperationException(string.Format("Disconnect exception: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Connects the <see cref="TcpClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="TcpClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            string integratedSecuritySetting;

            try
            {
                if (CurrentState == ClientState.Disconnected && !m_disposed)
                {
                    try
                    {
                        // Client may still be attempting to receive data from a prior connection
                        if (Interlocked.CompareExchange(ref m_receiving, 0, 0) != 0)
                            throw new InvalidOperationException("Client is not yet fully disconnected");

                        if ((object)m_connectWaitHandle == null)
                            m_connectWaitHandle = (ManualResetEvent)base.ConnectAsync();

                        OnConnectionAttempt();
                        m_connectWaitHandle.Reset();

                        // Overwrite config file if integrated security exists in connection string
                        if (m_connectData.TryGetValue("integratedSecurity", out integratedSecuritySetting))
                            m_integratedSecurity = integratedSecuritySetting.ParseBoolean();
#if MONO
                        // Force integrated security to be False under Mono since it's not supported
                        m_integratedSecurity = false;
#endif
                        // Create client socket to establish presence
                        if (m_tcpClient.Provider == null)
                            m_tcpClient.Provider = Transport.CreateSocket(m_connectData["interface"], 0, ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);

                        // Set socket receive buffer size; larger buffer size helps prevent buffer overrun
                        m_tcpClient.Provider.ReceiveBufferSize = ReceiveBufferSize;

                        Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

                        // Begin asynchronous connect operation and return wait handle for the asynchronous operation
                        using (SocketAsyncEventArgs connectArgs = m_connectArgs)
                        {
                            m_connectArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                        }

                        m_connectArgs.RemoteEndPoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);
                        m_connectArgs.Completed += m_connectHandler;

                        if (!m_tcpClient.Provider.ConnectAsync(m_connectArgs))
                            ThreadPool.QueueUserWorkItem(state => ProcessConnect());
                    }
                    catch (Exception ex)
                    {
                        if ((object)m_connectWaitHandle != null)
                            m_connectWaitHandle.Set();

                        OnConnectionException(ex);
                    }
                }

                return m_connectWaitHandle;
            }
            catch
            {
                Interlocked.Exchange(ref m_receiving, 0);
                throw;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TcpClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if ((object)m_connectWaitHandle != null)
                        {
                            m_connectWaitHandle.Set();
                            m_connectWaitHandle.Dispose();
                            m_connectWaitHandle = null;
                        }

                        if ((object)m_connectArgs != null)
                        {
                            m_connectArgs.Dispose();
                            m_connectArgs = null;
                        }

                        if ((object)m_sendArgs != null)
                        {
                            m_sendArgs.Dispose();
                            m_sendArgs = null;
                        }

                        if ((object)m_receiveArgs != null)
                        {
                            m_receiveArgs.Dispose();
                            m_receiveArgs = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Server property is missing.</exception>
        /// <exception cref="FormatException">Server property is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Server port value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_connectData);

            // Check if 'server' property is missing.
            if (!m_connectData.ContainsKey("server"))
                throw new ArgumentException(string.Format("Server property is missing (Example: {0})", DefaultConnectionString));

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888
            // Old Format: Server=localhost; Port=8888
            if (m_connectData.ContainsKey("port"))
                m_connectData["server"] = string.Format("{0}:{1}", m_connectData["server"], m_connectData["port"]);

            // Check if 'server' property is valid.
            Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

            if (endpoint == Match.Empty)
                throw new FormatException(string.Format("Server property is invalid (Example: {0})", DefaultConnectionString));

            if (!Transport.IsPortNumberValid(endpoint.Groups["port"].Value))
                throw new ArgumentOutOfRangeException("connectionString", string.Format("Server port must between {0} and {1}", Transport.PortRangeLow, Transport.PortRangeHigh));
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
            TcpClientPayload payload;
            TcpClientPayload dequeuedPayload;
            ManualResetEventSlim handle;

            try
            {
                // Check to see if the client has reached the maximum send queue size.
                if (m_maxSendQueueSize > 0 && m_sendQueue.Count >= m_maxSendQueueSize)
                {
                    for (int i = 0; i < m_maxSendQueueSize; i++)
                    {
                        if (m_sendQueue.TryDequeue(out payload))
                        {
                            payload.WaitHandle.Set();
                            payload.WaitHandle.Dispose();
                            payload.WaitHandle = null;
                        }
                    }

                    throw new InvalidOperationException(string.Format("TCP client reached maximum send queue size. {0} payloads dumped from the queue.", m_maxSendQueueSize));
                }

                // Prepare for payload-aware transmission.
                if (m_payloadAware)
                    Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

                // Create payload and wait handle.
                payload = ReusableObjectPool<TcpClientPayload>.Default.TakeObject();
                handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

                payload.Data = data;
                payload.Offset = offset;
                payload.Length = length;
                payload.WaitHandle = handle;
                handle.Reset();

                // Queue payload for sending.
                m_sendQueue.Enqueue(payload);

                lock (m_sendLock)
                {
                    // Send the next queued payload.
                    if (Interlocked.CompareExchange(ref m_sending, 1, 0) == 0)
                    {
                        if (m_sendQueue.TryDequeue(out dequeuedPayload))
                            ThreadPool.QueueUserWorkItem(state => SendPayload((TcpClientPayload)state), dequeuedPayload);
                        else
                            Interlocked.Exchange(ref m_sending, 0);
                    }
                }

                // Notify that the send operation has started.
                OnSendDataStart();

                // Return the async handle that can be used to wait for the async operation to complete.
                return handle.WaitHandle;
            }
            catch (Exception ex)
            {
                OnSendDataException(ex);
            }

            return null;
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected override void OnSendDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnSendDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected void OnReceiveDataException(SocketException ex)
        {
            if (ex.SocketErrorCode != SocketError.Disconnecting)
                OnReceiveDataException((Exception)ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnReceiveDataException(ex);
        }

        /// <summary>
        /// Callback method for asynchronous connect operation.
        /// </summary>
        private void ProcessConnect()
        {
            try
            {
                // Perform post-connect operations.
                m_connectionAttempts++;

                if (m_connectArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_connectArgs.SocketError);

#if !MONO
                // Send current Windows credentials for authentication.
                if (m_integratedSecurity)
                {
                    NetworkStream socketStream = null;
                    NegotiateStream authenticationStream = null;
                    try
                    {
                        socketStream = new NetworkStream(m_tcpClient.Provider);
                        authenticationStream = new NegotiateStream(socketStream);
                        authenticationStream.AuthenticateAsClient(m_networkCredential ?? (NetworkCredential)CredentialCache.DefaultCredentials, string.Empty);
                    }
                    catch (InvalidCredentialException)
                    {
                        if (!m_ignoreInvalidCredentials)
                            throw;
                    }
                    finally
                    {
                        if (socketStream != null)
                            socketStream.Dispose();

                        if (authenticationStream != null)
                            authenticationStream.Dispose();
                    }
                }
#endif

                // Set up send and receive args.
                using (SocketAsyncEventArgs sendArgs = m_sendArgs, receiveArgs = m_receiveArgs)
                {
                    m_sendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    m_receiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                }

                m_tcpClient.SetSendBuffer(SendBufferSize);
                m_sendArgs.SetBuffer(m_tcpClient.SendBuffer, 0, m_tcpClient.SendBufferSize);
                m_sendArgs.Completed += m_sendHandler;
                m_receiveArgs.Completed += ReceiveHandler;

                // Notify user of established connection.
                m_connectWaitHandle.Set();
                OnConnectionEstablished();

                // Set up send buffer and begin receiving.
                Interlocked.Exchange(ref m_receiving, 1);
                ReceivePayloadAsync();
            }
            catch (SocketException ex)
            {
                OnConnectionException(ex);
                if (ex.SocketErrorCode == SocketError.ConnectionRefused &&
                    (MaxConnectionAttempts == -1 || m_connectionAttempts < MaxConnectionAttempts))
                {
                    // Server is unavailable, so keep retrying connection to the server.
                    try
                    {
                        ConnectAsync();
                    }
                    catch
                    {
                        TerminateConnection();
                    }
                }
                else
                {
                    // For any other reason, clean-up as if the client was disconnected.
                    TerminateConnection();
                }
            }
            catch (Exception ex)
            {
                // This is highly unlikely, but we must handle this situation just-in-case.
                OnConnectionException(ex);
                TerminateConnection();
            }
        }

        /// <summary>
        /// Sends a payload on the socket.
        /// </summary>
        private void SendPayload(TcpClientPayload payload)
        {
            int copyLength;

            try
            {
                // Set the user token of the socket args.
                m_sendArgs.UserToken = payload;

                // Copy payload into send buffer.
                copyLength = Math.Min(payload.Length, m_tcpClient.SendBufferSize);
                Buffer.BlockCopy(payload.Data, payload.Offset, m_tcpClient.SendBuffer, 0, copyLength);

                // Set buffer of send args.
                m_sendArgs.SetBuffer(0, copyLength);

                // Update payload offset and length.
                payload.Offset += copyLength;
                payload.Length -= copyLength;

                // Send data over socket.
                if (!m_tcpClient.Provider.SendAsync(m_sendArgs))
                    ProcessSend();
            }
            catch (Exception ex)
            {
                OnSendDataException(ex);

                // Assume process send was not able
                // to continue the asynchronous loop.
                Interlocked.Exchange(ref m_sending, 0);
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend()
        {
            TcpClientPayload payload = null;
            ManualResetEventSlim handle = null;

            try
            {
                // Get the payload and its wait handle.
                payload = (TcpClientPayload)m_sendArgs.UserToken;
                handle = payload.WaitHandle;

                // Determine whether we are finished with this
                // payload and, if so, set the wait handle.
                if (payload.Length <= 0)
                    handle.Set();

                // Check for errors during send operation.
                if (m_sendArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_sendArgs.SocketError);

                // Update statistics.
                m_tcpClient.Statistics.UpdateBytesSent(m_sendArgs.BytesTransferred);

                // Send operation is complete.
                if (payload.Length <= 0)
                    OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
            finally
            {
                if ((object)payload != null)
                {
                    try
                    {
                        if (payload.Length > 0)
                        {
                            // Still more to send for this payload.
                            ThreadPool.QueueUserWorkItem(state => SendPayload((TcpClientPayload)state), payload);
                        }
                        else
                        {
                            payload.WaitHandle = null;

                            // Return payload and wait handle to their respective object pools.
                            ReusableObjectPool<TcpClientPayload>.Default.ReturnObject(payload);
                            ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                            // Begin sending next client payload.
                            if (m_sendQueue.TryDequeue(out payload))
                            {
                                ThreadPool.QueueUserWorkItem(state => SendPayload((TcpClientPayload)state), payload);
                            }
                            else
                            {
                                lock (m_sendLock)
                                {
                                    if (m_sendQueue.TryDequeue(out payload))
                                        ThreadPool.QueueUserWorkItem(state => SendPayload((TcpClientPayload)state), payload);
                                    else
                                        Interlocked.Exchange(ref m_sending, 0);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);
                        OnSendDataException(new Exception(errorMessage, ex));
                        Interlocked.Exchange(ref m_sending, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync()
        {
            // Initialize bytes received.
            m_tcpClient.BytesReceived = 0;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Set user token to indicate we are waiting for payload header.
                m_receiveArgs.UserToken = true;

                // Payload boundaries are to be preserved.
                m_tcpClient.SetReceiveBuffer(m_payloadMarker.Length + Payload.LengthSegment);
                ReceivePayloadAwareAsync();
            }
            else
            {
                // Payload boundaries are not to be preserved.
                m_tcpClient.SetReceiveBuffer(ReceiveBufferSize);
                ReceivePayloadUnawareAsync();
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync()
        {
            // Set buffer, offset, and length so that we write
            // into buffer space that has not yet been filled.
            byte[] buffer = m_tcpClient.ReceiveBuffer;
            int offset = m_tcpClient.BytesReceived;
            int length = m_tcpClient.ReceiveBufferSize - offset;

            m_receiveArgs.SetBuffer(buffer, offset, length);

            if (!m_tcpClient.Provider.ReceiveAsync(m_receiveArgs))
                ThreadPool.QueueUserWorkItem(state => ProcessReceivePayloadAware());
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware()
        {
            try
            {
                if (m_receiveArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_receiveArgs.SocketError);

                if (m_receiveArgs.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and bytes received.
                m_tcpClient.Statistics.UpdateBytesReceived(m_receiveArgs.BytesTransferred);
                m_tcpClient.BytesReceived += m_receiveArgs.BytesTransferred;

                if ((bool)m_receiveArgs.UserToken)
                {
                    // We're waiting on the payload length, so we'll check if the received data has this information.
                    int payloadLength = Payload.ExtractLength(m_tcpClient.ReceiveBuffer, m_tcpClient.BytesReceived, m_payloadMarker);

                    // We have the payload length.
                    // If it is set to zero, there is no payload; wait for another header.
                    // Otherwise we'll create a buffer that's big enough to hold the entire payload.
                    if (payloadLength == 0)
                    {
                        m_tcpClient.BytesReceived = 0;
                    }
                    else if (payloadLength != -1)
                    {
                        m_tcpClient.BytesReceived = 0;
                        m_tcpClient.SetReceiveBuffer(payloadLength);
                        m_receiveArgs.UserToken = false;
                    }

                    ReceivePayloadAwareAsync();
                }
                else
                {
                    // We're accumulating the payload in the receive buffer until the entire payload is received.
                    if (m_tcpClient.BytesReceived == m_tcpClient.ReceiveBufferSize)
                    {
                        // We've received the entire payload.
                        OnReceiveDataComplete(m_tcpClient.ReceiveBuffer, m_tcpClient.BytesReceived);
                        ReceivePayloadAsync();
                    }
                    else
                    {
                        // We've not yet received the entire payload.
                        ReceivePayloadAwareAsync();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed.
                TerminateConnection();
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveDataException(ex);
                TerminateConnection();
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveDataException(ex);
                    ReceivePayloadAsync();
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection();
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync()
        {
            // Set buffer and length so that we write into
            // buffer space that has not yet been filled.
            byte[] buffer = m_tcpClient.ReceiveBuffer;
            int length = m_tcpClient.ReceiveBufferSize;

            m_receiveArgs.SetBuffer(buffer, 0, length);

            if (!m_tcpClient.Provider.ReceiveAsync(m_receiveArgs))
                ThreadPool.QueueUserWorkItem(state => ProcessReceivePayloadUnaware());
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware()
        {
            try
            {
                if (m_receiveArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)m_receiveArgs.SocketError);

                if (m_receiveArgs.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and pointers.
                m_tcpClient.Statistics.UpdateBytesReceived(m_receiveArgs.BytesTransferred);
                m_tcpClient.BytesReceived = m_receiveArgs.BytesTransferred;

                // Notify of received data and resume receive operation.
                OnReceiveDataComplete(m_tcpClient.ReceiveBuffer, m_tcpClient.BytesReceived);
                ReceivePayloadUnawareAsync();
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed.
                TerminateConnection();
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveDataException(ex);
                TerminateConnection();
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveDataException(ex);
                    ReceivePayloadAsync();
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection();
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection()
        {
            try
            {
                if (CurrentState != ClientState.Disconnected)
                {
                    if ((object)m_connectWaitHandle != null)
                        m_connectWaitHandle.Set();

                    m_tcpClient.Reset();
                }

                Interlocked.Exchange(ref m_receiving, 0);
                OnConnectionTerminated();
            }
            catch (ThreadAbortException)
            {
                // This is a normal exception
                throw;
            }
            catch
            {
                // Other exceptions can happen (e.g., NullReferenceException) if thread resumes and the class is disposed middle way through this method
            }
        }

        #endregion
    }
}