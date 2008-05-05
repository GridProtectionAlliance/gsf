' PCP: 04/22/2008

Imports System.Text
Imports System.Web.UI.WebControls
Imports TVA.Security.Cryptography.Common

Namespace Application.Controls

    Public Class ChangePassword
        Inherits CompositeControl

#Region " Member Declaration "

        Private m_usernameTextBox As TextBox
        Private m_oldPasswordTextBox As TextBox
        Private m_newPasswordTextBox As TextBox
        Private m_confirmPasswordTextBox As TextBox
        Private m_container As ControlContainer
        Private m_securityProvider As WebSecurityProvider

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates an instance of the change password control.
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

            ' -----------------------------------------------
            ' |                    | ---------------------- |
            ' | Username:*         | |                    | |
            ' |                    | ---------------------- |
            ' -----------------------------------------------
            ' |                    | ---------------------- |
            ' | Old Password:*     | |                    | |
            ' |                    | ---------------------- |
            ' -----------------------------------------------
            ' |                    | ---------------------- |
            ' | New Password:*     | |                    | |
            ' |                    | ---------------------- |
            ' -----------------------------------------------
            ' |                    | ---------------------- |
            ' | Confirm Password:* | |                    | |
            ' |                    | ---------------------- |
            ' -----------------------------------------------
            ' |                    |         -------------- |
            ' |                    |         |   Submit   | |
            ' |                    |         -------------- |
            ' -----------------------------------------------

            ' Layout the control.
            Dim container As Table = ControlContainer.NewTable(5, 2)

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
            m_oldPasswordTextBox = New TextBox()
            m_oldPasswordTextBox.ID = "OldPasswordTextBox"
            m_oldPasswordTextBox.Width = Unit.Parse("150px")
            m_oldPasswordTextBox.TextMode = TextBoxMode.Password
            Dim oldPasswordValidator As New RequiredFieldValidator()
            oldPasswordValidator.Display = ValidatorDisplay.None
            oldPasswordValidator.ErrorMessage = "Old Password is required."
            oldPasswordValidator.ControlToValidate = m_oldPasswordTextBox.ID
            container.Rows(1).Cells(0).Text = "Old Password:*&nbsp;"
            container.Rows(1).Cells(0).HorizontalAlign = HorizontalAlign.Right
            container.Rows(1).Cells(1).Controls.Add(m_oldPasswordTextBox)
            container.Rows(1).Cells(1).Controls.Add(oldPasswordValidator)

            ' Row #3
            m_newPasswordTextBox = New TextBox()
            m_newPasswordTextBox.ID = "NewPasswordTextBox"
            m_newPasswordTextBox.Width = Unit.Parse("150px")
            m_newPasswordTextBox.TextMode = TextBoxMode.Password
            Dim newPasswordValidator As New RequiredFieldValidator()
            newPasswordValidator.Display = ValidatorDisplay.None
            newPasswordValidator.ErrorMessage = "New Password is required."
            newPasswordValidator.ControlToValidate = m_newPasswordTextBox.ID
            container.Rows(2).Cells(0).Text = "New Password:*&nbsp;"
            container.Rows(2).Cells(0).HorizontalAlign = HorizontalAlign.Right
            container.Rows(2).Cells(1).Controls.Add(m_newPasswordTextBox)
            container.Rows(2).Cells(1).Controls.Add(newPasswordValidator)

            ' Row #4
            m_confirmPasswordTextBox = New TextBox()
            m_confirmPasswordTextBox.ID = "ConfirmPasswordTextBox"
            m_confirmPasswordTextBox.Width = Unit.Parse("150px")
            m_confirmPasswordTextBox.TextMode = TextBoxMode.Password
            Dim confirmPasswordValidator As New RequiredFieldValidator()
            confirmPasswordValidator.Display = ValidatorDisplay.None
            confirmPasswordValidator.ErrorMessage = "Confirm Password is required."
            confirmPasswordValidator.ControlToValidate = m_confirmPasswordTextBox.ID
            container.Rows(3).Cells(0).Text = "Confirm Password:*&nbsp;"
            container.Rows(3).Cells(0).HorizontalAlign = HorizontalAlign.Right
            container.Rows(3).Cells(1).Controls.Add(m_confirmPasswordTextBox)
            container.Rows(3).Cells(1).Controls.Add(confirmPasswordValidator)

            ' Row #5
            Dim submitButton As New Button()
            submitButton.Text = "Submit"
            AddHandler submitButton.Click, AddressOf SubmitButton_Click
            Dim passwordCompareValidator As New CompareValidator()
            passwordCompareValidator.Display = ValidatorDisplay.None
            passwordCompareValidator.ErrorMessage = "New Password and Confirm Password must match."
            passwordCompareValidator.ControlToValidate = m_newPasswordTextBox.ID
            passwordCompareValidator.ControlToCompare = m_confirmPasswordTextBox.ID
            Dim validationSummary As New ValidationSummary()
            validationSummary.ShowSummary = False
            validationSummary.ShowMessageBox = True
            container.Rows(4).Cells(0).Controls.Add(passwordCompareValidator)
            container.Rows(4).Cells(0).Controls.Add(validationSummary)
            container.Rows(4).Cells(1).HorizontalAlign = HorizontalAlign.Right
            container.Rows(4).Cells(1).Controls.Add(submitButton)

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
                ' In RSA authentication mode the following substitution must take place:
                ' Old Password      -> Token
                ' New Password      -> New Pin
                ' Confirm Password  -> Confirm Pin
                oldPasswordValidator.ErrorMessage = "Token is required."
                newPasswordValidator.ErrorMessage = "New Pin is required."
                confirmPasswordValidator.ErrorMessage = "Confirm Pin is required."
                passwordCompareValidator.ErrorMessage = "New Pin and Confirm Pin must match."
                container.Rows(1).Cells(0).Text = "Token:*"
                container.Rows(2).Cells(0).Text = "New Pin:*"
                container.Rows(3).Cells(0).Text = "Confirm Pin:*"

                If Page.Session(Login.NewPinVerify) IsNot Nothing Then
                    ' It is verified that the user account is in "new pin" mode and user must create a new pin.
                    With New StringBuilder()
                        .Append("Note: You are required to create a new pin for your RSA SecurID key. The pin must ")
                        .Append("be a 4 to 8 character alpha-numeric string. Please wait for the token on your RSA ")
                        .Append("SecurID key to change before proceeding.")

                        m_container.UpdateMessageText(.ToString(), MessageType.Information)
                    End With
                Else
                    ' User clicked on the Change Password link, so cannot allow a new pin to be created.
                    Me.Enabled = False
                    With New StringBuilder()
                        .Append("This screen is only active as part of an automated process. To create a new pin, ")
                        .Append("you must call the Operations Duty Specialist at 423-751-1700.")

                        m_container.UpdateMessageText(.ToString(), MessageType.Error)
                    End With
                End If
            End If

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Sub SubmitButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)

            Dim user As User = Nothing
            Try
                If m_securityProvider IsNot Nothing Then
                    ' We instantiate a User object, but skip the authentication. Here's why:
                    ' - Under AD authentication mode, current password will be verified by ChangePassword().
                    ' - Under RSA authentication mode, we can use a token (i.e. old password substitution) only 
                    '   once and if we perform authentication, we will not be able to create a new pin using the 
                    '   same token again because the RSA server will reject our request to create a pin.
                    user = New User(m_usernameTextBox.Text, m_oldPasswordTextBox.Text, _
                                    m_securityProvider.ApplicationName, m_securityProvider.Server, _
                                    m_securityProvider.AuthenticationMode, False)

                    If Not user.IsDefined Then
                        ' Don't proceed, account doesn't exist for user.
                        m_container.UpdateMessageText("Operation aborted. No such account.", MessageType.Error)
                        Exit Sub
                    ElseIf user.IsLockedOut Then
                        ' Don't proceed, user's account has been locked.
                        m_container.UpdateMessageText("Operation aborted. Account is locked.", MessageType.Error)
                        Exit Sub
                    ElseIf m_securityProvider.AuthenticationMode = AuthenticationMode.AD Then
                        ' Under AD authentication, the following restriction apply:
                        If Not user.IsExternal Then
                            ' Don't proceed, only external user can change their password from here.
                            m_container.UpdateMessageText("Operation aborted. Internal users cannot perform this task.", MessageType.Error)
                            Exit Sub
                        Else
                            ' User's old and new password must be verified.
                            If user.Password <> user.EncryptPassword(m_oldPasswordTextBox.Text) Then
                                ' Don't proceed, user's failed to provide the correct current password.
                                m_container.UpdateMessageText("Operation aborted. Old password verification failed.", MessageType.Error)
                                Exit Sub
                            End If

                            Try
                                user.EncryptPassword(m_newPasswordTextBox.Text)
                            Catch ex As Exception
                                ' Don't proceed, user's new password doesn't meeet strong password requirements.
                                m_container.UpdateMessageText(ex.Message.Replace(Environment.NewLine, "<br />"), MessageType.Error)
                                Exit Sub
                            End Try
                        End If
                    End If

                    ' Go ahead and attempt to change the user password or create a new pin.
                    If user.ChangePassword(m_oldPasswordTextBox.Text, m_newPasswordTextBox.Text) Then
                        ' Inform user about the success.
                        Select Case m_securityProvider.AuthenticationMode
                            Case AuthenticationMode.AD
                                m_container.UpdateMessageText("Your password has been changed!<br />You can now use your new password to login.", MessageType.Information)
                            Case AuthenticationMode.RSA
                                Me.Enabled = False
                                Page.Session.Remove(Login.NewPinVerify)
                                m_container.UpdateMessageText("Your new pin has been created!<br />Please wait for the token to change before next login.", MessageType.Information)
                        End Select
                    Else
                        ' This is highly unlikely because we've performed all checks before we actually attempted 
                        ' to change the password or create a new pin.
                        m_container.UpdateMessageText("Operation failed. Reason unknown.", MessageType.Error)
                    End If
                End If
            Catch ex As Exception
                ' Show the encountered exception to the user.
                m_container.UpdateMessageText(String.Format("ERROR: {0}", ex.Message), MessageType.Error)
                ' Log encountered exception to security database if possible.
                If user IsNot Nothing Then user.LogError("API -> Change Password Control -> Submit", ex.ToString())
            End Try

        End Sub

#End Region

    End Class

End Namespace