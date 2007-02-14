'*******************************************************************************************************
'  Tva.Web.Services.Common.vb - Common web service related functions
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
'  01/24/2007 - J. Ritchie Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Data
Imports System.Data.SqlClient
Imports Tva.Configuration.Common
Imports Tva.Security.Application

Namespace Services

    ''' <summary>Defines common global functions related to Web Services</summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Shared Function SetWebServiceCredentials(ByVal webService As Object, ByVal userName As String, ByVal password As String, ByVal server As SecurityServer, ByVal passThroughAuthentication As Boolean) As Object

            ' Note "webService" parameter must be "Object", because web services create local proxy implementations
            ' of the AuthenticationSoapHeader and do not support interfaces - hence all calls will be made through
            ' reflection (i.e., late bound method invocation support), but everything works as expected...
            With webService
                .UserName = userName
                .Password = password
                .Server = server
                .PassThroughAuthentication = passThroughAuthentication
            End With

            Return webService

        End Function

        Public Shared Function ExceptionsArrayListToDataTable(ByVal exceptions As ArrayList) As DataTable

            Dim dtResult As New DataTable("Exceptions")
            dtResult.Columns.Add("ExceptionMessage")
            If exceptions.Count = 0 Then
                dtResult = HandleNoValuesInFields(dtResult)
            Else
                For i As Integer = 0 To exceptions.Count - 1
                    Dim dr As DataRow
                    dr = dtResult.NewRow()
                    dr.Item(0) = exceptions.Item(i)
                    dtResult.Rows.Add(dr)
                Next
            End If

            Return dtResult

        End Function

        Public Shared Function BuildExportInfoTable(ByVal recCount As Integer, ByVal messageName As String) As DataTable

            Dim dtExport As New DataTable("ExportInformation")
            dtExport.Columns.Add("RecordCount")
            dtExport.Columns.Add("RunTime")
            dtExport.Columns.Add("RefreshSchedule")
            dtExport.Columns.Add("SourceDataTimeZone")

            Dim dr As DataRow
            dr = dtExport.NewRow()
            dr.Item(0) = recCount
            dr.Item(1) = Now()
            dr.Item(2) = CategorizedSettings("WebServices.RefreshSchedule").Item(messageName & ".RefreshSchedule").Value
            dr.Item(3) = CategorizedSettings("WebServices.SourceDataTimeZone").Item(messageName & ".SourceDataTimeZone").Value
            dtExport.Rows.Add(dr)

            Return dtExport

        End Function

        Public Shared Function BuildSourceInfoTable(ByVal messageName As String) As DataTable

            Dim dtSource As New DataTable("SourceInformation")

            Dim sourceTables() As String = CategorizedSettings("WebServices.SourceTables").Item(messageName & ".SourceTables").Value.Split(";"c)
            'Dim col As DataColumn = dtSource.Columns.Add("SourceTable")
            'col.ColumnMapping = MappingType.Attribute
            dtSource.Columns.Add("SourceTable")
            For Each table As String In sourceTables

                Dim dr As DataRow
                dr = dtSource.NewRow()
                dr.Item(0) = table
                dtSource.Rows.Add(dr)
            Next

            Return dtSource

        End Function


        Public Shared Function HandleNoValuesInFields(ByVal dt As DataTable) As DataTable

            Dim drNull As DataRow
            Dim i As Integer
            drNull = dt.NewRow()
            For i = 0 To dt.Columns.Count - 1
                drNull.Item(i) = DBNull.Value
            Next

            dt.Rows.Add(drNull)

            Return dt
        End Function

    End Class

End Namespace
