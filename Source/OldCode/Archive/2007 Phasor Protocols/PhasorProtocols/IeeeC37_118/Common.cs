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

namespace PCS.PhasorProtocols.IeeeC37_118
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE C37.118 frame types enumeration.
    /// </summary>
    [Serializable()]
    public enum FrameType : short
    {
        /// <summary>
        /// 000 Data frame.
        /// </summary>
        DataFrame = Bit.Nill,
        /// <summary>
        /// 001 Header frame.
        /// </summary>
        HeaderFrame = Bit.Bit4,
        /// <summary>
        /// 010 Configuration frame 1.
        /// </summary>
        ConfigurationFrame1 = Bit.Bit5,
        /// <summary>
        /// 011 Configuration frame 2.
        /// </summary>
        ConfigurationFrame2 = Bit.Bit4 | Bit.Bit5,
        /// <summary>
        /// 100 Command frame.
        /// </summary>
        CommandFrame = Bit.Bit6,
        /// <summary>.
        /// Reserved bit.
        /// </summary>
        Reserved = Bit.Bit7,
        /// <summary>
        /// Version number mask.
        /// </summary>
        VersionNumberMask = Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3
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
    public enum FormatFlags : short
    {
        /// <summary>
        /// Frequency value format (Set = float, Clear = integer).
        /// </summary>
        Frequency = Bit.Bit3,
        /// <summary>
        /// Analog value format (Set = float, Clear = integer).
        /// </summary>
        Analog = Bit.Bit2,
        /// <summary>
        /// Phasor value format (Set = float, Clear = integer).
        /// </summary>
        Phasors = Bit.Bit1,
        /// <summary>
        /// Phasor coordinate format (Set = polar, Clear = rectangular).
        /// </summary>
        Coordinates = Bit.Bit0,
        /// <summary>
        /// Unsed format bits mask.
        /// </summary>
        UnusedMask = short.MaxValue & ~(Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3)
    }

    /// <summary>
    /// Time quality flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum TimeQualityFlags : int
    {
        /// <summary>
        /// Reserved bit.
        /// </summary>
        Reserved = Bit.Bit31,
        /// <summary>
        /// Leap second direction – 0 for add, 1 for delete.
        /// </summary>
        LeapSecondDirection = Bit.Bit30,
        /// <summary>
        /// Leap second occurred – set in the first second after the leap second occurs and remains set for 24 hours.
        /// </summary>
        LeapSecondOccurred = Bit.Bit29,
        /// <summary>
        /// Leap second pending – set before a leap second occurs and cleared in the second after the leap second occurs.
        /// </summary>
        LeapSecondPending = Bit.Bit28,
        /// <summary>
        /// Time quality indicator code mask.
        /// </summary>
        TimeQualityIndicatorCodeMask = Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24
    }

    /// <summary>
    /// Time quality indicator code enumeration.
    /// </summary>
    [Serializable()]
    public enum TimeQualityIndicatorCode : int
    {
        /// <summary>
        /// 1111 - 0xF:	Fault--clock failure, time not reliable.
        /// </summary>
        Failure = Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24,
        /// <summary>
        /// 1011 - 0xB:	Clock unlocked, time within 10^1 s.
        /// </summary>
        UnlockedWithin10Seconds = Bit.Bit27 | Bit.Bit25 | Bit.Bit24,
        /// <summary>
        /// 1010 - 0xA:	Clock unlocked, time within 10^0 s.
        /// </summary>
        UnlockedWithin1Second = Bit.Bit27 | Bit.Bit25,
        /// <summary>
        /// 1001 - 0x9: Clock unlocked, time within 10^-1 s.
        /// </summary>
        UnlockedWithinPoint1Seconds = Bit.Bit27 | Bit.Bit24,
        /// <summary>
        /// 1000 - 0x8: Clock unlocked, time within 10^-2 s.
        /// </summary>
        UnlockedWithinPoint01Seconds = Bit.Bit27,
        /// <summary>
        /// 0111 - 0x7: Clock unlocked, time within 10^-3 s.
        /// </summary>
        UnlockedWithinPoint001Seconds = Bit.Bit26 | Bit.Bit25 | Bit.Bit24,
        /// <summary>
        /// 0110 - 0x6: Clock unlocked, time within 10^-4 s.
        /// </summary>
        UnlockedWithinPoint0001Seconds = Bit.Bit26 | Bit.Bit25,
        /// <summary>
        /// 0101 - 0x5: Clock unlocked, time within 10^-5 s.
        /// </summary>
        UnlockedWithinPoint00001Seconds = Bit.Bit26 | Bit.Bit24,
        /// <summary>
        /// 0100 - 0x4: Clock unlocked, time within 10^-6 s.
        /// </summary>
        UnlockedWithinPoint000001Seconds = Bit.Bit26,
        /// <summary>
        /// 0011 - 0x3: Clock unlocked, time within 10^-7 s.
        /// </summary>
        UnlockedWithinPoint0000001Seconds = Bit.Bit25 | Bit.Bit24,
        /// <summary>
        /// 0010 - 0x2: Clock unlocked, time within 10^-8 s.
        /// </summary>
        UnlockedWithinPoint00000001Seconds = Bit.Bit25,
        /// <summary>
        /// 0001 - 0x1: Clock unlocked, time within 10^-9 s.
        /// </summary>
        UnlockedWithinPoint000000001Seconds = Bit.Bit24,
        /// <summary>
        /// 0000 - 0x0: Normal operation, clock locked.
        /// </summary>
        Locked = 0
    }

    /// <summary>
    /// Status flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum StatusFlags : short
    {
        /// <summary>
        /// Data is valid (0 when device data is valid, 1 when invalid or device is in test mode).
        /// </summary>
        DataIsValid = Bit.Bit15,
        /// <summary>
        /// Device error including configuration error, 0 when no error.
        /// </summary>
        DeviceError = Bit.Bit14,
        /// <summary>
        /// Device synchronization error, 0 when in sync.
        /// </summary>
        DeviceSynchronizationError = Bit.Bit13,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = Bit.Bit12,
        /// <summary>
        /// Device trigger detected, 0 when no trigger.
        /// </summary>
        DeviceTriggerDetected = Bit.Bit11,
        /// <summary>
        /// Configuration changed, set to 1 for one minute when configuration changed.
        /// </summary>
        ConfigurationChanged = Bit.Bit10,
        /// <summary>
        /// Reserved bits for security, presently set to 0.
        /// </summary>
        ReservedFlags = Bit.Bit9 | Bit.Bit8 | Bit.Bit7 | Bit.Bit6,
        /// <summary>
        /// Unlocked time mask.
        /// </summary>
        UnlockedTimeMask = Bit.Bit5 | Bit.Bit4,
        /// <summary>
        /// Trigger reason mask.
        /// </summary>
        TriggerReasonMask = Bit.Bit3 | Bit.Bit2 | Bit.Bit1 | Bit.Bit0
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
        SyncLocked = Bit.Nill,
        /// <summary>
        /// Unlocked for 10 seconds.
        /// </summary>
        UnlockedFor10Seconds = Bit.Bit4,
        /// <summary>
        /// Unlocked for 100 seconds.
        /// </summary>
        UnlockedFor100Seconds = Bit.Bit5,
        /// <summary>
        /// Unlocked for over 1000 seconds.
        /// </summary>
        UnlockedForOver1000Seconds = Bit.Bit5 | Bit.Bit4
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
        VendorDefinedTrigger8 = Bit.Bit3 | Bit.Bit2 | Bit.Bit1 | Bit.Bit0,
        /// <summary>
        /// 1110 Vendor defined trigger 7.
        /// </summary>
        VendorDefinedTrigger7 = Bit.Bit3 | Bit.Bit2 | Bit.Bit1,
        /// <summary>
        /// 1101 Vendor defined trigger 6.
        /// </summary>
        VendorDefinedTrigger6 = Bit.Bit3 | Bit.Bit2 | Bit.Bit0,
        /// <summary>
        /// 1100 Vendor defined trigger 5.
        /// </summary>
        VendorDefinedTrigger5 = Bit.Bit3 | Bit.Bit2,
        /// <summary>
        /// 1011 Vendor defined trigger 4.
        /// </summary>
        VendorDefinedTrigger4 = Bit.Bit3 | Bit.Bit1 | Bit.Bit0,
        /// <summary>
        /// 1010 Vendor defined trigger 3.
        /// </summary>
        VendorDefinedTrigger3 = Bit.Bit3 | Bit.Bit1,
        /// <summary>
        /// 1001 Vendor defined trigger 2.
        /// </summary>
        VendorDefinedTrigger2 = Bit.Bit3 | Bit.Bit0,
        /// <summary>
        /// 1000 Vendor defined trigger 1.
        /// </summary>
        VendorDefinedTrigger1 = Bit.Bit3,
        /// <summary>
        /// 0111 Digital.
        /// </summary>
        Digital = Bit.Bit2 | Bit.Bit1 | Bit.Bit0,
        /// <summary>
        /// 0101 df/dt high.
        /// </summary>
        DfDtHigh = Bit.Bit2 | Bit.Bit0,
        /// <summary>
        /// 0011 Phase angle difference.
        /// </summary>
        PhaseAngleDifference = Bit.Bit1 | Bit.Bit0,
        /// <summary>
        /// 0001 Magnitude low.
        /// </summary>
        MagnitudeLow = Bit.Bit0,
        /// <summary>
        /// 0110 Reserved.
        /// </summary>
        Reserved = Bit.Bit2 | Bit.Bit1,
        /// <summary>
        /// 0100 Frequency high/low.
        /// </summary>
        FrequencyHighOrLow = Bit.Bit2,
        /// <summary>
        /// 0010 Magnitude high.
        /// </summary>
        MagnitudeHigh = Bit.Bit1,
        /// <summary>
        /// 0000 Manual.
        /// </summary>
        Manual = Bit.Nill
    }

    /// <summary>
    /// Analog types enumeration.
    /// </summary>
    [Serializable()]
    public enum AnalogType : byte
    {
        /// <summary>
        /// Single point-on-wave.
        /// </summary>
        SinglePointOnWave = 0,
        /// <summary>
        /// RMS of analog input.
        /// </summary>
        RmsOfAnalogInput = 1,
        /// <summary>
        /// Peak of analog input.
        /// </summary>
        PeakOfAnalogInput = 2
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
        public const int MaximumPhasorValues = ushort.MaxValue / 4 - CommonFrameHeader.BinaryLength - 8;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const int MaximumAnalogValues = ushort.MaxValue / 2 - CommonFrameHeader.BinaryLength - 8;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const int MaximumDigitalValues = ushort.MaxValue / 2 - CommonFrameHeader.BinaryLength - 8;

        /// <summary>
        /// Absolute maximum number of bytes of data that could fit into a header frame.
        /// </summary>
        public const int MaximumHeaderDataLength = ushort.MaxValue - CommonFrameHeader.BinaryLength - 2;

        /// <summary>
        /// Absolute maximum number of bytes of extended data that could fit into a command frame.
        /// </summary>
        public const int MaximumExtendedDataLength = ushort.MaxValue - CommonFrameHeader.BinaryLength - 4;

        /// <summary>
        /// Time quality flags mask.
        /// </summary>
        public const int TimeQualityFlagsMask = Bit.Bit31 | Bit.Bit30 | Bit.Bit29 | Bit.Bit28 | Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24;
    }
}