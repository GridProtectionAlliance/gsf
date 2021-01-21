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
using System.Windows.Forms;
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
                m_consoleProcess?.Close();
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

            if (!LoadHostConfigFile(hostConfigFile))
                return;

            try
            {
                m_configFrame = SELPDCConfig.Parse(pdcConfigFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Analyze failed: Failed while parsing PDC configuration \"{pdcConfigFile}\": {ex.Message}", "Load PDC Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            buttonImport.Enabled = m_configFrame.Cells.Count > 0;

        #if DEBUG
            MessageBox.Show(this, $"Loaded {m_configFrame.Cells.Count:N0} PMU configurations");
        #endif
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
    }
}
