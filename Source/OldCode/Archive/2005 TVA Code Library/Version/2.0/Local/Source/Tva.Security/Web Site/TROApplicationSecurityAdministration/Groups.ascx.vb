Imports SecurityTableAdapters
Imports System.Data

Partial Class Groups
    Inherits System.Web.UI.UserControl

    Private groupsAdapter As New GroupsTableAdapter
    Private usersAndCompaniesAdapter As New UsersAndCompaniesAndSecurityQuestionsTableAdapter
    Private groupUsersAdapter As New GroupsUsersTableAdapter
    Private usersAdapter As New UsersTableAdapter
    Private rolesAdapter As New RolesTableAdapter
    Private appsAdapter As New ApplicationsTableAdapter
    Private rolesGroupsAdapter As New RolesGroupsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindToGrid()
            BindToUsersGrid()
            BindToRolesGrid()
            ViewState("Mode") = "Add"
            ViewState("Grp") = ""

            'Assign default button to search text box.
            Me.TextBoxSearch.Attributes.Add("onkeypress", "return btnClick(event,'" + Me.ButtonSearch.ClientID + "')")
        End If
    End Sub

    Private Sub BindToGrid()
        With Me.GridViewGroups
            .DataSource = groupsAdapter.GetGroups()
            .DataBind()
        End With
    End Sub

    Private Sub BindToRolesGrid()
        Me.UltraWebGridRoles.Clear()
        Dim ds As New DataSet
        ds.Tables.Add(rolesAdapter.GetRoles)
        ds.Tables(0).TableName = "roles"
        ds.Tables.Add(appsAdapter.GetApplications)
        ds.Tables(1).TableName = "apps"

        ds.Relations.Add("apps", ds.Tables("apps").Columns("ApplicationID"), ds.Tables("roles").Columns("ApplicationID"))

        With Me.UltraWebGridRoles
            .DataSource = ds.Tables("apps")
            .DataBind()
        End With
    End Sub

    Private Sub BindToUsersGrid()
        With Me.GridViewUsers
            .DataSource = usersAndCompaniesAdapter.GetUsersInfo
            .DataBind()
        End With
    End Sub

    Private Sub ClearForm()
        Me.TextBoxName.Text = ""
        Me.TextBoxDescription.Text = ""
        ViewState("Mode") = "Add"
        ViewState("Grp") = ""
        BindToGrid()
        BindToUsersGrid()
        For Each row As GridViewRow In Me.GridViewUsers.Rows
            DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = False
        Next

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator
        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("Select").Index).Value = False
        End While

    End Sub

    Protected Sub GridViewGroups_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewGroups.PageIndexChanging
        Me.GridViewGroups.PageIndex = e.NewPageIndex
        BindToGrid()
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewGroups.RowCommand
        If e.CommandName = "PopulateGroupInfo" Then
            PopulateGroupInfo(e.CommandArgument.ToString)
            ViewState("Mode") = "Edit"
            ViewState("Grp") = e.CommandArgument.ToString
        ElseIf e.CommandName = "DeleteGroup" Then
            groupsAdapter.DeleteGroup(e.CommandArgument.ToString)
            ClearForm()
            ViewState("Mode") = "Add"
            ViewState("Grp") = ""
            Session("RefreshData") = 1
        End If
    End Sub

    Private Sub PopulateGroupInfo(ByVal groupName As String)
        Dim groups As Security.GroupsDataTable = groupsAdapter.GetGroupByGroupName(groupName)
        Dim group As Security.GroupsRow = groups.Rows(0)

        Me.TextBoxName.Text = group.GroupName
        Me.TextBoxDescription.Text = group.GroupDescription

        'Populate list of users from the GroupUsers table for a selected groupName
        'First of all clear all the checkboxed from the users list grid.
        For Each row As GridViewRow In Me.GridViewUsers.Rows
            DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = False
        Next

        Dim groupID As Guid = groupsAdapter.GetGroupIDByGroupName(groupName)

        Dim userIds As Security.GroupsUsersDataTable = groupUsersAdapter.GetUserIDsByGroupID(groupID)
        For Each userId As Security.GroupsUsersRow In userIds.Rows

            Dim userName As String = usersAdapter.GetUserNameByUserID(userId.UserID)
            For Each row As GridViewRow In Me.GridViewUsers.Rows
                If userName = row.Cells(1).Text Then
                    DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = True
                End If
            Next
        Next

        CheckGroupsRoles(groupID)

    End Sub

    Private Sub CheckGroupsRoles(ByVal groupID As Guid)

        Dim rolesGroups As Security.RolesGroupsDataTable = rolesGroupsAdapter.GetRolesByGroupID(groupID)

        For Each rolesGroup As Security.RolesGroupsRow In rolesGroups.Rows

            Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator
            While en.MoveNext
                Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
                If row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("RoleID").Index).Value = rolesGroup.RoleID Then
                    row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("Select").Index).Value = True
                End If
            End While

        Next

    End Sub

    Protected Sub GridViewGroups_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GridViewGroups.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim deleteLink As LinkButton = e.Row.FindControl("LinkButton2")
            deleteLink.Attributes.Add("onclick", "javascript:return confirm('Do you want to delete group: " & _
                                                    DataBinder.Eval(e.Row.DataItem, "GroupName") & "? ');")
        End If
    End Sub

    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
    End Sub

    Protected Sub ButtonSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSearch.Click
        Dim searchStr As String = Me.TextBoxSearch.Text.Replace("'", "''")
        searchStr = searchStr.Replace("%", "")

        With Me.GridViewGroups
            .DataSource = groupsAdapter.GetGroups().Select("GroupName Like '%" & searchStr & "%' Or GroupDescription Like '%" & searchStr & "%'")
            .DataBind()
        End With
    End Sub

    Protected Sub LinkButtonShowAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LinkButtonShowAll.Click
        Me.TextBoxSearch.Text = ""
        ClearForm()
    End Sub

    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click
        Dim newGroupName As String = Me.TextBoxName.Text.Replace("'", "''")
        Dim newGroupDescription As String = Me.TextBoxDescription.Text.Replace("'", "''")

        If ViewState("Mode") = "Add" Then
            groupsAdapter.InsertGroup(newGroupName, newGroupDescription)

            'Insert associations into the GroupUsers Table.
            InsertIntoGroupUsers(newGroupName)
            UpdateGroupsRoles(newGroupName.Replace(" ", ""))
        Else
            If Not ViewState("Grp") = "" Then   'if viewstate information is set then use it
                groupsAdapter.UpdateGroup(newGroupName, newGroupDescription, ViewState("Grp"))

                'On update delete all the existing associations and insert all new ones.
                Dim newGroupID As Guid = groupsAdapter.GetGroupIDByGroupName(newGroupName.Replace(" ", ""))
                groupUsersAdapter.DeleteGroupUsers(newGroupID)
                InsertIntoGroupUsers(newGroupName.Replace(" ", ""))
                UpdateGroupsRoles(newGroupName.Replace(" ", ""))
            End If
        End If

        ClearForm()
        Session("RefreshData") = 1
    End Sub

    Private Sub UpdateGroupsRoles(ByVal groupName As String)

        Dim groupID As Guid = groupsAdapter.GetGroupIDByGroupName(groupName)
        rolesGroupsAdapter.DeleteByGroupID(groupID)

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator

        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            'Response.Write(row.Cells(0).Value.ToString & "<BR>")
            If row.Cells(0).Value = True Then
                rolesGroupsAdapter.InsertRolesGroups(row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("RoleID").Index).Value, groupID)
            End If
        End While
    End Sub

    Private Sub InsertIntoGroupUsers(ByVal groupName As String)
        Dim groupID As Guid = groupsAdapter.GetGroupIDByGroupName(groupName.Replace(" ", ""))
        For Each row As GridViewRow In Me.GridViewUsers.Rows
            If DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked Then
                Dim userID As Guid = usersAdapter.GetUserIDByUserName(row.Cells(1).Text)
                groupUsersAdapter.InsertGroupUsers(groupID, userID)
            End If
        Next
    End Sub

    Protected Sub GridViewUsers_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewUsers.Sorting
        Dim dv As New DataView
        dv = usersAndCompaniesAdapter.GetUsersInfo.DefaultView
        dv.Sort = e.SortExpression
        'dv.Sort = String.Format("{0} {1}", e.SortExpression, e.SortDirection)

        With Me.GridViewUsers
            .DataSource = dv
            .DataBind()
        End With
    End Sub

    Protected Sub GridViewGroups_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewGroups.Sorting
        Dim dv As New DataView
        dv = groupsAdapter.GetGroups().DefaultView
        dv.Sort = e.SortExpression()

        With Me.GridViewGroups
            .DataSource = dv
            .DataBind()
        End With
    End Sub

    Protected Sub UltraWebGridRoles_InitializeLayout(ByVal sender As Object, ByVal e As Infragistics.WebUI.UltraWebGrid.LayoutEventArgs) Handles UltraWebGridRoles.InitializeLayout
        Dim newCol As Infragistics.WebUI.UltraWebGrid.UltraGridColumn = New Infragistics.WebUI.UltraWebGrid.UltraGridColumn
        newCol.Key = "Select"
        newCol.Header.Caption = "Select"
        newCol.Type = Infragistics.WebUI.UltraWebGrid.ColumnType.CheckBox
        newCol.AllowUpdate = Infragistics.WebUI.UltraWebGrid.AllowUpdate.Yes

        If Not Me.UltraWebGridRoles.Bands(1).Columns.IndexOf("Select") >= 0 Then
            Me.UltraWebGridRoles.Bands(1).Columns.Insert(0, newCol)
        End If

        'Me.UltraWebGridRoles.Bands(1).Columns.Insert(0, newCol)

        With Me.UltraWebGridRoles.Bands(0).Columns
            .FromKey("ApplicationID").Hidden = True
            .FromKey("ApplicationName").Width = New Unit(100)
            .FromKey("ApplicationDescription").Width = New Unit(425)
        End With

        With Me.UltraWebGridRoles.Bands(1).Columns
            .FromKey("Select").Move(0)
            .FromKey("RoleName").Move(1)
            .FromKey("ApplicationID").Hidden = True
            .FromKey("RoleID").Hidden = True
            .FromKey("Select").Width = New Unit(75)
            .FromKey("RoleName").Width = New Unit(125)
            .FromKey("RoleDescription").Width = New Unit(325)
        End With
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshData") = 1 Then
            BindToGrid()
            BindToUsersGrid()
            BindToRolesGrid()
        End If
    End Sub
End Class
