//******************************************************************************************************
//  AuthenticationOptions.cs - Gbtc
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
//  08/25/2017 - Stephen C. sWills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using GSF.Web.Shared;

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents options for authentication using <see cref="AuthenticationHandler"/>.
    /// </summary>
    public sealed class AuthenticationOptions : Microsoft.Owin.Security.AuthenticationOptions
    {
        #region [ Members ]

        // Constants

        private const string AjaxTokenSuffix = "-Ajax";

        /// <summary>
        /// Default value for <see cref="RequestVerificationToken"/>.
        /// </summary>
        public const string DefaultRequestVerificationToken = "X-GSF-Verify";

        /// <summary>
        /// Default value for <see cref="AjaxRequestVerificationToken"/>.
        /// </summary>
        public const string DefaultAjaxRequestVerificationToken = DefaultRequestVerificationToken + AjaxTokenSuffix;

        /// <summary>
        /// Default value for <see cref="AuthFailureRedirectResourceExpression"/>.
        /// </summary>
        public const string DefaultAuthFailureRedirectResourceExpression = @"^/$|^/.+\.cshtml$|^/.+\.vbhtml$";

        /// <summary>
        /// Default value for <see cref="AnonymousResourceExpression"/>.
        /// </summary>
        public const string DefaultAnonymousResourceExpression = "^/@|^/favicon.ico$";

        /// <summary>
        /// Default value for <see cref="AlternateSecurityProviderResourceExpression"/>.
        /// </summary>
        public const string DefaultAlternateSecurityProviderResourceExpression = "";

        /// <summary>
        /// Default value for <see cref="LoginPage"/>.
        /// </summary>
        public const string DefaultLoginPage = Resources.DefaultRoot + "/Security/Views/Login.cshtml";

        /// <summary>
        /// Default value for <see cref="LogoutPage"/>.
        /// </summary>
        public const string DefaultLogoutPage = "/Security/logout";

        /// <summary>
        /// Default value for <see cref="AuthTestPage"/>.
        /// </summary>
        public const string DefaultAuthTestPage = "/AuthTest";

        /// <summary>
        /// Default value for <see cref="AuthenticationSchemes"/>.
        /// </summary>
        public const AuthenticationSchemes DefaultAuthenticationSchemes = AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;

        /// <summary>
        /// Default value for <see cref="ClearCredentialsParameter"/>.
        /// </summary>
        public const string DefaultClearCredentialsParameter = "clearcredentials";

        // Fields
        private readonly ConcurrentDictionary<string, bool> m_authFailureRedirectResourceCache;
        private readonly ConcurrentDictionary<string, bool> m_anonymousResourceCache;
        private readonly ConcurrentDictionary<string, bool> m_alternateSecurityProviderResourceCache;
        private string m_authFailureRedirectResourceExpression;
        private string m_anonymousResourceExpression;
        private string m_alternateSecurityProviderResourceExpression;
        private Regex m_authFailureRedirectResources;
        private Regex m_alternateSecurityProviderResources;
        private Regex m_anonymousResources;
        private string m_realm;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions() : base(SessionHandler.DefaultAuthenticationToken)
        {
            m_authFailureRedirectResourceCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            m_anonymousResourceCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            m_alternateSecurityProviderResourceCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the expression that will match paths for the resources on the web server
        /// that should redirect to the <see cref="LoginPage"/> when authentication fails.
        /// </summary>
        public string AuthFailureRedirectResourceExpression
        {
            get => m_authFailureRedirectResourceExpression;
            set
            {
                m_authFailureRedirectResourceExpression = value;
                m_authFailureRedirectResources = new Regex(m_authFailureRedirectResourceExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets the expression that will match paths for the resources on the web server
        /// that should user the Alternate SecurityProvider.
        /// </summary>
        public string AlternateSecurityProviderResourceExpression
        {
            get => m_alternateSecurityProviderResourceExpression;
            set
            {
                m_alternateSecurityProviderResourceExpression = value;
                m_alternateSecurityProviderResources = new Regex(m_alternateSecurityProviderResourceExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets the expression that will match paths for the resources on the web server
        /// that can be provided without checking credentials.
        /// </summary>
        public string AnonymousResourceExpression
        {
            get => m_anonymousResourceExpression;
            set
            {
                m_anonymousResourceExpression = value;
                m_anonymousResources = new Regex(m_anonymousResourceExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets the token used for identifying the authentication token in cookie headers.
        /// </summary>
        public string AuthenticationToken { get; set; } = SessionHandler.DefaultAuthenticationToken;

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; set; } = SessionHandler.DefaultSessionToken;

        /// <summary>
        /// Gets or sets the token used for anti-forgery verification in HTTP request headers.
        /// </summary>
        public string RequestVerificationToken { get; set; } = DefaultRequestVerificationToken;

        /// <summary>
        /// Gets or sets token to specify when using AJAX for request verification tokens.
        /// </summary>
        public string AjaxRequestVerificationToken => RequestVerificationToken + AjaxTokenSuffix;

        /// <summary>
        /// Gets or sets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage { get; set; } = DefaultLoginPage;

        /// <summary>
        /// Gets or sets the path for the logout page.
        /// </summary>
        public string LogoutPage { get; set; } = DefaultLogoutPage;

        /// <summary>
        /// Gets or sets the page name used to test user authorization.
        /// </summary>
        public string AuthTestPage { get; set; } = DefaultAuthTestPage;

        /// <summary>
        /// Gets or sets the authentication schemes to use when testing authentication with the <see cref="AuthTestPage"/>.
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes { get; set; } = DefaultAuthenticationSchemes;

        /// <summary>
        /// Gets or sets the parameter name for the <see cref="AuthTestPage"/> that forces it to use Basic authentication
        /// so that any cached browser credentials can be cleared.
        /// </summary>
        public string ClearCredentialsParameter { get; set; } = DefaultClearCredentialsParameter;

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
            get => m_realm;
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
        /// Gets or sets any custom header to be displayed on the <see cref="LoginPage"/>.
        /// </summary>
        public string LoginHeader { get; set; }

        /// <summary>
        /// Gets an immutable version of the authentication options.
        /// </summary>
        public ReadonlyAuthenticationOptions Readonly => new(this);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the given resource is an authentication failure redirect resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an authentication failure redirect resource.</param>
        /// <returns><c>true</c> if path is an authentication failure redirect resource; otherwise, <c>false</c>.</returns>
        public bool IsAuthFailureRedirectResource(string urlPath)
        {
            if (m_authFailureRedirectResourceExpression is null)
                AuthFailureRedirectResourceExpression = DefaultAuthFailureRedirectResourceExpression;

            return m_authFailureRedirectResourceCache.GetOrAdd(urlPath, m_authFailureRedirectResources.IsMatch);
        }

        /// <summary>
        /// Determines whether the given resource is a alternate securtityProvider resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an alternate securtityProvider resource.</param>
        /// <returns><c>true</c> if path is an alternate securtityProvider resource; otherwise, <c>false</c>.</returns>
        public bool IsAlternateSecurityProviderResource(string urlPath)
        {
            if (m_alternateSecurityProviderResources is null)
                AlternateSecurityProviderResourceExpression = DefaultAlternateSecurityProviderResourceExpression ;

            return string.IsNullOrEmpty(m_alternateSecurityProviderResourceExpression)
                ? m_alternateSecurityProviderResourceCache.GetOrAdd(urlPath, false)
                : m_alternateSecurityProviderResourceCache.GetOrAdd(urlPath, m_alternateSecurityProviderResources!.IsMatch);
        }

        /// <summary>
        /// Determines whether the given resource is an anonymous resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAnonymousResource(string urlPath)
        {
            if (m_anonymousResourceExpression is null)
                AnonymousResourceExpression = DefaultAnonymousResourceExpression;
                
            return m_anonymousResourceCache.GetOrAdd(urlPath, path =>
            {
                string resource = path;

                // If path is an embedded resource, convert it to a properly formatted type name identifier
                if (resource.StartsWith("/@"))
                    resource = resource.Substring(2).Replace('/', '.');
                else if (resource.StartsWith("@"))
                    resource = resource.Substring(1).Replace('/', '.');

                if (s_resourceRequiresAuthentication.TryGetValue(resource, out bool state))
                    return !state;

                // Always match against unmodified original path
                return m_anonymousResources.IsMatch(path);
            });
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, bool> s_resourceRequiresAuthentication;

        // Static Constructor
        static AuthenticationOptions()
        {
            s_resourceRequiresAuthentication = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Sets the resource to the required authentication state.
        /// </summary>
        /// <param name="resource">Resource name.</param>
        /// <param name="required">Set to <c>true</c> to require authenticated access; otherwise, <c>false</c> to require anonymous access.</param>
        /// <remarks>
        /// This state overrides any authentication state that would otherwise be derived from <see cref="AnonymousResourceExpression"/>.
        /// </remarks>
        public static void ResourceRequiresAuthentication(string resource, bool required)
        {
            s_resourceRequiresAuthentication[resource] = required;
        }

        #endregion
    }

    /// <summary>
    /// Represents an immutable version of <see cref="AuthenticationOptions"/>.
    /// </summary>
    public sealed class ReadonlyAuthenticationOptions
    {
        #region [ Members ]

        // Fields
        private readonly AuthenticationOptions m_authenticationOptions;

        #endregion

        #region [ Constructors ]

        internal ReadonlyAuthenticationOptions(AuthenticationOptions authenticationOptions)
        {
            m_authenticationOptions = authenticationOptions;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the expression that will match paths for the resources on the web server
        /// that should redirect to the <see cref="LoginPage"/> when authentication fails.
        /// </summary>
        public string AuthFailureRedirectResourceExpression => m_authenticationOptions.AuthFailureRedirectResourceExpression;

        /// <summary>
        /// Gets the expression that will match paths for the resources on the web server
        /// that can be provided without checking credentials.
        /// </summary>
        public string AnonymousResourceExpression => m_authenticationOptions.AnonymousResourceExpression;


        /// <summary>
        /// Gets the expression that will match paths for the resources on the web server
        /// that should use the alternate SecurityProvider.
        /// </summary>
        public string AlternateSecurityProviderResourceExpression => m_authenticationOptions.AlternateSecurityProviderResourceExpression;

        /// <summary>
        /// Gets the token used for identifying the authentication token in cookie headers.
        /// </summary>
        public string AuthenticationToken => m_authenticationOptions.AuthenticationToken;

        /// <summary>
        /// Gets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken => m_authenticationOptions.SessionToken;

        /// <summary>
        /// Gets the token used for anti-forgery verification in HTTP request headers.
        /// </summary>
        public string RequestVerificationToken => m_authenticationOptions.RequestVerificationToken;

        /// <summary>
        /// Gets token to specify when using AJAX for request verification tokens.
        /// </summary>
        public string AjaxRequestVerificationToken => m_authenticationOptions.AjaxRequestVerificationToken;

        /// <summary>
        /// Gets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage => m_authenticationOptions.LoginPage;

        /// <summary>
        /// Gets the path for the logout page.
        /// </summary>
        public string LogoutPage => m_authenticationOptions.LogoutPage;

        /// <summary>
        /// Gets the page name used to test user authorization.
        /// </summary>
        public string AuthTestPage => m_authenticationOptions.AuthTestPage;

        /// <summary>
        /// Gets the authentication schemes to use when testing authentication with the <see cref="AuthTestPage"/>.
        /// </summary>
        public AuthenticationSchemes AuthenticationSchemes => m_authenticationOptions.AuthenticationSchemes;

        /// <summary>
        /// Gets the parameter name for the <see cref="AuthTestPage"/> that forces it to use Basic authentication
        /// so that any cached browser credentials can be cleared.
        /// </summary>
        public string ClearCredentialsParameter => m_authenticationOptions.ClearCredentialsParameter;

        /// <summary>
        /// Gets the case-sensitive identifier that defines the protection space for this authentication.
        /// </summary>
        public string Realm => m_authenticationOptions.Realm;

        /// <summary>
        /// Gets any custom header to be displayed on the <see cref="LoginPage"/>.
        /// </summary>
        public string LoginHeader => m_authenticationOptions.LoginHeader;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the given resource is an authentication failure redirect resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an authentication failure redirect resource.</param>
        /// <returns><c>true</c> if path is an authentication failure redirect resource; otherwise, <c>false</c>.</returns>
        public bool IsAuthFailureRedirectResource(string urlPath) =>
            m_authenticationOptions.IsAuthFailureRedirectResource(urlPath);

        /// <summary>
        /// Determines whether the given resource is an anonymous resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAnonymousResource(string urlPath) =>
            m_authenticationOptions.IsAnonymousResource(urlPath);

        /// <summary>
        /// Determines whether the given resource is using the alternate SecuityProvider.
        /// </summary>
        /// <param name="urlPath">Path to check as an alternative SecurityProvider resource.</param>
        /// <returns><c>true</c> if path is an alternate Security Provider resource; otherwise, <c>false</c>.</returns>
        public bool IsAlternateSecurityProviderResource(string urlPath) =>
            m_authenticationOptions.IsAlternateSecurityProviderResource(urlPath);

        #endregion
    }

    /// <summary>
    /// Extension methods for authentication options.
    /// </summary>
    public static class AuthenticationOptionsExtensions
    {
        /// <summary>
        /// Gets full path to <see cref="AuthenticationOptions.LoginPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the login page.</returns>
        public static string GetFullLoginPath(this AuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.LoginPage, basePath);

        /// <summary>
        /// Gets full path to <see cref="AuthenticationOptions.LogoutPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the logout page.</returns>
        public static string GetFullLogoutPath(this AuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.LogoutPage, basePath);

        /// <summary>
        /// Gets full path to <see cref="AuthenticationOptions.AuthTestPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the auth test page.</returns>
        public static string GetFullAuthTestPath(this AuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.AuthTestPage, basePath);

        /// <summary>
        /// Gets full path to <see cref="ReadonlyAuthenticationOptions.LoginPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the login page.</returns>
        public static string GetFullLoginPath(this ReadonlyAuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.LoginPage, basePath);

        /// <summary>
        /// Gets full path to <see cref="ReadonlyAuthenticationOptions.LogoutPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the logout page.</returns>
        public static string GetFullLogoutPath(this ReadonlyAuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.LogoutPage, basePath);

        /// <summary>
        /// Gets full path to <see cref="ReadonlyAuthenticationOptions.AuthTestPage"/>,
        /// evaluating leading <c>~</c> character as the given base path.
        /// </summary>
        /// <param name="options">Authentication options for the website</param>
        /// <param name="basePath">Base path of the website</param>
        /// <returns>The full path to the auth test page.</returns>
        public static string GetFullAuthTestPath(this ReadonlyAuthenticationOptions options, string basePath) =>
            s_basePathRegex.Replace(options.AuthTestPage, basePath);

        private static readonly Regex s_basePathRegex = new("^~(?=/|$)", RegexOptions.Compiled);
    }
}