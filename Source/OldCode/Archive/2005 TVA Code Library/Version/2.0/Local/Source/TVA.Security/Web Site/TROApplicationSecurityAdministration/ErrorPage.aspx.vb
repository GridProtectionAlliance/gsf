
Partial Class ErrorPage
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim ex As Exception = HttpContext.Current.Server.GetLastError

        If TypeOf ex Is HttpUnhandledException AndAlso ex.InnerException IsNot Nothing Then
            ex = ex.InnerException
        End If

        If ex IsNot Nothing Then
            Me.LabelErrorDetail.Text = ex.Message
        Else
            Me.LabelErrorDetail.Text = ""
        End If

    End Sub
End Class
