//*******************************************************************************************************
//  Enumerations.cs
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
//  07/11/2007 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// IEEE1344 frame types enumeration.
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
        /// 101 User defined flags 1.
        /// </summary>
        UserDefined1 = Bit.Bit13 | Bit.Bit14 | Bit.Bit15
    }

    /// <summary>
    /// IEEE1344 trigger status enumeration.
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
}