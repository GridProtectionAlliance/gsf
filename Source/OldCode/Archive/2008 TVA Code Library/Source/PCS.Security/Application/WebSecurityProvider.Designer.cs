using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 09-26-06

namespace PCS.Security
{
	namespace Application
	{
		
		public partial class WebSecurityProvider : PCS.Security.Application.SecurityProviderBase
		{
			
			
			[System.Diagnostics.DebuggerNonUserCode()]public WebSecurityProvider(System.ComponentModel.IContainer Container) : this()
			{
				
				//Required for Windows.Forms Class Composition Designer support
				Container.Add(this);
				
			}
			
			[System.Diagnostics.DebuggerNonUserCode()]public WebSecurityProvider()
			{
				
				//This call is required by the Component Designer.
				InitializeComponent();
				
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
