namespace TVA.ErrorManagement
{
    partial class ErrorDialog
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
            this.ButtonOK = new System.Windows.Forms.Button();
            this.ButtonMore = new System.Windows.Forms.Button();
            this.RichTextBoxMoreInfo = new System.Windows.Forms.RichTextBox();
            this.LabelMoreInfo = new System.Windows.Forms.Label();
            this.RichTextBoxAction = new System.Windows.Forms.RichTextBox();
            this.LabelAction = new System.Windows.Forms.Label();
            this.RichTextBoxScope = new System.Windows.Forms.RichTextBox();
            this.LabelScope = new System.Windows.Forms.Label();
            this.RichTextBoxError = new System.Windows.Forms.RichTextBox();
            this.LabelError = new System.Windows.Forms.Label();
            this.PictureBoxIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.Location = new System.Drawing.Point(387, 538);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(75, 23);
            this.ButtonOK.TabIndex = 20;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UseVisualStyleBackColor = true;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonMore
            // 
            this.ButtonMore.Location = new System.Drawing.Point(118, 285);
            this.ButtonMore.Name = "ButtonMore";
            this.ButtonMore.Size = new System.Drawing.Size(32, 23);
            this.ButtonMore.TabIndex = 18;
            this.ButtonMore.Text = ">>";
            this.ButtonMore.UseVisualStyleBackColor = true;
            this.ButtonMore.Click += new System.EventHandler(this.ButtonMore_Click);
            // 
            // RichTextBoxMoreInfo
            // 
            this.RichTextBoxMoreInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RichTextBoxMoreInfo.BackColor = System.Drawing.SystemColors.Control;
            this.RichTextBoxMoreInfo.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RichTextBoxMoreInfo.Location = new System.Drawing.Point(14, 314);
            this.RichTextBoxMoreInfo.Name = "RichTextBoxMoreInfo";
            this.RichTextBoxMoreInfo.ReadOnly = true;
            this.RichTextBoxMoreInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.RichTextBoxMoreInfo.Size = new System.Drawing.Size(448, 212);
            this.RichTextBoxMoreInfo.TabIndex = 19;
            this.RichTextBoxMoreInfo.TabStop = false;
            this.RichTextBoxMoreInfo.Text = "";
            // 
            // LabelMoreInfo
            // 
            this.LabelMoreInfo.AutoSize = true;
            this.LabelMoreInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelMoreInfo.Location = new System.Drawing.Point(11, 290);
            this.LabelMoreInfo.Name = "LabelMoreInfo";
            this.LabelMoreInfo.Size = new System.Drawing.Size(101, 13);
            this.LabelMoreInfo.TabIndex = 17;
            this.LabelMoreInfo.Text = "More information";
            // 
            // RichTextBoxAction
            // 
            this.RichTextBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RichTextBoxAction.BackColor = System.Drawing.SystemColors.Control;
            this.RichTextBoxAction.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RichTextBoxAction.Location = new System.Drawing.Point(30, 212);
            this.RichTextBoxAction.Name = "RichTextBoxAction";
            this.RichTextBoxAction.ReadOnly = true;
            this.RichTextBoxAction.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.RichTextBoxAction.Size = new System.Drawing.Size(432, 62);
            this.RichTextBoxAction.TabIndex = 16;
            this.RichTextBoxAction.TabStop = false;
            this.RichTextBoxAction.Text = "";
            // 
            // LabelAction
            // 
            this.LabelAction.AutoSize = true;
            this.LabelAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAction.Location = new System.Drawing.Point(11, 196);
            this.LabelAction.Name = "LabelAction";
            this.LabelAction.Size = new System.Drawing.Size(151, 13);
            this.LabelAction.TabIndex = 15;
            this.LabelAction.Text = "What you can do about it";
            // 
            // RichTextBoxScope
            // 
            this.RichTextBoxScope.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RichTextBoxScope.BackColor = System.Drawing.SystemColors.Control;
            this.RichTextBoxScope.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RichTextBoxScope.Location = new System.Drawing.Point(30, 121);
            this.RichTextBoxScope.Name = "RichTextBoxScope";
            this.RichTextBoxScope.ReadOnly = true;
            this.RichTextBoxScope.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.RichTextBoxScope.Size = new System.Drawing.Size(432, 62);
            this.RichTextBoxScope.TabIndex = 14;
            this.RichTextBoxScope.TabStop = false;
            this.RichTextBoxScope.Text = "";
            // 
            // LabelScope
            // 
            this.LabelScope.AutoSize = true;
            this.LabelScope.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelScope.Location = new System.Drawing.Point(11, 105);
            this.LabelScope.Name = "LabelScope";
            this.LabelScope.Size = new System.Drawing.Size(139, 13);
            this.LabelScope.TabIndex = 13;
            this.LabelScope.Text = "How this will affect you";
            // 
            // RichTextBoxError
            // 
            this.RichTextBoxError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.RichTextBoxError.BackColor = System.Drawing.SystemColors.Control;
            this.RichTextBoxError.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RichTextBoxError.Location = new System.Drawing.Point(72, 28);
            this.RichTextBoxError.Name = "RichTextBoxError";
            this.RichTextBoxError.ReadOnly = true;
            this.RichTextBoxError.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.RichTextBoxError.Size = new System.Drawing.Size(390, 62);
            this.RichTextBoxError.TabIndex = 12;
            this.RichTextBoxError.TabStop = false;
            this.RichTextBoxError.Text = "";
            // 
            // LabelError
            // 
            this.LabelError.AutoSize = true;
            this.LabelError.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelError.Location = new System.Drawing.Point(69, 12);
            this.LabelError.Name = "LabelError";
            this.LabelError.Size = new System.Drawing.Size(97, 13);
            this.LabelError.TabIndex = 10;
            this.LabelError.Text = "What happened";
            // 
            // PictureBoxIcon
            // 
            this.PictureBoxIcon.Location = new System.Drawing.Point(14, 12);
            this.PictureBoxIcon.Name = "PictureBoxIcon";
            this.PictureBoxIcon.Size = new System.Drawing.Size(49, 44);
            this.PictureBoxIcon.TabIndex = 11;
            this.PictureBoxIcon.TabStop = false;
            // 
            // ErrorDialog
            // 
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 573);
            this.ControlBox = false;
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.ButtonMore);
            this.Controls.Add(this.RichTextBoxMoreInfo);
            this.Controls.Add(this.LabelMoreInfo);
            this.Controls.Add(this.RichTextBoxAction);
            this.Controls.Add(this.LabelAction);
            this.Controls.Add(this.RichTextBoxScope);
            this.Controls.Add(this.LabelScope);
            this.Controls.Add(this.RichTextBoxError);
            this.Controls.Add(this.LabelError);
            this.Controls.Add(this.PictureBoxIcon);
            this.Name = "ErrorDialog";
            this.ShowInTaskbar = false;
            this.Text = "ErrorDialog";
            this.Load += new System.EventHandler(this.ErrorDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button ButtonOK;
        internal System.Windows.Forms.Button ButtonMore;
        internal System.Windows.Forms.RichTextBox RichTextBoxMoreInfo;
        internal System.Windows.Forms.Label LabelMoreInfo;
        internal System.Windows.Forms.RichTextBox RichTextBoxAction;
        internal System.Windows.Forms.Label LabelAction;
        internal System.Windows.Forms.RichTextBox RichTextBoxScope;
        internal System.Windows.Forms.Label LabelScope;
        internal System.Windows.Forms.RichTextBox RichTextBoxError;
        internal System.Windows.Forms.Label LabelError;
        internal System.Windows.Forms.PictureBox PictureBoxIcon;

    }
}