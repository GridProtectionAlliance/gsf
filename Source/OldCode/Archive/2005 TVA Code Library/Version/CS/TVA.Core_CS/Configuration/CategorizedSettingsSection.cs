using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Configuration;

//*******************************************************************************************************
//  TVA.Configuration.CategorizedSettingsSection.vb - Categorized Settings Section
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
//  04/11/2006 - Pinal C. Patel
//       Generated original version of source code.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Configuration
	{
		
		/// <summary>
		/// Represents a section in the configuration with one or more
		/// TVA.Configuration.CategorizedSettingsCollection.
		/// </summary>
		public class CategorizedSettingsSection : ConfigurationSection
		{
			
			
			/// <summary>
			/// Gets the TVA.Configuration.CategorizedSettingsCollection for the specified category name.
			/// </summary>
			/// <param name="name">The name of the TVA.Configuration.CategorizedSettingsCollection to return.</param>
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
					name = TVA.Text.Common.RemoveWhiteSpace(Convert.ToString(nameChars));
					ConfigurationProperty configProperty = new ConfigurationProperty(name, typeof(CategorizedSettingsElementCollection));
					
					base.Properties.Add(configProperty);
					CategorizedSettingsElementCollection settingsCategory = null;
					if (base.Item(configProperty) != null)
					{
						settingsCategory = (CategorizedSettingsElementCollection) (base.Item(configProperty));
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
					return Category["general"];
				}
			}
			
			protected override void DeserializeSection(System.Xml.XmlReader reader)
			{
				
				System.IO.MemoryStream configSectionStream = new System.IO.MemoryStream();
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
				configSectionStream.Seek(0, System.IO.SeekOrigin.Begin);
				
				base.DeserializeSection(XmlReader.Create(configSectionStream));
				
			}
			
		}
		
	}
	
}
