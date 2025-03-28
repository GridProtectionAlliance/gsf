﻿//******************************************************************************************************
//  OleDbMetadataProvider.cs - Gbtc
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
//  07/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/17/2009 - Pinal C. Patel
//       Renamed ConnectString to ConnectionString.
//  02/03/2010 - Pinal C. Patel
//       Disabled the encryption of ConnectionString when persisted to the config file.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Data.OleDb;
using GSF.Configuration;
using GSF.Data;

namespace GSF.Historian.MetadataProviders;

/// <summary>
/// Represents a provider of data to a <see cref="GSF.Historian.Files.MetadataFile"/> from any OLE DB data store.
/// </summary>
/// <seealso cref="MetadataUpdater"/>
public class OleDbMetadataProvider : MetadataProviderBase
{
    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="OleDbMetadataProvider"/> class.
    /// </summary>
    public OleDbMetadataProvider()
    {
        ConnectionString = string.Empty;
        SelectString = string.Empty;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the connection string for connecting to the OLE DB data store of metadata.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the SELECT statement for retrieving metadata from the OLE DB data store.
    /// </summary>
    public string SelectString { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Saves <see cref="OleDbMetadataProvider"/> settings to the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void SaveSettings()
    {
        base.SaveSettings();

        if (!PersistSettings)
            return;
        
        // Save settings under the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings[nameof(ConnectionString), true].Update(ConnectionString);
        settings[nameof(SelectString), true].Update(SelectString);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved <see cref="OleDbMetadataProvider"/> settings from the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void LoadSettings()
    {
        base.LoadSettings();

        if (!PersistSettings)
            return;
        
        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings.Add(nameof(ConnectionString), ConnectionString, "Connection string for connecting to the OLE DB data store of metadata.");
        settings.Add(nameof(SelectString), SelectString, "SELECT statement for retrieving metadata from the OLE DB data store.");
        
        ConnectionString = settings[nameof(ConnectionString)].ValueAs(ConnectionString);
        SelectString = settings[nameof(SelectString)].ValueAs(SelectString);
    }

    /// <summary>
    /// Refreshes the <see cref="MetadataProviderBase.Metadata"/> from an OLE DB data store.
    /// </summary>
    /// <exception cref="ArgumentNullException"><see cref="ConnectionString"/> or <see cref="SelectString"/> is set to a null or empty string.</exception>
    protected override void RefreshMetadata()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new ArgumentNullException(nameof(ConnectionString));

        if (string.IsNullOrEmpty(SelectString))
            throw new ArgumentNullException(nameof(SelectString));

        OleDbConnection connection = new(ConnectionString);

        try
        {
            // Open OleDb connection.
            connection.Open();

            // Update existing metadata.
            MetadataUpdater metadataUpdater = new(Metadata);
            metadataUpdater.UpdateMetadata(connection.ExecuteReader(SelectString));
        }
        finally
        {
            connection?.Dispose();
        }
    }

    #endregion
}