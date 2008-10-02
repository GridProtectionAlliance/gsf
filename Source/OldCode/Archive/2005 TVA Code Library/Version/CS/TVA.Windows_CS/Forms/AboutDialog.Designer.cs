//*******************************************************************************************************
//  AboutDialog.Designer.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2006 - Pinal C. Patel
//       Generated original version of source code.
//  10/01/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TVA.Windows.Forms
{
    public partial class AboutDialog : Form
    {
        //Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
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
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.TabControlInformation = new TabControl();
            base.Load += new System.EventHandler(AboutDialog_Load);
            this.TabPageDisclaimer = new TabPage();
            this.RichTextBoxDisclaimer = new RichTextBox();
            this.RichTextBoxDisclaimer.LinkClicked += new LinkClickedEventHandler(RichTextBoxDisclaimer_LinkClicked);
            this.TabPageApplication = new TabPage();
            this.ListViewApplicationInfo = new ListView();
            this.ColumnHeaderApplicationKey = new ColumnHeader();
            this.ColumnHeaderApplicationValue = new ColumnHeader();
            this.TabPageAssemblies = new TabPage();
            this.ListViewAssemblyInfo = new ListView();
            this.ColumnHeaderAssemblyKey = new ColumnHeader();
            this.ColumnHeaderAssemblyValue = new ColumnHeader();
            this.ComboBoxAssemblies = new ComboBox();
            this.ComboBoxAssemblies.SelectedIndexChanged += new System.EventHandler(ComboBoxAssemblies_SelectedIndexChanged);
            this.ButtonOK = new Button();
            this.ButtonOK.Click += new System.EventHandler(ButtonOK_Click);
            this.PictureBoxLogo = new PictureBox();
            this.PictureBoxLogo.Click += new System.EventHandler(PictureBoxLogo_Click);
            this.TabControlInformation.SuspendLayout();
            this.TabPageDisclaimer.SuspendLayout();
            this.TabPageApplication.SuspendLayout();
            this.TabPageAssemblies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.PictureBoxLogo).BeginInit();
            this.SuspendLayout();
            //
            //TabControlInformation
            //
            this.TabControlInformation.Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right);
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
            this.TabPageDisclaimer.Padding = new Padding(3);
            this.TabPageDisclaimer.Size = new System.Drawing.Size(402, 227);
            this.TabPageDisclaimer.TabIndex = 0;
            this.TabPageDisclaimer.Text = "Disclaimer";
            this.TabPageDisclaimer.UseVisualStyleBackColor = true;
            //
            //RichTextBoxDisclaimer
            //
            this.RichTextBoxDisclaimer.Dock = DockStyle.Fill;
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
            this.TabPageApplication.Padding = new Padding(3);
            this.TabPageApplication.Size = new System.Drawing.Size(402, 227);
            this.TabPageApplication.TabIndex = 1;
            this.TabPageApplication.Text = "Application";
            this.TabPageApplication.UseVisualStyleBackColor = true;
            //
            //ListViewApplicationInfo
            //
            this.ListViewApplicationInfo.Columns.AddRange(new ColumnHeader[] { this.ColumnHeaderApplicationKey, this.ColumnHeaderApplicationValue });
            this.ListViewApplicationInfo.Dock = DockStyle.Fill;
            this.ListViewApplicationInfo.FullRowSelect = true;
            this.ListViewApplicationInfo.Location = new System.Drawing.Point(3, 3);
            this.ListViewApplicationInfo.Name = "ListViewApplicationInfo";
            this.ListViewApplicationInfo.Size = new System.Drawing.Size(396, 221);
            this.ListViewApplicationInfo.TabIndex = 0;
            this.ListViewApplicationInfo.UseCompatibleStateImageBehavior = false;
            this.ListViewApplicationInfo.View = View.Details;
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
            this.ListViewAssemblyInfo.Columns.AddRange(new ColumnHeader[] { this.ColumnHeaderAssemblyKey, this.ColumnHeaderAssemblyValue });
            this.ListViewAssemblyInfo.Dock = DockStyle.Fill;
            this.ListViewAssemblyInfo.FullRowSelect = true;
            this.ListViewAssemblyInfo.Location = new System.Drawing.Point(0, 21);
            this.ListViewAssemblyInfo.Name = "ListViewAssemblyInfo";
            this.ListViewAssemblyInfo.Size = new System.Drawing.Size(402, 206);
            this.ListViewAssemblyInfo.TabIndex = 1;
            this.ListViewAssemblyInfo.UseCompatibleStateImageBehavior = false;
            this.ListViewAssemblyInfo.View = View.Details;
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
            this.ComboBoxAssemblies.Dock = DockStyle.Top;
            this.ComboBoxAssemblies.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ComboBoxAssemblies.FormattingEnabled = true;
            this.ComboBoxAssemblies.Location = new System.Drawing.Point(0, 0);
            this.ComboBoxAssemblies.Name = "ComboBoxAssemblies";
            this.ComboBoxAssemblies.Size = new System.Drawing.Size(402, 21);
            this.ComboBoxAssemblies.TabIndex = 0;
            //
            //ButtonOK
            //
            this.ButtonOK.Anchor = AnchorStyles.Bottom;
            this.ButtonOK.DialogResult = DialogResult.Cancel;
            this.ButtonOK.Location = new System.Drawing.Point(181, 333);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(75, 23);
            this.ButtonOK.TabIndex = 3;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            //
            //PictureBoxLogo
            //
            this.PictureBoxLogo.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
            this.PictureBoxLogo.BackColor = System.Drawing.Color.Transparent;
            this.PictureBoxLogo.Cursor = Cursors.Hand;
            this.PictureBoxLogo.Location = new System.Drawing.Point(12, 12);
            this.PictureBoxLogo.Name = "PictureBoxLogo";
            this.PictureBoxLogo.Size = new System.Drawing.Size(410, 50);
            this.PictureBoxLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            this.PictureBoxLogo.TabIndex = 4;
            this.PictureBoxLogo.TabStop = false;
            //
            //AboutDialog
            //
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF((float)6.0, (float)13.0);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.ButtonOK;
            this.ClientSize = new System.Drawing.Size(434, 368);
            this.Controls.Add(this.PictureBoxLogo);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.TabControlInformation);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About {0}";
            this.TabControlInformation.ResumeLayout(false);
            this.TabPageDisclaimer.ResumeLayout(false);
            this.TabPageApplication.ResumeLayout(false);
            this.TabPageAssemblies.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.PictureBoxLogo).EndInit();
            this.ResumeLayout(false);

        }

        internal TabControl TabControlInformation;
        internal TabPage TabPageDisclaimer;
        internal TabPage TabPageApplication;
        internal TabPage TabPageAssemblies;
        internal Button ButtonOK;
        internal RichTextBox RichTextBoxDisclaimer;
        internal ListView ListViewApplicationInfo;
        internal ColumnHeader ColumnHeaderApplicationKey;
        internal ColumnHeader ColumnHeaderApplicationValue;
        internal ListView ListViewAssemblyInfo;
        internal ColumnHeader ColumnHeaderAssemblyKey;
        internal ColumnHeader ColumnHeaderAssemblyValue;
        internal ComboBox ComboBoxAssemblies;
        internal PictureBox PictureBoxLogo;
    }
}