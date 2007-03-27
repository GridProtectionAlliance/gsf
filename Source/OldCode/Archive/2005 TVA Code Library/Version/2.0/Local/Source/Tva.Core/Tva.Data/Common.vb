'*******************************************************************************************************
'  TVA.Data.Common.vb - Common Database Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/???? - J. Ritchie Carroll
'       Original version of source code generated
'  05/25/2004 - J. Ritchie Carroll 
'       Added "with parameters" overloads to all basic query functions
'  06/21/2004 - J. Ritchie Carroll
'       Added support for Oracle native .NET client since ESO systems can now work with this
'  12/10/2004 - Tim M Shults
'       Added several new WithParameters overloads that allow a programmer to send just the 
'       parameter values instead of creating a series of parameter objects and then sending 
'       them through.  Easy way to cut down on the amount of code.
'       This code is just for calls to Stored Procedures and will not work for in-line SQL
'  03/28/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'
'*******************************************************************************************************

Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Data.OracleClient
Imports System.ComponentModel
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.DateTime.Common

Namespace Data

    ''' <summary>
    ''' Defines common shared database related functions.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class Common

        ''' <summary>
        ''' The default timeout duration used for executing SQL statements when timeout duration is not specified.
        ''' </summary>
        ''' <remarks></remarks>
        Public Const TimeoutDuration As Integer = 30

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' Performs SQL encoding on given T-SQL string.
        ''' </summary>
        ''' <param name="sql">The string on which SQL encoding is to be performed.</param>
        ''' <returns>The SQL encoded string.</returns>
        ''' <remarks></remarks>
        Public Shared Function SqlEncode(ByVal sql As String) As String

            Return Replace(sql, "'", "''")

        End Function

#Region " ExecuteNonQuery Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connectString">The connection string used for connecting to the data source.</param>
        ''' <param name="connectionType">The type of data provider to use for connecting to the data source and executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connectString As String, _
                ByVal connectionType As ConnectionType, ByVal timeout As Integer) As Integer

            Dim executionResult As Integer = -1
            Dim connection As IDbConnection = Nothing
            Dim command As IDbCommand = Nothing

            Select Case connectionType
                Case connectionType.SqlClient
                    connection = New SqlConnection(connectString)
                    command = New SqlCommand(sql, connection)
                Case connectionType.OracleClient
                    connection = New OracleConnection(connectString)
                    command = New OracleCommand(sql, connection)
                Case connectionType.OleDb
                    connection = New OleDbConnection(connectString)
                    command = New OleDbCommand(sql, connection)
            End Select

            connection.Open()
            command.CommandTimeout = timeout
            executionResult = command.ExecuteNonQuery()
            connection.Close()
            Return executionResult

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection) As Integer

            Return ExecuteNonQuery(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As Integer

            Return ExecuteNonQuery(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Return ExecuteNonQuery(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Integer

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteNonQuery()

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection) As Integer

            Return ExecuteNonQuery(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As Integer

            Return ExecuteNonQuery(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Return ExecuteNonQuery(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Integer

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteNonQuery()

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OracleConnection) As Integer

            Return ExecuteNonQuery(sql, connection, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the number of rows affected.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The number of rows affected.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteNonQuery()

        End Function
#End Region

#Region " ExecuteReader Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection) As OleDbDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer) As OleDbDataReader

            Return ExecuteReader(sql, connection, behavior, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As OleDbDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.OleDb.OleDbDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As OleDbDataReader

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteReader(behavior)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection) As SqlDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer) As SqlDataReader

            Return ExecuteReader(sql, connection, behavior, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As SqlDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.SqlClient.SqlDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As SqlDataReader

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteReader(behavior)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection) As OracleDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal behavior As CommandBehavior) As OracleDataReader

            Return ExecuteReader(sql, connection, behavior, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As OracleDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and builds a data reader.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="behavior">One of the System.Data.CommandBehavior values.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.OracleClient.OracleDataReader object.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal behavior As CommandBehavior, ByVal ParamArray parameters As Object()) As OracleDataReader

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteReader(behavior)

        End Function
#End Region

#Region " ExecuteScalar Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection) As Object

            Return ExecuteScalar(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As Object

            Return ExecuteScalar(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Return ExecuteScalar(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Object

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteScalar()

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection) As Object

            Return ExecuteScalar(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As Object

            Return ExecuteScalar(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Return ExecuteScalar(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Object

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteScalar()

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OracleConnection) As Object

            Return ExecuteScalar(sql, connection, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first column of the 
        ''' first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first column of the first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteScalar()

        End Function
#End Region

#Region " RetrieveRow Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection) As DataRow

            Return RetrieveRow(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As DataRow

            Return RetrieveRow(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            Return RetrieveRow(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, timeout, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection) As DataRow

            Return RetrieveRow(sql, connection, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As DataRow

            Return RetrieveRow(sql, connection, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            Return RetrieveRow(sql, connection, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, timeout, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OracleConnection) As DataRow

            Return RetrieveRow(sql, connection, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first row in the resultset.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>The first row in the resultset.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function
#End Region

#Region " RetrieveData Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, 30)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables(0)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables(0)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the first table of resultset 
        ''' if the resultset contains multiple tables.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, parameters).Tables(0)

        End Function
#End Region

#Region " RetrieveDataSet Overloaded Functions "
        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        ''' multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        ''' multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        ''' multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net OleDb data provider, and returns the resultset that may contain
        ''' multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataSet

            Dim command As New OleDbCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)

            Dim dataAdapter As New OleDbDataAdapter(command)
            Dim data As New DataSet("Temp")
            dataAdapter.Fill(data, startRow, maxRows, "Table1")

            Return data

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, TimeoutDuration)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, TimeoutDuration, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Sql Server data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="timeout">The time in seconds to wait for the SQL statement to execute.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataSet

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)

            Dim dataAdapter As New SqlDataAdapter(command)
            Dim data As New DataSet("Temp")
            dataAdapter.Fill(data, startRow, maxRows, "Table1")

            Return data

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, Nothing)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, parameters)

        End Function

        ''' <summary>
        ''' Executes the SQL statement using .Net Oracle data provider, and returns the resultset that may 
        ''' contain multiple table depending on the SQL statement.
        ''' </summary>
        ''' <param name="sql">The SQL statement to be executed.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for executing the SQL statement.</param>
        ''' <param name="startRow">The zero-based record number to start with.</param>
        ''' <param name="maxRows">The maximum number of records to retrieve.</param>
        ''' <param name="parameters">The parameters to be passed to the SQL stored procedure being executed.</param>
        ''' <returns>An System.Data.DataSet object.</returns>
        ''' <remarks></remarks>
        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, _
                ByVal ParamArray parameters As Object()) As DataSet

            Dim command As New OracleCommand(sql, connection)
            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)

            Dim dataAdapter As New OracleDataAdapter(command)
            Dim data As New DataSet("Temp")
            dataAdapter.Fill(data, startRow, maxRows, "Table1")

            Return data

        End Function
#End Region

#Region " UpdateData Overloaded Functions "
        ''' <summary>
        ''' Updates the underlying data of the System.Data.DataTable using .Net OleDb data provider, and 
        ''' returns the number of rows successfully updated.
        ''' </summary>
        ''' <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        ''' <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        ''' <param name="connection">The System.Data.OleDb.OleDbConnection to use for updating the underlying data source.</param>
        ''' <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        ''' <remarks></remarks>
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, _
                ByVal connection As OleDbConnection) As Integer

            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(sourceSql, connection)
            Dim commandBuilder As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function

        ''' <summary>
        ''' Updates the underlying data of the System.Data.DataTable using .Net Sql Server data provider, and 
        ''' returns the number of rows successfully updated.
        ''' </summary>
        ''' <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        ''' <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        ''' <param name="connection">The System.Data.SqlClient.SqlConnection to use for updating the underlying data source.</param>
        ''' <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        ''' <remarks></remarks>
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, _
                ByVal connection As SqlConnection) As Integer

            Dim dataAdapter As SqlDataAdapter = New SqlDataAdapter(sourceSql, connection)
            Dim commandBuilder As SqlCommandBuilder = New SqlCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function

        ''' <summary>
        ''' Updates the underlying data of the System.Data.DataTable using .Net Oracle data provider, and 
        ''' returns the number of rows successfully updated.
        ''' </summary>
        ''' <param name="sourceData">The System.Data.DataTable used to update the underlying data source.</param>
        ''' <param name="sourceSql">The SQL statement used initially to populate the System.Data.DataTable.</param>
        ''' <param name="connection">The System.Data.OracleClient.OracleConnection to use for updating the underlying data source.</param>
        ''' <returns>The number of rows successfully updated from the System.Data.DataTable.</returns>
        ''' <remarks></remarks>
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, _
                ByVal connection As OracleConnection) As Integer

            Dim dataAdapter As OracleDataAdapter = New OracleDataAdapter(sourceSql, connection)
            Dim commandBuilder As OracleCommandBuilder = New OracleCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function
#End Region

#Region " Conversion Functions "
        ''' <summary>
        ''' Converts delimited text to System.Data.DataTable.
        ''' </summary>
        ''' <param name="delimitedData">The delimited text to be converted to System.Data.DataTable.</param>
        ''' <param name="delimiter">The character(es) used for delimiting the text.</param>
        ''' <param name="header">True if the delimited text contains header information; otherwise False.</param>
        ''' <returns>An System.Data.DataTable object.</returns>
        ''' <remarks></remarks>
        Public Shared Function DelimitedDataToDataTable(ByVal delimitedData As String, ByVal delimiter As String, _
                ByVal header As Boolean) As DataTable

            Dim table As DataTable = New DataTable()
            Dim pattern As String = Regex.Escape(delimiter) & "(?=(?:[^""]*""[^""]*"")*(?![^""]*""))" 'Regex pattern that will be used to split the delimited data.

            delimitedData = delimitedData.Trim(New Char() {" "c, vbCr, vbLf}).Replace(vbLf, "") 'Remove any leading and trailing whitespaces, carriage returns or line feeds.
            Dim lines() As String = delimitedData.Split(vbCr)  'Split delimited data into lines.

            Dim cursor As Integer = 0
            'Assume that the first line has header information.
            Dim headers() As String = Regex.Split(lines(cursor), pattern)
            'Create columns.
            If header Then
                'Use the first row as header row.
                For i As Integer = 0 To headers.Length() - 1
                    table.Columns.Add(New DataColumn(headers(i).Trim(New Char() {""""c}))) 'Remove any leading and trailing quotes from the column name.
                Next
                cursor += 1
            Else
                For i As Integer = 0 To headers.Length() - 1
                    table.Columns.Add(New DataColumn)
                Next
            End If

            'Populate the data table with csv data.
            For cursor = cursor To lines.Length() - 1
                Dim row As DataRow = table.NewRow() 'Create new row.

                'Populate the new row.
                Dim fields() As String = Regex.Split(lines(cursor), pattern)
                For i As Integer = 0 To fields.Length() - 1
                    row(i) = fields(i).Trim(New Char() {""""c})    'Remove any leading and trailing quotes from the data.
                Next

                table.Rows.Add(row) 'Add the new row.
            Next

            'Return the data table.
            Return table

        End Function

        ''' <summary>
        ''' Converts the System.Data.DataTable to delimited text.
        ''' </summary>
        ''' <param name="table">The System.Data.DataTable whose data is to be converted to delimited text.</param>
        ''' <param name="delimiter">The character(es) to be used for delimiting the text.</param>
        ''' <param name="quoted">True if text is to be surrounded by quotes; otherwise False.</param>
        ''' <param name="header">True if the delimited text should have header information.</param>
        ''' <returns>A string of delimited text.</returns>
        ''' <remarks></remarks>
        Public Shared Function DataTableToDelimitedData(ByVal table As DataTable, ByVal delimiter As String, _
                ByVal quoted As Boolean, ByVal header As Boolean) As String

            With New StringBuilder
                'Use the column names as the headers if headers are requested.
                If header Then
                    For i As Integer = 0 To table.Columns().Count() - 1
                        .Append(IIf(quoted, """", "") & table.Columns(i).ColumnName() & IIf(quoted, """", ""))

                        If i < table.Columns.Count() - 1 Then
                            .Append(delimiter)
                        End If
                    Next
                    .Append(vbCrLf)
                End If

                For i As Integer = 0 To table.Rows().Count() - 1
                    'Convert data table's data to delimited data.
                    For j As Integer = 0 To table.Columns().Count() - 1
                        .Append(IIf(quoted, """", "") & table.Rows(i)(j) & IIf(quoted, """", ""))

                        If j < table.Columns.Count() - 1 Then
                            .Append(delimiter)
                        End If
                    Next
                    .Append(vbCrLf)
                Next

                'Return the delimited data.
                Return .ToString()
            End With

        End Function
#End Region

#Region " Helpers "
        ' tmshults 12/10/2004 - This is the private method that takes the passed Command Object queries what the 
        '                       parameters are for the given StoredProcedure and then populates the values of the 
        '                       command used to populate DataSets, Datatables, DataReaders or just used simply to 
        '                       execute the required code with no need to return any data.
        Private Shared Sub FillStoredProcParameters(ByRef command As IDbCommand, ByVal connectionType As ConnectionType, ByVal parameters() As Object)

            If parameters IsNot Nothing Then
                ' This is required for the SqlCommandBuilder to call Derive Parameters
                command.CommandType = CommandType.StoredProcedure

                ' Makes quick query to db to find the parameters for the StoredProc 
                ' and then creates them for the command
                Select Case connectionType
                    Case connectionType.SqlClient
                        SqlClient.SqlCommandBuilder.DeriveParameters(command)
                    Case connectionType.OracleClient
                        OracleClient.OracleCommandBuilder.DeriveParameters(command)
                    Case connectionType.OleDb
                        OleDb.OleDbCommandBuilder.DeriveParameters(command)
                End Select

                ' Remove the ReturnValue Parameter
                command.Parameters.RemoveAt(0)

                ' Check to see if the Parameters found match the Values provide
                If command.Parameters.Count() <> parameters.Length() Then
                    ' If there are more values provide than parameters throw an error
                    If parameters.Length > command.Parameters.Count Then _
                        Throw New ArgumentException("You have supplied more Values than Parameters listed for the Stored Procedure")

                    ' Otherwise assume that the missing values are for Parameters that have default values
                    ' and the code is willing to use the default.  To do this fill the extended ParamValue as Nothing/Null
                    ReDim Preserve parameters(command.Parameters.Count - 1) ' Make the Values array match the Parameters of the Stored Proc
                End If

                ' Assign the values to the the Parameters.
                For i As Integer = 0 To command.Parameters.Count() - 1
                    command.Parameters(i).Value = parameters(i)
                Next
            End If

        End Sub
#End Region

    End Class

End Namespace