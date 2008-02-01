'*******************************************************************************************************
'  HistorianAdapterBase.vb - Historian adpater base class
'  Copyright © 2008 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2008
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/01/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Threading
Imports System.Runtime.CompilerServices
Imports TVA.Measurements
Imports TVA.Threading

Public MustInherit Class HistorianAdapterBase

    Inherits AdapterBase
    Implements IHistorianAdapter

    ''' <summary>This event will be raised if there is an exception encountered while attempting to archive measurements</summary>
    ''' <remarks>Connection cycle to historian will be restarted when an exception is encountered</remarks>
    Public Event ArchivalException(ByVal source As String, ByVal ex As Exception) Implements IHistorianAdapter.ArchivalException

    ''' <summary>This event gets raised every second allowing consumer to track total number of unarchived measurements</summary>
    ''' <remarks>If queue size reaches an unhealthy threshold, evasive action should be considered</remarks>
    Public Event UnarchivedMeasurements(ByVal total As Integer) Implements IHistorianAdapter.UnarchivedMeasurements

    Private m_measurementQueue As List(Of IMeasurement)
#If ThreadTracking Then
    Private m_dataProcessingThread As ManagedThread
#Else
    Private m_dataProcessingThread As Thread
#End If
    Private m_processedMeasurements As Long
    Private WithEvents m_connectionTimer As Timers.Timer
    Private WithEvents m_monitorTimer As Timers.Timer

    Private Const ProcessedMeasurementInterval As Integer = 100000

    Public Sub New()

        m_measurementQueue = New List(Of IMeasurement)

        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 2000
            .Enabled = False
        End With

        m_monitorTimer = New Timers.Timer

        ' We monitor total number of unarchived measurements every second - this is a useful statistic to monitor, if
        ' total number of unarchived measurements gets very large, measurement archival could be falling behind
        With m_monitorTimer
            .Interval = 1000
            .AutoReset = True
            .Enabled = False
        End With

    End Sub

    Public MustOverride Sub Initialize(ByVal connectionString As String) Implements IHistorianAdapter.Initialize

    Public Sub Connect() Implements IHistorianAdapter.Connect

        ' Make sure we are disconnected before attempting a connection
        Disconnect()

        ' Start the connection cycle
        m_connectionTimer.Enabled = True

    End Sub

    Protected MustOverride Sub AttemptConnection()

    Private Sub m_connectionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_connectionTimer.Elapsed

        Try
            UpdateStatus(String.Format("Starting connection attempt to {0}...", Name))

            ' Attempt connection to historian (consumer to call historian API connect function)
            ' This must happen before aborting data processing thread to keep disconnect from failing
            ' due to thread abort in case of reconnect caused by a processing exception
            AttemptConnection()

            ' Start data processing thread
#If ThreadTracking Then
            m_dataProcessingThread = New ManagedThread(AddressOf ProcessMeasurements)
            m_dataProcessingThread.Name = "InterfaceAdapters.HistorianAdapterBase.ProcessMeasurements() [" & Name & "]"
#Else
            m_dataProcessingThread = New Thread(AddressOf ProcessMeasurements)
#End If

            m_dataProcessingThread.Start()

            UpdateStatus(String.Format("Connection to {0} established.", Name))
        Catch ex As Exception
            UpdateStatus(String.Format("WARNING: Connection to {0} failed: {1}", Name, ex.Message))
            Connect()
        End Try

    End Sub

    Public Sub Disconnect() Implements IHistorianAdapter.Disconnect

        Try
            Dim performedDisconnect As Boolean

            ' Attempt disconnection from historian (consumer to call historian API disconnect function)
            AttemptDisconnection()

            ' Stop data processing thread
            If m_dataProcessingThread IsNot Nothing Then
                m_dataProcessingThread.Abort()
                performedDisconnect = True
            End If

            m_dataProcessingThread = Nothing

            If performedDisconnect Then UpdateStatus(String.Format("Disconnected from {0}", Name))
        Catch ex As Exception
            UpdateStatus(String.Format("Exception occured during disconnect from {0}: {1}", Name, ex.Message))
        End Try

    End Sub

    Public Overrides Sub Dispose()

        Disconnect()

    End Sub

    Protected MustOverride Sub AttemptDisconnection()

    Public Overridable Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement) Implements IHistorianAdapter.QueueMeasurementForArchival

        SyncLock m_measurementQueue
            m_measurementQueue.Add(measurement)
        End SyncLock

        IncrementProcessedMeasurements(1)

        '        ' We throw status message updates on the thread pool so we don't slow sorting operations
        '#If ThreadTracking Then
        '        With ManagedThreadPool.QueueUserWorkItem(AddressOf IncrementProcessedMeasurements, 1)
        '            .Name = "InterfaceAdapters.HistorianAdapterBase.IncrementProcessedMeasurements() [" & Name & "]"
        '        End With
        '#Else
        '        ThreadPool.UnsafeQueueUserWorkItem(AddressOf IncrementProcessedMeasurements, 1)
        '#End If

    End Sub

    Public Overridable Sub QueueMeasurementsForArchival(ByVal measurements As ICollection(Of IMeasurement)) Implements IHistorianAdapter.QueueMeasurementsForArchival

        SyncLock m_measurementQueue
            m_measurementQueue.AddRange(measurements)
        End SyncLock

        IncrementProcessedMeasurements(measurements.Count)

        '        ' We throw status message updates on the thread pool so we don't slow sorting operations
        '#If ThreadTracking Then
        '        With ManagedThreadPool.QueueUserWorkItem(AddressOf IncrementProcessedMeasurements, measurements.Count)
        '            .Name = "InterfaceAdapters.HistorianAdapterBase.IncrementProcessedMeasurements() [" & Name & "]"
        '        End With
        '#Else
        '        ThreadPool.UnsafeQueueUserWorkItem(AddressOf IncrementProcessedMeasurements, measurements.Count)
        '#End If

    End Sub

    'Private Sub IncrementProcessedMeasurements()

    '    Interlocked.Increment(m_processedMeasurements)
    '    If m_processedMeasurements Mod ProcessedMeasurementInterval = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been queued for archival so far...")

    'End Sub

    <MethodImpl(MethodImplOptions.Synchronized)> _
    Private Sub IncrementProcessedMeasurements(ByVal totalAdded As Long)

        ' Check to see if total number of added points will exceed process interval used to show periodic
        ' messages of how many points have been archived so far...
        Dim showMessage As Boolean = (m_processedMeasurements + totalAdded >= (m_processedMeasurements \ ProcessedMeasurementInterval + 1) * ProcessedMeasurementInterval)

        m_processedMeasurements += totalAdded

        If showMessage Then UpdateStatus(String.Format("{0:N0} measurements have been queued for archival so far...", m_processedMeasurements))

    End Sub

    ''' <summary>This function returns a range of measurements from the internal measurement queue, then removes the values</summary>
    ''' <remarks>
    ''' This method is typically only used to curtail size of measurement queue if it's getting too large.  If more points are
    ''' requested than there are points available - all points in the queue will be returned.
    ''' </remarks>
    Public Function GetMeasurements(ByVal total As Integer) As IMeasurement() Implements IHistorianAdapter.GetMeasurements

        Dim measurements As IMeasurement()

        SyncLock m_measurementQueue
            If total > m_measurementQueue.Count Then total = m_measurementQueue.Count
            measurements = m_measurementQueue.GetRange(0, total).ToArray()
            m_measurementQueue.RemoveRange(0, total)
        End SyncLock

        Return measurements

    End Function

    Protected Property ConnectionAttemptInterval() As Integer
        Get
            Return m_connectionTimer.Interval
        End Get
        Set(ByVal value As Integer)
            m_connectionTimer.Interval = value
        End Set
    End Property

    Public Overrides ReadOnly Property Status() As String
        Get
            With New StringBuilder
                .Append("      Historian connection: ")
                If m_dataProcessingThread Is Nothing Then
                    .Append("Inactive - not connected")
                Else
                    .Append("Active")
                End If
                .AppendLine()
                .Append("  Queued measurement count: ")
                SyncLock m_measurementQueue
                    .Append(m_measurementQueue.Count)
                End SyncLock
                .AppendLine()
                .Append("     Archived measurements: ")
                .Append(m_processedMeasurements)
                .AppendLine()
                Return .ToString()
            End With
        End Get
    End Property

    Protected Sub RaiseArchivalException(ByVal ex As Exception)

        RaiseEvent ArchivalException(Name, ex)

    End Sub

    ''' <summary>User implemented function used to send queued measurements to archive</summary>
    Protected MustOverride Sub ArchiveMeasurements(ByVal measurements As IMeasurement())

    Private Sub ProcessMeasurements()

        Dim measurements As IMeasurement()

        Do While True
            Try
                ' Grab a copy of all queued measurements and send to historian adapter when
                ' we can get a lock - this way the "queuing" of data will get lock priority
                ' and we'll keep lock time to a minimum
                measurements = Nothing

                If Monitor.TryEnter(m_measurementQueue) Then
                    Try
                        If m_measurementQueue.Count > 0 Then
                            measurements = m_measurementQueue.ToArray()
                            m_measurementQueue.Clear()
                        End If
                    Finally
                        Monitor.Exit(m_measurementQueue)
                    End Try
                End If

                If measurements IsNot Nothing Then ArchiveMeasurements(measurements)

                ' We sleep thread between polls to reduce CPU loading...
                Thread.Sleep(1)
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As ObjectDisposedException
                ' This will be a normal exception...
                Exit Do
            Catch ex As Exception
                ' If an exception is thrown by the archiver, we'll report the event and restart the connection cycle
                RaiseArchivalException(ex)
                Connect()
                Exit Do
            End Try
        Loop

    End Sub

    ' All we do here is expose the total number of unarchived measurements in the queue
    Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

        RaiseEvent UnarchivedMeasurements(m_measurementQueue.Count)

    End Sub

End Class
