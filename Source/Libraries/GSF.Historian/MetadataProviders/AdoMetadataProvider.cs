﻿//******************************************************************************************************
//  AdoMetadataProvider.cs - Gbtc
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
//  09/15/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/17/2009 - Pinal C. Patel
//       Renamed ConnectString to ConnectionString.
//  12/11/2009 - Pinal C. Patel
//       Disabled the encryption of DataProviderString when persisted to the config file.
//  02/03/2010 - Pinal C. Patel
//       Disabled the encryption of ConnectionString when persisted to the config file.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modifeid Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;

namespace GSF.Historian.MetadataProviders;

/// <summary>
/// Represents a provider of data to a <see cref="GSF.Historian.Files.MetadataFile"/> from any ADO.NET based data store.
/// </summary>
/// <seealso cref="MetadataUpdater"/>
public class AdoMetadataProvider : MetadataProviderBase
{
    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="AdoMetadataProvider"/> class.
    /// </summary>
    public AdoMetadataProvider()
    {
        ConnectionString = string.Empty;
        DataProviderString = string.Empty;
        SelectString = string.Empty;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the connection string for connecting to the ADO.NET based data store of metadata.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the ADO.NET data provider assembly type creation string.
    /// </summary>
    /// <remarks>
    /// Expected keys: AssemblyName;ConnectionType<br/>
    /// Examples:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Database Connection Type</term>
    ///         <description>Example ADO.NET Data Provider String</description>
    ///     </listheader>
    ///     <item>
    ///         <term>SQL Server</term>
    ///         <description>AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.SqlClient.SqlConnection</description>
    ///     </item>
    ///     <item>
    ///         <term>MySQL</term>
    ///         <description>AssemblyName={MySql.Data, Version=5.2.7.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d};ConnectionType=MySql.Data.MySqlClient.MySqlConnection</description>
    ///     </item>
    ///     <item>
    ///         <term>Oracle</term>
    ///         <description>AssemblyName={System.Data.OracleClient, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OracleClient.OracleConnection</description>
    ///     </item>
    ///     <item>
    ///         <term>OleDb</term>
    ///         <description>AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection</description>
    ///     </item>
    ///     <item>
    ///         <term>ODBC</term>
    ///         <description>AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.Odbc.OdbcConnection</description>
    ///     </item>
    /// </list>
    /// </remarks>
    public string DataProviderString { get; set; }

    /// <summary>
    /// Gets or sets the SELECT statement for retrieving metadata from the ADO.NET based data store.
    /// </summary>
    public string SelectString { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Saves <see cref="AdoMetadataProvider"/> settings to the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
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
        settings[nameof(DataProviderString), true].Update(DataProviderString);
        settings[nameof(SelectString), true].Update(SelectString);
        
        config.Save();
    }

    /// <summary>
    /// Loads saved <see cref="AdoMetadataProvider"/> settings from the config file if the <see cref="GSF.Adapters.Adapter.PersistSettings"/> property is set to true.
    /// </summary>
    public override void LoadSettings()
    {
        base.LoadSettings();

        if (!PersistSettings)
            return;
        
        // Load settings from the specified category.
        ConfigurationFile config = ConfigurationFile.Current;
        CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
        
        settings.Add(nameof(ConnectionString), "Eval(SystemSettings.ConnectionString)", "Connection string for connecting to the ADO.NET based data store of metadata.");
        settings.Add(nameof(DataProviderString), "Eval(SystemSettings.DataProviderString)", "The ADO.NET data provider assembly type creation string used to create a connection to the data store of metadata.");
        settings.Add(nameof(SelectString), SelectString, "SELECT statement for retrieving metadata from the ADO.NET based data store.");
        
        DataProviderString = settings[nameof(DataProviderString)].ValueAs(DataProviderString);
        SelectString = settings[nameof(SelectString)].ValueAs(SelectString);

        // Load the connection string from the specified category.
        string connectionString = settings[nameof(ConnectionString)].ValueAs(ConnectionString);
        Dictionary<string, string> connectionSettings = connectionString.ParseKeyValuePairs();

        if (connectionSettings.TryGetValue("Provider", out string connectionSetting))
        {
            // Check if provider is for Access
            if (connectionSetting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
            {
                // Make sure path to Access database is fully qualified
                if (connectionSettings.TryGetValue("Data Source", out connectionSetting))
                {
                    connectionSettings["Data Source"] = FilePath.GetAbsolutePath(connectionSetting);
                    connectionString = connectionSettings.JoinKeyValuePairs();
                }
            }
        }

        ConnectionString = connectionString;
    }

    /// <summary>
    /// Refreshes the <see cref="MetadataProviderBase.Metadata"/> from an ADO.NET based data store.
    /// </summary>
    /// <exception cref="ArgumentNullException"><see cref="ConnectionString"/> or <see cref="SelectString"/> is set to a null or empty string.</exception>
    protected override void RefreshMetadata()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new ArgumentNullException(nameof(ConnectionString));

        if (string.IsNullOrEmpty(SelectString))
            throw new ArgumentNullException(nameof(SelectString));
            
        // Attempt to load configuration from an ADO.NET database connection
        IDbConnection connection = null;

        try
        {
            Dictionary<string, string> settings = DataProviderString.ParseKeyValuePairs();
            string assemblyName = settings[nameof(AssemblyName)].ToNonNullString();
            string connectionTypeName = settings["ConnectionType"].ToNonNullString();
            string adapterTypeName = settings["AdapterType"].ToNonNullString();

            if (string.IsNullOrEmpty(connectionTypeName))
                throw new InvalidOperationException("Database connection type was not defined");

            if (string.IsNullOrEmpty(adapterTypeName))
                throw new InvalidOperationException("Database adapter type was not defined");

            Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
            Type connectionType = assembly.GetType(connectionTypeName);

            assembly.GetType(adapterTypeName);

            // Open ADO.NET provider connection
            connection = (IDbConnection)Activator.CreateInstance(connectionType);
            connection.ConnectionString = ConnectionString;
            connection.Open();

            // Update existing metadata
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