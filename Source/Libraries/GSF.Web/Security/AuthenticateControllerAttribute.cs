//******************************************************************************************************
//  AuthenticateControllerAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  08/18/2017 - J. Ritchie Carroll
//       Generated original version of source code based on ASP.NET BasicAuthentication sample.
//
//******************************************************************************************************

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using GSF.Configuration;
using GSF.Security;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines an MVC filter attribute to handle basic authentication using the current GSF security provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthenticateControllerAttribute : FilterAttribute, IAuthenticationFilter
    {
        #region [ Members ]

        // Fields
        private readonly AuthenticationOptions m_authenticationOptions = new AuthenticationOptions();
        private bool m_sessionTokenAssigned;
        private bool m_loginPageAssigned;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the case-sensitive identifier that defines the protection space for this authentication.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The "realm" authentication parameter is reserved for use by authentication schemes that wish to
        /// indicate a scope of protection.
        /// </para>
        /// <para>
        /// A protection space is defined by the canonical root URI (the scheme and authority components of the
        /// effective request URI) of the server being accessed, in combination with the realm value if present.
        /// These realms allow the protected resources on a server to be partitioned into a set of protection
        /// spaces, each with its own authentication scheme and/or authorization database. The realm value is a
        /// string, generally assigned by the origin server, that can have additional semantics specific to the
        /// authentication scheme. Note that a response can have multiple challenges with the same auth-scheme
        /// but with different realms.
        /// </para>
        /// </remarks>
        public string Realm
        {
            get
            {
                return m_authenticationOptions.Realm;
            }
            set
            {
                // AuthenticationOptions class validates Realm format
                m_authenticationOptions.Realm = value;
            }
        }

        /// <summary>
        /// Gets or sets settings category used to load configured settings. When defined,
        /// <see cref="SessionToken"/> and <see cref="LoginPage"/> will be loaded from the
        /// configuration file settings when not otherwise explicitly defined.
        /// </summary>
        public string SettingsCategory { get; set; } = "systemSettings";

        /// <summary>
        /// Gets or sets settings category used to lookup security connection for user data context.
        /// </summary>
        public string SecuritySettingsCategory { get; set; } = "securityProvider";

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken
        {
            get
            {
                return m_authenticationOptions.SessionToken;
            }
            set
            {
                m_authenticationOptions.SessionToken = value;
                m_sessionTokenAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage
        {
            get
            {
                return m_authenticationOptions.LoginPage;
            }
            set
            {
                m_authenticationOptions.LoginPage = value;
                m_loginPageAssigned = true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Authenticates the request.
        /// </summary>
        /// <returns>A Task that will perform authentication.</returns>
        /// <param name="context">The authentication context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // Do nothing if anonymous access is requested
            if (authorization?.Scheme == null)
                return Task.FromResult(0);

            LoadConfiguredSettings();

            // Try to retrieve any existing authorization
            SecurityPrincipal securityPrincipal = context.Principal as SecurityPrincipal;

            if ((object)securityPrincipal == null)
            {
                Guid sessionID;

                if (SessionHandler.TryGetSessionID(request, SessionToken, out sessionID))
                    AuthenticationHandler.TryGetPrincipal(sessionID, out securityPrincipal);
            }

            if (securityPrincipal?.Identity.IsAuthenticated ?? false)
                return Task.FromResult(0);

            string result;

            // In cases where authentication is being tested, such as a login page, a 401 (Unauthorized) is desired
            // instead of a redirect which could cause a cyclic redirection loop - to prevent these situations
            // a URL parameter can be specified to request the desired behavior, i.e., "?AuthFailRedirect=false"
            if (request.QueryParameters().TryGetValue("AuthFailRedirect", out result) && !result.ParseBoolean())
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request, HttpStatusCode.Unauthorized);
                return Task.FromResult(0);
            }

            // Not a valid user for current security provider, provide an obscure error message
            context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request, HttpStatusCode.Redirect, LoginPage);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Challenges the request.
        /// </summary>
        /// <returns>A Task that will perform challenge.</returns>
        /// <param name="context">The challenge context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            string parameter = null;

            if (!string.IsNullOrWhiteSpace(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Basic", parameter);

            return Task.FromResult(0);
        }

        private void LoadConfiguredSettings()
        {
            // Load configured settings, if not explicitly defined
            if (string.IsNullOrWhiteSpace(SettingsCategory) || m_sessionTokenAssigned && m_loginPageAssigned)
                return;

            if (!s_loadedConfiguredSettings)
            {
                try
                {
                    CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings[SettingsCategory];

                    systemSettings.Add("SessionToken", SessionHandler.DefaultSessionToken, "Defines the token used for identifying the session ID in cookie headers.");
                    systemSettings.Add("LoginPage", AuthenticationOptions.DefaultLoginPage, "Defines the login page, relative path, used as a redirect location when authentication fails.");

                    s_configuredSessionToken = systemSettings["SessionToken"].ValueAs(SessionHandler.DefaultSessionToken);
                    s_configuredLoginPage = systemSettings["LoginPage"].ValueAs(AuthenticationOptions.DefaultLoginPage);
                }
                finally
                {
                    s_loadedConfiguredSettings = true;
                }
            }

            if (!m_sessionTokenAssigned)
                SessionToken = s_configuredSessionToken;

            if (!m_loginPageAssigned)
                LoginPage = s_configuredLoginPage;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string s_configuredSessionToken;
        private static string s_configuredLoginPage;
        private static bool s_loadedConfiguredSettings;

        #endregion
    }
}
