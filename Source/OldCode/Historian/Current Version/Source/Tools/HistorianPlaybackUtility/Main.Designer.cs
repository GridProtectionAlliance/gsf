namespace HistorianPlaybackUtility
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
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBoxTransmit = new System.Windows.Forms.GroupBox();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.radioButtonPlaybackFullspeed = new System.Windows.Forms.RadioButton();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.labelServer = new System.Windows.Forms.Label();
            this.comboBoxProtocol = new System.Windows.Forms.ComboBox();
            this.labelProtocol = new System.Windows.Forms.Label();
            this.textBoxPlaybackSampleRate = new System.Windows.Forms.TextBox();
            this.radioButtonPlaybackControlled = new System.Windows.Forms.RadioButton();
            this.groupBoxFiles = new System.Windows.Forms.GroupBox();
            this.linkLabelSecondaryArchive = new System.Windows.Forms.LinkLabel();
            this.textBoxSecondaryArchive = new System.Windows.Forms.TextBox();
            this.labelSecondaryArchive = new System.Windows.Forms.Label();
            this.linkLabelPrimaryArchive = new System.Windows.Forms.LinkLabel();
            this.textBoxPrimaryArchive = new System.Windows.Forms.TextBox();
            this.labelPrimaryArchive = new System.Windows.Forms.Label();
            this.groupBoxData = new System.Windows.Forms.GroupBox();
            this.linkLabelFind = new System.Windows.Forms.LinkLabel();
            this.linkLabelClear = new System.Windows.Forms.LinkLabel();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.dateTimePickerEndTime = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerStartTime = new System.Windows.Forms.DateTimePicker();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.checkedListBoxPointIDs = new System.Windows.Forms.CheckedListBox();
            this.groupBoxMessages = new System.Windows.Forms.GroupBox();
            this.textBoxMessages = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBoxTransmit.SuspendLayout();
            this.groupBoxFiles.SuspendLayout();
            this.groupBoxData.SuspendLayout();
            this.groupBoxMessages.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxMessages);
            this.splitContainer1.Panel2.Controls.Add(this.buttonStart);
            this.splitContainer1.Panel2.Controls.Add(this.buttonStop);
            this.splitContainer1.Size = new System.Drawing.Size(692, 566);
            this.splitContainer1.SplitterDistance = 277;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBoxTransmit);
            this.splitContainer2.Panel1.Controls.Add(this.groupBoxFiles);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBoxData);
            this.splitContainer2.Size = new System.Drawing.Size(692, 277);
            this.splitContainer2.SplitterDistance = 346;
            this.splitContainer2.TabIndex = 0;
            // 
            // groupBoxTransmit
            // 
            this.groupBoxTransmit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTransmit.Controls.Add(this.checkBoxRepeat);
            this.groupBoxTransmit.Controls.Add(this.radioButtonPlaybackFullspeed);
            this.groupBoxTransmit.Controls.Add(this.textBoxPort);
            this.groupBoxTransmit.Controls.Add(this.labelPort);
            this.groupBoxTransmit.Controls.Add(this.textBoxServer);
            this.groupBoxTransmit.Controls.Add(this.labelServer);
            this.groupBoxTransmit.Controls.Add(this.comboBoxProtocol);
            this.groupBoxTransmit.Controls.Add(this.labelProtocol);
            this.groupBoxTransmit.Controls.Add(this.textBoxPlaybackSampleRate);
            this.groupBoxTransmit.Controls.Add(this.radioButtonPlaybackControlled);
            this.groupBoxTransmit.Location = new System.Drawing.Point(12, 131);
            this.groupBoxTransmit.Name = "groupBoxTransmit";
            this.groupBoxTransmit.Size = new System.Drawing.Size(331, 140);
            this.groupBoxTransmit.TabIndex = 1002;
            this.groupBoxTransmit.TabStop = false;
            this.groupBoxTransmit.Text = "Transmit";
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Location = new System.Drawing.Point(9, 111);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(226, 17);
            this.checkBoxRepeat.TabIndex = 17;
            this.checkBoxRepeat.Text = "Repeat transmission of data until stopped";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            // 
            // radioButtonPlaybackFullspeed
            // 
            this.radioButtonPlaybackFullspeed.AutoSize = true;
            this.radioButtonPlaybackFullspeed.Checked = true;
            this.radioButtonPlaybackFullspeed.Location = new System.Drawing.Point(9, 65);
            this.radioButtonPlaybackFullspeed.Name = "radioButtonPlaybackFullspeed";
            this.radioButtonPlaybackFullspeed.Size = new System.Drawing.Size(182, 17);
            this.radioButtonPlaybackFullspeed.TabIndex = 14;
            this.radioButtonPlaybackFullspeed.TabStop = true;
            this.radioButtonPlaybackFullspeed.Text = "Transmit data as fast as possible";
            this.radioButtonPlaybackFullspeed.UseVisualStyleBackColor = true;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(196, 34);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(50, 21);
            this.textBoxPort.TabIndex = 13;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(193, 18);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(31, 13);
            this.labelPort.TabIndex = 0;
            this.labelPort.Text = "Port:";
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(78, 34);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(100, 21);
            this.textBoxServer.TabIndex = 12;
            // 
            // labelServer
            // 
            this.labelServer.AutoSize = true;
            this.labelServer.Location = new System.Drawing.Point(75, 18);
            this.labelServer.Name = "labelServer";
            this.labelServer.Size = new System.Drawing.Size(43, 13);
            this.labelServer.TabIndex = 0;
            this.labelServer.Text = "Server:";
            // 
            // comboBoxProtocol
            // 
            this.comboBoxProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProtocol.FormattingEnabled = true;
            this.comboBoxProtocol.Location = new System.Drawing.Point(9, 34);
            this.comboBoxProtocol.Name = "comboBoxProtocol";
            this.comboBoxProtocol.Size = new System.Drawing.Size(50, 21);
            this.comboBoxProtocol.TabIndex = 11;
            // 
            // labelProtocol
            // 
            this.labelProtocol.AutoSize = true;
            this.labelProtocol.Location = new System.Drawing.Point(6, 18);
            this.labelProtocol.Name = "labelProtocol";
            this.labelProtocol.Size = new System.Drawing.Size(50, 13);
            this.labelProtocol.TabIndex = 0;
            this.labelProtocol.Text = "Protocol:";
            // 
            // textBoxPlaybackSampleRate
            // 
            this.textBoxPlaybackSampleRate.Location = new System.Drawing.Point(111, 86);
            this.textBoxPlaybackSampleRate.Name = "textBoxPlaybackSampleRate";
            this.textBoxPlaybackSampleRate.Size = new System.Drawing.Size(30, 21);
            this.textBoxPlaybackSampleRate.TabIndex = 16;
            // 
            // radioButtonPlaybackControlled
            // 
            this.radioButtonPlaybackControlled.AutoSize = true;
            this.radioButtonPlaybackControlled.Location = new System.Drawing.Point(9, 88);
            this.radioButtonPlaybackControlled.Name = "radioButtonPlaybackControlled";
            this.radioButtonPlaybackControlled.Size = new System.Drawing.Size(237, 17);
            this.radioButtonPlaybackControlled.TabIndex = 15;
            this.radioButtonPlaybackControlled.Text = "Transmit data at             samples per second";
            this.radioButtonPlaybackControlled.UseVisualStyleBackColor = true;
            // 
            // groupBoxFiles
            // 
            this.groupBoxFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFiles.Controls.Add(this.linkLabelSecondaryArchive);
            this.groupBoxFiles.Controls.Add(this.textBoxSecondaryArchive);
            this.groupBoxFiles.Controls.Add(this.labelSecondaryArchive);
            this.groupBoxFiles.Controls.Add(this.linkLabelPrimaryArchive);
            this.groupBoxFiles.Controls.Add(this.textBoxPrimaryArchive);
            this.groupBoxFiles.Controls.Add(this.labelPrimaryArchive);
            this.groupBoxFiles.Location = new System.Drawing.Point(12, 12);
            this.groupBoxFiles.Name = "groupBoxFiles";
            this.groupBoxFiles.Size = new System.Drawing.Size(331, 112);
            this.groupBoxFiles.TabIndex = 1000;
            this.groupBoxFiles.TabStop = false;
            this.groupBoxFiles.Text = "Files";
            // 
            // linkLabelSecondaryArchive
            // 
            this.linkLabelSecondaryArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelSecondaryArchive.AutoSize = true;
            this.linkLabelSecondaryArchive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelSecondaryArchive.Location = new System.Drawing.Point(285, 65);
            this.linkLabelSecondaryArchive.Name = "linkLabelSecondaryArchive";
            this.linkLabelSecondaryArchive.Size = new System.Drawing.Size(42, 13);
            this.linkLabelSecondaryArchive.TabIndex = 4;
            this.linkLabelSecondaryArchive.TabStop = true;
            this.linkLabelSecondaryArchive.Text = "Browse";
            this.linkLabelSecondaryArchive.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSecondaryArchive_LinkClicked);
            // 
            // textBoxSecondaryArchive
            // 
            this.textBoxSecondaryArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSecondaryArchive.Location = new System.Drawing.Point(8, 81);
            this.textBoxSecondaryArchive.Name = "textBoxSecondaryArchive";
            this.textBoxSecondaryArchive.Size = new System.Drawing.Size(316, 21);
            this.textBoxSecondaryArchive.TabIndex = 3;
            this.textBoxSecondaryArchive.TextChanged += new System.EventHandler(this.textBoxSecondaryArchive_TextChanged);
            // 
            // labelSecondaryArchive
            // 
            this.labelSecondaryArchive.AutoSize = true;
            this.labelSecondaryArchive.Location = new System.Drawing.Point(6, 65);
            this.labelSecondaryArchive.Name = "labelSecondaryArchive";
            this.labelSecondaryArchive.Size = new System.Drawing.Size(144, 13);
            this.labelSecondaryArchive.TabIndex = 0;
            this.labelSecondaryArchive.Text = "Secondary Archive Location:";
            // 
            // linkLabelPrimaryArchive
            // 
            this.linkLabelPrimaryArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelPrimaryArchive.AutoSize = true;
            this.linkLabelPrimaryArchive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelPrimaryArchive.Location = new System.Drawing.Point(285, 17);
            this.linkLabelPrimaryArchive.Name = "linkLabelPrimaryArchive";
            this.linkLabelPrimaryArchive.Size = new System.Drawing.Size(42, 13);
            this.linkLabelPrimaryArchive.TabIndex = 2;
            this.linkLabelPrimaryArchive.TabStop = true;
            this.linkLabelPrimaryArchive.Text = "Browse";
            this.linkLabelPrimaryArchive.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPrimaryArchive_LinkClicked);
            // 
            // textBoxPrimaryArchive
            // 
            this.textBoxPrimaryArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPrimaryArchive.Location = new System.Drawing.Point(9, 33);
            this.textBoxPrimaryArchive.Name = "textBoxPrimaryArchive";
            this.textBoxPrimaryArchive.Size = new System.Drawing.Size(315, 21);
            this.textBoxPrimaryArchive.TabIndex = 1;
            this.textBoxPrimaryArchive.TextChanged += new System.EventHandler(this.textBoxPrimaryArchive_TextChanged);
            // 
            // labelPrimaryArchive
            // 
            this.labelPrimaryArchive.AutoSize = true;
            this.labelPrimaryArchive.Location = new System.Drawing.Point(6, 17);
            this.labelPrimaryArchive.Name = "labelPrimaryArchive";
            this.labelPrimaryArchive.Size = new System.Drawing.Size(129, 13);
            this.labelPrimaryArchive.TabIndex = 0;
            this.labelPrimaryArchive.Text = "Primary Archive Location:";
            // 
            // groupBoxData
            // 
            this.groupBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxData.Controls.Add(this.linkLabelFind);
            this.groupBoxData.Controls.Add(this.linkLabelClear);
            this.groupBoxData.Controls.Add(this.textBoxSearch);
            this.groupBoxData.Controls.Add(this.dateTimePickerEndTime);
            this.groupBoxData.Controls.Add(this.dateTimePickerStartTime);
            this.groupBoxData.Controls.Add(this.labelEndTime);
            this.groupBoxData.Controls.Add(this.labelStartTime);
            this.groupBoxData.Controls.Add(this.checkedListBoxPointIDs);
            this.groupBoxData.Location = new System.Drawing.Point(3, 12);
            this.groupBoxData.Name = "groupBoxData";
            this.groupBoxData.Size = new System.Drawing.Size(328, 259);
            this.groupBoxData.TabIndex = 1001;
            this.groupBoxData.TabStop = false;
            this.groupBoxData.Text = "Data";
            // 
            // linkLabelFind
            // 
            this.linkLabelFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelFind.AutoSize = true;
            this.linkLabelFind.Location = new System.Drawing.Point(229, 17);
            this.linkLabelFind.Name = "linkLabelFind";
            this.linkLabelFind.Size = new System.Drawing.Size(41, 13);
            this.linkLabelFind.TabIndex = 6;
            this.linkLabelFind.TabStop = true;
            this.linkLabelFind.Text = "Find All";
            this.linkLabelFind.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelFind_LinkClicked);
            // 
            // linkLabelClear
            // 
            this.linkLabelClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelClear.AutoSize = true;
            this.linkLabelClear.Location = new System.Drawing.Point(276, 17);
            this.linkLabelClear.Name = "linkLabelClear";
            this.linkLabelClear.Size = new System.Drawing.Size(46, 13);
            this.linkLabelClear.TabIndex = 7;
            this.linkLabelClear.TabStop = true;
            this.linkLabelClear.Text = "Clear All";
            this.linkLabelClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelClear_LinkClicked);
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearch.Location = new System.Drawing.Point(9, 14);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(214, 21);
            this.textBoxSearch.TabIndex = 5;
            this.textBoxSearch.TextChanged += new System.EventHandler(this.textBoxSearch_TextChanged);
            this.textBoxSearch.Leave += new System.EventHandler(this.textBoxSearch_Leave);
            this.textBoxSearch.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxSearch_MouseClick);
            // 
            // dateTimePickerEndTime
            // 
            this.dateTimePickerEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerEndTime.CustomFormat = "MM/dd/yyyy HH:mm:ss tt";
            this.dateTimePickerEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEndTime.Location = new System.Drawing.Point(174, 225);
            this.dateTimePickerEndTime.Name = "dateTimePickerEndTime";
            this.dateTimePickerEndTime.Size = new System.Drawing.Size(145, 21);
            this.dateTimePickerEndTime.TabIndex = 10;
            // 
            // dateTimePickerStartTime
            // 
            this.dateTimePickerStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dateTimePickerStartTime.CustomFormat = "MM/dd/yyyy HH:mm:ss tt";
            this.dateTimePickerStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerStartTime.Location = new System.Drawing.Point(9, 225);
            this.dateTimePickerStartTime.Name = "dateTimePickerStartTime";
            this.dateTimePickerStartTime.Size = new System.Drawing.Size(145, 21);
            this.dateTimePickerStartTime.TabIndex = 9;
            // 
            // labelEndTime
            // 
            this.labelEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(171, 209);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(97, 13);
            this.labelEndTime.TabIndex = 0;
            this.labelEndTime.Text = "End Time (in GMT):";
            // 
            // labelStartTime
            // 
            this.labelStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(6, 209);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(103, 13);
            this.labelStartTime.TabIndex = 0;
            this.labelStartTime.Text = "Start Time (in GMT):";
            // 
            // checkedListBoxPointIDs
            // 
            this.checkedListBoxPointIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxPointIDs.CheckOnClick = true;
            this.checkedListBoxPointIDs.FormattingEnabled = true;
            this.checkedListBoxPointIDs.HorizontalScrollbar = true;
            this.checkedListBoxPointIDs.Location = new System.Drawing.Point(9, 42);
            this.checkedListBoxPointIDs.Name = "checkedListBoxPointIDs";
            this.checkedListBoxPointIDs.Size = new System.Drawing.Size(313, 164);
            this.checkedListBoxPointIDs.TabIndex = 8;
            // 
            // groupBoxMessages
            // 
            this.groupBoxMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMessages.Controls.Add(this.textBoxMessages);
            this.groupBoxMessages.Location = new System.Drawing.Point(12, 3);
            this.groupBoxMessages.Name = "groupBoxMessages";
            this.groupBoxMessages.Size = new System.Drawing.Size(669, 241);
            this.groupBoxMessages.TabIndex = 1004;
            this.groupBoxMessages.TabStop = false;
            this.groupBoxMessages.Text = "Messages";
            // 
            // textBoxMessages
            // 
            this.textBoxMessages.BackColor = System.Drawing.SystemColors.WindowText;
            this.textBoxMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxMessages.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxMessages.ForeColor = System.Drawing.SystemColors.Window;
            this.textBoxMessages.Location = new System.Drawing.Point(3, 17);
            this.textBoxMessages.Multiline = true;
            this.textBoxMessages.Name = "textBoxMessages";
            this.textBoxMessages.ReadOnly = true;
            this.textBoxMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessages.Size = new System.Drawing.Size(663, 221);
            this.textBoxMessages.TabIndex = 0;
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(603, 250);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 17;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.Location = new System.Drawing.Point(603, 250);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 17;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 566);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Historian Playback Utility";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.groupBoxTransmit.ResumeLayout(false);
            this.groupBoxTransmit.PerformLayout();
            this.groupBoxFiles.ResumeLayout(false);
            this.groupBoxFiles.PerformLayout();
            this.groupBoxData.ResumeLayout(false);
            this.groupBoxData.PerformLayout();
            this.groupBoxMessages.ResumeLayout(false);
            this.groupBoxMessages.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBoxMessages;
        private System.Windows.Forms.TextBox textBoxMessages;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.GroupBox groupBoxTransmit;
        private System.Windows.Forms.TextBox textBoxPlaybackSampleRate;
        private System.Windows.Forms.RadioButton radioButtonPlaybackControlled;
        private System.Windows.Forms.RadioButton radioButtonPlaybackFullspeed;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.ComboBox comboBoxProtocol;
        private System.Windows.Forms.Label labelProtocol;
        private System.Windows.Forms.GroupBox groupBoxFiles;
        private System.Windows.Forms.LinkLabel linkLabelSecondaryArchive;
        private System.Windows.Forms.TextBox textBoxSecondaryArchive;
        private System.Windows.Forms.Label labelSecondaryArchive;
        private System.Windows.Forms.LinkLabel linkLabelPrimaryArchive;
        private System.Windows.Forms.TextBox textBoxPrimaryArchive;
        private System.Windows.Forms.Label labelPrimaryArchive;
        private System.Windows.Forms.GroupBox groupBoxData;
        private System.Windows.Forms.LinkLabel linkLabelFind;
        private System.Windows.Forms.LinkLabel linkLabelClear;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.CheckedListBox checkedListBoxPointIDs;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndTime;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartTime;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
    }
}

