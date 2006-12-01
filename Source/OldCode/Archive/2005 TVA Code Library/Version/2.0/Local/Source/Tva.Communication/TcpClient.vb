'*******************************************************************************************************
'  Tva.Communication.TcpClient.vb - TCP-based communication client
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
'  06/02/2006 - Pinal C. Patel
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
Imports Tva.Serialization
Imports Tva.Communication.CommunicationHelper

''' <summary>
''' Represents a TCP-based communication client.
''' </summary>
Public Class TcpClient

    Private m_payloadAware As Boolean
    Private m_tcpClient As StateKeeper(Of Socket)
    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)

    ''' <summary>
    ''' Initializes a instance of Tva.Communication.TcpClient with the specified data.
    ''' </summary>
    ''' <param name="connectionString">The data that is required by the client to initialize.</param>
    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        MyBase.ConnectionString = connectionString  ' Override the default connection string.

    End Sub

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server will send the payload size before sending the payload.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the server will send the payload size before sending the payload; otherwise False.
    ''' </returns>
    ''' <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
    <Description("Indicates whether the server will send the payload size before sending the payload. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(GetType(Boolean), "False")> _
    Public Property PayloadAware() As Boolean
        Get
            Return m_payloadAware
        End Get
        Set(ByVal value As Boolean)
            m_payloadAware = value
        End Set
    End Property

    ''' <summary>
    ''' Cancels any active attempts of connecting to the server.
    ''' </summary>
    Public Overrides Sub CancelConnect()

        ' Client has not yet connected to the server so we'll abort the thread on which the client
        ' is attempting to connect to the server.
        If MyBase.Enabled AndAlso m_connectionThread IsNot Nothing Then
            m_connectionThread.Abort()
        End If
        m_connectionThread = Nothing

    End Sub

    ''' <summary>
    ''' Connects to the server asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If MyBase.Enabled AndAlso Not MyBase.IsConnected AndAlso ValidConnectionString(ConnectionString) Then
            ' Spawn a new thread on which the client will attempt to connect to the server.
            m_connectionThread = New Thread(AddressOf ConnectToServer)
            m_connectionThread.Start()
        End If

    End Sub

    ''' <summary>
    ''' Disconnects from the server it is connected to.
    ''' </summary>
    Public Overrides Sub Disconnect()

        CancelConnect() ' Cancel any active connection attempts.

        If MyBase.Enabled AndAlso MyBase.IsConnected AndAlso _
                m_tcpClient IsNot Nothing AndAlso m_tcpClient.Client IsNot Nothing Then
            ' Close the client socket that is connected to the server.
            m_tcpClient.Client.Close()
        End If

    End Sub

    ''' <summary>
    ''' Sends prepared data to the server.
    ''' </summary>
    ''' <param name="data">The prepared data that is to be sent to the server.</param>
    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If MyBase.Enabled AndAlso MyBase.IsConnected Then
            If MyBase.SecureSession Then data = EncryptData(data, m_tcpClient.Passphrase, MyBase.Encryption)
            OnSendDataBegin(New DataEventArgs(data))

            If m_payloadAware Then
                data = PayloadAwareHelper.AddPayloadHeader(data)
            End If
            ' We'll send data over the wire asynchronously for improved performance.
            m_tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)

            OnSendDataComplete(New DataEventArgs(data))
        End If

    End Sub

    ''' <summary>
    ''' Determines whether specified connection string required for connecting to the server is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("server") AndAlso _
                    Not String.IsNullOrEmpty(m_connectionData("server")) AndAlso _
                    m_connectionData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_connectionData("port")) Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Server=[Server name or IP]; Port=[Server port number]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    ''' <summary>
    ''' Connects to the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ConnectToServer()

        Dim connectionAttempts As Integer = 0

        Do While MyBase.MaximumConnectionAttempts = -1 OrElse connectionAttempts < MyBase.MaximumConnectionAttempts
            Try
                OnConnecting(EventArgs.Empty)   ' Notify that the client is connecting to the server.

                ' Create a socket for the client and bind it to a local endpoint.
                m_tcpClient = New StateKeeper(Of Socket)
                m_tcpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                m_tcpClient.Client.Bind(New IPEndPoint(IPAddress.Any, 0))
                m_tcpClient.Client.LingerState = New LingerOption(True, 10)
                If MyBase.ReceiveTimeout <> -1 Then m_tcpClient.Client.ReceiveTimeout = MyBase.ReceiveTimeout

                ' Connect the client socket to the remote server endpoint.
                m_tcpClient.Client.Connect(GetIpEndPoint(m_connectionData("server"), Convert.ToInt32(m_connectionData("port"))))

                If m_tcpClient.Client.Connected Then ' Client connected to the server successfully.
                    ' Start a seperate thread for the client to receive data from the server.
                    Dim receiveThread As New Thread(AddressOf ReceiveServerData)
                    receiveThread.Start()

                    m_connectionThread = Nothing
                    Exit Do ' Client successfully connected to the server.
                End If
            Catch ex As ThreadAbortException
                ' We'll stop trying to connect if a ThreadAbortException exception is encountered. This will
                ' be the case when the thread is deliberately aborted in CancelConnect() method in which case 
                ' we want to stop attempting to connect to the server.
                OnConnectingCancelled(EventArgs.Empty)
                Exit Do
            Catch ex As Exception
                connectionAttempts += 1
                OnConnectingException(New ExceptionEventArgs(ex, connectionAttempts))
            End Try
        Loop

    End Sub

    ''' <summary>
    ''' Receives data sent by the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ReceiveServerData()

        Try
            If MyBase.Handshake Then
                ' Handshaking is to be performed so we'll send our information to the server.
                Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(MyBase.ClientID, MyBase.HandshakePassphrase)))
                If m_payloadAware Then
                    myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)
                End If

                m_tcpClient.Client.Send(myInfo)
            Else
                OnConnected(EventArgs.Empty)
            End If

            Dim bytesReceived As Integer

            If ((MyBase.ReceiveRawDataFunction IsNot Nothing) OrElse _
                    (MyBase.ReceiveRawDataFunction Is Nothing AndAlso Not m_payloadAware)) Then
                ' We'll create the buffer that will be used to store the data that we pull off the wire.
                Dim bufferSize As Integer = MyBase.ReceiveBufferSize
                Dim buffer As Byte() = Tva.Common.CreateArray(Of Byte)(bufferSize)

                If MyBase.ReceiveRawDataFunction Is Nothing Then
                    MyBase.ReceiveRawDataFunction = AddressOf ProcessReceivedData
                End If

                Do While True
                    ' The maximum data we'll retrieve will be whatever the ReceiveBufferSize is specified to be
                    ' because that's the size of the buffer used for storing retrieved data.
                    bytesReceived = m_tcpClient.Client.Receive(buffer, 0, bufferSize, SocketFlags.None)

                    ' When we start receiving zero length data, it means that the connection was closed by the server.
                    If bytesReceived = 0 Then Exit Do

                    m_receiveRawDataFunction(buffer, 0, bytesReceived)
                    m_totalBytesReceived += bytesReceived
                Loop
            Else
                Dim payloadSize As Integer = -1
                Dim totalBytesReceived As Integer = 0
                Do While True
                    If payloadSize = -1 Then
                        ' If we don't have the payload size, we'll begin by reading the payload header which 
                        ' contains the payload size. Once we have the payload size we can receive payload.
                        m_tcpClient.DataBuffer = Tva.Common.CreateArray(Of Byte)(PayloadAwareHelper.PayloadHeaderSize)
                    End If

                    With m_tcpClient
                        bytesReceived = .Client.Receive(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None)
                    End With

                    ' When we start receiving zero length data, it means that the connection was closed by the server.
                    If bytesReceived = 0 Then Exit Do

                    If payloadSize = -1 Then
                        payloadSize = PayloadAwareHelper.GetPayloadSize(m_tcpClient.DataBuffer)
                        If payloadSize <> -1 AndAlso payloadSize <= CommunicationClientBase.MaximumDataSize Then
                            ' We have a valid payload size.
                            m_tcpClient.DataBuffer = Tva.Common.CreateArray(Of Byte)(payloadSize)
                        End If
                    Else
                        totalBytesReceived += bytesReceived
                        If totalBytesReceived = payloadSize Then
                            ' We've received the entire payload.
                            ProcessReceivedData(m_tcpClient.DataBuffer)

                            payloadSize = -1
                            totalBytesReceived = 0
                        End If
                    End If
                Loop
            End If
        Catch ex As Exception
            ' We don't need to take any action when an exception is encountered.
        Finally
            If m_tcpClient IsNot Nothing AndAlso m_tcpClient.Client IsNot Nothing Then
                m_tcpClient.Client.Close()
            End If
            OnDisconnected(EventArgs.Empty)
        End Try

    End Sub

    Private Sub ProcessReceivedData(ByVal data As Byte(), ByVal offset As Integer, ByVal length As Integer)

        ProcessReceivedData(Tva.IO.Common.CopyBuffer(data, offset, length))

    End Sub

    Private Sub ProcessReceivedData(ByVal data As Byte())

        If MyBase.ServerID = Guid.Empty AndAlso MyBase.Handshake Then
            ' Handshaking is to be performed, but it's not complete yet.
            Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(data))

            If serverInfo IsNot Nothing AndAlso serverInfo.ID <> Guid.Empty Then
                ' Authentication was successful and the server responded with its information.
                MyBase.ServerID = serverInfo.ID
                m_tcpClient.Passphrase = serverInfo.Passphrase
            Else
                ' Authetication was unsuccessful, so we must now disconnect.
                Throw New InvalidOperationException()
            End If

            OnConnected(EventArgs.Empty)
        Else
            If MyBase.SecureSession Then
                ' Decrypt the data usign private key if SecureSession is enabled.
                data = DecryptData(data, m_tcpClient.Passphrase, MyBase.Encryption)
            End If

            ' We'll pass the received data along to the consumer via event.
            OnReceivedData(New DataEventArgs(data))
        End If

    End Sub


    'With m_tcpClient
    '    Try
    '        If Handshake() Then
    '            ' Handshaking is to be performed so we'll send our information to the server.
    '            Dim myInfo As Byte() = _
    '                GetPreparedData(GetBytes(New HandshakeMessage(ClientID(), HandshakePassphrase())))
    '            If m_payloadAware Then .Client.Send(BitConverter.GetBytes(myInfo.Length()))
    '            .Client.Send(myInfo)
    '        Else
    '            ' Handshaking is not to be performed.
    '            OnConnected(EventArgs.Empty)    ' Notify that the client has been connected to the server.
    '        End If

    '        Dim received As Integer
    '        Dim length As Integer
    '        Dim dataBuffer As Byte() = Nothing
    '        Dim totalBytesReceived As Integer

    '        If m_receiveRawDataFunction Is Nothing Then
    '            If m_payloadAware Then
    '                length = TcpPacketHeaderSize
    '            Else
    '                length = ReceiveBufferSize
    '            End If
    '        Else
    '            length = m_buffer.Length
    '        End If

    '        ' Enter data read loop, this blocks thread while waiting for data from the server.
    '        Do While True
    '            Try
    '                ' Retrieve data from the TCP socket
    '                received = .Client.Receive(m_buffer, 0, length, SocketFlags.None)

    '                ' Post raw data to real-time function delegate if defined - this bypasses all other activity
    '                If m_receiveRawDataFunction IsNot Nothing Then
    '                    m_receiveRawDataFunction(m_buffer, 0, received)
    '                    m_totalBytesReceived += received
    '                    Continue Do
    '                End If

    '                If dataBuffer Is Nothing Then
    '                    dataBuffer = CreateArray(Of Byte)(length)
    '                    totalBytesReceived = 0
    '                End If

    '                ' Copy data into local cumulative buffer to start the unpacking process and eventually make the data available via event
    '                Buffer.BlockCopy(m_buffer, 0, dataBuffer, totalBytesReceived, dataBuffer.Length - totalBytesReceived)
    '                totalBytesReceived += received
    '            Catch ex As SocketException
    '                If ex.SocketErrorCode() = SocketError.TimedOut Then
    '                    OnReceiveTimedOut(EventArgs.Empty)  ' Notify that a timeout has been encountered.
    '                    ' NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
    '                    ' Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
    '                    .Client.Blocking = True  ' <= Temporary bug fix!
    '                    Continue Do
    '                Else
    '                    Throw
    '                End If
    '            Catch ex As Exception
    '                Throw
    '            End Try

    '            If received > 0 Then
    '                If m_payloadAware Then
    '                    If .PayloadSize = -1 AndAlso totalBytesReceived = TcpPacketHeaderSize Then
    '                        ' Size of the packet has been received.
    '                        .PayloadSize = BitConverter.ToInt32(dataBuffer, 0)
    '                        If .PayloadSize <= MaximumDataSize Then
    '                            dataBuffer = CreateArray(Of Byte)(.PayloadSize)
    '                            totalBytesReceived = 0
    '                            length = dataBuffer.Length
    '                            Continue Do
    '                        Else
    '                            Exit Do ' Packet size is not valid
    '                        End If
    '                    ElseIf .PayloadSize = -1 AndAlso totalBytesReceived < TcpPacketHeaderSize Then
    '                        ' Size of the packet is yet to be received.
    '                        Continue Do
    '                    ElseIf totalBytesReceived < dataBuffer.Length() Then
    '                        ' We have not yet received the entire packet.
    '                        Continue Do
    '                    End If
    '                Else
    '                    dataBuffer = CopyBuffer(dataBuffer, 0, received)
    '                    totalBytesReceived = 0
    '                End If

    '                If ServerID() = Guid.Empty AndAlso Handshake() Then
    '                    ' Authentication is required, but not performed yet. When authentication is required
    '                    ' the first message from the server, upon successful authentication, must be 
    '                    ' information about itself.
    '                    Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(dataBuffer))

    '                    If serverInfo IsNot Nothing AndAlso serverInfo.ID() <> Guid.Empty Then
    '                        ' Authentication was successful and the server responded with its information.
    '                        .Passphrase = serverInfo.Passphrase()
    '                        ServerID = serverInfo.ID()
    '                        OnConnected(EventArgs.Empty)    ' Notify that the client has been connected to the server.
    '                    Else
    '                        ' Authetication was unsuccessful, so we must now disconnect.
    '                        Exit Do
    '                    End If
    '                Else
    '                    If SecureSession() Then
    '                        dataBuffer = DecryptData(dataBuffer, .Passphrase, Encryption)
    '                        totalBytesReceived = 0
    '                    End If

    '                    ' Notify of data received from the client.
    '                    OnReceivedData(New DataEventArgs(dataBuffer))
    '                End If

    '                .PayloadSize = -1
    '                dataBuffer = Nothing
    '                totalBytesReceived = 0
    '                If m_payloadAware Then
    '                    length = TcpPacketHeaderSize
    '                Else
    '                    length = ReceiveBufferSize
    '                End If
    '            Else
    '                ' Client connection was forcibly closed by the server.
    '                Exit Do
    '            End If
    '        Loop
    '    Catch ex As Exception
    '        ' We don't need to take any action when an exception is encountered.
    '    Finally
    '        If m_tcpClient IsNot Nothing AndAlso .Client IsNot Nothing Then
    '            .Client.Close()
    '            .Client = Nothing
    '        End If
    '        OnDisconnected(EventArgs.Empty) ' Notify that the client has been disconnected to the server.
    '    End Try
    'End With

    'Private Sub ConcludeHandshake(ByVal data As Byte())

    '    Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(data))

    '    If serverInfo IsNot Nothing AndAlso serverInfo.ID <> Guid.Empty Then
    '        ' Authentication was successful and the server responded with its information.
    '        MyBase.ServerID = serverInfo.ID
    '        m_tcpClient.Passphrase = serverInfo.Passphrase
    '    Else
    '        ' Authetication was unsuccessful, so we must now disconnect.
    '        Throw New InvalidOperationException()
    '    End If

    'End Sub

    ' We'll copy the data that was received in the current receive operation to another buffer.
    'Dim receivedData As Byte() = Tva.IO.Common.CopyBuffer(m_tcpClient.DataBuffer, 0, bytesReceived)

    'If MyBase.ServerID = Guid.Empty AndAlso MyBase.Handshake Then
    '    ' Handshaking is to be performed, but it's not complete yet.
    '    ConcludeHandshake(receivedData)
    '    OnConnected(EventArgs.Empty)
    'Else
    '    If MyBase.SecureSession Then
    '        ' Decrypt the data usign private key if SecureSession is enabled.
    '        receivedData = DecryptData(receivedData, m_tcpClient.Passphrase, MyBase.Encryption)
    '    End If

    '    ' We'll pass the received data along to the consumer via event.
    '    OnReceivedData(New DataEventArgs(receivedData))
    'End If

End Class