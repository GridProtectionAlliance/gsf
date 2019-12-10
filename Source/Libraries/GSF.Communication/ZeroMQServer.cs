//******************************************************************************************************
//  ZeroMQServer.cs - Gbtc
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
//  05/22/2015 - J. Ritchie Carroll
//       Original version of source code generated.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/*
    This code uses zeromq/clrzmq4 library for ZeroMQ implementation (http://zeromq.org).
    Source code for the clrzmq4 project can be found here: https://github.com/zeromq/clrzmq4

    ZeroMQ and the clrzmq4 library are licensed under the GNU Lesser General Public License v3.0 plus
    a static linking exception (the "License"); you may not use this file except in compliance with
    the License. You may obtain a copy of the License at:

            https://www.gnu.org/licenses/lgpl-3.0.html

    Static linking exception: The copyright holders give you permission to link this library with
    independent modules to produce an executable, regardless of the license terms of these independent
    modules, and to copy and distribute the resulting executable under terms of your choice, provided
    that you also meet, for each linked independent module, the terms and conditions of the license of
    that module. An independent module is a module which is not derived from or based on this library.
    If you modify this library, you must extend this exception to your version of the library.    
*/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using GSF.IO;
using ZeroMQ;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a ZeroMQ ROUTER style socket as a communication server.
    /// </summary>
    public class ZeroMQServer : ServerBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ServerBase.ConfigurationString"/> property.
        /// </summary>
        public const string DefaultConfigurationString = "server=tcp://*:8888";

        /// <summary>
        /// Specifies the default value for the <see cref="MaxSendQueueSize"/> property.
        /// </summary>
        /// <remarks>
        /// Maps to ZeroMQ high water mark setting for outbound messages.
        /// </remarks>
        public const int DefaultMaxSendQueueSize = 500000;

        /// <summary>
        /// Specifies the default value for the <see cref="MaxReceiveQueueSize"/> property.
        /// </summary>
        /// <remarks>
        /// Maps to ZeroMQ high water mark setting for inbound messages.
        /// </remarks>
        public const int DefaultMaxReceiveQueueSize = 500000;

        /// <summary>
        /// Regular expression used to validate the format for a ZeroMQ endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Matches the following valid example input:<br/>
        /// - tcp://127.0.0.1:5959<br/>
        /// - tcp://[::1]:5959<br/>
        /// - tcp://dnsname.org:8186<br/>
        /// - pgm://[FF01:0:0:0:0:0:0:1]:6565<br/>
        /// - pgm://224.0.0.1:6565<br/>
        /// - inproc://pipe-name<br/>
        /// </para>
        /// </remarks>
        public const string EndpointFormatRegex = @"(?<protocol>.+)\://(?<host>(\[.+\]|[^\:])+)(\:(?<port>\d+$))?";
        private ZeroMQTransportProtocol m_zeroMQTransportProtocol;
        private readonly ManualResetEventSlim m_completedHandle;
        private Dictionary<string, string> m_configData;
        private readonly ConcurrentDictionary<Guid, TransportProvider<DateTime>> m_clientInfoLookup;
        private readonly Timer m_activeClientTimer;
        private Thread m_receiveDataThread;
        private readonly object m_sendLock;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQServer"/> class.
        /// </summary>
        public ZeroMQServer() : this(DefaultConfigurationString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQServer"/> class.
        /// </summary>
        /// <param name="configString">Config string of the <see cref="ZeroMQServer"/>. See <see cref="DefaultConfigurationString"/> for format.</param>
        public ZeroMQServer(string configString) : base(TransportProtocol.Tcp, configString)
        {
            m_zeroMQTransportProtocol = ZeroMQTransportProtocol.Tcp;
            m_completedHandle = new ManualResetEventSlim(true);
            MaxSendQueueSize = DefaultMaxSendQueueSize;
            MaxReceiveQueueSize = DefaultMaxReceiveQueueSize;
            m_clientInfoLookup = new ConcurrentDictionary<Guid, TransportProvider<DateTime>>();
            m_activeClientTimer = new Timer(MonitorActiveClients, null, TimeSpan.FromMinutes(1.0D), TimeSpan.FromMinutes(1.0D));
            m_sendLock = new object();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQServer"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ZeroMQServer"/>.</param>
        public ZeroMQServer(IContainer container) : this() => container?.Add(this);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the maximum size for the send queue before payloads are dumped from the queue.
        /// </summary>
        /// <remarks>
        /// Maps to ZeroMQ high water mark setting for outbound messages.
        /// </remarks>
        [Category("Settings")]
        [DefaultValue(DefaultMaxSendQueueSize)]
        [Description("The maximum size for the send queue before payloads are dumped from the queue.")]
        public int MaxSendQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size for the receive queue before payloads are dumped from the queue.
        /// </summary>
        /// <remarks>
        /// Maps to ZeroMQ high water mark setting for inbound messages.
        /// </remarks>
        [Category("Settings")]
        [DefaultValue(DefaultMaxReceiveQueueSize)]
        [Description("The maximum size for the receive queue before payloads are dumped from the queue.")]
        public int MaxReceiveQueueSize { get; set; }

        /// <summary>
        /// Gets the <see cref="GSF.Communication.TransportProtocol"/> used by the server for the transportation of data with the clients.
        /// </summary>
        public override TransportProtocol TransportProtocol
        {
            get
            {
                switch (m_zeroMQTransportProtocol)
                {
                    case ZeroMQTransportProtocol.Tcp:
                    case ZeroMQTransportProtocol.InProc:
                        return TransportProtocol.Tcp;
                    default:
                        return TransportProtocol.Udp;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ZeroMQ transport protocol to use for the <see cref="ZeroMQServer"/>.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(ZeroMQTransportProtocol.Tcp)]
        [Description("The ZeroMQ transport protocol to use for the connection.")]
        public ZeroMQTransportProtocol ZeroMQTransportProtocol
        {
            get => m_zeroMQTransportProtocol;
            set => m_zeroMQTransportProtocol = value;
        }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="ZeroMQServer"/>.
        /// </summary>
        [Browsable(false)]
        public ZSocket Server { get; private set; }

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                if (Server != null)
                {
                    try
                    {
                        statusBuilder.AppendFormat("              0MQ Identity: {0}", new Guid(Server.Identity));
                    }
                    catch
                    {
                        statusBuilder.AppendFormat("              0MQ Identity: {0}", Server.IdentityString);
                    }
                    statusBuilder.AppendLine();
                    statusBuilder.AppendFormat("         0MQ Last Endpoint: {0}", Server.LastEndpoint);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("    0MQ Transport Protocol: {0}", ZeroMQTransportProtocol);
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("      0MQ Binding Endpoint: {0}", m_configData["server"]);
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("       Max send queue size: {0}", MaxSendQueueSize);
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("    Max receive queue size: {0}", MaxReceiveQueueSize);
                statusBuilder.AppendLine();

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZeroMQServer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "m_activeClientTimer", Justification = "Field is properly disposed")]
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_completedHandle != null)
                {
                    m_completedHandle.Set();
                    m_completedHandle.Dispose();
                }

                m_activeClientTimer?.Dispose();

                Server?.Dispose();
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

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

            if (!m_clientInfoLookup.TryGetValue(clientID, out TransportProvider<DateTime> clientInfo))
                throw new InvalidOperationException("Specified client ID does not exist, cannot read buffer.");

            if (clientInfo.ReceiveBuffer == null)
                throw new InvalidOperationException("No received data buffer has been defined to read.");

            int readIndex = ReadIndicies[clientID];
            int sourceLength = clientInfo.BytesReceived - readIndex;
            int readBytes = length > sourceLength ? sourceLength : length;
            Buffer.BlockCopy(clientInfo.ReceiveBuffer, readIndex, buffer, startIndex, readBytes);

            // Update read index for next call
            readIndex += readBytes;

            if (readIndex >= clientInfo.BytesReceived)
                readIndex = 0;

            ReadIndicies[clientID] = readIndex;

            return readBytes;
        }

        /// <summary>
        /// Saves <see cref="ZeroMQServer"/> settings to the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (!PersistSettings)
                return;

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings["MaxSendQueueSize", true].Update(MaxSendQueueSize);
            settings["MaxReceiveQueueSize", true].Update(MaxReceiveQueueSize);
            
            config.Save();
        }

        /// <summary>
        /// Loads saved <see cref="ZeroMQServer"/> settings from the config file if the <see cref="ServerBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (!PersistSettings)
                return;

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
            settings.Add("MaxSendQueueSize", MaxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
            settings.Add("MaxReceiveQueueSize", MaxReceiveQueueSize, "The maximum size of the receive queue before payloads are dumped from the queue.");
            
            MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(MaxSendQueueSize);
            MaxReceiveQueueSize = settings["MaxReceiveQueueSize"].ValueAs(MaxReceiveQueueSize);

            // When transitioning from socket type to another, be sure to restore default values
            if (MaxSendQueueSize == -1)
                MaxSendQueueSize = DefaultMaxSendQueueSize;

            if (MaxReceiveQueueSize == -1)
                MaxReceiveQueueSize = DefaultMaxReceiveQueueSize;
        }

        /// <summary>
        /// Stops the <see cref="ZeroMQServer"/> synchronously and disconnects all connected clients.
        /// </summary>
        public override void Stop()
        {
            if (CurrentState != ServerState.Running)
                return;

            if (m_receiveDataThread != null)
            {
                m_receiveDataThread.Abort();
                m_receiveDataThread = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }

            DisconnectAll();            // Disconnection all clients.
            OnServerStopped();
        }

        /// <summary>
        /// Starts the <see cref="ZeroMQServer"/> synchronously and begins accepting client connections asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to <see cref="Start()"/> the <see cref="ZeroMQServer"/> when it is running.</exception>
        public override void Start()
        {
            if (CurrentState != ServerState.NotRunning)
                return;

            // Initialize if needed
            if (!Initialized)
                Initialize();

            // Overwrite config file if max client connections exists in connection string.
            if (m_configData.ContainsKey("maxClientConnections") && int.TryParse(m_configData["maxClientConnections"], out int maxClientConnections))
                MaxClientConnections = maxClientConnections;

            // Overwrite config file if max send queue size exists in connection string.
            if (m_configData.ContainsKey("maxSendQueueSize") && int.TryParse(m_configData["maxSendQueueSize"], out int maxQueueSize))
                MaxSendQueueSize = maxQueueSize;

            // Overwrite config file if max receive queue size exists in connection string.
            if (m_configData.ContainsKey("maxReceiveQueueSize") && int.TryParse(m_configData["maxReceiveQueueSize"], out maxQueueSize))
                MaxReceiveQueueSize = maxQueueSize;

            // Create ZeroMQ Router socket - closest match to IServer implementation
            Server = new ZSocket(ZContext.Create(), ZSocketType.ROUTER)
            {
                Identity = ServerID.ToByteArray(),
                SendHighWatermark = MaxSendQueueSize,
                ReceiveHighWatermark = MaxReceiveQueueSize,
                Immediate = true,
                IPv6 = Transport.GetDefaultIPStack() == IPStack.IPv6
            };

            Server.SetOption(ZSocketOption.LINGER, 0);
            Server.SetOption(ZSocketOption.SNDTIMEO, 1000);
            Server.SetOption(ZSocketOption.RCVTIMEO, -1);
            Server.SetOption(ZSocketOption.RECONNECT_IVL, -1);
            
            Server.Bind(m_configData["server"]);

            // Notify that the server has been started successfully.
            OnServerStarted();

            m_receiveDataThread = new Thread(ReceiveDataHandler)
            {
                IsBackground = true
            };

            m_receiveDataThread.Start();
        }

        private void ReceiveDataHandler()
        {
            while (Enabled)
            {
                Guid clientID = Guid.Empty;
                TransportProvider<DateTime> clientInfo = null;

                try
                {
                    // Receive data from the socket
                    using (ZMessage message = Server.ReceiveMessage())
                    {
                        // Router socket should provide identity, delimiter and data payload frames
                        if (message.Count == 3)
                        {
                            // Extract client identity
                            clientID = new Guid(message[0].ReadStream());

                            // Lookup client info, adding it if it doesn't exist
                            clientInfo = GetClient(clientID);

                            // Get data payload frame
                            ZFrame frame = message[2];

                            clientInfo.BytesReceived = (int)frame.Length;

                            if (clientInfo.ReceiveBufferSize < clientInfo.BytesReceived)
                                clientInfo.SetReceiveBuffer(clientInfo.BytesReceived);

                            frame.Read(clientInfo.ReceiveBuffer, 0, clientInfo.BytesReceived);

                            clientInfo.Statistics.UpdateBytesReceived(clientInfo.BytesReceived);

                            // Update last client activity time
                            clientInfo.Provider = DateTime.UtcNow;
                        }
                    }

                    // Notify consumer of received data
                    if (clientInfo != null)
                        OnReceiveClientDataComplete(clientID, clientInfo.ReceiveBuffer, clientInfo.BytesReceived);
                }
                catch (Exception ex)
                {
                    OnReceiveClientDataException(clientID, ex);
                }
            }
        }

        /// <summary>
        /// Disconnects the specified connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        /// <exception cref="InvalidOperationException">Client does not exist for the specified <paramref name="clientID"/>.</exception>
        public override void DisconnectOne(Guid clientID)
        {
            if (!m_clientInfoLookup.TryRemove(clientID, out TransportProvider<DateTime> clientInfo))
                return;

            try
            {
                OnClientDisconnected(clientID);
                clientInfo.Reset();
            }
            catch (Exception ex)
            {
                OnSendClientDataException(clientInfo.ID, new InvalidOperationException($"Client disconnection exception: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Gets the <see cref="TransportProvider{Socket}"/> object associated with the specified client ID.
        /// </summary>
        /// <param name="clientID">ID of the client.</param>
        /// <param name="clientInfo">Client information.</param>
        /// <returns><c>true</c> if client exists; otherwise, <c>false</c>.</returns>
        public bool TryGetClient(Guid clientID, out TransportProvider<DateTime> clientInfo) => m_clientInfoLookup.TryGetValue(clientID, out clientInfo);

        // Get client info, creating it if it doesn't exist
        private TransportProvider<DateTime> GetClient(Guid clientID)
        {
            return m_clientInfoLookup.GetOrAdd(clientID, id =>
            {
                OnClientConnected(id);

                return new TransportProvider<DateTime>
                {
                    Provider = DateTime.UtcNow,
                    ID = id
                };
            });
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

            // Check for "server" property, this is the preferred configuration for ZeroMQServer
            if (m_configData.ContainsKey("server"))
            {
                // Validate ZeroMQ configuration string
                Match endpointMatch = Regex.Match(m_configData["server"], EndpointFormatRegex);
                string port = null;
                bool validConfigurationString = false;

                if (endpointMatch.Success)
                {
                    if (!Enum.TryParse(endpointMatch.Groups["protocol"].Value.Trim(), true, out m_zeroMQTransportProtocol))
                        m_zeroMQTransportProtocol = ZeroMQTransportProtocol.Tcp;

                    m_configData["interface"] = endpointMatch.Groups["host"].Value.Trim();
                    port = endpointMatch.Groups["port"].Value.Trim();
                    validConfigurationString = true;
                }
                else
                {
                    // Support traditional IClient "server" property
                    endpointMatch = Regex.Match(m_configData["server"], Transport.EndpointFormatRegex);

                    if (endpointMatch.Success)
                    {
                        m_configData["server"] = $"{m_zeroMQTransportProtocol.ToString().ToLowerInvariant()}://{m_configData["server"]}";
                        m_configData["interface"] = endpointMatch.Groups["host"].Value.Trim();
                        port = endpointMatch.Groups["port"].Value.Trim();
                        validConfigurationString = true;
                    }
                }

                if (!validConfigurationString)
                    throw new FormatException($"Server property is invalid (Example: {DefaultConfigurationString})");

                if (m_zeroMQTransportProtocol != ZeroMQTransportProtocol.InProc && !Transport.IsPortNumberValid(port))
                    throw new ArgumentOutOfRangeException(nameof(configurationString), $"Port number must be between {Transport.PortRangeLow} and {Transport.PortRangeHigh}");
            }
            else
            {
                // Fall back on traditional server configuration strings
                Transport.GetInterfaceIPStack(m_configData);

                if (string.IsNullOrWhiteSpace(m_configData["interface"]) || m_configData["interface"].Equals("0.0.0.0", StringComparison.Ordinal))
                    m_configData["interface"] = "*";

                // For traditional style connection strings, also support a "zeroMQTransportProtocol" setting
                if (m_configData.ContainsKey("zeroMQTransportProtocol"))
                {
                    if (Enum.TryParse(m_configData["zeroMQTransportProtocol"].Trim(), true, out ZeroMQTransportProtocol protocol))
                        m_zeroMQTransportProtocol = protocol;
                }

                // For traditional IServer connection strings, a "port" property is expected
                if (m_configData.ContainsKey("port") && m_zeroMQTransportProtocol != ZeroMQTransportProtocol.InProc)
                    m_configData["server"] = $"{m_zeroMQTransportProtocol.ToString().ToLowerInvariant()}://{m_configData["interface"]}:{m_configData["port"]}";
                else
                    throw new FormatException($"Server property is invalid (Example: {DefaultConfigurationString})");
            }
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
            if (CurrentState != ServerState.Running)
                throw new SocketException((int)SocketError.NotConnected);

            try
            {
                if (Server != null)
                {
                    // Lookup client info, adding it if it doesn't exist
                    TransportProvider<DateTime> clientInfo = GetClient(clientID);

                    // Router socket should provide identity, delimiter and data payload frames
                    using (ZMessage message = new ZMessage())
                    {
                        // Add identity, delimiter and data payload frames
                        message.Add(new ZFrame(clientID.ToByteArray()));
                        message.Add(new ZFrame());
                        message.Add(new ZFrame(data, offset, length));

                        // ZeroMQ send is asynchronous, but API call is not thread-safe
                        lock (m_sendLock)
                            Server.Send(message);
                    }

                    clientInfo.Statistics.UpdateBytesSent(length);

                    // Update last client activity time
                    clientInfo.Provider = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                // Log exception during send operation
                OnSendClientDataException(clientID, ex);
            }

            return m_completedHandle.WaitHandle;
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.SendClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.SendClientDataException"/> event.</param>
        protected override void OnSendClientDataException(Guid clientID, Exception ex)
        {
            if (IsThreadAbortException(ex))
                return;

            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
            {
                if (ex is ZException zmqex && (zmqex.Error.Number == ZError.EAGAIN.Number || zmqex.Error.Number == ZError.ETERM.Number))
                    ThreadPool.QueueUserWorkItem(state => DisconnectOne(clientID));
                else
                    base.OnSendClientDataException(clientID, ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.ReceiveClientDataException"/> event.
        /// </summary>
        /// <param name="clientID">ID of client to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        /// <param name="ex">Exception to send to <see cref="ServerBase.ReceiveClientDataException"/> event.</param>
        protected override void OnReceiveClientDataException(Guid clientID, Exception ex)
        {
            if (IsThreadAbortException(ex))
                return;

            if (m_clientInfoLookup.ContainsKey(clientID) && CurrentState == ServerState.Running)
                base.OnReceiveClientDataException(clientID, ex);
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.ClientConnectingException"/> event.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> encountered when connecting to the client.</param>
        protected override void OnClientConnectingException(Exception ex)
        {
            if (IsThreadAbortException(ex))
                return;

            base.OnClientConnectingException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ServerBase.UnhandledUserException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ServerBase.UnhandledUserException"/> event.</param>
        protected override void OnUnhandledUserException(Exception ex)
        {
            if (IsThreadAbortException(ex))
                return;

            base.OnUnhandledUserException(ex);
        }

        private void MonitorActiveClients(object state)
        {
            if (m_clientInfoLookup == null)
                return;

            List<Guid> oldClients = new List<Guid>();

            // Maintain client info lookup table size by removing clients that haven't been active in the last minute
            foreach (TransportProvider<DateTime> clientInfo in m_clientInfoLookup.Values)
            {
                if ((DateTime.UtcNow - clientInfo.Provider).TotalMinutes > 1.0D)
                    oldClients.Add(clientInfo.ID);
            }

            foreach (Guid client in oldClients)
            {
                if (m_clientInfoLookup.TryRemove(client, out TransportProvider<DateTime> _))
                    OnClientDisconnected(client);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        internal static bool IsThreadAbortException(Exception ex)
        {
            while (ex != null)
            {
                if (ex is ThreadAbortException)
                    return true;

                ex = ex.InnerException;
            }

            return false;
        }

        #endregion
    }
}