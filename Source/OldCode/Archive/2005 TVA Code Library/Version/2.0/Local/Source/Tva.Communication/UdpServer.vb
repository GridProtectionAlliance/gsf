' 07-06-06

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Communication.SocketHelper
Imports Tva.Security.Cryptography.Common

Public Class UdpServer

    Private m_packetAware As Boolean
    Private m_udpServer As Socket
    Private m_udpClients As Dictionary(Of Guid, StateKeeper(Of IPEndPoint))
    Private m_pendingUdpClients As List(Of IPAddress)
    Private m_configurationData As Dictionary(Of String, String)
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    ''' <summary>
    ''' The maximum number of bytes that can be sent from the server to clients in a single packet.
    ''' </summary>
    Private Const MaximumPacketSize As Integer = 32768

    Public Sub New(ByVal configurationString As String)
        MyClass.New()
        MyBase.ConfigurationString = configurationString
    End Sub

    Public Property PacketAware() As Boolean
        Get
            Return m_packetAware
        End Get
        Set(ByVal value As Boolean)
            m_packetAware = value
        End Set
    End Property

    Public Overrides Sub Start()

        If Enabled() AndAlso Not IsRunning() AndAlso ValidConfigurationString(ConfigurationString()) Then
            ' Create a UDP server socket and bind it to a local endpoint.
            m_udpServer = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            m_udpServer.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationData("port"))))

            ' Process all the clients in the list.
            For Each clientNameOrAddress As String In m_configurationData("clients").Split(","c)
                clientNameOrAddress = clientNameOrAddress.Trim()

                Try
                    If Not Handshake() OrElse String.Compare(clientNameOrAddress, IPAddress.Broadcast.ToString()) = 0 Then
                        ' Handshaking with clients is not required or the client is a broadcast address.
                        Dim udpClient As New StateKeeper(Of IPEndPoint)
                        udpClient.ID = Guid.NewGuid()
                        udpClient.Client = GetIpEndPoint(clientNameOrAddress, Convert.ToInt32(m_configurationData("port")))
                        m_udpClients.Add(udpClient.ID(), udpClient)
                        OnClientConnected(udpClient.ID())
                    Else
                        ' Handshaking with the clients is required.
                        m_pendingUdpClients.Add(Dns.GetHostEntry(clientNameOrAddress).AddressList(0))
                    End If
                Catch ex As Exception
                    ' Ignore invalid client entries.
                End Try
            Next

            If Handshake() AndAlso m_pendingUdpClients.Count() > 0 Then
                ' Listen for handshake messages from the listed UDP clients.
                Dim recevingThread As New Thread(AddressOf ReceiveClientData)
                recevingThread.Start()
            End If

            OnServerStarted(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub [Stop]()

        If Enabled() AndAlso IsRunning() Then
            For Each udpClientID As Guid In m_udpClients.Keys()
                If Handshake() Then
                    Dim bye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(udpClientID)))
                    If m_packetAware Then bye = AddPacketHeader(bye)
                    m_udpServer.SendTo(bye, m_udpClients(udpClientID).Client())
                End If
                OnClientDisconnected(udpClientID)
            Next
            m_udpClients.Clear()

            m_pendingUdpClients.Clear()

            If m_udpServer IsNot Nothing Then m_udpServer.Close()
            OnServerStopped(EventArgs.Empty)
        End If

    End Sub

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data() As Byte)

        If Enabled() AndAlso IsRunning() Then
            Dim udpClient As StateKeeper(Of IPEndPoint) = Nothing
            If m_udpClients.TryGetValue(clientID, udpClient) Then
                If SecureSession() Then data = EncryptData(data, udpClient.Passphrase(), Encryption())
                If m_packetAware Then data = AddPacketHeader(data)

                For i As Integer = 0 To IIf(data.Length() > MaximumPacketSize, data.Length() - 1, 0) Step MaximumPacketSize
                    Dim packetSize As Integer = MaximumPacketSize
                    If data.Length() - i < MaximumPacketSize Then packetSize = data.Length() - i
                    m_udpServer.BeginSendTo(data, i, packetSize, SocketFlags.None, udpClient.Client(), Nothing, Nothing)
                Next
            Else
                Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
            End If
        End If

    End Sub

    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

        If Not String.IsNullOrEmpty(configurationString) Then
            m_configurationData = Tva.Text.Common.ParseKeyValuePairs(configurationString)
            If m_configurationData.ContainsKey("port") AndAlso _
                    ValidPortNumber(Convert.ToString(m_configurationData("port"))) AndAlso _
                    m_configurationData.ContainsKey("clients") AndAlso _
                    m_configurationData("clients").Split(","c).Length() > 0 Then
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Port=<Port Number>; Clients=<Client name or IP, ..., Client name or IP>")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub ReceiveClientData()

        Try
            Do While True
                Dim buffer As Byte() = CreateArray(Of Byte)(MaximumPacketSize)
                Dim clientEP As EndPoint = New IPEndPoint(IPAddress.Any, 0) ' Used to capture the client's identity.
                Dim dataLength As Integer = m_udpServer.ReceiveFrom(buffer, clientEP)

                Dim clientMessage As Object = GetObject(GetActualData(buffer))
                If clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is HandshakeMessage Then
                    ' The client sent a handshake message.
                    Dim clientInfo As HandshakeMessage = DirectCast(clientMessage, HandshakeMessage)
                    If clientInfo.Passphrase() = HandshakePassphrase() Then ' Authentication successful.
                        Dim udpClient As New StateKeeper(Of IPEndPoint)
                        udpClient.Client = CType(clientEP, IPEndPoint)
                        udpClient.ID = clientInfo.ID()
                        udpClient.Passphrase = clientInfo.Passphrase()
                        If SecureSession() Then udpClient.Passphrase = GenerateKey()

                        Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(ServerID(), udpClient.Passphrase())))
                        If m_packetAware Then myInfo = AddPacketHeader(myInfo)
                        m_udpServer.SendTo(myinfo, udpClient.Client())

                        m_pendingUdpClients.Remove(udpClient.Client.Address())
                        m_udpClients.Add(udpClient.ID(), udpClient)
                        OnClientConnected(udpClient.ID())
                    End If
                ElseIf clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is GoodbyeMessage Then
                    ' The client sent a goodbye message.
                    Dim clientInfo As GoodbyeMessage = DirectCast(clientMessage, GoodbyeMessage)
                    If m_udpClients.ContainsKey(clientInfo.ID()) Then
                        m_udpClients.Remove(clientInfo.ID())
                        OnClientDisconnected(clientInfo.ID())
                    End If
                End If
            Loop
        Catch ex As Exception

        Finally
            If m_udpServer IsNot Nothing Then
                m_udpServer.Close()
                m_udpServer = Nothing
            End If
        End Try

    End Sub

    Private Function AddPacketHeader(ByVal packet As Byte()) As Byte()

        Dim result As Byte() = CreateArray(Of Byte)(packet.Length() + 8)
        Buffer.BlockCopy(m_packetBeginMarker, 0, result, 0, 4)
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Length()), 0, result, 4, 4)
        Buffer.BlockCopy(packet, 0, result, 8, packet.Length())

        Return result

    End Function

End Class
