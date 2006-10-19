Imports SecurityTableAdapters
Imports System.Data

Partial Class Default2
    Inherits System.Web.UI.Page

    Private appsAdapter As New ApplicationsTableAdapter
    Private rolesAdapter As New RolesTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Dim roles As Security.RolesDataTable = rolesAdapter.GetRoles
        'Dim apps As Security.ApplicationsDataTable = appsAdapter.GetApplications()
        If Not IsPostBack Then
            Dim ds As New DataSet
            ds.Tables.Add(rolesAdapter.GetRoles)
            ds.Tables(0).TableName = "roles"
            ds.Tables.Add(appsAdapter.GetApplications)
            ds.Tables(1).TableName = "apps"

            ds.Relations.Add("apps", ds.Tables("apps").Columns("ApplicationID"), ds.Tables("roles").Columns("ApplicationID"))

            With Me.UltraWebGrid1
                .DataSource = ds.Tables("apps")
                .DataBind()
            End With
        End If
    End Sub

    Protected Sub UltraWebGrid1_InitializeLayout(ByVal sender As Object, ByVal e As Infragistics.WebUI.UltraWebGrid.LayoutEventArgs) Handles UltraWebGrid1.InitializeLayout

        Dim newCol As Infragistics.WebUI.UltraWebGrid.UltraGridColumn = New Infragistics.WebUI.UltraWebGrid.UltraGridColumn
        newCol.Key = "Select"
        newCol.Header.Caption = "Select"
        newCol.Type = Infragistics.WebUI.UltraWebGrid.ColumnType.CheckBox
        newCol.AllowUpdate = Infragistics.WebUI.UltraWebGrid.AllowUpdate.Yes
        Me.UltraWebGrid1.Bands(1).Columns.Insert(0, newCol)

        With Me.UltraWebGrid1.Bands(0).Columns
            .FromKey("ApplicationID").Hidden = True
            .FromKey("ApplicationName").Width = New Unit(100)
        End With

        With Me.UltraWebGrid1.Bands(1).Columns
            .FromKey("Select").Move(0)
            .FromKey("RoleName").Move(1)
            .FromKey("ApplicationID").Hidden = True
            .FromKey("RoleID").Hidden = True
        End With

    End Sub

    Protected Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGrid1.Bands(1).GetRowsEnumerator

        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            Response.Write(row.Cells(0).Value.ToString & "<BR>")
        End While


    End Sub
End Class
