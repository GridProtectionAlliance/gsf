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
        Private m_setRealTimeTicksLock As Object                                ' Object used to synchronize updating of real-time ticks
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
        Private m_totalSortTime As Long                                         ' Total frame sorting time (in ticks)
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
            m_setRealTimeTicksLock = New Object
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
                        m_totalSortTime = 0
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
        ''' local time measurement we have (this method ignores transmission delays) - but even these times can be incorrect so
        ''' we still have to apply reasonability checks to these times.  To do this we use the local time and the defined lag
        ''' and lead times to validate the latest measured timestamp.  If the newest received measurement timestamp gets too
        ''' old (as defined by lag time) or creeps too far into the future (as defined by lead time), we will fall back on local
        ''' system time.  Note that this creates a dependency on a fairly accurate local clock - the smaller the time deviation
        ''' tolerances the better the needed local clock acuracy.  For example, time deviation tolerances of a few seconds might
        ''' only require keeping the local clock synchronized to an NTP time source but sub-second tolerances would require that
        ''' the local clock be very close to GPS time.
        ''' </remarks>
        Public ReadOnly Property RealTimeTicks() As Long
            Get
                ' If the current value for real-time is outside of the time deviation tolerance of the local clock
                ' then we set latest measurement time (i.e., real-time) to be the current local clock time.  Note
                ' that we still need to validate this value regardless of whether or not we are using the local clock
                ' as real-time since the internal m_realTimeTicks value is used for creating measurement samples.
                ' Also, because of the frequency with which this function gets called, we don't call the TimeIsValid
                ' function to determine if the real-time ticks are valid, but instead manually implement the function
                ' here to avoid the overhead of a function call and perform a boolean OrElse operator optimization
                Dim currentTimeTicks As Long = Date.UtcNow.Ticks
                Dim currentRealTimeTicks As Long = m_realTimeTicks
                Dim distance As Double = (currentTimeTicks - currentRealTimeTicks) / TicksPerSecond

                If distance > m_lagTime OrElse distance < -m_leadTime Then
                    SyncLock m_setRealTimeTicksLock
                        ' Since real-time ticks value may have been updated by other threads since we aquired the lock
                        ' we'll check to see if we still need to set real-time ticks to the current time
                        Dim updateRealTimeTicks As Boolean = (currentRealTimeTicks = m_realTimeTicks)

                        If Not updateRealTimeTicks Then
                            distance = (currentTimeTicks - m_realTimeTicks) / TicksPerSecond
                            updateRealTimeTicks = (distance > m_lagTime OrElse distance < -m_leadTime)
                        End If

                        If updateRealTimeTicks Then
                            m_realTimeTicks = currentTimeTicks
                            m_currentSampleTimestamp = BaselinedTimestamp(currentTimeTicks, BaselineTimeInterval.Second)
                        End If
                    End SyncLock
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

            Return (RealTimeTicks - ticks) / TicksPerSecond

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

        ''' <summary>Total seconds of required sorting time since concentrator started</summary>
        Public ReadOnly Property TotalSortingTime() As Double
            Get
                Return m_totalSortTime / Stopwatch.Frequency
            End Get
        End Property

        ''' <summary>Average required sorting time per frame in seconds</summary>
        ''' <remarks>Slow reporting device measurements will have a negative impact on this statistic</remarks>
        Public ReadOnly Property AverageSortingTimePerFrame() As Double
            Get
                Return TotalSortingTime / m_publishedFrames
            End Get
        End Property

        ''' <summary>Data comes in as measurements, so we use this function to place the point in its proper sample and row/cell position</summary>
        Public Overridable Sub SortMeasurement(ByVal measurement As IMeasurement)

            SortMeasurements(New IMeasurement() {measurement})

        End Sub

        ''' <summary>Data comes in as measurements, so we use this function to place the points in its proper sample and row/cell position</summary>
        Public Overridable Sub SortMeasurements(ByVal measurements As ICollection(Of IMeasurement))

            ' This function is called continually with new measurements and handles the "time-alignment" (i.e., sorting) of these new values, many
            ' threads will be waiting for frames of time aligned data so make sure any work to be done here is executed as efficiently as possible
            Dim sample As Sample
            Dim frame As IFrame
            Dim ticks As Long
            Dim baseTimeTicks As Long
            Dim currentTimeTicks As Long
            Dim distance As Double

            ' Measurements usually come in groups - so we process all available measurements in the collection here directly as an
            ' optimization which avoids the overhead of a function call for each measurement
            For Each measurement As IMeasurement In measurements
                ' If measurement timestamp is not accurate, we'll set its timestamp to real-time and sort the measurement by arrival
                If Not measurement.TimestampQualityIsGood Then
                    measurement.Ticks = RealTimeTicks
                    m_measurementsSortedByArrival += 1
                End If

                '
                ' *** Manage sample queue ***
                '

                ' Get ticks for this measurement
                ticks = measurement.Ticks

                ' Baseline measurement timestamp at bottom of the second to use as sample "key"
                baseTimeTicks = BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks

                ' Find sample for this measurement
                sample = LookupSample(baseTimeTicks)

                ' This next critical sorting operation automatically manages the sample queue based on timestamps of incoming measurements
                ' Enter loop to wait until the sample exists, we will attempt to enter critical section and create it ourselves
                Do Until sample IsNot Nothing
                    ' We don't want to step on our own toes when creating new samples - so we create a critical section for
                    ' this code - if another thread is busy creating samples, we'll just wait for it below
                    If Monitor.TryEnter(m_sampleQueue.SyncRoot) Then
                        Try
                            ' Check difference between timestamp and current sample base-time in seconds and fill in any gaps.
                            ' Note that current sample base-time will be validated against local clock in call to RealTimeTicks
                            distance = DistanceFromRealTime(ticks)

                            If distance > m_lagTime OrElse distance < -m_leadTime Then
                                ' This data has come in late or has a future timestamp.  For old timestamps, we're not
                                ' going to create a sample for data that will never be processed.  For future dates we
                                ' must assume that the clock from source device must be advanced and out-of-sync with
                                ' real-time - either way this data will be discarded.
                                m_discardedMeasurements += 1
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

                '
                ' *** Sort measurement into proper frame ***
                '

                If sample IsNot Nothing Then
                    ' We've found the right sample for this data, so we access the proper data cell by first calculating the
                    ' proper frame index (i.e., the row) - we can then directly access the correct measurement using its key
                    frame = sample.Frames(Convert.ToInt32((ticks - baseTimeTicks) / m_ticksPerFrame))

                    ' Start the sorting timer for this frame if it hasn't been started
                    If frame.SortTime Is Nothing Then frame.SortTime = Stopwatch.StartNew()

                    ' Call user customizable function to assign new measurement to its frame
                    AssignMeasurementToFrame(frame, measurement)

                    '
                    ' *** Manage "real-time" ticks ***
                    '

                    ' Track absolute lastest measurement timestamp...
                    currentTimeTicks = Date.UtcNow.Ticks

                    ' If the measurement time is newer than the current real-time value and is within the specified time
                    ' deviation tolerance of the local clock time then we set the measurement time as real-time
                    If ticks > m_realTimeTicks Then
                        SyncLock m_setRealTimeTicksLock
                            ' Since real-time ticks value may have been updated by other threads since we aquired the lock
                            ' we'll verify again that our ticks are still the newest value we have
                            If ticks > m_realTimeTicks Then
                                ' Because of the frequency with which this function gets called, we don't call the TimeIsValid
                                ' function to determine if the real-time ticks are valid, but instead manually implement this
                                ' to avoid the overhead of a function call and reduce total required synclock time
                                distance = (currentTimeTicks - ticks) / TicksPerSecond

                                If distance >= -m_leadTime AndAlso distance <= m_lagTime Then
                                    ' New time measurement looks good, assume this time as "real-time"
                                    m_realTimeTicks = ticks
                                    m_currentSampleTimestamp = BaselinedTimestamp(m_realTimeTicks, BaselineTimeInterval.Second)
                                Else
                                    ' Current ticks were outside of time deviation tolerances so we'll also check to make
                                    ' sure current real-time ticks are within range as well
                                    distance = (currentTimeTicks - m_realTimeTicks) / TicksPerSecond

                                    If distance > m_lagTime OrElse distance < -m_leadTime Then
                                        ' New time measurement was invalid as was current real-time value so we
                                        ' have no choice but to assume the current time as "real-time"
                                        m_realTimeTicks = currentTimeTicks
                                        m_currentSampleTimestamp = BaselinedTimestamp(currentTimeTicks, BaselineTimeInterval.Second)
                                    End If
                                End If
                            End If
                        End SyncLock
                    End If

                    ' Track absolute latest measurement values
                    If m_trackLatestMeasurements Then m_latestMeasurements.UpdateMeasurementValue(measurement)
                End If
            Next

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
                                .Append("       Current frame = ")
                                .Append(frameIndex + 1)
                                .Append(" - sort time: ")

                                ' Calculate maximum sort time for publishing frame
                                .Append((currentFrame.SortTime.ElapsedTicks / Stopwatch.Frequency).ToString("0.0000"))
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
                    .Append("Average sorting time/frame: ")
                    .Append(AverageSortingTimePerFrame.ToString("0.0000"))
                    .Append(" seconds")
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

        ''' <summary>Consumers can choose to override this method to handle custom assignment of a measurement to its frame</summary>
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
                ' Available sorting time has passed - we're publishing the frame
                frame.SortTime.Stop()

                Try
                    ' Publish a synchronized copy of the current frame (this way consumer doesn't have to worry about frame synchronization)
                    PublishFrame(frame.Clone(), m_frameIndex)
                Catch ex As Exception
                    RaiseEvent ProcessException(ex)
                End Try

                ' Update publication statistics
                frame.Published = True
                m_publishedFrames += 1
                m_totalSortTime += frame.SortTime.ElapsedTicks
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

#Region " Old Code "

    ' Note: Moved this code directly into the sorting function since this was the only place it was being called to optimize by preventing function call overhead

    '''' <summary>This critical function automatically manages the sample queue based on timestamps of incoming measurements</summary>
    '''' <returns>The sample associated with the specified timestamp. If the sample is not found at timestamp, it will be created.</returns>
    '''' <param name="ticks">Ticks of the timestamp of the sample to get</param>
    '''' <remarks>Function will return null if timestamp is outside of the specified time deviation tolerance</remarks>
    'Protected Function GetSample(ByVal ticks As Long) As Sample

    '    ' Baseline measurement timestamp at bottom of the second
    '    Dim baseTimeTicks As Long = BaselinedTimestamp(ticks, BaselineTimeInterval.Second).Ticks
    '    Dim sample As Sample = LookupSample(baseTimeTicks)

    '    ' Enter loop to wait until the sample exists, we will attempt to enter critical section and create it ourselves
    '    Do Until sample IsNot Nothing
    '        ' We don't want to step on our own toes when creating new samples - so we create a critical section for
    '        ' this code - if another thread is busy creating samples, we'll just wait for it below
    '        If Monitor.TryEnter(m_sampleQueue.SyncRoot) Then
    '            Try
    '                ' Check difference between timestamp and current sample base-time in seconds and fill in any gaps.
    '                ' Note that current sample base-time will be validated against local clock
    '                Dim distance As Double = DistanceFromRealTime(ticks)

    '                If distance > m_lagTime OrElse distance < -m_leadTime Then
    '                    ' This data has come in late or has a future timestamp.  For old timestamps, we're not
    '                    ' going to create a sample for data that will never be processed.  For future dates we
    '                    ' must assume that the clock from source device must be advanced and out-of-sync with
    '                    ' real-time - either way this data will be discarded.
    '                    Exit Do
    '                ElseIf distance > 1 Then
    '                    ' Add intermediate samples as needed...
    '                    For x As Integer = 1 To System.Math.Floor(distance)
    '                        CreateSample(m_currentSampleTimestamp.AddSeconds(x).Ticks)
    '                    Next
    '                End If

    '                ' Create sample for new base time
    '                CreateSample(baseTimeTicks)
    '            Catch
    '                ' Rethrow any exceptions - we are just catching any exceptions so we can
    '                ' make sure to release thread lock in finally
    '                Throw
    '            Finally
    '                Monitor.Exit(m_sampleQueue.SyncRoot)
    '            End Try
    '        Else
    '            ' We sleep the thread between loops to help reduce CPU loading...
    '            Thread.Sleep(1)
    '        End If

    '        ' If we just created the sample we needed, then we'll get it here.  Otherwise the sample may have been
    '        ' created by another thread while we were sleeping, so we'll check again to see to see if sample exists.
    '        ' Additionally, the TryGetValue function (referenced from within LookupSample) internally performs a
    '        ' SyncLock on the SyncRoot and waits for it to be released, so if another thread was creating new
    '        ' samples then we'll definitely pick up our needed sample when the lock is released.  Nice and safe.
    '        sample = LookupSample(baseTimeTicks)
    '    Loop

    '    ' Return sample for this timestamp
    '    Return sample

    'End Function

#End Region

End Namespace
