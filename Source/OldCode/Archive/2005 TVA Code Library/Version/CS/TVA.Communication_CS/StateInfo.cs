//*******************************************************************************************************
//  StateInfo.cs
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
//  06/16/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Net.Sockets;

// JRC: Class properties converted to public fields for optimization...
namespace TVA.Communication
{
	public class StateInfo<T>
	{
		public T Client;
		public Guid ID;
		public string Passphrase;
		public byte[] DataBuffer;
		public DateTime LastSendTimestamp;
		public DateTime LastReceiveTimestamp;
	}
}
