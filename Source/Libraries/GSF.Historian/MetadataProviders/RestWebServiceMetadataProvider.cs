//******************************************************************************************************
//  RestWebServiceMetadataProvider.cs - Gbtc
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
//  08/11/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/18/2009 - Pinal C. Patel
//       Added cleanup code for response stream of the REST web service.
//  08/21/2009 - Pinal C. Patel
//       Moved RestDataFormat to Services namespace as SerializationFormat.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Net;
using GSF.Configuration;

namespace GSF.Historian.MetadataProviders;

/// <summary>
/// Represents a provider of data to a <see cref="GSF.Historian.Files.MetadataFile"/> from a REST (Representational State Transfer) web service.
/// </summary>
/// <seealso cref="MetadataUpdater"/>
public class RestWebServiceMetadataProvider : MetadataProviderBase
{
    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="RestWebServiceMetadataProvider"/> class.
    /// </summary>
    public RestWebServiceMetadataProvider()
    {
        ServiceUri = string.Empty;
        ServiceDataFormat = SerializationFormat.Xml;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the URI where the REST web service is hosted.
    /// </summary>
    public string ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SerializationFormat"/> in which the REST web service exposes the data.
    /// </summary>
    public SerializationFormat ServiceDataFormat { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Saves <see cref="RestWebServiceMetadataProvider"/> settings to the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void SaveSettings()
    {
        base.SaveSettings();

        if (!PersistSettings)
            return;
        
        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings[nameof(ServiceUri), true].Update(ServiceUri);
        settings[nameof(ServiceDataFormat), true].Update(ServiceDataFormat);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved <see cref="RestWebServiceMetadataProvider"/> settings from the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void LoadSettings()
    {
        base.LoadSettings();

        if (!PersistSettings)
            return;
        
        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings.Add(nameof(ServiceUri), ServiceUri, "URI where the REST web service is hosted.");
        settings.Add(nameof(ServiceDataFormat), ServiceDataFormat, "Format (Json; PoxAsmx; PoxRest) in which the REST web service exposes the data.");
        
        ServiceUri = settings[nameof(ServiceUri)].ValueAs(ServiceUri);
        ServiceDataFormat = settings[nameof(ServiceDataFormat)].ValueAs(ServiceDataFormat);
    }

    /// <summary>
    /// Refreshes the <see cref="MetadataProviderBase.Metadata"/> from a REST web service.
    /// </summary>
    /// <exception cref="ArgumentNullException"><see cref="ServiceUri"/> is set to a null or empty string.</exception>
    protected override void RefreshMetadata()
    {
        if (string.IsNullOrEmpty(ServiceUri))
            throw new ArgumentNullException(nameof(ServiceUri));

        WebResponse response = null;
        Stream responseStream = null;

        try
        {
            // Retrieve new metadata.
            response = WebRequest.Create(ServiceUri).GetResponse();
            responseStream = response.GetResponseStream();

            // Update existing metadata.
            MetadataUpdater metadataUpdater = new(Metadata);
            metadataUpdater.UpdateMetadata(responseStream, ServiceDataFormat);
        }
        finally
        {
            response?.Close();
            responseStream?.Dispose();
        }
    }

    #endregion
}