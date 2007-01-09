Imports SecurityTableAdapters

Partial Class Roles
    Inherits System.Web.UI.UserControl

    Private appsAdapter As New ApplicationsTableAdapter
    Private rolesAdapter As New RolesTableAdapter

    ''' <summary>
    ''' On page load populate applications drop down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            'Populate dropdown list with the list of applications available in the database.
            ViewState("App") = ""
            PopulateApplications()
            If Me.DropDownListApplications.Items.Count > 0 Then
                BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))
            End If

            ViewState("Mode") = "Add"
            ViewState("Role") = ""
            ViewState("App") = Me.DropDownListApplications.SelectedValue.ToString

            'Assign default button to search text box.
            Me.TextBoxSearch.Attributes.Add("onkeypress", "return btnClick(event,'" + Me.ButtonSearch.ClientID + "')")
        End If
    End Sub

    ''' <summary>
    ''' Populate applications list.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PopulateApplications()
        'Repopulate dropdown
        Me.DropDownListApplications.Items.Clear()
        With Me.DropDownListApplications
            .DataSource = appsAdapter.GetApplications
            .DataTextField = "ApplicationName"
            .DataValueField = "ApplicationID"
            .DataBind()
            .SelectedIndex = 0
        End With

        If Not ViewState("App") = "" Then
            Me.DropDownListApplications.SelectedValue = ViewState("App")
        End If
    End Sub

    ''' <summary>
    ''' Populate grid with the list of roles for selected application.
    ''' </summary>
    ''' <param name="applicationId"></param>
    ''' <remarks></remarks>
    Public Sub BindToGrid(ByVal applicationId As Guid)
        With Me.GridViewRoles
            .DataSource = rolesAdapter.GetRolesByApplicationID(applicationId)
            .DataBind()
        End With
    End Sub

    Protected Sub GridViewRoles_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewRoles.PageIndexChanging
        Me.GridViewRoles.PageIndex = e.NewPageIndex
        Me.BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))
    End Sub

    ''' <summary>
    ''' Populate roles information or delete role from the database.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewRoles.RowCommand
        If e.CommandName = "PopulateRoleInfo" Then
            PopulateRoleInfo(e.CommandArgument.ToString)
            ViewState("Mode") = "Edit"
            ViewState("Role") = e.CommandArgument.ToString
        ElseIf e.CommandName = "DeleteRole" Then
            rolesAdapter.DeleteRole(e.CommandArgument.ToString)

            Dim usersControl As New UsersForRoles
            usersControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("UsersForRoles1"), UsersForRoles)
            usersControl.DeleteFromRolesUsers(e.CommandArgument.ToString)

            Dim groupsControl As New GroupsForRoles
            groupsControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("GroupsForRoles1"), GroupsForRoles)
            groupsControl.DeleteFromRolesGroups(e.CommandArgument.ToString)

            ClearForm()
            ViewState("Mode") = "Add"
            ViewState("Role") = ""
            Session("RefreshRoles") = 1
        End If
    End Sub

    ''' <summary>
    ''' Populate role information in the form for editing.
    ''' </summary>
    ''' <param name="roleName"></param>
    ''' <remarks></remarks>
    Private Sub PopulateRoleInfo(ByVal roleName As String)
        Dim roles As Security.RolesDataTable = rolesAdapter.GetRoleByRoleName(roleName)
        Dim role As Security.RolesRow = roles.Rows(0)

        Me.TextBoxName.Text = role.RoleName
        Me.TextBoxDescription.Text = role.RoleDescription

        Dim usersControl As New UsersForRoles
        usersControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("UsersForRoles1"), UsersForRoles)
        usersControl.LoadData(roleName)

        Dim groupsControl As New GroupsForRoles
        groupsControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("GroupsForRoles1"), GroupsForRoles)
        groupsControl.LoadData(roleName)

    End Sub

    ''' <summary>
    ''' Save changes to the current role or insert new role into the database.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click
        Dim newRoleName As String = Me.TextBoxName.Text '.Replace("'", "''")
        Dim newRoleDescription As String = Me.TextBoxDescription.Text '.Replace("'", "''")

        If ViewState("Mode") = "Add" Then
            rolesAdapter.InsertRole(newRoleDescription, newRoleName, New Guid(Me.DropDownListApplications.SelectedValue.ToString))
        Else
            If ViewState("Role") <> "" Then
                Dim origRoleID As Guid = rolesAdapter.GetRoleID(ViewState("Role").ToString)
                rolesAdapter.UpdateRole(newRoleDescription, newRoleName, origRoleID, New Guid(Me.DropDownListApplications.SelectedValue.ToString))
            End If
        End If

        Dim usersControl As New UsersForRoles
        usersControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("UsersForRoles1"), UsersForRoles)
        usersControl.InsertIntoRolesUsers(newRoleName.Replace(" ", "_"))

        Dim groupsControl As New GroupsForRoles
        groupsControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("GroupsForRoles1"), GroupsForRoles)
        groupsControl.InsertIntoRolesGroups(newRoleName.Replace(" ", "_"))

        ClearForm()
        Session("RefreshRoles") = 1

    End Sub

    ''' <summary>
    ''' Clear all the fields in the form.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ClearForm()
        Me.TextBoxName.Text = ""
        Me.TextBoxDescription.Text = ""
        ViewState("Mode") = "Add"
        ViewState("Role") = ""

        'If Not ViewState("App") = "" Then
        '    Me.DropDownListApplications.SelectedValue = ViewState("App")
        'End If
        'BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))

        Dim usersControl As New UsersForRoles
        usersControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("UsersForRoles1"), UsersForRoles)
        usersControl.ClearCheckBoxes()

        Dim groupsControl As New GroupsForRoles
        groupsControl = DirectCast(Me.FindControl("UltraWebTabUsersAndGroups").FindControl("GroupsForRoles1"), GroupsForRoles)
        groupsControl.ClearCheckBoxes()
    End Sub

    ''' <summary>
    ''' Clear form fields and set viewstate to add mode.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
    End Sub

    ''' <summary>
    ''' When application selection changes, refresh grid with that applications roles.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub DropDownListApplications_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DropDownListApplications.SelectedIndexChanged
        ViewState("App") = Me.DropDownListApplications.SelectedValue.ToString
        Me.BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))
        ClearForm()
    End Sub

    ''' <summary>
    ''' Add onlclick handler for delete button to cofirm delete action.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub GridViewRoles_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GridViewRoles.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim deleteLink As LinkButton = e.Row.FindControl("LinkButton2")
            deleteLink.Attributes.Add("onclick", "javascript:return confirm('Do you want to delete role: " & _
                                                    DataBinder.Eval(e.Row.DataItem, "RoleName") & "? ');")
        End If
    End Sub

    Protected Sub ButtonSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSearch.Click
        Dim searchStr As String = Me.TextBoxSearch.Text.Replace("'", "''")
        searchStr = searchStr.Replace("%", "")

        With Me.GridViewRoles
            .DataSource = rolesAdapter.GetRolesByApplicationID(New Guid(Me.DropDownListApplications.SelectedValue.ToString)).Select("RoleName Like '%" & searchStr & "%' OR RoleDescription Like '%" & searchStr & "%'")
            .DataBind()
        End With
    End Sub

    Protected Sub LinkButtonShowAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LinkButtonShowAll.Click
        Me.TextBoxSearch.Text = ""
        Me.BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))
        ClearForm()
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshApps") = 1 Then
            PopulateApplications()
        End If

        If Session("RefreshRoles") = 1 Then
            If Me.DropDownListApplications.Items.Count > 0 Then
                BindToGrid(New Guid(Me.DropDownListApplications.SelectedValue.ToString))
            End If
        End If
    End Sub
End Class
