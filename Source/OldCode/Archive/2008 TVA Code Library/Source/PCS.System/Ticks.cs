/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

namespace System
{
    /// <summary>
    /// Defines constants and functions related to ticks.
    /// </summary>
    /// <remarks>
    /// Ticks are defined as 100-nanosecond intervals, there are 10,000,000 ticks in every second.
    /// </remarks>
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
