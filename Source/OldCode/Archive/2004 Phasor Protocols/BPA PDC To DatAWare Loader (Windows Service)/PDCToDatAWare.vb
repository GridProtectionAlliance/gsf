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

Imports System.Data.OleDb
Imports System.Text
Imports TVA.Services
Imports TVA.Shared.Bit
Imports TVA.Shared.FilePath
Imports TVA.Shared.DateTime
Imports TVA.Database.Common
Imports TVA.Config.Common
Imports TVA.ESO.Ssam

Namespace PDCToDatAWare

#Region " PMU Date Class "

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
    Public Class DataUnit

        Public AnalogDataUnits As DatAWare.ArchiveUnit()
        Public DigitalDataUnits As DatAWare.ArchiveUnit()

        Public Sub New(ByVal connection As DatAWare.Connection, ByVal analogIndices As Integer(), ByVal analogData As Double(,), ByVal digitalIndices As Integer(), ByVal digitalData As UInt16(,))

            Dim rowTime As PMUDate
            Dim x, y As Integer

            AnalogDataUnits = Array.CreateInstance(GetType(DatAWare.ArchiveUnit), analogData.GetLength(0))
            DigitalDataUnits = Array.CreateInstance(GetType(DatAWare.ArchiveUnit), digitalData.GetLength(0))

            For x = 0 To analogData.GetLength(0) - 1
                ' Get analog row time from this analog data set
                rowTime = New PMUDate(analogData(x, 0))

                ' Create a new DatAWare archive unit to hold this analog data
                AnalogDataUnits(x) = New DatAWare.ArchiveUnit(connection, analogIndices.Length - 1, rowTime.ToDateTime())

                ' Populate the archive unit
                For y = 1 To analogIndices.Length - 1
                    AnalogDataUnits(x).SetRow(y - 1, analogIndices(y), Convert.ToSingle(analogData(x, y)))
                Next
            Next

            For x = 0 To digitalData.GetLength(0) - 1
                ' Get digital row time from this digital data set
                rowTime = New PMUDate(MakeDWord(Convert.ToInt32(digitalData(x, 0)), Convert.ToInt32(digitalData(x, 1))))

                ' Create a new DatAWare archive unit to hold this digital data
                DigitalDataUnits(x) = New DatAWare.ArchiveUnit(connection, digitalIndices.Length - 3, rowTime.ToDateTime())

                ' Populate the archive unit
                For y = 3 To digitalIndices.Length - 1
                    DigitalDataUnits(x).SetRow(y - 3, digitalIndices(y), Convert.ToSingle(digitalData(x, y)))
                Next
            Next

        End Sub

    End Class

#End Region

#Region " PMU to DatAWare Conversion Class "

    Public Class Converter

        Implements IServiceComponent
        Implements IDisposable

        Public Server As String
        Public PlantCode As String
        Public TimeZone As String

        Private parent As BPAPDCToDatAWareLoader
        Private connection As DatAWare.Connection
        Private analogIndices As Integer()
        Private digitalIndices As Integer()
        Private queuedUnits As ArrayList
        Private processedCount As Long
        Private dataReadCount As Long
        Private appPath As String
        Private isEnabled As Boolean
        Private isProcessing As Boolean
        Private startTime As Single
        Private stopTime As Single
        Private WithEvents processTimer As Timers.Timer

        Public Sub New(ByVal parent As BPAPDCToDatAWareLoader, ByVal server As String, ByVal plantCode As String, ByVal timeZone As String)

            Me.parent = parent
            Me.Server = server
            Me.PlantCode = plantCode
            Me.TimeZone = timeZone
            Me.appPath = AddPathSuffix(parent.ServiceHelper.AppPath)
            Me.isEnabled = True
            Me.isProcessing = False

            Me.queuedUnits = New ArrayList
            Me.processTimer = New Timers.Timer

            With Me.processTimer
                .AutoReset = False
                .Interval = 1
            End With

            Me.connection = New DatAWare.Connection(server, plantCode, timeZone, DatAWare.AccessMode.WriteOnly)
            Me.connection.Open()

        End Sub

        Protected Overrides Sub Finalize()

            Dispose()

        End Sub

        Public Sub Dispose() Implements IServiceComponent.Dispose, IDisposable.Dispose

            GC.SuppressFinalize(Me)
            If Not connection Is Nothing Then connection.Close()

        End Sub

        Public Property Enabled() As Boolean
            Get
                Return isEnabled
            End Get
            Set(ByVal Value As Boolean)
                isEnabled = Value
                If isEnabled Then processTimer.Enabled = (QueuedDataUnits > 0)
            End Set
        End Property

        Public Sub QueueNewData(ByVal analogData As Double(,), ByVal digitalData As UInt16(,))

            ' When service is paused we can't immediately stop PMU data from coming in (have to wait for PDC Data Reader to shutdown),
            ' but if the service has been paused we should respectfully stop processing any incoming data
            If Enabled Then
                Dim newData As New DataUnit(connection, analogIndices, analogData, digitalIndices, digitalData)

                SyncLock queuedUnits.SyncRoot
                    queuedUnits.Add(newData)
                End SyncLock

                processTimer.Enabled = True
            End If

            dataReadCount += 1

        End Sub

        Public ReadOnly Property QueuedDataUnits() As Long
            Get
                SyncLock queuedUnits.SyncRoot
                    Return queuedUnits.Count
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property ProcessedDataUnits() As Long
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
            With IO.File.CreateText(appPath & "UndefinedAnalogIndices.csv")
                For x As Integer = 0 To analogIndices.Length - 1
                    If analogIndices(x) = -1 Then
                        .WriteLine(analogNames(x))
                    End If
                Next
                .Close()
            End With

            ' Produce an exception list of points not defined in DatAWare
            With IO.File.CreateText(appPath & "UndefinedDigitalIndices.csv")
                For x As Integer = 0 To digitalIndices.Length - 1
                    If digitalIndices(x) = -1 Then
                        .WriteLine(digitalNames(x))
                    End If
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

        Private Sub processTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles processTimer.Elapsed

            Static processing As Boolean
            Dim newData As DataUnit
            Dim x As Integer

            If Enabled And Not processing Then
                Try
                    processing = True

                    ' Get next item to process
                    SyncLock queuedUnits.SyncRoot
                        If queuedUnits.Count > 0 Then
                            newData = queuedUnits(0)
                            queuedUnits.RemoveAt(0)
                        Else
                            Exit Sub
                        End If
                    End SyncLock

                    With newData
                        For x = 0 To .AnalogDataUnits.Length - 1
                            ' Send this archive unit to DatAWare
                            .AnalogDataUnits(x).PostUnit()
                        Next

                        For x = 0 To .DigitalDataUnits.Length - 1
                            ' Send this archive unit to DatAWare
                            .DigitalDataUnits(x).PostUnit()
                        Next
                    End With

                    processedCount += 1

                    ' We log a success event to SSAM for primary service process every time we've processed 100 data units...
                    If processedCount Mod 100 = 0 Then _
                        parent.LogSSAMEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamFlowEntity, Variables("Ssam.BPAPDCLoaderPrimaryProcess"), "Processed " & processedCount & " archive units")
                Catch ex As Exception
                    Debug.WriteLine("Process Exception: " & ex.Message)

                    ' We requeue items on process failure
                    If Not newData Is Nothing Then
                        SyncLock queuedUnits.SyncRoot
                            queuedUnits.Insert(0, newData)
                        End SyncLock
                    End If
                Finally
                    processing = False
                End Try

                processTimer.Enabled = (Enabled And QueuedDataUnits > 0)
            End If

        End Sub

#Region " IService Component Implementation "

        ' Service component implementation
        Public ReadOnly Property Name() As String Implements IServiceComponent.Name
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

            ' This class executes as a result of a new data queued from PDC data reader, and the service process state
            ' should never change since there's nothing for the service to do on a schedule - so nothing to do here...

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
                    .Append("PDCToDatAWare converter is: " & IIf(Enabled, "Enabled", "Disabled") & vbCrLf)
                    .Append("  Current processing state: " & IIf(Processing, "Executing", "Idle") & vbCrLf)
                    .Append("    Total process run time: " & RunTime() & vbCrLf)
                    .Append("    PDC reader initialized: " & (Not parent.dataReader Is Nothing) & vbCrLf)
                    .Append("    PDC reader config file: " & Variables("PDCDataReader.ConfigFile") & vbCrLf)
                    .Append("      PDC reader data port: " & Variables("PDCDataReader.ListenPort") & vbCrLf)
                    .Append("          Total data reads: " & dataReadCount & vbCrLf)
                    .Append("    DatAWare connection is: " & IIf(connection.IsOpen, "Open", "Closed") & vbCrLf)
                    .Append("           DatAWare server: " & Variables("DatAWare.Server") & vbCrLf)
                    .Append("       DatAWare plant code: " & Variables("DatAWare.PlantCode") & vbCrLf)
                    .Append("      Configured time zone: " & Variables("DatAWare.TimeZone") & vbCrLf)
                    .Append("      Point index csv file: " & Variables("DatAWare.PointListFile") & vbCrLf)                    
                    .Append("        Analog point count: " & ArrayLength(analogIndices, -1) & vbCrLf)
                    .Append("       Digital point count: " & ArrayLength(digitalIndices, -3) & vbCrLf)
                    .Append("        Processed requests: " & ProcessedDataUnits & vbCrLf)
                    .Append("           Queued requests: " & QueuedDataUnits & vbCrLf)

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
                    startTime = Timer
                    stopTime = 0
                Else
                    stopTime = Timer
                End If
            End Set
        End Property

        Public ReadOnly Property RunTime() As String
            Get
                Dim ProcessingTime As Single

                If startTime > 0 Then
                    If stopTime > 0 Then
                        ProcessingTime = stopTime - startTime
                    Else
                        ProcessingTime = Timer - startTime
                    End If
                End If

                If ProcessingTime < 0 Then ProcessingTime = 0

                Return SecondsToText(ProcessingTime)
            End Get
        End Property

#End Region

    End Class

#End Region

End Namespace
