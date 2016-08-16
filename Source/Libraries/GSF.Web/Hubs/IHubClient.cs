//******************************************************************************************************
//  IHubClient.cs - Gbtc
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
//  08/16/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Interface that defines needed operations for hub client implementations.
    /// </summary>
    public interface IHubClient : IDisposable
    {
        /// <summary>
        /// Gets or sets reference to SignalR hub instance.
        /// </summary>
        IHub HubInstance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets delegate to use to log status messages.
        /// </summary>
        Action<string, UpdateType> LogStatusMessageFunction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets delegate to use to log exception messages.
        /// </summary>
        Action<Exception> LogExceptionFunction
        {
            get;
            set;
        }

    }
}
