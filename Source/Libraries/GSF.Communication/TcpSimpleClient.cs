//******************************************************************************************************
//  TcpSimpleClient.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/13/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GSF.Configuration;
using GSF.Diagnostics;
using TcpClientSocket = System.Net.Sockets.TcpClient;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a simple implementation of a TCP-based communication client.
    /// </summary>
    public class TcpSimpleClient : ClientBase
    {
        #region [ Members ]

        // Nested Types
        private class TcpClientProvider : TransportProvider<TcpClientSocket>
        {
            public int Offset;
            public int PayloadLength = -1;
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="PayloadAware"/> property.
        /// </summary>
        public const bool DefaultPayloadAware = false;

        /// <summary>
        /// Specifies the default value for the <see cref="NoDelay"/> property.
        /// </summary>
        public const bool DefaultNoDelay = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "Server=localhost:8888";

        // Fields
        private readonly TcpClientProvider m_tcpClient;
        private byte[] m_payloadMarker;
        private EndianOrder m_payloadEndianOrder;
        private IPStack m_ipStack;
        private string[] m_serverList;
        private Dictionary<string, string> m_connectData;
        private ManualResetEvent m_connectWaitHandle;
        private Func<Task> m_cancelReadAsync;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSimpleClient"/> class.
        /// </summary>
        public TcpSimpleClient() : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSimpleClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="TcpSimpleClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public TcpSimpleClient(string connectString) : base(TransportProtocol.Tcp, connectString)
        {
            m_tcpClient = new TcpClientProvider();
            PayloadAware = DefaultPayloadAware;
            m_payloadMarker = Payload.DefaultMarker;
            m_payloadEndianOrder = EndianOrder.LittleEndian;
            NoDelay = DefaultNoDelay;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSimpleClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="TcpSimpleClient"/>.</param>
        public TcpSimpleClient(IContainer container) : this() => container?.Add(this);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the payload boundaries are to be preserved during transmission.
        /// </summary>
        [Category("Data")]
        [DefaultValue(DefaultPayloadAware)]
        [Description("Indicates whether the payload boundaries are to be preserved during transmission.")]
        public bool PayloadAware { get; set; }

        /// <summary>
        /// Gets or sets the byte sequence used to mark the beginning of a payload in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <remarks>
        /// Setting property to <c>null</c> will create a zero-length payload marker.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] PayloadMarker
        {
            get => m_payloadMarker;
            set => m_payloadMarker = value ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets or sets the endian order to apply for encoding and decoding payload size in a <see cref="PayloadAware"/> transmission.
        /// </summary>
        /// <remarks>
        /// Setting property to <c>null</c> will force use of little-endian encoding.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EndianOrder PayloadEndianOrder
        {
            get => m_payloadEndianOrder;
            set => m_payloadEndianOrder = value ?? EndianOrder.LittleEndian;
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if small packets are delivered to the remote host without delay.
        /// </summary>
        [Category("Settings")]
        [DefaultValue(DefaultNoDelay)]
        [Description("Determines if small packets are delivered to the remote host without delay.")]
        public bool NoDelay { get; set; }

        /// <summary>
        /// Gets the <see cref="Socket"/> object for the <see cref="TcpSimpleClient"/>.
        /// </summary>
        [Browsable(false)]
        public Socket Client => m_tcpClient.Provider?.Client;

        /// <summary>
        /// Gets the server URI of the <see cref="TcpSimpleClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri => $"{TransportProtocol}://{ServerList[ServerIndex]}".ToLower();

        /// <summary>
        /// Determines whether the base class should track statistics.
        /// </summary>
        protected override bool TrackStatistics => false;

        // Gets server connect data as an array - will always be at least one empty string, not null
        private string[] ServerList
        {
            get
            {
                if (!(m_serverList is null))
                    return m_serverList;

                if (m_connectData is null || !m_connectData.TryGetValue("server", out string serverList) || string.IsNullOrWhiteSpace(serverList))
                    return Array.Empty<string>();

                return m_serverList = serverList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(server => server.Trim()).ToArray();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TcpClient"/> and optionally releases the managed resources.
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

                if (m_connectWaitHandle is not null)
                {
                    m_connectWaitHandle.Set();
                    m_connectWaitHandle.Dispose();
                    m_connectWaitHandle = null;
                }

                Task readTask = m_cancelReadAsync?.Invoke();

                if (!s_onReadThread)
                    readTask?.Wait(TimeSpan.FromSeconds(5.0D));
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Saves <see cref="TcpSimpleClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();

            if (!PersistSettings)
                return;

            // Save settings under the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings["PayloadAware", true].Update(PayloadAware);
            settings["NoDelay", true].Update(NoDelay);

            config.Save();
        }

        /// <summary>
        /// Loads saved <see cref="TcpSimpleClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();

            if (!PersistSettings)
                return;

            // Load settings from the specified category.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];

            settings.Add("PayloadAware", PayloadAware, "True if payload boundaries are to be preserved during transmission, otherwise False.");
            settings.Add("NoDelay", NoDelay, "True to disable Nagle so that small packets are delivered to the remote host without delay, otherwise False.");

            PayloadAware = settings["PayloadAware"].ValueAs(PayloadAware);
            NoDelay = settings["NoDelay"].ValueAs(NoDelay);
        }

        /// <summary>
        /// Connects the <see cref="TcpSimpleClient"/> to the server asynchronously.
        /// </summary>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            TcpClientSocket tcpClient = null;

            if (CurrentState != ClientState.Disconnected || m_disposed)
                return m_connectWaitHandle;

            void fail(Exception ex)
            {
                // Log exception during connection attempt
                OnConnectionException(ex);

                // ReSharper disable once AccessToDisposedClosure
                tcpClient?.Close();

                // Notify of terminated connection
                OnConnectionTerminated();

                // Ensure that the wait handle is set so that operations waiting
                // for completion of the asynchronous connection loop can continue
                m_connectWaitHandle?.Set();
            }

            try
            {
                // If we do not already have a wait handle to use for connections, get one from the base class
                if (m_connectWaitHandle is null)
                    m_connectWaitHandle = (ManualResetEvent)base.ConnectAsync();

                m_tcpClient.SetReceiveBuffer(ReceiveBufferSize);

                tcpClient = new TcpClientSocket(Transport.CreateEndPoint(m_connectData["interface"], 0, m_ipStack));

                using (m_tcpClient.Provider)
                    m_tcpClient.Provider = tcpClient;

                OnConnectionAttempt();
                m_connectWaitHandle.Reset();

                // Overwrite config file value if "noDelay" option exists in connection string
                if (m_connectData.TryGetValue("noDelay", out string noDelaySetting))
                    NoDelay = noDelaySetting.ParseBoolean();

                tcpClient.Client.NoDelay = NoDelay;

                Match endpoint = Regex.Match(ServerList[ServerIndex], Transport.EndpointFormatRegex);

                // Initiate the asynchronous connection loop
                tcpClient.ConnectAsync(endpoint.Groups["host"].Value, int.Parse(endpoint.Groups["port"].Value)).ContinueWith(_ =>
                {
                    try
                    {
                        // Notify of established connection
                        m_connectWaitHandle?.Set();
                        OnConnectionEstablished();

                        // Set up read cancellation before starting the read loop
                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
                        CancellationToken cancellationToken = cancellationTokenSource.Token;
                        int isCancelled = 0;

                        Task cancelAsync()
                        {
                            if (Interlocked.Exchange(ref isCancelled, 1) == 0)
                                cancellationTokenSource.Cancel();

                            return taskCompletionSource.Task;
                        }

                        // This ensures that no orphaned read loops can
                        // persist regardless of potential race conditions.
                        // If a previous read loop is replaced, we cancel it here.
                        Func<Task> cancelPreviousReadAsync = Interlocked.Exchange(ref m_cancelReadAsync, cancelAsync);
                        cancelPreviousReadAsync?.Invoke();

                        // Start continuous read loop
                        // ReSharper disable once MethodSupportsCancellation
                        Task.Run(async () =>
                        {
                            try
                            {
                                while (!cancellationToken.IsCancellationRequested && CurrentState == ClientState.Connected)
                                    await ReadDataAsync(cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                if (!(ex is ThreadAbortException))
                                    OnReceiveDataException(ex);
                            }
                            finally
                            {
                                taskCompletionSource.SetResult(null);
                                cancellationTokenSource.Dispose();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        fail(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                fail(ex);
            }
            finally
            {
                // If the operation was cancelled during execution,
                // make sure to dispose of erroneously allocated resources
                if (CurrentState == ClientState.Disconnected || m_disposed)
                    tcpClient?.Dispose();
            }

            // Return the wait handle that signals completion
            // of the asynchronous connection loop
            return m_connectWaitHandle;
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
        /// <exception cref="InvalidOperationException">Failed to get network stream.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if (m_tcpClient.ReceiveBuffer is null)
                throw new InvalidOperationException("No received data buffer has been defined to read.");

            int sourceLength = m_tcpClient.PayloadLength - ReadIndex;
            int readBytes = length > sourceLength ? sourceLength : length;
            Buffer.BlockCopy(m_tcpClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

            // Update read index for next call
            ReadIndex += readBytes;

            if (ReadIndex >= m_tcpClient.PayloadLength)
                ReadIndex = 0;

            return readBytes;
        }

        /// <summary>
        /// Requests that the client attempt to move to the next <see cref="ClientBase.ServerIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Return value will only be <c>true</c> if <see cref="ClientBase.ServerIndex"/> changed.
        /// </remarks>
        public override bool RequestNextServerIndex()
        {
            int serverListLength = ServerList.Length;

            if (serverListLength < 2)
                return false;

            // When multiple servers are available, move to next server connection
            ServerIndex++;

            if (ServerIndex >= serverListLength)
                ServerIndex = 0;

            return true;
        }

        // Reads data from the TcpClient stream.
        private async Task ReadDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream stream = m_tcpClient.Provider?.GetStream();

                if (stream is null)
                    throw new InvalidOperationException("Failed to get network stream.");

                while (!cancellationToken.IsCancellationRequested)
                {
                    int length;

                    if (PayloadAware)
                    {
                        if (m_tcpClient.PayloadLength < 0)
                            length = m_payloadMarker.Length + Payload.LengthSegment;
                        else
                            length = m_tcpClient.PayloadLength;
                    }
                    else
                    {
                        length = m_tcpClient.ReceiveBufferSize;
                    }

                    // Retrieve data from the socket
                    m_tcpClient.BytesReceived = await stream.ReadAsync(m_tcpClient.ReceiveBuffer, m_tcpClient.Offset, length - m_tcpClient.Offset, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        return;
                    
                    if (PayloadAware)
                        m_tcpClient.Offset += m_tcpClient.BytesReceived;

                    if (PayloadAware)
                    {
                        if (m_tcpClient.PayloadLength < 0)
                        {
                            m_tcpClient.PayloadLength = Payload.ExtractLength(m_tcpClient.ReceiveBuffer, m_tcpClient.Offset, m_payloadMarker, m_payloadEndianOrder);

                            if (m_tcpClient.PayloadLength > 0)
                            {
                                m_tcpClient.Offset = 0;

                                if (m_tcpClient.ReceiveBuffer.Length < m_tcpClient.PayloadLength)
                                    m_tcpClient.SetReceiveBuffer(m_tcpClient.PayloadLength);
                            }
                        }
                        else if (m_tcpClient.Offset == m_tcpClient.PayloadLength)
                        {
                            // Notify consumer that entire data payload has been received
                            OnReceiveDataComplete(m_tcpClient.ReceiveBuffer, m_tcpClient.PayloadLength);

                            // Reset payload length
                            m_tcpClient.Offset = 0;
                            m_tcpClient.PayloadLength = -1;
                        }
                    }
                    else
                    {
                        // Notify consumer that data has been received
                        OnReceiveDataComplete(m_tcpClient.ReceiveBuffer, m_tcpClient.BytesReceived);
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if (!(ex is NullReferenceException))
                    OnReceiveDataException(ex);

                // Terminate connection on read exceptions
                TerminateConnectionOnReadThread();
            }
        }

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Failed to get network stream.</exception>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            if (CurrentState == ClientState.Disconnected)
                throw new InvalidOperationException("Cannot send on disconnected client");

            // Prepare for payload-aware transmission.
            if (PayloadAware)
                Payload.AddHeader(ref data, ref offset, ref length, m_payloadMarker, m_payloadEndianOrder);

            NetworkStream stream = m_tcpClient.Provider?.GetStream();

            if (stream is null)
                throw new InvalidOperationException("Failed to get network stream.");

            Task result = stream.WriteAsync(data, offset, length);

            return ((IAsyncResult)result).AsyncWaitHandle;
        }

        /// <summary>
        /// Disconnects the <see cref="TcpSimpleClient"/> from the connected server synchronously.
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (CurrentState == ClientState.Disconnected)
                    return;
                
                m_tcpClient.Provider?.Dispose();
                m_connectWaitHandle?.Set();

                Task readTask = m_cancelReadAsync?.Invoke();

                if (!s_onReadThread)
                {
                    if (readTask is not null && !readTask.Wait(TimeSpan.FromSeconds(15.0D)))
                        throw new TimeoutException("Timeout waiting for read cancellation.");
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
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">Server property is missing.</exception>
        /// <exception cref="FormatException">Server property is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Server port value is not between <see cref="Transport.PortRangeLow"/> and <see cref="Transport.PortRangeHigh"/>.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            // Derive desired IP stack based on specified "interface" setting, adding setting if it's not defined
            m_ipStack = Transport.GetInterfaceIPStack(m_connectData);

            // Check if 'server' property is missing.
            if (!m_connectData.ContainsKey("server") || string.IsNullOrWhiteSpace(m_connectData["server"]))
                throw new ArgumentException($"Server property is missing (Example: {DefaultConnectionString})");

            // Backwards compatibility adjustments.
            // New Format: Server=localhost:8888
            // Old Format: Server=localhost; Port=8888
            if (m_connectData.ContainsKey("port") && !m_connectData["server"].Contains(','))
                m_connectData["server"] = $"{m_connectData["server"]}:{m_connectData["port"]}";

            m_serverList = null;

            foreach (string server in ServerList)
            {
                // Check if 'server' property is valid.
                Match endpoint = Regex.Match(server, Transport.EndpointFormatRegex);

                if (endpoint == Match.Empty)
                    throw new FormatException($"Server property is invalid (Example: {DefaultConnectionString})");

                if (!Transport.IsPortNumberValid(endpoint.Groups["port"].Value))
                    throw new ArgumentOutOfRangeException(nameof(connectionString), $"Server port must between {Transport.PortRangeLow} and {Transport.PortRangeHigh}");
            }
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.SendDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.SendDataException"/> event.</param>
        protected override void OnSendDataException(Exception ex)
        {
            if (CurrentState != ClientState.Disconnected)
                base.OnSendDataException(ex);
            else
                Logger.SwallowException(ex, $"{nameof(TcpSimpleClient)}: The client state was disconnected");
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataComplete"/> event.
        /// </summary>
        /// <param name="data">Data received from the client.</param>
        /// <param name="size">Number of bytes received from the client.</param>
        protected override void OnReceiveDataComplete(byte[] data, int size)
        {
            s_onReadThread = true;
            try { base.OnReceiveDataComplete(data, size); }
            finally { s_onReadThread = false; }
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event for <see cref="SocketException"/>.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected void OnReceiveDataException(SocketException ex)
        {
            if (ex.SocketErrorCode != SocketError.Disconnecting)
                OnReceiveDataException((Exception)ex);
            else
                Logger.SwallowException(ex, $"{nameof(TcpSimpleClient)}: The socket was disconnecting");
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ReceiveDataException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ReceiveDataException"/> event.</param>
        protected override void OnReceiveDataException(Exception ex)
        {
            s_onReadThread = true;

            try
            {
                if (CurrentState != ClientState.Disconnected)
                    base.OnReceiveDataException(ex);
                else
                    Logger.SwallowException(ex, $"{nameof(TcpSimpleClient)}: The socket was disconnected");
            }
            finally
            {
                s_onReadThread = false;
            }
        }

        private void TerminateConnectionOnReadThread()
        {
            s_onReadThread = true;
            try { OnConnectionTerminated(); }
            finally { s_onReadThread = false; }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        [ThreadStatic]
        private static bool s_onReadThread;

        #endregion
    }
}
