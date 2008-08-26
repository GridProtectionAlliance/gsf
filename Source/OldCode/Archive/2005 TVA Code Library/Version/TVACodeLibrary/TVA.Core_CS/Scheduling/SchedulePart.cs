using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;


namespace TVA
{
	namespace Scheduling
	{
		
		public class SchedulePart
		{
			
			
			#region " Member Declaration "
			
			private string m_text;
			private TVA.DateTime.DateTimePart m_dateTimePart;
			private SchedulePartTextSyntax m_textSyntax;
			private List<int> m_values;
			
			#endregion
			
			#region " Public Code "
			
			public SchedulePart(string text, TVA.DateTime.DateTimePart dateTimePart)
			{
				
				if (ValidateAndPopulate(text, dateTimePart))
				{
					// The text provided for populating the values is valid according to the specified date-time part.
					m_text = text;
					m_dateTimePart = dateTimePart;
				}
				else
				{
					throw (new ArgumentException("Text is not valid for " + DateTimePart.ToString() + " schedule part."));
				}
				
			}
			
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
			/// Gets the date-time part that the schedule part represents in a TVA.Scheduling.Schedule.
			/// </summary>
			/// <value></value>
			/// <returns>The date-time part that the schedule part represents in a TVA.Scheduling.Schedule.</returns>
			public TVA.DateTime.DateTimePart DateTimePart
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
			
			public bool Matches(System.DateTime dateAndTime)
			{
				
				if (m_dateTimePart == TVA.DateTime.DateTimePart.Minute)
				{
					return m_values.Contains(dateAndTime.Minute);
				}
				else if (m_dateTimePart == TVA.DateTime.DateTimePart.Hour)
				{
					return m_values.Contains(dateAndTime.Hour);
				}
				else if (m_dateTimePart == TVA.DateTime.DateTimePart.Day)
				{
					return m_values.Contains(dateAndTime.Day);
				}
				else if (m_dateTimePart == TVA.DateTime.DateTimePart.Month)
				{
					return m_values.Contains(dateAndTime.Month);
				}
				else if (m_dateTimePart == TVA.DateTime.DateTimePart.DayOfWeek)
				{
					return m_values.Contains(Convert.ToInt32(dateAndTime.DayOfWeek));
				}
				
			}
			
			#endregion
			
			#region " Private Code"
			
			private bool ValidateAndPopulate(string schedulePart, TVA.DateTime.DateTimePart dateTimePart)
			{
				
				int minValue = 0;
				int maxValue = 0;
				if (dateTimePart == DateTime.DateTimePart.Minute)
				{
					maxValue = 59;
				}
				else if (dateTimePart == DateTime.DateTimePart.Hour)
				{
					maxValue = 23;
				}
				else if (dateTimePart == DateTime.DateTimePart.Day)
				{
					minValue = 1;
					maxValue = 31;
				}
				else if (dateTimePart == DateTime.DateTimePart.Month)
				{
					minValue = 1;
					maxValue = 12;
				}
				else if (dateTimePart == DateTime.DateTimePart.DayOfWeek)
				{
					maxValue = 6;
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
					foreach (int value in schedulePart.Split(','))
					{
						if (!(value >= minValue && value <= maxValue))
						{
							return false;
						}
						else
						{
							if (! m_values.Contains(value))
							{
								m_values.Add(value);
							}
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
}
