//*******************************************************************************************************
//  ServiceHost.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Paul Trachian
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4279
//       Email: pbtrachian@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/20/2009 - Paul Trachian
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.ServiceProcess;
using TVA;

namespace UDPRebroadcaster
{
    public partial class ServiceHost : ServiceBase
    {
        #region [ Constructors ]

        public ServiceHost()
            : base()
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
            // Register event handlers.
            m_udpClient.ConnectionAttempt += UdpClient_ConnectionAttempt;
            m_udpClient.ConnectionException += UdpClient_ConnectionException;
            m_udpClient.ConnectionTerminated += UdpClient_ConnectionTerminated;
            m_udpClient.ReceiveDataComplete += UdpClient_ReceiveDataComplete;
            m_udpClient.ConnectionEstablished += UdpClient_ConnectionEstablished;
            m_udpClient.Connect();
            m_udpServer.Start();
        }
        
        private void UdpClient_ConnectionEstablished(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus("Connection successful.\r\n\r\n");
        }


        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            m_udpClient.Disconnect();
            m_udpServer.Stop();
        } 

        private void UdpClient_ReceiveDataComplete(object sender, TVA.EventArgs<byte[], int> e)
        {
            try
            {
                // Multicast received data to all clients.
                m_udpServer.MulticastAsync(e.Argument1, 0, e.Argument2);
            }
            catch (Exception ex)
            {
                m_serviceHelper.ErrorLogger.Log(ex);

                m_serviceHelper.UpdateStatus("Data Stream interrupted.\r\n\r\n");
            }
        }

        private void UdpClient_ConnectionTerminated(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus("Connection terminated.\r\n\r\n");
        }

        private void UdpClient_ConnectionException(object sender, EventArgs<Exception> e)
        {
            m_serviceHelper.UpdateStatus("A connection exception has occurred: {0}.\r\n\r\n", e.Argument.Message);
        }
       
        private void UdpClient_ConnectionAttempt(object sender, EventArgs e)
        {
            m_serviceHelper.UpdateStatus("Attempting connection...\r\n\r\n");
        }

        #endregion
    }
}
