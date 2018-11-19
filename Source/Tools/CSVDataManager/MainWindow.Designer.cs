namespace CSVDataManager
{
    partial class MainWindow
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
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.ExportTabPage = new System.Windows.Forms.TabPage();
            this.ExportFieldsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ExportTableComboBox = new System.Windows.Forms.ComboBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ImportTabPage = new System.Windows.Forms.TabPage();
            this.ImportActionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.InsertButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.ImportTableComboBox = new System.Windows.Forms.ComboBox();
            this.ExportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ImportFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ExportTopBarPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ExportCountLabel = new System.Windows.Forms.Label();
            this.ImportTopBarPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ImportCountLabel = new System.Windows.Forms.Label();
            this.ExportProgressBar = new System.Windows.Forms.ProgressBar();
            this.ImportProgressBar = new System.Windows.Forms.ProgressBar();
            this.MainTabControl.SuspendLayout();
            this.ExportTabPage.SuspendLayout();
            this.ImportTabPage.SuspendLayout();
            this.ImportActionsPanel.SuspendLayout();
            this.ExportTopBarPanel.SuspendLayout();
            this.ImportTopBarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.ExportTabPage);
            this.MainTabControl.Controls.Add(this.ImportTabPage);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(484, 361);
            this.MainTabControl.TabIndex = 0;
            // 
            // ExportTabPage
            // 
            this.ExportTabPage.Controls.Add(this.ExportFieldsPanel);
            this.ExportTabPage.Controls.Add(this.ExportTopBarPanel);
            this.ExportTabPage.Controls.Add(this.ExportProgressBar);
            this.ExportTabPage.Location = new System.Drawing.Point(4, 22);
            this.ExportTabPage.Name = "ExportTabPage";
            this.ExportTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.ExportTabPage.Size = new System.Drawing.Size(476, 335);
            this.ExportTabPage.TabIndex = 1;
            this.ExportTabPage.Text = "Export";
            this.ExportTabPage.UseVisualStyleBackColor = true;
            // 
            // ExportFieldsPanel
            // 
            this.ExportFieldsPanel.AutoScroll = true;
            this.ExportFieldsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ExportFieldsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExportFieldsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ExportFieldsPanel.Location = new System.Drawing.Point(10, 39);
            this.ExportFieldsPanel.Name = "ExportFieldsPanel";
            this.ExportFieldsPanel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 20);
            this.ExportFieldsPanel.Size = new System.Drawing.Size(456, 263);
            this.ExportFieldsPanel.TabIndex = 2;
            this.ExportFieldsPanel.WrapContents = false;
            // 
            // ExportTableComboBox
            // 
            this.ExportTableComboBox.DisplayMember = "Name";
            this.ExportTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExportTableComboBox.FormattingEnabled = true;
            this.ExportTableComboBox.Location = new System.Drawing.Point(3, 3);
            this.ExportTableComboBox.Name = "ExportTableComboBox";
            this.ExportTableComboBox.Size = new System.Drawing.Size(200, 21);
            this.ExportTableComboBox.TabIndex = 1;
            this.ExportTableComboBox.SelectedIndexChanged += new System.EventHandler(this.TableComboBox_SelectedIndexChanged);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(209, 3);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(75, 23);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export...";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ImportTabPage
            // 
            this.ImportTabPage.Controls.Add(this.ImportActionsPanel);
            this.ImportTabPage.Controls.Add(this.ImportTopBarPanel);
            this.ImportTabPage.Controls.Add(this.ImportProgressBar);
            this.ImportTabPage.Location = new System.Drawing.Point(4, 22);
            this.ImportTabPage.Name = "ImportTabPage";
            this.ImportTabPage.Padding = new System.Windows.Forms.Padding(10);
            this.ImportTabPage.Size = new System.Drawing.Size(476, 335);
            this.ImportTabPage.TabIndex = 0;
            this.ImportTabPage.Text = "Import";
            this.ImportTabPage.UseVisualStyleBackColor = true;
            // 
            // ImportActionsPanel
            // 
            this.ImportActionsPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.ImportActionsPanel.ColumnCount = 1;
            this.ImportActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ImportActionsPanel.Controls.Add(this.InsertButton, 0, 0);
            this.ImportActionsPanel.Controls.Add(this.UpdateButton, 0, 1);
            this.ImportActionsPanel.Controls.Add(this.DeleteButton, 0, 2);
            this.ImportActionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImportActionsPanel.Location = new System.Drawing.Point(10, 37);
            this.ImportActionsPanel.Name = "ImportActionsPanel";
            this.ImportActionsPanel.RowCount = 3;
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ImportActionsPanel.Size = new System.Drawing.Size(456, 265);
            this.ImportActionsPanel.TabIndex = 3;
            // 
            // InsertButton
            // 
            this.InsertButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.InsertButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InsertButton.Location = new System.Drawing.Point(178, 4);
            this.InsertButton.Name = "InsertButton";
            this.InsertButton.Size = new System.Drawing.Size(100, 30);
            this.InsertButton.TabIndex = 0;
            this.InsertButton.Text = "Insert...";
            this.InsertButton.UseVisualStyleBackColor = true;
            this.InsertButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.UpdateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdateButton.Location = new System.Drawing.Point(178, 41);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(100, 30);
            this.UpdateButton.TabIndex = 1;
            this.UpdateButton.Text = "Update...";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DeleteButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteButton.Location = new System.Drawing.Point(178, 78);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(100, 30);
            this.DeleteButton.TabIndex = 2;
            this.DeleteButton.Text = "Delete...";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // ImportTableComboBox
            // 
            this.ImportTableComboBox.DisplayMember = "Name";
            this.ImportTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ImportTableComboBox.FormattingEnabled = true;
            this.ImportTableComboBox.Location = new System.Drawing.Point(3, 3);
            this.ImportTableComboBox.Name = "ImportTableComboBox";
            this.ImportTableComboBox.Size = new System.Drawing.Size(200, 21);
            this.ImportTableComboBox.TabIndex = 1;
            this.ImportTableComboBox.SelectedIndexChanged += new System.EventHandler(this.TableComboBox_SelectedIndexChanged);
            // 
            // ExportFileDialog
            // 
            this.ExportFileDialog.DefaultExt = "csv";
            this.ExportFileDialog.Filter = "CSV Files|*.csv|All files|*.*";
            // 
            // ImportFileDialog
            // 
            this.ImportFileDialog.DefaultExt = "csv";
            // 
            // ExportTopBarPanel
            // 
            this.ExportTopBarPanel.AutoSize = true;
            this.ExportTopBarPanel.ColumnCount = 3;
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ExportTopBarPanel.Controls.Add(this.ExportTableComboBox, 0, 0);
            this.ExportTopBarPanel.Controls.Add(this.ExportButton, 1, 0);
            this.ExportTopBarPanel.Controls.Add(this.ExportCountLabel, 2, 0);
            this.ExportTopBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ExportTopBarPanel.Location = new System.Drawing.Point(10, 10);
            this.ExportTopBarPanel.Name = "ExportTopBarPanel";
            this.ExportTopBarPanel.RowCount = 1;
            this.ExportTopBarPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ExportTopBarPanel.Size = new System.Drawing.Size(456, 29);
            this.ExportTopBarPanel.TabIndex = 3;
            // 
            // ExportCountLabel
            // 
            this.ExportCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportCountLabel.AutoSize = true;
            this.ExportCountLabel.Location = new System.Drawing.Point(451, 5);
            this.ExportCountLabel.Margin = new System.Windows.Forms.Padding(5);
            this.ExportCountLabel.Name = "ExportCountLabel";
            this.ExportCountLabel.Size = new System.Drawing.Size(0, 13);
            this.ExportCountLabel.TabIndex = 2;
            // 
            // ImportTopBarPanel
            // 
            this.ImportTopBarPanel.AutoSize = true;
            this.ImportTopBarPanel.ColumnCount = 2;
            this.ImportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ImportTopBarPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ImportTopBarPanel.Controls.Add(this.ImportTableComboBox, 0, 0);
            this.ImportTopBarPanel.Controls.Add(this.ImportCountLabel, 1, 0);
            this.ImportTopBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ImportTopBarPanel.Location = new System.Drawing.Point(10, 10);
            this.ImportTopBarPanel.Name = "ImportTopBarPanel";
            this.ImportTopBarPanel.RowCount = 1;
            this.ImportTopBarPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ImportTopBarPanel.Size = new System.Drawing.Size(456, 27);
            this.ImportTopBarPanel.TabIndex = 4;
            // 
            // ImportCountLabel
            // 
            this.ImportCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportCountLabel.AutoSize = true;
            this.ImportCountLabel.Location = new System.Drawing.Point(451, 5);
            this.ImportCountLabel.Margin = new System.Windows.Forms.Padding(5);
            this.ImportCountLabel.Name = "ImportCountLabel";
            this.ImportCountLabel.Size = new System.Drawing.Size(0, 13);
            this.ImportCountLabel.TabIndex = 3;
            // 
            // ExportProgressBar
            // 
            this.ExportProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ExportProgressBar.Location = new System.Drawing.Point(10, 302);
            this.ExportProgressBar.Name = "ExportProgressBar";
            this.ExportProgressBar.Size = new System.Drawing.Size(456, 23);
            this.ExportProgressBar.TabIndex = 4;
            // 
            // ImportProgressBar
            // 
            this.ImportProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ImportProgressBar.Location = new System.Drawing.Point(10, 302);
            this.ImportProgressBar.Name = "ImportProgressBar";
            this.ImportProgressBar.Size = new System.Drawing.Size(456, 23);
            this.ImportProgressBar.TabIndex = 5;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.MainTabControl);
            this.MinimumSize = new System.Drawing.Size(500, 0);
            this.Name = "MainWindow";
            this.Text = "CSV Data Manager";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.MainTabControl.ResumeLayout(false);
            this.ExportTabPage.ResumeLayout(false);
            this.ExportTabPage.PerformLayout();
            this.ImportTabPage.ResumeLayout(false);
            this.ImportTabPage.PerformLayout();
            this.ImportActionsPanel.ResumeLayout(false);
            this.ExportTopBarPanel.ResumeLayout(false);
            this.ExportTopBarPanel.PerformLayout();
            this.ImportTopBarPanel.ResumeLayout(false);
            this.ImportTopBarPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage ImportTabPage;
        private System.Windows.Forms.TabPage ExportTabPage;
        private System.Windows.Forms.ComboBox ExportTableComboBox;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.FlowLayoutPanel ExportFieldsPanel;
        private System.Windows.Forms.SaveFileDialog ExportFileDialog;
        private System.Windows.Forms.ComboBox ImportTableComboBox;
        private System.Windows.Forms.TableLayoutPanel ImportActionsPanel;
        private System.Windows.Forms.Button InsertButton;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.OpenFileDialog ImportFileDialog;
        private System.Windows.Forms.TableLayoutPanel ExportTopBarPanel;
        private System.Windows.Forms.Label ExportCountLabel;
        private System.Windows.Forms.TableLayoutPanel ImportTopBarPanel;
        private System.Windows.Forms.Label ImportCountLabel;
        private System.Windows.Forms.ProgressBar ExportProgressBar;
        private System.Windows.Forms.ProgressBar ImportProgressBar;
    }
}

