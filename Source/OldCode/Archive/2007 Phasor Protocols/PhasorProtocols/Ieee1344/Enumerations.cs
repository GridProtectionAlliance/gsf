using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/11/2007 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    namespace Ieee1344
    {

        /// <summary>Frame type</summary>
        [Serializable()]
        public enum FrameType : short
        {
            /// <summary>000 Data frame</summary>
            DataFrame = Bit.Nill,
            /// <summary>001 Header frame</summary>
            HeaderFrame = Bit.Bit13,
            /// <summary>010 Configuration frame</summary>
            ConfigurationFrame = Bit.Bit14,
            /// <summary>011 Reserved flags 0</summary>
            Reserved0 = Bit.Bit13 | Bit.Bit14,
            /// <summary>110 Reserved flags 1</summary>
            Reserved1 = Bit.Bit14 | Bit.Bit15,
            /// <summary>100 Reserved flags 2</summary>
            Reserved2 = Bit.Bit15,
            /// <summary>101 User defined flags 0</summary>
            UserDefined0 = Bit.Bit13 | Bit.Bit15,
            /// <summary>101 User defined flags 1</summary>
            UserDefined1 = Bit.Bit13 | Bit.Bit14 | Bit.Bit15
        }

        /// <summary>Trigger status</summary>
        [Serializable()]
        public enum TriggerStatus : short
        {
            /// <summary>111 Frequency trigger</summary>
            FrequencyTrigger = Bit.Bit13 | Bit.Bit12 | Bit.Bit11,
            /// <summary>110 df/dt trigger</summary>
            DfDtTrigger = Bit.Bit13 | Bit.Bit12,
            /// <summary>101 Angle trigger</summary>
            AngleTrigger = Bit.Bit13 | Bit.Bit11,
            /// <summary>100 Overcurrent trigger</summary>
            OverCurrentTrigger = Bit.Bit13,
            /// <summary>011 Undervoltage trigger</summary>
            UnderVoltageTrigger = Bit.Bit12 | Bit.Bit11,
            /// <summary>101 Rate trigger</summary>
            RateTrigger = Bit.Bit12,
            /// <summary>001 User defined</summary>
            UserDefined = Bit.Bit11,
            /// <summary>000 Unused</summary>
            Unused = Bit.Nill
        }

    }
}
