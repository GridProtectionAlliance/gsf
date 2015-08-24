using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace PCS.Security
{
	namespace Application
	{
		namespace Controls
		{
			
			
			[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]public partial class AccessDenied : System.Windows.Forms.Form
			{
				
				//Form overrides dispose to clean up the component list.
				[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
				{
					if (disposing && (components != null))
					{
						components.Dispose();
					}
					base.Dispose(disposing);
				}
				
				//Required by the Windows Form Designer
				private System.ComponentModel.Container components = null;
				
				//NOTE: The following procedure is required by the Windows Form Designer
				//It can be modified using the Windows Form Designer.
				//Do not modify it using the code editor.
				[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
				{
					System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessDenied));
					this.ButtonRequestAccess = new System.Windows.Forms.Button();
					base.Load += new System.EventHandler(AccessDenied_Load);
					this.ButtonRequestAccess.Click += new System.EventHandler(ButtonRequestAccess_Click);
					this.ButtonExitApplication = new System.Windows.Forms.Button();
					this.ButtonExitApplication.Click += new System.EventHandler(ButtonExitApplication_Click);
					this.Label1 = new System.Windows.Forms.Label();
					this.SuspendLayout();
					//
					//ButtonRequestAccess
					//
					this.ButtonRequestAccess.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
					this.ButtonRequestAccess.Location = new System.Drawing.Point(282, 83);
					this.ButtonRequestAccess.Name = "ButtonRequestAccess";
					this.ButtonRequestAccess.Size = new System.Drawing.Size(100, 23);
					this.ButtonRequestAccess.TabIndex = 1;
					this.ButtonRequestAccess.TabStop = false;
					this.ButtonRequestAccess.Text = "Request Access";
					this.ButtonRequestAccess.UseVisualStyleBackColor = true;
					//
					//ButtonExitApplication
					//
					this.ButtonExitApplication.Location = new System.Drawing.Point(12, 83);
					this.ButtonExitApplication.Name = "ButtonExitApplication";
					this.ButtonExitApplication.Size = new System.Drawing.Size(100, 23);
					this.ButtonExitApplication.TabIndex = 2;
					this.ButtonExitApplication.TabStop = false;
					this.ButtonExitApplication.Text = "Exit Application";
					this.ButtonExitApplication.UseVisualStyleBackColor = true;
					//
					//Label1
					//
					this.Label1.Location = new System.Drawing.Point(9, 9);
					this.Label1.Name = "Label1";
					this.Label1.Size = new System.Drawing.Size(373, 58);
					this.Label1.TabIndex = 0;
					this.Label1.Text = resources.GetString("Label1.Text");
					//
					//AccessDenied
					//
                    this.AutoScaleDimensions = new System.Drawing.SizeF((float)6.0, (float)13.0);
					this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
					this.ClientSize = new System.Drawing.Size(394, 118);
					this.ControlBox = false;
					this.Controls.Add(this.Label1);
					this.Controls.Add(this.ButtonExitApplication);
					this.Controls.Add(this.ButtonRequestAccess);
					this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
					this.Name = "AccessDenied";
					this.ShowInTaskbar = false;
					this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
					this.Text = "Access Denied";
					this.ResumeLayout(false);
					
				}
				internal System.Windows.Forms.Button ButtonRequestAccess;
				internal System.Windows.Forms.Button ButtonExitApplication;
				internal System.Windows.Forms.Label Label1;
			}
			
		}
	}
}
