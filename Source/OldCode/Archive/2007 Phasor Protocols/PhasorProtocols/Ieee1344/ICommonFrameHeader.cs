//*******************************************************************************************************
//  ICommonFrameHeader.vb - IEEE 1344 Common frame header interface
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

using System;
using TVA.DateTime;

namespace PhasorProtocols
{
    namespace Ieee1344
    {
        /// <summary>IEEE 1344 Common frame header interface</summary>
        [CLSCompliant(false)]
        public interface ICommonFrameHeader : IChannelFrame, IDisposable
        {
            new ulong IDCode
            {
                get;
                set;
            }

            new FrameType FrameType
            {
                get;
            }

            FundamentalFrameType FundamentalFrameType
            {
                get;
            }

            ushort FrameLength
            {
                get;
            }

            ushort DataLength
            {
                get;
            }

            new NtpTimeTag TimeTag
            {
                get;
            }

            short InternalSampleCount
            {
                get;
                set;
            }

            short InternalStatusFlags
            {
                get;
                set;
            }
        }
    }
}