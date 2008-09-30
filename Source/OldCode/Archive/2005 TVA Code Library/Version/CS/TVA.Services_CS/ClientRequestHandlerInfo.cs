//*******************************************************************************************************
//  ClientRequestHandlerInfo.cs
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
//  05/02/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Services
{
	public class ClientRequestHandlerInfo
	{
        #region [ Members ]

        // Fields
        public string Command;
        public string CommandDescription;
        public Action<ClientRequestInfo> HandlerMethod;
        public bool IsAdvertised;

        #endregion

        #region [ Constructors ]

        public ClientRequestHandlerInfo(string requestCommand, string requestDescription, Action<ClientRequestInfo> handlerMethod)
            : this(requestCommand, requestDescription, handlerMethod, true)
        {
        }

        public ClientRequestHandlerInfo(string requestCommand, string requestDescription, Action<ClientRequestInfo> handlerMethod, bool isAdvertised)
        {
            Command = requestCommand;
            CommandDescription = requestDescription;
            HandlerMethod = handlerMethod;
            IsAdvertised = isAdvertised;
        }

        #endregion
	}
}
