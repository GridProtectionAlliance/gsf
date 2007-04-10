'*******************************************************************************************************
'  TVA.Communication.TcpServer.vb - TCP-based communication server
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
Imports TVA.Security.Cryptography.Common

''' <summary>
''' Represents a TCP-based communication server.
''' </summary>
Public Class TcpServer

#Region " Member Declaration "

    Private m_payloadAware As Boolean
    Private m_tcpServer As Socket
    Private m_tcpClients As Dictionary(Of Guid, StateKeeper(Of Socket))
    Private m_pendingTcpClients As List(Of StateKeeper(Of Socket))
    Private m_configurationData As Dictionary(Of String, String)
    Private m_listenerThread As Thread

#End Region

#Region " Code Scope: Public "

    ''' <summary>
    ''' Initializes a instance of TVA.Communication.TcpServer with the specified data.
    ''' </summary>
    ''' <param name="configurationString">The connection string containing the data required for the TCP server to run.</param>
    Public Sub New(ByVal configurationString As String)

        MyClass.New()
        MyBase.ConfigurationString = configurationString

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
    ''' Starts the server.
    ''' </summary>
    Public Overrides Sub Start()

        If MyBase.Enabled AndAlso Not MyBase.IsRunning AndAlso ValidConfigurationString(MyBase.ConfigurationString) Then
            ' Start the thread on which the server will listen for incoming connections.
            m_listenerThread = New Thread(AddressOf ListenForConnections)
            m_listenerThread.Start()
        End If

    End Sub

    ''' <summary>
    ''' Stops the server.
    ''' </summary>
    Public Overrides Sub [Stop]()

        If MyBase.Enabled AndAlso MyBase.IsRunning Then
            ' NOTE: Closing the server and all of the connected client sockets will cause a 
            '       System.Net.Socket.SocketException in the thread using the socket. This in turn will result in 
            '       the thread to exit gracefully because of the exception handling in place in the threads.

            ' *** Stop accepting incoming connections ***
            If m_tcpServer IsNot Nothing Then m_tcpServer.Close()

            ' *** Diconnect all of the connected clients ***
            SyncLock m_tcpClients
                For Each tcpClient As StateKeeper(Of Socket) In m_tcpClients.Values
                    If tcpClient IsNot Nothing AndAlso tcpClient.Client IsNot Nothing Then
                        tcpClient.Client.Close()
                    End If
                Next
            End SyncLock
            SyncLock m_pendingTcpClients
                For Each pendingTcpClient As StateKeeper(Of Socket) In m_pendingTcpClients
                    If pendingTcpClient IsNot Nothing AndAlso pendingTcpClient.Client IsNot Nothing Then
                        pendingTcpClient.Client.Close()
                    End If
                Next
            End SyncLock
        End If

    End Sub

    Public Overrides Sub LoadSettings()

        MyBase.LoadSettings()

        If PersistSettings Then
            Try
                With TVA.Configuration.Common.CategorizedSettings(SettingsCategoryName)
                    PayloadAware = .Item("PayloadAware").GetTypedValue(m_payloadAware)
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
    ''' Sends prepared data to the specified client.
    ''' </summary>
    ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
    ''' <param name="data">The prepared data that is to be sent to the client.</param>
    Protected Overrides Sub SendPreparedDataTo(ByVal clientID As Guid, ByVal data As Byte())

        If MyBase.Enabled AndAlso MyBase.IsRunning Then
            ' We don't want to synclock 'm_tcpClients' over here because doing so will block all
            ' all incoming connections (in ListenForConnections) while sending data to client(s). 
            Dim tcpClient As StateKeeper(Of Socket) = Nothing
            If m_tcpClients.TryGetValue(clientID, tcpClient) Then
                ' Encrypt the data with private key if SecureSession is enabled.
                If MyBase.SecureSession Then data = EncryptData(data, tcpClient.Passphrase, Encryption())

                ' Add payload header if client-server communication is PayloadAware.
                If m_payloadAware Then data = PayloadAwareHelper.AddPayloadHeader(data)

                ' We'll send data over the wire asynchronously for improved performance.
                tcpClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, Nothing, Nothing)
            Else
                Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
            End If
        End If

    End Sub

    ''' <summary>
    ''' Determines whether specified configuration string required for the server to initialize is valid.
    ''' </summary>
    ''' <param name="configurationString">The configuration string to be validated.</param>
    ''' <returns>True if the configuration string is valid.</returns>
    Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

        If Not String.IsNullOrEmpty(configurationString) Then
            m_configurationData = TVA.Text.Common.ParseKeyValuePairs(configurationString)
            If m_configurationData.ContainsKey("port") AndAlso _
                    ValidPortNumber(m_configurationData("port")) Then
                ' The configuration string must always contain the following:
                ' >> port - Port number on which the server will be listening for incoming connections.
                Return True
            Else
                ' Configuration string is not in the expected format.
                With New StringBuilder()
                    .Append("Configuration string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   Port=Local port number")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException("ConfigurationString")
        End If

    End Function

#End Region

#Region " Code Scope: Private"

    ''' <summary>
    ''' Listens for incoming client connections.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ListenForConnections()

        Try
            ' Create a TCP socket and bind it a local endpoint at the specified port. Binding the socket will 
            ' establish a physical presence of the socket necessary for listening to incoming connections.
            m_tcpServer = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            m_tcpServer.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationData("port"))))

            ' Start listening for connections and keep a maximum of 0 pending connection in the queue.
            m_tcpServer.Listen(0)

            OnServerStarted(EventArgs.Empty)

            Dim connectedClient As Integer = 0
            Do While True
                If MyBase.MaximumClients = -1 OrElse MyBase.ClientIDs.Count < MyBase.MaximumClients Then
                    ' We can accept incoming client connection requests.
                    Dim tcpClient As New StateKeeper(Of Socket)()
                    tcpClient.Client = m_tcpServer.Accept()  ' Accept client connection.
                    tcpClient.Client.LingerState = New LingerOption(True, 10)

                    ' Start the client on a seperate thread so all the connected clients run independently.
                    ThreadPool.QueueUserWorkItem(AddressOf ReceiveClientData, tcpClient)
                End If
            Loop
        Catch ex As ThreadAbortException
            ' This will be a normal exception...
        Catch ex As ObjectDisposedException
            ' This will be a normal exception...
        Catch ex As SocketException
            If ex.SocketErrorCode <> SocketError.Interrupted Then
                ' If we encounter a socket exception other than SocketError.Interrupt, we'll report it as an exception.
                OnServerStartupException(New ExceptionEventArgs(ex))
            End If
        Catch ex As Exception
            ' We will gracefully exit when an exception occurs.
            OnServerStartupException(New ExceptionEventArgs(ex))
        Finally
            If m_tcpServer IsNot Nothing Then
                m_tcpServer.Close()
            End If
            OnServerStopped(EventArgs.Empty)
        End Try

    End Sub

    ''' <summary>
    ''' Receives any data sent by a client that is connected to the server.
    ''' </summary>
    ''' <param name="state">TVA.Communication.StateKeeper(Of Socket) of the the connected client.</param>
    ''' <remarks>This method is meant to be executed on seperate threads.</remarks>
    Private Sub ReceiveClientData(ByVal state As Object)

        Dim tcpClient As StateKeeper(Of Socket) = DirectCast(state, StateKeeper(Of Socket))
        Try
            With tcpClient
                If MyBase.Handshake Then
                    ' Handshaking is to be performed to authenticate the client, so we'll add the client to the 
                    ' list of client who have not been authenticated and give it 30 seconds to initiate handshaking.
                    .Client.ReceiveTimeout = 30000

                    SyncLock m_pendingTcpClients
                        m_pendingTcpClients.Add(.This)
                    End SyncLock
                Else
                    ' No handshaking is to be performed for authenicating the client, so we'll add the client 
                    ' to the list of connected clients.
                    .ID = Guid.NewGuid()

                    SyncLock m_tcpClients
                        m_tcpClients.Add(.ID, .This)
                    End SyncLock

                    OnClientConnected(New IdentifiableSourceEventArgs(.ID))
                End If

                ' Used to count the number of bytes received in a single receive.
                Dim bytesReceived As Integer = 0
                ' Receiving of data from the client has been seperated into 2 different section resulting in some 
                ' redundant coding. This is necessary to achive a high performance TCP server component since it 
                ' may be used in real-time applications where performance is the key and evey millisecond  saved 
                ' makes a big difference.
                If m_receiveRawDataFunction IsNot Nothing OrElse _
                        (m_receiveRawDataFunction Is Nothing AndAlso Not m_payloadAware) Then
                    ' In this section the consumer either wants to receive data and pass it on to a delegate or 
                    ' receive data that doesn't contain metadata used for preserving message boundaries. In either
                    ' case we can use a static buffer that can be used over and over again for receiving data.
                    Do While True
                        ' Receive data into the static buffer.
                        bytesReceived = .Client.Receive(m_buffer, 0, m_buffer.Length, SocketFlags.None)

                        ' We start receiving zero-length data when a TCP connection is disconnected by the 
                        ' opposite party. In such case we must consider ourself disconnected from the client.
                        If bytesReceived = 0 Then Throw New SocketException(10101)

                        If m_receiveRawDataFunction IsNot Nothing Then
                            ' Post raw data to the delegate that is most likely used for real-time applications.
                            m_receiveRawDataFunction(m_buffer, 0, bytesReceived)
                        Else
                            ProcessReceivedClientData(TVA.IO.Common.CopyBuffer(m_buffer, 0, bytesReceived), .This)
                        End If
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
                                ProcessReceivedClientData(.DataBuffer, .This)

                                ' Initialize for receiving the next payload.
                                payloadSize = -1
                                totalBytesReceived = 0
                            End If
                        End If
                    Loop
                End If
            End With
        Catch ex As Exception
            ' We don't need to take any action when an exception is encountered.
        Finally
            ' We are now done with the client.
            If tcpClient IsNot Nothing AndAlso tcpClient.Client IsNot Nothing Then
                tcpClient.Client.Close()
            End If

            SyncLock m_pendingTcpClients
                m_pendingTcpClients.Remove(tcpClient)
            End SyncLock

            SyncLock m_tcpClients
                If m_tcpClients.ContainsKey(tcpClient.ID) Then
                    m_tcpClients.Remove(tcpClient.ID)
                    OnClientDisconnected(New IdentifiableSourceEventArgs(tcpClient.ID))
                End If
            End SyncLock
        End Try

    End Sub

    ''' <summary>
    ''' This method processes the data received from the client.
    ''' </summary>
    ''' <param name="data">The data received from the client.</param>
    ''' <param name="tcpClient">The TCP client who sent the data.</param>
    Private Sub ProcessReceivedClientData(ByVal data As Byte(), ByVal tcpClient As StateKeeper(Of Socket))

        If tcpClient.ID = Guid.Empty AndAlso MyBase.Handshake Then
            ' Authentication is required, but not performed yet. When authentication is required
            ' the first message from the client must be information about itself.
            Dim clientInfo As HandshakeMessage = GetObject(Of HandshakeMessage)(GetActualData(data))

            If clientInfo IsNot Nothing AndAlso clientInfo.ID <> Guid.Empty AndAlso clientInfo.Passphrase = MyBase.HandshakePassphrase Then
                ' We'll generate a private key for the client if SecureSession is enabled.
                tcpClient.ID = clientInfo.ID
                If MyBase.SecureSession Then tcpClient.Passphrase = GenerateKey()

                ' We'll send our information to the client which may contain a private crypto key .
                Dim myInfo As Byte() = GetPreparedData(GetBytes(New HandshakeMessage(MyBase.ServerID, tcpClient.Passphrase)))
                If m_payloadAware Then myInfo = PayloadAwareHelper.AddPayloadHeader(myInfo)
                tcpClient.Client.Send(myInfo)

                tcpClient.ID = clientInfo.ID
                tcpClient.Client.ReceiveTimeout = 0 ' We don't want to timeout while waiting for data from client.

                ' The client's authentication is now complete.
                SyncLock m_pendingTcpClients
                    m_pendingTcpClients.Remove(tcpClient)
                End SyncLock

                SyncLock m_tcpClients
                    m_tcpClients.Add(tcpClient.ID, tcpClient)
                End SyncLock

                OnClientConnected(New IdentifiableSourceEventArgs(tcpClient.ID))
            Else
                ' The first response from the client is either not information about itself, or
                ' the information provided by the client is invalid.
                Throw New ApplicationException("Failed to authenticate the client.")
            End If
        Else
            ' Decrypt the data usign private key if SecureSession is enabled.
            If MyBase.SecureSession Then data = DecryptData(data, tcpClient.Passphrase, MyBase.Encryption)

            ' We'll pass the received data along to the consumer via event.
            OnReceivedClientData(New IdentifiableItemEventArgs(Of Byte())(tcpClient.ID, data))
        End If

    End Sub

#End Region
    
End Class