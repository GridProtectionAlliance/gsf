//******************************************************************************************************
//  RoleBasedSecurityAttribute.cs - Gbtc
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
//  02/23/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Security;
using System.Threading;
using System.Web.Mvc;
using GSF.Collections;
using GSF.Data;
using GSF.Identity;
using GSF.Security;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines an MVC filter attribute to handle the GSF role based security model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthorizeControllerRoleAttribute : FilterAttribute, IAuthorizationFilter, IExceptionFilter
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Gets allowed roles as a string array.
        /// </summary>
        public readonly string[] AllowedRoles;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new MVC based authorization attribute.
        /// </summary>
        public AuthorizeControllerRoleAttribute()
        {
            AllowedRoles = new string[0];
            SettingsCategory = "securityProvider";
            SecurityExceptionViewName = "SecurityError";
        }

        /// <summary>
        /// Creates a new MVC based authorization attribute for specified roles.
        /// </summary>
        public AuthorizeControllerRoleAttribute(string allowedRoles)
        {
            AllowedRoles = allowedRoles?.Split(',').Select(role => role.Trim()).
                Where(role => !string.IsNullOrEmpty(role)).ToArray() ?? new string[0];

            SettingsCategory = "securityProvider";
            SecurityExceptionViewName = "SecurityError";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets settings category to use for loading data context for security info.
        /// </summary>
        public string SettingsCategory
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets view name to show for security exceptions.
        /// </summary>
        public string SecurityExceptionViewName
        {
            get; set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            SecurityProviderCache.ValidateCurrentProvider();

            // Setup the principal
            filterContext.HttpContext.User = Thread.CurrentPrincipal;

            // Get current user name
            string userName = Thread.CurrentPrincipal.Identity.Name;

            // Verify that the current thread principal has been authenticated.
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                throw new SecurityException($"Authentication failed for user '{userName}': {SecurityProviderCache.CurrentProvider.AuthenticationFailureReason}");

            if (AllowedRoles.Length > 0 && !AllowedRoles.Any(role => filterContext.HttpContext.User.IsInRole(role)))
                throw new SecurityException($"Access is denied for user '{userName}': minimum required roles = {AllowedRoles.ToDelimitedString(", ")}.");

            // Make sure current user ID is cached
            if (!AuthorizationCache.UserIDs.ContainsKey(userName))
            {
                using (AdoDataConnection connection = new AdoDataConnection(SettingsCategory))
                {
                    Guid? userID = connection.ExecuteScalar<Guid?>("SELECT ID FROM UserAccount WHERE Name={0}", UserInfo.UserNameToSID(userName));

                    if ((object)userID != null)
                        AuthorizationCache.UserIDs.TryAdd(userName, userID.GetValueOrDefault());
                }
            }
        }

        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is SecurityException)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = SecurityExceptionViewName,
                    ViewData = new ViewDataDictionary
                    {
                        ["Exception"] = filterContext.Exception
                    }
                };
                filterContext.ExceptionHandled = true;
            }
        }

        #endregion
    }
}
