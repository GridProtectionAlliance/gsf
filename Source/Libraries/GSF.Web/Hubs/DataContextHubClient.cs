//******************************************************************************************************
//  DataContextHubClient.cs - Gbtc
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
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNet.SignalR;
using GSF.Web.Model;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Represents a SignalR <see cref="Hub"/> client session instance of a data context.
    /// </summary>
    public class DataContextHubClient : HubClientBase
    {
        #region [ Members ]

        // Fields
        private readonly ConcurrentDictionary<Type, DataTable> m_primaryKeySessionCache;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataContextHubClient"/>.
        /// </summary>
        public DataContextHubClient()
        {
            m_primaryKeySessionCache = new ConcurrentDictionary<Type, DataTable>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets primary key cache for current session.
        /// </summary>
        public ConcurrentDictionary<Type, DataTable> PrimaryKeySessionCache => m_primaryKeySessionCache;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets a new <see cref="Model.DataContext"/> instance for the current SignalR hub session, applying
        /// any existing primary key caches.
        /// </summary>
        public DataContext GetDataContext(string settingsCategory)
        {
            // Create a new data context using session based primary key cache
            return new DataContext(settingsCategory ?? "systemSettings", exceptionHandler: ex => LogException(ex))
            {
                PrimaryKeySessionCache = m_primaryKeySessionCache
            };
        }

        #endregion
    }
}
