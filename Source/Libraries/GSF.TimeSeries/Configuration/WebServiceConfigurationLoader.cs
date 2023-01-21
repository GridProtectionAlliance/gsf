//******************************************************************************************************
//  WebServiceConfigurationLoader.cs - Gbtc
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
using System.Net;
using GSF.Diagnostics;

#pragma warning disable 0067

namespace GSF.TimeSeries.Configuration
{
    /// <summary>
    /// Represents a configuration loader that queries a web service for its configuration.
    /// </summary>
    public class WebServiceConfigurationLoader : ConfigurationLoaderBase
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the URI for the web service.
        /// </summary>
        public string URI { get; set; }

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
            WebResponse response;

            OnStatusMessage(MessageLevel.Info, "Webservice configuration connection opened.");

            DataSet configuration = new();
            WebRequest request = WebRequest.Create(URI);

            using (response = request.GetResponse())
            {
                configuration.ReadXml(response.GetResponseStream());
            }

            OnStatusMessage(MessageLevel.Info, "Webservice configuration successfully loaded.");

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
