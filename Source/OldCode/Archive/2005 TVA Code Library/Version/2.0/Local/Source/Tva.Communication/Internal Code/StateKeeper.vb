' 06-16-06

Imports System.Net.Sockets

' JRC: Class properties converted to public members for optimization...
Friend Class StateKeeper(Of T)

    Public Client As T
    Public ID As Guid
    Public Passphrase As String
    Public PacketSize As Integer

    Public Sub New()

        PacketSize = -1

    End Sub

    Public ReadOnly Property This() As StateKeeper(Of T)
        Get
            Return Me
        End Get
    End Property

#Region " Old Code "

    'Public DataBuffer As Byte()
    'Public BytesReceived As Integer

    'Public Property ID() As Guid
    '    Get
    '        Return m_id
    '    End Get
    '    Set(ByVal value As Guid)
    '        m_id = value
    '    End Set
    'End Property

    'Public Property Client() As T
    '    Get
    '        Return m_client
    '    End Get
    '    Set(ByVal value As T)
    '        m_client = value
    '    End Set
    'End Property

    'Public Property DataBuffer() As Byte()
    '    Get
    '        Return m_dataBuffer
    '    End Get
    '    Set(ByVal value As Byte())
    '        m_dataBuffer = value
    '        m_bytesReceived = 0
    '    End Set
    'End Property

    'Public Property Passphrase() As String
    '    Get
    '        Return m_passphrase
    '    End Get
    '    Set(ByVal value As String)
    '        m_passphrase = value
    '    End Set
    'End Property

    'Public Property BytesReceived() As Integer
    '    Get
    '        Return m_bytesReceived
    '    End Get
    '    Set(ByVal value As Integer)
    '        m_bytesReceived = value
    '    End Set
    'End Property

    'Public Property PacketSize() As Integer
    '    Get
    '        Return m_packetSize
    '    End Get
    '    Set(ByVal value As Integer)
    '        m_packetSize = value
    '    End Set
    'End Property

#End Region

End Class