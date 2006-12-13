
Partial Class ErrorPage
    Inherits System.Web.UI.Page

    Private Enum ErrorTypes As Integer
        AccessDenied
        AppNotConfig
        LockedOut
    End Enum

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then

            SetSessions()
            If Request("t") IsNot Nothing AndAlso _
                Not String.IsNullOrEmpty(Request("t").ToString()) Then

                Select Case CType(Request("t").ToString(), ErrorTypes)
                    Case ErrorTypes.AccessDenied
                        Me.LabelErrorType.Text = "Access Denied"
                        Me.LabelDetailError.Text = "You are not authorized to view this page. <BR>Please request an access by clicking on Request Application Access link below."
                    Case ErrorTypes.AppNotConfig
                        Me.LabelErrorType.Text = "Application Configuration Error"
                        Me.LabelDetailError.Text = "Client application is not configured properly."
                    Case ErrorTypes.LockedOut
                        Me.LabelErrorType.Text = "Account Locked Out"
                        Me.LabelDetailError.Text = "Your account has been locked out."
                    Case Else
                        Me.LabelErrorType.Text = "Error Occured"
                        Me.LabelDetailError.Text = "Error occured while processing your request. <BR>Please check that your username and password is correct." & _
                                                    "<BR>If you do not have an account please apply for one."
                End Select

            Else
                Me.LabelErrorType.Text = "Error Occured"
                Me.LabelDetailError.Text = "Error occured while processing your request. <BR>Please call the TVA Information Technology Service Center at 423-751-4357."

            End If

        End If
    End Sub

    Private Sub SetSessions()

        If Session("ConnectionString") Is Nothing AndAlso Request("c") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request("c").ToString()) Then
            Dim connectionString As String = Tva.Security.Cryptography.Common.Decrypt(Request("c").ToString, Tva.Security.Cryptography.EncryptLevel.Level4)
            Session("ConnectionString") = connectionString
        End If

        If Session("ApplicationName") Is Nothing AndAlso Request("a") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request("a").ToString()) Then
            Dim applicationName As String = Server.UrlDecode(Tva.Security.Cryptography.Common.Decrypt(Request("a").ToString, Tva.Security.Cryptography.EncryptLevel.Level4))
            Session("ApplicationName") = applicationName
        End If

        If Session("ReturnUrl") Is Nothing AndAlso Request("r") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Request("r").ToString()) Then
            Dim returnUrl As String = Server.UrlDecode(Request("r").ToString)
            Session("ReturnUrl") = returnUrl
        End If

    End Sub

End Class
