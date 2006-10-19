Imports System.Data
Imports System.Data.SqlClient
Imports Tva.Data.Common
Imports Tva.Security.Application

Partial Class ApplyForAccount
    Inherits System.Web.UI.Page

    Private conn As New SqlConnection
    Private qStr As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '***********
        'Session("ConnectionString") = "Server=RGOCDSQL; Database=ApplicationSecurity; UID=appsec; PWD=123-xyz"
        '***********
        '***********
        'Session("ApplicationName") = "TROAPPSEC"
        '***********

        If Session("ConnectionString") Is Nothing Then
            Response.Redirect("ErrorPage.aspx?t=1", True)
        Else
            conn = New SqlConnection(Session("ConnectionString"))
        End If

        If Session("ApplicationName") Is Nothing Then
            Response.Redirect("ErrorPage.aspx", False)
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

            'qStr = "Select CompanyID, CompanyName From Companies Order By CompanyName"
            'With Me.DropDownListCompanies
            '    .DataSource = RetrieveData(qStr, conn)
            '    .DataTextField = "CompanyName"
            '    .DataValueField = "CompanyID"
            '    .DataBind()
            '    .Items.Insert(.Items.Count, "Other")
            '    .SelectedIndex = 0
            'End With

            qStr = "Select SecurityQuestionID, SecurityQuestionText From SecurityQuestions Order By SecurityQuestionText"
            With Me.DropDownListSecurityQuestions
                .DataSource = RetrieveData(qStr, conn)
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
                
                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx PopulateDropDowns()", sqlEx.ToString)
                Response.Redirect("ErrorPage.aspx", False)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If
            
            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx PopulateDropDowns()", ex.ToString)
            Response.Redirect("ErrorPage.aspx", False)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try
    End Sub

    Protected Sub ButtonSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ButtonSubmit.Click
        If Page.IsValid Then

            Try
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If

                Dim userName As String = Me.TextBoxUserName.Text.Replace("'", "").Replace("%", "")
                Dim password As String = Tva.Security.Application.User.EncryptPassword(Me.TextBoxPassword.Text.Replace("'", "").Replace("%", ""))
                Dim securityQuestion As String = Me.DropDownListSecurityQuestions.SelectedItem.ToString
                Dim securityAnswer As String = Me.TextBoxSecurityAnswer.Text.Replace("'", "''")
                Dim firstName As String = Me.TextBoxFirstName.Text.Replace("'", "").Replace("%", "")
                Dim lastName As String = Me.TextBoxLastName.Text.Replace("'", "").Replace("%", "")
                Dim phone As String = Me.TextBoxPhone.Text.Replace("-", "").Replace("(", "").Replace(")", "")
                Dim email As String = Me.TextBoxEmail.Text.Replace("'", "").Replace("%", "")

                Dim company As String = Me.TextBoxCompanyName.Text.Replace("'", "").Replace("%", "")
                'If Me.DropDownListCompanies.SelectedItem.ToString = "Other" Then
                '    company = 
                'Else
                '    company = Me.DropDownListCompanies.SelectedItem.ToString
                'End If

                If company = "" Then
                    Me.LabelError.Text &= "Please enter company name or select company from the drop down. "
                    Exit Try
                End If

                Dim applicationName As String = String.Empty
                If Session("ApplicationName") IsNot Nothing Then
                    applicationName = Session("ApplicationName")
                    If applicationName = "" Then
                        Response.Redirect("ErrorPage.aspx", False)
                    End If
                Else
                    Response.Redirect("ErrorPage.aspx", False)
                End If

                With New User(userName, New SqlConnection(Session("ConnectionString")))

                    If .Exists Then
                        Me.LabelError.Text = "An account with this user name already exists in the system. If you are a new user please select a different user name. If you are a returning user please go to <a href=RequestAccess.aspx>Request Application Access</a> page."

                    ElseIf RequestIsPending(userName) Then
                        Me.LabelError.Text = "An account request is pending with this user name."

                    Else
                        If Not conn.State = ConnectionState.Open Then
                            conn.Open()
                        End If
                        ExecuteNonQuery("SubmitAccountRequest", conn, userName, password, firstName, lastName, company, phone, email, securityQuestion, securityAnswer, applicationName, True)

                        ClearForm()

                        Me.LabelError.Text = "Your account request has been received. You will receive confirmation email shortly."
                    End If

                End With

            Catch sqlEx As SqlException
                'Log into the error log.
                If sqlEx.Number = 18456 Then
                    Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
                Else
                    If Not conn.State = ConnectionState.Open Then
                        conn.Open()
                    End If
                    
                    ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx Submit()", sqlEx.ToString)

                    Response.Redirect("ErrorPage.aspx", False)
                End If
            Catch ex As Exception
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If
                
                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx Submit()", ex.ToString)

                Response.Redirect("ErrorPage.aspx", False)

            Finally
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If

            End Try
        Else
            Me.LabelError.Text &= "Please enter all the required fields.<br>"
        End If

    End Sub

    'Protected Sub DropDownListCompanies_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles DropDownListCompanies.SelectedIndexChanged
    '    If Me.DropDownListCompanies.SelectedItem.ToString = "Other" Then
    '        Me.LabelCompany.Visible = True
    '        Me.TextBoxCompanyName.Visible = True
    '    Else
    '        Me.LabelCompany.Visible = False
    '        Me.TextBoxCompanyName.Visible = False
    '    End If
    'End Sub

    Private Sub ClearForm()
        Me.TextBoxCompanyName.Text = ""
        Me.TextBoxEmail.Text = ""
        Me.TextBoxFirstName.Text = ""
        Me.TextBoxLastName.Text = ""
        Me.TextBoxPassword.Text = ""
        Me.TextBoxPhone.Text = ""
        Me.TextBoxSecurityAnswer.Text = ""
        Me.TextBoxUserName.Text = ""
        'Me.DropDownListCompanies.SelectedIndex = 0
        Me.DropDownListSecurityQuestions.SelectedIndex = 0
    End Sub

    Private Function RequestIsPending(ByVal userName As String) As Boolean

        Dim isPending As Boolean = False

        Try
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            qStr = "Select count(*) From AccountRequests Where UserName = '" & userName & "'"
            isPending = ExecuteScalar(qStr, conn)

        Catch sqlEx As SqlException
            If sqlEx.Number = 18456 Then
                Me.LabelError.Text = "Error occured while connecting to the database. We will not be able to process your request. Please try again later."
            Else
                'Log into the error log.
                If Not conn.State = ConnectionState.Open Then
                    conn.Open()
                End If
                
                ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx RequestIsPending()", sqlEx.ToString)
                Response.Redirect("ErrorPage.aspx", True)
            End If

        Catch ex As Exception
            'Log into the error log.
            If Not conn.State = ConnectionState.Open Then
                conn.Open()
            End If

            ExecuteNonQuery("LogError", conn, Session("ApplicationName"), "ApplyForAccount.aspx RequestIsPending()", ex.ToString)
            Response.Redirect("ErrorPage.aspx", True)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If

        End Try

        Return isPending
    End Function
End Class
