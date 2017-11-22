namespace StatHistorianReportGenerator
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
            this.ReportDateLabel = new System.Windows.Forms.Label();
            this.ReportDateDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.GenerateReportButton = new System.Windows.Forms.Button();
            this.TitleTextLabel = new System.Windows.Forms.Label();
            this.TitleTextTextBox = new System.Windows.Forms.TextBox();
            this.CompanyTextTextBox = new System.Windows.Forms.TextBox();
            this.CompanyTextLabel = new System.Windows.Forms.Label();
            this.Level4ThresholdTextBox = new System.Windows.Forms.TextBox();
            this.Level4ThresholdLabel = new System.Windows.Forms.Label();
            this.Level3ThresholdTextBox = new System.Windows.Forms.TextBox();
            this.Level3ThresholdLabel = new System.Windows.Forms.Label();
            this.PercentLabel1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Level4AliasTextBox = new System.Windows.Forms.TextBox();
            this.Level4AliasLabel = new System.Windows.Forms.Label();
            this.Level3AliasTextBox = new System.Windows.Forms.TextBox();
            this.Level3AliasLabel = new System.Windows.Forms.Label();
            this.GenerateCsvReportCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ReportDateLabel
            // 
            this.ReportDateLabel.AutoSize = true;
            this.ReportDateLabel.Location = new System.Drawing.Point(23, 94);
            this.ReportDateLabel.Name = "ReportDateLabel";
            this.ReportDateLabel.Size = new System.Drawing.Size(65, 13);
            this.ReportDateLabel.TabIndex = 0;
            this.ReportDateLabel.Text = "Report Date";
            // 
            // ReportDateDateTimePicker
            // 
            this.ReportDateDateTimePicker.CustomFormat = "MM/dd/yyyy";
            this.ReportDateDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.ReportDateDateTimePicker.Location = new System.Drawing.Point(96, 92);
            this.ReportDateDateTimePicker.Name = "ReportDateDateTimePicker";
            this.ReportDateDateTimePicker.Size = new System.Drawing.Size(142, 20);
            this.ReportDateDateTimePicker.TabIndex = 2;
            // 
            // GenerateReportButton
            // 
            this.GenerateReportButton.Location = new System.Drawing.Point(215, 156);
            this.GenerateReportButton.Name = "GenerateReportButton";
            this.GenerateReportButton.Size = new System.Drawing.Size(75, 39);
            this.GenerateReportButton.TabIndex = 7;
            this.GenerateReportButton.Text = "Generate Report";
            this.GenerateReportButton.UseVisualStyleBackColor = true;
            this.GenerateReportButton.Click += new System.EventHandler(this.GenerateReportButton_Click);
            // 
            // TitleTextLabel
            // 
            this.TitleTextLabel.AutoSize = true;
            this.TitleTextLabel.Location = new System.Drawing.Point(23, 44);
            this.TitleTextLabel.Name = "TitleTextLabel";
            this.TitleTextLabel.Size = new System.Drawing.Size(51, 13);
            this.TitleTextLabel.TabIndex = 0;
            this.TitleTextLabel.Text = "Title Text";
            // 
            // TitleTextTextBox
            // 
            this.TitleTextTextBox.Location = new System.Drawing.Point(96, 40);
            this.TitleTextTextBox.Name = "TitleTextTextBox";
            this.TitleTextTextBox.Size = new System.Drawing.Size(142, 20);
            this.TitleTextTextBox.TabIndex = 0;
            this.TitleTextTextBox.Text = "GSF Completeness Report";
            // 
            // CompanyTextTextBox
            // 
            this.CompanyTextTextBox.Location = new System.Drawing.Point(96, 66);
            this.CompanyTextTextBox.Name = "CompanyTextTextBox";
            this.CompanyTextTextBox.Size = new System.Drawing.Size(142, 20);
            this.CompanyTextTextBox.TabIndex = 1;
            this.CompanyTextTextBox.Text = "Grid Protection Alliance";
            // 
            // CompanyTextLabel
            // 
            this.CompanyTextLabel.AutoSize = true;
            this.CompanyTextLabel.Location = new System.Drawing.Point(23, 68);
            this.CompanyTextLabel.Name = "CompanyTextLabel";
            this.CompanyTextLabel.Size = new System.Drawing.Size(51, 13);
            this.CompanyTextLabel.TabIndex = 0;
            this.CompanyTextLabel.Text = "Company";
            // 
            // Level4ThresholdTextBox
            // 
            this.Level4ThresholdTextBox.Location = new System.Drawing.Point(367, 39);
            this.Level4ThresholdTextBox.Name = "Level4ThresholdTextBox";
            this.Level4ThresholdTextBox.Size = new System.Drawing.Size(75, 20);
            this.Level4ThresholdTextBox.TabIndex = 3;
            this.Level4ThresholdTextBox.Text = "99";
            this.Level4ThresholdTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Level4ThresholdLabel
            // 
            this.Level4ThresholdLabel.AutoSize = true;
            this.Level4ThresholdLabel.Location = new System.Drawing.Point(269, 43);
            this.Level4ThresholdLabel.Name = "Level4ThresholdLabel";
            this.Level4ThresholdLabel.Size = new System.Drawing.Size(92, 13);
            this.Level4ThresholdLabel.TabIndex = 0;
            this.Level4ThresholdLabel.Text = "Level 4 Threshold";
            // 
            // Level3ThresholdTextBox
            // 
            this.Level3ThresholdTextBox.Location = new System.Drawing.Point(367, 65);
            this.Level3ThresholdTextBox.Name = "Level3ThresholdTextBox";
            this.Level3ThresholdTextBox.Size = new System.Drawing.Size(75, 20);
            this.Level3ThresholdTextBox.TabIndex = 4;
            this.Level3ThresholdTextBox.Text = "90";
            this.Level3ThresholdTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Level3ThresholdLabel
            // 
            this.Level3ThresholdLabel.AutoSize = true;
            this.Level3ThresholdLabel.Location = new System.Drawing.Point(269, 69);
            this.Level3ThresholdLabel.Name = "Level3ThresholdLabel";
            this.Level3ThresholdLabel.Size = new System.Drawing.Size(92, 13);
            this.Level3ThresholdLabel.TabIndex = 0;
            this.Level3ThresholdLabel.Text = "Level 3 Threshold";
            // 
            // PercentLabel1
            // 
            this.PercentLabel1.AutoSize = true;
            this.PercentLabel1.Location = new System.Drawing.Point(444, 42);
            this.PercentLabel1.Name = "PercentLabel1";
            this.PercentLabel1.Size = new System.Drawing.Size(15, 13);
            this.PercentLabel1.TabIndex = 15;
            this.PercentLabel1.Text = "%";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(444, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "%";
            // 
            // Level4AliasTextBox
            // 
            this.Level4AliasTextBox.Location = new System.Drawing.Point(367, 91);
            this.Level4AliasTextBox.Name = "Level4AliasTextBox";
            this.Level4AliasTextBox.Size = new System.Drawing.Size(75, 20);
            this.Level4AliasTextBox.TabIndex = 5;
            this.Level4AliasTextBox.Text = "Good";
            // 
            // Level4AliasLabel
            // 
            this.Level4AliasLabel.AutoSize = true;
            this.Level4AliasLabel.Location = new System.Drawing.Point(269, 95);
            this.Level4AliasLabel.Name = "Level4AliasLabel";
            this.Level4AliasLabel.Size = new System.Drawing.Size(67, 13);
            this.Level4AliasLabel.TabIndex = 0;
            this.Level4AliasLabel.Text = "Level 4 Alias";
            // 
            // Level3AliasTextBox
            // 
            this.Level3AliasTextBox.Location = new System.Drawing.Point(367, 117);
            this.Level3AliasTextBox.Name = "Level3AliasTextBox";
            this.Level3AliasTextBox.Size = new System.Drawing.Size(75, 20);
            this.Level3AliasTextBox.TabIndex = 6;
            this.Level3AliasTextBox.Text = "Fair";
            // 
            // Level3AliasLabel
            // 
            this.Level3AliasLabel.AutoSize = true;
            this.Level3AliasLabel.Location = new System.Drawing.Point(269, 121);
            this.Level3AliasLabel.Name = "Level3AliasLabel";
            this.Level3AliasLabel.Size = new System.Drawing.Size(67, 13);
            this.Level3AliasLabel.TabIndex = 0;
            this.Level3AliasLabel.Text = "Level 3 Alias";
            // 
            // GenerateCsvReportCheckBox
            // 
            this.GenerateCsvReportCheckBox.AutoSize = true;
            this.GenerateCsvReportCheckBox.Location = new System.Drawing.Point(96, 121);
            this.GenerateCsvReportCheckBox.Name = "GenerateCsvReportCheckBox";
            this.GenerateCsvReportCheckBox.Size = new System.Drawing.Size(129, 17);
            this.GenerateCsvReportCheckBox.TabIndex = 17;
            this.GenerateCsvReportCheckBox.Text = "Generate CSV Report";
            this.GenerateCsvReportCheckBox.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 262);
            this.Controls.Add(this.GenerateCsvReportCheckBox);
            this.Controls.Add(this.Level3AliasTextBox);
            this.Controls.Add(this.Level3AliasLabel);
            this.Controls.Add(this.Level4AliasTextBox);
            this.Controls.Add(this.Level4AliasLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PercentLabel1);
            this.Controls.Add(this.Level3ThresholdTextBox);
            this.Controls.Add(this.Level3ThresholdLabel);
            this.Controls.Add(this.Level4ThresholdTextBox);
            this.Controls.Add(this.Level4ThresholdLabel);
            this.Controls.Add(this.CompanyTextTextBox);
            this.Controls.Add(this.CompanyTextLabel);
            this.Controls.Add(this.TitleTextTextBox);
            this.Controls.Add(this.TitleTextLabel);
            this.Controls.Add(this.GenerateReportButton);
            this.Controls.Add(this.ReportDateDateTimePicker);
            this.Controls.Add(this.ReportDateLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "STAT Historian Report Generator";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ReportDateLabel;
        private System.Windows.Forms.DateTimePicker ReportDateDateTimePicker;
        private System.Windows.Forms.Button GenerateReportButton;
        private System.Windows.Forms.Label TitleTextLabel;
        private System.Windows.Forms.TextBox TitleTextTextBox;
        private System.Windows.Forms.TextBox CompanyTextTextBox;
        private System.Windows.Forms.Label CompanyTextLabel;
        private System.Windows.Forms.TextBox Level4ThresholdTextBox;
        private System.Windows.Forms.Label Level4ThresholdLabel;
        private System.Windows.Forms.TextBox Level3ThresholdTextBox;
        private System.Windows.Forms.Label Level3ThresholdLabel;
        private System.Windows.Forms.Label PercentLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Level4AliasTextBox;
        private System.Windows.Forms.Label Level4AliasLabel;
        private System.Windows.Forms.TextBox Level3AliasTextBox;
        private System.Windows.Forms.Label Level3AliasLabel;
        private System.Windows.Forms.CheckBox GenerateCsvReportCheckBox;
    }
}

