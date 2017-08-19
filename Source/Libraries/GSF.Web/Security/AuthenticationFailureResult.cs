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
        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage requestMessage)
        {
            ReasonPhrase = reasonPhrase;
            RequestMessage = requestMessage;
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns unauthorized HTTP response message.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>Response message result of execution.</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = ReasonPhrase,
                RequestMessage = RequestMessage
            });
        }

        #endregion
    }
}
