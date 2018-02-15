//******************************************************************************************************
//  TokenValidator.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/15/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

// Derived from AspNetWebStack (https://github.com/aspnet/AspNetWebStack) 
// Copyright (c) .NET Foundation. All rights reserved.
// See NOTICE.txt file in Source folder for more information.

#endregion

using System;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Mvc;

namespace GSF.Web.Security.AntiCsrf
{
    internal sealed class TokenValidator
    {
        private readonly ReadOnlyAntiForgeryConfig m_config = new ReadOnlyAntiForgeryConfig();

        public AntiForgeryToken GenerateCookieToken()
        {
            return new AntiForgeryToken()
            {
                // SecurityToken will be populated automatically.
                IsSessionToken = true
            };
        }

        public AntiForgeryToken GenerateFormToken(HttpRequestMessage request, IIdentity identity, AntiForgeryToken cookieToken)
        {
            //Contract.Assert(IsCookieTokenValid(cookieToken));
            AntiForgeryToken formToken = new AntiForgeryToken()
            {
                SecurityToken = cookieToken.SecurityToken,
                IsSessionToken = false
            };

            bool requireAuthenticatedUserHeuristicChecks = false;

            // populate Username
            if (identity != null && identity.IsAuthenticated)
            {
                if (!m_config.SuppressIdentityHeuristicChecks)
                {
                    // If the user is authenticated and heuristic checks are not suppressed,
                    // then Username or AdditionalData must be set.
                    requireAuthenticatedUserHeuristicChecks = true;
                }

                formToken.Username = identity.Name;
            }

            // populate AdditionalData
            if (m_config.AdditionalDataProvider != null)
            {
                formToken.AdditionalData = m_config.AdditionalDataProvider.GetAdditionalData(request);
            }

            if (requireAuthenticatedUserHeuristicChecks
                && string.IsNullOrEmpty(formToken.Username)
                && string.IsNullOrEmpty(formToken.AdditionalData))
            {
                // Application says user is authenticated, but we have no identifier for the user.
                throw new InvalidOperationException($"The provided identity of type '{identity.GetType()}' is marked IsAuthenticated = true but does not have a value for Name. By default, the anti-forgery system requires that all authenticated identities have a unique Name. If it is not possible to provide a unique Name for this identity, consider setting the static property AntiForgeryConfig.AdditionalDataProvider to an instance of a type that can provide some form of unique identifier for the current user.");
            }

            return formToken;
        }

        public bool IsCookieTokenValid(AntiForgeryToken cookieToken)
        {
            return cookieToken != null && cookieToken.IsSessionToken;
        }

        public void ValidateTokens(HttpRequestMessage request, IIdentity identity, AntiForgeryToken sessionToken, AntiForgeryToken fieldToken)
        {
            // Were the tokens even present at all?
            if (sessionToken == null)
                throw new HttpAntiForgeryException($"The required anti-forgery cookie \"{m_config.CookieName}\" is not present.");

            if (fieldToken == null)
                throw new HttpAntiForgeryException($"The required anti-forgery form field \"{m_config.FormFieldName}\" is not present.");

            // Do the tokens have the correct format?
            if (!sessionToken.IsSessionToken || fieldToken.IsSessionToken)
                throw new HttpAntiForgeryException($"Validation of the provided anti-forgery token failed. The cookie \"{m_config.CookieName}\" and the form field \"{m_config.FormFieldName}\" were swapped.");

            // Are the security tokens embedded in each incoming token identical?
            if (!Equals(sessionToken.SecurityToken, fieldToken.SecurityToken))
                throw new HttpAntiForgeryException("The anti-forgery cookie token and form field token do not match.");
            
            // Is the incoming token meant for the current user?
            string currentUsername = string.Empty;

            // Requests for AuthTest page come from anonymous users
            if (identity != null && identity.IsAuthenticated && !request.RequestUri.LocalPath.Equals(request.GetAuthenticationOptions().AuthTestPage, StringComparison.OrdinalIgnoreCase))
                currentUsername = identity.Name ?? string.Empty;

            // OpenID and other similar authentication schemes use URIs for the username.
            // These should be treated as case-sensitive.
            bool useCaseSensitiveUsernameComparison = currentUsername.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                                      || currentUsername.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (!string.Equals(fieldToken.Username, currentUsername, useCaseSensitiveUsernameComparison ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                throw new HttpAntiForgeryException($"The provided anti-forgery token was meant for user \"{fieldToken.Username}\", but the current user is \"{currentUsername}\".");

            // Is the AdditionalData valid?
            if (m_config.AdditionalDataProvider != null && !m_config.AdditionalDataProvider.ValidateAdditionalData(request, fieldToken.AdditionalData))
                throw new HttpAntiForgeryException("The provided anti-forgery token failed a custom data check.");
        }
    }
}