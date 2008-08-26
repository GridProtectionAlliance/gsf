using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using TVA.Communication.Common;

//*******************************************************************************************************
//  TVA.Communication.UdpClient.Designer.vb - UDP-based communication client
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
	public partial class UdpClient : TVA.Communication.CommunicationClientBase
	{
		
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public UdpClient(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public UdpClient()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			m_payloadAware = false;
			m_destinationReachableCheck = false;
			#if ThreadTracking
			m_receivingThread = new TVA.Threading.ManagedThread(ReceiveServerData);
			m_receivingThread.Name = "TVA.Communication.UdpClient.ReceiveServerData()";
			
			m_connectionThread = new TVA.Threading.ManagedThread(ConnectToServer);
			m_connectionThread.Name = "TVA.Communication.UdpClient.ConnectToServer()";
			#else
			m_receivingThread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveServerData));
			m_connectionThread = new System.Threading.Thread(new System.Threading.ThreadStart(ConnectToServer));
			#endif
			base.ConnectionString = "Server=localhost; RemotePort=8888; LocalPort=8888";
			base.Protocol = TransportProtocol.Udp;
			base.ReceiveBufferSize = MaximumUdpDatagramSize;
			
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
