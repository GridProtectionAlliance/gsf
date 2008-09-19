//*******************************************************************************************************
//  CategorizedSettingsSection.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/17/2008 - Pinal C Patel
//       Converted code to C#.
//
//*******************************************************************************************************

using System;
using System.Configuration;
using System.Xml;
using System.IO;
//using TVA.Text;

namespace TVA.Configuration
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
        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="CategorizedSettingsElementCollection"/> representing settings under the specified category name.
        /// </summary>
        /// <param name="name">Name of the category whose settings are to be retrieved.</param>
        /// <returns>The TVA.Configuration.CategorizedSettingsCollection with the specified name; otherwise null.</returns>
        public CategorizedSettingsElementCollection this[string name]
        {
            get
            {
                // We will add the requested category to the default properties collection, so that when
                // the settings are saved off to the config file, all of the categories under which
                // settings may be saved are processed and saved to the config file. This is essentially
                // doing what marking a property with the <ConfigurationProperty()> attribute does.
                // Make the first letter of category name lower case, if not already.
                char[] nameChars = name.ToCharArray();
                nameChars[0] = char.ToLower(nameChars[0]);
                // Do not allow spaces in the category name, so that underlying .Net configuration API does not
                // break.
                name = (new string(nameChars)).RemoveWhiteSpace();
                ConfigurationProperty configProperty = new ConfigurationProperty(name, typeof(CategorizedSettingsElementCollection));

                base.Properties.Add(configProperty);
                CategorizedSettingsElementCollection settingsCategory = null;
                if (base[configProperty] != null)
                {
                    settingsCategory = (CategorizedSettingsElementCollection)base[configProperty];
                }
                return settingsCategory;
            }
        }

        /// <summary>
        /// Gets the "general" category under the "categorizedSettings" of the configuration file.
        /// </summary>
        /// <returns>The TVA.Configuration.CategorizedSettingsCollection of the "general" category.</returns>
        public CategorizedSettingsElementCollection General
        {
            get
            {
                return this["general"];
            }
        }

        protected override void DeserializeSection(XmlReader reader)
        {
            MemoryStream configSectionStream = new MemoryStream();
            XmlDocument configSection = new XmlDocument();
            configSection.Load(reader);
            configSection.Save(configSectionStream);
            // Adds all the categories that are under the categorizedSettings section of the configuration file
            // to the property collection. Again, this is essentially doing what marking a property with the
            // <ConfigurationProperty()> attribute does. If this is not done, then an exception will be raised
            // when the category elements are being deserialized.
            foreach (XmlNode category in configSection.DocumentElement.SelectNodes("*"))
            {
                base.Properties.Add(new ConfigurationProperty(category.Name, typeof(CategorizedSettingsElementCollection)));
            }
            configSectionStream.Seek(0, SeekOrigin.Begin);

            base.DeserializeSection(XmlReader.Create(configSectionStream));
        }

        #endregion
    }
}
