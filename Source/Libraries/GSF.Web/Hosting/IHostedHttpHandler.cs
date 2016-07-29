//******************************************************************************************************
//  IHostedHttpHandler.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  07/28/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Net.Http;
using System.Threading.Tasks;

namespace GSF.Web.Hosting
{
    /// <summary>
    /// Defines an HTTP handler for self-hosted <see cref="WebServer"/> instances.
    /// </summary>
    public interface IHostedHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP web requests by a custom handler that implements the <see cref="IHostedHttpHandler"/> interface.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="response">HTTP response message.</param>
        Task ProcessRequestAsync(HttpRequestMessage request, HttpResponseMessage response);
    }
}
