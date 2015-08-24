Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Principal
Imports System.DirectoryServices
Imports TVA.Data.Common
Imports TVA.Security.Application

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

        SetSessions()
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
            'PopulateForm()     'leave it for future use.

            If System.Threading.Thread.CurrentPrincipal.Identity.Name IsNot Nothing Then    'if internal user
                ButtonSubmit.Enabled = False
                Me.LabelError.Text = "This page is for external users only. TVA internal users should contact Cynthia Hill-Watson at 423.751.6747."
            Else
                ButtonSubmit.Enabled = True
                Me.LabelError.Text = ""
            End If

        End If

    End Sub

    Private Sub PopulateForm()
        'If user is internal and his NTID is available then get and populate his information from the active directory.

        If System.Threading.Thread.CurrentPrincipal.Identity.Name IsNot Nothing Then

            Dim ntid As String = System.Threading.Thread.CurrentPrincipal.Identity.Name
            If ntid.Contains("TVA\") Then
                ntid = ntid.Replace("TVA\", "")
            End If

            Dim entry As DirectoryEntry = New DirectoryEntry("LDAP://TVA:389/dc=main,dc=tva,dc=gov", "esocss", "pwd4ctrl")
            Dim filterStr As String = ntid
            Dim mySearcher As New System.DirectoryServices.DirectorySearcher
            mySearcher.SearchRoot = entry
            mySearcher.SearchScope = SearchScope.Subtree
            mySearcher.Filter = String.Format("(&(objectCategory=person)(objectclass=user)(samaccountName={0}))", filterStr)
            Dim resEnt As SearchResult = mySearcher.FindOne

            If resEnt IsNot Nothing Then
                With resEnt.GetDirectoryEntry
                    Me.TextBoxUserName.Text = ntid
                    Me.TextBoxFirstName.Text = DirectCast(.Properties("givenname").Value, String)
                    Me.TextBoxLastName.Text = DirectCast(.Properties("sn").Value, String)
                    Me.TextBoxEmail.Text = DirectCast(.Properties("mail").Value, String)
                    Me.TextBoxPhone.Text = DirectCast(.Properties("telephoneNumber").Value, String)
                    Me.TextBoxCompanyName.Text = "Tennessee Valley Authority"
                End With

                Me.TextBoxCompanyName.Enabled = False
                Me.TextBoxEmail.Enabled = False
                Me.TextBoxFirstName.Enabled = False
                Me.TextBoxLastName.Enabled = False
                Me.TextBoxPassword.Enabled = False
                Me.TextBoxPhone.Enabled = False
                Me.TextBoxSecurityAnswer.Enabled = False
                Me.TextBoxUserName.Enabled = False
                Me.DropDownListSecurityQuestions.Enabled = False
            End If

        Else
            Me.TextBoxCompanyName.Enabled = True
            Me.TextBoxEmail.Enabled = True
            Me.TextBoxFirstName.Enabled = True
            Me.TextBoxLastName.Enabled = True
            Me.TextBoxPassword.Enabled = True
            Me.TextBoxPhone.Enabled = True
            Me.TextBoxSecurityAnswer.Enabled = True
            Me.TextBoxUserName.Enabled = True
            Me.DropDownListSecurityQuestions.Enabled = True

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
                Dim password As String = TVA.Security.Application.User.EncryptPassword(Me.TextBoxPassword.Text.Replace("'", "").Replace("%", "").Replace(" ", ""))
                Dim securityQuestion As String = Me.DropDownListSecurityQuestions.SelectedItem.ToString
                Dim securityAnswer As String = Me.TextBoxSecurityAnswer.Text.Replace("'", "''")
                Dim firstName As String = Me.TextBoxFirstName.Text.Replace("'", "").Replace("%", "")
                Dim lastName As String = Me.TextBoxLastName.Text.Replace("'", "").Replace("%", "")
                Dim phone As String = Me.TextBoxPhone.Text.Replace("-", "").Replace("(", "").Replace(")", "")
                Dim email As String = Me.TextBoxEmail.Text.Replace("'", "").Replace("%", "")

                If System.Threading.Thread.CurrentPrincipal.Identity.Name Is Nothing Then
                    If password = "" Then
                        Me.LabelError.Text &= "Please enter password. "
                        Exit Try
                    End If

                    If securityAnswer = "" Then
                        Me.LabelError.Text &= "Please enter security answer. "
                        Exit Try
                    End If
                End If

                Dim company As String = Me.TextBoxCompanyName.Text.Replace("'", "").Replace("%", "")
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
