//******************************************************************************************************
//  BinaryFileConfigurationLoader.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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

#pragma warning disable 0067

namespace GSF.TimeSeries.Configuration
{
    /// <summary>
    /// Represents a configuration loader that gets its configuration from a binary file.
    /// </summary>
    public class BinaryFileConfigurationLoader : IConfigurationLoader
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when the configuration loader has a message to provide about its current status.
        /// </summary>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Occurs when the configuration loader encounters a non-catastrophic exception.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ProcessException;

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
        public bool CanAugment
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads the entire configuration data set from scratch.
        /// </summary>
        /// <returns>The configuration data set.</returns>
        public DataSet Load()
        {
            DataSet configuration;

            OnStatusMessage(string.Format("Loading binary based configuration from \"{0}\".", m_filePath));

            using (FileStream stream = File.OpenRead(m_filePath))
            {
                configuration = stream.DeserializeToDataSet();
            }

            OnStatusMessage("Binary based configuration successfully loaded.");

            return configuration;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public void Augment(DataSet configuration)
        {
            throw new NotSupportedException();
        }

        private void OnStatusMessage(string message)
        {
            if ((object)StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(message));
        }

        #endregion
    }
}
