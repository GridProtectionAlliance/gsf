'*******************************************************************************************************
'  HistorianAdapterBase.vb - Historian adpater base class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
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
Imports Tva.Measurements

Public MustInherit Class HistorianAdapterBase

    Inherits AdapterBase
    Implements IHistorianAdapter

    Public Event ArchivalException(ByVal source As String, ByVal ex As Exception) Implements IHistorianAdapter.ArchivalException

    Private m_measurementBuffer As List(Of IMeasurement)
    Private WithEvents m_connectionTimer As Timers.Timer
    Private m_dataProcessingThread As Thread
    Private m_processedMeasurements As Long

    Private Const ProcessedMeasurementInterval As Integer = 100000

    Public Sub New()

        m_measurementBuffer = New List(Of IMeasurement)

        m_connectionTimer = New Timers.Timer

        With m_connectionTimer
            .AutoReset = False
            .Interval = 2000
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
            UpdateStatus("Starting connection attempt to " & Name & "...")

            ' Attempt connection to historian (consumer to call historian API connect function)
            AttemptConnection()

            ' Start data processing thread
            m_dataProcessingThread = New Thread(AddressOf ProcessMeasurements)
            m_dataProcessingThread.Start()

            UpdateStatus("Connection to " & Name & " established.")
        Catch ex As Exception
            UpdateStatus(">> WARNING: Connection to " & Name & " failed: " & ex.Message)
            Connect()
        End Try

    End Sub

    Public Sub Disconnect() Implements IHistorianAdapter.Disconnect

        Try
            ' Stop data processing thread
            If m_dataProcessingThread IsNot Nothing Then m_dataProcessingThread.Abort()
            m_dataProcessingThread = Nothing

            ' Attempt disconnection from historian (consumer to call historian API disconnect function)
            AttemptDisconnection()
        Catch ex As Exception
            UpdateStatus("Exception occured during disconnect from " & Name & ": " & ex.Message)
        End Try

    End Sub

    Protected MustOverride Sub AttemptDisconnection()

    Public Overridable Sub QueueMeasurementForArchival(ByVal measurement As IMeasurement) Implements IHistorianAdapter.QueueMeasurementForArchival

        SyncLock m_measurementBuffer
            m_measurementBuffer.Add(measurement)
            IncrementProcessedMeasurements()
        End SyncLock

    End Sub

    Public Overridable Sub QueueMeasurementsForArchival(ByVal measurements As IList(Of IMeasurement)) Implements IHistorianAdapter.QueueMeasurementsForArchival

        SyncLock m_measurementBuffer
            If measurements IsNot Nothing Then
                For x As Integer = 0 To measurements.Count - 1
                    m_measurementBuffer.Add(measurements(x))
                    IncrementProcessedMeasurements()
                Next
            End If
        End SyncLock

    End Sub

    Public Overridable Sub QueueMeasurementsForArchival(ByVal measurements As IDictionary(Of MeasurementKey, IMeasurement)) Implements IHistorianAdapter.QueueMeasurementsForArchival

        SyncLock m_measurementBuffer
            If measurements IsNot Nothing Then
                For Each measurement As IMeasurement In measurements.Values
                    m_measurementBuffer.Add(measurement)
                    IncrementProcessedMeasurements()
                Next
            End If
        End SyncLock

    End Sub

    ' TODO: Optimize function to handle += measurements.Count yet still not miss sending the status update
    Private Sub IncrementProcessedMeasurements()

        m_processedMeasurements += 1
        If m_processedMeasurements Mod ProcessedMeasurementInterval = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been queued for archival so far...")

    End Sub

    'Private Sub IncrementProcessedMeasurements(ByVal totalAdded As Integer)

    '    m_processedMeasurements += totalAdded
    '    If m_processedMeasurements Mod ProcessedMeasurementInterval = 0 Then UpdateStatus(m_processedMeasurements.ToString("#,##0") & " measurements have been queued for archival so far...")

    'End Sub

    ''' <summary>
    ''' Queued measurements to be archived by historian
    ''' </summary>
    ''' <remarks>Base class consumers are expected to SyncLock this reference as needed before using data</remarks>
    Protected Overridable ReadOnly Property Measurements() As List(Of IMeasurement)
        Get
            Return m_measurementBuffer
        End Get
    End Property

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
                .Append("  Queued measurement count: ")
                SyncLock m_measurementBuffer
                    .Append(m_measurementBuffer.Count)
                End SyncLock
                .Append(Environment.NewLine)
                Return .ToString()
            End With
        End Get
    End Property

    Protected Sub RaiseArchivalException(ByVal ex As Exception)

        RaiseEvent ArchivalException(Name, ex)

    End Sub

    ''' <summary>
    ''' User implements to send queued meaurements to archive
    ''' </summary>
    ''' <remarks>
    ''' <para>As many measurements as possible should be sent from measurement queue to the historian, then removed, so that system doesn't fall behind</para>
    ''' <para>Note that measurement collection, accesible via "Measurements" property, should be SyncLock'ed as necessary during enumeration or updates</para>
    ''' </remarks>
    Protected MustOverride Sub ArchiveMeasurements()

    Private Sub ProcessMeasurements()

        ' TODO: This needs to be adjustable based on the total number of items that can be processed by the historian at once...
        Const statusInterval As Integer = 1000
        Const dumpInterval As Integer = 5000
        Const postDumpCount As Integer = 100

        Dim queuedMeasurements As Integer
        Dim pollEvents As Long

        ' Enter the data read loop
        Do While True
            Try
                pollEvents = 0

                Do
                    ' Get queue count before archival
                    SyncLock m_measurementBuffer
                        queuedMeasurements = Measurements.Count
                    End SyncLock

                    If queuedMeasurements > 0 Then
                        ' Send measurements to historian
                        ArchiveMeasurements()

                        ' Get queue count after archival
                        SyncLock m_measurementBuffer
                            queuedMeasurements = Measurements.Count
                        End SyncLock
                    End If

                    If queuedMeasurements > 0 Then
                        ' We shouldn't stay in this loop forever (this would mean we're falling behind) so we broadcast the status of things...
                        pollEvents += 1

                        If pollEvents Mod statusInterval = 0 Then
                            UpdateStatus(">> WARNING: " & queuedMeasurements.ToString("#,##0") & " measurements remain in the queue to be sent on " & Name & "...")
                            If queuedMeasurements > dumpInterval Then Exit Do
                        End If
                    Else
                        ' We sleep thread between polls to reduce CPU loading...
                        Thread.Sleep(1)
                    End If
                Loop While queuedMeasurements > 0

                ' We're getting behind, must dump measurements :(
                If queuedMeasurements > dumpInterval Then
                    ' TODO: It would certainly be advisable at this point to take pre-defined evasive action, e.g., you could
                    ' prioritize all data connections for this receiver and start dropping connections (with proper
                    ' notifications of control actions) to help alleviate data loss (that's all I do manually when this problem
                    ' happens)
                    SyncLock m_measurementBuffer
                        ' TODO: When this starts happening - you've overloaded the real-time capacity of your historian
                        ' and you must do something about it - bigger hardware, scale out, etc. Make sure to log this
                        ' error externally (alarm or something) so things can be fixed...
                        UpdateStatus(">> ERROR: Dumping " & (queuedMeasurements - postDumpCount).ToString("#,##0") & " measurements because we're falling behind :(")
                        m_measurementBuffer.RemoveRange(0, queuedMeasurements - postDumpCount)
                    End SyncLock
                ElseIf pollEvents > statusInterval Then
                    ' Send final status message - warning terminated...
                    UpdateStatus(">> INFO: Warning state terminated - all queued measurements have been sent")
                End If
            Catch ex As ThreadAbortException
                ' If we received an abort exception, we'll egress gracefully
                Exit Do
            Catch ex As ObjectDisposedException
                ' This will be a normal exception...
                Exit Do
            Catch ex As Exception
                RaiseArchivalException(ex)
                Connect()
                Exit Do
            End Try
        Loop

    End Sub

End Class
