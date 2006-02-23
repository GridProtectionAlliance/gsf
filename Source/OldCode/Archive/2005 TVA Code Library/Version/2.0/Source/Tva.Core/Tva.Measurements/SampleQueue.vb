'*******************************************************************************************************
'  Tva.Measurements.SampleQueue.vb - Collection of data samples
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a collection of data samples
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Threading
Imports Tva.DateTime.Common
Imports Tva.Collections

Namespace Measurements

    ' This class creates a queue of synchronized measurement samples
    Public Class SampleQueue

        Inherits KeyedProcessQueue(Of Long, ISample)

        ''' <summary>You must implement this function to create a new ISample</summary>
        Public Delegate Function CreateNewSampleSignature(ByVal sampleQueue As SampleQueue) As ISample

        Private m_baseTime As Date                      ' This represents the most recent encountered timestamp baselined at the bottom of the second
        Private m_interval As Double                    ' Sample time interval in seconds
        Private m_framesPerInterval As Integer          ' Frames per interval
        Private m_measurementRate As Decimal            ' We use a 64-bit floating point here to avoid round-off errors in calculations dealing with the sample rate
        Private m_timeDeviationTolerance As Double      ' Allowed time deviation tolerance
        Private m_discardedPoints As Long               ' Total number of discarded points
        Private m_createNewSampleFunction As CreateNewSampleSignature

        Public Sub New(ByVal createNewSampleFunction As CreateNewSampleSignature, ByVal processItemFunction As ProcessItemFunctionSignature, ByVal canProcessItemFunction As CanProcessItemFunctionSignature, ByVal interval As Double, ByVal framesPerInterval As Integer, ByVal timeDeviationTolerance As Double)

            MyBase.New(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, Timeout.Infinite, False, False)

            If timeDeviationTolerance < interval Then Throw New ArgumentOutOfRangeException("timeDeviationTolerance", "timeDeviationTolerance cannot be less than specified sample interval")
            If framesPerInterval < 1 Then Throw New ArgumentException("framesPerInterval must be greater than zero")

            m_createNewSampleFunction = createNewSampleFunction
            m_interval = interval
            m_framesPerInterval = framesPerInterval
            m_measurementRate = Convert.ToDecimal(SecondsToTicks(interval)) / Convert.ToDecimal(framesPerInterval)
            m_timeDeviationTolerance = timeDeviationTolerance
            m_baseTime = BaselinedTimestamp(Date.UtcNow)

        End Sub

        Public ReadOnly Property Interval() As Double
            Get
                Return m_interval
            End Get
        End Property

        Public ReadOnly Property FramesPerInterval() As Integer
            Get
                Return m_framesPerInterval
            End Get
        End Property

        Public ReadOnly Property BaseTime() As Date
            Get
                Dim currentTime As Date = BaselinedTimestamp(Date.UtcNow)

                ' If base time gets old, we fall back on local system time
                If System.Math.Abs(DistanceFromBaseTime(currentTime)) > m_timeDeviationTolerance Then
                    m_baseTime = currentTime
                End If

                Return m_baseTime
            End Get
        End Property

        Public Function DistanceFromBaseTime(ByVal timeStamp As Date) As Double

            Return TicksToSeconds(timeStamp.Ticks - m_baseTime.Ticks)

        End Function

        Public ReadOnly Property DiscardedPoints() As Long
            Get
                Return m_discardedPoints
            End Get
        End Property

        Default Public Overloads ReadOnly Property Item(ByVal baseTime As Date) As ISample
            Get
                Return Item(baseTime.Ticks)
            End Get
        End Property

        ''' <summary>Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position</summary>
        Public Sub SortMeasurement(ByVal measurement As IMeasurement)

            With measurement
                ' Find sample for this timestamp
                Dim sample As ISample = GetSample(.Timestamp)

                If sample Is Nothing Then
                    ' No samples exist for this timestamp - data must be old
                    m_discardedPoints += 1
                Else
                    ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                    ' proper frame index (i.e., the row) - we can then directly access the correct measurement using the index
                    sample.Frames(System.Math.Floor((.Ticks + 1@) / m_measurementRate)).Measurements(.Index).Value = .Value
                End If
            End With

        End Sub

        Public Sub ThreadSortMeasurement(ByVal measurement As IMeasurement)

            ThreadPool.QueueUserWorkItem(AddressOf SortMeasurement, measurement)

        End Sub

        Private Sub SortMeasurement(ByVal state As Object)

            SortMeasurement(DirectCast(state, IMeasurement))

        End Sub

        Private Function GetSample(ByVal timestamp As Date) As ISample

            ' Baseline timestamp at bottom of the second
            Dim baseTime As Date = BaselinedTimestamp(timestamp)
            Dim sample As ISample = Item(baseTime)

            ' Wait until the sample exists or we can enter critical section to create it ourselves
            Do Until sample IsNot Nothing
                ' We don't want to step on our own toes when creating new samples - so we create a critical section for
                ' this code - if another thread is busy creating samples, we'll just try again
                If Monitor.TryEnter(SyncRoot) Then
                    Try
                        ' Check difference between basetime and last basetime in seconds and fill any gaps
                        Dim difference As Double = DistanceFromBaseTime(baseTime)

                        If System.Math.Abs(difference) > m_timeDeviationTolerance Then
                            ' This data has come in late or has a future timestamp.  For old timestamps, we're not
                            ' going to create a sample for data that will never be processed.  For future dates we
                            ' must assume that the clock from source device must be advanced and out-of-sync with
                            ' real-time - either way this data will be discarded.
                            Exit Do
                        ElseIf difference > m_interval Then
                            ' Add intermediate samples as needed...
                            For x As Double = m_interval To System.Math.Floor(difference / m_interval) Step m_interval
                                CreateDataSample(m_baseTime.AddSeconds(x * m_interval))
                            Next
                        End If

                        ' Set this time as the new base time
                        m_baseTime = baseTime
                        CreateDataSample(m_baseTime)
                    Catch
                        ' Rethrow any exceptions - we are just catching any exceptions so we can
                        ' make sure to release thread lock in finally
                        Throw
                    Finally
                        Monitor.Exit(SyncRoot)
                    End Try
                Else
                    ' We sleep the thread between loops to help reduce CPU loading...
                    Thread.Sleep(1)
                End If

                sample = Item(baseTime)
            Loop

            ' Return sample for this timestamp
            Return sample

        End Function

        Private Sub CreateDataSample(ByVal baseTime As Date)

            If Not ContainsKey(baseTime.Ticks) Then Add(baseTime.Ticks, m_createNewSampleFunction(Me))

        End Sub

    End Class

End Namespace
