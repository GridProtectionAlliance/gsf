' 09-22-06

Imports System.Reflection
Imports System.ComponentModel
Imports System.Data.SqlClient

Namespace Application

    <ProvideProperty("ValidRoles", GetType(Object)), ProvideProperty("ValidRoleAction", GetType(Object))> _
    Public MustInherit Class SecurityProviderBase
        Implements IExtenderProvider, ISupportInitialize

#Region " Member Declaration "

        Private m_user As User
        Private m_server As SecurityServer
        Private m_applicationName As String
        Private m_enableCaching As Boolean
        Private m_extendeeControls As Hashtable
        Private m_devConnectionString As String
        Private m_accConnectionString As String
        Private m_prdConnectionString As String

        Private Const ConfigurationElement As String = "SecurityProvider"

#End Region

#Region " Event Declaration "

        Public Event BeforeLogin(ByVal sender As Object, ByVal e As CancelEventArgs)
        Public Event BeforeAuthenticate(ByVal sender As Object, ByVal e As CancelEventArgs)
        Public Event AfterLogin As EventHandler
        Public Event AfterAuthenticate As EventHandler
        Public Event AccessGranted(ByVal sender As Object, ByVal e As CancelEventArgs)
        Public Event AccessDenied(ByVal sender As Object, ByVal e As CancelEventArgs)
        Public Event ServerUnavailable As EventHandler

#End Region

#Region " Public Code "

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
                    Throw New ArgumentException("ApplicationName cannot be null or empty.")
                End If
            End Set
        End Property

        <Category("Configuration")> _
        Public Property EnableCaching() As Boolean
            Get
                Return m_enableCaching
            End Get
            Set(ByVal value As Boolean)
                m_enableCaching = value
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

        ''' <summary>
        ''' Logs in the current user.
        ''' </summary>
        Public Sub LoginUser()

            If Not String.IsNullOrEmpty(m_applicationName) Then
                Dim beforeLoginResponse As New CancelEventArgs()
                RaiseEvent BeforeLogin(Me, beforeLoginResponse)
                If beforeLoginResponse.Cancel Then Exit Sub

                RetrieveUserData()
                ' m_user will be initialized by RetrieveUserData() if user data was cached previously.
                If m_user Is Nothing Then
                    ' This is the best way of getting the current user's NT ID both in windows and web environments.
                    Dim userLoginID As String = System.Threading.Thread.CurrentPrincipal.Identity.Name
                    If Not String.IsNullOrEmpty(userLoginID) Then
                        ' User is internal since we have his/her login ID.
                        InitializeUser(userLoginID.Split("\"c)(1))
                    Else
                        ' User is either external or internal accessing from the internet (in case of web application).
                        ' NOTE: It is important to note that this condition will never be true in case of a windows
                        ' application, since we will always get the NT ID of the current user.
                        Dim username As String = GetUsername()
                        Dim password As String = GetPassword()
                        If Not String.IsNullOrEmpty(username) AndAlso Not String.IsNullOrEmpty(password) Then
                            InitializeUser(username, password)
                        Else
                            ' Since we don't have the username and password required for authenticating the user,
                            ' we'll ask for the user's username and password.
                            ShowLoginScreen()
                        End If
                    End If
                End If

                If m_user IsNot Nothing Then
                    Dim beforeAuthenticateResponse As New CancelEventArgs()
                    RaiseEvent BeforeAuthenticate(Me, beforeAuthenticateResponse)
                    If beforeAuthenticateResponse.Cancel Then Exit Sub

                    If m_user.IsAuthenticated AndAlso m_user.FindApplication(m_applicationName) IsNot Nothing Then
                        ' User has been authenticated successfully and has access to the specified application.
                        Dim accessGrantedResponse As New CancelEventArgs()
                        RaiseEvent AccessGranted(Me, accessGrantedResponse)
                        If accessGrantedResponse.Cancel Then Exit Sub

                        ProcessControls()
                        HandleAccessGranted()
                    Else
                        ' User could not be autheticated or doen't have access to the specified application.
                        Dim accessDeniedResponse As New CancelEventArgs()
                        RaiseEvent AccessDenied(Me, accessDeniedResponse)
                        If accessDeniedResponse.Cancel Then Exit Sub

                        HandleAccessDenied()
                    End If

                    RaiseEvent AfterAuthenticate(Me, EventArgs.Empty)
                End If

                RaiseEvent AfterLogin(Me, EventArgs.Empty)
            Else
                Throw New InvalidOperationException("ApplicationName must be set in order to login the user.")
            End If

        End Sub

        ''' <summary>
        ''' Logs out the logged in user.
        ''' </summary>
        Public MustOverride Sub LogoutUser()

#End Region

#Region " Protected Code "

        Protected Sub UpdateUserData(ByVal userData As User)

            m_user = userData

        End Sub

        ''' <summary>
        ''' Caches user data to be used later.
        ''' </summary>
        Protected MustOverride Sub CacheUserData()

        ''' <summary>
        ''' Retrieves previously cached user data.
        ''' </summary>
        Protected MustOverride Sub RetrieveUserData()

        ''' <summary>
        ''' Shows a login screen where user can enter his/her credentials.
        ''' </summary>
        ''' <remarks></remarks>
        Protected MustOverride Sub ShowLoginScreen()

        ''' <summary>
        ''' Performs any necessary actions that must be performed upon successful login.
        ''' </summary>
        Protected MustOverride Sub HandleAccessGranted()

        ''' <summary>
        ''' Performs any necessary actions that must be performed upon unsuccessful login.
        ''' </summary>
        Protected MustOverride Sub HandleAccessDenied()

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

#End Region

#Region " Private Code "

        Private Sub InitializeUser(ByVal username As String)

            InitializeUser(username, Nothing)

        End Sub

        Private Sub InitializeUser(ByVal username As String, ByVal password As String)

            Dim connection As SqlConnection = Nothing
            Try
                ' We'll try to retrieve user information from the security database.
                connection = New SqlConnection(ConnectionString)
                connection.Open()
                m_user = New User(username, password, connection)

                ' We'll cache the user data if specified in the configuration.
                If m_enableCaching Then CacheUserData()
            Catch ex As SqlException
                ' We're likely to encounter a SQL exception only when the database server is offline.
                RaiseEvent ServerUnavailable(Me, EventArgs.Empty)
            Catch ex As Exception
                ' We'll just ignore all other exceptions.
            Finally
                If connection IsNot Nothing Then
                    connection.Close()
                    connection.Dispose()
                End If
            End Try

        End Sub

        Private Sub ProcessControls()

            For Each extendee As Object In m_extendeeControls.Keys
                ProcessControl(extendee, DirectCast(m_extendeeControls(extendee), ControlProperties))
            Next

        End Sub

        Private Sub ProcessControl(ByVal extendee As Object, ByVal extendedProperties As ControlProperties)

            If Not extendedProperties.ActionTaken AndAlso _
                    extendedProperties.ValidRoleAction <> ValidRoleAction.None AndAlso _
                    extendedProperties.ValidRoles IsNot Nothing Then
                Dim controlProperty As PropertyInfo = _
                    extendee.GetType().GetProperty(extendedProperties.ValidRoleAction.ToString())

                If m_user IsNot Nothing AndAlso controlProperty IsNot Nothing Then
                    ' User has been logged in and the control property exists.
                    controlProperty.SetValue(extendee, False, Nothing)   ' By default we'll set the property to False.

                    For Each role As String In extendedProperties.ValidRoles.Replace(" ", "").Replace(",", ";").Split(";"c)
                        If m_user.FindRole(role, m_applicationName) IsNot Nothing Then
                            ' We'll set the property to True if the current user belongs either one of the valid roles.
                            controlProperty.SetValue(extendee, True, Nothing)
                            Exit For
                        End If
                    Next
                End If
            End If

        End Sub

#End Region

#Region " IExtenderProvider Implementation "

        Public Function CanExtend(ByVal extendee As Object) As Boolean Implements System.ComponentModel.IExtenderProvider.CanExtend

            Return (TypeOf extendee Is System.Web.UI.Control OrElse TypeOf extendee Is System.Windows.Forms.Control)

        End Function

        Public Function GetValidRoles(ByVal extendee As Object) As String

            Return GetProperties(extendee).ValidRoles

        End Function

        Public Sub SetValidRoles(ByVal extendee As Object, ByVal value As String)

            Dim extendedProperties As ControlProperties = GetProperties(extendee)
            extendedProperties.ValidRoles = value

            ProcessControl(extendee, extendedProperties)

        End Sub

        Public Function GetValidRoleAction(ByVal extendee As Object) As ValidRoleAction

            Return GetProperties(extendee).ValidRoleAction

        End Function

        Public Sub SetValidRoleAction(ByVal extendee As Object, ByVal value As ValidRoleAction)

            Dim extendedProperties As ControlProperties = GetProperties(extendee)
            extendedProperties.ValidRoleAction = value

            ProcessControl(extendee, extendedProperties)

        End Sub

        Private Function GetProperties(ByVal extendee As Object) As ControlProperties

            Dim properties As ControlProperties = DirectCast(m_extendeeControls(extendee), ControlProperties)
            If properties Is Nothing Then
                properties = New ControlProperties()
                m_extendeeControls.Add(extendee, properties)
            End If

            Return properties

        End Function

        Private Class ControlProperties

            Public ValidRoles As String
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