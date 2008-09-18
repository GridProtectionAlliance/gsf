//*******************************************************************************************************
//  Ticks.cs
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
//  09/18/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>Defines functions related to ticks.</summary>
    /// <remarks>Ticks are defined as 100-nanosecond intervals, there are 10,000,000 ticks in every second.</remarks>
    public static class Ticks
    {
        /// <summary>Number of 100-nanosecond ticks in one second.</summary>
        public const long PerSecond = 10000000;

        /// <summary>Number of 100-nanosecond ticks in one millisecond.</summary>
        public const long PerMillisecond = Ticks.PerSecond / Milliseconds.PerSecond;

        /// <summary>Converts 100-nanosecond tick intervals to seconds.</summary>
        public static double ToSeconds(long ticks)
        {
            return ticks / (double)PerSecond;
        }

        /// <summary>Converts 100-nanosecond tick intervals to milliseconds.</summary>
        public static double ToMilliseconds(long ticks)
        {
            return ticks / (double)PerMillisecond;
        }

        /// <summary>Turns the given number of ticks into textual representation of years, days, hours, minutes and seconds.</summary>
        /// <param name="ticks">Ticks to be converted.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        public static string ToText(long ticks, int secondPrecision)
        {
            return Seconds.ToText(Ticks.ToSeconds(ticks), secondPrecision);
        }
    }
}
