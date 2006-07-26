' 07-26-06

Public Interface ICommunicationClient

    Event Connecting As EventHandler
    Event ConnectingCancelled As EventHandler
    Event ConnectingException(ByVal ex As Exception)
    Event Connected As EventHandler
    Event Disconnected As EventHandler
    Event SendDataBegin(ByVal data As Byte())
    Event SendDataComplete(ByVal data As Byte())
    Event ReceivedData(ByVal data As Byte())
    Event ReceiveTimedOut As EventHandler

    Property ConnectionString() As String
    Property MaximumConnectionAttempts() As Integer
    Property SecureSession() As Boolean
    Property Handshake() As Boolean
    Property HandshakePassphrase() As String
    Property ReceiveBufferSize() As Integer
    Property ReceiveTimeout() As Integer
    Property Encryption() As Tva.Security.Cryptography.EncryptLevel
    Property Compression() As Tva.IO.Compression.CompressLevel
    Property Enabled() As Boolean
    Property TextEncoding() As System.Text.Encoding
    Property Protocol() As TransportProtocol
    Property ServerID() As Guid
    Property ClientID() As Guid
    ReadOnly Property IsConnected() As Boolean
    ReadOnly Property ConnectionTime() As Double
    ReadOnly Property TotalBytesSent() As Integer
    ReadOnly Property TotalBytesReceived() As Integer
    ReadOnly Property Status() As String

    Sub Connect()
    Sub CancelConnect()
    Sub Disconnect()
    Sub Send(ByVal data As String)
    Sub Send(ByVal serializableObject As Object)
    Sub Send(ByVal data As Byte())

End Interface
