'***********************************************************************
'  Concentrator.vb - PDCstream Concentrator
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
'  Note:  Note that your lag time is closely related to the rate
'  at which data data is coming into the concentrator.  DatAWare
'  allows some flexibility in its polling interval, so if you set
'  DatAWare to poll and broadcast at a rate of .10 seconds, you
'  might set the lag time to .50 to make sure you've had time to
'  receive all the data from the reporting servers - this works
'  since the data coming in is GPS based and should be very close
'  in time...
'
'***********************************************************************

Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports TVA.Shared.DateTime
Imports TVA.Shared.FilePath
Imports TVA.Services
Imports TVA.EE.Phasor.PDCstream

Public Class Concentrator

    Implements IServiceComponent
    Implements IDisposable

    Private m_parent As DatAWarePDC
    Private WithEvents m_configFile As ConfigFile
    Private m_dataQueue As DataQueue
    Private m_descriptor As DescriptorPacket
    Private m_udpClient As UdpClient
    Private m_broadcastIPs As IPEndPoint()
    Private m_lagTime As Double
    Private m_sampleRate As Decimal
    Private m_publishTime As DateTime
    Private m_sentDescriptor As Boolean
    Private m_rowIndex As Integer
    Private WithEvents m_processTimer As Timers.Timer
    Private WithEvents m_monitorTimer As Timers.Timer
    Private m_enabled As Boolean
    Private m_processing As Boolean
    Private m_startTime As Long
    Private m_stopTime As Long
    Private m_bytesBroadcasted As Long
    Private m_packetsPublished As Long

    Public Event SamplePublished(ByVal sample As DataSample)
    Public Event UnpublishedSamples(ByVal total As Integer)

#Region " Common Primary Service Process Code "

    Public Sub New(ByVal parent As DatAWarePDC, ByVal configFileName As String, ByVal lagTime As Double, ByVal broadcastIPs As IPEndPoint())

        m_parent = parent
        m_configFile = New ConfigFile(configFileName)
        m_dataQueue = New DataQueue(m_configFile)
        m_descriptor = New DescriptorPacket(m_configFile)
        m_udpClient = New UdpClient
        m_broadcastIPs = broadcastIPs
        m_lagTime = lagTime
        m_processTimer = New Timers.Timer
        m_monitorTimer = New Timers.Timer
        m_enabled = True
        m_processing = False
        m_sampleRate = Math.Floor(1000@ / m_configFile.SampleRate) - 1

        ' We define a process timer to monitor for samples to send...
        With m_processTimer
            .Interval = 1
            .AutoReset = False
            .Enabled = False
        End With

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

    Public Overridable Sub Dispose() Implements IServiceComponent.Dispose, IDisposable.Dispose

        GC.SuppressFinalize(Me)

        ' Any needed shutdown code for your primary service process should be added here - note that this class
        ' instance is available for the duration of the service lifetime...

    End Sub

    ' This function handles updating status for the primary service process
    Public Sub UpdateStatus(ByVal Status As String, Optional ByVal LogStatusToEventLog As Boolean = False, Optional ByVal EntryType As System.Diagnostics.EventLogEntryType = EventLogEntryType.Information)

        m_parent.UpdateStatus(Status, LogStatusToEventLog, EntryType)

    End Sub

    Public Sub UpdateProgress(ByVal current As Long, ByVal total As Long)

        m_parent.ServiceHelper.SendServiceProgress(current, total)

    End Sub

    Public Property Enabled() As Boolean
        Get
            Return m_enabled
        End Get
        Set(ByVal Value As Boolean)
            m_enabled = Value
        End Set
    End Property

    Public Property Processing() As Boolean
        Get
            Return m_processing
        End Get
        Set(ByVal Value As Boolean)
            m_processing = Value

            If m_processing Then
                ' Reset stats...
                m_stopTime = 0
                m_bytesBroadcasted = 0
                m_packetsPublished = 0
                m_startTime = DateTime.Now.Ticks
                m_publishTime = DateTime.Now.AddSeconds(m_lagTime)
                m_rowIndex = -1

                ' Start process timer
                m_processTimer.Enabled = True
            Else
                ' Stop process timer
                m_processTimer.Enabled = False
                m_stopTime = DateTime.Now.Ticks
            End If
        End Set
    End Property

    Public ReadOnly Property RunTime() As Double
        Get
            Dim ProcessingTime As Long

            If m_startTime > 0 Then
                If m_stopTime > 0 Then
                    ProcessingTime = m_stopTime - m_startTime
                Else
                    ProcessingTime = DateTime.Now.Ticks - m_startTime
                End If
            End If

            If ProcessingTime < 0 Then ProcessingTime = 0

            Return ProcessingTime / 10000000L
        End Get
    End Property

#End Region

    Public ReadOnly Property ConfigFile() As ConfigFile
        Get
            Return m_configFile
        End Get
    End Property

    Public ReadOnly Property DataQueue() As DataQueue
        Get
            Return m_dataQueue
        End Get
    End Property

    Private Sub m_processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_processTimer.Elapsed

        Try
            ' Since a typical process rate is 30 samples per second do everything you can to make sure code
            ' here is optimized to execute as quickly as possible...
            Dim currentTime As DateTime = DateTime.Now
            Dim binaryLength As Integer
            Dim x, y, z As Integer
            Dim exitLoop As Boolean

            ' Send a descriptor packet at the top of each minute...
            With currentTime
                If .Second = 0 Then
                    If Not m_sentDescriptor Then
                        m_sentDescriptor = True
                        Try
                            With m_descriptor
                                binaryLength = .BinaryLength

                                ' Send binary descriptor image over UDP broadcast channels
                                For x = 0 To m_broadcastIPs.Length - 1
                                    m_udpClient.Send(.BinaryImage, binaryLength, m_broadcastIPs(x))
                                Next
                            End With

                            m_bytesBroadcasted += binaryLength
                        Catch ex As Exception
                            UpdateStatus("Error publishing descriptor packet: " & ex.Message)
                        End Try
                    End If
                Else
                    m_sentDescriptor = False
                End If
            End With

            ' Check to see if it's time to broadcast...
            If currentTime.Ticks > m_publishTime.Ticks Or m_dataQueue.SampleCount > m_lagTime + 1 Then
                ' Access proper data packet out of current sample
                Dim currentSample As DataSample = m_dataQueue(0)

                If m_rowIndex = -1 Then
                    ' Make sure we've got enough lag-time between this sample and the most recent sample...
                    If m_dataQueue.DistanceFromBaseTime(currentSample.Timestamp) <= -m_lagTime OrElse m_dataQueue.SampleCount > m_lagTime + 1 Then
                        m_rowIndex = 0
                    End If
                End If

                If m_rowIndex > -1 Then
                    With currentSample.Rows(m_rowIndex)
                        Try
                            ' Even an attempt at publishing counts - we don't have time to go back and try again...
                            .Published = True

                            binaryLength = .BinaryLength

                            ' Send binary data packet image over UDP broadcast channels
                            For z = 0 To m_broadcastIPs.Length - 1
                                m_udpClient.Send(.BinaryImage, binaryLength, m_broadcastIPs(z))
                            Next

                            m_bytesBroadcasted += binaryLength
                            m_packetsPublished += 1
                        Catch ex As Exception
                            UpdateStatus("Error publishing data packet: " & ex.Message)
                        End Try

                        ' Increment row index
                        m_rowIndex += 1

                        If m_rowIndex = m_configFile.SampleRate Then
                            ' We published this sample, reset row index for next sample
                            m_rowIndex = -1

                            ' We send out a notification that a new sample has been published so that anyone can have a chance
                            ' to perform any last steps with the data before we remove it - in our case the DatAWare
                            ' Aggregator will pick this up and send the averaged sample to the permanent archive...
                            RaiseEvent SamplePublished(currentSample)
                            m_dataQueue.RemovePublishedSample()
                        End If
                    End With
                End If

                ' Set next broadcast time
                m_publishTime = currentTime.AddMilliseconds(m_sampleRate)
            End If
        Catch
            Throw
        Finally
            m_processTimer.Enabled = m_enabled
        End Try

    End Sub

    Private Sub m_monitorTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_monitorTimer.Elapsed

        If m_enabled Then
            ' Start process thread if needed
            If m_dataQueue.SampleCount > 1 And Not m_processing Then
                Processing = True
            End If

            RaiseEvent UnpublishedSamples(m_dataQueue.SampleCount - 1)
        ElseIf m_processing Then
            Processing = False
        End If

    End Sub

    Private Sub m_configFile_ConfigFileReloaded() Handles m_configFile.ConfigFileReloaded

        ' We recalculate the sample rate when the config file gets reloaded...
        m_sampleRate = Math.Floor(1000@ / m_configFile.SampleRate) - 1

    End Sub

#Region " IService Component Implementation "

    Public ReadOnly Property Name() As String Implements IServiceComponent.Name
        Get
            Return Me.GetType.Name
        End Get
    End Property

    Public Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

        ' This class executes as a result of a change in process state, so nothing to do...

    End Sub

    Public Sub ServiceStateChanged(ByVal NewState As ServiceState) Implements IServiceComponent.ServiceStateChanged

        ' We respect changes in service state by enabling or disabling our processing state as needed...
        Select Case NewState
            Case ServiceState.Paused
                Enabled = False
            Case ServiceState.Resumed
                Enabled = True
        End Select

    End Sub

    Public ReadOnly Property Status() As String Implements IServiceComponent.Status
        Get
            Dim publishingSample As DateTime
            Dim sampleDetail As String
            Dim currentTime As DateTime = BaselinedTimestamp(DateTime.Now.ToUniversalTime())

            With New StringBuilder
                For x As Integer = 0 To m_dataQueue.SampleCount - 1
                    .Append(vbCrLf & "     Sample " & x & " @ " & m_dataQueue(x).Timestamp.ToString("dd-MMM-yyyy HH:mm:ss") & ": ")

                    If x = 0 Then
                        .Append("publishing...")
                        publishingSample = m_dataQueue(x).Timestamp
                    Else
                        .Append("concentrating...")
                    End If

                    .Append(vbCrLf)
                Next

                sampleDetail = .ToString
            End With

            With New StringBuilder
                .Append("  Concentration process is: " & IIf(Enabled, "Enabled", "Disabled") & vbCrLf)
                .Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & vbCrLf)
                .Append("    Total process run time: " & SecondsToText(RunTime) & vbCrLf)
                .Append("          Discarded points: " & m_dataQueue.DiscardedPoints & vbCrLf)
                .Append("          Defined lag time: " & m_lagTime & " seconds" & vbCrLf)
                .Append("            Queued samples: " & m_dataQueue.SampleCount & vbCrLf)
                .Append("       Current server time: " & currentTime.ToString("dd-MMM-yyyy HH:mm:ss") & vbCrLf)
                .Append("        Most recent sample: " & m_dataQueue.BaseTime.ToString("dd-MMM-yyyy HH:mm:ss") & ", " & m_dataQueue.DistanceFromBaseTime(currentTime) & " second deviation" & vbCrLf)
                .Append("         Publishing sample: " & publishingSample.ToString("dd-MMM-yyyy HH:mm:ss") & ", " & (currentTime.Ticks - publishingSample.Ticks) / 10000000L & " second deviation" & vbCrLf)
                .Append("    Data packets published: " & m_packetsPublished & vbCrLf)
                .Append("         Mean publish rate: " & (m_packetsPublished / RunTime).ToString("0.00") & " samples/sec" & vbCrLf)
                .Append("       Mean broadcast rate: " & (m_bytesBroadcasted * 8 / RunTime / 1024 / 1024).ToString("0.00") & " mbps" & vbCrLf)
                .Append("    Total broadcast volume: " & (m_bytesBroadcasted / 1024 / 1024).ToString("0.00") & " Mb" & vbCrLf)
                .Append("    Referenced config file: " & TrimFileName(m_configFile.ConfigFileName, 50) & vbCrLf)
                .Append("       Defined sample rate: " & m_configFile.SampleRate & vbCrLf)
                .Append("        Total defined PMUs: " & m_configFile.PMUCount & vbCrLf)
                .Append(vbCrLf & "Current sample detail:" & vbCrLf & sampleDetail)

                Return .ToString()
            End With
        End Get
    End Property

#End Region

End Class
