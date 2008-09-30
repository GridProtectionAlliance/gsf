//*******************************************************************************************************
//  ClientRequestInfo.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/11/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Services
{
	public class ClientRequestInfo
	{
        #region [ Members ]

        // Fields
        public ClientRequest Request;
        public ClientInfo Sender;
        public DateTime ReceivedAt;

        #endregion

        #region [ Constructors ]

        public ClientRequestInfo(ClientInfo sender, ClientRequest request)
            : this(sender, request, DateTime.Now)
        {
        }

        public ClientRequestInfo(ClientInfo sender, ClientRequest request, DateTime receivedAt)
        {
            Request = request;
            Sender = sender;
            ReceivedAt = receivedAt;
        }

        #endregion
	}
}
