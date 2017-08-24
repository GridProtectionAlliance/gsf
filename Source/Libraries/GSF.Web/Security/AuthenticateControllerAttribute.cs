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

        // Fields
        private string m_realm;

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
        /// Gets or sets settings category to use for loading data context for security info.
        /// </summary>
        public string SettingsCategory { get; set; } = "securityProvider";

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; set; } = SessionHandler.DefaultSessionToken;

        /// <summary>
        /// Gets or sets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage { get; set; } = "/Login.cshtml";

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

            Guid sessionID;
            bool hasSession = SessionHandler.TryGetSessionID(request, SessionToken, out sessionID);
            string authorizationParameter = null;

            // Try to retrieve any existing authorization
            if (hasSession)
                s_authorizationCache.TryGetValue(sessionID, out authorizationParameter);

            if (authorization.Scheme != "Basic" && string.IsNullOrEmpty(authorizationParameter))
            {
                // Check if user was already previously validated with pass-through authentication
                if (authorizationParameter == "")
                    return;

                // Assuming pass-through authentication - make sure user is defined in security provider, authenticated and has a role
                string identityName = context.Principal?.Identity?.Name;

                if (!string.IsNullOrEmpty(identityName))
                {
                    SecurityProviderCache.ValidateCurrentProvider(identityName);
                    SecurityIdentity identity = Thread.CurrentPrincipal.Identity as SecurityIdentity;

                    if ((object)identity != null)
                    {
                        UserData userData = identity.Provider?.UserData;

                        if ((object)userData != null && userData.IsAuthenticated && userData.Roles.Count > 0)
                        {
                            // User is valid, do not set principal, which would indicate success, nor ErrorResult,
                            // which would indicate an error
                            authorizationParameter = "";

                            if (hasSession)
                                s_authorizationCache[sessionID] = authorizationParameter;

                            return;
                        }
                    }
                }

                // Not a valid user for current security provider, provide an obscure error message
                context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request, HttpStatusCode.Redirect, LoginPage);
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(authorizationParameter))
                    authorizationParameter = authorization.Parameter;
            }

            if (string.IsNullOrEmpty(authorizationParameter))
            {
                // No authorization credentials were provided, set ErrorResult
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request, HttpStatusCode.Redirect, LoginPage);
                return;
            }

            await Task.Run(() =>
            {
                string userName, password;

                if (TryParseCredentials(authorizationParameter, out userName, out password))
                {
                    // Setup the security principal
                    SecurityProviderCache.ValidateCurrentProvider(userName);
                    IPrincipal principal = Thread.CurrentPrincipal;

                    // Authenticate user, if not already authenticated
                    if (principal.Identity.IsAuthenticated || SecurityProviderCache.CurrentProvider.Authenticate(password))
                    {
                        context.Principal = principal;

                        if (hasSession)
                            s_authorizationCache[sessionID] = authorizationParameter;

                        ThreadPool.QueueUserWorkItem(start => AuthorizationCache.CacheAuthorization(userName, SettingsCategory));
                    }
                    else
                    {
                        // Authentication was attempted but failed, set ErrorResult - don't redirect, need a 401 status code
                        context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request);                        
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
        /// <param name="principal">Principal of user with specified <paramref name="sessionID"/>, if found.</param>
        /// <returns><c>true</c> if principal was found for specified <paramref name="sessionID"/>; otherwise, <c>false</c>.</returns>
        public static bool TryGetPrincipal(Guid sessionID, out IPrincipal principal)
        {
            string authorizationParameter = null;

            if (s_authorizationCache.TryGetValue(sessionID, out authorizationParameter))
            {
                string userName, password;

                if (TryParseCredentials(authorizationParameter, out userName, out password))
                {
                    // Setup the security principal
                    SecurityProviderCache.ValidateCurrentProvider(userName);
                    principal = Thread.CurrentPrincipal;

                    // Authenticate user, if not already authenticated
                    if (principal.Identity.IsAuthenticated || SecurityProviderCache.CurrentProvider.Authenticate(password))
                        return true;
                }
            }

            principal = null;
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

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<Guid, string> s_authorizationCache;

        // Static Constructor
        static AuthenticateControllerAttribute()
        {
            s_authorizationCache = new ConcurrentDictionary<Guid, string>();

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
            string authorizationParameter;
            return s_authorizationCache.TryRemove(sessionID, out authorizationParameter);
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
