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
Imports System.Data.OleDb
Imports TVA.Common
Imports TVA.DateTime.Common
Imports TVA.Data.Common
Imports TVA.Communication
Imports TVA.Measurements
Imports TVA.Text.Common
Imports TVA.ErrorManagement
Imports InterfaceAdapters
Imports PhasorProtocols
Imports PhasorProtocols.Common

Public Class PhasorMeasurementReceiver

    Public Event NewMeasurements(ByVal measurements As ICollection(Of IMeasurement))
    Public Event StatusMessage(ByVal status As String)

    Private WithEvents m_reportingStatus As Timers.Timer
    Private WithEvents m_historianAdapter As IHistorianAdapter
    Private m_archiverSource As String
    Private m_connectionString As String
    Private m_dataLossInterval As Integer
    Private m_mappers As Dictionary(Of String, PhasorMeasurementMapper)
    Private m_statusInterval As Integer
    Private m_intializing As Boolean
    Private m_exceptionLogger As GlobalExceptionLogger

    Public Sub New( _
        ByVal historianAdapter As IHistorianAdapter, _
        ByVal archiverSource As String, _
        ByVal statusInterval As Integer, _
        ByVal connectionString As String, _
        ByVal dataLossInterval As Integer, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        m_historianAdapter = historianAdapter
        m_archiverSource = archiverSource
        m_statusInterval = statusInterval
        m_connectionString = connectionString
        m_dataLossInterval = dataLossInterval
        m_exceptionLogger = exceptionLogger
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

    Public Sub Initialize(ByVal connection As OleDbConnection)

        ' Disconnect archiver and all phasor measurement mappers...
        Disconnect()

        ' Restart connect cycle to archiver
        Connect()

        UpdateStatus("Initializing phasor measurement receiver...")

        Try
            m_intializing = True

            Dim configurationCells As Dictionary(Of UInt16, ConfigurationCell)
            Dim measurementIDs As New Dictionary(Of String, IMeasurement)
            Dim row As DataRow
            Dim parser As MultiProtocolFrameParser
            Dim source As String
            Dim timezone As String
            Dim timeAdjustmentTicks As Long
            Dim accessID As Integer
            Dim x, y As Integer

            m_mappers = New Dictionary(Of String, PhasorMeasurementMapper)

            ' Initialize each data connection
            'With RetrieveData(String.Format("SELECT * FROM ActiveDeviceConnections WHERE Historian='{0}'", m_archiverSource), connection)
            With RetrieveData(String.Format("SELECT * FROM IEEEDataConnections WHERE PlantCode='{0}' OR SourceID IN (SELECT PDCID FROM IEEEDataConnectionPDCPMUs WHERE PlantCode='{1}')", m_archiverSource, m_archiverSource), connection)
                For x = 0 To .Rows.Count - 1
                    ' Get current row
                    row = .Rows(x)

                    parser = New MultiProtocolFrameParser
                    configurationCells = New Dictionary(Of UInt16, ConfigurationCell)

                    'source = row("Acronym").ToString().Trim().ToUpper()
                    source = row("SourceID").ToString().Trim().ToUpper()
                    timezone = row("TimeZone")
                    'timeAdjustmentTicks = row("TimeOffsetTicks")
                    timeAdjustmentTicks = row("TimeAdjustmentTicks")
                    accessID = row("AccessID")

                    ' Setup phasor frame parser
                    With parser
                        Try
                            '.PhasorProtocol = [Enum].Parse(GetType(PhasorProtocol), row("PhasorProtocol"), True)
                            .PhasorProtocol = [Enum].Parse(GetType(PhasorProtocol), row("DataID"), True)
                        Catch ex As ArgumentException
                            'UpdateStatus(String.Format("Unexpected phasor protocol encountered for ""{0}"": {1} - defaulting to IEEE C37.118 V1.", source, row("PhasorProtocol")))
                            UpdateStatus(String.Format("Unexpected phasor protocol encountered for ""{0}"": {1} - defaulting to IEEE C37.118 V1.", source, row("DataID")))
                            m_exceptionLogger.Log(ex)
                            .PhasorProtocol = PhasorProtocol.IeeeC37_118V1
                        End Try

                        ' TODO: This will need to modified to execute like the above if serial connections should be allowed
                        .TransportProtocol = IIf(String.Compare(row("NTP"), "UDP", True) = 0, TransportProtocol.Udp, TransportProtocol.Tcp)

                        If .TransportProtocol = TransportProtocol.Tcp Then
                            .ConnectionString = String.Format("server={0}; port={1}", row("IPAddress"), row("IPPort"))
                            .DeviceSupportsCommands = True
                        Else
                            ' TODO: May need to account for UDP connections supporting remote server commands at some point
                            ' Note that this will require an extra database field for remote port...
                            .ConnectionString = String.Format("localport={0}", row("IPPort"))
                            .DeviceSupportsCommands = False

                            ' Example UDP connect string supporting remote UDP commands
                            '.ConnectionString = "server=" & row("IPAddress") & "; localport=" & row("IPPort") & "; remoteport=" & row("IPCommandPort")
                            '.DeviceSupportsCommands = True

                            ' Handle special connection information
                            If .PhasorProtocol = PhasorProtocol.BpaPdcStream Then
                                ' BPA PDCstream has special connection needs
                                With DirectCast(.ConnectionParameters, BpaPdcStream.ConnectionParameters)
                                    .ConfigurationFileName = Concat(TVA.IO.FilePath.GetApplicationPath(), row("IPAddress"))
                                    .RefreshConfigurationFileOnChange = True
                                    .ParseWordCountFromByte = False
                                End With
                            End If
                        End If

                        'Try
                        '    .TransportProtocol = [Enum].Parse(GetType(TransportProtocol), row("TransportProtocol"), True)
                        'Catch ex As ArgumentException
                        '    UpdateStatus(String.Format("Unexpected transport protocol encountered for ""{0}"": {1} - defaulting to UDP.", source, row("TransportProtocol")))
                        '    m_exceptionLogger.Log(ex)
                        '    .TransportProtocol = TransportProtocol.Udp
                        'End Try

                        'Dim connectionString As Object = row("ConnectionString")

                        'If connectionString Is Nothing OrElse IsDBNull(connectionString) OrElse String.IsNullOrEmpty(connectionString.ToString()) Then
                        '    ' Use old fields for connections if connection string is not defined...
                        '    If .TransportProtocol = TransportProtocol.Tcp Then
                        '        .ConnectionString = String.Format("server={0}; port={1}", row("IPAddress"), row("IPPort"))
                        '        .DeviceSupportsCommands = True
                        '    Else
                        '        .ConnectionString = String.Format("localport={0}", row("IPPort"))
                        '        .DeviceSupportsCommands = False
                        '    End If
                        'Else
                        '    ' Use connection string if any is defined
                        '    .ConnectionString = row("ConnectionString")
                        'End If

                        '' Handle special connection parameters
                        'If .PhasorProtocol = PhasorProtocol.BpaPdcStream Then
                        '    ' Make sure required INI configuration file parameter gets initialized
                        '    Dim parameters As BpaPdcStream.ConnectionParameters = DirectCast(.ConnectionParameters, BpaPdcStream.ConnectionParameters)
                        '    Dim keys As Dictionary(Of String, String) = ParseKeyValuePairs(.ConnectionString)
                        '    parameters.ConfigurationFileName = keys("inifilename")
                        'End If

                        .DeviceID = accessID
                        .SourceName = source
                    End With

                    If row("IsConcentrator") <> 0 Then
                        UpdateStatus(String.Format("Loading expected PMU list for ""{0}"":", source))

                        Dim loadedPmuStatus As New StringBuilder
                        loadedPmuStatus.Append(Environment.NewLine)
                        ' Making a connection to a concentrator - this may support multiple PMU's
                        'With RetrieveData(String.Format("SELECT AccessID, Acronym FROM PdcPmus WHERE PdcAcronym='{0}' AND Historian='{1}' ORDER BY IOIndex", source, m_archiverSource), connection)
                        With RetrieveData(String.Format("SELECT PMUIndex, PMUID FROM IEEEDataConnectionPMUs WHERE PlantCode='{0}' AND PDCID='{1}' ORDER BY PMUIndex", m_archiverSource, source), connection)
                            For y = 0 To .Rows.Count - 1
                                With .Rows(y)
                                    'pmuIDs.Add(.Item("AccessID"), New PmuInfo(.Item("AccessID"), .Item("Acronym").ToString().Trim().ToUpper()))
                                    configurationCells.Add(.Item("PMUIndex"), New ConfigurationCell(.Item("PMUIndex"), .Item("PMUID").ToString().Trim().ToUpper()))

                                    ' Create status display string for loaded PMU
                                    loadedPmuStatus.Append("   PMU ")
                                    loadedPmuStatus.Append(y.ToString("00"))
                                    loadedPmuStatus.Append(": ")
                                    'loadedPmuStatus.Append(.Item("Acronym"))
                                    loadedPmuStatus.Append(.Item("PMUID"))
                                    loadedPmuStatus.Append(" (")
                                    'loadedPmuStatus.Append(.Item("AccessID"))
                                    loadedPmuStatus.Append(.Item("PMUIndex"))
                                    loadedPmuStatus.Append(")"c)
                                    loadedPmuStatus.Append(Environment.NewLine)
                                End With
                            Next
                        End With
                        UpdateStatus(loadedPmuStatus.ToString())
                    Else
                        ' Making a connection to a single device
                        configurationCells.Add(accessID, New ConfigurationCell(accessID, source))
                    End If

                    ' Initialize measurement list for this device connection keyed on the signal reference field
                    With RetrieveData(String.Format("SELECT * FROM ActiveDeviceMeasurements WHERE Acronym='{0}' AND Historian='{1}'", source, m_archiverSource), connection)
                        For y = 0 To .Rows.Count - 1
                            With .Rows(y)
                                measurementIDs.Add(.Item("SignalReference"), _
                                    New Measurement( _
                                        Convert.ToInt32(.Item("PointID")), _
                                        m_archiverSource, _
                                        .Item("SignalReference").ToString(), _
                                        Convert.ToDouble(.Item("Adder")), _
                                        Convert.ToDouble(.Item("Multiplier"))))
                            End With
                        Next
                    End With

                    UpdateStatus(String.Format("Loaded {0} active measurements for {1}...", measurementIDs.Count, source))

                    With New PhasorMeasurementMapper(parser, m_archiverSource, source, configurationCells, measurementIDs, m_dataLossInterval, m_exceptionLogger)
                        ' Add timezone mapping if not UTC...
                        If String.Compare(timezone, "GMT Standard Time", True) <> 0 Then
                            Try
                                .TimeZone = GetWin32TimeZone(timezone)
                            Catch ex As Exception
                                UpdateStatus(String.Format("Failed to assign timezone offset ""{0}"" to PDC/PMU ""{1}"" due to exception: {2}", timezone, source, ex.Message))
                                m_exceptionLogger.Log(ex)
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
                Next
            End With

            UpdateStatus("Phasor measurement receiver initialized successfully.")
        Catch ex As Exception
            UpdateStatus(String.Format("Phasor measurement receiver failed to initialize: {0}", ex.Message))
            m_exceptionLogger.Log(ex)
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
                .Append(String.Format("Phasor Measurement Receiver Status for ""{0}""", HistorianName))
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
        If String.Compare(measurement.Source, m_archiverSource, True) = 0 Then m_historianAdapter.QueueMeasurementForArchival(measurement)

    End Sub

    Public Sub QueueMeasurementsForArchival(ByVal measurements As ICollection(Of IMeasurement))

        ' Filter incoming measurements to just the ones destined for this archive
        Dim queuedMeasurements As New List(Of IMeasurement)

        For Each measurement As IMeasurement In measurements
            If String.Compare(measurement.Source, m_archiverSource, True) = 0 Then
                queuedMeasurements.Add(queuedMeasurements)
            End If
        Next

        If queuedMeasurements.Count > 0 Then m_historianAdapter.QueueMeasurementsForArchival(queuedMeasurements)

    End Sub

    Private Sub UpdateStatus(ByVal status As String) Handles m_historianAdapter.StatusMessage

        RaiseEvent StatusMessage(String.Format("[{0}]: {1}", m_archiverSource, status))

    End Sub

    Private Sub NewParsedMeasurements(ByVal measurements As ICollection(Of IMeasurement))

        ' Queue all of the measurements up for archival
        m_historianAdapter.QueueMeasurementsForArchival(measurements)

        ' Bubble real-time parsed measurements outside of receiver as needed...
        RaiseEvent NewMeasurements(measurements)

    End Sub

    Private Sub m_reportingStatus_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_reportingStatus.Elapsed

        If Not m_intializing Then
            Dim connection As New OleDbConnection(m_connectionString)

            Try
                Dim isReporting As Integer

                connection = New OleDbConnection(m_connectionString)
                connection.Open()

                ' Check all PMU's for "reporting status"...
                For Each mapper As PhasorMeasurementMapper In m_mappers.Values
                    ' Update reporting status for each PMU
                    For Each cell As ConfigurationCell In mapper.ConfigurationCells.Values
                        If Not String.IsNullOrEmpty(cell.IDLabel) Then
                            With New StringBuilder
                                isReporting = IIf(Math.Abs(DateTime.UtcNow.Subtract(New DateTime(cell.LastReportTime)).Seconds) <= m_statusInterval, -1, 0)

                                .Append("UPDATE PMUs SET IsReporting=")
                                .Append(isReporting)
                                .Append(", ReportTime='")
                                .Append(DateTime.UtcNow.ToString())
                                .Append("' WHERE PMUID_Uniq='")
                                .Append(cell.IDLabel)
                                .Append("'"c)

                                ExecuteNonQuery(.ToString(), connection)
                            End With
                        End If
                    Next
                Next
            Catch ex As Exception
                UpdateStatus(String.Format("[{0}] ERROR: Failed to update PMU reporting status due to exception: {1}", DateTime.Now, ex.Message))
                m_exceptionLogger.Log(ex)
            Finally
                If connection IsNot Nothing Then connection.Close()
            End Try
        End If

    End Sub

    Private Sub m_historianAdapter_ArchivalException(ByVal source As String, ByVal ex As System.Exception) Handles m_historianAdapter.ArchivalException

        UpdateStatus(String.Format("{0} data archival exception: {1}", source, ex.Message))
        m_exceptionLogger.Log(ex)

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