'*******************************************************************************************************
'  Tva.Data.Transport.TcpServer.vb - Server for transporting data using TCP
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
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Threading
Imports Tva.Data.Transport.Common

Namespace Data.Transport

    Public Class TcpServer

        Private m_tcpServer As Socket
        Private m_tcpClients As Dictionary(Of Guid, Socket)
        Private m_pendingTcpClients As List(Of Socket)
        Private m_configurationStringData As Dictionary(Of String, String)

        ''' <summary>
        ''' Initializes a instance of Tva.Data.Transport.TcpServer with the specified data.
        ''' </summary>
        ''' <param name="configurationString">The data that is required by the server to initialize.</param>
        Public Sub New(ByVal configurationString As String)
            MyClass.New()
            MyBase.ConfigurationString = configurationString    ' Override the default configuration string value.
        End Sub

        ''' <summary>
        ''' Starts the server.
        ''' </summary>
        Public Overrides Sub Start()

            If MyBase.Enabled() AndAlso Not MyBase.IsRunning() AndAlso _
                    MyClass.ValidConfigurationString(MyBase.ConfigurationString()) Then
                ' Start the thread that will be listening for client connections.
                Dim listenerThread As New Thread(AddressOf ListenForConnections)
                listenerThread.Start()
            End If

        End Sub

        ''' <summary>
        ''' Stops the server.
        ''' </summary>
        Public Overrides Sub [Stop]()

            ' NOTE: Closing the socket for server and all of the connected clients will cause a SocketException
            ' in the thread that is using the socket and result in the thread to exit gracefully.

            ' *** Stop accepting incoming connections ***
            If m_tcpServer IsNot Nothing Then m_tcpServer.Close()
            ' *** Diconnect all of the connected clients ***
            If m_tcpClients IsNot Nothing Then
                SyncLock m_tcpClients
                    For Each tcpClient As Socket In m_tcpClients.Values()
                        If tcpClient IsNot Nothing Then tcpClient.Close()
                    Next
                End SyncLock
            End If
            If m_pendingTcpClients IsNot Nothing Then
                SyncLock m_pendingTcpClients
                    For Each pendingTcpClient As Socket In m_pendingTcpClients
                        If pendingTcpClient IsNot Nothing Then pendingTcpClient.Close()
                    Next
                End SyncLock
            End If

        End Sub

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="data">The data that is to be sent to the client.</param>
        Public Overrides Sub SendTo(ByVal clientID As Guid, ByVal data As Byte())

            If MyBase.Enabled() AndAlso MyBase.IsRunning() Then
                If data IsNot Nothing AndAlso data.Length() > 0 Then
                    ' We don't want to synclock 'm_tcpClientThreads' over here because doing so will block all
                    ' all incoming connections (in ListenForConnections) while sending data to client(s). 
                    If m_tcpClients.ContainsKey(clientID) Then
                        Dim tcpClient As Socket = m_tcpClients(clientID)
                        If tcpClient IsNot Nothing Then tcpClient.Send(data)
                    Else
                        Throw New ArgumentException("Client ID '" & clientID.ToString() & "' is invalid.")
                    End If
                Else
                    Throw New ArgumentNullException("data")
                End If
            End If

        End Sub

        ''' <summary>
        ''' Determines whether specified configuration string, required for the server to initialize, is valid.
        ''' </summary>
        ''' <param name="configurationString">The configuration string to be validated.</param>
        ''' <returns>True if the configuration string is valid.</returns>
        Protected Overrides Function ValidConfigurationString(ByVal configurationString As String) As Boolean

            If Not String.IsNullOrEmpty(configurationString) Then
                m_configurationStringData = Tva.Text.Common.ParseKeyValuePairs(configurationString)
                If m_configurationStringData.ContainsKey("port") AndAlso _
                        ValidPortNumber(Convert.ToString(m_configurationStringData("port"))) Then
                    Return True
                Else
                    ' Configuration string is not in the expected format.
                    With New StringBuilder()
                        .Append("Configuration string must be in the following format:")
                        .Append(Environment.NewLine())
                        .Append("   Port=<Port Number>")
                        Throw New ArgumentException(.ToString())
                    End With
                End If
            Else
                Throw New ArgumentNullException()
            End If

        End Function

        ''' <summary>
        ''' Listens for incoming connections.
        ''' </summary>
        ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
        Protected Sub ListenForConnections()

            Try
                ' Create a socket for the server.
                m_tcpServer = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                ' Tie the server socket to a local endpoint.
                m_tcpServer.Bind(New IPEndPoint(IPAddress.Any, Convert.ToInt32(m_configurationStringData("port"))))
                ' Start listening for connections and keep a maximum of 0 pending connection in the queue.
                m_tcpServer.Listen(0)
                MyBase.OnServerStarted(EventArgs.Empty) ' Notify that the server has started.

                Do While True
                    If MyBase.MaximumClients() = -1 OrElse MyBase.ClientIDs.Count() < MyBase.MaximumClients() Then
                        ' We can accept incoming client connection requests.
                        Dim tcpClient As Socket = m_tcpServer.Accept()  ' Accept client connection.
                        ' Start the client on a seperate thread so all the connected clients run independently.
                        RunThread.ExecuteNonPublicMethod(Me, "ReceiveClientData", tcpClient)
                        Thread.Sleep(1000)   ' Wait enough for the client thread to kick-off.
                    End If
                Loop
            Catch ex As Exception
                ' We will gracefully exit when an exception occurs.
            Finally
                If m_tcpServer IsNot Nothing Then
                    m_tcpServer.Close()
                    m_tcpServer = Nothing
                End If
                MyBase.OnServerStopped(EventArgs.Empty) ' Notify that the server has stopped.
            End Try

        End Sub

        ''' <summary>
        ''' Receives any data sent by a client that is connected to the server.
        ''' </summary>
        ''' <param name="tcpClient">System.Net.Sockets.Socket of the the connected client.</param>
        ''' <remarks>This method is meant to be executed on seperate threads.</remarks>
        Protected Sub ReceiveClientData(ByVal tcpClient As Socket)

            Dim tcpClientId As Guid = Nothing
            Try
                If MyBase.Handshake() Then
                    ' Authentication is to be performed before the client is considered to be connected, so we
                    ' add it to a list of pending connections until the authentication is performed. This is 
                    ' done so that pending connections can be closed when the server is stopped.
                    SyncLock m_pendingTcpClients
                        m_pendingTcpClients.Add(tcpClient)
                    End SyncLock
                Else
                    ' The client is considered to be connected since authentication is not to be performed.
                    tcpClientId = Guid.NewGuid()
                    SyncLock m_tcpClients
                        m_tcpClients.Add(tcpClientId, tcpClient)
                    End SyncLock

                    MyBase.OnClientConnected(tcpClientId)    ' Notify that the client is connected.
                End If

                Do While True
                    ' Wait for data from the client.
                    Dim receivedData() As Byte = CreateArray(Of Byte)(MyBase.ReceiveBufferSize())
                    tcpClient.Receive(receivedData) ' Block until data is received from client.

                    If IsBufferValid(receivedData) Then
                        ' Data received from the client is valid.
                        If MyBase.Handshake() AndAlso tcpClientId = Guid.Empty Then
                            ' Authentication is required, but not performed yet. When authentication is required
                            ' the first message from the client must be information about itself.
                            Dim clientIdentification As IdentificationMessage = DirectCast(GetObject(receivedData), IdentificationMessage)
                            If clientIdentification IsNot Nothing AndAlso _
                                    clientIdentification.ID() <> Guid.Empty AndAlso _
                                    clientIdentification.HandshakePassphrase() = MyBase.HandshakePassphrase() Then
                                ' Information provided by the client is valid, so now the server needs to send
                                ' information about itself to the client.
                                tcpClient.Send(GetBytes(CreateIdentificationMessage(MyBase.ServerID(), MyBase.HandshakePassphrase())))
                                tcpClientId = clientIdentification.ID()

                                SyncLock m_pendingTcpClients
                                    m_pendingTcpClients.Remove(tcpClient)
                                End SyncLock
                                SyncLock m_tcpClients
                                    m_tcpClients.Add(tcpClientId, tcpClient)
                                End SyncLock
                                MyBase.OnClientConnected(tcpClientId)    ' Notify that the client is connected.
                            Else
                                ' The first response from the client is either not information about itself, or
                                ' the information provided by the client is invalid.
                                Exit Do
                            End If
                        Else
                            ' Notify of data received from the client.
                            MyBase.OnReceivedClientData(tcpClientId, receivedData)
                        End If
                    Else
                        ' Connection is closed by the client.
                        Exit Do
                    End If
                Loop
            Catch ex As Exception
                ' We will exit gracefully in case of any exception.
            Finally
                ' We are now done with the client.
                If tcpClient IsNot Nothing Then
                    tcpClient.Close()
                    tcpClient = Nothing
                End If
                SyncLock m_pendingTcpClients
                    m_pendingTcpClients.Remove(tcpClient)
                End SyncLock
                SyncLock m_tcpClients
                    If m_tcpClients.ContainsKey(tcpClientId) Then
                        m_tcpClients.Remove(tcpClientId)
                        MyBase.OnClientDisconnected(tcpClientId)    ' Notify that the client is disconnected.
                    End If
                End SyncLock
            End Try

        End Sub

    End Class

End Namespace