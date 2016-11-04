//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace GSF.PhasorProtocols.IEC61850_90_5
{
    #region [ Enumerations ]

    /// <summary>
    /// IEC 61850-90-5 frame types enumeration.
    /// </summary>
    [Serializable]
    public enum FrameType : ushort
    {
        /// <summary>
        /// 000 Data frame.
        /// </summary>
        DataFrame = (ushort)Bits.Nil,
        /// <summary>
        /// 011 Configuration frame.
        /// </summary>
        ConfigurationFrame = (ushort)(Bits.Bit04 | Bits.Bit05),
        /// <summary>
        /// 100 Command frame.
        /// </summary>
        CommandFrame = (ushort)Bits.Bit06,
        /// <summary>
        /// Version number mask.
        /// </summary>
        VersionNumberMask = (ushort)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03)
    }

    /// <summary>
    /// IEC 61850-90-5 session types.
    /// </summary>
    public enum SessionType : byte
    {
        /// <summary>
        /// Tunnelled values.
        /// </summary>
        Tunnelled = 0xA0,
        /// <summary>
        /// Goose values.
        /// </summary>
        Goose = 0xA1,
        /// <summary>
        /// Sampled values.
        /// </summary>
        SampledValues = 0xA2
    }

    /// <summary>
    /// IEC 61850-90-5 signature algorithm.
    /// </summary>
    public enum SignatureAlgorithm : byte
    {
        /// <summary>
        /// No algorithm used
        /// </summary>
        None,
        /// <summary>
        /// SHA 256/80
        /// </summary>
        Sha80,
        /// <summary>
        /// SHA 256/128
        /// </summary>
        Sha128,
        /// <summary>
        /// SHA 256/256
        /// </summary>
        Sha256,
        /// <summary>
        /// AES GMAC 64
        /// </summary>
        Aes64,
        /// <summary>
        /// AES GMAC 128
        /// </summary>
        Aes128
    }

    /// <summary>
    /// IEC 61850-90-5 security algorithm.
    /// </summary>
    public enum SecurityAlgorithm : byte
    {
        /// <summary>
        /// No algorithm used
        /// </summary>
        None,
        /// <summary>
        /// AES 128
        /// </summary>
        Aes128,
        /// <summary>
        /// AES 256
        /// </summary>
        Aes256
    }

    /// <summary>
    /// Sampled value tags.
    /// </summary>
    public enum SampledValueTag : byte
    {
        /// <summary>
        /// Sampled value protocol data unit.
        /// </summary>
        SvPdu = 0x60,
        /// <summary>
        /// Number of ASDUs. 
        /// </summary>
        AsduCount = 0x80,
        /// <summary>
        /// Sequence of ASDU.
        /// </summary>
        SequenceOfAsdu = 0xA2,
        /// <summary>
        /// ASDU sequence number.
        /// </summary>
        AsduSequence = 0x30,
        /// <summary>
        /// Multicast sampled value identifier.
        /// </summary>
        MsvID = 0x80,
        /// <summary>
        /// Data set.
        /// </summary>
        Dataset = 0x81,
        /// <summary>
        /// Sample Count
        /// </summary>
        SmpCnt = 0x82,
        /// <summary>
        /// Configuration revision.
        /// </summary>
        ConfRev = 0x83,
        /// <summary>
        /// Local refresh time.
        /// </summary>
        RefrTm = 0x84,
        /// <summary>
        /// Samples are synchronized.
        /// </summary>
        SmpSynch = 0x85,
        /// <summary>
        /// Sample rate.
        /// </summary>
        SmpRate = 0x86,
        /// <summary>
        /// Data samples.
        /// </summary>
        Samples = 0x87,
        /// <summary>
        /// Sample mod.
        /// </summary>
        SmpMod = 0x88,
        /// <summary>
        /// UTC Timestamp.
        /// </summary>
        UtcTimestamp = 0x89
    }

    /// <summary>
    /// Protocol draft revision numbers enumeration.
    /// </summary>
    [Serializable]
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
    [Flags, Serializable]
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
    [Flags, Serializable]
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
    [Serializable]
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
    [Flags, Serializable]
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
    [Serializable]
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
    [Serializable]
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
    /// Common IEC 61850-90-5 declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Timebase used by IEC 61850-90-5 protocol implementation.
        /// </summary>
        public const uint Timebase = 16777216;

        /// <summary>
        /// Marker for a connectionless transport protocol tag in IEC 61850-90-5 data frames.
        /// </summary>
        public const byte CltpTag = 0x40;

        /// <summary>
        /// Common session header size.
        /// </summary>
        public const byte SessionHeaderSize = 0x18;

        /// <summary>
        /// Size of keys.
        /// </summary>
        public const int KeySize = 4;

        /// <summary>
        /// Temporary key used by IEC 61850-90-5 draft implementations.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static byte[] DummyKey = 
        {
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88,
            0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x01, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x66, 0x77, 0x88
        };

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

        /// <summary>
        /// Validates sample value tag exists and skips past it.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value tag length.</param>
        /// <param name="tag">Sampled value tag to validate.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static int ValidateTag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            return buffer.ParseTagLength(ref index);
        }

        /// <summary>
        /// Validates and parses byte length sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static byte ParseByteTag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            if (tagLength < 1)
                throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", tag, tagLength));

            byte result = buffer[index];
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Validates and parses 2-byte length sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static ushort ParseUInt16Tag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            if (tagLength < 2)
                throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", tag, tagLength));

            ushort result = BigEndian.ToUInt16(buffer, index);
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Validates and parses 3-byte length sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static UInt24 ParseUInt24Tag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            if (tagLength < 3)
                throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", tag, tagLength));

            UInt24 result = BigEndian.ToUInt24(buffer, index);
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Validates and parses 4-byte length sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static uint ParseUInt32Tag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            if (tagLength < 4)
                throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", tag, tagLength));

            uint result = BigEndian.ToUInt32(buffer, index);
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Validates and parses 8-byte length sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static ulong ParseUInt64Tag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            if (tagLength < 8)
                throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", tag, tagLength));

            ulong result = BigEndian.ToUInt64(buffer, index);
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Validates and parses string sample value tag.
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value.</param>
        /// <param name="tag">Sampled value tag to parse.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        public static string ParseStringTag(this byte[] buffer, SampledValueTag tag, ref int index)
        {
            if ((SampledValueTag)buffer[index] != tag)
                throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[index].ToString("X").PadLeft(2, '0'));

            index++;
            int tagLength = buffer.ParseTagLength(ref index);

            string result = Encoding.ASCII.GetString(buffer, index, tagLength);
            index += tagLength;

            return result;
        }

        /// <summary>
        /// Encodes sampled value tag with only a 16-bit length.
        /// </summary>
        /// <param name="length">Value to encode.</param>
        /// <param name="tag">Sampled value tag to encode.</param>
        /// <param name="buffer">Buffer to hold encoded sampled value.</param>
        /// <param name="index">Start index of buffer where tag will begin - will be auto-incremented.</param>
        public static void EncodeTagLength(this ushort length, SampledValueTag tag, byte[] buffer, ref int index)
        {
            buffer[index++] = (byte)tag;
            buffer[index++] = 0x80 | 2;
            buffer[index++] = (byte)((length & 0xFF00) >> 8);
            buffer[index++] = (byte)(length & 0x00FF);
        }
        /// <summary>
        /// Encodes primitive type sampled value tag.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <param name="tag">Sampled value tag to encode.</param>
        /// <param name="buffer">Buffer to hold encoded sampled value.</param>
        /// <param name="index">Start index of buffer where tag will begin - will be auto-incremented.</param>
        public static void EncodeTagValue<T>(this T value, SampledValueTag tag, byte[] buffer, ref int index) where T : struct, IConvertible
        {
            if (!typeof(T).IsPrimitive)
                throw new ArgumentException("Value type is not primitive", "value");

            // Not sure if booleans would be encoded correctly here (due to Marshal sizeof) - also not sure
            // how IEC 61850 deals with booleans - as a result, booleans should likely be avoided.
            // I wonder if compiler is smart enough to exclude this expression in imlementations since this
            // is always false for non boolean types - where is my WHERE expression like "~bool"...
            if (typeof(T) == typeof(bool))
                throw new ArgumentOutOfRangeException("value", "Boolean encoding is currently not supported");

            ushort length = (ushort)Marshal.SizeOf(typeof(T));

            buffer[index++] = (byte)tag;
            length.EncodeTagLength(buffer, ref index);
            index += BigEndian.CopyBytes(value, buffer, index);
        }

        /// <summary>
        /// Encodes byte based sampled value tag.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <param name="tag">Sampled value tag to encode.</param>
        /// <param name="buffer">Buffer to hold encoded sampled value.</param>
        /// <param name="index">Start index of buffer where tag will begin - will be auto-incremented.</param>
        public static void EncodeTagValue(this byte value, SampledValueTag tag, byte[] buffer, ref int index)
        {
            const ushort length = 1;
            buffer[index++] = (byte)tag;
            length.EncodeTagLength(buffer, ref index);
            buffer[index++] = value;
        }

        /// <summary>
        /// Encodes string based sampled value tag.
        /// </summary>
        /// <param name="value">String to encode - null string will be encoded as empty string.</param>
        /// <param name="tag">Sampled value tag to encode.</param>
        /// <param name="buffer">Buffer to hold encoded sampled value.</param>
        /// <param name="index">Start index of buffer where tag will begin - will be auto-incremented.</param>
        public static void EncodeTagValue(this string value, SampledValueTag tag, byte[] buffer, ref int index)
        {
            if ((object)value == null)
                value = "";

            if (value.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("value", "Current implementation will not encode a string larger than " + ushort.MaxValue);

            ushort length = (ushort)value.Length;

            buffer[index++] = (byte)tag;
            length.EncodeTagLength(buffer, ref index);

            if (length > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value);
                Buffer.BlockCopy(bytes, 0, buffer, index, bytes.Length);
                index += bytes.Length;
            }
        }

        /// <summary>
        /// Gets decoded sample value tag length (currently limited to 16-bits).
        /// </summary>
        /// <param name="buffer">Buffer containing sampled value tag length.</param>
        /// <param name="index">Start index of buffer where tag length begins - will be auto-incremented.</param>
        /// <returns>Decoded sample value tag length.</returns>
        public static int ParseTagLength(this byte[] buffer, ref int index)
        {
            // See if high bit is set
            if ((buffer[index] & (byte)Bits.Bit07) > 0)
            {
                // Odd attempt at 7-bit encoding? Seems like a waste of bits for the
                // benefit of allowing variable length encoded 56-bit integers...
                switch (buffer[index++] & 0x7F)
                {
                    case 1:
                        return buffer[index++];
                    case 2:
                        return ((buffer[index++] & 0xFF) << 8) | (buffer[index++] & 0xFF);
                    default:
                        return 0;
                }
            }

            return buffer[index++];
        }

        /// <summary>
        /// Encodes sample value tag length (currently limited to 16-bits).
        /// </summary>
        /// <param name="length">Sample value tag length.</param>
        /// <param name="buffer">Buffer to hold encoded sampled value tag length.</param>
        /// <param name="index">Start index of buffer where tag length encoding begins - will be auto-incremented.</param>
        public static void EncodeTagLength(this ushort length, byte[] buffer, ref int index)
        {
            if (length > 0x7F)
            {
                if (length > 0xFF)
                {
                    // 16-bit length value
                    buffer[index++] = 0x80 | 2;
                    buffer[index++] = (byte)((length & 0xFF00) >> 8);
                    buffer[index++] = (byte)(length & 0x00FF);
                }
                else
                {
                    // 8-bit length value > 127
                    buffer[index++] = 0x80 | 1;
                    buffer[index++] = (byte)(length & 0xFF);
                }
            }
            else
            {
                // 8-bit length value < 128
                buffer[index++] = (byte)(length & 0xFF);
            }
        }
    }
}