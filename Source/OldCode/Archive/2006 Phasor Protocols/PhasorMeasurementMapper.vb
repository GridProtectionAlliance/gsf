'*******************************************************************************************************
'  PhasorMeasurementMapper.vb - Basic phasor measurement mapper
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
'  05/08/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.IO
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Soap
Imports TVA.DateTime
Imports TVA.Communication
Imports TVA.Measurements
Imports TVA.IO.FilePath
Imports TVA.ErrorManagement
Imports PhasorProtocols
Imports PhasorProtocols.Common

''' <summary>
''' <para>This class takes parsed phasor frames and maps measured elements to historian points</para>
''' <para>Real-time measurements are also provided to entites that need them (i.e., calculated measurements)</para>
''' </summary>
<CLSCompliant(False)> _
Public Class PhasorMeasurementMapper

    Public Event NewParsedMeasurements(ByVal measurements As Dictionary(Of MeasurementKey, IMeasurement))
    Public Event ParsingStatus(ByVal message As String)
    Public Event Connected()

    Private WithEvents m_dataStreamMonitor As Timers.Timer
    Private WithEvents m_frameParser As MultiProtocolFrameParser
    Private m_mapperName As String
    Private m_archiverSource As String
    Private m_source As String
    Private m_configurationCells As Dictionary(Of UInt16, ConfigurationCell)
    Private m_measurementIDs As Dictionary(Of String, IMeasurement)
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
    Private m_exceptionLogger As GlobalExceptionLogger

    Public Sub New( _
        ByVal frameParser As MultiProtocolFrameParser, _
        ByVal archiverSource As String, _
        ByVal source As String, _
        ByVal configurationCells As Dictionary(Of UInt16, ConfigurationCell), _
        ByVal measurementIDs As Dictionary(Of String, IMeasurement), _
        ByVal dataLossInterval As Integer, _
        ByVal exceptionLogger As GlobalExceptionLogger)

        m_frameParser = frameParser
        m_archiverSource = archiverSource
        m_source = source
        m_configurationCells = configurationCells
        m_measurementIDs = measurementIDs
        m_exceptionLogger = exceptionLogger

        m_dataStreamMonitor = New Timers.Timer

        With m_dataStreamMonitor
            .AutoReset = True
            .Interval = dataLossInterval
            .Enabled = False
        End With

        m_undefinedPmus = New Dictionary(Of String, Long)

    End Sub

    Public Sub Connect()

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_frameParser.Start()

    End Sub

    Public Sub Disconnect()

        ' Stop data stream monitor, if running
        If m_dataStreamMonitor IsNot Nothing Then
            m_dataStreamMonitor.Enabled = False
        End If

        ' Stop multi-protocol frame parser
        If m_frameParser IsNot Nothing Then m_frameParser.Stop()

        m_receivedConfigFrame = False
        m_errorCount = 0
        m_errorTime = 0
        m_unknownFramesReceived = 0

        If m_attemptingConnection Then UpdateStatus(String.Format("Canceling connection cycle to {0}.", Name))

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
                .Append(Environment.NewLine)
                .Append(m_frameParser.Status)
                .Append("Undefined PMU's Encountered: ")
                .Append(m_undefinedPmus.Count)
                .Append(Environment.NewLine)

                For Each item As KeyValuePair(Of String, Long) In m_undefinedPmus
                    .Append("    ")
                    .Append(item.Key)
                    .Append(" encountered ")
                    .Append(item.Value)
                    .Append("times")
                    .Append(Environment.NewLine)
                Next

                Return .ToString()
            End With
        End Get
    End Property

    Public ReadOnly Property LastReportTime() As Long
        Get
            Return m_lastReportTime
        End Get
    End Property

    Public ReadOnly Property TotalBytesReceived() As Long
        Get
            Return m_frameParser.TotalBytesReceived
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            If m_mapperName Is Nothing Then
                With New StringBuilder
                    .Append(m_source)

                    Dim displayChildren As Boolean = (m_configurationCells.Count > 1)

                    If Not displayChildren Then
                        ' Could still be a PDC with one child - so we'll compare the names
                        With m_configurationCells.Values.GetEnumerator()
                            If .MoveNext Then displayChildren = (String.Compare(m_source, .Current.IDLabel, True) <> 0)
                        End With
                    End If

                    If displayChildren Then
                        Dim index As Integer
                        .Append(" [")
                        For Each pmu As ConfigurationCell In m_configurationCells.Values
                            If index > 0 Then .Append(", ")
                            .Append(pmu.IDLabel)
                            index += 1
                        Next
                        .Append("]")
                    End If

                    m_mapperName = .ToString()
                End With
            End If

            Return m_mapperName
        End Get
    End Property

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
        If m_measurementIDs.TryGetValue(signalSynonym, measurementID) Then
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
        Dim pmu As ConfigurationCell = Nothing
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
        If m_timeAdjustmentTicks > 0 Then frame.Ticks += m_timeAdjustmentTicks

        ' Get ticks of this frame
        ticks = frame.Ticks

        ' Loop through each parsed PMU data cell
        For x = 0 To frame.Cells.Count - 1
            ' Get current PMU cell from data frame
            dataCell = frame.Cells(x)

            ' Lookup PMU information by its ID code
            If m_configurationCells.TryGetValue(dataCell.IDCode, pmu) Then
                ' Track lastest reporting time
                If ticks > pmu.LastReportTime Then pmu.LastReportTime = ticks
                If ticks > m_lastReportTime Then m_lastReportTime = ticks

                ' Map status flags (SF) from PMU data cell itself
                MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Status), dataCell)

                ' Map phase angles (PAn) and magnitudes (PMn)
                phasors = dataCell.PhasorValues
                count = phasors.Count

                For y = 0 To count - 1
                    ' Get composite phasor measurements
                    measurements = phasors(y).Measurements

                    ' Map angle
                    MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Angle, y, count), measurements(CompositePhasorValue.Angle))

                    ' Map magnitude
                    MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Magnitude, y, count), measurements(CompositePhasorValue.Magnitude))
                Next

                ' Map frequency (FQ) and df/dt (DF)
                measurements = dataCell.FrequencyValue.Measurements

                ' Map frequency
                MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Frequency), measurements(CompositeFrequencyValue.Frequency))

                ' Map df/dt
                MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.dfdt), measurements(CompositeFrequencyValue.DfDt))

                ' Map analog values (AVn)
                analogs = dataCell.AnalogValues
                count = analogs.Count

                For y = 0 To count - 1
                    ' Map analog value
                    MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Analog, y, count), analogs(y).Measurements(0))
                Next

                ' Map digital values (DVn)
                digitals = dataCell.DigitalValues
                count = digitals.Count

                For y = 0 To count - 1
                    ' Map digital value
                    MapSignalToMeasurement(frame, pmu.SignalSynonym(SignalType.Digital, y, count), digitals(y).Measurements(0))
                Next
            Else
                ' Encountered an undefined PMU, track frame counts
                Dim frameCount As Long

                If m_undefinedPmus.TryGetValue(dataCell.StationName, frameCount) Then
                    frameCount += 1
                    m_undefinedPmus(dataCell.StationName) = frameCount
                Else
                    m_undefinedPmus.Add(dataCell.StationName, 1)
                    UpdateStatus(String.Format("WARNING: Encountered an undefined PMU ""{0}"" for {1}.", dataCell.StationName, m_source))
                End If
            End If
        Next

        ' Provide real-time measurements where needed
        RaiseEvent NewParsedMeasurements(frame.Measurements)

    End Sub

    Private Sub m_frameParser_AttemptingConnection() Handles m_frameParser.AttemptingConnection

        m_attemptingConnection = True
        UpdateStatus(String.Format("Attempting {0} {1} based connection to {2}...", GetFormattedProtocolName(m_frameParser.PhasorProtocol), [Enum].GetName(GetType(TransportProtocol), m_frameParser.TransportProtocol).ToUpper(), m_source))

    End Sub

    Private Sub m_frameParser_Connected() Handles m_frameParser.Connected

        ' Enable data stream monitor for non-UDP connections
        m_attemptingConnection = False
        m_dataStreamMonitor.Enabled = (m_frameParser.TransportProtocol <> TransportProtocol.Udp)

        UpdateStatus(String.Format("Connection to {0} established.", m_source))
        RaiseEvent Connected()

    End Sub

    Private Sub m_frameParser_ConnectionException(ByVal ex As System.Exception, ByVal connectionAttempts As Integer) Handles m_frameParser.ConnectionException

        UpdateStatus(String.Format("{0} connection to ""{1}"" failed: {2}", m_source, m_frameParser.ConnectionName, ex.Message))
        m_exceptionLogger.Log(ex)

        ' Start reconnection attempt on a seperate thread (need to let this communications thread die gracefully)
        ThreadPool.UnsafeQueueUserWorkItem(AddressOf AttemptReconnection, Nothing)

    End Sub

    Private Sub AttemptReconnection(ByVal state As Object)

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

        If m_frameParser.Enabled Then
            ' Communications layer closed connection (close not initiated by system) - so we terminate gracefully...
            Disconnect()
            UpdateStatus(String.Format("WARNING: Connection closed by remote device {0}, attempting reconnection...", m_source))

            ' Start reconnection attempt on a seperate thread (need to let this communications thread die gracefully)
            ThreadPool.UnsafeQueueUserWorkItem(AddressOf AttemptReconnection, Nothing)
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

        If Not m_receivedConfigFrame Then ThreadPool.UnsafeQueueUserWorkItem(AddressOf CacheConfigurationFrame, frame)
        m_receivedConfigFrame = True

    End Sub

    Private Sub CacheConfigurationFrame(ByVal state As Object)

        Dim frame As IConfigurationFrame = DirectCast(state, IConfigurationFrame)

        UpdateStatus(String.Format("Received {0} configuration frame at {1}", m_source, Date.Now))

        Try
            Dim cachePath As String = GetApplicationPath() & "ConfigurationCache\"
            If Not Directory.Exists(cachePath) Then Directory.CreateDirectory(cachePath)
            Dim configFile As FileStream = File.Create(cachePath & m_source & ".configuration.xml")

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
            UpdateStatus(String.Format("{0}No data on {1} received in {2} seconds, restarting connect cycle...{3}", Environment.NewLine, m_source, Convert.ToInt32(m_dataStreamMonitor.Interval / 1000.0R), Environment.NewLine))
            Connect()
            m_dataStreamMonitor.Enabled = False
        End If

        m_bytesReceived = 0

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