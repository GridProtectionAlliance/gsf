Imports SecurityTableAdapters

Partial Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Session("RefreshData") = 0
            Dim context As System.Security.Principal.WindowsImpersonationContext
            context = Tva.Identity.Common.ImpersonateUser("esocss", "pwd4ctrl", "TVA")
            Me.LabelUser.Text = New Tva.Identity.UserInfo(Page.User.Identity.Name).FullName.ToString & " (" & My.User.CurrentPrincipal.Identity.Name & ")"
            Tva.Identity.Common.EndImpersonation(context)
            'Response.Write(System.Threading.Thread.CurrentPrincipal.Identity.Name)
        End If

    End Sub

    Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
        Session("RefreshData") = 0
    End Sub
End Class
