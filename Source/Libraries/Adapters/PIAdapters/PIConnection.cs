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
    public class PIConnection : IDisposable
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Raised when <see cref="PIConnection"/> has been disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        // Fields
        private Server m_server;                                    // SDK Server object for PI connection
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
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new OSI-PI connection.
        /// </summary>
        public PIConnection()
        {
            // Make sure shared instance of OSI-PI SDK is available - we wait to do this until an initial
            // instance of this class is created so that any possible exceptions related to SDK creation
            // can be better managed. If PI-SDK is not installed, this will throw an exception.
            lock (s_piSDKLock)
            {
                if ((object)s_piSDK == null)
                {
                    try
                    {
                        // Initialize PI SDK
                        s_piSDK = new PISDK.PISDK();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format("Failed to initialize OSI-PI SDK: {0}", ex.Message), ex);
                    }
                }
            }

            m_connectTimeout = 30000;
            m_pendingOperation = new AutoResetEvent(false);
            m_executingOperation = new ManualResetEventSlim(true);
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
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
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
            if (m_connected || (object)m_server != null)
                throw new InvalidOperationException("OSI-PI server connection is already open.");

            if (string.IsNullOrWhiteSpace(m_serverName))
                throw new InvalidOperationException("Cannot open OSI-PI server connection without a defined server name.");

            // Locate configured PI server
            try
            {
                lock (s_piSDKLock)
                {
                    m_server = s_piSDK.Servers[m_serverName];
                    ((_DServerDisconnectEvents_Event)m_server).OnDisconnect += PIConnection_OnDisconnect;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to locate configured OSI-PI server '{0}': {1}", m_serverName, ex.Message), ex);
            }

            ManualResetEventSlim connectionEvent = new ManualResetEventSlim(false);
            m_operation = null;
            m_connectionException = null;
            m_operationException = null;

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
                m_connected = false;

                // Attempt to open OSI-PI connection
                try
                {
                    if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                        m_server.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                    else
                        m_server.Open();

                    m_connected = true;
                }
                catch (Exception ex)
                {
                    m_connectionException = ex;
                }
                finally
                {
                    ManualResetEventSlim connectionEvent = state as ManualResetEventSlim;

                    if ((object)connectionEvent != null)
                        connectionEvent.Set();
                }

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
                            m_operation(m_server);

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
                if ((object)m_server != null)
                {
                    if (m_server.Connected)
                        m_server.Close();

                    // Detach from server disconnected event
                    ((_DServerDisconnectEvents_Event)m_server).OnDisconnect -= PIConnection_OnDisconnect;
                }
            }
            catch (Exception ex)
            {
                if (!m_disposed)
                    m_operationException = ex;
            }
            finally
            {
                // Always clear PI server object before thread exit
                m_server = null;
            }
        }

        private void PIConnection_OnDisconnect(Server server)
        {
            if ((object)m_server == null || server.ServerID != m_server.ServerID)
                return;

            if (m_connected)
            {
                if ((object)Disconnected != null)
                    Disconnected(this, EventArgs.Empty);

                Close();
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly object s_piSDKLock = new object();
        private static PISDK.PISDK s_piSDK;

        #endregion
    }
}