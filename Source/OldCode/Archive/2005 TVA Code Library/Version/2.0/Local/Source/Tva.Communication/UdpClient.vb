'*******************************************************************************************************
'  Tva.Communication.UdpClient.vb - UDP-based communication client
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

' PACKET STRUCTURE
' ================
' When the payload to be transmitted between the client and server exceeds the MaximumPacketSize, it is divided
' into a series of packets, where size of each packet is no greater than the MaximumPacketSize.
' > PacketAware = True
'   ------------------------------------------------------------
'   |   4 Byte Marker   |4 Byte Payload Size|   Actual Payload
'   --------------------------------------------------------------
' > PacketAware = False
'   ------------------------------------------------------------
'   |                        Actual Payload
'   --------------------------------------------------------------

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.IO.Common
Imports Tva.Serialization
Imports Tva.Communication.CommunicationHelper
Imports Tva.Communication.Common

Public Class UdpClient

    Private m_payloadAware As Boolean
    Private m_udpServer As IPEndPoint
    Private m_udpClient As StateKeeper(Of Socket)
    Private m_connectionData As Dictionary(Of String, String)
    Private m_receivingThread As Thread
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    Public Sub New(ByVal connectionString As String)
        MyClass.New()
        MyBase.ConnectionString = connectionString
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

    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() AndAlso ValidConnectionString(ConnectionString()) Then
            ' Initialize the server endpoint that will be used when sending data to the server.
            Dim server As String = "localhost"
            Dim remotePort As Integer = 0
            If m_connectionData.ContainsKey("server") Then server = m_connectionData("server")
            If m_connectionData.ContainsKey("remoteport") Then remotePort = Convert.ToInt32(m_connectionData("remoteport"))
            m_udpServer = GetIpEndPoint(server, remotePort)

            ' Use the specified port only if handshaking is disabled otherwise let the system pick a port for us.
            Dim localPort As Integer = 0
            If Not Handshake() Then localPort = Convert.ToInt32(m_connectionData("localport"))

            ' Initialize the client .
            m_udpClient = New StateKeeper(Of Socket)
            m_udpClient.ID = ClientID()
            m_udpClient.Passphrase = HandshakePassphrase()
            m_udpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            m_udpClient.Client.Bind(New IPEndPoint(IPAddress.Any, localPort))
            If ReceiveTimeout() <> -1 Then m_udpClient.Client.ReceiveTimeout = ReceiveTimeout() * 1000

            ' Start listening for data from the server on a seperate thread.
            m_receivingThread = New Thread(AddressOf ReceiveServerData)
            m_receivingThread.Start()
        End If

    End Sub

    Public Overrides Sub CancelConnect()

        If m_receivingThread IsNot Nothing AndAlso Not IsConnected() Then
            m_receivingThread.Abort()
            OnConnectingCancelled(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub Disconnect()

        CancelConnect()

        If Enabled() AndAlso IsConnected() AndAlso _
                m_udpClient IsNot Nothing AndAlso m_udpClient.Client IsNot Nothing Then
            If Handshake() Then
                ' Handshaking is enabled (making the session connectionful), so send a goodbye message to 
                ' the server indicating that the session has ended.
                Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(m_udpClient.ID)))
                If m_payloadAware Then goodbye = AddPacketHeader(goodbye)
                m_udpClient.Client.SendTo(goodbye, m_udpServer)
            End If

            m_udpClient.Client.Close()
        End If
    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If Enabled() AndAlso IsConnected() Then
            If SecureSession() Then data = EncryptData(data, m_udpClient.Passphrase, Encryption())
            If m_payloadAware Then data = AddPacketHeader(data)

            ' Since we can only send MaximumPacketSize bytes in a given packet, we may need to break the actual 
            ' packet into a series of packets if the packet's size exceed the MaximumPacketSize.
            OnSendDataBegin(data)
            For i As Integer = 0 To IIf(data.Length() > MaximumUdpPacketSize, data.Length() - 1, 0) Step MaximumUdpPacketSize
                Dim packetSize As Integer = MaximumUdpPacketSize
                If data.Length() - i < MaximumUdpPacketSize Then packetSize = data.Length() - i ' Last or the only packet in the series.
                m_udpClient.Client.BeginSendTo(data, i, packetSize, SocketFlags.None, m_udpServer, Nothing, Nothing)
            Next
            OnSendDataComplete(data)
        End If

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("localport") AndAlso _
                    ValidPortNumber(m_connectionData("localport")) Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Server=[Server name or IP]; RemotePort=[Server port number]; LocalPort=[Local port number]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    Private Sub ReceiveServerData()

        With m_udpClient
            Try
                Dim connectionAttempts As Integer = 0
                Dim received As Integer
                Dim length As Integer
                Dim dataBuffer As Byte() = Nothing
                Dim totalBytesReceived As Integer

                ' Enter data read loop, socket receive will block thread while waiting for data from the server.
                Do While True
                    If Not IsConnected() Then
                        OnConnecting(EventArgs.Empty)
                        If Handshake() Then
                            ' Handshaking is required, so we'll send our information to the server.
                            Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(.ID, .Passphrase)))
                            If m_payloadAware Then myInfo = AddPacketHeader(myInfo)
                            .Client.SendTo(myInfo, m_udpServer)
                        Else
                            OnConnected(EventArgs.Empty)
                        End If
                    End If

                    If m_receiveRawDataFunction Is Nothing Then
                        length = MaximumUdpPacketSize
                    Else
                        length = m_buffer.Length
                    End If

                    Try
                        ' Retrieve data from th4e UDP socket
                        received += .Client.ReceiveFrom(m_buffer, 0, length, SocketFlags.None, CType(m_udpServer, EndPoint))

                        ' Post raw data to real-time function delegate if defined - this bypasses all other activity
                        If m_receiveRawDataFunction IsNot Nothing Then
                            m_receiveRawDataFunction(m_buffer, 0, received)
                            Continue Do
                        End If
                    Catch ex As SocketException
                        If ex.SocketErrorCode = SocketError.TimedOut Then
                            .Client.Blocking = True
                            OnReceiveTimedOut(EventArgs.Empty)
                            Continue Do
                        ElseIf ex.SocketErrorCode = SocketError.ConnectionReset Then
                            If Not IsConnected() Then
                                OnConnectingException(ex)
                                connectionAttempts += 1
                                If MaximumConnectionAttempts() = -1 OrElse _
                                        connectionAttempts < MaximumConnectionAttempts() Then
                                    Continue Do
                                Else
                                    Exit Do
                                End If
                            End If
                        End If
                        Throw
                    Catch ex As Exception
                        Throw
                    End Try

                    ' By default we'll prepare to receive a maximum of MaximumUdpPacketSize from the server.
                    If dataBuffer Is Nothing Then
                        dataBuffer = CreateArray(Of Byte)(length)
                        totalBytesReceived = 0
                    End If

                    ' Copy data into local cumulative buffer to start the unpacking process and eventually make the data available via event
                    Buffer.BlockCopy(m_buffer, 0, dataBuffer, totalBytesReceived, received)
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
                                Buffer.BlockCopy(tempBuffer, 0, dataBuffer, 0, tempBuffer.Length)
                                totalBytesReceived = tempBuffer.Length
                                length = dataBuffer.Length
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

                    If ServerID = Guid.Empty AndAlso Handshake Then
                        Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(dataBuffer))
                        If serverInfo IsNot Nothing Then
                            ServerID = serverInfo.ID
                            .Passphrase = serverInfo.Passphrase
                            OnConnected(EventArgs.Empty)
                        End If
                    Else
                        If Handshake AndAlso GetObject(Of GoodbyeMessage)(GetActualData(dataBuffer)) IsNot Nothing Then
                            Exit Do
                        End If
                        If SecureSession Then
                            dataBuffer = DecryptData(dataBuffer, .Passphrase, Encryption)
                            totalBytesReceived = 0
                        End If

                        OnReceivedData(dataBuffer)
                    End If

                    dataBuffer = Nothing
                    totalBytesReceived = 0
                    length = MaximumUdpPacketSize
                    .PacketSize = -1
                Loop
            Catch ex As Exception

            Finally
                If m_udpClient IsNot Nothing AndAlso .Client IsNot Nothing Then
                    .Client.Close()
                    .Client = Nothing
                    m_receivingThread = Nothing
                    If IsConnected() Then OnDisconnected(EventArgs.Empty)
                End If
            End Try
        End With

    End Sub

    Private Function AddPacketHeader(ByVal packet As Byte()) As Byte()

        Dim result As Byte() = CreateArray(Of Byte)(packet.Length() + 8)
        ' Prepend the packet marker.
        Buffer.BlockCopy(m_packetBeginMarker, 0, result, 0, 4)
        ' Prepend the packet size after the packet marker.
        Buffer.BlockCopy(BitConverter.GetBytes(packet.Length()), 0, result, 4, 4)
        ' Append the payload after the header.
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
