'*******************************************************************************************************
'  TVA.Security.Application.SecurityProviderBase.vb - Base class for application security provider
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  09-22-06 - Pinal C. Patel
'       Original version of source code generated.
'  11/30/2007 - Pinal C. Patel
'       Modified the "design time" check in EndInit() method to use LicenseManager.UsageMode property
'       instead of DesignMode property as the former is more accurate than the latter.
'  12/28/2007 - Pinal C. Patel
'       Modified the ConnectionString property to use the backup SQL Server database in case if any of 
'       the primary databases are unavailable or offline.
'       Renamed the DbConnectionException event to DatabaseException as this event is raised in the
'       event of any SQL Server exception that is encountered.
'
'*******************************************************************************************************

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
        Private m_acpConnectionString As String
        Private m_prdConnectionString As String
        Private m_bakConnectionString As String

#End Region

#Region " Event Declaration "

        Public Event BeforeLogin As EventHandler(Of CancelEventArgs)
        Public Event BeforeAuthenticate As EventHandler(Of CancelEventArgs)
        Public Event AfterLogin As EventHandler
        Public Event AfterAuthenticate As EventHandler
        Public Event AccessGranted As EventHandler(Of CancelEventArgs)
        Public Event AccessDenied As EventHandler(Of CancelEventArgs)
        Public Event DatabaseException As EventHandler(Of GenericEventArgs(Of Exception))

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
        Public ReadOnly Property User() As User
            Get
                Return m_user
            End Get
        End Property

        Public Function UserHasApplicationAccess() As Boolean

            Return (m_user IsNot Nothing AndAlso Not m_user.IsLockedOut AndAlso _
                        m_user.IsAuthenticated AndAlso m_user.FindApplication(m_applicationName) IsNot Nothing)

        End Function

        ''' <summary>
        ''' Logs in the current user.
        ''' </summary>
        Public Sub LoginUser()

            If Not String.IsNullOrEmpty(m_applicationName) Then
                Dim beforeLoginEventData As New CancelEventArgs()
                RaiseEvent BeforeLogin(Me, beforeLoginEventData)
                If beforeLoginEventData.Cancel Then Exit Sub

                If m_enableCaching Then RetrieveUserData()
                ' m_user will be initialized by RetrieveUserData() if user data was cached previously.
                If m_user Is Nothing Then
                    ' We don't have data about the user, so we must get it.
                    Dim username As String = GetUsername()  ' Get the username from inheriting class if it has.
                    Dim password As String = GetPassword()  ' Get the password from inheriting class if it has.

                    ' This will get us the login ID of the current user. This will be null in case of web app if:
                    ' 1) Secured web page is being accessed from outside.
                    ' 2) Secured web page is being accessed from inside, but "Integrated Windows Authentication"
                    '    is turned off for the web site.
                    ' Note: In case of a windows app, we'll always get the login ID of the current user.
                    Dim userLoginID As String = System.Threading.Thread.CurrentPrincipal.Identity.Name

                    If Not String.IsNullOrEmpty(username) AndAlso Not String.IsNullOrEmpty(password) Then
                        ' First, if we get the username and password from the inheriting class, we'll use it to 
                        ' initialize the user data. This is very important for the following scenarios to work:
                        ' o Internal user wants to access a secure page for which he/she does not have access, but
                        '   have the credentials of a user who has access to this page and want to use the 
                        '   credentials in order to access the secure web page.
                        ' o Developer of an external facing web site wants to test the security without turning-off
                        '   "Integrated Windows Authentication" for the web site, as doing so disable the debugging
                        '   capabilities from the Visual Studio IDE.
                        ' Note: Both of the scenarios above require that the person trying do access the secured web 
                        '       page with someone else's credentials does not access to the web page. 
                        InitializeUser(username, password)
                    ElseIf Not String.IsNullOrEmpty(userLoginID) Then
                        InitializeUser(userLoginID.Split("\"c)(1))
                    Else
                        ' If both the above attempts fail to initialize the user data, we'll have to show the login
                        ' screen, capture the user credentials and then initialize the user data.
                        ShowLoginScreen()
                    End If
                End If

                If m_user IsNot Nothing Then
                    Dim beforeAuthenticateEventData As New CancelEventArgs()
                    RaiseEvent BeforeAuthenticate(Me, beforeAuthenticateEventData)
                    If beforeAuthenticateEventData.Cancel Then Exit Sub

                    If UserHasApplicationAccess() Then
                        ' Upon successful login, we'll cache the user data if specified in the configuration.
                        If m_enableCaching Then CacheUserData()

                        ' User has been authenticated successfully and has access to the specified application.
                        Dim accessGrantedEventData As New CancelEventArgs()
                        RaiseEvent AccessGranted(Me, accessGrantedEventData)
                        If accessGrantedEventData.Cancel Then Exit Sub

                        ProcessControls()
                        HandleAccessGranted()
                    Else
                        ' User could not be autheticated or doesn't have access to the specified application.
                        ' Most likely user authentication will never fail because if the user is external, the
                        ' login page will verify the user's password before this process kicks in.
                        Dim accessDeniedEventData As New CancelEventArgs()
                        RaiseEvent AccessDenied(Me, accessDeniedEventData)
                        If accessDeniedEventData.Cancel Then Exit Sub

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

        ''' <summary>
        ''' Category name under which settings are to be saved in the config file.
        ''' </summary>
        Protected Const SettingsCategory As String = "SecurityProvider"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This property must be accessible only by components inheriting from this component that exists in this 
        ''' assembly. This is done so that the database connection string is not exposed even when a component that
        ''' exists outside of this assembly inherits from this component.
        ''' </remarks>
        Protected Friend ReadOnly Property ConnectionString() As String
            Get
                ' First, we'll try connecting to the primary database for the selected SecurityServer. If we're
                ' successful, we'll return its connection string, and if not, we'll return the connection
                ' string of the backup database.
                Try
                    Dim primaryConnectionString As String = ""
                    Select Case m_server
                        Case SecurityServer.Development
                            primaryConnectionString = m_devConnectionString
                        Case SecurityServer.Acceptance
                            primaryConnectionString = m_acpConnectionString
                        Case SecurityServer.Production
                            primaryConnectionString = m_prdConnectionString
                    End Select

                    Using testConnection As New SqlConnection(primaryConnectionString)
                        testConnection.Open()
                    End Using

                    ' Return connection string of the primary database.
                    Return primaryConnectionString
                Catch ex As Exception
                    ' Return connection string of the backup database.
                    Return m_bakConnectionString
                End Try
            End Get
        End Property

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

                m_user = New User(username, password, connection, m_applicationName)
            Catch ex As SqlException
                ' We'll notifying about the excountered SQL exception by rasing an event.
                RaiseEvent DatabaseException(Me, New GenericEventArgs(Of Exception)(ex))
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

            If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then
                LoginUser()
            End If

        End Sub

#End Region

    End Class

End Namespace