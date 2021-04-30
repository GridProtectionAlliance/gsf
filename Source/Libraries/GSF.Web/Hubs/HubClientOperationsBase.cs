//******************************************************************************************************
//  HubClientOperationsBase.cs - Gbtc
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
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Represents a base class for SignalR hub operations that need to track session based clients.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IHubClient"/> for hub operations.</typeparam>
    public abstract class HubClientOperationsBase<T> where T : class, IHubClient, new()
    {
        #region [ Members ]

        // Fields
        private readonly IHub m_hub;
        private readonly Action<string, UpdateType> m_logStatusMessageFunction;
        private readonly Action<Exception> m_logExceptionFunction;
        private T m_hubClient;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="HubClientOperationsBase{T}"/> instance.
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        protected HubClientOperationsBase(IHub hub, Action<string, UpdateType> logStatusMessageFunction = null, Action<Exception> logExceptionFunction = null)
        {
            m_hub = hub;
            m_logStatusMessageFunction = logStatusMessageFunction;
            m_logExceptionFunction = logExceptionFunction;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets instance of hub client for current SignalR session.
        /// </summary>
        protected T HubClient
        {
            get
            {
                if ((object)m_hubClient != null)
                    return m_hubClient;

                // Attempt to get connection ID from active hub context
                string connectionID = m_hub?.Context?.ConnectionId;

                if (string.IsNullOrEmpty(connectionID))
                {
                    // If no connection ID was available, check to see if base hub is a record operations hub,
                    // if so, see if there is an assigned connection ID to use, such as may be the case if the
                    // hub instance is being used outside of a SignalR context, like an IHostedHttpHandler
                    IRecordOperationsHub recordOperationsHub = m_hub as IRecordOperationsHub;

                    connectionID = recordOperationsHub?.ConnectionID;

                    if (string.IsNullOrEmpty(connectionID))
                        return null;
                }

                m_hubClient = s_hubClients.GetOrAdd(connectionID, id => new T
                {
                    HubInstance = m_hub,
                    ConnectionID = connectionID,
                    LogStatusMessageFunction = m_logStatusMessageFunction,
                    LogExceptionFunction = m_logExceptionFunction
                });

                return m_hubClient;
            }
        }

        /// <summary>
        /// Disposes any associated <see cref="IHubClient"/> when SignalR session is disconnected.
        /// </summary>
        /// <remarks>
        /// This method should be called within implementing SignalR hub when stop is called.
        /// </remarks>
        public virtual void EndSession()
        {
            EndSession(m_hub?.Context?.ConnectionId);
        }

        #endregion

        #region [ Static ]

        // Static Fields

        // References to session based IHubClient implementations - since a SignalR session can persist through
        // any number of hub instances, this dictionary is used to track IHubClient instances
        private static readonly ConcurrentDictionary<string, T> s_hubClients;

        // Static Constructor
        static HubClientOperationsBase()
        {
            s_hubClients = new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Attempts to get hub client for SignalR session with specified <paramref name="connectionID"/>.
        /// </summary>
        /// <param name="connectionID">SignalR session ID to attempt to find.</param>
        /// <param name="client">Associated client, if found.</param>
        /// <returns><c>true</c> if session was found; otherwise, <c>false</c>.</returns>
        public bool TryGetHubClient(string connectionID, out T client) => 
            s_hubClients.TryGetValue(connectionID, out client);

        /// <summary>
        /// Disposes any associated <see cref="IHubClient"/> when SignalR session is disconnected.
        /// </summary>
        /// <param name="connectionID">SignalR connection ID.</param>
        /// <remarks>
        /// This method should be called within implementing SignalR hub when stop is called.
        /// </remarks>
        public static void EndSession(string connectionID)
        {
            // Dispose of hub client when SignalR session is disconnected
            if (!string.IsNullOrEmpty(connectionID) && s_hubClients.TryRemove(connectionID, out T client))
                client?.Dispose();
        }

        #endregion
    }
}
