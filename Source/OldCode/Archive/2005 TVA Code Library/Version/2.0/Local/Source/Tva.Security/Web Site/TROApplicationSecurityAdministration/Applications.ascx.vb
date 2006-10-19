Imports SecurityTableAdapters

''' <summary>
''' Allows users to manage information about the applications.
''' </summary>
''' <remarks></remarks>
Partial Class Applications
    Inherits System.Web.UI.UserControl

    Private appsAdapter As New ApplicationsTableAdapter

    ''' <summary>
    ''' On Page load, populate application drop down and set default values for the viewstate variables.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then
            'Populate dropdown list with the list of applications available in the database.
            BindToGrid()

            ViewState("Mode") = "Add"
            ViewState("App") = ""

            'Assign default button to search text box.
            Me.TextBoxSearch.Attributes.Add("onkeypress", "return btnClick(event,'" + Me.ButtonSearch.ClientID + "')")
        End If

    End Sub

    Private Sub BindToGrid()
        With Me.GridViewApplications
            .DataSource = appsAdapter.GetApplications
            .DataBind()
        End With
    End Sub

    Private Sub ClearForm()
        Me.TextBoxName.Text = ""
        Me.TextBoxDescription.Text = ""
        ViewState("Mode") = "Add"
        BindToGrid()
    End Sub

    Protected Sub GridViewApplications_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridViewApplications.PageIndexChanging
        Me.GridViewApplications.PageIndex = e.NewPageIndex
        Me.BindToGrid()
    End Sub

    Protected Sub HandleUserClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridViewApplications.RowCommand
        If e.CommandName = "PopulateApplicationInfo" Then
            PopulateApplicationInfo(e.CommandArgument.ToString)
            ViewState("Mode") = "Edit"
            ViewState("App") = e.CommandArgument.ToString
        ElseIf e.CommandName = "DeleteApplication" Then
            appsAdapter.DeleteApplication(e.CommandArgument.ToString)
            ClearForm()
            ViewState("Mode") = "Add"
            Session("RefreshData") = 1
        End If
    End Sub

    Protected Sub PopulateApplicationInfo(ByVal applicationName As String)

        Dim apps As Security.ApplicationsDataTable = appsAdapter.GetApplicationByApplicationName(applicationName)
        Dim app As Security.ApplicationsRow = apps.Rows(0)

        Me.TextBoxName.Text = app.ApplicationName
        Me.TextBoxDescription.Text = app.ApplicationDescription

    End Sub

    ''' <summary>
    ''' Inserts or updates applications table in the database.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub ButtonSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSave.Click
        Dim newAppName As String = Me.TextBoxName.Text.Replace("'", "''")
        Dim newAppDescription As String = Me.TextBoxDescription.Text.Replace("'", "''")

        If ViewState("Mode") = "Add" Then
            appsAdapter.InsertApplication(newAppName, newAppDescription)
        Else
            If Not ViewState("App") = "" Then   'if viewstate information is set then use it
                appsAdapter.UpdateApplication(newAppName, newAppDescription, ViewState("App"))
            End If

        End If
        ClearForm()
        Session("RefreshData") = 1

    End Sub

    Protected Sub GridViewApplications_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles GridViewApplications.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim deleteLink As LinkButton = e.Row.FindControl("LinkButton2")
            deleteLink.Attributes.Add("onclick", "javascript:return confirm('Do you want to delete application: " & _
                                                    DataBinder.Eval(e.Row.DataItem, "ApplicationName") & "? ');")
        End If
    End Sub

    Protected Sub ButtonCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        ClearForm()
    End Sub

    Protected Sub ButtonSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSearch.Click
        Dim searchStr As String = Me.TextBoxSearch.Text.Replace("'", "''")
        searchStr = searchStr.Replace("%", "")

        With Me.GridViewApplications
            .DataSource = appsAdapter.GetApplications().Select("ApplicationName Like '%" & searchStr & "%' Or ApplicationDescription Like '%" & searchStr & "%'")
            .DataBind()
        End With
    End Sub

    Protected Sub LinkButtonShowAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LinkButtonShowAll.Click
        Me.TextBoxSearch.Text = ""
        ClearForm()
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Session("RefreshData") = 1 Then
            BindToGrid()
        End If
    End Sub
End Class
