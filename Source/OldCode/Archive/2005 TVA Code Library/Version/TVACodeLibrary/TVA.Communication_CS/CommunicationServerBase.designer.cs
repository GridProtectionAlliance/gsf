using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;

//*******************************************************************************************************
//  TVA.Communication.ServerBase.Designer.vb - Base functionality of a server for transporting data
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
//  06/01/2006 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace TVA.Communication
{
	public partial class CommunicationServerBase : System.ComponentModel.Component
	{
		
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public CommunicationServerBase(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public CommunicationServerBase()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			// Setup the default values.
			m_receiveBufferSize = 8192;
			m_maximumClients = - 1;
			m_handshake = true;
			m_encryption = TVA.Security.Cryptography.EncryptLevel.None;
			m_compression = TVA.IO.Compression.CompressLevel.NoCompression;
			m_crcCheck = CRCCheckType.None;
			m_enabled = true;
			m_textEncoding = System.Text.Encoding.ASCII;
			m_serverID = Guid.NewGuid(); // Create an ID for the server.
			m_clientIDs = new List<Guid>;
			m_settingsCategoryName = this.GetType().Name;
			
			m_startTime = 0;
			m_stopTime = 0;
			m_buffer = TVA.Common.CreateArray<byte>(m_receiveBufferSize);
			
		}
		
		//Component overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			if (! m_disposed)
			{
				@Stop(); // Stop the server.
				SaveSettings(); // Saves settings to the config file.
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				base.Dispose(disposing);
			}
			m_disposed = true;
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
