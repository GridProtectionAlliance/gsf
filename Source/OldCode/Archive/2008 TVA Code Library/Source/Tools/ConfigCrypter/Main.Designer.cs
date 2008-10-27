using System.Diagnostics;
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace ConfigCrypter
{
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]public partial class Main : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
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
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.GroupBoxDirection = new System.Windows.Forms.GroupBox();
			base.Load += new System.EventHandler(Main_Load);
			this.RadioButtonDecrypt = new System.Windows.Forms.RadioButton();
			this.RadioButtonEncrypt = new System.Windows.Forms.RadioButton();
			this.RadioButtonEncrypt.CheckedChanged += new System.EventHandler(RadioButtonEncrypt_CheckedChanged);
			this.RadioButtonEncrypt.CheckedChanged += new System.EventHandler(RadioButtonEncrypt_CheckedChanged);
			this.GroupBoxConfiguration = new System.Windows.Forms.GroupBox();
			this.LinkLabelCopy = new System.Windows.Forms.LinkLabel();
			this.LinkLabelCopy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkLabelCopy_LinkClicked);
			this.TextBoxOutput = new System.Windows.Forms.TextBox();
			this.LabelOutput = new System.Windows.Forms.Label();
			this.TextBoxInput = new System.Windows.Forms.TextBox();
			this.TextBoxInput.TextChanged += new System.EventHandler(TextBoxInput_TextChanged);
			this.LabelInput = new System.Windows.Forms.Label();
			this.GroupBoxDirection.SuspendLayout();
			this.GroupBoxConfiguration.SuspendLayout();
			this.SuspendLayout();
			//
			//GroupBoxDirection
			//
			this.GroupBoxDirection.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.GroupBoxDirection.Controls.Add(this.RadioButtonDecrypt);
			this.GroupBoxDirection.Controls.Add(this.RadioButtonEncrypt);
			this.GroupBoxDirection.Location = new System.Drawing.Point(12, 12);
			this.GroupBoxDirection.Name = "GroupBoxDirection";
			this.GroupBoxDirection.Size = new System.Drawing.Size(268, 49);
			this.GroupBoxDirection.TabIndex = 0;
			this.GroupBoxDirection.TabStop = false;
			this.GroupBoxDirection.Text = "Direction";
			//
			//RadioButtonDecrypt
			//
			this.RadioButtonDecrypt.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.RadioButtonDecrypt.AutoSize = true;
			this.RadioButtonDecrypt.Location = new System.Drawing.Point(136, 20);
			this.RadioButtonDecrypt.Name = "RadioButtonDecrypt";
			this.RadioButtonDecrypt.Size = new System.Drawing.Size(126, 17);
			this.RadioButtonDecrypt.TabIndex = 1;
			this.RadioButtonDecrypt.Text = "Decrypt Config Value";
			this.RadioButtonDecrypt.UseVisualStyleBackColor = true;
			//
			//RadioButtonEncrypt
			//
			this.RadioButtonEncrypt.AutoSize = true;
			this.RadioButtonEncrypt.Checked = true;
			this.RadioButtonEncrypt.Location = new System.Drawing.Point(6, 20);
			this.RadioButtonEncrypt.Name = "RadioButtonEncrypt";
			this.RadioButtonEncrypt.Size = new System.Drawing.Size(125, 17);
			this.RadioButtonEncrypt.TabIndex = 0;
			this.RadioButtonEncrypt.TabStop = true;
			this.RadioButtonEncrypt.Text = "Encrypt Config Value";
			this.RadioButtonEncrypt.UseVisualStyleBackColor = true;
			//
			//GroupBoxConfiguration
			//
			this.GroupBoxConfiguration.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.GroupBoxConfiguration.Controls.Add(this.LinkLabelCopy);
			this.GroupBoxConfiguration.Controls.Add(this.TextBoxOutput);
			this.GroupBoxConfiguration.Controls.Add(this.LabelOutput);
			this.GroupBoxConfiguration.Controls.Add(this.TextBoxInput);
			this.GroupBoxConfiguration.Controls.Add(this.LabelInput);
			this.GroupBoxConfiguration.Location = new System.Drawing.Point(12, 67);
			this.GroupBoxConfiguration.Name = "GroupBoxConfiguration";
			this.GroupBoxConfiguration.Size = new System.Drawing.Size(268, 140);
			this.GroupBoxConfiguration.TabIndex = 1;
			this.GroupBoxConfiguration.TabStop = false;
			this.GroupBoxConfiguration.Text = "Config Value";
			//
			//LinkLabelCopy
			//
			this.LinkLabelCopy.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.LinkLabelCopy.AutoSize = true;
			this.LinkLabelCopy.Location = new System.Drawing.Point(169, 66);
			this.LinkLabelCopy.Name = "LinkLabelCopy";
			this.LinkLabelCopy.Size = new System.Drawing.Size(93, 13);
			this.LinkLabelCopy.TabIndex = 4;
			this.LinkLabelCopy.TabStop = true;
			this.LinkLabelCopy.Text = "Copy to Clipboard";
			//
			//TextBoxOutput
			//
			this.TextBoxOutput.Location = new System.Drawing.Point(9, 82);
			this.TextBoxOutput.Multiline = true;
			this.TextBoxOutput.Name = "TextBoxOutput";
			this.TextBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBoxOutput.Size = new System.Drawing.Size(253, 51);
			this.TextBoxOutput.TabIndex = 3;
			//
			//LabelOutput
			//
			this.LabelOutput.AutoSize = true;
			this.LabelOutput.Location = new System.Drawing.Point(6, 66);
			this.LabelOutput.Name = "LabelOutput";
			this.LabelOutput.Size = new System.Drawing.Size(45, 13);
			this.LabelOutput.TabIndex = 2;
			this.LabelOutput.Text = "Output:";
			//
			//TextBoxInput
			//
			this.TextBoxInput.Location = new System.Drawing.Point(9, 33);
			this.TextBoxInput.Name = "TextBoxInput";
			this.TextBoxInput.Size = new System.Drawing.Size(253, 21);
			this.TextBoxInput.TabIndex = 1;
			//
			//LabelInput
			//
			this.LabelInput.AutoSize = true;
			this.LabelInput.Location = new System.Drawing.Point(6, 17);
			this.LabelInput.Name = "LabelInput";
			this.LabelInput.Size = new System.Drawing.Size(37, 13);
			this.LabelInput.TabIndex = 0;
			this.LabelInput.Text = "Input:";
			//
			//Main
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) 6.0, (float) 13.0);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 216);
			this.Controls.Add(this.GroupBoxConfiguration);
			this.Controls.Add(this.GroupBoxDirection);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "{0} v{1}";
			this.GroupBoxDirection.ResumeLayout(false);
			this.GroupBoxDirection.PerformLayout();
			this.GroupBoxConfiguration.ResumeLayout(false);
			this.GroupBoxConfiguration.PerformLayout();
			this.ResumeLayout(false);
			
		}
		internal System.Windows.Forms.GroupBox GroupBoxDirection;
		internal System.Windows.Forms.RadioButton RadioButtonDecrypt;
		internal System.Windows.Forms.RadioButton RadioButtonEncrypt;
		internal System.Windows.Forms.GroupBox GroupBoxConfiguration;
		internal System.Windows.Forms.TextBox TextBoxOutput;
		internal System.Windows.Forms.Label LabelOutput;
		internal System.Windows.Forms.TextBox TextBoxInput;
		internal System.Windows.Forms.Label LabelInput;
		internal System.Windows.Forms.LinkLabel LinkLabelCopy;
	}
	
}
