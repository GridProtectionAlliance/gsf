Imports System.DirectoryServices
Imports System.Security.Principal
Imports System.Reflection.Assembly

' This class is exposed as a COM object so that it can be used in the template installation scripts...
Public Class WizTools

    Private currentUserEntry As DirectoryEntry

    Private ReadOnly Property CurrentUser() As DirectoryEntry
        Get
            If currentUserEntry Is Nothing Then
                Try
                    Dim loginID As String = CurrentUserID
                    Dim domain As String = loginID.Substring(0, loginID.IndexOf("\"c)).Trim()
                    Dim userName As String = loginID.Replace(domain & "\", "").Trim()
                    Dim entry As New DirectoryEntry("LDAP://" & domain)
                    With New DirectorySearcher(entry)
                        .Filter = "(SAMAccountName=" & userName & ")"
                        currentUserEntry = .FindOne().GetDirectoryEntry()
                    End With
                Catch
                    currentUserEntry = Nothing
                End Try
            End If

            Return currentUserEntry
        End Get
    End Property

    Public ReadOnly Property CurrentUserID() As String
        Get
            Return WindowsIdentity.GetCurrent.Name
        End Get
    End Property

    Public ReadOnly Property CurrentUserFullName() As String
        Get
            Dim fullName As String = CurrentUserProperty("displayName")

            If Len(fullName) > 0 Then
                Dim commaPos As Integer = fullName.IndexOf(","c)

                If commaPos > -1 Then
                    Return fullName.Substring(commaPos + 1).Trim() & " " & fullName.Substring(0, commaPos).Trim()
                Else
                    Return fullName
                End If
            Else
                Return CurrentUserID
            End If
        End Get
    End Property

    Public ReadOnly Property CurrentUserProperty(ByVal PropertyName As String) As String
        Get
            Try
                Return CurrentUser.Properties(PropertyName)(0).ToString().Replace("  ", " ").Trim()
            Catch ex As Exception
                Return ""
            End Try
        End Get
    End Property

    Public Sub UpdateACL(ByVal Path As String, ByVal UserID As String, ByVal Rights As String)

        Shell("cacls """ & Path & """ /T /E /C /P " & UserID & ":" & Rights, AppWinStyle.Hide, True, 5000)

    End Sub

    Public Function Location() As String

        Return GetExecutingAssembly.Location

    End Function

    Public Function Version() As String

        Return GetExecutingAssembly.FullName

    End Function

End Class
