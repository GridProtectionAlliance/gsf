' James Ritchie Carroll - 2003
' 05/25/2004 JRC - Added "with parameters" overloads to all basic query functions
' 06/21/2004 JRC - Added support for Oracle native .NET client since ESO systems can now work with this

Option Explicit On 

Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Data.OracleClient
Imports System.ComponentModel
Imports TVA.Shared.String
Imports TVA.Shared.DateTime

Namespace Database

    Public Class Common

        Public Enum ConnectionType
            [OleDb]
            [SqlClient]
            [OracleClient]
        End Enum

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' Performs Sql encoding on given string
        Public Shared Function SqlEncode(ByVal Sql As String) As String

            Return Replace(Sql, "'", "''")

        End Function

        ' Executes given Sql update query for given connection string
        Public Shared Function ExecuteNonQuery(ByVal Sql As String, ByVal ConnectString As String, ByVal ConnectionType As ConnectionType, Optional ByVal Timeout As Integer = 30) As Integer

            Select Case ConnectionType
                Case ConnectionType.SqlClient
                    Dim cnn As New SqlConnection(ConnectString)
                    Dim cmd As SqlCommand

                    cnn.Open()
                    cmd = New SqlCommand(Sql, cnn)
                    cmd.CommandTimeout = Timeout
                    ExecuteNonQuery = cmd.ExecuteNonQuery()

                    cnn.Close()
                Case ConnectionType.OracleClient
                    Dim cnn As New OracleConnection(ConnectString)
                    Dim cmd As OracleCommand

                    cnn.Open()
                    cmd = New OracleCommand(Sql, cnn)
                    ExecuteNonQuery = cmd.ExecuteNonQuery()

                    cnn.Close()
                Case ConnectionType.OleDb
                    Dim cnn As New OleDbConnection(ConnectString)
                    Dim cmd As OleDbCommand

                    cnn.Open()
                    cmd = New OleDbCommand(Sql, cnn)
                    cmd.CommandTimeout = Timeout
                    ExecuteNonQuery = cmd.ExecuteNonQuery()

                    cnn.Close()
            End Select

        End Function

        ' This function has been deprecated, so it is hidden from the editor - it still works as expected,
        ' but the preferred method for executing a Sql statement with a connect string is to call the above
        ' ExecuteNonQuery overload that accepts the connection type parameter
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Shared Function ExecuteNonQuery(ByVal Sql As String, ByVal ConnectString As String, Optional ByVal IsSqlClient As Boolean = True, Optional ByVal Timeout As Integer = 30) As Integer

            If IsSqlClient Then
                Return ExecuteNonQuery(Sql, ConnectString, ConnectionType.SqlClient, Timeout)
            Else
                Return ExecuteNonQuery(Sql, ConnectString, ConnectionType.OleDb, Timeout)
            End If

        End Function

        ' Executes given Sql update query for given connection
        Public Shared Function ExecuteNonQuery(ByVal Sql As String, ByVal Connection As OleDbConnection, Optional ByVal Timeout As Integer = 30) As Integer

            Return ExecuteNonQueryWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function ExecuteNonQueryWithParameters(ByVal Sql As String, ByVal Connection As OleDbConnection, ByVal Timeout As Integer, ByVal ParamArray Params As OleDbParameter()) As Integer

            Dim cmd As New OleDbCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As OleDbParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteNonQuery()

        End Function

        ' Executes given Sql update query for given connection
        Public Shared Function ExecuteNonQuery(ByVal Sql As String, ByVal Connection As SqlConnection, Optional ByVal Timeout As Integer = 30) As Integer

            Return ExecuteNonQueryWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function ExecuteNonQueryWithParameters(ByVal Sql As String, ByVal Connection As SqlConnection, ByVal Timeout As Integer, ByVal ParamArray Params As SqlParameter()) As Integer

            Dim cmd As New SqlCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As SqlParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteNonQuery()

        End Function

        ' Executes given Sql update query for given connection
        Public Shared Function ExecuteNonQuery(ByVal Sql As String, ByVal Connection As OracleConnection) As Integer

            Return ExecuteNonQueryWithParameters(Sql, Connection)

        End Function

        Public Shared Function ExecuteNonQueryWithParameters(ByVal Sql As String, ByVal Connection As OracleConnection, ByVal ParamArray Params As OracleParameter()) As Integer

            Dim cmd As New OracleCommand(Sql, Connection)

            If Params.Length > 0 Then
                For Each Param As OracleParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteNonQuery()

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal Sql As String, ByVal Connection As OleDbConnection, Optional ByVal Behavior As CommandBehavior = CommandBehavior.Default, Optional ByVal Timeout As Integer = 30) As OleDbDataReader

            Return ExecuteReaderWithParameters(Sql, Connection, Behavior, Timeout)

        End Function

        Public Shared Function ExecuteReaderWithParameters(ByVal Sql As String, ByVal Connection As OleDbConnection, ByVal Behavior As CommandBehavior, ByVal Timeout As Integer, ByVal ParamArray Params As OleDbParameter()) As OleDbDataReader

            Dim cmd As New OleDbCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As OleDbParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            If Behavior = CommandBehavior.Default Then
                Return cmd.ExecuteReader()
            Else
                Return cmd.ExecuteReader(Behavior)
            End If

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal Sql As String, ByVal Connection As SqlConnection, Optional ByVal Behavior As CommandBehavior = CommandBehavior.Default, Optional ByVal Timeout As Integer = 30) As SqlDataReader

            Return ExecuteReaderWithParameters(Sql, Connection, Behavior, Timeout)

        End Function

        Public Shared Function ExecuteReaderWithParameters(ByVal Sql As String, ByVal Connection As SqlConnection, ByVal Behavior As CommandBehavior, ByVal Timeout As Integer, ByVal ParamArray Params As SqlParameter()) As SqlDataReader

            Dim cmd As New SqlCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As SqlParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            If Behavior = CommandBehavior.Default Then
                Return cmd.ExecuteReader()
            Else
                Return cmd.ExecuteReader(Behavior)
            End If

        End Function

        ' Executes given Sql data query for given connection
        Public Shared Function ExecuteReader(ByVal Sql As String, ByVal Connection As OracleConnection, Optional ByVal Behavior As CommandBehavior = CommandBehavior.Default) As OracleDataReader

            Return ExecuteReaderWithParameters(Sql, Connection, Behavior)

        End Function

        Public Shared Function ExecuteReaderWithParameters(ByVal Sql As String, ByVal Connection As OracleConnection, ByVal Behavior As CommandBehavior, ByVal ParamArray Params As OracleParameter()) As OracleDataReader

            Dim cmd As New OracleCommand(Sql, Connection)

            If Params.Length > 0 Then
                For Each Param As OracleParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            If Behavior = CommandBehavior.Default Then
                Return cmd.ExecuteReader()
            Else
                Return cmd.ExecuteReader(Behavior)
            End If

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal Sql As String, ByVal Connection As OleDbConnection, Optional ByVal Timeout As Integer = 30) As Object

            Return ExecuteScalarWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function ExecuteScalarWithParameters(ByVal Sql As String, ByVal Connection As OleDbConnection, ByVal Timeout As Integer, ByVal ParamArray Params As OleDbParameter()) As Object

            Dim cmd As New OleDbCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As OleDbParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteScalar()

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal Sql As String, ByVal Connection As SqlConnection, Optional ByVal Timeout As Integer = 30) As Object

            Return ExecuteScalarWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function ExecuteScalarWithParameters(ByVal Sql As String, ByVal Connection As SqlConnection, ByVal Timeout As Integer, ByVal ParamArray Params As SqlParameter()) As Object

            Dim cmd As New SqlCommand(Sql, Connection)

            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As SqlParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteScalar()

        End Function

        ' Executes given Sql scalar query for given connection
        Public Shared Function ExecuteScalar(ByVal Sql As String, ByVal Connection As OracleConnection) As Object

            Return ExecuteScalarWithParameters(Sql, Connection)

        End Function

        Public Shared Function ExecuteScalarWithParameters(ByVal Sql As String, ByVal Connection As OracleConnection, ByVal ParamArray Params As OracleParameter()) As Object

            Dim cmd As New OracleCommand(Sql, Connection)

            If Params.Length > 0 Then
                For Each Param As OracleParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Return cmd.ExecuteScalar()

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal Sql As String, ByVal Connection As OleDbConnection, Optional ByVal Timeout As Integer = 30) As DataRow

            Return RetrieveRowWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function RetrieveRowWithParameters(ByVal Sql As String, ByVal Connection As OleDbConnection, ByVal Timeout As Integer, ByVal ParamArray Params As OleDbParameter()) As DataRow

            With RetrieveDataWithParameters(Sql, Connection, 0, 1, Timeout, Params)
                If .Rows.Count = 0 Then
                    .Rows.Add(.NewRow())
                End If

                Return .Rows(0)
            End With

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal Sql As String, ByVal Connection As SqlConnection, Optional ByVal Timeout As Integer = 30) As DataRow

            Return RetrieveRowWithParameters(Sql, Connection, Timeout)

        End Function

        Public Shared Function RetrieveRowWithParameters(ByVal Sql As String, ByVal Connection As SqlConnection, ByVal Timeout As Integer, ByVal ParamArray Params As SqlParameter()) As DataRow

            With RetrieveDataWithParameters(Sql, Connection, 0, 1, Timeout, Params)
                If .Rows.Count = 0 Then
                    .Rows.Add(.NewRow())
                End If

                Return .Rows(0)
            End With

        End Function

        ' Return a single row of data given a Sql statement and connection
        Public Shared Function RetrieveRow(ByVal Sql As String, ByVal Connection As OracleConnection) As DataRow

            Return RetrieveRowWithParameters(Sql, Connection)

        End Function

        Public Shared Function RetrieveRowWithParameters(ByVal Sql As String, ByVal Connection As OracleConnection, ByVal ParamArray Params As OracleParameter()) As DataRow

            With RetrieveDataWithParameters(Sql, Connection, 0, 1, Params)
                If .Rows.Count = 0 Then
                    .Rows.Add(.NewRow())
                End If

                Return .Rows(0)
            End With

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal Sql As String, ByVal Connection As OleDbConnection, Optional ByVal StartRow As Integer = 0, Optional ByVal MaxRows As Integer = Integer.MaxValue, Optional ByVal Timeout As Integer = 30) As DataTable

            Return RetrieveDataWithParameters(Sql, Connection, StartRow, MaxRows, Timeout)

        End Function

        Public Shared Function RetrieveDataWithParameters(ByVal Sql As String, ByVal Connection As OleDbConnection, ByVal StartRow As Integer, ByVal MaxRows As Integer, ByVal Timeout As Integer, ByVal ParamArray Params As OleDbParameter()) As DataTable

            Dim cmd As New OleDbCommand(Sql, Connection)
            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As OleDbParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Dim da As New OleDbDataAdapter(cmd)
            Dim ds As New DataSet("Temp")

            da.Fill(ds, StartRow, MaxRows, "Table1")

            Return ds.Tables(0)

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal Sql As String, ByVal Connection As SqlConnection, Optional ByVal StartRow As Integer = 0, Optional ByVal MaxRows As Integer = Integer.MaxValue, Optional ByVal Timeout As Integer = 30) As DataTable

            Return RetrieveDataWithParameters(Sql, Connection, StartRow, MaxRows, Timeout)

        End Function

        Public Shared Function RetrieveDataWithParameters(ByVal Sql As String, ByVal Connection As SqlConnection, ByVal StartRow As Integer, ByVal MaxRows As Integer, ByVal Timeout As Integer, ByVal ParamArray Params As SqlParameter()) As DataTable

            Dim cmd As New SqlCommand(Sql, Connection)
            cmd.CommandTimeout = Timeout

            If Params.Length > 0 Then
                For Each Param As SqlParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Dim da As New SqlDataAdapter(cmd)
            Dim ds As New DataSet("Temp")

            da.Fill(ds, StartRow, MaxRows, "Table1")

            Return ds.Tables(0)

        End Function

        ' Return a data table given a Sql statement and connection
        Public Shared Function RetrieveData(ByVal Sql As String, ByVal Connection As OracleConnection, Optional ByVal StartRow As Integer = 0, Optional ByVal MaxRows As Integer = Integer.MaxValue) As DataTable

            Return RetrieveDataWithParameters(Sql, Connection, StartRow, MaxRows)

        End Function

        Public Shared Function RetrieveDataWithParameters(ByVal Sql As String, ByVal Connection As OracleConnection, ByVal StartRow As Integer, ByVal MaxRows As Integer, ByVal ParamArray Params As OracleParameter()) As DataTable

            Dim cmd As New OracleCommand(Sql, Connection)

            If Params.Length > 0 Then
                For Each Param As OracleParameter In Params
                    cmd.Parameters.Add(Param)
                Next
            End If

            Dim da As New OracleDataAdapter(cmd)
            Dim ds As New DataSet("Temp")

            da.Fill(ds, StartRow, MaxRows, "Table1")

            Return ds.Tables(0)

        End Function

        ' Returns a date (as a string) in the format of "mm/dd/yyyy" or "dd/mm/yyyy" from a given db date string.  This is handy displaying a date from the DB on a web page, e.g., many db's (like Oracle) return dates as 25-Oct-1999
        Public Shared Function GetDisplayDateFromDBDate(ByVal DateString As String, Optional ByVal DateFormat As String = "mm/dd/yyyy") As String

            Dim dtm As DateTime

            If Len(DateString) > 0 Then
                Try
                    dtm = CDate(DateString)
                Catch ex As Exception
                    dtm = Now()
                End Try

                If StrComp(DateFormat, "mm-dd-yyyy", CompareMethod.Text) = 0 Or StrComp(DateFormat, "mm/dd/yyyy", CompareMethod.Text) = 0 Then
                    Return PadLeft(DatePart("m", dtm), 2, "0"c) & "/" & PadLeft(DatePart("d", dtm), 2, "0"c) & "/" & DatePart("yyyy", dtm)
                Else
                    Return PadLeft(DatePart("d", dtm), 2, "0"c) & "/" & PadLeft(DatePart("m", dtm), 2, "0"c) & "/" & DatePart("yyyy", dtm)
                End If
            End If

        End Function

        ' Gets a date into a format most databases will recognize given a date (as a string) in the format of "mm/dd/yyyy" or "dd/mm/yyyy", e.g., many db's (like Oracle) require date fields like 25-Oct-1999
        Public Shared Function GetDBDateFromDisplayDate(ByVal DateString As String, Optional ByVal DateFormat As String = "mm/dd/yyyy") As String

            Dim strDBDate As String
            Dim strDate As String
            Dim strMonth As String
            Dim strDay As String
            Dim strYear As String
            Dim intPos1 As Integer
            Dim intPos2 As Integer

            If Len(DateString) > 0 Then
                strDate = Replace(DateString, "-", "/")

                intPos1 = InStr(1, strDate, "/", CompareMethod.Binary)
                If intPos1 > 0 Then intPos2 = InStr(intPos1 + 1, strDate, "/", CompareMethod.Binary)

                If intPos1 > 0 And intPos2 > 0 Then
                    If StrComp(DateFormat, "mm-dd-yyyy", CompareMethod.Text) = 0 Or StrComp(DateFormat, "mm/dd/yyyy", CompareMethod.Text) = 0 Then
                        strMonth = Left(DateString, intPos1 - 1)
                        strDay = Mid(DateString, intPos1 + 1, intPos2 - intPos1 - 1)
                    Else
                        strDay = Left(DateString, intPos1 - 1)
                        strMonth = Mid(DateString, intPos1 + 1, intPos2 - intPos1 - 1)
                    End If

                    strYear = Mid(DateString, intPos2 + 1)

                    strDBDate = "'" & strDay & "-" & GetShortMonth(strMonth) & "-" & strYear & "'"
                End If
            End If

            If Len(strDBDate) = 0 Then strDBDate = "NULL"

            Return strDBDate

        End Function

    End Class

End Namespace