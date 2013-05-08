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
//  08/20/2009 - Paul B. Trachian
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/23/2009 - Pinal C. Patel
//       Fixed errors introduced by breaking change to add support for classification of service updates.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.ServiceProcess;
using GSF;

namespace UDPRebroadcaster
{
    public partial class ServiceHost : ServiceBase
    {
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
            // Register components.
            m_serviceHelper.ServiceComponents.Add(m_udpServer);
            m_serviceHelper.ServiceComponents.Add(m_udpClient);

            // Start the UDP server.
            m_udpServer.ClientConnected += UdpServer_ClientConnected;
            m_udpServer.ClientDisconnected += UdpServer_ClientDisconnected;
            m_udpServer.SendClientDataException += UdpServer_SendClientDataException;
            m_udpServer.Start();

            // Connect the UDP client.
            m_udpClient.ConnectionAttempt += UdpClient_ConnectionAttempt;
            m_udpClient.ConnectionEstablished += UdpClient_ConnectionEstablished;
            m_udpClient.ConnectionTerminated += UdpClient_ConnectionTerminated;
            m_udpClient.ConnectionException += UdpClient_ConnectionException;
            m_udpClient.ReceiveDataComplete += UdpClient_ReceiveDataComplete;
            m_udpClient.ConnectAsync();
        }

        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            // Unregister event handlers.
            m_udpServer.ClientConnected -= UdpServer_ClientConnected;
            m_udpServer.ClientDisconnected -= UdpServer_ClientDisconnected;
            m_udpServer.SendClientDataException -= UdpServer_SendClientDataException;
            m_udpClient.ConnectionAttempt -= UdpClient_ConnectionAttempt;
            m_udpClient.ConnectionEstablished -= UdpClient_ConnectionEstablished;
            m_udpClient.ConnectionTerminated -= UdpClient_ConnectionTerminated;
            m_udpClient.ConnectionException -= UdpClient_ConnectionException;
            m_udpClient.ReceiveDataComplete -= UdpClient_ReceiveDataComplete;
        }

        private void UdpServer_ClientConnected(object sender, EventArgs<Guid> e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SERVER] Client connected\r\n\r\n");
        }

        private void UdpServer_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SERVER] Client disconnected\r\n\r\n");
        }

        private void UdpServer_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            m_serviceHelper.ErrorLogger.Log(e.Argument2);
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[SERVER] Error rebroadcasting data - {0}\r\n\r\n", e.Argument2.Message);
        }

        private void UdpClient_ConnectionAttempt(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[CLIENT] Attempting connection to {0}\r\n\r\n", m_udpClient.ServerUri);
        }

        private void UdpClient_ConnectionEstablished(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Information, "[CLIENT] Connection established\r\n\r\n");
        }

        private void UdpClient_ConnectionTerminated(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Warning, "[CLIENT] Connection terminated\r\n\r\n");
        }

        private void UdpClient_ConnectionException(object sender, EventArgs<Exception> e)
        {
            m_serviceHelper.UpdateStatus(UpdateType.Alarm, "[CLIENT] Error connecting - {0}\r\n\r\n", e.Argument.Message);
        }

        private void UdpClient_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            try
            {
                // Rebroadcast received data to all clients.
                m_udpServer.MulticastAsync(e.Argument1, 0, e.Argument2);
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Warning, "[CLIENT] Error rebroadcasting data - {0}\r\n\r\n", ex.Message);
            }
        }

        #endregion
    }
}
