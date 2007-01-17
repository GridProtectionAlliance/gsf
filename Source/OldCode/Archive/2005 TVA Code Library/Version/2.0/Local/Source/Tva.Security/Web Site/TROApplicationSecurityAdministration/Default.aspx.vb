Imports SecurityTableAdapters
Imports Tva.Security.Application

Partial Class _Default
    Inherits Tva.Web.UI.SecurePage

    Public Sub New()
        MyBase.New("TRO_APP_SEC", SecurityServer.Development, False)
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Session("RefreshApps") = 0
            Session("RefreshRoles") = 0
            Session("RefreshGroups") = 0
            Session("RefreshUsers") = 0
            Session("RefreshCompanies") = 0

            Dim context As System.Security.Principal.WindowsImpersonationContext
            context = Tva.Identity.Common.ImpersonateUser("esocss", "pwd4ctrl", "TVA")
            Me.LabelUser.Text = New Tva.Identity.UserInfo(System.Threading.Thread.CurrentPrincipal.Identity.Name).FullName.ToString & " (" & System.Threading.Thread.CurrentPrincipal.Identity.Name & ")"
            Tva.Identity.Common.EndImpersonation(context)
            'Response.Write(System.Threading.Thread.CurrentPrincipal.Identity.Name & Page.User.Identity.Name & My.User.CurrentPrincipal.Identity.Name)
        End If

    End Sub

    Protected Sub Page_LoginSuccessful(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoginSuccessful

        With Me.UltraWebTabTools.Tabs

            If Me.SecurityProvider.User.FindRole("TRO_APP_SEC_ADMIN") IsNot Nothing Then
                .Item(5).Visible = True
                .Item(6).Visible = True
                .Item(7).Visible = True
                .Item(8).Visible = True
            Else
                .Item(5).Visible = False
                .Item(6).Visible = False
                .Item(7).Visible = False
                .Item(8).Visible = False
            End If

        End With

    End Sub

    Protected Sub Page_PreRenderComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRenderComplete
        Session("RefreshApps") = 0
        Session("RefreshRoles") = 0
        Session("RefreshGroups") = 0
        Session("RefreshUsers") = 0
        Session("RefreshCompanies") = 0
    End Sub

End Class
