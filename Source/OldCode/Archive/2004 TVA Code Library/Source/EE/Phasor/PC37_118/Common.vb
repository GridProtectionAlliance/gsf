'*******************************************************************************************************
'  Common.vb - Common IEEE C37.118 declarations and functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Shared.Bit
Imports TVA.Interop

Namespace EE.Phasor.IEEEC37_118

    Public Enum RevisionNumber As Byte
        RevisionD6 = 0                   ' Draft 6
        RevisionV1 = 1                   ' Draft 9 / Version 1.0
    End Enum

    <Flags()> _
    Public Enum FormatFlags As Short
        Frequency = Bit3                ' Set = float, Clear = integer
        Analog = Bit2                   ' Set = float, Clear = integer
        Phasors = Bit1                  ' Set = float, Clear = integer
        Coordinates = Bit0              ' Set = polar, Clear = rectangular
        UnusedMask = 65535 And Not (Bit0 Or Bit1 Or Bit2 Or Bit3)
    End Enum

    <Flags()> _
    Public Enum TimeQualityFlags As Byte
        Reserved = Bit7                 ' Reserved
        LeapSecondDirection = Bit6      ' Leap second direction – 0 for add, 1 for delete
        LeapSecondOccurred = Bit5       ' Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours
        LeapSecondPending = Bit4        ' Leap Second Pending – set before a leap second occurs and cleared in the second after the leap second occurs
        ClockQualityMask = Bit3 Or Bit2 Or Bit1 Or Bit0
    End Enum

    Public Enum ClockQuality As Byte
        Failure = Bit3 Or Bit2 Or Bit1 Or Bit0              ' 1111	F	Fault--clock failure, time not reliable
        UnlockedWithin10Seconds = Bit3 Or Bit1 Or Bit0      ' 1011	B	Clock unlocked, time within 10^1 s
        UnlockedWithin1Second = Bit3 Or Bit1                ' 1010	A	Clock unlocked, time within 10^0 s
        UnlockedWithin10_1Seconds = Bit3 Or Bit0            ' 1001	9	Clock unlocked, time within 10^-1 s
        UnlockedWithin10_2Seconds = Bit3                    ' 1000	8	Clock unlocked, time within 10^-2 s
        UnlockedWithin10_3Seconds = Bit2 Or Bit1 Or Bit0    ' 0111	7	Clock unlocked, time within 10^-3 s
        UnlockedWithin10_4Seconds = Bit2 Or Bit1            ' 0110	6	Clock unlocked, time within 10^-4 s
        UnlockedWithin10_5Seconds = Bit2 Or Bit0            ' 0101	5	Clock unlocked, time within 10^-5 s
        UnlockedWithin10_6Seconds = Bit2                    ' 0100	4	Clock unlocked, time within 10^-6 s
        UnlockedWithin10_7Seconds = Bit1 Or Bit0            ' 0011	3	Clock unlocked, time within 10^-7 s
        UnlockedWithin10_8Seconds = Bit1                    ' 0010	2	Clock unlocked, time within 10^-8 s
        UnlockedWithin10_9Seconds = Bit0                    ' 0001	1	Clock unlocked, time within 10^-9 s
        Normal = 0                                          ' 0000	0	Normal operation, clock locked
    End Enum

    Public Enum FrameType As Short
        DataFrame = 0
        HeaderFrame = Bit4
        ConfigurationFrame1 = Bit5
        ConfigurationFrame2 = Bit4 Or Bit5
        CommandFrame = Bit6
        Reserved = Bit7
        VersionNumberMask = Bit0 Or Bit1 Or Bit2 Or Bit3
    End Enum

    Public Enum Command As Short
        DisableRealTimeData = Bit0
        EnableRealTimeData = Bit1
        SendHeaderFile = Bit0 Or Bit1
        SendConfigFile1 = Bit2
        SendConfigFile2 = Bit0 Or Bit2
        ExtendedFrame = Bit3
        ReservedBits = (65535 And Not (Bit0 Or Bit1 Or Bit2 Or Bit3))
    End Enum

    Public Enum PMUTriggerStatus As Short
        None = 0
        FrequencyTrigger = Bit11 Or Bit12 Or Bit13
        DfDtTrigger = Bit12 Or Bit13
        AngleTrigger = Bit11 Or Bit13
        OverCurrentTrigger = Bit13
        UnderVoltageTrigger = Bit11 Or Bit12
        RateTrigger = Bit12
        UserDefined = Bit11
    End Enum

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Const SyncByte As Byte = &HAA
        Public Const DescriptorPacketFlag As Byte = &H0

        Public Const MaximumPhasorValues As Integer = Int16.MaxValue
        Public Const MaximumAnalogValues As Integer = Int16.MaxValue
        Public Const MaximumDigitalValues As Integer = Int16.MaxValue

    End Class

    ' This class generates and parses a frame header specfic to C37.118
    Public Class FrameHeader

        Private m_frameType As FrameType
        Private m_version As Byte
        Private m_soc As Int32
        Private m_fractionSec As Int32
        Private m_frameLength As Int16
        Private m_idCode As Int16
        Private m_timeQuality As Byte

        Public Const BinaryLength As Int16 = 20

        Public Sub New(ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            If binaryImage(startIndex) <> Common.SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in C37.118 frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
            End If

            ' Strip out version information from frame type...
            m_frameType = (binaryImage(startIndex + 1) And Not FrameType.VersionNumberMask)
            m_version = (binaryImage(startIndex + 1) And FrameType.VersionNumberMask)
            m_frameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            m_idCode = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4)
            m_soc = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 6)
            m_fractionSec = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 10)
            m_timeQuality = binaryImage(startIndex + 10)

        End Sub

        Public Property FrameType() As FrameType
            Get
                Return m_frameType
            End Get
            Set(ByVal Value As FrameType)
                m_frameType = Value
            End Set
        End Property

        Public Property Version() As Byte
            Get
                Return m_version
            End Get
            Set(ByVal Value As Byte)
                m_version = Value
            End Set
        End Property

        Public Property FrameLength() As Int16
            Get
                Return m_frameLength
            End Get
            Set(ByVal Value As Int16)
                m_frameLength = Value
            End Set
        End Property

        Public Property IDCode() As Int16
            Get
                Return m_idCode
            End Get
            Set(ByVal Value As Int16)
                m_idCode = Value
            End Set
        End Property

        Public Property SOC() As Int32
            Get
                Return m_soc
            End Get
            Set(ByVal Value As Int32)
                m_soc = Value
            End Set
        End Property

        Public Property SecondFraction() As Int32
            Get
                Return m_fractionSec
            End Get
            Set(ByVal Value As Int32)
                m_fractionSec = Value
            End Set
        End Property

        Public Property TimeQuality() As Byte
            Get
                Return m_timeQuality
            End Get
            Set(ByVal Value As Byte)
                m_timeQuality = Value
            End Set
        End Property

    End Class

End Namespace