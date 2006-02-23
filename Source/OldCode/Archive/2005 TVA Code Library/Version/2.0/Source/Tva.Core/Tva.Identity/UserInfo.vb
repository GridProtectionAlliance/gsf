'*******************************************************************************************************
'  Tva.Identity.UserInfo.vb - ActiveDirectory User Information Class
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/10/2004 - James R Carroll
'       Original version of source code generated
'  01/03/2006 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Identity)
'
'*******************************************************************************************************

Imports System.DirectoryServices

Namespace Identity

    Public Class UserInfo

        Private m_userDirectoryEntry As DirectoryEntry
        Private m_userLoginID As String

        ''' <summary>
        ''' Initializes a new instance of the user information class
        ''' </summary>
        ''' <remarks>
        ''' Specify login information as domain\username
        ''' </remarks>
        Public Sub New(ByVal loginID As String)

            m_userLoginID = loginID

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the user information class
        ''' </summary>
        Public Sub New(ByVal username As String, ByVal domain As String)

            MyClass.New(domain & "\" & username)

        End Sub

        ''' <summary>
        ''' Gets the login ID of the user.
        ''' </summary>
        Public ReadOnly Property LoginID() As String
            Get
                Return m_userLoginID
            End Get
        End Property

        ''' <summary>
        ''' Gets the System.DirectoryServices.DirectoryEntry of the user
        ''' </summary>
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
        ''' Returns adctive directory value for specified property
        ''' </summary>
        Public ReadOnly Property UserProperty(ByVal propertyName As System.String) As String
            Get
                Try
                    Return UserEntry.Properties(propertyName)(0).ToString().Replace("  ", " ").Trim()
                Catch ex As Exception
                    Return ""
                End Try
            End Get
        End Property

        ''' <summary>
        ''' Gets the full name of the user
        ''' </summary>
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
        ''' Gets the e-mail address of the user
        ''' </summary>
        Public ReadOnly Property Email() As String
            Get
                Return UserProperty("mail")
            End Get
        End Property

        ''' <summary>
        ''' Gets the telephone number of the user
        ''' </summary>
        Public ReadOnly Property Telephone() As String
            Get
                Return UserProperty("telephoneNumber")
            End Get
        End Property

        ''' <summary>
        ''' Gets the title of the user
        ''' </summary>
        Public ReadOnly Property Title() As String
            Get
                Return UserProperty("title")
            End Get
        End Property

        ''' <summary>
        ''' Gets the company of the user
        ''' </summary>
        Public ReadOnly Property Company() As String
            Get
                Return UserProperty("company")
            End Get
        End Property

        ''' <summary>
        ''' Returns the office location of the user
        ''' </summary>
        Public ReadOnly Property Office() As String
            Get
                Return UserProperty("physicalDeliveryOfficeName")
            End Get
        End Property

        ''' <summary>
        ''' Gets the department name where the user works
        ''' </summary>
        Public ReadOnly Property Department() As String
            Get
                Return UserProperty("department")
            End Get
        End Property

        ''' <summary>
        ''' Gets the city where the user works
        ''' </summary>
        Public ReadOnly Property City() As String
            Get
                Return UserProperty("l")
            End Get
        End Property

        ''' <summary>
        ''' Returns the mailbox of where the user works
        ''' </summary>
        Public ReadOnly Property Mailbox() As String
            Get
                Return UserProperty("streetAddress")
            End Get
        End Property

    End Class

End Namespace
