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
    public class ValidateRequestVerificationTokenAttribute : ActionFilterAttribute
    {
        private readonly bool m_skipValidation;

        /// <summary>
        /// Gets or sets flag that determines if validation should occur via posted form data or header data.
        /// Set to <c>true</c> to use form data with <see cref="HtmlHelper.RequestVerificationToken"/> function;
        /// otherwise, set to <c>false</c> to use with <see cref="HtmlHelper.RequestVerificationHeaderToken"/>
        /// function (e.g., when used via JSON). Defaults to <c>false</c>.
        /// </summary>
        public bool FormValidation { get; set; } = false;

        /// <summary>
        /// Creates a new <see cref="ValidateRequestVerificationTokenAttribute"/>.
        /// </summary>
        public ValidateRequestVerificationTokenAttribute()
        {
            m_skipValidation = false;
        }

        internal ValidateRequestVerificationTokenAttribute(bool skipValidation)
        {
            m_skipValidation = skipValidation;
        }

        /// <summary>Occurs before the action method is invoked.</summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (!m_skipValidation)
                actionContext.Request.ValidateRequestVerificationToken(FormValidation);

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}
