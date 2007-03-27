
Partial Class DataPorting
    Inherits System.Web.UI.UserControl

    Private appsAdapter As New DevelopmentDSTableAdapters.ApplicationsTableAdapter

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            With Me.DropDownListSource
                With .Items
                    .Add(New ListItem("Development", "0"))
                    .Add(New ListItem("Acceptance", "1"))
                    .Add(New ListItem("Production", "2"))
                End With
                .SelectedValue = "0"
            End With

            With Me.DropDownListDestination
                With .Items
                    .Add(New ListItem("Development", "0"))
                    .Add(New ListItem("Acceptance", "1"))
                    .Add(New ListItem("Production", "2"))
                End With
                .SelectedValue = "0"
            End With

            With Me.DropDownListApplication
                .DataSource = appsAdapter.GetApplications
                .DataTextField = "ApplicationName"
                .DataValueField = "ApplicationID"
                .DataBind()
            End With
        End If
    End Sub

End Class
