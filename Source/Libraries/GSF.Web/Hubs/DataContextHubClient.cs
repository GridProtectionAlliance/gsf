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

using GSF.Web.Model;
using Microsoft.AspNet.SignalR;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Represents a SignalR <see cref="Hub"/> client session instance of a data context.
    /// </summary>
    public class DataContextHubClient : HubClientBase
    {
        #region [ Members ]

        // Fields
        private DataContext m_dataContext;
        private bool m_disposed;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets <see cref="Model.DataContext"/> instance for the current SignalR hub session, creating it if needed.
        /// </summary>
        public DataContext GetDataContext(string settingsCategory) => m_dataContext ?? (m_dataContext = new DataContext(settingsCategory ?? "systemSettings", exceptionHandler: ex => LogException(ex)));

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataContextHubClient"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_dataContext?.Dispose();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        #endregion
    }
}
