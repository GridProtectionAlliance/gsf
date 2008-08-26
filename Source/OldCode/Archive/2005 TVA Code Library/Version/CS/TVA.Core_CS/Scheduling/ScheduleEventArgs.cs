using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace TVA
{
	namespace Scheduling
	{
		
		public class ScheduleEventArgs : EventArgs
		{
			
			
			private Schedule m_schedule;
			
			public ScheduleEventArgs(Schedule schedule)
			{
				
				m_schedule = schedule;
				
			}
			
			public Schedule Schedule
			{
				get
				{
					return m_schedule;
				}
				set
				{
					m_schedule = value;
				}
			}
			
		}
		
	}
}
