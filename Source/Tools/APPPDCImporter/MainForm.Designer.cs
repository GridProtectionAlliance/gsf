
namespace APPPDCImporter
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.buttonBrowseHostConfig = new System.Windows.Forms.Button();
            this.textBoxHostConfig = new System.Windows.Forms.TextBox();
            this.labelHostConfig = new System.Windows.Forms.Label();
            this.buttonBrowsePDCConfig = new System.Windows.Forms.Button();
            this.textBoxPDCConfig = new System.Windows.Forms.TextBox();
            this.labelPDCConfig = new System.Windows.Forms.Label();
            this.buttonAnalyze = new System.Windows.Forms.Button();
            this.buttonReview = new System.Windows.Forms.Button();
            this.openFileDialogHostConfig = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogPDCConfig = new System.Windows.Forms.OpenFileDialog();
            this.comboBoxIPAddresses = new System.Windows.Forms.ComboBox();
            this.labelPDCDetails = new System.Windows.Forms.Label();
            this.textBoxPDCDetails = new System.Windows.Forms.TextBox();
            this.labelAnalyzeStatus = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.labelSelectIPAddress = new System.Windows.Forms.Label();
            this.tabControlImportOptions = new System.Windows.Forms.TabControl();
            this.tabPageConnectionString = new System.Windows.Forms.TabPage();
            this.tabPageHostConnection = new System.Windows.Forms.TabPage();
            this.textBoxConsoleOutput = new System.Windows.Forms.TextBox();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.tabControlImportOptions.SuspendLayout();
            this.tabPageConnectionString.SuspendLayout();
            this.tabPageHostConnection.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonBrowseHostConfig
            // 
            this.buttonBrowseHostConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseHostConfig.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.buttonBrowseHostConfig.Location = new System.Drawing.Point(736, 10);
            this.buttonBrowseHostConfig.Margin = new System.Windows.Forms.Padding(0);
            this.buttonBrowseHostConfig.Name = "buttonBrowseHostConfig";
            this.buttonBrowseHostConfig.Size = new System.Drawing.Size(34, 22);
            this.buttonBrowseHostConfig.TabIndex = 2;
            this.buttonBrowseHostConfig.Text = "...";
            this.buttonBrowseHostConfig.UseVisualStyleBackColor = true;
            this.buttonBrowseHostConfig.Click += new System.EventHandler(this.buttonBrowseHostConfig_Click);
            // 
            // textBoxHostConfig
            // 
            this.textBoxHostConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHostConfig.Location = new System.Drawing.Point(80, 11);
            this.textBoxHostConfig.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textBoxHostConfig.Name = "textBoxHostConfig";
            this.textBoxHostConfig.Size = new System.Drawing.Size(657, 20);
            this.textBoxHostConfig.TabIndex = 1;
            this.textBoxHostConfig.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelHostConfig
            // 
            this.labelHostConfig.AutoSize = true;
            this.labelHostConfig.Location = new System.Drawing.Point(11, 14);
            this.labelHostConfig.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelHostConfig.Name = "labelHostConfig";
            this.labelHostConfig.Size = new System.Drawing.Size(65, 13);
            this.labelHostConfig.TabIndex = 0;
            this.labelHostConfig.Text = "&Host Config:";
            this.labelHostConfig.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonBrowsePDCConfig
            // 
            this.buttonBrowsePDCConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowsePDCConfig.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.buttonBrowsePDCConfig.Location = new System.Drawing.Point(736, 41);
            this.buttonBrowsePDCConfig.Margin = new System.Windows.Forms.Padding(0);
            this.buttonBrowsePDCConfig.Name = "buttonBrowsePDCConfig";
            this.buttonBrowsePDCConfig.Size = new System.Drawing.Size(34, 22);
            this.buttonBrowsePDCConfig.TabIndex = 5;
            this.buttonBrowsePDCConfig.Text = "...";
            this.buttonBrowsePDCConfig.UseVisualStyleBackColor = true;
            this.buttonBrowsePDCConfig.Click += new System.EventHandler(this.buttonBrowsePDCConfig_Click);
            // 
            // textBoxPDCConfig
            // 
            this.textBoxPDCConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPDCConfig.Location = new System.Drawing.Point(80, 42);
            this.textBoxPDCConfig.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textBoxPDCConfig.Name = "textBoxPDCConfig";
            this.textBoxPDCConfig.Size = new System.Drawing.Size(657, 20);
            this.textBoxPDCConfig.TabIndex = 4;
            this.textBoxPDCConfig.TextChanged += new System.EventHandler(this.textBoxPDCConfig_TextChanged);
            // 
            // labelPDCConfig
            // 
            this.labelPDCConfig.AutoSize = true;
            this.labelPDCConfig.Location = new System.Drawing.Point(11, 45);
            this.labelPDCConfig.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPDCConfig.Name = "labelPDCConfig";
            this.labelPDCConfig.Size = new System.Drawing.Size(65, 13);
            this.labelPDCConfig.TabIndex = 3;
            this.labelPDCConfig.Text = "&PDC Config:";
            this.labelPDCConfig.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonAnalyze
            // 
            this.buttonAnalyze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAnalyze.Enabled = false;
            this.buttonAnalyze.Location = new System.Drawing.Point(695, 69);
            this.buttonAnalyze.Name = "buttonAnalyze";
            this.buttonAnalyze.Size = new System.Drawing.Size(75, 23);
            this.buttonAnalyze.TabIndex = 6;
            this.buttonAnalyze.Text = "&Analyze";
            this.buttonAnalyze.UseVisualStyleBackColor = true;
            this.buttonAnalyze.Click += new System.EventHandler(this.buttonAnalyze_Click);
            // 
            // buttonReview
            // 
            this.buttonReview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReview.Enabled = false;
            this.buttonReview.Location = new System.Drawing.Point(695, 377);
            this.buttonReview.Name = "buttonReview";
            this.buttonReview.Size = new System.Drawing.Size(75, 23);
            this.buttonReview.TabIndex = 9;
            this.buttonReview.Text = "&Review";
            this.buttonReview.UseVisualStyleBackColor = true;
            this.buttonReview.Click += new System.EventHandler(this.buttonReview_Click);
            // 
            // openFileDialogHostConfig
            // 
            this.openFileDialogHostConfig.DefaultExt = "exe.config";
            this.openFileDialogHostConfig.Filter = "Host Config Files (*.exe.config)|*.exe.config|All Files (*.*)|*.*";
            this.openFileDialogHostConfig.SupportMultiDottedExtensions = true;
            this.openFileDialogHostConfig.Title = "Select Host Service Configuration File";
            // 
            // openFileDialogPDCConfig
            // 
            this.openFileDialogPDCConfig.DefaultExt = "3573";
            this.openFileDialogPDCConfig.Filter = "APP PDC Config Files (*.ini)|*.ini|All Files (*.*)|*.*";
            this.openFileDialogPDCConfig.Title = "Select APP PDC Configuration File to Import";
            // 
            // comboBoxIPAddresses
            // 
            this.comboBoxIPAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxIPAddresses.DisplayMember = "Value";
            this.comboBoxIPAddresses.FormattingEnabled = true;
            this.comboBoxIPAddresses.Location = new System.Drawing.Point(193, 5);
            this.comboBoxIPAddresses.Name = "comboBoxIPAddresses";
            this.comboBoxIPAddresses.Size = new System.Drawing.Size(108, 21);
            this.comboBoxIPAddresses.TabIndex = 1;
            this.comboBoxIPAddresses.ValueMember = "Key";
            this.comboBoxIPAddresses.SelectedIndexChanged += new System.EventHandler(this.comboBoxIPAddresses_SelectedIndexChanged);
            // 
            // labelPDCDetails
            // 
            this.labelPDCDetails.AutoSize = true;
            this.labelPDCDetails.Location = new System.Drawing.Point(12, 74);
            this.labelPDCDetails.Name = "labelPDCDetails";
            this.labelPDCDetails.Size = new System.Drawing.Size(67, 13);
            this.labelPDCDetails.TabIndex = 11;
            this.labelPDCDetails.Text = "PDC &Details:";
            // 
            // textBoxPDCDetails
            // 
            this.textBoxPDCDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPDCDetails.BackColor = System.Drawing.Color.Black;
            this.textBoxPDCDetails.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPDCDetails.ForeColor = System.Drawing.Color.Honeydew;
            this.textBoxPDCDetails.Location = new System.Drawing.Point(12, 90);
            this.textBoxPDCDetails.Multiline = true;
            this.textBoxPDCDetails.Name = "textBoxPDCDetails";
            this.textBoxPDCDetails.ReadOnly = true;
            this.textBoxPDCDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxPDCDetails.Size = new System.Drawing.Size(442, 310);
            this.textBoxPDCDetails.TabIndex = 12;
            this.textBoxPDCDetails.WordWrap = false;
            // 
            // labelAnalyzeStatus
            // 
            this.labelAnalyzeStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAnalyzeStatus.Location = new System.Drawing.Point(79, 71);
            this.labelAnalyzeStatus.Name = "labelAnalyzeStatus";
            this.labelAnalyzeStatus.Size = new System.Drawing.Size(380, 18);
            this.labelAnalyzeStatus.TabIndex = 11;
            this.labelAnalyzeStatus.Text = "Unanalyzed.";
            this.labelAnalyzeStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionString.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxConnectionString.Location = new System.Drawing.Point(0, 25);
            this.textBoxConnectionString.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.textBoxConnectionString.Multiline = true;
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxConnectionString.Size = new System.Drawing.Size(301, 244);
            this.textBoxConnectionString.TabIndex = 2;
            this.textBoxConnectionString.TextChanged += new System.EventHandler(this.textBoxConnectionString_TextChanged);
            // 
            // labelSelectIPAddress
            // 
            this.labelSelectIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSelectIPAddress.AutoSize = true;
            this.labelSelectIPAddress.Location = new System.Drawing.Point(93, 8);
            this.labelSelectIPAddress.Name = "labelSelectIPAddress";
            this.labelSelectIPAddress.Size = new System.Drawing.Size(94, 13);
            this.labelSelectIPAddress.TabIndex = 0;
            this.labelSelectIPAddress.Text = "&Select IP Address:";
            // 
            // tabControlImportOptions
            // 
            this.tabControlImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlImportOptions.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControlImportOptions.Controls.Add(this.tabPageConnectionString);
            this.tabControlImportOptions.Controls.Add(this.tabPageHostConnection);
            this.tabControlImportOptions.ItemSize = new System.Drawing.Size(110, 25);
            this.tabControlImportOptions.Location = new System.Drawing.Point(463, 69);
            this.tabControlImportOptions.Name = "tabControlImportOptions";
            this.tabControlImportOptions.Padding = new System.Drawing.Point(0, 0);
            this.tabControlImportOptions.SelectedIndex = 0;
            this.tabControlImportOptions.ShowToolTips = true;
            this.tabControlImportOptions.Size = new System.Drawing.Size(309, 302);
            this.tabControlImportOptions.TabIndex = 7;
            // 
            // tabPageConnectionString
            // 
            this.tabPageConnectionString.BackColor = System.Drawing.Color.Transparent;
            this.tabPageConnectionString.Controls.Add(this.textBoxConnectionString);
            this.tabPageConnectionString.Controls.Add(this.labelSelectIPAddress);
            this.tabPageConnectionString.Controls.Add(this.comboBoxIPAddresses);
            this.tabPageConnectionString.Location = new System.Drawing.Point(4, 29);
            this.tabPageConnectionString.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageConnectionString.Name = "tabPageConnectionString";
            this.tabPageConnectionString.Size = new System.Drawing.Size(301, 269);
            this.tabPageConnectionString.TabIndex = 0;
            this.tabPageConnectionString.Text = " Connection String  ";
            // 
            // tabPageHostConnection
            // 
            this.tabPageHostConnection.Controls.Add(this.textBoxConsoleOutput);
            this.tabPageHostConnection.Location = new System.Drawing.Point(4, 29);
            this.tabPageHostConnection.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageHostConnection.Name = "tabPageHostConnection";
            this.tabPageHostConnection.Size = new System.Drawing.Size(301, 269);
            this.tabPageHostConnection.TabIndex = 1;
            this.tabPageHostConnection.Text = " Host Connection  ";
            this.tabPageHostConnection.UseVisualStyleBackColor = true;
            // 
            // textBoxConsoleOutput
            // 
            this.textBoxConsoleOutput.BackColor = System.Drawing.Color.Black;
            this.textBoxConsoleOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxConsoleOutput.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxConsoleOutput.ForeColor = System.Drawing.Color.Honeydew;
            this.textBoxConsoleOutput.Location = new System.Drawing.Point(0, 0);
            this.textBoxConsoleOutput.MaxLength = 524288;
            this.textBoxConsoleOutput.Multiline = true;
            this.textBoxConsoleOutput.Name = "textBoxConsoleOutput";
            this.textBoxConsoleOutput.ReadOnly = true;
            this.textBoxConsoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxConsoleOutput.Size = new System.Drawing.Size(301, 269);
            this.textBoxConsoleOutput.TabIndex = 0;
            this.textBoxConsoleOutput.WordWrap = false;
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestConnection.Enabled = false;
            this.buttonTestConnection.Location = new System.Drawing.Point(467, 377);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(111, 23);
            this.buttonTestConnection.TabIndex = 8;
            this.buttonTestConnection.Text = "&Test Connection";
            this.buttonTestConnection.UseVisualStyleBackColor = true;
            this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 411);
            this.Controls.Add(this.buttonTestConnection);
            this.Controls.Add(this.buttonAnalyze);
            this.Controls.Add(this.tabControlImportOptions);
            this.Controls.Add(this.labelAnalyzeStatus);
            this.Controls.Add(this.textBoxPDCDetails);
            this.Controls.Add(this.labelPDCDetails);
            this.Controls.Add(this.buttonReview);
            this.Controls.Add(this.buttonBrowsePDCConfig);
            this.Controls.Add(this.textBoxPDCConfig);
            this.Controls.Add(this.labelPDCConfig);
            this.Controls.Add(this.buttonBrowseHostConfig);
            this.Controls.Add(this.textBoxHostConfig);
            this.Controls.Add(this.labelHostConfig);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "";
            this.Text = "APP PDC Configuration Import Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.APPPDCImporter_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.APPPDCImporter_FormClosed);
            this.Load += new System.EventHandler(this.APPPDCImporter_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tabControlImportOptions.ResumeLayout(false);
            this.tabPageConnectionString.ResumeLayout(false);
            this.tabPageConnectionString.PerformLayout();
            this.tabPageHostConnection.ResumeLayout(false);
            this.tabPageHostConnection.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowseHostConfig;
        private System.Windows.Forms.Label labelHostConfig;
        public System.Windows.Forms.TextBox textBoxHostConfig;
        private System.Windows.Forms.Button buttonBrowsePDCConfig;
        public System.Windows.Forms.TextBox textBoxPDCConfig;
        private System.Windows.Forms.Label labelPDCConfig;
        private System.Windows.Forms.Button buttonAnalyze;
        private System.Windows.Forms.Button buttonReview;
        private System.Windows.Forms.OpenFileDialog openFileDialogHostConfig;
        private System.Windows.Forms.OpenFileDialog openFileDialogPDCConfig;
        private System.Windows.Forms.ComboBox comboBoxIPAddresses;
        private System.Windows.Forms.Label labelPDCDetails;
        private System.Windows.Forms.TextBox textBoxPDCDetails;
        private System.Windows.Forms.Label labelAnalyzeStatus;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Label labelSelectIPAddress;
        private System.Windows.Forms.TabControl tabControlImportOptions;
        private System.Windows.Forms.TabPage tabPageConnectionString;
        private System.Windows.Forms.TabPage tabPageHostConnection;
        private System.Windows.Forms.TextBox textBoxConsoleOutput;
        private System.Windows.Forms.Button buttonTestConnection;
    }
}

