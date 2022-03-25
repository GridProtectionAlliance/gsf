//******************************************************************************************************
//  TcpClient.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#if !MONO
using System.Net.Security;
using System.Security.Authentication;
#endif
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Threading;

// ReSharper disable AccessToDisposedClosure
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
        private sealed class ConnectState : IDisposable
        {
            public Socket Socket;
        #if !MONO
            public NetworkStream NetworkStream;
            public NegotiateStream NegotiateStream;
        #endif

            public readonly SocketAsyncEventArgs ConnectArgs = new SocketAsyncEventArgs();
            public SocketAsyncEventArgs ReceiveArgs;
            public SocketAsyncEventArgs SendArgs;
            public int ConnectionAttempts;

            public readonly CancellationToken Token = new CancellationToken();
            public ICancellationToken TimeoutToken;

            public void Dispose()
            {
                ConnectArgs.Dispose();
                ReceiveArgs?.Dispose();
                SendArgs?.Dispose();
                Socket?.Dispose();
            #if !MONO
                NetworkStream?.Dispose();
                NegotiateStream?.Dispose();
            #endif
            }
        }

        private sealed class ReceiveState : IDisposable
        {
            public Socket Socket;
            public SocketAsyncEventArgs ReceiveArgs;
            public SocketAsyncEventArgs SendArgs;

            public byte[] Buffer;
            public int Offset;
            public int PayloadLength = -1;

            public CancellationToken Token;

            public void Dispose()
            {
                Dispose(Socket);
                Dispose(ReceiveArgs);
                Dispose(SendArgs);
            }

            private static void Dispose(IDisposable obj) => obj?.Dispose();
        }

        private sealed class SendState : IDisposable
        {
            public Socket Socket;
            public SocketAsyncEventArgs ReceiveArgs;
            public SocketAsyncEventArgs SendArgs;

            public readonly ConcurrentQueue<TcpClientPayload> SendQueue = new ConcurrentQueue<TcpClientPayload>();
            public TcpClientPayload Payload;
            public int Sending;

            public CancellationToken Token;

            public void Dispose()
            {
                Dispose(Socket);
                Dispose(ReceiveArgs);
                Dispose(SendArgs);

                while (SendQueue.TryDequeue(out TcpClientPayload payload))
                {
                    payload.WaitHandle.Set();
                    payload.WaitHandle.Dispose();
                }
            }

            private static void Dispose(IDisposable obj) => obj?.Dispose();
        }
        private class TcpClientPayload
        {
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEvent WaitHandle;
        }

        private class CancellationToken
        {
            private int m_cancelled;

            public bool Cancelled => Interlocked.CompareExchange(ref m_cancelled, 0, 0) != 0;

            public bool Cancel() => Interlocked.Exchange(ref m_cancelled, 1) != 0;
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
        public const int DefaultMaxSendQueueSize = 500000;

        /// <summary>
        /// Specifies the default value for the <see cref="NoDelay"/> property.
        /// </summary>
        public const bool DefaultNoDelay = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888";

        // Fields
        private byte[] m_payloadMarker;
        private EndianOrder m_payloadEndianOrder;
        private IPStack m_ipStack;
        private readonly ShortSynchronizedOperation m_dumpPayloadsOperation;
        private string[] m_serverList;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectWaitHandle;
        private ConnectState m_connectState;
        private ReceiveState m_receiveState;
        private SendState m_sendState;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        public TcpClient() : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="TcpClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public TcpClient(string connectString) : base(TransportProtocol.Tcp, connectString)
        {
            PayloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_payloadEndianOrder = EndianOrder.LittleEndian;
            IntegratedSecurity = DefaultIntegratedSecurity;
            IgnoreInvalidCredentials = DefaultIgnoreInvalidCredentials;
            AllowDualStackSocket = DefaultAllowDualStackSocket;
            MaxSendQueueSize = DefaultMaxSendQueueSize;
            NoDelay = DefaultNoDelay;
            m_dumpPayloadsOperation = new ShortSynchronizedOperation(DumpPayloads, OnSendDataException);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpClient"/>.</param>
        public TcpClient(IContainer container) : this() => container?.Add(this);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
        [Category("Data")]
        [DefaultValue(DefaultPayloadAware)]
        [Description("Indicates whether the payload boundaries are to be preserved during transmission.")]
        public bool PayloadAware { get; set; }

        /// <summary>
        /// Gets or sets the byte sequence used to mark the beginning of a payload in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <remarks>
        /// Setting property to <c>null</c> will create a zero-length payload marker.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] PayloadMarker
        {
            get => m_payloadMarker;
            set => m_payloadMarker = value ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets or sets the endian order to apply for encoding and decoding payload size in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <remarks>
        /// Setting property to <c>null</c> will force use of little-endian encoding.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EndianOrder PayloadEndianOrder
        {
            get => m_payloadEndianOrder;
            set => m_payloadEndianOrder = value ?? EndianOrder.LittleEndian;
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the current Windows account credentials are used for authentication.
        /// </summary>
        /// <remarks>   
        /// This option is ignored under Mono deployments.
        /// </remarks>
        [Category("Security")]
        [DefaultValue(DefaultIntegratedSecurity)]
        [Description("Indicates whether the current Windows account credentials are used for authentication.")]
        public bool IntegratedSecurity { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server
        /// should ignore errors when the client's credentials are invalid.
        /// </summary>
        /// <remarks>
        /// This property should only be set to true if there is an alternative by which
        /// to authenticate the client when integrated security fails.
        /// </remarks>
        [Category("Security")]
        [DefaultValue(DefaultIgnoreInvalidCredentials)]
        [Description("Indicates whether the client Windows account credentials are validated during authentication.")]
        public bool IgnoreInvalidCredentials { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that determines if dual-mode socket is allowed when endpoint address is IPv6.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(DefaultAllowDualStackSocket)]
        [Description("Determines if dual-mode socket is allowed when endpoint address is IPv6.")]
        public bool AllowDualStackSocket { get; set; }

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(DefaultMaxSendQueueSize)]
        [Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that determines if small packets are delivered to the remote host without delay.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(DefaultNoDelay)]
        [Description("Determines if small packets are delivered to the remote host without delay.")]
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpClient"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Client => m_connectState?.Socket;

        /// <summary>
        /// Gets the server URI of the <see cref="TcpClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri => $"{TransportProtocol}://{ServerList[ServerIndex]}".ToLower();

        /// <summary>
        /// Gets or sets network credential that is used when
        /// <see cref="IntegratedSecurity"/> is set to <c>true</c>.
        /// </summary>
        public NetworkCredential NetworkCredential { get; set; }

        /// <summary>
        /// Determines whether the base class should track statistics.
        /// </summary>
        protected override bool TrackStatistics => false;

        // Gets server connect data as an array - will always be at least one empty string, not null
        private string[] ServerList
        {
            get
            {
                if (!(m_serverList is null))
                    return m_serverList;

                if (m_connectData is null || !m_connectData.TryGetValue("server", out string serverList) || string.IsNullOrWhiteSpace(serverList))
                    return Array.Empty<string>();

                return m_serverList = serverList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(server => server.Trim()).ToArray();
            }
        }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                SendState sendState = m_sendState;
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                if (sendState is not null)
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0}", sendState.SendQueue.Count);
                    statusBuilder.AppendLine();
                }

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="TcpClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (!PersistSettings)
                return;

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings["PayloadAware", true].Update(PayloadAware);
            settings["IntegratedSecurity", true].Update(IntegratedSecurity);
            settings["AllowDualStackSocket", true].Update(AllowDualStackSocket);
            settings["MaxSendQueueSize", true].Update(MaxSendQueueSize);
            settings["NoDelay", true].Update(NoDelay);
            
            config.Save();
        }

        /// <summary>
        /// Loads saved <see cref="TcpClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (!PersistSettings)
                return;

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings.Add("PayloadAware", PayloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
            settings.Add("IntegratedSecurity", IntegratedSecurity, "True if the current Windows account credentials are used for authentication, otherwise False.");
            settings.Add("AllowDualStackSocket", AllowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
            settings.Add("MaxSendQueueSize", MaxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
            settings.Add("NoDelay", NoDelay, "True to disable Nagle so that small packets are delivered to the remote host without delay, otherwise False.");
            
            PayloadAware = settings["PayloadAware"].ValueAs(PayloadAware);
            IntegratedSecurity = settings["IntegratedSecurity"].ValueAs(IntegratedSecurity);
            AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(AllowDualStackSocket);
            MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(MaxSendQueueSize);
            NoDelay = settings["NoDelay"].ValueAs(NoDelay);

            // Overwrite config file if max send queue size exists in connection string.
            if (m_connectData.ContainsKey("maxSendQueueSize") && int.TryParse(m_connectData["maxSendQueueSize"], out int maxSendQueueSize))
                MaxSendQueueSize = maxSendQueueSize;
        }

        /// <summary>
        /// Connects the <see cref="TcpClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="TcpClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            ConnectState connectState = null;

            if (CurrentState != ClientState.Disconnected || m_disposed)
                return m_connectWaitHandle;

            try
            {
                // If we do not already have a wait handle to use
                // for connections, get one from the base class
                if (m_connectWaitHandle is null)
                    m_connectWaitHandle = (ManualResetEvent)base.ConnectAsync();

                // Create state object for the asynchronous connection loop
                connectState = new ConnectState();

                // Store connectState in m_connectState so that calls to Disconnect
                // and Dispose can dispose resources and cancel asynchronous loops
                using (m_connectState)
                {
                    m_connectState = connectState;
                }

                OnConnectionAttempt();
                m_connectWaitHandle.Reset();

                // Overwrite config file if integrated security exists in connection string
                if (m_connectData.TryGetValue("integratedSecurity", out string integratedSecuritySetting))
                    IntegratedSecurity = integratedSecuritySetting.ParseBoolean();

            #if MONO
                // Force integrated security to be False under Mono since it's not supported
                IntegratedSecurity = false;
            #endif

                // Overwrite config file if no delay exists in connection string.
                if (m_connectData.TryGetValue("noDelay", out string noDelaySetting))
                    NoDelay = noDelaySetting.ParseBoolean();

                // Initialize state object for the asynchronous connection loop
                Match endpoint = Regex.Match(ServerList[ServerIndex], Transport.EndpointFormatRegex);

                connectState.ConnectArgs.RemoteEndPoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);
                connectState.ConnectArgs.SocketFlags = SocketFlags.None;
                connectState.ConnectArgs.UserToken = connectState;
                connectState.ConnectArgs.Completed += (_, args) => ProcessConnect((ConnectState)args.UserToken);

                // Create client socket
                connectState.Socket = Transport.CreateSocket(m_connectData["interface"], 0, ProtocolType.Tcp, m_ipStack, AllowDualStackSocket);
                connectState.Socket.NoDelay = NoDelay;

                // Initiate the asynchronous connection loop
                ConnectAsync(connectState);
            }
            catch (Exception ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // Terminate the connection
                if (connectState is not null)
                    TerminateConnection(connectState.Token);

                // Ensure that the wait handle is set so that operations waiting
                // for completion of the asynchronous connection loop can continue
                m_connectWaitHandle?.Set();
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of erroneously allocated resources
                if (connectState is not null && connectState.Token.Cancelled)
                    connectState.Dispose();
            }

            // Return the wait handle that signals completion
            // of the asynchronous connection loop
            return m_connectWaitHandle;
        }

        /// <summary>
        /// Initiates an asynchronous connection attempt.
        /// </summary>
        private void ConnectAsync(ConnectState connectState)
        {
            if (connectState.Token.Cancelled)
                return;

            if (!connectState.Socket.ConnectAsync(connectState.ConnectArgs))
                ThreadPool.QueueUserWorkItem(_ => ProcessConnect(connectState));
        }

        /// <summary>
        /// Callback method for asynchronous connect operation.
        /// </summary>
        private void ProcessConnect(ConnectState connectState)
        {
            ReceiveState receiveState = null;
            SendState sendState = null;

            try
            {
                // Quit if this connection loop has been cancelled
                if (connectState.Token.Cancelled)
                    return;

                // Increment the number of connection attempts that
                // have occurred in this asynchronous connection loop
                connectState.ConnectionAttempts++;

                // Check the SocketAsyncEventArgs for errors during the asynchronous connection attempt
                if (connectState.ConnectArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)connectState.ConnectArgs.SocketError);

                // Set the size of the buffer used by the socket to store incoming data from the server
                connectState.Socket.ReceiveBufferSize = ReceiveBufferSize;

                if (IntegratedSecurity)
                {
                #if !MONO
                    // Check the state of cancellation one more time before
                    // proceeding to the next step of the connection loop
                    if (connectState.Token.Cancelled)
                        return;

                    // Create the SslStream object used to perform
                    // send and receive operations on the socket
                    connectState.NetworkStream = new NetworkStream(connectState.Socket, false);
                    connectState.NegotiateStream = new NegotiateStream(connectState.NetworkStream, true);

                    connectState.TimeoutToken = new Action(() =>
                    {
                        SocketException ex = new SocketException((int)SocketError.TimedOut);
                        OnConnectionException(ex);
                        TerminateConnection(connectState.Token);
                        connectState.Dispose();
                    }).DelayAndExecute(15000);

                    try
                    {
                        connectState.NegotiateStream.BeginAuthenticateAsClient(NetworkCredential ?? (NetworkCredential)CredentialCache.DefaultCredentials, string.Empty, ProcessIntegratedSecurityAuthentication, connectState);
                    }
                    catch
                    {
                        connectState.TimeoutToken.Cancel();
                        throw;
                    }
                #endif
                }
                else
                {
                    // Initialize the SocketAsyncEventArgs for receive operations
                    connectState.ReceiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    connectState.ReceiveArgs.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);

                    if (PayloadAware)
                        connectState.ReceiveArgs.Completed += (_, args) => ProcessReceivePayloadAware((ReceiveState)args.UserToken);
                    else
                        connectState.ReceiveArgs.Completed += (_, args) => ProcessReceivePayloadUnaware((ReceiveState)args.UserToken);

                    // Initialize the SocketAsyncEventArgs for send operations
                    connectState.SendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                    connectState.SendArgs.SetBuffer(new byte[SendBufferSize], 0, SendBufferSize);
                    connectState.SendArgs.Completed += (_, args) => ProcessSend((SendState)args.UserToken);

                    // Initialize state object for the asynchronous send loop
                    sendState = new SendState
                    {
                        Token = connectState.Token,
                        Socket = connectState.Socket,
                        ReceiveArgs = connectState.ReceiveArgs,
                        SendArgs = connectState.SendArgs
                    };

                    sendState.SendArgs.UserToken = sendState;

                    // Store sendState in m_sendState so that calls to Disconnect
                    // and Dispose can dispose resources and cancel asynchronous loops
                    using (m_sendState)
                    {
                        m_sendState = sendState;
                    }

                    // Check the state of cancellation one more time before
                    // proceeding to the next step of the connection loop
                    if (connectState.Token.Cancelled)
                        return;

                    // Notify of established connection
                    m_connectWaitHandle.Set();
                    OnConnectionEstablished();

                    // Initialize state object for the asynchronous receive loop
                    receiveState = new ReceiveState
                    {
                        Token = connectState.Token,
                        Socket = connectState.Socket,
                        Buffer = connectState.ReceiveArgs.Buffer,
                        ReceiveArgs = connectState.ReceiveArgs
                    };

                    receiveState.ReceiveArgs.UserToken = receiveState;
                    receiveState.SendArgs = connectState.SendArgs;

                    // Store receiveState in m_receiveState so that calls to Disconnect
                    // and Dispose can dispose resources and cancel asynchronous loops
                    using (m_receiveState)
                    {
                        m_receiveState = receiveState;
                    }

                    // Start receiving data
                    if (PayloadAware)
                        ReceivePayloadAwareAsync(receiveState);
                    else
                        ReceivePayloadUnawareAsync(receiveState);

                    // Further socket interactions are handled through the ReceiveArgs
                    // and SendArgs objects, so the ConnectArgs is no longer needed
                    connectState.ConnectArgs.Dispose();
                }
            }
            catch (SocketException ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // If the connection is refused by the server,
                // keep trying until we reach our maximum connection attempts
                if (ex.SocketErrorCode == SocketError.ConnectionRefused && (MaxConnectionAttempts == -1 || connectState.ConnectionAttempts < MaxConnectionAttempts))
                {
                    // Server is unavailable, so keep retrying connection to the server.
                    try
                    {
                        ConnectAsync(connectState);
                    }
                    catch
                    {
                        TerminateConnection(connectState.Token);
                    }
                }
                else
                {
                    // For any other reason, clean-up as if the client was disconnected.
                    TerminateConnection(connectState.Token);
                }
            }
            catch (Exception ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // Terminate the connection
                TerminateConnection(connectState.Token);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of erroneously allocated resources
                if (connectState is not null && connectState.Token.Cancelled)
                    connectState.Dispose();

                if (receiveState is not null && receiveState.Token.Cancelled)
                    receiveState.Dispose();

                if (sendState is not null && sendState.Token.Cancelled)
                    sendState.Dispose();
            }
        }

    #if !MONO
        private void ProcessIntegratedSecurityAuthentication(IAsyncResult asyncResult)
        {
            ConnectState connectState = null;
            ReceiveState receiveState = null;
            SendState sendState = null;

            try
            {
                // Get the connect state from the async result
                connectState = (ConnectState)asyncResult.AsyncState;

                // Attempt to cancel the timeout operation
                if (!connectState.TimeoutToken.Cancel())
                    return;

                // Quit if this connection loop has been cancelled
                if (connectState.Token.Cancelled)
                    return;

                try
                {
                    // Complete the operation to authenticate with the server
                    connectState.NegotiateStream.EndAuthenticateAsClient(asyncResult);
                }
                catch (InvalidCredentialException)
                {
                    if (!IgnoreInvalidCredentials)
                        throw;
                }

                // Initialize the SocketAsyncEventArgs for receive operations
                connectState.ReceiveArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                connectState.ReceiveArgs.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);

                if (PayloadAware)
                    connectState.ReceiveArgs.Completed += (sender, args) => ProcessReceivePayloadAware((ReceiveState)args.UserToken);
                else
                    connectState.ReceiveArgs.Completed += (sender, args) => ProcessReceivePayloadUnaware((ReceiveState)args.UserToken);

                // Initialize the SocketAsyncEventArgs for send operations
                connectState.SendArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();
                connectState.SendArgs.SetBuffer(new byte[SendBufferSize], 0, SendBufferSize);
                connectState.SendArgs.Completed += (sender, args) => ProcessSend((SendState)args.UserToken);

                // Initialize state object for the asynchronous send loop
                sendState = new SendState
                {
                    Token = connectState.Token,
                    Socket = connectState.Socket,
                    ReceiveArgs = connectState.ReceiveArgs,
                    SendArgs = connectState.SendArgs
                };

                sendState.SendArgs.UserToken = sendState;

                // Store sendState in m_sendState so that calls to Disconnect
                // and Dispose can dispose resources and cancel asynchronous loops
                using (m_sendState)
                {
                    m_sendState = sendState;
                }

                // Check the state of cancellation one more time before
                // proceeding to the next step of the connection loop
                if (connectState.Token.Cancelled)
                    return;

                // Notify of established connection
                // and begin receiving data.
                m_connectWaitHandle.Set();
                OnConnectionEstablished();

                // Initialize state object for the asynchronous receive loop
                receiveState = new ReceiveState
                {
                    Token = connectState.Token,
                    Socket = connectState.Socket,
                    Buffer = connectState.ReceiveArgs.Buffer,
                    ReceiveArgs = connectState.ReceiveArgs
                };

                receiveState.ReceiveArgs.UserToken = receiveState;
                receiveState.SendArgs = connectState.SendArgs;

                // Store receiveState in m_receiveState so that calls to Disconnect
                // and Dispose can dispose resources and cancel asynchronous loops
                using (m_receiveState)
                {
                    m_receiveState = receiveState;
                }

                // Start receiving data
                if (PayloadAware)
                    ReceivePayloadAwareAsync(receiveState);
                else
                    ReceivePayloadUnawareAsync(receiveState);

                // Further socket interactions are handled through the SslStream
                // object, so the SocketAsyncEventArgs is no longer needed
                connectState.ConnectArgs.Dispose();
            }
            catch (SocketException ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // If connectState is null, we cannot proceed
                if (connectState is null)
                    return;

                // If the connection is refused by the server,
                // keep trying until we reach our maximum connection attempts
                if (ex.SocketErrorCode == SocketError.ConnectionRefused && (MaxConnectionAttempts == -1 || connectState.ConnectionAttempts < MaxConnectionAttempts))
                {
                    try
                    {
                        ConnectAsync(connectState);
                    }
                    catch
                    {
                        TerminateConnection(connectState.Token);
                    }
                }
                else
                {
                    // For any other socket exception,
                    // terminate the connection
                    TerminateConnection(connectState.Token);
                }
            }
            catch (Exception ex)
            {
                // Log exception during connection attempt
                string errorMessage = $"Unable to authenticate connection to server: {ex.Message}";
                OnConnectionException(new Exception(errorMessage, ex));

                // Terminate the connection
                if (connectState is not null)
                    TerminateConnection(connectState.Token);
            }
            finally
            {
                if (connectState is not null)
                {
                    // If the operation was cancelled during execution,
                    // make sure to dispose of erroneously allocated resources;
                    // otherwise, dispose of the NegotiateStream which is only used for authentication
                    if (connectState.Token.Cancelled)
                    {
                        connectState.Dispose();
                    }
                    else
                    {
                        connectState.NetworkStream.Dispose();
                        connectState.NegotiateStream.Dispose();
                    }
                }

                if (receiveState is not null && receiveState.Token.Cancelled)
                    receiveState.Dispose();

                if (sendState is not null && sendState.Token.Cancelled)
                    sendState.Dispose();
            }
        }
    #endif

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(ReceiveState receiveState)
        {
            if (receiveState.Token.Cancelled)
                return;

            int length;

            if (receiveState.PayloadLength < 0)
                length = m_payloadMarker.Length + Payload.LengthSegment;
            else
                length = receiveState.PayloadLength;

            if (receiveState.Buffer != receiveState.ReceiveArgs.Buffer)
                receiveState.ReceiveArgs.SetBuffer(receiveState.Buffer, 0, length);
            else
                receiveState.ReceiveArgs.SetBuffer(receiveState.Offset, length - receiveState.Offset);

            if (!receiveState.Socket.ReceiveAsync(receiveState.ReceiveArgs))
                ThreadPool.QueueUserWorkItem(_ => ProcessReceivePayloadAware(receiveState));
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(ReceiveState receiveState)
        {
            try
            {
                // Quit if this receive loop has been cancelled
                if (receiveState.Token.Cancelled)
                    return;

                // Determine if the server disconnected gracefully
                if (receiveState.ReceiveArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)receiveState.ReceiveArgs.SocketError);

                if (receiveState.ReceiveArgs.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and bytes received.
                UpdateBytesReceived(receiveState.ReceiveArgs.BytesTransferred);
                receiveState.Offset += receiveState.ReceiveArgs.BytesTransferred;

                if (receiveState.PayloadLength < 0)
                {
                    // If we haven't parsed the length of the payload yet, attempt to parse it
                    receiveState.PayloadLength = Payload.ExtractLength(receiveState.Buffer, receiveState.Offset, m_payloadMarker, m_payloadEndianOrder);

                    if (receiveState.PayloadLength > 0)
                    {
                        receiveState.Offset = 0;

                        if (receiveState.Buffer.Length < receiveState.PayloadLength)
                            receiveState.Buffer = new byte[receiveState.PayloadLength];
                    }
                }
                else if (receiveState.Offset == receiveState.PayloadLength)
                {
                    // We've received the entire payload so notify the user
                    OnReceiveDataComplete(receiveState.Buffer, receiveState.PayloadLength);

                    // Reset payload length
                    receiveState.Offset = 0;
                    receiveState.PayloadLength = -1;
                }

                // Continue asynchronous loop
                ReceivePayloadAwareAsync(receiveState);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed.
                TerminateConnection(receiveState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during receive operation
                OnReceiveDataException(ex);

                // Terminate connection when socket exception is encountered
                TerminateConnection(receiveState.Token);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume
                    OnReceiveDataException(ex);
                    ReceivePayloadAwareAsync(receiveState);
                }
                catch
                {
                    // Terminate connection if resume fails
                    TerminateConnection(receiveState.Token);
                }
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if (receiveState is not null && receiveState.Token.Cancelled)
                    receiveState.Dispose();
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync(ReceiveState receiveState)
        {
            if (!receiveState.Token.Cancelled)
            {
                receiveState.ReceiveArgs.SetBuffer(0, ReceiveBufferSize);

                if (!receiveState.Socket.ReceiveAsync(receiveState.ReceiveArgs))
                    ThreadPool.QueueUserWorkItem(_ => ProcessReceivePayloadUnaware(receiveState));
            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(ReceiveState receiveState)
        {
            try
            {
                // Quit if this receive loop has been cancelled
                if (receiveState.Token.Cancelled)
                    return;

                if (receiveState.ReceiveArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)receiveState.ReceiveArgs.SocketError);

                if (receiveState.ReceiveArgs.BytesTransferred == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and bytes received
                UpdateBytesReceived(receiveState.ReceiveArgs.BytesTransferred);
                receiveState.PayloadLength = receiveState.ReceiveArgs.BytesTransferred;

                // Notify of received data and resume receive operation.
                OnReceiveDataComplete(receiveState.Buffer, receiveState.PayloadLength);
                ReceivePayloadUnawareAsync(receiveState);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed
                TerminateConnection(receiveState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during receive operation
                OnReceiveDataException(ex);

                // Terminate connection when socket exception is encountered
                TerminateConnection(receiveState.Token);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume
                    OnReceiveDataException(ex);
                    ReceivePayloadUnawareAsync(receiveState);
                }
                catch
                {
                    // Terminate connection if resume fails
                    TerminateConnection(receiveState.Token);
                }
            }
        }

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
            ReceiveState receiveState = m_receiveState;

            if (receiveState is null || receiveState.Token.Cancelled)
                return 0;

            buffer.ValidateParameters(startIndex, length);

            if (receiveState.Buffer is null)
                throw new InvalidOperationException("No received data buffer has been defined to read.");

            int sourceLength = receiveState.PayloadLength - ReadIndex;
            int readBytes = length > sourceLength ? sourceLength : length;
            Buffer.BlockCopy(receiveState.Buffer, ReadIndex, buffer, startIndex, readBytes);

            // Update read index for next call
            ReadIndex += readBytes;

            if (ReadIndex >= receiveState.PayloadLength)
                ReadIndex = 0;

            return readBytes;
        }

        /// <summary>
        /// Requests that the client attempt to move to the next <see cref="ClientBase.ServerIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Return value will only be <c>true</c> if <see cref="ClientBase.ServerIndex"/> changed.
        /// </remarks>
        public override bool RequestNextServerIndex()
        {
            int serverListLength = ServerList.Length;

            if (serverListLength < 2)
                return false;

            // When multiple servers are available, move to next server connection
            ServerIndex++;

            if (ServerIndex >= serverListLength)
                ServerIndex = 0;

            return true;
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
            SendState sendState = null;

            try
            {
                // Get the current send state
                sendState = m_sendState;

                // Quit if this send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return null;

                // Prepare for payload-aware transmission.
                if (PayloadAware)
                    Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker, m_payloadEndianOrder);

                // Create payload and wait handle.
                TcpClientPayload payload = FastObjectFactory<TcpClientPayload>.CreateObjectFunction();
                ManualResetEvent handle = new ManualResetEvent(false);

                payload.Data = data;
                payload.Offset = offset;
                payload.Length = length;
                payload.WaitHandle = handle;

                // Execute operation to take action if the client
                // has reached the maximum send queue size
                m_dumpPayloadsOperation.TryRun();

                // Queue payload for sending
                sendState.SendQueue.Enqueue(payload);

                // If the send loop is not already running, start the send loop
                if (!sendState.Token.Cancelled)
                {
                    if (Interlocked.CompareExchange(ref sendState.Sending, 1, 0) == 0)
                        SendPayloadAsync(sendState);

                    // Notify that the send operation has started.
                    OnSendDataStart();

                    // Return the async handle that can be used to wait for the async operation to complete
                    return handle;
                }
            }
            catch (Exception ex)
            {
                // Log exception during send operation
                OnSendDataException(ex);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if (sendState is not null && sendState.Token.Cancelled)
                    sendState.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Sends a payload on the socket.
        /// </summary>
        private void SendPayloadAsync(SendState sendState)
        {
            try
            {
                // Quit if this send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return;

                if (sendState.SendQueue.TryDequeue(out TcpClientPayload payload))
                {
                    // Save the payload currently
                    // being sent to the send state
                    sendState.Payload = payload;

                    // Ensure that the payload fits in the send buffer
                    if (payload.Length > sendState.SendArgs.Buffer.Length)
                        sendState.SendArgs.SetBuffer(new byte[payload.Length], 0, payload.Length);
                    else
                        sendState.SendArgs.SetBuffer(0, payload.Length);

                    // Copy payload into send buffer
                    Buffer.BlockCopy(payload.Data, payload.Offset, sendState.SendArgs.Buffer, 0, payload.Length);

                    // Send payload to the client asynchronously
                    if (!sendState.Socket.SendAsync(sendState.SendArgs))
                        ThreadPool.QueueUserWorkItem(state => ProcessSend((SendState)state), sendState);
                }
                else
                {
                    // No more payloads to send, so stop sending payloads
                    Interlocked.Exchange(ref sendState.Sending, 0);

                    // Double-check to ensure that a new payload didn't appear before exiting the send loop
                    if (!sendState.SendQueue.IsEmpty && Interlocked.CompareExchange(ref sendState.Sending, 1, 0) == 0)
                        ThreadPool.QueueUserWorkItem(state => SendPayloadAsync((SendState)state), sendState);
                }
            }
            catch (Exception ex)
            {
                // Log exception during send operation
                OnSendDataException(ex);

                // Continue asynchronous send loop
                ThreadPool.QueueUserWorkItem(state => SendPayloadAsync((SendState)state), sendState);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if (sendState.Token.Cancelled)
                    sendState.Dispose();
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend(SendState sendState)
        {
            ManualResetEvent handle = null;

            try
            {
                // Get the payload and its wait handle.
                TcpClientPayload payload = sendState.Payload;
                handle = payload.WaitHandle;

                // Quit if this send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return;

                // Determine if the server disconnected gracefully
                if (!sendState.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Check for errors during send operation
                if (sendState.SendArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)sendState.SendArgs.SocketError);

                try
                {
                    // Set the wait handle to indicate
                    // the send operation has finished
                    handle.Set();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if the consumer has
                    // disposed of the wait handle
                }

                // Update statistics and notify that the send operation is complete
                UpdateBytesSent(sendState.SendArgs.BytesTransferred);
                OnSendDataComplete();
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed
                TerminateConnection(sendState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during send operation
                OnSendDataException(ex);

                // Terminate connection when socket exception is encountered
                TerminateConnection(sendState.Token);
            }
            catch (Exception ex)
            {
                // For any other exception, notify and resume
                OnSendDataException(ex);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if (sendState is not null && sendState.Token.Cancelled)
                    sendState.Dispose();

                try
                {
                    // Make sure to set the wait handle
                    // even if an exception occurs
                    handle?.Set();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if the consumer has
                    // disposed of the wait handle
                }

                // Attempt to send the next payload
                SendPayloadAsync(sendState);
            }
        }

        /// <summary>
        /// Disconnects the <see cref="TcpClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (CurrentState == ClientState.Disconnected)
                    return;

                ConnectState connectState = m_connectState;
                ReceiveState receiveState = m_receiveState;
                SendState sendState = m_sendState;

                if (connectState is not null)
                {
                    TerminateConnection(connectState.Token);
                    connectState.Socket.Disconnect(false);
                    connectState.Dispose();
                }

                receiveState?.Dispose();
                sendState?.Dispose();
                m_connectWaitHandle?.Set();
            }
            catch (ObjectDisposedException)
            {
                // This can be safely ignored
            }
            catch (Exception ex)
            {
                OnSendDataException(new InvalidOperationException($"Disconnect exception: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TcpClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_connectState is not null)
                {
                    TerminateConnection(m_connectState.Token);
                    m_connectState.Dispose();
                    m_connectState = null;
                }

                if (m_receiveState is not null)
                {
                    m_receiveState.Dispose();
                    m_receiveState = null;
                }

                if (m_sendState is not null)
                {
                    m_sendState.Dispose();
                    m_sendState = null;
                }

                if (m_connectWaitHandle is not null)
                {
                    m_connectWaitHandle.Set();
                    m_connectWaitHandle.Dispose();
                    m_connectWaitHandle = null;
                }
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
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
            if (!m_connectData.ContainsKey("server") || string.IsNullOrWhiteSpace(m_connectData["server"]))
                throw new ArgumentException($"Server property is missing (Example: {DefaultConnectionString})");

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888
            // Old Format: Server=localhost; Port=8888
            if (m_connectData.ContainsKey("port") && !m_connectData["server"].Contains(','))
                m_connectData["server"] = $"{m_connectData["server"]}:{m_connectData["port"]}";

            m_serverList = null;

            foreach (string server in ServerList)
            {
                // Check if 'server' property is valid.
                Match endpoint = Regex.Match(server, Transport.EndpointFormatRegex);

                if (endpoint == Match.Empty)
                    throw new FormatException($"Server property is invalid (Example: {DefaultConnectionString})");

                if (!Transport.IsPortNumberValid(endpoint.Groups["port"].Value))
                    throw new ArgumentOutOfRangeException(nameof(connectionString), $"Server port must between {Transport.PortRangeLow} and {Transport.PortRangeHigh}");
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected override void OnSendDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnSendDataException(ex);
            else
                Logger.SwallowException(ex, "TcpClient.cs: The client state was disconnected");
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected void OnReceiveDataException(SocketException ex)
        {
            if (ex.SocketErrorCode != SocketError.Disconnecting)
                OnReceiveDataException((Exception)ex);
            else
                Logger.SwallowException(ex, "TcpClient.cs: The socket was disconnecting");
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnReceiveDataException(ex);
            else
                Logger.SwallowException(ex, "TcpClient.cs: The socket was disconnected");
        }

        /// <summary>
        /// Dumps payloads from the send queue when the send queue grows too large.
        /// </summary>
        private void DumpPayloads()
        {
            SendState sendState = m_sendState;

            // Quit if this send loop has been cancelled
            if (sendState is null || sendState.Token.Cancelled)
                return;

            // Check to see if the client has reached the maximum send queue size.
            if (MaxSendQueueSize <= 0 || sendState.SendQueue.Count < MaxSendQueueSize)
                return;

            for (int i = 0; i < MaxSendQueueSize; i++)
            {
                if (sendState.Token.Cancelled)
                    return;

                if (sendState.SendQueue.TryDequeue(out TcpClientPayload payload))
                {
                    payload.WaitHandle.Set();
                    payload.WaitHandle.Dispose();
                }
            }

            throw new InvalidOperationException($"TCP client reached maximum send queue size. {MaxSendQueueSize} payloads dumped from the queue.");
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(CancellationToken cancellationToken)
        {
            try
            {
                // Cancel all asynchronous loops associated with the cancellation token and notify user
                // of terminated connection if the connection had not previously been terminated
                if (!cancellationToken.Cancel())
                    OnConnectionTerminated();
            }
            catch (ThreadAbortException)
            {
                // This is a normal exception
                throw;
            }
            catch
            {
                // Other exceptions can happen (e.g., NullReferenceException) if thread
                // resumes and the class is disposed middle way through this method
            }
        }

        #endregion
    }
}