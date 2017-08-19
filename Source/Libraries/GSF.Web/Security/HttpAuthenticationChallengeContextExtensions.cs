//******************************************************************************************************
//  HttpAuthenticationChallengeContextExtensions.cs - Gbtc
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
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace GSF.Web.Security
{
    /// <summary>
    /// Defines extension functions related to <see cref="HttpAuthenticationChallengeContext"/>.
    /// </summary>
    public static class HttpAuthenticationChallengeContextExtensions
    {
        /// <summary>
        /// Adds challenge header to HTTP authentication challenge context execution result, when unauthorized,
        /// for specified <paramref name="scheme"/>.
        /// </summary>
        /// <param name="context">HTTP authentication challenge context.</param>
        /// <param name="scheme">Authentication scheme.</param>
        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme));
        }

        /// <summary>
        /// Adds challenge header to HTTP authentication challenge context execution result, when unauthorized,
        /// for specified <paramref name="scheme"/>.
        /// </summary>
        /// <param name="context">HTTP authentication challenge context.</param>
        /// <param name="scheme">Authentication scheme.</param>
        /// <param name="parameter">Authentication scheme parameter.</param>
        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme, string parameter)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme, parameter));
        }

        /// <summary>
        /// Adds specified <paramref name="challenge"/> header to HTTP authentication challenge context execution result,
        /// when unauthorized.
        /// </summary>
        /// <param name="context">HTTP authentication challenge context.</param>
        /// <param name="challenge">Authentication header value containing the challenge.</param>
        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, AuthenticationHeaderValue challenge)
        {
            if ((object)context == null)
                throw new ArgumentNullException(nameof(context));

            // Wrap provided HTTP action with one that will test if the status code of context execution
            // is unauthorized and add a needed challenge header when required
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
        }
    }
}
