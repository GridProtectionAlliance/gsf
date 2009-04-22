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

namespace PCS.PhasorProtocols.BpaPdcStream
{
    #region [ Enumerations ]

    /// <summary>
    /// Stream type enueration.
    /// </summary>
    [Serializable()]
    public enum StreamType : byte
    {
        /// <summary>
        /// Standard full data stream.
        /// </summary>
        Legacy = 0,
        /// <summary>
        /// Full data stream with PMU ID's and offsets removed from data packet.
        /// </summary>
        Compact = 1
    }

    /// <summary>
    /// Stream revision number enumeration.
    /// </summary>
    [Serializable()]
    public enum RevisionNumber : byte
    {
        /// <summary>
        /// Original revision for all to June 2002, use NTP timetag (start count 1900).
        /// </summary>
        Revision0 = 0,
        /// <summary>
        /// July 2002 revision for std. 37.118, use UNIX timetag (start count 1970).
        /// </summary>
        Revision1 = 1,
        /// <summary>
        /// May 2005 revision for std. 37.118, change ChanFlag for added data types.
        /// </summary>
        Revision2 = 2
    }

    /// <summary>
    /// Channel flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum ChannelFlags : byte
    {
        /// <summary>
        /// Valid if not set (yes = 0).
        /// </summary>
        DataIsValid = (byte)Bits.Bit07,
        /// <summary>
        /// Errors if set (yes = 1).
        /// </summary>
        TransmissionErrors = (byte)Bits.Bit06,
        /// <summary>
        /// Not sync'd if set (yes = 0).
        /// </summary>
        PMUSynchronized = (byte)Bits.Bit05,
        /// <summary>
        /// Data out of sync if set (yes = 1).
        /// </summary>
        DataSortedByArrival = (byte)Bits.Bit04,
        /// <summary>
        /// Sorted by timestamp if not set (yes = 0).
        /// </summary>
        DataSortedByTimestamp = (byte)Bits.Bit03,
        /// <summary>
        /// PDC format if set (yes = 1).
        /// </summary>
        PDCExchangeFormat = (byte)Bits.Bit02,
        /// <summary>
        /// Macrodyne or IEEE format (Macrodyne = 1).
        /// </summary>
        MacrodyneFormat = (byte)Bits.Bit01,
        /// <summary>
        /// Timestamp included if not set (yes = 0).
        /// </summary>
        TimestampIncluded = (byte)Bits.Bit00
    }

    /// <summary>
    /// Reserved flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum ReservedFlags : byte
    {
        /// <summary>
        /// Reserved bit 7.
        /// </summary>
        Reserved0 = (byte)Bits.Bit7,
        /// <summary>
        /// Reserved bit 6.
        /// </summary>
        Reserved1 = (byte)Bits.Bit6,
        /// <summary>
        /// Analog words mask.
        /// </summary>
        AnalogWordsMask = (byte)(Bits.Bit0 | Bits.Bit1 | Bits.Bit2 | Bits.Bit3 | Bits.Bit4 | Bits.Bit5)
    }

    /// <summary>
    /// IEEE format flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum IEEEFormatFlags : byte
    {
        /// <summary>
        /// Frequency data format: Set = float, Clear = integer.
        /// </summary>
        Frequency = (byte)Bits.Bit7,
        /// <summary>
        /// Analog data format: Set = float, Clear = integer.
        /// </summary>
        Analog = (byte)Bits.Bit6,
        /// <summary>
        /// Phasor data format: Set = float, Clear = integer.
        /// </summary>
        Phasors = (byte)Bits.Bit5,
        /// <summary>
        /// Phasor coordinate format: Set = polar, Clear = rectangular.
        /// </summary>
        Coordinates = (byte)Bits.Bit4,
        /// <summary>
        /// Digital words mask.
        /// </summary>
        DigitalWordsMask = (byte)(Bits.Bit0 | Bits.Bit1 | Bits.Bit2 | Bits.Bit3)
    }

    /// <summary>
    /// Frame type enumeration.
    /// </summary>
    [Serializable()]
    public enum FrameType : byte
    {
        /// <summary>
        /// Configuration frame.
        /// </summary>
        ConfigurationFrame,
        /// <summary>
        /// Data frame.
        /// </summary>
        DataFrame
    }

    /// <summary>
    /// PMU status flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum PMUStatusFlags : byte
    {
        /// <summary>
        /// Synchonization is invalid.
        /// </summary>
        SyncInvalid = (byte)Bits.Bit0,
        /// <summary>
        /// Data is invalid.
        /// </summary>
        DataInvalid = (byte)Bits.Bit1
    }

    #endregion

    /// <summary>
    /// Common BPA PDCstream declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Descriptor packet flag.
        /// </summary>
        public const byte DescriptorPacketFlag = 0x0;

        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = byte.MaxValue;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = (ushort)ReservedFlags.AnalogWordsMask;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = (ushort)IEEEFormatFlags.DigitalWordsMask;

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = (ushort)(ushort.MaxValue - CommonFrameHeader.FixedLength - 2);
    }
}