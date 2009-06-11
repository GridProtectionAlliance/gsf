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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace TVA
{
    #region [ Enumerations ]

    /// <summary>
    /// Time intervals enumeration used by <see cref="Ticks.BaselinedTimestamp"/> method.
    /// </summary>
    public enum BaselineTimeInterval
    {
        /// <summary>Baseline timestamp to the second (i.e., starting at zero milliseconds).</summary>
        Second,
        /// <summary>Baseline timestamp to the minute (i.e., starting at zero seconds and milliseconds).</summary>
        Minute,
        /// <summary>Baseline timestamp to the hour (i.e., starting at zero minutes, seconds and milliseconds).</summary>
        Hour,
        /// <summary>Baseline timestamp to the day (i.e., starting at zero hours, minutes, seconds and milliseconds).</summary>
        Day,
        /// <summary>Baseline timestamp to the month (i.e., starting at day one, zero hours, minutes, seconds and milliseconds).</summary>
        Month,
        /// <summary>Baseline timestamp to the year (i.e., starting at month one, day one, zero hours, minutes, seconds and milliseconds).</summary>
        Year
    }

    #endregion

    /// <summary>
    /// Represents an instant in time, or time period, as a 64-bit signed integer with a value that is expressed as the number
    /// of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Ticks"/> can represent an "instant in time" and therefore can be used exactly like a <see cref="DateTime"/>.
    /// The difference between <see cref="Ticks"/> and <see cref="DateTime"/> is that <see cref="Ticks"/> is essentially a long
    /// integer (i.e., an <see cref="Int64"/>) which represents the number of ticks that have elapsed since 12:00:00 midnight,
    /// January 1, 0001 with each tick having a resolution of 100-nanoseconds. You would use <see cref="Ticks"/> in places where
    /// you needed to directly represent time in high-resolution, i.e., time with subsecond accuracy, using an object that will
    /// act like a long integer but handle time conversions. <see cref="Ticks"/> can also represent a "time period" (e.g., the
    /// number of ticks elapsed since a process started) and thus can also be used like a <see cref="TimeSpan"/>; when used in
    /// this manner the <see cref="Ticks.ToString()"/> method can be used to convert the <see cref="Ticks"/> value into a handy
    /// textual representation of elapsed years, days, hours, minutes and seconds.
    /// </para>
    /// <para>
    /// This class behaves just like an <see cref="Int64"/> representing a time in ticks; it is implictly castable to and from
    /// an <see cref="Int64"/> and therefore can be generally used "as" an Int64 directly. It is also implicitly castable to and
    /// from a <see cref="DateTime"/>, a <see cref="TimeSpan"/>, an <see cref="NtpTimeTag"/> and a <see cref="UnixTimeTag"/>.
    /// </para>
    /// </remarks>
    [Serializable()]
    public struct Ticks : IComparable, IFormattable, IConvertible, IComparable<Ticks>, IComparable<Int64>, IComparable<DateTime>, IComparable<TimeSpan>, IEquatable<Ticks>, IEquatable<Int64>, IEquatable<DateTime>, IEquatable<TimeSpan>
    {
        #region [ Members ]

        // Nested Types
        private struct TimeName
        {
            // Note that this is a structure so that elements may be used as an
            // index into a string array without having to cast as (int)
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

        // Constants

        // Standard time names used by ToString method
        private static string[] m_timeNames = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds" };

        // Standard time names, without seconds, used by ToString method
        private static string[] m_timeNamesNoSeconds = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes" };

        /// <summary>Number of 100-nanosecond ticks in one second.</summary>
        public const long PerSecond = 10000000L;

        /// <summary>Number of 100-nanosecond ticks in one millisecond.</summary>
        public const long PerMillisecond = Ticks.PerSecond / 1000L;

        /// <summary>Number of 100-nanosecond ticks in one minute.</summary>
        public const long PerMinute = 60L * Ticks.PerSecond;

        /// <summary>Number of 100-nanosecond ticks in one hour.</summary>
        public const long PerHour = 60L * Ticks.PerMinute;

        /// <summary>Number of 100-nanosecond ticks in one day.</summary>
        public const long PerDay = 24L * Ticks.PerHour;

        // Fields
        private long m_value; // Time value stored in ticks

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Ticks"/>.
        /// </summary>
        /// <param name="value">New time value in ticks.</param>
        public Ticks(long value)
        {
            m_value = value;
        }

        /// <summary>
        /// Creates a new <see cref="Ticks"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="DateTime"/>.</param>
        public Ticks(DateTime value)
        {
            m_value = value.Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="Ticks"/>.
        /// </summary>
        /// <param name="value">New time value as a <see cref="TimeSpan"/>.</param>
        public Ticks(TimeSpan value)
        {
            m_value = value.Ticks;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Ticks"/> value in equivalent number of seconds.
        /// </summary>
        /// <returns>Value of <see cref="Ticks"/> in seconds.</returns>
        /// <remarks>
        /// If <see cref="Ticks"/> value represents an instant in time, returned value will represent the number of seconds
        /// that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public double ToSeconds()
        {
            return m_value / (double)Ticks.PerSecond;
        }

        /// <summary>
        /// Gets the <see cref="Ticks"/> value in equivalent number of milliseconds.
        /// </summary>
        /// <returns>Value of <see cref="Ticks"/> in milliseconds.</returns>
        /// <remarks>
        /// If <see cref="Ticks"/> value represents an instant in time, returned value will represent the number of milliseconds
        /// that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public double ToMilliseconds()
        {
            return m_value / (double)Ticks.PerMillisecond;
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value in UTC time, is valid by comparing it to
        /// the system clock.
        /// </summary>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if UTC time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of system clock time in UTC. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be set
        /// to sub-second intervals.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be less than one.
        /// </exception>
        public bool UtcTimeIsValid(double lagTime, double leadTime)
        {
            return TimeIsValid(DateTime.UtcNow.Ticks, lagTime, leadTime);
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value in UTC time, is valid by comparing it to
        /// the system clock.
        /// </summary>
        /// <param name="lagTime">The allowed lag time, in ticks, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in ticks, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if UTC time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of system clock time in UTC. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </exception>
        public bool UtcTimeIsValid(Ticks lagTime, Ticks leadTime)
        {
            return TimeIsValid(DateTime.UtcNow.Ticks, lagTime, leadTime);
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value in local time, is valid by comparing it to
        /// the system clock.
        /// </summary>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if local time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of local system clock time. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be set
        /// to sub-second intervals.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be less than one.
        /// </exception>
        public bool LocalTimeIsValid(double lagTime, double leadTime)
        {
            return TimeIsValid(DateTime.Now.Ticks, lagTime, leadTime);
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value in local time, is valid by comparing it to
        /// the system clock.
        /// </summary>
        /// <param name="lagTime">The allowed lag time, in ticks, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in ticks, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if local time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of local system clock time. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </exception>
        public bool LocalTimeIsValid(Ticks lagTime, Ticks leadTime)
        {
            return TimeIsValid(DateTime.Now.Ticks, lagTime, leadTime);
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value, is valid by comparing it to the specified
        /// current time.
        /// </summary>
        /// <param name="currentTime">Specified current time (e.g., could be DateTime.Now.Ticks).</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of <paramref name="currentTime"/>. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be set
        /// to sub-second intervals.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero, but can be less than one.
        /// </exception>
        public bool TimeIsValid(Ticks currentTime, double lagTime, double leadTime)
        {
            if (lagTime <= 0)
                throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one");

            if (leadTime <= 0)
                throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one");

            double distance = (currentTime.m_value - m_value) / (double)Ticks.PerSecond;

            return (distance >= -leadTime && distance <= lagTime);
        }

        /// <summary>
        /// Determines if time, represented by <see cref="Ticks"/> value, is valid by comparing it to the specified
        /// current time.
        /// </summary>
        /// <param name="currentTime">Specified current time (e.g., could be DateTime.Now.Ticks).</param>
        /// <param name="lagTime">The allowed lag time, in ticks, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in ticks, before assuming time is too advanced to be valid.</param>
        /// <returns>True, if time represented by <see cref="Ticks"/> value, is within the specified range.</returns>
        /// <remarks>
        /// Time, represented by <see cref="Ticks"/> value, is considered valid if it exists within the specified
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> range of <paramref name="currentTime"/>. Note
        /// that <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="lagTime"/> and <paramref name="leadTime"/> must be greater than zero.
        /// </exception>
        public bool TimeIsValid(Ticks currentTime, Ticks lagTime, Ticks leadTime)
        {
            if (lagTime.m_value <= 0)
                throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero");

            if (leadTime.m_value <= 0)
                throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero");

            long distance = (currentTime.m_value - m_value);

            return (distance >= -leadTime.m_value && distance <= lagTime.m_value);
        }

        /// <summary>
        /// Gets the distance, in 100-nanoseconds intervals, beyond the top of the second in the timestamp
        /// represented by the <see cref="Ticks"/>.
        /// </summary>
        /// <returns>
        /// Number of 100-nanoseconds intervals <see cref="Ticks"/> value is from the top of the second.
        /// </returns>
        public Ticks DistanceBeyondSecond()
        {
            return m_value - (m_value - m_value % Ticks.PerSecond);
        }

        /// <summary>
        /// Creates a new <see cref="Ticks"/> value that represents a baselined timestamp, in 100-nanoseconds
        /// intervals, that begins at the beginning of the specified time interval.
        /// </summary>
        /// <param name="interval">
        /// <see cref="BaselineTimeInterval"/> to which <see cref="Ticks"/> timestamp should be baselined.
        /// </param>
        /// <returns>
        /// A new <see cref="Ticks"/> value that represents a baselined timestamp, in 100-nanoseconds intervals,
        /// that begins at the specified <see cref="BaselineTimeInterval"/>.
        /// </returns>
        /// <remarks>
        /// Baselining to the <see cref="BaselineTimeInterval.Second"/> would return the <see cref="Ticks"/>
        /// value starting at zero milliseconds.<br/>
        /// Baselining to the <see cref="BaselineTimeInterval.Minute"/> would return the <see cref="Ticks"/>
        /// value starting at zero seconds and milliseconds.<br/>
        /// Baselining to the <see cref="BaselineTimeInterval.Hour"/> would return the <see cref="Ticks"/>
        /// value starting at zero minutes, seconds and milliseconds.<br/>
        /// Baselining to the <see cref="BaselineTimeInterval.Day"/> would return the <see cref="Ticks"/>
        /// value starting at zero hours, minutes, seconds and milliseconds.<br/>
        /// Baselining to the <see cref="BaselineTimeInterval.Month"/> would return the <see cref="Ticks"/>
        /// value starting at day one, zero hours, minutes, seconds and milliseconds.<br/>
        /// Baselining to the <see cref="BaselineTimeInterval.Year"/> would return the <see cref="Ticks"/>
        /// value starting at month one, day one, zero hours, minutes, seconds and milliseconds.
        /// </remarks>
        public Ticks BaselinedTimestamp(BaselineTimeInterval interval)
        {
            switch (interval)
            {
                case BaselineTimeInterval.Second:
                    return m_value - m_value % Ticks.PerSecond;
                case BaselineTimeInterval.Minute:
                    return m_value - m_value % Ticks.PerMinute;
                case BaselineTimeInterval.Hour:
                    return m_value - m_value % Ticks.PerHour;
                case BaselineTimeInterval.Day:
                    return m_value - m_value % Ticks.PerDay;
                case BaselineTimeInterval.Month:
                    DateTime toMonth = new DateTime(m_value);
                    return new DateTime(toMonth.Year, toMonth.Month, 1, 0, 0, 0, 0).Ticks;
                case BaselineTimeInterval.Year:
                    return new DateTime((new DateTime(m_value)).Year, 1, 1, 0, 0, 0, 0).Ticks;
                default:
                    return this;
            }
        }

        /// <summary>
        /// Converts the <see cref="Ticks"/> value into a textual representation of years, days, hours,
        /// minutes and seconds.
        /// </summary>
        /// <remarks>
        /// Note that this ToString overload will not display fractional seconds. To allow display of
        /// fractional seconds, or completely remove second resolution from the textual representation,
        /// use the <see cref="Ticks.ToString(int)"/> overload instead.
        /// </remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public override string ToString()
        {
            return ToString(0, m_timeNames);
        }

        /// <summary>
        /// Converts the <see cref="Ticks"/> value into a textual representation of years, days, hours,
        /// minutes and seconds with the specified number of fractional digits.
        /// </summary>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <remarks>Set <paramref name="secondPrecision"/> to -1 to suppress seconds display.</remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public string ToString(int secondPrecision)
        {
            if (secondPrecision < 0)
                return ToString(secondPrecision, m_timeNamesNoSeconds);
            else
                return ToString(secondPrecision, m_timeNames);
        }

        /// <summary>
        /// Converts the <see cref="Ticks"/> value into a textual representation of years, days, hours,
        /// minutes and seconds with the specified number of fractional digits given string array of
        /// time names.
        /// </summary>
        /// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
        /// <param name="timeNames">Time names array to use during textual conversion.</param>
        /// <remarks>
        /// <para>Set <paramref name="secondPrecision"/> to -1 to suppress seconds display.</para>
        /// <para>
        /// <paramref name="timeNames"/> array needs one string entry for each of the following names:<br/>
        /// "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds",
        /// "Less Than 60 Seconds", "0 Seconds".
        /// </para>
        /// </remarks>
        /// <returns>
        /// The string representation of the value of this instance, consisting of the number of
        /// years, days, hours, minutes and seconds represented by this value.
        /// </returns>
        public string ToString(int secondPrecision, string[] timeNames)
        {
            // One year of seconds estimated for display use as 365.2425 days, i.e., 31556952 seconds
            const int SecondsPerYear = 31556952;
            const int SecondsPerMinute = 60;
            const int SecondsPerHour = 60 * SecondsPerMinute;
            const int SecondsPerDay = 24 * SecondsPerHour;

            StringBuilder timeImage = new StringBuilder();
            int years, days, hours, minutes;
            double seconds = ToSeconds();

            // Checks if number of seconds ranges in years
            years = (int)(seconds / SecondsPerYear);

            if (years >= 1)
            {
                // Removes whole years from remaining seconds
                seconds = seconds - years * SecondsPerYear;

                // Appends textual representation of years
                timeImage.Append(years);
                timeImage.Append(' ');

                if (years == 1)
                    timeImage.Append(timeNames[TimeName.Year]);
                else
                    timeImage.Append(timeNames[TimeName.Years]);
            }

            // Checks if remaining number of seconds ranges in days
            days = (int)(seconds / SecondsPerDay);

            if (days >= 1)
            {
                // Removes whole days from remaining seconds
                seconds = seconds - days * SecondsPerDay;

                // Appends textual representation of days
                timeImage.Append(' ');
                timeImage.Append(days);
                timeImage.Append(' ');

                if (days == 1)
                    timeImage.Append(timeNames[TimeName.Day]);
                else
                    timeImage.Append(timeNames[TimeName.Days]);
            }

            // Checks if remaining number of seconds ranges in hours
            hours = (int)(seconds / SecondsPerHour);

            if (hours >= 1)
            {
                // Removes whole hours from remaining seconds
                seconds = seconds - hours * SecondsPerHour;

                // Appends textual representation of hours
                timeImage.Append(' ');
                timeImage.Append(hours);
                timeImage.Append(' ');

                if (hours == 1)
                    timeImage.Append(timeNames[TimeName.Hour]);
                else
                    timeImage.Append(timeNames[TimeName.Hours]);
            }

            // Checks if remaining number of seconds ranges in minutes
            minutes = (int)(seconds / SecondsPerMinute);

            if (minutes >= 1)
            {
                // Removes whole minutes from remaining seconds
                seconds = seconds - minutes * SecondsPerMinute;

                // If no fractional seconds were requested and remaining seconds are approximately 60,
                // then we just add another minute
                if (secondPrecision == 0 && (int)seconds == 60)
                {
                    minutes++;
                    seconds = 0;
                }

                // Appends textual representation of minutes
                timeImage.Append(' ');
                timeImage.Append(minutes);
                timeImage.Append(' ');

                if (minutes == 1)
                    timeImage.Append(timeNames[TimeName.Minute]);
                else
                    timeImage.Append(timeNames[TimeName.Minutes]);
            }

            // Handles remaining seconds
            if (secondPrecision == 0)
            {
                // No fractional seconds requested. Rounds seconds to nearest integer
                int wholeSeconds = (int)seconds;

                if (wholeSeconds > 0)
                {
                    // Appends textual representation of whole seconds
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
                // Handles fractional seconds request
                if (seconds > 0)
                {
                    if (secondPrecision < 0)
                    {
                        // If second display has been disabled and less than 60 seconds remain, we still need
                        // to show something
                        if (timeImage.Length == 0)
                            timeImage.Append(timeNames[TimeName.LessThan60Seconds]);
                    }
                    else
                    {
                        // Appends textual representation of fractional seconds
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

            // Handles zero seconds display
            if (timeImage.Length == 0)
                timeImage.Append(timeNames[TimeName.NoSeconds]);

            return timeImage.ToString().Trim();
        }

        #region [ Numeric Interface Implementations ]

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        /// <exception cref="ArgumentException">value is not an <see cref="Int64"/> or <see cref="Ticks"/>.</exception>
        public int CompareTo(object value)
        {
            if (value == null) return 1;

            if (!(value is long) && !(value is Ticks) && !(value is DateTime) && !(value is TimeSpan))
                throw new ArgumentException("Argument must be an Int64 or a Ticks");

            long num = (Ticks)value;
            return (m_value < num ? -1 : (m_value > num ? 1 : 0));
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Ticks"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="Ticks"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(Ticks value)
        {
            return CompareTo((long)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="DateTime"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(DateTime value)
        {
            return CompareTo((Ticks)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="TimeSpan"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">A <see cref="TimeSpan"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(TimeSpan value)
        {
            return CompareTo((Ticks)value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Int64"/> and returns an indication of their
        /// relative values.
        /// </summary>
        /// <param name="value">An <see cref="Int64"/> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. Returns less than zero
        /// if this instance is less than value, zero if this instance is equal to value, or greater than zero
        /// if this instance is greater than value.
        /// </returns>
        public int CompareTo(long value)
        {
            return (m_value < value ? -1 : (m_value > value ? 1 : 0));
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        /// True if obj is an instance of <see cref="Int64"/> or <see cref="Ticks"/> and equals the value of this instance;
        /// otherwise, False.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is long || obj is Ticks || obj is DateTime || obj is TimeSpan)
                return Equals((Ticks)obj);

            return false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Ticks"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="Ticks"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(Ticks obj)
        {
            return Equals((long)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="DateTime"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(DateTime obj)
        {
            return Equals((Ticks)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="obj">A <see cref="TimeSpan"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(TimeSpan obj)
        {
            return Equals((Ticks)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Int64"/> value.
        /// </summary>
        /// <param name="obj">An <see cref="Int64"/> value to compare to this instance.</param>
        /// <returns>
        /// True if obj has the same value as this instance; otherwise, False.
        /// </returns>
        public bool Equals(long obj)
        {
            return (m_value == obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// Converts this value to its equivalent string representation, using
        /// the specified format.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format.
        /// </returns>
        public string ToString(string format)
        {
            return m_value.ToString(format);
        }

        /// <summary>
        /// Converts this value to its equivalent string representation, using
        /// the specified culture-specific format information.
        /// </summary>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by provider.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return m_value.ToString(provider);
        }

        /// <summary>
        /// Converts this value to its equivalent string representation, using
        /// specified format and culture-specific format information.
        /// </summary>
        /// <param name="format">A format specification.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// The string representation of the value of this instance as specified by format and provider.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="Int64"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Int64"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        #region [ Explicit IConvertible Implementation ]

        // These are explicitly implemented on the native System.Int64 implementations, so we do the same...

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(m_value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return m_value;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value, provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value, provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(m_value, provider);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.ChangeType(m_value, type, provider);
        }

        #endregion

        #endregion

        #endregion

        #region [ Operators ]

        #region [ Type Conversion Operators ]

        /// <summary>
        /// Implicitly converts value, represented in ticks, to a <see cref="Ticks"/>.
        /// </summary>
        public static implicit operator Ticks(Int64 value)
        {
            return new Ticks(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="DateTime"/>, to a <see cref="Ticks"/>.
        /// </summary>
        public static implicit operator Ticks(DateTime value)
        {
            return new Ticks(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="TimeSpan"/>, to a <see cref="Ticks"/>.
        /// </summary>
        public static implicit operator Ticks(TimeSpan value)
        {
            return new Ticks(value);
        }

        /// <summary>
        /// Implicitly converts value, represented as a <see cref="TimeTagBase"/>, to a <see cref="Ticks"/>.
        /// </summary>
        public static implicit operator Ticks(TimeTagBase value)
        {
            return new Ticks(value.ToDateTime());
        }

        /// <summary>
        /// Implicitly converts <see cref="Ticks"/>, represented in ticks, to an <see cref="Int64"/>.
        /// </summary>
        public static implicit operator Int64(Ticks value)
        {
            return value.m_value;
        }

        /// <summary>
        /// Implicitly converts <see cref="Ticks"/>, represented in ticks, to a <see cref="DateTime"/>.
        /// </summary>
        public static implicit operator DateTime(Ticks value)
        {
            return new DateTime(value.m_value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Ticks"/>, represented in ticks, to a <see cref="TimeSpan"/>.
        /// </summary>
        public static implicit operator TimeSpan(Ticks value)
        {
            return new TimeSpan(value.m_value);
        }

        /// <summary>
        /// Implicitly converts <see cref="Ticks"/>, represented in ticks, to an <see cref="NtpTimeTag"/>.
        /// </summary>
        public static implicit operator NtpTimeTag(Ticks value)
        {
            return new NtpTimeTag(new DateTime(value.m_value));
        }

        /// <summary>
        /// Implicitly converts <see cref="Ticks"/>, represented in ticks, to a <see cref="UnixTimeTag"/>.
        /// </summary>
        public static implicit operator UnixTimeTag(Ticks value)
        {
            return new UnixTimeTag(new DateTime(value.m_value));
        }

        #endregion

        #region [ Boolean and Bitwise Operators ]

        /// <summary>
        /// Returns true if value is not zero.
        /// </summary>
        public static bool operator true(Ticks value)
        {
            return (value.m_value != 0);
        }

        /// <summary>
        /// Returns true if value is equal to zero.
        /// </summary>
        public static bool operator false(Ticks value)
        {
            return (value.m_value == 0);
        }

        /// <summary>
        /// Returns bitwise complement of value.
        /// </summary>
        public static Ticks operator ~(Ticks value)
        {
            return ~value.m_value;
        }

        /// <summary>
        /// Returns logical bitwise AND of values.
        /// </summary>
        public static Ticks operator &(Ticks value1, Ticks value2)
        {
            return value1.m_value & value2.m_value;
        }

        /// <summary>
        /// Returns logical bitwise OR of values.
        /// </summary>
        public static Ticks operator |(Ticks value1, Ticks value2)
        {
            return value1.m_value | value2.m_value;
        }

        /// <summary>
        /// Returns logical bitwise exclusive-OR of values.
        /// </summary>
        public static Ticks operator ^(Ticks value1, Ticks value2)
        {
            return value1.m_value ^ value2.m_value;
        }

        /// <summary>
        /// Returns value after right shifts of first value by the number of bits specified by second value.
        /// </summary>
        public static Ticks operator >>(Ticks value, int shifts)
        {
            return (Ticks)(value.m_value >> shifts);
        }

        /// <summary>
        /// Returns value after left shifts of first value by the number of bits specified by second value.
        /// </summary>
        public static Ticks operator <<(Ticks value, int shifts)
        {
            return (Ticks)(value.m_value << shifts);
        }

        #endregion

        #region [ Arithmetic Operators ]

        /// <summary>
        /// Returns computed remainder after dividing first value by the second.
        /// </summary>
        public static Ticks operator %(Ticks value1, Ticks value2)
        {
            return value1.m_value % value2.m_value;
        }

        /// <summary>
        /// Returns computed sum of values.
        /// </summary>
        public static Ticks operator +(Ticks value1, Ticks value2)
        {
            return value1.m_value + value2.m_value;
        }

        /// <summary>
        /// Returns computed difference of values.
        /// </summary>
        public static Ticks operator -(Ticks value1, Ticks value2)
        {
            return value1.m_value - value2.m_value;
        }

        /// <summary>
        /// Returns computed product of values.
        /// </summary>
        public static Ticks operator *(Ticks value1, Ticks value2)
        {
            return value1.m_value * value2.m_value;
        }

        // Integer division operators

        /// <summary>
        /// Returns computed division of values.
        /// </summary>
        public static Ticks operator /(Ticks value1, Ticks value2)
        {
            return value1.m_value / value2.m_value;
        }

        // C# doesn't expose an exponent operator but some other .NET languages do,
        // so we expose the operator via its native special IL function name

        /// <summary>
        /// Returns result of first value raised to power of second value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced), SpecialName()]
        public static double op_Exponent(Ticks value1, Ticks value2)
        {
            return Math.Pow((double)value1.m_value, (double)value2.m_value);
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>Represents the largest possible value of a <see cref="Ticks"/>. This field is constant.</summary>
        public static readonly Ticks MaxValue;

        /// <summary>Represents the smallest possible value of a <see cref="Ticks"/>. This field is constant.</summary>
        public static readonly Ticks MinValue;

        // Static Constructor
        static Ticks()
        {
            MaxValue = (Ticks)long.MaxValue;
            MinValue = (Ticks)long.MinValue;
        }

        // Static Methods

        /// <summary>
        /// Converts <paramref name="value"/>, in 100-nanosecond tick intervals, to seconds.
        /// </summary>
        /// <param name="value">Number of ticks to convert to seconds.</param>
        /// <returns>Number seconds represented by specified <paramref name="value"/> in ticks.</returns>
        /// <remarks>
        /// If <paramref name="value"/> represents an instant in time, returned value will represent the number of seconds
        /// that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public static double ToSeconds(Ticks value)
        {
            return value / (double)Ticks.PerSecond;
        }

        /// <summary>
        /// Converts <paramref name="value"/>, in 100-nanosecond tick intervals, to milliseconds.
        /// </summary>
        /// <param name="value">Number of ticks to convert to milliseconds.</param>
        /// <returns>Number milliseconds represented by specified <paramref name="value"/> in ticks.</returns>
        /// <remarks>
        /// If <paramref name="value"/> represents an instant in time, returned value will represent the number of milliseconds
        /// that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public static double ToMilliseconds(Ticks value)
        {
            return value / (double)Ticks.PerMillisecond;
        }

        /// <summary>
        /// Creates a new <see cref="Ticks"/> from the specified <paramref name="value"/> in seconds.
        /// </summary>
        /// <param name="value">New <see cref="Ticks"/> value in seconds.</param>
        /// <returns>New <see cref="Ticks"/> object from the specified <paramref name="value"/> in seconds.</returns>
        public static Ticks FromSeconds(double value)
        {
            return new Ticks((long)(value * Ticks.PerSecond));
        }

        /// <summary>
        /// Creates a new <see cref="Ticks"/> from the specified <paramref name="value"/> in milliseconds.
        /// </summary>
        /// <param name="value">New <see cref="Ticks"/> value in milliseconds.</param>
        /// <returns>New <see cref="Ticks"/> object from the specified <paramref name="value"/> in milliseconds.</returns>
        public static Ticks FromMilliseconds(double value)
        {
            return new Ticks((long)(value * Ticks.PerMillisecond));
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Ticks"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>
        /// A <see cref="Ticks"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Ticks.MinValue"/> or greater than <see cref="Ticks.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Ticks Parse(string s)
        {
            return (Ticks)long.Parse(s);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its <see cref="Ticks"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <returns>
        /// A <see cref="Ticks"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Ticks.MinValue"/> or greater than <see cref="Ticks.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Ticks Parse(string s, NumberStyles style)
        {
            return (Ticks)long.Parse(s, style);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its <see cref="Ticks"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Ticks"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Ticks.MinValue"/> or greater than <see cref="Ticks.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in the correct format.</exception>
        public static Ticks Parse(string s, IFormatProvider provider)
        {
            return (Ticks)long.Parse(s, provider);
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="Ticks"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>
        /// A <see cref="Ticks"/> equivalent to the number contained in s.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="OverflowException">
        /// s represents a number less than <see cref="Ticks.MinValue"/> or greater than <see cref="Ticks.MaxValue"/>.
        /// </exception>
        /// <exception cref="FormatException">s is not in a format compliant with style.</exception>
        public static Ticks Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            return (Ticks)long.Parse(s, style, provider);
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="Ticks"/> equivalent. A return value
        /// indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Ticks"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not of the correct format, or represents a number less than <see cref="Ticks.MinValue"/> or greater than <see cref="Ticks.MaxValue"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Ticks result)
        {
            long parseResult;
            bool parseResponse;

            parseResponse = long.TryParse(s, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its
        /// <see cref="Ticks"/> equivalent. A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="style">
        /// A bitwise combination of System.Globalization.NumberStyles values that indicates the permitted format of s.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="Ticks"/> value equivalent to the number contained in s,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the s parameter is null,
        /// is not in a format compliant with style, or represents a number less than <see cref="Ticks.MinValue"/> or
        /// greater than <see cref="Ticks.MaxValue"/>. This parameter is passed uninitialized.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> object that supplies culture-specific formatting information about s.
        /// </param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// style is not a System.Globalization.NumberStyles value. -or- style is not a combination of
        /// System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber values.
        /// </exception>
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out Ticks result)
        {
            long parseResult;
            bool parseResponse;

            parseResponse = long.TryParse(s, style, provider, out parseResult);
            result = parseResult;

            return parseResponse;
        }

        #endregion        
    }
}