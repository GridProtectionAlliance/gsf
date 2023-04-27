//******************************************************************************************************
//  SessionHandler.cs - Gbtc
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
//  08/24/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using GSF.Collections;
using GSF.Configuration;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Security;
using GSF.Web.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using RazorEngine.Templating;
using Random = GSF.Security.Cryptography.Random;
using Timer = System.Timers.Timer;

#pragma warning disable SG0015 // Validated - no hard-coded password present

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents an HTTP messaging handler that can inject a session ID that can be used for security and user state.
    /// </summary>
    public class SessionHandler : DelegatingHandler
    {
        #region [ Members ]

        // Nested Types
        private class Session
        {
            public readonly DynamicViewBag State = new();
            public DateTime LastAccess;
        }

        private class Credential
        {
            #region [ Constructors ]

            public Credential()
            {
            }

            // Note: Usage is dynamically called via FileBackedDictionary
            // ReSharper disable once UnusedMember.Local
            [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
            public Credential(Stream stream)
            {
                try
                {
                    using BinaryReader reader = new(stream, Encoding.Unicode, true);
                    int length = reader.ReadInt32();
                    byte[] protectedCredentials = reader.ReadBytes(length);
                    byte[] credentials = ProtectedData.Unprotect(protectedCredentials, null, DataProtectionScope.CurrentUser);

                    using MemoryStream memStream = new(credentials);
                    using BinaryReader memReader = new(memStream, Encoding.Unicode, true);

                    Validator = memReader.ReadString();
                    Username = memReader.ReadString();
                    Password = memReader.ReadString();
                    Expiration = new DateTime(memReader.ReadInt64());
                }
                catch (Exception ex)
                {
                    // Invalidate cache entry for decrypt failures (service could be running as different user)
                    Validator = Username = Password = null;
                    Expiration = DateTime.MinValue;
                    Logger.SwallowException(ex);
                }
            }

            #endregion

            #region [ Properties ]

            public string Validator { get; set; }

            public string Username { get; set; }

            public string Password { get; set; }

            public DateTime Expiration { get; set; }

            #endregion

            #region [ Methods ]

            // Note: Usage is dynamically called via FileBackedDictionary
            // ReSharper disable once UnusedMember.Local
            [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
            public void WriteTo(Stream stream)
            {
                using MemoryStream memStream = new();
                using BinaryWriter memWriter = new(memStream, Encoding.Unicode, true);
                
                memWriter.Write(Validator);
                memWriter.Write(Username);
                memWriter.Write(Password);
                memWriter.Write(Expiration.Ticks);

                byte[] credentials = memStream.ToArray();
                byte[] protectedCredentials = ProtectedData.Protect(credentials, null, DataProtectionScope.CurrentUser);

                using BinaryWriter writer = new(stream, Encoding.Unicode, true);
                writer.Write(protectedCredentials.Length);
                writer.Write(protectedCredentials);
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Default value for <see cref="SessionTimeout"/>.
        /// </summary>
        public const double DefaultSessionTimeout = 20.0D;

        /// <summary>
        /// Default value for <see cref="SessionMonitorInterval"/>.
        /// </summary>
        public const double DefaultSessionMonitorInterval = 60000.0D;

        /// <summary>
        /// Default value for a cached session expiration, in days.
        /// </summary>
        public const double DefaultSessionExpirationDays = 30.0D;

        /// <summary>
        /// Default value for <see cref="AuthenticationToken"/>;
        /// </summary>
        public const string DefaultAuthenticationToken = "x-gsf-auth";

        /// <summary>
        /// Default value for <see cref="SessionToken"/>;
        /// </summary>
        public const string DefaultSessionToken = "x-gsf-session";

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SessionHandler"/>.
        /// </summary>
        /// <param name="authenticationToken">Token used for identifying the authentication token in cookie headers.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in cookie headers.</param>
        public SessionHandler(string authenticationToken = DefaultAuthenticationToken, string sessionToken = DefaultSessionToken)
        {
            AuthenticationToken = authenticationToken;
            SessionToken = sessionToken;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the token used for identifying the authentication token in cookie headers.
        /// </summary>
        public string AuthenticationToken { get; }

        /// <summary>
        /// Gets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Modifies the HTTP request message pipeline to include a session identifier.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get existing session data from the request, if any
            CookieHeaderValue cookie = request.Headers.GetCookies(SessionToken).FirstOrDefault();
            string sessionCookieValue = cookie?[SessionToken].Value;

            // If session ID format is invalid, create a new one
            if (!Guid.TryParse(sessionCookieValue, out Guid sessionID))
                sessionID = Guid.NewGuid();

            sessionCookieValue = sessionID.ToString();

            // Save session ID (as Guid) in the request properties
            request.Properties[SessionToken] = sessionID;

            // Continue processing the HTTP request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Store session ID in response message cookie
            UrlHelper urlHelper = new();
            string basePath = urlHelper.Content("~");
            SetCookie(request, response, SessionToken, sessionCookieValue, basePath.ToNonNullNorEmptyString("/"));

            // Get authentication options associated with this request
            ReadonlyAuthenticationOptions options = request.GetAuthenticationOptions();

            // If requesting the AuthTest page using BASIC authentication, reissue the client's authentication token
            string authTestPage = options.GetFullAuthTestPath(basePath);

            if (request.RequestUri.LocalPath == authTestPage)
            {
                if (request.Method != HttpMethod.Post)
                    throw new SecurityException("Authorization test only operates with validated HTTP post operation");

                SecurityPrincipal securityPrincipal = request.GetRequestContext().Principal as SecurityPrincipal;
                SecurityIdentity securityIdentity = securityPrincipal?.Identity;
                ISecurityProvider securityProvider = securityIdentity?.Provider;

                string username = securityIdentity?.Name;
                string password = securityProvider?.Password;

                if (!string.IsNullOrEmpty(username) && (!string.IsNullOrEmpty(password) || securityIdentity.Provider.UserData.IsAzureAD))
                {
                    string authenticationToken = IssueAuthenticationToken(username, password);

                    InvalidateAuthenticationToken(request);

                    SetCookie(request, response, AuthenticationToken, authenticationToken, request.RequestUri.LocalPath, TimeSpan.FromDays(s_sessionExpirationDays));
                }

                // AuthTest page should always have a valid request verification token
                request.ValidateRequestVerificationToken(options);
            }

            return response;
        }

        private string IssueAuthenticationToken(string username, string password)
        {
            byte[] buffer = new byte[9];

            // Generate the selector for the token
            Random.GetBytes(buffer);
            string selector = Convert.ToBase64String(buffer);

            // Generate the validator for the token
            Random.GetBytes(buffer);
            string validator = Convert.ToBase64String(buffer);

            if (s_sessionCredentialCache is not null)
            {
                // Open the credential cache
                lock (s_sessionCredentialCacheLock)
                {
                    using FileBackedDictionary<string, Credential> credentialCache = new(s_sessionCredentialCache);

                    // Clean out expired credentials before issuing a new one
                    DateTime now = DateTime.UtcNow;

                    List<string> expiredSelectors = credentialCache
                        .Where(kvp => now >= kvp.Value.Expiration)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    if (expiredSelectors.Count > 0)
                    {
                        foreach (string expiredSelector in expiredSelectors)
                            credentialCache.Remove(expiredSelector);

                        credentialCache.Compact();
                    }

                    // Enter the new token into the credential cache
                    credentialCache[selector] = new Credential
                    {
                        Validator = validator,
                        Username = username,
                        Password = password,
                        Expiration = DateTime.UtcNow.AddDays(s_sessionExpirationDays)
                    };
                }
            }

            return $"{selector}:{validator}";
        }

        private void InvalidateAuthenticationToken(HttpRequestMessage request)
        {
            // Get the authentication token provided by the client in the request message
            string authenticationToken = GetAuthenticationTokenFromCookie(request, AuthenticationToken);

            if (authenticationToken is null)
                return;

            string selector = authenticationToken.Split(':')[0];

            if (s_sessionCredentialCache is null)
                return;

            // Open the credential cache
            lock (s_sessionCredentialCacheLock)
            {
                using FileBackedDictionary<string, Credential> credentialCache = new(s_sessionCredentialCache);
                credentialCache.Remove(selector);
            }
        }

        #endregion

        #region [ Static ]

        // Static Events

        /// <summary>
        /// Raised when a client session is being expired.
        /// </summary>
        public static event EventHandler<EventArgs<Guid, DynamicViewBag>> SessionExpired;

        /// <summary>
        /// Occurs when a static method encounters an exception.
        /// </summary>
        public static event EventHandler<EventArgs<Exception>> ProcessException;

        // Static Fields
        private static readonly Timer s_sessionCacheMonitor;
        private static readonly ConcurrentDictionary<Guid, Session> s_sessionCache;
        private static readonly string s_sessionCredentialCache;
        private static readonly object s_sessionCredentialCacheLock;
        private static readonly double s_sessionExpirationDays;

        // Static Constructor
        static SessionHandler()
        {
            CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings["systemSettings"];

            settings.Add("SessionTimeout", DefaultSessionTimeout, "The timeout, in minutes, for which inactive client sessions will be expired and removed from the cache.");
            settings.Add("SessionMonitorInterval", DefaultSessionMonitorInterval, "The interval, in milliseconds, over which the client session cache will be evaluated for expired sessions.");
            settings.Add("ConfigurationCachePath", $"{FilePath.GetAbsolutePath("")}{Path.DirectorySeparatorChar}ConfigurationCache{Path.DirectorySeparatorChar}", "Defines the path used to cache serialized phasor protocol configurations");
            settings.Add("SessionCredentialCacheEnabled", true, "Defines flag that determines if session credentials cache is enabled");
            settings.Add("SessionCredentialCache", Path.Combine(settings["ConfigurationCachePath"].Value, "CredentialCache.bin"), "Path and file name of session credentials cache");
            settings.Add("SessionExpirationDays", DefaultSessionExpirationDays, "Default number of days for session expiration");

            SessionTimeout = settings["SessionTimeout"].ValueAs(DefaultSessionTimeout);
            SessionMonitorInterval = settings["SessionMonitorInterval"].ValueAs(DefaultSessionMonitorInterval);

            s_sessionCacheMonitor = new Timer(SessionMonitorInterval);
            s_sessionCacheMonitor.Elapsed += s_sessionCacheMonitor_Elapsed;
            s_sessionCacheMonitor.Enabled = false;
            s_sessionCache = new ConcurrentDictionary<Guid, Session>();

            if (settings["SessionCredentialCacheEnabled"].ValueAs(true))
            {
                s_sessionCredentialCache = settings["SessionCredentialCache"].Value;
                s_sessionCredentialCacheLock = new object();
            }

            s_sessionExpirationDays = settings["SessionExpirationDays"].ValueAs(DefaultSessionExpirationDays);
        }

        // Static Properties

        /// <summary>
        /// Gets timeout, in minutes, for which inactive client sessions will be expired and removed from the cache.
        /// </summary>
        public static double SessionTimeout { get; }

        /// <summary>
        /// Gets interval, in milliseconds, over which the client session cache will be evaluated for expired sessions.
        /// </summary>
        public static double SessionMonitorInterval { get; }

        // Static Methods

        /// <summary>
        /// Clears any cached session for the specified <paramref name="sessionID"/>.
        /// </summary>
        /// <param name="sessionID">Identifier of session to clear.</param>
        /// <returns><c>true</c> if session was found and cleared; otherwise, <c>false</c>.</returns>
        public static bool ClearSessionCache(Guid sessionID)
        {
            if (!s_sessionCache.TryRemove(sessionID, out Session session))
                return false;

            OnSessionExpired(sessionID, session.State);
            return true;
        }

        /// <summary>
        /// Attempts to use the authentication token to retrieve the user's credentials from the credential cache.
        /// </summary>
        /// <param name="authenticationToken">The token used to retrieve the user's credentials.</param>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">THe user's password.</param>
        /// <returns>True if the user's credentials were successfully retrieved; false otherwise.</returns>
        public static bool TryGetCachedCredentials(string authenticationToken, out string username, out string password)
        {
            try
            {
                if (s_sessionCredentialCache is not null)
                {
                    // Parse the selector and validator from the authentication token
                    string[] authenticationTokenParts = authenticationToken.Split(':');

                    if (authenticationTokenParts.Length != 2)
                    {
                        username = null;
                        password = null;
                        return false;
                    }

                    string selector = authenticationTokenParts[0];
                    string validator = authenticationTokenParts[1];

                    // Read the credential cache to retrieve the user's
                    // credentials that were mapped to this authentication token
                    lock (s_sessionCredentialCacheLock)
                    {
                        using FileBackedDictionary<string, Credential> credentialCache = new(s_sessionCredentialCache);

                        if (credentialCache.TryGetValue(selector, out Credential credential) && validator == credential.Validator && DateTime.UtcNow < credential.Expiration)
                        {
                            username = credential.Username;
                            password = credential.Password;
                            return username is not null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }

            username = null;
            password = null;
            return false;
        }

        /// <summary>
        /// Tries to get the session ID Guid as defined in the request properties dictionary.
        /// </summary>
        /// <param name="request">The target HTTP request message.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in properties dictionary.</param>
        /// <param name="sessionID">Validated session ID to return.</param>
        /// <returns><c>true</c> if session ID was successfully accessed; otherwise, <c>false</c>.</returns>
        public static bool TryGetSessionID(HttpRequestMessage request, string sessionToken, out Guid sessionID)
        {
            if (request.Properties.TryGetValue(sessionToken ?? DefaultSessionToken, out object result) && result is Guid guid)
            {
                sessionID = guid;
                return true;
            }

            sessionID = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Tries to get the session state as defined in the request properties dictionary.
        /// </summary>
        /// <param name="request">The target HTTP request message.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in properties dictionary.</param>
        /// <param name="sessionState">Session state to return.</param>
        /// <returns><c>true</c> if session state was successfully accessed; otherwise, <c>false</c>.</returns>
        public static bool TryGetSessionState(HttpRequestMessage request, string sessionToken, out DynamicViewBag sessionState)
        {
            if (TryGetSessionID(request, sessionToken, out Guid sessionID) && s_sessionCache.TryGetValue(sessionID, out Session session))
            {
                sessionState = session.State;
                return true;
            }

            sessionState = null;
            return false;
        }

        /// <summary>
        /// Gets the authentication token as defined in the request cookie header values.
        /// </summary>
        /// <param name="request">The target HTTP request message.</param>
        /// <param name="authenticationToken">Token used for identifying the authentication token in cookie headers.</param>
        /// <returns>Authentication token string, if defined; otherwise, <c>null</c>.</returns>
        public static string GetAuthenticationTokenFromCookie(HttpRequestMessage request, string authenticationToken)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(authenticationToken).FirstOrDefault();
            return cookie?[authenticationToken ?? DefaultAuthenticationToken].Value;
        }

        /// <summary>
        /// Gets the authentication token as defined in the request cookie header values.
        /// </summary>
        /// <param name="request">The target Owin request.</param>
        /// <param name="authenticationToken">Token used for identifying the authentication token in cookie headers.</param>
        /// <returns>Authentication token string, if defined; otherwise, <c>null</c>.</returns>
        public static string GetAuthenticationTokenFromCookie(IOwinRequest request, string authenticationToken) => 
            request?.Cookies?[authenticationToken ?? DefaultAuthenticationToken];

        /// <summary>
        /// Gets the session ID Guid as defined in the request cookie header values.
        /// </summary>
        /// <param name="request">The target HTTP request message.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in cookie headers.</param>
        /// <returns>Session ID string, if defined; otherwise, <c>null</c>.</returns>
        public static Guid GetSessionIDFromCookie(HttpRequestMessage request, string sessionToken)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(sessionToken).FirstOrDefault();
            string value = cookie?[sessionToken ?? DefaultSessionToken].Value;

            if (Guid.TryParse(value, out Guid sessionID))
                UpdateSession(sessionID);

            return sessionID;
        }

        /// <summary>
        /// Gets the session ID Guid as defined in the Owin cookie header values.
        /// </summary>
        /// <param name="request">The target Owin request.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in cookie headers.</param>
        /// <returns>Session ID string, if defined; otherwise, <c>null</c>.</returns>
        public static Guid GetSessionIDFromCookie(IOwinRequest request, string sessionToken)
        {
            string value = request?.Cookies?[sessionToken ?? DefaultSessionToken];

            if (Guid.TryParse(value, out Guid sessionID))
                UpdateSession(sessionID);

            return sessionID;
        }

        /// <summary>
        /// Gets the session ID Guid as defined in the SignalR cookie header values.
        /// </summary>
        /// <param name="request">The target SignalR request.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in cookie headers.</param>
        /// <returns>Session ID string, if defined; otherwise, <c>null</c>.</returns>
        public static Guid GetSessionIDFromCookie(IRequest request, string sessionToken)
        {
            string value = request?.Cookies?[sessionToken ?? DefaultSessionToken].Value;

            if (Guid.TryParse(value, out Guid sessionID))
                UpdateSession(sessionID);

            return sessionID;
        }

        /// <summary>
        /// Gets the session state as defined in the request cookie header values.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="response">Response message.</param>
        /// <param name="key">Cookie key.</param>
        /// <param name="value">Cookie value.</param>
        /// <param name="path">Cookie path.</param>
        /// <param name="maxAge">Cookie max age.</param>
        public static void SetCookie(HttpRequestMessage request, HttpResponseMessage response, string key, string value, string path = "/", TimeSpan? maxAge = null)
        {
            if (HostingEnvironment.IsHosted)
            {
                IOwinContext owinContext = request.GetOwinContext();
                HttpContextBase httpContext = owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                
                httpContext.Response.AppendCookie(new HttpCookie(key, value)
                { 
                    Path = "/",
                    Expires = maxAge is null ? DateTime.MinValue : DateTime.UtcNow.Add(maxAge.Value)
                });
            }
            else
            {
                response.Headers.AddCookies(new[] { new CookieHeaderValue(key, value)
                {
                    Path = path,
                    MaxAge = maxAge
                }});
            }
        }

        private static void UpdateSession(Guid sessionID)
        {
            // Get cached session for this request, creating it if necessary
            Session session = s_sessionCache.GetOrAdd(sessionID, _ => new Session());

            // Update the last access time for the session state - as long as user is making requests, session will persist
            session.LastAccess = DateTime.UtcNow;
            s_sessionCacheMonitor.Enabled = true;
        }

        private static void OnSessionExpired(Guid sessionID, DynamicViewBag sessionState) => 
            SessionExpired?.Invoke(typeof(SessionHandler), new EventArgs<Guid, DynamicViewBag>(sessionID, sessionState));

        private static void OnProcessException(Exception ex) => 
            ProcessException?.Invoke(typeof(SessionHandler), new EventArgs<Exception>(ex));

        private static void s_sessionCacheMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check for expired client sessions
            foreach (KeyValuePair<Guid, Session> clientSession in s_sessionCache)
            {
                if ((DateTime.UtcNow - clientSession.Value.LastAccess).TotalMinutes > SessionTimeout)
                    ClearSessionCache(clientSession.Key);
            }

            s_sessionCacheMonitor.Enabled = s_sessionCache.Count > 0;
        }

        #endregion
    }

    /// <summary>
    /// Represents <see cref="HttpConfiguration"/> extension functions for GSF authentication middle-ware.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Adds session management to the HTTP configuration message handlers to enable GSF role-base security.
        /// </summary>
        /// <param name="httpConfig">Target <see cref="HttpConfiguration"/> instance.</param>
        /// <param name="options">Authentication options.</param>
        public static void EnableSessions(this HttpConfiguration httpConfig, AuthenticationOptions options)
        {
            if (!httpConfig.MessageHandlers.Any(handler => handler is SessionHandler))
                httpConfig.MessageHandlers.Add(new SessionHandler(options.AuthenticationToken, options.SessionToken));

            if (!httpConfig.Filters.Any(filter => filter.Instance is ValidateRequestVerificationTokenAttribute))
                httpConfig.Filters.Add(new ValidateRequestVerificationTokenAttribute(true));
        }

        /// <summary>
        /// Retrieves a read-only copy of the authentication options from the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The authentication options.</returns>
        public static ReadonlyAuthenticationOptions GetAuthenticationOptions(this HttpRequestMessage request)
        {
            IOwinContext context = request.GetOwinContext();

            if (context is not null && context.Environment.TryGetValue("AuthenticationOptions", out object value))
                return value as ReadonlyAuthenticationOptions;

            return null;
        }

        /// <summary>
        /// Retrieves the OWIN context from the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns><see cref="IOwinContext"/> if found; otherwise, <c>null</c>.</returns>
        public static IOwinContext GetOwinContext(this HttpRequestMessage request)
        {
            if (!request.Properties.TryGetValue("MS_OwinContext", out object value))
                return null;

            return value as IOwinContext;
        }

        /// <summary>
        /// Generates an anti-forgery token that can be manually added to an HTTP request header,
        /// e.g., from within an AJAX request.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <returns>Anti-forgery token to be added as an HTTP header value.</returns>
        public static string GenerateRequestVerficationHeaderToken(this HttpRequestMessage request)
        {
            AntiForgery.GetTokens(request, null, out string cookieToken, out string formToken);
            return $"{cookieToken}:{formToken}";
        }

        /// <summary>
        /// Adds a new anti-forgery request verification token to the headers of the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        public static void AddRequestVerificationHeaderToken(this HttpRequestMessage request) =>
            request.Headers.Add(
                request.GetAuthenticationOptions().RequestVerificationToken, 
                request.GenerateRequestVerficationHeaderToken());

        /// <summary>
        /// Validates that the anti-forgery request verification token value comes from the user who submitted the data.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="formValidation">Flag that determines if form validation should be used.</param>
        public static void ValidateRequestVerificationToken(this HttpRequestMessage request, bool formValidation = false) => 
            ValidateRequestVerificationToken(request, request.GetAuthenticationOptions(), formValidation);

        /// <summary>
        /// Validates that the anti-forgery request verification token value comes from the user who submitted the data.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="options">Read-only authentication options for this <paramref name="request"/>.</param>
        /// <param name="formValidation">Flag that determines if form validation should be used.</param>
        public static void ValidateRequestVerificationToken(this HttpRequestMessage request, ReadonlyAuthenticationOptions options, bool formValidation = false)
        {
            bool tryGetTokens(string name, out Tuple<string, string> values)
            {
                if (request.Headers.TryGetValues(name, out IEnumerable<string> tokenHeaders))
                {
                    string[] tokens = tokenHeaders.First().Split(':');

                    switch (tokens.Length)
                    {
                        case 1:
                            values = new Tuple<string, string>(name, tokens[0].Trim());
                            return true;
                        case 2:
                            values = new Tuple<string, string>(tokens[0].Trim(), tokens[1].Trim());
                            return true;
                    }
                }

                values = default;
                return false;
            }

            // Check for anti-forgery operation with AJAX requests, in these cases, do not perform form validation
            if (tryGetTokens(options.AjaxRequestVerificationToken, out Tuple<string, string> tokenValues) && tokenValues.Item2.ParseBoolean())
                formValidation = false;

            if (formValidation)
            {
                // Form validation
                AntiForgery.Validate(request);
            }
            else
            {
                // Header validation
                string cookieToken = "";
                string formToken = "";

                if (tryGetTokens(options.RequestVerificationToken, out tokenValues))
                {
                    cookieToken = tokenValues.Item1;
                    formToken = tokenValues.Item2;
                }

                AntiForgery.Validate(request, cookieToken, formToken);
            }
        }
    }
}
