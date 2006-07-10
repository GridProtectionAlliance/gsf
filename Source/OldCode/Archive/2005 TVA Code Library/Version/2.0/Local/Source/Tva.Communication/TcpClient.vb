'*******************************************************************************************************
'  Tva.Data.Transport.TcpClient.vb - Client for transporting data using TCP
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
'
'*******************************************************************************************************

Option Strict On

Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.IO.Common
Imports Tva.Serialization
Imports Tva.Communication.SocketHelper

''' <summary>
''' Represents a client involved in the transportation of data over the network using TCP.
''' </summary>
Public Class TcpClient

    Private m_packetAware As Boolean
    Private m_tcpClient As StateKeeper(Of Socket)
    Private m_connectionThread As Thread
    Private m_connectionData As IDictionary(Of String, String)

    ''' <summary>
    ''' Size of the packet that will contain the size of the acutal packet.
    ''' </summary>
    Private Const PacketHeaderSize As Integer = 4

    Public Sub New(ByVal connectionString As String)
        MyClass.New()
        MyBase.ConnectionString = connectionString  ' Override the default connection string.
    End Sub

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether the server will send the size of the packet before 
    ''' sending the actual packet.
    ''' </summary>
    ''' <value></value>
    ''' <returns>
    ''' True if the server will send the size of the packet before sending the actual packet; otherwise False.
    ''' </returns>
    ''' <remarks>This property must be set to True is wither Encryption or Compression is enabled.</remarks>
    <Description("Indicates whether the server will send the size of the packet before sending the actual packet."), Category("Data"), DefaultValue(GetType(Boolean), "True")> _
    Public Property PacketAware() As Boolean
        Get
            Return m_packetAware
        End Get
        Set(ByVal value As Boolean)
            m_packetAware = value
        End Set
    End Property

    ''' <summary>
    ''' Connects the client to the server asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If Enabled() AndAlso Not IsConnected() Then
            ' Spawn a new thread on which the client will attempt to connect to the server.
            m_connectionThread = New Thread(AddressOf ConnectToServer)
            m_connectionThread.Start()
        End If

    End Sub

    ''' <summary>
    ''' Cancels any active attempts of connecting the client to the server.
    ''' </summary>
    Public Overrides Sub CancelConnect()

        ' Client has not yet connected to the server so we'll abort the thread on which the client
        ' is attempting to connect to the server.
        If Enabled() AndAlso m_connectionThread IsNot Nothing Then m_connectionThread.Abort()

    End Sub

    ''' <summary>
    ''' Disconnects the client from the server it is connected to.
    ''' </summary>
    Public Overrides Sub Disconnect()

        CancelConnect() ' Cancel any active connection attempts.

        If Enabled() AndAlso IsConnected() AndAlso m_tcpClient IsNot Nothing AndAlso _
                m_tcpClient.Client() IsNot Nothing Then
            ' Close the client socket that is connected to the server.
            m_tcpClient.Client.Close()
        End If

    End Sub

    ''' <summary>
    ''' Sends prepared data to the server.
    ''' </summary>
    ''' <param name="data">The prepared data that is to be sent to the server.</param>
    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        If Enabled() AndAlso IsConnected() Then
            OnSendDataBegin(data)
            If SecureSession() Then data = EncryptData(data, m_tcpClient.Passphrase(), Encryption())
            ' We'll send data over the wire asynchronously for improved performance.
            If m_packetAware Then
                Dim packetHeader As Byte() = BitConverter.GetBytes(data.Length())
                m_tcpClient.Client.BeginSend(packetHeader, 0, packetHeader.Length(), SocketFlags.None, Nothing, Nothing)
            End If
            m_tcpClient.Client.BeginSend(data, 0, data.Length(), SocketFlags.None, Nothing, Nothing)
            OnSendDataComplete(data)
        End If

    End Sub

    ''' <summary>
    ''' Determines whether specified connection string required for the client to connect to the server is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("server") AndAlso _
                    m_connectionData.ContainsKey("port") AndAlso _
                    Dns.GetHostEntry(Convert.ToString(m_connectionData("server"))) IsNot Nothing AndAlso _
                    ValidPortNumber(Convert.ToString(m_connectionData("port"))) Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine())
                    .Append("   Server=<Server name or IP>; Port=<Port Number>")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    ''' <summary>
    ''' Connects the client to the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ConnectToServer()

        Dim connectionAttempts As Integer = 0
        Do While (MaximumConnectionAttempts() = -1) OrElse _
                (connectionAttempts < MaximumConnectionAttempts())
            Try
                OnConnecting(EventArgs.Empty)   ' Notify that the client is connecting to the server.

                ' Create a socket for the client and bind it to a local endpoint.
                m_tcpClient = New StateKeeper(Of Socket)
                m_tcpClient.Client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                m_tcpClient.Client.Bind(New IPEndPoint(IPAddress.Any, 0))
                m_tcpClient.Client.LingerState = New LingerOption(True, 10)
                If ReceiveTimeout() <> -1 Then m_tcpClient.Client.ReceiveTimeout = ReceiveTimeout() * 1000

                ' Connect the client socket to the remote server endpoint.
                m_tcpClient.Client.Connect(GetIpEndPoint(Convert.ToString(m_connectionData("server")), _
                    Convert.ToInt32(m_connectionData("port"))))

                If m_tcpClient.Client.Connected() Then ' Client connected to the server successfully.
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
                OnConnectingException(ex)
            Finally
                connectionAttempts += 1
            End Try
        Loop

    End Sub

    ''' <summary>
    ''' Receives data sent by the server.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ReceiveServerData()

        Try
            If Handshake() Then
                ' Handshaking is to be performed so we'll send our information to the server.
                Dim myInfo As Byte() = _
                    GetPreparedData(GetBytes(New HandshakeMessage(ClientID(), HandshakePassphrase())))
                If m_packetAware Then m_tcpClient.Client.Send(BitConverter.GetBytes(myInfo.Length()))
                m_tcpClient.Client.Send(myInfo)
            Else
                ' Handshaking is not to be performed.
                OnConnected(EventArgs.Empty)    ' Notify that the client has been connected to the server.
            End If

            Do While True   ' Wait for data from the server
                If m_tcpClient.DataBuffer() Is Nothing Then
                    Dim bufferSize As Integer = PacketHeaderSize
                    If Not m_packetAware Then bufferSize = ReceiveBufferSize()
                    m_tcpClient.DataBuffer = CreateArray(Of Byte)(bufferSize)
                End If

                Dim dataLength As Integer
                Try
                    dataLength = _
                        m_tcpClient.Client.Receive(m_tcpClient.DataBuffer(), m_tcpClient.BytesReceived(), m_tcpClient.DataBuffer.Length(), SocketFlags.None)
                    m_tcpClient.BytesReceived += dataLength
                Catch ex As SocketException
                    If ex.SocketErrorCode() = SocketError.TimedOut Then
                        OnReceiveTimedOut(EventArgs.Empty)  ' Notify that a timeout has been encountered.
                        ' NOTE: The line of code below is a fix to a known bug in .Net Framework 2.0.
                        ' Refer http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=178213&SiteID=1
                        m_tcpClient.Client.Blocking = True  ' <= Temporary bug fix!
                        Continue Do
                    Else
                        Throw
                    End If
                Catch ex As Exception
                    Throw
                End Try

                If dataLength > 0 Then
                    If m_packetAware Then
                        If m_tcpClient.PacketSize() = -1 AndAlso m_tcpClient.BytesReceived() = PacketHeaderSize Then
                            ' Size of the packet has been received.
                            m_tcpClient.PacketSize = BitConverter.ToInt32(m_tcpClient.DataBuffer(), 0)
                            If m_tcpClient.PacketSize() <= MaximumDataSize Then
                                m_tcpClient.DataBuffer = CreateArray(Of Byte)(m_tcpClient.PacketSize())
                                Continue Do
                            Else
                                Exit Do ' Packet size is not valid
                            End If
                        ElseIf m_tcpClient.PacketSize() = -1 AndAlso m_tcpClient.BytesReceived() < PacketHeaderSize Then
                            ' Size of the packet is yet to be received.
                            Continue Do
                        ElseIf m_tcpClient.BytesReceived() < m_tcpClient.DataBuffer.Length() Then
                            ' We have not yet received the entire packet.
                            Continue Do
                        End If
                    Else
                        m_tcpClient.DataBuffer = CopyBuffer(m_tcpClient.DataBuffer(), 0, m_tcpClient.BytesReceived())
                    End If

                    If ServerID() = Guid.Empty AndAlso Handshake() Then
                        ' Authentication is required, but not performed yet. When authentication is required
                        ' the first message from the server, upon successful authentication, must be 
                        ' information about itself.
                        Dim serverInfo As HandshakeMessage = _
                            DirectCast(GetObject(GetActualData(m_tcpClient.DataBuffer)), HandshakeMessage)
                        If serverInfo IsNot Nothing AndAlso _
                                serverInfo.ID() <> Guid.Empty Then
                            ' Authentication was successful and the server responded with its information.
                            m_tcpClient.Passphrase = serverInfo.Passphrase()
                            ServerID = serverInfo.ID()
                            OnConnected(EventArgs.Empty)    ' Notify that the client has been connected to the server.
                        Else
                            ' Authetication was unsuccessful, so we must now disconnect.
                            Exit Do
                        End If
                    Else
                        If SecureSession() Then
                            m_tcpClient.DataBuffer = DecryptData(m_tcpClient.DataBuffer(), m_tcpClient.Passphrase(), Encryption())
                        End If
                        ' Notify of data received from the client.
                        OnReceivedData(m_tcpClient.DataBuffer())
                    End If
                    m_tcpClient.PacketSize = -1
                    m_tcpClient.DataBuffer = Nothing
                Else
                    ' Client connection was forcibly closed by the server.
                    Exit Do
                End If
            Loop
        Catch ex As Exception
            ' We don't need to take any action when an exception is encountered.
        Finally
            If m_tcpClient IsNot Nothing AndAlso m_tcpClient.Client() IsNot Nothing Then
                m_tcpClient.Client.Close()
                m_tcpClient.Client = Nothing
            End If
            OnDisconnected(EventArgs.Empty) ' Notify that the client has been disconnected to the server.
        End Try

    End Sub

End Class