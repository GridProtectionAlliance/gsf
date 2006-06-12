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
Imports Tva.Common
Imports Tva.Serialization
Imports Tva.Data.Transport.Common

Namespace Data.Transport

    Public Class TcpClient

        Private m_tcpClient As Socket
        Private m_connectivityThread As Thread
        Private m_connectionStringData As IDictionary(Of String, String)

        Public Sub New(ByVal connectionString As String)
            MyClass.New()
            MyBase.ConnectionString = connectionString  ' Override the default connection string.
        End Sub

        ''' <summary>
        ''' Connects the client to the server asynchronously.
        ''' </summary>
        Public Overrides Sub Connect()

            If MyBase.Enabled() AndAlso Not MyBase.IsConnected() Then
                ' Spawn a new thread on which the client will attempt to connect to the server.
                m_connectivityThread = New Thread(AddressOf ConnectToServer)
                m_connectivityThread.Start()
            End If

        End Sub

        ''' <summary>
        ''' Cancels any active attempts of connecting the client to the server.
        ''' </summary>
        Public Overrides Sub CancelConnect()

            ' Client has not yet connected to the server so we'll abort the thread on which the client
            ' is attempting to connect to the server.
            If m_connectivityThread IsNot Nothing Then m_connectivityThread.Abort()

        End Sub

        ''' <summary>
        ''' Disconnects the client from the server it is connected to.
        ''' </summary>
        Public Overrides Sub Disconnect()

            CancelConnect() ' Cancel any pending connection attempts.

            ' Close the client socket that is connected to the server.
            If m_tcpClient IsNot Nothing Then m_tcpClient.Close()

        End Sub

        ''' <summary>
        ''' Sends data to the server.
        ''' </summary>
        ''' <param name="data">The data that is to be sent to the server.</param>
        Public Overrides Sub Send(ByVal data As Byte())

            If MyBase.Enabled() AndAlso MyBase.IsConnected() Then
                If data IsNot Nothing AndAlso data.Length() > 0 Then  ' There is some data to be sent.
                    MyBase.OnSendBegin(data)
                    m_tcpClient.Send(data)  ' Send data to the server.
                    MyBase.OnSendComplete(data)
                Else
                    Throw New ArgumentNullException("data")
                End If
            End If

        End Sub

        ''' <summary>
        ''' Determines whether specified connection string, required for the client to connect to the server, 
        ''' is valid.
        ''' </summary>
        ''' <param name="connectionString">The connection string to be validated.</param>
        ''' <returns>True is the connection string is valid; otherwise False.</returns>
        Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

            If Not String.IsNullOrEmpty(connectionString) Then
                m_connectionStringData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
                If m_connectionStringData.ContainsKey("server") AndAlso _
                        m_connectionStringData.ContainsKey("port") AndAlso _
                        Dns.GetHostEntry(Convert.ToString(m_connectionStringData("server"))) IsNot Nothing AndAlso _
                        ValidPortNumber(Convert.ToString(m_connectionStringData("port"))) Then
                    Return True
                Else
                    ' Connection string is not in the expected format.
                    With New StringBuilder()
                        .Append("Connection string must be in the following format:")
                        .Append(Environment.NewLine())
                        .Append("   Server=<Server name or IP>;Port=<Port Number>")
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
            Do While (MyBase.MaximumConnectionAttempts() = -1) OrElse _
                    (connectionAttempts < MyBase.MaximumConnectionAttempts())
                Try
                    MyBase.OnConnecting(EventArgs.Empty)

                    ' Create a socket for the client and bind it to a local endpoint.
                    m_tcpClient = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    m_tcpClient.Bind(New IPEndPoint(IPAddress.Any, 0))
                    ' Connect the client socket to the remote server endpoint.
                    m_tcpClient.Connect(GetIpEndPoint(Convert.ToString(m_connectionStringData("server")), _
                        Convert.ToInt32(m_connectionStringData("port"))))
                    If m_tcpClient.Connected() Then ' Client connected to the server successfully.
                        ' Start a seperate thread for the client to receive data from the server.
                        Dim receivingThread As New Thread(AddressOf ReceiveServerData)
                        receivingThread.Start()

                        m_connectivityThread = Nothing
                        Exit Do ' Client successfully connected to the server.
                    End If
                Catch ex As ThreadAbortException
                    ' We'll stop trying to connect if a ThreadAbortException exception is encountered. This will
                    ' be the case when the thread is deliberately aborted in CancelConnect() method in which case 
                    ' we want to stop attempting to connect to the server.
                    MyBase.OnConnectingCancelled(EventArgs.Empty)
                    Exit Do
                Catch ex As Exception
                    m_tcpClient = Nothing
                    If MyBase.MaximumConnectionAttempts() > 0 AndAlso _
                            connectionAttempts = MyBase.MaximumConnectionAttempts() - 1 Then
                        ' This is our last attempt for connecting to the server.
                        MyBase.OnConnectingException(ex)
                    End If
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
                If Not MyBase.Handshake() Then
                    MyBase.OnConnected(EventArgs.Empty)
                End If

                Do While True
                    ' We'll wait for data from the server until client is dissconnected from the server.
                    Dim receivedData() As Byte = CreateArray(Of Byte)(MyBase.ReceiveBufferSize())
                    m_tcpClient.Receive(receivedData)   ' Block until data is received from the server.

                    If MyBase.Handshake() AndAlso MyBase.ServerID() = Guid.Empty Then
                        Dim serverIdentification As IdentificationMessage = DirectCast(GetObject(receivedData), IdentificationMessage)
                        If serverIdentification IsNot Nothing AndAlso serverIdentification.ID() <> Guid.Empty Then
                            m_tcpClient.Send(GetBytes(CreateIdentificationMessage(MyBase.ClientID())))
                            MyBase.ServerID = serverIdentification.ID()
                            MyBase.OnConnected(EventArgs.Empty)
                        Else
                            Exit Do
                        End If
                    Else
                        MyBase.OnReceivedData(receivedData)
                    End If
                Loop
            Catch ex As Exception
                ' We'll don't need to take any action when an exception is encountered.
            Finally
                If m_tcpClient IsNot Nothing Then
                    m_tcpClient.Close()
                    m_tcpClient = Nothing
                End If
                MyBase.OnDisconnected(EventArgs.Empty)
            End Try

        End Sub

    End Class

End Namespace