//******************************************************************************************************
//  DataMigrationUtility.Designer.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/21/2012 - Starlynn Danyelle Gilliam
//       Generated original version of source code.
//
//******************************************************************************************************

namespace DataMigrationUtility
{
    partial class DataMigrationUtilityScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && ((object)components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataMigrationUtilityScreen));
            this.GroupBox = new System.Windows.Forms.GroupBox();
            this.ClearDestinationTables = new System.Windows.Forms.CheckBox();
            this.ExampleConnectionStringLinkLabel = new System.Windows.Forms.LinkLabel();
            this.ToConnectString = new System.Windows.Forms.TextBox();
            this.ToDataType = new System.Windows.Forms.ComboBox();
            this.ToDataTypeLabel = new System.Windows.Forms.Label();
            this.FromConnectString = new System.Windows.Forms.TextBox();
            this.FromDataType = new System.Windows.Forms.ComboBox();
            this.FromDataTypeLabel = new System.Windows.Forms.Label();
            this.LinkToTest = new System.Windows.Forms.LinkLabel();
            this.LinkFromTest = new System.Windows.Forms.LinkLabel();
            this.WarningLabelBold = new System.Windows.Forms.Label();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.UseToForRI = new System.Windows.Forms.RadioButton();
            this.UseFromForRI = new System.Windows.Forms.RadioButton();
            this.Cancel = new System.Windows.Forms.Button();
            this.Import = new System.Windows.Forms.Button();
            this.Version = new System.Windows.Forms.Label();
            this.ToConnectStringLabel = new System.Windows.Forms.Label();
            this.FromConnectStringLabel = new System.Windows.Forms.Label();
            this.ExcludedTablesTextBox = new System.Windows.Forms.TextBox();
            this.CommaSeparateValuesLabel = new System.Windows.Forms.Label();
            this.ExcludeTablesLabel = new System.Windows.Forms.Label();
            this.Messages = new System.Windows.Forms.TextBox();
            this.OverallProgress = new System.Windows.Forms.ProgressBar();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.PreserveAutoIncValues = new System.Windows.Forms.CheckBox();
            this.Serialize = new System.Windows.Forms.Button();
            this.GroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupBox
            // 
            this.GroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox.Controls.Add(this.ClearDestinationTables);
            this.GroupBox.Controls.Add(this.ExampleConnectionStringLinkLabel);
            this.GroupBox.Controls.Add(this.ToConnectString);
            this.GroupBox.Controls.Add(this.ToDataType);
            this.GroupBox.Controls.Add(this.ToDataTypeLabel);
            this.GroupBox.Controls.Add(this.FromConnectString);
            this.GroupBox.Controls.Add(this.FromDataType);
            this.GroupBox.Controls.Add(this.FromDataTypeLabel);
            this.GroupBox.Controls.Add(this.LinkToTest);
            this.GroupBox.Controls.Add(this.LinkFromTest);
            this.GroupBox.Controls.Add(this.WarningLabelBold);
            this.GroupBox.Controls.Add(this.WarningLabel);
            this.GroupBox.Controls.Add(this.UseToForRI);
            this.GroupBox.Controls.Add(this.UseFromForRI);
            this.GroupBox.Controls.Add(this.Cancel);
            this.GroupBox.Controls.Add(this.Import);
            this.GroupBox.Controls.Add(this.Version);
            this.GroupBox.Controls.Add(this.ToConnectStringLabel);
            this.GroupBox.Controls.Add(this.FromConnectStringLabel);
            this.GroupBox.Location = new System.Drawing.Point(12, 12);
            this.GroupBox.Name = "GroupBox";
            this.GroupBox.Size = new System.Drawing.Size(621, 202);
            this.GroupBox.TabIndex = 0;
            this.GroupBox.TabStop = false;
            // 
            // ClearDestinationTables
            // 
            this.ClearDestinationTables.AutoSize = true;
            this.ClearDestinationTables.Location = new System.Drawing.Point(181, 180);
            this.ClearDestinationTables.Name = "ClearDestinationTables";
            this.ClearDestinationTables.Size = new System.Drawing.Size(312, 17);
            this.ClearDestinationTables.TabIndex = 13;
            this.ClearDestinationTables.Text = "Delete existing data in destination database before migration.";
            this.ClearDestinationTables.UseVisualStyleBackColor = true;
            this.ClearDestinationTables.CheckedChanged += new System.EventHandler(this.ClearDestinationTables_CheckedChanged);
            // 
            // ExampleConnectionStringLinkLabel
            // 
            this.ExampleConnectionStringLinkLabel.AutoSize = true;
            this.ExampleConnectionStringLinkLabel.Location = new System.Drawing.Point(16, 181);
            this.ExampleConnectionStringLinkLabel.Name = "ExampleConnectionStringLinkLabel";
            this.ExampleConnectionStringLinkLabel.Size = new System.Drawing.Size(148, 13);
            this.ExampleConnectionStringLinkLabel.TabIndex = 12;
            this.ExampleConnectionStringLinkLabel.TabStop = true;
            this.ExampleConnectionStringLinkLabel.Text = "Example Connection Strings...";
            this.ExampleConnectionStringLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExampleConnectionStringLinkLabel_LinkClicked);
            // 
            // ToConnectString
            // 
            this.ToConnectString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToConnectString.Location = new System.Drawing.Point(16, 122);
            this.ToConnectString.Multiline = true;
            this.ToConnectString.Name = "ToConnectString";
            this.ToConnectString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ToConnectString.Size = new System.Drawing.Size(470, 56);
            this.ToConnectString.TabIndex = 11;
            // 
            // ToDataType
            // 
            this.ToDataType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToDataType.FormattingEnabled = true;
            this.ToDataType.Location = new System.Drawing.Point(314, 96);
            this.ToDataType.Name = "ToDataType";
            this.ToDataType.Size = new System.Drawing.Size(144, 21);
            this.ToDataType.TabIndex = 9;
            this.ToDataType.SelectedIndexChanged += new System.EventHandler(this.DataType_SelectedIndexChanged);
            // 
            // ToDataTypeLabel
            // 
            this.ToDataTypeLabel.BackColor = System.Drawing.SystemColors.Control;
            this.ToDataTypeLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ToDataTypeLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToDataTypeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ToDataTypeLabel.Location = new System.Drawing.Point(249, 98);
            this.ToDataTypeLabel.Name = "ToDataTypeLabel";
            this.ToDataTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ToDataTypeLabel.Size = new System.Drawing.Size(62, 18);
            this.ToDataTypeLabel.TabIndex = 8;
            this.ToDataTypeLabel.Text = "Data Type:";
            this.ToDataTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FromConnectString
            // 
            this.FromConnectString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FromConnectString.Location = new System.Drawing.Point(16, 37);
            this.FromConnectString.Multiline = true;
            this.FromConnectString.Name = "FromConnectString";
            this.FromConnectString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FromConnectString.Size = new System.Drawing.Size(470, 56);
            this.FromConnectString.TabIndex = 5;
            // 
            // FromDataType
            // 
            this.FromDataType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FromDataType.FormattingEnabled = true;
            this.FromDataType.Location = new System.Drawing.Point(314, 11);
            this.FromDataType.Name = "FromDataType";
            this.FromDataType.Size = new System.Drawing.Size(144, 21);
            this.FromDataType.TabIndex = 3;
            this.FromDataType.SelectedIndexChanged += new System.EventHandler(this.DataType_SelectedIndexChanged);
            // 
            // FromDataTypeLabel
            // 
            this.FromDataTypeLabel.BackColor = System.Drawing.SystemColors.Control;
            this.FromDataTypeLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.FromDataTypeLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FromDataTypeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FromDataTypeLabel.Location = new System.Drawing.Point(249, 13);
            this.FromDataTypeLabel.Name = "FromDataTypeLabel";
            this.FromDataTypeLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FromDataTypeLabel.Size = new System.Drawing.Size(62, 18);
            this.FromDataTypeLabel.TabIndex = 2;
            this.FromDataTypeLabel.Text = "Data Type:";
            this.FromDataTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LinkToTest
            // 
            this.LinkToTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkToTest.Location = new System.Drawing.Point(461, 99);
            this.LinkToTest.Name = "LinkToTest";
            this.LinkToTest.Size = new System.Drawing.Size(32, 16);
            this.LinkToTest.TabIndex = 10;
            this.LinkToTest.TabStop = true;
            this.LinkToTest.Text = "Test";
            this.LinkToTest.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkToTest_LinkClicked);
            // 
            // LinkFromTest
            // 
            this.LinkFromTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkFromTest.Location = new System.Drawing.Point(461, 14);
            this.LinkFromTest.Name = "LinkFromTest";
            this.LinkFromTest.Size = new System.Drawing.Size(32, 16);
            this.LinkFromTest.TabIndex = 4;
            this.LinkFromTest.TabStop = true;
            this.LinkFromTest.Text = "Test";
            this.LinkFromTest.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkFromTest_LinkClicked);
            // 
            // WarningLabelBold
            // 
            this.WarningLabelBold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WarningLabelBold.AutoSize = true;
            this.WarningLabelBold.Font = new System.Drawing.Font("Arial", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabelBold.ForeColor = System.Drawing.Color.DarkRed;
            this.WarningLabelBold.Location = new System.Drawing.Point(497, 104);
            this.WarningLabelBold.Name = "WarningLabelBold";
            this.WarningLabelBold.Size = new System.Drawing.Size(55, 12);
            this.WarningLabelBold.TabIndex = 17;
            this.WarningLabelBold.Text = "WARNING:";
            // 
            // WarningLabel
            // 
            this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WarningLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.Location = new System.Drawing.Point(497, 104);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(120, 74);
            this.WarningLabel.TabIndex = 17;
            this.WarningLabel.Text = "                  To maintain data integrity, ensure you have selected the correc" +
    "t source and destination databases.";
            // 
            // UseToForRI
            // 
            this.UseToForRI.Location = new System.Drawing.Point(125, 99);
            this.UseToForRI.Name = "UseToForRI";
            this.UseToForRI.Size = new System.Drawing.Size(130, 18);
            this.UseToForRI.TabIndex = 7;
            this.UseToForRI.Text = "Use for Ref. Integrity";
            // 
            // UseFromForRI
            // 
            this.UseFromForRI.Location = new System.Drawing.Point(125, 14);
            this.UseFromForRI.Name = "UseFromForRI";
            this.UseFromForRI.Size = new System.Drawing.Size(130, 18);
            this.UseFromForRI.TabIndex = 1;
            this.UseFromForRI.Text = "Use for Ref. Integrity";
            // 
            // Cancel
            // 
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.BackColor = System.Drawing.SystemColors.Control;
            this.Cancel.Cursor = System.Windows.Forms.Cursors.Default;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Cancel.Location = new System.Drawing.Point(500, 48);
            this.Cancel.Name = "Cancel";
            this.Cancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Cancel.Size = new System.Drawing.Size(105, 25);
            this.Cancel.TabIndex = 15;
            this.Cancel.Text = "E&xit";
            this.Cancel.UseVisualStyleBackColor = false;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Import
            // 
            this.Import.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Import.BackColor = System.Drawing.SystemColors.Control;
            this.Import.Cursor = System.Windows.Forms.Cursors.Default;
            this.Import.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Import.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Import.Location = new System.Drawing.Point(501, 17);
            this.Import.Name = "Import";
            this.Import.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Import.Size = new System.Drawing.Size(105, 25);
            this.Import.TabIndex = 14;
            this.Import.Text = "&Migrate";
            this.Import.UseVisualStyleBackColor = false;
            this.Import.Click += new System.EventHandler(this.Import_Click);
            // 
            // Version
            // 
            this.Version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Version.BackColor = System.Drawing.SystemColors.Control;
            this.Version.Cursor = System.Windows.Forms.Cursors.Default;
            this.Version.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Version.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Version.Location = new System.Drawing.Point(492, 78);
            this.Version.Name = "Version";
            this.Version.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Version.Size = new System.Drawing.Size(123, 13);
            this.Version.TabIndex = 16;
            this.Version.Text = "Version: x.x.x";
            this.Version.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ToConnectStringLabel
            // 
            this.ToConnectStringLabel.BackColor = System.Drawing.SystemColors.Control;
            this.ToConnectStringLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ToConnectStringLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToConnectStringLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ToConnectStringLabel.Location = new System.Drawing.Point(16, 99);
            this.ToConnectStringLabel.Name = "ToConnectStringLabel";
            this.ToConnectStringLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ToConnectStringLabel.Size = new System.Drawing.Size(117, 18);
            this.ToConnectStringLabel.TabIndex = 6;
            this.ToConnectStringLabel.Text = "&To Connect String:";
            this.ToConnectStringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FromConnectStringLabel
            // 
            this.FromConnectStringLabel.BackColor = System.Drawing.SystemColors.Control;
            this.FromConnectStringLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.FromConnectStringLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FromConnectStringLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FromConnectStringLabel.Location = new System.Drawing.Point(16, 14);
            this.FromConnectStringLabel.Name = "FromConnectStringLabel";
            this.FromConnectStringLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.FromConnectStringLabel.Size = new System.Drawing.Size(117, 18);
            this.FromConnectStringLabel.TabIndex = 0;
            this.FromConnectStringLabel.Text = "&From Connect String:";
            this.FromConnectStringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ExcludedTablesTextBox
            // 
            this.ExcludedTablesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExcludedTablesTextBox.Location = new System.Drawing.Point(95, 220);
            this.ExcludedTablesTextBox.Name = "ExcludedTablesTextBox";
            this.ExcludedTablesTextBox.Size = new System.Drawing.Size(535, 20);
            this.ExcludedTablesTextBox.TabIndex = 2;
            this.ExcludedTablesTextBox.Text = "Runtime,ErrorLog,AuditLog,AccessLog,AlarmLog,TrackedChange";
            // 
            // CommaSeparateValuesLabel
            // 
            this.CommaSeparateValuesLabel.BackColor = System.Drawing.SystemColors.Control;
            this.CommaSeparateValuesLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.CommaSeparateValuesLabel.Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommaSeparateValuesLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CommaSeparateValuesLabel.Location = new System.Drawing.Point(92, 236);
            this.CommaSeparateValuesLabel.Name = "CommaSeparateValuesLabel";
            this.CommaSeparateValuesLabel.Size = new System.Drawing.Size(518, 18);
            this.CommaSeparateValuesLabel.TabIndex = 6;
            this.CommaSeparateValuesLabel.Text = "Comma separate table names.";
            this.CommaSeparateValuesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ExcludeTablesLabel
            // 
            this.ExcludeTablesLabel.BackColor = System.Drawing.SystemColors.Control;
            this.ExcludeTablesLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ExcludeTablesLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExcludeTablesLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExcludeTablesLabel.Location = new System.Drawing.Point(9, 221);
            this.ExcludeTablesLabel.Name = "ExcludeTablesLabel";
            this.ExcludeTablesLabel.Size = new System.Drawing.Size(83, 18);
            this.ExcludeTablesLabel.TabIndex = 1;
            this.ExcludeTablesLabel.Text = "Exclude Tables:";
            this.ExcludeTablesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Messages
            // 
            this.Messages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Messages.Location = new System.Drawing.Point(9, 368);
            this.Messages.Multiline = true;
            this.Messages.Name = "Messages";
            this.Messages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Messages.Size = new System.Drawing.Size(621, 133);
            this.Messages.TabIndex = 5;
            this.Messages.Text = "Messages:";
            // 
            // OverallProgress
            // 
            this.OverallProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OverallProgress.Location = new System.Drawing.Point(9, 336);
            this.OverallProgress.Name = "OverallProgress";
            this.OverallProgress.Size = new System.Drawing.Size(621, 24);
            this.OverallProgress.TabIndex = 9;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(9, 304);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(621, 24);
            this.ProgressBar.TabIndex = 8;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressLabel.BackColor = System.Drawing.SystemColors.Control;
            this.ProgressLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.ProgressLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ProgressLabel.Location = new System.Drawing.Point(9, 285);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ProgressLabel.Size = new System.Drawing.Size(621, 15);
            this.ProgressLabel.TabIndex = 7;
            this.ProgressLabel.Text = "Progress:";
            // 
            // PreserveAutoIncValues
            // 
            this.PreserveAutoIncValues.AutoSize = true;
            this.PreserveAutoIncValues.Checked = true;
            this.PreserveAutoIncValues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PreserveAutoIncValues.Font = new System.Drawing.Font("Arial", 8F);
            this.PreserveAutoIncValues.Location = new System.Drawing.Point(12, 261);
            this.PreserveAutoIncValues.Name = "PreserveAutoIncValues";
            this.PreserveAutoIncValues.Size = new System.Drawing.Size(329, 18);
            this.PreserveAutoIncValues.TabIndex = 3;
            this.PreserveAutoIncValues.Text = "Preserve auto-increment field values while migrating database.";
            this.PreserveAutoIncValues.UseVisualStyleBackColor = true;
            // 
            // Serialize
            // 
            this.Serialize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Serialize.BackColor = System.Drawing.SystemColors.Control;
            this.Serialize.Cursor = System.Windows.Forms.Cursors.Default;
            this.Serialize.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Serialize.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Serialize.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Serialize.Location = new System.Drawing.Point(505, 257);
            this.Serialize.Name = "Serialize";
            this.Serialize.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Serialize.Size = new System.Drawing.Size(105, 25);
            this.Serialize.TabIndex = 4;
            this.Serialize.Text = "&Serialize";
            this.Serialize.UseVisualStyleBackColor = false;
            this.Serialize.Visible = false;
            this.Serialize.Click += new System.EventHandler(this.Serialize_Click);
            // 
            // DataMigrationUtilityScreen
            // 
            this.AcceptButton = this.Import;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(639, 507);
            this.Controls.Add(this.Serialize);
            this.Controls.Add(this.PreserveAutoIncValues);
            this.Controls.Add(this.ExcludedTablesTextBox);
            this.Controls.Add(this.CommaSeparateValuesLabel);
            this.Controls.Add(this.ExcludeTablesLabel);
            this.Controls.Add(this.Messages);
            this.Controls.Add(this.OverallProgress);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.GroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(655, 545);
            this.Name = "DataMigrationUtilityScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Data Migration Utility";
            this.Closed += new System.EventHandler(this.DataMigrationUtility_Closed);
            this.Load += new System.EventHandler(this.DataMigrationUtility_Load);
            this.GroupBox.ResumeLayout(false);
            this.GroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.GroupBox GroupBox;
        internal System.Windows.Forms.LinkLabel ExampleConnectionStringLinkLabel;
        internal System.Windows.Forms.TextBox ToConnectString;
        internal System.Windows.Forms.ComboBox ToDataType;
        internal System.Windows.Forms.Label ToDataTypeLabel;
        internal System.Windows.Forms.TextBox FromConnectString;
        internal System.Windows.Forms.ComboBox FromDataType;
        internal System.Windows.Forms.Label FromDataTypeLabel;
        internal System.Windows.Forms.LinkLabel LinkToTest;
        internal System.Windows.Forms.LinkLabel LinkFromTest;
        internal System.Windows.Forms.Label WarningLabelBold;
        internal System.Windows.Forms.Label WarningLabel;
        internal System.Windows.Forms.RadioButton UseToForRI;
        internal System.Windows.Forms.RadioButton UseFromForRI;
        internal System.Windows.Forms.Button Cancel;
        internal System.Windows.Forms.Button Import;
        internal System.Windows.Forms.Label Version;
        internal System.Windows.Forms.Label ToConnectStringLabel;
        internal System.Windows.Forms.Label FromConnectStringLabel;
        internal System.Windows.Forms.TextBox ExcludedTablesTextBox;
        internal System.Windows.Forms.Label CommaSeparateValuesLabel;
        internal System.Windows.Forms.Label ExcludeTablesLabel;
        internal System.Windows.Forms.TextBox Messages;
        internal System.Windows.Forms.ProgressBar OverallProgress;
        internal System.Windows.Forms.ProgressBar ProgressBar;
        internal System.Windows.Forms.Label ProgressLabel;
        internal System.Windows.Forms.CheckBox PreserveAutoIncValues;
        internal System.Windows.Forms.Button Serialize;
        private System.Windows.Forms.CheckBox ClearDestinationTables;
    }
}

