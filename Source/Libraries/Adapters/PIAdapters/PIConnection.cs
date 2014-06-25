//******************************************************************************************************
//  PIConnection.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/17/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using PISDK;

namespace PIAdapters
{
    /// <summary>
    /// Represents a STA connection to a PI server that handles open, close and execute operations.
    /// </summary>
    // ReSharper disable SuspiciousTypeConversion.Global
    public class PIConnection : IComparable<PIConnection>, IComparable, IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="ConnectTimeout"/>.
        /// </summary>
        public const int DefaultConnectTimeout = 30000;

        // Events

        /// <summary>
        /// Raised when <see cref="PIConnection"/> has been disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        // Fields
        private string m_serverID;                                  // Unique ID of server connection
        private string m_serverName;                                // Server name for PI connection
        private string m_userName;                                  // Username for PI connection
        private string m_password;                                  // Password for PI connection
        private int m_connectTimeout;                               // PI connection timeout
        private volatile bool m_connected;                          // Flag that determines connectivity
        private volatile Exception m_connectionException;           // Last connection exception
        private volatile Exception m_operationException;            // Last operation exception
        private volatile Action<Server> m_operation;                // Current operation
        private readonly AutoResetEvent m_pendingOperation;         // Pending operation wait event
        private readonly ManualResetEventSlim m_executingOperation; // Executing operation wait event
        private readonly Guid m_instanceID;                         // Unique instance ID for this connection
        private bool m_disposed;

        // Connection pool access count
        internal volatile int AccessCount;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new OSI-PI connection.
        /// </summary>
        public PIConnection()
        {
            m_connectTimeout = DefaultConnectTimeout;
            m_pendingOperation = new AutoResetEvent(false);
            m_executingOperation = new ManualResetEventSlim(true);
            m_instanceID = Guid.NewGuid();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PIConnection"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PIConnection()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        public string ServerName
        {
            get
            {
                return m_serverName;
            }
            set
            {
                m_serverName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection.
        /// </summary>
        public int ConnectTimeout
        {
            get
            {
                return m_connectTimeout;
            }
            set
            {
                m_connectTimeout = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PIConnection"/> is currently connected to <see cref="ServerName"/>.
        /// </summary>
        public bool Connected
        {
            get
            {
                return m_connected;
            }
        }

        /// <summary>
        /// Gets flag that determines if an OSI-PI server operation is currently executing.
        /// </summary>
        public bool Busy
        {
            get
            {
                return !m_executingOperation.IsSet;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PIConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIConnection"/> object and optionally releases the managed resources.
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
                        m_connected = false;
                        m_connectionException = null;
                        m_operationException = null;

                        if ((object)m_executingOperation != null)
                        {
                            m_executingOperation.Set();
                            m_executingOperation.Dispose();
                        }

                        if ((object)m_pendingOperation != null)
                        {
                            m_pendingOperation.Set();
                            m_pendingOperation.Dispose();
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
        /// Attempts to open <see cref="PIConnection"/>.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        public void Open()
        {
            if (m_connected || (object)m_serverID != null)
                throw new InvalidOperationException("OSI-PI server connection is already open.");

            if (string.IsNullOrWhiteSpace(m_serverName))
                throw new InvalidOperationException("Cannot open OSI-PI server connection without a defined server name.");

            // Reset operational variables
            m_operation = null;
            m_connectionException = null;
            m_operationException = null;

            // Clear any pending events
            m_executingOperation.Set();
            m_pendingOperation.Set();

            // Create a wait handle for connection on STA thread
            ManualResetEventSlim connectionEvent = new ManualResetEventSlim(false);

            // PI server only allows independent connections to the same PI server using a single-threaded apartment state.
            Thread operationsThread = new Thread(OperationsThread);
            operationsThread.IsBackground = true;
            operationsThread.SetApartmentState(ApartmentState.STA);
            operationsThread.Start(connectionEvent);

            if (!connectionEvent.Wait(m_connectTimeout))
            {
                operationsThread.Abort();
                throw new TimeoutException(string.Format("Timeout occurred while attempting connection to configured OSI-PI server '{0}'.", m_serverName));
            }

            if ((object)m_connectionException != null)
                throw new InvalidOperationException(string.Format("Failed to connect to configured OSI-PI server '{0}': {1}", m_serverName, m_connectionException.Message), m_connectionException);
        }

        /// <summary>
        /// Closes <see cref="PIConnection"/>.
        /// </summary>
        public void Close()
        {
            m_connected = false;
            m_pendingOperation.Set();
        }

        /// <summary>
        /// Executes an action against the <see cref="PIConnection"/> on its STA operations thread.
        /// </summary>
        /// <remarks>
        /// This method ensures that each operation on OSI-PI server object is handled synchronously on its primary STA thread.
        /// </remarks>
        /// <param name="operation">Action to execute.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Execute(Action<Server> operation)
        {
            if (!m_connected)
                throw new InvalidOperationException("PI Connection is not open - cannot execute action.");

            // Set new action to execute and resume operations thread
            m_executingOperation.Reset();
            m_operation = operation;
            m_pendingOperation.Set();

            // Wait for action to complete or fail
            m_executingOperation.Wait();

            // If action failed, raise its exception
            if ((object)m_operationException != null)
                throw m_operationException;
        }

        // All operations related to the OSI-PI server object instance are handled on this STA thread
        [HandleProcessCorruptedStateExceptions]
        private void OperationsThread(object state)
        {
            // Algorithm:
            //   -- open connection
            //   -- loop while connected
            //         execute operation
            //         wait for next operation
            //   -- close connection
            try
            {
                Server server = null;
                m_connected = false;

                try
                {
                    // Initialize PI SDK
                    PISDK.PISDK sdk = new PISDK.PISDK();

                    try
                    {
                        // Locate configured PI server
                        server = sdk.Servers[m_serverName];
                        ((_DServerDisconnectEvents_Event)server).OnDisconnect += PIConnection_OnDisconnect;

                        try
                        {
                            // Attempt to open OSI-PI connection
                            if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                                server.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                            else
                                server.Open();

                            m_serverID = server.ServerID;
                            m_connected = true;
                        }
                        catch (Exception ex)
                        {
                            m_connectionException = ex;
                        }
                    }
                    catch (Exception ex)
                    {
                        m_connectionException = new InvalidOperationException(string.Format("Failed to locate configured OSI-PI server '{0}': {1}", m_serverName, ex.Message), ex);
                    }
                }
                catch (Exception ex)
                {
                    m_connectionException = new InvalidOperationException(string.Format("Failed to initialize OSI-PI SDK: {0}", ex.Message), ex);
                }

                // Not expected, but if we get here with no server object we need to fail connection
                if ((object)server == null && (object)m_connectionException == null)
                    m_connectionException = new NullReferenceException("Failed to create PI server connection.");

                ManualResetEventSlim connectionEvent = state as ManualResetEventSlim;

                // Unblock any threads waiting for connection to complete or fail
                if ((object)connectionEvent != null)
                    connectionEvent.Set();

                // Exit thread if connection failed
                if ((object)m_connectionException != null)
                    return;

                // Perform operations on OSI-PI connection
                while (m_connected)
                {
                    try
                    {
                        m_operationException = null;

                        if ((object)m_operation != null)
                            m_operation(server);

                        m_executingOperation.Set();
                    }
                    catch (Exception ex)
                    {
                        m_operationException = ex;
                        m_executingOperation.Set();
                    }

                    m_pendingOperation.WaitOne();
                }

                // Attempt to close OSI-PI connection
                if (server.Connected)
                    server.Close();

                // Detach from server disconnected event
                ((_DServerDisconnectEvents_Event)server).OnDisconnect -= PIConnection_OnDisconnect;
            }
            catch (Exception ex)
            {
                if (!m_disposed)
                {
                    m_operationException = ex;
                    m_executingOperation.Set();
                }
            }
            finally
            {
                // Always clear server ID before thread exit
                m_serverID = null;
            }
        }

        private void PIConnection_OnDisconnect(Server server)
        {
            if ((object)m_serverID == null || server.ServerID != m_serverID)
                return;

            if (m_connected)
            {
                // Raise event in MTA space
                ThreadPool.QueueUserWorkItem(state =>
                {
                    if ((object)Disconnected != null)
                        Disconnected(this, EventArgs.Empty);

                    Close();
                });
            }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: 
        /// Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.
        /// Zero This object is equal to <paramref name="other"/>.
        /// Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(PIConnection other)
        {
            if ((object)other == null)
                return 1;

            return Comparer<Guid>.Default.Compare(m_instanceID, other.m_instanceID);
        }

        int IComparable.CompareTo(object obj)
        {
            PIConnection other = obj as PIConnection;

            if ((object)other != null)
                return CompareTo(other);

            throw new ArgumentException();
        }

        #endregion
    }
}