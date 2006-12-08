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

Imports System.Text
Imports System.Data
Imports System.Data.SqlClient
Imports Tva.Common
Imports Tva.DateTime.Common
Imports Tva.Data.Common
Imports Tva.Phasors
Imports Tva.Communication
Imports Tva.Measurements

Public Class PhasorMeasurementReceiver

    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_reportingStatus As Timers.Timer
    Private m_connectString As String
    Private m_processedMeasurements As Long
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_statusInterval As Integer
    Private m_intializing As Boolean

    Public Sub New(ByVal archiverIP As String, ByVal connectString As String, ByVal statusInterval As Integer)

        m_connectString = connectString
        m_statusInterval = statusInterval
        m_reportingStatus = New Timers.Timer

        With m_reportingStatus
            .AutoReset = True
            .Interval = 1000
            .Enabled = True
        End With

    End Sub

    Public Sub Connect()

        ' Loop through each defined historian connection defined in the database

    End Sub

    Public Sub Disconnect()

        ' Disconnect all defined historian connections

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
            m_intializing = True

            Dim connection As New SqlConnection(m_connectString)
            Dim measurementIDs As New Dictionary(Of String, MeasurementDefinition)
            Dim row As DataRow
            Dim parser As MultiProtocolFrameParser
            Dim source As String
            Dim timezone As String
            Dim pmuIDs As List(Of String)
            Dim x, y As Integer

            m_mappers = New Dictionary(Of String, PhasorMeasurementMapper)

            connection.Open()

            UpdateStatus("Database connection opened...")

            ' Initialize complete measurement ID list
            With RetrieveData("SELECT * FROM IEEEDataConnectionMeasurements", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    With .Rows(x)
                        measurementIDs.Add(.Item("Synonym"), New MeasurementDefinition(.Item("ID"), .Item("Synonym"), .Item("Adder"), .Item("Multiplier")))
                    End With
                Next
            End With

            UpdateStatus("Loaded " & measurementIDs.Count & " measurement ID's...")

            ' Initialize each data connection
            With RetrieveData("SELECT * FROM IEEEDataConnections", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New MultiProtocolFrameParser()
                    pmuIDs = New List(Of String)

                    source = row("SourceID").ToString.Trim.ToUpper
                    timezone = row("TimeZone")

                    With parser
                        .PhasorProtocol = [Enum].Parse(GetType(PhasorProtocol), row("DataID"))
                        .TransportProtocol = IIf(String.Compare(row("NTP"), "UDP", True) = 0, TransportProtocol.Udp, TransportProtocol.Tcp)
                        .ConnectionString = "server=" & row("IPAddress") & "; port=" & row("IPPort")
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

                    With New PhasorMeasurementMapper(parser, source, pmuIDs, measurementIDs)
                        ' Add timezone mapping if not UTC...
                        If String.Compare(timezone, "GMT Standard Time", True) <> 0 Then
                            Try
                                .TimeZone = GetWin32TimeZone(timezone)
                            Catch ex As Exception
                                UpdateStatus("Failed to assign timezone offset """ & timezone & """ to PDC/PMU """ & source & """ due to exception: " & ex.Message)
                            End Try
                        End If

                        ' Bubble mapper status messages out to local update status functions
                        AddHandler .ParsingStatus, AddressOf UpdateStatus

                        ' Add mapper to collection
                        m_mappers.Add(source, .This)

                        ' Start connection cycle
                        .Connect()
                    End With
                Next
            End With

            connection.Close()

            UpdateStatus("[" & Now() & "] Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus("[" & Now() & "] ERROR: Phasor measurement receiver failed to initialize: " & ex.Message)
        Finally
            m_intializing = False
        End Try

    End Sub

    Public Sub ArchiveMeasurements()

        Dim measurements As New List(Of IMeasurement)

        ' Extract all queued data frames from the data parsers
        For Each mapper As PhasorMeasurementMapper In m_mappers.Values
            ' Get all queued frames in this parser
            For Each frame As IFrame In mapper.GetQueuedFrames()
                ' Extract each measurement from the frame and add queue up for processing
                For Each measurement As IMeasurement In frame.Measurements.Values
                    measurements.Add(measurement)

                    m_processedMeasurements += 1
                    If m_processedMeasurements Mod 100000 = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been processed so far...")
                Next
            Next
        Next

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

                ' TODO: Append status from all adpaters

                Return .ToString()
            End With
        End Get
    End Property

    Private Sub UpdateStatus(ByVal status As String)

        RaiseEvent StatusMessage(status)

    End Sub

    Private Sub m_reportingStatus_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_reportingStatus.Elapsed

        If Not m_intializing Then
            Try
                Dim connection As New SqlConnection(m_connectString)
                Dim updateSqlBatch As New StringBuilder
                Dim isReporting As Integer

                connection.Open()

                ' Check all PMU's for "reporting status"...
                For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                    isReporting = IIf(Math.Abs(DateTime.UtcNow.Subtract(New DateTime(mapper.LastReportTime)).Seconds) <= m_statusInterval, 1, 0)

                    For Each pmu As String In mapper.PmuIDs
                        updateSqlBatch.Append("UPDATE PMUs SET IsReporting=" & isReporting & " WHERE PMUID_Uniq='" & pmu & "';" & Environment.NewLine)
                    Next
                Next

                ' Update reporting status for each PMU
                ExecuteNonQuery(updateSqlBatch.ToString(), connection, 30)

                connection.Close()
            Catch ex As Exception
                UpdateStatus("[" & Now() & "] ERROR: Failed to update PMU reporting status due to exception: " & ex.Message)
            End Try
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