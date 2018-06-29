//******************************************************************************************************
//  ModbusConfigurationController.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  06/29/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.IO;
using System.Web.Http;
using GSF.Configuration;
using GSF.IO;

namespace ModbusAdapters
{
    public class ModbusConfigController : ApiController
    {
        #region [ Methods ]

        [HttpPost]
        [Authorize(Roles = "Administrator,Editor")]
        public void SaveDeviceConfiguration([FromUri(Name = "id")] string acronym, [FromBody] string configuration)
        {
            File.WriteAllText(GetConfigurationCacheFileName(acronym), configuration);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Editor")]
        public string LoadDeviceConfiguration([FromUri(Name = "id")] string acronym)
        {
            string fileName = GetConfigurationCacheFileName(acronym);
            return File.Exists(fileName) ? File.ReadAllText(fileName) : "";
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Editor")]
        public string GetConfigurationCacheFileName([FromUri(Name = "id")] string acronym)
        {
            return Path.Combine(ConfigurationCachePath, $"{acronym}.configuration.json");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Editor")]
        public string GetConfigurationCachePath()
        {
            return ConfigurationCachePath;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static string s_configurationCachePath;


        // Static Methods

        private static string ConfigurationCachePath
        {
            get
            {
                // This property will not change during system life-cycle so we cache if for future use
                if (string.IsNullOrEmpty(s_configurationCachePath))
                {
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
                }

                return s_configurationCachePath;
            }
        }

        #endregion
    }
}
