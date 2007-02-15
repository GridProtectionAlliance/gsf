' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Data
Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports System.Text
Imports TVA.Shared.String
Imports TVA.Database.Common

Namespace Database

    Public Enum DatabaseType
        [SqlServer]
        [Oracle]
        [Access]
        [Unspecified]
    End Enum

    <Flags()> _
    Public Enum TableType
        [Table] = 1
        [View] = 2
        [SystemTable] = 4
        [SystemView] = 8
        [Alias] = 16
        [Synonym] = 32
        [GlobalTemp] = 64
        [LocalTemp] = 128
        [Link] = 256
        [Undetermined] = 512
    End Enum

    Public Enum ReferentialAction
        [Cascade]
        [SetNull]
        [SetDefault]
        [NoAction]
    End Enum

    Public Class Field

        Implements IComparable

        Private objParent As Fields
        Private strName As String
        Private dteType As OleDbType
        Friend intOrdinal As Integer
        Friend flgAllowsNulls As Boolean
        Friend flgAutoInc As Boolean
        Friend intAutoIncSeed As Integer
        Friend intAutoIncStep As Integer
        Friend flgHasDefault As Boolean
        Friend objDefaultValue As Object
        Friend intMaxLength As Integer
        Friend intNumericPrecision As Integer
        Friend intNumericScale As Integer
        Friend intDateTimePrecision As Integer
        Friend flgReadOnly As Boolean
        Friend flgUnique As Boolean
        Friend strDescription As String
        Friend tblAutoIncTranslations As Hashtable

        Public Value As Object                              ' This allows user to store a field value if desired
        Public IsPrimaryKey As Boolean
        Public PrimaryKeyOrdinal As Integer
        Public PrimaryKeyName As String
        Public ForeignKeys As ForeignKeyFields
        Public ReferencedBy As Field

        Friend Sub New(ByVal Parent As Fields, ByVal [Name] As String, ByVal [Type] As OleDbType)

            ' We only allow internal creation of this object
            objParent = Parent
            strName = [Name]
            dteType = [Type]
            ForeignKeys = New ForeignKeyFields(Me)

        End Sub

        Public ReadOnly Property [Name]() As String
            Get
                Return strName
            End Get
        End Property

        Public ReadOnly Property [Type]() As OleDbType
            Get
                Return dteType
            End Get
        End Property

        Public ReadOnly Property Ordinal() As Integer
            Get
                Return intOrdinal
            End Get
        End Property

        Public ReadOnly Property AllowsNulls() As Boolean
            Get
                Return flgAllowsNulls
            End Get
        End Property

        Public ReadOnly Property AutoIncrement() As Boolean
            Get
                Return flgAutoInc
            End Get
        End Property

        Public ReadOnly Property AutoIncrementSeed() As Integer
            Get
                Return intAutoIncSeed
            End Get
        End Property

        Public ReadOnly Property AutoIncrementStep() As Integer
            Get
                Return intAutoIncStep
            End Get
        End Property

        Public ReadOnly Property HasDefault() As Boolean
            Get
                Return flgHasDefault
            End Get
        End Property

        Public ReadOnly Property DefaultValue() As Object
            Get
                Return objDefaultValue
            End Get
        End Property

        Public ReadOnly Property MaxLength() As Integer
            Get
                Return intMaxLength
            End Get
        End Property

        Public ReadOnly Property NumericPrecision() As Integer
            Get
                Return intNumericPrecision
            End Get
        End Property

        Public ReadOnly Property NumericScale() As Integer
            Get
                Return intNumericScale
            End Get
        End Property

        Public ReadOnly Property DateTimePrecision() As Integer
            Get
                Return intDateTimePrecision
            End Get
        End Property

        Public ReadOnly Property [ReadOnly]() As Boolean
            Get
                Return flgReadOnly
            End Get
        End Property

        Public ReadOnly Property Unique() As Boolean
            Get
                Return flgUnique
            End Get
        End Property

        Public ReadOnly Property Description() As String
            Get
                Return strDescription
            End Get
        End Property

        Public ReadOnly Property IsForeignKey() As Boolean
            Get
                Return Not ReferencedBy Is Nothing
            End Get
        End Property

        Public ReadOnly Property Parent() As Fields
            Get
                Return objParent
            End Get
        End Property

        Public ReadOnly Property Table() As Table
            Get
                Return objParent.Parent
            End Get
        End Property

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' Fields are sorted in ordinal position order
            If TypeOf obj Is Field Then
                Return intOrdinal.CompareTo(DirectCast(obj, Field).intOrdinal)
            Else
                Throw New ArgumentException("Field can only be compared to other Fields")
            End If

        End Function

        Public ReadOnly Property SqlEncodedValue() As String
            Get
                Dim strValue As String

                If Not IsDBNull(Value) Then
                    Try
                        ' Attempt to get string based source field value
                        strValue = Trim(CStr(Value))

                        ' Format field value based on field's data type
                        Select Case Type
                            Case OleDbType.BigInt, OleDbType.Integer, OleDbType.SmallInt, OleDbType.TinyInt, _
                                    OleDbType.UnsignedBigInt, OleDbType.UnsignedInt, OleDbType.UnsignedSmallInt, _
                                    OleDbType.UnsignedTinyInt, OleDbType.Error
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "0"
                                    End If
                                Else
                                    If IsNumeric(Value) Then
                                        strValue = CLng(Value).ToString().Trim()
                                    Else
                                        If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                            strValue = "NULL"
                                        Else
                                            strValue = "0"
                                        End If
                                    End If
                                End If
                            Case OleDbType.Single
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "0.0"
                                    End If
                                Else
                                    If IsNumeric(Value) Then
                                        strValue = CSng(Value).ToString().Trim()
                                    Else
                                        If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                            strValue = "NULL"
                                        Else
                                            strValue = "0.0"
                                        End If
                                    End If
                                End If
                            Case OleDbType.Double
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "0.0"
                                    End If
                                Else
                                    If IsNumeric(Value) Then
                                        strValue = CDbl(Value).ToString().Trim()
                                    Else
                                        If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                            strValue = "NULL"
                                        Else
                                            strValue = "0.0"
                                        End If
                                    End If
                                End If
                            Case OleDbType.Currency, OleDbType.Decimal, OleDbType.Numeric, OleDbType.VarNumeric
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "0.00"
                                    End If
                                Else
                                    If IsNumeric(Value) Then
                                        strValue = CDec(Value).ToString().Trim()
                                    Else
                                        If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                            strValue = "NULL"
                                        Else
                                            strValue = "0.00"
                                        End If
                                    End If
                                End If
                            Case OleDbType.Boolean
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "0"
                                    End If
                                Else
                                    If IsNumeric(strValue) Then
                                        If Value = 0 Then
                                            strValue = "0"
                                        Else
                                            strValue = "1"
                                        End If
                                    Else
                                        Select Case Char.ToUpper(strValue.Trim.Chars(0))
                                            Case "Y"c, "T"c
                                                strValue = "1"
                                            Case "N"c, "F"c
                                                strValue = "0"
                                            Case Else
                                                If objParent.Parent.Parent.Parent.AllowNumericNulls Then
                                                    strValue = "NULL"
                                                Else
                                                    strValue = "0"
                                                End If
                                        End Select
                                    End If
                                End If
                            Case OleDbType.Char, OleDbType.WChar, OleDbType.VarChar, OleDbType.VarWChar, OleDbType.LongVarChar, OleDbType.LongVarWChar, OleDbType.BSTR
                                If Len(strValue) = 0 Then
                                    If objParent.Parent.Parent.Parent.AllowTextNulls Then
                                        strValue = "NULL"
                                    Else
                                        strValue = "''"
                                    End If
                                Else
                                    strValue = "'" & SqlEncode(strValue) & "'"
                                End If
                            Case OleDbType.DBTimeStamp, OleDbType.DBDate, OleDbType.Date
                                If Len(strValue) > 0 Then
                                    If IsDate(strValue) Then
                                        Select Case objParent.Parent.Parent.Parent.DataSourceType
                                            Case DatabaseType.SqlServer
                                                strValue = "'" & Format(CType(strValue, DateTime), "MM/dd/yyyy HH:mm:ss") & "'"
                                            Case DatabaseType.Oracle
                                                strValue = "'" & Format(CType(strValue, DateTime), "dd-MMM-yyyy HH:mm:ss") & "'"
                                            Case DatabaseType.Access
                                                strValue = "#" & Format(CType(strValue, DateTime), "MM/dd/yyyy HH:mm:ss") & "#"
                                            Case Else
                                                strValue = "'" & Format(CType(strValue, DateTime), "MM/dd/yyyy HH:mm:ss") & "'"
                                        End Select
                                    Else
                                        strValue = "NULL"
                                    End If
                                Else
                                    strValue = "NULL"
                                End If
                            Case OleDbType.DBTime
                                If Len(strValue) > 0 Then
                                    If IsDate(strValue) Then
                                        strValue = "'" & Format(CType(strValue, DateTime), "HH:mm:ss") & "'"
                                    Else
                                        strValue = "NULL"
                                    End If
                                Else
                                    strValue = "NULL"
                                End If
                            Case OleDbType.Filetime
                                If Len(strValue) > 0 Then
                                    strValue = "'" & strValue & "'"
                                Else
                                    strValue = "NULL"
                                End If
                        End Select
                    Catch ex As Exception
                        ' We'll default to NULL if we failed to evaluate field data
                        strValue = "NULL"
                    End Try
                End If

                If Len(strValue) = 0 Then strValue = "NULL"

                Return strValue
            End Get
        End Property

        Friend Shared Function GetReferentialAction(ByVal RefAction As String) As ReferentialAction

            Select Case UCase(Trim(RefAction))
                Case "CASCADE"
                    Return ReferentialAction.Cascade
                Case "SET NULL"
                    Return ReferentialAction.SetNull
                Case "SET DEFAULT"
                    Return ReferentialAction.SetDefault
                Case "NO ACTION"
                    Return ReferentialAction.NoAction
                Case Else
                    Return ReferentialAction.NoAction
            End Select

        End Function

    End Class

    Public Class ForeignKeyField

        Private objParent As ForeignKeyFields

        Public PrimaryKey As Field
        Public ForeignKey As Field
        Public Ordinal As Integer
        Public KeyName As String
        Public UpdateRule As ReferentialAction = ReferentialAction.NoAction
        Public DeleteRule As ReferentialAction = ReferentialAction.NoAction

        Friend Sub New(ByVal Parent As ForeignKeyFields)

            ' We only allow internal creation of this object
            objParent = Parent

        End Sub

        Public ReadOnly Property Parent() As ForeignKeyFields
            Get
                Return objParent
            End Get
        End Property

    End Class

    Public Class ForeignKeyFields

        Private objParent As Field
        Friend tblFields As Hashtable   ' Used for field name lookups
        Friend lstFields As ArrayList   ' Used for field index lookups

        Friend Sub New(ByVal Parent As Field)

            ' We only allow internal creation of this object
            objParent = Parent
            tblFields = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
            lstFields = New ArrayList()

        End Sub

        Friend Sub Add(ByVal NewField As ForeignKeyField)

            lstFields.Add(NewField)
            tblFields.Add(IIf(Len(NewField.KeyName) > 0, NewField.KeyName, "FK" & lstFields.Count), NewField)

        End Sub

        Default Public ReadOnly Property Item(ByVal Index As Integer) As ForeignKeyField
            Get
                If Index < 0 Or Index >= lstFields.Count Then
                    Return Nothing
                Else
                    Return DirectCast(lstFields(Index), ForeignKeyField)
                End If
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal Name As String) As ForeignKeyField
            Get
                Return DirectCast(tblFields(Name), ForeignKeyField)
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator

            Return lstFields.GetEnumerator()

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return lstFields.Count
            End Get
        End Property

        Public ReadOnly Property Parent() As Field
            Get
                Return objParent
            End Get
        End Property

        Public Function GetList() As String

            With New StringBuilder
                For Each fld As Field In lstFields
                    If .Length > 0 Then .Append(","c)
                    .Append("["c)
                    .Append(fld.Name)
                    .Append("]"c)
                Next

                Return .ToString
            End With

        End Function

    End Class

    Public Class Fields

        Private objParent As Table
        Friend tblFields As Hashtable   ' Used for field name lookups
        Friend lstFields As ArrayList   ' Used for field index lookups

        Friend Sub New(ByVal Parent As Table)

            ' We only allow internal creation of this object
            objParent = Parent
            tblFields = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
            lstFields = New ArrayList()

        End Sub

        Friend Sub Add(ByVal NewField As Field)

            tblFields.Add(NewField.Name, NewField)
            lstFields.Add(NewField)

        End Sub

        Default Public ReadOnly Property Item(ByVal Index As Integer) As Field
            Get
                If Index < 0 Or Index >= lstFields.Count Then
                    Return Nothing
                Else
                    Return DirectCast(lstFields(Index), Field)
                End If
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal Name As String) As Field
            Get
                Return DirectCast(tblFields(Name), Field)
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator

            Return lstFields.GetEnumerator()

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return lstFields.Count
            End Get
        End Property

        Public ReadOnly Property Parent() As Table
            Get
                Return objParent
            End Get
        End Property

        Public Function GetList(Optional ByVal ReturnAutoInc As Boolean = True) As String

            With New StringBuilder
                For Each fld As Field In lstFields
                    If Not fld.AutoIncrement Or ReturnAutoInc Then
                        If .Length > 0 Then .Append(","c)
                        .Append("["c)
                        .Append(fld.Name)
                        .Append("]"c)
                    End If
                Next

                Return .ToString
            End With

        End Function

    End Class

    Public Class Table

        Implements IComparable

        Private objParent As Tables
        Private strCatalog As String
        Private strSchema As String
        Private strName As String
        Private tteType As TableType
        Private strDescription As String
        Private intRows As Integer

        Public Fields As Fields
        Public MapName As String                            ' This is the name that will be used during table mapping when using a data handler
        Public Process As Boolean                           ' This flag allows users to override whether or not table will be processed in a data operation
        Public Priority As Integer                          ' This is the user-overridable I/O process priority for a table
        Public IdentitySql As String = "SELECT @@IDENTITY"  ' User definable Sql used to retrieve value from identity column

        Friend Sub New(ByVal Parent As Tables, ByVal Catalog As String, ByVal Schema As String, ByVal Name As String, ByVal Type As String, ByVal Description As String, ByVal Rows As Integer)

            ' We only allow internal creation of this object
            objParent = Parent
            Fields = New Fields(Me)

            strCatalog = Catalog
            strSchema = Schema
            strName = Name
            MapName = Name
            strDescription = Description

            Select Case UCase(Trim(Type))
                Case "TABLE"
                    tteType = TableType.Table
                Case "VIEW"
                    tteType = TableType.View
                Case "SYSTEM TABLE"
                    tteType = TableType.SystemTable
                Case "SYSTEM VIEW"
                    tteType = TableType.SystemView
                Case "ALIAS"
                    tteType = TableType.Alias
                Case "SYNONYM"
                    tteType = TableType.Synonym
                Case "GLOBAL TEMPORARY"
                    tteType = TableType.GlobalTemp
                Case "LOCAL TEMPORARY"
                    tteType = TableType.LocalTemp
                Case "LINK"
                    tteType = TableType.Link
                Case Else
                    tteType = TableType.Undetermined
            End Select

            If Rows = 0 And tteType = TableType.Table Then
                Try
                    intRows = ExecuteScalar("SELECT COUNT(*) AS Total FROM " & FullName, objParent.Parent.Connection)
                Catch
                    intRows = 0
                End Try
            Else
                intRows = Rows
            End If

        End Sub

        Public ReadOnly Property [Name]() As String
            Get
                Return strName
            End Get
        End Property

        Public ReadOnly Property FullName() As String
            Get
                Dim strFullName As String

                If Len(strCatalog) > 0 Then strFullName &= "[" & strCatalog & "]."
                If Len(strSchema) > 0 Then strFullName &= "[" & strSchema & "]."
                strFullName &= "[" & strName & "]"

                Return strFullName
            End Get
        End Property

        Public ReadOnly Property [Catalog]() As String
            Get
                Return strCatalog
            End Get
        End Property

        Public ReadOnly Property [Schema]() As String
            Get
                Return strSchema
            End Get
        End Property

        Public Function UsesDefaultSchema() As Boolean

            If Parent.Parent.DataSourceType = DatabaseType.SqlServer Then
                Return (StrComp(strSchema, "dbo", CompareMethod.Text) = 0)
            Else
                Return (Len(Schema) = 0)
            End If

        End Function

        Public ReadOnly Property [Type]() As TableType
            Get
                Return tteType
            End Get
        End Property

        Public ReadOnly Property Description() As String
            Get
                Return strDescription
            End Get
        End Property

        Public ReadOnly Property RowCount() As Integer
            Get
                Return intRows
            End Get
        End Property

        Public ReadOnly Property Parent() As Tables
            Get
                Return objParent
            End Get
        End Property

        Public ReadOnly Property Connection() As OleDbConnection
            Get
                Return objParent.Parent.Connection
            End Get
        End Property

        Public ReadOnly Property IsView() As Boolean
            Get
                Return (tteType = TableType.View Or tteType = TableType.SystemView)
            End Get
        End Property

        Public ReadOnly Property IsSystem() As Boolean
            Get
                Return (tteType = TableType.SystemTable Or tteType = TableType.SystemView)
            End Get
        End Property

        Public ReadOnly Property IsTemporary() As Boolean
            Get
                Return (tteType = TableType.GlobalTemp Or tteType = TableType.LocalTemp)
            End Get
        End Property

        Public ReadOnly Property IsLinked() As Boolean
            Get
                Return (tteType = TableType.Alias Or tteType = TableType.Link)
            End Get
        End Property

        Public ReadOnly Property PrimaryKeyFieldCount() As Integer
            Get
                Dim iCount As Integer

                For Each fld As Field In Fields
                    If fld.IsPrimaryKey Then iCount += 1
                Next

                Return iCount
            End Get
        End Property

        Public ReadOnly Property IsForeignKeyTable() As Boolean
            Get
                For Each fld As Field In Fields
                    If fld.IsForeignKey Then Return True
                Next
            End Get
        End Property

        Public ReadOnly Property HasAutoIncField() As Boolean
            Get
                For Each fld As Field In Fields
                    If fld.AutoIncrement Then Return True
                Next
            End Get
        End Property

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

            ' Tables are sorted in priority order
            If TypeOf obj Is Table Then
                Return Priority.CompareTo(DirectCast(obj, Table).Priority)
            Else
                Throw New ArgumentException("Table can only be compared to other Tables")
            End If

        End Function

        Friend Function ReferencedBy(ByVal OtherTable As Table, ByVal TableStack As ArrayList) As Boolean

            Dim tbl As Table
            Dim flgInStack As Boolean

            If TableStack Is Nothing Then TableStack = New ArrayList()
            TableStack.Add(Me)

            For Each fld As Field In Fields
                If fld.IsForeignKey Then
                    ' We don't want to circle back on ourselves
                    tbl = fld.ReferencedBy.Table
                    flgInStack = False

                    For x As Integer = 0 To TableStack.Count - 1
                        If tbl Is TableStack(x) Then
                            flgInStack = True
                            Exit For
                        End If
                    Next

                    If flgInStack Then
                        If StrComp(tbl.Name, OtherTable.Name, CompareMethod.Text) = 0 Then
                            Return True
                        End If
                    Else
                        If tbl.ReferencedBy(OtherTable, TableStack) Then
                            Return True
                        ElseIf StrComp(tbl.Name, OtherTable.Name, CompareMethod.Text) = 0 Then
                            Return True
                        End If
                    End If
                End If
            Next

        End Function

        Public Function ReferencedBy(ByVal OtherTable As Table) As Boolean

            Return ReferencedBy(OtherTable, Nothing)

        End Function

        Public Function DefinePrimaryKey(ByVal FieldName As String, Optional ByVal PrimaryKeyOrdinal As Integer = -1, Optional ByVal PrimaryKeyName As String = "") As Boolean

            Dim fldLookup As Field

            fldLookup = Fields(FieldName)
            If Not fldLookup Is Nothing Then
                With fldLookup
                    .IsPrimaryKey = True
                    .PrimaryKeyOrdinal = IIf(PrimaryKeyOrdinal = -1, PrimaryKeyFieldCount + 1, PrimaryKeyOrdinal)
                    .PrimaryKeyName = PrimaryKeyName
                    Return True
                End With
            End If

        End Function

        Public Function DefineForeignKey(ByVal PrimaryKeyFieldName As String, ByVal ForeignKeyTableName As String, ByVal ForeignKeyFieldName As String, Optional ByVal ForeignKeyOrdinal As Integer = -1, Optional ByVal ForeignKeyName As String = "", Optional ByVal ForeignKeyUpdateRule As ReferentialAction = ReferentialAction.NoAction, Optional ByVal ForeignKeyDeleteRule As ReferentialAction = ReferentialAction.NoAction) As Boolean

            Dim fldLookup As Field
            Dim tblLookup As Table
            Dim fldParentLookup As Field

            fldLookup = Fields(PrimaryKeyFieldName)
            If Not fldLookup Is Nothing Then
                tblLookup = objParent(ForeignKeyTableName)
                If Not tblLookup Is Nothing Then
                    fldParentLookup = tblLookup.Fields(ForeignKeyFieldName)
                    If Not fldParentLookup Is Nothing Then
                        Dim fkFld As New ForeignKeyField(fldLookup.ForeignKeys)
                        With fkFld
                            .PrimaryKey = fldLookup
                            .ForeignKey = fldParentLookup
                            .ForeignKey.ReferencedBy = .PrimaryKey
                            .Ordinal = IIf(ForeignKeyOrdinal = -1, fldLookup.ForeignKeys.Count + 1, ForeignKeyOrdinal)
                            .KeyName = ForeignKeyName
                            .UpdateRule = ForeignKeyUpdateRule
                            .DeleteRule = ForeignKeyDeleteRule
                        End With
                        fldLookup.ForeignKeys.Add(fkFld)
                        Return True
                    End If
                End If
            End If

        End Function

    End Class

    Public Class Tables

        Private objParent As Schema
        Friend tblTables As Hashtable   ' Used for table name lookups
        Friend lstTables As ArrayList   ' Used for table index lookups

        Friend Sub New(ByVal Parent As Schema)

            ' We only allow internal creation of this object
            objParent = Parent
            tblTables = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
            lstTables = New ArrayList()

        End Sub

        Friend Sub Add(ByVal NewTable As Table)

            tblTables.Add(NewTable.Name, NewTable)
            lstTables.Add(NewTable)

        End Sub

        Default Public ReadOnly Property Item(ByVal Index As Integer) As Table
            Get
                If Index < 0 Or Index >= lstTables.Count Then
                    Return Nothing
                Else
                    Return DirectCast(lstTables(Index), Table)
                End If
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal Name As String) As Table
            Get
                Return DirectCast(tblTables(Name), Table)
            End Get
        End Property

        Public Function FindByMapName(ByVal MapName As String) As Table

            For Each tbl As Table In lstTables
                If String.Compare(tbl.MapName, MapName, True) = 0 Then
                    Return tbl
                End If
            Next

            Return Nothing

        End Function

        Public Function GetEnumerator() As IEnumerator

            Return lstTables.GetEnumerator()

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return lstTables.Count
            End Get
        End Property

        Public ReadOnly Property Parent() As Schema
            Get
                Return objParent
            End Get
        End Property

        Public Function GetList() As String

            With New StringBuilder
                For Each tbl As Table In lstTables
                    If .Length > 0 Then .Append(","c)
                    .Append(tbl.FullName)
                Next

                Return .ToString
            End With

        End Function

        Public Class ReferentialOrderComparer

            Implements IComparer

            Public Shared ReadOnly Property [Default]() As ReferentialOrderComparer
                Get
                    Static rocDefault As New ReferentialOrderComparer()
                    Return rocDefault
                End Get
            End Property

            Public Function Compare(ByVal Table1 As Object, ByVal Table2 As Object) As Integer Implements IComparer.Compare

                ' This function allows tables to be sorted in proper referential integrity process order
                If TypeOf Table1 Is Table And TypeOf Table2 Is Table Then
                    Dim tbl1 As Table = DirectCast(Table1, Table)
                    Dim tbl2 As Table = DirectCast(Table2, Table)
                    Dim intCompare As Integer

                    If tbl1 Is tbl2 Then Return 0

                    If AutoIncCompare(tbl1, tbl2) = 0 Then
                        ' Either both tables have or don't have autoincs, sort in foreign key order
                        intCompare = ForeignKeyCompare(tbl1, tbl2)
                    Else
                        If tbl1.HasAutoIncField Then
                            If tbl1.ReferencedBy(tbl2) Then
                                ' Table1 is referenced by Table2, sort it below
                                intCompare = 1
                            Else
                                ' Otherwise, autoincs should process highest in the list
                                intCompare = -1
                            End If
                        ElseIf tbl2.HasAutoIncField Then
                            If tbl2.ReferencedBy(tbl1) Then
                                ' Table2 is referenced by Table1, sort it below
                                intCompare = -1
                            Else
                                ' Otherwise, autoincs should process highest in the list
                                intCompare = 1
                            End If
                        End If
                    End If

                    ' Last sort will be based on table name
                    Return IIf(intCompare = 0, Table1.Name.CompareTo(Table2.Name), intCompare)
                Else
                    Throw New ArgumentException("Table can only be compared to other Tables")
                End If

            End Function

            Private Function ForeignKeyCompare(ByVal tbl1 As Table, ByVal tbl2 As Table) As Integer

                If tbl1.IsForeignKeyTable And tbl2.IsForeignKeyTable Then
                    ' Both tables have foreign keys so if tables have a relationship,
                    ' table with foreign key reference must fall below
                    If tbl1.ReferencedBy(tbl2) Then
                        ' Table1 is referenced by Table2, sort it below
                        Return 1
                    ElseIf tbl2.ReferencedBy(tbl1) Then
                        ' Table2 is referenced by Table1, sort it below
                        Return -1
                    Else
                        ' If neither table references the other, consider them equal
                        Return 0
                    End If
                ElseIf Not tbl1.IsForeignKeyTable And Not tbl2.IsForeignKeyTable Then
                    ' Neither table has foreign key fields, consider them equal
                    Return 0
                ElseIf tbl1.IsForeignKeyTable Then
                    ' Table1 has foreign key fields and Table2 does not, sort it below
                    Return 1
                Else 'If tbl2.IsForeignKeyTable Then
                    ' Table2 has foreign key fields and Table1 does not, sort it below
                    Return -1
                End If

            End Function

            ' We compare based on the existance of AutoInc fields as a secondary compare in case user
            ' has no defined relational integrity - lastly we just sort by table name
            Private Function AutoIncCompare(ByVal tbl1 As Table, ByVal tbl2 As Table) As Integer

                Return IIf(CInt(tbl1.HasAutoIncField) = CInt(tbl2.HasAutoIncField), 0, IIf(tbl1.HasAutoIncField, -1, 1))

            End Function

        End Class

    End Class

    <ToolboxBitmap(GetType(Schema), "Schema.bmp"), DefaultProperty("SourceSchema")> _
    Public Class Schema

        Inherits Component

        <Browsable(False)> Public Tables As Tables
        Public Const NoRestriction As TableType = TableType.[Table] Or TableType.[View] Or TableType.[SystemTable] Or TableType.[SystemView] Or TableType.[Alias] Or TableType.[Synonym] Or TableType.[GlobalTemp] Or TableType.[LocalTemp] Or TableType.[Link] Or TableType.[Undetermined]

        Private cnnSchema As OleDbConnection
        Private strConnectString As String
        Private typDataSource As DatabaseType
        Private intRestriction As TableType
        Private flgImmediateClose As Boolean
        Private flgAllowTextNulls As Boolean
        Private flgAllowNumericNulls As Boolean

        <Browsable(True), Category("Configuration"), Description("OLEDB connection string to datasource to analyze.")> _
        Public Property ConnectString() As String
            Get
                Return strConnectString
            End Get
            Set(ByVal Value As String)
                strConnectString = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set this value to restrict the types of tables returned in your schema.  Table types can be OR'd together to create this table type restriction."), DefaultValue(NoRestriction)> _
        Public Property TableTypeRestriction() As TableType
            Get
                Return intRestriction
            End Get
            Set(ByVal Value As TableType)
                intRestriction = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set this value to False to keep the schema connection used during analysis open after analysis is complete."), DefaultValue(True)> _
        Public Property ImmediateClose() As Boolean
            Get
                Return flgImmediateClose
            End Get
            Set(ByVal Value As Boolean)
                flgImmediateClose = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Encoding"), Description("Type of database specified in connect string."), DefaultValue(DatabaseType.Unspecified)> _
        Public Property DataSourceType() As DatabaseType
            Get
                Return typDataSource
            End Get
            Set(ByVal Value As DatabaseType)
                typDataSource = Value
            End Set
        End Property

        <Browsable(True), Category("Sql Encoding"), Description("Set this value to False to convert all Null values encountered in character fields to empty strings."), DefaultValue(False)> _
        Public Property AllowTextNulls() As Boolean
            Get
                Return flgAllowTextNulls
            End Get
            Set(ByVal Value As Boolean)
                flgAllowTextNulls = True
            End Set
        End Property

        <Browsable(True), Category("Sql Encoding"), Description("Set this value to False to convert all Null values encountered in numeric fields to zeros."), DefaultValue(False)> _
        Public Property AllowNumericNulls() As Boolean
            Get
                Return flgAllowNumericNulls
            End Get
            Set(ByVal Value As Boolean)
                flgAllowNumericNulls = True
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property Connection() As OleDbConnection
            Get
                If DesignMode Then
                    Return Nothing
                Else
                    Return cnnSchema
                End If
            End Get
        End Property

        Public Sub New()

            strConnectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\SourceDB.mdb"
            typDataSource = DatabaseType.Unspecified
            intRestriction = NoRestriction
            flgImmediateClose = True
            flgAllowTextNulls = False
            flgAllowNumericNulls = False

        End Sub

        Public Sub New(ByVal ConnectString As String, Optional ByVal TableTypeRestriction As TableType = NoRestriction, Optional ByVal ImmediateClose As Boolean = True, Optional ByVal AnaylzeNow As Boolean = True)

            strConnectString = ConnectString
            intRestriction = TableTypeRestriction
            flgImmediateClose = ImmediateClose
            flgAllowTextNulls = False
            flgAllowNumericNulls = False
            If AnaylzeNow Then Analyze()

        End Sub

        Public Sub Analyze()

            Dim row As DataRow
            Dim tbl As Table
            Dim fld As Field
            Dim x As Integer
            Dim y As Integer

            ' Check http://msdn.microsoft.com/library/default.asp?url=/library/en-us/oledb/htm/olprappendixb.asp
            ' for detailed OLEDB schema rowset information
            Tables = New Tables(Me)
            cnnSchema = New OleDbConnection(strConnectString)
            cnnSchema.Open()

            ' Load all tables and views into the schema
            With cnnSchema.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, Nothing)
                For x = 0 To .Rows.Count - 1
                    row = .Rows(x)
                    tbl = New Table(Tables, NotNull(row("TABLE_CATALOG")), NotNull(row("TABLE_SCHEMA")), row("TABLE_NAME"), row("TABLE_TYPE"), NotNull(row("DESCRIPTION"), ""), NotNull(row("CARDINALITY"), 0))

                    If tbl.Type And intRestriction Then
                        ' Both the data adapter and the OleDB schema rowsets provide column properties
                        ' that the other doesn't - so we use both to get a very complete schema                        
                        Dim data As New DataSet
                        Dim adapter As New OleDbDataAdapter

                        If tbl.Name.IndexOf(" "c) = -1 And tbl.UsesDefaultSchema() Then
                            Try
                                ' For standard table names we can use direct table commands for speed
                                adapter.SelectCommand = New OleDbCommand(tbl.Name, cnnSchema)
                                adapter.SelectCommand.CommandType = CommandType.TableDirect
                                adapter.FillSchema(data, SchemaType.Mapped)
                            Catch
                                ' We'll fall back on the standard method (maybe provider doesn't support TableDirect)
                                adapter.SelectCommand = New OleDbCommand("SELECT TOP 1 * FROM " & tbl.FullName, cnnSchema)
                                adapter.FillSchema(data, SchemaType.Mapped)
                            End Try
                        Else
                            ' For schema based databases and non-standard table names we must use a regular select command
                            adapter.SelectCommand = New OleDbCommand("SELECT TOP 1 * FROM " & tbl.FullName, cnnSchema)
                            adapter.FillSchema(data, SchemaType.Mapped)
                        End If

                        ' Load all column data into the schema
                        With cnnSchema.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, New Object() {Nothing, Nothing, tbl.Name})
                            For y = 0 To .Rows.Count - 1
                                row = .Rows(y)

                                ' New field encountered, create new field
                                fld = New Field(tbl.Fields, row("COLUMN_NAME"), row("DATA_TYPE"))
                                fld.flgHasDefault = NotNull(row("COLUMN_HASDEFAULT"), False)
                                fld.intNumericPrecision = NotNull(row("NUMERIC_PRECISION"), 0)
                                fld.intNumericScale = NotNull(row("NUMERIC_SCALE"), 0)
                                fld.intDateTimePrecision = NotNull(row("DATETIME_PRECISION"), 0)
                                fld.strDescription = NotNull(row("DESCRIPTION"), "")

                                ' We also use as many properties as we can from data adapter schema
                                With data.Tables(0).Columns(fld.Name)
                                    fld.intOrdinal = .Ordinal
                                    fld.flgAllowsNulls = .AllowDBNull
                                    fld.objDefaultValue = .DefaultValue
                                    fld.intMaxLength = .MaxLength
                                    fld.flgAutoInc = .AutoIncrement
                                    fld.intAutoIncSeed = .AutoIncrementSeed
                                    fld.intAutoIncStep = .AutoIncrementStep
                                    fld.flgReadOnly = .ReadOnly
                                    fld.flgUnique = .Unique
                                End With

                                ' Add field to table's field collection
                                tbl.Fields.Add(fld)
                            Next
                        End With

                        ' Sort all loaded fields in ordinal order
                        tbl.Fields.lstFields.Sort()

                        ' Define primary keys
                        Try
                            With cnnSchema.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, New Object() {Nothing, Nothing, tbl.Name})
                                For y = 0 To .Rows.Count - 1
                                    row = .Rows(y)
                                    tbl.DefinePrimaryKey(row("COLUMN_NAME"), NotNull(row("ORDINAL"), -1), NotNull(row("PK_NAME"), ""))
                                Next
                            End With
                        Catch
                            ' It's possible that the data source doesn't provide a primary keys rowset
                        End Try

                        ' Add table to schema's table collection
                        Tables.Add(tbl)
                    End If
                Next
            End With

            ' Define foreign keys (must be done after all tables are defined so relations can be properly established)
            For Each tbl In Tables
                Try
                    With cnnSchema.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, New Object() {Nothing, Nothing, tbl.Name})
                        For x = 0 To .Rows.Count - 1
                            row = .Rows(x)
                            tbl.DefineForeignKey(row("PK_COLUMN_NAME"), row("FK_TABLE_NAME"), row("FK_COLUMN_NAME"), NotNull(row("ORDINAL"), -1), NotNull(row("FK_NAME"), ""), Field.GetReferentialAction(NotNull(row("UPDATE_RULE"), "")), Field.GetReferentialAction(NotNull(row("DELETE_RULE"), "")))
                        Next
                    End With
                Catch
                    ' It's possible that the data source doesn't provide a foreign keys rowset
                End Try
            Next

            ' Sort tables in proper referential integrity processing order
            Tables.lstTables.Sort(Tables.ReferentialOrderComparer.Default)

            ' Set initial I/O processing priorties for tables based on this order.  Processing tables
            ' based on the "Priority" field allows user to have final say in processing order
            For x = 0 To Tables.Count - 1
                Tables(x).Priority = x
            Next

            ' Check to see if user requested to keep connection open, this is just for convience...
            If flgImmediateClose Then Close()

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Close()
            GC.SuppressFinalize(Me)

        End Sub

        Public Sub Close()

            If Not cnnSchema Is Nothing Then
                Try
                    cnnSchema.Close()
                Catch
                    ' Keep on going here...
                End Try
            End If
            cnnSchema = Nothing

        End Sub

    End Class

End Namespace