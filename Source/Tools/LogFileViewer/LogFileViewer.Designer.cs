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
            this.BtnAddFilter = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RdoLowest = new System.Windows.Forms.RadioButton();
            this.RdoHighest = new System.Windows.Forms.RadioButton();
            this.RdoLow = new System.Windows.Forms.RadioButton();
            this.RdoNormal = new System.Windows.Forms.RadioButton();
            this.RdoAboveNormal = new System.Windows.Forms.RadioButton();
            this.RdoHigh = new System.Windows.Forms.RadioButton();
            this.RdoBelowNormal = new System.Windows.Forms.RadioButton();
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
            this.splitContainer1.Panel1.Controls.Add(this.BtnAddFilter);
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
            this.splitContainer1.SplitterDistance = 209;
            this.splitContainer1.TabIndex = 1;
            // 
            // BtnAddFilter
            // 
            this.BtnAddFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAddFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAddFilter.Location = new System.Drawing.Point(908, 169);
            this.BtnAddFilter.Name = "BtnAddFilter";
            this.BtnAddFilter.Size = new System.Drawing.Size(34, 29);
            this.BtnAddFilter.TabIndex = 14;
            this.BtnAddFilter.Text = "+";
            this.BtnAddFilter.UseVisualStyleBackColor = true;
            this.BtnAddFilter.Click += new System.EventHandler(this.BtnAddFilter_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RdoLowest);
            this.groupBox1.Controls.Add(this.RdoHighest);
            this.groupBox1.Controls.Add(this.RdoLow);
            this.groupBox1.Controls.Add(this.RdoNormal);
            this.groupBox1.Controls.Add(this.RdoAboveNormal);
            this.groupBox1.Controls.Add(this.RdoHigh);
            this.groupBox1.Controls.Add(this.RdoBelowNormal);
            this.groupBox1.Location = new System.Drawing.Point(93, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(109, 186);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Verbose Level";
            // 
            // RdoLowest
            // 
            this.RdoLowest.BackColor = System.Drawing.Color.Silver;
            this.RdoLowest.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoLowest.Location = new System.Drawing.Point(6, 19);
            this.RdoLowest.Name = "RdoLowest";
            this.RdoLowest.Size = new System.Drawing.Size(95, 17);
            this.RdoLowest.TabIndex = 7;
            this.RdoLowest.Text = "Lowest";
            this.RdoLowest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoLowest.UseVisualStyleBackColor = false;
            this.RdoLowest.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoHighest
            // 
            this.RdoHighest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.RdoHighest.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoHighest.Location = new System.Drawing.Point(6, 157);
            this.RdoHighest.Name = "RdoHighest";
            this.RdoHighest.Size = new System.Drawing.Size(95, 17);
            this.RdoHighest.TabIndex = 9;
            this.RdoHighest.Text = "Highest";
            this.RdoHighest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoHighest.UseVisualStyleBackColor = false;
            this.RdoHighest.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoLow
            // 
            this.RdoLow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.RdoLow.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoLow.Location = new System.Drawing.Point(6, 42);
            this.RdoLow.Name = "RdoLow";
            this.RdoLow.Size = new System.Drawing.Size(95, 17);
            this.RdoLow.TabIndex = 8;
            this.RdoLow.Text = "Low";
            this.RdoLow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoLow.UseVisualStyleBackColor = false;
            this.RdoLow.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoNormal
            // 
            this.RdoNormal.BackColor = System.Drawing.Color.White;
            this.RdoNormal.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoNormal.Checked = true;
            this.RdoNormal.Location = new System.Drawing.Point(6, 88);
            this.RdoNormal.Name = "RdoNormal";
            this.RdoNormal.Size = new System.Drawing.Size(95, 17);
            this.RdoNormal.TabIndex = 12;
            this.RdoNormal.TabStop = true;
            this.RdoNormal.Text = "Normal";
            this.RdoNormal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoNormal.UseVisualStyleBackColor = false;
            this.RdoNormal.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoAboveNormal
            // 
            this.RdoAboveNormal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.RdoAboveNormal.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoAboveNormal.Location = new System.Drawing.Point(6, 111);
            this.RdoAboveNormal.Name = "RdoAboveNormal";
            this.RdoAboveNormal.Size = new System.Drawing.Size(95, 17);
            this.RdoAboveNormal.TabIndex = 7;
            this.RdoAboveNormal.Text = "Above Normal";
            this.RdoAboveNormal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoAboveNormal.UseVisualStyleBackColor = false;
            this.RdoAboveNormal.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoHigh
            // 
            this.RdoHigh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.RdoHigh.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoHigh.Location = new System.Drawing.Point(6, 134);
            this.RdoHigh.Name = "RdoHigh";
            this.RdoHigh.Size = new System.Drawing.Size(95, 17);
            this.RdoHigh.TabIndex = 8;
            this.RdoHigh.Text = "High";
            this.RdoHigh.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoHigh.UseVisualStyleBackColor = false;
            this.RdoHigh.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
            // 
            // RdoBelowNormal
            // 
            this.RdoBelowNormal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.RdoBelowNormal.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoBelowNormal.Location = new System.Drawing.Point(6, 65);
            this.RdoBelowNormal.Name = "RdoBelowNormal";
            this.RdoBelowNormal.Size = new System.Drawing.Size(95, 17);
            this.RdoBelowNormal.TabIndex = 9;
            this.RdoBelowNormal.Text = "BelowNormal";
            this.RdoBelowNormal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.RdoBelowNormal.UseVisualStyleBackColor = false;
            this.RdoBelowNormal.CheckedChanged += new System.EventHandler(this.VisibleCheckedChanged);
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
            this.LstFilters.Location = new System.Drawing.Point(208, 12);
            this.LstFilters.Name = "LstFilters";
            this.LstFilters.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstFilters.Size = new System.Drawing.Size(694, 186);
            this.LstFilters.TabIndex = 1;
            this.LstFilters.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LstFilters_KeyUp);
            this.LstFilters.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstFilters_MouseDoubleClick);
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
            this.btnFilteredLoad.Location = new System.Drawing.Point(12, 42);
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
            this.dgvResults.Size = new System.Drawing.Size(995, 444);
            this.dgvResults.TabIndex = 0;
            this.dgvResults.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvResults_CellMouseClick);
            this.dgvResults.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvResults_CellMouseDoubleClick);
            this.dgvResults.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvResults_CellPainting);
            this.dgvResults.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgvResults_RowPrePaint);
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
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
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
        private System.Windows.Forms.RadioButton RdoNormal;
        private System.Windows.Forms.RadioButton RdoHighest;
        private System.Windows.Forms.RadioButton RdoHigh;
        private System.Windows.Forms.RadioButton RdoAboveNormal;
        private System.Windows.Forms.RadioButton RdoBelowNormal;
        private System.Windows.Forms.RadioButton RdoLow;
        private System.Windows.Forms.RadioButton RdoLowest;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnAddFilter;
    }
}