'*******************************************************************************************************
'  Tva.Measurements.Concentrator.vb - Measurement concentrator
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  This class synchronizes (i.e., sorts by timestamp) real-time measurements
'
'  Note that your lag time should be defined as it relates to the rateat which data data is coming
'  into the concentrator. Make sure you allow enough time for transmission of data over the network
'  allowing any needed time for possible network congestion.
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'  02/23/2006 - James R Carroll
'       Added to TVA code library
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports Tva.Collections
Imports Tva.DateTime.Common

Namespace Measurements

    Public Class Concentrator

        Implements IDisposable

#Region " Public Member Declarations "

#Region " Sample Class "

        ''' <summary>This class represents a collection of frames over one second (i.e., a sample of data)</summary>
        Public Class Sample

            Implements IComparable

            Private m_parent As Concentrator        ' Parent concentrator
            Private m_frames As IFrame()            ' Array of frames
            Private m_ticks As Long                 ' Ticks at the beginning of sample

            Private Sub New(ByVal parent As Concentrator, ByVal ticks As Long)

                m_parent = m_parent
                m_ticks = ticks

                ' Create new array of frames for this sample
                CreateFrameArray()

            End Sub

            ''' <summary>Handy instance reference to self</summary>
            Public ReadOnly Property This() As Sample
                Get
                    Return Me
                End Get
            End Property

            ''' <summary>Array of frames in this sample</summary>
            Public ReadOnly Property Frames() As IFrame()
                Get
                    Return m_frames
                End Get
            End Property

            ''' <summary>Gets published state of this sample (i.e., all frames published)</summary>
            Public ReadOnly Property Published() As Boolean
                Get
                    Dim allPublished As Boolean = True

                    ' The sample has been completely processed once all frames have been published
                    For x As Integer = 0 To m_frames.Length - 1
                        If Not m_frames(x).Published Then
                            allPublished = False
                            Exit For
                        End If
                    Next

                    Return allPublished
                End Get
            End Property

            ''' <summary>Exact timestamp of the beginning of data sample</summary>
            ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
            Public Property Ticks() As Long
                Get
                    Return m_ticks
                End Get
                Set(ByVal value As Long)
                    m_ticks = value
                End Set
            End Property

            ''' <summary>Closest date representation of ticks of data sample</summary>
            Public ReadOnly Property Timestamp() As Date
                Get
                    Return New Date(m_ticks)
                End Get
            End Property

            ''' <summary>Samples are sorted by timestamp</summary>
            Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

                If TypeOf obj Is Sample Then
                    Return m_ticks.CompareTo(DirectCast(obj, Sample).Ticks)
                Else
                    Throw New ArgumentException("Sample can only be compared with other Samples...")
                End If

            End Function

            ' Create new array of frames for this sample...
            Private Sub CreateFrameArray()

                m_frames = Array.CreateInstance(GetType(IFrame), m_parent.FramesPerSecond)

                For x As Integer = 0 To m_frames.Length - 1
                    m_frames(x) = m_parent.CreateNewFrameFunction.Invoke()
                Next

            End Sub

        End Class

#End Region

        ''' <summary>This delegate is used to publish a frame</summary>
        Public Delegate Sub PublishFrameFunctionSignature(ByVal frame As IFrame)

        ''' <summary>This delegate is used to create new a frame</summary>
        Public Delegate Function CreateNewFrameFunctionSignature() As IFrame

        Public Event SamplePublished(ByVal sample As Sample)
        Public Event UnpublishedSamples(ByVal total As Integer)
        Public Event StatusMessage(ByVal message As String)

#End Region

#Region " Private Member Declarations "

        Private m_baseTime As Date                      ' This represents the most recent encountered timestamp baselined at the bottom of the second
        Private m_framesPerSecond As Integer            ' Frames per second
        Private m_frameRate As Decimal                  ' Frame rate - we use a 64-bit scaled integer to avoid round-off errors in calculations
        Private m_evenMeasurementRate As Double         ' Even frame rate (base on 64-bite frame rate) used to calculate next broadcast time
        Private m_timeDeviationTolerance As Double      ' Allowed time deviation tolerance
        Private m_discardedMeasurements As Long         ' Total number of discarded measurements
        Private m_frameIndex As Integer                 ' Current publishing frame index
        Private m_lagTime As Double                     ' Defined lag time for concentrator (can be subsecond)
        Private m_publishedFrames As Long               ' Total number of published frames
        Private m_enabled As Boolean                    ' Enabled state of concentrator
        Private m_publishTime As Date                   ' Time of next frame broadcast
        Private m_measurements As ImmediateMeasurements ' Absolute latest received measurement values
        Private m_sampleQueue As KeyedProcessQueue(Of Long, Sample)
        Private m_publishFrameFunction As PublishFrameFunctionSignature
        Private m_createNewFrameFunction As CreateNewFrameFunctionSignature
        Private WithEvents m_monitorTimer As Timers.Timer

#End Region

        Public Sub New( _
            ByVal publishFrameFunction As PublishFrameFunctionSignature, _
            ByVal createNewFrameFunction As CreateNewFrameFunctionSignature, _
            ByVal framesPerSecond As Integer, _
            ByVal lagTime As Double, _
            ByVal timeDeviationTolerance As Double)

            'MyBase.New(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, Timeout.Infinite, False, False)

            If framesPerSecond < 1 Then Throw New ArgumentException("framesPerSecond must be at least one")

            m_sampleQueue = KeyedProcessQueue(Of Long, Sample).CreateRealTimeQueue(AddressOf PublishSample, AddressOf CanPublishSample, Timeout.Infinite, False, False)

            m_publishFrameFunction = publishFrameFunction
            m_framesPerSecond = framesPerSecond
            m_frameRate = Convert.ToDecimal(SecondsToTicks(1)) / Convert.ToDecimal(framesPerSecond)
            m_evenMeasurementRate = System.Math.Floor(m_frameRate) - 1
            m_timeDeviationTolerance = timeDeviationTolerance
            m_baseTime = BaselinedTimestamp(Date.UtcNow)
            'm_sampleQueue = New SampleQueue(createNewSampleFunction, AddressOf PublishSample, AddressOf CanPublishSample, framesPerSecond, timeDeviationTolerance)
            m_lagTime = lagTime
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
            If m_sampleQueue IsNot Nothing Then m_sampleQueue.Stop()

        End Sub

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As Concentrator
            Get
                Return Me
            End Get
        End Property

        Public Property PublishFrameFunction() As PublishFrameFunctionSignature
            Get
                Return m_publishFrameFunction
            End Get
            Set(ByVal value As PublishFrameFunctionSignature)
                m_publishFrameFunction = value
            End Set
        End Property

        Public Property CreateNewFrameFunction() As CreateNewFrameFunctionSignature
            Get
                Return m_createNewFrameFunction
            End Get
            Set(ByVal value As CreateNewFrameFunctionSignature)
                m_createNewFrameFunction = value
            End Set
        End Property

        ''' <summary>Frames per second</summary>
        Public ReadOnly Property FramesPerSecond() As Integer
            Get
                Return m_framesPerSecond
            End Get
        End Property

        ''' <summary>Frame rate (i.e., ticks per frame)</summary>
        Public ReadOnly Property FrameRate() As Decimal
            Get
                Return m_frameRate
            End Get
        End Property

        Public Property LagTime() As Double
            Get
                Return m_lagTime
            End Get
            Set(ByVal value As Double)
                m_lagTime = value
            End Set
        End Property

        ''' <summary>Allowed base time deviation in seconds</summary>
        Public Property TimeDeviationTolerance() As Double
            Get
                Return m_timeDeviationTolerance
            End Get
            Set(ByVal value As Double)
                m_timeDeviationTolerance = value
            End Set
        End Property

        Protected ReadOnly Property SampleQueue() As KeyedProcessQueue(Of Long, Sample)
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
            Set(ByVal value As Boolean)
                If value Then
                    If Not m_enabled Then m_sampleQueue.Start()
                Else
                    If m_enabled Then m_sampleQueue.Stop()
                End If

                m_enabled = value
            End Set
        End Property

        ''' <summary>Timestamp of most recent measurement, or current time if no measurements are within time deviation tolerance</summary>
        Public ReadOnly Property BaseTime() As Date
            Get
                Dim currentTime As Date = BaselinedTimestamp(Date.UtcNow)

                ' If base time gets old, we fall back on local system time
                If System.Math.Abs(TicksToSeconds(currentTime.Ticks - m_baseTime.Ticks)) > m_timeDeviationTolerance Then
                    m_baseTime = currentTime
                End If

                Return m_baseTime
            End Get
        End Property

        ''' <summary>Seconds given timestamp are away from base time</summary>
        Public Function DistanceFromBaseTime(ByVal timeStamp As Date) As Double

            Return TicksToSeconds(timeStamp.Ticks - BaseTime.Ticks)

        End Function

        ''' <summary>Number of measurements that have been discarded because of bad timestamps</summary>
        Public ReadOnly Property DiscardedMeasurements() As Long
            Get
                Return m_discardedMeasurements
            End Get
        End Property

        ''' <summary>Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position</summary>
        Public Sub SortMeasurement(ByVal measurement As IMeasurement)

            With measurement
                ' Get sample for this timestamp, creating it if needed
                Dim sample As Sample = GetSample(.Timestamp)

                If sample Is Nothing Then
                    ' No samples exist for this timestamp - data must be old
                    m_discardedMeasurements += 1
                Else
                    ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                    ' proper frame index (i.e., the row) - we can then directly access the correct measurement using the index
                    sample.Frames(System.Math.Floor((.Ticks + 1@) / m_frameRate)).Measurements(.Index).Value = .Value

                    ' TODO: Update static immediate measurements (local member instance) here...
                End If
            End With

        End Sub

        ''' <summary>Queues measurement for sorting in the thread pool</summary>
        ''' <remarks>
        ''' Sorting items directly may provide a small speed improvement and will use less resources, however TCP stream
        ''' processing can fall behind, so sorting measurements on a thread is recommended for TCP input streams
        ''' </remarks>
        Public Sub QueueMeasurementForSorting(ByVal measurement As IMeasurement)

            ThreadPool.QueueUserWorkItem(AddressOf SortMeasurement, measurement)

        End Sub

        Private Sub SortMeasurement(ByVal state As Object)

            SortMeasurement(DirectCast(state, IMeasurement))

        End Sub

        ' Each sample consists of several frames - the sample represents one second of data, so all frames
        ' are to get published during this second.  Typically the process queue's "process item function"
        ' does the work of the queue - but in this case we use the "can process item function" to process
        ' each frame in the sample until all frames have been published.
        Private Function CanPublishSample(ByVal ticks As Long, ByVal sample As Sample) As Boolean

            ' This function is executed in a real-time thread - make sure any work to be done here
            ' is executed as efficiently as possible
            Dim allFramesPublished As Boolean
            Dim currentTime As Date = Date.Now

            ' Check to see if it's time to broadcast...
            If currentTime.Ticks > m_publishTime.Ticks OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                ' Access proper frame out of the current sample
                If m_frameIndex = -1 Then
                    ' Make sure we've got enough lag-time between this sample and the most recent sample...
                    If DistanceFromBaseTime(sample.Timestamp) <= -m_lagTime OrElse m_sampleQueue.Count > m_lagTime + 1 Then
                        m_frameIndex = 0
                    End If
                End If

                If m_frameIndex > -1 Then
                    With sample.Frames(m_frameIndex)
                        m_publishFrameFunction(.This)
                        .Published = True
                        m_publishedFrames += 1

                        ' Increment row index
                        m_frameIndex += 1
                        allFramesPublished = (m_frameIndex >= m_framesPerSecond)
                    End With
                End If

                ' Set next broadcast time
                m_publishTime = currentTime.AddMilliseconds(m_evenMeasurementRate)
            End If

            ' We will say sample is ready to be discarded once all frames have been published
            Return allFramesPublished

        End Function

        Private Sub PublishSample(ByVal ticks As Long, ByVal sample As Sample)

            ' We published all the measurements of this sample, reset row index for next sample
            m_frameIndex = -1

            ' We send out a notification that a new sample has been published so that anyone can have a chance
            ' to perform any last steps with the data before we remove it
            RaiseEvent SamplePublished(sample)

        End Sub

        Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

            RaiseEvent UnpublishedSamples(m_sampleQueue.Count - 1)

        End Sub

        ''' <summary>This critical function automatically manages the sample queue based on timestamps of incoming measurements</summary>
        ''' <returns>The sample associated with the specified timestamp. If the sample is not found at timestamp, it will be created.</returns>
        ''' <param name="timestamp">The timestamp of the sample to get.</param>
        Protected Function GetSample(ByVal timestamp As Date) As Sample

            ' Baseline timestamp at bottom of the second
            Dim sample As Sample = LookupSample(timestamp)

            ' Wait until the sample exists or we can enter critical section to create it ourselves
            Do Until sample IsNot Nothing
                ' We don't want to step on our own toes when creating new samples - so we create a critical section for
                ' this code - if another thread is busy creating samples, we'll just try again
                If Monitor.TryEnter(m_sampleQueue.SyncRoot) Then
                    Try
                        ' Check difference between timestamp and basetime in seconds and fill in any gaps.  Note that
                        ' current base time will be validated against local clock in call to DistanceFromBaseTime
                        Dim difference As Double = DistanceFromBaseTime(timestamp)

                        If System.Math.Abs(difference) > m_timeDeviationTolerance Then
                            ' This data has come in late or has a future timestamp.  For old timestamps, we're not
                            ' going to create a sample for data that will never be processed.  For future dates we
                            ' must assume that the clock from source device must be advanced and out-of-sync with
                            ' real-time - either way this data will be discarded.
                            Exit Do
                        ElseIf difference > 1 Then
                            ' Add intermediate samples as needed...
                            For x As Integer = 1 To System.Math.Floor(difference)
                                CreateSample(m_baseTime.AddSeconds(x))
                            Next
                        End If

                        ' Set this time as the new base time
                        m_baseTime = BaselinedTimestamp(timestamp)
                        CreateSample(m_baseTime)
                    Catch
                        ' Rethrow any exceptions - we are just catching any exceptions so we can
                        ' make sure to release thread lock in finally
                        Throw
                    Finally
                        Monitor.Exit(m_sampleQueue.SyncRoot)
                    End Try
                Else
                    ' We sleep the thread between loops to help reduce CPU loading...
                    Thread.Sleep(1)
                End If

                ' If we just created the sample we needed, then we'll get it here.  Otherwise the sample may have been
                ' created by another thread while we were sleeping, so we'll check again to see to see if sample exists.
                ' Additionally, the Item property internally performs a SyncLock on the SyncRoot and waits for it to be
                ' released, so if another thread was creating new samples then we'll definitely pick up our needed
                ' sample when the lock becomes available.  Nice and safe.
                sample = LookupSample(timestamp)
            Loop

            ' Return sample for this timestamp
            Return sample

        End Function

        ''' <summary>Gets the sample associated with the specified timestamp.</summary>
        ''' <returns>The sample associated with the specified timestamp. If the specified timestamp is not found, property returns null.</returns>
        ''' <param name="timestamp">The timestamp of the sample to get.</param>
        Private Function LookupSample(ByVal timestamp As Date) As Sample

            Try
                ' Return sample with specified baselined ticks
                Return m_sampleQueue(BaselinedTimestamp(timestamp).Ticks)
            Catch ex As KeyNotFoundException
                ' We'll return null if it's not found
                Return Nothing
            Catch
                Throw
            End Try

        End Function

        ''' <summary>Gets the sample associated at the specified timestamp.</summary>
        ''' <param name="baseTime">The basetime of the sample to create.</param>
        Private Sub CreateSample(ByVal baseTime As Date)

            With m_sampleQueue
                If Not .ContainsKey(baseTime.Ticks) Then .Add(baseTime.Ticks, New Sample(Me, baseTime.Ticks))
            End With

        End Sub

        Public ReadOnly Property Status() As String
            Get
                Dim publishingSampleTimestamp As Date
                Dim sampleDetail As New StringBuilder
                Dim currentTime As Date = BaselinedTimestamp(Date.UtcNow)

                With sampleDetail
                    For x As Integer = 0 To m_sampleQueue.Count - 1
                        .Append(Environment.NewLine)
                        .Append("     Sample ")
                        .Append(x)
                        .Append(" @ ")
                        .Append(m_sampleQueue(x).Value.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
                        .Append(": ")

                        If x = 0 Then
                            .Append("publishing...")
                            publishingSampleTimestamp = m_sampleQueue(x).Value.Timestamp
                        Else
                            .Append("concentrating...")
                        End If

                        .Append(Environment.NewLine)
                    Next
                End With

                With New StringBuilder
                    .Append(m_sampleQueue.Status)
                    .Append("     Data concentration is: ")
                    If m_enabled Then
                        .Append("Enabled")
                    Else
                        .Append("Disabled")
                    End If
                    .Append(Environment.NewLine)
                    .Append("    Discarded measurements: ")
                    .Append(m_discardedMeasurements)
                    .Append(Environment.NewLine)
                    .Append("          Defined lag time: ")
                    .Append(m_lagTime)
                    .Append(" seconds")
                    .Append(Environment.NewLine)
                    .Append("       Current server time: ")
                    .Append(currentTime.ToString("dd-MMM-yyyy HH:mm:ss"))
                    .Append(Environment.NewLine)
                    .Append("        Most recent sample: ")
                    .Append(m_baseTime.ToString("dd-MMM-yyyy HH:mm:ss"))
                    .Append(", ")
                    .Append(DistanceFromBaseTime(currentTime))
                    .Append(" second deviation")
                    .Append(Environment.NewLine)
                    .Append("         Publishing sample: ")
                    .Append(publishingSampleTimestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
                    .Append(", ")
                    .Append(TicksToSeconds(currentTime.Ticks - publishingSampleTimestamp.Ticks))
                    .Append(" second deviation")
                    .Append(Environment.NewLine)
                    .Append("    Total published frames: ")
                    .Append(m_publishedFrames)
                    .Append(Environment.NewLine)
                    .Append("   Mean frame publish rate: ")
                    .Append((m_publishedFrames / m_sampleQueue.RunTime).ToString("0.00"))
                    .Append(" frames/sec")
                    .Append(Environment.NewLine)
                    .Append(Environment.NewLine)
                    .Append("Current sample detail:")
                    .Append(Environment.NewLine)
                    .Append(sampleDetail.ToString)

                    Return .ToString()
                End With
            End Get
        End Property

    End Class

End Namespace
