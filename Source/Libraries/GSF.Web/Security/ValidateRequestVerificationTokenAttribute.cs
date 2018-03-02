//******************************************************************************************************
//  ValidateRequestVerificationTokenAttribute.cs - Gbtc
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
//  02/13/2018 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using GSF.Web.Model;

namespace GSF.Web.Security
{
    /// <summary>
    /// Requests that the controller or method validate the anti-forgery request verification
    /// token values found in the HTTP headers.
    /// </summary>
    /// <remarks>
    /// For self-hosted web sites, use this attribute to validate anti-forgery tokens instead of
    /// the <see cref="System.Web.Mvc.ValidateAntiForgeryTokenAttribute"/> which requires an
    /// active <see cref="System.Web.HttpContext"/>.
    /// </remarks>
    public class ValidateRequestVerificationTokenAttribute : ActionFilterAttribute
    {
        private readonly HashSet<string> m_httpMethods;
        private readonly bool m_skipValidation;
        private bool m_skipMethodCheck;

        /// <summary>
        /// Gets or sets flag that determines if validation should occur via posted form data or header data.
        /// Set to <c>true</c> to use form data with <see cref="HtmlHelper.RequestVerificationToken"/> function;
        /// otherwise, set to <c>false</c> to use with <see cref="HtmlHelper.RequestVerificationHeaderToken"/>
        /// function (e.g., when used via JSON). Defaults to <c>false</c>.
        /// </summary>
        public bool FormValidation { get; set; } = false;

        /// <summary>
        /// Gets or sets HTTP methods, as a comma separated value string, for which validation will apply.
        /// Defaults to <c>*</c>, meaning validation applies to all possible HTTP methods.
        /// </summary>
        public string HttpMethods
        {
            get
            {
                return string.Join(",", m_httpMethods);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = "*";

                m_httpMethods.Clear();
                m_httpMethods.UnionWith(value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(method => method.Trim().ToUpperInvariant()));
                m_skipMethodCheck = value.Trim().Equals("*", StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Creates a new <see cref="ValidateRequestVerificationTokenAttribute"/>.
        /// </summary>
        public ValidateRequestVerificationTokenAttribute() : this(false)
        {
        }

        internal ValidateRequestVerificationTokenAttribute(bool skipValidation)
        {
            m_httpMethods = new HashSet<string>(new[] { "*" }, StringComparer.OrdinalIgnoreCase);
            m_skipValidation = skipValidation;
            m_skipMethodCheck = true;
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (!m_skipValidation && (m_skipMethodCheck || m_httpMethods.Contains(actionContext.Request.Method.Method)))
                actionContext.Request.ValidateRequestVerificationToken(FormValidation);

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}
