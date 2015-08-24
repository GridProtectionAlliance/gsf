//*******************************************************************************************************
//  Common.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.PhasorProtocols.IeeeC37_118
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE C37.118 frame types enumeration.
    /// </summary>
    [Serializable()]
    public enum FrameType : ushort
    {
        /// <summary>
        /// 000 Data frame.
        /// </summary>
        DataFrame = (ushort)Bits.Nil,
        /// <summary>
        /// 001 Header frame.
        /// </summary>
        HeaderFrame = (ushort)Bits.Bit04,
        /// <summary>
        /// 010 Configuration frame 1.
        /// </summary>
        ConfigurationFrame1 = (ushort)Bits.Bit05,
        /// <summary>
        /// 011 Configuration frame 2.
        /// </summary>
        ConfigurationFrame2 = (ushort)(Bits.Bit04 | Bits.Bit05),
        /// <summary>
        /// 100 Command frame.
        /// </summary>
        CommandFrame = (ushort)Bits.Bit06,
        /// <summary>.
        /// Reserved bit.
        /// </summary>
        Reserved = (ushort)Bits.Bit07,
        /// <summary>
        /// Version number mask.
        /// </summary>
        VersionNumberMask = (ushort)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)
    }

    /// <summary>
    /// Protocol draft revision numbers enumeration.
    /// </summary>
    [Serializable()]
    public enum DraftRevision : byte
    {
        /// <summary>
        /// Draft 6.0.
        /// </summary>
        Draft6 = 0,
        /// <summary>
        /// Draft 7.0 (Version 1.0).
        /// </summary>
        Draft7 = 1
    }

    /// <summary>
    /// Data format flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum FormatFlags : ushort
    {
        /// <summary>
        /// Frequency value format (Set = float, Clear = integer).
        /// </summary>
        Frequency = (ushort)Bits.Bit03,
        /// <summary>
        /// Analog value format (Set = float, Clear = integer).
        /// </summary>
        Analog = (ushort)Bits.Bit02,
        /// <summary>
        /// Phasor value format (Set = float, Clear = integer).
        /// </summary>
        Phasors = (ushort)Bits.Bit01,
        /// <summary>
        /// Phasor coordinate format (Set = polar, Clear = rectangular).
        /// </summary>
        Coordinates = (ushort)Bits.Bit00,
        /// <summary>
        /// Unsed format bits mask.
        /// </summary>
        UnusedMask = unchecked(ushort.MaxValue & (ushort)~(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Time quality flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum TimeQualityFlags : uint
    {
        /// <summary>
        /// Reserved bit.
        /// </summary>
        Reserved = (uint)Bits.Bit31,
        /// <summary>
        /// Leap second direction – 0 for add, 1 for delete.
        /// </summary>
        LeapSecondDirection = (uint)Bits.Bit30,
        /// <summary>
        /// Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours.
        /// </summary>
        LeapSecondOccurred = (uint)Bits.Bit29,
        /// <summary>
        /// Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs.
        /// </summary>
        LeapSecondPending = (uint)Bits.Bit28,
        /// <summary>
        /// Time quality indicator code mask.
        /// </summary>
        TimeQualityIndicatorCodeMask = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (uint)Bits.Nil
    }

    /// <summary>
    /// Time quality indicator code enumeration.
    /// </summary>
    [Serializable()]
    public enum TimeQualityIndicatorCode : uint
    {
        /// <summary>
        /// 1111 - 0xF:	Fault--clock failure, time not reliable.
        /// </summary>
        Failure = (uint)(Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1011 - 0xB:	Clock unlocked, time within 10^1 s.
        /// </summary>
        UnlockedWithin10Seconds = (uint)(Bits.Bit27 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 1010 - 0xA:	Clock unlocked, time within 10^0 s.
        /// </summary>
        UnlockedWithin1Second = (uint)(Bits.Bit27 | Bits.Bit25),
        /// <summary>
        /// 1001 - 0x9: Clock unlocked, time within 10^-1 s.
        /// </summary>
        UnlockedWithinPoint1Seconds = (uint)(Bits.Bit27 | Bits.Bit24),
        /// <summary>
        /// 1000 - 0x8: Clock unlocked, time within 10^-2 s.
        /// </summary>
        UnlockedWithinPoint01Seconds = (uint)Bits.Bit27,
        /// <summary>
        /// 0111 - 0x7: Clock unlocked, time within 10^-3 s.
        /// </summary>
        UnlockedWithinPoint001Seconds = (uint)(Bits.Bit26 | Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0110 - 0x6: Clock unlocked, time within 10^-4 s.
        /// </summary>
        UnlockedWithinPoint0001Seconds = (uint)(Bits.Bit26 | Bits.Bit25),
        /// <summary>
        /// 0101 - 0x5: Clock unlocked, time within 10^-5 s.
        /// </summary>
        UnlockedWithinPoint00001Seconds = (uint)(Bits.Bit26 | Bits.Bit24),
        /// <summary>
        /// 0100 - 0x4: Clock unlocked, time within 10^-6 s.
        /// </summary>
        UnlockedWithinPoint000001Seconds = (uint)Bits.Bit26,
        /// <summary>
        /// 0011 - 0x3: Clock unlocked, time within 10^-7 s.
        /// </summary>
        UnlockedWithinPoint0000001Seconds = (uint)(Bits.Bit25 | Bits.Bit24),
        /// <summary>
        /// 0010 - 0x2: Clock unlocked, time within 10^-8 s.
        /// </summary>
        UnlockedWithinPoint00000001Seconds = (uint)Bits.Bit25,
        /// <summary>
        /// 0001 - 0x1: Clock unlocked, time within 10^-9 s.
        /// </summary>
        UnlockedWithinPoint000000001Seconds = (uint)Bits.Bit24,
        /// <summary>
        /// 0000 - 0x0: Normal operation, clock locked.
        /// </summary>
        Locked = 0
    }

    /// <summary>
    /// Status flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum StatusFlags : ushort
    {
        /// <summary>
        /// Data is valid (0 when device data is valid, 1 when invalid or device is in test mode).
        /// </summary>
        DataIsValid = (ushort)Bits.Bit15,
        /// <summary>
        /// Device error including configuration error, 0 when no error.
        /// </summary>
        DeviceError = (ushort)Bits.Bit14,
        /// <summary>
        /// Device synchronization error, 0 when in sync.
        /// </summary>
        DeviceSynchronizationError = (ushort)Bits.Bit13,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = (ushort)Bits.Bit12,
        /// <summary>
        /// Device trigger detected, 0 when no trigger.
        /// </summary>
        DeviceTriggerDetected = (ushort)Bits.Bit11,
        /// <summary>
        /// Configuration changed, set to 1 for one minute when configuration changed.
        /// </summary>
        ConfigurationChanged = (ushort)Bits.Bit10,
        /// <summary>
        /// Reserved bits for security, presently set to 0.
        /// </summary>
        ReservedFlags = (ushort)(Bits.Bit09 | Bits.Bit08 | Bits.Bit07 | Bits.Bit06),
        /// <summary>
        /// Unlocked time mask.
        /// </summary>
        UnlockedTimeMask = (ushort)(Bits.Bit05 | Bits.Bit04),
        /// <summary>
        /// Trigger reason mask.
        /// </summary>
        TriggerReasonMask = (ushort)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// Unlocked time enumeration.
    /// </summary>
    [Serializable()]
    public enum UnlockedTime : byte
    {
        /// <summary>
        /// Sync locked, best quality.
        /// </summary>
        SyncLocked = (byte)Bits.Nil,
        /// <summary>
        /// Unlocked for 10 seconds.
        /// </summary>
        UnlockedFor10Seconds = (byte)Bits.Bit04,
        /// <summary>
        /// Unlocked for 100 seconds.
        /// </summary>
        UnlockedFor100Seconds = (byte)Bits.Bit05,
        /// <summary>
        /// Unlocked for over 1000 seconds.
        /// </summary>
        UnlockedForOver1000Seconds = (byte)(Bits.Bit05 | Bits.Bit04)
    }

    /// <summary>
    /// Trigger reason enumeration.
    /// </summary>
    [Serializable()]
    public enum TriggerReason : byte
    {
        /// <summary>
        /// 1111 Vendor defined trigger 8.
        /// </summary>
        VendorDefinedTrigger8 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1110 Vendor defined trigger 7.
        /// </summary>
        VendorDefinedTrigger7 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 1101 Vendor defined trigger 6.
        /// </summary>
        VendorDefinedTrigger6 = (byte)(Bits.Bit03 | Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 1100 Vendor defined trigger 5.
        /// </summary>
        VendorDefinedTrigger5 = (byte)(Bits.Bit03 | Bits.Bit02),
        /// <summary>
        /// 1011 Vendor defined trigger 4.
        /// </summary>
        VendorDefinedTrigger4 = (byte)(Bits.Bit03 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 1010 Vendor defined trigger 3.
        /// </summary>
        VendorDefinedTrigger3 = (byte)(Bits.Bit03 | Bits.Bit01),
        /// <summary>
        /// 1001 Vendor defined trigger 2.
        /// </summary>
        VendorDefinedTrigger2 = (byte)(Bits.Bit03 | Bits.Bit00),
        /// <summary>
        /// 1000 Vendor defined trigger 1.
        /// </summary>
        VendorDefinedTrigger1 = (byte)Bits.Bit03,
        /// <summary>
        /// 0111 Digital.
        /// </summary>
        Digital = (byte)(Bits.Bit02 | Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0101 df/dt high.
        /// </summary>
        DfDtHigh = (byte)(Bits.Bit02 | Bits.Bit00),
        /// <summary>
        /// 0011 Phase angle difference.
        /// </summary>
        PhaseAngleDifference = (byte)(Bits.Bit01 | Bits.Bit00),
        /// <summary>
        /// 0001 Magnitude low.
        /// </summary>
        MagnitudeLow = (byte)Bits.Bit00,
        /// <summary>
        /// 0110 Reserved.
        /// </summary>
        Reserved = (byte)(Bits.Bit02 | Bits.Bit01),
        /// <summary>
        /// 0100 Frequency high/low.
        /// </summary>
        FrequencyHighOrLow = (byte)Bits.Bit02,
        /// <summary>
        /// 0010 Magnitude high.
        /// </summary>
        MagnitudeHigh = (byte)Bits.Bit01,
        /// <summary>
        /// 0000 Manual.
        /// </summary>
        Manual = (byte)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Common IEEE C37.118 declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = (ushort)(ushort.MaxValue / 4 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = (ushort)(ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = (ushort)(ushort.MaxValue / 2 - CommonFrameHeader.FixedLength - 8);

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = (ushort)(ushort.MaxValue - CommonFrameHeader.FixedLength - 2);

        /// <summary>
        /// Absolute maximum number of bytes of extended data that could fit into a command frame.
        /// </summary>
        public const ushort MaximumExtendedDataLength = (ushort)(MaximumDataLength - 2);

        /// <summary>
        /// Time quality flags mask.
        /// </summary>
        public const uint TimeQualityFlagsMask = (uint)(Bits.Bit31 | Bits.Bit30 | Bits.Bit29 | Bits.Bit28 | Bits.Bit27 | Bits.Bit26 | Bits.Bit25 | Bits.Bit24);
    }
}