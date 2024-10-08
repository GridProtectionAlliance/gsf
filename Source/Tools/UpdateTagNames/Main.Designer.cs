﻿namespace UpdateTagNames
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
            this.labelConfigFile = new System.Windows.Forms.Label();
            this.textBoxConfigFile = new System.Windows.Forms.TextBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.labelExpression = new System.Windows.Forms.Label();
            this.textBoxExpression = new System.Windows.Forms.TextBox();
            this.buttonSelectConfigFile = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.checkBoxSetPortNumber = new System.Windows.Forms.CheckBox();
            this.maskedTextBoxPortNumber = new System.Windows.Forms.MaskedTextBox();
            this.labelInternalPublisherNote = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelConfigFile
            // 
            this.labelConfigFile.AutoSize = true;
            this.labelConfigFile.Location = new System.Drawing.Point(12, 20);
            this.labelConfigFile.Name = "labelConfigFile";
            this.labelConfigFile.Size = new System.Drawing.Size(132, 13);
            this.labelConfigFile.TabIndex = 0;
            this.labelConfigFile.Text = "Source Application &Config:";
            // 
            // textBoxConfigFile
            // 
            this.textBoxConfigFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConfigFile.Location = new System.Drawing.Point(148, 17);
            this.textBoxConfigFile.Name = "textBoxConfigFile";
            this.textBoxConfigFile.Size = new System.Drawing.Size(381, 20);
            this.textBoxConfigFile.TabIndex = 1;
            this.textBoxConfigFile.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.Location = new System.Drawing.Point(466, 248);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(91, 27);
            this.buttonApply.TabIndex = 7;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // labelExpression
            // 
            this.labelExpression.AutoSize = true;
            this.labelExpression.Location = new System.Drawing.Point(12, 52);
            this.labelExpression.Name = "labelExpression";
            this.labelExpression.Size = new System.Drawing.Size(141, 13);
            this.labelExpression.TabIndex = 3;
            this.labelExpression.Text = "Point Tag Name &Expression:";
            // 
            // textBoxExpression
            // 
            this.textBoxExpression.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExpression.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxExpression.Location = new System.Drawing.Point(15, 68);
            this.textBoxExpression.Multiline = true;
            this.textBoxExpression.Name = "textBoxExpression";
            this.textBoxExpression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxExpression.Size = new System.Drawing.Size(542, 167);
            this.textBoxExpression.TabIndex = 4;
            this.textBoxExpression.Text = resources.GetString("textBoxExpression.Text");
            this.textBoxExpression.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // buttonSelectConfigFile
            // 
            this.buttonSelectConfigFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectConfigFile.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSelectConfigFile.Location = new System.Drawing.Point(524, 16);
            this.buttonSelectConfigFile.Name = "buttonSelectConfigFile";
            this.buttonSelectConfigFile.Size = new System.Drawing.Size(33, 22);
            this.buttonSelectConfigFile.TabIndex = 2;
            this.buttonSelectConfigFile.Text = "...";
            this.buttonSelectConfigFile.UseVisualStyleBackColor = true;
            this.buttonSelectConfigFile.Click += new System.EventHandler(this.buttonSelectConfigFile_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "config";
            this.openFileDialog.Filter = "Config Files|*.config|All Files|*.*";
            this.openFileDialog.InitialDirectory = "C:\\Program Files\\";
            this.openFileDialog.Title = "Select Config File";
            // 
            // checkBoxSetPortNumber
            // 
            this.checkBoxSetPortNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxSetPortNumber.AutoSize = true;
            this.checkBoxSetPortNumber.Location = new System.Drawing.Point(15, 251);
            this.checkBoxSetPortNumber.Name = "checkBoxSetPortNumber";
            this.checkBoxSetPortNumber.Size = new System.Drawing.Size(153, 17);
            this.checkBoxSetPortNumber.TabIndex = 5;
            this.checkBoxSetPortNumber.Text = "Assign STTP &Port Number:";
            this.checkBoxSetPortNumber.UseVisualStyleBackColor = true;
            this.checkBoxSetPortNumber.CheckedChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // maskedTextBoxPortNumber
            // 
            this.maskedTextBoxPortNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.maskedTextBoxPortNumber.Location = new System.Drawing.Point(173, 249);
            this.maskedTextBoxPortNumber.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.maskedTextBoxPortNumber.Mask = "00000";
            this.maskedTextBoxPortNumber.Name = "maskedTextBoxPortNumber";
            this.maskedTextBoxPortNumber.Size = new System.Drawing.Size(46, 20);
            this.maskedTextBoxPortNumber.TabIndex = 6;
            this.maskedTextBoxPortNumber.Text = "7175";
            this.maskedTextBoxPortNumber.ValidatingType = typeof(int);
            this.maskedTextBoxPortNumber.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelInternalPublisherNote
            // 
            this.labelInternalPublisherNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInternalPublisherNote.AutoSize = true;
            this.labelInternalPublisherNote.Location = new System.Drawing.Point(222, 251);
            this.labelInternalPublisherNote.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelInternalPublisherNote.Name = "labelInternalPublisherNote";
            this.labelInternalPublisherNote.Size = new System.Drawing.Size(98, 13);
            this.labelInternalPublisherNote.TabIndex = 8;
            this.labelInternalPublisherNote.Text = "( internal publisher )";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 291);
            this.Controls.Add(this.labelInternalPublisherNote);
            this.Controls.Add(this.maskedTextBoxPortNumber);
            this.Controls.Add(this.checkBoxSetPortNumber);
            this.Controls.Add(this.buttonSelectConfigFile);
            this.Controls.Add(this.textBoxExpression);
            this.Controls.Add(this.labelExpression);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.textBoxConfigFile);
            this.Controls.Add(this.labelConfigFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(581, 288);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Update Point Tag Name Expression";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelConfigFile;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Label labelExpression;
        private System.Windows.Forms.Button buttonSelectConfigFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        public System.Windows.Forms.CheckBox checkBoxSetPortNumber;
        public System.Windows.Forms.TextBox textBoxConfigFile;
        public System.Windows.Forms.TextBox textBoxExpression;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxPortNumber;
        private System.Windows.Forms.Label labelInternalPublisherNote;
    }
}

