using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.ComponentModel;
using System.Text;
using Microsoft.Win32;

//*******************************************************************************************************
//  TVA.Date.Win32TimeZone.vb - Win32 Time Zone Classes
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
//  06/10/2004 - J. Ritchie Carroll
//       Integrated external source for Michael R. Brumm's TimeZone management into TVA.Shared.Date.
//  12/21/2005 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Date).
//       Because this code is typically not used directly, but rather through helper functions
//       defined in Common, no code comments were added to these classes.
//  12/28/2005 - J. Ritchie Carroll
//       Made modifications to original source (e.g., merged SimpleTimeZone into Win32TimeZone) to
//       help with FxCop compatibility.
//  02/09/2007 - Pinal C. Patel
//       Made modifications to Win32TimeZone.GetDaylightChanges() function in order to accomodate the
//       extended Daylight Savings Time taking effect from year 2007.
//       For more information: http://support.microsoft.com/gp/cp_dst.
//  07/09/2008 - Pinal C. Patel
//       Modified the TimeZones.LoadTimeZone() shared method to use a default value if any of the values
//       are missing from the registry as it happens to be case under Windows Vista (Index).
//
//*******************************************************************************************************


namespace TVA
{
    namespace DateTime
    {

        // *************************************************************************************************************
        //
        // Classes and code for TimeZone management follow.  Original code was written by Michael R. Brumm.
        // For updates and more information, visit: http://www.michaelbrumm.com/simpletimezone.html
        // or contact me@michaelbrumm.com
        //
        // Integrated into TVA code library on June 10th, 2004. Some minor modifications made for integration reasons.
        //
        // *************************************************************************************************************

        // SimpleTimeZone
        // by Michael R. Brumm
        //
        // For updates and more information, visit:
        // http://www.michaelbrumm.com/simpletimezone.html
        //
        // or contact me@michaelbrumm.com
        //
        // Please do not modify this code and re-release it. If you
        // require changes to this class, please derive your own class
        // from SimpleTimeZone, and add (or override) the methods and
        // properties on your own derived class. You never know when
        // your code might need to be version compatible with another
        // class that uses SimpleTimeZone.

        // IMPORTANT:
        // This class is immutable, and any derived classes
        // should also be immutable.
        [EditorBrowsable(EditorBrowsableState.Never), Serializable()]
        public class DaylightTimeChange
        {



            private const int NUM_DAYS_IN_WEEK = 7;

            private int _month;
            private DayOfWeek _dayOfWeek;
            private int _dayOfWeekIndex;
            private TimeSpan _timeOfDay;


            // Constructor without parameters is not allowed.
            private DaylightTimeChange()
            {
            }

            // Constructor allows the definition of a time change
            // for most time zones using daylight saving time. These
            // time zones often define the start or end of daylight
            // saving as "the first Sunday of April, at 2:00am". This
            // would be constructed as:
            //
            // New DaylightTimeChange( _
            //   4, _                      ' 4th month: April
            //   DayOfWeek.Sunday, 0, _    ' 1st Sunday
            //   New TimeSpan(2, 0, 0) _   ' at 2:00am
            // )
            //
            // "The last Sunday of October, at 2:00am" would be
            // constructed as:
            //
            // New DaylightTimeChange( _
            //   10, _                     ' 10th month: October
            //   DayOfWeek.Sunday, 4, _    ' 5th (last) Sunday
            //   New TimeSpan(2, 0, 0) _   ' at 2:00am
            // )
            //
            public DaylightTimeChange(int month, DayOfWeek dayOfWeek, int dayOfWeekIndex, TimeSpan timeOfDay)
            {

                // Parameter checking
                if ((month < 1) || (month > 12))
                {
                    throw (new ArgumentOutOfRangeException("month", month, "The month must be between 1 and 12, inclusive."));
                }

                if ((dayOfWeek < dayOfWeek.Sunday) || (dayOfWeek > dayOfWeek.Saturday))
                {
                    throw (new ArgumentOutOfRangeException("dayOfWeek", dayOfWeek, "The day of week must be between Sunday and Saturday."));
                }

                // 0 = 1st
                // 1 = 2nd
                // 2 = 3rd
                // 3 = 4th
                // 4 = 5th (last)
                if ((dayOfWeekIndex < 0) || (dayOfWeekIndex > 4))
                {
                    throw (new ArgumentOutOfRangeException("dayOfWeekIndex", dayOfWeekIndex, "The day of week index must be between 0 and 4, inclusive."));
                }

                if ((timeOfDay.Ticks < 0) || (timeOfDay.Ticks >= TimeSpan.TicksPerDay))
                {
                    throw (new ArgumentOutOfRangeException("timeOfDay", timeOfDay, "The time of the day must be less than one day, and not negative."));
                }

                // Initialize private storage
                _month = month;
                _dayOfWeek = dayOfWeek;
                _dayOfWeekIndex = dayOfWeekIndex;
                _timeOfDay = timeOfDay;

            }


            // Returns the time and date of the daylight saving change
            // for a particular year. For example:
            //   "the 1st Sunday of April at 2:00am" for the year "2000"
            //   is "2000/04/02 02:00"
            public virtual DateTime GetDate(int year)
            {

                if ((year < 1) || (year > DateTime.MaxValue.Year))
                {
                    throw (new ArgumentOutOfRangeException("year"));
                }

                // Get the first day of the change month for the specified year.
                DateTime resultDate = new DateTime(year, _month, 1);

                // Get the first day of the month that falls on the
                // day of the week for this change.
                if (resultDate.DayOfWeek > _dayOfWeek)
                {
                    resultDate = resultDate.AddDays(NUM_DAYS_IN_WEEK - (resultDate.DayOfWeek - _dayOfWeek));
                }
                else if (resultDate.DayOfWeek < _dayOfWeek)
                {
                    resultDate = resultDate.AddDays(_dayOfWeek - resultDate.DayOfWeek);
                }

                // Get the nth weekday (3rd Tuesday, for example)
                resultDate = resultDate.AddDays(NUM_DAYS_IN_WEEK * _dayOfWeekIndex);

                // If the date has passed the month, then go back a week. This allows
                // the 5th weekday to always be the last weekday.
                while (resultDate.Month > _month)
                {
                    resultDate = resultDate.AddDays(-NUM_DAYS_IN_WEEK);
                }

                // Add the time of day that daylight saving begins.
                resultDate = resultDate.Add(_timeOfDay);

                // Return the date and time of the change.
                return resultDate;

            }


        }


        // Win32 TimeZones
        // by Michael R. Brumm
        //
        // For updates and more information, visit:
        // http://www.michaelbrumm.com/simpletimezone.html
        //
        // or contact me@michaelbrumm.com
        //
        // Please do not modify this code and re-release it. If you
        // require changes to this class, please derive your own class
        // from SimpleTimeZone, and add (or override) the methods and
        // properties on your own derived class. You never know when
        // your code might need to be version compatible with another
        // class that uses Win32 TimeZones.

        // This should have been part of Microsoft.Win32, so that is
        // where I located it.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class TimeZones
        {



            private const string VALUE_INDEX = "Index";
            private const string VALUE_DISPLAY_NAME = "Display";
            private const string VALUE_STANDARD_NAME = "Std";
            private const string VALUE_DAYLIGHT_NAME = "Dlt";
            private const string VALUE_ZONE_INFO = "TZI";

            private const int LENGTH_ZONE_INFO = 44;
            private const int LENGTH_DWORD = 4;
            private const int LENGTH_WORD = 2;
            private const int LENGTH_SYSTEMTIME = 16;



            private static string[] REG_KEYS_TIME_ZONES = { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Time Zones", "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones" };


            private static string nameRegKeyTimeZones;

            private TimeZones()
            {

            }

            static TimeZones()
            {



                foreach (string currentNameRegKey in REG_KEYS_TIME_ZONES)
                {

                    if (!(Registry.LocalMachine.OpenSubKey(currentNameRegKey) == null))
                    {
                        nameRegKeyTimeZones = currentNameRegKey;
                        return;
                    }

                }


            }

            private class TZREGReader
            {



                public int Bias;
                public int StandardBias;
                public int DaylightBias;
                public SYSTEMTIMEReader StandardDate;
                public SYSTEMTIMEReader DaylightDate;


                public TZREGReader(byte[] bytes)
                {

                    int index;
                    index = 0;

                    Bias = BitConverter.ToInt32(bytes, index);
                    index = index + LENGTH_DWORD;

                    StandardBias = BitConverter.ToInt32(bytes, index);
                    index = index + LENGTH_DWORD;

                    DaylightBias = BitConverter.ToInt32(bytes, index);
                    index = index + LENGTH_DWORD;

                    StandardDate = new SYSTEMTIMEReader(bytes, index);
                    index = index + LENGTH_SYSTEMTIME;

                    DaylightDate = new SYSTEMTIMEReader(bytes, index);

                }


            }


            private class SYSTEMTIMEReader
            {



                public short Year;
                public short Month;
                public short DayOfWeek;
                public short Day;
                public short Hour;
                public short Minute;
                public short Second;
                public short Milliseconds;


                public SYSTEMTIMEReader(byte[] bytes, int index)
                {

                    Year = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Month = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    DayOfWeek = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Day = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Hour = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Minute = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Second = BitConverter.ToInt16(bytes, index);
                    index = index + LENGTH_WORD;

                    Milliseconds = BitConverter.ToInt16(bytes, index);

                }


            }


            // JRC: This function modified for performance
            private static string GetAbbreviation(string name)
			{
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				foreach (char currentChar in name.ToCharArray())
				{
					if (char.IsUpper(currentChar))
					{
						with_1.Append(currentChar);
					}
				}
				
				return with_1.ToString();
				
			}


            private static Win32TimeZone LoadTimeZone(RegistryKey regKeyTimeZone)
            {

                int timeZoneIndex;
                string displayName;
                string standardName;
                string daylightName;
                byte[] timeZoneData;

                // Under Windows Vista the Time Zone registry entries does not have the Index value, so the code has
                // been modified to use a default value if any of the values are missing from the registry entries.
                RegistryKey with_1 = regKeyTimeZone;
                timeZoneIndex = (int)(with_1.GetValue(VALUE_INDEX, -1));
                displayName = (string)(with_1.GetValue(VALUE_DISPLAY_NAME, string.Empty));
                standardName = (string)(with_1.GetValue(VALUE_STANDARD_NAME, string.Empty));
                daylightName = (string)(with_1.GetValue(VALUE_DAYLIGHT_NAME, string.Empty));
                timeZoneData = (byte[])(with_1.GetValue(VALUE_ZONE_INFO, new byte[] { }));

                if (timeZoneData.Length != LENGTH_ZONE_INFO)
                {
                    return null;
                }

                TZREGReader timeZoneInfo = new TZREGReader(timeZoneData);

                TimeSpan standardOffset = new TimeSpan(0, -(timeZoneInfo.Bias + timeZoneInfo.StandardBias), 0);

                TimeSpan daylightDelta = new TimeSpan(0, -(timeZoneInfo.DaylightBias), 0);

                if ((daylightDelta.Ticks == 0) || (timeZoneInfo.StandardDate.Month == 0) || (timeZoneInfo.DaylightDate.Month == 0))
                {
                    return new Win32TimeZone(timeZoneIndex, displayName, standardOffset, standardName, GetAbbreviation(standardName));
                }

                if ((timeZoneInfo.StandardDate.Year != 0) || (timeZoneInfo.DaylightDate.Year != 0))
                {
                    return null;
                }

                DaylightTimeChange daylightSavingsStart;
                DaylightTimeChange daylightSavingsEnd;

                daylightSavingsStart = new DaylightTimeChange(timeZoneInfo.DaylightDate.Month, ((DayOfWeek)timeZoneInfo.DaylightDate.DayOfWeek), (timeZoneInfo.DaylightDate.Day - 1), new TimeSpan(0, timeZoneInfo.DaylightDate.Hour, timeZoneInfo.DaylightDate.Minute, timeZoneInfo.DaylightDate.Second, timeZoneInfo.DaylightDate.Milliseconds));

                daylightSavingsEnd = new DaylightTimeChange(timeZoneInfo.StandardDate.Month, ((DayOfWeek)timeZoneInfo.StandardDate.DayOfWeek), (timeZoneInfo.StandardDate.Day - 1), new TimeSpan(0, timeZoneInfo.StandardDate.Hour, timeZoneInfo.StandardDate.Minute, timeZoneInfo.StandardDate.Second, timeZoneInfo.StandardDate.Milliseconds));

                return new Win32TimeZone(timeZoneIndex, displayName, standardOffset, standardName, GetAbbreviation(standardName), daylightDelta, daylightName, GetAbbreviation(daylightName), daylightSavingsStart, daylightSavingsEnd);

            }


            public static Win32TimeZone GetTimeZone(int index)
            {

                if (nameRegKeyTimeZones == null)
                {
                    return null;
                }

                RegistryKey regKeyTimeZones = null;

                try
                {
                    regKeyTimeZones = Registry.LocalMachine.OpenSubKey(nameRegKeyTimeZones);
                }
                catch
                {
                }

                if (regKeyTimeZones == null)
                {
                    return null;
                }

                Win32TimeZone result = null;


                string[] namesSubKeys;
                namesSubKeys = regKeyTimeZones.GetSubKeyNames();

                RegistryKey currentSubKey;

                //Dim currentTimeZone As Win32TimeZone
                int timeZoneIndex;

                foreach (string currentNameSubKey in namesSubKeys)
                {

                    try
                    {
                        currentSubKey = regKeyTimeZones.OpenSubKey(currentNameSubKey);
                    }
                    catch
                    {
                        currentSubKey = null;
                    }

                    if (!(currentSubKey == null))
                    {

                        try
                        {

                            timeZoneIndex = (int)(currentSubKey.GetValue(VALUE_INDEX));

                            if (timeZoneIndex == index)
                            {
                                result = LoadTimeZone(currentSubKey);
                                currentSubKey.Close();
                                goto endOfForLoop;
                            }

                        }
                        catch
                        {
                        }

                        currentSubKey.Close();

                    }

                }
            endOfForLoop:

                regKeyTimeZones.Close();

                return result;

            }


            public static Win32TimeZone[] GetTimeZones()
            {

                if (nameRegKeyTimeZones == null)
                {
                    return new Win32TimeZone[] { };
                }

                RegistryKey regKeyTimeZones = null;
                try
                {
                    regKeyTimeZones = Registry.LocalMachine.OpenSubKey(nameRegKeyTimeZones);
                }
                catch
                {
                }

                if (regKeyTimeZones == null)
                {
                    return new Win32TimeZone[] { };
                }

                ArrayList results = new ArrayList();


                string[] namesSubKeys;
                namesSubKeys = regKeyTimeZones.GetSubKeyNames();

                RegistryKey currentSubKey;

                Win32TimeZone currentTimeZone;

                foreach (string currentNameSubKey in namesSubKeys)
                {

                    try
                    {
                        currentSubKey = regKeyTimeZones.OpenSubKey(currentNameSubKey);
                    }
                    catch
                    {
                        currentSubKey = null;
                    }

                    if (!(currentSubKey == null))
                    {

                        try
                        {

                            currentTimeZone = LoadTimeZone(currentSubKey);

                            if (!(currentTimeZone == null))
                            {
                                results.Add(currentTimeZone);
                            }

                        }
                        catch
                        {
                        }

                        currentSubKey.Close();

                    }

                }

                regKeyTimeZones.Close();

                return ((Win32TimeZone[])(results.ToArray(typeof(Win32TimeZone))));

            }


        }

        // Win32TimeZone
        // by Michael R. Brumm
        //
        // For updates and more information, visit:
        // http://www.michaelbrumm.com/simpletimezone.html
        //
        // or contact me@michaelbrumm.com
        //
        // Please do not modify this code and re-release it. If you
        // require changes to this class, please derive your own class
        // from SimpleTimeZone, and add (or override) the methods and
        // properties on your own derived class. You never know when
        // your code might need to be version compatible with another
        // class that uses Win32TimeZone.

        // JRC: Merged SimpleTimeZone (original base class) directly
        // into Win32TimeZone since Win32TimeZone was the only class
        // being consumed by code library.  This was done for
        // simplification and to make FxCop happier.

        /// <summary>Win32 time zone class</summary>
        [Serializable()]
        public class Win32TimeZone : TimeZone
        {



            private bool _standardAlways;
            //Private _daylightAlwaysWithinStandard As Boolean
            //Private _standardAlwaysWithinDaylight As Boolean

            private TimeSpan _standardOffset;
            private string _standardName;
            private string _standardAbbreviation;

            private TimeSpan _daylightDelta;
            private TimeSpan _daylightOffset;
            private string _daylightName;
            private string _daylightAbbreviation;
            private DaylightTimeChange _daylightTimeChangeStart;
            private DaylightTimeChange _daylightTimeChangeEnd;

            private int _index;
            private string _displayName;

            private static List<string> m_unitedStatesTimeZones;

            static Win32TimeZone()
            {

                // This is a list of all the United States time zones that are afftected by the exteded Daylight
                // Savings Time taking effect from year 2007. The list is declared as a "Shared" member and
                // initialized in this default shared constructor so that this list gets initialized no matter which
                // public gets called.
                m_unitedStatesTimeZones = new List<string>();
                m_unitedStatesTimeZones.Add("ALASKAN STANDARD TIME");
                m_unitedStatesTimeZones.Add("ATLANTIC STANDARD TIME");
                m_unitedStatesTimeZones.Add("CENTRAL STANDARD TIME");
                m_unitedStatesTimeZones.Add("EASTERN STANDARD TIME");
                m_unitedStatesTimeZones.Add("HAWAIIAN STANDARD TIME");
                m_unitedStatesTimeZones.Add("MOUNTAIN STANDARD TIME");
                m_unitedStatesTimeZones.Add("PACIFIC STANDARD TIME");
                m_unitedStatesTimeZones.Sort();

            }

            public Win32TimeZone(int index, string displayName, TimeSpan standardOffset, string standardName, string standardAbbreviation)
                : this(standardOffset, standardName, standardAbbreviation)
            {


                _index = index;
                _displayName = displayName;

            }

            public Win32TimeZone(int index, string displayName, TimeSpan standardOffset, string standardName, string standardAbbreviation, TimeSpan daylightDelta, string daylightName, string daylightAbbreviation, DaylightTimeChange daylightTimeChangeStart, DaylightTimeChange daylightTimeChangeEnd)
                : this(standardOffset, standardName, standardAbbreviation, daylightDelta, daylightName, daylightAbbreviation, daylightTimeChangeStart, daylightTimeChangeEnd)
            {


                _index = index;
                _displayName = displayName;

            }


            // Constructor without parameters is not allowed.
            private Win32TimeZone()
            {
            }

            // Constructor for time zone without daylight saving time.
            public Win32TimeZone(TimeSpan standardOffset, string standardName, string standardAbbreviation)
            {

                // Initialize private storage
                //_standardAlways = True
                _standardOffset = standardOffset;
                _standardName = standardName;
                _standardAbbreviation = standardAbbreviation;

            }

            // Constructor for time zone with or without daylight saving time.
            public Win32TimeZone(TimeSpan standardOffset, string standardName, string standardAbbreviation, TimeSpan daylightDelta, string daylightName, string daylightAbbreviation, DaylightTimeChange daylightTimeChangeStart, DaylightTimeChange daylightTimeChangeEnd)
            {

                // Allow non-daylight saving time zones to be created
                // using this constructor.
                if ((daylightTimeChangeStart == null) && (daylightTimeChangeEnd == null))
                {

                    // Initialize private storage
                    //_standardAlways = True
                    _standardOffset = standardOffset;
                    _standardName = standardName;
                    _standardAbbreviation = standardAbbreviation;

                    return;

                }

                // If the time zone has a start OR an end, then it
                // must have a start AND an end.
                if (daylightTimeChangeStart == null)
                {
                    throw (new ArgumentNullException("daylightTimeChangeStart"));
                }

                if (daylightTimeChangeEnd == null)
                {
                    throw (new ArgumentNullException("daylightTimeChangeEnd"));
                }

                // Initialize private storage
                //_standardAlways = False
                _standardOffset = standardOffset;
                _standardName = standardName;
                _standardAbbreviation = standardAbbreviation;

                _daylightDelta = daylightDelta;
                _daylightOffset = _standardOffset.Add(daylightDelta);
                _daylightName = daylightName;
                _daylightAbbreviation = daylightAbbreviation;

                // These referance types are immutable, so they cannot be
                // changed outside this class' scope, and thus can be
                // permanently referenced.
                _daylightTimeChangeStart = daylightTimeChangeStart;
                _daylightTimeChangeEnd = daylightTimeChangeEnd;

            }


            public override string StandardName
            {
                get
                {
                    return _standardName;
                }
            }


            public virtual string StandardAbbreviation
            {
                get
                {
                    return _standardAbbreviation;
                }
            }


            public override string DaylightName
            {
                get
                {
                    return _daylightName;
                }
            }


            public virtual string DaylightAbbreviation
            {
                get
                {
                    return _daylightAbbreviation;
                }
            }


            // The name is dependant on whether the time zone is in daylight
            // saving time or not. This method can be ambiguous during
            // daylight changes.
            public virtual string GetNameLocalTime(DateTime time)
            {

                if (_standardAlways)
                {
                    return _standardName;
                }
                else if (IsDaylightSavingTime(time))
                {
                    return _daylightName;
                }
                else
                {
                    return _standardName;
                }

            }

            // This method is unambiguous during daylight changes.
            public virtual string GetNameUniversalTime(DateTime time)
            {

                if (IsDaylightSavingTimeUniversalTime(time))
                {
                    return _daylightName;
                }
                else
                {
                    return _standardName;
                }

            }


            // The abbreviation is dependant on whether the time zone is in
            // daylight saving time or not. This method can be ambiguous during
            // daylight changes.
            public virtual string GetAbbreviationLocalTime(DateTime time)
            {

                if (_standardAlways)
                {
                    return _standardAbbreviation;
                }
                else if (IsDaylightSavingTime(time))
                {
                    return _daylightAbbreviation;
                }
                else
                {
                    return _standardAbbreviation;
                }

            }

            // This method is unambiguous during daylight changes.
            public virtual string GetAbbreviationUniversalTime(DateTime time)
            {

                if (IsDaylightSavingTimeUniversalTime(time))
                {
                    return _daylightAbbreviation;
                }
                else
                {
                    return _standardAbbreviation;
                }

            }


            public override DaylightTime GetDaylightChanges(int year)
            {

                if ((year < 1) || (year > DateTime.MaxValue.Year))
                {
                    throw (new ArgumentOutOfRangeException("year"));
                }

                if (_standardAlways)
                {
                    return null;

                }
                else
                {
                    // 02/09/2007 - PCP: Made modifications to the code below that returns the start and end date for
                    // Daylight Savings Time for the current time zone. Because of the The U.S. Energy Policy Act of
                    // 2005, the start and end date for Daylight Savings Time for time zones in the United States have
                    // changed begging year 2007. As a result the registry entries that are used to determine the DST
                    // dates under Windows are updated to provide the new start and end dates, and the old dates that
                    // were used up until year 2006 are no longer available. This can cause a problem when looking
                    // at historical data (year 2006 and earlier), because the DST calculation will yeild incorrect
                    // result since we will be using the new start and end dates for DST instead of the old ones. To
                    // overcome this problem, we have hard-coded the DST start and end dates of United States time zones
                    // for year 2006 and earlier.
                    // Daylight Savings Time duration for United States Time zones:
                    // Until 2006 - first Sunday in April to last Sunday in October
                    // From 2007 - second Sunday in March to first Sunday in November
                    if (year <= 2006 && m_unitedStatesTimeZones.BinarySearch(_standardName.ToUpper()) >= 0)
                    {
                        // The requested year is 2006 or earlier and the time zone is one of the United States time
                        // zones affected by the extended Daylight Savings Time.
                        return new DaylightTime(new DaylightTimeChange(4, DayOfWeek.Sunday, 0, new TimeSpan(2, 0, 0)).GetDate(year), new DaylightTimeChange(10, DayOfWeek.Sunday, 4, new TimeSpan(2, 0, 0)).GetDate(year), _daylightDelta);
                    }
                    else
                    {

                        return new DaylightTime(_daylightTimeChangeStart.GetDate(year), _daylightTimeChangeEnd.GetDate(year), _daylightDelta);
                    }
                }

            }


            // This method can be ambiguous during daylight changes.
            public override bool IsDaylightSavingTime(DateTime time)
            {

                return IsDaylightSavingTime(time, false);

            }


            // This method is unambiguous during daylight changes.
            public virtual bool IsDaylightSavingTimeUniversalTime(DateTime time)
            {

                time = time.Add(_standardOffset);
                return IsDaylightSavingTime(time, true);

            }


            private bool IsDaylightSavingTime(DateTime time, bool fromUtcTime)
            {

                // If this time zone is never in daylight saving, then
                // return false.
                if (_standardAlways)
                {
                    return false;
                }

                // Get the daylight saving time start and end for this
                // time's year.
                DaylightTime daylightTimes;
                daylightTimes = GetDaylightChanges(time.Year);

                // Return whether the time is within the daylight saving
                // time for this year.
                return IsDaylightSavingTime(time, daylightTimes, fromUtcTime);

            }


            public static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes)
            {

                return IsDaylightSavingTime(time, daylightTimes, false);

            }


            private static bool IsDaylightSavingTime(DateTime time, DaylightTime daylightTimes, bool fromUtcTime)
            {

                // Mirrors .NET Framework TimeZone functionality, which
                // does not throw an exception.
                if (daylightTimes == null)
                {
                    return false;
                }

                DateTime daylightStart;
                DateTime daylightEnd;
                TimeSpan daylightDelta;
                daylightStart = daylightTimes.Start;
                daylightEnd = daylightTimes.End;
                daylightDelta = daylightTimes.Delta;

                // If the time came from a utc time, then the delta must be
                // removed from the end time, because the end of daylight
                // saving time is described using using a local time (which
                // is currently in daylight saving time).
                if (fromUtcTime)
                {
                    daylightEnd = daylightEnd.Subtract(daylightDelta);
                }

                // Northern hemisphere (normally)
                // The daylight saving time of the year falls between the
                // start and the end dates.
                if (daylightStart < daylightEnd)
                {

                    // The daylight saving time of the year falls between the
                    // start and the end dates.
                    if ((time >= daylightStart) && (time < daylightEnd))
                    {

                        // If the time was taken from a UTC time, then do not apply
                        // the backward compatibility.
                        if (fromUtcTime)
                        {
                            return true;

                            // Backward compatiblity with .NET Framework TimeZone.
                            // If the daylight saving delta is positive, then there is a
                            // period of time which does not exist (between 2am and 3am in
                            // most daylight saving time zones) at the beginning of the
                            // daylight saving. This period of non-existant time should be
                            // considered standard time (not daylight saving).
                        }
                        else
                        {

                            if (daylightDelta.Ticks > 0)
                            {
                                if (time < (daylightStart.Add(daylightDelta)))
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }

                        }

                        // Otherwise, the time and date is not within daylight
                        // saving time.
                    }
                    else
                    {

                        // If the time was taken from a UTC time, then do not apply
                        // the backward compatibility.
                        if (fromUtcTime)
                        {
                            return false;

                            // Backward compatiblity with .NET Framework TimeZone.
                            // If the daylight saving delta is negative (which shouldn't
                            // happen), then there is a period of time which does not exist
                            // (between 2am and 3am in most daylight saving time zones).
                            // at the end of daylight saving. This period of
                            // non-existant time should be considered daylight saving.
                        }
                        else
                        {

                            if (daylightDelta.Ticks < 0)
                            {

                                if ((time >= daylightEnd) && (time < daylightEnd.Subtract(daylightDelta)))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }

                            }
                            else
                            {
                                return false;
                            }

                        }

                    }

                    // Southern hemisphere (normally)
                    // The daylight saving time of the year is after the start,
                    // or before the end, but not between the two dates.
                }
                else
                {

                    // The daylight saving time of the year is after the start,
                    // or before the end, but not between the two dates.
                    if (time >= daylightStart)
                    {

                        // If the time was taken from a UTC time, then do not apply
                        // the backward compatibility.
                        if (fromUtcTime)
                        {
                            return true;

                            // Backward compatiblity with .NET Framework TimeZone.
                            // If the daylight saving delta is positive, then there is a
                            // period of time which does not exist (between 2am and 3am in
                            // most daylight saving time zones) at the beginning of the
                            // daylight saving. This period of non-existant time should be
                            // considered standard time (not daylight saving).
                        }
                        else
                        {

                            if (daylightDelta.Ticks > 0)
                            {
                                if (time < (daylightStart.Add(daylightDelta)))
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }

                        }

                        // The current time is before the end of daylight saving, so
                        // it is during daylight saving.
                    }
                    else if (time < daylightEnd)
                    {
                        return true;

                        // Otherwise, the time and date is not within daylight
                        // saving time.
                    }
                    else
                    {

                        // If the time was taken from a UTC time, then do not apply
                        // the backward compatibility.
                        if (fromUtcTime)
                        {
                            return false;

                            // Backward compatiblity with .NET Framework TimeZone.
                            // If the daylight saving delta is negative (which shouldn't
                            // happen), then there is a period of time which does not exist
                            // (between 2am and 3am in most daylight saving time zones).
                            // at the end of daylight saving. This period of
                            // non-existant time should be considered daylight saving.
                        }
                        else
                        {

                            if (daylightDelta.Ticks < 0)
                            {

                                if ((time >= daylightEnd) && (time < daylightEnd.Subtract(daylightDelta)))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }

                            }
                            else
                            {
                                return false;
                            }

                        }

                    }

                }

            }


            public virtual bool IsAmbiguous(DateTime time)
            {

                // If this time zone is never in daylight saving, then
                // return false.
                if (_standardAlways)
                {
                    return false;
                }

                // Get the daylight saving time start and end for this
                // time's year.
                DaylightTime daylightTimes;
                daylightTimes = GetDaylightChanges(time.Year);

                // Return whether the time is within the ambiguous
                // time for this year.
                return IsAmbiguous(time, daylightTimes);

            }


            public static bool IsAmbiguous(DateTime time, DaylightTime daylightTimes)
            {

                // Mirrors .NET Framework TimeZone functionality, which
                // does not throw an exception.
                if (daylightTimes == null)
                {
                    return false;
                }

                DateTime daylightStart;
                DateTime daylightEnd;
                TimeSpan daylightDelta;
                daylightStart = daylightTimes.Start;
                daylightEnd = daylightTimes.End;
                daylightDelta = daylightTimes.Delta;

                // The ambiguous time is at the end of the daylight
                // saving time when the delta is positive.
                if (daylightDelta.Ticks > 0)
                {

                    if ((time < daylightEnd) && (daylightEnd.Subtract(daylightDelta) <= time))
                    {
                        return true;
                    }

                    // The ambiguous time is at the start of the daylight
                    // saving time when the delta is negative.
                }
                else if (daylightDelta.Ticks < 0)
                {

                    if ((time < daylightStart) && (daylightStart.Add(daylightDelta) <= time))
                    {
                        return true;
                    }

                }

                return false;

            }


            public override TimeSpan GetUtcOffset(DateTime time)
            {

                // If this time zone is never in daylight saving, then
                // return the standard offset.
                if (_standardAlways)
                {
                    return _standardOffset;

                    // If the time zone is in daylight saving, then return
                    // the daylight saving offset.
                }
                else if (IsDaylightSavingTime(time))
                {
                    return _daylightOffset;

                    // Otherwise, return the standard offset.
                }
                else
                {
                    return _standardOffset;
                }

            }


            public override DateTime ToLocalTime(DateTime utcTime)
            {

                utcTime = utcTime.Add(_standardOffset);

                if (!(_standardAlways))
                {
                    if (IsDaylightSavingTime(utcTime, true))
                    {
                        utcTime = utcTime.Add(_daylightDelta);
                    }
                }

                return utcTime;

            }


            // This can return an incorrect time during the time change
            // between standard and daylight saving time, because
            // times near the daylight saving switch can be ambiguous.
            //
            // For example, if daylight saving ends at:
            // "2000/10/29 02:00", and fall back an hour, then is:
            // "2000/10/29 01:30", during daylight saving, or not
            //
            // Consequently, this function is provided for backwards
            // compatiblity only, and should be deprecated and replaced
            // with the overload that allows daylight saving to be
            // specified.
            public override DateTime ToUniversalTime(DateTime time)
            {

                if (_standardAlways)
                {
                    return time.Subtract(_standardOffset);

                }
                else
                {

                    if (IsDaylightSavingTime(time))
                    {
                        return time.Subtract(_daylightOffset);
                    }
                    else
                    {
                        return time.Subtract(_standardOffset);
                    }

                }


            }


            // This overload allows the status of daylight saving
            // to be specified along with the time. This conversion
            // is unambiguous and always correct.
            public DateTime ToUniversalTime(DateTime time, bool daylightSaving)
            {

                if (_standardAlways)
                {
                    return time.Subtract(_standardOffset);

                }
                else
                {

                    if (daylightSaving)
                    {
                        return time.Subtract(_daylightOffset);
                    }
                    else
                    {
                        return time.Subtract(_standardOffset);
                    }

                }

            }


            public int Index
            {
                get
                {
                    return _index;
                }
            }


            public string DisplayName
            {
                get
                {
                    return _displayName;
                }
            }


            public override string ToString()
            {
                return _displayName;
            }


        }

    }

}
