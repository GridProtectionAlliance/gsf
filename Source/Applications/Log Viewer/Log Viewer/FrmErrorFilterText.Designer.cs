namespace OGE.Core.GSF.Diagnostics.UI
{
    partial class FrmErrorFilterText
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
            this.TxtErrorName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TxtErrorName
            // 
            this.TxtErrorName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtErrorName.Location = new System.Drawing.Point(0, 0);
            this.TxtErrorName.Multiline = true;
            this.TxtErrorName.Name = "TxtErrorName";
            this.TxtErrorName.Size = new System.Drawing.Size(722, 433);
            this.TxtErrorName.TabIndex = 0;
            this.TxtErrorName.TextChanged += new System.EventHandler(this.TxtErrorName_TextChanged);
            // 
            // FrmErrorFilterText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 433);
            this.Controls.Add(this.TxtErrorName);
            this.Name = "FrmErrorFilterText";
            this.Text = "Error Name: Starts With";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtErrorName;
    }
}