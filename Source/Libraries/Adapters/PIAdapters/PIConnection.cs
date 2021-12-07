//******************************************************************************************************
//  PIConnection.cs - Gbtc
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
//  06/17/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2014 - J. Ritchie Carroll
//       Updated to use AF-SDK
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using OSIsoft.AF.PI;

namespace PIAdapters
{
    /// <summary>
    /// Represents a connection to a PI server that handles open, close and executes operations.
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

        private volatile bool m_connected;                          // Flag that determines connectivity
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
            ConnectTimeout = DefaultConnectTimeout;
            m_instanceID = Guid.NewGuid();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets PI server object for this PI connection.
        /// </summary>
        public PIServer Server { get; private set; }

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection.
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Gets flag that determines if <see cref="PIConnection"/> is currently connected to <see cref="ServerName"/>.
        /// </summary>
        public bool Connected => m_connected;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PIConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);   // In case any future subclass has a finalizer
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIConnection"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                Close();
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Attempts to open <see cref="PIConnection"/>.
        /// </summary>
        public void Open()
        {
            if (m_connected || (Server is not null && Server.ConnectionInfo.IsConnected))
                throw new InvalidOperationException("OSI-PI server connection is already open.");

            if (string.IsNullOrWhiteSpace(ServerName))
                throw new InvalidOperationException("Cannot open OSI-PI server connection without a defined server name.");

            m_connected = false;

            try
            {
                // Locate configured PI server
                PIServers servers = new();
                Server = servers[ServerName];

                if (Server is null)
                    throw new InvalidOperationException("Server not found in the PI servers collection.");

                Server.ConnectChanged += PIConnection_ConnectChanged;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to connect to PI server \"{0}\": {1}", ServerName, ex.Message));
            }

            if (!Server.ConnectionInfo.IsConnected)
            {
                // Attempt to open OSI-PI connection
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                    Server.Connect(new NetworkCredential(UserName, Password));
                else
                    Server.Connect(true);
            }

            m_connected = true;
        }

        /// <summary>
        /// Closes <see cref="PIConnection"/>.
        /// </summary>
        public void Close()
        {
            m_connected = false;

            // Attempt to close OSI-PI connection
            if (Server is not null)
            {
                if (Server.ConnectionInfo.IsConnected)
                    Server.Disconnect();

                // Detach from server disconnected event
                Server.ConnectChanged -= PIConnection_ConnectChanged;
            }
        }

        private void PIConnection_ConnectChanged(object sender, EventArgs e)
        {
            if (Server is null || !m_connected)
                return;

            Disconnected?.Invoke(this, EventArgs.Empty);

            Close();
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
            if (other is null)
                return 1;

            return Comparer<Guid>.Default.Compare(m_instanceID, other.m_instanceID);
        }

        int IComparable.CompareTo(object obj)
        {
            PIConnection other = obj as PIConnection;

            if (other is not null)
                return CompareTo(other);

            throw new ArgumentException();
        }

        #endregion
    }
}