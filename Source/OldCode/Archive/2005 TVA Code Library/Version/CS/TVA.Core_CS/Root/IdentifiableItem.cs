using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 03/12/2007

namespace TVA
{
	public class IdentifiableItem<TIdentifier, TItem>
	{
		
		
		private TIdentifier m_source;
		private TItem m_item;
		
		public IdentifiableItem(TIdentifier source, TItem item)
		{
			
			m_source = source;
			m_item = item;
			
		}
		
		public TIdentifier Source
		{
			get
			{
				return m_source;
			}
			set
			{
				m_source = value;
			}
		}
		
		public TItem Item
		{
			get
			{
				return m_item;
			}
			set
			{
				m_item = value;
			}
		}
		
	}
	
}
