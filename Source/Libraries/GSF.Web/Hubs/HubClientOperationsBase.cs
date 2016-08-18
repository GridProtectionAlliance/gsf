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
        protected T HubClient => m_hubClient ??
        (
            m_hubClient = s_hubClients.GetOrAdd(m_hub.Context.ConnectionId, connectionID =>
                new T
                {
                    HubInstance = m_hub,
                    ConnectionID = connectionID,
                    LogStatusMessageFunction = m_logStatusMessageFunction,
                    LogExceptionFunction = m_logExceptionFunction
                })
        );

        /// <summary>
        /// Disposes any associated <see cref="IHubClient"/> when SignalR session is disconnected.
        /// </summary>
        /// <remarks>
        /// This method should be called within implementing SignalR hub when stop is called.
        /// </remarks>
        public void EndSession()
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
        /// Disposes any associated <see cref="IHubClient"/> when SignalR session is disconnected.
        /// </summary>
        /// <param name="connectionID">SignalR connection ID.</param>
        /// <remarks>
        /// This method should be called within implementing SignalR hub when stop is called.
        /// </remarks>
        public static void EndSession(string connectionID)
        {
            T client;

            // Dispose of hub client when SignalR session is disconnected
            if (!string.IsNullOrEmpty(connectionID) && s_hubClients.TryRemove(connectionID, out client))
                client?.Dispose();
        }

        #endregion

    }
}
