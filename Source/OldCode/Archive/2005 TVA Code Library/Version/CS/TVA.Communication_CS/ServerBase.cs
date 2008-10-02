//*******************************************************************************************************
//  ServerBase.cs
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
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed server data access
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
using System.Text;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using TVA.Services;
using TVA.IO;
using TVA.IO.Compression;
using TVA.Security.Cryptography;
using TVA.Configuration;
using TVA.Security;

namespace TVA.Communication
{
    /// <summary>
    /// Represents a server involved in the transportation of data.
    /// </summary>
    [ToolboxBitmap(typeof(ServerBase)), DefaultEvent("ReceivedClientData")]
    public abstract partial class ServerBase : Component, IServer, IPersistSettings, ISupportInitialize
	{
        #region [ Members ]

        // Constants

        /// <summary>
        /// The maximum number of bytes that can be sent from the server to clients in a single send operation.
        /// </summary>
        public const int MaximumDataSize = 524288000; // 500 MB

        /// <summary>
        /// The key used for encryption and decryption when Encryption is enabled but HandshakePassphrase is not set.
        /// </summary>
        protected const string DefaultCryptoKey = "6572a33d-826f-4d96-8c28-8be66bbc700e";

        // Delegates

        // Events

        /// <summary>
        /// Occurs when the server is started.
        /// </summary>
        [Description("Occurs when the server is started."), Category("Server")]
        public event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the server is stopped.
        /// </summary>
        [Description("Occurs when the server is stopped."), Category("Server")]
        public event EventHandler ServerStopped;

        /// <summary>
        /// Occurs when an exception is encountered while starting up the server.
        /// </summary>
        [Description("Occurs when an exception is encountered while starting up the server."), Category("Server")]
        public event EventHandler<EventArgs<Exception>> ServerStartupException;

        /// <summary>
        /// Occurs when a client is connected to the server.
        /// </summary>
        [Description("Occurs when a client is connected to the server."), Category("Client")]
        public event EventHandler<EventArgs<Guid>> ClientConnected;

        /// <summary>
        /// Occurs when a client is disconnected from the server.
        /// </summary>
        [Description("Occurs when a client is disconnected from the server."), Category("Client")]
        public event EventHandler<EventArgs<Guid>> ClientDisconnected;

        /// <summary>
        /// Occurs when data is received from a client.
        /// </summary>
        [Description("Occurs when data is received from a client."), Category("Data")]
        public event EventHandler<EventArgs<IdentifiableItem<Guid, Byte[]>>> ReceivedClientData;		

        // Fields
        private string m_configurationString;
        private int m_receiveBufferSize;
        private int m_maximumClients;
        private bool m_secureSession;
        private bool m_handshake;
        private string m_handshakePassphrase;
        private CipherStrength m_encryption;
        private CompressionStrength m_compression;
        //private CRCCheckType m_crcCheck;
        private bool m_enabled;
        private Encoding m_textEncoding;
        private TransportProtocol m_protocol;
        private Guid m_serverID;
        private List<Guid> m_clientIDs;
        private bool m_isRunning;
        private bool m_persistSettings;
        private string m_settingsCategoryName;
        private bool m_disposed;
        private long m_startTime;
        private long m_stopTime;
        private bool m_previouslyEnabled;

        // We expose these two members to derived classes for their own internal use
        protected Action<byte[], int, int> m_receiveRawDataFunction;
        protected byte[] m_buffer;

        #endregion

        #region [ Constructors ]

        protected ServerBase()
		{
			// Setup the default values.
			m_receiveBufferSize = 8192;
			m_maximumClients = - 1;
			m_handshake = true;
			m_encryption = CipherStrength.None;
			m_compression = CompressionStrength.NoCompression;
			//m_crcCheck = CRCCheckType.None;
			m_enabled = true;
			m_textEncoding = System.Text.Encoding.ASCII;
			m_serverID = Guid.NewGuid(); // Create an ID for the server.
			m_clientIDs = new List<Guid>();
			m_settingsCategoryName = this.GetType().Name;			
			m_startTime = 0;
			m_stopTime = 0;
			m_buffer = new byte[m_receiveBufferSize];
		}

        protected ServerBase(string configurationString)
            : this()
		{
            m_configurationString = configurationString;
		}

        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="ServerBase" /> class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual <see cref="Dispose(bool)" /> method, passing in <strong>false</strong>.
        /// </remarks>
        ~ServerBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the data that is required by the server to initialize.
        /// </summary>
        /// <value></value>
        /// <returns>The data that is required by the server to initialize.</returns>
        [Description("The data that is required by the server to initialize."), Category("Configuration")]
        public virtual string ConfigurationString
        {
            get
            {
                return m_configurationString;
            }
            set
            {
                if (ValidConfigurationString(value))
                {
                    m_configurationString = value;
                    if (IsRunning)
                    {
                        // Restart the server when configuration data is changed.
                        Stop();
                        Start();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of clients that can connect to the server.
        /// </summary>
        /// <value></value>
        /// <returns>The maximum number of clients that can connect to the server.</returns>
        /// <remarks>Set MaximumClients = -1 for infinite client connections.</remarks>
        [Description("The maximum number of clients that can connect to the server. Set MaximumClients = -1 for infinite client connections."), Category("Configuration"), DefaultValue(typeof(int), "-1")]
        public virtual int MaximumClients
        {
            get
            {
                return m_maximumClients;
            }
            set
            {
                if (value == -1 || value > 0)
                {
                    m_maximumClients = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the data exchanged between the server and clients
        /// will be encrypted using a private session passphrase.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// True if the data exchanged between the server and clients will be encrypted using a private session
        /// passphrase; otherwise False.
        /// </returns>
        ///<remarks>Handshake and Encryption must be enabled in order to use SecureSession.</remarks>
        [Description("Indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase."), Category("Security"), DefaultValue(typeof(bool), "False")]
        public virtual bool SecureSession
        {
            get
            {
                return m_secureSession;
            }
            set
            {
                if (!value || (value && m_handshake && m_encryption != CipherStrength.None))
                {
                    m_secureSession = value;
                }
                else
                {
                    throw new InvalidOperationException("Handshake and Encryption must be enabled in order to use SecureSession.");
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the server will do a handshake with the client after
        /// accepting its connection.
        /// </summary>
        /// <value></value>
        /// <returns>True if the server will do a handshake with the client; otherwise False.</returns>
        /// <remarks>SecureSession must be disabled before disabling Handshake.</remarks>
        [Description("Indicates whether the server will do a handshake with the client after accepting its connection."), Category("Security"), DefaultValue(typeof(bool), "True")]
        public virtual bool Handshake
        {
            get
            {
                return m_handshake;
            }
            set
            {
                if (value || (!value && !m_secureSession))
                {
                    m_handshake = value;

                    if (!m_handshake)
                    {
                        // Handshake passphrase will have no effect if handshaking is disabled.
                        m_handshakePassphrase = "";
                    }
                }
                else
                {
                    throw new ArgumentException("SecureSession must be disabled before disabling Handshake.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the passpharse that the clients must provide for authentication during the handshake process.
        /// </summary>
        /// <value></value>
        /// <returns>The passpharse that the clients must provide for authentication during the handshake process.</returns>
        [Description("The passpharse that the clients must provide for authentication during the handshake process."), Category("Security"), DefaultValue(typeof(string), "")]
        public virtual string HandshakePassphrase
        {
            get
            {
                return m_handshakePassphrase;
            }
            set
            {
                m_handshakePassphrase = value;

                if (!string.IsNullOrEmpty(m_handshakePassphrase))
                {
                    // Handshake password has no effect until handshaking is enabled.
                    m_handshake = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
        /// </summary>
        /// <value>Receive buffer size</value>
        /// <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
        /// <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
        /// <returns>The maximum number of bytes that can be received at a time by the server from the clients.</returns>
        [Description("The maximum number of bytes that can be received at a time by the server from the clients."), Category("Data"), DefaultValue(typeof(int), "8192")]
        public virtual int ReceiveBufferSize
        {
            get
            {
                return m_receiveBufferSize;
            }
            set
            {
                if (m_isRunning)
                    throw new InvalidOperationException("Cannot change receive buffer size while server is running");

                if (value > 0)
                {
                    m_receiveBufferSize = value;
                    m_buffer = new byte[value];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the encryption level to be used for encrypting the data exchanged between the server and
        /// clients.
        /// </summary>
        /// <value></value>
        /// <returns>The encryption level to be used for encrypting the data exchanged between the server and clients.</returns>
        /// <remarks>
        /// <para>Set Encryption = None to disable encryption.</para>
        /// <para>
        /// The key used for performing cryptography will be different in different senarios.
        /// 1) HandshakePassphrase will be used as the key when HandshakePassphrase is set and SecureSession is
        ///    disabled.
        /// 2) DefaultCryptoKey will be used as the key when HandshakePassphrase is not set and SecureSession is
        ///    disabled.
        /// 3) A private session key will be used as the key when SecureSession is enabled.
        /// </para>
        /// </remarks>
        [Description("The encryption level to be used for encrypting the data exchanged between the server and clients."), Category("Data"), DefaultValue(typeof(CipherStrength), "None")]
        public virtual CipherStrength Encryption
        {
            get
            {
                return m_encryption;
            }
            set
            {
                if (!m_secureSession || (m_secureSession && value != CipherStrength.None))
                {
                    m_encryption = value;
                }
                else
                {
                    throw new ArgumentException("SecureSession session must be disabled before disabling Encryption.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the compression level to be used for compressing the data exchanged between the server and
        /// clients.
        /// </summary>
        /// <value></value>
        /// <returns>The compression level to be used for compressing the data exchanged between the server and clients.</returns>
        /// <remarks>Set Compression = NoCompression to disable compression.</remarks>
        [Description("The compression level to be used for compressing the data exchanged between the server and clients."), Category("Data"), DefaultValue(typeof(CompressionStrength), "NoCompression")]
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
        /// Gets or sets a boolean value indicating whether the server is enabled.
        /// </summary>
        /// <value></value>
        /// <returns>True if the server is enabled; otherwise False.</returns>
        [Description("Indicates whether the server is enabled."), Category("Behavior"), DefaultValue(typeof(bool), "True")]
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoding to be used for the text sent to the connected clients.
        /// </summary>
        /// <value></value>
        /// <returns>The encoding to be used for the text sent to the connected clients.</returns>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        /// Setting this property allows consumer to "intercept" data before it goes through normal processing
        /// </summary>
        /// <remarks>
        /// This property only needs to be implemented if you need data from the clients absolutelty as fast as possible, for most uses this
        /// will not be necessary.  Setting this property gives the consumer access to the data stream as soon as it's available, but this also
        /// bypasses all of the advanced convience properties (e.g., PayloadAware, Handshake, Encryption, Compression, etc.)
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Action<byte[], int, int> ReceiveRawDataFunction
        {
            get
            {
                return m_receiveRawDataFunction;
            }
            set
            {
                m_receiveRawDataFunction = value;
            }
        }

        /// <summary>
        /// Gets the protocol used by the server for transferring data to and from the clients.
        /// </summary>
        /// <value></value>
        /// <returns>The protocol used by the server for transferring data to and from the clients.</returns>
        [Browsable(false)]
        public virtual TransportProtocol Protocol
        {
            get
            {
                return m_protocol;
            }
            set
            {
                m_protocol = value;
            }
        }

        /// <summary>
        /// Gets the server's ID.
        /// </summary>
        /// <value></value>
        /// <returns>ID of the server.</returns>
        [Browsable(false)]
        public virtual Guid ServerID
        {
            get
            {
                return m_serverID;
            }
        }

        /// <summary>
        /// Gets a collection of client IDs that are connected to the server.
        /// </summary>
        /// <value></value>
        /// <returns>A collection of client IDs that are connected to the server.</returns>
        [Browsable(false)]
        public virtual List<Guid> ClientIDs
        {
            get
            {
                return m_clientIDs;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the server is currently running.
        /// </summary>
        /// <value></value>
        /// <returns>True if the server is running; otherwise False.</returns>
        [Browsable(false)]
        public virtual bool IsRunning
        {
            get
            {
                return m_isRunning;
            }
        }

        /// <summary>
        /// Gets the time in seconds for which the server has been running.
        /// </summary>
        /// <value></value>
        /// <returns>The time in seconds for which the server has been running.</returns>
        [Browsable(false)]
        public virtual double RunTime
        {
            get
            {
                double serverRunTime = 0.0D;

                if (m_startTime > 0)
                {
                    if (m_isRunning) // Server is running.
                    {
                        serverRunTime = Ticks.ToSeconds(DateTime.Now.Ticks - m_startTime);
                    }
                    else // Server is not running.
                    {
                        serverRunTime = Ticks.ToSeconds(m_stopTime - m_startTime);
                    }
                }
                return serverRunTime;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return m_settingsCategoryName;
            }
        }

        /// <summary>
        /// Gets the current status of the server.
        /// </summary>
        /// <value></value>
        /// <returns>The current status of the server.</returns>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("                 Server ID: ");
                status.Append(m_serverID.ToString());
                status.AppendLine();
                status.Append("              Server state: ");
                status.Append(m_isRunning ? "Running" : "Not Running");
                status.AppendLine();
                status.Append("            Server runtime: ");
                status.Append(Seconds.ToText(RunTime));
                status.AppendLine();
                status.Append("      Configuration string: ");
                status.Append(m_configurationString);
                status.AppendLine();
                status.Append("        Subscribed clients: ");
                lock (m_clientIDs)
                {
                    status.Append(m_clientIDs.Count);
                }
                status.AppendLine();
                status.Append("           Maximum clients: ");
                status.Append(m_maximumClients == -1 ? "Infinite" : m_maximumClients.ToString());
                status.AppendLine();
                status.Append("            Receive buffer: ");
                status.Append(m_receiveBufferSize.ToString());
                status.AppendLine();
                status.Append("        Transport protocol: ");
                status.Append(m_protocol.ToString());
                status.AppendLine();
                status.Append("        Text encoding used: ");
                status.Append(m_textEncoding.EncodingName);
                status.AppendLine();

                return status.ToString();
            }
        }

        [Category("Settings")]
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

        [Category("Settings")]
        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    m_settingsCategoryName = value;
                }
                else
                {
                    throw new ArgumentNullException("SettingsCategoryName");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="ServerBase" /> class and optionally releases the managed resources.
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
                        Stop();         // Stop the server.
                        SaveSettings(); // Saves settings to the config file.
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Sends data to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The plain-text data that is to be sent to the client.</param>
        public virtual void SendTo(Guid clientID, string data)
        {
            SendTo(clientID, m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="serializableObject">The serializable object that is to be sent to the client.</param>
        public virtual void SendTo(Guid clientID, object serializableObject)
        {
            SendTo(clientID, Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The binary data that is to be sent to the client.</param>
        public virtual void SendTo(Guid clientID, byte[] data)
        {
            SendTo(clientID, data, 0, data.Length);
        }

        /// <summary>
        /// Sends the specified subset of data from the data buffer to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent.</param>
        public virtual void SendTo(Guid clientID, byte[] data, int offset, int length)
        {
            if (m_enabled && m_isRunning)
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (length > 0)
                {
                    // Pre-condition data as needed (compression, encryption, etc.)
                    data = GetPreparedData(data, offset, length);

                    if (data.Length <= MaximumDataSize)
                    {
                        // PCP - 05/24/2007: Reverting to synchronous send to avoid out-of-sequence transmissions.
                        SendPreparedDataTo(clientID, data);
                    }
                    else
                    {
                        // Prepared data is too large to be sent.
                        throw new ArgumentException("Size of the data to be sent exceeds the maximum data size of " + MaximumDataSize + " bytes.");
                    }
                }
            }
        }

        /// <summary>
        /// Sends data to all of the subscribed clients.
        /// </summary>
        /// <param name="data">The plain-text data that is to sent to the subscribed clients.</param>
        public virtual void Multicast(string data)
        {
            Multicast(m_textEncoding.GetBytes(data));
        }

        /// <summary>
        /// Sends data to all of the subscribed clients.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent to the subscribed clients.</param>
        public virtual void Multicast(object serializableObject)
        {
            Multicast(Serialization.GetBytes(serializableObject));
        }

        /// <summary>
        /// Sends data to all of the subscribed clients.
        /// </summary>
        /// <param name="data">The binary data that is to sent to the subscribed clients.</param>
        public virtual void Multicast(byte[] data)
        {
            Multicast(data, 0, data.Length);
        }

        /// <summary>
        /// Sends the specified subset of data from the data buffer to all of the subscribed clients.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent.</param>
        public virtual void Multicast(byte[] data, int offset, int length)
        {
            if (m_enabled && m_isRunning)
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                if (length > 0)
                {
                    // Pre-condition data as needed (compression, encryption, etc.)
                    data = GetPreparedData(data, offset, length);

                    if (data.Length <= MaximumDataSize)
                    {
                        lock (m_clientIDs)
                        {
                            foreach (Guid clientID in m_clientIDs)
                            {
                                try
                                {
                                    // PCP - 05/24/2007: Reverting to synchronous send to avoid out-of-sequence transmissions.
                                    SendPreparedDataTo(clientID, data);
                                }
                                catch
                                {
                                    // In rare cases, we might encounter an exception here when the inheriting server
                                    // class doesn't think that the ID of the client to which it has to send the message is
                                    // valid (even though it is). This might happen when connection with a new client is
                                    // not yet complete and we're trying to send a message to the client.
                                }
                            }
                        }
                    }
                    else
                    {
                        // Prepared data is too large to be sent.
                        throw new ArgumentException("Size of the data to be sent exceeds the maximum data size of " + MaximumDataSize + " bytes.");
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects all of the connected clients.
        /// </summary>
        public void DisconnectAll()
        {
            List<Guid> clientIDs = new List<Guid>();

            lock (m_clientIDs)
            {
                ClientIDs.AddRange(m_clientIDs);
            }

            foreach (Guid clientID in clientIDs)
            {
                DisconnectOne(clientID);
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Disconnects a connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        public abstract void DisconnectOne(Guid clientID);

        public virtual void ProcessStateChanged(string processName, ProcessState newState)
        {
            // This component is not abstractly associated with any particular service process...
        }

        public virtual void ServiceStateChanged(ServiceState newState)
        {
            switch (newState)
            {
                case ServiceState.Paused:
                    m_previouslyEnabled = m_enabled;
                    Enabled = false;
                    break;
                case ServiceState.Resumed:
                    Enabled = m_previouslyEnabled;
                    break;
                case ServiceState.Shutdown:
                    Dispose();
                    break;
            }
        }

        public virtual void LoadSettings()
        {
            try
            {
                CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                if (settings.Count > 0)
                {
                    ConfigurationString = settings["ConfigurationString"].ValueAs(m_configurationString);
                    ReceiveBufferSize = settings["ReceiveBufferSize"].ValueAs(m_receiveBufferSize);
                    MaximumClients = settings["MaximumClients"].ValueAs(m_maximumClients);
                    SecureSession = settings["SecureSession"].ValueAs(m_secureSession);
                    Handshake = settings["Handshake"].ValueAs(m_handshake);
                    HandshakePassphrase = settings["HandshakePassphrase"].ValueAs(m_handshakePassphrase);
                    Encryption = settings["Encryption"].ValueAs(m_encryption);
                    Compression = settings["Compression"].ValueAs(m_compression);
                    Enabled = settings["Enabled"].ValueAs(m_enabled);
                }
            }
            catch
            {
                // We'll encounter exceptions if the settings are not present in the config file.
            }
        }

        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];
                    CategorizedSettingsElement setting;

                    settings.Clear();

                    setting = settings["ConfigurationString", true];
                    setting.Value = m_configurationString;
                    setting.Description = "Data required by the server to initialize.";
                    
                    setting = settings["ReceiveBufferSize", true];
                    setting.Value = m_receiveBufferSize.ToString();
                    setting.Description = "Maximum number of bytes that can be received at a time by the server from the clients.";
                    
                    setting = settings["MaximumClients", true];
                    setting.Value = m_maximumClients.ToString();
                    setting.Description = "Maximum number of clients that can connect to the server.";
                    
                    setting = settings["SecureSession", true];
                    setting.Value = m_secureSession.ToString();
                    setting.Description = "True if the data exchanged between the server and clients will be encrypted using a private session passphrase; otherwise False.";
                    
                    setting = settings["Handshake", true];
                    setting.Value = m_handshake.ToString();
                    setting.Description = "True if the server will do a handshake with the client; otherwise False.";
                    
                    setting = settings["HandshakePassphrase", true];
                    setting.Value = m_handshakePassphrase;
                    setting.Description = "Passpharse that the clients must provide for authentication during the handshake process.";
                    
                    setting = settings["Encryption", true];
                    setting.Value = m_encryption.ToString();
                    setting.Description = "Cipher strength (None; Level1; Level2; Level3; Level4; Level5) to be used for encrypting the data exchanged between the server and clients.";
                    
                    setting = settings["Compression", true];
                    setting.Value = m_compression.ToString();
                    setting.Description = "Compression strength (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the server and clients.";
                    
                    setting = settings["Enabled", true];
                    setting.Value = m_enabled.ToString();
                    setting.Description = "True if the server is enabled; otherwise False.";

                    ConfigurationFile.Current.Save();
                }
                catch
                {
                    // We might encounter an exception if for some reason the settings cannot be saved to the config file.
                }
            }
        }

        public void BeginInit()
        {
            // We don't need to do anything before the component is initialized.
        }

        public void EndInit()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                LoadSettings(); // Load settings from the config file.
            }
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ServerStarted event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        /// <remarks>This method is to be called after the server has been started.</remarks>
        protected virtual void OnServerStarted(EventArgs e)
        {
            m_isRunning = true;
            m_startTime = DateTime.Now.Ticks; // Save the time when server is started.
            m_stopTime = 0;
            if (ServerStarted != null) ServerStarted(this, e);
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ServerStopped event.
        /// </summary>
        /// <param name="e">A System.EventArgs that contains the event data.</param>
        /// <remarks>This method is to be called after the server has been stopped.</remarks>
        protected virtual void OnServerStopped(EventArgs e)
        {
            m_isRunning = false;
            m_stopTime = DateTime.Now.Ticks; // Save the time when server is stopped.
            if (ServerStopped != null) ServerStopped(this, e);
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ServerStartupException event.
        /// </summary>
        /// <param name="e">A TVA.ExceptionEventArgs that contains the event data.</param>
        /// <remarks>This method is to be called if the server throws an exception during startup.</remarks>
        protected virtual void OnServerStartupException(Exception e)
        {
            if (ServerStartupException != null) ServerStartupException(this, new EventArgs<Exception>(e));
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ClientConnected event.
        /// </summary>
        /// <param name="e">A TVA.IdentifiableSourceEventArgs that contains the event data.</param>
        /// <remarks>This method is to be called when a client is connected to the server.</remarks>
        protected virtual void OnClientConnected(Guid e)
        {
            lock (m_clientIDs)
            {
                m_clientIDs.Add(e);
            }

            if (ClientConnected != null) ClientConnected(this, new EventArgs<Guid>(e));
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ClientDisconnected event.
        /// </summary>
        /// <param name="e">A TVA.IdentifiableSourceEventArgs that contains the event data.</param>
        /// <remarks>This method is to be called when a client has disconnected from the server.</remarks>
        protected virtual void OnClientDisconnected(Guid e)
        {
            lock (m_clientIDs)
            {
                m_clientIDs.Remove(e);
            }

            if (ClientDisconnected != null) ClientDisconnected(this, new EventArgs<Guid>(e));
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ReceivedClientData event.
        /// </summary>
        /// <param name="e">A TVA.DataEventArgs that contains the event data.</param>
        /// <remarks>This method is to be called when the server receives data from a client.</remarks>
        protected virtual void OnReceivedClientData(IdentifiableItem<Guid, byte[]> e)
        {
            try
            {
                e.Item = GetActualData(e.Item);
            }
            catch
            {
                // We'll just pass on the data that we received.
            }

            if (ReceivedClientData != null) ReceivedClientData(this, new EventArgs<IdentifiableItem<Guid, byte[]>>(e));
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the specified data and returns it.
        /// </summary>
        /// <param name="data">The data on which compression and encryption is to be performed.</param>
        /// <returns>Compressed and encrypted data.</returns>
        /// <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
        protected virtual byte[] GetPreparedData(byte[] data)
        {
            return GetPreparedData(data, 0, data.Length);
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the specified data and returns it.
        /// </summary>
        /// <param name="data">The data on which compression and encryption is to be performed.</param>
        /// <param name="offset">The index into buffer at which the desired data begins.</param>
        /// <param name="length">The length of the stream in bytes.</param>
        /// <returns>Compressed and encrypted data.</returns>
        /// <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
        protected virtual byte[] GetPreparedData(byte[] data, int offset, int length)
        {
            if ((m_secureSession || m_encryption == CipherStrength.None) && m_compression == CompressionStrength.NoCompression)
            {
                // Return only used part of source buffer
                return data.CopyBuffer(offset, length);
            }
            else
            {
                if (m_compression != CompressionStrength.NoCompression)
                {
                    data = Transport.CompressData(data, offset, length, m_compression);
                    offset = 0;
                    length = data.Length;
                }

                if (!m_secureSession && m_encryption != CipherStrength.None)
                {
                    string key = m_handshakePassphrase;

                    if (string.IsNullOrEmpty(key))
                        key = DefaultCryptoKey;

                    data = Transport.EncryptData(data, offset, length, key, m_encryption);
                }

                return data;
            }
        }

        /// <summary>
        /// Performs the necessary uncompression and decryption on the specified data and returns it.
        /// </summary>
        /// <param name="data">The data on which uncompression and decryption is to be performed.</param>
        /// <returns>Uncompressed and decrypted data.</returns>
        /// <remarks>No decryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
        protected virtual byte[] GetActualData(byte[] data)
        {
            if (!m_secureSession && m_encryption != CipherStrength.None)
            {
                string key = m_handshakePassphrase;

                if (string.IsNullOrEmpty(key))
                    key = DefaultCryptoKey;

                data = Transport.DecryptData(data, key, m_encryption);
            }

            if (m_compression != CompressionStrength.NoCompression)
            {
                data = Transport.DecompressData(data, m_compression);
            }

            return data;
        }

        /// <summary>
        /// Sends prepared data to the specified client.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The prepared data that is to be sent to the client.</param>
        protected abstract void SendPreparedDataTo(Guid clientID, byte[] data);

        /// <summary>
        /// Determines whether specified configuration string required for the server to initialize is valid.
        /// </summary>
        /// <param name="configurationString">The configuration string to be validated.</param>
        /// <returns>True is the configuration string is valid; otherwise False.</returns>
        protected abstract bool ValidConfigurationString(string configurationString);

        #endregion

        #region [ Static ]

        /// <summary>
        /// Create a communications server
        /// </summary>
        /// <remarks>
        /// Note that typical configuration string should be prefixed with a "protocol=tcp" or a "protocol=udp"
        /// </remarks>
        public static IServer CreateCommunicationServer(string configurationString)
        {
            Dictionary<string, string> configurationData = configurationString.ParseKeyValuePairs();
            IServer server = null;
            string protocol;

            if (configurationData.TryGetValue("protocol", out protocol))
            {
                configurationData.Remove("protocol");
                StringBuilder settings = new StringBuilder();

                foreach (string key in configurationData.Keys)
                {
                    settings.Append(key);
                    settings.Append("=");
                    settings.Append(configurationData[key]);
                    settings.Append(";");
                }

                switch (protocol.ToLower())
                {
                    case "tcp":
                        server = new TcpServer(settings.ToString());
                        break;
                    case "udp":
                        server = new UdpServer(settings.ToString());
                        break;
                    default:
                        throw new ArgumentException("Transport protocol \'" + protocol + "\' is not valid.");
                }
            }
            else
            {
                throw new ArgumentException("Transport protocol must be specified.");
            }

            return server;
        }

        #endregion
				
	}	
}