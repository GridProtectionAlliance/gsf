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

Imports Tva.DateTime
Imports Tva.Interop
Imports Tva.Interop.Bit

Namespace IeeeC37_118

    ''' <summary>Protocol revision number</summary>
    Public Enum RevisionNumber As Byte
        ''' <summary>Draft 6</summary>
        RevisionD6 = 0
        ''' <summary>Version 1.0</summary>
        RevisionV1 = 1
    End Enum

    ''' <summary>Data format flags</summary>
    <Flags()> _
    Public Enum FormatFlags As Short
        ''' <summary>Frequency value format (Set = float, Clear = integer)</summary>
        Frequency = Bit3
        ''' <summary>Analog value format (Set = float, Clear = integer)</summary>
        Analog = Bit2
        ''' <summary>Phasor value format (Set = float, Clear = integer)</summary>
        Phasors = Bit1
        ''' <summary>Phasor coordinate format (Set = polar, Clear = rectangular)</summary>
        Coordinates = Bit0
        ''' <summary>Unsed format bits mask</summary>
        UnusedMask = Int16.MaxValue And Not (Bit0 Or Bit1 Or Bit2 Or Bit3)
    End Enum

    ''' <summary>Time quality flags</summary>
    <Flags()> _
    Public Enum TimeQualityFlags As Integer
        ''' <summary>Reserved</summary>
        Reserved = Bit31
        ''' <summary>Leap second direction – 0 for add, 1 for delete</summary>
        LeapSecondDirection = Bit30
        ''' <summary>Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours</summary>
        LeapSecondOccurred = Bit29
        ''' <summary>Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs</summary>
        LeapSecondPending = Bit28
        ''' <summary>Time quality indicator mask</summary>
        TimeQualityIndicatorMask = Bit27 Or Bit26 Or Bit25 Or Bit24
    End Enum

    ''' <summary>Time quality indicator code</summary>
    Public Enum TimeQualityIndicatorCode As Integer
        ''' <summary>1111	F	Fault--clock failure, time not reliable</summary>
        Failure = Bit27 Or Bit26 Or Bit25 Or Bit24
        ''' <summary>1011	B	Clock unlocked, time within 10^1 s</summary>
        UnlockedWithin10Seconds = Bit27 Or Bit25 Or Bit24
        ''' <summary>1010	A	Clock unlocked, time within 10^0 s</summary>
        UnlockedWithin1Second = Bit27 Or Bit25
        ''' <summary>1001	9	Clock unlocked, time within 10^-1 s</summary>
        UnlockedWithinPoint1Seconds = Bit27 Or Bit24
        ''' <summary>1000	8	Clock unlocked, time within 10^-2 s</summary>
        UnlockedWithinPoint01Seconds = Bit27
        ''' <summary>0111	7	Clock unlocked, time within 10^-3 s</summary>
        UnlockedWithinPoint001Seconds = Bit26 Or Bit25 Or Bit24
        ''' <summary>0110	6	Clock unlocked, time within 10^-4 s</summary>
        UnlockedWithinPoint0001Seconds = Bit26 Or Bit25
        ''' <summary>0101	5	Clock unlocked, time within 10^-5 s</summary>
        UnlockedWithinPoint00001Seconds = Bit26 Or Bit24
        ''' <summary>0100	4	Clock unlocked, time within 10^-6 s</summary>
        UnlockedWithinPoint000001Seconds = Bit26
        ''' <summary>0011	3	Clock unlocked, time within 10^-7 s</summary>
        UnlockedWithinPoint0000001Seconds = Bit25 Or Bit24
        ''' <summary>0010	2	Clock unlocked, time within 10^-8 s</summary>
        UnlockedWithinPoint00000001Seconds = Bit25
        ''' <summary>0001	1	Clock unlocked, time within 10^-9 s</summary>
        UnlockedWithinPoint000000001Seconds = Bit24
        ''' <summary>0000	0	Normal operation, clock locked</summary>
        Normal = 0
    End Enum

    ''' <summary>Frame type</summary>
    Public Enum FrameType As Short
        ''' <summary>000   Data frame</summary>
        DataFrame = 0
        ''' <summary>001   Header frame</summary>
        HeaderFrame = Bit4
        ''' <summary>010   Configuration frame 1</summary>
        ConfigurationFrame1 = Bit5
        ''' <summary>011   Configuration frame 2</summary>
        ConfigurationFrame2 = Bit4 Or Bit5
        ''' <summary>100   Command frame</summary>
        CommandFrame = Bit6
        ''' <summary>Reserved bit</summary>
        Reserved = Bit7
        ''' <summary>Version number mask</summary>
        VersionNumberMask = Bit0 Or Bit1 Or Bit2 Or Bit3
    End Enum

    ''' <summary>PMU commands</summary>
    Public Enum Command As Short
        ''' <summary>0001  Turn off transmission of data frames</summary>
        DisableRealTimeData = Bit0
        ''' <summary>0010  Turn on transmission of data frames</summary>
        EnableRealTimeData = Bit1
        ''' <summary>0011  Send header file</summary>
        SendHeaderFile = Bit0 Or Bit1
        ''' <summary>0100  Send configuration file 1</summary>
        SendConfigFile1 = Bit2
        ''' <summary>0101  Send configuration file 2</summary>
        SendConfigFile2 = Bit0 Or Bit2
        ''' <summary>1000  Send extended frame</summary>
        ExtendedFrame = Bit3
        ''' <summary>Reserved bits</summary>
        ReservedBits = Int16.MaxValue And Not (Bit0 Or Bit1 Or Bit2 Or Bit3)
    End Enum

    ''' <summary>Status flags</summary>
    <Flags()> _
    Public Enum StatusFlags As Short
        ''' <summary>Data is valid (0 when PMU data is valid, 1 when invalid or PMU is in test mode)</summary>
        DataIsValid = Bit15
        ''' <summary>PMU error including configuration error, 0 when no error</summary>
        PmuError = Bit14
        ''' <summary>PMU synchronization error, 0 when in sync</summary>
        PmuSynchronizationError = Bit13
        ''' <summary>Data sorting type, 0 by timestamp, 1 by arrival</summary>
        DataSortingType = Bit12
        ''' <summary>PMU trigger detected, 0 when no trigger</summary>
        PmuTriggerDetected = Bit11
        ''' <summary>Configuration changed, set to 1 for one minute when configuration changed</summary>
        ConfigurationChanged = Bit10
        ''' <summary>Reserved bits for security, presently set to 0</summary>
        ReservedFlags = Bit9 Or Bit8 Or Bit7 Or Bit6
        ''' <summary>Unlocked time mask</summary>
        UnlockedTimeMask = Bit5 Or Bit4
        ''' <summary>Trigger reason mask</summary>
        TriggerReasonMask = Bit3 Or Bit2 Or Bit1 Or Bit0
    End Enum

    ''' <summary>Unlocked time</summary>
    Public Enum UnlockedTime As Byte
        ''' <summary>Sync locked, best quality</summary>
        SyncLocked = 0
        ''' <summary>Unlocked for 10 seconds</summary>
        UnlockedFor10Seconds = Bit4
        ''' <summary>Unlocked for 100 seconds</summary>
        UnlockedFor100Seconds = Bit5
        ''' <summary>Unlocked for over 1000 seconds</summary>
        UnlockedForOver1000Seconds = Bit5 Or Bit4
    End Enum

    ''' <summary>Trigger reason</summary>
    Public Enum TriggerReason As Byte
        ''' <summary>1111 Vendor defined trigger 7</summary>
        VendorDefinedTrigger8 = Bit3 Or Bit2 Or Bit1 Or Bit0
        ''' <summary>1110 Vendor defined trigger 7</summary>
        VendorDefinedTrigger7 = Bit3 Or Bit2 Or Bit1
        ''' <summary>1101 Vendor defined trigger 6</summary>
        VendorDefinedTrigger6 = Bit3 Or Bit2 Or Bit0
        ''' <summary>1100 Vendor defined trigger 5</summary>
        VendorDefinedTrigger5 = Bit3 Or Bit2
        ''' <summary>1011 Vendor defined trigger 4</summary>
        VendorDefinedTrigger4 = Bit3 Or Bit1 Or Bit0
        ''' <summary>1010 Vendor defined trigger 3</summary>
        VendorDefinedTrigger3 = Bit3 Or Bit1
        ''' <summary>1001 Vendor defined trigger 2</summary>
        VendorDefinedTrigger2 = Bit3 Or Bit0
        ''' <summary>1000 Vendor defined trigger 1</summary>
        VendorDefinedTrigger1 = Bit3
        ''' <summary>0111 Digital</summary>
        Digital = Bit2 Or Bit1 Or Bit0
        ''' <summary>0101 df/dt high</summary>
        DfDtHigh = Bit2 Or Bit0
        ''' <summary>0011 Phase angle difference</summary>
        PhaseAngleDifference = Bit1 Or Bit0
        ''' <summary>0001 Magnitude low</summary>
        MagnitudeLow = Bit0
        ''' <summary>0110 Reserved</summary>
        Reserved = Bit2 Or Bit1
        ''' <summary>0100 Frequency high/low</summary>
        FrequencyHighOrLow = Bit2
        ''' <summary>0010 Magnitude high</summary>
        MagnitudeHigh = Bit1
        ''' <summary>0000 Manual</summary>
        Manual = 0
    End Enum

    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Const SyncByte As Byte = &HAA

        Public Const MaximumPhasorValues As Integer = Int16.MaxValue
        Public Const MaximumAnalogValues As Integer = Int16.MaxValue
        Public Const MaximumDigitalValues As Integer = Int16.MaxValue

        Public Const TimeQualityFlagsMask As Integer = Bit31 Or Bit30 Or Bit29 Or Bit28 Or Bit27 Or Bit26 Or Bit25 Or Bit24

    End Class

    ' This class generates and parses a frame header specfic to C37.118
    <CLSCompliant(False)> _
    Public Class FrameHeader

        Implements IChannel

        Private m_frameType As FrameType
        Private m_version As Byte
        Private m_frameLength As Int16
        Private m_idCode As UInt16
        Private m_secondOfCentury As UInt32
        Private m_fractionOfSecond As Int32
        Private m_timeQualityFlags As TimeQualityFlags
        Private m_timeBase As Int32 = 10000

        Public Const BinaryLength As Int16 = 14

        Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            ParseBinaryImage(Nothing, binaryImage, startIndex)

        End Sub

        Public Sub ParseBinaryImage(ByVal state As IChannelParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer) Implements IChannel.ParseBinaryImage

            Dim fractionOfSecond As Int32

            If binaryImage(startIndex) <> Common.SyncByte Then Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in C37.118 frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))

            ' Strip out frame type and version information...
            m_frameType = (binaryImage(startIndex + 1) And Not FrameType.VersionNumberMask)
            m_version = (binaryImage(startIndex + 1) And FrameType.VersionNumberMask)

            m_frameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            m_idCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4)
            m_secondOfCentury = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 6)

            ' Extract fraction of second and time quality flags
            fractionOfSecond = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 10)
            m_fractionOfSecond = fractionOfSecond And Not Common.TimeQualityFlagsMask
            m_timeQualityFlags = fractionOfSecond And Common.TimeQualityFlagsMask

        End Sub

        Public ReadOnly Property BinaryImage() As Byte() Implements IChannel.BinaryImage
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

                buffer(0) = Common.SyncByte
                buffer(1) = m_frameType Or m_version
                EndianOrder.BigEndian.CopyBytes(m_frameLength, buffer, 2)
                EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 4)
                EndianOrder.BigEndian.CopyBytes(m_secondOfCentury, buffer, 6)
                EndianOrder.BigEndian.CopyBytes(m_fractionOfSecond Or m_timeQualityFlags, buffer, 10)

                Return buffer
            End Get
        End Property

        Public ReadOnly Property InheritedType() As System.Type Implements IChannel.InheritedType
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overridable ReadOnly Property This() As IChannel Implements IChannel.This
            Get
                Return Me
            End Get
        End Property

        Public Property FrameType() As FrameType
            Get
                Return m_frameType
            End Get
            Set(ByVal value As FrameType)
                m_frameType = value
            End Set
        End Property

        Public Property Version() As Byte
            Get
                Return m_version
            End Get
            Set(ByVal value As Byte)
                m_version = value And FrameType.VersionNumberMask
            End Set
        End Property

        Public Property FrameLength() As Int16
            Get
                Return m_frameLength
            End Get
            Set(ByVal value As Int16)
                m_frameLength = value
            End Set
        End Property

        Public Property IDCode() As UInt16
            Get
                Return m_idCode
            End Get
            Set(ByVal value As UInt16)
                m_idCode = value
            End Set
        End Property

        Public Property TimeTag() As UnixTimeTag
            Get
                Return New UnixTimeTag(m_secondOfCentury + m_fractionOfSecond / m_timeBase)
            End Get
            Set(ByVal value As UnixTimeTag)
                m_secondOfCentury = System.Math.Floor(value.Value)
                m_fractionOfSecond = (value.Value - m_secondOfCentury) * m_timeBase
            End Set
        End Property

        Public Property SecondOfCentury() As UInt32
            Get
                Return m_secondOfCentury
            End Get
            Set(ByVal value As UInt32)
                m_secondOfCentury = value
            End Set
        End Property

        Public Property FractionOfSecond() As Int32
            Get
                Return m_fractionOfSecond
            End Get
            Set(ByVal value As Int32)
                m_fractionOfSecond = value
            End Set
        End Property

        Public Property TimeBase() As Int32
            Get
                Return m_timeBase
            End Get
            Set(ByVal value As Int32)
                m_timeBase = value
            End Set
        End Property

        Public Property TimeQualityFlags() As TimeQualityFlags
            Get
                Return m_timeQualityFlags And Not TimeQualityFlags.TimeQualityIndicatorMask
            End Get
            Set(ByVal value As TimeQualityFlags)
                m_timeQualityFlags = (m_timeQualityFlags And TimeQualityFlags.TimeQualityIndicatorMask) Or value
            End Set
        End Property

        Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode
            Get
                Return m_timeQualityFlags And TimeQualityFlags.TimeQualityIndicatorMask
            End Get
            Set(ByVal value As TimeQualityIndicatorCode)
                m_timeQualityFlags = (m_timeQualityFlags And Not TimeQualityFlags.TimeQualityIndicatorMask) Or value
            End Set
        End Property

        Private ReadOnly Property IChannelBinaryLength() As Short Implements IChannel.BinaryLength
            Get
                Return BinaryLength
            End Get
        End Property

    End Class

End Namespace