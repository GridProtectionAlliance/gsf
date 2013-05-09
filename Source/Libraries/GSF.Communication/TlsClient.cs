//******************************************************************************************************
//  TlsClient.cs - Gbtc
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
//  07/12/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using GSF.IO;
using GSF.Net.Security;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a TCP-based communication client with SSL authentication and encryption.
    /// </summary>
    /// <seealso cref="TcpClient"/>
    public class TlsClient : ClientBase
    {
        #region [ Members ]

        // Nested Types
        private class TlsClientPayload
        {
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="TrustedCertificatesPath"/> property.
        /// </summary>
        public readonly string DefaultTrustedCertificatesPath = FilePath.GetAbsolutePath("Trusted Certificates");

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        /// <summary>
        /// Specifies the default value for the <see cref="IntegratedSecurity"/> property.
        /// </summary>
        public const bool DefaultIntegratedSecurity = false;

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
        private readonly SimpleCertificateChecker m_defaultCertificateChecker;
        private ICertificateChecker m_certificateChecker;
        private RemoteCertificateValidationCallback m_remoteCertificateValidationCallback;
        private LocalCertificateSelectionCallback m_localCertificateSelectionCallback;
        private readonly X509Certificate2Collection m_clientCertificates;
        private SslProtocols m_enabledSslProtocols;
        private bool m_checkCertificateRevocation;
        private string m_trustedCertificatesPath;
        private string m_certificateFile;
        private X509Certificate m_certificate;

        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private bool m_integratedSecurity;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_connectionAttempts;
        private Socket m_socket;
        private readonly TransportProvider<SslStream> m_sslClient;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectWaitHandle;
        private NetworkCredential m_networkCredential;
        private readonly ConcurrentQueue<TlsClientPayload> m_sendQueue;
        private SpinLock m_sendLock;
        private int m_maxSendQueueSize;
        private int m_sending;
        private bool m_disposed;

        private readonly EventHandler<SocketAsyncEventArgs> m_connectHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsClient"/> class.
        /// </summary>
        public TlsClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="TcpClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public TlsClient(string connectString)
            : base(TransportProtocol.Tcp, connectString)
        {
            m_defaultCertificateChecker = new SimpleCertificateChecker();
            m_localCertificateSelectionCallback = DefaultLocalCertificateSelectionCallback;
            m_clientCertificates = new X509Certificate2Collection();
            m_enabledSslProtocols = SslProtocols.Default;
            m_checkCertificateRevocation = true;

            m_trustedCertificatesPath = DefaultTrustedCertificatesPath;
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_integratedSecurity = DefaultIntegratedSecurity;
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_sslClient = new TransportProvider<SslStream>();
            m_sendQueue = new ConcurrentQueue<TlsClientPayload>();
            m_sendLock = new SpinLock();

            m_connectHandler = (sender, args) => ProcessConnect(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpClient"/>.</param>
        public TlsClient(IContainer container)
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
                if (value == null || value.Length == 0)
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
                m_integratedSecurity = value;
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
                return m_socket;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="TlsClient"/>.
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
        /// Gets or sets the certificate checker used to validate remote certificates.
        /// </summary>
        /// <remarks>
        /// The certificate checker will only be used to validate certificates if
        /// the <see cref="RemoteCertificateValidationCallback"/> is set to null.
        /// </remarks>
        public ICertificateChecker CertificateChecker
        {
            get
            {
                return m_certificateChecker ?? m_defaultCertificateChecker;
            }
            set
            {
                m_certificateChecker = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to verify remote certificates.
        /// </summary>
        /// <remarks>
        /// Setting this property overrides the validation
        /// callback in the <see cref="CertificateChecker"/>.
        /// </remarks>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback
        {
            get
            {
                return m_remoteCertificateValidationCallback;
            }
            set
            {
                m_remoteCertificateValidationCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback used to select a local certificate.
        /// </summary>
        public LocalCertificateSelectionCallback LocalCertificateSelectionCallback
        {
            get
            {
                return m_localCertificateSelectionCallback;
            }
            set
            {
                m_localCertificateSelectionCallback = value;
            }
        }

        /// <summary>
        /// Gets the collection of X509 certificates for this client.
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return m_clientCertificates;
            }
        }

        /// <summary>
        /// Gets or sets a set of flags which determine the enabled <see cref="SslProtocols"/>.
        /// </summary>
        public SslProtocols EnabledSslProtocols
        {
            get
            {
                return m_enabledSslProtocols;
            }
            set
            {
                m_enabledSslProtocols = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines whether the certificate revocation list is checked during authentication.
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return m_checkCertificateRevocation;
            }
            set
            {
                m_checkCertificateRevocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the certificate used for authentication.
        /// </summary>
        public string CertificateFile
        {
            get
            {
                return m_certificateFile;
            }
            set
            {
                m_certificateFile = value;

                if (File.Exists(value))
                    Certificate = new X509Certificate2(value);
            }
        }

        /// <summary>
        /// Gets or sets the local certificate selected by the default <see cref="LocalCertificateSelectionCallback"/>.
        /// </summary>
        public X509Certificate Certificate
        {
            get
            {
                return m_certificate;
            }
            set
            {
                m_certificate = value;
            }
        }

        /// <summary>
        /// Gets or sets the path to the directory containing the trusted certificates.
        /// </summary>
        public string TrustedCertificatesPath
        {
            get
            {
                return m_trustedCertificatesPath;
            }
            set
            {
                m_trustedCertificatesPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the set of valid policy errors when validating remote certificates.
        /// </summary>
        public SslPolicyErrors ValidPolicyErrors
        {
            get
            {
                return m_defaultCertificateChecker.ValidPolicyErrors;
            }
            set
            {
                m_defaultCertificateChecker.ValidPolicyErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets the set of valid chain flags used when validating remote certificates.
        /// </summary>
        public X509ChainStatusFlags ValidChainFlags
        {
            get
            {
                return m_defaultCertificateChecker.ValidChainFlags;
            }
            set
            {
                m_defaultCertificateChecker.ValidChainFlags = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// When overridden in a derived class, reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside this event
        /// will have unexpected results.
        /// </remarks>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if ((object)m_sslClient.ReceiveBuffer != null)
            {
                int sourceLength = m_sslClient.BytesReceived - ReadIndex;
                int readBytes = length > sourceLength ? sourceLength : length;
                Buffer.BlockCopy(m_sslClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

                // Update read index for next call
                ReadIndex += readBytes;

                if (ReadIndex >= m_sslClient.BytesReceived)
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
                settings["EnabledSslProtocols", true].Update(m_enabledSslProtocols);
                settings["CheckCertificateRevocation", true].Update(m_checkCertificateRevocation);
                settings["CertificateFile", true].Update(m_certificateFile);
                settings["TrustedCertificatesPath", true].Update(m_trustedCertificatesPath);
                settings["ValidPolicyErrors", true].Update(ValidPolicyErrors);
                settings["ValidChainFlags", true].Update(ValidChainFlags);
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
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("EnabledSslProtocols", m_enabledSslProtocols, "The set of SSL protocols that are enabled for this client.");
                settings.Add("CheckCertificateRevocation", m_checkCertificateRevocation, "True if the certificate revocation list is to be checked during authentication, otherwise False.");
                settings.Add("CertificateFile", m_certificateFile, "Path to the certificate used by this client for authentication.");
                settings.Add("TrustedCertificatesPath", m_trustedCertificatesPath, "Path to the directory containing the trusted remote certificates.");
                settings.Add("ValidPolicyErrors", ValidPolicyErrors, "Set of valid policy errors when validating remote certificates.");
                settings.Add("ValidChainFlags", ValidChainFlags, "Set of valid chain flags used when validating remote certificates.");
                settings.Add("PayloadAware", m_payloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
                settings.Add("IntegratedSecurity", m_integratedSecurity, "True if the current Windows account credentials are used for authentication, otherwise False.");
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
                EnabledSslProtocols = settings["EnabledSslProtocols"].ValueAs(m_enabledSslProtocols);
                CheckCertificateRevocation = settings["CheckCertificateRevocation"].ValueAs(m_checkCertificateRevocation);
                CertificateFile = settings["CertificateFile"].ValueAs(m_certificateFile);
                TrustedCertificatesPath = settings["TrustedCertificatesPath"].ValueAs(m_trustedCertificatesPath);
                ValidPolicyErrors = settings["ValidPolicyErrors"].ValueAs(ValidPolicyErrors);
                ValidChainFlags = settings["ValidChainFlags"].ValueAs(ValidChainFlags);
                PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                IntegratedSecurity = settings["IntegratedSecurity"].ValueAs(m_integratedSecurity);
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
            }
        }

        /// <summary>
        /// When overridden in a derived class, disconnects client from the server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (CurrentState != ClientState.Disconnected)
                {
                    if ((object)m_socket != null && m_socket.Connected)
                        m_socket.Disconnect(false);

                    if ((object)m_connectWaitHandle != null)
                        m_connectWaitHandle.Set();

                    m_sslClient.Reset();
                }
            }
            catch (Exception ex)
            {
                OnSendDataException(new InvalidOperationException(string.Format("Disconnect exception: {0}", ex.Message), ex));
            }
            finally
            {
                base.Disconnect();
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

            if (CurrentState == ClientState.Disconnected)
            {
                if (m_connectWaitHandle == null)
                    m_connectWaitHandle = (ManualResetEvent)base.ConnectAsync();

                OnConnectionAttempt();
                m_connectWaitHandle.Reset();

                // Overwrite config file if integrated security exists in connection string
                if (m_connectData.TryGetValue("integratedSecurity", out integratedSecuritySetting))
                    m_integratedSecurity = integratedSecuritySetting.ParseBoolean();

                // Create client socket to establish presence
                Socket socket = Transport.CreateSocket(m_connectData["interface"], 0, ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);
                Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

                // Begin asynchronous connect operation and return wait handle for the asynchronous operation
                SocketAsyncEventArgs args = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();

                args.RemoteEndPoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);
                args.SocketFlags = SocketFlags.None;
                args.UserToken = socket;
                args.Completed += m_connectHandler;

                if (!socket.ConnectAsync(args))
                    ThreadPool.QueueUserWorkItem(state => ProcessConnect((SocketAsyncEventArgs)state), args);

                return m_connectWaitHandle;
            }
            else
            {
                throw new InvalidOperationException("Client is currently not disconnected");
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
        /// When overridden in a derived class, validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to be validated.</param>
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
        /// When overridden in a derived class, sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            TlsClientPayload payload;
            TlsClientPayload dequeuedPayload;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

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
                payload = ReusableObjectPool<TlsClientPayload>.Default.TakeObject();
                handle = ReusableObjectPool<ManualResetEventSlim>.Default.TakeObject();

                payload.Data = data;
                payload.Offset = offset;
                payload.Length = length;
                payload.WaitHandle = handle;
                handle.Reset();

                // Queue payload for sending.
                m_sendQueue.Enqueue(payload);

                try
                {
                    m_sendLock.Enter(ref lockTaken);

                    // Send the next queued payload.
                    if (Interlocked.CompareExchange(ref m_sending, 1, 0) == 0)
                    {
                        if (m_sendQueue.TryDequeue(out dequeuedPayload))
                            ThreadPool.QueueUserWorkItem(state => SendPayload((TlsClientPayload)state), dequeuedPayload);
                        else
                            Interlocked.Exchange(ref m_sending, 0);
                    }
                }
                finally
                {
                    if (lockTaken)
                        m_sendLock.Exit();
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
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnReceiveDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ConnectionTerminated"/> event.
        /// </summary>
        protected override void OnConnectionTerminated()
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnConnectionTerminated();
        }

        /// <summary>
        /// Callback method for asynchronous connect operation.
        /// </summary>
        private void ProcessConnect(SocketAsyncEventArgs args)
        {
            try
            {
                Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);
                NetworkStream netStream;

                // Perform post-connect operations.
                m_connectionAttempts++;

                if (args.SocketError != SocketError.Success)
                    throw new SocketException((int)args.SocketError);

                LoadTrustedCertificates();
                m_socket = (Socket)args.UserToken;
                m_socket.ReceiveBufferSize = ReceiveBufferSize;
                netStream = new NetworkStream(m_socket, true);
                m_sslClient.Provider = new SslStream(netStream, false, m_remoteCertificateValidationCallback ?? CertificateChecker.ValidateRemoteCertificate, m_localCertificateSelectionCallback);

                // Authenticate.
                m_sslClient.Provider.BeginAuthenticateAsClient(endpoint.Groups["host"].Value, m_clientCertificates, m_enabledSslProtocols, m_checkCertificateRevocation, ProcessTlsAuthentication, null);
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
                OnConnectionException(ex);
                TerminateConnection();
            }
            finally
            {
                args.Dispose();
            }
        }

        /// <summary>
        /// Callback method for asynchronous authenticate operation.
        /// </summary>
        private void ProcessTlsAuthentication(IAsyncResult asyncResult)
        {
            NegotiateStream negotiateStream;

            try
            {
                // Finish authentication.
                m_sslClient.Provider.EndAuthenticateAsClient(asyncResult);

                if (EnabledSslProtocols != SslProtocols.None)
                {
                    if (!m_sslClient.Provider.IsAuthenticated)
                        throw new InvalidOperationException("Connection could not be established because we could not authenticate with the server.");

                    if (!m_sslClient.Provider.IsEncrypted)
                        throw new InvalidOperationException("Connection could not be established because the data stream is not encrypted.");
                }

                if (m_integratedSecurity)
                {
                    negotiateStream = new NegotiateStream(m_sslClient.Provider, true);
                    negotiateStream.BeginAuthenticateAsClient(m_networkCredential ?? (NetworkCredential)CredentialCache.DefaultCredentials, string.Empty, ProcessIntegratedSecurityAuthentication, negotiateStream);
                }
                else
                {
                    // Notify of established connection
                    // and begin receiving data.
                    m_connectWaitHandle.Set();
                    OnConnectionEstablished();
                    ReceivePayloadAsync();
                }
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
                string errorMessage = string.Format("Unable to authenticate connection to server: {0}", CertificateChecker.ReasonForFailure ?? ex.Message);
                OnConnectionException(new Exception(errorMessage, ex));
                TerminateConnection();
            }
        }

        private void ProcessIntegratedSecurityAuthentication(IAsyncResult asyncResult)
        {
            NegotiateStream negotiateStream = (NegotiateStream)asyncResult.AsyncState;

            try
            {
                // Finish authentication.
                negotiateStream.EndAuthenticateAsClient(asyncResult);

                // Notify of established connection
                // and begin receiving data.
                m_connectWaitHandle.Set();
                OnConnectionEstablished();
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
                string errorMessage = string.Format("Unable to authenticate connection to server: {0}", CertificateChecker.ReasonForFailure ?? ex.Message);
                OnConnectionException(new Exception(errorMessage, ex));
                TerminateConnection();
            }
            finally
            {
                negotiateStream.Dispose();
            }
        }

        /// <summary>
        /// Sends a payload on the socket.
        /// </summary>
        private void SendPayload(TlsClientPayload payload)
        {
            byte[] data;
            int offset;
            int length;

            try
            {
                data = payload.Data;
                offset = payload.Offset;
                length = payload.Length;

                // Send payload to the client asynchronously.
                m_sslClient.Provider.BeginWrite(data, offset, length, ProcessSend, payload);
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
        private void ProcessSend(IAsyncResult asyncResult)
        {
            TlsClientPayload payload = null;
            ManualResetEventSlim handle = null;
            bool lockTaken = false;

            try
            {
                payload = (TlsClientPayload)asyncResult.AsyncState;
                handle = payload.WaitHandle;

                handle.Set();

                // Send operation is complete.
                m_sslClient.Provider.EndWrite(asyncResult);
                m_sslClient.Statistics.UpdateBytesSent(payload.Length);
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
            finally
            {
                try
                {
                    payload.WaitHandle = null;

                    // Return payload and wait handle to their respective object pools.
                    ReusableObjectPool<TlsClientPayload>.Default.ReturnObject(payload);
                    ReusableObjectPool<ManualResetEventSlim>.Default.ReturnObject(handle);

                    // Begin sending next client payload.
                    if (m_sendQueue.TryDequeue(out payload))
                    {
                        ThreadPool.QueueUserWorkItem(state => SendPayload((TlsClientPayload)state), payload);
                    }
                    else
                    {
                        try
                        {
                            m_sendLock.Enter(ref lockTaken);

                            if (m_sendQueue.TryDequeue(out payload))
                                ThreadPool.QueueUserWorkItem(state => SendPayload((TlsClientPayload)state), payload);
                            else
                                Interlocked.Exchange(ref m_sending, 0);
                        }
                        finally
                        {
                            if (lockTaken)
                                m_sendLock.Exit();
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

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync()
        {
            // Initialize bytes received.
            m_sslClient.BytesReceived = 0;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Payload boundaries are to be preserved.
                m_sslClient.SetReceiveBuffer(m_payloadMarker.Length + Payload.LengthSegment);
                ReceivePayloadAwareAsync(true);
            }
            else
            {
                // Payload boundares are not to be preserved.
                m_sslClient.SetReceiveBuffer(ReceiveBufferSize);
                ReceivePayloadUnawareAsync();
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(bool waitingForHeader)
        {
            m_sslClient.Provider.BeginRead(m_sslClient.ReceiveBuffer,
                                           m_sslClient.BytesReceived,
                                           m_sslClient.ReceiveBufferSize - m_sslClient.BytesReceived,
                                           ProcessReceivePayloadAware,
                                           waitingForHeader);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(IAsyncResult asyncResult)
        {
            try
            {
                bool waitingForHeader = (bool)asyncResult.AsyncState;

                // Update statistics and bytes received.
                m_sslClient.Statistics.UpdateBytesReceived(m_sslClient.Provider.EndRead(asyncResult));
                m_sslClient.BytesReceived += m_sslClient.Statistics.LastBytesReceived;

                // Client disconnected gracefully.
                if (!m_socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (m_sslClient.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (waitingForHeader)
                {
                    // We're waiting on the payload length, so we'll check if the received data has this information.
                    int payloadLength = Payload.ExtractLength(m_sslClient.ReceiveBuffer, m_sslClient.BytesReceived, m_payloadMarker);

                    // We have the payload length.
                    // If it is set to zero, there is no payload; wait for another header.
                    // Otherwise we'll create a buffer that's big enough to hold the entire payload.
                    if (payloadLength == 0)
                    {
                        m_sslClient.BytesReceived = 0;
                    }
                    else if (payloadLength != -1)
                    {
                        m_sslClient.BytesReceived = 0;
                        m_sslClient.SetReceiveBuffer(payloadLength);
                        waitingForHeader = false;
                    }

                    ReceivePayloadAwareAsync(waitingForHeader);
                }
                else
                {
                    // We're accumulating the payload in the receive buffer until the entire payload is received.
                    if (m_sslClient.BytesReceived == m_sslClient.ReceiveBufferSize)
                    {
                        // We've received the entire payload.
                        OnReceiveDataComplete(m_sslClient.ReceiveBuffer, m_sslClient.BytesReceived);
                        ReceivePayloadAsync();
                    }
                    else
                    {
                        // We've not yet received the entire payload.
                        ReceivePayloadAwareAsync(false);
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
            m_sslClient.Provider.BeginRead(m_sslClient.ReceiveBuffer,
                                           0,
                                           m_sslClient.ReceiveBufferSize,
                                           ProcessReceivePayloadUnaware,
                                           null);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(IAsyncResult asyncResult)
        {
            try
            {
                // Update statistics and pointers.
                m_sslClient.Statistics.UpdateBytesReceived(m_sslClient.Provider.EndRead(asyncResult));
                m_sslClient.BytesReceived = m_sslClient.Statistics.LastBytesReceived;

                // Client disconnected gracefully.
                if (!m_socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (m_sslClient.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Notify of received data and resume receive operation.
                OnReceiveDataComplete(m_sslClient.ReceiveBuffer, m_sslClient.BytesReceived);
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
            if ((object)m_connectWaitHandle != null)
                m_connectWaitHandle.Set();

            m_sslClient.Reset();
            OnConnectionTerminated();
        }

        /// <summary>
        /// Returns the certificate set by the user.
        /// </summary>
        private X509Certificate DefaultLocalCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return m_certificate;
        }

        /// <summary>
        /// Loads the list of trusted certificates into the default certificate checker.
        /// </summary>
        private void LoadTrustedCertificates()
        {
            string trustedCertificatesPath;

            if ((object)m_remoteCertificateValidationCallback == null && (object)m_certificateChecker == null)
            {
                m_defaultCertificateChecker.TrustedCertificates.Clear();
                trustedCertificatesPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(m_trustedCertificatesPath));

                foreach (string fileName in FilePath.GetFileList(trustedCertificatesPath))
                    m_defaultCertificateChecker.TrustedCertificates.Add(new X509Certificate2(fileName));
            }
        }

        #endregion
    }
}
