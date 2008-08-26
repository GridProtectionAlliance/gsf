' 05/05/2008

Imports System.ComponentModel
Imports TVA.Security.Application

Namespace UI

    Public Class SecureMasterPage
        Inherits System.Web.UI.MasterPage

#Region " Member Declaration "

        Private WithEvents m_securityProvider As WebSecurityProvider

#End Region

#Region " Event Declaration "

        ''' <summary>
        ''' Occurs when the login process is complete and  the current user has access to the application.
        ''' </summary>
        <Description("Occurs when the login process is complete and  the current user has access to the application."), Category("Security")> _
        Public Event LoginSuccessful As EventHandler

        ''' <summary>
        ''' Occurs when the login process is complete and the current user does not have access to the application.
        ''' </summary>
        <Description("Occurs when the login process is complete and the current user does not have access to the application."), Category("Security")> _
        Public Event LoginUnsuccessful As EventHandler

#End Region

#Region " Code Scope: Public Code "

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecureMasterPage class.
        ''' </summary>
        Public Sub New()

            MyClass.New("")

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecureMasterPage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        Public Sub New(ByVal applicationName As String)

            MyClass.New(applicationName, SecurityServer.Development)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecureMasterPage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer)

            MyClass.New(applicationName, securityServer, AuthenticationMode.AD)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecureMasterPage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
        ''' <param name="authenticationMode">One of the TVA.Security.Application.AuthenticationMode values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer, ByVal authenticationMode As AuthenticationMode)

            MyBase.New()
            m_securityProvider = New WebSecurityProvider()
            m_securityProvider.PersistSettings = True
            m_securityProvider.ApplicationName = applicationName
            m_securityProvider.Server = securityServer
            m_securityProvider.AuthenticationMode = authenticationMode

        End Sub

        ''' <summary>
        ''' Gets the TVA.Security.Application.WebSecurityProvider component that handles the security.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The TVA.Security.Application.WebSecurityProvider component.</returns>
        <Browsable(False)> _
        Public ReadOnly Property SecurityProvider() As WebSecurityProvider
            Get
                Return m_securityProvider
            End Get
        End Property

#End Region

#Region " Code Scope: Protected Code "

        ''' <summary>
        ''' Raises the TVA.Web.UI.SecureMasterPage.LoginSuccessful event.
        ''' </summary>
        ''' <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
        ''' <remarks>
        ''' This method is to be called when the login process is complete and  the current user has access to the 
        ''' application.
        ''' </remarks>
        Protected Sub OnLoginSuccessful(ByVal e As CancelEventArgs)

            RaiseEvent LoginSuccessful(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the TVA.Web.UI.SecureMasterPage.LoginUnsuccessful event.
        ''' </summary>
        ''' <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
        ''' <remarks>
        ''' This method is to be called when the login process is complete and the current user does not have 
        ''' access to the application.
        ''' </remarks>
        Protected Sub OnLoginUnsuccessful(ByVal e As CancelEventArgs)

            RaiseEvent LoginUnsuccessful(Me, e)

        End Sub

#End Region

#Region " Code Scope: Private Code "

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

            ' This is the earliest stage in the masterpage life-cycle we can engage the security.
            m_securityProvider.Parent = Me.Page
            m_securityProvider.LoginUser()

        End Sub

        Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload

            ' We're done with the security control so we'll set the member variable to Nothing. This will cause 
            ' all the event handlers to the security control events to be removed. If we don't do this then the 
            ' the security control will have reference to this control's page via the event handlers and since 
            ' it is cached, the page will also be cached - which we don't want to happen.
            m_securityProvider = Nothing

        End Sub

        Private Sub m_securityProvider_BeforeLoginPrompt(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.BeforeLoginPrompt

            ' If this secure masterpage is master of a secure page, we'll let the secure page handle login prompt.
            If TypeOf Me.Page Is SecurePage Then e.Cancel = True

            ' NOTE TO SELF: If an implementer wants to use a secure masterpage along with an unsecure page, they'll 
            ' have to cancel the login process through an advanced implementation of security at the control level.

        End Sub

        Private Sub m_securityProvider_AccessDenied(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessDenied

            ' If this secure masterpage is master of a secure page, we'll let the secure page handle access denial.
            If TypeOf Me.Page Is SecurePage Then e.Cancel = True

            OnLoginUnsuccessful(e)

        End Sub

        Private Sub m_securityProvider_AccessGranted(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessGranted

            OnLoginSuccessful(e)

        End Sub

#End Region

    End Class

End Namespace