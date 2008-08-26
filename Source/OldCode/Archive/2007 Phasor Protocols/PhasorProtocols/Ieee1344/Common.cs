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
//  Common.vb - Common IEEE1344 declarations and functions
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    namespace Ieee1344
    {

        [CLSCompliant(false)]
        public sealed class Common
        {


            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated

            }

            /// <summary>Frame type mask</summary>
            public const short FrameTypeMask = Bit.Bit13 | Bit.Bit14 | Bit.Bit15;

            /// <summary>Trigger mask</summary>
            public const short TriggerMask = Bit.Bit11 | Bit.Bit12 | Bit.Bit13;

            /// <summary>Frame length mask</summary>
            public const short FrameLengthMask = ~(TriggerMask | Bit.Bit14 | Bit.Bit15);

            /// <summary>Frame count mask (for multi-framed files)</summary>
            public const short FrameCountMask = ~(FrameTypeMask | Bit.Bit11 | Bit.Bit12);

            /// <summary>Maximum frame count (for multi-framed files)</summary>
            public const short MaximumFrameCount = ~(FrameTypeMask | Bit.Bit11 | Bit.Bit12);

            /// <summary>Absolute maximum number of samples</summary>
            public const short MaximumSampleCount = ~FrameTypeMask;

            /// <summary>Absolute maximum frame length</summary>
            public const short MaximumFrameLength = ~(TriggerMask | Bit.Bit14 | Bit.Bit15);

            /// <summary>Absolute maximum data length (within a frame)</summary>
            public const short MaximumDataLength = MaximumFrameLength - (short)CommonFrameHeader.BinaryLength - 2;

            /// <summary>Absolute maximum number of possible phasor values that could fit into a data frame</summary>
            public const int MaximumPhasorValues = MaximumDataLength / 4 - 6;

            /// <summary>Absolute maximum number of possible analog values that could fit into a data frame</summary>
            /// <remarks>IEEE 1344 doesn't support analog values</remarks>
            public const int MaximumAnalogValues = 0;

            /// <summary>Absolute maximum number of possible digital values that could fit into a data frame</summary>
            public const int MaximumDigitalValues = MaximumDataLength / 2 - 6;

            /// <summary>Absolute maximum number of bytes of data that could fit into a header frame</summary>
            public const int MaximumHeaderDataLength = MaximumFrameLength - CommonFrameHeader.BinaryLength - 2;

        }

    }
}
