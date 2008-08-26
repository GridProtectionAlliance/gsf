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
//  ICommonFrameHeader.vb - IEEE C37.118 Common frame header interface
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
    namespace IeeeC37_118
    {

        [CLSCompliant(false)]
        public interface ICommonFrameHeader : IChannelFrame
        {

            new FrameType FrameType
            {
                get;
                set;
            }

            FundamentalFrameType FundamentalFrameType
            {
                get;
            }

            byte Version
            {
                get;
                set;
            }

            ushort FrameLength
            {
                get;
                set;
            }

            int InternalTimeQualityFlags
            {
                get;
                set;
            }

            int TimeBase
            {
                get;
            }

            uint SecondOfCentury
            {
                get;
            }

            int FractionOfSecond
            {
                get;
            }

            TimeQualityFlags TimeQualityFlags
            {
                get;
                set;
            }

            TimeQualityIndicatorCode TimeQualityIndicatorCode
            {
                get;
                set;
            }

        }

    }

}
