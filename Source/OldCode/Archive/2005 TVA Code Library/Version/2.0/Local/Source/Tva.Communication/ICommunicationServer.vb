' 07-26-06

Public Interface ICommunicationServer

    Event ServerStarted As EventHandler
    Event ServerStopped As EventHandler
    Event ClientConnected(ByVal clientID As Guid)
    Event ClientDisconnected(ByVal clientID As Guid)
    Event ReceivedClientData(ByVal clientID As Guid, ByVal data As Byte())

    Property ConfigurationString() As String
    Property MaximumClients() As Integer
    Property SecureSession() As Boolean
    Property Handshake() As Boolean
    Property HandshakePassphrase() As String
    Property ReceiveBufferSize() As Integer
    Property Encryption() As Tva.Security.Cryptography.EncryptLevel
    Property Compression() As Tva.IO.Compression.CompressLevel
    Property Enabled() As Boolean
    Property TextEncoding() As System.Text.Encoding
    Property Protocol() As TransportProtocol
    ReadOnly Property ServerID() As Guid
    ReadOnly Property ClientIDs() As List(Of Guid)
    ReadOnly Property IsRunning() As Boolean
    ReadOnly Property RunTime() As Double
    ReadOnly Property Status() As String

    Sub Start()
    Sub [Stop]()
    Sub SendTo(ByVal clientID As Guid, ByVal serializableObject As Object)
    Sub SendTo(ByVal clientID As Guid, ByVal data As Byte())
    Sub Multicast(ByVal data As String)
    Sub Multicast(ByVal serializableObject As Object)
    Sub Multicast(ByVal data As Byte())

End Interface
