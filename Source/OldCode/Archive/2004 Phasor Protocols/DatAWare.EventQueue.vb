'***********************************************************************
'  DatAWare.EventQueue.vb - DatAWare Event Queue
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
'  11/5/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "DatAWare PDC".
'
'***********************************************************************

Imports System.Text
Imports System.Threading
Imports TVA.Config.Common
Imports TVA.Shared.DateTime
Imports TVA.Services
Imports DatAWarePDC.PDCstream

Namespace DatAWare

    Public Class EventQueue

        Implements IServiceComponent
        Implements IDisposable

        Private Class DataPacket

            Public PlantCode As String
            Public EventBuffer As Byte()

            Public Sub New(ByVal plantCode As String, ByVal eventBuffer As Byte())

                Me.PlantCode = plantCode
                Me.EventBuffer = eventBuffer

            End Sub

        End Class

        Private m_parent As DatAWarePDC
        Private m_configFile As ConfigFile
        Private WithEvents m_dataQueue As DataQueue
        Private m_serverPoints As Hashtable
        Private m_enabled As Boolean
        Private m_packetsReceived As Long
        Private m_processedEvents As Long
        Private m_activeThreads As Integer

#Region " Setup and Class Definition Code "

        ' Class auto-generated using TVA service template at Fri Nov 5 09:43:23 EST 2004
        Public Sub New(ByVal parent As DatAWarePDC)

            m_parent = parent
            m_configFile = m_parent.Concentrator.ConfigFile
            m_dataQueue = m_parent.Concentrator.DataQueue
            m_serverPoints = New Hashtable
            m_enabled = True

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

        Public Property Enabled() As Boolean
            Get
                Return m_enabled
            End Get
            Set(ByVal Value As Boolean)
                m_enabled = Value
            End Set
        End Property

#End Region

#Region " Event Queue Implementation "

        ' After a listener has been created, we will have a connection to the source DatAWare server - we use this
        ' connection to create a cross-reference list between DatAWare points and PMU data...
        Public Sub DefinePointList(ByVal connection As DatAWare.Connection, ByVal userName As String, ByVal password As String)

            Try
                m_serverPoints.Add(connection.PlantCode, New PMUServerPoints(m_parent.Concentrator, connection, userName, password))
            Catch ex As Exception
                ' This should be considered an unrecoverable error - connecting to the DatAWare server
                ' to retrieve a point list will be central to the concentration process...
                Throw New InvalidOperationException("Failed to load DatAWare points for " & connection.Server & "/" & connection.PlantCode & " due to exception: " & ex.Message, ex)
            End Try

        End Sub

        Public ReadOnly Property Points(ByVal plantCode As String) As PMUServerPoints
            Get
                Return DirectCast(m_serverPoints(plantCode), PMUServerPoints)
            End Get
        End Property

        Public ReadOnly Property Parent() As DatAWarePDC
            Get
                Return m_parent
            End Get
        End Property

        Public Sub QueueEventData(ByVal plantCode As String, ByVal eventBuffer As Byte())

            'If Enabled Then ThreadPool.QueueUserWorkItem(AddressOf ProcessEventBuffer, New DataPacket(plantCode, eventBuffer))
            ProcessEventBuffer(New DataPacket(plantCode, eventBuffer))
            m_packetsReceived += 1

        End Sub

        ' This method is expected to be run on an independent thread...
        Private Sub ProcessEventBuffer(ByVal stateInfo As Object)

            Try
                m_activeThreads += 1

                Dim eventData As DataPacket = DirectCast(stateInfo, DataPacket)
                Dim dataPoint As PMUDataPoint

                ' Parse events out of data packet and create a new PMU data point from timestamp and value
                For packetIndex As Integer = 0 To eventData.EventBuffer.Length - 1 Step StandardEvent.BinaryLength
                    If packetIndex + StandardEvent.BinaryLength < eventData.EventBuffer.Length Then
                        With New StandardEvent(eventData.EventBuffer, packetIndex)
                            ' Make sure we have a point defined for this value
                            If Points(eventData.PlantCode).GetPoint(.DatabaseIndex, dataPoint) Then
                                dataPoint.Timestamp = .TTag.ToDateTime
                                dataPoint.Value = .Value

                                ' If the new value is received on change, we'll update the samples in our current
                                ' data queue to use latest value...
                                If dataPoint.ReceivedOnChange Then
                                    Points(eventData.PlantCode)(.DatabaseIndex) = dataPoint
                                    UpdateReceivedOnChangePoints(dataPoint)
                                End If

                                ' Add this point value to the PDC concentrator data queue
                                m_dataQueue.SortDataPoint(dataPoint)
                            End If
                        End With

                        m_processedEvents += 1
                    End If
                Next
            Catch ex As Exception
                UpdateStatus("Exception in DatAWare.EventQueue.ProcessEventBuffer: " & ex.Message)
                Throw ex
            Finally
                m_activeThreads -= 1
            End Try

        End Sub

        Private Sub m_dataQueue_DataError(ByVal message As String) Handles m_dataQueue.DataError

            UpdateStatus(message)

        End Sub

        ' Prepopulate all received on changed points when new samples are created
        Private Sub m_dataQueue_NewDataSampleCreated(ByVal newDataSample As PDCstream.DataSample) Handles m_dataQueue.NewDataSampleCreated

            Dim timestamp As DateTime

            For x As Integer = 0 To newDataSample.Rows.Length - 1
                timestamp = newDataSample.Rows(x).Timestamp

                For Each pointSet As PMUServerPoints In m_serverPoints.Values
                    For Each dataPoint As PMUDataPoint In pointSet
                        If dataPoint.ReceivedOnChange Then
                            dataPoint.Timestamp = timestamp
                            m_dataQueue.SortDataPoint(dataPoint)
                        End If
                    Next
                Next
            Next

        End Sub

        ' When we receive a new point marked as "received on change", we update all the values from this point in time on...
        Private Sub UpdateReceivedOnChangePoints(ByVal dataPoint As PMUDataPoint)

            Dim stepInterval As Double = 1000 / m_configFile.SampleRate
            Dim timestamp As DateTime = dataPoint.Timestamp
            Dim baseTime As DateTime = DataQueue.BaselinedTimestamp(timestamp)
            Dim sampleIndex As Integer = m_dataQueue.GetSampleIndex(baseTime)

            If sampleIndex > -1 Then
                ' Update all values in current sample starting with current time interval
                For offset As Double = (Math.Floor(timestamp.Millisecond / stepInterval) + 0.5) * stepInterval To 999 Step stepInterval
                    dataPoint.Timestamp = baseTime.AddMilliseconds(offset)
                    m_dataQueue.SortDataPoint(dataPoint)
                Next

                ' Update the point value in all remaining samples
                For x As Integer = sampleIndex + 1 To m_dataQueue.SampleCount - 1
                    For y As Integer = 0 To m_dataQueue.Sample(x).Rows.Length - 1
                        dataPoint.Timestamp = m_dataQueue.Sample(x).Rows(y).Timestamp
                        m_dataQueue.SortDataPoint(dataPoint)
                    Next
                Next
            End If
        End Sub

#End Region

#Region " IService Component Implementation "

        ' Service component implementation
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
                With New StringBuilder
                    .Append("  Queue is currently: " & IIf(Enabled, "Enabled", "Disabled") & vbCrLf)
                    .Append("    Packets received: " & m_packetsReceived & vbCrLf)
                    .Append("    Processed events: " & m_processedEvents & vbCrLf)
                    .Append("      Active threads: " & m_activeThreads & vbCrLf & vbCrLf)
                    .Append("  Referencing points on the following DatAWare servers: " & vbCrLf & vbCrLf)

                    For Each de As DictionaryEntry In m_serverPoints
                        .Append("        Plant Code " & de.Key & ": " & DirectCast(de.Value, PMUServerPoints).Count & " points")
                    Next

                    Return .ToString()
                End With
            End Get
        End Property

#End Region

    End Class

End Namespace

