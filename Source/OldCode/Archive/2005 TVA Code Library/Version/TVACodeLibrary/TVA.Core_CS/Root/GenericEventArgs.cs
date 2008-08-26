using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 11/24/20006

namespace TVA
{
	public class GenericEventArgs<T> : EventArgs
	{
		
		
		private T m_argument;
		
		public GenericEventArgs(T argument)
		{
			
			m_argument = argument;
			
		}
		
		public T Argument
		{
			get
			{
				return m_argument;
			}
			set
			{
				m_argument = value;
			}
		}
		
	}
	
}
