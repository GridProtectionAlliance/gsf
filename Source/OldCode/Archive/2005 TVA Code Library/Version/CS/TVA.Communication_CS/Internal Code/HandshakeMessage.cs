using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 07-06-06

namespace TVA.Communication
{
	[Serializable()]internal class HandshakeMessage
	{
		
		
		private Guid m_id;
		private string m_passphrase;
		
		public HandshakeMessage(Guid id, string passphrase)
		{
			
			this.ID = id;
			this.Passphrase = passphrase;
			
		}
		
		/// <summary>
		/// Gets or sets the connecting client's ID.
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
		
		/// <summary>
		/// Gets or sets the passphrase used for authentication.
		/// </summary>
		public string Passphrase
		{
			get
			{
				return m_passphrase;
			}
			set
			{
				m_passphrase = value;
			}
		}
		
	}
	
}
