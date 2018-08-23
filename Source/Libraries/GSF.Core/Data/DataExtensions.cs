//******************************************************************************************************
//  DataExtensions.cs - Gbtc
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
//  02/05/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  05/25/2004 - J. Ritchie Carroll
//       Added "with parameters" overloads to all basic query functions.
//  12/10/2004 - Tim M Shults
//       Added several new WithParameters overloads that allow a programmer to send just the
//       parameter values instead of creating a series of parameter objects and then sending
//       them through. Easy way to cut down on the amount of code.
//       This code is just for calls to Stored Procedures and will not work for in-line SQL.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (GSF.Database.Common).
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - J. Ritchie Carroll
//       Converted to C# extensions.
//  09/29/2008 - Pinal C. Patel
//       Reviewed code comments.
//  09/09/2009 - J. Ritchie Carroll
//       Added extensions for ODBC providers.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/02/2009 - Stephen C. Wills
//       Added disposal of database command objects.
//  09/28/2010 - J. Ritchie Carroll
//       Added Stephen's CreateParameterizedCommand connection extension.
//  04/07/2011 - J. Ritchie Carroll
//       Added Mehul's AddParameterWithValue command extension. Added overloads for all
//       PopulateParameters() command extensions so that they could take a "params" style
//       array of values after initial value for ease-of-use. Added "params" style array
//       to all templated IDbConnection that will use the CreateParameterizedCommand
//       connection extension with optional parameters.
//  06/16/2011 - Pinal C. Patel
//       Modified AddParameterWithValue() to be backwards compatible.
//  07/18/2011 - Stephen C. Wills
//       Added DataRow extension functions to automatically convert from types that
//       implement the IConvertible interface.
//  08/12/2011 - Pinal C. Patel
//       Modified AddParameterWithValue() to correctly implement backwards compatible.
//  09/19/2011 - Stephen C. Wills
//       Modified AddParametersWithValues() to parse parameters prefixed
//       with a colon for Oracle database compatibility.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace GSF.Data
{
    // ReSharper disable UnusedVariable

    /// <summary>
    /// Defines extension functions related to database and SQL interaction.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// The default timeout duration used for executing SQL statements when timeout duration is not specified.
        /// </summary>
        public const int DefaultTimeoutDuration = 30;

        #region [ SQL Encoding String Extension ]

        /// <summary>
        /// Performs SQL encoding on given SQL string.
        /// </summary>
        /// <param name="sql">The string on which SQL encoding is to be performed.</param>
        /// <param name="databaseType">Database type for the SQL encoding.</param>
        /// <returns>The SQL encoded string.</returns>
        public static string SQLEncode(this string sql, DatabaseType databaseType = DatabaseType.Other)
        {
            if (databaseType == DatabaseType.MySQL)
                return sql.Replace("\\", "\\\\").Replace("\'", "\\\'");

            return sql.Replace("\'", "\'\'"); //.Replace("/*", "").Replace("--", "");
        }

        #endregion

        #region [ ExecuteNonQuery Overloaded Extension ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql, params object[] parameters) where TConnection : IDbConnection
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql, int timeout, params object[] parameters) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateParameterizedCommand(sql, parameters))
            {
                command.CommandTimeout = timeout;
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int ExecuteNonQuery(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this IDbCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.AddParametersWithValues(sql, parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteNonQuery();
        }

        #endregion

        #region [ ExecuteReader Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, params object[] parameters) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, int timeout, params object[] parameters) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateParameterizedCommand(sql, parameters))
            {
                command.CommandTimeout = timeout;
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static OdbcDataReader ExecuteReader(this OdbcConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteReader(behavior);
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        public static IDataReader ExecuteReader(this IDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        public static IDataReader ExecuteReader(this IDbCommand command, string sql, int timeout, params object[] parameters)
        {
            return command.ExecuteReader(sql, CommandBehavior.Default, timeout, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        public static IDataReader ExecuteReader(this IDbCommand command, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.AddParametersWithValues(sql, parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbCommand command, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcCommand command, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlCommand command, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        #endregion

        #region [ ExecuteScalar Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql, params object[] parameters) where TConnection : IDbConnection
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql, int timeout, params object[] parameters) where TConnection : IDbConnection
        {
            using (IDbCommand command = connection.CreateParameterizedCommand(sql, parameters))
            {
                command.CommandTimeout = timeout;
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object ExecuteScalar(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object ExecuteScalar(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object ExecuteScalar(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this IDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this IDbCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.AddParametersWithValues(sql, parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OleDbCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this OdbcCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the value in the first column 
        /// of the first row in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the result set.</returns>
        public static object ExecuteScalar(this SqlCommand command, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        #endregion

        #region [ ExecuteScript Overloaded Extensions ]

        /// <summary>
        /// Executes the statements defined in the given TSQL script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptPath">The path to the SQL script.</param>
        public static void ExecuteTSQLScript(this IDbConnection connection, string scriptPath)
        {
            using (TextReader scriptReader = File.OpenText(scriptPath))
            {
                ExecuteTSQLScript(connection, scriptReader);
            }
        }

        /// <summary>
        /// Executes the statements defined in the given TSQL script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptReader">The reader used to extract statements from the SQL script.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "CommandText updates are being removed, not accepting user input")]
        public static void ExecuteTSQLScript(this IDbConnection connection, TextReader scriptReader)
        {
            string line = scriptReader.ReadLine();

            using (IDbCommand command = connection.CreateCommand())
            {
                StringBuilder statementBuilder = new StringBuilder();
                Regex comment = new Regex(@"/\*.*\*/|--.*(?=\n)", RegexOptions.Multiline);

                while ((object)line != null)
                {
                    string trimLine = line.Trim();
                    string statement;

                    if (trimLine == "GO")
                    {
                        // Remove comments and execute the statement.
                        statement = statementBuilder.ToString();
                        command.CommandText = comment.Replace(statement, " ").Trim();
                        command.ExecuteNonQuery();
                        statementBuilder.Clear();
                    }
                    else
                    {
                        // Append this line to the statement
                        statementBuilder.Append(line);
                        statementBuilder.Append('\n');
                    }

                    // Read the next line from the file.
                    line = scriptReader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Executes the statements defined in the given MySQL script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptPath">The path to the SQL script.</param>
        public static void ExecuteMySQLScript(this IDbConnection connection, string scriptPath)
        {
            using (TextReader scriptReader = File.OpenText(scriptPath))
            {
                ExecuteMySQLScript(connection, scriptReader);
            }
        }

        /// <summary>
        /// Executes the statements defined in the given MySQL script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptReader">The reader used to extract statements from the SQL script.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "CommandText updates are being removed, not accepting user input")]
        public static void ExecuteMySQLScript(this IDbConnection connection, TextReader scriptReader)
        {
            string line;
            string delimiter;

            line = scriptReader.ReadLine();
            delimiter = ";";

            using (IDbCommand command = connection.CreateCommand())
            {
                StringBuilder statementBuilder = new StringBuilder();
                Regex comment = new Regex(@"/\*.*\*/|--.*(?=\n)", RegexOptions.Multiline);

                while ((object)line != null)
                {
                    string statement;

                    if (line.StartsWith("DELIMITER ", StringComparison.OrdinalIgnoreCase))
                    {
                        delimiter = line.Split(' ')[1].Trim();
                    }
                    else
                    {
                        statementBuilder.Append(line);
                        statementBuilder.Append('\n');
                        statement = statementBuilder.ToString();
                        statement = comment.Replace(statement, " ").Trim();

                        if (statement.EndsWith(delimiter, StringComparison.Ordinal))
                        {
                            // Remove trailing delimiter.
                            statement = statement.Remove(statement.Length - delimiter.Length);

                            // Remove comments and execute the statement.
                            command.CommandText = statement;
                            command.ExecuteNonQuery();
                            statementBuilder.Clear();
                        }
                    }

                    // Read the next line from the file.
                    line = scriptReader.ReadLine();
                }
            }
        }

        /// <summary>
        /// Executes the statements defined in the given Oracle database script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptPath">The path to the SQL script.</param>
        public static void ExecuteOracleScript(this IDbConnection connection, string scriptPath)
        {
            using (TextReader scriptReader = File.OpenText(scriptPath))
            {
                ExecuteOracleScript(connection, scriptReader);
            }
        }

        /// <summary>
        /// Executes the statements defined in the given Oracle database script.
        /// </summary>
        /// <param name="connection">The connection used to execute SQL statements.</param>
        /// <param name="scriptReader">The reader used to extract statements from the SQL script.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "CommandText updates are being removed, not accepting user input")]
        public static void ExecuteOracleScript(this IDbConnection connection, TextReader scriptReader)
        {
            string line;

            line = scriptReader.ReadLine();

            using (IDbCommand command = connection.CreateCommand())
            {
                StringBuilder statementBuilder = new StringBuilder();
                Regex comment = new Regex(@"/\*.*\*/|--.*(?=\n)", RegexOptions.Multiline);

                while ((object)line != null)
                {
                    string trimLine = line.Trim();
                    string statement;
                    bool isPlsqlBlock;

                    statementBuilder.Append(line);
                    statementBuilder.Append('\n');
                    statement = statementBuilder.ToString();
                    statement = comment.Replace(statement, " ").Trim();

                    // Determine whether the statement is a PL/SQL block.
                    // If the statement is a PL/SQL block, the delimiter
                    // is a forward slash. Otherwise, it is a semicolon.
                    isPlsqlBlock = s_plsqlIdentifiers.Any(ident => statement.IndexOf(ident, StringComparison.CurrentCultureIgnoreCase) >= 0);

                    // If the statement is a PL/SQL block and the current line is a forward slash,
                    // or if the statement is not a PL/SQL block and the statement in a semicolon,
                    // then execute and flush the statement so that the next statement can be executed.
                    if ((isPlsqlBlock && trimLine == "/") || (!isPlsqlBlock && statement.EndsWith(";", StringComparison.Ordinal)))
                    {
                        // Remove trailing delimiter and newlines.
                        statement = statement.Remove(statement.Length - 1);

                        // Remove comments and execute the statement.
                        command.CommandText = statement;
                        command.ExecuteNonQuery();
                        statementBuilder.Clear();
                    }

                    // Read the next line from the file.
                    line = scriptReader.ReadLine();
                }
            }
        }

        // Defines a list of keywords used to identify PL/SQL blocks.
        private static readonly string[] s_plsqlIdentifiers = { "CREATE FUNCTION", "CREATE OR REPLACE FUNCTION",
                                                                "CREATE PROCEDURE", "CREATE OR REPLACE PROCEDURE",
                                                                "CREATE PACKAGE", "CREATE OR REPLACE PACKAGE",
                                                                "DECLARE", "BEGIN" };

        #endregion

        #region [ RetrieveRow Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(dataAdapterType, sql, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbCommand command, string sql)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbCommand command, string sql, int timeout)
        {
            return command.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OleDbCommand command, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = command.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcCommand command, string sql)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcCommand command, string sql, int timeout)
        {
            return command.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this OdbcCommand command, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = command.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlCommand command, string sql)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlCommand command, string sql, int timeout)
        {
            return command.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this SqlCommand command, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = command.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this IDbCommand command, Type dataAdapterType, string sql, params object[] parameters)
        {
            return command.RetrieveRow(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the first <see cref="DataRow"/> in the result set.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        public static DataRow RetrieveRow(this IDbCommand command, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = command.RetrieveData(dataAdapterType, sql, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        #endregion

        #region [ RetrieveData Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql, params object[] parameters)
        {
            return connection.RetrieveData(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbCommand command, string sql)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcCommand command, string sql)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlCommand command, string sql)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbCommand command, Type dataAdapterType, string sql, params object[] parameters)
        {
            return command.RetrieveData(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the first <see cref="DataTable"/> 
        /// of result set, if the result set contains multiple tables.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbCommand command, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            return command.RetrieveDataSet(dataAdapterType, sql, timeout, parameters).Tables[0];
        }

        #endregion

        #region [ RetrieveDataSet Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            {
                command.PopulateParameters(parameters);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (OdbcCommand command = new OdbcCommand(sql, connection))
            {
                command.PopulateParameters(parameters);
                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple table depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = timeout;
                command.PopulateParameters(parameters);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            using (IDbCommand command = connection.CreateParameterizedCommand(sql, parameters))
            {
                command.CommandTimeout = timeout;
                IDataAdapter dataAdapter = (IDataAdapter)Activator.CreateInstance(dataAdapterType, command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data);

                return data;
            }
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbCommand command, string sql)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OleDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcCommand command, string sql)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="OdbcCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple table depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlCommand command, string sql)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlCommand command, string sql, int startRow, int maxRows, int timeout)
        {
            return command.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlCommand command, string sql, params object[] parameters)
        {
            return command.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="command">The <see cref="SqlCommand"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlCommand command, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.PopulateParameters(parameters);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbCommand command, Type dataAdapterType, string sql, params object[] parameters)
        {
            return command.RetrieveDataSet(dataAdapterType, sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbCommand"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbCommand command, Type dataAdapterType, string sql, int timeout, params object[] parameters)
        {
            command.CommandTimeout = timeout;
            command.Parameters.Clear();
            command.AddParametersWithValues(sql, parameters);
            IDataAdapter dataAdapter = (IDataAdapter)Activator.CreateInstance(dataAdapterType, command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data);

            return data;
        }

        #endregion

        #region [ DataRow Extensions ]

        /// <summary>
        /// Provides strongly-typed access to each of the column values in the specified row.
        /// Automatically applies type conversion to the column values.
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <returns>The value, of type T, of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static T ConvertField<T>(this DataRow row, string field)
        {
            return ConvertField(row, field, default(T));
        }

        /// <summary>
        /// Provides strongly-typed access to each of the column values in the specified row.
        /// Automatically applies type conversion to the column values.
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <returns>The value, of type T, of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static T ConvertField<T>(this DataRow row, string field, T defaultValue)
        {
            object value = row.Field<object>(field);

            if (value == null || value == DBNull.Value)
                return defaultValue;

            // If the value is an instance of the given type,
            // no type conversion is necessary
            if (value is T)
                return (T)value;

            Type type = typeof(T);

            // Nullable types cannot be used in type conversion, but we can use Nullable.GetUnderlyingType()
            // to determine whether the type is nullable and convert to the underlying type instead
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Handle Guids as a special case since they do not implement IConvertible
            if (underlyingType == typeof(Guid))
                return (T)(object)Guid.Parse(value.ToString());

            // Handle enums as a special case since they do not implement IConvertible
            if (underlyingType.IsEnum)
                return (T)Enum.Parse(underlyingType, value.ToString());

            return (T)Convert.ChangeType(value, underlyingType);
        }

        /// <summary>
        /// Automatically applies type conversion to column values when only a type is available.
        /// </summary>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <param name="type">Type of the column.</param>
        /// <returns>The value of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static object ConvertField(this DataRow row, string field, Type type)
        {
            return ConvertField(row, field, type, null);
        }

        /// <summary>
        /// Automatically applies type conversion to column values when only a type is available.
        /// </summary>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <param name="type">Type of the column.</param>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved.</param>
        /// <returns>The value of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static object ConvertField(this DataRow row, string field, Type type, object defaultValue)
        {
            object value = row.Field<object>(field);

            if (value == null || value == DBNull.Value)
                return defaultValue ?? (type.IsValueType ? Activator.CreateInstance(type) : null);

            // If the value is an instance of the given type,
            // no type conversion is necessary
            if (type.IsInstanceOfType(value))
                return value;

            // Nullable types cannot be used in type conversion, but we can use Nullable.GetUnderlyingType()
            // to determine whether the type is nullable and convert to the underlying type instead
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Handle Guids as a special case since they do not implement IConvertible
            if (underlyingType == typeof(Guid))
                return Guid.Parse(value.ToString());

            // Handle enums as a special case since they do not implement IConvertible
            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value.ToString());

            return Convert.ChangeType(value, underlyingType);
        }

        /// <summary>
        /// Provides strongly-typed access to each of the column values in the specified row.
        /// Automatically applies type conversion to the column values.
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <returns>The value, of type T, of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static T? ConvertNullableField<T>(this DataRow row, string field) where T : struct
        {
            object value = row.Field<object>(field);

            if (value == null)
                return null;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Parses a Guid from a database field that is a Guid type or a string representing a Guid.
        /// </summary>
        /// <param name="row">The input <see cref="DataRow"/>, which acts as the this instance for the extension method.</param>
        /// <param name="field">The name of the column to return the value of.</param>
        /// <param name="defaultValue">The value to be substituted if <see cref="DBNull.Value"/> is retrieved; defaults to <see cref="Guid.Empty"/>.</param>
        /// <returns>The <see cref="Guid"/> value of the <see cref="DataColumn"/> specified by <paramref name="field"/>.</returns>
        public static Guid ConvertGuidField(this DataRow row, string field, Guid? defaultValue = null)
        {
            object value = row.Field<object>(field);

            if (value == null || value == DBNull.Value)
                return defaultValue ?? Guid.Empty;

            if (value is Guid)
                return (Guid)value;

            return Guid.Parse(value.ToString());
        }

        #endregion

        #region [ UpdateData Overloaded Functions ]

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OleDbConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "commandBuilder"), SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults"), SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int UpdateData(this OleDbConnection connection, DataTable sourceData, string sourceSql)
        {
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(sourceSql, connection);
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OdbcConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "commandBuilder"), SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults"), SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int UpdateData(this OdbcConnection connection, DataTable sourceData, string sourceSql)
        {
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(sourceSql, connection);
            OdbcCommandBuilder commandBuilder = new OdbcCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the <see cref="DataTable"/> using <see cref="SqlConnection"/>, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "commandBuilder"), SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults"), SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int UpdateData(this SqlConnection connection, DataTable sourceData, string sourceSql)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sourceSql, connection);
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        #endregion

        #region [ Command Parameter Population Functions ]

        /// <summary>
        /// Takes the <see cref="OleDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OleDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameter1">The first parameter value to populate the <see cref="OleDbCommand"/> parameters with.</param>
        /// <param name="parameters">The remaining parameter values to populate the <see cref="OleDbCommand"/> parameters with.</param>
        public static void PopulateParameters(this OleDbCommand command, object parameter1, params object[] parameters)
        {
            command.PopulateParameters((new[] { parameter1 }).Concat(parameters).ToArray());
        }

        /// <summary>
        /// Takes the <see cref="OleDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OleDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameter values to populate the <see cref="OleDbCommand"/> parameters with.</param>
        /// <remarks>
        /// Automatic parameter derivation is currently not support for OleDB connections under Mono deployments.
        /// </remarks>
        public static void PopulateParameters(this OleDbCommand command, object[] parameters)
        {
#if !MONO
            command.PopulateParameters(OleDbCommandBuilder.DeriveParameters, parameters);
#endif
        }

        /// <summary>
        /// Takes the <see cref="OdbcCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OdbcCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameter1">The first parameter value to populate the <see cref="OdbcCommand"/> parameters with.</param>
        /// <param name="parameters">The remaining parameter values to populate the <see cref="OdbcCommand"/> parameters with.</param>
        public static void PopulateParameters(this OdbcCommand command, object parameter1, params object[] parameters)
        {
            command.PopulateParameters((new[] { parameter1 }).Concat(parameters).ToArray());
        }

        /// <summary>
        /// Takes the <see cref="OdbcCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OdbcCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameter values to populate the <see cref="OdbcCommand"/> parameters with.</param>
        /// <remarks>
        /// Automatic parameter derivation is currently not support for ODBC connections under Mono deployments.
        /// </remarks>
        public static void PopulateParameters(this OdbcCommand command, object[] parameters)
        {
#if !MONO
            command.PopulateParameters(OdbcCommandBuilder.DeriveParameters, parameters);
#endif
        }

        /// <summary>
        /// Takes the <see cref="SqlCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="SqlCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameter1">The first parameter value to populate the <see cref="SqlCommand"/> parameters with.</param>
        /// <param name="parameters">The remaining parameter values to populate the <see cref="SqlCommand"/> parameters with.</param>
        public static void PopulateParameters(this SqlCommand command, object parameter1, params object[] parameters)
        {
            command.PopulateParameters((new[] { parameter1 }).Concat(parameters).ToArray());
        }

        /// <summary>
        ///  Takes the <see cref="SqlCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="SqlCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameter values to populate the <see cref="SqlCommand"/> parameters with.</param>
        public static void PopulateParameters(this SqlCommand command, object[] parameters)
        {
            command.PopulateParameters(SqlCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        /// Takes the <see cref="IDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="deriveParameters">The DeriveParameters() implementation of the <paramref name="command"/> to use to populate parameters.</param>
        /// <param name="values">The parameter values to populate the <see cref="IDbCommand"/> parameters with.</param>
        /// <typeparam name="TDbCommand">Then <see cref="IDbCommand"/> type to be used.</typeparam>
        /// <exception cref="ArgumentException">
        /// Number of <see cref="IDbDataParameter"/> arguments in <see cref="IDbCommand.CommandText"/> of this <paramref name="command"/>, identified by '@', do not match number of supplied parameter <paramref name="values"/> -or-
        /// You have supplied more <paramref name="values"/> than parameters listed for the stored procedure.
        /// </exception>
        public static void PopulateParameters<TDbCommand>(this TDbCommand command, Action<TDbCommand> deriveParameters, object[] values) where TDbCommand : IDbCommand
        {
            // tmshults 12/10/2004
            if ((object)values != null)
            {
                string commandText = command.CommandText;

                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException(nameof(command), "command.CommandText is null");

                // Add parameters for standard SQL expressions (i.e., non stored procedure expressions)
                if (!IsStoredProcedure(commandText))
                {
                    command.AddParametersWithValues(commandText, values);
                    return;
                }

                command.CommandType = CommandType.StoredProcedure;

                // Makes quick query to db to find the parameters for the StoredProc, and then creates them for
                // the command. The DeriveParameters() is only for commands with CommandType of StoredProcedure.
                deriveParameters(command);

                // Removes the ReturnValue Parameter.
                command.Parameters.RemoveAt(0);

                // Checks to see if the Parameters found match the Values provided.
                if (command.Parameters.Count != values.Length)
                {
                    // If there are more values than parameters, throws an error.
                    if (values.Length > command.Parameters.Count)
                        throw new ArgumentException("You have supplied more values than parameters listed for the stored procedure");

                    // Otherwise, assume that the missing values are for Parameters that have default values,
                    // and the code uses the default. To do this fill the extended ParamValue as Nothing/Null.
                    Array.Resize(ref values, command.Parameters.Count); // Makes the Values array match the Parameters of the Stored Proc.
                }

                // Assigns the values to the the Parameters.
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    ((DbParameter)command.Parameters[i]).Value = values[i];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsStoredProcedure(string sql)
        {
            sql = sql.TrimStart();

            // Check for common SQL command
            return !sql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase) && 
                   !sql.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase) && 
                   !sql.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase) && 
                   !sql.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates and adds an <see cref="IDbDataParameter"/> to the <see cref="IDbCommand"/> object with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="command"><see cref="IDbCommand"/> to which parameter needs to be added.</param>
        /// <param name="name">Name of the <see cref="IDbDataParameter"/> to be added.</param>
        /// <param name="value">Value of the <see cref="IDbDataParameter"/> to be added.</param>
        /// <param name="direction"><see cref="ParameterDirection"/> for <see cref="IDbDataParameter"/>.</param>
        public static void AddParameterWithValue(this IDbCommand command, string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (value is IDbDataParameter)
            {
                // Value is already a parameter.
                command.Parameters.Add(value);
            }
            else
            {
                // Create a parameter for the value.
                IDbDataParameter parameter = command.CreateParameter();

                parameter.ParameterName = name;
                parameter.Value = value;
                parameter.Direction = direction;

                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Creates and adds a new <see cref="IDbDataParameter"/> for each of the specified <paramref name="values"/> to the <see cref="IDbCommand"/> object.
        /// </summary>
        /// <param name="command"><see cref="IDbCommand"/> to which parameters need to be added.</param>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="values">The values for the parameters of the <see cref="IDbCommand"/> in the order that they appear in the SQL statement.</param>
        /// <remarks>
        /// <para>
        /// This method does very rudimentary parsing of the SQL statement so parameter names should start with the '@'
        /// character and should be surrounded by either spaces, parentheses, or commas.
        /// </para>
        /// <para>
        /// Do not use the same parameter name twice in the expression so that each parameter, identified by '@', will
        /// have a corresponding value.
        /// </para>
        /// </remarks>
        /// <returns>The fully populated parameterized command.</returns>
        /// <exception cref="ArgumentException">Number of <see cref="IDbDataParameter"/> arguments in <paramref name="sql"/> expression, identified by '@', do not match number of supplied parameter <paramref name="values"/>.</exception>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static void AddParametersWithValues(this IDbCommand command, string sql, params object[] values)
        {
            if (values.FirstOrDefault(value => value is IDbDataParameter) != null)
            {
                // Values are already parameters.
                foreach (object param in values)
                {
                    command.Parameters.Add(param);
                }
            }
            else
            {
                // Pick up all parameters that start with @ or : but skip key words such as @@IDENTITY
                string[] tokens = sql.Split(' ', '(', ')', ',', '=')
                    .Where(token => token.StartsWith(":", StringComparison.Ordinal) || token.StartsWith("@", StringComparison.Ordinal) && !token.StartsWith("@@", StringComparison.Ordinal))
                    .Distinct()
                    .Where(IsValidToken)
                    .ToArray();

                int i = 0;

                if (tokens.Length != values.Length)
                    throw new ArgumentException("Number of parameter arguments in sql expression do not match number of supplied values", nameof(values));

                foreach (string token in tokens)
                {
                    if (!command.Parameters.Contains(token))
                        command.AddParameterWithValue(token, values[i++]);
                }
            }

            command.CommandText = sql;
        }

        private static bool IsValidToken(string token)
        {
            const string Pattern = @"^[:@][a-zA-Z]\w*$";
            return Regex.IsMatch(token, Pattern);
        }

        /// <summary>
        /// Creates and returns a parameterized <see cref="IDbCommand"/>. Parameter names are embedded in the SQL statement
        /// passed as a parameter to this method.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="values">The values for the parameters of the <see cref="IDbCommand"/> in the order that they appear in the SQL statement.</param>
        /// <remarks>
        /// <para>
        /// This method does very rudimentary parsing of the SQL statement so parameter names should start with the '@'
        /// character and should be surrounded by either spaces, parentheses, or commas.
        /// </para>
        /// <para>
        /// Do not use the same parameter name twice in the expression so that each parameter, identified by '@', will
        /// have a corresponding value.
        /// </para>
        /// </remarks>
        /// <returns>The fully populated parameterized command.</returns>
        /// <exception cref="ArgumentException">Number of <see cref="IDbDataParameter"/> arguments in <paramref name="sql"/> expression, identified by '@', do not match number of supplied parameter <paramref name="values"/>.</exception>
        public static IDbCommand CreateParameterizedCommand(this IDbConnection connection, string sql, params object[] values)
        {
            IDbCommand command = connection.CreateCommand();

            command.AddParametersWithValues(sql, values);

            if (IsStoredProcedure(sql))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Force parameters to have no name- cannot determine proper name in a database abstract way.
                // As a result, callers must specify proper number of parameters, in order.
                foreach (IDbDataParameter parameter in command.Parameters)
                    parameter.ParameterName = null;
            }

            return command;
        }

        #endregion

        #region [ CSV / DataTable Conversion Functions ]

        /// <summary>
        /// Converts a delimited string into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="delimitedData">The delimited text to be converted to <see cref="DataTable"/>.</param>
        /// <param name="delimiter">The character(s) used for delimiting the text.</param>
        /// <param name="header">true, if the delimited text contains header information; otherwise, false.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable ToDataTable(this string delimitedData, string delimiter, bool header)
        {
            DataTable table = new DataTable();
            string pattern;

            // Regex pattern that will be used to split the delimited data.
            pattern = Regex.Escape(delimiter) + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            // Remove any leading and trailing whitespaces, carriage returns or line feeds.
            delimitedData = delimitedData.Trim().Trim('\r', '\n').Replace("\n", "");

            string[] lines = delimitedData.Split('\r'); //Splits delimited data into lines.

            int cursor = 0;

            // Assumes that the first line has header information.
            string[] headers = Regex.Split(lines[cursor], pattern);

            // Creates columns.
            if (header)
            {
                // Uses the first row as header row.
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Columns.Add(new DataColumn(headers[i].Trim('\"'))); //Remove any leading and trailing quotes from the column name.
                }
                cursor++;
            }
            else
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Columns.Add(new DataColumn());
                }
            }

            // Populates the data table with csv data.
            for (; cursor < lines.Length; cursor++)
            {
                // Creates new row.
                DataRow row = table.NewRow();

                // Populates the new row.
                string[] fields = Regex.Split(lines[cursor], pattern);
                for (int i = 0; i < fields.Length; i++)
                {
                    // Removes any leading and trailing quotes from the data.
                    row[i] = fields[i].Trim('\"');
                }

                // Adds the new row.
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Converts the <see cref="DataTable"/> to a multi-line delimited string (e.g., CSV export).
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> whose data is to be converted to delimited text.</param>
        /// <param name="delimiter">The character(s) to be used for delimiting the text.</param>
        /// <param name="quoted">true, if text is to be surrounded by quotes; otherwise, false.</param>
        /// <param name="header">true, if the delimited text should have header information.</param>
        /// <returns>A string of delimited text.</returns>
        public static string ToDelimitedString(this DataTable table, string delimiter, bool quoted, bool header)
        {
            StringBuilder data = new StringBuilder();

            //Uses the column names as the headers if headers are requested.
            if (header)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    data.Append((quoted ? "\"" : "") + table.Columns[i].ColumnName + (quoted ? "\"" : ""));

                    if (i < table.Columns.Count - 1)
                    {
                        data.Append(delimiter);
                    }
                }
                data.Append("\r\n");
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                //Converts data table's data to delimited data.
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    data.Append((quoted ? "\"" : "") + table.Rows[i][j] + (quoted ? "\"" : ""));

                    if (j < table.Columns.Count - 1)
                    {
                        data.Append(delimiter);
                    }
                }
                data.Append("\r\n");
            }

            //Returns the delimited data.
            return data.ToString();
        }

        #endregion

        #region [ Oracle Extensions ]

        // Because of reference dependency, these should be added to a GSF.Data assembly along with MySql versions if useful

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the number of rows affected.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>The number of rows affected.</returns>
        //public static int ExecuteNonQuery(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteNonQuery();
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and builds a <see cref="OracleDataReader"/>.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="OracleDataReader"/> object.</returns>
        //public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.ExecuteReader(sql, CommandBehavior.Default, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and builds a <see cref="OracleDataReader"/>.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="OracleDataReader"/> object.</returns>
        //public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, CommandBehavior behavior, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteReader(behavior);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the value in the first column 
        ///// of the first row in the result set.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>Value in the first column of the first row in the result set.</returns>
        //public static object ExecuteScalar(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteScalar();
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveRow(sql, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the result set.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>The first <see cref="DataRow"/> in the result set.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    DataTable dataTable = connection.RetrieveData(sql, 0, 1, parameters);

        //    if (dataTable.Rows.Count == 0)
        //        dataTable.Rows.Add(dataTable.NewRow());

        //    return dataTable.Rows[0];
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of result set, if the result set contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveData(sql, 0, int.MaxValue);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of result set, if the result set contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows)
        //{
        //    return connection.RetrieveData(sql, startRow, maxRows, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of result set, if the result set contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.RetrieveData(sql, 0, int.MaxValue, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of result set, if the result set contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        //{
        //    return connection.RetrieveDataSet(sql, startRow, maxRows, parameters).Tables[0];
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveDataSet(sql, 0, int.MaxValue);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows)
        //{
        //    return connection.RetrieveDataSet(sql, startRow, maxRows, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.RetrieveDataSet(sql, 0, int.MaxValue, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the <see cref="DataSet"/> that 
        ///// may contain multiple tables, depending on the SQL statement.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <param name="parameters">The parameter values to be used to fill in <see cref="IDbDataParameter"/> parameters identified by '@' prefix in <paramref name="sql"/> expression -or- the parameter values to be passed into stored procedure being executed.</param>
        ///// <returns>A <see cref="DataSet"/> object.</returns>
        //public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
        //    DataSet data = new DataSet("Temp");
        //    dataAdapter.Fill(data, startRow, maxRows, "Table1");

        //    return data;
        //}

        ///// <summary>
        ///// Updates the underlying data of the <see cref="DataTable"/> using <see cref="OracleConnection"/>, and
        ///// returns the number of rows successfully updated.
        ///// </summary>
        ///// <param name="sourceData">The <see cref="DataTable"/> used to update the underlying data source.</param>
        ///// <param name="sourceSql">The SQL statement used initially to populate the <see cref="DataTable"/>.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for updating the underlying data source.</param>
        ///// <returns>The number of rows successfully updated from the <see cref="DataTable"/>.</returns>
        //public static int UpdateData(this OracleConnection connection, DataTable sourceData, string sourceSql)
        //{
        //    OracleDataAdapter dataAdapter = new OracleDataAdapter(sourceSql, connection);
        //    OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);
        //    return dataAdapter.Update(sourceData);
        //}

        ///// <summary>
        /////  Takes the <see cref="OracleCommand"/> object and populates it with the given parameters.
        ///// </summary>
        ///// <param name="command">The <see cref="OracleCommand"/> whose parameters are to be populated.</param>
        ///// <param name="parameters">The parameter values to populate the <see cref="OracleCommand"/> parameters with.</param>
        //public static void PopulateParameters(this OracleCommand command, object[] parameters)
        //{
        //    command.PopulateParameters(OracleCommandBuilder.DeriveParameters, parameters);
        //}

        #endregion
    }
}