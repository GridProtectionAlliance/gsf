'*******************************************************************************************************
'  Tva.Identity.vb - Common User Identity Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  ??/??/2003 - James R Carroll
'       Original version of source code generated
'  01/03/2006 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity)
'
'*******************************************************************************************************

Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.DirectoryServices
Imports Tva.Interop.Windows.Common

Public NotInheritable Class Identity

    Private Declare Auto Function LogonUser Lib "advapi32.dll" (ByVal lpszUsername As String, ByVal lpszDomain As String, ByVal lpszPassword As String, ByVal dwLogonType As Integer, ByVal dwLogonProvider As Integer, ByRef phToken As IntPtr) As Boolean
    Private Declare Auto Function CloseHandle Lib "kernel32.dll" (ByVal handle As IntPtr) As Boolean
    Private Declare Auto Function DuplicateToken Lib "advapi32.dll" (ByVal ExistingTokenHandle As IntPtr, ByVal SecurityImpersonationLevel As Integer, ByRef DuplicateTokenHandle As IntPtr) As Boolean

    Private Const LOGON32_PROVIDER_DEFAULT As Integer = 0
    Private Const LOGON32_LOGON_INTERACTIVE As Integer = 2
    Private Const LOGON32_LOGON_NETWORK As Integer = 3
    Private Const SECURITY_IMPERSONATION As Integer = 2

    Private Shared m_currentUserInfo As UserInfo

    Private Sub New()
        ' This class contains only global functions and is not meant to be instantiated
    End Sub

    ''' <summary>
    ''' Gets the current user's information.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current user's information.</returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property CurrentUser() As UserInfo
        Get
            If m_currentUserInfo Is Nothing Then m_currentUserInfo = New UserInfo(CurrentUserID)
            Return m_currentUserInfo
        End Get
    End Property

    ''' <summary>
    ''' Gets the current user's NT ID.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The current user's NT ID.</returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property CurrentUserID() As String
        Get
            Return WindowsIdentity.GetCurrent.Name
        End Get
    End Property

    ''' <summary>
    ''' Authenticates a user.
    ''' </summary>
    ''' <param name="username">To be provided.</param>
    ''' <param name="password">To be provided.</param>
    ''' <param name="domain">To be provided.</param>
    ''' <returns>To be provided.</returns>
    ''' <remarks></remarks>
    Public Shared Function AuthenticateUser(ByVal username As String, ByVal password As String, ByVal domain As String) As Boolean

        Dim tokenHandle As IntPtr = IntPtr.Zero
        Dim authenticated As Boolean

        Try
            ' Call LogonUser to attempt authentication
            authenticated = LogonUser(username, domain, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, tokenHandle)
        Catch
            ' We rethrow any exceptions back to user, we are just using try/catch so we can clean up in finally
            Throw
        Finally
            ' Free the token
            If Not IntPtr.op_Equality(tokenHandle, IntPtr.Zero) Then CloseHandle(tokenHandle)
        End Try

        Return authenticated

    End Function

    ''' <summary>
    ''' Impersonates the specified user.
    ''' </summary>
    ''' <param name="username">To be provided.</param>
    ''' <param name="password">To be provided.</param>
    ''' <param name="domain">To be provided.</param>
    ''' <returns>To be provided.</returns>
    ''' <remarks></remarks>
    Public Shared Function ImpersonateUser(ByVal username As String, ByVal password As String, ByVal domain As String) As WindowsImpersonationContext

        Dim impersonatedUser As WindowsImpersonationContext
        Dim tokenHandle As IntPtr = IntPtr.Zero
        Dim dupeTokenHandle As IntPtr = IntPtr.Zero

        Try
            ' Call LogonUser to obtain a handle to an access token.
            If Not LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, tokenHandle) Then
                Throw New InvalidOperationException("Failed to impersonate user " & domain & "\" & username & ".  " & GetErrorMessage(Marshal.GetLastWin32Error()))
            End If

            If Not DuplicateToken(tokenHandle, SECURITY_IMPERSONATION, dupeTokenHandle) Then
                CloseHandle(tokenHandle)
                Throw New InvalidOperationException("Failed to impersonate user " & domain & "\" & username & ".  Exception thrown while trying to duplicate token.")
            End If

            ' The token that is passed into WindowsIdentity must be a primary token in order to use it for impersonation
            impersonatedUser = WindowsIdentity.Impersonate(dupeTokenHandle)
        Catch
            ' We rethrow any exceptions back to user, we are just using try/catch so we can clean up in finally
            Throw
        Finally
            ' Free the tokens
            If Not IntPtr.op_Equality(tokenHandle, IntPtr.Zero) Then CloseHandle(tokenHandle)
            If Not IntPtr.op_Equality(dupeTokenHandle, IntPtr.Zero) Then CloseHandle(dupeTokenHandle)
        End Try

        Return impersonatedUser

    End Function

    ''' <summary>
    ''' Ends impersonation of the specified user.
    ''' </summary>
    ''' <param name="impersonatedUser">To be provided.</param>
    ''' <remarks></remarks>
    Public Shared Sub EndImpersonation(ByVal impersonatedUser As WindowsImpersonationContext)

        If Not impersonatedUser Is Nothing Then impersonatedUser.Undo()
        impersonatedUser = Nothing

    End Sub

    Public Class UserInfo

        Private m_userDirectoryEntry As DirectoryEntry
        Private m_userLoginID As String

        ''' <summary>
        ''' Initializes a new instance of the user information class.
        ''' </summary>
        ''' <param name="loginID">To be provided.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal loginID As String)

            Me.m_userLoginID = loginID

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the user information class.
        ''' </summary>
        ''' <param name="username">To be provided.</param>
        ''' <param name="domain">To be provided.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal username As String, ByVal domain As String)

            Me.New(domain & "\" & username)

        End Sub

        ''' <summary>
        ''' Gets the login ID of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LoginID() As String
            Get
                Return m_userLoginID
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.DirectoryServices.DirectoryEntry of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UserEntry() As DirectoryEntry
            Get
                If m_userDirectoryEntry Is Nothing Then
                    Try
                        Dim domain As String = m_userLoginID.Substring(0, m_userLoginID.IndexOf("\"c)).Trim()
                        Dim userName As String = m_userLoginID.Replace(domain & "\", "").Trim()
                        Dim entry As New DirectoryEntry("LDAP://" & domain)

                        With New DirectorySearcher(entry)
                            .Filter = "(SAMAccountName=" & userName & ")"
                            m_userDirectoryEntry = .FindOne().GetDirectoryEntry()
                        End With
                    Catch
                        m_userDirectoryEntry = Nothing
                    End Try
                End If

                Return m_userDirectoryEntry
            End Get
        End Property

        ''' <summary>
        ''' To be provided.
        ''' </summary>
        ''' <param name="propertyName">To be provided.</param>
        ''' <value></value>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UserProperty(ByVal propertyName As System.String) As String
            Get
                Try
                    Return UserEntry.Properties(PropertyName)(0).ToString().Replace("  ", " ").Trim()
                Catch ex As Exception
                    Return ""
                End Try
            End Get
        End Property

        ''' <summary>
        ''' Gets the full name of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The full name of the user.</returns>
        ''' <remarks></remarks>
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
                    Return m_userLoginID
                End If

            End Get
        End Property

        ''' <summary>
        ''' Gets the e-mail address of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The e-mail address of the user.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Email() As String
            Get
                Return UserProperty("mail")
            End Get
        End Property

        ''' <summary>
        ''' Gets the telephone number of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The telephone number of the user.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Telephone() As String
            Get
                Return UserProperty("telephoneNumber")
            End Get
        End Property

        ''' <summary>
        ''' Gets the title of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The title of the user.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Title() As String
            Get
                Return UserProperty("title")
            End Get
        End Property

        ''' <summary>
        ''' Gets the company of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The company of the user.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Company() As String
            Get
                Return UserProperty("company")
            End Get
        End Property

        ''' <summary>
        ''' To be provided.
        ''' </summary>
        ''' <value></value>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Office() As String
            Get
                Return UserProperty("physicalDeliveryOfficeName")
            End Get
        End Property

        ''' <summary>
        ''' Gets the department name the user works in.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The department name the user works in.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Department() As String
            Get
                Return UserProperty("department")
            End Get
        End Property

        ''' <summary>
        ''' Gets the city where the user works.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The city where the user works.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property City() As String
            Get
                Return UserProperty("l")
            End Get
        End Property

        ''' <summary>
        ''' To be provided.
        ''' </summary>
        ''' <value></value>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Mailbox() As String
            Get
                Return UserProperty("streetAddress")
            End Get
        End Property

    End Class

End Class
