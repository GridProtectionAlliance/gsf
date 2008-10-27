//*******************************************************************************************************
//  SchedulePart.cs
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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PCS.Scheduling
{
    /// <summary>Defines a schedule part.</summary>
    public class SchedulePart
    {
        #region [ Members ]

        // Fields
        private string m_text;
        private DateTimePart m_dateTimePart;
        private SchedulePartTextSyntax m_textSyntax;
        private List<int> m_values;

        #endregion

        #region [ Constructors ]

        public SchedulePart(string text, DateTimePart dateTimePart)
        {
            if (ValidateAndPopulate(text, dateTimePart))
            {
                // The text provided for populating the values is valid according to the specified date-time part.
                m_text = text;
                m_dateTimePart = dateTimePart;
            }
            else
            {
                throw new ArgumentException("Text is not valid for " + dateTimePart + " schedule part.");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the text used for populating the values of the schedule part.
        /// </summary>
        /// <value></value>
        /// <returns>The text used for populating the values of the schedule part.</returns>
        public string Text
        {
            get
            {
                return m_text;
            }
        }

        /// <summary>
        /// Gets the date-time part that the schedule part represents in a PCS.Scheduling.Schedule.
        /// </summary>
        /// <value></value>
        /// <returns>The date-time part that the schedule part represents in a PCS.Scheduling.Schedule.</returns>
        public DateTimePart DateTimePart
        {
            get
            {
                return m_dateTimePart;
            }
        }

        /// <summary>
        /// Gets the syntax used in the text specified for populating the values of the schedule part.
        /// </summary>
        /// <value></value>
        /// <returns>The syntax used in the text specified for populating the values of the schedule part.</returns>
        public SchedulePartTextSyntax TextSyntax
        {
            get
            {
                return m_textSyntax;
            }
        }

        /// <summary>
        /// Gets a meaningful description of the schedule part.
        /// </summary>
        /// <value></value>
        /// <returns>A meaningful description of the schedule part.</returns>
        public string Description
        {
            get
            {
                switch (m_textSyntax)
                {
                    case SchedulePartTextSyntax.Any:
                        return "Any " + m_dateTimePart.ToString();
                    case SchedulePartTextSyntax.EveryN:
                        return "Every " + m_text.Split('/')[1] + " " + m_dateTimePart.ToString();
                    case SchedulePartTextSyntax.Range:
                        string[] range = m_text.Split('-');
                        return m_dateTimePart.ToString() + " " + range[0] + " to " + range[1];
                    case SchedulePartTextSyntax.Specific:
                        return m_dateTimePart.ToString() + " " + m_text;
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Gets a list of values that were populated from based on the specified text and date-time part that the
        /// schedule part represents.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A list of values that were populated from based on the specified text and date-time part that the
        /// schedule part represents.</returns>
        public List<int> Values
        {
            get
            {
                return m_values;
            }
        }

        #endregion

        #region [ Methods ]

        public bool Matches(DateTime dateTime)
        {
            switch (m_dateTimePart)
            {
                case DateTimePart.Minute:
                    return m_values.Contains(dateTime.Minute);
                case DateTimePart.Hour:
                    return m_values.Contains(dateTime.Hour);
                case DateTimePart.Day:
                    return m_values.Contains(dateTime.Day);
                case DateTimePart.Month:
                    return m_values.Contains(dateTime.Month);
                case DateTimePart.DayOfWeek:
                    return m_values.Contains((int)dateTime.DayOfWeek);
                default:
                    return false;
            }
        }

        private bool ValidateAndPopulate(string schedulePart, DateTimePart dateTimePart)
        {
            int minValue = 0;
            int maxValue = 0;

            switch (dateTimePart)
            {
                case DateTimePart.Minute:
                    maxValue = 59;
                    break;
                case DateTimePart.Hour:
                    maxValue = 23;
                    break;
                case DateTimePart.Day:
                    minValue = 1;
                    maxValue = 31;
                    break;
                case DateTimePart.Month:
                    minValue = 1;
                    maxValue = 12;
                    break;
                case DateTimePart.DayOfWeek:
                    maxValue = 6;
                    break;
            }

            m_values = new List<int>();

            if (Regex.Match(schedulePart, "^(\\*){1}$").Success)
            {
                // ^(\*){1}$             Matches: *
                m_textSyntax = SchedulePartTextSyntax.Any;
                PopulateValues(minValue, maxValue, 1);

                return true;
            }
            else if (Regex.Match(schedulePart, "^(\\*/\\d+){1}$").Success)
            {
                // ^(\*/\d+){1}$         Matches: */[any digit]
                int interval = Convert.ToInt32(schedulePart.Split('/')[1]);
                if (interval > 0 && interval >= minValue && interval <= maxValue)
                {
                    m_textSyntax = SchedulePartTextSyntax.EveryN;
                    PopulateValues(minValue, maxValue, interval);

                    return true;
                }
            }
            else if (Regex.Match(schedulePart, "^(\\d+\\-\\d+){1}$").Success)
            {
                // ^(\d+\-\d+){1}$       Matches: [any digit]-[any digit]
                string[] range = schedulePart.Split('-');
                int lowRange = Convert.ToInt32(range[0]);
                int highRange = Convert.ToInt32(range[1]);
                if (lowRange < highRange && lowRange >= minValue && highRange <= maxValue)
                {
                    m_textSyntax = SchedulePartTextSyntax.Range;
                    PopulateValues(lowRange, highRange, 1);

                    return true;
                }
            }
            else if (Regex.Match(schedulePart, "^((\\d+,?)+){1}$").Success)
            {
                // ^((\d+,?)+){1}$       Matches: [any digit] AND [any digit], ..., [any digit]
                m_textSyntax = SchedulePartTextSyntax.Specific;

                int value;

                foreach (string part in schedulePart.Split(','))
                {
                    if (int.TryParse(part, out value))
                    {
                        if (!(value >= minValue && value <= maxValue))
                        {
                            return false;
                        }
                        else
                        {
                            if (!m_values.Contains(value))
                                m_values.Add(value);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private void PopulateValues(int fromValue, int toValue, int stepValue)
        {
            for (int i = fromValue; i <= toValue; i += stepValue)
            {
                m_values.Add(i);
            }
        }

        #endregion
    }
}