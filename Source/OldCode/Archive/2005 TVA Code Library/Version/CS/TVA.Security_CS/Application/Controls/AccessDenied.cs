using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Security.Application.Controls.AccessDenied.vb - Access denied windows dialog box
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
//  10/16/2006 - Pinal C. Patel
//       Original version of source code generated.
//  05/06/2008 - Pinal C. Patel
//       Moved from TVA.Security.Application namespace to TVA.Security.Application.Controls namespace.
//
//*******************************************************************************************************

namespace TVA.Security
{
	namespace Application
	{
		namespace Controls
		{
			
			
			public partial class AccessDenied
			{
				public AccessDenied()
				{
					InitializeComponent();
				}
				
				private void AccessDenied_Load(System.Object sender, System.EventArgs e)
				{
					
					if (this.Owner != null)
					{
						this.Font = this.Owner.Font;
						this.Text = System.Windows.Forms.Application.ProductName + " - " + this.Text;
					}
					
				}
				
				private void ButtonExitApplication_Click(System.Object sender, System.EventArgs e)
				{
					
					this.DialogResult = System.Windows.Forms.DialogResult.No;
					this.Close();
					
				}
				
				private void ButtonRequestAccess_Click(System.Object sender, System.EventArgs e)
				{
					
					this.DialogResult = System.Windows.Forms.DialogResult.Yes;
					this.Close();
					
				}
				
			}
			
		}
	}
}
