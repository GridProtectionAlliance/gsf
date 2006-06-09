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
Imports Tva.Threading
Imports Tva.Data.Transport.Common

Namespace Data.Transport

    Public Class TcpServer

        Private m_tcpServer As Socket
        Private m_tcpClients As Dictionary(Of Guid, Socket)
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

            If MyBase.Enabled() AndAlso MyBase.IsRunning() Then
                ' NOTE: Closing the socket for server and all of the connected clients will cause a SocketException
                ' in the thread that is using the socket and result in the thread to exit gracefully.

                ' ***  Stop accepting incoming connections ***
                If m_tcpServer IsNot Nothing Then m_tcpServer.Close()
                ' *** Diconnect all of the connected clients ***
                If m_tcpClients IsNot Nothing Then
                    SyncLock m_tcpClients
                        For Each tcpClient As Socket In m_tcpClients.Values()
                            If tcpClient IsNot Nothing Then tcpClient.Close()
                        Next
                    End SyncLock
                End If
            End If

        End Sub

        ''' <summary>
        ''' Sends data to the specified client.
        ''' </summary>
        ''' <param name="clientID">ID of the client to which the data is to be sent.</param>
        ''' <param name="data">The data that is to be sent to the client.</param>
        Public Overrides Sub SendTo(ByVal clientID As Guid, ByVal data() As Byte)

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
                        Dim tcpClientId As Guid = Guid.NewGuid() ' Create an ID for the client.
                        ' Start the client on a seperate thread so all the connected clients run independently.
                        RunThread.ExecuteNonPublicMethod(Me, "ReceiveClientData", tcpClientId, tcpClient)
                        Thread.Sleep(100)   ' Wait enough for the client thread to kick-off.
                    Else
                        ' We cannot accept any new connections, but we'll ensure that the server socket is alive
                        ' while we are waiting.
                        If m_tcpServer.IsBound() Then Continue Do
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
        ''' <param name="tcpClientID">ID of the connected client.</param>
        ''' <param name="tcpClient">System.Net.Sockets.Socket of the the connected client.</param>
        ''' <remarks>This method is meant to be executed on seperate threads.</remarks>
        Protected Sub ReceiveClientData(ByVal tcpClientID As Guid, ByVal tcpClient As Socket)

            Try
                SyncLock m_tcpClients
                    m_tcpClients.Add(tcpClientID, tcpClient)
                End SyncLock

                MyBase.OnClientConnected(tcpClientID)    ' Notify that the client is connected.

                Do While True
                    ' Wait for data from the client.
                    Dim receivedData() As Byte = CreateArray(Of Byte)(MyBase.ReceiveBufferSize())
                    tcpClient.Receive(receivedData) ' Block until data is received from client.
                    ' Notify of data received from the client.
                    MyBase.OnReceivedClientData(tcpClientID, receivedData)
                Loop
            Catch ex As Exception
                ' We will exit gracefully in case of any exception.
            Finally
                ' We are now done with the client.
                If tcpClient IsNot Nothing Then
                    tcpClient.Close()
                    tcpClient = Nothing
                End If
                SyncLock m_tcpClients
                    m_tcpClients.Remove(tcpClientID)
                End SyncLock
                MyBase.OnClientDisconnected(tcpClientID)    ' Notify that the client is disconnected.
            End Try

        End Sub

    End Class

End Namespace