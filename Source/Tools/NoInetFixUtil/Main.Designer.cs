namespace NoInetFixUtil
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ServiceOIDCheckBox = new System.Windows.Forms.CheckBox();
            this.ClientOIDCheckBox = new System.Windows.Forms.CheckBox();
            this.RootCertificateListCheckBox = new System.Windows.Forms.CheckBox();
            this.SecurityInfoTextBox = new System.Windows.Forms.TextBox();
            this.GPAProductsTextBox = new System.Windows.Forms.TextBox();
            this.FixesLabel = new System.Windows.Forms.Label();
            this.StatusTextBox = new System.Windows.Forms.TextBox();
            this.OuterSplitContainer = new System.Windows.Forms.SplitContainer();
            this.PublisherEvidenceCheckBox = new System.Windows.Forms.CheckBox();
            this.InnerSplitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.OuterSplitContainer)).BeginInit();
            this.OuterSplitContainer.Panel1.SuspendLayout();
            this.OuterSplitContainer.Panel2.SuspendLayout();
            this.OuterSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InnerSplitContainer)).BeginInit();
            this.InnerSplitContainer.Panel1.SuspendLayout();
            this.InnerSplitContainer.Panel2.SuspendLayout();
            this.InnerSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // ServiceOIDCheckBox
            // 
            this.ServiceOIDCheckBox.AutoSize = true;
            this.ServiceOIDCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ServiceOIDCheckBox.Location = new System.Drawing.Point(0, 13);
            this.ServiceOIDCheckBox.Name = "ServiceOIDCheckBox";
            this.ServiceOIDCheckBox.Size = new System.Drawing.Size(300, 17);
            this.ServiceOIDCheckBox.TabIndex = 3;
            this.ServiceOIDCheckBox.Text = "Register OIDs used by GSF services";
            this.ServiceOIDCheckBox.UseVisualStyleBackColor = true;
            this.ServiceOIDCheckBox.CheckedChanged += new System.EventHandler(this.ServiceOIDCheckBox_CheckedChanged);
            // 
            // ClientOIDCheckBox
            // 
            this.ClientOIDCheckBox.AutoSize = true;
            this.ClientOIDCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ClientOIDCheckBox.Location = new System.Drawing.Point(0, 30);
            this.ClientOIDCheckBox.Name = "ClientOIDCheckBox";
            this.ClientOIDCheckBox.Size = new System.Drawing.Size(300, 17);
            this.ClientOIDCheckBox.TabIndex = 4;
            this.ClientOIDCheckBox.Text = "Register OIDs used by clients (Console/Manager apps)";
            this.ClientOIDCheckBox.UseVisualStyleBackColor = true;
            this.ClientOIDCheckBox.CheckedChanged += new System.EventHandler(this.ClientOIDCheckBox_CheckedChanged);
            // 
            // RootCertificateListCheckBox
            // 
            this.RootCertificateListCheckBox.AutoSize = true;
            this.RootCertificateListCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RootCertificateListCheckBox.Location = new System.Drawing.Point(0, 47);
            this.RootCertificateListCheckBox.Name = "RootCertificateListCheckBox";
            this.RootCertificateListCheckBox.Size = new System.Drawing.Size(300, 30);
            this.RootCertificateListCheckBox.TabIndex = 5;
            this.RootCertificateListCheckBox.Text = "Disable automatic root certificate\r\nlist update through Windows Update";
            this.RootCertificateListCheckBox.UseVisualStyleBackColor = true;
            this.RootCertificateListCheckBox.CheckedChanged += new System.EventHandler(this.RootCertificateListCheckBox_CheckedChanged);
            // 
            // SecurityInfoTextBox
            // 
            this.SecurityInfoTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.SecurityInfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SecurityInfoTextBox.ForeColor = System.Drawing.Color.DarkRed;
            this.SecurityInfoTextBox.Location = new System.Drawing.Point(0, 94);
            this.SecurityInfoTextBox.Multiline = true;
            this.SecurityInfoTextBox.Name = "SecurityInfoTextBox";
            this.SecurityInfoTextBox.ReadOnly = true;
            this.SecurityInfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SecurityInfoTextBox.Size = new System.Drawing.Size(300, 306);
            this.SecurityInfoTextBox.TabIndex = 6;
            this.SecurityInfoTextBox.Text = resources.GetString("SecurityInfoTextBox.Text");
            // 
            // GPAProductsTextBox
            // 
            this.GPAProductsTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.GPAProductsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GPAProductsTextBox.Location = new System.Drawing.Point(0, 0);
            this.GPAProductsTextBox.Multiline = true;
            this.GPAProductsTextBox.Name = "GPAProductsTextBox";
            this.GPAProductsTextBox.ReadOnly = true;
            this.GPAProductsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.GPAProductsTextBox.Size = new System.Drawing.Size(167, 400);
            this.GPAProductsTextBox.TabIndex = 8;
            this.GPAProductsTextBox.Text = "These fixes will affect:";
            // 
            // FixesLabel
            // 
            this.FixesLabel.AutoSize = true;
            this.FixesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FixesLabel.Location = new System.Drawing.Point(0, 0);
            this.FixesLabel.Name = "FixesLabel";
            this.FixesLabel.Size = new System.Drawing.Size(77, 13);
            this.FixesLabel.TabIndex = 2;
            this.FixesLabel.Text = "Available fixes:";
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.StatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusTextBox.Location = new System.Drawing.Point(0, 0);
            this.StatusTextBox.Multiline = true;
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StatusTextBox.Size = new System.Drawing.Size(331, 400);
            this.StatusTextBox.TabIndex = 9;
            // 
            // OuterSplitContainer
            // 
            this.OuterSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OuterSplitContainer.Location = new System.Drawing.Point(10, 10);
            this.OuterSplitContainer.Name = "OuterSplitContainer";
            // 
            // OuterSplitContainer.Panel1
            // 
            this.OuterSplitContainer.Panel1.Controls.Add(this.SecurityInfoTextBox);
            this.OuterSplitContainer.Panel1.Controls.Add(this.PublisherEvidenceCheckBox);
            this.OuterSplitContainer.Panel1.Controls.Add(this.RootCertificateListCheckBox);
            this.OuterSplitContainer.Panel1.Controls.Add(this.ClientOIDCheckBox);
            this.OuterSplitContainer.Panel1.Controls.Add(this.ServiceOIDCheckBox);
            this.OuterSplitContainer.Panel1.Controls.Add(this.FixesLabel);
            this.OuterSplitContainer.Panel1MinSize = 300;
            // 
            // OuterSplitContainer.Panel2
            // 
            this.OuterSplitContainer.Panel2.Controls.Add(this.InnerSplitContainer);
            this.OuterSplitContainer.Panel2MinSize = 300;
            this.OuterSplitContainer.Size = new System.Drawing.Size(806, 400);
            this.OuterSplitContainer.SplitterDistance = 300;
            this.OuterSplitContainer.TabIndex = 1;
            // 
            // PublisherEvidenceCheckBox
            // 
            this.PublisherEvidenceCheckBox.AutoSize = true;
            this.PublisherEvidenceCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PublisherEvidenceCheckBox.Location = new System.Drawing.Point(0, 77);
            this.PublisherEvidenceCheckBox.Name = "PublisherEvidenceCheckBox";
            this.PublisherEvidenceCheckBox.Size = new System.Drawing.Size(300, 17);
            this.PublisherEvidenceCheckBox.TabIndex = 7;
            this.PublisherEvidenceCheckBox.Text = "Disable publisher evidence generation";
            this.PublisherEvidenceCheckBox.UseVisualStyleBackColor = true;
            this.PublisherEvidenceCheckBox.CheckedChanged += new System.EventHandler(this.PublisherEvidenceCheckBox_CheckedChanged);
            // 
            // InnerSplitContainer
            // 
            this.InnerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InnerSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.InnerSplitContainer.Name = "InnerSplitContainer";
            // 
            // InnerSplitContainer.Panel1
            // 
            this.InnerSplitContainer.Panel1.Controls.Add(this.GPAProductsTextBox);
            this.InnerSplitContainer.Panel1MinSize = 100;
            // 
            // InnerSplitContainer.Panel2
            // 
            this.InnerSplitContainer.Panel2.Controls.Add(this.StatusTextBox);
            this.InnerSplitContainer.Panel2MinSize = 100;
            this.InnerSplitContainer.Size = new System.Drawing.Size(502, 400);
            this.InnerSplitContainer.SplitterDistance = 167;
            this.InnerSplitContainer.TabIndex = 7;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(826, 420);
            this.Controls.Add(this.OuterSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "No Internet Access Fix Utility";
            this.Load += new System.EventHandler(this.Main_Load);
            this.OuterSplitContainer.Panel1.ResumeLayout(false);
            this.OuterSplitContainer.Panel1.PerformLayout();
            this.OuterSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OuterSplitContainer)).EndInit();
            this.OuterSplitContainer.ResumeLayout(false);
            this.InnerSplitContainer.Panel1.ResumeLayout(false);
            this.InnerSplitContainer.Panel1.PerformLayout();
            this.InnerSplitContainer.Panel2.ResumeLayout(false);
            this.InnerSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InnerSplitContainer)).EndInit();
            this.InnerSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox ServiceOIDCheckBox;
        private System.Windows.Forms.CheckBox ClientOIDCheckBox;
        private System.Windows.Forms.CheckBox RootCertificateListCheckBox;
        private System.Windows.Forms.TextBox SecurityInfoTextBox;
        private System.Windows.Forms.TextBox GPAProductsTextBox;
        private System.Windows.Forms.Label FixesLabel;
        private System.Windows.Forms.TextBox StatusTextBox;
        private System.Windows.Forms.SplitContainer OuterSplitContainer;
        private System.Windows.Forms.SplitContainer InnerSplitContainer;
        private System.Windows.Forms.CheckBox PublisherEvidenceCheckBox;
    }
}

