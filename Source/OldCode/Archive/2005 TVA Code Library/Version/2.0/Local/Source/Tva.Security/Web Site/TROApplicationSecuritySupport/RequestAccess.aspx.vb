Imports Tva.Security.Application
Imports System.Data
Imports System.Data.SqlClient
Imports Tva.Data.Common

Partial Class RequestAccess
    Inherits System.Web.UI.Page

    Private conn As New SqlConnection

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.TextBoxUserName.Focus()
        SetSessions()
        If Not IsPostBack Then
            Me.LabelHeading.Text = "This page is for authorized users of TVA web-based services. If you do not have an account, please apply for one by clicking <a href=ApplyForAccount.aspx>here</a>.<BR><BR>Please enter your ID and Password. "
        End If
    End Sub

    Protected Sub ButtonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSubmit.Click

        Try

            Dim userName As String = Me.TextBoxUserName.Text.Replace("'", "").Replace("%", "")
            Dim password As String = Me.TextBoxPassword.Text.Replace("'", "").Replace("%", "")
            Dim applicationName As String = String.Empty

            '***********
            'Session("ApplicationName") = "RTP_WEB"
            '***********

            If Session("ApplicationName") IsNot Nothing Then
                applicationName = Session("ApplicationName")
                If applicationName = "" Then
                    Response.Redirect("ErrorPage.aspx?t=1", True)
                End If
            Else
                Response.Redirect("ErrorPage.aspx?t=1", True)
            End If

            '***********
            'Session("ConnectionString") = "Server=RGOCDSQL; Database=ApplicationSecurity; UID=appsec; PWD=123-xyz"
            '***********

            If Session("ConnectionString") Is Nothing Then
                Response.Redirect("ErrorPage.aspx?t=1", True)
            End If

            conn = New SqlConnection(Session("ConnectionString"))
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            With New User(userName, password, New SqlConnection(Session("ConnectionString")))
                If .IsAuthenticated Then

                    If RequestIsPending(userName, applicationName) Then
                        Me.LabelError.Text = "This request is already pending in the system for approval."
                    Else
                        If Not conn.State = ConnectionState.Open Then
                            conn.Open()
                        End If
                        ExecuteNonQuery("SubmitAccessRequest", conn, userName, applicationName)
                        Me.LabelError.Text = "Your request for an application access has been received. You will receive confirmation email shortly."
                    End If
                    
                Else
                    Me.LabelError.Text = "System did not find any match for your user name and password. Please try again.<br><br> If you do not have an account, please apply for an account by clicking Apply For Account link below."
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

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "RequestAccess.aspx Submit()", sqlEx.ToString)

                Response.Redirect("ErrorPage.aspx", True)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "RequestAccess.aspx Submit()", ex.ToString)

            Response.Redirect("ErrorPage.aspx", True)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

    End Sub

    Private Function RequestIsPending(ByVal userName As String, ByVal applicationName As String) As Boolean
        Dim isPending As Boolean = False

        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            Dim qStr As String = "Select userID From Users Where UserName = '" & userName & "'"
            Dim userId As Guid = ExecuteScalar(qStr, conn)

            qStr = "Select ApplicationID From Applications Where ApplicationName = '" & applicationName & "'"
            Dim applicationId As Guid = ExecuteScalar(qStr, conn)

            qStr = "Select count(*) from AccessRequests Where UserID = '" & Convert.ToString(userId) & "' AND applicationId = '" & Convert.ToString(applicationId) & "'"
            isPending = ExecuteScalar(qStr, conn)

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "RequestAccess.aspx RequestIsPending()", sqlEx.ToString)

                Response.Redirect("ErrorPage.aspx", True)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "RequestAccess.aspx RequestIsPending()", ex.ToString)

            Response.Redirect("ErrorPage.aspx", True)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

        Return isPending
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
