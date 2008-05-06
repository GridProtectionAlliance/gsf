'*******************************************************************************************************
'  TVA.Security.Application.User.vb - User defined in the security database
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
'  09/26/2006 - Pinal C. Patel
'       Original version of source code generated.
'  10/22/2007 - Pinal C. Patel
'       Added GeneratePassword() shared function and enforced strong password rule in EncryptPassword().
'  05/06/2008 - Pinal C. Patel
'       Modified contructors so security database connection string is not to be passed in.
'       Added methods: RefreshData, Authenticate, ChangePassword, LogAccess and LogError.
'       Added support to authenticate user credentials against RSA server using RADIUS.
'
'*******************************************************************************************************

Imports System.Data
Imports System.Data.SqlClient
Imports System.Text
Imports System.Text.RegularExpressions
Imports TVA.Common
Imports TVA.Identity
Imports TVA.Data.Common
Imports TVA.Math.Common
Imports TVA.Security.Radius
Imports TVA.Security.Cryptography.Common

Namespace Application

    ''' <summary>
    ''' Represents a user defined in the security database.
    ''' </summary>
    <Serializable()> _
    Public Class User

#Region " Member Declaration "

        Private m_username As String
        Private m_password As String
        Private m_firstName As String
        Private m_lastName As String
        Private m_companyName As String
        Private m_phoneNumber As String
        Private m_emailAddress As String
        Private m_securityQuestion As String
        Private m_securityAnswer As String
        Private m_isExternal As Boolean
        Private m_isLockedOut As Boolean
        Private m_passwordChangeDateTime As System.DateTime
        Private m_accountCreatedDateTime As System.DateTime
        Private m_isDefined As Boolean
        Private m_isAuthenticated As Boolean
        Private m_groups As List(Of Group)
        Private m_roles As List(Of Role)
        Private m_applications As List(Of Application)
        Private m_applicationName As String
        Private m_securityServer As SecurityServer
        Private m_authenticationMode As AuthenticationMode

        Private Const MinimumPasswordLength As Integer = 8
        Private Const StrongPasswordRegex As String = "^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$"

        ''' <summary>
        ''' Connection string for the development security database.
        ''' </summary>
        Private Const DevConnectionString As String = "KACpAHIAWwDRAFIAGADyAEMAZwBsAEsAcAAQAFIAbwBNAE0ANwD9AFkAIwBUAG0AOQCSAEgAMADgAFAAWwB6AGAAOwBeAE0AMwAiAG0APQA4ADwAAwCPADMARwBqAFQAYABiADIASgBHAEoADgAFAG0AHQABAF8AawBrACQADAD6AC8ACQAGAFsAVQA="

        ''' <summary>
        ''' Connection string for the acceptance security database.
        ''' </summary>
        Private Const AcpConnectionString As String = "KACpAHIAWwDRAFIAGADlAFcAZwBuAFwAcgANAFkAEQAjAE0AFwDdAGQABgBwAFAAGADuAF0AewCwAHgAUwBtAGAALQBWAFEAOABMAEkALgA9ACIAAwCYACsACAAjAG4ARwB1AGoASABCAEgAFAAUAHcAHQABAFoAdQBrACQAXAC4AGwAVwAbAEEAFAC3AH8AeQBwADkACwDrACkADAB0AEEAVgA="

        ''' <summary>
        ''' Connection string for the production security database.
        ''' </summary>
        Private Const PrdConnectionString As String = "KACpAHIAWwDRAFIAGAD0AFYAZwBgAF8AZgATAF8AVAAkAEYAGADaAGQACAB7AFgADgDuAFoAFADCAGkAcQBNAFQAHQByABkAfQA1AGkAKgAsACwACwCIAC8AQQALAHEAWQBKAGYASABWAE4AFAAPAGAAdQBEAGwASQBdAHAASQCxACcABAArAGsAawCqAE4AXgBEAHcAXwC6ACEAAQBcAG8AaAAUAB8AaQCTADIASABTAD0A"

        ''' <summary>
        ''' Connection string for the backup security database.
        ''' </summary>
        Private Const BakConnectionString As String = "KACpAHIAWwDRAFIAGADlAFcAZwBgAF8AcAAQAFIAZQBWACkAEgDoAEwAIABXAH8ALwDKADQAAQDgAEwAXgBwAGIALgBDAEsAMgAfAFsAOwAuADsAGACSAD4ABQBxACEAfABvAEsAFgBWAEoADQATAGsARQAaAC8AbAB4AF0APQD5AC4AFwBTAFoAVgDtAA=="

        ''' <summary>
        ''' Primary RSA server used for RSA authentication.
        ''' </summary>
        Private Const RsaServer1 As String = "tro-rsa-1"

        ''' <summary>
        ''' Backup RSA server used for RSA authentication.
        ''' </summary>
        Private Const RsaServer2 As String = "tro-rsa-2"

        ''' <summary>
        ''' Shared secret for RSA authentication using the RADIUS protocol.
        ''' </summary>
        Private Const RadiusSharedSecret As String = "LACNADkAWQDAAFEAFgDMAGoASQBDAHgAWwAiAHwAPwA="

#End Region

#Region " Code Scope: Public "

        ''' <summary>
        ''' Creates an instance of a user defined in the security database.
        ''' </summary>
        ''' <param name="username">Username of the user.</param>
        ''' <param name="password">Password of the user.</param>
        Public Sub New(ByVal username As String, ByVal password As String)

            MyClass.New(username, password, String.Empty)

        End Sub

        ''' <summary>
        ''' Creates an instance of a user defined in the security database.
        ''' </summary>
        ''' <param name="username">Username of the user.</param>
        ''' <param name="password">Password of the user.</param>
        ''' <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
        Public Sub New(ByVal username As String, ByVal password As String, ByVal applicationName As String)

            MyClass.New(username, password, applicationName, SecurityServer.Development, AuthenticationMode.AD)

        End Sub

        ''' <summary>
        ''' Creates an instance of a user defined in the security database.
        ''' </summary>
        ''' <param name="username">Username of the user.</param>
        ''' <param name="password">Password of the user.</param>
        ''' <param name="securityServer">Security server from which user data is to be retrieved.</param>
        Public Sub New(ByVal username As String, ByVal password As String, ByVal securityServer As SecurityServer)

            MyClass.New(username, password, String.Empty, securityServer, AuthenticationMode.AD)

        End Sub

        ''' <summary>
        ''' Creates an instance of a user defined in the security database.
        ''' </summary>
        ''' <param name="username">Username of the user.</param>
        ''' <param name="password">Password of the user.</param>
        ''' <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
        ''' <param name="securityServer">Security server from which user data is to be retrieved.</param>
        ''' <param name="authenticationMode">Mode of authentication to be used for authenticating credentials.</param>
        Public Sub New(ByVal username As String, ByVal password As String, ByVal applicationName As String, _
                       ByVal securityServer As SecurityServer, ByVal authenticationMode As AuthenticationMode)

            MyClass.New(username, password, applicationName, securityServer, authenticationMode, True)

        End Sub

        ''' <summary>
        ''' Gets the user's username.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Username of the user.</returns>
        Public ReadOnly Property Username() As String
            Get
                Return m_username
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's password.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Password of the user.</returns>
        Public ReadOnly Property Password() As String
            Get
                Return m_password
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's first name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>First name of the user.</returns>
        Public ReadOnly Property FirstName() As String
            Get
                Return m_firstName
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's last name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Last name of the user.</returns>
        Public ReadOnly Property LastName() As String
            Get
                Return m_lastName
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's company name.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Company name of the user.</returns>
        Public ReadOnly Property CompanyName() As String
            Get
                Return m_companyName
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's phone number.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Phone number of the user.</returns>
        Public ReadOnly Property PhoneNumber() As String
            Get
                Return m_phoneNumber
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's email address.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Email address of the user.</returns>
        Public ReadOnly Property EmailAddress() As String
            Get
                Return m_emailAddress
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's security question.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Security question of the user.</returns>
        Public ReadOnly Property SecurityQuestion() As String
            Get
                Return m_securityQuestion
            End Get
        End Property

        ''' <summary>
        ''' Gets the user's security answer.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Security answer of the user.</returns>
        Public ReadOnly Property SecurityAnswer() As String
            Get
                Return m_securityAnswer
            End Get
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether or not the user is defined as an external user in the security
        ''' database. An external user is someone outside of TVA who does not have a TVA domain account.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if user is an external user; otherwise False.</returns>
        Public ReadOnly Property IsExternal() As Boolean
            Get
                Return m_isExternal
            End Get
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether or not the user's has been locked because of numerous
        ''' unsuccessful login attempts.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the user account is locked; otherwise False.</returns>
        Public ReadOnly Property IsLockedOut() As Boolean
            Get
                Return m_isLockedOut
            End Get
        End Property

        ''' <summary>
        ''' Gets the date and time when user must change the password.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The date and time when user must change the password.</returns>
        Public ReadOnly Property PasswordChangeDateTime() As System.DateTime
            Get
                Return m_passwordChangeDateTime
            End Get
        End Property

        ''' <summary>
        ''' Gets the date and time when user account was created.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The date and time when user account was created.</returns>
        Public ReadOnly Property AccountCreatedDateTime() As System.DateTime
            Get
                Return m_accountCreatedDateTime
            End Get
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether or not the user is defined in the security database.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the user is defined in the security database; otherwise False.</returns>
        Public ReadOnly Property IsDefined() As Boolean
            Get
                Return m_isDefined
            End Get
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating whether or not the user's credentials have been authenticated.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if the user's credentials have been authenticated; otherwise False.</returns>
        Public ReadOnly Property IsAuthenticated() As Boolean
            Get
                Return m_isAuthenticated
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of all the groups the user belongs to.
        ''' </summary>
        ''' <value></value>
        ''' <returns>Groups to which the user belongs.</returns>
        Public ReadOnly Property Groups() As List(Of Group)
            Get
                Return m_groups
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of roles that belong to the specified application, or all roles if no application is 
        ''' specified, to which the user is assigned. 
        ''' </summary>
        ''' <value></value>
        ''' <returns>List of roles to which the user is assigned.</returns>
        Public ReadOnly Property Roles() As List(Of Role)
            Get
                Return m_roles
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of roles that belong to the specified application to which the user is assigned.
        ''' </summary>
        ''' <param name="applicationName">Name of the application for which user roles are to be retrieved.</param>
        ''' <value></value>
        ''' <returns>List of roles to which the user is assigned.</returns>
        Public ReadOnly Property Roles(ByVal applicationName As String) As List(Of Role)
            Get
                '**** Added By Mehul
                Dim applicationRoles As New List(Of Role)()

                If m_roles IsNot Nothing Then
                    For i As Integer = 0 To m_roles.Count - 1
                        If String.Compare(m_roles(i).Application.Name, applicationName, True) = 0 Then
                            applicationRoles.Add(m_roles(i))
                        End If
                    Next
                End If

                Return applicationRoles
            End Get
        End Property

        ''' <summary>
        ''' Gets a list of all applications to which the user has access if application name is not specified.
        ''' </summary>
        ''' <value></value>
        ''' <returns>List of all applications to which the user has access.</returns>
        Public ReadOnly Property Applications() As List(Of Application)
            Get
                Return m_applications
            End Get
        End Property

        ''' <summary>
        ''' Refreshes user data from the security database.
        ''' </summary>
        Public Sub RefreshData()

            ' Initialize data.
            m_firstName = ""
            m_lastName = ""
            m_companyName = ""
            m_phoneNumber = ""
            m_emailAddress = ""
            m_securityQuestion = ""
            m_securityAnswer = ""
            m_isExternal = False
            m_isLockedOut = False
            m_passwordChangeDateTime = Date.MinValue
            m_accountCreatedDateTime = Date.MinValue
            m_isDefined = False
            m_isAuthenticated = False
            m_groups.Clear()
            m_roles.Clear()
            m_applications.Clear()

            Dim userData As DataSet
            Using dbConnection As SqlConnection = GetDatabaseConnection()
                If dbConnection.State <> ConnectionState.Open Then dbConnection.Open()
                ' We'll retrieve all the data we need in a single trip to the database by calling the stored 
                ' procedure 'RetrieveApiData' that will return 3 tables to us:
                ' Table1 (Index 0): Information about the user.
                ' Table2 (Index 1): Groups the user is a member of.
                ' Table3 (Index 2): Roles that are assigned to the user either directly or through a group.
                userData = RetrieveDataSet("dbo.RetrieveApiData", dbConnection, m_username, m_applicationName)
            End Using

            If userData.Tables(0).Rows.Count > 0 Then
                ' User does exist in the security database.
                m_isDefined = True
                With userData.Tables(0)
                    m_username = .Rows(0)("UserName").ToString()
                    m_password = .Rows(0)("UserPassword").ToString()
                    m_firstName = .Rows(0)("UserFirstName").ToString()
                    m_lastName = .Rows(0)("UserLastName").ToString()
                    m_companyName = .Rows(0)("UserCompanyName").ToString()
                    m_phoneNumber = .Rows(0)("UserPhoneNumber").ToString()
                    m_emailAddress = .Rows(0)("UserEmailAddress").ToString()
                    m_securityQuestion = .Rows(0)("UserSecurityQuestion").ToString()
                    m_securityAnswer = .Rows(0)("UserSecurityAnswer").ToString()
                    If .Rows(0)("UserIsExternal") IsNot DBNull.Value Then
                        m_isExternal = Convert.ToBoolean(.Rows(0)("UserIsExternal"))
                    End If
                    If .Rows(0)("UserIsLockedOut") IsNot DBNull.Value Then
                        m_isLockedOut = Convert.ToBoolean(.Rows(0)("UserIsLockedOut"))
                    End If
                    If .Rows(0)("UserPasswordChangeDateTime") IsNot DBNull.Value Then
                        m_passwordChangeDateTime = Convert.ToDateTime(.Rows(0)("UserPasswordChangeDateTime"))
                    End If
                    m_accountCreatedDateTime = Convert.ToDateTime(.Rows(0)("UserAccountCreatedDateTime"))
                End With

                With userData.Tables(1)
                    For i As Integer = 0 To .Rows.Count - 1
                        m_groups.Add(New Group(.Rows(i)("GroupName").ToString(), .Rows(i)("GroupDescription").ToString()))
                    Next
                End With

                With userData.Tables(2)
                    For i As Integer = 0 To .Rows.Count - 1
                        Dim application As New Application(.Rows(i)("ApplicationName").ToString(), .Rows(i)("ApplicationDescription").ToString())
                        m_roles.Add(New Role(.Rows(i)("RoleName").ToString(), .Rows(i)("RoleDescription").ToString(), application))

                        ' Since an application can have multiple roles, we're going to add an application to the list
                        ' of application only if it doesn't exist already.
                        If Not m_applications.Contains(application) Then m_applications.Add(application)
                    Next
                End With

                userData.Dispose()
            End If

        End Sub

        ''' <summary>
        ''' Authenticates user's credentials.
        ''' </summary>
        ''' <param name="password">Password to be authenticated.</param>
        ''' <returns>True if authentication was successful; otherwise False.</returns>
        Public Function Authenticate(ByVal password As String) As Boolean

            ' We will not authenticate if user account doesn't exist or is locked.
            If Not m_isDefined OrElse m_isLockedOut Then Exit Function

            ' Authenticate based on the specified authentication mode.
            Select Case m_authenticationMode
                Case AuthenticationMode.AD
                    If m_isExternal Then
                        ' User is external according to the security database.
                        If Not String.IsNullOrEmpty(password) Then
                            ' We'll validate the password against the security database.
                            m_isAuthenticated = (EncryptPassword(password) = m_password)
                        End If
                    Else
                        ' User is internal according to the security database.
                        If Not String.IsNullOrEmpty(password) Then
                            ' We'll validate the password against the Active Directory.
                            m_isAuthenticated = New UserInfo(m_username, "TVA", True).Authenticate(password)
                        Else
                            ' The user to be authenticated is defined as an internal user in the security database, 
                            ' but we don't have a password to authenticate against the Active Directory. ' In this 
                            ' case the authentication requirement becomes that the user we're authenticating must 
                            ' be user executing the request (i.e. accessing secure the app).
                            Dim loginID As String = System.Threading.Thread.CurrentPrincipal.Identity.Name
                            If Not String.IsNullOrEmpty(loginID) Then
                                m_isAuthenticated = (String.Compare(m_username, loginID.Split("\"c)(1), True) = 0)
                            End If
                        End If
                    End If
                Case AuthenticationMode.RSA
                    Dim client As RadiusClient = Nothing
                    Dim response As RadiusPacket = Nothing
                    Try
                        ' We first try to authenticate against the primary RSA server.
                        client = New RadiusClient(RsaServer1, Decrypt(RadiusSharedSecret))
                        response = client.Authenticate(m_username, password)

                        If response Is Nothing Then
                            ' We didn't get a response back from the primary RSA server. This is most likely
                            ' to happen when the primary server is unavailable, so we attempt to authenticate
                            ' against the backup RSA server.
                            client.Dispose()
                            client = New RadiusClient(RsaServer2, Decrypt(RadiusSharedSecret))
                            response = client.Authenticate(m_username, password)
                        End If

                        If response IsNot Nothing Then
                            ' We received a response back from the RSA server.
                            Select Case response.Type
                                Case PacketType.AccessAccept
                                    ' Credentials were accepted by the RSA server.
                                    m_isAuthenticated = True
                                Case PacketType.AccessChallenge
                                    ' RSA server challenged our authentication request.
                                    If client.IsUserInNewPinMode(response) Then
                                        ' If the user's account is in the "new pin" mode, we treat it as if it's 
                                        ' time for the user to change the password so appropriate input form is 
                                        ' served to the user.
                                        m_passwordChangeDateTime = Date.Now
                                    ElseIf client.IsUserInNextTokenMode(response) Then
                                        ' If the user's account is in the "next token" mode, we treat the account 
                                        ' as if it is disabled so the user must either email or call-in to get the
                                        ' account enabled.
                                        m_isLockedOut = True
                                    End If
                            End Select
                        End If
                    Catch ex As Exception
                        Throw
                    Finally
                        If client IsNot Nothing Then client.Dispose()
                    End Try
            End Select

            ' Log successful or unsuccessful authentication result to the security database so that a user account
            ' gets locked-out automatically after a set number of unsuccessful login attempts.
            Using dbConnection As SqlConnection = GetDatabaseConnection()
                If dbConnection.State <> ConnectionState.Open Then dbConnection.Open()
                ExecuteNonQuery("dbo.LogLogin", dbConnection, m_username, Not m_isAuthenticated)
            End Using

            Return m_isAuthenticated

        End Function

        ''' <summary>
        ''' Changes the user's current password under AD authentication mode if the user is external, or creates 
        ''' a new pin (if account is in "new pin" mode) under RSA authentication mode.
        ''' </summary>
        ''' <param name="oldPassword">
        ''' Current password under AD authentication. Current token under RSA authntication.</param>
        ''' <param name="newPassword">
        ''' New password under AD authentication; new pin under RSA authentication.
        ''' </param>
        ''' <returns>
        ''' True if password was changed successfully under AD authentication mode, or new pin was created 
        ''' successfully under RSA authentication mode; otherwise False.
        ''' </returns>
        Public Function ChangePassword(ByVal oldPassword As String, ByVal newPassword As String) As Boolean

            ' Don't proceed to change password or create pin if user account is not defined or is locked.
            If Not m_isDefined OrElse m_isLockedOut Then Exit Function

            Select Case m_authenticationMode
                Case AuthenticationMode.AD
                    ' Under AD authentication mode, we allow the password to be changed only if the user is
                    ' an external user and after the user's current password has been verified.
                    If m_isExternal AndAlso Authenticate(oldPassword) Then
                        Using dbConnection As SqlConnection = GetDatabaseConnection()
                            ExecuteScalar("dbo.ChangePassword", dbConnection, m_username, m_password, EncryptPassword(newPassword))
                        End Using

                        Return True
                    End If
                Case AuthenticationMode.RSA
                    ' Under RSA authentication mode, in order to create a pin, the account must actually be in
                    ' the "new pin" mode. If it is not, then repeated attempts to create a pin may result in the
                    ' account being placed in "next token" mode (i.e. equivalent to account being disabled). And
                    ' the problem is that we cannot verify if a account is actually in the "new pin" mode, because 
                    ' a token can be used only once and no more than once.
                    Dim client As RadiusClient = Nothing
                    Try
                        ' First, we try creating a pin against the primary RSA server.
                        client = New RadiusClient(RsaServer1, Decrypt(RadiusSharedSecret))

                        Return client.CreateNewPin(m_username, oldPassword, newPassword)
                    Catch ex1 As ArgumentNullException
                        ' When we encounter this exception, the primary RSA server didn't respond.
                        Try
                            ' Next, we try creating a pin against the backup RSA server.
                            client.Dispose()
                            client = New RadiusClient(RsaServer2, Decrypt(RadiusSharedSecret))

                            Return client.CreateNewPin(m_username, oldPassword, newPassword)
                        Catch ex2 As Exception
                            ' Absorb any other exception.
                        End Try
                    Catch ex1 As Exception
                        ' Absorb any other exception.
                    Finally
                        If client IsNot Nothing Then client.Dispose()
                    End Try
            End Select

        End Function

        ''' <summary>
        ''' Logs successful or unsuccessful attempt of accessing a secure application.
        ''' </summary>
        ''' <param name="accessDenied">True if user does not have access to the application; otherwise False.</param>
        ''' <returns>True if logging was successful; otherwise False.</returns>
        Public Function LogAccess(ByVal accessDenied As Boolean) As Boolean

            ' We will not log access attempt if user is not defined in the security database.
            If Not m_isDefined Then Exit Function

            If Not String.IsNullOrEmpty(m_applicationName) Then
                ' In order to log an access attempt to the security data, we must have the name of the application
                ' user was trying to access. If we don't have that then we cannot log access attempt to the database.
                Using dbConnection As SqlConnection = GetDatabaseConnection()
                    ExecuteNonQuery("dbo.LogAccess", dbConnection, m_username, m_applicationName, accessDenied)
                End Using

                Return True
            Else
                Throw New InvalidOperationException("Application name is not set.")
            End If

        End Function

        ''' <summary>
        ''' Logs information about an encountered exception to the security database.
        ''' </summary>
        ''' <param name="source">Source of the exception.</param>
        ''' <param name="message">Detailed description of the exception.</param>
        ''' <returns>True if information was logged successfully; otherwise False.</returns>
        Public Function LogError(ByVal source As String, ByVal message As String) As Boolean

            Try
                Using dbConnection As SqlConnection = GetDatabaseConnection()
                    ExecuteScalar("dbo.LogError", dbConnection, m_applicationName, source, message)
                End Using

                Return True
            Catch ex As Exception
                ' Absorb any exception we might encounter.
            End Try

        End Function

        ''' <summary>
        ''' Finds the specified group.
        ''' </summary>
        ''' <param name="groupName">Name of the group to be found.</param>
        ''' <returns>Group if one is found; otherwise Nothing.</returns>
        Public Function FindGroup(ByVal groupName As String) As Group

            If m_groups IsNot Nothing Then
                For i As Integer = 0 To m_groups.Count - 1
                    If String.Compare(m_groups(i).Name, groupName, True) = 0 Then
                        ' User does belong to the specified group.
                        Return m_groups(i)
                    End If
                Next
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Finds the specified role.
        ''' </summary>
        ''' <param name="roleName">Name of the role to be found.</param>
        ''' <returns>Role if one is found; otherwise Nothing.</returns>
        Public Function FindRole(ByVal roleName As String) As Role

            If m_roles IsNot Nothing Then
                For i As Integer = 0 To m_roles.Count - 1
                    If String.Compare(m_roles(i).Name, roleName, True) = 0 Then
                        ' User is in the specified role.
                        Return m_roles(i)
                    End If
                Next
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Finds the specified role.
        ''' </summary>
        ''' <param name="roleName">Name of the role to be found.</param>
        ''' <param name="applicationName">Name of the application to which the role belongs.</param>
        ''' <returns>Role if one is found; otherwise Nothing.</returns>
        Public Function FindRole(ByVal roleName As String, ByVal applicationName As String) As Role

            Dim role As Role = FindRole(roleName)
            If role IsNot Nothing AndAlso String.Compare(role.Application.Name, applicationName, True) = 0 Then
                ' User is in the specified role and the specified role belongs to the specified application.
                Return role
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Finds the specified application.
        ''' </summary>
        ''' <param name="applicationName">Name of the application to be found.</param>
        ''' <returns>Application if one is found; otherwise Nothing.</returns>
        Public Function FindApplication(ByVal applicationName As String) As Application

            If m_applications IsNot Nothing Then
                For i As Integer = 0 To m_applications.Count - 1
                    If String.Compare(m_applications(i).Name, applicationName, True) = 0 Then
                        ' User has access to the specified application.
                        Return m_applications(i)
                    End If
                Next
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Returns users roles for an application.
        ''' </summary>
        ''' <param name="applicationName">Application Name</param>
        ''' <returns>List of roles for specified application</returns>
        ''' <remarks></remarks>
        <Obsolete("Use the Roles property that takes an application name as a parameter instead. This function will be removed in a future release.", True)> _
        Public Function FindApplicationRoles(ByVal applicationName As String) As List(Of Role)

            '**** Added By Mehul
            Dim applicationRoles As New List(Of Role)()

            If m_roles IsNot Nothing Then
                For i As Integer = 0 To m_roles.Count - 1
                    If String.Compare(m_roles(i).Application.Name, applicationName, True) = 0 Then
                        applicationRoles.Add(m_roles(i))
                    End If
                Next
            End If

            Return applicationRoles

        End Function

#Region " Shared "

        ''' <summary>
        ''' Generates a random password of specified length with at least one uppercase character, one lowercase
        ''' character and one digit.
        ''' </summary>
        ''' <param name="length">Length of the random password.</param>
        ''' <returns>Random password of the specified lenght.</returns>
        Public Shared Function GeneratePassword(ByVal length As Integer) As String

            If length >= MinimumPasswordLength Then
                Dim password As Char() = CreateArray(Of Char)(length)

                ' ASCII character ranges:
                ' Digits - 48 to 57
                ' Upper case - 65 to 90
                ' Lower case - 97 to 122

                ' Out of the minimum of 8 characters in the password, we'll make sure that the password contains
                ' at least 2 digits and 2 upper case letter, so that the password meets the strong password rule.
                Dim digits As Integer = 0
                Dim upperCase As Integer = 0
                Dim minSpecialChars As Integer = 2
                For i As Integer = 0 To password.Length - 1
                    If digits < minSpecialChars Then
                        password(i) = Chr(CInt(RandomBetween(48, 57)))
                        digits += 1
                    ElseIf upperCase < minSpecialChars Then
                        password(i) = Chr(CInt(RandomBetween(65, 90)))
                        upperCase += 1
                    Else
                        password(i) = Chr(CInt(RandomBetween(97, 122)))
                    End If
                Next

                ' We have a random password that meets the strong password rule, now we'll shuffle it to make it 
                ' even more random.
                Dim temp As Char
                Dim swapIndex As Integer
                For i As Integer = 0 To password.Length - 1
                    swapIndex = CInt(RandomBetween(0, password.Length - 1))
                    temp = password(swapIndex)
                    password(swapIndex) = password(i)
                    password(i) = temp
                Next

                Return New String(password)
            Else
                Throw New ArgumentException(String.Format("Password length should be at least {0} characters.", MinimumPasswordLength))
            End If

        End Function

        ''' <summary>
        ''' Encrypts the password to a one-way hash using the SHA1 hash algorithm.
        ''' </summary>
        ''' <param name="password">Password to be encrypted.</param>
        ''' <returns>Encrypted password.</returns>
        Public Shared Function EncryptPassword(ByVal password As String) As String

            If Regex.IsMatch(password, StrongPasswordRegex) Then
                ' We prepend salt text to the password and then has it to make it even more secure.
                Return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("O3990\P78f9E66b:a35_V©6M13©6~2&[" & password, "SHA1")
            Else
                ' Password does not meet the strong password rule defined below, so we don't encrypt the password.
                With New StringBuilder()
                    .Append("Password does not meet the following criteria:")
                    .AppendLine()
                    .Append("- Password must be at least 8 characters")
                    .AppendLine()
                    .Append("- Password must contain at least 1 digit")
                    .AppendLine()
                    .Append("- Password must contain at least 1 upper case letter")
                    .AppendLine()
                    .Append("- Password must contain at least 1 lower case letter")

                    Throw New InvalidOperationException(.ToString())
                End With
            End If

        End Function

#End Region

#End Region

#Region " Code Scope: Friend "

        ''' <summary>
        ''' Creates an instance of a user defined in the security database.
        ''' </summary>
        ''' <param name="username">Username of the user.</param>
        ''' <param name="password">Password of the user.</param>
        ''' <param name="applicationName">Name of the application for which user data is to be retrieved.</param>
        ''' <param name="securityServer">Security server from which user data is to be retrieved.</param>
        ''' <param name="authenticationMode">Mode of authentication to be used for authenticating credentials.</param>
        ''' <param name="authenticate">True if user credentials are to be authenticated; otherwise False.</param>
        ''' <remarks>
        ''' This constructor is only to be used internally by the security provider control and its sub-components.
        ''' </remarks>
        Friend Sub New(ByVal username As String, ByVal password As String, ByVal applicationName As String, _
                       ByVal securityServer As SecurityServer, ByVal authenticationMode As AuthenticationMode, ByVal authenticate As Boolean)

            m_username = username
            m_password = password
            m_applicationName = applicationName
            m_securityServer = securityServer
            m_authenticationMode = authenticationMode
            m_groups = New List(Of Group)()
            m_roles = New List(Of Role)()
            m_applications = New List(Of Application)()

            Me.RefreshData()                ' Retrieve user data.
            If authenticate Then
                Me.Authenticate(password)   ' Authenticate user crendentials.
            Else
                m_isAuthenticated = True    ' Pretend user credentials are authenticated.
            End If

        End Sub

#End Region

#Region " Code Scope: Private "

        Private Function GetDatabaseConnection() As SqlConnection

            Dim connection As SqlConnection = Nothing
            Try
                Select Case m_securityServer
                    Case SecurityServer.Development
                        connection = New SqlConnection(Decrypt(DevConnectionString))
                    Case SecurityServer.Acceptance
                        connection = New SqlConnection(Decrypt(AcpConnectionString))
                    Case SecurityServer.Production
                        connection = New SqlConnection(Decrypt(PrdConnectionString))
                End Select

                connection.Open()
            Catch ex As SqlException
                ' Failed to open the connection, so we'll use the backup.
                connection = New SqlConnection(Decrypt(BakConnectionString))
            Catch ex As Exception
                Throw
            End Try

            Return connection

        End Function

#End Region

    End Class

End Namespace