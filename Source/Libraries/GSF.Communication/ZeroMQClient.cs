//******************************************************************************************************
//  ZeroMQClient.cs - Gbtc
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Configuration;
using ZeroMQ;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a ZeroMQ DEALER style socket as a communication client.
    /// </summary>
    public class ZeroMQClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "server=tcp://127.0.0.1:8888";

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

        // Fields
        private readonly TransportProvider<ZSocket> m_zeroMQClient;
        private ZeroMQTransportProtocol m_zeroMQTransportProtocol;
        private readonly ManualResetEventSlim m_completedHandle;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectionHandle;
        private Thread m_connectionThread;
        private readonly object m_sendLock;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQClient"/> class.
        /// </summary>
        public ZeroMQClient() : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="ZeroMQClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public ZeroMQClient(string connectString) : base(TransportProtocol.Tcp, connectString)
        {
            m_zeroMQClient = new TransportProvider<ZSocket>();
            m_zeroMQTransportProtocol = ZeroMQTransportProtocol.Tcp;
            m_completedHandle = new ManualResetEventSlim(true);
            MaxSendQueueSize = DefaultMaxSendQueueSize;
            MaxReceiveQueueSize = DefaultMaxReceiveQueueSize;
            m_sendLock = new object();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMQClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="ZeroMQClient"/>.</param>
        public ZeroMQClient(IContainer container) : this() => container?.Add(this);

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
        /// Gets the <see cref="GSF.Communication.TransportProtocol"/> used by the client for the transportation of data with the server.
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
        /// Gets or sets the ZeroMQ transport protocol to use for the <see cref="ZeroMQClient"/>.
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
        /// Gets the <see cref="Socket"/> object for the <see cref="ZeroMQClient"/>.
        /// </summary>
        [Browsable(false)]
        public ZSocket Client => m_zeroMQClient.Provider;

        /// <summary>
        /// Gets the server URI of the <see cref="ZeroMQClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri => m_connectData["server"];

        /// <summary>
        /// Gets the descriptive status of the client.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                ZSocket client = Client;

                if (client != null)
                {
                    try
                    {
                        statusBuilder.AppendFormat("              0MQ Identity: {0}", new Guid(client.Identity));
                    }
                    catch
                    {
                        statusBuilder.AppendFormat("              0MQ Identity: {0}", client.IdentityString);
                    }
                    statusBuilder.AppendLine();

                    statusBuilder.AppendFormat("         0MQ Last Endpoint: {0}", client.LastEndpoint);
                    statusBuilder.AppendLine();
                }

                statusBuilder.AppendFormat("    0MQ Transport Protocol: {0}", ZeroMQTransportProtocol);
                statusBuilder.AppendLine();
                statusBuilder.AppendFormat("   0MQ Connection Endpoint: {0}", ServerUri);
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
            settings["MaxSendQueueSize", true].Update(MaxSendQueueSize);
            settings["MaxReceiveQueueSize", true].Update(MaxReceiveQueueSize);
            
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
            settings.Add("MaxSendQueueSize", MaxSendQueueSize, "The maximum size of the send queue before payloads are dumped from the queue.");
            settings.Add("MaxReceiveQueueSize", MaxReceiveQueueSize, "The maximum size of the receive queue before payloads are dumped from the queue.");
            
            MaxSendQueueSize = settings["MaxSendQueueSize"].ValueAs(MaxSendQueueSize);
            MaxReceiveQueueSize = settings["MaxReceiveQueueSize"].ValueAs(MaxReceiveQueueSize);

            // Overwrite config file if max send or receive queue size exists in connection string
            if (m_connectData.ContainsKey("maxSendQueueSize") && int.TryParse(m_connectData["maxSendQueueSize"], out int maxQueueSize))
                MaxSendQueueSize = maxQueueSize;

            if (m_connectData.ContainsKey("maxReceiveQueueSize") && int.TryParse(m_connectData["maxReceiveQueueSize"], out maxQueueSize))
                MaxReceiveQueueSize = maxQueueSize;
        }

        /// <summary>
        /// Connects the <see cref="ZeroMQClient"/> to the server asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="ZeroMQClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            m_connectionHandle = (ManualResetEvent)base.ConnectAsync();

            m_zeroMQClient.SetReceiveBuffer(ReceiveBufferSize);

            m_connectionThread = new Thread(OpenSocket)
            {
                IsBackground = true
            };

            m_connectionThread.Start();

            return m_connectionHandle;
        }

        private void OpenSocket(object state)
        {
            int connectionAttempts = 0;

            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                if (m_zeroMQClient.Provider != null)
                {
                    // Disconnect any existing ZeroMQ socket
                    try
                    {
                        m_zeroMQClient.Provider.Disconnect(ServerUri);
                    }
                    catch (Exception ex)
                    {
                        OnDisconnectException(ex);
                    }

                    m_zeroMQClient.Reset();
                }

                try
                {
                    OnConnectionAttempt();

                    // Create ZeroMQ Dealer socket - closest match to IClient implementation
                    m_zeroMQClient.Provider = new ZSocket(ZContext.Create(), ZSocketType.DEALER)
                    {
                        Identity = m_zeroMQClient.ID.ToByteArray(), 
                        SendHighWatermark = MaxSendQueueSize, 
                        ReceiveHighWatermark = MaxReceiveQueueSize, 
                        Immediate = true,
                        IPv6 = Transport.GetDefaultIPStack() == IPStack.IPv6
                    };

                    m_zeroMQClient.Provider.SetOption(ZSocketOption.LINGER, 0);
                    m_zeroMQClient.Provider.SetOption(ZSocketOption.SNDTIMEO, 1000);
                    m_zeroMQClient.Provider.SetOption(ZSocketOption.RCVTIMEO, -1);
                    m_zeroMQClient.Provider.SetOption(ZSocketOption.RECONNECT_IVL, -1);
                    
                    m_zeroMQClient.Provider.Connect(ServerUri);

                    m_connectionHandle.Set();
                    connectionAttempts = 0;

                    OnConnectionEstablished();
                }
                catch (Exception ex)
                {
                    m_zeroMQClient.Provider = null;

                    // Log exception during connection attempt
                    OnConnectionException(ex);

                    // Keep retrying connection
                    Thread.Sleep(1000);
                    connectionAttempts++;
                    continue;
                }

                try
                {
                    // Start data reception loop
                    ReceiveDataHandler();
                }
                catch (Exception ex)
                {
                    // Notify of the exception.
                    OnReceiveDataException(ex);
                }

                // If client is no longer connected, exit loop, else sleep for a moment before attempting reconnect
                if (Enabled)
                    Thread.Sleep(1000);
                else
                    break;
            }
        }

        private void ReceiveDataHandler()
        {
            while (Enabled)
            {
                // Receive data from the socket
                using (ZMessage message = m_zeroMQClient.Provider.ReceiveMessage())
                {
                    // Dealer socket should have removed identity frame already, should be left
                    // with delimiter and data payload frames
                    if (message.Count == 2)
                    {
                        // Get the data payload frame
                        ZFrame frame = message[1];

                        m_zeroMQClient.BytesReceived = (int)frame.Length;

                        if (m_zeroMQClient.ReceiveBufferSize < m_zeroMQClient.BytesReceived)
                            m_zeroMQClient.SetReceiveBuffer(m_zeroMQClient.BytesReceived);

                        frame.Read(m_zeroMQClient.ReceiveBuffer, 0, m_zeroMQClient.BytesReceived);

                        m_zeroMQClient.Statistics.UpdateBytesReceived(m_zeroMQClient.BytesReceived);

                        // Notify consumer of received data
                        OnReceiveDataComplete(m_zeroMQClient.ReceiveBuffer, m_zeroMQClient.BytesReceived);
                    }
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
            buffer.ValidateParameters(startIndex, length);

            if (m_zeroMQClient.ReceiveBuffer == null)
                throw new InvalidOperationException("No received data buffer has been defined to read.");

            int sourceLength = m_zeroMQClient.BytesReceived - ReadIndex;
            int readBytes = length > sourceLength ? sourceLength : length;
            Buffer.BlockCopy(m_zeroMQClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

            // Update read index for next call
            ReadIndex += readBytes;

            if (ReadIndex >= m_zeroMQClient.BytesReceived)
                ReadIndex = 0;

            return readBytes;
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
            if (CurrentState != ClientState.Connected)
                throw new SocketException((int)SocketError.NotConnected);

            try
            {
                if (m_zeroMQClient.Provider != null)
                {
                    using (ZMessage message = new ZMessage())
                    {
                        // Dealer socket will auto-add identity, just add delimiter and data payload frames
                        message.Add(new ZFrame());
                        message.Add(new ZFrame(data, offset, length));

                        // ZeroMQ send is asynchronous, but API call is not thread-safe
                        lock (m_sendLock)
                            m_zeroMQClient.Provider.Send(message);
                    }

                    m_zeroMQClient.Statistics.UpdateBytesSent(length);
                }
            }
            catch (Exception ex)
            {
                // Log exception during send operation
                OnSendDataException(ex);
            }

            return m_completedHandle.WaitHandle;
        }

        /// <summary>
        /// Disconnects the <see cref="ZeroMQClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (CurrentState == ClientState.Disconnected)
                    return;

                if (m_connectionThread != null)
                {
                    m_connectionThread.Abort();
                    m_connectionThread = null;
                }

                if (m_zeroMQClient.Provider != null)
                {
                    m_zeroMQClient.Provider.Disconnect(ServerUri);
                    m_zeroMQClient.Reset();
                }

                OnConnectionTerminated();
            }
            catch (Exception ex)
            {
                OnDisconnectException(ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ZeroMQClient"/> and optionally releases the managed resources.
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

                if (m_completedHandle != null)
                {
                    m_completedHandle.Set();
                    m_completedHandle.Dispose();
                }

                m_zeroMQClient.Reset();
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

            // Make sure "interface" setting exists
            Transport.GetInterfaceIPStack(m_connectData);

            // Check if 'server' property is missing.
            if (!m_connectData.ContainsKey("server"))
                throw new ArgumentException($"Server property is missing (Example: {DefaultConnectionString})");

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888
            // Old Format: Server=localhost; Port=8888
            if (m_connectData.ContainsKey("port"))
                m_connectData["server"] = $"{m_connectData["server"]}:{m_connectData["port"]}";

            // For traditional style connection strings, also support a "zeroMQTransportProtocol" setting
            if (m_connectData.ContainsKey("zeroMQTransportProtocol"))
            {
                if (Enum.TryParse(m_connectData["zeroMQTransportProtocol"].Trim(), true, out ZeroMQTransportProtocol protocol))
                    m_zeroMQTransportProtocol = protocol;
            }

            // Validate ZeroMQ connection string
            Match endpointMatch = Regex.Match(m_connectData["server"], ZeroMQServer.EndpointFormatRegex);
            string port = null;
            bool validConnectionString = false;

            if (endpointMatch.Success)
            {
                if (!Enum.TryParse(endpointMatch.Groups["protocol"].Value.Trim(), true, out m_zeroMQTransportProtocol))
                    m_zeroMQTransportProtocol = ZeroMQTransportProtocol.Tcp;

                port = endpointMatch.Groups["port"].Value.Trim();
                validConnectionString = true;

                if (!string.IsNullOrWhiteSpace(m_connectData["interface"]) && (m_zeroMQTransportProtocol == ZeroMQTransportProtocol.Pgm || m_zeroMQTransportProtocol == ZeroMQTransportProtocol.Epgm))
                    m_connectData["server"] = $"{m_zeroMQTransportProtocol.ToString().ToLowerInvariant()}://{m_connectData["interface"]};{endpointMatch.Groups["host"].Value.Trim()}:{port}";
            }
            else
            {
                // Support traditional IClient "server" property
                endpointMatch = Regex.Match(m_connectData["server"], Transport.EndpointFormatRegex);

                if (endpointMatch.Success)
                {
                    m_connectData["server"] = $"{m_zeroMQTransportProtocol.ToString().ToLowerInvariant()}://{m_connectData["server"]}";
                    port = endpointMatch.Groups["port"].Value.Trim();
                    validConnectionString = true;

                    if (!string.IsNullOrWhiteSpace(m_connectData["interface"]) && (m_zeroMQTransportProtocol == ZeroMQTransportProtocol.Pgm || m_zeroMQTransportProtocol == ZeroMQTransportProtocol.Epgm))
                        m_connectData["server"] = $"{m_zeroMQTransportProtocol.ToString().ToLowerInvariant()}://{m_connectData["interface"]};{endpointMatch.Groups["host"].Value.Trim()}:{port}";
                }
            }

            if (!validConnectionString)
                throw new FormatException($"Server property is invalid (Example: {DefaultConnectionString})");

            if (m_zeroMQTransportProtocol != ZeroMQTransportProtocol.InProc && !Transport.IsPortNumberValid(port))
                throw new ArgumentOutOfRangeException(nameof(connectionString), $"Server port must between {Transport.PortRangeLow} and {Transport.PortRangeHigh}");
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected override void OnSendDataException(Exception ex)
        {
            if (ZeroMQServer.IsThreadAbortException(ex))
                return;

            if (CurrentState == ClientState.Disconnected)
                return;

            if (ex is ZException zmqex && (zmqex.Error.Number == ZError.EAGAIN.Number || zmqex.Error.Number == ZError.ETERM.Number))
                ThreadPool.QueueUserWorkItem(state => Disconnect());
            else
                base.OnSendDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            if (ZeroMQServer.IsThreadAbortException(ex))
                return;

            if (CurrentState != ClientState.Disconnected)
                base.OnReceiveDataException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ConnectionException"/> event.</param>
        protected override void OnConnectionException(Exception ex)
        {
            if (ZeroMQServer.IsThreadAbortException(ex))
                return;

            base.OnConnectionException(ex);
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event for disconnect exceptions.
        /// </summary>
        /// <param name="ex">Disconnect exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected virtual void OnDisconnectException(Exception ex)
        {
            if (ZeroMQServer.IsThreadAbortException(ex))
                return;

            OnSendDataException(new InvalidOperationException($"Disconnect exception: {ex.Message}", ex));
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.UnhandledUserException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.UnhandledUserException"/> event.</param>
        protected override void OnUnhandledUserException(Exception ex)
        {
            if (ZeroMQServer.IsThreadAbortException(ex))
                return;

            base.OnUnhandledUserException(ex);
        }

        #endregion
    }
}