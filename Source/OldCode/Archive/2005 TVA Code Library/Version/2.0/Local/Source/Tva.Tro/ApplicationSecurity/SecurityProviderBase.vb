' 09-22-06

Imports System.Reflection
Imports System.ComponentModel
Imports System.Data.SqlClient

Namespace ApplicationSecurity

    <ProvideProperty("ValidRole", GetType(Object)), ProvideProperty("ValidRoleAction", GetType(Object))> _
    Public MustInherit Class SecurityProviderBase
        Implements IExtenderProvider, ISupportInitialize

        Private m_user As User
        Private m_server As SecurityServer
        Private m_applicationName As String
        Private m_propertyValues As Hashtable
        Private m_devConnectionString As String
        Private m_accConnectionString As String
        Private m_prdConnectionString As String

        Private Const ConfigurationElement As String = "SecurityProvider"

        Public Event LoginFailed As EventHandler
        Public Event LoginSuccessful As EventHandler

        <Category("Configuration")> _
        Public Property Server() As SecurityServer
            Get
                Return m_server
            End Get
            Set(ByVal value As SecurityServer)
                m_server = value
            End Set
        End Property

        <Category("Configuration")> _
        Public Property ApplicationName() As String
            Get
                Return m_applicationName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then
                    m_applicationName = value
                Else
                    Throw New ArgumentException("ArgumentName cannot be null or empty.")
                End If
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ConnectionString() As String
            Get
                Select Case m_server
                    Case SecurityServer.Development
                        Return m_devConnectionString
                    Case SecurityServer.Acceptance
                        Return m_accConnectionString
                    Case SecurityServer.Production
                        Return m_prdConnectionString
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property User() As User
            Get
                Return m_user
            End Get
        End Property

        Protected Sub LoginUser()

            If Not String.IsNullOrEmpty(m_applicationName) Then
                Dim userLoginID As String = "" 'System.Threading.Thread.CurrentPrincipal.Identity.Name
                If Not String.IsNullOrEmpty(userLoginID) Then
                    ' User is internal since we have his/her login ID.
                    m_user = New User(userLoginID.Split("\"c)(1), New SqlConnection(ConnectionString))
                Else
                    ' User is either external or internal accessing from the internet (in case of web application).
                    Dim username As String = GetUsername()
                    Dim password As String = GetPassword()
                    If Not String.IsNullOrEmpty(username) AndAlso Not String.IsNullOrEmpty(password) Then
                        m_user = New User(username, password, New SqlConnection(ConnectionString))
                    Else
                        ' Show the login screen where the user can either his/her username and password.
                        ShowLoginScreen()
                    End If
                End If

                If m_user IsNot Nothing AndAlso m_user.IsAuthenticated AndAlso _
                        m_user.FindApplication(m_applicationName) IsNot Nothing Then
                    ' User has been authenticated successfully and has access to the current application.
                    RaiseEvent LoginSuccessful(Me, EventArgs.Empty)
                Else
                    RaiseEvent LoginFailed(Me, EventArgs.Empty)
                    HandleLoginFailure()
                End If
            Else
                Throw New InvalidOperationException("ApplicationName must be set in order to login the user.")
            End If

        End Sub

        ''' <summary>
        ''' Shows a screen to provide the logn credentials.
        ''' </summary>
        Protected MustOverride Sub ShowLoginScreen()

        ''' <summary>
        ''' Takes appropriate action when the login process for the user fails.
        ''' </summary>
        Protected MustOverride Sub HandleLoginFailure()

        ''' <summary>
        ''' Gets the name that the user provided on the login screen.
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function GetUsername() As String

        ''' <summary>
        ''' Gets the password that the user provided on the login screen.
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function GetPassword() As String

#Region " IExtenderProvider Implementation "

        Public Function CanExtend(ByVal extendee As Object) As Boolean Implements System.ComponentModel.IExtenderProvider.CanExtend

            Return (TypeOf extendee Is System.Web.UI.Control OrElse TypeOf extendee Is System.Windows.Forms.Control)

        End Function

        Public Function GetValidRole(ByVal extendee As Object) As String

            Return GetProperties(extendee).ValidRole

        End Function

        Public Sub SetValidRole(ByVal extendee As Object, ByVal value As String)

            GetProperties(extendee).ValidRole = value

        End Sub

        Public Function GetValidRoleAction(ByVal extendee As Object) As ValidRoleAction

            Return GetProperties(extendee).ValidRoleAction

        End Function

        Public Sub SetValidRoleAction(ByVal extendee As Object, ByVal value As ValidRoleAction)

            Dim extendedProperties As ControlProperties = GetProperties(extendee)
            extendedProperties.ValidRoleAction = value

            If Not extendedProperties.ActionTaken Then
                If extendedProperties.ValidRoleAction <> ValidRoleAction.None Then
                    Dim controlProperty As PropertyInfo = _
                        extendee.GetType().GetProperty(extendedProperties.ValidRoleAction.ToString())
                    If controlProperty IsNot Nothing AndAlso _
                            m_user IsNot Nothing AndAlso m_user.FindRole(extendedProperties.ValidRole) Is Nothing Then
                        ' User is not in the specified role, so we'll NOT the specified control property.
                        controlProperty.SetValue(extendee, Not Convert.ToBoolean(controlProperty.GetValue(extendee, Nothing)), Nothing)
                    End If
                End If

                extendedProperties.ActionTaken = True
            End If

        End Sub

        Private Function GetProperties(ByVal extendee As Object) As ControlProperties

            Dim properties As ControlProperties = DirectCast(m_propertyValues(extendee), ControlProperties)
            If properties Is Nothing Then
                properties = New ControlProperties()
                m_propertyValues.Add(extendee, properties)
            End If

            Return properties

        End Function

        Private Class ControlProperties

            Public ValidRole As String
            Public ValidRoleAction As ValidRoleAction
            Public ActionTaken As Boolean

        End Class

#End Region

#Region " ISupportInitialize Implementation "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            ' Nothing needs to be done when the component begins initializing.

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then
                LoginUser()
            End If

        End Sub

#End Region

    End Class

End Namespace