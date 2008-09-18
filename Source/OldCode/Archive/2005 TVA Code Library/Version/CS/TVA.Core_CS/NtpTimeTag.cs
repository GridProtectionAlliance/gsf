//*******************************************************************************************************
//  TVA.NtpTimeTag.vb - Standard Network Time Protocol Timetag Class
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/21/2005 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.DateTime).
//  07/12/2006 - J. Ritchie Carroll
//       Modified class to be derived from new "TimeTagBase" class.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//  09/12/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

/// <summary>Standard Network Time Protocol Timetag.</summary>
public class NtpTimeTag : TimeTagBase
{
    // NTP dates are measured as the number of seconds since 1/1/1900, so we calculate this
    // date to get offset in ticks for later conversion.
    private static long m_ntpDateOffsetTicks = (new DateTime(1900, 1, 1, 0, 0, 0)).Ticks;

    protected NtpTimeTag(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>Creates new NTP timetag, given number of seconds since 1/1/1900.</summary>
    /// <param name="seconds">Number of seconds since 1/1/1900.</param>
    public NtpTimeTag(double seconds)
        : base(m_ntpDateOffsetTicks, seconds)
    {
    }

    /// <summary>Creates new NTP timetag, given standard .NET DateTime.</summary>
    /// <param name="timestamp">.NET DateTime to create Unix timetag from (minimum valid date is 1/1/1900).</param>
    public NtpTimeTag(DateTime timestamp)
        : base(m_ntpDateOffsetTicks, timestamp)
    {
    }
}