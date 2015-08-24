
Partial Class DataPorting
    Inherits System.Web.UI.UserControl

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then

            Dim appsAdapter As New DevelopmentDSTableAdapters.ApplicationsTableAdapter
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

            Me.ButtonCompare.Attributes.Add("OnClick", "CompareSources()")

        End If

    End Sub

    Protected Sub ButtonCompare_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonCompare.Click

        Dim appsAdapter As Object

        If Me.DropDownListSource.SelectedValue = "0" Then
            appsAdapter = New DevelopmentDSTableAdapters.ApplicationsTableAdapter
        ElseIf Me.DropDownListSource.SelectedValue = "1" Then

        ElseIf Me.DropDownListSource.SelectedValue = "2" Then

        End If

    End Sub

End Class
