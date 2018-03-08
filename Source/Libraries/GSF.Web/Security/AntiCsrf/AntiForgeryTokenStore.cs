//******************************************************************************************************
//  AntiForgeryTokenStore.cs - Gbtc
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

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GSF.Web.Security.AntiCsrf
{
    // Saves anti-XSRF tokens split between HttpRequest.Cookies and HttpRequest.Form
    internal sealed class AntiForgeryTokenStore
    {
        private readonly ReadOnlyAntiForgeryConfig m_config = new ReadOnlyAntiForgeryConfig();

        public AntiForgeryToken GetCookieToken(HttpRequestMessage httpContext)
        {
            CookieHeaderValue cookie = httpContext.Headers.GetCookies(m_config.CookieName).FirstOrDefault();
            string cookieValue = cookie?[m_config.CookieName].Value;

            if (string.IsNullOrEmpty(cookieValue))
                return null;

            return AntiForgeryTokenSerializer.Deserialize(cookieValue);
        }

        public AntiForgeryToken GetFormToken(HttpRequestMessage request)
        {
            PostData postData = request.GetPostData();

            string formValue = postData.FormData[m_config.FormFieldName];

            if (string.IsNullOrEmpty(formValue))
                request.QueryParameters().TryGetValue(m_config.FormFieldName, out formValue);

            if (string.IsNullOrEmpty(formValue))
                return null;

            return AntiForgeryTokenSerializer.Deserialize(formValue);
        }

        public void SaveCookieToken(HttpResponseMessage response, AntiForgeryToken token)
        {
            string serializedToken = AntiForgeryTokenSerializer.Serialize(token);

            CookieHeaderValue newCookie = new CookieHeaderValue(m_config.CookieName, serializedToken)
            {
                HttpOnly = true
            };

            // Note: don't use "newCookie.Secure = _config.RequireSSL;" since the default
            // value of newCookie.Secure is automatically populated from the <httpCookies>
            // config element.
            if (m_config.RequireSSL)
                newCookie.Secure = true;

            response.Headers.AddCookies(new[] { newCookie });
        }
    }
}
