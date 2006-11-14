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
        Private m_extendeeControls As Hashtable
        Private m_devConnectionString As String
        Private m_accConnectionString As String
        Private m_prdConnectionString As String

        Private Const ConfigurationElement As String = "SecurityProvider"

#End Region

#Region " Event Declaration "

        Public Event LoginFailed As EventHandler
        Public Event LoginSuccessful As EventHandler

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
                        HandleLoginFailure()
                    End If
                End If

                If m_user IsNot Nothing AndAlso m_user.IsAuthenticated AndAlso _
                        m_user.FindApplication(m_applicationName) IsNot Nothing Then
                    ' User has been authenticated successfully and has access to the specified application.
                    ProcessControls()
                    RaiseEvent LoginSuccessful(Me, EventArgs.Empty)
                Else
                    ' The user could not be autheticated or doen't have access to the specified application.
                    RaiseEvent LoginFailed(Me, EventArgs.Empty)
                    HandleLoginFailure()
                End If
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

        ''' <summary>
        ''' Shows a screen to provide the logn credentials.
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
            Catch ex As Exception
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