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
            this.SuspendLayout();
            // 
            // ServiceOIDCheckBox
            // 
            this.ServiceOIDCheckBox.AutoSize = true;
            this.ServiceOIDCheckBox.Location = new System.Drawing.Point(12, 33);
            this.ServiceOIDCheckBox.Name = "ServiceOIDCheckBox";
            this.ServiceOIDCheckBox.Size = new System.Drawing.Size(198, 17);
            this.ServiceOIDCheckBox.TabIndex = 0;
            this.ServiceOIDCheckBox.Text = "Register OIDs used by GSF services";
            this.ServiceOIDCheckBox.UseVisualStyleBackColor = true;
            this.ServiceOIDCheckBox.CheckedChanged += new System.EventHandler(this.ServiceOIDCheckBox_CheckedChanged);
            // 
            // ClientOIDCheckBox
            // 
            this.ClientOIDCheckBox.AutoSize = true;
            this.ClientOIDCheckBox.Location = new System.Drawing.Point(12, 56);
            this.ClientOIDCheckBox.Name = "ClientOIDCheckBox";
            this.ClientOIDCheckBox.Size = new System.Drawing.Size(285, 17);
            this.ClientOIDCheckBox.TabIndex = 1;
            this.ClientOIDCheckBox.Text = "Register OIDs used by clients (Console/Manager apps)";
            this.ClientOIDCheckBox.UseVisualStyleBackColor = true;
            this.ClientOIDCheckBox.CheckedChanged += new System.EventHandler(this.ClientOIDCheckBox_CheckedChanged);
            // 
            // RootCertificateListCheckBox
            // 
            this.RootCertificateListCheckBox.AutoSize = true;
            this.RootCertificateListCheckBox.Location = new System.Drawing.Point(12, 79);
            this.RootCertificateListCheckBox.Name = "RootCertificateListCheckBox";
            this.RootCertificateListCheckBox.Size = new System.Drawing.Size(198, 30);
            this.RootCertificateListCheckBox.TabIndex = 2;
            this.RootCertificateListCheckBox.Text = "Disable automatic root certificate\r\nlist update through Windows Update";
            this.RootCertificateListCheckBox.UseVisualStyleBackColor = true;
            this.RootCertificateListCheckBox.CheckedChanged += new System.EventHandler(this.RootCertificateListCheckBox_CheckedChanged);
            // 
            // SecurityInfoTextBox
            // 
            this.SecurityInfoTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.SecurityInfoTextBox.ForeColor = System.Drawing.Color.DarkRed;
            this.SecurityInfoTextBox.Location = new System.Drawing.Point(12, 115);
            this.SecurityInfoTextBox.Multiline = true;
            this.SecurityInfoTextBox.Name = "SecurityInfoTextBox";
            this.SecurityInfoTextBox.ReadOnly = true;
            this.SecurityInfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SecurityInfoTextBox.Size = new System.Drawing.Size(285, 293);
            this.SecurityInfoTextBox.TabIndex = 3;
            this.SecurityInfoTextBox.Text = resources.GetString("SecurityInfoTextBox.Text");
            // 
            // GPAProductsTextBox
            // 
            this.GPAProductsTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.GPAProductsTextBox.Location = new System.Drawing.Point(311, 12);
            this.GPAProductsTextBox.Multiline = true;
            this.GPAProductsTextBox.Name = "GPAProductsTextBox";
            this.GPAProductsTextBox.ReadOnly = true;
            this.GPAProductsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.GPAProductsTextBox.Size = new System.Drawing.Size(149, 396);
            this.GPAProductsTextBox.TabIndex = 4;
            this.GPAProductsTextBox.Text = "These fixes will affect:";
            // 
            // FixesLabel
            // 
            this.FixesLabel.AutoSize = true;
            this.FixesLabel.Location = new System.Drawing.Point(9, 12);
            this.FixesLabel.Name = "FixesLabel";
            this.FixesLabel.Size = new System.Drawing.Size(77, 13);
            this.FixesLabel.TabIndex = 5;
            this.FixesLabel.Text = "Available fixes:";
            // 
            // StatusTextBox
            // 
            this.StatusTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.StatusTextBox.Location = new System.Drawing.Point(466, 12);
            this.StatusTextBox.Multiline = true;
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StatusTextBox.Size = new System.Drawing.Size(348, 396);
            this.StatusTextBox.TabIndex = 6;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(826, 420);
            this.Controls.Add(this.StatusTextBox);
            this.Controls.Add(this.FixesLabel);
            this.Controls.Add(this.GPAProductsTextBox);
            this.Controls.Add(this.SecurityInfoTextBox);
            this.Controls.Add(this.RootCertificateListCheckBox);
            this.Controls.Add(this.ClientOIDCheckBox);
            this.Controls.Add(this.ServiceOIDCheckBox);
            this.Name = "Main";
            this.Text = "NoInetFixUtil";
            this.Load += new System.EventHandler(this.Main_Load);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox ServiceOIDCheckBox;
        private System.Windows.Forms.CheckBox ClientOIDCheckBox;
        private System.Windows.Forms.CheckBox RootCertificateListCheckBox;
        private System.Windows.Forms.TextBox SecurityInfoTextBox;
        private System.Windows.Forms.TextBox GPAProductsTextBox;
        private System.Windows.Forms.Label FixesLabel;
        private System.Windows.Forms.TextBox StatusTextBox;
    }
}

