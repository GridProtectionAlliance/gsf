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

#Region " Public Code "

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecureUserControl class.
        ''' </summary>
        Public Sub New()

            MyClass.New("")

        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        Public Sub New(ByVal applicationName As String)

            MyClass.New(applicationName, SecurityServer.Development)

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

#Region " Protected Code "

        ''' <summary>
        ''' Raises the Tva.Web.UI.SecureUserControl.LoginSuccessful event.
        ''' </summary>
        ''' <param name="e">A System.ComponentModel.CancelEventArgs that contains the event data.</param>
        ''' <remarks>
        ''' This method is to be called when the login process is complete and  the current user has access to the 
        ''' application.
        ''' </remarks>
        Public Sub OnLoginSuccessful(ByVal e As CancelEventArgs)

            RaiseEvent LoginSuccessful(Me, e)

        End Sub

        ''' <summary>
        ''' Raises the Tva.Web.UI.SecureUserControl.LoginUnsuccessful event.
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

#Region " Private Code "

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

            ' Initialize the WebSecurityProvider component that will handle the security.
            Dim secureParent As SecurePage = TryCast(Me.Page, SecurePage)
            ' We'll check if this control is being used inside a secure page (i.e. page that is already 
            ' implementing security). If so, we'll just use the page-level security and not go through the login
            ' process again since it'll be redundant anyways.
            If secureParent IsNot Nothing AndAlso secureParent.SecurityProvider IsNot Nothing Then
                ' Security has been implemented at page-level so we'll just it instead. This means that we'll be
                ' checking the current user's access for the application that's defined at the page-level and
                ' ignore WebSecurityProvider information that is specified at the control-level.
                With secureParent
                    m_securityProvider = .SecurityProvider
                    If .SecurityProvider.UserHasApplicationAccess() Then
                        ' Current user has access to the application as defined at the page-level so we'll raise
                        ' the LoginSuccessful event so that the consumer can perform any necessary operations there.
                        OnLoginSuccessful(New CancelEventArgs())
                    Else
                        ' Current user does not have access to the application as defined at the page-level so 
                        ' we'll raise the LoginUnsuccessful event so that we can perform any necessary operations 
                        ' that we need to and so can the consumer.
                        OnLoginUnsuccessful(New CancelEventArgs())
                    End If
                End With
            Else
                ' We'll initialize the WebSecurityProvider component with the specified information since the page
                ' in which this control is being used does not implement security.
                m_securityProvider = New WebSecurityProvider()
                m_securityProvider.Parent = Me.Page
                m_securityProvider.ApplicationName = m_applicationName
                m_securityProvider.Server = m_securityServer
                m_securityProvider.EnableCaching = m_enableCaching

                ' We must explicitly call the LoginUser() method because when this event is fired, the page's  
                ' Pre_Init event has already fired and the WebSecurityProvider will not implicitly initiate the 
                ' login process.
                m_securityProvider.LoginUser()
            End If

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