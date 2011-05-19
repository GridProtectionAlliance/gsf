//******************************************************************************************************
//  WindowsServiceClient.cs - Gbtc
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
//  05/16/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using TVA;
using TVA.Communication;
using TVA.Services.ServiceProcess;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Class to connect and communicate with windows service.
    /// </summary>
    public class WindowsServiceClient : IDisposable
    {
        #region [ Members ]

        // Fields
        private TcpClient m_remotingClient;
        private ClientHelper m_clientHelper;
        private string m_cachedStatus;
        private int m_statusBufferSize;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="WindowsServiceClient"/>.
        /// </summary>
        /// <param name="connectionString">Connection information such as server ip address and port where windows service is running.</param>
        public WindowsServiceClient(string connectionString)
        {
            // Initialize status cache members.
            string statusBufferSize;

            if (connectionString.ParseKeyValuePairs().TryGetValue("statusBufferSize", out statusBufferSize))
                m_statusBufferSize = int.Parse(statusBufferSize);
            else
                m_statusBufferSize = 8192;

            m_cachedStatus = string.Empty;

            // Initialize remoting client socket.
            m_remotingClient = new TcpClient();
            m_remotingClient.ConnectionString = connectionString;
            m_remotingClient.SharedSecret = "openPDC";
            m_remotingClient.Handshake = true;
            m_remotingClient.PayloadAware = true;
            m_remotingClient.MaxConnectionAttempts = -1;

            // Initialize windows service client.
            m_clientHelper = new ClientHelper();
            m_clientHelper.RemotingClient = m_remotingClient;
            m_clientHelper.ReceivedServiceUpdate += ClientHelper_ReceivedServiceUpdate;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="WindowsServiceClient"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~WindowsServiceClient()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to ClientHelper object.
        /// </summary>
        public ClientHelper Helper
        {
            get
            {
                return m_clientHelper;
            }
        }

        /// <summary>
        /// Gets chached status information to display upon successful connection to windows service.
        /// </summary>
        public string CachedStatus
        {
            get
            {
                return m_cachedStatus;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles ConnectionException event for remoting client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_remotingClient_ConnectionException(object sender, EventArgs<Exception> e)
        {

        }

        /// <summary>
        /// Releases all the resources used by the <see cref="WindowsServiceClient"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="WindowsServiceClient"/> object and optionally releases the managed resources.
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
                        if (m_clientHelper != null)
                            m_clientHelper.ReceivedServiceUpdate -= ClientHelper_ReceivedServiceUpdate;
                        m_clientHelper = null;

                        if (m_remotingClient != null)
                        {
                            m_remotingClient.MaxConnectionAttempts = 0;

                            if (m_remotingClient.CurrentState == ClientState.Connected)
                                m_remotingClient.Disconnect();

                            m_remotingClient.Dispose();
                        }
                        m_remotingClient = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Handles ReceivedServiceUpdate event of ClientHelper.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ClientHelper_ReceivedServiceUpdate(object sender, EventArgs<UpdateType, string> e)
        {
            m_cachedStatus = (m_cachedStatus + e.Argument2).TruncateLeft(m_statusBufferSize);
        }

        #endregion
    }
}
