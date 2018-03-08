//******************************************************************************************************
//  AntiForgeryWorker.cs - Gbtc
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Mvc;
using GSF.Security;

namespace GSF.Web.Security.AntiCsrf
{
    internal sealed class AntiForgeryWorker
    {
        private readonly ReadOnlyAntiForgeryConfig m_config = new ReadOnlyAntiForgeryConfig();
        private readonly AntiForgeryTokenStore m_tokenStore = new AntiForgeryTokenStore();
        private readonly TokenValidator m_validator = new TokenValidator();

        private void CheckSSLConfig(HttpRequestMessage request)
        {
            if (m_config.RequireSSL && !request.RequestUri.Scheme.Equals("https"))
                throw new InvalidOperationException("The anti-forgery system has the configuration value AntiForgeryConfig.RequireSsl = true, but the current request is not an SSL request.");
        }

        private AntiForgeryToken DeserializeToken(string serializedToken)
        {
            return !string.IsNullOrEmpty(serializedToken) ? AntiForgeryTokenSerializer.Deserialize(serializedToken) : null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Caller will just regenerate token in case of failure.")]
        private AntiForgeryToken DeserializeTokenNoThrow(string serializedToken)
        {
            try
            {
                return DeserializeToken(serializedToken);
            }
            catch
            {
                // ignore failures since we'll just generate a new token
                return null;
            }
        }

        private static IIdentity ExtractIdentity(HttpRequestMessage request)
        {
            SecurityPrincipal securityPrincipal = request.GetRequestContext().Principal as SecurityPrincipal;
            return securityPrincipal?.Identity;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Caller will just regenerate token in case of failure.")]
        private AntiForgeryToken GetCookieTokenNoThrow(HttpRequestMessage request)
        {
            try
            {
                return m_tokenStore.GetCookieToken(request);
            }
            catch
            {
                // ignore failures since we'll just generate a new token
                return null;
            }
        }

        // [ ENTRY POINT ]
        // Generates an anti-XSRF token pair for the current user. The return
        // value is the hidden input form element that should be rendered in
        // the <form>. This method has a side effect: it may set a response
        // cookie.
        public TagBuilder GetFormInputElement(HttpResponseMessage response)
        {
            CheckSSLConfig(response.RequestMessage);

            AntiForgeryToken oldCookieToken = GetCookieTokenNoThrow(response.RequestMessage);
            AntiForgeryToken newCookieToken, formToken;

            GetTokens(response.RequestMessage, oldCookieToken, out newCookieToken, out formToken);

            // If a new cookie was generated, persist it.
            if (newCookieToken != null)
                m_tokenStore.SaveCookieToken(response, newCookieToken);

            if (!m_config.SuppressXFrameOptionsHeader)
            {
                // Adding X-Frame-Options header to prevent ClickJacking. See
                // http://tools.ietf.org/html/draft-ietf-websec-x-frame-options-10
                // for more information.
                const string FrameHeaderName = "X-Frame-Options";

                if (!response.Headers.Contains(FrameHeaderName))
                    response.Headers.Add(FrameHeaderName, "SAMEORIGIN");
            }

            // <input type="hidden" name="__AntiForgeryToken" value="..." />
            TagBuilder retVal = new TagBuilder("input");

            retVal.Attributes["type"] = "hidden";
            retVal.Attributes["name"] = m_config.FormFieldName;
            retVal.Attributes["value"] = AntiForgeryTokenSerializer.Serialize(formToken);

            return retVal;
        }

        // [ ENTRY POINT ]
        // Generates a (cookie, form) serialized token pair for the current user.
        // The caller may specify an existing cookie value if one exists. If the
        // 'new cookie value' out param is non-null, the caller *must* persist
        // the new value to cookie storage since the original value was null or
        // invalid. This method is side-effect free.
        public void GetTokens(HttpRequestMessage request, string serializedOldCookieToken, out string serializedNewCookieToken, out string serializedFormToken)
        {
            CheckSSLConfig(request);

            AntiForgeryToken oldCookieToken = DeserializeTokenNoThrow(serializedOldCookieToken);
            AntiForgeryToken newCookieToken, formToken;
            GetTokens(request, oldCookieToken, out newCookieToken, out formToken);

            serializedNewCookieToken = Serialize(newCookieToken);
            serializedFormToken = Serialize(formToken);
        }

        private void GetTokens(HttpRequestMessage request, AntiForgeryToken oldCookieToken, out AntiForgeryToken newCookieToken, out AntiForgeryToken formToken)
        {
            newCookieToken = null;

            // Need to make sure we're always operating with a good cookie token.
            if (!m_validator.IsCookieTokenValid(oldCookieToken))
                oldCookieToken = newCookieToken = m_validator.GenerateCookieToken();

            //Contract.Assert(m_validator.IsCookieTokenValid(oldCookieToken));
            formToken = m_validator.GenerateFormToken(request, ExtractIdentity(request), oldCookieToken);
        }

        private string Serialize(AntiForgeryToken token)
        {
            return token != null ? AntiForgeryTokenSerializer.Serialize(token) : null;
        }

        // [ ENTRY POINT ]
        // Given an HttpContext, validates that the anti-XSRF tokens contained
        // in the cookies & form are OK for this request.
        public void Validate(HttpRequestMessage request)
        {
            CheckSSLConfig(request);

            // Extract cookie & form tokens
            AntiForgeryToken cookieToken = m_tokenStore.GetCookieToken(request);
            AntiForgeryToken formToken = m_tokenStore.GetFormToken(request);

            // Validate
            m_validator.ValidateTokens(request, ExtractIdentity(request), cookieToken, formToken);
        }

        // [ ENTRY POINT ]
        // Given the serialized string representations of a cookie & form token,
        // validates that the pair is OK for this request.
        public void Validate(HttpRequestMessage request, string cookieToken, string formToken)
        {
            CheckSSLConfig(request);

            // Extract cookie & form tokens
            AntiForgeryToken deserializedCookieToken = DeserializeToken(cookieToken);
            AntiForgeryToken deserializedFormToken = DeserializeToken(formToken);

            // Validate
            m_validator.ValidateTokens(request, ExtractIdentity(request), deserializedCookieToken, deserializedFormToken);
        }
    }
}