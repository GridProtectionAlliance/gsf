Imports System.Text
Imports Tva.Phasors
Imports Tva.Phasors.PhasorValueBase
Imports Tva.Phasors.FrequencyValueBase
Imports Tva.Measurements

<CLSCompliant(False)> _
Public Class PhasorDataParser

    Public Event ParsingStatus(ByVal message As String)

    Private WithEvents m_connectionTimer As Timers.Timer
    Private WithEvents m_dataStreamMonitor As Timers.Timer
    Private WithEvents m_frameParser As FrameParser
    Private m_pmuIDs As List(Of String)
    Private m_measurementIDs As Dictionary(Of String, Integer)
    Private m_measurementFrames As List(Of IFrame)
    Private m_bytesReceived As Long
    Private m_errorCount As Integer
    Private m_errorTime As Long

    Public Sub New(ByVal frameParser As FrameParser, ByVal pmuIDs As List(Of String), ByVal measurementIDs As Dictionary(Of String, Integer))

        m_frameParser = frameParser
        m_pmuIDs = pmuIDs
        m_measurementIDs = measurementIDs
        m_measurementFrames = New List(Of IFrame)

        m_connectionTimer = New Timers.Timer
        m_dataStreamMonitor = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 4000
            .Enabled = False
        End With

        With m_dataStreamMonitor
            .AutoReset = True
            .Interval = 10000
            .Enabled = False
        End With

    End Sub

    Public Sub Connect()

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Public Sub Disconnect()

        m_errorCount = 0
        m_errorTime = 0

        If m_frameParser IsNot Nothing Then m_frameParser.Disconnect()

        ' Stop data stream monitor, if running
        If m_dataStreamMonitor IsNot Nothing Then m_dataStreamMonitor.Enabled = False

    End Sub

    Private Sub UpdateStatus(ByVal message As String)

        RaiseEvent ParsingStatus(message & Environment.NewLine)

    End Sub

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed
        Try
            UpdateStatus("Starting connection attempt for PMU " & m_pmuIDs(0) & "...")

            m_frameParser.Connect()

            ' Enable data stream monitor
            m_dataStreamMonitor.Enabled = True

            UpdateStatus("Connection to " & m_pmuIDs(0) & " established.")
        Catch ex As Exception
            UpdateStatus(m_pmuIDs(0) & " connection to """ & m_frameParser.HostIP & ":" & m_frameParser.Port & """ failed: " & ex.Message)
            Connect()
        End Try

    End Sub

    Private Sub m_dataStreamMonitor_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_dataStreamMonitor.Elapsed

        ' If we've received no data in the last little timespan, we restart connect cycle...
        If m_bytesReceived = 0 Then
            UpdateStatus(Environment.NewLine & "No data on " & m_pmuIDs(0) & " received in " & m_dataStreamMonitor.Interval / 1000 & " seconds, restarting connect cycle..." & Environment.NewLine)
            Connect()
            m_dataStreamMonitor.Enabled = False
        End If

        m_bytesReceived = 0

    End Sub

    Public Function GetQueuedFrames() As IFrame()

        Dim frames As IFrame()

        SyncLock m_measurementFrames
            frames = m_measurementFrames.ToArray()
            m_measurementFrames.Clear()
        End SyncLock

        Return frames

    End Function

    Public ReadOnly Property This() As PhasorDataParser
        Get
            Return Me
        End Get
    End Property

    Public ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("Phasor Data Parsing Connection for ")
                For x As Integer = 0 To m_pmuIDs.Count - 1
                    If x > 0 Then .Append(", ")
                    .Append(m_pmuIDs(x))
                Next
                .Append(Environment.NewLine)
                .Append(m_frameParser.Status)

                Return .ToString()
            End With
        End Get
    End Property

    Protected Overridable Sub MapDataFrameMeasurements(ByVal frame As Tva.Phasors.IDataFrame)

        ' Map data frame measurement instances to their associated point ID's
        With frame
            Dim pmuID As String
            Dim x, y As Integer

            ' Loop through each parsed PMU data cell
            For x = 0 To .Cells.Count - 1
                ' Get current PMU ID
                pmuID = m_pmuIDs(x)

                With .Cells(x)
                    ' Map status flags SF from PMU data cell
                    MapMeasurement(frame, pmuID & "-SF", .This)

                    With .PhasorValues
                        For y = 0 To .Count - 1
                            With .Item(y)
                                ' Map angle - PA(n)
                                MapMeasurement(frame, pmuID & "-PA" & (y + 1), .Measurements(PolarCompositeValue.Angle))

                                ' Map magnitude - PM(m)
                                MapMeasurement(frame, pmuID & "-PM" & (y + 1), .Measurements(PolarCompositeValue.Magnitude))
                            End With
                        Next
                    End With

                    With .FrequencyValue
                        ' Map frequency - FQ
                        MapMeasurement(frame, pmuID & "-FQ", .Measurements(CompositeValue.Frequency))

                        ' Map df/dt - DF
                        MapMeasurement(frame, pmuID & "-DF", .Measurements(CompositeValue.DfDt))
                    End With

                    With .DigitalValues
                        For y = 0 To .Count - 1
                            ' Map digital values - DV(n)
                            MapMeasurement(frame, pmuID & "-DV" & y, .Item(y).Measurements(0))
                        Next
                    End With
                End With
            Next
        End With


        ' Queue up frame for polled retrieval into DatAWare...
        SyncLock m_measurementFrames
            m_measurementFrames.Add(frame)
        End SyncLock

    End Sub

    Private Sub m_frameParser_DataStreamException(ByVal ex As System.Exception) Handles m_frameParser.DataStreamException

        UpdateStatus("Data stream exception: " & ex.Message)

        ' We monitor for exceptions that occur in quick succession
        If Date.Now.Ticks - m_errorTime > 100000000L Then
            m_errorTime = Date.Now.Ticks
            m_errorCount = 1
        End If

        m_errorCount += 1

        ' When we get 10 or more exceptions within a ten second timespan, we will then restart connection cycle...
        If m_errorCount >= 10 Then
            UpdateStatus("Client connection terminated due to excessive exceptions.")
            Connect()
        End If

    End Sub

    Private Sub m_frameParser_ParsingStatus(ByVal message As String) Handles m_frameParser.ParsingStatus

        UpdateStatus(message)

    End Sub

    Private Sub m_frameParser_ReceivedConfigurationFrame(ByVal frame As Tva.Phasors.IConfigurationFrame) Handles m_frameParser.ReceivedConfigurationFrame

        UpdateStatus("Received " & m_pmuIDs(0) & " configuration frame at " & Date.Now)

    End Sub

    Private Sub m_frameParser_ReceivedDataFrame(ByVal frame As Tva.Phasors.IDataFrame) Handles m_frameParser.ReceivedDataFrame

        MapDataFrameMeasurements(frame)

    End Sub

    Private Sub MapMeasurement(ByVal frame As IDataFrame, ByVal key As String, ByVal measurement As IMeasurement)

        Dim measurementID As Integer

        ' Lookup synonym value in measurement ID list
        If m_measurementIDs.TryGetValue(key, measurementID) Then
            measurement.ID = measurementID
            frame.Measurements.Add(measurementID, measurement)
        End If

    End Sub

    Private Sub m_frameParser_ReceivedFrameBufferImage(ByVal binaryImage() As Byte, ByVal offset As Integer, ByVal length As Integer) Handles m_frameParser.ReceivedFrameBufferImage

        m_bytesReceived += length

    End Sub

End Class
