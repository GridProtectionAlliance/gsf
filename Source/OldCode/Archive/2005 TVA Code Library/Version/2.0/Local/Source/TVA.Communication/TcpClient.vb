'*******************************************************************************************************
'  TVA.Communication.TcpClient.vb - TCP-based communication client
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
'  12/01/2006 - Pinal C. Patel
'       Modified code for handling "PayloadAware" transmissions
'
'*******************************************************************************************************

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Imports TVA.Serialization
Imports TVA.Communication.CommunicationHelper
Imports TVA.ErrorManagement

''' <summary>
''' Represents a TCP-based communication client.
''' </summary>
<DisplayName("TcpClient")> _
Public Class TcpClient

#Region " Member Declaration "

    Private m_payloadAware As Boolean
    Private m_tcpClient As StateInfo(Of Socket)
    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' Initializes a instance of TVA.Communication.TcpClient with the specified data.
    ''' </summary>
    ''' <param name="connectionString">The connection string containing the data required for connecting to a TCP server.</param>
    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        MyBase.ConnectionString = connectionString

    End Sub

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the message boundaries are to be preserved during transmission.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the message boundaries are to be preserved during transmission; otherwise False.
    ''' </returns>
    ''' <remarks>This property must be set to True if either Encryption or Compression is enabled.</remarks>
    <Description("Indicates whether the message boundaries are to be preserved during transmission. Set to True if either Encryption or Compression is enabled."), Category("Data"), DefaultValue(GetType(Boolean), "False")> _
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

        If MyBase.Enabled AndAlso m_connectionThread.IsAlive Then
            ' The client attempts to connect to the server on a seperate thread and since that thread is still 
            ' running, we know that the client has not yet connected to the server. We can now abort the thread 
            ' to stop the client from attempting to connect to the server.
            m_connectionThread.Abort()
        End If

    End Sub

    ''' <summary>
    ''' Connects to the server asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If MyBase.Enabled AndAlso Not MyBase.IsConnected AndAlso ValidConnectionString(MyBase.ConnectionString) Then
            ' Start the thread on which the client will attempt to connect to the server.
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
                m_tcpClient IsNot Nothing AndAlso m_tcpClient.Client IsNot Nothing Then
            ' Close the client socket that is connected to the server.
            m_tcpClient.Client.Close()
        End If

    End Sub

    Public Overrides Sub LoadSettings()

        MyBase.LoadSettings()

        Try
            With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                If .Count > 0 Then
                    PayloadAware = .Item("PayloadAware").GetTypedValue(m_payloadAware)
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
                        .Description = "True if the message boundaries are to be preserved during transmission; otherwise False."
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
            ' Encrypt the data with private key if SecureSession is enabled.
            If MyBase.SecureSession Then data = EncryptData(data, m_tcpClient.Passphrase, MyBase.Encryption)

            ' Add payload header if client-server communication is PayloadAware.
            If m_payloadAware Then data = PayloadAwareHelper.AddPayloadHeader(data)

            OnSendDataBegin(New IdentifiableItem(Of Guid, Byte())(ClientID, data))

            ' PCP - 05/30/2007: Using synchronous send to see if asynchronous transmission get out-of-sequence.
            m_tcpClient.Client.Send(data)
            '' We'll send data over the wire asynchronously for improved performance.
            'm_tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)

            OnSendDataComplete(New IdentifiableItem(Of Guid, Byte())(ClientID, data))
        End If

    End Sub

    ''' <summary>
    ''' Determines whether specified connection string required for connecting to the server is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = TVA.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("server") AndAlso _
                    Not String.IsNullOrEmpty(m_connectionData("server")) AndAlso _
                    m_connectionData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_connectionData("port")) Then
                ' The connection string must always contain the following:
                ' >> server - Name or IP of the machine machine on which the server is running.
                ' >> port - Port number on which the server is listening for connections.
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   Server=Server name or IP; Port=Server port number")
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

                ' Create a TCP socket and bind it to a local endpoint. Binding the socket will establish a 
                ' physical presence of the socket necessary for receving data from the server
                m_tcpClient = New StateInfo(Of Socket)()
                m_tcpClient.ID = MyBase.ClientID
                m_tcpClient.Passphrase = MyBase.HandshakePassphrase
                m_tcpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                m_tcpClient.Client.Bind(New IPEndPoint(IPAddress.Any, 0))
                m_tcpClient.Client.LingerState = New LingerOption(True, 10)
                ' Imposed a timeout on receiving data if specified.
                If MyBase.ReceiveTimeout <> -1 Then m_tcpClient.Client.ReceiveTimeout = MyBase.ReceiveTimeout

                ' Attempt to connect the client socket to the remote server endpoint.
                m_tcpClient.Client.Connect(GetIpEndPoint(m_connectionData("server"), Convert.ToInt32(m_connectionData("port"))))

                If m_tcpClient.Client.Connected Then ' Client connected to the server successfully.
                    ' Start a seperate thread for the client to receive data from the server.
                    With New Thread(AddressOf ReceiveServerData)
                        .Start()
                    End With

                    Exit Do ' Client successfully connected to the server.
                End If
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

    ''' <summary>
    ''' Receives data sent by the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ReceiveServerData()

        Try
            With m_tcpClient
                If MyBase.Handshake Then
                    ' Handshaking is to be performed so we'll send our information to the server.
                    Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(m_tcpClient.ID, m_tcpClient.Passphrase)))

                    ' Add payload header if client-server communication is PayloadAware.
                    If m_payloadAware Then myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)

                    .Client.Send(myInfo)
                Else
                    OnConnected(EventArgs.Empty)
                End If

                ' Used to count the number of bytes received in a single receive.
                Dim bytesReceived As Integer = 0
                ' Receiving of data from the server has been seperated into 2 different section resulting in some 
                ' redundant coding. This is necessary to achive a high performance TCP client component since it 
                ' may be used in real-time applications where performance is the key and evey millisecond saved 
                ' makes a big difference.
                If m_receiveRawDataFunction IsNot Nothing OrElse _
                        (m_receiveRawDataFunction Is Nothing AndAlso Not m_payloadAware) Then
                    ' In this section the consumer either wants to receive data and pass it on to a delegate or 
                    ' receive data that doesn't contain metadata used for preserving message boundaries. In either
                    ' case we can use a static buffer that can be used over and over again for receiving data.
                    Do While True
                        Try
                            ' Receive data into the static buffer.
                            bytesReceived = .Client.Receive(m_buffer, 0, m_buffer.Length, SocketFlags.None)

                            ' We start receiving zero-length data when a TCP connection is disconnected by the 
                            ' opposite party. In such case we must consider ourself disconnected from the server.
                            If bytesReceived = 0 Then Throw New SocketException(10101)

                            If m_receiveRawDataFunction IsNot Nothing Then
                                ' Post raw data to the delegate that is most likely used for real-time applications.
                                m_receiveRawDataFunction(m_buffer, 0, bytesReceived)
                                m_totalBytesReceived += bytesReceived
                            Else
                                ProcessReceivedServerData(TVA.IO.Common.CopyBuffer(m_buffer, 0, bytesReceived))
                            End If
                        Catch ex As SocketException
                            If ex.SocketErrorCode = SocketError.TimedOut Then
                                HandleReceiveTimeout()
                            Else
                                Throw
                            End If
                        End Try
                    Loop
                Else
                    ' In this section we will be receiving data that has metadata used for preserving message 
                    ' boundaries. Here a message (the payload) is sent by the other party along with some metadata 
                    ' (payload header) prepended to the message. The metadata (payload header) consists of a 4-byte 
                    ' marker used to mark the beginning of a message, followed by the message size (also 4-bytes), 
                    ' followed by the actual message.
                    Dim payloadSize As Integer = -1
                    Dim totalBytesReceived As Integer = 0
                    Do While True
                        If payloadSize = -1 Then
                            ' If we don't have the payload size, we'll begin by reading the payload header which 
                            ' contains the payload size. Once we have the payload size we can receive payload.
                            .DataBuffer = TVA.Common.CreateArray(Of Byte)(PayloadAwareHelper.PayloadHeaderSize)
                        End If

                        Try
                            ' Since TCP is a streaming protocol we can receive a part of the available data and
                            ' the remaing data can be received in subsequent receives.
                            bytesReceived = .Client.Receive(.DataBuffer, totalBytesReceived, (.DataBuffer.Length - totalBytesReceived), SocketFlags.None)

                            If bytesReceived = 0 Then Throw New SocketException(10101)

                            If payloadSize = -1 Then
                                ' We don't what the payload size is, so we'll check if the data we have contains
                                ' the size of the payload we need to receive.
                                payloadSize = PayloadAwareHelper.GetPayloadSize(.DataBuffer)
                                If payloadSize <> -1 AndAlso payloadSize <= CommunicationClientBase.MaximumDataSize Then
                                    ' We have a valid payload size, so we'll create a buffer that's big enough 
                                    ' to hold the entire payload. Remember, the payload at the most can be as big
                                    ' as whatever the MaximumDataSize is.
                                    .DataBuffer = TVA.Common.CreateArray(Of Byte)(payloadSize)
                                End If
                            Else
                                totalBytesReceived += bytesReceived
                                If totalBytesReceived = payloadSize Then
                                    ' We've received the entire payload.
                                    ProcessReceivedServerData(.DataBuffer)

                                    ' Initialize for receiving the next payload.
                                    payloadSize = -1
                                    totalBytesReceived = 0
                                End If
                            End If
                        Catch ex As SocketException
                            If ex.SocketErrorCode = SocketError.TimedOut Then
                                HandleReceiveTimeout()
                            Else
                                Throw
                            End If
                        End Try
                    Loop
                End If
            End With
        Catch ex As Exception
            ' We don't need to take any action when an exception is encountered.
        Finally
            If m_tcpClient IsNot Nothing AndAlso m_tcpClient.Client IsNot Nothing Then
                m_tcpClient.Client.Close()
            End If
            OnDisconnected(EventArgs.Empty)
        End Try

    End Sub

    ''' <summary>
    ''' This method will not be required once the bug in .Net Framwork is fixed.
    ''' </summary>
    Private Sub HandleReceiveTimeout()

        OnReceiveTimedOut(EventArgs.Empty)  ' Notify that a timeout has been encountered.
        ' NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
        ' Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
        m_tcpClient.Client.Blocking = True  ' <= Temporary bug fix!

    End Sub

    ''' <summary>
    ''' This method processes the data received from the server.
    ''' </summary>
    ''' <param name="data">The data received from the server.</param>
    Private Sub ProcessReceivedServerData(ByVal data As Byte())

        If MyBase.ServerID = Guid.Empty AndAlso MyBase.Handshake Then
            ' Handshaking is to be performed, but it's not complete yet.

            Dim serverInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(data))
            If serverInfo IsNot Nothing AndAlso serverInfo.ID <> Guid.Empty Then
                ' Authentication was successful and the server responded with its information.
                MyBase.ServerID = serverInfo.ID
                m_tcpClient.Passphrase = serverInfo.Passphrase
            Else
                ' Authetication was unsuccessful, so we must now disconnect.
                Throw New ApplicationException("Authentication with the server failed.")
            End If

            OnConnected(EventArgs.Empty)
        Else
            ' Decrypt the data usign private key if SecureSession is enabled.
            If MyBase.SecureSession Then data = DecryptData(data, m_tcpClient.Passphrase, MyBase.Encryption)

            ' We'll pass the received data along to the consumer via event.
            OnReceivedData(New IdentifiableItem(Of Guid, Byte())(ServerID, data))
        End If

    End Sub

#End Region

End Class