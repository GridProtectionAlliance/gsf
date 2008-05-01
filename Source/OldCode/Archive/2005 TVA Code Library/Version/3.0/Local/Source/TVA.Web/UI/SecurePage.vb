' 12/14/2006

Imports System.ComponentModel
Imports TVA.Security.Application

Namespace UI

    Public Class SecurePage
        Inherits System.Web.UI.Page

#Region " Member Declaration "

        Private m_applicationName As String
        Private m_securityServer As SecurityServer
        Private m_authenticationMode As AuthenticationMode

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
        ''' Initializes a new instance of TVA.Web.UI.SecurePage class.
        ''' </summary>
        Public Sub New()

            MyClass.New("")

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        Public Sub New(ByVal applicationName As String)

            MyClass.New(applicationName, SecurityServer.Development)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer)

            MyClass.New(applicationName, securityServer, AuthenticationMode.AD)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
        ''' <param name="authenticationMode">One of the TVA.Security.Application.AuthenticationMode values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer, ByVal authenticationMode As AuthenticationMode)

            MyBase.New()
            m_applicationName = applicationName
            m_securityServer = securityServer
            m_authenticationMode = authenticationMode

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
        ''' Raises the TVA.Web.UI.SecureUserControl.LoginSuccessful event.
        ''' </summary>
        ''' <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
        ''' <remarks>
        ''' This method is to be called when the login process is complete and  the current user has access to the 
        ''' application.
        ''' </remarks>
        Public Sub OnLoginSuccessful(ByVal e As CancelEventArgs)

            ' Upon successful login, we cache the security control for performance. Performing the caching 
            ' over here will guarantee that the security control gets cached regardless of weather or not the 
            ' implementer cancels the login process after login has been performed successfully. 
            WebSecurityProvider.SaveToCache(Me, m_securityProvider)

            RaiseEvent LoginSuccessful(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the TVA.Web.UI.SecureUserControl.LoginUnsuccessful event.
        ''' </summary>
        ''' <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
        ''' <remarks>
        ''' This method is to be called when the login process is complete and the current user does not have 
        ''' access to the application.
        ''' </remarks>
        Public Sub OnLoginUnsuccessful(ByVal e As CancelEventArgs)

            RaiseEvent LoginUnsuccessful(Me, e)

        End Sub

#End Region

#Region " Code Scope: Private Code "

        Private Sub Page_PreInit(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreInit

            ' This is the earliest stage in the page life-cycle we can engage the security. So for performace,
            ' we first look to see if we have a security control we cached previously. If so, we'll use it,
            ' and if we don't we'll initialize a new one.
            m_securityProvider = WebSecurityProvider.LoadFromCache(Me)
            If m_securityProvider IsNot Nothing Then
                ' Cached - use it.
                m_securityProvider.LoginUser()  ' Perform the login operation.
            Else
                ' Not cached - initialize new.
                m_securityProvider = New WebSecurityProvider()
                m_securityProvider.Parent = Me
                m_securityProvider.PersistSettings = True
                m_securityProvider.ApplicationName = m_applicationName
                m_securityProvider.Server = m_securityServer
                m_securityProvider.AuthenticationMode = m_authenticationMode
                m_securityProvider.EndInit()    ' This will load settings from config file & perform login.
            End If

        End Sub

        Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload

            ' We're done with the security control so we'll set the member variable to Nothing. This will cause 
            ' all the event handlers to the security control events to be removed. If we don't do this then the 
            ' the security control will have reference to this page via the event handlers and since it is cached,
            ' this page will also be cached - which we don't want to happen.
            m_securityProvider = Nothing

        End Sub

        Private Sub m_securityProvider_AccessDenied(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessDenied

            OnLoginUnsuccessful(e)

        End Sub

        Private Sub m_securityProvider_AccessGranted(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessGranted

            OnLoginSuccessful(e)

        End Sub

#End Region

    End Class

End Namespace