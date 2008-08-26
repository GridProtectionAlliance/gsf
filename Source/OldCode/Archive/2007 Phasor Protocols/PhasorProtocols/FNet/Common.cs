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
//  Common.vb - Common FNet declarations and functions
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
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    namespace FNet
    {

        [CLSCompliant(false)]
        public sealed class Common
        {


            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated

            }

            /// <summary>FNET data frame start byte</summary>
            public const byte StartByte = 0x1;

            /// <summary>FNET data frame start byte</summary>
            public const byte EndByte = 0x0;

            /// <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
            public const int MaximumPhasorValues = 1;

            /// <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
            /// <remarks>FNET doesn't support analog values</remarks>
            public const int MaximumAnalogValues = 0;

            /// <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
            /// <remarks>FNET doesn't support digital values</remarks>
            public const int MaximumDigitalValues = 0;

            /// <summary>Default frame rate for FNET devices is 10 frames per second</summary>
            public const short DefaultFrameRate = 10;

            /// <summary>Default nominal frequency for FNET devices is 60Hz</summary>
            public const LineFrequency DefaultNominalFrequency = LineFrequency.Hz60;

            /// <summary>Default real-time ticks offset for FNET</summary>
            public const long DefaultTicksOffset = 110000000;

        }

    }
}
