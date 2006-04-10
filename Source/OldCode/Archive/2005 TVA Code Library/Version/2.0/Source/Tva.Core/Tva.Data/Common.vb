'*******************************************************************************************************
'  Tva.Data.Common.vb - Common Database Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/???? - James R Carroll
'       Original version of source code generated
'  05/25/2004 - James R Carroll 
'       Added "with parameters" overloads to all basic query functions
'  06/21/2004 - James R Carroll
'       Added support for Oracle native .NET client since ESO systems can now work with this
'  12/10/2004 - Tim M Shults
'       Added several new WithParameters overloads that allow a programmer to send just the 
'       parameter values instead of creating a series of parameter objects and then sending 
'       them through.  Easy way to cut down on the amount of code.
'       This code is just for calls to Stored Procedures and will not work for in-line SQL
'  03/28/2006 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'
'*******************************************************************************************************

Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Data.OracleClient
Imports System.ComponentModel
Imports System.Text
Imports System.Text.RegularExpressions
Imports Tva.DateTime.Common

Namespace Data

    Public NotInheritable Class Common

        Public Enum ConnectionType As Integer
            [OleDb]
            [SqlClient]
            [OracleClient]
        End Enum

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' Performs Sql encoding on given string.
        ''' </summary>
        ''' <param name="sql">The string on which Sql encoding is to be performed.</param>
        ''' <returns>The Sql encoded string.</returns>
        ''' <remarks></remarks>
        Public Shared Function SqlEncode(ByVal sql As String) As String

            Return Replace(sql, "'", "''")

        End Function

#Region "ExecuteNonQuery Overloaded Functions"
        ' Executes given Sql update query for given connection string
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

        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection) As Integer

            Return ExecuteNonQuery(sql, connection, 30)

        End Function

        ' Executes given Sql update query for given connection
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As Integer

            Return ExecuteNonQuery(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Return ExecuteNonQuery(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteNonQuery to execute StoredProcedures that don't return data.
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Integer

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteNonQuery()

        End Function

        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection) As Integer

            Return ExecuteNonQuery(sql, connection, 30)

        End Function

        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As Integer

            Return ExecuteNonQuery(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Return ExecuteNonQuery(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteNonQuery to execute StoredProcedures that don't return data.
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Integer

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteNonQuery()

        End Function

        ' Executes given Sql update query for given connection
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OracleConnection) As Integer

            Return ExecuteNonQuery(sql, connection, Nothing)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteNonQuery to execute StoredProcedures that don't return data.
        Public Shared Function ExecuteNonQuery(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As Integer

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteNonQuery()

        End Function
#End Region

#Region "ExecuteReader Overloaded Functions"
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection) As OleDbDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, 30)

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer) As OleDbDataReader

            Return ExecuteReader(sql, connection, behavior, timeout, Nothing)

        End Function

        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As OleDbDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteReader as the data that is returned.
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As OleDbDataReader

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteReader(behavior)

        End Function

        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection) As SqlDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, 30)

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer) As SqlDataReader

            Return ExecuteReader(sql, connection, behavior, timeout, Nothing)

        End Function

        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As SqlDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteReader as the data that is returned.
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal behavior As CommandBehavior, ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As SqlDataReader

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteReader(behavior)

        End Function

        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection) As OracleDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default)

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal behavior As CommandBehavior) As OracleDataReader

            Return ExecuteReader(sql, connection, behavior, Nothing)

        End Function

        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As OracleDataReader

            Return ExecuteReader(sql, connection, CommandBehavior.Default, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteReader as the data that is returned.
        Public Shared Function ExecuteReader(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal behavior As CommandBehavior, ByVal ParamArray parameters As Object()) As OracleDataReader

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteReader(behavior)

        End Function
#End Region

#Region "ExecuteScalar Overloaded Functions"
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection) As Object

            Return ExecuteScalar(sql, connection, 30)

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As Object

            Return ExecuteScalar(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Return ExecuteScalar(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteScalar as the data that is returned.
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Object

            Dim command As New OleDbCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.OleDb, parameters)
            Return command.ExecuteScalar()

        End Function

        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection) As Object

            Return ExecuteScalar(sql, connection, 30)

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As Object

            Return ExecuteScalar(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Return ExecuteScalar(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteScalar as the data that is returned.
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As Object

            Dim command As New SqlCommand(sql, connection)
            command.CommandTimeout = timeout

            FillStoredProcParameters(command, ConnectionType.SqlClient, parameters)
            Return command.ExecuteScalar()

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OracleConnection) As Object

            Return ExecuteScalar(sql, connection, Nothing)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it uses the
        '                       cmd.ExecuteScalar as the data that is returned.
        Public Shared Function ExecuteScalar(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As Object

            Dim command As New OracleCommand(sql, connection)

            FillStoredProcParameters(command, ConnectionType.OracleClient, parameters)
            Return command.ExecuteScalar()

        End Function
#End Region

#Region "RetrieveRow Overloaded Functions"
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection) As DataRow

            Return RetrieveRow(sql, connection, 30)

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer) As DataRow

            Return RetrieveRow(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            Return RetrieveRow(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns  
        '                       the first row returned from the base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, timeout, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function

        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection) As DataRow

            Return RetrieveRow(sql, connection, 30)

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer) As DataRow

            Return RetrieveRow(sql, connection, timeout, Nothing)

        End Function

        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            Return RetrieveRow(sql, connection, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns  
        '                       the first row returned from the base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal timeout As Integer, ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, timeout, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OracleConnection) As DataRow

            Return RetrieveRow(sql, connection, Nothing)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns  
        '                       the first row returned from the base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveRow(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataRow

            With RetrieveData(sql, connection, 0, 1, parameters)
                If .Rows.Count = 0 Then .Rows.Add(.NewRow())
                Return .Rows(0)
            End With

        End Function
#End Region

#Region "RetrieveData Overloaded Functions"
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, 30)

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns the 
        '                       base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables(0)

        End Function

        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, 30)

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns the 
        '                       base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, parameters).Tables(0)

        End Function

        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue)

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer) As DataTable

            Return RetrieveData(sql, connection, startRow, maxRows, Nothing)

        End Function

        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveData(sql, connection, 0, Integer.MaxValue, parameters)

        End Function

        ' tmshults 12/10/2004 - This behaves exactly like the RetrieveDataSetWithParameters method except it returns the 
        '                       base DataTable that is linked to the underlying DataSet
        Public Shared Function RetrieveData(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal ParamArray parameters As Object()) As DataTable

            Return RetrieveDataSet(sql, connection, startRow, maxRows, parameters).Tables(0)

        End Function
#End Region

#Region "RetrieveDataSet Overloaded Functions"

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, 30)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OleDbConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, 30, parameters)

        End Function

        ' tmshults 12/10/2004 - Added this method as an easy way to populate a DataSet with a StoredProc call
        '                       This takes the given values and then populates the appropriate Parameters for
        '                       the StoredProc.
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

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, 30, parameters)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, 30)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As SqlConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer, ByVal timeout As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, timeout, Nothing)

        End Function

        ' tmshults 12/10/2004 - Added this method as an easy way to populate a DataSet with a StoredProc call
        '                       This takes the given values and then populates the appropriate Parameters for
        '                       the StoredProc.
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

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal startRow As Integer, ByVal maxRows As Integer) As DataSet

            Return RetrieveDataSet(sql, connection, startRow, maxRows, Nothing)

        End Function

        Public Shared Function RetrieveDataSet(ByVal sql As String, ByVal connection As OracleConnection, _
                ByVal ParamArray parameters As Object()) As DataSet

            Return RetrieveDataSet(sql, connection, 0, Integer.MaxValue, parameters)

        End Function

        ' tmshults 12/10/2004 - Added this method as an easy way to populate a DataSet with a StoredProc call
        '                       This takes the given values and then populates the appropriate Parameters for
        '                       the StoredProc.
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

#Region "UpdateData Overloaded Functions"
        'Pinal Patel 04/06/05 - Updates the underlying data source of the specified data table.
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, ByVal connection As SqlConnection) As Integer

            Dim dataAdapter As SqlDataAdapter = New SqlDataAdapter(sourceSql, connection)
            Dim commandBuilder As SqlCommandBuilder = New SqlCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function

        'Pinal Patel 04/06/05 - Updates the underlying data source of the specified data table.
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, ByVal connection As OleDbConnection) As Integer

            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(sourceSql, connection)
            Dim commandBuilder As OleDbCommandBuilder = New OleDbCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function

        'Pinal Patel 04/06/05 - Updates the underlying data source of the specified data table.
        Public Shared Function UpdateData(ByVal sourceData As DataTable, ByVal sourceSql As String, ByVal connection As OracleConnection) As Integer

            Dim dataAdapter As OracleDataAdapter = New OracleDataAdapter(sourceSql, connection)
            Dim commandBuilder As OracleCommandBuilder = New OracleCommandBuilder(dataAdapter)
            Return dataAdapter.Update(sourceData)

        End Function
#End Region

#Region "Conversion Functions"
        'Pinal Patel 05/27/05 - Converts delimited data to data table.
        Public Shared Function DelimitedDataToDataTable(ByVal delimitedData As String, ByVal delimiter As String, ByVal header As Boolean) As DataTable

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

        'Pinal Patel 05/27/05 - Converts a data table to delimited data.
        Public Shared Function DataTableToDelimitedData(ByVal table As DataTable, ByVal delimiter As String, ByVal quoted As Boolean, ByVal header As Boolean) As String

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

#Region "Helpers"
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