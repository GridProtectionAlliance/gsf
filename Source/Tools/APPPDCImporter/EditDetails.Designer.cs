
namespace APPPDCImporter
{
    partial class EditDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditDetails));
            this.tableLayoutPanelConfigDetails = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxTCFConnectionName = new System.Windows.Forms.TextBox();
            this.textBoxGCFConnectionName = new System.Windows.Forms.TextBox();
            this.labelConnectionName = new System.Windows.Forms.Label();
            this.labelTargetConfigFile = new System.Windows.Forms.Label();
            this.labelGSFConfigFile = new System.Windows.Forms.Label();
            this.labelAPPConfigFile = new System.Windows.Forms.Label();
            this.textBoxACFConnectionName = new System.Windows.Forms.TextBox();
            this.panelDataItem = new System.Windows.Forms.Panel();
            this.checkBoxDeleteAll = new System.Windows.Forms.CheckBox();
            this.labelDataItem = new System.Windows.Forms.Label();
            this.flowLayoutPanelActionButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.panelHistorian = new System.Windows.Forms.Panel();
            this.comboBoxHistorian = new System.Windows.Forms.ComboBox();
            this.labelHistorian = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.labelWarning1 = new System.Windows.Forms.Label();
            this.labelWarning2 = new System.Windows.Forms.Label();
            this.tableLayoutPanelConfigDetails.SuspendLayout();
            this.panelDataItem.SuspendLayout();
            this.flowLayoutPanelActionButtons.SuspendLayout();
            this.panelHistorian.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelConfigDetails
            // 
            this.tableLayoutPanelConfigDetails.AutoScroll = true;
            this.tableLayoutPanelConfigDetails.AutoScrollMargin = new System.Drawing.Size(10, 30);
            this.tableLayoutPanelConfigDetails.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelConfigDetails.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanelConfigDetails.ColumnCount = 4;
            this.tableLayoutPanelConfigDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelConfigDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelConfigDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelConfigDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelConfigDetails.Controls.Add(this.textBoxTCFConnectionName, 3, 1);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.textBoxGCFConnectionName, 2, 1);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelConnectionName, 0, 1);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelTargetConfigFile, 3, 0);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelGSFConfigFile, 2, 0);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelAPPConfigFile, 1, 0);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.textBoxACFConnectionName, 1, 1);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.panelDataItem, 0, 0);
            this.tableLayoutPanelConfigDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelConfigDetails.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelConfigDetails.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelConfigDetails.Name = "tableLayoutPanelConfigDetails";
            this.tableLayoutPanelConfigDetails.RowCount = 3;
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelConfigDetails.Size = new System.Drawing.Size(799, 411);
            this.tableLayoutPanelConfigDetails.TabIndex = 0;
            // 
            // textBoxTCFConnectionName
            // 
            this.textBoxTCFConnectionName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxTCFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorProvider.SetIconPadding(this.textBoxTCFConnectionName, -20);
            this.textBoxTCFConnectionName.Location = new System.Drawing.Point(601, 27);
            this.textBoxTCFConnectionName.Name = "textBoxTCFConnectionName";
            this.textBoxTCFConnectionName.Size = new System.Drawing.Size(194, 20);
            this.textBoxTCFConnectionName.TabIndex = 7;
            // 
            // textBoxGCFConnectionName
            // 
            this.textBoxGCFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxGCFConnectionName.Location = new System.Drawing.Point(402, 27);
            this.textBoxGCFConnectionName.Name = "textBoxGCFConnectionName";
            this.textBoxGCFConnectionName.ReadOnly = true;
            this.textBoxGCFConnectionName.Size = new System.Drawing.Size(192, 20);
            this.textBoxGCFConnectionName.TabIndex = 6;
            this.textBoxGCFConnectionName.TabStop = false;
            // 
            // labelConnectionName
            // 
            this.labelConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelConnectionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConnectionName.Location = new System.Drawing.Point(4, 24);
            this.labelConnectionName.Margin = new System.Windows.Forms.Padding(3, 0, 9, 0);
            this.labelConnectionName.Name = "labelConnectionName";
            this.labelConnectionName.Size = new System.Drawing.Size(186, 25);
            this.labelConnectionName.TabIndex = 4;
            this.labelConnectionName.Text = "Connection Name:";
            this.labelConnectionName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTargetConfigFile
            // 
            this.labelTargetConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTargetConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTargetConfigFile.Location = new System.Drawing.Point(601, 1);
            this.labelTargetConfigFile.Name = "labelTargetConfigFile";
            this.labelTargetConfigFile.Size = new System.Drawing.Size(194, 22);
            this.labelTargetConfigFile.TabIndex = 3;
            this.labelTargetConfigFile.Text = "Target Config:";
            this.labelTargetConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelGSFConfigFile
            // 
            this.labelGSFConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelGSFConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGSFConfigFile.Location = new System.Drawing.Point(402, 1);
            this.labelGSFConfigFile.Name = "labelGSFConfigFile";
            this.labelGSFConfigFile.Size = new System.Drawing.Size(192, 22);
            this.labelGSFConfigFile.TabIndex = 2;
            this.labelGSFConfigFile.Text = "GSF Config (Loaded):";
            this.labelGSFConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAPPConfigFile
            // 
            this.labelAPPConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAPPConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAPPConfigFile.Location = new System.Drawing.Point(203, 1);
            this.labelAPPConfigFile.Name = "labelAPPConfigFile";
            this.labelAPPConfigFile.Size = new System.Drawing.Size(192, 22);
            this.labelAPPConfigFile.TabIndex = 1;
            this.labelAPPConfigFile.Text = "APP Config (Parsed):";
            this.labelAPPConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxACFConnectionName
            // 
            this.textBoxACFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxACFConnectionName.Location = new System.Drawing.Point(203, 27);
            this.textBoxACFConnectionName.Name = "textBoxACFConnectionName";
            this.textBoxACFConnectionName.ReadOnly = true;
            this.textBoxACFConnectionName.Size = new System.Drawing.Size(192, 20);
            this.textBoxACFConnectionName.TabIndex = 5;
            this.textBoxACFConnectionName.TabStop = false;
            // 
            // panelDataItem
            // 
            this.panelDataItem.Controls.Add(this.checkBoxDeleteAll);
            this.panelDataItem.Controls.Add(this.labelDataItem);
            this.panelDataItem.Location = new System.Drawing.Point(1, 1);
            this.panelDataItem.Margin = new System.Windows.Forms.Padding(0);
            this.panelDataItem.Name = "panelDataItem";
            this.panelDataItem.Size = new System.Drawing.Size(182, 22);
            this.panelDataItem.TabIndex = 8;
            // 
            // checkBoxDeleteAll
            // 
            this.checkBoxDeleteAll.AutoSize = true;
            this.checkBoxDeleteAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkBoxDeleteAll.Location = new System.Drawing.Point(0, 0);
            this.checkBoxDeleteAll.Name = "checkBoxDeleteAll";
            this.checkBoxDeleteAll.Padding = new System.Windows.Forms.Padding(10, 3, 0, 0);
            this.checkBoxDeleteAll.Size = new System.Drawing.Size(73, 22);
            this.checkBoxDeleteAll.TabIndex = 2;
            this.checkBoxDeleteAll.TabStop = false;
            this.checkBoxDeleteAll.Text = "Delete?";
            this.checkBoxDeleteAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxDeleteAll.UseVisualStyleBackColor = true;
            // 
            // labelDataItem
            // 
            this.labelDataItem.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelDataItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDataItem.Location = new System.Drawing.Point(107, 0);
            this.labelDataItem.Name = "labelDataItem";
            this.labelDataItem.Padding = new System.Windows.Forms.Padding(0, 0, 9, 0);
            this.labelDataItem.Size = new System.Drawing.Size(75, 22);
            this.labelDataItem.TabIndex = 0;
            this.labelDataItem.Text = "Data Item:";
            this.labelDataItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // flowLayoutPanelActionButtons
            // 
            this.flowLayoutPanelActionButtons.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelActionButtons.Controls.Add(this.buttonImport);
            this.flowLayoutPanelActionButtons.Controls.Add(this.panelHistorian);
            this.flowLayoutPanelActionButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanelActionButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelActionButtons.Location = new System.Drawing.Point(0, 382);
            this.flowLayoutPanelActionButtons.Name = "flowLayoutPanelActionButtons";
            this.flowLayoutPanelActionButtons.Size = new System.Drawing.Size(799, 29);
            this.flowLayoutPanelActionButtons.TabIndex = 1;
            this.flowLayoutPanelActionButtons.WrapContents = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(721, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(640, 3);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 0;
            this.buttonImport.Text = "&Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            // 
            // panelHistorian
            // 
            this.panelHistorian.Controls.Add(this.labelWarning2);
            this.panelHistorian.Controls.Add(this.comboBoxHistorian);
            this.panelHistorian.Controls.Add(this.labelHistorian);
            this.panelHistorian.Controls.Add(this.labelWarning1);
            this.panelHistorian.Location = new System.Drawing.Point(0, 0);
            this.panelHistorian.Margin = new System.Windows.Forms.Padding(0);
            this.panelHistorian.Name = "panelHistorian";
            this.panelHistorian.Size = new System.Drawing.Size(637, 30);
            this.panelHistorian.TabIndex = 2;
            // 
            // comboBoxHistorian
            // 
            this.comboBoxHistorian.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxHistorian.FormattingEnabled = true;
            this.comboBoxHistorian.Location = new System.Drawing.Point(60, 6);
            this.comboBoxHistorian.Name = "comboBoxHistorian";
            this.comboBoxHistorian.Size = new System.Drawing.Size(121, 21);
            this.comboBoxHistorian.TabIndex = 1;
            // 
            // labelHistorian
            // 
            this.labelHistorian.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelHistorian.Location = new System.Drawing.Point(3, 4);
            this.labelHistorian.Name = "labelHistorian";
            this.labelHistorian.Size = new System.Drawing.Size(51, 23);
            this.labelHistorian.TabIndex = 0;
            this.labelHistorian.Text = "&Historian:";
            this.labelHistorian.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // labelWarning1
            // 
            this.labelWarning1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWarning1.BackColor = System.Drawing.Color.Yellow;
            this.labelWarning1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelWarning1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarning1.ForeColor = System.Drawing.Color.Red;
            this.labelWarning1.Location = new System.Drawing.Point(187, 4);
            this.labelWarning1.Name = "labelWarning1";
            this.labelWarning1.Size = new System.Drawing.Size(447, 22);
            this.labelWarning1.TabIndex = 2;
            this.labelWarning1.Text = "WARNING: Check device IP. No DB config loaded and target devices exist!";
            this.labelWarning1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelWarning1.Visible = false;
            // 
            // labelWarning2
            // 
            this.labelWarning2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWarning2.BackColor = System.Drawing.Color.Yellow;
            this.labelWarning2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelWarning2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarning2.ForeColor = System.Drawing.Color.Red;
            this.labelWarning2.Location = new System.Drawing.Point(187, 4);
            this.labelWarning2.Name = "labelWarning2";
            this.labelWarning2.Size = new System.Drawing.Size(447, 22);
            this.labelWarning2.TabIndex = 3;
            this.labelWarning2.Text = "WARNING: Check device IP. DB config does not match target devicest!";
            this.labelWarning2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelWarning2.Visible = false;
            // 
            // EditDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(799, 411);
            this.Controls.Add(this.flowLayoutPanelActionButtons);
            this.Controls.Add(this.tableLayoutPanelConfigDetails);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(815, 450);
            this.Name = "EditDetails";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Target PDC Config Details";
            this.Load += new System.EventHandler(this.EditDetails_Load);
            this.Resize += new System.EventHandler(this.EditDetails_Resize);
            this.tableLayoutPanelConfigDetails.ResumeLayout(false);
            this.tableLayoutPanelConfigDetails.PerformLayout();
            this.panelDataItem.ResumeLayout(false);
            this.panelDataItem.PerformLayout();
            this.flowLayoutPanelActionButtons.ResumeLayout(false);
            this.panelHistorian.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelConfigDetails;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActionButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label labelTargetConfigFile;
        private System.Windows.Forms.Label labelGSFConfigFile;
        private System.Windows.Forms.Label labelAPPConfigFile;
        private System.Windows.Forms.TextBox textBoxTCFConnectionName;
        private System.Windows.Forms.TextBox textBoxGCFConnectionName;
        private System.Windows.Forms.Label labelConnectionName;
        private System.Windows.Forms.TextBox textBoxACFConnectionName;
        private System.Windows.Forms.Panel panelDataItem;
        private System.Windows.Forms.CheckBox checkBoxDeleteAll;
        private System.Windows.Forms.Label labelDataItem;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Panel panelHistorian;
        private System.Windows.Forms.ComboBox comboBoxHistorian;
        private System.Windows.Forms.Label labelHistorian;
        private System.Windows.Forms.Label labelWarning1;
        private System.Windows.Forms.Label labelWarning2;
    }
}