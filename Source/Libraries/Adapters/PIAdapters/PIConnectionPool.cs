//******************************************************************************************************
//  PIConnectionPool.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/24/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF;

namespace PIAdapters
{
    /// <summary>
    /// Represents a pool of <see cref="PIConnection"/> instances.
    /// </summary>
    [Obsolete("Pooling connections was a PI-SDK optimization technique, this is not required for AF-SDK implementations.", false)]
    public class PIConnectionPool : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="AccessCountPerConnection"/>.
        /// </summary>
        public const int DefaultAccessCountPerConnection = 100;

        /// <summary>
        /// Default value for <see cref="MinimumPoolSize"/>.
        /// </summary>
        public const int DefaultMinimumPoolSize = 10;

        // Events

        /// <summary>
        /// Raised when a <see cref="PIConnection"/> has been disconnected from server.
        /// </summary>
        public event EventHandler<EventArgs<PIConnection>> Disconnected;

        // Fields
        private readonly List<PIConnection> m_connectionPool;
        private readonly int m_minimumPoolSize;

        private int m_accessCountPerConnection;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIConnectionPool"/>.
        /// </summary>
        public PIConnectionPool()
            : this(DefaultMinimumPoolSize)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PIConnectionPool"/>.
        /// </summary>
        /// <param name="minimumPoolSize">Minimum pool size to maintain.</param>
        public PIConnectionPool(int minimumPoolSize)
        {
            if (minimumPoolSize < 1)
                throw new ArgumentOutOfRangeException(nameof(minimumPoolSize));

            m_connectionPool = new List<PIConnection>();
            m_minimumPoolSize = minimumPoolSize;
            m_accessCountPerConnection = DefaultAccessCountPerConnection;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PIConnectionPool"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PIConnectionPool()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets minimum pool size for the <see cref="PIConnectionPool"/>.
        /// </summary>
        public int MinimumPoolSize
        {
            get
            {
                return m_minimumPoolSize;
            }
        }

        /// <summary>
        /// Gets or sets maximum accessibility count for each <see cref="PIConnection"/>.
        /// </summary>
        public int AccessCountPerConnection
        {
            get
            {
                return m_accessCountPerConnection;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_accessCountPerConnection = value;
            }
        }

        /// <summary>
        /// Gets current size of connection pool.
        /// </summary>
        public int Size
        {
            get
            {
                return m_connectionPool.Count;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PIConnectionPool"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIConnectionPool"/> object and optionally releases the managed resources.
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
                        if ((object)m_connectionPool != null)
                        {
                            foreach (PIConnection connection in m_connectionPool)
                            {
                                connection.Disconnected -= connection_Disconnected;
                                connection.Dispose();
                            }

                            m_connectionPool.Clear();
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Gets a connection from the pool or creates a new one if all current connections are being used at peak access.
        /// </summary>
        /// <param name="serverName">Name of the PI server for the adapter's PI connection.</param>
        /// <param name="userName">Name of the PI user ID for the adapter's PI connection.</param>
        /// <param name="password">Password used for the adapter's PI connection.</param>
        /// <param name="connectTimeout">Timeout interval (in milliseconds) for the adapter's connection.</param>
        /// <returns>A <see cref="PIConnection"/> from the pool.</returns>
        /// <exception cref="InvalidOperationException">Failed to get a pooled PI connection.</exception>
        public PIConnection GetPooledConnection(string serverName, string userName = null, string password = null, int connectTimeout = PIConnection.DefaultConnectTimeout)
        {
            PIConnection connection = null;

            // We dynamically allocate pooled PI server connections each having a maximum accessibility count.
            // PI's threading model can handle many connections each archiving a small volume of points, but
            // falls behind under load when archiving a large volume of points from a single connection.
            try
            {
                lock (m_connectionPool)
                {
                    while ((object)connection == null)
                    {
                        // Get next connection from the pool with lowest accessibility count
                        if (m_connectionPool.Count > 0)
                        {
                            PIConnection[] availableConnections = m_connectionPool.Where(c => c.AccessCount < m_accessCountPerConnection).ToArray();

                            if (availableConnections.Length > 0)
                                connection = availableConnections.Aggregate((currentMin, nextItem) => (object)nextItem != null && currentMin.AccessCount < nextItem.AccessCount ? currentMin : nextItem);
                        }

                        if ((object)connection == null)
                        {
                            // Add pooled connections in groups for better distribution
                            for (int i = 0; i < m_minimumPoolSize; i++)
                            {
                                // Create a new connection
                                connection = new PIConnection
                                {
                                    ServerName = serverName,
                                    UserName = userName,
                                    Password = password,
                                    ConnectTimeout = connectTimeout
                                };

                                // Since PI doesn't detect disconnection until an operation is attempted,
                                // we must monitor for disconnections from the pooled connections as well
                                connection.Disconnected += connection_Disconnected;
                                connection.Open();

                                // Add the new connection to the server pool
                                m_connectionPool.Add(connection);
                            }
                        }
                    }

                    // Increment current connection access count
                    connection.AccessCount++;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to get a pooled PI connection: {0}", ex.Message), ex);
            }

            return connection;
        }

        /// <summary>
        /// Returns <see cref="PIConnection"/> to the pool.
        /// </summary>
        /// <param name="connection"><see cref="PIConnection"/> to return to the pool.</param>
        /// <exception cref="InvalidOperationException">Provided PIConnection does not belong to this connection pool.</exception>
        public void ReturnPooledConnection(PIConnection connection)
        {
            if ((object)connection == null)
                return;

            lock (m_connectionPool)
            {
                if (!m_connectionPool.Contains(connection))
                    throw new InvalidOperationException("Provided PIConnection does not belong to this connection pool");

                if (connection.AccessCount > 0)
                    connection.AccessCount--;

                if (connection.AccessCount < 1)
                {
                    if (m_connectionPool.Count > m_minimumPoolSize && m_connectionPool.Remove(connection))
                    {
                        connection.Disconnected -= connection_Disconnected;
                        connection.Dispose();
                    }
                }
            }
        }

        private void connection_Disconnected(object sender, EventArgs e)
        {
            if ((object)Disconnected != null)
                Disconnected(this, new EventArgs<PIConnection>(sender as PIConnection));
        }

        #endregion
    }
}
