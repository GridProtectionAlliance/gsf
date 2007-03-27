Imports SecurityTableAdapters
Imports System.Data

Partial Class AccessRequest
    Inherits System.Web.UI.UserControl

    Private accessRequestsAdapter As New AccessRequestsAndUsersAndApplicationsTableAdapter
    Private queryAdapter As New QueriesTableAdapter
    Private rolesAdapter As New RolesTableAdapter
    Private applicationsAdapter As New ApplicationsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Me.DropDownListRoles.Items.Add("Select Role")
            Me.ButtonDisapprove.Visible = False
            Me.ButtonApprove.Visible = False
        End If
    End Sub

    Private Sub BindToGrid()
        With Me.GridViewRequests
            .DataSource = accessRequestsAdapter.GetAccessRequests()
            .DataBind()
        End With
    End Sub

    Private Sub PopulateRoles(ByVal applicationId As Guid)
        With Me.DropDownListRoles
            .DataSource = rolesAdapter.GetRolesByApplicationID(applicationId)
            .DataTextField = "RoleName"
            .DataValueField = "RoleID"
            .DataBind()
        End With
    End Sub

    Protected Sub GridViewRequests_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewRequests.PageIndexChanging
        Me.GridViewRequests.PageIndex = e.NewPageIndex
        BindToGrid()
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewRequests.RowCommand
        If e.CommandName = "PopulateRequestInfo" Then
            Dim userName As String = e.CommandArgument.ToString.Substring(0, e.CommandArgument.ToString.IndexOf(","))
            Dim appName As String = e.CommandArgument.ToString.Substring(e.CommandArgument.ToString.IndexOf(",") + 1)
            PopulateRequestInfo(userName, appName)
            PopulateRoles(applicationsAdapter.GetApplicationId(appName))
            ViewState("UserName") = userName
            ViewState("AppName") = appName
        End If
    End Sub

    Private Sub PopulateRequestInfo(ByVal userName As String, ByVal applicationName As String)
        Dim accessRequests As Security.AccessRequestsAndUsersAndApplicationsDataTable = accessRequestsAdapter.GetAccessRequestByUserAndApplication(userName, applicationName)
        Dim accessRequest As Security.AccessRequestsAndUsersAndApplicationsRow = accessRequests.Rows(0)

        Me.LabelFirstName.Text = accessRequest.UserFirstName
        Me.LabelLastName.Text = accessRequest.UserLastName
        Me.LabelUserName.Text = accessRequest.UserName
        Me.LabelApplication.Text = accessRequest.ApplicationName

        Me.ButtonApprove.Visible = True
        Me.ButtonDisapprove.Visible = True
    End Sub

    Private Sub ClearForm()
        Me.LabelApplication.Text = ""
        Me.LabelFirstName.Text = ""
        Me.LabelLastName.Text = ""
        Me.LabelUserName.Text = ""
        Me.DropDownListRoles.Items.Clear()
        Me.DropDownListRoles.Items.Add("Select Role")

    End Sub

    Protected Sub ButtonDisapprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonDisapprove.Click
        If Me.TextBoxReason.Text = "" Then
            Me.LabelError.Text = "Please provide a reason for disapproval of this request."
        Else
            queryAdapter.DisapproveAccessRequest(ViewState("UserName"), ViewState("AppName"), Me.TextBoxReason.Text.Replace("'", "''"))
            'BindToGrid()
            ClearForm()
        End If

    End Sub

    Protected Sub ButtonApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonApprove.Click
        queryAdapter.ApproveAccessRequest(ViewState("UserName"), ViewState("AppName"), Me.DropDownListRoles.SelectedItem.ToString)
        'BindToGrid()
        ClearForm()
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        BindToGrid()
    End Sub

    Protected Sub GridViewRequests_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewRequests.Sorting
        Dim dv As New DataView
        dv = accessRequestsAdapter.GetAccessRequests().DefaultView
        dv.Sort = e.SortExpression
        With Me.GridViewRequests
            .DataSource = dv
            .DataBind()
        End With
    End Sub
End Class
