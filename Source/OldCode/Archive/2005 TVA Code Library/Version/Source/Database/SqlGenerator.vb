' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Text
Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Shared.String

Namespace Database

    <ToolboxBitmap(GetType(SqlGenerator), "SqlGenerator.bmp"), DefaultProperty("SourceSchema")> _
    Public Class SqlGenerator

        Inherits Component

        Private schSource As Schema
        Private tblCurrent As Table
        Private colFields As New ArrayList
        Private strWhereSql As String
        Private flgIncludeNulls As Boolean

        Public Sub New()

            strWhereSql = ""
            flgIncludeNulls = False

        End Sub

        Public Sub New(ByVal SourceSchema As Schema)

            MyClass.New()
            schSource = SourceSchema

        End Sub

        <Browsable(True), Category("Configuration"), Description("Source Schema object to use for Sql generation.")> _
        Public Property SourceSchema() As Schema
            Get
                Return schSource
            End Get
            Set(ByVal Value As Schema)
                schSource = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("WHERE clause Sql to append to generated Sql statements."), DefaultValue("")> _
        Public Property WhereSql() As String
            Get
                Return strWhereSql
            End Get
            Set(ByVal Value As String)
                strWhereSql = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to include NULL values in generated Sql statements."), DefaultValue(False)> _
        Public Property IncludeNulls() As Boolean
            Get
                Return flgIncludeNulls
            End Get
            Set(ByVal Value As Boolean)
                flgIncludeNulls = Value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Connection() As OleDbConnection
            Get
                Return schSource.Connection
            End Get
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property TableName() As String
            Get
                Return tblCurrent.Name
            End Get
            Set(ByVal Value As String)
                ' Lookup table in collection
                tblCurrent = schSource.Tables(Value)
                If Not tblCurrent Is Nothing Then
                    ' Clear any existing field values
                    Dim fld As Field

                    For Each fld In tblCurrent.Fields
                        fld.Value = Nothing
                    Next

                    colFields.Clear()
                Else
                    Throw New InvalidOperationException("Table [" & Value & "] not found in schema")
                End If
            End Set
        End Property

        Public Sub SetField(ByVal FieldName As String, ByVal FieldValue As Object)

            tblCurrent.Fields(FieldName).Value = FieldValue

            If colFields.BinarySearch(FieldName, CaseInsensitiveComparer.Default) < 0 Then
                colFields.Add(FieldName)
                colFields.Sort(CaseInsensitiveComparer.Default)
            End If

        End Sub

        Public Function InsertSql() As String

            Dim strSql As New StringBuilder
            Dim strField As String
            Dim flgSetComma As Boolean

            With tblCurrent
                strSql.Append("INSERT INTO ")
                strSql.Append(.FullName)
                strSql.Append(" (")

                With .Fields
                    For Each strField In colFields
                        With .Item(strField)
                            If flgIncludeNulls Or Not NotNull(.Value) Is Nothing Then
                                If flgSetComma Then
                                    strSql.Append(", [")
                                Else
                                    strSql.Append("[")
                                End If
                                strSql.Append(.Name)
                                strSql.Append("]")
                                flgSetComma = True
                            End If
                        End With
                    Next

                    strSql.Append(") VALUES (")
                    flgSetComma = False

                    For Each strField In colFields
                        With .Item(strField)
                            If flgIncludeNulls Or Not NotNull(.Value) Is Nothing Then
                                If flgSetComma Then
                                    strSql.Append(", ")
                                End If
                                strSql.Append(.SqlEncodedValue)
                                flgSetComma = True
                            End If
                        End With
                    Next
                End With

                strSql.Append(")")
            End With

            If Not flgSetComma Then Throw New InvalidOperationException("No field values were specified in insert Sql")

            If Len(strWhereSql) > 0 Then
                strSql.Append(" ")
                strSql.Append(strWhereSql)
            End If

            Return strSql.ToString()

        End Function

        Public Function UpdateSql() As String

            Dim strSql As New StringBuilder
            Dim strField As String
            Dim flgSetComma As Boolean

            With tblCurrent
                strSql.Append("UPDATE ")
                strSql.Append(.FullName)
                strSql.Append(" SET ")

                With .Fields
                    For Each strField In colFields
                        With .Item(strField)
                            If flgIncludeNulls Or Not NotNull(.Value) Is Nothing Then
                                If flgSetComma Then
                                    strSql.Append(", [")
                                Else
                                    strSql.Append("[")
                                End If
                                strSql.Append(.Name)
                                strSql.Append("] = ")
                                strSql.Append(.SqlEncodedValue)
                                flgSetComma = True
                            End If
                        End With
                    Next
                End With
            End With

            If Not flgSetComma Then Throw New InvalidOperationException("No field values were specified in update Sql")

            If Len(strWhereSql) > 0 Then
                strSql.Append(" ")
                strSql.Append(strWhereSql)
            End If

            Return strSql.ToString()

        End Function

    End Class

End Namespace