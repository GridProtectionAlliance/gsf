'***********************************************************************
'  DatAWare.Listener.vb -  DatAWare Network Packet Listener
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/5/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "DatAWare PDC".
'
'***********************************************************************

Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.Encoding
Imports System.Threading
Imports System.ComponentModel
Imports TVA.Threading
Imports TVA.Config.Common
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports VB = Microsoft.VisualBasic

Namespace DatAWare

    Public Class Listener

        Implements IServiceComponent
        Implements IDisposable

        Private m_parent As DatAWarePDC
        Private m_connection As DatAWare.Connection
        Private m_enabled As Boolean
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_serverPort As Integer
        Private m_listenThread As RunThread
        Private m_clientThreads As Hashtable
        Private m_packetsAccepted As Long
        Private m_packetsRejected As Long

        Private Const BufferSize As Integer = 524288 ' 512Kb buffer

#Region " Setup and Class Definition Code "

        ' Class auto-generated using TVA service template at Fri Nov 5 09:43:23 EST 2004
        Public Sub New(ByVal parent As DatAWarePDC, ByVal serverPort As Integer, ByVal server As String, ByVal plantCode As String, ByVal timeZone As String)

            m_parent = parent
            m_connection = New DatAWare.Connection(server, plantCode, timeZone, DatAWare.AccessMode.ReadOnly)
            m_serverPort = serverPort
            m_enabled = True
            m_clientThreads = New Hashtable

        End Sub

        Protected Overrides Sub Finalize()

            MyBase.Finalize()
            Dispose()

        End Sub

        Public Overridable Sub Dispose() Implements IServiceComponent.Dispose, IDisposable.Dispose

            GC.SuppressFinalize(Me)

            ' Any needed shutdown code for your primary service process should be added here - note that this class
            ' instance is available for the duration of the service lifetime...
            [Stop]()

        End Sub

        ' This function handles updating status for the primary service process
        Public Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False, Optional ByVal EntryType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information)

            m_parent.UpdateStatus(Status, LogStatusToEventLog, EntryType)

        End Sub

        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal Value As Boolean)
                m_enabled = Value

                ' When service is paused we stop listening, we start listening again when service is resumed...
                If m_enabled Then
                    If Not IsRunning Then Start()
                Else
                    If IsRunning Then [Stop]()
                End If
            End Set
        End Property

        Public ReadOnly Property RunTime() As Double
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

#End Region

#Region " Listener Code "

        Public ReadOnly Property Connection() As DatAWare.Connection
            Get
                Return m_connection
            End Get
        End Property

        Public ReadOnly Property IsRunning() As Boolean
            Get
                Return (Not m_listenThread Is Nothing)
            End Get
        End Property

        Public ReadOnly Property ThreadsInUse() As Integer
            Get
                If IsRunning Then
                    Return m_clientThreads.Count + 1
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Sub Start()

            If IsRunning Then
                Throw New InvalidOperationException("Listener is already started.")
            Else
                ' Start listening thread
                m_listenThread = RunThread.ExecuteNonPublicMethod(Me, "ListenForConnections")
                m_startTime = DateTime.Now.Ticks
                m_stopTime = 0
            End If

        End Sub

        Public Sub [Stop]()

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

            Dim listener As New TcpListener(Dns.Resolve(System.Environment.MachineName).AddressList(0), m_serverPort)

            Try
                Dim client As TcpClient

                ' Start the web server
                listener.Start()

                ' Enter the listening loop
                UpdateStatus("Listener started")

                Do While True
                    ' Block thread until next client connection accepted...
                    client = listener.AcceptTcpClient()

                    ' Process client request on an independent thread so we can keep listening for new requests
                    SyncLock m_clientThreads.SyncRoot
                        Dim threadID As Guid = Guid.NewGuid
                        m_clientThreads.Add(threadID, RunThread.ExecuteNonPublicMethod(Me, "AcceptClientData", client, threadID))
                    End SyncLock

                    UpdateStatus("New client connection established...")
                Loop
            Catch ex As Exception
                UpdateStatus("Listener exception: " & ex.Message)
            Finally
                If Not listener Is Nothing Then listener.Stop()
                UpdateStatus("Listener stopped")
            End Try

        End Sub

        ' This procedure is meant to be executed on a seperate thread...
        Private Sub AcceptClientData(ByVal client As TcpClient, ByVal threadID As Guid)

            Dim clientStream As NetworkStream
            Dim clientData As MemoryStream
            Dim eventBuffer As Byte()
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BufferSize)
            Dim read As Integer
            Dim events As Integer
            Dim remainder As Integer
            Dim errorCount As Integer
            Dim errorTime As Long

            ' Open client data stream...
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

                    ' Every so often we get no data, if so we'll just go back to listening...
                    If eventBuffer.Length > 0 Then
                        ' We may not have received an even number of events - so we check for that
                        events = Math.DivRem(eventBuffer.Length, StandardEvent.BinaryLength, remainder)

                        If remainder > 0 Then
                            ' We have a little bit of the next event in this packet, so we'll take care of that...
                            Dim evenLength As Integer = events * StandardEvent.BinaryLength
                            Dim evenPacket As Byte() = Array.CreateInstance(GetType(Byte), evenLength)

                            ' Get all complete event data from this packet for processing...
                            Array.Copy(eventBuffer, 0, evenPacket, 0, evenLength)

                            ' Leave remainder of event data in the buffer for the next pass
                            clientData.Write(eventBuffer, evenLength, eventBuffer.Length - evenLength)

                            eventBuffer = evenPacket
                        End If

                        ' Queue this data packet for concentration...
                        m_parent.EventQueue.QueueEventData(m_connection.PlantCode, eventBuffer)

                        ' If client was finished sending packet, send acknowledgement back to DatAWare...
                        If remainder = 0 Then
                            SendResponse(clientStream, "ACK")
                            m_packetsAccepted += 1
                        End If
                    End If
                Catch ex As ThreadAbortException
                    ' If we received an abort exception, we'll egress gracefully
                    Exit Do
                Catch ex As IO.IOException
                    ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                    ' in this case we'll bow out gracefully as well...
                    Exit Do
                Catch ex As Exception
                    UpdateStatus("Exception occurred while accepting client data: " & ex.Message)

                    ' We monitor for exceptions that occur in quick succession
                    If DateTime.Now.Ticks - errorTime > 100000000L Then
                        errorTime = DateTime.Now.Ticks
                        errorCount = 1
                    End If

                    errorCount += 1

                    ' We close the client connection when we get 5 or more exceptions within a ten second
                    ' timespan, DatAWare will attempt to automatically reconnect in 30 seconds...
                    If errorCount >= 5 Then
                        Try
                            SendResponse(clientStream, "NAK - Client connection terminated due to excessive exceptions.")
                        Catch
                        End Try

                        Exit Do
                    Else
                        Try
                            ' We'll attempt to send an error response back to DatAWare
                            SendResponse(clientStream, "NAK - " & ex.Message)
                        Catch
                        End Try

                        m_packetsRejected += 1
                    End If
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

#End Region

#Region " IService Component Implementation "

        ' Service component implementation
        Public ReadOnly Property Name() As String Implements IServiceComponent.Name
            Get
                Return Me.GetType.Name & " (" & m_serverPort & ")"
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

            ' This class has no interaction with the primary archiving process, so nothing to do...

        End Sub

        Public Sub ServiceStateChanged(ByVal NewState As ServiceState) Implements IServiceComponent.ServiceStateChanged

            ' We respect changes in service state by enabling or disabling our processing state as needed...
            Select Case NewState
                Case ServiceState.Paused
                    Enabled = False
                Case ServiceState.Resumed
                    Enabled = True
            End Select

        End Sub

        Public ReadOnly Property Status() As String Implements IServiceComponent.Status
            Get
                With New StringBuilder
                    .Append("     DatAWare server: " & Connection.Server & vbCrLf)
                    .Append("          Plant code: " & Connection.PlantCode & vbCrLf)
                    .Append("           Time zone: " & Connection.TimeZone.DisplayName & vbCrLf)
                    .Append("   Listening on port: " & m_serverPort & vbCrLf)
                    .Append("      Listener state: " & IIf(IsRunning, "Running", "Stopped") & vbCrLf)
                    .Append("      Listening time: " & SecondsToText(RunTime) & vbCrLf)
                    .Append("      Threads in use: " & ThreadsInUse & vbCrLf)
                    .Append("    Packets Accepted: " & m_packetsAccepted & vbCrLf)
                    .Append("    Packets Rejected: " & m_packetsRejected & vbCrLf)

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

    End Class

End Namespace

