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
//  Common.vb - Common PDCstream declarations and functions
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    namespace BpaPdcStream
    {

        [CLSCompliant(false)]
        public sealed class Common
        {


            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated

            }

            public const byte DescriptorPacketFlag = 0x0;

            public const int MaximumPhasorValues = byte.MaxValue;
            public const int MaximumAnalogValues = (int)ReservedFlags.AnalogWordsMask;
            public const int MaximumDigitalValues = (int)IEEEFormatFlags.DigitalWordsMask;

        }

    }

}
