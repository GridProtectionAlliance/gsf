//******************************************************************************************************
//  WindowsServiceClient.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GSF.Communication;
using GSF.Net.Security;
using GSF.ServiceProcess;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Class to connect and communicate with windows service.
    /// </summary>
    public class WindowsServiceClient : IDisposable
    {
        #region [ Members ]

        // Fields
        private ClientBase m_remotingClient;
        private ClientHelper m_clientHelper;
        private string m_cachedStatus;
        private readonly int m_statusBufferSize;
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
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
            string setting;

            if (settings.TryGetValue("statusBufferSize", out setting) && !string.IsNullOrWhiteSpace(setting))
                m_statusBufferSize = int.Parse(setting);
            else
                m_statusBufferSize = 8192;

            m_cachedStatus = string.Empty;

            // Initialize remoting client socket.
            if (!settings.TryGetValue("enabledSslProtocols", out setting) || setting.Equals("None", StringComparison.OrdinalIgnoreCase))
                m_remotingClient = InitializeTlsClient(connectionString);
            else
                m_remotingClient = InitializeTcpClient(connectionString);

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

        private TcpClient InitializeTcpClient(string connectionString)
        {
            Dictionary<string, string> settings;
            string setting;

            TcpClient remotingClient;

            // Initialize remoting client socket.
            remotingClient = new TcpClient();
            remotingClient.ConnectionString = connectionString;
            remotingClient.PayloadAware = true;
            remotingClient.IgnoreInvalidCredentials = true;
            remotingClient.MaxConnectionAttempts = -1;

            // Parse connection string into key-value pairs
            settings = connectionString.ParseKeyValuePairs();

            // See if user wants to connect to remote service using integrated security
            if (settings.TryGetValue("integratedSecurity", out setting) && !string.IsNullOrWhiteSpace(setting))
                remotingClient.IntegratedSecurity = setting.ParseBoolean();

            return remotingClient;
        }

        private TlsClient InitializeTlsClient(string connectionString)
        {
            Dictionary<string, string> settings;
            string setting;

            TlsClient remotingClient;
            SslProtocols enabledSslProtocols;
            SslPolicyErrors validPolicyErrors;
            X509ChainStatusFlags validChainFlags;

            // Initialize remoting client socket.
            remotingClient = new TlsClient();
            remotingClient.ConnectionString = connectionString;
            remotingClient.PayloadAware = true;
            remotingClient.IgnoreInvalidCredentials = true;
            remotingClient.MaxConnectionAttempts = -1;
            remotingClient.RemoteCertificateValidationCallback = RemoteCertificateValidationCallback;

            // Parse connection string into key-value pairs
            settings = connectionString.ParseKeyValuePairs();

            // See if user wants to connect to remote service using integrated security
            if (settings.TryGetValue("integratedSecurity", out setting) && !string.IsNullOrWhiteSpace(setting))
                remotingClient.IntegratedSecurity = setting.ParseBoolean();

            // See if the user has explicitly defined the set of enabled SslProtocols
            if (settings.TryGetValue("enabledSslProtocols", out setting) && Enum.TryParse(setting, out enabledSslProtocols))
                remotingClient.EnabledSslProtocols = enabledSslProtocols;

            // See if the user has explicitly defined valid policy errors or valid chain flags
            if (settings.TryGetValue("validPolicyErrors", out setting) && Enum.TryParse(setting, out validPolicyErrors))
                remotingClient.ValidPolicyErrors = validPolicyErrors;

            if (settings.TryGetValue("validChainFlags", out setting) && Enum.TryParse(setting, out validChainFlags))
                remotingClient.ValidChainFlags = validChainFlags;

            // See if the user has explicitly defined whether to execute revocation checks on server certificates
            if (settings.TryGetValue("checkCertificateRevocation", out setting) && !string.IsNullOrWhiteSpace(setting))
                remotingClient.CheckCertificateRevocation = setting.ParseBoolean();

            return remotingClient;
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            TlsClient remotingClient;
            IPEndPoint remoteEndPoint;
            IPHostEntry localhost;
            SimplePolicyChecker policyChecker;

            remotingClient = m_remotingClient as TlsClient;

            if ((object)remotingClient != null)
            {
                remoteEndPoint = remotingClient.Client.RemoteEndPoint as IPEndPoint;

                if ((object)remoteEndPoint != null)
                {
                    // Create an exception and do not check policy for localhost
                    localhost = Dns.GetHostEntry("localhost");

                    if (localhost.AddressList.Any(address => address.Equals(remoteEndPoint.Address)))
                        return true;
                }

                // Not connected to localhost, so use the policy checker
                policyChecker = new SimplePolicyChecker();
                policyChecker.ValidPolicyErrors = remotingClient.ValidPolicyErrors;
                policyChecker.ValidChainFlags = remotingClient.ValidChainFlags;

                return policyChecker.ValidateRemoteCertificate(sender, certificate, chain, sslPolicyErrors);
            }

            return false;
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
