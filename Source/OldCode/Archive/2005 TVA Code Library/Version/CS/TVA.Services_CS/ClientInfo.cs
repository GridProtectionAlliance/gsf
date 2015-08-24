//*******************************************************************************************************
//  ClientInfo.cs
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
//  09/12/2006 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Threading;
using System.Collections.Generic;

namespace TVA.Services
{
	[Serializable()]
    public class ClientInfo
	{
        #region [ Members ]

        // Fields
        public Guid ClientID;
        public ApplicationType ClientType;
        public string ClientName;
        public string UserName;
        public string MachineName;
        public DateTime ConnectedAt;

        #endregion

        #region [ Constructors ]

        public ClientInfo(Guid clientID)
        {
            ClientID = clientID;
            ClientType = Common.GetApplicationType();
            
            UserName = Thread.CurrentPrincipal.Identity.Name;

            if (string.IsNullOrEmpty(UserName))
                UserName = Environment.UserDomainName + "\\" + Environment.UserName;

            MachineName = Environment.MachineName;

            if (ClientType == ApplicationType.WindowsCui || ClientType == ApplicationType.WindowsGui)
            {
                ClientName = AppDomain.CurrentDomain.FriendlyName;
            }
            else if (ClientType == ApplicationType.Web)
            {
                ClientName = System.Web.HttpContext.Current.Request.ApplicationPath;
            }
        }

        #endregion
	}
}
