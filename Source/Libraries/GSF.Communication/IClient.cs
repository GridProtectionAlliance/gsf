//******************************************************************************************************
//  IClient.cs - Gbtc
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
//  07/26/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added ReceiveRawDataFunction delegate to allow bypass optimizations for high-speed data access.
//  05/01/2007 - Pinal C. Patel
//       Made WaitForConnection() functions that returns a boolean value indicating success or failure.
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  11/07/2008 - Pinal C. Patel
//       Edited code comments.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using System.Threading;
using GSF.Units;

namespace GSF.Communication
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the current state of the client.
    /// </summary>
    public enum ClientState
    {
        /// <summary>
        /// Client is establishing connection.
        /// </summary>
        Connecting,
        /// <summary>
        /// Client has established connection.
        /// </summary>
        Connected,
        /// <summary>
        /// Client connection is terminated.
        /// </summary>
        Disconnected
    }

    #endregion

    /// <summary>
    /// Defines a client involved in server-client communication.
    /// </summary>
    public interface IClient : ISupportLifecycle, IProvideStatus
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when client is attempting connection to the server.
        /// </summary>
        event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when client connection to the server is established.
        /// </summary>
        event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when client connection to the server is terminated.
        /// </summary>
        event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to the server.
        /// </summary>
        event EventHandler<EventArgs<Exception>> ConnectionException;

        /// <summary>
        /// Occurs when the client begins sending data to the server.
        /// </summary>
        event EventHandler SendDataStart;

        /// <summary>
        /// Occurs when the client has successfully sent data to the server.
        /// </summary>
        event EventHandler SendDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending data to the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when sending data to the server.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> SendDataException;

        /// <summary>
        /// Occurs when unprocessed data has been received from the server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event can be used to receive a notification that server data has arrived. The <see cref="Read"/> method can then be used
        /// to copy data to an existing buffer. In many cases it will be optimal to use an existing buffer instead of subscribing to the
        /// <see cref="ReceiveDataComplete"/> event, however, the data that is available after calling the <see cref="Read"/> method
        /// will be the original unprocessed data received by the client, i.e., not optionally decrypted or decompressed data.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the number of bytes received in the buffer from the server.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<int>> ReceiveData;

        /// <summary>
        /// Occurs when data received from the server has been processed and is ready for consumption.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is a new buffer containing post-processed data received from the server starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of post-processed bytes received in the buffer from the server.
        /// </remarks>
        event EventHandler<EventArgs<byte[], int>> ReceiveDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when receiving data from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when receiving data from the server.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ReceiveDataException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered in a user-defined function via an event dispatch.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> thrown by the user-defined function.
        /// </remarks>
        event EventHandler<EventArgs<Exception>> UnhandledUserException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the data required by the client to connect to the server.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the client will attempt to connect to the server.
        /// </summary>
        int MaxConnectionAttempts { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        int SendBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
        /// </summary>
        Encoding TextEncoding { get; set; }

        /// <summary>
        /// Gets the server URI.
        /// </summary>
        string ServerUri { get; }

        /// <summary>
        /// Gets the current server index, when multiple server end points are defined.
        /// </summary>
        int ServerIndex { get; }

        /// <summary>
        /// Gets the current <see cref="ClientState"/>.
        /// </summary>
        ClientState CurrentState { get; }

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
        /// </summary>
        TransportProtocol TransportProtocol { get; }

        /// <summary>
        /// Gets the <see cref="Time"/> for which the client has been connected to the server.
        /// </summary>
        Time ConnectionTime { get; }

        /// <summary>
        /// Gets the <see cref="TransportStatistics"/> for the client connection.
        /// </summary>
        TransportStatistics Statistics { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Connects client to the server synchronously.
        /// </summary>
        void Connect();

        /// <summary>
        /// Connects client to the server asynchronously.
        /// </summary>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        WaitHandle ConnectAsync();

        /// <summary>
        /// Disconnects client from the server synchronously.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends data to the server synchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        void Send(byte[] data, int offset, int length);

        /// <summary>
        /// Sends data to the server asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        WaitHandle SendAsync(byte[] data, int offset, int length);

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ReceiveData"/> event handler. Calling this method outside this event
        /// will have unexpected results.
        /// </remarks>
        int Read(byte[] buffer, int startIndex, int length);

        /// <summary>
        /// Requests that the client attempt to move to the next <see cref="ServerIndex"/>.
        /// </summary>
        /// <returns><c>true</c> if request succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Return value will only be <c>true</c> if <see cref="ServerIndex"/> changed.
        /// </remarks>
        bool RequestNextServerIndex();

        #endregion
    }
}