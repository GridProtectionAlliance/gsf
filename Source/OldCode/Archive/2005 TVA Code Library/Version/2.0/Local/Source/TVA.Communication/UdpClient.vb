'*******************************************************************************************************
'  TVA.Communication.UdpClient.vb - UDP-based communication client
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

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Imports TVA.Common
Imports TVA.Serialization
Imports TVA.Communication.CommunicationHelper
Imports TVA.ErrorManagement

''' <summary>
''' Represents a UDP-based communication client.
''' </summary>
''' <remarks>
''' UDP by nature is a connectionless protocol, but with this implementation of UDP client we can have a 
''' connectionfull session with the server by enabling Handshake. This in-turn enables us to take advantage
''' of SecureSession which otherwise is not possible.
''' </remarks>
<DisplayName("UdpClient")> _
Public Class UdpClient

#Region " Member Declaration "

    Private m_payloadAware As Boolean
    Private m_destinationReachableCheck As Boolean
    Private m_udpServer As IPEndPoint
    Private m_udpClient As StateKeeper(Of Socket)
    Private m_receivingThread As Thread
    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)

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
    ''' Initializes a instance of TVA.Communication.UdpClient with the specified data.
    ''' </summary>
    ''' <param name="connectionString">The connection string containing the data required for initializing the UDP client.</param>
    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        MyBase.ConnectionString = connectionString

    End Sub

    ''' <summary>
    ''' Gets or sets the maximum number of bytes that can be received at a time by the client from the server.
    ''' </summary>
    ''' <value>Receive buffer size</value>
    ''' <exception cref="InvalidOperationException">This exception will be thrown if an attempt is made to change the receive buffer size while client is connected</exception>
    ''' <exception cref="ArgumentOutOfRangeException">This exception will be thrown if an attempt is made to set the receive buffer size to a value that is less than one</exception>
    ''' <returns>The maximum number of bytes that can be received at a time by the client from the server.</returns>
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

    ''' <summary>
    ''' Cancels any active attempts of connecting to the server.
    ''' </summary>
    Public Overrides Sub CancelConnect()

        If MyBase.Enabled Then
            ' We'll abort the thread on which the client is initialized if it's alive.
            If m_connectionThread.IsAlive Then m_connectionThread.Abort()

            ' *** The above and below conditions are mutually exclusive ***

            ' If the client has Handshake enabled, it is not considered connected until a handshake message is
            ' received from the server. So, if the thread on which we receive data from the server is alive, but
            ' the client is not yet flagged as connected, we'll abort that thread.
            If Not MyBase.IsConnected AndAlso m_receivingThread.IsAlive Then
                m_receivingThread.Abort()
                OnConnectingCancelled(EventArgs.Empty)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Connects to the server asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If MyBase.Enabled AndAlso Not MyBase.IsConnected AndAlso ValidConnectionString(MyBase.ConnectionString) Then
            ' Start the thread on which the client will be initialized.
            m_connectionThread = New Thread(AddressOf ConnectToServer)
            m_connectionThread.Start()
        End If

    End Sub

    ''' <summary>
    ''' Disconnects client from the connected server.
    ''' </summary>
    Public Overrides Sub Disconnect()

        CancelConnect() ' Cancel any active connection attempts.

        If MyBase.Enabled AndAlso MyBase.IsConnected AndAlso _
                m_udpClient IsNot Nothing AndAlso m_udpClient.Client IsNot Nothing Then
            If MyBase.Handshake Then
                ' We have a connectionfull session with the server, so we'll send a goodbye message to the server
                ' indicating the that the session has ended.
                Dim goodbye As Byte() = GetPreparedData(GetBytes(New GoodbyeMessage(m_udpClient.ID)))

                ' Add payload header if client-server communication is PayloadAware.
                If m_payloadAware Then goodbye = PayloadAwareHelper.AddPayloadHeader(goodbye)

                Try
                    m_udpClient.Client.SendTo(goodbye, m_udpServer)
                Catch ex As ObjectDisposedException
                    ' Its OK to igonore ObjectDisposedException which we might encounter if Disconnect() method is
                    ' called consecutively within a very short duration (before the client is flagged as disconnected).
                End Try
            End If

            m_udpClient.Client.Close()
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

#Region " Code Scope: Protected "

    ''' <summary>
    ''' Sends prepared data to the server.
    ''' </summary>
    ''' <param name="data">The prepared data that is to be sent to the server.</param>
    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If MyBase.Enabled AndAlso MyBase.IsConnected Then
            ' We'll check if the server is reachable before send data to it.
            If m_destinationReachableCheck AndAlso Not IsDestinationReachable(m_udpServer) Then Exit Sub

            ' Encrypt the data with private key if SecureSession is enabled.
            If MyBase.SecureSession Then data = EncryptData(data, m_udpClient.Passphrase, MyBase.Encryption)

            ' Add payload header if client-server communication is PayloadAware.
            If m_payloadAware Then data = PayloadAwareHelper.AddPayloadHeader(data)

            OnSendDataBegin(New IdentifiableItem(Of Guid, Byte())(ClientID, data))

            ' Since UDP is a Datagram protocol, we must make sure that the datagram we transmit are no bigger
            ' than what the server can receive. For this reason we'll break up the data into multiple datagrams
            ' if data being transmitted is bigger than what the server can receive. Since we don't know what the
            ' the server's ReceiveBufferSize is, we assume it to be the same as the client's. And it is for this 
            ' reason it is important that the ReceiveBufferSize of both the client and server are the same.
            Dim toIndex As Integer = 0
            Dim datagramSize As Integer = MyBase.ReceiveBufferSize
            If data.Length > datagramSize Then toIndex = data.Length - 1
            For i As Integer = 0 To toIndex Step datagramSize
                ' Last or the only datagram in the series.
                If data.Length - i < datagramSize Then datagramSize = data.Length - i

                ' We'll send the data asynchronously for better performance.
                m_udpClient.Client.BeginSendTo(data, i, datagramSize, SocketFlags.None, m_udpServer, Nothing, Nothing)
            Next

            OnSendDataComplete(New IdentifiableItem(Of Guid, Byte())(ClientID, data))
        End If

    End Sub

    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = TVA.Text.Common.ParseKeyValuePairs(connectionString)
            ' At the very least the connection string must have a local port specified and can optionally have a 
            ' server and a remote port. Server and remote port is required when Handshake is enable, but if they
            ' are not specified then an arbitrary server enpoint will be created and any attempt of sending data
            ' to the server will fail. So, it becomes the consumer's responsibility to provide a valid server name
            ' and remote port if Handshake is enabled. At the same time when Handshake is enabled, the local port
            ' value will be ignored even if it is specified.
            If (m_connectionData.ContainsKey("localport") AndAlso _
                    ValidPortNumber(m_connectionData("localport"))) OrElse _
                    (m_connectionData.ContainsKey("server") AndAlso _
                    Not String.IsNullOrEmpty(m_connectionData("server")) AndAlso _
                    (m_connectionData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_connectionData("port"))) OrElse _
                    m_connectionData.ContainsKey("remoteport") AndAlso _
                    ValidPortNumber(m_connectionData("remoteport"))) Then
                ' The connection string must always contain the following:
                ' >> localport - Port number on which the client is listening for data.
                ' OR
                ' >> server - Name or IP of the machine machine on which the server is running.
                ' >> port or remoteport - Port number on which the server is listening for connections.
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   [Server=Server name or IP;] [[Remote]Port=Server port number;] LocalPort=Local port number")
                    .Append(Environment.NewLine)
                    .Append("Text between square brackets, [...], is optional.")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException("ConnectionString")
        End If

    End Function

#End Region

#Region " Code Scope: Private "

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ConnectToServer()

        Dim connectionAttempts As Integer = 0
        Do While MyBase.MaximumConnectionAttempts = -1 OrElse connectionAttempts < MyBase.MaximumConnectionAttempts
            Try
                OnConnecting(EventArgs.Empty)

                ' When the client is not intended for communicating with the server, the "LocalPort" value will be
                ' present and "Server" and "Port" or "RemotePort" values may not be present in the connection string.
                ' In this case we'll use the default values for server (localhost) and remoteport (0) to create an 
                ' imaginary server endpoint.
                ' When the client is intended for communicating with the server, the "Server" and "Port" or 
                ' "RemotePort" will be present along with the "LocalPort" value. The "LocalPort" value however 
                ' becomes optional when client is configured to do Handshake with the server. When Handshake is 
                ' enabled, we let the system assign a port to us and the server will then send data to us at the 
                ' assigned port.
                Dim server As String = "localhost"
                Dim localPort As Integer = 0
                Dim remotePort As Integer = 0
                If m_connectionData.ContainsKey("server") Then server = m_connectionData("server")
                If m_connectionData.ContainsKey("port") Then
                    remotePort = Convert.ToInt32(m_connectionData("port"))
                ElseIf m_connectionData.ContainsKey("remoteport") Then
                    remotePort = Convert.ToInt32(m_connectionData("remoteport"))
                End If
                If Not MyBase.Handshake Then localPort = Convert.ToInt32(m_connectionData("localport"))

                ' Create the server endpoint that will be used for sending data.
                m_udpServer = GetIpEndPoint(server, remotePort)

                ' Create a UDP socket and bind it to a local endpoint for receiving data.
                m_udpClient = New StateKeeper(Of Socket)()
                m_udpClient.ID = MyBase.ClientID
                m_udpClient.Passphrase = MyBase.HandshakePassphrase
                m_udpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                m_udpClient.Client.Bind(New IPEndPoint(IPAddress.Any, localPort))
                ' Imposed a timeout on receiving data if specified.
                If MyBase.ReceiveTimeout <> -1 Then m_udpClient.Client.ReceiveTimeout = MyBase.ReceiveTimeout

                ' Start listening for data from the server on a seperate thread.
                m_receivingThread = New Thread(AddressOf ReceiveServerData)
                m_receivingThread.Start()

                Exit Do ' The process of initiating the connection is complete.
            Catch ex As ThreadAbortException
                ' We'll stop trying to connect if a System.Threading.ThreadAbortException exception is encountered. 
                ' This will be the case when the thread is deliberately aborted in CancelConnect() method in which 
                ' case we want to stop attempting to connect to the server.
                OnConnectingCancelled(EventArgs.Empty)
                Exit Do
            Catch ex As Exception
                connectionAttempts += 1
                OnConnectingException(ex)
            End Try
        Loop

    End Sub

    Private Sub ReceiveServerData()

        Try
            ' In order to make UDP connectionfull, which can be done by enabling Handshake, we must send our 
            ' information to the server so that it is knowledge of the client and in return the server sends us 
            ' its about itself, so we have knowledge of the server. This allows UDP to function more like TCP in 
            ' the sense that multiple UDP client can be connected to a server when both server and clien are on 
            ' the same machine.
            With m_udpClient
                Dim connectionAttempts As Integer = 0
                Do While MyBase.MaximumConnectionAttempts = -1 OrElse connectionAttempts < MyBase.MaximumConnectionAttempts
                    OnConnecting(EventArgs.Empty)
                    If MyBase.Handshake Then
                        ' Handshaking is enabled so we'll send our information to the server.
                        Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(.ID, .Passphrase)))

                        ' Add payload header if client-server communication is PayloadAware.
                        If m_payloadAware Then myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)

                        .Client.SendTo(myInfo, m_udpServer)
                    Else
                        ' If handshaking is disabled, the client is considered to be connected to the server.
                        OnConnected(EventArgs.Empty)
                    End If

                    ' Used to count the number of bytes received in a single receive.
                    Dim bytesReceived As Integer = 0
                    ' Receiving of data from the server has been seperated into 2 different section resulting in
                    ' some redundant coding. This is necessary to achive a high performance UDP client component
                    ' since it may be used in real-time applications where performance is the key and evey 
                    ' millisecond saved makes a big difference.
                    If m_receiveRawDataFunction IsNot Nothing OrElse _
                            (m_receiveRawDataFunction Is Nothing AndAlso Not m_payloadAware) Then
                        ' In this section the consumer either wants to receive the datagrams and pass it on to a
                        ' delegate or receive datagrams that don't contain metadata used for re-assembling the
                        ' datagrams into the original message and be notified via events. In either case we can use
                        ' a static buffer that can be used over and over again for receiving datagrams as long as
                        ' the datagrams received are not bigger than the receive buffer.
                        Do While True
                            Try
                                ' Receive a datagram into the static buffer.
                                bytesReceived = .Client.ReceiveFrom(m_buffer, 0, m_buffer.Length, SocketFlags.None, CType(m_udpServer, EndPoint))

                                If m_receiveRawDataFunction IsNot Nothing Then
                                    ' Post the received datagram to the delegate.
                                    m_receiveRawDataFunction(m_buffer, 0, bytesReceived)
                                    m_totalBytesReceived += bytesReceived
                                    Continue Do
                                Else
                                    ProcessReceivedServerData(TVA.IO.Common.CopyBuffer(m_buffer, 0, bytesReceived))
                                End If

                                ' If Handshake is enabled and we haven't received server information than we're not
                                ' considered as connected and so we'll keep trying to connect.
                                If Not MyBase.IsConnected Then Exit Do
                            Catch ex As SocketException
                                Select Case ex.SocketErrorCode
                                    Case SocketError.TimedOut
                                        HandleReceiveTimeout()
                                    Case SocketError.ConnectionReset
                                        ' We'll encounter this exception when we try sending our information to the
                                        ' server and the server is unreachable (or not running). So, keep trying!
                                        OnConnectingException(ex)
                                        Exit Do
                                    Case Else
                                        Throw
                                End Select
                            End Try
                        Loop
                    Else
                        ' In this section we will be receiving datagrams in which a single datagrams may contain
                        ' the entire message or a part of the message (i.e. A message too big to fit in a datagram
                        ' when sending is split up into multiple datagrams). In either case the first datagram will
                        ' contain the metadata (payload header) used for re-assembling the datagrams into the 
                        ' original message (payload). The metadata consists of a 4-byte marker used to identify the
                        ' first datagram in the series, followed by the message size (also 4-bytes), followed by the 
                        ' actual message.
                        Dim payloadSize As Integer = -1
                        Dim totalBytesReceived As Integer = 0
                        Do While True
                            If payloadSize = -1 Then
                                .DataBuffer = TVA.Common.CreateArray(Of Byte)(MyBase.ReceiveBufferSize)
                            End If

                            Try
                                ' Since UDP is a datagram protocol, we must receive the entire datagram and not just
                                ' a portion if it. This also means that our receive buffer must be as big as the
                                ' datagram that is to be received.
                                bytesReceived = .Client.ReceiveFrom(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None, CType(m_udpServer, EndPoint))

                                If payloadSize = -1 Then
                                    ' We don't have the payload size, so we'll check if the datagram we received
                                    ' contains the payload size. Remember, only the first datagram (even in a 
                                    ' series, if the message needs to be broken down into multiple datagrams)
                                    ' contains the payload size.
                                    payloadSize = PayloadAwareHelper.GetPayloadSize(.DataBuffer)
                                    If payloadSize <> -1 AndAlso payloadSize <= CommunicationClientBase.MaximumDataSize Then
                                        ' We have a valid payload size.
                                        Dim payload As Byte() = PayloadAwareHelper.GetPayload(.DataBuffer)

                                        ' We'll extract the payload we've received in the datagram. It may be 
                                        ' that this is the only datagram in the series and that this datagram
                                        ' contains the entire payload; this is tested in the code below.
                                        .DataBuffer = TVA.Common.CreateArray(Of Byte)(payloadSize)
                                        Buffer.BlockCopy(payload, 0, .DataBuffer, 0, payload.Length)
                                        bytesReceived = payload.Length
                                    End If
                                End If

                                totalBytesReceived += bytesReceived
                                If totalBytesReceived = payloadSize Then
                                    ' We've received the entire payload.
                                    ProcessReceivedServerData(.DataBuffer)

                                    ' Initialize for receiving the next payload.
                                    payloadSize = -1
                                    totalBytesReceived = 0
                                End If

                                If Not MyBase.IsConnected Then Exit Do
                            Catch ex As SocketException
                                Select Case ex.SocketErrorCode
                                    Case SocketError.TimedOut
                                        HandleReceiveTimeout()
                                    Case SocketError.ConnectionReset
                                        OnConnectingException(ex)
                                        Exit Do
                                    Case SocketError.MessageSize
                                        ' When in "PayloadAware" mode, we may by receving a payload broken down into
                                        ' a series of datagrams and if one of the datagrams from that series is
                                        ' dropped, we'll encounter this exception because we'll probably be expecting
                                        ' a datagram of one size whereas we receive a datagram of a different size 
                                        ' (most likely bigger than the size we're expecting). In this case we'll
                                        ' drop the partial content of the payload we've received so far and go back
                                        ' to receiving the next payload.
                                        payloadSize = -1
                                        totalBytesReceived = 0
                                    Case Else
                                        Throw
                                End Select
                            End Try
                        Loop
                    End If

                    ' If we're here, it means that Handshake is enabled and we were unable to get a response back
                    ' from the server, so we must keep trying to connect to the server.
                    connectionAttempts += 1
                Loop
            End With
        Catch ex As Exception
            ' We don't need to take any action when an exception is encountered.
        Finally
            If m_udpClient IsNot Nothing AndAlso m_udpClient.Client IsNot Nothing Then
                m_udpClient.Client.Close()
            End If
            If MyBase.IsConnected Then OnDisconnected(EventArgs.Empty)
        End Try

    End Sub

    ''' <summary>
    ''' This method will not be required once the bug in .Net Framwork is fixed.
    ''' </summary>
    Private Sub HandleReceiveTimeout()

        OnReceiveTimedOut(EventArgs.Empty)  ' Notify that a timeout has been encountered.
        ' NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
        ' Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
        m_udpClient.Client.Blocking = True  ' <= Temporary bug fix!

    End Sub

    ''' <summary>
    ''' This method processes the data received from the server.
    ''' </summary>
    ''' <param name="data">The data received from the server.</param>
    Private Sub ProcessReceivedServerData(ByVal data As Byte())

        ' Don't proceed further if there is not data to process.
        If data.Length = 0 Then Exit Sub

        If MyBase.ServerID = Guid.Empty AndAlso MyBase.Handshake Then
            ' Handshaking is to be performed, but it's not complete yet.

            Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(data))
            If serverInfo IsNot Nothing AndAlso serverInfo.ID <> Guid.Empty Then
                ' Authentication was successful and the server responded with its information.
                MyBase.ServerID = serverInfo.ID
                m_udpClient.Passphrase = serverInfo.Passphrase
                OnConnected(EventArgs.Empty)
            End If
        Else
            If MyBase.Handshake AndAlso GetObject(Of GoodbyeMessage)(GetActualData(data)) IsNot Nothing Then
                ' Handshaking is enabled and the server has sent up nootification to disconnect.
                Throw New SocketException(10101)
            End If

            ' Decrypt the data usign private key if SecureSession is enabled.
            If MyBase.SecureSession Then data = DecryptData(data, m_udpClient.Passphrase, MyBase.Encryption)

            ' We'll pass the received data along to the consumer via event.
            OnReceivedData(New IdentifiableItem(Of Guid, Byte())(ServerID, data))
        End If

    End Sub
#End Region

End Class