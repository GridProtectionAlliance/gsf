'*******************************************************************************************************
'  Tva.Communication.UdpServer.vb - UDP-based communication server
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
'  07/06/2006 - Pinal C. Patel
'       Original version of source code generated
'  09/06/2006 - J. Ritchie Carroll
'       Added bypass optimizations for high-speed socket access
'
'*******************************************************************************************************

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.IO.Common
Imports Tva.Serialization
Imports Tva.Communication.CommunicationHelper
Imports Tva.Security.Cryptography.Common
Imports Tva.Communication.Common

Public Class UdpServer

    Private m_payloadAware As Boolean
    Private m_udpServer As StateKeeper(Of Socket)
    Private m_udpClients As Dictionary(Of Guid, StateKeeper(Of IPEndPoint))
    Private m_pendingUdpClients As List(Of IPAddress)
    Private m_configurationData As Dictionary(Of String, String)
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Public Sub New(ByVal configurationString As String)
        MyClass.New()
        MyBase.ConfigurationString = configurationString
    End Sub

    <Category("Data"), DefaultValue(GetType(Boolean), "False")> _
    Public Property PayloadAware() As Boolean
        Get
            Return m_payloadAware
        End Get
        Set(ByVal value As Boolean)
            m_payloadAware = value
        End Set
    End Property

    Public Overrides Sub Start()

        If Enabled() AndAlso Not IsRunning() AndAlso ValidConfigurationString(ConfigurationString()) Then
            Try
                ' Create a UDP server socket and bind it to a local endpoint.
                m_udpServer = New StateKeeper(Of Socket)
                m_udpServer.ID = ServerID()
                m_udpServer.Passphrase = HandshakePassphrase()
                m_udpServer.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                m_udpServer.Client.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationData("port"))))

                ' Process all the clients in the list.
                For Each clientNameOrAddress As String In m_configurationData("clients").Split(","c)
                    clientNameOrAddress = clientNameOrAddress.Trim()

                    Try
                        If Not Handshake() OrElse String.Compare(clientNameOrAddress, IPAddress.Broadcast.ToString()) = 0 Then
                            ' Handshaking with clients is not required or the client is a broadcast address.
                            Dim udpClient As New StateKeeper(Of IPEndPoint)
                            udpClient.ID = Guid.NewGuid()
                            udpClient.Client = GetIpEndPoint(clientNameOrAddress, Convert.ToInt32(m_configurationData("port")))
                            m_udpClients.Add(udpClient.ID, udpClient)
                            OnClientConnected(New IdentifiableSourceEventArgs(udpClient.ID))
                        Else
                            ' Handshaking with the clients is required.
                            m_pendingUdpClients.Add(Dns.GetHostEntry(clientNameOrAddress).AddressList(0))
                        End If
                    Catch ex As Exception
                        ' Ignore invalid client entries.
                    End Try
                Next

                ' Listen for data sent by the clients.
                Dim recevingThread As New Thread(AddressOf ReceiveClientData)
                recevingThread.Start()

                OnServerStarted(EventArgs.Empty)
            Catch ex As Exception
                OnServerStartupException(New ExceptionEventArgs(ex))
            End Try
        End If

    End Sub

    Public Overrides Sub [Stop]()

        If Enabled() AndAlso IsRunning() Then
            For Each udpClientID As Guid In m_udpClients.Keys()
                If Handshake() Then
                    Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(udpClientID)))
                    If m_payloadAware Then goodbye = AddBeginHeader(goodbye)
                    m_udpServer.Client.SendTo(goodbye, m_udpClients(udpClientID).Client)
                End If
                OnClientDisconnected(New IdentifiableSourceEventArgs(udpClientID))
            Next
            m_udpClients.Clear()

            m_pendingUdpClients.Clear()

            If m_udpServer IsNot Nothing Then m_udpServer.Client.Close()
            OnServerStopped(EventArgs.Empty)
        End If

    End Sub

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data As Byte())

        If Enabled() AndAlso IsRunning() Then
            Dim udpClient As StateKeeper(Of IPEndPoint) = Nothing
            If m_udpClients.TryGetValue(clientID, udpClient) Then
                If SecureSession() Then data = EncryptData(data, udpClient.Passphrase, Encryption())
                If m_payloadAware Then data = AddBeginHeader(data)

                For i As Integer = 0 To IIf(data.Length() > MaximumUdpPacketSize, data.Length() - 1, 0) Step MaximumUdpPacketSize
                    Dim packetSize As Integer = MaximumUdpPacketSize
                    If data.Length() - i < MaximumUdpPacketSize Then packetSize = data.Length() - i
                    m_udpServer.Client.BeginSendTo(data, i, packetSize, SocketFlags.None, udpClient.Client, Nothing, Nothing)
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
                    ValidPortNumber(m_configurationData("port")) AndAlso _
                    m_configurationData.ContainsKey("clients") AndAlso _
                    m_configurationData("clients").Split(","c).Length() > 0 Then
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Port=[Port Number]; Clients=[Client name or IP, ..., Client name or IP]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub ReceiveClientData()

        With m_udpServer
            Try
                Dim received As Integer
                Dim length As Integer
                Dim dataBuffer As Byte() = Nothing
                Dim totalBytesReceived As Integer                
                Dim clientEndPoint As EndPoint = New IPEndPoint(IPAddress.Any, 0) ' Used to capture the client's identity.

                If m_receiveRawDataFunction Is Nothing Then
                    length = MaximumUdpPacketSize
                Else
                    length = m_buffer.Length
                End If

                ' Enter data read loop, socket receive will block thread while waiting for data from the client.
                Do While True
                    ' Retrieve data from the UDP socket
                    received = .Client.ReceiveFrom(m_buffer, 0, length, SocketFlags.None, clientEndPoint)

                    ' Post raw data to real-time function delegate if defined - this bypasses all other activity
                    If m_receiveRawDataFunction IsNot Nothing Then
                        m_receiveRawDataFunction(m_buffer, 0, received)
                        Continue Do
                    End If

                    If dataBuffer Is Nothing Then
                        ' By default we'll prepare to receive a maximum of MaximumPacketSize from the server.
                        dataBuffer = CreateArray(Of Byte)(length)
                        totalBytesReceived = 0
                    End If

                    ' Copy data into local cumulative buffer to start the unpacking process and eventually make the data available via event
                    Buffer.BlockCopy(m_buffer, 0, dataBuffer, totalBytesReceived, dataBuffer.Length - totalBytesReceived)
                    totalBytesReceived += received

                    If m_payloadAware Then
                        If .PacketSize = -1 Then
                            ' We have not yet received the payload size. 
                            If HasBeginMarker(dataBuffer) Then
                                ' This packet has the payload size.
                                .PacketSize = BitConverter.ToInt32(dataBuffer, m_packetBeginMarker.Length())
                                ' We'll save the payload received in this packet.
                                Dim tempBuffer As Byte() = CreateArray(Of Byte)(IIf(.PacketSize < MaximumUdpPacketSize - 8, .PacketSize, MaximumUdpPacketSize - 8))
                                Buffer.BlockCopy(dataBuffer, 8, tempBuffer, 0, tempBuffer.Length)
                                dataBuffer = CreateArray(Of Byte)(.PacketSize)
                                totalBytesReceived = 0
                                Buffer.BlockCopy(tempBuffer, 0, dataBuffer, 0, tempBuffer.Length)
                                totalBytesReceived = tempBuffer.Length
                            Else
                                ' We'll wait for a packet that has payload size.
                                Continue Do
                            End If
                        End If
                        If totalBytesReceived < .PacketSize Then
                            Continue Do
                        End If
                    Else
                        dataBuffer = CopyBuffer(dataBuffer, 0, totalBytesReceived)
                        totalBytesReceived = 0
                    End If

                    Dim clientMessage As Object = GetObject(GetActualData(dataBuffer))
                    If clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is HandshakeMessage Then
                        ' The client sent a handshake message.
                        Dim clientInfo As HandshakeMessage = DirectCast(clientMessage, HandshakeMessage)
                        If clientInfo.Passphrase = HandshakePassphrase Then ' Authentication successful.
                            Dim udpClient As New StateKeeper(Of IPEndPoint)
                            udpClient.Client = CType(clientEndPoint, IPEndPoint)
                            udpClient.ID = clientInfo.ID()
                            udpClient.Passphrase = clientInfo.Passphrase()
                            If SecureSession() Then udpClient.Passphrase = GenerateKey()

                            Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(.ID, udpClient.Passphrase)))
                            If m_payloadAware Then myInfo = AddBeginHeader(myInfo)
                            .Client.SendTo(myInfo, udpClient.Client)

                            m_pendingUdpClients.Remove(udpClient.Client.Address())
                            m_udpClients.Add(udpClient.ID, udpClient)
                            OnClientConnected(New IdentifiableSourceEventArgs(udpClient.ID))
                        End If
                    ElseIf clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is GoodbyeMessage Then
                        ' The client sent a goodbye message.
                        Dim clientInfo As GoodbyeMessage = DirectCast(clientMessage, GoodbyeMessage)
                        If m_udpClients.ContainsKey(clientInfo.ID()) Then
                            m_udpClients.Remove(clientInfo.ID())
                            OnClientDisconnected(New IdentifiableSourceEventArgs(clientInfo.ID()))
                        End If
                    Else
                        Dim remoteEP As IPEndPoint = CType(clientEndPoint, IPEndPoint)
                        If Not remoteEP.Equals(GetIpEndPoint(Dns.GetHostName(), Convert.ToInt32(m_configurationData("port")))) Then
                            ' We're interested in data received from clients (other machines) and we'll ignore 
                            ' all the data broadcasted by the server.
                            Dim clientID As Guid = Guid.Empty
                            For Each id As Guid In m_udpClients.Keys()
                                If m_udpClients(id).Client.Address.Equals(remoteEP.Address()) AndAlso _
                                        m_udpClients(id).Client.Port() = remoteEP.Port() Then
                                    clientID = id
                                    Exit For
                                End If
                            Next

                            If SecureSession() AndAlso clientID <> Guid.Empty Then
                                dataBuffer = DecryptData(dataBuffer, m_udpClients(clientID).Passphrase, Encryption())
                                totalBytesReceived = 0
                            End If

                            OnReceivedClientData(New DataEventArgs(clientID, dataBuffer))
                        End If
                    End If

                    dataBuffer = Nothing
                    totalBytesReceived = 0
                    length = MaximumUdpPacketSize
                    .PacketSize = -1
                Loop
            Catch ex As Exception

            Finally
                If m_udpServer IsNot Nothing AndAlso .Client IsNot Nothing Then
                    .Client.Close()
                    m_udpServer = Nothing
                End If
            End Try
        End With

    End Sub

    Private Function AddBeginHeader(ByVal packet As Byte()) As Byte()

        Dim result As Byte() = CreateArray(Of Byte)(packet.Length() + 8)
        Buffer.BlockCopy(m_packetBeginMarker, 0, result, 0, 4)
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Length()), 0, result, 4, 4)
        Buffer.BlockCopy(packet, 0, result, 8, packet.Length())

        Return result

    End Function

    Private Function HasBeginMarker(ByVal packet As Byte()) As Boolean

        For i As Integer = 0 To m_packetBeginMarker.Length() - 1
            If packet(i) <> m_packetBeginMarker(i) Then Return False
        Next
        Return True

    End Function

End Class
