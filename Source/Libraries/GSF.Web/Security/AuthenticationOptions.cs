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

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents options for authentication using <see cref="AuthenticationHandler"/>.
    /// </summary>
    public class AuthenticationOptions : Microsoft.Owin.Security.AuthenticationOptions
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="AuthFailureRedirectResourceExpression"/>.
        /// </summary>
        public const string DefaultAuthFailureRedirectResourceExpression = @"^/$|^/.+\.cshtml$|^/.+\.vbhtml$";

        /// <summary>
        /// Default value for <see cref="AnonymousResourceExpression"/>.
        /// </summary>
        public const string DefaultAnonymousResourceExpression = "^/@|^/favicon.ico$";

        /// <summary>
        /// Default value for <see cref="PassThroughAuthSupportedBrowserExpression"/>.
        /// </summary>
        public const string DefaultPassThroughAuthSupportedBrowserExpression = @"^(.+\(Windows.+(MSIE |Trident/))|(.+\(Windows.+Chrome/((?! Edge/).)*)$";

        /// <summary>
        /// Default value for <see cref="LoginPage"/>.
        /// </summary>
        public const string DefaultLoginPage = "/@GSF/Web/Security/Views/Login.cshtml";

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
        private readonly ConcurrentDictionary<string, bool> m_passThroughAuthSupportedBrowserCache;
        private string m_authFailureRedirectResourceExpression;
        private string m_anonymousResourceExpression;
        private string m_passThroughAuthSupportedBrowserExpression;
        private Regex m_authFailureRedirectResources;
        private Regex m_anonymousResources;
        private Regex m_passThroughAuthSupportedBrowsers;
        private string m_realm;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions() : base(SessionHandler.DefaultSessionToken)
        {
            m_authFailureRedirectResourceCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            m_anonymousResourceCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            m_passThroughAuthSupportedBrowserCache = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the expression that will match paths for the resources on the web server
        /// that should redirect to the <see cref="LoginPage"/> when authentication fails.
        /// </summary>
        public string AuthFailureRedirectResourceExpression
        {
            get
            {
                return m_authFailureRedirectResourceExpression;
            }
            set
            {
                m_authFailureRedirectResourceExpression = value;
                m_authFailureRedirectResources = new Regex(m_authFailureRedirectResourceExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets the expression that will match paths for the resources on the web server
        /// that can be provided without checking credentials.
        /// </summary>
        public string AnonymousResourceExpression
        {
            get
            {
                return m_anonymousResourceExpression;
            }
            set
            {
                m_anonymousResourceExpression = value;
                m_anonymousResources = new Regex(m_anonymousResourceExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets expression that will match user-agent header string for browser clients
        /// that can support NTLM based pass-through authentication.
        /// </summary>
        public string PassThroughAuthSupportedBrowserExpression
        {
            get
            {
                return m_passThroughAuthSupportedBrowserExpression;
            }
            set
            {
                m_passThroughAuthSupportedBrowserExpression = value;
                m_passThroughAuthSupportedBrowsers = new Regex(m_passThroughAuthSupportedBrowserExpression, RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Gets or sets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken { get; set; } = SessionHandler.DefaultSessionToken;

        /// <summary>
        /// Gets or sets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage { get; set; } = DefaultLoginPage;

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
        /// Gets or sets any custom header to be displayed on the <see cref="LoginPage"/>.
        /// </summary>
        public string LoginHeader { get; set; }

        /// <summary>
        /// Gets an immutable version of the authentication options.
        /// </summary>
        public ReadonlyAuthenticationOptions Readonly => new ReadonlyAuthenticationOptions(this);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the given resource is an authentication failure redirect resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAuthFailureRedirectResource(string urlPath)
        {
            if ((object)m_authFailureRedirectResourceExpression == null)
                AuthFailureRedirectResourceExpression = DefaultAuthFailureRedirectResourceExpression;

            return m_authFailureRedirectResourceCache.GetOrAdd(urlPath, m_authFailureRedirectResources.IsMatch);
        }

        /// <summary>
        /// Determines whether the given resource is an anonymous resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAnonymousResource(string urlPath)
        {
            if ((object)m_anonymousResourceExpression == null)
                AnonymousResourceExpression = DefaultAnonymousResourceExpression;

            return m_anonymousResourceCache.GetOrAdd(urlPath, m_anonymousResources.IsMatch);
        }

        #endregion
    }

    /// <summary>
    /// Represents an immutable version of <see cref="AuthenticationOptions"/>.
    /// </summary>
    public class ReadonlyAuthenticationOptions
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
        /// Gets expression that will match user-agent header string for browser clients
        /// that can support NTLM based pass-through authentication.
        /// </summary>
        public string PassThroughAuthSupportedBrowserExpression => m_authenticationOptions.PassThroughAuthSupportedBrowserExpression;

        /// <summary>
        /// Gets the token used for identifying the session ID in cookie headers.
        /// </summary>
        public string SessionToken => m_authenticationOptions.SessionToken;

        /// <summary>
        /// Gets the login page used as a redirect location when authentication fails.
        /// </summary>
        public string LoginPage => m_authenticationOptions.LoginPage;

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
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAuthFailureRedirectResource(string urlPath) => m_authenticationOptions.IsAuthFailureRedirectResource(urlPath);

        /// <summary>
        /// Determines whether the given resource is an anonymous resource.
        /// </summary>
        /// <param name="urlPath">Path to check as an anonymous resource.</param>
        /// <returns><c>true</c> if path is an anonymous resource; otherwise, <c>false</c>.</returns>
        public bool IsAnonymousResource(string urlPath) => m_authenticationOptions.IsAnonymousResource(urlPath);

        #endregion
    }
}