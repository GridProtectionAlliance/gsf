//*******************************************************************************************************
//  DateTimeExtensions.cs
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
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  06/10/2004 - J. Ritchie Carroll
//       Added SecondsToText overload to allow custom time names, e.g., 1 Min 2 Secs.
//  01/05/2005 - J. Ritchie Carroll
//       Added BaselinedTimestamp function.
//  12/21/2005 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.DateTime).
//  08/28/2006 - J. Ritchie Carroll
//       Added TimeIsValid, LocalTimeIsValid and UtcTimeIsValid functions.
//  09/15/2006 - J. Ritchie Carroll
//       Updated BaselinedTimestamp function to support multiple time intervals.
//  09/18/2006 - J. Ritchie Carroll
//       Added TicksBeyondSecond function to support high-resolution timestamp intervals.
//  07/17/2007 - J. Ritchie Carroll
//       Exposed TicksPerSecond as public shared constant.
//  08/31/2007 - Darrell Zuercher
//       Edited code comments.
//  08/22/2008 - J. Ritchie Carroll
//       Added TicksPerMillisecond constant and TicksToMilliseconds property.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C# extensions.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Globalization;

namespace TVA
{
    #region [ Enumerations ]

    /// <summary>Time intervals enumeration used by BaselinedTimestamp function.</summary>
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

    /// <summary>Defines extension functions related to Date/Time manipulation.</summary>
    public static class DateTimeExtensions
    {
        /// <summary>Determines if the specified UTC time is valid, by comparing it to the system clock.</summary>
        /// <param name="utcTime">UTC time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool UtcTimeIsValid(this DateTime utcTime, double lagTime, double leadTime)
        {
            return utcTime.Ticks.UtcTimeIsValid(lagTime, leadTime);
        }

        /// <summary>Determines if the specified UTC time ticks are valid, by comparing them to the system clock.</summary>
        /// <param name="utcTicks">Ticks of time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool UtcTimeIsValid(this long utcTicks, double lagTime, double leadTime)
        {
            return utcTicks.TimeIsValid(DateTime.UtcNow.Ticks, lagTime, leadTime);
        }

        /// <summary>Determines if the specified local time is valid, by comparing it to the system clock.</summary>
        /// <param name="localTime">Time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool LocalTimeIsValid(this DateTime localTime, double lagTime, double leadTime)
        {
            return localTime.Ticks.LocalTimeIsValid(lagTime, leadTime);
        }

        /// <summary>Determines if the specified local time ticks are valid, by comparing them to the system clock.</summary>
        /// <param name="localTicks">Ticks of time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool LocalTimeIsValid(this long localTicks, double lagTime, double leadTime)
        {
            return localTicks.TimeIsValid(DateTime.Now.Ticks, lagTime, leadTime);
        }

        /// <summary>Determines if time is valid, by comparing it to the specified current time.</summary>
        /// <param name="currentTime">Specified current time (e.g., could be Date.Now or Date.UtcNow).</param>
        /// <param name="testTime">Time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool TimeIsValid(this DateTime testTime, DateTime currentTime, double lagTime, double leadTime)
        {
            return testTime.Ticks.TimeIsValid(currentTime.Ticks, lagTime, leadTime);
        }

        /// <summary>Determines if time is valid, by comparing it to the specified current time.</summary>
        /// <param name="currentTicks">Specified ticks of current time (e.g., could be Date.Now.Ticks or
        /// Date.UtcNow.Ticks).</param>
        /// <param name="testTicks">Ticks of time to test for validity.</param>
        /// <param name="lagTime">The allowed lag time, in seconds, before assuming time is too old to be valid.</param>
        /// <param name="leadTime">The allowed lead time, in seconds, before assuming time is too advanced to be
        /// valid.</param>
        /// <returns>True, if time is within the specified range.</returns>
        /// <remarks>
        /// <para>Time is considered valid if it exists within the specified lag time/lead time range of current
        /// time.</para>
        /// <para>Note that lag time and lead time must be greater than zero, but can be set to sub-second
        /// intervals.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime and LeadTime must be greater than zero, but can
        /// be less than one.</exception>
        public static bool TimeIsValid(this long testTicks, long currentTicks, double lagTime, double leadTime)
        {
            if (lagTime <= 0) throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one");
            if (leadTime <= 0) throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one");

            double distance = Ticks.ToSeconds(currentTicks - testTicks);

            return (distance >= -leadTime && distance <= lagTime);
        }

        /// <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
        /// <param name="ticks">Ticks of timestamp to evaluate.</param>
        /// <returns>Timestamp's tick distance from the top of the second.</returns>
        public static long TicksBeyondSecond(this long ticks)
        {
            // Removed function call to BaselinedTimestamp just as an optimization...
            return ticks - (ticks - ticks % Ticks.PerSecond);
            //return ticks - BaselinedTimestamp(ticks, BaselineTimeInterval.Second);
        }

        /// <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
        /// <param name="timestamp">Timestamp to evaluate.</param>
        /// <returns>Timestamp's tick distance from the top of the second.</returns>
        public static long TicksBeyondSecond(this DateTime timestamp)
        {
            return TicksBeyondSecond(timestamp.Ticks);
            //return timestamp.Ticks - BaselinedTimestamp(timestamp.Ticks, BaselineTimeInterval.Second);
        }

        /// <summary>Creates a baselined timestamp which begins at the specified time interval.</summary>
        /// <param name="ticks">Ticks of timestamp to baseline.</param>
        /// <param name="baselineTo">Time interval to which timestamp should be baselined.</param>
        /// <returns>Baselined timestamp, in ticks, which begins at the specified time interval.</returns>
        /// <remarks>
        /// <para>Baselining to the second would return the timestamp starting at zero milliseconds.</para>
        /// <para>Baselining to the minute would return the timestamp starting at zero seconds and milliseconds.</para>
        /// <para>Baselining to the hour would return the timestamp starting at zero minutes, seconds and
        /// milliseconds.</para>
        /// <para>Baselining to the day would return the timestamp starting at zero hours, minutes, seconds and
        /// milliseconds.</para>
        /// <para>Baselining to the month would return the timestamp starting at day one, zero hours, minutes,
        /// seconds and milliseconds.</para>
        /// <para>Baselining to the year would return the timestamp starting at month one, day one, zero hours,
        /// minutes, seconds and milliseconds.</para>
        /// </remarks>
        public static long BaselinedTimestamp(this long ticks, BaselineTimeInterval baselineTo)
        {
            switch (baselineTo)
            {
                case BaselineTimeInterval.Second:
                    return ticks - ticks % Ticks.PerSecond;
                case BaselineTimeInterval.Minute:
                    return ticks - ticks % Ticks.PerMinute;
                case BaselineTimeInterval.Hour:
                    return ticks - ticks % Ticks.PerHour;
                case BaselineTimeInterval.Day:
                    return ticks - ticks % Ticks.PerDay;
                case BaselineTimeInterval.Month:
                    DateTime toMonth = new DateTime(ticks);
                    return new DateTime(toMonth.Year, toMonth.Month, 1, 0, 0, 0, 0).Ticks;
                case BaselineTimeInterval.Year:
                    DateTime toYear = new DateTime(ticks);
                    return new DateTime(toYear.Year, 1, 1, 0, 0, 0, 0).Ticks;
                default:
                    return ticks;
            }
        }

        /// <summary>Creates a baselined timestamp which begins at the specified time interval.</summary>
        /// <param name="timestamp">Timestamp to baseline.</param>
        /// <param name="baselineTo">Time interval to which timestamp should be baselined.</param>
        /// <returns>Baselined timestamp which begins at the specified time interval.</returns>
        /// <remarks>
        /// <para>Baselining to the second would return the timestamp starting at zero milliseconds.</para>
        /// <para>Baselining to the minute would return the timestamp starting at zero seconds and milliseconds.</para>
        /// <para>Baselining to the hour would return the timestamp starting at zero minutes, seconds and
        /// milliseconds.</para>
        /// <para>Baselining to the day would return the timestamp starting at zero hours, minutes, seconds and
        /// milliseconds.</para>
        /// <para>Baselining to the month would return the timestamp starting at day one, zero hours, minutes,
        /// seconds and milliseconds.</para>
        /// <para>Baselining to the year would return the timestamp starting at month one, day one, zero hours,
        /// minutes, seconds and milliseconds.</para>
        /// </remarks>
        public static DateTime BaselinedTimestamp(this DateTime timestamp, BaselineTimeInterval baselineTo)
        {
            return new DateTime(timestamp.Ticks.BaselinedTimestamp(baselineTo));
        }

        /// <summary>Converts given local time to Eastern time.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to Eastern time.</param>
        /// <returns>
        /// <para>Timestamp in Eastern time.</para>
        /// </returns>
        public static DateTime LocalTimeToEasternTime(this DateTime timestamp)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, USTimeZones.EasternTimeZone);
        }

        /// <summary>Converts given local time to Central time.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to Central time.</param>
        /// <returns>
        /// <para>Timestamp in Central time.</para>
        /// </returns>
        public static DateTime LocalTimeToCentralTime(this DateTime timestamp)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, USTimeZones.CentralTimeZone);
        }

        /// <summary>Converts given local time to Mountain time.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to Mountain time.</param>
        /// <returns>
        /// <para>Timestamp in Mountain time.</para>
        /// </returns>
        public static DateTime LocalTimeToMountainTime(this DateTime timestamp)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, USTimeZones.MountainTimeZone);
        }

        /// <summary>Converts given local time to Pacific time.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to Pacific time.</param>
        /// <returns>
        /// <para>Timestamp in Pacific time.</para>
        /// </returns>
        public static DateTime LocalTimeToPacificTime(this DateTime timestamp)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, USTimeZones.PacificTimeZone);
        }

        /// <summary>Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time).</summary>
        /// <remarks>This function is only provided for the sake of completeness. All it does is call the
        /// "ToUniversalTime" property on the given timestamp.</remarks>
        /// <param name="timestamp">Timestamp in local time to be converted to Universal time.</param>
        /// <returns>
        /// <para>Timestamp in UniversalTime (a.k.a., GMT).</para>
        /// </returns>
        public static DateTime LocalTimeToUniversalTime(this DateTime timestamp)
        {
            return timestamp.ToUniversalTime();
        }

        /// <summary>Converts given local time to time in specified time zone.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to time in specified time zone.</param>
        /// <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given
        /// timestamp.</param>
        /// <returns>
        /// <para>Timestamp in specified time zone.</para>
        /// </returns>
        public static DateTime LocalTimeTo(this DateTime timestamp, string destinationTimeZoneStandardName)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById(destinationTimeZoneStandardName));
        }

        /// <summary>Converts given local time to time in specified time zone.</summary>
        /// <param name="timestamp">Timestamp in local time to be converted to time in specified time zone.</param>
        /// <param name="destinationTimeZone">Desired end time zone for given timestamp.</param>
        /// <returns>
        /// <para>Timestamp in specified time zone.</para>
        /// </returns>
        public static DateTime LocalTimeTo(this DateTime timestamp, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.Local, destinationTimeZone);
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to Eastern time timestamp.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <returns>The timestamp in Eastern time.</returns>
        public static DateTime UniversalTimeToEasternTime(this DateTime universalTimestamp)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, USTimeZones.EasternTimeZone);
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to Central time timestamp.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <returns>The timestamp in Central time.</returns>
        public static DateTime UniversalTimeToCentralTime(this DateTime universalTimestamp)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, USTimeZones.CentralTimeZone);
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to Mountain time timestamp.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <returns>The timestamp in Mountain time.</returns>
        public static DateTime UniversalTimeToMountainTime(this DateTime universalTimestamp)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, USTimeZones.MountainTimeZone);
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to Pacific time timestamp.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <returns>The timestamp in Pacific time.</returns>
        public static DateTime UniversalTimeToPacificTime(this DateTime universalTimestamp)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, USTimeZones.PacificTimeZone);
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <param name="destinationTimeZoneStandardName">The time zone standard name to which the Universally
        /// Coordinated Time timestamp is to be converted to.</param>
        /// <returns>The timestamp in the specified time zone.</returns>
        public static DateTime UniversalTimeTo(this DateTime universalTimestamp, string destinationTimeZoneStandardName)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById(destinationTimeZoneStandardName));
        }

        /// <summary>
        /// Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
        /// </summary>
        /// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
        /// <param name="destinationTimeZone">The time zone to which the Universally Coordinated Time timestamp
        /// is to be converted to.</param>
        /// <returns>The timestamp in the specified time zone.</returns>
        public static DateTime UniversalTimeTo(this DateTime universalTimestamp, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(universalTimestamp, TimeZoneInfo.Utc, destinationTimeZone);
        }

        /// <summary>Converts given timestamp from one time zone to another using standard names for time zones.</summary>
        /// <param name="timestamp">Timestamp in source time zone to be converted to time in destination time zone.</param>
        /// <param name="sourceTimeZoneStandardName">Standard name of time zone for given source timestamp.</param>
        /// <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given source
        /// timestamp.</param>
        /// <returns>
        /// <para>Timestamp in destination time zone.</para>
        /// </returns>
        public static DateTime TimeZoneToTimeZone(this DateTime timestamp, string sourceTimeZoneStandardName, string destinationTimeZoneStandardName)
        {
            return TimeZoneInfo.ConvertTime(timestamp, TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneStandardName), TimeZoneInfo.FindSystemTimeZoneById(destinationTimeZoneStandardName));
        }

        /// <summary>Converts given timestamp from one time zone to another.</summary>
        /// <param name="timestamp">Timestamp in source time zone to be converted to time in destination time
        /// zone.</param>
        /// <param name="sourceTimeZone">Time zone for given source timestamp.</param>
        /// <param name="destinationTimeZone">Desired end time zone for given source timestamp.</param>
        /// <returns>
        /// <para>Timestamp in destination time zone.</para>
        /// </returns>
        public static DateTime TimeZoneToTimeZone(this DateTime timestamp, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(timestamp, sourceTimeZone, destinationTimeZone);
        }

        /// <summary>Gets the abbreviated month name for month of the timestamp.</summary>
        /// <param name="timestamp">Timestamp from which month name is extracted.</param>
        public static string AbbreviatedMonthName(this DateTime timestamp)
        {
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(timestamp.Month);
        }

        /// <summary>Gets the full month name for month of the timestamp.</summary>
        /// <param name="timestamp">Timestamp from which month name is extracted.</param>
        public static string MonthName(this DateTime timestamp)
        {
            return DateTimeFormatInfo.CurrentInfo.GetMonthName(timestamp.Month);
        }

        /// <summary>Gets the abbreviated weekday name for weekday of the timestamp.</summary>
        /// <param name="timestamp">Timestamp from which weekday name is extracted.</param>
        public static string AbbreviatedWeekdayName(this DateTime timestamp)
        {
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(timestamp.DayOfWeek);
        }

        /// <summary>Gets the shortest weekday name for weekday of the timestamp.</summary>
        /// <param name="timestamp">Timestamp from which weekday name is extracted.</param>
        public static string ShortWeekdayName(this DateTime timestamp)
        {
            return DateTimeFormatInfo.CurrentInfo.GetShortestDayName(timestamp.DayOfWeek);
        }

        /// <summary>Gets the full weekday name for weekday of the timestamp.</summary>
        /// <param name="timestamp">Timestamp from which weekday name is extracted.</param>
        public static string WeekdayName(this DateTime timestamp)
        {
            return DateTimeFormatInfo.CurrentInfo.GetDayName(timestamp.DayOfWeek);
        }
    }
}