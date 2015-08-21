namespace TVA
{
    partial class VerifyParameters
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
            this.TextBoxTabName = new System.Windows.Forms.TextBox();
            this.TextBoxAssemblyPath = new System.Windows.Forms.TextBox();
            this.LabelToolboxTabName = new System.Windows.Forms.Label();
            this.LabelAssemblyPath = new System.Windows.Forms.Label();
            this.ButtonBrowseForAssemblyPath = new System.Windows.Forms.Button();
            this.FolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.ButtonRefresh = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextBoxTabName
            // 
            this.TextBoxTabName.Location = new System.Drawing.Point(115, 13);
            this.TextBoxTabName.Name = "TextBoxTabName";
            this.TextBoxTabName.Size = new System.Drawing.Size(351, 20);
            this.TextBoxTabName.TabIndex = 1;
            // 
            // TextBoxAssemblyPath
            // 
            this.TextBoxAssemblyPath.Location = new System.Drawing.Point(115, 44);
            this.TextBoxAssemblyPath.Name = "TextBoxAssemblyPath";
            this.TextBoxAssemblyPath.Size = new System.Drawing.Size(316, 20);
            this.TextBoxAssemblyPath.TabIndex = 3;
            // 
            // LabelToolboxTabName
            // 
            this.LabelToolboxTabName.AutoSize = true;
            this.LabelToolboxTabName.Location = new System.Drawing.Point(10, 16);
            this.LabelToolboxTabName.Name = "LabelToolboxTabName";
            this.LabelToolboxTabName.Size = new System.Drawing.Size(101, 13);
            this.LabelToolboxTabName.TabIndex = 0;
            this.LabelToolboxTabName.Text = "&Toolbox Tab Name:";
            this.LabelToolboxTabName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelAssemblyPath
            // 
            this.LabelAssemblyPath.AutoSize = true;
            this.LabelAssemblyPath.Location = new System.Drawing.Point(30, 47);
            this.LabelAssemblyPath.Name = "LabelAssemblyPath";
            this.LabelAssemblyPath.Size = new System.Drawing.Size(79, 13);
            this.LabelAssemblyPath.TabIndex = 2;
            this.LabelAssemblyPath.Text = "&Assembly Path:";
            this.LabelAssemblyPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ButtonBrowseForAssemblyPath
            // 
            this.ButtonBrowseForAssemblyPath.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonBrowseForAssemblyPath.Location = new System.Drawing.Point(428, 42);
            this.ButtonBrowseForAssemblyPath.Name = "ButtonBrowseForAssemblyPath";
            this.ButtonBrowseForAssemblyPath.Size = new System.Drawing.Size(38, 23);
            this.ButtonBrowseForAssemblyPath.TabIndex = 4;
            this.ButtonBrowseForAssemblyPath.Text = "...";
            this.ButtonBrowseForAssemblyPath.UseVisualStyleBackColor = true;
            this.ButtonBrowseForAssemblyPath.Click += new System.EventHandler(this.ButtonBrowseForAssemblyPath_Click);
            // 
            // ButtonRefresh
            // 
            this.ButtonRefresh.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonRefresh.Location = new System.Drawing.Point(310, 75);
            this.ButtonRefresh.Name = "ButtonRefresh";
            this.ButtonRefresh.Size = new System.Drawing.Size(75, 23);
            this.ButtonRefresh.TabIndex = 5;
            this.ButtonRefresh.Text = "&Refresh";
            this.ButtonRefresh.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(391, 75);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "&Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // VerifyParameters
            // 
            this.AcceptButton = this.ButtonRefresh;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(480, 109);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonRefresh);
            this.Controls.Add(this.ButtonBrowseForAssemblyPath);
            this.Controls.Add(this.LabelAssemblyPath);
            this.Controls.Add(this.LabelToolboxTabName);
            this.Controls.Add(this.TextBoxAssemblyPath);
            this.Controls.Add(this.TextBoxTabName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerifyParameters";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Refresh {0} Toolbox Tab";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelToolboxTabName;
        private System.Windows.Forms.Label LabelAssemblyPath;
        private System.Windows.Forms.Button ButtonBrowseForAssemblyPath;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowser;
        private System.Windows.Forms.Button ButtonRefresh;
        private System.Windows.Forms.Button ButtonCancel;
        internal System.Windows.Forms.TextBox TextBoxTabName;
        internal System.Windows.Forms.TextBox TextBoxAssemblyPath;
    }
}