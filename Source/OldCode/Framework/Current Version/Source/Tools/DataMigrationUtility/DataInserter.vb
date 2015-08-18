'*******************************************************************************************************
'  DataInserter.vb - Gbtc
'
'  Tennessee Valley Authority, 2010
'  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
'
'  This software is made freely available under the TVA Open Source Agreement (see below).
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/28/2010 - J. Ritchie Carroll
'       Generated original version of source code.
'
'*******************************************************************************************************

#Region " TVA Open Source Agreement "

' THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
' MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
' TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
' ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
' DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
' MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
' ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

' Original Software Designation: openPDC
' Original Software Title: The TVA Open Source Phasor Data Concentrator
' User Registration Requested. Please Visit https://naspi.tva.com/Registration/
' Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

' 1. DEFINITIONS

' A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
' that makes a Modification.

' B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
' the use or sale of its Modification alone or when combined with the Subject Software.

' C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
' image, or any other device.

' D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
' another.

' E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
' software separate from the Subject Software that is not governed by the terms of this Agreement.

' F. "Modification" means any alteration of, including addition to or deletion from, the substance or
' structure of either the Original Software or Subject Software, and includes derivative works, as that
' term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
' as part of a Larger Work does not in and of itself constitute a Modification.

' G. "Original Software" means the computer software first released under this Agreement by Government
' Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

' H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
' Contributors.

' I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

' J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

' K. "Sale" means the exchange of the Subject Software for money or equivalent value.

' L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

' M. "Use" means the application or employment of the Subject Software for any purpose.

' 2. GRANT OF RIGHTS

' A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
' with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
' non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
' the Subject Software:

' 1. Use

' 2. Distribution

' 3. Reproduction

' 4. Modification

' 5. Redistribution

' 6. Display

' B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
' respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
' Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
' pertaining to the Subject Software:

' 1. Use

' 2. Distribution

' 3. Reproduction

' 4. Sale

' 5. Offer for Sale

' C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
' and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
' such Modification causes the combination to be covered by the Covered Patents. It does not apply to
' any other combinations that include a Modification. 

' D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
' Such sublicense must be under the same terms and conditions of this Agreement.

' 3. OBLIGATIONS OF RECIPIENT

' A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
' additions covered under paragraph 3H. 

' 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
' must be included with each copy of the Subject Software; and

' 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
' Recipient must also make the source code freely available, and must provide with each copy of the
' Subject Software information on how to obtain the source code in a reasonable manner on or through a
' medium customarily used for software exchange.

' B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
' Software:

'          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

' C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
' must identify itself as the originator of its Modification in a manner that reasonably allows
' subsequent Recipients to identify the originator of the Modification. In fulfillment of these
' requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
' made and the date of the alterations, identifies Contributor as originator of the alterations, and
' consents to characterization of the alterations as a Modification, for example, by including a
' statement that the Modification is derived, directly or indirectly, from Original Software provided by
' Government Agency. Once consent is granted, it may not thereafter be revoked.

' D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
' been added to the Subject Software, a Recipient may not remove it without the express permission of
' the Contributor who added the notice.

' E. A Recipient may not make any representation in the Subject Software or in any promotional,
' advertising or other material that may be construed as an endorsement by Government Agency or by any
' prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
' advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

' F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
' upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
' following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
' shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
' requested that the Recipient inform Government Agency at the web site provided above how to access the
' Modification.

' G. Each Contributor represents that that its Modification does not violate any existing agreements,
' regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
' conveyed by this Agreement.

' H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
' liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
' however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
' Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
' obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
' Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
' indemnity and/or liability offered by such Recipient.

' I. A Recipient may create a Larger Work by combining Subject Software with separate software not
' governed by the terms of this agreement and distribute the Larger Work as a single product. In such
' case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
' is subject to this Agreement.

' J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
' any goods or technical data from the United States may require some form of export license from the
' U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
' U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
' required, it shall be issued. Nothing granted herein provides any such export license.

' 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

' A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
' EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
' SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
' PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
' FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
' AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
' RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
' RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
' LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
' "AS IS."

' B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
' AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
' OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
' SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
' SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
' EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
' LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
' EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
' GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
' IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

' 5. GENERAL TERMS

' A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
' Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
' thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
' immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
' Software properly granted by the breaching Recipient shall survive any such termination of this
' Agreement.

' B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
' it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

' C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
' including, but not limited to, determining the validity of this Agreement, the meaning of its
' provisions and the rights, obligations and remedies of the parties.

' D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
' parties relating to release of the Subject Software and may not be superseded, modified or amended
' except by further written agreement duly executed by the parties.

' E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
' affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
' Recipient hereby agrees to all terms and conditions herein.

' F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
' representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

#End Region


' James Ritchie Carroll - 2003
Option Explicit On

Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Data
Imports TVA.Database.Common
'Imports TVA.Shared.String
'Imports TVA.Shared.FilePath
Imports TVA
Imports TVA.IO.FilePath
Imports VB = Microsoft.VisualBasic
Imports TVA.Reflection

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
        Private strBulkInsertFilePath As String = AssemblyInfo.ExecutingAssembly.Location   ' Mehul GetApplicationPath()
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

            intOverallProgress = 0
            intOverallTotal = 0

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
                ToTable.Connection.ExecuteNonQuery(strDeleteSql, Timeout)
                RaiseEvent TableCleared(ToTable.Name)
            Catch ex As Exception
                RaiseEvent_SqlFailure(strDeleteSql, ex)
            End Try

        End Sub

        Private Sub ExecuteInserts(ByVal FromTable As Table, ByVal ToTable As Table)

            Dim colFields As Fields
            Dim fld As Field
            Dim fldLookup As Field = Nothing
            Dim fldCommon As Field = Nothing
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
            'Dim flgRecordExists As Boolean
            Dim intProgress As Integer
            Dim intTotal As Integer
            Dim tblSource As Table = IIf(flgUseFromSchemaRI, FromTable, ToTable)
            Dim fldAutoInc As Field = Nothing
            Dim flgUseBulkInsert As Boolean
            Dim strBulkInsertRow As StringBuilder
            Dim strBulkInsertFile As String = ""
            Dim strFldTerminator As String = ""
            Dim strRowTerminator As String = ""
            Dim fsBulkInsert As FileStream = Nothing
            Dim bytDataRow As Byte()

            ' Create a field list of all of the common fields in both tables
            colFields = New Fields(ToTable)

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
                        strBulkInsertFile = strBulkInsertFilePath & (New Guid()).ToString() & ".tmp"
                        fsBulkInsert = File.Create(strBulkInsertFile)
                    End If
                End With
            End If

            ' Execute source query
            With FromTable.Connection.ExecuteReader("SELECT " & colFields.GetList() & " FROM " & FromTable.FullName, CommandBehavior.SequentialAccess, Timeout)
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
                                If flgAddedFirstInsert Or Not fldAutoInc Is Nothing Then ToTable.Connection.ExecuteNonQuery(strInsertSql.ToString(), Timeout)

                                ' Save new destination autoinc value
                                If Not fldAutoInc Is Nothing Then
                                    Try
                                        fldAutoInc.tblAutoIncTranslations.Add(CStr(fldAutoInc.Value), ToTable.Connection.ExecuteScalar(ToTable.IdentitySql, Timeout))
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
                                If NotNull(ToTable.Connection.ExecuteScalar(strCountSql.ToString(), Timeout), 0) > 0 Then
                                    ' Add where criteria to Sql update statement
                                    strUpdateSql.Append(strWhereSql.ToString())

                                    Try
                                        ' Update record in destination table
                                        If flgAddedFirstUpdate Then ToTable.Connection.ExecuteNonQuery(strUpdateSql.ToString(), Timeout)
                                    Catch ex As Exception
                                        RaiseEvent_SqlFailure(strUpdateSql.ToString(), ex)
                                    End Try
                                Else
                                    Try
                                        ' Insert record into destination table
                                        If flgAddedFirstInsert Then ToTable.Connection.ExecuteNonQuery(strInsertSql.ToString(), Timeout)
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
                    ToTable.Connection.ExecuteNonQuery(strBulkInsertSql, Timeout)
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