'*******************************************************************************************************
'  TVA.Measurements.Concentrator.vb - Measurement concentrator
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  This class synchronizes (i.e., sorts by timestamp) real-time measurements
'
'  Note that your lag time should be defined as it relates to the rate at which data data is coming
'  into the concentrator. Make sure you allow enough time for transmission of data over the network
'  allowing any needed time for possible network congestion.
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated for Super Phasor Data Concentrator
'  02/23/2006 - J. Ritchie Carroll
'       Classes abstracted for general use and added to TVA code library
'  04/23/2007 - J. Ritchie Carroll
'       Migrated concentrator to use a base class model instead of using delegates
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports TVA.Collections
Imports TVA.DateTime
Imports TVA.DateTime.Common
Imports TVA.Math.Common

Namespace Measurements

    Public MustInherit Class ConcentratorBase

        Implements IDisposable

#Region " Public Member Declarations "

        ''' <summary>This event is raised after a sample is published so that consumers may handle any last minute operations on a sample before it gets released</summary>
        Public Event SamplePublished(ByVal sample As Sample)

        ''' <summary>This event gets raised every second allowing consumer to track current number of unpublished samlples</summary>
        Public Event UnpublishedSamples(ByVal total As Integer)

        ''' <summary>This event will be raised if there is an exception encountered while attempting to process a frame in the sample queue</summary>
        ''' <remarks>Processing won't stop for any exceptions thrown by the user function, but any captured exceptions will be exposed through this event</remarks>
        Public Event ProcessException(ByVal ex As Exception)

#End Region

#Region " Private Member Declarations "

        Friend Event LeadTimeUpdated(ByVal leadTime As Double)                  ' Raised, for the benefit of dependent classes, when lead time is updated
        Friend Event LagTimeUpdated(ByVal lagTime As Double)                    ' Raised, for the benefit of dependent classes, when lag time is updated


        Private m_useLocalClockAsRealTime As Boolean                            ' Determines whether ot not to use local system clock as "real-time"
        Private m_realTimeTicks As Long                                         ' Ticks of the most recently received measurement (i.e., real-time)
        Private m_currentSampleTimestamp As Date                                ' Timestamp of current real-time value baselined at the bottom of the second
        Private m_framesPerSecond As Integer                                    ' Frames per second
        Private m_lagTime As Double                                             ' Allowed past time deviation tolerance
        Private m_leadTime As Double                                            ' Allowed future time deviation tolerance
        Private m_ticksPerFrame As Decimal                                      ' Frame rate - we use a 64-bit scaled integer to avoid round-off errors in calculations
        Private m_frameIndex As Integer                                         ' Current publishing frame index
        Private m_measurementsSortedByArrival As Long                           ' Total number of measurements that were sorted by arrival
        Private m_discardedMeasurements As Long                                 ' Total number of discarded measurements
        Private m_publishedMeasurements As Long                                 ' Total number of published measurements
        Private m_publishedFrames As Long                                       ' Total number of published frames
        Private m_enabled As Boolean                                            ' Enabled state of concentrator
        Private m_trackLatestMeasurements As Boolean                            ' Determines whether or not to track latest measurements
        Private m_latestMeasurements As ImmediateMeasurements                   ' Absolute latest received measurement values
        Private WithEvents m_sampleQueue As KeyedProcessQueue(Of Long, Sample)  ' Sample processing queue
        Private WithEvents m_monitorTimer As Timers.Timer                       ' Sample monitor

#End Region

#Region " Construction Functions "

        ''' <summary>Creates a new measurement concentrator</summary>
        ''' <param name="framesPerSecond">Number of frames to publish per second</param>
        ''' <param name="lagTime">Allowed past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins</param>
        ''' <param name="leadTime">Allowed future time deviation tolerance, in seconds</param>
        ''' <remarks>
        ''' <para>framesPerSecond must be at least one.</para>
        ''' <para>lagTime must be greater than zero but can be specified in sub-second intervals (e.g., set to .25 for a quarter-second lag time) - note that this defines time sensitivity to past timestamps.</para>
        ''' <para>leadTime must be greater than zero but can be specified in sub-second intervals (e.g., set to .5 for a half-second lead time) - note that this defines time sensitivity to future timestamps.</para>
        ''' <para>Note that concentration will not begin until consumer sets Enabled = True.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">Specified argument is outside of allowed value range (see remarks).</exception>
        Protected Sub New( _
            ByVal framesPerSecond As Integer, _
            ByVal lagTime As Double, _
            ByVal leadTime As Double)

            If framesPerSecond < 1 Then Throw New ArgumentOutOfRangeException("framesPerSecond", "framesPerSecond must be at least one")
            If lagTime <= 0 Then Throw New ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one")
            If leadTime <= 0 Then Throw New ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one")

            ' We create a real-time processing queue so we can process frames as quickly as needed (e.g., 30 frames per second)
            m_sampleQueue = KeyedProcessQueue(Of Long, Sample).CreateRealTimeQueue(AddressOf PublishSample, AddressOf CanPublishSample)

            Dim currentTime As Date = Date.UtcNow

            m_realTimeTicks = currentTime.Ticks
            m_currentSampleTimestamp = BaselinedTimestamp(currentTime, BaselineTimeInterval.Second)
            m_framesPerSecond = framesPerSecond
            m_ticksPerFrame = CDec(SecondsToTicks(1)) / CDec(framesPerSecond)
            m_lagTime = lagTime
            m_leadTime = leadTime
            m_latestMeasurements = New ImmediateMeasurements(Me)
            m_monitorTimer = New Timers.Timer

            ' We monitor total number of unpublished samples every second - this is a useful statistic to monitor, if
            ' total number of unpublished samples exceed lag time, measurement concentration could be falling behind
            With m_monitorTimer
                .Interval = 1000
                .AutoReset = True
                .Enabled = False
            End With

        End Sub

#End Region

#Region " Public Methods Implementation "

        ''' <summary>Handy instance reference to self</summary>
        Public ReadOnly Property This() As ConcentratorBase
            Get
                Return Me
            End Get
        End Property

        ''' <summary>Allowed past time deviation tolerance in seconds (can be subsecond)</summary>
        ''' <remarks>
        ''' <para>This value defines the time sensitivity to past measurement timestamps.</para>
        ''' <para>Defined the number of seconds allowed before assuming a measurement timestamp is too old.</para>
        ''' <para>This becomes the amount of "delay" introduced by the concentrator to allow time for data to flow into the system.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one</exception>
        Public Property LagTime() As Double
            Get
                Return m_lagTime
            End Get
            Set(ByVal value As Double)
                If value <= 0 Then Throw New ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one")
                m_lagTime = value
                RaiseEvent LagTimeUpdated(m_lagTime)
            End Set
        End Property

        ''' <summary>Allowed future time deviation tolerance in seconds (can be subsecond)</summary>
        ''' <remarks>
        ''' <para>This value defines the time sensitivity to future measurement timestamps.</para>
        ''' <para>Defined the number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        ''' </remarks>
        ''' <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one</exception>
        Public Property LeadTime() As Double
            Get
                Return m_leadTime
            End Get
            Set(ByVal value As Double)
                If value <= 0 Then Throw New ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one")
                m_leadTime = value
                RaiseEvent LeadTimeUpdated(m_leadTime)
            End Set
        End Property

        ''' <summary>Enables tracking of absolute latest received measurement values</summary>
        ''' <remarks>Enabling this feature will increase required sorting time</remarks>
        Public Property TrackLatestMeasurements() As Boolean
            Get
                Return m_trackLatestMeasurements
            End Get
            Set(ByVal value As Boolean)
                m_trackLatestMeasurements = value
            End Set
        End Property

        ''' <summary>Absolute latest received measurement values</summary>
        Public ReadOnly Property LatestMeasurements() As ImmediateMeasurements
            Get
                Return m_latestMeasurements
            End Get
        End Property

        ''' <summary>Frames per second</summary>
        Public Property FramesPerSecond() As Integer
            Get
                Return m_framesPerSecond
            End Get
            Set(ByVal value As Integer)
                m_framesPerSecond = value
            End Set
        End Property

        ''' <summary>Ticks per frame</summary>
        Public ReadOnly Property TicksPerFrame() As Decimal
            Get
                Return m_ticksPerFrame
            End Get
        End Property

        ''' <summary>Gets or sets current enabled state of concentrator</summary>
        ''' <returns>Current enabled state of concentrator</returns>
        ''' <remarks>Concentrator must be enabled (i.e., Enabled = True) before concentration will begin</remarks>
        Public Overridable Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal value As Boolean)
                If value Then
                    If Not m_enabled Then
                        ' Start real-time process queue
                        m_frameIndex = 0
                        m_sampleQueue.Start()
                    End If
                Else
                    If m_enabled Then m_sampleQueue.Stop()
                End If

                m_enabled = value
                m_monitorTimer.Enabled = value
            End Set
        End Property

        ''' <summary>Baselined timestamp of newest sample</summary>
        Public ReadOnly Property CurrentSampleTimestamp() As Date
            Get
                Return m_currentSampleTimestamp
            End Get
        End Property

        ''' <summary>Determines whether or not to use the local clock time as real-time</summary>
        ''' <remarks>
        ''' You should only use your local system clock as real-time if the time is locally GPS synchronized
        ''' or the measurement values being sorted were not measured relative to a GPS synchronized clock
        ''' </remarks>
        Public Property UseLocalClockAsRealTime() As Boolean
            Get
                Return m_useLocalClockAsRealTime
            End Get
            Set(ByVal value As Boolean)
                m_useLocalClockAsRealTime = value
            End Set
        End Property

        ''' <summary>
        ''' If using local clock as real-time this function will return UtcNow.Ticks, otherwise this function will return
        ''' ticks of most recent measurement, or local clock ticks if no measurements are within time deviation tolerances
        ''' </summary>
        ''' <remarks>
        ''' Because the measurements being received by remote devices are often measured relative to GPS time these timestamps
        ''' are typically more accurate than the local clock, as a result we can use the latest received timestamp as the best
        ''' local time measurement we have - but even these times can be incorrect, so we still have to apply reasonability
        ''' checks to these times, so we use the local time and the defined lag and lead times to validate the latest measured
        ''' timestamp.  If the newest received measurement timestamp gets too old (as defined by lag time) or creeps too far
        ''' into the future (as defined by lead time), we will fall back on local system time.  Note that this creates a
        ''' dependency on a fairly accurate local clock - the smaller the time deviation tolerances the better the needed local
        ''' clock acuracy.  For example, time deviation tolerances of a few seconds might only require keeping the local clock
        ''' synchronized to an NTP time source but sub-second tolerances would require that the local clock be very close to
        ''' GPS time.
        ''' </remarks>
        Public ReadOnly Property RealTimeTicks() As Long
            Get
                Dim currentTimeTicks As Long = Date.UtcNow.Ticks

                ' If the current value for real-time is outside of the time deviation tolerance of the local clock
                ' then we set latest measurement time (i.e., real-time) to be the current local clock time.  Note
                ' that we still need to validate this value regardless of whether or not we are using the local clock
                ' as real-time since the internal m_realTimeTicks value is used for creating measurement samples
                If Not TimeIsValid(currentTimeTicks, m_realTimeTicks, m_lagTime, m_leadTime) Then
                    m_realTimeTicks = currentTimeTicks
                    m_currentSampleTimestamp = BaselinedTimestamp(currentTimeTicks, BaselineTimeInterval.Second)
                End If

                If m_useLocalClockAsRealTime Then
                    ' Assuming local system clock is the best value we have for real-time
                    Return currentTimeTicks
                Else
                    ' Assuming lastest measurement timestamp is the best value we have for real-time
                    Return m_realTimeTicks
                End If
            End Get
        End Property

        ''' <summary>Seconds given number of ticks is away from real-time</summary>
        Public Function DistanceFromRealTime(ByVal ticks As Long) As Double

            Return TicksToSeconds(RealTimeTicks - ticks)

        End Function

        ''' <summary>Total number of measurements that have been discarded because of old timestamps (i.e., measurements that were outside the time deviation tolerance from base time - past or future)</summary>
        Public ReadOnly Property DiscardedMeasurements() As Long
            Get
                Return m_discardedMeasurements
            End Get
        End Property

        ''' <summary>Total number of published measurements</summary>
        Public ReadOnly Property PublishedMeasurements() As Long
            Get
                Return m_publishedMeasurements
            End Get
        End Property

        ''' <summary>Total number of published frames</summary>
        Public ReadOnly Property PublishedFrames() As Long
            Get
                Return m_publishedFrames
            End Get
        End Property

        ''' <summary>Total number of measurements that were sorted by arrival because the measurement reported a bad timestamp quality</summary>
        Public ReadOnly Property MeasurementsSortedByArrival() As Long
            Get
                Return m_measurementsSortedByArrival
            End Get
        End Property

        ''' <summary>Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position</summary>
        Public Overridable Sub SortMeasurement(ByVal measurement As IMeasurement)

            ' If measurement timestamp is not accurate, we'll set its timestamp to real-time and sort the measurement by arrival
            If Not measurement.TimestampQualityIsGood Then
                measurement.Ticks = RealTimeTicks
                m_measurementsSortedByArrival += 1
            End If

            Dim ticks As Long = measurement.Ticks
            Dim sample As Sample = GetSample(ticks)

            If sample Is Nothing Then
                ' No samples exist for this timestamp - measurement must have been outside time deviation tolerance (past or future) 
                m_discardedMeasurements += 1
            Else
                ' We've found the right sample for this data, so we access the proper data cell by first calculating the
                ' proper frame index (i.e., the row) - we can then directly access the correct measurement using its key
                Dim frame As IFrame = sample.Frames(Convert.ToInt32(TicksBeyondSecond(ticks) / m_ticksPerFrame))

                ' Call user customizable function to assign new measurement to its frame
                AssignMeasurementToFrame(frame, measurement)

                ' Track absolute lastest measurement timestamp...
                Dim currentTimeTicks As Long = Date.UtcNow.Ticks

                ' If the specified date is newer than the current value and is within the specified time
                ' deviation tolerance of the local clock time then we set the new date as "real-time"
                If ticks > m_realTimeTicks Then
                    If TimeIsValid(currentTimeTicks, ticks, m_lagTime, m_leadTime) Then
                        ' New time measurement looks good, assume this time as "real-time"
                        m_realTimeTicks = ticks
                        m_currentSampleTimestamp = BaselinedTimestamp(m_realTimeTicks, BaselineTimeInterval.Second)
                    ElseIf Not TimeIsValid(currentTimeTicks, m_realTimeTicks, m_lagTime, m_leadTime) Then
                        ' New time measurement was invalid and current real-time value was old so we
                        ' assume the current time as "real-time"
                        m_realTimeTicks = currentTimeTicks
                        m_currentSampleTimestamp = BaselinedTimestamp(currentTimeTicks, BaselineTimeInterval.Second)
                    End If
                End If

                ' Update latest measurement sort time for this frame
                If m_useLocalClockAsRealTime Then
                    frame.SortTicks = currentTimeTicks
                Else
                    frame.SortTicks = m_realTimeTicks
                End If

                ' Track absolute latest measurement values
                If m_trackLatestMeasurements Then m_latestMeasurements.UpdateMeasurementValue(measurement)
            End If

        End Sub

        ''' <summary>Queues measurement for sorting in the thread pool</summary>
        ''' <remarks>
        ''' Sorting items directly may provide a small speed improvement and will use less resources, however TCP stream processing
        ''' can fall behind under heavy load, so sorting measurements on a thread may be required for high-volume TCP input streams
        ''' </remarks>
        Public Overridable Sub QueueMeasurementForSorting(ByVal measurement As IMeasurement)

            ThreadPool.UnsafeQueueUserWorkItem(AddressOf SortMeasurement, measurement)

        End Sub

        ''' <summary>Detailed current state and status of concentrator</summary>
        Public Overridable ReadOnly Property Status() As String
            Get
                Dim publishingSampleTimestamp As Date
                Dim sampleDetail As New StringBuilder
                Dim currentTime As Date = Date.UtcNow

                With sampleDetail
                    Dim currentSample As Sample

                    SyncLock m_sampleQueue.SyncRoot
                        For x As Integer = 0 To m_sampleQueue.Count - 1
                            ' Get next sample
                            currentSample = m_sampleQueue(x).Value

                            .Append(Environment.NewLine)
                            .Append("     Sample ")
                            .Append(x)
                            .Append(" @ ")
                            .Append(currentSample.Timestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
                            .Append(": ")

                            If x = 0 Then
                                Dim frameIndex As Integer = m_frameIndex
                                Dim currentFrame As IFrame = currentSample.Frames(frameIndex)

                                ' Track timestamp of sample being published
                                publishingSampleTimestamp = currentSample.Timestamp

                                ' Append current frame detail
                                .Append("publishing...")
                                .Append(Environment.NewLine)
                                .Append(Environment.NewLine)
                                .Append("       Current frame =")
                                .Append(frameIndex + 1)
                                .Append(" - sort time: ")

                                ' Calculate maximum sort time for publishing frame
                                .Append(TicksToSeconds(currentFrame.SortTicks - currentFrame.Ticks).ToString("0.0000"))
                                .Append(" seconds")
                            Else
                                .Append("concentrating...")
                            End If

                            .Append(Environment.NewLine)
                        Next
                    End SyncLock
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
                    .Append("          Defined lag time: ")
                    .Append(m_lagTime)
                    .Append(" seconds")
                    .Append(Environment.NewLine)
                    .Append("         Defined lead time: ")
                    .Append(m_leadTime)
                    .Append(" seconds")
                    .Append(Environment.NewLine)
                    .Append("          Local clock time: ")
                    .Append(currentTime.ToString("dd-MMM-yyyy HH:mm:ss.fff"))
                    .Append(Environment.NewLine)
                    .Append("      Local clock accuracy: ")
                    .Append(DistanceFromRealTime(Date.UtcNow.Ticks).ToString("0.0000"))
                    .Append(" second deviation from real-time")
                    .Append(Environment.NewLine)
                    .Append("        Most recent sample: ")
                    .Append(m_currentSampleTimestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
                    .Append(", ")
                    .Append(TicksToSeconds(currentTime.Ticks - m_currentSampleTimestamp.Ticks).ToString("0"))
                    .Append(" second deviation")
                    .Append(Environment.NewLine)
                    .Append("         Publishing sample: ")
                    .Append(publishingSampleTimestamp.ToString("dd-MMM-yyyy HH:mm:ss"))
                    .Append(", ")
                    .Append(TicksToSeconds(currentTime.Ticks - publishingSampleTimestamp.Ticks).ToString("0"))
                    .Append(" second deviation")
                    .Append(Environment.NewLine)
                    .Append("    Published measurements: ")
                    .Append(m_publishedMeasurements)
                    .Append(Environment.NewLine)
                    .Append("    Discarded measurements: ")
                    .Append(m_discardedMeasurements)
                    .Append(Environment.NewLine)
                    .Append("    Total sorts by arrival: ")
                    .Append(m_measurementsSortedByArrival)
                    .Append(Environment.NewLine)
                    .Append("Published measurement loss: ")
                    .Append((m_discardedMeasurements / NotLessThan(m_publishedMeasurements, m_discardedMeasurements)).ToString("##0.0000%"))
                    .Append(Environment.NewLine)
                    .Append("    Total published frames: ")
                    .Append(m_publishedFrames)
                    .Append(Environment.NewLine)
                    .Append("        Defined frame rate: ")
                    .Append(m_framesPerSecond)
                    .Append(" frames/sec, ")
                    .Append(m_ticksPerFrame.ToString("0.00"))
                    .Append(" ticks/frame")
                    .Append(Environment.NewLine)
                    .Append("    Actual mean frame rate: ")
                    .Append((m_publishedFrames / (m_sampleQueue.RunTime - m_lagTime)).ToString("0.00"))
                    .Append(" frames/sec")
                    .Append(Environment.NewLine)
                    .Append(Environment.NewLine)
                    .Append("Current sample detail:")
                    .Append(Environment.NewLine)
                    .Append(sampleDetail.ToString)
                    .Append(Environment.NewLine)

                    Return .ToString()
                End With
            End Get
        End Property

        ''' <summary>Shuts down concentrator and clears sample queue in an orderly fashion</summary>
        Public Overridable Sub Dispose() Implements IDisposable.Dispose

            ' If user calls dispose, then we pull class out of garage collection finilzation queue
            GC.SuppressFinalize(Me)

            If m_sampleQueue IsNot Nothing Then
                Enabled = False
                m_sampleQueue.Clear()
            End If

            m_sampleQueue = Nothing

        End Sub

#End Region

#Region " Protected Methods Implementation "

        ''' <summary>Consumers must override this method in order to publish a frame</summary>
        ''' <remarks>Implementors are expected to return total published measurements as return value</remarks>
        Protected MustOverride Sub PublishFrame(ByVal frame As IFrame, ByVal index As Integer)

        ''' <summary>Consumers can choose to override this method to create a new custom frame</summary>
        ''' <remarks>Override is optional, the base class will create a basic frame to hold synchronized measurements</remarks>
        Protected Friend Overridable Function CreateNewFrame(ByVal ticks As Long) As IFrame

            Return New Frame(ticks)

        End Function

        ''' <summary>Consumers can choose to override this method to handle customize assignment of a measurement to its frame</summary>
        ''' <remarks>
        ''' <para>Override is optional, a measurement will simply be assigned to frame's measurement list otherwise</para>
        ''' <para>The frame's measurement dictionary should be synclocked prior to use by consumer</para>
        ''' </remarks>
        Protected Overridable Sub AssignMeasurementToFrame(ByVal frame As IFrame, ByVal measurement As IMeasurement)

            Dim frameMeasurement As IMeasurement

            SyncLock frame.Measurements
                If frame.Measurements.TryGetValue(measurement.Key, frameMeasurement) Then
                    ' Measurement already exists, so we just update with the latest value
                    frameMeasurement.Value = measurement.Value
                Else
                    ' Create new frame measurement if it doesn't exist
                    frame.Measurements.Add(measurement.Key, measurement)
                End If
            End SyncLock

        End Sub

        ''' <summary>We implement finalizer for this class to ensure sample queue shuts down in an orderly fashion</summary>
        Protected Overrides Sub Finalize()

            MyBase.Finalize()
            Dispose()

        End Sub

        ''' <summary>Sample processing queue</summary>
        Protected ReadOnly Property SampleQueue() As KeyedProcessQueue(Of Long, Sample)
            Get
                Return m_sampleQueue
            End Get
        End Property

        ''' <summary>This critical function automatically manages the sample queue based on timestamps of incoming measurements</summary>
        ''' <returns>The sample associated with the specified timestamp. If the sample is not found at timestamp, it will be created.</returns>
        ''' <param name="ticks">Ticks of the timestamp of the sample to get</param>
        ''' <remarks>Function will return null if timestamp is outside of the specified time deviation tolerance</remarks>
        Protected Function GetSample(ByVal ticks As Long) As Sample

            ' Baseline measurement timestamp at bottom of the second
            Dim baseTimeTicks As Long = BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks
            Dim sample As Sample = LookupSample(baseTimeTicks)

            ' Enter loop to wait until the sample exists, we will attempt to enter critical section and create it ourselves
            Do Until sample IsNot Nothing
                ' We don't want to step on our own toes when creating new samples - so we create a critical section for
                ' this code - if another thread is busy creating samples, we'll just wait for it below
                If Monitor.TryEnter(m_sampleQueue.SyncRoot) Then
                    Try
                        ' Check difference between timestamp and current sample base-time in seconds and fill in any gaps.
                        ' Note that current sample base-time will be validated against local clock
                        Dim distance As Double = DistanceFromRealTime(ticks)

                        If distance > m_lagTime OrElse distance < -m_leadTime Then
                            ' This data has come in late or has a future timestamp.  For old timestamps, we're not
                            ' going to create a sample for data that will never be processed.  For future dates we
                            ' must assume that the clock from source device must be advanced and out-of-sync with
                            ' real-time - either way this data will be discarded.
                            Exit Do
                        ElseIf distance > 1 Then
                            ' Add intermediate samples as needed...
                            For x As Integer = 1 To System.Math.Floor(distance)
                                CreateSample(m_currentSampleTimestamp.AddSeconds(x).Ticks)
                            Next
                        End If

                        ' Create sample for new base time
                        CreateSample(baseTimeTicks)
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
                ' Additionally, the TryGetValue function (referenced from within LookupSample) internally performs a
                ' SyncLock on the SyncRoot and waits for it to be released, so if another thread was creating new
                ' samples then we'll definitely pick up our needed sample when the lock is released.  Nice and safe.
                sample = LookupSample(baseTimeTicks)
            Loop

            ' Return sample for this timestamp
            Return sample

        End Function

        ''' <summary>Gets the sample associated with the specified timestamp.</summary>
        ''' <returns>The sample associated with the specified timestamp. If the specified timestamp is not found, property returns null.</returns>
        ''' <param name="ticks">The ticks of the baselined timestamp of the sample to get.</param>
        Protected Function LookupSample(ByVal ticks As Long) As Sample

            Dim foundSample As Sample

            ' Lookup sample with specified baselined ticks (this is internally SyncLock'd)
            If m_sampleQueue.TryGetValue(ticks, foundSample) Then
                Return foundSample
            Else
                Return Nothing
            End If

        End Function

#End Region

#Region " Private Methods Implementation "

        ' Each sample consists of an array of frames - the sample represents one second of data, so all frames
        ' are to get published during this second.  Typically the process queue's "process item function"
        ' does the work of the queue - but in this case we use the "can process item function" to process
        ' each frame in the sample until all frames have been published.
        Private Function CanPublishSample(ByVal ticks As Long, ByVal sample As Sample) As Boolean

            ' This function is executed on a real-time thread, so make sure any work to be done here
            ' is executed as efficiently as possible
            Dim frame As IFrame = sample.Frames(m_frameIndex)
            Dim allFramesPublished As Boolean

            ' Frame timestamps are evenly distributed across their parent sample, so all we need to do
            ' is just wait for the lagtime to pass and begin publishing...
            If DistanceFromRealTime(frame.Ticks) >= m_lagTime Then
                Try
                    ' Publish a copy of the current frame (this way consumer doesn't have to worry about frame synchronization)
                    PublishFrame(frame.Clone(), m_frameIndex)
                Catch ex As Exception
                    RaiseEvent ProcessException(ex)
                End Try

                frame.Published = True
                m_publishedFrames += 1
                m_publishedMeasurements += frame.PublishedMeasurements

                ' Increment frame index
                m_frameIndex += 1
                allFramesPublished = (m_frameIndex >= m_framesPerSecond)
            End If

            ' We will say sample is ready to be published (i.e., discarded) once all frames have been published
            Return allFramesPublished

        End Function

        ' When all the frames have been published, we expose sample to consumer as last step in sample publication
        ' cycle to allow for any last minute needed steps in measurement concentration (e.g., this could be used
        ' to step-down sample rate for data consumption by slower applications)
        Private Sub PublishSample(ByVal ticks As Long, ByVal sample As Sample)

            ' We published all the frames of this sample, reset frame index for next sample
            m_frameIndex = 0

            ' We send out a notification that a new sample has been published so that anyone can have a chance
            ' to perform any last steps with the data before we remove it from the sample queue
            RaiseEvent SamplePublished(sample)

        End Sub

        ' Creates a new sample associated with the specified baselined timestamp ticks, if it doesn't already exist
        Private Sub CreateSample(ByVal ticks As Long)

            With m_sampleQueue
                If Not .ContainsKey(ticks) Then .Add(ticks, New Sample(Me, ticks))
            End With

        End Sub

        ' This function declares the proper needed signature for putting a method into the thread pool (i.e., allows
        ' sort measurement to be called on an independent thread) - referenced by QueueMeasurementForSorting function
        Private Sub SortMeasurement(ByVal state As Object)

            SortMeasurement(DirectCast(state, IMeasurement))

        End Sub

        ' All we do here is expose the number of unpublished samples in the queue (note that one sample will always be "publishing")
        Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

            RaiseEvent UnpublishedSamples(m_sampleQueue.Count - 1)

        End Sub

        ' We expose any process exceptions to user
        Private Sub m_sampleQueue_ProcessException(ByVal ex As System.Exception) Handles m_sampleQueue.ProcessException

            RaiseEvent ProcessException(ex)

        End Sub

#End Region

    End Class

End Namespace
