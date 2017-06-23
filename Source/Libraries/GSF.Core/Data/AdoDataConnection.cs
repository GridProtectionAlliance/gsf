//******************************************************************************************************
//  AdoDataConnection.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using GSF.Annotations;
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
        /// Underlying ADO database type is PostgreSQL.
        /// </summary>
        PostgreSQL,

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
    ///         ConnectionString = "Data Source=databaseName.db; Version=3; Foreign Keys=True; FailIfMissing=True"<br/>
    ///         DataProviderString = "AssemblyName={System.Data.SQLite, Version=1.0.99.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter"
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>SQLite (Mono native driver)</term>
    ///         <description>
    ///         ConnectionString = "Data Source=./databaseName.db; Version=3; Foreign Keys=True; FailIfMissing=True"<br/>
    ///         DataProviderString = "AssemblyName={Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756}; ConnectionType=Mono.Data.Sqlite.SqliteConnection; AdapterType=Mono.Data.Sqlite.SqliteDataAdapter"
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
    ///     <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
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
        private readonly bool m_disposeConnection;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> based on connection settings in configuration file.
        /// </summary>
        /// <param name="settingsCategory">Settings category to use for connection settings.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public AdoDataConnection(string settingsCategory)
        {
            if (string.IsNullOrWhiteSpace(settingsCategory))
                throw new ArgumentNullException(nameof(settingsCategory), "Parameter cannot be null or empty");

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
                m_disposeConnection = true;

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

        /// <summary>
        /// Creates and opens a new <see cref="AdoDataConnection"/> from specified <paramref name="connectionString"/>,
        /// <paramref name="connectionType"/>, and <paramref name="adapterType"/>.
        /// </summary>
        /// <param name="connectionString">Database specific ADO connection string.</param>
        /// <param name="connectionType">The ADO type used to establish the database connection.</param>
        /// <param name="adapterType">The ADO type used to load data into <see cref="DataTable"/>s.</param>
        public AdoDataConnection(string connectionString, Type connectionType, Type adapterType)
        {
            if (!typeof(IDbConnection).IsAssignableFrom(connectionType))
                throw new ArgumentException("Connection type must implement the IDbConnection interface", nameof(connectionType));

            if (!typeof(IDbDataAdapter).IsAssignableFrom(adapterType))
                throw new ArgumentException("Adapter type must implement the IDbDataAdapter interface", nameof(adapterType));

            m_connectionString = connectionString;
            m_connectionType = connectionType;
            m_adapterType = adapterType;
            m_databaseType = GetDatabaseType();
            m_disposeConnection = true;

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
        /// Creates a new <see cref="AdoDataConnection"/> from specified <paramref name="connection"/>
        /// and <paramref name="adapterType"/>.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="adapterType">The ADO type used to load data into <see cref="DataTable"/>s.</param>
        /// <param name="disposeConnection">True if the database connection should be closed when the <see cref="AdoDataConnection"/> is disposed; false otherwise.</param>
        public AdoDataConnection(IDbConnection connection, Type adapterType, bool disposeConnection)
        {
            if (!typeof(IDbDataAdapter).IsAssignableFrom(adapterType))
                throw new ArgumentException("Adapter type must implement the IDbDataAdapter interface", nameof(adapterType));

            m_connection = connection;
            m_connectionString = connection.ConnectionString;
            m_connectionType = connection.GetType();
            m_adapterType = adapterType;
            m_databaseType = GetDatabaseType();
            m_disposeConnection = disposeConnection;
        }

        // Creates a new AdoDataConnection, optionally opening connection.
        private AdoDataConnection(string connectionString, string dataProviderString, bool openConnection)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Parameter cannot be null or empty");

            if (string.IsNullOrWhiteSpace(dataProviderString))
                throw new ArgumentNullException(nameof(dataProviderString), "Parameter cannot be null or empty");

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
                m_disposeConnection = true;
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
        public IDbConnection Connection => m_connection;

        /// <summary>
        /// Gets the type of data adapter for configured ADO.NET data source.
        /// </summary>
        public Type AdapterType => m_adapterType;

        /// <summary>
        /// Gets or sets the type of the database underlying the <see cref="AdoDataConnection"/>.
        /// </summary>
        /// <remarks>
        /// This value is automatically assigned based on the adapter type specified in the data provider string, however,
        /// if the database type cannot be determined it will be set to <see cref="Data.DatabaseType.Other"/>. In this
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
        /// Gets or sets default timeout for <see cref="AdoDataConnection"/> data operations.
        /// </summary>
        public int DefaultTimeout { get; set; } = DataExtensions.DefaultTimeoutDuration;

        /// <summary>
        /// Gets current UTC date-time in an implementation that is proper for the connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        public object UtcNow
        {
            get
            {
                if (IsJetEngine)
                    return DateTime.UtcNow.ToOADate();

                if (IsSqlite)
                    return new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified);

                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets the default <see cref="IsolationLevel"/> for the connected <see cref=" AdoDataConnection"/> database type.
        /// </summary>
        public IsolationLevel DefaultIsloationLevel
        {
            get
            {
                if (IsSQLServer)
                    return IsolationLevel.ReadUncommitted;

                return IsolationLevel.Unspecified;
            }
        }

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft Access.
        /// </summary>
        public bool IsJetEngine => m_databaseType == DatabaseType.Access;

        /// <summary>
        /// Gets a value to indicate whether source database is Microsoft SQL Server.
        /// </summary>
        public bool IsSQLServer => m_databaseType == DatabaseType.SQLServer;

        /// <summary>
        /// Gets a value to indicate whether source database is MySQL.
        /// </summary>
        public bool IsMySQL => m_databaseType == DatabaseType.MySQL;

        /// <summary>
        /// Gets a value to indicate whether source database is Oracle.
        /// </summary>
        public bool IsOracle => m_databaseType == DatabaseType.Oracle;

        /// <summary>
        /// Gets a value to indicate whether source database is SQLite.
        /// </summary>
        public bool IsSqlite => m_databaseType == DatabaseType.SQLite;

        /// <summary>
        /// Gets a value to indicate whether source database is PostgreSQL.
        /// </summary>
        public bool IsPostgreSQL => m_databaseType == DatabaseType.PostgreSQL;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes the given SQL script using <see cref="Connection"/>.
        /// </summary>
        /// <param name="scriptPath">The path to the SQL script.</param>
        public void ExecuteScript(string scriptPath)
        {
            using (TextReader scriptReader = File.OpenText(scriptPath))
            {
                ExecuteScript(scriptReader);
            }
        }

        /// <summary>
        /// Executes the given SQL script using <see cref="Connection"/>.
        /// </summary>
        /// <param name="scriptReader">The reader used to extract SQL statements to be executed.</param>
        public void ExecuteScript(TextReader scriptReader)
        {
            switch (m_databaseType)
            {
                case DatabaseType.SQLServer:
                    // For the standard way to execute a TSQL script, refer to the following.
                    //
                    // http://stackoverflow.com/questions/650098/how-to-execute-an-sql-script-file-using-c-sharp
                    //
                    // This solution was not used here because useLegacyV2RuntimeActivationPolicy
                    // needs to be added to the app.config file for it to work. This is not
                    // ideal for a core library since it has no control over an application's
                    // config, and it can make no assertion about the flag's suitability to the
                    // application. For more information, refer to the following.
                    //
                    // http://web.archive.org/web/20130128072944/http://www.marklio.com/marklio/PermaLink,guid,ecc34c3c-be44-4422-86b7-900900e451f9.aspx
                    m_connection.ExecuteTSQLScript(scriptReader);
                    break;

                case DatabaseType.Oracle:
                    m_connection.ExecuteOracleScript(scriptReader);
                    break;

                case DatabaseType.MySQL:
                    Type mySqlScriptType = m_connection.GetType().Assembly.GetType("MySql.Data.MySqlClient.MySqlScript");

                    if ((object)mySqlScriptType != null)
                    {
                        object executor = Activator.CreateInstance(mySqlScriptType, m_connection, scriptReader.ReadToEnd());
                        MethodInfo executeMethod = executor.GetType().GetMethod("Execute");
                        executeMethod.Invoke(executor, null);
                    }
                    else
                    {
                        m_connection.ExecuteMySQLScript(scriptReader);
                    }

                    break;

                default:
                    m_connection.ExecuteNonQuery(scriptReader.ReadToEnd());
                    break;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>The number of rows affected.</returns>
        [StringFormatMethod("sqlFormat")]
        public int ExecuteNonQuery(string sqlFormat, params object[] parameters)
        {
            return ExecuteNonQuery(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>The number of rows affected.</returns>
        [StringFormatMethod("sqlFormat")]
        public int ExecuteNonQuery(int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.ExecuteNonQuery(sql, timeout, ResolveParameters(parameters));
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public IDataReader ExecuteReader(string sqlFormat, params object[] parameters)
        {
            return ExecuteReader(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public IDataReader ExecuteReader(int timeout, string sqlFormat, params object[] parameters)
        {
            return ExecuteReader(CommandBehavior.Default, timeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public IDataReader ExecuteReader(CommandBehavior behavior, int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.ExecuteReader(sql, behavior, timeout, ResolveParameters(parameters));
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public T ExecuteScalar<T>(string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar<T>(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public T ExecuteScalar<T>(int timeout, string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar(default(T), timeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <typeparamref name="T"/>, substituting <paramref name="defaultValue"/>
        /// if <see cref="DBNull.Value"/> is retrieved.
        /// </summary>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public T ExecuteScalar<T>(T defaultValue, string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar(defaultValue, DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <typeparamref name="T"/>, substituting <paramref name="defaultValue"/>
        /// if <see cref="DBNull.Value"/> is retrieved.
        /// </summary>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public T ExecuteScalar<T>(T defaultValue, int timeout, string sqlFormat, params object[] parameters)
        {
            return (T)ExecuteScalar(typeof(T), defaultValue, timeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <paramref name="returnType"/>.
        /// </summary>
        /// <param name="returnType">The type to which the result of the query should be converted.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(Type returnType, string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar(returnType, DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <paramref name="returnType"/>.
        /// </summary>
        /// <param name="returnType">The type to which the result of the query should be converted.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(Type returnType, int timeout, string sqlFormat, params object[] parameters)
        {
            if (returnType.IsValueType)
                return ExecuteScalar(returnType, Activator.CreateInstance(returnType), timeout, sqlFormat, parameters);

            return ExecuteScalar(returnType, (object)null, timeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <paramref name="returnType"/>, substituting <paramref name="defaultValue"/>
        /// if <see cref="DBNull.Value"/> is retrieved.
        /// </summary>
        /// <param name="returnType">The type to which the result of the query should be converted.</param>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(Type returnType, object defaultValue, string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar(returnType, defaultValue, DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set as type <paramref name="returnType"/>, substituting <paramref name="defaultValue"/>
        /// if <see cref="DBNull.Value"/> is retrieved.
        /// </summary>
        /// <param name="returnType">The type to which the result of the query should be converted.</param>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(Type returnType, object defaultValue, int timeout, string sqlFormat, params object[] parameters)
        {
            object value = ExecuteScalar(timeout, sqlFormat, parameters);

            // It's important that we do not validate the default value to determine
            // whether it is assignable to the return type because this method is
            // sometimes used to return null for value types in default value
            // expressions where nullable types are not supported
            if ((object)value == null || value == DBNull.Value)
                return defaultValue;

            // Nullable types cannot be used in type conversion, but we can use Nullable.GetUnderlyingType()
            // to determine whether the type is nullable and convert to the underlying type instead
            Type type = Nullable.GetUnderlyingType(returnType) ?? returnType;

            // Handle Guids
            if (type == typeof(Guid))
                return System.Guid.Parse(value.ToString());

            // Handle string types that may have a converter function (e.g., Enums)
            if (value is string)
                return value.ToString().ConvertToType(type);

            // Handle native types
            if (value is IConvertible)
                return Convert.ChangeType(value, type);

            return value;
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(string sqlFormat, params object[] parameters)
        {
            return ExecuteScalar(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public object ExecuteScalar(int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.ExecuteScalar(sql, timeout, ResolveParameters(parameters));
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataRow RetrieveRow(string sqlFormat, params object[] parameters)
        {
            return RetrieveRow(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataRow RetrieveRow(int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.RetrieveRow(m_adapterType, sql, timeout, ResolveParameters(parameters));
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains at least one table.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataTable RetrieveData(string sqlFormat, params object[] parameters)
        {
            return RetrieveData(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains at least one table.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataTable RetrieveData(int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.RetrieveData(m_adapterType, sql, timeout, ResolveParameters(parameters));
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataSet RetrieveDataSet(string sqlFormat, params object[] parameters)
        {
            return RetrieveDataSet(DefaultTimeout, sqlFormat, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        [StringFormatMethod("sqlFormat")]
        public DataSet RetrieveDataSet(int timeout, string sqlFormat, params object[] parameters)
        {
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            return m_connection.RetrieveDataSet(m_adapterType, sql, timeout, ResolveParameters(parameters));
        }

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
                        if (m_disposeConnection && (object)m_connection != null)
                        {
                            m_connection.Dispose();
                            m_connection = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Escapes an identifier, e.g., a table or field name, using the common delimiter for the connected <see cref="AdoDataConnection"/>
        /// database type or the standard ANSI escaping delimiter, i.e., double-quotes, based on the <paramref name="useAnsiQuotes"/> flag.
        /// </summary>
        /// <param name="identifier">Field name to escape.</param>
        /// <param name="useAnsiQuotes">Force use of double-quote for identifier delimiter, per SQL-99 standard, regardless of database type.</param>
        /// <returns>Escaped identifier name.</returns>
        /// <exception cref="ArgumentException"><paramref name="identifier"/> value cannot be null, empty or whitespace.</exception>
        public string EscapeIdentifier(string identifier, bool useAnsiQuotes = false)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Value cannot be null, empty or whitespace.", nameof(identifier));

            identifier = identifier.Trim();

            if (useAnsiQuotes)
                return $"\"{identifier}\"";

            switch (m_databaseType)
            {
                case DatabaseType.SQLServer:
                case DatabaseType.Access:
                    return $"[{identifier}]";
                case DatabaseType.MySQL:
                    return $"`{identifier}`";
                default:
                    return $"\"{identifier}\"";
            }
        }

        /// <summary>
        /// Returns proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="value"><see cref="System.Boolean"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Boolean"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Bool(bool value)
        {
            if (IsOracle || IsPostgreSQL)
                return value ? 1 : 0;

            return value;
        }

        /// <summary>
        /// Returns proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.
        /// </summary>
        /// <param name="value"><see cref="System.Guid"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Guid(Guid value)
        {
            if (IsJetEngine)
                return "{" + value + "}";

            if (IsSqlite || IsOracle || IsPostgreSQL)
                return value.ToString().ToLower();

            //return "P" + guid.ToString();

            return value;
        }

        /// <summary>
        /// Retrieves <see cref="System.Guid"/> from a <see cref="DataRow"/> field based on database type.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> from which value needs to be retrieved.</param>
        /// <param name="fieldName">Name of the field which contains <see cref="System.Guid"/>.</param>
        /// <returns><see cref="System.Guid"/>.</returns>
        public Guid Guid(DataRow row, string fieldName)
        {
            if (IsSQLServer)
                return row.Field<Guid>(fieldName);

            return System.Guid.Parse(row.Field<object>(fieldName).ToString());
        }

        /// <summary>
        /// Creates a parameterized query string for the underlying database type based on the given format string and parameter names.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="parameterNames">A string array that contains zero or more parameter names to format.</param>
        /// <returns>A parameterized query string based on the given format and parameter names.</returns>
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ParameterizedQueryString(string format, params string[] parameterNames)
        {
            char paramChar = IsOracle ? ':' : '@';
            object[] parameters = parameterNames.Select(name => paramChar + name).Cast<object>().ToArray();
            return string.Format(format, parameters);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private DatabaseType GetDatabaseType()
        {
            DatabaseType type = DatabaseType.Other;

            if ((object)m_adapterType != null)
            {
                switch (m_adapterType.Name.ToLowerInvariant())
                {
                    case "sqldataadapter":
                        type = DatabaseType.SQLServer;
                        break;
                    case "mysqldataadapter":
                        type = DatabaseType.MySQL;
                        break;
                    case "oracledataadapter":
                        type = DatabaseType.Oracle;
                        break;
                    case "sqlitedataadapter":
                        type = DatabaseType.SQLite;
                        break;
                    case "npgsqldataadapter":
                        type = DatabaseType.PostgreSQL;
                        break;
                    case "oledbdataadapter":
                        if ((object)m_connectionString != null && m_connectionString.ToLowerInvariant().Contains("microsoft.jet.oledb"))
                            type = DatabaseType.Access;
                        break;
                }
            }

            return type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GenericParameterizedQueryString(string sqlFormat, object[] parameters)
        {
            string[] parameterNames = parameters.Select((parameter, index) => "p" + index).ToArray();
            return ParameterizedQueryString(sqlFormat, parameterNames);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object[] ResolveParameters(object[] parameters)
        {
            IDbDataParameter[] dataParameters = new IDbDataParameter[parameters.Length];

            if (parameters.Length > 0)
            {
                using (IDbCommand command = m_connection.CreateCommand())
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        object value = parameters[i];
                        DbType? type = null;

                        if (value is IDbDataParameter)
                        {
                            IDbDataParameter intermediateParameter = value as IDbDataParameter;
                            type = intermediateParameter.DbType;
                            value = intermediateParameter.Value;
                        }

                        if (value == null)
                            value = DBNull.Value;
                        else if (value is bool)
                            value = Bool((bool)value);
                        else if (value is Guid)
                            value = Guid((Guid)value);

                        IDbDataParameter parameter = command.CreateParameter();

                        if (type.HasValue)
                            parameter.DbType = type.Value;

                        parameter.ParameterName = "@p" + i;
                        parameter.Value = value;
                        dataParameters[i] = parameter;
                    }
                }
            }

            // ReSharper disable once CoVariantArrayConversion
            return dataParameters;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, AdoDataConnection> s_configuredConnections;

        // Static Constructor
        static AdoDataConnection()
        {
            s_configuredConnections = new ConcurrentDictionary<string, AdoDataConnection>(StringComparer.OrdinalIgnoreCase);
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

        /// <summary>
        /// Generates a data provider string for the given connection type and adapter type.
        /// </summary>
        /// <param name="connectionType">The type used to establish a connection to the database.</param>
        /// <param name="adapterType">The type used to load data from the database into a data table.</param>
        /// <returns></returns>
        public static string ToDataProviderString(Type connectionType, Type adapterType)
        {
            if (!typeof(IDbConnection).IsAssignableFrom(connectionType))
                throw new ArgumentException("Connection type must implement the IDbConnection interface", nameof(connectionType));

            if (!typeof(IDbDataAdapter).IsAssignableFrom(adapterType))
                throw new ArgumentException("Adapter type must implement the IDbDataAdapter interface", nameof(adapterType));

            if (connectionType.Assembly != adapterType.Assembly)
                throw new InvalidOperationException("Data provider string requires that connection type and adapter type reside in the same assembly");

            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                ["AssemblyName"] = connectionType.Assembly.FullName,
                ["ConnectionType"] = connectionType.FullName,
                ["AdapterType"] = adapterType.FullName
            };

            return settings.JoinKeyValuePairs();
        }

        #endregion
    }
}
