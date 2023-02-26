//******************************************************************************************************
//  SecurityIdentity.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Security.Principal;
using GSF.Diagnostics;
using GSF.Identity;

namespace GSF.Security
{
    /// <summary>
    /// A class that implements <see cref="IIdentity"/> interface to facilitate custom role-based security.
    /// </summary>
    /// <seealso cref="SecurityPrincipal"/>
    /// <seealso cref="ISecurityProvider"/>
    public class SecurityIdentity : IIdentity
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityIdentity"/> class.
        /// </summary>
        /// <param name="provider">An <see cref="ISecurityProvider"/> of the user.</param>
        /// <exception cref="ArgumentNullException">Value specified for <paramref name="provider"/> is null.</exception>
        public SecurityIdentity(ISecurityProvider provider) => 
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the type of authentication used to identify the user.
        /// </summary>
        public string AuthenticationType => Provider.GetType().Name;

        /// <summary>
        /// Gets a boolean value that indicates whether the user has been authenticated.
        /// </summary>
        public bool IsAuthenticated => Provider.IsUserAuthenticated;

        /// <summary>
        /// Gets the user's login name.
        /// </summary>
        public string Name => Provider.UserData.Username;

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public string Type
        {
            get
            {
                try
                {
                    if (Provider.UserData.IsExternal)
                        return Provider.UserData.IsAzureAD ? 
                            "Azure AD" : 
                            "Database";

                    string[] accountParts = Provider.UserData.LoginID.Split('\\');

                    if (accountParts.Length == 2)
                        return UserInfo.IsLocalDomain(accountParts[0].Trim()) ? 
                            "Local Account" :
                            "Active Directory";
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex);
                }

                return "Local Account";
            }
        }

        /// <summary>
        /// Gets the <see cref="ISecurityProvider"/> of the user.
        /// </summary>
        public ISecurityProvider Provider { get; }

        #endregion
    }
}
