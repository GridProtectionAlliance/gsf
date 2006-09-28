'*******************************************************************************************************
'  Tva.Identity.UserInfo.vb - ActiveDirectory User Information Class
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
'
'*******************************************************************************************************

Imports System.DirectoryServices

Namespace Identity

    Public Class UserInfo

        Private m_loginID As String
        Private m_domain As String
        Private m_username As String
        Private m_userEntry As DirectoryEntry

        ''' <summary>Initializes a new instance of the user information class.</summary>
        ''' <remarks>Specify login information as domain\username.</remarks>
        Public Sub New(ByVal loginID As String)

            Dim loginIDParts As String() = loginID.Split("\"c)
            If loginIDParts.Length = 2 Then
                m_domain = loginIDParts(0)
                m_username = loginIDParts(1)
            End If
            m_loginID = loginID

        End Sub

        ''' <summary>Initializes a new instance of the user information class.</summary>
        Public Sub New(ByVal username As String, ByVal domain As String)

            MyClass.New(domain & "\" & username)

        End Sub

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
                    Try
                        Dim entry As New DirectoryEntry("LDAP://" & m_domain)

                        With New DirectorySearcher(entry)
                            .Filter = "(SAMAccountName=" & m_username & ")"
                            m_userEntry = .FindOne().GetDirectoryEntry()
                        End With
                    Catch
                        m_userEntry = Nothing
                    End Try
                End If

                Return m_userEntry
            End Get
        End Property

        ''' <summary>Returns adctive directory value for specified property</summary>
        Public ReadOnly Property UserProperty(ByVal propertyName As System.String) As String
            Get
                Try
                    Return UserEntry.Properties(propertyName)(0).ToString().Replace("  ", " ").Trim()
                Catch ex As Exception
                    Return ""
                End Try
            End Get
        End Property

        ''' <summary>Gets the full name of the user</summary>
        Public ReadOnly Property FullName() As String
            Get
                Dim displayName As String = UserProperty("displayName")

                If Len(displayName) > 0 Then
                    Dim commaPos As Integer = displayName.IndexOf(","c)

                    If commaPos > -1 Then
                        Return displayName.Substring(commaPos + 1).Trim() & " " & displayName.Substring(0, commaPos).Trim()
                    Else
                        Return displayName
                    End If
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
                Dim userEntry As New DirectoryEntry("LDAP://" & m_domain, m_username, password)
                Return New DirectorySearcher(userEntry).FindOne() IsNot Nothing
            Catch ex As Exception
                ' Failed to authenticate against the active directory with the specified password.
            End Try
            Return False

        End Function

    End Class

End Namespace
