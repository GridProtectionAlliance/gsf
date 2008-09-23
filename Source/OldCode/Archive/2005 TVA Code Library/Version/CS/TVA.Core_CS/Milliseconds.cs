//*******************************************************************************************************
//  Milliseconds.cs
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
    /// <summary>Defines functions related to milliseconds.</summary>
    /// <remarks>One millisecond is defined as 1/1000th of a second.</remarks>
    public static class Milliseconds
    {
        /// <summary>Number of milliseconds in one second.</summary>
        public const int PerSecond = 1000;

        /// <summary>Fractional number of milliseconds in one tick.</summary>
        public const double PerTick = 1.0D / Ticks.PerMillisecond;

        /// <summary>Number of milliseconds in one minute.</summary>
        public const int PerMinute = Seconds.PerMinute * Milliseconds.PerSecond;

        /// <summary>Number of milliseconds in one hour.</summary>
        public const int PerHour = 60 * Milliseconds.PerMinute;

        /// <summary>Number of milliseconds in one day.</summary>
        public const int PerDay = 24 * Milliseconds.PerHour;

        /// <summary>
        /// Returns the number of milliseconds in the specified month and year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (a number ranging from 1 to 12).</param>
        /// <returns>
        /// The number of milliseconds in month for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Month is less than 1 or greater than 12. -or- year is less than 1 or greater than 9999.
        /// </exception>
        public static long PerMonth(int year, int month)
        {
            return (long)Seconds.PerMonth(year, month) * (long)Milliseconds.PerSecond;
        }

        /// <summary>
        /// Returns the number of milliseconds in the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The number of milliseconds in the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Year is less than 1 or greater than 9999.
        /// </exception>
        public static long PerYear(int year)
        {
            return Seconds.PerYear(year) * (long)Milliseconds.PerSecond;
        }

        /// <summary>Converts milliseconds to 100-nanosecond tick intervals.</summary>
        public static long ToTicks(double milliseconds)
        {
            return (long)(milliseconds * Ticks.PerMillisecond);
        }

        /// <summary>Converts milliseconds to seconds.</summary>
        public static double ToSeconds(double milliseconds)
        {
            return milliseconds / Milliseconds.PerSecond;
        }

        /// <summary>Turns the given number of milliseconds into textual representation of years, days, hours, minutes and seconds.</summary>
        /// <param name="milliseconds">Milliseconds to be converted.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        public static string ToText(double milliseconds, int secondPrecision)
        {
            return Seconds.ToText(Milliseconds.ToSeconds(milliseconds), secondPrecision);
        }
    }
}
