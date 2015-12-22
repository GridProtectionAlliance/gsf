namespace PQDIFExplorer
{
    partial class DetailsWindow
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
            this.DetailsTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // DetailsTextBox
            // 
            this.DetailsTextBox.Location = new System.Drawing.Point(12, 12);
            this.DetailsTextBox.Multiline = true;
            this.DetailsTextBox.Name = "DetailsTextBox";
            this.DetailsTextBox.ReadOnly = true;
            this.DetailsTextBox.Size = new System.Drawing.Size(462, 299);
            this.DetailsTextBox.TabIndex = 0;
            this.DetailsTextBox.WordWrap = false;
            this.DetailsTextBox.TextChanged += new System.EventHandler(this.DetailsTextBox_TextChanged);
            this.DetailsTextBox.Resize += new System.EventHandler(this.DetailsTextBox_Resize);
            // 
            // DetailsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 323);
            this.Controls.Add(this.DetailsTextBox);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DetailsWindow";
            this.Text = "DetailsWindow";
            this.Load += new System.EventHandler(this.DetailsWindow_Load);
            this.Resize += new System.EventHandler(this.DetailsWindow_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox DetailsTextBox;
    }
}