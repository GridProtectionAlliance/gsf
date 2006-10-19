Imports SecurityTableAdapters
Imports System.Data

Partial Class GroupsForRoles
    Inherits System.Web.UI.UserControl

    Private groupsAdapter As New GroupsTableAdapter
    Private rolesGroupsDetailAdapter As New RolesGroupsDetailTableAdapter
    Private rolesGroupsAdapter As New RolesGroupsTableAdapter
    Private rolesAdapter As New RolesTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindToGrid()
        End If
    End Sub

    Private Sub BindToGrid()
        With Me.GridViewGroups
            .DataSource = groupsAdapter.GetGroups
            .DataBind()
        End With
    End Sub

    Public Sub ClearCheckBoxes()
        For Each row As GridViewRow In Me.GridViewGroups.Rows
            DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = False
        Next
    End Sub

    Public Sub LoadData(ByVal roleName As String)

        ClearCheckBoxes()

        Dim rolesGroupsDetailTable As Security.RolesGroupsDetailDataTable = rolesGroupsDetailAdapter.GetRolesGroupsDetail(roleName)

        For Each rolesGroupsDetailRow As Security.RolesGroupsDetailRow In rolesGroupsDetailTable.Rows

            For Each row As GridViewRow In Me.GridViewGroups.Rows
                If row.Cells(1).Text = rolesGroupsDetailRow.GroupName Then
                    DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = True
                End If
            Next
        Next

    End Sub

    Public Sub InsertIntoRolesGroups(ByVal roleName As String)
        DeleteFromRolesGroups(roleName)

        Dim roleID As Guid = rolesAdapter.GetRoleID(roleName)

        For Each row As GridViewRow In Me.GridViewGroups.Rows
            If DirectCast(row.FindControl("CheckBox1"), CheckBox).Checked = True Then
                Dim groupID As Guid = groupsAdapter.GetGroupIDByGroupName(row.Cells(1).Text.ToString)
                rolesGroupsAdapter.InsertRolesGroups(roleID, groupID)
            End If
        Next
    End Sub

    Public Sub DeleteFromRolesGroups(ByVal roleName As String)
        Dim roleID As Guid = rolesAdapter.GetRoleID(roleName)
        rolesGroupsAdapter.DeleteRolesGroups(roleID)
    End Sub

    Protected Sub GridViewGroups_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewGroups.PageIndexChanging
        Me.GridViewGroups.PageIndex = e.NewPageIndex
        BindToGrid()
    End Sub

    Protected Sub GridViewGroups_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewGroups.Sorting
        Dim dv As New DataView
        dv = groupsAdapter.GetGroups.DefaultView()
        dv.Sort = e.SortExpression
        With Me.GridViewGroups
            .DataSource = dv
            .DataBind()
        End With
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshData") = 1 Then
            BindToGrid()
        End If
    End Sub
End Class
