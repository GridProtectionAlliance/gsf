' 07-26-06

Public Interface ICommunicationClient

    ''' <summary>
    ''' Occurs when the client is trying to connect to the server.
    ''' </summary>
    Event Connecting As EventHandler

    ''' <summary>
    ''' Occurs when connecting of the client to the server has been cancelled.
    ''' </summary>
    Event ConnectingCancelled As EventHandler

    ''' <summary>
    ''' Occurs when an exception is encountered while connecting to the server.
    ''' </summary>
    ''' <param name="ex">The exception that was encountered while connecting to the server.</param>
    Event ConnectingException(ByVal ex As Exception)

    ''' <summary>
    ''' Occurs when the client has successfully connected to the server.
    ''' </summary>
    Event Connected As EventHandler

    ''' <summary>
    ''' Occurs when the client has disconnected from the server.
    ''' </summary>
    Event Disconnected As EventHandler

    ''' <summary>
    ''' Occurs when the client begins sending data to the server.
    ''' </summary>
    ''' <param name="data">The data being sent to the server.</param>
    Event SendDataBegin(ByVal data As Byte())

    ''' <summary>
    ''' Occurs when the client has successfully send data to the server.
    ''' </summary>
    ''' <param name="data">The data sent to the server.</param>
    Event SendDataComplete(ByVal data As Byte())

    ''' <summary>
    ''' Occurs when the client receives data from the server.
    ''' </summary>
    ''' <param name="data">The data that was received from the server.</param>
    Event ReceivedData(ByVal data As Byte())

    ''' <summary>
    ''' Occurs when no data is received from the server after waiting for the specified time.
    ''' </summary>
    Event ReceiveTimedOut As EventHandler

    ''' <summary>
    ''' Gets or sets the data required by the client to connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The data required by the client to connect to the server.</returns>
    Property ConnectionString() As String

    ''' <summary>
    ''' Gets or sets the maximum number of times the client will attempt to connect to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of times the client will attempt to connect to the server.</returns>
    Property MaximumConnectionAttempts() As Integer

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
    ''' <returns>True is the server will do a handshake with the client; otherwise False.</returns>
    Property Handshake() As Boolean

    ''' <summary>
    ''' Gets or sets the passpharse that will be provided to the server for authentication during the handshake 
    ''' process.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The passpharse that will provided to the server for authentication during the handshake process.</returns>
    Property HandshakePassphrase() As String

    ''' <summary>
    ''' Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
    Property ReceiveBufferSize() As Integer

    ''' <summary>
    ''' Gets or sets the time to wait in seconds for data to be received from the server before timing out.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time to wait in seconds for data to be received from the server before timing out.</returns>
    Property ReceiveTimeout() As Integer

    ''' <summary>
    ''' Gets or sets the encryption level to be used for encrypting the data exchanged between the client and 
    ''' server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encryption level to be used for encrypting the data exchanged between the client and server.</returns>
    Property Encryption() As Tva.Security.Cryptography.EncryptLevel

    ''' <summary>
    ''' Gets or sets the compression level to be used for compressing the data exchanged between the client and 
    ''' server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The compression level to be used for compressing the data exchanged between the client and server.</returns>
    Property Compression() As Tva.IO.Compression.CompressLevel

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the client is enabled.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the client is enabled; otherwise False.</returns>
    Property Enabled() As Boolean

    ''' <summary>
    ''' Gets or sets the encoding to be used for the text sent to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The encoding to be used for the text sent to the server.</returns>
    Property TextEncoding() As System.Text.Encoding

    ''' <summary>
    ''' Gets the protocol used by the client for transferring data to and from the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The protocol used by the client for transferring data to and from the server.</returns>
    Property Protocol() As TransportProtocol

    ''' <summary>
    ''' Gets the ID of the server to which the client is connected.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the server to which the client is connected.</returns>
    Property ServerID() As Guid

    ''' <summary>
    ''' Gets the ID of the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ID of the client.</returns>
    Property ClientID() As Guid

    ''' <summary>
    ''' Gets the current instance of communication client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current instance communication client.</returns>
    ReadOnly Property This() As ICommunicationClient

    ''' <summary>
    ''' Gets a boolean value indicating whether the client is currently connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the client is connected; otherwise False.</returns>
    ReadOnly Property IsConnected() As Boolean

    ''' <summary>
    ''' Gets the time in seconds for which the client has been connected to the server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The time in seconds for which the client has been connected to the server.</returns>
    ReadOnly Property ConnectionTime() As Double

    ''' <summary>
    ''' Gets the total number of bytes sent by the client to the server since the connection is established.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The total number of bytes sent by the client to the server since the connection is established.</returns>
    ReadOnly Property TotalBytesSent() As Integer

    ''' <summary>
    ''' Gets the total number of bytes received by the client from the server since the connection is established.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The total number of bytes received by the client from the server since the connection is established.</returns>
    ReadOnly Property TotalBytesReceived() As Integer

    ''' <summary>
    ''' Gets the current status of the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current status of the client.</returns>
    ReadOnly Property Status() As String

    ''' <summary>
    ''' Connects the client to the server.
    ''' </summary>
    Sub Connect()

    ''' <summary>
    ''' Cancels connecting to the server.
    ''' </summary>
    Sub CancelConnect()

    ''' <summary>
    ''' Disconnects the client from the server it is connected to.
    ''' </summary>
    Sub Disconnect()

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="data">The plain-text data that is to be sent to the server.</param>
    Sub Send(ByVal data As String)

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="serializableObject">The serializable object that is to be sent to the server.</param>
    Sub Send(ByVal serializableObject As Object)

    ''' <summary>
    ''' Sends data to the server.
    ''' </summary>
    ''' <param name="data">The binary data that is to be sent to the server.</param>
    Sub Send(ByVal data As Byte())

End Interface
