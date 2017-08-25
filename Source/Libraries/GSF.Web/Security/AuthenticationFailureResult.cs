//******************************************************************************************************
//  AuthenticationFailureResult.cs - Gbtc
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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents an HTTP action result for an unauthorized request.
    /// </summary>
    public class AuthenticationFailureResult : IHttpActionResult
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AuthenticationFailureResult"/>.
        /// </summary>
        /// <param name="reasonPhrase">Phrase that describes reason for authorization failure.</param>
        /// <param name="requestMessage">HTTP request message to use as the source of the response.</param>
        /// <param name="statusCode">HTTP status code to return for authorization failure.</param>
        /// <param name="redirectLocation">HTTP redirect location, if used.</param>
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage requestMessage, HttpStatusCode statusCode, string redirectLocation = null)
        {
            ReasonPhrase = reasonPhrase;
            RequestMessage = requestMessage;
            StatusCode = statusCode;
            RedirectLocation = redirectLocation;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the phrase that describes reason for authorization failure.
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// Gets the HTTP request message used as the source of the response.
        /// </summary>
        public HttpRequestMessage RequestMessage { get; }

        /// <summary>
        /// Gets the HTTP status code to return for authorization failure.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the HTTP redirect location, if used.
        /// </summary>
        public string RedirectLocation { get; private set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns unauthorized HTTP response message.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>Response message result of execution.</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(StatusCode)
            {
                ReasonPhrase = ReasonPhrase,
                RequestMessage = RequestMessage,
            };

            if (!string.IsNullOrWhiteSpace(RedirectLocation))
            {
                if (!RedirectLocation.StartsWith("/"))
                    RedirectLocation = $"/{RedirectLocation}";

                response.Headers.Location = new Uri($"{RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority)}{RedirectLocation}");
            }

            return Task.FromResult(response);
        }

        #endregion
    }
}
