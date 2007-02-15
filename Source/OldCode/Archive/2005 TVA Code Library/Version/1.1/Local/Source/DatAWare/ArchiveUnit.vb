'***********************************************************************
'  ArchiveUnit.vb - DatAWare ArchiveUnit Class
'  Copyright © 2005 - TVA, all rights reserved
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
'***********************************************************************
Option Explicit On 

Namespace DatAWare

    ' All points coming in for a given timeframe can be captured as a single data unit
    ' for easy push into DatAWare with this class
    ' Note: This class expects a timestamp in UTC
    Public Class ArchiveUnit

        Public Connection As Connection
        Public TTag As TimeTag
        Public PlantCode As String
        Public PointCount As Integer
        Public Blocks As DataBlock()
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

            ' For the sake of efficiency in posting to DatAWare, we break down the points for this data unit into blocks...
            Me.Blocks = Array.CreateInstance(GetType(DataBlock), blocks)

            For x = 0 To blocks - 1
                If x = blocks - 1 And remainder > 0 Then
                    Me.Blocks(x) = New DataBlock(remainder)
                Else
                    Me.Blocks(x) = New DataBlock(BlockSize)
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

        Public Sub SetRow(ByVal rowIndex As Integer, ByVal databaseIndex As Integer, ByVal value As Single, Optional ByVal qual As Quality = Quality.Good)

            Dim block As Integer
            Dim offset As Integer

            block = rowIndex \ BlockSize
            offset = rowIndex - (block * BlockSize)

            With Blocks(block)
                .Channels(offset) = databaseIndex
                .Values(offset) = value
                If qual <> .Quals(offset) Then .Quals(offset) = qual
            End With

        End Sub

        Public Sub Post()

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

End Namespace
