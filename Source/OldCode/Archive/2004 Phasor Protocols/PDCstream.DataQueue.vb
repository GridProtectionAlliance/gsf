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

Imports System.Runtime.CompilerServices

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

            Try
                With dataPoint
                    ' Find sample for this timestamp
                    Dim sample As DataSample = GetSample(.Timestamp)

                    If sample Is Nothing Then
                        ' No samples exist for this timestamp - data must be very old
                        m_discardedPoints += 1
                    Else
                        ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                        ' proper sample index (i.e., the row) - we can then directly access the correct cell using the PMU index
                        Dim rowIndex As Integer = Math.Floor((.Timestamp.Millisecond + 1) / m_sampleRate)

                        If rowIndex < 0 Or rowIndex >= sample.Rows.Length Then
                            ' TODO: remove debug code...
                            Debug.WriteLine("PDCstream.DataQueue.SortDataPoint: Invalid row index " & rowIndex & " calculated from Math.Floor (" & .Timestamp.Millisecond & " + 1) / " & m_sampleRate)
                        Else
                            Dim dataCell As PMUDataCell = sample.Rows(rowIndex).Cells(.PMU.Index)

                            Select Case .Type
                                Case PointType.PhasorAngle
                                    If .Index < 0 Or .Index >= dataCell.PhasorValues.Length Then
                                        ' TODO: remove debug code...
                                        Debug.WriteLine("PDCstream.DataQueue.SortDataPoint: Invalid phasor value index " & .Index & " encountered for " & .PMU.ID & "-PhasorAngle")
                                    Else
                                        dataCell.PhasorValues(.Index).Angle = .Value
                                    End If
                                Case PointType.PhasorMagnitude
                                    If .Index < 0 Or .Index >= dataCell.PhasorValues.Length Then
                                        ' TODO: remove debug code...
                                        Debug.WriteLine("PDCstream.DataQueue.SortDataPoint: Invalid phasor value index " & .Index & " encountered for " & .PMU.ID & "-PhasorMagnitude")
                                    Else
                                        dataCell.PhasorValues(.Index).Magnitude = .Value
                                    End If
                                Case PointType.Frequency
                                    dataCell.FrequencyValue.ScaledFrequency = .Value
                                Case PointType.DfDt
                                    dataCell.FrequencyValue.ScaledDfDt = .Value
                                Case PointType.DigitalValue
                                    Try
                                        If .Index = 0 Then
                                            dataCell.Digital0 = Convert.ToUInt16(.Value)
                                        Else
                                            dataCell.Digital1 = Convert.ToUInt16(.Value)
                                        End If
                                    Catch ex As Exception
                                        ' TODO: remove debug code...
                                        Debug.WriteLine("PDCstream.DataQueue.SortDataPoint: Failed to set digital value from " & .Value & ": " & ex.Message)
                                    End Try
                                Case PointType.StatusFlags
                                    Try
                                        dataCell.StatusFlags = Convert.ToUInt16(.Value)
                                    Catch ex As Exception
                                        ' TODO: remove debug code...
                                        Debug.WriteLine("PDCstream.DataQueue.SortDataPoint: Failed to set status flags from " & .Value & ": " & ex.Message)
                                    End Try
                            End Select
                        End If
                    End If
                End With
            Catch ex As Exception
                ' We don't want to pass-up any data errors from here because they would bubble as errors in the DatAWare listener,
                ' so we just log the exceptions and post them to any remote clients
                RaiseEvent DataError("Error sorting data point: " & ex.Message)
            End Try

        End Sub

        Private Function GetSample(ByVal timestamp As DateTime) As DataSample

            ' Baseline timestamp at bottom of the new second
            Dim baseTime As DateTime = BaselinedTimestamp(timestamp)
            Dim sample As DataSample = DirectCast(m_dataSamples(baseTime.Ticks), DataSample)

            ' If sample for this timestamp doesn't exist, create one for it...
            If sample Is Nothing Then
                ' Check difference between baseTime and last baseTime and fill any gaps
                Dim difference As Integer = DistanceFromBaseTime(baseTime)

                If difference > 0 Then
                    If difference > 1 And m_baseTimeSet Then
                        For x As Integer = 1 To difference - 1
                            CreateDataSample(m_baseTime.AddSeconds(x))
                        Next
                    End If

                    ' Set this time as the new base time
                    m_baseTime = baseTime
                    m_baseTimeSet = True
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

        Public Function DistanceFromBaseTime(ByVal timeStamp As DateTime) As Integer

            If m_baseTimeSet Then
                Return Math.Floor(timeStamp.Subtract(m_baseTime).Ticks / 10000000L)
            Else
                ' If the basetime has not been set, we should always return a large positive difference...
                Return 1000
            End If

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