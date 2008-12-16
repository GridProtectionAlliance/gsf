//*******************************************************************************************************
//  IClient.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
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
//  05/01/2007 - Pinal C. Patel
//       Made WaitForConnection() functions that returns a boolean value indicating success or failure
//  09/27/2007 - J. Ritchie Carroll
//       Added disconnect timeout overload
//  09/29/2008 - James R Carroll
//       Converted to C#.
//  11/07/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Threading;
using PCS.IO.Compression;
using PCS.Security.Cryptography;

namespace PCS.Communication
{
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

    /// <summary>
    /// Defines a client involved in server-client communication.
    /// </summary>
    public interface IClient : ISupportLifecycle, IProvideStatus
    {
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
        /// Occurs when server-client handshake, when enabled, cannot be performed within the specified <see cref="HandshakeTimeout"/> time.
        /// </summary>
        event EventHandler HandshakeProcessTimeout;

        /// <summary>
        /// Occurs when server-client handshake, when enabled, cannot be performed successfully due to information mismatch.
        /// </summary>
        event EventHandler HandshakeProcessUnsuccessful;

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
        /// Occurs when no data is received from the server for the <see cref="ReceiveTimeout"/> time.
        /// </summary>
        event EventHandler ReceiveDataTimeout;

        /// <summary>
        /// Occurs when the client receives data from the server.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the buffer containing data received from the server starting at index zero.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of bytes received in the buffer from the server.
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
        /// Gets or sets the data required by the client to connect to the server.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the client will attempt to connect to the server.
        /// </summary>
        int MaxConnectionAttempts { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the client will do a handshake with the server after the connection has been established.
        /// </summary>
        bool Handshake { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds that the client will wait for the server's response to the <see cref="Handshake"/>.
        /// </summary>
        int HandshakeTimeout { get; set; }

        /// <summary>
        /// Gets or sets the passpharse that the client will provide for authentication during the <see cref="Handshake"/> process.
        /// </summary>
        string HandshakePassphrase { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the data exchanged between the client and server will be encrypted using a private session passphrase.
        /// </summary>
        bool SecureSession { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds after which the client will raise the <see cref="ReceiveDataTimeout"/> event if no data is received from the server.
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer used by the client for receiving data from the server.
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CipherStrength"/> to be used for ciphering the data exchanged between the client and server.
        /// </summary>
        CipherStrength Encryption { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CompressionStrength"/> to be used for compressing the data exchanged between the client and server.
        /// </summary>
        CompressionStrength Compression { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
        /// </summary>
        Encoding TextEncoding { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Delegate"/> to be invoked instead of the <see cref="ReceiveDataComplete"/> event when data is received from server.
        /// </summary>
        /// <remarks>
        /// arg1 in <see cref="ReceiveDataHandler"/> is the buffer containing the data received from the server.<br/>
        /// arg2 in <see cref="ReceiveDataHandler"/> is the zero based starting offset into the buffer containing the data received from the server.<br/>
        /// arg3 in <see cref="ReceiveDataHandler"/> is the number of bytes received from the server that is stored in the buffer (arg1) starting at index 0.
        /// </remarks>
        Action<byte[],int,int> ReceiveDataHandler { get; set; }

        /// <summary>
        /// Gets the server ID.
        /// </summary>
        Guid ServerID { get; set; }

        /// <summary>
        /// Gets the client ID.
        /// </summary>
        Guid ClientID { get; set; }

        /// <summary>
        /// Gets the current <see cref="ClientState"/>.
        /// </summary>
        ClientState CurrentState { get; }

        /// <summary>
        /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
        /// </summary>
        TransportProtocol TransportProtocol { get; }

        /// <summary>
        /// Gets the total number of seconds for which the client has been connected to the server.
        /// </summary>
        double ConnectionTime { get; }

        /// <summary>
        /// Connects client to the server synchronously.
        /// </summary>
        void Connect();

        /// <summary>
        /// Connects client to the server asynchronously.
        /// </summary>
        void ConnectAsync();

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
    }
}

///// <summary>
///// Gets the total number of bytes sent by the client to the server since the connection is established.
///// </summary>
///// <value></value>
///// <returns>The total number of bytes sent by the client to the server since the connection is established.</returns>
//long TotalBytesSent { get; }

///// <summary>
///// Gets the total number of bytes received by the client from the server since the connection is established.
///// </summary>
///// <value></value>
///// <returns>The total number of bytes received by the client from the server since the connection is established.</returns>
//long TotalBytesReceived { get; }

///// <summary>
///// Cancels asynchronous connection attempt to the server.
///// </summary>
//void CancelConnect();

///// <summary>
///// Disconnects the client from the connected server, timing out within specified milliseconds if needed.
///// </summary>
//void Disconnect(int timeout);

///// <summary>
///// Sends data to the server.
///// </summary>
///// <param name="data">The plain-text data that is to be sent to the server.</param>
//void Send(string data);

///// <summary>
///// Sends data to the server.
///// </summary>
///// <param name="serializableObject">The serializable object that is to be sent to the server.</param>
//void Send(object serializableObject);

///// <summary>
///// Sends data to the server.
///// </summary>
///// <param name="data">The binary data that is to be sent to the server.</param>
//void Send(byte[] data);

///// <summary>
///// Waits for the client to connect to the server for the specified time and optionally stop the client from
///// retrying connection attempts if the client is unable to connect to the server within the specified time.
///// </summary>
///// <param name="waitTime">
///// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1 or 0
///// will cause this method to wait indefinately until the client establishes connection with the server.
///// </param>
//bool WaitForConnection(int waitTime);

///// <summary>
///// Waits for the client to connect to the server for the specified time and optionally stop the client from
///// retrying connection attempts if the client is unable to connect to the server within the specified time.
///// </summary>
///// <param name="waitTime">
///// The time in milliseconds to wait for the client to connect to the server. Specifying a value of -1 or 0
///// will cause this method to wait indefinately until the client establishes connection with the server.
///// </param>
///// <param name="stopRetrying">
///// Boolean value indicating whether the client should stop trying to connect to the server if it is unable to
///// connect to the server after waiting for the specified duration.
///// </param>
//bool WaitForConnection(int waitTime, bool stopRetrying);