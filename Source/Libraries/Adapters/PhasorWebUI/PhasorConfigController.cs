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
using System.Linq;
using System.Web.Http;
using GSF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static PhasorProtocolAdapters.PhasorMeasurementMapper;

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
            File.WriteAllText(GetJsonConfigurationFileName(acronym), configuration.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Loads the configuration for the device with the given acronym.
        /// </summary>
        /// <param name="acronym">Acronym of device.</param>
        [HttpGet]
        [Authorize(Roles = "Administrator,Editor")]
        public string LoadDeviceConfiguration([FromUri(Name = "id")] string acronym)
        {
            string fileName = GetJsonConfigurationFileName(acronym);
            return File.Exists(fileName) ? File.ReadAllText(fileName) : "";
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets the file name of the JSON configuration file for the device with the given acronym.
        /// </summary>
        /// <param name="acronym">Acronym of device.</param>
        public static string GetJsonConfigurationFileName(string acronym)
        {
            // Path traversal attacks are prevented by replacing invalid file name characters
            return Path.Combine(JsonConfigurationPath, $"{acronym.ReplaceCharacters('_', c => Path.GetInvalidFileNameChars().Contains(c))}.json");
        }

        /// <summary>
        /// Gets the path to the configuration cache directory.
        /// </summary>
        public static string GetJsonConfigurationPath() => 
            JsonConfigurationPath;

        #endregion
    }
}
