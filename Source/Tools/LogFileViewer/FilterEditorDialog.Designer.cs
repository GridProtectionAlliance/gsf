namespace LogFileViewer
{
    partial class FilterEditorDialog
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
            this.cmbVerbose = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTime = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblClass = new System.Windows.Forms.Label();
            this.btnClass = new System.Windows.Forms.Button();
            this.lblLevel = new System.Windows.Forms.Label();
            this.lblFlags = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblAssembly = new System.Windows.Forms.Label();
            this.btnType = new System.Windows.Forms.Button();
            this.lblRelatedType = new System.Windows.Forms.Label();
            this.lblStackDetails = new System.Windows.Forms.Label();
            this.btnStackDetails = new System.Windows.Forms.Button();
            this.lblStackTrace = new System.Windows.Forms.Label();
            this.btnStackTrace = new System.Windows.Forms.Button();
            this.lblEventName = new System.Windows.Forms.Label();
            this.btnEventName = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.benMessage = new System.Windows.Forms.Button();
            this.lblDetails = new System.Windows.Forms.Label();
            this.btnDetails = new System.Windows.Forms.Button();
            this.lblException = new System.Windows.Forms.Label();
            this.btnException = new System.Windows.Forms.Button();
            this.chkTime = new System.Windows.Forms.CheckBox();
            this.chkClass = new System.Windows.Forms.CheckBox();
            this.chkLevel = new System.Windows.Forms.CheckBox();
            this.chkFlags = new System.Windows.Forms.CheckBox();
            this.chkAssembly = new System.Windows.Forms.CheckBox();
            this.chkType = new System.Windows.Forms.CheckBox();
            this.chkRelatedType = new System.Windows.Forms.CheckBox();
            this.chkStackDetails = new System.Windows.Forms.CheckBox();
            this.chkStackTrace = new System.Windows.Forms.CheckBox();
            this.chkEventName = new System.Windows.Forms.CheckBox();
            this.chkMessage = new System.Windows.Forms.CheckBox();
            this.chkDetails = new System.Windows.Forms.CheckBox();
            this.chkException = new System.Windows.Forms.CheckBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.btnLevel = new System.Windows.Forms.Button();
            this.btnFlags = new System.Windows.Forms.Button();
            this.btnAssembly = new System.Windows.Forms.Button();
            this.btnRelatedType = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbVerbose
            // 
            this.cmbVerbose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVerbose.FormattingEnabled = true;
            this.cmbVerbose.Items.AddRange(new object[] {
            "Lowest",
            "Low",
            "BelowNormal",
            "Normal",
            "AboveNormal",
            "High",
            "Highest"});
            this.cmbVerbose.Location = new System.Drawing.Point(93, 6);
            this.cmbVerbose.Name = "cmbVerbose";
            this.cmbVerbose.Size = new System.Drawing.Size(121, 21);
            this.cmbVerbose.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Verbose Level";
            // 
            // btnTime
            // 
            this.btnTime.Location = new System.Drawing.Point(32, 38);
            this.btnTime.Name = "btnTime";
            this.btnTime.Size = new System.Drawing.Size(78, 23);
            this.btnTime.TabIndex = 5;
            this.btnTime.Text = "Time";
            this.btnTime.UseVisualStyleBackColor = true;
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTime.AutoEllipsis = true;
            this.lblTime.Location = new System.Drawing.Point(116, 43);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(315, 13);
            this.lblTime.TabIndex = 6;
            this.lblTime.Text = "N/A";
            // 
            // lblClass
            // 
            this.lblClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClass.AutoEllipsis = true;
            this.lblClass.Location = new System.Drawing.Point(116, 72);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(315, 13);
            this.lblClass.TabIndex = 8;
            this.lblClass.Text = "Classification: N/A";
            // 
            // btnClass
            // 
            this.btnClass.Location = new System.Drawing.Point(32, 67);
            this.btnClass.Name = "btnClass";
            this.btnClass.Size = new System.Drawing.Size(78, 23);
            this.btnClass.TabIndex = 7;
            this.btnClass.Text = "Classification";
            this.btnClass.UseVisualStyleBackColor = true;
            // 
            // lblLevel
            // 
            this.lblLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLevel.AutoEllipsis = true;
            this.lblLevel.Location = new System.Drawing.Point(116, 101);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(315, 13);
            this.lblLevel.TabIndex = 10;
            this.lblLevel.Text = "Level: N/A";
            // 
            // lblFlags
            // 
            this.lblFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFlags.AutoEllipsis = true;
            this.lblFlags.Location = new System.Drawing.Point(116, 130);
            this.lblFlags.Name = "lblFlags";
            this.lblFlags.Size = new System.Drawing.Size(315, 13);
            this.lblFlags.TabIndex = 12;
            this.lblFlags.Text = "Flags: N/A";
            // 
            // lblType
            // 
            this.lblType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblType.AutoEllipsis = true;
            this.lblType.Location = new System.Drawing.Point(116, 188);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(315, 13);
            this.lblType.TabIndex = 14;
            this.lblType.Text = "Type: N/A";
            // 
            // lblAssembly
            // 
            this.lblAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAssembly.AutoEllipsis = true;
            this.lblAssembly.Location = new System.Drawing.Point(116, 159);
            this.lblAssembly.Name = "lblAssembly";
            this.lblAssembly.Size = new System.Drawing.Size(315, 13);
            this.lblAssembly.TabIndex = 16;
            this.lblAssembly.Text = "Assembly: N/A";
            // 
            // btnType
            // 
            this.btnType.Location = new System.Drawing.Point(32, 183);
            this.btnType.Name = "btnType";
            this.btnType.Size = new System.Drawing.Size(78, 23);
            this.btnType.TabIndex = 15;
            this.btnType.Text = "Type";
            this.btnType.UseVisualStyleBackColor = true;
            this.btnType.Click += new System.EventHandler(this.btnType_Click);
            // 
            // lblRelatedType
            // 
            this.lblRelatedType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRelatedType.AutoEllipsis = true;
            this.lblRelatedType.Location = new System.Drawing.Point(116, 217);
            this.lblRelatedType.Name = "lblRelatedType";
            this.lblRelatedType.Size = new System.Drawing.Size(315, 13);
            this.lblRelatedType.TabIndex = 17;
            this.lblRelatedType.Text = "Related Type: N/A";
            // 
            // lblStackDetails
            // 
            this.lblStackDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStackDetails.AutoEllipsis = true;
            this.lblStackDetails.Location = new System.Drawing.Point(116, 246);
            this.lblStackDetails.Name = "lblStackDetails";
            this.lblStackDetails.Size = new System.Drawing.Size(315, 13);
            this.lblStackDetails.TabIndex = 19;
            this.lblStackDetails.Text = "N/A";
            // 
            // btnStackDetails
            // 
            this.btnStackDetails.Location = new System.Drawing.Point(32, 241);
            this.btnStackDetails.Name = "btnStackDetails";
            this.btnStackDetails.Size = new System.Drawing.Size(78, 23);
            this.btnStackDetails.TabIndex = 18;
            this.btnStackDetails.Text = "Stack Details";
            this.btnStackDetails.UseVisualStyleBackColor = true;
            this.btnStackDetails.Click += new System.EventHandler(this.btnStackDetails_Click);
            // 
            // lblStackTrace
            // 
            this.lblStackTrace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStackTrace.AutoEllipsis = true;
            this.lblStackTrace.Location = new System.Drawing.Point(116, 275);
            this.lblStackTrace.Name = "lblStackTrace";
            this.lblStackTrace.Size = new System.Drawing.Size(315, 13);
            this.lblStackTrace.TabIndex = 21;
            this.lblStackTrace.Text = "N/A";
            // 
            // btnStackTrace
            // 
            this.btnStackTrace.Location = new System.Drawing.Point(32, 270);
            this.btnStackTrace.Name = "btnStackTrace";
            this.btnStackTrace.Size = new System.Drawing.Size(78, 23);
            this.btnStackTrace.TabIndex = 20;
            this.btnStackTrace.Text = "Stack Trace";
            this.btnStackTrace.UseVisualStyleBackColor = true;
            // 
            // lblEventName
            // 
            this.lblEventName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEventName.AutoEllipsis = true;
            this.lblEventName.Location = new System.Drawing.Point(116, 304);
            this.lblEventName.Name = "lblEventName";
            this.lblEventName.Size = new System.Drawing.Size(315, 13);
            this.lblEventName.TabIndex = 23;
            this.lblEventName.Text = "N/A";
            // 
            // btnEventName
            // 
            this.btnEventName.Location = new System.Drawing.Point(32, 299);
            this.btnEventName.Name = "btnEventName";
            this.btnEventName.Size = new System.Drawing.Size(78, 23);
            this.btnEventName.TabIndex = 22;
            this.btnEventName.Text = "Event Name";
            this.btnEventName.UseVisualStyleBackColor = true;
            this.btnEventName.Click += new System.EventHandler(this.btnEventName_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.AutoEllipsis = true;
            this.lblMessage.Location = new System.Drawing.Point(116, 333);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(315, 13);
            this.lblMessage.TabIndex = 25;
            this.lblMessage.Text = "N/A";
            // 
            // benMessage
            // 
            this.benMessage.Location = new System.Drawing.Point(32, 328);
            this.benMessage.Name = "benMessage";
            this.benMessage.Size = new System.Drawing.Size(78, 23);
            this.benMessage.TabIndex = 24;
            this.benMessage.Text = "Message";
            this.benMessage.UseVisualStyleBackColor = true;
            this.benMessage.Click += new System.EventHandler(this.benMessage_Click);
            // 
            // lblDetails
            // 
            this.lblDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDetails.AutoEllipsis = true;
            this.lblDetails.Location = new System.Drawing.Point(116, 362);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(315, 13);
            this.lblDetails.TabIndex = 27;
            this.lblDetails.Text = "N/A";
            // 
            // btnDetails
            // 
            this.btnDetails.Location = new System.Drawing.Point(32, 357);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(78, 23);
            this.btnDetails.TabIndex = 26;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // lblException
            // 
            this.lblException.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblException.AutoEllipsis = true;
            this.lblException.Location = new System.Drawing.Point(116, 391);
            this.lblException.Name = "lblException";
            this.lblException.Size = new System.Drawing.Size(315, 13);
            this.lblException.TabIndex = 29;
            this.lblException.Text = "N/A";
            // 
            // btnException
            // 
            this.btnException.Location = new System.Drawing.Point(32, 386);
            this.btnException.Name = "btnException";
            this.btnException.Size = new System.Drawing.Size(78, 23);
            this.btnException.TabIndex = 28;
            this.btnException.Text = "Exception";
            this.btnException.UseVisualStyleBackColor = true;
            this.btnException.Click += new System.EventHandler(this.btnException_Click);
            // 
            // chkTime
            // 
            this.chkTime.AutoSize = true;
            this.chkTime.Location = new System.Drawing.Point(11, 43);
            this.chkTime.Name = "chkTime";
            this.chkTime.Size = new System.Drawing.Size(15, 14);
            this.chkTime.TabIndex = 30;
            this.chkTime.UseVisualStyleBackColor = true;
            // 
            // chkClass
            // 
            this.chkClass.AutoSize = true;
            this.chkClass.Location = new System.Drawing.Point(11, 72);
            this.chkClass.Name = "chkClass";
            this.chkClass.Size = new System.Drawing.Size(15, 14);
            this.chkClass.TabIndex = 31;
            this.chkClass.UseVisualStyleBackColor = true;
            // 
            // chkLevel
            // 
            this.chkLevel.AutoSize = true;
            this.chkLevel.Location = new System.Drawing.Point(11, 101);
            this.chkLevel.Name = "chkLevel";
            this.chkLevel.Size = new System.Drawing.Size(15, 14);
            this.chkLevel.TabIndex = 32;
            this.chkLevel.UseVisualStyleBackColor = true;
            // 
            // chkFlags
            // 
            this.chkFlags.AutoSize = true;
            this.chkFlags.Location = new System.Drawing.Point(11, 130);
            this.chkFlags.Name = "chkFlags";
            this.chkFlags.Size = new System.Drawing.Size(15, 14);
            this.chkFlags.TabIndex = 33;
            this.chkFlags.UseVisualStyleBackColor = true;
            // 
            // chkAssembly
            // 
            this.chkAssembly.AutoSize = true;
            this.chkAssembly.Location = new System.Drawing.Point(11, 159);
            this.chkAssembly.Name = "chkAssembly";
            this.chkAssembly.Size = new System.Drawing.Size(15, 14);
            this.chkAssembly.TabIndex = 34;
            this.chkAssembly.UseVisualStyleBackColor = true;
            // 
            // chkType
            // 
            this.chkType.AutoSize = true;
            this.chkType.Location = new System.Drawing.Point(11, 188);
            this.chkType.Name = "chkType";
            this.chkType.Size = new System.Drawing.Size(15, 14);
            this.chkType.TabIndex = 35;
            this.chkType.UseVisualStyleBackColor = true;
            // 
            // chkRelatedType
            // 
            this.chkRelatedType.AutoSize = true;
            this.chkRelatedType.Location = new System.Drawing.Point(11, 217);
            this.chkRelatedType.Name = "chkRelatedType";
            this.chkRelatedType.Size = new System.Drawing.Size(15, 14);
            this.chkRelatedType.TabIndex = 36;
            this.chkRelatedType.UseVisualStyleBackColor = true;
            // 
            // chkStackDetails
            // 
            this.chkStackDetails.AutoSize = true;
            this.chkStackDetails.Location = new System.Drawing.Point(11, 246);
            this.chkStackDetails.Name = "chkStackDetails";
            this.chkStackDetails.Size = new System.Drawing.Size(15, 14);
            this.chkStackDetails.TabIndex = 37;
            this.chkStackDetails.UseVisualStyleBackColor = true;
            // 
            // chkStackTrace
            // 
            this.chkStackTrace.AutoSize = true;
            this.chkStackTrace.Location = new System.Drawing.Point(11, 275);
            this.chkStackTrace.Name = "chkStackTrace";
            this.chkStackTrace.Size = new System.Drawing.Size(15, 14);
            this.chkStackTrace.TabIndex = 38;
            this.chkStackTrace.UseVisualStyleBackColor = true;
            // 
            // chkEventName
            // 
            this.chkEventName.AutoSize = true;
            this.chkEventName.Location = new System.Drawing.Point(11, 304);
            this.chkEventName.Name = "chkEventName";
            this.chkEventName.Size = new System.Drawing.Size(15, 14);
            this.chkEventName.TabIndex = 39;
            this.chkEventName.UseVisualStyleBackColor = true;
            // 
            // chkMessage
            // 
            this.chkMessage.AutoSize = true;
            this.chkMessage.Location = new System.Drawing.Point(11, 333);
            this.chkMessage.Name = "chkMessage";
            this.chkMessage.Size = new System.Drawing.Size(15, 14);
            this.chkMessage.TabIndex = 40;
            this.chkMessage.UseVisualStyleBackColor = true;
            // 
            // chkDetails
            // 
            this.chkDetails.AutoSize = true;
            this.chkDetails.Location = new System.Drawing.Point(11, 362);
            this.chkDetails.Name = "chkDetails";
            this.chkDetails.Size = new System.Drawing.Size(15, 14);
            this.chkDetails.TabIndex = 41;
            this.chkDetails.UseVisualStyleBackColor = true;
            // 
            // chkException
            // 
            this.chkException.AutoSize = true;
            this.chkException.Location = new System.Drawing.Point(11, 391);
            this.chkException.Name = "chkException";
            this.chkException.Size = new System.Drawing.Size(15, 14);
            this.chkException.TabIndex = 42;
            this.chkException.UseVisualStyleBackColor = true;
            // 
            // BtnOK
            // 
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOK.Location = new System.Drawing.Point(275, 423);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 43;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(356, 423);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 44;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnLevel
            // 
            this.btnLevel.Location = new System.Drawing.Point(32, 96);
            this.btnLevel.Name = "btnLevel";
            this.btnLevel.Size = new System.Drawing.Size(78, 23);
            this.btnLevel.TabIndex = 45;
            this.btnLevel.Text = "Level";
            this.btnLevel.UseVisualStyleBackColor = true;
            // 
            // btnFlags
            // 
            this.btnFlags.Location = new System.Drawing.Point(32, 125);
            this.btnFlags.Name = "btnFlags";
            this.btnFlags.Size = new System.Drawing.Size(78, 23);
            this.btnFlags.TabIndex = 46;
            this.btnFlags.Text = "Flags";
            this.btnFlags.UseVisualStyleBackColor = true;
            // 
            // btnAssembly
            // 
            this.btnAssembly.Location = new System.Drawing.Point(32, 154);
            this.btnAssembly.Name = "btnAssembly";
            this.btnAssembly.Size = new System.Drawing.Size(78, 23);
            this.btnAssembly.TabIndex = 47;
            this.btnAssembly.Text = "Assembly";
            this.btnAssembly.UseVisualStyleBackColor = true;
            this.btnAssembly.Click += new System.EventHandler(this.btnAssembly_Click);
            // 
            // btnRelatedType
            // 
            this.btnRelatedType.Location = new System.Drawing.Point(32, 212);
            this.btnRelatedType.Name = "btnRelatedType";
            this.btnRelatedType.Size = new System.Drawing.Size(78, 23);
            this.btnRelatedType.TabIndex = 48;
            this.btnRelatedType.Text = "Related Type";
            this.btnRelatedType.UseVisualStyleBackColor = true;
            this.btnRelatedType.Click += new System.EventHandler(this.btnRelatedType_Click);
            // 
            // FilterEditorDialog
            // 
            this.AcceptButton = this.BtnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(450, 458);
            this.Controls.Add(this.btnRelatedType);
            this.Controls.Add(this.btnAssembly);
            this.Controls.Add(this.btnFlags);
            this.Controls.Add(this.btnLevel);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.chkException);
            this.Controls.Add(this.chkDetails);
            this.Controls.Add(this.chkMessage);
            this.Controls.Add(this.chkEventName);
            this.Controls.Add(this.chkStackTrace);
            this.Controls.Add(this.chkStackDetails);
            this.Controls.Add(this.chkRelatedType);
            this.Controls.Add(this.chkType);
            this.Controls.Add(this.chkAssembly);
            this.Controls.Add(this.chkFlags);
            this.Controls.Add(this.chkLevel);
            this.Controls.Add(this.chkClass);
            this.Controls.Add(this.chkTime);
            this.Controls.Add(this.lblException);
            this.Controls.Add(this.btnException);
            this.Controls.Add(this.lblDetails);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.benMessage);
            this.Controls.Add(this.lblEventName);
            this.Controls.Add(this.btnEventName);
            this.Controls.Add(this.lblStackTrace);
            this.Controls.Add(this.btnStackTrace);
            this.Controls.Add(this.lblStackDetails);
            this.Controls.Add(this.btnStackDetails);
            this.Controls.Add(this.lblRelatedType);
            this.Controls.Add(this.lblAssembly);
            this.Controls.Add(this.btnType);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblFlags);
            this.Controls.Add(this.lblLevel);
            this.Controls.Add(this.lblClass);
            this.Controls.Add(this.btnClass);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.btnTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbVerbose);
            this.Name = "FilterEditorDialog";
            this.Text = "Filter Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox cmbVerbose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTime;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblClass;
        private System.Windows.Forms.Button btnClass;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblFlags;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblAssembly;
        private System.Windows.Forms.Button btnType;
        private System.Windows.Forms.Label lblRelatedType;
        private System.Windows.Forms.Label lblStackDetails;
        private System.Windows.Forms.Button btnStackDetails;
        private System.Windows.Forms.Label lblStackTrace;
        private System.Windows.Forms.Button btnStackTrace;
        private System.Windows.Forms.Label lblEventName;
        private System.Windows.Forms.Button btnEventName;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button benMessage;
        private System.Windows.Forms.Label lblDetails;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Label lblException;
        private System.Windows.Forms.Button btnException;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button btnLevel;
        private System.Windows.Forms.Button btnFlags;
        private System.Windows.Forms.Button btnAssembly;
        private System.Windows.Forms.Button btnRelatedType;
        internal System.Windows.Forms.CheckBox chkTime;
        internal System.Windows.Forms.CheckBox chkClass;
        internal System.Windows.Forms.CheckBox chkLevel;
        internal System.Windows.Forms.CheckBox chkFlags;
        internal System.Windows.Forms.CheckBox chkAssembly;
        internal System.Windows.Forms.CheckBox chkType;
        internal System.Windows.Forms.CheckBox chkRelatedType;
        internal System.Windows.Forms.CheckBox chkStackDetails;
        internal System.Windows.Forms.CheckBox chkStackTrace;
        internal System.Windows.Forms.CheckBox chkEventName;
        internal System.Windows.Forms.CheckBox chkMessage;
        internal System.Windows.Forms.CheckBox chkDetails;
        internal System.Windows.Forms.CheckBox chkException;
    }
}