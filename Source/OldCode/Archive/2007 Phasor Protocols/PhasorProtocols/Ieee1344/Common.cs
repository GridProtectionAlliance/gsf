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

namespace PCS.PhasorProtocols.Ieee1344
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE 1344 frame types enumeration.
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
        HeaderFrame = Bit.Bit13,
        /// <summary>
        /// 010 Configuration frame.
        /// </summary>
        ConfigurationFrame = Bit.Bit14,
        /// <summary>
        /// 011 Reserved flags 0.
        /// </summary>
        Reserved0 = Bit.Bit13 | Bit.Bit14,
        /// <summary>
        /// 110 Reserved flags 1.
        /// </summary>
        Reserved1 = Bit.Bit14 | Bit.Bit15,
        /// <summary>
        /// 100 Reserved flags 2.
        /// </summary>
        Reserved2 = Bit.Bit15,
        /// <summary>
        /// 101 User defined flags 0.
        /// </summary>
        UserDefined0 = Bit.Bit13 | Bit.Bit15,
        /// <summary>
        /// 111 User defined flags 1.
        /// </summary>
        UserDefined1 = Bit.Bit13 | Bit.Bit14 | Bit.Bit15
    }

    /// <summary>
    /// IEEE 1344 trigger status enumeration.
    /// </summary>
    [Serializable()]
    public enum TriggerStatus : short
    {
        /// <summary>
        /// 111 Frequency trigger.
        /// </summary>
        FrequencyTrigger = Bit.Bit13 | Bit.Bit12 | Bit.Bit11,
        /// <summary>
        /// 110 df/dt trigger.
        /// </summary>
        DfDtTrigger = Bit.Bit13 | Bit.Bit12,
        /// <summary>
        /// 101 Angle trigger.
        /// </summary>
        AngleTrigger = Bit.Bit13 | Bit.Bit11,
        /// <summary>
        /// 100 Overcurrent trigger.
        /// </summary>
        OverCurrentTrigger = Bit.Bit13,
        /// <summary>
        /// 011 Undervoltage trigger.
        /// </summary>
        UnderVoltageTrigger = Bit.Bit12 | Bit.Bit11,
        /// <summary>
        /// 101 Rate trigger.
        /// </summary>
        RateTrigger = Bit.Bit12,
        /// <summary>
        /// 001 User defined.
        /// </summary>
        UserDefined = Bit.Bit11,
        /// <summary>
        /// 000 Unused.
        /// </summary>
        Unused = Bit.Nill
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
        public const short FrameTypeMask = Bit.Bit13 | Bit.Bit14 | Bit.Bit15;

        /// <summary>
        /// Trigger mask constant.
        /// </summary>
        public const short TriggerMask = Bit.Bit11 | Bit.Bit12 | Bit.Bit13;

        /// <summary>
        /// Frame length mask constant.
        /// </summary>
        public const short FrameLengthMask = ~(TriggerMask | Bit.Bit14 | Bit.Bit15);

        /// <summary>
        /// Frame count mask (for multi-framed files) constant.
        /// </summary>
        public const short FrameCountMask = ~(FrameTypeMask | Bit.Bit11 | Bit.Bit12);

        /// <summary>
        /// Maximum frame count (for multi-framed files) constant.
        /// </summary>
        public const short MaximumFrameCount = ~(FrameTypeMask | Bit.Bit11 | Bit.Bit12);

        /// <summary>
        /// Absolute maximum number of samples constant.
        /// </summary>
        public const short MaximumSampleCount = ~FrameTypeMask;

        /// <summary>
        /// Absolute maximum frame length constant.
        /// </summary>
        public const short MaximumFrameLength = ~(TriggerMask | Bit.Bit14 | Bit.Bit15);

        /// <summary>
        /// Absolute maximum data length (within a frame) constant.
        /// </summary>
        public const short MaximumDataLength = MaximumFrameLength - (short)CommonFrameHeader.FixedLength - 2;

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

        /// <summary>
        /// Absolute maximum number of bytes of data that could fit into a header frame constant.
        /// </summary>
        public const int MaximumHeaderDataLength = MaximumFrameLength - CommonFrameHeader.FixedLength - 2;
    }
}