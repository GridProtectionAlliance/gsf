using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Communication.TcpServer.Designer.vb - TCP-based communication server
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
//  06/02/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************

namespace TVA.Communication
{
	public partial class TcpServer : TVA.Communication.CommunicationServerBase
	{
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public TcpServer(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public TcpServer()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			// Setup the instance defaults.
			m_payloadAware = false;
			#if ThreadTracking
			m_listenerThread = new TVA.Threading.ManagedThread(ListenForConnections);
			m_listenerThread.Name = "TVA.Communication.TcpServer.ListenForConnections()";
			#else
			m_listenerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenForConnections));
			#endif
			m_tcpClients = new Dictionary<Guid, StateInfo<System.Net.Sockets.Socket>>;
			m_pendingTcpClients = new List<StateInfo<System.Net.Sockets.Socket>>;
			base.ConfigurationString = "Port=8888";
			base.Protocol = TransportProtocol.Tcp;
			
		}
		
		//Component overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
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
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		
	}
}
