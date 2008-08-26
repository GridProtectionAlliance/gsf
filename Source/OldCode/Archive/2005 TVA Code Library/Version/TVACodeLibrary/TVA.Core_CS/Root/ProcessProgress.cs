using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 05/23/2007

namespace TVA
{
	[Serializable()]public class ProcessProgress<TUnit>
	{
		
		
		#region " Member Declaration "
		
		private string m_processName;
		private string m_progressMessage;
		private TUnit m_total;
		private TUnit m_complete;
		
		#endregion
		
		#region " Code Scope: Public "
		
		public ProcessProgress(string processName)
		{
			
			m_processName = processName;
			
		}
		
		public string ProcessName
		{
			get
			{
				return m_processName;
			}
			set
			{
				m_processName = value;
			}
		}
		
		public string ProgressMessage
		{
			get
			{
				return m_progressMessage;
			}
			set
			{
				m_progressMessage = value;
			}
		}
		
		public TUnit Total
		{
			get
			{
				return m_total;
			}
			set
			{
				m_total = value;
			}
		}
		
		public TUnit Complete
		{
			get
			{
				return m_complete;
			}
			set
			{
				m_complete = value;
			}
		}
		
		#endregion
		
	}
	
}
