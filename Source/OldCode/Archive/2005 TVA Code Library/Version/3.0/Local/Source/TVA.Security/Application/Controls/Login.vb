'*******************************************************************************************************
'  TVA.Security.Application.Controls.Login.vb - Control for logging in to a web site
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/22/2008 - Pinal C. Patel
'       Original version of source code generated.
'
'*******************************************************************************************************

Imports System.Text
Imports System.Web.UI.WebControls
Imports TVA.Security.Cryptography.Common

Namespace Application.Controls

    ''' <summary>
    ''' Provides user interface (UI) for logging in to a secure Web Site.
    ''' </summary>
    Public Class Login
        Inherits CompositeControl

#Region " Member Declaration "

        Private m_usernameTextBox As TextBox
        Private m_passwordTextBox As TextBox
        Private m_container As ControlContainer
        Private m_securityProvider As WebSecurityProvider

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Session key used to cross-check whether or not a user account is in the "new pin" mode before a user
        ''' is allowed to create a new pin under RSA authentication mode.
        ''' </summary>
        Public Const NewPinVerify As String = "NewPinMode"

        ''' <summary>
        ''' Creates an instance of the login control.
        ''' </summary>
        ''' <param name="container">Control containing this control.</param>
        ''' <param name="securityProvider">Current security control.</param>
        Public Sub New(ByVal container As ControlContainer, ByVal securityProvider As WebSecurityProvider)

            MyBase.New()
            m_container = container
            m_securityProvider = securityProvider

        End Sub

        ''' <summary>
        ''' Gets or sets the container control for this control.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Container control for this control.</returns>
        Public Property Container() As ControlContainer
            Get
                Return m_container
            End Get
            Set(ByVal value As ControlContainer)
                If value IsNot Nothing Then
                    m_container = value
                Else
                    Throw New ArgumentNullException("Container")
                End If
            End Set
        End Property

#End Region

#Region " Code Scope: Protected "

        ''' <summary>
        ''' Performs layout of the control.
        ''' </summary>
        Protected Overrides Sub CreateChildControls()

            ' -----------------------------------------
            ' |              | ---------------------- |
            ' | Username:*   | |                    | |
            ' |              | ---------------------- |
            ' -----------------------------------------
            ' |              | ---------------------- |
            ' | Password:*   | |                    | |
            ' |              | ---------------------- |
            ' -----------------------------------------
            ' |              |         -------------- |
            ' |              |         |   Submit   | |
            ' |              |         -------------- |
            ' -----------------------------------------

            ' Layout the control.
            Dim container As Table = ControlContainer.NewTable(3, 2)

            ' Row #1
            m_usernameTextBox = New TextBox()
            m_usernameTextBox.ID = "UsernameTextBox"
            m_usernameTextBox.Width = Unit.Parse("150px")
            Dim usernameValidator As New RequiredFieldValidator()
            usernameValidator.Display = ValidatorDisplay.None
            usernameValidator.ErrorMessage = "Username is required."
            usernameValidator.ControlToValidate = m_usernameTextBox.ID
            container.Rows(0).Cells(0).Text = "Username:*&nbsp;"
            container.Rows(0).Cells(0).HorizontalAlign = HorizontalAlign.Right
            container.Rows(0).Cells(1).Controls.Add(m_usernameTextBox)
            container.Rows(0).Cells(1).Controls.Add(usernameValidator)

            ' Row #2
            m_passwordTextBox = New TextBox()
            m_passwordTextBox.ID = "PasswordTextBox"
            m_passwordTextBox.Width = Unit.Parse("150px")
            m_passwordTextBox.TextMode = TextBoxMode.Password
            Dim passwordValidator As New RequiredFieldValidator()
            passwordValidator.Display = ValidatorDisplay.None
            passwordValidator.ErrorMessage = "Password is required."
            passwordValidator.ControlToValidate = m_passwordTextBox.ID
            container.Rows(1).Cells(0).Text = "Password:*&nbsp;"
            container.Rows(1).Cells(0).HorizontalAlign = HorizontalAlign.Right
            container.Rows(1).Cells(1).Controls.Add(m_passwordTextBox)
            container.Rows(1).Cells(1).Controls.Add(passwordValidator)

            ' Row #3
            Dim submitButton As New Button()
            submitButton.Text = "Submit"
            AddHandler submitButton.Click, AddressOf SubmitButton_Click
            Dim validationSummary As New ValidationSummary()
            validationSummary.ShowSummary = False
            validationSummary.ShowMessageBox = True
            container.Rows(2).Cells(0).Controls.Add(validationSummary)
            container.Rows(2).Cells(1).HorizontalAlign = HorizontalAlign.Right
            container.Rows(2).Cells(1).Controls.Add(submitButton)

            Me.Controls.Clear()
            Me.Controls.Add(container)

            ' Setup client-side scripts.
            Page.SetFocus(m_usernameTextBox)
            With New StringBuilder()
                .Append("if (typeof(Page_ClientValidate) == 'function') {")
                .Append("if (Page_ClientValidate() == false) { return false; }}")
                .Append("this.disabled = true;")
                .AppendFormat("document.all.{0}.disabled = true;", submitButton.ClientID)
                .AppendFormat("{0};", Page.ClientScript.GetPostBackEventReference(submitButton, Nothing))

                submitButton.OnClientClick = .ToString()
            End With

            If m_securityProvider.AuthenticationMode = AuthenticationMode.RSA Then
                ' If RSA authentication is used, we'll provided a hint about the username and password, as there 
                ' may be some confussion as to what makes-up the password when a web page is secured using RSA.
                With New StringBuilder()
                    .Append("Note: This web page is secured using RSA Security. The Username is the ID that was ")
                    .Append("provided to you when you received your RSA SecurID key, and the Password consists ")
                    .Append("of your pin followed by the token currently being displayed on your RSA SecurID key.")

                    m_container.UpdateMessageText(.ToString(), MessageType.Information)
                End With
            End If

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub SubmitButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim user As User = Nothing
            Try
                If m_securityProvider IsNot Nothing Then
                    user = New User(m_usernameTextBox.Text, m_passwordTextBox.Text, _
                                    m_securityProvider.ApplicationName, m_securityProvider.Server, _
                                    m_securityProvider.AuthenticationMode, True)

                    If user.PasswordChangeDateTime > Date.MinValue AndAlso _
                            user.PasswordChangeDateTime <= Date.Now Then
                        ' It's time for the user to change password/create pin.
                        If m_securityProvider.AuthenticationMode = AuthenticationMode.RSA Then
                            ' Under RSA authentication, we don't allow a new pin to be created on-demand as it is
                            ' not supported. In other words, user cannot navigate to the Change Password control by
                            ' clicking on the link directly, instead the user must redirected from here after ensuring
                            ' that the user account is actually in the "new pin" mode (done by Authenticate()).
                            Page.Session(NewPinVerify) = True
                        End If

                        m_container.UpdateActiveControl("Change Password")
                        Exit Sub
                    End If

                    If user.IsAuthenticated Then
                        ' User's credentials have been verified, so we'll save them for use by the security control.
                        ' We'll save both the username and password in 2 places:
                        ' 1) Session for use by security control.
                        ' 2) Cookie for single-signon, but not when RSA security is employed.
                        Dim username As String = Encrypt(m_usernameTextBox.Text)
                        Dim password As String = Encrypt(m_passwordTextBox.Text)
                        Page.Session.Add(WebSecurityProvider.UsernameKey, username)
                        Page.Session.Add(WebSecurityProvider.PasswordKey, password)
                        If m_securityProvider.AuthenticationMode <> AuthenticationMode.RSA Then
                            Page.Response.Cookies(WebSecurityProvider.CredentialCookie)(WebSecurityProvider.UsernameKey) = username
                            Page.Response.Cookies(WebSecurityProvider.CredentialCookie)(WebSecurityProvider.PasswordKey) = password
                        End If

                        m_container.Redirect(String.Empty)  ' Refresh.
                    Else
                        If Not user.IsDefined Then
                            ' Account doesn't exist for user.
                            Page.SetFocus(m_usernameTextBox)
                            m_container.UpdateMessageText("Login failed. No such account.", MessageType.Error)
                        ElseIf user.IsLockedOut Then
                            ' User's account has been locked-out.
                            Page.SetFocus(m_usernameTextBox)
                            m_container.UpdateMessageText("Login failed. Account is locked.", MessageType.Error)
                        Else
                            ' Failed to verify the credentials.
                            Page.SetFocus(m_passwordTextBox)
                            m_container.UpdateMessageText("Login failed. Credential verification was unsuccessful.", MessageType.Error)
                        End If
                    End If
                End If
            Catch ex As Exception
                ' Show the encountered exception to the user.
                m_container.UpdateMessageText(String.Format("ERROR: {0}", ex.Message), MessageType.Error)
                ' Log encountered exception to security database if possible.
                If user IsNot Nothing Then user.LogError("API -> Login Control -> Submit", ex.ToString())
            End Try

        End Sub

#End Region

    End Class

End Namespace