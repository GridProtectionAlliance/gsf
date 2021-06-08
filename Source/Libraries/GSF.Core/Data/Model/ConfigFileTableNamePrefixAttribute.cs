//******************************************************************************************************
//  ConfigFileTableNamePrefixAttribute.cs - Gbtc
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
//  06/08/2021 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************



using GSF.Configuration;
using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will allow a general table name prefix for a modeled table instead of using
    /// the config file setting TableNamePrefix.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigFileTableNamePrefixAttribute : Attribute
    {
        /// <summary>
        /// Gets field name to use for property.
        /// </summary>
        public string Prefix
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigFileTableNamePrefixAttribute"/>.
        /// </summary>
        /// 
        public ConfigFileTableNamePrefixAttribute()
        {
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings["systemSettings"];
            settings.Add("TableNamePrefix", "", "Allows for a general prefixing of table names so databases can be consolidated.");
            string prefix = settings["TableNamePrefix"].ValueAsString("");
            Prefix = prefix;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigFileTableNamePrefixAttribute"/> from the systemSettings section and <paramref name="setting"/> used in the config file.
        /// </summary>
        /// 
        /// <param name="setting">Name the setting to use out of the config file systemSettings section.</param>
        public ConfigFileTableNamePrefixAttribute(string setting)
        {
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings["systemSettings"];
            settings.Add(setting, "", "Allows for a general prefixing of table names so databases can be consolidated.");
            string prefix = settings[setting].ValueAsString("");
            Prefix = prefix;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigFileTableNamePrefixAttribute"/> from the <paramref name="settingsCategory"/> and <paramref name="setting"/> used in the config file.
        /// </summary>
        /// 
        /// <param name="settingsCategory">Name the settings category to use out of the config file systemSettings section.</param>
        /// <param name="setting">Name the setting to use out of the config file systemSettings section.</param>
        public ConfigFileTableNamePrefixAttribute(string settingsCategory, string setting)
        {
            ConfigurationFile config = ConfigurationFile.Current;
            CategorizedSettingsElementCollection settings = config.Settings[settingsCategory];
            settings.Add(setting, "", "Allows for a general prefixing of table names so databases can be consolidated.");
            string prefix = settings[setting].ValueAsString("");
            Prefix = prefix;
        }

    }
}