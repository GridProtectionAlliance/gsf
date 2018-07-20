//******************************************************************************************************
//  TlsClient.cs - Gbtc
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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using GSF.IO;
using GSF.Net.Security;
using GSF.Threading;

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
        private sealed class ConnectState : IDisposable
        {
            public Socket Socket;
            public NetworkStream NetworkStream;
            public SslStream SslStream;
            public NegotiateStream NegotiateStream;

            public readonly SocketAsyncEventArgs ConnectArgs = new SocketAsyncEventArgs();
            public int ConnectionAttempts;

            public CancellationToken Token = new CancellationToken();
            public ICancellationToken TimeoutToken;

            public void Dispose()
            {
                ConnectArgs.Dispose();
                Socket?.Dispose();
                NetworkStream?.Dispose();
                SslStream?.Dispose();
                NegotiateStream?.Dispose();
            }
        }

        private sealed class ReceiveState : IDisposable
        {
            public Socket Socket;
            public NetworkStream NetworkStream;
            public SslStream SslStream;

            public byte[] Buffer;
            public int Offset;
            public int PayloadLength = -1;

            public CancellationToken Token;

            public void Dispose()
            {
                Dispose(Socket);
                Dispose(NetworkStream);
                Dispose(SslStream);
            }

            private static void Dispose(IDisposable obj)
            {
                if ((object)obj != null)
                    obj.Dispose();
            }
        }

        private sealed class SendState : IDisposable
        {
            public Socket Socket;
            public NetworkStream NetworkStream;
            public SslStream SslStream;

            public ConcurrentQueue<TlsClientPayload> SendQueue = new ConcurrentQueue<TlsClientPayload>();
            public TlsClientPayload Payload;
            public int Sending;

            public CancellationToken Token;

            public void Dispose()
            {
                TlsClientPayload payload;

                Dispose(Socket);
                Dispose(NetworkStream);
                Dispose(SslStream);

                while (SendQueue.TryDequeue(out payload))
                {
                    payload.WaitHandle.Set();
                    payload.WaitHandle.Dispose();
                }
            }

            private static void Dispose(IDisposable obj)
            {
                if ((object)obj != null)
                    obj.Dispose();
            }
        }

        private class TlsClientPayload
        {
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEvent WaitHandle;
        }

        private class CancellationToken
        {
            private int m_cancelled;

            public bool Cancelled
            {
                get
                {
                    return Interlocked.CompareExchange(ref m_cancelled, 0, 0) != 0;
                }
            }

            public bool Cancel()
            {
                return Interlocked.Exchange(ref m_cancelled, 1) != 0;
            }
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="TrustedCertificatesPath"/> property.
        /// </summary>
        public readonly string DefaultTrustedCertificatesPath = FilePath.GetAbsolutePath(@"Certs\Remotes");

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
        private bool m_ignoreInvalidCredentials;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private bool m_noDelay;
        private ShortSynchronizedOperation m_dumpPayloadsOperation;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectWaitHandle;
        private NetworkCredential m_networkCredential;

        private ConnectState m_connectState;
        private ReceiveState m_receiveState;
        private SendState m_sendState;

        private bool m_disposed;

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
        /// <param name="connectString">Connect string of the <see cref="TlsClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public TlsClient(string connectString)
            : base(TransportProtocol.Tcp, connectString)
        {
            m_defaultCertificateChecker = new SimpleCertificateChecker();
            m_localCertificateSelectionCallback = DefaultLocalCertificateSelectionCallback;
            m_clientCertificates = new X509Certificate2Collection();
#if MONO
            // Tls12 is not supported under Mono as of v3.8.0
            m_enabledSslProtocols = SslProtocols.Tls;
#else
            m_enabledSslProtocols = SslProtocols.Tls12;
#endif
            m_checkCertificateRevocation = true;

            m_trustedCertificatesPath = DefaultTrustedCertificatesPath;
            m_payloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_integratedSecurity = DefaultIntegratedSecurity;
            m_ignoreInvalidCredentials = DefaultIgnoreInvalidCredentials;
            m_allowDualStackSocket = DefaultAllowDualStackSocket;
            m_maxSendQueueSize = DefaultMaxSendQueueSize;
            m_noDelay = DefaultNoDelay;
            m_dumpPayloadsOperation = new ShortSynchronizedOperation(DumpPayloads, OnSendDataException);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TlsClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TlsClient"/>.</param>
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
                if ((object)value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(value));

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
        /// Gets or sets a boolean value that determines if small packets are delivered to the remote host without delay.
        /// </summary>
        [Category("Settings"),
        DefaultValue(DefaultNoDelay),
        Description("Determines if small packets are delivered to the remote host without delay.")]
        public bool NoDelay
        {
            get
            {
                return m_noDelay;
            }
            set
            {
                m_noDelay = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TlsClient"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Client
        {
            get
            {
                ConnectState connectState = m_connectState;

                if ((object)connectState != null)
                    return connectState.Socket;

                return null;
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
                return $"{TransportProtocol}://{m_connectData["server"]}".ToLower();
            }
        }

        /// <summary>
        /// Gets or sets network credential that is used when
        /// <see cref="IntegratedSecurity"/> is set to <c>true</c>.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Browsable(false)]
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
        /// <exception cref="SecurityException">Failed to write event log entry for security warning about use of less secure TLS/SSL protocols.</exception>
        [Category("Settings"),
        DefaultValue(SslProtocols.Tls12),
        Description("The set of SSL protocols that are enabled for this client.")]
        public SslProtocols EnabledSslProtocols
        {
            get
            {
                return m_enabledSslProtocols;
            }
            set
            {
                m_enabledSslProtocols = value;

                // As of July 2014, Tls12 is the latest and most secure TLS protocol - all other protocols
                // represent a security risk when used, as such we log a security message when any of the
                // older protocols are being used.
                if (m_enabledSslProtocols != SslProtocols.Tls12)
                {
                    try
                    {
                        string applicationName;

                        // Get application name
                        try
                        {
                            // Attempt to retrieve application name as defined in common security settings - this name
                            // is typically preconfigured as the desired event source for event log entries
                            ConfigurationFile config = ConfigurationFile.Current;
                            CategorizedSettingsElementCollection settings = config.Settings["SecurityProvider"];
                            applicationName = settings["ApplicationName"].Value;
                        }
                        catch
                        {
                            applicationName = null;
                        }

                        // Fall back on running executable name
                        if (string.IsNullOrWhiteSpace(applicationName))
                            applicationName = FilePath.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);

                        string message = $"One or more less secure TLS/SSL protocols \"{m_enabledSslProtocols}\" are being used by an instance of the TlsClient in {applicationName}";
                        EventLog.WriteEntry(applicationName, message, EventLogEntryType.Warning, 1);
                    }
                    catch (Exception ex)
                    {
                        throw new SecurityException($"Failed to write event log entry for security warning about use of less secure TLS/SSL protocols \"{m_enabledSslProtocols}\": {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines whether the certificate revocation list is checked during authentication.
        /// </summary>
        [Category("Settings"),
        DefaultValue(true),
        Description("True if the certificate revocation list is to be checked during authentication, otherwise False.")]
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
        [Category("Settings"),
        DefaultValue(null),
        Description("Path to the certificate used by this client for authentication.")]
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
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [Category("Settings"),
        DefaultValue("Trusted Certificates"),
        Description("Path to the directory containing the trusted remote certificates.")]
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
        [Category("Settings"),
        DefaultValue(SslPolicyErrors.None),
        Description("Set of valid policy errors when validating remote certificates.")]
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
        [Category("Settings"),
        DefaultValue(X509ChainStatusFlags.NoError),
        Description("Set of valid chain flags used when validating remote certificates.")]
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

        /// <summary>
        /// Determines whether the base class should track statistics.
        /// </summary>
        protected override bool TrackStatistics
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="TlsClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
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
                settings["NoDelay", true].Update(m_noDelay);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="TlsClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
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
                settings.Add("NoDelay", m_noDelay, "True to disable Nagle so that small packets are delivered to the remote host without delay, otherwise False.");

                try
                {
                    // Attempt to set desired transport security protocols
                    EnabledSslProtocols = settings["EnabledSslProtocols"].ValueAs(m_enabledSslProtocols);
                }
                catch (SecurityException ex)
                {
                    // Security exception can occur when user forces use of older TLS protocol through configuration but event log warning entry cannot be written
                    OnConnectionException(new SecurityException($"Transport layer security protocols assigned as configured: \"{EnabledSslProtocols}\", however, event log entry for security exception could not be written: {ex.Message}", ex));
                }

                CheckCertificateRevocation = settings["CheckCertificateRevocation"].ValueAs(m_checkCertificateRevocation);
                CertificateFile = settings["CertificateFile"].ValueAs(m_certificateFile);
                TrustedCertificatesPath = settings["TrustedCertificatesPath"].ValueAs(m_trustedCertificatesPath);
                ValidPolicyErrors = settings["ValidPolicyErrors"].ValueAs(ValidPolicyErrors);
                ValidChainFlags = settings["ValidChainFlags"].ValueAs(ValidChainFlags);
                PayloadAware = settings["PayloadAware"].ValueAs(m_payloadAware);
                IntegratedSecurity = settings["IntegratedSecurity"].ValueAs(m_integratedSecurity);
                AllowDualStackSocket = settings["AllowDualStackSocket"].ValueAs(m_allowDualStackSocket);
                MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(m_maxSendQueueSize);
                NoDelay = settings["NoDelay"].ValueAs(m_noDelay);
            }

            if (!FilePath.InApplicationPath(TrustedCertificatesPath))
                OnConnectionException(new SecurityException($"Trusted certificates path \"{TrustedCertificatesPath}\" is not in application path"));
        }

        /// <summary>
        /// Connects the <see cref="TlsClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="TlsClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            ConnectState connectState = null;

            Match endpoint;

            // If the client is already connecting or connected, there is nothing to do
            if (CurrentState == ClientState.Disconnected && !m_disposed)
            {
                try
                {
                    // If we do not already have a wait handle to use
                    // for connections, get one from the base class
                    if ((object)m_connectWaitHandle == null)
                        m_connectWaitHandle = (ManualResetEvent)base.ConnectAsync();

                    // Create state object for the asynchronous connection loop
                    connectState = new ConnectState();

                    // Store connectState in m_connectState so that calls to Disconnect
                    // and Dispose can dispose resources and cancel asynchronous loops
                    m_connectState = connectState;

                    // Prepare for connection attempt
                    OnConnectionAttempt();
                    m_connectWaitHandle.Reset();

                    // Overwrite setting from the config file if integrated security exists in connection string
                    if (m_connectData.TryGetValue("integratedSecurity", out string integratedSecuritySetting))
                        m_integratedSecurity = integratedSecuritySetting.ParseBoolean();

#if MONO
                    // Force integrated security to be False under Mono since it's not supported
                    m_integratedSecurity = false;
#endif

                    // Overwrite config file if no delay exists in connection string.
                    if (m_connectData.TryGetValue("noDelay", out string noDelaySetting))
                        m_noDelay = noDelaySetting.ParseBoolean();

                    // Initialize state object for the asynchronous connection loop
                    endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);
                    connectState.ConnectArgs.RemoteEndPoint = Transport.CreateEndPoint(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value), m_ipStack);
                    connectState.ConnectArgs.SocketFlags = SocketFlags.None;
                    connectState.ConnectArgs.UserToken = connectState;
                    connectState.ConnectArgs.Completed += (sender, args) => ProcessConnect((ConnectState)args.UserToken);

                    // Create client socket
                    connectState.Socket = Transport.CreateSocket(m_connectData["interface"], 0, ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);
                    connectState.Socket.NoDelay = m_noDelay;

                    // Initiate the asynchronous connection loop
                    ConnectAsync(connectState);
                }
                catch (Exception ex)
                {
                    // Log exception during connection attempt
                    OnConnectionException(ex);

                    // Terminate the connection
                    if ((object)connectState != null)
                        TerminateConnection(connectState.Token);

                    // Ensure that the wait handle is set so that operations waiting
                    // for completion of the asynchronous connection loop can continue
                    if ((object)m_connectWaitHandle != null)
                        m_connectWaitHandle.Set();
                }
                finally
                {
                    // If the operation was cancelled during execution,
                    // make sure to dispose of erroneously allocated resources
                    if ((object)connectState != null && connectState.Token.Cancelled)
                        connectState.Dispose();
                }
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
            if (!connectState.Token.Cancelled)
            {
                if (!connectState.Socket.ConnectAsync(connectState.ConnectArgs))
                    ThreadPool.QueueUserWorkItem(state => ProcessConnect(connectState));
            }
        }

        private void ProcessConnect(ConnectState connectState)
        {
            Match endpoint;

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

                // Create the SslStream object used to perform
                // send and receive operations on the socket
                connectState.NetworkStream = new NetworkStream(connectState.Socket, true);
                connectState.SslStream = new SslStream(connectState.NetworkStream, false, m_remoteCertificateValidationCallback ?? CertificateChecker.ValidateRemoteCertificate, m_localCertificateSelectionCallback);

                // Load trusted certificates from
                // the trusted certificates directory
                LoadTrustedCertificates();

                // Begin authentication with the TlsServer
                endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

                if (!connectState.Token.Cancelled)
                {
                    connectState.TimeoutToken = new Action(() =>
                    {
                        SocketException ex = new SocketException((int)SocketError.TimedOut);
                        OnConnectionException(ex);
                        TerminateConnection(connectState.Token);
                        connectState.Dispose();
                    }).DelayAndExecute(15000);

                    try
                    {
                        connectState.SslStream.BeginAuthenticateAsClient(endpoint.Groups["host"].Value, m_clientCertificates, m_enabledSslProtocols, m_checkCertificateRevocation, ProcessTlsAuthentication, connectState);
                    }
                    catch
                    {
                        connectState.TimeoutToken.Cancel();
                        throw;
                    }
                }
            }
            catch (SocketException ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // If the connection is refused by the server,
                // keep trying until we reach our maximum connection attempts
                if (ex.SocketErrorCode == SocketError.ConnectionRefused &&
                    (MaxConnectionAttempts == -1 || connectState.ConnectionAttempts < MaxConnectionAttempts))
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
                OnConnectionException(ex);

                // Terminate the connection
                TerminateConnection(connectState.Token);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of erroneously allocated resources
                if (connectState.Token.Cancelled)
                    connectState.Dispose();
            }
        }

        /// <summary>
        /// Callback method for asynchronous authenticate operation.
        /// </summary>
        private void ProcessTlsAuthentication(IAsyncResult asyncResult)
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

                // Complete the operation to authenticate with the server
                connectState.SslStream.EndAuthenticateAsClient(asyncResult);

                // Ensure that this client is authenticated and encrypted
                if (EnabledSslProtocols != SslProtocols.None)
                {
                    if (!connectState.SslStream.IsAuthenticated)
                        throw new InvalidOperationException("Connection could not be established because we could not authenticate with the server.");

                    if (!connectState.SslStream.IsEncrypted)
                        throw new InvalidOperationException("Connection could not be established because the data stream is not encrypted.");
                }

                if (m_integratedSecurity)
                {
#if !MONO
                    // Check the state of cancellation one more time before
                    // proceeding to the next step of the connection loop
                    if (connectState.Token.Cancelled)
                        return;

                    // Create the NegotiateStream to begin authentication of the user's Windows credentials
                    connectState.NegotiateStream = new NegotiateStream(connectState.SslStream, true);

                    connectState.TimeoutToken = new Action(() =>
                    {
                        SocketException ex = new SocketException((int)SocketError.TimedOut);
                        OnConnectionException(ex);
                        TerminateConnection(connectState.Token);
                        connectState.Dispose();
                    }).DelayAndExecute(15000);

                    try
                    {
                        connectState.NegotiateStream.BeginAuthenticateAsClient(m_networkCredential ?? (NetworkCredential)CredentialCache.DefaultCredentials, string.Empty, ProcessIntegratedSecurityAuthentication, connectState);
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
                    // Initialize state object for the asynchronous send loop
                    sendState = new SendState();

                    sendState.Socket = connectState.Socket;
                    sendState.NetworkStream = connectState.NetworkStream;
                    sendState.SslStream = connectState.SslStream;
                    sendState.Token = connectState.Token;

                    // Store sendState in m_sendState so that calls to Disconnect
                    // and Dispose can dispose resources and cancel asynchronous loops
                    m_sendState = sendState;

                    // Check the state of cancellation one more time before
                    // proceeding to the next step of the connection loop
                    if (connectState.Token.Cancelled)
                        return;

                    // Notify of established connection
                    m_connectWaitHandle.Set();
                    OnConnectionEstablished();

                    // Initialize state object for the asynchronous receive loop
                    receiveState = new ReceiveState();

                    receiveState.Socket = connectState.Socket;
                    receiveState.NetworkStream = connectState.NetworkStream;
                    receiveState.SslStream = connectState.SslStream;
                    receiveState.Buffer = new byte[m_payloadMarker.Length + Payload.LengthSegment];
                    receiveState.Token = connectState.Token;

                    // Store receiveState in m_receiveState so that calls to Disconnect
                    // and Dispose can dispose resources and cancel asynchronous loops
                    m_receiveState = receiveState;

                    // Start receiving data
                    if (m_payloadAware)
                        ReceivePayloadAwareAsync(receiveState);
                    else
                        ReceivePayloadUnawareAsync(receiveState);

                    // Further socket interactions are handled through the SslStream
                    // object, so the SocketAsyncEventArgs is no longer needed
                    connectState.ConnectArgs.Dispose();
                }
            }
            catch (SocketException ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // If connectState is null, we cannot proceed
                if ((object)connectState == null)
                    return;

                // If the connection is refused by the server,
                // keep trying until we reach our maximum connection attempts
                if (ex.SocketErrorCode == SocketError.ConnectionRefused &&
                    (MaxConnectionAttempts == -1 || connectState.ConnectionAttempts < MaxConnectionAttempts))
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
                string errorMessage = $"Unable to authenticate connection to server: {CertificateChecker.ReasonForFailure ?? ex.Message}";
                OnConnectionException(new Exception(errorMessage, ex));

                // Terminate the connection
                if ((object)connectState != null)
                    TerminateConnection(connectState.Token);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of erroneously allocated resources
                if ((object)connectState != null && connectState.Token.Cancelled)
                    connectState.Dispose();

                if ((object)receiveState != null && receiveState.Token.Cancelled)
                    receiveState.Dispose();

                if ((object)sendState != null && sendState.Token.Cancelled)
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
                    if (!m_ignoreInvalidCredentials)
                        throw;
                }

                // Initialize state object for the asynchronous send loop
                sendState = new SendState();

                sendState.Socket = connectState.Socket;
                sendState.NetworkStream = connectState.NetworkStream;
                sendState.SslStream = connectState.SslStream;
                sendState.Token = connectState.Token;

                // Store sendState in m_sendState so that calls to Disconnect
                // and Dispose can dispose resources and cancel asynchronous loops
                m_sendState = sendState;

                // Check the state of cancellation one more time before
                // proceeding to the next step of the connection loop
                if (connectState.Token.Cancelled)
                    return;

                // Notify of established connection
                // and begin receiving data.
                m_connectWaitHandle.Set();
                OnConnectionEstablished();

                // Initialize state object for the asynchronous receive loop
                receiveState = new ReceiveState();
                receiveState.Token = connectState.Token;

                receiveState.Socket = connectState.Socket;
                receiveState.NetworkStream = connectState.NetworkStream;
                receiveState.SslStream = connectState.SslStream;
                receiveState.Buffer = new byte[ReceiveBufferSize];

                // Store receiveState in m_receiveState so that calls to Disconnect
                // and Dispose can dispose resources and cancel asynchronous loops
                m_receiveState = receiveState;

                // Start receiving data
                if (m_payloadAware)
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
                if ((object)connectState == null)
                    return;

                // If the connection is refused by the server,
                // keep trying until we reach our maximum connection attempts
                if (ex.SocketErrorCode == SocketError.ConnectionRefused &&
                    (MaxConnectionAttempts == -1 || connectState.ConnectionAttempts < MaxConnectionAttempts))
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
                string errorMessage = $"Unable to authenticate connection to server: {CertificateChecker.ReasonForFailure ?? ex.Message}";
                OnConnectionException(new Exception(errorMessage, ex));

                // Terminate the connection
                if ((object)connectState != null)
                    TerminateConnection(connectState.Token);
            }
            finally
            {
                if ((object)connectState != null)
                {
                    // If the operation was cancelled during execution,
                    // make sure to dispose of erroneously allocated resources;
                    // otherwise, dispose of the NegotiateStream which is only used for authentication
                    if (connectState.Token.Cancelled)
                        connectState.Dispose();
                    else
                        connectState.NegotiateStream.Dispose();
                }

                if ((object)receiveState != null && receiveState.Token.Cancelled)
                    receiveState.Dispose();

                if ((object)sendState != null && sendState.Token.Cancelled)
                    sendState.Dispose();
            }
        }
#endif

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(ReceiveState receiveState)
        {
            int length;

            if (!receiveState.Token.Cancelled)
            {
                if (receiveState.PayloadLength < 0)
                    length = m_payloadMarker.Length + Payload.LengthSegment;
                else
                    length = receiveState.PayloadLength;

                receiveState.SslStream.BeginRead(receiveState.Buffer,
                    receiveState.Offset,
                    length - receiveState.Offset,
                    ProcessReceivePayloadAware,
                    receiveState);
            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(IAsyncResult asyncResult)
        {
            ReceiveState receiveState = null;
            int bytesReceived;

            try
            {
                // Get the receive state from the async result
                receiveState = (ReceiveState)asyncResult.AsyncState;

                // Quit if this receive loop has been cancelled
                if (receiveState.Token.Cancelled)
                    return;

                // Determine if the server disconnected gracefully
                if (!receiveState.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update statistics and bytes received
                bytesReceived = receiveState.SslStream.EndRead(asyncResult);
                UpdateBytesReceived(bytesReceived);
                receiveState.Offset += bytesReceived;

                // Sanity check to determine if the server disconnected gracefully
                if (bytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (receiveState.PayloadLength < 0)
                {
                    // If we haven't parsed the length of the payload yet, attempt to parse it
                    receiveState.PayloadLength = Payload.ExtractLength(receiveState.Buffer, receiveState.Offset, m_payloadMarker);

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
                // Make sure connection is terminated when client is disposed
                if ((object)receiveState != null)
                    TerminateConnection(receiveState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during receive operation
                OnReceiveDataException(ex);

                // Terminate connection when socket exception is encountered
                if ((object)receiveState != null)
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
                    if ((object)receiveState != null)
                        TerminateConnection(receiveState.Token);
                }
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if ((object)receiveState != null && receiveState.Token.Cancelled)
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
                receiveState.SslStream.BeginRead(receiveState.Buffer,
                    0,
                    receiveState.Buffer.Length,
                    ProcessReceivePayloadUnaware,
                    receiveState);
            }
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(IAsyncResult asyncResult)
        {
            ReceiveState receiveState = null;
            int bytesReceived;

            try
            {
                // Get the receive state from the async result
                receiveState = (ReceiveState)asyncResult.AsyncState;

                // Quit if this receive loop has been cancelled
                if (receiveState.Token.Cancelled)
                    return;

                // Determine if the server disconnected gracefully
                if (!receiveState.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Update bytes received
                bytesReceived = receiveState.SslStream.EndRead(asyncResult);
                UpdateBytesReceived(bytesReceived);
                receiveState.PayloadLength = bytesReceived;

                // Sanity check to determine if the server disconnected gracefully
                if (bytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Notify of received data and resume the asynchronous loop
                OnReceiveDataComplete(receiveState.Buffer, bytesReceived);
                ReceivePayloadUnawareAsync(receiveState);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed
                if ((object)receiveState != null)
                    TerminateConnection(receiveState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during receive operation
                OnReceiveDataException(ex);

                // Terminate connection when socket exception is encountered
                if ((object)receiveState != null)
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
                    if ((object)receiveState != null)
                        TerminateConnection(receiveState.Token);
                }
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of allocated resources
                if ((object)receiveState != null && receiveState.Token.Cancelled)
                    receiveState.Dispose();
            }
        }

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
            ReceiveState receiveState = m_receiveState;

            if ((object)receiveState == null || receiveState.Token.Cancelled)
                return 0;

            buffer.ValidateParameters(startIndex, length);

            if ((object)receiveState.Buffer != null)
            {
                int sourceLength = receiveState.PayloadLength - ReadIndex;
                int readBytes = length > sourceLength ? sourceLength : length;
                Buffer.BlockCopy(receiveState.Buffer, ReadIndex, buffer, startIndex, readBytes);

                // Update read index for next call
                ReadIndex += readBytes;

                if (ReadIndex >= receiveState.PayloadLength)
                    ReadIndex = 0;

                return readBytes;
            }

            throw new InvalidOperationException("No received data buffer has been defined to read.");
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
            SendState sendState = null;

            TlsClientPayload payload;
            ManualResetEvent handle;

            try
            {
                // Get the current send state
                sendState = m_sendState;

                // Quit if the send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return null;

                // Prepare for payload-aware transmission
                if (m_payloadAware)
                    Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

                // Create payload and wait handle.
                payload = FastObjectFactory<TlsClientPayload>.CreateObjectFunction();
                handle = new ManualResetEvent(false);

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
                if ((object)sendState != null && sendState.Token.Cancelled)
                    sendState.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Sends a payload on the socket.
        /// </summary>
        private void SendPayloadAsync(SendState sendState)
        {
            TlsClientPayload payload;

            byte[] data;
            int offset;
            int length;

            try
            {
                // Quit if this send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return;

                if (sendState.SendQueue.TryDequeue(out payload))
                {
                    // Save the payload currently
                    // being sent to the send state
                    sendState.Payload = payload;

                    data = payload.Data;
                    offset = payload.Offset;
                    length = payload.Length;

                    // Send payload to the client asynchronously.
                    sendState.SslStream.BeginWrite(data, offset, length, ProcessSend, sendState);
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
        private void ProcessSend(IAsyncResult asyncResult)
        {
            SendState sendState = null;
            TlsClientPayload payload;
            ManualResetEvent handle = null;

            try
            {
                // Get the send state from the async result
                sendState = (SendState)asyncResult.AsyncState;

                // Get the current payload and its wait handle
                payload = sendState.Payload;
                handle = payload.WaitHandle;

                // Quit if this send loop has been cancelled
                if (sendState.Token.Cancelled)
                    return;

                // Determine if the server disconnected gracefully
                if (!sendState.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Complete the send operation
                sendState.SslStream.EndWrite(asyncResult);

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

                // Notify that the send operation is complete
                UpdateBytesSent(payload.Length);
                OnSendDataComplete();
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when client is disposed
                if ((object)sendState != null)
                    TerminateConnection(sendState.Token);
            }
            catch (SocketException ex)
            {
                // Log exception during send operation
                OnSendDataException(ex);

                // Terminate connection when socket exception is encountered
                if ((object)sendState != null)
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
                if ((object)sendState != null && sendState.Token.Cancelled)
                    sendState.Dispose();

                try
                {
                    // Make sure to set the wait handle
                    // even if an exception occurs
                    if ((object)handle != null)
                        handle.Set();
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
        /// When overridden in a derived class, disconnects client from the server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            ConnectState connectState;
            ReceiveState receiveState;
            SendState sendState;

            try
            {
                if (CurrentState != ClientState.Disconnected)
                {
                    connectState = m_connectState;
                    receiveState = m_receiveState;
                    sendState = m_sendState;

                    if ((object)connectState != null)
                    {
                        TerminateConnection(connectState.Token);
                        connectState.Socket.Disconnect(false);
                        connectState.Dispose();
                    }

                    if ((object)receiveState != null)
                        receiveState.Dispose();

                    if ((object)sendState != null)
                        sendState.Dispose();

                    if ((object)m_connectWaitHandle != null)
                        m_connectWaitHandle.Set();
                }
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
        /// Releases the unmanaged resources used by the <see cref="TlsClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_connectState != null)
                        {
                            TerminateConnection(m_connectState.Token);
                            m_connectState.Dispose();
                            m_connectState = null;
                        }

                        if ((object)m_receiveState != null)
                        {
                            m_receiveState.Dispose();
                            m_receiveState = null;
                        }

                        if ((object)m_sendState != null)
                        {
                            m_sendState.Dispose();
                            m_sendState = null;
                        }

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
                throw new ArgumentException($"Server property is missing (Example: {DefaultConnectionString})");

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888
            // Old Format: Server=localhost; Port=8888
            if (m_connectData.ContainsKey("port"))
                m_connectData["server"] = $"{m_connectData["server"]}:{m_connectData["port"]}";

            // Check if 'server' property is valid.
            Match endpoint = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

            if (endpoint == Match.Empty)
                throw new FormatException($"Server property is invalid (Example: {DefaultConnectionString})");

            if (!Transport.IsPortNumberValid(endpoint.Groups["port"].Value))
                throw new ArgumentOutOfRangeException(nameof(connectionString), $"Server port must between {Transport.PortRangeLow} and {Transport.PortRangeHigh}");
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
        /// Dumps payloads from the send queue when the send queue grows too large.
        /// </summary>
        private void DumpPayloads()
        {
            SendState sendState = m_sendState;
            TlsClientPayload payload;

            // Quit if this send loop has been cancelled
            if ((object)sendState == null || sendState.Token.Cancelled)
                return;

            // Check to see if the client has reached the maximum send queue size.
            if (m_maxSendQueueSize > 0 && sendState.SendQueue.Count >= m_maxSendQueueSize)
            {
                for (int i = 0; i < m_maxSendQueueSize; i++)
                {
                    if (sendState.Token.Cancelled)
                        return;

                    if (sendState.SendQueue.TryDequeue(out payload))
                    {
                        payload.WaitHandle.Set();
                        payload.WaitHandle.Dispose();
                    }
                }

                throw new InvalidOperationException($"UDP client reached maximum send queue size. {m_maxSendQueueSize} payloads dumped from the queue.");
            }
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
