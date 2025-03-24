//******************************************************************************************************
//  DataListener.cs - Gbtc
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
//  06/12/2007 - Pinal C. Patel
//       Generated original version of source code.
//  11/26/2007 - Pinal C. Patel
//       Added overloaded Data property to retrieve current data for a point.
//  01/15/2008 - Pinal C. Patel
//       Removed logic to timeout waiting for TCP/UDP connection to complete. As a result the 
//       ListenerStartFailure event no longer exists.
//  04/02/2008 - Pinal C. Patel
//       Added SocketConnecting event to notify that socket connection is being attempted.
//  04/23/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/15/2009 - Pinal C. Patel
//       Made caching of data locally optional so DataListener can be used just for getting real-time
//       time-series data that is now being made available via the new DataExtracted event.
//  09/17/2009 - Pinal C. Patel
//       Added check to prevent raising DataExtracted and DataChanged events if no time-series data
//       was present in the received packets.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Threading;
using GSF.Communication;
using GSF.Configuration;
using GSF.Historian.Files;
using GSF.Historian.Packets;
using GSF.Parsing;
using GSF.Units;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable InconsistentlySynchronizedField

namespace GSF.Historian;

/// <summary>
/// Represents a listener that can receive time-series data in real-time using <see cref="System.Net.Sockets.Socket"/>s.
/// </summary>
/// <seealso cref="IDataPoint"/>
/// <seealso cref="PacketParser"/>
[ToolboxBitmap(typeof(DataListener))]
public class DataListener : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Specifies the default value for the <see cref="ID"/> property.
    /// </summary>
    public const string DefaultID = "Default";

    /// <summary>
    /// Specifies the default value for the <see cref="Server"/> property.
    /// </summary>
    public const string DefaultServer = "localhost";

    /// <summary>
    /// Specifies the default value for the <see cref="Port"/> property.
    /// </summary>
    public const int DefaultPort = 1004;

    /// <summary>
    /// Specifies the default value for the <see cref="Protocol"/> property.
    /// </summary>
    public const TransportProtocol DefaultProtocol = TransportProtocol.Udp;

    /// <summary>
    /// Specifies the default value for the <see cref="ConnectToServer"/> property.
    /// </summary>
    public const bool DefaultConnectToServer = true;

    /// <summary>
    /// Specifies the default value for the <see cref="CacheData"/> property.
    /// </summary>
    public const bool DefaultCacheData = true;

    /// <summary>
    /// Specifies the default value for the <see cref="InitializeData"/> property.
    /// </summary>
    public const bool DefaultInitializeData = true;

    /// <summary>
    /// Specifies the default value for the <see cref="InitializeDataTimeout"/> property.
    /// </summary>
    public const int DefaultInitializeDataTimeout = 60000;

    /// <summary>
    /// Specifies the default value for the <see cref="PersistSettings"/> property.
    /// </summary>
    public const bool DefaultPersistSettings = false;

    /// <summary>
    /// Specifies the default value for the <see cref="SettingsCategory"/> property.
    /// </summary>
    public const string DefaultSettingsCategory = nameof(DataListener);

    // Events

    /// <summary>
    /// Occurs when the <see cref="DataListener"/> is starting up.
    /// </summary>
    [Category("Listener")]
    [Description("Occurs when the DataListener is starting up.")]
    public event EventHandler ListenerStarting;

    /// <summary>
    /// Occurs when the <see cref="DataListener"/> has started.
    /// </summary>
    [Category("Listener")]
    [Description("Occurs when the DataListener has started.")]
    public event EventHandler ListenerStarted;

    /// <summary>
    /// Occurs when the <see cref="DataListener"/> is being stopped.
    /// </summary>
    [Category("Listener")]
    [Description("Occurs when the DataListener is being stopped.")]
    public event EventHandler ListenerStopping;

    /// <summary>
    /// Occurs when <see cref="DataListener"/> has stopped.
    /// </summary>
    [Category("Listener")]
    [Description("Occurs when DataListener has stopped.")]
    public event EventHandler ListenerStopped;

    /// <summary>
    /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is being attempted.
    /// </summary>
    [Category("Socket")]
    [Description("Occurs when the underlying Socket connection for receiving time-series data is being attempted.")]
    public event EventHandler SocketConnecting;

    /// <summary>
    /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is established.
    /// </summary>
    [Category("Socket")]
    [Description("Occurs when the underlying Socket connection for receiving time-series data is established.")]
    public event EventHandler SocketConnected;

    /// <summary>
    /// Occurs when the underlying <see cref="System.Net.Sockets.Socket"/> connection for receiving time-series data is terminated.
    /// </summary>
    [Category("Socket")]
    [Description("Occurs when the underlying Socket connection for receiving time-series data is terminated.")]
    public event EventHandler SocketDisconnected;

    /// <summary>
    /// Occurs when the <see cref="Data"/> is being populated on <see cref="Start"/>up.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when the Data is being populated on Startup.")]
    public event EventHandler DataInitStart;

    /// <summary>
    /// Occurs when the <see cref="Data"/> is populated completely on <see cref="Start"/>up.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when the Data is populated completely on Startup.")]
    public event EventHandler DataInitComplete;

    /// <summary>
    /// Occurs when the <see cref="Data"/> cannot be populated completely on <see cref="Start"/>up.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when the Data cannot be populated completely on Startup.")]
    public event EventHandler DataInitPartial;

    /// <summary>
    /// Occurs when the <see cref="Data"/> cannot be populated on <see cref="Start"/>up due to the unavailability of the <see cref="Server"/>.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when the Data cannot be populated on Startup due to the unavailability of the Server.")]
    public event EventHandler DataInitFailure;

    /// <summary>
    /// Occurs when time-series data is extracted from the received packets.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when time-series data is extracted from the received packets.")]
    public event EventHandler<EventArgs<IList<IDataPoint>>> DataExtracted;

    /// <summary>
    /// Occurs when the <see cref="Data"/> has changed.
    /// </summary>
    [Category(nameof(Data))]
    [Description("Occurs when the Data has changed.")]
    public event EventHandler DataChanged;

    // Fields
    private string m_server;
    private int m_port;
    private TransportProtocol m_protocol;
    private bool m_cacheData;
    private bool m_initializeData;
    private int m_initializeDataTimeout;
    private string m_settingsCategory;
    private readonly List<IDataPoint> m_data;
    private readonly ConcurrentDictionary<IClient, Guid> m_clientIDs;
    private bool m_listenerStopping;
    private Thread m_startupThread;
    private readonly AutoResetEvent m_initializeWaitHandle;

    private bool m_initialized;
    // WithEvents
    private readonly TcpClient m_tcpClient;
    private readonly UdpClient m_udpClient;
    private readonly TcpServer m_tcpServer;
    private readonly TcpClient m_dataInitClient;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="DataListener"/> class.
    /// </summary>
    public DataListener()
    {
        Name = DefaultID;
        m_server = DefaultServer;
        m_port = DefaultPort;
        m_protocol = DefaultProtocol;
        ConnectToServer = DefaultConnectToServer;
        m_cacheData = DefaultCacheData;
        m_initializeData = DefaultInitializeData;
        m_initializeDataTimeout = DefaultInitializeDataTimeout;
        PersistSettings = DefaultPersistSettings;
        m_settingsCategory = DefaultSettingsCategory;
        m_data = [];
        m_clientIDs = new ConcurrentDictionary<IClient, Guid>();
        m_initializeWaitHandle = new AutoResetEvent(false);

        Parser = new PacketParser();
        Parser.DataParsed += PacketParser_DataParsed;

        m_tcpClient = new TcpClient();
        m_tcpClient.ConnectionAttempt += ClientSocket_ConnectionAttempt;
        m_tcpClient.ConnectionEstablished += ClientSocket_ConnectionEstablished;
        m_tcpClient.ConnectionTerminated += ClientSocket_ConnectionTerminated;
        m_tcpClient.ReceiveDataComplete += ClientSocket_ReceiveDataComplete;
        m_clientIDs.TryAdd(m_tcpClient, Guid.NewGuid());

        m_udpClient = new UdpClient();
        m_udpClient.ConnectionAttempt += ClientSocket_ConnectionAttempt;
        m_udpClient.ConnectionEstablished += ClientSocket_ConnectionEstablished;
        m_udpClient.ConnectionTerminated += ClientSocket_ConnectionTerminated;
        m_udpClient.ReceiveDataComplete += ClientSocket_ReceiveDataComplete;
        m_clientIDs.TryAdd(m_udpClient, Guid.NewGuid());

        m_tcpServer = new TcpServer();
        m_tcpServer.ServerStarted += ServerSocket_ServerStarted;
        m_tcpServer.ServerStopped += ServerSocket_ServerStopped;
        m_tcpServer.ReceiveClientDataComplete += ServerSocket_ReceiveClientDataComplete;

        m_dataInitClient = new TcpClient();
        m_dataInitClient.ConnectionString = "Server={0}:1003; interface=0.0.0.0";
        m_dataInitClient.PayloadAware = true;
        m_dataInitClient.MaxConnectionAttempts = 10;
        m_dataInitClient.ReceiveDataComplete += DataInitClient_ReceiveDataComplete;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataListener"/> class.
    /// </summary>
    /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="DataListener"/>.</param>
    public DataListener(IContainer container)
        : this()
    {
        container?.Add(this);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the alphanumeric identifier of the <see cref="DataListener"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
    [Category(nameof(Identity))]
    [DefaultValue(DefaultID)]
    [Description("Alpha-numeric identifier of the DataListener.")]
    public string ID
    {
        get => Name;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            Name = value;
        }
    }

    /// <summary>
    /// Gets or sets the DNS name or IP address of the server from where the <see cref="DataListener"/> will get the time-series data.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
    [Category("Connection")]
    [DefaultValue(DefaultServer)]
    [Description("DNS name or IP address of the server from where the DataListener will get the time-series data.")]
    public string Server
    {
        get => m_server;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            m_server = value;
        }
    }

    /// <summary>
    /// Gets or sets the network port of the <see cref="Server"/> where the <see cref="DataListener"/> will connect to get the time-series data.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not between 0 and 65535.</exception>
    [Category("Connection")]
    [DefaultValue(DefaultPort)]
    [Description("Network port of the Server where the DataListener will connect to get the time-series data.")]
    public int Port
    {
        get => m_port;
        set
        {
            if (!Transport.IsPortNumberValid(value.ToString()))
                throw new ArgumentException("Value must be between 0 and 65535");

            m_port = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="TransportProtocol"/> to be used for receiving time-series data from the <see cref="Server"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not Tcp or Udp.</exception>
    [Category("Connection")]
    [DefaultValue(DefaultProtocol)]
    [Description("Protocol to be used for receiving time-series data from the Server.")]
    public TransportProtocol Protocol
    {
        get => m_protocol;
        set
        {
            if (value is not (TransportProtocol.Tcp or TransportProtocol.Udp))
                throw new ArgumentException("Value must be Tcp or Udp");

            m_protocol = value;
        }
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> will connect to the <see cref="Server"/> 
    /// for receiving the time-series data or the <see cref="Server"/> will make a connection to the <see cref="DataListener"/> on 
    /// the specified <see cref="Port"/> for sending time-series data.
    /// </summary>
    [Category("Connection")]
    [DefaultValue(DefaultConnectToServer)]
    [Description("Indicates whether the DataListener will connect to the Server for receiving the time-series data or the Server will make a connection to the DataListener on the specified Port for sending time-series data.")]
    public bool ConnectToServer { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the <see cref="Data"/> is to be updated with the latest time-series data.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="CacheData"/> is being disabled when <see cref="InitializeData"/> is enabled.</exception>
    [Category(nameof(Data))]
    [DefaultValue(DefaultCacheData)]
    [Description("Indicates whether the Data is to be updated with the latest time-series data.")]
    public bool CacheData
    {
        get => m_cacheData;
        set
        {
            if (!value && m_initializeData)
                throw new InvalidOperationException("CacheData cannot be disabled when InitializeData is enabled");

            m_cacheData = value;
        }
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> will initialize the <see cref="Data"/> from the <see cref="Server"/> on startup.
    /// </summary>
    /// <remarks>
    /// <see cref="InitializeData"/> should be enabled only if the <see cref="Server"/> software on port 1003 is programmed to accept <see cref="PacketType11"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><see cref="InitializeData"/> is being enabled when <see cref="CacheData"/> is disabled.</exception>
    [Category(nameof(Data))]
    [DefaultValue(DefaultInitializeData)]
    [Description("Indicates whether the DataListener will initialize the Data from the Server on startup.")]
    public bool InitializeData
    {
        get => m_initializeData;
        set
        {
            if (value && !m_cacheData)
                throw new InvalidOperationException("InitializeData cannot be enabled when CacheData is disabled");

            m_initializeData = value;
        }
    }

    /// <summary>
    /// Gets or sets the time (in milliseconds) to wait for the <see cref="Data"/> to be initialized from the <see cref="Server"/> on <see cref="Start"/>up.
    /// </summary>
    /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
    [Category(nameof(Data))]
    [DefaultValue(DefaultInitializeDataTimeout)]
    [Description("Time (in milliseconds) to wait for the Data to be initialized from the Server on Startup.")]
    public int InitializeDataTimeout
    {
        get => m_initializeDataTimeout;
        set
        {
            if (value < 1)
                throw new ArgumentException("Value must be positive");

            m_initializeDataTimeout = value;
        }
    }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the settings of <see cref="DataListener"/> are to be saved to the config file.
    /// </summary>
    [Category("Persistence")]
    [DefaultValue(DefaultPersistSettings)]
    [Description("Indicates whether the settings of DataListener are to be saved to the config file.")]
    public bool PersistSettings { get; set; }

    /// <summary>
    /// Gets or sets the category under which the settings of <see cref="DataListener"/> are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
    [Category("Persistence")]
    [DefaultValue(DefaultSettingsCategory)]
    [Description("Category under which the settings of DataListener are to be saved to the config file if the PersistSettings property is set to true.")]
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
    /// Gets or sets a boolean value that indicates whether the <see cref="DataListener"/> is currently enabled.
    /// </summary>
    /// <remarks>
    /// <see cref="Enabled"/> property is not be set by user-code directly.
    /// </remarks>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Enabled
    {
        get => RunTime != 0.0;
        set
        {
            if (value && !Enabled)
                Start();
            else if (!value && Enabled)
                Stop();
        }
    }

    /// <summary>
    /// Gets a flag that indicates whether the object has been disposed.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the newest time-series data received by the <see cref="DataListener"/>.
    /// </summary>
    /// <remarks>
    /// WARNING: <see cref="Data"/> is thread unsafe. Synchronized access is required.
    /// </remarks>
    [Browsable(false)]
    public IList<IDataPoint> Data => m_data.AsReadOnly();

    /// <summary>
    /// Gets the underlying <see cref="PacketParser"/> used the <see cref="DataListener"/> for extracting the time-series data.
    /// </summary>
    [Browsable(false)]
    public PacketParser Parser { get; }

    /// <summary>
    /// Gets the up-time (in seconds) of the <see cref="DataListener"/>.
    /// </summary>
    [Browsable(false)]
    public Time RunTime
    {
        get
        {
            if (m_tcpServer.CurrentState == ServerState.Running)
                return m_tcpServer.RunTime;
            
            if (m_tcpClient.CurrentState == ClientState.Connected)
                return m_tcpClient.ConnectionTime;
            
            if (m_udpClient.CurrentState == ClientState.Connected)
                return m_udpClient.ConnectionTime;
            
            return 0;
        }
    }

    /// <summary>
    /// Gets the total number of bytes received by the <see cref="DataListener"/> since it was <see cref="Start"/>ed.
    /// </summary>
    [Browsable(false)]
    public long TotalBytesReceived { get; private set; }

    /// <summary>
    /// Gets the total number of packets received by the <see cref="DataListener"/> since it was <see cref="Start"/>ed.
    /// </summary>
    [Browsable(false)]
    public long TotalPacketsReceived { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the <see cref="DataListener"/>.
    /// </summary>
    [Browsable(false)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets the descriptive status of the <see cref="DataListener"/>.
    /// </summary>
    [Browsable(false)]
    public string Status
    {
        get
        {
            StringBuilder status = new();
            status.AppendLine($"                      Name: {Name}");
            status.AppendLine($"            Server address: {m_server}");
            status.AppendLine($"               Server port: {m_port}");
            status.AppendLine($"        Transport protocol: {m_protocol}");
            status.AppendLine($"      Total bytes received: {TotalBytesReceived:N0}");
            status.AppendLine($"    Total packets received: {TotalPacketsReceived:N0}");
            status.AppendLine($"                  Run time: {RunTime.ToString(3)}");

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes the <see cref="DataListener"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Initialize()"/> is to be called by user-code directly only if the <see cref="DataListener"/> is not consumed through the designer surface of the IDE.
    /// </remarks>
    public void Initialize()
    {
        if (m_initialized)
            return;
        
        LoadSettings();         // Load settings from the config file.
        m_initialized = true;   // Initialize only once.
    }

    /// <summary>
    /// Performs necessary operations before the <see cref="DataListener"/> properties are initialized.
    /// </summary>
    /// <remarks>
    /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
    /// by the designer if the <see cref="DataListener"/> is consumed through the designer surface of the IDE.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void BeginInit()
    {
        if (!DesignMode)
        {
            try
            {
                // Nothing needs to be done before component is initialized.
            }
            catch
            {
                // Prevent the IDE from crashing when component is in design mode.
            }
        }
    }

    /// <summary>
    /// Performs necessary operations after the <see cref="DataListener"/> properties are initialized.
    /// </summary>
    /// <remarks>
    /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
    /// by the designer if the <see cref="DataListener"/> is consumed through the designer surface of the IDE.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void EndInit()
    {
        if (!DesignMode)
        {
            try
            {
                Initialize();
            }
            catch
            {
                // Prevent the IDE from crashing when component is in design mode.
            }
        }
    }

    /// <summary>
    /// Saves settings for the <see cref="DataListener"/> to the config file if the <see cref="PersistSettings"/> property is set to true.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
    public void SaveSettings()
    {
        if (!PersistSettings)
            return;
        
        // Ensure that settings category is specified.
        if (string.IsNullOrEmpty(m_settingsCategory))
            throw new ConfigurationErrorsException("SettingsCategory property has not been set");

        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
        
        settings[nameof(ID), true].Update(Name);
        settings[nameof(Server), true].Update(m_server);
        settings[nameof(Port), true].Update(m_port);
        settings[nameof(Protocol), true].Update(m_protocol);
        settings[nameof(ConnectToServer), true].Update(ConnectToServer);
        settings[nameof(CacheData), true].Update(m_cacheData);
        settings[nameof(InitializeData), true].Update(m_initializeData);
        settings[nameof(InitializeDataTimeout), true].Update(m_initializeDataTimeout);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved settings for the <see cref="DataListener"/> from the config file if the <see cref="PersistSettings"/>  property is set to true.
    /// </summary>
    /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
    public void LoadSettings()
    {
        if (!PersistSettings)
            return;
        
        // Ensure that settings category is specified.
        if (string.IsNullOrEmpty(m_settingsCategory))
            throw new ConfigurationErrorsException("SettingsCategory property has not been set");

        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
        
        settings.Add(nameof(ID), Name, "Alpha-numeric identifier of the listener.");
        settings.Add(nameof(Server), m_server, "DNS name or IP address of the server providing the time-series data.");
        settings.Add(nameof(Port), m_port, "Network port at the server where the time-series data is being server.");
        settings.Add(nameof(Protocol), m_protocol, "Protocol (Tcp; Udp) to be used for receiving time-series data.");
        settings.Add(nameof(ConnectToServer), ConnectToServer, "True is the listener to initiate connection to the server; otherwise False;");
        settings.Add(nameof(CacheData), m_cacheData, "True if newest data is to be cached locally; otherwise False.");
        settings.Add(nameof(InitializeData), m_initializeData, "True if data is to be initialized from the server on startup; otherwise False.");
        settings.Add(nameof(InitializeDataTimeout), m_initializeDataTimeout, "Number of milliseconds to wait for data to be initialized from the server on startup.");
        
        ID = settings[nameof(ID)].ValueAs(Name);
        Server = settings[nameof(Server)].ValueAs(m_server);
        Port = settings[nameof(Port)].ValueAs(m_port);
        Protocol = settings[nameof(Protocol)].ValueAs(m_protocol);
        ConnectToServer = settings[nameof(ConnectToServer)].ValueAs(ConnectToServer);
        CacheData = settings[nameof(CacheData)].ValueAs(m_cacheData);
        InitializeData = settings[nameof(InitializeData)].ValueAs(m_initializeData);
        InitializeDataTimeout = settings[nameof(InitializeDataTimeout)].ValueAs(m_initializeDataTimeout);
    }

    /// <summary>
    /// Starts the <see cref="DataListener"/> synchronously.
    /// </summary>
    public void Start()
    {
        if (Enabled)
            return;
        
        m_listenerStopping = false;

        OnListenerStarting();

        if (m_initializeData)
        {
            // Attempt to initialize data from the server.
            OnDataInitStart();
            
            m_dataInitClient.ConnectionString = string.Format(m_dataInitClient.ConnectionString, Server);
            m_dataInitClient.Connect();

            if (m_dataInitClient.CurrentState == ClientState.Connected)
            {
                // Wait enough for the handshaking to complete.
                Thread.Sleep(1000);

                // We'll request current data for all points.
                PacketType11 request = new();
                request.RequestIDs.Add(-1);
                m_dataInitClient.Send(request.BinaryImage());

                // Wait for the data to be initialized and timeout if it takes too long.
                if (!m_initializeWaitHandle.WaitOne(m_initializeDataTimeout, false))
                    OnDataInitPartial();
                else
                    OnDataInitComplete();
            
                m_dataInitClient.Disconnect();
            }
            else
            {
                // Archiver is either not running, or is running but is a legacy Archiver.
                OnDataInitFailure();
            }
        }

        // Start-up the appropriate communication component that'll get the raw data.
        if (Protocol == TransportProtocol.Udp)
        {
            m_udpClient.ConnectionString = $"Port={Port}; interface=0.0.0.0";
            m_udpClient.Connect(); // Start the connection cycle - try indefinitely.
        }
        else
        {
            if (ConnectToServer)
            {
                // TCP connection - going out to the server for connection.
                m_tcpClient.ConnectionString = $"Server={Server}:{Port}; interface=0.0.0.0";
                m_tcpClient.Connect(); // Start the connection cycle - try indefinitely.
            }
            else
            {
                // TCP connection - client coming in for connection.
                m_tcpServer.ConfigurationString = $"Port={Port}; interface=0.0.0.0";
                m_tcpServer.Start(); // Start the connection cycle - try indefinitely.
                OnSocketConnecting();
            }
        }

        // Start the parser that'll parse the raw data.
        Parser.Start();
    }

    /// <summary>
    /// Starts the <see cref="DataListener"/> asynchronously.
    /// </summary>
    public void StartAsync()
    {
        // Only allow one async startup attempt.
        if (m_startupThread is not null && m_startupThread.IsAlive)
            return;

        m_startupThread = new Thread(Start);
        m_startupThread.Start();
    }

    /// <summary>
    /// Stops the <see cref="DataListener"/>.
    /// </summary>
    public void Stop()
    {
        OnListenerStopping();

        // Abort async startup process if it is running.
        m_startupThread?.Abort();

        // Prevent communication clients from reconnecting.
        m_listenerStopping = true;

        m_tcpServer.Stop();
        m_tcpClient.Disconnect();
        m_udpClient.Disconnect();
        Parser.Stop();
        
        TotalBytesReceived = 0L;
        TotalPacketsReceived = 0L;

        OnListenerStopped();
    }

    /// <summary>
    /// Gets the current data for the specified <paramref name="historianID"/>.
    /// </summary>
    /// <param name="historianID">Historian identifier whose current data is to be retrieved.</param>
    /// <returns><see cref="IDataPoint"/> if a match is found; otherwise null.</returns>
    public IDataPoint FindData(int historianID)
    {
        IDataPoint currentData = null;
        
        lock (m_data)
        {
            // If valid ID is specified, lookup it's current data.
            if (historianID > 0 && historianID <= m_data.Count)
                currentData = m_data[historianID - 1];
        }

        return currentData;
    }

    /// <summary>
    /// Determines whether the current <see cref="DataListener"/> object is equal to <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object against which the current <see cref="DataListener"/> object is to be compared for equality.</param>
    /// <returns>true if the current <see cref="DataListener"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
    public override bool Equals(object obj)
    {
        if (obj is not DataListener other)
            return false;
        
        return string.Compare(Name, other.ID, StringComparison.OrdinalIgnoreCase) == 0;
    }

    /// <summary>
    /// Returns the hash code for the current <see cref="DataListener"/> object.
    /// </summary>
    /// <returns>A 32-bit signed integer value.</returns>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <summary>
    /// Raises the <see cref="ListenerStarting"/> event.
    /// </summary>
    protected virtual void OnListenerStarting()
    {
        ListenerStarting?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ListenerStarted"/> event.
    /// </summary>
    protected virtual void OnListenerStarted()
    {
        ListenerStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ListenerStopping"/> event.
    /// </summary>
    protected virtual void OnListenerStopping()
    {
        ListenerStopping?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ListenerStopped"/> event.
    /// </summary>
    protected virtual void OnListenerStopped()
    {
        ListenerStopped?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="SocketConnecting"/> event.
    /// </summary>
    protected virtual void OnSocketConnecting()
    {
        SocketConnecting?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="SocketConnected"/> event.
    /// </summary>
    protected virtual void OnSocketConnected()
    {
        SocketConnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="SocketDisconnected"/> event.
    /// </summary>
    protected virtual void OnSocketDisconnected()
    {
        SocketDisconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DataInitStart"/> event.
    /// </summary>
    protected virtual void OnDataInitStart()
    {
        DataInitStart?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DataInitComplete"/> event.
    /// </summary>
    protected virtual void OnDataInitComplete()
    {
        DataInitComplete?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DataInitPartial"/> event.
    /// </summary>
    protected virtual void OnDataInitPartial()
    {
        DataInitPartial?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DataInitFailure"/> event.
    /// </summary>
    protected virtual void OnDataInitFailure()
    {
        DataInitFailure?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DataExtracted"/> event.
    /// </summary>
    /// <param name="data">Extracted time-series data to send to <see cref="DataExtracted"/> event.</param>
    protected virtual void OnDataExtracted(IList<IDataPoint> data)
    {
        DataExtracted?.Invoke(this, new EventArgs<IList<IDataPoint>>(data));
    }

    /// <summary>
    /// Raises the <see cref="DataChanged"/> event.
    /// </summary>
    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DataListener"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;
        
        try
        {
            // This will be done regardless of whether the object is finalized or disposed.
            if (!disposing)
                return;

            // This will be done only when the object is disposed by calling Dispose().
            Stop();
            SaveSettings();

            m_data?.Clear();

            if (Parser is not null)
            {
                Parser.DataParsed -= PacketParser_DataParsed;
                Parser.Dispose();
            }

            if (m_tcpServer is not null)
            {
                m_tcpServer.ServerStarted -= ServerSocket_ServerStarted;
                m_tcpServer.ServerStopped -= ServerSocket_ServerStopped;
                m_tcpServer.ReceiveClientDataComplete -= ServerSocket_ReceiveClientDataComplete;
                m_tcpServer.Dispose();
            }

            if (m_tcpClient is not null)
            {
                m_tcpClient.ConnectionAttempt -= ClientSocket_ConnectionAttempt;
                m_tcpClient.ConnectionEstablished -= ClientSocket_ConnectionEstablished;
                m_tcpClient.ConnectionTerminated -= ClientSocket_ConnectionTerminated;
                m_tcpClient.ReceiveDataComplete -= ClientSocket_ReceiveDataComplete;
                m_tcpClient.Dispose();
            }

            if (m_udpClient is not null)
            {
                m_udpClient.ConnectionAttempt -= ClientSocket_ConnectionAttempt;
                m_udpClient.ConnectionEstablished -= ClientSocket_ConnectionEstablished;
                m_udpClient.ConnectionTerminated -= ClientSocket_ConnectionTerminated;
                m_udpClient.ReceiveDataComplete -= ClientSocket_ReceiveDataComplete;
                m_udpClient.Dispose();
            }

            if (m_dataInitClient is not null)
            {
                m_dataInitClient.ReceiveDataComplete -= DataInitClient_ReceiveDataComplete;
                m_dataInitClient.Dispose();
            }

            m_initializeWaitHandle?.Close();
        }
        finally
        {
            IsDisposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    private void DataInitClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
    {
        StateRecordSummary state = new(e.Argument1, 0, e.Argument2);

        if (state.HistorianID > 0)
        {
            lock (m_data)
                m_data.Add(new ArchiveDataPoint(state.HistorianID, state.CurrentData.Time, state.CurrentData.Value, state.CurrentData.Quality));
        }
        else
        {
            // This is the end-of-transmission to our request for current data from the server.
            m_initializeWaitHandle.Set();
        }
    }

    private void ServerSocket_ServerStarted(object sender, EventArgs e)
    {
        OnSocketConnected();
        OnListenerStarted();
    }

    private void ServerSocket_ServerStopped(object sender, EventArgs e)
    {
        OnSocketDisconnected();
    }

    private void ServerSocket_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
    {
        TotalPacketsReceived++;
        TotalBytesReceived += e.Argument3;
        Parser.Parse(e.Argument1, e.Argument2, 0, e.Argument3);
    }

    private void ClientSocket_ConnectionAttempt(object sender, EventArgs e)
    {
        OnSocketConnecting();
    }

    private void ClientSocket_ConnectionEstablished(object sender, EventArgs e)
    {
        OnSocketConnected();
        OnListenerStarted();
    }

    private void ClientSocket_ConnectionTerminated(object sender, EventArgs e)
    {
        OnSocketDisconnected();

        // Re-attempt connection to the server if the disconnect was not deliberate.
        if (!m_listenerStopping)
            ((IClient)sender).Connect();
    }

    private void ClientSocket_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
    {
        TotalPacketsReceived++;
        TotalBytesReceived += e.Argument2;
        Parser.Parse(m_clientIDs[(IClient)sender], e.Argument1, 0, e.Argument2);
    }

    private void PacketParser_DataParsed(object sender, EventArgs<IPacket> e)
    {
        // Extract data from the packets.
        List<IDataPoint> dataPoints = [];
        IEnumerable<IDataPoint> extractedData = e.Argument.ExtractTimeSeriesData();

        if (extractedData is not null)
            dataPoints.AddRange(extractedData);

        if (dataPoints.Count <= 0)
            return;
        
        // Published the extracted data.
        OnDataExtracted(dataPoints);

        // Cache extracted data for reuse.
        if (!m_cacheData)
            return;
        
        lock (m_data)
        {
            foreach (IDataPoint dataPoint in dataPoints)
            {
                if (dataPoint.HistorianID > m_data.Count)
                {
                    // No data exists for the id, so add one for it and others in-between.
                    for (int i = m_data.Count + 1; i <= dataPoint.HistorianID; i++)
                        m_data.Add(new ArchiveDataPoint(i));
                }

                // Replace existing data with the new data.
                m_data[dataPoint.HistorianID - 1] = dataPoint;
            }
        }
        
        OnDataChanged();
    }

    #endregion
}