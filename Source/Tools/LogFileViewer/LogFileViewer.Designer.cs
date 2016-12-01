namespace LogFileViewer
{
    partial class LogFileViewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogFileViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.chkNormal = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkHighlight4 = new System.Windows.Forms.CheckBox();
            this.chkHighlight3 = new System.Windows.Forms.CheckBox();
            this.chkHighlight2 = new System.Windows.Forms.CheckBox();
            this.chkHighlight1 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkExclude4 = new System.Windows.Forms.CheckBox();
            this.chkExclude3 = new System.Windows.Forms.CheckBox();
            this.chkExclude2 = new System.Windows.Forms.CheckBox();
            this.chkExclude1 = new System.Windows.Forms.CheckBox();
            this.LstFilters = new System.Windows.Forms.ListBox();
            this.cmsFilters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overwriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSaveSelected = new System.Windows.Forms.Button();
            this.btnFilteredLoad = new System.Windows.Forms.Button();
            this.btnCompactFiles = new System.Windows.Forms.Button();
            this.BtnLoad = new System.Windows.Forms.Button();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.cmsFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.chkNormal);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.LstFilters);
            this.splitContainer1.Panel1.Controls.Add(this.btnSaveSelected);
            this.splitContainer1.Panel1.Controls.Add(this.btnFilteredLoad);
            this.splitContainer1.Panel1.Controls.Add(this.btnCompactFiles);
            this.splitContainer1.Panel1.Controls.Add(this.BtnLoad);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgvResults);
            this.splitContainer1.Size = new System.Drawing.Size(995, 657);
            this.splitContainer1.SplitterDistance = 157;
            this.splitContainer1.TabIndex = 1;
            // 
            // chkNormal
            // 
            this.chkNormal.AutoSize = true;
            this.chkNormal.Checked = true;
            this.chkNormal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNormal.Location = new System.Drawing.Point(74, 129);
            this.chkNormal.Name = "chkNormal";
            this.chkNormal.Size = new System.Drawing.Size(105, 17);
            this.chkNormal.TabIndex = 12;
            this.chkNormal.Text = "Normal Message";
            this.chkNormal.UseVisualStyleBackColor = true;
            this.chkNormal.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkHighlight4);
            this.groupBox2.Controls.Add(this.chkHighlight3);
            this.groupBox2.Controls.Add(this.chkHighlight2);
            this.groupBox2.Controls.Add(this.chkHighlight1);
            this.groupBox2.Location = new System.Drawing.Point(103, 58);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(85, 65);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Highlight";
            // 
            // chkHighlight4
            // 
            this.chkHighlight4.AutoSize = true;
            this.chkHighlight4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.chkHighlight4.Checked = true;
            this.chkHighlight4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlight4.Location = new System.Drawing.Point(44, 42);
            this.chkHighlight4.Name = "chkHighlight4";
            this.chkHighlight4.Size = new System.Drawing.Size(32, 17);
            this.chkHighlight4.TabIndex = 10;
            this.chkHighlight4.Text = "4";
            this.chkHighlight4.UseVisualStyleBackColor = false;
            this.chkHighlight4.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkHighlight3
            // 
            this.chkHighlight3.AutoSize = true;
            this.chkHighlight3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.chkHighlight3.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkHighlight3.Checked = true;
            this.chkHighlight3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlight3.Location = new System.Drawing.Point(6, 42);
            this.chkHighlight3.Name = "chkHighlight3";
            this.chkHighlight3.Size = new System.Drawing.Size(32, 17);
            this.chkHighlight3.TabIndex = 9;
            this.chkHighlight3.Text = "3";
            this.chkHighlight3.UseVisualStyleBackColor = false;
            this.chkHighlight3.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkHighlight2
            // 
            this.chkHighlight2.AutoSize = true;
            this.chkHighlight2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.chkHighlight2.Checked = true;
            this.chkHighlight2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlight2.Location = new System.Drawing.Point(44, 19);
            this.chkHighlight2.Name = "chkHighlight2";
            this.chkHighlight2.Size = new System.Drawing.Size(32, 17);
            this.chkHighlight2.TabIndex = 8;
            this.chkHighlight2.Text = "2";
            this.chkHighlight2.UseVisualStyleBackColor = false;
            this.chkHighlight2.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkHighlight1
            // 
            this.chkHighlight1.AutoSize = true;
            this.chkHighlight1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.chkHighlight1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkHighlight1.Checked = true;
            this.chkHighlight1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlight1.Location = new System.Drawing.Point(6, 19);
            this.chkHighlight1.Name = "chkHighlight1";
            this.chkHighlight1.Size = new System.Drawing.Size(32, 17);
            this.chkHighlight1.TabIndex = 7;
            this.chkHighlight1.Text = "1";
            this.chkHighlight1.UseVisualStyleBackColor = false;
            this.chkHighlight1.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkExclude4);
            this.groupBox1.Controls.Add(this.chkExclude3);
            this.groupBox1.Controls.Add(this.chkExclude2);
            this.groupBox1.Controls.Add(this.chkExclude1);
            this.groupBox1.Location = new System.Drawing.Point(12, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(85, 65);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Exclude";
            // 
            // chkExclude4
            // 
            this.chkExclude4.AutoSize = true;
            this.chkExclude4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.chkExclude4.Location = new System.Drawing.Point(44, 42);
            this.chkExclude4.Name = "chkExclude4";
            this.chkExclude4.Size = new System.Drawing.Size(32, 17);
            this.chkExclude4.TabIndex = 10;
            this.chkExclude4.Text = "4";
            this.chkExclude4.UseVisualStyleBackColor = false;
            this.chkExclude4.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkExclude3
            // 
            this.chkExclude3.AutoSize = true;
            this.chkExclude3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.chkExclude3.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkExclude3.Location = new System.Drawing.Point(6, 42);
            this.chkExclude3.Name = "chkExclude3";
            this.chkExclude3.Size = new System.Drawing.Size(32, 17);
            this.chkExclude3.TabIndex = 9;
            this.chkExclude3.Text = "3";
            this.chkExclude3.UseVisualStyleBackColor = false;
            this.chkExclude3.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkExclude2
            // 
            this.chkExclude2.AutoSize = true;
            this.chkExclude2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.chkExclude2.Location = new System.Drawing.Point(44, 19);
            this.chkExclude2.Name = "chkExclude2";
            this.chkExclude2.Size = new System.Drawing.Size(32, 17);
            this.chkExclude2.TabIndex = 8;
            this.chkExclude2.Text = "2";
            this.chkExclude2.UseVisualStyleBackColor = false;
            this.chkExclude2.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // chkExclude1
            // 
            this.chkExclude1.AutoSize = true;
            this.chkExclude1.BackColor = System.Drawing.Color.Silver;
            this.chkExclude1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkExclude1.Location = new System.Drawing.Point(6, 19);
            this.chkExclude1.Name = "chkExclude1";
            this.chkExclude1.Size = new System.Drawing.Size(32, 17);
            this.chkExclude1.TabIndex = 7;
            this.chkExclude1.Text = "1";
            this.chkExclude1.UseVisualStyleBackColor = false;
            this.chkExclude1.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // LstFilters
            // 
            this.LstFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LstFilters.ContextMenuStrip = this.cmsFilters;
            this.LstFilters.DisplayMember = "Description";
            this.LstFilters.FormattingEnabled = true;
            this.LstFilters.HorizontalScrollbar = true;
            this.LstFilters.Location = new System.Drawing.Point(194, 12);
            this.LstFilters.Name = "LstFilters";
            this.LstFilters.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstFilters.Size = new System.Drawing.Size(708, 134);
            this.LstFilters.TabIndex = 1;
            this.LstFilters.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LstFilters_KeyUp);
            // 
            // cmsFilters
            // 
            this.cmsFilters.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveAsToolStripMenuItem,
            this.overwriteToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.cmsFilters.Name = "cmsFilters";
            this.cmsFilters.Size = new System.Drawing.Size(126, 98);
            this.cmsFilters.Opening += new System.ComponentModel.CancelEventHandler(this.cmsFilters_Opening);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(122, 6);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // overwriteToolStripMenuItem
            // 
            this.overwriteToolStripMenuItem.Name = "overwriteToolStripMenuItem";
            this.overwriteToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.overwriteToolStripMenuItem.Text = "Overwrite";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // btnSaveSelected
            // 
            this.btnSaveSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSelected.Location = new System.Drawing.Point(908, 13);
            this.btnSaveSelected.Name = "btnSaveSelected";
            this.btnSaveSelected.Size = new System.Drawing.Size(75, 23);
            this.btnSaveSelected.TabIndex = 6;
            this.btnSaveSelected.Text = "Save As";
            this.btnSaveSelected.UseVisualStyleBackColor = true;
            this.btnSaveSelected.Click += new System.EventHandler(this.btnSaveSelected_Click);
            // 
            // btnFilteredLoad
            // 
            this.btnFilteredLoad.Location = new System.Drawing.Point(93, 13);
            this.btnFilteredLoad.Name = "btnFilteredLoad";
            this.btnFilteredLoad.Size = new System.Drawing.Size(75, 39);
            this.btnFilteredLoad.TabIndex = 5;
            this.btnFilteredLoad.Text = "Filtered Load";
            this.toolTip1.SetToolTip(this.btnFilteredLoad, "Uses the current filter to load the unfiltered logs of the selected files. This a" +
        "llows loading of many more log files at one time.");
            this.btnFilteredLoad.UseVisualStyleBackColor = true;
            this.btnFilteredLoad.Click += new System.EventHandler(this.btnFilteredLoad_Click);
            // 
            // btnCompactFiles
            // 
            this.btnCompactFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompactFiles.Location = new System.Drawing.Point(908, 42);
            this.btnCompactFiles.Name = "btnCompactFiles";
            this.btnCompactFiles.Size = new System.Drawing.Size(75, 23);
            this.btnCompactFiles.TabIndex = 4;
            this.btnCompactFiles.Text = "Compact";
            this.btnCompactFiles.UseVisualStyleBackColor = true;
            this.btnCompactFiles.Click += new System.EventHandler(this.BtnCompactFiles_Click);
            // 
            // BtnLoad
            // 
            this.BtnLoad.Location = new System.Drawing.Point(12, 12);
            this.BtnLoad.Name = "BtnLoad";
            this.BtnLoad.Size = new System.Drawing.Size(75, 23);
            this.BtnLoad.TabIndex = 0;
            this.BtnLoad.Text = "Load";
            this.toolTip1.SetToolTip(this.BtnLoad, "Loads all of the logs of all selected files.");
            this.BtnLoad.UseVisualStyleBackColor = true;
            this.BtnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(0, 0);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.Size = new System.Drawing.Size(995, 496);
            this.dgvResults.TabIndex = 0;
            this.dgvResults.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvResults_CellMouseClick);
            this.dgvResults.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvResults_CellMouseDoubleClick);
            this.dgvResults.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvResults_CellPainting);
            // 
            // LogFileViewer
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(995, 657);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 300);
            this.Name = "LogFileViewer";
            this.Text = "Log File Viewer";
            this.Load += new System.EventHandler(this.LogFileViewer_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LogFileViewer_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LogFileViewer_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.cmsFilters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnCompactFiles;
        private System.Windows.Forms.ListBox LstFilters;
        private System.Windows.Forms.Button BtnLoad;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Button btnFilteredLoad;
        private System.Windows.Forms.ContextMenuStrip cmsFilters;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem overwriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button btnSaveSelected;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkNormal;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkHighlight4;
        private System.Windows.Forms.CheckBox chkHighlight3;
        private System.Windows.Forms.CheckBox chkHighlight2;
        private System.Windows.Forms.CheckBox chkHighlight1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkExclude4;
        private System.Windows.Forms.CheckBox chkExclude3;
        private System.Windows.Forms.CheckBox chkExclude2;
        private System.Windows.Forms.CheckBox chkExclude1;
    }
}