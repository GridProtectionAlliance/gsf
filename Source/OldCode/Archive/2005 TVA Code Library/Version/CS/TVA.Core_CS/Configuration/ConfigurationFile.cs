using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Configuration;
using System.Web.Configuration;
//using TVA.Xml.Common;
using TVA.IO.FilePath;

//*******************************************************************************************************
//  TVA.Configuration.ConfigurationFile.vb - Application Configuration File
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/12/2006 - Pinal C. Patel
//       Generated original version of source code.
//  11/14/2006 - Pinal C. Patel
//       Modified the ValidateConfigurationFile to save the config file only it was modified.
//  12/12/2006 - Pinal C. Patel
//       Wrote the TrimEnd function that is being used for initialized the Configuration object.
//  08/20/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
    namespace Configuration
    {

        /// <summary>
        /// Represents a configuration file of a Windows or Web application.
        /// </summary>
        public class ConfigurationFile
        {


            private System.Configuration.Configuration m_configuration;

            private const string CustomSectionName = "categorizedSettings";
            private const string CustomSectionType = "TVA.Configuration.CategorizedSettingsSection, TVA.Core";

            /// <summary>
            /// Initializes a default instance of TVA.Configuration.ConfigurationFile.
            /// </summary>
            public ConfigurationFile()
                : this("")
            {
            }

            /// <summary>
            /// Initializes an instance of TVA.Configuration.ConfigurationFile for the specified configuration file
            /// that belongs to a Windows or Web application.
            /// </summary>
            /// <param name="configFilePath">Path of the configuration file that belongs to a Windows or Web application.</param>
            public ConfigurationFile(string configFilePath)
            {
                m_configuration = GetConfiguration(configFilePath);
                if (m_configuration.HasFile)
                {
                    ValidateConfigurationFile(m_configuration.FilePath);
                }
                else
                {
                    CreateConfigurationFile(m_configuration.FilePath);
                }
                m_configuration = GetConfiguration(configFilePath);
            }

            /// <summary>
            /// Gets the TVA.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.
            /// </summary>
            /// <returns>The TVA.Configuration.CategorizedSettingsSection representing the "categorizedSettings" section of the configuration file.</returns>
            public CategorizedSettingsSection CategorizedSettings
            {
                get
                {
                    return ((CategorizedSettingsSection)(m_configuration.GetSection(CustomSectionName)));
                }
            }

            /// <summary>
            /// Gets the System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.
            /// </summary>
            /// <returns>The System.Configuration.AppSettingsSection representing the "appSettings" section of the configuration file.</returns>
            public AppSettingsSection AppSettings
            {
                get
                {
                    return m_configuration.AppSettings;
                }
            }

            /// <summary>
            /// Gets the System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.
            /// </summary>
            /// <returns>The System.Configuration.ConnectionStringsSection representing the "connectionStrings" section of the configuration file.</returns>
            public ConnectionStringsSection ConnectionStrings
            {
                get
                {
                    return m_configuration.ConnectionStrings;
                }
            }

            /// <summary>
            /// Gets the physical path to the configuration file represented by this TVA.Configuration.Configuration object.
            /// </summary>
            /// <returns>The physical path to the configuration file represented by this TVA.Configuration.ConfigurationFile object.</returns>
            public string FilePath
            {
                get
                {
                    return m_configuration.FilePath;
                }
            }

            /// <summary>
            /// Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object
            /// to the configuration file that it represents.
            /// </summary>
            public void Save()
            {

                Save(ConfigurationSaveMode.Modified);

            }

            /// <summary>
            /// Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object
            /// to the configuration file that it represents.
            /// </summary>
            /// <param name="saveMode">A System.Configuration.ConfigurationSaveMode value that determines which property values to save.</param>
            public void Save(ConfigurationSaveMode saveMode)
            {

                m_configuration.Save(saveMode);

            }

            /// <summary>
            /// Writes the configuration settings contained within this TVA.Configuration.ConfigurationFile object
            /// to the specified configuration file.
            /// </summary>
            /// <param name="fileName">The path and file name to save the configuration file to.</param>
            public void SaveAs(string fileName)
            {

                m_configuration.SaveAs(fileName);

            }

            private System.Configuration.Configuration GetConfiguration(string configFilePath)
            {

                System.Configuration.Configuration configuration = null;

                if (configFilePath != null)
                {
                    if (configFilePath == "" || JustFileExtension(configFilePath) == ".config")
                    {
                        // PCP - 12/12/2006: Using the TrimEnd function to get the correct value that needs to be passed
                        // to the method call for getting the Configuration object. The previous method (String.TrimEnd())
                        // yielded incorrect output resulting in the Configuration object not being initialized correctly.
                        switch (TVA.Common.GetApplicationType())
                        {
                            case ApplicationType.WindowsCui:
                            case ApplicationType.WindowsGui:
                                configuration = ConfigurationManager.OpenExeConfiguration(TrimEnd(configFilePath, ".config"));
                                break;
                            case ApplicationType.Web:
                                if (configFilePath == "")
                                {
                                    configFilePath = System.Web.HttpContext.Current.Request.ApplicationPath;
                                }
                                configuration = WebConfigurationManager.OpenWebConfiguration(TrimEnd(configFilePath, "web.config"));
                                break;
                        }
                    }
                    else
                    {
                        throw (new ArgumentException("Path of configuration file must be either empty or end in \'.config\'"));
                    }
                }
                else
                {
                    throw (new ArgumentNullException("configFilePath", "Path of configuration file path cannot be null"));
                }

                return configuration;

            }

            private void CreateConfigurationFile(string configFilePath)
            {

                if (!string.IsNullOrEmpty(configFilePath))
                {
                    XmlTextWriter configFileWriter = new XmlTextWriter(configFilePath, System.Text.Encoding.UTF8);
                    configFileWriter.Indentation = 4;
                    configFileWriter.Formatting = Formatting.Indented;
                    // Populates the very basic information required in a config file.
                    configFileWriter.WriteStartDocument();
                    configFileWriter.WriteStartElement("configuration");
                    configFileWriter.WriteEndElement();
                    configFileWriter.WriteEndDocument();
                    // Closes the config file.
                    configFileWriter.Close();

                    ValidateConfigurationFile(configFilePath);
                }
                else
                {
                    throw (new ArgumentNullException("configFilePath", "Path of configuration file path cannot be null"));
                }

            }

            private void ValidateConfigurationFile(string configFilePath)
            {

                if (!string.IsNullOrEmpty(configFilePath))
                {
                    XmlDocument configFile = new XmlDocument();
                    bool configFileModified = false;
                    configFile.Load(configFilePath);

                    // Make sure that the config file has the necessary section information under <customSections />
                    // that is required by the .Net configuration API to process our custom <categorizedSettings />
                    // section. The configuration API will raise an exception if it doesn't find this section.
                    if (configFile.DocumentElement.SelectNodes("configSections").Count() == 0)
                    {
                        // Adds a <configSections> node, if one is not present.
                        configFile.DocumentElement.InsertBefore(configFile.CreateElement("configSections"), configFile.DocumentElement.FirstChild);

                        configFileModified = true;
                    }
                    XmlNode configSectionsNode = configFile.DocumentElement.SelectSingleNode("configSections");
                    if (configSectionsNode.SelectNodes("section[@name = \'" + CustomSectionName + "\']").Count() == 0)
                    {
                        // Adds the <section> node that specifies the DLL that handles the <categorizedSettings> node in
                        // the config file, if one is not present.
                        XmlNode node = configFile.CreateElement("section");
                        Attribute(node, "name") = CustomSectionName;
                        Attribute(node, "type") = CustomSectionType;
                        configSectionsNode.AppendChild(node);

                        configFileModified = true;
                    }

                    // 11/14/2006 - PCP: We'll save the config file only it was modified. This will prevent ASP.Net
                    // web sites from restarting every time a configuration element is accessed.
                    if (configFileModified)
                    {
                        configFile.Save(configFilePath);
                    }
                }
                else
                {
                    throw (new ArgumentNullException("configFilePath", "Path of configuration file path cannot be null"));
                }

            }

            private string TrimEnd(string stringToTrim, string textToTrim)
            {

                int trimEndIndex = stringToTrim.LastIndexOf(textToTrim);
                if (trimEndIndex == -1)
                {
                    trimEndIndex = stringToTrim.Length;
                }

                return stringToTrim.Substring(0, trimEndIndex);

            }

        }

    }
}
