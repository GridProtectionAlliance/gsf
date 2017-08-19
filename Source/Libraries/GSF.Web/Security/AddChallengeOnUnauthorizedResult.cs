//******************************************************************************************************
//  AddChallengeOnUnauthorizedResult.cs - Gbtc
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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace GSF.Web.Security
{
    /// <summary>
    /// Represents an HTTP action result that will add a challenge header when a result is unauthorized.
    /// </summary>
    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AddChallengeOnUnauthorizedResult"/>.
        /// </summary>
        /// <param name="challenge">Authentication header value containing the challenge.</param>
        /// <param name="innerResult">Inner HTTP action result to execute for response.</param>
        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the authentication header value containing the challenge.
        /// </summary>
        public AuthenticationHeaderValue Challenge { get; }

        /// <summary>
        /// Gets the inner HTTP action result to execute for response.
        /// </summary>
        public IHttpActionResult InnerResult { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes <see cref="InnerResult"/> so challenge header can be added when unauthorized.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel async operation.</param>
        /// <returns>Response message result of execution.</returns>
        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

            // If request is not authenticated, add a challenge header
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Only add one challenge per authentication scheme
                if (response.Headers.WwwAuthenticate.All(header => header.Scheme != Challenge.Scheme))
                    response.Headers.WwwAuthenticate.Add(Challenge);
            }

            return response;
        }

        #endregion
    }
}
