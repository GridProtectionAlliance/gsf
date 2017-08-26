//******************************************************************************************************
//  AuthenticationMiddleware.cs - Gbtc
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
//
//******************************************************************************************************

using GSF.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace GSF.Web.Security
{
    /// <summary>
    /// Middleware for configuring authentication using <see cref="ISecurityProvider"/> in the Owin pipeline.
    /// </summary>
    public class AuthenticationMiddleware : AuthenticationMiddleware<AuthenticationOptions>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthenticationMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware object in the pipeline.</param>
        /// <param name="options">The options for authentication.</param>
        public AuthenticationMiddleware(OwinMiddleware next, AuthenticationOptions options)
            : base(next, options)
        {
        }

        /// <summary>
        /// Returns the authentication handler that provides the authentication logic.
        /// </summary>
        /// <returns>The authentication handler to provide authentication logic.</returns>
        protected override AuthenticationHandler<AuthenticationOptions> CreateHandler()
        {
            return new AuthenticationHandler();
        }
    }
}
