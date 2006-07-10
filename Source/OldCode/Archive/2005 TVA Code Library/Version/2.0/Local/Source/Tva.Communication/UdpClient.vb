' 07-06-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Communication.SocketHelper

Public Class UdpClient

    Private m_udpClient As StateKeeper(Of Socket)
    Private m_connectionData As IDictionary(Of String, String)
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Private Const MaximumPacketSize As Integer = 32768  ' 32 KB

    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() AndAlso ValidConnectionString(ConnectionString()) Then
            Dim port As Integer = 0
            If Not Handshake() Then port = Convert.ToInt32(m_connectionData("port"))

            m_udpClient = New StateKeeper(Of Socket)
            m_udpClient.ID = ClientID()
            m_udpClient.Passphrase = HandshakePassphrase()
            m_udpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            m_udpClient.Client.Bind(New IPEndPoint(IPAddress.Any, port))

            Dim receivingThread As New Thread(AddressOf ReceiveServerData)
            receivingThread.Start()
        End If

    End Sub

    Public Overrides Sub CancelConnect()

    End Sub

    Public Overrides Sub Disconnect()

    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data() As Byte)

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("server") AndAlso _
                    m_connectionData.ContainsKey("port") AndAlso _
                    Dns.GetHostEntry(Convert.ToString(m_connectionData("server"))) IsNot Nothing AndAlso _
                    ValidPortNumber(Convert.ToString(m_connectionData("port"))) Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Server=<Server name or IP>; Port=<Port Number>")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub ReceiveServerData()

        Try
            Dim serverEP As IPEndPoint = GetIpEndPoint(m_connectionData("server"), Convert.ToInt32(m_connectionData("port")))
            If Handshake() Then
                With m_udpClient
                    .Client.SendTo(GetBytes(New HandshakeMessage(.ID(), .Passphrase())), serverEP)
                End With
            End If
            Do While True
                m_udpClient.DataBuffer = CreateArray(Of Byte)(MaximumPacketSize)
                m_udpClient.Client.ReceiveFrom(m_udpClient.DataBuffer, CType(serverep, EndPoint))
            Loop
        Catch ex As Exception

        End Try

    End Sub

End Class
