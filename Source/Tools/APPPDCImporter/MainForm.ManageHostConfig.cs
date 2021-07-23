//******************************************************************************************************
//  MainForm.ManageHostConfig.cs - Gbtc
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
//  12/31/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GSF;
using GSF.Diagnostics;
using GSF.IO;

namespace APPPDCImporter
{
    partial class MainForm
    {
        private Process m_consoleProcess;

        // Attempt to auto-load initial host configuration file (useful when running from host service folder)
        private string LookupHostConfig()
        {
            // Search through list of known source applications (faster than manual search)
            foreach (string hostApp in new[] { "openPDC", "openHistorian", "SIEGate" })
            {
                string absolutePath = FilePath.GetAbsolutePath($"{hostApp}.exe.config");

                if (File.Exists(absolutePath))
                {
                    HostApp = hostApp;
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

        private void LoadHostConfigFile(string configFile, ImportParameters importParams)
        {
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

            Guid nodeID = Guid.Parse(serviceConfig
                .Descendants("systemSettings")
                .SelectMany(systemSettings => systemSettings.Elements("add"))
                .Where(element => "NodeID".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Select(element => (string)element.Attribute("value"))
                .FirstOrDefault() ?? Guid.Empty.ToString());

            importParams.InitializeConnection(connectionString, dataProviderString, nodeID);

            StartConsoleProcess(configFile);
        }

        private void StartConsoleProcess(string configFile)
        {
            // Stop any existing running process
            StopConsoleProcess();

            m_consoleProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = $"{FilePath.AddPathSuffix(Path.GetDirectoryName(configFile))}{HostApp}Console.exe"
                },
                EnableRaisingEvents = true
            };

            // Example to send command to console:
            // m_consoleProcess.StandardInput.WriteLine("ReloadConfig");

            m_consoleProcess.OutputDataReceived += m_consoleProcess_DataReceived;
            m_consoleProcess.ErrorDataReceived += m_consoleProcess_DataReceived;
            m_consoleProcess.Exited += m_consoleProcess_Exited;
            m_consoleProcess.Start();
            m_consoleProcess.BeginOutputReadLine();
        }

        private void StopConsoleProcess()
        {
            if (m_consoleProcess is null)
                return;

            m_consoleProcess.OutputDataReceived -= m_consoleProcess_DataReceived;
            m_consoleProcess.ErrorDataReceived -= m_consoleProcess_DataReceived;
            m_consoleProcess.Exited -= m_consoleProcess_Exited;
            m_consoleProcess.Close();
            m_consoleProcess.Dispose();
            m_consoleProcess = null;
        }

        private void m_consoleProcess_Exited(object sender, EventArgs e)
        {
            if (m_formClosing)
                return;

            AppendConsoleText($"Console process exited. Attempting restart...{Environment.NewLine}");

            try
            {
                StartConsoleProcess(textBoxHostConfig.Text);
            }
            catch (Exception ex)
            {
                AppendConsoleText($"Exception attempting to start console process: {ex.Message}{Environment.NewLine}");
            }
        }

        private void m_consoleProcess_DataReceived(object sender, DataReceivedEventArgs e) => 
            AppendConsoleText(e.Data);

        private void AppendConsoleText(string text)
        {
            if (m_formClosing)
                return;

            BeginInvoke(new Action(() =>
            {
                if (m_formClosing)
                    return;

                int maxOutput = textBoxConsoleOutput.MaxLength;

                string output = $"{textBoxConsoleOutput.Text}{text}{Environment.NewLine}";

                if (output.Length > maxOutput)
                    output = output.TruncateLeft(maxOutput);

                textBoxConsoleOutput.Text = output;
                textBoxConsoleOutput.SelectionStart = output.Length;
                textBoxConsoleOutput.ScrollToCaret();

                tabPageHostConnection.ToolTipText = $"{textBoxConsoleOutput.Lines.Length / 2:N0} messages...";
            }));
        }
    }
}
