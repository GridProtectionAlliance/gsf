'*******************************************************************************************************
'  PhasorMeasurementMapper.vb - Basic phasor measurement mapper
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/08/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.IO
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Soap
Imports TVA.Common
Imports TVA.DateTime
Imports TVA.Communication
Imports TVA.Measurements
Imports TVA.IO.FilePath
Imports TVA.Text.Common
Imports TVA.ErrorManagement
Imports TVA.Threading
Imports PhasorProtocols
Imports PhasorProtocols.Common

''' <summary>
''' <para>This class takes parsed phasor frames and maps measured elements to historian points</para>
''' <para>Real-time measurements are also provided to entites that need them (i.e., calculated measurements)</para>
''' </summary>
<CLSCompliant(False)> _
Public Class PhasorMeasurementMapper

    Implements IDisposable

    Public Event NewParsedMeasurements(ByVal measurements As ICollection(Of IMeasurement))
    Public Event ParsingStatus(ByVal message As String)

    Private WithEvents m_frameParser As MultiProtocolFrameParser
    Private WithEvents m_dataStreamMonitor As Timers.Timer
    Private WithEvents m_delayedConnection As Timers.Timer
    Private m_archiverSource As String
    Private m_source As String
    Private m_configurationCells As Dictionary(Of UInt16, ConfigurationCell)
    Private m_signalMeasurements As Dictionary(Of String, IMeasurement)
    Private m_activeMeasurementKeys As List(Of MeasurementKey)
    Private m_isConnected As Boolean
    Private m_lastReportTime As Long
    Private m_bytesReceived As Long
    Private m_errorCount As Integer
    Private m_errorTime As Long
    Private m_unknownFramesReceived As Integer
    Private m_receivedConfigFrame As Boolean
    Private m_timezone As Win32TimeZone
    Private m_timeAdjustmentTicks As Long
    Private m_attemptingConnection As Boolean
    Private m_undefinedPmus As Dictionary(Of String, Long)
    Private m_name As String
    Private m_isPDC As Integer
    Private m_totalVirtualCells As Integer
    Private m_exceptionLogger As GlobalExceptionLogger
    Private m_disposed As Boolean

    Public Sub New( _
        ByVal frameParser As MultiProtocolFrameParser, _
        ByVal archiverSource As String, _
        ByVal source As String, _
        ByVal configurationCells As Dictionary(Of UInt16, ConfigurationCell), _
        ByVal signalMeasurements As Dictionary(Of String, IMeasurement), _
        ByVal dataLossInterval As Integer, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        m_frameParser = frameParser
        m_archiverSource = archiverSource
        m_source = source
        m_configurationCells = configurationCells
        m_signalMeasurements = signalMeasurements
        m_exceptionLogger = exceptionLogger
        m_isPDC = -1

        m_dataStreamMonitor = New Timers.Timer

        With m_dataStreamMonitor
            .AutoReset = True
            .Interval = dataLossInterval
            .Enabled = False
        End With

        m_delayedConnection = New Timers.Timer

        With m_delayedConnection
            .AutoReset = False
            .Interval = 1500
            .Enabled = False
        End With

        m_undefinedPmus = New Dictionary(Of String, Long)

        ' Mapper configuration doesn't change during its lifecycle - so
        ' we only check to see if there are virtual cells once
        For Each cell As ConfigurationCell In m_configurationCells.Values
            If cell.IsVirtual Then m_totalVirtualCells += 1
        Next

        If m_totalVirtualCells > 0 Then
            ' When we have virtual cells to contend with, it will be helpful to
            ' quickly do measurement filtering...
            m_activeMeasurementKeys = New List(Of MeasurementKey)

            For Each measurement As IMeasurement In m_signalMeasurements.Values
                m_activeMeasurementKeys.Add(measurement.Key)
            Next

            m_activeMeasurementKeys.Sort()
        End If

    End Sub

    Protected Overrides Sub Finalize()

        Dispose(True)

    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)

        If Not m_disposed Then
            If disposing Then
                If m_frameParser IsNot Nothing Then m_frameParser.Dispose()
                If m_dataStreamMonitor IsNot Nothing Then m_dataStreamMonitor.Dispose()
                If m_delayedConnection IsNot Nothing Then m_delayedConnection.Dispose()
            End If
        End If

        m_disposed = True

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        Dispose(True)
        GC.SuppressFinalize(Me)

    End Sub

    Public Sub Connect()

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        m_lastReportTime = 0

        ' Start timer for delayed connection
        If m_frameParser Is Nothing Then
            ' Purely virtual mappers will be assumed to be connected
            m_isConnected = (m_totalVirtualCells > 0)
        Else
            m_delayedConnection.Enabled = True
        End If

        ' Display extra information for mappers with virtual devices
        If m_totalVirtualCells > 0 Then UpdateStatus(String.Format("Connected {0} with {1} virtual device(s) - {2} active measurements defined", m_source, m_totalVirtualCells, m_activeMeasurementKeys.Count))

    End Sub

    Public Sub Disconnect()

        ' Stop delayed connection timer, if enabled
        If m_delayedConnection IsNot Nothing Then m_delayedConnection.Enabled = False

        ' Stop data stream monitor, if running
        If m_dataStreamMonitor IsNot Nothing Then m_dataStreamMonitor.Enabled = False

        ' Stop multi-protocol frame parser
        If m_frameParser IsNot Nothing Then
            ' We perform disconnect on a separate thread so we can timeout since this can give us troubles :(
#If ThreadTracking Then
            With New ManagedThread(AddressOf m_frameParser.Stop)
                .Name = "TVASPDC.PhasorMeasurementMapper.Disconnect(m_frameParser.Stop) [" & Name & "]"
#Else
            With New Thread(AddressOf m_frameParser.Stop)
#End If
                .Start()
                If Not .Join(1000) Then .Abort()
            End With
        End If

        If m_attemptingConnection Then UpdateStatus(String.Format("Canceling connection cycle to {0}.", Name))

        If m_isConnected Then UpdateStatus(String.Format("Disconnected from {0}.", m_source))

        m_isConnected = False
        m_attemptingConnection = False
        m_receivedConfigFrame = False
        m_errorCount = 0
        m_errorTime = 0
        m_unknownFramesReceived = 0

    End Sub

    Public Sub SendDeviceCommand(ByVal command As DeviceCommand)

        If m_frameParser IsNot Nothing Then m_frameParser.SendDeviceCommand(command)
        UpdateStatus(String.Format(">> Sent device command ""{0}""...", [Enum].GetName(GetType(DeviceCommand), command)))

    End Sub

    Public ReadOnly Property This() As PhasorMeasurementMapper
        Get
            Return Me
        End Get
    End Property

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("Phasor Data Parsing Connection for ")
                .Append(Name)
                .AppendLine()
                If m_frameParser IsNot Nothing Then
                    .Append(m_frameParser.Status)
                Else
                    .AppendLine()
                    .Append(">> No frame parser defined for this device.")
                    .AppendLine()
                End If
                .AppendLine()
                .Append(CenterText("Parsed Frame Quality Statistics", 78))
                .AppendLine()
                .AppendLine()
                '                 1         2         3         4         5         6         7
                '        123456789012345678901234567890123456789012345678901234567890123456789012345678
                .Append("Device                  Bad Data   Bad Time    Frame      Total    Last Report")
                .AppendLine()
                .Append(" Name                    Frames     Frames     Errors     Frames      Time")
                .AppendLine()
                '        1234567890123456789012 1234567890 1234567890 1234567890 1234567890 123456789012
                .Append("---------------------- ---------- ---------- ---------- ---------- ------------")
                .AppendLine()

                Dim pmu As IConfigurationCell
                Dim stationName As String

                For Each cell As ConfigurationCell In m_configurationCells.Values
                    ' Attempt to lookup station name in configuration frame of connected device
                    stationName = Nothing

                    If m_frameParser IsNot Nothing Then
                        If m_frameParser.ConfigurationFrame IsNot Nothing AndAlso m_frameParser.ConfigurationFrame.Cells.TryGetByIDCode(cell.IDCode, pmu) Then stationName = pmu.StationName
                    End If

                    If String.IsNullOrEmpty(stationName) Then stationName = "<" & cell.IDLabel & ">"

                    If cell.IsVirtual Then
                        .Append(TruncateRight(">> Virtual device: " & stationName, 66).PadRight(66))
                    Else
                        .Append(TruncateRight(stationName, 22).PadRight(22))
                        .Append(" "c)
                        .Append(CenterText(cell.TotalDataQualityErrors.ToString(), 10))
                        .Append(" "c)
                        .Append(CenterText(cell.TotalTimeQualityErrors.ToString(), 10))
                        .Append(" "c)
                        .Append(CenterText(cell.TotalPmuErrors.ToString(), 10))
                        .Append(" "c)
                        .Append(CenterText(cell.TotalFrames.ToString(), 10))
                    End If
                    .Append(" "c)
                    .Append((New Date(cell.LastReportTime)).ToString("HH:mm:ss.fff"))
                    .AppendLine()
                Next

                .AppendLine()
                .Append("Undefined PMUs Encountered: ")
                .Append(m_undefinedPmus.Count)
                .AppendLine()

                SyncLock m_undefinedPmus
                    For Each item As KeyValuePair(Of String, Long) In m_undefinedPmus
                        .Append("    PMU """)
                        .Append(item.Key)
                        .Append(""" encountered ")
                        .Append(item.Value)
                        .Append(" times")
                        .AppendLine()
                    Next
                End SyncLock

                Return .ToString()
            End With
        End Get
    End Property

    Public ReadOnly Property IsConnected() As Boolean
        Get
            Return m_isConnected
        End Get
    End Property

    Public ReadOnly Property LastReportTime() As Long
        Get
            Return m_lastReportTime
        End Get
    End Property

    Public ReadOnly Property TotalBytesReceived() As Long
        Get
            If m_frameParser Is Nothing Then
                Return 0
            Else
                Return m_frameParser.TotalBytesReceived
            End If
        End Get
    End Property

    Public ReadOnly Property CalculatedFrameRate() As Double
        Get
            If m_frameParser Is Nothing Then
                If m_totalVirtualCells > 0 Then
                    ' Return the frame rate of the first virtual device
                    With m_configurationCells.Values.GetEnumerator()
                        Do While .MoveNext
                            If .Current.IsVirtual Then Return .Current.CalculatedFrameRate
                        Loop
                    End With
                End If

                Return 0.0R
            Else
                Return m_frameParser.FrameRate
            End If
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            If String.IsNullOrEmpty(m_name) Then
                With New StringBuilder
                    .Append(m_source)

                    If m_totalVirtualCells > 0 Then
                        If IsPDC Then
                            .AppendFormat(" [PDC: {0} Devices, {1} Virtual]", m_configurationCells.Count, m_totalVirtualCells)
                        Else
                            .Append(" [Virtual Device]")
                        End If
                    Else
                        If IsPDC Then .AppendFormat(" [PDC: {0} Devices]", m_configurationCells.Count)
                    End If

                    m_name = .ToString()
                End With
            End If

            Return m_name
        End Get
    End Property

    Public ReadOnly Property IsPDC() As Boolean
        Get
            If m_isPDC = -1 Then
                ' Configurations with more than one cell are considered PDC's
                Dim hasChildren As Boolean = (m_configurationCells.Count > 1)

                If Not hasChildren Then
                    ' Could still be a PDC with one child - so we'll compare the names
                    With m_configurationCells.Values.GetEnumerator()
                        If .MoveNext Then hasChildren = (String.Compare(m_source, .Current.IDLabel, True) <> 0)
                    End With
                End If

                If hasChildren Then
                    m_isPDC = 1
                Else
                    m_isPDC = 0
                End If
            End If

            Return (m_isPDC <> 0)
        End Get
    End Property

    Public ReadOnly Property HasVirtualCells() As Boolean
        Get
            Return (m_totalVirtualCells > 0)
        End Get
    End Property

    Public Sub ReceivedNewVirtualMeasurements(ByVal cell As ConfigurationCell, ByVal measurements As ICollection(Of IMeasurement))

        ' We use this function to verify that the virtual device is actually reporting - that is, that
        ' the composed points that make up this virtual device are actually being calculated...
        Dim measurement As IMeasurement

        For x As Integer = 0 To measurements.Count - 1
            measurement = measurements(x)

            If m_activeMeasurementKeys.BinarySearch(measurement.Key) >= 0 Then
                ' Track lastest reporting time
                Dim ticks As Long = measurement.Ticks

                If ticks > 0 Then
                    If ticks > cell.LastReportTime Then
                        ' We update last report time for this device
                        cell.LastReportTime = ticks

                        ' We count received measurements to calculate virtual frame rate
                        cell.IncrementFrameCount()
                    End If

                    ' We also update last report time for the mapper
                    If ticks > m_lastReportTime Then m_lastReportTime = ticks

                    Exit For
                End If
            End If
        Next

    End Sub

    Public Property TimeZone() As Win32TimeZone
        Get
            Return m_timezone
        End Get
        Set(ByVal value As Win32TimeZone)
            m_timezone = value
        End Set
    End Property

    Public Property TimeAdjustmentTicks() As Long
        Get
            Return m_timeAdjustmentTicks
        End Get
        Set(ByVal value As Long)
            m_timeAdjustmentTicks = value
        End Set
    End Property

    Public ReadOnly Property ConfigurationCells() As Dictionary(Of UInt16, ConfigurationCell)
        Get
            Return m_configurationCells
        End Get
    End Property

    Public ReadOnly Property UndefinedPmus() As Dictionary(Of String, Long)
        Get
            Return m_undefinedPmus
        End Get
    End Property

    Private Sub UpdateStatus(ByVal message As String)

        RaiseEvent ParsingStatus(message)

    End Sub

    ''' <summary>Maps measured phasor signal value to defined historian measurement ID</summary>
    ''' <remarks>This procedure is used to identify a signal value and apply any additional needed measurement attributes</remarks>
    Private Sub MapSignalToMeasurement(ByVal frame As IDataFrame, ByVal signalSynonym As String, ByVal signalValue As IMeasurement)

        ' Coming into this function the signalValue measurement will only have a "value" and a "timestamp";
        ' the measurement is not yet associated with an actual historian measurement ID as the measurement
        ' came out of the phasor protocols directly.  We take the generated "Signal Synonym" and use that
        ' to lookup the actual historian measurement ID, source, adder and multipler.
        Dim measurementID As IMeasurement

        ' Lookup synonym value in measurement ID list
        If m_signalMeasurements.TryGetValue(signalSynonym, measurementID) Then
            ' Assign ID and other relevant measurement attributes to the signal value
            signalValue.ID = measurementID.ID
            signalValue.Source = m_archiverSource
            signalValue.Adder = measurementID.Adder             ' Allows for run-time additive measurement value adjustments
            signalValue.Multiplier = measurementID.Multiplier   ' Allows for run-time mulplicative measurement value adjustments

            ' Add the updated measurement value to the keyed frame measurement list
            frame.Measurements.Add(signalValue.Key, signalValue)
        End If

    End Sub

    ' This key function is the glue that binds a phasor measurement value to a historian measurement ID
    Private Sub m_frameParser_ReceivedDataFrame(ByVal frame As IDataFrame) Handles m_frameParser.ReceivedDataFrame

        ' Map data frame measurement instances to their associated point ID's
        Dim cell As ConfigurationCell
        Dim dataCell As IDataCell
        Dim phasors As PhasorValueCollection
        Dim analogs As AnalogValueCollection
        Dim digitals As DigitalValueCollection
        Dim measurements As IMeasurement()
        Dim x, y, count As Integer
        Dim ticks As Long

        ' Adjust time to UTC based on source PDC/PMU time zone, if provided (typically when not UTC already)
        If m_timezone IsNot Nothing Then frame.Ticks = m_timezone.ToUniversalTime(frame.Timestamp).Ticks

        ' We also allow "fine tuning" of time for fickle GPS clocks...
        If m_timeAdjustmentTicks <> 0 Then frame.Ticks += m_timeAdjustmentTicks

        ' Get ticks of this frame
        ticks = frame.Ticks

        ' Loop through each parsed PMU data cell
        For x = 0 To frame.Cells.Count - 1
            Try
                ' Get current PMU cell from data frame
                dataCell = frame.Cells(x)

                ' Lookup PMU information by its ID code
                If m_configurationCells.TryGetValue(dataCell.IDCode, cell) Then
                    ' Track lastest reporting time
                    If ticks > cell.LastReportTime Then cell.LastReportTime = ticks
                    If ticks > m_lastReportTime Then m_lastReportTime = ticks

                    ' Track quality statistics for this PMU
                    cell.TotalFrames += 1
                    If Not dataCell.DataIsValid Then cell.TotalDataQualityErrors += 1
                    If Not dataCell.SynchronizationIsValid Then cell.TotalTimeQualityErrors += 1
                    If dataCell.PmuError Then cell.TotalPmuErrors += 1

                    ' Map status flags (SF) from PMU data cell itself
                    MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Status), dataCell)

                    ' Map phase angles (PAn) and magnitudes (PMn)
                    phasors = dataCell.PhasorValues
                    count = phasors.Count

                    For y = 0 To count - 1
                        ' Get composite phasor measurements
                        measurements = phasors(y).Measurements

                        ' Map angle
                        MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Angle, y, count), measurements(CompositePhasorValue.Angle))

                        ' Map magnitude
                        MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Magnitude, y, count), measurements(CompositePhasorValue.Magnitude))
                    Next

                    ' Map frequency (FQ) and df/dt (DF)
                    measurements = dataCell.FrequencyValue.Measurements

                    ' Map frequency
                    MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Frequency), measurements(CompositeFrequencyValue.Frequency))

                    ' Map df/dt
                    MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.dFdt), measurements(CompositeFrequencyValue.DfDt))

                    ' Map analog values (AVn)
                    analogs = dataCell.AnalogValues
                    count = analogs.Count

                    For y = 0 To count - 1
                        ' Map analog value
                        MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Analog, y, count), analogs(y).Measurements(0))
                    Next

                    ' Map digital values (DVn)
                    digitals = dataCell.DigitalValues
                    count = digitals.Count

                    For y = 0 To count - 1
                        ' Map digital value
                        MapSignalToMeasurement(frame, cell.SignalSynonym(SignalType.Digital, y, count), digitals(y).Measurements(0))
                    Next
                Else
                    ' Encountered an undefined PMU, track frame counts
                    SyncLock m_undefinedPmus
                        Dim frameCount As Long

                        If m_undefinedPmus.TryGetValue(dataCell.StationName, frameCount) Then
                            frameCount += 1
                            m_undefinedPmus(dataCell.StationName) = frameCount
                        Else
                            m_undefinedPmus.Add(dataCell.StationName, 1)
                            UpdateStatus(String.Format("WARNING: Encountered an undefined PMU ""{0}"" for {1}.", dataCell.StationName, m_source))
                        End If
                    End SyncLock
                End If
            Catch ex As Exception
                UpdateStatus(String.Format("Exception encountered while mapping ""{0}"" data frame cell ""{1} [{2}]"" elements to measurements: {3}", m_source, dataCell.StationName, x, ex.Message))
                m_exceptionLogger.Log(ex)
            End Try
        Next

        ' Provide real-time measurements where needed
        RaiseEvent NewParsedMeasurements(frame.Measurements.Values)

    End Sub

    Private Sub m_frameParser_AttemptingConnection() Handles m_frameParser.AttemptingConnection

        m_attemptingConnection = True
        UpdateStatus(String.Format("Attempting {0} {1} based connection to {2}...", GetFormattedProtocolName(m_frameParser.PhasorProtocol), [Enum].GetName(GetType(TransportProtocol), m_frameParser.TransportProtocol).ToUpper(), m_source))

    End Sub

    Private Sub m_frameParser_Connected() Handles m_frameParser.Connected

        m_isConnected = True
        m_attemptingConnection = False

        ' Enable data stream monitor for non-UDP connections
        m_dataStreamMonitor.Enabled = (m_frameParser.TransportProtocol <> TransportProtocol.Udp)

        UpdateStatus(String.Format("Connection to {0} established.", m_source))

    End Sub

    Private Sub m_frameParser_ConnectionException(ByVal ex As System.Exception, ByVal connectionAttempts As Integer) Handles m_frameParser.ConnectionException

        UpdateStatus(String.Format("{0} connection to ""{1}"" failed: {2}", m_source, m_frameParser.ConnectionName, ex.Message))
        m_exceptionLogger.Log(ex)
        m_attemptingConnection = False
        Connect()

    End Sub

    Private Sub m_frameParser_DataStreamException(ByVal ex As System.Exception) Handles m_frameParser.DataStreamException

        UpdateStatus(String.Format("WARNING: {0} data stream exception: {1}", m_source, ex.Message))
        m_exceptionLogger.Log(ex)

        ' We monitor for exceptions that occur in quick succession
        If Date.Now.Ticks - m_errorTime > 100000000L Then
            m_errorTime = Date.Now.Ticks
            m_errorCount = 1
        End If

        m_errorCount += 1

        ' When we get 10 or more exceptions within a ten second timespan, we will then restart connection cycle...
        If m_errorCount >= 10 Then
            UpdateStatus(String.Format("{0} connection terminated due to excessive exceptions.", m_source))
            Connect()
        End If

    End Sub

    Private Sub m_frameParser_Disconnected() Handles m_frameParser.Disconnected

        m_isConnected = False

        If m_frameParser.Enabled Then
            ' Communications layer closed connection (close not initiated by system) - so we terminate gracefully...
            UpdateStatus(String.Format("WARNING: Connection closed by remote device {0}, attempting reconnection...", m_source))
            m_attemptingConnection = False
            Connect()
        Else
            UpdateStatus(String.Format("Disconnected from {0}.", m_source))
        End If

    End Sub

    Private Sub m_frameParser_ConfigurationChanged() Handles m_frameParser.ConfigurationChanged

        m_receivedConfigFrame = False

        UpdateStatus(String.Format("NOTICE: Configuration has changed for {0}, requesting new configuration frame...", m_source))
        SendDeviceCommand(DeviceCommand.SendConfigurationFrame2)

    End Sub

    Private Sub m_frameParser_ReceivedConfigurationFrame(ByVal frame As IConfigurationFrame) Handles m_frameParser.ReceivedConfigurationFrame

        If Not m_receivedConfigFrame Then
#If ThreadTracking Then
            With TVA.Threading.ManagedThreadPool.QueueUserWorkItem(AddressOf CacheConfigurationFrame, frame)
                .Name = "TVASPDC.PhasorMeasurementMapper.CacheConfigurationFrame()"
            End With
#Else
            ThreadPool.UnsafeQueueUserWorkItem(AddressOf CacheConfigurationFrame, frame)
#End If
        End If

        m_receivedConfigFrame = True

    End Sub

    Private Sub CacheConfigurationFrame(ByVal state As Object)

        Dim frame As IConfigurationFrame = DirectCast(state, IConfigurationFrame)

        UpdateStatus(String.Format("Received {0} configuration frame at {1}", m_source, Date.Now))

        Try
            Dim cachePath As String = String.Format("{0}ConfigurationCache\", GetApplicationPath())
            If Not Directory.Exists(cachePath) Then Directory.CreateDirectory(cachePath)
            Dim configFile As FileStream = File.Create(String.Format("{0}{1}.configuration.xml", cachePath, m_source))

            With New SoapFormatter
                .AssemblyFormat = FormatterAssemblyStyle.Simple
                .TypeFormat = FormatterTypeStyle.TypesWhenNeeded
                .Serialize(configFile, frame)
            End With

            configFile.Close()
        Catch ex As Exception
            UpdateStatus(String.Format("Failed to serialize configuration frame: {0}", ex.Message))
            m_exceptionLogger.Log(ex)
        End Try

    End Sub

    Private Sub m_frameParser_ReceivedFrameBufferImage(ByVal frameType As FundamentalFrameType, ByVal binaryImage() As Byte, ByVal offset As Integer, ByVal length As Integer) Handles m_frameParser.ReceivedFrameBufferImage

        m_bytesReceived += length

        If frameType = FundamentalFrameType.Undetermined Then
            m_unknownFramesReceived += 1
            If m_unknownFramesReceived Mod 300 = 0 Then UpdateStatus(String.Format("WARNING: {0} has received {1} undetermined frame images.", m_source, m_unknownFramesReceived))
        End If

    End Sub

    Private Sub m_dataStreamMonitor_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_dataStreamMonitor.Elapsed

        ' If we've received no data in the last little timespan, we restart connect cycle...
        If m_bytesReceived = 0 Then
            m_dataStreamMonitor.Enabled = False
            UpdateStatus(String.Format("{0}No data on {1} received in {2} seconds, restarting connect cycle...{3}", Environment.NewLine, m_source, Convert.ToInt32(m_dataStreamMonitor.Interval / 1000.0R), Environment.NewLine))
            Connect()
        End If

        m_bytesReceived = 0

    End Sub

    ' Delayed connection handler
    Private Sub m_delayedConnection_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_delayedConnection.Elapsed

        ' Start frame parser (this will attempt connection)...
        m_frameParser.Start()

    End Sub

End Class

#Region " Old Code "

'''' <summary>This key function is the glue that binds a phasor measurement value to a historian measurement ID</summary>
'''' <remarks>This function is expected to be executed on an independent thread (e.g., queued via Threadpool)</remarks>
'Protected Overridable Sub MapDataFrameMeasurements(ByVal state As Object)

'    Dim frame As IDataFrame = DirectCast(state, IDataFrame)

'    ' Map data frame measurement instances to their associated point ID's
'    With frame
'        Dim pmuID As PmuInfo = Nothing
'        Dim x, y As Integer
'        Dim ticks As Long

'        ' Adjust time to UTC based on source PDC/PMU time zone, if provided (typically when not UTC already)
'        If m_timezone IsNot Nothing Then .Ticks = m_timezone.ToUniversalTime(.Timestamp).Ticks

'        ' We also allow "fine tuning" of time for fickle GPS clocks...
'        If m_timeAdjustmentTicks > 0 Then .Ticks += m_timeAdjustmentTicks

'        ' Get ticks of this frame
'        ticks = .Ticks

'        ' Loop through each parsed PMU data cell
'        For x = 0 To .Cells.Count - 1
'            With .Cells(x)
'                ' Lookup PMU information by its ID code
'                If m_pmuIDs.TryGetValue(.IDCode, pmuID) Then
'                    ' Track lastest reporting time
'                    If ticks > pmuID.LastReportTime Then pmuID.LastReportTime = ticks
'                    If ticks > m_lastReportTime Then m_lastReportTime = ticks

'                    ' Map status flags (SF) from PMU data cell itself
'                    MapMeasurementIDToValue(frame, pmuID.Tag & "-SF", .This)

'                    ' Map phasor angles (PAn) and magnitudes (PMn)
'                    With .PhasorValues
'                        For y = 0 To .Count - 1
'                            With .Item(y)
'                                ' Map angle
'                                MapMeasurementIDToValue(frame, pmuID.Tag & "-PA" & (y + 1), .Measurements(CompositePhasorValue.Angle))

'                                ' Map magnitude
'                                MapMeasurementIDToValue(frame, pmuID.Tag & "-PM" & (y + 1), .Measurements(CompositePhasorValue.Magnitude))
'                            End With
'                        Next
'                    End With

'                    ' Map frequency (FQ) and delta-frequency (DF)
'                    With .FrequencyValue
'                        ' Map frequency
'                        MapMeasurementIDToValue(frame, pmuID.Tag & "-FQ", .Measurements(CompositeFrequencyValue.Frequency))

'                        ' Map df/dt
'                        MapMeasurementIDToValue(frame, pmuID.Tag & "-DF", .Measurements(CompositeFrequencyValue.DfDt))
'                    End With

'                    ' Map digital values (DVn)
'                    With .DigitalValues
'                        For y = 0 To .Count - 1
'                            ' Map digital value
'                            MapMeasurementIDToValue(frame, pmuID.Tag & "-DV" & y, .Item(y).Measurements(0))
'                        Next
'                    End With
'                Else
'                    ' TODO: Encountered a new PMU - decide best way to report this...
'                    ' Don't report it 30 times a second :)
'                End If
'            End With
'        Next
'    End With

'    '' Queue up frame for polled retrieval into historian...
'    'SyncLock m_measurementFrames
'    '    m_measurementFrames.Add(frame)
'    'End SyncLock

'    ' Provide real-time measurements where needed
'    RaiseEvent NewParsedMeasurements(frame.Measurements)

'End Sub

'Private m_measurementFrames As List(Of IFrame)

'm_measurementFrames = New List(Of IFrame)

'Public Function GetQueuedFrames() As IFrame()

'    Dim frames As IFrame()

'    SyncLock m_measurementFrames
'        If m_measurementFrames.Count > 0 Then
'            ' It possible, because of threading, that frames can be processed out of order
'            ' so we at least sort them by time before providing them to a historian
'            m_measurementFrames.Sort(AddressOf CompareFramesByTicks)
'            frames = m_measurementFrames.ToArray()
'            m_measurementFrames.Clear()
'        End If
'    End SyncLock

'    Return frames

'End Function

'Private Function CompareFramesByTicks(ByVal x As IFrame, ByVal y As IFrame) As Integer

'    Return x.Ticks.CompareTo(y.Ticks)

'End Function

#End Region