/**************************************************************************\
   Copyright (c) 2008 - Gbtc, James Ritchie Carroll
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
    /// Defines constants and functions related to milliseconds.
    /// </summary>
    /// <remarks>
    /// One millisecond is defined as 1/1000th of a second.
    /// </remarks>
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
