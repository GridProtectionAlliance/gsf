' 12/14/2006

Imports System.ComponentModel
Imports Tva.Security.Application

Namespace UI

    Public Class SecureUserControl
        Inherits System.Web.UI.UserControl

#Region " Member Declaration "

        Private m_applicationName As String
        Private m_securityServer As SecurityServer
        Private m_enableCaching As Boolean
        Private m_isLoginUnsuccessful As Boolean
        Private WithEvents m_securityProvider As WebSecurityProvider

#End Region

#Region " Event Declaration "

        ''' <summary>
        ''' Occurs when the login process is complete, and  the current user has access to the application.
        ''' </summary>
        <Description("Occurs when the login process is complete, and  the current user has access to the application."), Category("Security")> _
        Public Event LoginSuccessful As EventHandler

        ''' <summary>
        ''' Occurs when the login process is complete, and the current user does not have access to the application.
        ''' </summary>
        <Description("Occurs when the login process is complete, and the current user does not have access to the application."), Category("Security")> _
        Public Event LoginUnsuccessful As EventHandler

#End Region

#Region " Public Code "

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecureUserControl class.
        ''' </summary>
        Public Sub New()

            MyClass.New("", SecurityServer.Development)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecureUserControl class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the Tva.Security.Application.SecurityServer values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer)

            MyClass.New(applicationName, securityServer, True)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecureUserControl class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the Tva.Security.Application.SecurityServer values.</param>
        ''' <param name="enableCaching">
        ''' Boolean value indicating whether the current user's information is to be cached upon successful login 
        ''' for improved performance.
        ''' </param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer, ByVal enableCaching As Boolean)

            MyBase.New()
            m_applicationName = applicationName
            m_securityServer = securityServer
            m_enableCaching = enableCaching
            m_securityProvider = New WebSecurityProvider()

        End Sub

        ''' <summary>
        ''' Gets the Tva.Security.Application.WebSecurityProvider component that handles the security.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The Tva.Security.Application.WebSecurityProvider component.</returns>
        <Browsable(False)> _
        Public ReadOnly Property SecurityProvider() As WebSecurityProvider
            Get
                Return m_securityProvider
            End Get
        End Property

#End Region

#Region " Private Code "

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

            ' Initialize the WebSecurityProvider component that will handle the security.
            m_securityProvider.Parent = Me.Page
            Dim secureParent As SecurePage = TryCast(m_securityProvider.Parent, SecurePage)
            If secureParent IsNot Nothing AndAlso secureParent.SecurityProvider IsNot Nothing Then
                ' The page in which this control is being used also incorporated security, so we'll use the
                ' information from the page for the WebSecurityProvider and ingnore the application name
                ' and security server that was provided (if it was) when this control was initialized.
                m_securityProvider.ApplicationName = secureParent.SecurityProvider.ApplicationName
                m_securityProvider.Server = secureParent.SecurityProvider.Server
            Else
                m_securityProvider.ApplicationName = m_applicationName
                m_securityProvider.Server = m_securityServer
                m_securityProvider.EnableCaching = m_enableCaching
            End If

            ' We must explicitly call the LoginUser() method because when this event is fired, the page's Pre_Init
            ' event has already fired and the WebSecurityProvider will not implicitly initiate the login process.
            ' Even if this control is being used inside a page that is secured (i.e. login process is performed),
            ' the performance hit will be minimum if caching is enabled, in which case this control will be using
            ' the user data that has already been initialized by the secure page that contains this control. 
            m_securityProvider.LoginUser()

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

            ' At a control level, we don't want the page using the control to be redirected to the default access 
            ' denied page if the current user doesn't have access to the application, but we'll hide the control instead. 
            e.Cancel = True
            m_isLoginUnsuccessful = True

            RaiseEvent LoginUnsuccessful(sender, e)

        End Sub

        Private Sub m_securityProvider_AccessGranted(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessGranted

            RaiseEvent LoginSuccessful(sender, e)

        End Sub

#End Region

    End Class

End Namespace