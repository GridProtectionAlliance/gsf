Imports System.Data
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports TVA.Security.Application

Partial Class Login
    Inherits System.Web.UI.Page

    Private connectionString, applicationName, returnUrl As String
    Private queryStringExists, returnUrlHasQueryString As Boolean

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If String.IsNullOrEmpty(System.Threading.Thread.CurrentPrincipal.Identity.Name) Then
            Me.ImageLogo.ImageUrl = "Images/LogoExternal.jpg"
        Else
            Me.ImageLogo.ImageUrl = "Images/LogoInternal.jpg"
        End If

        Me.TextBoxUserName.Focus()
        If Not IsPostBack Then
            'Session("ApplicationName") = "TEST_APP_1"
            'Session("ConnectionString") = "Server=RGOCDSQL; Database=ApplicationSecurity; UID=appsec; PWD=123-xyz"
            'Session("ReturnUrl") = "http://mail.yahoo.com"

            'c = connection string
            'a = application name
            'r = return url
            't = error type

            'If Request("c") IsNot Nothing AndAlso _
            '        Not String.IsNullOrEmpty(Request("c").ToString()) AndAlso _
            '            Request("a") IsNot Nothing AndAlso _
            '                Not String.IsNullOrEmpty(Request("a").ToString()) AndAlso _
            '                    Request("r") IsNot Nothing AndAlso _
            '                        Not String.IsNullOrEmpty(Request("r").ToString()) Then

            '    ViewState("QueryStringExists") = True

            '    'connectionString = Server.UrlDecode(Tva.Security.Cryptography.Common.Decrypt(Request("c").ToString, Tva.Security.Cryptography.EncryptLevel.Level4))
            '    connectionString = Tva.Security.Cryptography.Common.Decrypt(Request("c").ToString, Tva.Security.Cryptography.EncryptLevel.Level4)
            '    applicationName = Server.UrlDecode(Tva.Security.Cryptography.Common.Decrypt(Request("a").ToString, Tva.Security.Cryptography.EncryptLevel.Level4))
            '    returnUrl = Server.UrlDecode(Request("r").ToString)
            '    Session("ApplicationName") = applicationName
            '    Session("ConnectionString") = connectionString
            '    Session("ReturnUrl") = returnUrl

            '    Dim returnUri As Uri = New Uri(returnUrl)
            '    Dim query As String = returnUri.Query
            '    If query.Length > 0 Then
            '        ViewState("ReturnUrlHasQueryString") = True
            '    Else
            '        ViewState("ReturnUrlHasQueryString") = False
            '    End If

            '    ' 11/15/2006 - PCP: This logic has been moved to the WebSecurityProvider component in TVA Code Library.
            '    ''Check for existance of the Cookie
            '    'If Request.Cookies("Credentials") IsNot Nothing Then
            '    '    Dim userName As String = Tva.Security.Cryptography.Common.Decrypt(Request.Cookies("Credentials")("u"), Tva.Security.Cryptography.EncryptLevel.Level4)
            '    '    Dim password As String = Tva.Security.Cryptography.Common.Decrypt(Request.Cookies("Credentials")("p"), Tva.Security.Cryptography.EncryptLevel.Level4)
            '    '    AuthenticateUser(userName, password)
            '    '    'Response.Write(userName & " - " & password & "<BR>Expires On: " & Request.Cookies("Credentials").Expires.ToString)
            '    'End If

            'ElseIf Session("ApplicationName") IsNot Nothing AndAlso _
            '        Session("ConnectionString") IsNot Nothing AndAlso _
            '        Session("ReturnUrl") IsNot Nothing Then

            '    applicationName = Session("ApplicationName")
            '    connectionString = Session("ConnectionString")
            '    returnUrl = Session("ReturnUrl")

            '    Dim returnUri As Uri = New Uri(Session("ReturnUrl"))
            '    Dim query As String = returnUri.Query
            '    If query.Length > 0 Then
            '        ViewState("ReturnUrlHasQueryString") = True
            '    Else
            '        ViewState("ReturnUrlHasQueryString") = False
            '    End If
            '    ViewState("QueryStringExists") = True

            'Else
            '    queryStringExists = False
            '    Response.Redirect("ErrorPage.aspx?t=1", True)
            'End If

            SetSessions()

            If Session("ApplicationName") IsNot Nothing AndAlso _
                    Session("ConnectionString") IsNot Nothing AndAlso _
                    Session("ReturnUrl") IsNot Nothing Then

                applicationName = Session("ApplicationName")
                connectionString = Session("ConnectionString")
                returnUrl = Session("ReturnUrl")

                '*** This lines are commented as there was problem with the API when using relative urls to redirect.
                '*** Changes made on 11/08/2007 by Mehul
                'Dim returnUri As Uri = New Uri(Session("ReturnUrl"))
                'Dim query As String = returnUri.Query
                'If query.Length > 0 Then
                '    ViewState("ReturnUrlHasQueryString") = True
                'Else
                '    ViewState("ReturnUrlHasQueryString") = False
                'End If
                If returnUrl.Contains("?") Then
                    ViewState("ReturnUrlHasQueryString") = True
                Else
                    ViewState("ReturnUrlHasQueryString") = False
                End If
                ViewState("QueryStringExists") = True

            Else
                queryStringExists = False
                Response.Redirect("ErrorPage.aspx?t=1", True)
            End If


            Session("LoginAttempts") = 0
        End If

    End Sub

    Protected Sub ButtonLogin_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonLogin.Click

        Dim userName, password As String
        userName = Me.TextBoxUserName.Text
        password = Me.TextBoxPassword.Text

        userName = userName.Replace("'", "")
        userName = userName.Replace("%", "")
        password = password.Replace("'", "")
        password = password.Replace("%", "")

        AuthenticateUser(userName, password)

    End Sub

    Private Sub AuthenticateUser(ByVal userName As String, ByVal password As String)

        Dim conn As New SqlConnection(Session("ConnectionString").ToString)
        Try

            Dim u As New User(userName, password, New Data.SqlClient.SqlConnection(Session("ConnectionString").ToString))
            With u 'New User(userName, password, New Data.SqlClient.SqlConnection(connectionString))

                If .IsAuthenticated Then

                    If .IsLockedOut = False Then
                        If .FindApplication(Session("ApplicationName").ToString) IsNot Nothing Then

                            'Check if password is not expired for this username. If so then force user to change passsword.\
                            If .PasswordChangeDateTime <> DateTime.MinValue AndAlso _
                                    DateDiff(DateInterval.Day, .PasswordChangeDateTime, DateTime.Now) >= 0 Then
                                Response.Redirect("ChangePassword.aspx?m=0", False)
                            Else

                                If ViewState("QueryStringExists") Then

                                    'u = user name
                                    'p = password
                                    't = error type
                                    If ViewState("ReturnUrlHasQueryString") Then
                                        returnUrl = Session("ReturnUrl") & "&u=" & Server.UrlEncode(Tva.Security.Cryptography.Common.Encrypt(userName, Tva.Security.Cryptography.EncryptLevel.Level4)) & "&p=" & Server.UrlEncode(Tva.Security.Cryptography.Common.Encrypt(password, Tva.Security.Cryptography.EncryptLevel.Level4))
                                    Else
                                        returnUrl = Session("ReturnUrl") & "?u=" & Server.UrlEncode(Tva.Security.Cryptography.Common.Encrypt(userName, Tva.Security.Cryptography.EncryptLevel.Level4)) & "&p=" & Server.UrlEncode(Tva.Security.Cryptography.Common.Encrypt(password, Tva.Security.Cryptography.EncryptLevel.Level4))
                                    End If

                                    If Not conn.State = ConnectionState.Open Then
                                        conn.Open()
                                    End If
                                    ExecuteNonQuery("LogAccess", conn, userName, Session("ApplicationName"), False)

                                    ' 11/15/2006 - PCP: This logic has been moved to the WebSecurityProvider component in TVA Code Library.
                                    ''Create Cookie here...
                                    ''On successful login, this will overwrite any previous values for this cookies.
                                    'Dim credentialCookie As New HttpCookie("Credentials")
                                    'credentialCookie.Values.Add("u", Tva.Security.Cryptography.Common.Encrypt(userName, Tva.Security.Cryptography.EncryptLevel.Level4))
                                    'credentialCookie.Values.Add("p", Tva.Security.Cryptography.Common.Encrypt(password, Tva.Security.Cryptography.EncryptLevel.Level4))
                                    'Response.Cookies.Add(credentialCookie)

                                    Response.Redirect(returnUrl, False)
                                Else
                                    If Not conn.State = ConnectionState.Open Then
                                        conn.Open()
                                    End If
                                    ExecuteNonQuery("LogAccess", conn, userName, Session("ApplicationName"), True)
                                    Response.Redirect("ErrorPage.aspx?t=1", False)

                                End If

                            End If

                        Else
                            If Not conn.State = ConnectionState.Open Then
                                conn.Open()
                            End If
                            ExecuteNonQuery("LogAccess", conn, userName, Session("ApplicationName"), True)
                            Response.Redirect("ErrorPage.aspx?t=0", False)

                        End If
                    Else
                        If Not conn.State = ConnectionState.Open Then
                            conn.Open()
                        End If
                        ExecuteNonQuery("LogAccess", conn, userName, Session("ApplicationName"), True)
                        Response.Redirect("ErrorPage.aspx?t=2", False)

                    End If

                Else
                    'If Not conn.State = ConnectionState.Open Then
                    '    conn.Open()
                    'End If
                    'ExecuteNonQuery("LogAccess", conn, userName, Session("ApplicationName"), True)
                    Session("LoginAttempts") += 1
                    If Session("LoginAttempts") > GetMaxLoginAttempts() Then
                        LockUser(userName)
                        Response.Redirect("ErrorPage.aspx?t=2", False)
                    End If
                    Me.LabelError.Text = "Login Failed. Please try again."

                End If

            End With

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                Try
                    ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx LoginClick()", sqlEx.ToString)
                Catch
                    Response.Redirect("ErrorPage.aspx", False)
                End Try

                Response.Redirect("ErrorPage.aspx", False)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            Try
                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx LoginClick()", ex.ToString)
            Catch
                Response.Redirect("ErrorPage.aspx", False)
            End Try

            Response.Redirect("ErrorPage.aspx", False)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

    End Sub

    Private Sub LockUser(ByVal userName As String)
        Dim conn As New SqlConnection(Session("ConnectionString").ToString)
        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            Dim qStr As String = "Update Users Set UserIsLockedOut = '1' Where UserName = '" & userName & "'"
            ExecuteNonQuery(qStr, conn)

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx LockUser()", sqlEx.ToString)

                Response.Redirect("ErrorPage.aspx", True)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx LockUser()", ex.ToString)

            Response.Redirect("ErrorPage.aspx", True)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try
    End Sub

    Private Function GetMaxLoginAttempts() As Integer
        'Get maxumum number of login attempts allowed.
        Dim conn As New SqlConnection(Session("ConnectionString").ToString)
        Dim maxLoginAttempts As Integer = 0
        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            maxLoginAttempts = ExecuteScalar("Select SettingValue From Settings Where SettingName = 'Security.MaxLoginAttempts'", conn)

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx GettinMaxLoginAttempts()", sqlEx.ToString)

                Response.Redirect("ErrorPage.aspx", False)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "Login.aspx GettinMaxLoginAttempts()", ex.ToString)

            Response.Redirect("ErrorPage.aspx", False)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

        Return maxLoginAttempts

    End Function

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
