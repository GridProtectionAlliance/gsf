//******************************************************************************************************
//  BinaryFileConfigurationLoader.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  04/06/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using GSF.Data;
using GSF.Diagnostics;

#pragma warning disable 0067

namespace GSF.TimeSeries.Configuration
{
    /// <summary>
    /// Represents a configuration loader that gets its configuration from a binary file.
    /// </summary>
    public class BinaryFileConfigurationLoader : ConfigurationLoaderBase
    {
        #region [ Members ]

        // Fields
        private string m_filePath;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the path to the binary file.
        /// </summary>
        public string FilePath
        {
            get
            {
                return m_filePath;
            }
            set
            {
                m_filePath = value;
            }
        }

        /// <summary>
        /// Gets the flag that indicates whether augmentation is supported by this configuration loader.
        /// </summary>
        public override bool CanAugment => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads the entire configuration data set from scratch.
        /// </summary>
        /// <returns>The configuration data set.</returns>
        public override DataSet Load()
        {
            DataSet configuration;

            OnStatusMessage(MessageLevel.Info, $"Loading binary based configuration from \"{m_filePath}\".");

            using (FileStream stream = File.OpenRead(m_filePath))
            {
                configuration = stream.DeserializeToDataSet();
            }

            OnStatusMessage(MessageLevel.Info, "Binary based configuration successfully loaded.");

            return configuration;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Augment(DataSet configuration)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
