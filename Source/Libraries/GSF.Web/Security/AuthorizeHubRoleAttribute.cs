//******************************************************************************************************
//  AuthorizeHubRoleAttribute.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/25/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using GSF.Security;
using Microsoft.AspNet.SignalR;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines a SignalR authorization attribute to handle the GSF role based security model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthorizeHubRoleAttribute : AuthorizeAttribute
    {
        #region [ Members ]

        // Fields
        private string[] m_allowedRoles;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AuthorizeHubRoleAttribute"/>.
        /// </summary>
        public AuthorizeHubRoleAttribute()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AuthorizeHubRoleAttribute"/> with specified allowed roles.
        /// </summary>
        public AuthorizeHubRoleAttribute(string allowedRoles)
        {
            Roles = allowedRoles;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets settings category used to lookup security connection for user data context.
        /// </summary>
        public string SecuritySettingsCategory { get; set; } = "securityProvider";

        /// <summary>
        /// Gets the allowed <see cref="AuthorizeAttribute.Roles"/> as a string array.
        /// </summary>
        public string[] AllowedRoles => m_allowedRoles ?? (m_allowedRoles = Roles?.Split(',').Select(role => role.Trim()).Where(role => !string.IsNullOrEmpty(role)).ToArray() ?? new string[0]);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Provides an entry point for custom authorization checks.
        /// </summary>
        /// <param name="user">The <see cref="IPrincipal"/> for the client being authorize</param>
        /// <returns>
        /// <c>true</c> if the user is authorized, otherwise, <c>false</c>.
        /// </returns>
        protected override bool UserAuthorized(IPrincipal user)
        {
            SecurityPrincipal securityPrincipal = user as SecurityPrincipal;

            if ((object)securityPrincipal == null)
                return false;

            // Verify that the current thread principal has been authenticated.
            if (!securityPrincipal.Identity.IsAuthenticated)
                return false;

            if (AllowedRoles.Length > 0 && !AllowedRoles.Any(role => securityPrincipal.IsInRole(role)))
                return false;            

            ThreadPool.QueueUserWorkItem(start => AuthorizationCache.CacheAuthorization(securityPrincipal.Identity.Name, SecuritySettingsCategory));

            return true;
        }

        #endregion
    }
}