using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 07-06-06

namespace TVA.Communication
{
	[Serializable()]internal class GoodbyeMessage
	{
		
		
		private Guid m_id;
		
		public GoodbyeMessage(Guid id)
		{
			this.ID = id;
		}
		
		/// <summary>
		/// Gets or sets the disconnecting client's ID.
		/// </summary>
		public Guid ID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}
		
	}
	
}
