'***********************************************************************
'  Listener.vb - UDP/TCP based listener for network data
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System.IO
Imports TVA.Threading
Imports TVA.Shared.DateTime

Namespace Sockets

    Public Class Listener

        Implements IDisposable

        Private m_protocol As NetworkProtocol
        Private m_processBuffer As ProcessBufferSignature
        Private m_updateStatus As UpdateStatusSignature
        Private m_enabled As Boolean
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_serverPort As Integer
        Private m_listenThread As RunThread
        Private m_clientThreads As Hashtable
        Private m_packetsAccepted As Long
        Private m_packetsRejected As Long
        Private m_bytesReceived As Long
        Private m_udpBufferSize As Integer

        Private Const BufferSize As Integer = 4096   ' 4Kb buffer

        ' Initialize listener with function to directly process network buffers
        Public Sub New( _
            ByVal protocol As NetworkProtocol, _
            ByVal processBufferFunction As ProcessBufferSignature, _
            ByVal updateStatusFunction As UpdateStatusSignature, _
            ByVal serverPort As Integer)

            m_protocol = protocol
            m_processBuffer = processBufferFunction
            m_updateStatus = updateStatusFunction
            m_serverPort = serverPort
            m_enabled = True
            m_clientThreads = New Hashtable
            m_udpBufferSize = BufferSize

        End Sub

        Public Overridable Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal Value As Boolean)
                m_enabled = Value

                ' When disabled we stop listening, we start listening again when enabled...
                If m_enabled Then
                    If Not IsRunning Then Start()
                Else
                    If IsRunning Then [Stop]()
                End If
            End Set
        End Property

        Public Overridable ReadOnly Property Protocol() As NetworkProtocol
            Get
                Return m_protocol
            End Get
        End Property

        Public Overridable Property UdpBufferSize() As Integer
            Get
                Return m_udpBufferSize
            End Get
            Set(ByVal Value As Integer)
                m_udpBufferSize = Value
            End Set
        End Property

        Public Overridable ReadOnly Property ServerPort() As Integer
            Get
                Return m_serverPort
            End Get
        End Property

        Public Overridable ReadOnly Property RunTime() As Double
            Get
                Dim ProcessingTime As Long

                If m_startTime > 0 Then
                    If m_stopTime > 0 Then
                        ProcessingTime = m_stopTime - m_startTime
                    Else
                        ProcessingTime = DateTime.Now.Ticks - m_startTime
                    End If
                End If

                If ProcessingTime < 0 Then ProcessingTime = 0

                Return ProcessingTime / 10000000L
            End Get
        End Property

        Public Overridable ReadOnly Property IsRunning() As Boolean
            Get
                Return (Not m_listenThread Is Nothing)
            End Get
        End Property

        Public Overridable ReadOnly Property ThreadsInUse() As Integer
            Get
                If IsRunning Then
                    Return m_clientThreads.Count + 1
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overridable Sub Start()

            If IsRunning Then
                Throw New InvalidOperationException("Listener is already started.")
            Else
                ' Start listening thread
                m_listenThread = RunThread.ExecuteNonPublicMethod(Me, "ListenForConnections")
                m_startTime = DateTime.Now.Ticks
                m_stopTime = 0
            End If

        End Sub

        Public Overridable Sub [Stop]() Implements IDisposable.Dispose

            ' Stop listening thread
            If IsRunning Then
                m_listenThread.Abort()
                m_listenThread = Nothing
            End If
            m_stopTime = DateTime.Now.Ticks

            ' Stop any processing threads
            SyncLock m_clientThreads.SyncRoot
                For Each de As DictionaryEntry In m_clientThreads
                    CType(de.Value, RunThread).Abort()
                Next

                m_clientThreads.Clear()
            End SyncLock

        End Sub

        ' This procedure is meant to be executed on a seperate thread...
        Private Sub ListenForConnections()

            If m_protocol = NetworkProtocol.Tcp Then
                ' When we are in TCP mode, we can accept connections from multiple clients
                Dim listener As New TcpListener(Dns.Resolve(System.Environment.MachineName).AddressList(0), m_serverPort)

                Try
                    Dim client As TcpClient

                    ' Start the web server
                    listener.Start()

                    ' Enter the listening loop
                    m_updateStatus(Name & " started")

                    Do While True
                        ' Block thread until next client connection accepted...
                        client = listener.AcceptTcpClient()

                        ' Process client request on an independent thread so we can keep listening for new requests
                        SyncLock m_clientThreads.SyncRoot
                            Dim threadID As Guid = Guid.NewGuid
                            m_clientThreads.Add(threadID, RunThread.ExecuteNonPublicMethod(Me, "AcceptTcpClientData", client, threadID))
                        End SyncLock

                        m_updateStatus("New TCP client connection established for " & Name & "...")
                    Loop
                Catch ex As Exception
                    m_updateStatus(Name & " exception: " & ex.Message)
                Finally
                    If Not listener Is Nothing Then listener.Stop()
                    m_updateStatus(Name & " stopped")
                End Try
            Else
                ' In UDP mode, we just listen on a single connection using the specified port
                Dim udpSocket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                Dim remoteEP As System.Net.EndPoint = CType(New IPEndPoint(IPAddress.Any, m_serverPort), System.Net.EndPoint)

                udpSocket.Bind(remoteEP)
                m_updateStatus("New UDP client connection established for " & Name & "...")

                ' Enter the data read loop
                Do While Enabled
                    Try
                        With New UdpClientData(m_udpBufferSize)
                            ' Block thread until we've read some data...
                            .Read = udpSocket.ReceiveFrom(.Buffer, remoteEP)

                            ' Once we have any data, we'll queue it up for processing and get back to listening as quickly as possible...
                            ThreadPool.QueueUserWorkItem(AddressOf AcceptUdpClientData, .This)
                        End With
                    Catch ex As ThreadAbortException
                        ' If we received an abort exception, we'll egress gracefully
                        Exit Do
                    Catch ex As IOException
                        ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                        ' in this case we'll bow out gracefully as well...
                        Exit Do
                    Catch ex As Exception
                        m_updateStatus("Exception occurred for " & Name & " while accepting client data: " & ex.Message)
                        m_packetsRejected += 1
                    End Try
                Loop

                ' Close client connection
                If Not udpSocket Is Nothing Then
                    udpSocket.Shutdown(SocketShutdown.Receive)
                    udpSocket.Close()
                End If
            End If

        End Sub

        Private Class UdpClientData

            Public Buffer As Byte()
            Public Read As Integer

            Public Sub New(ByVal udpBufferSize As Integer)

                Buffer = Array.CreateInstance(GetType(Byte), udpBufferSize)

            End Sub

            Public ReadOnly Property This() As Object
                Get
                    Return Me
                End Get
            End Property

        End Class

        Private Sub AcceptUdpClientData(ByVal stateInfo As Object)

            With DirectCast(stateInfo, UdpClientData)
                If .Read > 0 Then
                    m_processBuffer(.Buffer, .Read)
                    m_bytesReceived += .Read
                    m_packetsAccepted += 1
                End If
            End With

        End Sub

        ' This procedure is meant to be executed on a seperate thread...
        Private Sub AcceptTcpClientData(ByVal client As TcpClient, ByVal threadID As Guid)

            Dim clientStream As NetworkStream
            Dim clientData As MemoryStream
            Dim eventBuffer As Byte()
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim read As Integer

            ' Open client data stream...
            client.SendBufferSize = 3
            clientStream = client.GetStream()
            clientData = New MemoryStream

            ' Enter the data read loop
            Do While Enabled
                Try
                    Do
                        ' Block thread until we've read some data...
                        read = clientStream.Read(buffer, 0, BufferSize)
                        clientData.Write(buffer, 0, read)
                    Loop While clientStream.DataAvailable

                    ' Get all data acquired from TCP client data stream
                    eventBuffer = clientData.ToArray()
                    clientData = New MemoryStream

                    ' If we get no data we'll just go back to listening...
                    If eventBuffer.Length > 0 Then
                        ' Process the network data packet...
                        m_bytesReceived += eventBuffer.Length
                        m_processBuffer(eventBuffer, eventBuffer.Length)
                        m_packetsAccepted += 1
                    End If
                Catch ex As ThreadAbortException
                    ' If we received an abort exception, we'll egress gracefully
                    Exit Do
                Catch ex As IOException
                    ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                    ' in this case we'll bow out gracefully as well...
                    Exit Do
                Catch ex As Exception
                    m_updateStatus("Exception occurred for " & Name & " while accepting client data: " & ex.Message)
                    m_packetsRejected += 1
                End Try
            Loop

            ' Thread is complete, remove it from the running thread list
            SyncLock m_clientThreads.SyncRoot
                m_clientThreads.Remove(threadID)
            End SyncLock

            ' Close client connection
            If Not client Is Nothing Then client.Close()

        End Sub

        Private Sub SendResponse(ByVal dataStream As NetworkStream, ByVal response As String)

            Dim responseData As Byte() = Encoding.Default.GetBytes(response)
            dataStream.Write(responseData, 0, responseData.Length)

        End Sub

        Public Overridable ReadOnly Property Name() As String
            Get
                Return "Socket Listener on Port " & m_serverPort
            End Get
        End Property

        Public Overridable ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("     Socket listener state: " & IIf(IsRunning, "Running", "Stopped") & vbCrLf)
                    .Append("          Network protocol: " & [Enum].GetName(GetType(NetworkProtocol), m_protocol) & vbCrLf)
                    .Append("         Listening on port: " & m_serverPort & vbCrLf)
                    .Append("            Listening time: " & SecondsToText(RunTime) & vbCrLf)
                    .Append("            Threads in use: " & ThreadsInUse & vbCrLf)
                    .Append("    Event packets accepted: " & m_packetsAccepted & vbCrLf)
                    .Append("    Event packets rejected: " & m_packetsRejected & vbCrLf)
                    .Append("     Packet broadcast rate: " & m_packetsAccepted / RunTime & " packets/sec" & vbCrLf)
                    .Append("       Data broadcast rate: " & (m_bytesReceived * 8 / RunTime / 1024 / 1024).ToString("0.00") & " mbps" & vbCrLf)
                    .Append("    Total broadcast volume: " & (m_bytesReceived / 1024 / 1024).ToString("0.00") & " Mb" & vbCrLf)

                    Return .ToString()
                End With
            End Get
        End Property

    End Class

End Namespace