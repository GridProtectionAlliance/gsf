Imports SecurityTableAdapters
Imports System.Data

Partial Class UsersForRoles
    Inherits System.Web.UI.UserControl

    Private usersAndCompaniesAdapter As New UsersAndCompaniesAndSecurityQuestionsTableAdapter
    Private rolesUsersDetailAdapter As New RolesUsersDetailTableAdapter
    Private rolesUsersAdapter As New RolesUsersTableAdapter
    Private rolesAdapter As New RolesTableAdapter
    Private usersAdapter As New UsersTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindToGrid()
            ViewState("RN") = ""
        End If
    End Sub

    Private Sub BindToGrid()
        With Me.GridViewUsers
            .DataSource = usersAndCompaniesAdapter.GetUsersInfo
            .DataBind()
        End With
    End Sub

    Public Sub ClearCheckBoxes()
        For Each row As GridViewRow In Me.GridViewUsers.Rows
            DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = False
        Next
        ViewState("RN") = ""
    End Sub

    Public Sub LoadData(ByVal roleName As String)

        ClearCheckBoxes()
        ViewState("RN") = roleName
        Dim rolesUsersDetailTable As Security.RolesUsersDetailDataTable = rolesUsersDetailAdapter.GetRolesUsersDetail(roleName)

        For Each rolesUsersDetailRow As Security.RolesUsersDetailRow In rolesUsersDetailTable.Rows
            For Each row As GridViewRow In Me.GridViewUsers.Rows
                If row.Cells(1).Text = rolesUsersDetailRow.UserName Then
                    DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = True
                End If
            Next
        Next

    End Sub

    Public Sub InsertIntoRolesUsers(ByVal roleName As String)
        DeleteFromRolesUsers(roleName)

        Dim roleID As Guid = rolesAdapter.GetRoleID(roleName)

        For Each row As GridViewRow In Me.GridViewUsers.Rows
            If DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = True Then
                Dim userID As Guid = usersAdapter.GetUserIDByUserName(row.Cells(1).Text.ToString)
                rolesUsersAdapter.InsertRolesUsers(roleID, userID)
            End If
        Next

    End Sub

    Public Sub DeleteFromRolesUsers(ByVal roleName As String)
        Dim roleID As Guid = rolesAdapter.GetRoleID(roleName)
        rolesUsersAdapter.DeleteRolesUsers(roleID)
    End Sub

    Protected Sub GridViewUsers_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewUsers.PageIndexChanging
        Me.GridViewUsers.PageIndex = e.NewPageIndex
        Me.BindToGrid()
    End Sub

    Protected Sub GridViewUsers_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewUsers.Sorting
        Dim dv As New DataView
        dv = usersAndCompaniesAdapter.GetUsersInfo.DefaultView()
        dv.Sort = e.SortExpression
        With Me.GridViewUsers
            .DataSource = dv
            .DataBind()
        End With

        If Not ViewState("RN") = "" Then
            LoadData(ViewState("RN"))
        End If
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshUsers") = 1 Then
            BindToGrid()
        End If
    End Sub
End Class
