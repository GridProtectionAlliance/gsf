'*******************************************************************************************************
'  Tva.Identity.Common.vb - Common User Identity Functions
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
'  ??/??/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/03/2006 - Pinal C. Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity)
'
'*******************************************************************************************************

Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.DirectoryServices
Imports Tva.Interop.WindowsApi

Namespace Identity

    Public NotInheritable Class Common

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

        ''' <summary>Gets the current user's information</summary>
        Public Shared ReadOnly Property CurrentUser() As UserInfo
            Get
                If m_currentUserInfo Is Nothing Then m_currentUserInfo = New UserInfo(CurrentUserID)
                Return m_currentUserInfo
            End Get
        End Property

        ''' <summary>Gets the current user's NT ID.</summary>
        Public Shared ReadOnly Property CurrentUserID() As String
            Get
                Return WindowsIdentity.GetCurrent.Name
            End Get
        End Property

        ''' <summary>Validates NT authentication given the specified credentials</summary>
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

        ''' <summary>Impersonates the specified user</summary>
        ''' <param name="username">To be provided.</param>
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

        ''' <summary>Ends impersonation of the specified user</summary>
        Public Shared Sub EndImpersonation(ByVal impersonatedUser As WindowsImpersonationContext)

            If Not impersonatedUser Is Nothing Then impersonatedUser.Undo()
            impersonatedUser = Nothing

        End Sub

    End Class

End Namespace
