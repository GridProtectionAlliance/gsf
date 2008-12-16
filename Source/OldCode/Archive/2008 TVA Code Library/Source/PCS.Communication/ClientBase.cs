//*******************************************************************************************************
//  ClientBase.cs
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
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed client data access
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//  02/19/2008 - Pinal C. Patel
//       Added code to detect and avoid redundant calls to Dispose().
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using PCS.Configuration;
using PCS.IO.Compression;
using PCS.Security.Cryptography;
using System.Collections.Generic;

namespace PCS.Communication
{
    /// <summary>
    /// Represents a client involved in server-client communication.
    /// </summary>
    [ToolboxBitmap(typeof(ClientBase))]
    public abstract class ClientBase : Component, IClient, ISupportInitialize, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="MaxConnectionAttempts"/> property.
        /// </summary>
        public const int DefaultMaxConnectionAttempts = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="Handshake"/> property.
        /// </summary>
        public const bool DefaultHandshake = false;

        /// <summary>
        /// Specifies the default value for the <see cref="HandshakeTimeout"/> property.
        /// </summary>
        public const int DefaultHandshakeTimeout = 3000;

        /// <summary>
        /// Specifies the default value for the <see cref="HandshakePassphrase"/> property.
        /// </summary>
        public const string DefaultHandshakePassphrase = "6572a33d-826f-4d96-8c28-8be66bbc700e";

        /// <summary>
        /// Specifies the default value for the <see cref="SecureSession"/> property.
        /// </summary>
        public const bool DefaultSecureSession = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveTimeout"/> property.
        /// </summary>
        public const int DefaultReceiveTimeout = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveBufferSize"/> property.
        /// </summary>
        public const int DefaultReceiveBufferSize = 8192;

        /// <summary>
        /// Specifies the default value for the <see cref="Encryption"/> property.
        /// </summary>
        public const CipherStrength DefaultEncryption = CipherStrength.None;

        /// <summary>
        /// Specifies the default value for the <see cref="Compression"/> property.
        /// </summary>
        public const CompressionStrength DefaultCompression = CompressionStrength.NoCompression;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "CommunicationClient";

        // Events

        /// <summary>
        /// Occurs when client is attempting connection to the server.
        /// </summary>
        [Category("Connection"),
        Description("Occurs when client is attempting connection to the server.")]
        public event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when client connection to the server is established.
        /// </summary>
        [Category("Connection"),
        Description("Occurs when client connection to the server is established.")]
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when client connection to the server is terminated.
        /// </summary>
        [Category("Connection"),
        Description("Occurs when client connection to the server is terminated")]
        public event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to the server.
        /// </summary>
        [Category("Connection"),
        Description("Occurs when an Exception is encountered during connection attempt to the server.")]
        public event EventHandler<EventArgs<Exception>> ConnectionException;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed within the specified <see cref="HandshakeTimeout"/> time.
        /// </summary>
        [Category("Server"),
        Description("Occurs when server-client handshake, when enabled, cannot be performed within the specified HandshakeTimeout time.")]
        public event EventHandler HandshakeProcessTimeout;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.
        /// </summary>
        [Category("Server"),
        Description("Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.")]
        public event EventHandler HandshakeProcessUnsuccessful;

        /// <summary>
        /// Occurs when the client begins sending data to the server.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the client begins sending data to the server.")]
        public event EventHandler SendDataStart;

        /// <summary>
        /// Occurs when the client has successfully sent data to the server.
        /// </summary>
        [Category("Data"),
        Description("Occurs when the client has successfully sent data to the server.")]
        public event EventHandler SendDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending data to the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when sending data to the server.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when an Exception is encountered when sending data to the server.")]
        public event EventHandler<EventArgs<Exception>> SendDataException;

        /// <summary>
        /// Occurs when no data is received from the server for the <see cref="ReceiveTimeout"/> time.
        /// </summary>
        [Category("Data"),
        Description("Occurs when no data is received from the server for the ReceiveTimeout time.")]
        public event EventHandler ReceiveDataTimeout;

        /// <summary>
        /// Occurs when the client receives data from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the buffer containing data received from the server starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of bytes received in the buffer from the server.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when the client receives data from the server.")]
        public event EventHandler<EventArgs<byte[], int>> ReceiveDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when receiving data from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when receiving data from the server.
        /// </remarks>
        [Category("Data"),
        Description("Occurs when an Exception is encountered when receiving data from the server.")]
        public event EventHandler<EventArgs<Exception>> ReceiveDataException;

        // Fields
        private string m_connectionString;
        private int m_maxConnectionAttempts;
        private bool m_handshake;
        private int m_handshakeTimeout;
        private string m_handshakePassphrase;
        private bool m_secureSession;
        private int m_receiveTimeout;
        private int m_receiveBufferSize;
        private CipherStrength m_encryption;
        private CompressionStrength m_compression;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private Encoding m_textEncoding;
        private Action<byte[], int> m_receiveDataHandler;
        private ClientState m_currentState;
        private TransportProtocol m_transportProtocol;
        private Guid m_serverID;
        private Guid m_clientID;
        private long m_connectTime;
        private long m_disconnectTime;
        private bool m_disposed;
        private bool m_initialized;
        //private ManualResetEvent m_connectHandle;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the client.
        /// </summary>
        protected ClientBase()
        {
            m_serverID = Guid.Empty;
            m_clientID = Guid.NewGuid();
            m_textEncoding = Encoding.ASCII;
            m_currentState = ClientState.Disconnected;
            m_maxConnectionAttempts = DefaultMaxConnectionAttempts;
            m_handshake = DefaultHandshake;
            m_handshakeTimeout = DefaultHandshakeTimeout;
            m_handshakePassphrase = DefaultHandshakePassphrase;
            m_secureSession = DefaultSecureSession;
            m_receiveTimeout = DefaultReceiveTimeout;
            m_receiveBufferSize = DefaultReceiveBufferSize;
            m_encryption = DefaultEncryption;
            m_compression = DefaultCompression;
            m_persistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;

        }

        /// <summary>
        /// Initializes a new instance of the client.
        /// </summary>
        /// <param name="transportProtocol">One of the <see cref="TransportProtocol"/> values.</param>
        /// <param name="connectionString">The data used by the client for connection to a server.</param>
        protected ClientBase(TransportProtocol transportProtocol, string connectionString)
            : this()
        {
            m_transportProtocol = transportProtocol;
            this.ConnectionString = connectionString;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the data required by the client to connect to the server.
        /// </summary>
        [Category("Settings"),
        Description("The data required by the client to connect to the server.")]
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                ValidateConnectionString(value);

                m_connectionString = value;
                if (m_currentState == ClientState.Connected)
                {
                    // Reconnect the client when connection data is changed.
                    Disconnect();
                    Connect();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times the client will attempt to connect to the server.
        /// </summary>
        /// <remarks>Set <see cref="MaxConnectionAttempts"/> to -1 for infinite connection attempts.</remarks>
        [Category("Settings"),
        DefaultValue(DefaultMaxConnectionAttempts),
        Description("The maximum number of times the client will attempt to connect to the server. Set MaxConnectionAttempts to -1 for infinite connection attempts.")]
        public virtual int MaxConnectionAttempts
        {
            get
            {
                return m_maxConnectionAttempts;
            }
            set
            {
                if (value < 1)
                    m_maxConnectionAttempts = -1;
                else
                    m_maxConnectionAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client will do a handshake with the server after the connection has been established.
        /// </summary>
        /// <remarks>
        /// <see cref="Handshake"/> is required when <see cref="SecureSession"/> is enabled.
        /// </remarks>
        /// <exception cref="InvalidOperationException"><see cref="Handshake"/> is being disabled while <see cref="SecureSession"/> is enabled.</exception>
        [Category("Security"),
        DefaultValue(DefaultHandshake),
        Description("Indicates whether the client will do a handshake with the server after accepting its connection.")]
        public virtual bool Handshake
        {
            get
            {
                return m_handshake;
            }
            set
            {
                // Can't disable handshake when secure session is enabled.
                if (!value && m_secureSession)
                    throw new InvalidOperationException("Handshake is required when SecureSession is enabled.");

                m_handshake = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds that the client will wait for the server's response to the <see cref="Handshake"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is either zero or negative.</exception>
        [Category("Security"),
        DefaultValue(DefaultHandshakeTimeout),
        Description("The number of milliseconds that the client will wait for the server's response to the Handshake.")]
        public virtual int HandshakeTimeout
        {
            get
            {
                return m_handshakeTimeout;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative.");

                m_handshakeTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the passpharse that the client will provide for authentication during the <see cref="Handshake"/> process.
        /// </summary>
        [Category("Security"),
        DefaultValue(DefaultHandshakePassphrase),
        Description("The passpharse that the client will provide for authentication during the Handshake process.")]
        public virtual string HandshakePassphrase
        {
            get
            {
                return m_handshakePassphrase;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_handshakePassphrase = value;
                else
                    m_handshakePassphrase = DefaultHandshakePassphrase;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data exchanged between the client and server will be encrypted using a private session passphrase.
        /// </summary>
        ///<remarks>
        ///<see cref="Handshake"/> and <see cref="Encryption"/> must be enabled in order to use <see cref="SecureSession"/>.
        ///</remarks>
        ///<exception cref="InvalidOperationException"><see cref="SecureSession"/> is being enabled before enabling <see cref="Handshake"/>.</exception>
        ///<exception cref="InvalidOperationException"><see cref="SecureSession"/> is being enabled before enabling <see cref="Encryption"/>.</exception>
        [Category("Security"),
        DefaultValue(DefaultSecureSession),
        Description("Indicates whether the data exchanged between the client and server will be encrypted using a private session passphrase.")]
        public virtual bool SecureSession
        {
            get
            {
                return m_secureSession;
            }
            set
            {
                // Handshake is required for SecureSession.
                if (value && !m_handshake)
                    throw new InvalidOperationException("Handshake must be enabled in order to use SecureSession.");

                // Encryption is required for SecureSession.
                if (value && m_encryption == CipherStrength.None)
                    throw new InvalidOperationException("Encryption must be enabled in order to use SecureSession.");

                m_secureSession = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds after which the client will raise the <see cref="ReceiveDataTimeout"/> event if no data is received from the server.
        /// </summary>
        /// <remarks>Set <see cref="ReceiveTimeout"/> to -1 to disable this feature.</remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveTimeout),
        Description("The number of milliseconds after which the client will raise the ReceiveClientDataTimedout event if no data is received from the server.")]
        public virtual int ReceiveTimeout
        {
            get
            {
                return m_receiveTimeout;
            }
            set
            {
                if (value < 1)
                    m_receiveTimeout = -1;
                else
                    m_receiveTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is either zero or negative.</exception>
        [Category("Data"),
        DefaultValue(DefaultReceiveBufferSize),
        Description("The size of the buffer used by the client for receiving data from the server.")]
        public virtual int ReceiveBufferSize
        {
            get
            {
                return m_receiveBufferSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative.");

                m_receiveBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CipherStrength"/> to be used for ciphering the data exchanged between the client and server.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        ///     <listheader>
        ///         <term><see cref="SecureSession"/></term>
        ///         <description>Key used for <see cref="Encryption"/></description>
        ///     </listheader>
        ///     <item>
        ///         <term>Disabled</term>
        ///         <description><see cref="HandshakePassphrase"/> is used.</description>
        ///     </item>
        ///     <item>
        ///         <term>Enabled</term>
        ///         <description>A private key exchanged between the client and server during the <see cref="Handshake"/> process is used.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultEncryption),
        Description("The CipherStrength to be used for ciphering the data exchanged between the client and server.")]
        public virtual CipherStrength Encryption
        {
            get
            {
                return m_encryption;
            }
            set
            {
                // Can't disable encryption when secure session is enabled.
                if (value == CipherStrength.None && m_secureSession)
                    throw new InvalidOperationException("Encryption is required when SecureSession is enabled.");

                m_encryption = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CompressionStrength"/> to be used for compressing the data exchanged between the client and server.
        /// </summary>
        [Category("Data"),
        DefaultValue(DefaultCompression),
        Description("The CompressionStrength to be used for compressing the data exchanged between the client and server.")]
        public virtual CompressionStrength Compression
        {
            get
            {
                return m_compression;
            }
            set
            {
                m_compression = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client settings are to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the client settings are to be saved to the config file.")]
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the client settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value specified is a null or empty string.</exception>
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the client settings are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException());

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start connection cycle for the client if it
        /// is not connected, setting to false will disconnect the client if it is connected.
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_currentState == ClientState.Connected;
            }
            set
            {
                if (value && m_currentState == ClientState.Disconnected)
                    Connect();
                else if (!value && m_currentState != ClientState.Disconnected)
                    Disconnect();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                m_textEncoding = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Delegate"/> to be invoked instead of the <see cref="ReceiveDataComplete"/> event when data is received from server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property only needs to be implemented if you need data from the server absolutelty as fast as possible, for most uses this will not be necessary.  
        /// Setting this property gives the consumer access to the data stream as soon as it's available, but this also bypasses <see cref="Encryption"/> and 
        /// <see cref="Compression"/> on received data.
        /// </para>
        /// <para>
        /// arg1 in <see cref="ReceiveDataHandler"/> is the buffer containing the data received from the server.<br/>
        /// arg2 in <see cref="ReceiveDataHandler"/> is the number of bytes received from the server that is stored in the buffer (arg1) starting at index 0.
        /// </para>
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Action<byte[], int> ReceiveDataHandler
        {
            get
            {
                return m_receiveDataHandler;
            }
            set
            {
                m_receiveDataHandler = value;
            }
        }

        /// <summary>
        /// Gets the server ID.
        /// </summary>
        /// <remarks>
        /// <see cref="ServerID"/> will be <see cref="Guid.Empty"/> when <see cref="Handshake"/> is disabled.
        /// </remarks>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Guid ServerID
        {
            get
            {
                return m_serverID;
            }
            set
            {
                m_serverID = value;
            }
        }

        /// <summary>
        /// Gets the client ID.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Guid ClientID
        {
            get
            {
                return m_clientID;
            }
            set
            {
                m_clientID = value;
            }
        }
                
        /// <summary>
        /// Gets the current <see cref="ClientState"/>.
        /// </summary>
        public ClientState CurrentState 
        {
            get
            {
                return m_currentState;
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
        /// </summary>
        [Browsable(false)]
        public virtual TransportProtocol TransportProtocol
        {
            get
            {
                return m_transportProtocol;
            }
        }

        /// <summary>
        /// Gets the total number of seconds for which the client has been connected to the server.
        /// </summary>
        [Browsable(false)]
        public virtual double ConnectionTime
        {
            get
            {
                double clientConnectionTime = 0.0D;

                if (m_connectTime > 0)
                {
                    if (m_currentState == ClientState.Connected) 
                        // Client is connected to the server.
                        clientConnectionTime = Ticks.ToSeconds(DateTime.Now.Ticks - m_connectTime);
                    else 
                        // Client is not connected to the server.
                        clientConnectionTime = Ticks.ToSeconds(m_disconnectTime - m_connectTime);
                }

                return clientConnectionTime;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the client.
        /// </summary>
        [Browsable(false)]
        public virtual string Name
        {
            get
            {
                return m_settingsCategory;
            }
        }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        [Browsable(false)]
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                if (m_handshake)
                {
                    // Display ID only if handshaking is enabled.
                    status.Append("                 Server ID: ");
                    status.Append(m_serverID.ToString());
                    status.AppendLine();
                    status.Append("                 Client ID: ");
                    status.Append(m_clientID.ToString());
                    status.AppendLine();
                }
                status.Append("              Client state: ");
                status.Append(m_currentState);
                status.AppendLine();
                status.Append("           Connection time: ");
                status.Append(Seconds.ToText(ConnectionTime));
                status.AppendLine();
                status.Append("            Receive buffer: ");
                status.Append(m_receiveBufferSize.ToString());
                status.AppendLine();
                status.Append("        Transport protocol: ");
                status.Append(m_transportProtocol.ToString());
                status.AppendLine();
                status.Append("        Text encoding used: ");
                status.Append(m_textEncoding.EncodingName);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, disconnects client from the server synchronously.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// When overridden in a derived class, connects client to the server asynchronously.
        /// </summary>
        public abstract void ConnectAsync();

        /// <summary>
        /// When overridden in a derived class, validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to be validated.</param>
        protected abstract void ValidateConnectionString(string connectionString);

        /// <summary>
        /// When overridden in a derived class, returns the passphrase used for ciphering data.
        /// </summary>
        protected abstract string GetSessionPassphrase();

        /// <summary>
        /// When overridden in a derived class, sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected abstract WaitHandle SendDataAsync(byte[] data, int offset, int length);

        #endregion

        /// <summary>
        /// Initializes the client.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the client is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Connects the client to the server synchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="MaxConnectionAttempts"/> is set to -1.</exception>
        public void Connect()
        {
            // Don't allow blocking connection to be attempted when MaxConnectionAttempts = -1.
            if (MaxConnectionAttempts == -1)
                throw new InvalidOperationException("Synchronous connection cannot be attempted with MaxConnectionAttempts = -1.");

            // Start asynchronous connection attempt.
            ConnectAsync();
            // Block for connection process to complete.
            while (m_currentState != ClientState.Connected)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        public virtual void Send(string data)
        {
            Send(m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        public virtual void Send(object serializableObject)
        {
            Send(Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        public virtual void Send(byte[] data)
        {
            Send(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        public virtual void Send(byte[] data, int offset, int length)
        {
            SendAsync(data, offset, length).WaitOne();
        }

        /// <summary>
        /// Saves client settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings["ConnectionString", true].Update(m_connectionString, "Data required by the client to connect to the server.");
                settings["ReceiveBufferSize", true].Update(m_receiveBufferSize, "Maximum number of bytes that can be received at a time by the client from the server.");
                settings["ReceiveTimeout", true].Update(m_receiveTimeout, "Time to wait in milliseconds for data to be received from the server before timing out.");
                settings["MaximumConnectionAttempts", true].Update(m_maxConnectionAttempts, "Maximum number of times the client will attempt to connect to the server.");
                settings["SecureSession", true].Update(m_secureSession, "True if the data exchanged between the client and server will be encrypted using a private session passphrase; otherwise False.");
                settings["Handshake", true].Update(m_handshake, "True if the client will do a handshake with the server; otherwise False.");
                settings["HandshakePassphrase", true].Update(m_handshakePassphrase, "Passpharse that will provided to the server for authentication during the handshake process.");
                settings["Encryption", true].Update(m_encryption, "Cipher strength (None; Level1; Level2; Level3; Level4; Level5) to be used for encrypting the data exchanged between the client and server.");
                settings["Compression", true].Update(m_compression, "Compression strength (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the client and server.");
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved client settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                ConnectionString = settings["ConnectionString", true].ValueAs(m_connectionString);
                MaxConnectionAttempts = settings["MaximumConnectionAttempts", true].ValueAs(m_maxConnectionAttempts);
                Handshake = settings["Handshake", true].ValueAs(m_handshake);
                HandshakePassphrase = settings["HandshakePassphrase", true].ValueAs(m_handshakePassphrase);
                ReceiveTimeout = settings["ReceiveTimeout", true].ValueAs(m_receiveTimeout);
                ReceiveBufferSize = settings["ReceiveBufferSize", true].ValueAs(m_receiveBufferSize);
                Encryption = settings["Encryption", true].ValueAs(m_encryption);
                Compression = settings["Compression", true].ValueAs(m_compression);
                SecureSession = settings["SecureSession", true].ValueAs(m_secureSession);
            }
        }

        /// <summary>
        /// Performs necessary operations before the client properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use by the designer if the server is consumed through 
        /// the designer surface of the IDE.
        /// </remarks>
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
        }

        /// <summary>
        /// Performs necessary operations after the client properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use by the designer if the server is consumed through the 
        /// designer surface of the IDE.
        /// </remarks>
        public void EndInit()
        {
            if (!DesignMode)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(string data)
        {
            return SendAsync(m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(object serializableObject)
        {
            return SendAsync(Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(byte[] data)
        {
            return SendAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(byte[] data, int offset, int length)
        {
            if (m_currentState == ClientState.Connected)
            {
                // Pre-condition data as needed and then send it.
                Payload.ProcessTransmit(ref data, ref offset, ref length, m_encryption, GetSessionPassphrase(), m_compression);
                return SendDataAsync(data, offset, length);
            }
            else
            {
                throw new InvalidOperationException("Client is not connected.");
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the client and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    SaveSettings();
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        Disconnect();
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
        /// Raises the <see cref="ConnectionAttempt"/> event.
        /// </summary>
        protected virtual void OnConnectionAttempt()
        {
            m_currentState = ClientState.Connecting;

            if (ConnectionAttempt != null)
                ConnectionAttempt(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected virtual void OnConnectionEstablish()
        {
            m_currentState = ClientState.Connected;
            m_disconnectTime = 0;
            m_connectTime = DateTime.Now.Ticks;     // Save the time when the client connected to the server.

            if (ConnectionEstablished != null)
                ConnectionEstablished(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionTerminated"/> event.
        /// </summary>
        protected virtual void OnConnectionTerminate()
        {
            m_currentState = ClientState.Disconnected;
            m_serverID = Guid.Empty;
            m_disconnectTime = DateTime.Now.Ticks;  // Save the time when client was disconnected from the server.

            if (ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ConnectionException"/> event.</param>
        protected virtual void OnConnectionException(Exception ex)
        {
            if (ConnectionException != null)
                ConnectionException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="HandshakeProcessTimeout"/> event.
        /// </summary>
        protected virtual void OnHandshakeProcessTimeout()
        {
            if (HandshakeProcessTimeout != null)
                HandshakeProcessTimeout(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="HandshakeProcessUnsuccessful"/> event.
        /// </summary>
        protected virtual void OnHandshakeProcessUnsuccessful()
        {
            if (HandshakeProcessUnsuccessful != null)
                HandshakeProcessUnsuccessful(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SendDataStart"/> event.
        /// </summary>
        protected virtual void OnSendDataStart()
        {
            if (SendDataStart != null)
                SendDataStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SendDataComplete"/> event.
        /// </summary>
        protected virtual void OnSendDataComplete()
        {
            if (SendDataComplete != null)
                SendDataComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="SendDataException"/> event.</param>
        protected virtual void OnSendDataException(Exception ex)
        {
            if (SendDataException != null)
                SendDataException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataTimeout"/> event.
        /// </summary>
        protected virtual void OnReceiveDataTimeout()
        {
            if (ReceiveDataTimeout != null)
                ReceiveDataTimeout(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataComplete"/> event.
        /// </summary>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        protected virtual void OnReceiveDataComplete(byte[] data, int size)
        {
            if (m_receiveDataHandler != null)
            {
                m_receiveDataHandler(data, size);
            }
            else
            {
                if (ReceiveDataComplete != null)
                {
                    try
                    {
                        int offset = 0; // Received buffer will always have valid data starting at offset zero.
                        Payload.ProcessReceived(ref data, ref offset, ref size, m_encryption, GetSessionPassphrase(), m_compression);
                    }
                    catch
                    {
                        // Ignore encountered exception and pass-on the raw data.
                    }
                    finally
                    {
                        ReceiveDataComplete(this, new EventArgs<byte[], int>(data, size));
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ReceiveDataException"/> event.</param>
        protected virtual void OnReceiveDataException(Exception ex)
        {
            if (ReceiveDataException != null)
                ReceiveDataException(this, new EventArgs<Exception>(ex));
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Create a communications client
        /// </summary>
        /// <remarks>
        /// Note that typical connection string should be prefixed with a "protocol=tcp", "protocol=udp", "protocol=serial" or "protocol=file"
        /// </remarks>
        public static IClient Create(string connectionString)
        {
            Dictionary<string, string> connectionData = connectionString.ParseKeyValuePairs();
            IClient client = null;
            string protocol;

            if (connectionData.TryGetValue("protocol", out protocol))
            {
                connectionData.Remove("protocol");
                StringBuilder settings = new StringBuilder();

                foreach (string key in connectionData.Keys)
                {
                    settings.Append(key);
                    settings.Append("=");
                    settings.Append(connectionData[key]);
                    settings.Append(";");
                }

                switch (protocol.ToLower())
                {
                    case "tcp":
                        client = new TcpClient(settings.ToString());
                        break;
                    case "udp":
                        client = new UdpClient(settings.ToString());
                        break;
                    //case "serial":
                    //    client = new SerialClient(settings.ToString());
                    //    break;
                    //case "file":
                    //    client = new FileClient(settings.ToString());
                    //    break;
                    default:
                        throw new ArgumentException("Transport protocol \'" + protocol + "\' is not valid.");
                }
            }
            else
            {
                throw new ArgumentException("Transport protocol must be specified.");
            }

            return client;
        }

        #endregion
    }
}











//namespace PCS.Communication
//{
//    /// <summary>
//    /// Represents a client involved in the transportation of data.
//    /// </summary>
//    [ToolboxBitmap(typeof(ClientBase)), DefaultEvent("ReceivedData")]
//    public abstract class ClientBase : Component, IClient, IPersistSettings, ISupportInitialize
//    {	
//        #region [ Members ]

//        // Constants

//        /// <summary>
//        /// The maximum number of bytes that can be sent from the client to server in a single send operation.
//        /// </summary>
//        public const int MaximumDataSize = 524288000; // 500 MB

//        /// <summary>
//        /// The key used for encryption and decryption when Encryption is enabled but HandshakePassphrase is not set.
//        /// </summary>
//        protected const string DefaultCryptoKey = "6572a33d-826f-4d96-8c28-8be66bbc700e";

//        // Events

//        /// <summary>
//        /// Occurs when the client is trying to connect to the server.
//        /// </summary>
//        [Description("Occurs when the client is trying to connect to the server."), Category("Connection")]
//        public event EventHandler Connecting;

//        /// <summary>
//        /// Occurs when connecting of the client to the server has been cancelled.
//        /// </summary>
//        [Description("Occurs when connecting of the client to the server has been cancelled."), Category("Connection")]
//        public event EventHandler ConnectingCancelled;

//        /// <summary>
//        /// Occurs when an exception is encountered while connecting to the server.
//        /// </summary>
//        [Description("Occurs when an exception occurs while connecting to the server."), Category("Connection")]
//        public event EventHandler<EventArgs<Exception>> ConnectingException;

//        /// <summary>
//        /// Occurs when the client has successfully connected to the server.
//        /// </summary>
//        [Description("Occurs when the client has successfully connected to the server."), Category("Connection")]
//        public event EventHandler Connected;

//        /// <summary>
//        /// Occurs when the client has disconnected from the server.
//        /// </summary>
//        [Description("Occurs when the client has disconnected from the server."), Category("Connection")]
//        public event EventHandler Disconnected;	

//        /// <summary>
//        /// Occurs when the client begins sending data to the server.
//        /// </summary>
//        [Description("Occurs when the client begins sending data to the server."), Category("Data")]
//        public event EventHandler<EventArgs<IdentifiableItem<Guid, Byte[]>>> SendDataBegin;	

//        /// <summary>
//        /// Occurs when the client has successfully send data to the server.
//        /// </summary>
//        [Description("Occurs when the client has successfully send data to the server."), Category("Data")]
//        public event EventHandler<EventArgs<IdentifiableItem<Guid, Byte[]>>> SendDataComplete;

//        /// <summary>
//        /// Occurs when the client receives data from the server.
//        /// </summary>
//        [Description("Occurs when the client receives data from the server."), Category("Data")]
//        public event EventHandler<EventArgs<IdentifiableItem<Guid, Byte[]>>> ReceivedData;

//        /// <summary>
//        /// Occurs when no data is received from the server after waiting for the specified time.
//        /// </summary>
//        [Description("Occurs when no data is received from the server after waiting for the specified time."), Category("Data")]
//        public event EventHandler ReceiveTimedOut;

//        // Fields
//        private string m_connectionString;
//        private int m_receiveBufferSize;
//        private int m_receiveTimeout;
//        private int m_maximumConnectionAttempts;
//        private Encoding m_textEncoding;
//        private TransportProtocol m_protocol;
//        private bool m_secureSession;
//        private bool m_handshake;
//        private string m_handshakePassphrase;
//        private CipherStrength m_encryption;
//        private CompressionStrength m_compression;
//        //private CRCCheckType m_crcCheck;
//        private bool m_enabled;
//        private Guid m_serverID;
//        private Guid m_clientID;
//        private bool m_isConnected;
//        private long m_totalBytesSent;
//        private bool m_persistSettings;
//        private string m_settingsCategory;
//        private bool m_disposed;		
//        private long m_connectTime;
//        private long m_disconnectTime;
//        private ManualResetEvent m_connectionWaitHandle;
//        private bool m_initialized;

//        // We expose these two members to derived classes for their own internal use
//        protected long m_totalBytesReceived;
//        protected Action<byte[], int, int> m_receiveRawDataFunction;
//        protected byte[] m_buffer;

//        #endregion

//        #region [ Constructors ]

//        protected ClientBase()
//        {
//            // Setup the default values.
//            m_receiveBufferSize = 8192;
//            m_receiveTimeout = - 1;
//            m_maximumConnectionAttempts = - 1;
//            m_textEncoding = Encoding.ASCII;
//            m_handshake = true;
//            m_encryption = CipherStrength.None;
//            m_compression = CompressionStrength.NoCompression;
//            //m_crcCheck = CRCCheckType.None;
//            m_enabled = true;
//            m_clientID = Guid.NewGuid();
//            m_settingsCategory = this.GetType().Name;			
//            m_connectionWaitHandle = new System.Threading.ManualResetEvent(false);
//            m_buffer = new byte[m_receiveBufferSize];
//        }

//        protected ClientBase(string connectionString) : this()
//        {
//            m_connectionString = connectionString;
//        }

//        /// <summary>
//        /// Releases the unmanaged resources before the <see cref="ClientBase"/> object is reclaimed by <see cref="GC"/>.
//        /// </summary>
//        ~ClientBase()
//        {
//            Dispose(false);
//        }


//        #endregion

//        #region [ Properties ]

//        /// <summary>
//        /// Gets or sets the data required by the client to connect to the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The data required by the client to connect to the server.</returns>
//        [Description("The data required by the client to connect to the server."), Category("Configuration")]
//        public virtual string ConnectionString
//        {
//            get
//            {
//                return m_connectionString;
//            }
//            set
//            {
//                if (ValidConnectionString(value))
//                {
//                    m_connectionString = value;
//                    if (IsConnected)
//                    {
//                        // Reconnect the client when connection data is changed.
//                        Disconnect();
//                        Connect();
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the maximum number of times the client will attempt to connect to the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The maximum number of times the client will attempt to connect to the server.</returns>
//        /// <remarks>Set MaximumConnectionAttempts = -1 for infinite connection attempts.</remarks>
//        [Description("The maximum number of times the client will attempt to connect to the server. Set MaximumConnectionAttempts = -1 for infinite connection attempts."), Category("Configuration"), DefaultValue(typeof(int), "-1")]
//        public virtual int MaximumConnectionAttempts
//        {
//            get
//            {
//                return m_maximumConnectionAttempts;
//            }
//            set
//            {
//                if (value == -1 || value > 0)
//                {
//                    m_maximumConnectionAttempts = value;
//                }
//                else
//                {
//                    throw (new ArgumentOutOfRangeException("value"));
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether the data exchanged between the server and clients
//        /// will be encrypted using a private session passphrase.
//        /// </summary>
//        /// <value></value>
//        /// <returns>
//        /// True if the data exchanged between the server and clients will be encrypted using a private session
//        /// passphrase; otherwise False.
//        /// </returns>
//        ///<remarks>Handshake and Encryption must be enabled in order to use SecureSession.</remarks>
//        [Description("Indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase."), Category("Security"), DefaultValue(typeof(bool), "False")]
//        public virtual bool SecureSession
//        {
//            get
//            {
//                return m_secureSession;
//            }
//            set
//            {
//                if (!value || (value && m_handshake && m_encryption != CipherStrength.None))
//                {
//                    m_secureSession = value;
//                }
//                else
//                {
//                    throw new InvalidOperationException("Handshake and Encryption must be enabled in order to use SecureSession.");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value indicating whether the server will do a handshake with the client after
//        /// accepting its connection.
//        /// </summary>
//        /// <value></value>
//        /// <returns>True if the server will do a handshake with the client; otherwise False.</returns>
//        /// <remarks>SecureSession must be disabled before disabling Handshake.</remarks>
//        [Description("Indicates whether the server will do a handshake with the client after accepting its connection."), Category("Security"), DefaultValue(typeof(bool), "True")]
//        public virtual bool Handshake
//        {
//            get
//            {
//                return m_handshake;
//            }
//            set
//            {
//                if (value || (!value && !m_secureSession))
//                {
//                    m_handshake = value;

//                    if (!m_handshake)
//                    {
//                        // Handshake passphrase will have no effect if handshaking is disabled.
//                        m_handshakePassphrase = "";
//                    }
//                }
//                else
//                {
//                    throw new ArgumentException("SecureSession must be disabled before disabling Handshake.");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the passpharse that will be provided to the server for authentication during the handshake
//        /// process.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The passpharse that will provided to the server for authentication during the handshake process.</returns>
//        [Description("The passpharse that will provided to the server for authentication during the handshake process."), Category("Security"), DefaultValue(typeof(string), "")]
//        public virtual string HandshakePassphrase
//        {
//            get
//            {
//                return m_handshakePassphrase;
//            }
//            set
//            {
//                m_handshakePassphrase = value;

//                if (!string.IsNullOrEmpty(m_handshakePassphrase))
//                {
//                    // Handshake password has no effect until handshaking is enabled.
//                    m_handshake = true;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
//        /// </summary>
//        /// <value>Receive buffer size</value>
//        /// <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while client is connected</exception>
//        /// <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
//        /// <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
//        [Description("The maximum number of bytes that can be received at a time by the client from the server."), Category("Data"), DefaultValue(typeof(int), "8192")]
//        public virtual int ReceiveBufferSize
//        {
//            get
//            {
//                return m_receiveBufferSize;
//            }
//            set
//            {
//                if (value > 0)
//                {
//                    if (m_isConnected)
//                        throw new InvalidOperationException("Cannot change receive buffer size while client is connected");

//                    m_receiveBufferSize = value;
//                    m_buffer = new byte[value];
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("value");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the time to wait in milliseconds for data to be received from the server before timing out.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The time to wait in milliseconds for data to be received from the server before timing out.</returns>
//        /// <remarks>Set ReceiveTimeout = -1 to disable timeout for receiving data.</remarks>
//        [Description("The time to wait in milliseconds for data to be received from the server before timing out. Set ReceiveTimeout = -1 to disable timeout for receiving data."), Category("Data"), DefaultValue(typeof(int), "-1")]
//        public virtual int ReceiveTimeout
//        {
//            get
//            {
//                return m_receiveTimeout;
//            }
//            set
//            {
//                if (value == -1 || value > 0)
//                {
//                    m_receiveTimeout = value;
//                }
//                else
//                {
//                    throw new ArgumentOutOfRangeException("value");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the encryption level to be used for encrypting the data exchanged between the client and
//        /// server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The encryption level to be used for encrypting the data exchanged between the client and server.</returns>
//        /// <remarks>
//        /// <para>Set Encryption = None to disable encryption.</para>
//        /// <para>
//        /// The key used for performing cryptography will be different in different senarios.
//        /// 1) HandshakePassphrase will be used as the key when HandshakePassphrase is set and SecureSession is
//        ///    disabled.
//        /// 2) DefaultCryptoKey will be used as the key when HandshakePassphrase is not set and SecureSession is
//        ///    disabled.
//        /// 3) A private session key will be used as the key when SecureSession is enabled.
//        /// </para>
//        /// </remarks>
//        [Description("The encryption level to be used for encrypting the data exchanged between the client and server."), Category("Data"), DefaultValue(typeof(CipherStrength), "None")]
//        public virtual CipherStrength Encryption
//        {
//            get
//            {
//                return m_encryption;
//            }
//            set
//            {
//                if (!m_secureSession || (m_secureSession && value != CipherStrength.None))
//                {
//                    m_encryption = value;
//                }
//                else
//                {
//                    throw new ArgumentException("SecureSession session must be disabled before disabling Encryption.");
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the compression level to be used for compressing the data exchanged between the client and
//        /// server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The compression level to be used for compressing the data exchanged between the client and server.</returns>
//        /// <remarks>Set Compression = NoCompression to disable compression.</remarks>
//        [Description("The compression level to be used for compressing the data exchanged between the client and server."), Category("Data"), DefaultValue(typeof(CompressionStrength), "NoCompression")]
//        public virtual CompressionStrength Compression
//        {
//            get
//            {
//                return m_compression;
//            }
//            set
//            {
//                m_compression = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets a boolean value that indicates whether the <see cref="ClientBase"/> object is currently enabled.
//        /// </summary>
//        /// <returns>True if the client is enabled; otherwise False.</returns>
//        [Description("Indicates whether the client is enabled."), Category("Behavior"), DefaultValue(typeof(bool), "True")]
//        public virtual bool Enabled
//        {
//            get
//            {
//                return m_enabled;
//            }
//            set
//            {
//                m_enabled = value;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the encoding to be used for the text sent to the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The encoding to be used for the text sent to the server.</returns>
//        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public virtual Encoding TextEncoding
//        {
//            get
//            {
//                return m_textEncoding;
//            }
//            set
//            {
//                m_textEncoding = value;
//            }
//        }

//        /// <summary>
//        /// Setting this property allows consumer to "intercept" data before it goes through normal processing
//        /// </summary>
//        /// <remarks>
//        /// This property only needs to be implemented if you need data from the server absolutelty as fast as possible, for most uses this
//        /// will not be necessary.  Setting this property gives the consumer access to the data stream as soon as it's available, but this also
//        /// bypasses all of the advanced convience properties (e.g., PayloadAware, Handshake, Encryption, Compression, etc.)
//        /// </remarks>
//        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public virtual Action<byte[], int, int> ReceiveRawDataFunction
//        {
//            get
//            {
//                return m_receiveRawDataFunction;
//            }
//            set
//            {
//                m_receiveRawDataFunction = value;
//            }
//        }

//        /// <summary>
//        /// Gets the protocol used by the client for transferring data to and from the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The protocol used by the client for transferring data to and from the server.</returns>
//        [Browsable(false)]
//        public virtual TransportProtocol Protocol
//        {
//            get
//            {
//                return m_protocol;
//            }
//            set
//            {
//                m_protocol = value;
//            }
//        }

//        /// <summary>
//        /// Gets the ID of the server to which the client is connected.
//        /// </summary>
//        /// <value></value>
//        /// <returns>ID of the server to which the client is connected.</returns>
//        [Browsable(false)]
//        public virtual Guid ServerID
//        {
//            get
//            {
//                return m_serverID;
//            }
//            set
//            {
//                m_serverID = value;
//            }
//        }

//        /// <summary>
//        /// Gets the ID of the client.
//        /// </summary>
//        /// <value></value>
//        /// <returns>ID of the client.</returns>
//        [Browsable(false)]
//        public virtual Guid ClientID
//        {
//            get
//            {
//                return m_clientID;
//            }
//            set
//            {
//                m_clientID = value;
//            }
//        }

//        /// <summary>
//        /// Gets a boolean value indicating whether the client is currently connected to the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>True if the client is connected; otherwise False.</returns>
//        [Browsable(false)]
//        public virtual bool IsConnected
//        {
//            get
//            {
//                return m_isConnected;
//            }
//        }

//        /// <summary>
//        /// Gets the time in seconds for which the client has been connected to the server.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The time in seconds for which the client has been connected to the server.</returns>
//        [Browsable(false)]
//        public virtual double ConnectionTime
//        {
//            get
//            {
//                double clientConnectionTime = 0.0D;

//                if (m_connectTime > 0)
//                {
//                    if (m_isConnected) // Client is connected to the server.
//                    {
//                        clientConnectionTime = Ticks.ToSeconds(DateTime.Now.Ticks - m_connectTime);
//                    }
//                    else // Client is not connected to the server.
//                    {
//                        clientConnectionTime = Ticks.ToSeconds(m_disconnectTime - m_connectTime);
//                    }
//                }

//                return clientConnectionTime;
//            }
//        }

//        /// <summary>
//        /// Gets the total number of bytes sent by the client to the server since the connection is established.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The total number of bytes sent by the client to the server since the connection is established.</returns>
//        [Browsable(false)]
//        public virtual long TotalBytesSent
//        {
//            get
//            {
//                return m_totalBytesSent;
//            }
//        }

//        /// <summary>
//        /// Gets the total number of bytes received by the client from the server since the connection is established.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The total number of bytes received by the client from the server since the connection is established.</returns>
//        [Browsable(false)]
//        public virtual long TotalBytesReceived
//        {
//            get
//            {
//                return m_totalBytesReceived;
//            }
//        }

//        [Browsable(false)]
//        public virtual string Name
//        {
//            get
//            {
//                return m_settingsCategory;
//            }
//        }

//        /// <summary>
//        /// Gets the current status of the client.
//        /// </summary>
//        /// <value></value>
//        /// <returns>The current status of the client.</returns>
//        [Browsable(false)]
//        public virtual string Status
//        {
//            get
//            {
//                StringBuilder status = new StringBuilder();

//                status.Append("                 Server ID: ");
//                status.Append(m_serverID.ToString());
//                status.AppendLine();
//                status.Append("                 Client ID: ");
//                status.Append(m_clientID.ToString());
//                status.AppendLine();
//                status.Append("              Client state: ");
//                status.Append(m_isConnected ? "Connected" : "Not Connected");
//                status.AppendLine();
//                status.Append("           Connection time: ");
//                status.Append(Seconds.ToText(ConnectionTime));
//                status.AppendLine();
//                status.Append("            Receive buffer: ");
//                status.Append(m_receiveBufferSize.ToString());
//                status.AppendLine();
//                status.Append("        Transport protocol: ");
//                status.Append(m_protocol.ToString());
//                status.AppendLine();
//                status.Append("        Text encoding used: ");
//                status.Append(m_textEncoding.EncodingName);
//                status.AppendLine();
//                status.Append("          Total bytes sent: ");
//                status.Append(m_totalBytesSent);
//                status.AppendLine();
//                status.Append("      Total bytes received: ");
//                status.Append(m_totalBytesReceived);
//                status.AppendLine();

//                return status.ToString();
//            }
//        }

//        [Category("Settings")]
//        public bool PersistSettings
//        {
//            get
//            {
//                return m_persistSettings;
//            }
//            set
//            {
//                m_persistSettings = value;
//            }
//        }

//        [Category("Settings")]
//        public string SettingsCategory
//        {
//            get
//            {
//                return m_settingsCategory;
//            }
//            set
//            {
//                if (!string.IsNullOrEmpty(value))
//                {
//                    m_settingsCategory = value;
//                }
//                else
//                {
//                    throw new ArgumentNullException("SettingsCategoryName");
//                }
//            }
//        }

//        #endregion

//        #region [ Methods ]

//        /// <summary>
//        /// Releases the unmanaged resources used by an instance of the <see cref="ClientBase" /> class and optionally releases the managed resources.
//        /// </summary>
//        /// <param name="disposing"><strong>true</strong> to release both managed and unmanaged resources; <strong>false</strong> to release only unmanaged resources.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (!m_disposed)
//            {
//                try
//                {
//                    if (disposing)
//                    {
//                        Disconnect();   // Disconnect client from the server.
//                        SaveSettings(); // Saves settings to the config file.
//                    }
//                }
//                finally
//                {
//                    base.Dispose(disposing);
//                    m_disposed = true;          // Prevent duplicate dispose.
//                }
//            }
//        }

//        /// <summary>
//        /// Initializes the <see cref="ClientBase"/> object.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="ClientBase"/> 
//        /// object is not consumed through the designer surface of the IDE.
//        /// </remarks>
//        public void Initialize()
//        {
//            if (!m_initialized)
//            {
//                LoadSettings();         // Load settings from the config file.
//                m_initialized = true;   // Initialize only once.
//            }
//        }

//        /// <summary>
//        /// Connects the client to the server.
//        /// </summary>
//        public abstract void Connect();

//        /// <summary>
//        /// Cancels connecting to the server.
//        /// </summary>
//        public abstract void CancelConnect();

//        /// <summary>
//        /// Disconnects the client from the connected server
//        /// </summary>
//        public virtual void Disconnect()
//        {
//            Disconnect(Timeout.Infinite);
//        }

//        /// <summary>
//        /// Disconnects the client from the connected server, timing out within specified milliseconds if needed
//        /// </summary>
//        public abstract void Disconnect(int timeout);

//        /// <summary>
//        /// Sends data to the server.
//        /// </summary>
//        /// <param name="data">The plain-text data that is to be sent to the server.</param>
//        public virtual void Send(string data)
//        {
//            Send(m_textEncoding.GetBytes(data));
//        }

//        /// <summary>
//        /// Sends data to the server.
//        /// </summary>
//        /// <param name="serializableObject">The serializable object that is to be sent to the server.</param>
//        public virtual void Send(object serializableObject)
//        {
//            Send(Serialization.GetBytes(serializableObject));
//        }

//        /// <summary>
//        /// Sends data to the server.
//        /// </summary>
//        /// <param name="data">The binary data that is to be sent to the server.</param>
//        public virtual void Send(byte[] data)
//        {
//            Send(data, 0, data.Length);
//        }

//        /// <summary>
//        /// Sends the specified subset of data from the data buffer to the server.
//        /// </summary>
//        /// <param name="data">The buffer that contains the binary data to be sent.</param>
//        /// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
//        /// <param name="length">The number of bytes to be sent.</param>
//        public virtual void Send(byte[] data, int offset, int length)
//        {
//            if (m_enabled && m_isConnected)
//            {
//                if (data == null)
//                    throw new ArgumentNullException("data");

//                if (length > 0)
//                {
//                    byte[] dataToSend = GetPreparedData(data, offset, length);

//                    if (dataToSend.Length <= MaximumDataSize)
//                    {
//                        // PCP - 05/24/2007 : Reverting to synchronous send to avoid out-of-sequence transmissions.
//                        SendPreparedData(dataToSend);
//                    }
//                    else
//                    {
//                        // Prepared data is too large to be sent.
//                        throw new ArgumentException("Size of the data to be sent exceeds the maximum data size of " + MaximumDataSize + " bytes.");
//                    }
//                }
//                else
//                {
//                    throw new ArgumentNullException("data");
//                }
//            }
//        }

//        /// <summary>
//        /// Waits for the client to connect to the server for the specified time and optionally stop the client from
//        /// retrying connection attempts if the client is unable to connect to the server within the specified time.
//        /// </summary>
//        /// <param name="waitTime">
//        /// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1
//        /// will cause this method to wait indefinately until the client establishes connection with the server.
//        /// </param>
//        /// <returns>True if the connection was successful; otherwise False.</returns>
//        public bool WaitForConnection(int waitTime)
//        {
//            return WaitForConnection(waitTime, true);
//        }

//        /// <summary>
//        /// Waits for the client to connect to the server for the specified time and optionally stop the client from
//        /// retrying connection attempts if the client is unable to connect to the server within the specified time.
//        /// </summary>
//        /// <param name="waitTime">
//        /// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1
//        /// will cause this method to wait indefinately until the client establishes connection with the server.
//        /// </param>
//        /// <param name="stopRetrying">
//        /// Boolean value indicating whether the client should stop trying to connect to the server if it is unable to
//        /// connect to the server after waiting for the specified duration.
//        /// </param>
//        /// <returns>True if the connection was successful; otherwise False.</returns>
//        public bool WaitForConnection(int waitTime, bool stopRetrying)
//        {
//            if (!m_isConnected)
//            {
//                // We'll wait until client has connected or until the time to wait for connection is reached.
//                bool connected = m_connectionWaitHandle.WaitOne(waitTime, false);

//                // If the client hasn't connected after waiting for the specified time and if it is specified to stop
//                // attempting to connect to the server, then we'll call the CancelConnect() method.
//                if (stopRetrying && !connected)
//                {
//                    CancelConnect();
//                }

//                return connected;
//            }
//            else
//            {
//                return true;
//            }
//        }

//        public virtual void LoadSettings()
//        {
//            try
//            {
//                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];

//                if (settings.Count > 0)
//                {
//                    ConnectionString = settings["ConnectionString"].ValueAs(m_connectionString);
//                    ReceiveBufferSize = settings["ReceiveBufferSize"].ValueAs(m_receiveBufferSize);
//                    ReceiveTimeout = settings["ReceiveTimeout"].ValueAs(m_receiveTimeout);
//                    MaximumConnectionAttempts = settings["MaximumConnectionAttempts"].ValueAs(m_maximumConnectionAttempts);
//                    SecureSession = settings["SecureSession"].ValueAs(m_secureSession);
//                    Handshake = settings["Handshake"].ValueAs(m_handshake);
//                    HandshakePassphrase = settings["HandshakePassphrase"].ValueAs(m_handshakePassphrase);
//                    Encryption = settings["Encryption"].ValueAs(m_encryption);
//                    Compression = settings["Compression"].ValueAs(m_compression);
//                    Enabled = settings["Enabled"].ValueAs(m_enabled);
//                }
//            }
//            catch
//            {
//                // We'll encounter exceptions if the settings are not present in the config file.
//            }
//        }

//        public virtual void SaveSettings()
//        {
//            if (m_persistSettings)
//            {
//                try
//                {
//                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategory];
//                    CategorizedSettingsElement setting;

//                    settings.Clear();

//                    setting = settings["ConnectionString", true];
//                    setting.Value = m_connectionString;
//                    setting.Description = "Data required by the client to connect to the server.";

//                    setting = settings["ReceiveBufferSize", true];
//                    setting.Value = m_receiveBufferSize.ToString();
//                    setting.Description = "Maximum number of bytes that can be received at a time by the client from the server.";

//                    setting = settings["ReceiveTimeout", true];
//                    setting.Value = m_receiveTimeout.ToString();
//                    setting.Description = "Time to wait in milliseconds for data to be received from the server before timing out.";

//                    setting = settings["MaximumConnectionAttempts", true];
//                    setting.Value = m_maximumConnectionAttempts.ToString();
//                    setting.Description = "Maximum number of times the client will attempt to connect to the server.";

//                    setting = settings["SecureSession", true];
//                    setting.Value = m_secureSession.ToString();
//                    setting.Description = "True if the data exchanged between the client and server will be encrypted using a private session passphrase; otherwise False.";

//                    setting = settings["Handshake", true];
//                    setting.Value = m_handshake.ToString();
//                    setting.Description = "True if the client will do a handshake with the server; otherwise False.";

//                    setting = settings["HandshakePassphrase", true];
//                    setting.Value = m_handshakePassphrase;
//                    setting.Description = "Passpharse that will provided to the server for authentication during the handshake process.";

//                    setting = settings["Encryption", true];
//                    setting.Value = m_encryption.ToString();
//                    setting.Description = "Cipher strength (None; Level1; Level2; Level3; Level4; Level5) to be used for encrypting the data exchanged between the client and server.";

//                    setting = settings["Compression", true];
//                    setting.Value = m_compression.ToString();
//                    setting.Description = "Compression strength (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the client and server.";

//                    setting = settings["Enabled", true];
//                    setting.Value = m_enabled.ToString();
//                    setting.Description = "True if the client is enabled; otherwise False.";

//                    ConfigurationFile.Current.Save();
//                }
//                catch
//                {
//                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
//                }
//            }
//        }

//        /// <summary>
//        /// Performs necessary operations before the <see cref="ClientBase"/> object properties are initialized.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
//        /// by the designer if the <see cref="ClientBase"/> object is consumed through the designer surface of the IDE.
//        /// </remarks>
//        public void BeginInit()
//        {
//            // We don't need to do anything before the component is initialized.
//        }

//        /// <summary>
//        /// Performs necessary operations after the <see cref="ClientBase"/> object properties are initialized.
//        /// </summary>
//        /// <remarks>
//        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
//        /// by the designer if the <see cref="ClientBase"/> object is consumed through the designer surface of the IDE.
//        /// </remarks>
//        public void EndInit()
//        {
//            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
//            {
//                Initialize();
//            }
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.Connecting event.
//        /// </summary>
//        /// <param name="e">A System.EventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client is attempting connection to the server.</remarks>
//        protected virtual void OnConnecting(EventArgs e)
//        {
//            m_connectionWaitHandle.Reset();
//            if (Connecting != null) Connecting(this, e);
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.ConnectingCancelled event.
//        /// </summary>
//        /// <param name="e">A System.EventArgs that contains the event data.</param>
//        /// <remarks>
//        /// This method is to be called when attempts for connecting the client to the server are stopped on user's
//        /// request (i.e. When CancelConnect() is called before client is connected to the server).
//        /// </remarks>
//        protected virtual void OnConnectingCancelled(EventArgs e)
//        {
//            if (ConnectingCancelled != null) ConnectingCancelled(this, e);
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.ConnectingException event.
//        /// </summary>
//        /// <param name="e">A PCS.ExceptionEventArgs that contains the event data.</param>
//        /// <remarks>
//        /// This method is to be called when all attempts for connecting to the server have been made but failed
//        /// due to exceptions.
//        /// </remarks>
//        protected virtual void OnConnectingException(Exception e)
//        {
//            if (ConnectingException != null) ConnectingException(this, new EventArgs<Exception>(e));
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.Connected event.
//        /// </summary>
//        /// <param name="e">A System.EventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client has successfully connected to the server.</remarks>
//        protected virtual void OnConnected(EventArgs e)
//        {
//            m_isConnected = true;
//            m_connectTime = DateTime.Now.Ticks; // Save the time when the client connected to the server.
//            m_disconnectTime = 0;
//            m_totalBytesSent = 0; // Reset the number of bytes sent and received between the client and server.
//            m_totalBytesReceived = 0;
//            m_connectionWaitHandle.Set();
//            if (Connected != null) Connected(this, e);
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.Disconnected event.
//        /// </summary>
//        /// <param name="e">A System.EventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client has disconnected from the server.</remarks>
//        protected virtual void OnDisconnected(EventArgs e)
//        {
//            m_serverID = Guid.Empty;
//            m_isConnected = false;
//            m_disconnectTime = DateTime.Now.Ticks; // Save the time when client was disconnected from the server.
//            if (Disconnected != null) Disconnected(this, e);
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.SendBegin event.
//        /// </summary>
//        /// <param name="e">A PCS.DataEventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client begins sending data to the server.</remarks>
//        protected virtual void OnSendDataBegin(IdentifiableItem<Guid, byte[]> e)
//        {
//            if (SendDataBegin != null) SendDataBegin(this, new EventArgs<IdentifiableItem<Guid, byte[]>>(e));
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.SendComplete event.
//        /// </summary>
//        /// <param name="e">A PCS.DataEventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client has finished sending data to the server.</remarks>
//        protected virtual void OnSendDataComplete(IdentifiableItem<Guid, byte[]> e)
//        {
//            m_totalBytesSent += e.Item.Length;
//            if (SendDataComplete != null) SendDataComplete(this, new EventArgs<IdentifiableItem<Guid, byte[]>>(e));
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.ReceivedData event.
//        /// </summary>
//        /// <param name="e">A PCS.DataEventArgs that contains the event data.</param>
//        /// <remarks>This method is to be called when the client receives data from the server.</remarks>
//        protected virtual void OnReceivedData(IdentifiableItem<Guid, byte[]> e)
//        {
//            m_totalBytesReceived += e.Item.Length;

//            try
//            {
//                e.Item = GetActualData(e.Item);
//            }
//            catch
//            {
//                // We'll just pass on the data that we received.
//            }

//            if (ReceivedData != null) ReceivedData(this, new EventArgs<IdentifiableItem<Guid, byte[]>>(e));
//        }

//        /// <summary>
//        /// Raises the PCS.Communication.ClientBase.ReceiveTimedOut event.
//        /// </summary>
//        /// <param name="e">A System.EventArgs that contains the event data.</param>
//        /// <remarks>
//        /// This method is to be called when no data is received from the server after waiting for the specified time.
//        /// </remarks>
//        protected virtual void OnReceiveTimedOut(EventArgs e)
//        {
//            if (ReceiveTimedOut != null) ReceiveTimedOut(this, e);
//        }

//        /// <summary>
//        /// Performs the necessary compression and encryption on the specified data and returns it.
//        /// </summary>
//        /// <param name="data">The data on which compression and encryption is to be performed.</param>
//        /// <returns>Compressed and encrypted data.</returns>
//        /// <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
//        protected virtual byte[] GetPreparedData(byte[] data)
//        {
//            return GetPreparedData(data, 0, data.Length);
//        }

//        /// <summary>
//        /// Performs the necessary compression and encryption on the specified data and returns it.
//        /// </summary>
//        /// <param name="data">The data on which compression and encryption is to be performed.</param>
//        /// <param name="offset">The index into buffer at which the desired data begins.</param>
//        /// <param name="length">The length of the stream in bytes.</param>
//        /// <returns>Compressed and encrypted data.</returns>
//        /// <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
//        protected virtual byte[] GetPreparedData(byte[] data, int offset, int length)
//        {
//            if ((m_secureSession || m_encryption == CipherStrength.None) && m_compression == CompressionStrength.NoCompression)
//            {
//                // Return only used part of source buffer
//                return data.CopyBuffer(offset, length);
//            }
//            else
//            {
//                if (m_compression != CompressionStrength.NoCompression)
//                {
//                    data = Transport.CompressData(data, offset, length, m_compression);
//                    offset = 0;
//                    length = data.Length;
//                }

//                if (!m_secureSession && m_encryption != CipherStrength.None)
//                {
//                    string key = m_handshakePassphrase;

//                    if (string.IsNullOrEmpty(key))
//                        key = DefaultCryptoKey;

//                    data = Transport.EncryptData(data, offset, length, key, m_encryption);
//                }

//                return data;
//            }
//        }

//        /// <summary>
//        /// Performs the necessary uncompression and decryption on the specified data and returns it.
//        /// </summary>
//        /// <param name="data">The data on which uncompression and decryption is to be performed.</param>
//        /// <returns>Uncompressed and decrypted data.</returns>
//        /// <remarks>No decryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
//        protected virtual byte[] GetActualData(byte[] data)
//        {
//            if (!m_secureSession && m_encryption != CipherStrength.None)
//            {
//                string key = m_handshakePassphrase;

//                if (string.IsNullOrEmpty(key))
//                    key = DefaultCryptoKey;

//                data = Transport.DecryptData(data, key, m_encryption);
//            }

//            if (m_compression != CompressionStrength.NoCompression)
//            {
//                data = Transport.DecompressData(data, m_compression);
//            }

//            return data;
//        }

//        /// <summary>
//        /// Sends prepared data to the server.
//        /// </summary>
//        /// <param name="data">The prepared data that is to be sent to the server.</param>
//        protected abstract void SendPreparedData(byte[] data);

//        /// <summary>
//        /// Determines whether specified connection string required for the client to connect to the server is valid.
//        /// </summary>
//        /// <param name="connectionString">The connection string to be validated.</param>
//        /// <returns>True is the connection string is valid; otherwise False.</returns>
//        protected abstract bool ValidConnectionString(string connectionString);

//        #endregion

//        #region [ Static ]

//        ///// <summary>
//        ///// Create a communications client
//        ///// </summary>
//        ///// <remarks>
//        ///// Note that typical connection string should be prefixed with a "protocol=tcp", "protocol=udp", "protocol=serial" or "protocol=file"
//        ///// </remarks>
//        //public static IClient Create(string connectionString)
//        //{
//        //    Dictionary<string, string> connectionData = connectionString.ParseKeyValuePairs();
//        //    IClient client = null;
//        //    string protocol;

//        //    if (connectionData.TryGetValue("protocol", out protocol))
//        //    {
//        //        connectionData.Remove("protocol");
//        //        StringBuilder settings = new StringBuilder();

//        //        foreach (string key in connectionData.Keys)
//        //        {
//        //            settings.Append(key);
//        //            settings.Append("=");
//        //            settings.Append(connectionData[key]);
//        //            settings.Append(";");
//        //        }

//        //        switch (protocol.ToLower())
//        //        {
//        //            case "tcp":
//        //                client = new TcpClient(settings.ToString());
//        //                break;
//        //            case "udp":
//        //                client = new UdpClient(settings.ToString());
//        //                break;
//        //            case "serial":
//        //                client = new SerialClient(settings.ToString());
//        //                break;
//        //            case "file":
//        //                client = new FileClient(settings.ToString());
//        //                break;
//        //            default:
//        //                throw new ArgumentException("Transport protocol \'" + protocol + "\' is not valid.");
//        //        }
//        //    }
//        //    else
//        //    {
//        //        throw new ArgumentException("Transport protocol must be specified.");
//        //    }

//        //    return client;
//        //}

//        #endregion
//    }
//}
