//******************************************************************************************************
//  Main.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  03/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using PhasorProtocolAdapters.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using GSF.Data;
using GSF.Data.Model;
using GSF.ComponentModel;
using GSF.Windows.Forms;
using GSF.Diagnostics;
using System.Diagnostics;
using System.ServiceProcess;
using GSF.IO;

namespace UpdateTagNames
{
    public partial class Main : Form
    {
        private Settings m_settings;
        private readonly LogPublisher m_log;
        private bool m_formLoaded;
        private volatile bool m_formClosing;

        public Main()
        {
            InitializeComponent();

            // Create a new log publisher instance
            m_log = Logger.CreatePublisher(typeof(Main), MessageClass.Application);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                // Load current settings registering a symbolic reference to this form instance for use by default value expressions
                m_settings = new Settings(new Dictionary<string, object> { { "Form", this } }.RegisterSymbols());

                // Restore last window size/location
                this.RestoreLayout();

                m_formLoaded = true;
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormLoad", "Failed while loading settings", exception: ex);

#if DEBUG
                throw;
#endif
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                m_formClosing = true;

                // Save current window size/location
                this.SaveLayout();

                // Save any updates to current screen values
                m_settings.Save();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "FormClosing", "Failed while saving settings", exception: ex);

#if DEBUG
                throw;
#endif
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_settings?.Dispose();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (!m_formLoaded || m_formClosing)
                return;

            string serviceConfigFile = FilePath.GetAbsolutePath(textBoxConfigFile.Text);

            if (!File.Exists(serviceConfigFile))
            {
                MessageBox.Show(this, $"Cannot find config file \"{serviceConfigFile}\".", "Cannot Apply Changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                XDocument serviceConfig = XDocument.Load(serviceConfigFile);

                //Guid nodeID = Guid.Parse(serviceConfig
                //    .Descendants("systemSettings")
                //    .SelectMany(systemSettings => systemSettings.Elements("add"))
                //    .Where(element => "NodeID".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                //    .Select(element => (string)element.Attribute("value"))
                //    .FirstOrDefault() ?? Guid.Empty.ToString());

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

                string serviceName = serviceConfig
                    .Descendants("securityProvider")
                    .SelectMany(securitySettings => securitySettings.Elements("add"))
                    .Where(element => "ApplicationName".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault();

                string managerConfigFile = $"{FilePath.GetDirectoryName(serviceConfigFile)}{serviceName}Manager.exe.config";
                XDocument managerConfig = null;
                bool applyManagerChanges = true;

                if (!File.Exists(managerConfigFile))
                {
                    if (MessageBox.Show(this, $"Failed to associated manager config file \"{managerConfigFile}\". Do you want to continue with changes to service only?", "Manager Config Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;

                    applyManagerChanges = false;
                }

                if (applyManagerChanges)
                    managerConfig = XDocument.Load(managerConfigFile);

                // Attempt to access service controller for the specified Windows service
                ServiceController serviceController = ServiceController.GetServices().SingleOrDefault(svc => string.Compare(svc.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase) == 0);

                if (serviceController != null)
                {
                    try
                    {
                        if (serviceController.Status == ServiceControllerStatus.Running)
                        {
                            m_log.Publish(MessageLevel.Info, $"Attempting to stop the {serviceName} Windows service...");

                            serviceController.Stop();

                            // Can't wait forever for service to stop, so we time-out after 20 seconds
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20.0D));

                            if (serviceController.Status == ServiceControllerStatus.Stopped)
                                m_log.Publish(MessageLevel.Info, $"Successfully stopped the {serviceName} Windows service.");
                            else
                                m_log.Publish(MessageLevel.Info, $"Failed to stop the {serviceName} Windows service after trying for 20 seconds...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SwallowException(ex, $"Failed to stop the {serviceName} Windows service: {ex.Message}");
                    }
                }

                // If the service failed to stop or it is installed as stand-alone debug application, we try to forcibly stop any remaining running instances
                Process[] instances = Process.GetProcessesByName(serviceName);

                if (instances.Length > 0)
                {
                    int total = 0;
                    m_log.Publish(MessageLevel.Info, $"Attempting to stop running instances of the {serviceName}...");

                    // Terminate all instances of service running on the local computer
                    foreach (Process process in instances)
                    {
                        process.Kill();
                        total++;
                    }

                    if (total > 0)
                        m_log.Publish(MessageLevel.Info, $"Stopped {total} {serviceName} instance{(total > 1 ? "s" : "")}.");
                }

                // Mark phasor data source validation to rename all point tags at next configuration reload
                using (AdoDataConnection connection = new AdoDataConnection(connectionString, dataProviderString))
                {
                    string filterExpression = "MethodName = 'PhasorDataSourceValidation'";
                    TableOperations<DataOperation> dataOperationTable = new TableOperations<DataOperation>(connection);
                    DataOperation record = dataOperationTable.QueryRecordWhere(filterExpression);

                    if (record == null)
                    {
                        string errorMessage = $"Failed to find DataOperation record with {filterExpression}.";
                        m_log.Publish(MessageLevel.Error, "ApplyChanges", errorMessage);
                        MessageBox.Show(this, errorMessage, "Cannot Apply Changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    record.Arguments = "renameAllPointTags=true";
                    dataOperationTable.UpdateRecordWhere(record, filterExpression);
                }

                // Update point tag name expression in config files
                XElement servicePointTagExpression = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(securitySettings => securitySettings.Elements("add"))
                    .FirstOrDefault(element => "PointTagNameExpression".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase));

                XAttribute value = servicePointTagExpression?.Attribute("value");

                if (value != null)
                {
                    value.Value = textBoxExpression.Text;
                    serviceConfig.Save(serviceConfigFile);
                }

                XElement managerPointTagExpression = managerConfig?
                    .Descendants("systemSettings")
                    .SelectMany(securitySettings => securitySettings.Elements("add"))
                    .FirstOrDefault(element => "PointTagNameExpression".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase));

                value = managerPointTagExpression?.Attribute("value");

                if (value != null)
                {
                    value.Value = textBoxExpression.Text;
                    serviceConfig.Save(managerConfigFile);
                }

                if (checkBoxSetPortNumber.Checked)
                {
                    // Update STTP port number
                    XElement sttpConfigString = serviceConfig
                        .Descendants("sttpdatapublisher")
                        .SelectMany(securitySettings => securitySettings.Elements("add"))
                        .FirstOrDefault(element => "ConfigurationString".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase));

                    value = sttpConfigString?.Attribute("value");

                    if (value != null)
                    {
                        value.Value = $"port={maskedTextBoxPortNumber.Text}";
                        serviceConfig.Save(serviceConfigFile);
                    }
                }

                // Refresh state in case service process was forcibly stopped
                serviceController.Refresh();

                // Attempt to restart Windows service...
                serviceController.Start();
            }
            catch (Exception ex)
            {
                m_log.Publish(MessageLevel.Error, "ApplyChanges", ex.Message, exception: ex);
                MessageBox.Show(this, $"Exception: {ex.Message}", "Failed to Apply Changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSelectConfigFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                textBoxConfigFile.Text = openFileDialog.FileName;
        }
    }
}
