using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
//using TVA.Text.Common;
using TVA.DateTime;


namespace TVA
{
	namespace Scheduling
	{
		
		public class Schedule
		{
			
			
			#region " Member declaration "
			
			private string m_name;
			private string m_description;
			private SchedulePart m_minutePart;
			private SchedulePart m_hourPart;
			private SchedulePart m_dayPart;
			private SchedulePart m_monthPart;
			private SchedulePart m_dayOfWeekPart;
			private System.DateTime m_lastDueAt;
			
			#endregion
			
			#region " Code Scope: Public "
			
			public Schedule(string name) : this(name, "* * * * *")
			{
				
				
			}
			
			public Schedule(string name, string rule) : this(name, rule, "")
			{
				
				
			}
			
			public Schedule(string name, string rule, string description)
			{
				
				this.Name = name;
				this.Rule = rule;
				this.Description = description;
				
			}
			
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
					if (! string.IsNullOrEmpty(value))
					{
						m_name = value;
					}
					else
					{
						throw (new ArgumentNullException("Name"));
					}
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
					if (! string.IsNullOrEmpty(value))
					{
						string with_1 = TVA.Text.Common.RemoveDuplicateWhiteSpace(value).Split(' ');
						if (with_1.Length == 5)
						{
							m_minutePart = new SchedulePart(with_1.GetValue(0).ToString(), DateTimePart.Minute);
							m_hourPart = new SchedulePart(with_1.GetValue(1).ToString(), DateTimePart.Hour);
							m_dayPart = new SchedulePart(with_1.GetValue(2).ToString(), DateTimePart.Day);
							m_monthPart = new SchedulePart(with_1.GetValue(3).ToString(), DateTimePart.Month);
							m_dayOfWeekPart = new SchedulePart(with_1.GetValue(4).ToString(), DateTimePart.DayOfWeek);
							
							// Update the schedule description.
							System.Text.StringBuilder with_2 = new StringBuilder();
							with_2.Append(m_minutePart.Description);
							with_2.Append(", ");
							with_2.Append(m_hourPart.Description);
							with_2.Append(", ");
							with_2.Append(m_dayPart.Description);
							with_2.Append(", ");
							with_2.Append(m_monthPart.Description);
							with_2.Append(", ");
							with_2.Append(m_dayOfWeekPart.Description);
							
							m_description = with_2.ToString();
						}
						else
						{
							throw (new ArgumentException("Schedule rule must have exactly 5 parts (Example: * * * * *)."));
						}
					}
					else
					{
						throw (new ArgumentNullException("Rule"));
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
					if (! string.IsNullOrEmpty(value))
					{
						m_description = value;
					}
				}
			}
			
			public System.DateTime LastDueAt
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
					System.Text.StringBuilder with_1 = new System.Text.StringBuilder();
					with_1.Append("             Schedule name: ");
					with_1.Append(m_name);
					with_1.AppendLine();
					with_1.Append("             Schedule rule: ");
					with_1.Append(Rule);
					with_1.AppendLine();
					with_1.Append("             Last run time: ");
					with_1.Append(m_lastDueAt == System.DateTime.MinValue ? "Never" : m_lastDueAt);
					with_1.AppendLine();
					
					return with_1.ToString();
				}
			}
			
			/// <summary>
			/// Checks whether the schedule is due at the present system time.
			/// </summary>
			/// <returns>True if the schedule is due at the present system time; otherwise False.</returns>
			public bool IsDue()
			{
				
				System.DateTime currentDateTime = System.DateTime.Now;
				if (m_minutePart.Matches(currentDateTime) && m_hourPart.Matches(currentDateTime) && m_dayPart.Matches(currentDateTime) && m_monthPart.Matches(currentDateTime) && m_dayOfWeekPart.Matches(currentDateTime))
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
				if ((other != null)&& other.Name == this.Name && other.Rule == this.Rule)
				{
					return true;
				}
				else
				{
					return false;
				}
				
			}
			
			#endregion
			
		}
		
	}
}
