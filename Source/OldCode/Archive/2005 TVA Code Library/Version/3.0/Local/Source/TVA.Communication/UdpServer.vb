'*******************************************************************************************************
'  TVA.Communication.UdpServer.vb - UDP-based communication server
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
Imports TVA.Serialization
Imports TVA.Communication.CommunicationHelper
Imports TVA.Security.Cryptography.Common
Imports TVA.Threading

''' <summary>
''' Represents a UDP-based communication server.
''' </summary>
''' <remarks>
''' UDP by nature is a connectionless protocol, but with this implementation of UDP server we can have a 
''' connectionfull session with the server by enabling Handshake. This in-turn enables us to take advantage
''' of SecureSession which otherwise is not possible.
''' </remarks>
<DisplayName("Udp Communication Server")> _
Public Class UdpServer

#Region " Member Declaration "

    Private m_payloadAware As Boolean
    Private m_destinationReachableCheck As Boolean
    Private m_udpServer As StateInfo(Of Socket)
    Private m_udpClients As Dictionary(Of Guid, StateInfo(Of IPEndPoint))
    Private m_configurationData As Dictionary(Of String, String)

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' The minimum size of the receive buffer for UDP.
    ''' </summary>
    Public Const MinimumUdpBufferSize As Integer = 512

    ''' <summary>
    ''' The maximum number of bytes that can be sent in a single UDP datagram.
    ''' </summary>
    Public Const MaximumUdpDatagramSize As Integer = 32768

    ''' <summary>
    ''' Initializes a instance of TVA.Communication.UdpServer with the specified data.
    ''' </summary>
    ''' <param name="configurationString">The configuration string containing the data required for initializing the UDP server.</param>
    Public Sub New(ByVal configurationString As String)

        MyClass.New()
        MyBase.ConfigurationString = configurationString

    End Sub

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the messages that are broken down into multiple datagram 
    ''' for the purpose of transmission while being sent are to be assembled back when received.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the messages that are broken down into multiple datagram for the purpose of transmission while being 
    ''' sent are to be assembled back when received; otherwise False.
    ''' </returns>
    ''' <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
    <Description("Indicates whether the messages that are broken down into multiple datagram for the purpose of transmission are to be assembled back when received. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(GetType(Boolean), "False")> _
    Public Property PayloadAware() As Boolean
        Get
            Return m_payloadAware
        End Get
        Set(ByVal value As Boolean)
            m_payloadAware = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether a test is to be performed to check if the destination 
    ''' endpoint that is to receive data is listening for data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if a test is to be performed to check if the destination endpoint that is to receive data is listening 
    ''' for data; otherwise False.
    ''' </returns>
    <Description("Indicates whether a test is to be performed to check if the destination endpoint that is to receive data is listening for data."), Category("Behavior"), DefaultValue(GetType(Boolean), "False")> _
    Public Property DestinationReachableCheck() As Boolean
        Get
            Return m_destinationReachableCheck
        End Get
        Set(ByVal value As Boolean)
            m_destinationReachableCheck = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the System.Net.IPEndPoint of the server.
    ''' </summary>
    ''' <returns>The System.Net.IPEndPoint of the server.</returns>
    <Browsable(False)> _
    Public ReadOnly Property Server() As IPEndPoint
        Get
            Return CType(m_udpServer.Client.LocalEndPoint, IPEndPoint)
        End Get
    End Property

    ''' <summary>
    ''' Gets the current states of all connected clients which includes the System.Net.IPEndPoint of clients.
    ''' </summary>
    ''' <remarks>
    ''' The current states of all connected clients which includes the System.Net.IPEndPoint of clients.
    ''' </remarks>
    <Browsable(False)> _
    Public ReadOnly Property Clients() As List(Of StateInfo(Of IPEndPoint))
        Get
            Dim clientList As New List(Of StateInfo(Of IPEndPoint))()
            SyncLock m_udpClients
                clientList.AddRange(m_udpClients.Values)
            End SyncLock

            Return clientList
        End Get
    End Property

    ''' <summary>
    ''' Gets the current state of the specified client which includes its System.Net.IPEndPoint.
    ''' </summary>
    ''' <param name="clientID"></param>
    ''' <value></value>
    ''' <returns>
    ''' The current state of the specified client which includes its System.Net.IPEndPoint if the 
    ''' specified client ID is valid (client is connected); otherwise Nothing.
    ''' </returns>
    <Browsable(False)> _
    Public ReadOnly Property Clients(ByVal clientID As Guid) As StateInfo(Of IPEndPoint)
        Get
            Dim client As StateInfo(Of IPEndPoint) = Nothing
            SyncLock m_udpClients
                m_udpClients.TryGetValue(clientID, client)
            End SyncLock

            Return client
        End Get
    End Property

#Region " Overrides "

    ''' <summary>
    ''' Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
    ''' </summary>
    ''' <value>Receive buffer size</value>
    ''' <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
    ''' <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>

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

    Public Overrides Sub Start()

        If Enabled AndAlso Not IsRunning AndAlso ValidConfigurationString(ConfigurationString) Then
            Try
                Dim serverPort As Integer = 0
                If m_configurationData.ContainsKey("port") Then serverPort = Convert.ToInt32(m_configurationData("port"))

                m_udpServer = New StateInfo(Of Socket)()
                m_udpServer.ID = ServerID
                m_udpServer.Passphrase = HandshakePassphrase
                m_udpServer.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                If Handshake Then
                    ' We will listen for data only when Handshake is enabled and a valid port has been specified in 
                    ' the configuration string. We do this in order to keep the server stable and besides that, the 
                    ' main purpose of a UDP server is to serve data in most cases.
                    If serverPort > 0 Then
                        m_udpServer.Client.Bind(New IPEndPoint(IPAddress.Any, serverPort))

#If ThreadTracking Then
                        With New ManagedThread(AddressOf ReceiveClientData)
                            .Name = "TVA.Communication.UdpServer.ReceiveClientData() [" & ServerID.ToString() & "]"
#Else
                        With New Thread(AddressOf ReceiveClientData)
#End If
                            .Start()
                        End With
                    Else
                        Throw New ArgumentException("Server port must be specified in the configuration string.")
                    End If
                End If

                OnServerStarted(EventArgs.Empty)

                If Not Handshake AndAlso m_configurationData.ContainsKey("clients") Then
                    ' We will ignore the client list in configuration string when Handshake is enabled.
                    For Each clientString As String In m_configurationData("clients").Replace(" ", "").Split(","c)
                        Try
                            Dim clientPort As Integer = serverPort
                            Dim clientStringSegments As String() = clientString.Split(":"c)
                            If clientStringSegments.Length = 2 Then
                                clientPort = Convert.ToInt32(clientStringSegments(1))
                            End If

                            Dim udpClient As New StateInfo(Of IPEndPoint)()
                            udpClient.ID = Guid.NewGuid()
                            udpClient.Client = GetIpEndPoint(clientStringSegments(0), clientPort)
                            SyncLock m_udpClients
                                m_udpClients.Add(udpClient.ID, udpClient)
                            End SyncLock

                            OnClientConnected(udpClient.ID)
                        Catch ex As Exception
                            ' Ignore invalid client entries.
                        End Try
                    Next
                End If
            Catch ex As Exception
                OnServerStartupException(ex)
            End Try
        End If

    End Sub

    Public Overrides Sub [Stop]()

        If Enabled AndAlso IsRunning Then
            ' Disconnect all of the clients.
            DisconnectAll()

            ' Stop the server after we've disconnected the clients.
            If m_udpServer IsNot Nothing AndAlso m_udpServer.Client IsNot Nothing Then
                m_udpServer.Client.Close()
            End If

            OnServerStopped(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub DisconnectOne(ByVal clientID As System.Guid)

        Dim udpClient As StateInfo(Of IPEndPoint) = Nothing
        SyncLock m_udpClients
            m_udpClients.TryGetValue(clientID, udpClient)
        End SyncLock

        If udpClient IsNot Nothing Then
            If Handshake Then
                ' Handshake is enabled so we'll notify the client.
                Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(udpClient.ID)))
                If m_payloadAware Then goodbye = PayloadAwareHelper.AddPayloadHeader(goodbye)

                m_udpServer.Client.SendTo(goodbye, udpClient.Client)
            End If

            SyncLock m_udpClients
                m_udpClients.Remove(udpClient.ID)
            End SyncLock
            OnClientDisconnected(udpClient.ID)
        Else
            Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
        End If

    End Sub

    Public Overrides Sub LoadSettings()

        MyBase.LoadSettings()

        Try
            With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                If .Count > 0 Then
                    PayloadAware = .Item("PayloadAware").GetTypedValue(m_payloadAware)
                    DestinationReachableCheck = .Item("DestinationReachableCheck").GetTypedValue(m_destinationReachableCheck)
                End If
            End With
        Catch ex As Exception
            ' We'll encounter exceptions if the settings are not present in the config file.
        End Try

    End Sub

    Public Overrides Sub SaveSettings()

        MyBase.SaveSettings()

        If PersistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    With .Item("PayloadAware", True)
                        .Value = m_payloadAware.ToString()
                        .Description = "True if the messages that are broken down into multiple datagram for the purpose of transmission while being sent are to be assembled back when received; otherwise False."
                    End With
                    With .Item("DestinationReachableCheck", True)
                        .Value = m_destinationReachableCheck.ToString()
                        .Description = "True if a test is to be performed to check if the destination endpoint that is to receive data is listening for data; otherwise False."
                    End With
                End With
                TVA.Configuration.Common.SaveSettings()
            Catch ex As Exception
                ' We might encounter an exception if for some reason the settings cannot be saved to the config file.
            End Try
        End If

    End Sub

#End Region

#End Region

#Region " Code Scope: Protected "

#Region " Overrides "

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data As Byte())

        If Enabled AndAlso IsRunning Then
            Dim udpClient As StateInfo(Of IPEndPoint) = Nothing
            SyncLock m_udpClients
                m_udpClients.TryGetValue(clientID, udpClient)
            End SyncLock

            If udpClient IsNot Nothing Then
                If m_destinationReachableCheck AndAlso Not IsDestinationReachable(udpClient.Client) Then Exit Sub

                If SecureSession Then data = EncryptData(data, udpClient.Passphrase, Encryption)

                If m_payloadAware Then data = PayloadAwareHelper.AddPayloadHeader(data)

                Dim toIndex As Integer = 0
                Dim datagramSize As Integer = ReceiveBufferSize
                If data.Length > datagramSize Then toIndex = data.Length - 1
                For i As Integer = 0 To toIndex Step datagramSize
                    ' Last or the only datagram in the series.
                    If data.Length - i < datagramSize Then datagramSize = data.Length - i

                    ' PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
                    m_udpServer.Client.SendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client)
                    udpClient.LastSendTimestamp = Date.Now
                    '' We'll send the data asynchronously for better performance.
                    'm_udpServer.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, udpClient.Client, Nothing, Nothing)
                Next
            Else
                Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
            End If
        End If

    End Sub

    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

        If Not String.IsNullOrEmpty(configurationString) Then
            m_configurationData = TVA.Text.Common.ParseKeyValuePairs(configurationString)
            If (m_configurationData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_configurationData("port"))) OrElse _
                    (m_configurationData.ContainsKey("clients") AndAlso _
                    Not String.IsNullOrEmpty(m_configurationData("clients"))) Then
                ' The configuration string must contain either of the following:
                ' >> port - Port number on which the server will be listening for incoming data.
                ' OR
                ' >> clients - A list of clients the server will be sending data to.
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .AppendLine()
                    .Append("   [Port=Local port number;] [Clients=Client name or IP[:Port number], ..., Client name or IP[:Port number]]")
                    .AppendLine()
                    .Append("Text between square brackets, [...], is optional.")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException("ConfigurationString")
        End If

    End Function

#End Region

#End Region

#Region " Code Scope: Private "

    Private Sub ReceiveClientData()

        Try
            With m_udpServer
                Dim bytesReceived As Integer = 0
                Dim clientEndPoint As EndPoint = New IPEndPoint(IPAddress.Any, 0)
                If m_receiveRawDataFunction IsNot Nothing OrElse _
                        (m_receiveRawDataFunction Is Nothing AndAlso Not m_payloadAware) Then
                    ' In this section the consumer either wants to receive the datagrams and pass it on to a
                    ' delegate or receive datagrams that don't contain metadata used for re-assembling the
                    ' datagrams into the original message and be notified via events. In either case we can use
                    ' a static buffer that can be used over and over again for receiving datagrams as long as
                    ' the datagrams received are not bigger than the receive buffer.
                    Do While True
                        bytesReceived = .Client.ReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, clientEndPoint)
                        .LastReceiveTimestamp = Date.Now

                        If m_receiveRawDataFunction IsNot Nothing Then
                            m_receiveRawDataFunction(m_buffer, 0, bytesReceived)
                        Else
                            ProcessReceivedClientData(TVA.IO.Common.CopyBuffer(m_buffer, 0, bytesReceived), clientEndPoint)
                        End If
                    Loop
                Else
                    Dim payloadSize As Integer = -1
                    Dim totalBytesReceived As Integer = 0
                    Do While True
                        If payloadSize = -1 Then
                            .DataBuffer = TVA.Common.CreateArray(Of Byte)(ReceiveBufferSize)
                        End If

                        bytesReceived = .Client.ReceiveFrom(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None, clientEndPoint)
                        .LastReceiveTimestamp = Date.Now

                        If payloadSize = -1 Then
                            payloadSize = PayloadAwareHelper.GetPayloadSize(.DataBuffer)
                            If payloadSize <> -1 AndAlso payloadSize <= CommunicationClientBase.MaximumDataSize Then
                                Dim payload As Byte() = PayloadAwareHelper.GetPayload(.DataBuffer)

                                .DataBuffer = TVA.Common.CreateArray(Of Byte)(payloadSize)
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
            ' We don't need to take any action when an exception is encountered.
        Finally
            If m_udpServer IsNot Nothing AndAlso m_udpServer.Client IsNot Nothing Then
                m_udpServer.Client.Close()
            End If
        End Try

    End Sub

    Private Sub ProcessReceivedClientData(ByVal data As Byte(), ByVal senderEndPoint As EndPoint)

        If data.Length = 0 Then Exit Sub

        Dim clientMessage As Object = GetObject(GetActualData(data))
        If clientMessage IsNot Nothing Then
            ' We were able to deserialize the data to an object.
            If TypeOf clientMessage Is HandshakeMessage Then
                Dim connectedClient As HandshakeMessage = DirectCast(clientMessage, HandshakeMessage)
                If connectedClient.ID <> Guid.Empty AndAlso connectedClient.Passphrase = HandshakePassphrase Then
                    Dim udpClient As New StateInfo(Of IPEndPoint)()
                    udpClient.ID = connectedClient.ID
                    udpClient.Client = CType(senderEndPoint, IPEndPoint)
                    If SecureSession Then udpClient.Passphrase = GenerateKey()

                    Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(ServerID, udpClient.Passphrase)))
                    If m_payloadAware Then myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)
                    m_udpServer.Client.SendTo(myInfo, udpClient.Client)

                    SyncLock m_udpClients
                        m_udpClients.Add(udpClient.ID, udpClient)
                    End SyncLock

                    OnClientConnected(udpClient.ID)
                End If
            ElseIf TypeOf clientMessage Is GoodbyeMessage Then
                Dim disconnectedClient As GoodbyeMessage = DirectCast(clientMessage, GoodbyeMessage)

                SyncLock m_udpClients
                    m_udpClients.Remove(disconnectedClient.ID)
                End SyncLock

                OnClientDisconnected(disconnectedClient.ID)
            End If
        Else
            Dim sender As StateInfo(Of IPEndPoint) = Nothing
            Dim senderIPEndPoint As IPEndPoint = CType(senderEndPoint, IPEndPoint)
            If Not senderIPEndPoint.Equals(GetIpEndPoint(Dns.GetHostName(), Convert.ToInt32(m_configurationData("port")))) Then
                ' The data received is not something that we might have broadcasted.
                SyncLock m_udpClients
                    ' So, now we'll find the ID of the client who sent the data.
                    For Each udpClient As StateInfo(Of IPEndPoint) In m_udpClients.Values
                        If senderIPEndPoint.Equals(udpClient.Client) Then
                            sender = udpClient
                            Exit For
                        End If
                    Next
                End SyncLock

                If sender IsNot Nothing Then
                    If SecureSession Then data = DecryptData(data, sender.Passphrase, Encryption)
                    OnReceivedClientData(New IdentifiableItem(Of Guid, Byte())(sender.ID, data))
                Else
                    OnReceivedClientData(New IdentifiableItem(Of Guid, Byte())(Guid.Empty, data))
                End If
            End If
        End If

    End Sub

#End Region

End Class
