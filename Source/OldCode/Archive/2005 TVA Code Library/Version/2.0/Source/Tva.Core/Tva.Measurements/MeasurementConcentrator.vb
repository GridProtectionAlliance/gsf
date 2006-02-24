'***********************************************************************
'  MeasurementConcentrator.vb - Measurement Concentrator
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'  Note:  Note that your lag time is closely related to the rate
'  at which data data is coming into the concentrator.  DatAWare
'  allows some flexibility in its polling interval, so if you set
'  DatAWare to poll and broadcast at a rate of .10 seconds, you
'  might set the lag time to .50 to make sure you've had time to
'  receive all the data from the reporting servers - this works
'  since the data coming in is GPS based and should be very close
'  in time...
'
'***********************************************************************

Imports System.Text
Imports System.Threading
Imports Tva.Collections
Imports Tva.DateTime.Common

Namespace Measurements

    Public Class Concentrator

        Implements IDisposable

        Private m_sampleQueue As SampleQueue
        Private m_measurements As ImmediateMeasurements
        Private m_lagTime As Double
        Private m_measurementsPerSecond As Integer
        Private m_measurementRate As Decimal     ' We use a 64-bit floating point here to avoid round-off errors in calculations dealing with the sample rate
        Private m_evenMeasurementRate As Double
        Private m_processThread As Thread
        Private m_enabled As Boolean
        Private m_processing As Boolean
        Private m_startTime As Long
        Private m_stopTime As Long
        Private m_packetsPublished As Long
        Private m_sortMeasurementsOnThread As Boolean
        Private m_timeDeviationTolerance As Double
        Private WithEvents m_monitorTimer As Timers.Timer

        Public Event PublishMeasurements(ByVal measurements As MeasurementValues)
        Public Event SamplePublished(ByVal sample As MeasurementSample)
        Public Event UnpublishedSamples(ByVal total As Integer)
        Public Event StatusMessage(ByVal message As String)

        ' Note issues with Math.Floor:
        '   500 / (1000 / 30) = 15 (calculator shows round-off error: 15.000000000000000000000000000002)
        '   Using doubles  (32-bit floats): Math.Floor(500# / (1000# / 30#)) = 14
        '   Using decimals (64-bit floats): Math.Floor(500@ / (1000@ / 30@)) = 15

        Public Sub New(ByVal measurementsPerSecond As Integer, ByVal lagTime As Double, ByVal timeDeviationTolerance As Double)

            m_measurementsPerSecond = measurementsPerSecond
            m_measurementRate = 1000@ / measurementsPerSecond
            m_evenMeasurementRate = System.Math.Floor(m_measurementRate) - 1
            m_lagTime = lagTime
            m_enabled = True
            m_processing = False
            m_sortMeasurementsOnThread = True
            m_timeDeviationTolerance = timeDeviationTolerance
            'm_sampleQueue = New MeasurementSampleQueue(
            m_measurements = New ImmediateMeasurements
            m_monitorTimer = New Timers.Timer

            ' We check for samples that need to be removed every second
            With m_monitorTimer
                .Interval = 1000
                .AutoReset = True
                .Enabled = True
            End With

        End Sub

        Protected Overrides Sub Finalize()

            MyBase.Finalize()
            Dispose()

        End Sub

        Public Overridable Sub Dispose() Implements IDisposable.Dispose

            GC.SuppressFinalize(Me)
            Processing = False

        End Sub

        Public Property TimeDeviationTolerance() As Double
            Get
                Return m_timeDeviationTolerance
            End Get
            Set(ByVal value As Double)
                m_timeDeviationTolerance = value
            End Set
        End Property

        Public ReadOnly Property MeasurementsPerSecond() As Integer
            Get
                Return m_measurementsPerSecond
            End Get
        End Property

        Public ReadOnly Property MeasurementRate() As Decimal
            Get
                Return m_measurementRate
            End Get
        End Property

        'Public ReadOnly Property EvenMeasurementRate() As Double
        '    Get
        '        Return m_evenSampleRate
        '    End Get
        'End Property

        Public Sub DefineMeasurements(ByVal measurements As DataTable)

            'm_measurements.DefineMeasurements(measurements)

        End Sub

        ' Sorting items directly may provide a small speed improvement and will use less resources, however TCP stream
        ' processing can fall behind, so sorting measurements on a thread is recommended for TCP input streams
        Public Property SortMeasurementsOnThread() As Boolean
            Get
                Return m_sortMeasurementsOnThread
            End Get
            Set(ByVal value As Boolean)
                m_sortMeasurementsOnThread = value
            End Set
        End Property

        Public Sub SortMeasurement(ByVal measurement As IMeasurement)

            If m_sortMeasurementsOnThread Then
                m_sampleQueue.ThreadSortMeasurement(measurement)
            Else
                m_sampleQueue.SortMeasurement(measurement)
            End If

        End Sub

        Public ReadOnly Property SampleQueue() As SampleQueue
            Get
                Return m_sampleQueue
            End Get
        End Property

        Public ReadOnly Property LatestMeasurements() As ImmediateMeasurements
            Get
                Return m_measurements
            End Get
        End Property

        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal Value As Boolean)
                m_enabled = Value
            End Set
        End Property

        Public Property Processing() As Boolean
            Get
                Return m_processing
            End Get
            Set(ByVal Value As Boolean)
                m_processing = Value

                If m_processing Then
                    ' Reset stats...
                    m_stopTime = 0
                    m_packetsPublished = 0
                    m_startTime = Date.Now.Ticks

                    ' Start process thread
                    m_processThread = New Thread(AddressOf PublishDataStream)
                    m_processThread.Start()
                Else
                    ' Stop process thread
                    If Not m_processThread Is Nothing Then m_processThread.Abort()
                    m_stopTime = Date.Now.Ticks
                    m_processThread = Nothing
                End If
            End Set
        End Property

        Public ReadOnly Property RunTime() As Double
            Get
                Dim processingTime As Long

                If m_startTime > 0 Then
                    If m_stopTime > 0 Then
                        processingTime = m_stopTime - m_startTime
                    Else
                        processingTime = Date.Now.Ticks - m_startTime
                    End If
                End If

                If processingTime < 0 Then processingTime = 0

                Return processingTime / 10000000L
            End Get
        End Property

        Friend Sub UpdateStatus(ByVal message As String)

            RaiseEvent StatusMessage(message)

        End Sub

        Private m_currentTime As Date
        Private m_publishTime As Date
        Private m_measurementIndex As Integer

        Private Function CanPublishSample(ByVal ticks As Long, ByVal sample As MeasurementSample) As Boolean

            Dim allMeasurementsPublished As Boolean

            m_currentTime = Date.Now

            ' Check to see if it's time to broadcast...
            If m_currentTime.Ticks > m_publishTime.Ticks OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                ' Access proper data packet out of current sample

                If m_measurementIndex = -1 Then
                    ' Make sure we've got enough lag-time between this sample and the most recent sample...
                    If m_sampleQueue.DistanceFromBaseTime(sample.BaseTime) <= -m_lagTime OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                        m_measurementIndex = 0
                    End If
                End If

                If m_measurementIndex > -1 Then
                    With sample.Rows(m_measurementIndex)
                        RaiseEvent PublishMeasurements(.This)
                        .Published = True
                        m_packetsPublished += 1

                        ' Increment row index
                        m_measurementIndex += 1
                        allMeasurementsPublished = (m_measurementIndex = m_measurementsPerSecond)
                    End With
                End If

                ' Set next broadcast time
                m_publishTime = m_currentTime.AddMilliseconds(m_evenMeasurementRate)
            End If

            Return allMeasurementsPublished

        End Function

        Private Sub PublishSample(ByVal ticks As Long, ByVal sample As MeasurementSample)

            ' We published all the measurements of this sample, reset row index for next sample
            m_measurementIndex = -1

            ' We send out a notification that a new sample has been published so that anyone can have a chance
            ' to perform any last steps with the data before we remove it
            RaiseEvent SamplePublished(sample)


            ' Since a typical process rate is 30 samples per second do everything you can to make sure code
            ' here is optimized to execute as quickly as possible...
            'Dim currentSample As MeasurementSample
            Dim currentTime As Date
            Dim publishTime As Date = Date.Now.AddSeconds(m_lagTime)
            'Dim binaryLength As Integer
            Dim rowIndex As Integer = -1
            'Dim x As Integer

            Do While True
                Try
                    currentTime = Date.Now

                    ' Check to see if it's time to broadcast...
                    If currentTime.Ticks > publishTime.Ticks OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                        ' Access proper data packet out of current sample
                        If m_sampleQueue.Count > 0 Then
                            currentSample = m_sampleQueue(0)

                            If rowIndex = -1 Then
                                ' Make sure we've got enough lag-time between this sample and the most recent sample...
                                If m_sampleQueue.DistanceFromBaseTime(currentSample.BaseTime) <= -m_lagTime OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                                    rowIndex = 0
                                End If
                            End If

                            If rowIndex > -1 Then
                                With currentSample.Rows(rowIndex)
                                    RaiseEvent PublishMeasurements(.This)
                                    .Published = True
                                    m_packetsPublished += 1

                                    ' Increment row index
                                    rowIndex += 1

                                    If rowIndex = m_measurementsPerSecond Then
                                        ' We published this sample, reset row index for next sample
                                        rowIndex = -1

                                        ' We send out a notification that a new sample has been published so that anyone can have a chance
                                        ' to perform any last steps with the data before we remove it
                                        RaiseEvent SamplePublished(currentSample)
                                        m_sampleQueue.RemovePublishedSample()
                                    End If
                                End With
                            End If
                        End If

                        ' Set next broadcast time
                        publishTime = currentTime.AddMilliseconds(m_evenMeasurementRate)
                    End If

                    ' We sleep the thread between each loop to help reduce CPU loading...
                    Thread.Sleep(1)
                Catch ex As ThreadAbortException
                    ' We egress gracefully when the thread's being aborted
                    Exit Do
                Catch ex As Exception
                    UpdateStatus("Exception encountered during data stream broadcast: " & ex.Message)
                End Try
            Loop

        End Sub

        Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

            If m_enabled Then
                ' Start process thread if needed
                If m_sampleQueue.Count > 1 And Not m_processing Then
                    Processing = True
                End If

                RaiseEvent UnpublishedSamples(m_sampleQueue.Count - 1)
            ElseIf m_processing Then
                Processing = False
            End If

        End Sub

        Public ReadOnly Property Status() As String
            Get
                Dim publishingSample As Date
                Dim sampleDetail As String
                Dim currentTime As Date = BaselinedTimestamp(Date.Now.ToUniversalTime())

                With New StringBuilder
                    For x As Integer = 0 To m_sampleQueue.Count - 1
                        .Append(Environment.NewLine & "     Sample " & x & " @ " & m_sampleQueue(x).BaseTime.ToString("dd-MMM-yyyy HH:mm:ss") & ": ")

                        If x = 0 Then
                            .Append("publishing...")
                            publishingSample = m_sampleQueue(x).BaseTime
                        Else
                            .Append("concentrating...")
                        End If

                        .Append(Environment.NewLine)
                    Next

                    sampleDetail = .ToString
                End With

                With New StringBuilder
                    .Append("  Concentration process is: " & IIf(Enabled, "Enabled", "Disabled") & Environment.NewLine)
                    .Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & Environment.NewLine)
                    .Append("    Total process run time: " & SecondsToText(RunTime) & Environment.NewLine)
                    .Append("          Discarded points: " & m_sampleQueue.DiscardedPoints & Environment.NewLine)
                    .Append("          Defined lag time: " & m_lagTime & " seconds" & Environment.NewLine)
                    .Append("            Queued samples: " & m_sampleQueue.Count & Environment.NewLine)
                    .Append("         Current base time: " & m_sampleQueue.BaseTime.ToString("dd-MMM-yyyy HH:mm:ss") & Environment.NewLine)
                    .Append("       Current server time: " & currentTime.ToString("dd-MMM-yyyy HH:mm:ss") & Environment.NewLine)
                    .Append("        Most recent sample: " & m_sampleQueue.BaseTime.ToString("dd-MMM-yyyy HH:mm:ss") & ", " & m_sampleQueue.DistanceFromBaseTime(currentTime) & " second deviation" & Environment.NewLine)
                    .Append("         Publishing sample: " & publishingSample.ToString("dd-MMM-yyyy HH:mm:ss") & ", " & (currentTime.Ticks - publishingSample.Ticks) / 10000000L & " second deviation" & Environment.NewLine)
                    .Append("    Data packets published: " & m_packetsPublished & Environment.NewLine)
                    .Append("         Mean publish rate: " & (m_packetsPublished / RunTime).ToString("0.00") & " samples/sec" & Environment.NewLine)
                    .Append(Environment.NewLine & "Current sample detail:" & Environment.NewLine & sampleDetail)

                    Return .ToString()
                End With
            End Get
        End Property

    End Class

End Namespace
