//*******************************************************************************************************
//  HandshakeMessage.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Communication
{
	[Serializable()]
    internal class HandshakeMessage
	{
		private Guid m_id;
		private string m_passphrase;
		
		public HandshakeMessage(Guid id, string passphrase)
		{
			ID = id;
			Passphrase = passphrase;
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
