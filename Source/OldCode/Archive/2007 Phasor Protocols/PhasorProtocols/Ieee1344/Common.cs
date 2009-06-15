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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using TVA;

namespace PhasorProtocols.Ieee1344
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE 1344 frame types enumeration.
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
        HeaderFrame = (ushort)Bits.Bit13,
        /// <summary>
        /// 010 Configuration frame.
        /// </summary>
        ConfigurationFrame = (ushort)Bits.Bit14,
        /// <summary>
        /// 011 Reserved flags 0.
        /// </summary>
        Reserved0 = (ushort)(Bits.Bit13 | Bits.Bit14),
        /// <summary>
        /// 110 Reserved flags 1.
        /// </summary>
        Reserved1 = (ushort)(Bits.Bit14 | Bits.Bit15),
        /// <summary>
        /// 100 Reserved flags 2.
        /// </summary>
        Reserved2 = (ushort)Bits.Bit15,
        /// <summary>
        /// 101 User defined flags 0.
        /// </summary>
        UserDefined0 = (ushort)(Bits.Bit13 | Bits.Bit15),
        /// <summary>
        /// 111 User defined flags 1.
        /// </summary>
        UserDefined1 = (ushort)(Bits.Bit13 | Bits.Bit14 | Bits.Bit15)
    }

    /// <summary>
    /// IEEE 1344 trigger status enumeration.
    /// </summary>
    [Serializable()]
    public enum TriggerStatus : ushort
    {
        /// <summary>
        /// 111 Frequency trigger.
        /// </summary>
        FrequencyTrigger = (ushort)(Bits.Bit13 | Bits.Bit12 | Bits.Bit11),
        /// <summary>
        /// 110 df/dt trigger.
        /// </summary>
        DfDtTrigger = (ushort)(Bits.Bit13 | Bits.Bit12),
        /// <summary>
        /// 101 Angle trigger.
        /// </summary>
        AngleTrigger = (ushort)(Bits.Bit13 | Bits.Bit11),
        /// <summary>
        /// 100 Overcurrent trigger.
        /// </summary>
        OverCurrentTrigger = (ushort)Bits.Bit13,
        /// <summary>
        /// 011 Undervoltage trigger.
        /// </summary>
        UnderVoltageTrigger = (ushort)(Bits.Bit12 | Bits.Bit11),
        /// <summary>
        /// 010 Rate trigger.
        /// </summary>
        RateTrigger = (ushort)Bits.Bit12,
        /// <summary>
        /// 001 User defined.
        /// </summary>
        UserDefined = (ushort)Bits.Bit11,
        /// <summary>
        /// 000 Unused.
        /// </summary>
        Unused = (ushort)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Common IEEE 1344 declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Frame type mask constant.
        /// </summary>
        public const ushort FrameTypeMask = (ushort)(Bits.Bit13 | Bits.Bit14 | Bits.Bit15);

        /// <summary>
        /// Trigger mask constant.
        /// </summary>
        public const ushort TriggerMask = (ushort)(Bits.Bit11 | Bits.Bit12 | Bits.Bit13);

        /// <summary>
        /// Frame length mask constant.
        /// </summary>
        public const ushort FrameLengthMask = unchecked((ushort)~(TriggerMask | (ushort)(Bits.Bit14 | Bits.Bit15)));

        /// <summary>
        /// Frame count mask (for multi-framed files) constant.
        /// </summary>
        public const ushort FrameCountMask = unchecked((ushort)~(FrameTypeMask | (ushort)(Bits.Bit11 | Bits.Bit12)));

        /// <summary>
        /// Maximum frame count (for multi-framed files) constant.
        /// </summary>
        public const ushort MaximumFrameCount = unchecked((ushort)~(FrameTypeMask | (ushort)(Bits.Bit11 | Bits.Bit12)));

        /// <summary>
        /// Absolute maximum number of samples constant.
        /// </summary>
        public const ushort MaximumSampleCount = unchecked((ushort)~FrameTypeMask);

        /// <summary>
        /// Absolute maximum frame length constant.
        /// </summary>
        public const ushort MaximumFrameLength = FrameLengthMask;

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = (ushort)(MaximumFrameLength - CommonFrameHeader.FixedLength - 2);

        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame constant.
        /// </summary>
        public const int MaximumPhasorValues = MaximumDataLength / 4 - 6;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame constant.
        /// </summary>
        /// <remarks>IEEE 1344 doesn't support analog values</remarks>
        public const int MaximumAnalogValues = 0;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame constant.
        /// </summary>
        public const int MaximumDigitalValues = MaximumDataLength / 2 - 6;
    }
}