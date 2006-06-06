' 06-01-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets

Namespace Data.Transport

    Public MustInherit Class ClientBase

        Private m_connectionString As String
        Private m_protocol As TransportProtocol
        Private m_enabled As Boolean

        Public Event ReceivedData(ByVal data() As Byte)

        'Public Sub New(ByVal connectionString As String)
        '    MyClass.New()
        '    m_connectionString = connectionString
        'End Sub

        Public Property ConnectionString() As String
            Get
                Return m_connectionString
            End Get
            Set(ByVal value As String)
                m_connectionString = value
            End Set
        End Property

        Public Property Protocol() As TransportProtocol
            Get
                Return m_protocol
            End Get
            Set(ByVal value As TransportProtocol)
                m_protocol = value
            End Set
        End Property

        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                m_enabled = value
            End Set
        End Property

        Public ReadOnly Property Status() As String
            Get
                With New StringBuilder()
                    Return .ToString()
                End With
            End Get
        End Property

        Public MustOverride Sub Connect()
        Public MustOverride Sub Disconnect()
        Public MustOverride Sub Send(ByVal data() As Byte)

        Protected Function GetIpEndPoint(ByVal hostNameOrAddress As String, ByVal port As Integer) As IPEndPoint

            ' SocketException will be thrown is the host is not found.
            Return New IPEndPoint(Dns.GetHostEntry(hostNameorAddress).AddressList(0), port)

        End Function

    End Class

End Namespace