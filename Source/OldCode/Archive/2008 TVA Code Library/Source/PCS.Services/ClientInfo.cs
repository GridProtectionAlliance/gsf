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
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Threading;
using System.Web;

namespace PCS.Services
{
    /// <summary>
    /// Represents information about a client using <see cref="ClientHelper"/> for connecting to a Windows Service that uses <see cref="ServiceHelper"/>.
    /// </summary>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ServiceHelper"/>
	[Serializable()]
    public class ClientInfo
	{
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInfo"/> class.
        /// </summary>
        public ClientInfo()
            : this(Guid.Empty)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInfo"/> class.
        /// </summary>
        /// <param name="clientID">ID of the remote client.</param>
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
                ClientName = HttpContext.Current.Request.ApplicationPath;
            }
        }

        #region [ Properties ]

        /// <summary>
        /// Gets the ID of the remote client application.
        /// </summary>
        public Guid ClientID { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the remote client application connected to the <see cref="ServiceHelper"/>.
        /// </summary>
        public DateTime ConnectedAt { get; set; }

        /// <summary>
        /// Gets the <see cref="ApplicationType"/> of the remote client application.
        /// </summary>
        public ApplicationType ClientType { get; private set; }

        /// <summary>
        /// Gets the name of the remote client application.
        /// </summary>
        public string ClientName { get; private set; }

        /// <summary>
        /// Gets the name of the user running the remote client application.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the name of the machine running the remote client application.
        /// </summary>
        public string MachineName { get; private set; }

        #endregion

        #endregion
    }
}
