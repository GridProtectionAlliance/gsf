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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
    ///         DataProviderString = "AssemblyName={System.Data.SQLite, Version=1.0.93.0, Culture=neutral, PublicKeyToken=db937bc2d44ff139}; ConnectionType=System.Data.SQLite.SQLiteConnection; AdapterType=System.Data.SQLite.SQLiteDataAdapter"
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
        /// Executes the SQL statement using <see cref="Connection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlFormat">Format string for the SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters.</param>
        /// <returns>The number of rows affected.</returns>
        [StringFormatMethod("sqlFormat")]
        public int ExecuteNonQuery(string sqlFormat, params object[] parameters)
        {
            return ExecuteNonQuery(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            FixParameters(parameters);
            return m_connection.ExecuteNonQuery(sql, timeout, parameters);
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
            return ExecuteReader(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            string sql = GenericParameterizedQueryString(sqlFormat, parameters);
            FixParameters(parameters);
            return m_connection.ExecuteReader(sql, timeout, parameters);
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
            return ExecuteScalar<T>(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            return ExecuteScalar(defaultValue, DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            object value = ExecuteScalar(timeout, sqlFormat, parameters);

            if (value == DBNull.Value)
                return defaultValue;

            return (T)Convert.ChangeType(value, typeof(T));
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
            return ExecuteScalar(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            FixParameters(parameters);
            return m_connection.ExecuteScalar(sql, timeout, parameters);
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
            return RetrieveRow(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            FixParameters(parameters);
            return m_connection.RetrieveRow(m_adapterType, sql, timeout, parameters);
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
            return RetrieveData(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            FixParameters(parameters);
            return m_connection.RetrieveData(m_adapterType, sql, timeout, parameters);
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
            return RetrieveDataSet(DataExtensions.DefaultTimeoutDuration, sqlFormat, parameters);
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
            FixParameters(parameters);
            return m_connection.RetrieveDataSet(m_adapterType, sql, timeout, parameters);
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
        /// <param name="value"><see cref="System.Guid"/> to format per database type.</param>
        /// <returns>Proper <see cref="System.Guid"/> implementation for connected <see cref="AdoDataConnection"/> database type.</returns>
        public object Guid(Guid value)
        {
            if (IsJetEngine)
                return "{" + value + "}";

            if (IsSqlite || IsOracle)
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
                    case "oledbdataadapter":
                        if ((object)m_connectionString != null && m_connectionString.ToLowerInvariant().Contains("microsoft.jet.oledb"))
                            type = DatabaseType.Access;
                        break;
                }
            }

            return type;
        }

        private string GenericParameterizedQueryString(string sqlFormat, object[] parameters)
        {
            string[] parameterNames = parameters
                .Select((parameter, index) => "p" + index)
                .ToArray();

            return ParameterizedQueryString(sqlFormat, parameterNames);
        }

        private void FixParameters(object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                    parameters[i] = DBNull.Value;
                else if (parameters[i] is bool)
                    parameters[i] = Bool((bool)parameters[i]);
                else if (parameters[i] is Guid)
                    parameters[i] = Guid((Guid)parameters[i]);
            }
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

        #endregion
    }
}
