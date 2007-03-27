Imports SecurityTableAdapters
Imports System.Data

Partial Class ShowUsers
    Inherits System.Web.UI.Page

    Private groupsAndUsersAdapter As New GroupsAndUsersTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then

            If Request("GroupName") IsNot Nothing AndAlso _
                Not String.IsNullOrEmpty(Request("GroupName").ToString()) Then

                Me.LabelGroupName.Text = "Users For Group: " & Request("GroupName")
                ViewState("GN") = Request("GroupName")
                BindToGrid(ViewState("GN"))

                Page.Header.Title = "Users For Group: " & Request("GroupName")

            End If

        End If

    End Sub

    Private Sub BindToGrid(ByVal groupName As String)

        Dim users As Security.GroupsAndUsersDataTable = groupsAndUsersAdapter.GetUsersByGroupName(groupName)

        With Me.GridViewUsers
            .DataSource = users
            .DataBind()
        End With
    End Sub

    Protected Sub GridViewUsers_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewUsers.PageIndexChanging
        Me.GridViewUsers.PageIndex = e.NewPageIndex
        BindToGrid(ViewState("GN"))
    End Sub

    Protected Sub GridViewUsers_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewUsers.Sorting
        Dim dv As New DataView
        dv = groupsAndUsersAdapter.GetUsersByGroupName(ViewState("GN")).DefaultView
        dv.Sort = e.SortExpression

        With Me.GridViewUsers
            .DataSource = dv
            .DataBind()
        End With

    End Sub
End Class
