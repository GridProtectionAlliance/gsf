//******************************************************************************************************
//  HubClientBase.cs - Gbtc
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
//  08/16/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Represents a base class implementation for <see cref="IHubClient"/> implementations.
    /// </summary>
    /// <remarks>
    /// This base class is useful for defining classes that should persist per SignalR session.
    /// </remarks>
    public abstract class HubClientBase : IHubClient
    {
        #region [ Members ]

        // Fields
        private dynamic m_hubClient;
        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to SignalR hub client instance.
        /// </summary>
        /// <remarks>
        /// This property can be used to call registered Javascript hub functions.
        /// </remarks>
        public dynamic HubClient => m_hubClient ?? (m_hubClient = HubInstance?.Clients?.Client(HubInstance?.Context?.ConnectionId));

        /// <summary>
        /// Gets or sets reference to SignalR hub instance.
        /// </summary>
        public IHub HubInstance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets delegate to use to log status messages.
        /// </summary>
        public Action<string, UpdateType> LogStatusMessageFunction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets delegate to use to log exception messages.
        /// </summary>
        public Action<Exception> LogExceptionFunction
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="HubClientBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="HubClientBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        m_hubClient = null;
                        HubInstance = null;
                        LogStatusMessageFunction = null;
                        LogExceptionFunction = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Logs a status message to hub client and any provided log status message delegate.
        /// </summary>
        /// <param name="message">Message content to log.</param>
        /// <param name="type">Type of message to log.</param>
        /// <param name="logToClient">Determines if message should be logged to hub client.</param>
        /// <remarks>
        /// When <paramref name="logToClient"/> is <c>true</c>, function will
        /// call "sendInfoMessage" Javascript hub client function if defined.
        /// </remarks>
        protected void LogStatusMessage(string message, UpdateType type = UpdateType.Information, bool logToClient = true)
        {
            // Send status message to hub client
            if (logToClient)
                HubClient?.sendInfoMessage(message, type == UpdateType.Information ? 2000 : -1);

            // Send status message to program host
            LogStatusMessageFunction?.Invoke(message, type);

        }

        /// <summary>
        /// Logs an exception to hub client and any provided log exception delegate.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="logToClient">Determines if message should be logged to hub client.</param>
        /// <remarks>
        /// When <paramref name="logToClient"/> is <c>true</c>, function will
        /// call "sendErrorMessage" Javascript hub client function if defined.
        /// </remarks>
        protected void LogException(Exception ex, bool logToClient = true)
        {
            // Send exception to hub client
            if (logToClient)
                HubClient?.sendErrorMessage(ex.Message, -1);

            // Send exception to program host
            LogExceptionFunction?.Invoke(ex);
        }

        #endregion
    }
}
