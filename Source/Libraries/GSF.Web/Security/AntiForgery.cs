//******************************************************************************************************
//  AntiForgery.cs - Gbtc
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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Mvc;
using GSF.Web.Security.AntiCsrf;

namespace GSF.Web.Security
{
    /// <summary>
    /// Provides access to the anti-forgery system, which provides protection against
    /// Cross-site Request Forgery (CSRF, also called XSRF) attacks.
    /// </summary>
    public static class AntiForgery
    {
        private static readonly AntiForgeryWorker s_worker = new AntiForgeryWorker();

        /// <summary>
        /// Generates an anti-forgery token for this request. This token can
        /// be validated by calling the Validate() method.
        /// </summary>
        /// <param name="response">Response message.</param>
        /// <returns>An HTML string corresponding to an &lt;input type="hidden"&gt;
        /// element. This element should be put inside a &lt;form&gt;.</returns>
        /// <remarks>
        /// This method has a side effect: it may set a response cookie.
        /// </remarks>
        public static string GetHtml(HttpResponseMessage response)
        {
            TagBuilder retVal = s_worker.GetFormInputElement(response);
            return retVal.ToString(TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// Generates an anti-forgery token pair (cookie and form token) for this request.
        /// This method is similar to GetHtml(), but this method gives the caller control
        /// over how to persist the returned values. To validate these tokens, call the
        /// appropriate overload of Validate.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="oldCookieToken">The anti-forgery token - if any - that already existed
        /// for this request. May be null. The anti-forgery system will try to reuse this cookie
        /// value when generating a matching form token.</param>
        /// <param name="newCookieToken">Will contain a new cookie value if the old cookie token
        /// was null or invalid. If this value is non-null when the method completes, the caller
        /// must persist this value in the form of a response cookie, and the existing cookie value
        /// should be discarded. If this value is null when the method completes, the existing
        /// cookie value was valid and needn't be modified.</param>
        /// <param name="formToken">The value that should be stored in the &lt;form&gt;. The caller
        /// should take care not to accidentally swap the cookie and form tokens.</param>
        /// <remarks>
        /// Unlike the GetHtml() method, this method has no side effect. The caller
        /// is responsible for setting the response cookie and injecting the returned
        /// form token as appropriate.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Method is intended for advanced audiences.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Method is intended for advanced audiences.")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void GetTokens(HttpRequestMessage request, string oldCookieToken, out string newCookieToken, out string formToken)
        {
            s_worker.GetTokens(request, oldCookieToken, out newCookieToken, out formToken);
        }

        /// <summary>
        /// Validates an anti-forgery token that was supplied for this request.
        /// The anti-forgery token may be generated by calling GetHtml().
        /// </summary>
        /// <remarks>
        /// Throws an HttpAntiForgeryException if validation fails.
        /// </remarks>
        public static void Validate(HttpRequestMessage request)
        {
            s_worker.Validate(request);
        }

        /// <summary>
        /// Validates an anti-forgery token pair that was generated by the GetTokens method.
        /// </summary>
        /// <param name="request">Request message.</param>
        /// <param name="cookieToken">The token that was supplied in the request cookie.</param>
        /// <param name="formToken">The token that was supplied in the request form body.</param>
        /// <remarks>
        /// Throws an HttpAntiForgeryException if validation fails.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Validate(HttpRequestMessage request, string cookieToken, string formToken)
        {
            s_worker.Validate(request, cookieToken, formToken);
        }
    }
}
