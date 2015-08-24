//*******************************************************************************************************
//  Schedule.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/01/2006 - Pinal C. Patel
//      Generated original version of source code.
//  09/15/2008 - James R. Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Text;

namespace TVA.Scheduling
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
            : this("Schedule" + (++m_instances))
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
        {
            this.Name = name;
            this.Rule = rule;
            this.Description = description;
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
                    throw new ArgumentNullException("value");

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
                    throw new ArgumentNullException("value");

                string[] scheduleParts = value.RemoveDuplicateWhiteSpace().Split(' ');

                if (scheduleParts.Length != 5)
                    throw new ArgumentException("Schedule rule must have exactly 5 parts (Example: * * * * *).");

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
            DateTime currentDateTime = DateTime.Now;

            if (m_minutePart.Matches(currentDateTime) &&
                m_hourPart.Matches(currentDateTime) &&
                m_dayPart.Matches(currentDateTime) &&
                m_monthPart.Matches(currentDateTime) &&
                m_dayOfWeekPart.Matches(currentDateTime))
            {
                m_lastDueAt = currentDateTime;
                return true;
            }
            else
            {
                return false;
            }
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

            return other != null && 
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
       
        #endregion

        #region [ Static ]

        // Static Fields
        private static int m_instances;

        #endregion
        
    }
}