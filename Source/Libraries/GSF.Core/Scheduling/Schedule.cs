//******************************************************************************************************
//  Schedule.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/01/2006 - Pinal C. Patel
//       Generated original version of source code.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Text;

namespace GSF.Scheduling
{
    /// <summary>
    /// Represents a schedule defined using UNIX crontab syntax.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Operators:
    /// </para>
    /// <para>
    /// There are several ways of specifying multiple date/time values in a field:
    /// <list type="bullet">
    /// <item>
    ///     <description>
    ///         The comma (',') operator specifies a list of values, for example: "1,3,4,7,8"
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The dash ('-') operator specifies a range of values, for example: "1-6",
    ///         which is equivalent to "1,2,3,4,5,6"
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The asterisk ('*') operator specifies all possible values for a field.
    ///         For example, an asterisk in the hour time field would be equivalent to
    ///         'every hour' (subject to matching other specified fields).
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         The slash ('/') operator (called "step"), which can be used to skip a given
    ///         number of values. For example, "*/3" in the hour time field is equivalent
    ///         to "0,3,6,9,12,15,18,21". So "*" specifies 'every hour' but the "*/3" means
    ///         only those hours divisible by 3.
    ///     </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Fields:
    /// </para>
    /// <para>
    /// <code>
    ///     +---------------- minute (0 - 59)
    ///     |  +------------- hour (0 - 23)
    ///     |  |  +---------- day of month (1 - 31)
    ///     |  |  |  +------- month (1 - 12)
    ///     |  |  |  |  +---- day of week (0 - 7) (Sunday=0 or 7)
    ///     |  |  |  |  |
    ///     *  *  *  *  *
    /// </code>
    /// </para>
    /// <para>
    /// Each of the patterns from the first five fields may be either * (an asterisk), which matches all legal values,
    /// or a list of elements separated by commas. 
    /// </para>
    /// <para>
    /// See <a href="http://en.wikipedia.org/wiki/Cron" target="_blank">http://en.wikipedia.org/wiki/Cron</a> for more information.
    /// </para>
    /// </remarks>
    /// <seealso cref="SchedulePart"/>
    /// <seealso cref="ScheduleManager"/>
    public class Schedule
    {
        #region [ Members ]

        // Fields
        private string m_name;
        private string m_description;
        private bool m_useLocalTime;
        private SchedulePart m_minutePart;
        private SchedulePart m_hourPart;
        private SchedulePart m_dayPart;
        private SchedulePart m_monthPart;
        private SchedulePart m_dayOfWeekPart;
        private DateTime m_lastDueAt;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        public Schedule()
            : this("Schedule" + (++s_instances))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <remarks>Default <see cref="Rule"/> of '* * * * *' is used.</remarks>
        public Schedule(string name)
            : this(name, "* * * * *")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <param name="rule">Rule formated in UNIX crontab syntax.</param>
        public Schedule(string name, string rule)
            : this(name, rule, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <param name="rule">Rule formated in UNIX crontab syntax.</param>
        /// <param name="description">Description of defined schedule.</param>
        public Schedule(string name, string rule, string description)
            : this(name, rule, description, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <param name="rule">Rule formated in UNIX crontab syntax.</param>
        /// <param name="description">Description of defined schedule.</param>
        /// <param name="useLocalTime">Flag that determines whether to use local time for schedule.</param>
        public Schedule(string name, string rule, string description, bool useLocalTime)
        {
            Name = name;
            Rule = rule;
            Description = description;
            UseLocalTime = useLocalTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="Schedule"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the rule of the <see cref="Schedule"/> defined in UNIX crontab syntax.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        /// <exception cref="ArgumentException">The number of <see cref="SchedulePart"/> in the rule is not exactly 5.</exception>
        public string Rule
        {
            get
            {
                return m_minutePart.ValueText + " " + m_hourPart.ValueText + " " + m_dayPart.ValueText + " " + m_monthPart.ValueText + " " + m_dayOfWeekPart.ValueText;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                string[] scheduleParts = value.RemoveDuplicateWhiteSpace().Split(' ');

                if (scheduleParts.Length != 5)
                    throw new ArgumentException("Schedule rule must have exactly 5 parts (Example: * * * * *)");

                m_minutePart = new SchedulePart(scheduleParts[0], DateTimePart.Minute);
                m_hourPart = new SchedulePart(scheduleParts[1], DateTimePart.Hour);
                m_dayPart = new SchedulePart(scheduleParts[2], DateTimePart.Day);
                m_monthPart = new SchedulePart(scheduleParts[3], DateTimePart.Month);
                m_dayOfWeekPart = new SchedulePart(scheduleParts[4], DateTimePart.DayOfWeek);

                // Update the schedule description.
                StringBuilder description = new StringBuilder();

                description.Append(m_minutePart.Description);
                description.Append(", ");
                description.Append(m_hourPart.Description);
                description.Append(", ");
                description.Append(m_dayPart.Description);
                description.Append(", ");
                description.Append(m_monthPart.Description);
                description.Append(", ");
                description.Append(m_dayOfWeekPart.Description);

                m_description = description.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a description of the <see cref="Schedule"/>.
        /// </summary>
        /// <remarks>A default description is created automatically when the <see cref="Rule"/> is set.</remarks>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_description = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines whether the scheduler uses local time or UTC time for scheduling.
        /// </summary>
        public bool UseLocalTime
        {
            get
            {
                return m_useLocalTime;
            }
            set
            {
                m_useLocalTime = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="SchedulePart"/> of the <see cref="Schedule"/> that represents minute <see cref="DateTimePart"/>.
        /// </summary>
        [Browsable(false)]
        public SchedulePart MinutePart
        {
            get
            {
                return m_minutePart;
            }
        }

        /// <summary>
        /// Gets the <see cref="SchedulePart"/> of the <see cref="Schedule"/> that represents hour <see cref="DateTimePart"/>.
        /// </summary>
        [Browsable(false)]
        public SchedulePart HourPart
        {
            get
            {
                return m_hourPart;
            }
        }

        /// <summary>
        /// Gets the <see cref="SchedulePart"/> of the <see cref="Schedule"/> that represents day of month <see cref="DateTimePart"/>.
        /// </summary>
        [Browsable(false)]
        public SchedulePart DayPart
        {
            get
            {
                return m_dayPart;
            }
        }

        /// <summary>
        /// Gets the <see cref="SchedulePart"/> of the <see cref="Schedule"/> that represents month <see cref="DateTimePart"/>.
        /// </summary>
        [Browsable(false)]
        public SchedulePart MonthPart
        {
            get
            {
                return m_monthPart;
            }
        }

        /// <summary>
        /// Gets the <see cref="SchedulePart"/> of the <see cref="Schedule"/> that represents day of week <see cref="DateTimePart"/>.
        /// </summary>
        [Browsable(false)]
        public SchedulePart DaysOfWeekPart
        {
            get
            {
                return m_dayOfWeekPart;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> when the <see cref="Schedule"/> was last due.
        /// </summary>
        [Browsable(false)]
        public DateTime LastDueAt
        {
            get
            {
                return m_lastDueAt;
            }
        }

        /// <summary>
        /// Gets the current status of the <see cref="Schedule"/>.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("             Schedule name: ");
                status.Append(m_name);
                status.AppendLine();
                status.Append("             Schedule rule: ");
                status.Append(Rule);
                status.AppendLine();
                status.Append("             Last run time: ");
                status.Append(m_lastDueAt == DateTime.MinValue ? "Never" : m_lastDueAt.ToString());
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Checks whether the <see cref="Schedule"/> is due at the present system time.
        /// </summary>
        /// <returns>true if the <see cref="Schedule"/> is due at the present system time; otherwise false.</returns>
        public bool IsDue()
        {
            DateTime currentDateTime = m_useLocalTime ? DateTime.Now : DateTime.UtcNow;

            if (m_minutePart.Matches(currentDateTime) &&
                m_hourPart.Matches(currentDateTime) &&
                m_dayPart.Matches(currentDateTime) &&
                m_monthPart.Matches(currentDateTime) &&
                m_dayOfWeekPart.Matches(currentDateTime))
            {
                m_lastDueAt = currentDateTime;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines the nearest timestamp older than
        /// the given timestamp that matches the schedule.
        /// </summary>
        /// <param name="targetDateTime">The target timestamp</param>
        /// <returns>The timestamp prior to the target that the schedule was previously due</returns>
        public DateTime PreviousTimeDue(DateTime targetDateTime)
        {
            if (m_minutePart.Values.Count == 0 ||
                m_hourPart.Values.Count == 0 ||
                m_dayPart.Values.Count == 0 ||
                m_monthPart.Values.Count == 0 ||
                m_dayOfWeekPart.Values.Count == 0)
            {
                return DateTime.MinValue;
            }

            DateTime currentDateTime = ToScheduleTimeZone(targetDateTime);

            bool FindMatchingMonth()
            {
                while (true)
                {
                    if (m_monthPart.Matches(currentDateTime))
                        return true;
                    if (currentDateTime == DateTime.MinValue)
                        return false;
                    currentDateTime = PreviousMonth(currentDateTime);
                }
            }

            bool FindMatchingDay()
            {
                while (true)
                {
                    if (m_dayPart.Matches(currentDateTime) && m_dayOfWeekPart.Matches(currentDateTime))
                        return true;
                    if (currentDateTime.Day == 1)
                        return false;
                    currentDateTime = PreviousDay(currentDateTime);
                }
            }

            bool FindMatchingHour()
            {
                while (true)
                {
                    if (m_hourPart.Matches(currentDateTime))
                        return true;
                    if (currentDateTime.Hour == 0)
                        return false;
                    currentDateTime = PreviousHour(currentDateTime);
                }
            }

            bool FindMatchingMinute()
            {
                while (true)
                {
                    if (m_minutePart.Matches(currentDateTime))
                        return true;
                    if (currentDateTime.Minute == 0)
                        return false;
                    currentDateTime = PreviousMinute(currentDateTime);
                }
            }

            while (currentDateTime > DateTime.MinValue)
            {
                currentDateTime = PreviousMinute(currentDateTime);

                if (!FindMatchingMonth())
                    continue;

                if (!FindMatchingDay())
                    continue;

                if (!FindMatchingHour())
                    continue;

                if (!FindMatchingMinute())
                    continue;

                return currentDateTime;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Determines the nearest timestamp past the
        /// given timestamp that matches the schedule.
        /// </summary>
        /// <param name="targetDateTime">The target timestamp</param>
        /// <returns>The timestamp past the target that the schedule will next be due</returns>
        public DateTime NextTimeDue(DateTime targetDateTime)
        {
            if (m_minutePart.Values.Count == 0 ||
                m_hourPart.Values.Count == 0 ||
                m_dayPart.Values.Count == 0 ||
                m_monthPart.Values.Count == 0 ||
                m_dayOfWeekPart.Values.Count == 0)
            {
                return DateTime.MaxValue;
            }

            DateTime currentDateTime = ToScheduleTimeZone(targetDateTime);

            bool FindMatchingMonth()
            {
                while (true)
                {
                    if (m_monthPart.Matches(currentDateTime))
                        return true;
                    if (currentDateTime == DateTime.MaxValue)
                        return false;
                    currentDateTime = NextMonth(currentDateTime);
                }
            }

            bool FindMatchingDay()
            {
                while (true)
                {
                    if (m_dayPart.Matches(currentDateTime) && m_dayOfWeekPart.Matches(currentDateTime))
                        return true;

                    DateTime nextDay = NextDay(currentDateTime);

                    if (nextDay.Day == 1)
                        return false;

                    currentDateTime = nextDay;
                }
            }

            bool FindMatchingHour()
            {
                while (true)
                {
                    if (m_hourPart.Matches(currentDateTime))
                        return true;

                    DateTime nextHour = NextHour(currentDateTime);

                    if (nextHour.Hour == 0)
                        return false;

                    currentDateTime = nextHour;
                }
            }

            bool FindMatchingMinute()
            {
                while (true)
                {
                    if (m_minutePart.Matches(currentDateTime))
                        return true;

                    DateTime nextMinute = NextMinute(currentDateTime);

                    if (currentDateTime.Minute == 0)
                        return false;

                    currentDateTime = nextMinute;
                }
            }

            while (currentDateTime < DateTime.MaxValue)
            {
                currentDateTime = NextMinute(currentDateTime);

                if (!FindMatchingMonth())
                    continue;

                if (!FindMatchingDay())
                    continue;

                if (!FindMatchingHour())
                    continue;

                if (!FindMatchingMinute())
                    continue;

                return currentDateTime;
            }

            return DateTime.MaxValue;
        }

        /// <summary>
        /// Gets a hash code for the <see cref="Schedule"/>.
        /// </summary>
        /// <returns>An <see cref="Int32"/> based hashcode.</returns>
        public override int GetHashCode()
        {
            return Rule.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Schedule"/> is equal to the current <see cref="Schedule"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Schedule"/> to compare with the current <see cref="Schedule"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Schedule"/> is equal to the current <see cref="Schedule"/>; otherwise false
        /// </returns>
        public override bool Equals(object obj)
        {
            Schedule other = obj as Schedule;

            return (object)other != null &&
                   this.Name == other.Name &&
                   this.Rule == other.Rule;
        }

        /// <summary>
        /// Gets the string representation of <see cref="Schedule"/>.
        /// </summary>
        /// <returns>String representation of <see cref="Schedule"/>.</returns>
        public override string ToString()
        {
            return m_name;
        }

        private DateTime ToScheduleTimeZone(DateTime dateTime)
        {
            DateTimeKind targetKind = m_useLocalTime
                ? DateTimeKind.Local
                : DateTimeKind.Utc;

            DateTime specifiedDateTime = (dateTime.Kind == DateTimeKind.Unspecified)
                ? DateTime.SpecifyKind(dateTime, targetKind)
                : dateTime;

            return m_useLocalTime
                ? specifiedDateTime.ToLocalTime()
                : specifiedDateTime.ToUniversalTime();
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static int s_instances;

        // Static Methods

        private static DateTime PreviousInterval(DateTime dt, TimeSpan interval)
        {
            long ticks = dt.Ticks % interval.Ticks;
            DateTime startOfInterval = dt.AddTicks(-ticks);

            if (startOfInterval <= DateTime.MinValue.AddMinutes(1.0D))
                return DateTime.MinValue;

            return startOfInterval.AddMinutes(-1.0D);
        }

        private static DateTime PreviousMonth(DateTime dt)
        {
            int day = dt.Day;
            DateTime startOfMonth = dt.Date.AddDays(-day + 1.0D);
            return PreviousMinute(startOfMonth);
        }

        private static DateTime PreviousDay(DateTime dt) =>
            PreviousInterval(dt, TimeSpan.FromDays(1.0D));

        private static DateTime PreviousHour(DateTime dt) =>
            PreviousInterval(dt, TimeSpan.FromHours(1.0D));

        private static DateTime PreviousMinute(DateTime dt) =>
            PreviousInterval(dt, TimeSpan.FromMinutes(1.0D));

        private static DateTime NextInterval(DateTime dt, TimeSpan interval)
        {
            long ticks = dt.Ticks % interval.Ticks;
            DateTime startOfInterval = dt.AddTicks(-ticks);

            if (startOfInterval >= DateTime.MaxValue.Add(-interval))
                return DateTime.MaxValue;

            return startOfInterval.Add(interval);
        }

        private static DateTime NextMonth(DateTime dt)
        {
            int day = dt.Day;
            DateTime startOfMonth = dt.Date.AddDays(-day + 1.0D);

            if (startOfMonth >= DateTime.MaxValue.AddMonths(-1))
                return DateTime.MaxValue;

            return startOfMonth.AddMonths(1);
        }

        private static DateTime NextDay(DateTime dt) =>
            NextInterval(dt, TimeSpan.FromDays(1.0D));

        private static DateTime NextHour(DateTime dt) =>
            NextInterval(dt, TimeSpan.FromHours(1.0D));

        private static DateTime NextMinute(DateTime dt) =>
            NextInterval(dt, TimeSpan.FromMinutes(1.0D));

        #endregion

    }
}