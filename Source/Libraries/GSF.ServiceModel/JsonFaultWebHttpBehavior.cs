//******************************************************************************************************
//  JsonFaultWebHttpBehavior.cs - Gbtc
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
//  09/28/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Extends <see cref="WebHttpBehavior"/> for JSON error handling.
    /// </summary>
    public class JsonFaultWebHttpBehavior : WebHttpBehavior
    {
        /// <summary>
        /// Override this method to change the way errors that occur on the service are handled.
        /// </summary>
        /// <param name="endpoint">The service endpoint.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher.</param>
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // Clear default error handlers
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();

            // Add the JSON error handler
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new JsonFaultHandler());
        }
    }
}
