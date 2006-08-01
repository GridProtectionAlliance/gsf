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

Public Class UdpClient

    Private m_payloadAware As Boolean
    Private m_udpServer As IPEndPoint
    Private m_udpClient As StateKeeper(Of Socket)
    Private m_connectionData As Dictionary(Of String, String)
    Private m_receivingThread As Thread
    Private m_packetBeginMarker As Byte() = {&HAA, &HBB, &HCC, &HDD}

    ''' <summary>
    ''' The maximum number of bytes that can be sent from the client to server in a single packet.
    ''' </summary>
    Private Const MaximumPacketSize As Integer = 32768

    <Category("Data"), DefaultValue(GetType(Boolean), "True")> _
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
            m_udpServer = GetIpEndPoint(m_connectionData("server"), Convert.ToInt32(m_connectionData("remoteport")))

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
                m_udpClient IsNot Nothing AndAlso m_udpClient.Client() IsNot Nothing Then
            If Handshake() Then
                ' Handshaking is enabled (making the session connectionful), so send a goodbye message to 
                ' the server indicating that the session has ended.
                Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(m_udpClient.ID())))
                If m_payloadAware Then goodbye = AddPacketHeader(goodbye)
                m_udpClient.Client.SendTo(goodbye, m_udpServer)
            End If

            m_udpClient.Client.Close()
        End If
    End Sub

    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If Enabled() AndAlso IsConnected() Then
            If SecureSession() Then data = EncryptData(data, m_udpClient.Passphrase(), Encryption())
            If m_payloadAware Then data = AddPacketHeader(data)

            ' Since we can only send MaximumPacketSize bytes in a given packet, we may need to break the actual 
            ' packet into a series of packets if the packet's size exceed the MaximumPacketSize.
            OnSendDataBegin(data)
            For i As Integer = 0 To IIf(data.Length() > MaximumPacketSize, data.Length() - 1, 0) Step MaximumPacketSize
                Dim packetSize As Integer = MaximumPacketSize
                If data.Length() - i < MaximumPacketSize Then packetSize = data.Length() - i ' Last or the only packet in the series.
                m_udpClient.Client.BeginSendTo(data, i, packetSize, SocketFlags.None, m_udpServer, Nothing, Nothing)
            Next
            OnSendDataComplete(data)
        End If

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("server") AndAlso _
                    Dns.GetHostEntry(m_connectionData("server")) IsNot Nothing AndAlso _
                    m_connectionData.ContainsKey("remoteport") AndAlso _
                    ValidPortNumber(m_connectionData("remoteport")) AndAlso _
                    m_connectionData.ContainsKey("localport") AndAlso _
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

        Try
            Dim connectionAttempts As Integer = 0

            Do While True
                If Not IsConnected() Then
                    OnConnecting(EventArgs.Empty)
                    If Handshake() Then
                        ' Handshaking is required, so we'll send our information to the server.
                        Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(m_udpClient.ID(), m_udpClient.Passphrase())))
                        If m_payloadAware Then myInfo = AddPacketHeader(myInfo)
                        m_udpClient.Client.SendTo(myInfo, m_udpServer)
                    Else
                        OnConnected(EventArgs.Empty)
                    End If
                End If

                If m_udpClient.DataBuffer() Is Nothing Then
                    ' By default we'll prepare to receive a maximum of MaximumPacketSize from the server.
                    m_udpClient.DataBuffer = CreateArray(Of Byte)(MaximumPacketSize)
                End If
                Try
                    m_udpClient.BytesReceived += _
                        m_udpClient.Client.ReceiveFrom(m_udpClient.DataBuffer, m_udpClient.BytesReceived, _
                        m_udpClient.DataBuffer.Length - m_udpClient.BytesReceived, SocketFlags.None, CType(m_udpServer, EndPoint))
                Catch ex As SocketException
                    If ex.SocketErrorCode = SocketError.TimedOut Then
                        m_udpClient.Client.Blocking = True
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

                If m_payloadAware Then
                    If m_udpClient.PacketSize() = -1 Then
                        ' We have not yet received the payload size. 
                        If HasBeginMarker(m_udpClient.DataBuffer) Then
                            ' This packet has the payload size.
                            m_udpClient.PacketSize = BitConverter.ToInt32(m_udpClient.DataBuffer(), m_packetBeginMarker.Length())
                            ' We'll save the payload received in this packet.
                            Dim tempBuffer As Byte() = CreateArray(Of Byte)(IIf(m_udpClient.PacketSize < MaximumPacketSize - 8, m_udpClient.PacketSize, MaximumPacketSize - 8))
                            Buffer.BlockCopy(m_udpClient.DataBuffer, 8, tempBuffer, 0, tempBuffer.Length)
                            m_udpClient.DataBuffer = CreateArray(Of Byte)(m_udpClient.PacketSize())
                            Buffer.BlockCopy(tempBuffer, 0, m_udpClient.DataBuffer, 0, tempBuffer.Length)
                            m_udpClient.BytesReceived = tempBuffer.Length
                        Else
                            ' We'll wait for a packet that has payload size.
                            Continue Do
                        End If
                    End If
                    If m_udpClient.BytesReceived < m_udpClient.PacketSize Then
                        Continue Do
                    End If
                Else
                    m_udpClient.DataBuffer = CopyBuffer(m_udpClient.DataBuffer(), 0, m_udpClient.BytesReceived())
                End If

                If ServerID = Guid.Empty AndAlso Handshake Then
                    Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(m_udpClient.DataBuffer))
                    If serverInfo IsNot Nothing Then
                        ServerID = serverInfo.ID
                        m_udpClient.Passphrase = serverInfo.Passphrase
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
                m_receivingThread = Nothing
                If IsConnected() Then OnDisconnected(EventArgs.Empty)
            End If
        End Try

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
