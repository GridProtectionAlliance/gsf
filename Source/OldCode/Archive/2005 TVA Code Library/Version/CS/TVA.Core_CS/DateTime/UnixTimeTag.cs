using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
//using TVA.DateTime.Common;

//*******************************************************************************************************
//  TVA.DateTime.UnixTimeTag.vb - Standard Unix Timetag Class
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/12/2005 - J. Ritchie Carroll
//       Gnerated original version of source.
//  01/05/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Interop.Unix.TimeTag).
//  01/24/2006 - J. Ritchie Carroll
//       Moved into DateTime namespace and renamed to UnixTimeTag.
//  07/12/2006 - J. Ritchie Carroll
//       Modified class to be derived from new "TimeTagBase" class.
//  09/13/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
    namespace DateTime
    {

        /// <summary>Standard Unix Timetag</summary>
        public class UnixTimeTag : TimeTagBase
        {


            // Unix dates are measured as the number of seconds since 1/1/1970, so this class calculates this
            // date to get the offset in ticks for later conversion.
            private static long m_unixDateOffsetTicks;

            protected UnixTimeTag(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                m_unixDateOffsetTicks = (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;



            }

            /// <summary>Creates new Unix timetag, given number of seconds since 1/1/1970.</summary>
            /// <param name="seconds">Number of seconds since 1/1/1970.</param>
            public UnixTimeTag(double seconds)
                : base(m_unixDateOffsetTicks, seconds)
            {
                m_unixDateOffsetTicks = (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;



            }

            /// <summary>Creates new Unix timetag, given standard .NET DateTime.</summary>
            /// <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1970).</param>
            public UnixTimeTag(DateTime timestamp)
                : base(m_unixDateOffsetTicks, timestamp)
            {
                m_unixDateOffsetTicks = (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;



            }

        }

    }

}
