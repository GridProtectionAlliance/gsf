//******************************************************************************************************
//  IAntiForgeryAdditionalDataProvider.cs - Gbtc
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

using System.Net.Http;

namespace GSF.Web.Security
{
    /// <summary>
    /// Allows providing or validating additional custom data for anti-forgery tokens.
    /// For example, the developer could use this to supply a nonce when the token is
    /// generated, then he could validate the nonce when the token is validated.
    /// </summary>
    /// <remarks>
    /// The anti-forgery system already embeds the client's username within the
    /// generated tokens. This interface provides and consumes <em>supplemental</em>
    /// data. If an incoming anti-forgery token contains supplemental data but no
    /// additional data provider is configured, the supplemental data will not be
    /// validated.
    /// </remarks>
    public interface IAntiForgeryAdditionalDataProvider
    {
        /// <summary>
        /// Provides additional data to be stored for the anti-forgery tokens generated
        /// during this request.
        /// </summary>
        /// <param name="request">Information about the current request.</param>
        /// <returns>Supplemental data to embed within the anti-forgery token.</returns>
        string GetAdditionalData(HttpRequestMessage request);

        /// <summary>
        /// Validates additional data that was embedded inside an incoming anti-forgery
        /// token.
        /// </summary>
        /// <param name="request">Information about the current request.</param>
        /// <param name="additionalData">Supplemental data that was embedded within the token.</param>
        /// <returns>True if the data is valid; false if the data is invalid.</returns>
        bool ValidateAdditionalData(HttpRequestMessage request, string additionalData);
    }
}