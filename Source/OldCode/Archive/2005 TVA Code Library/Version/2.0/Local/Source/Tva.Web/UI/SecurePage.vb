' 12/14/2006

Imports System.ComponentModel
Imports Tva.Security.Application

Namespace UI

    Public Class SecurePage
        Inherits System.Web.UI.Page

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

#Region " Public Code "

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecurePage class.
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
        ''' Initializes a new instance of Tva.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the Tva.Security.Application.SecurityServer values.</param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer)

            MyClass.New(applicationName, securityServer, True)

        End Sub

        ''' <summary>
        ''' Initializes a new instance of Tva.Web.UI.SecurePage class.
        ''' </summary>
        ''' <param name="applicationName">Name of the application as in the security database.</param>
        ''' <param name="securityServer">One of the Tva.Security.Application.SecurityServer values.</param>
        ''' <param name="enableCaching">
        ''' Boolean value indicating whether the current user's information is to be cached upon successful login 
        ''' for improved performance.
        ''' </param>
        Public Sub New(ByVal applicationName As String, ByVal securityServer As SecurityServer, ByVal enableCaching As Boolean)

            MyBase.New()
            m_securityProvider = New WebSecurityProvider()
            m_securityProvider.Parent = Me
            m_securityProvider.ApplicationName = applicationName
            m_securityProvider.Server = securityServer
            m_securityProvider.EnableCaching = enableCaching

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

            RaiseEvent LoginUnsuccessful(Me, e)

        End Sub

#End Region

#Region " Private Code "

        Private Sub m_securityProvider_AccessDenied(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessDenied

            OnLoginUnsuccessful(e)

        End Sub

        Private Sub m_securityProvider_AccessGranted(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles m_securityProvider.AccessGranted

            Me.OnLoginSuccessful(e)

        End Sub

#End Region

    End Class

End Namespace