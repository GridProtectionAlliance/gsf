' James Ritchie Carroll - 2003
Option Explicit On 

Imports System
Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.SessionState
Imports TVA.Config
Imports TVA.Config.Common
Imports TVA.Config.ApplicationVariables
Imports TVA.Shared.FilePath

' This class was moved into the TVA.Config assembly because of its dependence on the TVA.VarEval JScript assembly - this way
' only one assembly references the JScript assembly directly, thereby reducing the possiblity of cross version conflicts.
Namespace Config

    ' Loads application and session scope variables persisted in a database connection or a XML based configuration file for a web application
    Public Class Asp

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Enum DataSourceType
            [OleDb]         ' Variables persisted to ole db connection
            [SqlClient]     ' Variables persisted to sql server connection
            [Xml]           ' Variables persisted to local config file
        End Enum

        Public Enum VariableScope
            [Application]   ' ASP application scope variables
            [Session]       ' ASP session scope variables
        End Enum

        Public Shared VariableCategoriesTableName As String = "VariableCategories"
        Public Shared VariablesTableName As String = "Variables"

        Public Shared Sub InitializeVariables(ByVal Source As HttpApplication, ByVal Scope As VariableScope)

            ' Define default web application variable initialization settings if they don't exist
            Variables.Create("VarInit.ConnectionType", "XML", VariableType.Text, "Variable persistence type - can be one of: OleDb, SqlClient, or Xml (load from this config file)", 1)
            Variables.Create("VarInit.ConnectionString", "", VariableType.Text, "Variable initialization connection string - only needed for database persisted variables", 2)
            Variables.Create("VarInit.Category", "", VariableType.Text, "Variable initialization category - leave blank to load all variables for given scope", 3)
            Variables.Create("VarInit.WriteLog", False, VariableType.Bool, "Variable initialization log flag", 4)
            Variables.Create("VarInit.LogFile", "VarInit.log", VariableType.Text, "Variable initialization log file name (no path)", 5)
            Variables.Save()

            InitializeVariables(Source, Scope, TranslateDataSourceType(Variables("VarInit.ConnectionType")), Variables("VarInit.Category"), Variables("VarInit.ConnectionString"), Variables("VarInit.WriteLog"), GetWebAppPath() & Variables("VarInit.LogFile"))

        End Sub

        Public Shared Sub InitializeVariables(ByVal Source As HttpApplication, ByVal Scope As VariableScope, ByVal DataSourceType As DataSourceType, Optional ByVal CategoryName As String = "", Optional ByVal ConnectionString As String = "", Optional ByVal WriteLog As Boolean = False, Optional ByVal LogFile As String = "")

            If DataSourceType = DataSourceType.Xml Then
                InitializeVariablesFromXml(Source, Scope, CategoryName, WriteLog, LogFile)
            Else
                If Len(ConnectionString) = 0 Then
                    Throw New InvalidOperationException("You must specify a ConnectionString for OLEDB or SqlClient variable initialization connections")
                Else
                    InitializeVariablesFromDatabase(Source, Scope, CategoryName, ConnectionString, WriteLog, LogFile, (DataSourceType = DataSourceType.SqlClient))
                End If
            End If

        End Sub

        Public Shared Function GetWebAppPath() As String

            Return JustPath(GetConfigFileName())

        End Function

        Private Shared Sub InitializeVariablesFromXml(ByVal Source As HttpApplication, ByVal Scope As VariableScope, ByVal CategoryName As String, ByVal WriteLog As Boolean, ByVal LogFile As String)

            Dim vars As New ApplicationVariables("", WriteLog, "", False)
            Dim var As Variable
            Dim de As DictionaryEntry
            Dim sw As StreamWriter
            Dim strVarName As String
            Dim objVarValue As Object
            Dim flgLoadVar As Boolean = True

            ' Initialize variables used by eval scripts in web pages
            vars.varEval.Global = Source
            vars.varEval.Application = Source.Application
            If Scope = VariableScope.Session Then vars.varEval.Session = Source.Session

            ' Load and evaluate variables
            vars.LogFileName = LogFile
            vars.Refresh()

            If WriteLog Then
                sw = File.AppendText(LogFile)
                sw.WriteLine(Scope.GetName(GetType(VariableScope), Scope) & " Variable Initialization Log - " & Now())
                sw.WriteLine("Filtered From ""Variables"" Table")
                sw.WriteLine("")
            End If

            For Each de In vars.Table
                var = DirectCast(de.Value, Variable)
                ' We only load variables for the given scope
                If TranslateScope(var.Scope) = Scope Then
                    ' Variable categories can be used for organization or separation of variables.  Use a blank (i.e., null string "") CategoryName
                    ' if the variable categories are setup only for organization (i.e., you want to load all variables in categories of same scope)
                    If Len(CategoryName) > 0 Then flgLoadVar = (StrComp(var.Category, CategoryName, CompareMethod.Text) = 0)

                    If flgLoadVar Then
                        If Scope = VariableScope.Application Then
                            Source.Application(var.Name) = var.Value
                        Else
                            Source.Session(var.Name) = var.Value
                        End If

                        ' Write log of loaded variables if requested...
                        If WriteLog Then sw.WriteLine(var.Name & " [" & var.Type.GetName(GetType(VariableType), var.Type) & "] = " & CStr(var.Value))
                    End If
                End If
            Next

            If WriteLog Then
                sw.WriteLine("-----------------------------------------------------------------------")
                sw.Close()
            End If

        End Sub

        Private Shared Sub InitializeVariablesFromDatabase(ByVal Source As HttpApplication, ByVal Scope As VariableScope, ByVal CategoryName As String, ByVal ConnectionString As String, ByVal WriteLog As Boolean, ByVal LogFile As String, ByVal UseSqlClient As Boolean)

            Dim varEval As New VariableEvaluator
            Dim cnn As IDbConnection
            Dim cmd As IDbCommand
            Dim rst As IDataReader
            Dim sw As StreamWriter
            Dim strSql As String
            Dim strScope As String = Scope.GetName(GetType(VariableScope), Scope)
            Dim strVarName As String
            Dim objVarValue As Object

            ' Variable categories can be used for organization or separation of variables.  Use a blank (i.e., null string "") CategoryName
            ' if the variable categories are setup only for organization (i.e., you want to load all variables in categories of same scope)
            If Len(CategoryName) > 0 Then
                strSql = "SELECT V1.Name, V2.VariablePrefix, V1.Data, V1.Type FROM " & VariablesTableName & " V1, " & VariableCategoriesTableName & " V2 WHERE V1.CategoryID=V2.ID AND V1.Disabled=0 AND V2.Disabled=0 AND V2.Scope='" & strScope & "' AND V2.Name='" & CategoryName & "' ORDER BY V2.Priority, V1.Priority"
            Else
                strSql = "SELECT V1.Name, V2.VariablePrefix, V1.Data, V1.Type FROM " & VariablesTableName & " V1, " & VariableCategoriesTableName & " V2 WHERE V1.CategoryID=V2.ID AND V1.Disabled=0 AND V2.Disabled=0 AND V2.Scope='" & strScope & "' ORDER BY V2.Priority, V1.Priority"
            End If

            ' Initialize variables used by eval scripts in web pages
            varEval.Global = Source
            varEval.Application = Source.Application
            If Scope = VariableScope.Session Then varEval.Session = Source.Session

            If WriteLog Then
                sw = File.AppendText(LogFile)
                sw.WriteLine(strScope & " Variable Initialization Log - " & Now())
                sw.WriteLine("Loaded From " & IIf(UseSqlClient, "Sql Client", "OLEDB") & " Connection: " & ConnectionString)
                sw.WriteLine("")
            End If

            If UseSqlClient Then
                cnn = New SqlConnection(ConnectionString)
                cnn.Open()
                cmd = New SqlCommand(strSql, DirectCast(cnn, SqlConnection))
            Else
                cnn = New OleDbConnection(ConnectionString)
                cnn.Open()
                cmd = New OleDbCommand(strSql, DirectCast(cnn, OleDbConnection))
            End If

            rst = cmd.ExecuteReader()

            While rst.Read()
                ' Get variable name
                If IsDBNull(rst("VariablePrefix")) Then
                    strVarName = rst("Name")
                Else
                    strVarName = rst("VariablePrefix") & rst("Name")
                End If

                ' Evaluate variable type
                Try
                    objVarValue = varEval.Evaluate(strVarName, rst("Type"), rst("Data"), sw)
                Catch
                    objVarValue = Nothing
                End Try

                If Scope = VariableScope.Application Then
                    Source.Application(strVarName) = objVarValue
                Else
                    Source.Session(strVarName) = objVarValue
                End If
            End While

            rst.Close()
            cnn.Close()

            If WriteLog Then
                sw.WriteLine("-----------------------------------------------------------------------")
                sw.Close()
            End If

        End Sub

        Private Shared Function TranslateScope(ByVal enumName As String) As VariableScope

            Select Case LCase(Trim(enumName))
                Case "application", "app"
                    Return VariableScope.Application
                Case "session", "user"
                    Return VariableScope.Session
                Case Else
                    Return VariableScope.Application
            End Select

        End Function

        Private Shared Function TranslateDataSourceType(ByVal enumName As String) As DataSourceType

            Select Case LCase(Trim(enumName))
                Case "oledb", "db", "database", "access"
                    Return DataSourceType.OleDb
                Case "sqlclient", "sql", "sql server", "sqlserver"
                    Return DataSourceType.SqlClient
                Case "xml", "config", "app.config"
                    Return DataSourceType.Xml
                Case Else
                    Return DataSourceType.Xml
            End Select

        End Function

    End Class

End Namespace