'*******************************************************************************************************
'  TVA.Identity.UserInfo.vb - ActiveDirectory User Information Class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/10/2004 - J. Ritchie Carroll
'       Original version of source code generated
'  01/03/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity)
'  09/27/2006 - Pinal C. Patel
'       Added Authenticate() function.
'  09/29/2006 - Pinal C. Patel
'       Added support to impersonate previliged user for retrieving user information
'  11/06/2007 - Pinal C. Patel
'       Modified the logic of Authenticate method to use the user's DirectoryEntry instance
'       Modified UserEntry property to impersonate privileged user if configured for the instance
'
'*******************************************************************************************************

Imports System.DirectoryServices
Imports System.Security.Principal

Namespace Identity

    Public Class UserInfo

        Private m_loginID As String
        Private m_domain As String
        Private m_username As String
        Private m_usePreviligedAccount As Boolean
        Private m_userEntry As DirectoryEntry

        Private Const PrevilegedUserName As String = "esocss"
        Private Const PrevilegedUserPassword As String = "pwd4ctrl"

        Public Sub New(ByVal username As String, ByVal domain As String)

            MyClass.New(username, domain, False)

        End Sub

        ''' <summary>Initializes a new instance of the user information class.</summary>
        Public Sub New(ByVal username As String, ByVal domain As String, ByVal usePreviligedAccount As Boolean)

            MyClass.New(domain & "\" & username, usePreviligedAccount)

        End Sub

        Public Sub New(ByVal loginID As String)

            MyClass.New(loginID, False)

        End Sub

        ''' <summary>Initializes a new instance of the user information class.</summary>
        ''' <remarks>Specify login information as domain\username.</remarks>
        Public Sub New(ByVal loginID As String, ByVal usePreviligedAccount As Boolean)

            Dim loginIDParts As String() = loginID.Split("\"c)
            If loginIDParts.Length = 2 Then
                m_domain = loginIDParts(0)
                m_username = loginIDParts(1)
            End If
            m_loginID = loginID
            m_usePreviligedAccount = usePreviligedAccount

        End Sub

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether a previliged account will be used for retrieving
        ''' information about the user from the Active Directory.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if previliged account is to be used; otherwise False.</returns>
        Public Property UsePreviligedAccount() As Boolean
            Get
                Return m_usePreviligedAccount
            End Get
            Set(ByVal value As Boolean)
                m_usePreviligedAccount = value
            End Set
        End Property

        ''' <summary>Gets the login ID of the user.</summary>
        Public ReadOnly Property LoginID() As String
            Get
                Return m_loginID
            End Get
        End Property

        ''' <summary>Gets the System.DirectoryServices.DirectoryEntry of the user</summary>
        Public ReadOnly Property UserEntry() As DirectoryEntry
            Get
                If m_userEntry Is Nothing Then
                    Dim currentContext As WindowsImpersonationContext = Nothing
                    Try
                        ' 11/06/2007 - PCP: Some change in the AD now causes the searching the AD to fail also if 
                        ' this code is not being executed under a domain account which was not the case before; 
                        ' before only AD property lookup had this behavior.
                        If m_usePreviligedAccount Then
                            ' Impersonate to the previliged account if specified.
                            currentContext = Common.ImpersonateUser(PrevilegedUserName, PrevilegedUserPassword)
                        End If

                        ' 02/27/2007 - PCP: Using the default directory entry instead of specifying the domain name.
                        ' This is done to overcome "The server is not operational" COM exception that was being 
                        ' encountered when a domain name was being specified.
                        Dim entry As New DirectoryEntry()
                        'Dim entry As New DirectoryEntry("LDAP://" & m_domain)

                        With New DirectorySearcher(entry)
                            .Filter = "(SAMAccountName=" & m_username & ")"
                            m_userEntry = .FindOne().GetDirectoryEntry()
                        End With
                    Catch
                        m_userEntry = Nothing
                        Throw
                    Finally
                        ' Undo impersonation if it was performed.
                        If currentContext IsNot Nothing Then Common.EndImpersonation(currentContext)
                    End Try
                End If

                Return m_userEntry
            End Get
        End Property

        ''' <summary>Returns adctive directory value for specified property</summary>
        Public ReadOnly Property UserProperty(ByVal propertyName As System.String) As String
            Get
                Dim currentContext As WindowsImpersonationContext = Nothing
                Try
                    If m_usePreviligedAccount Then
                        ' Impersonate to the previliged account if specified.
                        currentContext = Common.ImpersonateUser(PrevilegedUserName, PrevilegedUserPassword)
                    End If

                    Return UserEntry.Properties(propertyName)(0).ToString().Replace("  ", " ").Trim()
                Catch ex As Exception
                    Return ""
                Finally
                    ' Undo impersonation if it was performed.
                    If currentContext IsNot Nothing Then Common.EndImpersonation(currentContext)
                End Try
            End Get
        End Property

        Public ReadOnly Property FirstName() As String
            Get
                Return UserProperty("givenName")
            End Get
        End Property

        Public ReadOnly Property LastName() As String
            Get
                Return UserProperty("sn")
            End Get
        End Property

        Public ReadOnly Property MiddleInitial() As String
            Get
                Return UserProperty("initials")
            End Get
        End Property

        ''' <summary>Gets the full name of the user</summary>
        Public ReadOnly Property FullName() As String
            Get
                If Not String.IsNullOrEmpty(FirstName) AndAlso Not String.IsNullOrEmpty(LastName) _
                        AndAlso Not String.IsNullOrEmpty(MiddleInitial) Then
                    Return FirstName & " " & MiddleInitial & " " & LastName
                Else
                    Return m_loginID
                End If
            End Get
        End Property

        ''' <summary>Gets the e-mail address of the user</summary>
        Public ReadOnly Property Email() As String
            Get
                Return UserProperty("mail")
            End Get
        End Property

        ''' <summary>Gets the telephone number of the user</summary>
        Public ReadOnly Property Telephone() As String
            Get
                Return UserProperty("telephoneNumber")
            End Get
        End Property

        ''' <summary>Gets the title of the user</summary>
        Public ReadOnly Property Title() As String
            Get
                Return UserProperty("title")
            End Get
        End Property

        ''' <summary>Gets the company of the user</summary>
        Public ReadOnly Property Company() As String
            Get
                Return UserProperty("company")
            End Get
        End Property

        ''' <summary>Returns the office location of the user</summary>
        Public ReadOnly Property Office() As String
            Get
                Return UserProperty("physicalDeliveryOfficeName")
            End Get
        End Property

        ''' <summary>Gets the department name where the user works</summary>
        Public ReadOnly Property Department() As String
            Get
                Return UserProperty("department")
            End Get
        End Property

        ''' <summary>Gets the city where the user works</summary>
        Public ReadOnly Property City() As String
            Get
                Return UserProperty("l")
            End Get
        End Property

        ''' <summary>Returns the mailbox of where the user works</summary>
        Public ReadOnly Property Mailbox() As String
            Get
                Return UserProperty("streetAddress")
            End Get
        End Property

        ''' <summary>
        ''' Authenticates the user against Active Directory with the specified password.
        ''' </summary>
        ''' <param name="password">The password to be used for authentication.</param>
        ''' <returns>True is the user can be authenticated; otherwise False.</returns>
        Public Function Authenticate(ByVal password As String) As Boolean

            Try
                Dim lookupResult As String

                ' Set the credentials to use for looking up AD info.
                UserEntry.Username = m_username
                UserEntry.Password = password

                ' We'll lookup a AD property which will fail if the credentials are incorrect.
                lookupResult = UserProperty("displayName")

                ' Remove the username and password we used for authentication.
                UserEntry.Username = Nothing
                UserEntry.Password = Nothing

                ' AD property value will be null string if the AD property lookup failed.
                Return Not String.IsNullOrEmpty(lookupResult)
            Catch ex As Exception
                ' The one exception we might get is when we're getting the user's AD entry.
            End Try

        End Function

    End Class

End Namespace
