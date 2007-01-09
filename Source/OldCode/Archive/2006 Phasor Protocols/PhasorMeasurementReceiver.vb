'*******************************************************************************************************
'  PhasorMeasurementReceiver.vb - Phasor measurement acquisition class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
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
Imports System.Data.SqlClient
Imports Tva.DateTime.Common
Imports Tva.Data.Common
Imports Tva.Phasors
Imports Tva.Phasors.Common
Imports Tva.Communication
Imports Tva.Measurements
Imports InterfaceAdapters

Public Class PhasorMeasurementReceiver

    Public Event NewMeasurements(ByVal measurements As Dictionary(Of MeasurementKey, IMeasurement))
    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_reportingStatus As Timers.Timer
    Private WithEvents m_historianAdapter As IHistorianAdapter
    Private m_archiverSource As String
    Private m_connectionString As String
    Private m_dataLossInterval As Integer
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_calculatedMeasurements As ICalculatedMeasurementAdapter()
    Private m_measurementBuffer As List(Of IMeasurement)
    Private m_statusInterval As Integer
    Private m_intializing As Boolean

    Public Sub New( _
        ByVal historianAdapter As IHistorianAdapter, _
        ByVal archiverSource As String, _
        ByVal statusInterval As Integer, _
        ByVal connectionString As String, _
        ByVal dataLossInterval As Integer, _
        ByVal calculatedMeasurements As ICalculatedMeasurementAdapter())

        m_historianAdapter = historianAdapter
        m_archiverSource = archiverSource
        m_statusInterval = statusInterval
        m_connectionString = connectionString
        m_dataLossInterval = dataLossInterval
        m_calculatedMeasurements = calculatedMeasurements
        m_measurementBuffer = New List(Of IMeasurement)
        m_reportingStatus = New Timers.Timer

        With m_reportingStatus
            .AutoReset = True
            .Interval = 10000
            .Enabled = False
        End With

    End Sub

    Public Sub Connect()

        m_historianAdapter.Connect()
        m_reportingStatus.Enabled = True

    End Sub

    Public Sub Disconnect()

        m_reportingStatus.Enabled = False

        ' Disconnect from PDC/PMU devices...
        If m_mappers IsNot Nothing Then
            For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                mapper.Disconnect()
            Next
        End If

        m_mappers = Nothing

        m_historianAdapter.Disconnect()

    End Sub

    Public Sub Initialize(ByVal connection As SqlConnection)

        ' Disconnect archiver and all phasor measurement mappers...
        Disconnect()

        ' Restart connect cycle to archiver
        Connect()

        UpdateStatus("Initializing phasor measurement receiver...")

        Try
            m_intializing = True

            Dim measurementIDs As New Dictionary(Of String, IMeasurement)
            Dim row As DataRow
            Dim parser As MultiProtocolFrameParser
            Dim source As String
            Dim timezone As String
            Dim timeAdjustmentTicks As Long
            Dim accessID As Integer
            Dim pmuIDs As PmuInfoCollection
            Dim x, y As Integer

            SyncLock m_measurementBuffer
                m_measurementBuffer.Clear()
            End SyncLock

            m_mappers = New Dictionary(Of String, PhasorMeasurementMapper)

            ' Initialize complete measurement list for this archive keyed on the synonym field
            With RetrieveData("SELECT * FROM IEEEDataConnectionMeasurements WHERE PlantCode='" & m_archiverSource & "'", connection)
                For x = 0 To .Rows.Count - 1
                    With .Rows(x)
                        measurementIDs.Add(.Item("Synonym"), _
                            New Measurement( _
                                Convert.ToInt32(.Item("ID")), _
                                m_archiverSource, _
                                .Item("Synonym").ToString(), _
                                Convert.ToDouble(.Item("Adder")), _
                                Convert.ToDouble(.Item("Multiplier"))))
                    End With
                Next
            End With

            UpdateStatus("Loaded " & measurementIDs.Count & " measurement ID's...")

            ' Initialize each data connection
            With RetrieveData("SELECT * FROM IEEEDataConnections WHERE PlantCode='" & m_archiverSource & "' OR SourceID IN (SELECT PDCID FROM IEEEDataConnectionPDCPMUs WHERE PlantCode='" & m_archiverSource & "')", connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New MultiProtocolFrameParser
                    pmuIDs = New PmuInfoCollection

                    source = row("SourceID").ToString.Trim.ToUpper
                    timezone = row("TimeZone")
                    timeAdjustmentTicks = row("TimeAdjustmentTicks")
                    accessID = row("AccessID")

                    ' Setup phasor frame parser
                    With parser
                        Try
                            .PhasorProtocol = [Enum].Parse(GetType(PhasorProtocol), row("DataID"), True)
                        Catch ex As ArgumentException
                            UpdateStatus("Unexpected phasor protocol encountered for """ & source & """: " & row("DataID") & " - defaulting to IEEE C37.118 V1.")
                            .PhasorProtocol = PhasorProtocol.IeeeC37_118V1
                        End Try

                        ' TODO: This will need to modified to execute like the above if serial connections should be allowed
                        .TransportProtocol = IIf(String.Compare(row("NTP"), "UDP", True) = 0, TransportProtocol.Udp, TransportProtocol.Tcp)

                        If .TransportProtocol = TransportProtocol.Tcp Then
                            .ConnectionString = "server=" & row("IPAddress") & "; port=" & row("IPPort")
                            .DeviceSupportsCommands = True
                        Else
                            ' TODO: May need to account for UDP connections supporting remote server commands at some point
                            ' Note that this will require an extra database field for remote port...
                            .ConnectionString = "localport=" & row("IPPort")
                            .DeviceSupportsCommands = False

                            ' Example UDP connect string supporting remote UDP commands
                            '.ConnectionString = "server=" & row("IPAddress") & "; localport=" & row("IPPort") & "; remoteport=" & row("IPCommandPort")
                            '.DeviceSupportsCommands = True
                        End If

                        .PmuID = accessID
                        .SourceName = source
                    End With

                    If row("IsConcentrator") = 1 Then
                        UpdateStatus("Loading expected PMU list for """ & source & """:")

                        ' Making a connection to a concentrator - this may support multiple PMU's
                        With RetrieveData("SELECT PMUIndex, PMUID FROM IEEEDataConnectionPMUs WHERE PlantCode='" & m_archiverSource & "' AND PDCID='" & source & "' ORDER BY PMUIndex", connection)
                            For y = 0 To .Rows.Count - 1
                                With .Rows(y)
                                    pmuIDs.Add(New PmuInfo(.Item("PMUIndex"), .Item("PMUID")))
                                End With
                                UpdateStatus("   >> " & pmuIDs(y).Tag)
                            Next
                        End With
                    Else
                        ' Making a connection to a single device
                        pmuIDs.Add(New PmuInfo(accessID, source))
                    End If

                    SyncLock m_measurementBuffer
                        With New PhasorMeasurementMapper(parser, m_archiverSource, source, pmuIDs, measurementIDs, m_dataLossInterval)
                            ' Add timezone mapping if not UTC...
                            If String.Compare(timezone, "GMT Standard Time", True) <> 0 Then
                                Try
                                    .TimeZone = GetWin32TimeZone(timezone)
                                Catch ex As Exception
                                    UpdateStatus("Failed to assign timezone offset """ & timezone & """ to PDC/PMU """ & source & """ due to exception: " & ex.Message)
                                End Try
                            End If

                            ' Define time adjustment ticks
                            .TimeAdjustmentTicks = timeAdjustmentTicks

                            ' Bubble mapper status messages out to local update status function
                            AddHandler .ParsingStatus, AddressOf UpdateStatus

                            ' Bubble newly parsed measurements out to functions that need the real-time data
                            AddHandler .NewParsedMeasurements, AddressOf NewParsedMeasurements

                            ' Add mapper to collection
                            m_mappers.Add(source, .This)

                            ' Start connection cycle
                            .Connect()
                        End With
                    End SyncLock
                Next
            End With

            UpdateStatus("Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus("Phasor measurement receiver failed to initialize: " & ex.Message)
        Finally
            m_intializing = False
        End Try

    End Sub

    Public ReadOnly Property HistorianName() As String
        Get
            Return m_historianAdapter.Name
        End Get
    End Property

    Public ReadOnly Property Mappers() As Dictionary(Of String, PhasorMeasurementMapper)
        Get
            Return m_mappers
        End Get
    End Property

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("Phasor Measurement Receiver Status for """ & HistorianName & """")
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append(m_historianAdapter.Status)

                For Each parser As PhasorMeasurementMapper In m_mappers.Values
                    .Append(parser.Status)
                    .Append(Environment.NewLine)
                Next

                Return .ToString()
            End With
        End Get
    End Property

    Public Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement)

        ' Filter incoming measurements to just the ones destined for this archive
        With measurement
            If String.Compare(.Source, m_archiverSource, True) = 0 Then
                m_historianAdapter.QueueMeasurementForArchival(.This)
            End If
        End With

    End Sub

    Public Sub QueueMeasurementsForArchival(ByVal measurements As IList(Of IMeasurement))

        For x As Integer = 0 To measurements.Count - 1
            QueueMeasurementForArchival(measurements(x))
        Next

    End Sub

    Public Sub QueueMeasurementsForArchival(ByVal measurements As IDictionary(Of MeasurementKey, IMeasurement))

        For Each measurement As IMeasurement In measurements.Values
            QueueMeasurementForArchival(measurement)
        Next

    End Sub

    Private Sub UpdateStatus(ByVal status As String) Handles m_historianAdapter.StatusMessage

        RaiseEvent StatusMessage("[" & m_archiverSource & "]: " & status)

    End Sub

    Private Sub NewParsedMeasurements(ByVal measurements As Dictionary(Of MeasurementKey, IMeasurement))

        ' Provide parsed measurements "directly" to all calculated measurement modules
        If m_calculatedMeasurements IsNot Nothing Then
            For x As Integer = 0 To m_calculatedMeasurements.Length - 1
                m_calculatedMeasurements(x).QueueMeasurementsForCalculation(measurements)
            Next
        End If

        ' Queue all of the measurements up for archival
        m_historianAdapter.QueueMeasurementsForArchival(measurements)

        ' Bubble real-time parsed measurements outside of receiver as needed...
        RaiseEvent NewMeasurements(measurements)

    End Sub

    Private Sub m_reportingStatus_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_reportingStatus.Elapsed

        If Not m_intializing Then
            Dim connection As New SqlConnection(m_connectionString)

            Try
                Dim updateSqlBatch As New StringBuilder
                Dim isReporting As Integer

                connection = New SqlConnection(m_connectionString)
                connection.Open()

                ' Check all PMU's for "reporting status"...
                For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                    For Each pmuID As PmuInfo In mapper.PmuIDs
                        If Not String.IsNullOrEmpty(pmuID.Tag) Then
                            isReporting = IIf(Math.Abs(DateTime.UtcNow.Subtract(New DateTime(pmuID.LastReportTime)).Seconds) <= m_statusInterval, 1, 0)
                            updateSqlBatch.Append("UPDATE PMUs SET IsReporting=" & isReporting & ", ReportTime='" & DateTime.UtcNow.ToString() & "' WHERE PMUID_Uniq='" & pmuID.Tag & "'; " & Environment.NewLine)
                        End If
                    Next
                Next

                ' Update reporting status for each PMU
                ExecuteNonQuery(updateSqlBatch.ToString(), connection)
            Catch ex As Exception
                UpdateStatus("[" & Now() & "] ERROR: Failed to update PMU reporting status due to exception: " & ex.Message)
            Finally
                If connection IsNot Nothing Then connection.Close()
            End Try
        End If

    End Sub

    Private Sub m_historianAdapter_ArchivalException(ByVal source As String, ByVal ex As System.Exception) Handles m_historianAdapter.ArchivalException

        UpdateStatus(source & """ data archival exception: " & ex.Message)

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