using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using TVA.Collections;

// 04-24-06


namespace TVA.TRO
{
	namespace Ssam
	{
		
		public partial class SsamLogger : System.ComponentModel.Component
		{
			
			
			[System.Diagnostics.DebuggerNonUserCode(), EditorBrowsable(EditorBrowsableState.Never)]public SsamLogger(System.ComponentModel.IContainer Container) : this()
			{
				
				//Required for Windows.Forms Class Composition Designer support
				Container.Add(this);
			}
			
			[System.Diagnostics.DebuggerNonUserCode(), EditorBrowsable(EditorBrowsableState.Never)]public SsamLogger() : this(SsamServer.Development, true)
			{
				
				
			}
			
			//Component overrides dispose to clean up the component list.
			[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
			{
				m_ssamApi.Dispose(); // Dispose the SSAM Api used for logging to SSAM.
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
