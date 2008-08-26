using System.Diagnostics;
using System;
////using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
////using TVA.Interop.Bit;
using System.Linq;

//*******************************************************************************************************
//  Common.vb - Common IEEE C37.118 declarations and functions
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
    namespace IeeeC37_118
    {

        [CLSCompliant(false)]
        public sealed class Common
        {


            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated

            }

            /// <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
            public const int MaximumPhasorValues = ushort.MaxValue / 4 - CommonFrameHeader.BinaryLength - 8;

            /// <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
            public const int MaximumAnalogValues = ushort.MaxValue / 2 - CommonFrameHeader.BinaryLength - 8;

            /// <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
            public const int MaximumDigitalValues = ushort.MaxValue / 2 - CommonFrameHeader.BinaryLength - 8;

            /// <summary>Absolute maximum number of bytes of data that could fit into a header frame</summary>
            public const int MaximumHeaderDataLength = ushort.MaxValue - CommonFrameHeader.BinaryLength - 2;

            /// <summary>Absolute maximum number of bytes of extended data that could fit into a command frame</summary>
            public const int MaximumExtendedDataLength = ushort.MaxValue - CommonFrameHeader.BinaryLength - 4;

            /// <summary>Time quality flags mask</summary>
            public const int TimeQualityFlagsMask = Bit.Bit31 | Bit.Bit30 | Bit.Bit29 | Bit.Bit28 | Bit.Bit27 | Bit.Bit26 | Bit.Bit25 | Bit.Bit24;

        }

    }
}
