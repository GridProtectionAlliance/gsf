Imports SecurityTableAdapters
Imports System.Data
Imports System.Security.Principal
Imports System.DirectoryServices
Imports Tva.Identity.Common

Partial Class Users
    Inherits Tva.Web.UI.SecureUserControl

    Private usersAdapter As New UsersTableAdapter
    Private rolesAdapter As New RolesTableAdapter
    Private rolesUsersAdapter As New RolesUsersTableAdapter
    Private appsAdapter As New ApplicationsTableAdapter
    Private companiesAdapter As New CompaniesTableAdapter
    Private securityQuestionsAdapter As New SecurityQuestionsTableAdapter
    Private userAndCompaniesAndSecurityQuestionsAdapter As New UsersAndCompaniesAndSecurityQuestionsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Me.SecurityProvider.User.FindRole("TRO_APP_SEC_ADMIN") Is Nothing And _
                                Me.SecurityProvider.User.FindRole("TRO_APP_SEC_EDITOR") Is Nothing Then
            Me.ButtonSave.Visible = False
        Else
            Me.ButtonSave.Visible = True
        End If

        If Not IsPostBack Then
            PopulateDropDowns()
            If Me.DropDownListSelectCompanies.Items.Count > 0 Then
                BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
            End If
            BindRolesToGrid()

            ViewState("Company") = Me.DropDownListSelectCompanies.SelectedValue.ToString
            ViewState("Mode") = "Add"
            ViewState("UN") = ""

            'Assign default button to search text box.
            Me.TextBoxSearch.Attributes.Add("onkeypress", "return btnClick(event,'" + Me.ButtonSearch.ClientID + "')")
            Me.CheckBoxIsExternal.Attributes.Add("onclick", "EnableExternalFields();")
        End If

    End Sub

    Private Sub BindToGrid(ByVal companyName As String)

        With Me.GridViewUsers
            Dim searchStr As String = Me.TextBoxSearch.Text.Replace("'", "''").Replace("%", "")
            If searchStr = String.Empty Then
                .DataSource = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUsersByCompanyName(companyName)
            Else
                .DataSource = Me.userAndCompaniesAndSecurityQuestionsAdapter.GetUsersByCompanyName(Me.DropDownListSelectCompanies.SelectedItem.ToString).Select("UserName Like '%" & searchStr & "%' OR UserFirstName Like '%" & searchStr & "%' OR UserLastName Like '%" & searchStr & "%'")
            End If

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

        If Not ViewState("Company") = "" Then
            Me.DropDownListSelectCompanies.SelectedValue = ViewState("Company")
        End If

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
            Session("RefreshUsers") = 1
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
        Me.CheckBoxDoNotReplicate.Checked = False
        'PopulateDropDowns()
        'BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)

        Dim en As Infragistics.WebUI.UltraWebGrid.UltraGridRowsEnumerator = Me.UltraWebGridRoles.Bands(1).GetRowsEnumerator
        While en.MoveNext
            Dim row As Infragistics.WebUI.UltraWebGrid.UltraGridRow = en.Current
            row.Cells(Me.UltraWebGridRoles.Bands(1).Columns.FromKey("Select").Index).Value = False
        End While

        ViewState("Mode") = "Add"

    End Sub

    Private Sub PopulateUserInfo(ByVal userName As String)
        ClearForm()
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

        If user.UserReplicate Then
            Me.CheckBoxDoNotReplicate.Checked = False
        Else
            Me.CheckBoxDoNotReplicate.Checked = True
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
        'If e.Row.RowType = DataControlRowType.DataRow Then
        '    Dim deleteLink As LinkButton = e.Row.FindControl("LinkButton2")
        '    deleteLink.Attributes.Add("onclick", "javascript:return confirm('Do you want to delete user: " & _
        '                                            DataBinder.Eval(e.Row.DataItem, "UserName") & "? ');")
        'End If
    End Sub

    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
        'PopulateDropDowns()
    End Sub

    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click

        Dim userName, password, firstName, lastName, email, securityAnswer, phone As String
        Dim companyID, securityQnID As Guid
        Dim isLockedOut, isExternal, userReplicate As Boolean

        userName = Me.TextBoxUserName.Text.Replace("'", "").Replace(" ", "_")
        If userName = "" Or userName = "_" Then
            Me.LabelMsg.Text = "Invalid User Name."
            Exit Sub
        End If
        userName = userName.ToUpper

        companyID = New Guid(Me.DropDownListCompanies.SelectedValue)
        isLockedOut = Me.CheckBoxIsLocked.Checked
        isExternal = Me.CheckBoxIsExternal.Checked
        userReplicate = Not (Me.CheckBoxDoNotReplicate.Checked)
        password = Me.TextBoxPassword.Text.Replace("'", "''")
        firstName = Me.TextBoxFirstName.Text.Replace("'", "''")
        lastName = Me.TextBoxLastName.Text.Replace("'", "''")
        email = Me.TextBoxEmail.Text.Replace("'", "''")
        securityAnswer = Me.TextBoxSecurityAnswer.Text.Replace("'", "''")
        phone = Me.TextBoxPhone.Text.Replace("'", "")
        phone = phone.Replace("-", "")
        phone = phone.Replace("(", "")
        phone = phone.Replace(")", "")
        securityQnID = New Guid(Me.DropDownListSecurityQuestions.SelectedValue)
        
        If ViewState("Mode") = "Add" Then

            'Before inserting internal or external users, make sure that username is unique.
            If usersAdapter.GetUserIDByUserName(userName).HasValue Then
                Me.LabelMsg.Text = "User Name already exists."
                Exit Sub
            End If

            If isExternal Then
                usersAdapter.InsertUser(companyID, securityQnID, userName, Tva.Security.Application.User.EncryptPassword(password), firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal, userReplicate)
            Else
                'If Internal User then company id must be TVA.
                companyID = companiesAdapter.GetCompanyIdByCompanyName("Tennessee Valley Authority")
                'Check if userName (NTID) exists in the active directory.
                If NtidExistInActiveDirectory(userName) Then
                    usersAdapter.InsertInternalUsers(companyID, userName, userReplicate)
                    Me.LabelMsg.Text = ""
                Else
                    Me.LabelMsg.Text = "User Name does not exist in the active directory."
                End If

            End If

            UpdateUsersRoles(userName.Replace(" ", "_"))
        Else
            If Not ViewState("UN") = "" Then

                If isExternal Then
                    If password = "" Then
                        usersAdapter.UpdateUserWithoutPassword(companyID, securityQnID, userName, firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal, userReplicate, ViewState("UN"))
                    Else
                        usersAdapter.UpdateUser(companyID, securityQnID, userName, Tva.Security.Application.User.EncryptPassword(password), firstName, lastName, phone, email, securityAnswer, isLockedOut, isExternal, userReplicate, ViewState("UN"))
                    End If
                Else

                    'If Internal User then company id must be TVA.
                    companyID = companiesAdapter.GetCompanyIdByCompanyName("Tennessee Valley Authority")
                    'Check if userName (NTID) exists in the active directory.
                    If NtidExistInActiveDirectory(userName) Then
                        usersAdapter.UpdateInternalUser(companyID, userName, userReplicate, ViewState("UN"))
                        Me.LabelMsg.Text = ""
                    Else
                        Me.LabelMsg.Text = "User Name does not exist in the active directory."
                    End If

                End If


                UpdateUsersRoles(userName.Replace(" ", "_"))
            End If
        End If

        ClearForm()
        Session("RefreshUsers") = 1

    End Sub

    Private Sub UpdateUsersRoles(ByVal userName As String)

        Dim userId As Guid = usersAdapter.GetUserIDByUserName(userName)
        rolesUsersAdapter.DeleteRolesUsersByUserID(userId)

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
        BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
        Me.DropDownListCompanies.SelectedValue = Me.DropDownListSelectCompanies.SelectedValue
        ViewState("Company") = Me.DropDownListSelectCompanies.SelectedValue.ToString
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
        BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
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
        With newCol
            .Key = "Select"
            .Header.Caption = "Select"
            .Type = Infragistics.WebUI.UltraWebGrid.ColumnType.CheckBox
            .AllowUpdate = Infragistics.WebUI.UltraWebGrid.AllowUpdate.Yes
        End With

        If Not Me.UltraWebGridRoles.Bands(1).Columns.IndexOf("Select") >= 0 Then
            Me.UltraWebGridRoles.Bands(1).Columns.Insert(0, newCol)
        End If

        With Me.UltraWebGridRoles.Bands(0).Columns
            .FromKey("ApplicationID").Hidden = True

            With .FromKey("ApplicationName")
                .Width = New Unit(100)
                .Header.Caption = "Application Name"
            End With

            With .FromKey("ApplicationDescription")
                .Width = New Unit(500)
                .Header.Caption = "Description"
            End With
        End With

        With Me.UltraWebGridRoles.Bands(1).Columns
            With .FromKey("Select")
                .Move(0)
                .CellStyle.HorizontalAlign = HorizontalAlign.Center
                .Width = New Unit(75)
                .Header.Caption = " Select "
            End With

            With .FromKey("RoleName")
                .Move(1)
                .Width = New Unit(125)
                .Header.Caption = "Role Name"
            End With

            .FromKey("ApplicationID").Hidden = True
            .FromKey("RoleID").Hidden = True

            With .FromKey("RoleDescription")
                .Width = New Unit(400)
                .Header.Caption = "Description"
            End With
        End With

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshCompanies") = 1 Then
            PopulateDropDowns()
            'If Me.DropDownListSelectCompanies.Items.Count > 0 Then
            '    BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
            'End If
        End If

        If Session("RefreshRoles") = 1 Then
            BindRolesToGrid()
        End If

        If Session("RefreshUsers") = 1 Then
            BindToGrid(Me.DropDownListSelectCompanies.SelectedItem.ToString)
        End If
    End Sub

    Private Function NtidExistInActiveDirectory(ByVal ntid As String) As Boolean
        Dim exists As Boolean = False

        Dim entry As DirectoryEntry = New DirectoryEntry("LDAP://TVA:389/dc=main,dc=tva,dc=gov", "esocss", "pwd4ctrl")
        Dim filterStr As String = ntid

        Dim mySearcher As New System.DirectoryServices.DirectorySearcher
        mySearcher.SearchRoot = entry
        mySearcher.SearchScope = SearchScope.Subtree
        mySearcher.Filter = String.Format("(&(objectCategory=person)(objectclass=user)(samaccountName={0}))", filterStr)

        Dim resEnt As SearchResult = mySearcher.FindOne

        exists = Not (resEnt Is Nothing)

        Return exists

    End Function

End Class
