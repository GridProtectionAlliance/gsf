'***********************************************************************
'  PDCstream.DataQueue.vb - PDC stream data sample queue
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
Imports DatAWarePDC.PDCstream.Common

Namespace PDCstream

    ' This class creates a queue of real-time data samples
    Public Class DataQueue

        Private m_configFile As ConfigFile
        Private m_dataSamples As SortedList
        Private m_baseTime As DateTime      ' This represents the most recent encountered timestamp baselined at the bottom of the second
        Private m_baseTimeSet As Boolean
        Private m_discardedPoints As Long
        Private m_sampleRate As Double

        Public Event NewDataSampleCreated(ByVal newDataSample As DataSample)
        Public Event DataError(ByVal message As String)

        Public Sub New(ByVal configFile As ConfigFile)

            m_configFile = configFile
            m_dataSamples = New SortedList
            m_baseTime = DateTime.Now.Subtract(New TimeSpan(1, 0, 0, 0))    ' Initialize base time to yesterday...
            m_sampleRate = 1000 / m_configFile.SampleRate

        End Sub

        Public ReadOnly Property BaseTime() As DateTime
            Get
                Return m_baseTime
            End Get
        End Property

        Public ReadOnly Property DiscardedPoints() As Long
            Get
                Return m_discardedPoints
            End Get
        End Property

        Default Public ReadOnly Property Sample(ByVal index As Integer) As DataSample
            Get
                SyncLock m_dataSamples.SyncRoot
                    Return DirectCast(m_dataSamples.GetByIndex(index), DataSample)
                End SyncLock
            End Get
        End Property

        Default Public ReadOnly Property Sample(ByVal baseTime As DateTime) As DataSample
            Get
                SyncLock m_dataSamples.SyncRoot
                    Return DirectCast(m_dataSamples(baseTime.Ticks), DataSample)
                End SyncLock
            End Get
        End Property

        Public Function GetSampleIndex(ByVal baseTime As DateTime) As Integer

            SyncLock m_dataSamples.SyncRoot
                Return m_dataSamples.IndexOfKey(baseTime.Ticks)
            End SyncLock

        End Function

        Public ReadOnly Property SampleCount() As Integer
            Get
                SyncLock m_dataSamples.SyncRoot
                    Return m_dataSamples.Count
                End SyncLock
            End Get
        End Property

        Public Sub RemovePublishedSamples()

            Dim publishedSamples As New ArrayList
            Dim x As Integer

            SyncLock m_dataSamples.SyncRoot
                For x = 0 To m_dataSamples.Count - 1
                    With DirectCast(m_dataSamples.GetByIndex(x), DataSample)
                        If .Published Then publishedSamples.Add(.Timestamp.Ticks)
                    End With
                Next

                For x = 0 To publishedSamples.Count - 1
                    m_dataSamples.Remove(publishedSamples(x))
                Next
            End SyncLock

        End Sub

        ' Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position
        Public Sub SortDataPoint(ByVal dataPoint As PMUDataPoint)

            With dataPoint
                Try
                    ' Find sample for this timestamp
                    Dim sample As DataSample = GetSample(.Timestamp)

                    If sample Is Nothing Then
                        ' No samples exist for this timestamp - data must be old
                        m_discardedPoints += 1
                    Else
                        ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                        ' proper sample index (i.e., the row) - we can then directly access the correct cell using the PMU index
                        Dim dataCell As PMUDataCell = sample.Rows(Math.Floor((.Timestamp.Millisecond + 1) / m_sampleRate)).Cells(.PMU.Index)

                        Select Case .Type
                            Case PointType.PhasorAngle
                                dataCell.PhasorValues(.Index).Angle = .Value
                            Case PointType.PhasorMagnitude
                                dataCell.PhasorValues(.Index).Magnitude = .Value
                            Case PointType.Frequency
                                dataCell.FrequencyValue.ScaledFrequency = .Value
                            Case PointType.DfDt
                                dataCell.FrequencyValue.ScaledDfDt = .Value
                            Case PointType.DigitalValue
                                If .Index = 0 Then
                                    dataCell.Digital0 = ParseInt16(.Value)
                                Else
                                    dataCell.Digital1 = ParseInt16(.Value)
                                End If
                            Case PointType.StatusFlags
                                dataCell.StatusFlags = ParseInt16(.Value)
                        End Select
                    End If
                Catch ex As Exception
                    ' We don't want to pass-up any data errors from here because they would bubble as errors in the DatAWare listener,
                    ' so we just raise event so any clients can log the exceptions or post them to remote clients
                    RaiseEvent DataError("Error sorting data point " & .PMU.ID & "-" & [Enum].GetName(GetType(PointType), .Type) & .Index & "@" & .Timestamp.ToString("dd-MMM-yyyy HH:mm:ss.fff") & ": " & ex.Message)
                End Try
            End With

        End Sub

        Private Function GetSample(ByVal timestamp As DateTime) As DataSample

            ' Baseline timestamp at bottom of the second
            Dim baseTime As DateTime = BaselinedTimestamp(timestamp)

            Dim sample As DataSample = DirectCast(m_dataSamples(baseTime.Ticks), DataSample)

            ' If sample for this timestamp doesn't exist, create one for it...
            If sample Is Nothing Then
                Dim difference As Double

                If m_baseTimeSet Then
                    ' Check difference between baseTime and last baseTime in seconds and fill any gaps
                    difference = DistanceFromBaseTime(baseTime)

                    If difference > 1 Then
                        For x As Integer = 1 To Math.Floor(difference) - 1
                            CreateDataSample(m_baseTime.AddSeconds(x))
                        Next
                    End If
                Else
                    m_baseTimeSet = True
                    difference = 1
                End If

                If difference > 0 Then
                    ' Set this time as the new base time
                    m_baseTime = baseTime
                    CreateDataSample(m_baseTime)
                End If

                ' Return new sample for this timestamp, if added
                Return DirectCast(m_dataSamples(baseTime.Ticks), DataSample)
            Else
                ' Return sample for this timestamp
                Return sample
            End If

        End Function

        Private Function CreateDataSample(ByVal baseTime As DateTime) As DataSample

            SyncLock m_dataSamples.SyncRoot
                If m_dataSamples(baseTime.Ticks) Is Nothing Then
                    Dim dataSample As New DataSample(m_configFile, baseTime)
                    m_dataSamples.Add(baseTime.Ticks, dataSample)
                    RaiseEvent NewDataSampleCreated(dataSample)
                End If
            End SyncLock

        End Function

        Public Function DistanceFromBaseTime(ByVal timeStamp As DateTime) As Double

            Return (timeStamp.Ticks - m_baseTime.Ticks) / 10000000L

        End Function

        ' This static function removes any milliseconds from a timestamp value to baseline the time at the bottom of the second
        Public Shared Function BaselinedTimestamp(ByVal timestamp As DateTime) As DateTime

            With timestamp
                If .Millisecond = 0 Then
                    Return timestamp
                Else
                    Return New DateTime(.Year, .Month, .Day, .Hour, .Minute, .Second, 0)
                End If
            End With

        End Function

    End Class

End Namespace