//*******************************************************************************************************
//  GoodbyeMessage.cs
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
    internal class GoodbyeMessage
	{
        /// <summary>
        /// Gets or sets the disconnecting client's ID.
        /// </summary>
        public Guid ID;
		
		public GoodbyeMessage(Guid id)
		{
			ID = id;
		}
	}
}
