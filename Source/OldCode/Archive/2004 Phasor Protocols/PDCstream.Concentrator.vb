'***********************************************************************
'  PDCstream.Concentrator.vb - PDCstream Concentrator
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
Imports TVA.Shared.DateTime
Imports TVA.Shared.FilePath
Imports TVA.Services

Namespace PDCstream

    Public Class Concentrator

        Implements IServiceComponent
        Implements IDisposable

        Private m_parent As DatAWarePDC
        Private m_configFile As ConfigFile
        Private m_lagTime As Double
        Private m_dataQueue As DataQueue
        Private m_descriptor As DescriptorPacket
        Private m_udpClient As UdpClient
        Private m_broadcastIPs As IPEndPoint()
        Private WithEvents m_processTimer As Timers.Timer
        Private WithEvents m_sweepTimer As Timers.Timer
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
            m_lagTime = lagTime
            m_dataQueue = New DataQueue(m_configFile)
            m_descriptor = New DescriptorPacket(m_configFile)
            m_udpClient = New UdpClient
            m_broadcastIPs = broadcastIPs
            m_processTimer = New Timers.Timer
            m_sweepTimer = New Timers.Timer
            m_enabled = True
            m_processing = False

            ' We set the sample rate accordingly allowing for a hint of more time per second to account for uneven rates
            With m_processTimer
                .Interval = Math.Floor(1000 / m_configFile.SampleRate) - 1
                .AutoReset = True
                .Enabled = False
            End With

            ' We check for samples that need to be removed every second
            With m_sweepTimer
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
                    m_processTimer.Enabled = True
                    m_startTime = DateTime.Now.Ticks
                    m_stopTime = 0
                    m_bytesBroadcasted = 0
                    m_packetsPublished = 0
                Else
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

            ' Since a typical process rate is 30 samples per second do eveything you can to make sure this
            ' function executes as quickly as possible...
            Static sentDescriptor As Boolean
            Dim binaryLength As Integer
            Dim x, y, z As Integer
            'Dim packetSent As Boolean

            ' Send a descriptor packet at the top of each minute...
            With DateTime.Now
                If .Second = 0 Then
                    If Not sentDescriptor Then
                        sentDescriptor = True
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
                    sentDescriptor = False
                End If
            End With

            ' Check for non-published samples...
            For x = 0 To m_dataQueue.SampleCount - 1
                With m_dataQueue(x)
                    If Not .Published Then
                        ' Check for non-published packets in this sample...
                        For y = 0 To .Rows.Length - 1
                            With .Rows(y)
                                If Not .Published Then
                                    ' We only send non-published data-packets that are older than specified lag time
                                    If m_dataQueue.DistanceFromBaseTime(.Timestamp) <= -m_lagTime Then
                                        Try
                                            binaryLength = .BinaryLength

                                            ' Send binary data packet image over UDP broadcast channels
                                            For z = 0 To m_broadcastIPs.Length - 1
                                                m_udpClient.Send(.BinaryImage, binaryLength, m_broadcastIPs(z))
                                            Next

                                            m_bytesBroadcasted += binaryLength
                                            m_packetsPublished += 1
                                        Catch ex As Exception
                                            UpdateStatus("Error publishing data packet: " & ex.Message)
                                        Finally
                                            ' Even an attempt at publishing counts - we don't have time to go back and try again...
                                            .Published = True
                                        End Try
                                    End If

                                    '' Under normal circumstances, this should be all we need to try to send - so we won't
                                    '' waste cycles looking for anything else that we'll catch at the next pass...
                                    'packetSent = True
                                    'Exit For
                                End If
                            End With
                        Next
                    End If
                End With
                'If packetSent Then Exit For
            Next

        End Sub

        Private Sub m_sweepTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_sweepTimer.Elapsed

            If m_enabled Then
                Dim unpublishedSamples As Integer

                ' Start process timer if needed
                If m_dataQueue.SampleCount > 0 And Not m_processing Then
                    Processing = True
                End If

                For x As Integer = 0 To m_dataQueue.SampleCount - 1
                    If m_dataQueue(x).Published Then
                        ' We send out a notification that a new sample has been published so that anyone can have a chance
                        ' to perform any last steps with the data before we remove it - in our case the DatAWare
                        ' Aggregator will pick this up and send the averaged sample to the permanent archive...
                        RaiseEvent SamplePublished(m_dataQueue(x))
                    Else
                        unpublishedSamples += 1
                    End If
                Next

                m_dataQueue.RemovePublishedSamples()

                RaiseEvent UnpublishedSamples(unpublishedSamples)
            ElseIf m_processing Then
                Processing = False
            End If

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
                Dim publishingSample As Long
                Dim sampleDetail As String

                With New StringBuilder
                    Dim pastNonPublished As Boolean

                    For x As Integer = 0 To m_dataQueue.SampleCount - 1
                        .Append(vbCrLf & "     Sample " & x & " @ " & m_dataQueue(x).Timestamp.ToString("dd-MMM-yyyy HH:mm:ss") & ": ")

                        If m_dataQueue(x).Published Then
                            .Append("published, awaiting aggregation...")
                        ElseIf pastNonPublished Then
                            .Append("concentrating...")
                        Else
                            .Append("publishing...")
                            pastNonPublished = True
                            publishingSample = m_dataQueue(x).Timestamp.Ticks
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
                    .Append("         Current base time: " & m_dataQueue.BaseTime.ToString("dd-MMM-yyyy HH:mm:ss") & vbCrLf)
                    .Append("      Queue time deviation: " & m_dataQueue.DistanceFromBaseTime(PDCstream.DataQueue.BaselinedTimestamp(DateTime.Now.ToUniversalTime())) & " seconds" & vbCrLf)
                    .Append("    Publish time deviation: " & ((DateTime.Now.ToUniversalTime().Ticks - publishingSample) / 10000000L).ToString("0.00") & " seconds" & vbCrLf)
                    .Append("            Queued samples: " & m_dataQueue.SampleCount & vbCrLf)
                    .Append("          Defined lag time: " & m_lagTime & " seconds" & vbCrLf)
                    .Append(" Data publication interval: " & m_processTimer.Interval & " ms" & vbCrLf)
                    .Append("    Data packets published: " & m_packetsPublished & vbCrLf)
                    .Append("  Data packet publish rate: " & (m_packetsPublished / RunTime).ToString("0.00") & " samples/sec" & vbCrLf)
                    .Append("            Broadcast rate: " & (m_bytesBroadcasted * 8 / RunTime / 1024 / 1024).ToString("0.00") & " mbps" & vbCrLf)
                    .Append("    Total broadcast volume: " & (m_bytesBroadcasted / 1024 / 1024).ToString("0.00") & " Mb" & vbCrLf)
                    .Append("    Referenced config file: " & TrimFileName(m_configFile.ConfigFileName, 50) & vbCrLf)
                    .Append("       Defined sample rate: " & m_configFile.SampleRate & vbCrLf)
                    .Append("        Total defined PMUs: " & m_configFile.PMUCount & vbCrLf)
                    .Append(vbCrLf & "Current sample details:" & vbCrLf & sampleDetail)


                    Return .ToString()
                End With
            End Get
        End Property

#End Region

    End Class

End Namespace