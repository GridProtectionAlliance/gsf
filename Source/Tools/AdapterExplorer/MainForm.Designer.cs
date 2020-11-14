
namespace AdapterExplorer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelHeader = new System.Windows.Forms.Panel();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.checkBoxOutputAdapters = new System.Windows.Forms.CheckBox();
            this.checkBoxInputAdapters = new System.Windows.Forms.CheckBox();
            this.checkBoxActionAdapters = new System.Windows.Forms.CheckBox();
            this.comboBoxAdapters = new System.Windows.Forms.ComboBox();
            this.labelAdapters = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.textBoxAdapterInfo = new System.Windows.Forms.TextBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.textBoxMessageOutput = new System.Windows.Forms.TextBox();
            this.splitContainerMeasurements = new System.Windows.Forms.SplitContainer();
            this.groupBoxInputMeasurements = new System.Windows.Forms.GroupBox();
            this.dataGridViewInputMeasurements = new System.Windows.Forms.DataGridView();
            this.groupBoxOutputMeasurements = new System.Windows.Forms.GroupBox();
            this.dataGridViewOutputMeasurements = new System.Windows.Forms.DataGridView();
            this.imageListStatus = new System.Windows.Forms.ImageList(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMeasurements)).BeginInit();
            this.splitContainerMeasurements.Panel1.SuspendLayout();
            this.splitContainerMeasurements.Panel2.SuspendLayout();
            this.splitContainerMeasurements.SuspendLayout();
            this.groupBoxInputMeasurements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInputMeasurements)).BeginInit();
            this.groupBoxOutputMeasurements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutputMeasurements)).BeginInit();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.pictureBoxStatus);
            this.panelHeader.Controls.Add(this.checkBoxOutputAdapters);
            this.panelHeader.Controls.Add(this.checkBoxInputAdapters);
            this.panelHeader.Controls.Add(this.checkBoxActionAdapters);
            this.panelHeader.Controls.Add(this.comboBoxAdapters);
            this.panelHeader.Controls.Add(this.labelAdapters);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(842, 44);
            this.panelHeader.TabIndex = 0;
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxStatus.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStatus.Image")));
            this.pictureBoxStatus.Location = new System.Drawing.Point(811, 12);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(28, 21);
            this.pictureBoxStatus.TabIndex = 5;
            this.pictureBoxStatus.TabStop = false;
            this.toolTip.SetToolTip(this.pictureBoxStatus, "Disconnected");
            // 
            // checkBoxOutputAdapters
            // 
            this.checkBoxOutputAdapters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxOutputAdapters.AutoSize = true;
            this.checkBoxOutputAdapters.Location = new System.Drawing.Point(705, 14);
            this.checkBoxOutputAdapters.Name = "checkBoxOutputAdapters";
            this.checkBoxOutputAdapters.Size = new System.Drawing.Size(103, 17);
            this.checkBoxOutputAdapters.TabIndex = 4;
            this.checkBoxOutputAdapters.Text = "Output Adapters";
            this.checkBoxOutputAdapters.UseVisualStyleBackColor = true;
            this.checkBoxOutputAdapters.CheckedChanged += new System.EventHandler(this.checkBoxAdapters_CheckedChanged);
            // 
            // checkBoxInputAdapters
            // 
            this.checkBoxInputAdapters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxInputAdapters.AutoSize = true;
            this.checkBoxInputAdapters.Location = new System.Drawing.Point(603, 14);
            this.checkBoxInputAdapters.Name = "checkBoxInputAdapters";
            this.checkBoxInputAdapters.Size = new System.Drawing.Size(95, 17);
            this.checkBoxInputAdapters.TabIndex = 3;
            this.checkBoxInputAdapters.Text = "Input Adapters";
            this.checkBoxInputAdapters.UseVisualStyleBackColor = true;
            this.checkBoxInputAdapters.CheckedChanged += new System.EventHandler(this.checkBoxAdapters_CheckedChanged);
            // 
            // checkBoxActionAdapters
            // 
            this.checkBoxActionAdapters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxActionAdapters.AutoSize = true;
            this.checkBoxActionAdapters.Checked = true;
            this.checkBoxActionAdapters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxActionAdapters.Location = new System.Drawing.Point(496, 14);
            this.checkBoxActionAdapters.Name = "checkBoxActionAdapters";
            this.checkBoxActionAdapters.Size = new System.Drawing.Size(101, 17);
            this.checkBoxActionAdapters.TabIndex = 2;
            this.checkBoxActionAdapters.Text = "Action Adapters";
            this.checkBoxActionAdapters.UseVisualStyleBackColor = true;
            this.checkBoxActionAdapters.CheckedChanged += new System.EventHandler(this.checkBoxAdapters_CheckedChanged);
            // 
            // comboBoxAdapters
            // 
            this.comboBoxAdapters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxAdapters.DisplayMember = "AdapterName";
            this.comboBoxAdapters.FormattingEnabled = true;
            this.comboBoxAdapters.Location = new System.Drawing.Point(65, 12);
            this.comboBoxAdapters.Name = "comboBoxAdapters";
            this.comboBoxAdapters.Size = new System.Drawing.Size(415, 21);
            this.comboBoxAdapters.TabIndex = 1;
            this.comboBoxAdapters.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdapters_SelectedIndexChanged);
            // 
            // labelAdapters
            // 
            this.labelAdapters.AutoSize = true;
            this.labelAdapters.Location = new System.Drawing.Point(12, 15);
            this.labelAdapters.Name = "labelAdapters";
            this.labelAdapters.Size = new System.Drawing.Size(47, 13);
            this.labelAdapters.TabIndex = 0;
            this.labelAdapters.Text = "Adapter:";
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.textBoxAdapterInfo);
            this.panelFooter.Controls.Add(this.buttonClear);
            this.panelFooter.Controls.Add(this.textBoxMessageOutput);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 502);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(842, 100);
            this.panelFooter.TabIndex = 1;
            // 
            // textBoxAdapterInfo
            // 
            this.textBoxAdapterInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.textBoxAdapterInfo.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxAdapterInfo.Location = new System.Drawing.Point(0, 0);
            this.textBoxAdapterInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAdapterInfo.Multiline = true;
            this.textBoxAdapterInfo.Name = "textBoxAdapterInfo";
            this.textBoxAdapterInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxAdapterInfo.Size = new System.Drawing.Size(419, 100);
            this.textBoxAdapterInfo.TabIndex = 3;
            this.textBoxAdapterInfo.Text = "Adapter Info: <no adapter selected>\r\n\r\nConnection String:";
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Font = new System.Drawing.Font("Consolas", 6.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClear.Location = new System.Drawing.Point(805, 2);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(0);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(18, 18);
            this.buttonClear.TabIndex = 2;
            this.buttonClear.Text = "X";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // textBoxMessageOutput
            // 
            this.textBoxMessageOutput.BackColor = System.Drawing.SystemColors.WindowText;
            this.textBoxMessageOutput.Dock = System.Windows.Forms.DockStyle.Right;
            this.textBoxMessageOutput.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxMessageOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.textBoxMessageOutput.Location = new System.Drawing.Point(421, 0);
            this.textBoxMessageOutput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxMessageOutput.Multiline = true;
            this.textBoxMessageOutput.Name = "textBoxMessageOutput";
            this.textBoxMessageOutput.ReadOnly = true;
            this.textBoxMessageOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessageOutput.Size = new System.Drawing.Size(421, 100);
            this.textBoxMessageOutput.TabIndex = 1;
            this.textBoxMessageOutput.TabStop = false;
            // 
            // splitContainerMeasurements
            // 
            this.splitContainerMeasurements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMeasurements.Location = new System.Drawing.Point(0, 44);
            this.splitContainerMeasurements.Name = "splitContainerMeasurements";
            // 
            // splitContainerMeasurements.Panel1
            // 
            this.splitContainerMeasurements.Panel1.Controls.Add(this.groupBoxInputMeasurements);
            // 
            // splitContainerMeasurements.Panel2
            // 
            this.splitContainerMeasurements.Panel2.Controls.Add(this.groupBoxOutputMeasurements);
            this.splitContainerMeasurements.Size = new System.Drawing.Size(842, 458);
            this.splitContainerMeasurements.SplitterDistance = 416;
            this.splitContainerMeasurements.TabIndex = 2;
            this.splitContainerMeasurements.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainerMeasurements_SplitterMoved);
            // 
            // groupBoxInputMeasurements
            // 
            this.groupBoxInputMeasurements.Controls.Add(this.dataGridViewInputMeasurements);
            this.groupBoxInputMeasurements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInputMeasurements.Location = new System.Drawing.Point(0, 0);
            this.groupBoxInputMeasurements.Name = "groupBoxInputMeasurements";
            this.groupBoxInputMeasurements.Size = new System.Drawing.Size(416, 458);
            this.groupBoxInputMeasurements.TabIndex = 0;
            this.groupBoxInputMeasurements.TabStop = false;
            this.groupBoxInputMeasurements.Text = "Input Measurements";
            // 
            // dataGridViewInputMeasurements
            // 
            this.dataGridViewInputMeasurements.AllowUserToAddRows = false;
            this.dataGridViewInputMeasurements.AllowUserToDeleteRows = false;
            this.dataGridViewInputMeasurements.AllowUserToResizeRows = false;
            this.dataGridViewInputMeasurements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridViewInputMeasurements.CausesValidation = false;
            this.dataGridViewInputMeasurements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewInputMeasurements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewInputMeasurements.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewInputMeasurements.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewInputMeasurements.MultiSelect = false;
            this.dataGridViewInputMeasurements.Name = "dataGridViewInputMeasurements";
            this.dataGridViewInputMeasurements.RowHeadersVisible = false;
            this.dataGridViewInputMeasurements.RowHeadersWidth = 62;
            this.dataGridViewInputMeasurements.ShowEditingIcon = false;
            this.dataGridViewInputMeasurements.ShowRowErrors = false;
            this.dataGridViewInputMeasurements.Size = new System.Drawing.Size(410, 439);
            this.dataGridViewInputMeasurements.TabIndex = 0;
            // 
            // groupBoxOutputMeasurements
            // 
            this.groupBoxOutputMeasurements.Controls.Add(this.dataGridViewOutputMeasurements);
            this.groupBoxOutputMeasurements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutputMeasurements.Location = new System.Drawing.Point(0, 0);
            this.groupBoxOutputMeasurements.Name = "groupBoxOutputMeasurements";
            this.groupBoxOutputMeasurements.Size = new System.Drawing.Size(422, 458);
            this.groupBoxOutputMeasurements.TabIndex = 1;
            this.groupBoxOutputMeasurements.TabStop = false;
            this.groupBoxOutputMeasurements.Text = "Output Measurements";
            // 
            // dataGridViewOutputMeasurements
            // 
            this.dataGridViewOutputMeasurements.AllowUserToAddRows = false;
            this.dataGridViewOutputMeasurements.AllowUserToDeleteRows = false;
            this.dataGridViewOutputMeasurements.AllowUserToResizeRows = false;
            this.dataGridViewOutputMeasurements.CausesValidation = false;
            this.dataGridViewOutputMeasurements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOutputMeasurements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewOutputMeasurements.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewOutputMeasurements.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewOutputMeasurements.MultiSelect = false;
            this.dataGridViewOutputMeasurements.Name = "dataGridViewOutputMeasurements";
            this.dataGridViewOutputMeasurements.RowHeadersVisible = false;
            this.dataGridViewOutputMeasurements.RowHeadersWidth = 62;
            this.dataGridViewOutputMeasurements.ShowEditingIcon = false;
            this.dataGridViewOutputMeasurements.ShowRowErrors = false;
            this.dataGridViewOutputMeasurements.Size = new System.Drawing.Size(416, 439);
            this.dataGridViewOutputMeasurements.TabIndex = 1;
            // 
            // imageListStatus
            // 
            this.imageListStatus.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListStatus.ImageStream")));
            this.imageListStatus.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListStatus.Images.SetKeyName(0, "red.png");
            this.imageListStatus.Images.SetKeyName(1, "green.png");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 602);
            this.Controls.Add(this.splitContainerMeasurements);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(598, 444);
            this.Name = "MainForm";
            this.Text = "Adapter Explorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            this.splitContainerMeasurements.Panel1.ResumeLayout(false);
            this.splitContainerMeasurements.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMeasurements)).EndInit();
            this.splitContainerMeasurements.ResumeLayout(false);
            this.groupBoxInputMeasurements.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInputMeasurements)).EndInit();
            this.groupBoxOutputMeasurements.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutputMeasurements)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.SplitContainer splitContainerMeasurements;
        private System.Windows.Forms.ComboBox comboBoxAdapters;
        private System.Windows.Forms.Label labelAdapters;
        private System.Windows.Forms.GroupBox groupBoxInputMeasurements;
        private System.Windows.Forms.GroupBox groupBoxOutputMeasurements;
        private System.Windows.Forms.DataGridView dataGridViewInputMeasurements;
        private System.Windows.Forms.DataGridView dataGridViewOutputMeasurements;
        private System.Windows.Forms.TextBox textBoxMessageOutput;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.TextBox textBoxAdapterInfo;
        public System.Windows.Forms.CheckBox checkBoxOutputAdapters;
        public System.Windows.Forms.CheckBox checkBoxInputAdapters;
        public System.Windows.Forms.CheckBox checkBoxActionAdapters;
        private System.Windows.Forms.ImageList imageListStatus;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

