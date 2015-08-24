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
        public const long PerSecond = 10000000L;

        /// <summary>Number of 100-nanosecond ticks in one millisecond.</summary>
        public const long PerMillisecond = Ticks.PerSecond / Milliseconds.PerSecond;

        /// <summary>Number of 100-nanosecond ticks in one minute.</summary>
        public const long PerMinute = Seconds.PerMinute * Ticks.PerSecond;

        /// <summary>Number of 100-nanosecond ticks in one hour.</summary>
        public const long PerHour = 60L * Ticks.PerMinute;

        /// <summary>Number of 100-nanosecond ticks in one day.</summary>
        public const long PerDay = 24L * Ticks.PerHour;

        /// <summary>
        /// Returns the number of 100-nanosecond ticks in the specified month and year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (a number ranging from 1 to 12).</param>
        /// <returns>
        /// The number of 100-nanosecond ticks in month for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Month is less than 1 or greater than 12. -or- year is less than 1 or greater than 9999.
        /// </exception>
        public static long PerMonth(int year, int month)
        {
            return Seconds.PerMonth(year, month) * Ticks.PerSecond;
        }

        /// <summary>
        /// Returns the number of 100-nanosecond ticks in the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The number of 100-nanosecond ticks in the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Year is less than 1 or greater than 9999.
        /// </exception>
        public static long PerYear(int year)
        {
            return Seconds.PerYear(year) * Ticks.PerSecond;
        }

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
