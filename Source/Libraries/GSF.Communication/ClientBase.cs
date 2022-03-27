//******************************************************************************************************
//  ClientBase.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed client data access.
//  11/30/2007 - Pinal C. Patel
//       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
//       instead of DesignMode property as the former is more accurate than the latter.
//  02/19/2008 - Pinal C. Patel
//       Added code to detect and avoid redundant calls to Dispose().
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  06/18/2009 - Pinal C. Patel
//       Fixed the implementation of Enabled property.
//  07/02/2009 - Pinal C. Patel
//       Modified state altering properties to reconnect the client when changed.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  07/15/2009 - Pinal C. Patel
//       Modified Connect() to wait for post-connection processing to complete.
//  07/17/2009 - Pinal C. Patel
//       Modified SharedSecret to be persisted as an encrypted value.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/29/2010 - Pinal C. Patel
//       Updated the implementation of Connect() method so it blocks correctly after updates made to 
//       ConnectAsync() method in the derived classes.
//  04/14/2011 - Pinal C. Patel
//       Updated to use new serialization methods in GSF.Serialization class.
//  12/02/2011 - J. Ritchie Carroll
//       Updated event data publication to provide "copy" of reusable buffer instead of original
//       buffer since you cannot assume how user will use the buffer (they may cache it).
//  12/29/2011 - J. Ritchie Carroll
//       Updated Status property to show ConnectionString information.
//  04/26/2012 - Pinal C. Patel
//       Updated Create() static method to apply settings from the connection string to the created 
//       client instance using reflection.
//  05/22/2015 - J. Ritchie Carroll
//       Added ZeroMQ to the create IClient options.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Units;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.Communication
{
    /// <summary>
    /// Base class for a client involved in server-client communication.
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
        /// Specifies the default value for the <see cref="SendBufferSize"/> property.
        /// </summary>
        public const int DefaultSendBufferSize = 32768;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveBufferSize"/> property.
        /// </summary>
        public const int DefaultReceiveBufferSize = 32768;

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
        [Category("Connection")]
        [Description("Occurs when client is attempting connection to the server.")]
        public event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when client connection to the server is established.
        /// </summary>
        [Category("Connection")]
        [Description("Occurs when client connection to the server is established.")]
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when client connection to the server is terminated.
        /// </summary>
        [Category("Connection")]
        [Description("Occurs when client connection to the server is terminated")]
        public event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to the server.
        /// </summary>
        [Category("Connection")]
        [Description("Occurs when an Exception is encountered during connection attempt to the server.")]
        public event EventHandler<EventArgs<Exception>> ConnectionException;

        /// <summary>
        /// Occurs when the client begins sending data to the server.
        /// </summary>
        [Category("Data")]
        [Description("Occurs when the client begins sending data to the server.")]
        public event EventHandler SendDataStart;

        /// <summary>
        /// Occurs when the client has successfully sent data to the server.
        /// </summary>
        [Category("Data")]
        [Description("Occurs when the client has successfully sent data to the server.")]
        public event EventHandler SendDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending data to the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when sending data to the server.
        /// </remarks>
        [Category("Data")]
        [Description("Occurs when an Exception is encountered when sending data to the server.")]
        public event EventHandler<EventArgs<Exception>> SendDataException;

        /// <summary>
        /// Occurs when unprocessed data has been received from the server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event can be used to receive a notification that server data has arrived. The <see cref="Read"/> method can then be used
        /// to copy data to an existing buffer. In many cases it will be optimal to use an existing buffer instead of subscribing to the
        /// <see cref="ReceiveDataComplete"/> event.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the number of bytes received in the buffer from the server.
        /// </para>
        /// </remarks>
        [Category("Data")]
        [Description("Occurs when unprocessed data has been received from the server.")]
        public event EventHandler<EventArgs<int>> ReceiveData;

        /// <summary>
        /// Occurs when data received from the server has been processed and is ready for consumption.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is a new buffer containing post-processed data received from the server starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of post-processed bytes received in the buffer from the server.
        /// </remarks>
        [Category("Data")]
        [Description("Occurs when data received from the server has been processed and is ready for consumption.")]
        public event EventHandler<EventArgs<byte[], int>> ReceiveDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when receiving data from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when receiving data from the server.
        /// </remarks>
        [Category("Data")]
        [Description("Occurs when an Exception is encountered when receiving data from the server.")]
        public event EventHandler<EventArgs<Exception>> ReceiveDataException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered in a user-defined function via an event dispatch.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> thrown by the user-defined function.
        /// </remarks>
        [Category("User")]
        [Description("Occurs when an Exception is encountered when calling a user-defined function.")]
        public event EventHandler<EventArgs<Exception>> UnhandledUserException;

        // Fields
        private string m_connectionString;
        private int m_maxConnectionAttempts;
        private int m_sendBufferSize;
        private int m_receiveBufferSize;
        private string m_settingsCategory;
        private Encoding m_textEncoding;
        private ClientState m_currentState;
        private readonly TransportProtocol m_transportProtocol;
        private Ticks m_connectTime;
        private Ticks m_disconnectTime;
        private ManualResetEvent m_connectHandle;
        private bool m_initialized;

        private readonly Action<int> m_updateBytesSent;
        private readonly Action<int> m_updateBytesReceived;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the client.
        /// </summary>
        protected ClientBase()
        {
            m_textEncoding = Encoding.ASCII;
            m_currentState = ClientState.Disconnected;
            m_maxConnectionAttempts = DefaultMaxConnectionAttempts;
            m_sendBufferSize = DefaultSendBufferSize;
            m_receiveBufferSize = DefaultReceiveBufferSize;
            PersistSettings = DefaultPersistSettings;
            m_settingsCategory = DefaultSettingsCategory;
            Statistics = new();
            m_updateBytesSent = TrackStatistics ? UpdateBytesSent : new(_ => { });
            m_updateBytesReceived = TrackStatistics ? UpdateBytesReceived : new(_ => { });
        }

        /// <summary>
        /// Initializes a new instance of the client.
        /// </summary>
        /// <param name="transportProtocol">One of the <see cref="TransportProtocol"/> values.</param>
        /// <param name="connectionString">The data used by the client for connection to a server.</param>
        protected ClientBase(TransportProtocol transportProtocol, string connectionString) : this()
        {
            m_transportProtocol = transportProtocol;
            ConnectionString = connectionString;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the server URI.
        /// </summary>
        [Browsable(false)]
        public abstract string ServerUri { get; }

        /// <summary>
        /// Gets the current server index, when multiple server end points are defined.
        /// </summary>
        [Browsable(false)]
        public int ServerIndex { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the data required by the client to connect to the server.
        /// </summary>
        [Category("Settings")]
        [Description("The data required by the client to connect to the server.")]
        public virtual string ConnectionString
        {
            get => m_connectionString;
            set
            {
                ValidateConnectionString(value);

                m_connectionString = value;
                ReConnect();
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times the client will attempt to connect to the server.
        /// </summary>
        /// <remarks>Set <see cref="MaxConnectionAttempts"/> to -1 for infinite connection attempts.</remarks>
        [Category("Settings")]
        [DefaultValue(DefaultMaxConnectionAttempts)]
        [Description("The maximum number of times the client will attempt to connect to the server. Set MaxConnectionAttempts to -1 for infinite connection attempts.")]
        public virtual int MaxConnectionAttempts
        {
            get => m_maxConnectionAttempts;
            set => m_maxConnectionAttempts = value < 1 ? -1 : value;
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for sending data to the server.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
        [Category("Data")]
        [DefaultValue(DefaultSendBufferSize)]
        [Description("The size of the buffer used by the client for receiving data from the server.")]
        public virtual int SendBufferSize
        {
            get => m_sendBufferSize;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative");

                m_sendBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
        [Category("Data")]
        [DefaultValue(DefaultReceiveBufferSize)]
        [Description("The size of the buffer used by the client for receiving data from the server.")]
        public virtual int ReceiveBufferSize
        {
            get => m_receiveBufferSize;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative");

                m_receiveBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client settings are to be saved to the config file.
        /// </summary>
        [Category("Persistence")]
        [DefaultValue(DefaultPersistSettings)]
        [Description("Indicates whether the client settings are to be saved to the config file.")]
        public bool PersistSettings { get; set; }

        /// <summary>
        /// Gets or sets the category under which the client settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        [Category("Persistence")]
        [DefaultValue(DefaultSettingsCategory)]
        [Description("Category under which the client settings are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get => m_settingsCategory;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

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
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get => m_currentState == ClientState.Connected;
            set
            {
                if (value && !Enabled)
                    Connect();
                else if (!value && Enabled)
                    Disconnect();
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get => m_textEncoding;
            set => m_textEncoding = value;
        }

        /// <summary>
        /// Gets the current <see cref="ClientState"/>.
        /// </summary>
        [Browsable(false)]
        public virtual ClientState CurrentState => m_currentState;

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
        /// </summary>
        [Browsable(false)]
        public virtual TransportProtocol TransportProtocol => m_transportProtocol;

        /// <summary>
        /// Gets the <see cref="Time"/> for which the client has been connected to the server.
        /// </summary>
        [Browsable(false)]
        public virtual Time ConnectionTime
        {
            get
            {
                Time clientConnectionTime = 0.0D;

                if (m_connectTime > 0)
                {
                    if (m_currentState == ClientState.Connected)
                    {
                        // Client is connected to the server.
                        clientConnectionTime = (DateTime.UtcNow.Ticks - m_connectTime).ToSeconds();
                    }
                    else
                    {
                        // Client is not connected to the server.
                        clientConnectionTime = (m_disconnectTime - m_connectTime).ToSeconds();
                    }
                }

                return clientConnectionTime;
            }
        }

        /// <summary>
        /// Gets or sets current read index for received data buffer incremented at each <see cref="Read"/> call.
        /// </summary>
        protected int ReadIndex { get; set; }

        /// <summary>
        /// Gets the unique identifier of the client.
        /// </summary>
        [Browsable(false)]
        public virtual string Name => m_settingsCategory;

        /// <summary>
        /// Gets the <see cref="TransportStatistics"/> for the client connection.
        /// </summary>
        public TransportStatistics Statistics { get; }

        /// <summary>
        /// Determines whether the base class should track statistics.
        /// </summary>
        protected virtual bool TrackStatistics => true;

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        [Browsable(false)]
        public virtual string Status
        {
            get
            {
                StringBuilder status = new();

                status.Append("              Client state: ");
                status.Append(m_currentState);
                status.AppendLine();
                status.Append("           Connection time: ");
                status.Append(ConnectionTime.ToString(3));
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

        /// <summary>
        /// When overridden in a derived class, reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ReceiveData"/> event handler. Calling this method outside this event
        /// will have unexpected results.
        /// </remarks>
        public abstract int Read(byte[] buffer, int startIndex, int length);

        /// <summary>
        /// Requests that the client attempt to move to the next <see cref="ServerIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Return value will only be <c>true</c> if <see cref="ServerIndex"/> changed.
        /// </remarks>
        public virtual bool RequestNextServerIndex() => false;

        /// <summary>
        /// When overridden in a derived class, validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to be validated.</param>
        protected abstract void ValidateConnectionString(string connectionString);

        /// <summary>
        /// When overridden in a derived class, sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected abstract WaitHandle SendDataAsync(byte[] data, int offset, int length);

        /// <summary>
        /// Initializes the client.
        /// </summary>
        /// <remarks>
        /// <see cref="Initialize()"/> is to be called by user-code directly only if the client is not consumed through the designer surface of the IDE.
        /// </remarks>
        public void Initialize()
        {
            if (m_initialized)
                return;

            LoadSettings();         // Load settings from the config file.
            m_initialized = true;   // Initialize only once.
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
            if (DesignMode)
                return;

            try
            {
                Initialize();
            }
            catch (Exception)
            {
                // Prevent the IDE from crashing when component is in design mode.
            }
        }

        /// <summary>
        /// Saves client settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void SaveSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

            settings["ConnectionString", true].Update(m_connectionString);
            settings["MaxConnectionAttempts", true].Update(m_maxConnectionAttempts);
            settings["SendBufferSize", true].Update(m_sendBufferSize);
            settings["ReceiveBufferSize", true].Update(m_receiveBufferSize);

            config.Save();
        }

        /// <summary>
        /// Loads saved client settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public virtual void LoadSettings()
        {
            if (!PersistSettings)
                return;

            // Ensure that settings category is specified.
            if (string.IsNullOrEmpty(m_settingsCategory))
                throw new ConfigurationErrorsException("SettingsCategory property has not been set");

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

            settings.Add("ConnectionString", m_connectionString, "Data required by the client to connect to the server.");
            settings.Add("MaxConnectionAttempts", m_maxConnectionAttempts, "Maximum number of times the client will attempt to connect to the server.");
            settings.Add("SendBufferSize", m_sendBufferSize, "Size of the buffer used by the client for sending data from the server.");
            settings.Add("ReceiveBufferSize", m_receiveBufferSize, "Size of the buffer used by the client for receiving data from the server.");

            ConnectionString = settings["ConnectionString"].ValueAs(m_connectionString);
            MaxConnectionAttempts = settings["MaxConnectionAttempts"].ValueAs(m_maxConnectionAttempts);
            SendBufferSize = settings["SendBufferSize"].ValueAs(m_sendBufferSize);
            ReceiveBufferSize = settings["ReceiveBufferSize"].ValueAs(m_receiveBufferSize);
        }

        /// <summary>
        /// Connects the client to the server synchronously.
        /// </summary>
        public virtual void Connect()
        {
            // Start asynchronous connection attempt.
            ConnectAsync();

            // Block until connection is established.
            do
            {
                Thread.Sleep(100);
            }
            while (m_currentState == ClientState.Connecting);
        }

        /// <summary>
        /// Connects the client to the server asynchronously.
        /// </summary>
        /// <exception cref="FormatException">Server property in <see cref="ConnectionString"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the client when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        /// <remarks>
        /// Derived classes are expected to override this method with protocol specific connection operations. Call the base class
        /// method to obtain an operational wait handle if protocol connection operation doesn't provide one already.
        /// </remarks>
        public virtual WaitHandle ConnectAsync()
        {
            if (CurrentState != ClientState.Disconnected)
                throw new InvalidOperationException("Client is currently not disconnected");

            // Initialize if uninitialized.
            if (!m_initialized)
                Initialize();

            // Set up connection event wait handle
            m_connectHandle = new(false);
            return m_connectHandle;
        }

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        public virtual void Send(string data) => Send(m_textEncoding.GetBytes(data));

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        public virtual void Send(object serializableObject) => Send(Serialization.Serialize(serializableObject, SerializationFormat.Binary));

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        public virtual void Send(byte[] data) => Send(data, 0, data.Length);

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        public virtual void Send(byte[] data, int offset, int length) => SendAsync(data, offset, length).WaitOne();

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The plain-text data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(string data) => SendAsync(m_textEncoding.GetBytes(data));

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="serializableObject">The serializable object that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(object serializableObject) => SendAsync(Serialization.Serialize(serializableObject, SerializationFormat.Binary));

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The binary data that is to be sent.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(byte[] data) => SendAsync(data, 0, data.Length);

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public virtual WaitHandle SendAsync(byte[] data, int offset, int length)
        {
            if (m_currentState != ClientState.Connected)
                throw new InvalidOperationException("Client is not connected");

            // Update transport statistics
            m_updateBytesSent(length);

            // Initiate send operation
            return SendDataAsync(data, offset, length);
        }

        /// <summary>
        /// When overridden in a derived class, disconnects client from the server synchronously.
        /// </summary>
        public virtual void Disconnect() => m_currentState = ClientState.Disconnected;

        /// <summary>
        /// Updates the <see cref="Statistics"/> pertaining to bytes sent.
        /// </summary>
        /// <param name="bytes"></param>
        protected void UpdateBytesSent(int bytes)
        {
            Statistics.LastSend = DateTime.UtcNow;
            Statistics.LastBytesSent = bytes;
            Statistics.TotalBytesSent += bytes;
        }

        /// <summary>
        /// Updates the <see cref="Statistics"/> pertaining to bytes received.
        /// </summary>
        /// <param name="bytes"></param>
        protected void UpdateBytesReceived(int bytes)
        {
            Statistics.LastReceive = DateTime.UtcNow;
            Statistics.LastBytesReceived = bytes;
            Statistics.TotalBytesReceived += bytes;
        }

        /// <summary>
        /// Raises the <see cref="ConnectionAttempt"/> event.
        /// </summary>
        protected virtual void OnConnectionAttempt()
        {
            try
            {
                m_currentState = ClientState.Connecting;
                ConnectionAttempt?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected virtual void OnConnectionEstablished()
        {
            try
            {
                m_currentState = ClientState.Connected;
                m_disconnectTime = 0;

                // Save the time when the client connected to the server.
                m_connectTime = DateTime.UtcNow.Ticks;

                // Signal any waiting threads about successful connection.
                m_connectHandle?.Set();

                ConnectionEstablished?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionTerminated"/> event.
        /// </summary>
        protected virtual void OnConnectionTerminated()
        {
            try
            {
                m_currentState = ClientState.Disconnected;

                // Save the time when client was disconnected from the server.
                m_disconnectTime = DateTime.UtcNow.Ticks;

                ConnectionTerminated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ConnectionException"/> event.</param>
        protected virtual void OnConnectionException(Exception ex)
        {
            try
            {
                // Move to next server connection, when multiple server end points are defined
                RequestNextServerIndex();

                if (ex is not ObjectDisposedException)
                    ConnectionException?.Invoke(this, new(ex));
            }
            catch (Exception userEx)
            {
                OnUnhandledUserException(userEx);
            }
        }

        /// <summary>
        /// Raises the <see cref="SendDataStart"/> event.
        /// </summary>
        protected virtual void OnSendDataStart()
        {
            try
            {
                SendDataStart?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="SendDataComplete"/> event.
        /// </summary>
        protected virtual void OnSendDataComplete()
        {
            try
            {
                SendDataComplete?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="SendDataException"/> event.</param>
        protected virtual void OnSendDataException(Exception ex)
        {
            try
            {
                if (ex is not ObjectDisposedException)
                    SendDataException?.Invoke(this, new(ex));
            }
            catch (Exception userEx)
            {
                OnUnhandledUserException(userEx);
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveData"/> event.
        /// </summary>
        /// <param name="size">Number of bytes received from the client.</param>
        /// <remarks>
        /// This event is automatically raised by call to <see cref="OnReceiveDataComplete"/> so that inheritors
        /// never need to worry about raising this event. This method is only included here in case any custom client
        /// implementations need to explicitly raise this event.
        /// </remarks>
        protected virtual void OnReceiveData(int size)
        {
            try
            {
                ReceiveData?.Invoke(this, new(size));
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataComplete"/> event.
        /// </summary>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        protected virtual void OnReceiveDataComplete(byte[] data, int size)
        {
            try
            {
                // Update transport statistics
                m_updateBytesReceived(size);

                // Reset buffer index used by read method
                ReadIndex = 0;

                // Notify users of data ready
                ReceiveData?.Invoke(this, new(size));

                // Most inheritors of this class "reuse" an existing buffer, as such you cannot assume what the user is going to do
                // with the buffer provided, so we pass in a "copy" of the buffer for the user since they may assume control of and
                // possibly even cache the provided buffer (e.g., passing the buffer to a process queue)
                ReceiveDataComplete?.Invoke(this, new(data.BlockCopy(0, size), size));
            }
            catch (Exception ex)
            {
                OnUnhandledUserException(ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ReceiveDataException"/> event.</param>
        protected virtual void OnReceiveDataException(Exception ex)
        {
            try
            {
                if (ex is not ObjectDisposedException)
                    ReceiveDataException?.Invoke(this, new(ex));
            }
            catch (Exception userEx)
            {
                OnUnhandledUserException(userEx);
            }
        }

        /// <summary>
        /// Raises the <see cref="UnhandledUserException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="UnhandledUserException"/> event.</param>
        protected virtual void OnUnhandledUserException(Exception ex)
        {
            try
            {
                UnhandledUserException?.Invoke(this, new(ex));
            }
            catch (Exception userEx)
            {
                // Suppress exceptions in user-defined exception handling
                // code, as there's nothing we can reasonably do about it.
                Logger.SwallowException(userEx);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the client and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_connectHandle", Justification = "Field is properly disposed")]
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                if (!disposing)
                    return;

                Disconnect();
                SaveSettings();

                m_connectHandle?.Dispose();
            }
            finally
            {
                IsDisposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Re-connects the client if currently connected.
        /// </summary>
        private void ReConnect()
        {
            if (m_currentState != ClientState.Connected)
                return;

            Disconnect();

            while (m_currentState != ClientState.Disconnected)
                Thread.Sleep(100);

            Connect();
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Create a communications client
        /// </summary>
        /// <remarks>
        /// Note that typical connection string should be prefixed with a "protocol=tcp", "protocol=udp", "protocol=serial" or "protocol=file"
        /// </remarks>
        /// <returns>A communications client.</returns>
        /// <param name="connectionString">Connection string for the client.</param>
        public static IClient Create(string connectionString)
        {
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
            IClient client;

            if (settings.TryGetValue("protocol", out string protocol))
            {
                settings.Remove("protocol");
                StringBuilder protocolSettings = new();

                foreach (string key in settings.Keys)
                {
                    protocolSettings.Append(key);
                    protocolSettings.Append("=");
                    protocolSettings.Append(settings[key]);
                    protocolSettings.Append(";");
                }

                // Create a client instance for the specified protocol.
                client = protocol.Trim().ToLower() switch
                {
                    "tls" => new TlsClient(protocolSettings.ToString()),
                    "tcp" => new TcpClient(protocolSettings.ToString()),
                    "udp" => new UdpClient(protocolSettings.ToString()),
                    "file" => new FileClient(protocolSettings.ToString()),
                    "serial" => new SerialClient(protocolSettings.ToString()),
                    "zeromq" => new ZeroMQClient(protocolSettings.ToString()),
                    _ => throw new ArgumentException($"{protocol} is not a valid transport protocol"),
                };

                // Apply client settings from the connection string to the client.
                foreach (KeyValuePair<string, string> setting in settings)
                {
                    PropertyInfo property = client.GetType().GetProperty(setting.Key);
                    property?.SetValue(client, Convert.ChangeType(setting.Value, property.PropertyType), null);
                }
            }
            else
            {
                throw new ArgumentException("Transport protocol must be specified");
            }

            return client;
        }

        #endregion
    }
}