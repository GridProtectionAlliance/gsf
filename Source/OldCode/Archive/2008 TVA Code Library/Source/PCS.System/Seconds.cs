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

using System.Text;

namespace System
{
    /// <summary>
    /// Defines constants and functions related to seconds.
    /// </summary>
    public static class Seconds
    {
        private struct TimeName
        {
            // Note that this is a structure so elements may be used as an index in
            // a string array without having to cast as (int)
            static readonly public int Year = 0;
            static readonly public int Years = 1;
            static readonly public int Day = 2;
            static readonly public int Days = 3;
            static readonly public int Hour = 4;
            static readonly public int Hours = 5;
            static readonly public int Minute = 6;
            static readonly public int Minutes = 7;
            static readonly public int Second = 8;
            static readonly public int Seconds = 9;
            static readonly public int LessThan60Seconds = 10;
            static readonly public int NoSeconds = 11;
        }

        // Standard time names used by ToText function
        private static string[] m_standardTimeNames = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds" };

        // Standard time names, without seconds, used by ToText function
        private static string[] m_standardTimeNamesWithoutSeconds = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes" };

        /// <summary>Fractional number of seconds in one tick.</summary>
        public const double PerTick = 1.0D / Ticks.PerSecond;

        /// <summary>Fractional number of seconds in one millisecond.</summary>
        public const double PerMillisecond = 1.0D / Milliseconds.PerSecond;

        /// <summary>Number of seconds in one minute.</summary>
        public const int PerMinute = 60;

        /// <summary>Number of seconds in one hour.</summary>
        public const int PerHour = 60 * Seconds.PerMinute;

        /// <summary>Number of seconds in one day.</summary>
        public const int PerDay = 24 * Seconds.PerHour;

        /// <summary>
        /// Returns the number of seconds in the specified month and year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month (a number ranging from 1 to 12).</param>
        /// <returns>
        /// The number of seconds in month for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Month is less than 1 or greater than 12. -or- year is less than 1 or greater than 9999.
        /// </exception>
        public static int PerMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month) * Seconds.PerDay;
        }

        /// <summary>
        /// Returns the number of seconds in the specified year.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The number of seconds in the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Year is less than 1 or greater than 9999.
        /// </exception>
        public static long PerYear(int year)
        {
            long total = 0;

            for (int month = 1; month <= 12; month++)
            {
                total += Seconds.PerMonth(year, month);
            }
            
            return total;
        }

        /// <summary>Converts seconds to 100-nanosecond tick intervals.</summary>
        public static long ToTicks(double seconds)
        {
            return (long)(seconds * Ticks.PerSecond);
        }

        /// <summary>Converts seconds to milliseconds.</summary>
        public static double ToMilliseconds(double seconds)
        {
            return seconds * Milliseconds.PerSecond;
        }

        /// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes and whole integer seconds.</summary>
        /// <param name="seconds">Seconds to be converted.</param>
        public static string ToText(double seconds)
        {
            return ToText(seconds, 0);
        }

        /// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes and seconds.</summary>
        /// <param name="seconds">Seconds to be converted.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
        public static string ToText(double seconds, int secondPrecision)
        {
            if (secondPrecision < 0)
                return ToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds);
            else
                return ToText(seconds, secondPrecision, m_standardTimeNames);
        }

        /// <summary>
        /// Turns the given number of seconds into textual representation of years, days, hours, minutes
        /// and seconds given string array of time names.
        /// </summary>
        /// <param name="seconds">Seconds to be converted.</param>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <param name="timeNames">Time names array to use during textual conversion.</param>
        /// <remarks>
        /// <para>Set second precision to -1 to suppress seconds display.</para>
        /// <para>
        /// <paramref name="timeNames"/> array needs one string entry for each of the following names:<br/>
        /// "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds",
        /// "Less Than 60 Seconds", "0 Seconds".
        /// </para>
        /// </remarks>
        public static string ToText(double seconds, int secondPrecision, string[] timeNames)
        {
            StringBuilder timeImage = new StringBuilder();

            // One year of seconds estimated for display use as 365.2425 days, i.e., 31556952 seconds
            const int SecondsPerYear = 31556952;

            int years;  
            int days;
            int hours;
            int minutes;

            // Checks if number of seconds ranges in years.
            years = (int)(seconds / SecondsPerYear);

            if (years >= 1)
            {
                // Removes whole years from remaining seconds.
                seconds = seconds - years * SecondsPerYear;

                // Appends textual representation of years.
                timeImage.Append(years);
                timeImage.Append(' ');

                if (years == 1)
                    timeImage.Append(timeNames[TimeName.Year]);
                else
                    timeImage.Append(timeNames[TimeName.Years]);
            }

            // Checks if remaining number of seconds ranges in days.
            days = (int)(seconds / Seconds.PerDay);
            if (days >= 1)
            {
                // Removes whole days from remaining seconds.
                seconds = seconds - days * Seconds.PerDay;

                // Appends textual representation of days.
                timeImage.Append(' ');
                timeImage.Append(days);
                timeImage.Append(' ');

                if (days == 1)
                    timeImage.Append(timeNames[TimeName.Day]);
                else
                    timeImage.Append(timeNames[TimeName.Days]);
            }

            // Checks if remaining number of seconds ranges in hours.
            hours = (int)(seconds / Seconds.PerHour);
            if (hours >= 1)
            {
                // Removes whole hours from remaining seconds.
                seconds = seconds - hours * Seconds.PerHour;

                // Appends textual representation of hours.
                timeImage.Append(' ');
                timeImage.Append(hours);
                timeImage.Append(' ');

                if (hours == 1)
                    timeImage.Append(timeNames[TimeName.Hour]);
                else
                    timeImage.Append(timeNames[TimeName.Hours]);
            }

            // Checks if remaining number of seconds ranges in minutes.
            minutes = (int)(seconds / Seconds.PerMinute);
            if (minutes >= 1)
            {
                // Removes whole minutes from remaining seconds.
                seconds = seconds - minutes * Seconds.PerMinute;

                // Appends textual representation of minutes.
                timeImage.Append(' ');
                timeImage.Append(minutes);
                timeImage.Append(' ');

                if (minutes == 1)
                    timeImage.Append(timeNames[TimeName.Minute]);
                else
                    timeImage.Append(timeNames[TimeName.Minutes]);
            }

            // Handles remaining seconds.
            if (secondPrecision == 0)
            {
                // No fractional seconds requested. Rounds seconds to nearest integer.
                int wholeSeconds = (int)Math.Round(seconds);

                if (wholeSeconds > 0)
                {
                    // Appends textual representation of whole seconds.
                    timeImage.Append(' ');
                    timeImage.Append(wholeSeconds);
                    timeImage.Append(' ');

                    if (wholeSeconds == 1)
                        timeImage.Append(timeNames[TimeName.Second]);
                    else
                        timeImage.Append(timeNames[TimeName.Seconds]);
                }
            }
            else
            {
                // Handles fractional seconds request.
                if (seconds > 0)
                {
                    if (secondPrecision < 0)
                    {
                        // If second display has been disabled and less than 60 seconds remain, we still need
                        // to show something.
                        if (timeImage.Length == 0)
                            timeImage.Append(timeNames[TimeName.LessThan60Seconds]);
                    }
                    else
                    {
                        // Appends textual representation of fractional seconds.
                        timeImage.Append(' ');
                        timeImage.Append(seconds.ToString("0." + (new string('0', secondPrecision))));
                        timeImage.Append(' ');

                        if (seconds == 1)
                            timeImage.Append(timeNames[TimeName.Second]);
                        else
                            timeImage.Append(timeNames[TimeName.Seconds]);
                    }
                }
            }

            // Handles zero seconds display.
            if (timeImage.Length == 0)
                timeImage.Append(timeNames[TimeName.NoSeconds]);

            return timeImage.ToString().Trim();
        }
    }
}
