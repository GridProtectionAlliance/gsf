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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
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

            if (authorization?.Scheme != "Basic")
            {
                // No authentication was attempted for this authentication method, do not set principal,
                // which would indicate success, nor ErrorResult, which would indicate an error
                return;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                // No authorization credentials were provided, set ErrorResult
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }

            await Task.Run(() =>
            {
                string userName, password;

                if (TryParseCredentials(authorization.Parameter, out userName, out password))
                {
                    // Setup the security principal
                    SecurityProviderCache.ValidateCurrentProvider(userName);
                    IPrincipal principal = Thread.CurrentPrincipal;

                    // Authenticate user, if not already authenticated
                    if (principal.Identity.IsAuthenticated || SecurityProviderCache.CurrentProvider.Authenticate(password))
                        context.Principal = principal;
                    else
                        context.ErrorResult = new AuthenticationFailureResult("Invalid user name or password", request);
                }
                else
                {
                    // Authentication was attempted but failed, set ErrorResult
                    context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                }
            },
            cancellationToken);
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

        // Static Methods

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
