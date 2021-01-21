
namespace SELPDCImporter
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
            this.buttonImport = new System.Windows.Forms.Button();
            this.openFileDialogHostConfig = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogPDCConfig = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // buttonBrowseHostConfig
            // 
            this.buttonBrowseHostConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseHostConfig.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.buttonBrowseHostConfig.Location = new System.Drawing.Point(576, 10);
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
            this.textBoxHostConfig.Size = new System.Drawing.Size(497, 20);
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
            this.buttonBrowsePDCConfig.Location = new System.Drawing.Point(576, 41);
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
            this.textBoxPDCConfig.Size = new System.Drawing.Size(497, 20);
            this.textBoxPDCConfig.TabIndex = 4;
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
            this.buttonAnalyze.Location = new System.Drawing.Point(454, 74);
            this.buttonAnalyze.Name = "buttonAnalyze";
            this.buttonAnalyze.Size = new System.Drawing.Size(75, 23);
            this.buttonAnalyze.TabIndex = 6;
            this.buttonAnalyze.Text = "&Analyze";
            this.buttonAnalyze.UseVisualStyleBackColor = true;
            this.buttonAnalyze.Click += new System.EventHandler(this.buttonAnalyze_Click);
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Enabled = false;
            this.buttonImport.Location = new System.Drawing.Point(535, 74);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 7;
            this.buttonImport.Text = "&Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
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
            this.openFileDialogPDCConfig.Filter = "SEL PDC Config Files (*.3373;*.3573)|*.3373;*.3573|All Files (*.*)|*.*";
            this.openFileDialogPDCConfig.Title = "Select SEL PDC Configuration File to Import";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 261);
            this.Controls.Add(this.buttonImport);
            this.Controls.Add(this.buttonAnalyze);
            this.Controls.Add(this.buttonBrowsePDCConfig);
            this.Controls.Add(this.textBoxPDCConfig);
            this.Controls.Add(this.labelPDCConfig);
            this.Controls.Add(this.buttonBrowseHostConfig);
            this.Controls.Add(this.textBoxHostConfig);
            this.Controls.Add(this.labelHostConfig);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 200);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "";
            this.Text = "SEL PDC Configuration Import Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SELPDCImporter_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SELPDCImporter_FormClosed);
            this.Load += new System.EventHandler(this.SELPDCImporter_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
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
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.OpenFileDialog openFileDialogHostConfig;
        private System.Windows.Forms.OpenFileDialog openFileDialogPDCConfig;
    }
}

