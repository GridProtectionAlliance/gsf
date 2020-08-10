//******************************************************************************************************
//  Main.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/26/2010 - Mehulbhai P. Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using GSF.Security.Cryptography;

namespace ConfigEditor
{
    public partial class Main : Form
    {

        #region [ Members and Properties ]

        private const CipherStrength CryptoStrength = CipherStrength.Aes256;
        private const string DefaultCryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";

        string m_configurationFileName;
        bool m_isDirty;
        List<string> m_categorizedSections;

        bool IsDirty
        {
            get { return m_isDirty; }
            set { m_isDirty = value; }
        }

        string ConfigurationFileName
        {
            get
            {
                return m_configurationFileName;
            }
            set
            {
                m_configurationFileName = value;
                TextBoxConfigurationFile.Text = value;
            }
        }

        #endregion

        #region [ Form Events ]

        public Main()
        {
            InitializeComponent();
            ButtonSave.Click += ButtonSave_Click;
            ButtonOpen.Click += ButtonOpen_Click;
            ButtonLoad.Click += ButtonLoad_Click;
            PropertyGridConfiguration.PropertyValueChanged += PropertyGridConfiguration_PropertyValueChanged;
            CheckBoxAutoRestart.CheckedChanged += CheckBoxAutoRestart_CheckedChanged;
            ButtonSaveSettings.Click += ButtonSaveSettings_Click;

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConfigurationFilePath"]))
                this.ConfigurationFileName = ConfigurationManager.AppSettings["DefaultConfigurationFilePath"];
        }

        void FormConfigurationEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (this.IsDirty)
                {
                    DialogResult result = MessageBox.Show("Do you want to save configuration changes before closing?", "Pending Configuration Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                        SaveConfiguration(this.ConfigurationFileName, (BindableProperties)PropertyGridConfiguration.SelectedObject);
                    else if (result == DialogResult.Cancel)
                        e.Cancel = true;
                }
            }
            catch
            {
                // Don't want exceptions while shutting down...
            }

            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (!string.IsNullOrEmpty(this.ConfigurationFileName) && File.Exists(this.ConfigurationFileName) && string.Compare(config.AppSettings.Settings["DefaultConfigurationFilePath"].Value, this.ConfigurationFileName, true) != 0)
                {
                    config.AppSettings.Settings["DefaultConfigurationFilePath"].Value = this.ConfigurationFileName;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                }
            }
            catch
            {
                // Don't want exceptions while shutting down...
            }
        }

        void FormConfigurationEditor_Load(object sender, EventArgs e)
        {
            LoadConfigurationFile();
            LoadApplicationSettings();
        }

        void PropertyGridConfiguration_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.IsDirty = true;
        }

        void ButtonLoad_Click(object sender, EventArgs e)
        {
            LoadConfigurationFile();
        }

        void ButtonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialogConfiguration.Filter = "Configuration Files (*.config)| *.config|All Files (*.*)|*.*";
            DialogResult openFileDialogResult = OpenFileDialogConfiguration.ShowDialog();
            if (openFileDialogResult == DialogResult.OK)
                this.ConfigurationFileName = OpenFileDialogConfiguration.FileName;
        }

        void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveConfiguration(this.ConfigurationFileName, (BindableProperties)PropertyGridConfiguration.SelectedObject);
        }

        private void CheckBoxAutoRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAutoRestart.Checked)
                TextBoxDefaultService.Enabled = true;
            else
                TextBoxDefaultService.Enabled = false;
        }

        private void ButtonSaveSettings_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["DefaultConfigurationFilePath"].Value = TextBoxDefaultConfiguration.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            if (CheckBoxAutoRestart.Checked)
                config.AppSettings.Settings["DefaultServiceName"].Value = TextBoxDefaultService.Text;
            else
                config.AppSettings.Settings["DefaultServiceName"].Value = string.Empty;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion

        #region [ Configuration Editor Code ]

        void LoadConfigurationFile()
        {
            if (!string.IsNullOrEmpty(this.ConfigurationFileName) && File.Exists(this.ConfigurationFileName))
            {
                try
                {
                    m_categorizedSections = new List<string>();

                    this.PropertyGridConfiguration.SelectedObject = LoadConfigurationSettings(this.ConfigurationFileName);

                    PropertyGridConfiguration.CollapseAllGridItems();

                    LableCurrentFile.Text = "Configuration loaded from: " + this.ConfigurationFileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        BindableProperties LoadConfigurationSettings(string configurationFile)
        {
            BindableProperties properties = new BindableProperties();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configurationFile);
                XmlNode configuration = xmlDoc.SelectSingleNode("configuration/categorizedSettings");
                XmlNodeList sectionList = configuration.ChildNodes;

                for (int y = 0; y < sectionList.Count; y++)
                {
                    XmlNodeList settingsList = xmlDoc.SelectNodes("configuration/categorizedSettings/" + sectionList[y].Name + "/add");

                    if (!m_categorizedSections.Contains(sectionList[y].Name))
                        m_categorizedSections.Add(sectionList[y].Name);

                    if (settingsList.Count != 0 && settingsList != null)
                    {
                        for (int i = 0; i < settingsList.Count; i++)
                        {
                            XmlAttribute atrribKey = settingsList[i].Attributes["name"];
                            XmlAttribute attribValue = settingsList[i].Attributes["value"];
                            XmlAttribute attribDescription = settingsList[i].Attributes["description"];
                            XmlAttribute attribEncrypted = settingsList[i].Attributes["encrypted"];
                            if (atrribKey != null && attribValue != null)
                            {
                                //If there's no description for the key - assign the name to the description.								
                                if (attribDescription == null)
                                    attribDescription = atrribKey;

                                Type propType;
                                //string selectedValue = string.Empty;

                                if (attribValue.Value.ToLower() == "true" || attribValue.Value.ToLower() == "false")
                                    propType = typeof(Boolean);
                                else
                                    propType = typeof(String);

                                //If value is encrypted, then decrypt before loading it into property grid.
                                string attribValueString;
                                if (attribEncrypted != null && attribEncrypted.Value.ToLower() == "true" && !string.IsNullOrEmpty(attribValue.Value))
                                    attribValueString = attribValue.Value.Decrypt(DefaultCryptoKey, CryptoStrength);
                                else
                                    attribValueString = attribValue.Value;

                                //Now add the property													
                                properties.AddProperty(atrribKey.Value, attribValueString,
                                        attribDescription.Value, sectionList[y].Name, propType, false, false);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return properties;
        }

        void SaveConfiguration(string configurationFile, BindableProperties properties)
        {
            ServiceController controller = new ServiceController();
            bool restartService = false;

            StringBuilder returnMessage = new StringBuilder();

            try
            {
                if (CheckBoxAutoRestart.Checked && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultServiceName"]))	//then stop service, modify config file and then start service.
                {
                    controller.ServiceName = ConfigurationManager.AppSettings["DefaultServiceName"];
                    if (controller.CanStop && controller.Status != ServiceControllerStatus.Stopped && controller.Status != ServiceControllerStatus.StopPending)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    restartService = true;
                    returnMessage.AppendLine("Successfully stopped " + ConfigurationManager.AppSettings["DefaultServiceName"] + " service.");
                    returnMessage.AppendLine();
                }
            }
            catch (Exception ex)
            {
                returnMessage.AppendLine("ERROR: Failed to stop " + ConfigurationManager.AppSettings["DefaultServiceName"] + " service." + Environment.NewLine + ex.Message + Environment.NewLine + " Failed to save configuration changes.");
                returnMessage.AppendLine();
                return;
            }

            try
            {
                //Reload the configuration file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(configurationFile);
                //Save a backup version
                xmlDoc.Save(configurationFile + ".bak");
                //Populate our property collection. 
                PropertyDescriptorCollection props = properties.GetProperties();

                foreach (string section in m_categorizedSections)
                {
                    RepopulateXmlSection(section, xmlDoc, props);
                }

                xmlDoc.Save(configurationFile);

                returnMessage.AppendLine("Successfully saved configuration changes to " + Environment.NewLine + this.ConfigurationFileName);
                returnMessage.AppendLine();

                this.IsDirty = false;
            }
            catch (Exception ex)
            {
                returnMessage.AppendLine("ERROR: Failed to save configuration changes: " + ex.Message);
                returnMessage.AppendLine();
            }

            try
            {
                if (restartService && controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    returnMessage.AppendLine("Successfully started " + ConfigurationManager.AppSettings["DefaultServiceName"] + " service.");
                    returnMessage.AppendLine();
                }
            }
            catch (Exception ex)
            {
                returnMessage.AppendLine("ERROR: Failed to start " + ConfigurationManager.AppSettings["DefaultServiceName"] + " service." + Environment.NewLine + ex.Message);
                returnMessage.AppendLine();
            }

            MessageBox.Show(returnMessage.ToString());
        }

        void RepopulateXmlSection(string sectionName, XmlDocument xmlDoc, PropertyDescriptorCollection props)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes("configuration/categorizedSettings/" + sectionName + "/add");
            for (int i = 0; i < nodes.Count; i++)
            {
                DynamicProperty property = null;

                //Find the property in the property collection with the same name as the current node in the Xml document
                //and same category as sectionName
                foreach (PropertyDescriptor pd in props)
                {
                    if (pd.Category == sectionName && pd.Name == nodes[i].Attributes["name"].Value)
                    {
                        property = (DynamicProperty)pd;
                        break;
                    }
                }

                ////Find the property in the property collection with the same name as the current node in the Xml document
                //DynamicProperty property = (DynamicProperty)props[nodes[i].Attributes["name"].Value];

                if (property != null)
                {
                    //Set the node value to the property value which will have been set in the Property grid.							
                    nodes[i].Attributes["value"].Value = property.GetValue(null).ToString();

                    //If encrypted=true, then encrypt the value before saving into configuration file.
                    if (nodes[i].Attributes["encrypted"] != null && nodes[i].Attributes["encrypted"].Value.ToLower() == "true" && !string.IsNullOrEmpty(property.GetValue(null).ToString()))
                        nodes[i].Attributes["value"].Value = property.GetValue(null).ToString().Encrypt(DefaultCryptoKey, CryptoStrength);
                    else
                        nodes[i].Attributes["value"].Value = property.GetValue(null).ToString();

                    //Check to see if we have a value for our extended custom xml attribute - the description attribute.
                    //The default description is the property name when no descripyion attribute is present.
                    //If they're not the same - then a value was passed when the property was created.
                    if (property.Description != property.Name)
                    {
                        //double check here that there is in fact a description attribute
                        if (nodes[i].Attributes["description"] != null)
                        {
                            nodes[i].Attributes["description"].Value = property.Description;
                        }
                    }
                }



            }
        }

        #endregion

        #region [ Application Settings Code ]

        void LoadApplicationSettings()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConfigurationFilePath"]))
                TextBoxDefaultConfiguration.Text = ConfigurationManager.AppSettings["DefaultConfigurationFilePath"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultServiceName"]))
            {
                CheckBoxAutoRestart.Checked = true;
                TextBoxDefaultService.Text = ConfigurationManager.AppSettings["DefaultServiceName"];
            }
        }

        #endregion
    }
}
