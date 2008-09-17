//*******************************************************************************************************
//  TVA.Data.DataExtensions.vb - Defines extension functions related to database connections
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  ??/??/???? - J. Ritchie Carroll
//       Generated original version of source code.
//  05/25/2004 - J. Ritchie Carroll
//       Added "with parameters" overloads to all basic query functions.
//  06/21/2004 - J. Ritchie Carroll
//       Added support for Oracle native .NET client since ESO systems can now work with this.
//  12/10/2004 - Tim M Shults
//       Added several new WithParameters overloads that allow a programmer to send just the
//       parameter values instead of creating a series of parameter objects and then sending
//       them through. Easy way to cut down on the amount of code.
//       This code is just for calls to Stored Procedures and will not work for in-line SQL.
//  03/28/2006 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Database.Common).
//  08/21/2007 - Darrell Zuercher
//       Edited code comments.
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C# extensions.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace TVA.Data
{
    /// <summary>
    /// Defines handy database connection extension functions.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// The default timeout duration used for executing SQL statements when timeout duration is not specified.
        /// </summary>
        public const int DefaultTimeoutDuration = 30;

        #region " ExecuteNonQuery Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.ExecuteNonQuery(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.OleDb, parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, int timeout)
        {
            return connection.ExecuteNonQuery(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteNonQuery(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.SqlClient, parameters);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OracleConnection connection, string sql)
        {
            return connection.ExecuteNonQuery(sql, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(this OracleConnection connection, string sql, params object[] parameters)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.PopulateParameters(ConnectionType.OracleClient, parameters);
            return command.ExecuteNonQuery();
        }

        #endregion

        #region " ExecuteReader Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, CommandBehavior behavior, int timeout)
        {
            return connection.ExecuteReader(sql, behavior, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        public static OleDbDataReader ExecuteReader(this OleDbConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.OleDb, parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, CommandBehavior behavior, int timeout)
        {
            return connection.ExecuteReader(sql, behavior, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        public static SqlDataReader ExecuteReader(this SqlConnection connection, string sql, CommandBehavior behavior, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.SqlClient, parameters);
            return command.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, CommandBehavior behavior)
        {
            return connection.ExecuteReader(sql, behavior, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteReader(sql, CommandBehavior.Default, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        public static OracleDataReader ExecuteReader(this OracleConnection connection, string sql, CommandBehavior behavior, params object[] parameters)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.PopulateParameters(ConnectionType.OracleClient, parameters);
            return command.ExecuteReader(behavior);
        }

        #endregion

        #region " ExecuteScalar Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.ExecuteScalar(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.OleDb, parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, int timeout)
        {
            return connection.ExecuteScalar(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.ExecuteScalar(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.SqlClient, parameters);
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OracleConnection connection, string sql)
        {
            return connection.ExecuteScalar(sql, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first column of the
        /// first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first column of the first row in the resultset.</returns>
        public static object ExecuteScalar(this OracleConnection connection, string sql, params object[] parameters)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.PopulateParameters(ConnectionType.OracleClient, parameters);
            return command.ExecuteScalar();
        }

        #endregion

        #region " RetrieveRow Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OleDbConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout)
        {
            return connection.RetrieveRow(sql, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveRow(sql, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this SqlConnection connection, string sql, int timeout, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, timeout, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OracleConnection connection, string sql)
        {
            return connection.RetrieveRow(sql, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>The first row in the resultset.</returns>
        public static DataRow RetrieveRow(this OracleConnection connection, string sql, params object[] parameters)
        {
            DataTable dataTable = connection.RetrieveData(sql, 0, 1, parameters);

            if (dataTable.Rows.Count == 0)
                dataTable.Rows.Add(dataTable.NewRow());

            return dataTable.Rows[0];
        }

        #endregion

        #region " RetrieveData Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, 30);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveData(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, parameters).Tables[0];
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OracleConnection connection, string sql)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows)
        {
            return connection.RetrieveData(sql, startRow, maxRows, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OracleConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveData(sql, 0, int.MaxValue, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset,
        /// if the resultset contains multiple tables.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable RetrieveData(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, parameters).Tables[0];
        }

        #endregion

        #region " RetrieveDataSet Overloaded Functions "

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        /// multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        /// multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        /// multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        /// multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OleDbConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);

            command.PopulateParameters(ConnectionType.OleDb, parameters);
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the resultset that may
        /// contain multiple table depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, timeout, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the resultset that may
        /// contain multiple tables depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, DefaultTimeoutDuration, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net SQL Server data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this SqlConnection connection, string sql, int startRow, int maxRows, int timeout, params object[] parameters)
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            command.PopulateParameters(ConnectionType.SqlClient, parameters);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OracleConnection connection, string sql)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows)
        {
            return connection.RetrieveDataSet(sql, startRow, maxRows, null);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, params object[] parameters)
        {
            return connection.RetrieveDataSet(sql, 0, int.MaxValue, parameters);
        }

        /// <summary>
        /// Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may
        /// contain multiple tables, depending on the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        /// <param name="startRow">The zero-based record number to start with.</param>
        /// <param name="maxRows">The maximum number of records to retrieve.</param>
        /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        /// <returns>An System.Data.DataSet object.</returns>
        public static DataSet RetrieveDataSet(this OracleConnection connection, string sql, int startRow, int maxRows, params object[] parameters)
        {
            OracleCommand command = new OracleCommand(sql, connection);
            command.PopulateParameters(ConnectionType.OracleClient, parameters);
            OracleDataAdapter dataAdapter = new OracleDataAdapter(command);
            DataSet data = new DataSet("Temp");
            dataAdapter.Fill(data, startRow, maxRows, "Table1");

            return data;
        }

        #endregion

        #region " UpdateData Overloaded Functions "

        /// <summary>
        /// Updates the underlying data of the System.Data.DataTable using .Net OleDb data provider, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        public static int UpdateData(this OleDbConnection connection, DataTable sourceData, string sourceSql)
        {
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(sourceSql, connection);
            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the System.Data.DataTable using .Net SQL Server data provider, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        public static int UpdateData(this SqlConnection connection, DataTable sourceData, string sourceSql)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sourceSql, connection);
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        /// <summary>
        /// Updates the underlying data of the System.Data.DataTable using .Net Oracle data provider, and
        /// returns the number of rows successfully updated.
        /// </summary>
        /// <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        /// <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for updating the underlying data source.</param>
        /// <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        public static int UpdateData(this OracleConnection connection, DataTable sourceData, string sourceSql)
        {
            OracleDataAdapter dataAdapter = new OracleDataAdapter(sourceSql, connection);
            OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);
            return dataAdapter.Update(sourceData);
        }

        #endregion

        #region " Helpers "

        // tmshults 12/10/2004 - Takes the passed Command Object queries, plus the parameters for the given StoredProcedure, and then populates
        //                       the values of the command used to populate DataSets, Datatables, DataReaders; or, executes the required code
        //                       with no need to return any data.
        private static void PopulateParameters(this IDbCommand command, ConnectionType connectionType, object[] parameters)
        {
            if (parameters != null)
            {
                if (command.CommandText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase) ||
                    command.CommandText.StartsWith("INSERT ", StringComparison.CurrentCultureIgnoreCase) ||
                    command.CommandText.StartsWith("UPDATE ", StringComparison.CurrentCultureIgnoreCase) ||
                    command.CommandText.StartsWith("DELETE ", StringComparison.CurrentCultureIgnoreCase))
                {
                    // We assume the command to be of type Text if it begins with one of the common SQL keywords.
                    command.CommandType = CommandType.Text;

                    for (int i = 0; i <= parameters.Length - 1; i++)
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
                    switch (connectionType)
                    {
                        case ConnectionType.SqlClient:
                            SqlCommandBuilder.DeriveParameters((SqlCommand)command);
                            break;
                        case ConnectionType.OleDb:
                            OleDbCommandBuilder.DeriveParameters((OleDbCommand)command);
                            break;
                        case ConnectionType.OracleClient:
                            OracleCommandBuilder.DeriveParameters((OracleCommand)command);
                            break;
                    }

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
                    for (int i = 0; i <= command.Parameters.Count - 1; i++)
                    {
                        command.Parameters[i] = parameters[i];
                    }
                }
            }
        }

        #endregion
    }
}