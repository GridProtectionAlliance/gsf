//******************************************************************************************************
//  TlsServer.cs - Gbtc
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
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using GSF.Configuration;
using GSF.IO;
using GSF.Net.Security;
using GSF.Threading;

#if MONO
#pragma warning disable 649
#endif

namespace GSF.Communication
{
    /// <summary>
    /// Represents a TCP-based communication server with SSL authentication and encryption.
    /// </summary>
    public class TlsServer : ServerBase
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a socket that has been wrapped
        /// in an <see cref="SslStream"/> for encryption.
        /// </summary>
        public sealed class TlsSocket : IDisposable
        {
            /// <summary>
            /// Gets the <see cref="Socket"/> connected to the remote host.
            /// </summary>
            public Socket Socket;

            /// <summary>
            /// Gets the stream through which data is passed when
            /// sending to or receiving from the remote host.
            /// </summary>
            public SslStream SslStream;

            /// <summary>
            /// Performs application-defined tasks associated with
            /// freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if ((object)Socket != null)
                    Socket.Dispose();

                if ((object)SslStream != null)
                    SslStream.Dispose();
            }
        }

        private class TlsClientInfo
        {
            public TransportProvider<TlsSocket> Client;
            public object SendLock;
            public ConcurrentQueue<TlsServerPayload> SendQueue;
            public ShortSynchronizedOperation DumpPayloadsOperation;
            public int Sending;

            public WindowsPrincipal ClientPrincipal;
        }

        private class TlsServerPayload
        {
            // Per payload state
            public byte[] Data;
            public int Offset;
            public int Length;
            public ManualResetEventSlim WaitHandle;

            // Per client state
            public TlsClientInfo ClientInfo;
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
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "Port=8888";

        // Fields
        private readonly SimpleCertificateChecker m_defaultCertificateChecker;
        private ICertificateChecker m_certificateChecker;
        private RemoteCertificateValidationCallback m_remoteCertificateValidationCallback;
        private LocalCertificateSelectionCallback m_localCertificateSelectionCallback;
        private string m_trustedCertificatesPath;
        private string m_certificateFile;
        private X509Certificate m_certificate;
        private SslProtocols m_enabledSslProtocols;
        private bool m_requireClientCertificate;
        private bool m_checkCertificateRevocation;

        private bool m_payloadAware;
        private byte[] m_payloadMarker;
        private bool m_integratedSecurity;
        private bool m_ignoreInvalidCredentials;
        private IPStack m_ipStack;
        private bool m_allowDualStackSocket;
        private int m_maxSendQueueSize;
        private Socket m_tlsServer;
        private SocketAsyncEventArgs m_acceptArgs;
        private readonly ConcurrentDictionary<Guid, TlsClientInfo> m_clientInfoLookup;
        private Dictionary<string, string> m_configData;

        private readonly EventHandler<SocketAsyncEventArgs> m_acceptHandler;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TlsServer()
            : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="TcpServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public TlsServer(string configString)
            : base(TransportProtocol.Tcp, configString)
        {
            m_defaultCertificateChecker = new SimpleCertificateChecker();
            m_localCertificateSelectionCallback = DefaultLocalCertificateSelectionCallback;
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
            m_clientInfoLookup = new ConcurrentDictionary<Guid, TlsClientInfo>();

            m_acceptHandler = (sender, args) => ProcessAccept(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpServer"/>.</param>
        public TlsServer(IContainer container)
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
        /// Gets or sets a boolean value that indicates whether the client Windows account credentials are used for authentication.
        /// </summary>
        /// <remarks>   
        /// This option is ignored under Mono deployments.
        /// </remarks>
        [Category("Security"),
        DefaultValue(DefaultIntegratedSecurity),
        Description("Indicates whether the client Windows account credentials are used for authentication.")]
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
        /// to authenticate the client when integrated security fails. When this is set
        /// to true, if the client's credentials are invalid, the <see cref="TryGetClientPrincipal"/>
        /// method will return true for that client, but the principal will still be null.
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
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpServer"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Server
        {
            get
            {
                return m_tlsServer;
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
        /// Gets or sets the callback used to validate remote certificates.
        /// </summary>
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
        /// Gets or sets the callback used to select local certificates.
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
        /// Gets or sets the path to the certificate used for authentication.
        /// </summary>
        [Category("Settings"),
        DefaultValue(null),
        Description("Path to the local certificate used by this server for authentication.")]
        public string CertificateFile
        {
            get
            {
                return m_certificateFile;
            }
            set
            {
                if ((object)value == null)
                {
                    m_certificateFile = null;
                    Certificate = null;
                }
                else
                {
                    m_certificateFile = FilePath.GetAbsolutePath(value);

                    if (File.Exists(m_certificateFile))
                        Certificate = new X509Certificate2(m_certificateFile);
                }
            }
        }

        /// <summary>
        /// Gets or sets the certificate used to identify this server.
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
        /// Gets or sets a set of flags which determine the enabled <see cref="SslProtocols"/>.
        /// </summary>
        /// <exception cref="SecurityException">Failed to write event log entry for security warning about use of less secure TLS/SSL protocols.</exception>
        [Category("Settings"),
        DefaultValue(SslProtocols.Tls12),
        Description("The set of SSL protocols that are enabled for this server.")]
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

                        string message = string.Format("One or more less secure TLS/SSL protocols \"{0}\" are being used by an instance of the TlsServer in {1}", m_enabledSslProtocols, applicationName);
                        EventLog.WriteEntry(applicationName, message, EventLogEntryType.Warning, 1);
                    }
                    catch (Exception ex)
                    {
                        throw new SecurityException(string.Format("Failed to write event log entry for security warning about use of less secure TLS/SSL protocols \"{0}\": {1}", m_enabledSslProtocols, ex.Message), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether a client certificate is required during authentication.
        /// </summary>
        [Category("Settings"),
        DefaultValue(false),
        Description("True if the client certificate is required during authentication, otherwise False.")]
        public bool RequireClientCertificate
        {
            get
            {
                return m_requireClientCertificate;
            }
            set
            {
                m_requireClientCertificate = value;
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
        /// Gets the descriptive status of the server.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);
                int count = 0;

                foreach (ConcurrentQueue<TlsServerPayload> sendQueue in m_clientInfoLookup.Values.Select(clientInfo => clientInfo.SendQueue))
                {
                    statusBuilder.AppendFormat("           Queued payloads: {0} for client {1}", sendQueue.Count, ++count);
                    statusBuilder.AppendLine();
                }

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="clientID">ID of the client from which data buffer should be read.</param>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ServerBase.ReceiveClientData"/> event handler. Calling this method
        /// outside this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// No received data buffer has been defined to read -or-
        /// Specified <paramref name="clientID"/> does not exist, cannot read buffer.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(Guid clientID, byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            TlsClientInfo clientInfo;
            TransportProvider<TlsSocket> tlsClient;

            if (m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
            {
                tlsClient = clientInfo.Client;

                if ((object)tlsClient.ReceiveBuffer != null)
                {
                    int readIndex = ReadIndicies[clientID];
                    int sourceLength = tlsClient.BytesReceived - readIndex;
                    int readBytes = length > sourceLength ? sourceLength : length;
                    Buffer.BlockCopy(tlsClient.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

                    // Update read index for next call
                    readIndex += readBytes;

                    if (readIndex >= tlsClient.BytesReceived)
                        readIndex = 0;

                    ReadIndicies[clientID] = readIndex;

                    return readBytes;
                }

                throw new InvalidOperationException("No received data buffer has been defined to read.");
            }

            throw new InvalidOperationException("Specified client ID does not exist, cannot read buffer.");
        }

        /// <summary>
        /// Saves <see cref="TcpServer"/> settings to the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
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
                settings["RequireClientCertificate", true].Update(m_requireClientCertificate);
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
        /// Loads saved <see cref="TcpServer"/> settings from the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("EnabledSslProtocols", m_enabledSslProtocols, "The set of SSL protocols that are enabled for this server.");
                settings.Add("RequireClientCertificate", m_requireClientCertificate, "True if the client certificate is required during authentication, otherwise False.");
                settings.Add("CheckCertificateRevocation", m_checkCertificateRevocation, "True if the certificate revocation list is to be checked during authentication, otherwise False.");
                settings.Add("CertificateFile", m_certificateFile, "Path to the local certificate used by this server for authentication.");
                settings.Add("TrustedCertificatesPath", m_trustedCertificatesPath, "Path to the directory containing the trusted remote certificates.");
                settings.Add("ValidPolicyErrors", ValidPolicyErrors, "Set of valid policy errors when validating remote certificates.");
                settings.Add("ValidChainFlags", ValidChainFlags, "Set of valid chain flags used when validating remote certificates.");
                settings.Add("PayloadAware", m_payloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
                settings.Add("IntegratedSecurity", m_integratedSecurity, "True if the client Windows account credentials are used for authentication, otherwise False.");
                settings.Add("AllowDualStackSocket", m_allowDualStackSocket, "True if dual-mode socket is allowed when IP address is IPv6, otherwise False.");
                settings.Add("MaxSendQueueSize", m_maxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");

                try
                {
                    // Attempt to set desired transport security protocols
                    EnabledSslProtocols = settings["EnabledSslProtocols"].ValueAs(m_enabledSslProtocols);
                }
                catch (SecurityException ex)
                {
                    // Security exception can occur when user forces use of older TLS protocol through configuration but event log warning entry cannot be written
                    OnClientConnectingException(new SecurityException(string.Format("Transport layer security protocols assigned as configured: \"{0}\", however, event log entry for security exception could not be written: {1}", EnabledSslProtocols, ex.Message), ex));
                }

                RequireClientCertificate = settings["RequireClientCertificate"].ValueAs(m_requireClientCertificate);
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

            if (!FilePath.InApplicationPath(TrustedCertificatesPath))
                OnClientConnectingException(new SecurityException(string.Format("Trusted certificates path \"{0}\" is not in application path", TrustedCertificatesPath)));
        }

        /// <summary>
        /// Stops the <see cref="TcpServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            SocketAsyncEventArgs acceptArgs = m_acceptArgs;

            m_acceptArgs = null;

            if (CurrentState == ServerState.Running)
            {
                DisconnectAll();        // Disconnection all clients.
                m_tlsServer.Close();    // Stop accepting new connections.

                // Clean up accept args.
                acceptArgs?.Dispose();

                OnServerStopped();
            }
        }

        /// <summary>
        /// Starts the <see cref="TcpServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="TcpServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState == ServerState.NotRunning)
            {
                string integratedSecuritySetting;

                // Initialize if unitialized.
                if (!Initialized)
                    Initialize();

                // Overwrite config file if integrated security exists in connection string.
                if (m_configData.TryGetValue("integratedSecurity", out integratedSecuritySetting))
                    m_integratedSecurity = integratedSecuritySetting.ParseBoolean();

#if MONO
                // Force integrated security to be False under Mono since it's not supported
                m_integratedSecurity = false;
#endif

                // Bind server socket to local end-point and listen.
                m_tlsServer = Transport.CreateSocket(m_configData["interface"], int.Parse(m_configData["port"]), ProtocolType.Tcp, m_ipStack, m_allowDualStackSocket);
                m_tlsServer.Listen(1);

                // Begin accepting incoming connection asynchronously.
                m_acceptArgs = FastObjectFactory<SocketAsyncEventArgs>.CreateObjectFunction();

                m_acceptArgs.AcceptSocket = null;
                m_acceptArgs.SetBuffer(null, 0, 0);
                m_acceptArgs.SocketFlags = SocketFlags.None;
                m_acceptArgs.Completed += m_acceptHandler;

                if (!m_tlsServer.AcceptAsync(m_acceptArgs))
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept((SocketAsyncEventArgs)state), m_acceptArgs);

                // Notify that the server has been started successfully.
                OnServerStarted();
            }
            else
            {
                throw new InvalidOperationException("Server is currently running");
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            TransportProvider<TlsSocket> tlsClient;

            if (!TryGetClient(clientID, out tlsClient))
                return;

            try
            {
                if ((object)tlsClient.Provider != null && tlsClient.Provider.Socket.Connected)
                    tlsClient.Provider.Socket.Disconnect(false);

                OnClientDisconnected(clientID);
                tlsClient.Reset();
            }
            catch (Exception ex)
            {
                OnSendClientDataException(clientID, new InvalidOperationException(string.Format("Client disconnection exception: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{TlsSocket}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="tlsClient">The TLS client.</param>
        /// <returns>An <see cref="TransportProvider{TlsSocket}"/> object.</returns>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public bool TryGetClient(Guid clientID, out TransportProvider<TlsSocket> tlsClient)
        {
            TlsClientInfo clientInfo;
            bool clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                tlsClient = clientInfo.Client;
            else
                tlsClient = null;

            return clientExists;
        }

        /// <summary>
        /// Gets the <see cref="WindowsPrincipal"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="clientPrincipal">The principal of the client.</param>
        /// <returns>A <see cref="WindowsPrincipal"/> object.</returns>
        public bool TryGetClientPrincipal(Guid clientID, out WindowsPrincipal clientPrincipal)
        {
            TlsClientInfo clientInfo;
            bool clientExists = m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

            if (clientExists)
                clientPrincipal = clientInfo.ClientPrincipal;
            else
                clientPrincipal = null;

            return clientExists;
        }

        /// <summary>
        /// Validates the specified <paramref name="configurationString"/>.
        /// </summary>
        /// <param name="configurationString">Configuration string to be validated.</param>
        /// <exception cref="ArgumentException">Port property is missing.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Port property value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConfigurationString(string configurationString)
        {
            m_configData = configurationString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_configData);

            if (!m_configData.ContainsKey("port"))
                throw new ArgumentException(string.Format("Port property is missing (Example: {0})", DefaultConfigurationString));

            if (!Transport.IsPortNumberValid(m_configData["port"]))
                throw new ArgumentOutOfRangeException(nameof(configurationString), string.Format("Port number must be between {0} and {1}", Transport.PortRangeLow, Transport.PortRangeHigh));
        }

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataToAsync(Guid clientID, byte[] data, int offset, int length)
        {
            TlsClientInfo clientInfo;
            ConcurrentQueue<TlsServerPayload> sendQueue;
            TlsServerPayload dequeuedPayload;

            TlsServerPayload payload;
            ManualResetEventSlim handle;

            if (!m_clientInfoLookup.TryGetValue(clientID, out clientInfo))
                throw new InvalidOperationException(string.Format("No client found for ID {0}.", clientID));

            sendQueue = clientInfo.SendQueue;

            // Execute operation to see if the client has reached the maximum send queue size.
            clientInfo.DumpPayloadsOperation.TryRun();

            // Prepare for payload-aware transmission.
            if (m_payloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker);

            // Create payload and wait handle.
            payload = FastObjectFactory<TlsServerPayload>.CreateObjectFunction();
            handle = FastObjectFactory<ManualResetEventSlim>.CreateObjectFunction();

            payload.Data = data;
            payload.Offset = offset;
            payload.Length = length;
            payload.WaitHandle = handle;
            payload.ClientInfo = clientInfo;
            handle.Reset();

            // Queue payload for sending.
            sendQueue.Enqueue(payload);

            lock (clientInfo.SendLock)
            {
                // Send next queued payload.
                if (Interlocked.CompareExchange(ref clientInfo.Sending, 1, 0) == 0)
                {
                    if (sendQueue.TryDequeue(out dequeuedPayload))
                        ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), dequeuedPayload);
                    else
                        Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }

            // Notify that the send operation has started.
            OnSendClientDataStart(clientID);

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle.WaitHandle;
        }

        /// <summary>
        /// Callback method for asynchronous accept operation.
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs acceptArgs)
        {
            TransportProvider<TlsSocket> client = new TransportProvider<TlsSocket>();
            IPEndPoint remoteEndPoint = null;
            NetworkStream netStream;

            try
            {
                if (CurrentState == ServerState.NotRunning)
                    return;

                // If acceptArgs was disposed, m_acceptArgs will either
                // be null or another instance of SocketAsyncEventArgs.
                // This check will tell us whether it's been disposed.
                if ((object)acceptArgs != m_acceptArgs)
                    return;

                if (acceptArgs.SocketError != SocketError.Success)
                {
                    // Error is unrecoverable.
                    // We need to make sure to restart the
                    // server before we throw the error.
                    SocketError error = acceptArgs.SocketError;
                    ThreadPool.QueueUserWorkItem(state => ReStart());
                    throw new SocketException((int)error);
                }

                // Get the remote end point in case we need to display useful error messages
                remoteEndPoint = acceptArgs.AcceptSocket.RemoteEndPoint as IPEndPoint;

                if (MaxClientConnections != -1 && ClientIDs.Length >= MaxClientConnections)
                {
                    // Reject client connection since limit has been reached.
                    TerminateConnection(client, false);
                }
                else
                {
                    // Process the newly connected client.
                    LoadTrustedCertificates();
                    netStream = new NetworkStream(acceptArgs.AcceptSocket);

                    client.Provider = new TlsSocket
                    {
                        Socket = acceptArgs.AcceptSocket,
                        SslStream = new SslStream(netStream, false, m_remoteCertificateValidationCallback ?? CertificateChecker.ValidateRemoteCertificate, m_localCertificateSelectionCallback)
                    };

                    client.Provider.Socket.ReceiveBufferSize = ReceiveBufferSize;
                    client.Provider.SslStream.BeginAuthenticateAsServer(m_certificate, m_requireClientCertificate, m_enabledSslProtocols, m_checkCertificateRevocation, ProcessTlsAuthentication, client);
                }

                // Return to accepting new connections.
                acceptArgs.AcceptSocket = null;

                if (!m_tlsServer.AcceptAsync(acceptArgs))
                {
                    ThreadPool.QueueUserWorkItem(state => ProcessAccept(acceptArgs));
                }
            }
            catch (ObjectDisposedException)
            {
                // m_acceptArgs may be disposed while in the middle of accepting a connection
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if ((object)remoteEndPoint != null)
                {
                    string clientAddress = remoteEndPoint.Address.ToString();
                    string errorMessage = string.Format("Unable to accept connection to client [{0}]: {1}", clientAddress, ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }

                TerminateConnection(client, false);
            }
        }

        /// <summary>
        /// Callback method for asynchronous authenticate operation.
        /// </summary>
        private void ProcessTlsAuthentication(IAsyncResult asyncResult)
        {
            TransportProvider<TlsSocket> client = (TransportProvider<TlsSocket>)asyncResult.AsyncState;
            IPEndPoint remoteEndPoint = client.Provider.Socket.RemoteEndPoint as IPEndPoint;
            SslStream stream = client.Provider.SslStream;

            try
            {
                stream.EndAuthenticateAsServer(asyncResult);

                if (EnabledSslProtocols != SslProtocols.None)
                {
                    if (!stream.IsAuthenticated)
                        throw new InvalidOperationException("Unable to authenticate.");

                    if (!stream.IsEncrypted)
                        throw new InvalidOperationException("Unable to encrypt data stream.");
                }

                if (m_integratedSecurity)
                {
#if !MONO
                    NegotiateStream negotiateStream = new NegotiateStream(stream, true);
                    negotiateStream.BeginAuthenticateAsServer(ProcessIntegratedSecurityAuthentication, Tuple.Create(client, negotiateStream));
#endif
                }
                else
                {
                    TlsClientInfo clientInfo = new TlsClientInfo
                    {
                        Client = client,
                        SendLock = new object(),
                        SendQueue = new ConcurrentQueue<TlsServerPayload>()
                    };

                    // Create operation to dump send queue payloads when the queue grows too large.
                    clientInfo.DumpPayloadsOperation = new ShortSynchronizedOperation(() =>
                    {
                        TlsServerPayload payload;

                        // Check to see if the client has reached the maximum send queue size.
                        if (m_maxSendQueueSize > 0 && clientInfo.SendQueue.Count >= m_maxSendQueueSize)
                        {
                            for (int i = 0; i < m_maxSendQueueSize; i++)
                            {
                                if (clientInfo.SendQueue.TryDequeue(out payload))
                                {
                                    payload.WaitHandle.Set();
                                    payload.WaitHandle.Dispose();
                                    payload.WaitHandle = null;
                                }
                            }

                            throw new InvalidOperationException(string.Format("Client {0} connected to TCP server reached maximum send queue size. {1} payloads dumped from the queue.", clientInfo.Client.ID, m_maxSendQueueSize));
                        }
                    }, ex => OnSendClientDataException(clientInfo.Client.ID, ex));

                    // We can proceed further with receiving data from the client.
                    m_clientInfoLookup.TryAdd(client.ID, clientInfo);

                    OnClientConnected(client.ID);
                    ReceivePayloadAsync(client);
                }
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if ((object)remoteEndPoint != null)
                {
                    string clientAddress = remoteEndPoint.Address.ToString();
                    string errorMessage = string.Format("Unable to authenticate connection to client [{0}]: {1}", clientAddress, CertificateChecker.ReasonForFailure ?? ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }
                TerminateConnection(client, false);
            }
        }

#if !MONO
        private void ProcessIntegratedSecurityAuthentication(IAsyncResult asyncResult)
        {
            Tuple<TransportProvider<TlsSocket>, NegotiateStream> state = (Tuple<TransportProvider<TlsSocket>, NegotiateStream>)asyncResult.AsyncState;
            TransportProvider<TlsSocket> client = state.Item1;
            IPEndPoint remoteEndPoint = client.Provider.Socket.RemoteEndPoint as IPEndPoint;
            NegotiateStream negotiateStream = state.Item2;
            WindowsPrincipal clientPrincipal = null;

            try
            {
                try
                {
                    negotiateStream.EndAuthenticateAsServer(asyncResult);

                    if (negotiateStream.RemoteIdentity is WindowsIdentity)
                        clientPrincipal = new WindowsPrincipal((WindowsIdentity)negotiateStream.RemoteIdentity);
                }
                catch (InvalidCredentialException)
                {
                    if (!m_ignoreInvalidCredentials)
                        throw;
                }

                TlsClientInfo clientInfo = new TlsClientInfo
                {
                    Client = client,
                    SendLock = new object(),
                    SendQueue = new ConcurrentQueue<TlsServerPayload>(),
                    ClientPrincipal = clientPrincipal
                };

                // Create operation to dump send queue payloads when the queue grows too large.
                clientInfo.DumpPayloadsOperation = new ShortSynchronizedOperation(() =>
                {
                    TlsServerPayload payload;

                    // Check to see if the client has reached the maximum send queue size.
                    if (m_maxSendQueueSize > 0 && clientInfo.SendQueue.Count >= m_maxSendQueueSize)
                    {
                        for (int i = 0; i < m_maxSendQueueSize; i++)
                        {
                            if (clientInfo.SendQueue.TryDequeue(out payload))
                            {
                                payload.WaitHandle.Set();
                                payload.WaitHandle.Dispose();
                                payload.WaitHandle = null;
                            }
                        }

                        throw new InvalidOperationException(string.Format("Client {0} connected to TCP server reached maximum send queue size. {1} payloads dumped from the queue.", clientInfo.Client.ID, m_maxSendQueueSize));
                    }
                }, ex => OnSendClientDataException(clientInfo.Client.ID, ex));

                // We can proceed further with receiving data from the client.
                m_clientInfoLookup.TryAdd(client.ID, clientInfo);

                OnClientConnected(client.ID);
                ReceivePayloadAsync(client);
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if ((object)remoteEndPoint != null)
                {
                    string clientAddress = remoteEndPoint.Address.ToString();
                    string errorMessage = string.Format("Unable to authenticate connection to client [{0}]: {1}", clientAddress, ex.Message);
                    OnClientConnectingException(new Exception(errorMessage, ex));
                }
                TerminateConnection(client, false);
            }
            finally
            {
                negotiateStream.Dispose();
            }
        }
#endif

        /// <summary>
        /// Asynchronous loop sends payloads on the socket.
        /// </summary>
        private void SendPayload(TlsServerPayload payload)
        {
            TlsClientInfo clientInfo = null;
            TransportProvider<TlsSocket> client = null;
            //ManualResetEventSlim handle;
            byte[] data;
            int offset;
            int length;

            try
            {
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                //handle = payload.WaitHandle;
                data = payload.Data;
                offset = payload.Offset;
                length = payload.Length;

                // Send payload to the client asynchronously.
                client.Provider.SslStream.BeginWrite(data, offset, length, ProcessSend, payload);
            }
            catch (Exception ex)
            {
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);

                if ((object)clientInfo != null)
                {
                    // Assume process send was not able
                    // to continue the asynchronous loop.
                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                }
            }
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void ProcessSend(IAsyncResult asyncResult)
        {
            TlsServerPayload payload = null;
            TlsClientInfo clientInfo = null;
            TransportProvider<TlsSocket> client = null;
            ConcurrentQueue<TlsServerPayload> sendQueue = null;
            ManualResetEventSlim handle;

            try
            {
                payload = (TlsServerPayload)asyncResult.AsyncState;
                clientInfo = payload.ClientInfo;
                client = clientInfo.Client;
                sendQueue = clientInfo.SendQueue;
                handle = payload.WaitHandle;

                // Set the wait handle to indicate
                // the send operation is complete.
                handle.Set();

                // Update statistics and notify.
                client.Provider.SslStream.EndWrite(asyncResult);
                client.Statistics.UpdateBytesSent(payload.Length);
                OnSendClientDataComplete(client.ID);
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                if ((object)client != null)
                    OnSendClientDataException(client.ID, ex);
            }
            finally
            {
                if ((object)payload != null && (object)sendQueue != null)
                {
                    try
                    {
                        payload.WaitHandle = null;
                        payload.ClientInfo = null;

                        // Begin sending next client payload.
                        if (sendQueue.TryDequeue(out payload))
                        {
                            ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), payload);
                        }
                        else if ((object)clientInfo != null)
                        {
                            lock (clientInfo.SendLock)
                            {
                                if (sendQueue.TryDequeue(out payload))
                                    ThreadPool.QueueUserWorkItem(state => SendPayload((TlsServerPayload)state), payload);
                                else
                                    Interlocked.Exchange(ref clientInfo.Sending, 0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = string.Format("Exception encountered while attempting to send next payload: {0}", ex.Message);

                        if ((object)client != null)
                            OnSendClientDataException(client.ID, new Exception(errorMessage, ex));

                        if ((object)clientInfo != null)
                            Interlocked.Exchange(ref clientInfo.Sending, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data.
        /// </summary>
        private void ReceivePayloadAsync(TransportProvider<TlsSocket> client)
        {
            // Initialize bytes received.
            client.BytesReceived = 0;

            // Initiate receiving.
            if (m_payloadAware)
            {
                // Payload boundaries are to be preserved.
                client.SetReceiveBuffer(m_payloadMarker.Length + Payload.LengthSegment);
                ReceivePayloadAwareAsync(client, true);
            }
            else
            {
                // Payload boundaries are not to be preserved.
                client.SetReceiveBuffer(ReceiveBufferSize);
                ReceivePayloadUnawareAsync(client);
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ReceivePayloadAwareAsync(TransportProvider<TlsSocket> client, bool waitingForHeader)
        {
            client.Provider.SslStream.BeginRead(client.ReceiveBuffer,
                                                client.BytesReceived,
                                                client.ReceiveBufferSize - client.BytesReceived,
                                                ProcessReceivePayloadAware,
                                                new Tuple<Guid, bool>(client.ID, waitingForHeader));
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-aware" mode.
        /// </summary>
        private void ProcessReceivePayloadAware(IAsyncResult asyncResult)
        {
            Tuple<Guid, bool> asyncState = (Tuple<Guid, bool>)asyncResult.AsyncState;
            bool waitingForHeader = asyncState.Item2;
            TransportProvider<TlsSocket> client;

            if (!TryGetClient(asyncState.Item1, out client))
                return;

            try
            {
                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(client.Provider.SslStream.EndRead(asyncResult));
                client.BytesReceived += client.Statistics.LastBytesReceived;

                if (!client.Provider.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (client.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (waitingForHeader)
                {
                    // We're waiting on the payload length, so we'll check if the received data has this information.
                    int payloadLength = Payload.ExtractLength(client.ReceiveBuffer, client.BytesReceived, m_payloadMarker);

                    // We have the payload length.
                    // If it is set to zero, there is no payload; wait for another header.
                    // Otherwise we'll create a buffer that's big enough to hold the entire payload.
                    if (payloadLength == 0)
                    {
                        client.BytesReceived = 0;
                    }
                    else if (payloadLength != -1)
                    {
                        client.BytesReceived = 0;
                        client.SetReceiveBuffer(payloadLength);
                        waitingForHeader = false;
                    }

                    ReceivePayloadAwareAsync(client, waitingForHeader);
                }
                else
                {
                    // We're accumulating the payload in the receive buffer until the entire payload is received.
                    if (client.BytesReceived == client.ReceiveBufferSize)
                    {
                        // We've received the entire payload.
                        OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                        ReceivePayloadAsync(client);
                    }
                    else
                    {
                        // We've not yet received the entire payload.
                        ReceivePayloadAwareAsync(client, false);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, true);
                }
            }
        }

        /// <summary>
        /// Initiate method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ReceivePayloadUnawareAsync(TransportProvider<TlsSocket> client)
        {
            client.Provider.SslStream.BeginRead(client.ReceiveBuffer,
                                                0,
                                                client.ReceiveBufferSize,
                                                ProcessReceivePayloadUnaware,
                                                client);
        }

        /// <summary>
        /// Callback method for asynchronous receive operation of payload data in "payload-unaware" mode.
        /// </summary>
        private void ProcessReceivePayloadUnaware(IAsyncResult asyncResult)
        {
            TransportProvider<TlsSocket> client = (TransportProvider<TlsSocket>)asyncResult.AsyncState;

            try
            {
                // Update statistics and pointers.
                client.Statistics.UpdateBytesReceived(client.Provider.SslStream.EndRead(asyncResult));
                client.BytesReceived = client.Statistics.LastBytesReceived;

                if (!client.Provider.Socket.Connected)
                    throw new SocketException((int)SocketError.Disconnecting);

                if (client.Statistics.LastBytesReceived == 0)
                    throw new SocketException((int)SocketError.Disconnecting);

                // Notify of received data and resume receive operation.
                OnReceiveClientDataComplete(client.ID, client.ReceiveBuffer, client.BytesReceived);
                ReceivePayloadUnawareAsync(client);
            }
            catch (ObjectDisposedException)
            {
                // Make sure connection is terminated when server is disposed.
                TerminateConnection(client, true);
            }
            catch (SocketException ex)
            {
                // Terminate connection when socket exception is encountered.
                OnReceiveClientDataException(client.ID, ex);
                TerminateConnection(client, true);
            }
            catch (Exception ex)
            {
                try
                {
                    // For any other exception, notify and resume receive.
                    OnReceiveClientDataException(client.ID, ex);
                    ReceivePayloadAsync(client);
                }
                catch
                {
                    // Terminate connection if resuming receiving fails.
                    TerminateConnection(client, true);
                }
            }
        }

        /// <summary>
        /// Processes the termination of client.
        /// </summary>
        private void TerminateConnection(TransportProvider<TlsSocket> client, bool raiseEvent)
        {
            TlsClientInfo clientInfo;

            client.Reset();

            if (raiseEvent)
                OnClientDisconnected(client.ID);

            m_clientInfoLookup.TryRemove(client.ID, out clientInfo);
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

                if (Directory.Exists(trustedCertificatesPath))
                {
                    foreach (string fileName in FilePath.GetFileList(trustedCertificatesPath))
                        m_defaultCertificateChecker.TrustedCertificates.Add(new X509Certificate2(fileName));
                }
            }
        }

        #endregion
    }
}
