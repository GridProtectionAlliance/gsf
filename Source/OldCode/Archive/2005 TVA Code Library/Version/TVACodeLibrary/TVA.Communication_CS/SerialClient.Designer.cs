using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 07-27-06

namespace TVA.Communication
{
	public partial class SerialClient : TVA.Communication.CommunicationClientBase
	{
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public SerialClient(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public SerialClient()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			#if ThreadTracking
			m_connectionThread = new TVA.Threading.ManagedThread(ConnectToPort);
			m_connectionThread.Name = "TVA.Communication.SerialClient.ConnectToPort()";
			#else
			m_connectionThread = new System.Threading.Thread(new System.Threading.ThreadStart(ConnectToPort));
			#endif
			m_serialClient = new System.IO.Ports.SerialPort();
			m_serialClient.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(m_serialClient_DataReceived);
			base.ConnectionString = "Port=COM1; BaudRate=9600; Parity=None; StopBits=One; DataBits=8; DtrEnable=False; RtsEnable=False";
			base.Protocol = TransportProtocol.Serial;
			
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
