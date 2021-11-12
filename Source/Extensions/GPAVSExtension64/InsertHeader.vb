'******************************************************************************************************
'  InsertHeader.vb - Gbtc
'
'  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
'
'  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
'  the NOTICE file distributed with this work for additional information regarding copyright ownership.
'  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
'  not use this file except in compliance with the License. You may obtain a copy of the License at:
'
'      http://opensource.org/licenses/MIT
'
'  Unless agreed to in writing, the subject software distributed under the License is distributed on an
'  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
'  License for the specific language governing permissions and limitations.
'
'  Code Modification History:
'  ----------------------------------------------------------------------------------------------------
'  10/02/2015 - Stephen C. Wills
'       Generated original version of source code.
'
'******************************************************************************************************

Imports System
Imports System.ComponentModel.Design
Imports System.DirectoryServices
Imports System.Text
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.Win32

''' <summary>
''' Command handler
''' </summary>
Public NotInheritable Class InsertHeader

    ''' <summary>
    ''' Command ID.
    ''' </summary>
    Public Const CommandId As Integer = 256

    ''' <summary>
    ''' Command menu group (command set GUID).
    ''' </summary>
    Public Shared ReadOnly CommandSet As New Guid("e5b1a060-7d63-4673-9524-936d738bf413")

    ''' <summary>
    ''' VS Package that provides this command, not null.
    ''' </summary>
    Private ReadOnly package As package
    
    Private m_userEntry As DirectoryEntry

    ''' <summary>
    ''' Initializes a new instance of the <see cref="InsertHeader"/> class.
    ''' Adds our command handlers for menu (the commands must exist in the command table file)
    ''' </summary>
    ''' <param name="package">Owner package, not null.</param>
    Private Sub New(package As package)
        If package Is Nothing Then
            Throw New ArgumentNullException("package")
        End If

        Me.package = package
        Dim commandService As OleMenuCommandService = Me.ServiceProvider.GetService(GetType(IMenuCommandService))
        If commandService IsNot Nothing Then
            Dim menuCommandId = New CommandID(CommandSet, CommandId)
            Dim menuCommand = New MenuCommand(AddressOf Me.MenuItemCallback, menuCommandId)
            commandService.AddCommand(menuCommand)
        End If
    End Sub

    ''' <summary>
    ''' Gets the instance of the command.
    ''' </summary>
    Public Shared Property Instance As InsertHeader

    ''' <summary>
    ''' Get service provider from the owner package.
    ''' </summary>
    Private ReadOnly Property ServiceProvider As IServiceProvider
        Get
            Return Me.package
        End Get
    End Property

    ''' <summary>
    ''' Initializes the singleton instance of the command.
    ''' </summary>
    ''' <param name="package">Owner package, Not null.</param>
    Public Shared Sub Initialize(package As package)
        Instance = New InsertHeader(package)
    End Sub

    ''' <summary>
    ''' This function is the callback used to execute the command when the menu item is clicked.
    ''' See the constructor to see how the menu item is associated with this function using
    ''' OleMenuCommandService service and MenuCommand class.
    ''' </summary>
    ''' <param name="sender">Event sender.</param>
    ''' <param name="e">Event args.</param>
    Private Sub MenuItemCallback(sender As Object, e As EventArgs)

        InsertHeader()

    End Sub

    Public Sub InsertHeader()

        Dim activeDoc As Document = ApplicationObject.ActiveDocument
        Dim headerText = New StringBuilder()
        Dim commentToken As String

        ' Select proper in-line comment token
        If (activeDoc.Name.EndsWith(".vb")) Then
            commentToken = "'"
        Else
            commentToken = "//"
        End If

        With headerText
            .AppendLine(commentToken & "******************************************************************************************************")
            .AppendLine(commentToken & "  " & ApplicationObject.ActiveWindow.ProjectItem.Name & " - Gbtc")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Copyright © " & DateTime.Now.ToString("yyyy") & ", Grid Protection Alliance.  All Rights Reserved.")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See")
            .AppendLine(commentToken & "  the NOTICE file distributed with this work for additional information regarding copyright ownership.")
            .AppendLine(commentToken & "  The GPA licenses this file to you under the MIT License (MIT), the ""License""; you may not use this")
            .AppendLine(commentToken & "  file except in compliance with the License. You may obtain a copy of the License at:")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "      http://opensource.org/licenses/MIT")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Unless agreed to in writing, the subject software distributed under the License is distributed on an")
            .AppendLine(commentToken & "  ""AS-IS"" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the")
            .AppendLine(commentToken & "  License for the specific language governing permissions and limitations.")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Code Modification History:")
            .AppendLine(commentToken & "  ----------------------------------------------------------------------------------------------------")
            .AppendLine(commentToken & "  " & DateTime.Now.ToString("MM/dd/yyyy") & " - " & FullName)
            .AppendLine(commentToken & "       Generated original version of source code.")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "******************************************************************************************************")
            .AppendLine()
        End With

        With activeDoc.Selection
            .StartOfDocument(False)
            .Insert(headerText.ToString(), vsInsertFlags.vsInsertFlagsCollapseToEnd)
        End With

    End Sub

    Private ReadOnly Property FullName() As String
        Get
            ' If machine name and domain are the same, user is likely not logged into a domain so
            ' there's a good probability that no active directory services will be available...
            If String.Compare(Environment.MachineName.Trim(), Environment.UserDomainName.Trim(), True) = 0 Then
                ' If not running on a domain, we use user name from Visual Studio registration
                Dim registeredName As String = Registry.GetValue("HKEY_USERS\.DEFAULT\Software\Microsoft\VisualStudio\14.0_Config\Registration", "UserName", "")

                If String.IsNullOrEmpty(registeredName) Then
                    Return Environment.UserName
                Else
                    Return registeredName
                End If
            Else
                ' Otherwise we get name defined in ActiveDirectory for logged in user
                If Not String.IsNullOrEmpty(FirstName) AndAlso Not String.IsNullOrEmpty(LastName) Then
                    If String.IsNullOrEmpty(MiddleInitial) Then
                        Return FirstName & " " & LastName
                    Else
                        Return FirstName & " " & MiddleInitial & ". " & LastName
                    End If
                Else
                    Return Environment.UserName
                End If
            End If
        End Get
    End Property

    Private ReadOnly Property UserEntry() As DirectoryEntry
        Get
            If m_userEntry Is Nothing Then
                Try
                    Dim entry As New DirectoryEntry()
                    With New DirectorySearcher(entry)
                        .Filter = "(SAMAccountName=" & Environment.UserName & ")"
                        m_userEntry = .FindOne().GetDirectoryEntry()
                    End With
                Catch
                    m_userEntry = Nothing
                    Throw
                End Try
            End If
            Return m_userEntry
        End Get
    End Property

    Private ReadOnly Property UserProperty(ByVal propertyName As System.String) As String
        Get
            Try
                Return UserEntry.Properties(propertyName)(0).ToString().Replace("  ", " ").Trim()
            Catch
                Return ""
            End Try
        End Get
    End Property

    Private ReadOnly Property FirstName() As String
        Get
            Return UserProperty("givenName")
        End Get
    End Property

    Private ReadOnly Property LastName() As String
        Get
            Return UserProperty("sn")
        End Get
    End Property

    Private ReadOnly Property MiddleInitial() As String
        Get
            Return UserProperty("initials")
        End Get
    End Property

    Private ReadOnly Property Email() As String
        Get
            Return UserProperty("mail")
        End Get
    End Property

    Private ReadOnly Property Telephone() As String
        Get
            Return UserProperty("telephoneNumber")
        End Get
    End Property

    Private ReadOnly Property Title() As String
        Get
            Return UserProperty("title")
        End Get
    End Property

    Private ReadOnly Property Company() As String
        Get
            Return UserProperty("company")
        End Get
    End Property

    Private ReadOnly Property Office() As String
        Get
            Return UserProperty("physicalDeliveryOfficeName")
        End Get
    End Property

    Private ReadOnly Property Department() As String
        Get
            Return UserProperty("department")
        End Get
    End Property

    Private ReadOnly Property City() As String
        Get
            Return UserProperty("l")
        End Get
    End Property

    Private ReadOnly Property Mailbox() As String
        Get
            Return UserProperty("streetAddress")
        End Get
    End Property

    Private ReadOnly Property ApplicationObject() As DTE2
        Get
            Return ServiceProvider.GetService(GetType(DTE))
        End Get
    End Property
End Class
