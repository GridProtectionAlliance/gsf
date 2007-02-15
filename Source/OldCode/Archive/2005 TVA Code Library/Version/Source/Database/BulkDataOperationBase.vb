Imports System.Data.OleDb
Imports System.ComponentModel
Imports System.Drawing
Imports TVA.Database.Common

Namespace Database

    ' This is the common interface for any bulk data operation
    Public Interface IBulkDataOperation

        Event TableProgress(ByVal TableName As String, ByVal Executed As Boolean, ByVal CurrentTable As Integer, ByVal TotalTables As Integer)
        Event RowProgress(ByVal TableName As String, ByVal CurrentRow As Integer, ByVal TotalRows As Integer)
        Event OverallProgress(ByVal Current As Integer, ByVal Total As Integer)
        Event SqlFailure(ByVal Sql As String, ByVal ex As Exception)

        ReadOnly Property WorkTables() As Tables
        Property FromSchema() As Schema
        Property ToSchema() As Schema
        Property RowReportInterval() As Integer
        Property Timeout() As Integer

        Sub Execute()
        Sub Close()

    End Interface

    ' This class defines a common set of functionality that any bulk data operation implementation can use
    Public MustInherit Class BulkDataOperationBase

        Implements IBulkDataOperation
        Implements IDisposable

        Protected ComponentSite As ISite
        Protected schFrom As Schema
        Protected schTo As Schema
        Protected intOverallProgress As Long        ' Implemetor can use this variable to track overall progress
        Protected intOverallTotal As Long           ' This is initialized to the overall total number of records to be processed
        Protected intRowReportInterval As Integer   ' Defines interval for reporting row progress
        Protected intTimeout As Integer             ' Timeout value for Sql operation
        Protected colTables As Tables
        Protected flgUseFromSchemaRI As Boolean

        Public Event TableProgress(ByVal TableName As String, ByVal Executed As Boolean, ByVal CurrentTable As Integer, ByVal TotalTables As Integer) Implements IBulkDataOperation.TableProgress
        Public Event RowProgress(ByVal TableName As String, ByVal CurrentRow As Integer, ByVal TotalRows As Integer) Implements IBulkDataOperation.RowProgress
        Public Event OverallProgress(ByVal Current As Integer, ByVal Total As Integer) Implements IBulkDataOperation.OverallProgress
        Public Event SqlFailure(ByVal Sql As String, ByVal ex As Exception) Implements IBulkDataOperation.SqlFailure

        Public Sub New()

            intRowReportInterval = 5
            intTimeout = 120
            flgUseFromSchemaRI = True

        End Sub

        Public Sub New(ByVal FromConnectString As String, ByVal ToConnectString As String)

            MyClass.New()
            schFrom = New Schema(FromConnectString, TableType.Table, False, False)
            schTo = New Schema(ToConnectString, TableType.Table, False, False)

        End Sub

        Public Sub New(ByVal FromSchema As Schema, ByVal ToSchema As Schema)

            MyClass.New()
            schFrom = FromSchema
            schTo = ToSchema

        End Sub

        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)

            Close()

        End Sub

        Public Overridable Sub Close() Implements IBulkDataOperation.Close, IDisposable.Dispose

            If Not schFrom Is Nothing Then schFrom.Close()
            If Not schTo Is Nothing Then schTo.Close()
            schFrom = Nothing
            schTo = Nothing
            GC.SuppressFinalize(Me)

        End Sub

        <Browsable(True), Category("Configuration"), Description("Source schema definition.")> _
        Public Overridable Property FromSchema() As Schema Implements IBulkDataOperation.FromSchema
            Get
                Return schFrom
            End Get
            Set(ByVal Value As Schema)
                schFrom = Value
                If Not ComponentSite Is Nothing Then If ComponentSite.DesignMode Then schFrom.ImmediateClose = False
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Destination schema definition.")> _
        Public Overridable Property ToSchema() As Schema Implements IBulkDataOperation.ToSchema
            Get
                Return schTo
            End Get
            Set(ByVal Value As Schema)
                schTo = Value
                If Not ComponentSite Is Nothing Then If ComponentSite.DesignMode Then schTo.ImmediateClose = False
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Number of rows to process before raising progress events."), DefaultValue(5)> _
        Public Overridable Property RowReportInterval() As Integer Implements IBulkDataOperation.RowReportInterval
            Get
                Return intRowReportInterval
            End Get
            Set(ByVal Value As Integer)
                intRowReportInterval = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Maximum number of seconds to wait when processing a Sql command before timing out."), DefaultValue(120)> _
        Public Overridable Property Timeout() As Integer Implements IBulkDataOperation.Timeout
            Get
                Return intTimeout
            End Get
            Set(ByVal Value As Integer)
                intTimeout = Value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set to True to use referential integrity information from source database during data processing, set to False to use destination database."), DefaultValue(True)> _
        Public Overridable Property UseFromSchemaReferentialIntegrity() As Boolean
            Get
                Return flgUseFromSchemaRI
            End Get
            Set(ByVal Value As Boolean)
                flgUseFromSchemaRI = Value
            End Set
        End Property

        ' These are the tables that were found in both source and dest to be used for data operation...
        <Browsable(False)> _
        Public Overridable ReadOnly Property WorkTables() As Tables Implements IBulkDataOperation.WorkTables
            Get
                Return colTables
            End Get
        End Property

        Public Overridable Sub Analyze()

            Dim tbl, tblLookup As Table

            schFrom.ImmediateClose = False
            schFrom.Analyze()

            schTo.ImmediateClose = False
            schTo.Analyze()

            colTables = New Tables(schFrom)

            ' We preprocess which tables we are going to access for data operation...
            For Each tbl In schFrom.Tables
                ' Lookup table name in destination datasource by map name
                tblLookup = schTo.Tables.FindByMapName(tbl.MapName)
                If Not tblLookup Is Nothing Then
                    intOverallTotal += tbl.RowCount

                    ' If user requested to use referential integrity of destination tables then
                    ' we use process priority of those tables instead...
                    If Not flgUseFromSchemaRI Then tbl.Priority = tblLookup.Priority

                    tbl.Process = True
                    colTables.Add(tbl)
                End If
            Next

        End Sub

        Public MustOverride Sub Execute() Implements IBulkDataOperation.Execute

        ' Derived classes can't directly raise base class events, hence the following...
        Protected Sub RaiseEvent_TableProgress(ByVal TableName As String, ByVal Executed As Boolean, ByVal CurrentTable As Integer, ByVal TotalTables As Integer)

            RaiseEvent TableProgress(TableName, Executed, CurrentTable, TotalTables)

        End Sub

        Protected Sub RaiseEvent_RowProgress(ByVal TableName As String, ByVal CurrentRow As Integer, ByVal TotalRows As Integer)

            RaiseEvent RowProgress(TableName, CurrentRow, TotalRows)

        End Sub

        Protected Sub RaiseEvent_OverallProgress(ByVal Current As Integer, ByVal Total As Integer)

            RaiseEvent OverallProgress(Current, Total)

        End Sub

        Protected Sub RaiseEvent_SqlFailure(ByVal Sql As String, ByVal ex As Exception)

            RaiseEvent SqlFailure(Sql, ex)

        End Sub

    End Class

End Namespace