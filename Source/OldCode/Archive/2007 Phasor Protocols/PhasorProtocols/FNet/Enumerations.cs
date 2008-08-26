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
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Moved all namespace level enumerations into "Enumerations.vb" file
//
//*******************************************************************************************************

using System.Diagnostics.CodeAnalysis;

namespace PhasorProtocols
{
    namespace FNet
    {
        /// <summary>FNet data elements</summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct Element
        {
            public const int UnitID = 0;
            public const int Date = 1;
            public const int Time = 2;
            public const int SampleIndex = 3;
            public const int Analog = 4;
            public const int Frequency = 5;
            public const int Voltage = 6;
            public const int Angle = 7;
        }
    }
}
