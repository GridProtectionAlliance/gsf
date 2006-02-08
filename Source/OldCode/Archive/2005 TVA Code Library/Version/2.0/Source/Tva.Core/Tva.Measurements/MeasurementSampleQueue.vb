'***********************************************************************
'  MeasurementSampleQueue.vb - Queue of sync'd measurement samples
'  Copyright © 2004 - TVA, all rights reserved
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
'***********************************************************************

Imports System.Threading
Imports TVA.Shared.DateTime

' This class creates a queue of synchronized measurement samples
Public Class MeasurementSampleQueue

    Private m_parent As MeasurementConcentrator
    Private m_dataSamples As SortedList
    Private m_baseTime As Date      ' This represents the most recent encountered timestamp baselined at the bottom of the second
    Private m_discardedPoints As Long
    Private m_criticalSection As Object

    Public Sub New(ByVal parent As MeasurementConcentrator)

        m_parent = parent
        m_dataSamples = New SortedList
        m_baseTime = BaselinedTimestamp(Date.UtcNow)
        m_criticalSection = New Object

    End Sub

    Public ReadOnly Property BaseTime() As Date
        Get
            Dim currentTime As Date = BaselinedTimestamp(Date.UtcNow)

            ' If base time gets old, we fall back on local system time
            If Math.Abs(DistanceFromBaseTime(currentTime)) > m_parent.TimeDeviationTolerance Then
                m_baseTime = currentTime
            End If

            Return m_baseTime
        End Get
    End Property

    Public ReadOnly Property DiscardedPoints() As Long
        Get
            Return m_discardedPoints
        End Get
    End Property

    Default Public ReadOnly Property Sample(ByVal index As Integer) As MeasurementSample
        Get
            SyncLock m_dataSamples.SyncRoot
                Return DirectCast(m_dataSamples.GetByIndex(index), MeasurementSample)
            End SyncLock
        End Get
    End Property

    Default Public ReadOnly Property Sample(ByVal baseTime As Date) As MeasurementSample
        Get
            SyncLock m_dataSamples.SyncRoot
                Return DirectCast(m_dataSamples(baseTime.Ticks), MeasurementSample)
            End SyncLock
        End Get
    End Property

    Public Function GetSampleIndex(ByVal baseTime As Date) As Integer

        SyncLock m_dataSamples.SyncRoot
            Return m_dataSamples.IndexOfKey(baseTime.Ticks)
        End SyncLock

    End Function

    Public ReadOnly Property Count() As Integer
        Get
            SyncLock m_dataSamples.SyncRoot
                Return m_dataSamples.Count
            End SyncLock
        End Get
    End Property

    Public Sub RemovePublishedSample()

        SyncLock m_dataSamples.SyncRoot
            m_dataSamples.RemoveAt(0)
        End SyncLock

    End Sub

    ' Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position
    Public Sub SortMeasurement(ByVal measurement As IMeasurement)

        With measurement
            ' Find sample for this timestamp
            Dim sample As MeasurementSample = GetSample(.Timestamp)

            If sample Is Nothing Then
                ' No samples exist for this timestamp - data must be old
                m_discardedPoints += 1
            Else
                ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                ' proper sample index (i.e., the row) - we can then directly access the correct cell using the PMU index
                sample.Rows(Math.Floor((.Timestamp.Millisecond + 1@) / m_parent.SampleRate)).Value(.Index) = .Value
            End If
        End With

    End Sub

    Public Sub ThreadSortMeasurement(ByVal measurement As IMeasurement)

        ThreadPool.QueueUserWorkItem(AddressOf SortMeasurement, measurement)

    End Sub

    Private Sub SortMeasurement(ByVal state As Object)

        SortMeasurement(DirectCast(state, IMeasurement))

    End Sub

    Private Function GetSample(ByVal timestamp As Date) As MeasurementSample

        ' Baseline timestamp at bottom of the second
        Dim baseTime As Date = BaselinedTimestamp(timestamp)
        Dim sample As MeasurementSample = Me(baseTime)

        ' Wait until the sample exists or we can enter critical section to create it ourselves
        Do Until Not sample Is Nothing
            ' We don't want to step on our own toes when creating new samples - so we create a critical section for
            ' this code - if another thread is busy creating samples, we'll just try again
            If Monitor.TryEnter(m_criticalSection) Then
                Try
                    ' Check difference between basetime and last basetime in seconds and fill any gaps
                    Dim difference As Double = DistanceFromBaseTime(baseTime)

                    If Math.Abs(difference) > m_parent.TimeDeviationTolerance Then
                        ' This data has come in late or has a future timestamp.  For old timestamps, we're not
                        ' going to create a sample for data that will never be processed.  For future dates we
                        ' must assume that the clock from source device must be advanced and out-of-sync with
                        ' real-time - either way this data will be discarded.
                        Exit Do
                    ElseIf difference > 1 Then
                        ' Add intermediate samples as needed...
                        For x As Integer = 1 To Math.Floor(difference) - 1
                            CreateDataSample(m_baseTime.AddSeconds(x))
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
                    Monitor.Exit(m_criticalSection)
                End Try
            Else
                ' We sleep the thread between loops to help reduce CPU loading...
                Thread.Sleep(1)
            End If

            sample = Me(baseTime)
        Loop

        ' Return sample for this timestamp
        Return sample

    End Function

    Private Function CreateDataSample(ByVal baseTime As Date) As MeasurementSample

        SyncLock m_dataSamples.SyncRoot
            If Not m_dataSamples.ContainsKey(baseTime.Ticks) Then
                m_dataSamples.Add(baseTime.Ticks, New MeasurementSample(m_parent, baseTime))
            End If
        End SyncLock

    End Function

    Public Function DistanceFromBaseTime(ByVal timeStamp As Date) As Double

        Return (timeStamp.Ticks - m_baseTime.Ticks) / 10000000L

    End Function

End Class
