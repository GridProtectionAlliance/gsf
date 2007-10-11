Imports System.Data
Imports System.Data.SqlClient
Imports TVA.Data.Common
Imports TVA.Security.Application

Partial Class ChangePassword
    Inherits System.Web.UI.Page

    Private conn As New SqlConnection

    Protected Sub ButtonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSubmit.Click
        Dim userName As String = Me.TextBoxUserName.Text.Replace("'", "").Replace("%", "")
        Dim oldPassword As String = Me.TextBoxPassword.Text.Replace("'", "").Replace("%", "")
        Dim newPassword As String = Me.TextBoxNewPassword.Text.Replace("'", "").Replace("%", "")
        Dim errorPageUrl As String = "ErrorPage.aspx"
        If Page.IsValid Then
            Try
                If Session("ConnectionString") Is Nothing Then
                    Response.Redirect("ErrorPage.aspx?t=1", True)
                End If

                If Session("ApplicationName") Is Nothing Then
                    Response.Redirect("ErrorPage.aspx?t=1", True)
                End If
                conn = New SqlConnection(Session("ConnectionString"))

                With New User(userName, oldPassword, New SqlConnection(Session("ConnectionString")))
                    If .IsExternal AndAlso .IsAuthenticated Then
                        If Not conn.State = ConnectionState.Open Then
                            conn.Open()
                        End If
                        Try
                            'EncryptPassword will throw an exception if password does not meet 
                            'the strong password criteria. This is placed in try catch just to 
                            'let user know about the strong password rules.
                            TVA.Security.Application.User.EncryptPassword(newPassword)
                        Catch ex As Exception
                            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ChangePassword.aspx IncorrectPasswordFormat", ex.ToString)
                            errorPageUrl = "ErrorPage.aspx?t=3"
                        End Try

                        ExecuteNonQuery("ChangePassword", conn, userName, TVA.Security.Application.User.EncryptPassword(oldPassword), TVA.Security.Application.User.EncryptPassword(newPassword))

                        Me.LabelError.Text = "Your password has been changed successfully. Please <a href=Login.aspx>login</a> with your new password."

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

                    ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ChangePassword.aspx Submit()", sqlEx.ToString)

                    Response.Redirect(errorPageUrl, False)
                End If

            Catch ex As Exception
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ChangePassword.aspx Submit()", ex.ToString)

                Response.Redirect(errorPageUrl, False)

            Finally
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If

            End Try

        Else
            Response.Redirect(errorPageUrl, False)

        End If

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.TextBoxUserName.Focus()
        SetSessions()
        If Request("m") IsNot Nothing AndAlso _
                Not String.IsNullOrEmpty(Request("m").ToString()) Then

            Select Case CType(Request("m").ToString(), MessageTypes)

                Case MessageTypes.PasswordExpiredOrReset
                    Me.LabelError.Text = "Your password has expired or reset. Please change your password."

                Case Else
                    Me.LabelError.Text = ""

            End Select

        End If
    End Sub

    Private Enum MessageTypes As Integer
        PasswordExpiredOrReset

    End Enum

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
