' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.IO
Imports System.Text
Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Database.Common
Imports TVA.Shared.String
Imports TVA.Shared.FilePath
Imports VB = Microsoft.VisualBasic

Namespace Database

    ' Note: if you have triggers that insert records into other tables automatically that have defined records to
    ' be inserted, this class will check for this occurance and do Sql updates instead of Sql inserts.  However,
    ' just like in the DataUpdater class, you must define the primary key fields in the database or through code
    ' to be used for updates for each table in the table collection in order for the updates to occur, hence any
    ' tables which have no key fields defined yet appear in the table collection will not be updated...
    <ToolboxBitmap(GetType(DataInserter), "DataInserter.bmp"), DefaultProperty("FromConnectString"), DefaultEvent("OverallProgress")> _
    Public Class DataInserter

        Inherits BulkDataOperationBase
        Implements IComponent

        Private flgAttemptBulkInsert As Boolean = False
        Private flgForceBulkInsert As Boolean = False
        Private strBulkInsertSettings As String = "FIELDTERMINATOR = '\t', ROWTERMINATOR = '\n', CODEPAGE = 'OEM', FIRE_TRIGGERS, KEEPNULLS"
        Private encBulkInsertEncoding As Encoding = Encoding.ASCII
        Private strBulkInsertFilePath As String = GetApplicationPath()
        Private strDelimeterReplacement As String = " - "
        Private flgClearDestinationTables As Boolean = False
        Private flgAttemptTruncateTable As Boolean = False
        Private flgForceTruncateTable As Boolean = False

        Public Event TableCleared(ByVal TableName As String)
        Public Event BulkInsertExecuting(ByVal TableName As String)
        Public Event BulkInsertCompleted(ByVal TableName As String, ByVal TotalRows As Integer, ByVal TotalSeconds As Integer)
        Public Event BulkInsertException(ByVal TableName As String, ByVal Sql As String, ByVal ex As Exception)

        ' Component Implementation
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Overloads Property Site() As ISite Implements IComponent.Site
            Get
                Return ComponentSite
            End Get
            Set(ByVal Value As ISite)
                ComponentSite = Value
            End Set
        End Property

        Public Overrides Sub Close()

            MyBase.Close()
            RaiseEvent Disposed(Me, EventArgs.Empty)

        End Sub

        Public Sub New()

            MyBase.New()

        End Sub

        Public Sub New(ByVal FromConnectString As String, ByVal ToConnectString As String)

            MyBase.New(FromConnectString, ToConnectString)

        End Sub

        Public Sub New(ByVal FromSchema As Schema, ByVal ToSchema As Schema)

            MyBase.New(FromSchema, ToSchema)

        End Sub

        <Browsable(True), Category("Sql Server Specific"), Description("Set to True to attempt use of a BULK INSERT on a destination Sql Server connection if it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(False)> _
        Public Property AttemptBulkInsert() As Boolean
            Get
                Return flgAttemptBulkInsert
            End Get
            Set(ByVal Value As Boolean)
                flgAttemptBulkInsert = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("Set to True to force use of a BULK INSERT on a destination Sql Server connection regardless of whether or not it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(False)> _
        Public Property ForceBulkInsert() As Boolean
            Get
                Return flgForceBulkInsert
            End Get
            Set(ByVal Value As Boolean)
                flgForceBulkInsert = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("This setting defines the Sql Server BULK INSERT settings that will be used if a BULK INSERT is performed.")> _
        Public Property BulkInsertSettings() As String
            Get
                Return strBulkInsertSettings
            End Get
            Set(ByVal Value As String)
                strBulkInsertSettings = Value
            End Set
        End Property

        <Browsable(False), Category("Sql Server Specific"), Description("This setting defines the text encoding that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the encoding output matches the specified CODEPAGE value in the BulkInsertSettings property."), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property BulkInsertEncoding() As Encoding
            Get
                Return encBulkInsertEncoding
            End Get
            Set(ByVal Value As Encoding)
                encBulkInsertEncoding = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("This setting defines the file path that will be used when writing a temporary BULK INSERT file that will be needed if a Sql Server BULK INSERT is performed - make sure the destination Sql Server has rights to this path.")> _
        Public Property BulkInsertFilePath() As String
            Get
                Return strBulkInsertFilePath
            End Get
            Set(ByVal Value As String)
                strBulkInsertFilePath = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("This specifies the string that will be substituted for the field terminator or row terminator if encountered in a database value while creating a BULK INSERT file.  The field terminator and row terminator values are defined in the BulkInsertSettings property specified by the FIELDTERMINATOR and ROWTERMINATOR keywords repectively.")> _
        Public Property DelimeterReplacement() As String
            Get
                Return strDelimeterReplacement
            End Get
            Set(ByVal Value As String)
                strDelimeterReplacement = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to clear all data from the destination database before processing data inserts."), DefaultValue(False)> _
        Public Property ClearDestinationTables() As Boolean
            Get
                Return flgClearDestinationTables
            End Get
            Set(ByVal Value As Boolean)
                flgClearDestinationTables = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("Set to True to attempt use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True and it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(False)> _
        Public Property AttemptTruncateTable() As Boolean
            Get
                Return flgAttemptTruncateTable
            End Get
            Set(ByVal Value As Boolean)
                flgAttemptTruncateTable = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Server Specific"), Description("Set to True to force use of a TRUNCATE TABLE on a destination Sql Server connection if ClearDestinationTables is True regardless of whether or not it looks like the referential integrity definition supports this.  Your Sql Server connection will need the rights to perform this operation."), DefaultValue(False)> _
        Public Property ForceTruncateTable() As Boolean
            Get
                Return flgForceTruncateTable
            End Get
            Set(ByVal Value As Boolean)
                flgForceTruncateTable = Value
            End Set
        End Property

        Public Overrides Sub Execute()

            Dim lstTables As New ArrayList
            Dim tblLookup As Table
            Dim tbl As Table
            Dim x As Integer

            If colTables Is Nothing Then Analyze()

            ' We copy the tables into an array list so we can sort and process them in priority order
            For Each tbl In colTables
                If tbl.Process Then lstTables.Add(tbl)
            Next

            lstTables.Sort()

            ' Clear data from destination tables, if requested - we do this in a child to parent
            ' direction to help avoid potential constraint issues
            If flgClearDestinationTables Then
                For x = lstTables.Count - 1 To 0 Step -1
                    ' Lookup table name in destination datasource
                    tblLookup = schTo.Tables.FindByMapName(DirectCast(lstTables(x), Table).MapName)
                    If Not tblLookup Is Nothing Then ClearTable(tblLookup)
                Next
            End If

            ' Begin inserting data into destination tables
            For x = 0 To lstTables.Count - 1
                tbl = DirectCast(lstTables(x), Table)

                ' Lookup table name in destination datasource
                tblLookup = schTo.Tables.FindByMapName(tbl.MapName)
                If Not tblLookup Is Nothing Then
                    If tbl.RowCount > 0 Then
                        ' Inform clients of table copy
                        RaiseEvent_TableProgress(tbl.Name, True, (x + 1), lstTables.Count)

                        ' Copy source table to destination
                        ExecuteInserts(tbl, tblLookup)
                    Else
                        ' Inform clients of table skip
                        RaiseEvent_TableProgress(tbl.Name, False, (x + 1), lstTables.Count)
                    End If
                Else
                    ' Inform clients of table skip
                    RaiseEvent_TableProgress(tbl.Name, False, (x + 1), lstTables.Count)
                End If
            Next

            ' Perform final update of progress information
            RaiseEvent_TableProgress("", False, lstTables.Count, lstTables.Count)

        End Sub

        Private Sub ClearTable(ByVal ToTable As Table)

            Dim strDeleteSql As String
            Dim flgUseTruncateTable As Boolean

            If flgAttemptTruncateTable Or flgForceTruncateTable Then
                ' We only attempt a truncate table if the destination data source type is Sql Server
                ' and table has no foreign key dependencies (or user forces procedure)
                flgUseTruncateTable = flgForceTruncateTable Or (ToTable.Parent.Parent.DataSourceType = DatabaseType.SqlServer And Not ToTable.IsForeignKeyTable)
            End If

            If flgUseTruncateTable Then
                strDeleteSql = "TRUNCATE TABLE " & ToTable.FullName
            Else
                strDeleteSql = "DELETE FROM " & ToTable.FullName
            End If

            Try
                ExecuteNonQuery(strDeleteSql, ToTable.Connection, Timeout)
                RaiseEvent TableCleared(ToTable.Name)
            Catch ex As Exception
                RaiseEvent_SqlFailure(strDeleteSql, ex)
            End Try

        End Sub

        Private Sub ExecuteInserts(ByVal FromTable As Table, ByVal ToTable As Table)

            Dim colFields As Fields
            Dim fld As Field
            Dim fldLookup As Field
            Dim fldCommon As Field
            Dim strInsertSqlStub As String
            Dim strInsertSql As StringBuilder
            Dim strUpdateSqlStub As String
            Dim strUpdateSql As StringBuilder
            Dim strCountSqlStub As String
            Dim strCountSql As StringBuilder
            Dim strWhereSql As StringBuilder
            Dim strValue As String
            Dim flgAddedFirstInsert As Boolean
            Dim flgAddedFirstUpdate As Boolean
            Dim flgIsPrimary As Boolean
            Dim flgRecordExists As Boolean
            Dim intProgress As Integer
            Dim intTotal As Integer
            Dim tblSource As Table = IIf(flgUseFromSchemaRI, FromTable, ToTable)
            Dim fldAutoInc As Field
            Dim flgUseBulkInsert As Boolean
            Dim strBulkInsertRow As StringBuilder
            Dim strBulkInsertFile As String
            Dim strFldTerminator As String
            Dim strRowTerminator As String
            Dim fsBulkInsert As FileStream
            Dim bytDataRow As Byte()

            ' Create a field list of all of the common fields in both tables
            colFields = New Fields(tblSource)

            For Each fld In FromTable.Fields
                ' Lookup field name in destination table
                fldLookup = ToTable.Fields(fld.Name)
                If Not fldLookup Is Nothing Then
                    With fldLookup
                        ' We currently don't handle binary fields...
                        If _
                            Not (fld.Type = OleDbType.Binary Or fld.Type = OleDbType.LongVarBinary Or fld.Type = OleDbType.VarBinary) And _
                            Not (.Type = OleDbType.Binary Or .Type = OleDbType.LongVarBinary Or .Type = OleDbType.VarBinary) _
                        Then
                            ' Copy field information from destination field
                            If flgUseFromSchemaRI Then
                                fldCommon = New Field(colFields, fld.Name, fld.Type)
                                fldCommon.flgAutoInc = fld.AutoIncrement
                            Else
                                fldCommon = New Field(colFields, .Name, .Type)
                                fldCommon.flgAutoInc = .AutoIncrement
                            End If

                            colFields.Add(fldCommon)
                        End If
                    End With
                End If
            Next

            ' Exit if no common field names were found
            If colFields.Count = 0 Then
                intOverallProgress += FromTable.RowCount
                Exit Sub
            End If

            intTotal = FromTable.RowCount
            RaiseEvent_RowProgress(FromTable.Name, 0, intTotal)
            RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)

            ' Setup to track to and from autoinc values if table has an identity field
            If tblSource.HasAutoIncField Then
                For Each fld In colFields
                    fldLookup = tblSource.Fields(fld.Name)
                    If Not fldLookup Is Nothing Then
                        With fldLookup
                            ' We need only track autoinc translations when field is referenced by foreign keys
                            If .AutoIncrement And .ForeignKeys.Count > 0 Then
                                ' Create a new hashtable to hold autoinc translations
                                .tblAutoIncTranslations = New Hashtable

                                ' Create a new autoinc field to hold source value
                                fldAutoInc = New Field(tblSource.Fields, fld.Name, .Type)
                                fldAutoInc.tblAutoIncTranslations = .tblAutoIncTranslations
                                Exit For
                            End If
                        End With
                    End If
                Next
            End If

            ' See if this table is a candidate for bulk inserts
            If flgAttemptBulkInsert Or flgForceBulkInsert Then
                With ToTable.Parent.Parent
                    ' We only attempt a bulk insert if the destination data source type is Sql Server and we are inserting
                    ' fields into a table that has no auto-incs with foreign key dependencies (or user forces procedure)
                    flgUseBulkInsert = flgForceBulkInsert Or (.DataSourceType = DatabaseType.SqlServer And (fldAutoInc Is Nothing Or colTables.Count = 1))
                    If flgUseBulkInsert Then
                        ParseBulkInsertSettings(strFldTerminator, strRowTerminator)
                        If Right(strBulkInsertFilePath, 1) <> "\" Then strBulkInsertFilePath &= "\"
                        strBulkInsertFile = strBulkInsertFilePath & GetTempFileName("bulkinsert")
                        fsBulkInsert = File.Create(strBulkInsertFile)
                    End If
                End With
            End If

            ' Execute source query
            With ExecuteReader("SELECT " & colFields.GetList() & " FROM " & FromTable.FullName, FromTable.Connection, CommandBehavior.SequentialAccess, Timeout)
                ' Create Sql stubs
                strInsertSqlStub = "INSERT INTO " & ToTable.FullName & " (" & colFields.GetList(False) & ") VALUES ("
                strUpdateSqlStub = "UPDATE " & ToTable.FullName & " SET "
                strCountSqlStub = "SELECT COUNT(*) AS Total FROM " & ToTable.FullName

                While .Read()
                    If flgUseBulkInsert Then
                        ' Handle creating bulk insert file data for each row...
                        strBulkInsertRow = New StringBuilder
                        flgAddedFirstInsert = False

                        ' Get all field data to create row for bulk insert
                        For Each fld In ToTable.Fields
                            Try
                                ' Lookup field in common field list
                                fldCommon = colFields(fld.Name)
                                If Not fldCommon Is Nothing Then
                                    ' Found it, so use it...
                                    fldCommon.Value = .Item(fld.Name)
                                Else
                                    ' Otherwise just use existing destination field
                                    fldCommon = fld
                                    fldCommon.Value = ""
                                End If
                            Catch ex As Exception
                                fldCommon.Value = ""
                                RaiseEvent_SqlFailure("Failed to get field value for [" & tblSource.Name & "." & fldCommon.Name & "]", ex)
                            End Try

                            ' Get translated auto-inc value for field if possible...
                            fldCommon.Value = DereferenceValue(tblSource, fldCommon.Name, fldCommon.Value)

                            ' Get field value
                            strValue = Trim(CStr(NotNull(fldCommon.Value)))

                            ' We manually parse data type here instead of using SqlEncodedValue because data inserted
                            ' into bulk insert file doesn't need Sql encoding...
                            Select Case fldCommon.Type
                                Case OleDbType.Boolean
                                    If Len(strValue) > 0 Then
                                        If IsNumeric(strValue) Then
                                            If CInt(Int(strValue)) = 0 Then
                                                strValue = "0"
                                            Else
                                                strValue = "1"
                                            End If
                                        ElseIf CBool(strValue) Then
                                            strValue = "1"
                                        Else
                                            Select Case UCase(Left(strValue, 1))
                                                Case "Y", "T"
                                                    strValue = "1"
                                                Case "N", "F"
                                                    strValue = "0"
                                                Case Else
                                                    strValue = "0"
                                            End Select
                                        End If
                                    End If
                                Case OleDbType.DBTimeStamp, OleDbType.DBDate, OleDbType.Date
                                    If Len(strValue) > 0 Then
                                        If IsDate(strValue) Then
                                            strValue = Format(CType(strValue, DateTime), "MM/dd/yyyy HH:mm:ss")
                                        End If
                                    End If
                                Case OleDbType.DBTime
                                    If Len(strValue) > 0 Then
                                        If IsDate(strValue) Then
                                            strValue = Format(CType(strValue, DateTime), "HH:mm:ss")
                                        End If
                                    End If
                            End Select

                            ' Make sure field value does not contain field terminator or row terminator
                            strValue = Replace(strValue, strFldTerminator, strDelimeterReplacement)
                            strValue = Replace(strValue, strRowTerminator, strDelimeterReplacement)

                            ' Construct bulk insert row
                            If flgAddedFirstInsert Then
                                strBulkInsertRow.Append(strFldTerminator)
                            Else
                                flgAddedFirstInsert = True
                            End If
                            strBulkInsertRow.Append(strValue)
                        Next

                        strBulkInsertRow.Append(strRowTerminator)

                        ' Add new row to temporary bulk insert file
                        bytDataRow = encBulkInsertEncoding.GetBytes(strBulkInsertRow.ToString())
                        fsBulkInsert.Write(bytDataRow, 0, bytDataRow.Length)
                    Else
                        ' Handle creating Sql for inserts or updates for each row...
                        strInsertSql = New StringBuilder(strInsertSqlStub)
                        strUpdateSql = New StringBuilder(strUpdateSqlStub)
                        strCountSql = New StringBuilder(strCountSqlStub)
                        strWhereSql = New StringBuilder
                        flgAddedFirstInsert = False
                        flgAddedFirstUpdate = False

                        ' Coerce all field data into proper Sql formats
                        For Each fld In colFields
                            Try
                                fld.Value = .Item(fld.Name)
                            Catch ex As Exception
                                fld.Value = ""
                                RaiseEvent_SqlFailure("Failed to get field value for [" & tblSource.Name & "." & fld.Name & "]", ex)
                            End Try

                            ' Get translated auto-inc value for field if necessary...
                            fld.Value = DereferenceValue(tblSource, fld.Name, fld.Value)

                            ' We don't attempt to insert values into auto-inc fields
                            If fld.AutoIncrement Then
                                If Not fldAutoInc Is Nothing Then
                                    ' Even if database supports multiple autoinc fields, we can only support automatic
                                    ' ID translation for one because the identity Sql can only return one value...
                                    If StrComp(fld.Name, fldAutoInc.Name, CompareMethod.Text) = 0 Then
                                        ' Track original autoinc value
                                        fldAutoInc.Value = fld.Value
                                    End If
                                End If
                            Else
                                ' Get Sql encoded field value
                                strValue = fld.SqlEncodedValue

                                ' Construct Sql statements
                                If flgAddedFirstInsert Then
                                    strInsertSql.Append(", ")
                                Else
                                    flgAddedFirstInsert = True
                                End If
                                strInsertSql.Append(strValue)

                                ' Check to see if this is a key field
                                fldLookup = tblSource.Fields(fld.Name)
                                If Not fldLookup Is Nothing Then
                                    flgIsPrimary = fldLookup.IsPrimaryKey
                                Else
                                    flgIsPrimary = False
                                End If

                                If StrComp(strValue, "NULL", CompareMethod.Text) <> 0 Then
                                    If flgIsPrimary Then
                                        If strWhereSql.Length = 0 Then
                                            strWhereSql.Append(" WHERE ")
                                        Else
                                            strWhereSql.Append(" AND ")
                                        End If

                                        strWhereSql.Append("[")
                                        strWhereSql.Append(fld.Name)
                                        strWhereSql.Append("] = ")
                                        strWhereSql.Append(strValue)
                                    Else
                                        If flgAddedFirstUpdate Then
                                            strUpdateSql.Append(", ")
                                        Else
                                            flgAddedFirstUpdate = True
                                        End If

                                        strUpdateSql.Append("[")
                                        strUpdateSql.Append(fld.Name)
                                        strUpdateSql.Append("] = ")
                                        strUpdateSql.Append(strValue)
                                    End If
                                End If
                            End If
                        Next

                        strInsertSql.Append(")")

                        If Not fldAutoInc Is Nothing Or strWhereSql.Length = 0 Then
                            ' For tables with autoinc fields that are referenced by foreign keys or
                            ' tables that have no primary key fields defined, we can only do inserts...
                            Try
                                ' Insert record into destination table
                                If flgAddedFirstInsert Or Not fldAutoInc Is Nothing Then ExecuteNonQuery(strInsertSql.ToString(), ToTable.Connection, Timeout)

                                ' Save new destination autoinc value
                                If Not fldAutoInc Is Nothing Then
                                    Try
                                        fldAutoInc.tblAutoIncTranslations.Add(CStr(fldAutoInc.Value), ExecuteScalar(tblSource.IdentitySql, ToTable.Connection, Timeout))
                                    Catch ex As Exception
                                        RaiseEvent_SqlFailure(tblSource.IdentitySql, ex)
                                    End Try
                                End If
                            Catch ex As Exception
                                RaiseEvent_SqlFailure(strInsertSql.ToString(), ex)
                            End Try
                        Else
                            ' Add where criteria to Sql count statement
                            strCountSql.Append(strWhereSql.ToString())

                            ' Make sure record doesn't already exist
                            Try
                                ' If record already exists due to triggers or other means we must update it instead of inserting it
                                If NotNull(ExecuteScalar(strCountSql.ToString(), ToTable.Connection, Timeout), 0) > 0 Then
                                    ' Add where criteria to Sql update statement
                                    strUpdateSql.Append(strWhereSql.ToString())

                                    Try
                                        ' Update record in destination table
                                        If flgAddedFirstUpdate Then ExecuteNonQuery(strUpdateSql.ToString(), ToTable.Connection, Timeout)
                                    Catch ex As Exception
                                        RaiseEvent_SqlFailure(strUpdateSql.ToString(), ex)
                                    End Try
                                Else
                                    Try
                                        ' Insert record into destination table
                                        If flgAddedFirstInsert Then ExecuteNonQuery(strInsertSql.ToString(), ToTable.Connection, Timeout)
                                    Catch ex As Exception
                                        RaiseEvent_SqlFailure(strInsertSql.ToString(), ex)
                                    End Try
                                End If
                            Catch ex As Exception
                                RaiseEvent_SqlFailure(strCountSql.ToString(), ex)
                            End Try
                        End If
                    End If

                    intProgress += 1
                    intOverallProgress += 1
                    If intProgress Mod RowReportInterval = 0 Then
                        RaiseEvent_RowProgress(FromTable.Name, intProgress, intTotal)
                        RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)
                    End If
                End While

                .Close()
            End With

            If flgUseBulkInsert And Not fsBulkInsert Is Nothing Then
                Dim strBulkInsertSql As String = "BULK INSERT " & ToTable.FullName & " FROM '" & strBulkInsertFile & "'" & IIf(Len(strBulkInsertSettings) > 0, " WITH (" & strBulkInsertSettings & ")", "")
                Dim dblStartTime As Double
                Dim dblStopTime As Double

                ' Close bulk insert file stream
                fsBulkInsert.Close()
                fsBulkInsert = Nothing

                RaiseEvent BulkInsertExecuting(ToTable.Name)

                Try
                    ' Give system a few seconds to close bulk insert file (might have been real big)
                    WaitForReadLock(strBulkInsertFile, 15)
                    dblStartTime = VB.Timer
                    ExecuteNonQuery(strBulkInsertSql, ToTable.Connection, Timeout * intProgress)
                Catch ex As Exception
                    RaiseEvent BulkInsertException(ToTable.Name, strBulkInsertSql, ex)
                Finally
                    dblStopTime = VB.Timer
                    If CInt(dblStartTime) = 0 Then dblStartTime = dblStopTime
                End Try

                Try
                    WaitForWriteLock(strBulkInsertFile, 15)
                    File.Delete(strBulkInsertFile)
                Catch ex As Exception
                    RaiseEvent BulkInsertException(ToTable.Name, strBulkInsertSql, New InvalidOperationException("Failed to delete temporary bulk insert file """ & strBulkInsertFile & """ due to exception [" & ex.Message & "], file will need to be manually deleted.", ex))
                End Try

                RaiseEvent BulkInsertCompleted(ToTable.Name, intProgress, CInt(dblStopTime - dblStartTime))
            End If

            RaiseEvent_RowProgress(FromTable.Name, intTotal, intTotal)
            RaiseEvent_OverallProgress(intOverallProgress, intOverallTotal)

        End Sub

        Friend Function DereferenceValue(ByVal SourceTable As Table, ByVal FieldName As String, ByVal Value As Object, Optional ByVal FieldStack As ArrayList = Nothing) As Object

            Dim fldLookup As Field
            Dim objValue As Object

            ' No need to attempt to deference null value
            If IsDBNull(Value) Or Value Is Nothing Then Return Value

            ' If this field is referenced as a foreign key field by a primary key field that is auto-incremented, we
            ' translate the auto-inc value if possible
            fldLookup = SourceTable.Fields(FieldName)
            If Not fldLookup Is Nothing Then
                With fldLookup
                    If .IsForeignKey Then
                        With .ReferencedBy
                            If .AutoIncrement Then
                                ' Return new auto-inc value
                                If .tblAutoIncTranslations Is Nothing Then
                                    Return Value
                                Else
                                    objValue = .tblAutoIncTranslations(CStr(Value))
                                    Return IIf(objValue Is Nothing, Value, objValue)
                                End If
                            Else
                                Dim flgInStack As Boolean
                                Dim x As Integer

                                If FieldStack Is Nothing Then FieldStack = New ArrayList

                                ' We don't want to circle back on ourselves
                                For x = 0 To FieldStack.Count - 1
                                    If fldLookup.ReferencedBy Is FieldStack(x) Then
                                        flgInStack = True
                                        Exit For
                                    End If
                                Next

                                ' Traverse path to parent autoinc field if it exists
                                If Not flgInStack Then
                                    FieldStack.Add(fldLookup.ReferencedBy)
                                    Return DereferenceValue(.Table, .Name, Value, FieldStack)
                                End If
                            End If
                        End With
                    End If
                End With
            End If

            Return Value

        End Function

        Private Sub ParseBulkInsertSettings(ByRef FieldTerminator As String, ByRef RowTerminator As String)

            Dim strSetting As String
            Dim strKeyValue As String()

            FieldTerminator = ""
            RowTerminator = ""

            For Each strSetting In strBulkInsertSettings.Split(",")
                strKeyValue = strSetting.Split("=")
                If strKeyValue.Length = 2 Then
                    If StrComp(Trim(strKeyValue(0)), "FIELDTERMINATOR", CompareMethod.Text) = 0 Then
                        FieldTerminator = Trim(strKeyValue(1))
                    ElseIf StrComp(Trim(strKeyValue(0)), "ROWTERMINATOR", CompareMethod.Text) = 0 Then
                        RowTerminator = Trim(strKeyValue(1))
                    End If
                End If
            Next

            If Len(FieldTerminator) = 0 Then FieldTerminator = "\t"
            If Len(RowTerminator) = 0 Then RowTerminator = "\n"

            FieldTerminator = UnEncodeSetting(FieldTerminator)
            RowTerminator = UnEncodeSetting(RowTerminator)

        End Sub

        Private Function UnEncodeSetting(ByVal Setting As String) As String

            Setting = RemoveQuotes(Setting)
            Setting = Replace(Setting, "\\", "\")
            Setting = Replace(Setting, "\'", "'")
            Setting = Replace(Setting, "\""", """")
            Setting = Replace(Setting, "\t", vbTab)
            Setting = Replace(Setting, "\n", vbCrLf)

            Return Setting

        End Function

        Private Function RemoveQuotes(ByVal Str As String) As String

            If Left(Str, 1) = "'" Then Str = Mid(Str, 2)
            If Right(Str, 1) = "'" Then Str = Mid(Str, 1, Len(Str) - 1)

            Return Str

        End Function

    End Class

End Namespace