//*******************************************************************************************************
//  TVA.Communication.CommunicationServerBase.vb - Base functionality of a server for transporting data
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
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed server data access
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter
//  02/19/2008 - Pinal C. Patel
//       Added code to detect and avoid redundant calls to Dispose().
//
//*******************************************************************************************************

using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using TVA.Services;
using TVA.IO;
using TVA.IO.Compression;
using TVA.Security.Cryptography;
using TVA.Configuration;
using TVA.Security;

/// <summary>
/// Represents a server involved in the transportation of data.
/// </summary>
namespace TVA.Communication
{
	[ToolboxBitmap(typeof(CommunicationServerBase)), DefaultEvent("ReceivedClientData")]
    public abstract partial class CommunicationServerBase : ICommunicationServer, IPersistSettings, ISupportInitialize
	{
		
		
		#region " Members "
		
		private string m_configurationString;
		private int m_receiveBufferSize;
		private int m_maximumClients;
		private bool m_secureSession;
		private bool m_handshake;
		private string m_handshakePassphrase;
		private EncryptLevel m_encryption;
		private CompressLevel m_compression;
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

        // We expose these two members to derived classes for their own internal use
        protected Action<byte[], int, int> m_receiveRawDataFunction;
        protected byte[] m_buffer;

		#endregion

        #region " Constants "

        /// <summary>
        /// The maximum number of bytes that can be sent from the server to clients in a single send operation.
        /// </summary>
        public const int MaximumDataSize = 524288000; // 500 MB

        /// <summary>
        /// The key used for encryption and decryption when Encryption is enabled but HandshakePassphrase is not set.
        /// </summary>
        protected const string DefaultCryptoKey = "6572a33d-826f-4d96-8c28-8be66bbc700e";

        #endregion

        #region " Events "

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
        public event EventHandler<GenericEventArgs<Exception>> ServerStartupException;
				
		/// <summary>
		/// Occurs when a client is connected to the server.
		/// </summary>
        [Description("Occurs when a client is connected to the server."), Category("Client")]
        public event EventHandler<GenericEventArgs<Guid>> ClientConnected;		
		
		/// <summary>
		/// Occurs when a client is disconnected from the server.
		/// </summary>
        [Description("Occurs when a client is disconnected from the server."), Category("Client")]
        public event EventHandler<GenericEventArgs<Guid>> ClientDisconnected;
		
		/// <summary>
		/// Occurs when data is received from a client.
		/// </summary>
        [Description("Occurs when data is received from a client."), Category("Data")]
        public event EventHandler<GenericEventArgs<IdentifiableItem<Guid, Byte[]>>> ReceivedClientData;		
		
		#endregion
			
		
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
					throw (new ArgumentOutOfRangeException("value"));
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
				if ((! value) || (value && m_handshake && m_encryption != EncryptLevel.None))
				{
					m_secureSession = value;
				}
				else
				{
					throw (new InvalidOperationException("Handshake and Encryption must be enabled in order to use SecureSession."));
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
				if ((value) || (! value && ! m_secureSession))
				{
					m_handshake = value;
					if (! m_handshake)
					{
						// Handshake passphrase will have no effect if handshaking is disabled.
						m_handshakePassphrase = "";
					}
				}
				else
				{
					throw (new ArgumentException("SecureSession must be disabled before disabling Handshake."));
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
				if (! string.IsNullOrEmpty(m_handshakePassphrase))
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
				{
					throw (new InvalidOperationException("Cannot change receive buffer size while server is running"));
				}
				if (value > 0)
				{
					m_receiveBufferSize = value;
                    m_buffer = new byte[value];
				}
				else
				{
					throw (new ArgumentOutOfRangeException("value"));
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
		[Description("The encryption level to be used for encrypting the data exchanged between the server and clients."), Category("Data"), DefaultValue(typeof(TVA.Security.Cryptography.EncryptLevel), "None")]
        public virtual EncryptLevel Encryption
		{
			get
			{
				return m_encryption;
			}
			set
			{
				if ((! m_secureSession) || (m_secureSession && value != EncryptLevel.None))
				{
					m_encryption = value;
				}
				else
				{
					throw (new ArgumentException("SecureSession session must be disabled before disabling Encryption."));
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
		[Description("The compression level to be used for compressing the data exchanged between the server and clients."), Category("Data"), DefaultValue(typeof(TVA.IO.Compression.CompressLevel), "NoCompression")]
        public virtual CompressLevel Compression
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
			protected set
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
				double serverRunTime;
				if (m_startTime > 0)
				{
					if (m_isRunning) // Server is running.
					{
						serverRunTime = TVA.DateTime.Common.TicksToSeconds(System.DateTime.Now.Ticks - m_startTime);
					}
					else // Server is not running.
					{
						serverRunTime = TVA.DateTime.Common.TicksToSeconds(m_stopTime - m_startTime);
					}
				}
				return serverRunTime;
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
		/// <param name="size">The number of bytes to be sent.</param>
		public virtual void SendTo(System.Guid clientID, byte[] data, int offset, int size)
		{
			if (m_enabled && m_isRunning)
			{
				if (data == null)
				{
					throw (new ArgumentNullException("data"));
				}
				if (size > 0)
				{
					// JRC - 06/29/2007: Expression evaluation is faster than a function call, so we're only calling "GetPreparedData" if we need to...
					if (m_compression == CompressLevel.NoCompression && m_encryption == EncryptLevel.None)
					{
						// We only grab needed portion of buffer - otherwise we use entire buffer
						if (offset != 0 || size != data.Length)
						{
							data = TVA.IO.Common.CopyBuffer(data, offset, size);
						}
					}
					else
					{
						// Pre-condition data as needed (compression, encryption, etc.)
						data = GetPreparedData(TVA.IO.Common.CopyBuffer(data, offset, size));
					}
					
					if (data.Length<= MaximumDataSize)
					{
						// PCP - 05/24/2007: Reverting to synchronous send to avoid out-of-sequence transmissions.
						SendPreparedDataTo(clientID, data);
						
						// JRC: Removed reflective thread invocation and changed to thread pool for speed...
						//   TVA.Threading.RunThread.ExecuteNonPublicMethod(Me, "SendPreparedDataTo", clientID, dataToSend)
						
						// Begin sending data on a seperate thread.
						//ThreadPool.QueueUserWorkItem(AddressOf SendPreparedDataTo, New Object() {clientID, dataToSend})
					}
					else
					{
						// Prepared data is too large to be sent.
						throw (new ArgumentException("Size of the data to be sent exceeds the maximum data size of " + MaximumDataSize + " bytes."));
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
		/// <param name="size">The number of bytes to be sent.</param>
		public virtual void Multicast(byte[] data, int offset, int size)
		{
			if (m_enabled && m_isRunning)
			{
				if (data == null)
				{
					throw (new ArgumentNullException("data"));
				}
				if (size > 0)
				{
					// JRC - 06/29/2007: Expression evaluation is faster than a function call, so we're only calling "GetPreparedData" if we need to...
					if (m_compression == CompressLevel.NoCompression && m_encryption == EncryptLevel.None)
					{
						// We only grab needed portion of buffer - otherwise we use entire buffer
						if (offset != 0 || size != data.Length)
						{
							data = TVA.IO.Common.CopyBuffer(data, offset, size);
						}
					}
					else
					{
						// Pre-condition data as needed (compression, encryption, etc.)
						data = GetPreparedData(TVA.IO.Common.CopyBuffer(data, offset, size));
					}
					
					if (data.Length <= MaximumDataSize)
					{
						lock(m_clientIDs)
						{
							foreach (Guid clientID in m_clientIDs)
							{
								try
								{
									// PCP - 05/24/2007: Reverting to synchronous send to avoid out-of-sequence transmissions.
									SendPreparedDataTo(clientID, data);
								}
								catch (Exception)
								{
									// In rare cases, we might encounter an exception here when the inheriting server
									// class doesn't think that the ID of the client to which it has to send the message is
									// valid (even though it is). This might happen when connection with a new client is
									// not yet complete and we're trying to send a message to the client.
								}
								
								// JRC: Removed reflective thread invocation and changed to thread pool for speed...
								//   TVA.Threading.RunThread.ExecuteNonPublicMethod(Me, "SendPreparedDataTo", clientID, dataToSend)
								
								// Begin sending data on a seperate thread.
								//ThreadPool.QueueUserWorkItem(AddressOf SendPreparedDataTo, New Object() {clientID, dataToSend})
							}
						}
					}
					else
					{
						// Prepared data is too large to be sent.
						throw (new ArgumentException("Size of the data to be sent exceeds the maximum data size of " + MaximumDataSize + " bytes."));
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
			lock(m_clientIDs)
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
        public abstract void DisconnectOne(System.Guid clientID);

        private bool m_previouslyEnabled = false;

        [Browsable(false)]
        public string Name
        {
            get
            {
                return this.GetType().Name;
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
                StringBuilder response = new StringBuilder();
                response.Append("                 Server ID: ");
                response.Append(m_serverID.ToString());
                response.AppendLine();
                response.Append("              Server state: ");
                response.Append(m_isRunning ? "Running" : "Not Running");
                response.AppendLine();
                response.Append("            Server runtime: ");
                response.Append(TVA.DateTime.Common.SecondsToText(RunTime));
                response.AppendLine();
                response.Append("      Configuration string: ");
                response.Append(m_configurationString);
                response.AppendLine();
                response.Append("        Subscribed clients: ");
                lock (m_clientIDs)
                {
                    response.Append(m_clientIDs.Count());
                }
                response.AppendLine();
                response.Append("           Maximum clients: ");
                response.Append(m_maximumClients == -1 ? "Infinite" : m_maximumClients.ToString());
                response.AppendLine();
                response.Append("            Receive buffer: ");
                response.Append(m_receiveBufferSize.ToString());
                response.AppendLine();
                response.Append("        Transport protocol: ");
                response.Append(m_protocol.ToString());
                response.AppendLine();
                response.Append("        Text encoding used: ");
                response.Append(m_textEncoding.EncodingName);
                response.AppendLine();

                return response.ToString();
            }
        }

        public virtual void ProcessStateChanged(string processName, Services.ProcessState newState)
        {

        }

        public virtual void ServiceStateChanged(ServiceState newState)
        {
            if (newState == ServiceState.Started)
            {
                this.Start();
            }
            else if ((newState == ServiceState.Stopped) || (newState == ServiceState.Shutdown))
            {
                this.Stop();
            }
            else if (newState == ServiceState.Paused)
            {
                m_previouslyEnabled = this.Enabled;
                this.Enabled = false;
            }
            else if (newState == ServiceState.Resumed)
            {
                this.Enabled = m_previouslyEnabled;
            }
            else if (newState == ServiceState.Shutdown)
            {
                this.Dispose();
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
                if (! string.IsNullOrEmpty(value))
                {
                    m_settingsCategoryName = value;
                }
                else
                {
                    throw (new ArgumentNullException("SettingsCategoryName"));
                }
            }
        }

        public virtual void LoadSettings()
        {
            try
            {
                TVA.Configuration.CategorizedSettingsElement config = TVA.Configuration.Common.CategorizedSettings(m_settingsCategoryName);
                if (config.Count > 0)
                {
                    ConfigurationString = config.Item("ConfigurationString").GetTypedValue(m_configurationString);
                    ReceiveBufferSize = config.Item("ReceiveBufferSize").GetTypedValue(m_receiveBufferSize);
                    MaximumClients = config.Item("MaximumClients").GetTypedValue(m_maximumClients);
                    SecureSession = config.Item("SecureSession").GetTypedValue(m_secureSession);
                    Handshake = config.Item("Handshake").GetTypedValue(m_handshake);
                    HandshakePassphrase = config.Item("HandshakePassphrase").GetTypedValue(m_handshakePassphrase);
                    Encryption = config.Item("Encryption").GetTypedValue(m_encryption);
                    Compression = config.Item("Compression").GetTypedValue(m_compression);
                    Enabled = config.Item("Enabled").GetTypedValue(m_enabled);
                }
            }
            catch (Exception)
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
                    TVA.Configuration.CategorizedSettingsElement with_1 = TVA.Configuration.Common.CategorizedSettings.(m_settingsCategoryName);
                    with_1.Clear();
                    object with_2 = with_1.Item("ConfigurationString", true);
                    with_2.Value = m_configurationString;
                    with_2.Description = "Data required by the server to initialize.";
                    object with_3 = with_1.Item("ReceiveBufferSize", true);
                    with_3.Value = m_receiveBufferSize.ToString();
                    with_3.Description = "Maximum number of bytes that can be received at a time by the server from the clients.";
                    object with_4 = with_1.Item("MaximumClients", true);
                    with_4.Value = m_maximumClients.ToString();
                    with_4.Description = "Maximum number of clients that can connect to the server.";
                    object with_5 = with_1.Item("SecureSession", true);
                    with_5.Value = m_secureSession.ToString();
                    with_5.Description = "True if the data exchanged between the server and clients will be encrypted using a private session passphrase; otherwise False.";
                    object with_6 = with_1.Item("Handshake", true);
                    with_6.Value = m_handshake.ToString();
                    with_6.Description = "True if the server will do a handshake with the client; otherwise False.";
                    object with_7 = with_1.Item("HandshakePassphrase", true);
                    with_7.Value = m_handshakePassphrase;
                    with_7.Description = "Passpharse that the clients must provide for authentication during the handshake process.";
                    object with_8 = with_1.Item("Encryption", true);
                    with_8.Value = m_encryption.ToString();
                    with_8.Description = "Encryption level (None; Level1; Level2; Level3; Level4) to be used for encrypting the data exchanged between the server and clients.";
                    object with_9 = with_1.Item("Compression", true);
                    with_9.Value = m_compression.ToString();
                    with_9.Description = "Compression level (NoCompression; DefaultCompression; BestSpeed; BestCompression; MultiPass) to be used for compressing the data exchanged between the server and clients.";
                    object with_10 = with_1.Item("Enabled", true);
                    with_10.Value = m_enabled.ToString();
                    with_10.Description = "True if the server is enabled; otherwise False.";
                    TVA.Configuration.Common.SaveSettings();
                }
                catch (Exception)
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
            m_startTime = System.DateTime.Now.Ticks; // Save the time when server is started.
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
            m_stopTime = System.DateTime.Now.Ticks; // Save the time when server is stopped.
            if (ServerStopped != null) ServerStopped(this, e);
        }

        /// <summary>
        /// Raises the TVA.Communication.ServerBase.ServerStartupException event.
        /// </summary>
        /// <param name="e">A TVA.ExceptionEventArgs that contains the event data.</param>
        /// <remarks>This method is to be called if the server throws an exception during startup.</remarks>
        protected virtual void OnServerStartupException(Exception e)
        {
            if (ServerStartupException != null) ServerStartupException(this, new GenericEventArgs<Exception>(e));
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
            if (ClientConnected != null) ClientConnected(this, new GenericEventArgs<Guid>(e));
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
            if (ClientDisconnected != null) ClientDisconnected(this, new GenericEventArgs<Guid>(e));
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
            catch (Exception)
            {
                // We'll just pass on the data that we received.
            }
            if (ReceivedClientData != null) ReceivedClientData(this, new GenericEventArgs<IdentifiableItem<Guid, byte[]>>(e));
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the specified data and returns it.
        /// </summary>
        /// <param name="data">The data on which compression and encryption is to be performed.</param>
        /// <returns>Compressed and encrypted data.</returns>
        /// <remarks>No encryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
        protected virtual byte[] GetPreparedData(byte[] data)
        {
            if (m_compression != CompressLevel.NoCompression)
            {
                data = CompressData(data, m_compression);
            }
            if (! m_secureSession)
            {
                string key = m_handshakePassphrase;
                if (string.IsNullOrEmpty(key))
                {
                    key = DefaultCryptoKey;
                }
                data = EncryptData(data, key, m_encryption);
            }

            return data;
        }

        /// <summary>
        /// Performs the necessary uncompression and decryption on the specified data and returns it.
        /// </summary>
        /// <param name="data">The data on which uncompression and decryption is to be performed.</param>
        /// <returns>Uncompressed and decrypted data.</returns>
        /// <remarks>No decryption is performed if SecureSession is enabled, even if Encryption is enabled.</remarks>
        protected virtual byte[] GetActualData(byte[] data)
        {
            if (! m_secureSession)
            {
                string key = m_handshakePassphrase;
                if (string.IsNullOrEmpty(key))
                {
                    key = DefaultCryptoKey;
                }
                data = Cryptography.Common.Decrypt(data, key, m_encryption);
            }
            if (m_compression != CompressLevel.NoCompression)
            {
                data = Compression.Common.UncompressData(data, m_compression);
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
	}	
}