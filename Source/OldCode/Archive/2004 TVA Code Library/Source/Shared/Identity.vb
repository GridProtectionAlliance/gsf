' James Ritchie Carroll - 2004
Option Explicit On 

Imports System.Runtime.InteropServices
Imports System.Security.Principal
Imports System.DirectoryServices

Namespace [Shared]

    ' Common user identity functions
    Public Class Identity

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
        Private Structure NETRESOURCE
            Public dwScope As Integer
            Public dwType As Integer
            Public dwDisplayType As Integer
            Public dwUsage As Integer
            Public lpLocalName As String
            Public lpRemoteName As String
            Public lpComment As String
            Public lpProvider As String
        End Structure

        Private Declare Auto Function LogonUser Lib "advapi32.dll" (ByVal lpszUsername As String, ByVal lpszDomain As String, ByVal lpszPassword As String, ByVal dwLogonType As Integer, ByVal dwLogonProvider As Integer, ByRef phToken As IntPtr) As Boolean
        Private Declare Auto Function CloseHandle Lib "kernel32.dll" (ByVal handle As IntPtr) As Boolean
        Private Declare Auto Function DuplicateToken Lib "advapi32.dll" (ByVal ExistingTokenHandle As IntPtr, ByVal SecurityImpersonationLevel As Integer, ByRef DuplicateTokenHandle As IntPtr) As Boolean
        Private Declare Auto Function WNetAddConnection2 Lib "mpr.dll" Alias "WNetAddConnection2W" (ByRef lpNetResource As NETRESOURCE, ByVal lpPassword As String, ByVal lpUsername As String, ByVal dwFlags As Integer) As Integer
        Private Declare Auto Function WNetCancelConnection2 Lib "mpr.dll" Alias "WNetCancelConnection2W" (ByVal lpName As String, ByVal dwFlags As Integer, ByVal fForce As Boolean) As Integer

        <DllImport("kernel32.dll")> _
        Private Shared Function FormatMessage(ByVal dwFlags As Integer, ByRef lpSource As IntPtr, ByVal dwMessageId As Integer, ByVal dwLanguageId As Integer, ByRef lpBuffer As String, ByVal nSize As Integer, ByRef Arguments As IntPtr) As Integer
        End Function

        Private Const LOGON32_PROVIDER_DEFAULT As Integer = 0
        Private Const LOGON32_LOGON_INTERACTIVE As Integer = 2
        Private Const LOGON32_LOGON_NETWORK As Integer = 3
        Private Const SECURITY_IMPERSONATION As Integer = 2
        Private Const RESOURCETYPE_DISK As Integer = &H1

        Private Shared currentUserInfo As UserInfo

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Class UserInfo

            Private userDirectoryEntry As DirectoryEntry
            Private userLoginID As String

            Public Sub New(ByVal loginID As String)

                Me.userLoginID = loginID

            End Sub

            Public Sub New(ByVal username As String, ByVal domain As String)

                Me.New(domain & "\" & username)

            End Sub

            Public ReadOnly Property LoginID() As String
                Get
                    Return userLoginID
                End Get
            End Property

            Public ReadOnly Property UserEntry() As DirectoryEntry
                Get
                    If userDirectoryEntry Is Nothing Then
                        Try
                            Dim domain As String = userLoginID.Substring(0, userLoginID.IndexOf("\"c)).Trim()
                            Dim userName As String = userLoginID.Replace(domain & "\", "").Trim()
                            Dim entry As New DirectoryEntry("LDAP://" & domain)

                            With New DirectorySearcher(entry)
                                .Filter = "(SAMAccountName=" & userName & ")"
                                userDirectoryEntry = .FindOne().GetDirectoryEntry()
                            End With
                        Catch
                            userDirectoryEntry = Nothing
                        End Try
                    End If

                    Return userDirectoryEntry
                End Get
            End Property

            Public ReadOnly Property UserProperty(ByVal PropertyName As String) As String
                Get
                    Try
                        Return UserEntry.Properties(PropertyName)(0).ToString().Replace("  ", " ").Trim()
                    Catch ex As Exception
                        Return ""
                    End Try
                End Get
            End Property

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
                        Return userLoginID
                    End If

                End Get
            End Property

            Public ReadOnly Property Email() As String
                Get
                    Return UserProperty("mail")
                End Get
            End Property

            Public ReadOnly Property Telephone() As String
                Get
                    Return UserProperty("telephoneNumber")
                End Get
            End Property

            Public ReadOnly Property Title() As String
                Get
                    Return UserProperty("title")
                End Get
            End Property

            Public ReadOnly Property Company() As String
                Get
                    Return UserProperty("company")
                End Get
            End Property

            Public ReadOnly Property Office() As String
                Get
                    Return UserProperty("physicalDeliveryOfficeName")
                End Get
            End Property

            Public ReadOnly Property Department() As String
                Get
                    Return UserProperty("department")
                End Get
            End Property

            Public ReadOnly Property City() As String
                Get
                    Return UserProperty("l")
                End Get
            End Property

            Public ReadOnly Property Mailbox() As String
                Get
                    Return UserProperty("streetAddress")
                End Get
            End Property

        End Class

        Public Shared ReadOnly Property CurrentUser() As UserInfo
            Get
                If currentUserInfo Is Nothing Then currentUserInfo = New UserInfo(CurrentUserID)
                Return currentUserInfo
            End Get
        End Property

        Public Shared ReadOnly Property CurrentUserID() As String
            Get
                Return WindowsIdentity.GetCurrent.Name
            End Get
        End Property

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

        Public Shared Sub EndImpersonation(ByVal impersonatedUser As WindowsImpersonationContext)

            If Not impersonatedUser Is Nothing Then impersonatedUser.Undo()
            impersonatedUser = Nothing

        End Sub

        Public Shared Sub ConnectToNetworkShare(ByVal sharename As String, ByVal username As String, ByVal password As String, ByVal domain As String)

            Dim resource As NETRESOURCE
            Dim result As Integer

            With resource
                .dwType = RESOURCETYPE_DISK
                .lpRemoteName = sharename
            End With

            If Len(domain) > 0 Then username = domain & "\" & username

            result = WNetAddConnection2(resource, password, username, 0)

            If result <> 0 Then
                Throw New InvalidOperationException("Failed to connect to network share """ & sharename & """ as user " & username & ".  " & GetErrorMessage(result))
            End If

        End Sub

        Public Shared Sub DisconnectFromNetworkShare(ByVal sharename As String, Optional ByVal force As Boolean = True)

            Dim result As Integer = WNetCancelConnection2(sharename, 0, force)

            If result <> 0 Then
                Throw New InvalidOperationException("Failed to disconnect from network share """ & sharename & """.  " & GetErrorMessage(result))
            End If

        End Sub

        ' GetErrorMessage formats and returns an error message orresponding to the input errorCode
        Private Shared Function GetErrorMessage(ByVal errorCode As Integer) As String

            Const FORMAT_MESSAGE_ALLOCATE_BUFFER As Integer = &H100
            Const FORMAT_MESSAGE_IGNORE_INSERTS As Integer = &H200
            Const FORMAT_MESSAGE_FROM_SYSTEM As Integer = &H1000

            Dim messageSize As Integer = 255
            Dim lpMsgBuf As String
            Dim dwFlags As Integer = FORMAT_MESSAGE_ALLOCATE_BUFFER Or FORMAT_MESSAGE_FROM_SYSTEM Or FORMAT_MESSAGE_IGNORE_INSERTS
            Dim ptrlpSource As IntPtr = IntPtr.Zero
            Dim prtArguments As IntPtr = IntPtr.Zero

            If FormatMessage(dwFlags, ptrlpSource, errorCode, 0, lpMsgBuf, messageSize, prtArguments) = 0 Then
                Throw New InvalidOperationException("Failed to format message for error code " & errorCode)
            End If

            Return lpMsgBuf

        End Function

    End Class

End Namespace
