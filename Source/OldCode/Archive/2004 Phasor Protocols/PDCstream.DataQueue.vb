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
        Private m_pointQueue As ArrayList
        Private m_baseTime As DateTime      ' This represents the most recent encountered timestamp baselined at the bottom of the second
        Private m_discardedPoints As Long
        Private WithEvents m_queueTimer As Timers.Timer

        Public Event NewDataSampleCreated(ByVal newDataSample As DataSample)

        Public Sub New(ByVal configFile As ConfigFile)

            m_configFile = configFile
            m_dataSamples = New SortedList
            m_pointQueue = New ArrayList
            m_baseTime = DateTime.Now.Subtract(New TimeSpan(1, 0, 0, 0))    ' Initialize base time to yesterday...
            m_queueTimer = New Timers.Timer

            ' We want to keep the process queue as busy as possible, so we'll process data
            ' at up to twice the sample rate - this is effective because multiple DatAWare
            ' servers can be posting data at any given moment during a second...
            With m_queueTimer
                .Interval = 1000 / m_configFile.SampleRate / 2
                .AutoReset = False
            End With

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
                    Return DirectCast(m_dataSamples(baseTime), DataSample)
                End SyncLock
            End Get
        End Property

        Public Function GetSampleIndex(ByVal baseTime As DateTime) As Integer

            SyncLock m_dataSamples.SyncRoot
                Return m_dataSamples.IndexOfKey(baseTime)
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
                        If .Published Then publishedSamples.Add(.Timestamp)
                    End With
                Next

                For x = 0 To publishedSamples.Count - 1
                    m_dataSamples.Remove(publishedSamples(x))
                Next
            End SyncLock

        End Sub

        Public ReadOnly Property QueuedPointCount() As Integer
            Get
                SyncLock m_pointQueue.SyncRoot
                    Return m_pointQueue.Count
                End SyncLock
            End Get
        End Property

        Public Sub QueueDataPoint(ByVal dataPoint As PMUDataPoint)

            SyncLock m_pointQueue.SyncRoot
                m_pointQueue.Add(dataPoint)
            End SyncLock

            m_queueTimer.Enabled = True

        End Sub

        Private Sub m_queueTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_queueTimer.Elapsed

            Dim dataPoints As PMUDataPoint()

            ' Get data points to process
            SyncLock m_pointQueue.SyncRoot
                If m_pointQueue.Count > 0 Then
                    dataPoints = m_pointQueue.ToArray(GetType(PMUDataPoint))
                    m_pointQueue.Clear()
                End If
            End SyncLock

            If Not dataPoints Is Nothing Then
                For x As Integer = 0 To dataPoints.Length - 1
                    ProcessDataPoint(dataPoints(x))
                Next
            End If

            m_queueTimer.Enabled = (QueuedPointCount > 0)

        End Sub

        ' Data comes in one-point at a time, so we use this function to place the point in its proper sample and row/cell position
        Private Sub ProcessDataPoint(ByVal dataPoint As PMUDataPoint)

            With dataPoint
                If DistanceFromBaseTime(.Timestamp) > 0 Then AddNewSample(.Timestamp)

                ' Find sample for this timestamp
                Dim sample As DataSample = m_dataSamples(BaselinedTimestamp(.Timestamp))

                If sample Is Nothing Then
                    ' No samples exists for this timestamp - data must be very old
                    m_discardedPoints += 1
                Else
                    ' We've found the right sample for this data, so lets access the proper data cell by first calculating the
                    ' proper sample index (i.e., the row) - we can then directly access the correct cell using the PMU index
                    Dim dataCell As PMUDataCell = sample.Rows(Math.Floor(.Timestamp.Millisecond / (1000 / m_configFile.SampleRate))).Cells(dataPoint.PMU.Index)

                    Select Case .Type
                        Case PointType.PhasorAngle
                            dataCell.PhasorValues(.Index).CompositeValues(PhasorValue.PolarCompositeValue.Angle) = .Value
                            CreatePhasorValue(dataCell, .Index)
                        Case PointType.PhasorMagnitude
                            dataCell.PhasorValues(.Index).CompositeValues(PhasorValue.PolarCompositeValue.Magnitude) = .Value
                            CreatePhasorValue(dataCell, .Index)
                        Case PointType.Frequency
                            dataCell.FrequencyValue.CompositeValues(FrequencyValue.CompositeValue.Frequency) = .Value
                            CreateFrequencyValue(dataCell)
                        Case PointType.DfDt
                            dataCell.FrequencyValue.CompositeValues(FrequencyValue.CompositeValue.DfDt) = .Value
                            CreateFrequencyValue(dataCell)
                        Case PointType.DigitalValue
                            If .Index = 0 Then
                                dataCell.Digital0 = Convert.ToUInt16(.Value)
                            Else
                                dataCell.Digital1 = Convert.ToUInt16(.Value)
                            End If
                        Case PointType.StatusFlags
                            dataCell.StatusFlags = Convert.ToUInt16(.Value)
                    End Select
                End If
            End With

        End Sub

        Private Sub CreatePhasorValue(ByVal dataCell As PMUDataCell, ByVal phasorIndex As Integer)

            Dim value As PhasorValue

            With dataCell.PhasorValues(phasorIndex)
                If .CompositeValues.AllReceived Then
                    ' All values received, create a new phasor value from composite values
                    value = PhasorValue.CreateFromPolarValues(dataCell.PMUDefinition.Phasors(phasorIndex), _
                        .CompositeValues(PhasorValue.PolarCompositeValue.Angle), _
                        .CompositeValues(PhasorValue.PolarCompositeValue.Magnitude))

                    ' We hang on to composite values since with received on change points
                    ' indivdual values may change over time
                    value.CompositeValues = .CompositeValues
                End If
            End With

            If Not value Is Nothing Then dataCell.PhasorValues(phasorIndex) = value

        End Sub

        Private Sub CreateFrequencyValue(ByVal dataCell As PMUDataCell)

            Dim value As FrequencyValue

            With dataCell.FrequencyValue
                If .CompositeValues.AllReceived Then
                    ' All values received, create a new frequency value from composite values
                    value = FrequencyValue.CreateFromScaledValues(dataCell.PMUDefinition.Frequency, _
                        .CompositeValues(FrequencyValue.CompositeValue.Frequency), _
                        .CompositeValues(FrequencyValue.CompositeValue.DfDt))

                    ' We hang on to composite values since with received on change points
                    ' indivdual values may change over time
                    value.CompositeValues = .CompositeValues
                End If
            End With

            If Not value Is Nothing Then dataCell.FrequencyValue = value

        End Sub

        ' When data from a new second is first encountered, we allocate a full data sample for that second
        <MethodImpl(MethodImplOptions.Synchronized)> _
        Private Sub AddNewSample(ByVal timeStamp As DateTime)

            ' Baseline timestamp at bottom of the new second, this becomes the new maximum second
            Dim baseTime As DateTime = BaselinedTimestamp(timeStamp)
            Dim difference As Integer = DistanceFromBaseTime(baseTime)

            ' Check difference between baseTime and last baseTime and fill any gaps
            If difference > 0 Then
                If difference > 1 And m_baseTime > DateTime.MinValue Then
                    For x As Integer = 1 To difference - 1
                        CreateDataSample(m_baseTime.AddSeconds(x))
                    Next
                End If

                m_baseTime = baseTime
                CreateDataSample(m_baseTime)
            End If

        End Sub

        Private Function CreateDataSample(ByVal baseTime As DateTime) As DataSample

            Dim dataSample As New DataSample(m_configFile, baseTime)

            SyncLock m_dataSamples.SyncRoot
                m_dataSamples.Add(baseTime, dataSample)
            End SyncLock

            RaiseEvent NewDataSampleCreated(dataSample)

        End Function

        Public Function DistanceFromBaseTime(ByVal timeStamp As DateTime) As Integer

            Return Math.Floor(timeStamp.Subtract(m_baseTime).Ticks / 10000000L)

        End Function

        ' This static function removes any milliseconds from a timestamp value to baseline the time at the bottom of the second
        Public Shared Function BaselinedTimestamp(ByVal timestamp As DateTime) As DateTime

            With timestamp
                If .Millisecond = 0 Then
                    Return timestamp
                Else
                    Return New DateTime(.Year, .Month, .Day, .Hour, .Minute, .Second)
                End If
            End With

        End Function

    End Class

End Namespace