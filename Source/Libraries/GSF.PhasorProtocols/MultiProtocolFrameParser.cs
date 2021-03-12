//******************************************************************************************************
//  MultiProtocolFrameParser.cs - Gbtc
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
//  03/16/2006 - J. Ritchie Carroll
//       Initial version of source generated.
//  06/26/2006 - Pinal C. Patel
//       Changed out the socket code with TcpClient and UdpClient components from GSF.Communication.
//  01/31/2007 - J. Ritchie Carroll
//       Added TCP "server" support to allow listening connections from devices that act as data
//       clients, e.g., F-NET devices.
//  05/23/2007 - Pinal C. Patel
//       Added member variable 'm_clientConnectionAttempts' to track the number of attempts made for
//       connecting to the server since this information is no longer provided by the event raised by
//       any of the Communication Client components.
//  07/05/2007 - J. Ritchie Carroll
//       Wrapped all event raising for frame parsing in Try/Catch so that any exceptions thrown in
//       consumer event handlers won't have a negative effect on continuous data parsing - exceptions
//       in consumer event handlers are duly noted and raised through the ParsingException event.
//  09/28/2007 - J. Ritchie Carroll
//       Implemented new disconnect overload on communications client that allows timeout on socket
//       close to fix an issue related non-responsive threads that "lock-up" after sending connection
//       commands that attempt to close the socket for remotely connected devices.
//  12/14/2007 - J. Ritchie Carroll
//       Implemented simulated timestamp injection for published frames to allow for real-time
//       data simulations from archived sample data.
//  10/28/2008 - J. Ritchie Carroll
//       Added support for SEL's UDP_T and UDP_U protocol implementations (UDP_S was already supported),
//       implementation handled by allowing definition of a "CommandChannel" in the connection string.
//  04/27/2009 - J. Ritchie Carroll
//       Added support for SEL Fast Message protocol.
//  02/12/2010 - Pinal C. Patel
//       Modified to start the IFrameParser object in InitializeFrameParser() instead of Start().
//  03/20/2010 - J. Ritchie Carroll
//       Added property "SkipDisableRealTimeData" to allow consumer to bypass sending the command to
//       turn off the real-time data stream when automatically starting the data parsing sequence. This
//       is useful when using UDP multicast that may have many listeners, in these cases you don't want
//       to disable the stream on startup or shutdown since other applications may be subscribed to the
//       real-time stream.
//  03/21/2010 - J. Ritchie Carroll
//       Added parsing exception threshold settings and consumer event to handle situation.
//  06/13/2010 - J. Ritchie Carroll
//       Added several more run-time statistics to the frame parser (e.g., missing frames, CRC errors).
//  08/10/2010 - J. Ritchie Carroll
//       Added code to handle high-resolution input timing to support accurate input simulations.
//  05/06/2010- Jian (Ryan) Zuo
//       Updated to exclude non-data frames from frame counts and injected waiting periods.
//  05/19/2011 - Ritchie
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.Communication;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Parsing;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.PhasorProtocols.Macrodyne;
using GSF.PhasorProtocols.SelFastMessage;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.Units;
using GSF.Units.EE;
using TcpClient = GSF.Communication.TcpClient;
using UdpClient = GSF.Communication.UdpClient;

// Ignore warnings about unused events that are required by IClient
#pragma warning disable 67

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor data protocols enumeration.
    /// </summary>
    [Serializable]
    public enum PhasorProtocol
    {
        /// <summary>
        /// IEEE C37.118.2-2011 protocol.
        /// </summary>
        IEEEC37_118V2,
        /// <summary>
        /// IEEE C37.118-2005 protocol.
        /// </summary>
        IEEEC37_118V1,
        /// <summary>
        /// IEEE C37.118, draft 6 protocol.
        /// </summary>
        IEEEC37_118D6,
        /// <summary>
        /// IEEE 1344-1995 protocol.
        /// </summary>
        IEEE1344,
        /// <summary>
        /// BPA PDCstream protocol.
        /// </summary>
        BPAPDCstream,
        /// <summary>
        /// UTK F-NET protocol.
        /// </summary>
        FNET,
        /// <summary>
        /// SEL Fast Message protocol.
        /// </summary>
        SelFastMessage,
        /// <summary>
        /// Macrodyne protocol.
        /// </summary>
        Macrodyne,
        /// <summary>
        /// IEC 61850-90-5 protocol.
        /// </summary>
        IEC61850_90_5
    }

    #endregion

    /// <summary>
    /// Protocol independent frame parser.
    /// </summary>
    /// <remarks>
    /// This class takes all protocol frame parsing implementations and reduces them to a single simple-to-use class exposing all
    /// data through abstract interfaces (e.g., IConfigurationFrame, IDataFrame, etc.) - this way new protocol implementations can
    /// be added without adversely affecting consuming code. Additionally, this class implements a variety of transport options
    /// (e.g., TCP, UDP, Serial, etc.) and hides the complexities of this connectivity and internally pushes all data received from
    /// the selected transport protocol to the selected phasor parsing protocol.
    /// </remarks>
    public sealed class MultiProtocolFrameParser : IFrameParser
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Creates an instance of a TCP server that can be shared for multiple devices (e.g., an F-NET server)
        /// </summary>
        private sealed class SharedTcpServerReference : IServer
        {
            #region [ Members ]

            // Events

            /// <summary>
            /// Occurs when the server is started.
            /// </summary>
            public event EventHandler ServerStarted;

            /// <summary>
            /// Occurs when the server is stopped.
            /// </summary>
            public event EventHandler ServerStopped;

            /// <summary>
            /// Occurs when a client connects to the server.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that connected to the server.
            /// </remarks>
            public event EventHandler<EventArgs<Guid>> ClientConnected;

            /// <summary>
            /// Occurs when a client disconnects from the server.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that disconnected from the server.
            /// </remarks>
            public event EventHandler<EventArgs<Guid>> ClientDisconnected;

            /// <summary>
            /// Occurs when an exception is encountered while a client is connecting.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when connecting to the client.
            /// </remarks>
            public event EventHandler<EventArgs<Exception>> ClientConnectingException;

            /// <summary>
            /// Occurs when data is being sent to a client.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data is being sent.
            /// </remarks>
            public event EventHandler<EventArgs<Guid>> SendClientDataStart;

            /// <summary>
            /// Occurs when data has been sent to a client.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data has been sent.
            /// </remarks>
            public event EventHandler<EventArgs<Guid>> SendClientDataComplete;

            /// <summary>
            /// Occurs when an <see cref="Exception"/> is encountered when sending data to a client.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client to which the data was being sent.<br/>
            /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when sending data to a client.
            /// </remarks>
            public event EventHandler<EventArgs<Guid, Exception>> SendClientDataException;

            /// <summary>
            /// Occurs when unprocessed data has been received from a client.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This event can be used to receive a notification that client data has arrived. The <see cref="Read"/> method can then be used
            /// to copy data to an existing buffer. In many cases it will be optimal to use an existing buffer instead of subscribing to the
            /// <see cref="ReceiveClientDataComplete"/> event, however, the data that is available after calling the <see cref="Read"/> method
            /// will be the original unprocessed data received by the client, i.e., not optionally decrypted or decompressed data.
            /// </para>
            /// <para>
            /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client from which data is received.<br/>
            /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of bytes received in the buffer from the client.
            /// </para>
            /// </remarks>
            public event EventHandler<EventArgs<Guid, int>> ReceiveClientData;

            /// <summary>
            /// Occurs when data received from a client has been processed and is ready for consumption.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is the ID of the client from which data is received.<br/>
            /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is a new buffer containing post-processed data received from the client starting at index zero.<br/>
            /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is the number of post-processed bytes received in the buffer from the client.
            /// </remarks>
            public event EventHandler<EventArgs<Guid, byte[], int>> ReceiveClientDataComplete;

            /// <summary>
            /// Occurs when an <see cref="Exception"/> is encountered when receiving data from a client.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client from which the data was being received.<br/>
            /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when receiving data from a client.
            /// </remarks>
            public event EventHandler<EventArgs<Guid, Exception>> ReceiveClientDataException;

            /// <summary>
            /// Occurs when an <see cref="Exception"/> is encountered in a user-defined function via an event dispatch.
            /// </summary>
            /// <remarks>
            /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> thrown by the user-defined function.
            /// </remarks>
            public event EventHandler<EventArgs<Exception>> UnhandledUserException;

            /// <summary>
            /// Raised after the source object has been properly disposed.
            /// </summary>
            public event EventHandler Disposed;

            // Fields
            private TcpServer m_tcpServer;
            private IPAddress m_remoteAddress;
            private Guid m_clientID;

        #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets the name of the object providing status information.
            /// </summary>
            public string Name => m_tcpServer?.Name;

            /// <summary>
            /// Gets the current status details about object providing status information.
            /// </summary>
            public string Status => m_tcpServer?.Status ?? string.Empty;

            /// <summary>
            /// Gets or sets the data required by the server to initialize.
            /// </summary>
            public string ConfigurationString { get; set; }

            /// <summary>
            /// Gets or sets the maximum number of clients that can connect to the server.
            /// </summary>
            public int MaxClientConnections
            {
                get => m_tcpServer?.MaxClientConnections ?? ServerBase.DefaultMaxClientConnections;
                set { } // Ignore this setting since we need to share
            }

            /// <summary>
            /// Gets or sets the size of the buffer used by the client for receiving data from the server.
            /// </summary>
            public int SendBufferSize
            {
                get => m_tcpServer?.SendBufferSize ?? ServerBase.DefaultSendBufferSize;
                set { } // Ignore this setting since we need to share
            }

            /// <summary>
            /// Gets or sets the size of the buffer used by the server for receiving data from the clients.
            /// </summary>
            public int ReceiveBufferSize { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the connected clients.
            /// </summary>
            public Encoding TextEncoding
            {
                get => m_tcpServer?.TextEncoding;
                set { } // Ignore this setting since we need to share
            }

            /// <summary>
            /// Gets the current <see cref="ServerState"/>.
            /// </summary>
            public ServerState CurrentState => m_tcpServer?.CurrentState ?? ServerState.NotRunning;

            /// <summary>
            /// Gets the <see cref="TransportProtocol"/> used by the server for the transportation of data with the clients.
            /// </summary>
            public TransportProtocol TransportProtocol => TransportProtocol.Tcp;

            /// <summary>
            /// Gets the server's ID.
            /// </summary>
            public Guid ServerID => m_tcpServer?.ServerID ?? Guid.Empty;

            /// <summary>
            /// Gets the IDs of clients connected to the server.
            /// </summary>
            public Guid[] ClientIDs => m_tcpServer?.ClientIDs ?? Array.Empty<Guid>();

            /// <summary>
            /// Gets the <see cref="Time"/> for which the server has been running.
            /// </summary>
            public Time RunTime => m_tcpServer?.RunTime ?? 0.0D;

            /// <summary>
            /// Gets or sets a boolean value that indicates whether the object is enabled.
            /// </summary>
            public bool Enabled
            {
                get => m_tcpServer?.Enabled ?? false;
                // ReSharper disable once ValueParameterNotUsed
                set
                {
                    // value parameter not used here intentionally
                    if (!(m_tcpServer is null))
                        m_tcpServer.Enabled = true;
                }
            }

            /// <summary>
            /// Gets a flag that indicates whether the object has been disposed.
            /// </summary>
            public bool IsDisposed { get; private set; }

        #endregion

            #region [ Methods ]

            /// <summary>
            /// Initializes the state of the object.
            /// </summary>
            public void Initialize()
            {
                Dictionary<string, string> settings = ConfigurationString.ParseKeyValuePairs();

                if (!settings.TryGetValue("receiveFrom", out string receiveFromSetting))
                    return;

                IPStack ipStack = Transport.GetInterfaceIPStack(settings);
                m_remoteAddress = Transport.CreateEndPoint(receiveFromSetting, 0, ipStack).Address;
            }

            /// <summary>
            /// Starts the server.
            /// </summary>
            public void Start() => GetSharedServer();

            /// <summary>
            /// Stops the server.
            /// </summary>
            public void Stop() => ReturnSharedServer();

            /// <summary>
            /// Disconnects a connected client.
            /// </summary>
            /// <param name="clientID">ID of the client to be disconnected.</param>
            public void DisconnectOne(Guid clientID)
            {
                if (clientID == m_clientID)
                    m_tcpServer?.DisconnectOne(clientID);
            }

            /// <summary>
            /// Disconnects all of the connected clients.
            /// </summary>
            public void DisconnectAll() => m_tcpServer?.DisconnectOne(m_clientID);

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (IsDisposed)
                    return;

                try
                {
                    ReturnSharedServer();
                }
                finally
                {
                    IsDisposed = true;
                }
            }

            /// <summary>
            /// Sends data to the specified client synchronously.
            /// </summary>
            /// <param name="clientID">ID of the client to which the data is to be sent.</param>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            public void SendTo(Guid clientID, byte[] data, int offset, int length) => SendToAsync(m_clientID, data, offset, length).WaitOne();

            /// <summary>
            /// Sends data to all of the connected clients synchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            public void Multicast(byte[] data, int offset, int length) => WaitHandle.WaitAll(MulticastAsync(data, offset, length));

            /// <summary>
            /// Sends data to the specified client asynchronously.
            /// </summary>
            /// <param name="clientID">ID of the client to which the data is to be sent.</param>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
            public WaitHandle SendToAsync(Guid clientID, byte[] data, int offset, int length) => m_tcpServer?.SendToAsync(clientID, data, offset, length) ?? new ManualResetEvent(true);

            /// <summary>
            /// Sends data to all of the connected clients asynchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
            public WaitHandle[] MulticastAsync(byte[] data, int offset, int length) => m_tcpServer is null ? Array.Empty<WaitHandle>() : new[] { m_tcpServer.SendToAsync(m_clientID, data, offset, length) };

            /// <summary>
            /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
            /// </summary>
            /// <param name="clientID">ID of the client from which data buffer should be read.</param>
            /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
            /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
            /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
            /// <returns>The number of bytes read.</returns>
            /// <remarks>
            /// This function should only be called from within the <see cref="ReceiveClientData"/> event handler. Calling this method outside this
            /// event will have unexpected results.
            /// </remarks>
            public int Read(Guid clientID, byte[] buffer, int startIndex, int length) => m_tcpServer?.Read(clientID, buffer, startIndex, length) ?? 0;

            /// <summary>
            /// Gets a reference to the shared server listening
            /// on this server's local end point.
            /// </summary>
            /// <returns>A reference to a shared server.</returns>
            private void GetSharedServer()
            {
                const string ConfigurationMismatchError = "Configuration mismatch detected between parsers using shared TCP server: {0}";

                lock (s_sharedServers)
                {
                    EndPoint localEndPoint = GetLocalEndPoint();
                    bool sharing = s_sharedServers.TryGetValue(localEndPoint, out TcpServer sharedServer);

                    if (sharing)
                    {
                        // Validate settings to ensure that they match
                        if (sharedServer.ReceiveBufferSize != ReceiveBufferSize)
                            throw new InvalidOperationException(string.Format(ConfigurationMismatchError, "Receive buffer size"));
                    }
                    else
                    {
                        // Create new client and add it to the shared list
                        sharedServer = new TcpServer();
                        s_sharedServers.Add(localEndPoint, sharedServer);
                        s_sharedReferenceCount.Add(localEndPoint, 0);
                    }

                    // Set the TCP server member variable ASAP to guarantee
                    // that it is set before callbacks can be triggered
                    m_tcpServer = sharedServer;

                    // Attach to event handlers
                    sharedServer.ClientConnected += SharedServer_ClientConnected;
                    sharedServer.ClientDisconnected += SharedServer_ClientDisconnected;
                    sharedServer.ClientConnectingException += SharedServer_ClientConnectingException;
                    sharedServer.ReceiveClientData += SharedServer_ReceiveClientData;
                    sharedServer.ReceiveClientDataException += SharedServer_ReceiveClientDataException;
                    sharedServer.SendClientDataException += SharedServer_SendClientDataException;
                    sharedServer.ServerStarted += SharedServer_ServerStarted;
                    sharedServer.ServerStopped += SharedServer_ServerStopped;
                    sharedServer.UnhandledUserException += SharedServer_UnhandledUserException;

                    if (!sharing)
                    {
                        // Initialize settings and connect
                        sharedServer.ConfigurationString = ConfigurationString;
                        sharedServer.ReceiveBufferSize = ReceiveBufferSize;
                        sharedServer.Start();
                    }

                    // Increment reference count
                    s_sharedReferenceCount[localEndPoint]++;
                }
            }

            /// <summary>
            /// Releases a reference to this server's shared server,
            /// and disposes of the shared server if nobody is using it.
            /// </summary>
            private void ReturnSharedServer()
            {
                lock (s_sharedServers)
                {
                    if (m_tcpServer is null)
                        return;

                    // Detach from event handlers
                    m_tcpServer.ClientConnected -= SharedServer_ClientConnected;
                    m_tcpServer.ClientDisconnected -= SharedServer_ClientDisconnected;
                    m_tcpServer.ReceiveClientData -= SharedServer_ReceiveClientData;
                    m_tcpServer.ReceiveClientDataException -= SharedServer_ReceiveClientDataException;
                    m_tcpServer.SendClientDataException -= SharedServer_SendClientDataException;
                    m_tcpServer.ServerStarted -= SharedServer_ServerStarted;
                    m_tcpServer.ServerStopped -= SharedServer_ServerStopped;
                    m_tcpServer.UnhandledUserException -= SharedServer_UnhandledUserException;

                    // Decrement reference count
                    EndPoint localEndPoint = GetLocalEndPoint();
                    s_sharedReferenceCount[localEndPoint]--;

                    if (s_sharedReferenceCount[localEndPoint] == 0)
                    {
                        // No more references to TCP server
                        // exist so dispose of it
                        m_tcpServer.Stop();
                        m_tcpServer.Dispose();
                        s_sharedServers.Remove(localEndPoint);
                        s_sharedReferenceCount.Remove(localEndPoint);
                    }

                    // Release reference to TCP server
                    m_tcpServer = null;
                }
            }

            /// <summary>
            /// Terminates the server as quickly as possible and
            /// removes it from the collection of shared servers.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            private void TerminateSharedClient()
            {
                lock (s_sharedServers)
                {
                    EndPoint localEndPoint = GetLocalEndPoint();

                    if (s_sharedServers.TryGetValue(localEndPoint, out TcpServer sharedServer))
                    {
                        // If the wrapped server and the shared server are the same,
                        // no one has terminated the shared server yet
                        if (m_tcpServer == sharedServer)
                        {
                            // Disconnect and dispose of the old server
                            m_tcpServer.Stop();
                            m_tcpServer.Dispose();

                            // Remove the old server from the
                            // collection of shared servers
                            localEndPoint = GetLocalEndPoint();
                            s_sharedServers.Remove(localEndPoint);
                            s_sharedReferenceCount.Remove(localEndPoint);
                        }
                    }

                    m_tcpServer = null;
                }
            }

            /// <summary>
            /// Determines the local end point this server intends
            /// to listen on via configuration string properties.
            /// </summary>
            /// <returns>The local end point.</returns>
            private EndPoint GetLocalEndPoint()
            {
                Dictionary<string, string> settings = ConfigurationString.ParseKeyValuePairs();
                IPStack ipStack = Transport.GetInterfaceIPStack(settings);

                if (!settings.TryGetValue("interface", out string localInterface))
                    localInterface = string.Empty;

                if (!settings.TryGetValue("port", out string localPortSetting))
                    throw new InvalidOperationException($"Local port property missing from connection string: {ConfigurationString}");

                if (!int.TryParse(localPortSetting, out int localPort))
                    throw new InvalidOperationException($"Unable to parse local port from \"{localPortSetting}\".");

                return Transport.CreateEndPoint(localInterface, localPort, ipStack);
            }

            // Shared server client connected handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ClientConnected(object sender, EventArgs<Guid> e)
            {
                Guid clientID = e.Argument;

                if (!m_tcpServer.TryGetClient(clientID, out TransportProvider<Socket> client))
                    return;

                if (!(client.Provider.RemoteEndPoint is IPEndPoint remoteEndPoint))
                    return;

                if (!remoteEndPoint.Address.Equals(m_remoteAddress))
                    return;

                m_clientID = clientID;
                ClientConnected?.Invoke(this, e);
            }

            // Shared server client disconnected handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ClientDisconnected(object sender, EventArgs<Guid> e)
            {
                if (e.Argument == m_clientID)
                    ClientDisconnected?.Invoke(this, e);
            }

            // Shared server client connecting exception handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ClientConnectingException(object sender, EventArgs<Exception> e) => ClientConnectingException?.Invoke(this, e);

            // Shared server receive client data handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ReceiveClientData(object sender, EventArgs<Guid, int> e)
            {
                if (e.Argument1 == m_clientID)
                    ReceiveClientData?.Invoke(this, e);
            }

            // Shared server receive client data exception handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
            {
                if (e.Argument1 == m_clientID)
                    ReceiveClientDataException?.Invoke(this, e);
            }

            // Shared server send client data exception handler.
            // Forwards event to users attached to this server.
            private void SharedServer_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
            {
                if (e.Argument1 == m_clientID)
                    SendClientDataException?.Invoke(this, e);
            }

            // Shared server server started handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ServerStarted(object sender, EventArgs e) => ServerStarted?.Invoke(this, e);

            // Shared server server stopped handler.
            // Forwards event to users attached to this server.
            private void SharedServer_ServerStopped(object sender, EventArgs e) => ServerStopped?.Invoke(this, e);

            // Shared server unhandled user exception handler.
            // Forwards event to users attached to this server.
            private void SharedServer_UnhandledUserException(object sender, EventArgs<Exception> e) => UnhandledUserException?.Invoke(this, e);

            #endregion

            #region [ Static ]

            // Static Fields
            private static readonly Dictionary<EndPoint, TcpServer> s_sharedServers = new Dictionary<EndPoint, TcpServer>();
            private static readonly Dictionary<EndPoint, int> s_sharedReferenceCount = new Dictionary<EndPoint, int>();

            #endregion
        }

        /// <summary>
        /// Shared UDP client reference.
        /// </summary>
        /// <remarks>
        /// This class is used to create multiple IClient instances which share a single UDP client.<br/>
        /// One shared UDP client instance will be created per local end point.
        /// </remarks>
        private sealed class SharedUdpClientReference : IClient
        {
            #region [ Members ]

            // Events

            public event EventHandler ConnectionAttempt;
            public event EventHandler ConnectionEstablished;
            public event EventHandler ConnectionTerminated;
            public event EventHandler<EventArgs<Exception>> ConnectionException;

            public event EventHandler SendDataStart;
            public event EventHandler SendDataComplete;
            public event EventHandler<EventArgs<Exception>> SendDataException;

            public event EventHandler<EventArgs<int>> ReceiveData;
            public event EventHandler<EventArgs<byte[], int>> ReceiveDataComplete;
            public event EventHandler<EventArgs<EndPoint, IPPacketInformation, int>> ReceiveDataFrom;
            public event EventHandler<EventArgs<Exception>> ReceiveDataException;

            public event EventHandler<EventArgs<Exception>> UnhandledUserException;
            public event EventHandler Disposed;

            // Fields
            private UdpClient m_udpClient;
            private EndPoint m_sendDestination;
            private IPAddress m_multicastServerAddress;
            private IPAddress m_multicastSourceAddress;
            private bool m_receivePacketInfo;

        #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the data required by the client to connect to the server.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// Gets the <see cref="Time"/> for which the client has been connected to the server.
            /// </summary>
            public Time ConnectionTime => m_udpClient?.ConnectionTime ?? 0.0D;

            /// <summary>
            /// Gets the current <see cref="ClientState"/>.
            /// </summary>
            public ClientState CurrentState => m_udpClient?.CurrentState ?? ClientState.Disconnected;

            /// <summary>
            /// Gets or sets the maximum number of times the client will attempt to connect to the server.
            /// </summary>
            /// <remarks>Set <see cref="MaxConnectionAttempts"/> to -1 for infinite connection attempts.</remarks>
            public int MaxConnectionAttempts { get; set; }

            /// <summary>
            /// Gets or sets the size of the buffer used by the client for receiving data from the server.
            /// </summary>
            /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
            public int ReceiveBufferSize { get; set; }

            /// <summary>
            /// Gets or sets the size of the buffer used by the client for sending data to the server.
            /// </summary>
            /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
            public int SendBufferSize
            {
                get => m_udpClient?.SendBufferSize ?? 0;
                set { }
            }

            /// <summary>
            /// Gets the server URI.
            /// </summary>
            public string ServerUri => m_udpClient?.ServerUri;
            
            /// <summary>
            /// Gets the current server index, when multiple server end points are defined.
            /// </summary>
            public int ServerIndex => 0;

            /// <summary>
            /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
            /// </summary>
            public Encoding TextEncoding
            {
                get => m_udpClient?.TextEncoding;
                set { }
            }

            /// <summary>
            /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
            /// </summary>
            public TransportProtocol TransportProtocol => TransportProtocol.Udp;

            /// <summary>
            /// Gets or sets a boolean value that indicates whether the client is currently enabled.
            /// </summary>
            /// <remarks>
            /// Setting <see cref="Enabled"/> to true will start connection cycle for the client if it
            /// is not connected, setting to false will disconnect the client if it is connected.
            /// </remarks>
            public bool Enabled
            {
                get => m_udpClient?.Enabled ?? false;
                set
                {
                    if (!(m_udpClient is null))
                        m_udpClient.Enabled = value;
                }
            }

            /// <summary>
            /// Gets a flag that indicates whether the object has been disposed.
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            /// Gets the unique identifier of the client.
            /// </summary>
            public string Name => m_udpClient?.Name;

            /// <summary>
            /// Gets or sets the flag that determines whether the UDP client
            /// should attempt to receive packet info when receiving data
            /// from the socket.
            /// </summary>
            public bool ReceivePacketInfo
            {
                // ReSharper disable once UnusedMember.Local
                get => m_udpClient?.ReceivePacketInfo ?? m_receivePacketInfo;
                set
                {
                    if (!(m_udpClient is null))
                        m_udpClient.ReceivePacketInfo = value;

                    m_receivePacketInfo = value;
                }
            }

            /// <summary>
            /// Gets the <see cref="TransportStatistics"/> for the client connection.
            /// </summary>
            public TransportStatistics Statistics => m_udpClient.Statistics;

            /// <summary>
            /// Gets the descriptive status of the client.
            /// </summary>
            public string Status => m_udpClient?.Status ?? string.Empty;

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Connects the client to the server synchronously.
            /// </summary>
            public void Connect() => ConnectAsync();

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
            public WaitHandle ConnectAsync()
            {
                Dictionary<string, string> settings = ConnectionString.ParseKeyValuePairs();

                // Set up destination used for send operations
                if (settings.TryGetValue("server", out string serverSetting))
                {
                    if (settings.TryGetValue("remotePort", out string remotePortSetting))
                        serverSetting = $"{serverSetting}:{remotePortSetting}";

                    Match endPointMatch = Regex.Match(serverSetting, Transport.EndpointFormatRegex);

                    if (int.TryParse(endPointMatch.Groups["port"].Value, out int remotePort))
                    {
                        IPStack ipStack = Transport.GetInterfaceIPStack(settings);
                        IPEndPoint sendDestination = Transport.CreateEndPoint(endPointMatch.Groups["host"].Value, remotePort, ipStack);
                        m_sendDestination = sendDestination;

                        if (Transport.IsMulticastIP(sendDestination.Address))
                        {
                            m_multicastServerAddress = sendDestination.Address;

                            if (settings.TryGetValue("multicastSource", out string multicastSourceSetting))
                                m_multicastSourceAddress = IPAddress.Parse(multicastSourceSetting);
                        }
                    }
                }

                GetSharedClient();

                return null;
            }

            /// <summary>
            /// When overridden in a derived class, disconnects client from the server synchronously.
            /// </summary>
            public void Disconnect()
            {
                if (!(m_multicastServerAddress is null))
                    m_udpClient.DropMulticastMembership(m_multicastServerAddress, m_multicastSourceAddress);

                ReturnSharedClient();
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
            public int Read(byte[] buffer, int startIndex, int length) => m_udpClient?.Read(buffer, startIndex, length) ?? 0;

            /// <summary>
            /// Requests that the client attempt to move to the next <see cref="ServerIndex"/>.
            /// </summary>
            /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
            public bool RequestNextServerIndex() => false;

            /// <summary>
            /// Sends data to the server synchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            public void Send(byte[] data, int offset, int length) => SendAsync(data, offset, length).WaitOne();

            /// <summary>
            /// Sends data to the server asynchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
            public WaitHandle SendAsync(byte[] data, int offset, int length) => m_sendDestination is null ? new ManualResetEvent(true) : m_udpClient.SendDataToAsync(data, offset, length, m_sendDestination);

            /// <summary>
            /// Initializes the client.
            /// </summary>
            /// <remarks>
            /// <see cref="Initialize()"/> is to be called by user-code directly only if the client is not consumed through the designer surface of the IDE.
            /// </remarks>
            public void Initialize() { }

            /// <summary>
            /// Releases the unmanaged resources used by the client and optionally releases the managed resources.
            /// </summary>
            public void Dispose()
            {
                if (IsDisposed)
                    return;

                try
                {
                    ReturnSharedClient();
                }
                finally
                {
                    IsDisposed = true;
                }
            }

            /// <summary>
            /// Gets a reference to the shared client listening
            /// on this client's local end point.
            /// </summary>
            /// <returns>A reference to a shared client.</returns>
            private void GetSharedClient()
            {
                const string ConfigurationMismatchError = "Configuration mismatch detected between parsers using shared UDP client: {0}";

                bool sharing;
                UdpClient sharedClient;

                lock (s_sharedClients)
                {
                    EndPoint localEndPoint = GetLocalEndPoint();
                    sharing = s_sharedClients.TryGetValue(localEndPoint, out sharedClient);

                    if (sharing)
                    {
                        // Validate settings to ensure that they match
                        if (sharedClient.ReceiveBufferSize != ReceiveBufferSize)
                            throw new InvalidOperationException(string.Format(ConfigurationMismatchError, "Receive buffer size"));

                        if (sharedClient.MaxConnectionAttempts != MaxConnectionAttempts)
                            throw new InvalidOperationException(string.Format(ConfigurationMismatchError, "Max connection attempts"));
                    }
                    else
                    {
                        // Create new client and add it to the shared list
                        sharedClient = new UdpClient();
                        s_sharedClients.Add(localEndPoint, sharedClient);
                        s_sharedReferenceCount.Add(localEndPoint, 0);
                    }

                    // Set the UDP client member variable ASAP to guarantee
                    // that it is set before callbacks can be triggered
                    m_udpClient = sharedClient;

                    // Attach to event handlers
                    sharedClient.ConnectionAttempt += SharedClient_ConnectionAttempt;
                    sharedClient.ConnectionEstablished += SharedClient_ConnectionEstablished;
                    sharedClient.ConnectionException += SharedClient_ConnectionException;
                    sharedClient.ConnectionTerminated += SharedClient_ConnectionTerminated;
                    sharedClient.ReceiveDataException += SharedClient_ReceiveDataException;
                    sharedClient.ReceiveDataFrom += SharedClient_ReceiveDataFrom;
                    sharedClient.SendDataException += SharedClient_SendDataException;
                    sharedClient.UnhandledUserException += SharedClient_UnhandledUserException;

                    // Make sure to set packet info flag if needed since
                    // other SharedUdpClientReferences may not need it
                    if (m_receivePacketInfo)
                        sharedClient.ReceivePacketInfo = true;

                    if (!sharing)
                    {
                        // Initialize settings and connect
                        sharedClient.ConnectionString = ConnectionString;
                        sharedClient.ReceiveBufferSize = ReceiveBufferSize;
                        sharedClient.MaxConnectionAttempts = MaxConnectionAttempts;
                        sharedClient.ConnectAsync();
                    }

                    // Increment reference count
                    s_sharedReferenceCount[localEndPoint]++;
                }

                if (sharing && sharedClient.CurrentState == ClientState.Connected)
                    OnConnectionEstablished();
            }

            /// <summary>
            /// Releases a reference to this client's shared client,
            /// and disposes of the shared client if nobody is using it.
            /// </summary>
            private void ReturnSharedClient()
            {
                lock (s_sharedClients)
                {
                    if (m_udpClient is null)
                        return;

                    // Detach from event handlers
                    m_udpClient.ConnectionAttempt -= SharedClient_ConnectionAttempt;
                    m_udpClient.ConnectionEstablished -= SharedClient_ConnectionEstablished;
                    m_udpClient.ConnectionException -= SharedClient_ConnectionException;
                    m_udpClient.ConnectionTerminated -= SharedClient_ConnectionTerminated;
                    m_udpClient.ReceiveDataException -= SharedClient_ReceiveDataException;
                    m_udpClient.ReceiveDataFrom -= SharedClient_ReceiveDataFrom;
                    m_udpClient.SendDataException -= SharedClient_SendDataException;

                    // Decrement reference count
                    EndPoint localEndPoint = GetLocalEndPoint();
                    s_sharedReferenceCount[localEndPoint]--;

                    if (s_sharedReferenceCount[localEndPoint] == 0)
                    {
                        // No more references to UDP client
                        // exist so dispose of it
                        m_udpClient.Disconnect();
                        m_udpClient.Dispose();
                        s_sharedClients.Remove(localEndPoint);
                        s_sharedReferenceCount.Remove(localEndPoint);
                    }

                    // Release reference to UDP client
                    m_udpClient = null;
                }
            }

            /// <summary>
            /// Terminates the client as quickly as possible and
            /// removes it from the collection of shared clients.
            /// </summary>
            private void TerminateSharedClient()
            {
                lock (s_sharedClients)
                {
                    EndPoint localEndPoint = GetLocalEndPoint();

                    if (s_sharedClients.TryGetValue(localEndPoint, out UdpClient sharedClient))
                    {
                        // If the wrapped client and the shared client are the same,
                        // no one has terminated the shared client yet
                        if (m_udpClient == sharedClient)
                        {
                            // Disconnect and dispose of the old client
                            m_udpClient.Disconnect();
                            m_udpClient.Dispose();

                            // Remove the old client from the
                            // collection of shared clients
                            localEndPoint = GetLocalEndPoint();
                            s_sharedClients.Remove(localEndPoint);
                            s_sharedReferenceCount.Remove(localEndPoint);
                        }
                    }

                    m_udpClient = null;
                }
            }

            /// <summary>
            /// Determines the local end point this client intends
            /// to listen on via connection string properties.
            /// </summary>
            /// <returns>The local end point.</returns>
            private EndPoint GetLocalEndPoint()
            {
                Dictionary<string, string> settings = ConnectionString.ParseKeyValuePairs();
                IPStack ipStack = Transport.GetInterfaceIPStack(settings);

                if (!settings.TryGetValue("interface", out string localInterface))
                    localInterface = string.Empty;

                if (!settings.TryGetValue("localport", out string localPortSetting) && !settings.TryGetValue("port", out localPortSetting))
                    throw new InvalidOperationException($"Local port property missing from connection string: {ConnectionString}");

                if (!int.TryParse(localPortSetting, out int localPort))
                    throw new InvalidOperationException($"Unable to parse local port from \"{localPortSetting}\".");

                return Transport.CreateEndPoint(localInterface, localPort, ipStack);
            }

            // Triggers the ConnectionEstablished event.
            private void OnConnectionEstablished()
            {
                if (!(m_multicastServerAddress is null))
                    m_udpClient.AddMulticastMembership(m_multicastServerAddress, m_multicastSourceAddress);

                ConnectionEstablished?.Invoke(this, new EventArgs());
            }

            // Shared client connection attempt handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionAttempt(object sender, EventArgs e) => ConnectionAttempt?.Invoke(this, e);

            // Shared client connection established handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionEstablished(object sender, EventArgs e) => OnConnectionEstablished();

            // Shared client connection exception handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                ConnectionException?.Invoke(this, e);
            }

            // Shared client connection terminated handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionTerminated(object sender, EventArgs e) => ConnectionTerminated?.Invoke(this, e);

            // Shared client receive data exception handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ReceiveDataException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                ReceiveDataException?.Invoke(this, e);
            }

            // Shared client receive data from handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ReceiveDataFrom(object sender, EventArgs<EndPoint, IPPacketInformation, int> e) => ReceiveDataFrom?.Invoke(this, e);

            // Shared client send data handler.
            // Forwards event to users attached to this client.
            private void SharedClient_SendDataException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                SendDataException?.Invoke(this, e);
            }

            // Shared client unhandled user exception handler.
            // Forwards event to users attached to this client.
            private void SharedClient_UnhandledUserException(object sender, EventArgs<Exception> e) => UnhandledUserException?.Invoke(this, e);

            #endregion

            #region [ Static ]

            // Static Fields
            private static readonly Dictionary<EndPoint, UdpClient> s_sharedClients = new Dictionary<EndPoint, UdpClient>();
            private static readonly Dictionary<EndPoint, int> s_sharedReferenceCount = new Dictionary<EndPoint, int>();

            #endregion
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="BufferSize"/> property.
        /// </summary>
        public const int DefaultBufferSize = 262144; // 256K

        /// <summary>
        /// Specifies the default value for the <see cref="DefinedFrameRate"/> property.
        /// </summary>
        public const int DefaultDefinedFrameRate = 30;

        /// <summary>
        /// Specifies the default value for the <see cref="MaximumConnectionAttempts"/> property.
        /// </summary>
        public const int DefaultMaximumConnectionAttempts = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoStartDataParsingSequence"/> property.
        /// </summary>
        public const bool DefaultAutoStartDataParsingSequence = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SkipDisableRealTimeData"/> property.
        /// </summary>
        public const bool DefaultSkipDisableRealTimeData = false;

        /// <summary>
        /// Specifies the default value for the <see cref="DisableRealTimeDataOnStop"/> property.
        /// </summary>
        public const bool DefaultDisableRealTimeDataOnStop = true;

        /// <summary>
        /// Specifies the default value for the <see cref="AllowedParsingExceptions"/> property.
        /// </summary>
        public const int DefaultAllowedParsingExceptions = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="ParsingExceptionWindow"/> property.
        /// </summary>
        public const long DefaultParsingExceptionWindow = 50000000L; // 5 seconds

        /// <summary>
        /// Specifies the default value for the <see cref="TrustHeaderLength"/> property.
        /// </summary>
        public const bool DefaultTrustHeaderLength = true;

        /// <summary>
        /// Specifies the default value for the <see cref="KeepCommandChannelOpen"/> property.
        /// </summary>
        public const bool DefaultKeepCommandChannelOpen = true;

        // Events

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ICommandFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> ReceivedCommandFrame;

        /// <summary>
        /// Occurs when a <see cref="IConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a <see cref="IDataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IDataFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IDataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when a <see cref="IHeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IHeaderFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IHeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an undetermined <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the undetermined <see cref="IChannelFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IChannelFrame>> ReceivedUndeterminedFrame;

        /// <summary>
        /// Occurs when a frame image has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the length of the frame image that was received.
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, int>> ReceivedFrameImage;

        /// <summary>
        /// Occurs when a frame buffer image has been received.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument2"/> is the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument3"/> is the offset into the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument4"/> is the length of data in the buffer that contains the frame image that was received.
        /// </para>
        /// <para>
        /// Consumers should use the more efficient <see cref="ReceivedFrameImage"/> event if the buffer is not needed.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, byte[], int, int>> ReceivedFrameBufferImage;

        /// <summary>
        /// Occurs when a device sends a notification that its configuration has changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while parsing the data stream.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing the data stream.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Occurs when buffer parsing has completed.
        /// </summary>
        public event EventHandler BufferParsed;

        /// <summary>
        /// Occurs when number of parsing exceptions exceed <see cref="AllowedParsingExceptions"/> during <see cref="ParsingExceptionWindow"/>.
        /// </summary>
        public event EventHandler ExceededParsingExceptionThreshold;

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> is sent to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a reference to the <see cref="ICommandFrame"/> that was sent to the device.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> SentCommandFrame;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the exception that occurred during the connection attempt.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of connections attempted so far.
        /// </remarks>
        public event EventHandler<EventArgs<Exception, int>> ConnectionException;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> is attempting connection to a device.
        /// </summary>
        public event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> has established a connection to a device.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when device connection has been terminated.
        /// </summary>
        public event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been started.
        /// </summary>
        public event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been stopped.
        /// </summary>
        public event EventHandler ServerStopped;

        /// <summary>
        /// Occurs when the <see cref="IClient.ServerIndex"/> of the associated connection is updated.
        /// </summary>
        public event EventHandler ServerIndexUpdated;

        // Fields
        private PhasorProtocol m_phasorProtocol;
        private TransportProtocol m_transportProtocol;
        private string m_connectionString;
        private int m_maximumConnectionAttempts;
        private int m_bufferSize;
        private IFrameParser m_frameParser;
        private IClient m_dataChannel;
        private IServer m_serverBasedDataChannel;
        private IClient m_commandChannel;
        private IPAddress m_receiveFromAddress;
        private IPAddress m_multicastServerAddress;
        private PrecisionInputTimer m_inputTimer;
        private ShortSynchronizedOperation m_readNextBuffer;
        private SharedTimer m_rateCalcTimer;
        private IConfigurationFrame m_configurationFrame;
        private CheckSumValidationFrameTypes m_checkSumValidationFrameTypes;
        private long m_dataStreamStartTime;
        private long m_missingFramesOverflow;
        private long m_lastFrameReceivedTime;
        private int m_frameRateTotal;
        private int m_byteRateTotal;
        private int m_parsingExceptionCount;
        private long m_lastParsingExceptionTime;
        private int m_definedFrameRate;
        private bool m_initiatingDataStream;
        private long m_initialBytesReceived;
        private bool m_initiatingSerialConnection;
        private IConnectionParameters m_connectionParameters;
        private int m_connectionAttempts;
        private int m_serverIndex;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MultiProtocolFrameParser"/> using the default settings.
        /// </summary>
        public MultiProtocolFrameParser()
        {
            m_connectionString = "server=127.0.0.1:4712";
            m_bufferSize = DefaultBufferSize;
            m_maximumConnectionAttempts = DefaultMaximumConnectionAttempts;
            m_phasorProtocol = PhasorProtocol.IEEEC37_118V1;
            m_transportProtocol = TransportProtocol.Tcp;
            m_checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames;

            // Set default frame rate, this calculates milliseconds for each frame
            DefinedFrameRate = DefaultDefinedFrameRate;

            m_rateCalcTimer = TimerScheduler.CreateTimer();
            m_rateCalcTimer.Elapsed += m_rateCalcTimer_Elapsed;
            m_rateCalcTimer.Interval = 5000;
            m_rateCalcTimer.AutoReset = true;
            m_rateCalcTimer.Enabled = false;

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="MultiProtocolFrameParser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~MultiProtocolFrameParser() => Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.PhasorProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public PhasorProtocol PhasorProtocol
        {
            get => m_phasorProtocol;
            set
            {
                m_phasorProtocol = value;
                DeviceSupportsCommands = DeriveCommandSupport();

                // Setup protocol specific connection parameters, for those protocols that have them...
                switch (value)
                {
                    case PhasorProtocol.BPAPDCstream:
                        m_connectionParameters = new BPAPDCstream.ConnectionParameters();
                        break;
                    case PhasorProtocol.FNET:
                        m_connectionParameters = new FNET.ConnectionParameters();
                        break;
                    case PhasorProtocol.SelFastMessage:
                        m_connectionParameters = new SelFastMessage.ConnectionParameters();
                        break;
                    case PhasorProtocol.IEC61850_90_5:
                        m_connectionParameters = new IEC61850_90_5.ConnectionParameters();
                        break;
                    case PhasorProtocol.Macrodyne:
                        m_connectionParameters = new Macrodyne.ConnectionParameters();
                        break;
                    default:
                        m_connectionParameters = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="TransportProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public TransportProtocol TransportProtocol
        {
            get => m_transportProtocol;
            set
            {
                m_transportProtocol = value;
                DeviceSupportsCommands = DeriveCommandSupport();

                // File based input connections are handled more carefully
                if (m_transportProtocol != TransportProtocol.File)
                    return;

                if (m_maximumConnectionAttempts < 1)
                    m_maximumConnectionAttempts = 1;
            }
        }

        /// <summary>
        /// Gets or sets the key/value pair based connection information required by the <see cref="MultiProtocolFrameParser"/> to connect to a device.
        /// </summary>
        public string ConnectionString
        {
            get => m_connectionString;
            set
            {
                m_connectionString = value;

                // Parse connection string to see if a phasor or transport protocol was assigned
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                if (settings.TryGetValue("phasorProtocol", out string setting))
                    PhasorProtocol = (PhasorProtocol)Enum.Parse(typeof(PhasorProtocol), setting, true);

                if (settings.TryGetValue("transportProtocol", out setting) || settings.TryGetValue("protocol", out setting))
                    TransportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), setting, true);

                if (settings.TryGetValue("keepCommandChannelOpen", out setting))
                    KeepCommandChannelOpen = setting.ParseBoolean();

                if (settings.TryGetValue("bufferSize", out setting) && int.TryParse(setting, out int bufferSize) && bufferSize >= 1)
                    BufferSize = bufferSize;

                if (settings.TryGetValue("checkSumValidationFrameTypes", out setting))
                {
                    if (!Enum.TryParse(setting, true, out m_checkSumValidationFrameTypes))
                        m_checkSumValidationFrameTypes = CheckSumValidationFrameTypes.AllFrames;
                }

                if (settings.TryGetValue("trustHeaderLength", out setting))
                    TrustHeaderLength = setting.ParseBoolean();

                DeviceSupportsCommands = DeriveCommandSupport();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether to keep the command channel open after the initial startup sequence.
        /// Defaults to <see cref="DefaultKeepCommandChannelOpen"/>.
        /// </summary>
        public bool KeepCommandChannelOpen { get; set; } = DefaultKeepCommandChannelOpen;

        /// <summary>
        /// Gets or sets flag that determines if a device supports commands.
        /// </summary>
        /// <remarks>
        /// This property is automatically derived based on the selected <see cref="PhasorProtocol"/>, <see cref="TransportProtocol"/>
        /// and <see cref="ConnectionString"/>, but can be overridden if the consumer already knows that a device supports commands.
        /// </remarks>
        public bool DeviceSupportsCommands { get; set; }

        /// <summary>
        /// Gets or sets the device identification code often needed to establish a connection.
        /// </summary>
        /// <remarks>
        /// Many devices validate this ID when sending commands, so it may need to be correct in order to start parsing sequence.
        /// </remarks>
        public ushort DeviceID { get; set; } = 1;

        /// <summary>
        /// Gets or sets the size of the buffer used by the <see cref="MultiProtocolFrameParser"/> for sending and receiving data from a device.
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is either zero or negative.</exception>
        public int BufferSize
        {
            get => m_bufferSize;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative.");

                m_bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times the <see cref="MultiProtocolFrameParser"/> will attempt to connect to a device.
        /// Defaults to <see cref="DefaultMaximumConnectionAttempts"/>.
        /// </summary>
        /// <remarks>Set to -1 for infinite connection attempts.</remarks>
        public int MaximumConnectionAttempts
        {
            get => m_maximumConnectionAttempts;
            set
            {
                m_maximumConnectionAttempts = value;

                // All values below zero are assumed to mean infinite connection attempts
                if (m_maximumConnectionAttempts < 1)
                    m_maximumConnectionAttempts = -1;

                // We don't allow maximum connection attempts set to infinite if using file based source since file based
                // connection errors are like "file not found", "invalid path", etc. These connection exceptions are returned
                // so quickly that they will queue up much faster than they will be reported.
                if (m_maximumConnectionAttempts < 1 && m_transportProtocol == TransportProtocol.File)
                    m_maximumConnectionAttempts = 1;
            }
        }

        /// <summary>
        /// Gets or sets flag to automatically send the ConfigFrame2 and EnableRealTimeData command frames used to start a typical data parsing sequence.
        /// Defaults to <see cref="DefaultAutoStartDataParsingSequence"/>.
        /// </summary>
        /// <remarks>
        /// For devices that support IEEE commands, setting this property to true will automatically start the data parsing sequence.
        /// </remarks>
        public bool AutoStartDataParsingSequence { get; set; } = DefaultAutoStartDataParsingSequence;

        /// <summary>
        /// Gets or sets flag to skip automatic disabling of the real-time data stream on shutdown or startup.
        /// Defaults to <see cref="DefaultSkipDisableRealTimeData"/>.
        /// </summary>
        /// <remarks>
        /// This flag may important when using UDP multicast with several subscribed clients.
        /// </remarks>
        public bool SkipDisableRealTimeData { get; set; } = DefaultSkipDisableRealTimeData;

        /// <summary>
        /// Gets or sets flag to disable real-time data on stop.
        /// Defaults to <see cref="DefaultDisableRealTimeDataOnStop"/>.
        /// </summary>
        /// <remarks>
        /// If <c>false</c>, disable real-time command will not be sent when parser is stopped regardless
        /// of <see cref="SkipDisableRealTimeData"/> value.
        /// </remarks>
        public bool DisableRealTimeDataOnStop { get; set; } = DefaultDisableRealTimeDataOnStop;

        /// <summary>
        /// Gets or sets number of parsing exceptions allowed during <see cref="ParsingExceptionWindow"/> before connection is reset.
        /// Defaults to <see cref="DefaultAllowedParsingExceptions"/>.
        /// </summary>
        public int AllowedParsingExceptions { get; set; } = DefaultAllowedParsingExceptions;

        /// <summary>
        /// Gets or sets time duration, in <see cref="Ticks"/>, to monitor parsing exceptions.
        /// Defaults to <see cref="DefaultParsingExceptionWindow"/>.
        /// </summary>
        public Ticks ParsingExceptionWindow { get; set; } = DefaultParsingExceptionWindow;

        /// <summary>
        /// Gets or sets a descriptive name for a device connection.
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if a high-resolution precision timer should be used for file based input.
        /// </summary>
        /// <remarks>
        /// Useful when input frames need be accurately time-aligned to the local clock to better simulate
        /// an input device and calculate downstream latencies.<br/>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public bool UseHighResolutionInputTimer
        {
            get => !(m_inputTimer is null);
            set
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                if (value && m_inputTimer is null && !Debugger.IsAttached)
                    m_inputTimer = PrecisionInputTimer.Attach(m_definedFrameRate, OnParsingException);
                else if (!value && !(m_inputTimer is null))
                    PrecisionInputTimer.Detach(ref m_inputTimer);
            }
        }

        /// <summary>
        /// Gets or sets desired frame rate to use for maintaining captured frame replay timing.
        /// Defaults to <see cref="DefaultDefinedFrameRate"/>.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public int DefinedFrameRate
        {
            get => m_definedFrameRate;
            set
            {
                if (m_definedFrameRate != value)
                {
                    bool timerActive = UseHighResolutionInputTimer;

                    // Deactivate timer before changing defined frame rate
                    if (timerActive)
                        UseHighResolutionInputTimer = false;

                    m_definedFrameRate = value;

                    // Reactivate timer if it was active
                    if (timerActive)
                        UseHighResolutionInputTimer = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the replay start time to use for file based input. File read will begin when encountered
        /// frame timestamps are greater than or equal to specified <see cref="ReplayStartTime"/>.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public DateTime ReplayStartTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the replay stop time to use for file based input. File read will end when encountered
        /// frame timestamps are greater than or equal to specified <see cref="ReplayStopTime"/>.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public DateTime ReplayStopTime { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// Gets or sets flag indicating whether or not to inject local system time into parsed data frames.
        /// </summary>
        /// <remarks>
        /// When connection is made to a file for replay purposes or consumer doesn't trust remote clock source, this flag
        /// can be set to true replace all frame timestamps with a UTC timestamp derived from the local system clock.
        /// </remarks>
        public bool InjectSimulatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a flag that determines if a file used for replaying data should be restarted at the beginning once it has been completed.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public bool AutoRepeatCapturedPlayback { get; set; }

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public IConfigurationFrame ConfigurationFrame
        {
            get => m_configurationFrame;
            set
            {
                m_configurationFrame = value;

                // Pass new config frame onto appropriate parser, casting into appropriate protocol if needed...
                if (!(m_frameParser is null))
                    m_frameParser.ConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets or sets flags that determine if check-sums for specified frames should be validated.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be set to <see cref="GSF.PhasorProtocols.CheckSumValidationFrameTypes.AllFrames"/>.
        /// </remarks>
        public CheckSumValidationFrameTypes CheckSumValidationFrameTypes
        {
            get => m_checkSumValidationFrameTypes;
            set => m_checkSumValidationFrameTypes = value;
        }

        /// <summary>
        /// Gets or sets flag that determines if header lengths should be trusted over parsed byte count.
        /// Defaults to <see cref="DefaultTrustHeaderLength"/>.
        /// </summary>
        /// <remarks>
        /// It is expected that this will normally be left as <c>true</c>.
        /// </remarks>
        public bool TrustHeaderLength { get; set; } = DefaultTrustHeaderLength;

        /// <summary>
        /// Gets the number of redundant frames in each packet.
        /// </summary>
        /// <remarks>
        /// This value is used when calculating statistics. It is assumed that for each
        /// frame that is received, that frame will be included in the next <c>n</c>
        /// packets, where <c>n</c> is the number of redundant frames per packet.
        /// </remarks>
        public int RedundantFramesPerPacket => m_frameParser?.RedundantFramesPerPacket ?? 0;

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="PhasorProtocol"/> is an IEEE standard protocol.
        /// </summary>
        public bool IsIEEEProtocol => m_phasorProtocol == PhasorProtocol.IEEEC37_118V2 ||
                                      m_phasorProtocol == PhasorProtocol.IEEEC37_118V1 ||
                                      m_phasorProtocol == PhasorProtocol.IEEEC37_118D6 ||
                                      m_phasorProtocol == PhasorProtocol.IEEE1344;

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="TransportProtocol"/> is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (!(m_commandChannel is null) && KeepCommandChannelOpen)
                    return m_commandChannel.CurrentState == ClientState.Connected;

                if (!(m_dataChannel is null))
                    return m_dataChannel.CurrentState == ClientState.Connected;

                if (!(m_serverBasedDataChannel is null))
                    return m_serverBasedDataChannel.ClientIDs.Length > 0;

                return false;
            }
        }

        /// <summary>
        /// Gets the current server index, when multiple server end points are defined.
        /// </summary>
        public int ServerIndex
        {
            get => m_serverIndex;
            private set
            {
                if (m_serverIndex == value)
                    return;

                m_serverIndex = value;
                ServerIndexUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets a string representing connectivity information.
        /// </summary>
        public string ConnectionInfo
        {
            get
            {
                if (!(m_serverBasedDataChannel is null))
                    return $"{m_serverBasedDataChannel.TransportProtocol} Server";

                string commandChannelServerUri = CommandChannelServerUri;
                string dataChannelServerUri = DataChannelServerUri;

                if (string.IsNullOrWhiteSpace(commandChannelServerUri) && string.IsNullOrWhiteSpace(dataChannelServerUri))
                    return null;

                if (string.IsNullOrWhiteSpace(dataChannelServerUri))
                    return commandChannelServerUri;

                if (string.IsNullOrWhiteSpace(commandChannelServerUri))
                    return dataChannelServerUri;

                return $"{commandChannelServerUri} / {dataChannelServerUri}";
            }
        }

        /// <summary>
        /// Gets the server URI of the command channel connection, or <c>null</c> if not connected.
        /// </summary>
        public string CommandChannelServerUri => m_commandChannel?.ServerUri;

        /// <summary>
        /// Gets the server URI of the data channel connection, or <c>null</c> if not connected.
        /// </summary>
        public string DataChannelServerUri => m_dataChannel?.ServerUri;

        /// <summary>
        /// Gets flag that determines if the connection type is multicast.
        /// </summary>
        public bool ConnectionIsMulticast
        {
            get
            {
                // Multicast will only be for UDP style connections
                if (m_transportProtocol != TransportProtocol.Udp)
                    return false;

                if (string.IsNullOrWhiteSpace(m_connectionString))
                    return false;

                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                if (!settings.TryGetValue("server", out string server))
                    return false;

                if (!settings.ContainsKey("remotePort"))
                {
                    Match endPoint = Regex.Match(server, Transport.EndpointFormatRegex);
                    server = endPoint.Groups["host"].Value;
                }

                return IPAddress.TryParse(server, out IPAddress serverAddress) && Transport.IsMulticastIP(serverAddress);
            }
        }

        /// <summary>
        /// Gets flag that determines if the connection type is a TCP server.
        /// </summary>
        public bool ConnectionIsListener
        {
            get
            {
                Dictionary<string, string> settings;

                // Listener setting is only valid for TCP data channels
                if (m_transportProtocol != TransportProtocol.Tcp)
                    return false;

                if (string.IsNullOrWhiteSpace(m_connectionString))
                    return false;

                settings = m_connectionString.ParseKeyValuePairs();

                return settings.TryGetValue("isListener", out string setting) && setting.ParseBoolean();
            }
        }

        /// <summary>
        /// Gets total time connection has been active.
        /// </summary>
        public Time ConnectionTime
        {
            get
            {
                if (!(m_commandChannel is null))
                    return m_commandChannel.ConnectionTime;

                if (!(m_dataChannel is null))
                    return m_dataChannel.ConnectionTime;

                return m_serverBasedDataChannel?.RunTime ?? 0.0D;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MultiProtocolFrameParser"/> is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the <see cref="MultiProtocolFrameParser"/> if it is not started,
        /// setting to false will stop the <see cref="MultiProtocolFrameParser"/> if it is started.
        /// </remarks>
        public bool Enabled
        {
            get => m_enabled;
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public int QueuedBuffers => m_frameParser?.QueuedBuffers ?? 0;

        /// <summary>
        /// Gets the total number of frames that are currently queued for publication, if any.
        /// </summary>
        public int QueuedOutputs => m_frameParser?.QueuedOutputs ?? 0;

        /// <summary>
        /// Gets a boolean value that determines if data channel is defined as a server based connection.
        /// </summary>
        public bool DataChannelIsServerBased
        {
            get
            {
                if (!(m_dataChannel is null))
                    return false;

                if (!(m_serverBasedDataChannel is null))
                    return true;

                if (string.IsNullOrWhiteSpace(m_connectionString))
                    return false;

                // No connection is currently active, see if connection string defines a server based connection
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                return settings.TryGetValue("isListener", out string setting) && setting.ParseBoolean();
            }
        }

        /// <summary>
        /// Gets total number of frames that have been received from a device so far.
        /// </summary>
        public long TotalFramesReceived { get; private set; }

        /// <summary>
        /// Gets total number of bytes that have been received from a device so far.
        /// </summary>
        public long TotalBytesReceived { get; private set; }

        /// <summary>
        /// Gets total number of frames that were missing from device so far.
        /// </summary>
        public long TotalMissingFrames { get; private set; }

        /// <summary>
        /// Gets total number of CRC exceptions encountered from device so far.
        /// </summary>
        public long TotalCrcExceptions { get; private set; }

        /// <summary>
        /// Gets the configured frame rate as reported by the connected device.
        /// </summary>
        public int ConfiguredFrameRate { get; private set; }

        /// <summary>
        /// Gets the calculated frame rate (i.e., frames per second) based on data received from device connection.
        /// </summary>
        public double CalculatedFrameRate { get; private set; }

        /// <summary>
        /// Gets the calculated byte rate (i.e., bytes per second) based on data received from device connection.
        /// </summary>
        public double ByteRate { get; private set; }

        /// <summary>
        /// Gets the calculated bit rate (i.e., bits per second (bps)) based on data received from device connection.
        /// </summary>
        public double BitRate => ByteRate * 8.0D;

        /// <summary>
        /// Gets the calculated megabits per second (Mbps) rate based on data received from device connection.
        /// </summary>
        public double MegaBitRate => BitRate / SI2.Mega;

        /// <summary>
        /// Gets a descriptive name for a device connection that includes <see cref="SourceName"/>, if provided.
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceName))
                    return $"ID {DeviceID} using {m_phasorProtocol.GetFormattedProtocolName()} over {m_transportProtocol}";

                return $"{SourceName} ({DeviceID})";
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine($"      Device Connection ID: {DeviceID}");
                status.AppendLine($"           Phasor protocol: {m_phasorProtocol.GetFormattedProtocolName()}");
                status.AppendLine($"           Connection type: {ConnectionType}{(ConnectionIsMulticast ? " - Multicast" : "")}");
                status.AppendLine($"         Source Connection: {ConnectionInfo.TruncateRight(51)}");
                status.AppendLine($"               Buffer size: {m_bufferSize:N0}");
                status.AppendLine($"     Total frames received: {TotalFramesReceived:N0}");
                status.AppendLine($"      Total missing frames: {TotalMissingFrames:N0}");
                status.AppendLine($"      Total CRC exceptions: {TotalCrcExceptions:N0}");
                status.AppendLine($"     Calculated frame rate: {CalculatedFrameRate:0.00} frames/sec");
                status.AppendLine($"      Calculated data rate: {ByteRate:0.0} bytes/sec, {MegaBitRate:0.0000} Mbps");
                status.AppendLine($"Allowed parsing exceptions: {AllowedParsingExceptions:N0}");
                status.AppendLine($"  Parsing exception window: {ParsingExceptionWindow.ToSeconds():0.00} seconds");
                status.AppendLine($"Using simulated timestamps: {(InjectSimulatedTimestamp ? "Yes" : "No")}");

                if (m_transportProtocol == TransportProtocol.File)
                {
                    status.AppendLine($"  Defined input frame rate: {m_definedFrameRate:N0} frames/sec");
                    status.AppendLine($"     Precision input timer: {(UseHighResolutionInputTimer ? "Enabled" : "Offline")}");

                    if (!(m_inputTimer is null))
                        status.AppendLine($"  Timer resynchronizations: {m_inputTimer.Resynchronizations:N0}");

                    if (ReplayStartTime > DateTime.MinValue || ReplayStopTime < DateTime.MaxValue)
                    {
                        status.AppendLine($"         Replay start time: {ReplayStartTime:yyyy-MM-dd HH:mm:ss.fff}");
                        status.AppendLine($"          Replay stop time: {ReplayStopTime:yyyy-MM-dd HH:mm:ss.fff}");
                    }
                }

                if (!(m_frameParser is null))
                    status.Append(m_frameParser.Status);

                if (!(m_dataChannel is null))
                    status.Append(m_dataChannel.Status);

                if (!(m_serverBasedDataChannel is null))
                    status.Append(m_serverBasedDataChannel.Status);

                if (!(m_commandChannel is null))
                    status.Append(m_commandChannel.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the connection type (Active, Passive or Hybrid) based on defined channels and transport selections.
        /// </summary>
        public string ConnectionType
        {
            get
            {
                switch (m_transportProtocol)
                {
                    case TransportProtocol.Tcp:
                    case TransportProtocol.Serial:
                        return "Active";
                    case TransportProtocol.Udp:
                    case TransportProtocol.File:
                        return m_commandChannel is null ? "Passive" : "Hybrid";
                    default:
                        return "Undetermined";
                }
            }
        }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be applicable for the current <see cref="PhasorProtocol"/>.
        /// </summary>
        public IConnectionParameters ConnectionParameters
        {
            get => m_connectionParameters;
            set
            {
                m_connectionParameters = value;

                // Pass new connection parameters along to derived frame parser if instantiated
                if (!(m_frameParser is null))
                    m_frameParser.ConnectionParameters = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="MultiProtocolFrameParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultiProtocolFrameParser"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                Stop();

                PrecisionInputTimer.Detach(ref m_inputTimer);

                if (!(m_rateCalcTimer is null))
                {
                    m_rateCalcTimer.Elapsed -= m_rateCalcTimer_Elapsed;
                    m_rateCalcTimer.Dispose();
                }

                m_rateCalcTimer = null;

                // Clear minimum timer resolution.
                PrecisionTimer.ClearMinimumTimerResolution(1);
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Starts the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public void Start()
        {
            IConfigurationFrame configurationFrame = m_configurationFrame;

            // Stop parser if it is already running - thus calling start after already started will have the effect
            // of "restarting" the parsing engine...
            Stop();

            // Reset statistics...
            TotalFramesReceived = 0L;
            TotalMissingFrames = 0L;
            TotalCrcExceptions = 0L;
            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            TotalBytesReceived = 0L;
            CalculatedFrameRate = 0.0D;
            ByteRate = 0.0D;
            m_lastParsingExceptionTime = 0L;
            m_parsingExceptionCount = 0;

            try
            {
                // Parse connection string to check for special parameters
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                // Reset connection attempt counter
                m_connectionAttempts = 0;

                // Validate that the high-precision input timer is necessary
                if (m_transportProtocol != TransportProtocol.File && UseHighResolutionInputTimer)
                    UseHighResolutionInputTimer = false;

                // Establish protocol specific frame parser
                InitializeFrameParser(settings);

                if (!(configurationFrame is null))
                    ConfigurationFrame = configurationFrame;

                if (settings.TryGetValue("commandChannel", out string setting))
                {
                    // Establish command channel connection, if defined...
                    InitializeCommandChannel(setting);
                }
                else
                {
                    // Establish data channel connection - must be defined.
                    InitializeDataChannel(settings);
                }

                m_rateCalcTimer.Enabled = true;
                m_enabled = true;
            }
            catch (SocketException ex)
            {
                Stop();

                // Check for common error when using an IPv4 address on an IPv6 stack
                if (ex.ErrorCode == 10014)
                    OnConnectionException(new InvalidOperationException($"Bad IP address format in \"{m_connectionString}\": {ex.Message}\r\n\r\nUse a DNS name or an IPv6 formatted IP address (e.g., ::1); otherwise, force IPv4 mode.", ex), 1);
                else
                    OnConnectionException(new InvalidOperationException($"{ex.Message} in \"{m_connectionString}\"", ex), 1);
            }
            catch (Exception ex)
            {
                Stop();
                OnConnectionException(new InvalidOperationException($"{ex.Message} in \"{m_connectionString}\"", ex), 1);
            }
        }

        /// <summary>
        /// Attempts to initialize the protocol specific frame parser.
        /// </summary>
        /// <returns><c>true</c> if frame parser was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Starting the multi-protocol frame parser will automatically initialize the frame parsers so calling
        /// this method then will be unnecessary, however, if you are using this class just to edit custom
        /// connection parameters then initializing the will be necessary.
        /// </remarks>
        public bool TryInitializeFrameParser()
        {
            bool success = true;

            try
            {
                if (!(m_frameParser is null))
                    Stop();

                InitializeFrameParser(m_connectionString.ParseKeyValuePairs());
            }
            catch
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Initialize frame parser.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        private void InitializeFrameParser(Dictionary<string, string> settings)
        {
            string setting;

            // Instantiate protocol specific frame parser
            switch (m_phasorProtocol)
            {
                case PhasorProtocol.IEEEC37_118V2:
                    m_frameParser = new IEEEC37_118.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength, DraftRevision.Std2011);
                    break;
                case PhasorProtocol.IEEEC37_118V1:
                case PhasorProtocol.IEEEC37_118D6:
                    m_frameParser = new IEEEC37_118.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength, m_phasorProtocol == PhasorProtocol.IEEEC37_118D6 ? DraftRevision.Draft6 : DraftRevision.Std2005);
                    break;
                case PhasorProtocol.IEEE1344:
                    m_frameParser = new IEEE1344.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);
                    break;
                case PhasorProtocol.IEC61850_90_5:
                    m_frameParser = new IEC61850_90_5.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);

                    // Check for IEC 61850-90-5 protocol specific parameters in connection string
                    if (m_connectionParameters is IEC61850_90_5.ConnectionParameters iecParameters)
                    {
                        if (settings.TryGetValue("useETRConfiguration", out setting))
                            iecParameters.UseETRConfiguration = setting.ParseBoolean();

                        if (settings.TryGetValue("guessConfiguration", out setting))
                            iecParameters.GuessConfiguration = setting.ParseBoolean();

                        if (settings.TryGetValue("parseRedundantASDUs", out setting))
                            iecParameters.ParseRedundantASDUs = setting.ParseBoolean();

                        if (settings.TryGetValue("ignoreSignatureValidationFailures", out setting))
                            iecParameters.IgnoreSignatureValidationFailures = setting.ParseBoolean();

                        if (settings.TryGetValue("ignoreSampleSizeValidationFailures", out setting))
                            iecParameters.IgnoreSampleSizeValidationFailures = setting.ParseBoolean();
                    }

                    break;
                case PhasorProtocol.BPAPDCstream:
                    m_frameParser = new BPAPDCstream.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);

                    // Check for BPA PDCstream protocol specific parameters in connection string
                    if (m_connectionParameters is BPAPDCstream.ConnectionParameters bpaPdcParameters)
                    {
                        // INI file name setting is required
                        if (settings.TryGetValue("iniFileName", out setting))
                            bpaPdcParameters.ConfigurationFileName = FilePath.GetAbsolutePath(setting);
                        else if (string.IsNullOrWhiteSpace(bpaPdcParameters.ConfigurationFileName))
                            throw new ArgumentException("BPA PDCstream INI filename setting (e.g., \"iniFileName=DEVICE_PDC.ini\") was not found. This setting is required for BPA PDCstream protocol connections - frame parser initialization terminated.");

                        if (settings.TryGetValue("refreshConfigFileOnChange", out setting))
                            bpaPdcParameters.RefreshConfigurationFileOnChange = setting.ParseBoolean();

                        if (settings.TryGetValue("parseWordCountFromByte", out setting))
                            bpaPdcParameters.ParseWordCountFromByte = setting.ParseBoolean();

                        if (settings.TryGetValue("usePhasorDataFileFormat", out setting))
                            bpaPdcParameters.UsePhasorDataFileFormat = setting.ParseBoolean();
                    }
                    break;
                case PhasorProtocol.FNET:
                    m_frameParser = new FNET.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);

                    // Check for F-NET protocol specific parameters in connection string
                    if (m_connectionParameters is FNET.ConnectionParameters fnetParameters)
                    {
                        if (settings.TryGetValue("timeOffset", out setting))
                            fnetParameters.TimeOffset = long.Parse(setting);

                        if (settings.TryGetValue("stationName", out setting))
                            fnetParameters.StationName = setting;

                        if (settings.TryGetValue("frameRate", out setting))
                            fnetParameters.FrameRate = ushort.Parse(setting);

                        if (settings.TryGetValue("nominalFrequency", out setting))
                            fnetParameters.NominalFrequency = (LineFrequency)int.Parse(setting);
                    }
                    break;
                case PhasorProtocol.SelFastMessage:
                    m_frameParser = new SelFastMessage.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);

                    // Check for SEL Fast Message protocol specific parameters in connection string
                    if (m_connectionParameters is SelFastMessage.ConnectionParameters selParameters)
                    {
                        if (settings.TryGetValue("messagePeriod", out setting))
                            selParameters.MessagePeriod = (MessagePeriod)Enum.Parse(typeof(MessagePeriod), setting, true);
                    }
                    break;
                case PhasorProtocol.Macrodyne:
                    m_frameParser = new Macrodyne.FrameParser(m_checkSumValidationFrameTypes, TrustHeaderLength);

                    // Check for Macrodyne protocol specific parameters in connection string
                    if (m_connectionParameters is Macrodyne.ConnectionParameters macrodyneParameters)
                    {
                        if (settings.TryGetValue("protocolVersion", out setting) && Enum.TryParse(setting, true, out ProtocolVersion protocolVersion))
                            macrodyneParameters.ProtocolVersion = protocolVersion;

                        // INI file name setting is required for 1690G protocol
                        if (settings.TryGetValue("iniFileName", out setting))
                            macrodyneParameters.ConfigurationFileName = FilePath.GetAbsolutePath(setting);
                        else if (macrodyneParameters.ProtocolVersion == ProtocolVersion.G && string.IsNullOrWhiteSpace(macrodyneParameters.ConfigurationFileName))
                            throw new ArgumentException("Macrodyne INI filename setting (e.g., \"iniFileName=DEVICE_PDC.ini\") was not found. This setting is required for 1690G devices - frame parser initialization terminated.");

                        // Device label setting is required for 1690G protocol
                        if (settings.TryGetValue("deviceLabel", out setting))
                            macrodyneParameters.DeviceLabel = setting;
                        else if (macrodyneParameters.ProtocolVersion == ProtocolVersion.G && string.IsNullOrWhiteSpace(macrodyneParameters.DeviceLabel))
                            throw new ArgumentException("Macrodyne device label setting (e.g., \"deviceLabel=DEVICE1\") was not found. This setting is required for 1690G devices - frame parser initialization terminated.");

                        if (settings.TryGetValue("refreshConfigFileOnChange", out setting))
                            macrodyneParameters.RefreshConfigurationFileOnChange = setting.ParseBoolean();
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Phasor protocol \"{m_phasorProtocol}\" is not recognized, failed to initialize frame parser");
            }

            // Assign frame parser properties
            m_frameParser.ConnectionParameters = m_connectionParameters;

            // Setup event handlers
            m_frameParser.ReceivedCommandFrame += m_frameParser_ReceivedCommandFrame;
            m_frameParser.ReceivedConfigurationFrame += m_frameParser_ReceivedConfigurationFrame;
            m_frameParser.ReceivedDataFrame += m_frameParser_ReceivedDataFrame;
            m_frameParser.ReceivedHeaderFrame += m_frameParser_ReceivedHeaderFrame;
            m_frameParser.ReceivedUndeterminedFrame += m_frameParser_ReceivedUndeterminedFrame;
            m_frameParser.ReceivedFrameImage += m_frameParser_ReceivedFrameImage;
            m_frameParser.ConfigurationChanged += m_frameParser_ConfigurationChanged;
            m_frameParser.ParsingException += m_frameParser_ParsingException;
            m_frameParser.BufferParsed += m_frameParser_BufferParsed;

            // Only attach to this event if consumer needs buffer image (i.e., has attached to our event)
            if (!(ReceivedFrameBufferImage is null))
                m_frameParser.ReceivedFrameBufferImage += m_frameParser_ReceivedFrameBufferImage;

            // Start parsing engine
            m_frameParser.Start();
        }

        /// <summary>
        /// Initialize command channel.
        /// </summary>
        /// <param name="connectionString">Command channel connection string.</param>
        private void InitializeCommandChannel(string connectionString)
        {
            // Parse command channel connection settings
            TransportProtocol transportProtocol;
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            // Verify user did not attempt to setup command channel as a TCP server
            if (settings.ContainsKey("isListener") && settings["isListener"].ParseBoolean())
                throw new ArgumentException("Command channel cannot be setup as a TCP server.");

            // Determine what transport protocol user selected
            if (settings.TryGetValue("transportProtocol", out string setting) || settings.TryGetValue("protocol", out setting))
            {
                transportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), setting, true);

                // The communications engine only recognizes the transport protocol key as "protocol"
                connectionString = connectionString.ReplaceCaseInsensitive("transportProtocol", "protocol");
            }
            else
            {
                throw new ArgumentException("No transport protocol was specified for command channel. For example: \"transportProtocol=Tcp\".");
            }

            // Validate command channel transport protocol selection
            if (transportProtocol != TransportProtocol.Tcp && transportProtocol != TransportProtocol.Serial && transportProtocol != TransportProtocol.File)
                throw new ArgumentException("Command channel transport protocol can only be defined as TCP, Serial or File");

            // Instantiate command channel based on defined transport layer
            m_commandChannel = ClientBase.Create(connectionString);

            // Setup event handlers
            m_commandChannel.ConnectionEstablished += m_commandChannel_ConnectionEstablished;
            m_commandChannel.ConnectionAttempt += m_commandChannel_ConnectionAttempt;
            m_commandChannel.ConnectionException += m_commandChannel_ConnectionException;
            m_commandChannel.ConnectionTerminated += m_commandChannel_ConnectionTerminated;
            m_commandChannel.ReceiveData += m_commandChannel_ReceiveData;
            m_commandChannel.ReceiveDataException += m_commandChannel_ReceiveDataException;
            m_commandChannel.SendDataException += m_commandChannel_SendDataException;
            m_commandChannel.UnhandledUserException += m_commandChannel_UnhandledUserException;

            // Restore last server index so that next connection attempt will be for the next configured server
            if (m_commandChannel is ClientBase clientBase)
                clientBase.ServerIndex = ServerIndex;

            // Attempt connection to device over command channel
            m_commandChannel.ReceiveBufferSize = m_bufferSize;
            m_commandChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
            m_commandChannel.ConnectAsync();
        }

        /// <summary>
        /// Initialize data channel.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        private void InitializeDataChannel(Dictionary<string, string> settings)
        {
            // Instantiate selected transport layer
            switch (m_transportProtocol)
            {
                case TransportProtocol.Tcp:
                    // The TCP transport may be set up as a server or as a client, we distinguish
                    // this simply by deriving the value of an added key/value pair in the
                    // connection string called "IsListener"
                    if (settings.TryGetValue("isListener", out string setting))
                    {
                        if (setting.ParseBoolean())
                            m_serverBasedDataChannel = settings.ContainsKey("receiveFrom") ? (IServer)new SharedTcpServerReference() : new TcpServer();
                        else
                            m_dataChannel = new TcpClient();
                    }
                    else
                    {
                        // If the key doesn't exist, we assume it's a client connection
                        m_dataChannel = new TcpClient();
                    }
                    break;
                case TransportProtocol.Udp:
                    InitializeUdpDataChannel(settings);
                    break;
                case TransportProtocol.Serial:
                    m_dataChannel = new SerialClient();
                    m_initiatingSerialConnection = true;
                    break;
                case TransportProtocol.File:
                    m_dataChannel = new FileClient();

                    // For file based playback, we allow the option of auto-repeat
                    FileClient fileClient = (FileClient)m_dataChannel;
                    fileClient.FileOpenMode = FileMode.Open;
                    fileClient.FileAccessMode = FileAccess.Read;
                    fileClient.FileShareMode = FileShare.Read;
                    fileClient.ReceiveOnDemand = true;
                    fileClient.ReceiveBufferSize = ushort.MaxValue;
                    fileClient.AutoRepeat = AutoRepeatCapturedPlayback;

                    // Setup synchronized read operation for file client operations
                    m_readNextBuffer = new ShortSynchronizedOperation(ReadNextFileBuffer, ex => OnParsingException(new InvalidOperationException($"Encountered an exception while reading file data: {ex.Message}", ex)));
                    break;
                default:
                    throw new InvalidOperationException($"Transport protocol \"{m_transportProtocol}\" is not recognized, failed to initialize data channel");
            }

            // Handle primary data connection, this *must* be defined...
            if (!(m_dataChannel is null))
            {
                // Setup event handlers
                m_dataChannel.ConnectionEstablished += m_dataChannel_ConnectionEstablished;
                m_dataChannel.ConnectionAttempt += m_dataChannel_ConnectionAttempt;
                m_dataChannel.ConnectionException += m_dataChannel_ConnectionException;
                m_dataChannel.ConnectionTerminated += m_dataChannel_ConnectionTerminated;
                m_dataChannel.ReceiveData += m_dataChannel_ReceiveData;
                m_dataChannel.ReceiveDataException += m_dataChannel_ReceiveDataException;
                m_dataChannel.SendDataException += m_dataChannel_SendDataException;
                m_dataChannel.UnhandledUserException += m_dataChannel_UnhandledUserException;

                // Restore last server index so that next connection attempt will be for the next configured server
                if (m_dataChannel is ClientBase clientBase)
                    clientBase.ServerIndex = ServerIndex;

                // Attempt connection to device
                m_dataChannel.ReceiveBufferSize = m_bufferSize;
                m_dataChannel.ConnectionString = m_connectionString;
                m_dataChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
                m_dataChannel.ConnectAsync();
            }
            else if (!(m_serverBasedDataChannel is null))
            {
                // Setup event handlers
                m_serverBasedDataChannel.ClientConnected += m_serverBasedDataChannel_ClientConnected;
                m_serverBasedDataChannel.ClientDisconnected += m_serverBasedDataChannel_ClientDisconnected;
                m_serverBasedDataChannel.ClientConnectingException += m_serverBasedDataChannel_ClientConnectingException;
                m_serverBasedDataChannel.ServerStarted += m_serverBasedDataChannel_ServerStarted;
                m_serverBasedDataChannel.ServerStopped += m_serverBasedDataChannel_ServerStopped;
                m_serverBasedDataChannel.ReceiveClientData += m_serverBasedDataChannel_ReceiveClientData;
                m_serverBasedDataChannel.ReceiveClientDataException += m_serverBasedDataChannel_ReceiveClientDataException;
                m_serverBasedDataChannel.SendClientDataException += m_serverBasedDataChannel_SendClientDataException;
                m_serverBasedDataChannel.UnhandledUserException += m_serverBasedDataChannel_UnhandledUserException;

                // Listen for device connection
                m_serverBasedDataChannel.ReceiveBufferSize = m_bufferSize;
                m_serverBasedDataChannel.ConfigurationString = m_connectionString;
                m_serverBasedDataChannel.MaxClientConnections = 1;
                m_serverBasedDataChannel.Start();
            }
            else
            {
                throw new InvalidOperationException("No data channel was initialized, cannot start frame parser");
            }
        }

        private void InitializeUdpDataChannel(Dictionary<string, string> settings)
        {
            m_multicastServerAddress = null;

            if (!settings.TryGetValue("receiveFrom", out string receiveFromSetting))
            {
                m_dataChannel = new UdpClient();
            }
            else
            {
                IPStack ipStack = Transport.GetInterfaceIPStack(settings);
                m_receiveFromAddress = Transport.CreateEndPoint(receiveFromSetting, 0, ipStack).Address;

                // Set up data channel
                SharedUdpClientReference udpRef = new SharedUdpClientReference();
                udpRef.ReceiveDataFrom += m_dataChannel_ReceiveDataFrom;
                m_dataChannel = udpRef;

                if (!settings.TryGetValue("server", out string serverSetting))
                    return;

                if (settings.TryGetValue("remotePort", out string remotePortSetting))
                    serverSetting = $"{serverSetting}:{remotePortSetting}";

                Match endPointMatch = Regex.Match(serverSetting, Transport.EndpointFormatRegex);

                if (IPAddress.TryParse(endPointMatch.Groups["host"].Value, out IPAddress serverAddress) && Transport.IsMulticastIP(serverAddress))
                {
                    m_multicastServerAddress = serverAddress;
                    udpRef.ReceivePacketInfo = true;
                }
            }
        }

        /// <summary>
        /// Stops the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public void Stop()
        {
            m_enabled = false;
            m_rateCalcTimer.Enabled = false;
            m_configurationFrame = null;

            // Make sure data stream is disabled
            if (!SkipDisableRealTimeData && DisableRealTimeDataOnStop)
            {
                WaitHandle commandWaitHandle = SendDeviceCommand(DeviceCommand.DisableRealTimeData);
                commandWaitHandle?.WaitOne(1000);
            }

            if (!(m_dataChannel is null))
            {
                try
                {
                    m_dataChannel.Disconnect();
                }
                catch (Exception ex)
                {
                    OnParsingException(new ConnectionException($"Failed to properly disconnect data channel: {ex.Message}", ex));
                }
                finally
                {
                    m_dataChannel.ConnectionEstablished -= m_dataChannel_ConnectionEstablished;
                    m_dataChannel.ConnectionAttempt -= m_dataChannel_ConnectionAttempt;
                    m_dataChannel.ConnectionException -= m_dataChannel_ConnectionException;
                    m_dataChannel.ConnectionTerminated -= m_dataChannel_ConnectionTerminated;
                    m_dataChannel.ReceiveData -= m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException -= m_dataChannel_ReceiveDataException;
                    m_dataChannel.SendDataException -= m_dataChannel_SendDataException;
                    m_dataChannel.UnhandledUserException -= m_dataChannel_UnhandledUserException;
                    m_dataChannel.Dispose();
                }

                m_dataChannel = null;
            }

            m_readNextBuffer = null;

            if (!(m_serverBasedDataChannel is null))
            {
                try
                {
                    m_serverBasedDataChannel.DisconnectAll();
                }
                catch (Exception ex)
                {
                    OnParsingException(new ConnectionException($"Failed to properly disconnect server based data channel: {ex.Message}", ex));
                }
                finally
                {
                    m_serverBasedDataChannel.ClientConnected -= m_serverBasedDataChannel_ClientConnected;
                    m_serverBasedDataChannel.ClientDisconnected -= m_serverBasedDataChannel_ClientDisconnected;
                    m_serverBasedDataChannel.ClientConnectingException -= m_serverBasedDataChannel_ClientConnectingException;
                    m_serverBasedDataChannel.ServerStarted -= m_serverBasedDataChannel_ServerStarted;
                    m_serverBasedDataChannel.ServerStopped -= m_serverBasedDataChannel_ServerStopped;
                    m_serverBasedDataChannel.ReceiveClientData -= m_serverBasedDataChannel_ReceiveClientData;
                    m_serverBasedDataChannel.ReceiveClientDataException -= m_serverBasedDataChannel_ReceiveClientDataException;
                    m_serverBasedDataChannel.SendClientDataException -= m_serverBasedDataChannel_SendClientDataException;
                    m_serverBasedDataChannel.UnhandledUserException -= m_serverBasedDataChannel_UnhandledUserException;
                    m_serverBasedDataChannel.Dispose();
                }

                m_serverBasedDataChannel = null;
            }

            if (!(m_commandChannel is null))
            {
                try
                {
                    m_commandChannel.Disconnect();
                }
                catch (Exception ex)
                {
                    OnParsingException(new ConnectionException($"Failed to properly disconnect command channel: {ex.Message}", ex));
                }
                finally
                {
                    m_commandChannel.ConnectionEstablished -= m_commandChannel_ConnectionEstablished;
                    m_commandChannel.ConnectionAttempt -= m_commandChannel_ConnectionAttempt;
                    m_commandChannel.ConnectionException -= m_commandChannel_ConnectionException;
                    m_commandChannel.ConnectionTerminated -= m_commandChannel_ConnectionTerminated;
                    m_commandChannel.ReceiveData -= m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException -= m_commandChannel_ReceiveDataException;
                    m_commandChannel.SendDataException -= m_commandChannel_SendDataException;
                    m_commandChannel.UnhandledUserException -= m_commandChannel_UnhandledUserException;
                    m_commandChannel.Dispose();
                }

                m_commandChannel = null;
            }

            if (!(m_frameParser is null))
            {
                try
                {
                    m_frameParser.Stop();
                }
                catch (Exception ex)
                {
                    OnParsingException(ex, "Failed to properly stop frame parser: {0}", ex.Message);
                }
                finally
                {
                    m_frameParser.ReceivedCommandFrame -= m_frameParser_ReceivedCommandFrame;
                    m_frameParser.ReceivedConfigurationFrame -= m_frameParser_ReceivedConfigurationFrame;
                    m_frameParser.ReceivedDataFrame -= m_frameParser_ReceivedDataFrame;
                    m_frameParser.ReceivedHeaderFrame -= m_frameParser_ReceivedHeaderFrame;
                    m_frameParser.ReceivedUndeterminedFrame -= m_frameParser_ReceivedUndeterminedFrame;
                    m_frameParser.ReceivedFrameImage -= m_frameParser_ReceivedFrameImage;
                    m_frameParser.ConfigurationChanged -= m_frameParser_ConfigurationChanged;
                    m_frameParser.ParsingException -= m_frameParser_ParsingException;
                    m_frameParser.BufferParsed -= m_frameParser_BufferParsed;

                    if (!(ReceivedFrameBufferImage is null))
                        m_frameParser.ReceivedFrameBufferImage -= m_frameParser_ReceivedFrameBufferImage;

                    m_frameParser.Dispose();
                }

                m_frameParser = null;
            }
        }

        /// <summary>
        /// Requests that the client attempt to move to the next <see cref="ServerIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Return value will only be <c>true</c> if <see cref="ServerIndex"/> changed.
        /// </remarks>
        public bool RequestNextServerIndex()
        {
            if (!IsConnected)
                return false;

            if (!(m_commandChannel is null))
            {
                if (m_commandChannel.RequestNextServerIndex())
                    ServerIndex = m_commandChannel.ServerIndex;
            }

            if (!(m_dataChannel is null))
            {
                if (m_dataChannel.RequestNextServerIndex())
                    ServerIndex = m_dataChannel.ServerIndex;
            }

            return false;
        }

        /// <summary>
        /// Sends the specified raw command to the remote device.
        /// </summary>
        /// <param name="rawCommand"><see cref="ushort"/> to send to the remote device.</param>
        /// <remarks>
        /// Command will only be sent if <see cref="DeviceSupportsCommands"/> is <c>true</c> and <see cref="MultiProtocolFrameParser"/>.
        /// </remarks>
        /// <returns>A <see cref="WaitHandle"/>.</returns>
        public WaitHandle SendRawDeviceCommand(ushort rawCommand) => SendDeviceCommand((DeviceCommand)rawCommand);

        /// <summary>
        /// Sends the specified <see cref="DeviceCommand"/> to the remote device.
        /// </summary>
        /// <param name="command"><see cref="DeviceCommand"/> to send to the remote device.</param>
        /// <remarks>
        /// Command will only be sent if <see cref="DeviceSupportsCommands"/> is <c>true</c> and <see cref="MultiProtocolFrameParser"/>.
        /// </remarks>
        /// <returns>A <see cref="WaitHandle"/>.</returns>
        public WaitHandle SendDeviceCommand(DeviceCommand command)
        {
            WaitHandle handle = null;

            try
            {
                if (DeviceSupportsCommands && (!(m_dataChannel is null) || !(m_serverBasedDataChannel is null) || !(m_commandChannel is null)))
                {
                    ICommandFrame commandFrame;

                    // Only IEEE C37.118-2011 supports requests for config frame 3, for any other protocol,
                    // downgrade frame request to config frame 2
                    if (command == DeviceCommand.SendConfigurationFrame3 && m_phasorProtocol != PhasorProtocol.IEEEC37_118V2)
                        command = DeviceCommand.SendConfigurationFrame2;

                    // Only the IEEE, SEL Fast Message and Macrodyne protocols support commands
                    switch (m_phasorProtocol)
                    {
                        case PhasorProtocol.IEEEC37_118V2:
                        case PhasorProtocol.IEEEC37_118V1:
                        case PhasorProtocol.IEEEC37_118D6:
                            commandFrame = new IEEEC37_118.CommandFrame(DeviceID, command, 1);
                            break;
                        case PhasorProtocol.IEEE1344:
                            commandFrame = new IEEE1344.CommandFrame(DeviceID, command);
                            break;
                        case PhasorProtocol.IEC61850_90_5:
                            commandFrame = new IEC61850_90_5.CommandFrame(DeviceID, command, 1);
                            break;
                        case PhasorProtocol.SelFastMessage:
                            // Get defined message period
                            MessagePeriod messagePeriod = MessagePeriod.DefaultRate;

                            if (m_connectionParameters is SelFastMessage.ConnectionParameters connectionParameters)
                                messagePeriod = connectionParameters.MessagePeriod;

                            commandFrame = new SelFastMessage.CommandFrame(command, messagePeriod);
                            break;
                        case PhasorProtocol.Macrodyne:
                            commandFrame = new Macrodyne.CommandFrame(command);
                            break;
                        default:
                            commandFrame = null;
                            break;
                    }

                    if (!(commandFrame is null))
                    {
                        byte[] buffer = commandFrame.BinaryImage();

                        // Send command over appropriate communications channel - command channel, if defined,
                        // will take precedence over other communications channels for command traffic...
                        if (!(m_commandChannel is null) && m_commandChannel.CurrentState == ClientState.Connected)
                        {
                            handle = m_commandChannel.SendAsync(buffer, 0, buffer.Length);
                        }
                        else if (!(m_dataChannel is null) && m_dataChannel.CurrentState == ClientState.Connected)
                        {
                            handle = m_dataChannel.SendAsync(buffer, 0, buffer.Length);
                        }
                        else if (!(m_serverBasedDataChannel is null) && m_serverBasedDataChannel.CurrentState == ServerState.Running)
                        {
                            WaitHandle[] handles = m_serverBasedDataChannel.MulticastAsync(buffer, 0, buffer.Length);

                            if (!(handles is null) && handles.Length > 0)
                                handle = handles[0];
                        }

                        SentCommandFrame?.Invoke(this, new EventArgs<ICommandFrame>(commandFrame));
                    }
                }
            }
            catch (Exception ex)
            {
                OnParsingException(new ConnectionException($"Failed to send device command \"{command}\": {ex.Message}", ex));
            }

            return handle;
        }

        /// <summary>
        /// Writes data directly to the frame parsing engine buffer.
        /// </summary>
        /// <remarks>
        /// This method is public to allow consumer to "manually send extra data" to the parsing engine to be parsed, if desired.
        /// </remarks>
        /// <param name="buffer">Buffer containing data to be parsed.</param>
        /// <param name="offset">Offset into buffer where data begins.</param>
        /// <param name="count">Length of data in buffer to be parsed.</param>
        public void Write(byte[] buffer, int offset, int count) => Parse(SourceChannel.Other, buffer, offset, count);

        /// <summary>
        /// Writes a sequence of bytes onto the <see cref="IBinaryImageParser"/> stream for parsing.
        /// </summary>
        /// <param name="source">Defines the source channel for the data.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        internal void Parse(SourceChannel source, byte[] buffer, int offset, int count)
        {
            // Pass data from communications client into protocol specific frame parser
            m_frameParser.Parse(source, buffer, offset, count);

            m_byteRateTotal += count;

            if (m_initiatingDataStream)
                m_initialBytesReceived += count;
        }

        void IFrameParser.Parse(SourceChannel source, byte[] buffer, int offset, int count) => Parse(source, buffer, offset, count);

        /// <summary>
        /// Resets the value for the <see cref="TotalBytesReceived"/> statistic.
        /// </summary>
        public void ResetTotalBytesReceived() => TotalBytesReceived = 0L;

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ParsingException"/> event.</param>
        private void OnParsingException(Exception ex)
        {
            if (!(ex is ThreadAbortException) && !(ex is ObjectDisposedException))
                ParsingException?.Invoke(this, new EventArgs<Exception>(ex));

            if (DateTime.UtcNow.Ticks - m_lastParsingExceptionTime > ParsingExceptionWindow)
            {
                // Exception window has passed since last exception, so we reset counters
                m_lastParsingExceptionTime = DateTime.UtcNow.Ticks;
                m_parsingExceptionCount = 0;
            }

            m_parsingExceptionCount++;

            if (m_parsingExceptionCount <= AllowedParsingExceptions)
                return;

            try
            {
                // When the parsing exception threshold has been exceeded, connection is stopped
                Stop();
            }
            finally
            {
                // Notify consumer of parsing exception threshold deviation
                OnExceededParsingExceptionThreshold();
                m_lastParsingExceptionTime = 0;
                m_parsingExceptionCount = 0;
            }
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="innerException">Actual exception to send as inner exception to <see cref="ParsingException"/> event.</param>
        /// <param name="message">Message of new exception to send to <see cref="ParsingException"/> event.</param>
        /// <param name="args">Arguments of message of new exception to send to <see cref="ParsingException"/> event.</param>
        private void OnParsingException(Exception innerException, string message, params object[] args)
        {
            if (!(innerException is ThreadAbortException) && !(innerException is ObjectDisposedException))
                OnParsingException(new Exception(string.Format(message, args), innerException));
        }

        /// <summary>
        /// Raises the <see cref="ExceededParsingExceptionThreshold"/> event.
        /// </summary>
        private void OnExceededParsingExceptionThreshold() => ExceededParsingExceptionThreshold?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to raise.</param>
        /// <param name="connectionAttempts">Number of connection attempts to report.</param>
        private void OnConnectionException(Exception ex, int connectionAttempts)
        {
            if (!(ex is ThreadAbortException) && !(ex is ObjectDisposedException))
                ConnectionException?.Invoke(this, new EventArgs<Exception, int>(ex, connectionAttempts));
        }

        /// <summary>
        /// Derives a flag based on settings that determines if the current connection supports device commands.
        /// </summary>
        /// <returns>Derived flag that determines if the current connection supports device commands.</returns>
        private bool DeriveCommandSupport()
        {
            // Command support is based on phasor protocol, transport protocol and connection style
            if (IsIEEEProtocol || m_phasorProtocol == PhasorProtocol.IEC61850_90_5 || m_phasorProtocol == PhasorProtocol.SelFastMessage || m_phasorProtocol == PhasorProtocol.Macrodyne)
            {
                // IEEE protocols using TCP or Serial connection support device commands
                if (m_transportProtocol == TransportProtocol.Tcp || m_transportProtocol == TransportProtocol.Serial)
                    return true;

                if (string.IsNullOrWhiteSpace(m_connectionString))
                    return false;

                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                // A defined command channel inherently means commands are supported
                if (settings.ContainsKey("commandChannel"))
                    return true;

                if (m_transportProtocol != TransportProtocol.Udp)
                    return false;

                // IEEE protocols "can" use UDP connection to support devices commands, but only
                // when remote device acts as a UDP listener (i.e., a "server" connection)
                // and remote device is not a multicast end point
                if (!settings.TryGetValue("server", out string server))
                    return false;

                if (!settings.ContainsKey("remotePort"))
                {
                    Match endPoint = Regex.Match(server, Transport.EndpointFormatRegex);
                    server = endPoint.Groups["host"].Value;
                }

                if (IPAddress.TryParse(server, out IPAddress serverAddress))
                    return !Transport.IsMulticastIP(serverAddress);
            }

            return false;
        }

        // Starts data parsing sequence.
        private void StartDataParsingSequence()
        {
            try
            {
                // Attempt to stop real-time data, waiting a maximum of three seconds for this activity
                if (!SkipDisableRealTimeData && m_phasorProtocol != PhasorProtocol.IEC61850_90_5)
                {
                    // Some devices will only send a config frame once data streaming has been disabled, so
                    // we use this code to disable real-time data and wait for data to stop streaming...
                    int attempts = 0;

                    // Make sure data stream is disabled
                    SendDeviceCommand(DeviceCommand.DisableRealTimeData);

                    Thread.Sleep(1000);

                    // Wait for real-time data stream to cease for up to two seconds
                    while (m_initialBytesReceived > 0)
                    {
                        m_initialBytesReceived = 0;
                        Thread.Sleep(100);

                        attempts++;

                        if (attempts >= 20)
                            break;
                    }
                }

                m_initiatingDataStream = false;

                // Request configuration frame once real-time data has been disabled. Note that data stream
                // will be enabled when we receive a configuration frame. 
                switch (m_phasorProtocol)
                {
                    case PhasorProtocol.SelFastMessage:
                        // SEL Fast Message doesn't define a binary configuration frame so we skip
                        // requesting one and jump straight to enabling the data stream.
                        SendDeviceCommand(DeviceCommand.EnableRealTimeData);
                        break;
                    case PhasorProtocol.Macrodyne:
                        // We collect the station name (i.e. the unit ID via 0xBB 0x48)) from the Macrodyne
                        // protocol interpreted as a header frame before we get the configuration frame
                        bool sendCommand = true;

                        if (m_connectionParameters is Macrodyne.ConnectionParameters parameters)
                            sendCommand = parameters.ProtocolVersion != ProtocolVersion.G;

                        if (sendCommand)
                            SendDeviceCommand(DeviceCommand.SendHeaderFrame);

                        break;
                    default:
                        // Otherwise we just request the configuration frame
                        SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
        }

        // Calculate frame and data rates
        private void m_rateCalcTimer_Elapsed(object sender, EventArgs<DateTime> e)
        {
            double time = Ticks.ToSeconds(DateTime.UtcNow.Ticks - m_dataStreamStartTime);

            CalculatedFrameRate = m_frameRateTotal / time;
            ByteRate = m_byteRateTotal / time;

            // Since rate calculation timer is not precise, the missing frames calculation can be calculated out
            // of sequence with the total frames. If there is a negative balance, we cache the value so it can
            // be applied to the next calculation to keep calculation more accurate.
            long missingFrames = (long)(ConfiguredFrameRate * m_rateCalcTimer.Interval * SI.Milli * (RedundantFramesPerPacket + 1)) - m_frameRateTotal;

            if (missingFrames > 0)
            {
                TotalMissingFrames += missingFrames + m_missingFramesOverflow;
                m_missingFramesOverflow = 0;
            }
            else
            {
                m_missingFramesOverflow = missingFrames;
            }

            TotalFramesReceived += m_frameRateTotal;
            TotalBytesReceived += m_byteRateTotal;

            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_dataStreamStartTime = DateTime.UtcNow.Ticks;
        }

        // Handles needed start-up actions once a client is connected
        private void ClientConnectedHandler()
        {
            try
            {
                ConnectionEstablished?.Invoke(this, EventArgs.Empty);

                if (!DeviceSupportsCommands || !AutoStartDataParsingSequence)
                    return;

                m_initialBytesReceived = 0L;
                m_initiatingDataStream = true;

                // Begin data parsing sequence to handle reception of configuration frame
                Thread startDataParsingThread = new Thread(StartDataParsingSequence)
                {
                    IsBackground = true
                };

                startDataParsingThread.Start();
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
        }

        private void MaintainCapturedFrameReplayTiming(IFrame sourceFrame)
        {
            long simulatedTimestamp = 0L;

            if (m_inputTimer is null)
            {
                if (m_lastFrameReceivedTime > 0L)
                {
                    // To maintain timing on "frames per second", we wait for defined frame rate interval
                    double sleepTime = 1.0D / m_definedFrameRate - (DateTime.UtcNow.Ticks - m_lastFrameReceivedTime) / (double)Ticks.PerSecond;

                    // Thread sleep time is a minimum suggested sleep time depending on system activity, when not using high-resolution
                    // input timer we assume getting close is good enough
                    if (sleepTime > 0.0D)
                        Thread.Sleep((int)(sleepTime * 1000.0D));
                }

                m_lastFrameReceivedTime = DateTime.UtcNow.Ticks;

                if (InjectSimulatedTimestamp)
                    simulatedTimestamp = Ticks.AlignToMillisecondDistribution(m_lastFrameReceivedTime, m_definedFrameRate);
            }
            else
            {
                // When high resolution input timing is requested, we only need to wait for the next signal...
                m_inputTimer.FrameWaitHandle.Wait();

                // Input timer can be disabled while thread is waiting, so we make sure it is not null
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (!(m_inputTimer is null))
                    simulatedTimestamp = m_inputTimer.LastFrameTime;
            }

            // If injecting a simulated timestamp, use the last received time
            if (InjectSimulatedTimestamp)
                sourceFrame.Timestamp = simulatedTimestamp;

            // Read next buffer if output frames are almost all processed
            if (QueuedOutputs < 2)
                m_readNextBuffer?.RunOnceAsync();
        }

        #region [ Data Channel Event Handlers ]

        private void m_dataChannel_ReceiveDataFrom(object sender, EventArgs<EndPoint, IPPacketInformation, int> e)
        {
            int length = e.Argument3;

            if (!(e.Argument1 is IPEndPoint remoteEndPoint))
                return;

            if (!remoteEndPoint.Address.Equals(m_receiveFromAddress))
                return;

            IPAddress destinationAddress = e.Argument2.Address;

            if (!(destinationAddress is null) && !(m_multicastServerAddress is null) && !destinationAddress.Equals(m_multicastServerAddress))
                return;

            byte[] buffer = new byte[length];
            length = m_dataChannel.Read(buffer, 0, length);
            Parse(SourceChannel.Data, buffer, 0, length);
        }

        private void m_dataChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            int length = e.Argument;
            byte[] buffer = new byte[length];
            length = m_dataChannel.Read(buffer, 0, length);
            Parse(SourceChannel.Data, buffer, 0, length);
        }

        private void m_dataChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            // Handle client connection from data channel
            ClientConnectedHandler();

            try
            {
                // Start reading file data
                if (m_transportProtocol == TransportProtocol.File)
                    m_readNextBuffer?.RunOnceAsync();
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnParsingException(new InvalidOperationException($"Failed to queue file read operation due to exception: {ex.Message}", ex));
            }
        }

        private void m_dataChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;
            ConnectionAttempt?.Invoke(this, EventArgs.Empty);
        }

        private void m_dataChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnConnectionException(ex, m_connectionAttempts);

            // Next server index is selected when a connection exception occurs
            if (sender is ClientBase clientBase)
                ServerIndex = clientBase.ServerIndex;
        }

        private void m_dataChannel_ConnectionTerminated(object sender, EventArgs e) => ConnectionTerminated?.Invoke(this, EventArgs.Empty);

        private void m_dataChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Data channel send exception: {ex.Message}", ex));
        }

        private void m_dataChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            // For some serially connected devices, a frame exception on initial connection is very common - so we ignore this during startup
            if (m_initiatingSerialConnection && ex is SerialException serialEx && serialEx.SerialError == SerialError.Frame)
            {
                m_initiatingSerialConnection = false;
                return;
            }

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Data channel receive exception: {ex.Message}", ex));
        }

        private void m_dataChannel_UnhandledUserException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(ex, "Data channel user unhandled exception: {0}", ex.Message);
        }

        #endregion

        #region [ Server Based Data Channel Event Handlers ]

        private void m_serverBasedDataChannel_ReceiveClientData(object sender, EventArgs<Guid, int> e)
        {
            Guid clientID = e.Argument1;
            int length = e.Argument2;
            byte[] buffer = new byte[length];

            length = m_serverBasedDataChannel.Read(clientID, buffer, 0, length);
            Parse(SourceChannel.Data, buffer, 0, length);
        }

        private void m_serverBasedDataChannel_ClientConnected(object sender, EventArgs<Guid> e) => ClientConnectedHandler();

        private void m_serverBasedDataChannel_ClientDisconnected(object sender, EventArgs<Guid> e) => ConnectionTerminated?.Invoke(this, EventArgs.Empty);

        private void m_serverBasedDataChannel_ClientConnectingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Server based data channel send exception: {ex.Message}", ex));
        }

        private void m_serverBasedDataChannel_ServerStarted(object sender, EventArgs e) => ServerStarted?.Invoke(this, EventArgs.Empty);

        private void m_serverBasedDataChannel_ServerStopped(object sender, EventArgs e) => ServerStopped?.Invoke(this, EventArgs.Empty);

        private void m_serverBasedDataChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Server based data channel send exception: {ex.Message}", ex));
        }

        private void m_serverBasedDataChannel_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Server based data channel receive exception: {ex.Message}", ex));
        }

        private void m_serverBasedDataChannel_UnhandledUserException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(ex, "Server based data channel user unhandled exception: {0}", ex.Message);
        }

        #endregion

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            int length = e.Argument;
            byte[] buffer = new byte[length];
            length = m_commandChannel.Read(buffer, 0, length);
            Parse(SourceChannel.Command, buffer, 0, length);
        }

        private void m_commandChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            try
            {
                // We'll start data channel once command channel has been established...
                InitializeDataChannel(m_connectionString.ParseKeyValuePairs());
            }
            catch (Exception ex)
            {
                OnConnectionException(ex, m_connectionAttempts);
            }
        }

        private void m_commandChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;
            ConnectionAttempt?.Invoke(this, EventArgs.Empty);
        }

        private void m_commandChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnConnectionException(ex, m_connectionAttempts);

            // Next server index is selected when a connection exception occurs
            if (sender is ClientBase clientBase)
                ServerIndex = clientBase.ServerIndex;
        }

        private void m_commandChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            if (!KeepCommandChannelOpen)
                return;

            ConnectionTerminated?.Invoke(this, EventArgs.Empty);
        }

        private void m_commandChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Command channel send exception: {ex.Message}", ex));
        }

        private void m_commandChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(new ConnectionException($"Command channel receive exception: {ex.Message}", ex));
        }

        private void m_commandChannel_UnhandledUserException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(ex, "Command channel user unhandled exception: {0}", ex.Message);
        }

        #endregion

        #region [ Frame Parser Event Handlers ]

        private void m_frameParser_ReceivedCommandFrame(object sender, EventArgs<ICommandFrame> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (InjectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                ReceivedCommandFrame?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedCommandFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedConfigurationFrame(object sender, EventArgs<IConfigurationFrame> e)
        {
            // We automatically request enabling of real-time data upon reception of config frame if requested. Note that SEL Fast Message will
            // have already been enabled at this point so we don't duplicate request for enabling real-time data stream
            if (m_configurationFrame is null && DeviceSupportsCommands && AutoStartDataParsingSequence && m_phasorProtocol != PhasorProtocol.SelFastMessage && m_phasorProtocol != PhasorProtocol.IEC61850_90_5)
                SendDeviceCommand(DeviceCommand.EnableRealTimeData);

            m_configurationFrame = e.Argument;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (InjectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                ReceivedConfigurationFrame?.Invoke(this, e);

                if (!(m_configurationFrame is null))
                    ConfiguredFrameRate = m_configurationFrame.FrameRate;
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedConfigurationFrame\" consumer event handler exception: {0}", ex.Message);
            }

            // If user has requested to not keep the command channel open, disconnect it once the system has received a configuration frame
            if (!KeepCommandChannelOpen && !(m_commandChannel is null) && m_commandChannel.CurrentState == ClientState.Connected)
                m_commandChannel.Disconnect();
        }

        private void m_frameParser_ReceivedDataFrame(object sender, EventArgs<IDataFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                bool publishFrame = true;
                IDataFrame dataFrame = e.Argument;

                if (m_transportProtocol == TransportProtocol.File)
                {
                    DateTime timestamp = dataFrame.Timestamp;

                    if (timestamp >= ReplayStartTime && timestamp < ReplayStopTime)
                    {
                        MaintainCapturedFrameReplayTiming(dataFrame);
                    }
                    else
                    {
                        publishFrame = false;

                        // Read next buffer if output frames are almost all processed
                        if (QueuedBuffers < 2)
                            m_readNextBuffer?.RunOnceAsync();
                    }
                }
                else if (InjectSimulatedTimestamp)
                {
                    dataFrame.Timestamp = DateTime.UtcNow.Ticks;
                }

                if (publishFrame)
                    ReceivedDataFrame?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedDataFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedHeaderFrame(object sender, EventArgs<IHeaderFrame> e)
        {
            // Macrodyne receives header frame which contains station name before configuration frame (this gets online data format: 0xBB 0x24)
            if (m_configurationFrame is null && m_phasorProtocol == PhasorProtocol.Macrodyne)
                SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (InjectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                ReceivedHeaderFrame?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedHeaderFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedUndeterminedFrame(object sender, EventArgs<IChannelFrame> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (InjectSimulatedTimestamp)
                    e.Argument.Timestamp = DateTime.UtcNow.Ticks;

                ReceivedUndeterminedFrame?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedUndeterminedFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedFrameImage(object sender, EventArgs<FundamentalFrameType, int> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                ReceivedFrameImage?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedFrameImage\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedFrameBufferImage(object sender, EventArgs<FundamentalFrameType, byte[], int, int> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                ReceivedFrameBufferImage?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedFrameBufferImage\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ConfigurationChanged(object sender, EventArgs e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                ConfigurationChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ConfigurationChanged\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ParsingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (ex is CrcException)
                TotalCrcExceptions++;

            OnParsingException(ex);
        }

        private void m_frameParser_BufferParsed(object sender, EventArgs e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                BufferParsed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"BufferParsed\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void ReadNextFileBuffer()
        {
            if (!(m_dataChannel is FileClient fileClient))
                return;

            if (fileClient.CurrentState == ClientState.Connected)
                fileClient.ReadNextBuffer();
        }

        #endregion

        #endregion

        #region [ Static ]

        private static readonly SharedTimerScheduler TimerScheduler;

        static MultiProtocolFrameParser()
        {
            using (Logger.AppendStackMessages("Owner", "MultiProtocolFrameParser"))
                TimerScheduler = new SharedTimerScheduler();
        }

        #endregion
    }
}
