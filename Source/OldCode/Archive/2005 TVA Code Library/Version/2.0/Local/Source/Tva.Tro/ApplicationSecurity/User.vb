' 09-26-06

Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Principal
Imports Tva.Data.Common
Imports Tva.Identity.Common

Namespace ApplicationSecurity

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
        Private m_isAuthenticated As Boolean
        Private m_exists As Boolean
        Private m_roles As List(Of Role)
        Private m_applications As List(Of Application)

        Private Const CryptoKey As String = "c7d8f9d6-bfff-4a74-bcf6-64ea7c3e3d7a"

        Public Sub New(ByVal username As String, ByVal dbConnection As SqlConnection)
            MyClass.New(username, "", dbConnection)
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
                    m_isExternal = Convert.ToBoolean(userData.Rows(0)("UserIsExternal"))
                    m_isLockedOut = Convert.ToBoolean(userData.Rows(0)("UserIsLockedOut"))
                    If m_isExternal AndAlso Not String.IsNullOrEmpty(m_password) Then
                        ' User is external according to the security database.
                        If m_password = EncryptPassword(password) Then
                            ' User's credentials are valid.
                            m_firstName = userData.Rows(0)("UserFirstName").ToString()
                            m_lastName = userData.Rows(0)("UserLastName").ToString()
                            m_companyName = userData.Rows(0)("UserCompanyName").ToString()
                            m_phoneNumber = userData.Rows(0)("UserPhoneNumber").ToString()
                            m_emailAddress = userData.Rows(0)("UserEmailAddress").ToString()
                            PopulateApplicationsAndRoles(dbConnection)
                            m_isAuthenticated = True
                        End If
                    Else
                        ' User is internal according to the security database.
                        Dim userInfo As Tva.Identity.UserInfo = New Tva.Identity.UserInfo(m_username, "TVA", True)
                        Try
                            If Not String.IsNullOrEmpty(password) AndAlso Not userInfo.Authenticate(password) Then
                                ' User is internal but has provided with a password, but authentication failed.
                                Throw New Exception()
                            End If
                            If userInfo.FullName <> userInfo.LoginID Then
                                m_firstName = userInfo.FirstName
                                m_lastName = userInfo.LastName
                                m_companyName = userInfo.Company
                                m_phoneNumber = userInfo.Telephone
                                m_emailAddress = userInfo.Email
                                PopulateApplicationsAndRoles(dbConnection)
                                m_isAuthenticated = True
                            End If
                        Catch ex As Exception

                        End Try
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

        Public Function FindRole(ByVal roleName As String) As Role

            If m_roles IsNot Nothing Then
                For i As Integer = 0 To m_roles.Count - 1
                    If m_roles(i).Name = roleName.ToUpper() Then Return m_roles(i)
                Next
            End If
            Return Nothing

        End Function

        Public Function FindApplication(applicationName as String) As Application

            If m_applications IsNot Nothing Then
                For i As Integer = 0 To m_applications.Count - 1
                    If m_applications(i).Name = applicationName.ToUpper() Then Return m_applications(i)
                Next
            End If
            Return Nothing

        End Function

        Public Shared Function EncryptPassword(ByVal password As String) As String

            Return Tva.Security.Cryptography.Common.Encrypt(password, CryptoKey, Security.Cryptography.EncryptLevel.Level4)

        End Function

#Region " Private Methods "

        Private Sub PopulateApplicationsAndRoles(ByVal dbConnection As SqlConnection)

            Dim sql As String = "SELECT * FROM dbo.GetUserRoles('" & m_username & "')"
            Dim userRoles As DataTable = RetrieveData(sql, dbConnection)
            If userRoles IsNot Nothing AndAlso userRoles.Rows.Count > 0 Then
                m_roles = New List(Of Role)
                m_applications = New List(Of Application)
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