Imports SecurityTableAdapters
Imports System.Data

Partial Class Users
    Inherits System.Web.UI.UserControl

    Private usersAdapter As New UsersTableAdapter
    Private rolesAdapter As New RolesTableAdapter
    Private rolesUsersAdapter As New RolesUsersTableAdapter
    Private appsAdapter As New ApplicationsTableAdapter
    Private companiesAdapter As New CompaniesTableAdapter
    Private securityQuestionsAdapter As New SecurityQuestionsTableAdapter
    Private userAndCompaniesAndSecurityQuestionsAdapter As New UsersAndCompaniesAndSecurityQuestionsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then
            PopulateDropDowns()
            If Me.DropDownListSelectCompanies.Items.Count > 0 Then
                BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
            End If
            BindRolesToGrid()

            ViewState("Mode") = "Add"
            ViewState("UN") = ""

            'Assign default button to search text box.
            Me.TextBoxSearch.Attributes.Add("onkeypress", "return btnClick(event,'" + Me.ButtonSearch.ClientID + "')")
            Me.CheckBoxIsExternal.Attributes.Add("onclick", "EnableExternalFields();")
        End If

    End Sub

    Private Sub BindToGrid(ByVal companyName As String)

        With Me.GridViewUsers
            .DataSource = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUsersByCompanyName(companyName)
            .DataBind()
        End With
    End Sub

    Private Sub BindRolesToGrid()

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

    Private Sub PopulateDropDowns()

        With Me.DropDownListSecurityQuestions
            .DataSource = securityQuestionsAdapter.GetSecurityQuestions
            .DataTextField = "SecurityQuestionText"
            .DataValueField = "SecurityQuestionID"
            .DataBind()
            .SelectedIndex = 0
        End With

        With Me.DropDownListCompanies
            .DataSource = companiesAdapter.GetCompanies
            .DataTextField = "CompanyName"
            .DataValueField = "CompanyID"
            .DataBind()
            .SelectedIndex = 0
        End With

        With Me.DropDownListSelectCompanies
            .DataSource = companiesAdapter.GetCompanies
            .DataTextField = "CompanyName"
            .DataValueField = "CompanyID"
            .DataBind()
            .SelectedIndex = 0
        End With

    End Sub

    Protected Sub GridViewUsers_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewUsers.PageIndexChanging
        GridViewUsers.PageIndex = e.NewPageIndex
        BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewUsers.RowCommand
        If e.CommandName = "PopulateUserInfo" Then
            PopulateUserInfo(e.CommandArgument.ToString)
            ViewState("Mode") = "Edit"
            ViewState("UN") = e.CommandArgument.ToString
        ElseIf e.CommandName = "DeleteUser" Then
            usersAdapter.DeleteUser(e.CommandArgument.ToString)
            ClearForm()
            ViewState("Mode") = "Add"
            Session("RefreshData") = 1
        End If
    End Sub

    Private Sub ClearForm()
        Me.TextBoxEmail.Text = ""
        Me.TextBoxFirstName.Text = ""
        Me.TextBoxLastName.Text = ""
        Me.TextBoxPassword.Text = ""
        Me.TextBoxPhone.Text = ""
        Me.TextBoxUserName.Text = ""
        Me.TextBoxSecurityAnswer.Text = ""
        Me.CheckBoxIsExternal.Checked = False
        Me.CheckBoxIsLocked.Checked = False
        'PopulateDropDowns()
        BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator
        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("Select").Index).Value = False
        End While

        ViewState("Mode") = "Add"

    End Sub

    Private Sub PopulateUserInfo(ByVal userName As String)
        Dim users As Security.UsersAndCompaniesAndSecurityQuestionsDataTable = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUserByUserName(userName)
        Dim user As Security.UsersAndCompaniesAndSecurityQuestionsRow = users.Rows(0)

        Me.TextBoxUserName.Text = user.UserName
        'Me.TextBoxPassword.Text = user.UserPassword
        Me.TextBoxPassword.Text = ""    'Do not display password.
        Me.TextBoxFirstName.Text = user.UserFirstName
        Me.TextBoxLastName.Text = user.UserLastName
        Me.TextBoxEmail.Text = user.UserEmailAddress
        Me.TextBoxPhone.Text = user.UserPhoneNumber
        Me.TextBoxSecurityAnswer.Text = user.UserSecurityAnswer


        If Not user.UserSecurityQuestion = "" Then
            Me.DropDownListSecurityQuestions.SelectedValue = user.SecurityQuestionID.ToString
        End If

        Me.DropDownListCompanies.SelectedValue = user.CompanyID.ToString

        If user.UserIsLockedOut Then
            Me.CheckBoxIsLocked.Checked = True
        Else
            Me.CheckBoxIsLocked.Checked = False
        End If

        If user.UserIsExternal Then
            Me.CheckBoxIsExternal.Checked = True
        Else
            Me.CheckBoxIsExternal.Checked = False
        End If

        ChangeFieldsStatus(user.UserIsExternal)

        'Check users roles.
        CheckUsersRoles(user.UserID)

    End Sub

    Private Sub CheckUsersRoles(ByVal userID As Guid)

        Dim rolesUsers As Security.RolesUsersDataTable = rolesUsersAdapter.GetRolesIDsByUserIDs(userID)

        For Each rolesUser As Security.RolesUsersRow In rolesUsers.Rows

            Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator
            While en.MoveNext
                Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
                If row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("RoleID").Index).Value = rolesUser.RoleID Then
                    row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("Select").Index).Value = True
                End If
            End While

        Next

    End Sub

    Private Sub ChangeFieldsStatus(ByVal userIsExternal As Boolean)
        If userIsExternal Then
            Me.TextBoxEmail.Enabled = True
            Me.TextBoxFirstName.Enabled = True
            Me.TextBoxLastName.Enabled = True
            Me.TextBoxPassword.Enabled = True
            Me.TextBoxPhone.Enabled = True
            Me.TextBoxSecurityAnswer.Enabled = True
            Me.DropDownListSecurityQuestions.Enabled = True

        Else
            Me.TextBoxEmail.Enabled = False
            Me.TextBoxFirstName.Enabled = False
            Me.TextBoxLastName.Enabled = False
            Me.TextBoxPassword.Enabled = False
            Me.TextBoxPhone.Enabled = False
            Me.TextBoxSecurityAnswer.Enabled = False
            Me.DropDownListSecurityQuestions.Enabled = False

        End If
    End Sub

    Protected Sub GridViewUsers_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GridViewUsers.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim deleteLink As LinkButton = e.Row.FindControl("LinkButton2")
            deleteLink.Attributes.Add("onclick", "javascript:return confirm('Do you want to delete user: " & _
                                                    DataBinder.Eval(e.Row.DataItem, "UserName") & "? ');")
        End If
    End Sub

    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
        PopulateDropDowns()
    End Sub

    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click

        Dim userName As String = Me.TextBoxUserName.Text.Replace("'", "''")
        Dim password As String = Me.TextBoxPassword.Text.Replace("'", "''")
        'password = Tva.Security.Application.User.EncryptPassword(password)
        Dim firstName As String = Me.TextBoxFirstName.Text.Replace("'", "''")
        Dim lastName As String = Me.TextBoxLastName.Text.Replace("'", "''")
        Dim email As String = Me.TextBoxEmail.Text.Replace("'", "''")
        Dim securityAnswer As String = Me.TextBoxSecurityAnswer.Text.Replace("'", "''")
        Dim phone As String = Me.TextBoxPhone.Text.Replace("'", "")
        phone = phone.Replace("-", "")
        phone = phone.Replace("(", "")
        phone = phone.Replace(")", "")
        Dim companyID As Guid = New Guid(Me.DropDownListCompanies.SelectedValue)
        Dim securityQnID As Guid = New Guid(Me.DropDownListSecurityQuestions.SelectedValue)
        Dim isLockedOut As Boolean = Me.CheckBoxIsLocked.Checked
        Dim isExternal As Boolean = Me.CheckBoxIsExternal.Checked

        If ViewState("Mode") = "Add" Then
            usersAdapter.InsertUser(companyID, securityQnID, userName, Tva.Security.Application.User.EncryptPassword(password), firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal)
            UpdateUsersRoles(userName.Replace(" ", ""))
        Else
            If Not ViewState("UN") = "" Then
                If password = "" Then
                    usersAdapter.UpdateUserWithoutPassword(companyID, securityQnID, userName, firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal, ViewState("UN"))
                Else
                    usersAdapter.UpdateUser(companyID, securityQnID, userName, Tva.Security.Application.User.EncryptPassword(password), firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal, ViewState("UN"))
                End If

                UpdateUsersRoles(userName.Replace(" ", ""))
            End If
        End If

        ClearForm()
        Session("RefreshData") = 1

    End Sub

    Private Sub UpdateUsersRoles(ByVal userName As String)

        Dim userID As Guid = usersAdapter.GetUserIDByUserName(userName)
        rolesUsersAdapter.DeleteRolesUsersByUserID(userID)

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator

        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            'Response.Write(row.Cells(0).Value.ToString & "<BR>")
            If row.Cells(0).Value = True Then
                rolesUsersAdapter.InsertRolesUsers(row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("RoleID").Index).Value, userID)
            End If
        End While

    End Sub

    Protected Sub DropDownListSelectCompanies_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DropDownListSelectCompanies.SelectedIndexChanged
        ClearForm()
        Me.DropDownListCompanies.SelectedValue = Me.DropDownListSelectCompanies.SelectedValue
    End Sub

    Protected Sub ButtonSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSearch.Click
        Dim searchStr As String = Me.TextBoxSearch.Text.Replace("'", "''")
        searchStr = searchStr.Replace("%", "")

        With Me.GridViewUsers
            .DataSource = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUsersByCompanyName(Me.DropDownListSelectCompanies.SelectedItem.ToString).Select("UserName Like '%" & searchStr & "%' OR UserFirstName Like '%" & searchStr & "%' OR UserLastName Like '%" & searchStr & "%'")
            .DataBind()
        End With
    End Sub

    Protected Sub LinkButtonShowAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LinkButtonShowAll.Click
        Me.TextBoxSearch.Text = ""
        ClearForm()
    End Sub

    Protected Sub GridViewUsers_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewUsers.Sorting
        Dim dv As New DataView
        dv = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUsersByCompanyName(Me.DropDownListSelectCompanies.SelectedItem.ToString).DefaultView
        dv.Sort = e.SortExpression
        With Me.GridViewUsers
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


        With Me.UltraWebGridRoles.Bands(0).Columns
            .FromKey("ApplicationID").Hidden = True
            .FromKey("ApplicationName").Width = New Unit(100)
            .FromKey("ApplicationDescription").Width = New Unit(500)
        End With

        With Me.UltraWebGridRoles.Bands(1).Columns
            .FromKey("Select").Move(0)
            .FromKey("RoleName").Move(1)
            .FromKey("ApplicationID").Hidden = True
            .FromKey("RoleID").Hidden = True
            .FromKey("Select").Width = New Unit(75)
            .FromKey("RoleName").Width = New Unit(125)
            .FromKey("RoleDescription").Width = New Unit(400)
        End With

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshData") = 1 Then
            PopulateDropDowns()
            If Me.DropDownListSelectCompanies.Items.Count > 0 Then
                BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
            End If
            BindRolesToGrid()
        End If
    End Sub
End Class
