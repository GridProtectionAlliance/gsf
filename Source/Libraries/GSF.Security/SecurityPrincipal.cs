//******************************************************************************************************
//  SecurityPrincipal.cs - Gbtc
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
//  12/03/2010 - Pinal C. Patel
//       Modified IsInRole() to allow for translation of role name before applying security using it.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Security.Principal;

namespace GSF.Security
{
    /// <summary>
    /// A class that implements <see cref="IPrincipal"/> interface to facilitate custom role-based security.
    /// </summary>
    /// <seealso cref="SecurityIdentity"/>
    /// <seealso cref="ISecurityProvider"/>
    public class SecurityPrincipal : IPrincipal
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPrincipal"/> class.
        /// </summary>
        /// <param name="identity">An <see cref="SecurityIdentity"/> object.</param>
        /// <exception cref="ArgumentNullException">Value specified for <paramref name="identity"/> is null.</exception>
        public SecurityPrincipal(SecurityIdentity identity) => 
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="SecurityIdentity"/> object of the user.
        /// </summary>
        public SecurityIdentity Identity{ get; }

        /// <summary>
        /// Gets the <see cref="IIdentity"/> object of the user.
        /// </summary>
        IIdentity IPrincipal.Identity => Identity;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the user is a member of either of the specified <paramref name="roles"/>.
        /// </summary>
        /// <param name="roles">Comma separated list of roles to check.</param>
        /// <returns>true if the user is a member of either of the specified <paramref name="roles"/>, otherwise false.</returns>
        public bool IsInRole(string roles)
        {
            if (!Identity.Provider.UserData.IsDefined || !Identity.Provider.IsUserAuthenticated ||
                Identity.Provider.UserData.IsDisabled || Identity.Provider.UserData.IsLockedOut)
            {
                // No need to check user roles.
                return false;
            }

            // Check if user has any one of the specified roles.
            foreach (string role in roles.Split(','))
            {
                if (Identity.Provider.UserData.Roles.FirstOrDefault(currentRole => SecurityProviderUtility.IsRegexMatch(Identity.Provider.TranslateRole(role.Trim()), currentRole)) is not null)
                    return true;
            }

            return false;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets the reason phrase to return for an unauthorized response.
        /// </summary>
        /// <param name="securityPrincipal">Security principal being authenticated, can be <c>null</c>.</param>
        /// <param name="authorizationScheme">Authentication scheme in use.</param>
        /// <param name="useProviderReason"><c>true</c> to use detailed response from security provider.</param>
        /// <returns>Reason phrase to return for an unauthorized response.</returns>
        /// <remarks>
        /// Detailed provider response should normally only be used for diagnostics, a more obscure reason is considered
        /// more secure since it limits knowledge about the successful elements of an authentication attempt.
        /// </remarks>
        public static string GetFailureReasonPhrase(SecurityPrincipal securityPrincipal, string authorizationScheme = "Basic", bool useProviderReason = false)
        {
            if (securityPrincipal is null)
                return "Invalid user name or password";

            if (useProviderReason)
            {
                // The security provider should be able to provide a reason for the failure
                string failureReason = securityPrincipal.Identity.Provider?.AuthenticationFailureReason;

                if (!string.IsNullOrEmpty(failureReason))
                    return failureReason;
            }

            return authorizationScheme == "Basic" ? 
                "Invalid user name or password" : 
                "Missing credentials";
        }

        #endregion
    }
}
