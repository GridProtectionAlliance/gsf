using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace TVA.Services
{
	public partial class ClientHelper : System.ComponentModel.Component
	{
		
		
		[System.Diagnostics.DebuggerNonUserCode()]public ClientHelper(System.ComponentModel.IContainer Container) : this()
		{
			
			//Required for Windows.Forms Class Composition Designer support
			Container.Add(this);
			
		}
		
		[System.Diagnostics.DebuggerNonUserCode()]public ClientHelper()
		{
			
			//This call is required by the Component Designer.
			InitializeComponent();
			
			m_serviceName = DefaultServiceName;
			m_persistSettings = DefaultPersistSettings;
			m_settingsCategoryName = DefaultSettingsCategoryName;
			
		}
		
		//Component overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				// Since we don't have our communication client as a component, we must call the ClientHelper's disconnect
				// method when ClientHelper is disposed and this in-turn will cause the communication client to disconnect.
				Disconnect();
				SaveSettings(); // Saves settings to the config file.
				if (disposing && (components != null))
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
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
