//******************************************************************************************************
//  DataContextOperations.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  04/26/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Data;
using Microsoft.AspNet.SignalR.Hubs;
using GSF.Web.Model;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Represents hub operations for using <see cref="DataContextHubClient"/> instances.
    /// </summary>
    public class DataContextOperations : HubClientOperationsBase<DataContextHubClient>
    {
        private readonly string m_settingsCategory;

        /// <summary>
        /// Creates a new <see cref="DataContextOperations"/> instance.
        /// </summary>
        /// <param name="hub">Parent hub.</param>
        /// <param name="settingsCategory">Setting category that contains the connection settings.</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public DataContextOperations(IHub hub, string settingsCategory, Action<string, UpdateType> logStatusMessageFunction = null, Action<Exception> logExceptionFunction = null) : base(hub, logStatusMessageFunction, logExceptionFunction)
        {
            m_settingsCategory = settingsCategory;
        }

        /// <summary>
        /// Gets primary key cache for current session.
        /// </summary>
        public ConcurrentDictionary<Type, DataTable> PrimaryKeySessionCache => HubClient.PrimaryKeySessionCache;

        /// <summary>
        /// Gets a new <see cref="Model.DataContext"/> instance for the current SignalR hub session, applying
        /// any existing primary key caches.
        /// </summary>
        public DataContext DataContext => HubClient.GetDataContext(m_settingsCategory);
    }
}
