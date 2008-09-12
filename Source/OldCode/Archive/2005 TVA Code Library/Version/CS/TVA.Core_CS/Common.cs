//*******************************************************************************************************
//  TVA.Common.vb - Globally available common functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This is the location for handy miscellaneous functions that are difficult to categorize elsewhere
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/03/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TVA.Collections;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>Defines common global functions.</summary>
namespace TVA
{
    public static class Common
    {
        /// <summary>Returns one of two strongly-typed objects.</summary>
        /// <returns>One of two objects, depending on the evaluation of given expression.</returns>
        /// <param name="expression">The expression you want to evaluate.</param>
        /// <param name="truePart">Returned if expression evaluates to True.</param>
        /// <param name="falsePart">Returned if expression evaluates to False.</param>
        /// <typeparam name="T">Return type used for immediate expression</typeparam>
        /// <remarks>
        /// <para>This function acts as a strongly-typed immediate if (a.k.a. inline if).</para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET as a strongly-typed IIf replacement.</para>
        /// </remarks>
        public static T IIf<T>(bool expression, T truePart, T falsePart)
        {
            return (expression ? truePart : falsePart);
        }

        /// <summary>Creates a strongly-typed Array.</summary>
        /// <returns>New array of specified type.</returns>
        /// <param name="length">Desired length of new array.</param>
        /// <typeparam name="T">Return type for new array.</typeparam>
        /// <remarks>
        /// <para>
        /// The Array.CreateInstance provides better performance and more direct CLR access for array creation (not to
        /// mention less confusion on the matter of array lengths), however the returned System.Array is not typed properly.
        /// This function properly casts the return array based on the the type specification helping when Option Strict is
        /// enabled.
        /// </para>
        /// <para>
        /// Examples:
        /// <code>
        ///     Dim buffer As Byte() = CreateArray(Of Byte)(12)
        ///     Dim matrix As Integer()() = CreateArray(Of Integer())(10)
        /// </code>
        /// </para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET.</para>
        /// </remarks>
        public static T[] CreateArray<T>(int length)
        {
            // The following provides better performance than "Return New T(length) {}".
            return (T[])Array.CreateInstance(typeof(T), length);
        }

        /// <summary>Creates a strongly-typed Array with an initial value parameter.</summary>
        /// <returns>New array of specified type.</returns>
        /// <param name="length">Desired length of new array.</param>
        /// <param name="initialValue">Value used to initialize all array elements.</param>
        /// <typeparam name="T">Return type for new array.</typeparam>
        /// <remarks>
        /// <para>
        /// Examples:
        /// <code>
        ///     Dim elements As Integer() = CreateArray(12, -1)
        ///     Dim names As String() = CreateArray(100, "undefined")
        /// </code>
        /// </para>
        /// <para>It is expected that this function will only be used in Visual Basic.NET.</para>
        /// </remarks>
        public static T[] CreateArray<T>(int length, T initialValue)
        {
            T[] typedArray = CreateArray<T>(length);

            // Initializes all elements with the default value.
            for (int x = 0; x <= typedArray.Length - 1; x++)
            {
                typedArray[x] = initialValue;
            }

            return typedArray;
        }

        /// <summary>
        /// Gets the root type in the inheritace hierarchy from which the specified type inherits.
        /// </summary>
        /// <param name="type">The System.Type whose root type is to be found.</param>
        /// <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
        /// <remarks>Unless input type is System.Object, the returned type will never be System.Object, even though all types ultimately inherit from it.</remarks>
        public static Type GetRootType(Type type)
		{
            // Recurse through types until you reach a base type of "System.Object"
            if (type.BaseType != typeof(object)) return GetRootType(type.BaseType);
            return type;
        }

        /// <summary>
        /// Gets the type of the currently executing application.
        /// </summary>
        /// <returns>One of the TVA.ApplicationType values.</returns>
        public static ApplicationType GetApplicationType()
        {
            if (System.Web.HttpContext.Current == null)
            {
                try
                {
                    // References:
                    // - http://support.microsoft.com/kb/65122
                    // - http://support.microsoft.com/kb/90493/en-us
                    // - http://www.codeguru.com/cpp/w-p/system/misc/article.php/c2897/
                    // We will always have an entry assembly for windows application.
                    FileStream exe = new FileStream(TVA.AssemblyInformation.EntryAssembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    byte[] dosHeader = new byte[64];
                    byte[] exeHeader = new byte[248];
                    byte[] subSystem = new byte[2];
                    exe.Read(dosHeader, 0, dosHeader.Length);
                    exe.Seek(BitConverter.ToInt16(dosHeader, 60), SeekOrigin.Begin);
                    exe.Read(exeHeader, 0, exeHeader.Length);
                    exe.Close();

                    Array.Copy(exeHeader, 92, subSystem, 0, 2);

                    return ((ApplicationType)(BitConverter.ToInt16(subSystem, 0)));
                }
                catch (Exception)
                {
                    // We are unable to determine the application type. This is possible in case of a web app/web site
                    // when this method is being called from a thread other than the main thread, in which case the
                    // System.Web.HttpContext.Current property, used to determine if it is a web app, will not be set.
                    return ApplicationType.Unknown;
                }
            }
            else
            {
                return ApplicationType.Web;
            }
        }

        /// <summary>Determines if given item is a reference type.</summary>
        public static bool IsReference(object item)
        {
            return !(item is ValueType);
        }

        /// <summary>Determines if given item is a reference type but not a string.</summary>
        public static bool IsNonStringReference(object item)
        {
            return (IsReference(item) && !(item is string));
        }


/*-----------------------------------------------------------------------------------------------------*\
 *
 *                    These functions were migrated here from TVA.Collections.Common
 *                  
\*-----------------------------------------------------------------------------------------------------*/


        /// <summary>Returns the smallest item from a list of parameters.</summary>
        public static object Min(params object[] itemList)
        {
            return itemList.Min<object>(Compare);
        }

        /// <summary>Returns the smallest item from a list of parameters.</summary>
        public static T Min<T>(params T[] itemList)
        {
            return itemList.Min<T>();
        }

        /// <summary>Returns the largest item from a list of parameters.</summary>
        public static object Max(params object[] itemList)
        {
            return itemList.Max<object>(Compare);
        }

        /// <summary>Returns the largest item from a list of parameters.</summary>
        public static T Max<T>(params T[] itemList)
        {
            return itemList.Max<T>();
        }

        /// <summary>Compares two elements of the specified type.</summary>
        public static int Compare<T>(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }

        /// <summary>Compares two elements of any type.</summary>
        public static int Compare(object x, object y)
        {
            if (IsReference(x) && IsReference(y))
            {
                // If both items are reference objects, then test object equality by reference.
                // If not equal by overridable Object.Equals function, use default Comparer.
                if (x == y)
                {
                    return 0;
                }
                else if (x.GetType().Equals(y.GetType()))
                {
                    // Compares two items that are the same type. Sees if the type supports IComparable interface.
                    if (x is IComparable)
                    {
                        return ((IComparable)x).CompareTo(y);
                    }
                    else if (x.Equals(y))
                    {
                        return 0;
                    }
                    else
                    {
                        return Comparer.Default.Compare(x, y);
                    }
                }
                else
                {
                    return Comparer.Default.Compare(x, y);
                }
            }
            else
            {
                // Compares non-reference (i.e., value) types, using VB rules.
                // ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
                return (Operators.ConditionalCompareObjectLess(x, y, false) ? -1 : (Operators.ConditionalCompareObjectGreater(x, y, false) ? 1 : 0));
            }
        }


/*-----------------------------------------------------------------------------------------------------*\
 *
 *                    These functions were migrated here from TVA.DateTime.Common
 *                  
\*-----------------------------------------------------------------------------------------------------*/


        /// <summary>Time names enumeration used by SecondsToText function.</summary>
        public static struct TimeName
        {
            static public int Year = 0;
            static public int Years = 1;
            static public int Day = 2;
            static public int Days = 3;
            static public int Hour = 4;
            static public int Hours = 5;
            static public int Minute = 6;
            static public int Minutes = 7;
            static public int Second = 8;
            static public int Seconds = 9;
            static public int LessThan60Seconds = 10;
            static public int NoSeconds = 11;
        }

        /// <summary>Number of 100-nanosecond ticks in one second.</summary>
        public const long TicksPerSecond = 10000000;

        /// <summary>Number of 100-nanosecond ticks in one millisecond.</summary>
        public const long TicksPerMillisecond = TicksPerSecond / 1000;

        /// <summary>Standard time names used by SecondsToText function.</summary>
        private static string[] m_standardTimeNames = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 60 Seconds", "0 Seconds" };

        /// <summary>Standard time names, without seconds, used by SecondsToText function.</summary>
        private static string[] m_standardTimeNamesWithoutSeconds = new string[] { "Year", "Years", "Day", "Days", "Hour", "Hours", "Minute", "Minutes", "Second", "Seconds", "Less Than 1 Minute", "0 Minutes" };

        /// <summary>Converts 100-nanosecond tick intervals to seconds.</summary>
        public static double TicksToSeconds(long ticks)
        {
            return ticks / TicksPerSecond;
        }

        /// <summary>Converts 100-nanosecond tick intervals to milliseconds.</summary>
        public static double TicksToMilliseconds(long ticks)
        {
            return ticks / TicksPerMillisecond;
        }

        /// <summary>Converts seconds to 100-nanosecond tick intervals.</summary>
        public static long SecondsToTicks(double seconds)
        {
            return (int)(seconds * TicksPerSecond);
        }

        /// <summary>Converts milliseconds to 100-nanosecond tick intervals.</summary>
        public static long MillisecondsToTicks(double milliseconds)
        {
            return (int)(milliseconds * TicksPerMillisecond);
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
                return SecondsToText(seconds, secondPrecision, m_standardTimeNamesWithoutSeconds);
            else
                return SecondsToText(seconds, secondPrecision, m_standardTimeNames);
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
            StringBuilder timeImage = new StringBuilder();

            int years; // 1 year   = 365.2425 days or 31556952 seconds
            int days; // 1 day    = 86400 seconds
            int hours; // 1 hour   = 3600 seconds
            int minutes; // 1 minute = 60 seconds

            // checks if number of seconds ranges in years.
            years = (int)(seconds / 31556952);

            if (years >= 1)
            {
                // Removes whole years from remaining seconds.
                seconds = seconds - years * 31556952;

                // Appends textual representation of years.
                timeImage.Append(years);
                timeImage.Append(' ');
                if (years == 1)
                {
                    timeImage.Append(timeNames[TimeName.Year]);
                }
                else
                {
                    timeImage.Append(timeNames[TimeName.Years]);
                }
            }

            // Checks if remaining number of seconds ranges in days.
            days = (int)(seconds / 86400);
            if (days >= 1)
            {
                // Removes whole days from remaining seconds.
                seconds = seconds - days * 86400;

                // Appends textual representation of days.
                timeImage.Append(' ');
                timeImage.Append(days);
                timeImage.Append(' ');
                if (days == 1)
                {
                    timeImage.Append(timeNames[TimeName.Day]);
                }
                else
                {
                    timeImage.Append(timeNames[TimeName.Days]);
                }
            }

            // Checks if remaining number of seconds ranges in hours.
            hours = (int)(seconds / 3600);
            if (hours >= 1)
            {
                // Removes whole hours from remaining seconds.
                seconds = seconds - hours * 3600;

                // Appends textual representation of hours.
                timeImage.Append(' ');
                timeImage.Append(hours);
                timeImage.Append(' ');
                if (hours == 1)
                {
                    timeImage.Append(timeNames[TimeName.Hour]);
                }
                else
                {
                    timeImage.Append(timeNames[TimeName.Hours]);
                }
            }

            // Checks if remaining number of seconds ranges in minutes.
            minutes = (int)(seconds / 60);
            if (minutes >= 1)
            {
                // Removes whole minutes from remaining seconds.
                seconds = seconds - minutes * 60;

                // Appends textual representation of minutes.
                timeImage.Append(' ');
                timeImage.Append(minutes);
                timeImage.Append(' ');
                if (minutes == 1)
                {
                    timeImage.Append(timeNames[TimeName.Minute]);
                }
                else
                {
                    timeImage.Append(timeNames[TimeName.Minutes]);
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
                    timeImage.Append(' ');
                    timeImage.Append(wholeSeconds);
                    timeImage.Append(' ');
                    if (wholeSeconds == 1)
                    {
                        timeImage.Append(timeNames[TimeName.Second]);
                    }
                    else
                    {
                        timeImage.Append(timeNames[TimeName.Seconds]);
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
                        if (timeImage.Length == 0)
                        {
                            timeImage.Append(timeNames[TimeName.LessThan60Seconds]);
                        }
                    }
                    else
                    {
                        // Appends textual representation of fractional seconds.
                        timeImage.Append(' ');
                        timeImage.Append(seconds.ToString("0." + (new string('0', secondPrecision))));
                        timeImage.Append(' ');
                        if (seconds == 1)
                        {
                            timeImage.Append(timeNames[TimeName.Second]);
                        }
                        else
                        {
                            timeImage.Append(timeNames[TimeName.Seconds]);
                        }
                    }
                }
            }

            // Handles zero seconds display.
            if (timeImage.Length == 0)
            {
                timeImage.Append(timeNames[TimeName.NoSeconds]);
            }

            return timeImage.ToString().Trim();

        }

    	/// <summary>Returns the specified Win32 time zone, using specified name.</summary>
		/// <param name="name">Value of name used for time zone lookup.</param>
		/// <param name="lookupBy">Type of name used for time zone lookup.</param>
        public static TimeZoneInfo GetTimeZone(string name, TimeZoneName lookupBy)
		{
            foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
			{
				if (lookupBy == TimeZoneName.DaylightName)
				{
					if (string.Compare(timeZone.DaylightName, name, true) == 0)
					{
						return timeZone;
					}
				}
				else if (lookupBy == TimeZoneName.DisplayName)
				{
					if (string.Compare(timeZone.DisplayName, name, true) == 0)
					{
						return timeZone;
					}
				}
				else if (lookupBy == TimeZoneName.StandardName)
				{
					if (string.Compare(timeZone.StandardName, name, true) == 0)
					{
						return timeZone;
					}
				}
			}
			
			throw new ArgumentException("Windows time zone with " + lookupBy + " of \"" + name + "\" was not found!");
		}
    }
}