'***********************************************************************
'  DatAWare.vb - DatAWare DAQ Template
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
'  TODO: Flesh out code for more general DatAWare IO and add to the
'        TVA .NET code library as an independent assembly
'
'***********************************************************************

Imports System.Runtime.InteropServices
Imports BpaPdcLoader

Namespace DatAWare

#Region " DatAWare Enumerations "

    <ComVisible(False)> _
    Public Enum AccessMode
        [ReadOnly] = 1
        [WriteOnly] = 2
        [ReadWrite] = 3
    End Enum

    <ComVisible(False)> _
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

    <ComVisible(False)> _
    Public Enum ReturnStatus
        Normal
        Abnormal
        Timeout
        NotConnected
    End Enum

#End Region

#Region " DatAWare TimeTag Class "

    <ComVisible(False)> _
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

#Region " DatAWare Data Structures "

    ' Standard-format data event
    <ComVisible(False)> _
    Public Class StandardEvent

        Public DatabaseIndex As Integer
        Public TTag As TimeTag
        Public QualityBits As Integer
        Public Value As Single

        ' ***************************************
        ' *  Bit usage of IntEvent.Quality word
        ' *
        ' *    Bits 0-4 (data quality indicator, a number between 0 and 31, 5 bits.
        ' *              Maps to the same qualities as used by PMS process computer.)
        ' *
        ' *             mask = &H1F
        ' *
        ' *    Bits 5-10 (index of time-zone used, number between 0 and 51, 6 bits)
        ' *
        ' *             mask = &H7E0
        ' *
        ' *    Bit 11 (Flag for Daylight Savings Time, one bit.  When set, indicates
        ' *            DST is in effect.  When clear, Standard Time.
        ' *
        ' *             mask = &H800
        ' *
        ' ***************************

        Public Const BinaryLength As Integer = 22

        Private Const FormatType As Short = 1 ' We are using the binary IP buffer format for efficiency
        Private Const QualityMask As Integer = &H1F

        Private Shared PacketFormatType As Byte()

        Shared Sub New()

            ' We pre-load the byte array for the packet format type - we do this at a shared instance level
            ' since this will be the same for all events...
            PacketFormatType = BitConverter.GetBytes(FormatType)

        End Sub

        Public Sub New(ByVal databaseIndex As Integer, ByVal ttag As TimeTag, ByVal value As Single, Optional ByVal qual As DatAWare.Quality = DatAWare.Quality.Good)

            Me.DatabaseIndex = databaseIndex
            Me.TTag = ttag
            Me.Value = value
            QualityBits = -1    ' A quality set to -1 tells Archiver to perform limit checking
            Quality = qual

        End Sub

        Public Sub New(ByVal databaseIndex As Integer, ByVal dtm As DateTime, ByVal value As Single, Optional ByVal valueQuality As DatAWare.Quality = DatAWare.Quality.Good)

            Me.New(databaseIndex, New TimeTag(dtm), value, valueQuality)

        End Sub

        Public Property Quality() As DatAWare.Quality
            Get
                Return (QualityBits And QualityMask)
            End Get
            Set(ByVal Value As DatAWare.Quality)
                QualityBits = (QualityBits Or Value)
            End Set
        End Property

        Public ReadOnly Property BinaryValue() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                ' Construct the binary IP buffer for this event
                Array.Copy(PacketFormatType, 0, buffer, 0, 2)
                Array.Copy(BitConverter.GetBytes(DatabaseIndex), 0, buffer, 2, 4)
                Array.Copy(BitConverter.GetBytes(TTag.Value), 0, buffer, 6, 8)
                Array.Copy(BitConverter.GetBytes(QualityBits), 0, buffer, 14, 4)
                Array.Copy(BitConverter.GetBytes(Value), 0, buffer, 18, 4)

                Return buffer
            End Get
        End Property

    End Class

#End Region

End Namespace