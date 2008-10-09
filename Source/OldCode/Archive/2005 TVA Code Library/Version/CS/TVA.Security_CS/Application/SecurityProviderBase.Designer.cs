using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Configuration.Common;


namespace TVA.Security
{
	namespace Application
	{
		
		public partial class SecurityProviderBase : System.ComponentModel.Component
		{
			
			
			[System.Diagnostics.DebuggerNonUserCode()]public SecurityProviderBase(System.ComponentModel.IContainer Container) : this()
			{
				
				//Required for Windows.Forms Class Composition Designer support
				Container.Add(this);
				
			}
			
			[System.Diagnostics.DebuggerNonUserCode()]public SecurityProviderBase()
			{
				
				//This call is required by the Component Designer.
				InitializeComponent();
				
				m_settingsCategory = "SecurityProvider";
				m_extendeeControls = new Hashtable();
				
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
}
