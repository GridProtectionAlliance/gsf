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
	[Serializable()]public class ObjectState<TState>
	{
		
		
		#region " Member Declaration "
		
		private string m_objectName;
		private TState m_currentState;
		private TState m_previousState;
		
		#endregion
		
		#region " Code Scope: Public "
		
		public ObjectState(string objectName) : this(objectName, null)
		{
			
			
		}
		
		public ObjectState(string objectName, TState currentState) : this(objectName, null, currentState)
		{
			
			
		}
		
		public ObjectState(string objectName, TState previousState, TState currentState)
		{
			
			m_objectName = objectName;
			m_currentState = currentState;
			m_previousState = previousState;
			
		}
		
		public string ObjectName
		{
			get
			{
				return m_objectName;
			}
			set
			{
				m_objectName = value;
			}
		}
		
		public TState CurrentState
		{
			get
			{
				return m_currentState;
			}
			set
			{
				m_currentState = value;
			}
		}
		
		public TState PreviousState
		{
			get
			{
				return m_previousState;
			}
			set
			{
				m_previousState = value;
			}
		}
		
		#endregion
		
	}
	
}
