using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;

//*******************************************************************************************************
//  TVA.DateTime.Common.vb - Common Date/Time Functions
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
//
//*******************************************************************************************************


namespace TVA
{
	namespace DateTime
	{
		
		/// <summary>Defines common global functions related to Date/Time manipulation.</summary>
		public sealed class Common
		{
			
			
			/// <summary>Number of 100-nanosecond ticks in one second.</summary>
			public const long TicksPerSecond = 10000000;
			
			/// <summary>Standard time names used by SecondsToText function.</summary>
			private static string[] m_standardTimeNames = new string[] {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds"};
			
			/// <summary>Standard time names, without seconds, used by SecondsToText function.</summary>
			private static string[] m_standardTimeNamesWithoutSeconds = new string[] {"Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes"};
			
			// We define a few common timezones for convenience.
			private static Win32TimeZone m_universalTimeZone;
			private static Win32TimeZone m_easternTimeZone;
			private static Win32TimeZone m_centralTimeZone;
			private static Win32TimeZone m_mountainTimeZone;
			private static Win32TimeZone m_pacificTimeZone;
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated
				
			}
			
			/// <summary>Converts 100-nanosecond tick intervals to seconds.</summary>
			public static double TicksToSeconds(long ticks)
			{
				return ticks / TicksPerSecond;
			}
			
			/// <summary>Converts seconds to 100-nanosecond tick intervals.</summary>
			public static long SecondsToTicks(double seconds)
			{
				return Convert.ToInt64(seconds * TicksPerSecond);
			}
			
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
			public static bool UtcTimeIsValid(DateTime utcTime, double lagTime, double leadTime)
			{
				
				return UtcTimeIsValid(utcTime.Ticks, lagTime, leadTime);
				
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
			public static bool UtcTimeIsValid(long utcTicks, double lagTime, double leadTime)
			{
				
				return TimeIsValid(DateTime.UtcNow.Ticks, utcTicks, lagTime, leadTime);
				
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
			public static bool LocalTimeIsValid(DateTime localTime, double lagTime, double leadTime)
			{
				
				return LocalTimeIsValid(localTime.Ticks, lagTime, leadTime);
				
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
			public static bool LocalTimeIsValid(long localTicks, double lagTime, double leadTime)
			{
				
				return TimeIsValid(DateTime.Now.Ticks, localTicks, lagTime, leadTime);
				
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
			public static bool TimeIsValid(DateTime currentTime, DateTime testTime, double lagTime, double leadTime)
			{
				
				return TimeIsValid(currentTime.Ticks, testTime.Ticks, lagTime, leadTime);
				
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
			public static bool TimeIsValid(long currentTicks, long testTicks, double lagTime, double leadTime)
			{
				
				if (lagTime <= 0)
				{
					throw (new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one"));
				}
				if (leadTime <= 0)
				{
					throw (new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one"));
				}
				
				double distance = TicksToSeconds(currentTicks - testTicks);
				return (distance >= - leadTime && distance <= lagTime);
				
			}
			
			/// <summary>Gets  the number of seconds in the local timezone, including fractional seconds, that have
			/// elapsed since 12:00:00 midnight, January 1, 0001.</summary>
			public static double SystemTimer
			{
				get
				{
					return TicksToSeconds(DateTime.Now.Ticks);
				}
			}
			
			/// <summary>Gets the number of seconds in the universally coordinated timezone, including fractional
			/// seconds, that have elapsed since 12:00:00 midnight, January 1, 0001.</summary>
			public static double UtcSystemTimer
			{
				get
				{
					return TicksToSeconds(DateTime.UtcNow.Ticks);
				}
			}
			
			/// <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
			/// <param name="ticks">Ticks of timestamp to evaluate.</param>
			/// <returns>Timestamp's tick distance from the top of the second.</returns>
			public static long TicksBeyondSecond(long ticks)
			{
				return ticks - BaselinedTimestamp(new DateTime(ticks), BaselineTimeInterval.Second).Ticks;
			}
			
			/// <summary>Gets the distance, in ticks, beyond the top of the timestamp second.</summary>
			/// <param name="timestamp">Timestamp to evaluate.</param>
			/// <returns>Timestamp's tick distance from the top of the second.</returns>
			public static long TicksBeyondSecond(DateTime timestamp)
			{
				return timestamp.Ticks - BaselinedTimestamp(timestamp, BaselineTimeInterval.Second).Ticks;
			}
			
			/// <summary>Removes any milliseconds from a timestamp value, to baseline the time at the bottom of the
			/// second.</summary>
			/// <param name="ticks">Ticks of timestamp to baseline.</param>
			/// <param name="baselineTo">Time interval to which timestamp should be baselined.</param>
			public static DateTime BaselinedTimestamp(long ticks, BaselineTimeInterval baselineTo)
			{
				
				return BaselinedTimestamp(new DateTime(ticks), baselineTo);
				
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
			public static DateTime BaselinedTimestamp(DateTime timestamp, BaselineTimeInterval baselineTo)
			{
				
				DateTime with_1 = timestamp;
				switch (baselineTo)
				{
					case BaselineTimeInterval.Second:
						return new DateTime(with_1.Year, with_1.Month, with_1.Day, with_1.Hour, with_1.Minute, with_1.Second, 0);
					case BaselineTimeInterval.Minute:
						return new DateTime(with_1.Year, with_1.Month, with_1.Day, with_1.Hour, with_1.Minute, 0, 0);
					case BaselineTimeInterval.Hour:
						return new DateTime(with_1.Year, with_1.Month, with_1.Day, with_1.Hour, 0, 0, 0);
					case BaselineTimeInterval.Day:
						return new DateTime(with_1.Year, with_1.Month, with_1.Day, 0, 0, 0, 0);
					case BaselineTimeInterval.Month:
						return new DateTime(with_1.Year, with_1.Month, 1, 0, 0, 0, 0);
					case BaselineTimeInterval.Year:
						return new DateTime(with_1.Year, 1, 1, 0, 0, 0, 0);
					default:
						return timestamp;
				}
				
			}
			
			/// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
			/// and whole integer seconds.</summary>
			/// <param name="seconds">Seconds to be converted.</param>
			public static string SecondsToText(double seconds)
			{
				
				return SecondsToText(seconds, 0);
				
			}
			
			/// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
			/// and seconds.</summary>
			/// <param name="seconds">Seconds to be converted.</param>
			/// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
			/// <remarks>Set second precision to -1 to suppress seconds display.</remarks>
			public static string SecondsToText(double seconds, int secondPrecision)
			{
				
				if (secondPrecision < 0)
				{
					return SecondsToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds);
				}
				else
				{
					return SecondsToText(seconds, secondPrecision, m_standardTimeNames);
				}
				
			}
			
			/// <summary>Turns the given number of seconds into textual representation of years, days, hours, minutes
			/// and seconds given string array of time names. Need one for each TimeName enum item.</summary>
			/// <param name="seconds">Seconds to be converted.</param>
			/// <param name="secondPrecision">Number of fractional digits to display for seconds.</param>
			/// <param name="timeNames">Time names array to use during textal conversion.</param>
			/// <remarks>
			/// <para>Set second precision to -1 to suppress seconds display.</para>
			/// <para>Time names array needs one string entry per element in <see cref="TimeName">TimeName</see>
			/// enumeration.</para>
			/// <para>Example timeNames array: "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes",
			/// "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds".</para>
			/// </remarks>
			public static string SecondsToText(double seconds, int secondPrecision, string[] timeNames)
			{
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				int years; // 1 year   = 365.2425 days or 31556952 seconds
				int days; // 1 day    = 86400 seconds
				int hours; // 1 hour   = 3600 seconds
				int minutes; // 1 minute = 60 seconds
				
				// checks if number of seconds ranges in years.
				years = (int) (seconds / 31556952);
				
				if (years >= 1)
				{
					// Removes whole years from remaining seconds.
					seconds = seconds - years * 31556952;
					
					// Appends textual representation of years.
					with_1.Append(years);
					with_1.Append(' ');
					if (years == 1)
					{
						with_1.Append(timeNames[TimeName.Year]);
					}
					else
					{
						with_1.Append(timeNames[TimeName.Years]);
					}
				}
				
				// Checks if remaining number of seconds ranges in days.
				days = (int) (seconds / 86400);
				if (days >= 1)
				{
					// Removes whole days from remaining seconds.
					seconds = seconds - days * 86400;
					
					// Appends textual representation of days.
					with_1.Append(' ');
					with_1.Append(days);
					with_1.Append(' ');
					if (days == 1)
					{
						with_1.Append(timeNames[TimeName.Day]);
					}
					else
					{
						with_1.Append(timeNames[TimeName.Days]);
					}
				}
				
				// Checks if remaining number of seconds ranges in hours.
				hours = (int) (seconds / 3600);
				if (hours >= 1)
				{
					// Removes whole hours from remaining seconds.
					seconds = seconds - hours * 3600;
					
					// Appends textual representation of hours.
					with_1.Append(' ');
					with_1.Append(hours);
					with_1.Append(' ');
					if (hours == 1)
					{
						with_1.Append(timeNames[TimeName.Hour]);
					}
					else
					{
						with_1.Append(timeNames[TimeName.Hours]);
					}
				}
				
				// Checks if remaining number of seconds ranges in minutes.
				minutes = (int) (seconds / 60);
				if (minutes >= 1)
				{
					// Removes whole minutes from remaining seconds.
					seconds = seconds - minutes * 60;
					
					// Appends textual representation of minutes.
					with_1.Append(' ');
					with_1.Append(minutes);
					with_1.Append(' ');
					if (minutes == 1)
					{
						with_1.Append(timeNames[TimeName.Minute]);
					}
					else
					{
						with_1.Append(timeNames[TimeName.Minutes]);
					}
				}
				
				// Handles remaining seconds.
				if (secondPrecision == 0)
				{
					// No fractional seconds requested. Rounds seconds to nearest integer.
					int wholeSeconds = Convert.ToInt32(System.Math.Round(seconds));
					
					if (wholeSeconds > 0)
					{
						// Appends textual representation of whole seconds.
						with_1.Append(' ');
						with_1.Append(wholeSeconds);
						with_1.Append(' ');
						if (wholeSeconds == 1)
						{
							with_1.Append(timeNames[TimeName.Second]);
						}
						else
						{
							with_1.Append(timeNames[TimeName.Seconds]);
						}
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
							if (with_1.Length == 0)
							{
								with_1.Append(timeNames[TimeName.LessThan60Seconds]);
							}
						}
						else
						{
							// Appends textual representation of fractional seconds.
							with_1.Append(' ');
							with_1.Append(seconds.ToString("0." + (new string("0", secondPrecision))));
							with_1.Append(' ');
							if (seconds == 1)
							{
								with_1.Append(timeNames[TimeName.Second]);
							}
							else
							{
								with_1.Append(timeNames[TimeName.Seconds]);
							}
						}
					}
				}
				
				// Handles zero seconds display.
				if (with_1.Length == 0)
				{
					with_1.Append(timeNames[TimeName.NoSeconds]);
				}
				
				return with_1.ToString().Trim();
				
			}
			
			// JRC - These functions were added to make time zone management classes easier to use.
			
			/// <summary>Returns the specified Win32 time zone, using its standard name.</summary>
			/// <param name="standardName">Standard name for desired Win32 time zone.</param>
			public static Win32TimeZone GetWin32TimeZone(string standardName)
			{
				
				return GetWin32TimeZone(standardName, TimeZoneName.StandardName);
				
			}
			
			/// <summary>Returns the specified Win32 time zone, using specified name.</summary>
			/// <param name="name">Value of name used for time zone lookup.</param>
			/// <param name="lookupBy">Type of name used for time zone lookup.</param>
			public static Win32TimeZone GetWin32TimeZone(string name, TimeZoneName lookupBy)
			{
				
				foreach (Win32TimeZone timeZone in TimeZones.GetTimeZones())
				{
					switch (lookupBy)
					{
						case TimeZoneName.DaylightAbbreviation:
							if (string.Compare(timeZone.DaylightAbbreviation, name, true) == 0)
							{
								return timeZone;
							}
							break;
						case TimeZoneName.DaylightName:
							if (string.Compare(timeZone.DaylightName, name, true) == 0)
							{
								return timeZone;
							}
							break;
						case TimeZoneName.DisplayName:
							if (string.Compare(timeZone.DisplayName, name, true) == 0)
							{
								return timeZone;
							}
							break;
						case TimeZoneName.StandardAbbreviation:
							if (string.Compare(timeZone.StandardAbbreviation, name, true) == 0)
							{
								return timeZone;
							}
							break;
						case TimeZoneName.StandardName:
							if (string.Compare(timeZone.StandardName, name, true) == 0)
							{
								return timeZone;
							}
							break;
					}
				}
				
				throw (new ArgumentException("Windows time zone with " + @Enum.GetName(typeof(TimeZoneName), lookupBy) + " of \"" + name + "\" was not found!"));
				
			}
			
			/// <summary>Gets Universally Coordinated Time Zone (a.k.a., Greenwich Meridian Time Zone).</summary>
			public static Win32TimeZone UniversalTimeZone
			{
				get
				{
					if (m_universalTimeZone == null)
					{
						m_universalTimeZone = GetWin32TimeZone("GMT Standard Time");
					}
					return m_universalTimeZone;
				}
			}
			
			/// <summary>Gets Eastern Time Zone.</summary>
			public static Win32TimeZone EasternTimeZone
			{
				get
				{
					if (m_easternTimeZone == null)
					{
						m_easternTimeZone = GetWin32TimeZone("Eastern Standard Time");
					}
					return m_easternTimeZone;
				}
			}
			
			/// <summary>Gets Central Time Zone.</summary>
			public static Win32TimeZone CentralTimeZone
			{
				get
				{
					if (m_centralTimeZone == null)
					{
						m_centralTimeZone = GetWin32TimeZone("Central Standard Time");
					}
					return m_centralTimeZone;
				}
			}
			
			/// <summary>Gets Mountain Time Zone.</summary>
			public static Win32TimeZone MountainTimeZone
			{
				get
				{
					if (m_mountainTimeZone == null)
					{
						m_mountainTimeZone = GetWin32TimeZone("Mountain Standard Time");
					}
					return m_mountainTimeZone;
				}
			}
			
			/// <summary>Gets Pacific Standard Time Zone.</summary>
			public static Win32TimeZone PacificTimeZone
			{
				get
				{
					if (m_pacificTimeZone == null)
					{
						m_pacificTimeZone = GetWin32TimeZone("Pacific Standard Time");
					}
					return m_pacificTimeZone;
				}
			}
			
			/// <summary>Converts given local time to Eastern time.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to Eastern time.</param>
			/// <returns>
			/// <para>Timestamp in Eastern time.</para>
			/// </returns>
			public static DateTime LocalTimeToEasternTime(DateTime localTimeStamp)
			{
				
				return LocalTimeTo(localTimeStamp, EasternTimeZone);
				
			}
			
			/// <summary>Converts given local time to Central time.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to Central time.</param>
			/// <returns>
			/// <para>Timestamp in Central time.</para>
			/// </returns>
			public static DateTime LocalTimeToCentralTime(DateTime localTimeStamp)
			{
				
				return LocalTimeTo(localTimeStamp, CentralTimeZone);
				
			}
			
			/// <summary>Converts given local time to Mountain time.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to Mountain time.</param>
			/// <returns>
			/// <para>Timestamp in Mountain time.</para>
			/// </returns>
			public static DateTime LocalTimeToMountainTime(DateTime localTimeStamp)
			{
				
				return LocalTimeTo(localTimeStamp, MountainTimeZone);
				
			}
			
			/// <summary>Converts given local time to Pacific time.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to Pacific time.</param>
			/// <returns>
			/// <para>Timestamp in Pacific time.</para>
			/// </returns>
			public static DateTime LocalTimeToPacificTime(DateTime localTimeStamp)
			{
				
				return LocalTimeTo(localTimeStamp, PacificTimeZone);
				
			}
			
			/// <summary>Converts given local time to Universally Coordinated Time (a.k.a., Greenwich Meridian Time).</summary>
			/// <remarks>This function is only provided for the sake of completeness. All it does is call the
			/// "ToUniversalTime" property on the given timestamp.</remarks>
			/// <param name="localTimestamp">Timestamp in local time to be converted to Universal time.</param>
			/// <returns>
			/// <para>Timestamp in UniversalTime (a.k.a., GMT).</para>
			/// </returns>
			public static DateTime LocalTimeToUniversalTime(DateTime localTimestamp)
			{
				
				return localTimestamp.ToUniversalTime();
				
			}
			
			/// <summary>Converts given local time to time in specified time zone.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone.</param>
			/// <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given
			/// timestamp.</param>
			/// <returns>
			/// <para>Timestamp in specified time zone.</para>
			/// </returns>
			public static DateTime LocalTimeTo(DateTime localTimestamp, string destinationTimeZoneStandardName)
			{
				
				return LocalTimeTo(localTimestamp, GetWin32TimeZone(destinationTimeZoneStandardName));
				
			}
			
			/// <summary>Converts given local time to time in specified time zone.</summary>
			/// <param name="localTimestamp">Timestamp in local time to be converted to time in specified time zone.</param>
			/// <param name="destinationTimeZone">Desired end time zone for given timestamp.</param>
			/// <returns>
			/// <para>Timestamp in specified time zone.</para>
			/// </returns>
			public static DateTime LocalTimeTo(DateTime localTimestamp, Win32TimeZone destinationTimeZone)
			{
				
				double destOffset;
				
				// Calculates exact UTC offset of destination time zone in hours.
				TimeSpan with_1 = destinationTimeZone.GetUtcOffset(localTimestamp);
				destOffset = with_1.Hours + with_1.Minutes / 60;
				
				return localTimestamp.ToUniversalTime().AddHours(destOffset);
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to Eastern time timestamp.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <returns>The timestamp in Eastern time.</returns>
			public static DateTime UniversalTimeToEasternTime(object universalTimestamp)
			{
				
				return UniversalTimeTo(universalTimestamp, EasternTimeZone);
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to Central time timestamp.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <returns>The timestamp in Central time.</returns>
			public static DateTime UniversalTimeToCentralTime(object universalTimestamp)
			{
				
				return UniversalTimeTo(universalTimestamp, CentralTimeZone);
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to Mountain time timestamp.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <returns>The timestamp in Mountain time.</returns>
			public static DateTime UniversalTimeToMountainTime(object universalTimestamp)
			{
				
				return UniversalTimeTo(universalTimestamp, MountainTimeZone);
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to Pacific time timestamp.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <returns>The timestamp in Pacific time.</returns>
			public static DateTime UniversalTimeToPacificTime(object universalTimestamp)
			{
				
				return UniversalTimeTo(universalTimestamp, PacificTimeZone);
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <param name="destinationTimeZoneStandardName">The time zone standard name to which the Universally
			/// Coordinated Time timestamp is to be converted to.</param>
			/// <returns>The timestamp in the specified time zone.</returns>
			public static DateTime UniversalTimeTo(DateTime universalTimestamp, string destinationTimeZoneStandardName)
			{
				
				return UniversalTimeTo(universalTimestamp, GetWin32TimeZone(destinationTimeZoneStandardName));
				
			}
			
			/// <summary>
			/// Converts the specified Universally Coordinated Time timestamp to timestamp in specified time zone.
			/// </summary>
			/// <param name="universalTimestamp">The Universally Coordinated Time timestamp that is to be converted.</param>
			/// <param name="destinationTimeZone">The time zone to which the Universally Coordinated Time timestamp
			/// is to be converted to.</param>
			/// <returns>The timestamp in the specified time zone.</returns>
			public static DateTime UniversalTimeTo(DateTime universalTimestamp, Win32TimeZone destinationTimeZone)
			{
				
				return destinationTimeZone.ToLocalTime(universalTimestamp);
				
			}
			
			/// <summary>Converts given timestamp from one time zone to another using standard names for time zones.</summary>
			/// <param name="timestamp">Timestamp in source time zone to be converted to time in destination time zone.</param>
			/// <param name="sourceTimeZoneStandardName">Standard name of time zone for given source timestamp.</param>
			/// <param name="destinationTimeZoneStandardName">Standard name of desired end time zone for given source
			/// timestamp.</param>
			/// <returns>
			/// <para>Timestamp in destination time zone.</para>
			/// </returns>
			public static DateTime TimeZoneToTimeZone(DateTime timestamp, string sourceTimeZoneStandardName, string destinationTimeZoneStandardName)
			{
				
				return TimeZoneToTimeZone(timestamp, GetWin32TimeZone(sourceTimeZoneStandardName), GetWin32TimeZone(destinationTimeZoneStandardName));
				
			}
			
			/// <summary>Converts given timestamp from one time zone to another.</summary>
			/// <param name="timestamp">Timestamp in source time zone to be converted to time in destination time
			/// zone.</param>
			/// <param name="sourceTimeZone">Time zone for given source timestamp.</param>
			/// <param name="destinationTimeZone">Desired end time zone for given source timestamp.</param>
			/// <returns>
			/// <para>Timestamp in destination time zone.</para>
			/// </returns>
			public static DateTime TimeZoneToTimeZone(DateTime timestamp, Win32TimeZone sourceTimeZone, Win32TimeZone destinationTimeZone)
			{
				
				double destOffset;
				
				// Calculates exact UTC offset of destination time zone in hours.
				TimeSpan with_1 = destinationTimeZone.GetUtcOffset(timestamp);
				destOffset = with_1.Hours + with_1.Minutes / 60;
				
				return sourceTimeZone.ToUniversalTime(timestamp).AddHours(destOffset);
				
			}
			
			/// <summary>Gets the 3-letter month abbreviation for given month number (1-12).</summary>
			/// <param name="monthNumber">Numeric month number (1-12).</param>
			/// <remarks>Month abbreviations are English only.</remarks>
			public static string ShortMonthName(int monthNumber)
			{
				switch (monthNumber)
				{
					case 1:
						return "Jan";
					case 2:
						return "Feb";
					case 3:
						return "Mar";
					case 4:
						return "Apr";
					case 5:
						return "May";
					case 6:
						return "Jun";
					case 7:
						return "Jul";
					case 8:
						return "Aug";
					case 9:
						return "Sep";
					case 10:
						return "Oct";
					case 11:
						return "Nov";
					case 12:
						return "Dec";
					default:
						throw (new ArgumentOutOfRangeException("monthNumber", "Invalid month number \"" + monthNumber + "\" specified - expected a value between 1 and 12"));
						break;
				}
			}
			
			/// <summary>Gets the full month name for given month number (1-12).</summary>
			/// <param name="monthNumber">Numeric month number (1-12).</param>
			/// <remarks>Month names are English only.</remarks>
			public static string LongMonthName(int monthNumber)
			{
				switch (monthNumber)
				{
					case 1:
						return "January";
					case 2:
						return "February";
					case 3:
						return "March";
					case 4:
						return "April";
					case 5:
						return "May";
					case 6:
						return "June";
					case 7:
						return "July";
					case 8:
						return "August";
					case 9:
						return "September";
					case 10:
						return "October";
					case 11:
						return "November";
					case 12:
						return "December";
					default:
						throw (new ArgumentOutOfRangeException("monthNumber", "Invalid month number \"" + monthNumber + "\" specified - expected a value between 1 and 12"));
						break;
				}
			}
			
		}
		
	}
	
}
