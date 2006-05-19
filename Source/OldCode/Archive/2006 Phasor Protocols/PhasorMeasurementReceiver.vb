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

    Private WithEvents m_connectionTimer As Timers.Timer
    Private m_connectString As String
    Private m_archiverIP As String
    Private m_archiverPort As Integer
    Private m_tcpSocket As TcpClient
    Private m_clientStream As NetworkStream
    Private m_socketThread As Thread
    Private m_maximumEvents As Integer
    Private m_bufferSize As Integer
    Private m_pollEvents As Long
    Private m_processedMeasurements As Long
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_measurementBuffer As List(Of IMeasurement)
    Private m_initializing As Boolean

    Public Sub New(ByVal archiverIP As String, ByVal connectString As String)

        m_archiverIP = archiverIP
        m_connectString = connectString
        m_measurementBuffer = New List(Of IMeasurement)
        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 4000
            .Enabled = False
        End With

        ' Archiver Settings Path: HKEY_LOCAL_MACHINE\SOFTWARE\DatAWare\Interface Configuration\
        With Registry.LocalMachine.OpenSubKey("SOFTWARE\DatAWare\Interface Configuration")
            m_archiverPort = .GetValue("ArchiverPort", 1002)
            m_maximumEvents = .GetValue("MaxPktSize", 50)
            .Close()
        End With

        If String.IsNullOrEmpty(m_archiverIP) Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to Archiver without specifing a host IP")
        If m_archiverPort = 0 Then Throw New InvalidOperationException("Cannot start TCP stream listener connection to Archiver without specifing a port")

        m_bufferSize = StandardEvent.BinaryLength * m_maximumEvents

        Connect()

    End Sub

    Public Sub Connect()

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Public Sub Disconnect()

        If m_socketThread IsNot Nothing Then m_socketThread.Abort()
        m_socketThread = Nothing

        If m_tcpSocket IsNot Nothing Then m_tcpSocket.Close()
        m_tcpSocket = Nothing

        m_clientStream = Nothing

    End Sub

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed

        Try
            UpdateStatus("Starting connection attempt to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """...")

            ' Connect to DatAWare archiver using TCP
            m_tcpSocket = New TcpClient
            m_tcpSocket.Connect(m_archiverIP, m_archiverPort)
            m_clientStream = m_tcpSocket.GetStream()

            ' Start listening to TCP data stream
            m_socketThread = New Thread(AddressOf ProcessTcpStream)
            m_socketThread.Start()

            UpdateStatus("Connection to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """ established.")
        Catch ex As Exception
            UpdateStatus("Connection to DatAWare Archiver """ & m_archiverIP & ":" & m_archiverPort & """ failed: " & ex.Message)
            Connect()
        End Try

    End Sub

    Private Sub ProcessTcpStream()

        Dim buffer As Byte() = CreateArray(Of Byte)(m_bufferSize)
        Dim events As StandardEvent()

        ' Enter the data read loop
        Do While True
            Try
                If Not m_initializing Then
                    m_pollEvents += 1
                    If m_pollEvents Mod 5000 = 0 Then UpdateStatus(m_pollEvents & " data polls have been processed...")

                    Do
                        events = LoadEvents()

                        If events IsNot Nothing Then
                            ' Load binary standard event images into local buffer
                            For x As Integer = 0 To events.Length - 1
                                System.Buffer.BlockCopy(events(x).BinaryImage, 0, buffer, x * StandardEvent.BinaryLength, StandardEvent.BinaryLength)
                            Next

                            ' Post data to TCP stream
                            m_clientStream.Write(buffer, 0, events.Length * StandardEvent.BinaryLength)
                        End If
                    Loop While m_measurementBuffer.Count > 0
                End If

                ' We sleep between data polls to prevent CPU loading
                Thread.Sleep(100)
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As IOException
                ' This will get thrown if the thread is being aborted and we are sitting in a blocked stream read, so
                ' in this case we'll bow out gracefully as well...
                Exit Do
            Catch ex As Exception
                UpdateStatus("Archiver connection exception: " & ex.Message)
                Connect()
                Exit Do
            End Try
        Loop

    End Sub

    Public Sub Initialize()

        UpdateStatus("[" & Now() & "] Initializing phasor measurement receiver...")

        Try
            Dim connection As New SqlConnection(m_connectString)
            Dim measurementIDs As New Dictionary(Of String, Integer)
            Dim row As DataRow
            Dim parser As FrameParser
            Dim source As String
            Dim pmuIDs As List(Of String)
            Dim x, y As Integer

            m_initializing = True

            m_measurementBuffer.Clear()
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
                    End With

                    If row("IsConcentrator") = 1 Then
                        UpdateStatus("Loading excepted PMU list for """ & source & """:")

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
                        Case Else
                            With New PhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                                AddHandler .ParsingStatus, AddressOf UpdateStatus
                                m_mappers.Add(source, .This)
                                .Connect()
                            End With
                    End Select
                Next
            End With

            connection.Close()

            UpdateStatus("[" & Now() & "] Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus("[" & Now() & "] ERROR: Phasor measurement receiver failed to initialize: " & ex.Message)
        Finally
            m_initializing = False
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
                        If m_processedMeasurements Mod 100000 = 0 Then UpdateStatus(m_processedMeasurements & " measurements have been uploaded so far...")
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

        UpdateStatus(status, True)

    End Sub

    Private Sub UpdateStatus(ByVal status As String, ByVal newLine As Boolean)

        If newLine Then
            Console.WriteLine(status)
        Else
            Console.Write(status)
        End If

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