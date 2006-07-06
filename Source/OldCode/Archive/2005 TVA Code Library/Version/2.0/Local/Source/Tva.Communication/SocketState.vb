' 06-16-06

Imports System.Net.Sockets

Friend Class SocketState

    Private m_socket As Socket
    Private m_socketID As Guid
    Private m_dataBuffer As Byte()
    Private m_passphrase As String
    Private m_bytesReceived As Integer
    Private m_packetSize As Integer

    Public Sub New()
        m_packetSize = -1
    End Sub

    Public Property Socket() As Socket
        Get
            Return m_socket
        End Get
        Set(ByVal value As Socket)
            m_socket = value
        End Set
    End Property

    Public Property SocketID() As Guid
        Get
            Return m_socketID
        End Get
        Set(ByVal value As Guid)
            m_socketID = value
        End Set
    End Property

    Public Property DataBuffer() As Byte()
        Get
            Return m_dataBuffer
        End Get
        Set(ByVal value As Byte())
            m_dataBuffer = value
            m_bytesReceived = 0
        End Set
    End Property

    Public Property Passphrase() As String
        Get
            Return m_passphrase
        End Get
        Set(ByVal value As String)
            m_passphrase = value
        End Set
    End Property

    Public Property BytesReceived() As Integer
        Get
            Return m_bytesReceived
        End Get
        Set(ByVal value As Integer)
            m_bytesReceived = value
        End Set
    End Property

    Public Property PacketSize() As Integer
        Get
            Return m_packetSize
        End Get
        Set(ByVal value As Integer)
            m_packetSize = value
        End Set
    End Property

End Class