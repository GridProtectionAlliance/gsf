' 07-06-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Communication.SocketHelper

Public Class UdpClient

    Private m_packetAware As Boolean
    Private m_udpServer As IPEndPoint
    Private m_udpClient As StateKeeper(Of Socket)
    Private m_connectionData As IDictionary(Of String, String)
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    ''' <summary>
    ''' The maximum number of bytes that can be sent from the client to server in a single packet.
    ''' </summary>
    Private Const MaximumPacketSize As Integer = 32768

    Public Property PacketAware() As Boolean
        Get
            Return m_packetAware
        End Get
        Set(ByVal value As Boolean)
            m_packetAware = value
        End Set
    End Property

    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() AndAlso ValidConnectionString(ConnectionString()) Then
            Dim port As Integer = 0
            If Not Handshake() Then port = Convert.ToInt32(m_connectionData("port"))

            m_udpServer = GetIpEndPoint(m_connectionData("server"), Convert.ToInt32(m_connectionData("port")))

            m_udpClient = New StateKeeper(Of Socket)
            m_udpClient.ID = ClientID()
            m_udpClient.Passphrase = HandshakePassphrase()
            m_udpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            m_udpClient.Client.Bind(New IPEndPoint(IPAddress.Any, port))

            Dim receivingThread As New Thread(AddressOf ReceiveServerData)
            receivingThread.Start()
            OnConnecting(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub CancelConnect()

    End Sub

    Public Overrides Sub Disconnect()

        If Enabled() AndAlso IsConnected() AndAlso _
                m_udpClient IsNot Nothing AndAlso m_udpClient.Client() IsNot Nothing Then
            If Handshake() Then
                Dim bye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(m_udpClient.ID())))
                m_udpClient.Client.SendTo(bye, m_udpServer)
            End If

            m_udpClient.Client.Close()
        End If
    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data() As Byte)

        Throw New NotSupportedException("UDP traffic is unidirectional from server to client.")

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
            If Handshake() Then
                Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(m_udpClient.ID(), m_udpClient.Passphrase())))
                m_udpClient.Client.SendTo(myInfo, m_udpServer)
            Else
                OnConnected(EventArgs.Empty)
            End If

            Do While True
                If m_udpClient.DataBuffer Is Nothing Then
                    m_udpClient.DataBuffer = CreateArray(Of Byte)(MaximumPacketSize)
                End If
                m_udpClient.BytesReceived += m_udpClient.Client.ReceiveFrom(m_udpClient.DataBuffer, m_udpClient.BytesReceived, m_udpClient.DataBuffer.Length - m_udpClient.BytesReceived, SocketFlags.None, CType(m_udpServer, EndPoint))

                If m_packetAware Then
                    If m_udpClient.PacketSize() = -1 Then
                        If HasBeginMarker(m_udpClient.DataBuffer) Then
                            m_udpClient.PacketSize = BitConverter.ToInt32(m_udpClient.DataBuffer(), m_packetBeginMarker.Length())
                            Dim tempBuffer As Byte() = CreateArray(Of Byte)(IIf(m_udpClient.PacketSize < MaximumPacketSize - 8, m_udpClient.PacketSize, MaximumPacketSize - 8))
                            Buffer.BlockCopy(m_udpClient.DataBuffer, 8, tempBuffer, 0, tempBuffer.Length)
                            m_udpClient.DataBuffer = CreateArray(Of Byte)(m_udpClient.PacketSize())
                            Buffer.BlockCopy(tempBuffer, 0, m_udpClient.DataBuffer, 0, tempBuffer.Length)
                            m_udpClient.BytesReceived = tempBuffer.Length
                        Else
                            Continue Do
                        End If
                    End If
                    If m_udpClient.BytesReceived < m_udpClient.PacketSize Then
                        Continue Do
                    End If
                Else

                End If

                If ServerID = Guid.Empty AndAlso Handshake Then
                    Dim hello As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(m_udpClient.DataBuffer))
                    If hello IsNot Nothing Then
                        ServerID = hello.ID
                        m_udpClient.Passphrase = hello.Passphrase
                        OnConnected(EventArgs.Empty)
                    End If
                Else
                    If Handshake AndAlso _
                            GetObject(Of GoodbyeMessage)(GetActualData(m_udpClient.DataBuffer)) IsNot Nothing Then
                        Exit Do
                    End If

                    If SecureSession Then
                        m_udpClient.DataBuffer = DecryptData(m_udpClient.DataBuffer, m_udpClient.Passphrase, Encryption)
                    End If
                    OnReceivedData(m_udpClient.DataBuffer)
                End If

                m_udpClient.DataBuffer = Nothing
                m_udpClient.PacketSize = -1
            Loop
        Catch ex As Exception

        Finally
            If m_udpClient IsNot Nothing AndAlso m_udpClient.Client() IsNot Nothing Then
                m_udpClient.Client.Close()
                m_udpClient.Client = Nothing
                OnDisconnected(EventArgs.Empty)
            End If
        End Try

    End Sub

    Private Function HasBeginMarker(ByVal packet As Byte()) As Boolean

        For i As Integer = 0 To m_packetBeginMarker.Length() - 1
            If packet(i) <> m_packetBeginMarker(i) Then Return False
        Next
        Return True

    End Function

End Class
