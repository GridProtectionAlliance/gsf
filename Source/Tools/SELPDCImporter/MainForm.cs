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
using System.Xml.Linq;
using GSF.ComponentModel;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Windows.Forms;

namespace SELPDCImporter
{
    public partial class MainForm : Form
    {
        private Settings m_settings;
        private readonly LogPublisher m_log;
        private string m_hostApp;
        private bool m_formLoaded;
        private volatile bool m_formClosing;
        private AdoDataConnection m_connection;
        private Process m_consoleProcess;

        public MainForm()
        {
            InitializeComponent();

            // Create a new log publisher instance
            m_log = Logger.CreatePublisher(typeof(MainForm), MessageClass.Application);
        }

        private string HostApp
        {
            get => m_hostApp;
            set
            {
                m_hostApp = value;
                Text = string.IsNullOrWhiteSpace(m_hostApp) ? Tag.ToString() : $"{Tag} - Targeting {m_hostApp}";
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
                m_log.Publish(MessageLevel.Error, "FormLoad", "Failed while loading settings", exception: ex);

            #if DEBUG
                throw;
            #else
                MessageBox.Show(this, $"Failed during initialization: {ex.Message}", "Initialization Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            #endif
            }
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

                // Close associated console process
                m_consoleProcess.Close();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormClosing", "Failed while saving settings", exception: ex);

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

        // Attempt to auto-load initial host configuration file (useful when running from host service folder)
        private string LookupHostConfig()
        {
            // Search through list of known source applications (faster than manual search)
            foreach (string sourceApp in new[] { "openPDC", "openHistorian", "SIEGate" })
            {
                string absolutePath = FilePath.GetAbsolutePath($"{sourceApp}.exe.config");

                if (File.Exists(absolutePath))
                {
                    HostApp = sourceApp;
                    return absolutePath;
                }
            }

            // Fall back on manual search
            foreach (string configFile in FilePath.GetFileList($"{FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""))}*.exe.config"))
            {
                if (IsHostConfig(configFile))
                    return configFile;
            }

            return "";
        }

        private bool IsHostConfig(string configFile)
        {
            try
            {
                XDocument serviceConfig = XDocument.Load(configFile);

                string applicationName = serviceConfig
                     .Descendants("securityProvider")
                     .SelectMany(systemSettings => systemSettings.Elements("add"))
                     .Where(element => "ApplicationName".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                     .Select(element => (string)element.Attribute("value"))
                     .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(applicationName))
                    return false;

                if (applicationName.Trim().Equals(FilePath.GetFileNameWithoutExtension(FilePath.GetFileNameWithoutExtension(configFile)), StringComparison.OrdinalIgnoreCase))
                {
                    HostApp = applicationName;
                    return true;
                }
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Warning, "IsHostConfig", $"Failed parse config file \"{configFile}\".", exception: ex);
            }

            return false;
        }

        private void LoadHostConfigFile(string configFile)
        {
            if (!IsHostConfig(configFile))
            {
                MessageBox.Show(this, $"Import failed: The configuration file \"{configFile}\" is not a valid host service configuration.", "Load Host Config File Issue", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            XDocument serviceConfig = XDocument.Load(configFile);

            string connectionString = serviceConfig
                .Descendants("systemSettings")
                .SelectMany(systemSettings => systemSettings.Elements("add"))
                .Where(element => "ConnectionString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Select(element => (string)element.Attribute("value"))
                .FirstOrDefault();

            string dataProviderString = serviceConfig
                .Descendants("systemSettings")
                .SelectMany(systemSettings => systemSettings.Elements("add"))
                .Where(element => "DataProviderString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Select(element => (string)element.Attribute("value"))
                .FirstOrDefault();

            m_connection = new AdoDataConnection(connectionString, dataProviderString);

            m_consoleProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = $"{FilePath.AddPathSuffix(Path.GetDirectoryName(configFile))}{HostApp}Console.exe"
                }
            };

            // Example to send command to console:
            // m_consoleProcess.StandardInput.WriteLine("ReloadConfig");

            m_consoleProcess.Start();
        }

    }
}
