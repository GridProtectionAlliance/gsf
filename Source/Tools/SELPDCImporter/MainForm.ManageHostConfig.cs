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
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;

namespace SELPDCImporter
{
    partial class MainForm
    {
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

        private void LoadHostConfigFile(string configFile)
        {
            XDocument serviceConfig = XDocument.Load(configFile);

            m_nodeID = Guid.Parse(serviceConfig
                .Descendants("systemSettings")
                .SelectMany(systemSettings => systemSettings.Elements("add"))
                .Where(element => "NodeID".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Select(element => (string)element.Attribute("value"))
                .FirstOrDefault() ?? Guid.Empty.ToString());

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
