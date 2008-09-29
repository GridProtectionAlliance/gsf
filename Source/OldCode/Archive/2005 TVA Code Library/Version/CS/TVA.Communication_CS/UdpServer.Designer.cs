using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using TVA.Communication.Common;

//*******************************************************************************************************
//  TVA.Communication.UdpServer.Designer.vb - UDP-based communication server
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.Communication
{
    public partial class UdpServer : TVA.Communication.CommunicationServerBase
    {


        [System.Diagnostics.DebuggerNonUserCode()]
        public UdpServer(System.ComponentModel.IContainer Container)
            : this()
        {

            //Required for Windows.Forms Class Composition Designer support
            Container.Add(this);

        }

        [System.Diagnostics.DebuggerNonUserCode()]
        public UdpServer()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			m_payloadAware = false;
			m_destinationReachableCheck = false;
			m_udpClients = new Dictionary<Guid, StateInfo<System.Net.IPEndPoint>>;
			base.ConfigurationString = "Port=8888; Clients=255.255.255.255:8888";
			base.Protocol = TransportProtocol.Udp;
			base.ReceiveBufferSize = MaximumUdpDatagramSize;
			
		}

        //Component overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        //Required by the Component Designer
        private System.ComponentModel.Container components = null;

        //NOTE: The following procedure is required by the Component Designer
        //It can be modified using the Component Designer.
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

    }

}
