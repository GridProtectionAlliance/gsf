' 12/14/2006

Imports System.ComponentModel
Imports TVA.Security.Application

Namespace UI

    Public Class SecureUserControl
        Inherits System.Web.UI.UserControl

#Region " Member Declaration "

        Private m_applicationName As String
        Private m_securityServer As SecurityServer
        Private m_authenticationMode As AuthenticationMode
        Private m_isLoginUnsuccessful As Boolean

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
        ''' Initializes a new instance of TVA.Web.UI.SecureUserControl class.
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
        ''' Initializes a new instance of TVA.Web.UI.SecureUserControl class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the TVA.Security.Application.SecurityServer values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer)

            MyClass.New(applicationName, securityServer, True)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of TVA.Web.UI.SecureUserControl class.
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
            WebSecurityProvider.SaveToCache(Me.Page, m_securityProvider)

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

            ' At control-level, we don't want the page using the control to be redirected to the default access 
            ' denied page if the current user doesn't have access to the application, so we cancel further 
            ' processing of the login process if this method is called from the AccessDenied event of the 
            ' WebSecurityProvider (remember if this control is being used inside a secure page than we'll just be 
            ' the page-level security and not go through the login process again); instead we'll set a member 
            ' variable that'll be used during the PreRender event to hide the control. 
            e.Cancel = True
            m_isLoginUnsuccessful = True

            RaiseEvent LoginUnsuccessful(Me, e)

        End Sub

#End Region

#Region " Code Scope: Private Code "

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

            ' This is the earliest stage in the control life-cycle we can engage the security. So for performace,
            ' we first look to see if we have a security control we cached previously either at the page or at 
            ' this control or another control . If so, we'll use it, and if we don't we'll initialize a new one.
            m_securityProvider = WebSecurityProvider.LoadFromCache(Me.Page)
            If m_securityProvider IsNot Nothing Then
                ' The fact that the security control was cached means that the login process was successful for
                ' the current user. So, at a control level all we need to verify user access and raise appropriate
                ' events (they come in handy for taking appropriate action at a control level). 
                If m_securityProvider.UserHasApplicationAccess() Then
                    ' This will always be the case.
                    OnLoginSuccessful(New CancelEventArgs())
                Else
                    ' This will never be the case, and here's why: 
                    ' 1) If this control is in a secure page, the page level security will prevent this control
                    '    from being loaded and processed, so this event will never get fired.
                    ' 2) If this control is in a non-secure page, the security control would not be in the cache,
                    '    as the security control is cached only upon successful login.
                    OnLoginUnsuccessful(New CancelEventArgs())
                End If
            Else
                ' Not cached - initialize new.
                m_securityProvider = New WebSecurityProvider()
                m_securityProvider.Parent = Me.Page
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
            ' the security control will have reference to this control's page via the event handlers and since 
            ' it is cached, the page will also be cached - which we don't want to happen.
            m_securityProvider = Nothing

        End Sub

        Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender

            If m_isLoginUnsuccessful Then
                ' By the time this event is raised, the login process will be complete unless it is cancelled.
                ' If the consumer cancelled the login process or some part of it, then the member valriable we're 
                ' checking will never be set even if the current user is not authorized; in this case the control 
                ' we still be visible, even when the user is unauthorized and we'll assume that the consumer want 
                ' to do something custom so we just won't intervene.
                Me.Visible = False
            End If

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