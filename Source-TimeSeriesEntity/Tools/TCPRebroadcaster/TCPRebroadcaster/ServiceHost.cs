//******************************************************************************************************
//  ServiceHost.cs - Gbtc
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
//  12/01/2011 - Pinal C. Patel
//       Generated original version of source code.
//  12/28/2011 - Pinal C. Patel
//       Modified to use TcpClient for both receiving and transmitting the data.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;
using GSF;
using GSF.Communication;
using GSF.Configuration;

namespace TCPRebroadcaster
{
    public partial class ServiceHost : ServiceBase
    {
        #region [ Members ]

        private bool m_shutdown;
        private TcpClient m_source;
        private List<TcpClient> m_targets;

        #endregion

        #region [ Constructors ]

        public ServiceHost()
        {
            InitializeComponent();

            // Register event handlers.
            m_serviceHelper.ServiceStarted += ServiceHelper_ServiceStarted;
            m_serviceHelper.ServiceStopping += ServiceHelper_ServiceStopping;
        }

        public ServiceHost(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Methods ]

        private void ServiceHelper_ServiceStarted(object sender, EventArgs e)
        {
            // Initialize config.
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings["SystemSettings"];

            // Connect to source.
            m_source = new TcpClient();
            m_source.ConnectionString = string.Format("Server={0}", settings["Source"].Value);
            m_source.ConnectionAttempt += SourceClient_ConnectionAttempt;
            m_source.ConnectionEstablished += SourceClient_ConnectionEstablished;
            m_source.ConnectionTerminated += SourceClient_ConnectionTerminated;
            m_source.ReceiveDataComplete += SourceClient_ReceiveDataComplete;
            m_source.ConnectAsync();
            m_serviceHelper.ServiceComponents.Add(m_source);

            // Connect to target.
            m_targets = new List<TcpClient>();
            foreach (string item in settings["Target"].Value.Replace(',', ';').Split(';'))
            {
                TcpClient target = new TcpClient();
                target.ConnectionString = string.Format("Server={0}", item.Trim());
                target.ConnectionAttempt += TargetClient_ConnectionAttempt;
                target.ConnectionEstablished += TargetClient_ConnectionEstablished;
                target.ConnectionTerminated += TargetClient_ConnectionTerminated;
                target.ReceiveDataException += TargetClient_ReceiveDataException;
                target.ConnectAsync();
                m_serviceHelper.ServiceComponents.Add(target);
            }
        }

        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            // Indicate shutdown.
            m_shutdown = true;

            // Disconnect from source.
            m_source.ConnectionAttempt -= SourceClient_ConnectionAttempt;
            m_source.ConnectionEstablished -= SourceClient_ConnectionEstablished;
            m_source.ConnectionTerminated -= SourceClient_ConnectionTerminated;
            m_source.ReceiveDataComplete -= SourceClient_ReceiveDataComplete;
            m_source.Dispose();

            // Disconnect from target.
            List<TcpClient> targets;
            lock (m_targets)
            {
                targets = new List<TcpClient>(m_targets);
            }

            foreach (TcpClient target in targets)
            {
                target.ConnectionAttempt -= TargetClient_ConnectionAttempt;
                target.ConnectionEstablished -= TargetClient_ConnectionEstablished;
                target.ConnectionTerminated -= TargetClient_ConnectionTerminated;
                target.ReceiveDataException -= TargetClient_ReceiveDataException;
                target.Dispose();
            }
        }

        private void SourceClient_ConnectionAttempt(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SOURCE] Attempting connection to {0}\r\n\r\n", m_source.ServerUri);
        }

        private void SourceClient_ConnectionEstablished(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SOURCE] Connection to {0} established\r\n\r\n", m_source.ServerUri);
        }

        private void SourceClient_ConnectionTerminated(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SOURCE] Connection to {0} terminated\r\n\r\n", m_source.ServerUri);
            m_source.ConnectAsync();
        }

        private void SourceClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            lock (m_targets)
            {
                foreach (TcpClient target in m_targets)
                {
                    try
                    {
                        target.SendAsync(e.Argument1, 0, e.Argument2);
                    }
                    catch (Exception ex)
                    {
                        m_serviceHelper.ErrorLogger.Log(ex);
                        m_serviceHelper.UpdateStatus(UpdateType.Warning, "[SOURCE] Error sending data to {0} - {1}\r\n\r\n", target.ServerUri, ex.Message);
                    }
                }
            }
        }

        private void TargetClient_ConnectionAttempt(object sender, EventArgs e)
        {
            // Stop connection attempts for non-connected endpoints.
            TcpClient endpoint = (TcpClient)sender;
            if (m_shutdown)
                endpoint.Disconnect();

            m_serviceHelper.UpdateStatus(UpdateType.Information, "[TARGET] Attempting connection to {0}\r\n\r\n", endpoint.ServerUri);
        }

        private void TargetClient_ConnectionEstablished(object sender, EventArgs e)
        {
            // Add endpoint to the distribution list.
            TcpClient endpoint = (TcpClient)sender;
            lock (m_targets)
            {
                m_targets.Add(endpoint);
            }

            m_serviceHelper.UpdateStatus(UpdateType.Information, "[TARGET] Connection to {0} established\r\n\r\n", endpoint.ServerUri);
        }

        private void TargetClient_ConnectionTerminated(object sender, EventArgs e)
        {
            // Remove endpoint from the distribution list.
            TcpClient endpoint = (TcpClient)sender;
            lock (m_targets)
            {
                m_targets.Remove(endpoint);
            }

            m_serviceHelper.UpdateStatus(UpdateType.Information, "[TARGET] Connection to {0} terminated\r\n\r\n", endpoint.ServerUri);
            endpoint.ConnectAsync();
        }

        private void TargetClient_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            TcpClient endpoint = (TcpClient)sender;

            m_serviceHelper.ErrorLogger.Log(e.Argument);
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[TARGET] Error receving data in {0} - {1}\r\n\r\n", endpoint.ServerUri, e.Argument.Message);
        }

        #endregion
    }
}
