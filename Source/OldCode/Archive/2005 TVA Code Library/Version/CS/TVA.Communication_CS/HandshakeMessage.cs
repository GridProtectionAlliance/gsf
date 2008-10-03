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
        /// <summary>
        /// Gets or sets the connecting client's ID.
        /// </summary>
        public Guid ID;

        /// <summary>
        /// Gets or sets the passphrase used for authentication.
        /// </summary>
        public string Passphrase;
		
		public HandshakeMessage(Guid id, string passphrase)
		{
			ID = id;
			Passphrase = passphrase;
		}
	}
}
