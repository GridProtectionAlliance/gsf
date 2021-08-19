namespace GEPDataExtractor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBoxServerConnection = new System.Windows.Forms.GroupBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelInstance = new System.Windows.Forms.Label();
            this.textBoxHistorianInstanceName = new System.Windows.Forms.TextBox();
            this.labeServer = new System.Windows.Forms.Label();
            this.textBoxHistorianHostAddress = new System.Windows.Forms.TextBox();
            this.maskedTextBoxHistorianPort = new System.Windows.Forms.MaskedTextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.buttonExport = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.buttonCancelExport = new System.Windows.Forms.Button();
            this.linkLabelOpenExportPage = new System.Windows.Forms.LinkLabel();
            this.tabControlOptions = new System.Windows.Forms.TabControl();
            this.tabPageTimeRange = new System.Windows.Forms.TabPage();
            this.textBoxDeviceFilter = new System.Windows.Forms.TextBox();
            this.labelDeviceFilter = new System.Windows.Forms.Label();
            this.labelEndTimeUTC = new System.Windows.Forms.Label();
            this.labelStartTimeUTC = new System.Windows.Forms.Label();
            this.buttonShowGraph = new System.Windows.Forms.Button();
            this.buttonCancelPreFilter = new System.Windows.Forms.Button();
            this.buttonPreFilter = new System.Windows.Forms.Button();
            this.dataGridViewDevices = new System.Windows.Forms.DataGridView();
            this.labelDevices = new System.Windows.Forms.Label();
            this.labelDataTypes = new System.Windows.Forms.Label();
            this.checkedListBoxDataTypes = new System.Windows.Forms.CheckedListBox();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.dateTimePickerEndTime = new System.Windows.Forms.DateTimePicker();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.dateTimePickerSourceTime = new System.Windows.Forms.DateTimePicker();
            this.checkBoxSelectAllDevices = new System.Windows.Forms.CheckBox();
            this.labelSelectCount = new System.Windows.Forms.Label();
            this.tabPageSettings = new System.Windows.Forms.TabPage();
            this.buttonRestoreDefaults = new System.Windows.Forms.Button();
            this.maskedTextBoxAcceptableMissingData = new System.Windows.Forms.MaskedTextBox();
            this.labelAcceptableMissingData = new System.Windows.Forms.Label();
            this.maskedTextBoxAcceptableBadTime = new System.Windows.Forms.MaskedTextBox();
            this.labelAccetableBadTime = new System.Windows.Forms.Label();
            this.maskedTextBoxAcceptableBadData = new System.Windows.Forms.MaskedTextBox();
            this.labelAcceptableBadData = new System.Windows.Forms.Label();
            this.checkBoxFillInMissingTimestamps = new System.Windows.Forms.CheckBox();
            this.checkBoxAlignTimestamps = new System.Windows.Forms.CheckBox();
            this.checkBoxExportMissingAsNaN = new System.Windows.Forms.CheckBox();
            this.labelReadIntervalDetails = new System.Windows.Forms.Label();
            this.labelSecondsReadInterval = new System.Windows.Forms.Label();
            this.maskedTextBoxReadInterval = new System.Windows.Forms.MaskedTextBox();
            this.labelReadInterval = new System.Windows.Forms.Label();
            this.maskedTextBoxFrameRate = new System.Windows.Forms.MaskedTextBox();
            this.checkBoxEnableLogging = new System.Windows.Forms.CheckBox();
            this.maskedTextBoxMessageInterval = new System.Windows.Forms.MaskedTextBox();
            this.labelMessageInterval = new System.Windows.Forms.Label();
            this.labelSecondsMetadataTimeout = new System.Windows.Forms.Label();
            this.labelPerSec = new System.Windows.Forms.Label();
            this.maskedTextBoxMetadataTimeout = new System.Windows.Forms.MaskedTextBox();
            this.labelFrameRate = new System.Windows.Forms.Label();
            this.labelMetaDataTimeout = new System.Windows.Forms.Label();
            this.labelBadDataPercent = new System.Windows.Forms.Label();
            this.labelMissingDataPercent = new System.Windows.Forms.Label();
            this.labelBadTimePercent = new System.Windows.Forms.Label();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.labelFilterExpression = new System.Windows.Forms.Label();
            this.textBoxFilterExpression = new System.Windows.Forms.TextBox();
            this.groupBoxExportOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxExportFilePerDataType = new System.Windows.Forms.CheckBox();
            this.labelExportFileName = new System.Windows.Forms.Label();
            this.buttonSelectFile = new System.Windows.Forms.Button();
            this.textBoxExportFileName = new System.Windows.Forms.TextBox();
            this.radioButtonCOMTRADE = new System.Windows.Forms.RadioButton();
            this.radioButtonCSV = new System.Windows.Forms.RadioButton();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabPageMessages = new System.Windows.Forms.TabPage();
            this.textBoxMessageOutput = new System.Windows.Forms.TextBox();
            this.textBoxPhaseFilter = new System.Windows.Forms.TextBox();
            this.labelPhaseFilter = new System.Windows.Forms.Label();
            this.labelMeasurementCount = new System.Windows.Forms.Label();
            this.groupBoxServerConnection.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.tabControlOptions.SuspendLayout();
            this.tabPageTimeRange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDevices)).BeginInit();
            this.tabPageSettings.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.groupBoxExportOptions.SuspendLayout();
            this.tabPageMessages.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxServerConnection
            // 
            this.groupBoxServerConnection.Controls.Add(this.buttonConnect);
            this.groupBoxServerConnection.Controls.Add(this.labelInstance);
            this.groupBoxServerConnection.Controls.Add(this.textBoxHistorianInstanceName);
            this.groupBoxServerConnection.Controls.Add(this.labeServer);
            this.groupBoxServerConnection.Controls.Add(this.textBoxHistorianHostAddress);
            this.groupBoxServerConnection.Controls.Add(this.maskedTextBoxHistorianPort);
            this.groupBoxServerConnection.Controls.Add(this.labelPort);
            this.groupBoxServerConnection.Location = new System.Drawing.Point(12, 8);
            this.groupBoxServerConnection.Name = "groupBoxServerConnection";
            this.groupBoxServerConnection.Size = new System.Drawing.Size(201, 137);
            this.groupBoxServerConnection.TabIndex = 0;
            this.groupBoxServerConnection.TabStop = false;
            this.groupBoxServerConnection.Text = "&Connection";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConnect.Location = new System.Drawing.Point(108, 103);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(88, 27);
            this.buttonConnect.TabIndex = 6;
            this.buttonConnect.Text = "&Connect >>";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelInstance
            // 
            this.labelInstance.AutoSize = true;
            this.labelInstance.Location = new System.Drawing.Point(28, 70);
            this.labelInstance.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelInstance.Name = "labelInstance";
            this.labelInstance.Size = new System.Drawing.Size(51, 13);
            this.labelInstance.TabIndex = 4;
            this.labelInstance.Text = "Instance:";
            this.labelInstance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxHistorianInstanceName
            // 
            this.textBoxHistorianInstanceName.Location = new System.Drawing.Point(83, 67);
            this.textBoxHistorianInstanceName.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxHistorianInstanceName.Name = "textBoxHistorianInstanceName";
            this.textBoxHistorianInstanceName.Size = new System.Drawing.Size(41, 20);
            this.textBoxHistorianInstanceName.TabIndex = 5;
            this.textBoxHistorianInstanceName.Text = "PPA";
            this.textBoxHistorianInstanceName.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labeServer
            // 
            this.labeServer.AutoSize = true;
            this.labeServer.Location = new System.Drawing.Point(38, 22);
            this.labeServer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labeServer.Name = "labeServer";
            this.labeServer.Size = new System.Drawing.Size(41, 13);
            this.labeServer.TabIndex = 0;
            this.labeServer.Text = "Server:";
            this.labeServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxHistorianHostAddress
            // 
            this.textBoxHistorianHostAddress.Location = new System.Drawing.Point(83, 19);
            this.textBoxHistorianHostAddress.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxHistorianHostAddress.Name = "textBoxHistorianHostAddress";
            this.textBoxHistorianHostAddress.Size = new System.Drawing.Size(88, 20);
            this.textBoxHistorianHostAddress.TabIndex = 1;
            this.textBoxHistorianHostAddress.Text = "localhost";
            this.textBoxHistorianHostAddress.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // maskedTextBoxHistorianPort
            // 
            this.maskedTextBoxHistorianPort.Location = new System.Drawing.Point(83, 43);
            this.maskedTextBoxHistorianPort.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxHistorianPort.Mask = "00000";
            this.maskedTextBoxHistorianPort.Name = "maskedTextBoxHistorianPort";
            this.maskedTextBoxHistorianPort.Size = new System.Drawing.Size(41, 20);
            this.maskedTextBoxHistorianPort.TabIndex = 3;
            this.maskedTextBoxHistorianPort.Text = "6175";
            this.maskedTextBoxHistorianPort.ValidatingType = typeof(int);
            this.maskedTextBoxHistorianPort.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(50, 45);
            this.labelPort.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(29, 13);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "Port:";
            this.labelPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Enabled = false;
            this.buttonExport.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonExport.Location = new System.Drawing.Point(515, 7);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(2);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(88, 27);
            this.buttonExport.TabIndex = 5;
            this.buttonExport.Text = "&Export!";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 396);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(819, 27);
            this.progressBar.TabIndex = 4;
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.buttonCancelExport);
            this.groupBoxOptions.Controls.Add(this.buttonExport);
            this.groupBoxOptions.Controls.Add(this.linkLabelOpenExportPage);
            this.groupBoxOptions.Controls.Add(this.tabControlOptions);
            this.groupBoxOptions.Location = new System.Drawing.Point(221, 8);
            this.groupBoxOptions.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxOptions.Size = new System.Drawing.Size(612, 384);
            this.groupBoxOptions.TabIndex = 2;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "&Options";
            // 
            // buttonCancelExport
            // 
            this.buttonCancelExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelExport.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancelExport.Location = new System.Drawing.Point(515, 8);
            this.buttonCancelExport.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancelExport.Name = "buttonCancelExport";
            this.buttonCancelExport.Size = new System.Drawing.Size(88, 27);
            this.buttonCancelExport.TabIndex = 13;
            this.buttonCancelExport.Text = "Cancel...";
            this.buttonCancelExport.UseVisualStyleBackColor = true;
            this.buttonCancelExport.Visible = false;
            this.buttonCancelExport.Click += new System.EventHandler(this.buttonExportCancel_Click);
            // 
            // linkLabelOpenExportPage
            // 
            this.linkLabelOpenExportPage.AutoSize = true;
            this.linkLabelOpenExportPage.Location = new System.Drawing.Point(296, 19);
            this.linkLabelOpenExportPage.Name = "linkLabelOpenExportPage";
            this.linkLabelOpenExportPage.Size = new System.Drawing.Size(153, 13);
            this.linkLabelOpenExportPage.TabIndex = 1;
            this.linkLabelOpenExportPage.TabStop = true;
            this.linkLabelOpenExportPage.Text = "Open Web-Based Export Page";
            this.linkLabelOpenExportPage.Visible = false;
            this.linkLabelOpenExportPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenExportPage_LinkClicked);
            // 
            // tabControlOptions
            // 
            this.tabControlOptions.Controls.Add(this.tabPageTimeRange);
            this.tabControlOptions.Controls.Add(this.tabPageSettings);
            this.tabControlOptions.Controls.Add(this.tabPageAdvanced);
            this.tabControlOptions.Controls.Add(this.tabPageMessages);
            this.tabControlOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlOptions.Location = new System.Drawing.Point(2, 15);
            this.tabControlOptions.Name = "tabControlOptions";
            this.tabControlOptions.SelectedIndex = 0;
            this.tabControlOptions.Size = new System.Drawing.Size(608, 367);
            this.tabControlOptions.TabIndex = 0;
            // 
            // tabPageTimeRange
            // 
            this.tabPageTimeRange.BackColor = System.Drawing.Color.Transparent;
            this.tabPageTimeRange.Controls.Add(this.labelMeasurementCount);
            this.tabPageTimeRange.Controls.Add(this.textBoxPhaseFilter);
            this.tabPageTimeRange.Controls.Add(this.labelPhaseFilter);
            this.tabPageTimeRange.Controls.Add(this.textBoxDeviceFilter);
            this.tabPageTimeRange.Controls.Add(this.labelDeviceFilter);
            this.tabPageTimeRange.Controls.Add(this.labelEndTimeUTC);
            this.tabPageTimeRange.Controls.Add(this.labelStartTimeUTC);
            this.tabPageTimeRange.Controls.Add(this.buttonShowGraph);
            this.tabPageTimeRange.Controls.Add(this.buttonCancelPreFilter);
            this.tabPageTimeRange.Controls.Add(this.buttonPreFilter);
            this.tabPageTimeRange.Controls.Add(this.dataGridViewDevices);
            this.tabPageTimeRange.Controls.Add(this.labelDevices);
            this.tabPageTimeRange.Controls.Add(this.labelDataTypes);
            this.tabPageTimeRange.Controls.Add(this.checkedListBoxDataTypes);
            this.tabPageTimeRange.Controls.Add(this.labelEndTime);
            this.tabPageTimeRange.Controls.Add(this.dateTimePickerEndTime);
            this.tabPageTimeRange.Controls.Add(this.labelStartTime);
            this.tabPageTimeRange.Controls.Add(this.dateTimePickerSourceTime);
            this.tabPageTimeRange.Controls.Add(this.checkBoxSelectAllDevices);
            this.tabPageTimeRange.Controls.Add(this.labelSelectCount);
            this.tabPageTimeRange.Location = new System.Drawing.Point(4, 22);
            this.tabPageTimeRange.Name = "tabPageTimeRange";
            this.tabPageTimeRange.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTimeRange.Size = new System.Drawing.Size(600, 341);
            this.tabPageTimeRange.TabIndex = 0;
            this.tabPageTimeRange.Text = "Time Range";
            // 
            // textBoxDeviceFilter
            // 
            this.textBoxDeviceFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxDeviceFilter.Location = new System.Drawing.Point(12, 273);
            this.textBoxDeviceFilter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDeviceFilter.Name = "textBoxDeviceFilter";
            this.textBoxDeviceFilter.Size = new System.Drawing.Size(79, 20);
            this.textBoxDeviceFilter.TabIndex = 17;
            this.textBoxDeviceFilter.Text = "P_%";
            this.textBoxDeviceFilter.TextChanged += new System.EventHandler(this.textBoxDeviceFilter_TextChanged);
            // 
            // labelDeviceFilter
            // 
            this.labelDeviceFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDeviceFilter.AutoSize = true;
            this.labelDeviceFilter.Location = new System.Drawing.Point(9, 258);
            this.labelDeviceFilter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDeviceFilter.Name = "labelDeviceFilter";
            this.labelDeviceFilter.Size = new System.Drawing.Size(69, 13);
            this.labelDeviceFilter.TabIndex = 16;
            this.labelDeviceFilter.Text = "Device Filter:";
            this.labelDeviceFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelEndTimeUTC
            // 
            this.labelEndTimeUTC.AutoSize = true;
            this.labelEndTimeUTC.Location = new System.Drawing.Point(460, 17);
            this.labelEndTimeUTC.Name = "labelEndTimeUTC";
            this.labelEndTimeUTC.Size = new System.Drawing.Size(29, 13);
            this.labelEndTimeUTC.TabIndex = 15;
            this.labelEndTimeUTC.Text = "UTC";
            // 
            // labelStartTimeUTC
            // 
            this.labelStartTimeUTC.AutoSize = true;
            this.labelStartTimeUTC.Location = new System.Drawing.Point(218, 17);
            this.labelStartTimeUTC.Name = "labelStartTimeUTC";
            this.labelStartTimeUTC.Size = new System.Drawing.Size(29, 13);
            this.labelStartTimeUTC.TabIndex = 14;
            this.labelStartTimeUTC.Text = "UTC";
            // 
            // buttonShowGraph
            // 
            this.buttonShowGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowGraph.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShowGraph.Location = new System.Drawing.Point(509, 36);
            this.buttonShowGraph.Margin = new System.Windows.Forms.Padding(2);
            this.buttonShowGraph.Name = "buttonShowGraph";
            this.buttonShowGraph.Size = new System.Drawing.Size(88, 27);
            this.buttonShowGraph.TabIndex = 13;
            this.buttonShowGraph.Text = "Show Graph";
            this.buttonShowGraph.UseVisualStyleBackColor = true;
            this.buttonShowGraph.Visible = false;
            this.buttonShowGraph.Click += new System.EventHandler(this.buttonShowGraph_Click);
            // 
            // buttonCancelPreFilter
            // 
            this.buttonCancelPreFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelPreFilter.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancelPreFilter.Location = new System.Drawing.Point(509, 10);
            this.buttonCancelPreFilter.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancelPreFilter.Name = "buttonCancelPreFilter";
            this.buttonCancelPreFilter.Size = new System.Drawing.Size(88, 27);
            this.buttonCancelPreFilter.TabIndex = 12;
            this.buttonCancelPreFilter.Text = "Cancel...";
            this.buttonCancelPreFilter.UseVisualStyleBackColor = true;
            this.buttonCancelPreFilter.Visible = false;
            this.buttonCancelPreFilter.Click += new System.EventHandler(this.buttonCancelPreFilter_Click);
            // 
            // buttonPreFilter
            // 
            this.buttonPreFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreFilter.Enabled = false;
            this.buttonPreFilter.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPreFilter.Location = new System.Drawing.Point(509, 10);
            this.buttonPreFilter.Margin = new System.Windows.Forms.Padding(2);
            this.buttonPreFilter.Name = "buttonPreFilter";
            this.buttonPreFilter.Size = new System.Drawing.Size(88, 27);
            this.buttonPreFilter.TabIndex = 4;
            this.buttonPreFilter.Text = "&Pre-filter >>";
            this.buttonPreFilter.UseVisualStyleBackColor = true;
            this.buttonPreFilter.Click += new System.EventHandler(this.buttonPreFilter_Click);
            // 
            // dataGridViewDevices
            // 
            this.dataGridViewDevices.AllowUserToAddRows = false;
            this.dataGridViewDevices.AllowUserToDeleteRows = false;
            this.dataGridViewDevices.AllowUserToOrderColumns = true;
            this.dataGridViewDevices.AllowUserToResizeRows = false;
            this.dataGridViewDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewDevices.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewDevices.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewDevices.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridViewDevices.EnableHeadersVisualStyles = false;
            this.dataGridViewDevices.GridColor = System.Drawing.SystemColors.Control;
            this.dataGridViewDevices.Location = new System.Drawing.Point(97, 66);
            this.dataGridViewDevices.Name = "dataGridViewDevices";
            this.dataGridViewDevices.RowHeadersVisible = false;
            this.dataGridViewDevices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDevices.Size = new System.Drawing.Size(489, 267);
            this.dataGridViewDevices.TabIndex = 11;
            this.dataGridViewDevices.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDevices_CellContentClick);
            this.dataGridViewDevices.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewDevices_ColumnHeaderMouseClick);
            this.dataGridViewDevices.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dataGridViewDevices_DataBindingComplete);
            // 
            // labelDevices
            // 
            this.labelDevices.AutoSize = true;
            this.labelDevices.Location = new System.Drawing.Point(96, 50);
            this.labelDevices.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDevices.Name = "labelDevices";
            this.labelDevices.Size = new System.Drawing.Size(49, 13);
            this.labelDevices.TabIndex = 8;
            this.labelDevices.Text = "Devices:";
            this.labelDevices.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelDataTypes
            // 
            this.labelDataTypes.AutoSize = true;
            this.labelDataTypes.Location = new System.Drawing.Point(9, 50);
            this.labelDataTypes.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDataTypes.Name = "labelDataTypes";
            this.labelDataTypes.Size = new System.Drawing.Size(65, 13);
            this.labelDataTypes.TabIndex = 5;
            this.labelDataTypes.Text = "Data Types:";
            this.labelDataTypes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkedListBoxDataTypes
            // 
            this.checkedListBoxDataTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkedListBoxDataTypes.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxDataTypes.CheckOnClick = true;
            this.checkedListBoxDataTypes.FormattingEnabled = true;
            this.checkedListBoxDataTypes.Location = new System.Drawing.Point(12, 66);
            this.checkedListBoxDataTypes.Name = "checkedListBoxDataTypes";
            this.checkedListBoxDataTypes.Size = new System.Drawing.Size(79, 184);
            this.checkedListBoxDataTypes.TabIndex = 6;
            this.checkedListBoxDataTypes.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxDataTypes_SelectedIndexChanged);
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(255, 17);
            this.labelEndTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(55, 13);
            this.labelEndTime.TabIndex = 2;
            this.labelEndTime.Text = "End Time:";
            this.labelEndTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dateTimePickerEndTime
            // 
            this.dateTimePickerEndTime.CustomFormat = "MM/dd/yyyy HH:mm:ss";
            this.dateTimePickerEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerEndTime.Location = new System.Drawing.Point(313, 14);
            this.dateTimePickerEndTime.Margin = new System.Windows.Forms.Padding(2);
            this.dateTimePickerEndTime.Name = "dateTimePickerEndTime";
            this.dateTimePickerEndTime.Size = new System.Drawing.Size(147, 20);
            this.dateTimePickerEndTime.TabIndex = 3;
            this.dateTimePickerEndTime.Value = new System.DateTime(2017, 1, 1, 0, 10, 0, 0);
            this.dateTimePickerEndTime.ValueChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(9, 17);
            this.labelStartTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(58, 13);
            this.labelStartTime.TabIndex = 0;
            this.labelStartTime.Text = "Start Time:";
            this.labelStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dateTimePickerSourceTime
            // 
            this.dateTimePickerSourceTime.CustomFormat = "MM/dd/yyyy HH:mm:ss";
            this.dateTimePickerSourceTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerSourceTime.Location = new System.Drawing.Point(71, 14);
            this.dateTimePickerSourceTime.Margin = new System.Windows.Forms.Padding(2);
            this.dateTimePickerSourceTime.Name = "dateTimePickerSourceTime";
            this.dateTimePickerSourceTime.Size = new System.Drawing.Size(147, 20);
            this.dateTimePickerSourceTime.TabIndex = 1;
            this.dateTimePickerSourceTime.Value = new System.DateTime(2017, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerSourceTime.ValueChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // checkBoxSelectAllDevices
            // 
            this.checkBoxSelectAllDevices.AutoSize = true;
            this.checkBoxSelectAllDevices.Location = new System.Drawing.Point(149, 50);
            this.checkBoxSelectAllDevices.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSelectAllDevices.Name = "checkBoxSelectAllDevices";
            this.checkBoxSelectAllDevices.Size = new System.Drawing.Size(69, 17);
            this.checkBoxSelectAllDevices.TabIndex = 9;
            this.checkBoxSelectAllDevices.Text = "Select all";
            this.checkBoxSelectAllDevices.UseVisualStyleBackColor = true;
            this.checkBoxSelectAllDevices.CheckedChanged += new System.EventHandler(this.checkBoxSelectAllDevices_CheckedChanged);
            // 
            // labelSelectCount
            // 
            this.labelSelectCount.AutoSize = true;
            this.labelSelectCount.Location = new System.Drawing.Point(224, 50);
            this.labelSelectCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSelectCount.Name = "labelSelectCount";
            this.labelSelectCount.Size = new System.Drawing.Size(121, 13);
            this.labelSelectCount.TabIndex = 10;
            this.labelSelectCount.Tag = "";
            this.labelSelectCount.Text = "{0:N0} devices selected";
            this.labelSelectCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPageSettings
            // 
            this.tabPageSettings.BackColor = System.Drawing.Color.Transparent;
            this.tabPageSettings.Controls.Add(this.buttonRestoreDefaults);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxAcceptableMissingData);
            this.tabPageSettings.Controls.Add(this.labelAcceptableMissingData);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxAcceptableBadTime);
            this.tabPageSettings.Controls.Add(this.labelAccetableBadTime);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxAcceptableBadData);
            this.tabPageSettings.Controls.Add(this.labelAcceptableBadData);
            this.tabPageSettings.Controls.Add(this.checkBoxFillInMissingTimestamps);
            this.tabPageSettings.Controls.Add(this.checkBoxAlignTimestamps);
            this.tabPageSettings.Controls.Add(this.checkBoxExportMissingAsNaN);
            this.tabPageSettings.Controls.Add(this.labelReadIntervalDetails);
            this.tabPageSettings.Controls.Add(this.labelSecondsReadInterval);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxReadInterval);
            this.tabPageSettings.Controls.Add(this.labelReadInterval);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxFrameRate);
            this.tabPageSettings.Controls.Add(this.checkBoxEnableLogging);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxMessageInterval);
            this.tabPageSettings.Controls.Add(this.labelMessageInterval);
            this.tabPageSettings.Controls.Add(this.labelSecondsMetadataTimeout);
            this.tabPageSettings.Controls.Add(this.labelPerSec);
            this.tabPageSettings.Controls.Add(this.maskedTextBoxMetadataTimeout);
            this.tabPageSettings.Controls.Add(this.labelFrameRate);
            this.tabPageSettings.Controls.Add(this.labelMetaDataTimeout);
            this.tabPageSettings.Controls.Add(this.labelBadDataPercent);
            this.tabPageSettings.Controls.Add(this.labelMissingDataPercent);
            this.tabPageSettings.Controls.Add(this.labelBadTimePercent);
            this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Size = new System.Drawing.Size(600, 341);
            this.tabPageSettings.TabIndex = 2;
            this.tabPageSettings.Text = "Settings";
            // 
            // buttonRestoreDefaults
            // 
            this.buttonRestoreDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRestoreDefaults.Location = new System.Drawing.Point(445, 261);
            this.buttonRestoreDefaults.Name = "buttonRestoreDefaults";
            this.buttonRestoreDefaults.Size = new System.Drawing.Size(100, 23);
            this.buttonRestoreDefaults.TabIndex = 25;
            this.buttonRestoreDefaults.Text = "Restore Defaults";
            this.buttonRestoreDefaults.UseVisualStyleBackColor = true;
            this.buttonRestoreDefaults.Click += new System.EventHandler(this.buttonRestoreDefaults_Click);
            // 
            // maskedTextBoxAcceptableMissingData
            // 
            this.maskedTextBoxAcceptableMissingData.Location = new System.Drawing.Point(138, 264);
            this.maskedTextBoxAcceptableMissingData.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxAcceptableMissingData.Mask = "000";
            this.maskedTextBoxAcceptableMissingData.Name = "maskedTextBoxAcceptableMissingData";
            this.maskedTextBoxAcceptableMissingData.Size = new System.Drawing.Size(31, 20);
            this.maskedTextBoxAcceptableMissingData.TabIndex = 17;
            this.maskedTextBoxAcceptableMissingData.Text = "100";
            this.maskedTextBoxAcceptableMissingData.ValidatingType = typeof(int);
            this.maskedTextBoxAcceptableMissingData.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelAcceptableMissingData
            // 
            this.labelAcceptableMissingData.AutoSize = true;
            this.labelAcceptableMissingData.Location = new System.Drawing.Point(6, 267);
            this.labelAcceptableMissingData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAcceptableMissingData.Name = "labelAcceptableMissingData";
            this.labelAcceptableMissingData.Size = new System.Drawing.Size(128, 13);
            this.labelAcceptableMissingData.TabIndex = 16;
            this.labelAcceptableMissingData.Text = "Acceptable Missing Data:";
            this.labelAcceptableMissingData.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // maskedTextBoxAcceptableBadTime
            // 
            this.maskedTextBoxAcceptableBadTime.Location = new System.Drawing.Point(138, 240);
            this.maskedTextBoxAcceptableBadTime.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxAcceptableBadTime.Mask = "000";
            this.maskedTextBoxAcceptableBadTime.Name = "maskedTextBoxAcceptableBadTime";
            this.maskedTextBoxAcceptableBadTime.Size = new System.Drawing.Size(31, 20);
            this.maskedTextBoxAcceptableBadTime.TabIndex = 15;
            this.maskedTextBoxAcceptableBadTime.Text = "100";
            this.maskedTextBoxAcceptableBadTime.ValidatingType = typeof(int);
            this.maskedTextBoxAcceptableBadTime.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelAccetableBadTime
            // 
            this.labelAccetableBadTime.AutoSize = true;
            this.labelAccetableBadTime.Location = new System.Drawing.Point(22, 243);
            this.labelAccetableBadTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAccetableBadTime.Name = "labelAccetableBadTime";
            this.labelAccetableBadTime.Size = new System.Drawing.Size(112, 13);
            this.labelAccetableBadTime.TabIndex = 14;
            this.labelAccetableBadTime.Text = "Acceptable Bad Time:";
            this.labelAccetableBadTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // maskedTextBoxAcceptableBadData
            // 
            this.maskedTextBoxAcceptableBadData.Location = new System.Drawing.Point(138, 216);
            this.maskedTextBoxAcceptableBadData.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxAcceptableBadData.Mask = "000";
            this.maskedTextBoxAcceptableBadData.Name = "maskedTextBoxAcceptableBadData";
            this.maskedTextBoxAcceptableBadData.Size = new System.Drawing.Size(31, 20);
            this.maskedTextBoxAcceptableBadData.TabIndex = 13;
            this.maskedTextBoxAcceptableBadData.Text = "100";
            this.maskedTextBoxAcceptableBadData.ValidatingType = typeof(int);
            this.maskedTextBoxAcceptableBadData.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelAcceptableBadData
            // 
            this.labelAcceptableBadData.AutoSize = true;
            this.labelAcceptableBadData.Location = new System.Drawing.Point(22, 219);
            this.labelAcceptableBadData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAcceptableBadData.Name = "labelAcceptableBadData";
            this.labelAcceptableBadData.Size = new System.Drawing.Size(112, 13);
            this.labelAcceptableBadData.TabIndex = 12;
            this.labelAcceptableBadData.Text = "Acceptable Bad Data:";
            this.labelAcceptableBadData.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxFillInMissingTimestamps
            // 
            this.checkBoxFillInMissingTimestamps.AutoSize = true;
            this.checkBoxFillInMissingTimestamps.Checked = true;
            this.checkBoxFillInMissingTimestamps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFillInMissingTimestamps.Location = new System.Drawing.Point(11, 173);
            this.checkBoxFillInMissingTimestamps.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxFillInMissingTimestamps.Name = "checkBoxFillInMissingTimestamps";
            this.checkBoxFillInMissingTimestamps.Size = new System.Drawing.Size(146, 17);
            this.checkBoxFillInMissingTimestamps.TabIndex = 11;
            this.checkBoxFillInMissingTimestamps.Text = "Fill-in Missing Timestamps";
            this.checkBoxFillInMissingTimestamps.UseVisualStyleBackColor = true;
            this.checkBoxFillInMissingTimestamps.CheckedChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // checkBoxAlignTimestamps
            // 
            this.checkBoxAlignTimestamps.AutoSize = true;
            this.checkBoxAlignTimestamps.Checked = true;
            this.checkBoxAlignTimestamps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAlignTimestamps.Location = new System.Drawing.Point(11, 126);
            this.checkBoxAlignTimestamps.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxAlignTimestamps.Name = "checkBoxAlignTimestamps";
            this.checkBoxAlignTimestamps.Size = new System.Drawing.Size(180, 17);
            this.checkBoxAlignTimestamps.TabIndex = 9;
            this.checkBoxAlignTimestamps.Text = "Align Timestamps by Frame Rate";
            this.checkBoxAlignTimestamps.UseVisualStyleBackColor = true;
            this.checkBoxAlignTimestamps.CheckedChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // checkBoxExportMissingAsNaN
            // 
            this.checkBoxExportMissingAsNaN.AutoSize = true;
            this.checkBoxExportMissingAsNaN.Checked = true;
            this.checkBoxExportMissingAsNaN.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxExportMissingAsNaN.Location = new System.Drawing.Point(11, 150);
            this.checkBoxExportMissingAsNaN.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxExportMissingAsNaN.Name = "checkBoxExportMissingAsNaN";
            this.checkBoxExportMissingAsNaN.Size = new System.Drawing.Size(168, 17);
            this.checkBoxExportMissingAsNaN.TabIndex = 10;
            this.checkBoxExportMissingAsNaN.Text = "Export Missing Values as NaN";
            this.checkBoxExportMissingAsNaN.UseVisualStyleBackColor = true;
            this.checkBoxExportMissingAsNaN.CheckedChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelReadIntervalDetails
            // 
            this.labelReadIntervalDetails.AutoSize = true;
            this.labelReadIntervalDetails.Location = new System.Drawing.Point(201, 64);
            this.labelReadIntervalDetails.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelReadIntervalDetails.Name = "labelReadIntervalDetails";
            this.labelReadIntervalDetails.Size = new System.Drawing.Size(283, 13);
            this.labelReadIntervalDetails.TabIndex = 8;
            this.labelReadIntervalDetails.Text = "( if > 0, read values at timestamp then skip to next interval )";
            // 
            // labelSecondsReadInterval
            // 
            this.labelSecondsReadInterval.AutoSize = true;
            this.labelSecondsReadInterval.Location = new System.Drawing.Point(156, 64);
            this.labelSecondsReadInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSecondsReadInterval.Name = "labelSecondsReadInterval";
            this.labelSecondsReadInterval.Size = new System.Drawing.Size(47, 13);
            this.labelSecondsReadInterval.TabIndex = 7;
            this.labelSecondsReadInterval.Text = "seconds";
            // 
            // maskedTextBoxReadInterval
            // 
            this.maskedTextBoxReadInterval.Location = new System.Drawing.Point(111, 61);
            this.maskedTextBoxReadInterval.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxReadInterval.Mask = "00000";
            this.maskedTextBoxReadInterval.Name = "maskedTextBoxReadInterval";
            this.maskedTextBoxReadInterval.Size = new System.Drawing.Size(43, 20);
            this.maskedTextBoxReadInterval.TabIndex = 6;
            this.maskedTextBoxReadInterval.Text = "0";
            this.maskedTextBoxReadInterval.ValidatingType = typeof(int);
            this.maskedTextBoxReadInterval.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelReadInterval
            // 
            this.labelReadInterval.AutoSize = true;
            this.labelReadInterval.Location = new System.Drawing.Point(39, 64);
            this.labelReadInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelReadInterval.Name = "labelReadInterval";
            this.labelReadInterval.Size = new System.Drawing.Size(74, 13);
            this.labelReadInterval.TabIndex = 5;
            this.labelReadInterval.Text = "Read Interval:";
            this.labelReadInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // maskedTextBoxFrameRate
            // 
            this.maskedTextBoxFrameRate.Location = new System.Drawing.Point(111, 13);
            this.maskedTextBoxFrameRate.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxFrameRate.Mask = "000";
            this.maskedTextBoxFrameRate.Name = "maskedTextBoxFrameRate";
            this.maskedTextBoxFrameRate.Size = new System.Drawing.Size(31, 20);
            this.maskedTextBoxFrameRate.TabIndex = 1;
            this.maskedTextBoxFrameRate.Text = "30";
            this.maskedTextBoxFrameRate.ValidatingType = typeof(int);
            this.maskedTextBoxFrameRate.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // checkBoxEnableLogging
            // 
            this.checkBoxEnableLogging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxEnableLogging.AutoSize = true;
            this.checkBoxEnableLogging.Checked = true;
            this.checkBoxEnableLogging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableLogging.Location = new System.Drawing.Point(449, 12);
            this.checkBoxEnableLogging.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxEnableLogging.Name = "checkBoxEnableLogging";
            this.checkBoxEnableLogging.Size = new System.Drawing.Size(100, 17);
            this.checkBoxEnableLogging.TabIndex = 18;
            this.checkBoxEnableLogging.Text = "Enable Logging";
            this.checkBoxEnableLogging.UseVisualStyleBackColor = true;
            this.checkBoxEnableLogging.CheckedChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // maskedTextBoxMessageInterval
            // 
            this.maskedTextBoxMessageInterval.Location = new System.Drawing.Point(111, 37);
            this.maskedTextBoxMessageInterval.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxMessageInterval.Mask = "0000000";
            this.maskedTextBoxMessageInterval.Name = "maskedTextBoxMessageInterval";
            this.maskedTextBoxMessageInterval.Size = new System.Drawing.Size(53, 20);
            this.maskedTextBoxMessageInterval.TabIndex = 4;
            this.maskedTextBoxMessageInterval.Text = "30000";
            this.maskedTextBoxMessageInterval.ValidatingType = typeof(int);
            this.maskedTextBoxMessageInterval.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelMessageInterval
            // 
            this.labelMessageInterval.AutoSize = true;
            this.labelMessageInterval.Location = new System.Drawing.Point(22, 40);
            this.labelMessageInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMessageInterval.Name = "labelMessageInterval";
            this.labelMessageInterval.Size = new System.Drawing.Size(91, 13);
            this.labelMessageInterval.TabIndex = 3;
            this.labelMessageInterval.Text = "Message Interval:";
            this.labelMessageInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelSecondsMetadataTimeout
            // 
            this.labelSecondsMetadataTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSecondsMetadataTimeout.AutoSize = true;
            this.labelSecondsMetadataTimeout.Location = new System.Drawing.Point(498, 38);
            this.labelSecondsMetadataTimeout.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSecondsMetadataTimeout.Name = "labelSecondsMetadataTimeout";
            this.labelSecondsMetadataTimeout.Size = new System.Drawing.Size(47, 13);
            this.labelSecondsMetadataTimeout.TabIndex = 21;
            this.labelSecondsMetadataTimeout.Text = "seconds";
            // 
            // labelPerSec
            // 
            this.labelPerSec.AutoSize = true;
            this.labelPerSec.Location = new System.Drawing.Point(143, 16);
            this.labelPerSec.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPerSec.Name = "labelPerSec";
            this.labelPerSec.Size = new System.Drawing.Size(101, 13);
            this.labelPerSec.TabIndex = 2;
            this.labelPerSec.Text = "samples per second";
            // 
            // maskedTextBoxMetadataTimeout
            // 
            this.maskedTextBoxMetadataTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maskedTextBoxMetadataTimeout.Location = new System.Drawing.Point(466, 35);
            this.maskedTextBoxMetadataTimeout.Margin = new System.Windows.Forms.Padding(2);
            this.maskedTextBoxMetadataTimeout.Mask = "000";
            this.maskedTextBoxMetadataTimeout.Name = "maskedTextBoxMetadataTimeout";
            this.maskedTextBoxMetadataTimeout.Size = new System.Drawing.Size(31, 20);
            this.maskedTextBoxMetadataTimeout.TabIndex = 20;
            this.maskedTextBoxMetadataTimeout.Text = "60";
            this.maskedTextBoxMetadataTimeout.ValidatingType = typeof(int);
            this.maskedTextBoxMetadataTimeout.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // labelFrameRate
            // 
            this.labelFrameRate.AutoSize = true;
            this.labelFrameRate.Location = new System.Drawing.Point(5, 16);
            this.labelFrameRate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelFrameRate.Name = "labelFrameRate";
            this.labelFrameRate.Size = new System.Drawing.Size(108, 13);
            this.labelFrameRate.TabIndex = 0;
            this.labelFrameRate.Text = "Frame Rate Estimate:";
            this.labelFrameRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelMetaDataTimeout
            // 
            this.labelMetaDataTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMetaDataTimeout.AutoSize = true;
            this.labelMetaDataTimeout.Location = new System.Drawing.Point(373, 38);
            this.labelMetaDataTimeout.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMetaDataTimeout.Name = "labelMetaDataTimeout";
            this.labelMetaDataTimeout.Size = new System.Drawing.Size(96, 13);
            this.labelMetaDataTimeout.TabIndex = 19;
            this.labelMetaDataTimeout.Text = "Metadata Timeout:";
            this.labelMetaDataTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelBadDataPercent
            // 
            this.labelBadDataPercent.AutoSize = true;
            this.labelBadDataPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBadDataPercent.Location = new System.Drawing.Point(168, 220);
            this.labelBadDataPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelBadDataPercent.Name = "labelBadDataPercent";
            this.labelBadDataPercent.Size = new System.Drawing.Size(15, 13);
            this.labelBadDataPercent.TabIndex = 22;
            this.labelBadDataPercent.Text = "%";
            // 
            // labelMissingDataPercent
            // 
            this.labelMissingDataPercent.AutoSize = true;
            this.labelMissingDataPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMissingDataPercent.Location = new System.Drawing.Point(168, 267);
            this.labelMissingDataPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMissingDataPercent.Name = "labelMissingDataPercent";
            this.labelMissingDataPercent.Size = new System.Drawing.Size(15, 13);
            this.labelMissingDataPercent.TabIndex = 24;
            this.labelMissingDataPercent.Text = "%";
            // 
            // labelBadTimePercent
            // 
            this.labelBadTimePercent.AutoSize = true;
            this.labelBadTimePercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBadTimePercent.Location = new System.Drawing.Point(168, 244);
            this.labelBadTimePercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelBadTimePercent.Name = "labelBadTimePercent";
            this.labelBadTimePercent.Size = new System.Drawing.Size(15, 13);
            this.labelBadTimePercent.TabIndex = 23;
            this.labelBadTimePercent.Text = "%";
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.BackColor = System.Drawing.Color.Transparent;
            this.tabPageAdvanced.Controls.Add(this.labelFilterExpression);
            this.tabPageAdvanced.Controls.Add(this.textBoxFilterExpression);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAdvanced.Size = new System.Drawing.Size(600, 341);
            this.tabPageAdvanced.TabIndex = 1;
            this.tabPageAdvanced.Text = "Advanced Selection";
            // 
            // labelFilterExpression
            // 
            this.labelFilterExpression.AutoSize = true;
            this.labelFilterExpression.Location = new System.Drawing.Point(5, 11);
            this.labelFilterExpression.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelFilterExpression.Name = "labelFilterExpression";
            this.labelFilterExpression.Size = new System.Drawing.Size(86, 13);
            this.labelFilterExpression.TabIndex = 0;
            this.labelFilterExpression.Text = "Filter Expression:";
            this.labelFilterExpression.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxFilterExpression
            // 
            this.textBoxFilterExpression.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilterExpression.Location = new System.Drawing.Point(7, 27);
            this.textBoxFilterExpression.Multiline = true;
            this.textBoxFilterExpression.Name = "textBoxFilterExpression";
            this.textBoxFilterExpression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxFilterExpression.Size = new System.Drawing.Size(587, 308);
            this.textBoxFilterExpression.TabIndex = 1;
            this.textBoxFilterExpression.Text = "FILTER ActiveMeasurements WHERE {0}";
            this.textBoxFilterExpression.TextChanged += new System.EventHandler(this.FormElementChanged);
            // 
            // groupBoxExportOptions
            // 
            this.groupBoxExportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxExportOptions.Controls.Add(this.checkBoxExportFilePerDataType);
            this.groupBoxExportOptions.Controls.Add(this.labelExportFileName);
            this.groupBoxExportOptions.Controls.Add(this.buttonSelectFile);
            this.groupBoxExportOptions.Controls.Add(this.textBoxExportFileName);
            this.groupBoxExportOptions.Controls.Add(this.radioButtonCOMTRADE);
            this.groupBoxExportOptions.Controls.Add(this.radioButtonCSV);
            this.groupBoxExportOptions.Location = new System.Drawing.Point(12, 252);
            this.groupBoxExportOptions.Name = "groupBoxExportOptions";
            this.groupBoxExportOptions.Size = new System.Drawing.Size(200, 138);
            this.groupBoxExportOptions.TabIndex = 1;
            this.groupBoxExportOptions.TabStop = false;
            this.groupBoxExportOptions.Text = "E&xport Options";
            // 
            // checkBoxExportFilePerDataType
            // 
            this.checkBoxExportFilePerDataType.AutoSize = true;
            this.checkBoxExportFilePerDataType.Checked = true;
            this.checkBoxExportFilePerDataType.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxExportFilePerDataType.Location = new System.Drawing.Point(8, 109);
            this.checkBoxExportFilePerDataType.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxExportFilePerDataType.Name = "checkBoxExportFilePerDataType";
            this.checkBoxExportFilePerDataType.Size = new System.Drawing.Size(158, 17);
            this.checkBoxExportFilePerDataType.TabIndex = 12;
            this.checkBoxExportFilePerDataType.Text = "Export one file per data type";
            this.checkBoxExportFilePerDataType.UseVisualStyleBackColor = true;
            this.checkBoxExportFilePerDataType.CheckedChanged += new System.EventHandler(this.checkBoxExportFilePerDataType_CheckedChanged);
            // 
            // labelExportFileName
            // 
            this.labelExportFileName.AutoSize = true;
            this.labelExportFileName.Location = new System.Drawing.Point(5, 67);
            this.labelExportFileName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelExportFileName.Name = "labelExportFileName";
            this.labelExportFileName.Size = new System.Drawing.Size(85, 13);
            this.labelExportFileName.TabIndex = 11;
            this.labelExportFileName.Tag = "";
            this.labelExportFileName.Text = "Export file name:";
            // 
            // buttonSelectFile
            // 
            this.buttonSelectFile.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSelectFile.Location = new System.Drawing.Point(164, 80);
            this.buttonSelectFile.Name = "buttonSelectFile";
            this.buttonSelectFile.Size = new System.Drawing.Size(30, 22);
            this.buttonSelectFile.TabIndex = 3;
            this.buttonSelectFile.Text = "...";
            this.buttonSelectFile.UseVisualStyleBackColor = true;
            this.buttonSelectFile.Click += new System.EventHandler(this.buttonSelectFile_Click);
            // 
            // textBoxExportFileName
            // 
            this.textBoxExportFileName.Location = new System.Drawing.Point(6, 81);
            this.textBoxExportFileName.Name = "textBoxExportFileName";
            this.textBoxExportFileName.Size = new System.Drawing.Size(161, 20);
            this.textBoxExportFileName.TabIndex = 2;
            this.textBoxExportFileName.TextChanged += new System.EventHandler(this.textBoxExportFileName_TextChanged);
            // 
            // radioButtonCOMTRADE
            // 
            this.radioButtonCOMTRADE.AutoSize = true;
            this.radioButtonCOMTRADE.Enabled = false;
            this.radioButtonCOMTRADE.Location = new System.Drawing.Point(12, 41);
            this.radioButtonCOMTRADE.Name = "radioButtonCOMTRADE";
            this.radioButtonCOMTRADE.Size = new System.Drawing.Size(86, 17);
            this.radioButtonCOMTRADE.TabIndex = 1;
            this.radioButtonCOMTRADE.TabStop = true;
            this.radioButtonCOMTRADE.Text = "COMTRADE";
            this.radioButtonCOMTRADE.UseVisualStyleBackColor = true;
            // 
            // radioButtonCSV
            // 
            this.radioButtonCSV.AutoSize = true;
            this.radioButtonCSV.Checked = true;
            this.radioButtonCSV.Location = new System.Drawing.Point(12, 22);
            this.radioButtonCSV.Name = "radioButtonCSV";
            this.radioButtonCSV.Size = new System.Drawing.Size(46, 17);
            this.radioButtonCSV.TabIndex = 0;
            this.radioButtonCSV.TabStop = true;
            this.radioButtonCSV.Text = "CSV";
            this.radioButtonCSV.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "csv";
            this.saveFileDialog.Filter = "CSV Files|*.csv|All Files|*.*";
            this.saveFileDialog.Title = "Select Export File Name";
            // 
            // tabPageMessages
            // 
            this.tabPageMessages.Controls.Add(this.textBoxMessageOutput);
            this.tabPageMessages.Location = new System.Drawing.Point(4, 22);
            this.tabPageMessages.Name = "tabPageMessages";
            this.tabPageMessages.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMessages.Size = new System.Drawing.Size(600, 341);
            this.tabPageMessages.TabIndex = 3;
            this.tabPageMessages.Text = "Messages";
            this.tabPageMessages.UseVisualStyleBackColor = true;
            // 
            // textBoxMessageOutput
            // 
            this.textBoxMessageOutput.BackColor = System.Drawing.SystemColors.WindowText;
            this.textBoxMessageOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxMessageOutput.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxMessageOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.textBoxMessageOutput.Location = new System.Drawing.Point(3, 3);
            this.textBoxMessageOutput.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxMessageOutput.Multiline = true;
            this.textBoxMessageOutput.Name = "textBoxMessageOutput";
            this.textBoxMessageOutput.ReadOnly = true;
            this.textBoxMessageOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessageOutput.Size = new System.Drawing.Size(594, 335);
            this.textBoxMessageOutput.TabIndex = 1;
            this.textBoxMessageOutput.TabStop = false;
            // 
            // textBoxPhaseFilter
            // 
            this.textBoxPhaseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxPhaseFilter.Location = new System.Drawing.Point(12, 313);
            this.textBoxPhaseFilter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxPhaseFilter.Name = "textBoxPhaseFilter";
            this.textBoxPhaseFilter.Size = new System.Drawing.Size(79, 20);
            this.textBoxPhaseFilter.TabIndex = 19;
            this.textBoxPhaseFilter.Text = "A,B,C,+,-,0";
            this.textBoxPhaseFilter.TextChanged += new System.EventHandler(this.textBoxPhaseFilter_TextChanged);
            // 
            // labelPhaseFilter
            // 
            this.labelPhaseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPhaseFilter.AutoSize = true;
            this.labelPhaseFilter.Location = new System.Drawing.Point(9, 298);
            this.labelPhaseFilter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPhaseFilter.Name = "labelPhaseFilter";
            this.labelPhaseFilter.Size = new System.Drawing.Size(65, 13);
            this.labelPhaseFilter.TabIndex = 18;
            this.labelPhaseFilter.Text = "Phase Filter:";
            this.labelPhaseFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelMeasurementCount
            // 
            this.labelMeasurementCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMeasurementCount.AutoSize = true;
            this.labelMeasurementCount.Location = new System.Drawing.Point(352, 50);
            this.labelMeasurementCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMeasurementCount.Name = "labelMeasurementCount";
            this.labelMeasurementCount.Size = new System.Drawing.Size(152, 13);
            this.labelMeasurementCount.TabIndex = 20;
            this.labelMeasurementCount.Tag = "";
            this.labelMeasurementCount.Text = "{0:N0} measurements selected";
            this.labelMeasurementCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 431);
            this.Controls.Add(this.groupBoxExportOptions);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBoxServerConnection);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(860, 470);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GEP Data Extractor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBoxServerConnection.ResumeLayout(false);
            this.groupBoxServerConnection.PerformLayout();
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.tabControlOptions.ResumeLayout(false);
            this.tabPageTimeRange.ResumeLayout(false);
            this.tabPageTimeRange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDevices)).EndInit();
            this.tabPageSettings.ResumeLayout(false);
            this.tabPageSettings.PerformLayout();
            this.tabPageAdvanced.ResumeLayout(false);
            this.tabPageAdvanced.PerformLayout();
            this.groupBoxExportOptions.ResumeLayout(false);
            this.groupBoxExportOptions.PerformLayout();
            this.tabPageMessages.ResumeLayout(false);
            this.tabPageMessages.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxServerConnection;
        private System.Windows.Forms.Label labelInstance;
        public System.Windows.Forms.TextBox textBoxHistorianInstanceName;
        private System.Windows.Forms.Label labeServer;
        public System.Windows.Forms.TextBox textBoxHistorianHostAddress;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxHistorianPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TabControl tabControlOptions;
        private System.Windows.Forms.TabPage tabPageTimeRange;
        private System.Windows.Forms.Label labelEndTime;
        public System.Windows.Forms.DateTimePicker dateTimePickerEndTime;
        private System.Windows.Forms.Label labelStartTime;
        public System.Windows.Forms.DateTimePicker dateTimePickerSourceTime;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.Label labelFilterExpression;
        public System.Windows.Forms.TextBox textBoxFilterExpression;
        private System.Windows.Forms.TabPage tabPageSettings;
        public System.Windows.Forms.CheckBox checkBoxFillInMissingTimestamps;
        public System.Windows.Forms.CheckBox checkBoxAlignTimestamps;
        public System.Windows.Forms.CheckBox checkBoxExportMissingAsNaN;
        private System.Windows.Forms.Label labelReadIntervalDetails;
        private System.Windows.Forms.Label labelSecondsReadInterval;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxReadInterval;
        private System.Windows.Forms.Label labelReadInterval;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxFrameRate;
        public System.Windows.Forms.CheckBox checkBoxEnableLogging;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxMessageInterval;
        private System.Windows.Forms.Label labelMessageInterval;
        private System.Windows.Forms.Label labelSecondsMetadataTimeout;
        private System.Windows.Forms.Label labelPerSec;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxMetadataTimeout;
        private System.Windows.Forms.Label labelFrameRate;
        private System.Windows.Forms.Label labelMetaDataTimeout;
        private System.Windows.Forms.Label labelDataTypes;
        private System.Windows.Forms.CheckedListBox checkedListBoxDataTypes;
        private System.Windows.Forms.Label labelDevices;
        private System.Windows.Forms.DataGridView dataGridViewDevices;
        public System.Windows.Forms.CheckBox checkBoxSelectAllDevices;
        private System.Windows.Forms.Button buttonPreFilter;
        private System.Windows.Forms.Label labelSelectCount;
        private System.Windows.Forms.GroupBox groupBoxExportOptions;
        public System.Windows.Forms.CheckBox checkBoxExportFilePerDataType;
        private System.Windows.Forms.Label labelExportFileName;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.RadioButton radioButtonCOMTRADE;
        private System.Windows.Forms.RadioButton radioButtonCSV;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxAcceptableMissingData;
        private System.Windows.Forms.Label labelAcceptableMissingData;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxAcceptableBadTime;
        private System.Windows.Forms.Label labelAccetableBadTime;
        public System.Windows.Forms.MaskedTextBox maskedTextBoxAcceptableBadData;
        private System.Windows.Forms.Label labelAcceptableBadData;
        private System.Windows.Forms.Label labelBadDataPercent;
        private System.Windows.Forms.Label labelMissingDataPercent;
        private System.Windows.Forms.Label labelBadTimePercent;
        private System.Windows.Forms.Button buttonCancelPreFilter;
        private System.Windows.Forms.Button buttonCancelExport;
        private System.Windows.Forms.Button buttonShowGraph;
        public System.Windows.Forms.TextBox textBoxExportFileName;
        private System.Windows.Forms.Label labelStartTimeUTC;
        private System.Windows.Forms.Label labelEndTimeUTC;
        private System.Windows.Forms.LinkLabel linkLabelOpenExportPage;
        private System.Windows.Forms.Button buttonRestoreDefaults;
        public System.Windows.Forms.TextBox textBoxDeviceFilter;
        private System.Windows.Forms.Label labelDeviceFilter;
        private System.Windows.Forms.TabPage tabPageMessages;
        private System.Windows.Forms.TextBox textBoxMessageOutput;
        public System.Windows.Forms.TextBox textBoxPhaseFilter;
        private System.Windows.Forms.Label labelPhaseFilter;
        private System.Windows.Forms.Label labelMeasurementCount;
    }
}

