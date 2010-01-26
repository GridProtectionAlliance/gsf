namespace ConfigurationEditor
{
	partial class FormConfigurationEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConfigurationEditor));
			this.TabControlLayout = new System.Windows.Forms.TabControl();
			this.TabPageEditor = new System.Windows.Forms.TabPage();
			this.TextBoxConfigurationFile = new System.Windows.Forms.TextBox();
			this.LableCurrentFile = new System.Windows.Forms.Label();
			this.ButtonLoad = new System.Windows.Forms.Button();
			this.PropertyGridConfiguration = new System.Windows.Forms.PropertyGrid();
			this.ButtonSave = new System.Windows.Forms.Button();
			this.ButtonOpen = new System.Windows.Forms.Button();
			this.TabPageSettings = new System.Windows.Forms.TabPage();
			this.ButtonSaveSettings = new System.Windows.Forms.Button();
			this.TextBoxDefaultConfiguration = new System.Windows.Forms.TextBox();
			this.TextBoxDefaultService = new System.Windows.Forms.TextBox();
			this.CheckBoxAutoRestart = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.OpenFileDialogConfiguration = new System.Windows.Forms.OpenFileDialog();
			this.TabControlLayout.SuspendLayout();
			this.TabPageEditor.SuspendLayout();
			this.TabPageSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabControlLayout
			// 
			this.TabControlLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TabControlLayout.Controls.Add(this.TabPageEditor);
			this.TabControlLayout.Controls.Add(this.TabPageSettings);
			this.TabControlLayout.Location = new System.Drawing.Point(12, 12);
			this.TabControlLayout.Name = "TabControlLayout";
			this.TabControlLayout.SelectedIndex = 0;
			this.TabControlLayout.Size = new System.Drawing.Size(568, 542);
			this.TabControlLayout.TabIndex = 8;
			// 
			// TabPageEditor
			// 
			this.TabPageEditor.Controls.Add(this.TextBoxConfigurationFile);
			this.TabPageEditor.Controls.Add(this.LableCurrentFile);
			this.TabPageEditor.Controls.Add(this.ButtonLoad);
			this.TabPageEditor.Controls.Add(this.PropertyGridConfiguration);
			this.TabPageEditor.Controls.Add(this.ButtonSave);
			this.TabPageEditor.Controls.Add(this.ButtonOpen);
			this.TabPageEditor.Location = new System.Drawing.Point(4, 22);
			this.TabPageEditor.Name = "TabPageEditor";
			this.TabPageEditor.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageEditor.Size = new System.Drawing.Size(560, 516);
			this.TabPageEditor.TabIndex = 0;
			this.TabPageEditor.Text = "Configuration Editor";
			this.TabPageEditor.UseVisualStyleBackColor = true;
			// 
			// TextBoxConfigurationFile
			// 
			this.TextBoxConfigurationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxConfigurationFile.Location = new System.Drawing.Point(6, 8);
			this.TextBoxConfigurationFile.Name = "TextBoxConfigurationFile";
			this.TextBoxConfigurationFile.Size = new System.Drawing.Size(406, 20);
			this.TextBoxConfigurationFile.TabIndex = 8;
			// 
			// LableCurrentFile
			// 
			this.LableCurrentFile.AutoSize = true;
			this.LableCurrentFile.Font = new System.Drawing.Font("Tahoma", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LableCurrentFile.Location = new System.Drawing.Point(6, 32);
			this.LableCurrentFile.Name = "LableCurrentFile";
			this.LableCurrentFile.Size = new System.Drawing.Size(0, 12);
			this.LableCurrentFile.TabIndex = 6;
			// 
			// ButtonLoad
			// 
			this.ButtonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonLoad.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonLoad.Location = new System.Drawing.Point(454, 6);
			this.ButtonLoad.Name = "ButtonLoad";
			this.ButtonLoad.Size = new System.Drawing.Size(100, 25);
			this.ButtonLoad.TabIndex = 9;
			this.ButtonLoad.Text = "Load Settings";
			this.ButtonLoad.UseVisualStyleBackColor = true;
			// 
			// PropertyGridConfiguration
			// 
			this.PropertyGridConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PropertyGridConfiguration.Location = new System.Drawing.Point(6, 47);
			this.PropertyGridConfiguration.Name = "PropertyGridConfiguration";
			this.PropertyGridConfiguration.Size = new System.Drawing.Size(548, 432);
			this.PropertyGridConfiguration.TabIndex = 6;
			this.PropertyGridConfiguration.ToolbarVisible = false;
			// 
			// ButtonSave
			// 
			this.ButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSave.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonSave.Location = new System.Drawing.Point(454, 485);
			this.ButtonSave.Name = "ButtonSave";
			this.ButtonSave.Size = new System.Drawing.Size(100, 25);
			this.ButtonSave.TabIndex = 7;
			this.ButtonSave.Text = "Save Settings";
			this.ButtonSave.UseVisualStyleBackColor = true;
			// 
			// ButtonOpen
			// 
			this.ButtonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOpen.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonOpen.Location = new System.Drawing.Point(418, 6);
			this.ButtonOpen.Name = "ButtonOpen";
			this.ButtonOpen.Size = new System.Drawing.Size(30, 25);
			this.ButtonOpen.TabIndex = 10;
			this.ButtonOpen.Text = "...";
			this.ButtonOpen.UseVisualStyleBackColor = true;
			// 
			// TabPageSettings
			// 
			this.TabPageSettings.Controls.Add(this.ButtonSaveSettings);
			this.TabPageSettings.Controls.Add(this.TextBoxDefaultConfiguration);
			this.TabPageSettings.Controls.Add(this.TextBoxDefaultService);
			this.TabPageSettings.Controls.Add(this.CheckBoxAutoRestart);
			this.TabPageSettings.Controls.Add(this.label3);
			this.TabPageSettings.Controls.Add(this.label2);
			this.TabPageSettings.Controls.Add(this.label1);
			this.TabPageSettings.Location = new System.Drawing.Point(4, 22);
			this.TabPageSettings.Name = "TabPageSettings";
			this.TabPageSettings.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageSettings.Size = new System.Drawing.Size(560, 516);
			this.TabPageSettings.TabIndex = 1;
			this.TabPageSettings.Text = "Application Settings";
			this.TabPageSettings.UseVisualStyleBackColor = true;
			// 
			// ButtonSaveSettings
			// 
			this.ButtonSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSaveSettings.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonSaveSettings.Location = new System.Drawing.Point(379, 485);
			this.ButtonSaveSettings.Name = "ButtonSaveSettings";
			this.ButtonSaveSettings.Size = new System.Drawing.Size(175, 25);
			this.ButtonSaveSettings.TabIndex = 8;
			this.ButtonSaveSettings.Text = "Save Application Settings";
			this.ButtonSaveSettings.UseVisualStyleBackColor = true;
			// 
			// TextBoxDefaultConfiguration
			// 
			this.TextBoxDefaultConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxDefaultConfiguration.Location = new System.Drawing.Point(141, 34);
			this.TextBoxDefaultConfiguration.Name = "TextBoxDefaultConfiguration";
			this.TextBoxDefaultConfiguration.Size = new System.Drawing.Size(413, 20);
			this.TextBoxDefaultConfiguration.TabIndex = 5;
			// 
			// TextBoxDefaultService
			// 
			this.TextBoxDefaultService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxDefaultService.Enabled = false;
			this.TextBoxDefaultService.Location = new System.Drawing.Point(141, 92);
			this.TextBoxDefaultService.Name = "TextBoxDefaultService";
			this.TextBoxDefaultService.Size = new System.Drawing.Size(413, 20);
			this.TextBoxDefaultService.TabIndex = 4;
			// 
			// CheckBoxAutoRestart
			// 
			this.CheckBoxAutoRestart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CheckBoxAutoRestart.AutoSize = true;
			this.CheckBoxAutoRestart.Location = new System.Drawing.Point(141, 67);
			this.CheckBoxAutoRestart.Name = "CheckBoxAutoRestart";
			this.CheckBoxAutoRestart.Size = new System.Drawing.Size(126, 17);
			this.CheckBoxAutoRestart.TabIndex = 3;
			this.CheckBoxAutoRestart.Text = "Auto Restart Service";
			this.CheckBoxAutoRestart.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 95);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(110, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Default Service Name";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 37);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(129, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Default Configuration File";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(6, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(212, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Manage Default Application Settings";
			// 
			// OpenFileDialogConfiguration
			// 
			this.OpenFileDialogConfiguration.FileName = "openFileDialog1";
			// 
			// FormConfigurationEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(592, 566);
			this.Controls.Add(this.TabControlLayout);
			this.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(450, 450);
			this.Name = "FormConfigurationEditor";
			this.Text = "Configuration Editor";
			this.TabControlLayout.ResumeLayout(false);
			this.TabPageEditor.ResumeLayout(false);
			this.TabPageEditor.PerformLayout();
			this.TabPageSettings.ResumeLayout(false);
			this.TabPageSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl TabControlLayout;
		private System.Windows.Forms.TabPage TabPageEditor;
		private System.Windows.Forms.TextBox TextBoxConfigurationFile;
		private System.Windows.Forms.Label LableCurrentFile;
		private System.Windows.Forms.Button ButtonLoad;
		private System.Windows.Forms.PropertyGrid PropertyGridConfiguration;
		private System.Windows.Forms.Button ButtonSave;
		private System.Windows.Forms.Button ButtonOpen;
		private System.Windows.Forms.TabPage TabPageSettings;
		private System.Windows.Forms.Button ButtonSaveSettings;
		private System.Windows.Forms.TextBox TextBoxDefaultConfiguration;
		private System.Windows.Forms.TextBox TextBoxDefaultService;
		private System.Windows.Forms.CheckBox CheckBoxAutoRestart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogConfiguration;
	}
}