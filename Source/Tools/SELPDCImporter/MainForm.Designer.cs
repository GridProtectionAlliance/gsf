
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
            this.SuspendLayout();
            // 
            // buttonBrowseHostConfig
            // 
            this.buttonBrowseHostConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseHostConfig.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.buttonBrowseHostConfig.Location = new System.Drawing.Point(587, 10);
            this.buttonBrowseHostConfig.Margin = new System.Windows.Forms.Padding(0);
            this.buttonBrowseHostConfig.Name = "buttonBrowseHostConfig";
            this.buttonBrowseHostConfig.Size = new System.Drawing.Size(34, 22);
            this.buttonBrowseHostConfig.TabIndex = 15;
            this.buttonBrowseHostConfig.Text = "...";
            this.buttonBrowseHostConfig.UseVisualStyleBackColor = true;
            // 
            // textBoxHostConfig
            // 
            this.textBoxHostConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHostConfig.Location = new System.Drawing.Point(80, 11);
            this.textBoxHostConfig.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textBoxHostConfig.Name = "textBoxHostConfig";
            this.textBoxHostConfig.Size = new System.Drawing.Size(508, 20);
            this.textBoxHostConfig.TabIndex = 14;
            // 
            // labelHostConfig
            // 
            this.labelHostConfig.AutoSize = true;
            this.labelHostConfig.Location = new System.Drawing.Point(11, 14);
            this.labelHostConfig.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelHostConfig.Name = "labelHostConfig";
            this.labelHostConfig.Size = new System.Drawing.Size(65, 13);
            this.labelHostConfig.TabIndex = 13;
            this.labelHostConfig.Text = "&Host Config:";
            // 
            // SELPDCImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 359);
            this.Controls.Add(this.buttonBrowseHostConfig);
            this.Controls.Add(this.textBoxHostConfig);
            this.Controls.Add(this.labelHostConfig);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SELPDCImporter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "";
            this.Text = "SEL PDC Configuration Import Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SELPDCImporter_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SELPDCImporter_FormClosed);
            this.Load += new System.EventHandler(this.SELPDCImporter_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowseHostConfig;
        private System.Windows.Forms.Label labelHostConfig;
        public System.Windows.Forms.TextBox textBoxHostConfig;
    }
}

