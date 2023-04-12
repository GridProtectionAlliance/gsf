//******************************************************************************************************
//  PhasorConfigController.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  03/27/2023 - J. Ritchie Carroll
//       Generated original version of source code based on similar class in ModbusAdapters.
//
//******************************************************************************************************

using System.IO;
using System.Web.Http;
using GSF.Configuration;
using GSF.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PhasorWebUI
{
    /// <summary>
    /// Defines an API controller for saving and loading phasor protocol configurations.
    /// </summary>
    public class PhasorConfigController : ApiController
    {
        #region [ Methods ]

        /// <summary>
        /// Saves the configuration for the device with the given acronym.
        /// </summary>
        /// <param name="acronym">Acronym of device.</param>
        /// <param name="configuration">JSON config.</param>
        [HttpPost]
        [Authorize(Roles = "Administrator,Editor")]
        public void SaveDeviceConfiguration([FromUri(Name = "id")] string acronym, [FromBody] JToken configuration)
        {
            File.WriteAllText(GetConfigurationCacheFileName(acronym), configuration.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Loads the configuration for the device with the given acronym.
        /// </summary>
        /// <param name="acronym">Acronym of device.</param>
        [HttpGet]
        [Authorize(Roles = "Administrator,Editor")]
        public string LoadDeviceConfiguration([FromUri(Name = "id")] string acronym)
        {
            string fileName = GetConfigurationCacheFileName(acronym);
            return File.Exists(fileName) ? File.ReadAllText(fileName) : "";
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string s_configurationCachePath;

        // Static Properties
        private static string ConfigurationCachePath
        {
            get
            {
                // This property will not change during system life-cycle so we cache if for future use
                if (!string.IsNullOrEmpty(s_configurationCachePath))
                    return s_configurationCachePath;

                // Define default configuration cache directory relative to path of host application
                s_configurationCachePath = string.Format("{0}{1}ConfigurationCache{1}", FilePath.GetAbsolutePath(""), Path.DirectorySeparatorChar);

                // Make sure configuration cache path setting exists within system settings section of config file
                ConfigurationFile configFile = ConfigurationFile.Current;
                CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                systemSettings.Add("ConfigurationCachePath", s_configurationCachePath, "Defines the path used to cache serialized phasor protocol configurations");

                // Retrieve configuration cache directory as defined in the config file
                s_configurationCachePath = FilePath.AddPathSuffix(systemSettings["ConfigurationCachePath"].Value);

                // Make sure configuration cache directory exists
                if (!Directory.Exists(s_configurationCachePath))
                    Directory.CreateDirectory(s_configurationCachePath);

                return s_configurationCachePath;
            }
        }

        /// <summary>
        /// Gets the file name of the configuration cache file for the device with the given acronym.
        /// </summary>
        /// <param name="acronym">Acronym of device.</param>
        public static string GetConfigurationCacheFileName([FromUri(Name = "id")] string acronym) => 
            Path.Combine(ConfigurationCachePath, $"{acronym}.configuration.json");

        /// <summary>
        /// Gets the path to the configuration cache directory.
        /// </summary>
        public static string GetConfigurationCachePath() => 
            ConfigurationCachePath;

        #endregion
    }
}
