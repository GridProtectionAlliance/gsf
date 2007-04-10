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

''' <summary>
''' Represents a UDP-based communication server.
''' </summary>
''' <remarks>
''' UDP by nature is a connectionless protocol, but with this implementation of UDP server we can have a 
''' connectionfull session with the server by enabling Handshake. This in-turn enables us to take advantage
''' of SecureSession which otherwise is not possible.
''' </remarks>
Public Class UdpServer

#Region " Member Declaration "

    Private m_payloadAware As Boolean
    Private m_destinationReachableCheck As Boolean
    Private m_udpServer As StateKeeper(Of Socket)
    Private m_udpClients As Dictionary(Of Guid, StateKeeper(Of IPEndPoint))
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
    ''' Gets or sets the maximum number of bytes that can be received at a time by the server from the clients.
    ''' </summary>
    ''' <value>Receive buffer size</value>
    ''' <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while server is running</exception>
    ''' <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
    ''' <returns>The maximum number of bytes that can be received at a time by the server from the clients.</returns>
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

    Public Overrides Sub Start()

        If MyBase.Enabled AndAlso Not MyBase.IsRunning AndAlso ValidConfigurationString(MyBase.ConfigurationString) Then
            Try
                Dim serverPort As Integer = 0
                If m_configurationData.ContainsKey("port") Then serverPort = Convert.ToInt32(m_configurationData("port"))

                m_udpServer = New StateKeeper(Of Socket)()
                m_udpServer.ID = MyBase.ServerID
                m_udpServer.Passphrase = MyBase.HandshakePassphrase
                m_udpServer.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                If MyBase.Handshake Then
                    ' We will listen for data only when Handshake is enabled and a valid port has been specified in 
                    ' the configuration string. We do this in order to keep the server stable and besides that, the 
                    ' main purpose of a UDP server is to serve data in most cases.
                    If serverPort > 0 Then
                        m_udpServer.Client.Bind(New IPEndPoint(IPAddress.Any, serverPort))

                        With New Thread(AddressOf ReceiveClientData)
                            .Start()
                        End With
                    Else
                        Throw New ArgumentException("Server port must be specified in the configuration string.")
                    End If
                End If

                OnServerStarted(EventArgs.Empty)

                If Not MyBase.Handshake AndAlso m_configurationData.ContainsKey("clients") Then
                    ' We will ignore the client list in configuration string when Handshake is enabled.
                    For Each clientString As String In m_configurationData("clients").Replace(" ", "").Split(","c)
                        Try
                            Dim clientPort As Integer = serverPort
                            Dim clientStringSegments As String() = clientString.Split(":"c)
                            If clientStringSegments.Length = 2 Then
                                clientPort = Convert.ToInt32(clientStringSegments(1))
                            End If

                            Dim udpClient As New StateKeeper(Of IPEndPoint)()
                            udpClient.ID = Guid.NewGuid()
                            udpClient.Client = GetIpEndPoint(clientStringSegments(0), clientPort)
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

            OnServerStopped(EventArgs.Empty)
        End If

    End Sub

    Public Overrides Sub LoadSettings()

        MyBase.LoadSettings()

        If PersistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    PayloadAware = .Item("PayloadAware").GetTypedValue(m_payloadAware)
                    DestinationReachableCheck = .Item("DestinationReachableCheck").GetTypedValue(m_destinationReachableCheck)
                End With
            Catch ex As Exception
                ' We'll encounter exceptions if the settings are not present in the config file.
            End Try
        End If

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

#Region " Code Scope: Protected "

    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As System.Guid, ByVal data As Byte())

        If MyBase.Enabled AndAlso MyBase.IsRunning Then
            Dim udpClient As StateKeeper(Of IPEndPoint) = Nothing
            If m_udpClients.TryGetValue(clientID, udpClient) Then
                If m_destinationReachableCheck AndAlso Not IsDestinationReachable(udpClient.Client) Then Exit Sub

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
                    .Append(Environment.NewLine)
                    .Append("   [Port=Local port number;] [Clients=Client name or IP[:Port number], ..., Client name or IP[:Port number]]")
                    .Append(Environment.NewLine)
                    .Append("Text between square brackets, [...], is optional.")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException("ConfigurationString")
        End If

    End Function

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
                            .DataBuffer = TVA.Common.CreateArray(Of Byte)(MyBase.ReceiveBufferSize)
                        End If

                        bytesReceived = .Client.ReceiveFrom(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None, clientEndPoint)

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

                        OnReceivedClientData(New IdentifiableItemEventArgs(Of Byte())(udpClient.ID, data))
                        Exit Sub
                    End If
                Next
                OnReceivedClientData(New IdentifiableItemEventArgs(Of Byte())(data))
            End If
        End If

    End Sub

#End Region

End Class
