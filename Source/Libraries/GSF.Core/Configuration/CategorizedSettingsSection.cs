//******************************************************************************************************
//  CategorizedSettingsSection.cs - Gbtc
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
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C. Patel
//       Converted code to C#.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/20/2010 - Pinal C. Patel
//       Added new File property for the purpose of managing user scope setting.
//  06/16/2010 - J. Ritchie Carroll
//       Added Remove method to remove a categorized settings section.
//  06/16/2010 - Pinal C. Patel
//       Fixed Remove method to remove a categorized settings section.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using GSF.Collections;
using GSF.IO;
using GSF.Xml;

namespace GSF.Configuration
{
    /// <summary>
    /// Represents a section in the config file with one or more <see cref="CategorizedSettingsElementCollection"/> 
    /// representing categories, each containing one or more <see cref="CategorizedSettingsElement"/> objects 
    /// representing settings under a specific category.
    /// </summary>
    /// <seealso cref="CategorizedSettingsElement"/>
    /// <seealso cref="CategorizedSettingsElementCollection"/>
    public class CategorizedSettingsSection : ConfigurationSection
    {
        #region [ Members ]

        // Fields
        private string m_cryptoKey;
        private ConfigurationFile m_file;
        private readonly Dictionary<string, CategorizedSettingsElementCollection> m_sections;
        private bool m_sectionLoaded;

        #endregion

        /// <summary>
        /// Creates a new <see cref="CategorizedSettingsSection"/>.
        /// </summary>
        public CategorizedSettingsSection()
        {
#if MONO
            m_sections = new Dictionary<string, CategorizedSettingsElementCollection>();
#else
            m_sections = null;
#endif
        }

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationFile"/> to which this <see cref="CategorizedSettingsSection"/> belongs.
        /// </summary>
        public ConfigurationFile File
        {
            get
            {
                return m_file;
            }
            internal set
            {
                m_file = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsElementCollection"/> object representing settings under the specified category name.
        /// </summary>
        /// <param name="name">Name of the category whose settings are to be retrieved.</param>
        /// <returns><see cref="CategorizedSettingsElementCollection"/> object with settings under the specified category name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty string.</exception>
        public new CategorizedSettingsElementCollection this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                // We will add the requested category to the default properties collection, so that when
                // the settings are saved off to the config file, all of the categories under which
                // settings may be saved are processed and saved to the config file. This is essentially
                // doing what marking a property with the <ConfigurationProperty()> attribute does.
                // Make the first letter of category name lower case, if not already.
                char[] nameChars = name.ToCharArray();
                nameChars[0] = char.ToLower(nameChars[0]);

                // Do not allow spaces in the name so that underlying .Net configuration API does not break.
                name = (new string(nameChars)).RemoveWhiteSpace();

                ConfigurationProperty configProperty = new ConfigurationProperty(name, typeof(CategorizedSettingsElementCollection));
                CategorizedSettingsElementCollection settingsCategory = null;

                if ((object)m_sections == null)
                {
                    base.Properties.Add(configProperty);

                    if ((object)base[configProperty] != null)
                    {
                        settingsCategory = (CategorizedSettingsElementCollection)base[configProperty];
                        settingsCategory.Name = name;
                        settingsCategory.Section = this;
                        settingsCategory.SetCryptoKey(m_cryptoKey);
                    }
                }
                else
                {
                    settingsCategory = m_sections.GetOrAdd(name, sectionName =>
                    {
                        base.Properties.Add(configProperty);

                        CategorizedSettingsElementCollection settings = new CategorizedSettingsElementCollection
                        {
                            Name = sectionName,
                            Section = this,
                        };

                        settings.SetCryptoKey(m_cryptoKey);
                        Modified = true;

                        return settings;
                    });
                }

                return settingsCategory;
            }
        }

        /// <summary>
        /// Gets the <see cref="CategorizedSettingsElementCollection"/> object representing settings under "general" category.
        /// </summary>
        /// <returns><see cref="CategorizedSettingsElementCollection"/> object with settings under the "general" category.</returns>
        public CategorizedSettingsElementCollection General
        {
            get
            {
                return this["general"];
            }
        }

        internal bool SectionLoaded
        {
            get
            {
                return m_sectionLoaded;
            }
        }

        internal bool Modified
        {
            set
            {
                if ((object)m_sections != null && (object)m_file != null && value)
                    m_file.m_forceSave = true;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the key to be used for encrypting and decrypting setting values.
        /// </summary>
        /// <param name="cryptoKey">New crypto key.</param>
        public void SetCryptoKey(string cryptoKey)
        {
            m_cryptoKey = cryptoKey;
        }

        /// <summary>
        /// Removes the specified category name including its associated settings.
        /// </summary>
        /// <param name="name">Name of the category to be removed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null or empty string.</exception>
        public void Remove(string name)
        {
            // Get the category to be removed.
            CategorizedSettingsElementCollection settingsCategory = this[name];

            // Remove existing category settings.
            while (settingsCategory.GetEnumerator().MoveNext())
            {
                settingsCategory.RemoveAt(0);
            }

            // Remove category from property bag.
            if ((object)m_sections == null)
                base.Properties.Remove(settingsCategory.Name);
            else
                m_sections.Remove(settingsCategory.Name);
        }

        /// <summary>
        /// Reads XML from the configuration file.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> object, which reads from the configuration file.</param>
        protected override void DeserializeSection(XmlReader reader)
        {
            using (BlockAllocatedMemoryStream configSectionStream = new BlockAllocatedMemoryStream())
            {
                XmlDocument configSection = new XmlDocument();

                configSection.Load(reader);
                configSection.Save(configSectionStream);

                // Adds all the categories that are under the categorizedSettings section of the configuration file
                // to the property collection. Again, this is essentially doing what marking a property with the
                // <ConfigurationProperty()> attribute does. If this is not done, then an exception will be raised
                // when the category elements are being deserialized.
                if ((object)configSection.DocumentElement != null)
                {
                    XmlNodeList categories = configSection.DocumentElement.SelectNodes("*");

                    if ((object)categories != null)
                    {
                        foreach (XmlNode category in categories)
                        {
                            ConfigurationProperty configProperty = new ConfigurationProperty(category.Name, typeof(CategorizedSettingsElementCollection));

                            base.Properties.Add(configProperty);

                            if ((object)m_sections != null)
                            {
                                CategorizedSettingsElementCollection settingsCategory = new CategorizedSettingsElementCollection
                                {
                                    Name = category.Name,
                                    Section = this,
                                };

                                settingsCategory.SetCryptoKey(m_cryptoKey);
                                m_sections.Add(category.Name, settingsCategory);

                                // Read all elements within this category section
                                XmlNodeList elements = category.SelectNodes("*");
                                SettingScope scope;

                                if ((object)elements != null)
                                {
                                    foreach (XmlNode element in elements)
                                    {
                                        CategorizedSettingsElement categorySetting = new CategorizedSettingsElement(settingsCategory);

                                        categorySetting.Name = element.GetAttributeValue("name");
                                        categorySetting.Value = element.GetAttributeValue("value");
                                        categorySetting.Description = element.GetAttributeValue("description") ?? "";
                                        categorySetting.Encrypted = element.GetAttributeValue("encrypted").ToNonNullNorWhiteSpace("false").ParseBoolean();

                                        if (Enum.TryParse(element.GetAttributeValue("scope").ToNonNullNorWhiteSpace("Application"), out scope))
                                            categorySetting.Scope = scope;
                                        else
                                            categorySetting.Scope = SettingScope.Application;

                                        settingsCategory.Add(categorySetting);
                                    }
                                }
                            }
                        }
                    }
                }

                m_sectionLoaded = true;

                if ((object)m_sections == null)
                {
                    configSectionStream.Seek(0, SeekOrigin.Begin);
                    base.DeserializeSection(XmlReader.Create(configSectionStream));
                }
            }
        }

        /// <summary>
        /// Creates an XML string containing an unmerged view of the <see cref="ConfigurationSection"/> object as a single section to write to a file.
        /// </summary>
        /// <returns>
        /// An XML string containing an unmerged view of the <see cref="ConfigurationSection"/> object.
        /// </returns>
        /// <param name="parentElement">The <see cref="ConfigurationElement"/> instance to use as the parent when performing the un-merge.</param>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="saveMode">The <see cref="ConfigurationSaveMode"/> instance to use when writing to a string.</param>
        protected override string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            if ((object)m_sections != null)
            {
                const string tempRoot = "__tempRoot__";

                using (BlockAllocatedMemoryStream stream = new BlockAllocatedMemoryStream())
                {
                    XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);

                    writer.Indentation = 2;
                    writer.Formatting = Formatting.Indented;

                    // Add a temporary root so that indentation is at the desired level
                    writer.WriteStartElement(tempRoot);
                    writer.WriteStartElement(name);

                    foreach (string section in m_sections.Keys)
                    {
                        CategorizedSettingsElementCollection categorySettings = this[section];

                        // Add category section
                        writer.WriteStartElement(section);

                        // Write each category value
                        foreach (CategorizedSettingsElement categorySetting in categorySettings)
                        {
                            // <add name="TestConfigParam" value="-1.0" description="Parameter description." encrypted="false" scope="User"/>
                            writer.WriteStartElement("add");

                            writer.WriteAttributeString("name", categorySetting.Name);
                            writer.WriteAttributeString("value", categorySetting.SerializedValue);
                            writer.WriteAttributeString("description", categorySetting.Description ?? "");
                            writer.WriteAttributeString("encrypted", categorySetting.Encrypted.ToString());

                            if (categorySetting.Scope == SettingScope.User)
                                writer.WriteAttributeString("scope", categorySetting.Scope.ToString());

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    writer.Flush();

                    string settings = Encoding.UTF8.GetString(stream.ToArray());

                    // Remove temporary root
                    return settings.Replace(string.Format("<{0}>", tempRoot), "").Replace(string.Format("</{0}>", tempRoot), "");
                }
            }

            return base.SerializeSection(parentElement, name, saveMode);
        }

        #endregion
    }
}
