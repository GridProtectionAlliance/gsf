Imports System.Data
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports TVA.Security.Application

Partial Class ResetPassword
    Inherits System.Web.UI.Page

    Private conn As New SqlConnection

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        '***********
        'Session("ConnectionString") = "Server=RGOCDSQL; Database=ApplicationSecurity; UID=appsec; PWD=123-xyz"
        '***********
        SetSessions()
        If Session("ConnectionString") Is Nothing Then
            Response.Redirect("ErrorPage.aspx", True)
        Else
            conn = New SqlConnection(Session("ConnectionString"))
        End If

        Me.TextBoxUserName.Focus()
        If Not IsPostBack Then
            PopulateDropDowns()
        End If

    End Sub

    Private Sub PopulateDropDowns()
        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If
            With Me.DropDownListSecurityQuestions
                .DataSource = RetrieveData("Select * From SecurityQuestions Order By SecurityQuestionText", conn)
                .DataTextField = "SecurityQuestionText"
                .DataValueField = "SecurityQuestionID"
                .DataBind()
            End With

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ResetPassword.aspx PopulateDropDowns()", sqlEx.ToString)

                Response.Redirect("ErrorPage.aspx")
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ResetPassword.aspx PopulateDropDowns()", ex.ToString)
            Response.Redirect("ErrorPage.aspx")

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try
    End Sub

    Private Sub ClearForm()
        Me.TextBoxSecurityAnswer.Text = ""
        Me.TextBoxUserName.Text = ""
        Me.DropDownListSecurityQuestions.SelectedIndex = 0
    End Sub

    Protected Sub ButtonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSubmit.Click
        If Page.IsValid Then
            Dim userName As String = Me.TextBoxUserName.Text.Replace("'", "").Replace("%", "")
            Dim securityAnswer As String = Me.TextBoxSecurityAnswer.Text.Replace("'", "").Replace("%", "")
            Dim securityQuestion As String = Me.DropDownListSecurityQuestions.SelectedItem.ToString
            Try

                With New User(userName, New SqlConnection(Session("ConnectionString")))

                    If .IsExternal Then


                        If Not conn.State = ConnectionState.Open Then
                            conn.Open()
                        End If

                        Dim qStr As String = "Select count(*) from UsersAndCompaniesAndSecurityQuestions Where UserName = '" & userName & "' AND UserSecurityQuestion = '" & securityQuestion & "' AND UserSecurityAnswer = '" & securityAnswer & "'"
                        Dim isValidUser As Boolean = ExecuteScalar(qStr, conn)

                        If isValidUser Then
                            Dim emailAddress As String = ExecuteScalar("Select IsNull(UserEmailAddress, '') As UserEmailAddress From Users Where UserName = '" & userName & "'", conn)

                            If Not emailAddress = "" Then
                                Dim newPassword As String = TVA.Security.Application.User.GeneratePassword(8)

                                Try
                                    'EncryptPassword will throw an exception if password does not meet 
                                    'the strong password criteria. This is placed in try catch just to 
                                    'let user know about the strong password rules.
                                    TVA.Security.Application.User.EncryptPassword(newPassword)
                                Catch ex As Exception
                                    ExecuteNonQuery("LogError", conn, Session("ApplicationName").ToString, "ResetPassword.aspx Submit()", ex.ToString)
                                    Response.Redirect("ErrorPage.aspx?t=3")
                                End Try

                                ExecuteNonQuery("ResetPassword", conn, userName, securityQuestion, securityAnswer, TVA.Security.Application.User.EncryptPassword(newPassword))

                                Me.LabelError.Text = "Your password has been reset. You will soon receive an email with your new password. You must change your password by clicking on the Change Password link below."

                                'Send an email to the user.
                                Dim emailBody As String = "<div style='font-family: Tahoma; font-size: .7em'>Dear " & .FirstName & ",<br /><br /> Your password has been reset. Below is your current account information.<BR><BR>" & _
                                                            "User Name: " & userName & "<BR>" & _
                                                            "New Password: " & newPassword & "<BR><br>" & _
                                                            "If you did not request this change, please contact the TVA Operations Duty Specialist at 423-751-1700." & _
                                                            GetMessageFooter() & "</div>"

                                Tva.Net.Smtp.Common.SendMail("troapplicationsecurity@tva.gov", emailAddress, "Your password has been reset.", emailBody, True, "mailhost.cha.tva.gov")
                            Else
                                Me.LabelError.Text = "Reset passowrd failed. System did not find an email address for your account where your new password can be sent to. Please contact the TVA Operations Duty Specialist at 423-751-1700 to update your account information and then try again."

                            End If

                        Else
                            Me.LabelError.Text = "Authentication failed for the set of information you have provided. Please make sure that the supplied information is correct."
                        End If


                    Else
                        Me.LabelError.Text = "This is a TVA internal user name. Internal users cannot change their password here."
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

                    ExecuteNonQuery("LogError", conn, Session("ApplicationName").ToString, "ResetPassword.aspx Submit()", sqlEx.ToString)

                    Response.Redirect("ErrorPage.aspx")
                End If

            Catch ex As Exception
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ResetPassword.aspx Submit()", ex.ToString)

                Response.Redirect("ErrorPage.aspx")

            Finally
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If

            End Try

        End If

    End Sub

    Private Function GetFirstName(ByVal userName As String) As String
        Dim firstName As String = String.Empty

        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            Dim qStr As String = "Select IsNull(UserFirstName, '') From Users Where UserName = '" & userName & "'"
            firstName = ExecuteScalar(qStr, conn)

        Catch ex As Exception
            'Do nothing. I do not want to break application for email footer.

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

        Return firstName
    End Function

    Private Function GetMessageFooter() As String
        Dim footerText As String = String.Empty

        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            Dim qStr As String = "Select IsNull(SettingValue, '') From Settings Where SettingName = 'EmailNotifications.MessageFooter'"
            footerText = ExecuteScalar(qStr, conn)

        Catch ex As Exception
            'Do nothing. I do not want to break application for email footer.
            footerText = "<br /><br />Regards,<br /><br />TVA, Transmission & Reliability Organization<br />1101 Market St.<br />Chattanooga, TN 37402-2801<br />Phone: 423-751-0011<br />"

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

        Return footerText
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
