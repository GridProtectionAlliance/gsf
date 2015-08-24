'***********************************************************************
'  PDCToDatAWare.vb - TVA Service Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  PDC to DatAWare conversion code
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source generated for new Windows service
'       project "BPA PDC To DatAWare Loader".
'
'***********************************************************************

Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Data.OleDb
Imports TVA.Shared.Bit
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Database.Common
Imports TVA.Config.Common
'Imports TVA.ESO.Ssam
Imports TVA.DatAWare

Namespace PDCToDatAWare

#Region " PMU Date Class "

    <ComVisible(False)> _
    Public Class PMUDate

        ' PMU dates (as reported by PDC Data Reader) are measured as the number of seconds since 1/1/1970,
        ' so we calculate this date to get offset in ticks for later conversion...
        Private Shared pmuDateOffsetTicks As Long = (New DateTime(1970, 1, 1, 0, 0, 0)).Ticks

        Private seconds As Double

        Public Sub New(ByVal seconds As Double)

            Value = seconds

        End Sub

        Public Sub New(ByVal dtm As DateTime)

            ' Zero base 100-nanosecond ticks from 1/1/1970 and convert to seconds
            Value = (dtm.Ticks - pmuDateOffsetTicks) / 10000000L

        End Sub

        Public Property Value() As Double
            Get
                Return seconds
            End Get
            Set(ByVal val As Double)
                seconds = val
                If seconds < 0 Then seconds = 0
            End Set
        End Property

        Public Function ToDateTime() As DateTime

            ' Convert seconds to 100-nanosecond ticks and add the 1/1/1970 offset
            Return New DateTime(seconds * 10000000L + pmuDateOffsetTicks)

        End Function

        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

    End Class

#End Region

#Region " DatAWare Formatted PMU Data Unit Class "

    ' This class converts a PDC data unit into a DatAWare style format
    <ComVisible(False)> _
    Public Class DataUnit

        Private m_events As ArrayList

        Public Property Events() As StandardEvent()
            Get
                Return m_events.ToArray(GetType(StandardEvent))
            End Get
            Set(ByVal Value As StandardEvent())
                m_events = New ArrayList(Value)
            End Set
        End Property

        Public ReadOnly Property EventCount() As Integer
            Get
                Return m_events.Count
            End Get
        End Property

        Public Sub New(ByVal analogIndices As Integer(), ByVal analogData As Double(,), ByVal digitalIndices As Integer(), ByVal digitalData As UInt16(,))

            Dim rowTime As DateTime
            Dim analogEvents As Integer = analogData.GetLength(0) * (analogIndices.Length - 1)
            Dim digitalEvents As Integer = digitalData.GetLength(0) * (digitalIndices.Length - 3)
            Dim eventIndex As Integer
            Dim x, y As Integer

            'Events = Array.CreateInstance(GetType(StandardEvent), analogEvents + digitalEvents)
            m_events = New ArrayList

            For x = 0 To analogData.GetLength(0) - 1
                ' Get analog row time from this analog data set
                rowTime = (New PMUDate(analogData(x, 0))).ToDateTime

                ' Populate the DatAWare standard event structures for the analog data
                For y = 1 To analogIndices.Length - 1
                    If analogIndices(y) > -1 Then
                        ' Create a new DatAWare standard event to hold this analog data
                        m_events.Add(New StandardEvent(analogIndices(y), rowTime, Convert.ToSingle(analogData(x, y))))
                    End If
                Next
            Next

            For x = 0 To digitalData.GetLength(0) - 1
                ' Get digital row time from this digital data set
                rowTime = (New PMUDate(MakeDWord(Convert.ToInt32(digitalData(x, 0)), Convert.ToInt32(digitalData(x, 1))))).ToDateTime

                ' Populate the DatAWare standard event structures for the digital data
                For y = 3 To digitalIndices.Length - 1
                    If digitalIndices(y) > -1 Then
                        ' Create a new DatAWare standard event to hold this digital data
                        m_events.Add(New StandardEvent(digitalIndices(y), rowTime, Convert.ToSingle(digitalData(x, y))))
                    End If
                Next
            Next

        End Sub

    End Class

#End Region

#Region " PMU to DatAWare Conversion Class "

    <ComVisible(False)> _
    Public Class Converter

        Private analogIndices As Integer()
        Private digitalIndices As Integer()
        Private queuedUnits As ArrayList
        Private processedCount As Long
        Private isProcessing As Boolean
        Private startTime As Long
        Private stopTime As Long
        Private WithEvents processTimer As Timers.Timer

        Public Instance As Integer

        Public Sub New()

            Me.isProcessing = False
            Me.queuedUnits = New ArrayList

        End Sub

        Public Sub QueueNewData(ByVal analogData As Double(,), ByVal digitalData As UInt16(,))

            Dim newData As New DataUnit(analogIndices, analogData, digitalIndices, digitalData)

            SyncLock queuedUnits.SyncRoot
                queuedUnits.Add(newData)
            End SyncLock

        End Sub

        Public ReadOnly Property QueuedDataUnits() As Long
            Get
                SyncLock queuedUnits.SyncRoot
                    Return queuedUnits.Count
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ProcessedEvents() As Long
            Get
                Return processedCount
            End Get
        End Property

        Public Sub LoadDescriptorData(ByVal csvPointFile As String, ByVal analogNames As String(), ByVal digitalNames As String())

            Dim pointIndexTable As Hashtable
            Dim point As Object

            ' Load point data as defined in DatAWare...
            pointIndexTable = LoadPointIndexTable(csvPointFile)

            ' Defining these point lists as array's means that the desired DatAWare database index
            ' will be directly accessible by index alone (i.e., no need for key lookup later)
            analogIndices = Array.CreateInstance(GetType(Integer), analogNames.Length)
            digitalIndices = Array.CreateInstance(GetType(Integer), digitalNames.Length)

            For x As Integer = 0 To analogIndices.Length - 1
                ' Lookup database index by point description
                point = pointIndexTable(analogNames(x))

                If point Is Nothing Then
                    analogIndices(x) = -1
                Else
                    analogIndices(x) = point
                End If
            Next

            For x As Integer = 0 To digitalIndices.Length - 1
                ' Lookup database index by point description
                point = pointIndexTable(digitalNames(x))

                If point Is Nothing Then
                    digitalIndices(x) = -1
                Else
                    digitalIndices(x) = point
                End If
            Next

            ' Produce an exception list of points not defined in DatAWare
            With IO.File.CreateText([Interface].ApplicationPath & "UndefinedAnalogIndices" & Instance & ".csv")
                For x As Integer = 0 To analogIndices.Length - 1
                    If analogIndices(x) = -1 Then .WriteLine(analogNames(x))
                Next
                .Close()
            End With

            ' Produce an exception list of points not defined in DatAWare
            With IO.File.CreateText([Interface].ApplicationPath & "UndefinedDigitalIndices" & Instance & ".csv")
                For x As Integer = 0 To digitalIndices.Length - 1
                    If digitalIndices(x) = -1 Then .WriteLine(digitalNames(x))
                Next
                .Close()
            End With

            ' As soon as we've got our descriptor data, we'll assume processing has begun...
            Processing = True

        End Sub

        Private Function LoadPointIndexTable(ByVal csvPointFile As String) As Hashtable

            Dim pointIndexTable As Hashtable
            Dim csvConnection As OleDbConnection

            ' The CSV file that was used to load points into DatAWare is in a specific format, so we open the file as a table
            ' and read in the point's database index and description so we can create a cross reference (based on description)
            ' between PDC data points and DatAWare database indicies (you need the database index to archive DatAWare data)
            Try
                pointIndexTable = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                csvConnection = New OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & JustPath(csvPointFile) & ";Extended Properties='Text;HDR=Yes;FMT=Delimited'")
                csvConnection.Open()

                With ExecuteReader("SELECT [Database Index], [Description] FROM " & JustFileName(csvPointFile), csvConnection)
                    Do While .Read()
                        pointIndexTable.Add(.Item(1), CInt(.Item(0)))
                    Loop

                    .Close()
                End With
            Catch
                Throw
            Finally
                If Not csvConnection Is Nothing Then csvConnection.Close()
            End Try

            Return pointIndexTable

        End Function

        Public Function GetEventDatabase() As StandardEvent()

            Dim standardEvents As New ArrayList

            For Each de As DictionaryEntry In LoadPointIndexTable(Variables("PDCDataReader.PointList" & Instance))
                standardEvents.Add(New StandardEvent(de.Value, New TimeTag(Date.Now), 0.0!))
            Next

            Return standardEvents.ToArray(GetType(StandardEvent))

        End Function

        Public Function GetEventData(ByRef queueIsEmpty As Boolean) As StandardEvent()

            Dim standardEvents As New ArrayList
            Dim eventCount As Integer
            Dim x As Integer

            ' Get next events to be processed
            With queuedUnits
                SyncLock .SyncRoot
                    If .Count > 0 Then
                        Dim eventData As DataUnit

                        ' Keep getting events until we get up to the buffer limit
                        Do While .Count > 0
                            eventData = .Item(0)

                            ' See if all the events in this data unit will fit into the buffer
                            If eventData.EventCount + eventCount <= [Interface].MaximumEvents Then
                                ' All of the events will fit, so we'll add all of them and check the next one
                                eventCount += eventData.EventCount
                                standardEvents.AddRange(eventData.Events)
                                .RemoveAt(0)
                            Else
                                ' Get as many events from data unit as possible and leave remaining events for next poll
                                Dim available As Integer = [Interface].MaximumEvents - eventCount

                                If available > 0 Then
                                    Dim remaining As Integer = eventData.EventCount - available
                                    Dim willFit As StandardEvent() = Array.CreateInstance(GetType(StandardEvent), available)
                                    Dim wontFit As StandardEvent() = Array.CreateInstance(GetType(StandardEvent), remaining)

                                    ' Separate items that will fit into remaining available buffer space from those that won't
                                    Array.Copy(eventData.Events, 0, willFit, 0, available)
                                    Array.Copy(eventData.Events, available, wontFit, 0, remaining)

                                    ' Add the events what will fit onto the event array that were returning
                                    eventCount += willFit.Length
                                    standardEvents.AddRange(willFit)

                                    ' Update the data unit so that it only hangs on to the remaining events that wouldn't fit
                                    eventData.Events = wontFit
                                End If

                                ' At this point there will be no more room in the buffer, so we pack up and go...
                                Exit Do
                            End If
                        Loop
                    End If
                End SyncLock
            End With

            processedCount += standardEvents.Count

            ' We log a success event to SSAM for primary service process every time we've processed 100 data units...
            'If processedCount Mod 100 = 0 Then _
            '   parent.LogSSAMEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamFlowEntity, Variables("Ssam.BPAPDCLoaderPrimaryProcess"), "Processed " & processedCount & " archive units")

            queueIsEmpty = (QueuedDataUnits = 0)

            Return standardEvents.ToArray(GetType(StandardEvent))

        End Function

        Public ReadOnly Property Status() As String
            Get
                With New StringBuilder
                    .Append("Converter Status for DAQ Instance " & Instance & ":" & vbCrLf)
                    .Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & vbCrLf)
                    .Append("    Total process run time: " & SecondsToText(RunTime) & vbCrLf)
                    .Append("      Listening on IP port: " & Variables("PDCDataReader.ListenPort" & Instance) & vbCrLf)
                    .Append("          BPA PDC ini file: " & Variables("PDCDataReader.ConfigFile" & Instance) & vbCrLf)
                    .Append("      Point index csv file: " & Variables("DatAWare.PointListFile") & vbCrLf)
                    .Append("        Analog point count: " & ArrayLength(analogIndices, -1) & vbCrLf)
                    .Append("       Digital point count: " & ArrayLength(digitalIndices, -3) & vbCrLf)

                    Return .ToString()
                End With
            End Get
        End Property

        Private Function ArrayLength(ByVal arr As Array, ByVal offset As Integer) As Integer

            If arr Is Nothing Then
                Return 0
            Else
                Return arr.Length + offset
            End If

        End Function

        Public Property Processing() As Boolean
            Get
                Return isProcessing
            End Get
            Set(ByVal Value As Boolean)
                isProcessing = Value

                If isProcessing Then
                    startTime = DateTime.Now.Ticks
                    stopTime = 0
                Else
                    stopTime = DateTime.Now.Ticks
                End If
            End Set
        End Property

        Public ReadOnly Property RunTime() As Double
            Get
                Dim processingTime As Long

                If startTime > 0 Then
                    If stopTime > 0 Then
                        processingTime = stopTime - startTime
                    Else
                        processingTime = DateTime.Now.Ticks - startTime
                    End If
                End If

                If processingTime < 0 Then processingTime = 0

                Return processingTime / 10000000L
            End Get
        End Property

    End Class

#End Region

End Namespace
