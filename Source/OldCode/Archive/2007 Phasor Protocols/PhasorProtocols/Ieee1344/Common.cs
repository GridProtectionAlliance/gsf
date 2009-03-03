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
    /// <summary>
    /// Common IEEE1344 declarations and functions.
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