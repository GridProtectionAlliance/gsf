using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

namespace TVA.Windows
{
	namespace UI
	{
		
		[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]public partial class AboutDialog : System.Windows.Forms.Form
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
				this.TabControlInformation = new System.Windows.Forms.TabControl();
				base.Load += new System.EventHandler(AboutDialog_Load);
				this.TabPageDisclaimer = new System.Windows.Forms.TabPage();
				this.RichTextBoxDisclaimer = new System.Windows.Forms.RichTextBox();
				this.RichTextBoxDisclaimer.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(RichTextBoxDisclaimer_LinkClicked);
				this.TabPageApplication = new System.Windows.Forms.TabPage();
				this.ListViewApplicationInfo = new System.Windows.Forms.ListView();
				this.ColumnHeaderApplicationKey = new System.Windows.Forms.ColumnHeader();
				this.ColumnHeaderApplicationValue = new System.Windows.Forms.ColumnHeader();
				this.TabPageAssemblies = new System.Windows.Forms.TabPage();
				this.ListViewAssemblyInfo = new System.Windows.Forms.ListView();
				this.ColumnHeaderAssemblyKey = new System.Windows.Forms.ColumnHeader();
				this.ColumnHeaderAssemblyValue = new System.Windows.Forms.ColumnHeader();
				this.ComboBoxAssemblies = new System.Windows.Forms.ComboBox();
				this.ComboBoxAssemblies.SelectedIndexChanged += new System.EventHandler(ComboBoxAssemblies_SelectedIndexChanged);
				this.ButtonOK = new System.Windows.Forms.Button();
				this.ButtonOK.Click += new System.EventHandler(ButtonOK_Click);
				this.PictureBoxLogo = new System.Windows.Forms.PictureBox();
				this.PictureBoxLogo.Click += new System.EventHandler(PictureBoxLogo_Click);
				this.TabControlInformation.SuspendLayout();
				this.TabPageDisclaimer.SuspendLayout();
				this.TabPageApplication.SuspendLayout();
				this.TabPageAssemblies.SuspendLayout();
				((System.ComponentModel.ISupportInitialize) this.PictureBoxLogo).BeginInit();
				this.SuspendLayout();
				//
				//TabControlInformation
				//
				this.TabControlInformation.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
				this.TabControlInformation.Controls.Add(this.TabPageDisclaimer);
				this.TabControlInformation.Controls.Add(this.TabPageApplication);
				this.TabControlInformation.Controls.Add(this.TabPageAssemblies);
				this.TabControlInformation.Location = new System.Drawing.Point(12, 68);
				this.TabControlInformation.Name = "TabControlInformation";
				this.TabControlInformation.SelectedIndex = 0;
				this.TabControlInformation.Size = new System.Drawing.Size(410, 253);
				this.TabControlInformation.TabIndex = 2;
				//
				//TabPageDisclaimer
				//
				this.TabPageDisclaimer.Controls.Add(this.RichTextBoxDisclaimer);
				this.TabPageDisclaimer.Location = new System.Drawing.Point(4, 22);
				this.TabPageDisclaimer.Name = "TabPageDisclaimer";
				this.TabPageDisclaimer.Padding = new System.Windows.Forms.Padding(3);
				this.TabPageDisclaimer.Size = new System.Drawing.Size(402, 227);
				this.TabPageDisclaimer.TabIndex = 0;
				this.TabPageDisclaimer.Text = "Disclaimer";
				this.TabPageDisclaimer.UseVisualStyleBackColor = true;
				//
				//RichTextBoxDisclaimer
				//
				this.RichTextBoxDisclaimer.Dock = System.Windows.Forms.DockStyle.Fill;
				this.RichTextBoxDisclaimer.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Bold);
				this.RichTextBoxDisclaimer.Location = new System.Drawing.Point(3, 3);
				this.RichTextBoxDisclaimer.Name = "RichTextBoxDisclaimer";
				this.RichTextBoxDisclaimer.ReadOnly = true;
				this.RichTextBoxDisclaimer.Size = new System.Drawing.Size(396, 221);
				this.RichTextBoxDisclaimer.TabIndex = 0;
				this.RichTextBoxDisclaimer.Text = "";
				//
				//TabPageApplication
				//
				this.TabPageApplication.Controls.Add(this.ListViewApplicationInfo);
				this.TabPageApplication.Location = new System.Drawing.Point(4, 22);
				this.TabPageApplication.Name = "TabPageApplication";
				this.TabPageApplication.Padding = new System.Windows.Forms.Padding(3);
				this.TabPageApplication.Size = new System.Drawing.Size(402, 227);
				this.TabPageApplication.TabIndex = 1;
				this.TabPageApplication.Text = "Application";
				this.TabPageApplication.UseVisualStyleBackColor = true;
				//
				//ListViewApplicationInfo
				//
				this.ListViewApplicationInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {this.ColumnHeaderApplicationKey, this.ColumnHeaderApplicationValue});
				this.ListViewApplicationInfo.Dock = System.Windows.Forms.DockStyle.Fill;
				this.ListViewApplicationInfo.FullRowSelect = true;
				this.ListViewApplicationInfo.Location = new System.Drawing.Point(3, 3);
				this.ListViewApplicationInfo.Name = "ListViewApplicationInfo";
				this.ListViewApplicationInfo.Size = new System.Drawing.Size(396, 221);
				this.ListViewApplicationInfo.TabIndex = 0;
				this.ListViewApplicationInfo.UseCompatibleStateImageBehavior = false;
				this.ListViewApplicationInfo.View = System.Windows.Forms.View.Details;
				//
				//ColumnHeaderApplicationKey
				//
				this.ColumnHeaderApplicationKey.Text = "Key";
				this.ColumnHeaderApplicationKey.Width = 110;
				//
				//ColumnHeaderApplicationValue
				//
				this.ColumnHeaderApplicationValue.Text = "Value";
				this.ColumnHeaderApplicationValue.Width = 260;
				//
				//TabPageAssemblies
				//
				this.TabPageAssemblies.Controls.Add(this.ListViewAssemblyInfo);
				this.TabPageAssemblies.Controls.Add(this.ComboBoxAssemblies);
				this.TabPageAssemblies.Location = new System.Drawing.Point(4, 22);
				this.TabPageAssemblies.Name = "TabPageAssemblies";
				this.TabPageAssemblies.Size = new System.Drawing.Size(402, 227);
				this.TabPageAssemblies.TabIndex = 2;
				this.TabPageAssemblies.Text = "Assemblies";
				this.TabPageAssemblies.UseVisualStyleBackColor = true;
				//
				//ListViewAssemblyInfo
				//
				this.ListViewAssemblyInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {this.ColumnHeaderAssemblyKey, this.ColumnHeaderAssemblyValue});
				this.ListViewAssemblyInfo.Dock = System.Windows.Forms.DockStyle.Fill;
				this.ListViewAssemblyInfo.FullRowSelect = true;
				this.ListViewAssemblyInfo.Location = new System.Drawing.Point(0, 21);
				this.ListViewAssemblyInfo.Name = "ListViewAssemblyInfo";
				this.ListViewAssemblyInfo.Size = new System.Drawing.Size(402, 206);
				this.ListViewAssemblyInfo.TabIndex = 1;
				this.ListViewAssemblyInfo.UseCompatibleStateImageBehavior = false;
				this.ListViewAssemblyInfo.View = System.Windows.Forms.View.Details;
				//
				//ColumnHeaderAssemblyKey
				//
				this.ColumnHeaderAssemblyKey.Text = "Key";
				this.ColumnHeaderAssemblyKey.Width = 110;
				//
				//ColumnHeaderAssemblyValue
				//
				this.ColumnHeaderAssemblyValue.Text = "Value";
				this.ColumnHeaderAssemblyValue.Width = 260;
				//
				//ComboBoxAssemblies
				//
				this.ComboBoxAssemblies.Dock = System.Windows.Forms.DockStyle.Top;
				this.ComboBoxAssemblies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
				this.ComboBoxAssemblies.FormattingEnabled = true;
				this.ComboBoxAssemblies.Location = new System.Drawing.Point(0, 0);
				this.ComboBoxAssemblies.Name = "ComboBoxAssemblies";
				this.ComboBoxAssemblies.Size = new System.Drawing.Size(402, 21);
				this.ComboBoxAssemblies.TabIndex = 0;
				//
				//ButtonOK
				//
				this.ButtonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
				this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.ButtonOK.Location = new System.Drawing.Point(181, 333);
				this.ButtonOK.Name = "ButtonOK";
				this.ButtonOK.Size = new System.Drawing.Size(75, 23);
				this.ButtonOK.TabIndex = 3;
				this.ButtonOK.Text = "OK";
				this.ButtonOK.UseVisualStyleBackColor = true;
				//
				//PictureBoxLogo
				//
				this.PictureBoxLogo.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
				this.PictureBoxLogo.BackColor = System.Drawing.Color.Transparent;
				this.PictureBoxLogo.Cursor = System.Windows.Forms.Cursors.Hand;
				this.PictureBoxLogo.Location = new System.Drawing.Point(12, 12);
				this.PictureBoxLogo.Name = "PictureBoxLogo";
				this.PictureBoxLogo.Size = new System.Drawing.Size(410, 50);
				this.PictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
				this.PictureBoxLogo.TabIndex = 4;
				this.PictureBoxLogo.TabStop = false;
				//
				//AboutDialog
				//
				this.AcceptButton = this.ButtonOK;
				this.AutoScaleDimensions = new System.Drawing.SizeF((float) 6.0, (float) 13.0);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.CancelButton = this.ButtonOK;
				this.ClientSize = new System.Drawing.Size(434, 368);
				this.Controls.Add(this.PictureBoxLogo);
				this.Controls.Add(this.ButtonOK);
				this.Controls.Add(this.TabControlInformation);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "AboutDialog";
				this.ShowInTaskbar = false;
				this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
				this.Text = "About {0}";
				this.TabControlInformation.ResumeLayout(false);
				this.TabPageDisclaimer.ResumeLayout(false);
				this.TabPageApplication.ResumeLayout(false);
				this.TabPageAssemblies.ResumeLayout(false);
				((System.ComponentModel.ISupportInitialize) this.PictureBoxLogo).EndInit();
				this.ResumeLayout(false);
				
			}
			internal System.Windows.Forms.TabControl TabControlInformation;
			internal System.Windows.Forms.TabPage TabPageDisclaimer;
			internal System.Windows.Forms.TabPage TabPageApplication;
			internal System.Windows.Forms.TabPage TabPageAssemblies;
			internal System.Windows.Forms.Button ButtonOK;
			internal System.Windows.Forms.RichTextBox RichTextBoxDisclaimer;
			internal System.Windows.Forms.ListView ListViewApplicationInfo;
			internal System.Windows.Forms.ColumnHeader ColumnHeaderApplicationKey;
			internal System.Windows.Forms.ColumnHeader ColumnHeaderApplicationValue;
			internal System.Windows.Forms.ListView ListViewAssemblyInfo;
			internal System.Windows.Forms.ColumnHeader ColumnHeaderAssemblyKey;
			internal System.Windows.Forms.ColumnHeader ColumnHeaderAssemblyValue;
			internal System.Windows.Forms.ComboBox ComboBoxAssemblies;
			internal System.Windows.Forms.PictureBox PictureBoxLogo;
		}
		
	}
	
}
