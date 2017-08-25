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
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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

        // Constants

        /// <summary>
        /// Default value for <see cref="LoginPage"/>.
        /// </summary>
        public const string DefaultLoginPage = "/Login.cshtml";

        // Fields
        private string m_realm;
        private string m_sessionToken = SessionHandler.DefaultSessionToken;
        private string m_loginPage = DefaultLoginPage;
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
                return m_realm;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    m_realm = null;
                    return;
                }

                // Verify that Realm does not contain a quote character unless properly
                // escaped, i.e., preceded by a backslash that is not itself escaped
                if (value.Length != Regex.Replace(value, @"\\\\""|(?<!\\)\""", "").Length)
                    throw new FormatException($"Realm value \"{value}\" contains an embedded quote that is not properly escaped.");

                m_realm = value;
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
                return m_sessionToken;
            }
            set
            {
                m_sessionToken = value;
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
                return m_loginPage;
            }
            set
            {
                m_loginPage = value;
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
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            // Do nothing if anonymous access is requested
            if (authorization?.Scheme == null)
                return;

            LoadConfiguredSettings();

            Guid sessionID;
            SecurityPrincipal cachedPrincipal;
            bool hasSession = SessionHandler.TryGetSessionID(request, SessionToken, out sessionID);
            string authorizationParameter = null;

            // Try to retrieve any existing authorization
            if (hasSession && s_authorizationCache.TryGetValue(sessionID, out cachedPrincipal) && cachedPrincipal.Identity.IsAuthenticated)
                return;

            if (authorization.Scheme != "Basic")
            {
                // Assuming pass-through authentication - make sure user is defined in security provider, authenticated and has a role
                SecurityPrincipal securityPrincipal = context.Principal as SecurityPrincipal;

                if ((object)securityPrincipal == null)
                {
                    string username = context.Principal?.Identity.Name;

                    if (!string.IsNullOrEmpty(username))
                    {
                        ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username);
                        securityProvider.PassthroughPrincipal = context.Principal;
                        securityProvider.Authenticate();

                        SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                        securityPrincipal = new SecurityPrincipal(securityIdentity);
                    }
                }

                if ((object)securityPrincipal != null && securityPrincipal.Identity.IsAuthenticated && securityPrincipal.Identity.Provider.UserData.Roles.Count > 0)
                {
                    // User is valid, do not set principal, which would indicate success, nor ErrorResult,
                    // which would indicate an error
                    if (hasSession)
                        s_authorizationCache[sessionID] = securityPrincipal;

                    return;
                }

                // Not a valid user for current security provider, provide an obscure error message
                context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request, HttpStatusCode.Redirect, LoginPage);
                return;
            }

            if (string.IsNullOrEmpty(authorizationParameter))
                authorizationParameter = authorization.Parameter;

            if (string.IsNullOrEmpty(authorizationParameter))
            {
                // No authorization credentials were provided, set ErrorResult
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request, HttpStatusCode.Redirect, LoginPage);
                return;
            }

            await Task.Run(() =>
            {
                string username, password;

                if (TryParseCredentials(authorizationParameter, out username, out password))
                {
                    // Setup the security principal
                    ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username);
                    securityProvider.Password = password;
                    securityProvider.Authenticate();

                    SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
                    SecurityPrincipal securityPrincipal = new SecurityPrincipal(securityIdentity);

                    // Authenticate user, if not already authenticated
                    if (securityPrincipal.Identity.IsAuthenticated && securityProvider.UserData.Roles.Count > 0)
                    {
                        context.Principal = securityPrincipal;

                        if (hasSession)
                            s_authorizationCache[sessionID] = securityPrincipal;

                        ThreadPool.QueueUserWorkItem(start => AuthorizationCache.CacheAuthorization(username, SecuritySettingsCategory));
                    }
                    else
                    {
                        // Authentication was attempted but failed, set ErrorResult - don't redirect, need a 401 status code
                        context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request, HttpStatusCode.Unauthorized);
                    }
                }
                else
                {
                    // Unable to parse authorization parameter
                    context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request, HttpStatusCode.Redirect, LoginPage);
                }
            },
            cancellationToken);
        }

        /// <summary>
        /// Attempt to authorize and provide current principal for specified <paramref name="sessionID"/>.
        /// </summary>
        /// <param name="sessionID">Session ID to user.</param>
        /// <param name="securityPrincipal">Principal of user with specified <paramref name="sessionID"/>, if found.</param>
        /// <returns><c>true</c> if principal was found for specified <paramref name="sessionID"/>; otherwise, <c>false</c>.</returns>
        public static bool TryGetPrincipal(Guid sessionID, out SecurityPrincipal securityPrincipal)
        {
            if (s_authorizationCache.TryGetValue(sessionID, out securityPrincipal))
                return true;

            securityPrincipal = null;
            return false;
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

            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings[SettingsCategory];

            systemSettings.Add("SessionToken", SessionHandler.DefaultSessionToken, "Defines the token used for identifying the session ID in cookie headers.");
            systemSettings.Add("LoginPage", DefaultLoginPage, "Defines the login page, relative path, used as a redirect location when authentication fails.");

            if (!m_sessionTokenAssigned)
                SessionToken = systemSettings["SessionToken"].ValueAs(SessionHandler.DefaultSessionToken);

            if (!m_loginPageAssigned)
                LoginPage = systemSettings["LoginPage"].ValueAs(DefaultLoginPage);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Guid, SecurityPrincipal> s_authorizationCache;

        // Static Constructor
        static AuthenticateControllerAttribute()
        {
            s_authorizationCache = new ConcurrentDictionary<Guid, SecurityPrincipal>();

            // Attach to razor view session expiration event so any cached authorizations can also be cleared
            Model.RazorView.SessionExpired += (sender, e) => ClearAuthorizationCache(e.Argument1);
        }

        // Static Methods

        /// <summary>
        /// Clears any cached authorizations for the specified <paramref name="sessionID"/>.
        /// </summary>
        /// <param name="sessionID">Identifier of session authorization to clear.</param>
        /// <returns><c>true</c> if session authorization was found and cleared; otherwise, <c>false</c>.</returns>
        public static bool ClearAuthorizationCache(Guid sessionID)
        {
            SecurityPrincipal securityPrincipal;
            return s_authorizationCache.TryRemove(sessionID, out securityPrincipal);
        }

        private static bool TryParseCredentials(string authorizationParameter, out string userName, out string password)
        {
            byte[] credentialBytes;

            userName = null;
            password = null;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return false;
            }

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // However, the current draft updated specification for HTTP 1.1 indicates this
            // encoding is infrequently used in practice and defines behavior only for ASCII.

            // Make a writable copy of the ASCII encoding to enable setting the decoder fall-back
            Encoding encoding = Encoding.ASCII.Clone() as Encoding;

            if ((object)encoding == null)
                return false;

            // Fail on invalid bytes rather than silently replacing and continuing
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

            string credentials;

            try
            {
                credentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(credentials))
                return false;

            int index = credentials.IndexOf(':');

            if (index == -1)
                return false;

            userName = credentials.Substring(0, index);
            password = credentials.Substring(index + 1);

            return true;
        }

        #endregion
    }
}
