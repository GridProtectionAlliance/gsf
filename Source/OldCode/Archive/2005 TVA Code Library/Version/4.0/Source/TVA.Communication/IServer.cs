//*******************************************************************************************************
//  IServer.cs
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
//  07/26/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/06/2006 - J. Ritchie Carroll
//       Added ReceiveRawDataFunction delegate to allow bypass optimizations for high-speed data access
//  09/29/2008 - James R Carroll
//       Converted to C#.
//  11/07/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Threading;
using TVA.IO.Compression;
using TVA.Security.Cryptography;
using TVA.Units;

namespace TVA.Communication
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the current state of the server.
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// Server is running.
        /// </summary>
        Running,
        /// <summary>
        /// Server is not running.
        /// </summary>
        NotRunning
    }

    #endregion

    /// <summary>
    /// Defines a server involved in server-client communication.
    /// </summary>
    public interface IServer : ISupportLifecycle, IProvideStatus
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the server is started.
        /// </summary>
        event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the server is stopped.
        /// </summary>
        event EventHandler ServerStopped;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed within the specified <see cref="HandshakeTimeout"/> time.
        /// </summary>
        event EventHandler HandshakeProcessTimeout;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.
        /// </summary>
        event EventHandler HandshakeProcessUnsuccessful;

        /// <summary>
        /// Occurs when a client connects to the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that connected to the server.
        /// </remarks>
        event EventHandler<EventArgs<Guid>> ClientConnected;

        /// <summary>
        /// Occurs when a client disconnects from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client that disconnected from the server.
        /// </remarks>
        event EventHandler<EventArgs<Guid>> ClientDisconnected;

        /// <summary>
        /// Occurs when data is being sent to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data is being sent.
        /// </remarks>
        event EventHandler<EventArgs<Guid>> SendClientDataStart;

        /// <summary>
        /// Occurs when data has been sent to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the ID of the client to which the data has been sent.
        /// </remarks>
        event EventHandler<EventArgs<Guid>> SendClientDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when sending data to a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client to which the data was being sent.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when sending data to a client.
        /// </remarks>
        event EventHandler<EventArgs<Guid, Exception>> SendClientDataException;

        /// <summary>
        /// Occurs when no data is received from a client for the <see cref="ReceiveTimeout"/> time.
        /// </summary>
        event EventHandler<EventArgs<Guid>> ReceiveClientDataTimeout;

        /// <summary>
        /// Occurs when data is received from a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2,T3}.Argument1"/> is the ID of the client from which data is received.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument2"/> is the buffer containing data received from the client starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2,T3}.Argument3"/> is the number of bytes received in the buffer from the client.
        /// </remarks>
        event EventHandler<EventArgs<Guid, byte[], int>> ReceiveClientDataComplete;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when receiving data from a client.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the client from which the data was being received.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when receiving data from a client.
        /// </remarks>
        event EventHandler<EventArgs<Guid, Exception>> ReceiveClientDataException;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the data required by the server to initialize.
        /// </summary>
        string ConfigurationString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of clients that can connect to the server.
        /// </summary>
        int MaxClientConnections { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the server will do a handshake with the clients after the connection has been established.
        /// </summary>
        bool Handshake { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that the server will wait for the clients to initiate the <see cref="Handshake"/> process.
        /// </summary>
        int HandshakeTimeout { get; set; }

        /// <summary>
        /// Gets or sets the passpharse that the clients must provide for authentication during the <see cref="Handshake"/> process.
        /// </summary>
        string HandshakePassphrase { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data exchanged between the server and clients will be encrypted using a private session passphrase.
        /// </summary>
        bool SecureSession { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds after which the server will raise the <see cref="ReceiveClientDataTimeout"/> event if no data is received from a client.
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer used by the server for receiving data from the clients.
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CipherStrength"/> to be used for ciphering the data exchanged between the server and clients.
        /// </summary>
        CipherStrength Encryption { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CompressionStrength"/> to be used for compressing the data exchanged between the server and clients.
        /// </summary>
        CompressionStrength Compression { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the connected clients.
        /// </summary>
        Encoding TextEncoding { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Delegate"/> to be invoked instead of the <see cref="ReceiveClientDataComplete"/> event when data is received from clients.
        /// </summary>
        /// <remarks>
        /// arg1 in <see cref="ReceiveClientDataHandler"/> is the ID or the client from which data is received.<br/>
        /// arg2 in <see cref="ReceiveClientDataHandler"/> is the buffer containing data received from the client starting at index zero.<br/>
        /// arg3 in <see cref="ReceiveClientDataHandler"/> is the zero-based starting offset into the buffer containing the data received from the server.<br/>
        /// arg4 in <see cref="ReceiveClientDataHandler"/> is the number of bytes received from the client that is stored in the buffer (arg2).
        /// </remarks>
        Action<Guid, byte[], int, int> ReceiveClientDataHandler { get; set; }

        /// <summary>
        /// Gets the current <see cref="ServerState"/>.
        /// </summary>
        ServerState CurrentState { get; }

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the server for the transportation of data with the clients.
        /// </summary>
        TransportProtocol TransportProtocol { get; }

        /// <summary>
        /// Gets the server's ID.
        /// </summary>
        Guid ServerID { get; }

        /// <summary>
        /// Gets the IDs of clients connected to the server.
        /// </summary>
        Guid[] ClientIDs { get; }

        /// <summary>
        /// Gets the <see cref="Time"/> for which the server has been running.
        /// </summary>
        Time RunTime { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Starts the server.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Disconnects all of the connected clients.
        /// </summary>
        void DisconnectAll();

        /// <summary>
        /// Disconnects a connected client.
        /// </summary>
        /// <param name="clientID">ID of the client to be disconnected.</param>
        void DisconnectOne(Guid clientID);

        /// <summary>
        /// Sends data to the specified client synchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        void SendTo(Guid clientID, byte[] data, int offset, int length);

        /// <summary>
        /// Sends data to all of the connected clients synchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        void Multicast(byte[] data, int offset, int length);

        /// <summary>
        /// Sends data to the specified client asynchronously.
        /// </summary>
        /// <param name="clientID">ID of the client to which the data is to be sent.</param>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        WaitHandle SendToAsync(Guid clientID, byte[] data, int offset, int length);

        /// <summary>
        /// Sends data to all of the connected clients asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent.</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
        /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns>Array of <see cref="WaitHandle"/> for the asynchronous operation.</returns>
        WaitHandle[] MulticastAsync(byte[] data, int offset, int length);

        #endregion
    }
}