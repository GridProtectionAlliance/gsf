Imports System
Imports System.DirectoryServices
Imports System.Text
Imports Microsoft.Win32
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private m_applicationObject As DTE2
    Private m_addInInstance As AddIn
    Private m_userEntry As DirectoryEntry

    '''<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    Public Sub New()

    End Sub

    '''<summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    '''<param name='application'>Root object of the host application.</param>
    '''<param name='connectMode'>Describes how the Add-in is being loaded.</param>
    '''<param name='addInInst'>Object representing this Add-in.</param>
    '''<remarks></remarks>
    Public Sub OnConnection(ByVal application As Object, ByVal connectMode As ext_ConnectMode, ByVal addInInst As Object, ByRef custom As Array) Implements IDTExtensibility2.OnConnection
        m_applicationObject = CType(application, DTE2)
        m_addInInstance = CType(addInInst, AddIn)
        If connectMode = ext_ConnectMode.ext_cm_UISetup Then

            Dim commands As Commands2 = CType(m_applicationObject.Commands, Commands2)

            Try
                'Add a command to the Commands collection:
                commands.AddNamedCommand2(m_addInInstance, "InsertHeader", "InsertHeader", "Executes the InsertHeader command for AddInTest", True, 59, Nothing, CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer), vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton)
                commands.AddNamedCommand2(m_addInInstance, "FormatAll", "FormatAll", "Executes the FormatAll command for AddInTest", True, 59, Nothing, CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer), vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton)
                commands.AddNamedCommand2(m_addInInstance, "XmlCodeCommentRegion", "XmlCodeCommentRegion", "Executes the XmlCodeCommentRegion command for AddInTest", True, 59, Nothing, CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer), vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton)
            Catch argumentException As System.ArgumentException
                'If we are here, then the exception is probably because a command with that name
                '  already exists. If so there is no need to recreate the command and we can 
                '  safely ignore the exception.
            End Try

        End If
    End Sub

    '''<summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    '''<param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnDisconnection(ByVal disconnectMode As ext_DisconnectMode, ByRef custom As Array) Implements IDTExtensibility2.OnDisconnection
    End Sub

    '''<summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification that the collection of Add-ins has changed.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnAddInsUpdate(ByRef custom As Array) Implements IDTExtensibility2.OnAddInsUpdate
    End Sub

    '''<summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnStartupComplete(ByRef custom As Array) Implements IDTExtensibility2.OnStartupComplete
    End Sub

    '''<summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnBeginShutdown(ByRef custom As Array) Implements IDTExtensibility2.OnBeginShutdown
    End Sub

    '''<summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    '''<param name='commandName'>The name of the command to determine state for.</param>
    '''<param name='neededText'>Text that is needed for the command.</param>
    '''<param name='status'>The state of the command in the user interface.</param>
    '''<param name='commandText'>Text requested by the neededText parameter.</param>
    '''<remarks></remarks>
    Public Sub QueryStatus(ByVal commandName As String, ByVal neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus
        If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then
            If commandName = "GPACodeHeaderAddIn.Connect.InsertHeader" Or commandName = "GPACodeHeaderAddIn.Connect.FormatAll" Or commandName = "GPACodeHeaderAddIn.Connect.XmlCodeCommentRegion" Then
                status = CType(vsCommandStatus.vsCommandStatusEnabled + vsCommandStatus.vsCommandStatusSupported, vsCommandStatus)
            Else
                status = vsCommandStatus.vsCommandStatusUnsupported
            End If
        End If
    End Sub

    '''<summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    '''<param name='commandName'>The name of the command to execute.</param>
    '''<param name='executeOption'>Describes how the command should be run.</param>
    '''<param name='varIn'>Parameters passed from the caller to the command handler.</param>
    '''<param name='varOut'>Parameters passed from the command handler to the caller.</param>
    '''<param name='handled'>Informs the caller if the command was handled or not.</param>
    '''<remarks></remarks>
    Public Sub Exec(ByVal commandName As String, ByVal executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec
        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
            If commandName = "GPACodeHeaderAddIn.Connect.InsertHeader" Then
                InsertHeader()
                handled = True
                Exit Sub
            ElseIf commandName = "GPACodeHeaderAddIn.Connect.FormatAll" Then
                FormatAll()
                handled = True
                Exit Sub
            ElseIf commandName = "GPACodeHeaderAddIn.Connect.XmlCodeCommentRegion" Then
                XmlCodeCommentRegion()
                handled = True
                Exit Sub
            End If
        End If
    End Sub

    Public Sub InsertHeader()

        Dim activeDoc As Document = m_applicationObject.ActiveDocument
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
            .AppendLine(commentToken & "  " & m_applicationObject.ActiveWindow.ProjectItem.Name & " - Gbtc")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Copyright © " & DateTime.Now.ToString("yyyy") & ", Grid Protection Alliance.  All Rights Reserved.")
            .AppendLine(commentToken & "")
            .AppendLine(commentToken & "  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See")
            .AppendLine(commentToken & "  the NOTICE file distributed with this work for additional information regarding copyright ownership.")
            .AppendLine(commentToken & "  The GPA licenses this file to you under the MIT License (MIT), the ""License""; you may")
            .AppendLine(commentToken & "  not use this file except in compliance with the License. You may obtain a copy of the License at:")
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

    Public Sub FormatAll()

        Dim project As Project
        Dim projectObjects As Object()
        Dim window As Window
        Dim target As Object

        window = m_applicationObject.Windows.Item(Constants.vsWindowKindCommandWindow)
        projectObjects = m_applicationObject.ActiveSolutionProjects

        If projectObjects.Length = 0 Then
            Exit Sub
        End If

        project = m_applicationObject.ActiveSolutionProjects(0)

        If (m_applicationObject.ActiveWindow Is window) Then
            target = window.Object
        Else
            target = GetOutputWindowPane("List Project")
            target.Clear()
        End If

        RecurseProjectFolders(project.ProjectItems(), 0, target)

    End Sub

    Private Sub RecurseProjectFolders(ByVal projectItems As EnvDTE.ProjectItems, ByVal level As Integer, ByVal outputWinPane As Object)

        Dim projectItem As EnvDTE.ProjectItem

        For Each projectItem In projectItems
            ' Ignore item if it is not rooted in this collection (check for VC project model).
            If projectItem.Collection Is projectItems Then
                ' Execute formatting action
                PerformCodeItem(projectItem, level, outputWinPane)
                ' Recurse if this item has subitems ...
                Dim projectItems2 As EnvDTE.ProjectItems = projectItem.ProjectItems
                If projectItems2 IsNot Nothing Then RecurseProjectFolders(projectItems2, level + 1, outputWinPane)
            End If
        Next

    End Sub

    Private Sub PerformCodeItem(ByVal projectItem As EnvDTE.ProjectItem, ByVal level As Integer, ByVal outputWinPane As Object)

        Dim window As EnvDTE.Window
        Dim alreadyOpen As Boolean

        If projectItem.Name.EndsWith(".cs") Then
            alreadyOpen = projectItem.IsOpen(Constants.vsext_vk_Code)
            window = projectItem.Open(Constants.vsext_vk_Code)
            window.Activate()
            m_applicationObject.ExecuteCommand("Edit.FormatDocument")
            If Not alreadyOpen Then window.Close(vsSaveChanges.vsSaveChangesYes)
        End If

    End Sub

    Private Function GetOutputWindowPane(ByVal Name As String, Optional ByVal show As Boolean = True) As OutputWindowPane

        Dim window As Window
        Dim outputWindow As OutputWindow
        Dim outputWindowPane As OutputWindowPane

        window = m_applicationObject.Windows.Item(EnvDTE.Constants.vsWindowKindOutput)
        If show Then window.Visible = True
        outputWindow = window.Object

        Try
            outputWindowPane = outputWindow.OutputWindowPanes.Item(Name)
        Catch e As System.Exception
            outputWindowPane = outputWindow.OutputWindowPanes.Add(Name)
        End Try

        outputWindowPane.Activate()

        Return outputWindowPane

    End Function

    Private ReadOnly Property FullName() As String
        Get
            ' If machine name and domain are the same, user is likely not logged into a domain so
            ' there's a good probablility that no active directory services will be available...
            If String.Compare(Environment.MachineName.Trim(), Environment.UserDomainName.Trim(), True) = 0 Then
                ' If not running on a domain, we use user name from Visual Studio registration
                Dim registeredName As String = Registry.GetValue("HKEY_USERS\.DEFAULT\Software\Microsoft\VisualStudio\11.0_Config\Registration", "UserName", "")

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

    Public Sub XmlCodeCommentRegion()

        Dim selection As EnvDTE.TextSelection
        Dim startPoint As EnvDTE.EditPoint
        Dim endPoint As TextPoint
        Dim commentStart As String

        selection = m_applicationObject.ActiveDocument.Selection()
        startPoint = selection.TopPoint.CreateEditPoint()
        endPoint = selection.BottomPoint
        commentStart = LineOrientedXmlCodeCommentStart()
        m_applicationObject.UndoContext.Open("Xml Code Comment Region")

        Try
            Do While (True)
                Dim line As Integer
                line = startPoint.Line
                startPoint.Insert(commentStart)
                startPoint.LineDown()
                startPoint.StartOfLine()
                If (line = endPoint.Line) Then Exit Do
            Loop
        Finally
            ' If an error occurred, then make sure that the undo context is cleaned up.
            ' Otherwise, the editor can be left in a perpetual undo context.
            m_applicationObject.UndoContext.Close()
        End Try

    End Sub

    Private Function LineOrientedXmlCodeCommentStart(Optional ByVal document As Document = Nothing) As String

        Dim extension As String

        If document Is Nothing Then document = m_applicationObject.ActiveDocument

        extension = document.Name

        If (extension.EndsWith(".cs") Or extension.EndsWith(".cpp") Or extension.EndsWith(".h") Or extension.EndsWith(".idl") Or extension.EndsWith(".jsl")) Then
            Return "/// "
        ElseIf (extension.EndsWith(".vb")) Then
            Return "''' "
        Else
            Throw New Exception("Unrecognized file type. You can add this file type by modifying the function LineOrientedXmlCodeCommentStart to include the extension of this file.")
        End If

    End Function
End Class
