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
using System.Security;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using GSF.Collections;
using GSF.Configuration;
using GSF.Security;

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
        private Guid m_sessionID;
        private string m_sessionToken = SessionHandler.DefaultSessionToken;
        private bool m_sessionTokenAssigned;

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
        /// Gets or sets settings category used to load configured settings. When defined,
        /// <see cref="SessionToken"/> will be loaded from the configuration file settings
        /// when not otherwise explicitly defined.
        /// </summary>
        public string SettingsCategory { get; set; } = "systemSettings";

        /// <summary>
        /// Gets or sets settings category used to lookup security connection for user data context.
        /// </summary>
        public string SecuritySettingsCategory { get; set; } = "securityProvider";

        /// <summary>
        /// Gets the allowed <see cref="AuthorizeAttribute.Roles"/> as a string array.
        /// </summary>
        public string[] AllowedRoles => m_allowedRoles ?? (m_allowedRoles = Roles?.Split(',').Select(role => role.Trim()).Where(role => !string.IsNullOrEmpty(role)).ToArray() ?? new string[0]);

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken
        {
            get
            {
                return m_sessionToken;
            }
            set
            {
                m_sessionToken = value;
                m_sessionTokenAssigned = true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether client is authorized to connect to <see cref="IHub" />.
        /// </summary>
        /// <param name="hubDescriptor">Description of the hub client is attempting to connect to.</param>
        /// <param name="request">The (re)connect request from the client.</param>
        /// <returns><c>true</c> if the caller is authorized to connect to the hub; otherwise, <c>false</c>.</returns>
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            LoadConfiguredSettings();
            m_sessionID = SessionHandler.GetSessionIDFromCookie(request, SessionToken);
            return base.AuthorizeHubConnection(hubDescriptor, request);
        }

        /// <summary>
        /// Determines whether client is authorized to invoke the <see cref="IHub" /> method.
        /// </summary>
        /// <param name="hubIncomingInvokerContext">An <see cref="IHubIncomingInvokerContext" /> providing details regarding the <see cref="IHub" /> method invocation.</param>
        /// <param name="appliesToMethod">Indicates whether the interface instance is an attribute applied directly to a method.</param>
        /// <returns><c>true</c> if the caller is authorized to invoke the <see cref="IHub" /> method; otherwise, <c>false</c>.</returns>
        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            LoadConfiguredSettings();
            m_sessionID = SessionHandler.GetSessionIDFromCookie(hubIncomingInvokerContext.Hub?.Context.Request, SessionToken);
            return base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
        }

        /// <summary>
        /// Provides an entry point for custom authorization checks.
        /// </summary>
        /// <param name="user">The <see cref="IPrincipal"/> for the client being authorize</param>
        /// <returns>
        /// <c>true</c> if the user is authorized, otherwise, <c>false</c>.
        /// </returns>
        protected override bool UserAuthorized(IPrincipal user)
        {
            SecurityPrincipal securityPrincipal;

            if (!AuthenticateControllerAttribute.TryGetPrincipal(m_sessionID, out securityPrincipal))
                return false;

            Thread.CurrentPrincipal = securityPrincipal;

            string username = securityPrincipal.Identity.Name;

            // Verify that the current thread principal has been authenticated.
            if (!securityPrincipal.Identity.IsAuthenticated)
                throw new SecurityException($"Authentication failed for user '{username}': {securityPrincipal.Identity.Provider.AuthenticationFailureReason}");

            if (AllowedRoles.Length > 0 && !AllowedRoles.Any(role => securityPrincipal.IsInRole(role)))
                throw new SecurityException($"Access is denied for user '{username}': minimum required roles = {AllowedRoles.ToDelimitedString(", ")}.");

            ThreadPool.QueueUserWorkItem(start => AuthorizationCache.CacheAuthorization(username, SecuritySettingsCategory));

            return true;
        }

        private void LoadConfiguredSettings()
        {
            // Load configured settings, if not explicitly defined
            if (string.IsNullOrWhiteSpace(SettingsCategory) || m_sessionTokenAssigned)
                return;

            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings[SettingsCategory];

            systemSettings.Add("SessionToken", SessionHandler.DefaultSessionToken, "Defines the token used for identifying the session ID in cookie headers.");

            if (!m_sessionTokenAssigned)
                SessionToken = systemSettings["SessionToken"].ValueAs(SessionHandler.DefaultSessionToken);
        }

        #endregion
    }
}