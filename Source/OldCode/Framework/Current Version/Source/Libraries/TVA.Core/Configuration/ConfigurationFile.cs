//*******************************************************************************************************
//  ConfigurationFile.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
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
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/29/2008 - Pinal C. Patel
//       Added new CurrentWeb and CurrentWin static properties to get the configuration file for a 
//       specific type of application (i.e. Web or Windows).
//       Modified GetConfiguration() to remove dependency on HttpContext.Current to avoid exception in
//       async web application operations where HttpContext.Current is null.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Configuration;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
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
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
        /// </summary>
        public ConfigurationFile()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
        /// </summary>
        /// <param name="appType">One of the <see cref="ApplicationType"/> values.</param>
        public ConfigurationFile(ApplicationType appType)
            : this(string.Empty, appType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
        /// </summary>
        /// <param name="configFilePath">Path of the config file that belongs to another Windows or Web application.</param>
        public ConfigurationFile(string configFilePath)
            : this(configFilePath, TVA.Common.GetApplicationType())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFile"/> class.
        /// </summary>
        /// <param name="configFilePath">Path of the config file that belongs to another Windows or Web application.</param>
        /// <param name="appType">One of the <see cref="ApplicationType"/> values.</param>
        public ConfigurationFile(string configFilePath, ApplicationType appType)
        {
            m_configuration = GetConfiguration(configFilePath, appType);
            if (m_configuration.HasFile)
            {
                ValidateConfigurationFile(m_configuration.FilePath);
            }
            else
            {
                CreateConfigurationFile(m_configuration.FilePath);
            }
            m_configuration = GetConfiguration(configFilePath, appType);
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

        private System.Configuration.Configuration GetConfiguration(string configFilePath, ApplicationType appType)
        {
            System.Configuration.Configuration configuration = null;

            if (configFilePath != null)
            {
                if (string.IsNullOrEmpty(configFilePath) || FilePath.GetExtension(configFilePath) == ".config")
                {
                    // PCP - 12/12/2006: Using the TrimEnd function to get the correct value that needs to be passed
                    // to the method call for getting the Configuration object. The previous method (String.TrimEnd())
                    // yielded incorrect output resulting in the Configuration object not being initialized correctly.
                    switch (appType)
                    {
                        case ApplicationType.WindowsCui:
                        case ApplicationType.WindowsGui:
                            configuration = ConfigurationManager.OpenExeConfiguration(TrimEnd(configFilePath, ".config"));
                            break;
                        case ApplicationType.Web:
                            if (string.IsNullOrEmpty(configFilePath))
                                configFilePath = HostingEnvironment.ApplicationVirtualPath;
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
        /// Gets the <see cref="ConfigurationFile"/> object that represents the config file of the currently executing Windows or Web application.
        /// </summary>
        public static ConfigurationFile Current
        {
            get
            {
                if (m_current == null)
                    m_current = new ConfigurationFile();
                
                return m_current;
            }
        }

        /// <summary>
        /// Gets the <see cref="ConfigurationFile"/> object that represents the config file of the currently executing Web application.
        /// </summary>
        public static ConfigurationFile CurrentWeb
        {
            get
            {
                if (m_current == null)
                    m_current = new ConfigurationFile(ApplicationType.Web);

                return m_current;
            }
        }

        /// <summary>
        /// Gets the <see cref="ConfigurationFile"/> object that represents the config file of the currently executing Windows application.
        /// </summary>
        public static ConfigurationFile CurrentWin
        {
            get
            {
                if (m_current == null)
                    m_current = new ConfigurationFile(ApplicationType.WindowsGui);
                
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
