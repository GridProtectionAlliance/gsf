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
Imports Tva.Serialization
Imports Tva.Communication.CommunicationHelper
Imports Tva.Security.Cryptography.Common

Public Class UdpServer

    Private m_payloadAware As Boolean
    Private m_udpServer As StateKeeper(Of Socket)
    Private m_udpClients As Dictionary(Of Guid, StateKeeper(Of IPEndPoint))
    Private m_pendingUdpClients As List(Of IPAddress)
    Private m_configurationData As Dictionary(Of String, String)

    Public Sub New(ByVal configurationString As String)

        MyClass.New()
        MyBase.ConfigurationString = configurationString

    End Sub

    ''' <summary>
    ''' The minimum size of the receive buffer for UDP.
    ''' </summary>
    Public Const MinimumUdpBufferSize As Integer = 512

    ''' <summary>
    ''' The maximum number of bytes that can be sent in a single UDP datagram.
    ''' </summary>
    Public Const MaximumUdpDatagramSize As Integer = 32768

    Public Overrides Property ReceiveBufferSize() As Integer
        Get
            Return MyBase.ReceiveBufferSize
        End Get
        Set(ByVal value As Integer)
            If value >= UdpClient.MinimumUdpBufferSize AndAlso value <= UdpClient.MaximumUdpDatagramSize Then
                MyBase.ReceiveBufferSize = value
            Else
                Throw New ArgumentOutOfRangeException("ReceiveBufferSize", "ReceiveBufferSize for UDP must be between " & UdpClient.MinimumUdpBufferSize & " and " & UdpClient.MaximumUdpDatagramSize & ".")
            End If
        End Set
    End Property

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

        If MyBase.Enabled AndAlso Not MyBase.IsRunning AndAlso ValidConfigurationString(MyBase.ConfigurationString) Then
            Try
                m_udpServer = New StateKeeper(Of Socket)()
                m_udpServer.ID = MyBase.ServerID
                m_udpServer.Passphrase = MyBase.HandshakePassphrase
                m_udpServer.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                m_udpServer.Client.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationData("port"))))

                Dim recevingThread As New Thread(AddressOf ReceiveClientData)
                recevingThread.Start()

                OnServerStarted(EventArgs.Empty)

                If Not MyBase.Handshake Then
                    For Each clientNameOrAddress As String In m_configurationData("clients").Replace(" ", "").Split(","c)
                        Try
                            Dim udpClient As New StateKeeper(Of IPEndPoint)()
                            udpClient.ID = Guid.NewGuid()
                            udpClient.Client = GetIpEndPoint(clientNameOrAddress, Convert.ToInt32(m_configurationData("port")))
                            m_udpClients.Add(udpClient.ID, udpClient)

                            OnClientConnected(New IdentifiableSourceEventArgs(udpClient.ID))
                        Catch ex As Exception
                            ' Ignore invalid client entries.
                        End Try
                    Next
                End If
            Catch ex As Exception
                OnServerStartupException(New ExceptionEventArgs(ex))
            End Try
        End If

    End Sub

    Public Overrides Sub [Stop]()

        If MyBase.Enabled AndAlso MyBase.IsRunning Then
            For Each udpClientID As Guid In m_udpClients.Keys
                If MyBase.Handshake Then
                    Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(udpClientID)))
                    If m_payloadAware Then goodbye = PayloadAwareHelper.AddPayloadHeader(goodbye)
                    m_udpServer.Client.SendTo(goodbye, m_udpClients(udpClientID).Client)
                End If

                OnClientDisconnected(New IdentifiableSourceEventArgs(udpClientID))
            Next
            m_udpClients.Clear()

            If m_udpServer IsNot Nothing AndAlso m_udpServer.Client IsNot Nothing Then
                m_udpServer.Client.Close()
            End If
        End If

    End Sub

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data As Byte())

        If MyBase.Enabled AndAlso MyBase.IsRunning Then
            Dim udpClient As StateKeeper(Of IPEndPoint) = Nothing
            If m_udpClients.TryGetValue(clientID, udpClient) Then
                If MyBase.SecureSession Then data = EncryptData(data, udpClient.Passphrase, MyBase.Encryption)
                If m_payloadAware Then data = PayloadAwareHelper.AddPayloadHeader(data)

                Dim toIndex As Integer = 0
                Dim datagramSize As Integer = MyBase.ReceiveBufferSize
                If data.Length > datagramSize Then toIndex = data.Length - 1
                For i As Integer = 0 To toIndex Step datagramSize
                    ' Last or the only datagram in the series.
                    If data.Length - i < datagramSize Then datagramSize = data.Length - i

                    ' We'll send the data asynchronously for better performance.
                    m_udpServer.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client, Nothing, Nothing)
                Next
            Else
                Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
            End If
        End If

    End Sub

    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

        If Not String.IsNullOrEmpty(configurationString) Then
            m_configurationData = Tva.Text.Common.ParseKeyValuePairs(configurationString)
            ' Even though clients value is required, it will be ignored if Handshake is enabled.
            If m_configurationData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_configurationData("port")) AndAlso _
                    m_configurationData.ContainsKey("clients") AndAlso _
                    m_configurationData("clients").Split(","c).Length > 0 Then
                ' The configuration string must always contain the following:
                ' >> port - Port number on which the server will be listening for incoming data.
                ' >> clients - A list of clients the server will be sending data to.
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   Port=Local port number; Clients=Client name or IP, ..., Client name or IP")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException("ConfigurationString")
        End If

    End Function

    Private Sub ReceiveClientData()

        Try
            With m_udpServer
                Dim bytesReceived As Integer = 0
                Dim clientEndPoint As EndPoint = New IPEndPoint(IPAddress.Any, 0)
                If ((MyBase.ReceiveRawDataFunction IsNot Nothing) OrElse _
                        (MyBase.ReceiveRawDataFunction Is Nothing AndAlso Not m_payloadAware)) Then
                    .DataBuffer = Tva.Common.CreateArray(Of Byte)(MyBase.ReceiveBufferSize)

                    Do While True
                        bytesReceived = .Client.ReceiveFrom(.DataBuffer, 0, .DataBuffer.Length, SocketFlags.None, clientEndPoint)

                        If m_receiveRawDataFunction IsNot Nothing Then
                            m_receiveRawDataFunction(.DataBuffer, 0, bytesReceived)
                        Else
                            ProcessReceivedClientData(Tva.IO.Common.CopyBuffer(.DataBuffer, 0, bytesReceived), clientEndPoint)
                        End If
                    Loop
                Else
                    Dim payloadSize As Integer = -1
                    Dim totalBytesReceived As Integer = 0
                    Do While True
                        If payloadSize = -1 Then
                            .DataBuffer = Tva.Common.CreateArray(Of Byte)(MyBase.ReceiveBufferSize)
                        End If

                        bytesReceived = .Client.ReceiveFrom(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None, clientEndPoint)

                        If payloadSize = -1 Then
                            payloadSize = PayloadAwareHelper.GetPayloadSize(.DataBuffer)
                            If payloadSize <> -1 AndAlso payloadSize <= CommunicationClientBase.MaximumDataSize Then
                                Dim payload As Byte() = PayloadAwareHelper.GetPayload(.DataBuffer)

                                .DataBuffer = Tva.Common.CreateArray(Of Byte)(payloadSize)
                                Buffer.BlockCopy(payload, 0, .DataBuffer, 0, payload.Length)
                                bytesReceived = payload.Length
                            End If
                        End If

                        totalBytesReceived += bytesReceived
                        If totalBytesReceived = payloadSize Then
                            ' We've received the entire payload.
                            ProcessReceivedClientData(.DataBuffer, clientEndPoint)

                            ' Initialize for receiving the next payload.
                            payloadSize = -1
                            totalBytesReceived = 0
                        End If
                    Loop
                End If
            End With
        Catch ex As Exception

        Finally
            If m_udpServer IsNot Nothing AndAlso m_udpServer.Client IsNot Nothing Then
                m_udpServer.Client.Close()
            End If
            OnServerStopped(EventArgs.Empty)
        End Try

    End Sub

    Private Sub ProcessReceivedClientData(ByVal data As Byte(), ByVal senderEndPoint As EndPoint)

        Dim clientMessage As Object = GetObject(GetActualData(data))
        If clientMessage IsNot Nothing Then
            ' We were able to deserialize the data to an object.
            If TypeOf clientMessage Is HandshakeMessage Then
                Dim connectedClient As HandshakeMessage = DirectCast(clientMessage, HandshakeMessage)
                If connectedClient.ID <> Guid.Empty AndAlso connectedClient.Passphrase = MyBase.HandshakePassphrase Then
                    Dim udpClient As New StateKeeper(Of IPEndPoint)()
                    udpClient.ID = connectedClient.ID
                    udpClient.Client = CType(senderEndPoint, IPEndPoint)
                    If MyBase.SecureSession Then udpClient.Passphrase = GenerateKey()

                    Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(MyBase.ServerID, udpClient.Passphrase)))
                    If m_payloadAware Then myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)
                    m_udpServer.Client.SendTo(myInfo, udpClient.Client)

                    SyncLock m_udpClients
                        m_udpClients.Add(udpClient.ID, udpClient)
                    End SyncLock

                    OnClientConnected(New IdentifiableSourceEventArgs(udpClient.ID))
                End If
            ElseIf TypeOf clientMessage Is GoodbyeMessage Then
                Dim disconnectedClient As GoodbyeMessage = DirectCast(clientMessage, GoodbyeMessage)

                SyncLock m_udpClients
                    m_udpClients.Remove(disconnectedClient.ID)
                End SyncLock

                OnClientDisconnected(New IdentifiableSourceEventArgs(disconnectedClient.ID))
            End If
        Else
            Dim senderIPEndPoint As IPEndPoint = CType(senderEndPoint, IPEndPoint)
            If Not senderIPEndPoint.Equals(GetIpEndPoint(Dns.GetHostName(), Convert.ToInt32(m_configurationData("port")))) Then
                For Each udpClient As StateKeeper(Of IPEndPoint) In m_udpClients.Values
                    If senderIPEndPoint.Equals(udpClient.Client) Then
                        If MyBase.SecureSession Then data = DecryptData(data, udpClient.Passphrase, MyBase.Encryption)

                        OnReceivedClientData(New DataEventArgs(udpClient.ID, data))
                        Exit Sub
                    End If
                Next
                OnReceivedClientData(New DataEventArgs(Guid.Empty, data))
            End If
        End If

    End Sub

    'With m_udpServer
    '    Try
    '        Dim received As Integer
    '        Dim length As Integer
    '        Dim dataBuffer As Byte() = Nothing
    '        Dim totalBytesReceived As Integer                
    '        Dim clientEndPoint As EndPoint = New IPEndPoint(IPAddress.Any, 0) ' Used to capture the client's identity.

    '        If m_receiveRawDataFunction Is Nothing Then
    '            length = MaximumUdpPacketSize
    '        Else
    '            length = m_buffer.Length
    '        End If

    '        ' Enter data read loop, socket receive will block thread while waiting for data from the client.
    '        Do While True
    '            ' Retrieve data from the UDP socket
    '            received = .Client.ReceiveFrom(m_buffer, 0, length, SocketFlags.None, clientEndPoint)

    '            ' Post raw data to real-time function delegate if defined - this bypasses all other activity
    '            If m_receiveRawDataFunction IsNot Nothing Then
    '                m_receiveRawDataFunction(m_buffer, 0, received)
    '                Continue Do
    '            End If

    '            If dataBuffer Is Nothing Then
    '                ' By default we'll prepare to receive a maximum of MaximumPacketSize from the server.
    '                dataBuffer = CreateArray(Of Byte)(length)
    '                totalBytesReceived = 0
    '            End If

    '            ' Copy data into local cumulative buffer to start the unpacking process and eventually make the data available via event
    '            Buffer.BlockCopy(m_buffer, 0, dataBuffer, totalBytesReceived, dataBuffer.Length - totalBytesReceived)
    '            totalBytesReceived += received

    '            If m_payloadAware Then
    '                If .PayloadSize = -1 Then
    '                    ' We have not yet received the payload size. 
    '                    If HasBeginMarker(dataBuffer) Then
    '                        ' This packet has the payload size.
    '                        .PayloadSize = BitConverter.ToInt32(dataBuffer, m_packetBeginMarker.Length())
    '                        ' We'll save the payload received in this packet.
    '                        Dim tempBuffer As Byte() = CreateArray(Of Byte)(IIf(.PayloadSize < MaximumUdpPacketSize - 8, .PayloadSize, MaximumUdpPacketSize - 8))
    '                        Buffer.BlockCopy(dataBuffer, 8, tempBuffer, 0, tempBuffer.Length)
    '                        dataBuffer = CreateArray(Of Byte)(.PayloadSize)
    '                        totalBytesReceived = 0
    '                        Buffer.BlockCopy(tempBuffer, 0, dataBuffer, 0, tempBuffer.Length)
    '                        totalBytesReceived = tempBuffer.Length
    '                    Else
    '                        ' We'll wait for a packet that has payload size.
    '                        Continue Do
    '                    End If
    '                End If
    '                If totalBytesReceived < .PayloadSize Then
    '                    Continue Do
    '                End If
    '            Else
    '                dataBuffer = CopyBuffer(dataBuffer, 0, totalBytesReceived)
    '                totalBytesReceived = 0
    '            End If

    '            Dim clientMessage As Object = GetObject(GetActualData(dataBuffer))
    '            If clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is HandshakeMessage Then
    '                ' The client sent a handshake message.
    '                Dim clientInfo As HandshakeMessage = DirectCast(clientMessage, HandshakeMessage)
    '                If clientInfo.Passphrase = HandshakePassphrase Then ' Authentication successful.
    '                    Dim udpClient As New StateKeeper(Of IPEndPoint)
    '                    udpClient.Client = CType(clientEndPoint, IPEndPoint)
    '                    udpClient.ID = clientInfo.ID()
    '                    udpClient.Passphrase = clientInfo.Passphrase()
    '                    If SecureSession() Then udpClient.Passphrase = GenerateKey()

    '                    Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(.ID, udpClient.Passphrase)))
    '                    If m_payloadAware Then myInfo = AddBeginHeader(myInfo)
    '                    .Client.SendTo(myInfo, udpClient.Client)

    '                    m_pendingUdpClients.Remove(udpClient.Client.Address())
    '                    m_udpClients.Add(udpClient.ID, udpClient)
    '                    OnClientConnected(New IdentifiableSourceEventArgs(udpClient.ID))
    '                End If
    '            ElseIf clientMessage IsNot Nothing AndAlso TypeOf clientMessage Is GoodbyeMessage Then
    '                ' The client sent a goodbye message.
    '                Dim clientInfo As GoodbyeMessage = DirectCast(clientMessage, GoodbyeMessage)
    '                If m_udpClients.ContainsKey(clientInfo.ID()) Then
    '                    m_udpClients.Remove(clientInfo.ID())
    '                    OnClientDisconnected(New IdentifiableSourceEventArgs(clientInfo.ID()))
    '                End If
    '            Else
    '                Dim remoteEP As IPEndPoint = CType(clientEndPoint, IPEndPoint)
    '                If Not remoteEP.Equals(GetIpEndPoint(Dns.GetHostName(), Convert.ToInt32(m_configurationData("port")))) Then
    '                    ' We're interested in data received from clients (other machines) and we'll ignore 
    '                    ' all the data broadcasted by the server.
    '                    Dim clientID As Guid = Guid.Empty
    '                    For Each id As Guid In m_udpClients.Keys()
    '                        If m_udpClients(id).Client.Address.Equals(remoteEP.Address()) AndAlso _
    '                                m_udpClients(id).Client.Port() = remoteEP.Port() Then
    '                            clientID = id
    '                            Exit For
    '                        End If
    '                    Next

    '                    If SecureSession() AndAlso clientID <> Guid.Empty Then
    '                        dataBuffer = DecryptData(dataBuffer, m_udpClients(clientID).Passphrase, Encryption())
    '                        totalBytesReceived = 0
    '                    End If

    '                    OnReceivedClientData(New DataEventArgs(clientID, dataBuffer))
    '                End If
    '            End If

    '            dataBuffer = Nothing
    '            totalBytesReceived = 0
    '            length = MaximumUdpPacketSize
    '            .PayloadSize = -1
    '        Loop
    '    Catch ex As Exception

    '    Finally
    '        If m_udpServer IsNot Nothing AndAlso .Client IsNot Nothing Then
    '            .Client.Close()
    '            m_udpServer = Nothing
    '        End If
    '    End Try
    'End With

End Class
