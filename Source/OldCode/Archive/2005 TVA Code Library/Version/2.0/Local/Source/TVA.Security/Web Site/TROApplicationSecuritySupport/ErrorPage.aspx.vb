
Partial Class ErrorPage
    Inherits System.Web.UI.Page

    Private Enum ErrorTypes As Integer
        AccessDenied
        AppNotConfig
        LockedOut
        PasswordFormat
    End Enum

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If String.Compare(Request.Url.Scheme, "https", True) <> 0 Then
            Me.ImageLogo.ImageUrl = "Images/LogoInternal.jpg"
        End If
        If Not IsPostBack Then
            SetSessions()
            If Request("t") IsNot Nothing AndAlso _
                Not String.IsNullOrEmpty(Request("t").ToString()) Then

                Select Case CType(Request("t").ToString(), ErrorTypes)
                    Case ErrorTypes.AccessDenied
                        Me.LabelErrorType.Text = "Access Denied"
                        Me.LabelDetailError.Text = "You are not authorized to view this page. <BR>Please request an access by calling TVA Operations Duty Specialist at 423-751-1700."
                    Case ErrorTypes.AppNotConfig
                        Me.LabelErrorType.Text = "Application Configuration Error"
                        Me.LabelDetailError.Text = "Client application is not configured properly."
                    Case ErrorTypes.LockedOut
                        Me.LabelErrorType.Text = "Account Locked Out"
                        Me.LabelDetailError.Text = "Your account has been locked out."
                    Case ErrorTypes.PasswordFormat
                        Me.LabelErrorType.Text = "Incorrect Password Format"
                        Me.LabelDetailError.Text = "Password does not meet the following criteria:<br />" & _
                                "<ul>" & "<li>Password must be at least 8 characters</li>" & _
                                        "<li>Password must contain at least 1 digit</li>" & _
                                        "<li>Password must contain at least 1 upper case letter</li>" & _
                                        "<li>Password must contain at least 1 lower case letter</li>" & "</ul>"
                    Case Else
                        Me.LabelErrorType.Text = "Error Occured"
                        Me.LabelDetailError.Text = "Error occured while processing your request. <BR>Please check that your username and password is correct." & _
                                                    "<BR>If you do not have an account please apply for one by calling TVA Operations Duty Specialist at 423-751-1700."
                End Select

            Else
                Me.LabelErrorType.Text = "Error Occured"
                Me.LabelDetailError.Text = "Error occured while processing your request. <BR>Please call the TVA Operations Duty Specialist at 423-751-1700."

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
