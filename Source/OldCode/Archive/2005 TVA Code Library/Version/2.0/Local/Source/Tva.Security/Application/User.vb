' 09-26-06

Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Principal
Imports Tva.Data.Common
Imports Tva.Identity

Namespace Application

    <Serializable()> _
    Public Class User

        Private m_username As String
        Private m_password As String
        Private m_firstName As String
        Private m_lastName As String
        Private m_companyName As String
        Private m_phoneNumber As String
        Private m_emailAddress As String
        Private m_isExternal As Boolean
        Private m_isLockedOut As Boolean
        Private m_passwordChangeDateTime As System.DateTime
        Private m_joinedDateTime As System.DateTime
        Private m_isAuthenticated As Boolean
        Private m_exists As Boolean
        Private m_groups As List(Of Group)
        Private m_roles As List(Of Role)
        Private m_applications As List(Of Application)

        Public Sub New(ByVal username As String, ByVal dbConnection As SqlConnection)

            MyClass.New(username, Nothing, dbConnection)

        End Sub

        Public Sub New(ByVal username As String, ByVal password As String, ByVal dbConnection As SqlConnection)

            MyBase.New()
            If dbConnection IsNot Nothing Then
                If dbConnection.State <> System.Data.ConnectionState.Open Then dbConnection.Open()
                Dim sql As String = "SELECT * FROM dbo.UsersAndCompaniesAndSecurityQuestions WHERE UserName = '" & username & "'"
                Dim userData As DataTable = RetrieveData(sql, dbConnection)
                If userData IsNot Nothing AndAlso userData.Rows.Count > 0 Then
                    ' User does exist in the security database.
                    m_exists = True
                    m_username = userData.Rows(0)("UserName").ToString()
                    m_password = userData.Rows(0)("UserPassword").ToString()
                    m_firstName = userData.Rows(0)("UserFirstName").ToString()
                    m_lastName = userData.Rows(0)("UserLastName").ToString()
                    m_companyName = userData.Rows(0)("UserCompanyName").ToString()
                    m_phoneNumber = userData.Rows(0)("UserPhoneNumber").ToString()
                    m_emailAddress = userData.Rows(0)("UserEmailAddress").ToString()
                    If userData.Rows(0)("UserIsExternal") IsNot DBNull.Value Then
                        m_isExternal = Convert.ToBoolean(userData.Rows(0)("UserIsExternal"))
                    End If
                    If userData.Rows(0)("UserIsLockedOut") IsNot DBNull.Value Then
                        m_isLockedOut = Convert.ToBoolean(userData.Rows(0)("UserIsLockedOut"))
                    End If
                    If userData.Rows(0)("UserPasswordChangeDateTime") IsNot DBNull.Value Then
                        m_passwordChangeDateTime = Convert.ToDateTime(userData.Rows(0)("UserPasswordChangeDateTime"))
                    End If
                    m_joinedDateTime = Convert.ToDateTime(userData.Rows(0)("UserJoinedDateTime"))

                    PopulateGroups(dbConnection)
                    PopulateApplicationsAndRoles(dbConnection)

                    If m_isExternal Then
                        ' User is external according to the security database.
                        If password IsNot Nothing Then
                            ' We'll validate the password against the security database.
                            m_isAuthenticated = (EncryptPassword(password) = m_password)
                        End If
                    Else
                        ' User is internal according to the security database.
                        If password IsNot Nothing Then
                            ' We'll validate the password against the active directory.
                            m_isAuthenticated = New UserInfo(m_username, "TVA", True).Authenticate(password)
                        Else
                            ' When an internal user is found in the security database, he/she is considered
                            ' autheticated and we are not required to validate the password unless one is 
                            ' provided to us by the caller.
                            m_isAuthenticated = True
                        End If
                    End If
                End If
            End If

        End Sub

        Public ReadOnly Property Username() As String
            Get
                Return m_username
            End Get
        End Property

        Public ReadOnly Property Password() As String
            Get
                Return m_password
            End Get
        End Property

        Public ReadOnly Property FirstName() As String
            Get
                Return m_firstName
            End Get
        End Property

        Public ReadOnly Property LastName() As String
            Get
                Return m_lastName
            End Get
        End Property

        Public ReadOnly Property CompanyName() As String
            Get
                Return m_companyName
            End Get
        End Property

        Public ReadOnly Property PhoneNumber() As String
            Get
                Return m_phoneNumber
            End Get
        End Property

        Public ReadOnly Property EmailAddress() As String
            Get
                Return m_emailAddress
            End Get
        End Property

        Public ReadOnly Property IsExternal() As Boolean
            Get
                Return m_isExternal
            End Get
        End Property

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
        Public ReadOnly Property JoinedDateTime() As System.DateTime
            Get
                Return m_joinedDateTime
            End Get
        End Property

        Public ReadOnly Property IsAuthenticated() As Boolean
            Get
                Return m_isAuthenticated
            End Get
        End Property

        Public ReadOnly Property Exists() As Boolean
            Get
                Return m_exists
            End Get
        End Property

        Public ReadOnly Property Groups() As List(Of Group)
            Get
                Return m_groups
            End Get
        End Property

        Public ReadOnly Property Roles() As List(Of Role)
            Get
                Return m_roles
            End Get
        End Property

        Public ReadOnly Property Applications() As List(Of Application)
            Get
                Return m_applications
            End Get
        End Property

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

        Public Function FindRole(ByVal roleName As String, ByVal applicationName As String) As Role

            Dim role As Role = FindRole(roleName)
            If role IsNot Nothing AndAlso String.Compare(role.Application.Name, applicationName, True) = 0 Then
                ' User is in the specified role and the specified role belongs to the specified application.
                Return role
            End If
            Return Nothing

        End Function

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

        Public Shared Function EncryptPassword(ByVal password As String) As String

            ' We prepend salt text to the password and then has it to make it even more secure.
            Return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("O3990\P78f9E66b:a35_V©6M13©6~2&[" & password, "SHA1")

        End Function

#Region " Private Methods "

        Private Sub PopulateGroups(ByVal dbConnection As SqlConnection)

            m_groups = New List(Of Group)

            Dim sql As String = "SELECT * FROM dbo.GetUserGroups('" & m_username & "')"
            Dim userGroups As DataTable = RetrieveData(sql, dbConnection)
            If userGroups IsNot Nothing AndAlso userGroups.Rows.Count > 0 Then
                For i As Integer = 0 To userGroups.Rows.Count - 1
                    m_groups.Add(New Group(userGroups.Rows(i)("GroupName").ToString(), userGroups.Rows(i)("GroupDescription").ToString()))
                Next
            End If

        End Sub

        Private Sub PopulateApplicationsAndRoles(ByVal dbConnection As SqlConnection)

            m_roles = New List(Of Role)
            m_applications = New List(Of Application)

            Dim sql As String = "SELECT * FROM dbo.GetUserRoles('" & m_username & "')"
            Dim userRoles As DataTable = RetrieveData(sql, dbConnection)
            If userRoles IsNot Nothing AndAlso userRoles.Rows.Count > 0 Then
                For i As Integer = 0 To userRoles.Rows.Count - 1
                    Dim application As New Application(userRoles.Rows(i)("ApplicationName").ToString(), userRoles.Rows(i)("ApplicationDescription").ToString())
                    m_roles.Add(New Role(userRoles.Rows(i)("RoleName").ToString(), userRoles.Rows(i)("RoleDescription").ToString(), application))
                    m_applications.Add(application)
                Next
            End If

        End Sub

#End Region

    End Class

End Namespace