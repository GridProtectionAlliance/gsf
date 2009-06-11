namespace ConfigCrypter
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
            this.GroupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.LinkLabelCopy = new System.Windows.Forms.LinkLabel();
            this.TextBoxOutput = new System.Windows.Forms.TextBox();
            this.LabelOutput = new System.Windows.Forms.Label();
            this.TextBoxInput = new System.Windows.Forms.TextBox();
            this.LabelInput = new System.Windows.Forms.Label();
            this.GroupBoxSettings = new System.Windows.Forms.GroupBox();
            this.TextBoxKey = new System.Windows.Forms.TextBox();
            this.LabelKey = new System.Windows.Forms.Label();
            this.RadioButtonDecrypt = new System.Windows.Forms.RadioButton();
            this.RadioButtonEncrypt = new System.Windows.Forms.RadioButton();
            this.GroupBoxConfiguration.SuspendLayout();
            this.GroupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupBoxConfiguration
            // 
            this.GroupBoxConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBoxConfiguration.Controls.Add(this.LinkLabelCopy);
            this.GroupBoxConfiguration.Controls.Add(this.TextBoxOutput);
            this.GroupBoxConfiguration.Controls.Add(this.LabelOutput);
            this.GroupBoxConfiguration.Controls.Add(this.TextBoxInput);
            this.GroupBoxConfiguration.Controls.Add(this.LabelInput);
            this.GroupBoxConfiguration.Location = new System.Drawing.Point(12, 97);
            this.GroupBoxConfiguration.Name = "GroupBoxConfiguration";
            this.GroupBoxConfiguration.Size = new System.Drawing.Size(270, 159);
            this.GroupBoxConfiguration.TabIndex = 999;
            this.GroupBoxConfiguration.TabStop = false;
            this.GroupBoxConfiguration.Text = "Config Value";
            // 
            // LinkLabelCopy
            // 
            this.LinkLabelCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkLabelCopy.AutoSize = true;
            this.LinkLabelCopy.Location = new System.Drawing.Point(171, 66);
            this.LinkLabelCopy.Name = "LinkLabelCopy";
            this.LinkLabelCopy.Size = new System.Drawing.Size(93, 13);
            this.LinkLabelCopy.TabIndex = 5;
            this.LinkLabelCopy.TabStop = true;
            this.LinkLabelCopy.Text = "Copy to Clipboard";
            this.LinkLabelCopy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelCopy_LinkClicked);
            // 
            // TextBoxOutput
            // 
            this.TextBoxOutput.Location = new System.Drawing.Point(9, 82);
            this.TextBoxOutput.Multiline = true;
            this.TextBoxOutput.Name = "TextBoxOutput";
            this.TextBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxOutput.Size = new System.Drawing.Size(253, 65);
            this.TextBoxOutput.TabIndex = 4;
            // 
            // LabelOutput
            // 
            this.LabelOutput.AutoSize = true;
            this.LabelOutput.Location = new System.Drawing.Point(6, 66);
            this.LabelOutput.Name = "LabelOutput";
            this.LabelOutput.Size = new System.Drawing.Size(45, 13);
            this.LabelOutput.TabIndex = 999;
            this.LabelOutput.Text = "Output:";
            // 
            // TextBoxInput
            // 
            this.TextBoxInput.Location = new System.Drawing.Point(9, 33);
            this.TextBoxInput.Name = "TextBoxInput";
            this.TextBoxInput.Size = new System.Drawing.Size(253, 21);
            this.TextBoxInput.TabIndex = 3;
            this.TextBoxInput.TextChanged += new System.EventHandler(this.TextBoxInput_TextChanged);
            // 
            // LabelInput
            // 
            this.LabelInput.AutoSize = true;
            this.LabelInput.Location = new System.Drawing.Point(6, 17);
            this.LabelInput.Name = "LabelInput";
            this.LabelInput.Size = new System.Drawing.Size(37, 13);
            this.LabelInput.TabIndex = 999;
            this.LabelInput.Text = "Input:";
            // 
            // GroupBoxSettings
            // 
            this.GroupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBoxSettings.Controls.Add(this.TextBoxKey);
            this.GroupBoxSettings.Controls.Add(this.LabelKey);
            this.GroupBoxSettings.Controls.Add(this.RadioButtonDecrypt);
            this.GroupBoxSettings.Controls.Add(this.RadioButtonEncrypt);
            this.GroupBoxSettings.Location = new System.Drawing.Point(12, 11);
            this.GroupBoxSettings.Name = "GroupBoxSettings";
            this.GroupBoxSettings.Size = new System.Drawing.Size(270, 80);
            this.GroupBoxSettings.TabIndex = 999;
            this.GroupBoxSettings.TabStop = false;
            this.GroupBoxSettings.Text = "Settings";
            // 
            // TextBoxKey
            // 
            this.TextBoxKey.Location = new System.Drawing.Point(9, 30);
            this.TextBoxKey.Name = "TextBoxKey";
            this.TextBoxKey.PasswordChar = '*';
            this.TextBoxKey.Size = new System.Drawing.Size(253, 21);
            this.TextBoxKey.TabIndex = 0;
            this.TextBoxKey.TextChanged += new System.EventHandler(this.TextBoxKey_TextChanged);
            // 
            // LabelKey
            // 
            this.LabelKey.AutoSize = true;
            this.LabelKey.Location = new System.Drawing.Point(6, 14);
            this.LabelKey.Name = "LabelKey";
            this.LabelKey.Size = new System.Drawing.Size(80, 13);
            this.LabelKey.TabIndex = 999;
            this.LabelKey.Text = "Key (Optional):";
            // 
            // RadioButtonDecrypt
            // 
            this.RadioButtonDecrypt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RadioButtonDecrypt.AutoSize = true;
            this.RadioButtonDecrypt.Location = new System.Drawing.Point(138, 57);
            this.RadioButtonDecrypt.Name = "RadioButtonDecrypt";
            this.RadioButtonDecrypt.Size = new System.Drawing.Size(126, 17);
            this.RadioButtonDecrypt.TabIndex = 2;
            this.RadioButtonDecrypt.Text = "Decrypt Config Value";
            this.RadioButtonDecrypt.UseVisualStyleBackColor = true;
            // 
            // RadioButtonEncrypt
            // 
            this.RadioButtonEncrypt.AutoSize = true;
            this.RadioButtonEncrypt.Checked = true;
            this.RadioButtonEncrypt.Location = new System.Drawing.Point(6, 57);
            this.RadioButtonEncrypt.Name = "RadioButtonEncrypt";
            this.RadioButtonEncrypt.Size = new System.Drawing.Size(125, 17);
            this.RadioButtonEncrypt.TabIndex = 1;
            this.RadioButtonEncrypt.TabStop = true;
            this.RadioButtonEncrypt.Text = "Encrypt Config Value";
            this.RadioButtonEncrypt.UseVisualStyleBackColor = true;
            this.RadioButtonEncrypt.CheckedChanged += new System.EventHandler(this.RadioButtonEncrypt_CheckedChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 268);
            this.Controls.Add(this.GroupBoxConfiguration);
            this.Controls.Add(this.GroupBoxSettings);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "{0} v{1}";
            this.Load += new System.EventHandler(this.Main_Load);
            this.GroupBoxConfiguration.ResumeLayout(false);
            this.GroupBoxConfiguration.PerformLayout();
            this.GroupBoxSettings.ResumeLayout(false);
            this.GroupBoxSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBoxConfiguration;
        private System.Windows.Forms.GroupBox GroupBoxSettings;
        private System.Windows.Forms.LinkLabel LinkLabelCopy;
        private System.Windows.Forms.TextBox TextBoxOutput;
        private System.Windows.Forms.Label LabelOutput;
        private System.Windows.Forms.TextBox TextBoxInput;
        private System.Windows.Forms.Label LabelInput;
        private System.Windows.Forms.RadioButton RadioButtonDecrypt;
        private System.Windows.Forms.RadioButton RadioButtonEncrypt;
        private System.Windows.Forms.TextBox TextBoxKey;
        private System.Windows.Forms.Label LabelKey;
    }
}

