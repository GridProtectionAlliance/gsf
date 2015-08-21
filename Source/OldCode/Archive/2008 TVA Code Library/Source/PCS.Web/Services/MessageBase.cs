using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Data;

// 02/14/2007


namespace PCS.Web
{
	namespace Services
	{
		
		public abstract class MessageBase
		{
			
			
			private string m_messageName;
			private DataSet m_messageData;
			
			public abstract DataSet GetData();
			
			protected abstract DataTable GetExportInformation();
			protected abstract DataTable GetSourceInformation();
			protected abstract DataTable GetExceptions();
			
			public MessageBase(string message)
			{
				this.m_messageName = message;
				m_messageData = new DataSet(message);
				
			}
			
			public string BuildMessageData()
			{
				
				m_messageData.Merge(GetData());
				m_messageData.Tables.Add(GetExceptions());
				m_messageData.Tables.Add(GetExportInformation());
				m_messageData.Tables.Add(GetSourceInformation());
				
				return m_messageData.GetXml();
				
			}
			
			
		}
		
	}
}
