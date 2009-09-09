//*******************************************************************************************************
//  DataExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/05/2003 - James R. Carroll
//       Generated original version of source code.
//  05/25/2004 - James R. Carroll
//       Added "with parameters" overloads to all basic query functions.
//  12/10/2004 - Tim M Shults
//       Added several new WithParameters overloads that allow a programmer to send just the
//       parameter values instead of creating a series of parameter objects and then sending
//       them through. Easy way to cut down on the amount of code.
//       This code is just for calls to Stored Procedures and will not work for in-line SQL.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Database.Common).
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - James R. Carroll
//      Converted to C# extensions.
//  09/29/2008 - Pinal C. Patel
//      Reviewed code comments.
//  09/09/2009 - James R. Carroll
//      Added extensions for ODBC providers.
//
//*******************************************************************************************************

using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace TVA.Data
{
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
        /// Performs SQL encoding on given T-SQL string.
        /// </summary>
        /// <param name="sql">The string on which SQL encoding is to be performed.</param>
        /// <returns>The SQL encoded string.</returns>
        public static string SqlEncode(this string sql)
        {
            return sql.Replace("\'", "\'\'").Replace("/*", "").Replace("--", "");
        }

        #endregion

        #region [ ExecuteNonQuery Overloaded Extension ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The number of rows affected.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static int ExecuteNonQuery<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            OdbcCommand command = new OdbcCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
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
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and builds a <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="IDataReader"/> object.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static IDataReader ExecuteReader<TConnection>(this TConnection connection, string sql, CommandBehavior behavior, int timeout) where TConnection : IDbConnection
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and builds a <see cref="OleDbDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OleDbDataReader"/> object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and builds a <see cref="OdbcDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="OdbcDataReader"/> object.</returns>
        public static OdbcDataReader ExecuteReader(this OdbcConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            OdbcCommand command = new OdbcCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and builds a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="SqlDataReader"/> object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteReader(behavior);
        }

        #endregion

        #region [ ExecuteScalar Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql) where TConnection : IDbConnection
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        /// <typeparam name="TConnection">Type of <see cref="IDbConnection"/> to use.</typeparam>
        public static object ExecuteScalar<TConnection>(this TConnection connection, string sql, int timeout) where TConnection : IDbConnection
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            OdbcCommand command = new OdbcCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the value in the first column 
        /// of the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>Value in the first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            return command.ExecuteScalar();
        }

        #endregion

        #region [ RetrieveRow Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this OdbcConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveRow(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        public static DataRow RetrieveRow(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            DataTable dataTable = connection.RetrieveData(dataAdapterType, sql, timeout);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        #endregion

        #region [ RetrieveData Overloaded Extensions ]

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OleDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OleDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="OdbcConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="OdbcConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
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
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="SqlConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveData(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the first <see cref="DataTable"/> 
        /// of resultset, if the resultset contains multiple tables.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable RetrieveData(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, timeout).Tables[0];
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.PopulateParameters(parameters);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this OdbcConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            OdbcCommand command = new OdbcCommand(sql, connection);
            command.PopulateParameters(parameters);
            OdbcDataAdapter dataAdapter = new OdbcDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(parameters);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql)
        {
            return connection.RetrieveDataSet(dataAdapterType, sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using <see cref="IDbConnection"/>, and returns the <see cref="DataSet"/> that 
        /// may contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use for executing the SQL statement.</param>
        /// <param name="dataAdapterType">The <see cref="Type"/> of data adapter to use to retreieve data.</param>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet RetrieveDataSet(this IDbConnection connection, Type dataAdapterType, string sql, int timeout)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = timeout;
            IDataAdapter dataAdapter = (IDataAdapter)Activator.CreateInstance(dataAdapterType, command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data);

            return data;
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
        /// <param name="parameters">The parameters to populate the <see cref="OleDbCommand"/> parameters with.</param>
        public static void PopulateParameters(this OleDbCommand command, object[] parameters)
        {
            command.PopulateParameters(OleDbCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        /// Takes the <see cref="OdbcCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="OdbcCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameters to populate the <see cref="OdbcCommand"/> parameters with.</param>
        public static void PopulateParameters(this OdbcCommand command, object[] parameters)
        {
            command.PopulateParameters(OdbcCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        ///  Takes the <see cref="SqlCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="SqlCommand"/> whose parameters are to be populated.</param>
        /// <param name="parameters">The parameters to populate the <see cref="SqlCommand"/> parameters with.</param>
        public static void PopulateParameters(this SqlCommand command, object[] parameters)
        {
            command.PopulateParameters(SqlCommandBuilder.DeriveParameters, parameters);
        }

        /// <summary>
        /// Takes the <see cref="IDbCommand"/> object and populates it with the given parameters.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> whose parameters are to be populated.</param>
        /// <param name="deriveParameters">The DeriveParameters implementation of the <paramref name="command"/> to use to populate parameters.</param>
        /// <param name="parameters">The parameters to populate the <see cref="IDbCommand"/> parameters with.</param>
        /// <typeparam name="TDbCommand">Then <see cref="IDbCommand"/> type to be used.</typeparam>
        public static void PopulateParameters<TDbCommand>(this TDbCommand command, Action<TDbCommand> deriveParameters, object[] parameters) where TDbCommand : IDbCommand
        {
            // tmshults 12/10/2004
            if (parameters != null)
            {
                string commandText = command.CommandText;

                if (string.IsNullOrEmpty(commandText))
                    throw new ArgumentNullException("command", "command.CommandText is null");

                if (commandText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("INSERT ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("UPDATE ", StringComparison.CurrentCultureIgnoreCase) ||
                    commandText.StartsWith("DELETE ", StringComparison.CurrentCultureIgnoreCase))
                {
                    // We assume the command to be of type Text if it begins with one of the common SQL keywords.
                    command.CommandType = CommandType.Text;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        command.Parameters.Add(parameters[i]);
                    }
                }
                else
                {
                    // If not we make the command a StoredProcedure type - most common use of parameterized execution.
                    command.CommandType = CommandType.StoredProcedure;

                    // Makes quick query to db to find the parameters for the StoredProc, and then creates them for
                    // the command. The DeriveParameters() only for commands with CommandType of StoredProcedure.
                    deriveParameters(command);

                    // Removes the ReturnValue Parameter.
                    command.Parameters.RemoveAt(0);

                    // Checks to see if the Parameters found match the Values provided.
                    if (command.Parameters.Count != parameters.Length)
                    {
                        // If there are more values than parameters, throws an error.
                        if (parameters.Length > command.Parameters.Count)
                            throw new ArgumentException("You have supplied more Values than Parameters listed for the Stored Procedure");

                        // Otherwise, assume that the missing values are for Parameters that have default values,
                        // and the code uses the default. To do this fill the extended ParamValue as Nothing/Null.
                        Array.Resize(ref parameters, command.Parameters.Count); // Makes the Values array match the Parameters of the Stored Proc.
                    }

                    // Assigns the values to the the Parameters.
                    for (int i = 0; i < command.Parameters.Count; i++)
                    {
                        ((DbParameter)command.Parameters[i]).Value = parameters[i];
                    }
                }
            }
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
            delimitedData = delimitedData.Trim().Trim(new char[] { '\r', '\n' }).Replace("\n", "");

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
                    table.Columns.Add(new DataColumn(headers[i].Trim(new char[] { '\"' }))); //Remove any leading and trailing quotes from the column name.
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
                    row[i] = fields[i].Trim(new char[] { '\"' });
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
                    data.Append((quoted ? "\"" : "") + table.Rows[i][j].ToString() + (quoted ? "\"" : ""));

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

        // Because of reference dependency, these should be added to a TVA.Data assembly along with MySql versions if useful
        
        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the number of rows affected.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="OracleDataReader"/> object.</returns>
        //public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, CommandBehavior behavior, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteReader(behavior);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the value in the first column 
        ///// of the first row in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>Value in the first column of the first row in the resultset.</returns>
        //public static object ExecuteScalar(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    OracleCommand command = new OracleCommand(sql, connection);
        //    command.PopulateParameters(parameters);
        //    return command.ExecuteScalar();
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql)
        //{
        //    return connection.RetrieveRow(sql, null);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataRow"/> in the resultset.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>The first <see cref="DataRow"/> in the resultset.</returns>
        //public static DataRow RetrieveRow(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    DataTable dataTable = connection.RetrieveData(sql, 0, 1, parameters);

        //    if (dataTable.Rows.Count == 0)
        //        dataTable.Rows.Add(dataTable.NewRow());

        //    return dataTable.Rows[0];
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
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
        ///// of resultset, if the resultset contains multiple tables.
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
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ///// <returns>A <see cref="DataTable"/> object.</returns>
        //public static DataTable RetrieveData(this OracleConnection connection, string sql, params object[] parameters)
        //{
        //    return connection.RetrieveData(sql, 0, int.MaxValue, parameters);
        //}

        ///// <summary>
        ///// Executes the SQL statement using <see cref="OracleConnection"/>, and returns the first <see cref="DataTable"/> 
        ///// of resultset, if the resultset contains multiple tables.
        ///// </summary>
        ///// <param name="sql">The SQL statement to be executed.</param>
        ///// <param name="connection">The <see cref="OracleConnection"/> to use for executing the SQL statement.</param>
        ///// <param name="startRow">The zero-based record number to start with.</param>
        ///// <param name="maxRows">The maximum number of records to retrieve.</param>
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        ///// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
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
        ///// <param name="parameters">The parameters to populate the <see cref="OracleCommand"/> parameters with.</param>
        //public static void PopulateParameters(this OracleCommand command, object[] parameters)
        //{
        //    command.PopulateParameters(OracleCommandBuilder.DeriveParameters, parameters);
        //}

        #endregion
    }
}