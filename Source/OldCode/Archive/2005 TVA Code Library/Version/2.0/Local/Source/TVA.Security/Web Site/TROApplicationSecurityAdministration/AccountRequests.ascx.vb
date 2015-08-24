Imports SecurityTableAdapters
Imports System.Data

Partial Class AccountRequests
    Inherits System.Web.UI.UserControl

    Private accountRequestsAdapter As New AccountRequestsAndApplicationsTableAdapter
    Private queryAdapter As New QueriesTableAdapter
    Private companiesAdapter As New CompaniesTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then
            'BindToGrid()
            Me.ButtonDisapprove.Visible = False
            Me.ButtonApprove.Visible = False
        End If

    End Sub

    Private Sub BindToGrid()
        With Me.GridViewRequests
            .DataSource = accountRequestsAdapter.GetAccountRequests()
            .DataBind()
        End With
    End Sub

    Private Sub PopulateCompanies()
        With Me.DropDownListCompanies
            .Items.Clear()
            .DataSource = companiesAdapter.GetCompanies
            .DataTextField = "CompanyName"
            .DataValueField = "CompanyID"
            .DataBind()
            .Items.Insert(0, New ListItem("Select Company", ""))
            .SelectedIndex = 0
        End With
    End Sub

    Protected Sub GridViewRequests_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewRequests.PageIndexChanging
        Me.GridViewRequests.PageIndex = e.NewPageIndex
        Me.BindToGrid()
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewRequests.RowCommand
        If e.CommandName = "PopulateRequestInfo" Then
            PopulateRequestInfo(e.CommandArgument.ToString)
            ViewState("UserName") = e.CommandArgument.ToString
        End If
    End Sub

    Private Sub PopulateRequestInfo(ByVal userName As String)
        Dim accountRequests As Security.AccountRequestsAndApplicationsDataTable = accountRequestsAdapter.GetAccountRequestByUserName(userName)
        Dim accountRequest As Security.AccountRequestsAndApplicationsRow = accountRequests.Rows(0)

        Me.LabelEmail.Text = accountRequest.UserEmailAddress
        Me.LabelFirstName.Text = accountRequest.UserFirstName
        Me.LabelLastName.Text = accountRequest.UserLastName
        Me.LabelPhone.Text = accountRequest.UserPhoneNumber
        Me.LabelUserName.Text = accountRequest.UserName
        Me.LabelCompany.Text = accountRequest.UserCompanyName

        Me.ButtonApprove.Visible = True
        Me.ButtonDisapprove.Visible = True
    End Sub

    Private Sub ClearForm()

    End Sub

    Protected Sub ButtonDisapprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonDisapprove.Click
        If Me.TextBoxReason.Text = "" Then
            Me.LabelError.Text = "Please provide a reason for disapproval of this request."
        Else
            queryAdapter.DisapproveAccountRequest(ViewState("UserName").ToString, Me.TextBoxReason.Text.Replace("'", "''"))
            Me.LabelError.Text = ""
        End If
        
    End Sub

    Protected Sub ButtonApprove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonApprove.Click

        If Me.DropDownListCompanies.SelectedValue = "" Then
            Me.LabelError.Text = "Please select a company from the dropdown."
        Else
            queryAdapter.ApproveAccountRequest(ViewState("UserName").ToString, Me.DropDownListCompanies.SelectedItem.ToString)
            Me.LabelError.Text = ""
        End If

    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        BindToGrid()
        PopulateCompanies()
    End Sub

    Protected Sub GridViewRequests_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs) Handles GridViewRequests.Sorting
        Dim dv As New DataView
        dv = accountRequestsAdapter.GetAccountRequests().DefaultView
        dv.Sort = e.SortExpression
        With Me.GridViewRequests
            .DataSource = dv
            .DataBind()
        End With
    End Sub
End Class
