'***********************************************************************
'  DatAWare.vb - TVA Service Template
'  Copyright © 2004 - TVA, all rights reserved
'
'  Common DatAWare functions for .NET
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
'       Initial version of source created
'
'  TODO: Flesh out code for reading data and add to TVA code library
'
'***********************************************************************

Imports TVA.Shared.DateTime
Imports DWApi

Namespace DatAWare

#Region " DatAWare Enumerations "

    Public Enum AccessMode
        [ReadOnly] = 1
        [WriteOnly] = 2
        [ReadWrite] = 3
    End Enum

    Public Enum Quality
        Unknown
        DeletedFromProcessing
        CouldNotCalcPoint
        DASFrontEndHardwareError
        SensorReadError
        OpenTransducerDetection
        InputCountsOutOfSensorRange
        UnreasonableHigh
        UnreasonableLow
        Old
        SuspectValueAboveHIHILimit
        SuspectValueBelowLOLOLimit
        SuspectValueAboveHILimit
        SuspectValueBelowLOLimit
        SuspectData
        DigitalSuspectAlarm
        InsertedValueAboveHIHILimit
        InsertedValueBelowLOLOLimit
        InsertedValueAboveHILimit
        InsertedValueBelowLOLimit
        InsertedValue
        DigitalInsertedStatusInAlarm
        LogicalAlarm
        ValueAboveHIHIAlarm
        ValueBelowLOLOAlarm
        ValueAboveHIAlarm
        ValueBelowLOAlarm
        DeletedFromAlarmChecks
        InhibitedByCutoutPoint
        Good
    End Enum

    Public Enum ReturnStatus
        Normal
        Abnormal
        Timeout
        NotConnected
    End Enum

#End Region

#Region " DatAWare TimeTag Class "

    Public Class TimeTag

        ' DatAWare time tags are measured as the number of seconds since January 1, 1995,
        ' so we calculate this date to get offset in ticks for later conversion...
        Private Shared timeTagOffsetTicks As Long = (New DateTime(1995, 1, 1, 0, 0, 0)).Ticks

        Private ttag As Double

        Public Sub New(ByVal ttag As Double)

            Value = ttag

        End Sub

        Public Sub New(ByVal dtm As DateTime)

            ' Zero base 100-nanosecond ticks from 1/1/1995 and convert to seconds
            Value = (dtm.Ticks - timeTagOffsetTicks) / 10000000L

        End Sub

        Public Property Value() As Double
            Get
                Return ttag
            End Get
            Set(ByVal val As Double)
                ttag = val
                If ttag < 0 Then ttag = 0
            End Set
        End Property

        Public Function ToDateTime() As DateTime

            ' Convert time tag seconds to 100-nanosecond ticks and add the 1/1/1995 offset
            Return New DateTime(ttag * 10000000L + timeTagOffsetTicks)

        End Function

        Public Overrides Function ToString() As String

            Return ToDateTime.ToString("dd-MMM-yyyy HH:mm:ss.fff")

        End Function

    End Class

#End Region

#Region " DatAWare Connection Class "

    Public Class Connection

        Implements IDisposable

        Public WithEvents DWAPI As DWAPIdllClass
        Public Server As String
        Public PlantCode As String
        Public Access As AccessMode
        Public TimeZone As Win32TimeZone
        Private connected As Boolean

        Public Event ServerMessage(ByVal message As String)
        Public Event ServerDebugMessage(ByVal message As String)

        Public Sub New(ByVal server As String, ByVal plantCode As String, Optional ByVal timeZone As String = "Central Standard Time", Optional ByVal access As AccessMode = AccessMode.ReadWrite)

            Me.DWAPI = New DWAPIdllClass
            Me.Server = server
            Me.PlantCode = plantCode
            Me.Access = access
            Me.TimeZone = GetWin32TimeZone(timeZone)

        End Sub

        Protected Overrides Sub Finalize()

            Close()

        End Sub

        Public Sub Open()

            Dim errorMessage As String

            DWAPI.ConnectTo(Server, PlantCode, Access, errorMessage)

            If Len(errorMessage) > 0 Then
                Throw New InvalidOperationException("Failed to connect to DatAWare server """ & Server & """ due to exception: " & errorMessage)
            Else
                connected = True
            End If

        End Sub

        Public Sub Close() Implements IDisposable.Dispose

            GC.SuppressFinalize(Me)
            If Not DWAPI Is Nothing Then DWAPI.Disconnect(PlantCode)
            connected = False

        End Sub

        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return connected
            End Get
        End Property

        Private Sub DWAPI_NormalMessage(ByRef mMsg As String) Handles DWApi.GotMessage

            RaiseEvent ServerMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & Server & """ Message: " & mMsg)
#End If

        End Sub

        Private Sub DWAPI_DebugMessage(ByRef mMsg As String) Handles DWApi.DEBUGMESSAGE

            RaiseEvent ServerDebugMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & Server & """ Debug Message: " & mMsg)
#End If

        End Sub

    End Class

#End Region

#Region " DatAWare Archive Unit "

    ' All points coming in for a given timeframe can be captured as a single data unit
    ' for easy push into DatAWare with this class
    Public Class ArchiveUnit

        ' For the sake of efficiency, we break down the points for this data unit into blocks...
        Public Class UnitBlock

            Public Channels As Integer()
            Public Values As Single()
            Public TTags As Double()
            Public Quals As Quality()
            Public Processed As Boolean

        End Class

        Public Connection As Connection
        Public TTag As TimeTag
        Public PlantCode As String
        Public PointCount As Integer
        Public Blocks As UnitBlock()
        Public PostAttempts As Integer

        Public Const BlockSize As Integer = 50

        Public Sub New(ByVal connection As Connection, ByVal pointCount As Integer, ByVal unitTime As DateTime, Optional ByVal unitQuality As Quality = Quality.Good)

            Dim blocks As Integer
            Dim remainder As Integer
            Dim x, y As Integer

            Me.Connection = connection
            Me.PlantCode = connection.PlantCode
            Me.PointCount = pointCount
            Me.PostAttempts = 2

            ' Create a new time tag in the proper time zone...
            Me.TTag = New TimeTag(connection.TimeZone.ToLocalTime(unitTime))

            ' Calculate the total number of needed blocks
            blocks = Math.DivRem(pointCount, BlockSize, remainder)
            If remainder > 0 Then blocks += 1

            Me.Blocks = Array.CreateInstance(GetType(UnitBlock), blocks)

            For x = 0 To blocks - 1
                Me.Blocks(x) = New UnitBlock
                If x = blocks - 1 And remainder > 0 Then
                    Me.Blocks(x).Channels = Array.CreateInstance(GetType(Integer), remainder)
                    Me.Blocks(x).Values = Array.CreateInstance(GetType(Single), remainder)
                    Me.Blocks(x).TTags = Array.CreateInstance(GetType(Double), remainder)
                    Me.Blocks(x).Quals = Array.CreateInstance(GetType(Quality), remainder)
                Else
                    Me.Blocks(x).Channels = Array.CreateInstance(GetType(Integer), BlockSize)
                    Me.Blocks(x).Values = Array.CreateInstance(GetType(Single), BlockSize)
                    Me.Blocks(x).TTags = Array.CreateInstance(GetType(Double), BlockSize)
                    Me.Blocks(x).Quals = Array.CreateInstance(GetType(Quality), BlockSize)
                End If
            Next

            ' Prepopulate time tag and qualilty values with given defaults
            For x = 0 To blocks - 1
                For y = 0 To Me.Blocks(x).TTags.Length - 1
                    Me.Blocks(x).TTags(y) = TTag.Value
                    Me.Blocks(x).Quals(y) = unitQuality
                Next
            Next

        End Sub

        Public Sub SetRow(ByVal index As Integer, ByVal channel As Integer, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            Dim block As Integer
            Dim offset As Integer

            block = index \ BlockSize
            offset = index - (block * BlockSize)

            With Blocks(block)
                .Channels(offset) = channel
                .Values(offset) = value
                If qual <> .Quals(offset) Then .Quals(offset) = qual
            End With

        End Sub

        Public Sub PostUnit()

            Dim status As ReturnStatus

            ' We only archive data units with a valid timestamp...
            If TTag.Value > 0 Then
                For x As Integer = 0 To Blocks.Length - 1
                    With Blocks(x)
                        If Not .Processed Then
                            Dim attempts As Integer
                            Do
                                attempts += 1
                                'Connection.DWAPI.Archive_PutBlock(PlantCode, .Channels, .Values, TimeTag, .Quals, .Channels.Length, status)
                                Connection.DWAPI.Archive_Put(PlantCode, .Channels, .Values, .TTags, .Quals, .Channels.Length, status)
                                .Processed = (status = ReturnStatus.Normal)
                                If status = ReturnStatus.NotConnected Then Connection.Open()
                            Loop While Not .Processed And attempts < PostAttempts
                        End If

                        If Not .Processed Then Throw New InvalidOperationException("Failed to archive DatAWare unit block due to exception: Archive_Put " & [Enum].GetName(GetType(ReturnStatus), status) & " error")
                    End With
                Next
            End If

        End Sub

    End Class

#End Region

End Namespace