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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.PhasorProtocols.IEEE1344
{
    #region [ Enumerations ]

    /// <summary>
    /// IEEE 1344 frame types enumeration.
    /// </summary>
    [Serializable]
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
    [Serializable]
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
        /// 100 Over-current trigger.
        /// </summary>
        OverCurrentTrigger = (ushort)Bits.Bit13,
        /// <summary>
        /// 011 Under-voltage trigger.
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
        public const ushort MaximumDataLength = MaximumFrameLength - CommonFrameHeader.FixedLength - 2;

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