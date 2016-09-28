//******************************************************************************************************
//  JsonFaultHandler.cs - Gbtc
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
//  09/28/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Creates an error handler for encoding errors as JSON responses.
    /// </summary>
    public class JsonFaultHandler : IErrorHandler
    {
        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the dispatcher aborts the session and the instance context in certain cases.
        /// </summary>
        /// <returns>true if Windows Communication Foundation (WCF) should not abort the session (if there is one) and instance context if the instance context is not <see cref="InstanceContextMode.Single" />; otherwise, false. The default is false.</returns>
        /// <param name="error">The exception thrown during processing.</param>
        public bool HandleError(Exception error)
        {
            return true;
        }

        /// <summary>
        /// Enables the creation of a custom <see cref="FaultException" /> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="Exception" /> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="Message" /> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            fault = Message.CreateMessage(version, "", new JsonFault(error), new DataContractJsonSerializer(typeof(JsonFault)));

            WebBodyFormatMessageProperty jsonFormatting = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, jsonFormatting);

            HttpResponseMessageProperty httpResponse = new HttpResponseMessageProperty
            {
                StatusCode = HttpStatusCode.InternalServerError,
                StatusDescription = error.Message,
            };

            fault.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
        }
    }
}
