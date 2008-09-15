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
//
//*******************************************************************************************************

using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.OracleClient;
using System.ComponentModel;

namespace TVA
{
    namespace Data
    {
        /// <summary>
        /// Defines common shared database related functions.
        /// </summary>
        public static class Common
        {
            /// <summary>
            /// The default timeout duration used for executing SQL statements when timeout duration is not specified.
            /// </summary>
            public const int DefaultTimeoutDuration = 30;

            /// <summary>
            /// Performs SQL encoding on given T-SQL string.
            /// </summary>
            /// <param name="sql">The string on which SQL encoding is to be performed.</param>
            /// <returns>The SQL encoded string.</returns>
            public static string SqlEncode(string sql)
            {
                return sql.Replace("\'", "\'\'").Replace("/*", "").Replace("--", "");
            }

            #region " ExecuteNonQuery Overloaded Functions "
            /// <summary>
            /// Executes the SQL statement, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connectString">The connection string used for connecting to the data source.</param>
            /// <param name="connectionType">The type of data provider to use for connecting to the data source and executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, string connectString, ConnectionType connectionType, int timeout)
            {

                int executionResult = -1;
                IDbConnection connection = null;
                IDbCommand command = null;

                switch (connectionType)
                {
                    case connectionType.SqlClient:
                        connection = new SqlConnection(connectString);
                        command = new SqlCommand(sql, connection);
                        break;
                    case connectionType.OracleClient:
                        connection = new OracleConnection(connectString);
                        command = new OracleCommand(sql, connection);
                        break;
                    case connectionType.OleDb:
                        connection = new OleDbConnection(connectString);
                        command = new OleDbCommand(sql, connection);
                        break;
                }

                connection.Open();
                command.CommandTimeout = timeout;
                executionResult = command.ExecuteNonQuery();
                connection.Close();
                return executionResult;

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OleDbConnection connection)
            {

                return ExecuteNonQuery(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OleDbConnection connection, int timeout)
            {

                return ExecuteNonQuery(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OleDbConnection connection, params object[] parameters)
            {

                return ExecuteNonQuery(sql, connection, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OleDbConnection connection, int timeout, params object[] parameters)
            {

                OleDbCommand command = new OleDbCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.OleDb, parameters);
                return command.ExecuteNonQuery();

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, SqlConnection connection)
            {

                return ExecuteNonQuery(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, SqlConnection connection, int timeout)
            {

                return ExecuteNonQuery(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, SqlConnection connection, params object[] parameters)
            {

                return ExecuteNonQuery(sql, connection, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, SqlConnection connection, int timeout, params object[] parameters)
            {

                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.SqlClient, parameters);
                return command.ExecuteNonQuery();

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OracleConnection connection)
            {

                return ExecuteNonQuery(sql, connection, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The number of rows affected.</returns>
            public static int ExecuteNonQuery(string sql, OracleConnection connection, params object[] parameters)
            {

                OracleCommand command = new OracleCommand(sql, connection);

                FillStoredProcParameters(command, ConnectionType.OracleClient, parameters);
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
            public static OleDbDataReader ExecuteReader(string sql, OleDbConnection connection)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
            public static OleDbDataReader ExecuteReader(string sql, OleDbConnection connection, CommandBehavior behavior, int timeout)
            {

                return ExecuteReader(sql, connection, behavior, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
            public static OleDbDataReader ExecuteReader(string sql, OleDbConnection connection, params object[] parameters)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default, DefaultTimeoutDuration, parameters);

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
            public static OleDbDataReader ExecuteReader(string sql, OleDbConnection connection, CommandBehavior behavior, int timeout, params object[] parameters)
            {

                OleDbCommand command = new OleDbCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.OleDb, parameters);
                return command.ExecuteReader(behavior);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
            public static SqlDataReader ExecuteReader(string sql, SqlConnection connection)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
            public static SqlDataReader ExecuteReader(string sql, SqlConnection connection, CommandBehavior behavior, int timeout)
            {

                return ExecuteReader(sql, connection, behavior, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
            public static SqlDataReader ExecuteReader(string sql, SqlConnection connection, params object[] parameters)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
            public static SqlDataReader ExecuteReader(string sql, SqlConnection connection, CommandBehavior behavior, int timeout, params object[] parameters)
            {

                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.SqlClient, parameters);
                return command.ExecuteReader(behavior);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
            public static OracleDataReader ExecuteReader(string sql, OracleConnection connection)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
            /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
            public static OracleDataReader ExecuteReader(string sql, OracleConnection connection, CommandBehavior behavior)
            {

                return ExecuteReader(sql, connection, behavior, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
            public static OracleDataReader ExecuteReader(string sql, OracleConnection connection, params object[] parameters)
            {

                return ExecuteReader(sql, connection, CommandBehavior.Default, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="behavior">One of the System.Data.CommandBehavior values.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
            public static OracleDataReader ExecuteReader(string sql, OracleConnection connection, CommandBehavior behavior, params object[] parameters)
            {

                OracleCommand command = new OracleCommand(sql, connection);

                FillStoredProcParameters(command, ConnectionType.OracleClient, parameters);
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
            public static object ExecuteScalar(string sql, OleDbConnection connection)
            {

                return ExecuteScalar(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, OleDbConnection connection, int timeout)
            {

                return ExecuteScalar(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, OleDbConnection connection, params object[] parameters)
            {

                return ExecuteScalar(sql, connection, DefaultTimeoutDuration, parameters);

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
            public static object ExecuteScalar(string sql, OleDbConnection connection, int timeout, params object[] parameters)
            {

                OleDbCommand command = new OleDbCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.OleDb, parameters);
                return command.ExecuteScalar();

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, SqlConnection connection)
            {

                return ExecuteScalar(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, SqlConnection connection, int timeout)
            {

                return ExecuteScalar(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, SqlConnection connection, params object[] parameters)
            {

                return ExecuteScalar(sql, connection, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, SqlConnection connection, int timeout, params object[] parameters)
            {

                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.SqlClient, parameters);
                return command.ExecuteScalar();

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, OracleConnection connection)
            {

                return ExecuteScalar(sql, connection, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first column of the
            /// first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first column of the first row in the resultset.</returns>
            public static object ExecuteScalar(string sql, OracleConnection connection, params object[] parameters)
            {

                OracleCommand command = new OracleCommand(sql, connection);

                FillStoredProcParameters(command, ConnectionType.OracleClient, parameters);
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
            public static DataRow RetrieveRow(string sql, OleDbConnection connection)
            {

                return RetrieveRow(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, OleDbConnection connection, int timeout)
            {

                return RetrieveRow(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, OleDbConnection connection, params object[] parameters)
            {

                return RetrieveRow(sql, connection, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, OleDbConnection connection, int timeout, params object[] parameters)
            {

                DataTable with_1 = RetrieveData(sql, connection, 0, 1, timeout, parameters);
                if (with_1.Rows.Count == 0)
                {
                    with_1.Rows.Add(with_1.NewRow());
                }
                return with_1.Rows[0];

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, SqlConnection connection)
            {

                return RetrieveRow(sql, connection, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, SqlConnection connection, int timeout)
            {

                return RetrieveRow(sql, connection, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, SqlConnection connection, params object[] parameters)
            {

                return RetrieveRow(sql, connection, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, SqlConnection connection, int timeout, params object[] parameters)
            {

                DataTable with_1 = RetrieveData(sql, connection, 0, 1, timeout, parameters);
                if (with_1.Rows.Count == 0)
                {
                    with_1.Rows.Add(with_1.NewRow());
                }
                return with_1.Rows[0];

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, OracleConnection connection)
            {

                return RetrieveRow(sql, connection, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>The first row in the resultset.</returns>
            public static DataRow RetrieveRow(string sql, OracleConnection connection, params object[] parameters)
            {

                DataTable with_1 = RetrieveData(sql, connection, 0, 1, parameters);
                if (with_1.Rows.Count == 0)
                {
                    with_1.Rows.Add(with_1.NewRow());
                }
                return with_1.Rows[0];

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
            public static DataTable RetrieveData(string sql, OleDbConnection connection)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue, 30);

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
            public static DataTable RetrieveData(string sql, OleDbConnection connection, int startRow, int maxRows, int timeout)
            {

                return RetrieveData(sql, connection, startRow, maxRows, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, OleDbConnection connection, params object[] parameters)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration, parameters);

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
            public static DataTable RetrieveData(string sql, OleDbConnection connection, int startRow, int maxRows, int timeout, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables[0];

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, SqlConnection connection)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="startRow">The zero-based record number to start with.</param>
            /// <param name="maxRows">The maximum number of records to retrieve.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, SqlConnection connection, int startRow, int maxRows, int timeout)
            {

                return RetrieveData(sql, connection, startRow, maxRows, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, SqlConnection connection, params object[] parameters)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="startRow">The zero-based record number to start with.</param>
            /// <param name="maxRows">The maximum number of records to retrieve.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, SqlConnection connection, int startRow, int maxRows, int timeout, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables[0];

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, OracleConnection connection)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue);

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
            public static DataTable RetrieveData(string sql, OracleConnection connection, int startRow, int maxRows)
            {

                return RetrieveData(sql, connection, startRow, maxRows, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset,
            /// if the resultset contains multiple tables.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataTable object.</returns>
            public static DataTable RetrieveData(string sql, OracleConnection connection, params object[] parameters)
            {

                return RetrieveData(sql, connection, 0, int.MaxValue, parameters);

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
            public static DataTable RetrieveData(string sql, OracleConnection connection, int startRow, int maxRows, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, parameters).Tables[0];

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
            public static DataSet RetrieveDataSet(string sql, OleDbConnection connection)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration);

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
            public static DataSet RetrieveDataSet(string sql, OleDbConnection connection, int startRow, int maxRows, int timeout)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
            /// multiple tables, depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, OleDbConnection connection, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration, parameters);

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
            public static DataSet RetrieveDataSet(string sql, OleDbConnection connection, int startRow, int maxRows, int timeout, params object[] parameters)
            {

                OleDbCommand command = new OleDbCommand(sql, connection);

                FillStoredProcParameters(command, ConnectionType.OleDb, parameters);

                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                DataSet data = new DataSet("Temp");
                dataAdapter.Fill(data, startRow, maxRows, "Table1");

                return data;

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may
            /// contain multiple table depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, SqlConnection connection)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may
            /// contain multiple tables, depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="startRow">The zero-based record number to start with.</param>
            /// <param name="maxRows">The maximum number of records to retrieve.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, SqlConnection connection, int startRow, int maxRows, int timeout)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may
            /// contain multiple tables depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, SqlConnection connection, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue, DefaultTimeoutDuration, parameters);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may
            /// contain multiple tables, depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
            /// <param name="startRow">The zero-based record number to start with.</param>
            /// <param name="maxRows">The maximum number of records to retrieve.</param>
            /// <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, SqlConnection connection, int startRow, int maxRows, int timeout, params object[] parameters)
            {

                SqlCommand command = new SqlCommand(sql, connection);
                command.CommandTimeout = timeout;

                FillStoredProcParameters(command, ConnectionType.SqlClient, parameters);

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
            public static DataSet RetrieveDataSet(string sql, OracleConnection connection)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue);

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
            public static DataSet RetrieveDataSet(string sql, OracleConnection connection, int startRow, int maxRows)
            {

                return RetrieveDataSet(sql, connection, startRow, maxRows, null);

            }

            /// <summary>
            /// Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may
            /// contain multiple tables, depending on the SQL statement.
            /// </summary>
            /// <param name="sql">The SQL statement to be executed.</param>
            /// <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
            /// <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
            /// <returns>An System.Data.DataSet object.</returns>
            public static DataSet RetrieveDataSet(string sql, OracleConnection connection, params object[] parameters)
            {

                return RetrieveDataSet(sql, connection, 0, int.MaxValue, parameters);

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
            public static DataSet RetrieveDataSet(string sql, OracleConnection connection, int startRow, int maxRows, params object[] parameters)
            {

                OracleCommand command = new OracleCommand(sql, connection);
                FillStoredProcParameters(command, ConnectionType.OracleClient, parameters);

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
            public static int UpdateData(DataTable sourceData, string sourceSql, OleDbConnection connection)
            {

                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(sourceSql, connection);
                OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(dataAdapter);
                return dataAdapter.Update(sourceData);

            }

            /// <summary>
            /// Updates the underlying data of the System.Data.DataTable using .Net Sql Server data provider, and
            /// returns the number of rows successfully updated.
            /// </summary>
            /// <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
            /// <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
            /// <param name="connection">The System.Data.SqlClient.SqlConnection to use for updating the underlying data source.</param>
            /// <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
            public static int UpdateData(DataTable sourceData, string sourceSql, SqlConnection connection)
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
            public static int UpdateData(DataTable sourceData, string sourceSql, OracleConnection connection)
            {

                OracleDataAdapter dataAdapter = new OracleDataAdapter(sourceSql, connection);
                OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);
                return dataAdapter.Update(sourceData);

            }
            #endregion

            #region " Conversion Functions "
            /// <summary>
            /// Converts delimited text to System.Data.DataTable.
            /// </summary>
            /// <param name="delimitedData">The delimited text to be converted to System.Data.DataTable.</param>
            /// <param name="delimiter">The character(s) used for delimiting the text.</param>
            /// <param name="header">True, if the delimited text contains header information; otherwise, false.</param>
            /// <returns>A System.Data.DataTable object.</returns>
            public static DataTable DelimitedDataToDataTable(string delimitedData, string delimiter, bool header)
            {

                DataTable table = new DataTable();
                string pattern = Regex.Escape(delimiter) + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"; //Regex pattern that will be used to split the delimited data.

                delimitedData = delimitedData.Trim(new char[] { ' ', Constants.vbCr, Constants.vbLf }).Replace(Constants.vbLf, ""); //Remove any leading and trailing whitespaces, carriage returns or line feeds.
                string[] lines = delimitedData.Split(Constants.vbCr); //Splits delimited data into lines.

                int cursor = 0;
                //Assumes that the first line has header information.
                string[] headers = Regex.Split(lines[cursor], pattern);
                //Creates columns.
                if (header)
                {
                    //Uses the first row as header row.
                    for (int i = 0; i <= headers.Length - 1; i++)
                    {
                        table.Columns.Add(new DataColumn(headers[i].Trim(new char[] { '\"' }))); //Remove any leading and trailing quotes from the column name.
                    }
                    cursor++;
                }
                else
                {
                    for (int i = 0; i <= headers.Length - 1; i++)
                    {
                        table.Columns.Add(new DataColumn());
                    }
                }

                //Populates the data table with csv data.
                for (cursor = cursor; cursor <= lines.Length - 1; cursor++)
                {
                    DataRow row = table.NewRow(); //Creates new row.

                    //Populates the new row.
                    string[] fields = Regex.Split(lines[cursor], pattern);
                    for (int i = 0; i <= fields.Length - 1; i++)
                    {
                        row[i] = fields[i].Trim(new char[] { '\"' }); //Removes any leading and trailing quotes from the data.
                    }

                    table.Rows.Add(row); //Adds the new row.
                }

                //Returns the data table.
                return table;

            }

            /// <summary>
            /// Converts the System.Data.DataTable to delimited text.
            /// </summary>
            /// <param name="table">The System.Data.DataTable whose data is to be converted to delimited text.</param>
            /// <param name="delimiter">The character(s) to be used for delimiting the text.</param>
            /// <param name="quoted">True, if text is to be surrounded by quotes; otherwise, false.</param>
            /// <param name="header">True, if the delimited text should have header information.</param>
            /// <returns>A string of delimited text.</returns>
            public static string DataTableToDelimitedData(DataTable table, string delimiter, bool quoted, bool header)
			{
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				//Uses the column names as the headers if headers are requested.
				if (header)
				{
					for (int i = 0; i <= table.Columns.Count() - 1; i++)
					{
						with_1.Append((quoted ? "\"" : "") + table.Columns[i].ColumnName+ (quoted ? "\"" : ""));
						
						if (i < table.Columns.Count- 1)
						{
							with_1.Append(delimiter);
						}
					}
					with_1.Append("\r\n");
				}
				
				for (int i = 0; i <= table.Rows.Count() - 1; i++)
				{
					//Converts data table's data to delimited data.
					for (int j = 0; j <= table.Columns.Count() - 1; j++)
					{
						with_1.Append((quoted ? "\"" : "") + table.Rows[i][j].ToString() + (quoted ? "\"" : ""));
						
						if (j < table.Columns.Count- 1)
						{
							with_1.Append(delimiter);
						}
					}
					with_1.Append("\r\n");
				}
				
				//Returns the delimited data.
				return with_1.ToString();
				
			}
            #endregion

            #region " Helpers "
            // tmshults 12/10/2004 - Takes the passed Command Object queries, plus the parameters for the given StoredProcedure, and then populates
            //                       the values of the command used to populate DataSets, Datatables, DataReaders; or, executes the required code
            //                       with no need to return any data.
            private static void FillStoredProcParameters(IDbCommand command, ConnectionType connectionType, object[] parameters)
            {

                if (parameters != null)
                {
                    if (command.CommandText.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase) || command.CommandText.StartsWith("INSERT ", StringComparison.CurrentCultureIgnoreCase) || command.CommandText.StartsWith("UPDATE ", StringComparison.CurrentCultureIgnoreCase) || command.CommandText.StartsWith("DELETE ", StringComparison.CurrentCultureIgnoreCase))
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
                            case connectionType.SqlClient:
                                System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(command);
                                break;
                            case connectionType.OracleClient:
                                System.Data.OracleClient.OracleCommandBuilder.DeriveParameters(command);
                                break;
                            case connectionType.OleDb:
                                System.Data.OleDb.OleDbCommandBuilder.DeriveParameters(command);
                                break;
                        }

                        // Removes the ReturnValue Parameter.
                        command.Parameters.RemoveAt(0);

                        // Checks to see if the Parameters found match the Values provided.
                        if (command.Parameters.Count != parameters.Length)
                        {
                            // If there are more values than parameters, throws an error.
                            if (parameters.Length > command.Parameters.Count)
                            {
                                throw (new ArgumentException("You have supplied more Values than Parameters listed for the Stored Procedure"));
                            }

                            // Otherwise, assume that the missing values are for Parameters that have default values,
                            // and the code uses the default. To do this fill the extended ParamValue as Nothing/Null.
                            Array.Resize(ref parameters, command.Parameters.Count); // Makes the Values array match the Parameters of the Stored Proc.
                        }

                        // Assigns the values to the the Parameters.
                        for (int i = 0; i <= command.Parameters.Count - 1; i++)
                        {
                            command.Parameters[i].Value = parameters[i];
                        }
                    }

                }

            }
            #endregion

        }

    }
}
