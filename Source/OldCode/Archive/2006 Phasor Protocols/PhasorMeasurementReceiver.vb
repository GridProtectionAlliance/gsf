'*******************************************************************************************************
'  PhasorMeasurementReceiver.vb - Phasor measurement acquisition class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/19/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Data
Imports System.Data.SqlClient
Imports System.Threading
Imports System.Net.Sockets
Imports Microsoft.Win32
Imports Tva.Common
Imports Tva.Collections.Common
Imports Tva.Configuration.Common
Imports Tva.Data.Common
Imports Tva.DatAWare
Imports Tva.Phasors
Imports Tva.Phasors.Common
Imports Tva.Measurements

Public Class PhasorMeasurementReceiver

    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_connectionTimer As Timers.Timer
    Private m_connectString As String
    Private m_archiverIP As String
    Private m_archiverPort As Integer
    Private m_tcpSocket As TcpClient
    Private m_clientStream As NetworkStream
    Private m_socketThread As Thread
    Private m_maximumEvents As Integer
    Private m_useTimeout As Boolean
    Private m_bufferSize As Integer
    Private m_pollEvents As Long
    Private m_processedMeasurements As Long
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_measurementBuffer As List(Of IMeasurement)

    Public Sub New(ByVal archiverIP As String, ByVal connectString As String)

        m_archiverIP = archiverIP
        m_connectString = connectString
        m_measurementBuffer = New List(Of IMeasurement)
        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 1
            .Enabled = False
        End With

        ' Archiver Settings Path: HKEY_LOCAL_MACHINE\SOFTWARE\DatAWare\Interface Configuration\
        With Registry.LocalMachine.OpenSubKey("SOFTWARE\DatAWare\Interface Configuration")
            m_archiverPort = .GetValue("ArchiverPort", 1002)
            m_maximumEvents = .GetValue("MaxPktSize", 50)
            m_useTimeout = Convert.ToBoolean(Convert.ToInt32(.GetValue("UseTimeout", -1)))
            .Close()
        End With

        If String.IsNullOrEmpty(m_archiverIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to Archiver without specifing a host IP")
        If m_archiverPort = 0 Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to Archiver without specifing a port")

        m_bufferSize = StandardEvent.BinaryLength * m_maximumEvents

    End Sub

    Public Sub Connect()

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Public Sub Disconnect()

        Try
            If m_socketThread IsNot Nothing AndAlso m_socketThread.IsAlive Then m_socketThread.Abort()
            m_socketThread = Nothing

            If m_tcpSocket IsNot Nothing AndAlso m_tcpSocket.Connected Then m_tcpSocket.Close()
            m_tcpSocket = Nothing

            m_clientStream = Nothing
        Catch ex As Exception
            UpdateStatus("Exception occured during disconnect from Archiver: " & ex.Message)
        End Try

    End Sub

    Public Sub DisconnectAll()

        ' Disconnect from PDC/PMU devices...
        If m_mappers IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                mapper.Disconnect()
            Next
        End If

        m_mappers = Nothing
        Disconnect()

    End Sub

    Public Sub Initialize()

        ' Disconnect archiver and all phasor measurement mappers...
        DisconnectAll()

        ' Restart connect cycle to archiver
        Connect()

        UpdateStatus("[" & Now() & "] Initializing phasor measurement receiver...")

        Try
            Dim connection As New SqlConnection(m_connectString)
            Dim measurementIDs As New Dictionary(Of String, Integer)
            Dim row As DataRow
            Dim parser As FrameParser
            Dim source As String
            Dim pmuIDs As List(Of String)
            Dim x, y As Integer

            SyncLock m_measurementBuffer
                m_measurementBuffer.Clear()
            End SyncLock

            m_mappers = New Dictionary(Of String, PhasorMeasurementMapper)

            connection.Open()

            UpdateStatus("Database connection opened...")

            ' Initialize measurement ID list
            With RetrieveData("SELECT * FROM IEEEDataConnectionMeasurements", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    With .Rows(x)
                        measurementIDs.Add(.Item("Synonym"), .Item("ID"))
                    End With
                Next
            End With

            UpdateStatus("Loaded " & measurementIDs.Count & " measurement ID's...")

            ' Initialize each data connection
            With RetrieveData("SELECT * FROM IEEEDataConnections", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New FrameParser()
                    source = row("SourceID").ToString.Trim.ToUpper
                    pmuIDs = New List(Of String)

                    With parser
                        ' TODO: Change database entries to match Enumeration Names - then do lookup:
                        '.Protocol = [Enum].Parse(GetType(Protocol), row("DataID<name>"))
                        .Protocol = Math.Abs(5 - Convert.ToInt32(row("DataID")))
                        .TransportLayer = IIf(String.Compare(row("NTP"), "UDP", True) = 0, DataTransportLayer.Udp, DataTransportLayer.Tcp)
                        .HostIP = row("IPAddress")
                        .Port = row("IPPort")
                        .PmuID = row("AccessID")
                        .SourceName = source
                    End With

                    If row("IsConcentrator") = 1 Then
                        UpdateStatus("Loading expected PMU list for """ & source & """:")

                        ' Making a connection to a concentrator - this may support multiple PMU's
                        With RetrieveData("SELECT PMUID FROM IEEEDataConnectionPMUs WHERE PDCID='" & source & "' ORDER BY PMUIndex", connection)
                            For y = 0 To .Rows.Count - 1
                                pmuIDs.Add(.Rows(y)("PMUID"))
                                UpdateStatus("   >> " & pmuIDs(y))
                            Next
                        End With
                    Else
                        ' Making a connection to a single device
                        pmuIDs.Add(source)
                    End If

                    ' UPDATE: Could compile these "special" case protocol "fixers" into indivdual DLL's
                    ' with an interface that could provide enough info to handle mapping
                    SyncLock m_measurementBuffer
                        Select Case source
                            Case "CONED"
                                With New ConEdPhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                                    AddHandler .ParsingStatus, AddressOf UpdateStatus
                                    m_mappers.Add(source, .This)
                                    .Connect()
                                End With
                            Case "AEP"
                                With New AEPPhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                                    AddHandler .ParsingStatus, AddressOf UpdateStatus
                                    m_mappers.Add(source, .This)
                                    .Connect()
                                End With
                            Case "ARPN"
                                With New ATCPhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                                    AddHandler .ParsingStatus, AddressOf UpdateStatus
                                    m_mappers.Add(source, .This)
                                    .Connect()
                                End With
                            Case Else
                                With New PhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                                    AddHandler .ParsingStatus, AddressOf UpdateStatus
                                    m_mappers.Add(source, .This)
                                    .Connect()
                                End With
                        End Select
                    End SyncLock
                Next
            End With

            connection.Close()

            UpdateStatus("[" & Now() & "] Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus("[" & Now() & "] ERROR: Phasor measurement receiver failed to initialize: " & ex.Message)
        End Try

    End Sub

    Public ReadOnly Property Mappers() As Dictionary(Of String, PhasorMeasurementMapper)
        Get
            Return m_mappers
        End Get
    End Property

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder
                For Each parser As PhasorMeasurementMapper In m_mappers.Values
                    .Append(parser.Status())
                    .Append(Environment.NewLine)
                Next

                Return .ToString()
            End With
        End Get
    End Property

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed

        Try
            UpdateStatus("Starting connection attempt to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """...")

            ' Connect to DatAWare archiver using TCP
            m_tcpSocket = New TcpClient
            m_tcpSocket.Connect(m_archiverIP, m_archiverPort)
            m_clientStream = m_tcpSocket.GetStream()
            m_clientStream.WriteTimeout = 100
            m_clientStream.ReadTimeout = 10

            ' Start listening to TCP data stream
            m_socketThread = New Thread(AddressOf ProcessTcpStream)
            m_socketThread.Start()

            UpdateStatus("Connection to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """ established.")
        Catch ex As Exception
            UpdateStatus(">> WARNING: Connection to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """ failed: " & ex.Message)
            Connect()
        End Try

    End Sub

    Private Sub ProcessTcpStream()

        Const statusInterval As Integer = 1000
        Const dumpInterval As Integer = 5000
        Const postDumpCount As Integer = 100

        Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        Dim events As StandardEvent()
        Dim received As Integer
        Dim response As String
        Dim pollEvents As Long

        ' Enter the data read loop
        Do While True
            Try
                pollEvents = 0

                Do
                    events = LoadEvents()

                    If events IsNot Nothing Then
                        ' Load binary standard event images into local buffer
                        For x As Integer = 0 To events.Length - 1
                            System.Buffer.BlockCopy(events(x).BinaryImage, 0, buffer, x * StandardEvent.BinaryLength, StandardEvent.BinaryLength)
                        Next

                        ' Post data to TCP stream
                        m_clientStream.Write(buffer, 0, events.Length * StandardEvent.BinaryLength)

                        If m_useTimeout Then
                            Try
                                ' Wait for acknowledgement (limited to readtimeout)...
                                received = m_clientStream.Read(buffer, 0, buffer.Length)

                                ' Interpret response as a string
                                response = Encoding.Default.GetString(buffer, 0, received)

                                ' Verify archiver response
                                If Not response.StartsWith("ACK", True, Nothing) Then Throw New InvalidOperationException("DatAWare archiver failed to acknowledge packet transmission: " & response)
                            Catch ex As IOException
                                UpdateStatus(">> WARNING: Timed-out waiting on acknowledgement from archiver...")
                            Catch
                                Throw
                            End Try
                        Else
                            ' We sleep between data polls to prevent CPU loading
                            Thread.Sleep(1)
                        End If
                    End If

                    ' We shouldn't stay in this loop forever (this would mean we're falling behind) so we broadcast the status of things...
                    pollEvents += 1
                    If pollEvents Mod statusInterval = 0 Then
                        UpdateStatus(">> WARNING: " & m_measurementBuffer.Count.ToString("#,##0") & " measurements remain in the queue to be sent...")
                        If m_measurementBuffer.Count > dumpInterval Then Exit Do
                    End If
                Loop While m_measurementBuffer.Count > 0

                ' We're getting behind, must dump measurements :(
                If m_measurementBuffer.Count > dumpInterval Then
                    SyncLock m_measurementBuffer
                        ' TODO: When this starts happening - you've overloaded the real-time capacity of your historian
                        ' and you must do something about it - more hardware, scale out, etc. Make sure to log this
                        ' error externally (alarm or something) so things can be fixed...
                        UpdateStatus(">> ERROR: Dumping " & (m_measurementBuffer.Count - postDumpCount).ToString("#,##0") & " measurements because we're falling behind :(")
                        m_measurementBuffer.RemoveRange(0, m_measurementBuffer.Count - postDumpCount)
                    End SyncLock
                ElseIf pollEvents > statusInterval Then
                    ' Send final status message - warning terminated...
                    UpdateStatus(">> INFO: Warning state terminated - all queued measurements have been sent")
                End If
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As Exception
                UpdateStatus("Archiver connection exception: " & ex.Message)
                Connect()
                Exit Do
            End Try
        Loop

    End Sub

    Private Function LoadEvents() As StandardEvent()

        Dim events As StandardEvent() = Nothing

        SyncLock m_measurementBuffer
            ' Extract all queued data frames from the data parsers
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                ' Get all queued frames in this parser
                For Each frame As IFrame In mapper.GetQueuedFrames()
                    ' Extract each measurement from the frame and add queue up for processing
                    For Each measurement As IMeasurement In frame.Measurements.Values
                        m_measurementBuffer.Add(measurement)

                        m_processedMeasurements += 1
                        If m_processedMeasurements Mod 100000 = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been uploaded so far...")
                    Next
                Next
            Next

            Dim totalEvents As Integer = Minimum(m_maximumEvents, m_measurementBuffer.Count)

            If totalEvents > 0 Then
                ' Create standard DatAWare event array of all points to be processed
                events = CreateArray(Of StandardEvent)(totalEvents)

                For x As Integer = 0 To totalEvents - 1
                    events(x) = New StandardEvent(m_measurementBuffer(x))
                Next

                ' Remove measurements being processed
                m_measurementBuffer.RemoveRange(0, totalEvents)
            End If
        End SyncLock

        Return events

    End Function

    Private Sub UpdateStatus(ByVal status As String)

        RaiseEvent StatusMessage(status)

    End Sub

End Class

#Region " Old Code "

'Public Sub Poll(ByRef IntIPBuf() As Byte, ByRef nBytes As Integer, ByRef iReturn As Integer, ByRef Status As Integer)

'    If m_mappers Is Nothing Then Initialize()

'    Try

'        If iReturn = -2 Then
'            ' Intialization request poll event
'            Busy = True
'            m_pollEvents += 1

'            nBytes = FillIPBuffer(IntIPBuf, LoadDatabase())
'            iReturn = 1

'            UpdateStatus("Local database intialized..." & vbCrLf)
'        Else
'            ' Standard poll event
'            Busy = True
'            m_pollEvents += 1

'            If m_pollEvents Mod 500 = 0 Then UpdateStatus(m_pollEvents & " have been processed...")

'            Dim events As StandardEvent() = LoadEvents()

'            If events IsNot Nothing Then
'                nBytes = FillIPBuffer(IntIPBuf, events)
'            End If

'            If m_measurementBuffer.Count > 0 Or m_lastTotal > 0 Then UpdateStatus("    >> Uploading measurements, " & m_measurementBuffer.Count & " remaining...")
'            If m_measurementBuffer.Count = 0 And m_lastTotal > 0 Then UpdateStatus(Environment.NewLine)
'            m_lastTotal = m_measurementBuffer.Count

'            ' We set iReturn to zero to have DatAWare call the poll event again immediately, else set to one
'            ' (i.e., set to zero if you still have more items in the queue to be processed)
'            iReturn = IIf(m_measurementBuffer.Count > 0, 0, 1)
'        End If
'    Catch ex As Exception
'        UpdateStatus("Exception occured during poll event: " & ex.Message)
'        nBytes = 0
'        Status = 1
'        iReturn = 1
'    Finally
'        Busy = False
'    End Try

'End Sub

'Private Function LoadDatabase() As StandardEvent()

'    Dim standardEvents As New List(Of StandardEvent)

'    For Each measurementID As Integer In m_measurementIDs.Values
'        standardEvents.Add(New StandardEvent(measurementID, New TimeTag(Date.UtcNow), 0.0!, Quality.Good))
'    Next

'    Return standardEvents.ToArray()

'End Function

'Private Sub LoadAssembly(ByVal assemblyName As String)

'    ' Hook into assembly resolve event for current domain so we can load assembly from an alternate path
'    If m_assemblyCache Is Nothing Then
'        ' Create a new assembly cache
'        m_assemblyCache = New Dictionary(Of String, Assembly)

'        ' DAQ Path: HKEY_LOCAL_MACHINE\SOFTWARE\DatAWare\DAQ Configuration\
'        With Registry.LocalMachine.OpenSubKey("SOFTWARE\DatAWare\DAQ Configuration")
'            ' We define alternate path as a subfolder of defined DAQ DLL path...
'            m_assemblyFolder = .GetValue("DAQDLLPath") & "\bin\"
'            .Close()
'        End With

'        ' Add hook into standard assembly resolution
'        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblyFromAlternatePath
'    End If

'    ' Load the assembly (this will invoke event that will resolve assembly from resource)
'    AppDomain.CurrentDomain.Load(assemblyName)

'End Sub

#End Region