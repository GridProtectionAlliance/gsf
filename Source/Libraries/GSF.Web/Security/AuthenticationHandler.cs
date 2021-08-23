//******************************************************************************************************
//  AuthenticationHandler.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/25/2017 - Stephen C. Wills
//       Generated original version of source code.
//  08/26/2017 - J. Ritchie Carroll
//       Updated handling for anonymous requests and added principal lookup function for a session ID
//  08/28/2017 - J. Ritchie Carroll
//       Improved NTLM pass-through authentication and unauthorized user handling
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GSF.Diagnostics;
using GSF.Reflection;
using GSF.Security;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

#pragma warning disable SG0015 // Validated - no hard-coded password present

namespace GSF.Web.Security
{
    /// <summary>
    /// Handles authentication using the configured <see cref="ISecurityProvider"/> implementation in the Owin pipeline.
    /// </summary>
    public class AuthenticationHandler : AuthenticationHandler<AuthenticationOptions>
    {
        #region [ Properties ]

        // Reads the authorization header value from the request
        private AuthenticationHeaderValue AuthorizationHeader
        {
            get
            {
                string[] authorization = Request.Headers["Authorization"]?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if ((object)authorization == null || authorization.Length < 2)
                    return null;

                return new AuthenticationHeaderValue(authorization[0], authorization[1]);
            }
        }

        // Gets a principal that represents an unauthenticated anonymous user
        private IPrincipal AnonymousPrincipal
        {
            get
            {
                IIdentity anonymousIdentity = new GenericIdentity("anonymous");
                return new GenericPrincipal(anonymousIdentity, new string[0]);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// The core authentication logic which must be provided by the handler. Will be invoked at most
        /// once per request. Do not call directly, call the wrapping Authenticate method instead.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic</returns>
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            SecurityPrincipal securityPrincipal;

            // Track original principal
            Request.Environment["OriginalPrincipal"] = Request.User;
            Request.Environment["AuthenticationOptions"] = Options.Readonly;

            // No authentication required for anonymous resources
            if (Options.IsAnonymousResource(Request.Path.Value))
                return Task.FromResult<AuthenticationTicket>(null);

            // Attempt to read the session ID from the HTTP cookies
            Guid sessionID = SessionHandler.GetSessionIDFromCookie(Request, Options.SessionToken);

            if (Request.Uri.LocalPath == Options.LogoutPage)
            {
                IIdentity logoutIdentity = new GenericIdentity(sessionID.ToString());
                string[] logoutRoles = { "logout" };
                Request.User = new GenericPrincipal(logoutIdentity, logoutRoles);

                return Task.FromResult<AuthenticationTicket>(null);
            }

            AuthenticationHeaderValue authorization = AuthorizationHeader;

            // Attempt to retrieve the user's credentials that were cached to the user's session
            if (TryGetPrincipal(sessionID, out securityPrincipal))
            {
                bool useCachedCredentials =
                    (object)Request.User == null ||
                    Request.User.Identity.Name.Equals(securityPrincipal.Identity.Name, StringComparison.OrdinalIgnoreCase) ||
                    authorization?.Scheme != "Basic";

                if (!useCachedCredentials)
                {
                    // Explicit login attempts as a different user
                    // cause credentials to be flushed from the session
                    ClearAuthorizationCache(sessionID);
                    securityPrincipal = null;
                }
            }

            if ((object)authorization == null && (object)securityPrincipal == null)
            {
                // Attempt to authenticate using cached credentials associated with the authentication token cookie
                string authenticationToken = SessionHandler.GetAuthenticationTokenFromCookie(Request, Options.AuthenticationToken);

                securityPrincipal = AuthenticateCachedCredentials(authenticationToken);

                // If authentication using cached credentials fails,
                // fall back on the other authentication methods
                if (securityPrincipal?.Identity.IsAuthenticated != true)
                    securityPrincipal = null;

                // Attempt to cache the security principal to the session
                if (sessionID != Guid.Empty && securityPrincipal?.Identity.IsAuthenticated == true)
                    CachePrincipal(sessionID, securityPrincipal);
            }

            if ((object)securityPrincipal == null)
            {
                NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(Request.QueryString.Value);
                // Pick the appropriate authentication logic based
                // on the authorization type in the HTTP headers
                // or in the URI Parameters if it is using OIDC.
                if (authorization?.Scheme == "Basic")
                    securityPrincipal = AuthenticateBasic(authorization.Parameter);
                // If the resources contains a code make an Attempt to Authorize via OIDC Auth server
                else if (Request.QueryString.HasValue && queryParameters.AllKeys.Contains("code"))
                    securityPrincipal = AuthenticateCode();
                else
                    securityPrincipal = AuthenticatePassthrough();

                // Attempt to cache the security principal to the session
                if (sessionID != Guid.Empty && securityPrincipal?.Identity.IsAuthenticated == true)
                    CachePrincipal(sessionID, securityPrincipal);
            }

            // Set the principal of the IOwinRequest so that it
            // can be propagated through the Owin pipeline
            Request.User = securityPrincipal ?? AnonymousPrincipal;

            return Task.FromResult<AuthenticationTicket>(null);
        }

        /// <summary>
        /// Called once by common code after initialization. If an authentication middle-ware
        /// responds directly to specifically known paths it must override this virtual,
        /// compare the request path to it's known paths, provide any response information
        /// as appropriate, and true to stop further processing.
        /// </summary>
        /// <returns>
        /// Returning false will cause the common code to call the next middle-ware in line.
        /// Returning true will cause the common code to begin the async completion journey
        /// without calling the rest of the middle-ware pipeline.
        /// </returns>
        public override async Task<bool> InvokeAsync()
        {
            // Use Cases:
            //
            //  (1) Access resource marked as anonymous - let pipeline continue
            //  (2) Access resource as authenticated user - let pipeline continue
            //  --- remaining use cases are unauthorized ---
            //  (2a) Access resource as result of a redirect from the OIDC Auth Server
            //  (3) Access resource marked for auth failure redirection - respond with 302 and abort pipeline
            //  (4) Access all other resources - respond with 401 and abort pipeline
            SecurityPrincipal securityPrincipal = Request.User as SecurityPrincipal;
            string urlPath = Request.Path.Value;

            if (!(Request.User is null) && Request.User.IsInRole("logout"))
            {
                string identityName = Request.User.Identity.Name;

                string bodyMessage = Guid.TryParse(identityName, out Guid sessionID) && LogOut(sessionID)
                    ? "Logout complete"
                    : "Unable to locate the user session";

                using TextWriter writer = new StreamWriter(Response.Body, Encoding.UTF8, 4096, true);
                await writer.WriteAsync(bodyMessage);
                Response.StatusCode = 200;
                return true; // Abort pipeline
            }


            // If request is for an anonymous resource or user is properly authenticated, allow
            // request to propagate through the Owin pipeline
            if (Options.IsAnonymousResource(urlPath) || securityPrincipal?.Identity.IsAuthenticated == true)
                return false; // Let pipeline continue

            // If the resources contains a code make an Attempt to Authorize via OIDC Auth server
            NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(Request.QueryString.Value);

            if (Request.QueryString.HasValue && queryParameters.AllKeys.Contains("code"))
                return false;

            


            // Abort pipeline with appropriate response
            if (Options.IsAuthFailureRedirectResource(urlPath) && !IsAjaxCall())
            {
                byte[] pathBytes;
                string base64Path, encodedPath, referrer = null;
                    
                if (Request.Headers.TryGetValue("Referer", out string[] values) && values.Length == 1)
                {
                    pathBytes = Encoding.UTF8.GetBytes(values[0]);
                    base64Path = Convert.ToBase64String(pathBytes);
                    encodedPath = WebUtility.UrlEncode(base64Path);
                    referrer = $"&referrer={encodedPath}";
                }

                string urlQueryString = Request.QueryString.HasValue ? "?" + Request.QueryString.Value : "";

                pathBytes = Encoding.UTF8.GetBytes(urlPath + urlQueryString);
                base64Path = Convert.ToBase64String(pathBytes);
                encodedPath = WebUtility.UrlEncode(base64Path);

                ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider("", autoRefresh: false);

                Response.Redirect(securityProvider.TranslateRedirect(Options.LoginPage, encodedPath, referrer));
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            // Add current identity to unauthorized response header
            string currentIdentity = securityPrincipal?.Identity.Name ?? "anonymous";
            object value;

            if (Request.Environment.TryGetValue("OriginalPrincipal", out value))
            {
                IPrincipal originalPrincpal = value as IPrincipal;

                if ((object)originalPrincpal != null && (object)originalPrincpal.Identity != null)
                    currentIdentity = AdjustedUserName(originalPrincpal.Identity.Name);
            }

            bool debuggable = Common.GetApplicationType() != ApplicationType.Web && AssemblyInfo.EntryAssembly.Debuggable;
            Response.Headers.Add("CurrentIdentity", new[] { currentIdentity });
            Response.ReasonPhrase = SecurityPrincipal.GetFailureReasonPhrase(securityPrincipal, AuthorizationHeader?.Scheme, debuggable);

            string failureReason = SecurityPrincipal.GetFailureReasonPhrase(securityPrincipal, AuthorizationHeader?.Scheme, true);
            Log.Publish(MessageLevel.Info, "AuthenticationFailure", $"Failed to authenticate {currentIdentity} for {Request.Path}: {failureReason}");
                
            return true; // Abort pipeline
        }

        private bool LogOut(Guid sessionID)
        {
            // Flush any cached information that has been saved for this user
            if (TryGetPrincipal(sessionID, out SecurityPrincipal securityPrincipal))
                SecurityProviderCache.Flush(securityPrincipal.Identity.Name);

            // Clear any cached session state for user (this also clears any cached authorizations)
            return SessionHandler.ClearSessionCache(sessionID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAjaxCall()
        {
            return Request.Headers.TryGetValue("X-Requested-With", out string[] values) && values.Any(value => value.Equals("XMLHttpRequest"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string AdjustedUserName(string username)
        {
            int index = username.IndexOf('\\');

            if (index < 1)
                return username;

            string[] parts = username.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                return username;

            return parts[0].Trim().Equals(Environment.MachineName) ? parts[1].Trim() : username;
        }

        // Applies authentication for requests where credentials are passed directly in the HTTP headers.
        private SecurityPrincipal AuthenticateCachedCredentials(string authenticationToken)
        {
            string username, password;

            if ((object)authenticationToken == null)
                return null;

            // Get the user's credentials from the credential cache
            if (!SessionHandler.TryGetCachedCredentials(authenticationToken, out username, out password))
                return null;

            // Create the security provider that will authenticate the user's credentials
            ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username, autoRefresh: false);
            securityProvider.Password = password;
            securityProvider.Authenticate();

            // Return the security principal that will be used for role-based authorization
            SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
            return new SecurityPrincipal(securityIdentity);
        }

        // Applies authentication for requests where credentials are passed directly in the HTTP headers.
        private SecurityPrincipal AuthenticateBasic(string credentials)
        {
            string username, password;

            // Get the user's credentials from the HTTP headers
            if (!TryParseCredentials(credentials, out username, out password))
                return null;

            // Create the security provider that will authenticate the user's credentials
            ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username, autoRefresh: false);
            securityProvider.Password = password;
            securityProvider.Authenticate();

            // Return the security principal that will be used for role-based authorization
            SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
            return new SecurityPrincipal(securityIdentity);
        }

        // Applies authentication for requests using Windows pass-through authentication.
        private SecurityPrincipal AuthenticatePassthrough()
        {
            string username = Request.User?.Identity.Name;

            
            if ((object)username == null)
                return null;

            // Get the principal used for verifying the user's pass-through authentication
            IPrincipal passthroughPrincipal = Request.User;

            // Create the security provider that will verify the user's pass-through authentication
            ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username, passthroughPrincipal, false);
            securityProvider.Authenticate();

            // Return the security principal that will be used for role-based authorization
            SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
            return new SecurityPrincipal(securityIdentity);
        }

        // Applies authentication for requests using OpenID Connect authentication.
        private SecurityPrincipal AuthenticateCode()
        {

            string username = System.Web.HttpUtility.ParseQueryString(Request.QueryString.Value).Get("code");

            if (String.IsNullOrEmpty(username))
                return null;

            // Create the security provider that will verify the user's pass-through authentication
            ISecurityProvider securityProvider = SecurityProviderCache.CreateProvider(username, null, false);
            securityProvider.Authenticate();

            // Return the security principal that will be used for role-based authorization
            SecurityIdentity securityIdentity = new SecurityIdentity(securityProvider);
            return new SecurityPrincipal(securityIdentity);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly LogPublisher Log = Logger.CreatePublisher(typeof(AppBuilderExtensions), MessageClass.Framework);
        private static readonly ConcurrentDictionary<Guid, SecurityPrincipal> s_authorizationCache;

        // Static Constructor
        static AuthenticationHandler()
        {
            s_authorizationCache = new ConcurrentDictionary<Guid, SecurityPrincipal>();

            // Attach to session expiration event so any cached authorizations can also be cleared
            SessionHandler.SessionExpired += (sender, e) => ClearAuthorizationCache(e.Argument1);
        }

        // Static Methods

        /// <summary>
        /// Attempt to get current security principal for specified <paramref name="sessionID"/>.
        /// </summary>
        /// <param name="sessionID">Session ID to user.</param>
        /// <param name="securityPrincipal">Principal of user with specified <paramref name="sessionID"/>, if found.</param>
        /// <returns><c>true</c> if principal was found for specified <paramref name="sessionID"/>; otherwise, <c>false</c>.</returns>
        public static bool TryGetPrincipal(Guid sessionID, out SecurityPrincipal securityPrincipal)
        {
            return s_authorizationCache.TryGetValue(sessionID, out securityPrincipal);
        }

        /// <summary>
        /// Clears any cached authorizations for the specified <paramref name="sessionID"/>.
        /// </summary>
        /// <param name="sessionID">Identifier of session authorization to clear.</param>
        /// <returns><c>true</c> if session authorization was found and cleared; otherwise, <c>false</c>.</returns>
        public static bool ClearAuthorizationCache(Guid sessionID)
        {
            SecurityPrincipal securityPrincipal;
            bool removed = s_authorizationCache.TryRemove(sessionID, out securityPrincipal);

            if (removed)
                SecurityProviderCache.DisableAutoRefresh(securityPrincipal.Identity.Provider);

            return removed;
        }

        private static void CachePrincipal(Guid sessionID, SecurityPrincipal principal)
        {
            if (s_authorizationCache.TryAdd(sessionID, principal))
                SecurityProviderCache.AutoRefresh(principal.Identity.Provider);
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
