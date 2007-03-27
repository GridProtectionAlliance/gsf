'*******************************************************************************************************
'  TVA.Communication.ICommunicationClient.vb - Abstract communications client interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  07/26/2006 - Pinal C. Patel
'       Original version of source code generated
'  09/06/2006 - J. Ritchie Carroll
'       Added ReceiveRawDataFunction delegate to allow bypass optimizations for high-speed data access
'
'*******************************************************************************************************

Imports TVA.Communication.Common

Public Interface ICommunicationServer

    Inherits TVA.Services.IServiceComponent

    ''' <summary>
    ''' Occurs when the server is started.
    ''' </summary>
    Event ServerStarted As EventHandler

    ''' <summary>
    ''' Occurs when the server is stopped.
    ''' </summary>
    Event ServerStopped As EventHandler

    ''' <summary>
    ''' Occurs when an exception is encountered while starting up the server.
    ''' </summary>
    Event ServerStartupException As EventHandler(Of ExceptionEventArgs)

    ''' <summary>
    ''' Occurs when a client is connected to the server.
    ''' </summary>
    Event ClientConnected As EventHandler(Of IdentifiableSourceEventArgs)

    ''' <summary>
    ''' Occurs when a client is disconnected from the server.
    ''' </summary>
    Event ClientDisconnected As EventHandler(Of IdentifiableSourceEventArgs)

    ''' <summary>
    ''' Occurs when data is received from a client.
    ''' </summary>
    Event ReceivedClientData As EventHandler(Of IdentifiableItemEventArgs(Of Byte()))

    ''' <summary>
    ''' Gets or sets the data that is required by the server to initialize.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The data that is required by the server to initialize.</returns>
    Property ConfigurationString() As String

    ''' <summary>
    ''' Gets or sets the maximum number of clients that can connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of clients that can connect to the server.</returns>
    Property MaximumClients() As Integer

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the data exchanged between the server and clients
    ''' will be encrypted using a private session passphrase.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the data exchanged between the server and clients will be encrypted using a private session 
    ''' passphrase; otherwise False.
    ''' </returns>
    Property SecureSession() As Boolean

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server will do a handshake with the client after 
    ''' accepting its connection.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server will do a handshake with the client; otherwise False.</returns>
    Property Handshake() As Boolean

    ''' <summary>
    ''' Gets or sets the passpharse that the clients must provide for authentication during the handshake process.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The passpharse that the clients must provide for authentication during the handshake process.</returns>
    Property HandshakePassphrase() As String

    ''' <summary>
    ''' Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of bytes that can be received at a time by the server from the clients.</returns>
    Property ReceiveBufferSize() As Integer

    ''' <summary>
    ''' Gets or sets the encryption level to be used for encrypting the data exchanged between the server and 
    ''' clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encryption level to be used for encrypting the data exchanged between the server and clients.</returns>
    Property Encryption() As TVA.Security.Cryptography.EncryptLevel

    ''' <summary>
    ''' Gets or sets the compression level to be used for compressing the data exchanged between the server and 
    ''' clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The compression level to be used for compressing the data exchanged between the server and clients.</returns>
    Property Compression() As TVA.IO.Compression.CompressLevel

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server is enabled.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server is enabled; otherwise False.</returns>
    Property Enabled() As Boolean

    ''' <summary>
    ''' Gets or sets the encoding to be used for the text sent to the connected clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encoding to be used for the text sent to the connected clients.</returns>
    Property TextEncoding() As System.Text.Encoding

    ''' <summary>
    ''' Gets the protocol used by the server for transferring data to and from the clients.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The protocol used by the server for transferring data to and from the clients.</returns>
    Property Protocol() As TransportProtocol

    ''' <summary>
    ''' Setting this property allows consumer to "intercept" data before it goes through normal processing
    ''' </summary>
    Property ReceiveRawDataFunction() As ReceiveRawDataFunctionSignature

    ''' <summary>
    ''' Gets the current instance of communication server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current instance communication server.</returns>
    ReadOnly Property This() As ICommunicationServer

    ''' <summary>
    ''' Gets the server's ID.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the server.</returns>
    ReadOnly Property ServerID() As Guid

    ''' <summary>
    ''' Gets a collection of client IDs that are connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A collection of client IDs that are connected to the server.</returns>
    ReadOnly Property ClientIDs() As List(Of Guid)

    ''' <summary>
    ''' Gets a boolean value indicating whether the server is currently running.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the server is running; otherwise False.</returns>
    ReadOnly Property IsRunning() As Boolean

    ''' <summary>
    ''' Gets the time in seconds for which the server has been running.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time in seconds for which the server has been running.</returns>
    ReadOnly Property RunTime() As Double

    ''' <summary>
    ''' Starts the server.
    ''' </summary>
    Sub Start()

    ''' <summary>
    ''' Stops the server.
    ''' </summary>
    Sub [Stop]()

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The plain-text data that is to be sent to the client.</param>
    Sub SendTo(ByVal clientID As Guid, ByVal data As String)

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="serializableObject">The serializable object that is to be sent to the client.</param>
    Sub SendTo(ByVal clientID As Guid, ByVal serializableObject As Object)

    ''' <summary>
    ''' Sends data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The binary data that is to be sent to the client.</param>
    Sub SendTo(ByVal clientID As Guid, ByVal data As Byte())

    ''' <summary>
    ''' Sends the specified subset of data from the data buffer to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The buffer that contains the binary data to be sent.</param>
    ''' <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
    ''' <param name="size">The number of bytes to be sent.</param>
    Sub SendTo(ByVal clientID As Guid, ByVal data As Byte(), ByVal offset As Integer, ByVal size As Integer)

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The plain-text data that is to sent to the subscribed clients.</param>
    Sub Multicast(ByVal data As String)

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object that is to be sent to the subscribed clients.</param>
    Sub Multicast(ByVal serializableObject As Object)

    ''' <summary>
    ''' Sends data to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The binary data that is to sent to the subscribed clients.</param>
    Sub Multicast(ByVal data As Byte())

    ''' <summary>
    ''' Sends the specified subset of data from the data buffer to all of the subscribed clients.
    ''' </summary>
    ''' <param name="data">The buffer that contains the binary data to be sent.</param>
    ''' <param name="offset">The zero-based position in the buffer parameter at which to begin sending data.</param>
    ''' <param name="size">The number of bytes to be sent.</param>
    Sub Multicast(ByVal data As Byte(), ByVal offset As Integer, ByVal size As Integer)

End Interface
