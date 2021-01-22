//******************************************************************************************************
//  MainForm.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/30/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GSF;
using GSF.ComponentModel;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Windows.Forms;
using SELPDCImporter.Model;

namespace SELPDCImporter
{
    public partial class MainForm : Form
    {
        private Settings m_settings;
        private readonly LogPublisher m_log;
        private string m_hostApp;
        private bool m_formLoaded;
        private bool m_initialShow;
        private volatile bool m_formClosing;
        private ConfigurationFrame m_configFrame;
        private AdoDataConnection m_connection;
        private Process m_consoleProcess;
        private Guid m_nodeID;

        public MainForm()
        {
            InitializeComponent();

            // Create a new log publisher instance
            m_log = Logger.CreatePublisher(typeof(MainForm), MessageClass.Application);
            m_initialShow = true;
        }

        private string HostApp
        {
            get => m_hostApp;
            set
            {
                m_hostApp = value;
                Text = string.IsNullOrWhiteSpace(m_hostApp) ? Tag.ToString() : $"{Tag} => Targeting \"{m_hostApp}\"";
            }
        }

        private void SELPDCImporter_Load(object sender, EventArgs e)
        {
            try
            {
                // Save original title text in form tag
                Tag = Text;

                // Save original label text in its tag
                labelAnalyzeStatus.Tag = labelAnalyzeStatus.Text;

                // Load current settings registering a symbolic reference to this form instance for use by default value expressions
                m_settings = new Settings(new Dictionary<string, object> {{ "Form", this }}.RegisterSymbols());

                // Restore last window size/location
                this.RestoreLayout();

                if (string.IsNullOrWhiteSpace(m_settings.HostConfig) || !File.Exists(m_settings.HostConfig))
                    m_settings.HostConfig = LookupHostConfig();

                m_formLoaded = true;
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormLoad", exception: ex);

            #if DEBUG
                throw;
            #else
                MessageBox.Show(this, $"Failed during initialization: {ex.Message}", "Initialization Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            #endif
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!m_initialShow || m_formClosing)
                return;

            m_initialShow = false;

            if (string.IsNullOrWhiteSpace(textBoxHostConfig.Text))
                textBoxHostConfig.Focus();
            else
                textBoxPDCConfig.Focus();
        }

        private void SELPDCImporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_formClosing = true;

                // Save current window size/location
                this.SaveLayout();

                // Save any updates to current screen values
                m_settings.Save();

                // Close any active database connection
                m_connection?.Dispose();

                // Close associated console process
                StopConsoleProcess();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormClosing", exception: ex);

            #if DEBUG
                throw;
            #endif
            }
        }

        private void SELPDCImporter_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_settings?.Dispose();
        }

        private void FormElementChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, EventArgs>(FormElementChanged), sender, e);
            }
            else
            {
                if (Visible && m_formLoaded)
                    m_settings?.UpdateProperties();
            }
        }

        private void textBoxPDCConfig_TextChanged(object sender, EventArgs e)
        {
            buttonAnalyze.Enabled = true;
            labelAnalyzeStatus.Text = $"{labelAnalyzeStatus.Tag}";
            textBoxPDCDetails.Text = "";
        }

        private void buttonBrowseHostConfig_Click(object sender, EventArgs e)
        {
            openFileDialogHostConfig.FileName = textBoxHostConfig.Text;

            if (openFileDialogHostConfig.ShowDialog(this) != DialogResult.OK)
                return;

            textBoxHostConfig.Text = openFileDialogHostConfig.FileName;
        }

        private void buttonBrowsePDCConfig_Click(object sender, EventArgs e)
        {
            openFileDialogPDCConfig.FileName = textBoxPDCConfig.Text;

            if (openFileDialogPDCConfig.ShowDialog(this) != DialogResult.OK)
                return;

            textBoxPDCConfig.Text = openFileDialogPDCConfig.FileName;

            string hostConfigFile = textBoxHostConfig.Text;

            // Auto analyze after PDC config file selection assuming host config looks OK
            if (File.Exists(hostConfigFile) && IsHostConfig(hostConfigFile))
                buttonAnalyze_Click(sender, e);
        }

        private void buttonAnalyze_Click(object sender, EventArgs e)
        {
            string hostConfigFile = textBoxHostConfig.Text;

            if (!File.Exists(hostConfigFile))
            {
                MessageBox.Show(this, $"Analyze failed: The specified host service configuration file \"{hostConfigFile}\" does not exist.", "Load Host Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pdcConfigFile = textBoxPDCConfig.Text;

            if (!File.Exists(pdcConfigFile))
            {
                MessageBox.Show(this, $"Analyze failed: The specified PDC configuration file \"{pdcConfigFile}\" does not exist.", "Load PDC Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Ticks startTime = DateTime.UtcNow.Ticks;

            if (!IsHostConfig(hostConfigFile))
            {
                MessageBox.Show(this, $"Analyze failed: The configuration file \"{hostConfigFile}\" is not a valid host service configuration.", "Load Host Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                LoadHostConfigFile(hostConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Analyze failed: Iniitialization failure using specified host service configuration \"{hostConfigFile}\": {ex.Message}", "Load Host Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                m_configFrame = SELPDCConfig.Parse(pdcConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Analyze failed: Failed while parsing PDC configuration \"{pdcConfigFile}\": {ex.Message}", "Load PDC Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            labelAnalyzeStatus.Text = $"Analyze completed in {(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2)}.";

            StringBuilder pdcDetails = new StringBuilder();

            pdcDetails.AppendLine($"Acronym: {m_configFrame.Acronym}");
            pdcDetails.AppendLine($"Name: {m_configFrame.Name}");
            pdcDetails.AppendLine($"ID Code: {m_configFrame.IDCode:N0}");
            pdcDetails.AppendLine($"Data Rate: {m_configFrame.FrameRate:N0} frames / second");
            pdcDetails.AppendLine($"IP Addresses: {string.Join(", ", m_configFrame.DeviceIPs.Values)}");
            pdcDetails.AppendLine($"PMU Count: {m_configFrame.Cells.Count:N0}");

            for (int i = 0; i < m_configFrame.Cells.Count; i++)
            {
                ConfigurationCell configCell = m_configFrame.Cells[i];

                pdcDetails.AppendLine();
                pdcDetails.AppendLine($"PMU {i + 1:N0} Details:");
                pdcDetails.AppendLine($"   Acronym: {configCell.IDLabel}");
                pdcDetails.AppendLine($"   Name: {configCell.StationName}");
                pdcDetails.AppendLine($"   ID Code: {configCell.IDCode:N0}");
                pdcDetails.AppendLine($"   Nominal Frequency: {(int)configCell.NominalFrequency}Hz");
                pdcDetails.AppendLine($"   Phasor Count: {configCell.PhasorDefinitions.Count:N0}");
                pdcDetails.AppendLine($"   Analog Count: {configCell.AnalogDefinitions.Count:N0}");
                pdcDetails.AppendLine($"   Digital Count: {configCell.DigitalDefinitions.Count:N0}");

                for (int j = 0; j < configCell.PhasorDefinitions.Count; j++)
                {
                    if (configCell.PhasorDefinitions[j] is not PhasorDefinition phasor)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Phasor {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {phasor.Label}");
                    pdcDetails.AppendLine($"      Type: {phasor.PhasorType}");
                    pdcDetails.AppendLine($"      Phase: {phasor.Phase}");
                    pdcDetails.AppendLine($"      Description: {phasor.Description}");
                }

                for (int j = 0; j < configCell.AnalogDefinitions.Count; j++)
                {
                    if (configCell.AnalogDefinitions[j] is not AnalogDefinition analog)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Analog {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {analog.Label}");
                    pdcDetails.AppendLine($"      Type: {analog.AnalogType}");
                    pdcDetails.AppendLine($"      Description: {analog.Description}");
                }

                for (int j = 0; j < configCell.DigitalDefinitions.Count; j++)
                {
                    if (configCell.DigitalDefinitions[j] is not DigitalDefinition digital)
                        continue;

                    pdcDetails.AppendLine();
                    pdcDetails.AppendLine($"   Digital {j + 1:N0} Details:");
                    pdcDetails.AppendLine($"      Name: {digital.Label}");
                    pdcDetails.AppendLine($"      Description: {digital.Description}");
                }
            }

            textBoxPDCDetails.Text = pdcDetails.ToString();

            buttonImport.Enabled = m_configFrame.Cells.Count > 0;
            buttonAnalyze.Enabled = !buttonImport.Enabled;

            comboBoxIPAddresses.DataSource = new BindingSource(m_configFrame.DeviceIPs, null);
            comboBoxIPAddresses.SelectedValue = m_configFrame.TargetDeviceIP;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            try
            {
                TableOperations<Device> deviceTable = new TableOperations<Device>(m_connection);

                SaveDeviceConfiguration(m_configFrame, new ImportParameters
                {
                    Connection = m_connection,
                    DeviceTable = deviceTable,
                    NodeID = m_nodeID,
                    HostConfig = textBoxHostConfig.Text,
                    Devices = deviceTable.QueryRecords().ToArray()
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Import failed: Failed while importing PDC configuration: {ex.Message}", "Import PDC Config Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxIPAddresses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxIPAddresses.SelectedItem is KeyValuePair<string, string> kvp)
                textBoxConnectionString.Text = m_configFrame.ConnectionString.Replace(SELPDCConfig.IPAddressToken, kvp.Value);
        }
    }
}
