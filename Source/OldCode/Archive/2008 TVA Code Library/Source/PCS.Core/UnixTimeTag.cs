//*******************************************************************************************************
//  UnixTimeTag.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/12/2005 - J. Ritchie Carroll
//       Gnerated original version of source.
//  01/05/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (PCS.Interop.Unix.TimeTag).
//  01/24/2006 - J. Ritchie Carroll
//       Moved into DateTime namespace and renamed to UnixTimeTag.
//  07/12/2006 - J. Ritchie Carroll
//       Modified class to be derived from new "TimeTagBase" class.
//  09/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/12/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS
{
    /// <summary>Represents a standard Unix timetag.</summary>
    public class UnixTimeTag : TimeTagBase
    {
        // Unix dates are measured as the number of seconds since 1/1/1970, so this class calculates this
        // date to get the offset in ticks for later conversion.
        private static long m_unixDateOffsetTicks = (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;

        /// <summary>
        /// Creates a new <see cref="UnixTimeTag"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected UnixTimeTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Creates a new <see cref="UnixTimeTag"/>, given number of seconds since 1/1/1970.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1970.</param>
        public UnixTimeTag(double seconds)
            : base(m_unixDateOffsetTicks, seconds)
        {
        }

        /// <summary>Creates a new <see cref="UnixTimeTag"/>, given standard .NET <see cref="DateTime"/>.</summary>
        /// <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1970).</param>
        public UnixTimeTag(DateTime timestamp)
            : base(m_unixDateOffsetTicks, timestamp)
        {
        }
    }
}