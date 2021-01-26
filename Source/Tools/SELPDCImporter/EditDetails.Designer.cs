
namespace SELPDCImporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditDetails));
            this.tableLayoutPanelConfigDetails = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxTCFConnectionName = new System.Windows.Forms.TextBox();
            this.textBoxGCFConnectionName = new System.Windows.Forms.TextBox();
            this.labelConnectionName = new System.Windows.Forms.Label();
            this.labelTargetConfigFile = new System.Windows.Forms.Label();
            this.labelGSFConfigFile = new System.Windows.Forms.Label();
            this.labelSELConfigFile = new System.Windows.Forms.Label();
            this.labelDataItem = new System.Windows.Forms.Label();
            this.textBoxSCFConnectionName = new System.Windows.Forms.TextBox();
            this.flowLayoutPanelActionButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonImport = new System.Windows.Forms.Button();
            this.tableLayoutPanelConfigDetails.SuspendLayout();
            this.flowLayoutPanelActionButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelConfigDetails
            // 
            this.tableLayoutPanelConfigDetails.AutoScroll = true;
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
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelSELConfigFile, 1, 0);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.labelDataItem, 0, 0);
            this.tableLayoutPanelConfigDetails.Controls.Add(this.textBoxSCFConnectionName, 1, 1);
            this.tableLayoutPanelConfigDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelConfigDetails.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelConfigDetails.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelConfigDetails.Name = "tableLayoutPanelConfigDetails";
            this.tableLayoutPanelConfigDetails.RowCount = 3;
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelConfigDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelConfigDetails.Size = new System.Drawing.Size(734, 361);
            this.tableLayoutPanelConfigDetails.TabIndex = 0;
            // 
            // textBoxTCFConnectionName
            // 
            this.textBoxTCFConnectionName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxTCFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTCFConnectionName.Location = new System.Drawing.Point(553, 25);
            this.textBoxTCFConnectionName.Name = "textBoxTCFConnectionName";
            this.textBoxTCFConnectionName.Size = new System.Drawing.Size(177, 20);
            this.textBoxTCFConnectionName.TabIndex = 7;
            // 
            // textBoxGCFConnectionName
            // 
            this.textBoxGCFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxGCFConnectionName.Location = new System.Drawing.Point(370, 25);
            this.textBoxGCFConnectionName.Name = "textBoxGCFConnectionName";
            this.textBoxGCFConnectionName.ReadOnly = true;
            this.textBoxGCFConnectionName.Size = new System.Drawing.Size(176, 20);
            this.textBoxGCFConnectionName.TabIndex = 6;
            // 
            // labelConnectionName
            // 
            this.labelConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelConnectionName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConnectionName.Location = new System.Drawing.Point(4, 22);
            this.labelConnectionName.Name = "labelConnectionName";
            this.labelConnectionName.Size = new System.Drawing.Size(176, 25);
            this.labelConnectionName.TabIndex = 4;
            this.labelConnectionName.Text = "Connection Name";
            this.labelConnectionName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTargetConfigFile
            // 
            this.labelTargetConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTargetConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTargetConfigFile.Location = new System.Drawing.Point(553, 1);
            this.labelTargetConfigFile.Name = "labelTargetConfigFile";
            this.labelTargetConfigFile.Size = new System.Drawing.Size(177, 20);
            this.labelTargetConfigFile.TabIndex = 3;
            this.labelTargetConfigFile.Text = "Target Config File:";
            this.labelTargetConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelGSFConfigFile
            // 
            this.labelGSFConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelGSFConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGSFConfigFile.Location = new System.Drawing.Point(370, 1);
            this.labelGSFConfigFile.Name = "labelGSFConfigFile";
            this.labelGSFConfigFile.Size = new System.Drawing.Size(176, 20);
            this.labelGSFConfigFile.TabIndex = 2;
            this.labelGSFConfigFile.Text = "GSF Config File (Loaded):";
            this.labelGSFConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSELConfigFile
            // 
            this.labelSELConfigFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSELConfigFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSELConfigFile.Location = new System.Drawing.Point(187, 1);
            this.labelSELConfigFile.Name = "labelSELConfigFile";
            this.labelSELConfigFile.Size = new System.Drawing.Size(176, 20);
            this.labelSELConfigFile.TabIndex = 1;
            this.labelSELConfigFile.Text = "SEL Config File (Parsed):";
            this.labelSELConfigFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDataItem
            // 
            this.labelDataItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelDataItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDataItem.Location = new System.Drawing.Point(4, 1);
            this.labelDataItem.Name = "labelDataItem";
            this.labelDataItem.Size = new System.Drawing.Size(176, 20);
            this.labelDataItem.TabIndex = 0;
            this.labelDataItem.Text = "Data Item:";
            this.labelDataItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxSCFConnectionName
            // 
            this.textBoxSCFConnectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSCFConnectionName.Location = new System.Drawing.Point(187, 25);
            this.textBoxSCFConnectionName.Name = "textBoxSCFConnectionName";
            this.textBoxSCFConnectionName.ReadOnly = true;
            this.textBoxSCFConnectionName.Size = new System.Drawing.Size(176, 20);
            this.textBoxSCFConnectionName.TabIndex = 5;
            // 
            // flowLayoutPanelActionButtons
            // 
            this.flowLayoutPanelActionButtons.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelActionButtons.Controls.Add(this.buttonImport);
            this.flowLayoutPanelActionButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanelActionButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelActionButtons.Location = new System.Drawing.Point(0, 332);
            this.flowLayoutPanelActionButtons.Name = "flowLayoutPanelActionButtons";
            this.flowLayoutPanelActionButtons.Size = new System.Drawing.Size(734, 29);
            this.flowLayoutPanelActionButtons.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(656, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(575, 3);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(75, 23);
            this.buttonImport.TabIndex = 1;
            this.buttonImport.Text = "&Import";
            this.buttonImport.UseVisualStyleBackColor = true;
            // 
            // EditDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(734, 361);
            this.Controls.Add(this.flowLayoutPanelActionButtons);
            this.Controls.Add(this.tableLayoutPanelConfigDetails);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditDetails";
            this.Text = "Edit PDC Details";
            this.Load += new System.EventHandler(this.EditDetails_Load);
            this.tableLayoutPanelConfigDetails.ResumeLayout(false);
            this.tableLayoutPanelConfigDetails.PerformLayout();
            this.flowLayoutPanelActionButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelConfigDetails;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActionButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label labelTargetConfigFile;
        private System.Windows.Forms.Label labelGSFConfigFile;
        private System.Windows.Forms.Label labelSELConfigFile;
        private System.Windows.Forms.Label labelDataItem;
        private System.Windows.Forms.TextBox textBoxTCFConnectionName;
        private System.Windows.Forms.TextBox textBoxGCFConnectionName;
        private System.Windows.Forms.Label labelConnectionName;
        private System.Windows.Forms.TextBox textBoxSCFConnectionName;
    }
}