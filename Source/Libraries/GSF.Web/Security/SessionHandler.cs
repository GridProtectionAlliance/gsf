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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents an HTTP messaging handler that can inject a session ID that can be used for security.
    /// </summary>
    public class SessionHandler : DelegatingHandler
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="SessionToken"/>;
        /// </summary>
        public const string DefaultSessionToken = "session";

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SessionHandler"/>.
        /// </summary>
        /// <param name="sessionToken">Token used for identifying the session ID in cookie headers.</param>
        public SessionHandler(string sessionToken = DefaultSessionToken)
        {
            SessionToken = sessionToken;
        }

        #endregion

        #region [ Properties ]

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
            string sessionID = cookie?[SessionToken].Value;
            Guid result;

            // If session ID format is invalid, create a new one
            if (!Guid.TryParse(sessionID, out result))
                result = Guid.NewGuid();

            sessionID = result.ToString();

            // Save session ID (as Guid) in the request properties
            request.Properties[SessionToken] = result;

            // Continue processing the HTTP request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Store session ID in response message cookie
            response.Headers.AddCookies(new [] { new CookieHeaderValue(SessionToken, sessionID) });

            return response;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets the session ID Guid as defined in the request properties dictionary.
        /// </summary>
        /// <param name="request">The target HTTP request message.</param>
        /// <param name="sessionToken">Token used for identifying the session ID in properties dictionary.</param>
        /// <param name="sessionID">Validated session ID to return.</param>
        /// <returns>Session ID <see cref="Guid"/>, if defined; otherwise, <see cref="Guid.Empty"/>.</returns>
        public static bool TryGetSessionID(HttpRequestMessage request, string sessionToken, out Guid sessionID)
        {
            object result;

            if (request.Properties.TryGetValue(sessionToken ?? DefaultSessionToken, out result) && result is Guid)
            {
                sessionID = (Guid)result;
                return true;
            }

            sessionID = Guid.Empty;
            return false;
        }

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
            Guid sessionID;

            Guid.TryParse(value, out sessionID);

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
            Guid sessionID;

            Guid.TryParse(value, out sessionID);

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
            Guid sessionID;

            Guid.TryParse(value, out sessionID);

            return sessionID;
        }

        #endregion
    }
}
