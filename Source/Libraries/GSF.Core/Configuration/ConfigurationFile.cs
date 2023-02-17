//******************************************************************************************************
//  ConfigurationFile.cs - Gbtc
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
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/29/2008 - Pinal C. Patel
//       Added new CurrentWeb and CurrentWin static properties to get the configuration file for a 
//       specific type of application (i.e. Web or Windows).
//       Modified GetConfiguration() to remove dependency on HttpContext.Current to avoid exception in
//       async web application operations where HttpContext.Current is null.
//  04/20/2010 - Pinal C. Patel
//       Added static Open() method as the only way of retrieving a ConfigurationFile instance.
//       Added internal UserConfigurationFile class for accessing and updating user scope settings.
//  07/01/2010 - Pinal C. Patel
//       Removed FileName, AppSettings and ConnectionStrings proxy properties and replaced them with
//       new Configuration property to allow for full access to the application configuration.
//  11/26/2010 - Pinal C. Patel
//       Updated usage example to include user-scope setting sample.
//  12/05/2010 - Pinal C. Patel
//       Added Culture property that can be used for specifying a culture to use for value conversion.
//  01/04/2011 - J. Ritchie Carroll
//       Modified culture to default to InvariantCulture for English style parsing defaults.
//  04/07/2011 - J. Ritchie Carroll
//       Added a Reload() method to reload the configuration file settings and a
//       RestoreDefaultUserSettings() method to restore the default user settings.
//  12/12/2011 - J. Ritchie Carroll
//       Added synchronization to save methods such that saves from multiple threads will not
//       cause issues when lots of new settings are being added.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml;
using GSF.IO;
using GSF.Threading;
using GSF.Xml;

namespace GSF.Configuration
{
    /// <summary>
    /// Represents a configuration file of a Windows or Web application.
    /// </summary>
    /// <example>
    /// This example shows how to save and read settings from the config file:
    /// <code>
    /// using System;
    /// using System.Configuration;
    /// using GSF;
    /// using GSF.Configuration;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Get the application config file.
    ///         ConfigurationFile config = ConfigurationFile.Current;
    /// 
    ///         // Get the sections of config file.
    ///         CategorizedSettingsElementCollection startup = config.Settings["Startup"];
    ///         CategorizedSettingsElementCollection passwords = config.Settings["Passwords"];
    ///         CategorizedSettingsElementCollection monitoring = config.Settings["Monitoring"];
    ///         KeyValueConfigurationCollection appSettings = config.Configuration.AppSettings.Settings;
    ///         ConnectionStringSettingsCollection connStrings = config.Configuration.ConnectionStrings.ConnectionStrings;
    /// 
    ///         // Add settings to the config file under the "appSettings" section.
    ///         appSettings.Add("SaveSettingOnExit", true.ToString());
    ///         // Add settings to the config file under the "connectionStrings" section.
    ///         connStrings.Add(new ConnectionStringSettings("DevSql", "Server=SqlServer;Database=Sandbox;Trusted_Connection=True"));
    ///         // Add settings to the config (if they don't exist) under a custom "monitoring" section.
    ///         monitoring.Add("RefreshInterval", 5, "Interval in seconds at which the Monitor screen is to be refreshed.");
    ///         monitoring.Add("MessagesSnapshot", 30000, "Maximum messages length to be displayed on the Monitor screen.");
    ///         // Add password to the config file encrypted (if it doesn't exist) under a custom "passwords" section.
    ///         passwords.Add("Admin", "Adm1nP4ss", "Password used for performing administrative tasks.", true);
    ///         // Add user-scope setting to the config (if it doesn't exist) under a custom "startup" section.
    ///         startup.Add("Theme", "Default", "Application theme to use for the session.", false, SettingScope.User);
    ///         config.Save();  // Save settings to the config file.
    /// 
    ///         // Read saved settings from the config file.
    ///         bool saveSettingsOnExit = appSettings["SaveSettingOnExit"].Value.ParseBoolean();
    ///         string devConnectionString = connStrings["DevSql"].ConnectionString;
    ///         string appTheme = startup["Theme"].Value;
    ///         string adminPassword = passwords["Admin"].Value;
    ///         int refreshInterval = monitoring["RefreshInterval"].ValueAsInt32();
    ///         int messagesSnapshot = monitoring["MessagesSnapshot"].ValueAsInt32();
    /// 
    ///         // Print the retrieved settings to the console.
    ///         Console.WriteLine("SaveSettingOnExit = {0}", saveSettingsOnExit);
    ///         Console.WriteLine("DevSql = {0}", devConnectionString);
    ///         Console.WriteLine("Theme = {0}", appTheme);
    ///         Console.WriteLine("Admin = {0}", adminPassword);
    ///         Console.WriteLine("RefreshInterval = {0}", refreshInterval);
    ///         Console.WriteLine("MessagesSnapshot = {0}", messagesSnapshot);
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
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <appSettings>
    ///     <add key="SaveSettingOnExit" value="True" />
    ///   </appSettings>
    ///   <categorizedSettings>
    ///     <startup>
    ///       <add name="Theme" value="Default" description="Application theme to use for the session."
    ///         encrypted="false" scope="User" />
    ///     </startup>
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
    /// <seealso cref="CategorizedSettingsSection"/>
    /// <seealso cref="CategorizedSettingsElement"/>
    /// <seealso cref="CategorizedSettingsElementCollection"/>
    // Since each instance of this class is statically cached for the process lifetime, IDisposable is not implemented so that
    // the instance will not be inadvertently disposed
    public class ConfigurationFile
    {
        #region [ Members ]

        // Nested Types
        internal class UserConfigurationFile
        {
            private readonly XmlDocument m_settings;
            private bool m_settingsModified;

            private const string RootNode = "settings";
            private const string Filename = "Settings.xml";

            public UserConfigurationFile()
            {
                m_settings = new XmlDocument();
                FileName = Path.Combine(FilePath.GetApplicationDataFolder(), Filename);

                if (File.Exists(FileName))
                {
                    try
                    {
                        // Load existing settings.
                        m_settings.Load(FileName);
                    }
                    catch (Exception)
                    {
                        // Create new settings file.
                        m_settings.AppendChild(m_settings.CreateNode(XmlNodeType.XmlDeclaration, null!, null));
                        m_settings.AppendChild(m_settings.CreateElement(RootNode));
                    }
                }
                else
                {
                    // Create new settings file.
                    m_settings.AppendChild(m_settings.CreateNode(XmlNodeType.XmlDeclaration, null!, null));
                    m_settings.AppendChild(m_settings.CreateElement(RootNode));
                }
            }

            public string FileName { get; }

            public void Save(bool forceSave)
            {
                if (!forceSave && !m_settingsModified)
                    return;

                // Create directory if missing.
                string folder = FilePath.GetDirectoryName(FileName);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Save settings to the file.
                m_settings.Save(FileName);
            }

            public void Write(string category, string name, string value)
            {
                XmlNode node = GetSetting(category, name);

                if (node is not null)
                {
                    // Setting exists so update it.
                    node.SetAttributeValue("value", value);
                }
                else
                {
                    // Setting doesn't exist so add it.
                    XmlNode setting = m_settings.CreateElement("add");
                    setting.SetAttributeValue("name", name);
                    setting.SetAttributeValue("value", value);
                    m_settings.GetXmlNode(category).AppendChild(setting);
                }

                m_settingsModified = true;
            }

            public string Read(string category, string name, string defaultValue)
            {
                XmlNode node = GetSetting(category, name);

                // If setting exists, return its value
                if (node?.Attributes != null)
                    return node.Attributes["value"].Value;

                // Setting doesn't exist so return the default
                return defaultValue;
            }

            public XmlNode GetSetting(string category, string name) => 
                m_settings.SelectSingleNode($"{RootNode}/{category}/add[@name='{name}']");
        }

        // Constants
        private const string CustomSectionName = "categorizedSettings";
        private const string CustomSectionType = "GSF.Configuration.CategorizedSettingsSection, GSF.Core";

        // Fields
        private string m_cryptoKey;
        private CultureInfo m_culture;
        private volatile ConfigurationSaveMode m_saveMode;
        private readonly LongSynchronizedOperation m_saveOperation;
        internal bool m_forceSave;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
        /// </summary>        
        internal ConfigurationFile(string configFilePath)
        {
            m_culture = CultureInfo.InvariantCulture;

            // Do not run the save operation on a background thread since the application may be
            // performing the save as a final step during shutdown and the save operation must
            // complete, incomplete saves can cause a zero length config file to be created.
            m_saveOperation = new LongSynchronizedOperation(ExecuteConfigurationSave)
            {
                IsBackground = false
            };

            Configuration = GetConfiguration(configFilePath);

            if (Configuration.HasFile && File.Exists(Configuration.FilePath))
                ValidateConfigurationFile(Configuration.FilePath);
            else
                CreateConfigurationFile(Configuration.FilePath);

            Configuration = GetConfiguration(configFilePath);
            UserSettings = new UserConfigurationFile();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to use for the conversion of setting values to and from <see cref="string"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get => m_culture;
            set => m_culture = value ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Get the underlying <see cref="System.Configuration.Configuration"/> that can be accessed using this <see cref="ConfigurationFile"/> object.
        /// </summary>
        public System.Configuration.Configuration Configuration { get; private set; }

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsSection"/> object representing settings under the "categorizedSettings" section of the config file.
        /// </summary>
        public CategorizedSettingsSection Settings
        {
            get
            {
                CategorizedSettingsSection settings = (CategorizedSettingsSection)Configuration.GetSection(CustomSectionName);
                settings.File = this;
                settings.SetCryptoKey(m_cryptoKey);
                return settings;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserConfigurationFile"/> where user specific settings are saved.
        /// </summary>
        internal UserConfigurationFile UserSettings { get; private set; }

        private string BackupConfigFilePath
        {
            get
            {
                string filePath = Configuration.FilePath;
                return Path.Combine(FilePath.GetDirectoryName(filePath), FilePath.GetFileNameWithoutExtension(filePath) + ".config.backup");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the configuration file that it represents.
        /// </summary>
        public void Save() => 
            Save(m_forceSave ? ConfigurationSaveMode.Full : ConfigurationSaveMode.Modified);

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the configuration file that it represents.
        /// </summary>
        /// <param name="saveMode">One of the <see cref="ConfigurationSaveMode"/> values.</param>
        public void Save(ConfigurationSaveMode saveMode)
        {
            m_saveMode = saveMode;

        #if MONO
            // As of Mono v3.8.0, threads are killed even when marked as non-background (at least on Linux)
            m_saveOperation.Run();
        #else
            m_saveOperation.RunOnceAsync();
        #endif
        }

        private void ExecuteConfigurationSave()
        {
            ConfigurationSaveMode saveMode = m_saveMode;
            UserSettings.Save(saveMode == ConfigurationSaveMode.Full);
            Configuration.Save(saveMode, m_forceSave);
            m_forceSave = false;

            try
            {
                // Attempt to create a backup configuration file
            #if MONO
                m_configuration.SaveAs(BackupConfigFilePath, ConfigurationSaveMode.Full);
            #else
                File.Copy(Configuration.FilePath, BackupConfigFilePath, true);
            #endif
            }
            catch
            {
                // May not have needed rights to save backup configuration file
            }
        }

        /// <summary>
        /// Writes the configuration settings contained within this <see cref="ConfigurationFile"/> object to the specified configuration file.
        /// </summary>
        /// <param name="fileName">The path and file name to save the configuration file to.</param>
        public void SaveAs(string fileName) => 
            Configuration.SaveAs(fileName);
        
        /// <summary>
        /// Reloads the current configuration settings from the configuration file that the <see cref="ConfigurationFile"/> represents.
        /// </summary>
        public void Reload()
        {
            Configuration = GetConfiguration(Configuration.FilePath);
            UserSettings = new UserConfigurationFile();
        }

        /// <summary>
        /// Restores all the default settings for <see cref="SettingScope.User"/> scoped settings.
        /// </summary>
        public void RestoreDefaultUserSettings()
        {
            if (UserSettings is null)
                return;

            string fileName = UserSettings.FileName;

            if (File.Exists(fileName))
                File.Delete(fileName);

            UserSettings = new UserConfigurationFile();
        }

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting values of <see cref="Settings"/>.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey) => 
            m_cryptoKey = cryptoKey;

        private System.Configuration.Configuration GetConfiguration(string configFilePath)
        {
            ApplicationType appType = Common.GetApplicationType();
            System.Configuration.Configuration configuration;

            if (configFilePath is not null)
            {
                if (string.IsNullOrEmpty(configFilePath) || string.Compare(FilePath.GetExtension(configFilePath), ".config", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // PCP - 12/12/2006: Using the TrimEnd function to get the correct value that needs to be passed
                    // to the method call for getting the Configuration object. The previous method (String.TrimEnd())
                    // yielded incorrect output resulting in the Configuration object not being initialized correctly.
                    switch (appType)
                    {
                        case ApplicationType.Web:
                            if (string.IsNullOrEmpty(configFilePath))
                                configFilePath = HostingEnvironment.ApplicationVirtualPath;
                            configuration = WebConfigurationManager.OpenWebConfiguration(TrimEnd(configFilePath, "web.config"));
                            break;
                        default:
                            configuration = ConfigurationManager.OpenExeConfiguration(TrimEnd(configFilePath, ".config").EnsureEnd(".exe"));
                            break;
                    }
                }
                else
                {
                    throw new ArgumentException("Path of configuration file must be either empty or end in \'.config\'");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(configFilePath), "Path of configuration file path cannot be null");
            }

            return configuration;
        }

        private void CreateConfigurationFile(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                XmlTextWriter configFileWriter = new(configFilePath, Encoding.UTF8);
                configFileWriter.Indentation = 2;
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
                throw new ArgumentNullException(nameof(configFilePath), "Path of configuration file path cannot be null");
            }
        }

        // Validate configuration file
        private void ValidateConfigurationFile(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                XmlDocument configFile = new();
                bool configFileModified = false;
                configFile.Load(configFilePath);

                // Make sure that the config file has the necessary section information under <customSections />
                // that is required by the .Net configuration API to process our custom <categorizedSettings />
                // section. The configuration API will raise an exception if it doesn't find this section.
                if (configFile.DocumentElement is not null)
                {
                    XmlNodeList configSections = configFile.DocumentElement.SelectNodes("configSections");

                    if (configSections is not null && configSections.Count == 0)
                    {
                        // Add a <configSections> node, if one is not present.
                        configFile.DocumentElement.InsertBefore(configFile.CreateElement("configSections"), configFile.DocumentElement.FirstChild);
                        configFileModified = true;
                    }

                    XmlNode configSectionsNode = configFile.DocumentElement.SelectSingleNode("configSections");

                    XmlNodeList section = configSectionsNode?.SelectNodes("section[@name = \'" + CustomSectionName + "\']");

                    if (section != null && section.Count == 0)
                    {
                        // Adds the <section> node that specifies the DLL that handles the <categorizedSettings> node in
                        // the config file, if one is not present.
                        XmlNode node = configFile.CreateElement("section");
                        node.SetAttributeValue("name", CustomSectionName);
                        node.SetAttributeValue("type", CustomSectionType);
                        configSectionsNode.AppendChild(node);
                        configFileModified = true;
                    }
                }

                // 11/14/2006 - PCP: We'll save the config file only it was modified. This will prevent ASP.Net
                // web sites from restarting every time a configuration element is accessed.
                if (configFileModified)
                    configFile.Save(configFilePath);
            }
            else
            {
                throw new ArgumentNullException(nameof(configFilePath), "Path of configuration file path cannot be null");
            }
        }

        // Trim suffix from end of string
        private static string TrimEnd(string stringToTrim, string textToTrim)
        {
            int trimEndIndex = stringToTrim.LastIndexOf(textToTrim, StringComparison.CurrentCultureIgnoreCase);

            if (trimEndIndex == -1)
                trimEndIndex = stringToTrim.Length;

            return stringToTrim.Substring(0, trimEndIndex);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, ConfigurationFile> s_configFiles;

        // Static Constructor
        static ConfigurationFile()
        {
            s_configFiles = new ConcurrentDictionary<string, ConfigurationFile>(StringComparer.CurrentCultureIgnoreCase);
        }

        // Static Properties

        /// <summary>
        /// Gets the <see cref="ConfigurationFile"/> object that represents the config file of the currently executing Windows or Web application.
        /// </summary>
        public static ConfigurationFile Current => Open(string.Empty);

        // Static Methods

        /// <summary>
        /// Opens application config file at the specified <paramref name="configFilePath"/>.
        /// </summary>
        /// <param name="configFilePath">Path of the config file that belongs to a Windows or Web application.</param>
        /// <returns>An <see cref="ConfigurationFile"/> object.</returns>
        public static ConfigurationFile Open(string configFilePath)
        {
            // Retrieve config file from cache if present or else add it for subsequent uses.
            return s_configFiles.GetOrAdd(configFilePath, path => new ConfigurationFile(path));
        }

        #endregion
    }
}
