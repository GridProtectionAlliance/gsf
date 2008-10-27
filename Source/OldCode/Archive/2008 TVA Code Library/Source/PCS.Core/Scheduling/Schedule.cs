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
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Text;

namespace PCS.Scheduling
{
    #region [ Enumerations ]

    /// <summary>Schedule part text syntax.</summary>
    public enum SchedulePartTextSyntax
    {
        Any,
        EveryN,
        Range,
        Specific
    }

    /// <summary>Date/time elements related to scheduling.</summary>
    /// <remarks>This enumeration specifically corresponds to the UNIX crontab date/time elements.</remarks>
    public enum DateTimePart
    {
        /// <summary>Minute part.</summary>
        Minute,
        /// <summary>Hour part.</summary>
        Hour,
        /// <summary>Day part.</summary>
        Day,
        /// <summary>Month part.</summary>
        Month,
        /// <summary>Day of week part.</summary>
        DayOfWeek
    }

    #endregion

    /// <summary>Defines a schedule using rules defined using UNIX crontab syntax.</summary>
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
        /// Creates a new schedule with the default rule of "* * * * *".
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        public Schedule(string name)
            : this(name, "* * * * *")
        {
        }

        /// <summary>
        /// Creates a new schedule using the specified rule.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <param name="rule">Rule formated in UNIX crontab syntax.</param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Cron for cron format.
        /// </remarks>
        public Schedule(string name, string rule)
            : this(name, rule, "")
        {
        }

        /// <summary>
        /// Creates a new schedule using the specified rule.
        /// </summary>
        /// <param name="name">Name of the schedule.</param>
        /// <param name="rule">Rule formated in UNIX crontab syntax.</param>
        /// <param name="description">Description of defined schedule.</param>
        /// <remarks>
        /// See http://en.wikipedia.org/wiki/Cron for cron format.
        /// </remarks>
        public Schedule(string name, string rule, string description)
        {
            this.Name = name;
            this.Rule = rule;
            this.Description = description;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the schedule name.
        /// </summary>
        /// <value></value>
        /// <returns>The schedule name.</returns>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_name = value;
                else
                    throw new ArgumentNullException("Name");
            }
        }

        /// <summary>
        /// Gets or sets the schedule rule.
        /// </summary>
        /// <value></value>
        /// <returns>The schedule rule.</returns>
        public string Rule
        {
            get
            {
                return m_minutePart.Text + " " + m_hourPart.Text + " " + m_dayPart.Text + " " + m_monthPart.Text + " " + m_dayOfWeekPart.Text;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] scheduleParts = value.RemoveDuplicateWhiteSpace().Split(' ');

                    if (scheduleParts.Length == 5)
                    {
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
                    else
                    {
                        throw new ArgumentException("Schedule rule must have exactly 5 parts (Example: * * * * *).");
                    }
                }
                else
                {
                    throw new ArgumentNullException("Rule");
                }
            }
        }

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

        public DateTime LastDueAt
        {
            get
            {
                return m_lastDueAt;
            }
        }

        public SchedulePart MinutePart
        {
            get
            {
                return m_minutePart;
            }
        }

        public SchedulePart HourPart
        {
            get
            {
                return m_hourPart;
            }
        }

        public SchedulePart DayPart
        {
            get
            {
                return m_dayPart;
            }
        }

        public SchedulePart MonthPart
        {
            get
            {
                return m_monthPart;
            }
        }

        public SchedulePart DaysOfWeekPart
        {
            get
            {
                return m_dayOfWeekPart;
            }
        }

        /// <summary>
        /// Gets the current status of the schedule.
        /// </summary>
        /// <value></value>
        /// <returns>The current status of the schedule.</returns>
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
        /// Checks whether the schedule is due at the present system time.
        /// </summary>
        /// <returns>True if the schedule is due at the present system time; otherwise False.</returns>
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

        public override bool Equals(object obj)
        {
            Schedule other = obj as Schedule;

            if ((other != null) && other.Rule == this.Rule)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Rule.GetHashCode();
        }

        #endregion
   }
}