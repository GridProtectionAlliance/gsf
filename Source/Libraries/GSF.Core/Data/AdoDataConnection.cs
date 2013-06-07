//******************************************************************************************************
//  AdoDataConnection.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/07/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/19/2011 - Stephen C. Wills
//       Added database awareness and Oracle database compatibility.
//  10/18/2011 - J. Ritchie Carroll
//       Modified ADO database class to allow directly instantiated instances, as well as configured.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using GSF.Configuration;

namespace GSF.Data
{
    /// <summary>
    /// Specifies the database type underlying an <see cref="AdoDataConnection"/>.
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Underlying ADO database type is Microsoft Access.
        /// </summary>
        Access,

        /// <summary>
        /// Underlying ADO database type is SQL Server.
        /// </summary>
        SQLServer,

        /// <summary>
        /// Underlying ADO database type is MySQL.
        /// </summary>
        MySQL,

        /// <summary>
        /// Underlying ADO database type is Oracle.
        /// </summary>
        Oracle,

        /// <summary>
        /// Underlying ADO database type is SQLite.
        /// </summary>
        SQLite,

        /// <summary>
        /// Underlying ADO database type is unknown.
        /// </summary>
        Other
    }

    /// <summary>
    /// Creates a new <see cref="IDbConnection"/> from any specified or configured ADO.NET data source.
    /// </summary>
    /// <remarks>
    /// Example connection and data provider strings:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Database type</term>
    ///         <description>Example connection string / data provider string</description>
    ///     </listheader>
    ///     <item>
    ///         <term>SQL Server</term>
    ///         <description>
    ///         ConnectionString = "Data Source=serverName; Initial Catalog=databaseName; User ID=userName; Password=password"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Oracle</term>
    ///         <description>
    ///         ConnectionString = "Data Source=tnsName; User ID=schemaUserName; Password=schemaPassword"<br/>
    ///         DataProviderString = "AssemblyName={Oracle.DataAccess, Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342}; ConnectionType=Oracle.DataAccess.Client.OracleConnection; AdapterType=Oracle.DataAccess.Client.OracleDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>MySQL</term>
    ///         <description>
    ///         ConnectionString = "Server=serverName; Database=databaseName; Uid=root; Pwd=password; allow user variables = true"<br/>
    ///         DataProviderString = "AssemblyName={MySql.Data, Version=6.3.6.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d}; ConnectionType=MySql.Data.MySqlClient.MySqlConnection; AdapterType=MySql.Data.MySqlClient.MySqlDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SQLite</term>
    ///         <description>
    ///         ConnectionString = "Data Source=databaseName.db; Version=3"<br/>
    ///         DataProviderString = "AssemblyName={System.Data.SQLite, Version=1.0.74.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>Access / OleDB</term>
    ///         <description>
    ///         ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=databaseName.mdb"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.OleDb.OleDbConnection; AdapterType=System.Data.OleDb.OleDbDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>ODBC Connection</term>
    ///         <description>
    ///         ConnectionString = "Driver={SQL Server Native Client 10.0}; Server=serverName; Database=databaseName; Uid=userName; Pwd=password"<br/>
    ///         DataProviderString = "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.Odbc.OdbcConnection; AdapterType=System.Data.Odbc.OdbcDataAdapter"
    ///         </description>
    ///     </item>
    /// </list>
    /// Example configuration file that defines connection settings:
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8"?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="categorizedSettings" type="GSF.Configuration.CategorizedSettingsSection, GSF.Core" />
    ///   </configSections>
    ///   <categorizedSettings>
    ///     <systemSettings>
    ///       <add name="ConnectionString" value="Data Source=localhost\SQLEXPRESS; Initial Catalog=MyDatabase; Integrated Security=SSPI" description="ADO database connection string" encrypted="false" />
    ///       <add name="DataProviderString" value="AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter" description="ADO database provider string" encrypted="false" />
    ///     </systemSettings>
    ///   </categorizedSettings>
    ///   <startup>
    ///     <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0" />
    ///   </startup>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </remarks>
    public class AdoDataConnection : IDisposable
    {
        #region [ Members ]

        // Fields
        private IDbConnection m_connection;
        private DatabaseType m_databaseType;
        private readonly string m_connectionString;
        private readonly Type m_connectionType;
        private readonly Type m_adapterType;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> based on connection settings in configuration file.
        /// </summary>
        /// <param name="settingsCategory">Settings category to use for connection settings.</param>
        public AdoDataConnection(string settingsCategory)
        {
            if (string.IsNullOrWhiteSpace(settingsCategory))
                throw new ArgumentNullException("settingsCategory", "Parameter cannot be null or empty");

            // Only need to establish data types and load settings once per defined section since they are being loaded from config file
            AdoDataConnection configuredConnection;

            if (!s_configuredConnections.TryGetValue(settingsCategory, out configuredConnection))
            {
                string connectionString, dataProviderString;

                try
                {
                    // Load connection settings from the system settings category				
                    ConfigurationFile config = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection configSettings = config.Settings[settingsCategory];

                    connectionString = configSettings["ConnectionString"].Value;
                    dataProviderString = configSettings["DataProviderString"].Value;

                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new NullReferenceException("ConnectionString setting is not defined in the configuration file.");

                    if (string.IsNullOrWhiteSpace(dataProviderString))
                        throw new NullReferenceException("DataProviderString setting is not defined in the configuration file.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to load ADO database connection settings from configuration file: " + ex.Message, ex);
                }

                // Define connection settings without opening a connection
                configuredConnection = new AdoDataConnection(connectionString, dataProviderString, false);
                s_configuredConnections.TryAdd(settingsCategory, configuredConnection);
            }

            try
            {
                // Copy static instance data to member variables
                m_databaseType = configuredConnection.m_databaseType;
                m_connectionString = configuredConnection.m_connectionString;
                m_connectionType = configuredConnection.m_connectionType;
                m_adapterType = configuredConnection.m_adapterType;

                // Open ADO.NET provider connection
                m_connection = (IDbConnection)Activator.CreateInstance(m_connectionType);
                m_connection.ConnectionString = m_connectionString;
                m_connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open ADO data connection, verify \"ConnectionString\" in configuration file: " + ex.Message, ex);
            }
        }
        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> from specified <paramref name="connectionString"/> and <paramref name="dataProviderString"/>.
        /// </summary>
        /// <param name="connectionString">Database specific ADO connection string.</param>
        /// <param name="dataProviderString">Key/value pairs that define which ADO assembly and types to load.</param>
        public AdoDataConnection(string connectionString, string dataProviderString)
            : this(connectionString, dataProviderString, true)
        {
        }

        // Creates a new AdoDataConnection, optionally opening connection.
        private AdoDataConnection(string connectionString, string dataProviderString, bool openConnection)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString", "Parameter cannot be null or empty");

            if (string.IsNullOrWhiteSpace(dataProviderString))
                throw new ArgumentNullException("dataProviderString", "Parameter cannot be null or empty");

            // Cache connection string as member level variable
            m_connectionString = connectionString;

            try
            {
                // Attempt to load configuration from an ADO.NET database connection
                Dictionary<string, string> settings;
                string assemblyName, connectionTypeName, adapterTypeName;
                Assembly assembly;

                settings = dataProviderString.ParseKeyValuePairs();
                assemblyName = settings["AssemblyName"].ToNonNullString();
                connectionTypeName = settings["ConnectionType"].ToNonNullString();
                adapterTypeName = settings["AdapterType"].ToNonNullString();

                if (string.IsNullOrEmpty(connectionTypeName))
                    throw new NullReferenceException("ADO database connection type was undefined.");

                if (string.IsNullOrEmpty(adapterTypeName))
                    throw new NullReferenceException("ADO database adapter type was undefined.");

                assembly = Assembly.Load(new AssemblyName(assemblyName));
                m_connectionType = assembly.GetType(connectionTypeName);
                m_adapterType = assembly.GetType(adapterTypeName);
                m_databaseType = GetDatabaseType();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load ADO data provider, verify \"DataProviderString\": " + ex.Message, ex);
            }

            if (!openConnection)
                return;

            try
            {
                // Open ADO.NET provider connection
                m_connection = (IDbConnection)Activator.CreateInstance(m_connectionType);
                m_connection.ConnectionString = m_connectionString;
                m_connection.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open ADO data connection, verify \"ConnectionString\": " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdoDataConnection"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdoDataConnection()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets an open <see cref="IDbConnection"/> to configured ADO.NET data source.
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return m_connection;
            }
        }

        /// <summary>
        /// Gets the type of data adapter for configured ADO.NET data source.
        /// </summary>
        public Type AdapterType
        {
            get
            {
                return m_adapterType;
            }
        }

        /// <summary>
        /// Gets or sets the type of the database underlying the <see cref="AdoDataConnection"/>.
        /// </summary>
        /// <remarks>
        /// This value is automatically assigned based on the adapter type specified in the data provider string, however,
        /// if the database type cannot be determined it will be set to <see cref="GSF.Data.DatabaseType.Other"/>. In this
        /// case, if you know the behavior of your custom ADO database connection matches that of another defined database
        /// type, you can manually assign the database type to allow for database interaction interoperability.
        /// </remarks>
        public DatabaseType DatabaseType
        {
            get
            {
                return m_databaseType;
            }
            set
            {
                m_databaseType = value;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft Access.
        /// </summary>
        public bool IsJetEngine
        {
            get
            {
                return m_databaseType == DatabaseType.Access;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft SQL Server.
        /// </summary>
        public bool IsSQLServer
        {
            get
            {
                return m_databaseType == DatabaseType.SQLServer;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is MySQL.
        /// </summary>
        public bool IsMySQL
        {
            get
            {
                return m_databaseType == DatabaseType.MySQL;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Oracle.
        /// </summary>
        public bool IsOracle
        {
            get
            {
                return m_databaseType == DatabaseType.Oracle;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is SQLite.
        /// </summary>
        public bool IsSqlite
        {
            get
            {
                return m_databaseType == DatabaseType.SQLite;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdoDataConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdoDataConnection"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_connection != null)
                            m_connection.Dispose();
                        m_connection = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Returns proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="value"><see cref="System.Boolean"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Bool(bool value)
        {
            if (IsOracle)
                return value ? 1 : 0;

            return value;
        }

        /// <summary>
        /// Returns proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="guid"><see cref="System.Guid"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Guid(Guid guid)
        {
            if (IsJetEngine)
                return "{" + guid.ToString() + "}";

            if (IsOracle || IsSqlite)
                return guid.ToString().ToLower();

            //return "P" + guid.ToString();

            return guid;
        }

        /// <summary>
        /// Retrieves <see cref="System.Guid"/> from a <see cref="DataRow"/> field based on database type.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> from which value needs to be retrieved.</param>
        /// <param name="fieldName">Name of the field which contains <see cref="System.Guid"/>.</param>
        /// <returns><see cref="System.Guid"/>.</returns>
        public Guid Guid(DataRow row, string fieldName)
        {
            if (IsJetEngine || IsMySQL || IsOracle || IsSqlite)
                return System.Guid.Parse(row.Field<object>(fieldName).ToString());

            return row.Field<Guid>(fieldName);
        }

        /// <summary>
        /// Returns current UTC time in an implementation that is proper for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="usePrecisionTime">Set to <c>true</c> to use precision time.</param>
        /// <returns>Current UTC time in implementation that is proper for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object UtcNow(bool usePrecisionTime = false)
        {
            if (usePrecisionTime)
            {
                if (IsJetEngine)
                    return DateTime.UtcNow.ToOADate();

                return DateTime.UtcNow;
            }

            if (IsJetEngine)
                return DateTime.UtcNow.ToOADate();

            return DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a parameterized query string for the underlying database type based on the given format string and parameter names.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="parameterNames">A string array that contains zero or more parameter names to format.</param>
        /// <returns>A parameterized query string based on the given format and parameter names.</returns>
        public string ParameterizedQueryString(string format, params string[] parameterNames)
        {
            char paramChar = IsOracle ? ':' : '@';
            object[] parameters = parameterNames.Select(name => paramChar + name).Cast<object>().ToArray();
            return string.Format(format, parameters);
        }

        private DatabaseType GetDatabaseType()
        {
            DatabaseType type = DatabaseType.Other;

            if ((object)m_adapterType != null)
            {
                switch (m_adapterType.Name)
                {
                    case "SqlDataAdapter":
                        type = DatabaseType.SQLServer;
                        break;
                    case "MySqlDataAdapter":
                        type = DatabaseType.MySQL;
                        break;
                    case "OracleDataAdapter":
                        type = DatabaseType.Oracle;
                        break;
                    case "SQLiteDataAdapter":
                        type = DatabaseType.SQLite;
                        break;
                    case "OleDbDataAdapter":
                        if ((object)m_connectionString != null && m_connectionString.Contains("Microsoft.Jet.OLEDB"))
                            type = DatabaseType.Access;
                        break;
                }
            }

            return type;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, AdoDataConnection> s_configuredConnections;

        // Static Constructor
        static AdoDataConnection()
        {
            s_configuredConnections = new ConcurrentDictionary<string, AdoDataConnection>(StringComparer.InvariantCultureIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Forces a reload of cached configuration connection settings.
        /// </summary>
        public static void ReloadConfigurationSettings()
        {
            if ((object)s_configuredConnections != null)
                s_configuredConnections.Clear();
        }

        #endregion
    }
}
