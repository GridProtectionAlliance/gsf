//******************************************************************************************************
//  FormatSpecificationInspector.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/30/2015 - A.S. Bikle
//       Generated original version of source code.
//
//******************************************************************************************************

#if !MONO

using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace GSF.ServiceModel.Activation
{
    /// <summary>
    /// Message inspector intended to be attached to WCF REST endpoints.
    /// Examines incoming requests for "application/json" in the "Accept" header, and sets the response format to Json when present.
    /// </summary>
    public class FormatSpecificationInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            WebOperationContext context = WebOperationContext.Current;

            if ((object)context == null)
                return null;

            string acceptHeader = context.IncomingRequest.Headers["Accept"];

            if ((object)acceptHeader != null && acceptHeader.Contains("application/json"))
                context.OutgoingResponse.Format = WebMessageFormat.Json;

            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }
    }
}

#endif