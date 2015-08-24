//*******************************************************************************************************
//  ConfigurationFile.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
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
//  09/22/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//
//*******************************************************************************************************

using System;
using System.Configuration;
using System.Text;
using System.Web.Configuration;
using System.Xml;
using TVA.IO;
using TVA.Xml;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents a configuration file of a Windows or Web application.
    /// </summary>
    /// <seealso cref="CategorizedSettingsSection"/>
    /// <seealso cref="CategorizedSettingsElement"/>
    /// <seealso cref="CategorizedSettingsElementCollection"/>
    /// <example>
    /// This example shows how to save and read settings from the config file:
    /// <code>
    /// using System;
    /// using System.Configuration;
    /// using TVA;
    /// using TVA.Configuration;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Get the application config file.
    ///         ConfigurationFile config = ConfigurationFile.Current;
    ///         
    ///         // Get the sections of config file.
    ///         CategorizedSettingsElementCollection passwords = config.Settings["Passwords"];
    ///         CategorizedSettingsElementCollection monitoring = config.Settings["Monitoring"];
    ///         KeyValueConfigurationCollection appSettings = config.AppSettings.Settings;
    ///         ConnectionStringSettingsCollection connStrings = config.ConnectionStrings.ConnectionStrings;
    ///        
    ///         // Add settings to the config file under the "appSettings" section.
    ///         appSettings.Add("SaveSettingOnExit", true.ToString());
    ///         // Add settings to the config file under the "connectionStrings" section.
    ///         connStrings.Add(new ConnectionStringSettings("DevSql", "Server=SqlServer;Database=Sandbox;Trusted_Connection=True"));
    ///         // Add settings to the config (if they don't exist) under a custom "monitoring" section.
    ///         monitoring.Add("RefreshInterval", 5, "Interval in seconds at which the Monitor screen is to be refreshed.");
    ///         monitoring.Add("MessagesSnapshot", 30000, "Maximum messages length to be displayed on the Monitor screen.");
    ///         // Add passwords to the config file encrypted (if they don't exist) under a custom "passwords" section.
    ///         passwords.Add("Admin", "Adm1nP4ss", "Password used for performing administrative tasks.", true);
    ///         config.Save();  // Save settings to the config file.
    ///        
    ///         // Read saved settings from the config file.
    ///         bool saveSettingsOnExit = appSettings["SaveSettingOnExit"].Value.ParseBoolean();
    ///         string devConnectionString = connStrings["DevSql"].ConnectionString;
    ///         string adminPassword = passwords["Admin"].Value;
    ///         int refreshInterval = monitoring["RefreshInterval"].ValueAsInt32();
    ///         int messagesSnapshot = monitoring["MessagesSnapshot"].ValueAsInt32();
    ///        
    ///         // Print the retrieved settings to the console.
    ///         Console.WriteLine(string.Format("SaveSettingOnExit = {0}", saveSettingsOnExit));
    ///         Console.WriteLine(string.Format("DevSql = {0}", devConnectionString));
    ///         Console.WriteLine(string.Format("Admin = {0}", adminPassword));
    ///         Console.WriteLine(string.Format("RefreshInterval = {0}", refreshInterval));
    ///         Console.WriteLine(string.Format("MessagesSnapshot = {0}", messagesSnapshot));
    ///        
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// This example shows the content of the config file from the sample code above:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="TVA.Configuration.CategorizedSettingsSection, TVA.Core" />
    ///   </configSections>
    ///   <appSettings>
    ///     <add key="SaveSettingOnExit" value="True" />
    ///   </appSettings>
    ///   <categorizedSettings>
    ///     <passwords>
    ///       <add name="Admin" value="C+0j6fE/N0Q9b5xaeDKgvRmSeY9zJkO1EQCr7cHoG3x24tztlbBB54PfWsuMGXc/"
    ///         description="Password used for performing administrative tasks."
    ///         encrypted="true" />
    ///     </passwords>
    ///     <monitoring>
    ///       <add name="RefreshInterval" value="5" description="Interval in seconds at which the Monitor screen is to be refreshed."
    ///         encrypted="false" />
    ///       <add name="MessagesSnapshot" value="30000" description="Maximum messages length to be displayed on the Monitor screen."
    ///         encrypted="false" />
    ///     </monitoring>
    ///   </categorizedSettings>
    ///   <connectionStrings>
    ///     <add name="DevSql" connectionString="Server=SqlServer;Database=Sandbox;Trusted_Connection=True" />
    ///   </connectionStrings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </example>
    public class ConfigurationFile
    {
        #region [ Members ]

        // Constants
        private const string CustomSectionName = "categorizedSettings";
        private const string CustomSectionType = "TVA.Configuration.CategorizedSettingsSection, TVA.Core";

        // Fields
        private string m_cryptoKey;
        private System.Configuration.Configuration m_configuration;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class representing the config file of 
        /// current Windows or Web application.
        /// </summary>
        public ConfigurationFile()
            : this("")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class representing the config file of 
        /// another Windows or Web application.
        /// </summary>
        /// <param name="configFilePath">Path of the config file that belongs to another Windows or Web application.</param>
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

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsSection"/> object representing settings under the 
        /// "categorizedSettings" section of the config file.
        /// </summary>
        /// <returns>A <see cref="CategorizedSettingsSection"/> object.</returns>
        public CategorizedSettingsSection Settings
        {
            get
            {
                CategorizedSettingsSection settings = (CategorizedSettingsSection)m_configuration.GetSection(CustomSectionName);
                settings.SetCryptoKey(m_cryptoKey);
                return settings;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Configuration.AppSettingsSection"/> object representing settings under the 
        /// "appSettings" section of the config file.
        /// </summary>
        /// <returns>A <see cref="System.Configuration.AppSettingsSection"/> object.</returns>
        public AppSettingsSection AppSettings
        {
            get
            {
                return m_configuration.AppSettings;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Configuration.ConnectionStringsSection"/> representing settings under the 
        /// "connectionStrings" section of the config file.
        /// </summary>
        /// <returns>A <see cref="System.Configuration.ConnectionStringsSection"/> object.</returns>
        public ConnectionStringsSection ConnectionStrings
        {
            get
            {
                return m_configuration.ConnectionStrings;
            }
        }

        /// <summary>
        /// Gets the name and path of the config file represented by this <see cref="ConfigurationFile"/> object.
        /// </summary>
        /// <returns>Name and path of the config file.</returns>
        public string FileName
        {
            get
            {
                return m_configuration.FilePath;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the configuration file that it represents.
        /// </summary>
        public void Save()
        {
            Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the configuration file that it represents.
        /// </summary>
        /// <param name="saveMode">One of the <see cref="ConfigurationSaveMode"/> values.</param>
        public void Save(ConfigurationSaveMode saveMode)
        {
            m_configuration.Save(saveMode);
        }

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the specified configuration file.
        /// </summary>
        /// <param name="fileName">The path and file name to save the configuration file to.</param>
        public void SaveAs(string fileName)
        {
            m_configuration.SaveAs(fileName);
        }

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting values of <see cref="Settings"/>.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey)
        {
            m_cryptoKey = cryptoKey;
        }

        private System.Configuration.Configuration GetConfiguration(string configFilePath)
        {
            System.Configuration.Configuration configuration = null;

            if (configFilePath != null)
            {
                if (string.IsNullOrEmpty(configFilePath) || FilePath.GetExtension(configFilePath) == ".config")
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
                            if (string.IsNullOrEmpty(configFilePath))
                                configFilePath = System.Web.HttpContext.Current.Request.ApplicationPath;
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
                XmlTextWriter configFileWriter = new XmlTextWriter(configFilePath, Encoding.UTF8);
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

        #endregion

        #region [ Static ]

        private static ConfigurationFile m_current;

        /// <summary>
        /// Gets the <see cref="ConfigurationFile"/> object that represents the config file of the currently 
        /// executing Windows or Web application.
        /// </summary>
        public static ConfigurationFile Current
        {
            get 
            {
                if (m_current == null)
                {
                    m_current = new ConfigurationFile();
                }
                return m_current;
            }
        }

        // Trim suffix from end of string
        private static string TrimEnd(string stringToTrim, string textToTrim)
        {
            int trimEndIndex = stringToTrim.LastIndexOf(textToTrim);

            if (trimEndIndex == -1)
                trimEndIndex = stringToTrim.Length;

            return stringToTrim.Substring(0, trimEndIndex);
        }

        // Validate configuration file
        private static void ValidateConfigurationFile(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                XmlDocument configFile = new XmlDocument();
                bool configFileModified = false;
                configFile.Load(configFilePath);

                // Make sure that the config file has the necessary section information under <customSections />
                // that is required by the .Net configuration API to process our custom <categorizedSettings />
                // section. The configuration API will raise an exception if it doesn't find this section.
                if (configFile.DocumentElement.SelectNodes("configSections").Count == 0)
                {
                    // Adds a <configSections> node, if one is not present.
                    configFile.DocumentElement.InsertBefore(configFile.CreateElement("configSections"), configFile.DocumentElement.FirstChild);

                    configFileModified = true;
                }
                XmlNode configSectionsNode = configFile.DocumentElement.SelectSingleNode("configSections");
                if (configSectionsNode.SelectNodes("section[@name = \'" + CustomSectionName + "\']").Count == 0)
                {
                    // Adds the <section> node that specifies the DLL that handles the <categorizedSettings> node in
                    // the config file, if one is not present.
                    XmlNode node = configFile.CreateElement("section");
                    node.SetAttributeValue("name", CustomSectionName);
                    node.SetAttributeValue("type", CustomSectionType);
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

        #endregion
    }
}
