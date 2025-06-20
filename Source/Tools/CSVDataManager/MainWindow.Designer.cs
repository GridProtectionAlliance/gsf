﻿namespace CSVDataManager
{
    partial class MainWindow
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
            if (disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ExportTabPage = new System.Windows.Forms.TabPage();
            this.ExportFieldsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ExportTopBarPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ExportTableComboBox = new System.Windows.Forms.ComboBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ExportCountLabel = new System.Windows.Forms.Label();
            this.ExportProgressBar = new System.Windows.Forms.ProgressBar();
            this.ImportTabPage = new System.Windows.Forms.TabPage();
            this.ImportActionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.InsertButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.ImportTopBarPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ImportTableComboBox = new System.Windows.Forms.ComboBox();
            this.ImportCountLabel = new System.Windows.Forms.Label();
            this.ImportProgressBar = new System.Windows.Forms.ProgressBar();
            this.ConfigurationTabPage = new System.Windows.Forms.TabPage();
            this.ConfigurationPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ConnectionStringLabel = new System.Windows.Forms.Label();
            this.ConnectionStringTextBox = new System.Windows.Forms.TextBox();
            this.DataProviderLabel = new System.Windows.Forms.Label();
            this.DataProviderComboBox = new System.Windows.Forms.ComboBox();
            this.DataProviderTextBox = new System.Windows.Forms.TextBox();
            this.SerializedSchemaLabel = new System.Windows.Forms.Label();
            this.SerializedSchemaConfigurationPanel = new System.Windows.Forms.Panel();
            this.SerializedSchemaTextBox = new System.Windows.Forms.TextBox();
            this.SerializedSchemaBrowseButton = new System.Windows.Forms.Button();
            this.ConfigurationTipLabel = new System.Windows.Forms.Label();
            this.ExportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ImportFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SerializedSchemaBrowseDialog = new System.Windows.Forms.OpenFileDialog();
            this.ErrorsTabPage = new System.Windows.Forms.TabPage();
            this.ErrorsTextBox = new System.Windows.Forms.TextBox();
            this.MainTabControl.SuspendLayout();
            this.ExportTabPage.SuspendLayout();
            this.ExportTopBarPanel.SuspendLayout();
            this.ImportTabPage.SuspendLayout();
            this.ImportActionsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            this.ImportTopBarPanel.SuspendLayout();
            this.ConfigurationTabPage.SuspendLayout();
            this.ConfigurationPanel.SuspendLayout();
            this.SerializedSchemaConfigurationPanel.SuspendLayout();
            this.ErrorsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.ExportTabPage);
            this.MainTabControl.Controls.Add(this.ImportTabPage);
            this.MainTabControl.Controls.Add(this.ConfigurationTabPage);
            this.MainTabControl.Controls.Add(this.ErrorsTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(484, 361);
            this.MainTabControl.TabIndex = 0;
            this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
            // 
            // ExportTabPage
            // 
            this.ExportTabPage.Controls.Add(this.ExportFieldsPanel);
            this.ExportTabPage.Controls.Add(this.ExportTopBarPanel);
            this.ExportTabPage.Controls.Add(this.ExportProgressBar);
            this.ExportTabPage.Location = new System.Drawing.Point(4, 22);
            this.ExportTabPage.Name = "ExportTabPage";
            this.ExportTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.ExportTabPage.Size = new System.Drawing.Size(476, 335);
            this.ExportTabPage.TabIndex = 1;
            this.ExportTabPage.Text = "Export";
            this.ExportTabPage.UseVisualStyleBackColor = true;
            // 
            // ExportFieldsPanel
            // 
            this.ExportFieldsPanel.AutoScroll = true;
            this.ExportFieldsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ExportFieldsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportFieldsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ExportFieldsPanel.Location = new System.Drawing.Point(10, 39);
            this.ExportFieldsPanel.Name = "ExportFieldsPanel";
            this.ExportFieldsPanel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 20);
            this.ExportFieldsPanel.Size = new System.Drawing.Size(456, 263);
            this.ExportFieldsPanel.TabIndex = 2;
            this.ExportFieldsPanel.WrapContents = false;
            // 
            // ExportTopBarPanel
            // 
            this.ExportTopBarPanel.AutoSize = true;
            this.ExportTopBarPanel.ColumnCount = 3;
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.Controls.Add(this.ExportTableComboBox, 0, 0);
            this.ExportTopBarPanel.Controls.Add(this.ExportButton, 1, 0);
            this.ExportTopBarPanel.Controls.Add(this.ExportCountLabel, 2, 0);
            this.ExportTopBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ExportTopBarPanel.Location = new System.Drawing.Point(10, 10);
            this.ExportTopBarPanel.Name = "ExportTopBarPanel";
            this.ExportTopBarPanel.RowCount = 1;
            this.ExportTopBarPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ExportTopBarPanel.Size = new System.Drawing.Size(456, 29);
            this.ExportTopBarPanel.TabIndex = 3;
            // 
            // ExportTableComboBox
            // 
            this.ExportTableComboBox.DisplayMember = "Name";
            this.ExportTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExportTableComboBox.FormattingEnabled = true;
            this.ExportTableComboBox.Location = new System.Drawing.Point(3, 3);
            this.ExportTableComboBox.Name = "ExportTableComboBox";
            this.ExportTableComboBox.Size = new System.Drawing.Size(200, 21);
            this.ExportTableComboBox.TabIndex = 1;
            this.ExportTableComboBox.SelectedIndexChanged += new System.EventHandler(this.TableComboBox_SelectedIndexChanged);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(209, 3);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export...";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ExportCountLabel
            // 
            this.ExportCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportCountLabel.AutoSize = true;
            this.ExportCountLabel.Location = new System.Drawing.Point(451, 5);
            this.ExportCountLabel.Margin = new System.Windows.Forms.Padding(5);
            this.ExportCountLabel.Name = "ExportCountLabel";
            this.ExportCountLabel.Size = new System.Drawing.Size(0, 13);
            this.ExportCountLabel.TabIndex = 2;
            // 
            // ExportProgressBar
            // 
            this.ExportProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ExportProgressBar.Location = new System.Drawing.Point(10, 302);
            this.ExportProgressBar.Name = "ExportProgressBar";
            this.ExportProgressBar.Size = new System.Drawing.Size(456, 23);
            this.ExportProgressBar.TabIndex = 4;
            // 
            // ImportTabPage
            // 
            this.ImportTabPage.Controls.Add(this.ImportActionsPanel);
            this.ImportTabPage.Controls.Add(this.ImportTopBarPanel);
            this.ImportTabPage.Controls.Add(this.ImportProgressBar);
            this.ImportTabPage.Location = new System.Drawing.Point(4, 22);
            this.ImportTabPage.Name = "ImportTabPage";
            this.ImportTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.ImportTabPage.Size = new System.Drawing.Size(476, 335);
            this.ImportTabPage.TabIndex = 0;
            this.ImportTabPage.Text = "Import";
            this.ImportTabPage.UseVisualStyleBackColor = true;
            // 
            // ImportActionsPanel
            // 
            this.ImportActionsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.ImportActionsPanel.ColumnCount = 1;
            this.ImportActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ImportActionsPanel.Controls.Add(this.InsertButton, 0, 0);
            this.ImportActionsPanel.Controls.Add(this.UpdateButton, 0, 1);
            this.ImportActionsPanel.Controls.Add(this.DeleteButton, 0, 2);
            this.ImportActionsPanel.Controls.Add(this.LogoPictureBox, 0, 3);
            this.ImportActionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportActionsPanel.Location = new System.Drawing.Point(10, 37);
            this.ImportActionsPanel.Name = "ImportActionsPanel";
            this.ImportActionsPanel.RowCount = 4;
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ImportActionsPanel.Size = new System.Drawing.Size(456, 265);
            this.ImportActionsPanel.TabIndex = 3;
            // 
            // InsertButton
            // 
            this.InsertButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.InsertButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InsertButton.Location = new System.Drawing.Point(178, 4);
            this.InsertButton.Name = "InsertButton";
            this.InsertButton.Size = new System.Drawing.Size(100, 30);
            this.InsertButton.TabIndex = 0;
            this.InsertButton.Text = "Insert...";
            this.InsertButton.UseVisualStyleBackColor = true;
            this.InsertButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.UpdateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdateButton.Location = new System.Drawing.Point(178, 41);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(100, 30);
            this.UpdateButton.TabIndex = 1;
            this.UpdateButton.Text = "Update...";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DeleteButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteButton.Location = new System.Drawing.Point(178, 78);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(100, 30);
            this.DeleteButton.TabIndex = 2;
            this.DeleteButton.Text = "Delete...";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("LogoPictureBox.Image")));
            this.LogoPictureBox.Location = new System.Drawing.Point(4, 115);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(448, 146);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.LogoPictureBox.TabIndex = 3;
            this.LogoPictureBox.TabStop = false;
            // 
            // ImportTopBarPanel
            // 
            this.ImportTopBarPanel.AutoSize = true;
            this.ImportTopBarPanel.ColumnCount = 2;
            this.ImportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ImportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ImportTopBarPanel.Controls.Add(this.ImportTableComboBox, 0, 0);
            this.ImportTopBarPanel.Controls.Add(this.ImportCountLabel, 1, 0);
            this.ImportTopBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ImportTopBarPanel.Location = new System.Drawing.Point(10, 10);
            this.ImportTopBarPanel.Name = "ImportTopBarPanel";
            this.ImportTopBarPanel.RowCount = 1;
            this.ImportTopBarPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ImportTopBarPanel.Size = new System.Drawing.Size(456, 27);
            this.ImportTopBarPanel.TabIndex = 4;
            // 
            // ImportTableComboBox
            // 
            this.ImportTableComboBox.DisplayMember = "Name";
            this.ImportTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ImportTableComboBox.FormattingEnabled = true;
            this.ImportTableComboBox.Location = new System.Drawing.Point(3, 3);
            this.ImportTableComboBox.Name = "ImportTableComboBox";
            this.ImportTableComboBox.Size = new System.Drawing.Size(200, 21);
            this.ImportTableComboBox.TabIndex = 1;
            this.ImportTableComboBox.SelectedIndexChanged += new System.EventHandler(this.TableComboBox_SelectedIndexChanged);
            // 
            // ImportCountLabel
            // 
            this.ImportCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportCountLabel.AutoSize = true;
            this.ImportCountLabel.Location = new System.Drawing.Point(451, 5);
            this.ImportCountLabel.Margin = new System.Windows.Forms.Padding(5);
            this.ImportCountLabel.Name = "ImportCountLabel";
            this.ImportCountLabel.Size = new System.Drawing.Size(0, 13);
            this.ImportCountLabel.TabIndex = 3;
            // 
            // ImportProgressBar
            // 
            this.ImportProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ImportProgressBar.Location = new System.Drawing.Point(10, 302);
            this.ImportProgressBar.Name = "ImportProgressBar";
            this.ImportProgressBar.Size = new System.Drawing.Size(456, 23);
            this.ImportProgressBar.TabIndex = 5;
            // 
            // ConfigurationTabPage
            // 
            this.ConfigurationTabPage.Controls.Add(this.ConfigurationPanel);
            this.ConfigurationTabPage.Location = new System.Drawing.Point(4, 22);
            this.ConfigurationTabPage.Name = "ConfigurationTabPage";
            this.ConfigurationTabPage.Size = new System.Drawing.Size(476, 335);
            this.ConfigurationTabPage.TabIndex = 2;
            this.ConfigurationTabPage.Text = "Configuration";
            this.ConfigurationTabPage.UseVisualStyleBackColor = true;
            // 
            // ConfigurationPanel
            // 
            this.ConfigurationPanel.ColumnCount = 1;
            this.ConfigurationPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ConfigurationPanel.Controls.Add(this.ConnectionStringLabel, 0, 0);
            this.ConfigurationPanel.Controls.Add(this.ConnectionStringTextBox, 0, 1);
            this.ConfigurationPanel.Controls.Add(this.DataProviderLabel, 0, 3);
            this.ConfigurationPanel.Controls.Add(this.DataProviderComboBox, 0, 4);
            this.ConfigurationPanel.Controls.Add(this.DataProviderTextBox, 0, 5);
            this.ConfigurationPanel.Controls.Add(this.SerializedSchemaLabel, 0, 7);
            this.ConfigurationPanel.Controls.Add(this.SerializedSchemaConfigurationPanel, 0, 8);
            this.ConfigurationPanel.Controls.Add(this.ConfigurationTipLabel, 0, 10);
            this.ConfigurationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConfigurationPanel.Location = new System.Drawing.Point(0, 0);
            this.ConfigurationPanel.Name = "ConfigurationPanel";
            this.ConfigurationPanel.Padding = new System.Windows.Forms.Padding(10);
            this.ConfigurationPanel.RowCount = 11;
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigurationPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ConfigurationPanel.Size = new System.Drawing.Size(476, 335);
            this.ConfigurationPanel.TabIndex = 1;
            // 
            // ConnectionStringLabel
            // 
            this.ConnectionStringLabel.AutoSize = true;
            this.ConnectionStringLabel.Location = new System.Drawing.Point(13, 10);
            this.ConnectionStringLabel.Name = "ConnectionStringLabel";
            this.ConnectionStringLabel.Size = new System.Drawing.Size(91, 13);
            this.ConnectionStringLabel.TabIndex = 1;
            this.ConnectionStringLabel.Text = "Connection String";
            // 
            // ConnectionStringTextBox
            // 
            this.ConnectionStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionStringTextBox.Location = new System.Drawing.Point(13, 26);
            this.ConnectionStringTextBox.MinimumSize = new System.Drawing.Size(4, 60);
            this.ConnectionStringTextBox.Multiline = true;
            this.ConnectionStringTextBox.Name = "ConnectionStringTextBox";
            this.ConnectionStringTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConnectionStringTextBox.Size = new System.Drawing.Size(450, 60);
            this.ConnectionStringTextBox.TabIndex = 0;
            this.ConnectionStringTextBox.TextChanged += new System.EventHandler(this.ConfigurationTextBox_TextChanged);
            // 
            // DataProviderLabel
            // 
            this.DataProviderLabel.AutoSize = true;
            this.DataProviderLabel.Location = new System.Drawing.Point(13, 109);
            this.DataProviderLabel.Name = "DataProviderLabel";
            this.DataProviderLabel.Size = new System.Drawing.Size(72, 13);
            this.DataProviderLabel.TabIndex = 2;
            this.DataProviderLabel.Text = "Data Provider";
            // 
            // DataProviderComboBox
            // 
            this.DataProviderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataProviderComboBox.FormattingEnabled = true;
            this.DataProviderComboBox.Items.AddRange(new object[] {
            "SQL Server",
            "SQLite",
            "PostgreSQL",
            "MySQL",
            "Oracle"});
            this.DataProviderComboBox.Location = new System.Drawing.Point(13, 125);
            this.DataProviderComboBox.Name = "DataProviderComboBox";
            this.DataProviderComboBox.Size = new System.Drawing.Size(160, 21);
            this.DataProviderComboBox.TabIndex = 3;
            this.DataProviderComboBox.SelectedIndexChanged += new System.EventHandler(this.DataProviderComboBox_SelectedIndexChanged);
            // 
            // DataProviderTextBox
            // 
            this.DataProviderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DataProviderTextBox.Location = new System.Drawing.Point(13, 152);
            this.DataProviderTextBox.Multiline = true;
            this.DataProviderTextBox.Name = "DataProviderTextBox";
            this.DataProviderTextBox.Size = new System.Drawing.Size(450, 60);
            this.DataProviderTextBox.TabIndex = 4;
            this.DataProviderTextBox.TextChanged += new System.EventHandler(this.ConfigurationTextBox_TextChanged);
            // 
            // SerializedSchemaLabel
            // 
            this.SerializedSchemaLabel.AutoSize = true;
            this.SerializedSchemaLabel.Location = new System.Drawing.Point(13, 235);
            this.SerializedSchemaLabel.Name = "SerializedSchemaLabel";
            this.SerializedSchemaLabel.Size = new System.Drawing.Size(94, 13);
            this.SerializedSchemaLabel.TabIndex = 8;
            this.SerializedSchemaLabel.Text = "Serialized Schema";
            // 
            // SerializedSchemaConfigurationPanel
            // 
            this.SerializedSchemaConfigurationPanel.Controls.Add(this.SerializedSchemaTextBox);
            this.SerializedSchemaConfigurationPanel.Controls.Add(this.SerializedSchemaBrowseButton);
            this.SerializedSchemaConfigurationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SerializedSchemaConfigurationPanel.Location = new System.Drawing.Point(13, 251);
            this.SerializedSchemaConfigurationPanel.Name = "SerializedSchemaConfigurationPanel";
            this.SerializedSchemaConfigurationPanel.Size = new System.Drawing.Size(450, 24);
            this.SerializedSchemaConfigurationPanel.TabIndex = 10;
            // 
            // SerializedSchemaTextBox
            // 
            this.SerializedSchemaTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SerializedSchemaTextBox.Location = new System.Drawing.Point(0, 0);
            this.SerializedSchemaTextBox.Name = "SerializedSchemaTextBox";
            this.SerializedSchemaTextBox.Size = new System.Drawing.Size(375, 20);
            this.SerializedSchemaTextBox.TabIndex = 9;
            this.SerializedSchemaTextBox.TextChanged += new System.EventHandler(this.ConfigurationTextBox_TextChanged);
            // 
            // SerializedSchemaBrowseButton
            // 
            this.SerializedSchemaBrowseButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.SerializedSchemaBrowseButton.Location = new System.Drawing.Point(375, 0);
            this.SerializedSchemaBrowseButton.Name = "SerializedSchemaBrowseButton";
            this.SerializedSchemaBrowseButton.Size = new System.Drawing.Size(75, 24);
            this.SerializedSchemaBrowseButton.TabIndex = 10;
            this.SerializedSchemaBrowseButton.Text = "Browse...";
            this.SerializedSchemaBrowseButton.UseVisualStyleBackColor = true;
            this.SerializedSchemaBrowseButton.Click += new System.EventHandler(this.SerializedSchemaBrowseButton_Click);
            // 
            // ConfigurationTipLabel
            // 
            this.ConfigurationTipLabel.AutoSize = true;
            this.ConfigurationTipLabel.Location = new System.Drawing.Point(13, 298);
            this.ConfigurationTipLabel.Name = "ConfigurationTipLabel";
            this.ConfigurationTipLabel.Size = new System.Drawing.Size(449, 26);
            this.ConfigurationTipLabel.TabIndex = 11;
            this.ConfigurationTipLabel.Text = "Tip: Erasing a setting will revert that setting to its default value. All changes" +
    " will be saved upon switching to another tab.";
            // 
            // ExportFileDialog
            // 
            this.ExportFileDialog.DefaultExt = "csv";
            this.ExportFileDialog.Filter = "CSV Files|*.csv|All files|*.*";
            // 
            // ImportFileDialog
            // 
            this.ImportFileDialog.DefaultExt = "csv";
            // 
            // SerializedSchemaBrowseDialog
            // 
            this.SerializedSchemaBrowseDialog.DefaultExt = "bin";
            this.SerializedSchemaBrowseDialog.FileName = "SerializedSchema.bin";
            this.SerializedSchemaBrowseDialog.Filter = "Bin files|*.bin|All files|*.*";
            // 
            // ErrorsTabPage
            // 
            this.ErrorsTabPage.Controls.Add(this.ErrorsTextBox);
            this.ErrorsTabPage.Location = new System.Drawing.Point(4, 22);
            this.ErrorsTabPage.Name = "ErrorsTabPage";
            this.ErrorsTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.ErrorsTabPage.Size = new System.Drawing.Size(476, 335);
            this.ErrorsTabPage.TabIndex = 3;
            this.ErrorsTabPage.Text = "Errors";
            this.ErrorsTabPage.UseVisualStyleBackColor = true;
            // 
            // ErrorsTextBox
            // 
            this.ErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorsTextBox.Location = new System.Drawing.Point(10, 10);
            this.ErrorsTextBox.Multiline = true;
            this.ErrorsTextBox.Name = "ErrorsTextBox";
            this.ErrorsTextBox.ReadOnly = true;
            this.ErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ErrorsTextBox.Size = new System.Drawing.Size(456, 315);
            this.ErrorsTextBox.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.MainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 39);
            this.Name = "MainWindow";
            this.Text = "CSV Data Manager";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.MainTabControl.ResumeLayout(false);
            this.ExportTabPage.ResumeLayout(false);
            this.ExportTabPage.PerformLayout();
            this.ExportTopBarPanel.ResumeLayout(false);
            this.ExportTopBarPanel.PerformLayout();
            this.ImportTabPage.ResumeLayout(false);
            this.ImportTabPage.PerformLayout();
            this.ImportActionsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            this.ImportTopBarPanel.ResumeLayout(false);
            this.ImportTopBarPanel.PerformLayout();
            this.ConfigurationTabPage.ResumeLayout(false);
            this.ConfigurationPanel.ResumeLayout(false);
            this.ConfigurationPanel.PerformLayout();
            this.SerializedSchemaConfigurationPanel.ResumeLayout(false);
            this.SerializedSchemaConfigurationPanel.PerformLayout();
            this.ErrorsTabPage.ResumeLayout(false);
            this.ErrorsTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ImportTabPage;
        private System.Windows.Forms.TabPage ExportTabPage;
        private System.Windows.Forms.ComboBox ExportTableComboBox;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.FlowLayoutPanel ExportFieldsPanel;
        private System.Windows.Forms.SaveFileDialog ExportFileDialog;
        private System.Windows.Forms.ComboBox ImportTableComboBox;
        private System.Windows.Forms.TableLayoutPanel ImportActionsPanel;
        private System.Windows.Forms.Button InsertButton;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.OpenFileDialog ImportFileDialog;
        private System.Windows.Forms.TableLayoutPanel ExportTopBarPanel;
        private System.Windows.Forms.Label ExportCountLabel;
        private System.Windows.Forms.TableLayoutPanel ImportTopBarPanel;
        private System.Windows.Forms.Label ImportCountLabel;
        private System.Windows.Forms.ProgressBar ExportProgressBar;
        private System.Windows.Forms.ProgressBar ImportProgressBar;
        private System.Windows.Forms.TabPage ConfigurationTabPage;
        private System.Windows.Forms.TableLayoutPanel ConfigurationPanel;
        private System.Windows.Forms.Label ConnectionStringLabel;
        private System.Windows.Forms.TextBox ConnectionStringTextBox;
        private System.Windows.Forms.Label DataProviderLabel;
        private System.Windows.Forms.ComboBox DataProviderComboBox;
        private System.Windows.Forms.TextBox DataProviderTextBox;
        private System.Windows.Forms.Label SerializedSchemaLabel;
        private System.Windows.Forms.Panel SerializedSchemaConfigurationPanel;
        private System.Windows.Forms.Button SerializedSchemaBrowseButton;
        private System.Windows.Forms.TextBox SerializedSchemaTextBox;
        private System.Windows.Forms.OpenFileDialog SerializedSchemaBrowseDialog;
        private System.Windows.Forms.Label ConfigurationTipLabel;
        private System.Windows.Forms.PictureBox LogoPictureBox;
        private System.Windows.Forms.TabPage ErrorsTabPage;
        private System.Windows.Forms.TextBox ErrorsTextBox;
    }
}

